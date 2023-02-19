﻿using System.Runtime.InteropServices;

namespace NETBinaryCookie.Types.Meta;


internal sealed class BinaryCookieJarMeta
{
    public FileMeta JarDetails { get; set; }

    public List<BinaryCookiePageMeta> JarPages { get; } = new();

    public int Checksum { get; set; }

    public byte[] TrailingData { get; set; } = Array.Empty<byte>();
    
    // signature + numPages [N] + (N * pageOffsets) + (N * pages) + checksum + 8-byte footer + len(trailingData)
    public int CalculatedSize => sizeof(int) + sizeof(int) + (int)(this.JarDetails.numPages * sizeof(int)) +
                                 this.JarPages.Aggregate(0, (i, page) => i += page.CalculatedSize) + sizeof(int) +
                                 sizeof(long) + TrailingData.Length;
}

internal sealed class BinaryCookiePageMeta
{
    public uint StartPosition { get; set; }
    
    public uint Size { get; init; }
    
    public int Checksum { get; set; }
    
    public PageMeta PageProperties { get; set; }

    public List<BinaryCookieMetaMeta> PageCookies { get; } = new();

    // pageStart + numCookies [N] + (N * sizeof(int) -> offsets) + pageEnd + (N * cookies.sizes)
    public int CalculatedSize =>
        Marshal.SizeOf<PageMeta>() + (int)(this.PageProperties.numCookies * sizeof(int)) + sizeof(int) +
        this.PageCookies.Aggregate(0, (i, cookie) => i += cookie.CalculatedSize);
}

internal sealed class BinaryCookieMetaMeta
{
    public uint StartPosition { get; set; }
    
    public uint Size { get; set; }
    
    public BinaryCookieMeta CookieProperties { get; set; }

    public BinaryCookie Cookie { get; set; } = default!;

    private static int _strlens(params string?[] str) =>
        str.Aggregate(0, (acc, s) => acc += s?.Length ?? 0);

    // sizeof(cookieMetaProperties) + expirationDate (long) + creationDate (long) + len(stringValues)
    public int CalculatedSize => Marshal.SizeOf<BinaryCookieMeta>() + sizeof(long) + sizeof(long) +
                                 _strlens(this.Cookie.Comment, this.Cookie.Domain, this.Cookie.Name, this.Cookie.Path,
                                     this.Cookie.Value);
}