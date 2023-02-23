namespace NETBinaryCookie.TestClient;

using NETBinaryCookie;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("You must specify a binarycookies file to parse.");
            Environment.Exit(1);
        }
        
        var fileName = args[0];

        var binaryCookieJar = NetBinaryCookie.ReadFromFile(fileName);

        Console.WriteLine(binaryCookieJar.CookiesToJson());

        foreach (var cookie in binaryCookieJar.GetCookies())
        {
            Console.WriteLine(cookie + "\n");
        }

        var mySpecificCookies = binaryCookieJar.GetCookies(cookie => cookie.Domain.ToLower().Contains("github."));
        foreach (var cookie in mySpecificCookies)
        {
            Console.WriteLine(cookie);
        }

        var cookiesAfterChristmas =
            binaryCookieJar.GetCookies(cookie => cookie.Creation > new DateTime(2023, 12, 25));

        binaryCookieJar.AddCookie(new()
        {
            Creation = DateTime.Now,
            Expiration = DateTime.Now + TimeSpan.FromDays(5),
            Domain = ".xmit.xyz",
            Name = "HelloFromDotnet",
            Path = "/myurl",
            Value = "Hello, world!",
            Flags = new[] { NetBinaryCookie.CookieFlag.SamesiteLax, NetBinaryCookie.CookieFlag.Secure }
        });

        binaryCookieJar.Save();
        
        Environment.Exit(0);
    }
}