﻿using System;
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
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public Html(string filename, bool baddumpCol = false, bool nodumpCol = false)
            : base(filename, baddumpCol, nodumpCol)
        {
        }

        /// <summary>
        /// Create a new report from the stream
        /// </summary>
        /// <param name="datfile">DatFile to write out statistics for</param>
        /// <param name="stream">Output stream to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public Html(Stream stream, bool baddumpCol = false, bool nodumpCol = false)
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
            string line = "\t\t\t<tr" + (_stats.DisplayName.StartsWith("DIR: ")
                            ? $" class=\"dir\"><td>{WebUtility.HtmlEncode(_stats.DisplayName.Remove(0, 5))}"
                            : $"><td>{WebUtility.HtmlEncode(_stats.DisplayName)}") + "</td>"
                        + $"<td align=\"right\">{GetBytesReadable(_stats.Statistics.TotalSize)}</td>"
                        + $"<td align=\"right\">{_stats.MachineCount}</td>"
                        + $"<td align=\"right\">{_stats.Statistics.RomCount}</td>"
                        + $"<td align=\"right\">{_stats.Statistics.DiskCount}</td>"
                        + $"<td align=\"right\">{_stats.Statistics.CRCCount}</td>"
                        + $"<td align=\"right\">{_stats.Statistics.MD5Count}</td>"
                        + $"<td align=\"right\">{_stats.Statistics.SHA1Count}</td>"
                        + $"<td align=\"right\">{_stats.Statistics.SHA256Count}</td>"
                        + (_baddumpCol ? $"<td align=\"right\">{_stats.Statistics.BaddumpCount}</td>" : string.Empty)
                        + (_nodumpCol ? $"<td align=\"right\">{_stats.Statistics.NodumpCount}</td>" : string.Empty)
                        + "</tr>\n";
            _writer.Write(line);
            _writer.Flush();
        }

        /// <summary>
        /// Write out the header to the stream, if any exists
        /// </summary>
        public override void WriteHeader()
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
            WriteMidHeader();
        }

        /// <summary>
        /// Write out the mid-header to the stream, if any exists
        /// </summary>
        public override void WriteMidHeader()
        {
            _writer.Write(@"			<tr bgcolor=string.Emptygraystring.Empty><th>File Name</th><th align=string.Emptyrightstring.Empty>Total Size</th><th align=string.Emptyrightstring.Empty>Games</th><th align=string.Emptyrightstring.Empty>Roms</th>"
+ @"<th align=string.Emptyrightstring.Empty>Disks</th><th align=string.Emptyrightstring.Empty>&#35; with CRC</th><th align=string.Emptyrightstring.Empty>&#35; with MD5</th><th align=string.Emptyrightstring.Empty>&#35; with SHA-1</th><th align=string.Emptyrightstring.Empty>&#35; with SHA-256</th>"
+ (_baddumpCol ? "<th class=\".right\">Baddumps</th>" : string.Empty) + (_nodumpCol ? "<th class=\".right\">Nodumps</th>" : string.Empty) + "</tr>\n");
            _writer.Flush();
        }

        /// <summary>
        /// Write out the separator to the stream, if any exists
        /// </summary>
        public override void WriteMidSeparator()
        {
            _writer.Write("<tr><td colspan=\""
                        + (_baddumpCol && _nodumpCol
                            ? "12"
                            : (_baddumpCol ^ _nodumpCol
                                ? "11"
                                : "10")
                            )
                        + "\"></td></tr>\n");
            _writer.Flush();
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        public override void WriteFooterSeparator()
        {
            _writer.Write("<tr border=\"0\"><td colspan=\""
                        + (_baddumpCol && _nodumpCol
                            ? "12"
                            : (_baddumpCol ^ _nodumpCol
                                ? "11"
                                : "10")
                            )
                        + "\"></td></tr>\n");
            _writer.Flush();
        }

        /// <summary>
        /// Write out the footer to the stream, if any exists
        /// </summary>
        public override void WriteFooter()
        {
            _writer.Write(@"		</table>
    </body>
</html>
");
            _writer.Flush();
        }
    }
}