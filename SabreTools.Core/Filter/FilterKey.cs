using System;
using SabreTools.Core.Tools;
using SabreTools.Data.Models.Metadata;

namespace SabreTools.Core.Filter
{
    /// <summary>
    /// Represents a single filter key
    /// </summary>
    public class FilterKey
    {
        /// <summary>
        /// Item name associated with the filter
        /// </summary>
        public readonly string ItemName;

        /// <summary>
        /// Field name associated with the filter
        /// </summary>
        public readonly string FieldName;

        /// <summary>
        /// Validating combined key constructor
        /// </summary>
        public FilterKey(string? key)
        {
            if (!ParseFilterId(key, out string itemName, out string fieldName))
                throw new ArgumentException(nameof(key));

            ItemName = itemName;
            FieldName = fieldName;
        }

        /// <summary>
        /// Validating discrete value constructor
        /// </summary>
        public FilterKey(string itemName, string fieldName)
        {
            if (!ParseFilterId(ref itemName, ref fieldName))
                throw new ArgumentException(nameof(itemName));

            ItemName = itemName;
            FieldName = fieldName;
        }

        /// <inheritdoc/>
        public override string ToString() => $"{ItemName}.{FieldName}";

        /// <summary>
        /// Parse a filter ID string into the item name and field name, if possible
        /// </summary>
        private static bool ParseFilterId(string? itemFieldString, out string itemName, out string fieldName)
        {
            // Set default values
            itemName = string.Empty; fieldName = string.Empty;

            // If we don't have a filter ID, we can't do anything
            if (string.IsNullOrEmpty(itemFieldString))
                return false;

            // If we only have one part, we can't do anything
            string[] splitFilter = itemFieldString!.Split('.');
            if (splitFilter.Length != 2)
                return false;

            // Set and sanitize the filter ID
            itemName = splitFilter[0];
            fieldName = splitFilter[1];
            return ParseFilterId(ref itemName, ref fieldName);
        }

        /// <summary>
        /// Parse a filter ID string into the item name and field name, if possible
        /// </summary>
        private static bool ParseFilterId(ref string itemName, ref string fieldName)
        {
            // If we don't have a filter ID, we can't do anything
            if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(fieldName))
                return false;

            // Return santized values based on the split ID
            return itemName.ToLowerInvariant() switch
            {
                // Header
                "header" => ParseHeaderFilterId(ref itemName, ref fieldName),

                // Machine
                "game" => ParseMachineFilterId(ref itemName, ref fieldName),
                "machine" => ParseMachineFilterId(ref itemName, ref fieldName),
                "resource" => ParseMachineFilterId(ref itemName, ref fieldName),
                "set" => ParseMachineFilterId(ref itemName, ref fieldName),

                // DatItem
                "datitem" => ParseDatItemFilterId(ref itemName, ref fieldName),
                "item" => ParseDatItemFilterId(ref itemName, ref fieldName),
                _ => ParseDatItemFilterId(ref itemName, ref fieldName),
            };
        }

        /// <summary>
        /// Parse and validate header fields
        /// </summary>
        private static bool ParseHeaderFilterId(ref string itemName, ref string fieldName)
        {
            // Get the set of constants
            var constants = TypeHelper.GetConstants(typeof(Header));
            if (constants == null)
                return false;

            // Get if there's a match to the constant
            string localFieldName = fieldName;
            string? constantMatch = Array.Find(constants, c => string.Equals(c, localFieldName, StringComparison.OrdinalIgnoreCase));
            if (constantMatch == null)
                return false;

            // Return the sanitized ID
            itemName = MetadataFile.HeaderKey;
            fieldName = constantMatch;
            return true;
        }

        /// <summary>
        /// Parse and validate machine/game fields
        /// </summary>
        private static bool ParseMachineFilterId(ref string itemName, ref string fieldName)
        {
            // Get the set of constants
            var constants = TypeHelper.GetConstants(typeof(Machine));
            if (constants == null)
                return false;

            // Get if there's a match to the constant
            string localFieldName = fieldName;
            string? constantMatch = Array.Find(constants, c => string.Equals(c, localFieldName, StringComparison.OrdinalIgnoreCase));
            if (constantMatch == null)
                return false;

            // Return the sanitized ID
            itemName = MetadataFile.MachineKey;
            fieldName = constantMatch;
            return true;
        }

        /// <summary>
        /// Parse and validate item fields
        /// </summary>
        private static bool ParseDatItemFilterId(ref string itemName, ref string fieldName)
        {
            // Special case if the item name is reserved
            if (string.Equals(itemName, "datitem", StringComparison.OrdinalIgnoreCase)
                || string.Equals(itemName, "item", StringComparison.OrdinalIgnoreCase))
            {
                // Get all item types
                var itemTypes = TypeHelper.GetDatItemTypeNames();

                // If we get any matches
                string localFieldName = fieldName;
                string? matchedType = Array.Find(itemTypes, t => DatItemContainsField(t, localFieldName));
                if (matchedType != null)
                {
                    // Check for a matching field
                    string? matchedField = GetMatchingField(matchedType, fieldName);
                    if (matchedField == null)
                        return false;

                    itemName = "item";
                    fieldName = matchedField;
                    return true;
                }
            }
            else
            {
                // Check for a matching field
                string? matchedField = GetMatchingField(itemName, fieldName);
                if (matchedField == null)
                    return false;

                itemName = itemName.ToLowerInvariant();
                fieldName = matchedField;
                return true;
            }

            // Nothing was found
            return false;
        }

        /// <summary>
        /// Determine if an item type contains a field
        /// </summary>
        private static bool DatItemContainsField(string itemName, string fieldName)
            => GetMatchingField(itemName, fieldName) != null;

        /// <summary>
        /// Determine if an item type contains a field
        /// </summary>
        private static string? GetMatchingField(string itemName, string fieldName)
        {
            // Get the correct item type
            var itemType = TypeHelper.GetDatItemType(itemName.ToLowerInvariant());
            if (itemType == null)
                return null;

            // Get the set of constants
            var constants = TypeHelper.GetConstants(itemType);
            if (constants == null)
                return null;

            // Get if there's a match to the constant
            string localFieldName = fieldName;
            string? constantMatch = Array.Find(constants, c => string.Equals(c, localFieldName, StringComparison.OrdinalIgnoreCase));
            return constantMatch;
        }
    }
}
