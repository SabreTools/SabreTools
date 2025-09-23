#if NET40_OR_GREATER || NETCOREAPP
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.IO;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif
using System.Xml.Serialization;
using Newtonsoft.Json;
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
    public class ItemDictionary
    {
        #region Private instance variables

        /// <summary>
        /// Determine the bucketing key for all items
        /// </summary>
        private ItemKey _bucketedBy = ItemKey.NULL;

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

        #region Fields

        /// <summary>
        /// Get the keys in sorted order from the file dictionary
        /// </summary>
        /// <returns>List of the keys in sorted order</returns>
        [JsonIgnore, XmlIgnore]
        public string[] SortedKeys
        {
            get
            {
                List<string> keys = [.. _items.Keys];
                keys.Sort(new NaturalComparer());
                return [.. keys];
            }
        }

        /// <summary>
        /// DAT statistics
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public DatStatistics DatStatistics { get; } = new DatStatistics();

        #endregion

        #region Constructors

        /// <summary>
        /// Generic constructor
        /// </summary>
        public ItemDictionary()
        {
            _logger = new Logger(this);
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Add a DatItem to the dictionary after checking
        /// </summary>
        /// <param name="item">Item data to check against</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <returns>The key for the item</returns>
        public string AddItem(DatItem item, bool statsOnly)
        {
            string key;

            // If we have a Disk, File, Media, or Rom, clean the hash data
            if (item is Disk disk)
            {
                // If the file has aboslutely no hashes, skip and log
                if (disk.GetStringFieldValue(Models.Metadata.Disk.StatusKey).AsItemStatus() != ItemStatus.Nodump
                    && string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.MD5Key))
                    && string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key)))
                {
                    _logger.Verbose($"Incomplete entry for '{disk.GetName()}' will be output as nodump");
                    disk.SetFieldValue<string?>(Models.Metadata.Disk.StatusKey, ItemStatus.Nodump.AsStringValue());
                }

                item = disk;
            }
            else if (item is DatItems.Formats.File file)
            {
                // If the file has aboslutely no hashes, skip and log
                if (string.IsNullOrEmpty(file.CRC)
                    && string.IsNullOrEmpty(file.MD5)
                    && string.IsNullOrEmpty(file.SHA1)
                    && string.IsNullOrEmpty(file.SHA256))
                {
                    _logger.Verbose($"Incomplete entry for '{file.GetName()}' will be output as nodump");
                }

                item = file;
            }
            else if (item is Media media)
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
                if (size == null && !string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key)))
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
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.RIPEMD128Key, null); // ZeroHash.GetString(HashType.RIPEMD128)
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.RIPEMD160Key, null); // ZeroHash.GetString(HashType.RIPEMD160)
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA1Key, ZeroHash.SHA1Str);
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA256Key, null); // ZeroHash.SHA256Str;
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA384Key, null); // ZeroHash.SHA384Str;
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA512Key, null); // ZeroHash.SHA512Str;
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SpamSumKey, null); // ZeroHash.SpamSumStr;
                }

                // If the file has no size and it's not the above case, skip and log
                else if (rom.GetStringFieldValue(Models.Metadata.Rom.StatusKey).AsItemStatus() != ItemStatus.Nodump && (size == 0 || size == null))
                {
                    //logger.Verbose($"{Header.GetStringFieldValue(DatHeader.FileNameKey)}: Incomplete entry for '{rom.GetName()}' will be output as nodump");
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.StatusKey, ItemStatus.Nodump.AsStringValue());
                }

                // If the file has a size but aboslutely no hashes, skip and log
                else if (rom.GetStringFieldValue(Models.Metadata.Rom.StatusKey).AsItemStatus() != ItemStatus.Nodump
                    && size != null && size > 0
                    && !rom.HasHashes())
                {
                    //logger.Verbose($"{Header.GetStringFieldValue(DatHeader.FileNameKey)}: Incomplete entry for '{rom.GetName()}' will be output as nodump");
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.StatusKey, ItemStatus.Nodump.AsStringValue());
                }

                item = rom;
            }

            // Get the key and add the file
            key = GetBucketKey(item, _bucketedBy, lower: true, norename: true);

            // If only adding statistics, we add an empty key for games and then just item stats
            if (statsOnly)
            {
                EnsureBucketingKey(key);
                DatStatistics.AddItemStatistics(item);
            }
            else
            {
                AddItem(key, item);
            }

            return key;
        }

        /// <summary>
        /// Remove all items marked for removal
        /// </summary>
        public void ClearMarked()
        {
            string[] keys = [.. SortedKeys];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(keys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(keys, key =>
#else
            foreach (var key in keys)
#endif
            {
                var list = GetItemsForBucket(key, filter: true);
                RemoveBucket(key);
                list.ForEach(item => AddItem(key, item));
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Get the items associated with a bucket name
        /// </summary>
        /// <param name="bucketName">Name of the bucket to retrive items for</param>
        /// <param name="filter">Indicates if RemoveKey filtering is performed</param>
        /// <returns>List representing the bucket items, empty on missing</returns>
        public List<DatItem> GetItemsForBucket(string? bucketName, bool filter = false)
        {
            if (bucketName == null)
                return [];

#if NET40_OR_GREATER || NETCOREAPP
            if (!_items.TryGetValue(bucketName, out var items))
                return [];
#else
            if (!_items.ContainsKey(bucketName))
                return [];

            var items = _items[bucketName];
#endif

            if (items == null || !filter)
                return [.. items ?? []];

            var datItems = new List<DatItem>();
            foreach (DatItem item in items)
            {
                if (item.GetBoolFieldValue(DatItem.RemoveKey) != true)
                    datItems.Add(item);
            }

            return datItems;
        }

        /// <summary>
        /// Remove a key from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove</param>
        public bool RemoveBucket(string key)
        {
#if NET40_OR_GREATER || NETCOREAPP
            bool removed = _items.TryRemove(key, out var list);
#else
            if (!_items.ContainsKey(key))
                return false;

            bool removed = true;
            var list = _items[key];
            _items.Remove(key);
#endif
            if (list == null)
                return removed;

            foreach (var item in list)
            {
                DatStatistics.RemoveItemStatistics(item);
            }

            return removed;
        }

        /// <summary>
        /// Remove the indexed instance of a value from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove from</param>
        /// <param name="value">Value to remove from the dictionary</param>
        /// <param name="index">Index of the item to be removed</param>
        public bool RemoveItem(string key, DatItem value, int index)
        {
            // Explicit lock for some weird corner cases
            lock (key)
            {
                // If the key doesn't exist, return
#if NET40_OR_GREATER || NETCOREAPP
                if (!_items.TryGetValue(key, out var list) || list == null)
                    return false;
#else
                if (!_items.ContainsKey(key))
                    return false;

                var list = _items[key];
                if (list == null)
                    return false;
#endif

                // If the value doesn't exist in the key, assume it has been removed
                if (index < 0)
                    return false;

                // Remove the statistics first
                DatStatistics.RemoveItemStatistics(value);

                list.RemoveAt(index);
                return true;
            }
        }

        /// <summary>
        /// Override the internal ItemKey value
        /// </summary>
        /// <param name="newBucket"></param>
        public void SetBucketedBy(ItemKey newBucket)
        {
            _bucketedBy = newBucket;
        }

        /// <summary>
        /// Add a value to the file dictionary
        /// </summary>
        /// <param name="key">Key in the dictionary to add to</param>
        /// <param name="value">Value to add to the dictionary</param>
        internal void AddItem(string key, DatItem value)
        {
            // Explicit lock for some weird corner cases
            lock (key)
            {
                // Ensure the key exists
                EnsureBucketingKey(key);

                // If item is null, don't add it
                if (value == null)
                    return;

                // Now add the value
                _items[key]!.Add(value);

                // Now update the statistics
                DatStatistics.AddItemStatistics(value);
            }
        }

        #endregion

        #region Bucketing

        /// <summary>
        /// Take the arbitrarily bucketed Files Dictionary and convert to one bucketed by a user-defined method
        /// </summary>
        /// <param name="bucketBy">ItemKey enum representing how to bucket the individual items</param>
        /// <param name="lower">True if the key should be lowercased (default), false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        public void BucketBy(ItemKey bucketBy, bool lower = true, bool norename = true)
        {
            // If we have a situation where there's no dictionary or no keys at all, we skip
            if (_items == null || _items.Count == 0)
                return;

            // If the sorted type isn't the same, we want to sort the dictionary accordingly
            if (_bucketedBy != bucketBy && bucketBy != ItemKey.NULL)
            {
                _logger.User($"Organizing roms by {bucketBy}");
                PerformBucketing(bucketBy, lower, norename);
            }

            // Sort the dictionary to be consistent
            _logger.User($"Sorting roms by {bucketBy}");
            PerformSorting(norename);
        }

        /// <summary>
        /// Perform deduplication on the current sorted dictionary
        /// </summary>
        public void Deduplicate()
        {
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(SortedKeys, key =>
#else
            foreach (var key in SortedKeys)
#endif
            {
                // Get the possibly unsorted list
                List<DatItem> sortedList = GetItemsForBucket(key);

                // Sort and merge the list
                Sort(ref sortedList, norename: false);
                sortedList = Merge(sortedList);

                // Add the list back to the dictionary
                RemoveBucket(key);
                sortedList.ForEach(item => AddItem(key, item));
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Return the duplicate status of two items
        /// </summary>
        /// <param name="self">Current DatItem</param>
        /// <param name="last">DatItem to check against</param>
        /// <returns>The DupeType corresponding to the relationship between the two</returns>
        public DupeType GetDuplicateStatus(DatItem? self, DatItem? last)
        {
            DupeType output = 0x00;

            // If either item is null
            if (self == null || last == null)
                return output;

            // If we don't have a duplicate at all, return none
            if (!self.Equals(last))
                return output;

            // Get the sources for comparison
            var selfSource = self.GetFieldValue<Source?>(DatItem.SourceKey);
            var lastSource = last.GetFieldValue<Source?>(DatItem.SourceKey);

            // Get the machines for comparison
            var selfMachine = self.GetMachine();
            string? selfMachineName = selfMachine?.GetName();
            var lastMachine = last.GetMachine();
            string? lastMachineName = lastMachine?.GetName();

            // If the duplicate is external already
#if NET20 || NET35
            if ((last.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.External) != 0)
#else
            if (last.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.External))
#endif
                output |= DupeType.External;

            // If the duplicate should be external
            else if (lastSource?.Index != selfSource?.Index)
                output |= DupeType.External;

            // Otherwise, it's considered an internal dupe
            else
                output |= DupeType.Internal;

            // If the item and machine names match
            if (lastMachineName == selfMachineName && last.GetName() == self.GetName())
                output |= DupeType.All;

            // Otherwise, hash match is assumed
            else
                output |= DupeType.Hash;

            return output;
        }

        /// <summary>
        /// Merge an arbitrary set of DatItems based on the supplied information
        /// </summary>
        /// <param name="items">List of DatItem objects representing the items to be merged</param>
        /// <returns>A List of DatItem objects representing the merged items</returns>
        /// TODO: Make this internal like the DB counterpart
        public static List<DatItem> Merge(List<DatItem>? items)
        {
            // Check for null or blank inputs first
            if (items == null || items.Count == 0)
                return [];

            // Create placeholder object for checking duplicates
            var dupDict = new ItemDictionary();

            // Create output list
            List<DatItem> output = [];

            // Then deduplicate them by checking to see if data matches previous saved roms
            int nodumpCount = 0;
            foreach (DatItem datItem in items)
            {
                // If we don't have a Disk, File, Media, or Rom, we skip checking for duplicates
                if (datItem is not Disk && datItem is not DatItems.Formats.File && datItem is not Media && datItem is not Rom)
                    continue;

                // If it's a nodump, add and skip
                if (datItem is Rom rom && rom.GetStringFieldValue(Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Nodump)
                {
                    output.Add(datItem);
                    nodumpCount++;
                    continue;
                }
                else if (datItem is Disk disk && disk.GetStringFieldValue(Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Nodump)
                {
                    output.Add(datItem);
                    nodumpCount++;
                    continue;
                }

                // If it's the first non-nodump item in the list, don't touch it
                if (output.Count == nodumpCount)
                {
                    output.Add(datItem);
                    continue;
                }

                // Find the index of the first duplicate, if one exists
                int pos = output.FindIndex(lastItem => dupDict.GetDuplicateStatus(datItem, lastItem) != 0x00);
                if (pos < 0)
                {
                    output.Add(datItem);
                    continue;
                }

                // Get the duplicate item
                DatItem savedItem = output[pos];
                DupeType dupetype = dupDict.GetDuplicateStatus(datItem, savedItem);

                // Disks, File, Media, and Roms have more information to fill
                if (datItem is Disk diskItem && savedItem is Disk savedDisk)
                    savedDisk.FillMissingInformation(diskItem);
                else if (datItem is DatItems.Formats.File fileItem && savedItem is DatItems.Formats.File savedFile)
                    savedFile.FillMissingInformation(fileItem);
                else if (datItem is Media mediaItem && savedItem is Media savedMedia)
                    savedMedia.FillMissingInformation(mediaItem);
                else if (datItem is Rom romItem && savedItem is Rom savedRom)
                    savedRom.FillMissingInformation(romItem);

                // Set the duplicate type on the saved item
                savedItem.SetFieldValue<DupeType>(DatItem.DupeTypeKey, dupetype);

                // Get the sources associated with the items
                var savedSource = savedItem.GetFieldValue<Source?>(DatItem.SourceKey);
                var itemSource = datItem.GetFieldValue<Source?>(DatItem.SourceKey);

                // Get the machines associated with the items
                var savedMachine = savedItem.GetMachine();
                var itemMachine = datItem.GetMachine();

                // If the current source has a lower ID than the saved, use the saved source
                if (itemSource?.Index < savedSource?.Index)
                {
                    datItem.SetFieldValue<Source?>(DatItem.SourceKey, savedSource.Clone() as Source);
                    savedItem.CopyMachineInformation(datItem);
                    savedItem.SetName(datItem.GetName());
                }

                // If the saved machine is a child of the current machine, use the current machine instead
                if (savedMachine?.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey) == itemMachine?.GetName()
                    || savedMachine?.GetStringFieldValue(Models.Metadata.Machine.RomOfKey) == itemMachine?.GetName())
                {
                    savedItem.CopyMachineInformation(datItem);
                    savedItem.SetName(datItem.GetName());
                }

                // Replace the original item in the list
                output.RemoveAt(pos);
                output.Insert(pos, savedItem);
            }

            // Then return the result
            return output;
        }

        /// <summary>
        /// List all duplicates found in a DAT based on a DatItem
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>List of matched DatItem objects</returns>
        /// <remarks>This also sets the remove flag on any duplicates found</remarks>
        /// TODO: Figure out if removal should be a flag or just removed entirely
        internal List<DatItem> GetDuplicates(DatItem datItem, bool sorted = false)
        {
            // Check for an empty rom list first
            if (DatStatistics.TotalCount == 0)
                return [];

            // We want to get the proper key for the DatItem
            string key = SortAndGetKey(datItem, sorted);

            // Get the items for the current key, if possible
            List<DatItem> items = GetItemsForBucket(key, filter: false);
            if (items.Count == 0)
                return [];

            // Try to find duplicates
            List<DatItem> output = [];
            foreach (DatItem other in items)
            {
                // Skip items marked for removal
                if (other.GetBoolFieldValue(DatItem.RemoveKey) == true)
                    continue;

                // Mark duplicates for future removal
                if (datItem.Equals(other))
                {
                    other.SetFieldValue<bool?>(DatItem.RemoveKey, true);
                    output.Add(other);
                }
            }

            // Return any matching items
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

            // Try to find duplicates
            List<DatItem> roms = GetItemsForBucket(key);
            if (roms.Count == 0)
                return false;

            return roms.FindIndex(datItem.Equals) > -1;
        }

        /// <summary>
        /// Ensure the key exists in the items dictionary
        /// </summary>
        /// <param name="key">Key to ensure</param>
        private void EnsureBucketingKey(string key)
        {
            // If the key is missing from the dictionary, add it
#if NET40_OR_GREATER || NETCOREAPP
            _items.GetOrAdd(key, []);
#else
            if (!_items.ContainsKey(key))
                _items[key] = [];
#endif
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

            // If all items are supposed to have a RIPEMD160, we bucket by that
            else if (diskCount + mediaCount + romCount - nodumpCount == DatStatistics.GetHashCount(HashType.RIPEMD160))
                return ItemKey.RIPEMD160;

            // If all items are supposed to have a RIPEMD128, we bucket by that
            else if (diskCount + mediaCount + romCount - nodumpCount == DatStatistics.GetHashCount(HashType.RIPEMD128))
                return ItemKey.RIPEMD128;

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
        /// Get the bucketing key for a given item
        /// <param name="datItem">The current item</param>
        /// <param name="bucketBy">ItemKey value representing what key to get</param>
        /// <param name="lower">True if the key should be lowercased, false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        /// </summary>
        private static string GetBucketKey(DatItem datItem, ItemKey bucketBy, bool lower, bool norename)
        {
            if (datItem == null)
                return string.Empty;

            // Treat NULL like machine
            if (bucketBy == ItemKey.NULL)
                bucketBy = ItemKey.Machine;

            // Get the machine and source
            var machine = datItem.GetMachine();
            var source = datItem.GetFieldValue<Source?>(DatItem.SourceKey);

            // Get the bucket key
            return datItem.GetKey(bucketBy, machine, source, lower, norename);
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
            _bucketedBy = bucketBy;

            // First do the initial sort of all of the roms inplace
            List<string> oldkeys = [.. SortedKeys];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, oldkeys.Count, Core.Globals.ParallelOptions, k =>
#elif NET40_OR_GREATER
            Parallel.For(0, oldkeys.Count, k =>
#else
            for (int k = 0; k < oldkeys.Count; k++)
#endif
            {
                string key = oldkeys[k];
                if (GetItemsForBucket(key, filter: true).Count == 0)
                    RemoveBucket(key);

                // Now add each of the roms to their respective keys
                for (int i = 0; i < GetItemsForBucket(key).Count; i++)
                {
                    DatItem item = GetItemsForBucket(key)[i];
                    if (item == null || item.GetBoolFieldValue(DatItem.RemoveKey) == true)
                        continue;

                    // Get the machine and source
                    var machine = item.GetMachine();
                    var source = item.GetFieldValue<Source?>(DatItem.SourceKey);

                    // We want to get the key most appropriate for the given sorting type
                    string newkey = item.GetKey(bucketBy, machine, source, lower, norename);

                    // If the key is different, move the item to the new key
                    if (newkey != key)
                    {
                        AddItem(newkey, item);
                        bool removed = RemoveItem(key, item, i);
                        if (!removed)
                            continue;

                        i--; // This make sure that the pointer stays on the correct since one was removed
                    }
                }

                // If the key is now empty, remove it
                if (GetItemsForBucket(key, filter: true).Count == 0)
                    RemoveBucket(key);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Perform inplace sorting of the dictionary
        /// </summary>
        private void PerformSorting(bool norename)
        {
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(SortedKeys, key =>
#else
            foreach (var key in SortedKeys)
#endif
            {
                // Get the possibly unsorted list
                List<DatItem> sortedList = GetItemsForBucket(key);

                // Sort the list of items to be consistent
                Sort(ref sortedList, norename);

                // Add the list back to the dictionary
                RemoveBucket(key);
                sortedList.ForEach(item => AddItem(key, item));
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Sort a list of DatItem objects by SourceID, Game, and Name (in order)
        /// </summary>
        /// <param name="items">List of DatItem objects representing the items to be sorted</param>
        /// <param name="norename">True if files are not renamed, false otherwise</param>
        /// <returns>True if it sorted correctly, false otherwise</returns>
        private bool Sort(ref List<DatItem> items, bool norename)
        {
            // Create the comparer extenal to the delegate
            var nc = new NaturalComparer();

            // Sort by machine, type, item name, and source
            items.Sort(delegate (DatItem x, DatItem y)
            {
                try
                {
                    // Compare on source if renaming
                    if (!norename)
                    {
                        int xSourceIndex = x.GetFieldValue<Source?>(DatItem.SourceKey)?.Index ?? 0;
                        int ySourceIndex = y.GetFieldValue<Source?>(DatItem.SourceKey)?.Index ?? 0;
                        if (xSourceIndex != ySourceIndex)
                            return xSourceIndex - ySourceIndex;
                    }

                    // Get the machines
                    Machine? xMachine = x.GetMachine();
                    Machine? yMachine = y.GetMachine();

                    // If machine names don't match
                    string? xMachineName = xMachine?.GetName();
                    string? yMachineName = yMachine?.GetName();
                    if (xMachineName != yMachineName)
                        return nc.Compare(xMachineName, yMachineName);

                    // If types don't match
                    string? xType = x.GetStringFieldValue(Models.Metadata.DatItem.TypeKey);
                    string? yType = y.GetStringFieldValue(Models.Metadata.DatItem.TypeKey);
                    if (xType != yType)
                        return xType.AsItemType() - yType.AsItemType();

                    // If directory names don't match
                    string? xDirectoryName = Path.GetDirectoryName(TextHelper.RemovePathUnsafeCharacters(x.GetName() ?? string.Empty));
                    string? yDirectoryName = Path.GetDirectoryName(TextHelper.RemovePathUnsafeCharacters(y.GetName() ?? string.Empty));
                    if (xDirectoryName != yDirectoryName)
                        return nc.Compare(xDirectoryName, yDirectoryName);

                    // If item names don't match
                    string? xName = Path.GetFileName(TextHelper.RemovePathUnsafeCharacters(x.GetName() ?? string.Empty));
                    string? yName = Path.GetFileName(TextHelper.RemovePathUnsafeCharacters(y.GetName() ?? string.Empty));
                    return nc.Compare(xName, yName);
                }
                catch
                {
                    // Absorb the error
                    return 0;
                }
            });

            return true;
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
                BucketBy(GetBestAvailable());

            // Now that we have the sorted type, we get the proper key
            return GetBucketKey(datItem, _bucketedBy, lower: true, norename: true);
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
            if (_items == null || _items.Count == 0)
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
    }
}
