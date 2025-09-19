using SabreTools.Core.Tools;
using Xunit;

namespace SabreTools.Core.Test.Tools
{
    public class TextHelperTests
    {
        #region NormalizeCharacters

        // TODO: Write tests for NormalizeCharacters

        #endregion

        #region NormalizeCRC32

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "00001234")]
        [InlineData("0x1234", "00001234")]
        [InlineData("1234ABCDE", "")]
        [InlineData("0x1234ABCDE", "")]
        [InlineData("1234ABCD", "1234abcd")]
        [InlineData("0x1234ABCD", "1234abcd")]
        [InlineData("abcdefgh", "")]
        [InlineData("0xabcdefgh", "")]
        public void NormalizeCRC32Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeCRC32(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeMD2

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "00000000000000000000000000001234")]
        [InlineData("0x1234", "00000000000000000000000000001234")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("abcdefghabcdefghabcdefghabcdefgh", "")]
        [InlineData("0xabcdefghabcdefghabcdefghabcdefgh", "")]
        public void NormalizeMD2Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeMD2(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeMD4

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "00000000000000000000000000001234")]
        [InlineData("0x1234", "00000000000000000000000000001234")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("abcdefghabcdefghabcdefghabcdefgh", "")]
        [InlineData("0xabcdefghabcdefghabcdefghabcdefgh", "")]
        public void NormalizeMD4Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeMD4(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeMD5

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "00000000000000000000000000001234")]
        [InlineData("0x1234", "00000000000000000000000000001234")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("abcdefghabcdefghabcdefghabcdefgh", "")]
        [InlineData("0xabcdefghabcdefghabcdefghabcdefgh", "")]
        public void NormalizeMD5Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeMD5(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeRIPEMD128

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "00000000000000000000000000001234")]
        [InlineData("0x1234", "00000000000000000000000000001234")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("abcdefghabcdefghabcdefghabcdefgh", "")]
        [InlineData("0xabcdefghabcdefghabcdefghabcdefgh", "")]
        public void NormalizeRIPEMD128Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeRIPEMD128(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeRIPEMD160

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "0000000000000000000000000000000000001234")]
        [InlineData("0x1234", "0000000000000000000000000000000000001234")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("abcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        [InlineData("0xabcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        public void NormalizeRIPEMD160Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeRIPEMD160(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeSHA1

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "0000000000000000000000000000000000001234")]
        [InlineData("0x1234", "0000000000000000000000000000000000001234")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("abcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        [InlineData("0xabcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        public void NormalizeSHA1Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeSHA1(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeSHA256

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "0000000000000000000000000000000000000000000000000000000000001234")]
        [InlineData("0x1234", "0000000000000000000000000000000000000000000000000000000000001234")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("abcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        [InlineData("0xabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        public void NormalizeSHA256Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeSHA256(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeSHA384

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001234")]
        [InlineData("0x1234", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001234")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("abcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        [InlineData("0xabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        public void NormalizeSHA384Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeSHA384(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeSHA512

        [Theory]
        [InlineData(null, null)]
        [InlineData("", "")]
        [InlineData("-", "")]
        [InlineData("_", "")]
        [InlineData("0x", "")]
        [InlineData("1234", "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001234")]
        [InlineData("0x1234", "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001234")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCDE", "")]
        [InlineData("1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("0x1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD1234ABCD", "1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd1234abcd")]
        [InlineData("abcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        [InlineData("0xabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefghabcdefgh", "")]
        public void NormalizeSHA512Test(string? hash, string? expected)
        {
            string? actual = TextHelper.NormalizeSHA512(hash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region RemovePathUnsafeCharacters

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("\0", "")]
        public void RemovePathUnsafeCharactersTest(string? input, string expected)
        {
            string? actual = TextHelper.RemovePathUnsafeCharacters(input);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region RemoveUnicodeCharacters

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("Ā", "")]
        public void RemoveUnicodeCharactersTest(string? input, string expected)
        {
            string? actual = TextHelper.RemoveUnicodeCharacters(input);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}