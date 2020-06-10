using System;
using System.IO;
using System.Linq;
using System.Net;

using SabreTools.Library.DatFiles;
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
        /// Create a new report from the input DatFile and the filename
        /// </summary>
        /// <param name="datfile">DatFile to write out statistics for</param>
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public Html(DatFile datfile, string filename, bool baddumpCol = false, bool nodumpCol = false)
            : base(datfile, filename, baddumpCol, nodumpCol)
        {
        }

        /// <summary>
        /// Create a new report from the input DatFile and the stream
        /// </summary>
        /// <param name="datfile">DatFile to write out statistics for</param>
        /// <param name="stream">Output stream to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public Html(DatFile datfile, Stream stream, bool baddumpCol = false, bool nodumpCol = false)
            : base(datfile, stream, baddumpCol, nodumpCol)
        {
        }

        /// <summary>
        /// Write the report to file
        /// </summary>
        /// <param name="game">Number of games to use, -1 means use the number of keys</param>
        public override void Write(long game = -1)
        {
            string line = "\t\t\t<tr" + (_datFile.FileName.StartsWith("DIR: ")
                            ? $" class=\"dir\"><td>{WebUtility.HtmlEncode(_datFile.FileName.Remove(0, 5))}"
                            : $"><td>{WebUtility.HtmlEncode(_datFile.FileName)}") + "</td>"
                        + $"<td align=\"right\">{Utilities.GetBytesReadable(_datFile.TotalSize)}</td>"
                        + $"<td align=\"right\">{(game == -1 ? _datFile.Keys.Count() : game)}</td>"
                        + $"<td align=\"right\">{_datFile.RomCount}</td>"
                        + $"<td align=\"right\">{_datFile.DiskCount}</td>"
                        + $"<td align=\"right\">{_datFile.CRCCount}</td>"
                        + $"<td align=\"right\">{_datFile.MD5Count}</td>"
                        + $"<td align=\"right\">{_datFile.RIPEMD160Count}</td>"
                        + $"<td align=\"right\">{_datFile.SHA1Count}</td>"
                        + $"<td align=\"right\">{_datFile.SHA256Count}</td>"
                        + (_baddumpCol ? $"<td align=\"right\">{_datFile.BaddumpCount}</td>" : string.Empty)
                        + (_nodumpCol ? $"<td align=\"right\">{_datFile.NodumpCount}</td>" : string.Empty)
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
