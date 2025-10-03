using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SabreTools.Features;
using SabreTools.Help;
using SabreTools.IO.Logging;

namespace SabreTools
{
    public class Program
    {
        #region Static Variables

        /// <summary>
        /// Feature set that determines available functionality
        /// </summary>
        private static FeatureSet? _features;

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger _staticLogger = new();

        #endregion

        /// <summary>
        /// Entry point for the SabreTools application
        /// </summary>
        /// <param name="args">String array representing command line parameters</param>
        public static void Main(string[] args)
        {
            // Reformat the arguments, if needed
            if (Array.Exists(args, a => a.Contains("\"")))
                args = ReformatArguments(args);

            // Create a new Help object for this program
            _features = RetrieveHelp();

            // Credits take precidence over all
            if (Array.Exists(args, a => a == "--credits"))
            {
                OutputCredits();
                return;
            }

            // If there's no arguments, show help
            if (args.Length == 0)
            {
                _features.OutputGenericHelp();
                return;
            }

            // Get the first argument as a feature flag
            string featureName = args[0];
            if (!_features.TopLevelFlag(featureName))
            {
                Console.WriteLine($"'{featureName}' is not valid feature flag");
                _features.OutputIndividualFeature(featureName);
                return;
            }

            // Get the proper name for the feature
            featureName = _features.GetInputName(featureName);

            // Get the associated feature
            if (_features[featureName] is not Feature feature)
            {
                Console.WriteLine($"'{featureName}' is not valid feature flag");
                _features.OutputIndividualFeature(featureName);
                return;
            }

            // If we had the help feature first
            if (featureName == DefaultHelp.DisplayName || featureName == DefaultHelpExtended.DisplayName)
            {
                feature.ProcessArgs(args, 0, _features);
                return;
            }

            // Now verify that all other flags are valid
            if (!feature.ProcessArgs(args, 1, _features))
                return;

#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            // If output is being redirected or we are in script mode, don't allow clear screens
            if (!Console.IsOutputRedirected && feature is BaseFeature bf && bf.ScriptMode)
            {
                Console.Clear();
                Core.Globals.SetConsoleHeader("SabreTools");
            }
#endif

            // If inputs are required
            if (feature.RequiresInputs)
                VerifyInputs(feature);

            // Now process the current feature
            if (!feature.ProcessFeatures())
            {
                _staticLogger.Error("An error occurred during processing!");
                _features.OutputIndividualFeature(featureName);
            }

            LoggerImpl.Close();
            return;
        }

        /// <summary>
        /// Output the SabreTools suite credits
        /// </summary>
        private static void OutputCredits()
        {
            const string _barrier = "-----------------------------------------";

            Console.WriteLine(_barrier);
            Console.WriteLine("Credits");
            Console.WriteLine(_barrier);
            Console.WriteLine();
            Console.WriteLine("Programmer / Lead:	Matt Nadareski (darksabre76)");
            Console.WriteLine("Additional code:	emuLOAD, @tractivo, motoschifo");
            Console.WriteLine("Testing:		emuLOAD, @tractivo, Kludge, Obiwantje, edc");
            Console.WriteLine("Suggestions:		edc, AcidX, Amiga12, EliUmniCk");
            Console.WriteLine("Based on work by:	The Wizard of DATz");
            Console.WriteLine(_barrier);
            Console.WriteLine(_barrier);

#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            if (!Console.IsOutputRedirected)
#endif
            {
                Console.WriteLine();
                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Reformat arguments incorrectly split in quotes
        /// </summary>
        private static string[] ReformatArguments(string[] args)
        {
            // Handle empty arguments
            if (args.Length == 0)
                return args;

            // Recombine arguments using a single space
            string argsString = string.Join(" ", args).Trim();

            // Split the string using Regex
            var matches = Regex.Matches(argsString, @"([a-zA-Z0-9\-]*=)?[\""].+?[\""]|[^ ]+", RegexOptions.Compiled);

            // Get just the values from the matches
            var matchArr = new Match[matches.Count];
            matches.CopyTo(matchArr, 0);
            return Array.ConvertAll(matchArr, m => m.Value);
        }

        /// <summary>
        /// Generate a Help object for this program
        /// </summary>
        /// <returns>Populated Help object</returns>
        private static FeatureSet RetrieveHelp()
        {
            // Create and add the header to the Help object
            string barrier = "-----------------------------------------";
            List<string> helpHeader =
            [
                "SabreTools - Manipulate, convert, and use DAT files",
                barrier,
                "Usage: SabreTools [option] [flags] [filename|dirname] ...",
                string.Empty
            ];

            // Create the base help object with header
            var help = new FeatureSet(helpHeader);

            // Add all of the features
            help.Add(new DefaultHelp());
            help.Add(new DefaultHelpExtended());
            help.Add(new Batch());
            help.Add(new DatFromDir());
            help.Add(new Sort());
            help.Add(new Split());
            help.Add(new Stats());
            help.Add(new Update());
            help.Add(new Verify());
            help.Add(new DefaultVersion());

            return help;
        }

        /// <summary>
        /// Verify that there are inputs, show help otherwise
        /// </summary>
        /// <param name="feature">Name of the current feature</param>
        /// <remarks>Assumes inputs need to be a file, directory, or wildcard path</remarks>
        private static void VerifyInputs(Feature feature)
        {
            // If there are no inputs
            if (feature.Inputs.Count == 0)
            {
                _staticLogger.Error("This feature requires at least one input");
                _features?.OutputIndividualFeature(feature.Name);
                Environment.Exit(0);
            }

            // Loop through and verify all inputs are valid
            for (int i = 0; i < feature.Inputs.Count; i++)
            {
                // Files and directories are valid
                if (File.Exists(feature.Inputs[i]) || Directory.Exists(feature.Inputs[i]))
                    continue;

                // Wildcard inputs are treated as potential paths
#if NETFRAMEWORK || NETSTANDARD
                if (feature.Inputs[i].Contains("*") || feature.Inputs[i].Contains("?"))
#else
                if (feature.Inputs[i].Contains('*') || feature.Inputs[i].Contains('?'))
#endif
                    continue;

                // Everything else is an error
                Console.Error.WriteLine($"Invalid input detected: {feature.Inputs[i]}");
                _features?.OutputIndividualFeature(feature.Name);
                Environment.Exit(0);
            }
        }
    }
}
