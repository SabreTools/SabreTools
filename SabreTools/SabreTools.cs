using System;
using System.Collections.Generic;
using System.Xml;
using SabreTools.Library.Data;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

namespace SabreTools
{
    /// <summary>
    /// Entry class for the DATabase application
    /// </summary>
    /// TODO: Look into async read/write to make things quicker. Ask edc for help?
    public partial class SabreTools
    {
        // Private required variables
        private static Help _help;

        /// <summary>
        /// Entry class for the SabreTools application
        /// </summary>
        /// <param name="args">String array representing command line parameters</param>
        public static void Main(string[] args)
        {
            // Perform initial setup and verification
            Globals.Logger = new Logger(true, "sabretools.log");

            // Create a new Help object for this program
            _help = SabreTools.RetrieveHelp();

            // Get the location of the script tag, if it exists
            int scriptLocation = (new List<string>(args)).IndexOf("--script");

            // If output is being redirected or we are in script mode, don't allow clear screens
            if (!Console.IsOutputRedirected && scriptLocation == -1)
            {
                Console.Clear();
                Build.PrepareConsole("SabreTools");
            }

            // Now we remove the script tag because it messes things up
            if (scriptLocation > -1)
            {
                List<string> newargs = new List<string>(args);
                newargs.RemoveAt(scriptLocation);
                args = newargs.ToArray();
            }

            // Credits take precidence over all
            if ((new List<string>(args)).Contains("--credits"))
            {
                _help.OutputCredits();
                Globals.Logger.Close();
                return;
            }

            // If there's no arguments, show help
            if (args.Length == 0)
            {
                _help.OutputGenericHelp();
                Globals.Logger.Close();
                return;
            }

            // Get the first argument as a feature flag
            string featureName = args[0];

            // Verify that the flag is valid
            if (!_help.TopLevelFlag(featureName))
            {
                Globals.Logger.User($"'{featureName}' is not valid feature flag");
                _help.OutputIndividualFeature(featureName);
                Globals.Logger.Close();
                return;
            }

            // Get the proper name for the feature
            featureName = _help.GetFeatureName(featureName);

            // Get the associated feature
            SabreToolsFeature feature = _help[featureName] as SabreToolsFeature;

            // If we had the help feature first
            if (featureName == HelpFeature.Value || featureName == DetailedHelpFeature.Value)
            {
                feature.ProcessArgs(args, _help);
                Globals.Logger.Close();
                return;
            }

            // Now verify that all other flags are valid
            if (!feature.ProcessArgs(args, _help))
            {
                Globals.Logger.Close();
                return;
            }

            // Now process the current feature
            Dictionary<string, Feature> features = _help.GetEnabledFeatures();
            switch (featureName)
            {
                // No-op as these should be caught
                case HelpFeature.Value:
                case DetailedHelpFeature.Value:
                case ScriptFeature.Value:
                    break;

                // Require input verification
                case DatFromDirFeature.Value:
                case ExtractFeature.Value:
                case RestoreFeature.Value:
                case SplitFeature.Value:
                case StatsFeature.Value:
                case UpdateFeature.Value:
                case VerifyFeature.Value:
                    VerifyInputs(feature.Inputs, featureName);
                    feature.ProcessFeatures(features);
                    break;

                // Requires no input verification
                case SortFeature.Value:
                    feature.ProcessFeatures(features);
                    break;

                // If nothing is set, show the help
                default:
                    _help.OutputGenericHelp();
                    break;
            }

            Globals.Logger.Close();
            return;
        }

        private static void VerifyInputs(List<string> inputs, string feature)
        {
            if (inputs.Count == 0)
            {
                Globals.Logger.Error("This feature requires at least one input");
                _help.OutputIndividualFeature(feature);
                Environment.Exit(0);
            }
        }
    }
}
