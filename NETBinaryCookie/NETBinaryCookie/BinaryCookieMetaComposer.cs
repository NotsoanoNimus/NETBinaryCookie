using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using NETBinaryCookie.Types;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie;

internal static class BinaryCookieMetaComposer
{
    // Loosely prefer page max sizes around 1,024 bytes, but each page will hold at LEAST 1 cookie.
    private const int PreferredPageMaxSize = 0x_02_00;
    
    // The strategy for composing the file is not going to win any awards. Just create the lowest units (cookies & meta)
    //   first, then turn the page each time the size is creeping uncomfortably. It builds from the inside-out.
    //   After constructing the meta object, the writer can do its work. Personally, I prefer to keep these separated.
    internal static void Compose(ImmutableArray<BinaryCookie> cookies, Stream stream, byte[] stub)
    {
        var meta = new BinaryCookieJarMeta();
        
        for (int currentPageSize = 0, j = 0, i = 1; i <= cookies.Length; i++)
        {
            if (currentPageSize + cookies[i - 1].CalculatedSize < PreferredPageMaxSize)
            {
                currentPageSize += cookies[i - 1].CalculatedSize;
                
                // never let the loop 'continue' on the last cookie element so it isn't lost
                if (i != cookies.Length)
                {
                    continue;
                }
            }
            
            
            // Create a new page with this section of cookies.
            var pageCookies = cookies.Skip(j).Take(i - j).ToArray();
            
            var page = new BinaryCookiePageMeta
            {
                PageProperties = new PageStructuredProperties((uint)pageCookies.Length)
            };
            
            // pageStart, numCookies [C], (C * sizeof(int)), pageFooter
            var spaceForPageMetaAndCookieOffsets =
                Marshal.SizeOf<PageStructuredProperties>() + (pageCookies.Length * sizeof(int)) + sizeof(int);
            var rollingPageOffset = spaceForPageMetaAndCookieOffsets;

            foreach (var cookie in pageCookies)
            {
                var cookieMeta = new BinaryCookieMeta
                {
                    Cookie = cookie,
                    OffsetFromPageStart = (uint)rollingPageOffset
                };

                // In order for these offsets to work correctly, the binary writer MUST encode in the SAME ORDER:
                //   Domain, Name, Path, Value, Comment
                // REMINDER: The ctor here helps w/ the offset calculations too.
                // We don't use comment length here because it's always the LAST property, so its offset doesn't factor.
                cookieMeta.CookieProperties = new BinaryCookieStructuredProperties(
                    cookieMeta.CalculatedSize,
                    cookie.Flags.Aggregate(0, (agg, flag) => agg | (int)flag),
                    cookie.Domain.Length,
                    cookie.Name.Length,
                    cookie.Path.Length,
                    cookie.Value.Length,
                    cookie.Comment?.Length ?? 0);
                
                page.PageCookies.Add(cookieMeta);
                rollingPageOffset += cookieMeta.CalculatedSize;
            }
            
            // Don't need to do anything with offsets because the CalculatedSize property handles that later.
            meta.JarPages.Add(page);
            
            j = i;
            currentPageSize = 0;
        }

        meta.JarDetails = new JarStructuredProperties((uint)meta.JarPages.Count);
        
        // --------
        // --------
        // Begin stream writing. NOTE: Do not seek here; this allows the user to give a stream at a specific position.
        //   This section uses named sections from the README to display which field is being written.
        var writer = new BinaryWriter(stream, Encoding.UTF8, false);

        // signature, numPages
        writer.Write(BinaryCookieTranscoder.StructToBytes(meta.JarDetails));
        
        // pageSizes
        foreach (var page in meta.JarPages)
        {
            writer.Write(BitConverter.GetBytes(page.CalculatedSize).Reverse().ToArray());
        }

        foreach (var page in meta.JarPages)
        {
            var startOfPagePosition = writer.BaseStream.Position;
            
            // pageHeader, numCookies
            writer.Write(BinaryCookieTranscoder.StructToBytes(page.PageProperties));

            foreach (var cookie in page.PageCookies)
            {
                // cookieOffsets
                writer.Write(cookie.OffsetFromPageStart);
            }
            
            // pageFooter
            writer.Write(BinaryCookieMetaConstants.PageMetaEndMarker);

            foreach (var cookieMeta in page.PageCookies)
            {
                // cookieSize, unknownOne, cookieFlags, unknownTwo, domainOffset, nameOffset, pathOffset,
                //   valueOffset, commentOffset, separator
                writer.Write(BinaryCookieTranscoder.StructToBytes(cookieMeta.CookieProperties));

                var c = cookieMeta.Cookie;
                
                // expires
                writer.WriteDateTimeAsBinaryNsDate(c.Expiration);
                
                // creation
                writer.WriteDateTimeAsBinaryNsDate(c.Creation);

                // [field]
                foreach (var field in new[] { c.Domain, c.Name, c.Path, c.Value, c.Comment })
                {
                    if (field is null)
                    {
                        continue;
                    }
                    
                    writer.Write(Encoding.UTF8.GetBytes(field));
                    writer.Write((byte)0x00);
                }
            }
            
            // Rewind the stream temporarily and record the page checksum.
            var endOfPagePosition = writer.BaseStream.Position;

            var rdr = new BinaryReader(stream);
            page.Checksum = rdr.GetInt32Checksum((int)startOfPagePosition);

            writer.BaseStream.Seek(endOfPagePosition, SeekOrigin.Begin);
        }
        
        // checksum
        writer.Write(BitConverter.GetBytes(meta.CalculatedChecksum).Reverse().ToArray());
        
        // fileFooter (big-endian)
        writer.Write(BitConverter.GetBytes(BinaryCookieMetaConstants.FileFooterSignature).Reverse().ToArray());
        
        // stub
        if (stub.Length > 0)
        {
            writer.Write(stub);
        }
    }
}