namespace SabreTools.CommandLine.Features
{
    /// <summary>
    /// Default help feature implementation
    /// </summary>
    public class Help : Feature
    {
        public const string DisplayName = "Help";

        private static readonly string[] _defaultFlags = ["?", "h", "help"];

        private const string _description = "Show this help";

        private const string _longDescription = "Built-in to most of the programs is a basic help text.";

        public Help()
            : base(DisplayName, _defaultFlags, _description, _longDescription)
        {
            RequiresInputs = false;
        }

        public Help(string[] flags)
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
                parentSet?.OutputIndividualFeature(args[1]);
                return true;
            }

            // Otherwise, show generic help
            else
            {
                parentSet?.OutputGenericHelp();
                return true;
            }
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;

        /// <inheritdoc/>
        public override bool Execute() => true;
    }
}
