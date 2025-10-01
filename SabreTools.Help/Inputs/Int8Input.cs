using System;
using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a user input bounded to the range of <see cref="sbyte"/> 
    /// </summary>
    public class Int8Input : UserInput<sbyte>
    {
        #region Constructors

        public Int8Input(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
            Value = sbyte.MinValue;
        }

        public Int8Input(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
            Value = sbyte.MinValue;
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
                if (!sbyte.TryParse(splitInput[1], out sbyte value))
                    value = sbyte.MinValue;

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
        public override bool IsEnabled() => Value != sbyte.MinValue;

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
