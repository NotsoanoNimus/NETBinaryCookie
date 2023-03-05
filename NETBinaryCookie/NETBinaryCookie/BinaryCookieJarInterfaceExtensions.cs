using System.Text;
using System.Text.Json;
using System.Xml.Serialization;
using NETBinaryCookie.Types;

namespace NETBinaryCookie;

public static class BinaryCookieJarInterfaceExtensions
{
    public static string CookiesToJson(this IBinaryCookieJar jar) => JsonSerializer.Serialize(jar.GetCookies());

    public static string CookiesToXml(this IBinaryCookieJar jar)
    {
        var stream = new MemoryStream();
        new XmlSerializer(typeof(BinaryCookie[])).Serialize(stream, jar.GetCookies());

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    internal static void Export(this IBinaryCookieJar jar, string fileName)
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

    internal static void Export(this IBinaryCookieJar jar, Stream stream) =>
        BinaryCookieMetaComposer.Compose(jar.GetCookies(), stream, jar.Stub);
}