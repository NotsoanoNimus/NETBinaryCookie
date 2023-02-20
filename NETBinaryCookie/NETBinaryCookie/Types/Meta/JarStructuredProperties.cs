using System.Runtime.InteropServices;

namespace NETBinaryCookie.Types.Meta;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal struct JarStructuredProperties
{
    [FieldOffset(0)] [MarshalAs(UnmanagedType.U4)] [Endianness(EndiannessAttribute.Endianness.BigEndian)]
    public readonly uint signature;

    [FieldOffset(4)] [MarshalAs(UnmanagedType.U4)] [Endianness(EndiannessAttribute.Endianness.BigEndian)]
    public readonly uint numPages;

    public JarStructuredProperties(uint signature, uint numPages)
    {
        this.signature = signature;
        this.numPages = numPages;
    }
}