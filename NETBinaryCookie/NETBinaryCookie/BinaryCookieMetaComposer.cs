using System.Collections.Immutable;
using System.Runtime.InteropServices;
using System.Text;
using NETBinaryCookie.Types;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie;

internal static class BinaryCookieMetaComposer
{
    // Loosely prefer page max sizes around 1,024 bytes, but each page will hold at LEAST 1 cookie.
    private const int PreferredPageMaxSize = 0x_04_00;
    
    // The strategy for composing the file is not going to win any awards. Just create the lowest units (cookies & meta)
    //   first, then turn the page each time the size is creeping uncomfortably. It builds from the inside-out.
    //   After constructing the meta object, the writer can do its work. Personally, I prefer to keep these separated.
    internal static void Compose(ImmutableArray<BinaryCookie> cookies, Stream stream)
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
            
            var spaceForPageMetaAndCookieOffsets =
                Marshal.SizeOf<PageStructuredProperties>() + (pageCookies.Length * sizeof(int));
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
        // Begin stream writing.
        var writer = new BinaryWriter(stream, Encoding.UTF8, false);
    }
}