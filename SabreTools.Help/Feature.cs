﻿using System;
using System.Collections.Generic;

namespace SabreTools.Help
{
    public class Feature
    {
        #region Protected instance variables

        protected ParameterType _featureType;
        protected bool _foundOnce = false;
        protected object? _value = null;

        #endregion

        #region Publicly facing variables

        public string? Name { get; protected set; }
        public readonly List<string> Flags = [];
        public string? Description { get; protected set; }
        public string? LongDescription { get; protected set; }
        public readonly Dictionary<string, Feature?> Features = [];

        #endregion

        #region Constructors

        /// <summary>
        /// Only used by inheriting classes
        /// </summary>
        protected Feature()
        {
        }

        public Feature(string name, string flag, string description, ParameterType featureType, string? longDescription = null)
        {
            _featureType = featureType;
            Name = name;
            Flags.Add(flag);
            Description = description;
            LongDescription = longDescription;
        }

        public Feature(string name, List<string> flags, string description, ParameterType featureType, string? longDescription = null)
        {
            _featureType = featureType;
            Name = name;
            Flags.AddRange(flags);
            Description = description;
            LongDescription = longDescription;
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Directly address a given subfeature
        /// </summary>
        public Feature? this[string name]
        {
            get { return Features.ContainsKey(name) ? Features[name] : null; }
            set { Features[name] = value; }
        }

        /// <summary>
        /// Directly address a given subfeature
        /// </summary>
        public Feature? this[Feature? subfeature]
        {
            get { return Features.ContainsKey(subfeature?.Name ?? string.Empty) ? Features[subfeature?.Name ?? string.Empty] : null; }
            set { Features[subfeature?.Name ?? string.Empty] = value; }
        }

        /// <summary>
        /// Add a new feature for this feature
        /// </summary>
        /// <param name="feature"></param>
        public void AddFeature(Feature feature)
        {
            lock (Features)
            {
                Features[feature.Name ?? string.Empty] = feature;
            }
        }

        /// <summary>
        /// Add a new flag for this feature
        /// </summary>
        /// <param name="flag">Flag to add for this feature</param>
        public void AddFlag(string flag)
        {
            lock (Flags)
            {
                Flags.Add(flag);
            }
        }

        /// <summary>
        /// Add a set of new flags for this feature
        /// </summary>
        /// <param name="flags">List of flags to add to this feature</param>
        public void AddFlags(List<string> flags)
        {
            lock (Flags)
            {
                Flags.AddRange(flags);
            }
        }

        /// <summary>
        /// Returns if a flag exists for the current feature
        /// </summary>
        /// <param name="name">Name of the flag to check</param>
        /// <returns>True if the flag was found, false otherwise</returns>
        public bool ContainsFlag(string name)
        {
            return Flags.Exists(f => f == name || f.TrimStart('-') == name);
        }

        /// <summary>
        /// Returns if the feature contains a flag that starts with the given character
        /// </summary>
        /// <param name="c">Character to check against</param>
        /// <returns>True if the flag was found, false otherwise</returns>
        public bool StartsWith(char c)
        {
            return Flags.Exists(f => f.TrimStart('-').ToLowerInvariant()[0] == c);
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Output this feature only
        /// </summary>
        /// <param name="pre">Positive number representing number of spaces to put in front of the feature</param>
        /// <param name="midpoint">Positive number representing the column where the description should start</param>
        /// <param name="includeLongDescription">True if the long description should be formatted and output, false otherwise</param>
        public List<string> Output(int pre = 0, int midpoint = 0, bool includeLongDescription = false)
        {
            // Create the output list
            List<string> outputList = [];

            // Build the output string first
            string output = string.Empty;

            // Add the pre-space first
            output += CreatePadding(pre);

            // Preprocess the flags, if necessary
            string[] newflags = new string[Flags.Count];
            Flags.CopyTo(newflags);
            switch (_featureType)
            {
                case ParameterType.Int32:
                case ParameterType.Int64:
                case ParameterType.List:
                case ParameterType.String:
                    for (int i = 0; i < newflags.Length; i++)
                    {
                        newflags[i] += "=";
                    }
                    break;
                case ParameterType.Flag:
                default:
                    // No-op
                    break;
            }

            // Now add all flags
            output += string.Join(", ", newflags);

            // If we have a midpoint set, check to see if the string needs padding
            if (midpoint > 0 && output.Length < midpoint)
                output += CreatePadding(midpoint - output.Length);
            else
                output += " ";

            // Append the description
            output += Description;

            // Now append it to the list
            outputList.Add(output);

            // If we are outputting the long description, format it and then add it as well
            if (includeLongDescription)
            {
                // Get the width of the console for wrapping reference
                int width = Console.WindowWidth - 1;

                // Prepare the output string
                output = CreatePadding(pre + 4);

                // Now split the input description and start processing
                string[]? split = LongDescription?.Split(' ');
                if (split == null)
                    return outputList;

                for (int i = 0; i < split.Length; i++)
                {
                    // If we have a newline character, reset the line and continue
                    if (split[i].Contains("\n"))
                    {
                        string[] subsplit = split[i].Replace("\r", string.Empty).Split('\n');
                        for (int j = 0; j < subsplit.Length - 1; j++)
                        {
                            // Add the next word only if the total length doesn't go above the width of the screen
                            if (output.Length + subsplit[j].Length < width)
                            {
                                output += (output.Length == pre + 4 ? string.Empty : " ") + subsplit[j];
                            }
                            // Otherwise, we want to cache the line to output and create a new blank string
                            else
                            {
                                outputList.Add(output);
                                output = CreatePadding(pre + 4);
                                output += (output.Length == pre + 4 ? string.Empty : " ") + subsplit[j];
                            }

                            outputList.Add(output);
                            output = CreatePadding(pre + 4);
                        }

                        output += subsplit[subsplit.Length - 1];
                        continue;
                    }

                    // Add the next word only if the total length doesn't go above the width of the screen
                    if (output.Length + split[i].Length < width)
                    {
                        output += (output.Length == pre + 4 ? string.Empty : " ") + split[i];
                    }
                    // Otherwise, we want to cache the line to output and create a new blank string
                    else
                    {
                        outputList.Add(output);
                        output = CreatePadding(pre + 4);
                        output += (output.Length == pre + 4 ? string.Empty : " ") + split[i];
                    }
                }

                // Add the last created output and a blank line for clarity
                outputList.Add(output);
                outputList.Add(string.Empty);
            }

            return outputList;
        }

        /// <summary>
        /// Create a padding space based on the given length
        /// </summary>
        /// <param name="spaces">Number of padding spaces to add</param>
        /// <returns>String with requested number of blank spaces</returns>
        private static string CreatePadding(int spaces)
        {
            return string.Empty.PadRight(spaces);
        }

        /// <summary>
        /// Output this feature and all subfeatures
        /// </summary>
        /// <param name="tabLevel">Level of indentation for this feature</param>
        /// <param name="pre">Positive number representing number of spaces to put in front of the feature</param>
        /// <param name="midpoint">Positive number representing the column where the description should start</param>
        /// <param name="includeLongDescription">True if the long description should be formatted and output, false otherwise</param>
        public List<string> OutputRecursive(int tabLevel, int pre = 0, int midpoint = 0, bool includeLongDescription = false)
        {
            // Create the output list
            List<string> outputList = [];

            // Build the output string first
            string output = string.Empty;

            // Normalize based on the tab level
            int preAdjusted = pre;
            int midpointAdjusted = midpoint;
            if (tabLevel > 0)
            {
                preAdjusted += 4 * tabLevel;
                midpointAdjusted += 4 * tabLevel;
            }

            // Add the pre-space first
            output += CreatePadding(preAdjusted);

            // Preprocess the flags, if necessary
            string[] newflags = new string[Flags.Count];
            Flags.CopyTo(newflags);
            switch (_featureType)
            {
                case ParameterType.Int32:
                case ParameterType.Int64:
                case ParameterType.List:
                case ParameterType.String:
                    for (int i = 0; i < newflags.Length; i++)
                    {
                        newflags[i] += "=";
                    }
                    break;
                case ParameterType.Flag:
                default:
                    // No-op
                    break;
            }

            // Now add all flags
            output += string.Join(", ", newflags);

            // If we have a midpoint set, check to see if the string needs padding
            if (midpoint > 0 && output.Length < midpointAdjusted)
                output += CreatePadding(midpointAdjusted - output.Length);
            else
                output += " ";

            // Append the description
            output += Description;

            // Now append it to the list
            outputList.Add(output);

            // If we are outputting the long description, format it and then add it as well
            if (includeLongDescription)
            {
                // Get the width of the console for wrapping reference
                int width = Console.WindowWidth - 1;

                // Prepare the output string
                output = CreatePadding(preAdjusted + 4);

                // Now split the input description and start processing
                string[]? split = LongDescription?.Split(' ');
                if (split == null)
                    return outputList;

                for (int i = 0; i < split.Length; i++)
                {
                    // If we have a newline character, reset the line and continue
                    if (split[i].Contains("\n"))
                    {
                        string[] subsplit = split[i].Replace("\r", string.Empty).Split('\n');
                        for (int j = 0; j < subsplit.Length - 1; j++)
                        {
                            // Add the next word only if the total length doesn't go above the width of the screen
                            if (output.Length + subsplit[j].Length < width)
                            {
                                output += (output.Length == preAdjusted + 4 ? string.Empty : " ") + subsplit[j];
                            }
                            // Otherwise, we want to cache the line to output and create a new blank string
                            else
                            {
                                outputList.Add(output);
                                output = CreatePadding(preAdjusted + 4);
                                output += (output.Length == preAdjusted + 4 ? string.Empty : " ") + subsplit[j];
                            }

                            outputList.Add(output);
                            output = CreatePadding(preAdjusted + 4);
                        }

                        output += subsplit[subsplit.Length - 1];
                        continue;
                    }

                    // Add the next word only if the total length doesn't go above the width of the screen
                    if (output.Length + split[i].Length < width)
                    {
                        output += (output.Length == preAdjusted + 4 ? string.Empty : " ") + split[i];
                    }
                    // Otherwise, we want to cache the line to output and create a new blank string
                    else
                    {
                        outputList.Add(output);
                        output = CreatePadding(preAdjusted + 4);
                        output += (output.Length == preAdjusted + 4 ? string.Empty : " ") + split[i];
                    }
                }

                // Add the last created output and a blank line for clarity
                outputList.Add(output);
                outputList.Add(string.Empty);
            }

            // Now let's append all subfeatures
            foreach (string feature in Features.Keys)
            {
                outputList.AddRange(Features[feature]!.OutputRecursive(tabLevel + 1, pre, midpoint, includeLongDescription));
            }

            return outputList;
        }

        /// <summary>
        /// Validate whether a flag is valid for this feature or not
        /// </summary>
        /// <param name="input">Input to check against</param>
        /// <param name="exact">True if just this feature should be checked, false if all subfeatures are checked as well</param>
        /// <param name="ignore">True if the existing flag should be ignored, false otherwise</param>
        /// <returns>True if the flag was valid, false otherwise</returns>
        public bool ValidateInput(string input, bool exact = false, bool ignore = false)
        {
            bool valid = false;

            // Pre-split the input for efficiency
            string[] splitInput = input.Split('=');

            // Determine what we should be looking for
            switch (_featureType)
            {
                // If we have a flag, make sure it doesn't have an equal sign in it 
                case ParameterType.Flag:
                    valid = !input.Contains("=") && Flags.Contains(input);
                    if (valid)
                    {
                        _value = true;

                        // If we've already found this feature before
                        if (_foundOnce && !ignore)
                            valid = false;

                        _foundOnce = true;
                    }

                    break;

                // If we have an Int32, try to parse it if at all possible
                case ParameterType.Int32:
                    valid = input.Contains("=") && Flags.Contains(splitInput[0]);
                    if (valid)
                    {
                        if (!Int32.TryParse(splitInput[1], out int value))
                            value = Int32.MinValue;

                        _value = value;

                        // If we've already found this feature before
                        if (_foundOnce && !ignore)
                            valid = false;

                        _foundOnce = true;
                    }

                    break;

                // If we have an Int32, try to parse it if at all possible
                case ParameterType.Int64:
                    valid = input.Contains("=") && Flags.Contains(splitInput[0]);
                    if (valid)
                    {
                        if (!Int64.TryParse(splitInput[1], out long value))
                            value = Int64.MinValue;

                        _value = value;

                        // If we've already found this feature before
                        if (_foundOnce && !ignore)
                            valid = false;

                        _foundOnce = true;
                    }

                    break;

                // If we have an input, make sure it has an equals sign in it
                case ParameterType.List:
                    valid = input.Contains("=") && Flags.Contains(splitInput[0]);
                    if (valid)
                    {
                        _value ??= new List<string>();
                        (_value as List<string>)?.Add(string.Join("=", splitInput, 1, splitInput.Length - 1));
                    }

                    break;

                case ParameterType.String:
                    valid = input.Contains("=") && Flags.Contains(input.Split('=')[0]);
                    if (valid)
                    {
                        _value = string.Join("=", splitInput, 1, splitInput.Length - 1);

                        // If we've already found this feature before
                        if (_foundOnce && !ignore)
                            valid = false;

                        _foundOnce = true;
                    }

                    break;
            }

            // If we haven't found a valid flag and we're not looking for just this feature, check to see if any of the subfeatures are valid
            if (!valid && !exact)
            {
                string[] featureKeys = [.. Features.Keys];
                valid = Array.Exists(featureKeys, k => Features[k]!.ValidateInput(input));
            }

            return valid;
        }

        /// <summary>
        /// Get the boolean value associated with this feature
        /// </summary>
        public bool GetBoolValue()
        {
            if (_featureType != ParameterType.Flag)
                throw new ArgumentException("Feature is not a flag");

            return (_value as bool?) ?? false;
        }

        /// <summary>
        /// Get the string value associated with this feature
        /// </summary>
        public string? GetStringFieldValue()
        {
            if (_featureType != ParameterType.String)
                throw new ArgumentException("Feature is not a string");

            return _value as string;
        }

        /// <summary>
        /// Get the Int32 value associated with this feature
        /// </summary>
        public int GetInt32Value()
        {
            if (_featureType != ParameterType.Int32)
                throw new ArgumentException("Feature is not an int");

            return (_value as int?) ?? int.MinValue;
        }

        /// <summary>
        /// Get the Int64 value associated with this feature
        /// </summary>
        public long GetInt64Value()
        {
            if (_featureType != ParameterType.Int64)
                throw new ArgumentException("Feature is not a long");

            return (_value as long?) ?? long.MinValue;
        }

        /// <summary>
        /// Get the List\<string\> value associated with this feature
        /// </summary>
        public List<string> GetListValue()
        {
            if (_featureType != ParameterType.List)
                throw new ArgumentException("Feature is not a list");

            return (_value as List<string>) ?? [];
        }

        /// <summary>
        /// Returns if this feature has a valid value or not
        /// </summary>
        /// <returns>True if the feature is enabled, false otherwise</returns>
        public bool IsEnabled()
        {
            return _featureType switch
            {
                ParameterType.Flag => (_value as bool?) == true,
                ParameterType.String => (_value as string) != null,
                ParameterType.Int32 => (_value as int?).HasValue && (_value as int?)!.Value != int.MinValue,
                ParameterType.Int64 => (_value as long?).HasValue && (_value as long?)!.Value != long.MinValue,
                ParameterType.List => (_value as List<string>) != null,
                _ => false,
            };
        }

        #endregion
    }
}
