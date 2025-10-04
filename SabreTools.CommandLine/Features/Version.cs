using System;
using System.Reflection;

namespace SabreTools.CommandLine.Features
{
    /// <summary>
    /// Default version feature implementation
    /// </summary>
    public class Version : Feature
    {
        public const string DisplayName = "Version";

        private static readonly string[] _defaultFlags = ["v", "version"];

        private const string _description = "Prints version";

        private const string _longDescription = "Prints current program version.";

        public Version()
            : base(DisplayName, _defaultFlags, _description, _longDescription)
        {
            RequiresInputs = false;
        }

        public Version(string[] flags)
            : base(DisplayName, flags, _description, _longDescription)
        {
            RequiresInputs = false;
        }

        /// <inheritdoc/>
        public override bool VerifyInputs() => true;

        /// <inheritdoc/>
        public override bool Execute()
        {
            Console.WriteLine($"Version: {GetVersion()}");
            return true;
        }

        /// <summary>
        /// The current toolset version to be used by all child applications
        /// </summary>
        private static string? GetVersion()
        {
            try
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly == null)
                    return null;

                var assemblyVersion = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
                return assemblyVersion?.InformationalVersion;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}
