using SabreTools.Core.Tools;
using Xunit;

namespace SabreTools.Core.Test.Tools
{
    public class UtilitiesTests
    {
        // TODO: Replace when Serialization is updated
        #region ConditionalHashEquals

        [Theory]
        [InlineData(null, null, true)]
        [InlineData(new byte[0], new byte[0], true)]
        [InlineData(new byte[] { 0x01 }, new byte[0], true)]
        [InlineData(new byte[0], new byte[] { 0x01 }, true)]
        [InlineData(new byte[] { 0x01 }, new byte[] { 0x01 }, true)]
        [InlineData(new byte[] { 0x01, 0x02 }, new byte[] { 0x01 }, false)]
        [InlineData(new byte[] { 0x01 }, new byte[] { 0x01, 0x02 }, false)]
        [InlineData(new byte[] { 0x01, 0x02 }, new byte[] { 0x02, 0x01 }, false)]
        public void ConditionalHashEquals_Array(byte[]? firstHash, byte[]? secondHash, bool expected)
        {
            bool actual = Utilities.ConditionalHashEquals(firstHash, secondHash);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData("", "", true)]
        [InlineData("01", "", true)]
        [InlineData("", "01", true)]
        [InlineData("01", "01", true)]
        [InlineData("0102", "01", false)]
        [InlineData("01", "0102", false)]
        [InlineData("0102", "0201", false)]
        public void ConditionalHashEquals_String(string? firstHash, string? secondHash, bool expected)
        {
            bool actual = Utilities.ConditionalHashEquals(firstHash, secondHash);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetDepotPath

        [Theory]
        [InlineData(null, 0, null)]
        [InlineData(null, 4, null)]
        [InlineData(new byte[] { 0x12, 0x34, 0x56 }, 0, null)]
        [InlineData(new byte[] { 0x12, 0x34, 0x56 }, 4, null)]
        [InlineData(new byte[] { 0xda, 0x39, 0xa3, 0xee, 0x5e, 0x6b, 0x4b, 0x0d, 0x32, 0x55, 0xbf, 0xef, 0x95, 0x60, 0x18, 0x90, 0xaf, 0xd8, 0x07, 0x09 }, -1, "da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(new byte[] { 0xda, 0x39, 0xa3, 0xee, 0x5e, 0x6b, 0x4b, 0x0d, 0x32, 0x55, 0xbf, 0xef, 0x95, 0x60, 0x18, 0x90, 0xaf, 0xd8, 0x07, 0x09 }, 0, "da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(new byte[] { 0xda, 0x39, 0xa3, 0xee, 0x5e, 0x6b, 0x4b, 0x0d, 0x32, 0x55, 0xbf, 0xef, 0x95, 0x60, 0x18, 0x90, 0xaf, 0xd8, 0x07, 0x09 }, 1, "da\\da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        public void GetDepotPath_Array(byte[]? hash, int depth, string? expected)
        {
            string? actual = Utilities.GetDepotPath(hash, depth);
            if (System.IO.Path.DirectorySeparatorChar == '/')
                expected = expected?.Replace('\\', '/');

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, 0, null)]
        [InlineData(null, 4, null)]
        [InlineData("123456", 0, null)]
        [InlineData("123456", 4, null)]
        [InlineData("da39a3ee5e6b4b0d3255bfef95601890afd80709", -1, "da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData("da39a3ee5e6b4b0d3255bfef95601890afd80709", 0, "da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData("da39a3ee5e6b4b0d3255bfef95601890afd80709", 1, "da\\da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        public void GetDepotPath_String(string? hash, int depth, string? expected)
        {
            string? actual = Utilities.GetDepotPath(hash, depth);
            if (System.IO.Path.DirectorySeparatorChar == '/')
                expected = expected?.Replace('\\', '/');

            Assert.Equal(expected, actual);
        }

        #endregion

        #region HasValidDatExtension

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("no-extension", false)]
        [InlineData("no-extension.", false)]
        [InlineData("invalid.ext", false)]
        [InlineData("invalid..ext", false)]
        [InlineData("INVALID.EXT", false)]
        [InlineData("INVALID..EXT", false)]
        [InlineData(".dat", true)]
        [InlineData(".DAT", true)]
        [InlineData("valid_extension.dat", true)]
        [InlineData("valid_extension..dat", true)]
        [InlineData("valid_extension.DAT", true)]
        [InlineData("valid_extension..DAT", true)]
        public void HasValidDatExtensionTest(string? path, bool expected)
        {
            bool actual = Utilities.HasValidDatExtension(path);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
