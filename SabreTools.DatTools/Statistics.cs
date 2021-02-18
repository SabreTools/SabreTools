using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.IO;
using SabreTools.Logging;
using SabreTools.Reports;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Helper methods for dealing with DatFile statistics
    /// </summary>
    
    public class Statistics
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger logger = new Logger();

        #endregion

        /// <summary>
        /// Output the stats for a list of input dats as files in a human-readable format
        /// </summary>
        /// <param name="inputs">List of input files and folders</param>
        /// <param name="reportName">Name of the output file</param>
        /// <param name="single">True if single DAT stats are output, false otherwise</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        /// <param name="statDatFormat" > Set the statistics output format to use</param>
        public static void OutputStats(
            List<string> inputs,
            string reportName,
            string outDir,
            bool single,
            bool baddumpCol,
            bool nodumpCol,
            StatReportFormat statDatFormat)
        {
            // If there's no output format, set the default
            if (statDatFormat == StatReportFormat.None)
                statDatFormat = StatReportFormat.Textfile;

            // Get the proper output file name
            if (string.IsNullOrWhiteSpace(reportName))
                reportName = "report";

            // Get the proper output directory name
            outDir = outDir.Ensure();

            // Get the dictionary of desired output report names
            Dictionary<StatReportFormat, string> outputs = CreateOutStatsNames(outDir, statDatFormat, reportName);

            // Make sure we have all files and then order them
            List<ParentablePath> files = PathTool.GetFilesOnly(inputs);
            files = files
                .OrderBy(i => Path.GetDirectoryName(i.CurrentPath))
                .ThenBy(i => Path.GetFileName(i.CurrentPath))
                .ToList();

            // Get all of the writers that we need
            List<BaseReport> reports = outputs.Select(kvp => BaseReport.Create(kvp.Key, kvp.Value, baddumpCol, nodumpCol)).ToList();

            // Write the header, if any
            reports.ForEach(report => report.WriteHeader());

            // Init all total variables
            ItemDictionary totalStats = new ItemDictionary();

            // Init directory-level variables
            string lastdir = null;
            string basepath = null;
            ItemDictionary dirStats = new ItemDictionary();

            // Now process each of the input files
            foreach (ParentablePath file in files)
            {
                // Get the directory for the current file
                string thisdir = Path.GetDirectoryName(file.CurrentPath);
                basepath = Path.GetDirectoryName(Path.GetDirectoryName(file.CurrentPath));

                // If we don't have the first file and the directory has changed, show the previous directory stats and reset
                if (lastdir != null && thisdir != lastdir)
                {
                    // Output separator if needed
                    reports.ForEach(report => report.WriteMidSeparator());

                    DatFile lastdirdat = DatFile.Create();

                    reports.ForEach(report => report.ReplaceStatistics(dirStats, $"DIR: {WebUtility.HtmlEncode(lastdir)}", dirStats.GameCount, true));
                    reports.ForEach(report => report.Write());

                    // Write the mid-footer, if any
                    reports.ForEach(report => report.WriteFooterSeparator());

                    // Write the header, if any
                    reports.ForEach(report => report.WriteMidHeader());

                    // Reset the directory stats
                    dirStats.ResetStatistics();
                }

                logger.Verbose($"Beginning stat collection for '{file.CurrentPath}'");
                List<string> games = new List<string>();
                DatFile datdata = Parser.CreateAndParse(file.CurrentPath, statsOnly: true);
                datdata.Items.BucketBy(ItemKey.Machine, DedupeType.None, norename: true);

                // Output single DAT stats (if asked)
                logger.User($"Adding stats for file '{file.CurrentPath}'\n");
                if (single)
                {
                    reports.ForEach(report => report.ReplaceStatistics(datdata.Items, datdata.Header.FileName, datdata.Items.Keys.Count, false));
                    reports.ForEach(report => report.Write());
                }

                // Add single DAT stats to dir
                dirStats.AddStatistics(datdata.Items);
                dirStats.GameCount += datdata.Items.Keys.Count();

                // Add single DAT stats to totals
                totalStats.AddStatistics(datdata.Items);
                totalStats.GameCount += datdata.Items.Keys.Count();

                // Make sure to assign the new directory
                lastdir = thisdir;
            }

            // Output the directory stats one last time
            reports.ForEach(report => report.WriteMidSeparator());

            if (single)
            {
                reports.ForEach(report => report.ReplaceStatistics(dirStats, $"DIR: {WebUtility.HtmlEncode(lastdir)}", dirStats.GameCount, true));
                reports.ForEach(report => report.Write());
            }

            // Write the mid-footer, if any
            reports.ForEach(report => report.WriteFooterSeparator());

            // Write the header, if any
            reports.ForEach(report => report.WriteMidHeader());

            // Reset the directory stats
            dirStats.ResetStatistics();

            // Output total DAT stats
            reports.ForEach(report => report.ReplaceStatistics(totalStats, "DIR: All DATs", totalStats.GameCount, true));
            reports.ForEach(report => report.Write());

            // Output footer if needed
            reports.ForEach(report => report.WriteFooter());

            logger.User($"{Environment.NewLine}Please check the log folder if the stats scrolled offscreen");
        }

        /// <summary>
        /// Get the proper extension for the stat output format
        /// </summary>
        /// <param name="outDir">Output path to use</param>
        /// <param name="statDatFormat">StatDatFormat to get the extension for</param>
        /// <param name="reportName">Name of the input file to use</param>
        /// <returns>Dictionary of output formats mapped to file names</returns>
        private static Dictionary<StatReportFormat, string> CreateOutStatsNames(string outDir, StatReportFormat statDatFormat, string reportName, bool overwrite = true)
        {
            Dictionary<StatReportFormat, string> output = new Dictionary<StatReportFormat, string>();

            // First try to create the output directory if we need to
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            // Double check the outDir for the end delim
            if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                outDir += Path.DirectorySeparatorChar;

            // For each output format, get the appropriate stream writer
            output.Add(StatReportFormat.None, CreateOutStatsNamesHelper(outDir, ".null", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.Textfile))
                output.Add(StatReportFormat.Textfile, CreateOutStatsNamesHelper(outDir, ".txt", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.CSV))
                output.Add(StatReportFormat.CSV, CreateOutStatsNamesHelper(outDir, ".csv", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.HTML))
                output.Add(StatReportFormat.HTML, CreateOutStatsNamesHelper(outDir, ".html", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.SSV))
                output.Add(StatReportFormat.SSV, CreateOutStatsNamesHelper(outDir, ".ssv", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.TSV))
                output.Add(StatReportFormat.TSV, CreateOutStatsNamesHelper(outDir, ".tsv", reportName, overwrite));

            return output;
        }

        /// <summary>
        /// Help generating the outstats name
        /// </summary>
        /// <param name="outDir">Output directory</param>
        /// <param name="extension">Extension to use for the file</param>
        /// <param name="reportName">Name of the input file to use</param>
        /// <param name="overwrite">True if we ignore existing files, false otherwise</param>
        /// <returns>String containing the new filename</returns>
        private static string CreateOutStatsNamesHelper(string outDir, string extension, string reportName, bool overwrite)
        {
            string outfile = outDir + reportName + extension;
            outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());

            if (!overwrite)
            {
                int i = 1;
                while (File.Exists(outfile))
                {
                    outfile = $"{outDir}{reportName}_{i}{extension}";
                    outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());
                    i++;
                }
            }

            return outfile;
        }
    }
}
