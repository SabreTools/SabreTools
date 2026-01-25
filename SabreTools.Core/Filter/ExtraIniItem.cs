using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.IO.Logging;
using SabreTools.IO.Readers;

namespace SabreTools.Core.Filter
{
    public class ExtraIniItem
    {
        #region Fields

        /// <summary>
        /// Item type and field to update with INI information
        /// </summary>
        public readonly FilterKey Key;

        /// <summary>
        /// Mappings from machine names to value
        /// </summary>
        public readonly Dictionary<string, string> Mappings = [];

        #endregion

        #region Constructors

        public ExtraIniItem(string key, string iniPath)
        {
            Key = new FilterKey(key);
            if (!PopulateFromFile(iniPath))
                Mappings.Clear();
        }

        public ExtraIniItem(string itemName, string fieldName, string iniPath)
        {
            Key = new FilterKey(itemName, fieldName);
            if (!PopulateFromFile(iniPath))
                Mappings.Clear();
        }

        #endregion

        #region Extras Population

        /// <summary>
        /// Populate the dictionary from an INI file
        /// </summary>
        /// <param name="iniPath">Path to INI file to populate from</param>
        /// <remarks>
        /// The INI file format that is supported here is not exactly the same
        /// as a traditional one. This expects a MAME extras format, which usually
        /// doesn't contain key value pairs and always at least contains one section
        /// called `ROOT_FOLDER`. If that's the name of a section, then we assume
        /// the value is boolean. If there's another section name, then that is set
        /// as the value instead.
        /// </remarks>
        private bool PopulateFromFile(string iniPath)
        {
            // Validate the path
            if (iniPath.Length == 0)
                return false;
            else if (!File.Exists(iniPath))
                return false;

            // Prepare all internal variables
            var ir = new IniReader(iniPath) { ValidateRows = false };
            bool foundRootFolder = false;

            // If we got a null reader, just return
            if (ir is null)
                return false;

            // Otherwise, read the file to the end
            try
            {
                while (!ir.EndOfStream)
                {
                    // Read in the next line and process
                    ir.ReadNextLine();

                    // We don't care about whitespace or comments
                    if (ir.RowType == IniRowType.None || ir.RowType == IniRowType.Comment)
                        continue;

                    // If we have a section, just read it in
                    if (ir.RowType == IniRowType.SectionHeader)
                    {
                        // If we've found the start of the extras, set the flag
                        if (string.Equals(ir.Section, "ROOT_FOLDER", StringComparison.OrdinalIgnoreCase))
                            foundRootFolder = true;

                        continue;
                    }

                    // If we have a value, then we start populating the dictionary
                    else if (foundRootFolder)
                    {
                        // Get the value and machine name
                        string? value = ir.Section;
                        string? machineName = ir.CurrentLine?.Trim();

                        // If the section is "ROOT_FOLDER", then we use the value "true" instead.
                        // This is done because some INI files use the name of the file as the
                        // category to be assigned to the items included.
                        if (value == "ROOT_FOLDER")
                            value = "true";

                        // Add the new mapping
                        if (machineName is not null && value is not null)
                            Mappings[machineName] = value;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerImpl.Warning(ex, $"Exception found while parsing '{iniPath}'");
                return false;
            }

            ir.Dispose();
            return true;
        }

        #endregion
    }
}
