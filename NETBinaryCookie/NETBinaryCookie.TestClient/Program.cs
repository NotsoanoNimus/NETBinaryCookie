using NETBinaryCookie;


const string fileName = @"C:\Users\ZackPuhl\Downloads\Cookies.binarycookies";

var binaryCookieJar = NetBinaryCookie.ReadFromFile(fileName);

foreach (var cookie in binaryCookieJar.GetCookies())
{
    Console.WriteLine($"DOMAIN: {cookie.Domain}\nNAME: {cookie.Name}\nPATH: {cookie.Path}\n" +
                      $"FLAGS: {(cookie.Flags.Length > 0 ? string.Join(", ", cookie.Flags) : "(none)")}\n" +
                      $"SET: {cookie.Creation}\nEXPIRES: {cookie.Expiration}\nVALUE: {cookie.Value}\n");
}

Environment.Exit(0);
