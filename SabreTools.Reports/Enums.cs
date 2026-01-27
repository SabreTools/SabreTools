using System;

namespace SabreTools.Reports
{
    /// <summary>
    /// Determine which format to output Stats to
    /// </summary>
    [Flags]
    public enum StatReportFormat
    {
        /// <summary>
        /// Only output to the console
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Console-formatted
        /// </summary>
        Textfile = 1 << 0,

        /// <summary>
        /// ClrMamePro HTML
        /// </summary>
        HTML = 1 << 1,

        /// <summary>
        /// Comma-Separated Values (Standardized)
        /// </summary>
        CSV = 1 << 2,

        /// <summary>
        /// Semicolon-Separated Values (Standardized)
        /// </summary>
        SSV = 1 << 3,

        /// <summary>
        /// Tab-Separated Values (Standardized)
        /// </summary>
        TSV = 1 << 4,

        All = int.MaxValue,
    }
}
