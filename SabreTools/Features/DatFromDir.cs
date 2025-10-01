using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.DatFiles;
using SabreTools.DatTools;
using SabreTools.Help.Inputs;

namespace SabreTools.Features
{
    internal class DatFromDir : BaseFeature
    {
        public const string DisplayName = "DATFromDir";

        private static readonly string[] _flags = ["d", "d2d", "dfd"];

        private const string _description = "Create DAT(s) from an input directory";

        private const string _longDescription = "Create a DAT file from an input directory or set of files. By default, this will output a DAT named based on the input directory and the current date. It will also treat all archives as possible games and add all three hashes (CRC, MD5, SHA-1) for each file.";

        public DatFromDir()
            : base(DisplayName, _flags, _description, _longDescription)
        {
            // Common Features
            AddCommonFeatures();

            // Hash Features
            AddFeature(IncludeCrcFlag);
            AddFeature(IncludeMd2Flag);
            AddFeature(IncludeMd4Flag);
            AddFeature(IncludeMd5Flag);
            AddFeature(IncludeRipeMD128Flag);
            AddFeature(IncludeRipeMD160Flag);
            AddFeature(IncludeSha1Flag);
            AddFeature(IncludeSha256Flag);
            AddFeature(IncludeSha384Flag);
            AddFeature(IncludeSha512Flag);
            AddFeature(IncludeSpamSumFlag);

            AddFeature(NoAutomaticDateFlag);
            AddFeature(AaruFormatsAsFilesFlag);
            AddFeature(ArchivesAsFilesFlag);
            AddFeature(ChdsAsFilesFlag);
            AddFeature(OutputTypeListInput);
            this[OutputTypeListInput]!.AddFeature(DeprecatedFlag);
            AddFeature(RombaFlag);
            this[RombaFlag]!.AddFeature(RombaDepthInt32Input);
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

        public override bool ProcessFeatures(Dictionary<string, UserInput?> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
                return false;

            // Get feature flags
            bool addBlankFiles = GetBoolean(features, AddBlankFilesValue);
            bool addFileDates = GetBoolean(features, AddDateValue);
            TreatAsFile treatAsFile = GetTreatAsFile(features);
            bool noAutomaticDate = GetBoolean(features, NoAutomaticDateValue);
            var includeInScan = GetIncludeInScan(features);
            var skipFileType = GetSkipFileType(features);
            var dfd = new DatTools.DatFromDir(includeInScan, skipFileType, treatAsFile, addBlankFiles);

            // Apply the specialized field removals to the cleaner
            if (!addFileDates)
                Remover!.PopulateExclusionsFromList(["DatItem.Date"]);

            // Create a new DATFromDir object and process the inputs
            DatFile basedat = Parser.CreateDatFile(Header!, Modifiers!);
            basedat.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DateKey, DateTime.Now.ToString("yyyy-MM-dd"));

            // Update the cleaner based on certain flags
            if (addBlankFiles)
                Cleaner!.KeepEmptyGames = true;

            // For each input directory, create a DAT
            foreach (string path in Inputs)
            {
                // TODO: Should this be logged?
                if (!Directory.Exists(path) && !File.Exists(path))
                    continue;

                // Clone the base Dat for information
                DatFile datdata = Parser.CreateDatFile(basedat.Header, basedat.Modifiers);

                // Get the base path and fill the header, if needed
                string basePath = Path.GetFullPath(path);
                datdata.FillHeaderFromPath(basePath, noAutomaticDate);

                // Now populate from the path
                bool success = dfd.PopulateFromDir(datdata, basePath);
                if (success)
                {
                    // Perform additional processing steps
                    Extras!.ApplyExtras(datdata);
                    Extras!.ApplyExtrasDB(datdata);
                    Splitter!.ApplySplitting(datdata, useTags: false, filterRunner: FilterRunner);
                    datdata.ExecuteFilters(FilterRunner!);
                    Cleaner!.ApplyCleaning(datdata);
                    Remover!.ApplyRemovals(datdata);

                    // Write out the file
                    Writer.Write(datdata, OutputDir);
                }
                else
                {
                    Console.WriteLine();
                    OutputRecursive(0);
                }
            }

            return true;
        }
    }
}
