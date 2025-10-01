using System;
using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a boolean flag without a trailing value
    /// </summary>
    public class FlagInput : Input<bool>
    {
        #region Constructors

        /// <inheritdoc/>
        public FlagInput(string name, string[] flags, string description)
            : base(name, flags, description) { }

        /// <inheritdoc/>
        public FlagInput(string name, string[] flags, string description, bool required)
            : base(name, flags, description, required) { }

        /// <inheritdoc/>
        public FlagInput(string name, string[] flags, string description, string longDescription, bool required)
            : base(name, flags, description, longDescription, required) { }

        #endregion

        /// <inheritdoc/>
        public override string Format(bool useEquals)
        {
            // Do not output if there is no value
            if (Value == false)
                return string.Empty;

            // Build the output format
            var builder = new StringBuilder();

            // Flag name
            builder.Append(_flags[0]);

            return builder.ToString();
        }

        /// <inheritdoc/>
        protected override string FormatFlags()
        {
            var sb = new StringBuilder();
            Array.ForEach(_flags, flag => sb.Append($"{flag}, "));
            return sb.ToString().TrimEnd(' ', ',');
        }

        /// <inheritdoc/>
        public override bool Process(string[] parts, ref int index)
        {
            // Check the parts array
            if (index < 0 || index >= parts.Length)
                return false;

            // Check the name
            string part = parts[index];
            if (Array.FindIndex(_flags, n => n == part) > -1)
            {
                Value = true;
                return true;
            }

            return false;
        }
    }
}