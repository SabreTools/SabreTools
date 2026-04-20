using System;
using System.Collections.Generic;
using System.IO;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
using System.Threading.Tasks;
#endif
using SabreTools.DatTools;
using SabreTools.IO;
using SabreTools.IO.Extensions;
using SabreTools.Logging;
using SabreTools.Metadata.DatFiles;

namespace SabreTools.Features
{
    internal class Update : BaseFeature
    {
        public const string DisplayName = "Update";

        private static readonly string[] _flags = ["ud", "update"];

        private const string _description = "Update and manipulate DAT(s)";

        private const string _detailed = "This is the multitool part of the program, allowing for almost every manipulation to a DAT, or set of DATs. This is also a combination of many different programs that performed DAT manipulation that work better together.";

        public Update()
            : base(DisplayName, _flags, _description, _detailed)
        {
            RequiresInputs = true;

            // Common Features
            AddCommonFeatures();

            // Output Formats
            Add(OutputTypeListInput);
            this[OutputTypeListInput]!.Add(PrefixStringInput);
            this[OutputTypeListInput]!.Add(PostfixStringInput);
            this[OutputTypeListInput]!.Add(QuotesFlag);
            this[OutputTypeListInput]!.Add(RomsFlag);
            this[OutputTypeListInput]!.Add(GamePrefixFlag);
            this[OutputTypeListInput]!.Add(AddExtensionStringInput);
            this[OutputTypeListInput]!.Add(ReplaceExtensionStringInput);
            this[OutputTypeListInput]!.Add(RemoveExtensionsFlag);
            this[OutputTypeListInput]!.Add(RombaFlag);
            this[OutputTypeListInput]![RombaFlag]!.Add(RombaDepthInt32Input);
            this[OutputTypeListInput]!.Add(DeprecatedFlag);

            AddHeaderFeatures();
            Add(KeepEmptyGamesFlag);
            Add(CleanFlag);
            Add(RemoveUnicodeFlag);
            Add(DescriptionAsNameFlag);
            AddInternalSplitFeatures();
            Add(TrimFlag);
            this[TrimFlag]!.Add(RootDirStringInput);
            Add(SingleSetFlag);
            Add(DedupFlag);
            Add(GameDedupFlag);
            Add(MergeFlag);
            this[MergeFlag]!.Add(NoAutomaticDateFlag);
            Add(DiffAllFlag);
            this[DiffAllFlag]!.Add(NoAutomaticDateFlag);
            Add(DiffDuplicatesFlag);
            this[DiffDuplicatesFlag]!.Add(NoAutomaticDateFlag);
            Add(DiffIndividualsFlag);
            this[DiffIndividualsFlag]!.Add(NoAutomaticDateFlag);
            Add(DiffNoDuplicatesFlag);
            this[DiffNoDuplicatesFlag]!.Add(NoAutomaticDateFlag);
            Add(DiffAgainstFlag);
            this[DiffAgainstFlag]!.Add(BaseDatListInput);
            this[DiffAgainstFlag]!.Add(ByGameFlag);
            Add(BaseReplaceFlag);
            this[BaseReplaceFlag]!.Add(BaseDatListInput);
            this[BaseReplaceFlag]!.Add(UpdateFieldListInput);
            this[BaseReplaceFlag]![UpdateFieldListInput]!.Add(OnlySameFlag);
            Add(ReverseBaseReplaceFlag);
            this[ReverseBaseReplaceFlag]!.Add(BaseDatListInput);
            this[ReverseBaseReplaceFlag]!.Add(UpdateFieldListInput);
            this[ReverseBaseReplaceFlag]![UpdateFieldListInput]!.Add(OnlySameFlag);
            Add(DiffCascadeFlag);
            this[DiffCascadeFlag]!.Add(SkipFirstOutputFlag);
            Add(DiffReverseCascadeFlag);
            this[DiffReverseCascadeFlag]!.Add(SkipFirstOutputFlag);
            Add(ExtraIniListInput);
            AddFilteringFeatures();
            Add(OutputDirStringInput);
            Add(InplaceFlag);
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            // If the base fails, just fail out
            if (!base.Execute())
                return false;

            // Get feature flags
            var updateMachineFieldNames = GetUpdateMachineFields();
            var updateItemFieldNames = GetUpdateDatItemFields();
            var updateMode = GetUpdateMode();

            // Normalize the extensions
            Modifiers!.AddExtension = string.IsNullOrEmpty(Modifiers.AddExtension) || Modifiers.AddExtension!.StartsWith(".")
                ? Modifiers.AddExtension
                : $".{Modifiers.AddExtension}";
            Modifiers.ReplaceExtension = string.IsNullOrEmpty(Modifiers.ReplaceExtension) || Modifiers.ReplaceExtension!.StartsWith(".")
                ? Modifiers.ReplaceExtension
                : $".{Modifiers.ReplaceExtension}";

            // If no update fields are set, default to Names
            if (updateItemFieldNames is null || updateItemFieldNames.Count == 0)
            {
                updateItemFieldNames = [];
                updateItemFieldNames["item"] = ["name"];
            }

            // Ensure we only have files in the inputs
            List<ParentablePath> inputPaths = IOExtensions.GetFilesOnly(Inputs, appendParent: true);
            List<ParentablePath> basePaths = IOExtensions.GetFilesOnly(GetStringList(BaseDatListValue));

            // Ensure the output directory
            OutputDir = OutputDir.EnsureDirectory();

            // If we're in standard update mode, run through all of the inputs
            if (updateMode == UpdateMode.None)
            {
                StandardUpdate(inputPaths, GetBoolean(InplaceValue), GetBoolean(NoAutomaticDateValue));
                return true;
            }

            // Reverse inputs if we're in a required mode
#if NET20 || NET35
            if ((updateMode & UpdateMode.DiffReverseCascade) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.DiffReverseCascade))
#endif
            {
                updateMode |= UpdateMode.DiffCascade;
                inputPaths.Reverse();
            }
#if NET20 || NET35
            if ((updateMode & UpdateMode.ReverseBaseReplace) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.ReverseBaseReplace))
#endif
            {
                updateMode |= UpdateMode.BaseReplace;
                basePaths.Reverse();
            }

            // Create a DAT to capture inputs
            DatFile userInputDat = Parser.CreateDatFile(Header!, Modifiers);

            // If we're in a non-replacement special update mode and the names aren't set, set defaults
            SetDefaultHeaderValues(userInputDat, updateMode, GetBoolean(NoAutomaticDateValue));

            // Populate using the correct set
            List<DatHeader> datHeaders = GetDatHeaders(updateMode, inputPaths, basePaths, userInputDat);

            // Perform additional processing steps
            AdditionalProcessing(userInputDat);

            // Output only DatItems that are duplicated across inputs
#if NET20 || NET35
            if ((updateMode & UpdateMode.DiffDupesOnly) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.DiffDupesOnly))
#endif
            {
                DatFile dupeData = Diffing.Duplicates(userInputDat, inputPaths);

                // Ensure there are output formats
                var datFormats = DatFormats;
                if (datFormats is null || datFormats.Count == 0)
                    datFormats = [dupeData.Header.DatFormat ?? DatFormat.Logiqx];

                InternalStopwatch watch = new("Outputting duplicate DAT");
                dupeData.Write(datFormats, OutputDir, overwrite: false);
                watch.Stop();
            }

            // Output only DatItems that are not duplicated across inputs
#if NET20 || NET35
            if ((updateMode & UpdateMode.DiffNoDupesOnly) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.DiffNoDupesOnly))
#endif
            {
                DatFile outerDiffData = Diffing.NoDuplicates(userInputDat, inputPaths);

                // Ensure there are output formats
                var datFormats = DatFormats;
                if (datFormats is null || datFormats.Count == 0)
                    datFormats = [outerDiffData.Header.DatFormat ?? DatFormat.Logiqx];

                InternalStopwatch watch = new("Outputting no duplicate DAT");
                outerDiffData.Write(datFormats, OutputDir, overwrite: false);
                watch.Stop();
            }

            // Output only DatItems that are unique to each input
#if NET20 || NET35
            if ((updateMode & UpdateMode.DiffIndividualsOnly) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.DiffIndividualsOnly))
#endif
            {
                // Get all of the output DatFiles
                List<DatFile> datFiles = Diffing.Individuals(userInputDat, inputPaths);

                // Loop through and output the new DatFiles
                InternalStopwatch watch = new("Outputting all individual DATs");

#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                Parallel.For(0, inputPaths.Count, j =>
#else
                for (int j = 0; j < inputPaths.Count; j++)
#endif
                {
                    // Ensure there are output formats
                    var datFormats = DatFormats;
                    if (datFormats is null || datFormats.Count == 0)
                        datFormats = [datFiles[j].Header.DatFormat ?? DatFormat.Logiqx];

                    // Try to output the file
                    string path = inputPaths[j].GetOutputPath(OutputDir, GetBoolean(InplaceValue))!;
                    datFiles[j].Write(datFormats, path, overwrite: GetBoolean(InplaceValue));
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                });
#else
                }
#endif

                watch.Stop();
            }

            // Output cascaded diffs
#if NET20 || NET35
            if ((updateMode & UpdateMode.DiffCascade) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.DiffCascade))
#endif
            {
                // Preprocess the DatHeaders
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                Parallel.For(0, datHeaders.Count, j =>
#else
                for (int j = 0; j < datHeaders.Count; j++)
#endif
                {
                    // Skip renaming if not outputting to the runtime folder
                    if (GetBoolean(InplaceValue) || OutputDir != Environment.CurrentDirectory)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif

                    // Update the naming for the header
                    string innerpost = $" ({j} - {inputPaths[j].GetNormalizedFileName(true)} Only)";
                    datHeaders[j] = userInputDat.Header;
                    datHeaders[j].FileName = datHeaders[j].FileName + innerpost;
                    datHeaders[j].Name = datHeaders[j].Name + innerpost;
                    datHeaders[j].Description = datHeaders[j].Description + innerpost;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                });
#else
                }
#endif

                // Get all of the output DatFiles
                List<DatFile> datFiles = Diffing.Cascade(userInputDat, datHeaders);

                // Loop through and output the new DatFiles
                InternalStopwatch watch = new("Outputting all created DATs");

                int startIndex = GetBoolean(SkipFirstOutputValue) ? 1 : 0;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                Parallel.For(startIndex, inputPaths.Count, j =>
#else
                for (int j = startIndex; j < inputPaths.Count; j++)
#endif
                {
                    // Ensure there are output formats
                    var datFormats = DatFormats;
                    if (datFormats is null || datFormats.Count == 0)
                        datFormats = [datFiles[j].Header.DatFormat ?? DatFormat.Logiqx];

                    // Try to output the file
                    string path = inputPaths[j].GetOutputPath(OutputDir, GetBoolean(InplaceValue))!;
                    datFiles[j].Write(datFormats, path, overwrite: GetBoolean(InplaceValue));
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                });
#else
                }
#endif

                watch.Stop();
            }

            // Output differences against a base DAT
#if NET20 || NET35
            if ((updateMode & UpdateMode.DiffAgainst) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.DiffAgainst))
#endif
            {
                // Loop through each input and diff against the base
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                Parallel.ForEach(inputPaths, inputPath =>
#else
                foreach (var inputPath in inputPaths)
#endif
                {
                    // Create a new DatFile
                    DatFile repDat = Parser.CreateDatFile(Header!, Modifiers!);
                    _logger.User($"Processing '{Path.GetFileName(inputPath.CurrentPath)}'");

                    // Tell users if their file doesn't have a recognized extension
                    if (!Parser.HasValidDatExtension(inputPath.CurrentPath))
                    {
                        _logger.Warning($"'{inputPath.CurrentPath} does not have a recognized extension! Skipping...");
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif
                    }

                    // Check the current format
                    bool isSeparatedFile = DatFormats!.Contains(DatFormat.CSV)
                        || DatFormats.Contains(DatFormat.SSV)
                        || DatFormats.Contains(DatFormat.TSV);

                    // Clear format and parse
                    repDat.Header.DatFormat = null;
                    Parser.ParseInto(repDat,
                        inputPath.CurrentPath,
                        indexId: 1,
                        keep: true,
                        keepext: isSeparatedFile,
                        filterRunner: FilterRunner);

                    // Ensure there are output formats
                    var datFormats = DatFormats;
                    if (datFormats is null || datFormats.Count == 0)
                        datFormats = [repDat.Header.DatFormat ?? DatFormat.Logiqx];

                    // Perform additional processing steps
                    AdditionalProcessing(repDat);

                    // Now replace the fields from the base DatFile
                    Diffing.Against(userInputDat, repDat, GetBoolean(ByGameValue));

                    // Finally output the diffed DatFile
                    string interOutDir = inputPath.GetOutputPath(OutputDir, GetBoolean(InplaceValue))!;
                    repDat.Write(datFormats, interOutDir, overwrite: GetBoolean(InplaceValue));
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                });
#else
                }
#endif
            }

            // Output DATs after replacing fields from a base DatFile
#if NET20 || NET35
            if ((updateMode & UpdateMode.BaseReplace) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.BaseReplace))
#endif
            {
                // Loop through each input and apply the base DatFile
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                Parallel.ForEach(inputPaths, inputPath =>
#else
                foreach (var inputPath in inputPaths)
#endif
                {
                    // Create a new DatFile
                    DatFile repDat = Parser.CreateDatFile(Header!, Modifiers!);
                    _logger.User($"Processing '{Path.GetFileName(inputPath.CurrentPath)}'");

                    // Tell users if their file doesn't have a recognized extension
                    if (!Parser.HasValidDatExtension(inputPath.CurrentPath))
                    {
                        _logger.Warning($"'{inputPath.CurrentPath} does not have a recognized extension! Skipping...");
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif
                    }

                    // Check the current format
                    bool isSeparatedFile = DatFormats!.Contains(DatFormat.CSV)
                        || DatFormats.Contains(DatFormat.SSV)
                        || DatFormats.Contains(DatFormat.TSV);

                    // Clear format and parse
                    repDat.Header.DatFormat = null;
                    Parser.ParseInto(repDat,
                        inputPath.CurrentPath,
                        indexId: 1,
                        keep: true,
                        keepext: isSeparatedFile,
                        filterRunner: FilterRunner);

                    // Ensure there are output formats
                    var datFormats = DatFormats;
                    if (datFormats is null || datFormats.Count == 0)
                        datFormats = [repDat.Header.DatFormat ?? DatFormat.Logiqx];

                    // Perform additional processing steps
                    AdditionalProcessing(repDat);

                    // Now replace the fields from the base DatFile
                    Replacer.BaseReplace(
                        userInputDat,
                        repDat,
                        updateMachineFieldNames,
                        updateItemFieldNames,
                        GetBoolean(OnlySameValue));

                    // Finally output the replaced DatFile
                    string interOutDir = inputPath.GetOutputPath(OutputDir, GetBoolean(InplaceValue))!;
                    repDat.Write(datFormats, interOutDir, overwrite: GetBoolean(InplaceValue));
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                });
#else
                }
#endif
            }

            // Merge all input files and write
            // This has to be last due to the SuperDAT handling
#if NET20 || NET35
            if ((updateMode & UpdateMode.Merge) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.Merge))
#endif
            {
                // If we're in SuperDAT mode, prefix all games with their respective DATs
                if (string.Equals(userInputDat.Header.Type, "SuperDAT", StringComparison.OrdinalIgnoreCase))
                {
                    MergeSplit.ApplySuperDAT(userInputDat, inputPaths);
                    MergeSplit.ApplySuperDATDB(userInputDat, inputPaths);
                }

                // Ensure there are output formats
                var datFormats = DatFormats;
                if (datFormats is null || datFormats.Count == 0)
                    datFormats = [userInputDat.Header.DatFormat ?? DatFormat.Logiqx];

                userInputDat.Write(datFormats, OutputDir);
            }

            return true;
        }

        /// <summary>
        /// Set default header values for non-specialized update types
        /// </summary>
        /// <param name="datFile">DatFile to update the header for</param>
        /// <param name="updateMode">Update mode that is currently being run</param>
        /// <param name="noAutomaticDate">True if date should be omitted from the description, false otherwise</param>
        private void SetDefaultHeaderValues(DatFile datFile, UpdateMode updateMode, bool noAutomaticDate)
        {
            // Skip running if a required objects are null
            if (datFile.Header is null || Cleaner is null)
                return;

            // Skip running for diff against and base replacement
#if NET20 || NET35
            if ((updateMode & UpdateMode.DiffAgainst) != 0)
                return;
            if ((updateMode & UpdateMode.BaseReplace) != 0)
                return;
#else
            if (updateMode.HasFlag(UpdateMode.DiffAgainst))
                return;
            if (updateMode.HasFlag(UpdateMode.BaseReplace))
                return;
#endif

            // Date
            if (string.IsNullOrEmpty(datFile.Header.Date))
                datFile.Header.Date = DateTime.Now.ToString("yyyy-MM-dd");

            // Name
            if (string.IsNullOrEmpty(datFile.Header.Name))
            {
                datFile.Header.Name = (updateMode != 0 ? "DiffDAT" : "MergeDAT")
                    + (datFile.Header.Type == "SuperDAT" ? "-SuperDAT" : string.Empty)
                    + (Cleaner.DedupeRoms != DedupeType.None ? "-deduped" : string.Empty);
            }

            // Description
            if (string.IsNullOrEmpty(datFile.Header.Description))
            {
                datFile.Header.Description = (updateMode != 0 ? "DiffDAT" : "MergeDAT")
                    + (datFile.Header.Type == "SuperDAT" ? "-SuperDAT" : string.Empty)
                    + (Cleaner!.DedupeRoms != DedupeType.None ? " - deduped" : string.Empty);

                if (!noAutomaticDate)
                    datFile.Header.Description = $"{datFile.Header.Description} ({datFile.Header.Date})";
            }

            // Category
            if (string.IsNullOrEmpty(datFile.Header.Category) && updateMode != 0)
                datFile.Header.Category = "DiffDAT";

            // Author
            if (string.IsNullOrEmpty(datFile.Header.Author))
                datFile.Header.Author = $"SabreTools {Globals.Version}";

            // Comment
            if (string.IsNullOrEmpty(datFile.Header.Comment))
                datFile.Header.Comment = $"Generated by SabreTools {Globals.Version}";
        }

        /// <summary>
        /// Perform standard processing and cleaning
        /// </summary>
        /// <param name="inputPaths">Set of input paths to process</param>
        /// <param name="inplace">True to output to the input folder, false otherwise</param>
        /// <param name="noAutomaticDate">True if date should be omitted from the description, false otherwise</param>
        private void StandardUpdate(List<ParentablePath> inputPaths, bool inplace, bool noAutomaticDate)
        {
            // Loop through each input and update
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(inputPaths, inputPath =>
#else
            foreach (var inputPath in inputPaths)
#endif
            {
                // Tell users if their file doesn't have a recognized extension
                if (!Parser.HasValidDatExtension(inputPath.CurrentPath))
                {
                    _logger.Warning($"'{inputPath.CurrentPath} does not have a recognized extension! Skipping...");
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif
                }

                // Create a new base DatFile
                DatFile datFile = Parser.CreateDatFile(Header!, Modifiers!);
                _logger.User($"Processing '{Path.GetFileName(inputPath.CurrentPath)}'");

                // Check the current format
                bool isSeparatedFile = DatFormats!.Contains(DatFormat.CSV)
                    || DatFormats.Contains(DatFormat.SSV)
                    || DatFormats.Contains(DatFormat.TSV);

                // Clear format and parse
                datFile.Header.DatFormat = null;
                Parser.ParseInto(datFile,
                    inputPath.CurrentPath,
                    keep: true,
                    keepext: isSeparatedFile,
                    filterRunner: FilterRunner);

                // Ensure there are output formats
                var datFormats = DatFormats;
                if (datFormats is null || datFormats.Count == 0)
                    datFormats = [datFile.Header.DatFormat ?? DatFormat.Logiqx];

                // Set any missing header values
                SetDefaultHeaderValues(datFile, updateMode: UpdateMode.None, noAutomaticDate: noAutomaticDate);

                // Perform additional processing steps
                AdditionalProcessing(datFile);

                // Get the correct output path
                string realOutDir = inputPath.GetOutputPath(OutputDir, inplace)!;

                // Try to output the file, overwriting only if it's not in the current directory
                datFile.Write(datFormats, realOutDir, overwrite: inplace);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Get the DatHeader values appopriate for the update mode
        /// </summary>
        /// <param name="updateMode">Update mode that is currently being run</param>
        /// <param name="inputPaths">Set of input paths</param>
        /// <param name="basePaths">Set of base paths</param>
        /// <param name="userInputDat">DatFile to parse into</param>
        /// <returns>List of DatHeader values representing the parsed files</returns>
        private List<DatHeader> GetDatHeaders(UpdateMode updateMode, List<ParentablePath> inputPaths, List<ParentablePath> basePaths, DatFile userInputDat)
        {
#if NET20 || NET35
            if ((updateMode & UpdateMode.DiffAgainst) != 0 || (updateMode & UpdateMode.BaseReplace) != 0)
#else
            if (updateMode.HasFlag(UpdateMode.DiffAgainst) || updateMode.HasFlag(UpdateMode.BaseReplace))
#endif
                return Parser.PopulateUserData(userInputDat, basePaths, FilterRunner!);
            else
                return Parser.PopulateUserData(userInputDat, inputPaths, FilterRunner!);
        }

        /// <summary>
        /// Perform additional processing on a given DatFile
        /// </summary>
        /// <param name="datFile">DatFile to process</param>
        private void AdditionalProcessing(DatFile datFile)
        {
            Extras!.ApplyExtras(datFile);
            Extras!.ApplyExtrasDB(datFile);
            Splitter!.ApplySplitting(datFile, useTags: false, filterRunner: FilterRunner);
            datFile.ExecuteFilters(FilterRunner!);
            Cleaner!.ApplyCleaning(datFile);
            Remover!.ApplyRemovals(datFile);
        }
    }
}
