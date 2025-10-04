using System.IO;
using SabreTools.DatFiles;
using SabreTools.DatTools;
using SabreTools.FileTypes;
using SabreTools.IO;
using SabreTools.IO.Extensions;
using SabreTools.IO.Logging;

namespace SabreTools.Features
{
    internal class Sort : BaseFeature
    {
        public const string DisplayName = "Sort";

        private static readonly string[] _flags = ["ss", "sort"];

        private const string _description = "Sort inputs by a set of DATs";

        private const string _longDescription = "This feature allows the user to quickly rebuild based on a supplied DAT file(s). By default all files will be rebuilt to uncompressed folders in the output directory.";

        public Sort()
            : base(DisplayName, _flags, _description, _longDescription)
        {
            RequiresInputs = false;

            // Common Features
            AddCommonFeatures();

            Add(DatListInput);
            Add(OutputDirStringInput);
            Add(DepotFlag);
            this[DepotFlag]!.Add(DepotDepthInt32Input);
            Add(DeleteFlag);
            Add(InverseFlag);
            Add(QuickFlag);
            Add(AaruFormatsAsFilesFlag);
            Add(ChdsAsFilesFlag);
            Add(AddDateFlag);
            Add(IndividualFlag);

            // Output Formats
            Add(Torrent7zipFlag);
            Add(TarFlag);
            Add(TorrentGzipFlag);
            this[TorrentGzipFlag]!.Add(RombaFlag);
            this[TorrentGzipFlag]![RombaFlag]!.Add(RombaDepthInt32Input);
            //AddFeature(SharedInputs.TorrentRarFlag);
            //AddFeature(SharedInputs.TorrentXzFlag);
            //this[SharedInputs.TorrentXzFlag]!.AddFeature(SharedInputs.RombaFlag);
            Add(TorrentZipFlag);

            Add(HeaderStringInput);
            AddInternalSplitFeatures();
            Add(UpdateDatFlag);
        }

        /// <inheritdoc/>
        public override bool Execute()
        {
            // If the base fails, just fail out
            if (!base.Execute())
                return false;

            // Get feature flags
            TreatAsFile asFile = GetTreatAsFile();
            bool date = GetBoolean(AddDateValue);
            bool delete = GetBoolean(DeleteValue);
            bool inverse = GetBoolean(InverseValue);
            bool quickScan = GetBoolean(QuickValue);
            bool updateDat = GetBoolean(UpdateDatValue);
            var outputFormat = GetOutputFormat();

            // Get the depots
            var inputDepot = Modifiers!.InputDepot;
            var outputDepot = Modifiers.OutputDepot;

            // If we have the romba flag
            if (outputDepot?.IsActive == true)
            {
                // Update TorrentGzip output
                if (outputFormat == OutputFormat.TorrentGzip)
                    outputFormat = OutputFormat.TorrentGzipRomba;

                // Update TorrentXz output
                else if (outputFormat == OutputFormat.TorrentXZ)
                    outputFormat = OutputFormat.TorrentXZRomba;
            }

            // Ensure the output directory
            OutputDir = OutputDir.Ensure();

            // Get a list of files from the input datfiles
            var datfiles = GetStringList(DatListValue);
            var datfilePaths = PathTool.GetFilesOnly(datfiles);

            // If we are in individual mode, process each DAT on their own, appending the DAT name to the output dir
            if (GetBoolean(IndividualValue))
            {
                foreach (ParentablePath datfile in datfilePaths)
                {
                    DatFile datdata = Parser.CreateDatFile();
                    datdata.Header.RemoveField(DatHeader.DatFormatKey);

                    Parser.ParseInto(datdata,
                        datfile.CurrentPath,
                        indexId: int.MaxValue,
                        keep: true,
                        filterRunner: FilterRunner);

                    // Skip if nothing was parsed
                    if (datdata.DatStatistics.TotalCount == 0) // datdata.ItemsDB.SortedKeys.Length == 0
                        continue;

                    // Set depot information
                    datdata.Modifiers.InputDepot = inputDepot?.Clone() as DepotInformation;
                    datdata.Modifiers.OutputDepot = outputDepot?.Clone() as DepotInformation;

                    // If we have overridden the header skipper, set it now
                    if (!string.IsNullOrEmpty(Header!.GetStringFieldValue(Data.Models.Metadata.Header.HeaderKey)))
                        datdata.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.HeaderKey, Header.GetStringFieldValue(Data.Models.Metadata.Header.HeaderKey));

                    // If we have the depot flag, respect it
                    bool success;
                    if (inputDepot?.IsActive ?? false)
                    {
                        success = Rebuilder.RebuildDepot(datdata,
                            Inputs,
                            Path.Combine(OutputDir!, datdata.Header.GetStringFieldValue(DatHeader.FileNameKey)!),
                            date,
                            delete,
                            inverse,
                            outputFormat);
                    }
                    else
                    {
                        success = Rebuilder.RebuildGeneric(datdata,
                            Inputs,
                            Path.Combine(OutputDir!, datdata.Header.GetStringFieldValue(DatHeader.FileNameKey)!),
                            quickScan,
                            date,
                            delete,
                            inverse,
                            outputFormat,
                            asFile);
                    }

                    // If we have a success and we're updating the DAT, write it out
                    if (success && updateDat)
                    {
                        datdata.Header.SetFieldValue<string?>(DatHeader.FileNameKey, $"fixDAT_{Header.GetStringFieldValue(DatHeader.FileNameKey)}");
                        datdata.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, $"fixDAT_{Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)}");
                        datdata.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, $"fixDAT_{Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)}");
                        datdata.ClearMarked();
                        Writer.Write(datdata, OutputDir);
                    }
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
                        indexId: int.MaxValue,
                        keep: true,
                        filterRunner: FilterRunner);
                }

                // Set depot information
                datdata.Modifiers.InputDepot = inputDepot?.Clone() as DepotInformation;
                datdata.Modifiers.OutputDepot = outputDepot?.Clone() as DepotInformation;

                // If we have overridden the header skipper, set it now
                string? headerSkpper = Header!.GetStringFieldValue(Data.Models.Metadata.Header.HeaderKey);
                if (!string.IsNullOrEmpty(headerSkpper))
                    datdata.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.HeaderKey, headerSkpper);

                watch.Stop();

                // If we have the depot flag, respect it
                bool success;
                if (inputDepot?.IsActive ?? false)
                {
                    success = Rebuilder.RebuildDepot(datdata,
                        Inputs,
                        OutputDir!,
                        date,
                        delete,
                        inverse,
                        outputFormat);
                }
                else
                {
                    success = Rebuilder.RebuildGeneric(datdata,
                        Inputs,
                        OutputDir!,
                        quickScan,
                        date,
                        delete,
                        inverse,
                        outputFormat,
                        asFile);
                }

                // If we have a success and we're updating the DAT, write it out
                if (success && updateDat)
                {
                    datdata.Header.SetFieldValue<string?>(DatHeader.FileNameKey,
                        $"fixDAT_{Header.GetStringFieldValue(DatHeader.FileNameKey)}");
                    datdata.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey,
                        $"fixDAT_{Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)}");
                    datdata.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey,
                        $"fixDAT_{Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)}");

                    datdata.ClearMarked();
                    Writer.Write(datdata, OutputDir);
                }
            }

            return true;
        }
    }
}
