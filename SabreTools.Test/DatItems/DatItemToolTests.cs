using System.Collections.Generic;

using SabreTools.Core;
using SabreTools.DatItems;
using Xunit;

namespace SabreTools.Test.DatItems
{
    public class DatItemToolTests
    {
        [Fact]
        public void SetFieldsDatItemTest()
        {
            var datItem = CreateDatItem();
            var mappings = new Dictionary<DatItemField, string> { [DatItemField.Name] = "bar" };
            DatItemTool.SetFields(datItem, mappings, null);
            Assert.Equal("bar", datItem.GetName());
        }

        [Fact]
        public void SetFieldsMachineTest()
        {
            var datItem = CreateDatItem();
            var mappings = new Dictionary<MachineField, string> { [MachineField.Name] = "foo" };
            DatItemTool.SetFields(datItem, null, mappings);
            Assert.Equal("foo", datItem.Machine.Name);
        }

        /// <summary>
        /// Generate a consistent DatItem for testing
        /// </summary>
        private DatItem CreateDatItem()
        {
            return new Rom
            {
                Name = "foo",
                Machine = new Machine
                {
                    Name = "bar",
                    Description = "bar",
                }
            };
        }
    }
}