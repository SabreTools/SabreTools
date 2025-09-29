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
            Data.Models.Metadata.MetadataFile? item = null;

            DatFile datFile = new Formats.Logiqx(null, useGame: false);
            datFile.ConvertFromMetadata(item, "filename", indexId: 0, keep: true, statsOnly: false, filterRunner: null);

            Assert.Equal(0, datFile.Items.DatStatistics.TotalCount);
            Assert.Equal(0, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Fact]
        public void ConvertFromMetadata_Empty()
        {
            Data.Models.Metadata.MetadataFile? item = new Data.Models.Metadata.MetadataFile();

            DatFile datFile = new Formats.Logiqx(null, useGame: false);
            datFile.ConvertFromMetadata(item, "filename", indexId: 0, keep: true, statsOnly: false, filterRunner: null);

            Assert.Equal(0, datFile.Items.DatStatistics.TotalCount);
            Assert.Equal(0, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Fact]
        public void ConvertFromMetadata_FilledHeader()
        {
            Data.Models.Metadata.Header? header = CreateMetadataHeader();
            Data.Models.Metadata.Machine[]? machines = null;
            Data.Models.Metadata.MetadataFile? item = new Data.Models.Metadata.MetadataFile
            {
                [Data.Models.Metadata.MetadataFile.HeaderKey] = header,
                [Data.Models.Metadata.MetadataFile.MachineKey] = machines,
            };

            DatFile datFile = new Formats.Logiqx(null, useGame: false);
            datFile.ConvertFromMetadata(item, "filename", indexId: 0, keep: true, statsOnly: false, filterRunner: null);

            ValidateHeader(datFile.Header);
        }

        [Fact]
        public void ConvertFromMetadata_FilledMachine()
        {
            Data.Models.Metadata.Header? header = null;
            Data.Models.Metadata.Machine machine = CreateMetadataMachine();
            Data.Models.Metadata.Machine[]? machines = [machine];
            Data.Models.Metadata.MetadataFile? item = new Data.Models.Metadata.MetadataFile
            {
                [Data.Models.Metadata.MetadataFile.HeaderKey] = header,
                [Data.Models.Metadata.MetadataFile.MachineKey] = machines,
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

            Display? display = Array.Find(datItems, item => item is Display display && display.GetInt64FieldValue(Data.Models.Metadata.Video.AspectXKey) == null) as Display;
            ValidateDisplay(display);

            Driver? driver = Array.Find(datItems, item => item is Driver) as Driver;
            ValidateDriver(driver);

            // All other fields are tested separately
            Rom? dump = Array.Find(datItems, item => item is Rom rom && rom.GetStringFieldValue(Data.Models.Metadata.Rom.OpenMSXMediaType) != null) as Rom;
            Assert.NotNull(dump);
            Assert.Equal("rom", dump.GetStringFieldValue(Data.Models.Metadata.Rom.OpenMSXMediaType));

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

            Rom? rom = Array.Find(datItems, item => item is Rom rom && !rom.DataAreaSpecified && !rom.PartSpecified && rom.GetStringFieldValue(Data.Models.Metadata.Rom.OpenMSXMediaType) == null) as Rom;
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

            Display? video = Array.Find(datItems, item => item is Display display && display.GetInt64FieldValue(Data.Models.Metadata.Video.AspectXKey) != null) as Display;
            ValidateVideo(video);
        }

        #endregion

        #region Creation Helpers

        private static Data.Models.Metadata.Header CreateMetadataHeader()
        {
            Data.Models.OfflineList.CanOpen canOpen = new Data.Models.OfflineList.CanOpen
            {
                Extension = ["ext"],
            };

            Data.Models.OfflineList.Images images = new Data.Models.OfflineList.Images();

            Data.Models.OfflineList.Infos infos = new Data.Models.OfflineList.Infos();

            Data.Models.OfflineList.NewDat newDat = new Data.Models.OfflineList.NewDat();

            Data.Models.OfflineList.Search search = new Data.Models.OfflineList.Search();

            return new Data.Models.Metadata.Header
            {
                [Data.Models.Metadata.Header.AuthorKey] = "author",
                [Data.Models.Metadata.Header.BiosModeKey] = "merged",
                [Data.Models.Metadata.Header.BuildKey] = "build",
                [Data.Models.Metadata.Header.CanOpenKey] = canOpen,
                [Data.Models.Metadata.Header.CategoryKey] = "category",
                [Data.Models.Metadata.Header.CommentKey] = "comment",
                [Data.Models.Metadata.Header.DateKey] = "date",
                [Data.Models.Metadata.Header.DatVersionKey] = "datversion",
                [Data.Models.Metadata.Header.DebugKey] = "yes",
                [Data.Models.Metadata.Header.DescriptionKey] = "description",
                [Data.Models.Metadata.Header.EmailKey] = "email",
                [Data.Models.Metadata.Header.EmulatorVersionKey] = "emulatorversion",
                [Data.Models.Metadata.Header.ForceMergingKey] = "merged",
                [Data.Models.Metadata.Header.ForceNodumpKey] = "required",
                [Data.Models.Metadata.Header.ForcePackingKey] = "zip",
                [Data.Models.Metadata.Header.ForceZippingKey] = "yes",
                [Data.Models.Metadata.Header.HeaderKey] = "header",
                [Data.Models.Metadata.Header.HomepageKey] = "homepage",
                [Data.Models.Metadata.Header.IdKey] = "id",
                [Data.Models.Metadata.Header.ImagesKey] = images,
                [Data.Models.Metadata.Header.ImFolderKey] = "imfolder",
                [Data.Models.Metadata.Header.InfosKey] = infos,
                [Data.Models.Metadata.Header.LockBiosModeKey] = "yes",
                [Data.Models.Metadata.Header.LockRomModeKey] = "yes",
                [Data.Models.Metadata.Header.LockSampleModeKey] = "yes",
                [Data.Models.Metadata.Header.MameConfigKey] = "mameconfig",
                [Data.Models.Metadata.Header.NameKey] = "name",
                [Data.Models.Metadata.Header.NewDatKey] = newDat,
                [Data.Models.Metadata.Header.NotesKey] = "notes",
                [Data.Models.Metadata.Header.PluginKey] = "plugin",
                [Data.Models.Metadata.Header.RefNameKey] = "refname",
                [Data.Models.Metadata.Header.RomModeKey] = "merged",
                [Data.Models.Metadata.Header.RomTitleKey] = "romtitle",
                [Data.Models.Metadata.Header.RootDirKey] = "rootdir",
                [Data.Models.Metadata.Header.SampleModeKey] = "merged",
                [Data.Models.Metadata.Header.SchemaLocationKey] = "schemalocation",
                [Data.Models.Metadata.Header.ScreenshotsHeightKey] = "screenshotsheight",
                [Data.Models.Metadata.Header.ScreenshotsWidthKey] = "screenshotsWidth",
                [Data.Models.Metadata.Header.SearchKey] = search,
                [Data.Models.Metadata.Header.SystemKey] = "system",
                [Data.Models.Metadata.Header.TimestampKey] = "timestamp",
                [Data.Models.Metadata.Header.TypeKey] = "type",
                [Data.Models.Metadata.Header.UrlKey] = "url",
                [Data.Models.Metadata.Header.VersionKey] = "version",
            };
        }

        private static Data.Models.Metadata.Machine CreateMetadataMachine()
        {
            return new Data.Models.Metadata.Machine
            {
                [Data.Models.Metadata.Machine.AdjusterKey] = new Data.Models.Metadata.Adjuster[] { CreateMetadataAdjuster() },
                [Data.Models.Metadata.Machine.ArchiveKey] = new Data.Models.Metadata.Archive[] { CreateMetadataArchive() },
                [Data.Models.Metadata.Machine.BiosSetKey] = new Data.Models.Metadata.BiosSet[] { CreateMetadataBiosSet() },
                [Data.Models.Metadata.Machine.BoardKey] = "board",
                [Data.Models.Metadata.Machine.ButtonsKey] = "buttons",
                [Data.Models.Metadata.Machine.CategoryKey] = "category",
                [Data.Models.Metadata.Machine.ChipKey] = new Data.Models.Metadata.Chip[] { CreateMetadataChip() },
                [Data.Models.Metadata.Machine.CloneOfKey] = "cloneof",
                [Data.Models.Metadata.Machine.CloneOfIdKey] = "cloneofid",
                [Data.Models.Metadata.Machine.CommentKey] = "comment",
                [Data.Models.Metadata.Machine.CompanyKey] = "company",
                [Data.Models.Metadata.Machine.ConfigurationKey] = new Data.Models.Metadata.Configuration[] { CreateMetadataConfiguration() },
                [Data.Models.Metadata.Machine.ControlKey] = "control",
                [Data.Models.Metadata.Machine.CountryKey] = "country",
                [Data.Models.Metadata.Machine.DescriptionKey] = "description",
                [Data.Models.Metadata.Machine.DeviceKey] = new Data.Models.Metadata.Device[] { CreateMetadataDevice() },
                [Data.Models.Metadata.Machine.DeviceRefKey] = new Data.Models.Metadata.DeviceRef[] { CreateMetadataDeviceRef() },
                [Data.Models.Metadata.Machine.DipSwitchKey] = new Data.Models.Metadata.DipSwitch[] { CreateMetadataDipSwitch() },
                [Data.Models.Metadata.Machine.DirNameKey] = "dirname",
                [Data.Models.Metadata.Machine.DiskKey] = new Data.Models.Metadata.Disk[] { CreateMetadataDisk() },
                [Data.Models.Metadata.Machine.DisplayCountKey] = "displaycount",
                [Data.Models.Metadata.Machine.DisplayKey] = new Data.Models.Metadata.Display[] { CreateMetadataDisplay() },
                [Data.Models.Metadata.Machine.DisplayTypeKey] = "displaytype",
                [Data.Models.Metadata.Machine.DriverKey] = CreateMetadataDriver(),
                [Data.Models.Metadata.Machine.DumpKey] = new Data.Models.Metadata.Dump[] { CreateMetadataDump() },
                [Data.Models.Metadata.Machine.DuplicateIDKey] = "duplicateid",
                [Data.Models.Metadata.Machine.EmulatorKey] = "emulator",
                [Data.Models.Metadata.Machine.ExtraKey] = "extra",
                [Data.Models.Metadata.Machine.FavoriteKey] = "favorite",
                [Data.Models.Metadata.Machine.FeatureKey] = new Data.Models.Metadata.Feature[] { CreateMetadataFeature() },
                [Data.Models.Metadata.Machine.GenMSXIDKey] = "genmsxid",
                [Data.Models.Metadata.Machine.HistoryKey] = "history",
                [Data.Models.Metadata.Machine.IdKey] = "id",
                [Data.Models.Metadata.Machine.Im1CRCKey] = ZeroHash.CRC32Str,
                [Data.Models.Metadata.Machine.Im2CRCKey] = ZeroHash.CRC32Str,
                [Data.Models.Metadata.Machine.ImageNumberKey] = "imagenumber",
                [Data.Models.Metadata.Machine.InfoKey] = new Data.Models.Metadata.Info[] { CreateMetadataInfo() },
                [Data.Models.Metadata.Machine.InputKey] = CreateMetadataInput(),
                [Data.Models.Metadata.Machine.IsBiosKey] = "yes",
                [Data.Models.Metadata.Machine.IsDeviceKey] = "yes",
                [Data.Models.Metadata.Machine.IsMechanicalKey] = "yes",
                [Data.Models.Metadata.Machine.LanguageKey] = "language",
                [Data.Models.Metadata.Machine.LocationKey] = "location",
                [Data.Models.Metadata.Machine.ManufacturerKey] = "manufacturer",
                [Data.Models.Metadata.Machine.MediaKey] = new Data.Models.Metadata.Media[] { CreateMetadataMedia() },
                [Data.Models.Metadata.Machine.NameKey] = "name",
                [Data.Models.Metadata.Machine.NotesKey] = "notes",
                [Data.Models.Metadata.Machine.PartKey] = new Data.Models.Metadata.Part[] { CreateMetadataPart() },
                [Data.Models.Metadata.Machine.PlayedCountKey] = "playedcount",
                [Data.Models.Metadata.Machine.PlayedTimeKey] = "playedtime",
                [Data.Models.Metadata.Machine.PlayersKey] = "players",
                [Data.Models.Metadata.Machine.PortKey] = new Data.Models.Metadata.Port[] { CreateMetadataPort() },
                [Data.Models.Metadata.Machine.PublisherKey] = "publisher",
                [Data.Models.Metadata.Machine.RamOptionKey] = new Data.Models.Metadata.RamOption[] { CreateMetadataRamOption() },
                [Data.Models.Metadata.Machine.RebuildToKey] = "rebuildto",
                [Data.Models.Metadata.Machine.ReleaseKey] = new Data.Models.Metadata.Release[] { CreateMetadataRelease() },
                [Data.Models.Metadata.Machine.ReleaseNumberKey] = "releasenumber",
                [Data.Models.Metadata.Machine.RomKey] = new Data.Models.Metadata.Rom[] { CreateMetadataRom() },
                [Data.Models.Metadata.Machine.RomOfKey] = "romof",
                [Data.Models.Metadata.Machine.RotationKey] = "rotation",
                [Data.Models.Metadata.Machine.RunnableKey] = "yes",
                [Data.Models.Metadata.Machine.SampleKey] = new Data.Models.Metadata.Sample[] { CreateMetadataSample() },
                [Data.Models.Metadata.Machine.SampleOfKey] = "sampleof",
                [Data.Models.Metadata.Machine.SaveTypeKey] = "savetype",
                [Data.Models.Metadata.Machine.SharedFeatKey] = new Data.Models.Metadata.SharedFeat[] { CreateMetadataSharedFeat() },
                [Data.Models.Metadata.Machine.SlotKey] = new Data.Models.Metadata.Slot[] { CreateMetadataSlot() },
                [Data.Models.Metadata.Machine.SoftwareListKey] = new Data.Models.Metadata.SoftwareList[] { CreateMetadataSoftwareList() },
                [Data.Models.Metadata.Machine.SoundKey] = CreateMetadataSound(),
                [Data.Models.Metadata.Machine.SourceFileKey] = "sourcefile",
                [Data.Models.Metadata.Machine.SourceRomKey] = "sourcerom",
                [Data.Models.Metadata.Machine.StatusKey] = "status",
                [Data.Models.Metadata.Machine.SupportedKey] = "yes",
                [Data.Models.Metadata.Machine.SystemKey] = "system",
                [Data.Models.Metadata.Machine.TagsKey] = "tags",
                [Data.Models.Metadata.Machine.TruripKey] = CreateMetadataTrurip(),
                [Data.Models.Metadata.Machine.VideoKey] = new Data.Models.Metadata.Video[] { CreateMetadataVideo() },
                [Data.Models.Metadata.Machine.YearKey] = "year",
            };
        }

        private static Data.Models.Metadata.Adjuster CreateMetadataAdjuster()
        {
            return new Data.Models.Metadata.Adjuster
            {
                [Data.Models.Metadata.Adjuster.ConditionKey] = CreateMetadataCondition(),
                [Data.Models.Metadata.Adjuster.DefaultKey] = true,
                [Data.Models.Metadata.Adjuster.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.Analog CreateMetadataAnalog()
        {
            return new Data.Models.Metadata.Analog
            {
                [Data.Models.Metadata.Analog.MaskKey] = "mask",
            };
        }

        private static Data.Models.Metadata.Archive CreateMetadataArchive()
        {
            return new Data.Models.Metadata.Archive
            {
                [Data.Models.Metadata.Archive.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.BiosSet CreateMetadataBiosSet()
        {
            return new Data.Models.Metadata.BiosSet
            {
                [Data.Models.Metadata.BiosSet.DefaultKey] = true,
                [Data.Models.Metadata.BiosSet.DescriptionKey] = "description",
                [Data.Models.Metadata.BiosSet.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.Chip CreateMetadataChip()
        {
            return new Data.Models.Metadata.Chip
            {
                [Data.Models.Metadata.Chip.ClockKey] = 12345L,
                [Data.Models.Metadata.Chip.FlagsKey] = "flags",
                [Data.Models.Metadata.Chip.NameKey] = "name",
                [Data.Models.Metadata.Chip.SoundOnlyKey] = "yes",
                [Data.Models.Metadata.Chip.TagKey] = "tag",
                [Data.Models.Metadata.Chip.ChipTypeKey] = "cpu",
            };
        }

        private static Data.Models.Metadata.Configuration CreateMetadataConfiguration()
        {
            return new Data.Models.Metadata.Configuration
            {
                [Data.Models.Metadata.Configuration.ConditionKey] = CreateMetadataCondition(),
                [Data.Models.Metadata.Configuration.ConfLocationKey] = new Data.Models.Metadata.ConfLocation[] { CreateMetadataConfLocation() },
                [Data.Models.Metadata.Configuration.ConfSettingKey] = new Data.Models.Metadata.ConfSetting[] { CreateMetadataConfSetting() },
                [Data.Models.Metadata.Configuration.MaskKey] = "mask",
                [Data.Models.Metadata.Configuration.NameKey] = "name",
                [Data.Models.Metadata.Configuration.TagKey] = "tag",
            };
        }

        private static Data.Models.Metadata.Condition CreateMetadataCondition()
        {
            return new Data.Models.Metadata.Condition
            {
                [Data.Models.Metadata.Condition.ValueKey] = "value",
                [Data.Models.Metadata.Condition.MaskKey] = "mask",
                [Data.Models.Metadata.Condition.RelationKey] = "eq",
                [Data.Models.Metadata.Condition.TagKey] = "tag",
            };
        }

        private static Data.Models.Metadata.ConfLocation CreateMetadataConfLocation()
        {
            return new Data.Models.Metadata.ConfLocation
            {
                [Data.Models.Metadata.ConfLocation.InvertedKey] = "yes",
                [Data.Models.Metadata.ConfLocation.NameKey] = "name",
                [Data.Models.Metadata.ConfLocation.NumberKey] = "number",
            };
        }

        private static Data.Models.Metadata.ConfSetting CreateMetadataConfSetting()
        {
            return new Data.Models.Metadata.ConfSetting
            {
                [Data.Models.Metadata.ConfSetting.ConditionKey] = CreateMetadataCondition(),
                [Data.Models.Metadata.ConfSetting.DefaultKey] = "yes",
                [Data.Models.Metadata.ConfSetting.NameKey] = "name",
                [Data.Models.Metadata.ConfSetting.ValueKey] = "value",
            };
        }

        private static Data.Models.Metadata.Control CreateMetadataControl()
        {
            return new Data.Models.Metadata.Control
            {
                [Data.Models.Metadata.Control.ButtonsKey] = 12345L,
                [Data.Models.Metadata.Control.KeyDeltaKey] = 12345L,
                [Data.Models.Metadata.Control.MaximumKey] = 12345L,
                [Data.Models.Metadata.Control.MinimumKey] = 12345L,
                [Data.Models.Metadata.Control.PlayerKey] = 12345L,
                [Data.Models.Metadata.Control.ReqButtonsKey] = 12345L,
                [Data.Models.Metadata.Control.ReverseKey] = "yes",
                [Data.Models.Metadata.Control.SensitivityKey] = 12345L,
                [Data.Models.Metadata.Control.ControlTypeKey] = "lightgun",
                [Data.Models.Metadata.Control.WaysKey] = "ways",
                [Data.Models.Metadata.Control.Ways2Key] = "ways2",
                [Data.Models.Metadata.Control.Ways3Key] = "ways3",
            };
        }

        private static Data.Models.Metadata.Device CreateMetadataDevice()
        {
            return new Data.Models.Metadata.Device
            {
                [Data.Models.Metadata.Device.ExtensionKey] = new Data.Models.Metadata.Extension[] { CreateMetadataExtension() },
                [Data.Models.Metadata.Device.FixedImageKey] = "fixedimage",
                [Data.Models.Metadata.Device.InstanceKey] = CreateMetadataInstance(),
                [Data.Models.Metadata.Device.InterfaceKey] = "interface",
                [Data.Models.Metadata.Device.MandatoryKey] = 1L,
                [Data.Models.Metadata.Device.TagKey] = "tag",
                [Data.Models.Metadata.Device.DeviceTypeKey] = "punchtape",
            };
        }

        private static Data.Models.Metadata.DeviceRef CreateMetadataDeviceRef()
        {
            return new Data.Models.Metadata.DeviceRef
            {
                [Data.Models.Metadata.DeviceRef.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.DipLocation CreateMetadataDipLocation()
        {
            return new Data.Models.Metadata.DipLocation
            {
                [Data.Models.Metadata.DipLocation.InvertedKey] = "yes",
                [Data.Models.Metadata.DipLocation.NameKey] = "name",
                [Data.Models.Metadata.DipLocation.NumberKey] = "number",
            };
        }

        private static Data.Models.Metadata.DipSwitch CreateMetadataDipSwitch()
        {
            return new Data.Models.Metadata.DipSwitch
            {
                [Data.Models.Metadata.DipSwitch.ConditionKey] = CreateMetadataCondition(),
                [Data.Models.Metadata.DipSwitch.DefaultKey] = "yes",
                [Data.Models.Metadata.DipSwitch.DipLocationKey] = new Data.Models.Metadata.DipLocation[] { CreateMetadataDipLocation() },
                [Data.Models.Metadata.DipSwitch.DipValueKey] = new Data.Models.Metadata.DipValue[] { CreateMetadataDipValue() },
                [Data.Models.Metadata.DipSwitch.EntryKey] = new string[] { "entry" },
                [Data.Models.Metadata.DipSwitch.MaskKey] = "mask",
                [Data.Models.Metadata.DipSwitch.NameKey] = "name",
                [Data.Models.Metadata.DipSwitch.TagKey] = "tag",
            };
        }

        private static Data.Models.Metadata.DipValue CreateMetadataDipValue()
        {
            return new Data.Models.Metadata.DipValue
            {
                [Data.Models.Metadata.DipValue.ConditionKey] = CreateMetadataCondition(),
                [Data.Models.Metadata.DipValue.DefaultKey] = "yes",
                [Data.Models.Metadata.DipValue.NameKey] = "name",
                [Data.Models.Metadata.DipValue.ValueKey] = "value",
            };
        }

        private static Data.Models.Metadata.DataArea CreateMetadataDataArea()
        {
            return new Data.Models.Metadata.DataArea
            {
                [Data.Models.Metadata.DataArea.EndiannessKey] = "big",
                [Data.Models.Metadata.DataArea.NameKey] = "name",
                [Data.Models.Metadata.DataArea.RomKey] = new Data.Models.Metadata.Rom[] { new Data.Models.Metadata.Rom() },
                [Data.Models.Metadata.DataArea.SizeKey] = 12345L,
                [Data.Models.Metadata.DataArea.WidthKey] = 64,
            };
        }

        private static Data.Models.Metadata.Disk CreateMetadataDisk()
        {
            return new Data.Models.Metadata.Disk
            {
                [Data.Models.Metadata.Disk.FlagsKey] = "flags",
                [Data.Models.Metadata.Disk.IndexKey] = "index",
                [Data.Models.Metadata.Disk.MD5Key] = ZeroHash.MD5Str,
                [Data.Models.Metadata.Disk.MergeKey] = "merge",
                [Data.Models.Metadata.Disk.NameKey] = "name",
                [Data.Models.Metadata.Disk.OptionalKey] = "yes",
                [Data.Models.Metadata.Disk.RegionKey] = "region",
                [Data.Models.Metadata.Disk.SHA1Key] = ZeroHash.SHA1Str,
                [Data.Models.Metadata.Disk.WritableKey] = "yes",
            };
        }

        private static Data.Models.Metadata.DiskArea CreateMetadataDiskArea()
        {
            return new Data.Models.Metadata.DiskArea
            {
                [Data.Models.Metadata.DiskArea.DiskKey] = new Data.Models.Metadata.Disk[] { new Data.Models.Metadata.Disk() },
                [Data.Models.Metadata.DiskArea.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.Display CreateMetadataDisplay()
        {
            return new Data.Models.Metadata.Display
            {
                [Data.Models.Metadata.Display.FlipXKey] = "yes",
                [Data.Models.Metadata.Display.HBEndKey] = 12345L,
                [Data.Models.Metadata.Display.HBStartKey] = 12345L,
                [Data.Models.Metadata.Display.HeightKey] = 12345L,
                [Data.Models.Metadata.Display.HTotalKey] = 12345L,
                [Data.Models.Metadata.Display.PixClockKey] = 12345L,
                [Data.Models.Metadata.Display.RefreshKey] = 12345L,
                [Data.Models.Metadata.Display.RotateKey] = 90,
                [Data.Models.Metadata.Display.TagKey] = "tag",
                [Data.Models.Metadata.Display.DisplayTypeKey] = "vector",
                [Data.Models.Metadata.Display.VBEndKey] = 12345L,
                [Data.Models.Metadata.Display.VBStartKey] = 12345L,
                [Data.Models.Metadata.Display.VTotalKey] = 12345L,
                [Data.Models.Metadata.Display.WidthKey] = 12345L,
            };
        }

        private static Data.Models.Metadata.Driver CreateMetadataDriver()
        {
            return new Data.Models.Metadata.Driver
            {
                [Data.Models.Metadata.Driver.BlitKey] = "plain",
                [Data.Models.Metadata.Driver.CocktailKey] = "good",
                [Data.Models.Metadata.Driver.ColorKey] = "good",
                [Data.Models.Metadata.Driver.EmulationKey] = "good",
                [Data.Models.Metadata.Driver.IncompleteKey] = "yes",
                [Data.Models.Metadata.Driver.NoSoundHardwareKey] = "yes",
                [Data.Models.Metadata.Driver.PaletteSizeKey] = "pallettesize",
                [Data.Models.Metadata.Driver.RequiresArtworkKey] = "yes",
                [Data.Models.Metadata.Driver.SaveStateKey] = "supported",
                [Data.Models.Metadata.Driver.SoundKey] = "good",
                [Data.Models.Metadata.Driver.StatusKey] = "good",
                [Data.Models.Metadata.Driver.UnofficialKey] = "yes",
            };
        }

        private static Data.Models.Metadata.Dump CreateMetadataDump()
        {
            return new Data.Models.Metadata.Dump
            {
                [Data.Models.Metadata.Dump.OriginalKey] = CreateMetadataOriginal(),

                // The following are searched for in order
                // For the purposes of this test, only RomKey will be populated
                // The only difference is what OpenMSXSubType value is applied
                [Data.Models.Metadata.Dump.RomKey] = new Data.Models.Metadata.Rom(),
                [Data.Models.Metadata.Dump.MegaRomKey] = null,
                [Data.Models.Metadata.Dump.SCCPlusCartKey] = null,
            };
        }

        private static Data.Models.Metadata.Extension CreateMetadataExtension()
        {
            return new Data.Models.Metadata.Extension
            {
                [Data.Models.Metadata.Extension.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.Feature CreateMetadataFeature()
        {
            return new Data.Models.Metadata.Feature
            {
                [Data.Models.Metadata.Feature.NameKey] = "name",
                [Data.Models.Metadata.Feature.OverallKey] = "imperfect",
                [Data.Models.Metadata.Feature.StatusKey] = "imperfect",
                [Data.Models.Metadata.Feature.FeatureTypeKey] = "protection",
                [Data.Models.Metadata.Feature.ValueKey] = "value",
            };
        }

        private static Data.Models.Metadata.Info CreateMetadataInfo()
        {
            return new Data.Models.Metadata.Info
            {
                [Data.Models.Metadata.Info.NameKey] = "name",
                [Data.Models.Metadata.Info.ValueKey] = "value",
            };
        }

        private static Data.Models.Metadata.Input CreateMetadataInput()
        {
            return new Data.Models.Metadata.Input
            {
                [Data.Models.Metadata.Input.ButtonsKey] = 12345L,
                [Data.Models.Metadata.Input.CoinsKey] = 12345L,
                [Data.Models.Metadata.Input.ControlKey] = new Data.Models.Metadata.Control[] { CreateMetadataControl() },
                [Data.Models.Metadata.Input.PlayersKey] = 12345L,
                [Data.Models.Metadata.Input.ServiceKey] = "yes",
                [Data.Models.Metadata.Input.TiltKey] = "yes",
            };
        }

        private static Data.Models.Metadata.Instance CreateMetadataInstance()
        {
            return new Data.Models.Metadata.Instance
            {
                [Data.Models.Metadata.Instance.BriefNameKey] = "briefname",
                [Data.Models.Metadata.Instance.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.Media CreateMetadataMedia()
        {
            return new Data.Models.Metadata.Media
            {
                [Data.Models.Metadata.Media.MD5Key] = ZeroHash.MD5Str,
                [Data.Models.Metadata.Media.NameKey] = "name",
                [Data.Models.Metadata.Media.SHA1Key] = ZeroHash.SHA1Str,
                [Data.Models.Metadata.Media.SHA256Key] = ZeroHash.SHA256Str,
                [Data.Models.Metadata.Media.SpamSumKey] = ZeroHash.SpamSumStr,
            };
        }

        private static Data.Models.Metadata.Original CreateMetadataOriginal()
        {
            return new Data.Models.Metadata.Original
            {
                [Data.Models.Metadata.Original.ContentKey] = "content",
                [Data.Models.Metadata.Original.ValueKey] = true,
            };
        }

        private static Data.Models.Metadata.Part CreateMetadataPart()
        {
            return new Data.Models.Metadata.Part
            {
                [Data.Models.Metadata.Part.DataAreaKey] = new Data.Models.Metadata.DataArea[] { CreateMetadataDataArea() },
                [Data.Models.Metadata.Part.DiskAreaKey] = new Data.Models.Metadata.DiskArea[] { CreateMetadataDiskArea() },
                [Data.Models.Metadata.Part.DipSwitchKey] = new Data.Models.Metadata.DipSwitch[] { new Data.Models.Metadata.DipSwitch() },
                [Data.Models.Metadata.Part.FeatureKey] = new Data.Models.Metadata.Feature[] { CreateMetadataFeature() },
                [Data.Models.Metadata.Part.InterfaceKey] = "interface",
                [Data.Models.Metadata.Part.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.Port CreateMetadataPort()
        {
            return new Data.Models.Metadata.Port
            {
                [Data.Models.Metadata.Port.AnalogKey] = new Data.Models.Metadata.Analog[] { CreateMetadataAnalog() },
                [Data.Models.Metadata.Port.TagKey] = "tag",
            };
        }

        private static Data.Models.Metadata.RamOption CreateMetadataRamOption()
        {
            return new Data.Models.Metadata.RamOption
            {
                [Data.Models.Metadata.RamOption.ContentKey] = "content",
                [Data.Models.Metadata.RamOption.DefaultKey] = "yes",
                [Data.Models.Metadata.RamOption.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.Release CreateMetadataRelease()
        {
            return new Data.Models.Metadata.Release
            {
                [Data.Models.Metadata.Release.DateKey] = "date",
                [Data.Models.Metadata.Release.DefaultKey] = "yes",
                [Data.Models.Metadata.Release.LanguageKey] = "language",
                [Data.Models.Metadata.Release.NameKey] = "name",
                [Data.Models.Metadata.Release.RegionKey] = "region",
            };
        }

        private static Data.Models.Metadata.Rom CreateMetadataRom()
        {
            return new Data.Models.Metadata.Rom
            {
                [Data.Models.Metadata.Rom.AlbumKey] = "album",
                [Data.Models.Metadata.Rom.AltRomnameKey] = "alt_romname",
                [Data.Models.Metadata.Rom.AltTitleKey] = "alt_title",
                [Data.Models.Metadata.Rom.ArtistKey] = "artist",
                [Data.Models.Metadata.Rom.ASRDetectedLangKey] = "asr_detected_lang",
                [Data.Models.Metadata.Rom.ASRDetectedLangConfKey] = "asr_detected_lang_conf",
                [Data.Models.Metadata.Rom.ASRTranscribedLangKey] = "asr_transcribed_lang",
                [Data.Models.Metadata.Rom.BiosKey] = "bios",
                [Data.Models.Metadata.Rom.BitrateKey] = "bitrate",
                [Data.Models.Metadata.Rom.BitTorrentMagnetHashKey] = "btih",
                [Data.Models.Metadata.Rom.ClothCoverDetectionModuleVersionKey] = "cloth_cover_detection_module_version",
                [Data.Models.Metadata.Rom.CollectionCatalogNumberKey] = "collection-catalog-number",
                [Data.Models.Metadata.Rom.CommentKey] = "comment",
                [Data.Models.Metadata.Rom.CRCKey] = ZeroHash.CRC32Str,
                [Data.Models.Metadata.Rom.CreatorKey] = "creator",
                [Data.Models.Metadata.Rom.DateKey] = "date",
                [Data.Models.Metadata.Rom.DisposeKey] = "yes",
                [Data.Models.Metadata.Rom.ExtensionKey] = "extension",
                [Data.Models.Metadata.Rom.FileCountKey] = 12345L,
                [Data.Models.Metadata.Rom.FileIsAvailableKey] = true,
                [Data.Models.Metadata.Rom.FlagsKey] = "flags",
                [Data.Models.Metadata.Rom.FormatKey] = "format",
                [Data.Models.Metadata.Rom.HeaderKey] = "header",
                [Data.Models.Metadata.Rom.HeightKey] = "height",
                [Data.Models.Metadata.Rom.hOCRCharToWordhOCRVersionKey] = "hocr_char_to_word_hocr_version",
                [Data.Models.Metadata.Rom.hOCRCharToWordModuleVersionKey] = "hocr_char_to_word_module_version",
                [Data.Models.Metadata.Rom.hOCRFtsTexthOCRVersionKey] = "hocr_fts_text_hocr_version",
                [Data.Models.Metadata.Rom.hOCRFtsTextModuleVersionKey] = "hocr_fts_text_module_version",
                [Data.Models.Metadata.Rom.hOCRPageIndexhOCRVersionKey] = "hocr_pageindex_hocr_version",
                [Data.Models.Metadata.Rom.hOCRPageIndexModuleVersionKey] = "hocr_pageindex_module_version",
                [Data.Models.Metadata.Rom.InvertedKey] = "yes",
                [Data.Models.Metadata.Rom.LastModifiedTimeKey] = "mtime",
                [Data.Models.Metadata.Rom.LengthKey] = "length",
                [Data.Models.Metadata.Rom.LoadFlagKey] = "load16_byte",
                [Data.Models.Metadata.Rom.MatrixNumberKey] = "matrix_number",
                [Data.Models.Metadata.Rom.MD2Key] = ZeroHash.GetString(HashType.MD2),
                [Data.Models.Metadata.Rom.MD4Key] = ZeroHash.GetString(HashType.MD4),
                [Data.Models.Metadata.Rom.MD5Key] = ZeroHash.MD5Str,
                // [Data.Models.Metadata.Rom.OpenMSXMediaType] = null, // Omit due to other test
                [Data.Models.Metadata.Rom.MergeKey] = "merge",
                [Data.Models.Metadata.Rom.MIAKey] = "yes",
                [Data.Models.Metadata.Rom.NameKey] = "name",
                [Data.Models.Metadata.Rom.TesseractOCRKey] = "ocr",
                [Data.Models.Metadata.Rom.TesseractOCRConvertedKey] = "ocr_converted",
                [Data.Models.Metadata.Rom.TesseractOCRDetectedLangKey] = "ocr_detected_lang",
                [Data.Models.Metadata.Rom.TesseractOCRDetectedLangConfKey] = "ocr_detected_lang_conf",
                [Data.Models.Metadata.Rom.TesseractOCRDetectedScriptKey] = "ocr_detected_script",
                [Data.Models.Metadata.Rom.TesseractOCRDetectedScriptConfKey] = "ocr_detected_script_conf",
                [Data.Models.Metadata.Rom.TesseractOCRModuleVersionKey] = "ocr_module_version",
                [Data.Models.Metadata.Rom.TesseractOCRParametersKey] = "ocr_parameters",
                [Data.Models.Metadata.Rom.OffsetKey] = "offset",
                [Data.Models.Metadata.Rom.OptionalKey] = "yes",
                [Data.Models.Metadata.Rom.OriginalKey] = "original",
                [Data.Models.Metadata.Rom.PDFModuleVersionKey] = "pdf_module_version",
                [Data.Models.Metadata.Rom.PreviewImageKey] = "preview-image",
                [Data.Models.Metadata.Rom.PublisherKey] = "publisher",
                [Data.Models.Metadata.Rom.RegionKey] = "region",
                [Data.Models.Metadata.Rom.RemarkKey] = "remark",
                [Data.Models.Metadata.Rom.RIPEMD128Key] = ZeroHash.GetString(HashType.RIPEMD128),
                [Data.Models.Metadata.Rom.RIPEMD160Key] = ZeroHash.GetString(HashType.RIPEMD160),
                [Data.Models.Metadata.Rom.RotationKey] = "rotation",
                [Data.Models.Metadata.Rom.SerialKey] = "serial",
                [Data.Models.Metadata.Rom.SHA1Key] = ZeroHash.SHA1Str,
                [Data.Models.Metadata.Rom.SHA256Key] = ZeroHash.SHA256Str,
                [Data.Models.Metadata.Rom.SHA384Key] = ZeroHash.SHA384Str,
                [Data.Models.Metadata.Rom.SHA512Key] = ZeroHash.SHA512Str,
                [Data.Models.Metadata.Rom.SizeKey] = 12345L,
                [Data.Models.Metadata.Rom.SoundOnlyKey] = "yes",
                [Data.Models.Metadata.Rom.SourceKey] = "source",
                [Data.Models.Metadata.Rom.SpamSumKey] = ZeroHash.SpamSumStr,
                [Data.Models.Metadata.Rom.StartKey] = "start",
                [Data.Models.Metadata.Rom.StatusKey] = "good",
                [Data.Models.Metadata.Rom.SummationKey] = "summation",
                [Data.Models.Metadata.Rom.TitleKey] = "title",
                [Data.Models.Metadata.Rom.TrackKey] = "track",
                [Data.Models.Metadata.Rom.OpenMSXType] = "type",
                [Data.Models.Metadata.Rom.ValueKey] = "value",
                [Data.Models.Metadata.Rom.WhisperASRModuleVersionKey] = "whisper_asr_module_version",
                [Data.Models.Metadata.Rom.WhisperModelHashKey] = "whisper_model_hash",
                [Data.Models.Metadata.Rom.WhisperModelNameKey] = "whisper_model_name",
                [Data.Models.Metadata.Rom.WhisperVersionKey] = "whisper_version",
                [Data.Models.Metadata.Rom.WidthKey] = "width",
                [Data.Models.Metadata.Rom.WordConfidenceInterval0To10Key] = "word_conf_0_10",
                [Data.Models.Metadata.Rom.WordConfidenceInterval11To20Key] = "word_conf_11_20",
                [Data.Models.Metadata.Rom.WordConfidenceInterval21To30Key] = "word_conf_21_30",
                [Data.Models.Metadata.Rom.WordConfidenceInterval31To40Key] = "word_conf_31_40",
                [Data.Models.Metadata.Rom.WordConfidenceInterval41To50Key] = "word_conf_41_50",
                [Data.Models.Metadata.Rom.WordConfidenceInterval51To60Key] = "word_conf_51_60",
                [Data.Models.Metadata.Rom.WordConfidenceInterval61To70Key] = "word_conf_61_70",
                [Data.Models.Metadata.Rom.WordConfidenceInterval71To80Key] = "word_conf_71_80",
                [Data.Models.Metadata.Rom.WordConfidenceInterval81To90Key] = "word_conf_81_90",
                [Data.Models.Metadata.Rom.WordConfidenceInterval91To100Key] = "word_conf_91_100",
                [Data.Models.Metadata.Rom.xxHash364Key] = ZeroHash.GetString(HashType.XxHash3),
                [Data.Models.Metadata.Rom.xxHash3128Key] = ZeroHash.GetString(HashType.XxHash128),
            };
        }

        private static Data.Models.Metadata.Sample CreateMetadataSample()
        {
            return new Data.Models.Metadata.Sample
            {
                [Data.Models.Metadata.Sample.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.SharedFeat CreateMetadataSharedFeat()
        {
            return new Data.Models.Metadata.SharedFeat
            {
                [Data.Models.Metadata.SharedFeat.NameKey] = "name",
                [Data.Models.Metadata.SharedFeat.ValueKey] = "value",
            };
        }

        private static Data.Models.Metadata.Slot CreateMetadataSlot()
        {
            return new Data.Models.Metadata.Slot
            {
                [Data.Models.Metadata.Slot.NameKey] = "name",
                [Data.Models.Metadata.Slot.SlotOptionKey] = new Data.Models.Metadata.SlotOption[] { CreateMetadataSlotOption() },
            };
        }

        private static Data.Models.Metadata.SlotOption CreateMetadataSlotOption()
        {
            return new Data.Models.Metadata.SlotOption
            {
                [Data.Models.Metadata.SlotOption.DefaultKey] = "yes",
                [Data.Models.Metadata.SlotOption.DevNameKey] = "devname",
                [Data.Models.Metadata.SlotOption.NameKey] = "name",
            };
        }

        private static Data.Models.Metadata.Software CreateMetadataSoftware()
        {
            return new Data.Models.Metadata.Software
            {
                [Data.Models.Metadata.Software.CloneOfKey] = "cloneof",
                [Data.Models.Metadata.Software.DescriptionKey] = "description",
                [Data.Models.Metadata.Software.InfoKey] = new Data.Models.Metadata.Info[] { CreateMetadataInfo() },
                [Data.Models.Metadata.Software.NameKey] = "name",
                [Data.Models.Metadata.Software.NotesKey] = "notes",
                [Data.Models.Metadata.Software.PartKey] = new Data.Models.Metadata.Part[] { CreateMetadataPart() },
                [Data.Models.Metadata.Software.PublisherKey] = "publisher",
                [Data.Models.Metadata.Software.SharedFeatKey] = new Data.Models.Metadata.SharedFeat[] { CreateMetadataSharedFeat() },
                [Data.Models.Metadata.Software.SupportedKey] = "yes",
                [Data.Models.Metadata.Software.YearKey] = "year",
            };
        }

        private static Data.Models.Metadata.SoftwareList CreateMetadataSoftwareList()
        {
            return new Data.Models.Metadata.SoftwareList
            {
                [Data.Models.Metadata.SoftwareList.DescriptionKey] = "description",
                [Data.Models.Metadata.SoftwareList.FilterKey] = "filter",
                [Data.Models.Metadata.SoftwareList.NameKey] = "name",
                [Data.Models.Metadata.SoftwareList.NotesKey] = "notes",
                [Data.Models.Metadata.SoftwareList.SoftwareKey] = new Data.Models.Metadata.Software[] { CreateMetadataSoftware() },
                [Data.Models.Metadata.SoftwareList.StatusKey] = "original",
                [Data.Models.Metadata.SoftwareList.TagKey] = "tag",
            };
        }

        private static Data.Models.Metadata.Sound CreateMetadataSound()
        {
            return new Data.Models.Metadata.Sound
            {
                [Data.Models.Metadata.Sound.ChannelsKey] = 12345L,
            };
        }

        private static Data.Models.Logiqx.Trurip CreateMetadataTrurip()
        {
            return new Data.Models.Logiqx.Trurip
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

        private static Data.Models.Metadata.Video CreateMetadataVideo()
        {
            return new Data.Models.Metadata.Video
            {
                [Data.Models.Metadata.Video.AspectXKey] = 12345L,
                [Data.Models.Metadata.Video.AspectYKey] = 12345L,
                [Data.Models.Metadata.Video.HeightKey] = 12345L,
                [Data.Models.Metadata.Video.OrientationKey] = "vertical",
                [Data.Models.Metadata.Video.RefreshKey] = 12345L,
                [Data.Models.Metadata.Video.ScreenKey] = "vector",
                [Data.Models.Metadata.Video.WidthKey] = 12345L,
            };
        }

        #endregion

        #region Validation Helpers

        private static void ValidateHeader(DatHeader datHeader)
        {
            Assert.Equal("author", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.AuthorKey));
            Assert.Equal("merged", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.BiosModeKey));
            Assert.Equal("build", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.BuildKey));
            Assert.Equal("ext", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.CanOpenKey));
            Assert.Equal("category", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.CategoryKey));
            Assert.Equal("comment", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.CommentKey));
            Assert.Equal("date", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.DateKey));
            Assert.Equal("datversion", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.DatVersionKey));
            Assert.True(datHeader.GetBoolFieldValue(Data.Models.Metadata.Header.DebugKey));
            Assert.Equal("description", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey));
            Assert.Equal("email", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.EmailKey));
            Assert.Equal("emulatorversion", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.EmulatorVersionKey));
            Assert.Equal("merged", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.ForceMergingKey));
            Assert.Equal("required", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.ForceNodumpKey));
            Assert.Equal("zip", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.ForcePackingKey));
            Assert.True(datHeader.GetBoolFieldValue(Data.Models.Metadata.Header.ForceZippingKey));
            Assert.Equal("header", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.HeaderKey));
            Assert.Equal("homepage", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.HomepageKey));
            Assert.Equal("id", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.IdKey));
            Assert.NotNull(datHeader.GetStringFieldValue(Data.Models.Metadata.Header.ImagesKey));
            Assert.Equal("imfolder", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.ImFolderKey));
            Assert.NotNull(datHeader.GetStringFieldValue(Data.Models.Metadata.Header.InfosKey));
            Assert.True(datHeader.GetBoolFieldValue(Data.Models.Metadata.Header.LockBiosModeKey));
            Assert.True(datHeader.GetBoolFieldValue(Data.Models.Metadata.Header.LockRomModeKey));
            Assert.True(datHeader.GetBoolFieldValue(Data.Models.Metadata.Header.LockSampleModeKey));
            Assert.Equal("mameconfig", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.MameConfigKey));
            Assert.Equal("name", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.NameKey));
            Assert.NotNull(datHeader.GetStringFieldValue(Data.Models.Metadata.Header.NewDatKey));
            Assert.Equal("notes", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.NotesKey));
            Assert.Equal("plugin", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.PluginKey));
            Assert.Equal("refname", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.RefNameKey));
            Assert.Equal("merged", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.RomModeKey));
            Assert.Equal("romtitle", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.RomTitleKey));
            Assert.Equal("rootdir", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.RootDirKey));
            Assert.Equal("merged", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.SampleModeKey));
            Assert.Equal("schemalocation", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.SchemaLocationKey));
            Assert.Equal("screenshotsheight", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.ScreenshotsHeightKey));
            Assert.Equal("screenshotsWidth", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.ScreenshotsWidthKey));
            Assert.NotNull(datHeader.GetStringFieldValue(Data.Models.Metadata.Header.SearchKey));
            Assert.Equal("system", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.SystemKey));
            Assert.Equal("timestamp", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.TimestampKey));
            Assert.Equal("type", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.TypeKey));
            Assert.Equal("url", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.UrlKey));
            Assert.Equal("version", datHeader.GetStringFieldValue(Data.Models.Metadata.Header.VersionKey));
        }

        private static void ValidateMachine(DatItems.Machine machine)
        {
            Assert.Equal("board", machine.GetStringFieldValue(Data.Models.Metadata.Machine.BoardKey));
            Assert.Equal("buttons", machine.GetStringFieldValue(Data.Models.Metadata.Machine.ButtonsKey));
            Assert.Equal("category", machine.GetStringFieldValue(Data.Models.Metadata.Machine.CategoryKey));
            Assert.Equal("cloneof", machine.GetStringFieldValue(Data.Models.Metadata.Machine.CloneOfKey));
            Assert.Equal("cloneofid", machine.GetStringFieldValue(Data.Models.Metadata.Machine.CloneOfIdKey));
            Assert.Equal("comment", machine.GetStringFieldValue(Data.Models.Metadata.Machine.CommentKey));
            Assert.Equal("company", machine.GetStringFieldValue(Data.Models.Metadata.Machine.CompanyKey));
            Assert.Equal("control", machine.GetStringFieldValue(Data.Models.Metadata.Machine.ControlKey));
            Assert.Equal("country", machine.GetStringFieldValue(Data.Models.Metadata.Machine.CountryKey));
            Assert.Equal("description", machine.GetStringFieldValue(Data.Models.Metadata.Machine.DescriptionKey));
            Assert.Equal("dirname", machine.GetStringFieldValue(Data.Models.Metadata.Machine.DirNameKey));
            Assert.Equal("displaycount", machine.GetStringFieldValue(Data.Models.Metadata.Machine.DisplayCountKey));
            Assert.Equal("displaytype", machine.GetStringFieldValue(Data.Models.Metadata.Machine.DisplayTypeKey));
            Assert.Equal("duplicateid", machine.GetStringFieldValue(Data.Models.Metadata.Machine.DuplicateIDKey));
            Assert.Equal("emulator", machine.GetStringFieldValue(Data.Models.Metadata.Machine.EmulatorKey));
            Assert.Equal("extra", machine.GetStringFieldValue(Data.Models.Metadata.Machine.ExtraKey));
            Assert.Equal("favorite", machine.GetStringFieldValue(Data.Models.Metadata.Machine.FavoriteKey));
            Assert.Equal("genmsxid", machine.GetStringFieldValue(Data.Models.Metadata.Machine.GenMSXIDKey));
            Assert.Equal("history", machine.GetStringFieldValue(Data.Models.Metadata.Machine.HistoryKey));
            Assert.Equal("id", machine.GetStringFieldValue(Data.Models.Metadata.Machine.IdKey));
            Assert.Equal(ZeroHash.CRC32Str, machine.GetStringFieldValue(Data.Models.Metadata.Machine.Im1CRCKey));
            Assert.Equal(ZeroHash.CRC32Str, machine.GetStringFieldValue(Data.Models.Metadata.Machine.Im2CRCKey));
            Assert.Equal("imagenumber", machine.GetStringFieldValue(Data.Models.Metadata.Machine.ImageNumberKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Data.Models.Metadata.Machine.IsBiosKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Data.Models.Metadata.Machine.IsDeviceKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Data.Models.Metadata.Machine.IsMechanicalKey));
            Assert.Equal("language", machine.GetStringFieldValue(Data.Models.Metadata.Machine.LanguageKey));
            Assert.Equal("location", machine.GetStringFieldValue(Data.Models.Metadata.Machine.LocationKey));
            Assert.Equal("manufacturer", machine.GetStringFieldValue(Data.Models.Metadata.Machine.ManufacturerKey));
            Assert.Equal("name", machine.GetName());
            Assert.Equal("notes", machine.GetStringFieldValue(Data.Models.Metadata.Machine.NotesKey));
            Assert.Equal("playedcount", machine.GetStringFieldValue(Data.Models.Metadata.Machine.PlayedCountKey));
            Assert.Equal("playedtime", machine.GetStringFieldValue(Data.Models.Metadata.Machine.PlayedTimeKey));
            Assert.Equal("players", machine.GetStringFieldValue(Data.Models.Metadata.Machine.PlayersKey));
            Assert.Equal("publisher", machine.GetStringFieldValue(Data.Models.Metadata.Machine.PublisherKey));
            Assert.Equal("rebuildto", machine.GetStringFieldValue(Data.Models.Metadata.Machine.RebuildToKey));
            Assert.Equal("releasenumber", machine.GetStringFieldValue(Data.Models.Metadata.Machine.ReleaseNumberKey));
            Assert.Equal("romof", machine.GetStringFieldValue(Data.Models.Metadata.Machine.RomOfKey));
            Assert.Equal("rotation", machine.GetStringFieldValue(Data.Models.Metadata.Machine.RotationKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Data.Models.Metadata.Machine.RunnableKey));
            Assert.Equal("sampleof", machine.GetStringFieldValue(Data.Models.Metadata.Machine.SampleOfKey));
            Assert.Equal("savetype", machine.GetStringFieldValue(Data.Models.Metadata.Machine.SaveTypeKey));
            Assert.Equal("sourcefile", machine.GetStringFieldValue(Data.Models.Metadata.Machine.SourceFileKey));
            Assert.Equal("sourcerom", machine.GetStringFieldValue(Data.Models.Metadata.Machine.SourceRomKey));
            Assert.Equal("status", machine.GetStringFieldValue(Data.Models.Metadata.Machine.StatusKey));
            Assert.Equal("yes", machine.GetStringFieldValue(Data.Models.Metadata.Machine.SupportedKey));
            Assert.Equal("system", machine.GetStringFieldValue(Data.Models.Metadata.Machine.SystemKey));
            Assert.Equal("tags", machine.GetStringFieldValue(Data.Models.Metadata.Machine.TagsKey));
            Assert.Equal("year", machine.GetStringFieldValue(Data.Models.Metadata.Machine.YearKey));

            DatItems.Trurip? trurip = machine.GetFieldValue<DatItems.Trurip>(Data.Models.Metadata.Machine.TruripKey);
            ValidateTrurip(trurip);
        }

        private static void ValidateAdjuster(Adjuster? adjuster)
        {
            Assert.NotNull(adjuster);
            Assert.True(adjuster.GetBoolFieldValue(Data.Models.Metadata.Adjuster.DefaultKey));
            Assert.Equal("name", adjuster.GetStringFieldValue(Data.Models.Metadata.Adjuster.NameKey));

            Condition? condition = adjuster.GetFieldValue<Condition>(Data.Models.Metadata.Adjuster.ConditionKey);
            ValidateCondition(condition);
        }

        private static void ValidateAnalog(Analog? analog)
        {
            Assert.NotNull(analog);
            Assert.Equal("mask", analog.GetStringFieldValue(Data.Models.Metadata.Analog.MaskKey));
        }

        private static void ValidateArchive(Archive? archive)
        {
            Assert.NotNull(archive);
            Assert.Equal("name", archive.GetStringFieldValue(Data.Models.Metadata.Archive.NameKey));
        }

        private static void ValidateBiosSet(BiosSet? biosSet)
        {
            Assert.NotNull(biosSet);
            Assert.True(biosSet.GetBoolFieldValue(Data.Models.Metadata.BiosSet.DefaultKey));
            Assert.Equal("description", biosSet.GetStringFieldValue(Data.Models.Metadata.BiosSet.DescriptionKey));
            Assert.Equal("name", biosSet.GetStringFieldValue(Data.Models.Metadata.BiosSet.NameKey));
        }

        private static void ValidateChip(Chip? chip)
        {
            Assert.NotNull(chip);
            Assert.Equal(12345L, chip.GetInt64FieldValue(Data.Models.Metadata.Chip.ClockKey));
            Assert.Equal("flags", chip.GetStringFieldValue(Data.Models.Metadata.Chip.FlagsKey));
            Assert.Equal("name", chip.GetStringFieldValue(Data.Models.Metadata.Chip.NameKey));
            Assert.True(chip.GetBoolFieldValue(Data.Models.Metadata.Chip.SoundOnlyKey));
            Assert.Equal("tag", chip.GetStringFieldValue(Data.Models.Metadata.Chip.TagKey));
            Assert.Equal("cpu", chip.GetStringFieldValue(Data.Models.Metadata.Chip.ChipTypeKey));
        }

        private static void ValidateCondition(Condition? condition)
        {
            Assert.NotNull(condition);
            Assert.Equal("value", condition.GetStringFieldValue(Data.Models.Metadata.Condition.ValueKey));
            Assert.Equal("mask", condition.GetStringFieldValue(Data.Models.Metadata.Condition.MaskKey));
            Assert.Equal("eq", condition.GetStringFieldValue(Data.Models.Metadata.Condition.RelationKey));
            Assert.Equal("tag", condition.GetStringFieldValue(Data.Models.Metadata.Condition.TagKey));
        }

        private static void ValidateConfiguration(Configuration? configuration)
        {
            Assert.NotNull(configuration);
            Assert.Equal("mask", configuration.GetStringFieldValue(Data.Models.Metadata.Configuration.MaskKey));
            Assert.Equal("name", configuration.GetStringFieldValue(Data.Models.Metadata.Configuration.NameKey));
            Assert.Equal("tag", configuration.GetStringFieldValue(Data.Models.Metadata.Configuration.TagKey));

            Condition? condition = configuration.GetFieldValue<Condition>(Data.Models.Metadata.Configuration.ConditionKey);
            ValidateCondition(condition);

            ConfLocation[]? confLocations = configuration.GetFieldValue<ConfLocation[]>(Data.Models.Metadata.Configuration.ConfLocationKey);
            Assert.NotNull(confLocations);
            ConfLocation? confLocation = Assert.Single(confLocations);
            ValidateConfLocation(confLocation);

            ConfSetting[]? confSettings = configuration.GetFieldValue<ConfSetting[]>(Data.Models.Metadata.Configuration.ConfSettingKey);
            Assert.NotNull(confSettings);
            ConfSetting? confSetting = Assert.Single(confSettings);
            ValidateConfSetting(confSetting);
        }

        private static void ValidateConfLocation(ConfLocation? confLocation)
        {
            Assert.NotNull(confLocation);
            Assert.True(confLocation.GetBoolFieldValue(Data.Models.Metadata.ConfLocation.InvertedKey));
            Assert.Equal("name", confLocation.GetStringFieldValue(Data.Models.Metadata.ConfLocation.NameKey));
            Assert.Equal("number", confLocation.GetStringFieldValue(Data.Models.Metadata.ConfLocation.NumberKey));
        }

        private static void ValidateConfSetting(ConfSetting? confSetting)
        {
            Assert.NotNull(confSetting);
            Assert.True(confSetting.GetBoolFieldValue(Data.Models.Metadata.ConfSetting.DefaultKey));
            Assert.Equal("name", confSetting.GetStringFieldValue(Data.Models.Metadata.ConfSetting.NameKey));
            Assert.Equal("value", confSetting.GetStringFieldValue(Data.Models.Metadata.ConfSetting.ValueKey));

            Condition? condition = confSetting.GetFieldValue<Condition>(Data.Models.Metadata.ConfSetting.ConditionKey);
            ValidateCondition(condition);
        }

        private static void ValidateControl(Control? control)
        {
            Assert.NotNull(control);
            Assert.Equal(12345L, control.GetInt64FieldValue(Data.Models.Metadata.Control.ButtonsKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Data.Models.Metadata.Control.KeyDeltaKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Data.Models.Metadata.Control.MaximumKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Data.Models.Metadata.Control.MinimumKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Data.Models.Metadata.Control.PlayerKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Data.Models.Metadata.Control.ReqButtonsKey));
            Assert.True(control.GetBoolFieldValue(Data.Models.Metadata.Control.ReverseKey));
            Assert.Equal(12345L, control.GetInt64FieldValue(Data.Models.Metadata.Control.SensitivityKey));
            Assert.Equal("lightgun", control.GetStringFieldValue(Data.Models.Metadata.Control.ControlTypeKey));
            Assert.Equal("ways", control.GetStringFieldValue(Data.Models.Metadata.Control.WaysKey));
            Assert.Equal("ways2", control.GetStringFieldValue(Data.Models.Metadata.Control.Ways2Key));
            Assert.Equal("ways3", control.GetStringFieldValue(Data.Models.Metadata.Control.Ways3Key));
        }

        private static void ValidateDataArea(DataArea? dataArea)
        {
            Assert.NotNull(dataArea);
            Assert.Equal("big", dataArea.GetStringFieldValue(Data.Models.Metadata.DataArea.EndiannessKey));
            Assert.Equal("name", dataArea.GetStringFieldValue(Data.Models.Metadata.DataArea.NameKey));
            Assert.Equal(12345L, dataArea.GetInt64FieldValue(Data.Models.Metadata.DataArea.SizeKey));
            Assert.Equal(64, dataArea.GetInt64FieldValue(Data.Models.Metadata.DataArea.WidthKey));
        }

        private static void ValidateDevice(Device? device)
        {
            Assert.NotNull(device);
            Assert.Equal("fixedimage", device.GetStringFieldValue(Data.Models.Metadata.Device.FixedImageKey));
            Assert.Equal("interface", device.GetStringFieldValue(Data.Models.Metadata.Device.InterfaceKey));
            Assert.Equal(1, device.GetInt64FieldValue(Data.Models.Metadata.Device.MandatoryKey));
            Assert.Equal("tag", device.GetStringFieldValue(Data.Models.Metadata.Device.TagKey));
            Assert.Equal("punchtape", device.GetStringFieldValue(Data.Models.Metadata.Device.DeviceTypeKey));

            Extension[]? extensions = device.GetFieldValue<Extension[]>(Data.Models.Metadata.Device.ExtensionKey);
            Assert.NotNull(extensions);
            Extension? extension = Assert.Single(extensions);
            ValidateExtension(extension);

            Instance? instance = device.GetFieldValue<Instance>(Data.Models.Metadata.Device.InstanceKey);
            ValidateInstance(instance);
        }

        private static void ValidateDeviceRef(DeviceRef? deviceRef)
        {
            Assert.NotNull(deviceRef);
            Assert.Equal("name", deviceRef.GetStringFieldValue(Data.Models.Metadata.DeviceRef.NameKey));
        }

        private static void ValidateDipLocation(DipLocation? dipLocation)
        {
            Assert.NotNull(dipLocation);
            Assert.True(dipLocation.GetBoolFieldValue(Data.Models.Metadata.DipLocation.InvertedKey));
            Assert.Equal("name", dipLocation.GetStringFieldValue(Data.Models.Metadata.DipLocation.NameKey));
            Assert.Equal("number", dipLocation.GetStringFieldValue(Data.Models.Metadata.DipLocation.NumberKey));
        }

        private static void ValidateDipSwitch(DipSwitch? dipSwitch)
        {
            Assert.NotNull(dipSwitch);
            Assert.True(dipSwitch.GetBoolFieldValue(Data.Models.Metadata.DipSwitch.DefaultKey));
            Assert.Equal("mask", dipSwitch.GetStringFieldValue(Data.Models.Metadata.DipSwitch.MaskKey));
            Assert.Equal("name", dipSwitch.GetStringFieldValue(Data.Models.Metadata.DipSwitch.NameKey));
            Assert.Equal("tag", dipSwitch.GetStringFieldValue(Data.Models.Metadata.DipSwitch.TagKey));

            Condition? condition = dipSwitch.GetFieldValue<Condition>(Data.Models.Metadata.DipSwitch.ConditionKey);
            ValidateCondition(condition);

            DipLocation[]? dipLocations = dipSwitch.GetFieldValue<DipLocation[]>(Data.Models.Metadata.DipSwitch.DipLocationKey);
            Assert.NotNull(dipLocations);
            DipLocation? dipLocation = Assert.Single(dipLocations);
            ValidateDipLocation(dipLocation);

            DipValue[]? dipValues = dipSwitch.GetFieldValue<DipValue[]>(Data.Models.Metadata.DipSwitch.DipValueKey);
            Assert.NotNull(dipValues);
            DipValue? dipValue = Assert.Single(dipValues);
            ValidateDipValue(dipValue);

            string[]? entries = dipSwitch.GetStringArrayFieldValue(Data.Models.Metadata.DipSwitch.EntryKey);
            Assert.NotNull(entries);
            string entry = Assert.Single(entries);
            Assert.Equal("entry", entry);
        }

        private static void ValidateDipValue(DipValue? dipValue)
        {
            Assert.NotNull(dipValue);
            Assert.True(dipValue.GetBoolFieldValue(Data.Models.Metadata.DipValue.DefaultKey));
            Assert.Equal("name", dipValue.GetStringFieldValue(Data.Models.Metadata.DipValue.NameKey));
            Assert.Equal("value", dipValue.GetStringFieldValue(Data.Models.Metadata.DipValue.ValueKey));

            Condition? condition = dipValue.GetFieldValue<Condition>(Data.Models.Metadata.DipValue.ConditionKey);
            ValidateCondition(condition);
        }

        private static void ValidateDisk(Disk? disk)
        {
            Assert.NotNull(disk);
            Assert.Equal("flags", disk.GetStringFieldValue(Data.Models.Metadata.Disk.FlagsKey));
            Assert.Equal("index", disk.GetStringFieldValue(Data.Models.Metadata.Disk.IndexKey));
            Assert.Equal(ZeroHash.MD5Str, disk.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key));
            Assert.Equal("merge", disk.GetStringFieldValue(Data.Models.Metadata.Disk.MergeKey));
            Assert.Equal("name", disk.GetStringFieldValue(Data.Models.Metadata.Disk.NameKey));
            Assert.True(disk.GetBoolFieldValue(Data.Models.Metadata.Disk.OptionalKey));
            Assert.Equal("region", disk.GetStringFieldValue(Data.Models.Metadata.Disk.RegionKey));
            Assert.Equal(ZeroHash.SHA1Str, disk.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key));
            Assert.True(disk.GetBoolFieldValue(Data.Models.Metadata.Disk.WritableKey));
        }

        private static void ValidateDiskArea(DiskArea? diskArea)
        {
            Assert.NotNull(diskArea);
            Assert.Equal("name", diskArea.GetStringFieldValue(Data.Models.Metadata.DiskArea.NameKey));
        }

        private static void ValidateDisplay(Display? display)
        {
            Assert.NotNull(display);
            Assert.True(display.GetBoolFieldValue(Data.Models.Metadata.Display.FlipXKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.HBEndKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.HBStartKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.HeightKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.HTotalKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.PixClockKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.RefreshKey));
            Assert.Equal(90, display.GetInt64FieldValue(Data.Models.Metadata.Display.RotateKey));
            Assert.Equal("tag", display.GetStringFieldValue(Data.Models.Metadata.Display.TagKey));
            Assert.Equal("vector", display.GetStringFieldValue(Data.Models.Metadata.Display.DisplayTypeKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.VBEndKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.VBStartKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.VTotalKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.WidthKey));
        }

        private static void ValidateDriver(Driver? driver)
        {
            Assert.NotNull(driver);
            Assert.Equal("plain", driver.GetStringFieldValue(Data.Models.Metadata.Driver.BlitKey));
            Assert.Equal("good", driver.GetStringFieldValue(Data.Models.Metadata.Driver.CocktailKey));
            Assert.Equal("good", driver.GetStringFieldValue(Data.Models.Metadata.Driver.ColorKey));
            Assert.Equal("good", driver.GetStringFieldValue(Data.Models.Metadata.Driver.EmulationKey));
            Assert.True(driver.GetBoolFieldValue(Data.Models.Metadata.Driver.IncompleteKey));
            Assert.True(driver.GetBoolFieldValue(Data.Models.Metadata.Driver.NoSoundHardwareKey));
            Assert.Equal("pallettesize", driver.GetStringFieldValue(Data.Models.Metadata.Driver.PaletteSizeKey));
            Assert.True(driver.GetBoolFieldValue(Data.Models.Metadata.Driver.RequiresArtworkKey));
            Assert.Equal("supported", driver.GetStringFieldValue(Data.Models.Metadata.Driver.SaveStateKey));
            Assert.Equal("good", driver.GetStringFieldValue(Data.Models.Metadata.Driver.SoundKey));
            Assert.Equal("good", driver.GetStringFieldValue(Data.Models.Metadata.Driver.StatusKey));
            Assert.True(driver.GetBoolFieldValue(Data.Models.Metadata.Driver.UnofficialKey));
        }

        private static void ValidateExtension(Extension? extension)
        {
            Assert.NotNull(extension);
            Assert.Equal("name", extension.GetStringFieldValue(Data.Models.Metadata.Extension.NameKey));
        }

        private static void ValidateFeature(Feature? feature)
        {
            Assert.NotNull(feature);
            Assert.Equal("name", feature.GetStringFieldValue(Data.Models.Metadata.Feature.NameKey));
            Assert.Equal("imperfect", feature.GetStringFieldValue(Data.Models.Metadata.Feature.OverallKey));
            Assert.Equal("imperfect", feature.GetStringFieldValue(Data.Models.Metadata.Feature.StatusKey));
            Assert.Equal("protection", feature.GetStringFieldValue(Data.Models.Metadata.Feature.FeatureTypeKey));
            Assert.Equal("value", feature.GetStringFieldValue(Data.Models.Metadata.Feature.ValueKey));
        }

        private static void ValidateInfo(Info? info)
        {
            Assert.NotNull(info);
            Assert.Equal("name", info.GetStringFieldValue(Data.Models.Metadata.Info.NameKey));
            Assert.Equal("value", info.GetStringFieldValue(Data.Models.Metadata.Info.ValueKey));
        }

        private static void ValidateInput(Input? input)
        {
            Assert.NotNull(input);
            Assert.Equal(12345L, input.GetInt64FieldValue(Data.Models.Metadata.Input.ButtonsKey));
            Assert.Equal(12345L, input.GetInt64FieldValue(Data.Models.Metadata.Input.CoinsKey));
            Assert.Equal(12345L, input.GetInt64FieldValue(Data.Models.Metadata.Input.PlayersKey));
            Assert.True(input.GetBoolFieldValue(Data.Models.Metadata.Input.ServiceKey));
            Assert.True(input.GetBoolFieldValue(Data.Models.Metadata.Input.TiltKey));

            Control[]? controls = input.GetFieldValue<Control[]>(Data.Models.Metadata.Input.ControlKey);
            Assert.NotNull(controls);
            Control? control = Assert.Single(controls);
            ValidateControl(control);
        }

        private static void ValidateInstance(Instance? instance)
        {
            Assert.NotNull(instance);
            Assert.Equal("briefname", instance.GetStringFieldValue(Data.Models.Metadata.Instance.BriefNameKey));
            Assert.Equal("name", instance.GetStringFieldValue(Data.Models.Metadata.Instance.NameKey));
        }

        private static void ValidateMedia(Media? media)
        {
            Assert.NotNull(media);
            Assert.Equal(ZeroHash.MD5Str, media.GetStringFieldValue(Data.Models.Metadata.Media.MD5Key));
            Assert.Equal("name", media.GetStringFieldValue(Data.Models.Metadata.Media.NameKey));
            Assert.Equal(ZeroHash.SHA1Str, media.GetStringFieldValue(Data.Models.Metadata.Media.SHA1Key));
            Assert.Equal(ZeroHash.SHA256Str, media.GetStringFieldValue(Data.Models.Metadata.Media.SHA256Key));
            Assert.Equal(ZeroHash.SpamSumStr, media.GetStringFieldValue(Data.Models.Metadata.Media.SpamSumKey));
        }

        private static void ValidatePart(Part? part)
        {
            Assert.NotNull(part);
            Assert.Equal("interface", part.GetStringFieldValue(Data.Models.Metadata.Part.InterfaceKey));
            Assert.Equal("name", part.GetStringFieldValue(Data.Models.Metadata.Part.NameKey));
        }

        private static void ValidatePartFeature(PartFeature? partFeature)
        {
            Assert.NotNull(partFeature);
            Assert.Equal("name", partFeature.GetStringFieldValue(Data.Models.Metadata.Feature.NameKey));
            Assert.Equal("imperfect", partFeature.GetStringFieldValue(Data.Models.Metadata.Feature.OverallKey));
            Assert.Equal("imperfect", partFeature.GetStringFieldValue(Data.Models.Metadata.Feature.StatusKey));
            Assert.Equal("protection", partFeature.GetStringFieldValue(Data.Models.Metadata.Feature.FeatureTypeKey));
            Assert.Equal("value", partFeature.GetStringFieldValue(Data.Models.Metadata.Feature.ValueKey));

            Part? part = partFeature.GetFieldValue<Part>(PartFeature.PartKey);
            ValidatePart(part);
        }

        private static void ValidatePort(Port? port)
        {
            Assert.NotNull(port);
            Assert.Equal("tag", port.GetStringFieldValue(Data.Models.Metadata.Port.TagKey));

            Analog[]? dipValues = port.GetFieldValue<Analog[]>(Data.Models.Metadata.Port.AnalogKey);
            Assert.NotNull(dipValues);
            Analog? dipValue = Assert.Single(dipValues);
            ValidateAnalog(dipValue);
        }

        private static void ValidateRamOption(RamOption? ramOption)
        {
            Assert.NotNull(ramOption);
            Assert.Equal("content", ramOption.GetStringFieldValue(Data.Models.Metadata.RamOption.ContentKey));
            Assert.True(ramOption.GetBoolFieldValue(Data.Models.Metadata.RamOption.DefaultKey));
            Assert.Equal("name", ramOption.GetStringFieldValue(Data.Models.Metadata.RamOption.NameKey));
        }

        private static void ValidateRelease(Release? release)
        {
            Assert.NotNull(release);
            Assert.Equal("date", release.GetStringFieldValue(Data.Models.Metadata.Release.DateKey));
            Assert.True(release.GetBoolFieldValue(Data.Models.Metadata.Release.DefaultKey));
            Assert.Equal("language", release.GetStringFieldValue(Data.Models.Metadata.Release.LanguageKey));
            Assert.Equal("name", release.GetStringFieldValue(Data.Models.Metadata.Release.NameKey));
            Assert.Equal("region", release.GetStringFieldValue(Data.Models.Metadata.Release.RegionKey));
        }

        private static void ValidateRom(Rom? rom)
        {
            Assert.NotNull(rom);
            Assert.Equal("album", rom.GetStringFieldValue(Data.Models.Metadata.Rom.AlbumKey));
            Assert.Equal("alt_romname", rom.GetStringFieldValue(Data.Models.Metadata.Rom.AltRomnameKey));
            Assert.Equal("alt_title", rom.GetStringFieldValue(Data.Models.Metadata.Rom.AltTitleKey));
            Assert.Equal("artist", rom.GetStringFieldValue(Data.Models.Metadata.Rom.ArtistKey));
            Assert.Equal("asr_detected_lang", rom.GetStringFieldValue(Data.Models.Metadata.Rom.ASRDetectedLangKey));
            Assert.Equal("asr_detected_lang_conf", rom.GetStringFieldValue(Data.Models.Metadata.Rom.ASRDetectedLangConfKey));
            Assert.Equal("asr_transcribed_lang", rom.GetStringFieldValue(Data.Models.Metadata.Rom.ASRTranscribedLangKey));
            Assert.Equal("bios", rom.GetStringFieldValue(Data.Models.Metadata.Rom.BiosKey));
            Assert.Equal("bitrate", rom.GetStringFieldValue(Data.Models.Metadata.Rom.BitrateKey));
            Assert.Equal("btih", rom.GetStringFieldValue(Data.Models.Metadata.Rom.BitTorrentMagnetHashKey));
            Assert.Equal("cloth_cover_detection_module_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.ClothCoverDetectionModuleVersionKey));
            Assert.Equal("collection-catalog-number", rom.GetStringFieldValue(Data.Models.Metadata.Rom.CollectionCatalogNumberKey));
            Assert.Equal("comment", rom.GetStringFieldValue(Data.Models.Metadata.Rom.CommentKey));
            Assert.Equal(ZeroHash.CRC32Str, rom.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey));
            Assert.Equal("creator", rom.GetStringFieldValue(Data.Models.Metadata.Rom.CreatorKey));
            Assert.Equal("date", rom.GetStringFieldValue(Data.Models.Metadata.Rom.DateKey));
            Assert.True(rom.GetBoolFieldValue(Data.Models.Metadata.Rom.DisposeKey));
            Assert.Equal("extension", rom.GetStringFieldValue(Data.Models.Metadata.Rom.ExtensionKey));
            Assert.Equal(12345L, rom.GetInt64FieldValue(Data.Models.Metadata.Rom.FileCountKey));
            Assert.True(rom.GetBoolFieldValue(Data.Models.Metadata.Rom.FileIsAvailableKey));
            Assert.Equal("flags", rom.GetStringFieldValue(Data.Models.Metadata.Rom.FlagsKey));
            Assert.Equal("format", rom.GetStringFieldValue(Data.Models.Metadata.Rom.FormatKey));
            Assert.Equal("header", rom.GetStringFieldValue(Data.Models.Metadata.Rom.HeaderKey));
            Assert.Equal("height", rom.GetStringFieldValue(Data.Models.Metadata.Rom.HeightKey));
            Assert.Equal("hocr_char_to_word_hocr_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.hOCRCharToWordhOCRVersionKey));
            Assert.Equal("hocr_char_to_word_module_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.hOCRCharToWordModuleVersionKey));
            Assert.Equal("hocr_fts_text_hocr_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.hOCRFtsTexthOCRVersionKey));
            Assert.Equal("hocr_fts_text_module_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.hOCRFtsTextModuleVersionKey));
            Assert.Equal("hocr_pageindex_hocr_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.hOCRPageIndexhOCRVersionKey));
            Assert.Equal("hocr_pageindex_module_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.hOCRPageIndexModuleVersionKey));
            Assert.True(rom.GetBoolFieldValue(Data.Models.Metadata.Rom.InvertedKey));
            Assert.Equal("mtime", rom.GetStringFieldValue(Data.Models.Metadata.Rom.LastModifiedTimeKey));
            Assert.Equal("length", rom.GetStringFieldValue(Data.Models.Metadata.Rom.LengthKey));
            Assert.Equal("load16_byte", rom.GetStringFieldValue(Data.Models.Metadata.Rom.LoadFlagKey));
            Assert.Equal("matrix_number", rom.GetStringFieldValue(Data.Models.Metadata.Rom.MatrixNumberKey));
            Assert.Equal(ZeroHash.GetString(HashType.MD2), rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key));
            Assert.Equal(ZeroHash.GetString(HashType.MD4), rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key));
            Assert.Equal(ZeroHash.MD5Str, rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key));
            Assert.Null(rom.GetStringFieldValue(Data.Models.Metadata.Rom.OpenMSXMediaType)); // Omit due to other test
            Assert.Equal("merge", rom.GetStringFieldValue(Data.Models.Metadata.Rom.MergeKey));
            Assert.True(rom.GetBoolFieldValue(Data.Models.Metadata.Rom.MIAKey));
            Assert.Equal("name", rom.GetStringFieldValue(Data.Models.Metadata.Rom.NameKey));
            Assert.Equal("ocr", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TesseractOCRKey));
            Assert.Equal("ocr_converted", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TesseractOCRConvertedKey));
            Assert.Equal("ocr_detected_lang", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TesseractOCRDetectedLangKey));
            Assert.Equal("ocr_detected_lang_conf", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TesseractOCRDetectedLangConfKey));
            Assert.Equal("ocr_detected_script", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TesseractOCRDetectedScriptKey));
            Assert.Equal("ocr_detected_script_conf", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TesseractOCRDetectedScriptConfKey));
            Assert.Equal("ocr_module_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TesseractOCRModuleVersionKey));
            Assert.Equal("ocr_parameters", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TesseractOCRParametersKey));
            Assert.Equal("offset", rom.GetStringFieldValue(Data.Models.Metadata.Rom.OffsetKey));
            Assert.True(rom.GetBoolFieldValue(Data.Models.Metadata.Rom.OptionalKey));
            Assert.Equal("original", rom.GetStringFieldValue(Data.Models.Metadata.Rom.OriginalKey));
            Assert.Equal("pdf_module_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.PDFModuleVersionKey));
            Assert.Equal("preview-image", rom.GetStringFieldValue(Data.Models.Metadata.Rom.PreviewImageKey));
            Assert.Equal("publisher", rom.GetStringFieldValue(Data.Models.Metadata.Rom.PublisherKey));
            Assert.Equal("region", rom.GetStringFieldValue(Data.Models.Metadata.Rom.RegionKey));
            Assert.Equal("remark", rom.GetStringFieldValue(Data.Models.Metadata.Rom.RemarkKey));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD128), rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key));
            Assert.Equal(ZeroHash.GetString(HashType.RIPEMD160), rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key));
            Assert.Equal("rotation", rom.GetStringFieldValue(Data.Models.Metadata.Rom.RotationKey));
            Assert.Equal("serial", rom.GetStringFieldValue(Data.Models.Metadata.Rom.SerialKey));
            Assert.Equal(ZeroHash.SHA1Str, rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal(ZeroHash.SHA256Str, rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key));
            Assert.Equal(ZeroHash.SHA384Str, rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key));
            Assert.Equal(ZeroHash.SHA512Str, rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key));
            Assert.Equal(12345L, rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey));
            Assert.True(rom.GetBoolFieldValue(Data.Models.Metadata.Rom.SoundOnlyKey));
            Assert.Equal("source", rom.GetStringFieldValue(Data.Models.Metadata.Rom.SourceKey));
            Assert.Equal(ZeroHash.SpamSumStr, rom.GetStringFieldValue(Data.Models.Metadata.Rom.SpamSumKey));
            Assert.Equal("start", rom.GetStringFieldValue(Data.Models.Metadata.Rom.StartKey));
            Assert.Equal("good", rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey));
            Assert.Equal("summation", rom.GetStringFieldValue(Data.Models.Metadata.Rom.SummationKey));
            Assert.Equal("title", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TitleKey));
            Assert.Equal("track", rom.GetStringFieldValue(Data.Models.Metadata.Rom.TrackKey));
            Assert.Equal("type", rom.GetStringFieldValue(Data.Models.Metadata.Rom.OpenMSXType));
            Assert.Equal("value", rom.GetStringFieldValue(Data.Models.Metadata.Rom.ValueKey));
            Assert.Equal("whisper_asr_module_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WhisperASRModuleVersionKey));
            Assert.Equal("whisper_model_hash", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WhisperModelHashKey));
            Assert.Equal("whisper_model_name", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WhisperModelNameKey));
            Assert.Equal("whisper_version", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WhisperVersionKey));
            Assert.Equal("width", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WidthKey));
            Assert.Equal("word_conf_0_10", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval0To10Key));
            Assert.Equal("word_conf_11_20", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval11To20Key));
            Assert.Equal("word_conf_21_30", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval21To30Key));
            Assert.Equal("word_conf_31_40", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval31To40Key));
            Assert.Equal("word_conf_41_50", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval41To50Key));
            Assert.Equal("word_conf_51_60", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval51To60Key));
            Assert.Equal("word_conf_61_70", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval61To70Key));
            Assert.Equal("word_conf_71_80", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval71To80Key));
            Assert.Equal("word_conf_81_90", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval81To90Key));
            Assert.Equal("word_conf_91_100", rom.GetStringFieldValue(Data.Models.Metadata.Rom.WordConfidenceInterval91To100Key));
            Assert.Equal(ZeroHash.GetString(HashType.XxHash3), rom.GetStringFieldValue(Data.Models.Metadata.Rom.xxHash364Key));
            Assert.Equal(ZeroHash.GetString(HashType.XxHash128), rom.GetStringFieldValue(Data.Models.Metadata.Rom.xxHash3128Key));
        }

        private static void ValidateSample(Sample? sample)
        {
            Assert.NotNull(sample);
            Assert.Equal("name", sample.GetStringFieldValue(Data.Models.Metadata.Sample.NameKey));
        }

        private static void ValidateSharedFeat(SharedFeat? sharedFeat)
        {
            Assert.NotNull(sharedFeat);
            Assert.Equal("name", sharedFeat.GetStringFieldValue(Data.Models.Metadata.SharedFeat.NameKey));
            Assert.Equal("value", sharedFeat.GetStringFieldValue(Data.Models.Metadata.SharedFeat.ValueKey));
        }

        private static void ValidateSlot(Slot? slot)
        {
            Assert.NotNull(slot);
            Assert.Equal("name", slot.GetStringFieldValue(Data.Models.Metadata.Slot.NameKey));

            SlotOption[]? slotOptions = slot.GetFieldValue<SlotOption[]>(Data.Models.Metadata.Slot.SlotOptionKey);
            Assert.NotNull(slotOptions);
            SlotOption? slotOption = Assert.Single(slotOptions);
            ValidateSlotOption(slotOption);
        }

        private static void ValidateSlotOption(SlotOption? slotOption)
        {
            Assert.NotNull(slotOption);
            Assert.True(slotOption.GetBoolFieldValue(Data.Models.Metadata.SlotOption.DefaultKey));
            Assert.Equal("devname", slotOption.GetStringFieldValue(Data.Models.Metadata.SlotOption.DevNameKey));
            Assert.Equal("name", slotOption.GetStringFieldValue(Data.Models.Metadata.SlotOption.NameKey));
        }

        private static void ValidateSoftwareList(SoftwareList? softwareList)
        {
            Assert.NotNull(softwareList);
            Assert.Equal("description", softwareList.GetStringFieldValue(Data.Models.Metadata.SoftwareList.DescriptionKey));
            Assert.Equal("filter", softwareList.GetStringFieldValue(Data.Models.Metadata.SoftwareList.FilterKey));
            Assert.Equal("name", softwareList.GetStringFieldValue(Data.Models.Metadata.SoftwareList.NameKey));
            Assert.Equal("notes", softwareList.GetStringFieldValue(Data.Models.Metadata.SoftwareList.NotesKey));
            // TODO: Figure out why Data.Models.Metadata.SoftwareList.SoftwareKey doesn't get processed
            Assert.Equal("original", softwareList.GetStringFieldValue(Data.Models.Metadata.SoftwareList.StatusKey));
            Assert.Equal("tag", softwareList.GetStringFieldValue(Data.Models.Metadata.SoftwareList.TagKey));
        }

        private static void ValidateSound(Sound? sound)
        {
            Assert.NotNull(sound);
            Assert.Equal(12345L, sound.GetInt64FieldValue(Data.Models.Metadata.Sound.ChannelsKey));
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
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Video.AspectXKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Video.AspectYKey));
            Assert.Equal("vector", display.GetStringFieldValue(Data.Models.Metadata.Display.DisplayTypeKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.HeightKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.RefreshKey));
            Assert.Equal(12345L, display.GetInt64FieldValue(Data.Models.Metadata.Display.WidthKey));
            Assert.Equal(90, display.GetInt64FieldValue(Data.Models.Metadata.Display.RotateKey));
        }

        #endregion
    }
}