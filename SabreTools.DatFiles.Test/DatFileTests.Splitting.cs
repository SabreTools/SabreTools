using SabreTools.DatFiles.Formats;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using Xunit;

namespace SabreTools.DatFiles.Test
{
    public partial class DatFileTests
    {
        #region AddItemsFromChildren

        [Fact]
        public void AddItemsFromChildren_Items_Dedup()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(parentItem, statsOnly: false);
            datFile.AddItem(matchChildItem, statsOnly: false);
            datFile.AddItem(noMatchChildItem, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromChildren(subfolder: true, skipDedup: false);

            Assert.Equal(2, datFile.GetItemsForBucket("parent").Count);
        }

        [Fact]
        public void AddItemsFromChildren_Items_SkipDedup()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(parentItem, statsOnly: false);
            datFile.AddItem(matchChildItem, statsOnly: false);
            datFile.AddItem(noMatchChildItem, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromChildren(subfolder: true, skipDedup: true);

            Assert.Equal(3, datFile.GetItemsForBucket("parent").Count);
        }

        [Fact]
        public void AddItemsFromChildren_ItemsDB_Dedup()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long biosMachineIndex = datFile.AddMachineDB(parentMachine);
            long deviceMachineIndex = datFile.AddMachineDB(childMachine);
            long sourceIndex = datFile.AddSourceDB(source);
            _ = datFile.AddItemDB(parentItem, biosMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(matchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(noMatchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromChildren(subfolder: true, skipDedup: false);

            Assert.Equal(2, datFile.GetItemsForBucketDB("parent").Count);
        }

        [Fact]
        public void AddItemsFromChildren_ItemsDB_SkipDedup()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long biosMachineIndex = datFile.AddMachineDB(parentMachine);
            long deviceMachineIndex = datFile.AddMachineDB(childMachine);
            long sourceIndex = datFile.AddSourceDB(source);
            _ = datFile.AddItemDB(parentItem, biosMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(matchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(noMatchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromChildren(subfolder: true, skipDedup: true);

            Assert.Equal(3, datFile.GetItemsForBucketDB("parent").Count);
        }

        #endregion

        #region AddItemsFromCloneOfParent

        [Fact]
        public void AddItemsFromCloneOfParent_Items()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(parentItem, statsOnly: false);
            datFile.AddItem(matchChildItem, statsOnly: false);
            datFile.AddItem(noMatchChildItem, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromCloneOfParent();

            Assert.Equal(2, datFile.GetItemsForBucket("child").Count);
        }

        [Fact]
        public void AddItemsFromCloneOfParent_ItemsDB()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long biosMachineIndex = datFile.AddMachineDB(parentMachine);
            long deviceMachineIndex = datFile.AddMachineDB(childMachine);
            long sourceIndex = datFile.AddSourceDB(source);
            _ = datFile.AddItemDB(parentItem, biosMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(matchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(noMatchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromCloneOfParent();

            Assert.Equal(2, datFile.GetItemsForBucketDB("child").Count);
        }

        #endregion

        #region AddItemsFromDevices

        [Theory]
        [InlineData(false, false, 4)]
        [InlineData(false, true, 4)]
        [InlineData(true, false, 3)]
        [InlineData(true, true, 3)]
        public void AddItemsFromDevices_Items(bool deviceOnly, bool useSlotOptions, int expected)
        {
            Source source = new Source(0, source: null);

            Machine deviceMachine = new Machine();
            deviceMachine.SetName("device");
            deviceMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsDeviceKey, true);

            Machine slotOptionMachine = new Machine();
            slotOptionMachine.SetName("slotoption");

            Machine itemMachine = new Machine();
            itemMachine.SetName("machine");

            DatItem deviceItem = new Sample();
            deviceItem.SetName("device_item");
            deviceItem.SetFieldValue<Machine>(DatItem.MachineKey, deviceMachine);
            deviceItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem slotOptionItem = new Sample();
            slotOptionItem.SetName("slot_option_item");
            slotOptionItem.SetFieldValue<Machine>(DatItem.MachineKey, slotOptionMachine);
            slotOptionItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem datItem = new Rom();
            datItem.SetName("rom");
            datItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            datItem.SetFieldValue<Machine>(DatItem.MachineKey, itemMachine);
            datItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem deviceRef = new DeviceRef();
            deviceRef.SetName("device");
            deviceRef.SetFieldValue<Machine>(DatItem.MachineKey, itemMachine);
            deviceRef.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem slotOption = new SlotOption();
            slotOption.SetName("slotoption");
            slotOption.SetFieldValue<Machine>(DatItem.MachineKey, itemMachine);
            slotOption.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(deviceItem, statsOnly: false);
            datFile.AddItem(slotOptionItem, statsOnly: false);
            datFile.AddItem(datItem, statsOnly: false);
            datFile.AddItem(deviceRef, statsOnly: false);
            datFile.AddItem(slotOption, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromDevices(deviceOnly, useSlotOptions);

            Assert.Equal(expected, datFile.GetItemsForBucket("machine").Count);
        }

        [Theory]
        [InlineData(false, false, 4)]
        [InlineData(false, true, 4)]
        [InlineData(true, false, 3)]
        [InlineData(true, true, 3)]
        public void AddItemsFromDevices_ItemsDB(bool deviceOnly, bool useSlotOptions, int expected)
        {
            Source source = new Source(0, source: null);

            Machine deviceMachine = new Machine();
            deviceMachine.SetName("device");
            deviceMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsDeviceKey, true);

            Machine slotOptionMachine = new Machine();
            slotOptionMachine.SetName("slotoption");

            Machine itemMachine = new Machine();
            itemMachine.SetName("machine");

            DatItem deviceItem = new Sample();
            deviceItem.SetName("device_item");

            DatItem slotOptionItem = new Sample();
            slotOptionItem.SetName("slot_option_item");

            DatItem datItem = new Rom();
            datItem.SetName("rom");
            datItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");

            DatItem deviceRef = new DeviceRef();
            deviceRef.SetName("device");

            DatItem slotOption = new SlotOption();
            slotOption.SetName("slotoption");

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long deviceMachineIndex = datFile.AddMachineDB(deviceMachine);
            long slotOptionMachineIndex = datFile.AddMachineDB(slotOptionMachine);
            long itemMachineIndex = datFile.AddMachineDB(itemMachine);
            long sourceIndex = datFile.AddSourceDB(source);
            _ = datFile.AddItemDB(deviceItem, deviceMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(slotOptionItem, slotOptionMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(datItem, itemMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(deviceRef, itemMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(slotOption, itemMachineIndex, sourceIndex, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromDevices(deviceOnly, useSlotOptions);

            Assert.Equal(expected, datFile.GetItemsForBucketDB("machine").Count);
        }

        #endregion

        #region AddItemsFromRomOfParent

        [Fact]
        public void AddItemsFromRomOfParent_Items()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.RomOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(parentItem, statsOnly: false);
            datFile.AddItem(matchChildItem, statsOnly: false);
            datFile.AddItem(noMatchChildItem, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromRomOfParent();

            Assert.Equal(2, datFile.GetItemsForBucket("child").Count);
        }

        [Fact]
        public void AddItemsFromRomOfParent_ItemsDB()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.RomOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long biosMachineIndex = datFile.AddMachineDB(parentMachine);
            long deviceMachineIndex = datFile.AddMachineDB(childMachine);
            long sourceIndex = datFile.AddSourceDB(source);
            _ = datFile.AddItemDB(parentItem, biosMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(matchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(noMatchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.AddItemsFromRomOfParent();

            Assert.Equal(2, datFile.GetItemsForBucketDB("child").Count);
        }

        #endregion

        #region RemoveBiosAndDeviceSets

        [Fact]
        public void RemoveBiosAndDeviceSets_Items()
        {
            Source source = new Source(0, source: null);

            Machine biosMachine = new Machine();
            biosMachine.SetName("bios");
            biosMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            Machine deviceMachine = new Machine();
            deviceMachine.SetName("device");
            deviceMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsDeviceKey, true);

            DatItem biosItem = new Rom();
            biosItem.SetFieldValue<Machine>(DatItem.MachineKey, biosMachine);
            biosItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem deviceItem = new Rom();
            deviceItem.SetFieldValue<Machine>(DatItem.MachineKey, deviceMachine);
            deviceItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(biosItem, statsOnly: false);
            datFile.AddItem(deviceItem, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.RemoveBiosAndDeviceSets();

            Assert.Empty(datFile.GetItemsForBucket("bios"));
            Assert.Empty(datFile.GetItemsForBucket("device"));
        }

        [Fact]
        public void RemoveBiosAndDeviceSets_ItemsDB()
        {
            Source source = new Source(0, source: null);

            Machine biosMachine = new Machine();
            biosMachine.SetName("bios");
            biosMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            Machine deviceMachine = new Machine();
            deviceMachine.SetName("device");
            deviceMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsDeviceKey, true);

            DatItem biosItem = new Rom();
            DatItem deviceItem = new Rom();

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long biosMachineIndex = datFile.AddMachineDB(biosMachine);
            long deviceMachineIndex = datFile.AddMachineDB(deviceMachine);
            long sourceIndex = datFile.AddSourceDB(source);
            _ = datFile.AddItemDB(biosItem, biosMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(deviceItem, deviceMachineIndex, sourceIndex, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.RemoveBiosAndDeviceSets();

            Assert.Empty(datFile.GetMachinesDB());
        }

        #endregion

        #region RemoveItemsFromCloneOfChild

        [Fact]
        public void RemoveItemsFromCloneOfChild_Items()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");
            parentMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.RomOfKey, "romof");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "parent");

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(parentItem, statsOnly: false);
            datFile.AddItem(matchChildItem, statsOnly: false);
            datFile.AddItem(noMatchChildItem, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.RemoveItemsFromCloneOfChild();

            Assert.Single(datFile.GetItemsForBucket("parent"));
            DatItem actual = Assert.Single(datFile.GetItemsForBucket("child"));
            Machine? actualMachine = actual.GetMachine();
            Assert.NotNull(actualMachine);
            Assert.Equal("child", actualMachine.GetName());
            Assert.Equal("romof", actualMachine.GetStringFieldValue(Data.Models.Metadata.Machine.RomOfKey));
        }

        [Fact]
        public void RemoveItemsFromCloneOfChild_ItemsDB()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");
            parentMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.RomOfKey, "romof");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "parent");

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long biosMachineIndex = datFile.AddMachineDB(parentMachine);
            long deviceMachineIndex = datFile.AddMachineDB(childMachine);
            long sourceIndex = datFile.AddSourceDB(source);
            _ = datFile.AddItemDB(parentItem, biosMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(matchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(noMatchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.RemoveItemsFromCloneOfChild();

            Assert.Single(datFile.GetItemsForBucketDB("parent"));
            long actual = Assert.Single(datFile.GetItemsForBucketDB("child")).Key;
            Machine? actualMachine = datFile.GetMachineForItemDB(actual).Value;
            Assert.NotNull(actualMachine);
            Assert.Equal("child", actualMachine.GetName());
            Assert.Equal("romof", actualMachine.GetStringFieldValue(Data.Models.Metadata.Machine.RomOfKey));
        }

        #endregion

        #region RemoveItemsFromRomOfChild

        [Fact]
        public void RemoveItemsFromRomOfChild_Items()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.RomOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(parentItem, statsOnly: false);
            datFile.AddItem(matchChildItem, statsOnly: false);
            datFile.AddItem(noMatchChildItem, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.RemoveItemsFromRomOfChild();

            Assert.Single(datFile.GetItemsForBucket("parent"));
            DatItem actual = Assert.Single(datFile.GetItemsForBucket("child"));
            Machine? actualMachine = actual.GetMachine();
            Assert.NotNull(actualMachine);
            Assert.Equal("child", actualMachine.GetName());
        }

        [Fact]
        public void RemoveItemsFromRomOfChild_ItemsDB()
        {
            Source source = new Source(0, source: null);

            Machine parentMachine = new Machine();
            parentMachine.SetName("parent");

            Machine childMachine = new Machine();
            childMachine.SetName("child");
            childMachine.SetFieldValue<string?>(Data.Models.Metadata.Machine.RomOfKey, "parent");
            childMachine.SetFieldValue<bool>(Data.Models.Metadata.Machine.IsBiosKey, true);

            DatItem parentItem = new Rom();
            parentItem.SetName("parent_rom");
            parentItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            parentItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            parentItem.SetFieldValue<Machine>(DatItem.MachineKey, parentMachine);
            parentItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem matchChildItem = new Rom();
            matchChildItem.SetName("match_child_rom");
            matchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            matchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            matchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            matchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatItem noMatchChildItem = new Rom();
            noMatchChildItem.SetName("no_match_child_rom");
            noMatchChildItem.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, 12345);
            noMatchChildItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "beefdead");
            noMatchChildItem.SetFieldValue<Machine>(DatItem.MachineKey, childMachine);
            noMatchChildItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long biosMachineIndex = datFile.AddMachineDB(parentMachine);
            long deviceMachineIndex = datFile.AddMachineDB(childMachine);
            long sourceIndex = datFile.AddSourceDB(source);
            _ = datFile.AddItemDB(parentItem, biosMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(matchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);
            _ = datFile.AddItemDB(noMatchChildItem, deviceMachineIndex, sourceIndex, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.RemoveItemsFromRomOfChild();

            Assert.Single(datFile.GetItemsForBucketDB("parent"));
            long actual = Assert.Single(datFile.GetItemsForBucketDB("child")).Key;
            Machine? actualMachine = datFile.GetMachineForItemDB(actual).Value;
            Assert.NotNull(actualMachine);
            Assert.Equal("child", actualMachine.GetName());
        }

        #endregion

        #region RemoveMachineRelationshipTags

        [Fact]
        public void RemoveMachineRelationshipTags_Items()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("machine");
            machine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "XXXXXX");
            machine.SetFieldValue<string?>(Data.Models.Metadata.Machine.RomOfKey, "XXXXXX");
            machine.SetFieldValue<string?>(Data.Models.Metadata.Machine.SampleOfKey, "XXXXXX");

            DatItem datItem = new Rom();
            datItem.SetFieldValue<Machine>(DatItem.MachineKey, machine);
            datItem.SetFieldValue<Source>(DatItem.SourceKey, source);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(datItem, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.RemoveMachineRelationshipTags();

            DatItem actualItem = Assert.Single(datFile.GetItemsForBucket("machine"));
            Machine? actual = actualItem.GetMachine();
            Assert.NotNull(actual);
            Assert.Null(actual.GetStringFieldValue(Data.Models.Metadata.Machine.CloneOfKey));
            Assert.Null(actual.GetStringFieldValue(Data.Models.Metadata.Machine.RomOfKey));
            Assert.Null(actual.GetStringFieldValue(Data.Models.Metadata.Machine.SampleOfKey));
        }

        [Fact]
        public void RemoveMachineRelationshipTags_ItemsDB()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("machine");
            machine.SetFieldValue<string?>(Data.Models.Metadata.Machine.CloneOfKey, "XXXXXX");
            machine.SetFieldValue<string?>(Data.Models.Metadata.Machine.RomOfKey, "XXXXXX");
            machine.SetFieldValue<string?>(Data.Models.Metadata.Machine.SampleOfKey, "XXXXXX");

            DatItem datItem = new Rom();

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long machineIndex = datFile.AddMachineDB(machine);
            long sourceIndex = datFile.AddSourceDB(source);
            _ = datFile.AddItemDB(datItem, machineIndex, sourceIndex, statsOnly: false);

            datFile.BucketBy(ItemKey.Machine);
            datFile.RemoveMachineRelationshipTags();

            Machine actual = Assert.Single(datFile.GetMachinesDB()).Value;
            Assert.Null(actual.GetStringFieldValue(Data.Models.Metadata.Machine.CloneOfKey));
            Assert.Null(actual.GetStringFieldValue(Data.Models.Metadata.Machine.RomOfKey));
            Assert.Null(actual.GetStringFieldValue(Data.Models.Metadata.Machine.SampleOfKey));
        }

        #endregion
    }
}