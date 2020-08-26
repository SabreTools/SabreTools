using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Filtering;
using SabreTools.Library.IO;
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
            LongDescription = @"Run a special mode that takes input files as lists of batch commands to run sequentially. Each command has to be its own line and must be followed by a semicolon (`;`). Commented lines may start with either `REM` or `#`. Multiple batch files are allowed but they will be run independently from each other.

The following commands are currently implemented:

Set a header field (if default):    set(header.field, value);
Parse new file(s):                  input(datpath, ...);
Filter on a field and value:        filter(machine.field|item.field, value, [negate = false]);
Apply a MAME Extra INI for a field: extra(field, inipath);
Perform a split/merge:              merge(split|merged|nonmerged|full|device);
Set game names from description:    descname();
Run 1G1R on the items:              1g1r(region, ...);
Split into one rom per game:        orpg();
Remove fields from games/items:     remove(machine.field|item.field, ...);
Remove scene dates from names:      sds();
Add new output format(s):           format(datformat, ...);
Set the output directory:           output(outdir);
Write the internal items:           write([overwrite = true]);
Reset the internal state:           reset();";
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
                    Globals.Logger.User($"{path} does not exist. Skipping...");
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
                            Globals.Logger.User($"Could not process {path} due to the following line: {line}");
                            break;
                        }

                        // Now switch on the command
                        Globals.Logger.User($"Attempting to invoke {command.Name} with {(command.Arguments.Count == 0 ? "no arguments" : "the following argument(s): " + string.Join(", ", command.Arguments))}");
                        switch (command.Name.ToLowerInvariant())
                        {
                            // Set a header field
                            case "set":
                                if (command.Arguments.Count != 2)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} but no arguments were provided");
                                    Globals.Logger.User("Usage: set(header.field, value);");
                                    continue;
                                }

                                // Read in the individual arguments
                                Field field = command.Arguments[0].AsField();
                                string value = command.Arguments[1];

                                // Set the header field
                                datFile.Header.SetFields(new Dictionary<Field, string> { [field] = value });

                                break;

                            // Parse in new input file(s)
                            case "input":
                                if (command.Arguments.Count == 0)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} but no arguments were provided");
                                    Globals.Logger.User("Usage: input(datpath, ...);");
                                    continue;
                                }

                                // Get only files from inputs
                                List<ParentablePath> onlyFiles = DirectoryExtensions.GetFilesOnly(command.Arguments);

                                // Assume there could be multiple
                                foreach (ParentablePath input in onlyFiles)
                                {
                                    datFile.Parse(input, index++);
                                }

                                break;

                            // Apply a filter
                            case "filter":
                                if (command.Arguments.Count < 2 || command.Arguments.Count > 3)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} and expected between 2-3 arguments, but {command.Arguments.Count} arguments were provided");
                                    Globals.Logger.User("Usage: filter(field, value, [negate = false]);");
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
                                    Globals.Logger.User($"Invoked {command.Name} and expected 2 arguments, but {command.Arguments.Count} arguments were provided");
                                    Globals.Logger.User("Usage: extra(field, inipath);");
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

                            // Apply internal split/merge
                            case "merge":
                                if (command.Arguments.Count != 1)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} and expected 1 argument, but {command.Arguments.Count} arguments were provided");
                                    Globals.Logger.User("Usage: merge(split|merged|nonmerged|full|device);");
                                    continue;
                                }

                                // Read in the individual arguments
                                MergingFlag mergingFlag = command.Arguments[0].AsMergingFlag();

                                // Apply the merging flag
                                datFile.ProcessSplitType(mergingFlag);

                                break;

                            // Apply description-as-name logic
                            case "descname":
                                if (command.Arguments.Count != 0)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} and expected no arguments, but {command.Arguments.Count} arguments were provided");
                                    Globals.Logger.User("Usage: descname();");
                                    continue;
                                }

                                // Apply the logic
                                datFile.MachineDescriptionToName();

                                break;

                            // Apply 1G1R
                            case "1g1r":
                                if (command.Arguments.Count == 0)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} but no arguments were provided");
                                    Globals.Logger.User("Usage: 1g1r(region, ...);");
                                    continue;
                                }

                                // Set the region list
                                var tempRegionList = datFile.Header.RegionList;
                                datFile.Header.RegionList = command.Arguments;

                                // Run the 1G1R functionality
                                datFile.OneGamePerRegion();

                                // Reset the header value
                                datFile.Header.RegionList = tempRegionList;

                                break;

                            // Apply one rom per game (ORPG)
                            case "orpg":
                                if (command.Arguments.Count != 0)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} and expected no arguments, but {command.Arguments.Count} arguments were provided");
                                    Globals.Logger.User("Usage: orpg();");
                                    continue;
                                }

                                // Apply the logic
                                datFile.OneRomPerGame();

                                break;

                            // Remove a field
                            case "remove":
                                if (command.Arguments.Count == 0)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} but no arguments were provided");
                                    Globals.Logger.User("Usage: remove(field, ...);");
                                    continue;
                                }

                                // Set the field list
                                var tempRemoveFields = datFile.Header.ExcludeFields;
                                datFile.Header.ExcludeFields = command.Arguments.Select(s => s.AsField()).ToList();

                                // Run the removal functionality
                                datFile.RemoveFieldsFromItems();

                                // Reset the header value
                                datFile.Header.ExcludeFields = tempRemoveFields;

                                break;

                            // Apply scene date stripping
                            case "sds":
                                if (command.Arguments.Count != 0)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} and expected no arguments, but {command.Arguments.Count} arguments were provided");
                                    Globals.Logger.User("Usage: sds();");
                                    continue;
                                }

                                // Apply the logic
                                datFile.StripSceneDatesFromItems();

                                break;

                            // Set new output format(s)
                            case "format":
                                if (command.Arguments.Count == 0)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} but no arguments were provided");
                                    Globals.Logger.User("Usage: format(datformat, ...);");
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
                                    Globals.Logger.User($"Invoked {command.Name} and expected exactly 1 argument, but {command.Arguments.Count} arguments were provided");
                                    Globals.Logger.User("Usage: output(outdir);");
                                    continue;
                                }

                                // Only set the first as the output directory
                                outputDirectory = command.Arguments[0];
                                break;

                            // Write out the current DatFile
                            case "write":
                                if (command.Arguments.Count > 1)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} and expected 0-1 arguments, but {command.Arguments.Count} arguments were provided");
                                    Globals.Logger.User("Usage: write([overwrite = true]);");
                                    continue;
                                }

                                // Get overwrite value, if possible
                                bool overwrite = command.Arguments.Count == 1 ? command.Arguments[0].AsYesNo() ?? true : true;

                                // Write out the dat with the current state
                                datFile.Write(outputDirectory, overwrite: overwrite);
                                break;

                            // Reset the internal state
                            case "reset":
                                if (command.Arguments.Count != 0)
                                {
                                    Globals.Logger.User($"Invoked {command.Name} and expected no arguments, but {command.Arguments.Count} arguments were provided");
                                    Globals.Logger.User("Usage: reset();");
                                    continue;
                                }

                                // Reset all state variables
                                index = 0;
                                datFile = DatFile.Create();
                                outputDirectory = null;
                                break;

                            default:
                                Globals.Logger.User($"Could not find a match for '{command.Name}'. Please see the help text for more details.");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Globals.Logger.Error($"There was an exception processing {path}: {ex}");
                    continue;
                }
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
                string splitRegex = @"^(\S+)\((.*?)\);";
                var match = Regex.Match(line, splitRegex);

                // If we didn't get a success, just return null
                if (!match.Success)
                    return null;

                // Otherwise, get the name and arguments
                string commandName = match.Groups[1].Value;
                List<string> arguments = match
                    .Groups[2]
                    .Value
                    .Split(',')
                    .Select(s => s.Trim().Trim('"').Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s)) // TODO: This may interfere with header value replacement
                    .ToList();

                return new BatchCommand { Name = commandName, Arguments = arguments };
            }
        }
    }
}
