using System.Collections.Generic;
using SabreTools.Core;
using SabreTools.Help.Inputs;

namespace SabreTools.Features
{
    internal class Version : BaseFeature
    {
        public const string DisplayName = "Version";

        private static readonly string[] _flags = ["v", "version"];

        private const string _description = "Prints version";

        private const string _longDescription = "Prints current program version.";

        public Version()
            : base(DisplayName, _flags, _description, _longDescription)
        {
            // Common Features
            AddCommonFeatures();
        }

        public override bool ProcessFeatures(Dictionary<string, UserInput> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
                return false;

            _logger.User($"SabreTools version: {Globals.Version}");
            return true;
        }
    }
}
