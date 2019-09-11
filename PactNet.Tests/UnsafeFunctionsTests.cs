namespace PactNet.Tests
{
    using PactNet.Core;
    using Xunit;

    public class UnsafeFunctionsTests
    {
        [Fact]
        public void ToNullTerminated()
        {
            Assert.Equal(new byte[] { 0 }, NativeRustFunctions.CStyleStringFromSystemString(string.Empty));
            Assert.Equal(new byte[] { 65, 0 }, NativeRustFunctions.CStyleStringFromSystemString("A"));
            Assert.Equal(new byte[] { 196, 132, 196, 134, 0 }, NativeRustFunctions.CStyleStringFromSystemString("ĄĆ"));
            Assert.Equal(new byte[] { 196, 132, 0, 196, 134, 0 }, NativeRustFunctions.CStyleStringFromSystemString("Ą\0Ć"));
        }

        [Fact]
        public unsafe void FromNullTerminated()
        {
            fixed(byte* p = new byte[] { 0 })
                Assert.Equal(string.Empty, NativeRustFunctions.SystemStringFromCStyleString(p));
            fixed (byte* p = new byte[] { 65, 0 })
                Assert.Equal("A", NativeRustFunctions.SystemStringFromCStyleString(p));
            fixed (byte* p = new byte[] { 196, 132, 196, 134, 0 })
                Assert.Equal("ĄĆ", NativeRustFunctions.SystemStringFromCStyleString(p));
            fixed (byte* p = new byte[] { 196, 132, 0, 196, 134, 0 })
                Assert.Equal("Ą", NativeRustFunctions.SystemStringFromCStyleString(p));
        }
    }
}
