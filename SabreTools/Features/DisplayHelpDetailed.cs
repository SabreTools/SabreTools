﻿using SabreTools.Help;

namespace SabreTools.Features
{
    internal class DisplayHelpDetailed : BaseFeature
    {
        public const string DisplayName = "Help (Detailed)";

        private static readonly string[] _flags = ["??", "hd", "help-detailed"];

        private const string _description = "Show this detailed help";

        private const string _longDescription = "Display a detailed help text to the screen.";

        public DisplayHelpDetailed()
            : base(DisplayName, _flags, _description, _longDescription)
        {
        }

        public override bool ProcessArgs(string[] args, FeatureSet help)
        {
            // If we had something else after help
            if (args.Length > 1)
            {
                help.OutputIndividualFeature(args[1], includeLongDescription: true);
                return true;
            }

            // Otherwise, show generic help
            else
            {
                help.OutputAllHelp();
                return true;
            }
        }
    }
}
