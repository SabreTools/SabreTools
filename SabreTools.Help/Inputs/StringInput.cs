using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a string input with a single instance allowed
    /// </summary>
    public class StringInput : UserInput<string>
    {
        #region Constructors

        public StringInput(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
            Value = null;
        }

        public StringInput(string name, string[] flags, string description, string? longDescription = null)
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
            if (ContainsFlag(part) && !part.Contains("="))
            {
                // Ensure the value exists
                if (index + 1 >= args.Length)
                    return false;

                index++;
                Value = args[index];
                return true;
            }

            // Check for equal separated
            if (ContainsFlag(part) && part.Contains("="))
            {
                // Split the string, using the first equal sign as the separator
                string[] tempSplit = part.Split('=');
                string key = tempSplit[0];
                string val = string.Join("=", tempSplit, 1, tempSplit.Length - 1);

                Value = val;
                return true;
            }

            // If the current flag doesn't match, check to see if any of the subfeatures are valid
            foreach (var kvp in Children)
            {
                if (kvp.Value.ProcessInput(args, ref index))
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
