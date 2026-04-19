using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;
using SabreTools.Metadata.Filter;
using Xunit;

namespace SabreTools.DatTools.Test
{
    public class SetterTests
    {
        #region SetFields

        // TODO: Add SetFields_DatHeader test

        [Fact]
        public void SetFields_DatItem()
        {
            var datItem = new Rom();
            datItem.SetName("foo");

            var setter = new Setter();
            setter.PopulateSetters(new FilterKey("datitem", "name"), "bar");
            setter.SetFields(datItem);

            Assert.Equal("bar", datItem.GetName());
        }

        [Fact]
        public void SetFields_Machine()
        {
            var machine = new Machine
            {
                Name = "bar",
                Description = "bar",
            };

            var setter = new Setter();
            setter.PopulateSetters(new FilterKey("machine", "name"), "foo");
            setter.SetFields(machine);

            Assert.Equal("foo", machine.Name);
        }

        #endregion
    }
}
