using System.Text;
using System.Text.Json;
using NETBinaryCookie.Types;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie;

public static class BinaryCookieJarExtensions
{
    public static string CookiesToJson(this BinaryCookieJar jar) => JsonSerializer.Serialize(jar.GetCookies());

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
        BinaryCookieMetaComposer.Compose(jar.GetCookies(), stream);
}