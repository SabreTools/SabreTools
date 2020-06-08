using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using SabreTools.Library.Data;
using SabreTools.Library.Help;

namespace SabreTools
{
    public partial class SabreTools
    {
        #region Public Top-level Features

        public const string HelpFeatureValue = "Help";
        private static Feature _helpFeatureFlag
        {
            get
            {
                return new Feature(
                    HelpFeatureValue,
                    new List<string>() { "-?", "-h", "--help" },
                    "Show this help",
                    FeatureType.Flag,
                    longDescription: "Built-in to most of the programs is a basic help text.");
            }
        }

        public const string HelpDetailedFeatureValue = "Help (Detailed)";
        private static Feature _helpDetailedFeatureFlag
        {
            get
            {
                return new Feature(
                    HelpDetailedFeatureValue,
                    new List<string>() { "-??", "-hd", "--help-detailed" },
                    "Show this detailed help",
                    FeatureType.Flag,
                    longDescription: "Display a detailed help text to the screen.");
            }
        }

        public const string ScriptFeatureValue = "Script";
        private static Feature _scriptFeatureFlag
        {
            get
            {
                return new Feature(
                    ScriptFeatureValue,
                    "--script",
                    "Enable script mode (no clear screen)",
                    FeatureType.Flag,
                    longDescription: "For times when SabreTools is being used in a scripted environment, the user may not want the screen to be cleared every time that it is called. This flag allows the user to skip clearing the screen on run just like if the console was being redirected.");
            }
        }

        public const string DatFromDirFeatureValue = "DATFromDir";
        private static Feature _datFromDirFeatureFlag
        {
            get
            {
                return new Feature(
                    DatFromDirFeatureValue,
                    new List<string>() { "-d", "--d2d", "--dfd" },
                    "Create DAT(s) from an input directory",
                    FeatureType.Flag,
                    longDescription: "Create a DAT file from an input directory or set of files. By default, this will output a DAT named based on the input directory and the current date. It will also treat all archives as possible games and add all three hashes (CRC, MD5, SHA-1) for each file.");
            }
        }

        public const string ExtractFeatureValue = "Extract";
        private static Feature _extractFeatureFlag
        {
            get
            {
                return new Feature(
                    ExtractFeatureValue,
                    new List<string>() { "-ex", "--extract" },
                    "Extract and remove copier headers",
                    FeatureType.Flag,
                    longDescription: @"This will detect, store, and remove copier headers from a file or folder of files. The headers are backed up and collated by the hash of the unheadered file. Files are then output without the detected copier header alongside the originals with the suffix .new. No input files are altered in the process.

The following systems have headers that this program can work with:
  - Atari 7800
  - Atari Lynx
  - Commodore PSID Music
  - NEC PC - Engine / TurboGrafx 16
  - Nintendo Famicom / Nintendo Entertainment System
  - Nintendo Famicom Disk System
  - Nintendo Super Famicom / Super Nintendo Entertainment System
  - Nintendo Super Famicom / Super Nintendo Entertainment System SPC");
            }
        }

        public const string RestoreFeatureValue = "Restore";
        private static Feature _restoreFeatureFlag
        {
            get
            {
                return new Feature(
                    RestoreFeatureValue,
                    new List<string>() { "-re", "--restore" },
                    "Restore header to file based on SHA-1",
                    FeatureType.Flag,
                    longDescription: @"This will make use of stored copier headers and reapply them to files if they match the included hash. More than one header can be applied to a file, so they will be output to new files, suffixed with .newX, where X is a number. No input files are altered in the process.

The following systems have headers that this program can work with:
  - Atari 7800
  - Atari Lynx
  - Commodore PSID Music
  - NEC PC - Engine / TurboGrafx 16
  - Nintendo Famicom / Nintendo Entertainment System
  - Nintendo Famicom Disk System
  - Nintendo Super Famicom / Super Nintendo Entertainment System
  - Nintendo Super Famicom / Super Nintendo Entertainment System SPC");
            }
        }

        public const string SortFeatureValue = "Sort";
        private static Feature _sortFeatureFlag
        {
            get
            {
                return new Feature(
                    SortFeatureValue,
                    new List<string>() { "-ss", "--sort" },
                    "Sort inputs by a set of DATs",
                    FeatureType.Flag,
                    longDescription: "This feature allows the user to quickly rebuild based on a supplied DAT file(s). By default all files will be rebuilt to uncompressed folders in the output directory.");
            }
        }

        public const string SplitFeatureValue = "Split";
        private static Feature _splitFeatureFlag
        {
            get
            {
                return new Feature(
                    SplitFeatureValue,
                    new List<string>() { "-sp", "--split" },
                    "Split input DATs by a given criteria",
                    FeatureType.Flag,
                    longDescription: "This feature allows the user to split input DATs by a number of different possible criteria. See the individual input information for details. More than one split type is allowed at a time.");
            }
        }

        public const string StatsFeatureValue = "Stats";
        private static Feature _statsFeatureFlag
        {
            get
            {
                return new Feature(
                    StatsFeatureValue,
                    new List<string>() { "-st", "--stats" },
                    "Get statistics on all input DATs",
                    FeatureType.Flag,
                    longDescription: @"This will output by default the combined statistics for all input DAT files.

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
- Items with Nodump status");
            }
        }

        public const string UpdateFeatureValue = "Update";
        private static Feature _updateFeatureFlag
        {
            get
            {
                return new Feature(
                    UpdateFeatureValue,
                    new List<string>() { "-ud", "--update" },
                    "Update and manipulate DAT(s)",
                    FeatureType.Flag,
                    longDescription: "This is the multitool part of the program, allowing for almost every manipulation to a DAT, or set of DATs. This is also a combination of many different programs that performed DAT manipulation that work better together.");
            }
        }

        public const string VerifyFeatureValue = "Verify";
        private static Feature _verifyFeatureFlag
        {
            get
            {
                return new Feature(
                    VerifyFeatureValue,
                    new List<string>() { "-ve", "--verify" },
                    "Verify a folder against DATs",
                    FeatureType.Flag,
                    longDescription: "When used, this will use an input DAT or set of DATs to blindly check against an input folder. The base of the folder is considered the base for the combined DATs and games are either the directories or archives within. This will only do a direct verification of the items within and will create a fixdat afterwards for missing files.");
            }
        }

        #endregion

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

            // Build the feature trees
            Help help = new Help(helpHeader);

            #region Help

            Feature helpFeature = _helpFeatureFlag;

            #endregion

            #region Help (Detailed)

            Feature detailedHelpFeature = _helpDetailedFeatureFlag;

            #endregion

            #region Script

            Feature script = _scriptFeatureFlag;

            #endregion

            #region DATFromDir

            Feature datFromDir = _datFromDirFeatureFlag;
            datFromDir.AddFeature(_skipMd5Flag);
            datFromDir.AddFeature(_skipRipeMd160Flag);
            datFromDir.AddFeature(_skipSha1Flag);
            datFromDir.AddFeature(_skipSha256Flag);
            datFromDir.AddFeature(_skipSha384Flag);
            datFromDir.AddFeature(_skipSha512Flag);
            datFromDir.AddFeature(_noAutomaticDateFlag);
            datFromDir.AddFeature(_forcepackingStringInput);
            datFromDir.AddFeature(_archivesAsFilesFlag);
            datFromDir.AddFeature(_outputTypeListInput);
                datFromDir[_outputTypeListInput].AddFeature(_deprecatedFlag);
            datFromDir.AddFeature(_rombaFlag);
            datFromDir.AddFeature(_skipArchivesFlag);
            datFromDir.AddFeature(_skipFilesFlag);
            datFromDir.AddFeature(_filenameStringInput);
            datFromDir.AddFeature(_nameStringInput);
            datFromDir.AddFeature(_descriptionStringInput);
            datFromDir.AddFeature(_categoryStringInput);
            datFromDir.AddFeature(_versionStringInput);
            datFromDir.AddFeature(_authorStringInput);
            datFromDir.AddFeature(_emailStringInput);
            datFromDir.AddFeature(_homepageStringInput);
            datFromDir.AddFeature(_urlStringInput);
            datFromDir.AddFeature(_commentStringInput);
            datFromDir.AddFeature(_superdatFlag);
            datFromDir.AddFeature(_excludeFieldListInput);
            datFromDir.AddFeature(_oneRomPerGameFlag);
            datFromDir.AddFeature(_sceneDateStripFlag);
            datFromDir.AddFeature(_addBlankFilesFlag);
            datFromDir.AddFeature(_addDateFlag);
            datFromDir.AddFeature(_copyFilesFlag);
            datFromDir.AddFeature(_headerStringInput);
            datFromDir.AddFeature(_chdsAsFilesFlag);
            datFromDir.AddFeature(_gameNameListInput);
            datFromDir.AddFeature(_notGameNameListInput);
            datFromDir.AddFeature(_gameDescriptionListInput);
            datFromDir.AddFeature(_notGameDescriptionListInput);
            datFromDir.AddFeature(_matchOfTagsFlag);
            datFromDir.AddFeature(_itemNameListInput);
            datFromDir.AddFeature(_notItemNameListInput);
            datFromDir.AddFeature(_itemTypeListInput);
            datFromDir.AddFeature(_notItemTypeListInput);
            datFromDir.AddFeature(_greaterStringInput);
            datFromDir.AddFeature(_lessStringInput);
            datFromDir.AddFeature(_equalStringInput);
            datFromDir.AddFeature(_crcListInput);
            datFromDir.AddFeature(_notCrcListInput);
            datFromDir.AddFeature(_md5ListInput);
            datFromDir.AddFeature(_notMd5ListInput);
            datFromDir.AddFeature(_ripeMd160ListInput);
            datFromDir.AddFeature(_notRipeMd160ListInput);
            datFromDir.AddFeature(_sha1ListInput);
            datFromDir.AddFeature(_notSha1ListInput);
            datFromDir.AddFeature(_sha256ListInput);
            datFromDir.AddFeature(_notSha256ListInput);
            datFromDir.AddFeature(_sha384ListInput);
            datFromDir.AddFeature(_notSha384ListInput);
            datFromDir.AddFeature(_sha512ListInput);
            datFromDir.AddFeature(_notSha512ListInput);
            datFromDir.AddFeature(_statusListInput);
            datFromDir.AddFeature(_notStatusListInput);
            datFromDir.AddFeature(_gameTypeListInput);
            datFromDir.AddFeature(_notGameTypeListInput);
            datFromDir.AddFeature(_runnableFlag);
            datFromDir.AddFeature(_notRunnableFlag);
            datFromDir.AddFeature(_tempStringInput);
            datFromDir.AddFeature(_outputDirStringInput);
            datFromDir.AddFeature(_threadsInt32Input);

            #endregion

            #region Extract

            Feature extract = _extractFeatureFlag;
            extract.AddFeature(_outputDirStringInput);
            extract.AddFeature(_noStoreHeaderFlag);

            #endregion

            #region Restore

            Feature restore = _restoreFeatureFlag;
            restore.AddFeature(_outputDirStringInput);

            #endregion

            #region Sort

            Feature sort = _sortFeatureFlag;
            sort.AddFeature(_datListInput);
            sort.AddFeature(_outputDirStringInput);
            sort.AddFeature(_depotFlag);
            sort.AddFeature(_deleteFlag);
            sort.AddFeature(_inverseFlag);
            sort.AddFeature(_quickFlag);
            sort.AddFeature(_chdsAsFilesFlag);
            sort.AddFeature(_addDateFlag);
            sort.AddFeature(_individualFlag);
            sort.AddFeature(_torrent7zipFlag);
            sort.AddFeature(_tarFlag);
            sort.AddFeature(_torrentGzipFlag);
                sort[_torrentGzipFlag].AddFeature(_rombaFlag);
            sort.AddFeature(_torrentLrzipFlag);
            sort.AddFeature(_torrentLz4Flag);
            sort.AddFeature(_torrentRarFlag);
            sort.AddFeature(_torrentXzFlag);
            sort.AddFeature(_torrentZipFlag);
            sort.AddFeature(_torrentZpaqFlag);
            sort.AddFeature(_torrentZstdFlag);
            sort.AddFeature(_headerStringInput);
            sort.AddFeature(_sevenZipInt32Input);
            sort.AddFeature(_gzInt32Input);
            sort.AddFeature(_rarInt32Input);
            sort.AddFeature(_zipInt32Input);
            sort.AddFeature(_scanAllFlag);
            sort.AddFeature(_datMergedFlag);
            sort.AddFeature(_datSplitFlag);
            sort.AddFeature(_datNonMergedFlag);
            sort.AddFeature(_datDeviceNonMergedFlag);
            sort.AddFeature(_datFullNonMergedFlag);
            sort.AddFeature(_updateDatFlag);
            sort.AddFeature(_threadsInt32Input);

            #endregion

            #region Split

            Feature split = _splitFeatureFlag;
            split.AddFeature(_outputTypeListInput);
                split[_outputTypeListInput].AddFeature(_deprecatedFlag);
            split.AddFeature(_outputDirStringInput);
            split.AddFeature(_inplaceFlag);
            split.AddFeature(_extensionFlag);
                split[_extensionFlag].AddFeature(_extaListInput);
                split[_extensionFlag].AddFeature(_extbListInput);
            split.AddFeature(_hashFlag);
            split.AddFeature(_levelFlag);
                split[_levelFlag].AddFeature(_shortFlag);
                split[_levelFlag].AddFeature(_baseFlag);
            split.AddFeature(_sizeFlag);
                split[_sizeFlag].AddFeature(_radixInt64Input);
            split.AddFeature(_typeFlag);

            #endregion

            #region Stats

            Feature stats = _statsFeatureFlag;
            stats.AddFeature(_reportTypeListInput);
            stats.AddFeature(_filenameStringInput);
            stats.AddFeature(_outputDirStringInput);
            stats.AddFeature(_baddumpColumnFlag);
            stats.AddFeature(_nodumpColumnFlag);
            stats.AddFeature(_individualFlag);

            #endregion

            #region Update

            Feature update = _updateFeatureFlag;
            update.AddFeature(_outputTypeListInput);
                update[_outputTypeListInput].AddFeature(_prefixStringInput);
                update[_outputTypeListInput].AddFeature(_postfixStringInput);
                update[_outputTypeListInput].AddFeature(_quotesFlag);
                update[_outputTypeListInput].AddFeature(_romsFlag);
                update[_outputTypeListInput].AddFeature(_gamePrefixFlag);
                update[_outputTypeListInput].AddFeature(_addExtensionStringInput);
                update[_outputTypeListInput].AddFeature(_replaceExtensionStringInput);
                update[_outputTypeListInput].AddFeature(_removeExtensionsFlag);
                update[_outputTypeListInput].AddFeature(_rombaFlag);
                update[_outputTypeListInput].AddFeature(_deprecatedFlag);
            update.AddFeature(_filenameStringInput);
            update.AddFeature(_nameStringInput);
            update.AddFeature(_descriptionStringInput);
            update.AddFeature(_rootStringInput);
            update.AddFeature(_categoryStringInput);
            update.AddFeature(_versionStringInput);
            update.AddFeature(_dateStringInput);
            update.AddFeature(_authorStringInput);
            update.AddFeature(_emailStringInput);
            update.AddFeature(_homepageStringInput);
            update.AddFeature(_urlStringInput);
            update.AddFeature(_commentStringInput);
            update.AddFeature(_headerStringInput);
            update.AddFeature(_superdatFlag);
            update.AddFeature(_forcemergingStringInput);
            update.AddFeature(_forcenodumpStringInput);
            update.AddFeature(_forcepackingStringInput);
            update.AddFeature(_excludeFieldListInput);
            update.AddFeature(_oneRomPerGameFlag);
            update.AddFeature(_keepEmptyGamesFlag);
            update.AddFeature(_sceneDateStripFlag);
            update.AddFeature(_cleanFlag);
            update.AddFeature(_removeUnicodeFlag);
            update.AddFeature(_descriptionAsNameFlag);
            update.AddFeature(_datMergedFlag);
            update.AddFeature(_datSplitFlag);
            update.AddFeature(_datNonMergedFlag);
            update.AddFeature(_datDeviceNonMergedFlag);
            update.AddFeature(_datFullNonMergedFlag);
            update.AddFeature(_trimFlag);
                update[_trimFlag].AddFeature(_rootDirStringInput);
            update.AddFeature(_singleSetFlag);
            update.AddFeature(_dedupFlag);
            update.AddFeature(_gameDedupFlag);
            update.AddFeature(_mergeFlag);
                update[_mergeFlag].AddFeature(_noAutomaticDateFlag);
            update.AddFeature(_diffAllFlag);
                update[_diffAllFlag].AddFeature(_noAutomaticDateFlag);
            update.AddFeature(_diffDuplicatesFlag);
                update[_diffDuplicatesFlag].AddFeature(_noAutomaticDateFlag);
            update.AddFeature(_diffIndividualsFlag);
                update[_diffIndividualsFlag].AddFeature(_noAutomaticDateFlag);
            update.AddFeature(_diffNoDuplicatesFlag);
                update[_diffNoDuplicatesFlag].AddFeature(_noAutomaticDateFlag);
            update.AddFeature(_diffAgainstFlag);
                update[_diffAgainstFlag].AddFeature(_baseDatListInput);
            update.AddFeature(_baseReplaceFlag);
                update[_baseReplaceFlag].AddFeature(_baseDatListInput);
                update[_baseReplaceFlag].AddFeature(_updateFieldListInput);
                    update[_baseReplaceFlag][_updateFieldListInput].AddFeature(_onlySameFlag);
                update[_baseReplaceFlag].AddFeature(_updateNamesFlag);
                update[_baseReplaceFlag].AddFeature(_updateHashesFlag);
                update[_baseReplaceFlag].AddFeature(_updateDescriptionFlag);
                    update[_baseReplaceFlag][_updateDescriptionFlag].AddFeature(_onlySameFlag);
                update[_baseReplaceFlag].AddFeature(_updateGameTypeFlag);
                update[_baseReplaceFlag].AddFeature(_updateYearFlag);
                update[_baseReplaceFlag].AddFeature(_updateManufacturerFlag);
                update[_baseReplaceFlag].AddFeature(_updateParentsFlag);
            update.AddFeature(_reverseBaseReplaceFlag);
                update[_reverseBaseReplaceFlag].AddFeature(_baseDatListInput);
                update[_baseReplaceFlag].AddFeature(_updateFieldListInput);
                    update[_baseReplaceFlag][_updateFieldListInput].AddFeature(_onlySameFlag);
                update[_reverseBaseReplaceFlag].AddFeature(_updateNamesFlag);
                update[_reverseBaseReplaceFlag].AddFeature(_updateHashesFlag);
                update[_reverseBaseReplaceFlag].AddFeature(_updateDescriptionFlag);
                    update[_reverseBaseReplaceFlag][_updateDescriptionFlag].AddFeature(_onlySameFlag);
                update[_reverseBaseReplaceFlag].AddFeature(_updateGameTypeFlag);
                update[_reverseBaseReplaceFlag].AddFeature(_updateYearFlag);
                update[_reverseBaseReplaceFlag].AddFeature(_updateManufacturerFlag);
                update[_reverseBaseReplaceFlag].AddFeature(_updateParentsFlag);
            update.AddFeature(_diffCascadeFlag);
                update[_diffCascadeFlag].AddFeature(_skipFirstOutputFlag);
            update.AddFeature(_diffReverseCascadeFlag);
                update[_diffReverseCascadeFlag].AddFeature(_skipFirstOutputFlag);
            update.AddFeature(_gameNameListInput);
            update.AddFeature(_notGameNameListInput);
            update.AddFeature(_gameDescriptionListInput);
            update.AddFeature(_notGameDescriptionListInput);
            update.AddFeature(_matchOfTagsFlag);
            update.AddFeature(_itemNameListInput);
            update.AddFeature(_notItemNameListInput);
            update.AddFeature(_itemTypeListInput);
            update.AddFeature(_notItemTypeListInput);
            update.AddFeature(_greaterStringInput);
            update.AddFeature(_lessStringInput);
            update.AddFeature(_equalStringInput);
            update.AddFeature(_crcListInput);
            update.AddFeature(_notCrcListInput);
            update.AddFeature(_md5ListInput);
            update.AddFeature(_notMd5ListInput);
            update.AddFeature(_ripeMd160ListInput);
            update.AddFeature(_notRipeMd160ListInput);
            update.AddFeature(_sha1ListInput);
            update.AddFeature(_notSha1ListInput);
            update.AddFeature(_sha256ListInput);
            update.AddFeature(_notSha256ListInput);
            update.AddFeature(_sha384ListInput);
            update.AddFeature(_notSha384ListInput);
            update.AddFeature(_sha512ListInput);
            update.AddFeature(_notSha512ListInput);
            update.AddFeature(_statusListInput);
            update.AddFeature(_notStatusListInput);
            update.AddFeature(_gameTypeListInput);
            update.AddFeature(_notGameTypeListInput);
            update.AddFeature(_runnableFlag);
            update.AddFeature(_notRunnableFlag);
            update.AddFeature(_outputDirStringInput);
            update.AddFeature(_inplaceFlag);
            update.AddFeature(_threadsInt32Input);

            #endregion

            #region Verify

            Feature verify = _verifyFeatureFlag;
            verify.AddFeature(_datListInput);
            verify.AddFeature(_depotFlag);
            verify.AddFeature(_tempStringInput);
            verify.AddFeature(_hashOnlyFlag);
            verify.AddFeature(_quickFlag);
            verify.AddFeature(_headerStringInput);
            verify.AddFeature(_chdsAsFilesFlag);
            verify.AddFeature(_individualFlag);
            verify.AddFeature(_datMergedFlag);
            verify.AddFeature(_datSplitFlag);
            verify.AddFeature(_datDeviceNonMergedFlag);
            verify.AddFeature(_datNonMergedFlag);
            verify.AddFeature(_datFullNonMergedFlag);
            verify.AddFeature(_gameNameListInput);
            verify.AddFeature(_notGameNameListInput);
            verify.AddFeature(_gameDescriptionListInput);
            verify.AddFeature(_notGameDescriptionListInput);
            verify.AddFeature(_matchOfTagsFlag);
            verify.AddFeature(_itemNameListInput);
            verify.AddFeature(_notItemNameListInput);
            verify.AddFeature(_itemTypeListInput);
            verify.AddFeature(_notItemTypeListInput);
            verify.AddFeature(_greaterStringInput);
            verify.AddFeature(_lessStringInput);
            verify.AddFeature(_equalStringInput);
            verify.AddFeature(_crcListInput);
            verify.AddFeature(_notCrcListInput);
            verify.AddFeature(_md5ListInput);
            verify.AddFeature(_notMd5ListInput);
            verify.AddFeature(_ripeMd160ListInput);
            verify.AddFeature(_notRipeMd160ListInput);
            verify.AddFeature(_sha1ListInput);
            verify.AddFeature(_notSha1ListInput);
            verify.AddFeature(_sha256ListInput);
            verify.AddFeature(_notSha256ListInput);
            verify.AddFeature(_sha384ListInput);
            verify.AddFeature(_notSha384ListInput);
            verify.AddFeature(_sha512ListInput);
            verify.AddFeature(_notSha512ListInput);
            verify.AddFeature(_statusListInput);
            verify.AddFeature(_notStatusListInput);
            verify.AddFeature(_gameTypeListInput);
            verify.AddFeature(_notGameTypeListInput);
            verify.AddFeature(_runnableFlag);
            verify.AddFeature(_notRunnableFlag);

            #endregion

            // Now, add all of the main features to the Help object
            help.Add(helpFeature);
            help.Add(detailedHelpFeature);
            help.Add(script);
            help.Add(datFromDir);
            help.Add(extract);
            help.Add(restore);
            help.Add(sort);
            help.Add(split);
            help.Add(stats);
            help.Add(update);
            help.Add(verify);

            return help;
        }
    }
}
