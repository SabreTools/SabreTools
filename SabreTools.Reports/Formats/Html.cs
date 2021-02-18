using System;
using System.IO;
using System.Net;

using SabreTools.Logging;

namespace SabreTools.Reports.Formats
{
    /// <summary>
    /// HTML report format
    /// </summary>
    /// TODO: Make output standard width, without making the entire thing a table
    internal class Html : BaseReport
    {
        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="filename">Name of the file to write out to</param>
        public Html(string filename)
            : base(filename)
        {
        }

        /// <summary>
        /// Create a new report from the stream
        /// </summary>
        /// <param name="stream">Output stream to write to</param>
        public Html(Stream stream)
            : base(stream)
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
                WriteHeader(baddumpCol, nodumpCol);

                // Now process each of the statistics
                foreach (DatStatistics stat in Statistics)
                {
                    // If we have a directory statistic
                    if (stat.IsDirectory)
                    {
                        WriteMidSeparator(baddumpCol, nodumpCol);
                        WriteIndividual(stat, baddumpCol, nodumpCol);
                        WriteFooterSeparator(baddumpCol, nodumpCol);
                        WriteMidHeader(baddumpCol, nodumpCol);
                    }

                    // If we have a normal statistic
                    else
                    {
                        WriteIndividual(stat, baddumpCol, nodumpCol);
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
        /// Write out the header to the stream, if any exists
        /// </summary>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteHeader(bool baddumpCol, bool nodumpCol)
        {
            _writer.Write(@"<!DOCTYPE html>
<html>
    <header>
        <title>DAT Statistics Report</title>
        <style>
            body {
                background-color: lightgray;
            }
            .dir {
                color: #0088FF;
            }
            .right {
                align: right;
            }
        </style>
    </header>
    <body>
        <h2>DAT Statistics Report (" + DateTime.Now.ToShortDateString() + @")</h2>
        <table border=string.Empty1string.Empty cellpadding=string.Empty5string.Empty cellspacing=string.Empty0string.Empty>
");
            _writer.Flush();

            // Now write the mid header for those who need it
            WriteMidHeader(baddumpCol, nodumpCol);
        }

        /// <summary>
        /// Write out the mid-header to the stream, if any exists
        /// </summary>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteMidHeader(bool baddumpCol, bool nodumpCol)
        {
            _writer.Write(@"			<tr bgcolor=string.Emptygraystring.Empty><th>File Name</th><th align=string.Emptyrightstring.Empty>Total Size</th><th align=string.Emptyrightstring.Empty>Games</th><th align=string.Emptyrightstring.Empty>Roms</th>"
+ @"<th align=string.Emptyrightstring.Empty>Disks</th><th align=string.Emptyrightstring.Empty>&#35; with CRC</th><th align=string.Emptyrightstring.Empty>&#35; with MD5</th><th align=string.Emptyrightstring.Empty>&#35; with SHA-1</th><th align=string.Emptyrightstring.Empty>&#35; with SHA-256</th>"
+ (baddumpCol ? "<th class=\".right\">Baddumps</th>" : string.Empty) + (nodumpCol ? "<th class=\".right\">Nodumps</th>" : string.Empty) + "</tr>\n");
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
            string line = "\t\t\t<tr" + (stat.DisplayName.StartsWith("DIR: ")
                            ? $" class=\"dir\"><td>{WebUtility.HtmlEncode(stat.DisplayName.Remove(0, 5))}"
                            : $"><td>{WebUtility.HtmlEncode(stat.DisplayName)}") + "</td>"
                        + $"<td align=\"right\">{GetBytesReadable(stat.Statistics.TotalSize)}</td>"
                        + $"<td align=\"right\">{stat.MachineCount}</td>"
                        + $"<td align=\"right\">{stat.Statistics.RomCount}</td>"
                        + $"<td align=\"right\">{stat.Statistics.DiskCount}</td>"
                        + $"<td align=\"right\">{stat.Statistics.CRCCount}</td>"
                        + $"<td align=\"right\">{stat.Statistics.MD5Count}</td>"
                        + $"<td align=\"right\">{stat.Statistics.SHA1Count}</td>"
                        + $"<td align=\"right\">{stat.Statistics.SHA256Count}</td>"
                        + (baddumpCol ? $"<td align=\"right\">{stat.Statistics.BaddumpCount}</td>" : string.Empty)
                        + (nodumpCol ? $"<td align=\"right\">{stat.Statistics.NodumpCount}</td>" : string.Empty)
                        + "</tr>\n";
            _writer.Write(line);
            _writer.Flush();
        }

        /// <summary>
        /// Write out the separator to the stream, if any exists
        /// </summary>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteMidSeparator(bool baddumpCol, bool nodumpCol)
        {
            _writer.Write("<tr><td colspan=\""
                        + (baddumpCol && nodumpCol
                            ? "12"
                            : (baddumpCol ^ nodumpCol
                                ? "11"
                                : "10")
                            )
                        + "\"></td></tr>\n");
            _writer.Flush();
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteFooterSeparator(bool baddumpCol, bool nodumpCol)
        {
            _writer.Write("<tr border=\"0\"><td colspan=\""
                        + (baddumpCol && nodumpCol
                            ? "12"
                            : (baddumpCol ^ nodumpCol
                                ? "11"
                                : "10")
                            )
                        + "\"></td></tr>\n");
            _writer.Flush();
        }

        /// <summary>
        /// Write out the footer to the stream, if any exists
        /// </summary>
        private void WriteFooter()
        {
            _writer.Write(@"		</table>
    </body>
</html>
");
            _writer.Flush();
        }
    }
}
