﻿using System.Collections.Generic;
using System.IO;
using SabreTools.DatTools;
using SabreTools.Help;

namespace SabreTools.Features
{
    internal class Stats : BaseFeature
    {
        public const string Value = "Stats";

        public Stats()
        {
            Name = Value;
            Flags.AddRange(["st", "stats"]);
            Description = "Get statistics on all input DATs";
            _featureType = ParameterType.Flag;
            LongDescription = @"This will output by default the combined statistics for all input DAT files.

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

            // Common Features
            AddCommonFeatures();

            AddFeature(ReportTypeListInput);
            AddFeature(FilenameStringInput);
            AddFeature(OutputDirStringInput);
            AddFeature(BaddumpColumnFlag);
            AddFeature(NodumpColumnFlag);
            AddFeature(IndividualFlag);
        }

        public override bool ProcessFeatures(Dictionary<string, Feature?> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
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

            var statistics = Statistics.CalculateStatistics(Inputs, GetBoolean(features, IndividualValue));
            Statistics.Write(
                statistics,
                filename,
                OutputDir,
                GetBoolean(features, BaddumpColumnValue),
                GetBoolean(features, NodumpColumnValue),
                GetStatReportFormat(features));

            return true;
        }
    }
}
