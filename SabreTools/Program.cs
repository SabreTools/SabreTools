using System;
using System.Collections.Generic;

using SabreTools.Features;
using SabreTools.Library.Data;
using SabreTools.Library.Help;
using SabreTools.Library.Logging;

namespace SabreTools
{
    public class Program
    {
        // Private required variables
        private static Help _help;

        /// <summary>
        /// Entry point for the SabreTools application
        /// </summary>
        /// <param name="args">String array representing command line parameters</param>
        public static void Main(string[] args)
        {
            // Perform initial setup and verification
            Globals.Logger = new Logger(true, "sabretools.log");

            // Create a new Help object for this program
            _help = RetrieveHelp();

            // Get the location of the script tag, if it exists
            int scriptLocation = (new List<string>(args)).IndexOf("--script");

            // If output is being redirected or we are in script mode, don't allow clear screens
            if (!Console.IsOutputRedirected && scriptLocation == -1)
            {
                Console.Clear();
                Prepare.SetConsoleHeader("SabreTools");
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
            BaseFeature feature = _help[featureName] as BaseFeature;

            // If we had the help feature first
            if (featureName == DisplayHelp.Value || featureName == DisplayHelpDetailed.Value)
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
                case DisplayHelp.Value:
                case DisplayHelpDetailed.Value:
                case Script.Value:
                    break;

                // Require input verification
                case Batch.Value:
                case DatFromDir.Value:
                case Extract.Value:
                case Restore.Value:
                case Split.Value:
                case Stats.Value:
                case Update.Value:
                case Verify.Value:
                    VerifyInputs(feature.Inputs, featureName);
                    feature.ProcessFeatures(features);
                    break;

                // Requires no input verification
                case Sort.Value:
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

        /// <summary>
        /// Generate a Help object for this program
        /// </summary>
        /// <returns>Populated Help object</returns>
        private static Help RetrieveHelp()
        {
            // Create and add the header to the Help object
            string barrier = "-----------------------------------------";
            List<string> helpHeader = new List<string>()
            {
                "SabreTools - Manipulate, convert, and use DAT files",
                barrier,
                "Usage: SabreTools [option] [flags] [filename|dirname] ...",
                string.Empty
            };

            // Create the base help object with header
            Help help = new Help(helpHeader);

            // Add all of the features
            help.Add(new DisplayHelp());
            help.Add(new DisplayHelpDetailed());
            help.Add(new Script());
            help.Add(new Batch());
            help.Add(new DatFromDir());
            help.Add(new Extract());
            help.Add(new Restore());
            help.Add(new Sort());
            help.Add(new Split());
            help.Add(new Stats());
            help.Add(new Update());
            help.Add(new Verify());

            return help;
        }

        /// <summary>
        /// Verify that there are inputs, show help otherwise
        /// </summary>
        /// <param name="inputs">List of inputs</param>
        /// <param name="feature">Name of the current feature</param>
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
