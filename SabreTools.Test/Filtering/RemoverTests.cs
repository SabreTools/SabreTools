using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.Test.Filtering
{
    // TODO: Reenable tests when there's a reasonable way of doing so
    public class RemoverTests
    {
        //[Fact]
        //public void RemoveFieldsDatItemTest()
        //{
        //    var datItem = CreateDatItem();
        //    var remover = new Remover();
        //    remover.PopulateExclusions("DatItem.Name");
        //    remover.RemoveFields(datItem);
        //    Assert.Null(datItem.GetName());
        //}

        //[Fact]
        //public void RemoveFieldsMachineTest()
        //{
        //    var datItem = CreateDatItem();
        //    var remover = new Remover();
        //    remover.PopulateExclusions("Machine.Name");
        //    remover.RemoveFields(datItem);
        //    Assert.Null(datItem.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey));
        //}

        /// <summary>
        /// Generate a consistent DatItem for testing
        /// </summary>
        private static DatItem CreateDatItem()
        {
           var machine = new Machine();
            machine.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, "bar");
            machine.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, "bar");

            var rom = new Rom();
            rom.SetName("foo");
            rom.SetFieldValue<Machine>(DatItem.MachineKey, machine);

            return rom;
        }
    }
}