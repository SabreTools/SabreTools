using System;
using SabreTools.Core.Tools;
using Xunit;

namespace SabreTools.Core.Test.Tools
{
    public class DateTimeHelperTests
    {
        #region ConvertToMsDosTimeFormat

        [Fact]
        public void ConvertToMsDosTimeFormat_MinSupported()
        {
            long expected = 2162688;
            DateTime dateTime = new DateTime(1980, 01, 01, 00, 00, 00, 00);
            long actual = DateTimeHelper.ConvertToMsDosTimeFormat(dateTime);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConvertToMsDosTimeFormat_Y2K()
        {
            long expected = 673251328;
            DateTime dateTime = new DateTime(2000, 01, 01, 00, 00, 00, 00);
            long actual = DateTimeHelper.ConvertToMsDosTimeFormat(dateTime);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ConvertFromMsDosTimeFormat

        [Fact]
        public void ConvertFromMsDosTimeFormat_MinSupported()
        {
            uint msDosDateTime = 2162688;
            DateTime actual = DateTimeHelper.ConvertFromMsDosTimeFormat(msDosDateTime);

            Assert.Equal(1980, actual.Year);
            Assert.Equal(01, actual.Month);
            Assert.Equal(01, actual.Day);
            Assert.Equal(00, actual.Hour);
            Assert.Equal(00, actual.Minute);
            Assert.Equal(00, actual.Second);
            Assert.Equal(000, actual.Millisecond);
        }

        [Fact]
        public void ConvertFromMsDosTimeFormat_Y2K()
        {
            uint msDosDateTime = 673251328;
            DateTime actual = DateTimeHelper.ConvertFromMsDosTimeFormat(msDosDateTime);

            Assert.Equal(2000, actual.Year);
            Assert.Equal(01, actual.Month);
            Assert.Equal(01, actual.Day);
            Assert.Equal(00, actual.Hour);
            Assert.Equal(00, actual.Minute);
            Assert.Equal(00, actual.Second);
            Assert.Equal(000, actual.Millisecond);
        }

        #endregion
    }
}
