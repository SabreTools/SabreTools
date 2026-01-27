using System;
using SabreTools.Core.Filter;
using SabreTools.Data.Models.Metadata;
using Xunit;

namespace SabreTools.Core.Test.Filter
{
    public class FilterObjectTests
    {
        #region Constructor

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Sample.Name")]
        [InlineData("Sample.Name++")]
        public void Constructor_InvalidKey_Throws(string? filterString)
        {
            Assert.Throws<ArgumentException>(() => new FilterObject(filterString));
        }

        [Theory]
        [InlineData("Sample.Name=XXXXXX", Operation.Equals)]
        [InlineData("Sample.Name==XXXXXX", Operation.Equals)]
        [InlineData("Sample.Name:XXXXXX", Operation.Equals)]
        [InlineData("Sample.Name::XXXXXX", Operation.Equals)]
        [InlineData("Sample.Name!XXXXXX", Operation.NotEquals)]
        [InlineData("Sample.Name!=XXXXXX", Operation.NotEquals)]
        [InlineData("Sample.Name!:XXXXXX", Operation.NotEquals)]
        [InlineData("Sample.Name>XXXXXX", Operation.GreaterThan)]
        [InlineData("Sample.Name>=XXXXXX", Operation.GreaterThanOrEqual)]
        [InlineData("Sample.Name<XXXXXX", Operation.LessThan)]
        [InlineData("Sample.Name<=XXXXXX", Operation.LessThanOrEqual)]
        [InlineData("Sample.Name:!XXXXXX", Operation.NONE)]
        [InlineData("Sample.Name=>XXXXXX", Operation.NONE)]
        [InlineData("Sample.Name=<XXXXXX", Operation.NONE)]
        [InlineData("Sample.Name<>XXXXXX", Operation.NONE)]
        [InlineData("Sample.Name><XXXXXX", Operation.NONE)]
        public void Constructor_FilterString(string filterString, Operation expected)
        {
            FilterObject filterObject = new FilterObject(filterString);
            Assert.Equal("sample", filterObject.Key.ItemName);
            Assert.Equal("name", filterObject.Key.FieldName);
            Assert.Equal("XXXXXX", filterObject.Value);
            Assert.Equal(expected, filterObject.Operation);
        }

        [Theory]
        [InlineData("Sample.Name", "XXXXXX", "=", Operation.Equals)]
        [InlineData("Sample.Name", "XXXXXX", "==", Operation.Equals)]
        [InlineData("Sample.Name", "XXXXXX", ":", Operation.Equals)]
        [InlineData("Sample.Name", "XXXXXX", "::", Operation.Equals)]
        [InlineData("Sample.Name", "XXXXXX", "!", Operation.NotEquals)]
        [InlineData("Sample.Name", "XXXXXX", "!=", Operation.NotEquals)]
        [InlineData("Sample.Name", "XXXXXX", "!:", Operation.NotEquals)]
        [InlineData("Sample.Name", "XXXXXX", ">", Operation.GreaterThan)]
        [InlineData("Sample.Name", "XXXXXX", ">=", Operation.GreaterThanOrEqual)]
        [InlineData("Sample.Name", "XXXXXX", "<", Operation.LessThan)]
        [InlineData("Sample.Name", "XXXXXX", "<=", Operation.LessThanOrEqual)]
        [InlineData("Sample.Name", "XXXXXX", "@@", Operation.NONE)]
        [InlineData("Sample.Name", "XXXXXX", ":!", Operation.NONE)]
        [InlineData("Sample.Name", "XXXXXX", "=>", Operation.NONE)]
        [InlineData("Sample.Name", "XXXXXX", "=<", Operation.NONE)]
        public void Constructor_TripleString(string itemField, string? value, string? operation, Operation expected)
        {
            FilterObject filterObject = new FilterObject(itemField, value, operation);
            Assert.Equal("sample", filterObject.Key.ItemName);
            Assert.Equal("name", filterObject.Key.FieldName);
            Assert.Equal("XXXXXX", filterObject.Value);
            Assert.Equal(expected, filterObject.Operation);
        }

        #endregion

        #region MatchesEqual

        [Fact]
        public void MatchesEqual_NoKey()
        {
            var sample = new Sample();
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.Equals);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesEqual_NoValue()
        {
            var sample = new Sample { [Sample.NameKey] = null };
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.Equals);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesEqual_BoolValue()
        {
            var sample = new Sample { [Sample.NameKey] = "true" };
            FilterObject filterObject = new FilterObject("Sample.Name", "yes", Operation.Equals);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesEqual_Int64Value()
        {
            var sample = new Sample { [Sample.NameKey] = "12345" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12345", Operation.Equals);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesEqual_DoubleValue()
        {
            var sample = new Sample { [Sample.NameKey] = "12.345" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12.345", Operation.Equals);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesEqual_RegexValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "^XXXXXX$", Operation.Equals);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesEqual_StringValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "XXXXXX", Operation.Equals);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        #endregion

        #region MatchesNotEqual

        [Fact]
        public void MatchesNotEqual_NoKey()
        {
            var sample = new Sample();
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.NotEquals);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesNotEqual_NoValue()
        {
            var sample = new Sample { [Sample.NameKey] = null };
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.NotEquals);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesNotEqual_BoolValue()
        {
            var sample = new Sample { [Sample.NameKey] = "true" };
            FilterObject filterObject = new FilterObject("Sample.Name", "yes", Operation.NotEquals);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesNotEqual_Int64Value()
        {
            var sample = new Sample { [Sample.NameKey] = "12345" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12345", Operation.NotEquals);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesNotEqual_DoubleValue()
        {
            var sample = new Sample { [Sample.NameKey] = "12.345" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12.345", Operation.NotEquals);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesNotEqual_RegexValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "^XXXXXX$", Operation.NotEquals);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesNotEqual_StringValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "XXXXXX", Operation.NotEquals);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        #endregion

        #region MatchesGreaterThan

        [Fact]
        public void MatchesGreaterThan_NoKey()
        {
            var sample = new Sample();
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.GreaterThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesGreaterThan_NoValue()
        {
            var sample = new Sample { [Sample.NameKey] = null };
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.GreaterThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesGreaterThan_BoolValue()
        {
            var sample = new Sample { [Sample.NameKey] = "true" };
            FilterObject filterObject = new FilterObject("Sample.Name", "yes", Operation.GreaterThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesGreaterThan_Int64Value()
        {
            var sample = new Sample { [Sample.NameKey] = "12346" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12345", Operation.GreaterThan);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesGreaterThan_DoubleValue()
        {
            var sample = new Sample { [Sample.NameKey] = "12.346" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12.345", Operation.GreaterThan);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesGreaterThan_RegexValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "^XXXXXX$", Operation.GreaterThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesGreaterThan_StringValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "XXXXXX", Operation.GreaterThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        #endregion

        #region MatchesGreaterThanOrEqual

        [Fact]
        public void MatchesGreaterThanOrEqual_NoKey()
        {
            var sample = new Sample();
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.GreaterThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesGreaterThanOrEqual_NoValue()
        {
            var sample = new Sample { [Sample.NameKey] = null };
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.GreaterThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesGreaterThanOrEqual_BoolValue()
        {
            var sample = new Sample { [Sample.NameKey] = "true" };
            FilterObject filterObject = new FilterObject("Sample.Name", "yes", Operation.GreaterThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesGreaterThanOrEqual_Int64Value()
        {
            var sample = new Sample { [Sample.NameKey] = "12346" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12345", Operation.GreaterThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesGreaterThanOrEqual_DoubleValue()
        {
            var sample = new Sample { [Sample.NameKey] = "12.346" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12.345", Operation.GreaterThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesGreaterThanOrEqual_RegexValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "^XXXXXX$", Operation.GreaterThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesGreaterThanOrEqual_StringValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "XXXXXX", Operation.GreaterThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        #endregion

        #region MatchesLessThan

        [Fact]
        public void MatchesLessThan_NoKey()
        {
            var sample = new Sample();
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.LessThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesLessThan_NoValue()
        {
            var sample = new Sample { [Sample.NameKey] = null };
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.LessThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesLessThan_BoolValue()
        {
            var sample = new Sample { [Sample.NameKey] = "true" };
            FilterObject filterObject = new FilterObject("Sample.Name", "yes", Operation.LessThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesLessThan_Int64Value()
        {
            var sample = new Sample { [Sample.NameKey] = "12344" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12345", Operation.LessThan);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesLessThan_DoubleValue()
        {
            var sample = new Sample { [Sample.NameKey] = "12.344" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12.345", Operation.LessThan);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesLessThan_RegexValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "^XXXXXX$", Operation.LessThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesLessThan_StringValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "XXXXXX", Operation.LessThan);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        #endregion

        #region MatchesLessThanOrEqual

        [Fact]
        public void MatchesLessThanOrEqual_NoKey()
        {
            var sample = new Sample();
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.LessThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesLessThanOrEqual_NoValue()
        {
            var sample = new Sample { [Sample.NameKey] = null };
            FilterObject filterObject = new FilterObject("Sample.Name", null, Operation.LessThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesLessThanOrEqual_BoolValue()
        {
            var sample = new Sample { [Sample.NameKey] = "true" };
            FilterObject filterObject = new FilterObject("Sample.Name", "yes", Operation.LessThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesLessThanOrEqual_Int64Value()
        {
            var sample = new Sample { [Sample.NameKey] = "12344" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12345", Operation.LessThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesLessThanOrEqual_DoubleValue()
        {
            var sample = new Sample { [Sample.NameKey] = "12.344" };
            FilterObject filterObject = new FilterObject("Sample.Name", "12.345", Operation.LessThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.True(actual);
        }

        [Fact]
        public void MatchesLessThanOrEqual_RegexValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "^XXXXXX$", Operation.LessThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        [Fact]
        public void MatchesLessThanOrEqual_StringValue()
        {
            var sample = new Sample { [Sample.NameKey] = "XXXXXX" };
            FilterObject filterObject = new FilterObject("Sample.Name", "XXXXXX", Operation.LessThanOrEqual);
            bool actual = filterObject.Matches(sample);
            Assert.False(actual);
        }

        #endregion
    }
}
