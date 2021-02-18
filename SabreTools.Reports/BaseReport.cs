using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.DatFiles;
using SabreTools.Logging;
using SabreTools.Reports.Formats;

namespace SabreTools.Reports
{
    /// <summary>
    /// Base class for a report output format
    /// </summary>
    /// TODO: Can this be overhauled to have all types write like DatFiles?
    public abstract class BaseReport
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        protected readonly Logger logger = new Logger();

        #endregion

        public List<DatStatistics> Statistics { get; set; }

        protected StreamWriter _writer;

        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="filename">Name of the file to write out to</param>
        public BaseReport(string filename)
        {
            var fs = File.Create(filename);
            if (fs != null)
                _writer = new StreamWriter(fs) { AutoFlush = true };
        }

        /// <summary>
        /// Create a new report from the stream
        /// </summary>
        /// <param name="stream">Output stream to write to</param>
        public BaseReport(Stream stream)
        {
            if (!stream.CanWrite)
                throw new ArgumentException(nameof(stream));

            _writer = new StreamWriter(stream) { AutoFlush = true };
        }

        /// <summary>
        /// Create a specific type of BaseReport to be used based on a format and user inputs
        /// </summary>
        /// <param name="statReportFormat">Format of the Statistics Report to be created</param>
        /// <param name="statsList">List of statistics objects to set</param>
        /// <param name="filename">Name of the file to write out to</param>
        /// <returns>BaseReport of the specific internal type that corresponds to the inputs</returns>
        public static BaseReport Create(StatReportFormat statReportFormat, List<DatStatistics> statsList, string filename)
        {
            return statReportFormat switch
            {
                StatReportFormat.None => new Textfile(Console.OpenStandardOutput(), true) { Statistics = statsList },
                StatReportFormat.Textfile => new Textfile(filename, false) { Statistics = statsList },
                StatReportFormat.CSV => new SeparatedValue(filename, ',') { Statistics = statsList },
                StatReportFormat.HTML => new Html(filename) { Statistics = statsList },
                StatReportFormat.SSV => new SeparatedValue(filename, ';') { Statistics = statsList },
                StatReportFormat.TSV => new SeparatedValue(filename, '\t') { Statistics = statsList },
                _ => null,
            };
        }

        /// <summary>
        /// Create and open an output file for writing direct from a set of statistics
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the report was written correctly, false otherwise</returns>
        public abstract bool WriteToFile(string outfile, bool baddumpCol, bool nodumpCol, bool throwOnError = false);
    
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
