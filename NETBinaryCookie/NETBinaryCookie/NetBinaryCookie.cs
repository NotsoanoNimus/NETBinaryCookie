using NETBinaryCookie.Types;

namespace NETBinaryCookie;

public static class NetBinaryCookie
{
    public static BinaryCookieJar ReadFromFile(string fileName) => new(fileName);
}