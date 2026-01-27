using Xunit;

namespace SabreTools.DatItems.Test
{
    public class SourceTests
    {
        #region Clone

        [Fact]
        public void CloneTest()
        {
            Source item = new Source(1, source: "src");

            object clone = item.Clone();
            Source? actual = clone as Source;
            Assert.NotNull(actual);
            Assert.Equal(1, actual.Index);
            Assert.Equal("src", actual.Name);
        }

        #endregion
    }
}
