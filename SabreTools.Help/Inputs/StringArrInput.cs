using System;
using System.Collections.Generic;
using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents an string flag with an optional trailing value
    /// </summary>
    public class StringArrInput : Input<List<string?>>
    {
        #region Constructors

        /// <inheritdoc/>
        public StringArrInput(string name, string[] flags, string description)
            : base(name, flags, description) { }

        /// <inheritdoc/>
        public StringArrInput(string name, string[] flags, string description, bool required)
            : base(name, flags, description, required) { }

        /// <inheritdoc/>
        public StringArrInput(string name, string[] flags, string description, string longDescription, bool required)
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
            if (_required || (!_required && Value != null))
            {
                // Separator
                if (useEquals)
                    builder.Append("=");
                else
                    builder.Append(" ");

                // Value
                List<string?> nonNull = Value.FindAll(i => i != null);
                List<string> stringValues = nonNull.ConvertAll(i => i ?? string.Empty);
                string[] stringArr = [.. stringValues];
                builder.Append(string.Join(" ", stringArr));
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        public override bool Process(string[] parts, ref int index)
        {
            // Check the parts array
            if (index < 0 || index >= parts.Length)
                return false;

            // Ensure the value list exists
            Value ??= [];

            // Check for space-separated
            string part = parts[index];
            if (Array.FindIndex(_flags, n => n == part) > -1)
            {
                // Ensure the value exists
                if (index + 1 >= parts.Length)
                {
                    Value.Add(_required ? null : string.Empty);
                    return !_required;
                }

                index++;
                Value.Add(parts[index].Trim('"'));
                return true;
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
                    Value.Add(_required ? null : string.Empty);
                    return !_required;
                }

                Value.Add(val.Trim('"'));
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        protected override string FormatFlags()
        {
            var sb = new StringBuilder();
            Array.ForEach(_flags, flag => sb.Append($"{flag}=, "));
            return sb.ToString().TrimEnd(' ', ',');
        }
    }
}