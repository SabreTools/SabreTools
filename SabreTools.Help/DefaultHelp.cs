namespace SabreTools.Help
{
    /// <summary>
    /// Default help feature implementation
    /// </summary>
    public class DefaultHelp : Feature
    {
        public const string DisplayName = "Help";

        private static readonly string[] _flags = ["?", "h", "help"];

        private const string _description = "Show this help";

        private const string _longDescription = "Built-in to most of the programs is a basic help text.";

        public DefaultHelp()
            : base(DisplayName, _flags, _description, _longDescription)
        {
            RequiresInputs = false;
        }

        /// <inheritdoc/>
        public override bool ProcessArgs(string[] args, int index, CommandSet? parentSet)
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
        public override bool ProcessFeatures() => true;
    }
}
