using System.IO;

using SabreTools.Library.Tools;

namespace SabreTools.Library.Reports
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

        /// <summary>
        /// Write the report to file
        /// </summary>
        public override void Write()
        {
            string line = @"'" + _name + @"':
--------------------------------------------------
    Uncompressed size:       " + Utilities.GetBytesReadable(_stats.TotalSize) + @"
    Games found:             " + _machineCount + @"
    Roms found:              " + _stats.RomCount + @"
    Disks found:             " + _stats.DiskCount + @"
    Roms with CRC:           " + _stats.CRCCount + @"
    Roms with MD5:           " + _stats.MD5Count
#if NET_FRAMEWORK
+ @"
    Roms with RIPEMD160:     " + _stats.RIPEMD160Count
#endif
+ @"
    Roms with SHA-1:         " + _stats.SHA1Count + @"
    Roms with SHA-256:       " + _stats.SHA256Count + @"
    Roms with SHA-384:       " + _stats.SHA384Count + @"
    Roms with SHA-512:       " + _stats.SHA512Count + "\n";

            if (_baddumpCol)
                line += "	Roms with BadDump status: " + _stats.BaddumpCount + "\n";

            if (_nodumpCol)
                line += "	Roms with Nodump status: " + _stats.NodumpCount + "\n";

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
