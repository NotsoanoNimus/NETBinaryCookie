namespace NETBinaryCookie.Types.Meta;


// TODO: Make sure all these go internal once the application is initially tested.
public sealed class BinaryCookieJarMeta
{
    public FileMeta JarDetails { get; set; }

    public List<BinaryCookiePageMeta> JarPages { get; set; } = new();

    public byte[] Checksum { get; set; } = new byte[8];

    public byte[] TrailingData { get; set; } = Array.Empty<byte>();
}

public sealed class BinaryCookiePageMeta
{
    public uint StartPosition { get; set; }
    
    public uint Size { get; set; }
    
    public PageMeta PageProperties { get; set; }

    public List<BinaryCookieMetaMeta> PageCookies { get; set; } = new();
}

public sealed class BinaryCookieMetaMeta
{
    public uint StartPosition { get; set; }
    
    public uint Size { get; set; }
    
    public BinaryCookieMeta CookieProperties { get; set; }

    public BinaryCookie Cookie { get; set; } = default!;
}