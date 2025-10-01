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
    /// way to address user inputs from the application
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
        private readonly Dictionary<string, UserInput> _features = [];

        /// <summary>
        /// Custom formatting string for writing to console
        /// </summary>
        private const string _barrier = "-----------------------------------------";

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
                if (!_features.ContainsKey(name))
                    return null;

                return _features[name];
            }
        }

        public UserInput? this[UserInput subfeature]
        {
            get
            {
                if (subfeature.Name == null)
                    return null;

                if (!_features.ContainsKey(subfeature.Name))
                    return null;

                return _features[subfeature.Name];
            }
        }

        /// <summary>
        /// Add a new feature to the help
        /// </summary>
        /// <param name="feature">Feature object to map to</param>
        public void Add(UserInput feature)
        {
            if (feature.Name == null)
                return;

            lock (_features)
            {
                _features.Add(feature.Name, feature);
            }
        }

        #endregion

        #region Instance Methods

        /// <summary>
        /// Get the feature name for a given flag or short name
        /// </summary>
        /// <returns>Feature name</returns>
        public string GetFeatureName(string name)
        {
            foreach (var key in _features.Keys)
            {
                // Skip invalid features
                var feature = _features[key];
                if (feature == null)
                    continue;

                // If validation passes
                if (feature.ValidateInput(name, exact: true, ignore: true))
                    return key;
            }

            // No feature could be found
            return string.Empty;
        }

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
            foreach (string feature in _features.Keys)
            {
                var outputs = _features[feature]?.Output(pre: 2, midpoint: 30);
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
            foreach (string feature in _features.Keys)
            {
                var outputs = _features[feature]?.OutputRecursive(0, pre: 2, midpoint: 30, includeLongDescription: true);
                if (outputs != null)
                    output.AddRange(outputs);
            }

            // Now write out everything in a staged manner
            WriteOutWithPauses(output);
        }

        /// <summary>
        /// Output the SabreTools suite credits
        /// </summary>
        public static void OutputCredits()
        {
            List<string> credits =
            [
                _barrier,
                "Credits",
                _barrier,
                string.Empty,
                "Programmer / Lead:	Matt Nadareski (darksabre76)",
                "Additional code:	emuLOAD, @tractivo, motoschifo",
                "Testing:		emuLOAD, @tractivo, Kludge, Obiwantje, edc",
                "Suggestions:		edc, AcidX, Amiga12, EliUmniCk",
                "Based on work by:	The Wizard of DATz"
            ];
            WriteOutWithPauses(credits);
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
            foreach (string feature in _features.Keys)
            {
                // If we have a match to the feature name somehow
                if (feature == featurename)
                {
                    realname = feature;
                    break;
                }

                // If we have an invalid feature
                else if (!_features.ContainsKey(feature) || _features[feature] == null)
                {
                    startsWith.Add(feature);
                }

                // If we have a match within the flags
                else if (_features[feature]!.ContainsFlag(featurename!))
                {
                    realname = feature;
                    break;
                }

                // Otherwise, we want to get features with the same start
                else if (_features[feature]!.StartsWith(featurename!.TrimStart('-')[0]))
                {
                    startsWith.Add(feature);
                }
            }

            // If we have a real name found, append all available subflags recursively
            if (realname != null)
            {
                output.Add($"Available options for {realname}:");
                output.AddRange(_features[realname]!.OutputRecursive(0, pre: 2, midpoint: 30, includeLongDescription: includeLongDescription));
            }

            // If no name was found but we have possible matches, show them
            else if (startsWith.Count > 0)
            {
                output.Add($"\"{featurename}\" not found. Did you mean:");
                foreach (string possible in startsWith)
                {
                    output.AddRange(_features[possible]!.Output(pre: 2, midpoint: 30, includeLongDescription: includeLongDescription));
                }
            }

            // Now write out everything in a staged manner
            WriteOutWithPauses(output);
        }

        /// <summary>
        /// Check if a flag is a top-level (main application) flag
        /// </summary>
        /// <param name="flag">Name of the flag to check</param>
        /// <returns>True if the feature was found, false otherwise</returns>
        public bool TopLevelFlag(string flag)
        {
            foreach (var key in _features.Keys)
            {
                // Skip invalid features
                var feature = _features[key];
                if (feature == null)
                    continue;

                // If validation passes
                if (feature.ValidateInput(flag, exact: true))
                    return true;
            }

            // No feature could be found
            return false;
        }

        /// <summary>
        /// Retrieve a list of enabled features
        /// </summary>
        /// <returns>List of Features representing what is enabled</returns>
        public Dictionary<string, UserInput?> GetEnabledFeatures()
        {
            Dictionary<string, UserInput?> enabled = [];

            // Loop through the features
            foreach (var feature in _features)
            {
                var temp = GetEnabledSubfeatures(feature.Key, feature.Value);
                foreach (var tempfeat in temp)
                {
                    if (!enabled.ContainsKey(tempfeat.Key))
                        enabled.Add(tempfeat.Key, null);

                    enabled[tempfeat.Key] = tempfeat.Value;
                }
            }

            return enabled;
        }

        /// <summary>
        /// Retrieve a nested list of subfeatures from the current feature
        /// </summary>
        /// <param name="key">Name that should be assigned to the feature</param>
        /// <param name="feature">Feature with possible subfeatures to test</param>
        /// <returns>List of Features representing what is enabled</returns>
        private static Dictionary<string, UserInput?> GetEnabledSubfeatures(string key, UserInput? feature)
        {
            Dictionary<string, UserInput?> enabled = [];

            // If the feature is invalid
            if (feature == null)
                return enabled;

            // First determine if the current feature is enabled
            if (feature.IsEnabled())
                enabled.Add(key, feature);

            // Now loop through the subfeatures recursively
            foreach (KeyValuePair<string, UserInput?> sub in feature.Features)
            {
                var temp = GetEnabledSubfeatures(sub.Key, sub.Value);
                foreach (var tempfeat in temp)
                {
                    if (!enabled.ContainsKey(tempfeat.Key))
                        enabled.Add(tempfeat.Key, null);

                    enabled[tempfeat.Key] = tempfeat.Value;
                }
            }

            return enabled;
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

        /// <summary>
        /// Pause on console output
        /// </summary>
        private static void Pause()
        {
#if NET452_OR_GREATER || NETCOREAPP
            if (!Console.IsOutputRedirected)
#endif
            {
                Console.WriteLine();
                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();
            }
        }

        #endregion
    }
}
