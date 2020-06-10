using System;
using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

namespace RombaSharp
{
    /// <summary>
    /// Entry class for the RombaSharp application
    /// </summary>
    /// <remarks>
    /// In the database, we want to enable "offline mode". That is, when a user does an operation
    /// that needs to read from the depot themselves, if the depot folder cannot be found, the
    /// user is prompted to reconnect the depot OR skip that depot entirely.
    /// </remarks>
    public partial class RombaSharp
    {
        // General settings
        private static string _logdir;		// Log folder location
        private static string _tmpdir;		// Temp folder location
        private static string _webdir;		// Web frontend location
        private static string _baddir;		// Fail-to-unpack file folder location
        private static int _verbosity;		// Verbosity of the output
        private static int _cores;			// Forced CPU cores

        // DatRoot settings
        private static string _dats;		// DatRoot folder location
        private static string _db;			// Database name

        // Depot settings
        private static Dictionary<string, Tuple<long, bool>> _depots; // Folder location, Max size

        // Server settings
        private static int _port;			// Web server port

        // Other private variables
        private const string _config = "config.xml";
        private const string _dbSchema = "rombasharp";
        private static string _connectionString;
        private static Help _help;

        /// <summary>
        /// Entry class for the RombaSharp application
        /// </summary>
        public static void Main(string[] args)
        {
            // Perform initial setup and verification
            Globals.Logger = new Logger(true, "romba.log");

            InitializeConfiguration();
            DatabaseTools.EnsureDatabase(_dbSchema, _db, _connectionString);

            // Create a new Help object for this program
            _help = RombaSharp.RetrieveHelp();

            // Get the location of the script tag, if it exists
            int scriptLocation = (new List<string>(args)).IndexOf("--script");

            // If output is being redirected or we are in script mode, don't allow clear screens
            if (!Console.IsOutputRedirected && scriptLocation == -1)
            {
                Console.Clear();
                Build.PrepareConsole("RombaSharp");
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
            RombaSharpFeature feature = _help[featureName] as RombaSharpFeature;

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
                case DetailedHelpFeature.Value:
                case HelpFeature.Value:
                case ScriptFeature.Value:
                    // No-op as this should be caught
                    break;

                // Require input verification
                case ArchiveFeature.Value:
                case BuildFeature.Value:
                case DatStatsFeature.Value:
                case FixdatFeature.Value:
                case ImportFeature.Value:
                case LookupFeature.Value:
                case MergeFeature.Value:
                case MissFeature.Value:
                case RescanDepotsFeature.Value:
                    VerifyInputs(feature.Inputs, featureName);
                    feature.ProcessFeatures(features);
                    break;

                // Requires no input verification
                case CancelFeature.Value:
                case DbStatsFeature.Value:
                case DiffdatFeature.Value:
                case Dir2DatFeature.Value:
                case EDiffdatFeature.Value:
                case ExportFeature.Value:
                case MemstatsFeature.Value:
                case ProgressFeature.Value:
                case PurgeBackupFeature.Value:
                case PurgeDeleteFeature.Value:
                case RefreshDatsFeature.Value:
                case ShutdownFeature.Value:
                case VersionFeature.Value:
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
