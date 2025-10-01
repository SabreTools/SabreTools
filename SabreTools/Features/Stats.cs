using System.IO;
using SabreTools.DatTools;

namespace SabreTools.Features
{
    internal class Stats : BaseFeature
    {
        public const string DisplayName = "Stats";

        private static readonly string[] _flags = ["st", "stats"];

        private const string _description = "Get statistics on all input DATs";

        private const string _longDescription = @"This will output by default the combined statistics for all input DAT files.

The stats that are outputted are as follows:
- Total uncompressed size
- Number of games found
- Number of roms found
- Number of disks found
- Items that include a CRC
- Items that include a MD5
- Items that include a SHA-1
- Items that include a SHA-256
- Items that include a SHA-384
- Items that include a SHA-512
- Items with Nodump status";

        public Stats()
            : base(DisplayName, _flags, _description, _longDescription)
        {
            // Common Features
            AddCommonFeatures();

            AddFeature(ReportTypeListInput);
            AddFeature(FilenameStringInput);
            AddFeature(OutputDirStringInput);
            AddFeature(BaddumpColumnFlag);
            AddFeature(NodumpColumnFlag);
            AddFeature(IndividualFlag);
        }

        /// <inheritdoc/>
        public override bool ProcessFeatures()
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures())
                return false;

            string filename = Header!.GetStringFieldValue(DatFiles.DatHeader.FileNameKey)!;
            if (Path.GetFileName(filename) != filename)
            {
                if (string.IsNullOrEmpty(OutputDir))
                    OutputDir = Path.GetDirectoryName(filename);
                else
                    OutputDir = Path.Combine(OutputDir, Path.GetDirectoryName(filename)!);

                filename = Path.GetFileName(filename);
            }

            var statistics = Statistics.CalculateStatistics(Inputs, GetBoolean(IndividualValue));
            Statistics.Write(
                statistics,
                filename,
                OutputDir,
                GetBoolean(BaddumpColumnValue),
                GetBoolean(NodumpColumnValue),
                GetStatReportFormat());

            return true;
        }
    }
}
