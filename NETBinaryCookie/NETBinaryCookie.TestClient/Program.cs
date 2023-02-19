using NETBinaryCookie;


const string fileName = @"C:\Users\ZackPuhl\Downloads\com.microsoft.teams.binarycookies";

var binaryCookieJar = NetBinaryCookie.ReadFromFile(fileName);

foreach (var cookie in binaryCookieJar.GetCookies())
{
    Console.WriteLine($"DOMAIN: {cookie.Domain}\nNAME: {cookie.Name}\nPATH: {cookie.Path}\nSET: {cookie.Creation}\nEXPIRES: {cookie.Expiration}\nVALUE: {cookie.Value}\n");
}

Environment.Exit(0);
