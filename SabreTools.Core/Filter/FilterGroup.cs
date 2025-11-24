using System.Collections.Generic;
using System.Text.RegularExpressions;
using SabreTools.Data.Models.Metadata;

namespace SabreTools.Core.Filter
{
    /// <summary>
    /// Represents a set of filters and groups
    /// </summary>
    public class FilterGroup
    {
        /// <summary>
        /// How to apply the group filters
        /// </summary>
        public readonly GroupType GroupType;

        /// <summary>
        /// All standalone filters in the group
        /// </summary>
        private readonly List<FilterObject> _subfilters = [];

        /// <summary>
        /// All filter groups contained in the group
        /// </summary>
        private readonly List<FilterGroup> _subgroups = [];

        public FilterGroup(GroupType groupType)
        {
            GroupType = groupType;
        }

        public FilterGroup(FilterObject[] filters, GroupType groupType)
        {
            _subfilters.AddRange(filters);
            GroupType = groupType;
        }

        public FilterGroup(FilterGroup[] groups, GroupType groupType)
        {
            _subgroups.AddRange(groups);
            GroupType = groupType;
        }

        public FilterGroup(FilterObject[] filters, FilterGroup[] groups, GroupType groupType)
        {
            _subfilters.AddRange(filters);
            _subgroups.AddRange(groups);
            GroupType = groupType;
        }

        #region Accessors

        /// <summary>
        /// Add a FilterObject to the set
        /// </summary>
        public void AddFilter(FilterObject filter) => _subfilters.Add(filter);

        /// <summary>
        /// Add a FilterGroup to the set
        /// </summary>
        public void AddGroup(FilterGroup group) => _subgroups.Add(group);

        #endregion

        #region Matching

        /// <summary>
        /// Determine if a DictionaryBase object matches the group
        /// </summary>
        public bool Matches(DictionaryBase dictionaryBase)
        {
            return GroupType switch
            {
                GroupType.AND => MatchesAnd(dictionaryBase),
                GroupType.OR => MatchesOr(dictionaryBase),
                _ => false,
            };
        }

        /// <summary>
        /// Determines if a value matches all filters
        /// </summary>
        private bool MatchesAnd(DictionaryBase dictionaryBase)
        {
            // Run standalone filters
            foreach (var filter in _subfilters)
            {
                // One failed match fails the group
                if (!filter.Matches(dictionaryBase))
                    return false;
            }

            // Run filter subgroups
            foreach (var group in _subgroups)
            {
                // One failed match fails the group
                if (!group.Matches(dictionaryBase))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if a value matches any filters
        /// </summary>
        private bool MatchesOr(DictionaryBase dictionaryBase)
        {
            // Run standalone filters
            foreach (var filter in _subfilters)
            {
                // One successful match passes the group
                if (filter.Matches(dictionaryBase))
                    return true;
            }

            // Run filter subgroups
            foreach (var group in _subgroups)
            {
                // One successful match passes the group
                if (group.Matches(dictionaryBase))
                    return true;
            }

            return false;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Derive a group type from the input string, if possible
        /// </summary>
        private static GroupType GetGroupType(string? groupType)
        {
            return groupType?.ToLowerInvariant() switch
            {
                "&" => GroupType.AND,
                "&&" => GroupType.AND,

                "|" => GroupType.OR,
                "||" => GroupType.OR,

                _ => GroupType.NONE,
            };
        }

        /// <summary>
        /// Parse an input string into a filter group
        /// </summary>
        private static void Parse(string? input)
        {
            // Tokenize the string
            string[] tokens = Tokenize(input);
            if (tokens.Length == 0)
                return;

            // Loop through the tokens and parse
            for (int i = 0; i < tokens.Length; i++)
            {
                // TODO: Implement parsing
                // - Opening parenthesis means a new group
                // - Closing parenthesis means finalize group and return it
                // - Current starting and ending with a parenthesis strips them off
                // - Unbalanced parenthesis can only be found on parse
                // - Failed parsing of FilterObjects(?)
                // - Invalid FilterObjects(?)
            }
        }

        /// <summary>
        /// Tokenize an input string for parsing
        /// </summary>
        private static string[] Tokenize(string? input)
        {
            // Null inputs are ignored
            if (input == null)
                return [];

            // Split the string into parseable pieces
            // - Left and right parenthesis are separate
            // - Operators & and | are separate
            // - Key-value pairs are enforced for statements
            // - Numbers can be a value without quotes
            // - All other values require quotes
            return Regex.Split(input, @"(\(|\)|[&|]{1,2}|[^\s()""]+[:!=]\d+|[^\s()""]+[:!=]{1,2}""[^""]*"")", RegexOptions.Compiled);
        }

        #endregion
    }
}
