using System;
using System.IO;
using SabreTools.DatFiles;
using SabreTools.DatTools;

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
            Add(IncludeCrcFlag);
            Add(IncludeMd2Flag);
            Add(IncludeMd4Flag);
            Add(IncludeMd5Flag);
            Add(IncludeRipeMD128Flag);
            Add(IncludeRipeMD160Flag);
            Add(IncludeSha1Flag);
            Add(IncludeSha256Flag);
            Add(IncludeSha384Flag);
            Add(IncludeSha512Flag);
            Add(IncludeSpamSumFlag);

            Add(NoAutomaticDateFlag);
            Add(AaruFormatsAsFilesFlag);
            Add(ArchivesAsFilesFlag);
            Add(ChdsAsFilesFlag);
            Add(OutputTypeListInput);
            this[OutputTypeListInput]!.Add(DeprecatedFlag);
            Add(RombaFlag);
            this[RombaFlag]!.Add(RombaDepthInt32Input);
            Add(SkipArchivesFlag);
            Add(SkipFilesFlag);
            AddHeaderFeatures();
            Add(AddBlankFilesFlag);
            Add(AddDateFlag);
            Add(HeaderStringInput);
            Add(ExtraIniListInput);
            AddFilteringFeatures();
            Add(OutputDirStringInput);
        }

        /// <inheritdoc/>
        public override bool ProcessFeatures()
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures())
                return false;

            // Get feature flags
            bool addBlankFiles = GetBoolean(AddBlankFilesValue);
            bool addFileDates = GetBoolean(AddDateValue);
            TreatAsFile treatAsFile = GetTreatAsFile();
            bool noAutomaticDate = GetBoolean(NoAutomaticDateValue);
            var includeInScan = GetIncludeInScan();
            var skipFileType = GetSkipFileType();
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
