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
        // TODO: Remove explicit constructors for filename and stream
        private readonly bool _writeToConsole;

        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="writeToConsole">True to write to consoke output, false otherwise</param>
        public Textfile(string filename, bool writeToConsole)
            : base(filename)
        {
            _writeToConsole = writeToConsole;
        }

        /// <summary>
        /// Create a new report from the stream
        /// </summary>
        /// <param name="stream">Output stream to write to</param>
        /// <param name="writeToConsole">True to write to consoke output, false otherwise</param>
        public Textfile(Stream stream, bool writeToConsole)
            : base(stream)
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
                FileStream fs = File.Create(outfile);
                if (fs == null)
                {
                    logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

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
        /// Write a single set of statistics
        /// </summary>
        /// <param name="stat">DatStatistics object to write out</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteIndividual(DatStatistics stat, bool baddumpCol, bool nodumpCol)
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
