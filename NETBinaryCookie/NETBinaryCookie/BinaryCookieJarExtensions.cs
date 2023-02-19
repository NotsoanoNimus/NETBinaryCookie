using System.Text;
using NETBinaryCookie.Types;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie;

public static class BinaryCookieJarExtensions
{
    internal static void Export(this BinaryCookieJar jar, string fileName)
        => jar.Export(File.Open(fileName, FileMode.Create));

    internal static void Export(this BinaryCookieJar jar, Stream stream)
    {
        var writer = new BinaryWriter(stream, Encoding.UTF8, false);
        var numCookies = jar.GetCookies().Length;
    }
}