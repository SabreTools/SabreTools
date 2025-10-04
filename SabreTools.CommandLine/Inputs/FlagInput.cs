using System.Text;

namespace SabreTools.CommandLine.Inputs
{
    /// <summary>
    /// Represents a boolean user input
    /// </summary>
    public class FlagInput : UserInput<bool>
    {
        #region Constructors

        public FlagInput(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
            Value = false;
        }

        public FlagInput(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
            Value = false;
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public override bool ProcessInput(string[] args, ref int index)
        {
            // If the index is invalid
            if (index < 0 || index >= args.Length)
                return false;

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

                return false;
            }

            Value = true;
            return true;
        }

        /// <inheritdoc/>
        protected override string FormatFlags()
        {
            var sb = new StringBuilder();
            Flags.ForEach(flag => sb.Append($"{flag}, "));
            return sb.ToString().TrimEnd(' ', ',');
        }

        #endregion
    }
}
