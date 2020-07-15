using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Reports;
using SabreTools.Library.Tools;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents statistical data associated with a DAT
    /// </summary>
    public class DatStats
    {
        #region Private instance variables

        /// <summary>
        /// Object used to lock stats updates
        /// </summary>
        private readonly object _lockObject = new object();

        #endregion

        #region Publicly facing variables

        /// <summary>
        /// Statistics writing format
        /// </summary>
        public StatReportFormat ReportFormat { get; set; } = StatReportFormat.None;

        /// <summary>
        /// Statistics output file name
        /// </summary>
        public string ReportName { get; set; } = "report";

        /// <summary>
        /// Overall item count
        /// </summary>
        public long Count { get; set; } = 0;

        /// <summary>
        /// Number of Archive items
        /// </summary>
        public long ArchiveCount { get; set; } = 0;

        /// <summary>
        /// Number of BiosSet items
        /// </summary>
        public long BiosSetCount { get; set; } = 0;

        /// <summary>
        /// Number of Disk items
        /// </summary>
        public long DiskCount { get; set; } = 0;

        /// <summary>
        /// Number of Release items
        /// </summary>
        public long ReleaseCount { get; set; } = 0;

        /// <summary>
        /// Number of Rom items
        /// </summary>
        public long RomCount { get; set; } = 0;

        /// <summary>
        /// Number of Sample items
        /// </summary>
        public long SampleCount { get; set; } = 0;

        /// <summary>
        /// Number of machines
        /// </summary>
        /// <remarks>Special count only used by statistics output</remarks>
        public long GameCount { get; set; } = 0;

        /// <summary>
        /// Total uncompressed size
        /// </summary>
        public long TotalSize { get; set; } = 0;

        /// <summary>
        /// Number of items with a CRC hash
        /// </summary>
        public long CRCCount { get; set; } = 0;

        /// <summary>
        /// Number of items with an MD5 hash
        /// </summary>
        public long MD5Count { get; set; } = 0;

#if NET_FRAMEWORK
        /// <summary>
        /// Number of items with a RIPEMD160 hash
        /// </summary>
        public long RIPEMD160Count { get; set; } = 0;
#endif

        /// <summary>
        /// Number of items with a SHA-1 hash
        /// </summary>
        public long SHA1Count { get; set; } = 0;

        /// <summary>
        /// Number of items with a SHA-256 hash
        /// </summary>
        public long SHA256Count { get; set; } = 0;

        /// <summary>
        /// Number of items with a SHA-384 hash
        /// </summary>
        public long SHA384Count { get; set; } = 0;

        /// <summary>
        /// Number of items with a SHA-512 hash
        /// </summary>
        public long SHA512Count { get; set; } = 0;

        /// <summary>
        /// Number of items with the baddump status
        /// </summary>
        public long BaddumpCount { get; set; } = 0;

        /// <summary>
        /// Number of items with the good status
        /// </summary>
        public long GoodCount { get; set; } = 0;

        /// <summary>
        /// Number of items with the nodump status
        /// </summary>
        public long NodumpCount { get; set; } = 0;

        /// <summary>
        /// Number of items with the verified status
        /// </summary>
        public long VerifiedCount { get; set; } = 0;

        #endregion

        #region Instance Methods

        #region Updates

        /// <summary>
        /// Add to the statistics given a DatItem
        /// </summary>
        /// <param name="item">Item to add info from</param>
        public void AddItem(DatItem item)
        {
            // No matter what the item is, we increate the count
            lock (_lockObject)
            {
                this.Count += 1;

                // Now we do different things for each item type

                switch (item.ItemType)
                {
                    case ItemType.Archive:
                        this.ArchiveCount += 1;
                        break;
                    case ItemType.BiosSet:
                        this.BiosSetCount += 1;
                        break;
                    case ItemType.Disk:
                        this.DiskCount += 1;
                        if (((Disk)item).ItemStatus != ItemStatus.Nodump)
                        {
                            this.MD5Count += (string.IsNullOrWhiteSpace(((Disk)item).MD5) ? 0 : 1);
#if NET_FRAMEWORK
                            this.RIPEMD160Count += (string.IsNullOrWhiteSpace(((Disk)item).RIPEMD160) ? 0 : 1);
#endif
                            this.SHA1Count += (string.IsNullOrWhiteSpace(((Disk)item).SHA1) ? 0 : 1);
                            this.SHA256Count += (string.IsNullOrWhiteSpace(((Disk)item).SHA256) ? 0 : 1);
                            this.SHA384Count += (string.IsNullOrWhiteSpace(((Disk)item).SHA384) ? 0 : 1);
                            this.SHA512Count += (string.IsNullOrWhiteSpace(((Disk)item).SHA512) ? 0 : 1);
                        }

                        this.BaddumpCount += (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
                        this.GoodCount += (((Disk)item).ItemStatus == ItemStatus.Good ? 1 : 0);
                        this.NodumpCount += (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
                        this.VerifiedCount += (((Disk)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
                        break;
                    case ItemType.Release:
                        this.ReleaseCount += 1;
                        break;
                    case ItemType.Rom:
                        this.RomCount += 1;
                        if (((Rom)item).ItemStatus != ItemStatus.Nodump)
                        {
                            this.TotalSize += ((Rom)item).Size;
                            this.CRCCount += (string.IsNullOrWhiteSpace(((Rom)item).CRC) ? 0 : 1);
                            this.MD5Count += (string.IsNullOrWhiteSpace(((Rom)item).MD5) ? 0 : 1);
#if NET_FRAMEWORK
                            this.RIPEMD160Count += (string.IsNullOrWhiteSpace(((Rom)item).RIPEMD160) ? 0 : 1);
#endif
                            this.SHA1Count += (string.IsNullOrWhiteSpace(((Rom)item).SHA1) ? 0 : 1);
                            this.SHA256Count += (string.IsNullOrWhiteSpace(((Rom)item).SHA256) ? 0 : 1);
                            this.SHA384Count += (string.IsNullOrWhiteSpace(((Rom)item).SHA384) ? 0 : 1);
                            this.SHA512Count += (string.IsNullOrWhiteSpace(((Rom)item).SHA512) ? 0 : 1);
                        }

                        this.BaddumpCount += (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
                        this.GoodCount += (((Rom)item).ItemStatus == ItemStatus.Good ? 1 : 0);
                        this.NodumpCount += (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
                        this.VerifiedCount += (((Rom)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
                        break;
                    case ItemType.Sample:
                        this.SampleCount += 1;
                        break;
                }
            }
        }

        /// <summary>
        /// Add statistics from another DatStats object
        /// </summary>
        /// <param name="stats">DatStats object to add from</param>
        public void AddStats(DatStats stats)
        {
            this.Count += stats.Count;

            this.ArchiveCount += stats.ArchiveCount;
            this.BiosSetCount += stats.BiosSetCount;
            this.DiskCount += stats.DiskCount;
            this.ReleaseCount += stats.ReleaseCount;
            this.RomCount += stats.RomCount;
            this.SampleCount += stats.SampleCount;

            this.GameCount += stats.GameCount;

            this.TotalSize += stats.TotalSize;

            // Individual hash counts
            this.CRCCount += stats.CRCCount;
            this.MD5Count += stats.MD5Count;
#if NET_FRAMEWORK
            this.RIPEMD160Count += stats.RIPEMD160Count;
#endif
            this.SHA1Count += stats.SHA1Count;
            this.SHA256Count += stats.SHA256Count;
            this.SHA384Count += stats.SHA384Count;
            this.SHA512Count += stats.SHA512Count;

            // Individual status counts
            this.BaddumpCount += stats.BaddumpCount;
            this.GoodCount += stats.GoodCount;
            this.NodumpCount += stats.NodumpCount;
            this.VerifiedCount += stats.VerifiedCount;
        }

        /// <summary>
        /// Get the highest-order BucketedBy value that represents the statistics
        /// </summary>
        public BucketedBy GetBestAvailable()
        {
            // If all items are supposed to have a SHA-512, we sort by that
            if (RomCount + DiskCount - NodumpCount == SHA512Count)
                return BucketedBy.SHA512;

            // If all items are supposed to have a SHA-384, we sort by that
            else if (RomCount + DiskCount - NodumpCount == SHA384Count)
                return BucketedBy.SHA384;

            // If all items are supposed to have a SHA-256, we sort by that
            else if (RomCount + DiskCount - NodumpCount == SHA256Count)
                return BucketedBy.SHA256;

            // If all items are supposed to have a SHA-1, we sort by that
            else if (RomCount + DiskCount - NodumpCount == SHA1Count)
                return BucketedBy.SHA1;

#if NET_FRAMEWORK
            // If all items are supposed to have a RIPEMD160, we sort by that
            else if (RomCount + DiskCount - NodumpCount == RIPEMD160Count)
                return BucketedBy.RIPEMD160;
#endif

            // If all items are supposed to have a MD5, we sort by that
            else if (RomCount + DiskCount - NodumpCount == MD5Count)
                return BucketedBy.MD5;

            // Otherwise, we sort by CRC
            else
                return BucketedBy.CRC;
        }

        /// <summary>
        /// Remove from the statistics given a DatItem
        /// </summary>
        /// <param name="item">Item to remove info for</param>
        public void RemoveItem(DatItem item)
        {
            // No matter what the item is, we increate the count
            lock (_lockObject)
            {
                this.Count -= 1;

                // Now we do different things for each item type

                switch (item.ItemType)
                {
                    case ItemType.Archive:
                        this.ArchiveCount -= 1;
                        break;
                    case ItemType.BiosSet:
                        this.BiosSetCount -= 1;
                        break;
                    case ItemType.Disk:
                        this.DiskCount -= 1;
                        if (((Disk)item).ItemStatus != ItemStatus.Nodump)
                        {
                            this.MD5Count -= (string.IsNullOrWhiteSpace(((Disk)item).MD5) ? 0 : 1);
#if NET_FRAMEWORK
                            this.RIPEMD160Count -= (string.IsNullOrWhiteSpace(((Disk)item).RIPEMD160) ? 0 : 1);
#endif
                            this.SHA1Count -= (string.IsNullOrWhiteSpace(((Disk)item).SHA1) ? 0 : 1);
                            this.SHA256Count -= (string.IsNullOrWhiteSpace(((Disk)item).SHA256) ? 0 : 1);
                            this.SHA384Count -= (string.IsNullOrWhiteSpace(((Disk)item).SHA384) ? 0 : 1);
                            this.SHA512Count -= (string.IsNullOrWhiteSpace(((Disk)item).SHA512) ? 0 : 1);
                        }

                        this.BaddumpCount -= (((Disk)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
                        this.GoodCount -= (((Disk)item).ItemStatus == ItemStatus.Good ? 1 : 0);
                        this.NodumpCount -= (((Disk)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
                        this.VerifiedCount -= (((Disk)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
                        break;
                    case ItemType.Release:
                        this.ReleaseCount -= 1;
                        break;
                    case ItemType.Rom:
                        this.RomCount -= 1;
                        if (((Rom)item).ItemStatus != ItemStatus.Nodump)
                        {
                            this.TotalSize -= ((Rom)item).Size;
                            this.CRCCount -= (string.IsNullOrWhiteSpace(((Rom)item).CRC) ? 0 : 1);
                            this.MD5Count -= (string.IsNullOrWhiteSpace(((Rom)item).MD5) ? 0 : 1);
#if NET_FRAMEWORK
                            this.RIPEMD160Count -= (string.IsNullOrWhiteSpace(((Rom)item).RIPEMD160) ? 0 : 1);
#endif
                            this.SHA1Count -= (string.IsNullOrWhiteSpace(((Rom)item).SHA1) ? 0 : 1);
                            this.SHA256Count -= (string.IsNullOrWhiteSpace(((Rom)item).SHA256) ? 0 : 1);
                            this.SHA384Count -= (string.IsNullOrWhiteSpace(((Rom)item).SHA384) ? 0 : 1);
                            this.SHA512Count -= (string.IsNullOrWhiteSpace(((Rom)item).SHA512) ? 0 : 1);
                        }

                        this.BaddumpCount -= (((Rom)item).ItemStatus == ItemStatus.BadDump ? 1 : 0);
                        this.GoodCount -= (((Rom)item).ItemStatus == ItemStatus.Good ? 1 : 0);
                        this.NodumpCount -= (((Rom)item).ItemStatus == ItemStatus.Nodump ? 1 : 0);
                        this.VerifiedCount -= (((Rom)item).ItemStatus == ItemStatus.Verified ? 1 : 0);
                        break;
                    case ItemType.Sample:
                        this.SampleCount -= 1;
                        break;
                }
            }
        }

        /// <summary>
        /// Reset all statistics
        /// </summary>
        public void Reset()
        {
            this.Count = 0;

            this.ArchiveCount = 0;
            this.BiosSetCount = 0;
            this.DiskCount = 0;
            this.ReleaseCount = 0;
            this.RomCount = 0;
            this.SampleCount = 0;

            this.GameCount = 0;

            this.TotalSize = 0;

            this.CRCCount = 0;
            this.MD5Count = 0;
#if NET_FRAMEWORK
            this.RIPEMD160Count = 0;
#endif
            this.SHA1Count = 0;
            this.SHA256Count = 0;
            this.SHA384Count = 0;
            this.SHA512Count = 0;

            this.BaddumpCount = 0;
            this.GoodCount = 0;
            this.NodumpCount = 0;
            this.VerifiedCount = 0;
        }

        #endregion

        #region Writing

        /// <summary>
        /// Output the stats for a list of input dats as files in a human-readable format
        /// </summary>
        /// <param name="inputs">List of input files and folders</param>
        /// <param name="reportName">Name of the output file</param>
        /// <param name="single">True if single DAT stats are output, false otherwise</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        /// <param name="statDatFormat" > Set the statistics output format to use</param>
        public static void OutputStats(List<string> inputs, string reportName, string outDir, bool single,
            bool baddumpCol, bool nodumpCol, StatReportFormat statDatFormat)
        {
            // If there's no output format, set the default
            if (statDatFormat == StatReportFormat.None)
                statDatFormat = StatReportFormat.Textfile;

            // Get the proper output file name
            if (string.IsNullOrWhiteSpace(reportName))
                reportName = "report";

            // Get the proper output directory name
            outDir = DirectoryExtensions.Ensure(outDir);

            // Get the dictionary of desired output report names
            Dictionary<StatReportFormat, string> outputs = DatStats.CreateOutStatsNames(outDir, statDatFormat, reportName);

            // Make sure we have all files and then order them
            List<string> files = DirectoryExtensions.GetFilesOnly(inputs);
            files = files
                .OrderBy(i => Path.GetDirectoryName(i))
                .ThenBy(i => Path.GetFileName(i))
                .ToList();

            // Get all of the writers that we need
            List<BaseReport> reports = outputs.Select(kvp => BaseReport.Create(kvp.Key, kvp.Value, baddumpCol, nodumpCol)).ToList();

            // Write the header, if any
            reports.ForEach(report => report.WriteHeader());

            // Init all total variables
            DatStats totalStats = new DatStats();

            // Init directory-level variables
            string lastdir = null;
            string basepath = null;
            DatStats dirStats = new DatStats();

            // Now process each of the input files
            foreach (string file in files)
            {
                // Get the directory for the current file
                string thisdir = Path.GetDirectoryName(file);
                basepath = Path.GetDirectoryName(Path.GetDirectoryName(file));

                // If we don't have the first file and the directory has changed, show the previous directory stats and reset
                if (lastdir != null && thisdir != lastdir)
                {
                    // Output separator if needed
                    reports.ForEach(report => report.WriteMidSeparator());

                    DatFile lastdirdat = DatFile.Create();

                    reports.ForEach(report => report.ReplaceStatistics($"DIR: {WebUtility.HtmlEncode(lastdir)}", dirStats.GameCount, dirStats));
                    reports.ForEach(report => report.Write());

                    // Write the mid-footer, if any
                    reports.ForEach(report => report.WriteFooterSeparator());

                    // Write the header, if any
                    reports.ForEach(report => report.WriteMidHeader());

                    // Reset the directory stats
                    dirStats.Reset();
                }

                Globals.Logger.Verbose($"Beginning stat collection for '{file}'", false);
                List<string> games = new List<string>();
                DatFile datdata = DatFile.CreateAndParse(file);
                datdata.BucketBy(BucketedBy.Game, DedupeType.None, norename: true);

                // Output single DAT stats (if asked)
                Globals.Logger.User($"Adding stats for file '{file}'\n", false);
                if (single)
                {
                    reports.ForEach(report => report.ReplaceStatistics(datdata.DatHeader.FileName, datdata.Keys.Count, datdata.DatStats));
                    reports.ForEach(report => report.Write());
                }

                // Add single DAT stats to dir
                dirStats.AddStats(datdata.DatStats);
                dirStats.GameCount += datdata.Keys.Count();

                // Add single DAT stats to totals
                totalStats.AddStats(datdata.DatStats);
                totalStats.GameCount += datdata.Keys.Count();

                // Make sure to assign the new directory
                lastdir = thisdir;
            }

            // Output the directory stats one last time
            reports.ForEach(report => report.WriteMidSeparator());

            if (single)
            {
                reports.ForEach(report => report.ReplaceStatistics($"DIR: {WebUtility.HtmlEncode(lastdir)}", dirStats.GameCount, dirStats));
                reports.ForEach(report => report.Write());
            }

            // Write the mid-footer, if any
            reports.ForEach(report => report.WriteFooterSeparator());

            // Write the header, if any
            reports.ForEach(report => report.WriteMidHeader());

            // Reset the directory stats
            dirStats.Reset();

            // Output total DAT stats
            reports.ForEach(report => report.ReplaceStatistics("DIR: All DATs", totalStats.GameCount, totalStats));
            reports.ForEach(report => report.Write());

            // Output footer if needed
            reports.ForEach(report => report.WriteFooter());

            Globals.Logger.User(@"
Please check the log folder if the stats scrolled offscreen", false);
        }

        /// <summary>
        /// Get the proper extension for the stat output format
        /// </summary>
        /// <param name="outDir">Output path to use</param>
        /// <param name="statDatFormat">StatDatFormat to get the extension for</param>
        /// <param name="reportName">Name of the input file to use</param>
        /// <returns>Dictionary of output formats mapped to file names</returns>
        private static Dictionary<StatReportFormat, string> CreateOutStatsNames(string outDir, StatReportFormat statDatFormat, string reportName, bool overwrite = true)
        {
            Dictionary<StatReportFormat, string> output = new Dictionary<StatReportFormat, string>();

            // First try to create the output directory if we need to
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            // Double check the outDir for the end delim
            if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                outDir += Path.DirectorySeparatorChar;

            // For each output format, get the appropriate stream writer
            if (statDatFormat.HasFlag(StatReportFormat.None))
                output.Add(StatReportFormat.None, CreateOutStatsNamesHelper(outDir, ".null", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.Textfile))
                output.Add(StatReportFormat.Textfile, CreateOutStatsNamesHelper(outDir, ".txt", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.CSV))
                output.Add(StatReportFormat.CSV, CreateOutStatsNamesHelper(outDir, ".csv", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.HTML))
                output.Add(StatReportFormat.HTML, CreateOutStatsNamesHelper(outDir, ".html", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.SSV))
                output.Add(StatReportFormat.SSV, CreateOutStatsNamesHelper(outDir, ".ssv", reportName, overwrite));

            if (statDatFormat.HasFlag(StatReportFormat.TSV))
                output.Add(StatReportFormat.TSV, CreateOutStatsNamesHelper(outDir, ".tsv", reportName, overwrite));

            return output;
        }

        /// <summary>
        /// Help generating the outstats name
        /// </summary>
        /// <param name="outDir">Output directory</param>
        /// <param name="extension">Extension to use for the file</param>
        /// <param name="reportName">Name of the input file to use</param>
        /// <param name="overwrite">True if we ignore existing files, false otherwise</param>
        /// <returns>String containing the new filename</returns>
        private static string CreateOutStatsNamesHelper(string outDir, string extension, string reportName, bool overwrite)
        {
            string outfile = outDir + reportName + extension;
            outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());

            if (!overwrite)
            {
                int i = 1;
                while (File.Exists(outfile))
                {
                    outfile = $"{outDir}{reportName}_{i}{extension}";
                    outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());
                    i++;
                }
            }

            return outfile;
        }


        #endregion

        #endregion // Instance Methods
    }
}
