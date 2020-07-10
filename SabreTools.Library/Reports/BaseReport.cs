using System;
using System.IO;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Reports
{
    /// <summary>
    /// Base class for a report output format
    /// </summary>
    /// TODO: Can this be overhauled to have all types write like DatFiles?
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
            _writer = new StreamWriter(FileExtensions.TryCreate(filename));
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
        /// Create a specific type of BaseReport to be used based on a format and user inputs
        /// </summary>
        /// <param name="statReportFormat">Format of the Statistics Report to be created</param>
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        /// <returns>BaseReport of the specific internal type that corresponds to the inputs</returns>
        public static BaseReport Create(StatReportFormat statReportFormat, string filename, bool baddumpCol, bool nodumpCol)
        {
            switch (statReportFormat)
            {
                case StatReportFormat.Textfile:
                    return new Textfile(null, filename, baddumpCol, nodumpCol);

                case StatReportFormat.CSV:
                    return new Reports.SeparatedValue(null, filename, ',', baddumpCol, nodumpCol);

                case StatReportFormat.HTML:
                    return new Html(null, filename, baddumpCol, nodumpCol);

                case StatReportFormat.SSV:
                    return new Reports.SeparatedValue(null, filename, ';', baddumpCol, nodumpCol);

                case StatReportFormat.TSV:
                    return new Reports.SeparatedValue(null, filename, '\t', baddumpCol, nodumpCol);
            }

            return null;
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
