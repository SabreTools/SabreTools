using Xunit;

namespace SabreTools.DatItems.Test
{
    public class MachineTests
    {
        #region Clone

        [Fact]
        public void CloneTest()
        {
            Machine item = new Machine();
            item.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "name");

            object clone = item.Clone();
            Machine? actual = clone as Machine;
            Assert.NotNull(actual);
            Assert.Equal("name", actual.GetName());
        }

        #endregion

        #region GetInternalClone

        [Fact]
        public void GetInternalCloneTest()
        {
            Machine item = new Machine();
            item.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "name");

            Data.Models.Metadata.Machine actual = item.GetInternalClone();
            Assert.Equal("name", actual[Data.Models.Metadata.Machine.NameKey]);
        }

        #endregion

        #region Equals

        [Fact]
        public void Equals_Null_False()
        {
            Machine self = new Machine();
            Machine? other = null;

            bool actual = self.Equals(other);
            Assert.False(actual);
        }

        [Fact]
        public void Equals_DefaultInternal_True()
        {
            Machine self = new Machine();
            Machine? other = new Machine();

            bool actual = self.Equals(other);
            Assert.True(actual);
        }

        [Fact]
        public void Equals_MismatchedInternal_False()
        {
            Machine self = new Machine();
            self.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "self");

            Machine? other = new Machine();
            other.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "other");

            bool actual = self.Equals(other);
            Assert.False(actual);
        }

        [Fact]
        public void Equals_EqualInternal_True()
        {
            Machine self = new Machine();
            self.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "name");

            Machine? other = new Machine();
            other.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "name");

            bool actual = self.Equals(other);
            Assert.True(actual);
        }

        #endregion
    }
}