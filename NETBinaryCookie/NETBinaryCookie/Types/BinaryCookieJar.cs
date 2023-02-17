using System.Collections.Immutable;

namespace NETBinaryCookie.Types;

public sealed class BinaryCookieJar : IBinaryCookieJar
{
    private List<BinaryCookie> Cookies { get; } = new();
    
    public string TargetFile { get; }

    public BinaryCookieJar(string fileName)
    {
        this.TargetFile = fileName;
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
        
        var newCookie = new BinaryCookie()
        {
            Expiration = expiration.ToOADate(),
            Domain = domain,
            Name = name,
            Path = path,
            Value = value,
            Comment = comment ?? null,
            Creation = creation?.ToOADate() ?? DateTime.Now.ToOADate(),
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
}