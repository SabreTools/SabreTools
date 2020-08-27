using System.Collections.Generic;

using SabreTools.Library.DatFiles;
using SabreTools.Library.Help;
using SabreTools.Library.IO;
using SabreTools.Library.Tools;

namespace SabreTools.Features
{
    internal class Verify : BaseFeature
    {
        public const string Value = "Verify";

        public Verify()
        {
            Name = Value;
            Flags = new List<string>() { "-ve", "--verify" };
            Description = "Verify a folder against DATs";
            _featureType = FeatureType.Flag;
            LongDescription = "When used, this will use an input DAT or set of DATs to blindly check against an input folder. The base of the folder is considered the base for the combined DATs and games are either the directories or archives within. This will only do a direct verification of the items within and will create a fixdat afterwards for missing files.";
            Features = new Dictionary<string, Feature>();

            AddFeature(DatListInput);
            AddFeature(DepotFlag);
            this[DepotFlag].AddFeature(DepotDepthInt32Input);
            AddFeature(TempStringInput);
            AddFeature(OutputDirStringInput);
            AddFeature(HashOnlyFlag);
            AddFeature(QuickFlag);
            AddFeature(HeaderStringInput);
            AddFeature(AaruFormatsAsFilesFlag);
            AddFeature(ChdsAsFilesFlag);
            AddFeature(IndividualFlag);
            AddInternalSplitFeatures();
            AddFeature(ExtraIniListInput);
            AddFilteringFeatures();
        }

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            base.ProcessFeatures(features);

            // Get a list of files from the input datfiles
            var datfiles = GetList(features, DatListValue);
            var datfilePaths = DirectoryExtensions.GetFilesOnly(datfiles);

            // Get feature flags
            TreatAsFiles asFiles = GetTreatAsFiles(features);
            bool hashOnly = GetBoolean(features, HashOnlyValue);
            bool quickScan = GetBoolean(features, QuickValue);

            // If we are in individual mode, process each DAT on their own
            if (GetBoolean(features, IndividualValue))
            {
                foreach (ParentablePath datfile in datfilePaths)
                {
                    DatFile datdata = DatFile.Create();
                    datdata.Parse(datfile, 99, keep: true);
                    datdata.ApplyExtras(Extras);
                    datdata.ApplyFilter(Filter, true);

                    // Set depot information
                    datdata.Header.InputDepot = Header.InputDepot.Clone() as DepotInformation;

                    // If we have overridden the header skipper, set it now
                    if (!string.IsNullOrEmpty(Header.HeaderSkipper))
                        datdata.Header.HeaderSkipper = Header.HeaderSkipper;

                    // If we have the depot flag, respect it
                    if (Header.InputDepot?.IsActive ?? false)
                        datdata.VerifyDepot(Inputs, OutputDir);
                    else
                        datdata.VerifyGeneric(Inputs, OutputDir, hashOnly, quickScan, asFiles, Extras, Filter);
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
                    datdata.ApplyExtras(Extras);
                    datdata.ApplyFilter(Filter, true);
                }

                // Set depot information
                datdata.Header.InputDepot = Header.InputDepot.Clone() as DepotInformation;

                // If we have overridden the header skipper, set it now
                if (!string.IsNullOrEmpty(Header.HeaderSkipper))
                    datdata.Header.HeaderSkipper = Header.HeaderSkipper;

                watch.Stop();

                // If we have the depot flag, respect it
                if (Header.InputDepot?.IsActive ?? false)
                    datdata.VerifyDepot(Inputs, OutputDir);
                else
                    datdata.VerifyGeneric(Inputs, OutputDir, hashOnly, quickScan, asFiles, Extras, Filter);
            }
        }
    }
}
