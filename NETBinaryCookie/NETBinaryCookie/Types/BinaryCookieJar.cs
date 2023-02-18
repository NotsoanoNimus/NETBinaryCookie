using System.Collections.Immutable;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie.Types;

public sealed class BinaryCookieJar : IBinaryCookieJar
{
    private List<BinaryCookie> Cookies { get; } = new();
    
    public string TargetFile { get; }

    public BinaryCookieJarMeta Meta { get; }

    public BinaryCookieJar(string fileName)
    {
        this.TargetFile = fileName;

        // Note: "META" exists to track the original imported state of the BinaryCookie file.
        //   When the modified cookie jar is saved, this library generates its own paging and checksum.
        this.Meta = BinaryCookieParser.ImportFromFile(fileName);

        var pageCookies = this.Meta.JarPages.SelectMany(page => page.PageCookies).ToList();
        
        var cookies = pageCookies.Select(cookie => cookie.Cookie).ToList();
        
        this.Cookies.AddRange(cookies);
    }
    
    public ImmutableArray<BinaryCookie> GetCookies() => this.Cookies.ToImmutableArray();

    public BinaryCookie? GetCookie((string Name, string Domain, string Path) cookieProps) => this.Cookies
        .FirstOrDefault(cookie => cookie.Name == cookieProps.Name && cookie.Domain == cookieProps.Domain &&
                                  cookie.Path == cookieProps.Path);

    public BinaryCookie? GetCookie(BinaryCookie cookie) => this.Cookies.FirstOrDefault(c => c == cookie);

    public BinaryCookie? AddCookie(DateTime expiration, string domain, string name,
        string path, string value, string? comment, DateTime? creation)
    {
        if (this.GetCookie((name, domain, path)) is not null)
        {
            return null;
        }
        
        var newCookie = new BinaryCookie
        {
            Expiration = expiration,
            Domain = domain,
            Name = name,
            Path = path,
            Value = value,
            Comment = comment ?? null,
            Creation = creation ?? DateTime.Now,
        };
        
        this.Cookies.Add(newCookie);

        return newCookie;
    }

    public BinaryCookie? AddCookie(BinaryCookie cookie)
    {
        if (this.GetCookie(cookie) is null)
        {
            this.Cookies.Add(cookie);

            return cookie;
        }

        return null;
    }
    
    public ImmutableArray<BinaryCookie>? RemoveCookiesByComparator(Func<BinaryCookie, bool> comparator)
    {
        var toRemove = this.Cookies.Where(comparator).ToList();

        if (toRemove.Count < 1)
        {
            return null;
        }

        this.Cookies.RemoveAll(new(comparator));

        return toRemove.ToImmutableArray();
    }

    public void Save(string? fileName = null) => BinaryCookieParser.ExportToFile(this.Meta, fileName ?? this.TargetFile);
}
