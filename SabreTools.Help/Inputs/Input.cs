using System;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a single input for an execution context
    /// </summary>
    public abstract class Input
    {
        #region Properties

        /// <summary>
        /// Display name for the input
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Set of flags for the input
        /// </summary>
        protected readonly string[] _flags;

        /// <summary>
        /// Short description for printing
        /// </summary>
        protected readonly string _description;

        /// <summary>
        /// Optional extended description for printing
        /// </summary>
        protected readonly string? _longDescription;

        /// <summary>
        /// Indicates if the value following is required or not
        /// </summary>
        protected readonly bool _required;

        /// <summary>
        /// Indicates if a value has been set
        /// </summary>
        public abstract bool ValueSet { get; }

        #endregion

        #region Constructors

        public Input(string name, string[] flags, string description)
        {
            if (flags.Length == 0)
                throw new ArgumentException($"{nameof(flags)} requires at least one value");

            Name = name;
            _flags = flags;
            _description = description;
            _longDescription = null;
            _required = true;
        }

        public Input(string name, string[] flags, string description, bool required)
        {
            if (flags.Length == 0)
                throw new ArgumentException($"{nameof(flags)} requires at least one value");

            Name = name;
            _flags = flags;
            _description = description;
            _longDescription = null;
            _required = required;
        }

        public Input(string name, string[] flags, string description, string longDescription, bool required)
        {
            if (flags.Length == 0)
                throw new ArgumentException($"{nameof(flags)} requires at least one value");

            Name = name;
            _flags = flags;
            _description = description;
            _longDescription = longDescription;
            _required = required;
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Clear any accumulated value
        /// </summary>
        public abstract void ClearValue();

        /// <summary>
        /// Create a formatted representation of the input and possible value
        /// </summary>
        /// <param name="useEquals">Use an equal sign as a separator on output</param>
        public abstract string Format(bool useEquals);

        /// <summary>
        /// Process the current index, if possible
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>True if a value could be determined, false otherwise</returns>
        public abstract bool Process(string[] parts, ref int index);

        #endregion

        #region Helpers

        /// <summary>
        /// Get the trimmed value and multiplication factor from a value
        /// </summary>
        /// <param name="value">String value to treat as suffixed number</param>
        /// <returns>Trimmed value and multiplication factor</returns>
        internal static string ExtractFactorFromValue(string value, out long factor)
        {
            value = value.Trim('"');
            factor = 1;

            // Characters
            if (value.EndsWith("c", StringComparison.Ordinal))
            {
                factor = 1;
                value = value.TrimEnd('c');
            }

            // Words
            else if (value.EndsWith("w", StringComparison.Ordinal))
            {
                factor = 2;
                value = value.TrimEnd('w');
            }

            // Double Words
            else if (value.EndsWith("d", StringComparison.Ordinal))
            {
                factor = 4;
                value = value.TrimEnd('d');
            }

            // Quad Words
            else if (value.EndsWith("q", StringComparison.Ordinal))
            {
                factor = 8;
                value = value.TrimEnd('q');
            }

            // Kilobytes
            else if (value.EndsWith("k", StringComparison.Ordinal))
            {
                factor = 1024;
                value = value.TrimEnd('k');
            }

            // Megabytes
            else if (value.EndsWith("M", StringComparison.Ordinal))
            {
                factor = 1024 * 1024;
                value = value.TrimEnd('M');
            }

            // Gigabytes
            else if (value.EndsWith("G", StringComparison.Ordinal))
            {
                factor = 1024 * 1024 * 1024;
                value = value.TrimEnd('G');
            }

            return value;
        }

        /// <summary>
        /// Removes a leading 0x if it exists, case insensitive
        /// </summary>
        /// <param name="value">String with removed leading 0x</param>
        /// <returns></returns>
        internal static string RemoveHexIdentifier(string value)
        {
            if (value.Length <= 2)
                return value;
            if (value[0] != '0')
                return value;
            if (value[1] != 'x' && value[1] != 'X')
                return value;

            return value.Substring(2);
        }

        #endregion
    }
}
