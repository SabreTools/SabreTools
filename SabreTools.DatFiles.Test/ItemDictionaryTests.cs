using System.Collections.Generic;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using Xunit;

namespace SabreTools.DatFiles.Test
{
    public class ItemDictionaryTests
    {
        #region AddItem

        [Fact]
        public void AddItem_Disk_WithHashes()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem disk = new Disk();
            disk.SetName("item");
            disk.SetFieldValue<string?>(Data.Models.Metadata.Disk.SHA1Key, "deadbeef");
            disk.SetFieldValue<Source?>(DatItem.SourceKey, source);
            disk.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(disk, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is Disk);
            Assert.Equal("none", actual.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey));
        }

        [Fact]
        public void AddItem_Disk_WithoutHashes()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem disk = new Disk();
            disk.SetName("item");
            disk.SetFieldValue<Source?>(DatItem.SourceKey, source);
            disk.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(disk, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is Disk);
            Assert.Equal("nodump", actual.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey));
        }

        [Fact]
        public void AddItem_File_WithHashes()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            var file = new File();
            file.SetName("item");
            file.SHA1 = "deadbeef";
            file.SetFieldValue<Source?>(DatItem.SourceKey, source);
            file.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(file, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is File);
            //Assert.Equal("none", actual.GetStringFieldValue(File.StatusKey));
        }

        [Fact]
        public void AddItem_File_WithoutHashes()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem file = new File();
            file.SetName("item");
            file.SetFieldValue<Source?>(DatItem.SourceKey, source);
            file.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(file, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is File);
            //Assert.Equal("nodump", actual.GetStringFieldValue(File.StatusKey));
        }

        [Fact]
        public void AddItem_Media_WithHashes()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem media = new Media();
            media.SetName("item");
            media.SetFieldValue<string?>(Data.Models.Metadata.Media.SHA1Key, "deadbeef");
            media.SetFieldValue<Source?>(DatItem.SourceKey, source);
            media.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(media, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is Media);
            //Assert.Equal("none", actual.GetStringFieldValue(Data.Models.Metadata.Media.StatusKey));
        }

        [Fact]
        public void AddItem_Media_WithoutHashes()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem media = new Media();
            media.SetName("item");
            media.SetFieldValue<Source?>(DatItem.SourceKey, source);
            media.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(media, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is Media);
            //Assert.Equal("nodump", actual.GetStringFieldValue(Data.Models.Metadata.Media.StatusKey));
        }

        [Fact]
        public void AddItem_Rom_WithHashesWithSize()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem rom = new Rom();
            rom.SetName("item");
            rom.SetFieldValue<long?>(Data.Models.Metadata.Rom.SizeKey, 12345);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "deadbeef");
            rom.SetFieldValue<Source?>(DatItem.SourceKey, source);
            rom.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(rom, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is Rom);
            Assert.Equal(12345, actual.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey));
            Assert.Equal("deadbeef", actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal("none", actual.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey));
        }

        [Fact]
        public void AddItem_Rom_WithoutHashesWithSize()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem rom = new Rom();
            rom.SetName("item");
            rom.SetFieldValue<long?>(Data.Models.Metadata.Rom.SizeKey, 12345);
            rom.SetFieldValue<Source?>(DatItem.SourceKey, source);
            rom.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(rom, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is Rom);
            Assert.Equal(12345, actual.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey));
            Assert.Null(actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal("nodump", actual.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey));
        }

        [Fact]
        public void AddItem_Rom_WithHashesWithoutSize()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem rom = new Rom();
            rom.SetName("item");
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "deadbeef");
            rom.SetFieldValue<Source?>(DatItem.SourceKey, source);
            rom.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(rom, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is Rom);
            Assert.Null(actual.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey));
            Assert.Equal("deadbeef", actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal("none", actual.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey));
        }

        [Fact]
        public void AddItem_Rom_WithoutHashesWithoutSize()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem rom = new Rom();
            rom.SetName("item");
            rom.SetFieldValue<Source?>(DatItem.SourceKey, source);
            rom.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(rom, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default"));
            Assert.True(actual is Rom);
            Assert.Equal(0, actual.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey));
            Assert.Equal(ZeroHash.SHA1Str, actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal("none", actual.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey));
        }

        [Fact]
        public void AddItem_StatsOnly()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem item = new Rom();
            item.SetName("item");
            item.SetFieldValue<Source?>(DatItem.SourceKey, source);
            item.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(item, statsOnly: true);

            Assert.Empty(dict.GetItemsForBucket("default"));
        }

        [Fact]
        public void AddItem_NormalAdd()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem item = new Rom();
            item.SetName("item");
            item.SetFieldValue<Source?>(DatItem.SourceKey, source);
            item.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(item, statsOnly: false);

            Assert.Single(dict.GetItemsForBucket("default"));
        }

        #endregion

        #region ClearMarked

        [Fact]
        public void ClearMarkedTest()
        {
            // Setup the items
            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEAEEF");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom1.CopyMachineInformation(machine);

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEAEEF");
            rom2.SetFieldValue<bool?>(DatItem.RemoveKey, true);
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "000000e948edcb4f7704b8af85a77a3339ecce44");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom2.CopyMachineInformation(machine);

            // Setup the dictionary
            var dict = new ItemDictionary();
            dict.AddItem(rom1, statsOnly: false);
            dict.AddItem(rom2, statsOnly: false);

            dict.ClearMarked();
            Assert.Empty(dict.GetItemsForBucket("default"));
            List<DatItem> items = dict.GetItemsForBucket("game-1");
            Assert.Single(items);
        }

        #endregion

        #region GetItemsForBucket

        [Fact]
        public void GetItemsForBucket_NullBucketName()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("machine");

            DatItem item = new Rom();
            item.SetFieldValue<Source?>(DatItem.SourceKey, source);
            item.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(item, statsOnly: false);

            var actual = dict.GetItemsForBucket(null, filter: false);

            Assert.Empty(actual);
        }

        [Fact]
        public void GetItemsForBucket_InvalidBucketName()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("machine");

            DatItem item = new Rom();
            item.SetFieldValue<Source?>(DatItem.SourceKey, source);
            item.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(item, statsOnly: false);

            var actual = dict.GetItemsForBucket("INVALID", filter: false);

            Assert.Empty(actual);
        }

        [Fact]
        public void GetItemsForBucket_RemovedFilter()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("machine");

            DatItem item = new Rom();
            item.SetFieldValue<bool?>(DatItem.RemoveKey, true);
            item.SetFieldValue<Source?>(DatItem.SourceKey, source);
            item.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(item, statsOnly: false);

            var actual = dict.GetItemsForBucket("machine", filter: true);

            Assert.Empty(actual);
        }

        [Fact]
        public void GetItemsForBucket_RemovedNoFilter()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("machine");

            DatItem item = new Rom();
            item.SetFieldValue<bool?>(DatItem.RemoveKey, true);
            item.SetFieldValue<Source?>(DatItem.SourceKey, source);
            item.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(item, statsOnly: false);

            var actual = dict.GetItemsForBucket("machine", filter: false);

            Assert.Single(actual);
        }

        [Fact]
        public void GetItemsForBucket_Standard()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("machine");

            DatItem item = new Rom();
            item.SetFieldValue<Source?>(DatItem.SourceKey, source);
            item.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(item, statsOnly: false);

            var actual = dict.GetItemsForBucket("machine", filter: false);

            Assert.Single(actual);
        }

        #endregion

        #region RemoveBucket

        [Fact]
        public void RemoveBucketTest()
        {
            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem datItem = new Rom();
            datItem.SetName("rom-1");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEAEEF");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            datItem.CopyMachineInformation(machine);

            var dict = new ItemDictionary();
            dict.AddItem(datItem, statsOnly: false);

            dict.RemoveBucket("game-1");

            Assert.Empty(dict.GetItemsForBucket("default"));
            Assert.Empty(dict.GetItemsForBucket("game-1"));
        }

        #endregion

        #region RemoveItem

        [Fact]
        public void RemoveItemTest()
        {
            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem datItem = new Rom();
            datItem.SetName("rom-1");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEAEEF");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            datItem.CopyMachineInformation(machine);

            var dict = new ItemDictionary();
            dict.AddItem(datItem, statsOnly: false);

            dict.RemoveItem("game-1", (Rom)datItem.Clone(), 0);

            Assert.Empty(dict.GetItemsForBucket("default"));
            Assert.Empty(dict.GetItemsForBucket("game-1"));
        }

        #endregion

        #region BucketBy

        [Theory]
        [InlineData(ItemKey.NULL, 2)]
        [InlineData(ItemKey.Machine, 2)]
        [InlineData(ItemKey.CRC, 1)]
        [InlineData(ItemKey.SHA1, 4)]
        public void BucketByTest(ItemKey itemKey, int expected)
        {
            // Setup the items
            Machine machine1 = new Machine();
            machine1.SetName("game-1");

            Machine machine2 = new Machine();
            machine2.SetName("game-2");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEAEEF");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom1.CopyMachineInformation(machine1);

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEAEEF");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "000000e948edcb4f7704b8af85a77a3339ecce44");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom1.CopyMachineInformation(machine1);

            DatItem rom3 = new Rom();
            rom3.SetName("rom-3");
            rom3.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEAEEF");
            rom3.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "00000ea4014ce66679e7e17d56ac510f67e39e26");
            rom3.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom1.CopyMachineInformation(machine2);

            DatItem rom4 = new Rom();
            rom4.SetName("rom-4");
            rom4.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEAEEF");
            rom4.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "00000151d437442e74e5134023fab8bf694a2487");
            rom4.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom1.CopyMachineInformation(machine2);

            // Setup the dictionary
            var dict = new ItemDictionary();
            dict.AddItem(rom1, statsOnly: false);
            dict.AddItem(rom2, statsOnly: false);
            dict.AddItem(rom3, statsOnly: false);
            dict.AddItem(rom4, statsOnly: false);

            dict.BucketBy(itemKey);
            Assert.Equal(expected, dict.SortedKeys.Length);
        }

        #endregion

        #region Deduplicate

        [Fact]
        public void DeduplicateTest()
        {
            // Setup the items
            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom1.CopyMachineInformation(machine);

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom2.CopyMachineInformation(machine);

            // Setup the dictionary
            var dict = new ItemDictionary();
            dict.AddItem(rom1, statsOnly: false);
            dict.AddItem(rom2, statsOnly: false);

            dict.Deduplicate();
            Assert.Equal(1, dict.DatStatistics.TotalCount);
        }

        #endregion

        #region GetDuplicateStatus

        [Fact]
        public void GetDuplicateStatus_NullOther_NoDupe()
        {
            var dict = new ItemDictionary();
            DatItem item = new Rom();
            DatItem? lastItem = null;
            var actual = dict.GetDuplicateStatus(item, lastItem);
            Assert.Equal((DupeType)0x00, actual);
        }

        [Fact]
        public void GetDuplicateStatus_DifferentTypes_NoDupe()
        {
            var dict = new ItemDictionary();
            var rom = new Rom();
            DatItem? lastItem = new Disk();
            var actual = dict.GetDuplicateStatus(rom, lastItem);
            Assert.Equal((DupeType)0x00, actual);
        }

        [Fact]
        public void GetDuplicateStatus_MismatchedHashes_NoDupe()
        {
            var dict = new ItemDictionary();

            Machine? machineA = new Machine();
            machineA.SetName("name-same");

            Machine? machineB = new Machine();
            machineB.SetName("name-same");

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "BEEFDEAD");
            romA.SetFieldValue<Source?>(DatItem.SourceKey, new Source(0));
            romA.CopyMachineInformation(machineA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            romB.SetFieldValue<Source?>(DatItem.SourceKey, new Source(1));
            romB.CopyMachineInformation(machineB);

            var actual = dict.GetDuplicateStatus(romA, romB);
            Assert.Equal((DupeType)0x00, actual);
        }

        [Fact]
        public void GetDuplicateStatus_DifferentSource_NameMatch_ExternalAll()
        {
            var dict = new ItemDictionary();

            Machine? machineA = new Machine();
            machineA.SetName("name-same");

            Machine? machineB = new Machine();
            machineB.SetName("name-same");

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            romA.SetFieldValue<Source?>(DatItem.SourceKey, new Source(0));
            romA.CopyMachineInformation(machineA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            romB.SetFieldValue<Source?>(DatItem.SourceKey, new Source(1));
            romB.CopyMachineInformation(machineB);

            var actual = dict.GetDuplicateStatus(romA, romB);
            Assert.Equal(DupeType.External | DupeType.All, actual);
        }

        [Fact]
        public void GetDuplicateStatus_DifferentSource_NoNameMatch_ExternalHash()
        {
            var dict = new ItemDictionary();

            Machine? machineA = new Machine();
            machineA.SetName("name-same");

            Machine? machineB = new Machine();
            machineB.SetName("not-name-same");

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            romA.SetFieldValue<Source?>(DatItem.SourceKey, new Source(0));
            romA.CopyMachineInformation(machineA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            romB.SetFieldValue<Source?>(DatItem.SourceKey, new Source(1));
            romB.CopyMachineInformation(machineB);

            var actual = dict.GetDuplicateStatus(romA, romB);
            Assert.Equal(DupeType.External | DupeType.Hash, actual);
        }

        [Fact]
        public void GetDuplicateStatus_SameSource_NameMatch_InternalAll()
        {
            var dict = new ItemDictionary();

            Machine? machineA = new Machine();
            machineA.SetName("name-same");

            Machine? machineB = new Machine();
            machineB.SetName("name-same");

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            romA.SetFieldValue<Source?>(DatItem.SourceKey, new Source(0));
            romA.CopyMachineInformation(machineA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            romB.SetFieldValue<Source?>(DatItem.SourceKey, new Source(0));
            romB.CopyMachineInformation(machineB);

            var actual = dict.GetDuplicateStatus(romA, romB);
            Assert.Equal(DupeType.Internal | DupeType.All, actual);
        }

        [Fact]
        public void GetDuplicateStatus_SameSource_NoNameMatch_InternalHash()
        {
            var dict = new ItemDictionary();

            Machine? machineA = new Machine();
            machineA.SetName("name-same");

            Machine? machineB = new Machine();
            machineB.SetName("not-name-same");

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            romA.SetFieldValue<Source?>(DatItem.SourceKey, new Source(0));
            romA.CopyMachineInformation(machineA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            romB.SetFieldValue<Source?>(DatItem.SourceKey, new Source(0));
            romB.CopyMachineInformation(machineB);

            var actual = dict.GetDuplicateStatus(romA, romB);
            Assert.Equal(DupeType.Internal | DupeType.Hash, actual);
        }

        #endregion

        #region GetDuplicates

        [Theory]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public void GetDuplicatesTest(bool hasDuplicate, int expected)
        {
            // Setup the items
            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom1.CopyMachineInformation(machine);

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "000000e948edcb4f7704b8af85a77a3339ecce44");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom2.CopyMachineInformation(machine);

            // Setup the dictionary
            var dict = new ItemDictionary();
            dict.AddItem(rom1, statsOnly: false);
            dict.AddItem(rom2, statsOnly: false);

            // Setup the test item
            DatItem rom = new Rom();
            rom.SetName("rom-1");
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, hasDuplicate ? "1024" : "2048");
            rom.CopyMachineInformation(machine);

            var actual = dict.GetDuplicates(rom);
            Assert.Equal(expected, actual.Count);
        }

        #endregion

        #region HasDuplicates

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void HasDuplicatesTest(bool expected)
        {
            // Setup the items
            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom1.CopyMachineInformation(machine);

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "000000e948edcb4f7704b8af85a77a3339ecce44");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");
            rom1.CopyMachineInformation(machine);

            // Setup the dictionary
            var dict = new ItemDictionary();
            dict.AddItem("game-1", rom1);
            dict.AddItem("game-1", rom2);

            // Setup the test item
            DatItem rom = new Rom();
            rom.SetName("rom-1");
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, expected ? "1024" : "2048");
            rom1.CopyMachineInformation(machine);

            bool actual = dict.HasDuplicates(rom);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region RecalculateStats

        [Fact]
        public void RecalculateStatsTest()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("machine");

            DatItem item = new Rom();
            item.SetName("rom");
            item.SetFieldValue<long?>(Data.Models.Metadata.Rom.SizeKey, 12345);
            item.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            item.SetFieldValue<Source?>(DatItem.SourceKey, source);
            item.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            var dict = new ItemDictionary();
            _ = dict.AddItem(item, statsOnly: false);

            Assert.Equal(1, dict.DatStatistics.TotalCount);
            Assert.Equal(1, dict.DatStatistics.GetItemCount(ItemType.Rom));
            Assert.Equal(12345, dict.DatStatistics.TotalSize);
            Assert.Equal(1, dict.DatStatistics.GetHashCount(HashType.CRC32));
            Assert.Equal(0, dict.DatStatistics.GetHashCount(HashType.MD5));

            item.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD5Key, "deadbeef");

            dict.RecalculateStats();

            Assert.Equal(1, dict.DatStatistics.TotalCount);
            Assert.Equal(1, dict.DatStatistics.GetItemCount(ItemType.Rom));
            Assert.Equal(12345, dict.DatStatistics.TotalSize);
            Assert.Equal(1, dict.DatStatistics.GetHashCount(HashType.CRC32));
            Assert.Equal(1, dict.DatStatistics.GetHashCount(HashType.MD5));
        }

        #endregion
    }
}