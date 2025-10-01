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
                if (ValidateInput(args, ref i))
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

        #region Generic Extraction

        /// <summary>
        /// Get boolean value from nullable feature
        /// </summary>
        protected static bool GetBoolean(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return false;

            if (features[key] is BooleanInput b)
                return b.Value;
            else if (features[key] is FlagInput f)
                return f.Value;

            throw new ArgumentException("Feature is not a bool");
        }

        /// <summary>
        /// Get sbyte value from nullable feature
        /// </summary>
        protected static sbyte GetInt8(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return sbyte.MinValue;

            if (features[key] is not Int8Input i)
                throw new ArgumentException("Feature is not an sbyte");

            return i.Value;
        }

        /// <summary>
        /// Get short value from nullable feature
        /// </summary>
        protected static short GetInt16(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return short.MinValue;

            if (features[key] is not Int16Input i)
                throw new ArgumentException("Feature is not a short");

            return i.Value;
        }

        /// <summary>
        /// Get int value from nullable feature
        /// </summary>
        protected static int GetInt32(Dictionary<string, UserInput> features, string key)
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
        protected static long GetInt64(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return long.MinValue;

            if (features[key] is not Int64Input l)
                throw new ArgumentException("Feature is not a long");

            return l.Value;
        }

        /// <summary>
        /// Get string value from nullable feature
        /// </summary>
        protected static string? GetString(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return null;

            if (features[key] is not StringInput s)
                throw new ArgumentException("Feature is not a string");

            return s.Value;
        }

        /// <summary>
        /// Get list value from nullable feature
        /// </summary>
        protected static List<string> GetStringList(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return [];

            if (features[key] is not StringListInput l)
                throw new ArgumentException("Feature is not a list");

            return l.Value ?? [];
        }

        /// <summary>
        /// Get byte value from nullable feature
        /// </summary>
        protected static byte GetUInt8(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return byte.MinValue;

            if (features[key] is not UInt8Input i)
                throw new ArgumentException("Feature is not an byte");

            return i.Value;
        }

        /// <summary>
        /// Get short value from nullable feature
        /// </summary>
        protected static ushort GetUInt16(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return ushort.MinValue;

            if (features[key] is not UInt16Input i)
                throw new ArgumentException("Feature is not a ushort");

            return i.Value;
        }

        /// <summary>
        /// Get int value from nullable feature
        /// </summary>
        protected static uint GetUInt32(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return uint.MinValue;

            if (features[key] is not UInt32Input i)
                throw new ArgumentException("Feature is not an uint");

            return i.Value;
        }

        /// <summary>
        /// Get long value from nullable feature
        /// </summary>
        protected static ulong GetUInt64(Dictionary<string, UserInput> features, string key)
        {
            if (!features.ContainsKey(key))
                return ulong.MinValue;

            if (features[key] is not UInt64Input l)
                throw new ArgumentException("Feature is not a ulong");

            return l.Value;
        }

        #endregion
    }
}
