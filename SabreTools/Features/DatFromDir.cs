using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Library.DatFiles;
using SabreTools.Library.Help;

namespace SabreTools.Features
{
    internal class DatFromDir : BaseFeature
    {
        public const string Value = "DATFromDir";

        public DatFromDir()
        {
            Name = Value;
            Flags = new List<string>() { "-d", "--d2d", "--dfd" };
            Description = "Create DAT(s) from an input directory";
            _featureType = FeatureType.Flag;
            LongDescription = "Create a DAT file from an input directory or set of files. By default, this will output a DAT named based on the input directory and the current date. It will also treat all archives as possible games and add all three hashes (CRC, MD5, SHA-1) for each file.";
            Features = new Dictionary<string, Feature>();

            // Hash Features
            AddFeature(SkipMd5Flag);
#if NET_FRAMEWORK
            AddFeature(SkipRipeMd160Flag);
#endif
            AddFeature(SkipSha1Flag);
            AddFeature(SkipSha256Flag);
            AddFeature(SkipSha384Flag);
            AddFeature(SkipSha512Flag);

            AddFeature(NoAutomaticDateFlag);
            AddFeature(AaruFormatsAsFilesFlag);
            AddFeature(ArchivesAsFilesFlag);
            AddFeature(ChdsAsFilesFlag);
            AddFeature(OutputTypeListInput);
            this[OutputTypeListInput].AddFeature(DeprecatedFlag);
            AddFeature(RombaFlag);
            this[RombaFlag].AddFeature(RombaDepthInt32Input);
            AddFeature(SkipArchivesFlag);
            AddFeature(SkipFilesFlag);
            AddHeaderFeatures();
            AddFeature(AddBlankFilesFlag);
            AddFeature(AddDateFlag);
            AddFeature(CopyFilesFlag);
            AddFeature(HeaderStringInput);
            AddFeature(ExtraIniListInput);
            AddFilteringFeatures();
            AddFeature(TempStringInput);
            AddFeature(OutputDirStringInput);
            AddFeature(ThreadsInt32Input);
        }

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            base.ProcessFeatures(features);

            // Get feature flags
            bool addBlankFiles = GetBoolean(features, AddBlankFilesValue);
            bool addFileDates = GetBoolean(features, AddDateValue);
            TreatAsFiles asFiles = GetTreatAsFiles(features);
            bool copyFiles = GetBoolean(features, CopyFilesValue);
            bool noAutomaticDate = GetBoolean(features, NoAutomaticDateValue);
            var omitFromScan = GetOmitFromScan(features);
            var skipFileType = GetSkipFileType(features);

            // Create a new DATFromDir object and process the inputs
            DatFile basedat = DatFile.Create(Header);
            basedat.Header.Date = DateTime.Now.ToString("yyyy-MM-dd");

            // For each input directory, create a DAT
            foreach (string path in Inputs)
            {
                if (Directory.Exists(path) || File.Exists(path))
                {
                    // Clone the base Dat for information
                    DatFile datdata = DatFile.Create(basedat.Header);

                    string basePath = Path.GetFullPath(path);
                    bool success = datdata.PopulateFromDir(
                        basePath,
                        omitFromScan,
                        noAutomaticDate,
                        asFiles,
                        skipFileType,
                        addBlankFiles,
                        addFileDates,
                        copyFiles,
                        Extras,
                        Filter);

                    if (success)
                    {
                        datdata.Write(OutputDir);
                    }
                    else
                    {
                        Console.WriteLine();
                        OutputRecursive(0);
                    }
                }
            }
        }
    }
}
