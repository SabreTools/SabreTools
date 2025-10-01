using System.Text;

namespace SabreTools.Help.Inputs
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
            // Check for space-separated
            string part = args[index];
            if (Flags.FindIndex(n => n == part) > -1)
            {
                Value = true;
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
        public override bool IsEnabled() => Value;

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
