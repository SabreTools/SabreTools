using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents a single filter within the overall filter
    /// </summary>
    /// <typeparam name="T">Generic type representing the filtered object</typeparam>
    public class FilterItem<T>
    {
        /// <summary>
        /// Single positive value for this filter
        /// </summary>
        public T Positive { get; set; }

        /// <summary>
        /// List of positive values for this filter
        /// </summary>
        public List<T> PositiveSet { get; set; } = new List<T>();

        /// <summary>
        /// Single negative value for this filter
        /// </summary>
        public T Negative { get; set; }

        /// <summary>
        /// List of negative values for this filter
        /// </summary>
        public List<T> NegativeSet { get; set; } = new List<T>();

        /// <summary>
        /// Single neutral value for this filter
        /// </summary>
        public T Neutral { get; set; }

        /// <summary>
        /// List of neutral values for this filter
        /// </summary>
        public List<T> NeutralSet { get; set; } = new List<T>();

        /// <summary>
        /// Check if a value matches the positive filter
        /// </summary>
        /// <param name="def">Default value to check filter value</param>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value was found in the positive filter, null on default value, false otherwise</returns>
        public bool? MatchesPositive(T def, T value)
        {
            return Matches(this.Positive, def, value);
        }

        /// <summary>
        /// Check if a value matches the negative filter
        /// </summary>
        /// <param name="def">Default value to check filter value</param>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value was found in the negative filter, null on default value, false otherwise</returns>
        public bool? MatchesNegative(T def, T value)
        {
            return Matches(this.Negative, def, value);
        }

        /// <summary>
        /// Check if a value matches the neutral filter
        /// </summary>
        /// <param name="def">Default value to check filter value</param>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value was found in the neutral filter, null on default value, false otherwise</returns>
        public bool? MatchesNeutral(T def, T value)
        {
            return Matches(this.Neutral, def, value);
        }

        /// <summary>
        /// Check if the given value matches any of the positive filters
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value was found in a positive filter, null on an empty set, false otherwise</returns>
        public bool? MatchesPositiveSet(T value)
        {
            return MatchesSet(this.PositiveSet, value);
        }

        /// <summary>
        /// Check if the given value matches any of the negative filters
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value was found in a negative filter, null on an empty set, false otherwise</returns>
        public bool? MatchesNegativeSet(T value)
        {
            return MatchesSet(this.NegativeSet, value);
        }

        /// <summary>
        /// Check if the given value matches any of the neutral filters
        /// </summary>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value was found in a neutral filter, null on an empty set, false otherwise</returns>
        public bool? MatchesNeutralSet(T value)
        {
            return MatchesSet(this.NeutralSet, value);
        }

        /// <summary>
        /// Check if a value matches the supplied filter
        /// </summary>
        /// <param name="single">Value to check against</param>
        /// <param name="def">Default value to check filter value</param>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value was found in the supplied filter, null on default value, false otherwise</returns>
        private bool? Matches(T single, T def, T value)
        {
            // If the filter is default, we ignore
            if (single.Equals(def))
                return null;

            // If we have a flag
            if (typeof(T).IsEnum && (single as Enum).HasFlag(value as Enum))
                return true;

            return single.Equals(value);
        }

        /// <summary>
        /// Check if a value matches the supplied filter
        /// </summary>
        /// <param name="set">Set to check against</param>
        /// <param name="value">Value to check</param>
        /// <returns>True if the value was found in the supplied filter, null on an empty set, false otherwise</returns>
        private bool? MatchesSet(List<T> set, T value)
        {
            if (set.Count == 0)
                return null;

            if (this.FindValueInList(set, value))
                return true;

            return false;
        }

        /// <summary>
        /// Generic code to check if a specific value is in the list given
        /// </summary>
        /// <param name="haystack">List to search for the value in</param>
        /// <param name="needle">Value to search the list for</param>
        /// <returns>True if the value could be found, false otherwise</returns>
        private bool FindValueInList(List<T> haystack, T needle)
        {
            bool found = false;
            foreach (T straw in haystack)
            {
                if (straw is string)
                {
                    string needleString = needle as string;
                    string strawString = straw as string;
                    if (!string.IsNullOrWhiteSpace(strawString) && needleString != null)
                    {
                        string regexStraw = strawString;

                        // If the straw has no special characters at all (excluding whitespace), treat it as an exact match
                        if (regexStraw == Regex.Escape(regexStraw).Replace("\\ ", " "))
                            regexStraw = $"^{regexStraw}$";

                        // Check if a match is found with the regex
                        found |= Regex.IsMatch(needleString, regexStraw, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    }
                }
                else
                {
                    found |= (needle.Equals(straw));
                }
            }

            return found;
        }
    }
}
