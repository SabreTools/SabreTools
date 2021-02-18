using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Logging;

namespace SabreTools.Reports.Formats
{
    /// <summary>
    /// Textfile report format
    /// </summary>
    internal class Textfile : BaseReport
    {
        private readonly bool _writeToConsole;

        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="statsList">List of statistics objects to set</param>
        /// <param name="writeToConsole">True to write to consoke output, false otherwise</param>
        public Textfile(List<DatStatistics> statsList, bool writeToConsole)
            : base(statsList)
        {
            _writeToConsole = writeToConsole;
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool baddumpCol, bool nodumpCol, bool throwOnError = false)
        {
            InternalStopwatch watch = new InternalStopwatch($"Writing statistics to '{outfile}");

            try
            {
                // Try to create the output file
                Stream fs = _writeToConsole ? Console.OpenStandardOutput() : File.Create(outfile);
                if (fs == null)
                {
                    logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                StreamWriter sw = new StreamWriter(fs);

                // Now process each of the statistics
                for (int i = 0; i < Statistics.Count; i++)
                {
                    // Get the current statistic
                    DatStatistics stat = Statistics[i];

                    // If we have a directory statistic
                    if (stat.IsDirectory)
                    {
                        WriteIndividual(sw, stat, baddumpCol, nodumpCol);
                        
                        // If we have anything but the last value, write the separator
                        if (i < Statistics.Count - 1)
                            WriteFooterSeparator(sw);
                    }

                    // If we have a normal statistic
                    else
                    {
                        WriteIndividual(sw, stat, baddumpCol, nodumpCol);
                    }
                }

                sw.Dispose();
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
        /// Write a single set of statistics
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        /// <param name="stat">DatStatistics object to write out</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteIndividual(StreamWriter sw, DatStatistics stat, bool baddumpCol, bool nodumpCol)
        {
            string line = @"'" + stat.DisplayName + @"':
--------------------------------------------------
    Uncompressed size:       " + GetBytesReadable(stat.Statistics.TotalSize) + @"
    Games found:             " + stat.MachineCount + @"
    Roms found:              " + stat.Statistics.RomCount + @"
    Disks found:             " + stat.Statistics.DiskCount + @"
    Roms with CRC:           " + stat.Statistics.CRCCount + @"
    Roms with MD5:           " + stat.Statistics.MD5Count + @"
    Roms with SHA-1:         " + stat.Statistics.SHA1Count + @"
    Roms with SHA-256:       " + stat.Statistics.SHA256Count + @"
    Roms with SHA-384:       " + stat.Statistics.SHA384Count + @"
    Roms with SHA-512:       " + stat.Statistics.SHA512Count + "\n";

            if (baddumpCol)
                line += "	Roms with BadDump status: " + stat.Statistics.BaddumpCount + "\n";

            if (nodumpCol)
                line += "	Roms with Nodump status: " + stat.Statistics.NodumpCount + "\n";

            // For spacing between DATs
            line += "\n\n";

            sw.Write(line);
            sw.Flush();
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        private void WriteFooterSeparator(StreamWriter sw)
        {
            sw.Write("\n");
            sw.Flush();
        }
    }
}
