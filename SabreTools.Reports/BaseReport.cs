using System;
using System.IO;

using SabreTools.DatFiles;
using SabreTools.Reports.Formats;

namespace SabreTools.Reports
{
    /// <summary>
    /// Base class for a report output format
    /// </summary>
    /// TODO: Can this be overhauled to have all types write like DatFiles?
    public abstract class BaseReport
    {
        protected DatStatistics _stats;

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
            var fs = File.Create(filename);
            if (fs != null)
                _writer = new StreamWriter(fs) { AutoFlush = true };

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

            _writer = new StreamWriter(stream) { AutoFlush = true };
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
        }

        /// <summary>
        /// Replace the statistics that is being output
        /// </summary>
        /// <param name="stats">Format of the Statistics Report to be created</param>
        /// <param name="displayName">Format of the Statistics Report to be created</param>
        /// <param name="machineCount">Format of the Statistics Report to be created</param>
        /// <param name="isDirectory">Format of the Statistics Report to be created</param>
        public void ReplaceStatistics(ItemDictionary stats, string displayName, long machineCount, bool isDirectory)
        {
            _stats.Statistics = stats;
            _stats.DisplayName = displayName;
            _stats.MachineCount = machineCount;
            _stats.IsDirectory = isDirectory;
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
    
        /// <summary>
        /// Returns the human-readable file size for an arbitrary, 64-bit file size 
        /// The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Human-readable file size</returns>
        /// <link>http://www.somacon.com/p576.php</link>
        protected static string GetBytesReadable(long input)
        {
            // Get absolute value
            long absolute_i = (input < 0 ? -input : input);

            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (input >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (input >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (input >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (input >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (input >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = input;
            }
            else
            {
                return input.ToString("0 B"); // Byte
            }

            // Divide by 1024 to get fractional value
            readable /= 1024;

            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }
    }
}
