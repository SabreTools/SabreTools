using System;
using System.Collections.Generic;
using System.Text;

namespace SabreTools.Help
{
    public class ListFeature : Feature<List<string>>
    {
        #region Constructors

        public ListFeature(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
        }

        public ListFeature(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public override bool ValidateInput(string input, bool exact = false, bool ignore = false)
        {
            // Pre-split the input for efficiency
            string[] splitInput = input.Split('=');

            bool valid = input.Contains("=") && Flags.Contains(splitInput[0]);
            if (valid)
            {
                Value ??= [];
                Value.Add(string.Join("=", splitInput, 1, splitInput.Length - 1));
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
