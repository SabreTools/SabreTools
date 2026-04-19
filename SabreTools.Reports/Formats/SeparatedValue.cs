using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.Hashing;
using SabreTools.Metadata.DatFiles;
using SabreTools.Text.SeparatedValue;
using ItemStatus = SabreTools.Data.Models.Metadata.ItemStatus;
using ItemType = SabreTools.Data.Models.Metadata.ItemType;

#pragma warning disable IDE0290 // Use primary constructor
namespace SabreTools.Reports.Formats
{
    /// <summary>
    /// Separated-Value report format
    /// </summary>
    public abstract class SeparatedValue : BaseReport
    {
        /// <summary>
        /// Represents the delimiter between fields
        /// </summary>
        protected char _delim;

        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="statsList">List of statistics objects to set</param>
        public SeparatedValue(List<DatStatistics> statsList)
            : base(statsList)
        {
        }

        /// <inheritdoc/>
        public override bool WriteToStream(Stream stream, bool baddumpCol, bool nodumpCol, bool throwOnError = false)
        {
            try
            {
                Writer svw = new(stream, Encoding.UTF8)
                {
                    Separator = _delim,
                    Quotes = true,
                };

                // Write out the header
                WriteHeader(svw, baddumpCol, nodumpCol);

                // Now process each of the statistics
                for (int i = 0; i < _statistics.Count; i++)
                {
                    // Get the current statistic
                    DatStatistics stat = _statistics[i];

                    // If we have a directory statistic
                    if (stat.IsDirectory)
                    {
                        WriteIndividual(svw, stat, baddumpCol, nodumpCol);

                        // If we have anything but the last value, write the separator
                        if (i < _statistics.Count - 1)
                            WriteFooterSeparator(svw);
                    }

                    // If we have a normal statistic
                    else
                    {
                        WriteIndividual(svw, stat, baddumpCol, nodumpCol);
                    }
                }

                svw.Dispose();
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Error(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write out the header to the stream, if any exists
        /// </summary>
        /// <param name="svw">Writer to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private static void WriteHeader(Writer svw, bool baddumpCol, bool nodumpCol)
        {
            string[] headers =
            [
                "File Name",
                "Total Size",
                "Games",
                "Roms",
                "Disks",
                "# with CRC",
                "# with MD5",
                "# with SHA-1",
                "# with SHA-256",
                "# with SHA-384",
                "# with SHA-512",
                baddumpCol ? "BadDumps" : string.Empty,
                nodumpCol ? "Nodumps" : string.Empty,
            ];
            svw.WriteHeader(headers);
            svw.Flush();
        }

        /// <summary>
        /// Write a single set of statistics
        /// </summary>
        /// <param name="svw">Writer to write to</param>
        /// <param name="stat">DatStatistics object to write out</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private static void WriteIndividual(Writer svw, DatStatistics stat, bool baddumpCol, bool nodumpCol)
        {
            string[] values =
            [
                stat.DisplayName!,
                stat.TotalSize.ToString(),
                stat.MachineCount.ToString(),
                stat.GetItemCount(ItemType.Rom).ToString(),
                stat.GetItemCount(ItemType.Disk).ToString(),
                stat.GetHashCount(HashType.CRC32).ToString(),
                stat.GetHashCount(HashType.MD5).ToString(),
                stat.GetHashCount(HashType.SHA1).ToString(),
                stat.GetHashCount(HashType.SHA256).ToString(),
                stat.GetHashCount(HashType.SHA384).ToString(),
                stat.GetHashCount(HashType.SHA512).ToString(),
                baddumpCol ? stat.GetStatusCount(ItemStatus.BadDump).ToString() : string.Empty,
                nodumpCol ? stat.GetStatusCount(ItemStatus.Nodump).ToString() : string.Empty,
            ];
            svw.WriteValues(values);
            svw.Flush();
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        /// <param name="svw">Writer to write to</param>
        private static void WriteFooterSeparator(Writer svw)
        {
            svw.WriteString("\n");
            svw.Flush();
        }
    }

    /// <summary>
    /// Represents a comma-separated value file
    /// </summary>
    public sealed class CommaSeparatedValue : SeparatedValue
    {
        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="statsList">List of statistics objects to set</param>
        public CommaSeparatedValue(List<DatStatistics> statsList) : base(statsList)
        {
            _delim = ',';
        }
    }

    /// <summary>
    /// Represents a semicolon-separated value file
    /// </summary>
    public sealed class SemicolonSeparatedValue : SeparatedValue
    {
        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="statsList">List of statistics objects to set</param>
        public SemicolonSeparatedValue(List<DatStatistics> statsList) : base(statsList)
        {
            _delim = ';';
        }
    }

    /// <summary>
    /// Represents a tab-separated value file
    /// </summary>
    public sealed class TabSeparatedValue : SeparatedValue
    {
        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="statsList">List of statistics objects to set</param>
        public TabSeparatedValue(List<DatStatistics> statsList) : base(statsList)
        {
            _delim = '\t';
        }
    }
}
