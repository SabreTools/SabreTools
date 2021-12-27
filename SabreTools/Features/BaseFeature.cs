using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Core;
using SabreTools.Core.Tools;
using SabreTools.DatFiles;
using SabreTools.DatTools;
using SabreTools.FileTypes;
using SabreTools.Filtering;
using SabreTools.Help;
using SabreTools.IO;
using SabreTools.Logging;
using SabreTools.Reports;
using Microsoft.Data.Sqlite;

namespace SabreTools.Features
{
    internal class BaseFeature : TopLevel
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        protected Logger logger = new Logger();

        #endregion

        #region Constants

        public static string HeadererFileName = Path.Combine(PathTool.GetRuntimeDirectory(), "Headerer.sqlite");
        public static string HeadererConnectionString = $"Data Source={HeadererFileName};";

        #endregion

        #region Features

        #region Flag features

        internal const string AaruFormatsAsFilesValue = "aaruformats-as-files";
        internal static Feature AaruFormatsAsFilesFlag
        {
            get
            {
                return new Feature(
                    AaruFormatsAsFilesValue,
                    new List<string>() { "-caf", "--aaruformats-as-files" },
                    "Treat AaruFormats as files",
                    ParameterType.Flag,
                    longDescription: "Normally, AaruFormats would be processed using their internal hash to compare against the input DATs. This flag forces all AaruFormats to be treated like regular files.");
            }
        }

        internal const string AddBlankFilesValue = "add-blank-files";
        internal static Feature AddBlankFilesFlag
        {
            get
            {
                return new Feature(
                    AddBlankFilesValue,
                    new List<string>() { "-ab", "--add-blank-files" },
                    "Output blank files for folders",
                    ParameterType.Flag,
                    longDescription: "If this flag is set, then blank entries will be created for each of the empty directories in the source. This is useful for tools that require all folders be accounted for in the output DAT.");
            }
        }

        internal const string AddDateValue = "add-date";
        internal static Feature AddDateFlag
        {
            get
            {
                return new Feature(
                    AddDateValue,
                    new List<string>() { "-ad", "--add-date" },
                    "Add dates to items, where possible",
                    ParameterType.Flag,
                    longDescription: "If this flag is set, then the Date will be appended to each file information in the output DAT. The output format is standardized as \"yyyy/MM/dd HH:mm:ss\".");
            }
        }

        internal const string ArchivesAsFilesValue = "archives-as-files";
        internal static Feature ArchivesAsFilesFlag
        {
            get
            {
                return new Feature(
                    ArchivesAsFilesValue,
                    new List<string>() { "-aaf", "--archives-as-files" },
                    "Treat archives as files",
                    ParameterType.Flag,
                    longDescription: "Instead of trying to enumerate the files within archives, treat the archives as files themselves. This is good for uncompressed sets that include archives that should be read as-is.");
            }
        }

        internal const string BaddumpColumnValue = "baddump-column";
        internal static Feature BaddumpColumnFlag
        {
            get
            {
                return new Feature(
                    BaddumpColumnValue,
                    new List<string>() { "-bc", "--baddump-column" },
                    "Add baddump stats to output",
                    ParameterType.Flag,
                    longDescription: "Add a new column or field for counting the number of baddumps in the DAT.");
            }
        }

        internal const string BaseValue = "base";
        internal static Feature BaseFlag
        {
            get
            {
                return new Feature(
                    BaseValue,
                    new List<string>() { "-ba", "--base" },
                    "Use source DAT as base name for outputs",
                    ParameterType.Flag,
                    longDescription: "If splitting an entire folder of DATs, some output files may be normally overwritten since the names would be the same. With this flag, the original DAT name is used in the output name, in the format of \"Original Name(Dir - Name)\". This can be used in conjunction with --short to output in the format of \"Original Name (Name)\" instead.");
            }
        }

        internal const string BaseReplaceValue = "base-replace";
        internal static Feature BaseReplaceFlag
        {
            get
            {
                return new Feature(
                    BaseReplaceValue,
                    new List<string>() { "-br", "--base-replace" },
                    "Replace from base DATs in order",
                    ParameterType.Flag,
                    longDescription: "By default, no item names are changed except when there is a merge occurring. This flag enables users to define a DAT or set of base DATs to use as \"replacements\" for all input DATs. Note that the first found instance of an item in the base DAT(s) will be used and all others will be discarded. If no additional flag is given, it will default to updating names.");
            }
        }

        internal const string ByGameValue = "by-game";
        internal static Feature ByGameFlag
        {
            get
            {
                return new Feature(
                    ByGameValue,
                    new List<string>() { "-bg", "--by-game" },
                    "Diff against by game instead of hashes",
                    ParameterType.Flag,
                    longDescription: "By default, diffing against uses hashes to determine similar files. This flag enables using using each game as a comparision point instead.");
            }
        }

        internal const string ChdsAsFilesValue = "chds-as-files";
        internal static Feature ChdsAsFilesFlag
        {
            get
            {
                return new Feature(
                    ChdsAsFilesValue,
                    new List<string>() { "-ic", "--chds-as-files" },
                    "Treat CHDs as regular files",
                    ParameterType.Flag,
                    longDescription: "Normally, CHDs would be processed using their internal hash to compare against the input DATs. This flag forces all CHDs to be treated like regular files.");
            }
        }

        internal const string CleanValue = "clean";
        internal static Feature CleanFlag
        {
            get
            {
                return new Feature(
                    CleanValue,
                    new List<string>() { "-clean", "--clean" },
                    "Clean game names according to WoD standards",
                    ParameterType.Flag,
                    longDescription: "Game names will be sanitized to remove what the original WoD standards deemed as unneeded information, such as parenthesized or bracketed strings.");
            }
        }

        internal const string DatDeviceNonMergedValue = "dat-device-non-merged";
        internal static Feature DatDeviceNonMergedFlag
        {
            get
            {
                return new Feature(
                    DatDeviceNonMergedValue,
                    new List<string>() { "-dnd", "--dat-device-non-merged" },
                    "Create device non-merged sets",
                    ParameterType.Flag,
                    longDescription: "Preprocess the DAT to have child sets contain all items from the device references. This is incompatible with the other --dat-X flags.");
            }
        }

        internal const string DatFullNonMergedValue = "dat-full-non-merged";
        internal static Feature DatFullNonMergedFlag
        {
            get
            {
                return new Feature(
                    DatFullNonMergedValue,
                    new List<string>() { "-df", "--dat-full-non-merged" },
                    "Create fully non-merged sets",
                    ParameterType.Flag,
                    longDescription: "Preprocess the DAT to have child sets contain all items from the parent sets based on the cloneof and romof tags as well as device references. This is incompatible with the other --dat-X flags.");
            }
        }

        internal const string DatMergedValue = "dat-merged";
        internal static Feature DatMergedFlag
        {
            get
            {
                return new Feature(
                    DatMergedValue,
                    new List<string>() { "-dm", "--dat-merged" },
                    "Force creating merged sets",
                    ParameterType.Flag,
                    longDescription: "Preprocess the DAT to have parent sets contain all items from the children based on the cloneof tag. This is incompatible with the other --dat-X flags.");
            }
        }

        internal const string DatNonMergedValue = "dat-non-merged";
        internal static Feature DatNonMergedFlag
        {
            get
            {
                return new Feature(
                    DatNonMergedValue,
                    new List<string>() { "-dnm", "--dat-non-merged" },
                    "Force creating non-merged sets",
                    ParameterType.Flag,
                    longDescription: "Preprocess the DAT to have child sets contain all items from the parent set based on the romof and cloneof tags. This is incompatible with the other --dat-X flags.");
            }
        }

        internal const string DatSplitValue = "dat-split";
        internal static Feature DatSplitFlag
        {
            get
            {
                return new Feature(
                    DatSplitValue,
                    new List<string>() { "-ds", "--dat-split" },
                    "Force creating split sets",
                    ParameterType.Flag,
                    longDescription: "Preprocess the DAT to remove redundant files between parents and children based on the romof and cloneof tags. This is incompatible with the other --dat-X flags.");
            }
        }

        internal const string DedupValue = "dedup";
        internal static Feature DedupFlag
        {
            get
            {
                return new Feature(
                    DedupValue,
                    new List<string>() { "-dd", "--dedup" },
                    "Enable deduping in the created DAT",
                    ParameterType.Flag,
                    longDescription: "For all outputted DATs, allow for hash deduping. This makes sure that there are effectively no duplicates in the output files. Cannot be used with game dedup.");
            }
        }

        internal const string DeleteValue = "delete";
        internal static Feature DeleteFlag
        {
            get
            {
                return new Feature(
                    DeleteValue,
                    new List<string>() { "-del", "--delete" },
                    "Delete fully rebuilt input files",
                    ParameterType.Flag,
                    longDescription: "Optionally, the input files, once processed and fully matched, can be deleted. This can be useful when the original file structure is no longer needed or if there is limited space on the source drive.");
            }
        }

        internal const string DepotValue = "depot";
        internal static Feature DepotFlag
        {
            get
            {
                return new Feature(
                    DepotValue,
                    new List<string>() { "-dep", "--depot" },
                    "Assume directories are Romba depots",
                    ParameterType.Flag,
                    longDescription: "Normally, input directories will be treated with no special format. If this flag is used, all input directories will be assumed to be Romba-style depots.");
            }
        }

        internal const string DeprecatedValue = "deprecated";
        internal static Feature DeprecatedFlag
        {
            get
            {
                return new Feature(
                    DeprecatedValue,
                    new List<string>() { "-dpc", "--deprecated" },
                    "Output 'game' instead of 'machine'",
                    ParameterType.Flag,
                    longDescription: "By default, Logiqx XML DATs output with the more modern \"machine\" tag for each set. This flag allows users to output the older \"game\" tag instead, for compatibility reasons. [Logiqx only]");
            }
        }

        internal const string DescriptionAsNameValue = "description-as-name";
        internal static Feature DescriptionAsNameFlag
        {
            get
            {
                return new Feature(
                    DescriptionAsNameValue,
                    new List<string>() { "-dan", "--description-as-name" },
                    "Use description instead of machine name",
                    ParameterType.Flag,
                    longDescription: "By default, all DATs are converted exactly as they are input. Enabling this flag allows for the machine names in the DAT to be replaced by the machine description instead. In most cases, this will result in no change in the output DAT, but a notable example would be a software list DAT where the machine names are generally DOS-friendly while the description is more complete.");
            }
        }

        internal const string DiffAgainstValue = "diff-against";
        internal static Feature DiffAgainstFlag
        {
            get
            {
                return new Feature(
                    DiffAgainstValue,
                    new List<string>() { "-dag", "--diff-against" },
                    "Diff all inputs against a set of base DATs",
                    ParameterType.Flag,
                    "This flag will enable a special type of diffing in which a set of base DATs are used as a comparison point for each of the input DATs. This allows users to get a slightly different output to cascaded diffing, which may be more useful in some cases. This is heavily influenced by the diffing model used by Romba.");
            }
        }

        internal const string DiffAllValue = "diff-all";
        internal static Feature DiffAllFlag
        {
            get
            {
                return new Feature(
                    DiffAllValue,
                    new List<string>() { "-di", "--diff-all" },
                    "Create diffdats from inputs (all standard outputs)",
                    ParameterType.Flag,
                    longDescription: "By default, all DATs are processed individually with the user-specified flags. With this flag enabled, input DATs are diffed against each other to find duplicates, no duplicates, and only in individuals.");
            }
        }

        internal const string DiffCascadeValue = "diff-cascade";
        internal static Feature DiffCascadeFlag
        {
            get
            {
                return new Feature(
                    DiffCascadeValue,
                    new List<string>() { "-dc", "--diff-cascade" },
                    "Enable cascaded diffing",
                    ParameterType.Flag,
                    longDescription: "This flag allows for a special type of diffing in which the first DAT is considered a base, and for each additional input DAT, it only leaves the files that are not in one of the previous DATs. This can allow for the creation of rollback sets or even just reduce the amount of duplicates across multiple sets.");
            }
        }

        internal const string DiffDuplicatesValue = "diff-duplicates";
        internal static Feature DiffDuplicatesFlag
        {
            get
            {
                return new Feature(
                    DiffDuplicatesValue,
                    new List<string>() { "-did", "--diff-duplicates" },
                    "Create diffdat containing just duplicates",
                    ParameterType.Flag,
                    longDescription: "All files that have duplicates outside of the original DAT are included.");
            }
        }

        internal const string DiffIndividualsValue = "diff-individuals";
        internal static Feature DiffIndividualsFlag
        {
            get
            {
                return new Feature(
                    DiffIndividualsValue,
                    new List<string>() { "-dii", "--diff-individuals" },
                    "Create diffdats for individual DATs",
                    ParameterType.Flag,
                    longDescription: "All files that have no duplicates outside of the original DATs are put into DATs that are named after the source DAT.");
            }
        }

        internal const string DiffNoDuplicatesValue = "diff-no-duplicates";
        internal static Feature DiffNoDuplicatesFlag
        {
            get
            {
                return new Feature(
                    DiffNoDuplicatesValue,
                    new List<string>() { "-din", "--diff-no-duplicates" },
                    "Create diffdat containing no duplicates",
                    ParameterType.Flag,
                    longDescription: "All files that have no duplicates outside of the original DATs are included.");
            }
        }

        internal const string DiffReverseCascadeValue = "diff-reverse-cascade";
        internal static Feature DiffReverseCascadeFlag
        {
            get
            {
                return new Feature(
                    DiffReverseCascadeValue,
                    new List<string>() { "-drc", "--diff-reverse-cascade" },
                    "Enable reverse cascaded diffing",
                    ParameterType.Flag,
                    longDescription: "This flag allows for a special type of diffing in which the last DAT is considered a base, and for each additional input DAT, it only leaves the files that are not in one of the previous DATs. This can allow for the creation of rollback sets or even just reduce the amount of duplicates across multiple sets.");
            }
        }

        internal const string ExtensionValue = "extension";
        internal static Feature ExtensionFlag
        {
            get
            {
                return new Feature(
                    ExtensionValue,
                    new List<string>() { "-es", "--extension" },
                    "Split DAT(s) by two file extensions",
                    ParameterType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on a list of input extensions. This can allow for combined DAT files, such as those combining two separate systems, to be split. Files with any extensions not listed in the input lists will be included in both outputted DAT files.");
            }
        }

        internal const string GameDedupValue = "game-dedup";
        internal static Feature GameDedupFlag
        {
            get
            {
                return new Feature(
                    GameDedupValue,
                    new List<string>() { "-gdd", "--game-dedup" },
                    "Enable deduping within games in the created DAT",
                    ParameterType.Flag,
                    longDescription: "For all outputted DATs, allow for hash deduping but only within the games, and not across the entire DAT. This makes sure that there are effectively no duplicates within each of the output sets. Cannot be used with standard dedup.");
            }
        }

        internal const string GamePrefixValue = "game-prefix";
        internal static Feature GamePrefixFlag
        {
            get
            {
                return new Feature(
                    GamePrefixValue,
                    new List<string>() { "-gp", "--game-prefix" },
                    "Add game name as a prefix",
                    ParameterType.Flag,
                    longDescription: "This flag allows for the name of the game to be used as a prefix to each file.");
            }
        }

        internal const string HashValue = "hash";
        internal static Feature HashFlag
        {
            get
            {
                return new Feature(
                    HashValue,
                    new List<string>() { "-hs", "--hash" },
                    "Split DAT(s) or folder by best-available hashes",
                    ParameterType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on the best available hash for each file within. The order of preference for the outputted DATs is as follows: Nodump, SHA-512, SHA-384, SHA-256, SHA-1, MD5, CRC (or worse).");
            }
        }

        internal const string HashOnlyValue = "hash-only";
        internal static Feature HashOnlyFlag
        {
            get
            {
                return new Feature(
                    HashOnlyValue,
                    new List<string>() { "-ho", "--hash-only" },
                    "Check files by hash only",
                    ParameterType.Flag,
                    longDescription: "This sets a mode where files are not checked based on name but rather hash alone. This allows verification of (possibly) incorrectly named folders and sets to be verified without worrying about the proper set structure to be there.");
            }
        }

        internal const string IncludeCrcValue = "include-crc";
        internal static Feature IncludeCrcFlag
        {
            get
            {
                return new Feature(
                    IncludeCrcValue,
                    new List<string>() { "-crc", "--include-crc" },
                    "Include CRC32 in output",
                    ParameterType.Flag,
                    longDescription: "This enables CRC32 calculation for each of the files. Adding this flag overrides the default hashing behavior of including CRC32, MD5, and SHA-1 hashes.");
            }
        }

        internal const string IncludeMd5Value = "include-md5";
        internal static Feature IncludeMd5Flag
        {
            get
            {
                return new Feature(
                    IncludeMd5Value,
                    new List<string>() { "-md5", "--include-md5" },
                    "Include MD5 in output",
                    ParameterType.Flag,
                    longDescription: "This enables MD5 calculation for each of the files. Adding this flag overrides the default hashing behavior of including CRC32, MD5, and SHA-1 hashes.");
            }
        }

        internal const string IncludeSha1Value = "include-sha1";
        internal static Feature IncludeSha1Flag
        {
            get
            {
                return new Feature(
                    IncludeSha1Value,
                    new List<string>() { "-sha1", "--include-sha1" },
                    "Include SHA-1 in output",
                    ParameterType.Flag,
                    longDescription: "This enables SHA-1 calculation for each of the files. Adding this flag overrides the default hashing behavior of including CRC32, MD5, and SHA-1 hashes.");
            }
        }

        internal const string IncludeSha256Value = "include-sha256";
        internal static Feature IncludeSha256Flag
        {
            get
            {
                return new Feature(
                    IncludeSha256Value,
                    new List<string>() { "-sha256", "--include-sha256" },
                    "Include SHA-256 in output",
                    ParameterType.Flag,
                    longDescription: "This enables SHA-256 calculation for each of the files. Adding this flag overrides the default hashing behavior of including CRC32, MD5, and SHA-1 hashes.");
            }
        }

        internal const string IncludeSha384Value = "include-sha384";
        internal static Feature IncludeSha384Flag
        {
            get
            {
                return new Feature(
                    IncludeSha384Value,
                    new List<string>() { "-sha384", "--include-sha384" },
                    "Include SHA-384 in output",
                    ParameterType.Flag,
                    longDescription: "This enables SHA-384 calculation for each of the files. Adding this flag overrides the default hashing behavior of including CRC32, MD5, and SHA-1 hashes.");
            }
        }

        internal const string IncludeSha512Value = "include-sha512";
        internal static Feature IncludeSha512Flag
        {
            get
            {
                return new Feature(
                    IncludeSha512Value,
                    new List<string>() { "-sha512", "--include-sha512" },
                    "Include SHA-512 in output",
                    ParameterType.Flag,
                    longDescription: "This enables SHA-512 calculation for each of the files. Adding this flag overrides the default hashing behavior of including CRC32, MD5, and SHA-1 hashes.");
            }
        }

        internal const string IncludeSpamSumValue = "include-spamsum";
        internal static Feature IncludeSpamSumFlag
        {
            get
            {
                return new Feature(
                    IncludeSpamSumValue,
                    new List<string>() { "-spamsum", "--include-spamsum" },
                    "Include SpamSum in output",
                    ParameterType.Flag,
                    longDescription: "This enables SpamSum calculation for each of the files. Adding this flag overrides the default hashing behavior of including CRC32, MD5, and SHA-1 hashes.");
            }
        }

        internal const string IndividualValue = "individual";
        internal static Feature IndividualFlag
        {
            get
            {
                return new Feature(
                    IndividualValue,
                    new List<string>() { "-ind", "--individual" },
                    "Process input DATs individually",
                    ParameterType.Flag,
                    longDescription: "In cases where DATs would be processed in bulk, this flag allows them to be processed on their own instead.");
            }
        }

        internal const string InplaceValue = "inplace";
        internal static Feature InplaceFlag
        {
            get
            {
                return new Feature(
                    InplaceValue,
                    new List<string>() { "-ip", "--inplace" },
                    "Write to the input directories, where possible",
                    ParameterType.Flag,
                    longDescription: "By default, files are written to the runtime directory (or the output directory, if set). This flag enables users to write out to the directory that the DATs originated from.");
            }
        }

        internal const string InverseValue = "inverse";
        internal static Feature InverseFlag
        {
            get
            {
                return new Feature(
                    InverseValue,
                    new List<string>() { "-in", "--inverse" },
                    "Rebuild only files not in DAT",
                    ParameterType.Flag,
                    longDescription: "Instead of the normal behavior of rebuilding using a DAT, this flag allows the user to use the DAT as a filter instead. All files that are found in the DAT will be skipped and everything else will be output in the selected format.");
            }
        }

        internal const string KeepEmptyGamesValue = "keep-empty-games";
        internal static Feature KeepEmptyGamesFlag
        {
            get
            {
                return new Feature(
                    KeepEmptyGamesValue,
                    new List<string>() { "-keg", "--keep-empty-games" },
                    "Keep originally empty sets from the input(s)",
                    ParameterType.Flag,
                    longDescription: "Normally, any sets that are considered empty will not be included in the output, this flag allows these empty sets to be added to the output.");
            }
        }

        internal const string LevelValue = "level";
        internal static Feature LevelFlag
        {
            get
            {
                return new Feature(
                    LevelValue,
                    new List<string>() { "-ls", "--level" },
                    "Split a SuperDAT or folder by lowest available level",
                    ParameterType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on the lowest available level of game name. That is, if a game name is top/mid/last, then it will create an output DAT for the parent directory \"mid\" in a folder called \"top\" with a game called \"last\".");
            }
        }

        internal const string MatchOfTagsValue = "match-of-tags";
        internal static Feature MatchOfTagsFlag
        {
            get
            {
                return new Feature(
                    MatchOfTagsValue,
                    new List<string>() { "-ofg", "--match-of-tags" },
                    "Allow cloneof and romof tags to match game name filters",
                    ParameterType.Flag,
                    longDescription: "If filter or exclude by game name is used, this flag will allow those filters to be checked against the romof and cloneof tags as well. This can allow for more advanced set-building, especially in arcade-based sets.");
            }
        }

        internal const string MergeValue = "merge";
        internal static Feature MergeFlag
        {
            get
            {
                return new Feature(
                    MergeValue,
                    new List<string>() { "-m", "--merge" },
                    "Merge the input DATs",
                    ParameterType.Flag,
                    longDescription: "By default, all DATs are processed individually with the user-specified flags. With this flag enabled, all of the input DATs are merged into a single output. This is best used with the dedup flag.");
            }
        }

        internal const string NoAutomaticDateValue = "no-automatic-date";
        internal static Feature NoAutomaticDateFlag
        {
            get
            {
                return new Feature(
                    NoAutomaticDateValue,
                    new List<string>() { "-b", "--no-automatic-date" },
                    "Don't include date in file name",
                    ParameterType.Flag,
                    longDescription: "Normally, the DAT will be created with the date in the file name in brackets. This flag removes that instead of the default.");
            }
        }

        internal const string NodumpColumnValue = "nodump-column";
        internal static Feature NodumpColumnFlag
        {
            get
            {
                return new Feature(
                    NodumpColumnValue,
                    new List<string>() { "-nc", "--nodump-column" },
                    "Add statistics for nodumps to output",
                    ParameterType.Flag,
                    longDescription: "Add a new column or field for counting the number of nodumps in the DAT.");
            }
        }

        internal const string NoStoreHeaderValue = "no-store-header";
        internal static Feature NoStoreHeaderFlag
        {
            get
            {
                return new Feature(
                    NoStoreHeaderValue,
                    new List<string>() { "-nsh", "--no-store-header" },
                    "Don't store the extracted header",
                    ParameterType.Flag,
                    longDescription: "By default, all headers that are removed from files are backed up in the database. This flag allows users to skip that step entirely, avoiding caching the headers at all.");
            }
        }

        internal const string OneGamePerRegionValue = "one-game-per-region";
        internal static Feature OneGamePerRegionFlag
        {
            get
            {
                return new Feature(
                    OneGamePerRegionValue,
                    new List<string>() { "-1g1r", "--one-game-per-region" },
                    "Try to ensure one game per user-defined region",
                    ParameterType.Flag,
                    longDescription: "This allows users to input a list of regions to use to filter on in order so only one game from each set of parent and clones will be included. This requires either cloneof or romof tags to function properly.");
            }
        }

        internal const string OneRomPerGameValue = "one-rom-per-game";
        internal static Feature OneRomPerGameFlag
        {
            get
            {
                return new Feature(
                    OneRomPerGameValue,
                    new List<string>() { "-orpg", "--one-rom-per-game" },
                    "Try to ensure each rom has its own game",
                    ParameterType.Flag,
                    longDescription: "In some cases, it is beneficial to have every rom put into its own output set as a subfolder of the original parent. This flag enables outputting each rom to its own game for this purpose.");
            }
        }

        internal const string OnlySameValue = "only-same";
        internal static Feature OnlySameFlag
        {
            get
            {
                return new Feature(
                    OnlySameValue,
                    new List<string>() { "-ons", "--only-same" },
                    "Only update description if machine name matches description",
                    ParameterType.Flag,
                    longDescription: "Normally, updating the description will always overwrite if the machine names are the same. With this flag, descriptions will only be overwritten if they are the same as the machine names.");
            }
        }

        internal const string QuickValue = "quick";
        internal static Feature QuickFlag
        {
            get
            {
                return new Feature(
                    QuickValue,
                    new List<string>() { "-qs", "--quick" },
                    "Enable quick scanning of archives",
                    ParameterType.Flag,
                    longDescription: "For all archives, if this flag is enabled, it will only use the header information to get the archive entries' file information. The upside to this is that it is the fastest option. On the downside, it can only get the CRC and size from most archive formats, leading to possible issues.");
            }
        }

        internal const string QuotesValue = "quotes";
        internal static Feature QuotesFlag
        {
            get
            {
                return new Feature(
                    QuotesValue,
                    new List<string>() { "-q", "--quotes" },
                    "Double-quote each item",
                    ParameterType.Flag,
                    longDescription: "This flag surrounds the item by double-quotes, not including the prefix or postfix.");
            }
        }

        internal const string RemoveExtensionsValue = "remove-extensions";
        internal static Feature RemoveExtensionsFlag
        {
            get
            {
                return new Feature(
                    RemoveExtensionsValue,
                    new List<string>() { "-rme", "--remove-extensions" },
                    "Remove all extensions from all items",
                    ParameterType.Flag,
                    longDescription: "For each item, remove the extension.");
            }
        }

        internal const string RemoveUnicodeValue = "remove-unicode";
        internal static Feature RemoveUnicodeFlag
        {
            get
            {
                return new Feature(
                    RemoveUnicodeValue,
                    new List<string>() { "-ru", "--remove-unicode" },
                    "Remove unicode characters from names",
                    ParameterType.Flag,
                    longDescription: "By default, the character set from the original file(s) will be used for item naming. This flag removes all Unicode characters from the item names, machine names, and machine descriptions.");
            }
        }

        internal const string ReverseBaseReplaceValue = "reverse-base-replace";
        internal static Feature ReverseBaseReplaceFlag
        {
            get
            {
                return new Feature(
                    ReverseBaseReplaceValue,
                    new List<string>() { "-rbr", "--reverse-base-replace" },
                    "Replace item names from base DATs in reverse",
                    ParameterType.Flag,
                    longDescription: "By default, no item names are changed except when there is a merge occurring. This flag enables users to define a DAT or set of base DATs to use as \"replacements\" for all input DATs. Note that the first found instance of an item in the last base DAT(s) will be used and all others will be discarded. If no additional flag is given, it will default to updating names.");
            }
        }

        internal const string RombaValue = "romba";
        internal static Feature RombaFlag
        {
            get
            {
                return new Feature(
                    RombaValue,
                    new List<string>() { "-ro", "--romba" },
                    "Treat like a Romba depot (requires SHA-1)",
                    ParameterType.Flag,
                    longDescription: "This flag allows reading and writing of DATs and output files to and from a Romba-style depot. This also implies TorrentGZ input and output for physical files. Where appropriate, Romba depot files will be created as well.");
            }
        }

        internal const string RomsValue = "roms";
        internal static Feature RomsFlag
        {
            get
            {
                return new Feature(
                    RomsValue,
                    new List<string>() { "-r", "--roms" },
                    "Output roms to miss instead of sets",
                    ParameterType.Flag,
                    longDescription: "By default, the outputted file will include the name of the game so this flag allows for the name of the rom to be output instead. [Missfile only]");
            }
        }

        internal const string SceneDateStripValue = "scene-date-strip";
        internal static Feature SceneDateStripFlag
        {
            get
            {
                return new Feature(
                    SceneDateStripValue,
                    new List<string>() { "-sds", "--scene-date-strip" },
                    "Remove date from scene-named sets",
                    ParameterType.Flag,
                    longDescription: "If this flag is enabled, sets with \"scene\" names will have the date removed from the beginning. For example \"01.01.01-Game_Name-GROUP\" would become \"Game_Name-Group\".");
            }
        }

        internal const string ScriptValue = "script";
        internal static Feature ScriptFlag
        {
            get
            {
                return new Feature(
                    ScriptValue,
                    new List<string>() { "-sc", "--script" },
                    "Enable script mode (no clear screen)",
                    ParameterType.Flag,
                    "For times when SabreTools is being used in a scripted environment, the user may not want the screen to be cleared every time that it is called. This flag allows the user to skip clearing the screen on run just like if the console was being redirected.");
            }
        }

        internal const string ShortValue = "short";
        internal static Feature ShortFlag
        {
            get
            {
                return new Feature(
                    ShortValue,
                    new List<string>() { "-s", "--short" },
                    "Use short output names",
                    ParameterType.Flag,
                    longDescription: "Instead of using ClrMamePro-style long names for DATs, use just the name of the folder as the name of the DAT. This can be used in conjunction with --base to output in the format of \"Original Name (Name)\" instead.");
            }
        }

        internal const string SingleSetValue = "single-set";
        internal static Feature SingleSetFlag
        {
            get
            {
                return new Feature(
                    SingleSetValue,
                    new List<string>() { "-si", "--single-set" },
                    "All game names replaced by '!'",
                    ParameterType.Flag,
                    longDescription: "This is useful for keeping all roms in a DAT in the same archive or folder.");
            }
        }

        internal const string SizeValue = "size";
        internal static Feature SizeFlag
        {
            get
            {
                return new Feature(
                    SizeValue,
                    new List<string>() { "-szs", "--size" },
                    "Split DAT(s) or folder by file sizes",
                    ParameterType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on the sizes of the files, specifically if the type is a Rom (most item types don't have sizes).");
            }
        }

        internal const string SkipArchivesValue = "skip-archives";
        internal static Feature SkipArchivesFlag
        {
            get
            {
                return new Feature(
                    SkipArchivesValue,
                    new List<string>() { "-ska", "--skip-archives" },
                    "Skip all archives",
                    ParameterType.Flag,
                    longDescription: "Skip any files that are treated like archives");
            }
        }

        internal const string SkipFilesValue = "skip-files";
        internal static Feature SkipFilesFlag
        {
            get
            {
                return new Feature(
                    SkipFilesValue,
                    new List<string>() { "-skf", "--skip-files" },
                    "Skip all non-archives",
                    ParameterType.Flag,
                    longDescription: "Skip any files that are not treated like archives");
            }
        }

        internal const string SkipFirstOutputValue = "skip-first-output";
        internal static Feature SkipFirstOutputFlag
        {
            get
            {
                return new Feature(
                    SkipFirstOutputValue,
                    new List<string>() { "-sf", "--skip-first-output" },
                    "Skip output of first DAT",
                    ParameterType.Flag,
                    longDescription: "In times where the first DAT does not need to be written out a second time, this will skip writing it. This can often speed up the output process.");
            }
        }

        internal const string SuperdatValue = "superdat";
        internal static Feature SuperdatFlag
        {
            get
            {
                return new Feature(
                    SuperdatValue,
                    new List<string>() { "-sd", "--superdat" },
                    "Enable SuperDAT creation",
                    ParameterType.Flag,
                    longDescription: "Set the type flag to \"SuperDAT\" for the output DAT as well as preserving the directory structure of the inputted folder, if applicable.");
            }
        }

        internal const string TarValue = "tar";
        internal static Feature TarFlag
        {
            get
            {
                return new Feature(
                    TarValue,
                    new List<string>() { "-tar", "--tar" },
                    "Enable Tape ARchive output",
                    ParameterType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to Tape ARchive (TAR) files. This format is a standardized storage archive without any compression, usually used with other compression formats around it. It is widely used in backup applications and source code archives.");
            }
        }

        internal const string Torrent7zipValue = "torrent-7zip";
        internal static Feature Torrent7zipFlag
        {
            get
            {
                return new Feature(
                    Torrent7zipValue,
                    new List<string>() { "-t7z", "--torrent-7zip" },
                    "Enable Torrent7Zip output",
                    ParameterType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent7Zip (T7Z) files. This format is based on the LZMA container format 7Zip, but with custom header information. This is currently unused by any major application. Currently does not produce proper Torrent-compatible outputs.");
            }
        }

        internal const string TorrentGzipValue = "torrent-gzip";
        internal static Feature TorrentGzipFlag
        {
            get
            {
                return new Feature(
                    TorrentGzipValue,
                    new List<string>() { "-tgz", "--torrent-gzip" },
                    "Enable Torrent GZip output",
                    ParameterType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to TorrentGZ (TGZ) files. This format is based on the GZip archive format, but with custom header information and a file name replaced by the SHA-1 of the file inside. This is primarily used by external tool Romba (https://github.com/uwedeportivo/romba), but may be used more widely in the future.");
            }
        }

        internal const string TorrentZipValue = "torrent-zip";
        internal static Feature TorrentZipFlag
        {
            get
            {
                return new Feature(
                    TorrentZipValue,
                    new List<string>() { "-tzip", "--torrent-zip" },
                    "Enable Torrent Zip output",
                    ParameterType.Flag,
                    longDescription: "Instead of outputting files to folder, files will be rebuilt to TorrentZip (TZip) files. This format is based on the ZIP archive format, but with custom header information. This is primarily used by external tool RomVault (http://www.romvault.com/) and is already widely used.");
            }
        }

        internal const string TotalSizeValue = "total-size";
        internal static Feature TotalSizeFlag
        {
            get
            {
                return new Feature(
                    TotalSizeValue,
                    new List<string>() { "-tis", "--total-size" },
                    "Split DAT(s) or folder by total game sizes",
                    ParameterType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on the combined sizes of the games, splitting into individual chunks.");
            }
        }

        internal const string TrimValue = "trim";
        internal static Feature TrimFlag
        {
            get
            {
                return new Feature(
                    TrimValue,
                    new List<string>() { "-trim", "--trim" },
                    "Trim file names to fit NTFS length",
                    ParameterType.Flag,
                    longDescription: "In the cases where files will have too long a name, this allows for trimming the name of the files to the NTFS maximum length at most.");
            }
        }

        internal const string TypeValue = "type";
        internal static Feature TypeFlag
        {
            get
            {
                return new Feature(
                    TypeValue,
                    new List<string>() { "-ts", "--type" },
                    "Split DAT(s) or folder by file types (rom/disk)",
                    ParameterType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on the types of the files, specifically if the type is a rom or a disk.");
            }
        }

        internal const string UpdateDatValue = "update-dat";
        internal static Feature UpdateDatFlag
        {
            get
            {
                return new Feature(
                    UpdateDatValue,
                    new List<string>() { "-ud", "--update-dat" },
                    "Output updated DAT to output directory",
                    ParameterType.Flag,
                    longDescription: "Once the files that were able to rebuilt are taken care of, a DAT of the files that could not be matched will be output to the output directory.");
            }
        }

        #endregion

        #region Int32 features

        internal const string DepotDepthInt32Value = "depot-depth";
        internal static Feature DepotDepthInt32Input
        {
            get
            {
                return new Feature(
                    DepotDepthInt32Value,
                    new List<string>() { "-depd", "--depot-depth" },
                    "Set depth of depot for inputs",
                    ParameterType.Int32,
                    longDescription: "Optionally, set the depth of input depots. Defaults to 4 deep otherwise.");
            }
        }

        internal const string RombaDepthInt32Value = "romba-depth";
        internal static Feature RombaDepthInt32Input
        {
            get
            {
                return new Feature(
                    RombaDepthInt32Value,
                    new List<string>() { "-depr", "--romba-depth" },
                    "Set depth of depot for outputs",
                    ParameterType.Int32,
                    longDescription: "Optionally, set the depth of output depots. Defaults to 4 deep otherwise.");
            }
        }

        internal const string ThreadsInt32Value = "threads";
        internal static Feature ThreadsInt32Input
        {
            get
            {
                return new Feature(
                    ThreadsInt32Value,
                    new List<string>() { "-mt", "--threads" },
                    "Amount of threads to use (default = # cores)",
                    ParameterType.Int32,
                    longDescription: "Optionally, set the number of threads to use for the multithreaded operations. The default is the number of available machine threads; -1 means unlimited threads created.");
            }
        }

        #endregion

        #region Int64 features

        internal const string ChunkSizeInt64Value = "chunk-size";
        internal static Feature ChunkSizeInt64Input
        {
            get
            {
                return new Feature(
                    ChunkSizeInt64Value,
                    new List<string>() { "-cs", "--chunk-size" },
                    "Set a chunk size to output",
                    ParameterType.Int64,
                    longDescription: "Set the total game size to cut off at for each chunked DAT. It is recommended to use a sufficiently large size such as 1GB or else you may run into issues, especially if a single game could be larger than the size provided.");
            }
        }

        internal const string RadixInt64Value = "radix";
        internal static Feature RadixInt64Input
        {
            get
            {
                return new Feature(
                    RadixInt64Value,
                    new List<string>() { "-rad", "--radix" },
                    "Set the midpoint to split at",
                    ParameterType.Int64,
                    longDescription: "Set the size at which all roms less than the size are put in the first DAT, and everything greater than or equal goes in the second.");
            }
        }

        #endregion

        #region List<string> features

        internal const string BaseDatListValue = "base-dat";
        internal static Feature BaseDatListInput
        {
            get
            {
                return new Feature(
                    BaseDatListValue,
                    new List<string>() { "-bd", "--base-dat" },
                    "Add a base DAT for processing",
                    ParameterType.List,
                    longDescription: "Add a DAT or folder of DATs to the base set to be used for all operations. Multiple instances of this flag are allowed.");
            }
        }

        internal const string DatListValue = "dat";
        internal static Feature DatListInput
        {
            get
            {
                return new Feature(
                    DatListValue,
                    new List<string>() { "-dat", "--dat" },
                    "Input DAT to be used",
                    ParameterType.List,
                    longDescription: "User-supplied DAT for use in all operations. Multiple instances of this flag are allowed.");
            }
        }

        internal const string ExcludeFieldListValue = "exclude-field";
        internal static Feature ExcludeFieldListInput
        {
            get
            {
                return new Feature(
                    ExcludeFieldListValue,
                    new List<string>() { "-ef", "--exclude-field" },
                    "Exclude a game/rom field from outputs",
                    ParameterType.List,
                    longDescription: "Exclude any valid item or machine field from outputs. Examples include: romof, publisher, and offset.");
            }
        }

        internal const string ExtAListValue = "exta";
        internal static Feature ExtaListInput
        {
            get
            {
                return new Feature(
                    ExtAListValue,
                    new List<string>() { "-exta", "--exta" },
                    "Set extension to be included in first DAT",
                    ParameterType.List,
                    longDescription: "Set the extension to be used to populate the first DAT. Multiple instances of this flag are allowed.");
            }
        }

        internal const string ExtBListValue = "extb";
        internal static Feature ExtbListInput
        {
            get
            {
                return new Feature(
                    ExtBListValue,
                    new List<string>() { "-extb", "--extb" },
                    "Set extension to be included in second DAT",
                    ParameterType.List,
                    longDescription: "Set the extension to be used to populate the second DAT. Multiple instances of this flag are allowed.");
            }
        }

        internal const string ExtraIniListValue = "extra-ini";
        internal static Feature ExtraIniListInput
        {
            get
            {
                return new Feature(
                    ExtraIniListValue,
                    new List<string>() { "-ini", "--extra-ini" },
                    "Apply a MAME INI for given field(s)",
                    ParameterType.List,
                    longDescription: "Apply any valid MAME INI for any valid field in the DatFile. Inputs are of the form 'Field:path\\to\\ini'. Multiple instances of this flag are allowed.");
            }
        }

        internal const string FilterListValue = "filter";
        internal static Feature FilterListInput
        {
            get
            {
                return new Feature(
                    FilterListValue,
                    new List<string>() { "-fi", "--filter" },
                    "Filter a game/rom field with the given value(s)",
                    ParameterType.List,
                    longDescription: "Filter any valid item or machine field from inputs. Filters are input in the form 'key:value' or '!key:value', where the '!' signifies 'not matching'. Numeric values may also prefix the 'value' with '>', '<', or '=' accordingly. Key examples include: romof, category, and game. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        internal const string OutputTypeListValue = "output-type";
        internal static Feature OutputTypeListInput
        {
            get
            {
                return new Feature(
                    OutputTypeListValue,
                    new List<string>() { "-ot", "--output-type" },
                    "Output DATs to a specified format",
                    ParameterType.List,
                    longDescription: @"Add outputting the created DAT to known format. Multiple instances of this flag are allowed.

Possible values are:
    all              - All available DAT types
    ado, archive     - Archive.org file list
    am, attractmode  - AttractMode XML
    cmp, clrmamepro  - ClrMamePro
    csv              - Standardized Comma-Separated Value
    dc, doscenter    - DOSCenter
    lr, listrom      - MAME Listrom
    lx, listxml      - MAME Listxml
    miss, missfile   - GoodTools Missfile
    md5              - MD5
    msx, openmsx     - openMSX Software List
    ol, offlinelist  - OfflineList XML
    rc, romcenter    - RomCenter
    sj, sabrejson    - SabreJSON
    sx, sabrexml     - SabreDAT XML
    sfv              - SFV
    sha1             - SHA1
    sha256           - SHA256
    sha384           - SHA384
    sha512           - SHA512
    smdb, everdrive  - Everdrive SMDB
    sl, softwarelist - MAME Software List XML
    spamsum          - SpamSum
    ssv              - Standardized Semicolon-Separated Value
    tsv              - Standardized Tab-Separated Value
    xml, logiqx      - Logiqx XML");
            }
        }

        internal const string RegionListValue = "region";
        internal static Feature RegionListInput
        {
            get
            {
                return new Feature(
                    RegionListValue,
                    new List<string>() { "-reg", "--region" },
                    "Add a region for 1G1R",
                    ParameterType.List,
                    longDescription: "Add a region (in order) for use with 1G1R filtering. If this is not supplied, then by default, only parent sets will be included in the output. Multiple instances of this flag are allowed.");
            }
        }

        internal const string ReportTypeListValue = "report-type";
        internal static Feature ReportTypeListInput
        {
            get
            {
                return new Feature(
                    ReportTypeListValue,
                    new List<string>() { "-srt", "--report-type" },
                    "Output statistics to a specified format",
                    ParameterType.List,
                    longDescription: @"Add outputting the created DAT to known format. Multiple instances of this flag are allowed.

Possible values are:
    all              - All available DAT types
    csv              - Standardized Comma-Separated Value
    html             - HTML webpage
    ssv              - Standardized Semicolon-Separated Value
    text             - Generic textfile
    tsv              - Standardized Tab-Separated Value");
            }
        }

        internal const string UpdateFieldListValue = "update-field";
        internal static Feature UpdateFieldListInput
        {
            get
            {
                return new Feature(
                    UpdateFieldListValue,
                    new List<string>() { "-uf", "--update-field" },
                    "Update a game/rom field from base DATs",
                    ParameterType.List,
                    longDescription: "Update any valid item or machine field from base DAT(s). Examples include: romof, publisher, and offset.");
            }
        }

        #endregion

        #region String features

        internal const string AddExtensionStringValue = "add-extension";
        internal static Feature AddExtensionStringInput
        {
            get
            {
                return new Feature(
                    AddExtensionStringValue,
                    new List<string>() { "-ae", "--add-extension" },
                    "Add an extension to each item",
                    ParameterType.String,
                    longDescription: "Add a postfix extension to each full item name.");
            }
        }

        internal const string AuthorStringValue = "author";
        internal static Feature AuthorStringInput
        {
            get
            {
                return new Feature(
                    AuthorStringValue,
                    new List<string>() { "-au", "--author" },
                    "Set the author of the DAT",
                    ParameterType.String,
                    longDescription: "Set the author header field for the output DAT(s)");
            }
        }

        internal const string CategoryStringValue = "category";
        internal static Feature CategoryStringInput
        {
            get
            {
                return new Feature(
                    CategoryStringValue,
                    new List<string>() { "-c", "--category" },
                    "Set the category of the DAT",
                    ParameterType.String,
                    longDescription: "Set the category header field for the output DAT(s)");
            }
        }

        internal const string CommentStringValue = "comment";
        internal static Feature CommentStringInput
        {
            get
            {
                return new Feature(
                    CommentStringValue,
                    new List<string>() { "-co", "--comment" },
                    "Set a new comment of the DAT",
                    ParameterType.String,
                    longDescription: "Set the comment header field for the output DAT(s)");
            }
        }

        internal const string DateStringValue = "date";
        internal static Feature DateStringInput
        {
            get
            {
                return new Feature(
                    DateStringValue,
                    new List<string>() { "-da", "--date" },
                    "Set a new date",
                    ParameterType.String,
                    longDescription: "Set the date header field for the output DAT(s)");
            }
        }

        internal const string DescriptionStringValue = "description";
        internal static Feature DescriptionStringInput
        {
            get
            {
                return new Feature(
                    DescriptionStringValue,
                    new List<string>() { "-de", "--description" },
                    "Set the description of the DAT",
                    ParameterType.String,
                    longDescription: "Set the description header field for the output DAT(s)");
            }
        }

        internal const string EmailStringValue = "email";
        internal static Feature EmailStringInput
        {
            get
            {
                return new Feature(
                    EmailStringValue,
                    new List<string>() { "-em", "--email" },
                    "Set a new email of the DAT",
                    ParameterType.String,
                    longDescription: "Set the email header field for the output DAT(s)");
            }
        }

        internal const string FilenameStringValue = "filename";
        internal static Feature FilenameStringInput
        {
            get
            {
                return new Feature(
                    FilenameStringValue,
                    new List<string>() { "-f", "--filename" },
                    "Set the external name of the DAT",
                    ParameterType.String,
                    longDescription: "Set the external filename for the output DAT(s)");
            }
        }

        internal const string ForceMergingStringValue = "forcemerging";
        internal static Feature ForceMergingStringInput
        {
            get
            {
                return new Feature(
                    ForceMergingStringValue,
                    new List<string>() { "-fm", "--forcemerging" },
                    "Set force merging",
                    ParameterType.String,
                    longDescription: @"Set the forcemerging tag to the given value.
Possible values are: None, Split, Device, Merged, Nonmerged, Full");
            }
        }

        internal const string ForceNodumpStringValue = "forcenodump";
        internal static Feature ForceNodumpStringInput
        {
            get
            {
                return new Feature(
                    ForceNodumpStringValue,
                    new List<string>() { "-fn", "--forcenodump" },
                    "Set force nodump",
                    ParameterType.String,
                    longDescription: @"Set the forcenodump tag to the given value.
Possible values are: None, Obsolete, Required, Ignore");
            }
        }

        internal const string ForcePackingStringValue = "forcepacking";
        internal static Feature ForcePackingStringInput
        {
            get
            {
                return new Feature(
                    ForcePackingStringValue,
                    new List<string>() { "-fp", "--forcepacking" },
                    "Set force packing",
                    ParameterType.String,
                    longDescription: @"Set the forcepacking tag to the given value.
Possible values are: None, Zip, Unzip, Partial, Flat");
            }
        }

        internal const string HeaderStringValue = "header";
        internal static Feature HeaderStringInput
        {
            get
            {
                return new Feature(
                    HeaderStringValue,
                    new List<string>() { "-h", "--header" },
                    "Set a header skipper to use, blank means all",
                    ParameterType.String,
                    longDescription: "Set the header special field for the output DAT(s). In file rebuilding, this flag allows for either all copier headers (using \"\") or specific copier headers by name (such as \"fds.xml\") to determine if a file matches or not.");

            }
        }

        internal const string HomepageStringValue = "homepage";
        internal static Feature HomepageStringInput
        {
            get
            {
                return new Feature(
                    HomepageStringValue,
                    new List<string>() { "-hp", "--homepage" },
                    "Set a new homepage of the DAT",
                    ParameterType.String,
                    longDescription: "Set the homepage header field for the output DAT(s)");
            }
        }

        internal const string LogLevelStringValue = "log-level";
        internal static Feature LogLevelStringInput
        {
            get
            {
                return new Feature(
                    LogLevelStringValue,
                    new List<string>() { "-ll", "--log-level" },
                    "Set the lowest log level for output",
                    ParameterType.String,
                    longDescription: @"Set the lowest log level for output.
Possible values are: Verbose, User, Warning, Error");
            }
        }

        internal const string NameStringValue = "name";
        internal static Feature NameStringInput
        {
            get
            {
                return new Feature(
                    NameStringValue,
                    new List<string>() { "-n", "--name" },
                    "Set the internal name of the DAT",
                    ParameterType.String,
                    longDescription: "Set the name header field for the output DAT(s)");
            }
        }

        internal const string OutputDirStringValue = "output-dir";
        internal static Feature OutputDirStringInput
        {
            get
            {
                return new Feature(
                    OutputDirStringValue,
                    new List<string>() { "-out", "--output-dir" },
                    "Set output directory",
                    ParameterType.String,
                    longDescription: "This sets an output folder to be used when the files are created. If a path is not defined, the runtime directory is used instead.");
            }
        }

        internal const string PostfixStringValue = "postfix";
        internal static Feature PostfixStringInput
        {
            get
            {
                return new Feature(
                    PostfixStringValue,
                    new List<string>() { "-post", "--postfix" },
                    "Set postfix for all lines",
                    ParameterType.String,
                    longDescription: @"Set a generic postfix to be appended to all outputted lines.

Some special strings that can be used:
- %game% / %machine% - Replaced with the Game/Machine name
- %name% - Replaced with the Rom name
- %manufacturer% - Replaced with game Manufacturer
- %publisher% - Replaced with game Publisher
- %category% - Replaced with game Category
- %crc% - Replaced with the CRC
- %md5% - Replaced with the MD5
- %sha1% - Replaced with the SHA-1
- %sha256% - Replaced with the SHA-256
- %sha384% - Replaced with the SHA-384
- %sha512% - Replaced with the SHA-512
- %size% - Replaced with the size");
            }
        }

        internal const string PrefixStringValue = "prefix";
        internal static Feature PrefixStringInput
        {
            get
            {
                return new Feature(
                    PrefixStringValue,
                    new List<string>() { "-pre", "--prefix" },
                    "Set prefix for all lines",
                    ParameterType.String,
                    longDescription: @"Set a generic prefix to be prepended to all outputted lines.

Some special strings that can be used:
- %game% / %machine% - Replaced with the Game/Machine name
- %name% - Replaced with the Rom name
- %manufacturer% - Replaced with game Manufacturer
- %publisher% - Replaced with game Publisher
- %category% - Replaced with game Category
- %crc% - Replaced with the CRC
- %md5% - Replaced with the MD5
- %sha1% - Replaced with the SHA-1
- %sha256% - Replaced with the SHA-256
- %sha384% - Replaced with the SHA-384
- %sha512% - Replaced with the SHA-512
- %size% - Replaced with the size");
            }
        }

        internal const string ReplaceExtensionStringValue = "replace-extension";
        internal static Feature ReplaceExtensionStringInput
        {
            get
            {
                return new Feature(
                    ReplaceExtensionStringValue,
                    new List<string>() { "-rep", "--replace-extension" },
                    "Replace all extensions with specified",
                    ParameterType.String,
                    longDescription: "When an extension exists, replace it with the provided instead.");
            }
        }

        internal const string RootStringValue = "root";
        internal static Feature RootStringInput
        {
            get
            {
                return new Feature(
                    RootStringValue,
                    new List<string>() { "-r", "--root" },
                    "Set a new rootdir",
                    ParameterType.String,
                    longDescription: "Set the rootdir (as used by SuperDAT mode) for the output DAT(s).");
            }
        }

        internal const string RootDirStringValue = "root-dir";
        internal static Feature RootDirStringInput
        {
            get
            {
                return new Feature(
                    RootDirStringValue,
                    new List<string>() { "-rd", "--root-dir" },
                    "Set the root directory for calc",
                    ParameterType.String,
                    longDescription: "In the case that the files will not be stored from the root directory, a new root can be set for path length calculations.");
            }
        }

        internal const string SymlinkDirStringValue = "symlink-dir";
        internal static Feature SymlinkStringInput
        {
            get
            {
                return new Feature(
                    SymlinkDirStringValue,
                    new List<string>() { "-sd", "--symlink-dir" },
                    "Set the root directory for calc",
                    ParameterType.String,
                    longDescription: "In the case that the files will not be stored from the root directory, a new root can be set for path length calculations.");
            }
        }
        
        internal const string UrlStringValue = "url";
        internal static Feature UrlStringInput
        {
            get
            {
                return new Feature(
                    UrlStringValue,
                    new List<string>() { "-u", "--url" },
                    "Set a new URL of the DAT",
                    ParameterType.String,
                    longDescription: "Set the URL header field for the output DAT(s)");
            }
        }

        internal const string VersionStringValue = "version";
        internal static Feature VersionStringInput
        {
            get
            {
                return new Feature(
                    VersionStringValue,
                    new List<string>() { "-v", "--version" },
                    "Set the version of the DAT",
                    ParameterType.String,
                    longDescription: "Set the version header field for the output DAT(s)");
            }
        }

        #endregion

        #endregion

        #region Fields

        /// <summary>
        /// Preconfigured Cleaner
        /// </summary>
        protected Cleaner Cleaner { get; set; }

        /// <summary>
        /// Preconfigured ExtraIni set
        /// </summary>
        protected ExtraIni Extras { get; set; }

        /// <summary>
        /// Preconfigured Filter
        /// </summary>
        protected Filter Filter { get; set; }

        /// <summary>
        /// Pre-configured DatHeader
        /// </summary>
        /// <remarks>Public because it's an indicator something went wrong</remarks>
        public DatHeader Header { get; set; }

        /// <summary>
        /// Lowest log level for output
        /// </summary>
        public LogLevel LogLevel { get; protected set; }

        /// <summary>
        /// Output directory
        /// </summary>
        protected string OutputDir { get; set; }

        /// <summary>
        /// Pre-configured Remover
        /// </summary>
        protected Remover Remover { get; set; }
        
        /// <summary>
        /// Determines if scripting mode is enabled
        /// </summary>
        public bool ScriptMode { get; protected set; }

        /// <summary>
        /// Pre-configured Splitter
        /// </summary>
        protected Filtering.Splitter Splitter { get; set; }

        #endregion

        #region Add Feature Groups

        /// <summary>
        /// Add common features
        /// </summary>
        protected void AddCommonFeatures()
        {
            AddFeature(ScriptFlag);
            AddFeature(LogLevelStringInput);
            AddFeature(ThreadsInt32Input);
        }

        /// <summary>
        /// Add Filter-specific features
        /// </summary>
        protected void AddFilteringFeatures()
        {
            AddFeature(FilterListInput);
            AddFeature(MatchOfTagsFlag);
        }

        /// <summary>
        /// Add Header-specific features
        /// </summary>
        protected void AddHeaderFeatures()
        {
            // Header Values
            AddFeature(FilenameStringInput);
            AddFeature(NameStringInput);
            AddFeature(DescriptionStringInput);
            AddFeature(RootStringInput);
            AddFeature(CategoryStringInput);
            AddFeature(VersionStringInput);
            AddFeature(DateStringInput);
            AddFeature(AuthorStringInput);
            AddFeature(EmailStringInput);
            AddFeature(HomepageStringInput);
            AddFeature(UrlStringInput);
            AddFeature(CommentStringInput);
            AddFeature(HeaderStringInput);
            AddFeature(SuperdatFlag);
            AddFeature(ForceMergingStringInput);
            AddFeature(ForceNodumpStringInput);
            AddFeature(ForcePackingStringInput);

            // Header Filters
            AddFeature(ExcludeFieldListInput);
            AddFeature(OneGamePerRegionFlag);
            this[OneGamePerRegionFlag].AddFeature(RegionListInput);
            AddFeature(OneRomPerGameFlag);
            AddFeature(SceneDateStripFlag);
        }

        /// <summary>
        /// Add internal split/merge features
        /// </summary>
        protected void AddInternalSplitFeatures()
        {
            AddFeature(DatMergedFlag);
            AddFeature(DatSplitFlag);
            AddFeature(DatNonMergedFlag);
            AddFeature(DatDeviceNonMergedFlag);
            AddFeature(DatFullNonMergedFlag);
        }

        #endregion

        public override bool ProcessFeatures(Dictionary<string, Feature> features)
        {
            // Generic feature flags
            Cleaner = GetCleaner(features);
            Extras = GetExtras(features);
            Filter = GetFilter(features);
            Header = GetDatHeader(features);
            LogLevel = GetString(features, LogLevelStringValue).AsLogLevel();
            OutputDir = GetString(features, OutputDirStringValue)?.Trim('"');
            Remover = GetRemover(features);
            ScriptMode = GetBoolean(features, ScriptValue);
            Splitter = GetSplitter(features);

            // Set threading flag, if necessary
            if (features.ContainsKey(ThreadsInt32Value))
                Globals.MaxThreads = GetInt32(features, ThreadsInt32Value);
            
            // Failure conditions
            if (Header == null)
                return false;
            
            return true;
        }

        #region Protected Specific Extraction

        /// <summary>
        /// Get include from scan from feature list
        /// </summary>
        protected Hash GetIncludeInScan(Dictionary<string, Feature> features)
        {
            Hash includeInScan = 0x00;

            if (GetBoolean(features, IncludeCrcValue))
                includeInScan |= Hash.CRC;
            if (GetBoolean(features, IncludeMd5Value))
                includeInScan |= Hash.MD5;
            if (GetBoolean(features, IncludeSha1Value))
                includeInScan |= Hash.SHA1;
            if (GetBoolean(features, IncludeSha256Value))
                includeInScan |= Hash.SHA256;
            if (GetBoolean(features, IncludeSha384Value))
                includeInScan |= Hash.SHA384;
            if (GetBoolean(features, IncludeSha512Value))
                includeInScan |= Hash.SHA512;
            if (GetBoolean(features, IncludeSpamSumValue))
                includeInScan |= Hash.SpamSum;

            // Fallback to "Standard" if no flags are set
            if (includeInScan == 0x00)
                includeInScan = Hash.Standard;

            return includeInScan;
        }

        /// <summary>
        /// Get OutputFormat from feature list
        /// </summary>
        protected OutputFormat GetOutputFormat(Dictionary<string, Feature> features)
        {
            if (GetBoolean(features, TarValue))
                return OutputFormat.TapeArchive;
            else if (GetBoolean(features, Torrent7zipValue))
                return OutputFormat.Torrent7Zip;
            else if (GetBoolean(features, TorrentGzipValue))
                return OutputFormat.TorrentGzip;
            //else if (GetBoolean(features, SharedTorrentLrzipValue))
            //    return OutputFormat.TorrentLRZip;
            //else if (GetBoolean(features, SharedTorrentLz4Value))
            //    return OutputFormat.TorrentLZ4;
            //else if (GetBoolean(features, SharedTorrentRarValue))
            //    return OutputFormat.TorrentRar;
            //else if (GetBoolean(features, SharedTorrentXzValue))
            //    return OutputFormat.TorrentXZ;
            else if (GetBoolean(features, TorrentZipValue))
                return OutputFormat.TorrentZip;
            //else if (GetBoolean(features, SharedTorrentZpaqValue))
            //    return OutputFormat.TorrentZPAQ;
            //else if (GetBoolean(features, SharedTorrentZstdValue))
            //    return OutputFormat.TorrentZstd;
            else
                return OutputFormat.Folder;
        }

        /// <summary>
        /// Get SkipFileType from feature list
        /// </summary>
        protected SkipFileType GetSkipFileType(Dictionary<string, Feature> features)
        {
            if (GetBoolean(features, SkipArchivesValue))
                return SkipFileType.Archive;
            else if (GetBoolean(features, SkipFilesValue))
                return SkipFileType.File;
            else
                return SkipFileType.None;
        }

        /// <summary>
        /// Get SplittingMode from feature list
        /// </summary>
        protected SplittingMode GetSplittingMode(Dictionary<string, Feature> features)
        {
            SplittingMode splittingMode = SplittingMode.None;

            if (GetBoolean(features, ExtensionValue))
                splittingMode |= SplittingMode.Extension;
            if (GetBoolean(features, HashValue))
                splittingMode |= SplittingMode.Hash;
            if (GetBoolean(features, LevelValue))
                splittingMode |= SplittingMode.Level;
            if (GetBoolean(features, SizeValue))
                splittingMode |= SplittingMode.Size;
            if (GetBoolean(features, TotalSizeValue))
                splittingMode |= SplittingMode.TotalSize;
            if (GetBoolean(features, TypeValue))
                splittingMode |= SplittingMode.Type;

            return splittingMode;
        }

        /// <summary>
        /// Get StatReportFormat from feature list
        /// </summary>
        protected StatReportFormat GetStatReportFormat(Dictionary<string, Feature> features)
        {
            StatReportFormat statDatFormat = StatReportFormat.None;

            foreach (string rt in GetList(features, ReportTypeListValue))
            {
                statDatFormat |= GetStatReportFormat(rt);
            }

            return statDatFormat;
        }

        /// <summary>
        /// Get TreatAsFiles from feature list
        /// </summary>
        protected TreatAsFile GetTreatAsFiles(Dictionary<string, Feature> features)
        {
            TreatAsFile asFiles = 0x00;
            if (GetBoolean(features, AaruFormatsAsFilesValue))
                asFiles |= TreatAsFile.AaruFormat;
            if (GetBoolean(features, ArchivesAsFilesValue))
                asFiles |= TreatAsFile.Archive;
            if (GetBoolean(features, ChdsAsFilesValue))
                asFiles |= TreatAsFile.CHD;

            return asFiles;
        }

        /// <summary>
        /// Get update DatItem fields from feature list
        /// </summary>
        protected List<DatItemField> GetUpdateDatItemFields(Dictionary<string, Feature> features)
        {
            List<DatItemField> updateFields = new List<DatItemField>();
            foreach (string fieldName in GetList(features, UpdateFieldListValue))
            {
                updateFields.Add(fieldName.AsDatItemField());
            }

            return updateFields;
        }

        /// <summary>
        /// Get update Machine fields from feature list
        /// </summary>
        protected List<MachineField> GetUpdateMachineFields(Dictionary<string, Feature> features)
        {
            List<MachineField> updateFields = new List<MachineField>();
            foreach (string fieldName in GetList(features, UpdateFieldListValue))
            {
                updateFields.Add(fieldName.AsMachineField());
            }

            return updateFields;
        }

        /// <summary>
        /// Get UpdateMode from feature list
        /// </summary>
        protected UpdateMode GetUpdateMode(Dictionary<string, Feature> features)
        {
            UpdateMode updateMode = UpdateMode.None;

            if (GetBoolean(features, DiffAllValue))
                updateMode |= UpdateMode.AllDiffs;

            if (GetBoolean(features, BaseReplaceValue))
                updateMode |= UpdateMode.BaseReplace;

            if (GetBoolean(features, DiffAgainstValue))
                updateMode |= UpdateMode.DiffAgainst;

            if (GetBoolean(features, DiffCascadeValue))
                updateMode |= UpdateMode.DiffCascade;

            if (GetBoolean(features, DiffDuplicatesValue))
                updateMode |= UpdateMode.DiffDupesOnly;

            if (GetBoolean(features, DiffIndividualsValue))
                updateMode |= UpdateMode.DiffIndividualsOnly;

            if (GetBoolean(features, DiffNoDuplicatesValue))
                updateMode |= UpdateMode.DiffNoDupesOnly;

            if (GetBoolean(features, DiffReverseCascadeValue))
                updateMode |= UpdateMode.DiffReverseCascade;

            if (GetBoolean(features, MergeValue))
                updateMode |= UpdateMode.Merge;

            if (GetBoolean(features, ReverseBaseReplaceValue))
                updateMode |= UpdateMode.ReverseBaseReplace;

            return updateMode;
        }

        #endregion

        #region Private Specific Extraction

        /// <summary>
        /// Get Cleaner from feature list
        /// </summary>
        private Cleaner GetCleaner(Dictionary<string, Feature> features)
        {
            Cleaner cleaner = new Cleaner()
            {
                Clean = GetBoolean(features, CleanValue),
                DedupeRoms = GetDedupeType(features),
                DescriptionAsName = GetBoolean(features, DescriptionAsNameValue),
                KeepEmptyGames = GetBoolean(features, KeepEmptyGamesValue),
                OneGamePerRegion = GetBoolean(features, OneGamePerRegionValue),
                RegionList = GetList(features, RegionListValue),
                OneRomPerGame = GetBoolean(features, OneRomPerGameValue),
                RemoveUnicode = GetBoolean(features, RemoveUnicodeValue),
                Root = GetString(features, RootDirStringValue),
                SceneDateStrip = GetBoolean(features, SceneDateStripValue),
                Single = GetBoolean(features, SingleSetValue),
                Trim = GetBoolean(features, TrimValue),
            };

            return cleaner;
        }

        /// <summary>
        /// Get DatHeader from feature list
        /// </summary>
        private DatHeader GetDatHeader(Dictionary<string, Feature> features)
        {
            // TODO: Sort this by region, like the actual header
            DatHeader datHeader = new DatHeader
            {
                AddExtension = GetString(features, AddExtensionStringValue),
                Author = GetString(features, AuthorStringValue),
                Category = GetString(features, CategoryStringValue),
                Comment = GetString(features, CommentStringValue),
                Date = GetString(features, DateStringValue),
                Description = GetString(features, DescriptionStringValue),
                Email = GetString(features, EmailStringValue),
                FileName = GetString(features, FilenameStringValue),
                ForceMerging = GetString(features, ForceMergingStringValue).AsMergingFlag(),
                ForceNodump = GetString(features, ForceNodumpStringValue).AsNodumpFlag(),
                ForcePacking = GetString(features, ForcePackingStringValue).AsPackingFlag(),
                GameName = GetBoolean(features, GamePrefixValue),
                HeaderSkipper = GetString(features, HeaderStringValue),
                Homepage = GetString(features, HomepageStringValue),
                Name = GetString(features, NameStringValue),
                Postfix = GetString(features, PostfixStringValue),
                Prefix = GetString(features, PrefixStringValue),
                Quotes = GetBoolean(features, QuotesValue),
                RemoveExtension = GetBoolean(features, RemoveExtensionsValue),
                ReplaceExtension = GetString(features, ReplaceExtensionStringValue),
                RootDir = GetString(features, RootStringValue),
                Type = GetBoolean(features, SuperdatValue) ? "SuperDAT" : null,
                Url = GetString(features, UrlStringValue),
                UseRomName = GetBoolean(features, RomsValue),
                Version = GetString(features, VersionStringValue),
            };

            // Get the depot information
            datHeader.InputDepot = new DepotInformation(
                GetBoolean(features, DepotValue),
                GetInt32(features, DepotDepthInt32Value));
            datHeader.OutputDepot = new DepotInformation(
                GetBoolean(features, RombaValue),
                GetInt32(features, RombaDepthInt32Value));

            bool deprecated = GetBoolean(features, DeprecatedValue);
            foreach (string ot in GetList(features, OutputTypeListValue))
            {
                DatFormat dftemp = GetDatFormat(ot);
                if (dftemp == 0x00)
                {
                    logger.Error($"{ot} is not a recognized DAT format");
                    return null;
                }
                else if (dftemp == DatFormat.Logiqx && deprecated)
                {
                    datHeader.DatFormat |= DatFormat.LogiqxDeprecated;
                }
                else
                {
                    datHeader.DatFormat |= dftemp;
                }
            }

            return datHeader;
        }

        /// <summary>
        /// Get DedupeType from feature list
        /// </summary>
        private DedupeType GetDedupeType(Dictionary<string, Feature> features)
        {
            if (GetBoolean(features, DedupValue))
                return DedupeType.Full;
            else if (GetBoolean(features, GameDedupValue))
                return DedupeType.Game;
            else
                return DedupeType.None;
        }

        /// <summary>
        /// Get ExtraIni from feature list
        /// </summary>
        private ExtraIni GetExtras(Dictionary<string, Feature> features)
        {
            ExtraIni extraIni = new ExtraIni();
            extraIni.PopulateFromList(GetList(features, ExtraIniListValue));
            return extraIni;
        }

        /// <summary>
        /// Get Filter from feature list
        /// </summary>
        private Filter GetFilter(Dictionary<string, Feature> features)
        {
            Filter filter = new Filter()
            {
                DatItemFilter = new DatItemFilter(),
                MachineFilter = new MachineFilter(),
            };

            // Populate filters
            List<string> filterPairs = GetList(features, FilterListValue);
            filter.PopulateFiltersFromList(filterPairs);

            // Include 'of" in game filters
            filter.MachineFilter.IncludeOfInGame = GetBoolean(features, MatchOfTagsValue);

            return filter;
        }

        /// <summary>
        /// Get Remover from feature list
        /// </summary>
        private Remover GetRemover(Dictionary<string, Feature> features)
        {
            Remover remover = new Remover();
            
            // Populate field exclusions
            List<string> exclusionFields = GetList(features, ExcludeFieldListValue);
            remover.PopulateExclusionsFromList(exclusionFields);

            return remover;
        }

        /// <summary>
        /// Get Splitter from feature list
        /// </summary>
        private Filtering.Splitter GetSplitter(Dictionary<string, Feature> features)
        {
            Filtering.Splitter splitter = new Filtering.Splitter
            {
                SplitType = GetSplitType(features),
            };
            return splitter;
        }

        /// <summary>
        /// Get SplitType from feature list
        /// </summary>
        private MergingFlag GetSplitType(Dictionary<string, Feature> features)
        {
            MergingFlag splitType = MergingFlag.None;
            if (GetBoolean(features, DatDeviceNonMergedValue))
                splitType = MergingFlag.Device;
            else if (GetBoolean(features, DatFullNonMergedValue))
                splitType = MergingFlag.Full;
            else if (GetBoolean(features, DatMergedValue))
                splitType = MergingFlag.Merged;
            else if (GetBoolean(features, DatNonMergedValue))
                splitType = MergingFlag.NonMerged;
            else if (GetBoolean(features, DatSplitValue))
                splitType = MergingFlag.Split;

            return splitType;
        }

        #endregion
    
        #region Protected Helpers

        /// <summary>
        /// Ensure that the database exists and has the proper schema
        /// </summary>
        protected static void EnsureDatabase()
        {
            // Make sure the file exists
            if (!File.Exists(HeadererFileName))
                File.Create(HeadererFileName);

            // Open the database connection
            SqliteConnection dbc = new SqliteConnection(HeadererConnectionString);
            dbc.Open();

            // Make sure the database has the correct schema
            string query = @"
CREATE TABLE IF NOT EXISTS data (
    'sha1'		TEXT		NOT NULL,
    'header'	TEXT		NOT NULL,
    'type'		TEXT		NOT NULL,
    PRIMARY KEY (sha1, header, type)
)";
            SqliteCommand slc = new SqliteCommand(query, dbc);
            slc.ExecuteNonQuery();
            slc.Dispose();
            dbc.Dispose();
        }

        /// <summary>
        /// Get DatFormat value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>DatFormat value corresponding to the string</returns>
        protected static DatFormat GetDatFormat(string input)
        {
            switch (input?.Trim().ToLowerInvariant())
            {
                case "all":
                    return DatFormat.ALL;
                case "ado":
                case "archive":
                    return DatFormat.ArchiveDotOrg;
                case "am":
                case "attractmode":
                    return DatFormat.AttractMode;
                case "cmp":
                case "clrmamepro":
                    return DatFormat.ClrMamePro;
                case "csv":
                    return DatFormat.CSV;
                case "dc":
                case "doscenter":
                    return DatFormat.DOSCenter;
                case "everdrive":
                case "smdb":
                    return DatFormat.EverdriveSMDB;
                case "json":
                case "sj":
                case "sabrejson":
                    return DatFormat.SabreJSON;
                case "lr":
                case "listrom":
                    return DatFormat.Listrom;
                case "lx":
                case "listxml":
                    return DatFormat.Listxml;
                case "md5":
                    return DatFormat.RedumpMD5;
                case "miss":
                case "missfile":
                    return DatFormat.MissFile;
                case "msx":
                case "openmsx":
                    return DatFormat.OpenMSX;
                case "ol":
                case "offlinelist":
                    return DatFormat.OfflineList;
                case "rc":
                case "romcenter":
                    return DatFormat.RomCenter;
                case "sd":
                case "sabredat":
                case "sx":
                case "sabrexml":
                    return DatFormat.SabreXML;
                case "sfv":
                    return DatFormat.RedumpSFV;
                case "sha1":
                    return DatFormat.RedumpSHA1;
                case "sha256":
                    return DatFormat.RedumpSHA256;
                case "sha384":
                    return DatFormat.RedumpSHA384;
                case "sha512":
                    return DatFormat.RedumpSHA512;
                case "sl":
                case "softwarelist":
                    return DatFormat.SoftwareList;
                case "spamsum":
                    return DatFormat.RedumpSpamSum;
                case "ssv":
                    return DatFormat.SSV;
                case "tsv":
                    return DatFormat.TSV;
                case "xml":
                case "logiqx":
                    return DatFormat.Logiqx;
                default:
                    return 0x0;
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Get StatReportFormat value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>StatReportFormat value corresponding to the string</returns>
        private static StatReportFormat GetStatReportFormat(string input)
        {
            return input?.Trim().ToLowerInvariant() switch
            {
                "all" => StatReportFormat.All,
                "csv" => StatReportFormat.CSV,
                "html" => StatReportFormat.HTML,
                "ssv" => StatReportFormat.SSV,
                "text" => StatReportFormat.Textfile,
                "tsv" => StatReportFormat.TSV,
                _ => 0x0,
            };
        }
    
        #endregion
    }
}
