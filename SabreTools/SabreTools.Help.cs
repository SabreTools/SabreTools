using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

namespace SabreTools
{
    public partial class SabreTools
    {
        #region Private Flag features

        public const string AddBlankFilesValue = "add-blank-files";
        private static Feature _addBlankFilesFlag
        {
            get
            {
                return new Feature(
                    AddBlankFilesValue,
                    new List<string>() { "-ab", "--add-blank-files" },
                    "Output blank files for folders",
                    FeatureType.Flag,
                    longDescription: "If this flag is set, then blank entries will be created for each of the empty directories in the source. This is useful for tools that require all folders be accounted for in the output DAT.");
            }
        }

        public const string AddDateValue = "add-date";
        private static Feature _addDateFlag
        {
            get
            {
                return new Feature(
                    AddDateValue,
                    new List<string>() { "-ad", "--add-date" },
                    "Add dates to items, where posible",
                    FeatureType.Flag,
                    longDescription: "If this flag is set, then the Date will be appended to each file information in the output DAT. The output format is standardized as \"yyyy/MM/dd HH:mm:ss\".");
            }
        }

        public const string ArchivesAsFilesValue = "archives-as-files";
        private static Feature _archivesAsFilesFlag
        {
            get
            {
                return new Feature(
                    ArchivesAsFilesValue,
                    new List<string>() { "-aaf", "--archives-as-files" },
                    "Treat archives as files",
                    FeatureType.Flag,
                    longDescription: "Instead of trying to enumerate the files within archives, treat the archives as files themselves. This is good for uncompressed sets that include archives that should be read as-is.");
            }
        }

        public const string BaddumpColumnValue = "baddump-column";
        private static Feature _baddumpColumnFlag
        {
            get
            {
                return new Feature(
                    BaddumpColumnValue,
                    new List<string>() { "-bc", "--baddump-column" },
                    "Add baddump stats to output",
                    FeatureType.Flag,
                    longDescription: "Add a new column or field for counting the number of baddumps in the DAT.");
            }
        }

        public const string BaseValue = "base";
        private static Feature _baseFlag
        {
            get
            {
                return new Feature(
                    BaseValue,
                    new List<string>() { "-ba", "--base" },
                    "Use source DAT as base name for outputs",
                    FeatureType.Flag,
                    longDescription: "If splitting an entire folder of DATs, some output files may be normally overwritten since the names would be the same. With this flag, the original DAT name is used in the output name, in the format of \"Original Name(Dir - Name)\". This can be used in conjunction with --short to output in the format of \"Original Name (Name)\" instead.");
            }
        }

        public const string BaseReplaceValue = "base-replace";
        private static Feature _baseReplaceFlag
        {
            get
            {
                return new Feature(
                    BaseReplaceValue,
                    new List<string>() { "-br", "--base-replace" },
                    "Replace from base DATs in order",
                    FeatureType.Flag,
                    longDescription: "By default, no item names are changed except when there is a merge occurring. This flag enables users to define a DAT or set of base DATs to use as \"replacements\" for all input DATs. Note that the first found instance of an item in the base DAT(s) will be used and all others will be discarded. If no additional flag is given, it will default to updating names.");
            }
        }

        public const string ChdsAsFilesValue = "chds-as-files";
        private static Feature _chdsAsFilesFlag
        {
            get
            {
                return new Feature(
                    ChdsAsFilesValue,
                    new List<string>() { "-ic", "--chds-as-files" },
                    "Treat CHDs as regular files",
                    FeatureType.Flag,
                    longDescription: "Normally, CHDs would be processed using their internal hash to compare against the input DATs. This flag forces all CHDs to be treated like regular files.");
            }
        }

        public const string CleanValue = "clean";
        private static Feature _cleanFlag
        {
            get
            {
                return new Feature(
                    CleanValue,
                    new List<string>() { "-clean", "--clean" },
                    "Clean game names according to WoD standards",
                    FeatureType.Flag,
                    longDescription: "Game names will be sanitized to remove what the original WoD standards deemed as unneeded information, such as parenthesized or bracketed strings.");
            }
        }

        public const string CopyFilesValue = "copy-files";
        private static Feature _copyFilesFlag
        {
            get
            {
                return new Feature(
                    CopyFilesValue,
                    new List<string>() { "-cf", "--copy-files" },
                    "Copy files to the temp directory before parsing",
                    FeatureType.Flag,
                    longDescription: "If this flag is set, then all files that are going to be parsed are moved to the temporary directory before being hashed. This can be helpful in cases where the temp folder is located on an SSD and the user wants to take advantage of this.");
            }
        }

        public const string DatDeviceNonMergedValue = "dat-device-non-merged";
        private static Feature _datDeviceNonMergedFlag
        {
            get
            {
                return new Feature(
                    DatDeviceNonMergedValue,
                    new List<string>() { "-dnd", "--dat-device-non-merged" },
                    "Create device non-merged sets",
                    FeatureType.Flag,
                    longDescription: "Preprocess the DAT to have child sets contain all items from the device references. This is incompatible with the other --dat-X flags.");
            }
        }

        public const string DatFullNonMergedValue = "dat-full-non-merged";
        private static Feature _datFullNonMergedFlag
        {
            get
            {
                return new Feature(
                    DatFullNonMergedValue,
                    new List<string>() { "-df", "--dat-full-non-merged" },
                    "Create fully non-merged sets",
                    FeatureType.Flag,
                    longDescription: "Preprocess the DAT to have child sets contain all items from the parent sets based on the cloneof and romof tags as well as device references. This is incompatible with the other --dat-X flags.");
            }
        }

        public const string DatMergedValue = "dat-merged";
        private static Feature _datMergedFlag
        {
            get
            {
                return new Feature(
                    DatMergedValue,
                    new List<string>() { "-dm", "--dat-merged" },
                    "Force creating merged sets",
                    FeatureType.Flag,
                    longDescription: "Preprocess the DAT to have parent sets contain all items from the children based on the cloneof tag. This is incompatible with the other --dat-X flags.");
            }
        }

        public const string DatNonMergedValue = "dat-non-merged";
        private static Feature _datNonMergedFlag
        {
            get
            {
                return new Feature(
                    DatNonMergedValue,
                    new List<string>() { "-dnm", "--dat-non-merged" },
                    "Force creating non-merged sets",
                    FeatureType.Flag,
                    longDescription: "Preprocess the DAT to have child sets contain all items from the parent set based on the romof and cloneof tags. This is incompatible with the other --dat-X flags.");
            }
        }

        public const string DatSplitValue = "dat-split";
        private static Feature _datSplitFlag
        {
            get
            {
                return new Feature(
                    DatSplitValue,
                    new List<string>() { "-ds", "--dat-split" },
                    "Force creating split sets",
                    FeatureType.Flag,
                    longDescription: "Preprocess the DAT to remove redundant files between parents and children based on the romof and cloneof tags. This is incompatible with the other --dat-X flags.");
            }
        }

        public const string DedupValue = "dedup";
        private static Feature _dedupFlag
        {
            get
            {
                return new Feature(
                    DedupValue,
                    new List<string>() { "-dd", "--dedup" },
                    "Enable deduping in the created DAT",
                    FeatureType.Flag,
                    longDescription: "For all outputted DATs, allow for hash deduping. This makes sure that there are effectively no duplicates in the output files. Cannot be used with game dedup.");
            }
        }

        public const string DeleteValue = "delete";
        private static Feature _deleteFlag
        {
            get
            {
                return new Feature(
                    DeleteValue,
                    new List<string>() { "-del", "--delete" },
                    "Delete fully rebuilt input files",
                    FeatureType.Flag,
                    longDescription: "Optionally, the input files, once processed and fully matched, can be deleted. This can be useful when the original file structure is no longer needed or if there is limited space on the source drive.");
            }
        }

        public const string DepotValue = "depot";
        private static Feature _depotFlag
        {
            get
            {
                return new Feature(
                    DepotValue,
                    new List<string>() { "-dep", "--depot" },
                    "Assume directories are romba depots",
                    FeatureType.Flag,
                    longDescription: "Normally, input directories will be treated with no special format. If this flag is used, all input directories will be assumed to be romba-style depots.");
            }
        }

        public const string DeprecatedValue = "deprecated";
        private static Feature _deprecatedFlag
        {
            get
            {
                return new Feature(
                    DeprecatedValue,
                    new List<string>() { "-dpc", "--deprecated" },
                    "Output 'game' instead of 'machine'",
                    FeatureType.Flag,
                    longDescription: "By default, Logiqx XML DATs output with the more modern \"machine\" tag for each set. This flag allows users to output the older \"game\" tag instead, for compatibility reasons. [Logiqx only]");
            }
        }

        public const string DescriptionAsNameValue = "description-as-name";
        private static Feature _descriptionAsNameFlag
        {
            get
            {
                return new Feature(
                    DescriptionAsNameValue,
                    new List<string>() { "-dan", "--description-as-name" },
                    "Use description instead of machine name",
                    FeatureType.Flag,
                    longDescription: "By default, all DATs are converted exactly as they are input. Enabling this flag allows for the machine names in the DAT to be replaced by the machine description instead. In most cases, this will result in no change in the output DAT, but a notable example would be a software list DAT where the machine names are generally DOS-friendly while the description is more complete.");
            }
        }

        public const string DiffAgainstValue = "diff-against";
        private static Feature _diffAgainstFlag
        {
            get
            {
                return new Feature(
                    DiffAgainstValue,
                    new List<string>() { "-dag", "--diff-against" },
                    "Diff all inputs against a set of base DATs",
                    FeatureType.Flag,
                    "This flag will enable a special type of diffing in which a set of base DATs are used as a comparison point for each of the input DATs. This allows users to get a slightly different output to cascaded diffing, which may be more useful in some cases. This is heavily influenced by the diffing model used by Romba.");
            }
        }

        public const string DiffAllValue = "diff-all";
        private static Feature _diffAllFlag
        {
            get
            {
                return new Feature(
                    DiffAllValue,
                    new List<string>() { "-di", "--diff-all" },
                    "Create diffdats from inputs (all standard outputs)",
                    FeatureType.Flag,
                    longDescription: "By default, all DATs are processed individually with the user-specified flags. With this flag enabled, input DATs are diffed against each other to find duplicates, no duplicates, and only in individuals.");
            }
        }

        public const string DiffCascadeValue = "diff-cascade";
        private static Feature _diffCascadeFlag
        {
            get
            {
                return new Feature(
                    DiffCascadeValue,
                    new List<string>() { "-dc", "--diff-cascade" },
                    "Enable cascaded diffing",
                    FeatureType.Flag,
                    longDescription: "This flag allows for a special type of diffing in which the first DAT is considered a base, and for each additional input DAT, it only leaves the files that are not in one of the previous DATs. This can allow for the creation of rollback sets or even just reduce the amount of duplicates across multiple sets.");
            }
        }

        public const string DiffDuplicatesValue = "diff-duplicates";
        private static Feature _diffDuplicatesFlag
        {
            get
            {
                return new Feature(
                    DiffDuplicatesValue,
                    new List<string>() { "-did", "--diff-duplicates" },
                    "Create diffdat containing just duplicates",
                    FeatureType.Flag,
                    longDescription: "All files that have duplicates outside of the original DAT are included.");
            }
        }

        public const string DiffIndividualsValue = "diff-individuals";
        private static Feature _diffIndividualsFlag
        {
            get
            {
                return new Feature(
                    DiffIndividualsValue,
                    new List<string>() { "-dii", "--diff-individuals" },
                    "Create diffdats for individual DATs",
                    FeatureType.Flag,
                    longDescription: "All files that have no duplicates outside of the original DATs are put into DATs that are named after the source DAT.");
            }
        }

        public const string DiffNoDuplicatesValue = "diff-no-duplicates";
        private static Feature _diffNoDuplicatesFlag
        {
            get
            {
                return new Feature(
                    DiffNoDuplicatesValue,
                    new List<string>() { "-din", "--diff-no-duplicates" },
                    "Create diffdat containing no duplicates",
                    FeatureType.Flag,
                    longDescription: "All files that have no duplicates outside of the original DATs are included.");
            }
        }

        public const string DiffReverseCascadeValue = "diff-reverse-cascade";
        private static Feature _diffReverseCascadeFlag
        {
            get
            {
                return new Feature(
                    DiffReverseCascadeValue,
                    new List<string>() { "-drc", "--diff-reverse-cascade" },
                    "Enable reverse cascaded diffing",
                    FeatureType.Flag,
                    longDescription: "This flag allows for a special type of diffing in which the last DAT is considered a base, and for each additional input DAT, it only leaves the files that are not in one of the previous DATs. This can allow for the creation of rollback sets or even just reduce the amount of duplicates across multiple sets.");
            }
        }

        public const string ExtensionValue = "extension";
        private static Feature _extensionFlag
        {
            get
            {
                return new Feature(
                    ExtensionValue,
                    new List<string>() { "-es", "--extension" },
                    "Split DAT(s) by two file extensions",
                    FeatureType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on a list of input extensions. This can allow for combined DAT files, such as those combining two separate systems, to be split. Files with any extensions not listed in the input lists will be included in both outputted DAT files.");
            }
        }

        public const string GameDedupValue = "game-dedup";
        private static Feature _gameDedupFlag
        {
            get
            {
                return new Feature(
                    GameDedupValue,
                    new List<string>() { "-gdd", "--game-dedup" },
                    "Enable deduping within games in the created DAT",
                    FeatureType.Flag,
                    longDescription: "For all outputted DATs, allow for hash deduping but only within the games, and not across the entire DAT. This makes sure that there are effectively no duplicates within each of the output sets. Cannot be used with standard dedup.");
            }
        }

        public const string GamePrefixValue = "game-prefix";
        private static Feature _gamePrefixFlag
        {
            get
            {
                return new Feature(
                    GamePrefixValue,
                    new List<string>() { "-gp", "--game-prefix" },
                    "Add game name as a prefix",
                    FeatureType.Flag,
                    longDescription: "This flag allows for the name of the game to be used as a prefix to each file.");
            }
        }

        public const string HashValue = "hash";
        private static Feature _hashFlag
        {
            get
            {
                return new Feature(
                    HashValue,
                    new List<string>() { "-hs", "--hash" },
                    "Split DAT(s) or folder by best-available hashes",
                    FeatureType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on the best available hash for each file within. The order of preference for the outputted DATs is as follows: Nodump, SHA-512, SHA-384, SHA-256, SHA-1, MD5, CRC (or worse).");
            }
        }

        public const string HashOnlyValue = "hash-only";
        private static Feature _hashOnlyFlag
        {
            get
            {
                return new Feature(
                    HashOnlyValue,
                    new List<string>() { "-ho", "--hash-only" },
                    "Check files by hash only",
                    FeatureType.Flag,
                    longDescription: "This sets a mode where files are not checked based on name but rather hash alone. This allows verification of (possibly) incorrectly named folders and sets to be verified without worrying about the proper set structure to be there.");
            }
        }

        public const string IndividualValue = "individual";
        private static Feature _individualFlag
        {
            get
            {
                return new Feature(
                    IndividualValue,
                    new List<string>() { "-ind", "--individual" },
                    "Process input DATs individually",
                    FeatureType.Flag,
                    longDescription: "In cases where DATs would be processed in bulk, this flag allows them to be processed on their own instead.");
            }
        }

        public const string InplaceValue = "inplace";
        private static Feature _inplaceFlag
        {
            get
            {
                return new Feature(
                    InplaceValue,
                    new List<string>() { "-ip", "--inplace" },
                    "Write to the input directories, where possible",
                    FeatureType.Flag,
                    longDescription: "By default, files are written to the runtime directory (or the output directory, if set). This flag enables users to write out to the directory that the DATs originated from.");
            }
        }

        public const string InverseValue = "inverse";
        private static Feature _inverseFlag
        {
            get
            {
                return new Feature(
                    InverseValue,
                    new List<string>() { "-in", "--inverse" },
                    "Rebuild only files not in DAT",
                    FeatureType.Flag,
                    longDescription: "Instead of the normal behavior of rebuilding using a DAT, this flag allows the user to use the DAT as a filter instead. All files that are found in the DAT will be skipped and everything else will be output in the selected format.");
            }
        }

        public const string KeepEmptyGamesValue = "keep-empty-games";
        private static Feature _keepEmptyGamesFlag
        {
            get
            {
                return new Feature(
                    KeepEmptyGamesValue,
                    new List<string>() { "-keg", "--keep-empty-games" },
                    "Keep originally empty sets from the input(s)",
                    FeatureType.Flag,
                    longDescription: "Normally, any sets that are considered empty will not be included in the output, this flag allows these empty sets to be added to the output.");
            }
        }

        public const string LevelValue = "level";
        private static Feature _levelFlag
        {
            get
            {
                return new Feature(
                    LevelValue,
                    new List<string>() { "-ls", "--level" },
                    "Split a SuperDAT or folder by lowest available level",
                    FeatureType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on the lowest available level of game name. That is, if a game name is top/mid/last, then it will create an output DAT for the parent directory \"mid\" in a folder called \"top\" with a game called \"last\".");
            }
        }

        public const string MatchOfTagsValue = "match-of-tags";
        private static Feature _matchOfTagsFlag
        {
            get
            {
                return new Feature(
                    MatchOfTagsValue,
                    new List<string>() { "-ofg", "--match-of-tags" },
                    "Allow cloneof and romof tags to match game name filters",
                    FeatureType.Flag,
                    longDescription: "If filter or exclude by game name is used, this flag will allow those filters to be checked against the romof and cloneof tags as well. This can allow for more advanced set-building, especially in arcade-based sets.");
            }
        }

        public const string MergeValue = "merge";
        private static Feature _mergeFlag
        {
            get
            {
                return new Feature(
                    MergeValue,
                    new List<string>() { "-m", "--merge" },
                    "Merge the input DATs",
                    FeatureType.Flag,
                    longDescription: "By default, all DATs are processed individually with the user-specified flags. With this flag enabled, all of the input DATs are merged into a single output. This is best used with the dedup flag.");
            }
        }

        public const string NoAutomaticDateValue = "no-automatic-date";
        private static Feature _noAutomaticDateFlag
        {
            get
            {
                return new Feature(
                    NoAutomaticDateValue,
                    new List<string>() { "-b", "--no-automatic-date" },
                    "Don't include date in file name",
                    FeatureType.Flag,
                    longDescription: "Normally, the DAT will be created with the date in the file name in brackets. This flag removes that instead of the default.");
            }
        }

        public const string NodumpColumnValue = "nodump-column";
        private static Feature _nodumpColumnFlag
        {
            get
            {
                return new Feature(
                    NodumpColumnValue,
                    new List<string>() { "-nc", "--nodump-column" },
                    "Add statistics for nodumps to output",
                    FeatureType.Flag,
                    longDescription: "Add a new column or field for counting the number of nodumps in the DAT.");
            }
        }

        public const string NoStoreHeaderValue = "no-store-header";
        private static Feature _noStoreHeaderFlag
        {
            get
            {
                return new Feature(
                    NoStoreHeaderValue,
                    new List<string>() { "-nsh", "--no-store-header" },
                    "Don't store the extracted header",
                    FeatureType.Flag,
                    longDescription: "By default, all headers that are removed from files are backed up in the database. This flag allows users to skip that step entirely, avoiding caching the headers at all.");
            }
        }

        public const string NotRunnableValue = "not-runnable";
        private static Feature _notRunnableFlag
        {
            get
            {
                return new Feature(
                    NotRunnableValue,
                    new List<string>() { "-nrun", "--not-runnable" },
                    "Include only items that are not marked runnable",
                    FeatureType.Flag,
                    longDescription: "This allows users to include only unrunnable games.");
            }
        }

        public const string OneRomPerGameValue = "one-rom-per-game";
        private static Feature _oneRomPerGameFlag
        {
            get
            {
                return new Feature(
                    OneRomPerGameValue,
                    new List<string>() { "-orpg", "--one-rom-per-game" },
                    "Try to ensure each rom has its own game",
                    FeatureType.Flag,
                    longDescription: "In some cases, it is beneficial to have every rom put into its own output set as a subfolder of the original parent. This flag enables outputting each rom to its own game for this purpose.");
            }
        }

        public const string OnlySameValue = "only-same";
        private static Feature _onlySameFlag
        {
            get
            {
                return new Feature(
                    OnlySameValue,
                    new List<string>() { "-ons", "--only-same" },
                    "Only update description if machine name matches description",
                    FeatureType.Flag,
                    longDescription: "Normally, updating the description will always overwrite if the machine names are the same. With this flag, descriptions will only be overwritten if they are the same as the machine names.");
            }
        }

        public const string QuickValue = "quick";
        private static Feature _quickFlag
        {
            get
            {
                return new Feature(
                    QuickValue,
                    new List<string>() { "-qs", "--quick" },
                    "Enable quick scanning of archives",
                    FeatureType.Flag,
                    longDescription: "For all archives, if this flag is enabled, it will only use the header information to get the archive entries' file information. The upside to this is that it is the fastest option. On the downside, it can only get the CRC and size from most archive formats, leading to possible issues.");
            }
        }

        public const string QuotesValue = "quotes";
        private static Feature _quotesFlag
        {
            get
            {
                return new Feature(
                    QuotesValue,
                    new List<string>() { "-q", "--quotes" },
                    "Double-quote each item",
                    FeatureType.Flag,
                    longDescription: "This flag surrounds the item by double-quotes, not including the prefix or postfix.");
            }
        }

        public const string RemoveExtensionsValue = "remove-extensions";
        private static Feature _removeExtensionsFlag
        {
            get
            {
                return new Feature(
                    RemoveExtensionsValue,
                    new List<string>() { "-rme", "--remove-extensions" },
                    "Remove all extensions from all items",
                    FeatureType.Flag,
                    longDescription: "For each item, remove the extension.");
            }
        }

        public const string RemoveUnicodeValue = "remove-unicode";
        private static Feature _removeUnicodeFlag
        {
            get
            {
                return new Feature(
                    RemoveUnicodeValue,
                    new List<string>() { "-ru", "--remove-unicode" },
                    "Remove unicode characters from names",
                    FeatureType.Flag,
                    longDescription: "By default, the character set from the original file(s) will be used for item naming. This flag removes all Unicode characters from the item names, machine names, and machine descriptions.");
            }
        }

        public const string ReverseBaseReplaceValue = "reverse-base-replace";
        private static Feature _reverseBaseReplaceFlag
        {
            get
            {
                return new Feature(
                    ReverseBaseReplaceValue,
                    new List<string>() { "-rbr", "--reverse-base-replace" },
                    "Replace item names from base DATs in reverse",
                    FeatureType.Flag,
                    longDescription: "By default, no item names are changed except when there is a merge occurring. This flag enables users to define a DAT or set of base DATs to use as \"replacements\" for all input DATs. Note that the first found instance of an item in the last base DAT(s) will be used and all others will be discarded. If no additional flag is given, it will default to updating names.");
            }
        }

        public const string RombaValue = "romba";
        private static Feature _rombaFlag
        {
            get
            {
                return new Feature(
                    RombaValue,
                    new List<string>() { "-ro", "--romba" },
                    "Treat like a Romba depot (requires SHA-1)",
                    FeatureType.Flag,
                    longDescription: "This flag allows reading and writing of DATs and output files to and from a Romba-style depot. This also implies TorrentGZ input and output for physical files. Where appropriate, Romba depot files will be created as well.");
            }
        }

        public const string RomsValue = "roms";
        private static Feature _romsFlag
        {
            get
            {
                return new Feature(
                    RomsValue,
                    new List<string>() { "-r", "--roms" },
                    "Output roms to miss instead of sets",
                    FeatureType.Flag,
                    longDescription: "By default, the outputted file will include the name of the game so this flag allows for the name of the rom to be output instead. [Missfile only]");
            }
        }

        public const string RunnableValue = "runnable";
        private static Feature _runnableFlag
        {
            get
            {
                return new Feature(
                    RunnableValue,
                    new List<string>() { "-run", "--runnable" },
                    "Include only items that are marked runnable",
                    FeatureType.Flag,
                    longDescription: "This allows users to include only verified runnable games.");
            }
        }

        public const string ScanAllValue = "scan-all";
        private static Feature _scanAllFlag
        {
            get
            {
                return new Feature(
                    ScanAllValue,
                    new List<string>() { "-sa", "--scan-all" },
                    "Set scanning levels for all archives to 0",
                    FeatureType.Flag,
                    longDescription: "This flag is the short equivalent to -7z=0 -gz=0 -rar=0 -zip=0 wrapped up. Generally this will be helpful in all cases where the content of the rebuild folder is not entirely known or is known to be mixed.");
            }
        }

        public const string SceneDateStripValue = "scene-date-strip";
        private static Feature _sceneDateStripFlag
        {
            get
            {
                return new Feature(
                    SceneDateStripValue,
                    new List<string>() { "-sds", "--scene-date-strip" },
                    "Remove date from scene-named sets",
                    FeatureType.Flag,
                    longDescription: "If this flag is enabled, sets with \"scene\" names will have the date removed from the beginning. For example \"01.01.01-Game_Name-GROUP\" would become \"Game_Name-Group\".");
            }
        }

        public const string ShortValue = "short";
        private static Feature _shortFlag
        {
            get
            {
                return new Feature(
                    ShortValue,
                    new List<string>() { "-s", "--short" },
                    "Use short output names",
                    FeatureType.Flag,
                    longDescription: "Instead of using ClrMamePro-style long names for DATs, use just the name of the folder as the name of the DAT. This can be used in conjunction with --base to output in the format of \"Original Name (Name)\" instead.");
            }
        }

        public const string SingleSetValue = "single-set";
        private static Feature _singleSetFlag
        {
            get
            {
                return new Feature(
                    SingleSetValue,
                    new List<string>() { "-si", "--single-set" },
                    "All game names replaced by '!'",
                    FeatureType.Flag,
                    longDescription: "This is useful for keeping all roms in a DAT in the same archive or folder.");
            }
        }

        public const string SizeValue = "size";
        private static Feature _sizeFlag
        {
            get
            {
                return new Feature(
                    SizeValue,
                    new List<string>() { "-szs", "--size" },
                    "Split DAT(s) or folder by file sizes",
                    FeatureType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on the sizes of the files, specifically if the type is a Rom (most item types don't have sizes).");
            }
        }

        public const string SkipArchivesValue = "skip-archives";
        private static Feature _skipArchivesFlag
        {
            get
            {
                return new Feature(
                    SkipArchivesValue,
                    new List<string>() { "-ska", "--skip-archives" },
                    "Skip all archives",
                    FeatureType.Flag,
                    longDescription: "Skip any files that are treated like archives");
            }
        }

        public const string SkipFilesValue = "skip-files";
        private static Feature _skipFilesFlag
        {
            get
            {
                return new Feature(
                    SkipFilesValue,
                    new List<string>() { "-skf", "--skip-files" },
                    "Skip all non-archives",
                    FeatureType.Flag,
                    longDescription: "Skip any files that are not treated like archives");
            }
        }

        public const string SkipFirstOutputValue = "skip-first-output";
        private static Feature _skipFirstOutputFlag
        {
            get
            {
                return new Feature(
                    SkipFirstOutputValue,
                    new List<string>() { "-sf", "--skip-first-output" },
                    "Skip output of first DAT",
                    FeatureType.Flag,
                    longDescription: "In times where the first DAT does not need to be written out a second time, this will skip writing it. This can often speed up the output process.");
            }
        }

        public const string SkipMd5Value = "skip-md5";
        private static Feature _skipMd5Flag
        {
            get
            {
                return new Feature(
                    SkipMd5Value,
                    new List<string>() { "-nm", "--skip-md5" },
                    "Don't include MD5 in output",
                    FeatureType.Flag,
                    longDescription: "This allows the user to skip calculating the MD5 for each of the files which will speed up the creation of the DAT.");
            }
        }

        public const string SkipRipeMd160Value = "skip-ripemd160";
        private static Feature _skipRipeMd160Flag
        {
            get
            {
                return new Feature(
                    SkipRipeMd160Value,
                    new List<string>() { "-nr160", "--skip-ripemd160" },
                    "Include RIPEMD160 in output", // TODO: Invert this later
                    FeatureType.Flag,
                    longDescription: "This allows the user to skip calculating the RIPEMD160 for each of the files which will speed up the creation of the DAT.");
            }
        }

        public const string SkipSha1Value = "skip-sha1";
        private static Feature _skipSha1Flag
        {
            get
            {
                return new Feature(
                    SkipSha1Value,
                    new List<string>() { "-ns", "--skip-sha1" },
                    "Don't include SHA-1 in output",
                    FeatureType.Flag,
                    longDescription: "This allows the user to skip calculating the SHA-1 for each of the files which will speed up the creation of the DAT.");
            }
        }

        public const string SkipSha256Value = "skip-sha256";
        private static Feature _skipSha256Flag
        {
            get
            {
                return new Feature(
                    SkipSha256Value,
                    new List<string>() { "-ns256", "--skip-sha256" },
                    "Include SHA-256 in output", // TODO: Invert this later
                    FeatureType.Flag,
                    longDescription: "This allows the user to skip calculating the SHA-256 for each of the files which will speed up the creation of the DAT.");
            }
        }

        public const string SkipSha384Value = "skip-sha384";
        private static Feature _skipSha384Flag
        {
            get
            {
                return new Feature(
                    SkipSha384Value,
                    new List<string>() { "-ns384", "--skip-sha384" },
                    "Include SHA-384 in output", // TODO: Invert this later
                    FeatureType.Flag,
                    longDescription: "This allows the user to skip calculating the SHA-384 for each of the files which will speed up the creation of the DAT.");
            }
        }

        public const string SkipSha512Value = "skip-sha512";
        private static Feature _skipSha512Flag
        {
            get
            {
                return new Feature(
                    SkipSha512Value,
                    new List<string>() { "-ns512", "--skip-sha512" },
                    "Include SHA-512 in output", // TODO: Invert this later
                    FeatureType.Flag,
                    longDescription: "This allows the user to skip calculating the SHA-512 for each of the files which will speed up the creation of the DAT.");
            }
        }

        public const string SuperdatValue = "superdat";
        private static Feature _superdatFlag
        {
            get
            {
                return new Feature(
                    SuperdatValue,
                    new List<string>() { "-sd", "--superdat" },
                    "Enable SuperDAT creation",
                    FeatureType.Flag,
                    longDescription: "Set the type flag to \"SuperDAT\" for the output DAT as well as preserving the directory structure of the inputted folder, if applicable.");
            }
        }

        public const string TarValue = "tar";
        private static Feature _tarFlag
        {
            get
            {
                return new Feature(
                    TarValue,
                    new List<string>() { "-tar", "--tar" },
                    "Enable Tape ARchive output",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to Tape ARchive (TAR) files. This format is a standardized storage archive without any compression, usually used with other compression formats around it. It is widely used in backup applications and source code archives.");
            }
        }

        public const string Torrent7zipValue = "torrent-7zip";
        private static Feature _torrent7zipFlag
        {
            get
            {
                return new Feature(
                    Torrent7zipValue,
                    new List<string>() { "-t7z", "--torrent-7zip" },
                    "Enable Torrent7Zip output",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent7Zip (T7Z) files. This format is based on the LZMA container format 7Zip, but with custom header information. This is currently unused by any major application. Currently does not produce proper Torrent-compatible outputs.");
            }
        }

        public const string TorrentGzipValue = "torrent-gzip";
        private static Feature _torrentGzipFlag
        {
            get
            {
                return new Feature(
                    TorrentGzipValue,
                    new List<string>() { "-tgz", "--torrent-gzip" },
                    "Enable Torrent GZip output",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to TorrentGZ (TGZ) files. This format is based on the GZip archive format, but with custom header information and a file name replaced by the SHA-1 of the file inside. This is primarily used by external tool Romba (https://github.com/uwedeportivo/romba), but may be used more widely in the future.");
            }
        }

        public const string TorrentLrzipValue = "torrent-lrzip";
        private static Feature _torrentLrzipFlag
        {
            get
            {
                return new Feature(
                    TorrentLrzipValue,
                    new List<string>() { "-tlrz", "--torrent-lrzip" },
                    "Enable Torrent Long-Range Zip output [UNIMPLEMENTED]",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent Long-Range Zip (TLRZ) files. This format is based on the LRZip file format as defined at https://github.com/ckolivas/lrzip but with custom header information. This is currently unused by any major application.");
            }
        }
        
        public const string TorrentLz4Value = "torrent-lz4";
        private static Feature _torrentLz4Flag
        {
            get
            {
                return new Feature(
                    TorrentLz4Value,
                    new List<string>() { "-tlz4", "--torrent-lz4" },
                    "Enable Torrent LZ4 output [UNIMPLEMENTED]",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent LZ4 (TLZ4) files. This format is based on the LZ4 file format as defined at https://github.com/lz4/lz4 but with custom header information. This is currently unused by any major application.");
            }
        }

        public const string TorrentRarValue = "torrent-rar";
        private static Feature _torrentRarFlag
        {
            get
            {
                return new Feature(
                    TorrentRarValue,
                    new List<string>() { "-trar", "--torrent-rar" },
                    "Enable Torrent RAR output [UNIMPLEMENTED]",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting files to folder, files will be rebuilt to Torrent RAR (TRAR) files. This format is based on the RAR propietary format but with custom header information. This is currently unused by any major application.");
            }
        }

        public const string TorrentXzValue = "torrent-xz";
        private static Feature _torrentXzFlag
        {
            get
            {
                return new Feature(
                    TorrentXzValue,
                    new List<string>() { "-txz", "--torrent-xz" },
                    "Enable Torrent XZ output [UNSUPPORTED]",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting files to folder, files will be rebuilt to Torrent XZ (TXZ) files. This format is based on the LZMA container format XZ, but with custom header information. This is currently unused by any major application. Currently does not produce proper Torrent-compatible outputs.");
            }
        }

        public const string TorrentZipValue = "torrent-zip";
        private static Feature _torrentZipFlag
        {
            get
            {
                return new Feature(
                    TorrentZipValue,
                    new List<string>() { "-tzip", "--torrent-zip" },
                    "Enable Torrent Zip output",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting files to folder, files will be rebuilt to TorrentZip (TZip) files. This format is based on the ZIP archive format, but with custom header information. This is primarily used by external tool RomVault (http://www.romvault.com/) and is already widely used.");
            }
        }

        public const string TorrentZpaqValue = "torrent-zpaq";
        private static Feature _torrentZpaqFlag
        {
            get
            {
                return new Feature(
                    TorrentZpaqValue,
                    new List<string>() { "-tzpaq", "--torrent-zpaq" },
                    "Enable Torrent ZPAQ output [UNIMPLEMENTED]",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent ZPAQ (TZPAQ) files. This format is based on the ZPAQ file format as defined at https://github.com/zpaq/zpaq but with custom header information. This is currently unused by any major application.");
            }
        }

        public const string TorrentZstdValue = "torrent-zstd";
        private static Feature _torrentZstdFlag
        {
            get
            {
                return new Feature(
                    TorrentZstdValue,
                    new List<string>() { "-tzstd", "--torrent-zstd" },
                    "Enable Torrent Zstd output [UNIMPLEMENTED]",
                    FeatureType.Flag,
                    longDescription: "Instead of outputting the files to folder, files will be rebuilt to Torrent Zstd (TZstd) files. This format is based on the Zstd file format as defined at https://github.com/skbkontur/ZstdNet but with custom header information. This is currently unused by any major application.");
            }
        }

        public const string TrimValue = "trim";
        private static Feature _trimFlag
        {
            get
            {
                return new Feature(
                    TrimValue,
                    new List<string>() { "-trim", "--trim" },
                    "Trim file names to fit NTFS length",
                    FeatureType.Flag,
                    longDescription: "In the cases where files will have too long a name, this allows for trimming the name of the files to the NTFS maximum length at most.");
            }
        }

        public const string TypeValue = "type";
        private static Feature _typeFlag
        {
            get
            {
                return new Feature(
                    TypeValue,
                    new List<string>() { "-ts", "--type" },
                    "Split DAT(s) or folder by file types (rom/disk)",
                    FeatureType.Flag,
                    longDescription: "For a DAT, or set of DATs, allow for splitting based on the types of the files, specifically if the type is a rom or a disk.");
            }
        }

        public const string UpdateDatValue = "update-dat";
        private static Feature _updateDatFlag
        {
            get
            {
                return new Feature(
                    UpdateDatValue,
                    new List<string>() { "-ud", "--update-dat" },
                    "Output updated DAT to output directory",
                    FeatureType.Flag,
                    longDescription: "Once the files that were able to rebuilt are taken care of, a DAT of the files that could not be matched will be output to the output directory.");
            }
        }

        public const string UpdateDescriptionValue = "update-description";
        private static Feature _updateDescriptionFlag
        {
            get
            {
                return new Feature(
                    UpdateDescriptionValue,
                    new List<string>() { "-udd", "--update-description" },
                    "Update machine descriptions from base DATs",
                    FeatureType.Flag,
                    longDescription: "This flag enables updating of machine descriptions from base DATs.");
            }
        }

        public const string UpdateGameTypeValue = "update-game-type";
        private static Feature _updateGameTypeFlag
        {
            get
            {
                return new Feature(
                    UpdateGameTypeValue,
                    new List<string>() { "-ugt", "--update-game-type" },
                    "Update machine type from base DATs",
                    FeatureType.Flag,
                    longDescription: "This flag enables updating of machine type from base DATs.");
            }
        }

        public const string UpdateHashesValue = "update-hashes";
        private static Feature _updateHashesFlag
        {
            get
            {
                return new Feature(
                    UpdateHashesValue,
                    new List<string>() { "-uh", "--update-hashes" },
                    "Update hashes from base DATs",
                    FeatureType.Flag,
                    longDescription: "This flag enables updating of hashes from base DATs.");
            }
        }

        public const string UpdateManufacturerValue = "update-manufacturer";
        private static Feature _updateManufacturerFlag
        {
            get
            {
                return new Feature(
                    UpdateManufacturerValue,
                    new List<string>() { "-um", "--update-manufacturer" },
                    "Update machine manufacturers from base DATs",
                    FeatureType.Flag,
                    longDescription: "This flag enables updating of machine manufacturers from base DATs.");
            }
        }

        public const string UpdateNamesValue = "update-names";
        private static Feature _updateNamesFlag
        {
            get
            {
                return new Feature(
                    UpdateNamesValue,
                    new List<string>() { "-un", "--update-names" },
                    "Update item names from base DATs",
                    FeatureType.Flag,
                    longDescription: "This flag enables updating of item names from base DATs.");
            }
        }

        public const string UpdateParentsValue = "update-parents";
        private static Feature _updateParentsFlag
        {
            get
            {
                return new Feature(
                    UpdateParentsValue,
                    new List<string>() { "-up", "--update-parents" },
                    "Update machine parents from base DATs",
                    FeatureType.Flag,
                    longDescription: "This flag enables updating of machine parents (romof, cloneof, sampleof) from base DATs.");
            }
        }

        public const string UpdateYearValue = "update-year";
        private static Feature _updateYearFlag
        {
            get
            {
                return new Feature(
                    UpdateYearValue,
                    new List<string>() { "-uy", "--update-year" },
                    "Update machine years from base DATs",
                    FeatureType.Flag,
                    longDescription: "This flag enables updating of machine years from base DATs.");
            }
        }

        #endregion

        #region Private Int32 features

        public const string GzInt32Value = "gz";
        private static Feature _gzInt32Input
        {
            get
            {
                return new Feature(
                    GzInt32Value,
                    new List<string>() { "-gz", "--gz" },
                    "Set scanning level for GZip archives (default 1)",
                    FeatureType.Int32,
                    longDescription: @"Scan GZip archives in one of the following ways:
0 - Hash both archive and its contents
1 - Only hash contents of the archive
2 - Only hash archive itself (treat like a regular file)");
            }
        }

        public const string RarInt32Value = "rar";
        private static Feature _rarInt32Input
        {
            get
            {
                return new Feature(
                    RarInt32Value,
                    new List<string>() { "-rar", "--rar" },
                    "Set scanning level for RAR archives (default 1)",
                    FeatureType.Int32,
                    longDescription: @"Scan RAR archives in one of the following ways:
0 - Hash both archive and its contents
1 - Only hash contents of the archive
2 - Only hash archive itself (treat like a regular file)");
            }
        }

        public const string SevenZipInt32Value = "7z";
        private static Feature _sevenZipInt32Input
        {
            get
            {
                return new Feature(
                    SevenZipInt32Value,
                    new List<string>() { "-7z", "--7z" },
                    "Set scanning level for 7zip archives (default 1)",
                    FeatureType.Int32,
                    longDescription: @"Scan 7Zip archives in one of the following ways:
0 - Hash both archive and its contents
1 - Only hash contents of the archive
2 - Only hash archive itself (treat like a regular file)");
            }
        }

        public const string ThreadsInt32Value = "threads";
        private static Feature _threadsInt32Input
        {
            get
            {
                return new Feature(
                    ThreadsInt32Value,
                    new List<string>() { "-mt", "--threads" },
                    "Amount of threads to use (default = # cores)",
                    FeatureType.Int32,
                    longDescription: "Optionally, set the number of threads to use for the multithreaded operations. The default is the number of available machine threads; -1 means unlimited threads created.");
            }
        }

        public const string ZipInt32Value = "zip";
        private static Feature _zipInt32Input
        {
            get
            {
                return new Feature(
                    ZipInt32Value,
                    new List<string>() { "-zip", "--zip" },
                    "Set scanning level for Zip archives (default 1)",
                    FeatureType.Int32,
                    longDescription: @"Scan Zip archives in one of the following ways:
0 - Hash both archive and its contents
1 - Only hash contents of the archive
2 - Only hash archive itself (treat like a regular file)");
            }
        }

        #endregion

        #region Private Int64 features

        public const string RadixInt64Value = "radix";
        private static Feature _radixInt64Input
        {
            get
            {
                return new Feature(
                    RadixInt64Value,
                    new List<string>() { "-rad", "--radix" },
                    "Set the midpoint to split at",
                    FeatureType.Int64,
                    longDescription: "Set the size at which all roms less than the size are put in the first DAT, and everything greater than or equal goes in the second.");
            }
        }

        #endregion

        #region Private List<string> features

        public const string BaseDatListValue = "base-dat";
        private static Feature _baseDatListInput
        {
            get
            {
                return new Feature(
                    BaseDatListValue,
                    new List<string>() { "-bd", "--base-dat" },
                    "Add a base DAT for processing",
                    FeatureType.List,
                    longDescription: "Add a DAT or folder of DATs to the base set to be used for all operations. Multiple instances of this flag are allowed.");
            }
        }

        public const string CrcListValue = "crc";
        private static Feature _crcListInput
        {
            get
            {
                return new Feature(
                    CrcListValue,
                    new List<string>() { "-crc", "--crc" },
                    "Filter by CRC hash",
                    FeatureType.List,
                    longDescription: "Include only items with this CRC hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string DatListValue = "dat";
        private static Feature _datListInput
        {
            get
            {
                return new Feature(
                    DatListValue,
                    new List<string>() { "-dat", "--dat" },
                    "Input DAT to be used",
                    FeatureType.List,
                    longDescription: "User-supplied DAT for use in all operations. Multiple instances of this flag are allowed.");
            }
        }

        public const string ExcludeFieldListValue = "exclude-field";
        private static Feature _excludeFieldListInput 
        {
            get
            {
                return new Feature(
                    ExcludeFieldListValue,
                    new List<string>() { "-ef", "--exclude-field" },
                    "Exclude a game/rom field from outputs",
                    FeatureType.List,
                    longDescription: "Exclude any valid item or machine field from outputs. Examples include: romof, publisher, and offset.");
            }
        }
        
        public const string ExtAListValue = "exta";
        private static Feature _extaListInput
        {
            get
            {
                return new Feature(
                    ExtAListValue,
                    new List<string>() { "-exta", "--exta" },
                    "Set extension to be included in first DAT",
                    FeatureType.List,
                    longDescription: "Set the extension to be used to populate the first DAT. Multiple instances of this flag are allowed.");
            }
        }

        public const string ExtBListValue = "extb";
        private static Feature _extbListInput
        {
            get
            {
                return new Feature(
                    ExtBListValue,
                    new List<string>() { "-extb", "--extb" },
                    "Set extension to be included in second DAT",
                    FeatureType.List,
                    longDescription: "Set the extension to be used to populate the second DAT. Multiple instances of this flag are allowed.");
            }
        }

        public const string GameDescriptionListValue = "game-description";
        private static Feature _gameDescriptionListInput
        {
            get
            {
                return new Feature(
                    GameDescriptionListValue,
                    new List<string>() { "-gd", "--game-description" },
                    "Filter by game description",
                    FeatureType.List,
                    longDescription: "Include only items with this game description in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string GameNameListValue = "game-name";
        private static Feature _gameNameListInput
        {
            get
            {
                return new Feature(
                    GameNameListValue,
                    new List<string>() { "-gn", "--game-name" },
                    "Filter by game name",
                    FeatureType.List,
                    longDescription: "Include only items with this game name in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string GameTypeListValue = "game-type";
        private static Feature _gameTypeListInput
        {
            get
            {
                return new Feature(
                    GameTypeListValue,
                    new List<string>() { "-gt", "--game-type" },
                    "Include only games with a given type",
                    FeatureType.List,
                    longDescription: @"Include only items with this game type in the output. Multiple instances of this flag are allowed.
Possible values are: None, Bios, Device, Mechanical");
            }
        }

        public const string ItemNameListValue = "item-name";
        private static Feature _itemNameListInput
        {
            get
            {
                return new Feature(
                    ItemNameListValue,
                    new List<string>() { "-rn", "--item-name" },
                    "Filter by item name",
                    FeatureType.List,
                    longDescription: "Include only items with this item name in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string ItemTypeListValue = "item-type";
        private static Feature _itemTypeListInput
        {
            get
            {
                return new Feature(
                    ItemTypeListValue,
                    new List<string>() { "-rt", "--item-type" },
                    "Filter by item type",
                    FeatureType.List,
                    longDescription: "Include only items with this item type in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string Md5ListValue = "md5";
        private static Feature _md5ListInput
        {
            get
            {
                return new Feature(
                    Md5ListValue,
                    new List<string>() { "-md5", "--md5" },
                    "Filter by MD5 hash",
                    FeatureType.List,
                    longDescription: "Include only items with this MD5 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotCrcListValue = "not-crc";
        private static Feature _notCrcListInput
        {
            get
            {
                return new Feature(
                    NotCrcListValue,
                    new List<string>() { "-ncrc", "--not-crc" },
                    "Filter by not CRC hash",
                    FeatureType.List,
                    longDescription: "Include only items without this CRC hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotGameDescriptionListValue = "not-game-description";
        private static Feature _notGameDescriptionListInput
        {
            get
            {
                return new Feature(
                    NotGameDescriptionListValue,
                    new List<string>() { "-ngd", "--not-game-description" },
                    "Filter by not game description",
                    FeatureType.List,
                    longDescription: "Include only items without this game description in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotGameNameListValue = "not-game-name";
        private static Feature _notGameNameListInput
        {
            get
            {
                return new Feature(
                    NotGameNameListValue,
                    new List<string>() { "-ngn", "--not-game-name" },
                    "Filter by not game name",
                    FeatureType.List,
                    longDescription: "Include only items without this game name in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotGameTypeListValue = "not-game-type";
        private static Feature _notGameTypeListInput
        {
            get
            {
                return new Feature(
                    NotGameTypeListValue,
                    new List<string>() { "-ngt", "--not-game-type" },
                    "Exclude only games with a given type",
                    FeatureType.List,
                    longDescription: @"Include only items without this game type in the output. Multiple instances of this flag are allowed.
Possible values are: None, Bios, Device, Mechanical");
            }
        }

        public const string NotItemNameListValue = "not-item-name";
        private static Feature _notItemNameListInput
        {
            get
            {
                return new Feature(
                    NotItemNameListValue,
                    new List<string>() { "-nrn", "--not-item-name" },
                    "Filter by not item name",
                    FeatureType.List,
                    longDescription: "Include only items without this item name in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotItemTypeListValue = "not-item-type";
        private static Feature _notItemTypeListInput
        {
            get
            {
                return new Feature(
                    NotItemTypeListValue,
                    new List<string>() { "-nrt", "--not-item-type" },
                    "Filter by not item type",
                    FeatureType.List,
                    longDescription: "Include only items without this item type in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotMd5ListValue = "not-md5";
        private static Feature _notMd5ListInput
        {
            get
            {
                return new Feature(
                    NotMd5ListValue,
                    new List<string>() { "-nmd5", "--not-md5" },
                    "Filter by not MD5 hash",
                    FeatureType.List,
                    longDescription: "Include only items without this MD5 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotRipeMd160ListValue = "not-ripemd160";
        private static Feature _notRipeMd160ListInput
        {
            get
            {
                return new Feature(
                    NotRipeMd160ListValue,
                    new List<string>() { "-nripemd160", "--not-ripemd160" },
                    "Filter by not RIPEMD160 hash",
                    FeatureType.List,
                    longDescription: "Include only items without this RIPEMD160 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotSha1ListValue = "not-sha1";
        private static Feature _notSha1ListInput
        {
            get
            {
                return new Feature(
                    NotSha1ListValue,
                    new List<string>() { "-nsha1", "--not-sha1" },
                    "Filter by not SHA-1 hash",
                    FeatureType.List,
                    longDescription: "Include only items without this SHA-1 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotSha256ListValue = "not-sha256";
        private static Feature _notSha256ListInput
        {
            get
            {
                return new Feature(
                    NotSha256ListValue,
                    new List<string>() { "-nsha256", "--not-sha256" },
                    "Filter by not SHA-256 hash",
                    FeatureType.List,
                    longDescription: "Include only items without this SHA-256 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotSha384ListValue = "not-sha384";
        private static Feature _notSha384ListInput
        {
            get
            {
                return new Feature(
                    NotSha384ListValue,
                    new List<string>() { "-nsha384", "--not-sha384" },
                    "Filter by not SHA-384 hash",
                    FeatureType.List,
                    longDescription: "Include only items without this SHA-384 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotSha512ListValue = "not-sha512";
        private static Feature _notSha512ListInput
        {
            get
            {
                return new Feature(
                    NotSha512ListValue,
                    new List<string>() { "-nsha512", "--not-sha512" },
                    "Filter by not SHA-512 hash",
                    FeatureType.List,
                    longDescription: "Include only items without this SHA-512 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string NotStatusListValue = "not-status";
        private static Feature _notStatusListInput
        {
            get
            {
                return new Feature(
                    NotStatusListValue,
                    new List<string>() { "-nis", "--not-status" },
                    "Exclude only items with a given status",
                    FeatureType.List,
                    longDescription: @"Include only items without this item status in the output. Multiple instances of this flag are allowed.
Possible values are: None, Good, BadDump, Nodump, Verified");
            }
        }

        public const string OutputTypeListValue = "output-type";
        private static Feature _outputTypeListInput
        {
            get
            {
                return new Feature(
                    OutputTypeListValue,
                    new List<string>() { "-ot", "--output-type" },
                    "Output DATs to a specified format",
                    FeatureType.List,
                    longDescription: @"Add outputting the created DAT to known format. Multiple instances of this flag are allowed.

Possible values are:
    all              - All available DAT types
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
    ripemd160        - RIPEMD160
    sd, sabredat     - SabreDat XML
    sfv              - SFV
    sha1             - SHA1
    sha256           - SHA256
    sha384           - SHA384
    sha512           - SHA512
    smdb, everdrive  - Everdrive SMDB
    sl, softwarelist - MAME Software List XML
    ssv              - Standardized Semicolon-Separated Value
    tsv              - Standardized Tab-Separated Value
    xml, logiqx      - Logiqx XML");
            }
        }

        public const string ReportTypeListValue = "report-type";
        private static Feature _reportTypeListInput
        {
            get
            {
                return new Feature(
                    ReportTypeListValue,
                    new List<string>() { "-srt", "--report-type" },
                    "Output statistics to a specified format",
                    FeatureType.List,
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

        public const string RipeMd160ListValue = "ripemd160";
        private static Feature _ripeMd160ListInput
        {
            get
            {
                return new Feature(
                    RipeMd160ListValue,
                    new List<string>() { "-ripemd160", "--ripemd160" },
                    "Filter by RIPEMD160 hash",
                    FeatureType.List,
                    longDescription: "Include only items with this RIPEMD160 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string Sha1ListValue = "sha1";
        private static Feature _sha1ListInput
        {
            get
            {
                return new Feature(
                    Sha1ListValue,
                    new List<string>() { "-sha1", "--sha1" },
                    "Filter by SHA-1 hash",
                    FeatureType.List,
                    longDescription: "Include only items with this SHA-1 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string Sha256ListValue = "sha256";
        private static Feature _sha256ListInput
        {
            get
            {
                return new Feature(
                    Sha256ListValue,
                    new List<string>() { "-sha256", "--sha256" },
                    "Filter by SHA-256 hash",
                    FeatureType.List,
                    longDescription: "Include only items with this SHA-256 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string Sha384ListValue = "sha384";
        private static Feature _sha384ListInput
        {
            get
            {
                return new Feature(
                    Sha384ListValue,
                    new List<string>() { "-sha384", "--sha384" },
                    "Filter by SHA-384 hash",
                    FeatureType.List,
                    longDescription: "Include only items with this SHA-384 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string Sha512ListValue = "sha512";
        private static Feature _sha512ListInput
        {
            get
            {
                return new Feature(
                    Sha512ListValue,
                    new List<string>() { "-sha512", "--sha512" },
                    "Filter by SHA-512 hash",
                    FeatureType.List,
                    longDescription: "Include only items with this SHA-512 hash in the output. Additionally, the user can specify an exact match or full C#-style regex for pattern matching. Multiple instances of this flag are allowed.");
            }
        }

        public const string StatusListValue = "status";
        private static Feature _statusListInput
        {
            get
            {
                return new Feature(
                    StatusListValue,
                    new List<string>() { "-is", "--status" },
                    "Include only items with a given status",
                    FeatureType.List,
                    longDescription: @"Include only items with this item status in the output. Multiple instances of this flag are allowed.
Possible values are: None, Good, BadDump, Nodump, Verified");
            }
        }

        public const string UpdateFieldListValue = "update-field";
        private static Feature _updateFieldListInput
        {
            get
            {
                return new Feature(
                    UpdateFieldListValue,
                    new List<string>() { "-uf", "--update-field" },
                    "Update a game/rom field from base DATs",
                    FeatureType.List,
                    longDescription: "Update any valid item or machine field from base DAT(s). Examples include: romof, publisher, and offset.");
            }
        }

        #endregion

        #region Private String features

        public const string AddExtensionStringValue = "add-extension";
        private static Feature _addExtensionStringInput
        {
            get
            {
                return new Feature(
                    AddExtensionStringValue,
                    new List<string>() { "-ae", "--add-extension" },
                    "Add an extension to each item",
                    FeatureType.String,
                    longDescription: "Add a postfix extension to each full item name.");
            }
        }

        public const string AuthorStringValue = "author";
        private static Feature _authorStringInput
        {
            get
            {
                return new Feature(
                    AuthorStringValue,
                    new List<string>() { "-au", "--author" },
                    "Set the author of the DAT",
                    FeatureType.String,
                    longDescription: "Set the author header field for the output DAT(s)");
            }
        }

        public const string CategoryStringValue = "category";
        private static Feature _categoryStringInput
        {
            get
            {
                return new Feature(
                    CategoryStringValue,
                    new List<string>() { "-c", "--category" },
                    "Set the category of the DAT",
                    FeatureType.String,
                    longDescription: "Set the category header field for the output DAT(s)");
            }
        }

        public const string CommentStringValue = "comment";
        private static Feature _commentStringInput
        {
            get
            {
                return new Feature(
                    CommentStringValue,
                    new List<string>() { "-co", "--comment" },
                    "Set a new comment of the DAT",
                    FeatureType.String,
                    longDescription: "Set the comment header field for the output DAT(s)");
            }
        }

        public const string DateStringValue = "date";
        private static Feature _dateStringInput
        {
            get
            {
                return new Feature(
                    DateStringValue,
                    new List<string>() { "-da", "--date" },
                    "Set a new date",
                    FeatureType.String,
                    longDescription: "Set the date header field for the output DAT(s)");
            }
        }

        public const string DescriptionStringValue = "description";
        private static Feature _descriptionStringInput
        {
            get
            {
                return new Feature(
                    DescriptionStringValue,
                    new List<string>() { "-de", "--description" },
                    "Set the description of the DAT",
                    FeatureType.String,
                    longDescription: "Set the description header field for the output DAT(s)");
            }
        }

        public const string EmailStringValue = "email";
        private static Feature _emailStringInput
        {
            get
            {
                return new Feature(
                    EmailStringValue,
                    new List<string>() { "-em", "--email" },
                    "Set a new email of the DAT",
                    FeatureType.String,
                    longDescription: "Set the email header field for the output DAT(s)");
            }
        }

        public const string EqualStringValue = "equal";
        private static Feature _equalStringInput
        {
            get
            {
                return new Feature(
                    EqualStringValue,
                    new List<string>() { "-seq", "--equal" },
                    "Filter by size ==",
                    FeatureType.String,
                    longDescription: "Only include items of this exact size in the output DAT. Users can specify either a regular integer number or a number with a standard postfix. e.g. 8kb => 8000 or 8kib => 8192");
            }
        }

        public const string FilenameStringValue = "filename";
        private static Feature _filenameStringInput
        {
            get
            {
                return new Feature(
                    FilenameStringValue,
                    new List<string>() { "-f", "--filename" },
                    "Set the external name of the DAT",
                    FeatureType.String,
                    longDescription: "Set the external filename for the output DAT(s)");
            }
        }

        public const string ForceMergingStringInput = "forcemerging";
        private static Feature _forcemergingStringInput
        {
            get
            {
                return new Feature(
                    ForceMergingStringInput,
                    new List<string>() { "-fm", "--forcemerging" },
                    "Set force merging",
                    FeatureType.String,
                    longDescription: @"Set the forcemerging tag to the given value.
Possible values are: None, Split, Merged, Nonmerged, Full");
            }
        }

        public const string ForceNodumpStringInput = "forcenodump";
        private static Feature _forcenodumpStringInput
        {
            get
            {
                return new Feature(
                    ForceNodumpStringInput,
                    new List<string>() { "-fn", "--forcenodump" },
                    "Set force nodump",
                    FeatureType.String,
                    longDescription: @"Set the forcenodump tag to the given value.
Possible values are: None, Obsolete, Required, Ignore");
            }
        }

        public const string ForcePackingStringInput = "forcepacking";
        private static Feature _forcepackingStringInput
        {
            get
            {
                return new Feature(
                    ForcePackingStringInput,
                    new List<string>() { "-fp", "--forcepacking" },
                    "Set force packing",
                    FeatureType.String,
                    longDescription: @"Set the forcepacking tag to the given value.
Possible values are: None, Zip, Unzip");
            }
        }

        public const string GreaterStringValue = "greater";
        private static Feature _greaterStringInput
        {
            get
            {
                return new Feature(
                    GreaterStringValue,
                    new List<string>() { "-sgt", "--greater" },
                    "Filter by size >=",
                    FeatureType.String,
                    longDescription: "Only include items whose size is greater than or equal to this value in the output DAT. Users can specify either a regular integer number or a number with a standard postfix. e.g. 8kb => 8000 or 8kib => 8192");
            }
        }

        public const string HeaderStringValue = "header";
        private static Feature _headerStringInput
        {
            get
            {
                return new Feature(
                    HeaderStringValue,
                    new List<string>() { "-h", "--header" },
                    "Set a header skipper to use, blank means all",
                    FeatureType.String,
                    longDescription: "Set the header special field for the output DAT(s). In file rebuilding, this flag allows for either all copier headers (using \"\") or specific copier headers by name (such as \"fds.xml\") to determine if a file matches or not.");

            }
        }

        public const string HomepageStringValue = "homepage";
        private static Feature _homepageStringInput
        {
            get
            {
                return new Feature(
                    HomepageStringValue,
                    new List<string>() { "-hp", "--homepage" },
                    "Set a new homepage of the DAT",
                    FeatureType.String,
                    longDescription: "Set the homepage header field for the output DAT(s)");
            }
        }

        public const string LessStringValue = "less";
        private static Feature _lessStringInput
        {
            get
            {
                return new Feature(
                    LessStringValue,
                    new List<string>() { "-slt", "--less" },
                    "Filter by size =<",
                    FeatureType.String,
                    longDescription: "Only include items whose size is less than or equal to this value in the output DAT. Users can specify either a regular integer number or a number with a standard postfix. e.g. 8kb => 8000 or 8kib => 8192");
            }
        }

        public const string NameStringValue = "name";
        private static Feature _nameStringInput
        {
            get
            {
                return new Feature(
                    NameStringValue,
                    new List<string>() { "-n", "--name" },
                    "Set the internal name of the DAT",
                    FeatureType.String,
                    longDescription: "Set the name header field for the output DAT(s)");
            }
        }

        public const string OutputDirStringValue = "output-dir";
        private static Feature _outputDirStringInput
        {
            get
            {
                return new Feature(
                    OutputDirStringValue,
                    new List<string>() { "-out", "--output-dir" },
                    "Output directory",
                    FeatureType.String,
                    longDescription: "This sets an output folder to be used when the files are created. If a path is not defined, the runtime directory is used instead.");
            }
        }

        public const string PostfixStringValue = "postfix";
        private static Feature _postfixStringInput
        {
            get
            {
                return new Feature(
                    PostfixStringValue,
                    new List<string>() { "-post", "--postfix" },
                    "Set postfix for all lines",
                    FeatureType.String,
                    longDescription: @"Set a generic postfix to be appended to all outputted lines.

Some special strings that can be used:
- %game% / %machine% - Replaced with the Game/Machine name
- %name% - Replaced with the Rom name
- %manufacturer% - Replaced with game Manufacturer
- %publisher% - Replaced with game Publisher
- %crc% - Replaced with the CRC
- %md5% - Replaced with the MD5
- %ripemd160% - Replaced with the RIPEMD160
- %sha1% - Replaced with the SHA-1
- %sha256% - Replaced with the SHA-256
- %sha384% - Replaced with the SHA-384
- %sha512% - Replaced with the SHA-512
- %size% - Replaced with the size");
            }
        }

        public const string PrefixStringValue = "prefix";
        private static Feature _prefixStringInput
        {
            get
            {
                return new Feature(
                    PrefixStringValue,
                    new List<string>() { "-pre", "--prefix" },
                    "Set prefix for all lines",
                    FeatureType.String,
                    longDescription: @"Set a generic prefix to be prepended to all outputted lines.

Some special strings that can be used:
- %game% / %machine% - Replaced with the Game/Machine name
- %name% - Replaced with the Rom name
- %manufacturer% - Replaced with game Manufacturer
- %publisher% - Replaced with game Publisher
- %crc% - Replaced with the CRC
- %md5% - Replaced with the MD5
- %sha1% - Replaced with the SHA-1
- %sha256% - Replaced with the SHA-256
- %sha384% - Replaced with the SHA-384
- %sha512% - Replaced with the SHA-512
- %size% - Replaced with the size");
            }
        }

        public const string ReplaceExtensionStringValue = "replace-extension";
        private static Feature _replaceExtensionStringInput
        {
            get
            {
                return new Feature(
                    ReplaceExtensionStringValue,
                    new List<string>() { "-rep", "--replace-extension" },
                    "Replace all extensions with specified",
                    FeatureType.String,
                    longDescription: "When an extension exists, replace it with the provided instead.");
            }
        }

        public const string RootStringValue = "root";
        private static Feature _rootStringInput
        {
            get
            {
                return new Feature(
                    RootStringValue,
                    new List<string>() { "-r", "--root" },
                    "Set a new rootdir",
                    FeatureType.String,
                    longDescription: "Set the rootdir (as used by SuperDAT mode) for the output DAT(s).");
            }
        }

        public const string RootDirStringValue = "root-dir";
        private static Feature _rootDirStringInput
        {
            get
            {
                return new Feature(
                    RootDirStringValue,
                    new List<string>() { "-rd", "--root-dir" },
                    "Set the root directory for calc",
                    FeatureType.String,
                    longDescription: "In the case that the files will not be stored from the root directory, a new root can be set for path length calculations.");
            }
        }

        public const string TempStringValue = "temp";
        private static Feature _tempStringInput
        {
            get
            {
                return new Feature(
                    TempStringValue,
                    new List<string>() { "-t", "--temp" },
                    "Set the temporary directory to use",
                    FeatureType.String,
                    longDescription: "Optionally, a temp folder can be supplied in the case the default temp directory is not preferred.");
            }
        }

        public const string UrlStringValue = "url";
        private static Feature _urlStringInput
        {
            get
            {
                return new Feature(
                    UrlStringValue,
                    new List<string>() { "-u", "--url" },
                    "Set a new URL of the DAT",
                    FeatureType.String,
                    longDescription: "Set the URL header field for the output DAT(s)");
            }
        }

        public const string VersionStringValue = "version";
        private static Feature _versionStringInput
        {
            get
            {
                return new Feature(
                    VersionStringValue,
                    new List<string>() { "-v", "--version" },
                    "Set the version of the DAT",
                    FeatureType.String,
                    longDescription: "Set the version header field for the output DAT(s)");
            }
        }

        #endregion

        public static Help RetrieveHelp()
        {
            // Create and add the header to the Help object
            string barrier = "-----------------------------------------";
            List<string> helpHeader = new List<string>()
            {
                "SabreTools - Manipulate, convert, and use DAT files",
                barrier,
                "Usage: SabreTools [option] [flags] [filename|dirname] ...",
                string.Empty
            };

            // Create the base help object with header
            Help help = new Help(helpHeader);

            // Add all of the features
            help.Add(new HelpFeature());
            help.Add(new DetailedHelpFeature());
            help.Add(new ScriptFeature());
            help.Add(new DatFromDirFeature());
            help.Add(new ExtractFeature());
            help.Add(new RestoreFeature());
            help.Add(new SortFeature());
            help.Add(new SplitFeature());
            help.Add(new StatsFeature());
            help.Add(new UpdateFeature());
            help.Add(new VerifyFeature());

            return help;
        }

        #region Top-level Features

        private class SabreToolsFeature : TopLevel
        {
            #region Specific Extraction

            /// <summary>
            /// Get DatHeader from feature list
            /// </summary>
            protected DatHeader GetDatHeader(Dictionary<string, Feature> features)
            {
                DatHeader datHeader = new DatHeader
                {
                    AddExtension = GetString(features, AddExtensionStringValue),
                    Author = GetString(features, AuthorStringValue),
                    Category = GetString(features, CategoryStringValue),
                    Comment = GetString(features, CommentStringValue),
                    Date = GetString(features, DateStringValue),
                    DedupeRoms = GetDedupeType(features),
                    Description = GetString(features, DescriptionStringValue),
                    Email = GetString(features, EmailStringValue),
                    FileName = GetString(features, FilenameStringValue),
                    ForceMerging = Utilities.GetForceMerging(GetString(features, ForceMergingStringInput)),
                    ForceNodump = Utilities.GetForceNodump(GetString(features, ForceNodumpStringInput)),
                    ForcePacking = Utilities.GetForcePacking(GetString(features, ForcePackingStringInput)),
                    GameName = GetBoolean(features, GamePrefixValue),
                    Header = GetString(features, HeaderStringValue),
                    Homepage = GetString(features, HomepageStringValue),
                    KeepEmptyGames = GetBoolean(features, KeepEmptyGamesValue),
                    Name = GetString(features, NameStringValue),
                    OneRom = GetBoolean(features, OneRomPerGameValue),
                    Postfix = GetString(features, PostfixStringValue),
                    Prefix = GetString(features, PrefixStringValue),
                    Quotes = GetBoolean(features, QuotesValue),
                    RemoveExtension = GetBoolean(features, RemoveExtensionsValue),
                    ReplaceExtension = GetString(features, ReplaceExtensionStringValue),
                    Romba = GetBoolean(features, RombaValue),
                    RootDir = GetString(features, RootStringValue),
                    SceneDateStrip = GetBoolean(features, SceneDateStripValue),
                    Type = GetBoolean(features, SuperdatValue) ? "SuperDAT" : null,
                    Url = GetString(features, UrlStringValue),
                    UseRomName = GetBoolean(features, RomsValue),
                    Version = GetString(features, VersionStringValue),
                };

                bool deprecated = GetBoolean(features, DeprecatedValue);
                foreach (string ot in GetList(features, OutputTypeListValue))
                {
                    DatFormat dftemp = Utilities.GetDatFormat(ot);
                    if (dftemp == DatFormat.Logiqx && deprecated)
                        datHeader.DatFormat |= DatFormat.LogiqxDeprecated;
                    else
                        datHeader.DatFormat |= dftemp;
                }

                foreach (string field in GetList(features, ExcludeFieldListValue))
                {
                    datHeader.ExcludeFields[(int)Utilities.GetField(field)] = true;
                }

                return datHeader;
            }

            /// <summary>
            /// Get DedupeType from feature list
            /// </summary>
            protected DedupeType GetDedupeType(Dictionary<string, Feature> features)
            {
                if (GetBoolean(features, DedupValue))
                    return DedupeType.Full;
                else if (GetBoolean(features, GameDedupValue))
                    return DedupeType.Game;
                else
                    return DedupeType.None;
            }

            /// <summary>
            /// Get Filter from feature list
            /// </summary>
            protected Filter GetFilter(Dictionary<string, Feature> features)
            {
                Filter filter = new Filter();

                // CRC
                filter.CRC.NegativeSet.AddRange(GetList(features, NotCrcListValue));
                filter.CRC.PositiveSet.AddRange(GetList(features, CrcListValue));

                // Include 'of" in game filters
                filter.IncludeOfInGame.Neutral = GetBoolean(features, MatchOfTagsValue);

                // Item name
                filter.ItemName.NegativeSet.AddRange(GetList(features, NotItemNameListValue));
                filter.ItemName.PositiveSet.AddRange(GetList(features, ItemNameListValue));

                // Item status
                foreach (string stat in GetList(features, NotStatusListValue))
                {
                    filter.ItemStatuses.Negative |= Utilities.GetItemStatus(stat);
                }
                foreach (string stat in GetList(features, StatusListValue))
                {
                    filter.ItemStatuses.Positive |= Utilities.GetItemStatus(stat);
                }

                // Item type
                filter.ItemTypes.NegativeSet.AddRange(GetList(features, NotItemTypeListValue));
                filter.ItemTypes.PositiveSet.AddRange(GetList(features, ItemTypeListValue));

                // Machine description
                filter.MachineDescription.NegativeSet.AddRange(GetList(features, NotGameDescriptionListValue));
                filter.MachineDescription.PositiveSet.AddRange(GetList(features, GameDescriptionListValue));

                // Machine name
                filter.MachineName.NegativeSet.AddRange(GetList(features, NotGameNameListValue));
                filter.MachineName.PositiveSet.AddRange(GetList(features, GameNameListValue));

                // Machine type
                foreach (string mach in GetList(features, NotGameTypeListValue))
                {
                    filter.MachineTypes.Negative |= Utilities.GetMachineType(mach);
                }
                foreach (string mach in GetList(features, GameTypeListValue))
                {
                    filter.MachineTypes.Positive |= Utilities.GetMachineType(mach);
                }

                // MD5
                filter.MD5.NegativeSet.AddRange(GetList(features, NotMd5ListValue));
                filter.MD5.PositiveSet.AddRange(GetList(features, Md5ListValue));

                // RIPEMD160
                filter.RIPEMD160.NegativeSet.AddRange(GetList(features, NotRipeMd160ListValue));
                filter.RIPEMD160.PositiveSet.AddRange(GetList(features, RipeMd160ListValue));

                // Root directory
                filter.Root.Neutral = GetString(features, RootDirStringValue);

                // Runnable
                if (GetBoolean(features, NotRunnableValue))
                    filter.Runnable.Neutral = false;
                if (GetBoolean(features, RunnableValue))
                    filter.Runnable.Neutral = true;

                // SHA-1
                filter.SHA1.NegativeSet.AddRange(GetList(features, NotSha1ListValue));
                filter.SHA1.PositiveSet.AddRange(GetList(features, Sha1ListValue));

                // SHA-256
                filter.SHA256.NegativeSet.AddRange(GetList(features, NotSha256ListValue));
                filter.SHA256.PositiveSet.AddRange(GetList(features, Sha256ListValue));

                // SHA-384
                filter.SHA384.NegativeSet.AddRange(GetList(features, NotSha384ListValue));
                filter.SHA384.PositiveSet.AddRange(GetList(features, Sha384ListValue));

                // SHA-512
                filter.SHA512.NegativeSet.AddRange(GetList(features, NotSha512ListValue));
                filter.SHA512.PositiveSet.AddRange(GetList(features, Sha512ListValue));

                // Single game in output
                filter.Single.Neutral = GetBoolean(features, SingleSetValue);

                // Size
                filter.Size.Negative = Utilities.GetSizeFromString(GetString(features, LessStringValue));
                filter.Size.Neutral = Utilities.GetSizeFromString(GetString(features, EqualStringValue));
                filter.Size.Positive = Utilities.GetSizeFromString(GetString(features, GreaterStringValue));

                // Trim to NTFS length
                filter.Trim.Neutral = GetBoolean(features, TrimValue);

                return filter;
            }
            
            /// <summary>
            /// Get omit from scan from feature list
            /// </summary>
            protected Hash GetOmitFromScan(Dictionary<string, Feature> features)
            {
                Hash omitFromScan = Hash.DeepHashes; // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually

                if (GetBoolean(features, SkipMd5Value))
                    omitFromScan |= Hash.MD5;
                if (GetBoolean(features, SkipRipeMd160Value))
                    omitFromScan &= ~Hash.RIPEMD160; // TODO: This needs to be inverted later
                if (GetBoolean(features, SkipSha1Value))
                    omitFromScan |= Hash.SHA1;
                if (GetBoolean(features, SkipSha256Value))
                    omitFromScan &= ~Hash.SHA256; // TODO: This needs to be inverted later
                if (GetBoolean(features, SkipSha384Value))
                    omitFromScan &= ~Hash.SHA384; // TODO: This needs to be inverted later
                if (GetBoolean(features, SkipSha512Value))
                    omitFromScan &= ~Hash.SHA512; // TODO: This needs to be inverted later

                return omitFromScan;
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
                else if (GetBoolean(features, TorrentLrzipValue))
                    return OutputFormat.TorrentLRZip;
                else if (GetBoolean(features, TorrentLz4Value))
                    return OutputFormat.TorrentLZ4;
                else if (GetBoolean(features, TorrentRarValue))
                    return OutputFormat.TorrentRar;
                else if (GetBoolean(features, TorrentXzValue))
                    return OutputFormat.TorrentXZ;
                else if (GetBoolean(features, TorrentZipValue))
                    return OutputFormat.TorrentZip;
                else if (GetBoolean(features, TorrentZpaqValue))
                    return OutputFormat.TorrentZPAQ;
                else if (GetBoolean(features, TorrentZstdValue))
                    return OutputFormat.TorrentZstd;
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
            /// Get SplitType from feature list
            /// </summary>
            protected SplitType GetSplitType(Dictionary<string, Feature> features)
            {
                SplitType splitType = SplitType.None;
                if (GetBoolean(features, DatDeviceNonMergedValue))
                    splitType = SplitType.DeviceNonMerged;
                else if (GetBoolean(features, DatFullNonMergedValue))
                    splitType = SplitType.FullNonMerged;
                else if (GetBoolean(features, DatMergedValue))
                    splitType = SplitType.Merged;
                else if (GetBoolean(features, DatNonMergedValue))
                    splitType = SplitType.NonMerged;
                else if (GetBoolean(features, DatSplitValue))
                    splitType = SplitType.Split;

                return splitType;
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
                    statDatFormat |= Utilities.GetStatFormat(rt);
                }

                return statDatFormat;
            }

            /// <summary>
            /// Get update fields from feature list
            /// </summary>
            protected List<Field> GetUpdateFields(Dictionary<string, Feature> features)
            {
                List<Field> updateFields = new List<Field>();

                if (GetBoolean(features, UpdateDescriptionValue))
                {
                    Globals.Logger.User($"This flag '{UpdateDescriptionValue}' is deprecated, please use {string.Join(", ", _updateFieldListInput.Flags)} instead");
                    updateFields.Add(Field.Description);
                }

                if (GetBoolean(features, UpdateGameTypeValue))
                {
                    Globals.Logger.User($"This flag '{UpdateGameTypeValue}' is deprecated, please use {string.Join(", ", _updateFieldListInput.Flags)} instead");
                    updateFields.Add(Field.MachineType);
                }

                if (GetBoolean(features, UpdateHashesValue))
                {
                    Globals.Logger.User($"This flag '{UpdateHashesValue}' is deprecated, please use {string.Join(", ", _updateFieldListInput.Flags)} instead");
                    updateFields.Add(Field.CRC);
                    updateFields.Add(Field.MD5);
                    updateFields.Add(Field.RIPEMD160);
                    updateFields.Add(Field.SHA1);
                    updateFields.Add(Field.SHA256);
                    updateFields.Add(Field.SHA384);
                    updateFields.Add(Field.SHA512);
                }

                if (GetBoolean(features, UpdateManufacturerValue))
                {
                    Globals.Logger.User($"This flag '{UpdateManufacturerValue}' is deprecated, please use {string.Join(", ", _updateFieldListInput.Flags)} instead");
                    updateFields.Add(Field.Manufacturer);
                }

                if (GetBoolean(features, UpdateNamesValue))
                {
                    Globals.Logger.User($"This flag '{UpdateNamesValue}' is deprecated, please use {string.Join(", ", _updateFieldListInput.Flags)} instead");
                    updateFields.Add(Field.Name);
                }

                if (GetBoolean(features, UpdateParentsValue))
                {
                    Globals.Logger.User($"This flag '{UpdateParentsValue}' is deprecated, please use {string.Join(", ", _updateFieldListInput.Flags)} instead");
                    updateFields.Add(Field.CloneOf);
                    updateFields.Add(Field.RomOf);
                    updateFields.Add(Field.SampleOf);
                }

                if (GetBoolean(features, UpdateYearValue))
                {
                    Globals.Logger.User($"This flag '{UpdateYearValue}' is deprecated, please use {string.Join(", ", _updateFieldListInput.Flags)} instead");
                    updateFields.Add(Field.Year);
                }

                foreach (string field in GetList(features, UpdateFieldListValue))
                {
                    updateFields.Add(Utilities.GetField(field));
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
        }

        private class DetailedHelpFeature : SabreToolsFeature
        {
            public const string Value = "Help (Detailed)";

            public DetailedHelpFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-??", "-hd", "--help-detailed" };
                this.Description = "Show this detailed help";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Display a detailed help text to the screen.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override bool ProcessArgs(string[] args, Help help)
            {
                // If we had something else after help
                if (args.Length > 1)
                {
                    help.OutputIndividualFeature(args[1], includeLongDescription: true);
                    return true;
                }

                // Otherwise, show generic help
                else
                {
                    help.OutputAllHelp();
                    return true;
                }
            }
        }

        private class DatFromDirFeature : SabreToolsFeature
        {
            public const string Value = "DATFromDir";

            public DatFromDirFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-d", "--d2d", "--dfd" };
                this.Description = "Create DAT(s) from an input directory";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Create a DAT file from an input directory or set of files. By default, this will output a DAT named based on the input directory and the current date. It will also treat all archives as possible games and add all three hashes (CRC, MD5, SHA-1) for each file.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(_skipMd5Flag);
                AddFeature(_skipRipeMd160Flag);
                AddFeature(_skipSha1Flag);
                AddFeature(_skipSha256Flag);
                AddFeature(_skipSha384Flag);
                AddFeature(_skipSha512Flag);
                AddFeature(_noAutomaticDateFlag);
                AddFeature(_forcepackingStringInput);
                AddFeature(_archivesAsFilesFlag);
                AddFeature(_outputTypeListInput);
                    this[_outputTypeListInput].AddFeature(_deprecatedFlag);
                AddFeature(_rombaFlag);
                AddFeature(_skipArchivesFlag);
                AddFeature(_skipFilesFlag);
                AddFeature(_filenameStringInput);
                AddFeature(_nameStringInput);
                AddFeature(_descriptionStringInput);
                AddFeature(_categoryStringInput);
                AddFeature(_versionStringInput);
                AddFeature(_authorStringInput);
                AddFeature(_emailStringInput);
                AddFeature(_homepageStringInput);
                AddFeature(_urlStringInput);
                AddFeature(_commentStringInput);
                AddFeature(_superdatFlag);
                AddFeature(_excludeFieldListInput);
                AddFeature(_oneRomPerGameFlag);
                AddFeature(_sceneDateStripFlag);
                AddFeature(_addBlankFilesFlag);
                AddFeature(_addDateFlag);
                AddFeature(_copyFilesFlag);
                AddFeature(_headerStringInput);
                AddFeature(_chdsAsFilesFlag);
                AddFeature(_gameNameListInput);
                AddFeature(_notGameNameListInput);
                AddFeature(_gameDescriptionListInput);
                AddFeature(_notGameDescriptionListInput);
                AddFeature(_matchOfTagsFlag);
                AddFeature(_itemNameListInput);
                AddFeature(_notItemNameListInput);
                AddFeature(_itemTypeListInput);
                AddFeature(_notItemTypeListInput);
                AddFeature(_greaterStringInput);
                AddFeature(_lessStringInput);
                AddFeature(_equalStringInput);
                AddFeature(_crcListInput);
                AddFeature(_notCrcListInput);
                AddFeature(_md5ListInput);
                AddFeature(_notMd5ListInput);
                AddFeature(_ripeMd160ListInput);
                AddFeature(_notRipeMd160ListInput);
                AddFeature(_sha1ListInput);
                AddFeature(_notSha1ListInput);
                AddFeature(_sha256ListInput);
                AddFeature(_notSha256ListInput);
                AddFeature(_sha384ListInput);
                AddFeature(_notSha384ListInput);
                AddFeature(_sha512ListInput);
                AddFeature(_notSha512ListInput);
                AddFeature(_statusListInput);
                AddFeature(_notStatusListInput);
                AddFeature(_gameTypeListInput);
                AddFeature(_notGameTypeListInput);
                AddFeature(_runnableFlag);
                AddFeature(_notRunnableFlag);
                AddFeature(_tempStringInput);
                AddFeature(_outputDirStringInput);
                AddFeature(_threadsInt32Input);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                bool addBlankFiles = GetBoolean(features, AddBlankFilesValue);
                bool addFileDates = GetBoolean(features, AddDateValue);
                bool archivesAsFiles = GetBoolean(features, ArchivesAsFilesValue);
                bool chdsAsFiles = GetBoolean(features, ChdsAsFilesValue);
                bool copyFiles = GetBoolean(features, CopyFilesValue);
                bool noAutomaticDate = GetBoolean(features, NoAutomaticDateValue);
                string outDir = GetString(features, OutputDirStringValue);
                string tempDir = GetString(features, TempStringValue);
                var datHeader = GetDatHeader(features);
                var filter = GetFilter(features);
                var omitFromScan = GetOmitFromScan(features);
                var skipFileType = GetSkipFileType(features);

                // Create a new DATFromDir object and process the inputs
                DatFile basedat = new DatFile(datHeader)
                {
                    Date = DateTime.Now.ToString("yyyy-MM-dd"),
                };

                // For each input directory, create a DAT
                foreach (string path in Inputs)
                {
                    if (Directory.Exists(path) || File.Exists(path))
                    {
                        // Clone the base Dat for information
                        DatFile datdata = new DatFile(basedat);

                        string basePath = Path.GetFullPath(path);
                        bool success = datdata.PopulateFromDir(
                            basePath,
                            omitFromScan,
                            noAutomaticDate,
                            archivesAsFiles,
                            skipFileType,
                            addBlankFiles,
                            addFileDates,
                            tempDir,
                            copyFiles,
                            datHeader.Header,
                            chdsAsFiles,
                            filter);

                        if (success)
                        {
                            datdata.Write(outDir);
                        }
                        else
                        {
                            Console.WriteLine();
                            _help.OutputIndividualFeature("DATFromDir");
                        }
                    }
                }
            }
        }

        private class ExtractFeature : SabreToolsFeature
        {
            public const string Value = "Extract";

            public ExtractFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-ex", "--extract" };
                this.Description = "Extract and remove copier headers";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"This will detect, store, and remove copier headers from a file or folder of files. The headers are backed up and collated by the hash of the unheadered file. Files are then output without the detected copier header alongside the originals with the suffix .new. No input files are altered in the process.

The following systems have headers that this program can work with:
  - Atari 7800
  - Atari Lynx
  - Commodore PSID Music
  - NEC PC - Engine / TurboGrafx 16
  - Nintendo Famicom / Nintendo Entertainment System
  - Nintendo Famicom Disk System
  - Nintendo Super Famicom / Super Nintendo Entertainment System
  - Nintendo Super Famicom / Super Nintendo Entertainment System SPC";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(_outputDirStringInput);
                AddFeature(_noStoreHeaderFlag);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                bool nostore = GetBoolean(features, NoStoreHeaderValue);
                string outDir = GetString(features, OutputDirStringValue);

                // Get only files from the inputs
                List<string> files = Utilities.GetOnlyFilesFromInputs(Inputs);
                foreach (string file in files)
                {
                    Utilities.DetectSkipperAndTransform(file, outDir, nostore);
                }
            }
        }

        private class HelpFeature : SabreToolsFeature
        {
            public const string Value = "Help";

            public HelpFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-?", "-h", "--help" };
                this.Description = "Show this help";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Built-in to most of the programs is a basic help text.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override bool ProcessArgs(string[] args, Help help)
            {
                // If we had something else after help
                if (args.Length > 1)
                {
                    help.OutputIndividualFeature(args[1]);
                    return true;
                }

                // Otherwise, show generic help
                else
                {
                    help.OutputGenericHelp();
                    return true;
                }
            }
        }

        private class RestoreFeature : SabreToolsFeature
        {
            public const string Value = "Restore";

            public RestoreFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-re", "--restore" };
                this.Description = "Restore header to file based on SHA-1";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"This will make use of stored copier headers and reapply them to files if they match the included hash. More than one header can be applied to a file, so they will be output to new files, suffixed with .newX, where X is a number. No input files are altered in the process.

The following systems have headers that this program can work with:
  - Atari 7800
  - Atari Lynx
  - Commodore PSID Music
  - NEC PC - Engine / TurboGrafx 16
  - Nintendo Famicom / Nintendo Entertainment System
  - Nintendo Famicom Disk System
  - Nintendo Super Famicom / Super Nintendo Entertainment System
  - Nintendo Super Famicom / Super Nintendo Entertainment System SPC";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(_outputDirStringInput);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                string outDir = GetString(features, OutputDirStringValue);

                // Get only files from the inputs
                List<string> files = Utilities.GetOnlyFilesFromInputs(Inputs);
                foreach (string file in files)
                {
                    Utilities.RestoreHeader(file, outDir);
                }
            }
        }

        private class ScriptFeature : SabreToolsFeature
        {
            public const string Value = "Script";

            public ScriptFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "--script" };
                this.Description = "Enable script mode (no clear screen)";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "For times when SabreTools is being used in a scripted environment, the user may not want the screen to be cleared every time that it is called. This flag allows the user to skip clearing the screen on run just like if the console was being redirected.";
                this.Features = new Dictionary<string, Feature>();
            }
        }

        private class SortFeature : SabreToolsFeature
        {
            public const string Value = "Sort";

            public SortFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-ss", "--sort" };
                this.Description = "Sort inputs by a set of DATs";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "This feature allows the user to quickly rebuild based on a supplied DAT file(s). By default all files will be rebuilt to uncompressed folders in the output directory.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(_datListInput);
                AddFeature(_outputDirStringInput);
                AddFeature(_depotFlag);
                AddFeature(_deleteFlag);
                AddFeature(_inverseFlag);
                AddFeature(_quickFlag);
                AddFeature(_chdsAsFilesFlag);
                AddFeature(_addDateFlag);
                AddFeature(_individualFlag);
                AddFeature(_torrent7zipFlag);
                AddFeature(_tarFlag);
                AddFeature(_torrentGzipFlag);
                    this[_torrentGzipFlag].AddFeature(_rombaFlag);
                AddFeature(_torrentLrzipFlag);
                AddFeature(_torrentLz4Flag);
                AddFeature(_torrentRarFlag);
                AddFeature(_torrentXzFlag);
                AddFeature(_torrentZipFlag);
                AddFeature(_torrentZpaqFlag);
                AddFeature(_torrentZstdFlag);
                AddFeature(_headerStringInput);
                AddFeature(_sevenZipInt32Input);
                AddFeature(_gzInt32Input);
                AddFeature(_rarInt32Input);
                AddFeature(_zipInt32Input);
                AddFeature(_scanAllFlag);
                AddFeature(_datMergedFlag);
                AddFeature(_datSplitFlag);
                AddFeature(_datNonMergedFlag);
                AddFeature(_datDeviceNonMergedFlag);
                AddFeature(_datFullNonMergedFlag);
                AddFeature(_updateDatFlag);
                AddFeature(_threadsInt32Input);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get the archive scanning level
                int sevenzip = GetInt32(features, SevenZipInt32Value);
                sevenzip = sevenzip == Int32.MinValue ? 1 : sevenzip;

                int gz = GetInt32(features, GzInt32Value);
                gz = gz == Int32.MinValue ? 1 : gz;

                int rar = GetInt32(features, RarInt32Value);
                rar = rar == Int32.MinValue ? 1 : rar;

                int zip = GetInt32(features, ZipInt32Value);
                zip = zip == Int32.MinValue ? 1 : zip;

                var asl = Utilities.GetArchiveScanLevelFromNumbers(sevenzip, gz, rar, zip);

                // Get feature flags
                bool chdsAsFiles = GetBoolean(features, ChdsAsFilesValue);
                bool date = GetBoolean(features, AddDateValue);
                bool delete = GetBoolean(features, DeleteValue);
                bool depot = GetBoolean(features, DepotValue);
                bool inverse = GetBoolean(features, InverseValue);
                bool quickScan = GetBoolean(features, QuickValue);
                bool romba = GetBoolean(features, RombaValue);
                bool updateDat = GetBoolean(features, UpdateDatValue);
                string headerToCheckAgainst = GetString(features, HeaderStringValue);
                string outDir = GetString(features, OutputDirStringValue);
                var outputFormat = GetOutputFormat(features);
                var splitType = GetSplitType(features);

                // If we have TorrentGzip output and the romba flag, update
                if (romba && outputFormat == OutputFormat.TorrentGzip)
                    outputFormat = OutputFormat.TorrentGzipRomba;

                // Get a list of files from the input datfiles
                var datfiles = GetList(features, DatListValue);
                datfiles = Utilities.GetOnlyFilesFromInputs(datfiles);

                // If we are in individual mode, process each DAT on their own, appending the DAT name to the output dir
                if (GetBoolean(features, IndividualValue))
                {
                    foreach (string datfile in datfiles)
                    {
                        DatFile datdata = new DatFile();
                        datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);

                        // If we have the depot flag, respect it
                        if (depot)
                            datdata.RebuildDepot(Inputs, Path.Combine(outDir, datdata.FileName), date, delete, inverse, outputFormat, updateDat, headerToCheckAgainst);
                        else
                            datdata.RebuildGeneric(Inputs, Path.Combine(outDir, datdata.FileName), quickScan, date, delete, inverse, outputFormat, asl, updateDat, headerToCheckAgainst, chdsAsFiles);
                    }
                }

                // Otherwise, process all DATs into the same output
                else
                {
                    InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

                    // Add all of the input DATs into one huge internal DAT
                    DatFile datdata = new DatFile();
                    foreach (string datfile in datfiles)
                    {
                        datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);
                    }

                    watch.Stop();

                    // If we have the depot flag, respect it
                    if (depot)
                        datdata.RebuildDepot(Inputs, outDir, date, delete, inverse, outputFormat, updateDat, headerToCheckAgainst);
                    else
                        datdata.RebuildGeneric(Inputs, outDir, quickScan, date, delete, inverse, outputFormat, asl, updateDat, headerToCheckAgainst, chdsAsFiles);
                }
            }
        }

        private class SplitFeature : SabreToolsFeature
        {
            public const string Value = "Split";

            public SplitFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-sp", "--split" };
                this.Description = "Split input DATs by a given criteria";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "This feature allows the user to split input DATs by a number of different possible criteria. See the individual input information for details. More than one split type is allowed at a time.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(_outputTypeListInput);
                    this[_outputTypeListInput].AddFeature(_deprecatedFlag);
                AddFeature(_outputDirStringInput);
                AddFeature(_inplaceFlag);
                AddFeature(_extensionFlag);
                    this[_extensionFlag].AddFeature(_extaListInput);
                    this[_extensionFlag].AddFeature(_extbListInput);
                AddFeature(_hashFlag);
                AddFeature(_levelFlag);
                    this[_levelFlag].AddFeature(_shortFlag);
                    this[_levelFlag].AddFeature(_baseFlag);
                AddFeature(_sizeFlag);
                    this[_sizeFlag].AddFeature(_radixInt64Input);
                AddFeature(_typeFlag);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                DatFile datfile = new DatFile
                {
                    DatFormat = GetDatHeader(features).DatFormat,
                };

                datfile.DetermineSplitType(
                    Inputs,
                    GetString(features, OutputDirStringValue),
                    GetBoolean(features, InplaceValue),
                    GetSplittingMode(features),
                    GetList(features, ExtAListValue),
                    GetList(features, ExtBListValue),
                    GetBoolean(features, ShortValue),
                    GetBoolean(features, BaseValue),
                    GetInt64(features, RadixInt64Value));
            }
        }

        private class StatsFeature : SabreToolsFeature
        {
            public const string Value = "Stats";

            public StatsFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-st", "--stats" };
                this.Description = "Get statistics on all input DATs";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"This will output by default the combined statistics for all input DAT files.

The stats that are outputted are as follows:
- Total uncompressed size
- Number of games found
- Number of roms found
- Number of disks found
- Items that include a CRC
- Items that include a MD5
- Items that include a SHA-1
- Items that include a SHA-256
- Items that include a SHA-384
- Items that include a SHA-512
- Items with Nodump status";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(_reportTypeListInput);
                AddFeature(_filenameStringInput);
                AddFeature(_outputDirStringInput);
                AddFeature(_baddumpColumnFlag);
                AddFeature(_nodumpColumnFlag);
                AddFeature(_individualFlag);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                DatFile.OutputStats(
                    Inputs,
                    GetDatHeader(features).FileName,
                    GetString(features, OutputDirStringValue),
                    GetBoolean(features, IndividualValue),
                    GetBoolean(features, BaddumpColumnValue),
                    GetBoolean(features, NodumpColumnValue),
                    GetStatReportFormat(features));
            }
        }

        private class UpdateFeature : SabreToolsFeature
        {
            public const string Value = "Update";

            public UpdateFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-ud", "--update" };
                this.Description = "Update and manipulate DAT(s)";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "This is the multitool part of the program, allowing for almost every manipulation to a DAT, or set of DATs. This is also a combination of many different programs that performed DAT manipulation that work better together.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(_outputTypeListInput);
                    this[_outputTypeListInput].AddFeature(_prefixStringInput);
                    this[_outputTypeListInput].AddFeature(_postfixStringInput);
                    this[_outputTypeListInput].AddFeature(_quotesFlag);
                    this[_outputTypeListInput].AddFeature(_romsFlag);
                    this[_outputTypeListInput].AddFeature(_gamePrefixFlag);
                    this[_outputTypeListInput].AddFeature(_addExtensionStringInput);
                    this[_outputTypeListInput].AddFeature(_replaceExtensionStringInput);
                    this[_outputTypeListInput].AddFeature(_removeExtensionsFlag);
                    this[_outputTypeListInput].AddFeature(_rombaFlag);
                    this[_outputTypeListInput].AddFeature(_deprecatedFlag);
                AddFeature(_filenameStringInput);
                AddFeature(_nameStringInput);
                AddFeature(_descriptionStringInput);
                AddFeature(_rootStringInput);
                AddFeature(_categoryStringInput);
                AddFeature(_versionStringInput);
                AddFeature(_dateStringInput);
                AddFeature(_authorStringInput);
                AddFeature(_emailStringInput);
                AddFeature(_homepageStringInput);
                AddFeature(_urlStringInput);
                AddFeature(_commentStringInput);
                AddFeature(_headerStringInput);
                AddFeature(_superdatFlag);
                AddFeature(_forcemergingStringInput);
                AddFeature(_forcenodumpStringInput);
                AddFeature(_forcepackingStringInput);
                AddFeature(_excludeFieldListInput);
                AddFeature(_oneRomPerGameFlag);
                AddFeature(_keepEmptyGamesFlag);
                AddFeature(_sceneDateStripFlag);
                AddFeature(_cleanFlag);
                AddFeature(_removeUnicodeFlag);
                AddFeature(_descriptionAsNameFlag);
                AddFeature(_datMergedFlag);
                AddFeature(_datSplitFlag);
                AddFeature(_datNonMergedFlag);
                AddFeature(_datDeviceNonMergedFlag);
                AddFeature(_datFullNonMergedFlag);
                AddFeature(_trimFlag);
                    this[_trimFlag].AddFeature(_rootDirStringInput);
                AddFeature(_singleSetFlag);
                AddFeature(_dedupFlag);
                AddFeature(_gameDedupFlag);
                AddFeature(_mergeFlag);
                    this[_mergeFlag].AddFeature(_noAutomaticDateFlag);
                AddFeature(_diffAllFlag);
                    this[_diffAllFlag].AddFeature(_noAutomaticDateFlag);
                AddFeature(_diffDuplicatesFlag);
                    this[_diffDuplicatesFlag].AddFeature(_noAutomaticDateFlag);
                AddFeature(_diffIndividualsFlag);
                    this[_diffIndividualsFlag].AddFeature(_noAutomaticDateFlag);
                AddFeature(_diffNoDuplicatesFlag);
                    this[_diffNoDuplicatesFlag].AddFeature(_noAutomaticDateFlag);
                AddFeature(_diffAgainstFlag);
                    this[_diffAgainstFlag].AddFeature(_baseDatListInput);
                AddFeature(_baseReplaceFlag);
                    this[_baseReplaceFlag].AddFeature(_baseDatListInput);
                    this[_baseReplaceFlag].AddFeature(_updateFieldListInput);
                        this[_baseReplaceFlag][_updateFieldListInput].AddFeature(_onlySameFlag);
                    this[_baseReplaceFlag].AddFeature(_updateNamesFlag);
                    this[_baseReplaceFlag].AddFeature(_updateHashesFlag);
                    this[_baseReplaceFlag].AddFeature(_updateDescriptionFlag);
                        this[_baseReplaceFlag][_updateDescriptionFlag].AddFeature(_onlySameFlag);
                    this[_baseReplaceFlag].AddFeature(_updateGameTypeFlag);
                    this[_baseReplaceFlag].AddFeature(_updateYearFlag);
                    this[_baseReplaceFlag].AddFeature(_updateManufacturerFlag);
                    this[_baseReplaceFlag].AddFeature(_updateParentsFlag);
                AddFeature(_reverseBaseReplaceFlag);
                    this[_reverseBaseReplaceFlag].AddFeature(_baseDatListInput);
                    this[_baseReplaceFlag].AddFeature(_updateFieldListInput);
                        this[_baseReplaceFlag][_updateFieldListInput].AddFeature(_onlySameFlag);
                    this[_reverseBaseReplaceFlag].AddFeature(_updateNamesFlag);
                    this[_reverseBaseReplaceFlag].AddFeature(_updateHashesFlag);
                    this[_reverseBaseReplaceFlag].AddFeature(_updateDescriptionFlag);
                        this[_reverseBaseReplaceFlag][_updateDescriptionFlag].AddFeature(_onlySameFlag);
                    this[_reverseBaseReplaceFlag].AddFeature(_updateGameTypeFlag);
                    this[_reverseBaseReplaceFlag].AddFeature(_updateYearFlag);
                    this[_reverseBaseReplaceFlag].AddFeature(_updateManufacturerFlag);
                    this[_reverseBaseReplaceFlag].AddFeature(_updateParentsFlag);
                AddFeature(_diffCascadeFlag);
                    this[_diffCascadeFlag].AddFeature(_skipFirstOutputFlag);
                AddFeature(_diffReverseCascadeFlag);
                    this[_diffReverseCascadeFlag].AddFeature(_skipFirstOutputFlag);
                AddFeature(_gameNameListInput);
                AddFeature(_notGameNameListInput);
                AddFeature(_gameDescriptionListInput);
                AddFeature(_notGameDescriptionListInput);
                AddFeature(_matchOfTagsFlag);
                AddFeature(_itemNameListInput);
                AddFeature(_notItemNameListInput);
                AddFeature(_itemTypeListInput);
                AddFeature(_notItemTypeListInput);
                AddFeature(_greaterStringInput);
                AddFeature(_lessStringInput);
                AddFeature(_equalStringInput);
                AddFeature(_crcListInput);
                AddFeature(_notCrcListInput);
                AddFeature(_md5ListInput);
                AddFeature(_notMd5ListInput);
                AddFeature(_ripeMd160ListInput);
                AddFeature(_notRipeMd160ListInput);
                AddFeature(_sha1ListInput);
                AddFeature(_notSha1ListInput);
                AddFeature(_sha256ListInput);
                AddFeature(_notSha256ListInput);
                AddFeature(_sha384ListInput);
                AddFeature(_notSha384ListInput);
                AddFeature(_sha512ListInput);
                AddFeature(_notSha512ListInput);
                AddFeature(_statusListInput);
                AddFeature(_notStatusListInput);
                AddFeature(_gameTypeListInput);
                AddFeature(_notGameTypeListInput);
                AddFeature(_runnableFlag);
                AddFeature(_notRunnableFlag);
                AddFeature(_outputDirStringInput);
                AddFeature(_inplaceFlag);
                AddFeature(_threadsInt32Input);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                var datHeader = GetDatHeader(features);
                var updateFields = GetUpdateFields(features);
                var updateMode = GetUpdateMode(features);

                // Normalize the extensions
                datHeader.AddExtension = (string.IsNullOrWhiteSpace(datHeader.AddExtension) || datHeader.AddExtension.StartsWith(".")
                    ? datHeader.AddExtension
                    : $".{datHeader.AddExtension}");
                datHeader.ReplaceExtension = (string.IsNullOrWhiteSpace(datHeader.ReplaceExtension) || datHeader.ReplaceExtension.StartsWith(".")
                    ? datHeader.ReplaceExtension
                    : $".{datHeader.ReplaceExtension}");

                // If we're in a special update mode and the names aren't set, set defaults
                if (updateMode != 0)
                {
                    // Get the values that will be used
                    if (string.IsNullOrWhiteSpace(datHeader.Date))
                        datHeader.Date = DateTime.Now.ToString("yyyy-MM-dd");

                    if (string.IsNullOrWhiteSpace(datHeader.Name))
                    {
                        datHeader.Name = (updateMode != 0 ? "DiffDAT" : "MergeDAT")
                            + (datHeader.Type == "SuperDAT" ? "-SuperDAT" : string.Empty)
                            + (datHeader.DedupeRoms != DedupeType.None ? "-deduped" : string.Empty);
                    }

                    if (string.IsNullOrWhiteSpace(datHeader.Description))
                    {
                        datHeader.Description = (updateMode != 0 ? "DiffDAT" : "MergeDAT")
                            + (datHeader.Type == "SuperDAT" ? "-SuperDAT" : string.Empty)
                            + (datHeader.DedupeRoms != DedupeType.None ? " - deduped" : string.Empty);

                        if (!GetBoolean(features, NoAutomaticDateValue))
                            datHeader.Description += $" ({datHeader.Date})";
                    }

                    if (string.IsNullOrWhiteSpace(datHeader.Category) && updateMode != 0)
                        datHeader.Category = "DiffDAT";

                    if (string.IsNullOrWhiteSpace(datHeader.Author))
                        datHeader.Author = "SabreTools";
                }

                // If no update fields are set, default to Names
                if (updateFields == null || updateFields.Count == 0)
                    updateFields = new List<Field>() { Field.Name };

                // Populate the DatData object
                DatFile userInputDat = new DatFile(datHeader);

                userInputDat.DetermineUpdateType(
                    Inputs,
                    GetList(features, BaseDatListValue),
                    GetString(features, OutputDirStringValue),
                    updateMode,
                    GetBoolean(features, InplaceValue),
                    GetBoolean(features, SkipFirstOutputValue),
                    GetBoolean(features, CleanValue),
                    GetBoolean(features, RemoveUnicodeValue),
                    GetBoolean(features, DescriptionAsNameValue),
                    GetFilter(features),
                    GetSplitType(features),
                    updateFields,
                    GetBoolean(features, OnlySameValue));
            }
        }

        private class VerifyFeature : SabreToolsFeature
        {
            public const string Value = "Verify";

            public VerifyFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-ve", "--verify" };
                this.Description = "Verify a folder against DATs";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "When used, this will use an input DAT or set of DATs to blindly check against an input folder. The base of the folder is considered the base for the combined DATs and games are either the directories or archives within. This will only do a direct verification of the items within and will create a fixdat afterwards for missing files.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(_datListInput);
                AddFeature(_depotFlag);
                AddFeature(_tempStringInput);
                AddFeature(_hashOnlyFlag);
                AddFeature(_quickFlag);
                AddFeature(_headerStringInput);
                AddFeature(_chdsAsFilesFlag);
                AddFeature(_individualFlag);
                AddFeature(_datMergedFlag);
                AddFeature(_datSplitFlag);
                AddFeature(_datDeviceNonMergedFlag);
                AddFeature(_datNonMergedFlag);
                AddFeature(_datFullNonMergedFlag);
                AddFeature(_gameNameListInput);
                AddFeature(_notGameNameListInput);
                AddFeature(_gameDescriptionListInput);
                AddFeature(_notGameDescriptionListInput);
                AddFeature(_matchOfTagsFlag);
                AddFeature(_itemNameListInput);
                AddFeature(_notItemNameListInput);
                AddFeature(_itemTypeListInput);
                AddFeature(_notItemTypeListInput);
                AddFeature(_greaterStringInput);
                AddFeature(_lessStringInput);
                AddFeature(_equalStringInput);
                AddFeature(_crcListInput);
                AddFeature(_notCrcListInput);
                AddFeature(_md5ListInput);
                AddFeature(_notMd5ListInput);
                AddFeature(_ripeMd160ListInput);
                AddFeature(_notRipeMd160ListInput);
                AddFeature(_sha1ListInput);
                AddFeature(_notSha1ListInput);
                AddFeature(_sha256ListInput);
                AddFeature(_notSha256ListInput);
                AddFeature(_sha384ListInput);
                AddFeature(_notSha384ListInput);
                AddFeature(_sha512ListInput);
                AddFeature(_notSha512ListInput);
                AddFeature(_statusListInput);
                AddFeature(_notStatusListInput);
                AddFeature(_gameTypeListInput);
                AddFeature(_notGameTypeListInput);
                AddFeature(_runnableFlag);
                AddFeature(_notRunnableFlag);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get a list of files from the input datfiles
                var datfiles = GetList(features, DatListValue);
                datfiles = Utilities.GetOnlyFilesFromInputs(datfiles);

                // Get feature flags
                bool chdsAsFiles = GetBoolean(features, ChdsAsFilesValue);
                bool depot = GetBoolean(features, DepotValue);
                bool hashOnly = GetBoolean(features, HashOnlyValue);
                bool quickScan = GetBoolean(features, QuickValue);
                string headerToCheckAgainst = GetDatHeader(features).Header;
                var filter = GetFilter(features);
                var splitType = GetSplitType(features);

                // If we are in individual mode, process each DAT on their own
                if (GetBoolean(features, IndividualValue))
                {
                    foreach (string datfile in datfiles)
                    {
                        DatFile datdata = new DatFile();
                        datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);

                        // If we have the depot flag, respect it
                        if (depot)
                            datdata.VerifyDepot(Inputs, headerToCheckAgainst);
                        else
                            datdata.VerifyGeneric(Inputs, hashOnly, quickScan, headerToCheckAgainst, chdsAsFiles, filter);
                    }
                }
                // Otherwise, process all DATs into the same output
                else
                {
                    InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

                    // Add all of the input DATs into one huge internal DAT
                    DatFile datdata = new DatFile();
                    foreach (string datfile in datfiles)
                    {
                        datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);
                    }

                    watch.Stop();

                    // If we have the depot flag, respect it
                    if (depot)
                        datdata.VerifyDepot(Inputs, headerToCheckAgainst);
                    else
                        datdata.VerifyGeneric(Inputs, hashOnly, quickScan, headerToCheckAgainst, chdsAsFiles, filter);
                }
            }
        }

        #endregion
    }
}
