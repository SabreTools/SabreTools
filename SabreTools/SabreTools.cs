using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

namespace SabreTools
{
    /// <summary>
    /// Entry class for the DATabase application
    /// </summary>
    /// TODO: Look into async read/write to make things quicker. Ask edc for help?
    public partial class SabreTools
    {
        // Private required variables
        private static Help _help;

        /// <summary>
        /// Entry class for the SabreTools application
        /// </summary>
        /// <param name="args">String array representing command line parameters</param>
        public static void Main(string[] args)
        {
            // Perform initial setup and verification
            Globals.Logger = new Logger(true, "sabretools.log");

            // Create a new Help object for this program
            _help = SabreTools.RetrieveHelp();

            // Get the location of the script tag, if it exists
            int scriptLocation = (new List<string>(args)).IndexOf("--script");

            // If output is being redirected or we are in script mode, don't allow clear screens
            if (!Console.IsOutputRedirected && scriptLocation == -1)
            {
                Console.Clear();
                Build.PrepareConsole("SabreTools");
            }

            // Now we remove the script tag because it messes things up
            if (scriptLocation > -1)
            {
                List<string> newargs = new List<string>(args);
                newargs.RemoveAt(scriptLocation);
                args = newargs.ToArray();
            }

            // Credits take precidence over all
            if ((new List<string>(args)).Contains("--credits"))
            {
                _help.OutputCredits();
                Globals.Logger.Close();
                return;
            }

            // If there's no arguments, show help
            if (args.Length == 0)
            {
                _help.OutputGenericHelp();
                Globals.Logger.Close();
                return;
            }

            // Get the first argument as a feature flag
            string feature = args[0];

            // Verify that the flag is valid
            if (!_help.TopLevelFlag(feature))
            {
                Globals.Logger.User("'{0}' is not valid feature flag", feature);
                _help.OutputIndividualFeature(feature);
                Globals.Logger.Close();
                return;
            }

            // Now get the proper name for the feature
            feature = _help.GetFeatureName(feature);

            // If we had the help feature first
            if (feature == HelpFeatureValue)
            {
                // If we had something else after help
                if (args.Length > 1)
                {
                    _help.OutputIndividualFeature(args[1]);
                    Globals.Logger.Close();
                    return;
                }
                // Otherwise, show generic help
                else
                {
                    _help.OutputGenericHelp();
                    Globals.Logger.Close();
                    return;
                }
            }
            else if (feature == HelpDetailedFeatureValue)
            {
                // If we had something else after help
                if (args.Length > 1)
                {
                    _help.OutputIndividualFeature(args[1], includeLongDescription: true);
                    Globals.Logger.Close();
                    return;
                }
                // Otherwise, show generic help
                else
                {
                    _help.OutputAllHelp();
                    Globals.Logger.Close();
                    return;
                }
            }

            // Now verify that all other flags are valid
            List<string> inputs = new List<string>();
            for (int i = 1; i < args.Length; i++)
            {
                // Verify that the current flag is proper for the feature
                if (!_help[feature].ValidateInput(args[i]))
                {
                    Globals.Logger.Error("Invalid input detected: {0}", args[i]);
                    _help.OutputIndividualFeature(feature);
                    Globals.Logger.Close();
                    return;
                }

                // Special precautions for files and directories
                if (File.Exists(args[i]) || Directory.Exists(args[i]))
                    inputs.Add(args[i]);
            }

            // Now process all of the features
            Dictionary<string, Feature> features = _help.GetEnabledFeatures();

            #region Default values

            // User flags
            Hash omitFromScan = Hash.DeepHashes; // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
            OutputFormat outputFormat = OutputFormat.Folder;
            SkipFileType skipFileType = SkipFileType.None;
            SplittingMode splittingMode = SplittingMode.None;
            SplitType splitType = SplitType.None;
            StatReportFormat statDatFormat = StatReportFormat.None;
            UpdateMode updateMode = UpdateMode.None;

            // User inputs
            int gz = 1,
                rar = 1,
                sevenzip = 1,
                zip = 1;
            long radix = 0;
            string outDir = null,
                tempDir = string.Empty;
            DatHeader datHeader = new DatHeader();
            Filter filter = new Filter();
            List<string> basePaths = new List<string>();
            List<string> datfiles = new List<string>();
            List<string> exta = new List<string>();
            List<string> extb = new List<string>();
            List<Field> updateFields = new List<Field>();

            #endregion

            #region User Flags

            // Add blank files
            bool addBlankFiles = features.ContainsKey(AddBlankFilesValue);

            // Add file dates
            bool addFileDates = features.ContainsKey(AddDateValue);

            // Archives as files
            bool archivesAsFiles = features.ContainsKey(ArchivesAsFilesValue);

            // Show baddump column
            bool showBaddumpColumn = features.ContainsKey(BaddumpColumnValue);

            // Use base dat
            bool basedat = features.ContainsKey(BaseValue);

            // Base replacement mode
            if (features.ContainsKey(BaseReplaceValue))
                updateMode |= UpdateMode.BaseReplace;

            // CHDs as files
            bool chdsAsFiles = features.ContainsKey(ChdsAsFilesValue);

            // Clean game names
            bool cleanGameNames = features.ContainsKey(CleanValue);

            // Copy files before processing
            bool copyFiles = features.ContainsKey(CopyFilesValue);

            // Device non-merged splitting
            if (features.ContainsKey(DatDeviceNonMergedValue))
                splitType = SplitType.DeviceNonMerged;

            // Full non-merged splitting
            if (features.ContainsKey(DatFullNonMergedValue))
                splitType = SplitType.FullNonMerged;

            // Merged splitting
            if (features.ContainsKey(DatMergedValue))
                splitType = SplitType.Merged;

            // Non-merged splitting
            if (features.ContainsKey(DatNonMergedValue))
                splitType = SplitType.NonMerged;

            // Split splitting
            if (features.ContainsKey(DatSplitValue))
                splitType = SplitType.Split;

            // Full dedup
            if (features.ContainsKey(DedupValue))
                datHeader.DedupeRoms = DedupeType.Full;

            // Delete
            bool delete = features.ContainsKey(DeleteValue);

            // Depot
            bool depot = features.ContainsKey(DepotValue);

            // Deprecated Logiqx output
            bool deprecated = features.ContainsKey(DeprecatedValue);

            // Description as game name
            bool descAsName = features.ContainsKey(DescriptionAsNameValue);

            // Diff against
            if (features.ContainsKey(DiffAgainstValue))
                updateMode |= UpdateMode.DiffAgainst;

            // Diff all
            if (features.ContainsKey(DiffAllValue))
                updateMode |= UpdateMode.AllDiffs;

            // Diff cascade
            if (features.ContainsKey(DiffCascadeValue))
                updateMode |= UpdateMode.DiffCascade;

            // Diff duplicates
            if (features.ContainsKey(DiffDuplicatesValue))
                updateMode |= UpdateMode.DiffDupesOnly;

            // Diff individuals
            if (features.ContainsKey(DiffIndividualsValue))
                updateMode |= UpdateMode.DiffIndividualsOnly;

            // Diff no duplicates
            if (features.ContainsKey(DiffNoDuplicatesValue))
                updateMode |= UpdateMode.DiffNoDupesOnly;

            // Diff reverse cascade
            if (features.ContainsKey(DiffReverseCascadeValue))
                updateMode |= UpdateMode.DiffReverseCascade;

            // Extension splitting mode
            if (features.ContainsKey(ExtensionValue))
                splittingMode |= SplittingMode.Extension;

            // Game deduplication
            if (features.ContainsKey(GameDedupValue))
                datHeader.DedupeRoms = DedupeType.Game;

            // Game prefixing
            datHeader.GameName = features.ContainsKey(GamePrefixValue);

            // Hash splitting mode
            if (features.ContainsKey(HashValue))
                splittingMode |= SplittingMode.Hash;

            // Hash only
            bool hashOnly = features.ContainsKey(HashOnlyValue);

            // Individual processing
            bool individual = features.ContainsKey(IndividualValue);

            // Inplace processing
            bool inplace = features.ContainsKey(InplaceValue);

            // Inverse dat processing
            bool inverse = features.ContainsKey(InverseValue);

            // Keep empty games
            datHeader.KeepEmptyGames = features.ContainsKey(KeepEmptyGamesValue);

            // Level splitting mode
            if (features.ContainsKey(LevelValue))
                splittingMode |= SplittingMode.Level;

            // Match "of" tags in filter
            filter.IncludeOfInGame.Neutral = features.ContainsKey(MatchOfTagsValue);

            // Merge processing mode
            if (features.ContainsKey(MergeValue))
                updateMode |= UpdateMode.Merge;

            // No automatic date
            bool noAutomaticDate = features.ContainsKey(NoAutomaticDateValue);

            // Show nodump column
            bool showNodumpColumn = features.ContainsKey(NodumpColumnValue);

            // Don't store headers
            bool nostore = features.ContainsKey(NoStoreHeaderValue);

            // Filter by not runnable
            filter.Runnable.Neutral = features.ContainsKey(NotRunnableValue);

            // One rom per game output
            datHeader.OneRom = features.ContainsKey(OneRomPerGameValue);

            // Only same
            bool onlySame = features.ContainsKey(OnlySameValue);

            // Quick scan
            bool quickScan = features.ContainsKey(QuickValue);

            // Add quotes
            datHeader.Quotes = features.ContainsKey(QuotesValue);

            // Remove extensions
            datHeader.RemoveExtension = features.ContainsKey(RemoveExtensionsValue);

            // Remove unicode characters
            bool removeUnicode = features.ContainsKey(RemoveUnicodeValue);

            // Reverse base replacement mode
            if (features.ContainsKey(ReverseBaseReplaceValue))
                updateMode |= UpdateMode.ReverseBaseReplace;

            // Romba input/output
            datHeader.Romba = features.ContainsKey(RombaValue);

            // Output roms instead of games
            datHeader.UseRomName = features.ContainsKey(RomsValue);

            // Filter by runnable
            filter.Runnable.Neutral = features.ContainsKey(RunnableValue);

            // Set scan levels to all 0
            if (features.ContainsKey(ScanAllValue))
            {
                sevenzip = 0;
                gz = 0;
                rar = 0;
                zip = 0;
            }

            // Strip scene dates from names
            datHeader.SceneDateStrip = features.ContainsKey(SceneDateStripValue);

            // Use short output names
            bool shortname = features.ContainsKey(ShortValue);

            // Output all to a single set
            filter.Single.Neutral = features.ContainsKey(SingleSetValue);

            // Split by size
            if (features.ContainsKey(SizeValue))
                splittingMode |= SplittingMode.Size;

            // Skip archives
            if (features.ContainsKey(SkipArchivesValue))
                skipFileType = SkipFileType.Archive;

            // Skip files
            if (features.ContainsKey(SkipFilesValue))
                skipFileType = SkipFileType.File;

            // Skip first output
            bool skipFirstOutput = features.ContainsKey(SkipFirstOutputValue);

            // Skip MD5
            if (features.ContainsKey(SkipMd5Value))
                omitFromScan |= Hash.MD5;

            // Skip RIPEMD160
            if (features.ContainsKey(SkipRipeMd160Value))
                omitFromScan &= ~Hash.RIPEMD160; // TODO: This needs to be inverted later

            // Skip SHA-1
            if (features.ContainsKey(SkipSha1Value))
                omitFromScan |= Hash.SHA1;

            // Skip SHA-256
            if (features.ContainsKey(SkipSha256Value))
                omitFromScan &= ~Hash.SHA256; // TODO: This needs to be inverted later

            // Skip SHA-384
            if (features.ContainsKey(SkipSha384Value))
                omitFromScan &= ~Hash.SHA384; // TODO: This needs to be inverted later

            // Skip SHA-512
            if (features.ContainsKey(SkipSha512Value))
                omitFromScan &= ~Hash.SHA512; // TODO: This needs to be inverted later

            // Superdat output
            if (features.ContainsKey(SuperdatValue))
                datHeader.Type = "SuperDAT";

            // Tape archive format
            if (features.ContainsKey(TarValue))
                outputFormat = OutputFormat.TapeArchive;

            // Torrent 7-zip format
            if (features.ContainsKey(Torrent7zipValue))
                outputFormat = OutputFormat.Torrent7Zip;

            // Torrent GZip format
            if (features.ContainsKey(TorrentGzipValue))
                outputFormat = OutputFormat.TorrentGzip;

            // Torrent LRZip format
            if (features.ContainsKey(TorrentLrzipValue))
                outputFormat = OutputFormat.TorrentLRZip;

            // Torrent LZ4 format
            if (features.ContainsKey(TorrentLz4Value))
                outputFormat = OutputFormat.TorrentLZ4;

            // Torrent RAR format
            if (features.ContainsKey(TorrentRarValue))
                outputFormat = OutputFormat.TorrentRar;

            // Torrent XZ format
            if (features.ContainsKey(TorrentXzValue))
                outputFormat = OutputFormat.TorrentXZ;

            // Torrent Zip format
            if (features.ContainsKey(TorrentZipValue))
                outputFormat = OutputFormat.TorrentZip;

            // Torrent ZPAQ format
            if (features.ContainsKey(TorrentZpaqValue))
                outputFormat = OutputFormat.TorrentZPAQ;

            // Torrent Zstd format
            if (features.ContainsKey(TorrentZstdValue))
                outputFormat = OutputFormat.TorrentZstd;

            // Trim to NTFS length
            filter.Trim.Neutral = features.ContainsKey(TrimValue);

            // Split by type
            if (features.ContainsKey(TypeValue))
                splittingMode |= SplittingMode.Type;

            // Update dat after processing
            bool updateDat = features.ContainsKey(UpdateDatValue);

            // Update description from base
            if (features.ContainsKey(UpdateDescriptionValue))
            {
                Globals.Logger.User("This flag '{0}' is deprecated, please use {1} instead", UpdateDescriptionValue, string.Join(", ", _updateFieldListInput.Flags));
                updateFields.Add(Field.Description);
            }

            // Update game type from base
            if (features.ContainsKey(UpdateGameTypeValue))
            {
                Globals.Logger.User("This flag '{0}' is deprecated, please use {1} instead", UpdateGameTypeValue, string.Join(", ", _updateFieldListInput.Flags));
                updateFields.Add(Field.MachineType);
            }

            // Update hashes from base
            if (features.ContainsKey(UpdateHashesValue))
            {
                Globals.Logger.User("This flag '{0}' is deprecated, please use {1} instead", UpdateHashesValue, string.Join(", ", _updateFieldListInput.Flags));
                updateFields.Add(Field.CRC);
                updateFields.Add(Field.MD5);
                updateFields.Add(Field.RIPEMD160);
                updateFields.Add(Field.SHA1);
                updateFields.Add(Field.SHA256);
                updateFields.Add(Field.SHA384);
                updateFields.Add(Field.SHA512);
            }

            // Update manufacturer from base
            if (features.ContainsKey(UpdateManufacturerValue))
            {
                Globals.Logger.User("This flag '{0}' is deprecated, please use {1} instead", UpdateManufacturerValue, string.Join(", ", _updateFieldListInput.Flags));
                updateFields.Add(Field.Manufacturer);
            }

            // Update item names from base
            if (features.ContainsKey(UpdateNamesValue))
            {
                Globals.Logger.User("This flag '{0}' is deprecated, please use {1} instead", UpdateNamesValue, string.Join(", ", _updateFieldListInput.Flags));
                updateFields.Add(Field.Name);
            }

            // Update parents from base
            if (features.ContainsKey(UpdateParentsValue))
            {
                Globals.Logger.User("This flag '{0}' is deprecated, please use {1} instead", UpdateParentsValue, string.Join(", ", _updateFieldListInput.Flags));
                updateFields.Add(Field.CloneOf);
                updateFields.Add(Field.RomOf);
                updateFields.Add(Field.SampleOf);
            }

            // Update year from base
            if (features.ContainsKey(UpdateYearValue))
            {
                Globals.Logger.User("This flag '{0}' is deprecated, please use {1} instead", UpdateYearValue, string.Join(", ", _updateFieldListInput.Flags));
                updateFields.Add(Field.Year);
            }

            #endregion

            #region User Int32 Inputs

            // 7-zip scanning level
            if (features.ContainsKey(SevenZipInt32Value))
                sevenzip = features[SevenZipInt32Value].GetInt32Value() == Int32.MinValue ? features[SevenZipInt32Value].GetInt32Value() : 1;

            // GZip scanning level
            if (features.ContainsKey(GzInt32Value))
                gz = features[GzInt32Value].GetInt32Value() == Int32.MinValue ? features[GzInt32Value].GetInt32Value() : 1;

            // RAR scanning level
            if (features.ContainsKey(RarInt32Value))
                rar = features[RarInt32Value].GetInt32Value() == Int32.MinValue ? features[RarInt32Value].GetInt32Value() : 1;

            // Thread workers count
            if (features.ContainsKey(ThreadsInt32Value))
                Globals.MaxThreads = features[ThreadsInt32Value].GetInt32Value() == Int32.MinValue ? features[ThreadsInt32Value].GetInt32Value() : Globals.MaxThreads;

            // Zip scanning level
            if (features.ContainsKey(ZipInt32Value))
                zip = features[ZipInt32Value].GetInt32Value() == Int32.MinValue ? features[ZipInt32Value].GetInt32Value() : 1;

            #endregion

            #region User Int64 Inputs

            // Radix for splitting
            if (features.ContainsKey(RadixInt64Value))
                radix = features[RadixInt64Value].GetInt64Value() == Int64.MinValue ? features[RadixInt64Value].GetInt64Value() : 0;

            #endregion

            #region User List<string> Inputs

            // Base DATs list
            if (features.ContainsKey(BaseDatListValue))
                basePaths.AddRange(features[BaseDatListValue].GetListValue());

            // Match CRCs
            if (features.ContainsKey(CrcListValue))
                filter.CRC.PositiveSet.AddRange(features[CrcListValue].GetListValue());

            // Input DATs
            // TODO: Should this be a required flag instead of implied as "inputs"?
            if (features.ContainsKey(DatListValue))
                datfiles.AddRange(features[DatListValue].GetListValue());

            // Exclude fields
            if (features.ContainsKey(ExcludeFieldListValue))
            {
                foreach (string field in features[ExcludeFieldListValue].GetListValue())
                {
                    datHeader.ExcludeFields[(int)Utilities.GetField(field)] = true;
                }
            }

            // Extensions list 'A'
            if (features.ContainsKey(ExtAListValue))
                exta.AddRange(features[ExtAListValue].GetListValue());

            // Extensions list 'B'
            if (features.ContainsKey(ExtBListValue))
                extb.AddRange(features[ExtBListValue].GetListValue());

            // Match game descriptions
            if (features.ContainsKey(GameDescriptionListValue))
                filter.MachineDescription.PositiveSet.AddRange(features[GameDescriptionListValue].GetListValue());

            // Match game names
            if (features.ContainsKey(GameNameListValue))
                filter.MachineName.PositiveSet.AddRange(features[GameNameListValue].GetListValue());

            // Match game types
            if (features.ContainsKey(GameTypeListValue))
            {
                foreach (string mach in features[GameTypeListValue].GetListValue())
                {
                    filter.MachineTypes.Positive |= Utilities.GetMachineType(mach);
                }
            }

            // Match item names
            if (features.ContainsKey(ItemNameListValue))
                filter.ItemName.PositiveSet.AddRange(features[ItemNameListValue].GetListValue());

            // Match item types
            if (features.ContainsKey(ItemTypeListValue))
                filter.ItemTypes.PositiveSet.AddRange(features[ItemTypeListValue].GetListValue());

            // Match MD5s
            if (features.ContainsKey(Md5ListValue))
                filter.MD5.PositiveSet.AddRange(features[Md5ListValue].GetListValue());

            // Not match CRCs
            if (features.ContainsKey(NotCrcListValue))
                filter.CRC.NegativeSet.AddRange(features[NotCrcListValue].GetListValue());

            // Not match game descriptions
            if (features.ContainsKey(NotGameDescriptionListValue))
                filter.MachineDescription.NegativeSet.AddRange(features[NotGameDescriptionListValue].GetListValue());

            // Not match game names
            if (features.ContainsKey(NotGameNameListValue))
                filter.MachineName.NegativeSet.AddRange(features[NotGameNameListValue].GetListValue());

            // Not match game types
            if (features.ContainsKey(NotGameTypeListValue))
            {
                foreach (string mach in features[NotGameTypeListValue].GetListValue())
                {
                    filter.MachineTypes.Negative |= Utilities.GetMachineType(mach);
                }
            }

            // Not match item names
            if (features.ContainsKey(NotItemNameListValue))
                filter.ItemName.NegativeSet.AddRange(features[NotItemNameListValue].GetListValue());

            // Not match item types
            if (features.ContainsKey(NotItemTypeListValue))
                filter.ItemTypes.NegativeSet.AddRange(features[NotItemTypeListValue].GetListValue());

            // Not match MD5s
            if (features.ContainsKey(NotMd5ListValue))
                filter.MD5.NegativeSet.AddRange(features[NotMd5ListValue].GetListValue());

            // Not match RIPEMD160s
            if (features.ContainsKey(NotRipeMd160ListValue))
                filter.RIPEMD160.NegativeSet.AddRange(features[NotRipeMd160ListValue].GetListValue());

            // Not match SHA-1s
            if (features.ContainsKey(NotSha1ListValue))
                filter.SHA1.NegativeSet.AddRange(features[NotSha1ListValue].GetListValue());

            // Not match SHA-256s
            if (features.ContainsKey(NotSha256ListValue))
                filter.SHA256.NegativeSet.AddRange(features[NotSha256ListValue].GetListValue());

            // Not match SHA-384s
            if (features.ContainsKey(NotSha384ListValue))
                filter.SHA384.NegativeSet.AddRange(features[NotSha384ListValue].GetListValue());

            // Not match SHA-512s
            if (features.ContainsKey(NotSha512ListValue))
                filter.SHA512.NegativeSet.AddRange(features[NotSha512ListValue].GetListValue());

            // Not match item status
            if (features.ContainsKey(NotStatusListValue))
            {
                foreach (string nstat in features[NotStatusListValue].GetListValue())
                {
                    filter.ItemStatuses.Negative |= Utilities.GetItemStatus(nstat);
                }
            }

            // DAT output types
            if (features.ContainsKey(OutputTypeListValue))
            {
                foreach (string ot in features[OutputTypeListValue].GetListValue())
                {
                    DatFormat dftemp = Utilities.GetDatFormat(ot);
                    if (dftemp == DatFormat.Logiqx && deprecated)
                        datHeader.DatFormat |= DatFormat.LogiqxDeprecated;
                    else
                        datHeader.DatFormat |= dftemp;
                }
            }

            // Stats report output types
            if (features.ContainsKey(ReportTypeListValue))
            {
                foreach (string rt in features[ReportTypeListValue].GetListValue())
                {
                    statDatFormat |= Utilities.GetStatFormat(rt);
                }
            }

            // Match RIPEMD160s
            if (features.ContainsKey(RipeMd160ListValue))
                filter.RIPEMD160.PositiveSet.AddRange(features[RipeMd160ListValue].GetListValue());

            // Match SHA-1s
            if (features.ContainsKey(Sha1ListValue))
                filter.SHA1.PositiveSet.AddRange(features[Sha1ListValue].GetListValue());

            // Match SHA-256s
            if (features.ContainsKey(Sha256ListValue))
                filter.SHA256.PositiveSet.AddRange(features[Sha256ListValue].GetListValue());

            // Match SHA-384s
            if (features.ContainsKey(Sha384ListValue))
                filter.SHA384.PositiveSet.AddRange(features[Sha384ListValue].GetListValue());

            // Match SHA-512s
            if (features.ContainsKey(Sha512ListValue))
                filter.SHA512.PositiveSet.AddRange(features[Sha512ListValue].GetListValue());

            // Match item statuses
            if (features.ContainsKey(StatusListValue))
            {
                foreach (string stat in features[StatusListValue].GetListValue())
                {
                    filter.ItemStatuses.Positive |= Utilities.GetItemStatus(stat);
                }
            }

            // Update fields
            if (features.ContainsKey(UpdateFieldListValue))
            {
                foreach (string field in features[UpdateFieldListValue].GetListValue())
                {
                    updateFields.Add(Utilities.GetField(field));
                }
            }

            #endregion

            #region User String Inputs

            // Add extensions
            if (features.ContainsKey(AddExtensionStringValue))
                datHeader.AddExtension = features[AddExtensionStringValue].GetStringValue();

            // Author
            if (features.ContainsKey(AuthorStringValue))
                datHeader.Author = features[AuthorStringValue].GetStringValue();

            // Category
            if (features.ContainsKey(CategoryStringValue))
                datHeader.Category = features[CategoryStringValue].GetStringValue();

            // Comment
            if (features.ContainsKey(CommentStringValue))
                datHeader.Comment = features[CommentStringValue].GetStringValue();

            // Date
            if (features.ContainsKey(DateStringValue))
                datHeader.Date = features[DateStringValue].GetStringValue();

            // Description
            if (features.ContainsKey(DescriptionStringValue))
                datHeader.Description = features[DescriptionStringValue].GetStringValue();

            // Email
            if (features.ContainsKey(EmailStringValue))
                datHeader.Email = features[EmailStringValue].GetStringValue();

            // Filter by size equal
            if (features.ContainsKey(EqualStringValue))
                filter.Size.Neutral = Utilities.GetSizeFromString(features[EqualStringValue].GetStringValue());

            // File name
            if (features.ContainsKey(FilenameStringValue))
                datHeader.FileName = features[FilenameStringValue].GetStringValue();

            // Force merging
            if (features.ContainsKey(ForceMergingStringInput))
                datHeader.ForceMerging = Utilities.GetForceMerging(features[ForceMergingStringInput].GetStringValue());

            // Force nodump
            if (features.ContainsKey(ForceNodumpStringInput))
                datHeader.ForceNodump = Utilities.GetForceNodump(features[ForceNodumpStringInput].GetStringValue());

            // Force packing
            if (features.ContainsKey(ForcePackingStringInput))
                datHeader.ForcePacking = Utilities.GetForcePacking(features[ForcePackingStringInput].GetStringValue());

            // Filter by size greater or equal
            if (features.ContainsKey(GreaterStringValue))
                filter.Size.Positive = Utilities.GetSizeFromString(features[GreaterStringValue].GetStringValue());

            // Header
            if (features.ContainsKey(HeaderStringValue))
                datHeader.Header = features[HeaderStringValue].GetStringValue();

            // Homepage
            if (features.ContainsKey(HomepageStringValue))
                datHeader.Homepage = features[HomepageStringValue].GetStringValue();

            // Filter by size less than or equal
            if (features.ContainsKey(LessStringValue))
                filter.Size.Negative = Utilities.GetSizeFromString(features[LessStringValue].GetStringValue());

            // Name
            if (features.ContainsKey(NameStringValue))
                datHeader.Name = features[NameStringValue].GetStringValue();

            // Output directory
            if (features.ContainsKey(OutputDirStringValue))
                outDir = features[OutputDirStringValue].GetStringValue();

            // Postfix
            if (features.ContainsKey(PostfixStringValue))
                datHeader.Postfix = features[PostfixStringValue].GetStringValue();

            // Prefix
            if (features.ContainsKey(PrefixStringValue))
                datHeader.Prefix = features[PrefixStringValue].GetStringValue();

            // Replace extension
            if (features.ContainsKey(ReplaceExtensionStringValue))
                datHeader.ReplaceExtension = features[ReplaceExtensionStringValue].GetStringValue();

            // Root directory
            if (features.ContainsKey(RootStringValue))
                datHeader.RootDir = features[RootStringValue].GetStringValue();

            // Root directory for recalc
            if (features.ContainsKey(RootDirStringValue))
                filter.Root.Neutral = features[RootDirStringValue].GetStringValue();

            // Temp directory
            if (features.ContainsKey(TempStringValue))
                tempDir = features[TempStringValue].GetStringValue();

            // Url
            if (features.ContainsKey(UrlStringValue))
                datHeader.Url = features[UrlStringValue].GetStringValue();

            // Version
            if (features.ContainsKey(VersionStringValue))
                datHeader.Version = features[VersionStringValue].GetStringValue();

            #endregion

            // Now take care of each mode in succesion
            // TODO: Should each feature have its own class?
            // TODO: Should each feature have subsets of the flags?
            switch (feature)
            {
                case HelpFeatureValue:
                case HelpDetailedFeatureValue:
                case ScriptFeatureValue:
                    // No-op as this should be caught
                    break;

                // Create a DAT from a directory or set of directories
                case DatFromDirFeatureValue:
                    VerifyInputs(inputs, feature);
                    InitDatFromDir(inputs, datHeader, omitFromScan, noAutomaticDate, archivesAsFiles, chdsAsFiles,
                        skipFileType, addBlankFiles, addFileDates, tempDir, outDir, copyFiles, filter);
                    break;

                // If we're in header extract and remove mode
                case ExtractFeatureValue:
                    VerifyInputs(inputs, feature);
                    InitExtractRemoveHeader(inputs, outDir, nostore);
                    break;

                // If we're in header restore mode
                case RestoreFeatureValue:
                    VerifyInputs(inputs, feature);
                    InitReplaceHeader(inputs, outDir);
                    break;

                // If we're using the sorter
                case SortFeatureValue:
                    InitSort(datfiles, inputs, outDir, depot, quickScan, addFileDates, delete, inverse,
                        outputFormat, datHeader.Romba, sevenzip, gz, rar, zip, updateDat, datHeader.Header,
                        splitType, chdsAsFiles, individual);
                    break;

                // Split a DAT by the split type
                case SplitFeatureValue:
                    VerifyInputs(inputs, feature);
                    InitSplit(inputs, outDir, inplace, datHeader.DatFormat, splittingMode, exta, extb, shortname, basedat, radix);
                    break;

                // Get statistics on input files
                case StatsFeatureValue:
                    VerifyInputs(inputs, feature);
                    InitStats(inputs, datHeader.FileName, outDir, individual, showBaddumpColumn, showNodumpColumn, statDatFormat);
                    break;

                // Convert, update, merge, diff, and filter a DAT or folder of DATs
                case UpdateFeatureValue:
                    VerifyInputs(inputs, feature);
                    InitUpdate(inputs, basePaths, datHeader, updateMode, inplace, skipFirstOutput, noAutomaticDate, filter,
                        splitType, outDir, cleanGameNames, removeUnicode, descAsName, updateFields, onlySame);
                    break;

                // If we're using the verifier
                case VerifyFeatureValue:
                    VerifyInputs(inputs, feature);
                    InitVerify(datfiles, inputs, depot, hashOnly, quickScan, datHeader.Header, splitType, chdsAsFiles, individual, filter);
                    break;

                // If nothing is set, show the help
                default:
                    _help.OutputGenericHelp();
                    break;
            }

            Globals.Logger.Close();
            return;
        }

        private static void VerifyInputs(List<string> inputs, string feature)
        {
            if (inputs.Count == 0)
            {
                Globals.Logger.Error("This feature requires at least one input");
                _help.OutputIndividualFeature(feature);
                Environment.Exit(0);
            }
        }
    }
}
