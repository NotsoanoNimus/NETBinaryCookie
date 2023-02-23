using System.Collections.Immutable;

namespace NETBinaryCookie.Types;

public sealed class BinaryCookieJar : IBinaryCookieJar
{
    private List<BinaryCookie> Cookies { get; } = new();

    private string? TargetFileName { get; }

    // The "META" property exists to track the original imported state of the BinaryCookie file.
    //   When the modified cookie jar is saved, this library generates its own paging and checksum.
    //   This can also be null on a new jar that isn't based on an incoming file/stream.
    private BinaryCookieJarMeta? Meta { get; }

    public BinaryCookieJar(Stream stream)
    {
        this.Meta = BinaryCookieMetaParser.Import(stream);
        
        this.RefreshCookiesFromMeta();
    }

    public BinaryCookieJar(string fileName)
    {
        this.Meta = BinaryCookieMetaParser.Import(fileName);
        
        this.RefreshCookiesFromMeta();

        this.TargetFileName = fileName;
    }

    public BinaryCookieJar()
    {
    }

    private void RefreshCookiesFromMeta()
    {
        this.Cookies.Clear();
        
        var pageCookies = this.Meta!.JarPages.SelectMany(page => page.PageCookies).ToList();
        
        var cookies = pageCookies.Select(cookie => cookie.Cookie).ToList();
        
        this.Cookies.AddRange(cookies);
    }
    
    public ImmutableArray<BinaryCookie> GetCookies() => this.Cookies.ToImmutableArray();

    public BinaryCookie? GetCookie((string Name, string Domain, string Path) cookieProps) => this.Cookies
        .FirstOrDefault(cookie => cookie.Name == cookieProps.Name && cookie.Domain == cookieProps.Domain &&
                                  cookie.Path == cookieProps.Path);

    public BinaryCookie? GetCookie(BinaryCookie cookie) => this.Cookies.FirstOrDefault(c => c == cookie);

    public IEnumerable<BinaryCookie?> GetCookies(Func<BinaryCookie, bool> comparator) =>
        this.Cookies.Where(comparator);

    public BinaryCookie? AddCookie(BinaryCookie cookie, bool throwOnInvalidCookie = true)
    {
        if (this.GetCookie(cookie) is not null)
        {
            return null;
        }

        if (cookie.CalculatedSize > BinaryCookieMetaConstants.MaxCookieLength)
        {
            if (throwOnInvalidCookie)
            {
                throw new BinaryCookieException(
                    "The provided cookie to add exceeds the maximum cookie "
                    + $"size of {BinaryCookieMetaConstants.MaxCookieLength} bytes");
            }

            return null;
        }

        this.Cookies.Add(cookie);

        return cookie;
    }
    
    public ImmutableArray<BinaryCookie>? RemoveCookies(Func<BinaryCookie, bool> comparator)
    {
        var toRemove = this.Cookies.Where(comparator).ToList();

        if (toRemove.Count < 1)
        {
            return null;
        }

        this.Cookies.RemoveAll(new(comparator));

        return toRemove.ToImmutableArray();
    }

    public void Save(string? fileName = null) =>
        this.Export(
            fileName ?? this.TargetFileName ??
            throw new BinaryCookieException("An explicit filename is required when saving a jar from a file"));

    public void Save(Stream stream) => this.Export(stream);
}
