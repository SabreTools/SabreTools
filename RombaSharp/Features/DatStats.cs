using System.Collections.Generic;
using System.IO;

using SabreTools.DatTools;
using SabreTools.Help;
using SabreTools.Reports;

namespace RombaSharp.Features
{
    internal class DatStats : BaseFeature
    {
        public const string Value = "DatStats";

        public DatStats()
        {
            Name = Value;
            Flags = new List<string>() { "datstats" };
            Description = "Prints dat stats.";
            _featureType = ParameterType.Flag;
            LongDescription = "Print dat stats.";
            Features = new Dictionary<string, Feature>();

            // Common Features
            AddCommonFeatures();
        }

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            base.ProcessFeatures(features);

            // If we have no inputs listed, we want to use datroot
            if (Inputs == null || Inputs.Count == 0)
                Inputs = new List<string> { Path.GetFullPath(_dats) };

            // Now output the stats for all inputs
            var statistics = Statistics.CalculateStatistics(Inputs, single: true);
            Statistics.Write(
                statistics,
                "rombasharp-datstats",
                outDir: null,
                baddumpCol: true,
                nodumpCol: true,
                StatReportFormat.Textfile);
        }
    }
}
