using System.Collections.Immutable;

namespace NETBinaryCookie.Types;

// NOTE: CUD functions for the jar need to go in the concrete facade because
//   the jar's cookie list is not directly modifiable (intentionally).
public interface IBinaryCookieJar
{
    public ImmutableArray<BinaryCookie> GetCookies();

    public BinaryCookie? GetCookie((string Name, string Domain, string Path) cookieProps);

    public BinaryCookie? GetCookie(BinaryCookie cookie);

    public BinaryCookie? AddCookie(DateTime expiration, string domain, string name,
        string path, string value, string? comment, DateTime? creation);

    public BinaryCookie? AddCookie(BinaryCookie cookie);

    public ImmutableArray<BinaryCookie>? RemoveCookiesByComparator(Func<BinaryCookie, bool> comparator);

    public void Save(string? fileName = null);
}

public static class BinaryCookieJarExtensions
{
    public static BinaryCookie? RemoveCookie(this IBinaryCookieJar jar, BinaryCookie inputCookie) =>
        jar.RemoveCookiesByComparator(cookie => cookie == inputCookie)?[0] ?? null;
    
    public static ImmutableArray<BinaryCookie>? RemoveCookiesByDomain(this IBinaryCookieJar jar, string domain) =>
        jar.RemoveCookiesByComparator(cookie => cookie.Domain == domain);

    public static ImmutableArray<BinaryCookie>? RemoveCookiesByName(this IBinaryCookieJar jar, string name)
        => jar.RemoveCookiesByComparator(cookie => cookie.Name == name);

    public static ImmutableArray<BinaryCookie>? RemoveCookiesByExpiration(this IBinaryCookieJar jar, DateTime expiration,
        bool removeCookiesOlderThanDate = false)
    {
        Func<BinaryCookie, bool> comparator = removeCookiesOlderThanDate
            ? cookie => cookie.Expiration < expiration
            : cookie => cookie.Expiration >= expiration;

        return jar.RemoveCookiesByComparator(comparator);
    }

    public static BinaryCookie? RemoveCookie(this IBinaryCookieJar jar,
        (string Name, string Domain, string Path) cookieProps) =>
        jar.RemoveCookiesByComparator(
            cookie => cookie.Name == cookieProps.Name && cookie.Domain == cookieProps.Domain &&
                      cookie.Path == cookieProps.Path)?[0] ?? null;
}