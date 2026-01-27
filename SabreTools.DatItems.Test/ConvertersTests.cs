using SabreTools.Core.Tools;
using Xunit;

namespace SabreTools.DatItems.Test
{
    public class ConvertersTests
    {
        #region Generators

        [Theory]
        [InlineData(ChipType.NULL, 2)]
        [InlineData(ControlType.NULL, 15)]
        [InlineData(DeviceType.NULL, 21)]
        [InlineData(DisplayType.NULL, 5)]
        [InlineData(Endianness.NULL, 2)]
        [InlineData(FeatureStatus.NULL, 2)]
        [InlineData(FeatureType.NULL, 14)]
        [InlineData(ItemStatus.NULL, 7)]
        [InlineData(ItemType.NULL, 54)]
        [InlineData(LoadFlag.NULL, 14)]
        [InlineData(MachineType.None, 6)]
        [InlineData(OpenMSXSubType.NULL, 3)]
        [InlineData(Relation.NULL, 6)]
        [InlineData(Runnable.NULL, 3)]
        [InlineData(SoftwareListStatus.None, 3)]
        [InlineData(Supported.NULL, 5)]
        [InlineData(SupportStatus.NULL, 3)]
        public void GenerateToEnumTest<T>(T value, int expected)
        {
            var actual = Converters.GenerateToEnum<T>();
            Assert.Equal(default, value);
            Assert.Equal(expected, actual.Keys.Count);
        }

        #endregion
    }
}
