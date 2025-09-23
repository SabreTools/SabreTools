#if NET40_OR_GREATER || NETCOREAPP
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading;
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

/*
 * Planning Notes:
 * 
 * In order for this in-memory "database" design to work, there need to be a few things:
 * - Feature parity with all existing item dictionary operations
 * - A way to transition between the two item dictionaries (a flag?)
 * - Helper methods that target the "database" version instead of assuming the standard dictionary
 * 
 * Notable changes include:
 * - Separation of Machine from DatItem, leading to a mapping instead
 *      + Should DatItem include an index reference to the machine? Or should that be all external?
 * - Adding machines to the dictionary distinctly from the items
 * - Having a separate "bucketing" system that only reorders indicies and not full items; quicker?
 * - Non-key-based add/remove of values; use explicit methods instead of dictionary-style accessors
*/

namespace SabreTools.DatFiles
{
    /// <summary>
    /// Item dictionary with statistics, bucketing, and sorting
    /// </summary>
    [JsonObject("items"), XmlRoot("items")]
    public class ItemDictionaryDB
    {
        #region Private instance variables

        /// <summary>
        /// Internal dictionary for all items
        /// </summary>
        [JsonIgnore, XmlIgnore]
#if NET40_OR_GREATER || NETCOREAPP
        private readonly ConcurrentDictionary<long, DatItem> _items = [];
#else
        private readonly Dictionary<long, DatItem> _items = [];
#endif

        /// <summary>
        /// Current highest available item index
        /// </summary>
        [JsonIgnore, XmlIgnore]
        private long _itemIndex = 0;

        /// <summary>
        /// Internal dictionary for all machines
        /// </summary>
        [JsonIgnore, XmlIgnore]
#if NET40_OR_GREATER || NETCOREAPP
        private readonly ConcurrentDictionary<long, Machine> _machines = [];
#else
        private readonly Dictionary<long, Machine> _machines = [];
#endif

        /// <summary>
        /// Current highest available machine index
        /// </summary>
        [JsonIgnore, XmlIgnore]
        private long _machineIndex = 0;

        /// <summary>
        /// Internal dictionary for all sources
        /// </summary>
        [JsonIgnore, XmlIgnore]
#if NET40_OR_GREATER || NETCOREAPP
        private readonly ConcurrentDictionary<long, Source> _sources = [];
#else
        private readonly Dictionary<long, Source> _sources = [];
#endif

        /// <summary>
        /// Current highest available source index
        /// </summary>
        [JsonIgnore, XmlIgnore]
        private long _sourceIndex = 0;

        /// <summary>
        /// Internal dictionary for item to machine mappings
        /// </summary>
        [JsonIgnore, XmlIgnore]
#if NET40_OR_GREATER || NETCOREAPP
        private readonly ConcurrentDictionary<long, long> _itemToMachineMapping = [];
#else
        private readonly Dictionary<long, long> _itemToMachineMapping = [];
#endif

        /// <summary>
        /// Internal dictionary for item to source mappings
        /// </summary>
        [JsonIgnore, XmlIgnore]
#if NET40_OR_GREATER || NETCOREAPP
        private readonly ConcurrentDictionary<long, long> _itemToSourceMapping = [];
#else
        private readonly Dictionary<long, long> _itemToSourceMapping = [];
#endif

        /// <summary>
        /// Internal dictionary representing the current buckets
        /// </summary>
        [JsonIgnore, XmlIgnore]
#if NET40_OR_GREATER || NETCOREAPP
        private readonly ConcurrentDictionary<string, List<long>> _buckets = [];
#else
        private readonly Dictionary<string, List<long>> _buckets = [];
#endif

        /// <summary>
        /// Current bucketed by value
        /// </summary>
        private ItemKey _bucketedBy = ItemKey.NULL;

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
                List<string> keys = [.. _buckets.Keys];
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
        public ItemDictionaryDB()
        {
            _logger = new Logger(this);
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Add a DatItem to the dictionary after validation
        /// </summary>
        /// <param name="item">Item data to validate</param>
        /// <param name="machineIndex">Index of the machine related to the item</param>
        /// <param name="sourceIndex">Index of the source related to the item</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <returns>The index for the added item, -1 on error</returns>
        public long AddItem(DatItem item, long machineIndex, long sourceIndex, bool statsOnly)
        {
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

            // If only adding statistics, we add just item stats
            if (statsOnly)
            {
                DatStatistics.AddItemStatistics(item);
                return -1;
            }
            else
            {
                return AddItem(item, machineIndex, sourceIndex);
            }
        }

        /// <summary>
        /// Add a machine, returning the insert index
        /// </summary>
        public long AddMachine(Machine machine)
        {
#if NET40_OR_GREATER || NETCOREAPP
            long index = Interlocked.Increment(ref _machineIndex) - 1;
            _machines.TryAdd(index, machine);
            return index;
#else
            long index = _machineIndex++ - 1;
            _machines[index] = machine;
            return index;
#endif
        }

        /// <summary>
        /// Add a source, returning the insert index
        /// </summary>
        public long AddSource(Source source)
        {
#if NET40_OR_GREATER || NETCOREAPP
            long index = Interlocked.Increment(ref _sourceIndex) - 1;
            _sources.TryAdd(index, source);
            return index;
#else
            long index = _sourceIndex++ - 1;
            _sources[index] = source;
            return index;
#endif
        }

        /// <summary>
        /// Remove all items marked for removal
        /// </summary>
        public void ClearMarked()
        {
            long[] itemIndices = [.. _items.Keys];
            foreach (long itemIndex in itemIndices)
            {
#if NET40_OR_GREATER || NETCOREAPP
                if (!_items.TryGetValue(itemIndex, out var datItem) || datItem == null)
                    continue;
#else
                var datItem = _items[itemIndex];
#endif

                if (datItem.GetBoolFieldValue(DatItem.RemoveKey) != true)
                    continue;

                RemoveItem(itemIndex);
            }
        }

        /// <summary>
        /// Get all items and their indicies
        /// </summary>
        public IDictionary<long, DatItem> GetItems() => _items;

        /// <summary>
        /// Get the indices and items associated with a bucket name
        /// </summary>
        public Dictionary<long, DatItem> GetItemsForBucket(string? bucketName, bool filter = false)
        {
            if (bucketName == null)
                return [];

#if NET40_OR_GREATER || NETCOREAPP
            if (!_buckets.TryGetValue(bucketName, out var itemIds))
                return [];
#else
            if (!_buckets.ContainsKey(bucketName))
                return [];

            var itemIds = _buckets[bucketName];
#endif

            var datItems = new Dictionary<long, DatItem>();
            foreach (long itemId in itemIds)
            {
                // Ignore missing IDs
#if NET40_OR_GREATER || NETCOREAPP
                if (!_items.TryGetValue(itemId, out var datItem) || datItem == null)
                    continue;
#else
                if (!_items.ContainsKey(itemId))
                    continue;

                var datItem = _items[itemId];
                if (datItem == null)
                    continue;
#endif

                if (!filter || datItem.GetBoolFieldValue(DatItem.RemoveKey) != true)
                    datItems[itemId] = datItem;
            }

            return datItems;
        }

        /// <summary>
        /// Get a machine based on the index
        /// </summary>
        public Machine? GetMachine(long index)
        {
#if NET40_OR_GREATER || NETCOREAPP
            if (!_machines.TryGetValue(index, out var machine))
                return null;

            return machine;
#else
            if (!_machines.ContainsKey(index))
                return null;

            return _machines[index];
#endif
        }

        /// <summary>
        /// Get a machine based on the name
        /// </summary>
        /// <remarks>This assume that all machines have unique names</remarks>
        public KeyValuePair<long, Machine?> GetMachine(string? name)
        {
            if (string.IsNullOrEmpty(name))
                return new KeyValuePair<long, Machine?>(-1, null);

            var machine = _machines.FirstOrDefault(m => m.Value.GetName() == name);
            return new KeyValuePair<long, Machine?>(machine.Key, machine.Value);
        }

        /// <summary>
        /// Get the index and machine associated with an item index
        /// </summary>
        public KeyValuePair<long, Machine?> GetMachineForItem(long itemIndex)
        {
#if NET40_OR_GREATER || NETCOREAPP
            if (!_itemToMachineMapping.TryGetValue(itemIndex, out long machineIndex))
                return new KeyValuePair<long, Machine?>(-1, null);

            if (!_machines.TryGetValue(machineIndex, out var machine))
                return new KeyValuePair<long, Machine?>(-1, null);

            return new KeyValuePair<long, Machine?>(machineIndex, machine);
#else
            if (!_itemToMachineMapping.ContainsKey(itemIndex))
                return new KeyValuePair<long, Machine?>(-1, null);

            long machineIndex = _itemToMachineMapping[itemIndex];
            if (!_machines.ContainsKey(machineIndex))
                return new KeyValuePair<long, Machine?>(-1, null);

            var machine = _machines[machineIndex];
            return new KeyValuePair<long, Machine?>(machineIndex, machine);
#endif
        }

        /// <summary>
        /// Get all machines and their indicies
        /// </summary>
        public IDictionary<long, Machine> GetMachines() => _machines;

        /// <summary>
        /// Get a source based on the index
        /// </summary>
        public Source? GetSource(long index)
        {
            if (!_sources.ContainsKey(index))
                return null;

            return _sources[index];
        }

        /// <summary>
        /// Get the index and source associated with an item index
        /// </summary>
        public KeyValuePair<long, Source?> GetSourceForItem(long itemIndex)
        {
#if NET40_OR_GREATER || NETCOREAPP
            if (!_itemToSourceMapping.TryGetValue(itemIndex, out long sourceIndex))
                return new KeyValuePair<long, Source?>(-1, null);

            if (!_sources.TryGetValue(sourceIndex, out var source))
                return new KeyValuePair<long, Source?>(-1, null);

            return new KeyValuePair<long, Source?>(sourceIndex, source);
#else
            if (!_itemToSourceMapping.ContainsKey(itemIndex))
                return new KeyValuePair<long, Source?>(-1, null);

            long sourceIndex = _itemToSourceMapping[itemIndex];
            if (!_sources.ContainsKey(sourceIndex))
                return new KeyValuePair<long, Source?>(-1, null);

            var source = _sources[sourceIndex];
            return new KeyValuePair<long, Source?>(sourceIndex, source);
#endif
        }

        /// <summary>
        /// Get all sources and their indicies
        /// </summary>
        public IDictionary<long, Source> GetSources() => _sources;

        /// <summary>
        /// Remap an item to a new machine index without validation
        /// </summary>
        /// <param name="itemIndex">Current item index</param>
        /// <param name="machineIndex">New machine index</param>
        public void RemapDatItemToMachine(long itemIndex, long machineIndex)
        {
            lock (_itemToMachineMapping)
            {
                _itemToMachineMapping[itemIndex] = machineIndex;
            }
        }

        /// <summary>
        /// Remove a key from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove</param>
        public bool RemoveBucket(string key)
        {
#if NET40_OR_GREATER || NETCOREAPP
            bool removed = _buckets.TryRemove(key, out var list);
#else
            if (!_buckets.ContainsKey(key))
                return false;

            bool removed = true;
            var list = _buckets[key];
            _buckets.Remove(key);
#endif
            if (list == null)
                return removed;

            foreach (var index in list)
            {
#if NET40_OR_GREATER || NETCOREAPP
                if (!_items.TryGetValue(index, out var datItem) || datItem == null)
                    continue;
#else
                if (!_items.ContainsKey(index))
                    continue;

                var datItem = _items[index];
#endif

                RemoveItem(index);
            }

            return removed;
        }

        /// <summary>
        /// Remove an item, returning if it could be removed
        /// </summary>
        public bool RemoveItem(long itemIndex)
        {
            // If the key doesn't exist, return
#if NET40_OR_GREATER || NETCOREAPP
            if (!_items.TryRemove(itemIndex, out var datItem))
                return false;
#else
            if (!_items.ContainsKey(itemIndex))
                return false;

            var datItem = _items[itemIndex];
            _items.Remove(itemIndex);
#endif

            // Remove statistics, if possible
            if (datItem != null)
                DatStatistics.RemoveItemStatistics(datItem);

            // Remove the machine mapping
#if NET40_OR_GREATER || NETCOREAPP
            _itemToMachineMapping.TryRemove(itemIndex, out _);
#else
            if (_itemToMachineMapping.ContainsKey(itemIndex))
                _itemToMachineMapping.Remove(itemIndex);
#endif

            // Remove the source mapping
#if NET40_OR_GREATER || NETCOREAPP
            _itemToSourceMapping.TryRemove(itemIndex, out _);
#else
            if (_itemToSourceMapping.ContainsKey(itemIndex))
                _itemToSourceMapping.Remove(itemIndex);
#endif

            return true;
        }

        /// <summary>
        /// Remove a machine, returning if it could be removed
        /// </summary>
        public bool RemoveMachine(long machineIndex)
        {
            if (!_machines.ContainsKey(machineIndex))
                return false;

#if NET40_OR_GREATER || NETCOREAPP
            _machines.TryRemove(machineIndex, out _);
#else
            _machines.Remove(machineIndex);
#endif

            var itemIds = _itemToMachineMapping
                .Where(mapping => mapping.Value == machineIndex)
                .Select(mapping => mapping.Key);

            foreach (long itemId in itemIds)
            {
#if NET40_OR_GREATER || NETCOREAPP
                _itemToMachineMapping.TryRemove(itemId, out _);
#else
                _itemToMachineMapping.Remove(itemId);
#endif
            }

            return true;
        }

        /// <summary>
        /// Remove a machine, returning if it could be removed
        /// </summary>
        public bool RemoveMachine(string machineName)
        {
            if (string.IsNullOrEmpty(machineName))
                return false;

            var machine = _machines.FirstOrDefault(m => m.Value.GetName() == machineName);
            return RemoveMachine(machine.Key);
        }

        /// <summary>
        /// Add an item, returning the insert index
        /// </summary>
        internal long AddItem(DatItem item, long machineIndex, long sourceIndex)
        {
#if NET40_OR_GREATER || NETCOREAPP
            // Add the item with a new index
            long index = Interlocked.Increment(ref _itemIndex) - 1;
            _items.TryAdd(index, item);

            // Add the machine mapping
            _itemToMachineMapping.TryAdd(index, machineIndex);

            // Add the source mapping
            _itemToSourceMapping.TryAdd(index, sourceIndex);
#else
            // Add the item with a new index
            long index = _itemIndex++ - 1;
            _items[index] = item;

            // Add the machine mapping
            _itemToMachineMapping[index] = machineIndex;

            // Add the source mapping
            _itemToSourceMapping[index] = sourceIndex;
#endif

            // Add the item statistics
            DatStatistics.AddItemStatistics(item);

            // Add the item to the default bucket
            PerformItemBucketing(index, _bucketedBy, lower: true, norename: true);

            // Return the used index
            return index;
        }

        #endregion

        #region Bucketing

        /// <summary>
        /// Update the bucketing dictionary
        /// </summary>
        /// <param name="bucketBy">ItemKey enum representing how to bucket the individual items</param>
        /// <param name="lower">True if the key should be lowercased (default), false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        /// <returns></returns>
        public void BucketBy(ItemKey bucketBy, bool lower = true, bool norename = true)
        {
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
                List<KeyValuePair<long, DatItem>> sortedList = [.. GetItemsForBucket(key)];

                // Sort and merge the list
                Sort(ref sortedList, false);
                sortedList = Merge(sortedList);

                // Get all existing mappings
                List<ItemMappings> currentMappings = sortedList.ConvertAll(item =>
                {
                    return new ItemMappings(
                        item.Value,
                        GetMachineForItem(item.Key).Key,
                        GetSourceForItem(item.Key).Key
                    );
                });

                // Add the list back to the dictionary
                RemoveBucket(key);
                currentMappings.ForEach(map =>
                    AddItem(map.Item, map.MachineId, map.SourceId));
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Return the duplicate status of two items
        /// </summary>
        /// <param name="selfItem">Current DatItem</param>
        /// <param name="selfSource">Source associated with this item</param>
        /// <param name="lastItem">DatItem to check against</param>
        /// <param name="lastSource">Source associated with the last item</param>
        /// <returns>The DupeType corresponding to the relationship between the two</returns>
        public DupeType GetDuplicateStatus(KeyValuePair<long, DatItem>? selfItem, Source? selfSource, KeyValuePair<long, DatItem>? lastItem, Source? lastSource)
        {
            DupeType output = 0x00;

            // If either item is null
            if (selfItem == null || lastItem == null)
                return output;

            // If we don't have a duplicate at all, return none
            if (!selfItem.Value.Value.Equals(lastItem.Value.Value))
                return output;

            // Get the machines for comparison
            var selfMachine = GetMachineForItem(selfItem.Value.Key).Value;
            string? selfMachineName = selfMachine?.GetName();
            var lastMachine = GetMachineForItem(lastItem.Value.Key).Value;
            string? lastMachineName = lastMachine?.GetName();

            // If the duplicate is external already
#if NET20 || NET35
            if ((lastItem.Value.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.External) != 0)
#else
            if (lastItem.Value.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.External))
#endif
                output |= DupeType.External;

            // If the duplicate should be external
            else if (lastSource?.Index != selfSource?.Index)
                output |= DupeType.External;

            // Otherwise, it's considered an internal dupe
            else
                output |= DupeType.Internal;

            // If the item and machine names match
            if (lastMachineName == selfMachineName && lastItem.Value.Value.GetName() == selfItem.Value.Value.GetName())
                output |= DupeType.All;

            // Otherwise, hash match is assumed
            else
                output |= DupeType.Hash;

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
        internal Dictionary<long, DatItem> GetDuplicates(KeyValuePair<long, DatItem> datItem, bool sorted = false)
        {
            // Check for an empty rom list first
            if (DatStatistics.TotalCount == 0)
                return [];

            // We want to get the proper key for the DatItem, ignoring the index
            _ = SortAndGetKey(datItem, sorted);
            var machine = GetMachineForItem(datItem.Key);
            var source = GetSourceForItem(datItem.Key);
            string key = datItem.Value.GetKey(_bucketedBy, machine.Value, source.Value);

            // If the key doesn't exist, return the empty list
            var items = GetItemsForBucket(key);
            if (items.Count == 0)
                return [];

            // Try to find duplicates
            Dictionary<long, DatItem> output = [];
            foreach (var rom in items)
            {
                // Skip items marked for removal
                if (rom.Value.GetBoolFieldValue(DatItem.RemoveKey) == true)
                    continue;

                // Mark duplicates for future removal
                if (datItem.Value.Equals(rom.Value))
                {
                    rom.Value.SetFieldValue<bool?>(DatItem.RemoveKey, true);
                    output[rom.Key] = rom.Value;
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
        internal bool HasDuplicates(KeyValuePair<long, DatItem> datItem, bool sorted = false)
        {
            // Check for an empty rom list first
            if (DatStatistics.TotalCount == 0)
                return false;

            // We want to get the proper key for the DatItem, ignoring the index
            _ = SortAndGetKey(datItem, sorted);
            var machine = GetMachineForItem(datItem.Key);
            var source = GetSourceForItem(datItem.Key);
            string key = datItem.Value.GetKey(_bucketedBy, machine.Value, source.Value);

            // If the key doesn't exist
            var roms = GetItemsForBucket(key);
            if (roms == null || roms.Count == 0)
                return false;

            // Try to find duplicates
            return roms.Values.Any(datItem.Value.Equals);
        }

        /// <summary>
        /// Merge an arbitrary set of item pairs based on the supplied information
        /// </summary>
        /// <param name="itemMappings">List of pairs representing the items to be merged</param>
        private List<KeyValuePair<long, DatItem>> Merge(List<KeyValuePair<long, DatItem>> itemMappings)
        {
            // Check for null or blank roms first
            if (itemMappings == null || itemMappings.Count == 0)
                return [];

            // Create output list
            List<KeyValuePair<long, DatItem>> output = [];

            // Then deduplicate them by checking to see if data matches previous saved roms
            int nodumpCount = 0;
            foreach (var kvp in itemMappings)
            {
                long itemIndex = kvp.Key;
                DatItem datItem = kvp.Value;

                // If we don't have a Disk, File, Media, or Rom, we skip checking for duplicates
                if (datItem is not Disk && datItem is not DatItems.Formats.File && datItem is not Media && datItem is not Rom)
                    continue;

                // If it's a nodump, add and skip
                if (datItem is Rom rom && rom.GetStringFieldValue(Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Nodump)
                {
                    output.Add(new KeyValuePair<long, DatItem>(itemIndex, datItem));
                    nodumpCount++;
                    continue;
                }
                else if (datItem is Disk disk && disk.GetStringFieldValue(Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Nodump)
                {
                    output.Add(new KeyValuePair<long, DatItem>(itemIndex, datItem));
                    nodumpCount++;
                    continue;
                }

                // If it's the first non-nodump rom in the list, don't touch it
                if (output.Count == nodumpCount)
                {
                    output.Add(new KeyValuePair<long, DatItem>(itemIndex, datItem));
                    continue;
                }

                // Find the index of the first duplicate, if one exists
                var datItemSource = GetSourceForItem(itemIndex);
                int pos = output.FindIndex(lastItem =>
                {
                    var lastItemSource = GetSourceForItem(lastItem.Key);
                    return GetDuplicateStatus(kvp, datItemSource.Value, lastItem, lastItemSource.Value) != 0x00;
                });
                if (pos < 0)
                {
                    output.Add(new KeyValuePair<long, DatItem>(itemIndex, datItem));
                    continue;
                }

                // Get the duplicate item
                long savedIndex = output[pos].Key;
                DatItem savedItem = output[pos].Value;
                var savedItemSource = GetSourceForItem(savedIndex);
                DupeType dupetype = GetDuplicateStatus(kvp, datItemSource.Value, output[pos], savedItemSource.Value);

                // Disks, Media, and Roms have more information to fill
                if (datItem is Disk diskItem && savedItem is Disk savedDisk)
                    savedDisk.FillMissingInformation(diskItem);
                else if (datItem is DatItems.Formats.File fileItem && savedItem is DatItems.Formats.File savedFile)
                    savedFile.FillMissingInformation(fileItem);
                else if (datItem is Media mediaItem && savedItem is Media savedMedia)
                    savedMedia.FillMissingInformation(mediaItem);
                else if (datItem is Rom romItem && savedItem is Rom savedRom)
                    savedRom.FillMissingInformation(romItem);

                savedItem.SetFieldValue<DupeType>(DatItem.DupeTypeKey, dupetype);

                // Get the sources associated with the items
                var savedSource = _sources[_itemToSourceMapping[savedIndex]];
                var itemSource = _sources[_itemToSourceMapping[itemIndex]];

                // Get the machines associated with the items
                var savedMachine = _machines[_itemToMachineMapping[savedIndex]];
                var itemMachine = _machines[_itemToMachineMapping[itemIndex]];

                // If the current source has a lower ID than the saved, use the saved source
                if (itemSource?.Index < savedSource?.Index)
                {
                    _itemToSourceMapping[itemIndex] = _itemToSourceMapping[savedIndex];
                    _machines[_itemToMachineMapping[savedIndex]] = (itemMachine.Clone() as Machine)!;
                    savedItem.SetName(datItem.GetName());
                }

                // If the saved machine is a child of the current machine, use the current machine instead
                if (savedMachine.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey) == itemMachine.GetName()
                    || savedMachine.GetStringFieldValue(Models.Metadata.Machine.RomOfKey) == itemMachine.GetName())
                {
                    _machines[_itemToMachineMapping[savedIndex]] = (itemMachine.Clone() as Machine)!;
                    savedItem.SetName(datItem.GetName());
                }

                // Replace the original item in the list
                output.RemoveAt(pos);
                output.Insert(pos, new KeyValuePair<long, DatItem>(savedIndex, savedItem));
            }

            return output;
        }

        /// <summary>
        /// Ensure the key exists in the items dictionary
        /// </summary>
        private void EnsureBucketingKey(string key)
        {
            // If the key is missing from the dictionary, add it
#if NET40_OR_GREATER || NETCOREAPP
            _buckets.GetOrAdd(key, []);
#else
            if (!_buckets.ContainsKey(key))
                _buckets[key] = [];
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
        /// Get the bucketing key for a given item index
        /// <param name="itemIndex">Index of the current item</param>
        /// <param name="bucketBy">ItemKey value representing what key to get</param>
        /// <param name="lower">True if the key should be lowercased, false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        /// </summary>
        private string GetBucketKey(long itemIndex, ItemKey bucketBy, bool lower, bool norename)
        {
#if NET40_OR_GREATER || NETCOREAPP
            if (!_items.TryGetValue(itemIndex, out var datItem) || datItem == null)
                return string.Empty;
#else
            if (!_items.ContainsKey(itemIndex))
                return string.Empty;

            var datItem = _items[itemIndex];
            if (datItem == null)
                return string.Empty;
#endif

            var source = GetSourceForItem(itemIndex);
            var machine = GetMachineForItem(itemIndex);

            // Treat NULL like machine
            if (bucketBy == ItemKey.NULL)
                bucketBy = ItemKey.Machine;

            // Get the bucket key
            return datItem.GetKey(bucketBy, machine.Value, source.Value, lower, norename);
        }

        /// <summary>
        /// Perform bucketing based on the item key provided
        /// </summary>
        /// <param name="bucketBy">ItemKey enum representing how to bucket the individual items</param>
        /// <param name="lower">True if the key should be lowercased, false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        private void PerformBucketing(ItemKey bucketBy, bool lower, bool norename)
        {
            // Reset the bucketing values
            _bucketedBy = bucketBy;
            _buckets.Clear();

            // Get the current list of item indicies
            long[] itemIndicies = [.. _items.Keys];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, itemIndicies.Length, Core.Globals.ParallelOptions, i =>
#elif NET40_OR_GREATER
            Parallel.For(0, itemIndicies.Length, i =>
#else
            for (int i = 0; i < itemIndicies.Length; i++)
#endif
            {
                PerformItemBucketing(i, bucketBy, lower, norename);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Bucket a single DatItem
        /// </summary>
        /// <param name="itemIndex">Index of the item to bucket</param>
        /// <param name="bucketBy">ItemKey enum representing how to bucket the individual items</param>
        /// <param name="lower">True if the key should be lowercased, false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        private void PerformItemBucketing(long itemIndex, ItemKey bucketBy, bool lower, bool norename)
        {
            string? bucketKey = GetBucketKey(itemIndex, bucketBy, lower, norename);
            lock (bucketKey)
            {
                EnsureBucketingKey(bucketKey);

#if NET40_OR_GREATER || NETCOREAPP
                if (!_buckets.TryGetValue(bucketKey, out var bucket) || bucket == null)
                    return;

                bucket.Add(itemIndex);
#else
                _buckets[bucketKey].Add(itemIndex);
#endif
            }
        }

        /// <summary>
        /// Sort existing buckets for consistency
        /// </summary>
        private void PerformSorting(bool norename)
        {
            // Get the current list of bucket keys
            string[] bucketKeys = [.. _buckets.Keys];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, bucketKeys.Length, Core.Globals.ParallelOptions, i =>
#elif NET40_OR_GREATER
            Parallel.For(0, bucketKeys.Length, i =>
#else
            for (int i = 0; i < bucketKeys.Length; i++)
#endif
            {
#if NET452_OR_GREATER || NETCOREAPP
                _buckets.TryGetValue(bucketKeys[i], out var itemIndices);
#else
                var itemIndices = _buckets[bucketKeys[i]];
#endif
                if (itemIndices == null || itemIndices.Count == 0)
                {
#if NET40_OR_GREATER || NETCOREAPP
                    _buckets.TryRemove(bucketKeys[i], out _);
                    return;
#else
                    _buckets.Remove(bucketKeys[i]);
                    continue;
#endif
                }

                var datItems = itemIndices
                    .FindAll(i => _items.ContainsKey(i))
                    .ConvertAll(i => new KeyValuePair<long, DatItem>(i, _items[i]));

                Sort(ref datItems, norename);

#if NET40_OR_GREATER || NETCOREAPP
                _buckets.TryAdd(bucketKeys[i], datItems.ConvertAll(kvp => kvp.Key));
            });
#else
                _buckets[bucketKeys[i]] = datItems.ConvertAll(kvp => kvp.Key);
            }
#endif
        }

        /// <summary>
        /// Sort a list of item pairs by SourceID, Game, and Name (in order)
        /// </summary>
        /// <param name="itemMappings">List of pairs representing the items to be sorted</param>
        /// <param name="norename">True if files are not renamed, false otherwise</param>
        /// <returns>True if it sorted correctly, false otherwise</returns>
        private bool Sort(ref List<KeyValuePair<long, DatItem>> itemMappings, bool norename)
        {
            // Create the comparer extenal to the delegate
            var nc = new NaturalComparer();

            itemMappings.Sort(delegate (KeyValuePair<long, DatItem> x, KeyValuePair<long, DatItem> y)
            {
                try
                {
                    // Compare on source if renaming
                    if (!norename)
                    {
                        int xSourceIndex = GetSourceForItem(x.Key).Value?.Index ?? 0;
                        int ySourceIndex = GetSourceForItem(y.Key).Value?.Index ?? 0;
                        if (xSourceIndex != ySourceIndex)
                            return xSourceIndex - ySourceIndex;
                    }

                    // Get the machines
                    Machine? xMachine = _machines[_itemToMachineMapping[x.Key]];
                    Machine? yMachine = _machines[_itemToMachineMapping[y.Key]];

                    // If machine names don't match
                    string? xMachineName = xMachine?.GetName();
                    string? yMachineName = yMachine?.GetName();
                    if (xMachineName != yMachineName)
                        return nc.Compare(xMachineName, yMachineName);

                    // If types don't match
                    string? xType = x.Value.GetStringFieldValue(Models.Metadata.DatItem.TypeKey);
                    string? yType = y.Value.GetStringFieldValue(Models.Metadata.DatItem.TypeKey);
                    if (xType != yType)
                        return xType.AsItemType() - yType.AsItemType();

                    // If directory names don't match
                    string? xDirectoryName = Path.GetDirectoryName(TextHelper.RemovePathUnsafeCharacters(x.Value.GetName()));
                    string? yDirectoryName = Path.GetDirectoryName(TextHelper.RemovePathUnsafeCharacters(y.Value.GetName()));
                    if (xDirectoryName != yDirectoryName)
                        return nc.Compare(xDirectoryName, yDirectoryName);

                    // If item names don't match
                    string? xName = Path.GetFileName(TextHelper.RemovePathUnsafeCharacters(x.Value.GetName()));
                    string? yName = Path.GetFileName(TextHelper.RemovePathUnsafeCharacters(y.Value.GetName()));
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
        private string SortAndGetKey(KeyValuePair<long, DatItem> datItem, bool sorted = false)
        {
            // If we're not already sorted, take care of it
            if (!sorted)
                BucketBy(GetBestAvailable());

            // Now that we have the sorted type, we get the proper key
            return GetBucketKey(datItem.Key, _bucketedBy, lower: true, norename: true);
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

            // If there are no items
            if (_items == null || _items.Count == 0)
                return;

            // Loop through and add
            foreach (var item in _items.Values)
            {
                if (item == null)
                    continue;

                DatStatistics.AddItemStatistics(item);
            }
        }

        #endregion
    }
}
