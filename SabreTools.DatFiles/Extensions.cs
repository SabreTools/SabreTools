using System.Collections.Generic;
using SabreTools.Core.Tools;

namespace SabreTools.DatFiles
{
    public static class Extensions
    {
        #region Private Maps

        /// <summary>
        /// Set of enum to string mappings for MergingFlag
        /// </summary>
        private static readonly Dictionary<string, MergingFlag> _toMergingFlagMap = Converters.GenerateToEnum<MergingFlag>();

        /// <summary>
        /// Set of string to enum mappings for MergingFlag
        /// </summary>
        private static readonly Dictionary<MergingFlag, string> _fromMergingFlagMap = Converters.GenerateToString<MergingFlag>(useSecond: false);

        /// <summary>
        /// Set of string to enum mappings for MergingFlag (secondary)
        /// </summary>
        private static readonly Dictionary<MergingFlag, string> _fromMergingFlagSecondaryMap = Converters.GenerateToString<MergingFlag>(useSecond: true);

        /// <summary>
        /// Set of enum to string mappings for NodumpFlag
        /// </summary>
        private static readonly Dictionary<string, NodumpFlag> _toNodumpFlagMap = Converters.GenerateToEnum<NodumpFlag>();

        /// <summary>
        /// Set of string to enum mappings for NodumpFlag
        /// </summary>
        private static readonly Dictionary<NodumpFlag, string> _fromNodumpFlagMap = Converters.GenerateToString<NodumpFlag>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for PackingFlag
        /// </summary>
        private static readonly Dictionary<string, PackingFlag> _toPackingFlagMap = Converters.GenerateToEnum<PackingFlag>();

        /// <summary>
        /// Set of string to enum mappings for PackingFlag
        /// </summary>
        private static readonly Dictionary<PackingFlag, string> _fromPackingFlagMap = Converters.GenerateToString<PackingFlag>(useSecond: false);

        /// <summary>
        /// Set of string to enum mappings for PackingFlag (secondary)
        /// </summary>
        private static readonly Dictionary<PackingFlag, string> _fromPackingFlagSecondaryMap = Converters.GenerateToString<PackingFlag>(useSecond: true);

        #endregion

        #region String to Enum

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static MergingFlag AsMergingFlag(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value is null)
                return default;

            // Try to get the value from the mappings
            if (_toMergingFlagMap.ContainsKey(value))
                return _toMergingFlagMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static NodumpFlag AsNodumpFlag(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value is null)
                return default;

            // Try to get the value from the mappings
            if (_toNodumpFlagMap.ContainsKey(value))
                return _toNodumpFlagMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static PackingFlag AsPackingFlag(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value is null)
                return default;

            // Try to get the value from the mappings
            if (_toPackingFlagMap.ContainsKey(value))
                return _toPackingFlagMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        #endregion

        #region Enum to String

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this MergingFlag value, bool useSecond = false)
        {
            // Try to get the value from the mappings
            if (!useSecond && _fromMergingFlagMap.ContainsKey(value))
                return _fromMergingFlagMap[value];
            else if (useSecond && _fromMergingFlagSecondaryMap.ContainsKey(value))
                return _fromMergingFlagSecondaryMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this NodumpFlag value)
        {
            // Try to get the value from the mappings
            if (_fromNodumpFlagMap.ContainsKey(value))
                return _fromNodumpFlagMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this PackingFlag value, bool useSecond = false)
        {
            // Try to get the value from the mappings
            if (!useSecond && _fromPackingFlagMap.ContainsKey(value))
                return _fromPackingFlagMap[value];
            else if (useSecond && _fromPackingFlagSecondaryMap.ContainsKey(value))
                return _fromPackingFlagSecondaryMap[value];

            // Otherwise, return null
            return null;
        }

        #endregion

        #region Splitting

        /// <summary>
        /// Split a format flag into multiple distinct values
        /// </summary>
        /// <param name="datFormat">Combined DatFormat value to split</param>
        /// <returns>List representing the individual flag values set</returns>
        /// TODO: Consider making DatFormat a non-flag enum so this doesn't need to happen
        public static List<DatFormat> SplitFormats(this DatFormat datFormat)
        {
            List<DatFormat> splitFormats = [];

#if NET20 || NET35
            if ((datFormat & DatFormat.ArchiveDotOrg) != 0)
                splitFormats.Add(DatFormat.ArchiveDotOrg);
            if ((datFormat & DatFormat.AttractMode) != 0)
                splitFormats.Add(DatFormat.AttractMode);
            if ((datFormat & DatFormat.ClrMamePro) != 0)
                splitFormats.Add(DatFormat.ClrMamePro);
            if ((datFormat & DatFormat.CSV) != 0)
                splitFormats.Add(DatFormat.CSV);
            if ((datFormat & DatFormat.DOSCenter) != 0)
                splitFormats.Add(DatFormat.DOSCenter);
            if ((datFormat & DatFormat.EverdriveSMDB) != 0)
                splitFormats.Add(DatFormat.EverdriveSMDB);
            if ((datFormat & DatFormat.Listrom) != 0)
                splitFormats.Add(DatFormat.Listrom);
            if ((datFormat & DatFormat.Listxml) != 0)
                splitFormats.Add(DatFormat.Listxml);
            if ((datFormat & DatFormat.Logiqx) != 0)
                splitFormats.Add(DatFormat.Logiqx);
            if ((datFormat & DatFormat.LogiqxDeprecated) != 0)
                splitFormats.Add(DatFormat.LogiqxDeprecated);
            if ((datFormat & DatFormat.MissFile) != 0)
                splitFormats.Add(DatFormat.MissFile);
            if ((datFormat & DatFormat.OfflineList) != 0)
                splitFormats.Add(DatFormat.OfflineList);
            if ((datFormat & DatFormat.OpenMSX) != 0)
                splitFormats.Add(DatFormat.OpenMSX);
            if ((datFormat & DatFormat.RedumpMD2) != 0)
                splitFormats.Add(DatFormat.RedumpMD2);
            if ((datFormat & DatFormat.RedumpMD4) != 0)
                splitFormats.Add(DatFormat.RedumpMD4);
            if ((datFormat & DatFormat.RedumpMD5) != 0)
                splitFormats.Add(DatFormat.RedumpMD5);
            if ((datFormat & DatFormat.RedumpRIPEMD128) != 0)
                splitFormats.Add(DatFormat.RedumpRIPEMD128);
            if ((datFormat & DatFormat.RedumpRIPEMD160) != 0)
                splitFormats.Add(DatFormat.RedumpRIPEMD160);
            if ((datFormat & DatFormat.RedumpSFV) != 0)
                splitFormats.Add(DatFormat.RedumpSFV);
            if ((datFormat & DatFormat.RedumpSHA1) != 0)
                splitFormats.Add(DatFormat.RedumpSHA1);
            if ((datFormat & DatFormat.RedumpSHA256) != 0)
                splitFormats.Add(DatFormat.RedumpSHA256);
            if ((datFormat & DatFormat.RedumpSHA384) != 0)
                splitFormats.Add(DatFormat.RedumpSHA384);
            if ((datFormat & DatFormat.RedumpSHA512) != 0)
                splitFormats.Add(DatFormat.RedumpSHA512);
            if ((datFormat & DatFormat.RedumpSpamSum) != 0)
                splitFormats.Add(DatFormat.RedumpSpamSum);
            if ((datFormat & DatFormat.RomCenter) != 0)
                splitFormats.Add(DatFormat.RomCenter);
            if ((datFormat & DatFormat.SabreJSON) != 0)
                splitFormats.Add(DatFormat.SabreJSON);
            if ((datFormat & DatFormat.SabreXML) != 0)
                splitFormats.Add(DatFormat.SabreXML);
            if ((datFormat & DatFormat.SoftwareList) != 0)
                splitFormats.Add(DatFormat.SoftwareList);
            if ((datFormat & DatFormat.SSV) != 0)
                splitFormats.Add(DatFormat.SSV);
            if ((datFormat & DatFormat.TSV) != 0)
                splitFormats.Add(DatFormat.TSV);
#else
            if (datFormat.HasFlag(DatFormat.ArchiveDotOrg))
                splitFormats.Add(DatFormat.ArchiveDotOrg);
            if (datFormat.HasFlag(DatFormat.AttractMode))
                splitFormats.Add(DatFormat.AttractMode);
            if (datFormat.HasFlag(DatFormat.ClrMamePro))
                splitFormats.Add(DatFormat.ClrMamePro);
            if (datFormat.HasFlag(DatFormat.CSV))
                splitFormats.Add(DatFormat.CSV);
            if (datFormat.HasFlag(DatFormat.DOSCenter))
                splitFormats.Add(DatFormat.DOSCenter);
            if (datFormat.HasFlag(DatFormat.EverdriveSMDB))
                splitFormats.Add(DatFormat.EverdriveSMDB);
            if (datFormat.HasFlag(DatFormat.Listrom))
                splitFormats.Add(DatFormat.Listrom);
            if (datFormat.HasFlag(DatFormat.Listxml))
                splitFormats.Add(DatFormat.Listxml);
            if (datFormat.HasFlag(DatFormat.Logiqx))
                splitFormats.Add(DatFormat.Logiqx);
            if (datFormat.HasFlag(DatFormat.LogiqxDeprecated))
                splitFormats.Add(DatFormat.LogiqxDeprecated);
            if (datFormat.HasFlag(DatFormat.MissFile))
                splitFormats.Add(DatFormat.MissFile);
            if (datFormat.HasFlag(DatFormat.OfflineList))
                splitFormats.Add(DatFormat.OfflineList);
            if (datFormat.HasFlag(DatFormat.OpenMSX))
                splitFormats.Add(DatFormat.OpenMSX);
            if (datFormat.HasFlag(DatFormat.RedumpMD2))
                splitFormats.Add(DatFormat.RedumpMD2);
            if (datFormat.HasFlag(DatFormat.RedumpMD4))
                splitFormats.Add(DatFormat.RedumpMD4);
            if (datFormat.HasFlag(DatFormat.RedumpMD5))
                splitFormats.Add(DatFormat.RedumpMD5);
            if (datFormat.HasFlag(DatFormat.RedumpRIPEMD128))
                splitFormats.Add(DatFormat.RedumpRIPEMD128);
            if (datFormat.HasFlag(DatFormat.RedumpRIPEMD160))
                splitFormats.Add(DatFormat.RedumpRIPEMD160);
            if (datFormat.HasFlag(DatFormat.RedumpSFV))
                splitFormats.Add(DatFormat.RedumpSFV);
            if (datFormat.HasFlag(DatFormat.RedumpSHA1))
                splitFormats.Add(DatFormat.RedumpSHA1);
            if (datFormat.HasFlag(DatFormat.RedumpSHA256))
                splitFormats.Add(DatFormat.RedumpSHA256);
            if (datFormat.HasFlag(DatFormat.RedumpSHA384))
                splitFormats.Add(DatFormat.RedumpSHA384);
            if (datFormat.HasFlag(DatFormat.RedumpSHA512))
                splitFormats.Add(DatFormat.RedumpSHA512);
            if (datFormat.HasFlag(DatFormat.RedumpSpamSum))
                splitFormats.Add(DatFormat.RedumpSpamSum);
            if (datFormat.HasFlag(DatFormat.RomCenter))
                splitFormats.Add(DatFormat.RomCenter);
            if (datFormat.HasFlag(DatFormat.SabreJSON))
                splitFormats.Add(DatFormat.SabreJSON);
            if (datFormat.HasFlag(DatFormat.SabreXML))
                splitFormats.Add(DatFormat.SabreXML);
            if (datFormat.HasFlag(DatFormat.SoftwareList))
                splitFormats.Add(DatFormat.SoftwareList);
            if (datFormat.HasFlag(DatFormat.SSV))
                splitFormats.Add(DatFormat.SSV);
            if (datFormat.HasFlag(DatFormat.TSV))
                splitFormats.Add(DatFormat.TSV);
#endif

            return splitFormats;
        }

        #endregion
    }
}
