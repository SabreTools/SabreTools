using System;

// TODO: Figure out the best way to put these back in their libraries
namespace SabreTools.Core
{
    #region DatFiles

    /// <summary>
    /// Available hashing types
    /// </summary>
    [Flags]
    public enum Hash
    {
        CRC = 1 << 0,
        MD5 = 1 << 1,
        SHA1 = 1 << 2,
        SHA256 = 1 << 3,
        SHA384 = 1 << 4,
        SHA512 = 1 << 5,
        SpamSum = 1 << 6,

        // Special combinations
        Standard = CRC | MD5 | SHA1,
        DeepHashes = SHA256 | SHA384 | SHA512 | SpamSum,
        SecureHashes = MD5 | SHA1 | SHA256 | SHA384 | SHA512 | SpamSum,
        All =  CRC | MD5 | SHA1 | SHA256 | SHA384 | SHA512 | SpamSum,
    }

    /// <summary>
    /// Determines merging tag handling for DAT output
    /// </summary>
    public enum MergingFlag
    {
        None = 0,
        Split,
        Merged,
        NonMerged,
        Full,

        /// <remarks>This is not usually defined for Merging flags</remarks>
        Device,
    }

    /// <summary>
    /// Determines nodump tag handling for DAT output
    /// </summary>
    public enum NodumpFlag
    {
        None = 0,
        Obsolete,
        Required,
        Ignore,
    }

    /// <summary>
    /// Determines packing tag handling for DAT output
    /// </summary>
    public enum PackingFlag
    {
        None = 0,

        /// <summary>
        /// Force all sets to be in archives, except disk and media
        /// </summary>
        Zip,

        /// <summary>
        /// Force all sets to be extracted into subfolders
        /// </summary>
        Unzip,

        /// <summary>
        /// Force sets with single items to be extracted to the parent folder
        /// </summary>
        Partial,

        /// <summary>
        /// Force all sets to be extracted to the parent folder
        /// </summary>
        Flat,
    }

    #endregion

    #region DatItems

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

        CPU = 1 << 0,
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

        Joy = 1 << 0,
        Stick = 1 << 1,
        Paddle = 1 << 2,
        Pedal = 1 << 3,
        Lightgun = 1 << 4,
        Positional = 1 << 5,
        Dial = 1 << 6,
        Trackball = 1 << 7,
        Mouse = 1 << 8,
        OnlyButtons = 1 << 9,
        Keypad = 1 << 10,
        Keyboard = 1 << 11,
        Mahjong = 1 << 12,
        Hanafuda = 1 << 13,
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

        Unknown = 1 << 0,
        Cartridge = 1 << 1,
        FloppyDisk = 1 << 2,
        HardDisk = 1 << 3,
        Cylinder = 1 << 4,
        Cassette = 1 << 5,
        PunchCard = 1 << 6,
        PunchTape = 1 << 7,
        Printout = 1 << 8,
        Serial = 1 << 9,
        Parallel = 1 << 10,
        Snapshot = 1 << 11,
        QuickLoad = 1 << 12,
        MemCard = 1 << 13,
        CDROM = 1 << 14,
        MagTape = 1 << 15,
        ROMImage = 1 << 16,
        MIDIIn = 1 << 17,
        MIDIOut = 1 << 18,
        Picture = 1 << 19,
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

        Raster = 1 << 0,
        Vector = 1 << 1,
        LCD = 1 << 2,
        SVG = 1 << 3,
        Unknown = 1 << 4,
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

        Big = 1 << 0,
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

        Unemulated = 1 << 0,
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

        Protection = 1 << 0,
        Palette = 1 << 1,
        Graphics = 1 << 2,
        Sound = 1 << 3,
        Controls = 1 << 4,
        Keyboard = 1 << 5,
        Mouse = 1 << 6,
        Microphone = 1 << 7,
        Camera = 1 << 8,
        Disk = 1 << 9,
        Printer = 1 << 10,
        Lan = 1 << 11,
        Wan = 1 << 12,
        Timing = 1 << 13,
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

        None = 1 << 0,
        Good = 1 << 1,
        BadDump = 1 << 2,
        Nodump = 1 << 3,
        Verified = 1 << 4,
    }

    /// <summary>
    /// Determine what type of file an item is
    /// </summary>
    public enum ItemType
    {
        // "Actionable" item types
        Rom,
        Disk,
        Media,

        // "Auxiliary" item types
        Adjuster,
        Analog,
        Archive,
        BiosSet,
        Chip,
        Condition,
        Configuration,
        Control,
        DataArea,
        Device,
        DeviceReference,
        DipSwitch,
        DiskArea,
        Display,
        Driver,
        Extension,
        Feature,
        Info,
        Input,
        Instance,
        Location,
        Part,
        PartFeature,
        Port,
        RamOption,
        Release,
        Sample,
        Setting,
        SharedFeature,
        Slot,
        SlotOption,
        SoftwareList,
        Sound,

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

        Load16Byte = 1 << 0,
        Load16Word = 1 << 1,
        Load16WordSwap = 1 << 2,
        Load32Byte = 1 << 3,
        Load32Word = 1 << 4,
        Load32WordSwap = 1 << 5,
        Load32DWord = 1 << 6,
        Load64Word = 1 << 7,
        Load64WordSwap = 1 << 8,
        Reload = 1 << 9,
        Fill = 1 << 10,
        Continue = 1 << 11,
        ReloadPlain = 1 << 12,
        Ignore = 1 << 13,
    }

    /// <summary>
    /// Determine what type of machine it is
    /// </summary>
    [Flags]
    public enum MachineType
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 1 << 0,

        Bios = 1 << 1,
        Device = 1 << 2,
        Mechanical = 1 << 3,
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

        Rom = 1 << 0,
        MegaRom = 1 << 1,
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

        Equal = 1 << 0,
        NotEqual = 1 << 1,
        GreaterThan = 1 << 2,
        LessThanOrEqual = 1 << 3,
        LessThan = 1 << 4,
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

        No = 1 << 0,
        Partial = 1 << 1,
        Yes = 1 << 2,
    }

    /// <summary>
    /// Determine software list status
    /// </summary>
    [Flags]
    public enum SoftwareListStatus
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0,

        Original = 1 << 0,
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

        No = 1 << 0,
        Partial = 1 << 1,
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

        Good = 1 << 0,
        Imperfect = 1 << 1,
        Preliminary = 1 << 2,
    }

    #endregion

    #region Fields

    /// <summary>
    /// List of valid field types within a DatHeader
    /// </summary>
    public enum DatHeaderField
    {
        NULL = 0,

        #region Common

        FileName,
        Name,
        Description,
        RootDir,
        Category,
        Version,
        Date,
        Author,
        Email,
        Homepage,
        Url,
        Comment,
        HeaderSkipper,
        Type,
        ForceMerging,
        ForceNodump,
        ForcePacking,

        #endregion

        #region ListXML

        Debug,
        MameConfig,

        #endregion

        #region Logiqx

        Build,
        RomMode,
        BiosMode,
        SampleMode,
        LockRomMode,
        LockBiosMode,
        LockSampleMode,

        #endregion

        #region OfflineList

        System,
        ScreenshotsWidth,
        ScreenshotsHeight,
        CanOpen,
        RomTitle,

        // Infos
        Info_Name,
        Info_Visible,
        Info_IsNamingOption,
        Info_Default,

        #endregion

        #region RomCenter

        RomCenterVersion,

        #endregion
    }

    /// <summary>
    /// List of valid field types within a DatItem
    /// </summary>
    public enum DatItemField
    {
        NULL = 0,

        #region Common

        Type,

        #endregion

        #region Item-Specific

        #region Actionable

        // Rom
        Name,
        Bios,
        Size,
        CRC,
        MD5,
        SHA1,
        SHA256,
        SHA384,
        SHA512,
        SpamSum,
        Merge,
        Region,
        Offset,
        Date,
        Status,
        Optional,
        Inverted,
        MIA,

        // Rom (Archive.org)
        ArchiveDotOrgSource,
        ArchiveDotOrgFormat,
        OriginalFilename,
        Rotation,
        Summation,

        // Rom (AttractMode)
        AltName,
        AltTitle,

        // Rom (OpenMSX)
        Original,
        OpenMSXSubType,
        OpenMSXType,
        Remark,
        Boot,

        // Rom (SoftwareList)
        LoadFlag,
        Value,

        // Disk
        Index,
        Writable,

        #endregion

        #region Auxiliary

        // Adjuster
        Default,

        // Analog
        Analog_Mask,

        // BiosSet
        Description,

        // Chip
        Tag,
        ChipType,
        Clock,

        // Condition
        Mask,
        Relation,
        Condition_Tag,
        Condition_Mask,
        Condition_Relation,
        Condition_Value,

        // Control
        Control_Type,
        Control_Player,
        Control_Buttons,
        Control_RequiredButtons,
        Control_Minimum,
        Control_Maximum,
        Control_Sensitivity,
        Control_KeyDelta,
        Control_Reverse,
        Control_Ways,
        Control_Ways2,
        Control_Ways3,

        // DataArea
        AreaName,
        AreaSize,
        AreaWidth,
        AreaEndianness,

        // Device
        DeviceType,
        FixedImage,
        Mandatory,
        Interface,

        // Display
        DisplayType,
        Rotate,
        FlipX,
        Width,
        Height,
        Refresh,
        PixClock,
        HTotal,
        HBEnd,
        HBStart,
        VTotal,
        VBEnd,
        VBStart,

        // Driver
        SupportStatus,
        EmulationStatus,
        CocktailStatus,
        SaveStateStatus,
        RequiresArtwork,
        Unofficial,
        NoSoundHardware,
        Incomplete,

        // Extension
        Extension_Name,

        // Feature
        FeatureType,
        FeatureStatus,
        FeatureOverall,

        // Input
        Service,
        Tilt,
        Players,
        Coins,

        // Instance
        Instance_Name,
        Instance_BriefName,

        // Location
        Location_Name,
        Location_Number,
        Location_Inverted,

        // Part
        Part_Name,
        Part_Interface,

        // PartFeature
        Part_Feature_Name,
        Part_Feature_Value,

        // RamOption
        Content,

        // Release
        Language,

        // Setting
        Setting_Name,
        Setting_Value,
        Setting_Default,

        // SlotOption
        SlotOption_Name,
        SlotOption_DeviceName,
        SlotOption_Default,

        // SoftwareList
        SoftwareListStatus,
        Filter,

        // Sound
        Channels,

        #endregion

        #endregion // Item-Specific
    }

    /// <summary>
    /// List of valid field types within a Machine
    /// </summary>
    public enum MachineField
    {
        NULL = 0,

        #region Common

        Name,
        Comment,
        Description,
        Year,
        Manufacturer,
        Publisher,
        Category,
        RomOf,
        CloneOf,
        SampleOf,
        Type,

        #endregion

        #region AttractMode

        Players,
        Rotation,
        Control,
        Status,
        DisplayCount,
        DisplayType,
        Buttons,

        #endregion

        #region ListXML

        History,
        SourceFile,
        Runnable,

        #endregion

        #region Logiqx

        Board,
        RebuildTo,

        #endregion

        #region Logiqx EmuArc

        TitleID,
        Developer,
        Genre,
        Subgenre,
        Ratings,
        Score,
        Enabled,
        CRC,
        RelatedTo,

        #endregion

        #region OpenMSX

        GenMSXID,
        System,
        Country,

        #endregion

        #region SoftwareList

        Supported,

        #endregion
    }

    #endregion

    #region Logging

    /// <summary>
    /// Severity of the logging statement
    /// </summary>
    public enum LogLevel
    {
        VERBOSE = 0,
        USER,
        WARNING,
        ERROR,
    }

    #endregion
}
