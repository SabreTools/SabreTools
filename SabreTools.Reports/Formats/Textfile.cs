using System;
using System.IO;

using SabreTools.Logging;

namespace SabreTools.Reports.Formats
{
    /// <summary>
    /// Textfile report format
    /// </summary>
    internal class Textfile : BaseReport
    {
        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public Textfile(string filename, bool baddumpCol = false, bool nodumpCol = false)
            : base(filename, baddumpCol, nodumpCol)
        {
        }

        /// <summary>
        /// Create a new report from the stream
        /// </summary>
        /// <param name="stream">Output stream to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public Textfile(Stream stream, bool baddumpCol = false, bool nodumpCol = false)
            : base(stream, baddumpCol, nodumpCol)
        {
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
            string line = @"'" + _stats.DisplayName + @"':
--------------------------------------------------
    Uncompressed size:       " + GetBytesReadable(_stats.Statistics.TotalSize) + @"
    Games found:             " + _stats.MachineCount + @"
    Roms found:              " + _stats.Statistics.RomCount + @"
    Disks found:             " + _stats.Statistics.DiskCount + @"
    Roms with CRC:           " + _stats.Statistics.CRCCount + @"
    Roms with MD5:           " + _stats.Statistics.MD5Count + @"
    Roms with SHA-1:         " + _stats.Statistics.SHA1Count + @"
    Roms with SHA-256:       " + _stats.Statistics.SHA256Count + @"
    Roms with SHA-384:       " + _stats.Statistics.SHA384Count + @"
    Roms with SHA-512:       " + _stats.Statistics.SHA512Count + "\n";

            if (_baddumpCol)
                line += "	Roms with BadDump status: " + _stats.Statistics.BaddumpCount + "\n";

            if (_nodumpCol)
                line += "	Roms with Nodump status: " + _stats.Statistics.NodumpCount + "\n";

            // For spacing between DATs
            line += "\n\n";

            _writer.Write(line);
            _writer.Flush();
        }

        /// <summary>
        /// Write out the header to the stream, if any exists
        /// </summary>
        public override void WriteHeader()
        {
            // This call is a no-op for textfile output
        }

        /// <summary>
        /// Write out the mid-header to the stream, if any exists
        /// </summary>
        public override void WriteMidHeader()
        {
            // This call is a no-op for textfile output
        }

        /// <summary>
        /// Write out the separator to the stream, if any exists
        /// </summary>
        public override void WriteMidSeparator()
        {
            // This call is a no-op for textfile output
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
            // This call is a no-op for textfile output
        }
    }
}
