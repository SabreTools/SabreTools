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
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public SeparatedValue(string filename, char separator, bool baddumpCol = false, bool nodumpCol = false)
            : base(filename, baddumpCol, nodumpCol)
        {
            _separator = separator;
        }

        /// <summary>
        /// Create a new report from the input DatFile and the stream
        /// </summary>
        /// <param name="stream">Output stream to write to</param>
        /// <param name="separator">Separator character to use in output</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public SeparatedValue(Stream stream, char separator, bool baddumpCol = false, bool nodumpCol = false)
            : base(stream, baddumpCol, nodumpCol)
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
                WriteHeader();

                // Now process each of the statistics
                foreach (DatStatistics stat in _statsList)
                {
                    // If we have a directory statistic
                    if (stat.IsDirectory)
                    {
                        WriteMidSeparator();
                        ReplaceStatistics(stat);
                        WriteIndividual();
                        WriteFooterSeparator();
                        WriteMidHeader();
                    }

                    // If we have a normal statistic
                    else
                    {
                        ReplaceStatistics(stat);
                        WriteIndividual();
                    }
                }

                WriteFooter();
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
        /// Write the report to file
        /// </summary>
        public override void WriteIndividual()
        {
            string line = string.Format("\"" + _stats.DisplayName + "\"{0}"
                    + "\"" + _stats.Statistics.TotalSize + "\"{0}"
                    + "\"" + _stats.MachineCount + "\"{0}"
                    + "\"" + _stats.Statistics.RomCount + "\"{0}"
                    + "\"" + _stats.Statistics.DiskCount + "\"{0}"
                    + "\"" + _stats.Statistics.CRCCount + "\"{0}"
                    + "\"" + _stats.Statistics.MD5Count + "\"{0}"
                    + "\"" + _stats.Statistics.SHA1Count + "\"{0}"
                    + "\"" + _stats.Statistics.SHA256Count + "\"{0}"
                    + "\"" + _stats.Statistics.SHA384Count + "\"{0}"
                    + "\"" + _stats.Statistics.SHA512Count + "\""
                    + (_baddumpCol ? "{0}\"" + _stats.Statistics.BaddumpCount + "\"" : string.Empty)
                    + (_nodumpCol ? "{0}\"" + _stats.Statistics.NodumpCount + "\"" : string.Empty)
                    + "\n", _separator);

            _writer.Write(line);
            _writer.Flush();
        }

        /// <summary>
        /// Write out the header to the stream, if any exists
        /// </summary>
        public override void WriteHeader()
        {
            _writer.Write(string.Format("\"File Name\"{0}\"Total Size\"{0}\"Games\"{0}\"Roms\"{0}\"Disks\"{0}\"# with CRC\"{0}\"# with MD5\"{0}\"# with SHA-1\"{0}\"# with SHA-256\""
                + (_baddumpCol ? "{0}\"BadDumps\"" : string.Empty) + (_nodumpCol ? "{0}\"Nodumps\"" : string.Empty) + "\n", _separator));
            _writer.Flush();
        }

        /// <summary>
        /// Write out the mid-header to the stream, if any exists
        /// </summary>
        public override void WriteMidHeader()
        {
            // This call is a no-op for separated value formats
        }

        /// <summary>
        /// Write out the separator to the stream, if any exists
        /// </summary>
        public override void WriteMidSeparator()
        {
            // This call is a no-op for separated value formats
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        public override void WriteFooterSeparator()
        {
            _writer.Write("\n");
            _writer.Flush();
        }

        /// <summary>
        /// Write out the footer to the stream, if any exists
        /// </summary>
        public override void WriteFooter()
        {
            // This call is a no-op for separated value formats
        }
    }
}
