using System;
using System.Collections.Generic;
using System.Linq;

namespace SabreTools.Library.Help
{
    public class Help
    {
        #region Private variables

        private List<string> _header;
        private Dictionary<string, Feature> _features;
        private static string _barrier = "-----------------------------------------";

        #endregion

        #region Constructors

        public Help()
        {
            _header = new List<string>();
            _features = new Dictionary<string, Feature>();
        }

        public Help(List<string> header)
        {
            _header = header;
            _features = new Dictionary<string, Feature>();
        }

        #endregion

        #region Accessors

        public Feature this[string name]
        {
            get
            {
                if (_features == null)
                    _features = new Dictionary<string, Feature>();

                if (!_features.ContainsKey(name))
                    return null;

                return _features[name];
            }
            set
            {
                if (_features == null)
                    _features = new Dictionary<string, Feature>();

                _features[name] = value;
            }
        }

        public Feature this[Feature subfeature]
        {
            get
            {
                if (_features == null)
                    _features = new Dictionary<string, Feature>();

                if (!_features.ContainsKey(subfeature.Name))
                    return null;

                return _features[subfeature.Name];
            }
            set
            {
                if (_features == null)
                    _features = new Dictionary<string, Feature>();

                _features[subfeature.Name] = value;
            }
        }

        /// <summary>
        /// Add a new feature to the help
        /// </summary>
        /// <param name="feature">Feature object to map to</param>
        public void Add(Feature feature)
        {
            if (_features == null)
                _features = new Dictionary<string, Feature>();

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
            return _features.Keys.FirstOrDefault(f => _features[f].ValidateInput(name, exact: true, ignore: true)) ?? string.Empty;
        }

        /// <summary>
        /// Output top-level features only
        /// </summary>
        public void OutputGenericHelp()
        {
            // Start building the output list
            List<string> output = new List<string>();

            // Append the header first
            output.AddRange(_header);

            // Now append all available top-level flags
            output.Add("Available options:");
            foreach (string feature in _features.Keys)
            {
                output.AddRange(_features[feature].Output(pre: 2, midpoint: 30));
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
            List<string> output = new List<string>();

            // Append the header first
            output.AddRange(_header);

            // Now append all available flags recursively
            output.Add("Available options:");
            foreach (string feature in _features.Keys)
            {
                output.AddRange(_features[feature].OutputRecursive(0, pre: 2, midpoint: 30, includeLongDescription: true));
            }

            // Now write out everything in a staged manner
            WriteOutWithPauses(output);
        }

        /// <summary>
        /// Output the SabreTools suite credits
        /// </summary>
        public void OutputCredits()
        {
            List<string> credits = new List<string>();
            credits.Add(_barrier);
            credits.Add("Credits");
            credits.Add(_barrier);
            credits.Add(string.Empty);
            credits.Add("Programmer / Lead:	Matt Nadareski (darksabre76)");
            credits.Add("Additional code:	emuLOAD, @tractivo, motoschifo");
            credits.Add("Testing:		emuLOAD, @tractivo, Kludge, Obiwantje, edc");
            credits.Add("Suggestions:		edc, AcidX, Amiga12, EliUmniCk");
            credits.Add("Based on work by:	The Wizard of DATz");
            WriteOutWithPauses(credits);
        }

        /// <summary>
        /// Output a single feature recursively
        /// </summary>
        /// <param name="featurename">Name of the feature to output information for, if possible</param>
        /// <param name="includeLongDescription">True if the long description should be formatted and output, false otherwise</param>
        public void OutputIndividualFeature(string featurename, bool includeLongDescription = false)
        {
            // Start building the output list
            List<string> output = new List<string>();

            // Now try to find the feature that has the name included
            string realname = null;
            List<string> startsWith = new List<string>();
            foreach (string feature in _features.Keys)
            {
                // If we have a match to the feature name somehow
                if (feature == featurename)
                {
                    realname = feature;
                    break;
                }

                // If we have a match within the flags
                else if (_features[feature].ContainsFlag(featurename))
                {
                    realname = feature;
                    break;
                }

                // Otherwise, we want to get features with the same start
                else if (_features[feature].StartsWith(featurename.TrimStart('-')[0]))
                {
                    startsWith.Add(feature);
                }
            }

            // If we have a real name found, append all available subflags recursively
            if (realname != null)
            {
                output.Add($"Available options for {realname}:");
                output.AddRange(_features[realname].OutputRecursive(0, pre: 2, midpoint: 30, includeLongDescription: includeLongDescription));
            }

            // If no name was found but we have possible matches, show them
            else if (startsWith.Count > 0)
            {
                output.Add($"\"{featurename}\" not found. Did you mean:");
                foreach (string possible in startsWith)
                {
                    output.AddRange(_features[possible].Output(pre: 2, midpoint: 30, includeLongDescription: includeLongDescription));
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
            return _features.Keys.Any(f => _features[f].ValidateInput(flag, exact: true));
        }

        /// <summary>
        /// Retrieve a list of enabled features
        /// </summary>
        /// <returns>List of Features representing what is enabled</returns>
        public Dictionary<string, Feature> GetEnabledFeatures()
        {
            Dictionary<string, Feature> enabled = new Dictionary<string, Feature>();

            // Loop through the features
            foreach (KeyValuePair<string, Feature> feature in _features)
            {
                Dictionary<string, Feature> temp = GetEnabledSubfeatures(feature.Key, feature.Value);
                foreach (KeyValuePair<string, Feature> tempfeat in temp)
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
        private Dictionary<string, Feature> GetEnabledSubfeatures(string key, Feature feature)
        {
            Dictionary<string, Feature> enabled = new Dictionary<string, Feature>();

            // First determine if the current feature is enabled
            if (feature.IsEnabled())
                enabled.Add(key, feature);

            // Now loop through the subfeatures recursively
            foreach (KeyValuePair<string, Feature> sub in feature.Features)
            {
                Dictionary<string, Feature> temp = GetEnabledSubfeatures(sub.Key, sub.Value);
                foreach (KeyValuePair<string, Feature> tempfeat in temp)
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
        /// <param name="helptext"></param>
        private void WriteOutWithPauses(List<string> helptext)
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
            if (!Console.IsOutputRedirected)
            {
                Console.WriteLine();
                Console.WriteLine("Press enter to continue...");
                Console.ReadLine();
            }
        }

        #endregion
    }
}
