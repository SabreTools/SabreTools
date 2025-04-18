using System;
using System.Text;

namespace SabreTools.Help
{
    public class FlagFeature : Feature<bool>
    {
        #region Constructors

        public FlagFeature(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
            Value = false;
        }

        public FlagFeature(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
            Value = false;
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public override bool ValidateInput(string input, bool exact = false, bool ignore = false)
        {
            // Pre-split the input for efficiency
            string[] splitInput = input.Split('=');

            bool valid = !input.Contains("=") && Flags.Contains(input);
            if (valid)
            {
                Value = true;

                // If we've already found this feature before
                if (_foundOnce && !ignore)
                    valid = false;

                _foundOnce = true;
            }

            // If we haven't found a valid flag and we're not looking for just this feature, check to see if any of the subfeatures are valid
            if (!valid && !exact)
            {
                string[] featureKeys = [.. Features.Keys];
                valid = Array.Exists(featureKeys, k => Features[k]!.ValidateInput(input));
            }

            return valid;
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
