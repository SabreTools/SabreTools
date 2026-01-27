using Xunit;

namespace SabreTools.DatItems.Test
{
    public class ExtensionsTests
    {
        #region String to Enum

        [Theory]
        [InlineData(null, ChipType.NULL)]
        [InlineData("cpu", ChipType.CPU)]
        [InlineData("audio", ChipType.Audio)]
        public void AsChipTypeTest(string? field, ChipType expected)
        {
            ChipType actual = field.AsChipType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, ControlType.NULL)]
        [InlineData("joy", ControlType.Joy)]
        [InlineData("stick", ControlType.Stick)]
        [InlineData("paddle", ControlType.Paddle)]
        [InlineData("pedal", ControlType.Pedal)]
        [InlineData("lightgun", ControlType.Lightgun)]
        [InlineData("positional", ControlType.Positional)]
        [InlineData("dial", ControlType.Dial)]
        [InlineData("trackball", ControlType.Trackball)]
        [InlineData("mouse", ControlType.Mouse)]
        [InlineData("only_buttons", ControlType.OnlyButtons)]
        [InlineData("keypad", ControlType.Keypad)]
        [InlineData("keyboard", ControlType.Keyboard)]
        [InlineData("mahjong", ControlType.Mahjong)]
        [InlineData("hanafuda", ControlType.Hanafuda)]
        [InlineData("gambling", ControlType.Gambling)]
        public void AsControlTypeTest(string? field, ControlType expected)
        {
            ControlType actual = field.AsControlType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, DeviceType.NULL)]
        [InlineData("unknown", DeviceType.Unknown)]
        [InlineData("cartridge", DeviceType.Cartridge)]
        [InlineData("floppydisk", DeviceType.FloppyDisk)]
        [InlineData("harddisk", DeviceType.HardDisk)]
        [InlineData("cylinder", DeviceType.Cylinder)]
        [InlineData("cassette", DeviceType.Cassette)]
        [InlineData("punchcard", DeviceType.PunchCard)]
        [InlineData("punchtape", DeviceType.PunchTape)]
        [InlineData("printout", DeviceType.Printout)]
        [InlineData("serial", DeviceType.Serial)]
        [InlineData("parallel", DeviceType.Parallel)]
        [InlineData("snapshot", DeviceType.Snapshot)]
        [InlineData("quickload", DeviceType.QuickLoad)]
        [InlineData("memcard", DeviceType.MemCard)]
        [InlineData("cdrom", DeviceType.CDROM)]
        [InlineData("magtape", DeviceType.MagTape)]
        [InlineData("romimage", DeviceType.ROMImage)]
        [InlineData("midiin", DeviceType.MIDIIn)]
        [InlineData("midiout", DeviceType.MIDIOut)]
        [InlineData("picture", DeviceType.Picture)]
        [InlineData("vidfile", DeviceType.VidFile)]
        public void AsDeviceTypeTest(string? field, DeviceType expected)
        {
            DeviceType actual = field.AsDeviceType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, DisplayType.NULL)]
        [InlineData("raster", DisplayType.Raster)]
        [InlineData("vector", DisplayType.Vector)]
        [InlineData("lcd", DisplayType.LCD)]
        [InlineData("svg", DisplayType.SVG)]
        [InlineData("unknown", DisplayType.Unknown)]
        public void AsDisplayTypeTest(string? field, DisplayType expected)
        {
            DisplayType actual = field.AsDisplayType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, Endianness.NULL)]
        [InlineData("big", Endianness.Big)]
        [InlineData("little", Endianness.Little)]
        public void AsEndiannessTest(string? field, Endianness expected)
        {
            Endianness actual = field.AsEndianness();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, FeatureStatus.NULL)]
        [InlineData("unemulated", FeatureStatus.Unemulated)]
        [InlineData("imperfect", FeatureStatus.Imperfect)]
        public void AsFeatureStatusTest(string? field, FeatureStatus expected)
        {
            FeatureStatus actual = field.AsFeatureStatus();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, FeatureType.NULL)]
        [InlineData("protection", FeatureType.Protection)]
        [InlineData("palette", FeatureType.Palette)]
        [InlineData("graphics", FeatureType.Graphics)]
        [InlineData("sound", FeatureType.Sound)]
        [InlineData("controls", FeatureType.Controls)]
        [InlineData("keyboard", FeatureType.Keyboard)]
        [InlineData("mouse", FeatureType.Mouse)]
        [InlineData("microphone", FeatureType.Microphone)]
        [InlineData("camera", FeatureType.Camera)]
        [InlineData("disk", FeatureType.Disk)]
        [InlineData("printer", FeatureType.Printer)]
        [InlineData("lan", FeatureType.Lan)]
        [InlineData("wan", FeatureType.Wan)]
        [InlineData("timing", FeatureType.Timing)]
        public void AsFeatureTypeTest(string? field, FeatureType expected)
        {
            FeatureType actual = field.AsFeatureType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, ItemStatus.NULL)]
        [InlineData("none", ItemStatus.None)]
        [InlineData("no", ItemStatus.None)]
        [InlineData("good", ItemStatus.Good)]
        [InlineData("baddump", ItemStatus.BadDump)]
        [InlineData("nodump", ItemStatus.Nodump)]
        [InlineData("yes", ItemStatus.Nodump)]
        [InlineData("verified", ItemStatus.Verified)]
        public void AsItemStatusTest(string? field, ItemStatus expected)
        {
            ItemStatus actual = field.AsItemStatus();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, ItemType.NULL)]
        [InlineData("adjuster", ItemType.Adjuster)]
        [InlineData("analog", ItemType.Analog)]
        [InlineData("archive", ItemType.Archive)]
        [InlineData("biosset", ItemType.BiosSet)]
        [InlineData("blank", ItemType.Blank)]
        [InlineData("chip", ItemType.Chip)]
        [InlineData("condition", ItemType.Condition)]
        [InlineData("configuration", ItemType.Configuration)]
        [InlineData("conflocation", ItemType.ConfLocation)]
        [InlineData("confsetting", ItemType.ConfSetting)]
        [InlineData("control", ItemType.Control)]
        [InlineData("dataarea", ItemType.DataArea)]
        [InlineData("device", ItemType.Device)]
        [InlineData("deviceref", ItemType.DeviceRef)]
        [InlineData("device_ref", ItemType.DeviceRef)]
        [InlineData("diplocation", ItemType.DipLocation)]
        [InlineData("dipswitch", ItemType.DipSwitch)]
        [InlineData("dipvalue", ItemType.DipValue)]
        [InlineData("disk", ItemType.Disk)]
        [InlineData("diskarea", ItemType.DiskArea)]
        [InlineData("display", ItemType.Display)]
        [InlineData("driver", ItemType.Driver)]
        [InlineData("extension", ItemType.Extension)]
        [InlineData("feature", ItemType.Feature)]
        [InlineData("file", ItemType.File)]
        [InlineData("info", ItemType.Info)]
        [InlineData("input", ItemType.Input)]
        [InlineData("instance", ItemType.Instance)]
        [InlineData("media", ItemType.Media)]
        [InlineData("part", ItemType.Part)]
        [InlineData("partfeature", ItemType.PartFeature)]
        [InlineData("part_feature", ItemType.PartFeature)]
        [InlineData("port", ItemType.Port)]
        [InlineData("ramoption", ItemType.RamOption)]
        [InlineData("ram_option", ItemType.RamOption)]
        [InlineData("release", ItemType.Release)]
        [InlineData("releasedetails", ItemType.ReleaseDetails)]
        [InlineData("release_details", ItemType.ReleaseDetails)]
        [InlineData("rom", ItemType.Rom)]
        [InlineData("sample", ItemType.Sample)]
        [InlineData("serials", ItemType.Serials)]
        [InlineData("sharedfeat", ItemType.SharedFeat)]
        [InlineData("shared_feat", ItemType.SharedFeat)]
        [InlineData("sharedfeature", ItemType.SharedFeat)]
        [InlineData("shared_feature", ItemType.SharedFeat)]
        [InlineData("slot", ItemType.Slot)]
        [InlineData("slotoption", ItemType.SlotOption)]
        [InlineData("slot_option", ItemType.SlotOption)]
        [InlineData("softwarelist", ItemType.SoftwareList)]
        [InlineData("software_list", ItemType.SoftwareList)]
        [InlineData("sound", ItemType.Sound)]
        [InlineData("sourcedetails", ItemType.SourceDetails)]
        [InlineData("source_details", ItemType.SourceDetails)]
        public void AsItemTypeTest(string? field, ItemType expected)
        {
            ItemType actual = field.AsItemType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, LoadFlag.NULL)]
        [InlineData("load16_byte", LoadFlag.Load16Byte)]
        [InlineData("load16_word", LoadFlag.Load16Word)]
        [InlineData("load16_word_swap", LoadFlag.Load16WordSwap)]
        [InlineData("load32_byte", LoadFlag.Load32Byte)]
        [InlineData("load32_word", LoadFlag.Load32Word)]
        [InlineData("load32_word_swap", LoadFlag.Load32WordSwap)]
        [InlineData("load32_dword", LoadFlag.Load32DWord)]
        [InlineData("load64_word", LoadFlag.Load64Word)]
        [InlineData("load64_word_swap", LoadFlag.Load64WordSwap)]
        [InlineData("reload", LoadFlag.Reload)]
        [InlineData("fill", LoadFlag.Fill)]
        [InlineData("continue", LoadFlag.Continue)]
        [InlineData("reload_plain", LoadFlag.ReloadPlain)]
        [InlineData("ignore", LoadFlag.Ignore)]
        public void AsLoadFlagTest(string? field, LoadFlag expected)
        {
            LoadFlag actual = field.AsLoadFlag();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, MachineType.None)]
        [InlineData("none", MachineType.None)]
        [InlineData("bios", MachineType.Bios)]
        [InlineData("dev", MachineType.Device)]
        [InlineData("device", MachineType.Device)]
        [InlineData("mech", MachineType.Mechanical)]
        [InlineData("mechanical", MachineType.Mechanical)]
        public void AsMachineTypeTest(string? field, MachineType expected)
        {
            MachineType actual = field.AsMachineType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, OpenMSXSubType.NULL)]
        [InlineData("rom", OpenMSXSubType.Rom)]
        [InlineData("megarom", OpenMSXSubType.MegaRom)]
        [InlineData("sccpluscart", OpenMSXSubType.SCCPlusCart)]
        public void AsOpenMSXSubTypeTest(string? field, OpenMSXSubType expected)
        {
            OpenMSXSubType actual = field.AsOpenMSXSubType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, Relation.NULL)]
        [InlineData("eq", Relation.Equal)]
        [InlineData("ne", Relation.NotEqual)]
        [InlineData("gt", Relation.GreaterThan)]
        [InlineData("le", Relation.LessThanOrEqual)]
        [InlineData("lt", Relation.LessThan)]
        [InlineData("ge", Relation.GreaterThanOrEqual)]
        public void AsRelationTest(string? field, Relation expected)
        {
            Relation actual = field.AsRelation();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, Runnable.NULL)]
        [InlineData("no", Runnable.No)]
        [InlineData("partial", Runnable.Partial)]
        [InlineData("yes", Runnable.Yes)]
        public void AsRunnableTest(string? field, Runnable expected)
        {
            Runnable actual = field.AsRunnable();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, SoftwareListStatus.None)]
        [InlineData("none", SoftwareListStatus.None)]
        [InlineData("original", SoftwareListStatus.Original)]
        [InlineData("compatible", SoftwareListStatus.Compatible)]
        public void AsSoftwareListStatusTest(string? field, SoftwareListStatus expected)
        {
            SoftwareListStatus actual = field.AsSoftwareListStatus();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, Supported.NULL)]
        [InlineData("no", Supported.No)]
        [InlineData("unsupported", Supported.No)]
        [InlineData("partial", Supported.Partial)]
        [InlineData("yes", Supported.Yes)]
        [InlineData("supported", Supported.Yes)]
        public void AsSupportedTest(string? field, Supported expected)
        {
            Supported actual = field.AsSupported();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, SupportStatus.NULL)]
        [InlineData("good", SupportStatus.Good)]
        [InlineData("imperfect", SupportStatus.Imperfect)]
        [InlineData("preliminary", SupportStatus.Preliminary)]
        public void AsSupportStatusTest(string? field, SupportStatus expected)
        {
            SupportStatus actual = field.AsSupportStatus();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Enum to String

        [Theory]
        [InlineData(ChipType.NULL, null)]
        [InlineData(ChipType.CPU, "cpu")]
        [InlineData(ChipType.Audio, "audio")]
        public void FromChipTypeTest(ChipType field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(ControlType.NULL, null)]
        [InlineData(ControlType.Joy, "joy")]
        [InlineData(ControlType.Stick, "stick")]
        [InlineData(ControlType.Paddle, "paddle")]
        [InlineData(ControlType.Pedal, "pedal")]
        [InlineData(ControlType.Lightgun, "lightgun")]
        [InlineData(ControlType.Positional, "positional")]
        [InlineData(ControlType.Dial, "dial")]
        [InlineData(ControlType.Trackball, "trackball")]
        [InlineData(ControlType.Mouse, "mouse")]
        [InlineData(ControlType.OnlyButtons, "only_buttons")]
        [InlineData(ControlType.Keypad, "keypad")]
        [InlineData(ControlType.Keyboard, "keyboard")]
        [InlineData(ControlType.Mahjong, "mahjong")]
        [InlineData(ControlType.Hanafuda, "hanafuda")]
        [InlineData(ControlType.Gambling, "gambling")]
        public void FromControlTypeTest(ControlType field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(DeviceType.NULL, null)]
        [InlineData(DeviceType.Unknown, "unknown")]
        [InlineData(DeviceType.Cartridge, "cartridge")]
        [InlineData(DeviceType.FloppyDisk, "floppydisk")]
        [InlineData(DeviceType.HardDisk, "harddisk")]
        [InlineData(DeviceType.Cylinder, "cylinder")]
        [InlineData(DeviceType.Cassette, "cassette")]
        [InlineData(DeviceType.PunchCard, "punchcard")]
        [InlineData(DeviceType.PunchTape, "punchtape")]
        [InlineData(DeviceType.Printout, "printout")]
        [InlineData(DeviceType.Serial, "serial")]
        [InlineData(DeviceType.Parallel, "parallel")]
        [InlineData(DeviceType.Snapshot, "snapshot")]
        [InlineData(DeviceType.QuickLoad, "quickload")]
        [InlineData(DeviceType.MemCard, "memcard")]
        [InlineData(DeviceType.CDROM, "cdrom")]
        [InlineData(DeviceType.MagTape, "magtape")]
        [InlineData(DeviceType.ROMImage, "romimage")]
        [InlineData(DeviceType.MIDIIn, "midiin")]
        [InlineData(DeviceType.MIDIOut, "midiout")]
        [InlineData(DeviceType.Picture, "picture")]
        [InlineData(DeviceType.VidFile, "vidfile")]
        public void FromDeviceTypeTest(DeviceType field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(DisplayType.NULL, null)]
        [InlineData(DisplayType.Raster, "raster")]
        [InlineData(DisplayType.Vector, "vector")]
        [InlineData(DisplayType.LCD, "lcd")]
        [InlineData(DisplayType.SVG, "svg")]
        [InlineData(DisplayType.Unknown, "unknown")]
        public void FromDisplayTypeTest(DisplayType field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Endianness.NULL, null)]
        [InlineData(Endianness.Big, "big")]
        [InlineData(Endianness.Little, "little")]
        public void FromEndiannessTest(Endianness field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(FeatureStatus.NULL, null)]
        [InlineData(FeatureStatus.Unemulated, "unemulated")]
        [InlineData(FeatureStatus.Imperfect, "imperfect")]
        public void FromFeatureStatusTest(FeatureStatus field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(FeatureType.NULL, null)]
        [InlineData(FeatureType.Protection, "protection")]
        [InlineData(FeatureType.Palette, "palette")]
        [InlineData(FeatureType.Graphics, "graphics")]
        [InlineData(FeatureType.Sound, "sound")]
        [InlineData(FeatureType.Controls, "controls")]
        [InlineData(FeatureType.Keyboard, "keyboard")]
        [InlineData(FeatureType.Mouse, "mouse")]
        [InlineData(FeatureType.Microphone, "microphone")]
        [InlineData(FeatureType.Camera, "camera")]
        [InlineData(FeatureType.Disk, "disk")]
        [InlineData(FeatureType.Printer, "printer")]
        [InlineData(FeatureType.Lan, "lan")]
        [InlineData(FeatureType.Wan, "wan")]
        [InlineData(FeatureType.Timing, "timing")]
        public void FromFeatureTypeTest(FeatureType field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(ItemStatus.NULL, null)]
        [InlineData(ItemStatus.None, "none")]
        [InlineData(ItemStatus.Good, "good")]
        [InlineData(ItemStatus.BadDump, "baddump")]
        [InlineData(ItemStatus.Nodump, "nodump")]
        [InlineData(ItemStatus.Verified, "verified")]
        public void FromItemStatusTest(ItemStatus field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(ItemType.NULL, null)]
        [InlineData(ItemType.Adjuster, "adjuster")]
        [InlineData(ItemType.Analog, "analog")]
        [InlineData(ItemType.Archive, "archive")]
        [InlineData(ItemType.BiosSet, "biosset")]
        [InlineData(ItemType.Blank, "blank")]
        [InlineData(ItemType.Chip, "chip")]
        [InlineData(ItemType.Condition, "condition")]
        [InlineData(ItemType.Configuration, "configuration")]
        [InlineData(ItemType.ConfLocation, "conflocation")]
        [InlineData(ItemType.ConfSetting, "confsetting")]
        [InlineData(ItemType.Control, "control")]
        [InlineData(ItemType.DataArea, "dataarea")]
        [InlineData(ItemType.Device, "device")]
        [InlineData(ItemType.DeviceRef, "device_ref")]
        [InlineData(ItemType.DipLocation, "diplocation")]
        [InlineData(ItemType.DipSwitch, "dipswitch")]
        [InlineData(ItemType.DipValue, "dipvalue")]
        [InlineData(ItemType.Disk, "disk")]
        [InlineData(ItemType.DiskArea, "diskarea")]
        [InlineData(ItemType.Display, "display")]
        [InlineData(ItemType.Driver, "driver")]
        [InlineData(ItemType.Extension, "extension")]
        [InlineData(ItemType.Feature, "feature")]
        [InlineData(ItemType.File, "file")]
        [InlineData(ItemType.Info, "info")]
        [InlineData(ItemType.Input, "input")]
        [InlineData(ItemType.Instance, "instance")]
        [InlineData(ItemType.Media, "media")]
        [InlineData(ItemType.Part, "part")]
        [InlineData(ItemType.PartFeature, "part_feature")]
        [InlineData(ItemType.Port, "port")]
        [InlineData(ItemType.RamOption, "ramoption")]
        [InlineData(ItemType.Release, "release")]
        [InlineData(ItemType.ReleaseDetails, "release_details")]
        [InlineData(ItemType.Rom, "rom")]
        [InlineData(ItemType.Sample, "sample")]
        [InlineData(ItemType.Serials, "serials")]
        [InlineData(ItemType.SharedFeat, "sharedfeat")]
        [InlineData(ItemType.Slot, "slot")]
        [InlineData(ItemType.SlotOption, "slotoption")]
        [InlineData(ItemType.SoftwareList, "softwarelist")]
        [InlineData(ItemType.Sound, "sound")]
        [InlineData(ItemType.SourceDetails, "source_details")]
        public void FromItemTypeTest(ItemType field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(LoadFlag.NULL, null)]
        [InlineData(LoadFlag.Load16Byte, "load16_byte")]
        [InlineData(LoadFlag.Load16Word, "load16_word")]
        [InlineData(LoadFlag.Load16WordSwap, "load16_word_swap")]
        [InlineData(LoadFlag.Load32Byte, "load32_byte")]
        [InlineData(LoadFlag.Load32Word, "load32_word")]
        [InlineData(LoadFlag.Load32WordSwap, "load32_word_swap")]
        [InlineData(LoadFlag.Load32DWord, "load32_dword")]
        [InlineData(LoadFlag.Load64Word, "load64_word")]
        [InlineData(LoadFlag.Load64WordSwap, "load64_word_swap")]
        [InlineData(LoadFlag.Reload, "reload")]
        [InlineData(LoadFlag.Fill, "fill")]
        [InlineData(LoadFlag.Continue, "continue")]
        [InlineData(LoadFlag.ReloadPlain, "reload_plain")]
        [InlineData(LoadFlag.Ignore, "ignore")]
        public void FromLoadFlagTest(LoadFlag field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(OpenMSXSubType.NULL, null)]
        [InlineData(OpenMSXSubType.Rom, "rom")]
        [InlineData(OpenMSXSubType.MegaRom, "megarom")]
        [InlineData(OpenMSXSubType.SCCPlusCart, "sccpluscart")]
        public void FromOpenMSXSubTypeTest(OpenMSXSubType field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Relation.NULL, null)]
        [InlineData(Relation.Equal, "eq")]
        [InlineData(Relation.NotEqual, "ne")]
        [InlineData(Relation.GreaterThan, "gt")]
        [InlineData(Relation.LessThanOrEqual, "le")]
        [InlineData(Relation.LessThan, "lt")]
        [InlineData(Relation.GreaterThanOrEqual, "ge")]
        public void FromRelationTest(Relation field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Runnable.NULL, null)]
        [InlineData(Runnable.No, "no")]
        [InlineData(Runnable.Partial, "partial")]
        [InlineData(Runnable.Yes, "yes")]
        public void FromRunnableTest(Runnable field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(SoftwareListStatus.None, "none")]
        [InlineData(SoftwareListStatus.Original, "original")]
        [InlineData(SoftwareListStatus.Compatible, "compatible")]
        public void FromSoftwareListStatusTest(SoftwareListStatus field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Supported.NULL, true, null)]
        [InlineData(Supported.NULL, false, null)]
        [InlineData(Supported.No, true, "unsupported")]
        [InlineData(Supported.No, false, "no")]
        [InlineData(Supported.Partial, true, "partial")]
        [InlineData(Supported.Partial, false, "partial")]
        [InlineData(Supported.Yes, true, "supported")]
        [InlineData(Supported.Yes, false, "yes")]
        public void FromSupportedTest(Supported field, bool useSecond, string? expected)
        {
            string? actual = field.AsStringValue(useSecond);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(SupportStatus.NULL, null)]
        [InlineData(SupportStatus.Good, "good")]
        [InlineData(SupportStatus.Imperfect, "imperfect")]
        [InlineData(SupportStatus.Preliminary, "preliminary")]
        public void FromSupportStatusTest(SupportStatus field, string? expected)
        {
            string? actual = field.AsStringValue();
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
