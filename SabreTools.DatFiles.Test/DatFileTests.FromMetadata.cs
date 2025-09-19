using System;
using System.Linq;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using Xunit;

namespace SabreTools.DatFiles.Test
{
    public partial class DatFileTests
    {
        #region ConvertFromMetadata

        [Fact]
        public void ConvertFromMetadata_Null()
        {
            Models.Metadata.MetadataFile? item = null;

            DatFile datFile = new Formats.Logiqx(null, useGame: false);
            datFile.ConvertFromMetadata(item, "filename", indexId: 0, keep: true, statsOnly: false, filterRunner: null);

            Assert.Equal(0, datFile.Items.DatStatistics.TotalCount);
            Assert.Equal(0, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Fact]
        public void ConvertFromMetadata_Empty()
        {
            Models.Metadata.MetadataFile? item = new Models.Metadata.MetadataFile();

            DatFile datFile = new Formats.Logiqx(null, useGame: false);
            datFile.ConvertFromMetadata(item, "filename", indexId: 0, keep: true, statsOnly: false, filterRunner: null);

            Assert.Equal(0, datFile.Items.DatStatistics.TotalCount);
            Assert.Equal(0, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Fact]
        public void ConvertFromMetadata_FilledHeader()
        {
            Models.Metadata.Header? header = CreateMetadataHeader();
            Models.Metadata.Machine[]? machines = null;
            Models.Metadata.MetadataFile? item = new Models.Metadata.MetadataFile
            {
                [Models.Metadata.MetadataFile.HeaderKey] = header,
                [Models.Metadata.MetadataFile.MachineKey] = machines,
            };

            DatFile datFile = new Formats.Logiqx(null, useGame: false);
            datFile.ConvertFromMetadata(item, "filename", indexId: 0, keep: true, statsOnly: false, filterRunner: null);

            ValidateHeader(datFile.Header);
        }

        [Fact]
        public void ConvertFromMetadata_FilledMachine()
        {
            Models.Metadata.Header? header = null;
            Models.Metadata.Machine machine = CreateMetadataMachine();
            Models.Metadata.Machine[]? machines = [machine];
            Models.Metadata.MetadataFile? item = new Models.Metadata.MetadataFile
            {
                [Models.Metadata.MetadataFile.HeaderKey] = header,
                [Models.Metadata.MetadataFile.MachineKey] = machines,
            };

            DatFile datFile = new Formats.Logiqx(null, useGame: false);
            datFile.ConvertFromMetadata(item, "filename", indexId: 0, keep: true, statsOnly: false, filterRunner: null);

            // TODO: Reenable when ItemsDB is used again
            // DatItems.Machine actualMachine = Assert.Single(datFile.ItemsDB.GetMachines()).Value;
            // ValidateMachine(actualMachine);

            // Aggregate for easier validation
            DatItems.DatItem[] datItems = datFile.Items.SortedKeys
                .SelectMany(key => datFile.GetItemsForBucket(key))
                .ToArray();

            Adjuster? adjuster = Array.Find(datItems, item => item is Adjuster) as Adjuster;
            ValidateAdjuster(adjuster);

            Archive? archive = Array.Find(datItems, item => item is Archive) as Archive;
            ValidateArchive(archive);

            BiosSet? biosSet = Array.Find(datItems, item => item is BiosSet) as BiosSet;
            ValidateBiosSet(biosSet);

            Chip? chip = Array.Find(datItems, item => item is Chip) as Chip;
            ValidateChip(chip);

            Configuration? configuration = Array.Find(datItems, item => item is Configuration) as Configuration;
            ValidateConfiguration(configuration);

            Device? device = Array.Find(datItems, item => item is Device) as Device;
            ValidateDevice(device);

            DeviceRef? deviceRef = Array.Find(datItems, item => item is DeviceRef) as DeviceRef;
            ValidateDeviceRef(deviceRef);

            DipSwitch? dipSwitch = Array.Find(datItems, item => item is DipSwitch dipSwitch && !dipSwitch.PartSpecified) as DipSwitch;
            ValidateDipSwitch(dipSwitch);

            Disk? disk = Array.Find(datItems, item => item is Disk disk && !disk.DiskAreaSpecified && !disk.PartSpecified) as Disk;
            ValidateDisk(disk);

            Display? display = Array.Find(datItems, item => item is Display display && display.GetInt64FieldValue(Models.Metadata.Video.AspectXKey) == null) as Display;
            ValidateDisplay(display);

            Driver? driver = Array.Find(datItems, item => item is Driver) as Driver;
            ValidateDriver(driver);

            // All other fields are tested separately
            Rom? dump = Array.Find(datItems, item => item is Rom rom && rom.GetStringFieldValue(Models.Metadata.Rom.OpenMSXMediaType) != null) as Rom;
            Assert.NotNull(dump);
            Assert.Equal("rom", dump.GetStringFieldValue(Models.Metadata.Rom.OpenMSXMediaType));

            Feature? feature = Array.Find(datItems, item => item is Feature) as Feature;
            ValidateFeature(feature);

            Info? info = Array.Find(datItems, item => item is Info) as Info;
            ValidateInfo(info);

            Input? input = Array.Find(datItems, item => item is Input) as Input;
            ValidateInput(input);

            Media? media = Array.Find(datItems, item => item is Media) as Media;
            ValidateMedia(media);

            // All other fields are tested separately
            DipSwitch? partDipSwitch = Array.Find(datItems, item => item is DipSwitch dipSwitch && dipSwitch.PartSpecified) as DipSwitch;
            Assert.NotNull(partDipSwitch);
            Part? dipSwitchPart = partDipSwitch.GetFieldValue<Part>(DipSwitch.PartKey);
            ValidatePart(dipSwitchPart);

            // All other fields are tested separately
            Disk? partDisk = Array.Find(datItems, item => item is Disk disk && disk.DiskAreaSpecified && disk.PartSpecified) as Disk;
            Assert.NotNull(partDisk);
            DiskArea? diskDiskArea = partDisk.GetFieldValue<DiskArea>(Disk.DiskAreaKey);
            ValidateDiskArea(diskDiskArea);
            Part? diskPart = partDisk.GetFieldValue<Part>(Disk.PartKey);
            ValidatePart(diskPart);

            PartFeature? partFeature = Array.Find(datItems, item => item is PartFeature) as PartFeature;
            ValidatePartFeature(partFeature);

            // All other fields are tested separately
            Rom? partRom = Array.Find(datItems, item => item is Rom rom && rom.DataAreaSpecified && rom.PartSpecified) as Rom;
            Assert.NotNull(partRom);
            DataArea? romDataArea = partRom.GetFieldValue<DataArea>(Rom.DataAreaKey);
            ValidateDataArea(romDataArea);
            Part? romPart = partRom.GetFieldValue<Part>(Rom.PartKey);
            ValidatePart(romPart);

            Port? port = Array.Find(datItems, item => item is Port) as Port;
            ValidatePort(port);

            RamOption? ramOption = Array.Find(datItems, item => item is RamOption) as RamOption;
            ValidateRamOption(ramOption);

            Release? release = Array.Find(datItems, item => item is Release) as Release;
            ValidateRelease(release);

            Rom? rom = Array.Find(datItems, item => item is Rom rom && !rom.DataAreaSpecified && !rom.PartSpecified && rom.GetStringFieldValue(Models.Metadata.Rom.OpenMSXMediaType) == null) as Rom;
            ValidateRom(rom);

            Sample? sample = Array.Find(datItems, item => item is Sample) as Sample;
            ValidateSample(sample);

            SharedFeat? sharedFeat = Array.Find(datItems, item => item is SharedFeat) as SharedFeat;
            ValidateSharedFeat(sharedFeat);

            Slot? slot = Array.Find(datItems, item => item is Slot) as Slot;
            ValidateSlot(slot);

            SoftwareList? softwareList = Array.Find(datItems, item => item is SoftwareList) as SoftwareList;
            ValidateSoftwareList(softwareList);

            Sound? sound = Array.Find(datItems, item => item is Sound) as Sound;
            ValidateSound(sound);

            Display? video = Array.Find(datItems, item => item is Display display && display.GetInt64FieldValue(Models.Metadata.Video.AspectXKey) != null) as Display;
            ValidateVideo(video);
        }

        #endregion

        #region Creation Helpers

        private static Models.Metadata.Header CreateMetadataHeader()
        {
            Models.OfflineList.CanOpen canOpen = new Models.OfflineList.CanOpen
            {
                Extension = ["ext"],
            };

            Models.OfflineList.Images images = new Models.OfflineList.Images();

            Models.OfflineList.Infos infos = new Models.OfflineList.Infos();

            Models.OfflineList.NewDat newDat = new Models.OfflineList.NewDat();

            Models.OfflineList.Search search = new Models.OfflineList.Search();

            return new Models.Metadata.Header
            {
                [Models.Metadata.Header.AuthorKey] = "author",
                [Models.Metadata.Header.BiosModeKey] = "merged",
                [Models.Metadata.Header.BuildKey] = "build",
                [Models.Metadata.Header.CanOpenKey] = canOpen,
                [Models.Metadata.Header.CategoryKey] = "category",
                [Models.Metadata.Header.CommentKey] = "comment",
                [Models.Metadata.Header.DateKey] = "date",
                [Models.Metadata.Header.DatVersionKey] = "datversion",
                [Models.Metadata.Header.DebugKey] = "yes",
                [Models.Metadata.Header.DescriptionKey] = "description",
                [Models.Metadata.Header.EmailKey] = "email",
                [Models.Metadata.Header.EmulatorVersionKey] = "emulatorversion",
                [Models.Metadata.Header.ForceMergingKey] = "merged",
                [Models.Metadata.Header.ForceNodumpKey] = "required",
                [Models.Metadata.Header.ForcePackingKey] = "zip",
                [Models.Metadata.Header.ForceZippingKey] = "yes",
                [Models.Metadata.Header.HeaderKey] = "header",
                [Models.Metadata.Header.HomepageKey] = "homepage",
                [Models.Metadata.Header.IdKey] = "id",
                [Models.Metadata.Header.ImagesKey] = images,
                [Models.Metadata.Header.ImFolderKey] = "imfolder",
                [Models.Metadata.Header.InfosKey] = infos,
                [Models.Metadata.Header.LockBiosModeKey] = "yes",
                [Models.Metadata.Header.LockRomModeKey] = "yes",
                [Models.Metadata.Header.LockSampleModeKey] = "yes",
                [Models.Metadata.Header.MameConfigKey] = "mameconfig",
                [Models.Metadata.Header.NameKey] = "name",
                [Models.Metadata.Header.NewDatKey] = newDat,
                [Models.Metadata.Header.NotesKey] = "notes",
                [Models.Metadata.Header.PluginKey] = "plugin",
                [Models.Metadata.Header.RefNameKey] = "refname",
                [Models.Metadata.Header.RomModeKey] = "merged",
                [Models.Metadata.Header.RomTitleKey] = "romtitle",
                [Models.Metadata.Header.RootDirKey] = "rootdir",
                [Models.Metadata.Header.SampleModeKey] = "merged",
                [Models.Metadata.Header.SchemaLocationKey] = "schemalocation",
                [Models.Metadata.Header.ScreenshotsHeightKey] = "screenshotsheight",
                [Models.Metadata.Header.ScreenshotsWidthKey] = "screenshotsWidth",
                [Models.Metadata.Header.SearchKey] = search,
                [Models.Metadata.Header.SystemKey] = "system",
                [Models.Metadata.Header.TimestampKey] = "timestamp",
                [Models.Metadata.Header.TypeKey] = "type",
                [Models.Metadata.Header.UrlKey] = "url",
                [Models.Metadata.Header.VersionKey] = "version",
            };
        }

        private static Models.Metadata.Machine CreateMetadataMachine()
        {
            return new Models.Metadata.Machine
            {
                [Models.Metadata.Machine.AdjusterKey] = new Models.Metadata.Adjuster[] { CreateMetadataAdjuster() },
                [Models.Metadata.Machine.ArchiveKey] = new Models.Metadata.Archive[] { CreateMetadataArchive() },
                [Models.Metadata.Machine.BiosSetKey] = new Models.Metadata.BiosSet[] { CreateMetadataBiosSet() },
                [Models.Metadata.Machine.BoardKey] = "board",
                [Models.Metadata.Machine.ButtonsKey] = "buttons",
                [Models.Metadata.Machine.CategoryKey] = "category",
                [Models.Metadata.Machine.ChipKey] = new Models.Metadata.Chip[] { CreateMetadataChip() },
                [Models.Metadata.Machine.CloneOfKey] = "cloneof",
                [Models.Metadata.Machine.CloneOfIdKey] = "cloneofid",
                [Models.Metadata.Machine.CommentKey] = "comment",
                [Models.Metadata.Machine.CompanyKey] = "company",
                [Models.Metadata.Machine.ConfigurationKey] = new Models.Metadata.Configuration[] { CreateMetadataConfiguration() },
                [Models.Metadata.Machine.ControlKey] = "control",
                [Models.Metadata.Machine.CountryKey] = "country",
                [Models.Metadata.Machine.DescriptionKey] = "description",
                [Models.Metadata.Machine.DeviceKey] = new Models.Metadata.Device[] { CreateMetadataDevice() },
                [Models.Metadata.Machine.DeviceRefKey] = new Models.Metadata.DeviceRef[] { CreateMetadataDeviceRef() },
                [Models.Metadata.Machine.DipSwitchKey] = new Models.Metadata.DipSwitch[] { CreateMetadataDipSwitch() },
                [Models.Metadata.Machine.DirNameKey] = "dirname",
                [Models.Metadata.Machine.DiskKey] = new Models.Metadata.Disk[] { CreateMetadataDisk() },
                [Models.Metadata.Machine.DisplayCountKey] = "displaycount",
                [Models.Metadata.Machine.DisplayKey] = new Models.Metadata.Display[] { CreateMetadataDisplay() },
                [Models.Metadata.Machine.DisplayTypeKey] = "displaytype",
                [Models.Metadata.Machine.DriverKey] = CreateMetadataDriver(),
                [Models.Metadata.Machine.DumpKey] = new Models.Metadata.Dump[] { CreateMetadataDump() },
                [Models.Metadata.Machine.DuplicateIDKey] = "duplicateid",
                [Models.Metadata.Machine.EmulatorKey] = "emulator",
                [Models.Metadata.Machine.ExtraKey] = "extra",
                [Models.Metadata.Machine.FavoriteKey] = "favorite",
                [Models.Metadata.Machine.FeatureKey] = new Models.Metadata.Feature[] { CreateMetadataFeature() },
                [Models.Metadata.Machine.GenMSXIDKey] = "genmsxid",
                [Models.Metadata.Machine.HistoryKey] = "history",
                [Models.Metadata.Machine.IdKey] = "id",
                [Models.Metadata.Machine.Im1CRCKey] = ZeroHash.CRC32Str,
                [Models.Metadata.Machine.Im2CRCKey] = ZeroHash.CRC32Str,
                [Models.Metadata.Machine.ImageNumberKey] = "imagenumber",
                [Models.Metadata.Machine.InfoKey] = new Models.Metadata.Info[] { CreateMetadataInfo() },
                [Models.Metadata.Machine.InputKey] = CreateMetadataInput(),
                [Models.Metadata.Machine.IsBiosKey] = "yes",
                [Models.Metadata.Machine.IsDeviceKey] = "yes",
                [Models.Metadata.Machine.IsMechanicalKey] = "yes",
                [Models.Metadata.Machine.LanguageKey] = "language",
                [Models.Metadata.Machine.LocationKey] = "location",
                [Models.Metadata.Machine.ManufacturerKey] = "manufacturer",
                [Models.Metadata.Machine.MediaKey] = new Models.Metadata.Media[] { CreateMetadataMedia() },
                [Models.Metadata.Machine.NameKey] = "name",
                [Models.Metadata.Machine.NotesKey] = "notes",
                [Models.Metadata.Machine.PartKey] = new Models.Metadata.Part[] { CreateMetadataPart() },
                [Models.Metadata.Machine.PlayedCountKey] = "playedcount",
                [Models.Metadata.Machine.PlayedTimeKey] = "playedtime",
                [Models.Metadata.Machine.PlayersKey] = "players",
                [Models.Metadata.Machine.PortKey] = new Models.Metadata.Port[] { CreateMetadataPort() },
                [Models.Metadata.Machine.PublisherKey] = "publisher",
                [Models.Metadata.Machine.RamOptionKey] = new Models.Metadata.RamOption[] { CreateMetadataRamOption() },
                [Models.Metadata.Machine.RebuildToKey] = "rebuildto",
                [Models.Metadata.Machine.ReleaseKey] = new Models.Metadata.Release[] { CreateMetadataRelease() },
                [Models.Metadata.Machine.ReleaseNumberKey] = "releasenumber",
                [Models.Metadata.Machine.RomKey] = new Models.Metadata.Rom[] { CreateMetadataRom() },
                [Models.Metadata.Machine.RomOfKey] = "romof",
                [Models.Metadata.Machine.RotationKey] = "rotation",
                [Models.Metadata.Machine.RunnableKey] = "yes",
                [Models.Metadata.Machine.SampleKey] = new Models.Metadata.Sample[] { CreateMetadataSample() },
                [Models.Metadata.Machine.SampleOfKey] = "sampleof",
                [Models.Metadata.Machine.SaveTypeKey] = "savetype",
                [Models.Metadata.Machine.SharedFeatKey] = new Models.Metadata.SharedFeat[] { CreateMetadataSharedFeat() },
                [Models.Metadata.Machine.SlotKey] = new Models.Metadata.Slot[] { CreateMetadataSlot() },
                [Models.Metadata.Machine.SoftwareListKey] = new Models.Metadata.SoftwareList[] { CreateMetadataSoftwareList() },
                [Models.Metadata.Machine.SoundKey] = CreateMetadataSound(),
                [Models.Metadata.Machine.SourceFileKey] = "sourcefile",
                [Models.Metadata.Machine.SourceRomKey] = "sourcerom",
                [Models.Metadata.Machine.StatusKey] = "status",
                [Models.Metadata.Machine.SupportedKey] = "yes",
                [Models.Metadata.Machine.SystemKey] = "system",
                [Models.Metadata.Machine.TagsKey] = "tags",
                [Models.Metadata.Machine.TruripKey] = CreateMetadataTrurip(),
                [Models.Metadata.Machine.VideoKey] = new Models.Metadata.Video[] { CreateMetadataVideo() },
                [Models.Metadata.Machine.YearKey] = "year",
            };
        }

        private static Models.Metadata.Adjuster CreateMetadataAdjuster()
        {
            return new Models.Metadata.Adjuster
            {
                [Models.Metadata.Adjuster.ConditionKey] = CreateMetadataCondition(),
                [Models.Metadata.Adjuster.DefaultKey] = true,
                [Models.Metadata.Adjuster.NameKey] = "name",
            };
        }

        private static Models.Metadata.Analog CreateMetadataAnalog()
        {
            return new Models.Metadata.Analog
            {
                [Models.Metadata.Analog.MaskKey] = "mask",
            };
        }

        private static Models.Metadata.Archive CreateMetadataArchive()
        {
            return new Models.Metadata.Archive
            {
                [Models.Metadata.Archive.NameKey] = "name",
            };
        }

        private static Models.Metadata.BiosSet CreateMetadataBiosSet()
        {
            return new Models.Metadata.BiosSet
            {
                [Models.Metadata.BiosSet.DefaultKey] = true,
                [Models.Metadata.BiosSet.DescriptionKey] = "description",
                [Models.Metadata.BiosSet.NameKey] = "name",
            };
        }

        private static Models.Metadata.Chip CreateMetadataChip()
        {
            return new Models.Metadata.Chip
            {
                [Models.Metadata.Chip.ClockKey] = 12345L,
                [Models.Metadata.Chip.FlagsKey] = "flags",
                [Models.Metadata.Chip.NameKey] = "name",
                [Models.Metadata.Chip.SoundOnlyKey] = "yes",
                [Models.Metadata.Chip.TagKey] = "tag",
                [Models.Metadata.Chip.ChipTypeKey] = "cpu",
            };
        }

        private static Models.Metadata.Configuration CreateMetadataConfiguration()
        {
            return new Models.Metadata.Configuration
            {
                [Models.Metadata.Configuration.ConditionKey] = CreateMetadataCondition(),
                [Models.Metadata.Configuration.ConfLocationKey] = new Models.Metadata.ConfLocation[] { CreateMetadataConfLocation() },
                [Models.Metadata.Configuration.ConfSettingKey] = new Models.Metadata.ConfSetting[] { CreateMetadataConfSetting() },
                [Models.Metadata.Configuration.MaskKey] = "mask",
                [Models.Metadata.Configuration.NameKey] = "name",
                [Models.Metadata.Configuration.TagKey] = "tag",
            };
        }

        private static Models.Metadata.Condition CreateMetadataCondition()
        {
            return new Models.Metadata.Condition
            {
                [Models.Metadata.Condition.ValueKey] = "value",
                [Models.Metadata.Condition.MaskKey] = "mask",
                [Models.Metadata.Condition.RelationKey] = "eq",
                [Models.Metadata.Condition.TagKey] = "tag",
            };
        }

        private static Models.Metadata.ConfLocation CreateMetadataConfLocation()
        {
            return new Models.Metadata.ConfLocation
            {
                [Models.Metadata.ConfLocation.InvertedKey] = "yes",
                [Models.Metadata.ConfLocation.NameKey] = "name",
                [Models.Metadata.ConfLocation.NumberKey] = "number",
            };
        }

        private static Models.Metadata.ConfSetting CreateMetadataConfSetting()
        {
            return new Models.Metadata.ConfSetting
            {
                [Models.Metadata.ConfSetting.ConditionKey] = CreateMetadataCondition(),
                [Models.Metadata.ConfSetting.DefaultKey] = "yes",
                [Models.Metadata.ConfSetting.NameKey] = "name",
                [Models.Metadata.ConfSetting.ValueKey] = "value",
            };
        }

        private static Models.Metadata.Control CreateMetadataControl()
        {
            return new Models.Metadata.Control
            {
                [Models.Metadata.Control.ButtonsKey] = 12345L,
                [Models.Metadata.Control.KeyDeltaKey] = 12345L,
                [Models.Metadata.Control.MaximumKey] = 12345L,
                [Models.Metadata.Control.MinimumKey] = 12345L,
                [Models.Metadata.Control.PlayerKey] = 12345L,
                [Models.Metadata.Control.ReqButtonsKey] = 12345L,
                [Models.Metadata.Control.ReverseKey] = "yes",
                [Models.Metadata.Control.SensitivityKey] = 12345L,
                [Models.Metadata.Control.ControlTypeKey] = "lightgun",
                [Models.Metadata.Control.WaysKey] = "ways",
                [Models.Metadata.Control.Ways2Key] = "ways2",
                [Models.Metadata.Control.Ways3Key] = "ways3",
            };
        }

        private static Models.Metadata.Device CreateMetadataDevice()
        {
            return new Models.Metadata.Device
            {
                [Models.Metadata.Device.ExtensionKey] = new Models.Metadata.Extension[] { CreateMetadataExtension() },
                [Models.Metadata.Device.FixedImageKey] = "fixedimage",
                [Models.Metadata.Device.InstanceKey] = CreateMetadataInstance(),
                [Models.Metadata.Device.InterfaceKey] = "interface",
                [Models.Metadata.Device.MandatoryKey] = 1L,
                [Models.Metadata.Device.TagKey] = "tag",
                [Models.Metadata.Device.DeviceTypeKey] = "punchtape",
            };
        }

        private static Models.Metadata.DeviceRef CreateMetadataDeviceRef()
        {
            return new Models.Metadata.DeviceRef
            {
                [Models.Metadata.DeviceRef.NameKey] = "name",
            };
        }

        private static Models.Metadata.DipLocation CreateMetadataDipLocation()
        {
            return new Models.Metadata.DipLocation
            {
                [Models.Metadata.DipLocation.InvertedKey] = "yes",
                [Models.Metadata.DipLocation.NameKey] = "name",
                [Models.Metadata.DipLocation.NumberKey] = "number",
            };
        }

        private static Models.Metadata.DipSwitch CreateMetadataDipSwitch()
        {
            return new Models.Metadata.DipSwitch
            {
                [Models.Metadata.DipSwitch.ConditionKey] = CreateMetadataCondition(),
                [Models.Metadata.DipSwitch.DefaultKey] = "yes",
                [Models.Metadata.DipSwitch.DipLocationKey] = new Models.Metadata.DipLocation[] { CreateMetadataDipLocation() },
                [Models.Metadata.DipSwitch.DipValueKey] = new Models.Metadata.DipValue[] { CreateMetadataDipValue() },
                [Models.Metadata.DipSwitch.EntryKey] = new string[] { "entry" },
                [Models.Metadata.DipSwitch.MaskKey] = "mask",
                [Models.Metadata.DipSwitch.NameKey] = "name",
                [Models.Metadata.DipSwitch.TagKey] = "tag",
            };
        }

        private static Models.Metadata.DipValue CreateMetadataDipValue()
        {
            return new Models.Metadata.DipValue
            {
                [Models.Metadata.DipValue.ConditionKey] = CreateMetadataCondition(),
                [Models.Metadata.DipValue.DefaultKey] = "yes",
                [Models.Metadata.DipValue.NameKey] = "name",
                [Models.Metadata.DipValue.ValueKey] = "value",
            };
        }

        private static Models.Metadata.DataArea CreateMetadataDataArea()
        {
            return new Models.Metadata.DataArea
            {
                [Models.Metadata.DataArea.EndiannessKey] = "big",
                [Models.Metadata.DataArea.NameKey] = "name",
                [Models.Metadata.DataArea.RomKey] = new Models.Metadata.Rom[] { new Models.Metadata.Rom() },
                [Models.Metadata.DataArea.SizeKey] = 12345L,
                [Models.Metadata.DataArea.WidthKey] = 64,
            };
        }

        private static Models.Metadata.Disk CreateMetadataDisk()
        {
            return new Models.Metadata.Disk
            {
                [Models.Metadata.Disk.FlagsKey] = "flags",
                [Models.Metadata.Disk.IndexKey] = "index",
                [Models.Metadata.Disk.MD5Key] = ZeroHash.MD5Str,
                [Models.Metadata.Disk.MergeKey] = "merge",
                [Models.Metadata.Disk.NameKey] = "name",
                [Models.Metadata.Disk.OptionalKey] = "yes",
                [Models.Metadata.Disk.RegionKey] = "region",
                [Models.Metadata.Disk.SHA1Key] = ZeroHash.SHA1Str,
                [Models.Metadata.Disk.WritableKey] = "yes",
            };
        }

        private static Models.Metadata.DiskArea CreateMetadataDiskArea()
        {
            return new Models.Metadata.DiskArea
            {
                [Models.Metadata.DiskArea.DiskKey] = new Models.Metadata.Disk[] { new Models.Metadata.Disk() },
                [Models.Metadata.DiskArea.NameKey] = "name",
            };
        }

        private static Models.Metadata.Display CreateMetadataDisplay()
        {
            return new Models.Metadata.Display
            {
                [Models.Metadata.Display.FlipXKey] = "yes",
                [Models.Metadata.Display.HBEndKey] = 12345L,
                [Models.Metadata.Display.HBStartKey] = 12345L,
                [Models.Metadata.Display.HeightKey] = 12345L,
                [Models.Metadata.Display.HTotalKey] = 12345L,
                [Models.Metadata.Display.PixClockKey] = 12345L,
                [Models.Metadata.Display.RefreshKey] = 12345L,
                [Models.Metadata.Display.RotateKey] = 90,
                [Models.Metadata.Display.TagKey] = "tag",
                [Models.Metadata.Display.DisplayTypeKey] = "vector",
                [Models.Metadata.Display.VBEndKey] = 12345L,
                [Models.Metadata.Display.VBStartKey] = 12345L,
                [Models.Metadata.Display.VTotalKey] = 12345L,
                [Models.Metadata.Display.WidthKey] = 12345L,
            };
        }

        private static Models.Metadata.Driver CreateMetadataDriver()
        {
            return new Models.Metadata.Driver
            {
                [Models.Metadata.Driver.BlitKey] = "plain",
                [Models.Metadata.Driver.CocktailKey] = "good",
                [Models.Metadata.Driver.ColorKey] = "good",
                [Models.Metadata.Driver.EmulationKey] = "good",
                [Models.Metadata.Driver.IncompleteKey] = "yes",
                [Models.Metadata.Driver.NoSoundHardwareKey] = "yes",
                [Models.Metadata.Driver.PaletteSizeKey] = "pallettesize",
                [Models.Metadata.Driver.RequiresArtworkKey] = "yes",
                [Models.Metadata.Driver.SaveStateKey] = "supported",
                [Models.Metadata.Driver.SoundKey] = "good",
                [Models.Metadata.Driver.StatusKey] = "good",
                [Models.Metadata.Driver.UnofficialKey] = "yes",
            };
        }

        private static Models.Metadata.Dump CreateMetadataDump()
        {
            return new Models.Metadata.Dump
            {
                [Models.Metadata.Dump.OriginalKey] = CreateMetadataOriginal(),

                // The following are searched for in order
                // For the purposes of this test, only RomKey will be populated
                // The only difference is what OpenMSXSubType value is applied
                [Models.Metadata.Dump.RomKey] = new Models.Metadata.Rom(),
                [Models.Metadata.Dump.MegaRomKey] = null,
                [Models.Metadata.Dump.SCCPlusCartKey] = null,
            };
        }

        private static Models.Metadata.Extension CreateMetadataExtension()
        {
            return new Models.Metadata.Extension
            {
                [Models.Metadata.Extension.NameKey] = "name",
            };
        }

        private static Models.Metadata.Feature CreateMetadataFeature()
        {
            return new Models.Metadata.Feature
            {
                [Models.Metadata.Feature.NameKey] = "name",
                [Models.Metadata.Feature.OverallKey] = "imperfect",
                [Models.Metadata.Feature.StatusKey] = "imperfect",
                [Models.Metadata.Feature.FeatureTypeKey] = "protection",
                [Models.Metadata.Feature.ValueKey] = "value",
            };
        }

        private static Models.Metadata.Info CreateMetadataInfo()
        {
            return new Models.Metadata.Info
            {
                [Models.Metadata.Info.NameKey] = "name",
                [Models.Metadata.Info.ValueKey] = "value",
            };
        }

        private static Models.Metadata.Input CreateMetadataInput()
        {
            return new Models.Metadata.Input
            {
                [Models.Metadata.Input.ButtonsKey] = 12345L,
                [Models.Metadata.Input.CoinsKey] = 12345L,
                [Models.Metadata.Input.ControlKey] = new Models.Metadata.Control[] { CreateMetadataControl() },
                [Models.Metadata.Input.PlayersKey] = 12345L,
                [Models.Metadata.Input.ServiceKey] = "yes",
                [Models.Metadata.Input.TiltKey] = "yes",
            };
        }

        private static Models.Metadata.Instance CreateMetadataInstance()
        {
            return new Models.Metadata.Instance
            {
                [Models.Metadata.Instance.BriefNameKey] = "briefname",
                [Models.Metadata.Instance.NameKey] = "name",
            };
        }

        private static Models.Metadata.Media CreateMetadataMedia()
        {
            return new Models.Metadata.Media
            {
                [Models.Metadata.Media.MD5Key] = ZeroHash.MD5Str,
                [Models.Metadata.Media.NameKey] = "name",
                [Models.Metadata.Media.SHA1Key] = ZeroHash.SHA1Str,
                [Models.Metadata.Media.SHA256Key] = ZeroHash.SHA256Str,
                [Models.Metadata.Media.SpamSumKey] = ZeroHash.SpamSumStr,
            };
        }

        private static Models.Metadata.Original CreateMetadataOriginal()
        {
            return new Models.Metadata.Original
            {
                [Models.Metadata.Original.ContentKey] = "content",
                [Models.Metadata.Original.ValueKey] = true,
            };
        }

        private static Models.Metadata.Part CreateMetadataPart()
        {
            return new Models.Metadata.Part
            {
                [Models.Metadata.Part.DataAreaKey] = new Models.Metadata.DataArea[] { CreateMetadataDataArea() },
                [Models.Metadata.Part.DiskAreaKey] = new Models.Metadata.DiskArea[] { CreateMetadataDiskArea() },
                [Models.Metadata.Part.DipSwitchKey] = new Models.Metadata.DipSwitch[] { new Models.Metadata.DipSwitch() },
                [Models.Metadata.Part.FeatureKey] = new Models.Metadata.Feature[] { CreateMetadataFeature() },
                [Models.Metadata.Part.InterfaceKey] = "interface",
                [Models.Metadata.Part.NameKey] = "name",
            };
        }

        private static Models.Metadata.Port CreateMetadataPort()
        {
            return new Models.Metadata.Port
            {
                [Models.Metadata.Port.AnalogKey] = new Models.Metadata.Analog[] { CreateMetadataAnalog() },
                [Models.Metadata.Port.TagKey] = "tag",
            };
        }

        private static Models.Metadata.RamOption CreateMetadataRamOption()
        {
            return new Models.Metadata.RamOption
            {
                [Models.Metadata.RamOption.ContentKey] = "content",
                [Models.Metadata.RamOption.DefaultKey] = "yes",
                [Models.Metadata.RamOption.NameKey] = "name",
            };
        }

        private static Models.Metadata.Release CreateMetadataRelease()
        {
            return new Models.Metadata.Release
            {
                [Models.Metadata.Release.DateKey] = "date",
                [Models.Metadata.Release.DefaultKey] = "yes",
                [Models.Metadata.Release.LanguageKey] = "language",
                [Models.Metadata.Release.NameKey] = "name",
                [Models.Metadata.Release.RegionKey] = "region",
            };
        }

        private static Models.Metadata.Rom CreateMetadataRom()
        {
            return new Models.Metadata.Rom
            {
                [Models.Metadata.Rom.AlbumKey] = "album",
                [Models.Metadata.Rom.AltRomnameKey] = "alt_romname",
                [Models.Metadata.Rom.AltTitleKey] = "alt_title",
                [Models.Metadata.Rom.ArtistKey] = "artist",
                [Models.Metadata.Rom.ASRDetectedLangKey] = "asr_detected_lang",
                [Models.Metadata.Rom.ASRDetectedLangConfKey] = "asr_detected_lang_conf",
                [Models.Metadata.Rom.ASRTranscribedLangKey] = "asr_transcribed_lang",
                [Models.Metadata.Rom.BiosKey] = "bios",
                [Models.Metadata.Rom.BitrateKey] = "bitrate",
                [Models.Metadata.Rom.BitTorrentMagnetHashKey] = "btih",
                [Models.Metadata.Rom.ClothCoverDetectionModuleVersionKey] = "cloth_cover_detection_module_version",
                [Models.Metadata.Rom.CollectionCatalogNumberKey] = "collection-catalog-number",
                [Models.Metadata.Rom.CommentKey] = "comment",
                [Models.Metadata.Rom.CRCKey] = ZeroHash.CRC32Str,
                [Models.Metadata.Rom.CreatorKey] = "creator",
                [Models.Metadata.Rom.DateKey] = "date",
                [Models.Metadata.Rom.DisposeKey] = "yes",
                [Models.Metadata.Rom.ExtensionKey] = "extension",
                [Models.Metadata.Rom.FileCountKey] = 12345L,
                [Models.Metadata.Rom.FileIsAvailableKey] = true,
                [Models.Metadata.Rom.FlagsKey] = "flags",
                [Models.Metadata.Rom.FormatKey] = "format",
                [Models.Metadata.Rom.HeaderKey] = "header",
                [Models.Metadata.Rom.HeightKey] = "height",
                [Models.Metadata.Rom.hOCRCharToWordhOCRVersionKey] = "hocr_char_to_word_hocr_version",
                [Models.Metadata.Rom.hOCRCharToWordModuleVersionKey] = "hocr_char_to_word_module_version",
                [Models.Metadata.Rom.hOCRFtsTexthOCRVersionKey] = "hocr_fts_text_hocr_version",
                [Models.Metadata.Rom.hOCRFtsTextModuleVersionKey] = "hocr_fts_text_module_version",
                [Models.Metadata.Rom.hOCRPageIndexhOCRVersionKey] = "hocr_pageindex_hocr_version",
                [Models.Metadata.Rom.hOCRPageIndexModuleVersionKey] = "hocr_pageindex_module_version",
                [Models.Metadata.Rom.InvertedKey] = "yes",
                [Models.Metadata.Rom.LastModifiedTimeKey] = "mtime",
                [Models.Metadata.Rom.LengthKey] = "length",
                [Models.Metadata.Rom.LoadFlagKey] = "load16_byte",
                [Models.Metadata.Rom.MatrixNumberKey] = "matrix_number",
                [Models.Metadata.Rom.MD2Key] = ZeroHash.GetString(HashType.MD2),
                [Models.Metadata.Rom.MD4Key] = ZeroHash.GetString(HashType.MD4),
                [Models.Metadata.Rom.MD5Key] = ZeroHash.MD5Str,
                // [Models.Metadata.Rom.OpenMSXMediaType] = null, // Omit due to other test
                [Models.Metadata.Rom.MergeKey] = "merge",
                [Models.Metadata.Rom.MIAKey] = "yes",
                [Models.Metadata.Rom.NameKey] = "name",
                [Models.Metadata.Rom.TesseractOCRKey] = "ocr",
                [Models.Metadata.Rom.TesseractOCRConvertedKey] = "ocr_converted",
                [Models.Metadata.Rom.TesseractOCRDetectedLangKey] = "ocr_detected_lang",
                [Models.Metadata.Rom.TesseractOCRDetectedLangConfKey] = "ocr_detected_lang_conf",
                [Models.Metadata.Rom.TesseractOCRDetectedScriptKey] = "ocr_detected_script",
                [Models.Metadata.Rom.TesseractOCRDetectedScriptConfKey] = "ocr_detected_script_conf",
                [Models.Metadata.Rom.TesseractOCRModuleVersionKey] = "ocr_module_version",
                [Models.Metadata.Rom.TesseractOCRParametersKey] = "ocr_parameters",
                [Models.Metadata.Rom.OffsetKey] = "offset",
                [Models.Metadata.Rom.OptionalKey] = "yes",
                [Models.Metadata.Rom.OriginalKey] = "original",
                [Models.Metadata.Rom.PDFModuleVersionKey] = "pdf_module_version",
                [Models.Metadata.Rom.PreviewImageKey] = "preview-image",
                [Models.Metadata.Rom.PublisherKey] = "publisher",
                [Models.Metadata.Rom.RegionKey] = "region",
                [Models.Metadata.Rom.RemarkKey] = "remark",
                ["RIPEMD128"] = ZeroHash.GetString(HashType.RIPEMD128),
                ["RIPEMD160"] = ZeroHash.GetString(HashType.RIPEMD160),
                [Models.Metadata.Rom.RotationKey] = "rotation",
                [Models.Metadata.Rom.SerialKey] = "serial",
                [Models.Metadata.Rom.SHA1Key] = ZeroHash.SHA1Str,
                [Models.Metadata.Rom.SHA256Key] = ZeroHash.SHA256Str,
                [Models.Metadata.Rom.SHA384Key] = ZeroHash.SHA384Str,
                [Models.Metadata.Rom.SHA512Key] = ZeroHash.SHA512Str,
                [Models.Metadata.Rom.SizeKey] = 12345L,
                [Models.Metadata.Rom.SoundOnlyKey] = "yes",
                [Models.Metadata.Rom.SourceKey] = "source",
                [Models.Metadata.Rom.SpamSumKey] = ZeroHash.SpamSumStr,
                [Models.Metadata.Rom.StartKey] = "start",
                [Models.Metadata.Rom.StatusKey] = "good",
                [Models.Metadata.Rom.SummationKey] = "summation",
                [Models.Metadata.Rom.TitleKey] = "title",
                [Models.Metadata.Rom.TrackKey] = "track",
                [Models.Metadata.Rom.OpenMSXType] = "type",
                [Models.Metadata.Rom.ValueKey] = "value",
                [Models.Metadata.Rom.WhisperASRModuleVersionKey] = "whisper_asr_module_version",
                [Models.Metadata.Rom.WhisperModelHashKey] = "whisper_model_hash",
                [Models.Metadata.Rom.WhisperModelNameKey] = "whisper_model_name",
                [Models.Metadata.Rom.WhisperVersionKey] = "whisper_version",
                [Models.Metadata.Rom.WidthKey] = "width",
                [Models.Metadata.Rom.WordConfidenceInterval0To10Key] = "word_conf_0_10",
                [Models.Metadata.Rom.WordConfidenceInterval11To20Key] = "word_conf_11_20",
                [Models.Metadata.Rom.WordConfidenceInterval21To30Key] = "word_conf_21_30",
                [Models.Metadata.Rom.WordConfidenceInterval31To40Key] = "word_conf_31_40",
                [Models.Metadata.Rom.WordConfidenceInterval41To50Key] = "word_conf_41_50",
                [Models.Metadata.Rom.WordConfidenceInterval51To60Key] = "word_conf_51_60",
                [Models.Metadata.Rom.WordConfidenceInterval61To70Key] = "word_conf_61_70",
                [Models.Metadata.Rom.WordConfidenceInterval71To80Key] = "word_conf_71_80",
                [Models.Metadata.Rom.WordConfidenceInterval81To90Key] = "word_conf_81_90",
                [Models.Metadata.Rom.WordConfidenceInterval91To100Key] = "word_conf_91_100",
                [Models.Metadata.Rom.xxHash364Key] = ZeroHash.GetString(HashType.XxHash3),
                [Models.Metadata.Rom.xxHash3128Key] = ZeroHash.GetString(HashType.XxHash128),
            };
        }

        private static Models.Metadata.Sample CreateMetadataSample()
        {
            return new Models.Metadata.Sample
            {
                [Models.Metadata.Sample.NameKey] = "name",
            };
        }

        private static Models.Metadata.SharedFeat CreateMetadataSharedFeat()
        {
            return new Models.Metadata.SharedFeat
            {
                [Models.Metadata.SharedFeat.NameKey] = "name",
                [Models.Metadata.SharedFeat.ValueKey] = "value",
            };
        }

        private static Models.Metadata.Slot CreateMetadataSlot()
        {
            return new Models.Metadata.Slot
            {
                [Models.Metadata.Slot.NameKey] = "name",
                [Models.Metadata.Slot.SlotOptionKey] = new Models.Metadata.SlotOption[] { CreateMetadataSlotOption() },
            };
        }

        private static Models.Metadata.SlotOption CreateMetadataSlotOption()
        {
            return new Models.Metadata.SlotOption
            {
                [Models.Metadata.SlotOption.DefaultKey] = "yes",
                [Models.Metadata.SlotOption.DevNameKey] = "devname",
                [Models.Metadata.SlotOption.NameKey] = "name",
            };
        }

        private static Models.Metadata.Software CreateMetadataSoftware()
        {
            return new Models.Metadata.Software
            {
                [Models.Metadata.Software.CloneOfKey] = "cloneof",
                [Models.Metadata.Software.DescriptionKey] = "description",
                [Models.Metadata.Software.InfoKey] = new Models.Metadata.Info[] { CreateMetadataInfo() },
                [Models.Metadata.Software.NameKey] = "name",
                [Models.Metadata.Software.NotesKey] = "notes",
                [Models.Metadata.Software.PartKey] = new Models.Metadata.Part[] { CreateMetadataPart() },
                [Models.Metadata.Software.PublisherKey] = "publisher",
                [Models.Metadata.Software.SharedFeatKey] = new Models.Metadata.SharedFeat[] { CreateMetadataSharedFeat() },
                [Models.Metadata.Software.SupportedKey] = "yes",
                [Models.Metadata.Software.YearKey] = "year",
            };
        }

        private static Models.Metadata.SoftwareList CreateMetadataSoftwareList()
        {
            return new Models.Metadata.SoftwareList
            {
                [Models.Metadata.SoftwareList.DescriptionKey] = "description",
                [Models.Metadata.SoftwareList.FilterKey] = "filter",
                [Models.Metadata.SoftwareList.NameKey] = "name",
                [Models.Metadata.SoftwareList.NotesKey] = "notes",
                [Models.Metadata.SoftwareList.SoftwareKey] = new Models.Metadata.Software[] { CreateMetadataSoftware() },
                [Models.Metadata.SoftwareList.StatusKey] = "original",
                [Models.Metadata.SoftwareList.TagKey] = "tag",
            };
        }

        private static Models.Metadata.Sound CreateMetadataSound()
        {
            return new Models.Metadata.Sound
            {
                [Models.Metadata.Sound.ChannelsKey] = 12345L,
            };
        }

        private static Models.Logiqx.Trurip CreateMetadataTrurip()
        {
            return new Models.Logiqx.Trurip
            {
                TitleID = "titleid",
                Publisher = "publisher",
                Developer = "developer",
                Year = "year",
                Genre = "genre",
                Subgenre = "subgenre",
                Ratings = "ratings",
                Score = "score",
                Players = "players",
                Enabled = "enabled",
                CRC = "true",
                Source = "source",
                CloneOf = "cloneof",
                RelatedTo = "relatedto",
            };
        }

        private static Models.Metadata.Video CreateMetadataVideo()
        {
            return new Models.Metadata.Video
            {
                [Models.Metadata.Video.AspectXKey] = 12345L,
                [Models.Metadata.Video.AspectYKey] = 12345L,
                [Models.Metadata.Video.HeightKey] = 12345L,
                [Models.Metadata.Video.OrientationKey] = "vertical",
                [Models.Metadata.Video.RefreshKey] = 12345L,
                [Models.Metadata.Video.ScreenKey] = "vector",
                [Models.Metadata.Video.WidthKey] = 12345L,
            };
        }

        #endregion

        #region Validation Helpers

        private static void ValidateHeader(DatHeader datHeader)
        {
            Assert.Equal("author", datHeader.GetStringFieldValue(Models.Metadata.Header.AuthorKey));
            Assert.Equal("merged", datHeader.GetStringFieldValue(Models.Metadata.Header.BiosModeKey));
            Assert.Equal("build", datHeader.GetStringFieldValue(Models.Metadata.Header.BuildKey));
            Assert.Equal("ext", datHeader.GetStringFieldValue(Models.Metadata.Header.CanOpenKey));
            Assert.Equal("category", datHeader.GetStringFieldValue(Models.Metadata.Header.CategoryKey));
            Assert.Equal("comment", datHeader.GetStringFieldValue(Models.Metadata.Header.CommentKey));
            Assert.Equal("date", datHeader.GetStringFieldValue(Models.Metadata.Header.DateKey));
            Assert.Equal("datversion", datHeader.GetStringFieldValue(Models.Metadata.Header.DatVersionKey));
            Assert.True(datHeader.GetBoolFieldValue(Models.Metadata.Header.DebugKey));
            Assert.Equal("description", datHeader.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
            Assert.Equal("email", datHeader.GetStringFieldValue(Models.Metadata.Header.EmailKey));
            Assert.Equal("emulatorversion", datHeader.GetStringFieldValue(Models.Metadata.Header.EmulatorVersionKey));
            Assert.Equal("merged", datHeader.GetStringFieldValue(Models.Metadata.Header.ForceMergingKey));
            Assert.Equal("required", datHeader.GetStringFieldValue(Models.Metadata.Header.ForceNodumpKey));
            Assert.Equal("zip", datHeader.GetStringFieldValue(Models.Metadata.Header.ForcePackingKey));
            Assert.True(datHeader.GetBoolFieldValue(Models.Metadata.Header.ForceZippingKey));
            Assert.Equal("header", datHeader.GetStringFieldValue(Models.Metadata.Header.HeaderKey));
            Assert.Equal("homepage", datHeader.GetStringFieldValue(Models.Metadata.Header.HomepageKey));
            Assert.Equal("id", datHeader.GetStringFieldValue(Models.Metadata.Header.IdKey));
            Assert.NotNull(datHeader.GetStringFieldValue(Models.Metadata.Header.ImagesKey));
            Assert.Equal("imfolder", datHeader.GetStringFieldValue(Models.Metadata.Header.ImFolderKey));
            Assert.NotNull(datHeader.GetStringFieldValue(Models.Metadata.Header.InfosKey));
            Assert.True(datHeader.GetBoolFieldValue(Models.Metadata.Header.LockBiosModeKey));
            Assert.True(datHeader.GetBoolFieldValue(Models.Metadata.Header.LockRomModeKey));
            Assert.True(datHeader.GetBoolFieldValue(Models.Metadata.Header.LockSampleModeKey));
            Assert.Equal("mameconfig", datHeader.GetStringFieldValue(Models.Metadata.Header.MameConfigKey));
            Assert.Equal("name", datHeader.GetStringFieldValue(Models.Metadata.Header.NameKey));
            Assert.NotNull(datHeader.GetStringFieldValue(Models.Metadata.Header.NewDatKey));
            Assert.Equal("notes", datHeader.GetStringFieldValue(Models.Metadata.Header.NotesKey));
            Assert.Equal("plugin", datHeader.GetStringFieldValue(Models.Metadata.Header.PluginKey));
            Assert.Equal("refname", datHeader.GetStringFieldValue(Models.Metadata.Header.RefNameKey));
            Assert.Equal("merged", datHeader.GetStringFieldValue(Models.Metadata.Header.RomModeKey));
            Assert.Equal("romtitle", datHeader.GetStringFieldValue(Models.Metadata.Header.RomTitleKey));
            Assert.Equal("rootdir", datHeader.GetStringFieldValue(Models.Metadata.Header.RootDirKey));
            Assert.Equal("merged", datHeader.GetStringFieldValue(Models.Metadata.Header.SampleModeKey));
            Assert.Equal("schemalocation", datHeader.GetStringFieldValue(Models.Metadata.Header.SchemaLocationKey));
            Assert.Equal("screenshotsheight", datHeader.GetStringFieldValue(Models.Metadata.Header.ScreenshotsHeightKey));
            Assert.Equal("screenshotsWidth", datHeader.GetStringFieldValue(Models.Metadata.Header.ScreenshotsWidthKey));
            Assert.NotNull(datHeader.GetStringFieldValue(Models.Metadata.Header.SearchKey));
            Assert.Equal("system", datHeader.GetStringFieldValue(Models.Metadata.Header.SystemKey));
            Assert.Equal("timestamp", datHeader.GetStringFieldValue(Models.Metadata.Header.TimestampKey));
            Assert.Equal("type", datHeader.GetStringFieldValue(Models.Metadata.Header.TypeKey));
            Assert.Equal("url", datHeader.GetStringFieldValue(Models.Metadata.Header.UrlKey));
            Assert.Equal("version", datHeader.GetStringFieldValue(Models.Metadata.Header.VersionKey));
        }

        private static void ValidateMachine(DatItems.Machine machine)
        {
            Assert.Equal("board", machine.GetStringFieldValue(Models.Metadata.Machine.BoardKey));
            Assert.Equal("buttons", machine.GetStringFieldValue(Models.Metadata.Machine.ButtonsKey));
            Assert.Equal("category", machine.GetStringFieldValue(Models.Metadata.Machine.CategoryKey));
            Assert.Equal("cloneof", machine.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey));
            Assert.Equal("cloneofid", machine.GetStringFieldValue(Models.Metadata.Machine.CloneOfIdKey));
            Assert.Equal("comment", machine.GetStringFieldValue(Models.Metadata.Machine.CommentKey));
            Assert.Equal("company", machine.GetStringFieldValue(Models.Metadata.Machine.CompanyKey));
            Assert.Equal("control", machine.GetStringFieldValue(Models.Metadata.Machine.ControlKey));
            Assert.Equal("country", machine.GetStringFieldValue(Models.Metadata.Machine.CountryKey));
            Assert.Equal("description", machine.GetStringFieldValue(Models.Metadata.Machine.DescriptionKey));
            Assert.Equal("dirname", machine.GetStringFieldValue(Models.Metadata.Machine.DirNameKey));
            Assert.Equal("displaycount", machine.GetStringFieldValue(Models.Metadata.Machine.DisplayCountKey));
            Assert.Equal("displaytype", machine.GetStringFieldValue(Models.Metadata.Machine.DisplayTypeKey));
            Assert.Equal("duplicateid", machine.GetStringFieldValue(Models.Metadata.Machine.DuplicateIDKey));
            Assert.Equal("emulator", machine.GetStringFieldValue(Models.Metadata.Machine.EmulatorKey));
            Assert.Equal("extra", machine.GetStringFieldValue(Models.Metadata.Machine.ExtraKey));
            Assert.Equal("favorite", machine.GetStringFieldValue(Models.Metadata.Machine.FavoriteKey));
            Assert.Equal("genmsxid", machine.GetStringFieldValue(Models.Metadata.Machine.GenMSXIDKey));
            Assert.Equal("history", machine.GetStringFieldValue(Models.Metadata.Machine.HistoryKey));
            Assert.Equal("id", machine.GetStringFieldValue(Models.Metadata.Machine.IdKey));
            Assert.Equal(ZeroHash.CRC32Str, machine.GetStringFieldValue(Models.Metadata.Machine.Im1CRCKey));
            Assert.Equal(ZeroHash.CRC32Str, machine.GetStringFieldValue(Models.Metadata.Machine.Im2CRCKey));
            Assert.Equal("imagenumber", machine.GetStringFieldValue(Models.Metadata.Machine.ImageNumberKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Models.Metadata.Machine.IsBiosKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Models.Metadata.Machine.IsDeviceKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Models.Metadata.Machine.IsMechanicalKey));
            Assert.Equal("language", machine.GetStringFieldValue(Models.Metadata.Machine.LanguageKey));
            Assert.Equal("location", machine.GetStringFieldValue(Models.Metadata.Machine.LocationKey));
            Assert.Equal("manufacturer", machine.GetStringFieldValue(Models.Metadata.Machine.ManufacturerKey));
            Assert.Equal("name", machine.GetName());
            Assert.Equal("notes", machine.GetStringFieldValue(Models.Metadata.Machine.NotesKey));
            Assert.Equal("playedcount", machine.GetStringFieldValue(Models.Metadata.Machine.PlayedCountKey));
            Assert.Equal("playedtime", machine.GetStringFieldValue(Models.Metadata.Machine.PlayedTimeKey));
            Assert.Equal("players", machine.GetStringFieldValue(Models.Metadata.Machine.PlayersKey));
            Assert.Equal("publisher", machine.GetStringFieldValue(Models.Metadata.Machine.PublisherKey));
            Assert.Equal("rebuildto", machine.GetStringFieldValue(Models.Metadata.Machine.RebuildToKey));
            Assert.Equal("releasenumber", machine.GetStringFieldValue(Models.Metadata.Machine.ReleaseNumberKey));
            Assert.Equal("romof", machine.GetStringFieldValue(Models.Metadata.Machine.RomOfKey));
            Assert.Equal("rotation", machine.GetStringFieldValue(Models.Metadata.Machine.RotationKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Models.Metadata.Machine.RunnableKey));
            Assert.Equal("sampleof", machine.GetStringFieldValue(Models.Metadata.Machine.SampleOfKey));
            Assert.Equal("savetype", machine.GetStringFieldValue(Models.Metadata.Machine.SaveTypeKey));
            Assert.Equal("sourcefile", machine.GetStringFieldValue(Models.Metadata.Machine.SourceFileKey));
            Assert.Equal("sourcerom", machine.GetStringFieldValue(Models.Metadata.Machine.SourceRomKey));
            Assert.Equal("status", machine.GetStringFieldValue(Models.Metadata.Machine.StatusKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Models.Metadata.Machine.SupportedKey));
            Assert.Equal("system", machine.GetStringFieldValue(Models.Metadata.Machine.SystemKey));
            Assert.Equal("tags", machine.GetStringFieldValue(Models.Metadata.Machine.TagsKey));
            Assert.Equal("year", machine.GetStringFieldValue(Models.Metadata.Machine.YearKey));

            DatItems.Trurip? trurip = machine.GetFieldValue<DatItems.Trurip>(Models.Metadata.Machine.TruripKey);
            ValidateTrurip(trurip);
        }

        private static void ValidateAdjuster(Adjuster? adjuster)
        {
            Assert.NotNull(adjuster);
            Assert.True(adjuster.GetBoolFieldValue(Models.Metadata.Adjuster.DefaultKey));
            Assert.Equal("name", adjuster.GetStringFieldValue(Models.Metadata.Adjuster.NameKey));

            Condition? condition = adjuster.GetFieldValue<Condition>(Models.Metadata.Adjuster.ConditionKey);
            ValidateCondition(condition);
        }

        private static void ValidateAnalog(Analog? analog)
        {
            Assert.NotNull(analog);
            Assert.Equal("mask", analog.GetStringFieldValue(Models.Metadata.Analog.MaskKey));
        }

        private static void ValidateArchive(Archive? archive)
        {
            Assert.NotNull(archive);
            Assert.Equal("name", archive.GetStringFieldValue(Models.Metadata.Archive.NameKey));
        }

        private static void ValidateBiosSet(BiosSet? biosSet)
        {
            Assert.NotNull(biosSet);
            Assert.True(biosSet.GetBoolFieldValue(Models.Metadata.BiosSet.DefaultKey));
            Assert.Equal("description", biosSet.GetStringFieldValue(Models.Metadata.BiosSet.DescriptionKey));
            Assert.Equal("name", biosSet.GetStringFieldValue(Models.Metadata.BiosSet.NameKey));
        }

        private static void ValidateChip(Chip? chip)
        {
            Assert.NotNull(chip);
            Assert.Equal(12345L, chip.GetInt64FieldValue(Models.Metadata.Chip.ClockKey));
            Assert.Equal("flags", chip.GetStringFieldValue(Models.Metadata.Chip.FlagsKey));
            Assert.Equal("name", chip.GetStringFieldValue(Models.Metadata.Chip.NameKey));
            Assert.True(chip.GetBoolFieldValue(Models.Metadata.Chip.SoundOnlyKey));
            Assert.Equal("tag", chip.GetStringFieldValue(Models.Metadata.Chip.TagKey));
            Assert.Equal("cpu", chip.GetStringFieldValue(Models.Metadata.Chip.ChipTypeKey));
        }

        private static void ValidateCondition(Condition? condition)
        {
            Assert.NotNull(condition);
            Assert.Equal("value", condition.GetStringFieldValue(Models.Metadata.Condition.ValueKey));
            Assert.Equal("mask", condition.GetStringFieldValue(Models.Metadata.Condition.MaskKey));
            Assert.Equal("eq", condition.GetStringFieldValue(Models.Metadata.Condition.RelationKey));
            Assert.Equal("tag", condition.GetStringFieldValue(Models.Metadata.Condition.TagKey));
        }

        private static void ValidateConfiguration(Configuration? configuration)
        {
            Assert.NotNull(configuration);
            Assert.Equal("mask", configuration.GetStringFieldValue(Models.Metadata.Configuration.MaskKey));
            Assert.Equal("name", configuration.GetStringFieldValue(Models.Metadata.Configuration.NameKey));
            Assert.Equal("tag", configuration.GetStringFieldValue(Models.Metadata.Configuration.TagKey));

            Condition? condition = configuration.GetFieldValue<Condition>(Models.Metadata.Configuration.ConditionKey);
            ValidateCondition(condition);

            ConfLocation[]? confLocations = configuration.GetFieldValue<ConfLocation[]>(Models.Metadata.Configuration.ConfLocationKey);
            Assert.NotNull(confLocations);
            ConfLocation? confLocation = Assert.Single(confLocations);
            ValidateConfLocation(confLocation);

            ConfSetting[]? confSettings = configuration.GetFieldValue<ConfSetting[]>(Models.Metadata.Configuration.ConfSettingKey);
            Assert.NotNull(confSettings);
            ConfSetting? confSetting = Assert.Single(confSettings);
            ValidateConfSetting(confSetting);
        }

        private static void ValidateConfLocation(ConfLocation? confLocation)
        {
            Assert.NotNull(confLocation);
            Assert.True(confLocation.GetBoolFieldValue(Models.Metadata.ConfLocation.InvertedKey));
            Assert.Equal("name", confLocation.GetStringFieldValue(Models.Metadata.ConfLocation.NameKey));
            Assert.Equal("number", confLocation.GetStringFieldValue(Models.Metadata.ConfLocation.NumberKey));
        }

        private static void ValidateConfSetting(ConfSetting? confSetting)
        {
            Assert.NotNull(confSetting);
            Assert.True(confSetting.GetBoolFieldValue(Models.Metadata.ConfSetting.DefaultKey));
            Assert.Equal("name", confSetting.GetStringFieldValue(Models.Metadata.ConfSetting.NameKey));
            Assert.Equal("value", confSetting.GetStringFieldValue(Models.Metadata.ConfSetting.ValueKey));

            Condition? condition = confSetting.GetFieldValue<Condition>(Models.Metadata.ConfSetting.ConditionKey);
            ValidateCondition(condition);
        }

        private static void ValidateControl(Control? control)
        {
            Assert.NotNull(control);
            Assert.Equal(12345L, control.GetInt64FieldValue(Models.Metadata.Control.ButtonsKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Models.Metadata.Control.KeyDeltaKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Models.Metadata.Control.MaximumKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Models.Metadata.Control.MinimumKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Models.Metadata.Control.PlayerKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Models.Metadata.Control.ReqButtonsKey));
            Assert.True(control.GetBoolFieldValue(Models.Metadata.Control.ReverseKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Models.Metadata.Control.SensitivityKey));
            Assert.Equal("lightgun", control.GetStringFieldValue(Models.Metadata.Control.ControlTypeKey));
            Assert.Equal("ways", control.GetStringFieldValue(Models.Metadata.Control.WaysKey));
            Assert.Equal("ways2", control.GetStringFieldValue(Models.Metadata.Control.Ways2Key));
            Assert.Equal("ways3", control.GetStringFieldValue(Models.Metadata.Control.Ways3Key));
        }

        private static void ValidateDataArea(DataArea? dataArea)
        {
            Assert.NotNull(dataArea);
            Assert.Equal("big", dataArea.GetStringFieldValue(Models.Metadata.DataArea.EndiannessKey));
            Assert.Equal("name", dataArea.GetStringFieldValue(Models.Metadata.DataArea.NameKey));
            Assert.Equal(12345L, dataArea.GetInt64FieldValue(Models.Metadata.DataArea.SizeKey));
            Assert.Equal(64, dataArea.GetInt64FieldValue(Models.Metadata.DataArea.WidthKey));
        }

        private static void ValidateDevice(Device? device)
        {
            Assert.NotNull(device);
            Assert.Equal("fixedimage", device.GetStringFieldValue(Models.Metadata.Device.FixedImageKey));
            Assert.Equal("interface", device.GetStringFieldValue(Models.Metadata.Device.InterfaceKey));
            Assert.Equal(1, device.GetInt64FieldValue(Models.Metadata.Device.MandatoryKey));
            Assert.Equal("tag", device.GetStringFieldValue(Models.Metadata.Device.TagKey));
            Assert.Equal("punchtape", device.GetStringFieldValue(Models.Metadata.Device.DeviceTypeKey));

            Extension[]? extensions = device.GetFieldValue<Extension[]>(Models.Metadata.Device.ExtensionKey);
            Assert.NotNull(extensions);
            Extension? extension = Assert.Single(extensions);
            ValidateExtension(extension);

            Instance? instance = device.GetFieldValue<Instance>(Models.Metadata.Device.InstanceKey);
            ValidateInstance(instance);
        }

        private static void ValidateDeviceRef(DeviceRef? deviceRef)
        {
            Assert.NotNull(deviceRef);
            Assert.Equal("name", deviceRef.GetStringFieldValue(Models.Metadata.DeviceRef.NameKey));
        }

        private static void ValidateDipLocation(DipLocation? dipLocation)
        {
            Assert.NotNull(dipLocation);
            Assert.True(dipLocation.GetBoolFieldValue(Models.Metadata.DipLocation.InvertedKey));
            Assert.Equal("name", dipLocation.GetStringFieldValue(Models.Metadata.DipLocation.NameKey));
            Assert.Equal("number", dipLocation.GetStringFieldValue(Models.Metadata.DipLocation.NumberKey));
        }

        private static void ValidateDipSwitch(DipSwitch? dipSwitch)
        {
            Assert.NotNull(dipSwitch);
            Assert.True(dipSwitch.GetBoolFieldValue(Models.Metadata.DipSwitch.DefaultKey));
            Assert.Equal("mask", dipSwitch.GetStringFieldValue(Models.Metadata.DipSwitch.MaskKey));
            Assert.Equal("name", dipSwitch.GetStringFieldValue(Models.Metadata.DipSwitch.NameKey));
            Assert.Equal("tag", dipSwitch.GetStringFieldValue(Models.Metadata.DipSwitch.TagKey));

            Condition? condition = dipSwitch.GetFieldValue<Condition>(Models.Metadata.DipSwitch.ConditionKey);
            ValidateCondition(condition);

            DipLocation[]? dipLocations = dipSwitch.GetFieldValue<DipLocation[]>(Models.Metadata.DipSwitch.DipLocationKey);
            Assert.NotNull(dipLocations);
            DipLocation? dipLocation = Assert.Single(dipLocations);
            ValidateDipLocation(dipLocation);

            DipValue[]? dipValues = dipSwitch.GetFieldValue<DipValue[]>(Models.Metadata.DipSwitch.DipValueKey);
            Assert.NotNull(dipValues);
            DipValue? dipValue = Assert.Single(dipValues);
            ValidateDipValue(dipValue);

            string[]? entries = dipSwitch.GetStringArrayFieldValue(Models.Metadata.DipSwitch.EntryKey);
            Assert.NotNull(entries);
            string entry = Assert.Single(entries);
            Assert.Equal("entry", entry);
        }

        private static void ValidateDipValue(DipValue? dipValue)
        {
            Assert.NotNull(dipValue);
            Assert.True(dipValue.GetBoolFieldValue(Models.Metadata.DipValue.DefaultKey));
            Assert.Equal("name", dipValue.GetStringFieldValue(Models.Metadata.DipValue.NameKey));
            Assert.Equal("value", dipValue.GetStringFieldValue(Models.Metadata.DipValue.ValueKey));

            Condition? condition = dipValue.GetFieldValue<Condition>(Models.Metadata.DipValue.ConditionKey);
            ValidateCondition(condition);
        }

        private static void ValidateDisk(Disk? disk)
        {
            Assert.NotNull(disk);
            Assert.Equal("flags", disk.GetStringFieldValue(Models.Metadata.Disk.FlagsKey));
            Assert.Equal("index", disk.GetStringFieldValue(Models.Metadata.Disk.IndexKey));
            Assert.Equal(ZeroHash.MD5Str, disk.GetStringFieldValue(Models.Metadata.Disk.MD5Key));
            Assert.Equal("merge", disk.GetStringFieldValue(Models.Metadata.Disk.MergeKey));
            Assert.Equal("name", disk.GetStringFieldValue(Models.Metadata.Disk.NameKey));
            Assert.True(disk.GetBoolFieldValue(Models.Metadata.Disk.OptionalKey));
            Assert.Equal("region", disk.GetStringFieldValue(Models.Metadata.Disk.RegionKey));
            Assert.Equal(ZeroHash.SHA1Str, disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key));
            Assert.True(disk.GetBoolFieldValue(Models.Metadata.Disk.WritableKey));
        }

        private static void ValidateDiskArea(DiskArea? diskArea)
        {
            Assert.NotNull(diskArea);
            Assert.Equal("name", diskArea.GetStringFieldValue(Models.Metadata.DiskArea.NameKey));
        }

        private static void ValidateDisplay(Display? display)
        {
            Assert.NotNull(display);
            Assert.True(display.GetBoolFieldValue(Models.Metadata.Display.FlipXKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.HBEndKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.HBStartKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.HeightKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.HTotalKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.PixClockKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.RefreshKey));
            Assert.Equal(90, display.GetInt64FieldValue(Models.Metadata.Display.RotateKey));
            Assert.Equal("tag", display.GetStringFieldValue(Models.Metadata.Display.TagKey));
            Assert.Equal("vector", display.GetStringFieldValue(Models.Metadata.Display.DisplayTypeKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.VBEndKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.VBStartKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.VTotalKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.WidthKey));
        }

        private static void ValidateDriver(Driver? driver)
        {
            Assert.NotNull(driver);
            Assert.Equal("plain", driver.GetStringFieldValue(Models.Metadata.Driver.BlitKey));
            Assert.Equal("good", driver.GetStringFieldValue(Models.Metadata.Driver.CocktailKey));
            Assert.Equal("good", driver.GetStringFieldValue(Models.Metadata.Driver.ColorKey));
            Assert.Equal("good", driver.GetStringFieldValue(Models.Metadata.Driver.EmulationKey));
            Assert.True(driver.GetBoolFieldValue(Models.Metadata.Driver.IncompleteKey));
            Assert.True(driver.GetBoolFieldValue(Models.Metadata.Driver.NoSoundHardwareKey));
            Assert.Equal("pallettesize", driver.GetStringFieldValue(Models.Metadata.Driver.PaletteSizeKey));
            Assert.True(driver.GetBoolFieldValue(Models.Metadata.Driver.RequiresArtworkKey));
            Assert.Equal("supported", driver.GetStringFieldValue(Models.Metadata.Driver.SaveStateKey));
            Assert.Equal("good", driver.GetStringFieldValue(Models.Metadata.Driver.SoundKey));
            Assert.Equal("good", driver.GetStringFieldValue(Models.Metadata.Driver.StatusKey));
            Assert.True(driver.GetBoolFieldValue(Models.Metadata.Driver.UnofficialKey));
        }

        private static void ValidateExtension(Extension? extension)
        {
            Assert.NotNull(extension);
            Assert.Equal("name", extension.GetStringFieldValue(Models.Metadata.Extension.NameKey));
        }

        private static void ValidateFeature(Feature? feature)
        {
            Assert.NotNull(feature);
            Assert.Equal("name", feature.GetStringFieldValue(Models.Metadata.Feature.NameKey));
            Assert.Equal("imperfect", feature.GetStringFieldValue(Models.Metadata.Feature.OverallKey));
            Assert.Equal("imperfect", feature.GetStringFieldValue(Models.Metadata.Feature.StatusKey));
            Assert.Equal("protection", feature.GetStringFieldValue(Models.Metadata.Feature.FeatureTypeKey));
            Assert.Equal("value", feature.GetStringFieldValue(Models.Metadata.Feature.ValueKey));
        }

        private static void ValidateInfo(Info? info)
        {
            Assert.NotNull(info);
            Assert.Equal("name", info.GetStringFieldValue(Models.Metadata.Info.NameKey));
            Assert.Equal("value", info.GetStringFieldValue(Models.Metadata.Info.ValueKey));
        }

        private static void ValidateInput(Input? input)
        {
            Assert.NotNull(input);
            Assert.Equal(12345L, input.GetInt64FieldValue(Models.Metadata.Input.ButtonsKey));
            Assert.Equal(12345L, input.GetInt64FieldValue(Models.Metadata.Input.CoinsKey));
            Assert.Equal(12345L, input.GetInt64FieldValue(Models.Metadata.Input.PlayersKey));
            Assert.True(input.GetBoolFieldValue(Models.Metadata.Input.ServiceKey));
            Assert.True(input.GetBoolFieldValue(Models.Metadata.Input.TiltKey));

            Control[]? controls = input.GetFieldValue<Control[]>(Models.Metadata.Input.ControlKey);
            Assert.NotNull(controls);
            Control? control = Assert.Single(controls);
            ValidateControl(control);
        }

        private static void ValidateInstance(Instance? instance)
        {
            Assert.NotNull(instance);
            Assert.Equal("briefname", instance.GetStringFieldValue(Models.Metadata.Instance.BriefNameKey));
            Assert.Equal("name", instance.GetStringFieldValue(Models.Metadata.Instance.NameKey));
        }

        private static void ValidateMedia(Media? media)
        {
            Assert.NotNull(media);
            Assert.Equal(ZeroHash.MD5Str, media.GetStringFieldValue(Models.Metadata.Media.MD5Key));
            Assert.Equal("name", media.GetStringFieldValue(Models.Metadata.Media.NameKey));
            Assert.Equal(ZeroHash.SHA1Str, media.GetStringFieldValue(Models.Metadata.Media.SHA1Key));
            Assert.Equal(ZeroHash.SHA256Str, media.GetStringFieldValue(Models.Metadata.Media.SHA256Key));
            Assert.Equal(ZeroHash.SpamSumStr, media.GetStringFieldValue(Models.Metadata.Media.SpamSumKey));
        }

        private static void ValidatePart(Part? part)
        {
            Assert.NotNull(part);
            Assert.Equal("interface", part.GetStringFieldValue(Models.Metadata.Part.InterfaceKey));
            Assert.Equal("name", part.GetStringFieldValue(Models.Metadata.Part.NameKey));
        }

        private static void ValidatePartFeature(PartFeature? partFeature)
        {
            Assert.NotNull(partFeature);
            Assert.Equal("name", partFeature.GetStringFieldValue(Models.Metadata.Feature.NameKey));
            Assert.Equal("imperfect", partFeature.GetStringFieldValue(Models.Metadata.Feature.OverallKey));
            Assert.Equal("imperfect", partFeature.GetStringFieldValue(Models.Metadata.Feature.StatusKey));
            Assert.Equal("protection", partFeature.GetStringFieldValue(Models.Metadata.Feature.FeatureTypeKey));
            Assert.Equal("value", partFeature.GetStringFieldValue(Models.Metadata.Feature.ValueKey));

            Part? part = partFeature.GetFieldValue<Part>(PartFeature.PartKey);
            ValidatePart(part);
        }

        private static void ValidatePort(Port? port)
        {
            Assert.NotNull(port);
            Assert.Equal("tag", port.GetStringFieldValue(Models.Metadata.Port.TagKey));

            Analog[]? dipValues = port.GetFieldValue<Analog[]>(Models.Metadata.Port.AnalogKey);
            Assert.NotNull(dipValues);
            Analog? dipValue = Assert.Single(dipValues);
            ValidateAnalog(dipValue);
        }

        private static void ValidateRamOption(RamOption? ramOption)
        {
            Assert.NotNull(ramOption);
            Assert.Equal("content", ramOption.GetStringFieldValue(Models.Metadata.RamOption.ContentKey));
            Assert.True(ramOption.GetBoolFieldValue(Models.Metadata.RamOption.DefaultKey));
            Assert.Equal("name", ramOption.GetStringFieldValue(Models.Metadata.RamOption.NameKey));
        }

        private static void ValidateRelease(Release? release)
        {
            Assert.NotNull(release);
            Assert.Equal("date", release.GetStringFieldValue(Models.Metadata.Release.DateKey));
            Assert.True(release.GetBoolFieldValue(Models.Metadata.Release.DefaultKey));
            Assert.Equal("language", release.GetStringFieldValue(Models.Metadata.Release.LanguageKey));
            Assert.Equal("name", release.GetStringFieldValue(Models.Metadata.Release.NameKey));
            Assert.Equal("region", release.GetStringFieldValue(Models.Metadata.Release.RegionKey));
        }

        private static void ValidateRom(Rom? rom)
        {
            Assert.NotNull(rom);
            Assert.Equal("album", rom.GetStringFieldValue(Models.Metadata.Rom.AlbumKey));
            Assert.Equal("alt_romname", rom.GetStringFieldValue(Models.Metadata.Rom.AltRomnameKey));
            Assert.Equal("alt_title", rom.GetStringFieldValue(Models.Metadata.Rom.AltTitleKey));
            Assert.Equal("artist", rom.GetStringFieldValue(Models.Metadata.Rom.ArtistKey));
            Assert.Equal("asr_detected_lang", rom.GetStringFieldValue(Models.Metadata.Rom.ASRDetectedLangKey));
            Assert.Equal("asr_detected_lang_conf", rom.GetStringFieldValue(Models.Metadata.Rom.ASRDetectedLangConfKey));
            Assert.Equal("asr_transcribed_lang", rom.GetStringFieldValue(Models.Metadata.Rom.ASRTranscribedLangKey));
            Assert.Equal("bios", rom.GetStringFieldValue(Models.Metadata.Rom.BiosKey));
            Assert.Equal("bitrate", rom.GetStringFieldValue(Models.Metadata.Rom.BitrateKey));
            Assert.Equal("btih", rom.GetStringFieldValue(Models.Metadata.Rom.BitTorrentMagnetHashKey));
            Assert.Equal("cloth_cover_detection_module_version", rom.GetStringFieldValue(Models.Metadata.Rom.ClothCoverDetectionModuleVersionKey));
            Assert.Equal("collection-catalog-number", rom.GetStringFieldValue(Models.Metadata.Rom.CollectionCatalogNumberKey));
            Assert.Equal("comment", rom.GetStringFieldValue(Models.Metadata.Rom.CommentKey));
            Assert.Equal(ZeroHash.CRC32Str, rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
            Assert.Equal("creator", rom.GetStringFieldValue(Models.Metadata.Rom.CreatorKey));
            Assert.Equal("date", rom.GetStringFieldValue(Models.Metadata.Rom.DateKey));
            Assert.True(rom.GetBoolFieldValue(Models.Metadata.Rom.DisposeKey));
            Assert.Equal("extension", rom.GetStringFieldValue(Models.Metadata.Rom.ExtensionKey));
            Assert.Equal(12345L, rom.GetInt64FieldValue(Models.Metadata.Rom.FileCountKey));
            Assert.True(rom.GetBoolFieldValue(Models.Metadata.Rom.FileIsAvailableKey));
            Assert.Equal("flags", rom.GetStringFieldValue(Models.Metadata.Rom.FlagsKey));
            Assert.Equal("format", rom.GetStringFieldValue(Models.Metadata.Rom.FormatKey));
            Assert.Equal("header", rom.GetStringFieldValue(Models.Metadata.Rom.HeaderKey));
            Assert.Equal("height", rom.GetStringFieldValue(Models.Metadata.Rom.HeightKey));
            Assert.Equal("hocr_char_to_word_hocr_version", rom.GetStringFieldValue(Models.Metadata.Rom.hOCRCharToWordhOCRVersionKey));
            Assert.Equal("hocr_char_to_word_module_version", rom.GetStringFieldValue(Models.Metadata.Rom.hOCRCharToWordModuleVersionKey));
            Assert.Equal("hocr_fts_text_hocr_version", rom.GetStringFieldValue(Models.Metadata.Rom.hOCRFtsTexthOCRVersionKey));
            Assert.Equal("hocr_fts_text_module_version", rom.GetStringFieldValue(Models.Metadata.Rom.hOCRFtsTextModuleVersionKey));
            Assert.Equal("hocr_pageindex_hocr_version", rom.GetStringFieldValue(Models.Metadata.Rom.hOCRPageIndexhOCRVersionKey));
            Assert.Equal("hocr_pageindex_module_version", rom.GetStringFieldValue(Models.Metadata.Rom.hOCRPageIndexModuleVersionKey));
            Assert.True(rom.GetBoolFieldValue(Models.Metadata.Rom.InvertedKey));
            Assert.Equal("mtime", rom.GetStringFieldValue(Models.Metadata.Rom.LastModifiedTimeKey));
            Assert.Equal("length", rom.GetStringFieldValue(Models.Metadata.Rom.LengthKey));
            Assert.Equal("load16_byte", rom.GetStringFieldValue(Models.Metadata.Rom.LoadFlagKey));
            Assert.Equal("matrix_number", rom.GetStringFieldValue(Models.Metadata.Rom.MatrixNumberKey));
            Assert.Equal(ZeroHash.GetString(HashType.MD2), rom.GetStringFieldValue(Models.Metadata.Rom.MD2Key));
            Assert.Equal(ZeroHash.GetString(HashType.MD4), rom.GetStringFieldValue(Models.Metadata.Rom.MD4Key));
            Assert.Equal(ZeroHash.MD5Str, rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key));
            Assert.Null(rom.GetStringFieldValue(Models.Metadata.Rom.OpenMSXMediaType)); // Omit due to other test
            Assert.Equal("merge", rom.GetStringFieldValue(Models.Metadata.Rom.MergeKey));
            Assert.True(rom.GetBoolFieldValue(Models.Metadata.Rom.MIAKey));
            Assert.Equal("name", rom.GetStringFieldValue(Models.Metadata.Rom.NameKey));
            Assert.Equal("ocr", rom.GetStringFieldValue(Models.Metadata.Rom.TesseractOCRKey));
            Assert.Equal("ocr_converted", rom.GetStringFieldValue(Models.Metadata.Rom.TesseractOCRConvertedKey));
            Assert.Equal("ocr_detected_lang", rom.GetStringFieldValue(Models.Metadata.Rom.TesseractOCRDetectedLangKey));
            Assert.Equal("ocr_detected_lang_conf", rom.GetStringFieldValue(Models.Metadata.Rom.TesseractOCRDetectedLangConfKey));
            Assert.Equal("ocr_detected_script", rom.GetStringFieldValue(Models.Metadata.Rom.TesseractOCRDetectedScriptKey));
            Assert.Equal("ocr_detected_script_conf", rom.GetStringFieldValue(Models.Metadata.Rom.TesseractOCRDetectedScriptConfKey));
            Assert.Equal("ocr_module_version", rom.GetStringFieldValue(Models.Metadata.Rom.TesseractOCRModuleVersionKey));
            Assert.Equal("ocr_parameters", rom.GetStringFieldValue(Models.Metadata.Rom.TesseractOCRParametersKey));
            Assert.Equal("offset", rom.GetStringFieldValue(Models.Metadata.Rom.OffsetKey));
            Assert.True(rom.GetBoolFieldValue(Models.Metadata.Rom.OptionalKey));
            Assert.Equal("original", rom.GetStringFieldValue(Models.Metadata.Rom.OriginalKey));
            Assert.Equal("pdf_module_version", rom.GetStringFieldValue(Models.Metadata.Rom.PDFModuleVersionKey));
            Assert.Equal("preview-image", rom.GetStringFieldValue(Models.Metadata.Rom.PreviewImageKey));
            Assert.Equal("publisher", rom.GetStringFieldValue(Models.Metadata.Rom.PublisherKey));
            Assert.Equal("region", rom.GetStringFieldValue(Models.Metadata.Rom.RegionKey));
            Assert.Equal("remark", rom.GetStringFieldValue(Models.Metadata.Rom.RemarkKey));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD128), rom.GetStringFieldValue("RIPEMD128"));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD160), rom.GetStringFieldValue("RIPEMD160"));
            Assert.Equal("rotation", rom.GetStringFieldValue(Models.Metadata.Rom.RotationKey));
            Assert.Equal("serial", rom.GetStringFieldValue(Models.Metadata.Rom.SerialKey));
            Assert.Equal(ZeroHash.SHA1Str, rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key));
            Assert.Equal(ZeroHash.SHA256Str, rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key));
            Assert.Equal(ZeroHash.SHA384Str, rom.GetStringFieldValue(Models.Metadata.Rom.SHA384Key));
            Assert.Equal(ZeroHash.SHA512Str, rom.GetStringFieldValue(Models.Metadata.Rom.SHA512Key));
            Assert.Equal(12345L, rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey));
            Assert.True(rom.GetBoolFieldValue(Models.Metadata.Rom.SoundOnlyKey));
            Assert.Equal("source", rom.GetStringFieldValue(Models.Metadata.Rom.SourceKey));
            Assert.Equal(ZeroHash.SpamSumStr, rom.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey));
            Assert.Equal("start", rom.GetStringFieldValue(Models.Metadata.Rom.StartKey));
            Assert.Equal("good", rom.GetStringFieldValue(Models.Metadata.Rom.StatusKey));
            Assert.Equal("summation", rom.GetStringFieldValue(Models.Metadata.Rom.SummationKey));
            Assert.Equal("title", rom.GetStringFieldValue(Models.Metadata.Rom.TitleKey));
            Assert.Equal("track", rom.GetStringFieldValue(Models.Metadata.Rom.TrackKey));
            Assert.Equal("type", rom.GetStringFieldValue(Models.Metadata.Rom.OpenMSXType));
            Assert.Equal("value", rom.GetStringFieldValue(Models.Metadata.Rom.ValueKey));
            Assert.Equal("whisper_asr_module_version", rom.GetStringFieldValue(Models.Metadata.Rom.WhisperASRModuleVersionKey));
            Assert.Equal("whisper_model_hash", rom.GetStringFieldValue(Models.Metadata.Rom.WhisperModelHashKey));
            Assert.Equal("whisper_model_name", rom.GetStringFieldValue(Models.Metadata.Rom.WhisperModelNameKey));
            Assert.Equal("whisper_version", rom.GetStringFieldValue(Models.Metadata.Rom.WhisperVersionKey));
            Assert.Equal("width", rom.GetStringFieldValue(Models.Metadata.Rom.WidthKey));
            Assert.Equal("word_conf_0_10", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval0To10Key));
            Assert.Equal("word_conf_11_20", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval11To20Key));
            Assert.Equal("word_conf_21_30", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval21To30Key));
            Assert.Equal("word_conf_31_40", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval31To40Key));
            Assert.Equal("word_conf_41_50", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval41To50Key));
            Assert.Equal("word_conf_51_60", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval51To60Key));
            Assert.Equal("word_conf_61_70", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval61To70Key));
            Assert.Equal("word_conf_71_80", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval71To80Key));
            Assert.Equal("word_conf_81_90", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval81To90Key));
            Assert.Equal("word_conf_91_100", rom.GetStringFieldValue(Models.Metadata.Rom.WordConfidenceInterval91To100Key));
            Assert.Equal(ZeroHash.GetString(HashType.XxHash3), rom.GetStringFieldValue(Models.Metadata.Rom.xxHash364Key));
            Assert.Equal(ZeroHash.GetString(HashType.XxHash128), rom.GetStringFieldValue(Models.Metadata.Rom.xxHash3128Key));
        }

        private static void ValidateSample(Sample? sample)
        {
            Assert.NotNull(sample);
            Assert.Equal("name", sample.GetStringFieldValue(Models.Metadata.Sample.NameKey));
        }

        private static void ValidateSharedFeat(SharedFeat? sharedFeat)
        {
            Assert.NotNull(sharedFeat);
            Assert.Equal("name", sharedFeat.GetStringFieldValue(Models.Metadata.SharedFeat.NameKey));
            Assert.Equal("value", sharedFeat.GetStringFieldValue(Models.Metadata.SharedFeat.ValueKey));
        }

        private static void ValidateSlot(Slot? slot)
        {
            Assert.NotNull(slot);
            Assert.Equal("name", slot.GetStringFieldValue(Models.Metadata.Slot.NameKey));

            SlotOption[]? slotOptions = slot.GetFieldValue<SlotOption[]>(Models.Metadata.Slot.SlotOptionKey);
            Assert.NotNull(slotOptions);
            SlotOption? slotOption = Assert.Single(slotOptions);
            ValidateSlotOption(slotOption);
        }

        private static void ValidateSlotOption(SlotOption? slotOption)
        {
            Assert.NotNull(slotOption);
            Assert.True(slotOption.GetBoolFieldValue(Models.Metadata.SlotOption.DefaultKey));
            Assert.Equal("devname", slotOption.GetStringFieldValue(Models.Metadata.SlotOption.DevNameKey));
            Assert.Equal("name", slotOption.GetStringFieldValue(Models.Metadata.SlotOption.NameKey));
        }

        private static void ValidateSoftwareList(SoftwareList? softwareList)
        {
            Assert.NotNull(softwareList);
            Assert.Equal("description", softwareList.GetStringFieldValue(Models.Metadata.SoftwareList.DescriptionKey));
            Assert.Equal("filter", softwareList.GetStringFieldValue(Models.Metadata.SoftwareList.FilterKey));
            Assert.Equal("name", softwareList.GetStringFieldValue(Models.Metadata.SoftwareList.NameKey));
            Assert.Equal("notes", softwareList.GetStringFieldValue(Models.Metadata.SoftwareList.NotesKey));
            // TODO: Figure out why Models.Metadata.SoftwareList.SoftwareKey doesn't get processed
            Assert.Equal("original", softwareList.GetStringFieldValue(Models.Metadata.SoftwareList.StatusKey));
            Assert.Equal("tag", softwareList.GetStringFieldValue(Models.Metadata.SoftwareList.TagKey));
        }

        private static void ValidateSound(Sound? sound)
        {
            Assert.NotNull(sound);
            Assert.Equal(12345L, sound.GetInt64FieldValue(Models.Metadata.Sound.ChannelsKey));
        }

        // TODO: Figure out why so many fields are omitted
        private static void ValidateTrurip(DatItems.Trurip? trurip)
        {
            Assert.NotNull(trurip);
            Assert.Equal("titleid", trurip.TitleID);
            //Assert.Equal("publisher", trurip.Publisher); // Omitted from conversion
            Assert.Equal("developer", trurip.Developer);
            // Assert.Equal("year", trurip.Year); // Omitted from conversion
            Assert.Equal("genre", trurip.Genre);
            Assert.Equal("subgenre", trurip.Subgenre);
            Assert.Equal("ratings", trurip.Ratings);
            Assert.Equal("score", trurip.Score);
            // Assert.Equal("players", trurip.Players); // Omitted from conversion
            Assert.Equal("enabled", trurip.Enabled);
            Assert.True(trurip.Crc);
            // Assert.Equal("source", trurip.Source); // Omitted from conversion
            // Assert.Equal("cloneof", trurip.CloneOf); // Omitted from conversion
            Assert.Equal("relatedto", trurip.RelatedTo);
        }

        private static void ValidateVideo(Display? display)
        {
            Assert.NotNull(display);
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Video.AspectXKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Video.AspectYKey));
            Assert.Equal("vector", display.GetStringFieldValue(Models.Metadata.Display.DisplayTypeKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.HeightKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.RefreshKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Models.Metadata.Display.WidthKey));
            Assert.Equal(90, display.GetInt64FieldValue(Models.Metadata.Display.RotateKey));
        }

        #endregion
    }
}