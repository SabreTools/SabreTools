using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Helper methods for writing from DatFiles
    /// </summary>
    public class Writer
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger logger = new Logger();

        #endregion

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        /// <param name="outDir">Set the output directory (current directory on null)</param>
        /// <param name="overwrite">True if files should be overwritten (default), false if they should be renamed instead</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <param name="quotes">True if quotes are assumed in supported types (default), false otherwise</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public static bool Write(
            DatFile datFile,
            string outDir,
            bool overwrite = true,
            bool ignoreblanks = false,
            bool quotes = true,
            bool throwOnError = false)
        {
            // If we have nothing writable, abort
            if (!HasWritable(datFile))
            {
                logger.User("There were no items to write out!");
                return false;
            }

            // Ensure the output directory is set and created
            outDir = outDir.Ensure(create: true);

            InternalStopwatch watch = new InternalStopwatch($"Writing out internal dat to '{outDir}'");

            // If the DAT has no output format, default to XML
            if (datFile.Header.DatFormat == 0)
            {
                logger.Verbose("No DAT format defined, defaulting to XML");
                datFile.Header.DatFormat = DatFormat.Logiqx;
            }

            // Make sure that the three essential fields are filled in
            EnsureHeaderFields(datFile);

            // Bucket roms by game name, if not already
            datFile.Items.BucketBy(ItemKey.Machine, DedupeType.None);

            // Output the number of items we're going to be writing
            logger.User($"A total of {datFile.Items.TotalCount - datFile.Items.RemovedCount} items will be written out to '{datFile.Header.FileName}'");

            // Get the outfile names
            Dictionary<DatFormat, string> outfiles = datFile.Header.CreateOutFileNames(outDir, overwrite);

            try
            {
                // Write out all required formats
                Parallel.ForEach(outfiles.Keys, Globals.ParallelOptions, datFormat =>
                {
                    string outfile = outfiles[datFormat];
                    try
                    {
                        DatFile.Create(datFormat, datFile, quotes)?.WriteToFile(outfile, ignoreblanks, throwOnError);
                    }
                    catch (Exception ex) when (!throwOnError)
                    {
                        logger.Error(ex, $"Datfile '{outfile}' could not be written out");
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
        /// Write the stats out to console for the current DatFile
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        public static void WriteStatsToConsole(DatFile datFile)
        {
            if (datFile.Items.RomCount + datFile.Items.DiskCount == 0)
                datFile.Items.RecalculateStats();

            datFile.Items.BucketBy(ItemKey.Machine, DedupeType.None, norename: true);

            var statsList = new List<DatStatistics>
            {
                new DatStatistics
                {
                    Statistics = datFile.Items,
                    DisplayName = datFile.Header.FileName,
                    MachineCount = datFile.Items.Keys.Count(),
                    IsDirectory = false,
                },
            };
            var consoleOutput = BaseReport.Create(StatReportFormat.None, statsList);
            consoleOutput.WriteToFile(null, true, true);
        }

        /// <summary>
        /// Ensure that FileName, Name, and Description are filled with some value
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        private static void EnsureHeaderFields(DatFile datFile)
        {
            // Empty FileName
            if (string.IsNullOrWhiteSpace(datFile.Header.FileName))
            {
                if (string.IsNullOrWhiteSpace(datFile.Header.Name) && string.IsNullOrWhiteSpace(datFile.Header.Description))
                    datFile.Header.FileName = datFile.Header.Name = datFile.Header.Description = "Default";

                else if (string.IsNullOrWhiteSpace(datFile.Header.Name) && !string.IsNullOrWhiteSpace(datFile.Header.Description))
                    datFile.Header.FileName = datFile.Header.Name = datFile.Header.Description;

                else if (!string.IsNullOrWhiteSpace(datFile.Header.Name) && string.IsNullOrWhiteSpace(datFile.Header.Description))
                    datFile.Header.FileName = datFile.Header.Description = datFile.Header.Name;

                else if (!string.IsNullOrWhiteSpace(datFile.Header.Name) && !string.IsNullOrWhiteSpace(datFile.Header.Description))
                    datFile.Header.FileName = datFile.Header.Description;
            }

            // Filled FileName
            else
            {
                if (string.IsNullOrWhiteSpace(datFile.Header.Name) && string.IsNullOrWhiteSpace(datFile.Header.Description))
                    datFile.Header.Name = datFile.Header.Description = datFile.Header.FileName;

                else if (string.IsNullOrWhiteSpace(datFile.Header.Name) && !string.IsNullOrWhiteSpace(datFile.Header.Description))
                    datFile.Header.Name = datFile.Header.Description;

                else if (!string.IsNullOrWhiteSpace(datFile.Header.Name) && string.IsNullOrWhiteSpace(datFile.Header.Description))
                    datFile.Header.Description = datFile.Header.Name;
            }
        }

        /// <summary>
        /// Get if the DatFile has any writable items
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        /// <returns>True if there are any writable items, false otherwise</returns>
        private static bool HasWritable(DatFile datFile)
        {
            // Force a statistics recheck, just in case
            datFile.Items.RecalculateStats();

            // If there's nothing there, abort
            if (datFile.Items.TotalCount == 0)
                return false;

            // If every item is removed, abort
            if (datFile.Items.TotalCount == datFile.Items.RemovedCount)
                return false;

            return true;
        }    
    }
}