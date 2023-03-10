using System.Runtime.InteropServices;

namespace NETBinaryCookie.Types.Meta;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 8)]
internal struct PageStructuredProperties
{
    [FieldOffset(0)] [MarshalAs(UnmanagedType.U4)] [Endianness(EndiannessAttribute.Endianness.BigEndian)]
    public readonly uint pageStart = BinaryCookieMetaConstants.PageMetaStartMarker;

    [FieldOffset(4)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint numCookies;

    public PageStructuredProperties(uint numCookies)
    {
        this.numCookies = numCookies;
    }
}