using System;
using SabreTools.CommandLine;

namespace SabreTools.Features
{
    /// <summary>
    /// Default version feature implementation
    /// </summary>
    public class Version : Feature
    {
        public const string DisplayName = "Version";

        private static readonly string[] _defaultFlags = ["v", "version"];

        private const string _description = "Prints version";

        private const string _detailedDescription = "Prints current program version.";

        public Version()
            : base(DisplayName, _defaultFlags, _description, _detailedDescription)
        {
            RequiresInputs = false;
        }

        public Version(string[] flags)
            : base(DisplayName, flags, _description, _detailedDescription)
        {
            RequiresInputs = false;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;

        /// <inheritdoc/>
        public override bool Execute()
        {
            Console.WriteLine($"Version: {Globals.Version}");
            return true;
        }
    }
}
