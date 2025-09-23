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

            Models.Metadata.MetadataFile? actual = datFile.ConvertToMetadata();
            Assert.Null(actual);
        }

        [Fact]
        public void ConvertToMetadata_FilledHeader()
        {
            DatHeader header = CreateHeader();

            DatFile datFile = new Formats.Logiqx(null, useGame: false);
            datFile.SetHeader(header);
            datFile.AddItem(new Rom(), statsOnly: false);

            Models.Metadata.MetadataFile? actual = datFile.ConvertToMetadata();
            Assert.NotNull(actual);

            Models.Metadata.Header? actualHeader = actual.Read<Models.Metadata.Header>(Models.Metadata.MetadataFile.HeaderKey);
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

            Models.Metadata.MetadataFile? actual = datFile.ConvertToMetadata();
            Assert.NotNull(actual);

            Models.Metadata.Machine[]? machines = actual.ReadItemArray<Models.Metadata.Machine>(Models.Metadata.MetadataFile.MachineKey);
            Assert.NotNull(machines);
            Models.Metadata.Machine actualMachine = Assert.Single(machines);
            ValidateMetadataMachine(actualMachine);
        }

        #endregion

        #region Creation Helpers

        private static DatHeader CreateHeader()
        {
            DatHeader item = new DatHeader(CreateMetadataHeader());

            item.SetFieldValue<string[]>(Models.Metadata.Header.CanOpenKey, ["ext"]);
            item.SetFieldValue(Models.Metadata.Header.ImagesKey,
                new Models.OfflineList.Images() { Height = "height" });
            item.SetFieldValue(Models.Metadata.Header.InfosKey,
                new Models.OfflineList.Infos() { Comment = new Models.OfflineList.Comment() });
            item.SetFieldValue(Models.Metadata.Header.NewDatKey,
                new Models.OfflineList.NewDat() { DatUrl = new Models.OfflineList.DatUrl() });
            item.SetFieldValue(Models.Metadata.Header.SearchKey,
                new Models.OfflineList.Search() { To = [] });

            return item;
        }

        private static Machine CreateMachine()
        {
            Machine item = new Machine(CreateMetadataMachine());
            item.SetFieldValue(Models.Metadata.Machine.TruripKey, CreateTrurip());
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

        private static void ValidateMetadataHeader(Models.Metadata.Header? header)
        {
            Assert.NotNull(header);
            Assert.Equal("author", header.ReadString(Models.Metadata.Header.AuthorKey));
            Assert.Equal("merged", header.ReadString(Models.Metadata.Header.BiosModeKey));
            Assert.Equal("build", header.ReadString(Models.Metadata.Header.BuildKey));
            Assert.NotNull(header.Read<Models.OfflineList.CanOpen>(Models.Metadata.Header.CanOpenKey));
            Assert.Equal("category", header.ReadString(Models.Metadata.Header.CategoryKey));
            Assert.Equal("comment", header.ReadString(Models.Metadata.Header.CommentKey));
            Assert.Equal("date", header.ReadString(Models.Metadata.Header.DateKey));
            Assert.Equal("datversion", header.ReadString(Models.Metadata.Header.DatVersionKey));
            Assert.True(header.ReadBool(Models.Metadata.Header.DebugKey));
            Assert.Equal("description", header.ReadString(Models.Metadata.Header.DescriptionKey));
            Assert.Equal("email", header.ReadString(Models.Metadata.Header.EmailKey));
            Assert.Equal("emulatorversion", header.ReadString(Models.Metadata.Header.EmulatorVersionKey));
            Assert.Equal("merged", header.ReadString(Models.Metadata.Header.ForceMergingKey));
            Assert.Equal("required", header.ReadString(Models.Metadata.Header.ForceNodumpKey));
            Assert.Equal("zip", header.ReadString(Models.Metadata.Header.ForcePackingKey));
            Assert.True(header.ReadBool(Models.Metadata.Header.ForceZippingKey));
            Assert.Equal("header", header.ReadString(Models.Metadata.Header.HeaderKey));
            Assert.Equal("homepage", header.ReadString(Models.Metadata.Header.HomepageKey));
            Assert.Equal("id", header.ReadString(Models.Metadata.Header.IdKey));
            Assert.NotNull(header.Read<Models.OfflineList.Images>(Models.Metadata.Header.ImagesKey));
            Assert.Equal("imfolder", header.ReadString(Models.Metadata.Header.ImFolderKey));
            Assert.NotNull(header.Read<Models.OfflineList.Infos>(Models.Metadata.Header.InfosKey));
            Assert.True(header.ReadBool(Models.Metadata.Header.LockBiosModeKey));
            Assert.True(header.ReadBool(Models.Metadata.Header.LockRomModeKey));
            Assert.True(header.ReadBool(Models.Metadata.Header.LockSampleModeKey));
            Assert.Equal("mameconfig", header.ReadString(Models.Metadata.Header.MameConfigKey));
            Assert.Equal("name", header.ReadString(Models.Metadata.Header.NameKey));
            Assert.NotNull(header.Read<Models.OfflineList.NewDat>(Models.Metadata.Header.NewDatKey));
            Assert.Equal("notes", header.ReadString(Models.Metadata.Header.NotesKey));
            Assert.Equal("plugin", header.ReadString(Models.Metadata.Header.PluginKey));
            Assert.Equal("refname", header.ReadString(Models.Metadata.Header.RefNameKey));
            Assert.Equal("merged", header.ReadString(Models.Metadata.Header.RomModeKey));
            Assert.Equal("romtitle", header.ReadString(Models.Metadata.Header.RomTitleKey));
            Assert.Equal("rootdir", header.ReadString(Models.Metadata.Header.RootDirKey));
            Assert.Equal("merged", header.ReadString(Models.Metadata.Header.SampleModeKey));
            Assert.Equal("schemalocation", header.ReadString(Models.Metadata.Header.SchemaLocationKey));
            Assert.Equal("screenshotsheight", header.ReadString(Models.Metadata.Header.ScreenshotsHeightKey));
            Assert.Equal("screenshotsWidth", header.ReadString(Models.Metadata.Header.ScreenshotsWidthKey));
            Assert.NotNull(header.Read<Models.OfflineList.Search>(Models.Metadata.Header.SearchKey));
            Assert.Equal("system", header.ReadString(Models.Metadata.Header.SystemKey));
            Assert.Equal("timestamp", header.ReadString(Models.Metadata.Header.TimestampKey));
            Assert.Equal("type", header.ReadString(Models.Metadata.Header.TypeKey));
            Assert.Equal("url", header.ReadString(Models.Metadata.Header.UrlKey));
            Assert.Equal("version", header.ReadString(Models.Metadata.Header.VersionKey));
        }

        private static void ValidateMetadataMachine(Models.Metadata.Machine machine)
        {
            Assert.Equal("board", machine.ReadString(Models.Metadata.Machine.BoardKey));
            Assert.Equal("buttons", machine.ReadString(Models.Metadata.Machine.ButtonsKey));
            Assert.Equal("category", machine.ReadString(Models.Metadata.Machine.CategoryKey));
            Assert.Equal("cloneof", machine.ReadString(Models.Metadata.Machine.CloneOfKey));
            Assert.Equal("cloneofid", machine.ReadString(Models.Metadata.Machine.CloneOfIdKey));
            Assert.Equal("comment", machine.ReadString(Models.Metadata.Machine.CommentKey));
            Assert.Equal("company", machine.ReadString(Models.Metadata.Machine.CompanyKey));
            Assert.Equal("control", machine.ReadString(Models.Metadata.Machine.ControlKey));
            Assert.Equal("country", machine.ReadString(Models.Metadata.Machine.CountryKey));
            Assert.Equal("description", machine.ReadString(Models.Metadata.Machine.DescriptionKey));
            Assert.Equal("dirname", machine.ReadString(Models.Metadata.Machine.DirNameKey));
            Assert.Equal("displaycount", machine.ReadString(Models.Metadata.Machine.DisplayCountKey));
            Assert.Equal("displaytype", machine.ReadString(Models.Metadata.Machine.DisplayTypeKey));
            Assert.Equal("duplicateid", machine.ReadString(Models.Metadata.Machine.DuplicateIDKey));
            Assert.Equal("emulator", machine.ReadString(Models.Metadata.Machine.EmulatorKey));
            Assert.Equal("extra", machine.ReadString(Models.Metadata.Machine.ExtraKey));
            Assert.Equal("favorite", machine.ReadString(Models.Metadata.Machine.FavoriteKey));
            Assert.Equal("genmsxid", machine.ReadString(Models.Metadata.Machine.GenMSXIDKey));
            Assert.Equal("history", machine.ReadString(Models.Metadata.Machine.HistoryKey));
            Assert.Equal("id", machine.ReadString(Models.Metadata.Machine.IdKey));
            Assert.Equal(ZeroHash.CRC32Str, machine.ReadString(Models.Metadata.Machine.Im1CRCKey));
            Assert.Equal(ZeroHash.CRC32Str, machine.ReadString(Models.Metadata.Machine.Im2CRCKey));
            Assert.Equal("imagenumber", machine.ReadString(Models.Metadata.Machine.ImageNumberKey));
            Assert.Equal("yes", machine.ReadString(Models.Metadata.Machine.IsBiosKey));
            Assert.Equal("yes", machine.ReadString(Models.Metadata.Machine.IsDeviceKey));
            Assert.Equal("yes", machine.ReadString(Models.Metadata.Machine.IsMechanicalKey));
            Assert.Equal("language", machine.ReadString(Models.Metadata.Machine.LanguageKey));
            Assert.Equal("location", machine.ReadString(Models.Metadata.Machine.LocationKey));
            Assert.Equal("manufacturer", machine.ReadString(Models.Metadata.Machine.ManufacturerKey));
            Assert.Equal("name", machine.ReadString(Models.Metadata.Machine.NameKey));
            Assert.Equal("notes", machine.ReadString(Models.Metadata.Machine.NotesKey));
            Assert.Equal("playedcount", machine.ReadString(Models.Metadata.Machine.PlayedCountKey));
            Assert.Equal("playedtime", machine.ReadString(Models.Metadata.Machine.PlayedTimeKey));
            Assert.Equal("players", machine.ReadString(Models.Metadata.Machine.PlayersKey));
            Assert.Equal("publisher", machine.ReadString(Models.Metadata.Machine.PublisherKey));
            Assert.Equal("rebuildto", machine.ReadString(Models.Metadata.Machine.RebuildToKey));
            Assert.Equal("releasenumber", machine.ReadString(Models.Metadata.Machine.ReleaseNumberKey));
            Assert.Equal("romof", machine.ReadString(Models.Metadata.Machine.RomOfKey));
            Assert.Equal("rotation", machine.ReadString(Models.Metadata.Machine.RotationKey));
            Assert.Equal("yes", machine.ReadString(Models.Metadata.Machine.RunnableKey));
            Assert.Equal("sampleof", machine.ReadString(Models.Metadata.Machine.SampleOfKey));
            Assert.Equal("savetype", machine.ReadString(Models.Metadata.Machine.SaveTypeKey));
            Assert.Equal("sourcefile", machine.ReadString(Models.Metadata.Machine.SourceFileKey));
            Assert.Equal("sourcerom", machine.ReadString(Models.Metadata.Machine.SourceRomKey));
            Assert.Equal("status", machine.ReadString(Models.Metadata.Machine.StatusKey));
            Assert.Equal("yes", machine.ReadString(Models.Metadata.Machine.SupportedKey));
            Assert.Equal("system", machine.ReadString(Models.Metadata.Machine.SystemKey));
            Assert.Equal("tags", machine.ReadString(Models.Metadata.Machine.TagsKey));
            Assert.Equal("year", machine.ReadString(Models.Metadata.Machine.YearKey));

            Models.Metadata.Adjuster[]? adjusters = machine.ReadItemArray<Models.Metadata.Adjuster>(Models.Metadata.Machine.AdjusterKey);
            Assert.NotNull(adjusters);
            Models.Metadata.Adjuster adjuster = Assert.Single(adjusters);
            ValidateMetadataAdjuster(adjuster);

            Models.Metadata.Archive[]? archives = machine.ReadItemArray<Models.Metadata.Archive>(Models.Metadata.Machine.ArchiveKey);
            Assert.NotNull(archives);
            Models.Metadata.Archive archive = Assert.Single(archives);
            ValidateMetadataArchive(archive);

            Models.Metadata.BiosSet[]? biosSets = machine.ReadItemArray<Models.Metadata.BiosSet>(Models.Metadata.Machine.BiosSetKey);
            Assert.NotNull(biosSets);
            Models.Metadata.BiosSet biosSet = Assert.Single(biosSets);
            ValidateMetadataBiosSet(biosSet);

            Models.Metadata.Chip[]? chips = machine.ReadItemArray<Models.Metadata.Chip>(Models.Metadata.Machine.ChipKey);
            Assert.NotNull(chips);
            Models.Metadata.Chip chip = Assert.Single(chips);
            ValidateMetadataChip(chip);

            Models.Metadata.Configuration[]? configurations = machine.ReadItemArray<Models.Metadata.Configuration>(Models.Metadata.Machine.ConfigurationKey);
            Assert.NotNull(configurations);
            Models.Metadata.Configuration configuration = Assert.Single(configurations);
            ValidateMetadataConfiguration(configuration);

            Models.Metadata.Device[]? devices = machine.ReadItemArray<Models.Metadata.Device>(Models.Metadata.Machine.DeviceKey);
            Assert.NotNull(devices);
            Models.Metadata.Device device = Assert.Single(devices);
            ValidateMetadataDevice(device);

            Models.Metadata.DeviceRef[]? deviceRefs = machine.ReadItemArray<Models.Metadata.DeviceRef>(Models.Metadata.Machine.DeviceRefKey);
            Assert.NotNull(deviceRefs);
            Models.Metadata.DeviceRef deviceRef = Assert.Single(deviceRefs);
            ValidateMetadataDeviceRef(deviceRef);

            Models.Metadata.DipSwitch[]? dipSwitches = machine.ReadItemArray<Models.Metadata.DipSwitch>(Models.Metadata.Machine.DipSwitchKey);
            Assert.NotNull(dipSwitches);
            Assert.Equal(2, dipSwitches.Length);
            Models.Metadata.DipSwitch dipSwitch = dipSwitches[0];
            ValidateMetadataDipSwitch(dipSwitch);

            Models.Metadata.Disk[]? disks = machine.ReadItemArray<Models.Metadata.Disk>(Models.Metadata.Machine.DiskKey);
            Assert.NotNull(disks);
            Assert.Equal(2, disks.Length);
            Models.Metadata.Disk disk = disks[0];
            ValidateMetadataDisk(disk);

            Models.Metadata.Display[]? displays = machine.ReadItemArray<Models.Metadata.Display>(Models.Metadata.Machine.DisplayKey);
            Assert.NotNull(displays);
            Assert.Equal(2, displays.Length);
            Models.Metadata.Display? display = Array.Find(displays, d => !d.ContainsKey(Models.Metadata.Video.AspectXKey));
            ValidateMetadataDisplay(display);

            Models.Metadata.Driver[]? drivers = machine.ReadItemArray<Models.Metadata.Driver>(Models.Metadata.Machine.DriverKey);
            Assert.NotNull(drivers);
            Models.Metadata.Driver driver = Assert.Single(drivers);
            ValidateMetadataDriver(driver);

            // TODO: Implement this validation
            // Models.Metadata.Dump[]? dumps = machine.ReadItemArray<Models.Metadata.Dump>(Models.Metadata.Machine.DumpKey);
            // Assert.NotNull(dumps);
            // Models.Metadata.Dump dump = Assert.Single(dumps);
            // ValidateMetadataDump(dump);

            Models.Metadata.Feature[]? features = machine.ReadItemArray<Models.Metadata.Feature>(Models.Metadata.Machine.FeatureKey);
            Assert.NotNull(features);
            Assert.Equal(2, features.Length);
            Models.Metadata.Feature feature = features[0];
            ValidateMetadataFeature(feature);

            Models.Metadata.Info[]? infos = machine.ReadItemArray<Models.Metadata.Info>(Models.Metadata.Machine.InfoKey);
            Assert.NotNull(infos);
            Models.Metadata.Info info = Assert.Single(infos);
            ValidateMetadataInfo(info);

            Models.Metadata.Input[]? inputs = machine.ReadItemArray<Models.Metadata.Input>(Models.Metadata.Machine.InputKey);
            Assert.NotNull(inputs);
            Models.Metadata.Input input = Assert.Single(inputs);
            ValidateMetadataInput(input);

            Models.Metadata.Media[]? media = machine.ReadItemArray<Models.Metadata.Media>(Models.Metadata.Machine.MediaKey);
            Assert.NotNull(media);
            Models.Metadata.Media medium = Assert.Single(media);
            ValidateMetadataMedia(medium);

            Models.Metadata.Part[]? parts = machine.ReadItemArray<Models.Metadata.Part>(Models.Metadata.Machine.PartKey);
            Assert.NotNull(parts);
            Models.Metadata.Part part = Assert.Single(parts);
            ValidateMetadataPart(part);

            Models.Metadata.Port[]? ports = machine.ReadItemArray<Models.Metadata.Port>(Models.Metadata.Machine.PortKey);
            Assert.NotNull(ports);
            Models.Metadata.Port port = Assert.Single(ports);
            ValidateMetadataPort(port);

            Models.Metadata.RamOption[]? ramOptions = machine.ReadItemArray<Models.Metadata.RamOption>(Models.Metadata.Machine.RamOptionKey);
            Assert.NotNull(ramOptions);
            Models.Metadata.RamOption ramOption = Assert.Single(ramOptions);
            ValidateMetadataRamOption(ramOption);

            Models.Metadata.Release[]? releases = machine.ReadItemArray<Models.Metadata.Release>(Models.Metadata.Machine.ReleaseKey);
            Assert.NotNull(releases);
            Models.Metadata.Release release = Assert.Single(releases);
            ValidateMetadataRelease(release);

            Models.Metadata.Rom[]? roms = machine.ReadItemArray<Models.Metadata.Rom>(Models.Metadata.Machine.RomKey);
            Assert.NotNull(roms);
            Assert.Equal(2, roms.Length);
            Models.Metadata.Rom rom = roms[0];
            ValidateMetadataRom(rom);

            Models.Metadata.Sample[]? samples = machine.ReadItemArray<Models.Metadata.Sample>(Models.Metadata.Machine.SampleKey);
            Assert.NotNull(samples);
            Models.Metadata.Sample sample = Assert.Single(samples);
            ValidateMetadataSample(sample);

            Models.Metadata.SharedFeat[]? sharedFeats = machine.ReadItemArray<Models.Metadata.SharedFeat>(Models.Metadata.Machine.SharedFeatKey);
            Assert.NotNull(sharedFeats);
            Models.Metadata.SharedFeat sharedFeat = Assert.Single(sharedFeats);
            ValidateMetadataSharedFeat(sharedFeat);

            Models.Metadata.Slot[]? slots = machine.ReadItemArray<Models.Metadata.Slot>(Models.Metadata.Machine.SlotKey);
            Assert.NotNull(slots);
            Models.Metadata.Slot slot = Assert.Single(slots);
            ValidateMetadataSlot(slot);

            Models.Metadata.SoftwareList[]? softwareLists = machine.ReadItemArray<Models.Metadata.SoftwareList>(Models.Metadata.Machine.SoftwareListKey);
            Assert.NotNull(softwareLists);
            Models.Metadata.SoftwareList softwareList = Assert.Single(softwareLists);
            ValidateMetadataSoftwareList(softwareList);

            Models.Metadata.Sound[]? sounds = machine.ReadItemArray<Models.Metadata.Sound>(Models.Metadata.Machine.SoundKey);
            Assert.NotNull(sounds);
            Models.Metadata.Sound sound = Assert.Single(sounds);
            ValidateMetadataSound(sound);

            Models.Logiqx.Trurip? trurip = machine.Read<Models.Logiqx.Trurip>(Models.Metadata.Machine.TruripKey);
            ValidateMetadataTrurip(trurip);

            Models.Metadata.Video[]? videos = machine.ReadItemArray<Models.Metadata.Video>(Models.Metadata.Machine.VideoKey);
            Assert.NotNull(videos);
            Models.Metadata.Video video = Assert.Single(videos);
            ValidateMetadataVideo(video);
        }

        private static void ValidateMetadataAdjuster(Models.Metadata.Adjuster? adjuster)
        {
            Assert.NotNull(adjuster);
            Assert.True(adjuster.ReadBool(Models.Metadata.Adjuster.DefaultKey));
            Assert.Equal("name", adjuster.ReadString(Models.Metadata.Adjuster.NameKey));

            Models.Metadata.Condition? condition = adjuster.Read<Models.Metadata.Condition>(Models.Metadata.Adjuster.ConditionKey);
            ValidateMetadataCondition(condition);
        }

        private static void ValidateMetadataAnalog(Models.Metadata.Analog? analog)
        {
            Assert.NotNull(analog);
            Assert.Equal("mask", analog.ReadString(Models.Metadata.Analog.MaskKey));
        }

        private static void ValidateMetadataArchive(Models.Metadata.Archive? archive)
        {
            Assert.NotNull(archive);
            Assert.Equal("name", archive.ReadString(Models.Metadata.Archive.NameKey));
        }

        private static void ValidateMetadataBiosSet(Models.Metadata.BiosSet? biosSet)
        {
            Assert.NotNull(biosSet);
            Assert.True(biosSet.ReadBool(Models.Metadata.BiosSet.DefaultKey));
            Assert.Equal("description", biosSet.ReadString(Models.Metadata.BiosSet.DescriptionKey));
            Assert.Equal("name", biosSet.ReadString(Models.Metadata.BiosSet.NameKey));
        }

        private static void ValidateMetadataChip(Models.Metadata.Chip? chip)
        {
            Assert.NotNull(chip);
            Assert.Equal(12345, chip.ReadLong(Models.Metadata.Chip.ClockKey));
            Assert.Equal("flags", chip.ReadString(Models.Metadata.Chip.FlagsKey));
            Assert.Equal("name", chip.ReadString(Models.Metadata.Chip.NameKey));
            Assert.True(chip.ReadBool(Models.Metadata.Chip.SoundOnlyKey));
            Assert.Equal("tag", chip.ReadString(Models.Metadata.Chip.TagKey));
            Assert.Equal("cpu", chip.ReadString(Models.Metadata.Chip.ChipTypeKey));
        }

        private static void ValidateMetadataCondition(Models.Metadata.Condition? condition)
        {
            Assert.NotNull(condition);
            Assert.Equal("value", condition.ReadString(Models.Metadata.Condition.ValueKey));
            Assert.Equal("mask", condition.ReadString(Models.Metadata.Condition.MaskKey));
            Assert.Equal("eq", condition.ReadString(Models.Metadata.Condition.RelationKey));
            Assert.Equal("tag", condition.ReadString(Models.Metadata.Condition.TagKey));
        }

        private static void ValidateMetadataConfiguration(Models.Metadata.Configuration? configuration)
        {
            Assert.NotNull(configuration);
            Assert.Equal("mask", configuration.ReadString(Models.Metadata.Configuration.MaskKey));
            Assert.Equal("name", configuration.ReadString(Models.Metadata.Configuration.NameKey));
            Assert.Equal("tag", configuration.ReadString(Models.Metadata.Configuration.TagKey));

            Models.Metadata.Condition? condition = configuration.Read<Models.Metadata.Condition>(Models.Metadata.Configuration.ConditionKey);
            ValidateMetadataCondition(condition);

            Models.Metadata.ConfLocation[]? confLocations = configuration.ReadItemArray<Models.Metadata.ConfLocation>(Models.Metadata.Configuration.ConfLocationKey);
            Assert.NotNull(confLocations);
            Models.Metadata.ConfLocation? confLocation = Assert.Single(confLocations);
            ValidateMetadataConfLocation(confLocation);

            Models.Metadata.ConfSetting[]? confSettings = configuration.ReadItemArray<Models.Metadata.ConfSetting>(Models.Metadata.Configuration.ConfSettingKey);
            Assert.NotNull(confSettings);
            Models.Metadata.ConfSetting? confSetting = Assert.Single(confSettings);
            ValidateMetadataConfSetting(confSetting);
        }

        private static void ValidateMetadataConfLocation(Models.Metadata.ConfLocation? confLocation)
        {
            Assert.NotNull(confLocation);
            Assert.True(confLocation.ReadBool(Models.Metadata.ConfLocation.InvertedKey));
            Assert.Equal("name", confLocation.ReadString(Models.Metadata.ConfLocation.NameKey));
            Assert.Equal("number", confLocation.ReadString(Models.Metadata.ConfLocation.NumberKey));
        }

        private static void ValidateMetadataConfSetting(Models.Metadata.ConfSetting? confSetting)
        {
            Assert.NotNull(confSetting);
            Assert.True(confSetting.ReadBool(Models.Metadata.ConfSetting.DefaultKey));
            Assert.Equal("name", confSetting.ReadString(Models.Metadata.ConfSetting.NameKey));
            Assert.Equal("value", confSetting.ReadString(Models.Metadata.ConfSetting.ValueKey));

            Models.Metadata.Condition? condition = confSetting.Read<Models.Metadata.Condition>(Models.Metadata.ConfSetting.ConditionKey);
            ValidateMetadataCondition(condition);
        }

        private static void ValidateMetadataControl(Models.Metadata.Control? control)
        {
            Assert.NotNull(control);
            Assert.Equal(12345, control.ReadLong(Models.Metadata.Control.ButtonsKey));
            Assert.Equal(12345, control.ReadLong(Models.Metadata.Control.KeyDeltaKey));
            Assert.Equal(12345, control.ReadLong(Models.Metadata.Control.MaximumKey));
            Assert.Equal(12345, control.ReadLong(Models.Metadata.Control.MinimumKey));
            Assert.Equal(12345, control.ReadLong(Models.Metadata.Control.PlayerKey));
            Assert.Equal(12345, control.ReadLong(Models.Metadata.Control.ReqButtonsKey));
            Assert.True(control.ReadBool(Models.Metadata.Control.ReverseKey));
            Assert.Equal(12345, control.ReadLong(Models.Metadata.Control.SensitivityKey));
            Assert.Equal("lightgun", control.ReadString(Models.Metadata.Control.ControlTypeKey));
            Assert.Equal("ways", control.ReadString(Models.Metadata.Control.WaysKey));
            Assert.Equal("ways2", control.ReadString(Models.Metadata.Control.Ways2Key));
            Assert.Equal("ways3", control.ReadString(Models.Metadata.Control.Ways3Key));
        }

        private static void ValidateMetadataDataArea(Models.Metadata.DataArea? dataArea)
        {
            Assert.NotNull(dataArea);
            Assert.Equal("big", dataArea.ReadString(Models.Metadata.DataArea.EndiannessKey));
            Assert.Equal("name", dataArea.ReadString(Models.Metadata.DataArea.NameKey));
            Assert.Equal(12345, dataArea.ReadLong(Models.Metadata.DataArea.SizeKey));
            Assert.Equal(64, dataArea.ReadLong(Models.Metadata.DataArea.WidthKey));
        
            Models.Metadata.Rom[]? roms = dataArea.ReadItemArray<Models.Metadata.Rom>(Models.Metadata.DataArea.RomKey);
            Assert.NotNull(roms);
            Models.Metadata.Rom? rom = Assert.Single(roms);
            ValidateMetadataRom(rom);
        }

        private static void ValidateMetadataDevice(Models.Metadata.Device? device)
        {
            Assert.NotNull(device);
            Assert.Equal("fixedimage", device.ReadString(Models.Metadata.Device.FixedImageKey));
            Assert.Equal("interface", device.ReadString(Models.Metadata.Device.InterfaceKey));
            Assert.Equal(1, device.ReadLong(Models.Metadata.Device.MandatoryKey));
            Assert.Equal("tag", device.ReadString(Models.Metadata.Device.TagKey));
            Assert.Equal("punchtape", device.ReadString(Models.Metadata.Device.DeviceTypeKey));

            Models.Metadata.Extension[]? extensions = device.ReadItemArray<Models.Metadata.Extension>(Models.Metadata.Device.ExtensionKey);
            Assert.NotNull(extensions);
            Models.Metadata.Extension? extension = Assert.Single(extensions);
            ValidateMetadataExtension(extension);

            Models.Metadata.Instance? instance = device.Read<Models.Metadata.Instance>(Models.Metadata.Device.InstanceKey);
            ValidateMetadataInstance(instance);
        }

        private static void ValidateMetadataDeviceRef(Models.Metadata.DeviceRef? deviceRef)
        {
            Assert.NotNull(deviceRef);
            Assert.Equal("name", deviceRef.ReadString(Models.Metadata.DeviceRef.NameKey));
        }

        private static void ValidateMetadataDipLocation(Models.Metadata.DipLocation? dipLocation)
        {
            Assert.NotNull(dipLocation);
            Assert.True(dipLocation.ReadBool(Models.Metadata.DipLocation.InvertedKey));
            Assert.Equal("name", dipLocation.ReadString(Models.Metadata.DipLocation.NameKey));
            Assert.Equal("number", dipLocation.ReadString(Models.Metadata.DipLocation.NumberKey));
        }

        private static void ValidateMetadataDipSwitch(Models.Metadata.DipSwitch? dipSwitch)
        {
            Assert.NotNull(dipSwitch);
            Assert.True(dipSwitch.ReadBool(Models.Metadata.DipSwitch.DefaultKey));
            Assert.Equal("mask", dipSwitch.ReadString(Models.Metadata.DipSwitch.MaskKey));
            Assert.Equal("name", dipSwitch.ReadString(Models.Metadata.DipSwitch.NameKey));
            Assert.Equal("tag", dipSwitch.ReadString(Models.Metadata.DipSwitch.TagKey));

            Models.Metadata.Condition? condition = dipSwitch.Read<Models.Metadata.Condition>(Models.Metadata.DipSwitch.ConditionKey);
            ValidateMetadataCondition(condition);

            Models.Metadata.DipLocation[]? dipLocations = dipSwitch.ReadItemArray<Models.Metadata.DipLocation>(Models.Metadata.DipSwitch.DipLocationKey);
            Assert.NotNull(dipLocations);
            Models.Metadata.DipLocation? dipLocation = Assert.Single(dipLocations);
            ValidateMetadataDipLocation(dipLocation);

            Models.Metadata.DipValue[]? dipValues = dipSwitch.ReadItemArray<Models.Metadata.DipValue>(Models.Metadata.DipSwitch.DipValueKey);
            Assert.NotNull(dipValues);
            Models.Metadata.DipValue? dipValue = Assert.Single(dipValues);
            ValidateMetadataDipValue(dipValue);

            string[]? entries = dipSwitch.ReadStringArray(Models.Metadata.DipSwitch.EntryKey);
            Assert.NotNull(entries);
            string entry = Assert.Single(entries);
            Assert.Equal("entry", entry);
        }

        private static void ValidateMetadataDipValue(Models.Metadata.DipValue? dipValue)
        {
            Assert.NotNull(dipValue);
            Assert.True(dipValue.ReadBool(Models.Metadata.DipValue.DefaultKey));
            Assert.Equal("name", dipValue.ReadString(Models.Metadata.DipValue.NameKey));
            Assert.Equal("value", dipValue.ReadString(Models.Metadata.DipValue.ValueKey));

            Models.Metadata.Condition? condition = dipValue.Read<Models.Metadata.Condition>(Models.Metadata.DipValue.ConditionKey);
            ValidateMetadataCondition(condition);
        }

        private static void ValidateMetadataDisk(Models.Metadata.Disk? disk)
        {
            Assert.NotNull(disk);
            Assert.Equal("flags", disk.ReadString(Models.Metadata.Disk.FlagsKey));
            Assert.Equal("index", disk.ReadString(Models.Metadata.Disk.IndexKey));
            Assert.Equal(ZeroHash.MD5Str, disk.ReadString(Models.Metadata.Disk.MD5Key));
            Assert.Equal("merge", disk.ReadString(Models.Metadata.Disk.MergeKey));
            Assert.Equal("name", disk.ReadString(Models.Metadata.Disk.NameKey));
            Assert.True(disk.ReadBool(Models.Metadata.Disk.OptionalKey));
            Assert.Equal("region", disk.ReadString(Models.Metadata.Disk.RegionKey));
            Assert.Equal(ZeroHash.SHA1Str, disk.ReadString(Models.Metadata.Disk.SHA1Key));
            Assert.True(disk.ReadBool(Models.Metadata.Disk.WritableKey));
        }

        private static void ValidateMetadataDiskArea(Models.Metadata.DiskArea? diskArea)
        {
            Assert.NotNull(diskArea);
            Assert.Equal("name", diskArea.ReadString(Models.Metadata.DiskArea.NameKey));
        
            Models.Metadata.Disk[]? disks = diskArea.ReadItemArray<Models.Metadata.Disk>(Models.Metadata.DiskArea.DiskKey);
            Assert.NotNull(disks);
            Models.Metadata.Disk? disk = Assert.Single(disks);
            ValidateMetadataDisk(disk);
        }

        private static void ValidateMetadataDisplay(Models.Metadata.Display? display)
        {
            Assert.NotNull(display);
            Assert.True(display.ReadBool(Models.Metadata.Display.FlipXKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.HBEndKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.HBStartKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.HeightKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.HTotalKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.PixClockKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.RefreshKey));
            Assert.Equal(90, display.ReadLong(Models.Metadata.Display.RotateKey));
            Assert.Equal("tag", display.ReadString(Models.Metadata.Display.TagKey));
            Assert.Equal("vector", display.ReadString(Models.Metadata.Display.DisplayTypeKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.VBEndKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.VBStartKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.VTotalKey));
            Assert.Equal(12345, display.ReadLong(Models.Metadata.Display.WidthKey));
        }

        private static void ValidateMetadataDriver(Models.Metadata.Driver? driver)
        {
            Assert.NotNull(driver);
            Assert.Equal("plain", driver.ReadString(Models.Metadata.Driver.BlitKey));
            Assert.Equal("good", driver.ReadString(Models.Metadata.Driver.CocktailKey));
            Assert.Equal("good", driver.ReadString(Models.Metadata.Driver.ColorKey));
            Assert.Equal("good", driver.ReadString(Models.Metadata.Driver.EmulationKey));
            Assert.True(driver.ReadBool(Models.Metadata.Driver.IncompleteKey));
            Assert.True(driver.ReadBool(Models.Metadata.Driver.NoSoundHardwareKey));
            Assert.Equal("pallettesize", driver.ReadString(Models.Metadata.Driver.PaletteSizeKey));
            Assert.True(driver.ReadBool(Models.Metadata.Driver.RequiresArtworkKey));
            Assert.Equal("supported", driver.ReadString(Models.Metadata.Driver.SaveStateKey));
            Assert.Equal("good", driver.ReadString(Models.Metadata.Driver.SoundKey));
            Assert.Equal("good", driver.ReadString(Models.Metadata.Driver.StatusKey));
            Assert.True(driver.ReadBool(Models.Metadata.Driver.UnofficialKey));
        }

        private static void ValidateMetadataExtension(Models.Metadata.Extension? extension)
        {
            Assert.NotNull(extension);
            Assert.Equal("name", extension.ReadString(Models.Metadata.Extension.NameKey));
        }

        private static void ValidateMetadataFeature(Models.Metadata.Feature? feature)
        {
            Assert.NotNull(feature);
            Assert.Equal("name", feature.ReadString(Models.Metadata.Feature.NameKey));
            Assert.Equal("imperfect", feature.ReadString(Models.Metadata.Feature.OverallKey));
            Assert.Equal("imperfect", feature.ReadString(Models.Metadata.Feature.StatusKey));
            Assert.Equal("protection", feature.ReadString(Models.Metadata.Feature.FeatureTypeKey));
            Assert.Equal("value", feature.ReadString(Models.Metadata.Feature.ValueKey));
        }

        private static void ValidateMetadataInfo(Models.Metadata.Info? info)
        {
            Assert.NotNull(info);
            Assert.Equal("name", info.ReadString(Models.Metadata.Info.NameKey));
            Assert.Equal("value", info.ReadString(Models.Metadata.Info.ValueKey));
        }

        private static void ValidateMetadataInput(Models.Metadata.Input? input)
        {
            Assert.NotNull(input);
            Assert.Equal(12345, input.ReadLong(Models.Metadata.Input.ButtonsKey));
            Assert.Equal(12345, input.ReadLong(Models.Metadata.Input.CoinsKey));
            Assert.Equal(12345, input.ReadLong(Models.Metadata.Input.PlayersKey));
            Assert.True(input.ReadBool(Models.Metadata.Input.ServiceKey));
            Assert.True(input.ReadBool(Models.Metadata.Input.TiltKey));

            Models.Metadata.Control[]? controls = input.ReadItemArray<Models.Metadata.Control>(Models.Metadata.Input.ControlKey);
            Assert.NotNull(controls);
            Models.Metadata.Control? control = Assert.Single(controls);
            ValidateMetadataControl(control);
        }

        private static void ValidateMetadataInstance(Models.Metadata.Instance? instance)
        {
            Assert.NotNull(instance);
            Assert.Equal("briefname", instance.ReadString(Models.Metadata.Instance.BriefNameKey));
            Assert.Equal("name", instance.ReadString(Models.Metadata.Instance.NameKey));
        }

        private static void ValidateMetadataMedia(Models.Metadata.Media? media)
        {
            Assert.NotNull(media);
            Assert.Equal(ZeroHash.MD5Str, media.ReadString(Models.Metadata.Media.MD5Key));
            Assert.Equal("name", media.ReadString(Models.Metadata.Media.NameKey));
            Assert.Equal(ZeroHash.SHA1Str, media.ReadString(Models.Metadata.Media.SHA1Key));
            Assert.Equal(ZeroHash.SHA256Str, media.ReadString(Models.Metadata.Media.SHA256Key));
            Assert.Equal(ZeroHash.SpamSumStr, media.ReadString(Models.Metadata.Media.SpamSumKey));
        }

        private static void ValidateMetadataPart(Models.Metadata.Part? part)
        {
            Assert.NotNull(part);
            Assert.Equal("interface", part.ReadString(Models.Metadata.Part.InterfaceKey));
            Assert.Equal("name", part.ReadString(Models.Metadata.Part.NameKey));

            Models.Metadata.DataArea[]? dataAreas = part.ReadItemArray<Models.Metadata.DataArea>(Models.Metadata.Part.DataAreaKey);
            Assert.NotNull(dataAreas);
            Models.Metadata.DataArea? dataArea = Assert.Single(dataAreas);
            ValidateMetadataDataArea(dataArea);

            Models.Metadata.DiskArea[]? diskAreas = part.ReadItemArray<Models.Metadata.DiskArea>(Models.Metadata.Part.DiskAreaKey);
            Assert.NotNull(diskAreas);
            Models.Metadata.DiskArea? diskArea = Assert.Single(diskAreas);
            ValidateMetadataDiskArea(diskArea);

            Models.Metadata.DipSwitch[]? dipSwitches = part.ReadItemArray<Models.Metadata.DipSwitch>(Models.Metadata.Part.DipSwitchKey);
            Assert.NotNull(dipSwitches);
            Models.Metadata.DipSwitch? dipSwitch = Assert.Single(dipSwitches);
            ValidateMetadataDipSwitch(dipSwitch);

            Models.Metadata.Feature[]? features = part.ReadItemArray<Models.Metadata.Feature>(Models.Metadata.Part.FeatureKey);
            Assert.NotNull(features);
            Models.Metadata.Feature? feature = Assert.Single(features);
            ValidateMetadataFeature(feature);
        }

        private static void ValidateMetadataPort(Models.Metadata.Port? port)
        {
            Assert.NotNull(port);
            Assert.Equal("tag", port.ReadString(Models.Metadata.Port.TagKey));

            Models.Metadata.Analog[]? dipValues = port.ReadItemArray<Models.Metadata.Analog>(Models.Metadata.Port.AnalogKey);
            Assert.NotNull(dipValues);
            Models.Metadata.Analog? dipValue = Assert.Single(dipValues);
            ValidateMetadataAnalog(dipValue);
        }

        private static void ValidateMetadataRamOption(Models.Metadata.RamOption? ramOption)
        {
            Assert.NotNull(ramOption);
            Assert.Equal("content", ramOption.ReadString(Models.Metadata.RamOption.ContentKey));
            Assert.True(ramOption.ReadBool(Models.Metadata.RamOption.DefaultKey));
            Assert.Equal("name", ramOption.ReadString(Models.Metadata.RamOption.NameKey));
        }

        private static void ValidateMetadataRelease(Models.Metadata.Release? release)
        {
            Assert.NotNull(release);
            Assert.Equal("date", release.ReadString(Models.Metadata.Release.DateKey));
            Assert.True(release.ReadBool(Models.Metadata.Release.DefaultKey));
            Assert.Equal("language", release.ReadString(Models.Metadata.Release.LanguageKey));
            Assert.Equal("name", release.ReadString(Models.Metadata.Release.NameKey));
            Assert.Equal("region", release.ReadString(Models.Metadata.Release.RegionKey));
        }

        private static void ValidateMetadataRom(Models.Metadata.Rom? rom)
        {
            Assert.NotNull(rom);
            Assert.Equal("album", rom.ReadString(Models.Metadata.Rom.AlbumKey));
            Assert.Equal("alt_romname", rom.ReadString(Models.Metadata.Rom.AltRomnameKey));
            Assert.Equal("alt_title", rom.ReadString(Models.Metadata.Rom.AltTitleKey));
            Assert.Equal("artist", rom.ReadString(Models.Metadata.Rom.ArtistKey));
            Assert.Equal("asr_detected_lang", rom.ReadString(Models.Metadata.Rom.ASRDetectedLangKey));
            Assert.Equal("asr_detected_lang_conf", rom.ReadString(Models.Metadata.Rom.ASRDetectedLangConfKey));
            Assert.Equal("asr_transcribed_lang", rom.ReadString(Models.Metadata.Rom.ASRTranscribedLangKey));
            Assert.Equal("bios", rom.ReadString(Models.Metadata.Rom.BiosKey));
            Assert.Equal("bitrate", rom.ReadString(Models.Metadata.Rom.BitrateKey));
            Assert.Equal("btih", rom.ReadString(Models.Metadata.Rom.BitTorrentMagnetHashKey));
            Assert.Equal("cloth_cover_detection_module_version", rom.ReadString(Models.Metadata.Rom.ClothCoverDetectionModuleVersionKey));
            Assert.Equal("collection-catalog-number", rom.ReadString(Models.Metadata.Rom.CollectionCatalogNumberKey));
            Assert.Equal("comment", rom.ReadString(Models.Metadata.Rom.CommentKey));
            Assert.Equal(ZeroHash.CRC32Str, rom.ReadString(Models.Metadata.Rom.CRCKey));
            Assert.Equal("creator", rom.ReadString(Models.Metadata.Rom.CreatorKey));
            Assert.Equal("date", rom.ReadString(Models.Metadata.Rom.DateKey));
            Assert.True(rom.ReadBool(Models.Metadata.Rom.DisposeKey));
            Assert.Equal("extension", rom.ReadString(Models.Metadata.Rom.ExtensionKey));
            Assert.Equal(12345, rom.ReadLong(Models.Metadata.Rom.FileCountKey));
            Assert.True(rom.ReadBool(Models.Metadata.Rom.FileIsAvailableKey));
            Assert.Equal("flags", rom.ReadString(Models.Metadata.Rom.FlagsKey));
            Assert.Equal("format", rom.ReadString(Models.Metadata.Rom.FormatKey));
            Assert.Equal("header", rom.ReadString(Models.Metadata.Rom.HeaderKey));
            Assert.Equal("height", rom.ReadString(Models.Metadata.Rom.HeightKey));
            Assert.Equal("hocr_char_to_word_hocr_version", rom.ReadString(Models.Metadata.Rom.hOCRCharToWordhOCRVersionKey));
            Assert.Equal("hocr_char_to_word_module_version", rom.ReadString(Models.Metadata.Rom.hOCRCharToWordModuleVersionKey));
            Assert.Equal("hocr_fts_text_hocr_version", rom.ReadString(Models.Metadata.Rom.hOCRFtsTexthOCRVersionKey));
            Assert.Equal("hocr_fts_text_module_version", rom.ReadString(Models.Metadata.Rom.hOCRFtsTextModuleVersionKey));
            Assert.Equal("hocr_pageindex_hocr_version", rom.ReadString(Models.Metadata.Rom.hOCRPageIndexhOCRVersionKey));
            Assert.Equal("hocr_pageindex_module_version", rom.ReadString(Models.Metadata.Rom.hOCRPageIndexModuleVersionKey));
            Assert.True(rom.ReadBool(Models.Metadata.Rom.InvertedKey));
            Assert.Equal("mtime", rom.ReadString(Models.Metadata.Rom.LastModifiedTimeKey));
            Assert.Equal("length", rom.ReadString(Models.Metadata.Rom.LengthKey));
            Assert.Equal("load16_byte", rom.ReadString(Models.Metadata.Rom.LoadFlagKey));
            Assert.Equal("matrix_number", rom.ReadString(Models.Metadata.Rom.MatrixNumberKey));
            Assert.Equal(ZeroHash.GetString(HashType.MD2), rom.ReadString(Models.Metadata.Rom.MD2Key));
            Assert.Equal(ZeroHash.GetString(HashType.MD4), rom.ReadString(Models.Metadata.Rom.MD4Key));
            Assert.Equal(ZeroHash.MD5Str, rom.ReadString(Models.Metadata.Rom.MD5Key));
            Assert.Null(rom.ReadString(Models.Metadata.Rom.OpenMSXMediaType)); // Omit due to other test
            Assert.Equal("merge", rom.ReadString(Models.Metadata.Rom.MergeKey));
            Assert.True(rom.ReadBool(Models.Metadata.Rom.MIAKey));
            Assert.Equal("name", rom.ReadString(Models.Metadata.Rom.NameKey));
            Assert.Equal("ocr", rom.ReadString(Models.Metadata.Rom.TesseractOCRKey));
            Assert.Equal("ocr_converted", rom.ReadString(Models.Metadata.Rom.TesseractOCRConvertedKey));
            Assert.Equal("ocr_detected_lang", rom.ReadString(Models.Metadata.Rom.TesseractOCRDetectedLangKey));
            Assert.Equal("ocr_detected_lang_conf", rom.ReadString(Models.Metadata.Rom.TesseractOCRDetectedLangConfKey));
            Assert.Equal("ocr_detected_script", rom.ReadString(Models.Metadata.Rom.TesseractOCRDetectedScriptKey));
            Assert.Equal("ocr_detected_script_conf", rom.ReadString(Models.Metadata.Rom.TesseractOCRDetectedScriptConfKey));
            Assert.Equal("ocr_module_version", rom.ReadString(Models.Metadata.Rom.TesseractOCRModuleVersionKey));
            Assert.Equal("ocr_parameters", rom.ReadString(Models.Metadata.Rom.TesseractOCRParametersKey));
            Assert.Equal("offset", rom.ReadString(Models.Metadata.Rom.OffsetKey));
            Assert.True(rom.ReadBool(Models.Metadata.Rom.OptionalKey));
            Assert.Equal("original", rom.ReadString(Models.Metadata.Rom.OriginalKey));
            Assert.Equal("pdf_module_version", rom.ReadString(Models.Metadata.Rom.PDFModuleVersionKey));
            Assert.Equal("preview-image", rom.ReadString(Models.Metadata.Rom.PreviewImageKey));
            Assert.Equal("publisher", rom.ReadString(Models.Metadata.Rom.PublisherKey));
            Assert.Equal("region", rom.ReadString(Models.Metadata.Rom.RegionKey));
            Assert.Equal("remark", rom.ReadString(Models.Metadata.Rom.RemarkKey));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD128), rom.ReadString(Models.Metadata.Rom.RIPEMD128Key));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD160), rom.ReadString(Models.Metadata.Rom.RIPEMD160Key));
            Assert.Equal("rotation", rom.ReadString(Models.Metadata.Rom.RotationKey));
            Assert.Equal("serial", rom.ReadString(Models.Metadata.Rom.SerialKey));
            Assert.Equal(ZeroHash.SHA1Str, rom.ReadString(Models.Metadata.Rom.SHA1Key));
            Assert.Equal(ZeroHash.SHA256Str, rom.ReadString(Models.Metadata.Rom.SHA256Key));
            Assert.Equal(ZeroHash.SHA384Str, rom.ReadString(Models.Metadata.Rom.SHA384Key));
            Assert.Equal(ZeroHash.SHA512Str, rom.ReadString(Models.Metadata.Rom.SHA512Key));
            Assert.Equal(12345, rom.ReadLong(Models.Metadata.Rom.SizeKey));
            Assert.True(rom.ReadBool(Models.Metadata.Rom.SoundOnlyKey));
            Assert.Equal("source", rom.ReadString(Models.Metadata.Rom.SourceKey));
            Assert.Equal(ZeroHash.SpamSumStr, rom.ReadString(Models.Metadata.Rom.SpamSumKey));
            Assert.Equal("start", rom.ReadString(Models.Metadata.Rom.StartKey));
            Assert.Equal("good", rom.ReadString(Models.Metadata.Rom.StatusKey));
            Assert.Equal("summation", rom.ReadString(Models.Metadata.Rom.SummationKey));
            Assert.Equal("title", rom.ReadString(Models.Metadata.Rom.TitleKey));
            Assert.Equal("track", rom.ReadString(Models.Metadata.Rom.TrackKey));
            Assert.Equal("type", rom.ReadString(Models.Metadata.Rom.OpenMSXType));
            Assert.Equal("value", rom.ReadString(Models.Metadata.Rom.ValueKey));
            Assert.Equal("whisper_asr_module_version", rom.ReadString(Models.Metadata.Rom.WhisperASRModuleVersionKey));
            Assert.Equal("whisper_model_hash", rom.ReadString(Models.Metadata.Rom.WhisperModelHashKey));
            Assert.Equal("whisper_model_name", rom.ReadString(Models.Metadata.Rom.WhisperModelNameKey));
            Assert.Equal("whisper_version", rom.ReadString(Models.Metadata.Rom.WhisperVersionKey));
            Assert.Equal("width", rom.ReadString(Models.Metadata.Rom.WidthKey));
            Assert.Equal("word_conf_0_10", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval0To10Key));
            Assert.Equal("word_conf_11_20", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval11To20Key));
            Assert.Equal("word_conf_21_30", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval21To30Key));
            Assert.Equal("word_conf_31_40", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval31To40Key));
            Assert.Equal("word_conf_41_50", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval41To50Key));
            Assert.Equal("word_conf_51_60", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval51To60Key));
            Assert.Equal("word_conf_61_70", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval61To70Key));
            Assert.Equal("word_conf_71_80", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval71To80Key));
            Assert.Equal("word_conf_81_90", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval81To90Key));
            Assert.Equal("word_conf_91_100", rom.ReadString(Models.Metadata.Rom.WordConfidenceInterval91To100Key));
            Assert.Equal(ZeroHash.GetString(HashType.XxHash3), rom.ReadString(Models.Metadata.Rom.xxHash364Key));
            Assert.Equal(ZeroHash.GetString(HashType.XxHash128), rom.ReadString(Models.Metadata.Rom.xxHash3128Key));
        }

        private static void ValidateMetadataSample(Models.Metadata.Sample? sample)
        {
            Assert.NotNull(sample);
            Assert.Equal("name", sample.ReadString(Models.Metadata.Sample.NameKey));
        }

        private static void ValidateMetadataSharedFeat(Models.Metadata.SharedFeat? sharedFeat)
        {
            Assert.NotNull(sharedFeat);
            Assert.Equal("name", sharedFeat.ReadString(Models.Metadata.SharedFeat.NameKey));
            Assert.Equal("value", sharedFeat.ReadString(Models.Metadata.SharedFeat.ValueKey));
        }

        private static void ValidateMetadataSlot(Models.Metadata.Slot? slot)
        {
            Assert.NotNull(slot);
            Assert.Equal("name", slot.ReadString(Models.Metadata.Slot.NameKey));

            Models.Metadata.SlotOption[]? slotOptions = slot.ReadItemArray<Models.Metadata.SlotOption>(Models.Metadata.Slot.SlotOptionKey);
            Assert.NotNull(slotOptions);
            Models.Metadata.SlotOption? slotOption = Assert.Single(slotOptions);
            ValidateMetadataSlotOption(slotOption);
        }

        private static void ValidateMetadataSlotOption(Models.Metadata.SlotOption? slotOption)
        {
            Assert.NotNull(slotOption);
            Assert.True(slotOption.ReadBool(Models.Metadata.SlotOption.DefaultKey));
            Assert.Equal("devname", slotOption.ReadString(Models.Metadata.SlotOption.DevNameKey));
            Assert.Equal("name", slotOption.ReadString(Models.Metadata.SlotOption.NameKey));
        }

        private static void ValidateMetadataSoftwareList(Models.Metadata.SoftwareList? softwareList)
        {
            Assert.NotNull(softwareList);
            Assert.Equal("description", softwareList.ReadString(Models.Metadata.SoftwareList.DescriptionKey));
            Assert.Equal("filter", softwareList.ReadString(Models.Metadata.SoftwareList.FilterKey));
            Assert.Equal("name", softwareList.ReadString(Models.Metadata.SoftwareList.NameKey));
            Assert.Equal("notes", softwareList.ReadString(Models.Metadata.SoftwareList.NotesKey));
            Assert.Equal("original", softwareList.ReadString(Models.Metadata.SoftwareList.StatusKey));
            Assert.Equal("tag", softwareList.ReadString(Models.Metadata.SoftwareList.TagKey));

            // TODO: Figure out why Models.Metadata.SoftwareList.SoftwareKey doesn't get processed
        }

        private static void ValidateMetadataSound(Models.Metadata.Sound? sound)
        {
            Assert.NotNull(sound);
            Assert.Equal(12345, sound.ReadLong(Models.Metadata.Sound.ChannelsKey));
        }

        private static void ValidateMetadataTrurip(Models.Logiqx.Trurip? trurip)
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

        private static void ValidateMetadataVideo(Models.Metadata.Video? video)
        {
            Assert.NotNull(video);
            Assert.Equal(12345, video.ReadLong(Models.Metadata.Video.AspectXKey));
            Assert.Equal(12345, video.ReadLong(Models.Metadata.Video.AspectYKey));
            Assert.Equal(12345, video.ReadLong(Models.Metadata.Video.HeightKey));
            Assert.Equal("vertical", video.ReadString(Models.Metadata.Video.OrientationKey));
            Assert.Equal(12345, video.ReadLong(Models.Metadata.Video.RefreshKey));
            Assert.Equal("vector", video.ReadString(Models.Metadata.Video.ScreenKey));
            Assert.Equal(12345, video.ReadLong(Models.Metadata.Video.WidthKey));
        }

        #endregion
    }
}