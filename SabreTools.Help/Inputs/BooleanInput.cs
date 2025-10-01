using System;
using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a user input bounded to the range of <see cref="bool"/> 
    /// </summary>
    public class BooleanInput : UserInput<bool>
    {
        #region Constructors

        public BooleanInput(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
            Value = false;
        }

        public BooleanInput(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
            Value = false;
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
                if (!bool.TryParse(splitInput[1], out bool value))
                    value = false;

                Value = value;
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
        public override bool IsEnabled() => Value;

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
