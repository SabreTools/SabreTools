using SabreTools.Core.Tools;
using Xunit;

namespace SabreTools.Core.Test.Tools
{
    public class ConvertersTests
    {
        #region String to Enum

        [Theory]
        [InlineData(null, null)]
        [InlineData("INVALID", null)]
        [InlineData("yes", true)]
        [InlineData("True", true)]
        [InlineData("no", false)]
        [InlineData("False", false)]
        public void AsYesNoTest(string? field, bool? expected)
        {
            bool? actual = field.AsYesNo();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Enum to String

        [Theory]
        [InlineData(null, null)]
        [InlineData(true, "yes")]
        [InlineData(false, "no")]
        public void FromYesNo(bool? field, string? expected)
        {
            string? actual = field.FromYesNo();
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
