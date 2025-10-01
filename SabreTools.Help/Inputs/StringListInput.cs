using System;
using System.Collections.Generic;
using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a string input with multiple instances allowed
    /// </summary>
    public class StringListInput : UserInput<List<string>>
    {
        #region Constructors

        public StringListInput(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
            Value = null;
        }

        public StringListInput(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
            Value = null;
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public override bool ValidateInput(string[] args, ref int index)
        {
            // Pre-split the input for efficiency
            string[] splitInput = args[index].Split('=');

            if (args[index].Contains("=") && Flags.Contains(splitInput[0]))
            {
                Value ??= [];
                Value.Add(string.Join("=", splitInput, 1, splitInput.Length - 1));
                return true;
            }

            // If the current flag doesn't match, check to see if any of the subfeatures are valid
            foreach (var kvp in Features)
            {
                if (kvp.Value.ValidateInput(args, ref index))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool IsEnabled() => Value != null;

        /// <inheritdoc/>
        protected override string FormatFlags()
        {
            var sb = new StringBuilder();
            Flags.ForEach(flag => sb.Append($"{flag}=, "));
            return sb.ToString().TrimEnd(' ', ',');
        }

        #endregion
    }
}
