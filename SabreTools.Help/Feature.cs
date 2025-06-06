﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SabreTools.Help
{
    public abstract class Feature
    {
        #region Fields

        // <summary>
        /// Indicates if the feature has been seen already
        /// </summary>
        protected bool _foundOnce = false;

        #endregion

        #region Properties

        /// <summary>
        /// Display name for the feature
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Set of flags associated with the feature
        /// </summary>
        public readonly List<string> Flags = [];

        /// <summary>
        /// Short description of the feature
        /// </summary>
        public string Description { get; protected set; }

        /// <summary>
        /// Optional long description of the feature
        /// </summary>
        public string? LongDescription { get; protected set; }

        /// <summary>
        /// Set of subfeatures associated with this feature
        /// </summary>
        public readonly Dictionary<string, Feature?> Features = [];

        #endregion

        #region Constructors

        internal Feature(string name, string flag, string description, string? longDescription = null)
        {
            Name = name;
            Flags.Add(flag);
            Description = description;
            LongDescription = longDescription;
        }

        internal Feature(string name, string[] flags, string description, string? longDescription = null)
        {
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
        public Feature? this[Feature subfeature]
        {
            get { return Features.ContainsKey(subfeature.Name) ? Features[subfeature.Name] : null; }
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
            var output = new StringBuilder();

            // Add the pre-space first
            output.Append(CreatePadding(pre));

            // Preprocess and add the flags
            output.Append(FormatFlags());

            // If we have a midpoint set, check to see if the string needs padding
            if (midpoint > 0 && output.ToString().Length < midpoint)
                output.Append(CreatePadding(midpoint - output.ToString().Length));
            else
                output.Append(" ");

            // Append the description
            output.Append(Description);

            // Now append it to the list
            outputList.Add(output.ToString());

            // If we are outputting the long description, format it and then add it as well
            if (includeLongDescription)
            {
                // Get the width of the console for wrapping reference
                int width = (Console.WindowWidth == 0 ? 80 : Console.WindowWidth) - 1;

                // Prepare the output string
#if NET20 || NET35
                output = new();
#else
                output.Clear();
#endif
                output.Append(CreatePadding(pre + 4));

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
                            if (output.ToString().Length + subsplit[j].Length < width)
                            {
                                output.Append((output.ToString().Length == pre + 4 ? string.Empty : " ") + subsplit[j]);
                            }
                            // Otherwise, we want to cache the line to output and create a new blank string
                            else
                            {
                                outputList.Add(output.ToString());
#if NET20 || NET35
                                output = new();
#else
                                output.Clear();
#endif
                                output.Append(CreatePadding(pre + 4));
                                output.Append((output.ToString().Length == pre + 4 ? string.Empty : " ") + subsplit[j]);
                            }

                            outputList.Add(output.ToString());
#if NET20 || NET35
                            output = new();
#else
                            output.Clear();
#endif
                            output.Append(CreatePadding(pre + 4));
                        }

                        output.Append(subsplit[subsplit.Length - 1]);
                        continue;
                    }

                    // Add the next word only if the total length doesn't go above the width of the screen
                    if (output.ToString().Length + split[i].Length < width)
                    {
                        output.Append((output.ToString().Length == pre + 4 ? string.Empty : " ") + split[i]);
                    }
                    // Otherwise, we want to cache the line to output and create a new blank string
                    else
                    {
                        outputList.Add(output.ToString());
                        output.Append(CreatePadding(pre + 4));
                        output.Append((output.ToString().Length == pre + 4 ? string.Empty : " ") + split[i]);
                    }
                }

                // Add the last created output and a blank line for clarity
                outputList.Add(output.ToString());
                outputList.Add(string.Empty);
            }

            return outputList;
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
            var output = new StringBuilder();

            // Normalize based on the tab level
            int preAdjusted = pre;
            int midpointAdjusted = midpoint;
            if (tabLevel > 0)
            {
                preAdjusted += 4 * tabLevel;
                midpointAdjusted += 4 * tabLevel;
            }

            // Add the pre-space first
            output.Append(CreatePadding(preAdjusted));

            // Preprocess and add the flags
            output.Append(FormatFlags());

            // If we have a midpoint set, check to see if the string needs padding
            if (midpoint > 0 && output.ToString().Length < midpointAdjusted)
                output.Append(CreatePadding(midpointAdjusted - output.ToString().Length));
            else
                output.Append(" ");

            // Append the description
            output.Append(Description);

            // Now append it to the list
            outputList.Add(output.ToString());

            // If we are outputting the long description, format it and then add it as well
            if (includeLongDescription)
            {
                // Get the width of the console for wrapping reference
                int width = (Console.WindowWidth == 0 ? 80 : Console.WindowWidth) - 1;

                // Prepare the output string
#if NET20 || NET35
                output = new();
#else
                output.Clear();
#endif
                output.Append(CreatePadding(preAdjusted + 4));

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
                            if (output.ToString().Length + subsplit[j].Length < width)
                            {
                                output.Append((output.ToString().Length == preAdjusted + 4 ? string.Empty : " ") + subsplit[j]);
                            }
                            // Otherwise, we want to cache the line to output and create a new blank string
                            else
                            {
                                outputList.Add(output.ToString());
#if NET20 || NET35
                                output = new();
#else
                                output.Clear();
#endif
                                output.Append(CreatePadding(preAdjusted + 4));
                                output.Append((output.ToString().Length == preAdjusted + 4 ? string.Empty : " ") + subsplit[j]);
                            }

                            outputList.Add(output.ToString());
#if NET20 || NET35
                            output = new();
#else
                            output.Clear();
#endif
                            output.Append(CreatePadding(preAdjusted + 4));
                        }

                        output.Append(subsplit[subsplit.Length - 1]);
                        continue;
                    }

                    // Add the next word only if the total length doesn't go above the width of the screen
                    if (output.ToString().Length + split[i].Length < width)
                    {
                        output.Append((output.ToString().Length == preAdjusted + 4 ? string.Empty : " ") + split[i]);
                    }
                    // Otherwise, we want to cache the line to output and create a new blank string
                    else
                    {
                        outputList.Add(output.ToString());
#if NET20 || NET35
                        output = new();
#else
                        output.Clear();
#endif
                        output.Append(CreatePadding(preAdjusted + 4));
                        output.Append((output.ToString().Length == preAdjusted + 4 ? string.Empty : " ") + split[i]);
                    }
                }

                // Add the last created output and a blank line for clarity
                outputList.Add(output.ToString());
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
        public abstract bool ValidateInput(string input, bool exact = false, bool ignore = false);

        /// <summary>
        /// Returns if this feature has a valid value or not
        /// </summary>
        /// <returns>True if the feature is enabled, false otherwise</returns>
        public abstract bool IsEnabled();

        /// <summary>
        /// Pre-format the flags for output
        /// </summary>
        protected abstract string FormatFlags();

        /// <summary>
        /// Create a padding space based on the given length
        /// </summary>
        /// <param name="spaces">Number of padding spaces to add</param>
        /// <returns>String with requested number of blank spaces</returns>
        private static string CreatePadding(int spaces) => string.Empty.PadRight(spaces);

        #endregion
    }

    public abstract class Feature<T> : Feature
    {
        public T? Value { get; protected set; }

        #region Constructors

        internal Feature(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
        }

        internal Feature(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
        }

        #endregion

        #region Instance Methods

        /// <inheritdoc/>
        public override abstract bool ValidateInput(string input, bool exact = false, bool ignore = false);

        /// <inheritdoc/>
        public override abstract bool IsEnabled();

        /// <inheritdoc/>
        protected override abstract string FormatFlags();

        #endregion
    }
}
