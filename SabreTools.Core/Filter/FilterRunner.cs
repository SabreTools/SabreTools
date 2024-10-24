using System;
using System.Collections.Generic;
using SabreTools.Core.Tools;
using SabreTools.Models.Metadata;

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
        public FilterObject[] Filters { get; }

        public FilterRunner(FilterObject[]? filters)
        {
            if (filters == null)
                throw new ArgumentNullException(nameof(filters));

            Filters = filters;
        }

        public FilterRunner(string[]? filterStrings)
        {
            if (filterStrings == null)
                throw new ArgumentNullException(nameof(filterStrings));

            var filters = new List<FilterObject>();
            foreach (string filterString in filterStrings)
            {
                filters.Add(new FilterObject(filterString));
            }

            Filters = [.. filters];
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

            // Loop through and run each filter in order
            foreach (var filter in Filters)
            {
                // If the filter isn't for this object type, skip
                if (filter.Key[0] != itemName)
                    continue;
                else if (filter.Key[0] == "item" && Array.IndexOf(TypeHelper.GetDatItemTypeNames(), itemName) > -1)
                    continue;

                // If we don't get a match, it's a failure
                if (!filter.Matches(dictionaryBase))
                    return false;
            }

            return true;
        }
    }
}