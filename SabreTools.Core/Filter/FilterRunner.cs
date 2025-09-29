using System;
using System.Collections.Generic;
using SabreTools.Core.Tools;
using SabreTools.Data.Models.Metadata;

namespace SabreTools.Core.Filter
{
    /// <summary>
    /// Represents a set of filters that can be run against an object
    /// </summary>
    public class FilterRunner
    {
        /// <summary>
        /// Set of filters to be run against an object
        /// </summary>
        public readonly Dictionary<string, FilterGroup> Filters = [];

        /// <summary>
        /// Cached item type names for filter selection
        /// </summary>
        private readonly string[] _datItemTypeNames = TypeHelper.GetDatItemTypeNames();

        public FilterRunner(FilterObject[] filters)
        {
            Array.ForEach(filters, AddFilter);
        }

        public FilterRunner(string[] filterStrings)
        {
            Array.ForEach(filterStrings, AddFilter);
        }

        /// <summary>
        /// Run filtering on a DictionaryBase item
        /// </summary>
        public bool Run(DictionaryBase dictionaryBase)
        {
            string? itemName = dictionaryBase switch
            {
                Header => MetadataFile.HeaderKey,
                Machine => MetadataFile.MachineKey,
                DatItem => TypeHelper.GetXmlRootAttributeElementName(dictionaryBase.GetType()),
                _ => null,
            };

            // Null is invalid
            if (itemName == null)
                return false;

            // Loop through and run each filter in order
            foreach (var filterKey in Filters.Keys)
            {
                // Skip filters not applicable to the item
                if (filterKey.StartsWith("item.") && Array.IndexOf(_datItemTypeNames, itemName) == -1)
                    continue;
                else if (!filterKey.StartsWith("item.") && !filterKey.StartsWith(itemName))
                    continue;

                // If we don't get a match, it's a failure
                bool matchOne = Filters[filterKey].Matches(dictionaryBase);
                if (!matchOne)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Add a single filter to the runner in a group by key
        /// </summary>
        private void AddFilter(FilterObject filter)
        {
            // Get the key as a string
            string key = filter.Key.ToString();

            // Special case for machine types
            if (filter.Key.ItemName == MetadataFile.MachineKey && filter.Key.FieldName == Machine.IsBiosKey)
                key = $"{MetadataFile.MachineKey}.COMBINEDTYPE";
            else if (filter.Key.ItemName == MetadataFile.MachineKey && filter.Key.FieldName == Machine.IsDeviceKey)
                key = $"{MetadataFile.MachineKey}.COMBINEDTYPE";
            else if (filter.Key.ItemName == MetadataFile.MachineKey && filter.Key.FieldName == Machine.IsMechanicalKey)
                key = $"{MetadataFile.MachineKey}.COMBINEDTYPE";

            // Ensure the key exists
            if (!Filters.ContainsKey(key))
                Filters[key] = new FilterGroup(GroupType.OR);

            // Add the filter to the set
            Filters[key].AddFilter(filter);
        }

        /// <summary>
        /// Add a single filter to the runner in a group by key
        /// </summary>
        private void AddFilter(string filterString)
        {
            try
            {
                var filter = new FilterObject(filterString);
                AddFilter(filter);
            }
            catch { }
        }
    }
}
