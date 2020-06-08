using System;
using System.Collections.Generic;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

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
        private static string _config = "config.xml";
        private static string _dbSchema = "rombasharp";
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

            // User flags
            bool copy = false,
                fixdatOnly = false,
                logOnly = false,
                noDb = false,
                onlyNeeded = false,
                skipInitialScan = false,
                useGolangZip = false;

            // User inputs
            string backup = string.Empty,
                description = string.Empty,
                missingSha1s = string.Empty,
                name = string.Empty,
                newdat = string.Empty,
                old = string.Empty,
                outdat = string.Empty,
                resume = string.Empty,
                source = string.Empty;
            int include7Zips = 1,
                includeGZips = 1,
                includeZips = 1,
                subworkers = 0,
                workers = 0;
            long size = -1;
            List<string> dats = new List<string>();
            List<string> depot = new List<string>();
            List<string> inputs = new List<string>();

            // Get the first argument as a feature flag
            string feature = args[0];

            // Verify that the flag is valid
            if (!_help.TopLevelFlag(feature))
            {
                Globals.Logger.User("'{0}' is not valid feature flag", feature);
                _help.OutputIndividualFeature(feature);
                Globals.Logger.Close();
                return;
            }

            // Now get the proper name for the feature
            feature = _help.GetFeatureName(feature);

            // If we had the help feature first
            if (feature == "Help")
            {
                // If we had something else after help
                if (args.Length > 1)
                {
                    _help.OutputIndividualFeature(args[1]);
                    Globals.Logger.Close();
                    return;
                }
                // Otherwise, show generic help
                else
                {
                    _help.OutputGenericHelp();
                    Globals.Logger.Close();
                    return;
                }
            }

            // Now verify that all other flags are valid
            for (int i = 1; i < args.Length; i++)
            {
                // Verify that the current flag is proper for the feature
                if (!_help[feature].ValidateInput(args[i]))
                {
                    // Everything else is treated as a generic input
                    inputs.Add(args[i]);
                }
            }

            // Now loop through all inputs
            Dictionary<string, Feature> features = _help.GetEnabledFeatures();
            foreach (KeyValuePair<string, Feature> feat in features)
            {
                // Check all of the flag names and translate to arguments
                switch (feat.Key)
                {
                    #region User Flags

                    case "copy":
                        copy = true;
                        break;
                    case "fixdatOnly":
                        fixdatOnly = true;
                        break;
                    case "log-only":
                        logOnly = true;
                        break;
                    case "no-db":
                        noDb = true;
                        break;
                    case "only-needed":
                        onlyNeeded = true;
                        break;
                    case "skip-initial-scan":
                        skipInitialScan = true;
                        break;
                    case "use-golang-zip":
                        useGolangZip = true;
                        break;

                    #endregion

                    #region User Int32 Inputs

                    case "include-7zips":
                        include7Zips = feat.Value.GetInt32Value() == Int32.MinValue ? feat.Value.GetInt32Value() : 0;
                        break;
                    case "include-gzips":
                        includeGZips = feat.Value.GetInt32Value() == Int32.MinValue ? feat.Value.GetInt32Value() : 0;
                        break;
                    case "include-zips":
                        includeZips = feat.Value.GetInt32Value() == Int32.MinValue ? feat.Value.GetInt32Value() : 0;
                        break;
                    case "subworkers":
                        subworkers = feat.Value.GetInt32Value() == Int32.MinValue ? feat.Value.GetInt32Value() : _cores;
                        break;
                    case "workers":
                        workers = feat.Value.GetInt32Value() == Int32.MinValue ? feat.Value.GetInt32Value() : _cores;
                        break;

                    #endregion

                    #region User Int64 Inputs

                    case "size":
                        size = feat.Value.GetInt64Value() == Int64.MinValue ? feat.Value.GetInt64Value() : 0;
                        break;

                    #endregion

                    #region User List<string> Inputs

                    case "dats":
                        dats.AddRange(feat.Value.GetListValue());
                        break;
                    case "depot":
                        depot.AddRange(feat.Value.GetListValue());
                        break;

                    #endregion

                    #region User String Inputs

                    case "backup":
                        backup = feat.Value.GetStringValue();
                        break;
                    case "description":
                        description = feat.Value.GetStringValue();
                        break;
                    case "missingSha1s":
                        missingSha1s = feat.Value.GetStringValue();
                        break;
                    case "name":
                        name = feat.Value.GetStringValue();
                        break;
                    case "new":
                        newdat = feat.Value.GetStringValue();
                        break;
                    case "old":
                        old = feat.Value.GetStringValue();
                        break;
                    case "out":
                        outdat = feat.Value.GetStringValue();
                        break;
                    case "resume":
                        resume = feat.Value.GetStringValue();
                        break;
                    case "source":
                        source = feat.Value.GetStringValue();
                        break;

                    #endregion
                }
            }

            // Now take care of each mode in succesion
            switch(feature)
            {
                case "Help":
                    // No-op as this should be caught
                    break;
                // Adds ROM files from the specified directories to the ROM archive
                case "Archive":
                    VerifyInputs(inputs, feature);
                    InitArchive(inputs, onlyNeeded, resume, includeZips, workers, includeGZips, include7Zips, skipInitialScan, useGolangZip, noDb);
                    break;
                // For each specified DAT file it creates the torrentzip files
                case "Build":
                    VerifyInputs(inputs, feature);
                    InitBuild(inputs, outdat, fixdatOnly, copy, workers, subworkers);
                    break;
                // Cancels current long-running job
                case "Cancel":
                    InitCancel();
                    break;
                // Prints dat stats
                case "DatStats":
                    VerifyInputs(inputs, feature);
                    InitDatStats(inputs);
                    break;
                // Prints db stats
                case "DbStats":
                    InitDbStats();
                    break;
                // Creates a DAT file with those entries that are in -new DAT
                case "Diffdat":
                    InitDiffDat(outdat, old, newdat, name, description);
                    break;
                // Creates a DAT file for the specified input directory and saves it to the -out filename
                case "Dir2Dat":
                    InitDir2Dat(outdat, source, name, description);
                    break;
                // Creates a DAT file with those entries that are in -new DAT
                case "EDiffdat":
                    InitEDiffDat(outdat, old, newdat);
                    break;
                // Exports db to export.csv
                case "Export":
                    InitExport();
                    break;
                // For each specified DAT file it creates a fix DAT
                case "Fixdat":
                    VerifyInputs(inputs, feature);
                    InitFixdat(inputs, outdat, fixdatOnly, workers, subworkers);
                    break;
                // Import a database from a formatted CSV file
                case "Import":
                    VerifyInputs(inputs, feature);
                    InitImport(inputs);
                    break;
                // For each specified hash it looks up any available information
                case "Lookup":
                    VerifyInputs(inputs, feature);
                    InitLookup(inputs, size, outdat);
                    break;
                // Prints memory stats
                case "Memstats":
                    InitMemstats();
                    break;
                // Merges depot
                case "Merge":
                    VerifyInputs(inputs, feature);
                    InitMerge(inputs, onlyNeeded, resume, workers, skipInitialScan);
                    break;
                // Create miss and have file
                case "Miss":
                    VerifyInputs(inputs, feature);
                    InitMiss(inputs);
                    break;
                // Shows progress of the currently running command
                case "Progress":
                    InitProgress();
                    break;
                // Moves DAT index entries for orphaned DATs
                case "Purge Backup":
                    InitPurgeBackup(backup, workers, depot, dats, logOnly);
                    break;
                // Deletes DAT index entries for orphaned DATs
                case "Purge Delete":
                    InitPurgeDelete(workers, depot, dats, logOnly);
                    break;
                // Refreshes the DAT index from the files in the DAT master directory tree
                case "Refresh DATs":
                    InitRefreshDats(workers, missingSha1s);
                    break;
                // Rescan a specific depot
                case "Rescan Depots":
                    VerifyInputs(inputs, feature);
                    InitRescanDepots(inputs);
                    break;
                // Gracefully shuts down server
                case "Shutdown":
                    InitShutdown();
                    break;
                // Prints version
                case "Version":
                    InitVersion();
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
