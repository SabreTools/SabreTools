using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SabreTools.CommandLine;
using SabreTools.CommandLine.Features;
using SabreTools.Features;
using SabreTools.IO.Logging;

namespace SabreTools
{
    public class Program
    {
        #region Static Variables

        /// <summary>
        /// Command set that determines available functionality
        /// </summary>
        private static readonly CommandSet _commands = RetrieveCommands();

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

            // Credits take precidence over all
            if (Array.Exists(args, a => a == "--credits"))
            {
                OutputCredits();
                return;
            }

            // If there's no arguments, show help
            if (args.Length == 0)
            {
                _commands.OutputGenericHelp();
                return;
            }

            // Get the first argument as a feature flag
            string featureName = args[0];
            if (!_commands.TopLevelFlag(featureName))
            {
                Console.WriteLine($"'{featureName}' is not valid feature flag");
                _commands.OutputIndividualFeature(featureName);
                return;
            }

            // Get the proper name for the feature
            featureName = _commands.GetInputName(featureName);

            // Get the associated feature
            if (_commands[featureName] is not Feature feature)
            {
                Console.WriteLine($"'{featureName}' is not valid feature flag");
                _commands.OutputIndividualFeature(featureName);
                return;
            }

            // If we had a help feature first
            if (feature is DefaultHelp helpFeature)
            {
                helpFeature.ProcessArgs(args, 0, _commands);
                return;
            }
            else if (feature is DefaultHelpExtended helpExtFeature)
            {
                helpExtFeature.ProcessArgs(args, 0, _commands);
                return;
            }

            // Now verify that all other flags are valid
            if (!feature.ProcessArgs(args, 1))
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
            if (feature.RequiresInputs && !feature.VerifyInputs())
            {
                _commands.OutputIndividualFeature(feature.Name);
                Environment.Exit(0);
            }

            // Now execute the current feature
            if (!feature.Execute())
            {
                _staticLogger.Error("An error occurred during processing!");
                _commands.OutputIndividualFeature(featureName);
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
        /// Generate a CommandSet object for this program
        /// </summary>
        /// <returns>Populated CommandSet object</returns>
        private static CommandSet RetrieveCommands()
        {
            // Create and add the header to the CommandSet object
            const string barrier = "-----------------------------------------";
            List<string> header =
            [
                "SabreTools - Manipulate, convert, and use DAT files",
                barrier,
                "Usage: SabreTools [option] [flags] [filename|dirname] ...",
                string.Empty
            ];
            List<string> footer =
            [
                string.Empty,
                "For information on available flags, put the option name after help",
            ];

            // Create the base command set with header and footer
            var commands = new CommandSet(header, footer);

            // Add all of the features
            commands.Add(new DefaultHelp());
            commands.Add(new DefaultHelpExtended());
            commands.Add(new Batch());
            commands.Add(new DatFromDir());
            commands.Add(new Sort());
            commands.Add(new Split());
            commands.Add(new Stats());
            commands.Add(new Update());
            commands.Add(new Verify());
            commands.Add(new DefaultVersion());

            return commands;
        }
    }
}
