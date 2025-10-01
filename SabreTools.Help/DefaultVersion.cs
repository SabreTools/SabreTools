using System;
using System.Collections.Generic;
using System.Reflection;
using SabreTools.Help.Inputs;

namespace SabreTools.Help
{
    /// <summary>
    /// Default version feature implementation
    /// </summary>
    public class DefaultVersion : Feature
    {
        public const string DisplayName = "Version";

        private static readonly string[] _flags = ["v", "version"];

        private const string _description = "Prints version";

        private const string _longDescription = "Prints current program version.";

        public DefaultVersion()
            : base(DisplayName, _flags, _description, _longDescription)
        {
        }

        public override bool ProcessFeatures(Dictionary<string, UserInput> features)
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
