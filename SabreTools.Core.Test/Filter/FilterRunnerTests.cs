using SabreTools.Core.Filter;
using SabreTools.Data.Models.Metadata;
using Xunit;

namespace SabreTools.Core.Test.Filter
{
    public class FilterRunnerTests
    {
        private static readonly FilterRunner _filterRunner;

        static FilterRunnerTests()
        {
            FilterObject[] filters =
            [
                new FilterObject("header.author", "auth", Operation.Equals),
                new FilterObject("machine.description", "desc", Operation.Equals),
                new FilterObject("item.name", "name", Operation.Equals),
                new FilterObject("rom.crc", "crc", Operation.Equals),
            ];

            _filterRunner = new FilterRunner(filters);
        }

        #region Header

        [Fact]
        public void Header_Missing_False()
        {
            Header header = new Header();
            bool actual = _filterRunner.Run(header);
            Assert.False(actual);
        }

        [Fact]
        public void Header_Null_False()
        {
            Header header = new Header { [Header.AuthorKey] = null };
            bool actual = _filterRunner.Run(header);
            Assert.False(actual);
        }

        [Fact]
        public void Header_Empty_False()
        {
            Header header = new Header { [Header.AuthorKey] = "" };
            bool actual = _filterRunner.Run(header);
            Assert.False(actual);
        }

        [Fact]
        public void Header_Incorrect_False()
        {
            Header header = new Header { [Header.AuthorKey] = "NO_MATCH" };
            bool actual = _filterRunner.Run(header);
            Assert.False(actual);
        }

        [Fact]
        public void Header_Correct_True()
        {
            Header header = new Header { [Header.AuthorKey] = "auth" };
            bool actual = _filterRunner.Run(header);
            Assert.True(actual);
        }

        #endregion

        #region Machine

        [Fact]
        public void Machine_Missing_False()
        {
            Machine machine = new Machine();
            bool actual = _filterRunner.Run(machine);
            Assert.False(actual);
        }

        [Fact]
        public void Machine_Null_False()
        {
            Machine machine = new Machine { [Machine.DescriptionKey] = null };
            bool actual = _filterRunner.Run(machine);
            Assert.False(actual);
        }

        [Fact]
        public void Machine_Empty_False()
        {
            Machine machine = new Machine { [Machine.DescriptionKey] = "" };
            bool actual = _filterRunner.Run(machine);
            Assert.False(actual);
        }

        [Fact]
        public void Machine_Incorrect_False()
        {
            Machine machine = new Machine { [Machine.DescriptionKey] = "NO_MATCH" };
            bool actual = _filterRunner.Run(machine);
            Assert.False(actual);
        }

        [Fact]
        public void Machine_Correct_True()
        {
            Machine machine = new Machine { [Machine.DescriptionKey] = "desc" };
            bool actual = _filterRunner.Run(machine);
            Assert.True(actual);
        }

        #endregion

        #region DatItem (General)

        [Fact]
        public void DatItem_Missing_False()
        {
            DatItem datItem = new Sample();
            bool actual = _filterRunner.Run(datItem);
            Assert.False(actual);
        }

        [Fact]
        public void DatItem_Null_False()
        {
            DatItem datItem = new Sample { [Sample.NameKey] = null };
            bool actual = _filterRunner.Run(datItem);
            Assert.False(actual);
        }

        [Fact]
        public void DatItem_Empty_False()
        {
            DatItem datItem = new Sample { [Sample.NameKey] = "" };
            bool actual = _filterRunner.Run(datItem);
            Assert.False(actual);
        }

        [Fact]
        public void DatItem_Incorrect_False()
        {
            DatItem datItem = new Sample { [Sample.NameKey] = "NO_MATCH" };
            bool actual = _filterRunner.Run(datItem);
            Assert.False(actual);
        }

        [Fact]
        public void DatItem_Correct_True()
        {
            DatItem datItem = new Sample { [Sample.NameKey] = "name" };
            bool actual = _filterRunner.Run(datItem);
            Assert.True(actual);
        }

        #endregion

        #region DatItem (Specific)

        [Fact]
        public void Rom_Missing_False()
        {
            DatItem datItem = new Rom();
            bool actual = _filterRunner.Run(datItem);
            Assert.False(actual);
        }

        [Fact]
        public void Rom_Null_False()
        {
            DatItem datItem = new Rom
            {
                [Rom.NameKey] = "name",
                [Rom.CRCKey] = null,
            };

            bool actual = _filterRunner.Run(datItem);
            Assert.False(actual);
        }

        [Fact]
        public void Rom_Empty_False()
        {
            DatItem datItem = new Rom
            {
                [Rom.NameKey] = "name",
                [Rom.CRCKey] = "",
            };

            bool actual = _filterRunner.Run(datItem);
            Assert.False(actual);
        }

        [Fact]
        public void Rom_Incorrect_False()
        {
            DatItem datItem = new Rom
            {
                [Rom.NameKey] = "name",
                [Rom.CRCKey] = "NO_MATCH",
            };

            bool actual = _filterRunner.Run(datItem);
            Assert.False(actual);
        }

        [Fact]
        public void Rom_Correct_True()
        {
            DatItem datItem = new Rom
            {
                [Rom.NameKey] = "name",
                [Rom.CRCKey] = "crc",
            };

            bool actual = _filterRunner.Run(datItem);
            Assert.True(actual);
        }

        #endregion
    }
}