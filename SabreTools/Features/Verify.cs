using SabreTools.DatFiles;
using SabreTools.DatTools;
using SabreTools.Hashing;
using SabreTools.IO;
using SabreTools.IO.Extensions;
using SabreTools.IO.Logging;

namespace SabreTools.Features
{
    internal class Verify : BaseFeature
    {
        public const string DisplayName = "Verify";

        private static readonly string[] _flags = ["ve", "verify"];

        private const string _description = "Verify a folder against DATs";

        private const string _longDescription = "When used, this will use an input DAT or set of DATs to blindly check against an input folder. The base of the folder is considered the base for the combined DATs and games are either the directories or archives within. This will only do a direct verification of the items within and will create a fixdat afterwards for missing files.";

        public Verify()
            : base(DisplayName, _flags, _description, _longDescription)
        {
            RequiresInputs = true;

            // Common Features
            AddCommonFeatures();

            Add(DatListInput);
            Add(DepotFlag);
            this[DepotFlag]!.Add(DepotDepthInt32Input);
            Add(OutputDirStringInput);
            Add(HashOnlyFlag);
            Add(QuickFlag);
            Add(HeaderStringInput);
            Add(AaruFormatsAsFilesFlag);
            Add(ChdsAsFilesFlag);
            Add(IndividualFlag);
            AddInternalSplitFeatures();
            Add(ExtraIniListInput);
            AddFilteringFeatures();
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            // If the base fails, just fail out
            if (!base.Execute())
                return false;

            // Get a list of files from the input datfiles
            var datfiles = GetStringList(DatListValue);
            var datfilePaths = PathTool.GetFilesOnly(datfiles);

            // Get feature flags
            TreatAsFile treatAsFile = GetTreatAsFile();
            bool hashOnly = GetBoolean(HashOnlyValue);
            bool quickScan = GetBoolean(QuickValue);
            HashType[] hashes = quickScan ? [HashType.CRC32] : [HashType.CRC32, HashType.MD5, HashType.SHA1];
            var dfd = new DatTools.DatFromDir(hashes, SkipFileType.None, treatAsFile, addBlanks: false);

            // Ensure the output directory
            OutputDir = OutputDir.Ensure();

            // If we are in individual mode, process each DAT on their own
            if (GetBoolean(IndividualValue))
            {
                foreach (ParentablePath datfile in datfilePaths)
                {
                    // Parse in from the file
                    DatFile datdata = Parser.CreateDatFile();
                    datdata.Header.RemoveField(DatHeader.DatFormatKey);

                    Parser.ParseInto(datdata,
                        datfile.CurrentPath,
                        indexId: int.MaxValue,
                        keep: true,
                        filterRunner: FilterRunner!);

                    // Perform additional processing steps
                    Extras!.ApplyExtras(datdata);
                    Extras!.ApplyExtrasDB(datdata);
                    Splitter!.ApplySplitting(datdata, useTags: true, filterRunner: FilterRunner);
                    datdata.ExecuteFilters(FilterRunner!);
                    Cleaner!.ApplyCleaning(datdata);
                    Remover!.ApplyRemovals(datdata);

                    // Set depot information
                    var inputDepot = Modifiers!.InputDepot;
                    datdata.Modifiers.InputDepot = inputDepot?.Clone() as DepotInformation;

                    // If we have overridden the header skipper, set it now
                    string? headerSkipper = Header!.GetStringFieldValue(Data.Models.Metadata.Header.HeaderKey);
                    if (!string.IsNullOrEmpty(headerSkipper))
                        datdata.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.HeaderKey, headerSkipper);

                    // If we have the depot flag, respect it
                    if (inputDepot?.IsActive ?? false)
                    {
                        Verification.VerifyDepot(datdata, Inputs);
                    }
                    else
                    {
                        // Loop through and add the inputs to check against
                        _logger.User("Processing files:\n");
                        foreach (string input in Inputs)
                        {
                            dfd.PopulateFromDir(datdata, input);
                        }

                        Verification.VerifyGeneric(datdata, hashOnly);
                        //Verification.VerifyGenericDB(datdata, hashOnly);
                    }

                    // Now write out if there are any items left
                    Writer.WriteStatsToConsole(datdata);
                    Writer.Write(datdata, OutputDir!);
                }
            }
            // Otherwise, process all DATs into the same output
            else
            {
                var watch = new InternalStopwatch("Populating internal DAT");

                // Add all of the input DATs into one huge internal DAT
                DatFile datdata = Parser.CreateDatFile();
                foreach (ParentablePath datfile in datfilePaths)
                {
                    datdata.Header.RemoveField(DatHeader.DatFormatKey);
                    Parser.ParseInto(datdata,
                        datfile.CurrentPath,
                        int.MaxValue,
                        keep: true,
                        filterRunner: FilterRunner);
                }

                // Perform additional processing steps
                Extras!.ApplyExtras(datdata);
                Extras!.ApplyExtrasDB(datdata);
                Splitter!.ApplySplitting(datdata, useTags: true, filterRunner: FilterRunner);
                datdata.ExecuteFilters(FilterRunner!);
                Cleaner!.ApplyCleaning(datdata);
                Remover!.ApplyRemovals(datdata);

                // Set depot information
                var inputDepot = Modifiers!.InputDepot;
                datdata.Modifiers.InputDepot = inputDepot?.Clone() as DepotInformation;

                // If we have overridden the header skipper, set it now
                string? headerSkipper = Header!.GetStringFieldValue(Data.Models.Metadata.Header.HeaderKey);
                if (!string.IsNullOrEmpty(headerSkipper))
                    datdata.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.HeaderKey, headerSkipper);

                watch.Stop();

                // If we have the depot flag, respect it
                if (inputDepot?.IsActive ?? false)
                {
                    Verification.VerifyDepot(datdata, Inputs);
                }
                else
                {
                    // Loop through and add the inputs to check against
                    _logger.User("Processing files:\n");
                    foreach (string input in Inputs)
                    {
                        dfd.PopulateFromDir(datdata, input);
                    }

                    Verification.VerifyGeneric(datdata, hashOnly);
                    //Verification.VerifyGenericDB(datdata, hashOnly);
                }

                // Now write out if there are any items left
                Writer.WriteStatsToConsole(datdata);
                Writer.Write(datdata, OutputDir!);
            }

            return true;
        }
    }
}
