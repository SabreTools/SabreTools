namespace SabreTools.CommandLine.Features
{
    /// <summary>
    /// Default extended help feature implementation
    /// </summary>
    public class HelpExtended : Feature
    {
        public const string DisplayName = "Help (Detailed)";

        private static readonly string[] _defaultFlags = ["??", "hd", "help-detailed"];

        private const string _description = "Show this detailed help";

        private const string _longDescription = "Display a detailed help text to the screen.";

        public HelpExtended()
            : base(DisplayName, _defaultFlags, _description, _longDescription)
        {
            RequiresInputs = false;
        }

        public HelpExtended(string[] flags)
            : base(DisplayName, flags, _description, _longDescription)
        {
            RequiresInputs = false;
        }

        /// <inheritdoc/>
        public override bool ProcessArgs(string[] args, int index)
            => ProcessArgs(args, index, null);

        /// <inheritdoc cref="ProcessArgs(string[], int)"/>
        /// <param name="parentSet">Reference to the enclosing parent set</param>
        public bool ProcessArgs(string[] args, int index, CommandSet? parentSet)
        {
            // If we had something else after help
            if (args.Length > 1)
            {
                parentSet?.OutputIndividualFeature(args[1], includeLongDescription: true);
                return true;
            }

            // Otherwise, show generic help
            else
            {
                parentSet?.OutputAllHelp();
                return true;
            }
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;

        /// <inheritdoc/>
        public override bool Execute() => true;
    }
}
