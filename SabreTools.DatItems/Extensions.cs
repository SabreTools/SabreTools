using System.Collections.Generic;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems
{
    public static class Extensions
    {
        #region Private Maps

        /// <summary>
        /// Set of enum to string mappings for ChipType
        /// </summary>
        private static readonly Dictionary<string, ChipType> _toChipTypeMap = Converters.GenerateToEnum<ChipType>();

        /// <summary>
        /// Set of string to enum mappings for ChipType
        /// </summary>
        private static readonly Dictionary<ChipType, string> _fromChipTypeMap = Converters.GenerateToString<ChipType>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for ControlType
        /// </summary>
        private static readonly Dictionary<string, ControlType> _toControlTypeMap = Converters.GenerateToEnum<ControlType>();

        /// <summary>
        /// Set of string to enum mappings for ControlType
        /// </summary>
        private static readonly Dictionary<ControlType, string> _fromControlTypeMap = Converters.GenerateToString<ControlType>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for DeviceType
        /// </summary>
        private static readonly Dictionary<string, DeviceType> _toDeviceTypeMap = Converters.GenerateToEnum<DeviceType>();

        /// <summary>
        /// Set of string to enum mappings for DeviceType
        /// </summary>
        private static readonly Dictionary<DeviceType, string> _fromDeviceTypeMap = Converters.GenerateToString<DeviceType>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for DisplayType
        /// </summary>
        private static readonly Dictionary<string, DisplayType> _toDisplayTypeMap = Converters.GenerateToEnum<DisplayType>();

        /// <summary>
        /// Set of string to enum mappings for DisplayType
        /// </summary>
        private static readonly Dictionary<DisplayType, string> _fromDisplayTypeMap = Converters.GenerateToString<DisplayType>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for Endianness
        /// </summary>
        private static readonly Dictionary<string, Endianness> _toEndiannessMap = Converters.GenerateToEnum<Endianness>();

        /// <summary>
        /// Set of string to enum mappings for Endianness
        /// </summary>
        private static readonly Dictionary<Endianness, string> _fromEndiannessMap = Converters.GenerateToString<Endianness>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for FeatureStatus
        /// </summary>
        private static readonly Dictionary<string, FeatureStatus> _toFeatureStatusMap = Converters.GenerateToEnum<FeatureStatus>();

        /// <summary>
        /// Set of string to enum mappings for FeatureStatus
        /// </summary>
        private static readonly Dictionary<FeatureStatus, string> _fromFeatureStatusMap = Converters.GenerateToString<FeatureStatus>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for FeatureType
        /// </summary>
        private static readonly Dictionary<string, FeatureType> _toFeatureTypeMap = Converters.GenerateToEnum<FeatureType>();

        /// <summary>
        /// Set of string to enum mappings for FeatureType
        /// </summary>
        private static readonly Dictionary<FeatureType, string> _fromFeatureTypeMap = Converters.GenerateToString<FeatureType>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for ItemStatus
        /// </summary>
        private static readonly Dictionary<string, ItemStatus> _toItemStatusMap = Converters.GenerateToEnum<ItemStatus>();

        /// <summary>
        /// Set of string to enum mappings for ItemStatus
        /// </summary>
        private static readonly Dictionary<ItemStatus, string> _fromItemStatusMap = Converters.GenerateToString<ItemStatus>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for ItemType
        /// </summary>
        private static readonly Dictionary<string, ItemType> _toItemTypeMap = Converters.GenerateToEnum<ItemType>();

        /// <summary>
        /// Set of string to enum mappings for ItemType
        /// </summary>
        private static readonly Dictionary<ItemType, string> _fromItemTypeMap = Converters.GenerateToString<ItemType>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for LoadFlag
        /// </summary>
        private static readonly Dictionary<string, LoadFlag> _toLoadFlagMap = Converters.GenerateToEnum<LoadFlag>();

        /// <summary>
        /// Set of string to enum mappings for LoadFlag
        /// </summary>
        private static readonly Dictionary<LoadFlag, string> _fromLoadFlagMap = Converters.GenerateToString<LoadFlag>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for MachineType
        /// </summary>
        private static readonly Dictionary<string, MachineType> _toMachineTypeMap = Converters.GenerateToEnum<MachineType>();

        /// <summary>
        /// Set of enum to string mappings for OpenMSXSubType
        /// </summary>
        private static readonly Dictionary<string, OpenMSXSubType> _toOpenMSXSubTypeMap = Converters.GenerateToEnum<OpenMSXSubType>();

        /// <summary>
        /// Set of string to enum mappings for OpenMSXSubType
        /// </summary>
        private static readonly Dictionary<OpenMSXSubType, string> _fromOpenMSXSubTypeMap = Converters.GenerateToString<OpenMSXSubType>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for Relation
        /// </summary>
        private static readonly Dictionary<string, Relation> _toRelationMap = Converters.GenerateToEnum<Relation>();

        /// <summary>
        /// Set of string to enum mappings for Relation
        /// </summary>
        private static readonly Dictionary<Relation, string> _fromRelationMap = Converters.GenerateToString<Relation>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for Runnable
        /// </summary>
        private static readonly Dictionary<string, Runnable> _toRunnableMap = Converters.GenerateToEnum<Runnable>();

        /// <summary>
        /// Set of string to enum mappings for Runnable
        /// </summary>
        private static readonly Dictionary<Runnable, string> _fromRunnableMap = Converters.GenerateToString<Runnable>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for SoftwareListStatus
        /// </summary>
        private static readonly Dictionary<string, SoftwareListStatus> _toSoftwareListStatusMap = Converters.GenerateToEnum<SoftwareListStatus>();

        /// <summary>
        /// Set of string to enum mappings for SoftwareListStatus
        /// </summary>
        private static readonly Dictionary<SoftwareListStatus, string> _fromSoftwareListStatusMap = Converters.GenerateToString<SoftwareListStatus>(useSecond: false);

        /// <summary>
        /// Set of enum to string mappings for Supported
        /// </summary>
        private static readonly Dictionary<string, Supported> _toSupportedMap = Converters.GenerateToEnum<Supported>();

        /// <summary>
        /// Set of string to enum mappings for Supported
        /// </summary>
        private static readonly Dictionary<Supported, string> _fromSupportedMap = Converters.GenerateToString<Supported>(useSecond: false);

        /// <summary>
        /// Set of string to enum mappings for Supported (secondary)
        /// </summary>
        private static readonly Dictionary<Supported, string> _fromSupportedSecondaryMap = Converters.GenerateToString<Supported>(useSecond: true);

        /// <summary>
        /// Set of enum to string mappings for SupportStatus
        /// </summary>
        private static readonly Dictionary<string, SupportStatus> _toSupportStatusMap = Converters.GenerateToEnum<SupportStatus>();

        /// <summary>
        /// Set of string to enum mappings for SupportStatus
        /// </summary>
        private static readonly Dictionary<SupportStatus, string> _fromSupportStatusMap = Converters.GenerateToString<SupportStatus>(useSecond: false);

        #endregion

        #region String to Enum

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static ChipType AsChipType(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toChipTypeMap.ContainsKey(value))
                return _toChipTypeMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static ControlType AsControlType(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toControlTypeMap.ContainsKey(value))
                return _toControlTypeMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static DeviceType AsDeviceType(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toDeviceTypeMap.ContainsKey(value))
                return _toDeviceTypeMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static DisplayType AsDisplayType(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toDisplayTypeMap.ContainsKey(value))
                return _toDisplayTypeMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static Endianness AsEndianness(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toEndiannessMap.ContainsKey(value))
                return _toEndiannessMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static FeatureStatus AsFeatureStatus(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toFeatureStatusMap.ContainsKey(value))
                return _toFeatureStatusMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static FeatureType AsFeatureType(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toFeatureTypeMap.ContainsKey(value))
                return _toFeatureTypeMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static ItemStatus AsItemStatus(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toItemStatusMap.ContainsKey(value))
                return _toItemStatusMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static ItemType AsItemType(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toItemTypeMap.ContainsKey(value))
                return _toItemTypeMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static LoadFlag AsLoadFlag(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toLoadFlagMap.ContainsKey(value))
                return _toLoadFlagMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static MachineType AsMachineType(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toMachineTypeMap.ContainsKey(value))
                return _toMachineTypeMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static OpenMSXSubType AsOpenMSXSubType(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toOpenMSXSubTypeMap.ContainsKey(value))
                return _toOpenMSXSubTypeMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static Relation AsRelation(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toRelationMap.ContainsKey(value))
                return _toRelationMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static Runnable AsRunnable(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toRunnableMap.ContainsKey(value))
                return _toRunnableMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static SoftwareListStatus AsSoftwareListStatus(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toSoftwareListStatusMap.ContainsKey(value))
                return _toSoftwareListStatusMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static Supported AsSupported(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toSupportedMap.ContainsKey(value))
                return _toSupportedMap[value];

            // Otherwise, return the default value for the enum
            return default;
        }

        /// <summary>
        /// Get the enum value for an input string, if possible
        /// </summary>
        /// <param name="value">String value to parse/param>
        /// <returns>Enum value representing the input, default on error</returns>
        public static SupportStatus AsSupportStatus(this string? value)
        {
            // Normalize the input value
            value = value?.ToLowerInvariant();
            if (value == null)
                return default;

            // Try to get the value from the mappings
            if (_toSupportStatusMap.ContainsKey(value))
                return _toSupportStatusMap[value];

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
        public static string? AsStringValue(this ChipType value)
        {
            // Try to get the value from the mappings
            if (_fromChipTypeMap.ContainsKey(value))
                return _fromChipTypeMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this ControlType value)
        {
            // Try to get the value from the mappings
            if (_fromControlTypeMap.ContainsKey(value))
                return _fromControlTypeMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this DeviceType value)
        {
            // Try to get the value from the mappings
            if (_fromDeviceTypeMap.ContainsKey(value))
                return _fromDeviceTypeMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this DisplayType value)
        {
            // Try to get the value from the mappings
            if (_fromDisplayTypeMap.ContainsKey(value))
                return _fromDisplayTypeMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this Endianness value)
        {
            // Try to get the value from the mappings
            if (_fromEndiannessMap.ContainsKey(value))
                return _fromEndiannessMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this FeatureStatus value)
        {
            // Try to get the value from the mappings
            if (_fromFeatureStatusMap.ContainsKey(value))
                return _fromFeatureStatusMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this FeatureType value)
        {
            // Try to get the value from the mappings
            if (_fromFeatureTypeMap.ContainsKey(value))
                return _fromFeatureTypeMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this ItemStatus value)
        {
            // Try to get the value from the mappings
            if (_fromItemStatusMap.ContainsKey(value))
                return _fromItemStatusMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this ItemType value)
        {
            // Try to get the value from the mappings
            if (_fromItemTypeMap.ContainsKey(value))
                return _fromItemTypeMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this LoadFlag value)
        {
            // Try to get the value from the mappings
            if (_fromLoadFlagMap.ContainsKey(value))
                return _fromLoadFlagMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this OpenMSXSubType value)
        {
            // Try to get the value from the mappings
            if (_fromOpenMSXSubTypeMap.ContainsKey(value))
                return _fromOpenMSXSubTypeMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this Relation value)
        {
            // Try to get the value from the mappings
            if (_fromRelationMap.ContainsKey(value))
                return _fromRelationMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this Runnable value)
        {
            // Try to get the value from the mappings
            if (_fromRunnableMap.ContainsKey(value))
                return _fromRunnableMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this SoftwareListStatus value)
        {
            // Try to get the value from the mappings
            if (_fromSoftwareListStatusMap.ContainsKey(value))
                return _fromSoftwareListStatusMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this Supported value, bool useSecond = false)
        {
            // Try to get the value from the mappings
            if (!useSecond && _fromSupportedMap.ContainsKey(value))
                return _fromSupportedMap[value];
            else if (useSecond && _fromSupportedSecondaryMap.ContainsKey(value))
                return _fromSupportedSecondaryMap[value];

            // Otherwise, return null
            return null;
        }

        /// <summary>
        /// Get the string value for an input enum, if possible
        /// </summary>
        /// <param name="value">Enum value to parse/param>
        /// <param name="useSecond">True to use the second mapping option, if it exists</param>
        /// <returns>String value representing the input, default on error</returns>
        public static string? AsStringValue(this SupportStatus value)
        {
            // Try to get the value from the mappings
            if (_fromSupportStatusMap.ContainsKey(value))
                return _fromSupportStatusMap[value];

            // Otherwise, return null
            return null;
        }

        #endregion
    }
}
