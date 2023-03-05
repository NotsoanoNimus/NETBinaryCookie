using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using NETBinaryCookie.Types;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie;

public static class BinaryCookieJarExtensions
{
    public static string CookiesToJson(this BinaryCookieJar jar) => JsonSerializer.Serialize(jar.GetCookies());

    public static string CookiesToXml(this BinaryCookieJar jar)
    {
        var stream = new MemoryStream();
        new XmlSerializer(typeof(BinaryCookie[])).Serialize(stream, jar.GetCookies());

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    internal static void Export(this BinaryCookieJar jar, string fileName)
    {
        var backupFileName = $@"{fileName}.{Guid.NewGuid()}";
        
        // Create a backup file just in case something goes wrong.
        if (File.Exists(fileName))
        {
            File.Copy(fileName, backupFileName);
        }

        using var fileStream = File.Open(fileName, FileMode.Create);
        
        try
        {
            jar.Export(fileStream);
        }
        finally
        {
            fileStream.Close();
        }

        // If the export DOES NOT throw, the backup should be removed.
        if (File.Exists(backupFileName))
        {
            File.Delete(backupFileName);
        }
    }

    internal static void Export(this BinaryCookieJar jar, Stream stream) =>
        BinaryCookieMetaComposer.Compose(jar.GetCookies(), stream, jar.Stub ?? new byte[] { });
}