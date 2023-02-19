using System.Runtime.InteropServices;

namespace NETBinaryCookie.Types.Meta;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
internal struct BinaryCookieMeta
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

    public BinaryCookieMeta(uint size, uint flags, uint domainOffset, uint nameOffset, uint pathOffset,
        uint valueOffset, uint commentOffset, uint endHeader)
    {
        this.cookieSize = size;
        this.cookieFlags = flags;
        this.domainOffset = domainOffset;
        this.nameOffset = nameOffset;
        this.pathOffset = pathOffset;
        this.valueOffset = valueOffset;
        this.commentOffset = commentOffset;
        this.endHeader = endHeader;
    }
}