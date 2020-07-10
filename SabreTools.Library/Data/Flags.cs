using System;

namespace SabreTools.Library.Data
{
    #region Archival

    /// <summary>
    /// Determines the level to scan archives at
    /// </summary>
    [Flags]
    public enum ArchiveScanLevel
    {
        // 7zip
        SevenZipExternal = 1 << 0,
        SevenZipInternal = 1 << 1,
        SevenZipBoth = SevenZipExternal | SevenZipInternal,

        // GZip
        GZipExternal = 1 << 2,
        GZipInternal = 1 << 3,
        GZipBoth = GZipExternal | GZipInternal,

        // RAR
        RarExternal = 1 << 4,
        RarInternal = 1 << 5,
        RarBoth = RarExternal | RarInternal,

        // Zip
        ZipExternal = 1 << 6,
        ZipInternal = 1 << 7,
        ZipBoth = ZipExternal | ZipInternal,

        // Tar
        TarExternal = 1 << 8,
        TarInternal = 1 << 9,
        TarBoth = TarExternal | TarInternal,
    }

    /// <summary>
    /// Determines the archive general bit flags
    /// </summary>
    [Flags]
    public enum GeneralPurposeBitFlag : ushort
    {
        Encrypted = 1 << 0,
        ZeroedCRCAndSize = 1 << 3,
        CompressedPatchedData = 1 << 5,
        StrongEncryption = 1 << 6,
        LanguageEncodingFlag = 1 << 11,
        EncryptedCentralDirectory = 1 << 13,

        // For Method 6 - Imploding
        Imploding8KSlidingDictionary = 1 << 1,
        Imploding3ShannonFanoTrees = 1 << 2,

        // For Methods 8 and 9 - Deflating
        DeflatingMaximumCompression = 1 << 1,
        DeflatingFastCompression = 1 << 2,
        DeflatingSuperFastCompression = 1 << 1 | 1 << 2,
        EnhancedDeflating = 1 << 4,

        // For Method 14 - LZMA
        LZMAEOSMarkerUsed = 1 << 1,

        // Reserved and unused (SHOULD NOT BE USED)
        Bit7 = 1 << 7,
        Bit8 = 1 << 8,
        Bit9 = 1 << 9,
        Bit10 = 1 << 10,
        Bit12 = 1 << 12, // Reserved by PKWARE for enhanced compression
        Bit14 = 1 << 14, // Reserved by PKWARE
        Bit15 = 1 << 15, // Reserved by PKWARE
    }

    /// <summary>
    /// Internal file attributes used by archives
    /// </summary>
    [Flags]
    public enum InternalFileAttributes : ushort
    {
        ASCIIOrTextFile = 1 << 0,
        RecordLengthControl = 1 << 1,

        // Reserved and unused (SHOULD NOT BE USED)
        Bit1 = 1 << 1,
        Bit2 = 1 << 2,
    }

    /// <summary>
    /// RAR archive flags
    /// </summary>
    [Flags]
    public enum RarArchiveFlags : uint
    {
        /// <summary>
        /// Volume. Archive is a part of multivolume set.
        /// </summary>
        Volume = 1 << 0,

        /// <summary>
        /// Volume number field is present. This flag is present in all volumes except first.
        /// </summary>
        VolumeNumberField = 1 << 1,

        /// <summary>
        /// Solid archive.
        /// </summary>
        Solid = 1 << 2,

        /// <summary>
        /// Recovery record is present.
        /// </summary>
        RecoveryRecordPresent = 1 << 3,

        /// <summary>
        /// Locked archive.
        /// </summary>
        Locked = 1 << 4,
    }

    /// <summary>
    /// RAR entry encryption flags
    /// </summary>
    [Flags]
    public enum RarEncryptionFlags : uint
    {
        PasswordCheckDataPresent = 1 << 0,
        UseTweakedChecksums = 1 << 1,

        /*
        If flag 0x0002 is present, RAR transforms the checksum preserving file or service data integrity, so it becomes dependent on 
        encryption key. It makes guessing file contents based on checksum impossible. It affects both data CRC32 in file header and 
        checksums in file hash record in extra area.
        */
    }

    /// <summary>
    /// RAR file flags
    /// </summary>
    [Flags]
    public enum RarFileFlags : uint
    {
        /// <summary>
        /// Directory file system object (file header only)
        /// </summary>
        Directory = 1 << 0,

        /// <summary>
        /// Time field in Unix format is present
        /// </summary>
        TimeInUnix = 1 << 1,

        /// <summary>
        /// CRC32 field is present
        /// </summary>
        CRCPresent = 1 << 2,

        /// <summary>
        /// Unpacked size is unknown
        /// </summary>
        UnpackedSizeUnknown = 1 << 3,

        /*
        If flag 0x0008 is set, unpacked size field is still present, but must be ignored and extraction
        must be performed until reaching the end of compression stream. This flag can be set if actual
        file size is larger than reported by OS or if file size is unknown such as for all volumes except
        last when archiving from stdin to multivolume archive
        */
    }

    /// <summary>
    /// RAR header flags
    /// </summary>
    [Flags]
    public enum RarHeaderFlags : uint
    {
        /// <summary>
        /// Extra area is present in the end of header
        /// </summary>
        ExtraAreaPresent = 1 << 0,

        /// <summary>
        /// Data area is present in the end of header
        /// </summary>
        DataAreaPresent = 1 << 1,

        /// <summary>
        /// Blocks with unknown type and this flag must be skipped when updating an archive
        /// </summary>
        BlocksWithUnknownType = 1 << 2,

        /// <summary>
        /// Data area is continuing from previous volume
        /// </summary>
        DataAreaContinuingFromPrevious = 1 << 3,

        /// <summary>
        /// Data area is continuing in next volume
        /// </summary>
        DataAreaContinuingToNext = 1 << 4,

        /// <summary>
        /// Block depends on preceding file block
        /// </summary>
        BlockDependsOnPreceding = 1 << 5,

        /// <summary>
        /// Preserve a child block if host block is modified
        /// </summary>
        PreserveChildBlock = 1 << 6,
    }

    [Flags]
    public enum RarUnixOwnerRecordFlags : uint
    {
        UserNameStringIsPresent = 1 << 0,
        GroupNameStringIsPresent = 1 << 1,
        NumericUserIdIsPresent = 1 << 2,
        NumericGroupIdIsPresent = 1 << 3,
    }

    /// <summary>
    /// RAR entry time flags
    /// </summary>
    [Flags]
    public enum RarTimeFlags : uint
    {
        TimeInUnixFormat = 1 << 0,
        ModificationTimePresent = 1 << 1,
        CreationTimePresent = 1 << 2,
        LastAccessTimePresent = 1 << 3,
    }

    #endregion

    #region DatFile related

    /// <summary>
    /// DAT output formats
    /// </summary>
    [Flags]
    public enum DatFormat
    {
        #region XML Formats

        /// <summary>
        /// Logiqx XML (using machine)
        /// </summary>
        Logiqx = 1 << 0,

        /// <summary>
        /// Logiqx XML (using game)
        /// </summary>
        LogiqxDeprecated = 1 << 1,

        /// <summary>
        /// MAME Softare List XML
        /// </summary>
        SoftwareList = 1 << 2,

        /// <summary>
        /// MAME Listxml output
        /// </summary>
        Listxml = 1 << 3,

        /// <summary>
        /// OfflineList XML
        /// </summary>
        OfflineList = 1 << 4,

        /// <summary>
        /// SabreDat XML
        /// </summary>
        SabreDat = 1 << 5,

        /// <summary>
        /// openMSX Software List XML
        /// </summary>
        OpenMSX = 1 << 6,

        #endregion

        #region Propietary Formats

        /// <summary>
        /// ClrMamePro custom
        /// </summary>
        ClrMamePro = 1 << 7,

        /// <summary>
        /// RomCetner INI-based
        /// </summary>
        RomCenter = 1 << 8,

        /// <summary>
        /// DOSCenter custom
        /// </summary>
        DOSCenter = 1 << 9,

        /// <summary>
        /// AttractMode custom
        /// </summary>
        AttractMode = 1 << 10,

        #endregion

        #region Standardized Text Formats

        /// <summary>
        /// ClrMamePro missfile
        /// </summary>
        MissFile = 1 << 11,

        /// <summary>
        /// Comma-Separated Values (standardized)
        /// </summary>
        CSV = 1 << 12,

        /// <summary>
        /// Semicolon-Separated Values (standardized)
        /// </summary>
        SSV = 1 << 13,

        /// <summary>
        /// Tab-Separated Values (standardized)
        /// </summary>
        TSV = 1 << 14,

        /// <summary>
        /// MAME Listrom output
        /// </summary>
        Listrom = 1 << 15,

        /// <summary>
        /// Everdrive Packs SMDB
        /// </summary>
        EverdriveSMDB = 1 << 16,

        /// <summary>
        /// JSON
        /// </summary>
        Json = 1 << 17,

        #endregion

        #region SFV-similar Formats

        /// <summary>
        /// CRC32 hash list
        /// </summary>
        RedumpSFV = 1 << 18,

        /// <summary>
        /// MD5 hash list
        /// </summary>
        RedumpMD5 = 1 << 19,

#if NET_FRAMEWORK
        /// <summary>
        /// RIPEMD160 hash list
        /// </summary>
        RedumpRIPEMD160 =       1 << 20,
#endif

        /// <summary>
        /// SHA-1 hash list
        /// </summary>
        RedumpSHA1 = 1 << 21,

        /// <summary>
        /// SHA-256 hash list
        /// </summary>
        RedumpSHA256 = 1 << 22,

        /// <summary>
        /// SHA-384 hash list
        /// </summary>
        RedumpSHA384 = 1 << 23,

        /// <summary>
        /// SHA-512 hash list
        /// </summary>
        RedumpSHA512 = 1 << 24,

        #endregion

        // Specialty combinations
        ALL = Int32.MaxValue,
    }

    /// <summary>
    /// Available hashing types
    /// </summary>
    [Flags]
    public enum Hash
    {
        CRC = 1 << 0,
        MD5 = 1 << 1,
#if NET_FRAMEWORK
        RIPEMD160 = 1 << 2,
#endif
        SHA1 = 1 << 3,
        SHA256 = 1 << 4,
        SHA384 = 1 << 5,
        SHA512 = 1 << 6,

        // Special combinations
        Standard = CRC | MD5 | SHA1,
#if NET_FRAMEWORK
        DeepHashes = SHA256 | SHA384 | SHA512,
        SecureHashes = MD5 | RIPEMD160 | SHA1 | SHA256 | SHA384 | SHA512,
#else
        DeepHashes = SHA256 | SHA384 | SHA512,
        SecureHashes = MD5 | SHA1 | SHA256 | SHA384 | SHA512,
#endif
    }

    /// <summary>
    /// Determine which format to output Stats to
    /// </summary>
    [Flags]
    public enum StatReportFormat
    {
        /// <summary>
        /// Only output to the console
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Console-formatted
        /// </summary>
        Textfile = 1 << 0,

        /// <summary>
        /// ClrMamePro HTML
        /// </summary>
        HTML = 1 << 1,

        /// <summary>
        /// Comma-Separated Values (Standardized)
        /// </summary>
        CSV = 1 << 2,

        /// <summary>
        /// Semicolon-Separated Values (Standardized)
        /// </summary>
        SSV = 1 << 3,

        /// <summary>
        /// Tab-Separated Values (Standardized)
        /// </summary>
        TSV = 1 << 4,

        All = Int32.MaxValue,
    }

    /// <summary>
    /// Determines how the DAT will be split on output
    /// </summary>
    [Flags]
    public enum SplittingMode
    {
        None = 0x00,

        Extension = 1 << 0,
        Hash = 1 << 2,
        Level = 1 << 3,
        Type = 1 << 4,
        Size = 1 << 5,
    }

    /// <summary>
    /// Determines special update modes
    /// </summary>
    [Flags]
    public enum UpdateMode
    {
        None = 0x00,

        // Standard diffs
        DiffDupesOnly = 1 << 0,
        DiffNoDupesOnly = 1 << 1,
        DiffIndividualsOnly = 1 << 2,

        // Cascaded diffs
        DiffCascade = 1 << 3,
        DiffReverseCascade = 1 << 4,

        // Base diffs
        DiffAgainst = 1 << 5,

        // Special update modes
        Merge = 1 << 6,
        BaseReplace = 1 << 7,
        ReverseBaseReplace = 1 << 8,

        // Combinations
        AllDiffs = DiffDupesOnly | DiffNoDupesOnly | DiffIndividualsOnly,
    }

    #endregion

    #region DatItem related

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
    /// Determine the status of the item
    /// </summary>
    [Flags]
    public enum ItemStatus
    {
        /// <summary>
        /// This is a fake flag that is used for filter only
        /// </summary>
        NULL = 0x00,

        None = 1 << 0,
        Good = 1 << 1,
        BadDump = 1 << 2,
        Nodump = 1 << 3,
        Verified = 1 << 4,
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
        NULL = 0x00,

        None = 1 << 0,
        Bios = 1 << 1,
        Device = 1 << 2,
        Mechanical = 1 << 3,
    }

    #endregion
}
