using System.Collections.Generic;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;

namespace SabreTools.DatFiles
{
    /// <summary>
    /// Statistics wrapper for outputting
    /// </summary>
    public class DatStatistics
    {
        #region Private instance variables

        /// <summary>
        /// Number of items for each hash type
        /// </summary>
        private readonly Dictionary<HashType, long> _hashCounts = [];

        /// <summary>
        /// Number of items for each item type
        /// </summary>
        private readonly Dictionary<ItemType, long> _itemCounts = [];

        /// <summary>
        /// Number of items for each item status
        /// </summary>
        private readonly Dictionary<ItemStatus, long> _statusCounts = [];

        /// <summary>
        /// Lock for statistics calculation
        /// </summary>
        private readonly object statsLock = new();

        #endregion

        #region Fields

        /// <summary>
        /// Overall item count
        /// </summary>
        public long TotalCount { get; private set; } = 0;

        /// <summary>
        /// Number of machines
        /// </summary>
        /// <remarks>Special count only used by statistics output</remarks>
        public long GameCount { get; set; } = 0;

        /// <summary>
        /// Total uncompressed size
        /// </summary>
        public long TotalSize { get; private set; } = 0;

        /// <summary>
        /// Number of items with the remove flag
        /// </summary>
        public long RemovedCount { get; private set; } = 0;

        /// <summary>
        /// Name to display on output
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// Total machine count to use on output
        /// </summary>
        public long MachineCount { get; set; }

        /// <summary>
        /// Determines if statistics are for a directory or not
        /// </summary>
        public readonly bool IsDirectory;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DatStatistics()
        {
            DisplayName = null;
            MachineCount = 0;
            IsDirectory = false;
        }

        /// <summary>
        /// Constructor for aggregate data
        /// </summary>
        public DatStatistics(string? displayName, bool isDirectory)
        {
            DisplayName = displayName;
            MachineCount = 0;
            IsDirectory = isDirectory;
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Add to the statistics for a given DatItem
        /// </summary>
        /// <param name="item">Item to add info from</param>
        public void AddItemStatistics(DatItem item)
        {
            lock (statsLock)
            {
                // No matter what the item is, we increment the count
                TotalCount++;

                // Increment removal count
                if (item.GetBoolFieldValue(DatItem.RemoveKey) == true)
                    RemovedCount++;

                // Increment the item count for the type
                AddItemCount(item.GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey).AsItemType());

                // Some item types require special processing
                switch (item)
                {
                    case Disk disk:
                        AddItemStatistics(disk);
                        break;
                    case DatItems.Formats.File file:
                        AddItemStatistics(file);
                        break;
                    case Media media:
                        AddItemStatistics(media);
                        break;
                    case Rom rom:
                        AddItemStatistics(rom);
                        break;
                }
            }
        }

        /// <summary>
        /// Add to the statistics for a given DatItem
        /// </summary>
        /// <param name="item">Item to add info from</param>
        public void AddItemStatistics(Data.Models.Metadata.DatItem item)
        {
            lock (statsLock)
            {
                // No matter what the item is, we increment the count
                TotalCount++;

                // Increment removal count
                if (item.ReadBool(DatItem.RemoveKey) == true)
                    RemovedCount++;

                // Increment the item count for the type
                AddItemCount(item.ReadString(Data.Models.Metadata.DatItem.TypeKey).AsItemType());

                // Some item types require special processing
                switch (item)
                {
                    case Data.Models.Metadata.Disk disk:
                        AddItemStatistics(disk);
                        break;
                    case Data.Models.Metadata.Media media:
                        AddItemStatistics(media);
                        break;
                    case Data.Models.Metadata.Rom rom:
                        AddItemStatistics(rom);
                        break;
                }
            }
        }

        /// <summary>
        /// Add statistics from another DatStatistics object
        /// </summary>
        /// <param name="stats">DatStatistics object to add from</param>
        public void AddStatistics(DatStatistics stats)
        {
            TotalCount += stats.TotalCount;

            // Loop through and add stats for all items
            foreach (var itemCountKvp in stats._itemCounts)
            {
                AddItemCount(itemCountKvp.Key, itemCountKvp.Value);
            }

            GameCount += stats.GameCount;

            TotalSize += stats.TotalSize;

            // Individual hash counts
            foreach (var hashCountKvp in stats._hashCounts)
            {
                AddHashCount(hashCountKvp.Key, hashCountKvp.Value);
            }

            // Individual status counts
            foreach (var statusCountKvp in stats._statusCounts)
            {
                AddStatusCount(statusCountKvp.Key, statusCountKvp.Value);
            }

            RemovedCount += stats.RemovedCount;
        }

        /// <summary>
        /// Get the item count for a given hash type, defaulting to 0 if it does not exist
        /// </summary>
        /// <param name="hashType">Hash type to retrieve</param>
        /// <returns>The number of items with that hash, if it exists</returns>
        public long GetHashCount(HashType hashType)
        {
            lock (_hashCounts)
            {
                if (!_hashCounts.ContainsKey(hashType))
                    return 0;

                return _hashCounts[hashType];
            }
        }

        /// <summary>
        /// Get the item count for a given item type, defaulting to 0 if it does not exist
        /// </summary>
        /// <param name="itemType">Item type to retrieve</param>
        /// <returns>The number of items of that type, if it exists</returns>
        public long GetItemCount(ItemType itemType)
        {
            lock (_itemCounts)
            {
                if (!_itemCounts.ContainsKey(itemType))
                    return 0;

                return _itemCounts[itemType];
            }
        }

        /// <summary>
        /// Get the item count for a given item status, defaulting to 0 if it does not exist
        /// </summary>
        /// <param name="itemStatus">Item status to retrieve</param>
        /// <returns>The number of items of that type, if it exists</returns>
        public long GetStatusCount(ItemStatus itemStatus)
        {
            lock (_statusCounts)
            {
                if (!_statusCounts.ContainsKey(itemStatus))
                    return 0;

                return _statusCounts[itemStatus];
            }
        }

        /// <summary>
        /// Remove from the statistics given a DatItem
        /// </summary>
        /// <param name="item">Item to remove info for</param>
        public void RemoveItemStatistics(DatItem item)
        {
            // If we have a null item, we can't do anything
            if (item == null)
                return;

            lock (statsLock)
            {
                // No matter what the item is, we decrease the count
                TotalCount--;

                // Decrement removal count
                if (item.GetBoolFieldValue(DatItem.RemoveKey) == true)
                    RemovedCount--;

                // Decrement the item count for the type
                RemoveItemCount(item.GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey).AsItemType());

                // Some item types require special processing
                switch (item)
                {
                    case Disk disk:
                        RemoveItemStatistics(disk);
                        break;
                    case DatItems.Formats.File file:
                        RemoveItemStatistics(file);
                        break;
                    case Media media:
                        RemoveItemStatistics(media);
                        break;
                    case Rom rom:
                        RemoveItemStatistics(rom);
                        break;
                }
            }
        }

        /// <summary>
        /// Remove from the statistics given a DatItem
        /// </summary>
        /// <param name="item">Item to remove info for</param>
        public void RemoveItemStatistics(Data.Models.Metadata.DatItem item)
        {
            // If we have a null item, we can't do anything
            if (item == null)
                return;

            lock (statsLock)
            {
                // No matter what the item is, we decrease the count
                TotalCount--;

                // Decrement removal count
                if (item.ReadBool(DatItem.RemoveKey) == true)
                    RemovedCount--;

                // Decrement the item count for the type
                RemoveItemCount(item.ReadString(Data.Models.Metadata.DatItem.TypeKey).AsItemType());

                // Some item types require special processing
                switch (item)
                {
                    case Data.Models.Metadata.Disk disk:
                        RemoveItemStatistics(disk);
                        break;
                    case Data.Models.Metadata.Media media:
                        RemoveItemStatistics(media);
                        break;
                    case Data.Models.Metadata.Rom rom:
                        RemoveItemStatistics(rom);
                        break;
                }
            }
        }

        /// <summary>
        /// Reset all statistics
        /// </summary>
        public void ResetStatistics()
        {
            _hashCounts.Clear();
            _itemCounts.Clear();
            _statusCounts.Clear();

            TotalCount = 0;
            GameCount = 0;
            TotalSize = 0;
            RemovedCount = 0;
        }

        /// <summary>
        /// Increment the hash count for a given hash type
        /// </summary>
        /// <param name="hashType">Hash type to increment</param>
        /// <param name="interval">Amount to increment by, defaults to 1</param>
        private void AddHashCount(HashType hashType, long interval = 1)
        {
            lock (_hashCounts)
            {
                if (!_hashCounts.ContainsKey(hashType))
                    _hashCounts[hashType] = 0;

                _hashCounts[hashType] += interval;
                if (_hashCounts[hashType] < 0)
                    _hashCounts[hashType] = 0;
            }
        }

        /// <summary>
        /// Increment the item count for a given item type
        /// </summary>
        /// <param name="itemType">Item type to increment</param>
        /// <param name="interval">Amount to increment by, defaults to 1</param>
        private void AddItemCount(ItemType itemType, long interval = 1)
        {
            lock (_itemCounts)
            {
                if (!_itemCounts.ContainsKey(itemType))
                    _itemCounts[itemType] = 0;

                _itemCounts[itemType] += interval;
                if (_itemCounts[itemType] < 0)
                    _itemCounts[itemType] = 0;
            }
        }

        /// <summary>
        /// Add to the statistics for a given Disk
        /// </summary>
        /// <param name="disk">Item to add info from</param>
        private void AddItemStatistics(Disk disk)
        {
            if (disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() != ItemStatus.Nodump)
            {
                AddHashCount(HashType.MD5, string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key)) ? 0 : 1);
                AddHashCount(HashType.SHA1, string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key)) ? 0 : 1);
            }

            AddStatusCount(ItemStatus.BadDump, disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.BadDump ? 1 : 0);
            AddStatusCount(ItemStatus.Good, disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Good ? 1 : 0);
            AddStatusCount(ItemStatus.Nodump, disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Nodump ? 1 : 0);
            AddStatusCount(ItemStatus.Verified, disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Verified ? 1 : 0);
        }

        /// <summary>
        /// Add to the statistics for a given Disk
        /// </summary>
        /// <param name="disk">Item to add info from</param>
        private void AddItemStatistics(Data.Models.Metadata.Disk disk)
        {
            if (disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() != ItemStatus.Nodump)
            {
                AddHashCount(HashType.MD5, string.IsNullOrEmpty(disk.ReadString(Data.Models.Metadata.Disk.MD5Key)) ? 0 : 1);
                AddHashCount(HashType.SHA1, string.IsNullOrEmpty(disk.ReadString(Data.Models.Metadata.Disk.SHA1Key)) ? 0 : 1);
            }

            AddStatusCount(ItemStatus.BadDump, disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.BadDump ? 1 : 0);
            AddStatusCount(ItemStatus.Good, disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Good ? 1 : 0);
            AddStatusCount(ItemStatus.Nodump, disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Nodump ? 1 : 0);
            AddStatusCount(ItemStatus.Verified, disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Verified ? 1 : 0);
        }

        /// <summary>
        /// Add to the statistics for a given File
        /// </summary>
        /// <param name="file">Item to add info from</param>
        private void AddItemStatistics(DatItems.Formats.File file)
        {
            TotalSize += file.Size ?? 0;
            AddHashCount(HashType.CRC32, string.IsNullOrEmpty(file.CRC) ? 0 : 1);
            AddHashCount(HashType.MD5, string.IsNullOrEmpty(file.MD5) ? 0 : 1);
            AddHashCount(HashType.SHA1, string.IsNullOrEmpty(file.SHA1) ? 0 : 1);
            AddHashCount(HashType.SHA256, string.IsNullOrEmpty(file.SHA256) ? 0 : 1);
        }

        /// <summary>
        /// Add to the statistics for a given Media
        /// </summary>
        /// <param name="media">Item to add info from</param>
        private void AddItemStatistics(Media media)
        {
            AddHashCount(HashType.MD5, string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.MD5Key)) ? 0 : 1);
            AddHashCount(HashType.SHA1, string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.SHA1Key)) ? 0 : 1);
            AddHashCount(HashType.SHA256, string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.SHA256Key)) ? 0 : 1);
            AddHashCount(HashType.SpamSum, string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.SpamSumKey)) ? 0 : 1);
        }

        /// <summary>
        /// Add to the statistics for a given Media
        /// </summary>
        /// <param name="media">Item to add info from</param>
        private void AddItemStatistics(Data.Models.Metadata.Media media)
        {
            AddHashCount(HashType.MD5, string.IsNullOrEmpty(media.ReadString(Data.Models.Metadata.Media.MD5Key)) ? 0 : 1);
            AddHashCount(HashType.SHA1, string.IsNullOrEmpty(media.ReadString(Data.Models.Metadata.Media.SHA1Key)) ? 0 : 1);
            AddHashCount(HashType.SHA256, string.IsNullOrEmpty(media.ReadString(Data.Models.Metadata.Media.SHA256Key)) ? 0 : 1);
            AddHashCount(HashType.SpamSum, string.IsNullOrEmpty(media.ReadString(Data.Models.Metadata.Media.SpamSumKey)) ? 0 : 1);
        }

        /// <summary>
        /// Add to the statistics for a given Rom
        /// </summary>
        /// <param name="rom">Item to add info from</param>
        private void AddItemStatistics(Rom rom)
        {
            if (rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() != ItemStatus.Nodump)
            {
                TotalSize += rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) ?? 0;
                AddHashCount(HashType.CRC32, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey)) ? 0 : 1);
                AddHashCount(HashType.MD2, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key)) ? 0 : 1);
                AddHashCount(HashType.MD4, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key)) ? 0 : 1);
                AddHashCount(HashType.MD5, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key)) ? 0 : 1);
                AddHashCount(HashType.RIPEMD128, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key)) ? 0 : 1);
                AddHashCount(HashType.RIPEMD160, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key)) ? 0 : 1);
                AddHashCount(HashType.SHA1, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key)) ? 0 : 1);
                AddHashCount(HashType.SHA256, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key)) ? 0 : 1);
                AddHashCount(HashType.SHA384, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key)) ? 0 : 1);
                AddHashCount(HashType.SHA512, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key)) ? 0 : 1);
                AddHashCount(HashType.SpamSum, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SpamSumKey)) ? 0 : 1);
            }

            AddStatusCount(ItemStatus.BadDump, rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.BadDump ? 1 : 0);
            AddStatusCount(ItemStatus.Good, rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Good ? 1 : 0);
            AddStatusCount(ItemStatus.Nodump, rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Nodump ? 1 : 0);
            AddStatusCount(ItemStatus.Verified, rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Verified ? 1 : 0);
        }

        /// <summary>
        /// Add to the statistics for a given Rom
        /// </summary>
        /// <param name="rom">Item to add info from</param>
        private void AddItemStatistics(Data.Models.Metadata.Rom rom)
        {
            if (rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() != ItemStatus.Nodump)
            {
                TotalSize += rom.ReadLong(Data.Models.Metadata.Rom.SizeKey) ?? 0;
                AddHashCount(HashType.CRC32, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.CRCKey)) ? 0 : 1);
                AddHashCount(HashType.MD2, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.MD2Key)) ? 0 : 1);
                AddHashCount(HashType.MD4, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.MD4Key)) ? 0 : 1);
                AddHashCount(HashType.MD5, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.MD5Key)) ? 0 : 1);
                AddHashCount(HashType.RIPEMD128, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.RIPEMD128Key)) ? 0 : 1);
                AddHashCount(HashType.RIPEMD160, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.RIPEMD160Key)) ? 0 : 1);
                AddHashCount(HashType.SHA1, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SHA1Key)) ? 0 : 1);
                AddHashCount(HashType.SHA256, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SHA256Key)) ? 0 : 1);
                AddHashCount(HashType.SHA384, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SHA384Key)) ? 0 : 1);
                AddHashCount(HashType.SHA512, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SHA512Key)) ? 0 : 1);
                AddHashCount(HashType.SpamSum, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SpamSumKey)) ? 0 : 1);
            }

            AddStatusCount(ItemStatus.BadDump, rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.BadDump ? 1 : 0);
            AddStatusCount(ItemStatus.Good, rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Good ? 1 : 0);
            AddStatusCount(ItemStatus.Nodump, rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Nodump ? 1 : 0);
            AddStatusCount(ItemStatus.Verified, rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Verified ? 1 : 0);
        }

        /// <summary>
        /// Increment the item count for a given item status
        /// </summary>
        /// <param name="itemStatus">Item type to increment</param>
        /// <param name="interval">Amount to increment by, defaults to 1</param>
        private void AddStatusCount(ItemStatus itemStatus, long interval = 1)
        {
            lock (_statusCounts)
            {
                if (!_statusCounts.ContainsKey(itemStatus))
                    _statusCounts[itemStatus] = 0;

                _statusCounts[itemStatus] += interval;
                if (_statusCounts[itemStatus] < 0)
                    _statusCounts[itemStatus] = 0;
            }
        }

        /// <summary>
        /// Decrement the hash count for a given hash type
        /// </summary>
        /// <param name="hashType">Hash type to increment</param>
        /// <param name="interval">Amount to increment by, defaults to 1</param>
        private void RemoveHashCount(HashType hashType, long interval = 1)
        {
            lock (_hashCounts)
            {
                if (!_hashCounts.ContainsKey(hashType))
                    return;

                _hashCounts[hashType] -= interval;
                if (_hashCounts[hashType] < 0)
                    _hashCounts[hashType] = 0;
            }
        }

        /// <summary>
        /// Decrement the item count for a given item type
        /// </summary>
        /// <param name="itemType">Item type to decrement</param>
        /// <param name="interval">Amount to increment by, defaults to 1</param>
        private void RemoveItemCount(ItemType itemType, long interval = 1)
        {
            lock (_itemCounts)
            {
                if (!_itemCounts.ContainsKey(itemType))
                    return;

                _itemCounts[itemType] -= interval;
                if (_itemCounts[itemType] < 0)
                    _itemCounts[itemType] = 0;
            }
        }

        /// <summary>
        /// Remove from the statistics given a Disk
        /// </summary>
        /// <param name="disk">Item to remove info for</param>
        private void RemoveItemStatistics(Disk disk)
        {
            if (disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() != ItemStatus.Nodump)
            {
                RemoveHashCount(HashType.MD5, string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA1, string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key)) ? 0 : 1);
            }

            RemoveStatusCount(ItemStatus.BadDump, disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.BadDump ? 1 : 0);
            RemoveStatusCount(ItemStatus.Good, disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Good ? 1 : 0);
            RemoveStatusCount(ItemStatus.Nodump, disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Nodump ? 1 : 0);
            RemoveStatusCount(ItemStatus.Verified, disk.GetStringFieldValue(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Verified ? 1 : 0);
        }

        /// <summary>
        /// Remove from the statistics given a Disk
        /// </summary>
        /// <param name="disk">Item to remove info for</param>
        private void RemoveItemStatistics(Data.Models.Metadata.Disk disk)
        {
            if (disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() != ItemStatus.Nodump)
            {
                RemoveHashCount(HashType.MD5, string.IsNullOrEmpty(disk.ReadString(Data.Models.Metadata.Disk.MD5Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA1, string.IsNullOrEmpty(disk.ReadString(Data.Models.Metadata.Disk.SHA1Key)) ? 0 : 1);
            }

            RemoveStatusCount(ItemStatus.BadDump, disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.BadDump ? 1 : 0);
            RemoveStatusCount(ItemStatus.Good, disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Good ? 1 : 0);
            RemoveStatusCount(ItemStatus.Nodump, disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Nodump ? 1 : 0);
            RemoveStatusCount(ItemStatus.Verified, disk.ReadString(Data.Models.Metadata.Disk.StatusKey).AsItemStatus() == ItemStatus.Verified ? 1 : 0);
        }

        /// <summary>
        /// Remove from the statistics given a File
        /// </summary>
        /// <param name="file">Item to remove info for</param>
        private void RemoveItemStatistics(DatItems.Formats.File file)
        {
            TotalSize -= file.Size ?? 0;
            RemoveHashCount(HashType.CRC32, string.IsNullOrEmpty(file.CRC) ? 0 : 1);
            RemoveHashCount(HashType.MD5, string.IsNullOrEmpty(file.MD5) ? 0 : 1);
            RemoveHashCount(HashType.SHA1, string.IsNullOrEmpty(file.SHA1) ? 0 : 1);
            RemoveHashCount(HashType.SHA256, string.IsNullOrEmpty(file.SHA256) ? 0 : 1);
        }

        /// <summary>
        /// Remove from the statistics given a Media
        /// </summary>
        /// <param name="media">Item to remove info for</param>
        private void RemoveItemStatistics(Media media)
        {
            RemoveHashCount(HashType.MD5, string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.MD5Key)) ? 0 : 1);
            RemoveHashCount(HashType.SHA1, string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.SHA1Key)) ? 0 : 1);
            RemoveHashCount(HashType.SHA256, string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.SHA256Key)) ? 0 : 1);
            RemoveHashCount(HashType.SpamSum, string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.SpamSumKey)) ? 0 : 1);
        }

        /// <summary>
        /// Remove from the statistics given a Media
        /// </summary>
        /// <param name="media">Item to remove info for</param>
        private void RemoveItemStatistics(Data.Models.Metadata.Media media)
        {
            RemoveHashCount(HashType.MD5, string.IsNullOrEmpty(media.ReadString(Data.Models.Metadata.Media.MD5Key)) ? 0 : 1);
            RemoveHashCount(HashType.SHA1, string.IsNullOrEmpty(media.ReadString(Data.Models.Metadata.Media.SHA1Key)) ? 0 : 1);
            RemoveHashCount(HashType.SHA256, string.IsNullOrEmpty(media.ReadString(Data.Models.Metadata.Media.SHA256Key)) ? 0 : 1);
            RemoveHashCount(HashType.SpamSum, string.IsNullOrEmpty(media.ReadString(Data.Models.Metadata.Media.SpamSumKey)) ? 0 : 1);
        }

        /// <summary>
        /// Remove from the statistics given a Rom
        /// </summary>
        /// <param name="rom">Item to remove info for</param>
        private void RemoveItemStatistics(Rom rom)
        {
            if (rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() != ItemStatus.Nodump)
            {
                TotalSize -= rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) ?? 0;
                RemoveHashCount(HashType.CRC32, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey)) ? 0 : 1);
                RemoveHashCount(HashType.MD2, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key)) ? 0 : 1);
                RemoveHashCount(HashType.MD4, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key)) ? 0 : 1);
                RemoveHashCount(HashType.MD5, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key)) ? 0 : 1);
                RemoveHashCount(HashType.RIPEMD128, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key)) ? 0 : 1);
                RemoveHashCount(HashType.RIPEMD160, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA1, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA256, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA384, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA512, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key)) ? 0 : 1);
                RemoveHashCount(HashType.SpamSum, string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SpamSumKey)) ? 0 : 1);
            }

            RemoveStatusCount(ItemStatus.BadDump, rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.BadDump ? 1 : 0);
            RemoveStatusCount(ItemStatus.Good, rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Good ? 1 : 0);
            RemoveStatusCount(ItemStatus.Nodump, rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Nodump ? 1 : 0);
            RemoveStatusCount(ItemStatus.Verified, rom.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Verified ? 1 : 0);
        }

        /// <summary>
        /// Remove from the statistics given a Rom
        /// </summary>
        /// <param name="rom">Item to remove info for</param>
        private void RemoveItemStatistics(Data.Models.Metadata.Rom rom)
        {
            if (rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() != ItemStatus.Nodump)
            {
                TotalSize -= rom.ReadLong(Data.Models.Metadata.Rom.SizeKey) ?? 0;
                RemoveHashCount(HashType.CRC32, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.CRCKey)) ? 0 : 1);
                RemoveHashCount(HashType.MD2, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.MD2Key)) ? 0 : 1);
                RemoveHashCount(HashType.MD4, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.MD4Key)) ? 0 : 1);
                RemoveHashCount(HashType.MD5, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.MD5Key)) ? 0 : 1);
                RemoveHashCount(HashType.RIPEMD128, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.RIPEMD128Key)) ? 0 : 1);
                RemoveHashCount(HashType.RIPEMD160, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.RIPEMD160Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA1, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SHA1Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA256, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SHA256Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA384, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SHA384Key)) ? 0 : 1);
                RemoveHashCount(HashType.SHA512, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SHA512Key)) ? 0 : 1);
                RemoveHashCount(HashType.SpamSum, string.IsNullOrEmpty(rom.ReadString(Data.Models.Metadata.Rom.SpamSumKey)) ? 0 : 1);
            }

            RemoveStatusCount(ItemStatus.BadDump, rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.BadDump ? 1 : 0);
            RemoveStatusCount(ItemStatus.Good, rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Good ? 1 : 0);
            RemoveStatusCount(ItemStatus.Nodump, rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Nodump ? 1 : 0);
            RemoveStatusCount(ItemStatus.Verified, rom.ReadString(Data.Models.Metadata.Rom.StatusKey).AsItemStatus() == ItemStatus.Verified ? 1 : 0);
        }

        /// <summary>
        /// Decrement the item count for a given item status
        /// </summary>
        /// <param name="itemStatus">Item type to decrement</param>
        /// <param name="interval">Amount to increment by, defaults to 1</param>
        private void RemoveStatusCount(ItemStatus itemStatus, long interval = 1)
        {
            lock (_statusCounts)
            {
                if (!_statusCounts.ContainsKey(itemStatus))
                    return;

                _statusCounts[itemStatus] -= interval;
                if (_statusCounts[itemStatus] < 0)
                    _statusCounts[itemStatus] = 0;
            }
        }

        #endregion
    }
}
