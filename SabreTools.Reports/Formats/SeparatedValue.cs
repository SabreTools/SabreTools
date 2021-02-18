using System;
using System.IO;

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
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="separator">Separator character to use in output</param>
        public SeparatedValue(string filename, char separator)
            : base(filename)
        {
            _separator = separator;
        }

        /// <summary>
        /// Create a new report from the input DatFile and the stream
        /// </summary>
        /// <param name="stream">Output stream to write to</param>
        /// <param name="separator">Separator character to use in output</param>
        public SeparatedValue(Stream stream, char separator)
            : base(stream)
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

                // Write out the header
                WriteHeader(baddumpCol, nodumpCol);

                // Now process each of the statistics
                foreach (DatStatistics stat in Statistics)
                {
                    // If we have a directory statistic
                    if (stat.IsDirectory)
                    {
                        WriteIndividual(stat, baddumpCol, nodumpCol);
                        WriteFooterSeparator();
                    }

                    // If we have a normal statistic
                    else
                    {
                        WriteIndividual(stat, baddumpCol, nodumpCol);
                    }
                }

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
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteHeader(bool baddumpCol, bool nodumpCol)
        {
            _writer.Write(string.Format("\"File Name\"{0}\"Total Size\"{0}\"Games\"{0}\"Roms\"{0}\"Disks\"{0}\"# with CRC\"{0}\"# with MD5\"{0}\"# with SHA-1\"{0}\"# with SHA-256\""
                + (baddumpCol ? "{0}\"BadDumps\"" : string.Empty) + (nodumpCol ? "{0}\"Nodumps\"" : string.Empty) + "\n", _separator));
            _writer.Flush();
        }

        /// <summary>
        /// Write a single set of statistics
        /// </summary>
        /// <param name="stat">DatStatistics object to write out</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteIndividual(DatStatistics stat, bool baddumpCol, bool nodumpCol)
        {
            string line = string.Format("\"" + stat.DisplayName + "\"{0}"
                    + "\"" + stat.Statistics.TotalSize + "\"{0}"
                    + "\"" + stat.MachineCount + "\"{0}"
                    + "\"" + stat.Statistics.RomCount + "\"{0}"
                    + "\"" + stat.Statistics.DiskCount + "\"{0}"
                    + "\"" + stat.Statistics.CRCCount + "\"{0}"
                    + "\"" + stat.Statistics.MD5Count + "\"{0}"
                    + "\"" + stat.Statistics.SHA1Count + "\"{0}"
                    + "\"" + stat.Statistics.SHA256Count + "\"{0}"
                    + "\"" + stat.Statistics.SHA384Count + "\"{0}"
                    + "\"" + stat.Statistics.SHA512Count + "\""
                    + (baddumpCol ? "{0}\"" + stat.Statistics.BaddumpCount + "\"" : string.Empty)
                    + (nodumpCol ? "{0}\"" + stat.Statistics.NodumpCount + "\"" : string.Empty)
                    + "\n", _separator);

            _writer.Write(line);
            _writer.Flush();
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        private void WriteFooterSeparator()
        {
            _writer.Write("\n");
            _writer.Flush();
        }
    }
}
