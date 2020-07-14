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
        protected string _name;
        protected long _machineCount;
        protected DatStats _stats;

        protected StreamWriter _writer;
        protected bool _baddumpCol;
        protected bool _nodumpCol;

        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public BaseReport(string filename, bool baddumpCol = false, bool nodumpCol = false)
        {
            _writer = new StreamWriter(FileExtensions.TryCreate(filename));
            _baddumpCol = baddumpCol;
            _nodumpCol = nodumpCol;
        }

        /// <summary>
        /// Create a new report from the stream
        /// </summary>
        /// <param name="stream">Output stream to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        public BaseReport(Stream stream, bool baddumpCol = false, bool nodumpCol = false)
        {
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
#if NET_FRAMEWORK
            switch (statReportFormat)
            {
                case StatReportFormat.None:
                    return new Textfile(Console.OpenStandardOutput(), baddumpCol, nodumpCol);

                case StatReportFormat.Textfile:
                    return new Textfile(filename, baddumpCol, nodumpCol);

                case StatReportFormat.CSV:
                    return new SeparatedValue(filename, ',', baddumpCol, nodumpCol);

                case StatReportFormat.HTML:
                    return new Html(filename, baddumpCol, nodumpCol);

                case StatReportFormat.SSV:
                    return new SeparatedValue(filename, ';', baddumpCol, nodumpCol);

                case StatReportFormat.TSV:
                    return new SeparatedValue(filename, '\t', baddumpCol, nodumpCol);

                default:
                    return null;
            }
#else
            return statReportFormat switch
            {
                StatReportFormat.None => new Textfile(Console.OpenStandardOutput(), baddumpCol, nodumpCol),
                StatReportFormat.Textfile => new Textfile(filename, baddumpCol, nodumpCol),
                StatReportFormat.CSV => new SeparatedValue(filename, ',', baddumpCol, nodumpCol),
                StatReportFormat.HTML => new Html(filename, baddumpCol, nodumpCol),
                StatReportFormat.SSV => new SeparatedValue(filename, ';', baddumpCol, nodumpCol),
                StatReportFormat.TSV => new SeparatedValue(filename, '\t', baddumpCol, nodumpCol),
                _ => null,
            };
#endif
        }

        /// <summary>
        /// Replace the statistics that is being output
        /// </summary>
        public void ReplaceStatistics(string datName, long machineCount, DatStats datStats)
        {
            _name = datName;
            _machineCount = machineCount;
            _stats = datStats;
        }

        /// <summary>
        /// Write the report to the output stream
        /// </summary>
        public abstract void Write();

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
