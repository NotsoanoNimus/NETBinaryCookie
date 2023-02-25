using System.Runtime.InteropServices;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie.Types;


internal sealed class BinaryCookieJarMeta
{
    public JarStructuredProperties JarDetails { get; set; }

    public List<BinaryCookiePageMeta> JarPages { get; } = new();

    public int Checksum { get; set; }

    public byte[] TrailingData { get; set; } = Array.Empty<byte>();
    
    // signature + numPages [N] + (N * pageOffsets) + (N * pages) + checksum + 8-byte footer + len(trailingData)
    public int CalculatedSize => sizeof(int) + sizeof(int) + (int)(this.JarDetails.numPages * sizeof(int)) +
                                 this.JarPages.Aggregate(0, (i, page) => i += page.CalculatedSize) + sizeof(int) +
                                 sizeof(long) + TrailingData.Length;

    public int CalculatedChecksum => this.JarPages.Select(x => x.Checksum).Aggregate(0, (i, j) => i + j);
}

internal sealed class BinaryCookiePageMeta
{
    public uint StartPosition { get; set; }
    
    public uint Size { get; set; }
    
    public int Checksum { get; set; }
    
    public PageStructuredProperties PageProperties { get; set; }

    public List<BinaryCookieMeta> PageCookies { get; } = new();

    // pageStart + numCookies [N] + (N * sizeof(int) -> offsets) + pageEnd + (N * cookies.sizes)
    public int CalculatedSize =>
        Marshal.SizeOf<PageStructuredProperties>() + (int)(this.PageProperties.numCookies * sizeof(int)) + sizeof(int) +
        this.PageCookies.Aggregate(0, (i, cookieMeta) => i + cookieMeta.CalculatedSize);
}

internal sealed class BinaryCookieMeta
{
    public uint StartPosition { get; set; }
    
    public uint OffsetFromPageStart { get; set; }
    
    public BinaryCookieStructuredProperties CookieProperties { get; set; }

    public BinaryCookie Cookie { get; set; } = default!;

    public int CalculatedSize => Marshal.SizeOf<BinaryCookieStructuredProperties>() + this.Cookie.CalculatedSize;
}