using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;
using Xunit;

namespace SabreTools.DatTools.Test
{
    public class CleaningTests
    {
        [Fact]
        public void CleanDatItem_Normalize()
        {
            var datItem = new Rom();
            datItem.SetName("name");

            var machine = new Machine
            {
                Name = "\"ÁБ\"",
                Description = "ä|/Ж",
            };

            var cleaner = new Cleaner { Normalize = true };
            cleaner.CleanDatItem(datItem, machine);

            Assert.Equal("name", datItem.GetName());
            Assert.Equal("'AB'", machine.Name);
            Assert.Equal("ae-Zh", machine.Description);
        }

        [Fact]
        public void CleanDatItem_RemoveUnicode()
        {
            var datItem = new Rom();
            datItem.SetName("nam诶");

            var machine = new Machine
            {
                Name = "nam诶-2",
                Description = "nam诶-3",
            };

            var cleaner = new Cleaner { RemoveUnicode = true };
            cleaner.CleanDatItem(datItem, machine);

            Assert.Equal("nam", datItem.GetName());
            Assert.Equal("nam-2", machine.Name);
            Assert.Equal("nam-3", machine.Description);
        }

        [Fact]
        public void CleanDatItem_Single()
        {
            var datItem = new Rom();
            datItem.SetName("name");

            var machine = new Machine
            {
                Name = "name-2",
                Description = "name-3",
            };

            var cleaner = new Cleaner { Single = true };
            cleaner.CleanDatItem(datItem, machine);

            Assert.Equal("name", datItem.GetName());
            Assert.Equal("!", machine.Name);
            Assert.Equal("!", machine.Description);
        }

        [Theory]
        [InlineData(null, "name")]
        [InlineData("", "name")]
        [InlineData("C:\\Normal\\Depth\\Path", "name")]
        [InlineData("C:\\AbnormalFolderLengthPath\\ThatReallyPushesTheLimit\\OfHowLongYou\\ReallyShouldNameThings\\AndItGetsEvenWorse\\TheMoreSubfoldersThatYouTraverse\\BecauseWhyWouldYouStop\\AtSomethingReasonable\\LikeReallyThisIsGettingDumb\\AndIKnowItsJustATest\\ButNotAsMuchAsMe", "nam")]
        public void CleanDatItem_TrimRoot(string? root, string expected)
        {
            var datItem = new Rom();
            datItem.SetName("name");

            var machine = new Machine
            {
                Name = "name-2",
                Description = "name-3",
            };

            var cleaner = new Cleaner
            {
                Trim = true,
                Root = root,
            };
            cleaner.CleanDatItem(datItem, machine);

            Assert.Equal(expected, datItem.GetName());
            Assert.Equal("name-2", machine.Name);
            Assert.Equal("name-3", machine.Description);
        }
    }
}
