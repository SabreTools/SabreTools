using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.DatFiles;
using SabreTools.IO.Logging;

#pragma warning disable IDE0290 // Use primary constructor
namespace SabreTools.Reports
{
    /// <summary>
    /// Base class for a report output format
    /// </summary>
    public abstract class BaseReport
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        protected readonly Logger _logger = new();

        #endregion

        /// <summary>
        /// Set of DatStatistics objects to use for formatting
        /// </summary>
        protected List<DatStatistics> _statistics;

        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="statsList">List of statistics objects to set</param>
        public BaseReport(List<DatStatistics> statsList)
        {
            _statistics = statsList;
        }

        /// <summary>
        /// Create and open an output file for writing direct from a set of statistics
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the report was written correctly, false otherwise</returns>
        public bool WriteToFile(string? outfile, bool baddumpCol, bool nodumpCol, bool throwOnError = false)
        {
            InternalStopwatch watch = new($"Writing statistics to '{outfile}");

            try
            {
                // Try to create the output file
                FileStream stream = File.Create(outfile ?? string.Empty);
                if (stream is null)
                {
                    _logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                // Write to the stream
                bool result = WriteToStream(stream, baddumpCol, nodumpCol, throwOnError);

                // Dispose of the stream
                stream.Dispose();
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Error(ex);
                return false;
            }
            finally
            {
                watch.Stop();
            }

            return true;
        }

        /// <summary>
        /// Write a set of statistics to an input stream
        /// </summary>
        /// <param name="stream">Stream to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the report was written correctly, false otherwise</returns>
        public abstract bool WriteToStream(Stream stream, bool baddumpCol, bool nodumpCol, bool throwOnError = false);
    }
}
