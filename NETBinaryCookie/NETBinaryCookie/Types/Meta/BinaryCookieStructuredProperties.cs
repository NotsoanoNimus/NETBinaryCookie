using System.Runtime.InteropServices;

namespace NETBinaryCookie.Types.Meta;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 40)]
internal struct BinaryCookieStructuredProperties
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
    public readonly uint endHeader = BinaryCookieMetaConstants.CookieMetaEndMarker;

    public BinaryCookieStructuredProperties(
        int size,
        int flags,
        int domainLen,
        int nameLen,
        int pathLen,
        int valueLen,
        int commentLen)
    {
        this.cookieSize = (uint)size;
        this.cookieFlags = (uint)flags;
        
        // Header struct size + two DateTime doubles always gets counted in offset calcs.
        //   Extra +1 is for null-terminating 0s on strings.
        var prologueLength = (uint)(Marshal.SizeOf<BinaryCookieStructuredProperties>() + (sizeof(long) * 2));
        
        this.domainOffset = prologueLength;
        this.nameOffset = this.domainOffset + (uint)domainLen + 1;
        this.pathOffset = this.nameOffset + (uint)nameLen + 1;
        this.valueOffset = this.pathOffset + (uint)pathLen + 1;
        this.commentOffset = commentLen > 0 ? (this.valueOffset + (uint)valueLen + 1) : 0;
    }
}