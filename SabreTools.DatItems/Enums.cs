using System;
using SabreTools.Core;

namespace SabreTools.DatItems
{
    /// <summary>
    /// Determine the chip type
    /// </summary>
    [Flags]
    public enum ChipType
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("cpu")]
        CPU = 1 << 0,

        [Mapping("audio")]
        Audio = 1 << 1,
    }

    /// <summary>
    /// Determine the control type
    /// </summary>
    [Flags]
    public enum ControlType
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("joy")]
        Joy = 1 << 0,

        [Mapping("stick")]
        Stick = 1 << 1,

        [Mapping("paddle")]
        Paddle = 1 << 2,

        [Mapping("pedal")]
        Pedal = 1 << 3,

        [Mapping("lightgun")]
        Lightgun = 1 << 4,

        [Mapping("positional")]
        Positional = 1 << 5,

        [Mapping("dial")]
        Dial = 1 << 6,

        [Mapping("trackball")]
        Trackball = 1 << 7,

        [Mapping("mouse")]
        Mouse = 1 << 8,

        [Mapping("only_buttons")]
        OnlyButtons = 1 << 9,

        [Mapping("keypad")]
        Keypad = 1 << 10,

        [Mapping("keyboard")]
        Keyboard = 1 << 11,

        [Mapping("mahjong")]
        Mahjong = 1 << 12,

        [Mapping("hanafuda")]
        Hanafuda = 1 << 13,

        [Mapping("gambling")]
        Gambling = 1 << 14,
    }

    /// <summary>
    /// Determine the device type
    /// </summary>
    [Flags]
    public enum DeviceType
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("unknown")]
        Unknown = 1 << 0,

        [Mapping("cartridge")]
        Cartridge = 1 << 1,

        [Mapping("floppydisk")]
        FloppyDisk = 1 << 2,

        [Mapping("harddisk")]
        HardDisk = 1 << 3,

        [Mapping("cylinder")]
        Cylinder = 1 << 4,

        [Mapping("cassette")]
        Cassette = 1 << 5,

        [Mapping("punchcard")]
        PunchCard = 1 << 6,

        [Mapping("punchtape")]
        PunchTape = 1 << 7,

        [Mapping("printout")]
        Printout = 1 << 8,

        [Mapping("serial")]
        Serial = 1 << 9,

        [Mapping("parallel")]
        Parallel = 1 << 10,

        [Mapping("snapshot")]
        Snapshot = 1 << 11,

        [Mapping("quickload")]
        QuickLoad = 1 << 12,

        [Mapping("memcard")]
        MemCard = 1 << 13,

        [Mapping("cdrom")]
        CDROM = 1 << 14,

        [Mapping("magtape")]
        MagTape = 1 << 15,

        [Mapping("romimage")]
        ROMImage = 1 << 16,

        [Mapping("midiin")]
        MIDIIn = 1 << 17,

        [Mapping("midiout")]
        MIDIOut = 1 << 18,

        [Mapping("picture")]
        Picture = 1 << 19,

        [Mapping("vidfile")]
        VidFile = 1 << 20,
    }

    /// <summary>
    /// Determine the display type
    /// </summary>
    [Flags]
    public enum DisplayType
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("raster")]
        Raster = 1 << 0,

        [Mapping("vector")]
        Vector = 1 << 1,

        [Mapping("lcd")]
        LCD = 1 << 2,

        [Mapping("svg")]
        SVG = 1 << 3,

        [Mapping("unknown")]
        Unknown = 1 << 4,
    }

    /// <summary>
    /// Determines which type of duplicate a file is
    /// </summary>
    [Flags]
    public enum DupeType
    {
        // Type of match
        Hash = 1 << 0,
        All = 1 << 1,

        // Location of match
        Internal = 1 << 2,
        External = 1 << 3,
    }

    /// <summary>
    /// Determine the endianness
    /// </summary>
    [Flags]
    public enum Endianness
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("big")]
        Big = 1 << 0,

        [Mapping("little")]
        Little = 1 << 1,
    }

    /// <summary>
    /// Determine the emulation status
    /// </summary>
    [Flags]
    public enum FeatureStatus
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("unemulated")]
        Unemulated = 1 << 0,

        [Mapping("imperfect")]
        Imperfect = 1 << 1,
    }

    /// <summary>
    /// Determine the feature type
    /// </summary>
    [Flags]
    public enum FeatureType
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("protection")]
        Protection = 1 << 0,

        [Mapping("palette")]
        Palette = 1 << 1,

        [Mapping("graphics")]
        Graphics = 1 << 2,

        [Mapping("sound")]
        Sound = 1 << 3,

        [Mapping("controls")]
        Controls = 1 << 4,

        [Mapping("keyboard")]
        Keyboard = 1 << 5,

        [Mapping("mouse")]
        Mouse = 1 << 6,

        [Mapping("microphone")]
        Microphone = 1 << 7,

        [Mapping("camera")]
        Camera = 1 << 8,

        [Mapping("disk")]
        Disk = 1 << 9,

        [Mapping("printer")]
        Printer = 1 << 10,

        [Mapping("lan")]
        Lan = 1 << 11,

        [Mapping("wan")]
        Wan = 1 << 12,

        [Mapping("timing")]
        Timing = 1 << 13,
    }

    /// <summary>
    /// A subset of fields that can be used as keys
    /// </summary>
    public enum ItemKey
    {
        NULL = 0,

        Machine,

        CRC,
        MD2,
        MD4,
        MD5,
        RIPEMD128,
        RIPEMD160,
        SHA1,
        SHA256,
        SHA384,
        SHA512,
        SpamSum,
    }

    /// <summary>
    /// Determine the status of the item
    /// </summary>
    [Flags]
    public enum ItemStatus
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("none", "no")]
        None = 1 << 0,

        [Mapping("good")]
        Good = 1 << 1,

        [Mapping("baddump")]
        BadDump = 1 << 2,

        [Mapping("nodump", "yes")]
        Nodump = 1 << 3,

        [Mapping("verified")]
        Verified = 1 << 4,
    }

    /// <summary>
    /// Determine what type of file an item is
    /// </summary>
    public enum ItemType
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        // "Actionable" item types

        [Mapping("rom")]
        Rom,

        [Mapping("disk")]
        Disk,

        [Mapping("file")]
        File,

        [Mapping("media")]
        Media,

        // "Auxiliary" item types

        [Mapping("adjuster")]
        Adjuster,

        [Mapping("analog")]
        Analog,

        [Mapping("archive")]
        Archive,

        [Mapping("biosset")]
        BiosSet,

        [Mapping("chip")]
        Chip,

        [Mapping("condition")]
        Condition,

        [Mapping("configuration")]
        Configuration,

        [Mapping("conflocation")]
        ConfLocation,

        [Mapping("confsetting")]
        ConfSetting,

        [Mapping("control")]
        Control,

        [Mapping("dataarea")]
        DataArea,

        [Mapping("device")]
        Device,

        [Mapping("device_ref", "deviceref")]
        DeviceRef,

        [Mapping("diplocation")]
        DipLocation,

        [Mapping("dipswitch")]
        DipSwitch,

        [Mapping("dipvalue")]
        DipValue,

        [Mapping("diskarea")]
        DiskArea,

        [Mapping("display")]
        Display,

        [Mapping("driver")]
        Driver,

        [Mapping("extension")]
        Extension,

        [Mapping("feature")]
        Feature,

        [Mapping("info")]
        Info,

        [Mapping("input")]
        Input,

        [Mapping("instance")]
        Instance,

        [Mapping("original")]
        Original,

        [Mapping("part")]
        Part,

        [Mapping("part_feature", "partfeature")]
        PartFeature,

        [Mapping("port")]
        Port,

        [Mapping("ramoption", "ram_option")]
        RamOption,

        [Mapping("release")]
        Release,

        [Mapping("release_details", "releasedetails")]
        ReleaseDetails,

        [Mapping("sample")]
        Sample,

        [Mapping("serials")]
        Serials,

        [Mapping("sharedfeat", "shared_feat", "sharedfeature", "shared_feature")]
        SharedFeat,

        [Mapping("slot")]
        Slot,

        [Mapping("slotoption", "slot_option")]
        SlotOption,

        [Mapping("softwarelist", "software_list")]
        SoftwareList,

        [Mapping("sound")]
        Sound,

        [Mapping("source_details", "sourcedetails")]
        SourceDetails,

        [Mapping("blank")]
        Blank = 99, // This is not a real type, only used internally
    }

    /// <summary>
    /// Determine the loadflag value
    /// </summary>
    [Flags]
    public enum LoadFlag
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("load16_byte")]
        Load16Byte = 1 << 0,

        [Mapping("load16_word")]
        Load16Word = 1 << 1,

        [Mapping("load16_word_swap")]
        Load16WordSwap = 1 << 2,

        [Mapping("load32_byte")]
        Load32Byte = 1 << 3,

        [Mapping("load32_word")]
        Load32Word = 1 << 4,

        [Mapping("load32_word_swap")]
        Load32WordSwap = 1 << 5,

        [Mapping("load32_dword")]
        Load32DWord = 1 << 6,

        [Mapping("load64_word")]
        Load64Word = 1 << 7,

        [Mapping("load64_word_swap")]
        Load64WordSwap = 1 << 8,

        [Mapping("reload")]
        Reload = 1 << 9,

        [Mapping("fill")]
        Fill = 1 << 10,

        [Mapping("continue")]
        Continue = 1 << 11,

        [Mapping("reload_plain")]
        ReloadPlain = 1 << 12,

        [Mapping("ignore")]
        Ignore = 1 << 13,
    }

    /// <summary>
    /// Determine what type of machine it is
    /// </summary>
    [Flags]
    public enum MachineType
    {
        [Mapping("none")]
        None = 0,

        [Mapping("bios")]
        Bios = 1 << 0,

        [Mapping("device", "dev")]
        Device = 1 << 1,

        [Mapping("mechanical", "mech")]
        Mechanical = 1 << 2,
    }

    /// <summary>
    /// Determine which OpenMSX subtype an item is
    /// </summary>
    [Flags]
    public enum OpenMSXSubType
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("rom")]
        Rom = 1 << 0,

        [Mapping("megarom")]
        MegaRom = 1 << 1,

        [Mapping("sccpluscart")]
        SCCPlusCart = 1 << 2,
    }

    /// <summary>
    /// Determine relation of value to condition
    /// </summary>
    [Flags]
    public enum Relation
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("eq")]
        Equal = 1 << 0,

        [Mapping("ne")]
        NotEqual = 1 << 1,

        [Mapping("gt")]
        GreaterThan = 1 << 2,

        [Mapping("le")]
        LessThanOrEqual = 1 << 3,

        [Mapping("lt")]
        LessThan = 1 << 4,

        [Mapping("ge")]
        GreaterThanOrEqual = 1 << 5,
    }

    /// <summary>
    /// Determine machine runnable status
    /// </summary>
    [Flags]
    public enum Runnable
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("no")]
        No = 1 << 0,

        [Mapping("partial")]
        Partial = 1 << 1,

        [Mapping("yes")]
        Yes = 1 << 2,
    }

    /// <summary>
    /// Determine software list status
    /// </summary>
    [Flags]
    public enum SoftwareListStatus
    {
        [Mapping("none")]
        None = 0,

        [Mapping("original")]
        Original = 1 << 0,

        [Mapping("compatible")]
        Compatible = 1 << 1,
    }

    /// <summary>
    /// Determine machine support status
    /// </summary>
    [Flags]
    public enum Supported
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("no", "unsupported")]
        No = 1 << 0,

        [Mapping("partial")]
        Partial = 1 << 1,

        [Mapping("yes", "supported")]
        Yes = 1 << 2,
    }

    /// <summary>
    /// Determine driver support statuses
    /// </summary>
    [Flags]
    public enum SupportStatus
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        [Mapping("good")]
        Good = 1 << 0,

        [Mapping("imperfect")]
        Imperfect = 1 << 1,

        [Mapping("preliminary")]
        Preliminary = 1 << 2,
    }
}