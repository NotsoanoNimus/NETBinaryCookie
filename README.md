# NETBinaryCookie

A simple, independent Apple binarycookies format read-write package for .NET projects.

Supports reading and parsing lists of cookies from an input binarycookies file or stream otherwise. Does not yet support writing modified lists of cookies back to disk.

### TODOs

- [ ] Reverse-engineer the checksum calculation used and implement it successfully in the library.
- [ ] Ability to write a cookie jar back to its source file (or another stream/file) after modifying.
- [ ] Thorough unit testing to detect breaking changes easily.
- [ ] Expository README documentation that will help others even more thoroughly than other guides/specifications have helped me!

### Dependencies

The dependency graph for this package is kept intentionally minimal, and concerted effort has been made to keep it dependency-free.

### Feedback

Please submit any bug reports, feature requests, or other concerns on the [project's GitHub page](https://github.com/NotsoanoNimus/NETBinaryCookie).

To send appreciation, feel free to send me a direct email: `github@xmit.xyz`    

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
```
