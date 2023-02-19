namespace NETBinaryCookie.Types.Meta;


internal sealed class BinaryCookieJarMeta
{
    public FileMeta JarDetails { get; set; }

    public List<BinaryCookiePageMeta> JarPages { get; } = new();

    public int Checksum { get; set; }

    public byte[] TrailingData { get; set; } = Array.Empty<byte>();
}

internal sealed class BinaryCookiePageMeta
{
    public uint StartPosition { get; set; }
    
    public uint Size { get; init; }
    
    public int Checksum { get; set; }
    
    public PageMeta PageProperties { get; set; }

    public List<BinaryCookieMetaMeta> PageCookies { get; } = new();
}

internal sealed class BinaryCookieMetaMeta
{
    public uint StartPosition { get; set; }
    
    public uint Size { get; set; }
    
    public BinaryCookieMeta CookieProperties { get; set; }

    public BinaryCookie Cookie { get; set; } = default!;
}