using System.Collections.Immutable;
using System.Text;
using NETBinaryCookie.Types;

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
        
        var cookieSegments = new List<BinaryCookie[]>();
        
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
            
            cookieSegments.Add(cookies.Skip(j).Take(i - j).ToArray());
            
            j = i;
            currentPageSize = 0;
        }
        
        // --------
        // --------
        // Begin stream writing.
        var writer = new BinaryWriter(stream, Encoding.UTF8, false);
    }
}