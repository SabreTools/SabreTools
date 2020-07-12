using System;
using System.IO;
using System.Net;

using SabreTools.Library.Tools;

namespace SabreTools.Library.Reports
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

        /// <summary>
        /// Write the report to file
        /// </summary>
        public override void Write()
        {
            string line = "\t\t\t<tr" + (_name.StartsWith("DIR: ")
                            ? $" class=\"dir\"><td>{WebUtility.HtmlEncode(_name.Remove(0, 5))}"
                            : $"><td>{WebUtility.HtmlEncode(_name)}") + "</td>"
                        + $"<td align=\"right\">{Utilities.GetBytesReadable(_stats.TotalSize)}</td>"
                        + $"<td align=\"right\">{_machineCount}</td>"
                        + $"<td align=\"right\">{_stats.RomCount}</td>"
                        + $"<td align=\"right\">{_stats.DiskCount}</td>"
                        + $"<td align=\"right\">{_stats.CRCCount}</td>"
                        + $"<td align=\"right\">{_stats.MD5Count}</td>"
#if NET_FRAMEWORK
                        + $"<td align=\"right\">{_stats.RIPEMD160Count}</td>"
#endif
                        + $"<td align=\"right\">{_stats.SHA1Count}</td>"
                        + $"<td align=\"right\">{_stats.SHA256Count}</td>"
                        + (_baddumpCol ? $"<td align=\"right\">{_stats.BaddumpCount}</td>" : string.Empty)
                        + (_nodumpCol ? $"<td align=\"right\">{_stats.NodumpCount}</td>" : string.Empty)
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
