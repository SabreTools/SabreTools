using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a user input bounded to the range of <see cref="int"/> 
    /// </summary>
    public class Int32Input : UserInput<int>
    {
        #region Constructors

        public Int32Input(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
            Value = int.MinValue;
        }

        public Int32Input(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
            Value = int.MinValue;
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public override bool ValidateInput(string[] args, ref int index)
        {
            // Check for space-separated
            string part = args[index];
            if (Flags.FindIndex(n => n == part) > -1)
            {
                // Ensure the value exists
                if (index + 1 >= args.Length)
                    return false;

                // If the next value is valid
                if (!int.TryParse(args[index + 1], out int value))
                    value = int.MinValue;

                index++;
                Value = value;
                return true;
            }

            // Check for equal separated
            if (Flags.FindIndex(n => part.StartsWith($"{n}=")) > -1)
            {
                // Split the string, using the first equal sign as the separator
                string[] tempSplit = part.Split('=');
                string key = tempSplit[0];
                string val = string.Join("=", tempSplit, 1, tempSplit.Length - 1);

                // Ensure the value exists
                if (string.IsNullOrEmpty(val))
                    return false;

                // If the next value is valid
                if (!int.TryParse(val, out int value))
                    value = int.MinValue;

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
        public override bool IsEnabled() => Value != int.MinValue;

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
