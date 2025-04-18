﻿using SabreTools.Help;

namespace SabreTools.Features
{
    internal class DisplayHelp : BaseFeature
    {
        public const string DisplayName = "Help";

        private static readonly string[] _flags = ["?", "h", "help"];

        private const string _description = "Show this help";

        private const string _longDescription = "Built-in to most of the programs is a basic help text.";

        public DisplayHelp()
            : base(DisplayName, _flags, _description, _longDescription)
        {
        }

        public override bool ProcessArgs(string[] args, FeatureSet help)
        {
            // If we had something else after help
            if (args.Length > 1)
            {
                help.OutputIndividualFeature(args[1]);
                return true;
            }

            // Otherwise, show generic help
            else
            {
                help.OutputGenericHelp();
                return true;
            }
        }
    }
}
