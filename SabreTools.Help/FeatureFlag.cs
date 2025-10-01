using System;
using System.Collections.Generic;
using System.IO;

namespace SabreTools.Help
{
    /// <summary>
    /// Represents an application-level feature
    /// </summary>
    public abstract class FeatureFlag : FlagInput
    {
        #region Fields

        /// <summary>
        /// List of files, directories, and potential wildcard paths
        /// </summary>
        public readonly List<string> Inputs = [];

        #endregion

        #region Constructors

        public FeatureFlag(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
        }

        public FeatureFlag(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
        }

        #endregion

        #region Processing

        /// <summary>
        /// Process args list based on current feature
        /// </summary>
        public virtual bool ProcessArgs(string[] args, FeatureSet help)
        {
            for (int i = 1; i < args.Length; i++)
            {
                // Verify that the current flag is proper for the feature
                if (ValidateInput(args[i]))
                    continue;

                // Special precautions for files and directories
                if (File.Exists(args[i]) || Directory.Exists(args[i]))
                {
                    Inputs.Add(item: args[i]);
                }

                // Special precautions for wildcarded inputs (potential paths)
#if NETFRAMEWORK || NETSTANDARD
                else if (args[i].Contains("*") || args[i].Contains("?"))
#else
                else if (args[i].Contains('*') || args[i].Contains('?'))
#endif
                {
                    Inputs.Add(args[i]);
                }

                // Everything else isn't a file
                else
                {
                    Console.Error.WriteLine($"Invalid input detected: {args[i]}");
                    help.OutputIndividualFeature(Name);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Process and extract variables based on current feature
        /// </summary>
        /// <returns>True if execution was successful, false otherwise</returns>
        public virtual bool ProcessFeatures(Dictionary<string, UserInput?> features) => true;

        #endregion

        #region Generic Extraction

        /// <summary>
        /// Get boolean value from nullable feature
        /// </summary>
        protected static bool GetBoolean(Dictionary<string, UserInput?> features, string key)
        {
            if (!features.ContainsKey(key))
                return false;

            return true;
        }

        /// <summary>
        /// Get int value from nullable feature
        /// </summary>
        protected static int GetInt32(Dictionary<string, UserInput?> features, string key)
        {
            if (!features.ContainsKey(key))
                return int.MinValue;

            if (features[key] is not Int32Input i)
                throw new ArgumentException("Feature is not an int");

            return i.Value;
        }

        /// <summary>
        /// Get long value from nullable feature
        /// </summary>
        protected static long GetInt64(Dictionary<string, UserInput?> features, string key)
        {
            if (!features.ContainsKey(key))
                return long.MinValue;

            if (features[key] is not Int64Input l)
                throw new ArgumentException("Feature is not a long");

            return l.Value;
        }

        /// <summary>
        /// Get list value from nullable feature
        /// </summary>
        protected static List<string> GetList(Dictionary<string, UserInput?> features, string key)
        {
            if (!features.ContainsKey(key))
                return [];

            if (features[key] is not StringListInput l)
                throw new ArgumentException("Feature is not a list");

            return l.Value ?? [];
        }

        /// <summary>
        /// Get string value from nullable feature
        /// </summary>
        protected static string? GetString(Dictionary<string, UserInput?> features, string key)
        {
            if (!features.ContainsKey(key))
                return null;

            if (features[key] is not StringInput s)
                throw new ArgumentException("Feature is not a string");

            return s.Value;
        }

        #endregion
    }
}
