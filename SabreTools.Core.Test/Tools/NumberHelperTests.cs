using System;
using SabreTools.Core.Tools;
using Xunit;

namespace SabreTools.Core.Test.Tools
{
    public class NumberHelperTests
    {
        #region ConvertToDouble

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("INVALID")]
        public void ConvertToDoubleTest_NullExpected(string? numeric)
        {
            double? actual = NumberHelper.ConvertToDouble(numeric);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("0", 0f)]
        [InlineData("100", 100f)]
        [InlineData("-100", -100f)]
        [InlineData("3.14", 3.14f)]
        [InlineData("-3.14", -3.14f)]
        public void ConvertToDoubleTest_NumericExpected(string? numeric, double expected)
        {
            double? actual = NumberHelper.ConvertToDouble(numeric);
            Assert.NotNull(actual);
            double variance = Math.Abs(expected - actual.Value);
            Assert.True(variance < 0.1f);
        }

        #endregion

        #region ConvertToInt64

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("INVALID")]
        [InlineData("0b0001")]
        [InlineData("0o765")]
        [InlineData("01h")]
        public void ConvertToInt64_NullExpected(string? numeric)
        {
            long? actual = NumberHelper.ConvertToInt64(numeric);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData(" 0 ", 0)]
        [InlineData("100", 100)]
        [InlineData("-100", -100)]
        [InlineData("0x01", 1)]
        [InlineData("1k", 1_000)]
        [InlineData("1ki", 1_024)]
        [InlineData("1m", 1_000_000)]
        [InlineData("1mi", 1_048_576)]
        [InlineData("1g", 1_000_000_000)]
        [InlineData("1gi", 1_073_741_824)]
        [InlineData("1t", 1_000_000_000_000)]
        [InlineData("1ti", 1_099_511_627_776)]
        [InlineData("1p", 1_000_000_000_000_000)]
        [InlineData("1pi", 1_125_899_906_842_624)]
        // [InlineData("1e", 1_000_000_000_000_000_000)]
        // [InlineData("1ei", 1_152_921_504_606_846_976)]
        // [InlineData("1z", 1_000_000_000_000_000_000_000)]
        // [InlineData("1zi", 1_180_591_620_717_411_303_424)]
        // [InlineData("1y", 1_000_000_000_000_000_000_000_000)]
        // [InlineData("1yi", 1_208_925_819_614_629_174_706_176)]
        public void ConvertToInt64_NumericExpected(string? numeric, long expected)
        {
            long? actual = NumberHelper.ConvertToInt64(numeric);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region IsNumeric

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("0x", false)]
        [InlineData("0", true)]
        [InlineData("100", true)]
        [InlineData("-100", true)]
        [InlineData("3.14", true)]
        [InlineData("-3.14", true)]
        [InlineData("1,000", true)]
        [InlineData("-1,000", true)]
        [InlineData("1k", true)]
        [InlineData("1ki", true)]
        public void IsNumericTest(string? value, bool expected)
        {
            bool actual = NumberHelper.IsNumeric(value);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetBytesReadable

        [Theory]
        [InlineData(0, "0 B")]
        [InlineData(1, "1 B")]
        [InlineData(-1, "-1 B")]
        [InlineData(0x400, "1 KB")]
        [InlineData(-0x400, "-1 KB")]
        [InlineData(1_234, "1.205 KB")]
        [InlineData(-1_234, "-1.205 KB")]
        [InlineData(0x10_0000, "1 MB")]
        [InlineData(-0x10_0000, "-1 MB")]
        [InlineData(0x4000_0000, "1 GB")]
        [InlineData(-0x4000_0000, "-1 GB")]
        [InlineData(0x100_0000_0000, "1 TB")]
        [InlineData(-0x100_0000_0000, "-1 TB")]
        [InlineData(0x4_0000_0000_0000, "1 PB")]
        [InlineData(-0x4_0000_0000_0000, "-1 PB")]
        public void GetBytesReadableTest(long input, string expected)
        {
            string actual = NumberHelper.GetBytesReadable(input);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
