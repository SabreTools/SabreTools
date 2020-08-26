using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Filtering;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

namespace SabreTools.Features
{
    internal class Batch : BaseFeature
    {
        public const string Value = "Batch";

        public Batch()
        {
            Name = Value;
            Flags = new List<string>() { "-bt", "--batch" };
            Description = "Enable batch mode";
            _featureType = FeatureType.Flag;
            LongDescription = "Run a special mode that takes input files as lists of batch commands to run sequentially.";
            Features = new Dictionary<string, Feature>();
        }

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            base.ProcessFeatures(features);

            // Try to read each input as a batch run file
            foreach (string path in Inputs)
            {
                // If the file doesn't exist, warn but continue
                if (!File.Exists(path))
                {
                    Globals.Logger.Warning($"{path} does not exist. Skipping...");
                    continue;
                }

                // Try to process the file now
                try
                {
                    // Every line is its own command
                    string[] lines = File.ReadAllLines(path);

                    // Each batch file has its own state
                    int index = 0;
                    DatFile datFile = DatFile.Create();
                    string outputDirectory = null;

                    // Process each command line
                    foreach (string line in lines)
                    {
                        // Skip empty lines
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        // Skip lines that start with REM or #
                        if (line.StartsWith("REM") || line.StartsWith("#"))
                            continue;

                        // Read the command in, if possible
                        var command = BatchCommand.Create(line);
                        if (command == null)
                        {
                            Globals.Logger.Warning($"Could not process {path} due to the following line: {line}");
                            break;
                        }

                        // Now switch on the command
                        switch (command.Name.ToLowerInvariant())
                        {
                            // Parse in new input file(s)
                            case "input":
                                if (command.Arguments.Count == 0)
                                {
                                    Globals.Logger.Warning($"Invoked {command.Name} but no arguments were provided");
                                    continue;
                                }

                                // Assume there could be multiple
                                foreach (string input in command.Arguments)
                                {
                                    datFile.Parse(input, index++);
                                }

                                break;

                            // Apply a filter
                            case "filter":
                                if (command.Arguments.Count < 2 || command.Arguments.Count > 3)
                                {
                                    Globals.Logger.Warning($"Invoked {command.Name} and expected between 2-3 arguments, but {command.Arguments.Count} arguments were provided");
                                    continue;
                                }

                                // Read in the individual arguments
                                Field filterField = command.Arguments[0].AsField();
                                string filterValue = command.Arguments[1];
                                bool filterNegate = (command.Arguments.Count == 3 ? command.Arguments[2].AsYesNo() ?? false : false);

                                // Create a filter with this new set of fields
                                Filter filter = new Filter();
                                filter.SetFilter(filterField, filterValue, filterNegate);

                                // Apply the filter blindly
                                datFile.ApplyFilter(filter, false);

                                break;

                            // Apply an extra INI
                            case "extra":
                                if (command.Arguments.Count != 2)
                                {
                                    Globals.Logger.Warning($"Invoked {command.Name} and expected 2 arguments, but {command.Arguments.Count} arguments were provided");
                                    continue;
                                }

                                // Read in the individual arguments
                                Field extraField = command.Arguments[0].AsField();
                                string extraFile = command.Arguments[1];

                                // Create the extra INI
                                ExtraIni extraIni = new ExtraIni();
                                ExtraIniItem extraIniItem = new ExtraIniItem();
                                extraIniItem.PopulateFromFile(extraFile);
                                extraIniItem.Field = extraField;
                                extraIni.Items.Add(extraIniItem);

                                // Apply the extra INI blindly
                                datFile.ApplyExtras(extraIni);

                                break;

                            // TODO: Implement internal split/merge
                            // TODO: Implement field removal (good for post-Extras) (possible two steps? add + apply?)
                            // TODO: Implement description to name
                            // TODO: Implement 1G1R
                            // TODO: Implement 1RPG
                            // TODO: Implement scene date strip

                            // Set new output format(s)
                            case "format":
                            case "type":
                                if (command.Arguments.Count == 0)
                                {
                                    Globals.Logger.Warning($"Invoked {command.Name} but no arguments were provided");
                                    continue;
                                }

                                // Assume there could be multiple
                                datFile.Header.DatFormat = 0x00;
                                foreach (string format in command.Arguments)
                                {
                                    datFile.Header.DatFormat |= format.AsDatFormat();
                                }

                                break;

                            // Set output directory
                            case "output":
                                if (command.Arguments.Count != 1)
                                {
                                    Globals.Logger.Warning($"Invoked {command.Name} and expected exactly 1 argument, but {command.Arguments.Count} arguments were provided");
                                    continue;
                                }

                                // Only set the first as the output directory
                                outputDirectory = command.Arguments[0];
                                break;

                            // Write out the current DatFile
                            case "write":
                                if (command.Arguments.Count != 0)
                                {
                                    Globals.Logger.Warning($"Invoked {command.Name} and expected no arguments, but {command.Arguments.Count} arguments were provided");
                                    continue;
                                }

                                // TODO: should argument be allowed for overwrite?

                                // Write out the dat with the current state
                                datFile.Write(outputDirectory);
                                break;

                            // Reset the internal state
                            case "reset":
                                if (command.Arguments.Count != 0)
                                {
                                    Globals.Logger.Warning($"Invoked {command.Name} and expected no arguments, but {command.Arguments.Count} arguments were provided");
                                    continue;
                                }

                                // Reset all state variables
                                index = 0;
                                datFile = DatFile.Create();
                                outputDirectory = null;
                                break;

                                /*
                                 * Add base [Ignore for now, only needed in diffing]
                                 * 1G1R, description as name, etc [Ignore for now]
                                 * Set header values [Ignore for now]
                                 */
                        }
                    }
                }
                catch (Exception ex)
                {
                    Globals.Logger.Error($"There was an exception processing {path}: {ex}");
                    continue;
                }                

                // TODO: The following is needed here - 
                /*
                Limitations:

                Multi outputs may not work, like split or diff outputs
                Internal state might have to be set strangely
                May not easily support sort and verify
                May not easily support extract and restore
                May not easily support DFD
                Honesty beat supports the update path only, but should be a separate feature so it can be extended
                */
            }
        }

        /// <summary>
        /// Internal representation of a single batch command
        /// </summary>
        private class BatchCommand
        {
            public string Name { get; private set; }
            public List<string> Arguments { get; private set; } = new List<string>();

            /// <summary>
            /// Create a command based on parsing a line
            /// </summary>
            public static BatchCommand Create(string line)
            {
                // Empty lines don't count
                if (string.IsNullOrEmpty(line))
                    return null;

                // Split into name and arguments
                string splitRegex = @"^(\S+)\((.*?)\);?";
                var match = Regex.Match(line, splitRegex);

                // If we didn't get a success, just return null
                if (!match.Success)
                    return null;

                // Otherwise, get the name and arguments
                string commandName = match.Groups[1].Value;
                List<string> arguments = match.Groups[2].Value.Split(',').Select(s => s.Trim()).ToList();

                return new BatchCommand { Name = commandName, Arguments = arguments };
            }
        }
    }
}
