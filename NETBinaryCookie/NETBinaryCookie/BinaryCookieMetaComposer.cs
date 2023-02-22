using System.Collections.Immutable;
using System.Text;
using NETBinaryCookie.Types;

namespace NETBinaryCookie;

internal static class BinaryCookieMetaComposer
{
    internal static void Compose(ImmutableArray<BinaryCookie> cookies, Stream stream)
    {
        var writer = new BinaryWriter(stream, Encoding.UTF8, false);
        var numCookies = cookies.Length;
    }
}