using System;
using System.Collections.Generic;
using System.IO;
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
    /// Represents a format-agnostic DAT
    /// </summary>
    [JsonObject("datfile"), XmlRoot("datfile")]
    public abstract partial class DatFile
    {
        #region Fields

        /// <summary>
        /// Header values
        /// </summary>
        [JsonProperty("header"), XmlElement("header")]
        public DatHeader Header { get; private set; } = new DatHeader();

        /// <summary>
        /// Modifier values
        /// </summary>
        [JsonProperty("modifiers"), XmlElement("modifiers")]
        public DatModifiers Modifiers { get; private set; } = new DatModifiers();

        /// <summary>
        /// DatItems and related statistics
        /// </summary>
        [JsonProperty("items"), XmlElement("items")]
        public ItemDictionary Items { get; private set; } = new ItemDictionary();

        /// <summary>
        /// DatItems and related statistics
        /// </summary>
        [JsonProperty("items"), XmlElement("items")]
        public ItemDictionaryDB ItemsDB { get; private set; } = new ItemDictionaryDB();

        /// <summary>
        /// DAT statistics
        /// </summary>
        [JsonIgnore, XmlIgnore]
        public DatStatistics DatStatistics => Items.DatStatistics;
        //public DatStatistics DatStatistics => ItemsDB.DatStatistics;

        /// <summary>
        /// List of supported types for writing
        /// </summary>
        public abstract ItemType[] SupportedTypes { get; }

        #endregion

        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        [JsonIgnore, XmlIgnore]
        protected Logger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new DatFile from an existing one
        /// </summary>
        /// <param name="datFile">DatFile to get the values from</param>
        public DatFile(DatFile? datFile)
        {
            _logger = new Logger(this);
            if (datFile != null)
            {
                Header = (DatHeader)datFile.Header.Clone();
                Modifiers = (DatModifiers)datFile.Modifiers.Clone();
                Items = datFile.Items;
                ItemsDB = datFile.ItemsDB;
            }
        }

        /// <summary>
        /// Fill the header values based on existing Header and path
        /// </summary>
        /// <param name="path">Path used for creating a name, if necessary</param>
        /// <param name="bare">True if the date should be omitted from name and description, false otherwise</param>
        public void FillHeaderFromPath(string path, bool bare)
        {
            // Get the header strings
            string? name = Header.GetStringFieldValue(Models.Metadata.Header.NameKey);
            string? description = Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey);
            string? date = Header.GetStringFieldValue(Models.Metadata.Header.DateKey);

            // If the description is defined but not the name, set the name from the description
            if (string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(description))
            {
                name = description + (bare ? string.Empty : $" ({date})");
            }

            // If the name is defined but not the description, set the description from the name
            else if (!string.IsNullOrEmpty(name) && string.IsNullOrEmpty(description))
            {
                description = name + (bare ? string.Empty : $" ({date})");
            }

            // If neither the name or description are defined, set them from the automatic values
            else if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(description))
            {
                string[] splitpath = path.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
#if NETFRAMEWORK
                name = splitpath[splitpath.Length - 1];
                description = splitpath[splitpath.Length - 1] + (bare ? string.Empty : $" ({date})");
#else
                name = splitpath[^1] + (bare ? string.Empty : $" ({date})");
                description = splitpath[^1] + (bare ? string.Empty : $" ({date})");
#endif
            }

            // Trim both fields
            name = name?.Trim();
            description = description?.Trim();

            // Set the fields back
            Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, name);
            Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, description);
        }

        #endregion

        #region Accessors

        /// <summary>
        /// Remove any keys that have null or empty values
        /// </summary>
        public void ClearEmpty()
        {
            ClearEmptyImpl();
            ClearEmptyImplDB();
        }

        /// <summary>
        /// Set the internal header
        /// </summary>
        /// <param name="datHeader">Replacement header to be used</param>
        public void SetHeader(DatHeader? datHeader)
        {
            if (datHeader != null)
                Header = (DatHeader)datHeader.Clone();
        }

        /// <summary>
        /// Set the internal header
        /// </summary>
        /// <param name="datHeader">Replacement header to be used</param>
        public void SetModifiers(DatModifiers datModifers)
        {
            Modifiers = (DatModifiers)datModifers.Clone();
        }

        /// <summary>
        /// Remove any keys that have null or empty values
        /// </summary>
        private void ClearEmptyImpl()
        {
            foreach (string key in Items.SortedKeys)
            {
                // If the value is empty, remove
                List<DatItem> value = GetItemsForBucket(key);
                if (value.Count == 0)
                    RemoveBucket(key);

                // If there are no non-blank items, remove
                else if (value.FindIndex(i => i != null && i is not Blank) == -1)
                    RemoveBucket(key);
            }
        }

        /// <summary>
        /// Remove any keys that have null or empty values
        /// </summary>
        private void ClearEmptyImplDB()
        {
            foreach (string key in ItemsDB.SortedKeys)
            {
                // If the value is empty, remove
                List<DatItem> value = [.. GetItemsForBucketDB(key).Values];
                if (value.Count == 0)
                    RemoveBucketDB(key);

                // If there are no non-blank items, remove
                else if (value.FindIndex(i => i != null && i is not Blank) == -1)
                    RemoveBucketDB(key);
            }
        }

        #endregion

        #region Item Dictionary Passthrough - Accessors

        /// <summary>
        /// Add a DatItem to the dictionary after checking
        /// </summary>
        /// <param name="item">Item data to check against</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <returns>The key for the item</returns>
        public string AddItem(DatItem item, bool statsOnly)
        {
            return Items.AddItem(item, statsOnly);
        }

        /// <summary>
        /// Add a DatItem to the dictionary after validation
        /// </summary>
        /// <param name="item">Item data to validate</param>
        /// <param name="machineIndex">Index of the machine related to the item</param>
        /// <param name="sourceIndex">Index of the source related to the item</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <returns>The index for the added item, -1 on error</returns>
        public long AddItemDB(DatItem item, long machineIndex, long sourceIndex, bool statsOnly)
        {
            return ItemsDB.AddItem(item, machineIndex, sourceIndex, statsOnly);
        }

        /// <summary>
        /// Add a machine, returning the insert index
        /// </summary>
        public long AddMachineDB(Machine machine)
        {
            return ItemsDB.AddMachine(machine);
        }

        /// <summary>
        /// Add a source, returning the insert index
        /// </summary>
        public long AddSourceDB(Source source)
        {
            return ItemsDB.AddSource(source);
        }

        /// <summary>
        /// Remove all items marked for removal
        /// </summary>
        public void ClearMarked()
        {
            Items.ClearMarked();
            ItemsDB.ClearMarked();
        }

        /// <summary>
        /// Get the items associated with a bucket name
        /// </summary>
        public List<DatItem> GetItemsForBucket(string? bucketName, bool filter = false)
            => Items.GetItemsForBucket(bucketName, filter);

        /// <summary>
        /// Get the indices and items associated with a bucket name
        /// </summary>
        public Dictionary<long, DatItem> GetItemsForBucketDB(string? bucketName, bool filter = false)
            => ItemsDB.GetItemsForBucket(bucketName, filter);

        /// <summary>
        /// Get all machines and their indicies
        /// </summary>
        public IDictionary<long, Machine> GetMachinesDB()
            => ItemsDB.GetMachines();

        /// <summary>
        /// Get the index and machine associated with an item index
        /// </summary>
        public KeyValuePair<long, Machine?> GetMachineForItemDB(long itemIndex)
            => ItemsDB.GetMachineForItem(itemIndex);

        /// <summary>
        /// Get the index and source associated with an item index
        /// </summary>
        public KeyValuePair<long, Source?> GetSourceForItemDB(long itemIndex)
            => ItemsDB.GetSourceForItem(itemIndex);

        /// <summary>
        /// Remove a key from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove</param>
        public bool RemoveBucket(string key)
        {
            return Items.RemoveBucket(key);
        }

        /// <summary>
        /// Remove a key from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove</param>
        public bool RemoveBucketDB(string key)
        {
            return ItemsDB.RemoveBucket(key);
        }

        /// <summary>
        /// Remove the indexed instance of a value from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove from</param>
        /// <param name="value">Value to remove from the dictionary</param>
        /// <param name="index">Index of the item to be removed</param>
        public bool RemoveItem(string key, DatItem value, int index)
        {
            return Items.RemoveItem(key, value, index);
        }

        /// <summary>
        /// Remove an item, returning if it could be removed
        /// </summary>
        public bool RemoveItemDB(long itemIndex)
        {
            return ItemsDB.RemoveItem(itemIndex);
        }

        /// <summary>
        /// Remove a machine, returning if it could be removed
        /// </summary>
        public bool RemoveMachineDB(long machineIndex)
        {
            return ItemsDB.RemoveMachine(machineIndex);
        }

        /// <summary>
        /// Remove a machine, returning if it could be removed
        /// </summary>
        public bool RemoveMachineDB(string machineName)
        {
            return ItemsDB.RemoveMachine(machineName);
        }

        /// <summary>
        /// Reset the internal item dictionary
        /// </summary>
        public void ResetDictionary()
        {
            Items = new ItemDictionary();
            ItemsDB = new ItemDictionaryDB();
        }

        #endregion

        #region Item Dictionary Passthrough - Bucketing

        /// <summary>
        /// Take the arbitrarily bucketed Files Dictionary and convert to one bucketed by a user-defined method
        /// </summary>
        /// <param name="bucketBy">ItemKey enum representing how to bucket the individual items</param>
        /// <param name="lower">True if the key should be lowercased (default), false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        public void BucketBy(ItemKey bucketBy, bool lower = true, bool norename = true)
        {
            Items.BucketBy(bucketBy, lower, norename);
            //ItemsDB.BucketBy(bucketBy, lower, norename);
        }

        /// <summary>
        /// Perform deduplication based on the deduplication type provided
        /// </summary>
        public void Deduplicate()
        {
            Items.Deduplicate();
            ItemsDB.Deduplicate();
        }

        /// <summary>
        /// List all duplicates found in a DAT based on a DatItem
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>List of matched DatItem objects</returns>
        public List<DatItem> GetDuplicates(DatItem datItem, bool sorted = false)
            => Items.GetDuplicates(datItem, sorted);

        /// <summary>
        /// List all duplicates found in a DAT based on a DatItem
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>List of matched DatItem objects</returns>
        public Dictionary<long, DatItem> GetDuplicatesDB(KeyValuePair<long, DatItem> datItem, bool sorted = false)
            => ItemsDB.GetDuplicates(datItem, sorted);

        /// <summary>
        /// Check if a DAT contains the given DatItem
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>True if it contains the rom, false otherwise</returns>
        public bool HasDuplicates(DatItem datItem, bool sorted = false)
            => Items.HasDuplicates(datItem, sorted);

        /// <summary>
        /// Check if a DAT contains the given DatItem
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>True if it contains the rom, false otherwise</returns>
        public bool HasDuplicates(KeyValuePair<long, DatItem> datItem, bool sorted = false)
            => ItemsDB.HasDuplicates(datItem, sorted);

        #endregion

        #region Item Dictionary Passthrough - Statistics

        /// <summary>
        /// Recalculate the statistics for the Dat
        /// </summary>
        public void RecalculateStats()
        {
            Items.RecalculateStats();
            ItemsDB.RecalculateStats();
        }

        #endregion

        #region Parsing

        /// <summary>
        /// Parse DatFile and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        public abstract void ParseFile(string filename,
            int indexId,
            bool keep,
            bool statsOnly = false,
            FilterRunner? filterRunner = null,
            bool throwOnError = false);

        #endregion

        #region Writing

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public abstract bool WriteToFile(string outfile, bool ignoreblanks = false, bool throwOnError = false);

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public abstract bool WriteToFileDB(string outfile, bool ignoreblanks = false, bool throwOnError = false);

        /// <summary>
        /// Process an item and correctly set the item name
        /// </summary>
        /// <param name="item">DatItem to update</param>
        /// <param name="forceRemoveQuotes">True if the Quotes flag should be ignored, false otherwise</param>
        /// <param name="forceRomName">True if the UseRomName should be always on, false otherwise</param>
        /// <remarks>
        /// There are some unique interactions that can occur because of the large number of effective
        /// inputs into this method.
        /// - If both a replacement extension is set and the remove extension flag is enabled,
        ///   the replacement extension will be overridden by the remove extension flag.
        /// - Extension addition, removal, and replacement are not done at all if the output
        ///   depot is specified. Only prefix and postfix logic is applied.
        /// - Both methods of using the item name are overridden if the output depot is specified.
        ///   Instead, the name is always set based on the SHA-1 hash.
        /// </remarks>
        protected internal void ProcessItemName(DatItem item, Machine? machine, bool forceRemoveQuotes, bool forceRomName)
        {
            // Get the relevant processing values
            bool quotes = forceRemoveQuotes ? false : Modifiers.Quotes;
            bool useRomName = forceRomName ? true : Modifiers.UseRomName;

            // Create the full Prefix
            string pre = Modifiers.Prefix + (quotes ? "\"" : string.Empty);
            pre = FormatPrefixPostfix(item, machine, pre);

            // Create the full Postfix
            string post = (quotes ? "\"" : string.Empty) + Modifiers.Postfix;
            post = FormatPrefixPostfix(item, machine, post);

            // Get the name to update
            string? name = (useRomName
                ? item.GetName()
                : machine?.GetName()) ?? string.Empty;

            // If we're in Depot mode, take care of that instead
            if (Modifiers.OutputDepot?.IsActive == true)
            {
                if (item is Disk disk)
                {
                    // We can only write out if there's a SHA-1
                    string? sha1 = disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key);
                    if (!string.IsNullOrEmpty(sha1))
                    {
                        name = Utilities.GetDepotPath(sha1, Modifiers.OutputDepot.Depth)?.Replace('\\', '/');
                        item.SetName($"{pre}{name}{post}");
                    }
                }
                else if (item is DatItems.Formats.File file)
                {
                    // We can only write out if there's a SHA-1
                    string? sha1 = file.SHA1;
                    if (!string.IsNullOrEmpty(sha1))
                    {
                        name = Utilities.GetDepotPath(sha1, Modifiers.OutputDepot.Depth)?.Replace('\\', '/');
                        item.SetName($"{pre}{name}{post}");
                    }
                }
                else if (item is Media media)
                {
                    // We can only write out if there's a SHA-1
                    string? sha1 = media.GetStringFieldValue(Models.Metadata.Media.SHA1Key);
                    if (!string.IsNullOrEmpty(sha1))
                    {
                        name = Utilities.GetDepotPath(sha1, Modifiers.OutputDepot.Depth)?.Replace('\\', '/');
                        item.SetName($"{pre}{name}{post}");
                    }
                }
                else if (item is Rom rom)
                {
                    // We can only write out if there's a SHA-1
                    string? sha1 = rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key);
                    if (!string.IsNullOrEmpty(sha1))
                    {
                        name = Utilities.GetDepotPath(sha1, Modifiers.OutputDepot.Depth)?.Replace('\\', '/');
                        item.SetName($"{pre}{name}{post}");
                    }
                }

                return;
            }

            if (!string.IsNullOrEmpty(Modifiers.ReplaceExtension) || Modifiers.RemoveExtension)
            {
                if (Modifiers.RemoveExtension)
                    Modifiers.ReplaceExtension = string.Empty;

                string? dir = Path.GetDirectoryName(name);
                if (dir != null)
                {
                    dir = dir.TrimStart(Path.DirectorySeparatorChar);
                    name = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + Modifiers.ReplaceExtension);
                }
            }

            if (!string.IsNullOrEmpty(Modifiers.AddExtension))
                name += Modifiers.AddExtension;

            if (useRomName && Modifiers.GameName)
                name = Path.Combine(machine?.GetName() ?? string.Empty, name);

            // Now assign back the formatted name
            name = $"{pre}{name}{post}";
            if (useRomName)
                item.SetName(name);
            else
                machine?.SetName(name);
        }

        /// <summary>
        /// Format a prefix or postfix string
        /// </summary>
        /// <param name="item">DatItem to create a prefix/postfix for</param>
        /// <param name="machine">Machine to get information from</param>
        /// <param name="fix">Prefix or postfix pattern to populate</param>
        /// <returns>Sanitized string representing the postfix or prefix</returns>
        protected internal static string FormatPrefixPostfix(DatItem item, Machine? machine, string fix)
        {
            // Initialize strings
            string? type = item.GetStringFieldValue(Models.Metadata.DatItem.TypeKey);
            string
                game = machine?.GetName() ?? string.Empty,
                manufacturer = machine?.GetStringFieldValue(Models.Metadata.Machine.ManufacturerKey) ?? string.Empty,
                publisher = machine?.GetStringFieldValue(Models.Metadata.Machine.PublisherKey) ?? string.Empty,
                category = machine?.GetStringFieldValue(Models.Metadata.Machine.CategoryKey) ?? string.Empty,
                name = item.GetName() ?? type.AsItemType().AsStringValue() ?? string.Empty,
                crc = string.Empty,
                md2 = string.Empty,
                md4 = string.Empty,
                md5 = string.Empty,
                ripemd128 = string.Empty,
                ripemd160 = string.Empty,
                sha1 = string.Empty,
                sha256 = string.Empty,
                sha384 = string.Empty,
                sha512 = string.Empty,
                size = string.Empty,
                spamsum = string.Empty;

            // Ensure we have the proper values for replacement
            if (item is Disk disk)
            {
                md5 = disk.GetStringFieldValue(Models.Metadata.Disk.MD5Key) ?? string.Empty;
                sha1 = disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key) ?? string.Empty;
            }
            else if (item is DatItems.Formats.File file)
            {
                name = $"{file.Id}.{file.Extension}";
                size = file.Size.ToString() ?? string.Empty;
                crc = file.CRC ?? string.Empty;
                md5 = file.MD5 ?? string.Empty;
                sha1 = file.SHA1 ?? string.Empty;
                sha256 = file.SHA256 ?? string.Empty;
            }
            else if (item is Media media)
            {
                md5 = media.GetStringFieldValue(Models.Metadata.Media.MD5Key) ?? string.Empty;
                sha1 = media.GetStringFieldValue(Models.Metadata.Media.SHA1Key) ?? string.Empty;
                sha256 = media.GetStringFieldValue(Models.Metadata.Media.SHA256Key) ?? string.Empty;
                spamsum = media.GetStringFieldValue(Models.Metadata.Media.SpamSumKey) ?? string.Empty;
            }
            else if (item is Rom rom)
            {
                crc = rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey) ?? string.Empty;
                md2 = rom.GetStringFieldValue(Models.Metadata.Rom.MD2Key) ?? string.Empty;
                md4 = rom.GetStringFieldValue(Models.Metadata.Rom.MD4Key) ?? string.Empty;
                md5 = rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key) ?? string.Empty;
                ripemd128 = rom.GetStringFieldValue("RIPEMD128") ?? string.Empty;
                ripemd160 = rom.GetStringFieldValue("RIPEMD160") ?? string.Empty;
                sha1 = rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key) ?? string.Empty;
                sha256 = rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key) ?? string.Empty;
                sha384 = rom.GetStringFieldValue(Models.Metadata.Rom.SHA384Key) ?? string.Empty;
                sha512 = rom.GetStringFieldValue(Models.Metadata.Rom.SHA512Key) ?? string.Empty;
                size = rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey).ToString() ?? string.Empty;
                spamsum = rom.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey) ?? string.Empty;
            }

            // Now do bulk replacement where possible
            fix = fix
                .Replace("%game%", game)
                .Replace("%machine%", game)
                .Replace("%name%", name)
                .Replace("%manufacturer%", manufacturer)
                .Replace("%publisher%", publisher)
                .Replace("%category%", category)
                .Replace("%crc%", crc)
                .Replace("%md2%", md2)
                .Replace("%md4%", md4)
                .Replace("%md5%", md5)
                .Replace("%ripemd128%", ripemd128)
                .Replace("%ripemd160%", ripemd160)
                .Replace("%sha1%", sha1)
                .Replace("%sha256%", sha256)
                .Replace("%sha384%", sha384)
                .Replace("%sha512%", sha512)
                .Replace("%size%", size)
                .Replace("%spamsum%", spamsum);

            return fix;
        }

        /// <summary>
        /// Process any DatItems that are "null", usually created from directory population
        /// </summary>
        /// <param name="item">DatItem to check for "null" status</param>
        /// <returns>Cleaned DatItem, if possible</returns>
        protected internal static DatItem ProcessNullifiedItem(DatItem item)
        {
            // If we don't have a Rom, we can ignore it
            if (item is not Rom rom)
                return item;

            // If the item has a size
            if (rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey) != null)
                return rom;

            // If the item CRC isn't "null"
            if (rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey) != "null")
                return rom;

            // If the Rom has "null" characteristics, ensure all fields
            rom.SetName(rom.GetName() == "null" ? "-" : rom.GetName());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SizeKey, Constants.SizeZero.ToString());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.CRCKey,
                rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey) == "null" ? ZeroHash.CRC32Str : null);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.MD2Key,
                rom.GetStringFieldValue(Models.Metadata.Rom.MD2Key) == "null" ? ZeroHash.GetString(HashType.MD2) : null);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.MD4Key,
                rom.GetStringFieldValue(Models.Metadata.Rom.MD4Key) == "null" ? ZeroHash.GetString(HashType.MD4) : null);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.MD5Key,
                rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key) == "null" ? ZeroHash.MD5Str : null);
            rom.SetFieldValue<string?>("RIPEMD128",
                rom.GetStringFieldValue("RIPEMD128") == "null" ? ZeroHash.GetString(HashType.RIPEMD128) : null);
            rom.SetFieldValue<string?>("RIPEMD160",
                rom.GetStringFieldValue("RIPEMD160") == "null" ? ZeroHash.GetString(HashType.RIPEMD160) : null);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA1Key,
                rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key) == "null" ? ZeroHash.SHA1Str : null);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA256Key,
                rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key) == "null" ? ZeroHash.SHA256Str : null);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA384Key,
                rom.GetStringFieldValue(Models.Metadata.Rom.SHA384Key) == "null" ? ZeroHash.SHA384Str : null);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA512Key,
                rom.GetStringFieldValue(Models.Metadata.Rom.SHA512Key) == "null" ? ZeroHash.SHA512Str : null);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SpamSumKey,
                rom.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey) == "null" ? ZeroHash.SpamSumStr : null);

            return rom;
        }

        /// <summary>
        /// Return list of required fields missing from a DatItem
        /// </summary>
        /// <returns>List of missing required fields, null or empty if none were found</returns>
        protected internal virtual List<string>? GetMissingRequiredFields(DatItem datItem) => null;

        /// <summary>
        /// Get if a list contains any writable items
        /// </summary>
        /// <param name="datItems">DatItems to check</param>
        /// <returns>True if the list contains at least one writable item, false otherwise</returns>
        /// <remarks>Empty list are kept with this</remarks>
        protected internal bool ContainsWritable(List<DatItem> datItems)
        {
            // Empty list are considered writable
            if (datItems.Count == 0)
                return true;

            foreach (DatItem datItem in datItems)
            {
                ItemType itemType = datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType();
                if (Array.Exists(SupportedTypes, t => t == itemType))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        /// <returns>String representing the suffix</returns>
        protected static internal string GetDuplicateSuffix(DatItem datItem)
        {
            return datItem switch
            {
                Disk diskItem => GetDuplicateSuffix(diskItem),
                DatItems.Formats.File fileItem => GetDuplicateSuffix(fileItem),
                Media mediaItem => GetDuplicateSuffix(mediaItem),
                Rom romItem => GetDuplicateSuffix(romItem),
                _ => "_1",
            };
        }

        /// <summary>
        /// Resolve name duplicates in an arbitrary set of DatItems based on the supplied information
        /// </summary>
        /// <param name="datItems">List of DatItem objects representing the items to be merged</param>
        /// <returns>A List of DatItem objects representing the renamed items</returns>
        protected internal List<DatItem> ResolveNames(List<DatItem> datItems)
        {
            // Ignore empty lists
            if (datItems.Count == 0)
                return [];

            // Create the output list
            List<DatItem> output = [];

            // First we want to make sure the list is in alphabetical order
            Sort(ref datItems, true);

            // Now we want to loop through and check names
            DatItem? lastItem = null;
            string? lastrenamed = null;
            int lastid = 0;
            for (int i = 0; i < datItems.Count; i++)
            {
                DatItem datItem = datItems[i];

                // If we have the first item, we automatically add it
                if (lastItem == null)
                {
                    output.Add(datItem);
                    lastItem = datItem;
                    continue;
                }

                // Get the last item name, if applicable
                string lastItemName = lastItem.GetName()
                    ?? lastItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue()
                    ?? string.Empty;

                // Get the current item name, if applicable
                string datItemName = datItem.GetName()
                    ?? datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue()
                    ?? string.Empty;

                // If the current item exactly matches the last item, then we don't add it
#if NET20 || NET35
                if ((Items.GetDuplicateStatus(datItem, lastItem) & DupeType.All) != 0)
#else
                if (Items.GetDuplicateStatus(datItem, lastItem).HasFlag(DupeType.All))
#endif
                {
                    _logger.Verbose($"Exact duplicate found for '{datItemName}'");
                    continue;
                }

                // If the current name matches the previous name, rename the current item
                else if (datItemName == lastItemName)
                {
                    _logger.Verbose($"Name duplicate found for '{datItemName}'");

                    // Get the duplicate suffix
                    datItemName += GetDuplicateSuffix(datItem);
                    lastrenamed ??= datItemName;

                    // If we have a conflict with the last renamed item, do the right thing
                    if (datItemName == lastrenamed)
                    {
                        lastrenamed = datItemName;
                        datItemName += (lastid == 0 ? string.Empty : "_" + lastid);
                        lastid++;
                    }
                    // If we have no conflict, then we want to reset the lastrenamed and id
                    else
                    {
                        lastrenamed = null;
                        lastid = 0;
                    }

                    // Set the item name back to the datItem
                    datItem.SetName(datItemName);

                    output.Add(datItem);
                }

                // Otherwise, we say that we have a valid named file
                else
                {
                    output.Add(datItem);
                    lastItem = datItem;
                    lastrenamed = null;
                    lastid = 0;
                }
            }

            // One last sort to make sure this is ordered
            Sort(ref output, true);

            return output;
        }

        /// <summary>
        /// Resolve name duplicates in an arbitrary set of DatItems based on the supplied information
        /// </summary>
        /// <param name="mappings">List of item ID to DatItem mappings representing the items to be merged</param>
        /// <returns>A List of DatItem objects representing the renamed items</returns>
        protected internal List<KeyValuePair<long, DatItem>> ResolveNamesDB(List<KeyValuePair<long, DatItem>> mappings)
        {
            // Ignore empty lists
            if (mappings.Count == 0)
                return [];

            // Create the output dict
            List<KeyValuePair<long, DatItem>> output = [];

            // First we want to make sure the list is in alphabetical order
            SortDB(ref mappings, true);

            // Now we want to loop through and check names
            KeyValuePair<long, DatItem>? lastItem = null;
            string? lastrenamed = null;
            int lastid = 0;
            foreach (var datItem in mappings)
            {
                // If we have the first item, we automatically add it
                if (lastItem == null)
                {
                    output.Add(datItem);
                    lastItem = datItem;
                    continue;
                }

                // Get the last item name, if applicable
                string lastItemName = lastItem.Value.Value.GetName()
                    ?? lastItem.Value.Value.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue()
                    ?? string.Empty;

                // Get the current item name, if applicable
                string datItemName = datItem.Value.GetName()
                    ?? datItem.Value.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue()
                    ?? string.Empty;

                // Get sources for both items
                var datItemSource = ItemsDB.GetSourceForItem(datItem.Key);
                var lastItemSource = ItemsDB.GetSourceForItem(lastItem.Value.Key);

                // If the current item exactly matches the last item, then we don't add it
#if NET20 || NET35
                if ((ItemsDB.GetDuplicateStatus(datItem, datItemSource.Value, lastItem, lastItemSource.Value) & DupeType.All) != 0)
#else
                if (ItemsDB.GetDuplicateStatus(datItem, datItemSource.Value, lastItem, lastItemSource.Value).HasFlag(DupeType.All))
#endif
                {
                    _logger.Verbose($"Exact duplicate found for '{datItemName}'");
                    continue;
                }

                // If the current name matches the previous name, rename the current item
                else if (datItemName == lastItemName)
                {
                    _logger.Verbose($"Name duplicate found for '{datItemName}'");

                    // Get the duplicate suffix
                    datItemName += GetDuplicateSuffix(datItem.Value);
                    lastrenamed ??= datItemName;

                    // If we have a conflict with the last renamed item, do the right thing
                    if (datItemName == lastrenamed)
                    {
                        lastrenamed = datItemName;
                        datItemName += (lastid == 0 ? string.Empty : "_" + lastid);
                        lastid++;
                    }
                    // If we have no conflict, then we want to reset the lastrenamed and id
                    else
                    {
                        lastrenamed = null;
                        lastid = 0;
                    }

                    // Set the item name back to the datItem
                    datItem.Value.SetName(datItemName);
                    output.Add(datItem);
                }

                // Otherwise, we say that we have a valid named file
                else
                {
                    output.Add(datItem);
                    lastItem = datItem;
                    lastrenamed = null;
                    lastid = 0;
                }
            }

            // One last sort to make sure this is ordered
            SortDB(ref output, true);

            return output;
        }

        /// <summary>
        /// Get if an item should be ignored on write
        /// </summary>
        /// <param name="datItem">DatItem to check</param>
        /// <param name="ignoreBlanks">True if blank roms should be skipped on output, false otherwise</param>
        /// <returns>True if the item should be skipped on write, false otherwise</returns>
        protected internal bool ShouldIgnore(DatItem? datItem, bool ignoreBlanks)
        {
            // If this is invoked with a null DatItem, we ignore
            if (datItem == null)
            {
                _logger.Verbose($"Item was skipped because it was null");
                return true;
            }

            // If the item is supposed to be removed, we ignore
            if (datItem.GetBoolFieldValue(DatItem.RemoveKey) == true)
            {
                string itemString = JsonConvert.SerializeObject(datItem, Formatting.None);
                _logger.Verbose($"Item '{itemString}' was skipped because it was marked for removal");
                return true;
            }

            // If we have the Blank dat item, we ignore
            if (datItem is Blank)
            {
                string itemString = JsonConvert.SerializeObject(datItem, Formatting.None);
                _logger.Verbose($"Item '{itemString}' was skipped because it was of type 'Blank'");
                return true;
            }

            // If we're ignoring blanks and we have a Rom
            if (ignoreBlanks && datItem is Rom rom)
            {
                // If we have a 0-size or blank rom, then we ignore
                long? size = rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey);
                if (size == 0 || size == null)
                {
                    string itemString = JsonConvert.SerializeObject(datItem, Formatting.None);
                    _logger.Verbose($"Item '{itemString}' was skipped because it had an invalid size");
                    return true;
                }
            }

            // If we have an item type not in the list of supported values
            ItemType itemType = datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType();
            if (!Array.Exists(SupportedTypes, t => t == itemType))
            {
                string itemString = JsonConvert.SerializeObject(datItem, Formatting.None);
                _logger.Verbose($"Item '{itemString}' was skipped because it was not supported for output");
                return true;
            }

            // If we have an item with missing required fields
            List<string>? missingFields = GetMissingRequiredFields(datItem);
            if (missingFields != null && missingFields.Count != 0)
            {
                string itemString = JsonConvert.SerializeObject(datItem, Formatting.None);
#if NET20 || NET35
                _logger.Verbose($"Item '{itemString}' was skipped because it was missing required fields: {string.Join(", ", [.. missingFields])}");
#else
                _logger.Verbose($"Item '{itemString}' was skipped because it was missing required fields: {string.Join(", ", missingFields)}");
#endif
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        private static string GetDuplicateSuffix(Disk datItem)
        {
            string? md5 = datItem.GetStringFieldValue(Models.Metadata.Disk.MD5Key);
            if (!string.IsNullOrEmpty(md5))
                return $"_{md5}";

            string? sha1 = datItem.GetStringFieldValue(Models.Metadata.Disk.SHA1Key);
            if (!string.IsNullOrEmpty(sha1))
                return $"_{sha1}";

            return "_1";
        }

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        /// <returns>String representing the suffix</returns>
        private static string GetDuplicateSuffix(DatItems.Formats.File datItem)
        {
            if (!string.IsNullOrEmpty(datItem.CRC))
                return $"_{datItem.CRC}";
            else if (!string.IsNullOrEmpty(datItem.MD5))
                return $"_{datItem.MD5}";
            else if (!string.IsNullOrEmpty(datItem.SHA1))
                return $"_{datItem.SHA1}";
            else if (!string.IsNullOrEmpty(datItem.SHA256))
                return $"_{datItem.SHA256}";
            else
                return "_1";
        }

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        private static string GetDuplicateSuffix(Media datItem)
        {
            string? md5 = datItem.GetStringFieldValue(Models.Metadata.Media.MD5Key);
            if (!string.IsNullOrEmpty(md5))
                return $"_{md5}";

            string? sha1 = datItem.GetStringFieldValue(Models.Metadata.Media.SHA1Key);
            if (!string.IsNullOrEmpty(sha1))
                return $"_{sha1}";

            string? sha256 = datItem.GetStringFieldValue(Models.Metadata.Media.SHA256Key);
            if (!string.IsNullOrEmpty(sha256))
                return $"_{sha256}";

            string? spamSum = datItem.GetStringFieldValue(Models.Metadata.Media.SpamSumKey);
            if (!string.IsNullOrEmpty(spamSum))
                return $"_{spamSum}";

            return "_1";
        }

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        private static string GetDuplicateSuffix(Rom datItem)
        {
            string? crc = datItem.GetStringFieldValue(Models.Metadata.Rom.CRCKey);
            if (!string.IsNullOrEmpty(crc))
                return $"_{crc}";

            string? md2 = datItem.GetStringFieldValue(Models.Metadata.Rom.MD2Key);
            if (!string.IsNullOrEmpty(md2))
                return $"_{md2}";

            string? md4 = datItem.GetStringFieldValue(Models.Metadata.Rom.MD4Key);
            if (!string.IsNullOrEmpty(md4))
                return $"_{md4}";

            string? md5 = datItem.GetStringFieldValue(Models.Metadata.Rom.MD5Key);
            if (!string.IsNullOrEmpty(md5))
                return $"_{md5}";

            string? ripemd128 = datItem.GetStringFieldValue("RIPEMD128");
            if (!string.IsNullOrEmpty(ripemd128))
                return $"_{ripemd128}";

            string? ripemd160 = datItem.GetStringFieldValue("RIPEMD160");
            if (!string.IsNullOrEmpty(ripemd160))
                return $"_{ripemd160}";

            string? sha1 = datItem.GetStringFieldValue(Models.Metadata.Rom.SHA1Key);
            if (!string.IsNullOrEmpty(sha1))
                return $"_{sha1}";

            string? sha256 = datItem.GetStringFieldValue(Models.Metadata.Rom.SHA256Key);
            if (!string.IsNullOrEmpty(sha256))
                return $"_{sha256}";

            string? sha384 = datItem.GetStringFieldValue(Models.Metadata.Rom.SHA384Key);
            if (!string.IsNullOrEmpty(sha384))
                return $"_{sha384}";

            string? sha512 = datItem.GetStringFieldValue(Models.Metadata.Rom.SHA512Key);
            if (!string.IsNullOrEmpty(sha512))
                return $"_{sha512}";

            string? spamSum = datItem.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey);
            if (!string.IsNullOrEmpty(spamSum))
                return $"_{spamSum}";

            return "_1";
        }

        /// <summary>
        /// Sort a list of DatItem objects by SourceID, Game, and Name (in order)
        /// </summary>
        /// <param name="items">List of DatItem objects representing the items to be sorted</param>
        /// <param name="norename">True if files are not renamed, false otherwise</param>
        /// <returns>True if it sorted correctly, false otherwise</returns>
        private static bool Sort(ref List<DatItem> items, bool norename)
        {
            // Create the comparer extenal to the delegate
            var nc = new NaturalComparer();

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

                    // If machine names don't match
                    string? xMachineName = x.GetMachine()?.GetName();
                    string? yMachineName = y.GetMachine()?.GetName();
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
        /// Sort a list of DatItem objects by SourceID, Game, and Name (in order)
        /// </summary>
        /// <param name="mappings">List of item ID to DatItem mappings representing the items to be sorted</param>
        /// <param name="norename">True if files are not renamed, false otherwise</param>
        /// <returns>True if it sorted correctly, false otherwise</returns>
        private bool SortDB(ref List<KeyValuePair<long, DatItem>> mappings, bool norename)
        {
            // Create the comparer extenal to the delegate
            var nc = new NaturalComparer();

            mappings.Sort(delegate (KeyValuePair<long, DatItem> x, KeyValuePair<long, DatItem> y)
            {
                try
                {
                    // Compare on source if renaming
                    if (!norename)
                    {
                        int xSourceIndex = ItemsDB.GetSourceForItem(x.Key).Value?.Index ?? 0;
                        int ySourceIndex = ItemsDB.GetSourceForItem(y.Key).Value?.Index ?? 0;
                        if (xSourceIndex != ySourceIndex)
                            return xSourceIndex - ySourceIndex;
                    }

                    // If machine names don't match
                    string? xMachineName = ItemsDB.GetMachineForItem(x.Key).Value?.GetName();
                    string? yMachineName = ItemsDB.GetMachineForItem(y.Key).Value?.GetName();
                    if (xMachineName != yMachineName)
                        return nc.Compare(xMachineName, yMachineName);

                    // If types don't match
                    string? xType = x.Value.GetStringFieldValue(Models.Metadata.DatItem.TypeKey);
                    string? yType = y.Value.GetStringFieldValue(Models.Metadata.DatItem.TypeKey);
                    if (xType != yType)
                        return xType.AsItemType() - yType.AsItemType();

                    // If directory names don't match
                    string? xDirectoryName = Path.GetDirectoryName(TextHelper.RemovePathUnsafeCharacters(x.Value.GetName() ?? string.Empty));
                    string? yDirectoryName = Path.GetDirectoryName(TextHelper.RemovePathUnsafeCharacters(y.Value.GetName() ?? string.Empty));
                    if (xDirectoryName != yDirectoryName)
                        return nc.Compare(xDirectoryName, yDirectoryName);

                    // If item names don't match
                    string? xName = Path.GetFileName(TextHelper.RemovePathUnsafeCharacters(x.Value.GetName() ?? string.Empty));
                    string? yName = Path.GetFileName(TextHelper.RemovePathUnsafeCharacters(y.Value.GetName() ?? string.Empty));
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

        #endregion
    }
}
