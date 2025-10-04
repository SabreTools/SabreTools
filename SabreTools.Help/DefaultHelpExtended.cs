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
        public override bool Execute() => true;

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;
    }
}
