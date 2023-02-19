using System.Runtime.InteropServices;

namespace NETBinaryCookie.Types.Meta;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal struct PageMeta
{
    [FieldOffset(0)] [MarshalAs(UnmanagedType.U4)] [Endianness(EndiannessAttribute.Endianness.BigEndian)]
    public readonly uint pageStart;

    [FieldOffset(4)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint numCookies;
}