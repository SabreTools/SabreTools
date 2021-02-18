using System;
using System.Collections.Generic;
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
        /// <param name="statsList">List of statistics objects to set</param>
        public Html(List<DatStatistics> statsList)
            : base(statsList)
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

                // TODO: Use XTW instead
                StreamWriter sw = new StreamWriter(fs);

                // Write out the header
                WriteHeader(sw, baddumpCol, nodumpCol);

                // Now process each of the statistics
                foreach (DatStatistics stat in Statistics)
                {
                    // If we have a directory statistic
                    if (stat.IsDirectory)
                    {
                        WriteMidSeparator(sw, baddumpCol, nodumpCol);
                        WriteIndividual(sw, stat, baddumpCol, nodumpCol);
                        WriteFooterSeparator(sw, baddumpCol, nodumpCol);
                        WriteMidHeader(sw, baddumpCol, nodumpCol);
                    }

                    // If we have a normal statistic
                    else
                    {
                        WriteIndividual(sw, stat, baddumpCol, nodumpCol);
                    }
                }

                WriteFooter(sw);
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
        /// Write out the header to the stream, if any exists
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteHeader(StreamWriter sw, bool baddumpCol, bool nodumpCol)
        {
            sw.Write(@"<!DOCTYPE html>
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
            sw.Flush();

            // Now write the mid header for those who need it
            WriteMidHeader(sw, baddumpCol, nodumpCol);
        }

        /// <summary>
        /// Write out the mid-header to the stream, if any exists
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteMidHeader(StreamWriter sw, bool baddumpCol, bool nodumpCol)
        {
            sw.Write(@"			<tr bgcolor=string.Emptygraystring.Empty><th>File Name</th><th align=string.Emptyrightstring.Empty>Total Size</th><th align=string.Emptyrightstring.Empty>Games</th><th align=string.Emptyrightstring.Empty>Roms</th>"
+ @"<th align=string.Emptyrightstring.Empty>Disks</th><th align=string.Emptyrightstring.Empty>&#35; with CRC</th><th align=string.Emptyrightstring.Empty>&#35; with MD5</th><th align=string.Emptyrightstring.Empty>&#35; with SHA-1</th><th align=string.Emptyrightstring.Empty>&#35; with SHA-256</th>"
+ (baddumpCol ? "<th class=\".right\">Baddumps</th>" : string.Empty) + (nodumpCol ? "<th class=\".right\">Nodumps</th>" : string.Empty) + "</tr>\n");
            sw.Flush();
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
            sw.Write(line);
            sw.Flush();
        }

        /// <summary>
        /// Write out the separator to the stream, if any exists
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteMidSeparator(StreamWriter sw, bool baddumpCol, bool nodumpCol)
        {
            sw.Write("<tr><td colspan=\""
                        + (baddumpCol && nodumpCol
                            ? "12"
                            : (baddumpCol ^ nodumpCol
                                ? "11"
                                : "10")
                            )
                        + "\"></td></tr>\n");
            sw.Flush();
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteFooterSeparator(StreamWriter sw, bool baddumpCol, bool nodumpCol)
        {
            sw.Write("<tr border=\"0\"><td colspan=\""
                        + (baddumpCol && nodumpCol
                            ? "12"
                            : (baddumpCol ^ nodumpCol
                                ? "11"
                                : "10")
                            )
                        + "\"></td></tr>\n");
            sw.Flush();
        }

        /// <summary>
        /// Write out the footer to the stream, if any exists
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        private void WriteFooter(StreamWriter sw)
        {
            sw.Write(@"		</table>
    </body>
</html>
");
            sw.Flush();
        }
    }
}
