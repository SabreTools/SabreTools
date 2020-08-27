using System.Collections.Generic;
using System.IO;

using SabreTools.Library.DatFiles;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Help;
using SabreTools.Library.IO;
using SabreTools.Library.Tools;

namespace SabreTools.Features
{
    internal class Sort : BaseFeature
    {
        public const string Value = "Sort";

        public Sort()
        {
            Name = Value;
            Flags = new List<string>() { "-ss", "--sort" };
            Description = "Sort inputs by a set of DATs";
            _featureType = FeatureType.Flag;
            LongDescription = "This feature allows the user to quickly rebuild based on a supplied DAT file(s). By default all files will be rebuilt to uncompressed folders in the output directory.";
            Features = new Dictionary<string, Feature>();

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
            //AddFeature(SharedInputs.TorrentZpaqFlag);
            //AddFeature(SharedInputs.TorrentZstdFlag);

            AddFeature(HeaderStringInput);
            AddInternalSplitFeatures();
            AddFeature(UpdateDatFlag);
            AddFeature(ThreadsInt32Input);
        }

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            base.ProcessFeatures(features);

            // Get feature flags
            TreatAsFiles asFiles = GetTreatAsFiles(features);
            bool date = GetBoolean(features, AddDateValue);
            bool delete = GetBoolean(features, DeleteValue);
            bool inverse = GetBoolean(features, InverseValue);
            bool quickScan = GetBoolean(features, QuickValue);
            bool updateDat = GetBoolean(features, UpdateDatValue);
            var outputFormat = GetOutputFormat(features);

            // If we have TorrentGzip output and the romba flag, update
            if ((Header.OutputDepot?.IsActive ?? false) && outputFormat == OutputFormat.TorrentGzip)
                outputFormat = OutputFormat.TorrentGzipRomba;

            // If we hae TorrentXZ output and the romba flag, update
            if ((Header.OutputDepot?.IsActive ?? false) && outputFormat == OutputFormat.TorrentXZ)
                outputFormat = OutputFormat.TorrentXZRomba;

            // Get a list of files from the input datfiles
            var datfiles = GetList(features, DatListValue);
            var datfilePaths = DirectoryExtensions.GetFilesOnly(datfiles);

            // If we are in individual mode, process each DAT on their own, appending the DAT name to the output dir
            if (GetBoolean(features, IndividualValue))
            {
                foreach (ParentablePath datfile in datfilePaths)
                {
                    DatFile datdata = DatFile.Create();
                    datdata.Parse(datfile, 99, keep: true);

                    // Set depot information
                    datdata.Header.InputDepot = Header.InputDepot.Clone() as DepotInformation;
                    datdata.Header.OutputDepot = Header.OutputDepot.Clone() as DepotInformation;

                    // If we have overridden the header skipper, set it now
                    if (!string.IsNullOrEmpty(Header.HeaderSkipper))
                        datdata.Header.HeaderSkipper = Header.HeaderSkipper;

                    // If we have the depot flag, respect it
                    if (Header.InputDepot?.IsActive ?? false)
                        datdata.RebuildDepot(Inputs, Path.Combine(OutputDir, datdata.Header.FileName), date, delete, inverse, outputFormat, updateDat);
                    else
                        datdata.RebuildGeneric(Inputs, Path.Combine(OutputDir, datdata.Header.FileName), quickScan, date, delete, inverse, outputFormat, updateDat, asFiles);
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
                    datdata.Parse(datfile, 99, keep: true);
                }

                // Set depot information
                datdata.Header.InputDepot = Header.InputDepot.Clone() as DepotInformation;
                datdata.Header.OutputDepot = Header.OutputDepot.Clone() as DepotInformation;

                // If we have overridden the header skipper, set it now
                if (!string.IsNullOrEmpty(Header.HeaderSkipper))
                    datdata.Header.HeaderSkipper = Header.HeaderSkipper;

                watch.Stop();

                // If we have the depot flag, respect it
                if (Header.InputDepot?.IsActive ?? false)
                    datdata.RebuildDepot(Inputs, OutputDir, date, delete, inverse, outputFormat, updateDat);
                else
                    datdata.RebuildGeneric(Inputs, OutputDir, quickScan, date, delete, inverse, outputFormat, updateDat, asFiles);
            }
        }
    }
}
