using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.Hashing;
using SabreTools.Metadata.DatFiles;
using SabreTools.Text.Extensions;
using ItemStatus = SabreTools.Data.Models.Metadata.ItemStatus;
using ItemType = SabreTools.Data.Models.Metadata.ItemType;

#pragma warning disable IDE0290 // Use primary constructor
namespace SabreTools.Reports.Formats
{
    /// <summary>
    /// Textfile report format
    /// </summary>
    public class Textfile : BaseReport
    {
        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="statsList">List of statistics objects to set</param>
        public Textfile(List<DatStatistics> statsList)
            : base(statsList)
        {
        }

        /// <inheritdoc/>
        public override bool WriteToStream(Stream stream, bool baddumpCol, bool nodumpCol, bool throwOnError = false)
        {
            try
            {
                StreamWriter sw = new(stream);

                // Now process each of the statistics
                for (int i = 0; i < _statistics.Count; i++)
                {
                    // Get the current statistic
                    DatStatistics stat = _statistics[i];

                    // If we have a directory statistic
                    if (stat.IsDirectory)
                    {
                        WriteIndividual(sw, stat, baddumpCol, nodumpCol);

                        // If we have anything but the last value, write the separator
                        if (i < _statistics.Count - 1)
                            WriteFooterSeparator(sw);
                    }

                    // If we have a normal statistic
                    else
                    {
                        WriteIndividual(sw, stat, baddumpCol, nodumpCol);
                    }
                }

                sw.Dispose();
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Error(ex);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Write a single set of statistics
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        /// <param name="stat">DatStatistics object to write out</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private static void WriteIndividual(StreamWriter sw, DatStatistics stat, bool baddumpCol, bool nodumpCol)
        {
            var line = new StringBuilder();

            line.AppendLine($"'{stat.DisplayName}':");
            line.AppendLine($"--------------------------------------------------");
            line.AppendLine($"    Uncompressed size:       {NumberHelper.GetBytesReadable(stat!.TotalSize)}");
            line.AppendLine($"    Games found:             {stat.MachineCount}");
            line.AppendLine($"    Roms found:              {stat.GetItemCount(ItemType.Rom)}");
            line.AppendLine($"    Disks found:             {stat.GetItemCount(ItemType.Disk)}");
            line.AppendLine($"    Roms with CRC:           {stat.GetHashCount(HashType.CRC32)}");
            line.AppendLine($"    Roms with MD5:           {stat.GetHashCount(HashType.MD5)}");
            line.AppendLine($"    Roms with SHA-1:         {stat.GetHashCount(HashType.SHA1)}");
            line.AppendLine($"    Roms with SHA-256:       {stat.GetHashCount(HashType.SHA256)}");
            line.AppendLine($"    Roms with SHA-384:       {stat.GetHashCount(HashType.SHA384)}");
            line.AppendLine($"    Roms with SHA-512:       {stat.GetHashCount(HashType.SHA512)}");

            if (baddumpCol)
                line.AppendLine($"    Roms with BadDump status: {stat.GetStatusCount(ItemStatus.BadDump)}");

            if (nodumpCol)
                line.AppendLine($"    Roms with Nodump status: {stat.GetStatusCount(ItemStatus.Nodump)}");

            // For spacing between DATs
            line.AppendLine();
            line.AppendLine();

            sw.Write(line.ToString());
            sw.Flush();
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        /// <param name="sw">StreamWriter to write to</param>
        private static void WriteFooterSeparator(StreamWriter sw)
        {
            sw.Write("\n");
            sw.Flush();
        }
    }
}
