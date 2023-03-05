using System.Text;
using NETBinaryCookie.Types;

namespace NETBinaryCookie.TestClient;

using NETBinaryCookie;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("USAGE: bcj.exe {inputFile} {outputFile} [-f XML/JSON]\n\t" +
                              "Reads a binarycookies file and outputs its contents to the specified\n\t" +
                              "output file, in the specified formatting (defaults to JSON).");
            Environment.Exit(1);
        }
        
        var fileName = args[0];
        var outputFileName = args[1];
        var outputFormat = args.Length > 2 ? args[2].ToLowerInvariant() : null;
        BinaryCookieJar binaryCookieJar = null!;

        if (!Directory.Exists(Path.GetDirectoryName(outputFileName)))
        {
            Console.WriteLine($"Output directory '{Path.GetDirectoryName(outputFileName)}' does not exist");
            Environment.Exit(1);
        }

        try
        {
            binaryCookieJar = NetBinaryCookie.ReadFromFile(fileName);
        }
        catch (BinaryCookieException bex)
        {
            Console.WriteLine($"Failed to parse the input file: {bex}");
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Parsing binarycookies file failed for an unknown reason: {ex}");
            Environment.Exit(1);
        }

        try
        {
            var fileStream = File.Create(outputFileName);

            fileStream.Write(outputFormat switch
            {
                "xml" => Encoding.UTF8.GetBytes(binaryCookieJar.CookiesToXml()),
                _ => Encoding.UTF8.GetBytes(binaryCookieJar.CookiesToJson()),
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write output file: {ex}");
            Environment.Exit(1);
        }

        Environment.Exit(0);
    }
}