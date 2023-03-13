namespace NETBinaryCookie.Tests;

public sealed class Test_BinaryReaderExtensions
{
    [Theory]
    [InlineData(new byte[] { 0xFF, 0xEE, 0xDD, 0xCC }, 0xFFEEDDCC)]
    public void Test_ReadBinaryBigEndianUInt32(byte[] bytes, UInt32 read)
    {
        var reader = new BinaryReader(new MemoryStream(bytes));
        
        Assert.Equal(reader.ReadBinaryBigEndianUInt32(), read);
    }
}