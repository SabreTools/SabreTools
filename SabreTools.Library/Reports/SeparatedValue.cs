using System.IO;
using System.Linq;

using SabreTools.Library.DatFiles;

namespace SabreTools.Library.Reports
{
    /// <summary>
    /// Separated-Value report format
    /// </summary>
    internal class SeparatedValue : BaseReport
    {
        private char _separator;

        /// <summary>
        /// Create a new report from the input DatFile and the filename
        /// </summary>
        /// <param name="datfile">DatFile to write out statistics for</param>
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="separator">Separator character to use in output</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public SeparatedValue(DatFile datfile, string filename, char separator, bool baddumpCol = false, bool nodumpCol = false)
            : base(datfile, filename, baddumpCol, nodumpCol)
        {
            _separator = separator;
        }

        /// <summary>
        /// Create a new report from the input DatFile and the stream
        /// </summary>
        /// <param name="datfile">DatFile to write out statistics for</param>
        /// <param name="stream">Output stream to write to</param>
        /// <param name="separator">Separator character to use in output</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public SeparatedValue(DatFile datfile, Stream stream, char separator, bool baddumpCol = false, bool nodumpCol = false)
            : base(datfile, stream, baddumpCol, nodumpCol)
        {
            _separator = separator;
        }

        /// <summary>
        /// Write the report to file
        /// </summary>
        /// <param name="game">Number of games to use, -1 means use the number of keys</param>
        public override void Write(long game = -1)
        {
            string line = string.Format("\"" + _datFile.FileName + "\"{0}"
                    + "\"" + _datFile.TotalSize + "\"{0}"
                    + "\"" + (game == -1 ? _datFile.Keys.Count() : game) + "\"{0}"
                    + "\"" + _datFile.RomCount + "\"{0}"
                    + "\"" + _datFile.DiskCount + "\"{0}"
                    + "\"" + _datFile.CRCCount + "\"{0}"
                    + "\"" + _datFile.MD5Count + "\"{0}"
                    + "\"" + _datFile.RIPEMD160Count + "\"{0}"
                    + "\"" + _datFile.SHA1Count + "\"{0}"
                    + "\"" + _datFile.SHA256Count + "\"{0}"
                    + "\"" + _datFile.SHA384Count + "\"{0}"
                    + "\"" + _datFile.SHA512Count + "\""
                    + (_baddumpCol ? "{0}\"" + _datFile.BaddumpCount + "\"" : string.Empty)
                    + (_nodumpCol ? "{0}\"" + _datFile.BaddumpCount + "\"" : string.Empty)
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
