using System.Runtime.InteropServices;

namespace NETBinaryCookie.Types.Meta;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
public struct BinaryCookieMeta
{
    [FieldOffset(0)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint cookieSize;
    
    // 4 bytes discarded here as 'unknownOne'

    [FieldOffset(8)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint cookieFlags;
    
    // 4 bytes discarded here as 'unknownTwo'

    [FieldOffset(16)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint domainOffset;

    [FieldOffset(20)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint nameOffset;

    [FieldOffset(24)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint pathOffset;

    [FieldOffset(28)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint valueOffset;

    [FieldOffset(32)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint commentOffset;

    [FieldOffset(36)] [MarshalAs(UnmanagedType.U4)]
    public readonly uint endHeader;
}