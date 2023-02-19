using System.Reflection;
using System.Runtime.InteropServices;

namespace NETBinaryCookie;

internal static class BinaryCookieTranscoder
{
    public static uint ConvertBigEndianBytesToUInt32(this byte[] stream, uint offsetIntoStream = 0)
    {
        if (stream.Length < offsetIntoStream + 3)
        {
            throw new IndexOutOfRangeException("The byte-stream ended before a UInt32 could be read");
        }

        return (uint)((stream[offsetIntoStream] << 24) | (stream[offsetIntoStream + 1] << 16) |
                      (stream[offsetIntoStream + 2] << 8) | stream[offsetIntoStream + 3]);
    }
    
    private static EndiannessAttribute.Endianness GetPropertyEndianness(FieldInfo property)
    {
        try
        {
            return ((EndiannessAttribute)property.GetCustomAttributes(typeof(EndiannessAttribute), false).First())
                .Endian;
        }
        catch (Exception ex)
        {
            // Always assume LE by default since BinaryCookie spec only lists BE in two data locations.
            return EndiannessAttribute.Endianness.LittleEndian;
        }
    }
    
    private static void MaybeAdjustEndianness<T>(this byte[] data, int startOffset = 0)
    {
        var currentEndianness = BitConverter.IsLittleEndian
            ? EndiannessAttribute.Endianness.LittleEndian
            : EndiannessAttribute.Endianness.BigEndian;
        
        foreach (var field in typeof(T).GetFields())
        {
            var fieldType = field.FieldType;
            if (field.IsStatic || fieldType == typeof(string))
            {
                // don't process static fields or swap bytes for strings
                continue;
            }

            var endian = GetPropertyEndianness(field);

            var offset = Marshal.OffsetOf(typeof(T), field.Name).ToInt32();

            if (fieldType.IsEnum)
            {
                fieldType = Enum.GetUnderlyingType(fieldType);
            }

            // check for sub-fields to recurse if necessary
            var subFields = fieldType.GetFields().Where(subField => subField.IsStatic == false).ToArray();

            var effectiveOffset = startOffset + offset;

            if (subFields.Length != 0)
            {
                data.MaybeAdjustEndianness<T>(effectiveOffset);

                return;
            }

            if (endian != currentEndianness)
            {
                Array.Reverse(data, effectiveOffset, Marshal.SizeOf(fieldType));
            }
        }
    }

    internal static byte[] StructToBytes<TStruct>(TStruct data) where TStruct : struct
    {
        var rawData = new byte[Marshal.SizeOf(data)];

        var gcHandle = GCHandle.Alloc(rawData, GCHandleType.Pinned);

        try
        {
            var rawDataStructPtr = gcHandle.AddrOfPinnedObject();
            Marshal.StructureToPtr(data, rawDataStructPtr, false);
        }
        finally
        {
            gcHandle.Free();
        }

        rawData.MaybeAdjustEndianness<TStruct>();

        return rawData;
    }

    internal static TStruct BytesToStruct<TStruct>(byte[] rawData) where TStruct : struct
    {
        rawData.MaybeAdjustEndianness<TStruct>();

        var handle = GCHandle.Alloc(rawData, GCHandleType.Pinned);

        try
        {
            var rawDataPtr = handle.AddrOfPinnedObject();
            
            return (TStruct)Marshal.PtrToStructure(rawDataPtr, typeof(TStruct))!;
        }
        finally
        {
            handle.Free();
        }
    }
}