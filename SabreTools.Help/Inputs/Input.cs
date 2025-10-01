using System;
using System.Collections.Generic;
using System.Text;

namespace SabreTools.Help.Inputs
{
    /// <summary>
    /// Represents a single input for an execution context
    /// </summary>
    public abstract class Input
    {
        #region Properties

        /// <summary>
        /// Display name for the input
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Set of flags for the input
        /// </summary>
        protected readonly string[] _flags;

        /// <summary>
        /// Short description for printing
        /// </summary>
        protected readonly string _description;

        /// <summary>
        /// Optional extended description for printing
        /// </summary>
        protected readonly string? _longDescription;

        /// <summary>
        /// Indicates if the value following is required or not
        /// </summary>
        protected readonly bool _required;

        /// <summary>
        /// Indicates if a value has been set
        /// </summary>
        public abstract bool ValueSet { get; }

        /// <summary>
        /// All children inputs that require the parent to exist
        /// </summary>
        public readonly Dictionary<string, Input> Children = [];

        #endregion

        #region Constructors

        public Input(string name, string[] flags, string description)
        {
            if (flags.Length == 0)
                throw new ArgumentException($"{nameof(flags)} requires at least one value");

            Name = name;
            _flags = flags;
            _description = description;
            _longDescription = null;
            _required = true;
        }

        public Input(string name, string[] flags, string description, bool required)
        {
            if (flags.Length == 0)
                throw new ArgumentException($"{nameof(flags)} requires at least one value");

            Name = name;
            _flags = flags;
            _description = description;
            _longDescription = null;
            _required = required;
        }

        public Input(string name, string[] flags, string description, string longDescription, bool required)
        {
            if (flags.Length == 0)
                throw new ArgumentException($"{nameof(flags)} requires at least one value");

            Name = name;
            _flags = flags;
            _description = description;
            _longDescription = longDescription;
            _required = required;
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Add a child to the dictionary
        /// </summary>
        /// <param name="child">Input to add to the dictionary</param>
        /// <returns>True if the input could be added, false otherwise</returns>
        public bool AddChild(Input child)
        {
            // Validate there is a key to use
            if (child.Name.Length == 0)
                throw new ArgumentNullException($"{nameof(child)} must contain a valid name");

            // Don't allow duplicate children
            if (Children.ContainsKey(child.Name))
                return false;

            // Add the child
            Children.Add(child.Name, child);
            return true;
        }

        /// <summary>
        /// Clear any accumulated value
        /// </summary>
        public abstract void ClearValue();

        /// <summary>
        /// Indicates the flag is valid for this input
        /// </summary>
        public bool MatchesFlag(string flag)
            => Array.Exists(_flags, f => f == flag);

        /// <summary>
        /// Indicates if the input contains a flag that starts with the given character
        /// </summary>
        public bool StartsWith(char c)
            => Array.Exists(_flags, f => f.TrimStart('-').ToLowerInvariant()[0] == c);

        #endregion

        #region Formatting

        /// <summary>
        /// Create a formatted representation of the input and possible value
        /// </summary>
        /// <param name="useEquals">Use an equal sign as a separator on output</param>
        public abstract string Format(bool useEquals);

        #endregion

        #region Processing

        /// <summary>
        /// Process the current index, if possible
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>True if a value could be determined, false otherwise</returns>
        public abstract bool Process(string[] parts, ref int index);

        #endregion

        #region Printing

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
            output.Append(_description);

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
                string[]? split = _longDescription?.Split(' ');
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
            output.Append(_description);

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
                string[]? split = _longDescription?.Split(' ');
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
            foreach (string feature in Children.Keys)
            {
                outputList.AddRange(Children[feature]!.OutputRecursive(tabLevel + 1, pre, midpoint, includeLongDescription));
            }

            return outputList;
        }

        /// <summary>
        /// Pre-format the flags for output
        /// </summary>
        protected abstract string FormatFlags();

        #endregion

        #region Helpers

        /// <summary>
        /// Create a padding space based on the given length
        /// </summary>
        /// <param name="spaces">Number of padding spaces to add</param>
        /// <returns>String with requested number of blank spaces</returns>
        internal static string CreatePadding(int spaces)
            => string.Empty.PadRight(spaces);

        /// <summary>
        /// Get the trimmed value and multiplication factor from a value
        /// </summary>
        /// <param name="value">String value to treat as suffixed number</param>
        /// <returns>Trimmed value and multiplication factor</returns>
        internal static string ExtractFactorFromValue(string value, out long factor)
        {
            value = value.Trim('"');
            factor = 1;

            // Characters
            if (value.EndsWith("c", StringComparison.Ordinal))
            {
                factor = 1;
                value = value.TrimEnd('c');
            }

            // Words
            else if (value.EndsWith("w", StringComparison.Ordinal))
            {
                factor = 2;
                value = value.TrimEnd('w');
            }

            // Double Words
            else if (value.EndsWith("d", StringComparison.Ordinal))
            {
                factor = 4;
                value = value.TrimEnd('d');
            }

            // Quad Words
            else if (value.EndsWith("q", StringComparison.Ordinal))
            {
                factor = 8;
                value = value.TrimEnd('q');
            }

            // Kilobytes
            else if (value.EndsWith("k", StringComparison.Ordinal))
            {
                factor = 1024;
                value = value.TrimEnd('k');
            }

            // Megabytes
            else if (value.EndsWith("M", StringComparison.Ordinal))
            {
                factor = 1024 * 1024;
                value = value.TrimEnd('M');
            }

            // Gigabytes
            else if (value.EndsWith("G", StringComparison.Ordinal))
            {
                factor = 1024 * 1024 * 1024;
                value = value.TrimEnd('G');
            }

            return value;
        }

        /// <summary>
        /// Removes a leading 0x if it exists, case insensitive
        /// </summary>
        /// <param name="value">String with removed leading 0x</param>
        /// <returns></returns>
        internal static string RemoveHexIdentifier(string value)
        {
            if (value.Length <= 2)
                return value;
            if (value[0] != '0')
                return value;
            if (value[1] != 'x' && value[1] != 'X')
                return value;

            return value.Substring(2);
        }

        #endregion
    }
}
