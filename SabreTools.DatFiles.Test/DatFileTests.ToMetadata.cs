using System;
using System.Collections.Generic;
using SabreTools.Core;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using Xunit;

namespace SabreTools.DatFiles.Test
{
    public partial class DatFileTests
    {
        #region ConvertToMetadata

        [Fact]
        public void ConvertToMetadata_Empty()
        {
            DatFile datFile = new Formats.Logiqx(null, useGame: false);

            Data.Models.Metadata.MetadataFile? actual = datFile.ConvertToMetadata();
            Assert.Null(actual);
        }

        [Fact]
        public void ConvertToMetadata_FilledHeader()
        {
            DatHeader header = CreateHeader();

            DatFile datFile = new Formats.Logiqx(null, useGame: false);
            datFile.SetHeader(header);
            datFile.AddItem(new Rom(), statsOnly: false);

            Data.Models.Metadata.MetadataFile? actual = datFile.ConvertToMetadata();
            Assert.NotNull(actual);

            Data.Models.Metadata.Header? actualHeader = actual.Read<Data.Models.Metadata.Header>(Data.Models.Metadata.MetadataFile.HeaderKey);
            ValidateMetadataHeader(actualHeader);
        }

        [Fact]
        public void ConvertToMetadata_FilledMachine()
        {
            Machine machine = CreateMachine();

            List<DatItem> datItems =
            [
                CreateAdjuster(machine),
                CreateArchive(machine),
                CreateBiosSet(machine),
                CreateChip(machine),
                CreateConfiguration(machine),
                CreateDevice(machine),
                CreateDeviceRef(machine),
                CreateDipSwitch(machine),
                CreateDipSwitchWithPart(machine),
                CreateDisk(machine),
                CreateDiskWithDiskAreaPart(machine),
                CreateDisplay(machine),
                CreateDriver(machine),
                CreateFeature(machine),
                CreateInfo(machine),
                CreateInput(machine),
                CreateMedia(machine),
                CreatePartFeature(machine),
                CreatePort(machine),
                CreateRamOption(machine),
                CreateRelease(machine),
                CreateRom(machine),
                CreateRomWithDiskAreaPart(machine),
                CreateSample(machine),
                CreateSharedFeat(machine),
                CreateSlot(machine),
                CreateSoftwareList(machine),
                CreateSound(machine),
                CreateVideo(machine),
            ];

            DatFile datFile = new Formats.SabreJSON(null);
            datItems.ForEach(item => datFile.AddItem(item, statsOnly: false));

            Data.Models.Metadata.MetadataFile? actual = datFile.ConvertToMetadata();
            Assert.NotNull(actual);

            Data.Models.Metadata.Machine[]? machines = actual.ReadItemArray<Data.Models.Metadata.Machine>(Data.Models.Metadata.MetadataFile.MachineKey);
            Assert.NotNull(machines);
            Data.Models.Metadata.Machine actualMachine = Assert.Single(machines);
            ValidateMetadataMachine(actualMachine);
        }

        #endregion

        #region Creation Helpers

        private static DatHeader CreateHeader()
        {
            DatHeader item = new DatHeader(CreateMetadataHeader());

            item.SetFieldValue<string[]>(Data.Models.Metadata.Header.CanOpenKey, ["ext"]);
            item.SetFieldValue(Data.Models.Metadata.Header.ImagesKey,
                new Data.Models.OfflineList.Images() { Height = "height" });
            item.SetFieldValue(Data.Models.Metadata.Header.InfosKey,
                new Data.Models.OfflineList.Infos() { Comment = new Data.Models.OfflineList.Comment() });
            item.SetFieldValue(Data.Models.Metadata.Header.NewDatKey,
                new Data.Models.OfflineList.NewDat() { DatUrl = new Data.Models.OfflineList.DatUrl() });
            item.SetFieldValue(Data.Models.Metadata.Header.SearchKey,
                new Data.Models.OfflineList.Search() { To = [] });

            return item;
        }

        private static Machine CreateMachine()
        {
            Machine item = new Machine(CreateMetadataMachine());
            item.SetFieldValue(Data.Models.Metadata.Machine.TruripKey, CreateTrurip());
            return item;
        }

        private static Adjuster CreateAdjuster(Machine machine)
        {
            Adjuster item = new Adjuster(CreateMetadataAdjuster());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Archive CreateArchive(Machine machine)
        {
            Archive item = new Archive(CreateMetadataArchive());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static BiosSet CreateBiosSet(Machine machine)
        {
            BiosSet item = new BiosSet(CreateMetadataBiosSet());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Chip CreateChip(Machine machine)
        {
            Chip item = new Chip(CreateMetadataChip());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Configuration CreateConfiguration(Machine machine)
        {
            Configuration item = new Configuration(CreateMetadataConfiguration());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Device CreateDevice(Machine machine)
        {
            Device item = new Device(CreateMetadataDevice());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static DataArea CreateDataArea(Machine machine)
        {
            DataArea item = new DataArea(CreateMetadataDataArea());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static DeviceRef CreateDeviceRef(Machine machine)
        {
            DeviceRef item = new DeviceRef(CreateMetadataDeviceRef());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static DipSwitch CreateDipSwitch(Machine machine)
        {
            DipSwitch item = new DipSwitch(CreateMetadataDipSwitch());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static DipSwitch CreateDipSwitchWithPart(Machine machine)
        {
            DipSwitch item = new DipSwitch(CreateMetadataDipSwitch());
            item.CopyMachineInformation(machine);
            item.SetFieldValue(DipSwitch.PartKey, CreatePart(machine));
            return item;
        }

        private static Disk CreateDisk(Machine machine)
        {
            Disk item = new Disk(CreateMetadataDisk());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Disk CreateDiskWithDiskAreaPart(Machine machine)
        {
            Disk item = new Disk(CreateMetadataDisk());
            item.CopyMachineInformation(machine);
            item.SetFieldValue(Disk.DiskAreaKey, CreateDiskArea(machine));
            item.SetFieldValue(Disk.PartKey, CreatePart(machine));
            return item;
        }

        private static DiskArea CreateDiskArea(Machine machine)
        {
            DiskArea item = new DiskArea(CreateMetadataDiskArea());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Display CreateDisplay(Machine machine)
        {
            Display item = new Display(CreateMetadataDisplay());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Driver CreateDriver(Machine machine)
        {
            Driver item = new Driver(CreateMetadataDriver());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Feature CreateFeature(Machine machine)
        {
            Feature item = new Feature(CreateMetadataFeature());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Info CreateInfo(Machine machine)
        {
            Info item = new Info(CreateMetadataInfo());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Input CreateInput(Machine machine)
        {
            Input item = new Input(CreateMetadataInput());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Media CreateMedia(Machine machine)
        {
            Media item = new Media(CreateMetadataMedia());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Part CreatePart(Machine machine)
        {
            Part item = new Part(CreateMetadataPart());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static PartFeature CreatePartFeature(Machine machine)
        {
            PartFeature item = new PartFeature(CreateMetadataFeature());
            item.CopyMachineInformation(machine);
            item.SetFieldValue(PartFeature.PartKey, CreatePart(machine));
            return item;
        }

        private static Port CreatePort(Machine machine)
        {
            Port item = new Port(CreateMetadataPort());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static RamOption CreateRamOption(Machine machine)
        {
            RamOption item = new RamOption(CreateMetadataRamOption());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Release CreateRelease(Machine machine)
        {
            Release item = new Release(CreateMetadataRelease());
            item.CopyMachineInformation(machine);
            return item;
        }

        // TODO: Create variant that results in a Dump
        private static Rom CreateRom(Machine machine)
        {
            Rom item = new Rom(CreateMetadataRom());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Rom CreateRomWithDiskAreaPart(Machine machine)
        {
            Rom item = new Rom(CreateMetadataRom());
            item.CopyMachineInformation(machine);
            item.SetFieldValue(Rom.DataAreaKey, CreateDataArea(machine));
            item.SetFieldValue(Rom.PartKey, CreatePart(machine));
            return item;
        }

        private static Sample CreateSample(Machine machine)
        {
            Sample item = new Sample(CreateMetadataSample());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static SharedFeat CreateSharedFeat(Machine machine)
        {
            SharedFeat item = new SharedFeat(CreateMetadataSharedFeat());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Slot CreateSlot(Machine machine)
        {
            Slot item = new Slot(CreateMetadataSlot());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static SoftwareList CreateSoftwareList(Machine machine)
        {
            SoftwareList item = new SoftwareList(CreateMetadataSoftwareList());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Sound CreateSound(Machine machine)
        {
            Sound item = new Sound(CreateMetadataSound());
            item.CopyMachineInformation(machine);
            return item;
        }

        private static Trurip CreateTrurip()
        {
            return new Trurip
            {
                TitleID = "titleid",
                // Publisher = "publisher",
                Developer = "developer",
                // Year = "year",
                Genre = "genre",
                Subgenre = "subgenre",
                Ratings = "ratings",
                Score = "score",
                // Players = "players",
                Enabled = "enabled",
                Crc = true,
                // Source = "source",
                // CloneOf = "cloneof",
                RelatedTo = "relatedto",
            };
        }

        private static Display CreateVideo(Machine machine)
        {
            Display item = new Display(CreateMetadataVideo());
            item.CopyMachineInformation(machine);
            return item;
        }

        #endregion

        #region Validation Helpers

        private static void ValidateMetadataHeader(Data.Models.Metadata.Header? header)
        {
            Assert.NotNull(header);
            Assert.Equal("author", header.ReadString(Data.Models.Metadata.Header.AuthorKey));
            Assert.Equal("merged", header.ReadString(Data.Models.Metadata.Header.BiosModeKey));
            Assert.Equal("build", header.ReadString(Data.Models.Metadata.Header.BuildKey));
            Assert.NotNull(header.Read<Data.Models.OfflineList.CanOpen>(Data.Models.Metadata.Header.CanOpenKey));
            Assert.Equal("category", header.ReadString(Data.Models.Metadata.Header.CategoryKey));
            Assert.Equal("comment", header.ReadString(Data.Models.Metadata.Header.CommentKey));
            Assert.Equal("date", header.ReadString(Data.Models.Metadata.Header.DateKey));
            Assert.Equal("datversion", header.ReadString(Data.Models.Metadata.Header.DatVersionKey));
            Assert.True(header.ReadBool(Data.Models.Metadata.Header.DebugKey));
            Assert.Equal("description", header.ReadString(Data.Models.Metadata.Header.DescriptionKey));
            Assert.Equal("email", header.ReadString(Data.Models.Metadata.Header.EmailKey));
            Assert.Equal("emulatorversion", header.ReadString(Data.Models.Metadata.Header.EmulatorVersionKey));
            Assert.Equal("merged", header.ReadString(Data.Models.Metadata.Header.ForceMergingKey));
            Assert.Equal("required", header.ReadString(Data.Models.Metadata.Header.ForceNodumpKey));
            Assert.Equal("zip", header.ReadString(Data.Models.Metadata.Header.ForcePackingKey));
            Assert.True(header.ReadBool(Data.Models.Metadata.Header.ForceZippingKey));
            Assert.Equal("header", header.ReadString(Data.Models.Metadata.Header.HeaderKey));
            Assert.Equal("homepage", header.ReadString(Data.Models.Metadata.Header.HomepageKey));
            Assert.Equal("id", header.ReadString(Data.Models.Metadata.Header.IdKey));
            Assert.NotNull(header.Read<Data.Models.OfflineList.Images>(Data.Models.Metadata.Header.ImagesKey));
            Assert.Equal("imfolder", header.ReadString(Data.Models.Metadata.Header.ImFolderKey));
            Assert.NotNull(header.Read<Data.Models.OfflineList.Infos>(Data.Models.Metadata.Header.InfosKey));
            Assert.True(header.ReadBool(Data.Models.Metadata.Header.LockBiosModeKey));
            Assert.True(header.ReadBool(Data.Models.Metadata.Header.LockRomModeKey));
            Assert.True(header.ReadBool(Data.Models.Metadata.Header.LockSampleModeKey));
            Assert.Equal("mameconfig", header.ReadString(Data.Models.Metadata.Header.MameConfigKey));
            Assert.Equal("name", header.ReadString(Data.Models.Metadata.Header.NameKey));
            Assert.NotNull(header.Read<Data.Models.OfflineList.NewDat>(Data.Models.Metadata.Header.NewDatKey));
            Assert.Equal("notes", header.ReadString(Data.Models.Metadata.Header.NotesKey));
            Assert.Equal("plugin", header.ReadString(Data.Models.Metadata.Header.PluginKey));
            Assert.Equal("refname", header.ReadString(Data.Models.Metadata.Header.RefNameKey));
            Assert.Equal("merged", header.ReadString(Data.Models.Metadata.Header.RomModeKey));
            Assert.Equal("romtitle", header.ReadString(Data.Models.Metadata.Header.RomTitleKey));
            Assert.Equal("rootdir", header.ReadString(Data.Models.Metadata.Header.RootDirKey));
            Assert.Equal("merged", header.ReadString(Data.Models.Metadata.Header.SampleModeKey));
            Assert.Equal("schemalocation", header.ReadString(Data.Models.Metadata.Header.SchemaLocationKey));
            Assert.Equal("screenshotsheight", header.ReadString(Data.Models.Metadata.Header.ScreenshotsHeightKey));
            Assert.Equal("screenshotsWidth", header.ReadString(Data.Models.Metadata.Header.ScreenshotsWidthKey));
            Assert.NotNull(header.Read<Data.Models.OfflineList.Search>(Data.Models.Metadata.Header.SearchKey));
            Assert.Equal("system", header.ReadString(Data.Models.Metadata.Header.SystemKey));
            Assert.Equal("timestamp", header.ReadString(Data.Models.Metadata.Header.TimestampKey));
            Assert.Equal("type", header.ReadString(Data.Models.Metadata.Header.TypeKey));
            Assert.Equal("url", header.ReadString(Data.Models.Metadata.Header.UrlKey));
            Assert.Equal("version", header.ReadString(Data.Models.Metadata.Header.VersionKey));
        }

        private static void ValidateMetadataMachine(Data.Models.Metadata.Machine machine)
        {
            Assert.Equal("board", machine.ReadString(Data.Models.Metadata.Machine.BoardKey));
            Assert.Equal("buttons", machine.ReadString(Data.Models.Metadata.Machine.ButtonsKey));
            Assert.Equal("category", machine.ReadString(Data.Models.Metadata.Machine.CategoryKey));
            Assert.Equal("cloneof", machine.ReadString(Data.Models.Metadata.Machine.CloneOfKey));
            Assert.Equal("cloneofid", machine.ReadString(Data.Models.Metadata.Machine.CloneOfIdKey));
            Assert.Equal("comment", machine.ReadString(Data.Models.Metadata.Machine.CommentKey));
            Assert.Equal("company", machine.ReadString(Data.Models.Metadata.Machine.CompanyKey));
            Assert.Equal("control", machine.ReadString(Data.Models.Metadata.Machine.ControlKey));
            Assert.Equal("country", machine.ReadString(Data.Models.Metadata.Machine.CountryKey));
            Assert.Equal("description", machine.ReadString(Data.Models.Metadata.Machine.DescriptionKey));
            Assert.Equal("dirname", machine.ReadString(Data.Models.Metadata.Machine.DirNameKey));
            Assert.Equal("displaycount", machine.ReadString(Data.Models.Metadata.Machine.DisplayCountKey));
            Assert.Equal("displaytype", machine.ReadString(Data.Models.Metadata.Machine.DisplayTypeKey));
            Assert.Equal("duplicateid", machine.ReadString(Data.Models.Metadata.Machine.DuplicateIDKey));
            Assert.Equal("emulator", machine.ReadString(Data.Models.Metadata.Machine.EmulatorKey));
            Assert.Equal("extra", machine.ReadString(Data.Models.Metadata.Machine.ExtraKey));
            Assert.Equal("favorite", machine.ReadString(Data.Models.Metadata.Machine.FavoriteKey));
            Assert.Equal("genmsxid", machine.ReadString(Data.Models.Metadata.Machine.GenMSXIDKey));
            Assert.Equal("history", machine.ReadString(Data.Models.Metadata.Machine.HistoryKey));
            Assert.Equal("id", machine.ReadString(Data.Models.Metadata.Machine.IdKey));
            Assert.Equal(ZeroHash.CRC32Str, machine.ReadString(Data.Models.Metadata.Machine.Im1CRCKey));
            Assert.Equal(ZeroHash.CRC32Str, machine.ReadString(Data.Models.Metadata.Machine.Im2CRCKey));
            Assert.Equal("imagenumber", machine.ReadString(Data.Models.Metadata.Machine.ImageNumberKey));
            Assert.Equal("yes", machine.ReadString(Data.Models.Metadata.Machine.IsBiosKey));
            Assert.Equal("yes", machine.ReadString(Data.Models.Metadata.Machine.IsDeviceKey));
            Assert.Equal("yes", machine.ReadString(Data.Models.Metadata.Machine.IsMechanicalKey));
            Assert.Equal("language", machine.ReadString(Data.Models.Metadata.Machine.LanguageKey));
            Assert.Equal("location", machine.ReadString(Data.Models.Metadata.Machine.LocationKey));
            Assert.Equal("manufacturer", machine.ReadString(Data.Models.Metadata.Machine.ManufacturerKey));
            Assert.Equal("name", machine.ReadString(Data.Models.Metadata.Machine.NameKey));
            Assert.Equal("notes", machine.ReadString(Data.Models.Metadata.Machine.NotesKey));
            Assert.Equal("playedcount", machine.ReadString(Data.Models.Metadata.Machine.PlayedCountKey));
            Assert.Equal("playedtime", machine.ReadString(Data.Models.Metadata.Machine.PlayedTimeKey));
            Assert.Equal("players", machine.ReadString(Data.Models.Metadata.Machine.PlayersKey));
            Assert.Equal("publisher", machine.ReadString(Data.Models.Metadata.Machine.PublisherKey));
            Assert.Equal("rebuildto", machine.ReadString(Data.Models.Metadata.Machine.RebuildToKey));
            Assert.Equal("releasenumber", machine.ReadString(Data.Models.Metadata.Machine.ReleaseNumberKey));
            Assert.Equal("romof", machine.ReadString(Data.Models.Metadata.Machine.RomOfKey));
            Assert.Equal("rotation", machine.ReadString(Data.Models.Metadata.Machine.RotationKey));
            Assert.Equal("yes", machine.ReadString(Data.Models.Metadata.Machine.RunnableKey));
            Assert.Equal("sampleof", machine.ReadString(Data.Models.Metadata.Machine.SampleOfKey));
            Assert.Equal("savetype", machine.ReadString(Data.Models.Metadata.Machine.SaveTypeKey));
            Assert.Equal("sourcefile", machine.ReadString(Data.Models.Metadata.Machine.SourceFileKey));
            Assert.Equal("sourcerom", machine.ReadString(Data.Models.Metadata.Machine.SourceRomKey));
            Assert.Equal("status", machine.ReadString(Data.Models.Metadata.Machine.StatusKey));
            Assert.Equal("yes", machine.ReadString(Data.Models.Metadata.Machine.SupportedKey));
            Assert.Equal("system", machine.ReadString(Data.Models.Metadata.Machine.SystemKey));
            Assert.Equal("tags", machine.ReadString(Data.Models.Metadata.Machine.TagsKey));
            Assert.Equal("year", machine.ReadString(Data.Models.Metadata.Machine.YearKey));

            Data.Models.Metadata.Adjuster[]? adjusters = machine.ReadItemArray<Data.Models.Metadata.Adjuster>(Data.Models.Metadata.Machine.AdjusterKey);
            Assert.NotNull(adjusters);
            Data.Models.Metadata.Adjuster adjuster = Assert.Single(adjusters);
            ValidateMetadataAdjuster(adjuster);

            Data.Models.Metadata.Archive[]? archives = machine.ReadItemArray<Data.Models.Metadata.Archive>(Data.Models.Metadata.Machine.ArchiveKey);
            Assert.NotNull(archives);
            Data.Models.Metadata.Archive archive = Assert.Single(archives);
            ValidateMetadataArchive(archive);

            Data.Models.Metadata.BiosSet[]? biosSets = machine.ReadItemArray<Data.Models.Metadata.BiosSet>(Data.Models.Metadata.Machine.BiosSetKey);
            Assert.NotNull(biosSets);
            Data.Models.Metadata.BiosSet biosSet = Assert.Single(biosSets);
            ValidateMetadataBiosSet(biosSet);

            Data.Models.Metadata.Chip[]? chips = machine.ReadItemArray<Data.Models.Metadata.Chip>(Data.Models.Metadata.Machine.ChipKey);
            Assert.NotNull(chips);
            Data.Models.Metadata.Chip chip = Assert.Single(chips);
            ValidateMetadataChip(chip);

            Data.Models.Metadata.Configuration[]? configurations = machine.ReadItemArray<Data.Models.Metadata.Configuration>(Data.Models.Metadata.Machine.ConfigurationKey);
            Assert.NotNull(configurations);
            Data.Models.Metadata.Configuration configuration = Assert.Single(configurations);
            ValidateMetadataConfiguration(configuration);

            Data.Models.Metadata.Device[]? devices = machine.ReadItemArray<Data.Models.Metadata.Device>(Data.Models.Metadata.Machine.DeviceKey);
            Assert.NotNull(devices);
            Data.Models.Metadata.Device device = Assert.Single(devices);
            ValidateMetadataDevice(device);

            Data.Models.Metadata.DeviceRef[]? deviceRefs = machine.ReadItemArray<Data.Models.Metadata.DeviceRef>(Data.Models.Metadata.Machine.DeviceRefKey);
            Assert.NotNull(deviceRefs);
            Data.Models.Metadata.DeviceRef deviceRef = Assert.Single(deviceRefs);
            ValidateMetadataDeviceRef(deviceRef);

            Data.Models.Metadata.DipSwitch[]? dipSwitches = machine.ReadItemArray<Data.Models.Metadata.DipSwitch>(Data.Models.Metadata.Machine.DipSwitchKey);
            Assert.NotNull(dipSwitches);
            Assert.Equal(2, dipSwitches.Length);
            Data.Models.Metadata.DipSwitch dipSwitch = dipSwitches[0];
            ValidateMetadataDipSwitch(dipSwitch);

            Data.Models.Metadata.Disk[]? disks = machine.ReadItemArray<Data.Models.Metadata.Disk>(Data.Models.Metadata.Machine.DiskKey);
            Assert.NotNull(disks);
            Assert.Equal(2, disks.Length);
            Data.Models.Metadata.Disk disk = disks[0];
            ValidateMetadataDisk(disk);

            Data.Models.Metadata.Display[]? displays = machine.ReadItemArray<Data.Models.Metadata.Display>(Data.Models.Metadata.Machine.DisplayKey);
            Assert.NotNull(displays);
            Assert.Equal(2, displays.Length);
            Data.Models.Metadata.Display? display = Array.Find(displays, d => !d.ContainsKey(Data.Models.Metadata.Video.AspectXKey));
            ValidateMetadataDisplay(display);

            Data.Models.Metadata.Driver[]? drivers = machine.ReadItemArray<Data.Models.Metadata.Driver>(Data.Models.Metadata.Machine.DriverKey);
            Assert.NotNull(drivers);
            Data.Models.Metadata.Driver driver = Assert.Single(drivers);
            ValidateMetadataDriver(driver);

            // TODO: Implement this validation
            // Data.Models.Metadata.Dump[]? dumps = machine.ReadItemArray<Data.Models.Metadata.Dump>(Data.Models.Metadata.Machine.DumpKey);
            // Assert.NotNull(dumps);
            // Data.Models.Metadata.Dump dump = Assert.Single(dumps);
            // ValidateMetadataDump(dump);

            Data.Models.Metadata.Feature[]? features = machine.ReadItemArray<Data.Models.Metadata.Feature>(Data.Models.Metadata.Machine.FeatureKey);
            Assert.NotNull(features);
            Assert.Equal(2, features.Length);
            Data.Models.Metadata.Feature feature = features[0];
            ValidateMetadataFeature(feature);

            Data.Models.Metadata.Info[]? infos = machine.ReadItemArray<Data.Models.Metadata.Info>(Data.Models.Metadata.Machine.InfoKey);
            Assert.NotNull(infos);
            Data.Models.Metadata.Info info = Assert.Single(infos);
            ValidateMetadataInfo(info);

            Data.Models.Metadata.Input[]? inputs = machine.ReadItemArray<Data.Models.Metadata.Input>(Data.Models.Metadata.Machine.InputKey);
            Assert.NotNull(inputs);
            Data.Models.Metadata.Input input = Assert.Single(inputs);
            ValidateMetadataInput(input);

            Data.Models.Metadata.Media[]? media = machine.ReadItemArray<Data.Models.Metadata.Media>(Data.Models.Metadata.Machine.MediaKey);
            Assert.NotNull(media);
            Data.Models.Metadata.Media medium = Assert.Single(media);
            ValidateMetadataMedia(medium);

            Data.Models.Metadata.Part[]? parts = machine.ReadItemArray<Data.Models.Metadata.Part>(Data.Models.Metadata.Machine.PartKey);
            Assert.NotNull(parts);
            Data.Models.Metadata.Part part = Assert.Single(parts);
            ValidateMetadataPart(part);

            Data.Models.Metadata.Port[]? ports = machine.ReadItemArray<Data.Models.Metadata.Port>(Data.Models.Metadata.Machine.PortKey);
            Assert.NotNull(ports);
            Data.Models.Metadata.Port port = Assert.Single(ports);
            ValidateMetadataPort(port);

            Data.Models.Metadata.RamOption[]? ramOptions = machine.ReadItemArray<Data.Models.Metadata.RamOption>(Data.Models.Metadata.Machine.RamOptionKey);
            Assert.NotNull(ramOptions);
            Data.Models.Metadata.RamOption ramOption = Assert.Single(ramOptions);
            ValidateMetadataRamOption(ramOption);

            Data.Models.Metadata.Release[]? releases = machine.ReadItemArray<Data.Models.Metadata.Release>(Data.Models.Metadata.Machine.ReleaseKey);
            Assert.NotNull(releases);
            Data.Models.Metadata.Release release = Assert.Single(releases);
            ValidateMetadataRelease(release);

            Data.Models.Metadata.Rom[]? roms = machine.ReadItemArray<Data.Models.Metadata.Rom>(Data.Models.Metadata.Machine.RomKey);
            Assert.NotNull(roms);
            Assert.Equal(2, roms.Length);
            Data.Models.Metadata.Rom rom = roms[0];
            ValidateMetadataRom(rom);

            Data.Models.Metadata.Sample[]? samples = machine.ReadItemArray<Data.Models.Metadata.Sample>(Data.Models.Metadata.Machine.SampleKey);
            Assert.NotNull(samples);
            Data.Models.Metadata.Sample sample = Assert.Single(samples);
            ValidateMetadataSample(sample);

            Data.Models.Metadata.SharedFeat[]? sharedFeats = machine.ReadItemArray<Data.Models.Metadata.SharedFeat>(Data.Models.Metadata.Machine.SharedFeatKey);
            Assert.NotNull(sharedFeats);
            Data.Models.Metadata.SharedFeat sharedFeat = Assert.Single(sharedFeats);
            ValidateMetadataSharedFeat(sharedFeat);

            Data.Models.Metadata.Slot[]? slots = machine.ReadItemArray<Data.Models.Metadata.Slot>(Data.Models.Metadata.Machine.SlotKey);
            Assert.NotNull(slots);
            Data.Models.Metadata.Slot slot = Assert.Single(slots);
            ValidateMetadataSlot(slot);

            Data.Models.Metadata.SoftwareList[]? softwareLists = machine.ReadItemArray<Data.Models.Metadata.SoftwareList>(Data.Models.Metadata.Machine.SoftwareListKey);
            Assert.NotNull(softwareLists);
            Data.Models.Metadata.SoftwareList softwareList = Assert.Single(softwareLists);
            ValidateMetadataSoftwareList(softwareList);

            Data.Models.Metadata.Sound[]? sounds = machine.ReadItemArray<Data.Models.Metadata.Sound>(Data.Models.Metadata.Machine.SoundKey);
            Assert.NotNull(sounds);
            Data.Models.Metadata.Sound sound = Assert.Single(sounds);
            ValidateMetadataSound(sound);

            Data.Models.Logiqx.Trurip? trurip = machine.Read<Data.Models.Logiqx.Trurip>(Data.Models.Metadata.Machine.TruripKey);
            ValidateMetadataTrurip(trurip);

            Data.Models.Metadata.Video[]? videos = machine.ReadItemArray<Data.Models.Metadata.Video>(Data.Models.Metadata.Machine.VideoKey);
            Assert.NotNull(videos);
            Data.Models.Metadata.Video video = Assert.Single(videos);
            ValidateMetadataVideo(video);
        }

        private static void ValidateMetadataAdjuster(Data.Models.Metadata.Adjuster? adjuster)
        {
            Assert.NotNull(adjuster);
            Assert.True(adjuster.ReadBool(Data.Models.Metadata.Adjuster.DefaultKey));
            Assert.Equal("name", adjuster.ReadString(Data.Models.Metadata.Adjuster.NameKey));

            Data.Models.Metadata.Condition? condition = adjuster.Read<Data.Models.Metadata.Condition>(Data.Models.Metadata.Adjuster.ConditionKey);
            ValidateMetadataCondition(condition);
        }

        private static void ValidateMetadataAnalog(Data.Models.Metadata.Analog? analog)
        {
            Assert.NotNull(analog);
            Assert.Equal("mask", analog.ReadString(Data.Models.Metadata.Analog.MaskKey));
        }

        private static void ValidateMetadataArchive(Data.Models.Metadata.Archive? archive)
        {
            Assert.NotNull(archive);
            Assert.Equal("name", archive.ReadString(Data.Models.Metadata.Archive.NameKey));
        }

        private static void ValidateMetadataBiosSet(Data.Models.Metadata.BiosSet? biosSet)
        {
            Assert.NotNull(biosSet);
            Assert.True(biosSet.ReadBool(Data.Models.Metadata.BiosSet.DefaultKey));
            Assert.Equal("description", biosSet.ReadString(Data.Models.Metadata.BiosSet.DescriptionKey));
            Assert.Equal("name", biosSet.ReadString(Data.Models.Metadata.BiosSet.NameKey));
        }

        private static void ValidateMetadataChip(Data.Models.Metadata.Chip? chip)
        {
            Assert.NotNull(chip);
            Assert.Equal(12345, chip.ReadLong(Data.Models.Metadata.Chip.ClockKey));
            Assert.Equal("flags", chip.ReadString(Data.Models.Metadata.Chip.FlagsKey));
            Assert.Equal("name", chip.ReadString(Data.Models.Metadata.Chip.NameKey));
            Assert.True(chip.ReadBool(Data.Models.Metadata.Chip.SoundOnlyKey));
            Assert.Equal("tag", chip.ReadString(Data.Models.Metadata.Chip.TagKey));
            Assert.Equal("cpu", chip.ReadString(Data.Models.Metadata.Chip.ChipTypeKey));
        }

        private static void ValidateMetadataCondition(Data.Models.Metadata.Condition? condition)
        {
            Assert.NotNull(condition);
            Assert.Equal("value", condition.ReadString(Data.Models.Metadata.Condition.ValueKey));
            Assert.Equal("mask", condition.ReadString(Data.Models.Metadata.Condition.MaskKey));
            Assert.Equal("eq", condition.ReadString(Data.Models.Metadata.Condition.RelationKey));
            Assert.Equal("tag", condition.ReadString(Data.Models.Metadata.Condition.TagKey));
        }

        private static void ValidateMetadataConfiguration(Data.Models.Metadata.Configuration? configuration)
        {
            Assert.NotNull(configuration);
            Assert.Equal("mask", configuration.ReadString(Data.Models.Metadata.Configuration.MaskKey));
            Assert.Equal("name", configuration.ReadString(Data.Models.Metadata.Configuration.NameKey));
            Assert.Equal("tag", configuration.ReadString(Data.Models.Metadata.Configuration.TagKey));

            Data.Models.Metadata.Condition? condition = configuration.Read<Data.Models.Metadata.Condition>(Data.Models.Metadata.Configuration.ConditionKey);
            ValidateMetadataCondition(condition);

            Data.Models.Metadata.ConfLocation[]? confLocations = configuration.ReadItemArray<Data.Models.Metadata.ConfLocation>(Data.Models.Metadata.Configuration.ConfLocationKey);
            Assert.NotNull(confLocations);
            Data.Models.Metadata.ConfLocation? confLocation = Assert.Single(confLocations);
            ValidateMetadataConfLocation(confLocation);

            Data.Models.Metadata.ConfSetting[]? confSettings = configuration.ReadItemArray<Data.Models.Metadata.ConfSetting>(Data.Models.Metadata.Configuration.ConfSettingKey);
            Assert.NotNull(confSettings);
            Data.Models.Metadata.ConfSetting? confSetting = Assert.Single(confSettings);
            ValidateMetadataConfSetting(confSetting);
        }

        private static void ValidateMetadataConfLocation(Data.Models.Metadata.ConfLocation? confLocation)
        {
            Assert.NotNull(confLocation);
            Assert.True(confLocation.ReadBool(Data.Models.Metadata.ConfLocation.InvertedKey));
            Assert.Equal("name", confLocation.ReadString(Data.Models.Metadata.ConfLocation.NameKey));
            Assert.Equal("number", confLocation.ReadString(Data.Models.Metadata.ConfLocation.NumberKey));
        }

        private static void ValidateMetadataConfSetting(Data.Models.Metadata.ConfSetting? confSetting)
        {
            Assert.NotNull(confSetting);
            Assert.True(confSetting.ReadBool(Data.Models.Metadata.ConfSetting.DefaultKey));
            Assert.Equal("name", confSetting.ReadString(Data.Models.Metadata.ConfSetting.NameKey));
            Assert.Equal("value", confSetting.ReadString(Data.Models.Metadata.ConfSetting.ValueKey));

            Data.Models.Metadata.Condition? condition = confSetting.Read<Data.Models.Metadata.Condition>(Data.Models.Metadata.ConfSetting.ConditionKey);
            ValidateMetadataCondition(condition);
        }

        private static void ValidateMetadataControl(Data.Models.Metadata.Control? control)
        {
            Assert.NotNull(control);
            Assert.Equal(12345, control.ReadLong(Data.Models.Metadata.Control.ButtonsKey));
            Assert.Equal(12345, control.ReadLong(Data.Models.Metadata.Control.KeyDeltaKey));
            Assert.Equal(12345, control.ReadLong(Data.Models.Metadata.Control.MaximumKey));
            Assert.Equal(12345, control.ReadLong(Data.Models.Metadata.Control.MinimumKey));
            Assert.Equal(12345, control.ReadLong(Data.Models.Metadata.Control.PlayerKey));
            Assert.Equal(12345, control.ReadLong(Data.Models.Metadata.Control.ReqButtonsKey));
            Assert.True(control.ReadBool(Data.Models.Metadata.Control.ReverseKey));
            Assert.Equal(12345, control.ReadLong(Data.Models.Metadata.Control.SensitivityKey));
            Assert.Equal("lightgun", control.ReadString(Data.Models.Metadata.Control.ControlTypeKey));
            Assert.Equal("ways", control.ReadString(Data.Models.Metadata.Control.WaysKey));
            Assert.Equal("ways2", control.ReadString(Data.Models.Metadata.Control.Ways2Key));
            Assert.Equal("ways3", control.ReadString(Data.Models.Metadata.Control.Ways3Key));
        }

        private static void ValidateMetadataDataArea(Data.Models.Metadata.DataArea? dataArea)
        {
            Assert.NotNull(dataArea);
            Assert.Equal("big", dataArea.ReadString(Data.Models.Metadata.DataArea.EndiannessKey));
            Assert.Equal("name", dataArea.ReadString(Data.Models.Metadata.DataArea.NameKey));
            Assert.Equal(12345, dataArea.ReadLong(Data.Models.Metadata.DataArea.SizeKey));
            Assert.Equal(64, dataArea.ReadLong(Data.Models.Metadata.DataArea.WidthKey));
        
            Data.Models.Metadata.Rom[]? roms = dataArea.ReadItemArray<Data.Models.Metadata.Rom>(Data.Models.Metadata.DataArea.RomKey);
            Assert.NotNull(roms);
            Data.Models.Metadata.Rom? rom = Assert.Single(roms);
            ValidateMetadataRom(rom);
        }

        private static void ValidateMetadataDevice(Data.Models.Metadata.Device? device)
        {
            Assert.NotNull(device);
            Assert.Equal("fixedimage", device.ReadString(Data.Models.Metadata.Device.FixedImageKey));
            Assert.Equal("interface", device.ReadString(Data.Models.Metadata.Device.InterfaceKey));
            Assert.Equal(1, device.ReadLong(Data.Models.Metadata.Device.MandatoryKey));
            Assert.Equal("tag", device.ReadString(Data.Models.Metadata.Device.TagKey));
            Assert.Equal("punchtape", device.ReadString(Data.Models.Metadata.Device.DeviceTypeKey));

            Data.Models.Metadata.Extension[]? extensions = device.ReadItemArray<Data.Models.Metadata.Extension>(Data.Models.Metadata.Device.ExtensionKey);
            Assert.NotNull(extensions);
            Data.Models.Metadata.Extension? extension = Assert.Single(extensions);
            ValidateMetadataExtension(extension);

            Data.Models.Metadata.Instance? instance = device.Read<Data.Models.Metadata.Instance>(Data.Models.Metadata.Device.InstanceKey);
            ValidateMetadataInstance(instance);
        }

        private static void ValidateMetadataDeviceRef(Data.Models.Metadata.DeviceRef? deviceRef)
        {
            Assert.NotNull(deviceRef);
            Assert.Equal("name", deviceRef.ReadString(Data.Models.Metadata.DeviceRef.NameKey));
        }

        private static void ValidateMetadataDipLocation(Data.Models.Metadata.DipLocation? dipLocation)
        {
            Assert.NotNull(dipLocation);
            Assert.True(dipLocation.ReadBool(Data.Models.Metadata.DipLocation.InvertedKey));
            Assert.Equal("name", dipLocation.ReadString(Data.Models.Metadata.DipLocation.NameKey));
            Assert.Equal("number", dipLocation.ReadString(Data.Models.Metadata.DipLocation.NumberKey));
        }

        private static void ValidateMetadataDipSwitch(Data.Models.Metadata.DipSwitch? dipSwitch)
        {
            Assert.NotNull(dipSwitch);
            Assert.True(dipSwitch.ReadBool(Data.Models.Metadata.DipSwitch.DefaultKey));
            Assert.Equal("mask", dipSwitch.ReadString(Data.Models.Metadata.DipSwitch.MaskKey));
            Assert.Equal("name", dipSwitch.ReadString(Data.Models.Metadata.DipSwitch.NameKey));
            Assert.Equal("tag", dipSwitch.ReadString(Data.Models.Metadata.DipSwitch.TagKey));

            Data.Models.Metadata.Condition? condition = dipSwitch.Read<Data.Models.Metadata.Condition>(Data.Models.Metadata.DipSwitch.ConditionKey);
            ValidateMetadataCondition(condition);

            Data.Models.Metadata.DipLocation[]? dipLocations = dipSwitch.ReadItemArray<Data.Models.Metadata.DipLocation>(Data.Models.Metadata.DipSwitch.DipLocationKey);
            Assert.NotNull(dipLocations);
            Data.Models.Metadata.DipLocation? dipLocation = Assert.Single(dipLocations);
            ValidateMetadataDipLocation(dipLocation);

            Data.Models.Metadata.DipValue[]? dipValues = dipSwitch.ReadItemArray<Data.Models.Metadata.DipValue>(Data.Models.Metadata.DipSwitch.DipValueKey);
            Assert.NotNull(dipValues);
            Data.Models.Metadata.DipValue? dipValue = Assert.Single(dipValues);
            ValidateMetadataDipValue(dipValue);

            string[]? entries = dipSwitch.ReadStringArray(Data.Models.Metadata.DipSwitch.EntryKey);
            Assert.NotNull(entries);
            string entry = Assert.Single(entries);
            Assert.Equal("entry", entry);
        }

        private static void ValidateMetadataDipValue(Data.Models.Metadata.DipValue? dipValue)
        {
            Assert.NotNull(dipValue);
            Assert.True(dipValue.ReadBool(Data.Models.Metadata.DipValue.DefaultKey));
            Assert.Equal("name", dipValue.ReadString(Data.Models.Metadata.DipValue.NameKey));
            Assert.Equal("value", dipValue.ReadString(Data.Models.Metadata.DipValue.ValueKey));

            Data.Models.Metadata.Condition? condition = dipValue.Read<Data.Models.Metadata.Condition>(Data.Models.Metadata.DipValue.ConditionKey);
            ValidateMetadataCondition(condition);
        }

        private static void ValidateMetadataDisk(Data.Models.Metadata.Disk? disk)
        {
            Assert.NotNull(disk);
            Assert.Equal("flags", disk.ReadString(Data.Models.Metadata.Disk.FlagsKey));
            Assert.Equal("index", disk.ReadString(Data.Models.Metadata.Disk.IndexKey));
            Assert.Equal(ZeroHash.MD5Str, disk.ReadString(Data.Models.Metadata.Disk.MD5Key));
            Assert.Equal("merge", disk.ReadString(Data.Models.Metadata.Disk.MergeKey));
            Assert.Equal("name", disk.ReadString(Data.Models.Metadata.Disk.NameKey));
            Assert.True(disk.ReadBool(Data.Models.Metadata.Disk.OptionalKey));
            Assert.Equal("region", disk.ReadString(Data.Models.Metadata.Disk.RegionKey));
            Assert.Equal(ZeroHash.SHA1Str, disk.ReadString(Data.Models.Metadata.Disk.SHA1Key));
            Assert.True(disk.ReadBool(Data.Models.Metadata.Disk.WritableKey));
        }

        private static void ValidateMetadataDiskArea(Data.Models.Metadata.DiskArea? diskArea)
        {
            Assert.NotNull(diskArea);
            Assert.Equal("name", diskArea.ReadString(Data.Models.Metadata.DiskArea.NameKey));
        
            Data.Models.Metadata.Disk[]? disks = diskArea.ReadItemArray<Data.Models.Metadata.Disk>(Data.Models.Metadata.DiskArea.DiskKey);
            Assert.NotNull(disks);
            Data.Models.Metadata.Disk? disk = Assert.Single(disks);
            ValidateMetadataDisk(disk);
        }

        private static void ValidateMetadataDisplay(Data.Models.Metadata.Display? display)
        {
            Assert.NotNull(display);
            Assert.True(display.ReadBool(Data.Models.Metadata.Display.FlipXKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.HBEndKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.HBStartKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.HeightKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.HTotalKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.PixClockKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.RefreshKey));
            Assert.Equal(90, display.ReadLong(Data.Models.Metadata.Display.RotateKey));
            Assert.Equal("tag", display.ReadString(Data.Models.Metadata.Display.TagKey));
            Assert.Equal("vector", display.ReadString(Data.Models.Metadata.Display.DisplayTypeKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.VBEndKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.VBStartKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.VTotalKey));
            Assert.Equal(12345, display.ReadLong(Data.Models.Metadata.Display.WidthKey));
        }

        private static void ValidateMetadataDriver(Data.Models.Metadata.Driver? driver)
        {
            Assert.NotNull(driver);
            Assert.Equal("plain", driver.ReadString(Data.Models.Metadata.Driver.BlitKey));
            Assert.Equal("good", driver.ReadString(Data.Models.Metadata.Driver.CocktailKey));
            Assert.Equal("good", driver.ReadString(Data.Models.Metadata.Driver.ColorKey));
            Assert.Equal("good", driver.ReadString(Data.Models.Metadata.Driver.EmulationKey));
            Assert.True(driver.ReadBool(Data.Models.Metadata.Driver.IncompleteKey));
            Assert.True(driver.ReadBool(Data.Models.Metadata.Driver.NoSoundHardwareKey));
            Assert.Equal("pallettesize", driver.ReadString(Data.Models.Metadata.Driver.PaletteSizeKey));
            Assert.True(driver.ReadBool(Data.Models.Metadata.Driver.RequiresArtworkKey));
            Assert.Equal("supported", driver.ReadString(Data.Models.Metadata.Driver.SaveStateKey));
            Assert.Equal("good", driver.ReadString(Data.Models.Metadata.Driver.SoundKey));
            Assert.Equal("good", driver.ReadString(Data.Models.Metadata.Driver.StatusKey));
            Assert.True(driver.ReadBool(Data.Models.Metadata.Driver.UnofficialKey));
        }

        private static void ValidateMetadataExtension(Data.Models.Metadata.Extension? extension)
        {
            Assert.NotNull(extension);
            Assert.Equal("name", extension.ReadString(Data.Models.Metadata.Extension.NameKey));
        }

        private static void ValidateMetadataFeature(Data.Models.Metadata.Feature? feature)
        {
            Assert.NotNull(feature);
            Assert.Equal("name", feature.ReadString(Data.Models.Metadata.Feature.NameKey));
            Assert.Equal("imperfect", feature.ReadString(Data.Models.Metadata.Feature.OverallKey));
            Assert.Equal("imperfect", feature.ReadString(Data.Models.Metadata.Feature.StatusKey));
            Assert.Equal("protection", feature.ReadString(Data.Models.Metadata.Feature.FeatureTypeKey));
            Assert.Equal("value", feature.ReadString(Data.Models.Metadata.Feature.ValueKey));
        }

        private static void ValidateMetadataInfo(Data.Models.Metadata.Info? info)
        {
            Assert.NotNull(info);
            Assert.Equal("name", info.ReadString(Data.Models.Metadata.Info.NameKey));
            Assert.Equal("value", info.ReadString(Data.Models.Metadata.Info.ValueKey));
        }

        private static void ValidateMetadataInput(Data.Models.Metadata.Input? input)
        {
            Assert.NotNull(input);
            Assert.Equal(12345, input.ReadLong(Data.Models.Metadata.Input.ButtonsKey));
            Assert.Equal(12345, input.ReadLong(Data.Models.Metadata.Input.CoinsKey));
            Assert.Equal(12345, input.ReadLong(Data.Models.Metadata.Input.PlayersKey));
            Assert.True(input.ReadBool(Data.Models.Metadata.Input.ServiceKey));
            Assert.True(input.ReadBool(Data.Models.Metadata.Input.TiltKey));

            Data.Models.Metadata.Control[]? controls = input.ReadItemArray<Data.Models.Metadata.Control>(Data.Models.Metadata.Input.ControlKey);
            Assert.NotNull(controls);
            Data.Models.Metadata.Control? control = Assert.Single(controls);
            ValidateMetadataControl(control);
        }

        private static void ValidateMetadataInstance(Data.Models.Metadata.Instance? instance)
        {
            Assert.NotNull(instance);
            Assert.Equal("briefname", instance.ReadString(Data.Models.Metadata.Instance.BriefNameKey));
            Assert.Equal("name", instance.ReadString(Data.Models.Metadata.Instance.NameKey));
        }

        private static void ValidateMetadataMedia(Data.Models.Metadata.Media? media)
        {
            Assert.NotNull(media);
            Assert.Equal(ZeroHash.MD5Str, media.ReadString(Data.Models.Metadata.Media.MD5Key));
            Assert.Equal("name", media.ReadString(Data.Models.Metadata.Media.NameKey));
            Assert.Equal(ZeroHash.SHA1Str, media.ReadString(Data.Models.Metadata.Media.SHA1Key));
            Assert.Equal(ZeroHash.SHA256Str, media.ReadString(Data.Models.Metadata.Media.SHA256Key));
            Assert.Equal(ZeroHash.SpamSumStr, media.ReadString(Data.Models.Metadata.Media.SpamSumKey));
        }

        private static void ValidateMetadataPart(Data.Models.Metadata.Part? part)
        {
            Assert.NotNull(part);
            Assert.Equal("interface", part.ReadString(Data.Models.Metadata.Part.InterfaceKey));
            Assert.Equal("name", part.ReadString(Data.Models.Metadata.Part.NameKey));

            Data.Models.Metadata.DataArea[]? dataAreas = part.ReadItemArray<Data.Models.Metadata.DataArea>(Data.Models.Metadata.Part.DataAreaKey);
            Assert.NotNull(dataAreas);
            Data.Models.Metadata.DataArea? dataArea = Assert.Single(dataAreas);
            ValidateMetadataDataArea(dataArea);

            Data.Models.Metadata.DiskArea[]? diskAreas = part.ReadItemArray<Data.Models.Metadata.DiskArea>(Data.Models.Metadata.Part.DiskAreaKey);
            Assert.NotNull(diskAreas);
            Data.Models.Metadata.DiskArea? diskArea = Assert.Single(diskAreas);
            ValidateMetadataDiskArea(diskArea);

            Data.Models.Metadata.DipSwitch[]? dipSwitches = part.ReadItemArray<Data.Models.Metadata.DipSwitch>(Data.Models.Metadata.Part.DipSwitchKey);
            Assert.NotNull(dipSwitches);
            Data.Models.Metadata.DipSwitch? dipSwitch = Assert.Single(dipSwitches);
            ValidateMetadataDipSwitch(dipSwitch);

            Data.Models.Metadata.Feature[]? features = part.ReadItemArray<Data.Models.Metadata.Feature>(Data.Models.Metadata.Part.FeatureKey);
            Assert.NotNull(features);
            Data.Models.Metadata.Feature? feature = Assert.Single(features);
            ValidateMetadataFeature(feature);
        }

        private static void ValidateMetadataPort(Data.Models.Metadata.Port? port)
        {
            Assert.NotNull(port);
            Assert.Equal("tag", port.ReadString(Data.Models.Metadata.Port.TagKey));

            Data.Models.Metadata.Analog[]? dipValues = port.ReadItemArray<Data.Models.Metadata.Analog>(Data.Models.Metadata.Port.AnalogKey);
            Assert.NotNull(dipValues);
            Data.Models.Metadata.Analog? dipValue = Assert.Single(dipValues);
            ValidateMetadataAnalog(dipValue);
        }

        private static void ValidateMetadataRamOption(Data.Models.Metadata.RamOption? ramOption)
        {
            Assert.NotNull(ramOption);
            Assert.Equal("content", ramOption.ReadString(Data.Models.Metadata.RamOption.ContentKey));
            Assert.True(ramOption.ReadBool(Data.Models.Metadata.RamOption.DefaultKey));
            Assert.Equal("name", ramOption.ReadString(Data.Models.Metadata.RamOption.NameKey));
        }

        private static void ValidateMetadataRelease(Data.Models.Metadata.Release? release)
        {
            Assert.NotNull(release);
            Assert.Equal("date", release.ReadString(Data.Models.Metadata.Release.DateKey));
            Assert.True(release.ReadBool(Data.Models.Metadata.Release.DefaultKey));
            Assert.Equal("language", release.ReadString(Data.Models.Metadata.Release.LanguageKey));
            Assert.Equal("name", release.ReadString(Data.Models.Metadata.Release.NameKey));
            Assert.Equal("region", release.ReadString(Data.Models.Metadata.Release.RegionKey));
        }

        private static void ValidateMetadataRom(Data.Models.Metadata.Rom? rom)
        {
            Assert.NotNull(rom);
            Assert.Equal("album", rom.ReadString(Data.Models.Metadata.Rom.AlbumKey));
            Assert.Equal("alt_romname", rom.ReadString(Data.Models.Metadata.Rom.AltRomnameKey));
            Assert.Equal("alt_title", rom.ReadString(Data.Models.Metadata.Rom.AltTitleKey));
            Assert.Equal("artist", rom.ReadString(Data.Models.Metadata.Rom.ArtistKey));
            Assert.Equal("asr_detected_lang", rom.ReadString(Data.Models.Metadata.Rom.ASRDetectedLangKey));
            Assert.Equal("asr_detected_lang_conf", rom.ReadString(Data.Models.Metadata.Rom.ASRDetectedLangConfKey));
            Assert.Equal("asr_transcribed_lang", rom.ReadString(Data.Models.Metadata.Rom.ASRTranscribedLangKey));
            Assert.Equal("bios", rom.ReadString(Data.Models.Metadata.Rom.BiosKey));
            Assert.Equal("bitrate", rom.ReadString(Data.Models.Metadata.Rom.BitrateKey));
            Assert.Equal("btih", rom.ReadString(Data.Models.Metadata.Rom.BitTorrentMagnetHashKey));
            Assert.Equal("cloth_cover_detection_module_version", rom.ReadString(Data.Models.Metadata.Rom.ClothCoverDetectionModuleVersionKey));
            Assert.Equal("collection-catalog-number", rom.ReadString(Data.Models.Metadata.Rom.CollectionCatalogNumberKey));
            Assert.Equal("comment", rom.ReadString(Data.Models.Metadata.Rom.CommentKey));
            Assert.Equal(ZeroHash.CRC32Str, rom.ReadString(Data.Models.Metadata.Rom.CRCKey));
            Assert.Equal("creator", rom.ReadString(Data.Models.Metadata.Rom.CreatorKey));
            Assert.Equal("date", rom.ReadString(Data.Models.Metadata.Rom.DateKey));
            Assert.True(rom.ReadBool(Data.Models.Metadata.Rom.DisposeKey));
            Assert.Equal("extension", rom.ReadString(Data.Models.Metadata.Rom.ExtensionKey));
            Assert.Equal(12345, rom.ReadLong(Data.Models.Metadata.Rom.FileCountKey));
            Assert.True(rom.ReadBool(Data.Models.Metadata.Rom.FileIsAvailableKey));
            Assert.Equal("flags", rom.ReadString(Data.Models.Metadata.Rom.FlagsKey));
            Assert.Equal("format", rom.ReadString(Data.Models.Metadata.Rom.FormatKey));
            Assert.Equal("header", rom.ReadString(Data.Models.Metadata.Rom.HeaderKey));
            Assert.Equal("height", rom.ReadString(Data.Models.Metadata.Rom.HeightKey));
            Assert.Equal("hocr_char_to_word_hocr_version", rom.ReadString(Data.Models.Metadata.Rom.hOCRCharToWordhOCRVersionKey));
            Assert.Equal("hocr_char_to_word_module_version", rom.ReadString(Data.Models.Metadata.Rom.hOCRCharToWordModuleVersionKey));
            Assert.Equal("hocr_fts_text_hocr_version", rom.ReadString(Data.Models.Metadata.Rom.hOCRFtsTexthOCRVersionKey));
            Assert.Equal("hocr_fts_text_module_version", rom.ReadString(Data.Models.Metadata.Rom.hOCRFtsTextModuleVersionKey));
            Assert.Equal("hocr_pageindex_hocr_version", rom.ReadString(Data.Models.Metadata.Rom.hOCRPageIndexhOCRVersionKey));
            Assert.Equal("hocr_pageindex_module_version", rom.ReadString(Data.Models.Metadata.Rom.hOCRPageIndexModuleVersionKey));
            Assert.True(rom.ReadBool(Data.Models.Metadata.Rom.InvertedKey));
            Assert.Equal("mtime", rom.ReadString(Data.Models.Metadata.Rom.LastModifiedTimeKey));
            Assert.Equal("length", rom.ReadString(Data.Models.Metadata.Rom.LengthKey));
            Assert.Equal("load16_byte", rom.ReadString(Data.Models.Metadata.Rom.LoadFlagKey));
            Assert.Equal("matrix_number", rom.ReadString(Data.Models.Metadata.Rom.MatrixNumberKey));
            Assert.Equal(ZeroHash.GetString(HashType.MD2), rom.ReadString(Data.Models.Metadata.Rom.MD2Key));
            Assert.Equal(ZeroHash.GetString(HashType.MD4), rom.ReadString(Data.Models.Metadata.Rom.MD4Key));
            Assert.Equal(ZeroHash.MD5Str, rom.ReadString(Data.Models.Metadata.Rom.MD5Key));
            Assert.Null(rom.ReadString(Data.Models.Metadata.Rom.OpenMSXMediaType)); // Omit due to other test
            Assert.Equal("merge", rom.ReadString(Data.Models.Metadata.Rom.MergeKey));
            Assert.True(rom.ReadBool(Data.Models.Metadata.Rom.MIAKey));
            Assert.Equal("name", rom.ReadString(Data.Models.Metadata.Rom.NameKey));
            Assert.Equal("ocr", rom.ReadString(Data.Models.Metadata.Rom.TesseractOCRKey));
            Assert.Equal("ocr_converted", rom.ReadString(Data.Models.Metadata.Rom.TesseractOCRConvertedKey));
            Assert.Equal("ocr_detected_lang", rom.ReadString(Data.Models.Metadata.Rom.TesseractOCRDetectedLangKey));
            Assert.Equal("ocr_detected_lang_conf", rom.ReadString(Data.Models.Metadata.Rom.TesseractOCRDetectedLangConfKey));
            Assert.Equal("ocr_detected_script", rom.ReadString(Data.Models.Metadata.Rom.TesseractOCRDetectedScriptKey));
            Assert.Equal("ocr_detected_script_conf", rom.ReadString(Data.Models.Metadata.Rom.TesseractOCRDetectedScriptConfKey));
            Assert.Equal("ocr_module_version", rom.ReadString(Data.Models.Metadata.Rom.TesseractOCRModuleVersionKey));
            Assert.Equal("ocr_parameters", rom.ReadString(Data.Models.Metadata.Rom.TesseractOCRParametersKey));
            Assert.Equal("offset", rom.ReadString(Data.Models.Metadata.Rom.OffsetKey));
            Assert.True(rom.ReadBool(Data.Models.Metadata.Rom.OptionalKey));
            Assert.Equal("original", rom.ReadString(Data.Models.Metadata.Rom.OriginalKey));
            Assert.Equal("pdf_module_version", rom.ReadString(Data.Models.Metadata.Rom.PDFModuleVersionKey));
            Assert.Equal("preview-image", rom.ReadString(Data.Models.Metadata.Rom.PreviewImageKey));
            Assert.Equal("publisher", rom.ReadString(Data.Models.Metadata.Rom.PublisherKey));
            Assert.Equal("region", rom.ReadString(Data.Models.Metadata.Rom.RegionKey));
            Assert.Equal("remark", rom.ReadString(Data.Models.Metadata.Rom.RemarkKey));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD128), rom.ReadString(Data.Models.Metadata.Rom.RIPEMD128Key));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD160), rom.ReadString(Data.Models.Metadata.Rom.RIPEMD160Key));
            Assert.Equal("rotation", rom.ReadString(Data.Models.Metadata.Rom.RotationKey));
            Assert.Equal("serial", rom.ReadString(Data.Models.Metadata.Rom.SerialKey));
            Assert.Equal(ZeroHash.SHA1Str, rom.ReadString(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal(ZeroHash.SHA256Str, rom.ReadString(Data.Models.Metadata.Rom.SHA256Key));
            Assert.Equal(ZeroHash.SHA384Str, rom.ReadString(Data.Models.Metadata.Rom.SHA384Key));
            Assert.Equal(ZeroHash.SHA512Str, rom.ReadString(Data.Models.Metadata.Rom.SHA512Key));
            Assert.Equal(12345, rom.ReadLong(Data.Models.Metadata.Rom.SizeKey));
            Assert.True(rom.ReadBool(Data.Models.Metadata.Rom.SoundOnlyKey));
            Assert.Equal("source", rom.ReadString(Data.Models.Metadata.Rom.SourceKey));
            Assert.Equal(ZeroHash.SpamSumStr, rom.ReadString(Data.Models.Metadata.Rom.SpamSumKey));
            Assert.Equal("start", rom.ReadString(Data.Models.Metadata.Rom.StartKey));
            Assert.Equal("good", rom.ReadString(Data.Models.Metadata.Rom.StatusKey));
            Assert.Equal("summation", rom.ReadString(Data.Models.Metadata.Rom.SummationKey));
            Assert.Equal("title", rom.ReadString(Data.Models.Metadata.Rom.TitleKey));
            Assert.Equal("track", rom.ReadString(Data.Models.Metadata.Rom.TrackKey));
            Assert.Equal("type", rom.ReadString(Data.Models.Metadata.Rom.OpenMSXType));
            Assert.Equal("value", rom.ReadString(Data.Models.Metadata.Rom.ValueKey));
            Assert.Equal("whisper_asr_module_version", rom.ReadString(Data.Models.Metadata.Rom.WhisperASRModuleVersionKey));
            Assert.Equal("whisper_model_hash", rom.ReadString(Data.Models.Metadata.Rom.WhisperModelHashKey));
            Assert.Equal("whisper_model_name", rom.ReadString(Data.Models.Metadata.Rom.WhisperModelNameKey));
            Assert.Equal("whisper_version", rom.ReadString(Data.Models.Metadata.Rom.WhisperVersionKey));
            Assert.Equal("width", rom.ReadString(Data.Models.Metadata.Rom.WidthKey));
            Assert.Equal("word_conf_0_10", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval0To10Key));
            Assert.Equal("word_conf_11_20", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval11To20Key));
            Assert.Equal("word_conf_21_30", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval21To30Key));
            Assert.Equal("word_conf_31_40", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval31To40Key));
            Assert.Equal("word_conf_41_50", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval41To50Key));
            Assert.Equal("word_conf_51_60", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval51To60Key));
            Assert.Equal("word_conf_61_70", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval61To70Key));
            Assert.Equal("word_conf_71_80", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval71To80Key));
            Assert.Equal("word_conf_81_90", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval81To90Key));
            Assert.Equal("word_conf_91_100", rom.ReadString(Data.Models.Metadata.Rom.WordConfidenceInterval91To100Key));
            Assert.Equal(ZeroHash.GetString(HashType.XxHash3), rom.ReadString(Data.Models.Metadata.Rom.xxHash364Key));
            Assert.Equal(ZeroHash.GetString(HashType.XxHash128), rom.ReadString(Data.Models.Metadata.Rom.xxHash3128Key));
        }

        private static void ValidateMetadataSample(Data.Models.Metadata.Sample? sample)
        {
            Assert.NotNull(sample);
            Assert.Equal("name", sample.ReadString(Data.Models.Metadata.Sample.NameKey));
        }

        private static void ValidateMetadataSharedFeat(Data.Models.Metadata.SharedFeat? sharedFeat)
        {
            Assert.NotNull(sharedFeat);
            Assert.Equal("name", sharedFeat.ReadString(Data.Models.Metadata.SharedFeat.NameKey));
            Assert.Equal("value", sharedFeat.ReadString(Data.Models.Metadata.SharedFeat.ValueKey));
        }

        private static void ValidateMetadataSlot(Data.Models.Metadata.Slot? slot)
        {
            Assert.NotNull(slot);
            Assert.Equal("name", slot.ReadString(Data.Models.Metadata.Slot.NameKey));

            Data.Models.Metadata.SlotOption[]? slotOptions = slot.ReadItemArray<Data.Models.Metadata.SlotOption>(Data.Models.Metadata.Slot.SlotOptionKey);
            Assert.NotNull(slotOptions);
            Data.Models.Metadata.SlotOption? slotOption = Assert.Single(slotOptions);
            ValidateMetadataSlotOption(slotOption);
        }

        private static void ValidateMetadataSlotOption(Data.Models.Metadata.SlotOption? slotOption)
        {
            Assert.NotNull(slotOption);
            Assert.True(slotOption.ReadBool(Data.Models.Metadata.SlotOption.DefaultKey));
            Assert.Equal("devname", slotOption.ReadString(Data.Models.Metadata.SlotOption.DevNameKey));
            Assert.Equal("name", slotOption.ReadString(Data.Models.Metadata.SlotOption.NameKey));
        }

        private static void ValidateMetadataSoftwareList(Data.Models.Metadata.SoftwareList? softwareList)
        {
            Assert.NotNull(softwareList);
            Assert.Equal("description", softwareList.ReadString(Data.Models.Metadata.SoftwareList.DescriptionKey));
            Assert.Equal("filter", softwareList.ReadString(Data.Models.Metadata.SoftwareList.FilterKey));
            Assert.Equal("name", softwareList.ReadString(Data.Models.Metadata.SoftwareList.NameKey));
            Assert.Equal("notes", softwareList.ReadString(Data.Models.Metadata.SoftwareList.NotesKey));
            Assert.Equal("original", softwareList.ReadString(Data.Models.Metadata.SoftwareList.StatusKey));
            Assert.Equal("tag", softwareList.ReadString(Data.Models.Metadata.SoftwareList.TagKey));

            // TODO: Figure out why Data.Models.Metadata.SoftwareList.SoftwareKey doesn't get processed
        }

        private static void ValidateMetadataSound(Data.Models.Metadata.Sound? sound)
        {
            Assert.NotNull(sound);
            Assert.Equal(12345, sound.ReadLong(Data.Models.Metadata.Sound.ChannelsKey));
        }

        private static void ValidateMetadataTrurip(Data.Models.Logiqx.Trurip? trurip)
        {
            Assert.NotNull(trurip);
            Assert.Equal("titleid", trurip.TitleID);
            Assert.Equal("publisher", trurip.Publisher);
            Assert.Equal("developer", trurip.Developer);
            Assert.Equal("year", trurip.Year);
            Assert.Equal("genre", trurip.Genre);
            Assert.Equal("subgenre", trurip.Subgenre);
            Assert.Equal("ratings", trurip.Ratings);
            Assert.Equal("score", trurip.Score);
            Assert.Equal("players", trurip.Players);
            Assert.Equal("enabled", trurip.Enabled);
            Assert.Equal("yes", trurip.CRC);
            Assert.Equal("sourcefile", trurip.Source);
            Assert.Equal("cloneof", trurip.CloneOf);
            Assert.Equal("relatedto", trurip.RelatedTo);
        }

        private static void ValidateMetadataVideo(Data.Models.Metadata.Video? video)
        {
            Assert.NotNull(video);
            Assert.Equal(12345, video.ReadLong(Data.Models.Metadata.Video.AspectXKey));
            Assert.Equal(12345, video.ReadLong(Data.Models.Metadata.Video.AspectYKey));
            Assert.Equal(12345, video.ReadLong(Data.Models.Metadata.Video.HeightKey));
            Assert.Equal("vertical", video.ReadString(Data.Models.Metadata.Video.OrientationKey));
            Assert.Equal(12345, video.ReadLong(Data.Models.Metadata.Video.RefreshKey));
            Assert.Equal("vector", video.ReadString(Data.Models.Metadata.Video.ScreenKey));
            Assert.Equal(12345, video.ReadLong(Data.Models.Metadata.Video.WidthKey));
        }

        #endregion
    }
}