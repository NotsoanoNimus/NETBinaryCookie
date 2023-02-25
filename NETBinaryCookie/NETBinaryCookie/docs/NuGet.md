# NETBinaryCookie

A simple, independent Apple binarycookies format read-write package for .NET projects.

Makes Mac cookie forensics without limitations a breeze with .NET!

Supports parsing lists of cookies from an input binarycookies file or stream otherwise.
Also supports writing those cookies back to disk or another stream.

### Dependencies

The dependency graph for this package is kept intentionally minimal, and concerted effort has been made to keep it dependency-free.

### Feedback

Please submit any bug reports, feature requests, or other concerns on the [project's GitHub page](https://github.com/NotsoanoNimus/NETBinaryCookie)
(where you can also find a lot more juicy information about the `binarycookies` file format!).

To drop me a line, feel free to send me a direct email: `github@xmit.xyz`    

### Example Usage

Below demonstrates a very rudimentary usage of the read functions within this package.

At some later time, the to-be-implemented _write_ functions will make modifying saved cookie stores much more viable.

```c#
using NETBinaryCookie;

// ...

var cookieJar = NETBinaryCookie.ReadFromFile(@"/Users/myuser/Documents/Cookies.binarycookies");

var mySpecificCookies = cookieJar.GetCookies(
    cookie => cookie.Domain.ToLower().Contains("github."));
// ...

var cookiesAfterChristmas = cookieJar.GetCookies(
    cookie => cookie.Creation > new DateTime(2023, 12, 25));

foreach (var cookie in cookiesAfterChristmas)
{
    Console.WriteLine(cookie);
}
// ...

// If you're so inclined to do MORE than read the cookies file...
cookieJar.AddCookie(new()
    {
        Creation = DateTime.Now,
        Expiration = DateTime.Now + TimeSpan.FromDays(5),
        Domain = ".xmit.xyz",
        Name = "HelloFromDotnet",
        Path = "/myurl",
        Value = "Hello, world!",
        Flags = new[] { NetBinaryCookie.CookieFlag.SamesiteLax, NetBinaryCookie.CookieFlag.Secure }
    });

cookieJar.Save();
```