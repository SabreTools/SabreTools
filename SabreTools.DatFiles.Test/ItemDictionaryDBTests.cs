using System.Collections.Generic;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using Xunit;

namespace SabreTools.DatFiles.Test
{
    public class ItemDictionaryDBTests
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(disk, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(disk, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(file, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(file, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(media, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(media, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(rom, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(rom, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(rom, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(rom, machineIndex, sourceIndex, statsOnly: false);

            DatItem actual = Assert.Single(dict.GetItemsForBucket("default")).Value;
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: true);

            Assert.Empty(dict.GetItemsForBucket("default"));
        }

        [Fact]
        public void AddItem_NormalAdd()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();

            DatItem item = new Rom();
            item.SetName("item");

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: false);

            Assert.Single(dict.GetItemsForBucket("default"));
        }

        #endregion

        #region AddMachine

        [Fact]
        public void AddMachineTest()
        {
            Machine machine = new Machine();
            var dict = new ItemDictionaryDB();
            long machineIndex = dict.AddMachine(machine);

            Assert.Equal(0, machineIndex);
            Assert.Single(dict.GetMachines());
        }

        #endregion

        #region AddSource

        [Fact]
        public void AddSourceTest()
        {
            Source source = new Source(0, source: null);
            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);

            Assert.Equal(0, sourceIndex);
            Assert.Single(dict.GetSources());
        }

        #endregion

        #region ClearMarked

        [Fact]
        public void ClearMarkedTest()
        {
            // Setup the items
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            rom2.SetFieldValue<bool?>(DatItem.RemoveKey, true);
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "000000e948edcb4f7704b8af85a77a3339ecce44");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            // Setup the dictionary
            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            dict.AddItem(rom1, machineIndex, sourceIndex, statsOnly: false);
            dict.AddItem(rom2, machineIndex, sourceIndex, statsOnly: false);

            dict.ClearMarked();
            string key = Assert.Single(dict.SortedKeys);
            Assert.Equal("game-1", key);
            Dictionary<long, DatItem> items = dict.GetItemsForBucket(key);
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: false);

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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: false);

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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: false);

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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: false);

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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: false);

            var actual = dict.GetItemsForBucket("machine", filter: false);

            Assert.Single(actual);
        }

        #endregion

        #region GetMachine

        [Fact]
        public void GetMachineTest()
        {
            Machine machine = new Machine();
            var dict = new ItemDictionaryDB();
            long machineIndex = dict.AddMachine(machine);

            Assert.Equal(0, machineIndex);
            var actual = dict.GetMachine(machineIndex);
            Assert.NotNull(actual);
        }

        #endregion

        #region GetMachineForItem

        [Fact]
        public void GetMachineForItemTest()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();
            DatItem item = new Rom();

            var dict = new ItemDictionaryDB();
            long machineIndex = dict.AddMachine(machine);
            long sourceIndex = dict.AddSource(source);
            long itemIndex = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: false);

            var actual = dict.GetMachineForItem(itemIndex);
            Assert.Equal(0, actual.Key);
            Assert.NotNull(actual.Value);
        }

        #endregion

        #region GetSource

        [Fact]
        public void GetSourceTest()
        {
            Source source = new Source(0, source: null);
            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);

            Assert.Equal(0, sourceIndex);
            var actual = dict.GetSource(sourceIndex);
            Assert.NotNull(actual);
        }

        #endregion

        #region GetSourceForItem

        [Fact]
        public void GetSourceForItemTest()
        {
            Source source = new Source(0, source: null);
            Machine machine = new Machine();
            DatItem item = new Rom();

            var dict = new ItemDictionaryDB();
            long machineIndex = dict.AddMachine(machine);
            long sourceIndex = dict.AddSource(source);
            long itemIndex = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: false);

            var actual = dict.GetSourceForItem(itemIndex);
            Assert.Equal(0, actual.Key);
            Assert.NotNull(actual.Value);
        }

        #endregion

        #region RemapDatItemToMachine

        [Fact]
        public void RemapDatItemToMachineTest()
        {
            Source source = new Source(0, source: null);

            Machine origMachine = new Machine();
            origMachine.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "original");

            Machine newMachine = new Machine();
            newMachine.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "new");

            DatItem datItem = new Rom();

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long origMachineIndex = dict.AddMachine(origMachine);
            long newMachineIndex = dict.AddMachine(newMachine);
            long itemIndex = dict.AddItem(datItem, origMachineIndex, sourceIndex, statsOnly: false);

            dict.RemapDatItemToMachine(itemIndex, newMachineIndex);

            var actual = dict.GetMachineForItem(itemIndex);
            Assert.Equal(1, actual.Key);
            Assert.NotNull(actual.Value);
            Assert.Equal("new", actual.Value.GetName());
        }

        #endregion

        #region RemoveBucket

        [Fact]
        public void RemoveBucketTest()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem datItem = new Rom();
            datItem.SetName("rom-1");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            dict.AddItem(datItem, machineIndex, sourceIndex, statsOnly: false);

            dict.RemoveBucket("game-1");

            Assert.Empty(dict.GetItemsForBucket("game-1"));
        }

        #endregion

        #region RemoveItem

        [Fact]
        public void RemoveItemTest()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem datItem = new Rom();
            datItem.SetName("rom-1");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            long itemIndex = dict.AddItem(datItem, machineIndex, sourceIndex, statsOnly: false);

            dict.RemoveItem(itemIndex);

            Assert.Empty(dict.GetItemsForBucket("game-1"));
        }

        #endregion

        #region RemoveMachine

        [Fact]
        public void RemoveMachineTest()
        {
            Machine machine = new Machine();
            var dict = new ItemDictionaryDB();
            long machineIndex = dict.AddMachine(machine);

            bool actual = dict.RemoveMachine(machineIndex);
            Assert.True(actual);
            Assert.Empty(dict.GetMachines());
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
            Source source = new Source(0, source: null);

            Machine machine1 = new Machine();
            machine1.SetName("game-1");

            Machine machine2 = new Machine();
            machine2.SetName("game-2");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "000000e948edcb4f7704b8af85a77a3339ecce44");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            DatItem rom3 = new Rom();
            rom3.SetName("rom-3");
            rom3.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            rom3.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "00000ea4014ce66679e7e17d56ac510f67e39e26");
            rom3.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            DatItem rom4 = new Rom();
            rom4.SetName("rom-4");
            rom4.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            rom4.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "00000151d437442e74e5134023fab8bf694a2487");
            rom4.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            // Setup the dictionary
            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machine1Index = dict.AddMachine(machine1);
            long machine2Index = dict.AddMachine(machine2);
            dict.AddItem(rom1, machine1Index, sourceIndex, statsOnly: false);
            dict.AddItem(rom2, machine1Index, sourceIndex, statsOnly: false);
            dict.AddItem(rom3, machine2Index, sourceIndex, statsOnly: false);
            dict.AddItem(rom4, machine2Index, sourceIndex, statsOnly: false);

            dict.BucketBy(itemKey);
            Assert.Equal(expected, dict.SortedKeys.Length);
        }

        #endregion

        #region Deduplicate

        [Fact]
        public void DeduplicateTest()
        {
            // Setup the items
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            // Setup the dictionary
            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            dict.AddItem(rom1, machineIndex, sourceIndex, statsOnly: false);
            dict.AddItem(rom2, machineIndex, sourceIndex, statsOnly: false);

            dict.Deduplicate();
            Assert.Equal(1, dict.DatStatistics.TotalCount);
        }

        #endregion

        #region GetDuplicateStatus

        [Fact]
        public void GetDuplicateStatus_NullOther_NoDupe()
        {
            var dict = new ItemDictionaryDB();

            Source? selfSource = null;
            Source? lastSource = null;

            KeyValuePair<long, DatItem>? item = new KeyValuePair<long, DatItem>(0, new Rom());
            KeyValuePair<long, DatItem>? lastItem = null;

            var actual = dict.GetDuplicateStatus(item, selfSource, lastItem, lastSource);
            Assert.Equal((DupeType)0x00, actual);
        }

        [Fact]
        public void GetDuplicateStatus_DifferentTypes_NoDupe()
        {
            var dict = new ItemDictionaryDB();

            Source? selfSource = null;
            Source? lastSource = null;

            KeyValuePair<long, DatItem>? rom = new KeyValuePair<long, DatItem>(0, new Rom());
            KeyValuePair<long, DatItem>? lastItem = new KeyValuePair<long, DatItem>(1, new Disk());
            var actual = dict.GetDuplicateStatus(rom, selfSource, lastItem, lastSource);
            Assert.Equal((DupeType)0x00, actual);
        }

        [Fact]
        public void GetDuplicateStatus_MismatchedHashes_NoDupe()
        {
            var dict = new ItemDictionaryDB();

            Source? sourceA = new Source(0);
            long sourceAIndex = dict.AddSource(sourceA);
            Source? sourceB = new Source(1);
            long sourceBIndex = dict.AddSource(sourceB);

            Machine? machineA = new Machine();
            machineA.SetName("name-same");
            long machineAIndex = dict.AddMachine(machineA);

            Machine? machineB = new Machine();
            machineB.SetName("name-same");
            long machineBIndex = dict.AddMachine(machineB);

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "BEEFDEAD");
            long romAIndex = dict.AddItem(romA, machineAIndex, sourceAIndex);
            KeyValuePair<long, DatItem>? romAPair = new KeyValuePair<long, DatItem>(romAIndex, romA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            long romBIndex = dict.AddItem(romB, machineBIndex, sourceBIndex);
            KeyValuePair<long, DatItem>? romBPair = new KeyValuePair<long, DatItem>(romBIndex, romB);

            var actual = dict.GetDuplicateStatus(romAPair, sourceA, romBPair, sourceB);
            Assert.Equal((DupeType)0x00, actual);
        }

        [Fact]
        public void GetDuplicateStatus_DifferentSource_NameMatch_ExternalAll()
        {
            var dict = new ItemDictionaryDB();

            Source? sourceA = new Source(0);
            long sourceAIndex = dict.AddSource(sourceA);
            Source? sourceB = new Source(1);
            long sourceBIndex = dict.AddSource(sourceB);

            Machine? machineA = new Machine();
            machineA.SetName("name-same");
            long machineAIndex = dict.AddMachine(machineA);

            Machine? machineB = new Machine();
            machineB.SetName("name-same");
            long machineBIndex = dict.AddMachine(machineB);

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            long romAIndex = dict.AddItem(romA, machineAIndex, sourceAIndex);
            KeyValuePair<long, DatItem>? romAPair = new KeyValuePair<long, DatItem>(romAIndex, romA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            long romBIndex = dict.AddItem(romB, machineBIndex, sourceBIndex);
            KeyValuePair<long, DatItem>? romBPair = new KeyValuePair<long, DatItem>(romBIndex, romB);

            var actual = dict.GetDuplicateStatus(romAPair, sourceA, romBPair, sourceB);
            Assert.Equal(DupeType.External | DupeType.All, actual);
        }

        [Fact]
        public void GetDuplicateStatus_DifferentSource_NoNameMatch_ExternalHash()
        {
            var dict = new ItemDictionaryDB();

            Source? sourceA = new Source(0);
            long sourceAIndex = dict.AddSource(sourceA);
            Source? sourceB = new Source(1);
            long sourceBIndex = dict.AddSource(sourceB);

            Machine? machineA = new Machine();
            machineA.SetName("name-same");
            long machineAIndex = dict.AddMachine(machineA);

            Machine? machineB = new Machine();
            machineB.SetName("not-name-same");
            long machineBIndex = dict.AddMachine(machineB);

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            long romAIndex = dict.AddItem(romA, machineAIndex, sourceAIndex);
            KeyValuePair<long, DatItem>? romAPair = new KeyValuePair<long, DatItem>(romAIndex, romA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            long romBIndex = dict.AddItem(romB, machineBIndex, sourceBIndex);
            KeyValuePair<long, DatItem>? romBPair = new KeyValuePair<long, DatItem>(romBIndex, romB);

            var actual = dict.GetDuplicateStatus(romAPair, sourceA, romBPair, sourceB);
            Assert.Equal(DupeType.External | DupeType.Hash, actual);
        }

        [Fact]
        public void GetDuplicateStatus_SameSource_NameMatch_InternalAll()
        {
            var dict = new ItemDictionaryDB();

            Source? sourceA = new Source(0);
            long sourceAIndex = dict.AddSource(sourceA);
            Source? sourceB = new Source(0);
            long sourceBIndex = dict.AddSource(sourceB);

            Machine? machineA = new Machine();
            machineA.SetName("name-same");
            long machineAIndex = dict.AddMachine(machineA);

            Machine? machineB = new Machine();
            machineB.SetName("name-same");
            long machineBIndex = dict.AddMachine(machineB);

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            long romAIndex = dict.AddItem(romA, machineAIndex, sourceAIndex);
            KeyValuePair<long, DatItem>? romAPair = new KeyValuePair<long, DatItem>(romAIndex, romA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            long romBIndex = dict.AddItem(romB, machineBIndex, sourceBIndex);
            KeyValuePair<long, DatItem>? romBPair = new KeyValuePair<long, DatItem>(romBIndex, romB);

            var actual = dict.GetDuplicateStatus(romAPair, sourceA, romBPair, sourceB);
            Assert.Equal(DupeType.Internal | DupeType.All, actual);
        }

        [Fact]
        public void GetDuplicateStatus_SameSource_NoNameMatch_InternalHash()
        {
            var dict = new ItemDictionaryDB();

            Source? sourceA = new Source(0);
            long sourceAIndex = dict.AddSource(sourceA);
            Source? sourceB = new Source(0);
            long sourceBIndex = dict.AddSource(sourceB);

            Machine? machineA = new Machine();
            machineA.SetName("name-same");
            long machineAIndex = dict.AddMachine(machineA);

            Machine? machineB = new Machine();
            machineB.SetName("not-name-same");
            long machineBIndex = dict.AddMachine(machineB);

            var romA = new Rom();
            romA.SetName("same-name");
            romA.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            long romAIndex = dict.AddItem(romA, machineAIndex, sourceAIndex);
            KeyValuePair<long, DatItem>? romAPair = new KeyValuePair<long, DatItem>(romAIndex, romA);

            var romB = new Rom();
            romB.SetName("same-name");
            romB.SetFieldValue(Data.Models.Metadata.Rom.CRCKey, "DEADBEEF");
            long romBIndex = dict.AddItem(romB, machineBIndex, sourceBIndex);
            KeyValuePair<long, DatItem>? romBPair = new KeyValuePair<long, DatItem>(romBIndex, romB);

            var actual = dict.GetDuplicateStatus(romAPair, sourceA, romBPair, sourceB);
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
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "000000e948edcb4f7704b8af85a77a3339ecce44");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            // Setup the dictionary
            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            dict.AddItem(rom1, machineIndex, sourceIndex, statsOnly: false);
            dict.AddItem(rom2, machineIndex, sourceIndex, statsOnly: false);

            // Setup the test item
            DatItem rom = new Rom();
            rom.SetName("rom-1");
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, hasDuplicate ? "1024" : "2048");

            var actual = dict.GetDuplicates(new KeyValuePair<long, DatItem>(-1, rom));
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
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem rom1 = new Rom();
            rom1.SetName("rom-1");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom1.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            DatItem rom2 = new Rom();
            rom2.SetName("rom-2");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "000000e948edcb4f7704b8af85a77a3339ecce44");
            rom2.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, "1024");

            // Setup the dictionary
            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            dict.AddItem(rom1, machineIndex, sourceIndex, statsOnly: false);
            dict.AddItem(rom2, machineIndex, sourceIndex, statsOnly: false);

            // Setup the test item
            DatItem rom = new Rom();
            rom.SetName("rom-1");
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "0000000fbbb37f8488100b1b4697012de631a5e6");
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SizeKey, expected ? "1024" : "2048");

            bool actual = dict.HasDuplicates(new KeyValuePair<long, DatItem>(-1, rom));
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

            var dict = new ItemDictionaryDB();
            long sourceIndex = dict.AddSource(source);
            long machineIndex = dict.AddMachine(machine);
            _ = dict.AddItem(item, machineIndex, sourceIndex, statsOnly: false);

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