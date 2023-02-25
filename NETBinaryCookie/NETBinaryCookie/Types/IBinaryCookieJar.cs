using System.Collections.Immutable;

namespace NETBinaryCookie.Types;

// NOTE: CUD functions for the jar need to go in the concrete facade because
//   the jar's cookie list is not directly modifiable (intentionally).
public interface IBinaryCookieJar
{
    public ImmutableArray<BinaryCookie> GetCookies();

    public BinaryCookie? GetCookie((string Name, string Domain, string Path) cookieProps);

    public BinaryCookie? GetCookie(BinaryCookie cookie);

    public IEnumerable<BinaryCookie?> GetCookies(Func<BinaryCookie, bool> comparator);

    public BinaryCookie? AddCookie(BinaryCookie cookie, bool throwOnInvalidCookie = true);

    public ImmutableArray<BinaryCookie>? RemoveCookies(Func<BinaryCookie, bool> comparator);

    public void Save(string? fileName = null);

    public void Save(Stream stream);

    public byte[] Stub { get; set; }
}

public static class BinaryCookieJarExtensions
{
    public static BinaryCookie? RemoveCookie(this IBinaryCookieJar jar, BinaryCookie inputCookie) =>
        jar.RemoveCookies(cookie => cookie == inputCookie)?[0] ?? null;
    
    public static ImmutableArray<BinaryCookie>? RemoveCookiesByDomain(this IBinaryCookieJar jar, string domain) =>
        jar.RemoveCookies(cookie => cookie.Domain == domain);

    public static ImmutableArray<BinaryCookie>? RemoveCookiesByName(this IBinaryCookieJar jar, string name)
        => jar.RemoveCookies(cookie => cookie.Name == name);

    public static ImmutableArray<BinaryCookie>? RemoveCookiesByExpiration(this IBinaryCookieJar jar, DateTime expiration,
        bool removeCookiesOlderThanDate = false)
    {
        Func<BinaryCookie, bool> comparator = removeCookiesOlderThanDate
            ? cookie => cookie.Expiration < expiration
            : cookie => cookie.Expiration >= expiration;

        return jar.RemoveCookies(comparator);
    }

    public static BinaryCookie? RemoveCookie(this IBinaryCookieJar jar,
        (string Name, string Domain, string Path) cookieProps) =>
        jar.RemoveCookies(
            cookie => cookie.Name == cookieProps.Name && cookie.Domain == cookieProps.Domain &&
                      cookie.Path == cookieProps.Path)?[0] ?? null;
}