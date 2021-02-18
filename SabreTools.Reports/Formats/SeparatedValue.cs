using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using SabreTools.IO.Writers;
using SabreTools.Logging;

namespace SabreTools.Reports.Formats
{
    /// <summary>
    /// Separated-Value report format
    /// </summary>
    internal class SeparatedValue : BaseReport
    {
        private readonly char _separator;

        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="statsList">List of statistics objects to set</param>
        /// <param name="separator">Separator character to use in output</param>
        public SeparatedValue(List<DatStatistics> statsList, char separator)
            : base(statsList)
        {
            _separator = separator;
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool baddumpCol, bool nodumpCol, bool throwOnError = false)
        {
            InternalStopwatch watch = new InternalStopwatch($"Writing statistics to '{outfile}");

            try
            {
                // Try to create the output file
                FileStream fs = File.Create(outfile);
                if (fs == null)
                {
                    logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                SeparatedValueWriter svw = new SeparatedValueWriter(fs, Encoding.UTF8)
                {
                    Separator = _separator,
                    Quotes = true,
                };

                // Write out the header
                WriteHeader(svw, baddumpCol, nodumpCol);

                // Now process each of the statistics
                for (int i = 0; i < Statistics.Count; i++)
                {
                    // Get the current statistic
                    DatStatistics stat = Statistics[i];

                    // If we have a directory statistic
                    if (stat.IsDirectory)
                    {
                        WriteIndividual(svw, stat, baddumpCol, nodumpCol);

                        // If we have anything but the last value, write the separator
                        if (i < Statistics.Count - 1)
                            WriteFooterSeparator(svw);
                    }

                    // If we have a normal statistic
                    else
                    {
                        WriteIndividual(svw, stat, baddumpCol, nodumpCol);
                    }
                }

                svw.Dispose();
                fs.Dispose();
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
        /// Write out the header to the stream, if any exists
        /// </summary>
        /// <param name="svw">SeparatedValueWriter to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteHeader(SeparatedValueWriter svw, bool baddumpCol, bool nodumpCol)
        {
            string[] headers = new string[]
            {
                "File Name",
                "Total Size",
                "Games",
                "Roms",
                "Disks",
                "# with CRC",
                "# with MD5",
                "# with SHA-1",
                "# with SHA-256",
                "# with SHA-384",
                "# with SHA-512",
                baddumpCol ? "BadDumps" : string.Empty,
                nodumpCol ? "Nodumps" : string.Empty,
            };
            svw.WriteHeader(headers);
            svw.Flush();
        }

        /// <summary>
        /// Write a single set of statistics
        /// </summary>
        /// <param name="svw">SeparatedValueWriter to write to</param>
        /// <param name="stat">DatStatistics object to write out</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteIndividual(SeparatedValueWriter svw, DatStatistics stat, bool baddumpCol, bool nodumpCol)
        {
            string[] values = new string[]
            {
                stat.DisplayName,
                stat.Statistics.TotalSize.ToString(),
                stat.MachineCount.ToString(),
                stat.Statistics.RomCount.ToString(),
                stat.Statistics.DiskCount.ToString(),
                stat.Statistics.CRCCount.ToString(),
                stat.Statistics.MD5Count.ToString(),
                stat.Statistics.SHA1Count.ToString(),
                stat.Statistics.SHA256Count.ToString(),
                stat.Statistics.SHA384Count.ToString(),
                stat.Statistics.SHA512Count.ToString(),
                baddumpCol ? stat.Statistics.BaddumpCount.ToString() : string.Empty,
                nodumpCol ? stat.Statistics.NodumpCount.ToString() : string.Empty,
            };
            svw.WriteValues(values);
            svw.Flush();
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        /// <param name="svw">SeparatedValueWriter to write to</param>
        private void WriteFooterSeparator(SeparatedValueWriter svw)
        {
            svw.WriteString("\n");
            svw.Flush();
        }
    }
}
