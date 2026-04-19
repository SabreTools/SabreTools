using System.Collections.Generic;
using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;
using Xunit;

namespace SabreTools.DatTools.Test
{
    public class RemoverTests
    {
        #region PopulateExclusionsFromList

        [Fact]
        public void PopulateExclusionsFromList_Null()
        {
            // Setup the list
            List<string>? exclusions = null;

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusions(exclusions);

            // Check the exclusion lists
            Assert.Empty(remover.HeaderFieldNames);
            Assert.Empty(remover.MachineFieldNames);
            Assert.Empty(remover.ItemFieldNames);
        }

        [Fact]
        public void PopulateExclusionsFromList_Empty()
        {
            // Setup the list
            List<string> exclusions = [];

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusions(exclusions);

            // Check the exclusion lists
            Assert.Empty(remover.HeaderFieldNames);
            Assert.Empty(remover.MachineFieldNames);
            Assert.Empty(remover.ItemFieldNames);
        }

        [Fact]
        public void PopulateExclusionsFromList_HeaderField()
        {
            // Setup the list
            List<string> exclusions =
            [
                "header.datname",
            ];

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusions(exclusions);

            // Check the exclusion lists
            Assert.Empty(remover.HeaderFieldNames);
            Assert.Empty(remover.MachineFieldNames);
            Assert.Empty(remover.ItemFieldNames);
        }

        [Fact]
        public void PopulateExclusionsFromList_MachineField()
        {
            // Setup the list
            List<string> exclusions =
            [
                "machine.name",
            ];

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusions(exclusions);

            // Check the exclusion lists
            Assert.Empty(remover.HeaderFieldNames);
            Assert.Single(remover.MachineFieldNames);
            Assert.Empty(remover.ItemFieldNames);
        }

        [Fact]
        public void PopulateExclusionsFromList_ItemField()
        {
            // Setup the list
            List<string> exclusions =
            [
                "item.name",
            ];

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusions(exclusions);

            // Check the exclusion lists
            Assert.Empty(remover.HeaderFieldNames);
            Assert.Empty(remover.MachineFieldNames);
            Assert.Single(remover.ItemFieldNames);
        }

        #endregion

        #region RemoveFields

        // TODO: Add RemoveFields_DatHeader test

        [Fact]
        public void RemoveFields_DatItem()
        {
            var datItem = new Rom();
            datItem.SetName("foo");

            var remover = new Remover();
            remover.PopulateExclusions("DatItem.Name");
            remover.RemoveFields(datItem);

            Assert.Null(datItem.GetName());
        }

        [Fact]
        public void RemoveFields_Machine()
        {
            var machine = new Machine
            {
                Name = "bar",
                Description = "bar",
            };

            var remover = new Remover();
            remover.PopulateExclusions("Machine.Name");
            remover.RemoveFields(machine);

            Assert.Null(machine.Name);
        }

        #endregion
    }
}
