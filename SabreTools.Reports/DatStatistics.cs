using SabreTools.DatFiles;

namespace SabreTools.Reports
{
    /// <summary>
    /// Statistics wrapper for outputting
    /// </summary>
    public class DatStatistics
    {
        /// <summary>
        /// ItemDictionary representing the statistics
        /// </summary>
        public ItemDictionary Statistics { get; set; }
        
        /// <summary>
        /// Name to display on output
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Total machine count to use on output
        /// </summary>
        public long MachineCount { get; set; }

        /// <summary>
        /// Determines if statistics are for a directory or not
        /// </summary>
        public bool IsDirectory { get; set; } = false;
    }
}
