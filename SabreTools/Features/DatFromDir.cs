using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.DatFiles;
using SabreTools.DatTools;
using SabreTools.FileTypes;
using SabreTools.Help;

namespace SabreTools.Features
{
    internal class DatFromDir : BaseFeature
    {
        public const string Value = "DATFromDir";

        public DatFromDir()
        {
            Name = Value;
            Flags = new List<string>() { "d", "d2d", "dfd" };
            Description = "Create DAT(s) from an input directory";
            _featureType = ParameterType.Flag;
            LongDescription = "Create a DAT file from an input directory or set of files. By default, this will output a DAT named based on the input directory and the current date. It will also treat all archives as possible games and add all three hashes (CRC, MD5, SHA-1) for each file.";
            Features = new Dictionary<string, Help.Feature>();

            // Common Features
            AddCommonFeatures();

            // Hash Features
            AddFeature(IncludeCrcFlag);
            AddFeature(IncludeMd5Flag);
            AddFeature(IncludeSha1Flag);
            AddFeature(IncludeSha256Flag);
            AddFeature(IncludeSha384Flag);
            AddFeature(IncludeSha512Flag);
            AddFeature(IncludeSpamSumFlag);

            AddFeature(NoAutomaticDateFlag);
            AddFeature(AaruFormatsAsFilesFlag);
            AddFeature(ArchivesAsFilesFlag);
            AddFeature(ChdsAsFilesFlag);
            AddFeature(AddAsFilesFlag);
            AddFeature(OutputTypeListInput);
            this[OutputTypeListInput].AddFeature(DeprecatedFlag);
            AddFeature(RombaFlag);
            this[RombaFlag].AddFeature(RombaDepthInt32Input);
            AddFeature(SkipArchivesFlag);
            AddFeature(SkipFilesFlag);
            AddHeaderFeatures();
            AddFeature(AddBlankFilesFlag);
            AddFeature(AddDateFlag);
            AddFeature(HeaderStringInput);
            AddFeature(ExtraIniListInput);
            AddFilteringFeatures();
            AddFeature(OutputDirStringInput);
        }

        public override bool ProcessFeatures(Dictionary<string, Help.Feature> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
                return false;

            // Get feature flags
            bool addBlankFiles = GetBoolean(features, AddBlankFilesValue);
            bool addFileDates = GetBoolean(features, AddDateValue);
            TreatAsFile asFiles = GetTreatAsFiles(features);
            bool noAutomaticDate = GetBoolean(features, NoAutomaticDateValue);
            bool addAsFiles = GetBoolean(features, AddAsFilesValue);
            var includeInScan = GetIncludeInScan(features);
            var skipFileType = GetSkipFileType(features);

            // Apply the specialized field removals to the cleaner
            if (!addFileDates)
                Remover.PopulateExclusionsFromList(new List<string> { "DatItem.Date" });

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

                    // Get the base path and fill the header, if needed
                    string basePath = Path.GetFullPath(path);
                    datdata.FillHeaderFromPath(basePath, noAutomaticDate);

                    // Now populate from the path
                    bool success = DatTools.DatFromDir.PopulateFromDir(
                        datdata,
                        basePath,
                        asFiles,
                        addAsFiles,
                        skipFileType,
                        addBlankFiles,
                        hashes: includeInScan);

                    if (success)
                    {
                        // Perform additional processing steps
                        Extras.ApplyExtras(datdata);
                        Splitter.ApplySplitting(datdata, false);
                        Filter.ApplyFilters(datdata);
                        Cleaner.ApplyCleaning(datdata);
                        Remover.ApplyRemovals(datdata);

                        // Write out the file
                        Writer.Write(datdata, OutputDir);
                    }
                    else
                    {
                        Console.WriteLine();
                        OutputRecursive(0);
                    }
                }
            }

            return true;
        }
    }
}
