namespace SabreTools.Reports
{
    /// <summary>
    /// Determine which format to output Stats to
    /// </summary>
    public enum StatReportFormat
    {
        /// <summary>
        /// Only output to the console
        /// </summary>
        None,

        /// <summary>
        /// Console-formatted
        /// </summary>
        Textfile,

        /// <summary>
        /// ClrMamePro HTML
        /// </summary>
        HTML,

        /// <summary>
        /// Comma-Separated Values (Standardized)
        /// </summary>
        CSV,

        /// <summary>
        /// Semicolon-Separated Values (Standardized)
        /// </summary>
        SSV,

        /// <summary>
        /// Tab-Separated Values (Standardized)
        /// </summary>
        TSV,
    }
}
