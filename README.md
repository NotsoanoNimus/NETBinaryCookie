# NETBinaryCookie

A simple, independent Apple binarycookies format read-write package for .NET projects.

Supports parsing lists of cookies from an input binarycookies file or stream otherwise.
Also supports writing those cookies back to disk or another stream.

This package is [available on NuGet](https://www.nuget.org/packages/NETBinaryCookie)!

### TODOs

- [X] Reverse-engineer the checksum calculation used and implement it successfully in the library.
- [ ] Ability to write a cookie jar back to its source file (or another stream/file) after modifying.
- [ ] Thorough unit testing to detect breaking changes easily.
- [ ] Expository README documentation that will help others even more thoroughly than other guides/specifications have helped me!

### Dependencies

The dependency graph for this package is kept intentionally minimal, and concerted effort has been made to keep it dependency-free.

### Feedback

Please submit any bug reports, feature requests, or other concerns on the [project's GitHub page](https://github.com/NotsoanoNimus/NETBinaryCookie).

### Example Usage

Below demonstrates a very rudimentary usage of the read functions within this package.

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


## File Format Specification

#### Structure

The binarycookies file format is composed of sets of `cookies`, each contained together in `pages`.
There can be multiple `pages`, which together comprise the binarycookies file itself.

> FILE_HEADER
> > PAGE1
> > > COOKIE1
> > > COOKIE2
> > > COOKIE3
> > PAGE2
> > > COOKIE4
> > > COOKIE5
> CHECKSUM
> FILE_FOOTER
> (STUB)

The start of each file describes the layout of the `pages`, and the start of each `page` describes the layout of its corresponding `cookies`.

At the end of the file, a checksum follows the final cookie of the final page. Then there's an identifying file footer.

After that there may or may not be a bplist (binary plist) stub describing some extra properties about the container itself.

#### Byte-by-byte

At the top of the binarycookies file is a header which gives the count of total pages and each of their offsets from the beginning of the file.

| Field | Alias (if any) | Type | Size (bytes) | Endianness (B/L) | Description |
|:------|:---------------|:-----|:------------:|:----------------:|:------------|
| signature | x | byte[] | 4 | B | The 'binarycookies' file signature; always `cook` (or `0x636f6f6b`). |
| numPages | P | UInt32 | 4 | B | The number of pages in the file. |
| pageSizes | x | UInt32 | P * 4 | B | A sequential list of page sizes (offsets). |


Next begins immediately the first page of the file. There will always be `P` page headers in a file, corresponding to `numPages`.

| Field | Alias (if any) | Type | Size (bytes) | Endianness (B/L) | Description |
|:------|:---------------|:-----|:------------:|:----------------:|:------------|
| pageHeader | x | byte[] | 4 | B | Signature which indicates the start of a new page of cookies. Equal to `0x00000100`. |
| numCookies | C | UInt32 | 4 | L | Indicates the number of cookies contained in the page. |
| cookieOffsets | x | UInt32 | C * 4 | L | An array of integers indicating the starting offset of each cookie from the beginning of `pageHeader`. |
| pageFooter | x | byte[] | 4 | B | Signature marking the end of the page header. Always `0x00000000`. |


Finally, after the `pageFooter` begins the first cookie from the page. Each cookie is composed the same way, and the first byte of the cookie can always be found using the `cookieOffsets` index in the page header.

_NOTE_: It is possible for there to be some padding in between cookies in the page, but it's unlikely. What is better, however, is to always trust the `cookieOffsets` table more than relying on there being no padding between cookies.

| Field | Alias (if any) | Type | Size (bytes) | Endianness (B/L) | Description |
|:------|:---------------|:-----|:------------:|:----------------:|:------------|
| cookieSize | x | UInt32 | 4 | L | Indicates the size of this cookie in bytes. |
| unknownOne | x | byte | 4 | ? | Unknown or otherwise reserved value. |
| cookieFlags | x | UInt32 | 4 | L | Bitwise flags indicating cookie properties like `Secure` and `Samesite` policies. |
| unknownTwo | x | byte | 4 | ? | Unknown or otherwise reserved value. |
| domainOffset | x | UInt32 | 4 | L | Offset to cookie domain. |
| nameOffset | x | UInt32 | 4 | L | Offset to cookie name. |
| pathOffset | x | UInt32 | 4 | L | Offset to cookie path. |
| valueOffset | x | UInt32 | 4 | L | Offset to cookie value. |
| commentOffset | x | UInt32 | 4 | L | Offset to cookie comment. This value is often null, meaning there is no cookie comment. |
| separator | x | byte | 4 | B | Marks the end of the cookie offset table. Always `0x00000000`. |
| expires | x | double | 8 | B | Cookie expiration time in NSDate format. Add `978,307,200` to convert to Unix epoch. |
| creation | x | double | 8 | B | Cookie creation time in NSDate format. Add `978,307,200` to convert to Unix epoch. |
| domain | x | string | strlen(domain) | x | Cookie domain. |
| name | x | string | strlen(domain) | x | Cookie name. |
| path | x | string | strlen(domain) | x | Cookie path. |
| value | x | string | strlen(domain) | x | Cookie value. |
| comment | x | string | strlen(domain) | x | Cookie comment. |

<sup>*Note: Any of the string fields for the cookie can be in any order according to offset values, as strings are simply null-terminated values (most of the time).*</sup>


When that page of cookies is finished, the next one should immediately begin. But, again, don't rely on that: you should always trust the offset/size table for the document.

At the very end of the last cookie of the last table, there is a 4-byte checksum and an 8-byte signature, then an optional `bplist` stub.

| Field | Alias (if any) | Type | Size (bytes) | Endianness (B/L) | Description |
|:------|:---------------|:-----|:------------:|:----------------:|:------------|
| checksum | x | UInt32 | 4 | L | A 32-bit checksum calculated by adding every fourth byte in each page together, then summing all page checksums. |
| fileFooter | x | byte | 8 | B | A binarycookies file footer signature. Should always be `0717 2005 0000 004B`. |
| propertiesStub | x | byte | Any | ? | (_Optional_) A binary plist-formatted stub on the end of the file which contains extra metadata about the file. |
