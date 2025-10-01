using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Help.Inputs;

namespace SabreTools.Help
{
    /// <summary>
    /// Represents an application-level feature
    /// </summary>
    public abstract class Feature : FlagInput
    {
        #region Fields

        /// <summary>
        /// List of files, directories, and potential wildcard paths
        /// </summary>
        public readonly List<string> Inputs = [];

        #endregion

        #region Constructors

        public Feature(string name, string flag, string description, string? longDescription = null)
            : base(name, flag, description, longDescription)
        {
        }

        public Feature(string name, string[] flags, string description, string? longDescription = null)
            : base(name, flags, description, longDescription)
        {
        }

        #endregion

        #region Processing

        /// <summary>
        /// Process args list based on current feature
        /// </summary>
        /// <param name="args">Set of arguments to process</param>
        /// <param name="parentSet">Reference to the enclosing parent set</param>
        /// <returns>True if all arguments were processed correctly, false otherwise</returns>
        /// <remarks>
        /// This assumes that the argument representing this feature was
        /// the first in the set of passed in arguments.
        /// </remarks>
        public virtual bool ProcessArgs(string[] args, FeatureSet parentSet)
        {
            for (int i = 1; i < args.Length; i++)
            {
                // Verify that the current flag is proper for the feature
                if (ProcessInput(args, ref i))
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
                    parentSet.OutputIndividualFeature(Name);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Process and extract variables based on current feature
        /// </summary>
        /// <returns>True if execution was successful, false otherwise</returns>
        public abstract bool ProcessFeatures(Dictionary<string, UserInput> features);

        #endregion
    }
}
