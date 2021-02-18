using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using SabreTools.Core;
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
        /// Calculate statistics from a list of inputs
        /// </summary>
        /// <param name="inputs">List of input files and folders</param>
        /// <param name="single">True if single DAT stats are output, false otherwise</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        public static List<DatStatistics> CalculateStatistics(List<string> inputs, bool single, bool throwOnError = false)
        {
            // Create the output list
            List<DatStatistics> stats = new List<DatStatistics>();

            // Make sure we have all files and then order them
            List<ParentablePath> files = PathTool.GetFilesOnly(inputs);
            files = files
                .OrderBy(i => Path.GetDirectoryName(i.CurrentPath))
                .ThenBy(i => Path.GetFileName(i.CurrentPath))
                .ToList();

            // Init total
            DatStatistics totalStats = new DatStatistics
            {
                Statistics = new ItemDictionary(),
                DisplayName = "DIR: All DATs",
                MachineCount = 0,
                IsDirectory = true,
            };

            // Init directory-level variables
            string lastdir = null;
            DatStatistics dirStats = new DatStatistics
            {
                Statistics = new ItemDictionary(),
                MachineCount = 0,
                IsDirectory = true,
            };

            // Now process each of the input files
            foreach (ParentablePath file in files)
            {
                // Get the directory for the current file
                string thisdir = Path.GetDirectoryName(file.CurrentPath);

                // If we don't have the first file and the directory has changed, show the previous directory stats and reset
                if (lastdir != null && thisdir != lastdir && single)
                {
                    dirStats.DisplayName = $"DIR: {WebUtility.HtmlEncode(lastdir)}";
                    dirStats.MachineCount = dirStats.Statistics.GameCount;
                    stats.Add(dirStats);
                    dirStats = new DatStatistics
                    {
                        Statistics = new ItemDictionary(),
                        MachineCount = 0,
                        IsDirectory = true,
                    };
                }

                InternalStopwatch watch = new InternalStopwatch($"Collecting statistics for '{file.CurrentPath}'");

                List<string> machines = new List<string>();
                DatFile datdata = Parser.CreateAndParse(file.CurrentPath, statsOnly: true, throwOnError: throwOnError);
                datdata.Items.BucketBy(ItemKey.Machine, DedupeType.None, norename: true);

                // Add single DAT stats (if asked)
                if (single)
                {
                    DatStatistics individualStats = new DatStatistics
                    {
                        Statistics = datdata.Items,
                        DisplayName = datdata.Header.FileName,
                        MachineCount = datdata.Items.Keys.Count,
                        IsDirectory = false,
                    };
                    stats.Add(individualStats);
                }

                // Add single DAT stats to dir
                dirStats.Statistics.AddStatistics(datdata.Items);
                dirStats.Statistics.GameCount += datdata.Items.Keys.Count();

                // Add single DAT stats to totals
                totalStats.Statistics.AddStatistics(datdata.Items);
                totalStats.Statistics.GameCount += datdata.Items.Keys.Count();

                // Make sure to assign the new directory
                lastdir = thisdir;

                watch.Stop();
            }

            // Add last directory stats
            if (single)
            {
                dirStats.DisplayName = $"DIR: {WebUtility.HtmlEncode(lastdir)}";
                dirStats.MachineCount = dirStats.Statistics.GameCount;
                stats.Add(dirStats);
            }

            // Add total DAT stats
            totalStats.MachineCount = totalStats.Statistics.GameCount;
            stats.Add(totalStats);

            return stats;
        }

        /// <summary>
        /// Output the stats for a list of input dats as files in a human-readable format
        /// </summary>
        /// <param name="stats">List of pre-calculated statistics objects</param>
        /// <param name="reportName">Name of the output file</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        /// <param name="statDatFormat"> Set the statistics output format to use</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the report was written correctly, false otherwise</returns>
        public static bool Write(
            List<DatStatistics> stats,
            string reportName,
            string outDir,
            bool baddumpCol,
            bool nodumpCol,
            StatReportFormat statDatFormat,
            bool throwOnError = false)
        {
            // If there's no output format, set the default
            if (statDatFormat == StatReportFormat.None)
            {
                logger.Verbose("No report format defined, defaulting to textfile");
                statDatFormat = StatReportFormat.Textfile;
            }

            // Get the proper output file name
            if (string.IsNullOrWhiteSpace(reportName))
                reportName = "report";

            // Get the proper output directory name
            outDir = outDir.Ensure();

            InternalStopwatch watch = new InternalStopwatch($"Writing out report data to '{outDir}'");

            // Get the dictionary of desired output report names
            Dictionary<StatReportFormat, string> outfiles = CreateOutStatsNames(outDir, statDatFormat, reportName);

            try
            {
                // Write out all required formats
                Parallel.ForEach(outfiles.Keys, Globals.ParallelOptions, reportFormat =>
                {
                    string outfile = outfiles[reportFormat];
                    try
                    {
                        BaseReport.Create(reportFormat, stats)?.WriteToFile(outfile, baddumpCol, nodumpCol, throwOnError);
                    }
                    catch (Exception ex) when (!throwOnError)
                    {
                        logger.Error(ex, $"Report '{outfile}' could not be written out");
                    }

                });
            }
            catch (Exception ex) when (!throwOnError)
            {
                logger.Error(ex);
                return false;
            }
            finally
            {
                watch.Stop();
            }

            return true;
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
