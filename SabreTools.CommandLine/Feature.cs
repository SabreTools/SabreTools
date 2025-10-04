using System.Collections.Generic;
using SabreTools.CommandLine.Inputs;

namespace SabreTools.CommandLine
{
    /// <summary>
    /// Represents an application-level feature
    /// </summary>
    public abstract class Feature : FlagInput
    {
        #region Fields

        /// <summary>
        /// List of undefined inputs
        /// </summary>
        public readonly List<string> Inputs = [];

        /// <summary>
        /// Indicates if the feature requires inputs to be set
        /// </summary>
        public bool RequiresInputs { get; protected set; } = false;

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
        /// <param name="index">Starting index into the arguments</param>
        /// <returns>True if all arguments were processed correctly, false otherwise</returns>
        public virtual bool ProcessArgs(string[] args, int index)
        {
            // Empty arguments is always successful
            if (args.Length == 0)
                return true;

            // Invalid index values are not processed
            if (index < 0 || index >= args.Length)
                return false;

            for (int i = index; i < args.Length; i++)
            {
                // Verify that the current flag is proper for the feature
                if (ProcessInput(args, ref i))
                    continue;

                // Add all other arguments to the generic inputs
                Inputs.Add(item: args[i]);
            }

            return true;
        }

        /// <summary>
        /// Verify all inputs based on the feature requirements
        /// </summary>
        /// <returns>True if the inputs verified correctly, false otherwise</returns>
        public abstract bool VerifyInputs();

        /// <summary>
        /// Execute the logic associated with the feature
        /// </summary>
        /// <returns>True if execution was successful, false otherwise</returns>
        public abstract bool Execute();

        #endregion
    }
}
