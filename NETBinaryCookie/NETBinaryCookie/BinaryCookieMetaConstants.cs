namespace NETBinaryCookie;

internal static class BinaryCookieMetaConstants
{
    internal const string FileSignature = "cook";
    internal const uint FileSignatureHex = 0x_63_6F_6F_6B;

    internal const uint PageMetaStartMarker = 0x_00_00_01_00;
    internal const uint PageMetaEndMarker = 0x_00_00_00_00;

    internal const uint CookieMetaEndMarker = 0x_00_00_00_00;

    internal const ulong FileFooterSignature = 0x_07_17_20_05_00_00_00_4B;

    internal const uint MaxCookieLength = 0x_10_00;   // 4 KiB max cookie length
    
    // SECONDS between 01/01/1970 and 01/01/2001.
    //   This is added to extracted cookie timers because the latter date is what Apple uses
    //   in the BinaryCookie timestamps.
    internal const uint OffsetFromNsDateToUnixTime = 978_307_200;
}