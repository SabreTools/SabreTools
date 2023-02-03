using System.Collections.Generic;
using System.IO;

using SabreTools.DatFiles;
using SabreTools.DatTools;
using SabreTools.FileTypes;
using SabreTools.Help;
using SabreTools.IO;
using SabreTools.Logging;

namespace SabreTools.Features
{
    internal class Sort : BaseFeature
    {
        public const string Value = "Sort";

        public Sort()
        {
            Name = Value;
            Flags = new List<string>() { "ss", "sort" };
            Description = "Sort inputs by a set of DATs";
            _featureType = ParameterType.Flag;
            LongDescription = "This feature allows the user to quickly rebuild based on a supplied DAT file(s). By default all files will be rebuilt to uncompressed folders in the output directory.";
            Features = new Dictionary<string, Feature>();

            // Common Features
            AddCommonFeatures();

            AddFeature(DatListInput);
            AddFeature(OutputDirStringInput);
            AddFeature(DepotFlag);
            this[DepotFlag].AddFeature(DepotDepthInt32Input);
            AddFeature(DeleteFlag);
            AddFeature(InverseFlag);
            AddFeature(QuickFlag);
            AddFeature(AaruFormatsAsFilesFlag);
            AddFeature(ChdsAsFilesFlag);
            AddFeature(AddDateFlag);
            AddFeature(IndividualFlag);

            // Output Formats
            AddFeature(Torrent7zipFlag);
            AddFeature(TarFlag);
            AddFeature(TorrentGzipFlag);
            this[TorrentGzipFlag].AddFeature(RombaFlag);
            this[TorrentGzipFlag][RombaFlag].AddFeature(RombaDepthInt32Input);
            //AddFeature(SharedInputs.TorrentLrzipFlag);
            //AddFeature(SharedInputs.TorrentLz4Flag);
            //AddFeature(SharedInputs.TorrentRarFlag);
            //AddFeature(SharedInputs.TorrentXzFlag);
            //this[SharedInputs.TorrentXzFlag].AddFeature(SharedInputs.RombaFlag);
            AddFeature(TorrentZipFlag);
            AddFeature(BaseReplaceFlag); // change name of folder or zip, do not change name inside it
            AddFeature(ArchivesAsFilesFlag); // write out symbolic links to files
            AddFeature(SymlinkStringInput); // Path to add to symbolic link in linux
            //AddFeature(SharedInputs.TorrentZpaqFlag);
            //AddFeature(SharedInputs.TorrentZstdFlag);

            AddFeature(HeaderStringInput);
            AddInternalSplitFeatures();
            AddFeature(UpdateDatFlag);
        }

        public override bool ProcessFeatures(Dictionary<string, Feature> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
                return false;

            // Get feature flags
            TreatAsFile asFiles = GetTreatAsFiles(features);
            bool date = GetBoolean(features, AddDateValue);
            bool delete = GetBoolean(features, DeleteValue);
            bool inverse = GetBoolean(features, InverseValue);
            bool quickScan = GetBoolean(features, QuickValue);
            bool updateDat = GetBoolean(features, UpdateDatValue);
            bool baseReplace = GetBoolean(features, BaseReplaceValue);
            bool archivesAsFiles = GetBoolean(features, ArchivesAsFilesValue);
            string symlinkDir = GetString(features, SymlinkDirStringValue);
            var outputFormat = GetOutputFormat(features);

            // If we have the romba flag
            if (Header.OutputDepot?.IsActive == true)
            {
                // Update TorrentGzip output
                if (outputFormat == OutputFormat.TorrentGzip)
                    outputFormat = OutputFormat.TorrentGzipRomba;

                // Update TorrentXz output
                else if (outputFormat == OutputFormat.TorrentXZ)
                    outputFormat = OutputFormat.TorrentXZRomba;
            }

            // Get a list of files from the input datfiles
            var datfiles = GetList(features, DatListValue);
            var datfilePaths = PathTool.GetFilesOnly(datfiles);

            // If we are in individual mode, process each DAT on their own, appending the DAT name to the output dir
            if (GetBoolean(features, IndividualValue))
            {
                foreach (ParentablePath datfile in datfilePaths)
                {
                    DatFile datdata = DatFile.Create();
                    Parser.ParseInto(datdata, datfile, int.MaxValue, keep: true);

                    // Set depot information
                    datdata.Header.InputDepot = Header.InputDepot?.Clone() as DepotInformation;
                    datdata.Header.OutputDepot = Header.OutputDepot?.Clone() as DepotInformation;

                    // If we have overridden the header skipper, set it now
                    if (!string.IsNullOrEmpty(Header.HeaderSkipper))
                        datdata.Header.HeaderSkipper = Header.HeaderSkipper;

                    // If we have the depot flag, respect it
                    bool success;
                    if (Header.InputDepot?.IsActive ?? false)
                        success = Rebuilder.RebuildDepot(datdata, Inputs, Path.Combine(OutputDir, datdata.Header.FileName), date, delete, inverse, baseReplace, archivesAsFiles, symlinkDir, outputFormat);
                    else
                        success = Rebuilder.RebuildGeneric(datdata, Inputs, Path.Combine(OutputDir, datdata.Header.FileName), quickScan, date, delete, inverse, baseReplace, archivesAsFiles, symlinkDir, outputFormat, asFiles);

                    // If we have a success and we're updating the DAT, write it out
                    if (success && updateDat)
                    {
                        datdata.Header.FileName = $"fixDAT_{Header.FileName}";
                        datdata.Header.Name = $"fixDAT_{Header.Name}";
                        datdata.Header.Description = $"fixDAT_{Header.Description}";
                        datdata.Items.ClearMarked();
                        Writer.Write(datdata, OutputDir);
                    }
                }
            }

            // Otherwise, process all DATs into the same output
            else
            {
                InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

                // Add all of the input DATs into one huge internal DAT
                DatFile datdata = DatFile.Create();
                foreach (ParentablePath datfile in datfilePaths)
                {
                    Parser.ParseInto(datdata, datfile, int.MaxValue, keep: true);
                }

                // Set depot information
                datdata.Header.InputDepot = Header.InputDepot?.Clone() as DepotInformation;
                datdata.Header.OutputDepot = Header.OutputDepot?.Clone() as DepotInformation;

                // If we have overridden the header skipper, set it now
                if (!string.IsNullOrEmpty(Header.HeaderSkipper))
                    datdata.Header.HeaderSkipper = Header.HeaderSkipper;

                watch.Stop();

                // If we have the depot flag, respect it
                bool success;
                if (Header.InputDepot?.IsActive ?? false)
                    success = Rebuilder.RebuildDepot(datdata, Inputs, OutputDir, date, delete, inverse, baseReplace, archivesAsFiles, symlinkDir, outputFormat);
                else
                    success = Rebuilder.RebuildGeneric(datdata, Inputs, OutputDir, quickScan, date, delete, inverse, baseReplace, archivesAsFiles, symlinkDir, outputFormat, asFiles);

                // If we have a success and we're updating the DAT, write it out
                if (success && updateDat)
                {
                    datdata.Header.FileName = $"fixDAT_{Header.FileName}";
                    datdata.Header.Name = $"fixDAT_{Header.Name}";
                    datdata.Header.Description = $"fixDAT_{Header.Description}";
                    datdata.Items.ClearMarked();
                    Writer.Write(datdata, OutputDir);
                }
            }

            return true;
        }
    }
}
