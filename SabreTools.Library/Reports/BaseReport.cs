using System;
using System.IO;

using SabreTools.Library.DatFiles;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Reports
{
    /// <summary>
    /// Base class for a report output format
    /// </summary>
    public abstract class BaseReport
    {
        protected DatFile _datFile;
        protected StreamWriter _writer;
        protected bool _baddumpCol;
        protected bool _nodumpCol;

        /// <summary>
        /// Create a new report from the input DatFile and the filename
        /// </summary>
        /// <param name="datfile">DatFile to write out statistics for</param>
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public BaseReport(DatFile datfile, string filename, bool baddumpCol = false, bool nodumpCol = false)
        {
            _datFile = datfile;
            _writer = new StreamWriter(Utilities.TryCreate(filename));
            _baddumpCol = baddumpCol;
            _nodumpCol = nodumpCol;
        }

        /// <summary>
        /// Create a new report from the input DatFile and the stream
        /// </summary>
        /// <param name="datfile">DatFile to write out statistics for</param>
        /// <param name="stream">Output stream to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public BaseReport(DatFile datfile, Stream stream, bool baddumpCol = false, bool nodumpCol = false)
        {
            _datFile = datfile;

            if (!stream.CanWrite)
                throw new ArgumentException(nameof(stream));

            _writer = new StreamWriter(stream);
            _baddumpCol = baddumpCol;
            _nodumpCol = nodumpCol;
        }

        /// <summary>
        /// Replace the DatFile that is being output
        /// </summary>
        /// <param name="datfile"></param>
        public void ReplaceDatFile(DatFile datfile)
        {
            _datFile = datfile;
        }

        /// <summary>
        /// Write the report to the output stream
        /// </summary>
        /// <param name="game">Number of games to use, -1 means use the number of keys</param>
        public abstract void Write(long game = -1);

        /// <summary>
        /// Write out the header to the stream, if any exists
        /// </summary>
        public abstract void WriteHeader();

        /// <summary>
        /// Write out the mid-header to the stream, if any exists
        /// </summary>
        public abstract void WriteMidHeader();

        /// <summary>
        /// Write out the separator to the stream, if any exists
        /// </summary>
        public abstract void WriteMidSeparator();

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        public abstract void WriteFooterSeparator();

        /// <summary>
        /// Write out the footer to the stream, if any exists
        /// </summary>
        public abstract void WriteFooter();
    }
}
