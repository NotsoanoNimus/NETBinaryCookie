namespace NETBinaryCookie;

internal static class BinaryWriterExtensions
{
    public static void WriteDateTimeAsBinaryNsDate(this BinaryWriter wtr, DateTime dateTime)
    {
        var toUnix = (dateTime.ToUniversalTime() - DateTime.UnixEpoch).TotalSeconds;
        
        var convertedDateTime = toUnix - BinaryCookieMetaConstants.OffsetFromNsDateToUnixTime;

        var rawData = BitConverter.GetBytes(convertedDateTime).ToArray();   // field is Big-Endian
        
        wtr.Write(rawData);
    }
}