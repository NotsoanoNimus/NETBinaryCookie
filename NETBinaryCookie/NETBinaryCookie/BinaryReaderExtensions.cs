using System.Text;

namespace NETBinaryCookie;

internal static class BinaryReaderExtensions
{
    public static uint ReadBinaryBigEndianUInt32(this BinaryReader rdr) =>
        BitConverter.ToUInt32(rdr.ReadBytes(sizeof(uint)).Reverse().ToArray());

    public static int ReadBinaryBigEndianInt32(this BinaryReader rdr) =>
        BitConverter.ToInt32(rdr.ReadBytes(sizeof(uint)).Reverse().ToArray());

    public static ulong ReadBinaryBigEndianUInt64(this BinaryReader rdr) =>
        BitConverter.ToUInt64(rdr.ReadBytes(sizeof(ulong)).Reverse().ToArray());
    
    public static string? ReadBinaryStringToEnd(this BinaryReader rdr)
    {
        var readByte = rdr.ReadByte();
        var ret = new List<byte>();

        while (readByte != 0x00)
        {
            ret.Add(readByte);
            readByte = rdr.ReadByte();
        }

        return ret.Count < 1 ? null : Encoding.UTF8.GetString(ret.ToArray());
    }
    
    public static DateTime ReadBinaryNsDateAsDateTime(this BinaryReader rdr)
    {
        var rawData = rdr.ReadBytes(8).ToArray();
                    
        var dateTimeRead = BitConverter.ToDouble(rawData);
        var convertedDateTime = (uint)(BinaryCookieMetaConstants.OffsetFromNsDateToUnixTime + dateTimeRead);
                    
        return DateTimeOffset.FromUnixTimeSeconds(convertedDateTime).DateTime;
    }

    public static int GetInt32Checksum(this BinaryReader rdr, int rewindToPosition)
    {
        /* Let me tell you about this checksum and the nights I wasted banging my head against the wall changing my
         *   Safari cookies byte-by-byte. Some sample code I'd read from a savvy Swift programmer read:
         *
         *         let data = try BinaryDataEncoder().encode(self)
         *         var checksum: Int32 = 0
         *         for index in stride(from: 0, to: data.count, by: 4) {
         *             checksum += Int32(data[index])
         *         }
         *         return checksum
         * 
         * Source: https://github.com/interstateone/BinaryCookies/blob/master/Sources/BinaryCookies/BinaryCookies.swift#L116
         *
         * Imagine not noticing for DAYS that the Int32 cast is NOT ON AN ARRAY SLICE! It's casting a single data byte
         *   to an Int32 value. So yes, this checksum only adds up every fourth byte in the pages, which is quite
         *   strange considering it's supposed to verify all data integrity and not 1/4 of it.
         *
         * The worst part of this all is the seeming lack of easily available documentation on the file format, but I am
         *   determined to reverse-engineer the best solution that can both read AND write binarycookies. :)
         * 
         */
        var savePos = rdr.BaseStream.Position;
        rdr.BaseStream.Seek(rewindToPosition, SeekOrigin.Begin);

        return rdr.ReadBytes((int)(savePos - rewindToPosition)).Where((_, i) => i % 4 == 0)
            .Aggregate(0, (i, j) => i + j);
    }
}