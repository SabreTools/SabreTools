using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a user input bounded to the range of <see cref="short"/> 
    /// </summary>
    public class Int16Input : UserInput<short>
    {
        #region Constructors

        public Int16Input(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
            Value = short.MinValue;
        }

        public Int16Input(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
            Value = short.MinValue;
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public override bool ProcessInput(string[] args, ref int index)
        {
            // Get the current part
            string part = args[index];

            // If the current flag doesn't match, check to see if any of the subfeatures are valid
            if (!ContainsFlag(part))
            {
                foreach (var kvp in Children)
                {
                    if (kvp.Value.ProcessInput(args, ref index))
                        return true;
                }
            }

            // Check for space-separated
            if (!part.Contains("="))
            {
                // Ensure the value exists
                if (index + 1 >= args.Length)
                    return false;

                // If the next value is valid
                if (!short.TryParse(args[index + 1], out short value))
                    return false;

                index++;
                Value = value;
                return true;
            }

            // Check for equal separated
            if (part.Contains("="))
            {
                // Split the string, using the first equal sign as the separator
                string[] tempSplit = part.Split('=');
                string val = string.Join("=", tempSplit, 1, tempSplit.Length - 1);

                // Ensure the value exists
                if (string.IsNullOrEmpty(val))
                    return false;

                // If the next value is valid
                if (!short.TryParse(val, out short value))
                    return false;

                Value = value;
                return true;
            }

            return false;
        }

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
