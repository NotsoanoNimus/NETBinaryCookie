using System.Text;

namespace NETBinaryCookie;

internal static class BinaryReaderExtensions
{
    // SECONDS between 01/01/1970 and 01/01/2001.
    //   This is added to extracted cookie timers because the latter date is what Apple uses
    //   in the BinaryCookie timestamps.
    private const uint OffsetFromNsDateToUnixTime = 978_307_200;

    public static uint ReadBinaryBigEndianUInt32(this BinaryReader rdr) =>
        BitConverter.ToUInt32(rdr.ReadBytes(sizeof(uint)).Reverse().ToArray());

    public static int ReadBinaryBigEndianInt32(this BinaryReader rdr) =>
        BitConverter.ToInt32(rdr.ReadBytes(sizeof(uint)).Reverse().ToArray());

    public static ulong ReadBinaryBigEndianUInt64(this BinaryReader rdr) =>
        BitConverter.ToUInt64(rdr.ReadBytes(sizeof(ulong)).Reverse().ToArray());
    
    public static string? ReadBinaryStringToEnd(this BinaryReader rdr)
    {
        var readByte = rdr.ReadByte();
        var ret = new List<byte> { readByte };

        while (readByte != 0x00)
        {
            readByte = rdr.ReadByte();
            ret.Add(readByte);
        }

        return ret.Count < 1 ? null : Encoding.UTF8.GetString(ret.ToArray());
    }
    
    public static DateTime ReadBinaryNsDateAsDateTime(this BinaryReader rdr)
    {
        var rawData = rdr.ReadBytes(8).ToArray();
                    
        var dateTimeRead = BitConverter.ToDouble(rawData);
        var convertedDateTime = (uint)(OffsetFromNsDateToUnixTime + dateTimeRead);
                    
        return DateTimeOffset.FromUnixTimeSeconds(convertedDateTime).DateTime;
    }

    public static int GetInt32Checksum(this BinaryReader rdr, uint length) =>
        Enumerable.Range(1, (int)length / 4).Aggregate(0, (i, _) => rdr.ReadInt32());
}