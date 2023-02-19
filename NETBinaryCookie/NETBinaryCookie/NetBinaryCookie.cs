using NETBinaryCookie.Types;

namespace NETBinaryCookie;

public static class NetBinaryCookie
{
    public enum CookieFlag
    {
        Secure = 1 << 0,
        ReservedOne = 1 << 1,
        HttpOnly = 1 << 2,
        SamesiteLax = 1 << 3,
        SamesiteStrict = 1 << 4,
        DomainSpecific = 1 << 5,
        SamesiteNone = 1 << 6,
        ReservedTwo = 1 << 7
    }
    
    public static BinaryCookieJar ReadFromFile(string fileName) => new(fileName);

    public static BinaryCookieJar ReadFromStream(Stream stream) => new(stream);
}