using System;
using SabreTools.Core;

namespace SabreTools.DatFiles
{
    /// <summary>
    /// DAT output formats
    /// </summary>
    [Flags]
    public enum DatFormat : ulong
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
        /// SabreDAT XML
        /// </summary>
        SabreXML = 1 << 5,

        /// <summary>
        /// openMSX Software List XML
        /// </summary>
        OpenMSX = 1 << 6,

        /// <summary>
        /// Archive.org file list XML
        /// </summary>
        ArchiveDotOrg = 1 << 7,

        #endregion

        #region Propietary Formats

        /// <summary>
        /// ClrMamePro custom
        /// </summary>
        ClrMamePro = 1 << 8,

        /// <summary>
        /// RomCenter INI-based
        /// </summary>
        RomCenter = 1 << 9,

        /// <summary>
        /// DOSCenter custom
        /// </summary>
        DOSCenter = 1 << 10,

        /// <summary>
        /// AttractMode custom
        /// </summary>
        AttractMode = 1 << 11,

        #endregion

        #region Standardized Text Formats

        /// <summary>
        /// ClrMamePro missfile
        /// </summary>
        MissFile = 1 << 12,

        /// <summary>
        /// Comma-Separated Values (standardized)
        /// </summary>
        CSV = 1 << 13,

        /// <summary>
        /// Semicolon-Separated Values (standardized)
        /// </summary>
        SSV = 1 << 14,

        /// <summary>
        /// Tab-Separated Values (standardized)
        /// </summary>
        TSV = 1 << 15,

        /// <summary>
        /// MAME Listrom output
        /// </summary>
        Listrom = 1 << 16,

        /// <summary>
        /// Everdrive Packs SMDB
        /// </summary>
        EverdriveSMDB = 1 << 17,

        /// <summary>
        /// SabreJSON
        /// </summary>
        SabreJSON = 1 << 18,

        #endregion

        #region SFV-similar Formats

        /// <summary>
        /// CRC32 hash list
        /// </summary>
        RedumpSFV = 1 << 19,

        /// <summary>
        /// MD2 hash list
        /// </summary>
        RedumpMD2 = 1 << 20,

        /// <summary>
        /// MD4 hash list
        /// </summary>
        RedumpMD4 = 1 << 21,

        /// <summary>
        /// MD5 hash list
        /// </summary>
        RedumpMD5 = 1 << 22,

        /// <summary>
        /// RIPEMD128 hash list
        /// </summary>
        RedumpRIPEMD128 = 1 << 23,

        /// <summary>
        /// RIPEMD160 hash list
        /// </summary>
        RedumpRIPEMD160 = 1 << 24,

        /// <summary>
        /// SHA-1 hash list
        /// </summary>
        RedumpSHA1 = 1 << 25,

        /// <summary>
        /// SHA-256 hash list
        /// </summary>
        RedumpSHA256 = 1 << 26,

        /// <summary>
        /// SHA-384 hash list
        /// </summary>
        RedumpSHA384 = 1 << 27,

        /// <summary>
        /// SHA-512 hash list
        /// </summary>
        RedumpSHA512 = 1 << 28,

        /// <summary>
        /// SpamSum hash list
        /// </summary>
        RedumpSpamSum = 1 << 29,

        #endregion

        // Specialty combinations
        ALL = ulong.MaxValue,
    }

    /// <summary>
    /// Determines merging tag handling for DAT output
    /// </summary>
    public enum MergingFlag
    {
        [Mapping("none")]
        None = 0,

        [Mapping("split")]
        Split,

        [Mapping("merged")]
        Merged,

        [Mapping("nonmerged", "unmerged")]
        NonMerged,

        /// <remarks>This is not usually defined for Merging flags</remarks>
        [Mapping("fullmerged")]
        FullMerged,

        /// <remarks>This is not usually defined for Merging flags</remarks>
        [Mapping("device", "deviceunmerged", "devicenonmerged")]
        DeviceNonMerged,

        /// <remarks>This is not usually defined for Merging flags</remarks>
        [Mapping("full", "fullunmerged", "fullnonmerged")]
        FullNonMerged,
    }

    /// <summary>
    /// Determines nodump tag handling for DAT output
    /// </summary>
    public enum NodumpFlag
    {
        [Mapping("none")]
        None = 0,

        [Mapping("obsolete")]
        Obsolete,

        [Mapping("required")]
        Required,

        [Mapping("ignore")]
        Ignore,
    }

    /// <summary>
    /// Determines packing tag handling for DAT output
    /// </summary>
    public enum PackingFlag
    {
        [Mapping("none")]
        None = 0,

        /// <summary>
        /// Force all sets to be in archives, except disk and media
        /// </summary>
        [Mapping("zip", "yes")]
        Zip,

        /// <summary>
        /// Force all sets to be extracted into subfolders
        /// </summary>
        [Mapping("unzip", "no")]
        Unzip,

        /// <summary>
        /// Force sets with single items to be extracted to the parent folder
        /// </summary>
        [Mapping("partial")]
        Partial,

        /// <summary>
        /// Force all sets to be extracted to the parent folder
        /// </summary>
        [Mapping("flat")]
        Flat,

        /// <summary>
        /// Force all sets to have all archives treated as files
        /// </summary>
        [Mapping("fileonly")]
        FileOnly, 
    }
}