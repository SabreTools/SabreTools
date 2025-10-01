using System.Collections.Generic;
using SabreTools.Help.Inputs;

namespace SabreTools.Help
{
    /// <summary>
    /// Default extended help feature implementation
    /// </summary>
    public class DefaultHelpExtended : Feature
    {
        public const string DisplayName = "Help (Detailed)";

        private static readonly string[] _flags = ["??", "hd", "help-detailed"];

        private const string _description = "Show this detailed help";

        private const string _longDescription = "Display a detailed help text to the screen.";

        public DefaultHelpExtended()
            : base(DisplayName, _flags, _description, _longDescription)
        {
        }

        /// <inheritdoc/>
        public override bool ProcessArgs(string[] args, FeatureSet parentSet)
        {
            // If we had something else after help
            if (args.Length > 1)
            {
                parentSet.OutputIndividualFeature(args[1], includeLongDescription: true);
                return true;
            }

            // Otherwise, show generic help
            else
            {
                parentSet.OutputAllHelp();
                return true;
            }
        }
    
        /// <inheritdoc/>
        public override bool ProcessFeatures(Dictionary<string, UserInput> features) => true;
    }
}
