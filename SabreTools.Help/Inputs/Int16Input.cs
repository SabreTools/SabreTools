using System;
using System.Globalization;
using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents an Int16 flag with an optional trailing value
    /// </summary>
    public class Int16Input : Input<short?>
    {
        #region Properties

        /// <summary>
        /// Indicates a minimum value (inclusive) for the flag
        /// </summary>
        public short? MinValue { get; set; } = null;

        /// <summary>
        /// Indicates a maximum value (inclusive) for the flag
        /// </summary>
        public short? MaxValue { get; set; } = null;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public Int16Input(string name, string[] flags, string description)
            : base(name, flags, description) { }

        /// <inheritdoc/>
        public Int16Input(string name, string[] flags, string description, bool required)
            : base(name, flags, description, required) { }

        /// <inheritdoc/>
        public Int16Input(string name, string[] flags, string description, string longDescription, bool required)
            : base(name, flags, description, longDescription, required) { }

        #endregion

        /// <inheritdoc/>
        public override string Format(bool useEquals)
        {
            // Do not output if there is no value
            if (Value == null)
                return string.Empty;

            // Build the output format
            var builder = new StringBuilder();

            // Flag name
            builder.Append(_flags[0]);

            // Only output separator and value if needed
            if (_required || (!_required && Value != short.MinValue))
            {
                // Separator
                if (useEquals)
                    builder.Append("=");
                else
                    builder.Append(" ");

                // Value
                builder.Append(Value.ToString());
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        public override bool Process(string[] parts, ref int index)
        {
            // Check the parts array
            if (index < 0 || index >= parts.Length)
                return false;

            // Check for space-separated
            string part = parts[index];
            if (Array.FindIndex(_flags, n => n == part) > -1)
            {
                // Ensure the value exists
                if (index + 1 >= parts.Length)
                {
                    Value = _required ? null : short.MinValue;
                    Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                    Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                    return !_required;
                }

                // If the next value is valid
                if (ParseValue(parts[index + 1], out short? value) && value != null)
                {
                    index++;
                    Value = value;
                    Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                    Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                    return true;
                }

                // Return value based on required flag
                Value = _required ? null : short.MinValue;
                Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                return !_required;
            }

            // Check for equal separated
            if (Array.FindIndex(_flags, n => part.StartsWith($"{n}=")) > -1)
            {
                // Split the string, using the first equal sign as the separator
                string[] tempSplit = part.Split('=');
                string key = tempSplit[0];
                string val = string.Join("=", tempSplit, 1, tempSplit.Length - 1);

                // Ensure the value exists
                if (string.IsNullOrEmpty(val))
                {
                    Value = _required ? null : short.MinValue;
                    Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                    Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                    return !_required;
                }

                // If the next value is valid
                if (ParseValue(val, out short? value) && value != null)
                {
                    Value = value;
                    Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                    Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                    return true;
                }

                // Return value based on required flag
                Value = _required ? null : short.MinValue;
                Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                return !_required;
            }

            return false;
        }

        /// <summary>
        /// Parse a value from a string
        /// </summary>
        private static bool ParseValue(string str, out short? output)
        {
            // If the next value is valid
            if (short.TryParse(str, out short value))
            {
                output = value;
                return true;
            }

            // Try to process as a formatted string
            string baseVal = ExtractFactorFromValue(str, out long factor);
            if (short.TryParse(baseVal, out value))
            {
                output = (short)(value * factor);
                return true;
            }

            // Try to process as a hex string
            string hexValue = RemoveHexIdentifier(baseVal);
            if (short.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
            {
                output = (short)(value * factor);
                return true;
            }

            // The value could not be parsed
            output = null;
            return false;
        }
    }
}