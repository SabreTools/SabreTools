using SabreTools.Core.Tools;
using Xunit;

namespace SabreTools.DatFiles.Test
{
    public class ConvertersTests
    {
        #region Generators

        [Theory]
        [InlineData(MergingFlag.None, 12)]
        [InlineData(NodumpFlag.None, 4)]
        [InlineData(PackingFlag.None, 8)]
        public void GenerateToEnumTest<T>(T value, int expected)
        {
            var actual = Converters.GenerateToEnum<T>();
            Assert.Equal(default, value);
            Assert.Equal(expected, actual.Keys.Count);
        }

        #endregion
    }
}
