namespace NETBinaryCookie;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal sealed class EndiannessAttribute : Attribute 
{
    internal enum Endianness
    {
        LittleEndian,
        BigEndian
    }
    
    internal Endianness Endian { get; }

    internal EndiannessAttribute(Endianness endianness)
    {
        this.Endian = endianness;
    }
}