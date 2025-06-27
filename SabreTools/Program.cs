using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using SabreTools.Features;
using SabreTools.Help;
using SabreTools.IO;
using SabreTools.IO.Logging;

namespace SabreTools
{
    public class Program
    {
        #region Static Variables

        /// <summary>
        /// Help object that determines available functionality
        /// </summary>
        private static FeatureSet? _help;

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
            _help = RetrieveHelp();

            // Credits take precidence over all
            if (Array.Exists(args, a => a == "--credits"))
            {
                FeatureSet.OutputCredits();
                return;
            }

            // If there's no arguments, show help
            if (args.Length == 0)
            {
                _help.OutputGenericHelp();
                return;
            }

            // Get the first argument as a feature flag
            string featureName = args[0];

            // TODO: Remove this block once trimming is no longer needed
            // TODO: Update wiki documentation ONLY after this reaches stable
            // TODO: Re-evaluate feature flags with this change in mind
            featureName = featureName.TrimStart('-');
            if (args[0].StartsWith("-"))
                Console.WriteLine($"Feature flags no longer require leading '-' characters");

            // Verify that the flag is valid
            if (!_help.TopLevelFlag(featureName))
            {
                Console.WriteLine($"'{featureName}' is not valid feature flag");
                _help.OutputIndividualFeature(featureName);
                return;
            }

            // Get the proper name for the feature
            featureName = _help.GetFeatureName(featureName);

            // Get the associated feature
            BaseFeature feature = (_help[featureName] as BaseFeature)!;

            // If we had the help feature first
            if (featureName == DisplayHelp.DisplayName || featureName == DisplayHelpDetailed.DisplayName)
            {
                feature.ProcessArgs(args, _help);
                return;
            }

            // Now verify that all other flags are valid
            if (!feature.ProcessArgs(args, _help))
                return;

#if NET452_OR_GREATER || NETCOREAPP
            // If output is being redirected or we are in script mode, don't allow clear screens
            if (!Console.IsOutputRedirected && feature.ScriptMode)
            {
                Console.Clear();
                SabreTools.Core.Globals.SetConsoleHeader("SabreTools");
            }
#endif

            // Now process the current feature
            Dictionary<string, Feature?> features = _help.GetEnabledFeatures();
            bool success = false;
            switch (featureName)
            {
                // No-op as these should be caught
                case DisplayHelp.DisplayName:
                case DisplayHelpDetailed.DisplayName:
                    break;

                // Require input verification
                case Batch.DisplayName:
                case DatFromDir.DisplayName:
                case Split.DisplayName:
                case Stats.DisplayName:
                case Update.DisplayName:
                case Verify.DisplayName:
                    VerifyInputs(feature.Inputs, feature);
                    success = feature.ProcessFeatures(features);
                    break;

                // Requires no input verification
                case Sort.DisplayName:
                case Features.Version.DisplayName:
                    success = feature.ProcessFeatures(features);
                    break;

                // If nothing is set, show the help
                default:
                    _help.OutputGenericHelp();
                    break;
            }

            // If the feature failed, output help
            if (!success)
            {
                _staticLogger.Error("An error occurred during processing!");
                _help.OutputIndividualFeature(featureName);
            }

            LoggerImpl.Close();
            return;
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
            help.Add(new DisplayHelp());
            help.Add(new DisplayHelpDetailed());
            help.Add(new Batch());
            help.Add(new DatFromDir());
            help.Add(new Sort());
            help.Add(new Split());
            help.Add(new Stats());
            help.Add(new Update());
            help.Add(new Verify());
            help.Add(new Features.Version());

            return help;
        }

        /// <summary>
        /// Verify that there are inputs, show help otherwise
        /// </summary>
        /// <param name="inputs">List of inputs</param>
        /// <param name="feature">Name of the current feature</param>
        private static void VerifyInputs(List<string> inputs, BaseFeature feature)
        {
            if (inputs.Count == 0)
            {
                _staticLogger.Error("This feature requires at least one input");
                _help?.OutputIndividualFeature(feature.Name);
                Environment.Exit(0);
            }
        }
    }
}
