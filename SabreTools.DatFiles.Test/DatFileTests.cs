using System.Collections.Generic;
using System.IO;
using SabreTools.DatFiles.Formats;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using Xunit;

namespace SabreTools.DatFiles.Test
{
    public partial class DatFileTests
    {
        #region Constructor

        [Fact]
        public void Constructor_Null()
        {
            DatFile? datFile = null;
            DatFile created = new Formats.Logiqx(datFile, useGame: false);

            Assert.NotNull(created.Header);
            Assert.NotNull(created.Items);
            Assert.Equal(0, created.Items.DatStatistics.TotalCount);
            Assert.NotNull(created.ItemsDB);
            Assert.Equal(0, created.ItemsDB.DatStatistics.TotalCount);
        }

        [Fact]
        public void Constructor_NonNull()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("key");

            DatItem rom = new Rom();
            rom.SetName("rom");
            rom.SetFieldValue(Models.Metadata.Rom.CRCKey, "deadbeef");
            rom.SetFieldValue(DatItem.SourceKey, source);
            rom.SetFieldValue(DatItem.MachineKey, machine);

            DatFile? datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, "name");
            datFile.AddItem(rom, statsOnly: false);

            long sourceIndex = datFile.AddSourceDB(source);
            long machineIndex = datFile.AddMachineDB(machine);
            datFile.AddItemDB(rom, machineIndex, sourceIndex, statsOnly: false);

            DatFile created = new Formats.Logiqx(datFile, useGame: false);
            created.BucketBy(ItemKey.Machine);

            Assert.NotNull(created.Header);
            Assert.Equal("name", created.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));

            Assert.NotNull(created.Items);
            DatItem datItem = Assert.Single(created.GetItemsForBucket("key"));
            Assert.True(datItem is Rom);

            Assert.NotNull(created.ItemsDB);
            KeyValuePair<long, DatItem> dbKvp = Assert.Single(created.GetItemsForBucketDB("key"));
            Assert.Equal(0, dbKvp.Key);
            Assert.True(dbKvp.Value is Rom);
        }

        #endregion

        #region ClearEmpty

        [Fact]
        public void ClearEmpty_Items()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem datItem = new Rom();
            datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
            datItem.SetFieldValue<Machine?>(DatItem.MachineKey, machine);

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            datFile.AddItem(datItem, statsOnly: false);

            datFile.ClearEmpty();
            Assert.Single(datFile.Items.SortedKeys);
        }

        [Fact]
        public void ClearEmpty_ItemsDB()
        {
            Source source = new Source(0, source: null);

            Machine machine = new Machine();
            machine.SetName("game-1");

            DatItem datItem = new Rom();

            DatFile datFile = new Logiqx(datFile: null, useGame: false);
            long sourceIndex = datFile.AddSourceDB(source);
            long machineIndex = datFile.AddMachineDB(machine);
            _ = datFile.AddItemDB(datItem, machineIndex, sourceIndex, statsOnly: false);

            datFile.ClearEmpty();
            Assert.Single(datFile.ItemsDB.SortedKeys);
        }

        #endregion

        #region FillHeaderFromPath

        [Fact]
        public void FillHeaderFromPath_NoNameNoDesc_NotBare()
        {
            DatFile datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, string.Empty);
            datFile.Header.SetFieldValue(Models.Metadata.Header.DescriptionKey, string.Empty);
            datFile.Header.SetFieldValue(Models.Metadata.Header.DateKey, "1980-01-01");

            string path = Path.Combine("Fake", "Path", "Filename");
            datFile.FillHeaderFromPath(path, false);

            Assert.Equal("Filename (1980-01-01)", datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));
            Assert.Equal("Filename (1980-01-01)", datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
        }

        [Fact]
        public void FillHeaderFromPath_NoNameNoDesc_Bare()
        {
            DatFile datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, string.Empty);
            datFile.Header.SetFieldValue(Models.Metadata.Header.DescriptionKey, string.Empty);
            datFile.Header.SetFieldValue(Models.Metadata.Header.DateKey, "1980-01-01");

            string path = Path.Combine("Fake", "Path", "Filename");
            datFile.FillHeaderFromPath(path, true);

            Assert.Equal("Filename", datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));
            Assert.Equal("Filename", datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
        }

        [Fact]
        public void FillHeaderFromPath_NoNameDesc_NotBare()
        {
            DatFile datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, string.Empty);
            datFile.Header.SetFieldValue(Models.Metadata.Header.DescriptionKey, "Description");
            datFile.Header.SetFieldValue(Models.Metadata.Header.DateKey, "1980-01-01");

            string path = Path.Combine("Fake", "Path", "Filename");
            datFile.FillHeaderFromPath(path, false);

            Assert.Equal("Description (1980-01-01)", datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));
            Assert.Equal("Description", datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
        }

        [Fact]
        public void FillHeaderFromPath_NoNameDesc_Bare()
        {
            DatFile datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, string.Empty);
            datFile.Header.SetFieldValue(Models.Metadata.Header.DescriptionKey, "Description");
            datFile.Header.SetFieldValue(Models.Metadata.Header.DateKey, "1980-01-01");

            string path = Path.Combine("Fake", "Path", "Filename");
            datFile.FillHeaderFromPath(path, true);

            Assert.Equal("Description", datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));
            Assert.Equal("Description", datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
        }

        [Fact]
        public void FillHeaderFromPath_NameNoDesc_NotBare()
        {
            DatFile datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, "Name");
            datFile.Header.SetFieldValue(Models.Metadata.Header.DescriptionKey, string.Empty);
            datFile.Header.SetFieldValue(Models.Metadata.Header.DateKey, "1980-01-01");

            string path = Path.Combine("Fake", "Path", "Filename");
            datFile.FillHeaderFromPath(path, false);

            Assert.Equal("Name", datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));
            Assert.Equal("Name (1980-01-01)", datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
        }

        [Fact]
        public void FillHeaderFromPath_NameNoDesc_Bare()
        {
            DatFile datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, "Name");
            datFile.Header.SetFieldValue(Models.Metadata.Header.DescriptionKey, string.Empty);
            datFile.Header.SetFieldValue(Models.Metadata.Header.DateKey, "1980-01-01");

            string path = Path.Combine("Fake", "Path", "Filename");
            datFile.FillHeaderFromPath(path, true);

            Assert.Equal("Name", datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));
            Assert.Equal("Name", datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
        }

        [Fact]
        public void FillHeaderFromPath_NameDesc_NotBare()
        {
            DatFile datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, "Name");
            datFile.Header.SetFieldValue(Models.Metadata.Header.DescriptionKey, "Description");
            datFile.Header.SetFieldValue(Models.Metadata.Header.DateKey, "1980-01-01");

            string path = Path.Combine("Fake", "Path", "Filename");
            datFile.FillHeaderFromPath(path, false);

            Assert.Equal("Name", datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));
            Assert.Equal("Description", datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
        }

        [Fact]
        public void FillHeaderFromPath_NameDesc_Bare()
        {
            DatFile datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, "Name ");
            datFile.Header.SetFieldValue(Models.Metadata.Header.DescriptionKey, "Description ");
            datFile.Header.SetFieldValue(Models.Metadata.Header.DateKey, "1980-01-01");

            string path = Path.Combine("Fake", "Path", "Filename");
            datFile.FillHeaderFromPath(path, true);

            Assert.Equal("Name", datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));
            Assert.Equal("Description", datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
        }

        #endregion

        #region SetHeader

        [Fact]
        public void SetHeaderTest()
        {
            DatHeader datHeader = new DatHeader();
            datHeader.SetFieldValue(Models.Metadata.Header.NameKey, "name");

            DatFile? datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, "notname");

            datFile.SetHeader(datHeader);
            Assert.NotNull(datFile.Header);
            Assert.Equal("name", datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey));
        }

        #endregion

        #region SetModifiers

        [Fact]
        public void SetModifiersTest()
        {
            DatModifiers datModifiers = new DatModifiers();
            datModifiers.AddExtension = ".new";

            DatFile? datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Modifiers.AddExtension = ".old";

            datFile.SetModifiers(datModifiers);
            Assert.NotNull(datFile.Modifiers);
            Assert.Equal(".new", datFile.Modifiers.AddExtension);
        }

        #endregion

        #region ResetDictionary

        [Fact]
        public void ResetDictionaryTest()
        {
            DatFile datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Header.SetFieldValue(Models.Metadata.Header.NameKey, "name");
            datFile.AddItem(new Rom(), statsOnly: false);
            datFile.AddItemDB(new Rom(), 0, 0, false);

            datFile.ResetDictionary();

            Assert.NotNull(datFile.Header);
            Assert.NotNull(datFile.Items);
            Assert.Equal(0, datFile.Items.DatStatistics.TotalCount);
            Assert.NotNull(datFile.ItemsDB);
            Assert.Equal(0, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        #endregion

        #region ProcessItemName

        [Theory]
        [InlineData(false, false, false, false, null, false, null, false, null, false, "name")]
        [InlineData(false, false, false, false, null, false, null, false, null, true, "name")]
        [InlineData(false, false, false, false, null, false, null, false, "add", false, "name")]
        [InlineData(false, false, false, false, null, false, null, false, "add", true, "name")]
        [InlineData(false, false, false, false, null, false, null, true, null, false, "name")]
        [InlineData(false, false, false, false, null, false, null, true, null, true, "name")]
        [InlineData(false, false, false, false, null, false, null, true, "add", false, "name")]
        [InlineData(false, false, false, false, null, false, null, true, "add", true, "name")]
        [InlineData(false, false, false, false, null, false, "rep", false, null, false, "name")]
        [InlineData(false, false, false, false, null, false, "rep", false, null, true, "name")]
        [InlineData(false, false, false, false, null, false, "rep", false, "add", false, "name")]
        [InlineData(false, false, false, false, null, false, "rep", false, "add", true, "name")]
        [InlineData(false, false, false, false, null, false, "rep", true, null, false, "name")]
        [InlineData(false, false, false, false, null, false, "rep", true, null, true, "name")]
        [InlineData(false, false, false, false, null, false, "rep", true, "add", false, "name")]
        [InlineData(false, false, false, false, null, false, "rep", true, "add", true, "name")]
        [InlineData(false, false, false, false, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, null, false, null, false, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, null, false, null, true, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, null, false, "add", false, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, null, false, "add", true, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, null, true, null, false, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, null, true, null, true, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, null, true, "add", false, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, null, true, "add", true, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, "rep", false, null, false, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, "rep", false, null, true, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, "rep", false, "add", false, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, "rep", false, "add", true, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, "rep", true, null, false, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, "rep", true, null, true, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, "rep", true, "add", false, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", false, "rep", true, "add", true, "name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, false, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, null, false, null, false, null, false, "name")]
        [InlineData(false, false, false, true, null, false, null, false, null, true, "machine/name")]
        [InlineData(false, false, false, true, null, false, null, false, "add", false, "nameadd")]
        [InlineData(false, false, false, true, null, false, null, false, "add", true, "machine/nameadd")]
        [InlineData(false, false, false, true, null, false, null, true, null, false, "name")]
        [InlineData(false, false, false, true, null, false, null, true, null, true, "machine/name")]
        [InlineData(false, false, false, true, null, false, null, true, "add", false, "nameadd")]
        [InlineData(false, false, false, true, null, false, null, true, "add", true, "machine/nameadd")]
        [InlineData(false, false, false, true, null, false, "rep", false, null, false, "namerep")]
        [InlineData(false, false, false, true, null, false, "rep", false, null, true, "machine/namerep")]
        [InlineData(false, false, false, true, null, false, "rep", false, "add", false, "namerepadd")]
        [InlineData(false, false, false, true, null, false, "rep", false, "add", true, "machine/namerepadd")]
        [InlineData(false, false, false, true, null, false, "rep", true, null, false, "name")]
        [InlineData(false, false, false, true, null, false, "rep", true, null, true, "machine/name")]
        [InlineData(false, false, false, true, null, false, "rep", true, "add", false, "nameadd")]
        [InlineData(false, false, false, true, null, false, "rep", true, "add", true, "machine/nameadd")]
        [InlineData(false, false, false, true, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, null, false, null, false, "machine_namenamemachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, null, false, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, null, false, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, null, false, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, null, true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, null, true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, null, true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, null, true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, "rep", false, null, false, "machine_namenamerepmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, "rep", false, null, true, "machine_namemachine/namerepmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, "rep", false, "add", false, "machine_namenamerepaddmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, "rep", false, "add", true, "machine_namemachine/namerepaddmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, "rep", true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, "rep", true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, "rep", true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", false, "rep", true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, false, true, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, null, false, null, false, null, false, "name")]
        [InlineData(false, false, true, false, null, false, null, false, null, true, "name")]
        [InlineData(false, false, true, false, null, false, null, false, "add", false, "name")]
        [InlineData(false, false, true, false, null, false, null, false, "add", true, "name")]
        [InlineData(false, false, true, false, null, false, null, true, null, false, "name")]
        [InlineData(false, false, true, false, null, false, null, true, null, true, "name")]
        [InlineData(false, false, true, false, null, false, null, true, "add", false, "name")]
        [InlineData(false, false, true, false, null, false, null, true, "add", true, "name")]
        [InlineData(false, false, true, false, null, false, "rep", false, null, false, "name")]
        [InlineData(false, false, true, false, null, false, "rep", false, null, true, "name")]
        [InlineData(false, false, true, false, null, false, "rep", false, "add", false, "name")]
        [InlineData(false, false, true, false, null, false, "rep", false, "add", true, "name")]
        [InlineData(false, false, true, false, null, false, "rep", true, null, false, "name")]
        [InlineData(false, false, true, false, null, false, "rep", true, null, true, "name")]
        [InlineData(false, false, true, false, null, false, "rep", true, "add", false, "name")]
        [InlineData(false, false, true, false, null, false, "rep", true, "add", true, "name")]
        [InlineData(false, false, true, false, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, null, false, null, false, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, null, false, null, true, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, null, false, "add", false, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, null, false, "add", true, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, null, true, null, false, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, null, true, null, true, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, null, true, "add", false, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, null, true, "add", true, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, "rep", false, null, false, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, "rep", false, null, true, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, "rep", false, "add", false, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, "rep", false, "add", true, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, "rep", true, null, false, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, "rep", true, null, true, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, "rep", true, "add", false, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", false, "rep", true, "add", true, "name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, false, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, null, false, null, false, null, false, "name")]
        [InlineData(false, false, true, true, null, false, null, false, null, true, "machine/name")]
        [InlineData(false, false, true, true, null, false, null, false, "add", false, "nameadd")]
        [InlineData(false, false, true, true, null, false, null, false, "add", true, "machine/nameadd")]
        [InlineData(false, false, true, true, null, false, null, true, null, false, "name")]
        [InlineData(false, false, true, true, null, false, null, true, null, true, "machine/name")]
        [InlineData(false, false, true, true, null, false, null, true, "add", false, "nameadd")]
        [InlineData(false, false, true, true, null, false, null, true, "add", true, "machine/nameadd")]
        [InlineData(false, false, true, true, null, false, "rep", false, null, false, "namerep")]
        [InlineData(false, false, true, true, null, false, "rep", false, null, true, "machine/namerep")]
        [InlineData(false, false, true, true, null, false, "rep", false, "add", false, "namerepadd")]
        [InlineData(false, false, true, true, null, false, "rep", false, "add", true, "machine/namerepadd")]
        [InlineData(false, false, true, true, null, false, "rep", true, null, false, "name")]
        [InlineData(false, false, true, true, null, false, "rep", true, null, true, "machine/name")]
        [InlineData(false, false, true, true, null, false, "rep", true, "add", false, "nameadd")]
        [InlineData(false, false, true, true, null, false, "rep", true, "add", true, "machine/nameadd")]
        [InlineData(false, false, true, true, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, null, false, null, false, "machine_namenamemachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, null, false, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, null, false, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, null, false, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, null, true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, null, true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, null, true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, null, true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, "rep", false, null, false, "machine_namenamerepmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, "rep", false, null, true, "machine_namemachine/namerepmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, "rep", false, "add", false, "machine_namenamerepaddmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, "rep", false, "add", true, "machine_namemachine/namerepaddmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, "rep", true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, "rep", true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, "rep", true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", false, "rep", true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, false, true, true, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, null, false, null, false, null, false, "name")]
        [InlineData(false, true, false, false, null, false, null, false, null, true, "machine/name")]
        [InlineData(false, true, false, false, null, false, null, false, "add", false, "nameadd")]
        [InlineData(false, true, false, false, null, false, null, false, "add", true, "machine/nameadd")]
        [InlineData(false, true, false, false, null, false, null, true, null, false, "name")]
        [InlineData(false, true, false, false, null, false, null, true, null, true, "machine/name")]
        [InlineData(false, true, false, false, null, false, null, true, "add", false, "nameadd")]
        [InlineData(false, true, false, false, null, false, null, true, "add", true, "machine/nameadd")]
        [InlineData(false, true, false, false, null, false, "rep", false, null, false, "namerep")]
        [InlineData(false, true, false, false, null, false, "rep", false, null, true, "machine/namerep")]
        [InlineData(false, true, false, false, null, false, "rep", false, "add", false, "namerepadd")]
        [InlineData(false, true, false, false, null, false, "rep", false, "add", true, "machine/namerepadd")]
        [InlineData(false, true, false, false, null, false, "rep", true, null, false, "name")]
        [InlineData(false, true, false, false, null, false, "rep", true, null, true, "machine/name")]
        [InlineData(false, true, false, false, null, false, "rep", true, "add", false, "nameadd")]
        [InlineData(false, true, false, false, null, false, "rep", true, "add", true, "machine/nameadd")]
        [InlineData(false, true, false, false, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, null, false, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, null, false, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, null, false, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, null, false, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, null, true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, null, true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, null, true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, null, true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, "rep", false, null, false, "machine_namenamerepmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, "rep", false, null, true, "machine_namemachine/namerepmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, "rep", false, "add", false, "machine_namenamerepaddmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, "rep", false, "add", true, "machine_namemachine/namerepaddmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, "rep", true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, "rep", true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, "rep", true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", false, "rep", true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, false, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, null, false, null, false, null, false, "name")]
        [InlineData(false, true, false, true, null, false, null, false, null, true, "machine/name")]
        [InlineData(false, true, false, true, null, false, null, false, "add", false, "nameadd")]
        [InlineData(false, true, false, true, null, false, null, false, "add", true, "machine/nameadd")]
        [InlineData(false, true, false, true, null, false, null, true, null, false, "name")]
        [InlineData(false, true, false, true, null, false, null, true, null, true, "machine/name")]
        [InlineData(false, true, false, true, null, false, null, true, "add", false, "nameadd")]
        [InlineData(false, true, false, true, null, false, null, true, "add", true, "machine/nameadd")]
        [InlineData(false, true, false, true, null, false, "rep", false, null, false, "namerep")]
        [InlineData(false, true, false, true, null, false, "rep", false, null, true, "machine/namerep")]
        [InlineData(false, true, false, true, null, false, "rep", false, "add", false, "namerepadd")]
        [InlineData(false, true, false, true, null, false, "rep", false, "add", true, "machine/namerepadd")]
        [InlineData(false, true, false, true, null, false, "rep", true, null, false, "name")]
        [InlineData(false, true, false, true, null, false, "rep", true, null, true, "machine/name")]
        [InlineData(false, true, false, true, null, false, "rep", true, "add", false, "nameadd")]
        [InlineData(false, true, false, true, null, false, "rep", true, "add", true, "machine/nameadd")]
        [InlineData(false, true, false, true, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, null, false, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, null, false, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, null, false, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, null, false, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, null, true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, null, true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, null, true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, null, true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, "rep", false, null, false, "machine_namenamerepmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, "rep", false, null, true, "machine_namemachine/namerepmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, "rep", false, "add", false, "machine_namenamerepaddmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, "rep", false, "add", true, "machine_namemachine/namerepaddmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, "rep", true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, "rep", true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, "rep", true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", false, "rep", true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, false, true, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, null, false, null, false, null, false, "name")]
        [InlineData(false, true, true, false, null, false, null, false, null, true, "machine/name")]
        [InlineData(false, true, true, false, null, false, null, false, "add", false, "nameadd")]
        [InlineData(false, true, true, false, null, false, null, false, "add", true, "machine/nameadd")]
        [InlineData(false, true, true, false, null, false, null, true, null, false, "name")]
        [InlineData(false, true, true, false, null, false, null, true, null, true, "machine/name")]
        [InlineData(false, true, true, false, null, false, null, true, "add", false, "nameadd")]
        [InlineData(false, true, true, false, null, false, null, true, "add", true, "machine/nameadd")]
        [InlineData(false, true, true, false, null, false, "rep", false, null, false, "namerep")]
        [InlineData(false, true, true, false, null, false, "rep", false, null, true, "machine/namerep")]
        [InlineData(false, true, true, false, null, false, "rep", false, "add", false, "namerepadd")]
        [InlineData(false, true, true, false, null, false, "rep", false, "add", true, "machine/namerepadd")]
        [InlineData(false, true, true, false, null, false, "rep", true, null, false, "name")]
        [InlineData(false, true, true, false, null, false, "rep", true, null, true, "machine/name")]
        [InlineData(false, true, true, false, null, false, "rep", true, "add", false, "nameadd")]
        [InlineData(false, true, true, false, null, false, "rep", true, "add", true, "machine/nameadd")]
        [InlineData(false, true, true, false, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, null, false, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, null, false, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, null, false, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, null, false, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, null, true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, null, true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, null, true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, null, true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, "rep", false, null, false, "machine_namenamerepmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, "rep", false, null, true, "machine_namemachine/namerepmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, "rep", false, "add", false, "machine_namenamerepaddmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, "rep", false, "add", true, "machine_namemachine/namerepaddmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, "rep", true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, "rep", true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, "rep", true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", false, "rep", true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, false, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, null, false, null, false, null, false, "name")]
        [InlineData(false, true, true, true, null, false, null, false, null, true, "machine/name")]
        [InlineData(false, true, true, true, null, false, null, false, "add", false, "nameadd")]
        [InlineData(false, true, true, true, null, false, null, false, "add", true, "machine/nameadd")]
        [InlineData(false, true, true, true, null, false, null, true, null, false, "name")]
        [InlineData(false, true, true, true, null, false, null, true, null, true, "machine/name")]
        [InlineData(false, true, true, true, null, false, null, true, "add", false, "nameadd")]
        [InlineData(false, true, true, true, null, false, null, true, "add", true, "machine/nameadd")]
        [InlineData(false, true, true, true, null, false, "rep", false, null, false, "namerep")]
        [InlineData(false, true, true, true, null, false, "rep", false, null, true, "machine/namerep")]
        [InlineData(false, true, true, true, null, false, "rep", false, "add", false, "namerepadd")]
        [InlineData(false, true, true, true, null, false, "rep", false, "add", true, "machine/namerepadd")]
        [InlineData(false, true, true, true, null, false, "rep", true, null, false, "name")]
        [InlineData(false, true, true, true, null, false, "rep", true, null, true, "machine/name")]
        [InlineData(false, true, true, true, null, false, "rep", true, "add", false, "nameadd")]
        [InlineData(false, true, true, true, null, false, "rep", true, "add", true, "machine/nameadd")]
        [InlineData(false, true, true, true, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, null, false, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, null, false, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, null, false, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, null, false, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, null, true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, null, true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, null, true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, null, true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, "rep", false, null, false, "machine_namenamerepmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, "rep", false, null, true, "machine_namemachine/namerepmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, "rep", false, "add", false, "machine_namenamerepaddmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, "rep", false, "add", true, "machine_namemachine/namerepaddmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, "rep", true, null, false, "machine_namenamemachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, "rep", true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, "rep", true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", false, "rep", true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(false, true, true, true, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, false, false, null, false, null, false, null, false, "name")]
        [InlineData(true, false, false, false, null, false, null, false, null, true, "name")]
        [InlineData(true, false, false, false, null, false, null, false, "add", false, "name")]
        [InlineData(true, false, false, false, null, false, null, false, "add", true, "name")]
        [InlineData(true, false, false, false, null, false, null, true, null, false, "name")]
        [InlineData(true, false, false, false, null, false, null, true, null, true, "name")]
        [InlineData(true, false, false, false, null, false, null, true, "add", false, "name")]
        [InlineData(true, false, false, false, null, false, null, true, "add", true, "name")]
        [InlineData(true, false, false, false, null, false, "rep", false, null, false, "name")]
        [InlineData(true, false, false, false, null, false, "rep", false, null, true, "name")]
        [InlineData(true, false, false, false, null, false, "rep", false, "add", false, "name")]
        [InlineData(true, false, false, false, null, false, "rep", false, "add", true, "name")]
        [InlineData(true, false, false, false, null, false, "rep", true, null, false, "name")]
        [InlineData(true, false, false, false, null, false, "rep", true, null, true, "name")]
        [InlineData(true, false, false, false, null, false, "rep", true, "add", false, "name")]
        [InlineData(true, false, false, false, null, false, "rep", true, "add", true, "name")]
        [InlineData(true, false, false, false, null, true, null, false, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, null, false, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, null, false, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, null, false, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, null, true, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, null, true, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, null, true, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, null, true, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, "rep", false, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, "rep", false, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, "rep", false, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, "rep", false, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, "rep", true, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, "rep", true, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, "rep", true, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, null, true, "rep", true, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, null, false, null, false, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, null, false, null, true, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, null, false, "add", false, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, null, false, "add", true, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, null, true, null, false, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, null, true, null, true, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, null, true, "add", false, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, null, true, "add", true, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, "rep", false, null, false, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, "rep", false, null, true, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, "rep", false, "add", false, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, "rep", false, "add", true, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, "rep", true, null, false, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, "rep", true, null, true, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, "rep", true, "add", false, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", false, "rep", true, "add", true, "name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, null, false, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, null, false, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, null, false, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, null, false, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, null, true, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, null, true, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, null, true, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, null, true, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, "rep", false, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, "rep", false, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, "rep", false, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, "rep", false, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, "rep", true, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, "rep", true, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, "rep", true, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, false, "%machine%_%name%", true, "rep", true, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, null, false, null, false, null, false, "\"name\"")]
        [InlineData(true, false, false, true, null, false, null, false, null, true, "\"machine/name\"")]
        [InlineData(true, false, false, true, null, false, null, false, "add", false, "\"nameadd\"")]
        [InlineData(true, false, false, true, null, false, null, false, "add", true, "\"machine/nameadd\"")]
        [InlineData(true, false, false, true, null, false, null, true, null, false, "\"name\"")]
        [InlineData(true, false, false, true, null, false, null, true, null, true, "\"machine/name\"")]
        [InlineData(true, false, false, true, null, false, null, true, "add", false, "\"nameadd\"")]
        [InlineData(true, false, false, true, null, false, null, true, "add", true, "\"machine/nameadd\"")]
        [InlineData(true, false, false, true, null, false, "rep", false, null, false, "\"namerep\"")]
        [InlineData(true, false, false, true, null, false, "rep", false, null, true, "\"machine/namerep\"")]
        [InlineData(true, false, false, true, null, false, "rep", false, "add", false, "\"namerepadd\"")]
        [InlineData(true, false, false, true, null, false, "rep", false, "add", true, "\"machine/namerepadd\"")]
        [InlineData(true, false, false, true, null, false, "rep", true, null, false, "\"name\"")]
        [InlineData(true, false, false, true, null, false, "rep", true, null, true, "\"machine/name\"")]
        [InlineData(true, false, false, true, null, false, "rep", true, "add", false, "\"nameadd\"")]
        [InlineData(true, false, false, true, null, false, "rep", true, "add", true, "\"machine/nameadd\"")]
        [InlineData(true, false, false, true, null, true, null, false, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, null, false, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, null, false, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, null, false, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, null, true, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, null, true, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, null, true, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, null, true, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, "rep", false, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, "rep", false, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, "rep", false, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, "rep", false, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, "rep", true, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, "rep", true, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, "rep", true, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, null, true, "rep", true, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, null, false, null, false, "machine_name\"name\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, null, false, null, true, "machine_name\"machine/name\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, null, false, "add", false, "machine_name\"nameadd\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, null, false, "add", true, "machine_name\"machine/nameadd\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, null, true, null, false, "machine_name\"name\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, null, true, null, true, "machine_name\"machine/name\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, null, true, "add", false, "machine_name\"nameadd\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, null, true, "add", true, "machine_name\"machine/nameadd\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, "rep", false, null, false, "machine_name\"namerep\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, "rep", false, null, true, "machine_name\"machine/namerep\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, "rep", false, "add", false, "machine_name\"namerepadd\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, "rep", false, "add", true, "machine_name\"machine/namerepadd\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, "rep", true, null, false, "machine_name\"name\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, "rep", true, null, true, "machine_name\"machine/name\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, "rep", true, "add", false, "machine_name\"nameadd\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", false, "rep", true, "add", true, "machine_name\"machine/nameadd\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, null, false, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, null, false, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, null, false, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, null, false, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, null, true, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, null, true, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, null, true, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, null, true, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, "rep", false, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, "rep", false, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, "rep", false, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, "rep", false, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, "rep", true, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, "rep", true, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, "rep", true, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, false, true, "%machine%_%name%", true, "rep", true, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, false, true, false, null, false, null, false, null, false, "name")]
        [InlineData(true, false, true, false, null, false, null, false, null, true, "name")]
        [InlineData(true, false, true, false, null, false, null, false, "add", false, "name")]
        [InlineData(true, false, true, false, null, false, null, false, "add", true, "name")]
        [InlineData(true, false, true, false, null, false, null, true, null, false, "name")]
        [InlineData(true, false, true, false, null, false, null, true, null, true, "name")]
        [InlineData(true, false, true, false, null, false, null, true, "add", false, "name")]
        [InlineData(true, false, true, false, null, false, null, true, "add", true, "name")]
        [InlineData(true, false, true, false, null, false, "rep", false, null, false, "name")]
        [InlineData(true, false, true, false, null, false, "rep", false, null, true, "name")]
        [InlineData(true, false, true, false, null, false, "rep", false, "add", false, "name")]
        [InlineData(true, false, true, false, null, false, "rep", false, "add", true, "name")]
        [InlineData(true, false, true, false, null, false, "rep", true, null, false, "name")]
        [InlineData(true, false, true, false, null, false, "rep", true, null, true, "name")]
        [InlineData(true, false, true, false, null, false, "rep", true, "add", false, "name")]
        [InlineData(true, false, true, false, null, false, "rep", true, "add", true, "name")]
        [InlineData(true, false, true, false, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, null, false, null, false, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, null, false, null, true, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, null, false, "add", false, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, null, false, "add", true, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, null, true, null, false, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, null, true, null, true, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, null, true, "add", false, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, null, true, "add", true, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, "rep", false, null, false, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, "rep", false, null, true, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, "rep", false, "add", false, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, "rep", false, "add", true, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, "rep", true, null, false, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, "rep", true, null, true, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, "rep", true, "add", false, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", false, "rep", true, "add", true, "name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, false, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, null, false, null, false, null, false, "name")]
        [InlineData(true, false, true, true, null, false, null, false, null, true, "machine/name")]
        [InlineData(true, false, true, true, null, false, null, false, "add", false, "nameadd")]
        [InlineData(true, false, true, true, null, false, null, false, "add", true, "machine/nameadd")]
        [InlineData(true, false, true, true, null, false, null, true, null, false, "name")]
        [InlineData(true, false, true, true, null, false, null, true, null, true, "machine/name")]
        [InlineData(true, false, true, true, null, false, null, true, "add", false, "nameadd")]
        [InlineData(true, false, true, true, null, false, null, true, "add", true, "machine/nameadd")]
        [InlineData(true, false, true, true, null, false, "rep", false, null, false, "namerep")]
        [InlineData(true, false, true, true, null, false, "rep", false, null, true, "machine/namerep")]
        [InlineData(true, false, true, true, null, false, "rep", false, "add", false, "namerepadd")]
        [InlineData(true, false, true, true, null, false, "rep", false, "add", true, "machine/namerepadd")]
        [InlineData(true, false, true, true, null, false, "rep", true, null, false, "name")]
        [InlineData(true, false, true, true, null, false, "rep", true, null, true, "machine/name")]
        [InlineData(true, false, true, true, null, false, "rep", true, "add", false, "nameadd")]
        [InlineData(true, false, true, true, null, false, "rep", true, "add", true, "machine/nameadd")]
        [InlineData(true, false, true, true, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, null, false, null, false, "machine_namenamemachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, null, false, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, null, false, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, null, false, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, null, true, null, false, "machine_namenamemachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, null, true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, null, true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, null, true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, "rep", false, null, false, "machine_namenamerepmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, "rep", false, null, true, "machine_namemachine/namerepmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, "rep", false, "add", false, "machine_namenamerepaddmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, "rep", false, "add", true, "machine_namemachine/namerepaddmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, "rep", true, null, false, "machine_namenamemachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, "rep", true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, "rep", true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", false, "rep", true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, false, true, true, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, false, false, null, false, null, false, null, false, "\"name\"")]
        [InlineData(true, true, false, false, null, false, null, false, null, true, "\"machine/name\"")]
        [InlineData(true, true, false, false, null, false, null, false, "add", false, "\"nameadd\"")]
        [InlineData(true, true, false, false, null, false, null, false, "add", true, "\"machine/nameadd\"")]
        [InlineData(true, true, false, false, null, false, null, true, null, false, "\"name\"")]
        [InlineData(true, true, false, false, null, false, null, true, null, true, "\"machine/name\"")]
        [InlineData(true, true, false, false, null, false, null, true, "add", false, "\"nameadd\"")]
        [InlineData(true, true, false, false, null, false, null, true, "add", true, "\"machine/nameadd\"")]
        [InlineData(true, true, false, false, null, false, "rep", false, null, false, "\"namerep\"")]
        [InlineData(true, true, false, false, null, false, "rep", false, null, true, "\"machine/namerep\"")]
        [InlineData(true, true, false, false, null, false, "rep", false, "add", false, "\"namerepadd\"")]
        [InlineData(true, true, false, false, null, false, "rep", false, "add", true, "\"machine/namerepadd\"")]
        [InlineData(true, true, false, false, null, false, "rep", true, null, false, "\"name\"")]
        [InlineData(true, true, false, false, null, false, "rep", true, null, true, "\"machine/name\"")]
        [InlineData(true, true, false, false, null, false, "rep", true, "add", false, "\"nameadd\"")]
        [InlineData(true, true, false, false, null, false, "rep", true, "add", true, "\"machine/nameadd\"")]
        [InlineData(true, true, false, false, null, true, null, false, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, null, false, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, null, false, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, null, false, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, null, true, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, null, true, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, null, true, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, null, true, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, "rep", false, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, "rep", false, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, "rep", false, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, "rep", false, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, "rep", true, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, "rep", true, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, "rep", true, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, null, true, "rep", true, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, null, false, null, false, "machine_name\"name\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, null, false, null, true, "machine_name\"machine/name\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, null, false, "add", false, "machine_name\"nameadd\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, null, false, "add", true, "machine_name\"machine/nameadd\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, null, true, null, false, "machine_name\"name\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, null, true, null, true, "machine_name\"machine/name\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, null, true, "add", false, "machine_name\"nameadd\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, null, true, "add", true, "machine_name\"machine/nameadd\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, "rep", false, null, false, "machine_name\"namerep\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, "rep", false, null, true, "machine_name\"machine/namerep\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, "rep", false, "add", false, "machine_name\"namerepadd\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, "rep", false, "add", true, "machine_name\"machine/namerepadd\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, "rep", true, null, false, "machine_name\"name\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, "rep", true, null, true, "machine_name\"machine/name\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, "rep", true, "add", false, "machine_name\"nameadd\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", false, "rep", true, "add", true, "machine_name\"machine/nameadd\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, null, false, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, null, false, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, null, false, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, null, false, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, null, true, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, null, true, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, null, true, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, null, true, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, "rep", false, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, "rep", false, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, "rep", false, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, "rep", false, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, "rep", true, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, "rep", true, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, "rep", true, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, false, "%machine%_%name%", true, "rep", true, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, null, false, null, false, null, false, "\"name\"")]
        [InlineData(true, true, false, true, null, false, null, false, null, true, "\"machine/name\"")]
        [InlineData(true, true, false, true, null, false, null, false, "add", false, "\"nameadd\"")]
        [InlineData(true, true, false, true, null, false, null, false, "add", true, "\"machine/nameadd\"")]
        [InlineData(true, true, false, true, null, false, null, true, null, false, "\"name\"")]
        [InlineData(true, true, false, true, null, false, null, true, null, true, "\"machine/name\"")]
        [InlineData(true, true, false, true, null, false, null, true, "add", false, "\"nameadd\"")]
        [InlineData(true, true, false, true, null, false, null, true, "add", true, "\"machine/nameadd\"")]
        [InlineData(true, true, false, true, null, false, "rep", false, null, false, "\"namerep\"")]
        [InlineData(true, true, false, true, null, false, "rep", false, null, true, "\"machine/namerep\"")]
        [InlineData(true, true, false, true, null, false, "rep", false, "add", false, "\"namerepadd\"")]
        [InlineData(true, true, false, true, null, false, "rep", false, "add", true, "\"machine/namerepadd\"")]
        [InlineData(true, true, false, true, null, false, "rep", true, null, false, "\"name\"")]
        [InlineData(true, true, false, true, null, false, "rep", true, null, true, "\"machine/name\"")]
        [InlineData(true, true, false, true, null, false, "rep", true, "add", false, "\"nameadd\"")]
        [InlineData(true, true, false, true, null, false, "rep", true, "add", true, "\"machine/nameadd\"")]
        [InlineData(true, true, false, true, null, true, null, false, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, null, false, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, null, false, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, null, false, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, null, true, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, null, true, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, null, true, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, null, true, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, "rep", false, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, "rep", false, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, "rep", false, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, "rep", false, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, "rep", true, null, false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, "rep", true, null, true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, "rep", true, "add", false, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, null, true, "rep", true, "add", true, "\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, null, false, null, false, "machine_name\"name\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, null, false, null, true, "machine_name\"machine/name\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, null, false, "add", false, "machine_name\"nameadd\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, null, false, "add", true, "machine_name\"machine/nameadd\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, null, true, null, false, "machine_name\"name\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, null, true, null, true, "machine_name\"machine/name\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, null, true, "add", false, "machine_name\"nameadd\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, null, true, "add", true, "machine_name\"machine/nameadd\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, "rep", false, null, false, "machine_name\"namerep\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, "rep", false, null, true, "machine_name\"machine/namerep\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, "rep", false, "add", false, "machine_name\"namerepadd\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, "rep", false, "add", true, "machine_name\"machine/namerepadd\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, "rep", true, null, false, "machine_name\"name\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, "rep", true, null, true, "machine_name\"machine/name\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, "rep", true, "add", false, "machine_name\"nameadd\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", false, "rep", true, "add", true, "machine_name\"machine/nameadd\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, null, false, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, null, false, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, null, false, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, null, false, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, null, true, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, null, true, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, null, true, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, null, true, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, "rep", false, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, "rep", false, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, "rep", false, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, "rep", false, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, "rep", true, null, false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, "rep", true, null, true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, "rep", true, "add", false, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, false, true, "%machine%_%name%", true, "rep", true, "add", true, "machine_name\"da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz\"machine_name")]
        [InlineData(true, true, true, false, null, false, null, false, null, false, "name")]
        [InlineData(true, true, true, false, null, false, null, false, null, true, "machine/name")]
        [InlineData(true, true, true, false, null, false, null, false, "add", false, "nameadd")]
        [InlineData(true, true, true, false, null, false, null, false, "add", true, "machine/nameadd")]
        [InlineData(true, true, true, false, null, false, null, true, null, false, "name")]
        [InlineData(true, true, true, false, null, false, null, true, null, true, "machine/name")]
        [InlineData(true, true, true, false, null, false, null, true, "add", false, "nameadd")]
        [InlineData(true, true, true, false, null, false, null, true, "add", true, "machine/nameadd")]
        [InlineData(true, true, true, false, null, false, "rep", false, null, false, "namerep")]
        [InlineData(true, true, true, false, null, false, "rep", false, null, true, "machine/namerep")]
        [InlineData(true, true, true, false, null, false, "rep", false, "add", false, "namerepadd")]
        [InlineData(true, true, true, false, null, false, "rep", false, "add", true, "machine/namerepadd")]
        [InlineData(true, true, true, false, null, false, "rep", true, null, false, "name")]
        [InlineData(true, true, true, false, null, false, "rep", true, null, true, "machine/name")]
        [InlineData(true, true, true, false, null, false, "rep", true, "add", false, "nameadd")]
        [InlineData(true, true, true, false, null, false, "rep", true, "add", true, "machine/nameadd")]
        [InlineData(true, true, true, false, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, null, false, null, false, "machine_namenamemachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, null, false, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, null, false, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, null, false, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, null, true, null, false, "machine_namenamemachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, null, true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, null, true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, null, true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, "rep", false, null, false, "machine_namenamerepmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, "rep", false, null, true, "machine_namemachine/namerepmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, "rep", false, "add", false, "machine_namenamerepaddmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, "rep", false, "add", true, "machine_namemachine/namerepaddmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, "rep", true, null, false, "machine_namenamemachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, "rep", true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, "rep", true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", false, "rep", true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, false, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, null, false, null, false, null, false, "name")]
        [InlineData(true, true, true, true, null, false, null, false, null, true, "machine/name")]
        [InlineData(true, true, true, true, null, false, null, false, "add", false, "nameadd")]
        [InlineData(true, true, true, true, null, false, null, false, "add", true, "machine/nameadd")]
        [InlineData(true, true, true, true, null, false, null, true, null, false, "name")]
        [InlineData(true, true, true, true, null, false, null, true, null, true, "machine/name")]
        [InlineData(true, true, true, true, null, false, null, true, "add", false, "nameadd")]
        [InlineData(true, true, true, true, null, false, null, true, "add", true, "machine/nameadd")]
        [InlineData(true, true, true, true, null, false, "rep", false, null, false, "namerep")]
        [InlineData(true, true, true, true, null, false, "rep", false, null, true, "machine/namerep")]
        [InlineData(true, true, true, true, null, false, "rep", false, "add", false, "namerepadd")]
        [InlineData(true, true, true, true, null, false, "rep", false, "add", true, "machine/namerepadd")]
        [InlineData(true, true, true, true, null, false, "rep", true, null, false, "name")]
        [InlineData(true, true, true, true, null, false, "rep", true, null, true, "machine/name")]
        [InlineData(true, true, true, true, null, false, "rep", true, "add", false, "nameadd")]
        [InlineData(true, true, true, true, null, false, "rep", true, "add", true, "machine/nameadd")]
        [InlineData(true, true, true, true, null, true, null, false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, null, false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, null, false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, null, false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, null, true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, null, true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, null, true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, null, true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, "rep", false, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, "rep", false, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, "rep", false, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, "rep", false, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, "rep", true, null, false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, "rep", true, null, true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, "rep", true, "add", false, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, null, true, "rep", true, "add", true, "da/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gz")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, null, false, null, false, "machine_namenamemachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, null, false, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, null, false, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, null, false, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, null, true, null, false, "machine_namenamemachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, null, true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, null, true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, null, true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, "rep", false, null, false, "machine_namenamerepmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, "rep", false, null, true, "machine_namemachine/namerepmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, "rep", false, "add", false, "machine_namenamerepaddmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, "rep", false, "add", true, "machine_namemachine/namerepaddmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, "rep", true, null, false, "machine_namenamemachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, "rep", true, null, true, "machine_namemachine/namemachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, "rep", true, "add", false, "machine_namenameaddmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", false, "rep", true, "add", true, "machine_namemachine/nameaddmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, null, false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, null, false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, null, false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, null, false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, null, true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, null, true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, null, true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, null, true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, "rep", false, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, "rep", false, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, "rep", false, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, "rep", false, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, "rep", true, null, false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, "rep", true, null, true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, "rep", true, "add", false, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        [InlineData(true, true, true, true, "%machine%_%name%", true, "rep", true, "add", true, "machine_nameda/39/a3/ee/da39a3ee5e6b4b0d3255bfef95601890afd80709.gzmachine_name")]
        public void ProcessItemNameTest(
            bool quotes,
            bool useRomName,
            bool forceRemoveQuotes,
            bool forceRomName,
            string? fix,
            bool depot,
            string? replaceExtension,
            bool removeExtension,
            string? addExtension,
            bool gameName,
            string expected)
        {
            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");

            DatItem item = new Rom();
            item.SetFieldValue(Models.Metadata.Rom.NameKey, "name");
            item.SetFieldValue(Models.Metadata.Rom.SHA1Key, ZeroHash.SHA1Str);

            DatFile? datFile = new Formats.Logiqx(datFile: null, useGame: false);
            datFile.Modifiers.Prefix = fix;
            datFile.Modifiers.Postfix = fix;
            datFile.Modifiers.AddExtension = addExtension;
            datFile.Modifiers.RemoveExtension = removeExtension;
            datFile.Modifiers.ReplaceExtension = replaceExtension;
            datFile.Modifiers.GameName = gameName;
            datFile.Modifiers.Quotes = quotes;
            datFile.Modifiers.UseRomName = useRomName;
            if (depot)
                datFile.Modifiers.OutputDepot = new DepotInformation(isActive: true, depth: 4);

            datFile.ProcessItemName(item, machine, forceRemoveQuotes, forceRomName);
            string? actual = item.GetName()?.Replace('\\', '/');
            Assert.Equal(expected, actual);
        }

        #endregion

        #region FormatPrefixPostfix

        [Fact]
        public void FormatPrefixPostfix_EmptyFix()
        {
            string fix = string.Empty;
            string expected = string.Empty;

            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");
            machine.SetFieldValue(Models.Metadata.Machine.ManufacturerKey, "manufacturer");
            machine.SetFieldValue(Models.Metadata.Machine.PublisherKey, "publisher");
            machine.SetFieldValue(Models.Metadata.Machine.CategoryKey, "category");

            DatItem item = new Rom();
            item.SetFieldValue(Models.Metadata.Rom.NameKey, "name");
            item.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            item.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            item.SetFieldValue(Models.Metadata.Rom.MD2Key, "md2");
            item.SetFieldValue(Models.Metadata.Rom.MD4Key, "md4");
            item.SetFieldValue(Models.Metadata.Rom.MD5Key, "md5");
            item.SetFieldValue("RIPEMD128", "ripemd128");
            item.SetFieldValue("RIPEMD160", "ripemd160");
            item.SetFieldValue(Models.Metadata.Rom.SHA1Key, "sha1");
            item.SetFieldValue(Models.Metadata.Rom.SHA256Key, "sha256");
            item.SetFieldValue(Models.Metadata.Rom.SHA384Key, "sha384");
            item.SetFieldValue(Models.Metadata.Rom.SHA512Key, "sha512");
            item.SetFieldValue(Models.Metadata.Rom.SpamSumKey, "spamsum");

            string actual = DatFile.FormatPrefixPostfix(item, machine, fix);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormatPrefixPostfix_Disk()
        {
            string fix = "%game%_%machine%_%name%_%manufacturer%_%publisher%_%category%_%crc%_%md2%_%md4%_%md5%_%sha1%_%sha256%_%sha384%_%sha512%_%size%_%spamsum%";
            string expected = "machine_machine_name_manufacturer_publisher_category____md5_sha1_____";

            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");
            machine.SetFieldValue(Models.Metadata.Machine.ManufacturerKey, "manufacturer");
            machine.SetFieldValue(Models.Metadata.Machine.PublisherKey, "publisher");
            machine.SetFieldValue(Models.Metadata.Machine.CategoryKey, "category");

            DatItem item = new Disk();
            item.SetFieldValue(Models.Metadata.Disk.NameKey, "name");
            item.SetFieldValue(Models.Metadata.Disk.MD5Key, "md5");
            item.SetFieldValue(Models.Metadata.Disk.SHA1Key, "sha1");

            string actual = DatFile.FormatPrefixPostfix(item, machine, fix);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormatPrefixPostfix_File()
        {
            string fix = "%game%_%machine%_%name%_%manufacturer%_%publisher%_%category%_%crc%_%md2%_%md4%_%md5%_%sha1%_%sha256%_%sha384%_%sha512%_%size%_%spamsum%";
            string expected = "machine_machine_name.bin_manufacturer_publisher_category_00000000___d41d8cd98f00b204e9800998ecf8427e_da39a3ee5e6b4b0d3255bfef95601890afd80709_e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855___12345_";

            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");
            machine.SetFieldValue(Models.Metadata.Machine.ManufacturerKey, "manufacturer");
            machine.SetFieldValue(Models.Metadata.Machine.PublisherKey, "publisher");
            machine.SetFieldValue(Models.Metadata.Machine.CategoryKey, "category");

            DatItem item = new DatItems.Formats.File
            {
                Id = "name",
                Extension = "bin",
                Size = 12345,
                CRC = ZeroHash.CRC32Str,
                MD5 = ZeroHash.MD5Str,
                SHA1 = ZeroHash.SHA1Str,
                SHA256 = ZeroHash.SHA256Str,
            };

            string actual = DatFile.FormatPrefixPostfix(item, machine, fix);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormatPrefixPostfix_Media()
        {
            string fix = "%game%_%machine%_%name%_%manufacturer%_%publisher%_%category%_%crc%_%md2%_%md4%_%md5%_%sha1%_%sha256%_%sha384%_%sha512%_%size%_%spamsum%";
            string expected = "machine_machine_name_manufacturer_publisher_category____md5_sha1_sha256____spamsum";

            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");
            machine.SetFieldValue(Models.Metadata.Machine.ManufacturerKey, "manufacturer");
            machine.SetFieldValue(Models.Metadata.Machine.PublisherKey, "publisher");
            machine.SetFieldValue(Models.Metadata.Machine.CategoryKey, "category");

            DatItem item = new Media();
            item.SetFieldValue(Models.Metadata.Media.NameKey, "name");
            item.SetFieldValue(Models.Metadata.Media.MD5Key, "md5");
            item.SetFieldValue(Models.Metadata.Media.SHA1Key, "sha1");
            item.SetFieldValue(Models.Metadata.Media.SHA256Key, "sha256");
            item.SetFieldValue(Models.Metadata.Media.SpamSumKey, "spamsum");

            string actual = DatFile.FormatPrefixPostfix(item, machine, fix);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormatPrefixPostfix_Rom()
        {
            string fix = "%game%_%machine%_%name%_%manufacturer%_%publisher%_%category%_%crc%_%md2%_%md4%_%md5%_%ripemd128%_%ripemd160%_%sha1%_%sha256%_%sha384%_%sha512%_%size%_%spamsum%";
            string expected = "machine_machine_name_manufacturer_publisher_category_crc_md2_md4_md5_ripemd128_ripemd160_sha1_sha256_sha384_sha512_12345_spamsum";

            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");
            machine.SetFieldValue(Models.Metadata.Machine.ManufacturerKey, "manufacturer");
            machine.SetFieldValue(Models.Metadata.Machine.PublisherKey, "publisher");
            machine.SetFieldValue(Models.Metadata.Machine.CategoryKey, "category");

            DatItem item = new Rom();
            item.SetFieldValue(Models.Metadata.Rom.NameKey, "name");
            item.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            item.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            item.SetFieldValue(Models.Metadata.Rom.MD2Key, "md2");
            item.SetFieldValue(Models.Metadata.Rom.MD4Key, "md4");
            item.SetFieldValue(Models.Metadata.Rom.MD5Key, "md5");
            item.SetFieldValue("RIPEMD128", "ripemd128");
            item.SetFieldValue("RIPEMD160", "ripemd160");
            item.SetFieldValue(Models.Metadata.Rom.SHA1Key, "sha1");
            item.SetFieldValue(Models.Metadata.Rom.SHA256Key, "sha256");
            item.SetFieldValue(Models.Metadata.Rom.SHA384Key, "sha384");
            item.SetFieldValue(Models.Metadata.Rom.SHA512Key, "sha512");
            item.SetFieldValue(Models.Metadata.Rom.SpamSumKey, "spamsum");

            string actual = DatFile.FormatPrefixPostfix(item, machine, fix);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessNullifiedItem

        [Fact]
        public void ProcessNullifiedItem_NonRom()
        {
            DatItem item = new Sample();

            DatItem actual = DatFile.ProcessNullifiedItem(item);
            Sample? sample = actual as Sample;
            Assert.NotNull(sample);
            Assert.Null(sample.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Null(sample.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
            Assert.Null(sample.GetStringFieldValue(Models.Metadata.Rom.MD2Key));
            Assert.Null(sample.GetStringFieldValue(Models.Metadata.Rom.MD4Key));
            Assert.Null(sample.GetStringFieldValue(Models.Metadata.Rom.MD5Key));
            Assert.Null(sample.GetStringFieldValue("RIPEMD128"));
            Assert.Null(sample.GetStringFieldValue("RIPEMD160"));
            Assert.Null(sample.GetStringFieldValue(Models.Metadata.Rom.SHA1Key));
            Assert.Null(sample.GetStringFieldValue(Models.Metadata.Rom.SHA256Key));
            Assert.Null(sample.GetStringFieldValue(Models.Metadata.Rom.SHA384Key));
            Assert.Null(sample.GetStringFieldValue(Models.Metadata.Rom.SHA512Key));
            Assert.Null(sample.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey));
        }

        [Fact]
        public void ProcessNullifiedItem_SizeNonNull()
        {
            DatItem item = new Rom();
            item.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);

            DatItem actual = DatFile.ProcessNullifiedItem(item);
            Rom? rom = actual as Rom;
            Assert.NotNull(rom);
            Assert.Equal(12345, rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.MD2Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.MD4Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key));
            Assert.Null(rom.GetStringFieldValue("RIPEMD128"));
            Assert.Null(rom.GetStringFieldValue("RIPEMD160"));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SHA384Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SHA512Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey));
        }

        [Fact]
        public void ProcessNullifiedItem_CrcNonNull()
        {
            DatItem item = new Rom();
            item.SetFieldValue(Models.Metadata.Rom.CRCKey, ZeroHash.CRC32Str);

            DatItem actual = DatFile.ProcessNullifiedItem(item);
            Rom? rom = actual as Rom;
            Assert.NotNull(rom);
            Assert.Null(rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal(ZeroHash.CRC32Str, rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.MD2Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.MD4Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key));
            Assert.Null(rom.GetStringFieldValue("RIPEMD128"));
            Assert.Null(rom.GetStringFieldValue("RIPEMD160"));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SHA384Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SHA512Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey));
        }

        [Fact]
        public void ProcessNullifiedItem_AllNull()
        {
            DatItem item = new Rom();
            item.SetFieldValue(Models.Metadata.Rom.CRCKey, "null");
            item.SetFieldValue(Models.Metadata.Rom.MD2Key, "null");
            item.SetFieldValue(Models.Metadata.Rom.MD4Key, "null");
            item.SetFieldValue(Models.Metadata.Rom.MD5Key, "null");
            item.SetFieldValue("RIPEMD128", "null");
            item.SetFieldValue("RIPEMD160", "null");
            item.SetFieldValue(Models.Metadata.Rom.SHA1Key, "null");
            item.SetFieldValue(Models.Metadata.Rom.SHA256Key, "null");
            item.SetFieldValue(Models.Metadata.Rom.SHA384Key, "null");
            item.SetFieldValue(Models.Metadata.Rom.SHA512Key, "null");
            item.SetFieldValue(Models.Metadata.Rom.SpamSumKey, "null");

            DatItem actual = DatFile.ProcessNullifiedItem(item);
            Rom? rom = actual as Rom;
            Assert.NotNull(rom);
            Assert.Equal(0, rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal(ZeroHash.CRC32Str, rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
            Assert.Equal(ZeroHash.GetString(HashType.MD2), rom.GetStringFieldValue(Models.Metadata.Rom.MD2Key));
            Assert.Equal(ZeroHash.GetString(HashType.MD4), rom.GetStringFieldValue(Models.Metadata.Rom.MD4Key));
            Assert.Equal(ZeroHash.MD5Str, rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD128), rom.GetStringFieldValue("RIPEMD128"));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD160), rom.GetStringFieldValue("RIPEMD160"));
            Assert.Equal(ZeroHash.SHA1Str, rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key));
            Assert.Equal(ZeroHash.SHA256Str, rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key));
            Assert.Equal(ZeroHash.SHA384Str, rom.GetStringFieldValue(Models.Metadata.Rom.SHA384Key));
            Assert.Equal(ZeroHash.SHA512Str, rom.GetStringFieldValue(Models.Metadata.Rom.SHA512Key));
            Assert.Equal(ZeroHash.SpamSumStr, rom.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey));
        }

        #endregion

        #region ContainsWritable

        [Fact]
        public void ContainsWritable_Empty_True()
        {
            List<DatItem> datItems = [];
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ContainsWritable(datItems);
            Assert.True(actual);
        }

        [Fact]
        public void ContainsWritable_NoWritable_False()
        {
            List<DatItem> datItems = [new Blank()];
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ContainsWritable(datItems);
            Assert.False(actual);
        }

        [Fact]
        public void ContainsWritable_Writable_True()
        {
            List<DatItem> datItems = [new Rom()];
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ContainsWritable(datItems);
            Assert.True(actual);
        }

        #endregion

        #region GetDuplicateSuffix

        [Fact]
        public void GetDuplicateSuffix_Disk_NoHash_Generic()
        {
            DatItem datItem = new Disk();
            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal("_1", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Disk_MD5()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Disk();
            datItem.SetFieldValue(Models.Metadata.Disk.MD5Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Disk_SHA1()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Disk();
            datItem.SetFieldValue(Models.Metadata.Disk.SHA1Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_File_NoHash_Generic()
        {
            DatItem datItem = new DatItems.Formats.File();
            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal("_1", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_File_CRC()
        {
            string hash = "deadbeef";
            DatItems.Formats.File datItem = new DatItems.Formats.File { CRC = hash };

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_File_MD5()
        {
            string hash = "000000000000000000000000deadbeef";
            DatItem datItem = new DatItems.Formats.File { MD5 = hash };

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_File_SHA1()
        {
            string hash = "00000000000000000000000000000000deadbeef";
            DatItem datItem = new DatItems.Formats.File { SHA1 = hash };

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_File_SHA256()
        {
            string hash = "00000000000000000000000000000000000000000000000000000000deadbeef";
            DatItem datItem = new DatItems.Formats.File { SHA256 = hash };

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Media_NoHash_Generic()
        {
            DatItem datItem = new Media();
            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal("_1", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Media_MD5()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Media();
            datItem.SetFieldValue(Models.Metadata.Media.MD5Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Media_SHA1()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Media();
            datItem.SetFieldValue(Models.Metadata.Media.SHA1Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Media_SHA256()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Media();
            datItem.SetFieldValue(Models.Metadata.Media.SHA256Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Media_SpamSum()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Media();
            datItem.SetFieldValue(Models.Metadata.Media.SpamSumKey, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_NoHash_Generic()
        {
            DatItem datItem = new Rom();
            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal("_1", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_CRC()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.CRCKey, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_MD2()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.MD2Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_MD4()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.MD4Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_MD5()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.MD5Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_RIPEMD128()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue("RIPEMD128", hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_RIPEMD160()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue("RIPEMD160", hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_SHA1()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.SHA1Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_SHA256()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.SHA256Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_SHA384()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.SHA384Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_SHA512()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.SHA512Key, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Rom_SpamSum()
        {
            string hash = "XXXXXX";
            DatItem datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.SpamSumKey, hash);

            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal($"_{hash}", actual);
        }

        [Fact]
        public void GetDuplicateSuffix_Other_Generic()
        {
            DatItem datItem = new Sample();
            string actual = DatFile.GetDuplicateSuffix(datItem);
            Assert.Equal("_1", actual);
        }

        #endregion

        #region ResolveNames

        [Fact]
        public void ResolveNames_EmptyList_Empty()
        {
            List<DatItem> datItems = [];

            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            List<DatItem> actual = datFile.ResolveNames(datItems);
            Assert.Empty(actual);
        }

        [Fact]
        public void ResolveNames_SingleItem_Single()
        {
            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");

            Source source = new Source(0);

            Rom romA = new Rom();
            romA.SetName("name");
            romA.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romA.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            romA.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romA.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            List<DatItem> datItems = [romA];

            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            List<DatItem> actual = datFile.ResolveNames(datItems);
            DatItem actualItemA = Assert.Single(actual);
            Rom? actualRomA = actualItemA as Rom;
            Assert.NotNull(actualRomA);
            Assert.Equal("name", actualRomA.GetName());
            Assert.Equal(12345, actualRomA.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc", actualRomA.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
        }

        [Fact]
        public void ResolveNames_NonDuplicate_AllUntouched()
        {
            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");

            Source source = new Source(0);

            Rom romA = new Rom();
            romA.SetName("romA");
            romA.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romA.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            romA.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romA.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            Rom romB = new Rom();
            romB.SetName("romB");
            romB.SetFieldValue(Models.Metadata.Rom.SizeKey, 23456);
            romB.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc2");
            romB.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romB.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            List<DatItem> datItems = [romA, romB];

            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            List<DatItem> actual = datFile.ResolveNames(datItems);
            Assert.Equal(2, actual.Count);

            Rom? actualRomA = actual[0] as Rom;
            Assert.NotNull(actualRomA);
            Assert.Equal("romA", actualRomA.GetName());
            Assert.Equal(12345, actualRomA.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc", actualRomA.GetStringFieldValue(Models.Metadata.Rom.CRCKey));

            Rom? actualRomB = actual[1] as Rom;
            Assert.NotNull(actualRomB);
            Assert.Equal("romB", actualRomB.GetName());
            Assert.Equal(23456, actualRomB.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc2", actualRomB.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
        }

        [Fact]
        public void ResolveNames_AllDuplicate_Single()
        {
            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");

            Source source = new Source(0);

            Rom romA = new Rom();
            romA.SetName("rom");
            romA.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romA.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            romA.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romA.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            Rom romB = new Rom();
            romB.SetName("rom");
            romB.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romB.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            romB.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romB.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            List<DatItem> datItems = [romA, romB];

            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            List<DatItem> actual = datFile.ResolveNames(datItems);
            DatItem actualItemA = Assert.Single(actual);
            Rom? actualRomA = actualItemA as Rom;
            Assert.NotNull(actualRomA);
            Assert.Equal("rom", actualRomA.GetName());
            Assert.Equal(12345, actualRomA.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc", actualRomA.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
        }

        [Fact]
        public void ResolveNames_NameMatch_SingleRenamed()
        {
            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");

            Source source = new Source(0);

            Rom romA = new Rom();
            romA.SetName("rom");
            romA.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romA.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            romA.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romA.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            Rom romB = new Rom();
            romB.SetName("rom");
            romB.SetFieldValue(Models.Metadata.Rom.SizeKey, 23456);
            romB.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc2");
            romB.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romB.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            List<DatItem> datItems = [romA, romB];

            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            List<DatItem> actual = datFile.ResolveNames(datItems);
            Assert.Equal(2, actual.Count);

            Rom? actualRomA = actual[0] as Rom;
            Assert.NotNull(actualRomA);
            Assert.Equal("rom", actualRomA.GetName());
            Assert.Equal(12345, actualRomA.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc", actualRomA.GetStringFieldValue(Models.Metadata.Rom.CRCKey));

            Rom? actualRomB = actual[1] as Rom;
            Assert.NotNull(actualRomB);
            Assert.Equal("rom_crc2", actualRomB.GetName());
            Assert.Equal(23456, actualRomB.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc2", actualRomB.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
        }

        #endregion

        #region ResolveNamesDB

        [Fact]
        public void ResolveNamesDB_EmptyList_Empty()
        {
            List<KeyValuePair<long, DatItem>> mappings = [];

            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            List<KeyValuePair<long, DatItem>> actual = datFile.ResolveNamesDB(mappings);
            Assert.Empty(actual);
        }

        [Fact]
        public void ResolveNamesDB_SingleItem_Single()
        {
            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");

            Source source = new Source(0);

            Rom romA = new Rom();
            romA.SetName("name");
            romA.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romA.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            romA.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romA.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            List<KeyValuePair<long, DatItem>> mappings =
            [
                new KeyValuePair<long, DatItem>(0, romA),
            ];
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            List<KeyValuePair<long, DatItem>> actual = datFile.ResolveNamesDB(mappings);
            KeyValuePair<long, DatItem> actualItemA = Assert.Single(actual);
            Rom? actualRomA = actualItemA.Value as Rom;
            Assert.NotNull(actualRomA);
            Assert.Equal("name", actualRomA.GetName());
            Assert.Equal(12345, actualRomA.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc", actualRomA.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
        }

        [Fact]
        public void ResolveNamesDB_NonDuplicate_AllUntouched()
        {
            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");

            Source source = new Source(0);

            Rom romA = new Rom();
            romA.SetName("romA");
            romA.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romA.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            romA.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romA.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            Rom romB = new Rom();
            romB.SetName("romB");
            romB.SetFieldValue(Models.Metadata.Rom.SizeKey, 23456);
            romB.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc2");
            romB.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romB.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            List<KeyValuePair<long, DatItem>> mappings =
            [
                new KeyValuePair<long, DatItem>(0, romA),
                new KeyValuePair<long, DatItem>(1, romB),
            ];
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            List<KeyValuePair<long, DatItem>> actual = datFile.ResolveNamesDB(mappings);
            Assert.Equal(2, actual.Count);

            Rom? actualRomA = actual[0].Value as Rom;
            Assert.NotNull(actualRomA);
            Assert.Equal("romA", actualRomA.GetName());
            Assert.Equal(12345, actualRomA.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc", actualRomA.GetStringFieldValue(Models.Metadata.Rom.CRCKey));

            Rom? actualRomB = actual[1].Value as Rom;
            Assert.NotNull(actualRomB);
            Assert.Equal("romB", actualRomB.GetName());
            Assert.Equal(23456, actualRomB.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc2", actualRomB.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
        }

        [Fact]
        public void ResolveNamesDB_AllDuplicate_Single()
        {
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");
            long machineIndex = datFile.AddMachineDB(machine);

            Source source = new Source(0);
            long sourceIndex = datFile.AddSourceDB(source);

            Rom romA = new Rom();
            romA.SetName("rom");
            romA.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romA.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            long romAIndex = datFile.AddItemDB(romA, machineIndex, sourceIndex, statsOnly: false);

            Rom romB = new Rom();
            romB.SetName("rom");
            romB.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romB.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            long romBIndex = datFile.AddItemDB(romB, machineIndex, sourceIndex, statsOnly: false);

            List<KeyValuePair<long, DatItem>> mappings =
            [
                new KeyValuePair<long, DatItem>(romAIndex, romA),
                new KeyValuePair<long, DatItem>(romBIndex, romB),
            ];

            List<KeyValuePair<long, DatItem>> actual = datFile.ResolveNamesDB(mappings);
            KeyValuePair<long, DatItem> actualItemA = Assert.Single(actual);
            Rom? actualRomA = actualItemA.Value as Rom;
            Assert.NotNull(actualRomA);
            Assert.Equal("rom", actualRomA.GetName());
            Assert.Equal(12345, actualRomA.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc", actualRomA.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
        }

        [Fact]
        public void ResolveNamesDB_NameMatch_SingleRenamed()
        {
            Machine machine = new Machine();
            machine.SetFieldValue(Models.Metadata.Machine.NameKey, "machine");

            Source source = new Source(0);

            Rom romA = new Rom();
            romA.SetName("rom");
            romA.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            romA.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            romA.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romA.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            Rom romB = new Rom();
            romB.SetName("rom");
            romB.SetFieldValue(Models.Metadata.Rom.SizeKey, 23456);
            romB.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc2");
            romB.SetFieldValue(DatItem.MachineKey, (Machine)machine.Clone());
            romB.SetFieldValue(DatItem.SourceKey, (Source)source.Clone());

            List<KeyValuePair<long, DatItem>> mappings =
            [
                new KeyValuePair<long, DatItem>(0, romA),
                new KeyValuePair<long, DatItem>(1, romB),
            ];
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            List<KeyValuePair<long, DatItem>> actual = datFile.ResolveNamesDB(mappings);
            Assert.Equal(2, actual.Count);

            Rom? actualRomA = actual[0].Value as Rom;
            Assert.NotNull(actualRomA);
            Assert.Equal("rom", actualRomA.GetName());
            Assert.Equal(12345, actualRomA.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc", actualRomA.GetStringFieldValue(Models.Metadata.Rom.CRCKey));

            Rom? actualRomB = actual[1].Value as Rom;
            Assert.NotNull(actualRomB);
            Assert.Equal("rom_crc2", actualRomB.GetName());
            Assert.Equal(23456, actualRomB.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.Equal("crc2", actualRomB.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
        }

        #endregion

        #region ShouldIgnore

        [Fact]
        public void ShouldIgnore_NullItem_True()
        {
            DatItem? datItem = null;
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ShouldIgnore(datItem, ignoreBlanks: true);
            Assert.True(actual);
        }

        [Fact]
        public void ShouldIgnore_RemoveSet_True()
        {
            DatItem? datItem = new Rom();
            datItem.SetFieldValue(DatItem.RemoveKey, true);
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ShouldIgnore(datItem, ignoreBlanks: true);
            Assert.True(actual);
        }

        [Fact]
        public void ShouldIgnore_Blank_True()
        {
            DatItem? datItem = new Blank();
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ShouldIgnore(datItem, ignoreBlanks: true);
            Assert.True(actual);
        }

        [Fact]
        public void ShouldIgnore_IgnoreBlanksZeroRom_True()
        {
            DatItem? datItem = new Rom();
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ShouldIgnore(datItem, ignoreBlanks: true);
            Assert.True(actual);
        }

        [Fact]
        public void ShouldIgnore_NoIgnoreBlanksZeroRom_False()
        {
            DatItem? datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.NameKey, "name");
            datItem.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            datItem.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ShouldIgnore(datItem, ignoreBlanks: false);
            Assert.False(actual);
        }

        [Fact]
        public void ShouldIgnore_UnsupportedType_True()
        {
            DatItem? datItem = new DatItems.Formats.SoftwareList();
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ShouldIgnore(datItem, ignoreBlanks: true);
            Assert.True(actual);
        }

        [Fact]
        public void ShouldIgnore_MissingRequired_True()
        {
            DatItem? datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.NameKey, "name");
            datItem.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ShouldIgnore(datItem, ignoreBlanks: true);
            Assert.True(actual);
        }

        [Fact]
        public void ShouldIgnore_AllVerified_False()
        {
            DatItem? datItem = new Rom();
            datItem.SetFieldValue(Models.Metadata.Rom.NameKey, "name");
            datItem.SetFieldValue(Models.Metadata.Rom.SizeKey, 12345);
            datItem.SetFieldValue(Models.Metadata.Rom.CRCKey, "crc");
            datItem.SetFieldValue(Models.Metadata.Rom.MD2Key, "crc");
            datItem.SetFieldValue(Models.Metadata.Rom.MD4Key, "crc");
            datItem.SetFieldValue(Models.Metadata.Rom.MD5Key, "crc");
            datItem.SetFieldValue("RIPEMD128", "crc");
            datItem.SetFieldValue("RIPEMD160", "crc");
            datItem.SetFieldValue(Models.Metadata.Rom.SHA1Key, "crc");
            datItem.SetFieldValue(Models.Metadata.Rom.SHA256Key, "crc");
            datItem.SetFieldValue(Models.Metadata.Rom.SHA384Key, "crc");
            datItem.SetFieldValue(Models.Metadata.Rom.SHA512Key, "crc");
            datItem.SetFieldValue(Models.Metadata.Rom.SpamSumKey, "crc");
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            bool actual = datFile.ShouldIgnore(datItem, ignoreBlanks: true);
            Assert.False(actual);
        }

        #endregion
    }
}