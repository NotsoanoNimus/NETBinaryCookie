using System.Text;
using System.Text.Json;
using NETBinaryCookie.Types;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie;

public static class BinaryCookieJarExtensions
{
    public static string CookiesToJson(this BinaryCookieJar jar) => JsonSerializer.Serialize(jar.GetCookies());

    internal static void Export(this BinaryCookieJar jar, string fileName)
        => jar.Export(File.Open(fileName, FileMode.Create));

    internal static void Export(this BinaryCookieJar jar, Stream stream) =>
        BinaryCookieMetaComposer.Compose(jar.GetCookies(), stream);
}