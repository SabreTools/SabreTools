using System;
using System.Collections.Generic;
using SabreTools.Help.Inputs;

namespace SabreTools.Help
{
    /// <summary>
    /// Represents a logically-grouped set of user inputs
    /// </summary>
    /// <remarks>
    /// It is recommended to use this class as the primary
    /// way to address user inputs from the application. It
    /// is also recommended that all directly-included
    /// inputs are <see cref="Feature"/> unless the implementing
    /// program only has a single utility.
    /// </remarks>
    public class FeatureSet
    {
        #region Private variables

        /// <summary>
        /// Preamble used when printing help text to console
        /// </summary>
        private readonly List<string> _header = [];

        /// <summary>
        /// Set of all user inputs in this grouping
        /// </summary>
        /// <remarks>
        /// Only the top level inputs need to be defined. All
        /// children will be included by default.
        /// </remarks>
        private readonly Dictionary<string, UserInput> _inputs = [];

        #endregion

        #region Constructors

        public FeatureSet(List<string> header)
        {
            _header.AddRange(header);
        }

        #endregion

        #region Accessors

        public UserInput? this[string name]
        {
            get
            {
                if (!_inputs.ContainsKey(name))
                    return null;

                return _inputs[name];
            }
        }

        public UserInput? this[UserInput subfeature]
        {
            get
            {
                if (subfeature.Name == null)
                    return null;

                if (!_inputs.ContainsKey(subfeature.Name))
                    return null;

                return _inputs[subfeature.Name];
            }
        }

        /// <summary>
        /// Add a new input to the set
        /// </summary>
        /// <param name="input">UserInput object to map to</param>
        public void Add(UserInput input)
        {
            if (input.Name == null)
                return;

            _inputs.Add(input.Name, input);
        }

        #endregion

        #region Inputs

        /// <summary>
        /// Get the input name for a given flag or short name
        /// </summary>
        public string GetInputName(string name)
        {
            // Pre-split the input for efficiency
            string[] splitInput = name.Split('=');

            foreach (var key in _inputs.Keys)
            {
                // Skip invalid features
                var feature = _inputs[key];
                if (feature == null)
                    continue;

                // Validate the name matches
                if (feature.Name == splitInput[0])
                    return key;

                // Validate the flag is contained
                if (feature.ContainsFlag(splitInput[0]))
                    return key;
            }

            // No feature could be found
            return string.Empty;
        }

        /// <summary>
        /// Check if a flag is a top-level (main application) flag
        /// </summary>
        /// <param name="flag">Name of the flag to check</param>
        /// <returns>True if the feature was found, false otherwise</returns>
        public bool TopLevelFlag(string flag)
            => GetInputName(flag).Length > 0;

        #endregion

        #region Output

        /// <summary>
        /// Output top-level features only
        /// </summary>
        public void OutputGenericHelp()
        {
            // Start building the output list
            List<string> output = [];

            // Append the header first
            output.AddRange(_header);

            // Now append all available top-level flags
            output.Add("Available options:");
            foreach (string feature in _inputs.Keys)
            {
                var outputs = _inputs[feature]?.Output(pre: 2, midpoint: 30);
                if (outputs != null)
                    output.AddRange(outputs);
            }

            // And append the generic ending
            output.Add(string.Empty);
            output.Add("For information on available flags, put the option name after help");

            // Now write out everything in a staged manner
            WriteOutWithPauses(output);
        }

        /// <summary>
        /// Output all features recursively
        /// </summary>
        public void OutputAllHelp()
        {
            // Start building the output list
            List<string> output = [];

            // Append the header first
            output.AddRange(_header);

            // Now append all available flags recursively
            output.Add("Available options:");
            foreach (string feature in _inputs.Keys)
            {
                var outputs = _inputs[feature]?.OutputRecursive(0, pre: 2, midpoint: 30, includeLongDescription: true);
                if (outputs != null)
                    output.AddRange(outputs);
            }

            // Now write out everything in a staged manner
            WriteOutWithPauses(output);
        }

        /// <summary>
        /// Output a single feature recursively
        /// </summary>
        /// <param name="featurename">Name of the feature to output information for, if possible</param>
        /// <param name="includeLongDescription">True if the long description should be formatted and output, false otherwise</param>
        public void OutputIndividualFeature(string? featurename, bool includeLongDescription = false)
        {
            // Start building the output list
            List<string> output = [];

            // If the feature name is null, empty, or just consisting of `-` characters, just show everything
            if (string.IsNullOrEmpty(featurename?.TrimStart('-')))
            {
                OutputGenericHelp();
                return;
            }

            // Now try to find the feature that has the name included
            string? realname = null;
            List<string> startsWith = [];
            foreach (string feature in _inputs.Keys)
            {
                // If we have a match to the feature name somehow
                if (feature == featurename)
                {
                    realname = feature;
                    break;
                }

                // If we have an invalid feature
                else if (!_inputs.ContainsKey(feature) || _inputs[feature] == null)
                {
                    startsWith.Add(feature);
                }

                // If we have a match within the flags
                else if (_inputs[feature]!.ContainsFlag(featurename!))
                {
                    realname = feature;
                    break;
                }

                // Otherwise, we want to get features with the same start
                else if (_inputs[feature]!.StartsWith(featurename!.TrimStart('-')[0]))
                {
                    startsWith.Add(feature);
                }
            }

            // If we have a real name found, append all available subflags recursively
            if (realname != null)
            {
                output.Add($"Available options for {realname}:");
                output.AddRange(_inputs[realname]!.OutputRecursive(0, pre: 2, midpoint: 30, includeLongDescription: includeLongDescription));
            }

            // If no name was found but we have possible matches, show them
            else if (startsWith.Count > 0)
            {
                output.Add($"\"{featurename}\" not found. Did you mean:");
                foreach (string possible in startsWith)
                {
                    output.AddRange(_inputs[possible]!.Output(pre: 2, midpoint: 30, includeLongDescription: includeLongDescription));
                }
            }

            // Now write out everything in a staged manner
            WriteOutWithPauses(output);
        }

        /// <summary>
        /// Pause on console output
        /// </summary>
        private static void Pause()
        {
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
        /// Write out the help text with pauses, if needed
        /// </summary>
        private static void WriteOutWithPauses(List<string> helptext)
        {
            // Now output based on the size of the screen
            int i = 0;
            for (int line = 0; line < helptext.Count; line++)
            {
                string help = helptext[line];

                Console.WriteLine(help);
                i++;

                // If we're not being redirected and we reached the size of the screen, pause
                if (i == Console.WindowHeight - 3 && line != helptext.Count - 1)
                {
                    i = 0;
                    Pause();
                }
            }

            Pause();
        }

        #endregion
    }
}
