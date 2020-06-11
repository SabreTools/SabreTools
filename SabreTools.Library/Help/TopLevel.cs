using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Help
{
    /// <summary>
    /// Represents an actionable top-level feature
    /// </summary>
    public abstract class TopLevel : Feature
    {
        public List<string> Inputs = new List<string>();

        /// <summary>
        /// Process args list based on current feature
        /// </summary>
        public virtual bool ProcessArgs(string[] args, Help help)
        {
            for (int i = 1; i < args.Length; i++)
            {
                // Verify that the current flag is proper for the feature
                if (!ValidateInput(args[i]))
                {
                    Globals.Logger.Error($"Invalid input detected: {args[i]}");
                    help.OutputIndividualFeature(this.Name);
                    Globals.Logger.Close();
                    return false;
                }

                // Special precautions for files and directories
                if (File.Exists(args[i]) || Directory.Exists(args[i]))
                    Inputs.Add(args[i]);
            }

            return true;
        }

        /// <summary>
        /// Process and extract variables based on current feature
        /// </summary>
        public virtual void ProcessFeatures(Dictionary<string, Feature> features) { }

        #region Generic Extraction

        /// <summary>
        /// Get boolean value from nullable feature
        /// </summary>
        protected bool GetBoolean(Dictionary<string, Feature> features, string key)
        {
            if (!features.ContainsKey(key))
                return false;

            return true;
        }

        /// <summary>
        /// Get int value from nullable feature
        /// </summary>
        protected int GetInt32(Dictionary<string, Feature> features, string key)
        {
            if (!features.ContainsKey(key))
                return Int32.MinValue;

            return features[key].GetInt32Value();
        }

        /// <summary>
        /// Get long value from nullable feature
        /// </summary>
        protected long GetInt64(Dictionary<string, Feature> features, string key)
        {
            if (!features.ContainsKey(key))
                return Int64.MinValue;

            return features[key].GetInt64Value();
        }

        /// <summary>
        /// Get list value from nullable feature
        /// </summary>
        protected List<string> GetList(Dictionary<string, Feature> features, string key)
        {
            if (!features.ContainsKey(key))
                return new List<string>();

            return features[key].GetListValue() ?? new List<string>();
        }

        /// <summary>
        /// Get string value from nullable feature
        /// </summary>
        protected string GetString(Dictionary<string, Feature> features, string key)
        {
            if (!features.ContainsKey(key))
                return null;

            return features[key].GetStringValue();
        }

        #endregion
    }
}
