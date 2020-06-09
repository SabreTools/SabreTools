using System.IO;
using System.Linq;

using SabreTools.Library.DatFiles;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Reports
{
    /// <summary>
    /// Textfile report format
    /// </summary>
    internal class Textfile : BaseReport
    {
        /// <summary>
        /// Create a new report from the input DatFile and the filename
        /// </summary>
        /// <param name="datfile">DatFile to write out statistics for</param>
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public Textfile(DatFile datfile, string filename, bool baddumpCol = false, bool nodumpCol = false)
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
        public Textfile(DatFile datfile, Stream stream, bool baddumpCol = false, bool nodumpCol = false)
            : base(datfile, stream, baddumpCol, nodumpCol)
        {
        }

        /// <summary>
        /// Write the report to file
        /// </summary>
        /// <param name="game">Number of games to use, -1 means use the number of keys</param>
        public override void Write(long game = -1)
        {
            string line = @"'" + _datFile.FileName + @"':
--------------------------------------------------
    Uncompressed size:       " + Utilities.GetBytesReadable(_datFile.TotalSize) + @"
    Games found:             " + (game == -1 ? _datFile.Keys.Count() : game) + @"
    Roms found:              " + _datFile.RomCount + @"
    Disks found:             " + _datFile.DiskCount + @"
    Roms with CRC:           " + _datFile.CRCCount + @"
    Roms with MD5:           " + _datFile.MD5Count + @"
    Roms with RIPEMD160:     " + _datFile.RIPEMD160Count + @"
    Roms with SHA-1:         " + _datFile.SHA1Count + @"
    Roms with SHA-256:       " + _datFile.SHA256Count + @"
    Roms with SHA-384:       " + _datFile.SHA384Count + @"
    Roms with SHA-512:       " + _datFile.SHA512Count + "\n";

            if (_baddumpCol)
                line += "	Roms with BadDump status: " + _datFile.BaddumpCount + "\n";

            if (_nodumpCol)
                line += "	Roms with Nodump status: " + _datFile.NodumpCount + "\n";

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
