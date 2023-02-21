using System.Runtime.InteropServices;
using System.Text;
using NETBinaryCookie.Types;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie;

internal static class BinaryCookieParser
{
    private const string FileSignature = "cook";
    private const uint FileSignatureHex = 0x_63_6F_6F_6B;

    private const uint PageMetaStartMarker = 0x_00_00_01_00;
    private const uint PageMetaEndMarker = 0x_00_00_00_00;

    private const uint CookieMetaEndMarker = 0x_00_00_00_00;

    private const ulong FileFooterSignature = 0x_07_17_20_05_00_00_00_4b;

    internal static BinaryCookieJarMeta Import(string fileName)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("The binarycookie file does not exist");
        }
        
        using var stream = File.Open(fileName, FileMode.Open);
        return Import(stream);
    }
    
    internal static BinaryCookieJarMeta Import(Stream stream)
    {
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        var meta = new BinaryCookieJarMeta
        {
            JarDetails = BinaryCookieTranscoder.BytesToStruct<JarStructuredProperties>(reader.ReadBytes(Marshal.SizeOf<JarStructuredProperties>()))
        };

        // Start by checking the signature and getting the number of pages.
        if (meta.JarDetails.signature != FileSignatureHex)
        {
            throw new BinaryCookieException("Invalid binarycookie signature");
        }
        
        // Get each page's size and create the associated PageStructuredProperties object to use later.
        foreach (var _ in Enumerable.Range(0, (int)meta.JarDetails.numPages))
        {
            meta.JarPages.Add(new() { Size = reader.ReadBinaryBigEndianUInt32() });
        }

        foreach (var pageMeta in meta.JarPages)
        {
            // The first page header (and beyond) are always marked in this position as the start location.
            //   This is verified below by checking the page header signature indicates a page-start.
            pageMeta.StartPosition = (uint)stream.Position;

            pageMeta.PageProperties =
                BinaryCookieTranscoder.BytesToStruct<PageStructuredProperties>(reader.ReadBytes(Marshal.SizeOf<PageStructuredProperties>()));
    
            // Check the page header signature.
            if (pageMeta.PageProperties.pageStart != PageMetaStartMarker)
            {
                throw new BinaryCookieException("Invalid page header signature");
            }
            
            // Parse the cookie offsets.
            foreach (var _ in Enumerable.Range(0, (int)pageMeta.PageProperties.numCookies))
            {
                pageMeta.PageCookies.Add(new() { Size = reader.ReadUInt32() });
            }

            // At this point, the reader cursor should be at the pageEnd marker, after which the cookies begin.
            if (reader.ReadUInt32() != PageMetaEndMarker)
            {
                throw new BinaryCookieException("Missing or malformed page end marker");
            }
            
            // -----
            // GET THE COOKIES IN THIS PAGE.
            foreach (var pageCookie in pageMeta.PageCookies)
            {
                // Mark the cookie starting position.
                pageCookie.StartPosition = (uint)stream.Position;

                pageCookie.CookieProperties =
                    BinaryCookieTranscoder.BytesToStruct<BinaryCookieStructuredProperties>(
                        reader.ReadBytes(Marshal.SizeOf<BinaryCookieStructuredProperties>()));

                if (pageCookie.CookieProperties.endHeader != CookieMetaEndMarker)
                {
                    throw new BinaryCookieException("Missing or malformed cookie properties");
                }

                Func<uint, NetBinaryCookie.CookieFlag[]> GetCookieFlags = flagsRaw => Enum
                    .GetValues(typeof(NetBinaryCookie.CookieFlag)).Cast<NetBinaryCookie.CookieFlag>()
                    .Where(flag => (flagsRaw & (uint)flag) > 0).ToArray();
                
                // Set all cookie properties here as they're read.
                pageCookie.Cookie = new()
                {
                    Expiration = reader.ReadBinaryNsDateAsDateTime(),
                    Creation = reader.ReadBinaryNsDateAsDateTime(),
                    Comment = pageCookie.CookieProperties.commentOffset > 0 ? reader.ReadBinaryStringToEnd() : null,
                    Domain = reader.ReadBinaryStringToEnd()!,
                    Name = reader.ReadBinaryStringToEnd()!,
                    Path = reader.ReadBinaryStringToEnd()!,
                    Value = reader.ReadBinaryStringToEnd()!,
                    Flags = GetCookieFlags(pageCookie.CookieProperties.cookieFlags)
                };

                // Go to the end of the cookie in preparation to get the next one.
                stream.Seek(pageCookie.StartPosition + pageCookie.CookieProperties.cookieSize, SeekOrigin.Begin);
            }
            // -----
            
            // Briefly reposition the stream at the start of the page and calculate a checksum of the page to this spot.
            pageMeta.Checksum = reader.GetInt32Checksum((int)pageMeta.StartPosition);

            // The next page should be located at the start of this page plus its size/offset/length.
            stream.Seek(pageMeta.StartPosition + pageMeta.Size, SeekOrigin.Begin);
        }

        // Get the current checksum value. Not that it's being used, but it moves the cursor.
        meta.Checksum = reader.ReadBinaryBigEndianInt32();

        // Manually calculate a checksum. This isn't really checked here, but it's useful in unit testing.
        meta.CalculatedChecksum = meta.JarPages.Select(x => x.Checksum).Aggregate(0, (i, j) => i + j);

        // Verify the cookie footer signature.
        if (reader.ReadBinaryBigEndianUInt64() != FileFooterSignature)
        {
            throw new BinaryCookieException("File footer signature mismatch");
        }

        // Finally, get the trailing binary data stub that sometimes follows the checksum.
        var trailingData = new List<byte>();
        try
        {
            while (true)
            {
                trailingData.Add(reader.ReadByte());
            }
        }
        catch
        {
            meta.TrailingData = trailingData.ToArray();
        }
        
        return meta;
    }
}