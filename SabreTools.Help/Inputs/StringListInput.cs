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
        public override bool ProcessInput(string[] args, ref int index)
        {
            // Check for space-separated
            string part = args[index];
            if (Flags.FindIndex(n => n == part) > -1)
            {
                // Ensure the value exists
                if (index + 1 >= args.Length)
                    return false;

                index++;
                Value ??= [];
                Value.Add(args[index]);
                return true;
            }

            // Check for equal separated
            if (Flags.FindIndex(n => part.StartsWith($"{n}=")) > -1)
            {
                // Split the string, using the first equal sign as the separator
                string[] tempSplit = part.Split('=');
                string key = tempSplit[0];
                string val = string.Join("=", tempSplit, 1, tempSplit.Length - 1);

                Value ??= [];
                Value.Add(val);
                return true;
            }

            // If the current flag doesn't match, check to see if any of the subfeatures are valid
            foreach (var kvp in Features)
            {
                if (kvp.Value.ProcessInput(args, ref index))
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
