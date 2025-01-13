﻿using System;
using System.Collections;
#if NET40_OR_GREATER || NETCOREAPP
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Filter;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using SabreTools.IO.Logging;
using SabreTools.Matching.Compare;

namespace SabreTools.DatFiles
{
    /// <summary>
    /// Item dictionary with statistics, bucketing, and sorting
    /// </summary>
    [JsonObject("items"), XmlRoot("items")]
    public class ItemDictionary : IDictionary<string, List<DatItem>?>
    {
        #region Private instance variables

        /// <summary>
        /// Determine the bucketing key for all items
        /// </summary>
        private ItemKey bucketedBy;

        /// <summary>
        /// Determine merging type for all items
        /// </summary>
        private DedupeType mergedBy;

        /// <summary>
        /// Internal dictionary for the class
        /// </summary>
#if NET40_OR_GREATER || NETCOREAPP
        private readonly ConcurrentDictionary<string, List<DatItem>?> _items = [];
#else
        private readonly Dictionary<string, List<DatItem>?> _items = [];
#endif

        /// <summary>
        /// Logging object
        /// </summary>
        private readonly Logger _logger;

        #endregion

        #region Publically available fields

        #region Keys

        /// <summary>
        /// Get the keys from the file dictionary
        /// </summary>
        /// <returns>List of the keys</returns>
        [JsonIgnore, XmlIgnore]
        public ICollection<string> Keys
        {
            get { return _items.Keys; }
        }

        /// <summary>
        /// Get the keys in sorted order from the file dictionary
        /// </summary>
        /// <returns>List of the keys in sorted order</returns>
        [JsonIgnore, XmlIgnore]
        public List<string> SortedKeys
        {
            get
            {
                List<string> keys = [.. _items.Keys];
                keys.Sort(new NaturalComparer());
                return keys;
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// DAT statistics
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public DatStatistics DatStatistics { get; } = new DatStatistics();

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Generic constructor
        /// </summary>
        public ItemDictionary()
        {
            bucketedBy = ItemKey.NULL;
            mergedBy = DedupeType.None;
            _logger = new Logger(this);
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Passthrough to access the file dictionary
        /// </summary>
        /// <param name="key">Key in the dictionary to reference</param>
        public List<DatItem>? this[string key]
        {
            get
            {
                // Explicit lock for some weird corner cases
                lock (key)
                {
                    // Ensure the key exists
                    EnsureKey(key);

                    // Now return the value
                    return _items[key];
                }
            }
            set
            {
                Remove(key);
                if (value == null)
                    _items[key] = null;
                else
                    Add(key, value);
            }
        }

        /// <summary>
        /// Add a value to the file dictionary
        /// </summary>
        /// <param name="key">Key in the dictionary to add to</param>
        /// <param name="value">Value to add to the dictionary</param>
        public void Add(string key, DatItem value)
        {
            // Explicit lock for some weird corner cases
            lock (key)
            {
                // Ensure the key exists
                EnsureKey(key);

                // If item is null, don't add it
                if (value == null)
                    return;

                // Now add the value
                _items[key]!.Add(value);

                // Now update the statistics
                DatStatistics.AddItemStatistics(value);
            }
        }

        /// <summary>
        /// Add a range of values to the file dictionary
        /// </summary>
        /// <param name="key">Key in the dictionary to add to</param>
        /// <param name="value">Value to add to the dictionary</param>
        public void Add(string key, List<DatItem>? value)
        {
            // Explicit lock for some weird corner cases
            lock (key)
            {
                // If the value is null or empty, just return
                if (value == null || value.Count == 0)
                    return;

                // Ensure the key exists
                EnsureKey(key);

                // Now add the value
                _items[key]!.AddRange(value);

                // Now update the statistics
                foreach (DatItem item in value)
                {
                    DatStatistics.AddItemStatistics(item);
                }
            }
        }

        /// <summary>
        /// Add a DatItem to the dictionary after checking
        /// </summary>
        /// <param name="item">Item data to check against</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <returns>The key for the item</returns>
        public string AddItem(DatItem item, bool statsOnly)
        {
            string key;

            // If we have a Disk, Media, or Rom, clean the hash data
            if (item is Disk disk)
            {
                // If the file has aboslutely no hashes, skip and log
                if (disk.GetStringFieldValue(Models.Metadata.Disk.StatusKey).AsEnumValue<ItemStatus>() != ItemStatus.Nodump
                    && string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.MD5Key))
                    && string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key)))
                {
                    _logger.Verbose($"Incomplete entry for '{disk.GetName()}' will be output as nodump");
                    disk.SetFieldValue<string?>(Models.Metadata.Disk.StatusKey, ItemStatus.Nodump.AsStringValue());
                }

                item = disk;
            }
            if (item is Media media)
            {
                // If the file has aboslutely no hashes, skip and log
                if (string.IsNullOrEmpty(media.GetStringFieldValue(Models.Metadata.Media.MD5Key))
                    && string.IsNullOrEmpty(media.GetStringFieldValue(Models.Metadata.Media.SHA1Key))
                    && string.IsNullOrEmpty(media.GetStringFieldValue(Models.Metadata.Media.SHA256Key))
                    && string.IsNullOrEmpty(media.GetStringFieldValue(Models.Metadata.Media.SpamSumKey)))
                {
                    _logger.Verbose($"Incomplete entry for '{media.GetName()}' will be output as nodump");
                }

                item = media;
            }
            else if (item is Rom rom)
            {
                long? size = rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey);

                // If we have the case where there is SHA-1 and nothing else, we don't fill in any other part of the data
                if (size == null && !rom.HasHashes())
                {
                    // No-op, just catch it so it doesn't go further
                    //logger.Verbose($"{Header.GetStringFieldValue(DatHeader.FileNameKey)}: Entry with only SHA-1 found - '{rom.GetName()}'");
                }

                // If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
                else if ((size == 0 || size == null)
                    && (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey)) || rom.HasZeroHash()))
                {
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SizeKey, Constants.SizeZero.ToString());
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.CRCKey, ZeroHash.CRC32Str);
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.MD2Key, null); // ZeroHash.GetString(HashType.MD2)
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.MD4Key, null); // ZeroHash.GetString(HashType.MD4)
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.MD5Key, ZeroHash.MD5Str);
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA1Key, ZeroHash.SHA1Str);
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA256Key, null); // ZeroHash.SHA256Str;
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA384Key, null); // ZeroHash.SHA384Str;
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA512Key, null); // ZeroHash.SHA512Str;
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SpamSumKey, null); // ZeroHash.SpamSumStr;
                }

                // If the file has no size and it's not the above case, skip and log
                else if (rom.GetStringFieldValue(Models.Metadata.Rom.StatusKey).AsEnumValue<ItemStatus>() != ItemStatus.Nodump && (size == 0 || size == null))
                {
                    //logger.Verbose($"{Header.GetStringFieldValue(DatHeader.FileNameKey)}: Incomplete entry for '{rom.GetName()}' will be output as nodump");
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.StatusKey, ItemStatus.Nodump.AsStringValue());
                }

                // If the file has a size but aboslutely no hashes, skip and log
                else if (rom.GetStringFieldValue(Models.Metadata.Rom.StatusKey).AsEnumValue<ItemStatus>() != ItemStatus.Nodump
                    && size != null && size > 0
                    && !rom.HasHashes())
                {
                    //logger.Verbose($"{Header.GetStringFieldValue(DatHeader.FileNameKey)}: Incomplete entry for '{rom.GetName()}' will be output as nodump");
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.StatusKey, ItemStatus.Nodump.AsStringValue());
                }

                item = rom;
            }

            // Get the key and add the file
            key = item.GetKey(ItemKey.Machine);

            // If only adding statistics, we add an empty key for games and then just item stats
            if (statsOnly)
            {
                EnsureKey(key);
                DatStatistics.AddItemStatistics(item);
            }
            else
            {
                Add(key, item);
            }

            return key;
        }

        /// <summary>
        /// Remove any keys that have null or empty values
        /// </summary>
        internal void ClearEmpty()
        {
            string[] keys = [.. Keys];
            foreach (string key in keys)
            {
#if NET40_OR_GREATER || NETCOREAPP
                // If the key doesn't exist, skip
                if (!_items.TryGetValue(key, out var value))
                    continue;

                // If the value is null, remove
                else if (value == null)
                    _items.TryRemove(key, out _);

                // If there are no non-blank items, remove
                else if (value!.FindIndex(i => i != null && i is not Blank) == -1)
                    _items.TryRemove(key, out _);
#else
                // If the key doesn't exist, skip
                if (!_items.ContainsKey(key))
                    continue;

                // If the value is null, remove
                else if (_items[key] == null)
                    _items.Remove(key);

                // If there are no non-blank items, remove
                else if (_items[key]!.FindIndex(i => i != null && i is not Blank) == -1)
                    _items.Remove(key);
#endif
            }
        }

        /// <summary>
        /// Remove all items marked for removal
        /// </summary>
        internal void ClearMarked()
        {
            string[] keys = [.. Keys];
            foreach (string key in keys)
            {
                // Skip invalid item lists
                List<DatItem>? oldItemList = this[key];
                if (oldItemList == null)
                    return;

                List<DatItem> newItemList = oldItemList.FindAll(i => i.GetBoolFieldValue(DatItem.RemoveKey) != true);

                Remove(key);
                Add(key, newItemList);
            }
        }

        /// <summary>
        /// Get if the file dictionary contains the key
        /// </summary>
        /// <param name="key">Key in the dictionary to check</param>
        /// <returns>True if the key exists, false otherwise</returns>
        public bool ContainsKey(string key)
        {
            // If the key is null, we return false since keys can't be null
            if (key == null)
                return false;

            // Explicit lock for some weird corner cases
            lock (key)
            {
                return _items.ContainsKey(key);
            }
        }

        /// <summary>
        /// Get if the file dictionary contains the key and value
        /// </summary>
        /// <param name="key">Key in the dictionary to check</param>
        /// <param name="value">Value in the dictionary to check</param>
        /// <returns>True if the key exists, false otherwise</returns>
        public bool Contains(string key, DatItem value)
        {
            // If the key is null, we return false since keys can't be null
            if (key == null)
                return false;

            // Explicit lock for some weird corner cases
            lock (key)
            {
#if NET40_OR_GREATER || NETCOREAPP
                if (_items.TryGetValue(key, out var list) && list != null)
                    return list.Contains(value);
#else
                if (_items.ContainsKey(key) && _items[key] != null)
                    return _items[key]!.Contains(value);
#endif
            }

            return false;
        }

        /// <summary>
        /// Ensure the key exists in the items dictionary
        /// </summary>
        /// <param name="key">Key to ensure</param>
        public void EnsureKey(string key)
        {
            // If the key is missing from the dictionary, add it
            if (!_items.ContainsKey(key))
#if NET40_OR_GREATER || NETCOREAPP
                _items.TryAdd(key, []);
#else
                _items[key] = [];
#endif
        }

        /// <summary>
        /// Get the items associated with a bucket name
        /// </summary>
        public List<DatItem> GetItemsForBucket(string bucketName, bool filter = false)
        {
            if (!_items.ContainsKey(bucketName))
                return [];

            var items = _items[bucketName];
            if (items == null)
                return [];

            var datItems = new List<DatItem>();
            foreach (DatItem item in items)
            {
                if (!filter || item.GetBoolFieldValue(DatItem.RemoveKey) != true)
                    datItems.Add(item);
            }

            return datItems;
        }

        /// <summary>
        /// Remove a key from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove</param>
        public bool Remove(string key)
        {
            // Explicit lock for some weird corner cases
            lock (key)
            {
                // If the key doesn't exist, return
                if (!ContainsKey(key) || _items[key] == null)
                    return false;

                // Remove the statistics first
                foreach (DatItem item in _items[key]!)
                {
                    DatStatistics.RemoveItemStatistics(item);
                }

                // Remove the key from the dictionary
#if NET40_OR_GREATER || NETCOREAPP
                return _items.TryRemove(key, out _);
#else
                return _items.Remove(key);
#endif
            }
        }

        /// <summary>
        /// Remove the first instance of a value from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove from</param>
        /// <param name="value">Value to remove from the dictionary</param>
        public bool Remove(string key, DatItem value)
        {
            // Explicit lock for some weird corner cases
            lock (key)
            {
                // If the key and value doesn't exist, return
                if (!Contains(key, value) || _items[key] == null)
                    return false;

                // Remove the statistics first
                DatStatistics.RemoveItemStatistics(value);

                return _items[key]!.Remove(value);
            }
        }

        /// <summary>
        /// Reset a key from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to reset</param>
        public bool Reset(string key)
        {
            // If the key doesn't exist, return
            if (!ContainsKey(key) || _items[key] == null)
                return false;

            // Remove the statistics first
            foreach (DatItem item in _items[key]!)
            {
                DatStatistics.RemoveItemStatistics(item);
            }

            // Remove the key from the dictionary
            _items[key] = [];
            return true;
        }

        /// <summary>
        /// Override the internal ItemKey value
        /// </summary>
        /// <param name="newBucket"></param>
        public void SetBucketedBy(ItemKey newBucket)
        {
            bucketedBy = newBucket;
        }

        #endregion

        #region Bucketing

        /// <summary>
        /// Take the arbitrarily bucketed Files Dictionary and convert to one bucketed by a user-defined method
        /// </summary>
        /// <param name="bucketBy">ItemKey enum representing how to bucket the individual items</param>
        /// <param name="dedupeType">Dedupe type that should be used</param>
        /// <param name="lower">True if the key should be lowercased (default), false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        internal void BucketBy(ItemKey bucketBy, DedupeType dedupeType, bool lower = true, bool norename = true)
        {
            // If we have a situation where there's no dictionary or no keys at all, we skip
            if (_items == null || _items.Count == 0)
                return;

            // If the sorted type isn't the same, we want to sort the dictionary accordingly
            if (bucketedBy != bucketBy && bucketBy != ItemKey.NULL)
            {
                _logger.User($"Organizing roms by {bucketBy}");
                PerformBucketing(bucketBy, lower, norename);
            }

            // If the merge type isn't the same, we want to merge the dictionary accordingly
            if (mergedBy != dedupeType)
            {
                _logger.User($"Deduping roms by {dedupeType}");
                PerformDeduplication(bucketBy, dedupeType);
            }
            // If the merge type is the same, we want to sort the dictionary to be consistent
            else
            {
                _logger.User($"Sorting roms by {bucketBy}");
                PerformSorting();
            }
        }

        /// <summary>
        /// List all duplicates found in a DAT based on a DatItem
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>List of matched DatItem objects</returns>
        internal List<DatItem> GetDuplicates(DatItem datItem, bool sorted = false)
        {
            List<DatItem> output = [];

            // Check for an empty rom list first
            if (DatStatistics.TotalCount == 0)
                return output;

            // We want to get the proper key for the DatItem
            string key = SortAndGetKey(datItem, sorted);

            // If the key doesn't exist, return the empty list
            if (!ContainsKey(key))
                return output;

            // Try to find duplicates
            List<DatItem>? roms = this[key];
            if (roms == null)
                return output;

            List<DatItem> left = [];
            for (int i = 0; i < roms.Count; i++)
            {
                DatItem other = roms[i];
                if (other.GetBoolFieldValue(DatItem.RemoveKey) == true)
                    continue;

                if (datItem.Equals(other))
                {
                    other.SetFieldValue<bool?>(DatItem.RemoveKey, true);
                    output.Add(other);
                }
                else
                {
                    left.Add(other);
                }
            }

            // Add back all roms with the proper flags
            Remove(key);
            Add(key, output);
            Add(key, left);

            return output;
        }

        /// <summary>
        /// Check if a DAT contains the given DatItem
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>True if it contains the rom, false otherwise</returns>
        internal bool HasDuplicates(DatItem datItem, bool sorted = false)
        {
            // Check for an empty rom list first
            if (DatStatistics.TotalCount == 0)
                return false;

            // We want to get the proper key for the DatItem
            string key = SortAndGetKey(datItem, sorted);

            // If the key doesn't exist, return the empty list
            if (!ContainsKey(key))
                return false;

            // Try to find duplicates
            List<DatItem>? roms = this[key];
            if (roms == null)
                return false;

            return roms.FindIndex(r => datItem.Equals(r)) > -1;
        }

        /// <summary>
        /// Get the highest-order Field value that represents the statistics
        /// </summary>
        private ItemKey GetBestAvailable()
        {
            // Get the required counts
            long diskCount = DatStatistics.GetItemCount(ItemType.Disk);
            long mediaCount = DatStatistics.GetItemCount(ItemType.Media);
            long romCount = DatStatistics.GetItemCount(ItemType.Rom);
            long nodumpCount = DatStatistics.GetStatusCount(ItemStatus.Nodump);

            // If all items are supposed to have a SHA-512, we bucket by that
            if (diskCount + mediaCount + romCount - nodumpCount == DatStatistics.GetHashCount(HashType.SHA512))
                return ItemKey.SHA512;

            // If all items are supposed to have a SHA-384, we bucket by that
            else if (diskCount + mediaCount + romCount - nodumpCount == DatStatistics.GetHashCount(HashType.SHA384))
                return ItemKey.SHA384;

            // If all items are supposed to have a SHA-256, we bucket by that
            else if (diskCount + mediaCount + romCount - nodumpCount == DatStatistics.GetHashCount(HashType.SHA256))
                return ItemKey.SHA256;

            // If all items are supposed to have a SHA-1, we bucket by that
            else if (diskCount + mediaCount + romCount - nodumpCount == DatStatistics.GetHashCount(HashType.SHA1))
                return ItemKey.SHA1;

            // If all items are supposed to have a MD5, we bucket by that
            else if (diskCount + mediaCount + romCount - nodumpCount == DatStatistics.GetHashCount(HashType.MD5))
                return ItemKey.MD5;

            // If all items are supposed to have a MD4, we bucket by that
            else if (diskCount + mediaCount + romCount - nodumpCount == DatStatistics.GetHashCount(HashType.MD4))
                return ItemKey.MD4;

            // If all items are supposed to have a MD2, we bucket by that
            else if (diskCount + mediaCount + romCount - nodumpCount == DatStatistics.GetHashCount(HashType.MD2))
                return ItemKey.MD2;

            // Otherwise, we bucket by CRC
            else
                return ItemKey.CRC;
        }

        /// <summary>
        /// Perform bucketing based on the item key provided
        /// </summary>
        /// <param name="bucketBy">ItemKey enum representing how to bucket the individual items</param>
        /// <param name="lower">True if the key should be lowercased, false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        private void PerformBucketing(ItemKey bucketBy, bool lower, bool norename)
        {
            // Set the sorted type
            bucketedBy = bucketBy;

            // Reset the merged type since this might change the merge
            mergedBy = DedupeType.None;

            // First do the initial sort of all of the roms inplace
            List<string> oldkeys = [.. Keys];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, oldkeys.Count, Core.Globals.ParallelOptions, k =>
#elif NET40_OR_GREATER
            Parallel.For(0, oldkeys.Count, k =>
#else
            for (int k = 0; k < oldkeys.Count; k++)
#endif
            {
                string key = oldkeys[k];
                if (this[key] == null)
                    Remove(key);

                // Now add each of the roms to their respective keys
                for (int i = 0; i < this[key]!.Count; i++)
                {
                    DatItem item = this[key]![i];
                    if (item == null)
                        continue;

                    // We want to get the key most appropriate for the given sorting type
                    string newkey = item.GetKey(bucketBy, lower, norename);

                    // If the key is different, move the item to the new key
                    if (newkey != key)
                    {
                        Add(newkey, item);
                        Remove(key, item);
                        i--; // This make sure that the pointer stays on the correct since one was removed
                    }
                }

                // If the key is now empty, remove it
                if (this[key]!.Count == 0)
                    Remove(key);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Perform deduplication based on the deduplication type provided
        /// </summary>
        /// <param name="bucketBy">ItemKey enum representing how to bucket the individual items</param>
        /// <param name="dedupeType">Dedupe type that should be used</param>
        private void PerformDeduplication(ItemKey bucketBy, DedupeType dedupeType)
        {
            // Set the sorted type
            mergedBy = dedupeType;

            List<string> keys = [.. Keys];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(keys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(keys, key =>
#else
            foreach (var key in keys)
#endif
            {
                // Get the possibly unsorted list
                List<DatItem>? sortedlist = this[key];
                if (sortedlist == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                // Sort the list of items to be consistent
                DatFileTool.Sort(ref sortedlist, false);

                // If we're merging the roms, do so
                if (dedupeType == DedupeType.Full || (dedupeType == DedupeType.Game && bucketBy == ItemKey.Machine))
                    sortedlist = DatFileTool.Merge(sortedlist);

                // Add the list back to the dictionary
                Reset(key);
                Add(key, sortedlist);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Perform inplace sorting of the dictionary
        /// </summary>
        private void PerformSorting()
        {
            List<string> keys = [.. Keys];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(keys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(keys, key =>
#else
            foreach (var key in keys)
#endif
            {
                // Get the possibly unsorted list
                List<DatItem>? sortedlist = this[key];

                // Sort the list of items to be consistent
                if (sortedlist != null)
                    DatFileTool.Sort(ref sortedlist, false);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Sort the input DAT and get the key to be used by the item
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>Key to try to use</returns>
        private string SortAndGetKey(DatItem datItem, bool sorted = false)
        {
            // If we're not already sorted, take care of it
            if (!sorted)
                BucketBy(GetBestAvailable(), DedupeType.None);

            // Now that we have the sorted type, we get the proper key
            return datItem.GetKey(bucketedBy);
        }

        #endregion

        // TODO: All internal, can this be put into a better location?
        #region Filtering

        /// <summary>
        /// Execute all filters in a filter runner on the items in the dictionary
        /// </summary>
        /// <param name="filterRunner">Preconfigured filter runner to use</param>
        internal void ExecuteFilters(FilterRunner filterRunner)
        {
            List<string> keys = [.. Keys];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(keys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(keys, key =>
#else
            foreach (var key in keys)
#endif
            {
                List<DatItem>? items = this[key];
                if (items == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                // Filter all items in the current key
                List<DatItem> newItems = [];
                foreach (var item in items)
                {
                    if (item.PassesFilter(filterRunner))
                        newItems.Add(item);
                }

                // Set the value in the key to the new set
                this[key] = newItems;

#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Use game descriptions as names, updating cloneof/romof/sampleof
        /// </summary>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        internal void MachineDescriptionToName(bool throwOnError = false)
        {
            try
            {
                // First we want to get a mapping for all games to description
                var mapping = CreateMachineToDescriptionMapping();

                // Now we loop through every item and update accordingly
                UpdateMachineNamesFromDescriptions(mapping);
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Warning(ex.ToString());
            }
        }

        /// <summary>
        /// Ensure that all roms are in their own game (or at least try to ensure)
        /// </summary>
        internal void SetOneRomPerGame()
        {
            // For each rom, we want to update the game to be "<game name>/<rom name>"
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(Keys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(Keys, key =>
#else
            foreach (var key in Keys)
#endif
            {
                var items = this[key];
                if (items == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                for (int i = 0; i < items.Count; i++)
                {
                    SetOneRomPerGame(items[i]);
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Filter a DAT using 1G1R logic given an ordered set of regions
        /// </summary>
        /// <param name="regionList">List of regions in order of priority</param>
        /// <remarks>
        /// In the most technical sense, the way that the region list is being used does not
        /// confine its values to be just regions. Since it's essentially acting like a
        /// specialized version of the machine name filter, anything that is usually encapsulated
        /// in parenthesis would be matched on, including disc numbers, languages, editions,
        /// and anything else commonly used. Please note that, unlike other existing 1G1R 
        /// solutions, this does not have the ability to contain custom mappings of parent
        /// to clone sets based on name, nor does it have the ability to match on the 
        /// Release DatItem type.
        /// </remarks>
        internal void SetOneGamePerRegion(List<string> regionList)
        {
            // If we have null region list, make it empty
            regionList ??= [];

            // For sake of ease, the first thing we want to do is bucket by game
            BucketBy(ItemKey.Machine, DedupeType.None, norename: true);

            // Then we want to get a mapping of all machines to parents
            Dictionary<string, List<string>> parents = [];
            foreach (string key in Keys)
            {
                DatItem item = this[key]![0];

                // Get machine information
                Machine? machine = item.GetFieldValue<Machine>(DatItem.MachineKey);
                string? machineName = machine?.GetStringFieldValue(Models.Metadata.Machine.NameKey)?.ToLowerInvariant();
                if (machine == null || machineName == null)
                    continue;

                // Get the string values
                string? cloneOf = machine.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey)?.ToLowerInvariant();
                string? romOf = machine.GetStringFieldValue(Models.Metadata.Machine.RomOfKey)?.ToLowerInvariant();

                // Match on CloneOf first
                if (!string.IsNullOrEmpty(cloneOf))
                {
                    if (!parents.ContainsKey(cloneOf!))
                        parents.Add(cloneOf!, []);

                    parents[cloneOf!].Add(machineName);
                }

                // Then by RomOf
                else if (!string.IsNullOrEmpty(romOf))
                {
                    if (!parents.ContainsKey(romOf!))
                        parents.Add(romOf!, []);

                    parents[romOf!].Add(machineName);
                }

                // Otherwise, treat it as a parent
                else
                {
                    if (!parents.ContainsKey(machineName))
                        parents.Add(machineName, []);

                    parents[machineName].Add(machineName);
                }
            }

            // Once we have the full list of mappings, filter out games to keep
            foreach (string key in parents.Keys)
            {
                // Find the first machine that matches the regions in order, if possible
                string? machine = default;
                foreach (string region in regionList)
                {
                    machine = parents[key].Find(m => Regex.IsMatch(m, @"\(.*" + region + @".*\)", RegexOptions.IgnoreCase));
                    if (machine != default)
                        break;
                }

                // If we didn't get a match, use the parent
                if (machine == default)
                    machine = key;

                // Remove the key from the list
                parents[key].Remove(machine);

                // Remove the rest of the items from this key
                parents[key].ForEach(k => Remove(k));
            }

            // Finally, strip out the parent tags
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Strip the dates from the beginning of scene-style set names
        /// </summary>
        internal void StripSceneDatesFromItems()
        {
            // Set the regex pattern to use
            string pattern = @"([0-9]{2}\.[0-9]{2}\.[0-9]{2}-)(.*?-.*?)";

            // Now process all of the roms
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(Keys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(Keys, key =>
#else
            foreach (var key in Keys)
#endif
            {
                var items = this[key];
                if (items == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                for (int j = 0; j < items.Count; j++)
                {
                    DatItem item = items[j];
                    if (Regex.IsMatch(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)!, pattern))
                        item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, Regex.Replace(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)!, pattern, "$2"));

                    if (Regex.IsMatch(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.DescriptionKey)!, pattern))
                        item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, Regex.Replace(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.DescriptionKey)!, pattern, "$2"));

                    items[j] = item;
                }

                Remove(key);
                Add(key, items);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Create machine to description mapping dictionary
        /// </summary>
        private IDictionary<string, string> CreateMachineToDescriptionMapping()
        {
#if NET40_OR_GREATER || NETCOREAPP
            ConcurrentDictionary<string, string> mapping = new();
#else
            Dictionary<string, string> mapping = [];
#endif
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(Keys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(Keys, key =>
#else
            foreach (var key in Keys)
#endif
            {
                var items = this[key];
                if (items == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                foreach (DatItem item in items)
                {
                    // If the key mapping doesn't exist, add it
#if NET40_OR_GREATER || NETCOREAPP
                    mapping.TryAdd(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)!,
                        item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.DescriptionKey)!.Replace('/', '_').Replace("\"", "''").Replace(":", " -"));
#else
                    mapping[item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)!]
                        = item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.DescriptionKey)!.Replace('/', '_').Replace("\"", "''").Replace(":", " -");
#endif
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            return mapping;
        }

        /// <summary>
        /// Set internal names to match One Rom Per Game (ORPG) logic
        /// </summary>
        /// <param name="datItem">DatItem to run logic on</param>
        private static void SetOneRomPerGame(DatItem datItem)
        {
            // If the item name is null
            string? machineName = datItem.GetName();
            if (machineName == null)
                return;

            // Get the current machine
            var machine = datItem.GetFieldValue<Machine>(DatItem.MachineKey);
            if (machine == null)
                return;

            // Remove extensions from Rom items
            if (datItem is Rom)
            {
                string[] splitname = machineName.Split('.');
                machineName = machine.GetStringFieldValue(Models.Metadata.Machine.NameKey)
                    + $"/{string.Join(".", splitname, 0, splitname.Length > 1 ? splitname.Length - 1 : 1)}";
            }

            // Strip off "Default" prefix only for ORPG
            if (machineName.StartsWith("Default"))
                machineName = machineName.Substring("Default".Length + 1);

            datItem.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, machineName);
            datItem.SetName(Path.GetFileName(datItem.GetName()));
        }

        /// <summary>
        /// Update machine names from descriptions according to mappings
        /// </summary>
        private void UpdateMachineNamesFromDescriptions(IDictionary<string, string> mapping)
        {
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(Keys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(Keys, key =>
#else
            foreach (var key in Keys)
#endif
            {
                var items = this[key];
                if (items == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                List<DatItem> newItems = [];
                foreach (DatItem item in items)
                {
                    // Update machine name
                    if (!string.IsNullOrEmpty(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)) && mapping.ContainsKey(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)!))
                        item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, mapping[item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)!]);

                    // Update cloneof
                    if (!string.IsNullOrEmpty(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey)) && mapping.ContainsKey(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey)!))
                        item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.CloneOfKey, mapping[item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey)!]);

                    // Update romof
                    if (!string.IsNullOrEmpty(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.RomOfKey)) && mapping.ContainsKey(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.RomOfKey)!))
                        item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, mapping[item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.RomOfKey)!]);

                    // Update sampleof
                    if (!string.IsNullOrEmpty(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.SampleOfKey)) && mapping.ContainsKey(item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.SampleOfKey)!))
                        item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.SampleOfKey, mapping[item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.SampleOfKey)!]);

                    // Add the new item to the output list
                    newItems.Add(item);
                }

                // Replace the old list of roms with the new one
                Remove(key);
                Add(key, newItems);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        #endregion

        // TODO: All internal, can this be put into a better location?
        #region Splitting

        /// <summary>
        /// Use romof tags to add roms to the children
        /// </summary>
        internal void AddRomsFromBios()
        {
            List<string> games = [.. Keys];
            games.Sort();

            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                var items = this[game];
                if (items == null || items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetFieldValue<Machine>(DatItem.MachineKey);
                if (machine == null)
                    continue;

                // Get the bios parent
                string? romOf = machine.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                if (string.IsNullOrEmpty(romOf))
                    continue;

                // If the parent doesn't have any items, we want to continue
                var parentItems = this[romOf!];
                if (parentItems == null || parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = items[0];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (items.FindIndex(i => i.GetName() == datItem.GetName()) == -1 && !items.Contains(datItem))
                        Add(game, datItem);
                }
            }
        }

        /// <summary>
        /// Use device_ref and optionally slotoption tags to add roms to the children
        /// </summary>
        /// <param name="dev">True if only child device sets are touched, false for non-device sets (default)</param>
        /// <param name="useSlotOptions">True if slotoptions tags are used as well, false otherwise</param>
        internal bool AddRomsFromDevices(bool dev, bool useSlotOptions)
        {
            bool foundnew = false;
            List<string> machines = [.. Keys];
            machines.Sort();

            foreach (string machine in machines)
            {
                // If the machine doesn't have items, we continue
                var datItems = this[machine];
                if (datItems == null || datItems.Count == 0)
                    continue;

                // If the machine (is/is not) a device, we want to continue
                if (dev ^ (datItems[0].GetFieldValue<Machine>(DatItem.MachineKey)!.GetBoolFieldValue(Models.Metadata.Machine.IsDeviceKey) == true))
                    continue;

                // Get all device reference names from the current machine
                List<string?> deviceReferences = datItems
                    .FindAll(i => i is DeviceRef)
                    .ConvertAll(i => i as DeviceRef)
                    .ConvertAll(dr => dr!.GetName())
                    .Distinct()
                    .ToList();

                // Get all slot option names from the current machine
                List<string?> slotOptions = datItems
                    .FindAll(i => i is Slot)
                    .ConvertAll(i => i as Slot)
                    .FindAll(s => s!.SlotOptionsSpecified)
                    .SelectMany(s => s!.GetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey)!)
                    .Select(so => so.GetStringFieldValue(Models.Metadata.SlotOption.DevNameKey))
                    .Distinct()
                    .ToList();

                // If we're checking device references
                if (deviceReferences.Count > 0)
                {
                    // Loop through all names and check the corresponding machines
                    var newDeviceReferences = new HashSet<string>();
                    foreach (string? deviceReference in deviceReferences)
                    {
                        // If the device reference is missing
                        if (string.IsNullOrEmpty(deviceReference))
                            continue;

                        // Add to the list of new device reference names
                        var devItems = this[deviceReference!];
                        if (devItems == null || devItems.Count == 0)
                            continue;

                        newDeviceReferences.UnionWith(devItems
                            .FindAll(i => i is DeviceRef)
                            .ConvertAll(i => (i as DeviceRef)!.GetName()!));

                        // Set new machine information and add to the current machine
                        DatItem copyFrom = datItems[0];
                        foreach (DatItem item in devItems)
                        {
                            // If the parent machine doesn't already contain this item, add it
                            if (!datItems.Exists(i => i.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) == item.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) && i.GetName() == item.GetName()))
                            {
                                // Set that we found new items
                                foundnew = true;

                                // Clone the item and then add it
                                DatItem datItem = (DatItem)item.Clone();
                                datItem.CopyMachineInformation(copyFrom);
                                Add(machine, datItem);
                            }
                        }
                    }

                    // Now that every device reference is accounted for, add the new list of device references, if they don't already exist
                    foreach (string deviceReference in newDeviceReferences)
                    {
                        if (!deviceReferences.Contains(deviceReference))
                        {
                            var deviceRef = new DeviceRef();
                            deviceRef.SetName(deviceReference);
                            datItems.Add(deviceRef);
                        }
                    }
                }

                // If we're checking slotoptions
                if (useSlotOptions && slotOptions.Count > 0)
                {
                    // Loop through all names and check the corresponding machines
                    var newSlotOptions = new HashSet<string>();
                    foreach (string? slotOption in slotOptions)
                    {
                        // If the slot option is missing
                        if (string.IsNullOrEmpty(slotOption))
                            // If the machine doesn't exist then we continue
                            continue;

                        // Add to the list of new slot option names
                        var slotItems = this[slotOption!];
                        if (slotItems == null || slotItems.Count == 0)
                            continue;

                        newSlotOptions.UnionWith(slotItems
                            .FindAll(i => i is Slot)
                            .FindAll(s => (s as Slot)!.SlotOptionsSpecified)
                            .SelectMany(s => (s as Slot)!.GetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey)!)
                            .Select(o => o.GetStringFieldValue(Models.Metadata.SlotOption.DevNameKey)!));

                        // Set new machine information and add to the current machine
                        DatItem copyFrom = datItems[0];
                        foreach (DatItem item in slotItems)
                        {
                            // If the parent machine doesn't already contain this item, add it
                            if (!datItems.Exists(i => i.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) == item.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) && i.GetName() == item.GetName()))
                            {
                                // Set that we found new items
                                foundnew = true;

                                // Clone the item and then add it
                                DatItem datItem = (DatItem)item.Clone();
                                datItem.CopyMachineInformation(copyFrom);
                                Add(machine, datItem);
                            }
                        }
                    }

                    // Now that every device is accounted for, add the new list of slot options, if they don't already exist
                    foreach (string slotOption in newSlotOptions)
                    {
                        if (!slotOptions.Contains(slotOption))
                        {
                            var slotOptionItem = new SlotOption();
                            slotOptionItem.SetFieldValue<string?>(Models.Metadata.SlotOption.DevNameKey, slotOption);

                            var slotItem = new Slot();
                            slotItem.SetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey, [slotOptionItem]);

                            datItems.Add(slotItem);
                        }
                    }
                }
            }

            return foundnew;
        }

        /// <summary>
        /// Use cloneof tags to add roms to the children, setting the new romof tag in the process
        /// </summary>
        internal void AddRomsFromParent()
        {
            List<string> games = [.. Keys];
            games.Sort();

            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                var items = this[game];
                if (items == null || items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetFieldValue<Machine>(DatItem.MachineKey);
                if (machine == null)
                    continue;

                // Get the clone parent
                string? cloneOf = machine.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey);
                if (string.IsNullOrEmpty(cloneOf))
                    continue;

                // If the parent doesn't have any items, we want to continue
                var parentItems = this[cloneOf!];
                if (parentItems == null || parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = items[0];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (items.FindIndex(i => string.Equals(i.GetName(), datItem.GetName(), StringComparison.OrdinalIgnoreCase)) == -1
                        && !items.Contains(datItem))
                    {
                        Add(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the items
                items = this[game];
                string? romof = this[cloneOf!]![0].GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                foreach (DatItem item in items!)
                {
                    item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, romof);
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to add roms to the parents, removing the child sets in the process
        /// </summary>
        /// <param name="subfolder">True to add DatItems to subfolder of parent (not including Disk), false otherwise</param>
        /// <param name="skipDedup">True to skip checking for duplicate ROMs in parent, false otherwise</param>
        internal void AddRomsFromChildren(bool subfolder, bool skipDedup)
        {
            List<string> games = [.. Keys];
            games.Sort();

            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                var items = this[game];
                if (items == null || items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetFieldValue<Machine>(DatItem.MachineKey);
                if (machine == null)
                    continue;

                // Get the clone parent
                string? cloneOf = machine.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey);
                if (string.IsNullOrEmpty(cloneOf))
                    continue;

                // Get the parent items
                var parentItems = this[cloneOf!];

                // Otherwise, move the items from the current game to a subfolder of the parent game
                DatItem copyFrom;
                if (parentItems == null || parentItems.Count == 0)
                {
                    copyFrom = new Rom();
                    copyFrom.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, cloneOf);
                    copyFrom.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, cloneOf);
                }
                else
                {
                    copyFrom = parentItems[0];
                }

                items = this[game];
                foreach (DatItem item in items!)
                {
                    // Special disk handling
                    if (item is Disk disk)
                    {
                        string? mergeTag = disk.GetStringFieldValue(Models.Metadata.Disk.MergeKey);

                        // If the merge tag exists and the parent already contains it, skip
                        if (mergeTag != null && this[cloneOf!]!
                            .FindAll(i => i is Disk)
                            .ConvertAll(i => (i as Disk)!.GetName()).Contains(mergeTag))
                        {
                            continue;
                        }

                        // If the merge tag exists but the parent doesn't contain it, add to parent
                        else if (mergeTag != null && !this[cloneOf!]!
                            .FindAll(i => i is Disk)
                            .ConvertAll(i => (i as Disk)!.GetName()).Contains(mergeTag))
                        {
                            disk.CopyMachineInformation(copyFrom);
                            Add(cloneOf!, disk);
                        }

                        // If there is no merge tag, add to parent
                        else if (mergeTag == null)
                        {
                            disk.CopyMachineInformation(copyFrom);
                            Add(cloneOf!, disk);
                        }
                    }

                    // Special rom handling
                    else if (item is Rom rom)
                    {
                        // If the merge tag exists and the parent already contains it, skip
                        if (rom.GetStringFieldValue(Models.Metadata.Rom.MergeKey) != null && this[cloneOf!]!
                            .FindAll(i => i is Rom)
                            .ConvertAll(i => (i as Rom)!.GetName())
                            .Contains(rom.GetStringFieldValue(Models.Metadata.Rom.MergeKey)))
                        {
                            continue;
                        }

                        // If the merge tag exists but the parent doesn't contain it, add to subfolder of parent
                        else if (rom.GetStringFieldValue(Models.Metadata.Rom.MergeKey) != null && !this[cloneOf!]!
                            .FindAll(i => i is Rom)
                            .ConvertAll(i => (i as Rom)!.GetName())
                            .Contains(rom.GetStringFieldValue(Models.Metadata.Rom.MergeKey)))
                        {
                            if (subfolder)
                                rom.SetName($"{rom.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)}\\{rom.GetName()}");

                            rom.CopyMachineInformation(copyFrom);
                            Add(cloneOf!, rom);
                        }

                        // If the parent doesn't already contain this item, add to subfolder of parent
                        else if (!this[cloneOf!]!.Contains(item) || skipDedup)
                        {
                            if (subfolder)
                                rom.SetName($"{item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)}\\{rom.GetName()}");

                            rom.CopyMachineInformation(copyFrom);
                            Add(cloneOf!, rom);
                        }
                    }

                    // All other that would be missing to subfolder of parent
                    else if (!this[cloneOf!]!.Contains(item))
                    {
                        if (subfolder)
                            item.SetName($"{item.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey)}\\{item.GetName()}");

                        item.CopyMachineInformation(copyFrom);
                        Add(cloneOf!, item);
                    }
                }

                // Then, remove the old game so it's not picked up by the writer
                Remove(game);
            }
        }

        /// <summary>
        /// Remove all BIOS and device sets
        /// </summary>
        internal void RemoveBiosAndDeviceSets()
        {
            List<string> games = [.. Keys];
            games.Sort();

            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                var items = this[game];
                if (items == null || items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetFieldValue<Machine>(DatItem.MachineKey);
                if (machine == null)
                    continue;

                // Remove flagged items
                if ((machine.GetBoolFieldValue(Models.Metadata.Machine.IsBiosKey) == true)
                    || (machine.GetBoolFieldValue(Models.Metadata.Machine.IsDeviceKey) == true))
                {
                    Remove(game);
                }
            }
        }

        /// <summary>
        /// Use romof tags to remove bios roms from children
        /// </summary>
        /// <param name="bios">True if only child Bios sets are touched, false for non-bios sets</param>
        internal void RemoveBiosRomsFromChild(bool bios)
        {
            // Loop through the romof tags
            List<string> games = [.. Keys];
            games.Sort();

            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                var items = this[game];
                if (items == null || items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetFieldValue<Machine>(DatItem.MachineKey);
                if (machine == null)
                    continue;

                // If the game (is/is not) a bios, we want to continue
                if (bios ^ (machine.GetBoolFieldValue(Models.Metadata.Machine.IsBiosKey) == true))
                    continue;

                // Get the bios parent
                string? romOf = machine.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                if (string.IsNullOrEmpty(romOf))
                    continue;

                // If the parent doesn't have any items, we want to continue
                var parentItems = this[romOf!];
                if (parentItems == null || parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we remove the items that are in the parent from the current game
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (items.Contains(datItem))
                    {
                        Remove(game, datItem);
                    }
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to remove roms from the children
        /// </summary>
        internal void RemoveRomsFromChild()
        {
            List<string> games = [.. Keys];
            games.Sort();

            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                var items = this[game];
                if (items == null || items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetFieldValue<Machine>(DatItem.MachineKey);
                if (machine == null)
                    continue;

                // Get the clone parent
                string? cloneOf = machine.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey);
                if (string.IsNullOrEmpty(cloneOf))
                    continue;

                // If the parent doesn't have any items, we want to continue
                var parentItems = this[cloneOf!];
                if (parentItems == null || parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we remove the parent items from the current game
                foreach (DatItem item in parentItems!)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (items.Contains(datItem))
                    {
                        Remove(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the remaining items
                items = this[game];
                string? romof = this[cloneOf!]![0].GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                foreach (DatItem item in items!)
                {
                    item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, romof);
                }
            }
        }

        /// <summary>
        /// Remove all romof and cloneof tags from all games
        /// </summary>
        internal void RemoveTagsFromChild()
        {
            List<string> games = [.. Keys];
            games.Sort();

            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                var items = this[game];
                if (items == null || items.Count == 0)
                    continue;

                foreach (DatItem item in items)
                {
                    item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.CloneOfKey, null);
                    item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, null);
                    item.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.SampleOfKey, null);
                }
            }
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Recalculate the statistics for the Dat
        /// </summary>
        public void RecalculateStats()
        {
            // Wipe out any stats already there
            DatStatistics.ResetStatistics();

            // If we have a blank Dat in any way, return
            if (_items == null)
                return;

            // Loop through and add
            foreach (string key in _items.Keys)
            {
                List<DatItem>? datItems = _items[key];
                if (datItems == null)
                    continue;

                foreach (DatItem item in datItems)
                {
                    DatStatistics.AddItemStatistics(item);
                }
            }
        }

        #endregion

        #region IDictionary Implementations

        public ICollection<List<DatItem>?> Values => ((IDictionary<string, List<DatItem>?>)_items).Values;

        public int Count => ((ICollection<KeyValuePair<string, List<DatItem>?>>)_items).Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<string, List<DatItem>?>>)_items).IsReadOnly;

        public bool TryGetValue(string key, out List<DatItem>? value)
        {
            return ((IDictionary<string, List<DatItem>?>)_items).TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<string, List<DatItem>?> item)
        {
            ((ICollection<KeyValuePair<string, List<DatItem>?>>)_items).Add(item);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<string, List<DatItem>?>>)_items).Clear();
        }

        public bool Contains(KeyValuePair<string, List<DatItem>?> item)
        {
            return ((ICollection<KeyValuePair<string, List<DatItem>?>>)_items).Contains(item);
        }

        public void CopyTo(KeyValuePair<string, List<DatItem>?>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, List<DatItem>?>>)_items).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, List<DatItem>?> item)
        {
            return ((ICollection<KeyValuePair<string, List<DatItem>?>>)_items).Remove(item);
        }

        public IEnumerator<KeyValuePair<string, List<DatItem>?>> GetEnumerator()
        {
            return ((IEnumerable<KeyValuePair<string, List<DatItem>?>>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        #endregion
    }
}
