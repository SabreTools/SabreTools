using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Reports;
using SabreTools.Library.Skippers;
using SabreTools.Library.Tools;
using NaturalSort;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents a format-agnostic DAT
    /// </summary>
    public abstract class DatFile
    {
        #region Private instance variables

        // Internal DatHeader values
        internal DatHeader DatHeader = new DatHeader();

        // DatItems dictionary
        internal SortedDictionary<string, List<DatItem>> Items = new SortedDictionary<string, List<DatItem>>();

        // Internal statistical data
        internal DatStats DatStats = new DatStats();

        /// <summary>
        /// Determine the sorting key for all items
        /// </summary>
        private SortedBy SortedBy;

        /// <summary>
        /// Determine merging type for all items
        /// </summary>
        private DedupeType MergedBy;

        #endregion

        #region Instance Methods

        #region Accessors

        /// <summary>
        /// Passthrough to access the file dictionary
        /// </summary>
        /// <param name="key">Key in the dictionary to reference</param>
        /// <remarks>We don't want to allow direct setting of values because it bypasses the statistics</remarks>
        public List<DatItem> this[string key]
        {
            get
            {
                // Ensure the dictionary is created
                EnsureDictionary();

                lock (Items)
                {
                    // Ensure the key exists
                    EnsureKey(key);

                    // Now return the value
                    return Items[key];
                }
            }
        }

        /// <summary>
        /// Add a new key to the file dictionary
        /// </summary>
        /// <param name="key">Key in the dictionary to add</param>
        public void Add(string key)
        {
            // Ensure the dictionary is created
            EnsureDictionary();

            lock (Items)
            {
                // Ensure the key exists
                EnsureKey(key);
            }
        }

        /// <summary>
        /// Add a value to the file dictionary
        /// </summary>
        /// <param name="key">Key in the dictionary to add to</param>
        /// <param name="value">Value to add to the dictionary</param>
        public void Add(string key, DatItem value)
        {
            // Ensure the dictionary is created
            EnsureDictionary();

            // Add the key, if necessary
            Add(key);

            lock (Items)
            {
                // Now add the value
                Items[key].Add(value);

                // Now update the statistics
                DatStats.AddItem(value);
            }
        }

        /// <summary>
        /// Add a range of values to the file dictionary
        /// </summary>
        /// <param name="key">Key in the dictionary to add to</param>
        /// <param name="value">Value to add to the dictionary</param>
        public void AddRange(string key, List<DatItem> value)
        {
            // Ensure the dictionary is created
            EnsureDictionary();

            // Add the key, if necessary
            Add(key);

            lock (Items)
            {
                // Now add the value
                Items[key].AddRange(value);

                // Now update the statistics
                foreach (DatItem item in value)
                {
                    DatStats.AddItem(item);
                }
            }
        }

        /// <summary>
        /// Get if the file dictionary contains the key
        /// </summary>
        /// <param name="key">Key in the dictionary to check</param>
        /// <returns>True if the key exists, false otherwise</returns>
        public bool Contains(string key)
        {
            bool contains = false;

            // Ensure the dictionary is created
            EnsureDictionary();

            // If the key is null, we return false since keys can't be null
            if (key == null)
            {
                return contains;
            }

            lock (Items)
            {
                contains = Items.ContainsKey(key);
            }

            return contains;
        }

        /// <summary>
        /// Get if the file dictionary contains the key and value
        /// </summary>
        /// <param name="key">Key in the dictionary to check</param>
        /// <param name="value">Value in the dictionary to check</param>
        /// <returns>True if the key exists, false otherwise</returns>
        public bool Contains(string key, DatItem value)
        {
            bool contains = false;

            // Ensure the dictionary is created
            EnsureDictionary();

            // If the key is null, we return false since keys can't be null
            if (key == null)
                return contains;

            lock (Items)
            {
                if (Items.ContainsKey(key))
                    contains = Items[key].Contains(value);
            }

            return contains;
        }

        /// <summary>
        /// Get total item count statistic
        /// </summary>
        public long GetCount()
        {
            return DatStats.Count;
        }

        /// <summary>
        /// Get the FileName header value
        /// </summary>
        public string GetFileName()
        {
            return DatHeader.FileName;
        }

        /// <summary>
        /// Get the keys from the file dictionary
        /// </summary>
        /// <returns>List of the keys</returns>
        public List<string> Keys
        {
            get
            {
                // Ensure the dictionary is created
                EnsureDictionary();

                lock (Items)
                {
                    return Items.Keys.Select(item => (String)item.Clone()).ToList();
                }
            }
        }

        /// <summary>
        /// Remove a key from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove</param>
        public void Remove(string key)
        {
            // Ensure the dictionary is created
            EnsureDictionary();

            // If the key doesn't exist, return
            if (!Contains(key))
            {
                return;
            }

            lock (Items)
            {
                // Remove the statistics first
                foreach (DatItem item in Items[key])
                {
                    DatStats.RemoveItem(item);
                }

                // Remove the key from the dictionary
                Items.Remove(key);
            }
        }

        /// <summary>
        /// Remove the first instance of a value from the file dictionary if it exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove from</param>
        /// <param name="value">Value to remove from the dictionary</param>
        public void Remove(string key, DatItem value)
        {
            // Ensure the dictionary is created
            EnsureDictionary();

            // If the key and value doesn't exist, return
            if (!Contains(key, value))
            {
                return;
            }

            lock (Items)
            {
                // Remove the statistics first
                DatStats.RemoveItem(value);

                Items[key].Remove(value);
            }
        }

        /// <summary>
        /// Remove a range of values from the file dictionary if they exists
        /// </summary>
        /// <param name="key">Key in the dictionary to remove from</param>
        /// <param name="value">Value to remove from the dictionary</param>
        public void RemoveRange(string key, List<DatItem> value)
        {
            foreach (DatItem item in value)
            {
                Remove(key, item);
            }
        }

        /// <summary>
        /// Set the Date header value
        /// </summary>
        /// <param name="date"></param>
        public void SetDate(string date)
        {
            DatHeader.Date = date;
        }

        /// <summary>
        /// Set the Description header value
        /// </summary>
        public void SetDescription(string description)
        {
            DatHeader.Description = description;
        }

        /// <summary>
        /// Set the Name header value
        /// </summary>
        public void SetName(string name)
        {
            DatHeader.Name = name;
        }

        /// <summary>
        /// Set the Type header value
        /// </summary>
        public void SetType(string type)
        {
            DatHeader.Type = type;
        }

        /// <summary>
        /// Ensure the items dictionary
        /// </summary>
        private void EnsureDictionary()
        {
            // If the dictionary is null, create it
            if (Items == null)
            {
                Items = new SortedDictionary<string, List<DatItem>>();
            }
        }

        /// <summary>
        /// Ensure the key exists in the items dictionary
        /// </summary>
        /// <param name="key">Key to ensure</param>
        private void EnsureKey(string key)
        {
            // If the key is missing from the dictionary, add it
            if (!Items.ContainsKey(key))
                Items.Add(key, new List<DatItem>());
        }

        #endregion

        #region Bucketing

        /// <summary>
        /// Take the arbitrarily sorted Files Dictionary and convert to one sorted by a user-defined method
        /// </summary>
        /// <param name="bucketBy">SortedBy enum representing how to sort the individual items</param>
        /// <param name="deduperoms">Dedupe type that should be used</param>
        /// <param name="lower">True if the key should be lowercased (default), false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        public void BucketBy(SortedBy bucketBy, DedupeType deduperoms, bool lower = true, bool norename = true)
        {
            // If we have a situation where there's no dictionary or no keys at all, we skip
            if (Items == null || Items.Count == 0)
                return;

            // If the sorted type isn't the same, we want to sort the dictionary accordingly
            if (this.SortedBy != bucketBy)
            {
                Globals.Logger.User($"Organizing roms by {bucketBy}");

                // Set the sorted type
                this.SortedBy = bucketBy;

                // Reset the merged type since this might change the merge
                this.MergedBy = DedupeType.None;

                // First do the initial sort of all of the roms inplace
                List<string> oldkeys = Keys;
                for (int k = 0; k < oldkeys.Count; k++)
                {
                    string key = oldkeys[k];

                    // Get the unsorted current list
                    List<DatItem> roms = this[key];

                    // Now add each of the roms to their respective keys
                    for (int i = 0; i < roms.Count; i++)
                    {
                        DatItem rom = roms[i];

                        // We want to get the key most appropriate for the given sorting type
                        string newkey = rom.GetKey(bucketBy, lower, norename);

                        // If the key is different, move the item to the new key
                        if (newkey != key)
                        {
                            Add(newkey, rom);
                            Remove(key, rom);
                            i--; // This make sure that the pointer stays on the correct since one was removed
                        }
                    }
                }
            }

            // If the merge type isn't the same, we want to merge the dictionary accordingly
            if (this.MergedBy != deduperoms)
            {
                Globals.Logger.User($"Deduping roms by {deduperoms}");

                // Set the sorted type
                this.MergedBy = deduperoms;

                List<string> keys = Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    // Get the possibly unsorted list
                    List<DatItem> sortedlist = this[key];

                    // Sort the list of items to be consistent
                    DatItem.Sort(ref sortedlist, false);

                    // If we're merging the roms, do so
                    if (deduperoms == DedupeType.Full || (deduperoms == DedupeType.Game && bucketBy == SortedBy.Game))
                        sortedlist = DatItem.Merge(sortedlist);

                    // Add the list back to the dictionary
                    Remove(key);
                    AddRange(key, sortedlist);
                });
            }
            // If the merge type is the same, we want to sort the dictionary to be consistent
            else
            {
                List<string> keys = Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    // Get the possibly unsorted list
                    List<DatItem> sortedlist = this[key];

                    // Sort the list of items to be consistent
                    DatItem.Sort(ref sortedlist, false);
                });
            }

            // Now clean up all empty keys
            CleanEmptyKeys();
        }

        /// <summary>
        /// Clean out all empty keys in the dictionary
        /// </summary>
        private void CleanEmptyKeys()
        {
            List<string> keys = Keys;
            foreach (string key in keys)
            {
                if (this[key].Count == 0)
                    Remove(key);
            }
        }

        /// <summary>
        /// Check if a DAT contains the given DatItem
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>True if it contains the rom, false otherwise</returns>
        private bool HasDuplicates(DatItem datItem, bool sorted = false)
        {
            // Check for an empty rom list first
            if (DatStats.Count == 0)
                return false;

            // We want to get the proper key for the DatItem
            string key = SortAndGetKey(datItem, sorted);

            // If the key doesn't exist, return the empty list
            if (!Contains(key))
                return false;

            // Try to find duplicates
            List<DatItem> roms = this[key];
            return roms.Any(r => datItem.Equals(r));
        }

        /// <summary>
        /// List all duplicates found in a DAT based on a DatItem
        /// </summary>
        /// <param name="datItem">Item to try to match</param>
        /// <param name="remove">True to mark matched roms for removal from the input, false otherwise (default)</param>
        /// <param name="sorted">True if the DAT is already sorted accordingly, false otherwise (default)</param>
        /// <returns>List of matched DatItem objects</returns>
        private List<DatItem> GetDuplicates(DatItem datItem, bool remove = false, bool sorted = false)
        {
            List<DatItem> output = new List<DatItem>();

            // Check for an empty rom list first
            if (DatStats.Count == 0)
                return output;

            // We want to get the proper key for the DatItem
            string key = SortAndGetKey(datItem, sorted);

            // If the key doesn't exist, return the empty list
            if (!Contains(key))
                return output;

            // Try to find duplicates
            List<DatItem> roms = this[key];
            List<DatItem> left = new List<DatItem>();
            for (int i = 0; i < roms.Count; i++)
            {
                DatItem other = roms[i];

                if (datItem.Equals(other))
                {
                    other.Remove = true;
                    output.Add(other);
                }
                else
                {
                    left.Add(other);
                }
            }

            // If we're in removal mode, add back all roms with the proper flags
            if (remove)
            {
                Remove(key);
                AddRange(key, output);
                AddRange(key, left);
            }

            return output;
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
                BucketBy(DatStats.GetBestAvailable(), DedupeType.None);

            // Now that we have the sorted type, we get the proper key
            return datItem.GetKey(SortedBy);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new DatFile from an existing one
        /// </summary>
        /// <param name="datFile">DatFile to get the values from</param>
        /// <param name="cloneHeader">True if only the header should be cloned (default), false if this should be a reference to another DatFile</param>
        public DatFile(DatFile datFile, bool cloneHeader = true)
        {
            if (datFile != null)
            {
                if (cloneHeader)
                {
                    DatHeader = (DatHeader)datFile.DatHeader.Clone();
                    Items = new SortedDictionary<string, List<DatItem>>();
                }
                else
                {
                    DatHeader = datFile.DatHeader;
                    this.Items = datFile.Items;
                    this.SortedBy = datFile.SortedBy;
                    this.MergedBy = datFile.MergedBy;
                    this.DatStats = datFile.DatStats;
                }
            }
        }

        /// <summary>
        /// Create a specific type of DatFile to be used based on a format and a base DAT
        /// </summary>
        /// <param name="datFormat">Format of the DAT to be created</param>
        /// <param name="baseDat">DatFile containing the information to use in specific operations</param>
        /// <returns>DatFile of the specific internal type that corresponds to the inputs</returns>
        public static DatFile Create(DatFormat? datFormat = null, DatFile baseDat = null)
        {
            switch (datFormat)
            {
                case DatFormat.AttractMode:
                    return new AttractMode(baseDat);

                case DatFormat.ClrMamePro:
                    return new ClrMamePro(baseDat);

                case DatFormat.CSV:
                    return new DatFiles.SeparatedValue(baseDat, ',');

                case DatFormat.DOSCenter:
                    return new DosCenter(baseDat);

                case DatFormat.EverdriveSMDB:
                    return new EverdriveSMDB(baseDat);

                case DatFormat.Json:
                    return new Json(baseDat);

                case DatFormat.Listrom:
                    return new Listrom(baseDat);

                case DatFormat.Listxml:
                    return new Listxml(baseDat);

                case DatFormat.Logiqx:
                    return new Logiqx(baseDat, false);

                case DatFormat.LogiqxDeprecated:
                    return new Logiqx(baseDat, true);

                case DatFormat.MissFile:
                    return new Missfile(baseDat);

                case DatFormat.OfflineList:
                    return new OfflineList(baseDat);

                case DatFormat.OpenMSX:
                    return new OpenMSX(baseDat);

                case DatFormat.RedumpMD5:
                    return new Hashfile(baseDat, Hash.MD5);

#if NET_FRAMEWORK
                case DatFormat.RedumpRIPEMD160:
                    return new Hashfile(baseDat, Hash.RIPEMD160);
#endif

                case DatFormat.RedumpSFV:
                    return new Hashfile(baseDat, Hash.CRC);

                case DatFormat.RedumpSHA1:
                    return new Hashfile(baseDat, Hash.SHA1);

                case DatFormat.RedumpSHA256:
                    return new Hashfile(baseDat, Hash.SHA256);

                case DatFormat.RedumpSHA384:
                    return new Hashfile(baseDat, Hash.SHA384);

                case DatFormat.RedumpSHA512:
                    return new Hashfile(baseDat, Hash.SHA512);

                case DatFormat.RomCenter:
                    return new RomCenter(baseDat);

                case DatFormat.SabreDat:
                    return new SabreDat(baseDat);

                case DatFormat.SoftwareList:
                    return new SoftwareList(baseDat);

                case DatFormat.SSV:
                    return new DatFiles.SeparatedValue(baseDat, ';');

                case DatFormat.TSV:
                    return new DatFiles.SeparatedValue(baseDat, '\t');

                // We use new-style Logiqx as a backup for generic DatFile
                case null:
                default:
                    return new Logiqx(baseDat, false);
            }
        }

        /// <summary>
        /// Create a new DatFile from an existing DatHeader
        /// </summary>
        /// <param name="datHeader">DatHeader to get the values from</param>
        public static DatFile Create(DatHeader datHeader)
        {
            DatFile datFile = Create(datHeader.DatFormat);
            datFile.DatHeader = (DatHeader)datHeader.Clone();
            return datFile;
        }

        #endregion

        #region Converting and Updating

        /// <summary>
        /// Determine if input files should be merged, diffed, or processed invidually
        /// </summary>
        /// <param name="inputPaths">Names of the input files and/or folders</param>
        /// <param name="basePaths">Names of base files and/or folders</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="updateMode">Non-zero flag for diffing mode, zero otherwise</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
        /// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <param name="descAsName">True to use game descriptions as the names, false otherwise (default)</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
        /// <param name="updateFields">List of Fields representing what should be updated [only for base replacement]</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise [only for base replacement]</param>
        public void DetermineUpdateType(
            List<string> inputPaths,
            List<string> basePaths,
            string outDir,
            UpdateMode updateMode,
            bool inplace,
            bool skip,
            bool clean,
            bool remUnicode,
            bool descAsName,
            Filter filter,
            SplitType splitType,
            List<Field> updateFields,
            bool onlySame)
        {
            // Ensure we only have files in the inputs
            List<string> inputFileNames = DirectoryExtensions.GetFilesOnly(inputPaths, appendparent: true);
            List<string> baseFileNames = DirectoryExtensions.GetFilesOnly(basePaths);

            // If we're in standard update mode, run through all of the inputs
            if (updateMode == UpdateMode.None)
            {
                Update(inputFileNames, outDir, inplace, clean, remUnicode, descAsName, filter, splitType);
                return;
            }

            // Reverse inputs if we're in a required mode
            if ((updateMode & UpdateMode.DiffReverseCascade) != 0)
                inputFileNames.Reverse();
            if ((updateMode & UpdateMode.ReverseBaseReplace) != 0)
                baseFileNames.Reverse();

            // If we're in merging mode
            if ((updateMode & UpdateMode.Merge) != 0)
            {
                // Populate the combined data and get the headers
                PopulateUserData(inputFileNames, clean, remUnicode, descAsName, filter, splitType);
                MergeNoDiff(inputFileNames, outDir);
            }

            // If we have one of the standard diffing modes
            else if ((updateMode & UpdateMode.DiffDupesOnly) != 0
                || (updateMode & UpdateMode.DiffNoDupesOnly) != 0
                || (updateMode & UpdateMode.DiffIndividualsOnly) != 0)
            {
                // Populate the combined data
                PopulateUserData(inputFileNames, clean, remUnicode, descAsName, filter, splitType);
                DiffNoCascade(inputFileNames, outDir, updateMode);
            }

            // If we have one of the cascaded diffing modes
            else if ((updateMode & UpdateMode.DiffCascade) != 0
                || (updateMode & UpdateMode.DiffReverseCascade) != 0)
            {
                // Populate the combined data and get the headers
                List<DatFile> datHeaders = PopulateUserData(inputFileNames, clean, remUnicode, descAsName, filter, splitType);
                DiffCascade(inputFileNames, datHeaders, outDir, inplace, skip);
            }

            // If we have diff against mode
            else if ((updateMode & UpdateMode.DiffAgainst) != 0)
            {
                // Populate the combined data
                PopulateUserData(baseFileNames, clean, remUnicode, descAsName, filter, splitType);
                DiffAgainst(inputFileNames, outDir, inplace, clean, remUnicode, descAsName);
            }

            // If we have one of the base replacement modes
            else if ((updateMode & UpdateMode.BaseReplace) != 0
                || (updateMode & UpdateMode.ReverseBaseReplace) != 0)
            {
                // Populate the combined data
                PopulateUserData(baseFileNames, clean, remUnicode, descAsName, filter, splitType);
                BaseReplace(inputFileNames, outDir, inplace, clean, remUnicode, descAsName, filter, updateFields, onlySame);
            }

            return;
        }

        /// <summary>
        /// Populate the user DatData object from the input files
        /// </summary>
        /// <param name="inputs">Paths to DATs to parse</param>
        /// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <param name="descAsName">True to use game descriptions as the names, false otherwise (default)</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
        /// <returns>List of DatData objects representing headers</returns>
        private List<DatFile> PopulateUserData(
            List<string> inputs,
            bool clean,
            bool remUnicode,
            bool descAsName,
            Filter filter,
            SplitType splitType)
        {
            DatFile[] datHeaders = new DatFile[inputs.Count];
            InternalStopwatch watch = new InternalStopwatch("Processing individual DATs");

            // Parse all of the DATs into their own DatFiles in the array
            Parallel.For(0, inputs.Count, Globals.ParallelOptions, i =>
            {
                string input = inputs[i];
                Globals.Logger.User($"Adding DAT: {input.Split('¬')[0]}");
                datHeaders[i] = Create(DatHeader.DatFormat);

                // Filtering that needs to be copied over
                datHeaders[i].DatHeader.ExcludeFields = (bool[])DatHeader.ExcludeFields.Clone();
                datHeaders[i].DatHeader.OneRom = DatHeader.OneRom;
                datHeaders[i].DatHeader.KeepEmptyGames = DatHeader.KeepEmptyGames;
                datHeaders[i].DatHeader.SceneDateStrip = DatHeader.SceneDateStrip;
                datHeaders[i].DatHeader.DedupeRoms = DatHeader.DedupeRoms;
                datHeaders[i].DatHeader.Prefix = DatHeader.Prefix;
                datHeaders[i].DatHeader.Postfix = DatHeader.Postfix;
                datHeaders[i].DatHeader.AddExtension = DatHeader.AddExtension;
                datHeaders[i].DatHeader.ReplaceExtension = DatHeader.ReplaceExtension;
                datHeaders[i].DatHeader.RemoveExtension = DatHeader.RemoveExtension;
                datHeaders[i].DatHeader.GameName = DatHeader.GameName;
                datHeaders[i].DatHeader.Quotes = DatHeader.Quotes;
                datHeaders[i].DatHeader.UseRomName = DatHeader.UseRomName;

                datHeaders[i].Parse(input, i, i, splitType, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);
            });

            watch.Stop();

            watch.Start("Populating internal DAT");
            Parallel.For(0, inputs.Count, Globals.ParallelOptions, i =>
            {
                // Get the list of keys from the DAT
                List<string> keys = datHeaders[i].Keys;
                foreach (string key in keys)
                {
                    // Add everything from the key to the internal DAT
                    AddRange(key, datHeaders[i][key]);

                    // Now remove the key from the source DAT
                    datHeaders[i].Remove(key);
                }

                // Now remove the file dictionary from the source DAT to save memory
                datHeaders[i].DeleteDictionary();
            });

            // Now that we have a merged DAT, filter it
            filter.FilterDatFile(this);

            watch.Stop();

            return datHeaders.ToList();
        }

        /// <summary>
        /// Replace item values from the base set represented by the current DAT
        /// </summary>
        /// <param name="inputFileNames">Names of the input files</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <param name="descAsName">True to allow SL DATs to have game names used instead of descriptions, false otherwise (default)</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <param name="updateFields">List of Fields representing what should be updated [only for base replacement]</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise</param>
        private void BaseReplace(
            List<string> inputFileNames,
            string outDir,
            bool inplace,
            bool clean,
            bool remUnicode,
            bool descAsName,
            Filter filter,
            List<Field> updateFields,
            bool onlySame)
        {
            // Fields unique to a DatItem
            List<Field> datItemFields = new List<Field>()
            {
                Field.Name,
                Field.PartName,
                Field.PartInterface,
                Field.Features,
                Field.AreaName,
                Field.AreaSize,
                Field.BiosDescription,
                Field.Default,
                Field.Language,
                Field.Date,
                Field.Bios,
                Field.Size,
                Field.Offset,
                Field.Merge,
                Field.Region,
                Field.Index,
                Field.Writable,
                Field.Optional,
                Field.Status,

                Field.CRC,
                Field.MD5,
#if NET_FRAMEWORK
                Field.RIPEMD160,
#endif
                Field.SHA1,
                Field.SHA256,
                Field.SHA384,
                Field.SHA512,
            };

            // Fields unique to a Machine
            List<Field> machineFields = new List<Field>()
            {
                Field.MachineName,
                Field.Comment,
                Field.Description,
                Field.Year,
                Field.Manufacturer,
                Field.Publisher,
                Field.RomOf,
                Field.CloneOf,
                Field.SampleOf,
                Field.Supported,
                Field.SourceFile,
                Field.Runnable,
                Field.Board,
                Field.RebuildTo,
                Field.Devices,
                Field.SlotOptions,
                Field.Infos,
                Field.MachineType,
            };

            // We want to try to replace each item in each input DAT from the base
            foreach (string path in inputFileNames)
            {
                Globals.Logger.User($"Replacing items in '{path.Split('¬')[0]}' from the base DAT");

                // First we parse in the DAT internally
                DatFile intDat = Create(DatHeader.DatFormat);

                // Filtering that needs to be copied over
                intDat.DatHeader.ExcludeFields = (bool[])DatHeader.ExcludeFields.Clone();
                intDat.DatHeader.OneRom = DatHeader.OneRom;
                intDat.DatHeader.KeepEmptyGames = DatHeader.KeepEmptyGames;
                intDat.DatHeader.SceneDateStrip = DatHeader.SceneDateStrip;
                intDat.DatHeader.DedupeRoms = DatHeader.DedupeRoms;
                intDat.DatHeader.Prefix = DatHeader.Prefix;
                intDat.DatHeader.Postfix = DatHeader.Postfix;
                intDat.DatHeader.AddExtension = DatHeader.AddExtension;
                intDat.DatHeader.ReplaceExtension = DatHeader.ReplaceExtension;
                intDat.DatHeader.RemoveExtension = DatHeader.RemoveExtension;
                intDat.DatHeader.GameName = DatHeader.GameName;
                intDat.DatHeader.Quotes = DatHeader.Quotes;
                intDat.DatHeader.UseRomName = DatHeader.UseRomName;

                intDat.Parse(path, 1, 1, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);
                filter.FilterDatFile(intDat);

                // If we are matching based on DatItem fields of any sort
                if (updateFields.Intersect(datItemFields).Any())
                {
                    // For comparison's sake, we want to use CRC as the base ordering
                    BucketBy(SortedBy.CRC, DedupeType.Full);
                    intDat.BucketBy(SortedBy.CRC, DedupeType.None);

                    // Then we do a hashwise comparison against the base DAT
                    List<string> keys = intDat.Keys;
                    Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                    {
                        List<DatItem> datItems = intDat[key];
                        List<DatItem> newDatItems = new List<DatItem>();
                        foreach (DatItem datItem in datItems)
                        {
                            // If we have something other than a Rom or Disk, then this doesn't do anything
                            // TODO: Make this do something
                            if (datItem.ItemType != ItemType.Disk && datItem.ItemType != ItemType.Rom)
                            {
                                newDatItems.Add(datItem.Clone() as DatItem);
                                continue;
                            }

                            List<DatItem> dupes = GetDuplicates(datItem, sorted: true);
                            DatItem newDatItem = datItem.Clone() as DatItem;

                            // Cast versions of the new DatItem for use below
                            var archive = newDatItem as Archive;
                            var biosSet = newDatItem as BiosSet;
                            var blank = newDatItem as Blank;
                            var disk = newDatItem as Disk;
                            var release = newDatItem as Release;
                            var rom = newDatItem as Rom;
                            var sample = newDatItem as Sample;

                            if (dupes.Count > 0)
                            {
                                // Get the first duplicate for grabbing information from
                                var firstDupe = dupes.First();
                                var archiveDupe = firstDupe as Archive;
                                var biosSetDupe = firstDupe as BiosSet;
                                var blankDupe = firstDupe as Blank;
                                var diskDupe = firstDupe as Disk;
                                var releaseDupe = firstDupe as Release;
                                var romDupe = firstDupe as Rom;
                                var sampleDupe = firstDupe as Sample;

                                #region Non-hash fields

                                if (updateFields.Contains(Field.Name))
                                    newDatItem.Name = firstDupe.Name;

                                if (updateFields.Contains(Field.PartName))
                                    newDatItem.PartName = firstDupe.PartName;

                                if (updateFields.Contains(Field.PartInterface))
                                    newDatItem.PartInterface = firstDupe.PartInterface;

                                if (updateFields.Contains(Field.Features))
                                    newDatItem.Features = firstDupe.Features;

                                if (updateFields.Contains(Field.AreaName))
                                    newDatItem.AreaName = firstDupe.AreaName;

                                if (updateFields.Contains(Field.AreaSize))
                                    newDatItem.AreaSize = firstDupe.AreaSize;

                                if (updateFields.Contains(Field.BiosDescription))
                                {
                                    if (newDatItem.ItemType == ItemType.BiosSet)
                                        biosSet.Description = biosSetDupe.Description;
                                }

                                if (updateFields.Contains(Field.Default))
                                {
                                    if (newDatItem.ItemType == ItemType.BiosSet)
                                        biosSet.Default = biosSetDupe.Default;

                                    else if (newDatItem.ItemType == ItemType.Release)
                                        release.Default = releaseDupe.Default;
                                }

                                if (updateFields.Contains(Field.Language))
                                {
                                    if (newDatItem.ItemType == ItemType.Release)
                                        release.Language = releaseDupe.Language;
                                }

                                if (updateFields.Contains(Field.Date))
                                {
                                    if (newDatItem.ItemType == ItemType.Release)
                                        release.Date = releaseDupe.Date;

                                    else if (newDatItem.ItemType == ItemType.Rom)
                                        rom.Date = romDupe.Date;
                                }

                                if (updateFields.Contains(Field.Bios))
                                {
                                    if (newDatItem.ItemType == ItemType.Rom)
                                        rom.Bios = romDupe.Bios;
                                }

                                if (updateFields.Contains(Field.Size))
                                {
                                    if (newDatItem.ItemType == ItemType.Rom)
                                        rom.Size = romDupe.Size;
                                }

                                if (updateFields.Contains(Field.Offset))
                                {
                                    if (newDatItem.ItemType == ItemType.Rom)
                                        rom.Offset = romDupe.Offset;
                                }

                                if (updateFields.Contains(Field.Merge))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                        disk.MergeTag = diskDupe.MergeTag;

                                    else if (newDatItem.ItemType == ItemType.Rom)
                                        rom.MergeTag = romDupe.MergeTag;
                                }

                                if (updateFields.Contains(Field.Region))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                        disk.Region = diskDupe.Region;

                                    else if (newDatItem.ItemType == ItemType.Release)
                                        release.Region = releaseDupe.Region;

                                    else if (newDatItem.ItemType == ItemType.Rom)
                                        rom.Region = romDupe.Region;
                                }

                                if (updateFields.Contains(Field.Index))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                        disk.Index = diskDupe.Index;
                                }

                                if (updateFields.Contains(Field.Writable))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                        disk.Writable = diskDupe.Writable;
                                }

                                if (updateFields.Contains(Field.Optional))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                        disk.Optional = diskDupe.Optional;

                                    else if (newDatItem.ItemType == ItemType.Rom)
                                        rom.Optional = romDupe.Optional;
                                }

                                if (updateFields.Contains(Field.Status))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                        disk.ItemStatus = diskDupe.ItemStatus;

                                    else if (newDatItem.ItemType == ItemType.Rom)
                                        rom.ItemStatus = romDupe.ItemStatus;
                                }

                                #endregion

                                #region Hash fields

                                if (updateFields.Contains(Field.CRC))
                                {
                                    if (newDatItem.ItemType == ItemType.Rom)
                                    {
                                        if (string.IsNullOrEmpty(rom.CRC) && !string.IsNullOrEmpty(romDupe.CRC))
                                            rom.CRC = romDupe.CRC;
                                    }
                                }

                                if (updateFields.Contains(Field.MD5))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                    {
                                        if (string.IsNullOrEmpty(disk.MD5) && !string.IsNullOrEmpty(diskDupe.MD5))
                                            disk.MD5 = diskDupe.MD5;
                                    }

                                    if (newDatItem.ItemType == ItemType.Rom)
                                    {
                                        if (string.IsNullOrEmpty(rom.MD5) && !string.IsNullOrEmpty(romDupe.MD5))
                                            rom.MD5 = romDupe.MD5;
                                    }
                                }

#if NET_FRAMEWORK
                                if (updateFields.Contains(Field.RIPEMD160))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                    {
                                        if (string.IsNullOrEmpty(disk.RIPEMD160) && !string.IsNullOrEmpty(diskDupe.RIPEMD160))
                                            disk.RIPEMD160 = diskDupe.RIPEMD160;
                                    }

                                    if (newDatItem.ItemType == ItemType.Rom)
                                    {
                                        if (string.IsNullOrEmpty(rom.RIPEMD160) && !string.IsNullOrEmpty(romDupe.RIPEMD160))
                                            rom.RIPEMD160 = romDupe.RIPEMD160;
                                    }
                                }
#endif

                                if (updateFields.Contains(Field.SHA1))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                    {
                                        if (string.IsNullOrEmpty(disk.SHA1) && !string.IsNullOrEmpty(diskDupe.SHA1))
                                            disk.SHA1 = diskDupe.SHA1;
                                    }

                                    if (newDatItem.ItemType == ItemType.Rom)
                                    {
                                        if (string.IsNullOrEmpty(rom.SHA1) && !string.IsNullOrEmpty(romDupe.SHA1))
                                            rom.SHA1 = romDupe.SHA1;
                                    }
                                }

                                if (updateFields.Contains(Field.SHA256))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                    {
                                        if (string.IsNullOrEmpty(disk.SHA256) && !string.IsNullOrEmpty(diskDupe.SHA256))
                                            disk.SHA256 = diskDupe.SHA256;
                                    }

                                    if (newDatItem.ItemType == ItemType.Rom)
                                    {
                                        if (string.IsNullOrEmpty(rom.SHA256) && !string.IsNullOrEmpty(romDupe.SHA256))
                                            rom.SHA256 = romDupe.SHA256;
                                    }
                                }

                                if (updateFields.Contains(Field.SHA384))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                    {
                                        if (string.IsNullOrEmpty(disk.SHA384) && !string.IsNullOrEmpty(diskDupe.SHA384))
                                            disk.SHA384 = diskDupe.SHA384;
                                    }

                                    if (newDatItem.ItemType == ItemType.Rom)
                                    {
                                        if (string.IsNullOrEmpty(rom.SHA384) && !string.IsNullOrEmpty(romDupe.SHA384))
                                            rom.SHA384 = romDupe.SHA384;
                                    }
                                }

                                if (updateFields.Contains(Field.SHA512))
                                {
                                    if (newDatItem.ItemType == ItemType.Disk)
                                    {
                                        if (string.IsNullOrEmpty(disk.SHA512) && !string.IsNullOrEmpty(diskDupe.SHA512))
                                            disk.SHA512 = diskDupe.SHA512;
                                    }

                                    if (newDatItem.ItemType == ItemType.Rom)
                                    {
                                        if (string.IsNullOrEmpty(rom.SHA512) && !string.IsNullOrEmpty(romDupe.SHA512))
                                            rom.SHA512 = romDupe.SHA512;
                                    }
                                }

                                #endregion

                                // Now assign back the duplicate archive to the original
                                switch (newDatItem.ItemType)
                                {
                                    case ItemType.Archive:
                                        newDatItem = archive.Clone() as Archive;
                                        break;

                                    case ItemType.BiosSet:
                                        newDatItem = biosSet.Clone() as BiosSet;
                                        break;

                                    case ItemType.Blank:
                                        newDatItem = blank.Clone() as Blank;
                                        break;

                                    case ItemType.Disk:
                                        newDatItem = disk.Clone() as Disk;
                                        break;

                                    case ItemType.Release:
                                        newDatItem = release.Clone() as Release;
                                        break;

                                    case ItemType.Rom:
                                        newDatItem = rom.Clone() as Rom;
                                        break;

                                    case ItemType.Sample:
                                        newDatItem = sample.Clone() as Sample;
                                        break;
                                }
                            }

                            newDatItems.Add(newDatItem);
                        }

                        // Now add the new list to the key
                        intDat.Remove(key);
                        intDat.AddRange(key, newDatItems);
                    });
                }

                // If we are matching based on Machine fields of any sort
                if (updateFields.Intersect(machineFields).Any())
                {
                    // For comparison's sake, we want to use Machine Name as the base ordering
                    BucketBy(SortedBy.Game, DedupeType.Full);
                    intDat.BucketBy(SortedBy.Game, DedupeType.None);

                    // Then we do a namewise comparison against the base DAT
                    List<string> keys = intDat.Keys;
                    Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                    {
                        List<DatItem> datItems = intDat[key];
                        List<DatItem> newDatItems = new List<DatItem>();
                        foreach (DatItem datItem in datItems)
                        {
                            DatItem newDatItem = datItem.Clone() as DatItem;
                            if (Contains(key) && this[key].Count() > 0)
                            {
                                var firstDupe = this[key][0];

                                if (updateFields.Contains(Field.MachineName))
                                    newDatItem.MachineName = firstDupe.MachineName;

                                if (updateFields.Contains(Field.Comment))
                                    newDatItem.Comment = firstDupe.Comment;

                                if (updateFields.Contains(Field.Description))
                                {
                                    if (!onlySame || (onlySame && newDatItem.MachineName == newDatItem.MachineDescription))
                                        newDatItem.MachineDescription = firstDupe.MachineDescription;
                                }

                                if (updateFields.Contains(Field.Year))
                                    newDatItem.Year = firstDupe.Year;

                                if (updateFields.Contains(Field.Manufacturer))
                                    newDatItem.Manufacturer = firstDupe.Manufacturer;

                                if (updateFields.Contains(Field.Publisher))
                                    newDatItem.Publisher = firstDupe.Publisher;

                                if (updateFields.Contains(Field.RomOf))
                                    newDatItem.RomOf = firstDupe.RomOf;

                                if (updateFields.Contains(Field.CloneOf))
                                    newDatItem.CloneOf = firstDupe.CloneOf;

                                if (updateFields.Contains(Field.SampleOf))
                                    newDatItem.SampleOf = firstDupe.SampleOf;

                                if (updateFields.Contains(Field.Supported))
                                    newDatItem.Supported = firstDupe.Supported;

                                if (updateFields.Contains(Field.SourceFile))
                                    newDatItem.SourceFile = firstDupe.SourceFile;

                                if (updateFields.Contains(Field.Runnable))
                                    newDatItem.Runnable = firstDupe.Runnable;

                                if (updateFields.Contains(Field.Board))
                                    newDatItem.Board = firstDupe.Board;

                                if (updateFields.Contains(Field.RebuildTo))
                                    newDatItem.RebuildTo = firstDupe.RebuildTo;

                                if (updateFields.Contains(Field.Devices))
                                    newDatItem.Devices = firstDupe.Devices;

                                if (updateFields.Contains(Field.SlotOptions))
                                    newDatItem.SlotOptions = firstDupe.SlotOptions;

                                if (updateFields.Contains(Field.Infos))
                                    newDatItem.Infos = firstDupe.Infos;

                                if (updateFields.Contains(Field.MachineType))
                                    newDatItem.MachineType = firstDupe.MachineType;
                            }

                            newDatItems.Add(newDatItem);
                        }

                        // Now add the new list to the key
                        intDat.Remove(key);
                        intDat.AddRange(key, newDatItems);
                    });
                }

                // Determine the output path for the DAT
                string interOutDir = PathExtensions.GetOutputPath(outDir, path, inplace);

                // Once we're done, try writing out
                intDat.Write(interOutDir, overwrite: inplace);

                // Due to possible memory requirements, we force a garbage collection
                GC.Collect();
            }
        }

        /// <summary>
        /// Output diffs against a base set represented by the current DAT
        /// </summary>
        /// <param name="inputFileNames">Names of the input files</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <param name="descAsName">True to use game descriptions as the names, false otherwise (default)</param>
        private void DiffAgainst(
            List<string> inputFileNames,
            string outDir,
            bool inplace,
            bool clean,
            bool remUnicode,
            bool descAsName)
        {
            // For comparison's sake, we want to use CRC as the base ordering
            BucketBy(SortedBy.CRC, DedupeType.Full);

            // Now we want to compare each input DAT against the base
            foreach (string path in inputFileNames)
            {
                Globals.Logger.User($"Comparing '{path.Split('¬')[0]}' to base DAT");

                // First we parse in the DAT internally
                DatFile intDat = Create();
                intDat.Parse(path, 1, 1, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName);

                // For comparison's sake, we want to use CRC as the base ordering
                intDat.BucketBy(SortedBy.CRC, DedupeType.Full);

                // Then we do a hashwise comparison against the base DAT
                List<string> keys = intDat.Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> datItems = intDat[key];
                    List<DatItem> keepDatItems = new List<DatItem>();
                    foreach (DatItem datItem in datItems)
                    {
                        if (!HasDuplicates(datItem, true))
                            keepDatItems.Add(datItem);
                    }

                    // Now add the new list to the key
                    intDat.Remove(key);
                    intDat.AddRange(key, keepDatItems);
                });

                // Determine the output path for the DAT
                string interOutDir = PathExtensions.GetOutputPath(outDir, path, inplace);

                // Once we're done, try writing out
                intDat.Write(interOutDir, overwrite: inplace);

                // Due to possible memory requirements, we force a garbage collection
                GC.Collect();
            }
        }

        /// <summary>
        /// Output cascading diffs
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="datHeaders">Dat headers used optionally</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        /// <param name="inplace">True if cascaded diffs are outputted in-place, false otherwise</param>
        /// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
        private void DiffCascade(List<string> inputs, List<DatFile> datHeaders, string outDir, bool inplace, bool skip)
        {
            // Create a list of DatData objects representing output files
            List<DatFile> outDats = new List<DatFile>();

            // Loop through each of the inputs and get or create a new DatData object
            InternalStopwatch watch = new InternalStopwatch("Initializing all output DATs");

            DatFile[] outDatsArray = new DatFile[inputs.Count];
            Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
            {
                string innerpost = $" ({j} - {PathExtensions.GetNormalizedFileName(inputs[j], true)} Only)";
                DatFile diffData;

                // If we're in inplace mode or the output directory is set, take the appropriate DatData object already stored
                if (inplace || outDir != Environment.CurrentDirectory)
                {
                    diffData = datHeaders[j];
                }
                else
                {
                    diffData = Create(baseDat: this);
                    diffData.DatHeader.FileName += innerpost;
                    diffData.DatHeader.Name += innerpost;
                    diffData.DatHeader.Description += innerpost;
                }

                diffData.ResetDictionary();
                outDatsArray[j] = diffData;
            });

            outDats = outDatsArray.ToList();
            watch.Stop();

            // Then, ensure that the internal dat can be sorted in the best possible way
            BucketBy(SortedBy.CRC, DedupeType.None);

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating all output DATs");
            List<string> keys = Keys;

            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = DatItem.Merge(this[key]);

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
                    return;

                foreach (DatItem item in items)
                {
                    // There's odd cases where there are items with System ID < 0. Skip them for now
                    if (item.SystemID < 0)
                    {
                        Globals.Logger.Warning($"Item found with a <0 SystemID: {item.Name}");
                        continue;
                    }

                    outDats[item.SystemID].Add(key, item);
                }
            });

            watch.Stop();

            // Finally, loop through and output each of the DATs
            watch.Start("Outputting all created DATs");

            Parallel.For((skip ? 1 : 0), inputs.Count, Globals.ParallelOptions, j =>
            {
                string path = PathExtensions.GetOutputPath(outDir, inputs[j], inplace);

                // Try to output the file
                outDats[j].Write(path, overwrite: inplace);
            });

            watch.Stop();
        }

        /// <summary>
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        /// <param name="diff">Non-zero flag for diffing mode, zero otherwise</param>
        private void DiffNoCascade(List<string> inputs, string outDir, UpdateMode diff)
        {
            InternalStopwatch watch = new InternalStopwatch("Initializing all output DATs");

            // Default vars for use
            string post = string.Empty;
            DatFile outerDiffData = Create();
            DatFile dupeData = Create();

            // Fill in any information not in the base DAT
            if (string.IsNullOrWhiteSpace(DatHeader.FileName))
                DatHeader.FileName = "All DATs";

            if (string.IsNullOrWhiteSpace(DatHeader.Name))
                DatHeader.Name = "All DATs";

            if (string.IsNullOrWhiteSpace(DatHeader.Description))
                DatHeader.Description = "All DATs";

            // Don't have External dupes
            if ((diff & UpdateMode.DiffNoDupesOnly) != 0)
            {
                post = " (No Duplicates)";
                outerDiffData = Create(baseDat: this);
                outerDiffData.DatHeader.FileName += post;
                outerDiffData.DatHeader.Name += post;
                outerDiffData.DatHeader.Description += post;
                outerDiffData.ResetDictionary();
            }

            // Have External dupes
            if ((diff & UpdateMode.DiffDupesOnly) != 0)
            {
                post = " (Duplicates)";
                dupeData = Create(baseDat: this);
                dupeData.DatHeader.FileName += post;
                dupeData.DatHeader.Name += post;
                dupeData.DatHeader.Description += post;
                dupeData.ResetDictionary();
            }

            // Create a list of DatData objects representing individual output files
            List<DatFile> outDats = new List<DatFile>();

            // Loop through each of the inputs and get or create a new DatData object
            if ((diff & UpdateMode.DiffIndividualsOnly) != 0)
            {
                DatFile[] outDatsArray = new DatFile[inputs.Count];

                Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
                {
                    string innerpost = $" ({j} - {PathExtensions.GetNormalizedFileName(inputs[j], true)} Only)";
                    DatFile diffData = Create(baseDat: this);
                    diffData.DatHeader.FileName += innerpost;
                    diffData.DatHeader.Name += innerpost;
                    diffData.DatHeader.Description += innerpost;
                    diffData.ResetDictionary();
                    outDatsArray[j] = diffData;
                });

                outDats = outDatsArray.ToList();
            }

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating all output DATs");

            List<string> keys = Keys;
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = DatItem.Merge(this[key]);

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
                    return;

                // Loop through and add the items correctly
                foreach (DatItem item in items)
                {
                    // No duplicates
                    if ((diff & UpdateMode.DiffNoDupesOnly) != 0 || (diff & UpdateMode.DiffIndividualsOnly) != 0)
                    {
                        if ((item.DupeType & DupeType.Internal) != 0 || item.DupeType == 0x00)
                        {
                            // Individual DATs that are output
                            if ((diff & UpdateMode.DiffIndividualsOnly) != 0)
                                outDats[item.SystemID].Add(key, item);

                            // Merged no-duplicates DAT
                            if ((diff & UpdateMode.DiffNoDupesOnly) != 0)
                            {
                                DatItem newrom = item.Clone() as DatItem;
                                newrom.MachineName += $" ({Path.GetFileNameWithoutExtension(inputs[item.SystemID].Split('¬')[0])})";

                                outerDiffData.Add(key, newrom);
                            }
                        }
                    }

                    // Duplicates only
                    if ((diff & UpdateMode.DiffDupesOnly) != 0)
                    {
                        if ((item.DupeType & DupeType.External) != 0)
                        {
                            DatItem newrom = item.Clone() as DatItem;
                            newrom.MachineName += $" ({Path.GetFileNameWithoutExtension(inputs[item.SystemID].Split('¬')[0])})";

                            dupeData.Add(key, newrom);
                        }
                    }
                }
            });

            watch.Stop();

            // Finally, loop through and output each of the DATs
            watch.Start("Outputting all created DATs");

            // Output the difflist (a-b)+(b-a) diff
            if ((diff & UpdateMode.DiffNoDupesOnly) != 0)
                outerDiffData.Write(outDir, overwrite: false);

            // Output the (ab) diff
            if ((diff & UpdateMode.DiffDupesOnly) != 0)
                dupeData.Write(outDir, overwrite: false);

            // Output the individual (a-b) DATs
            if ((diff & UpdateMode.DiffIndividualsOnly) != 0)
            {
                Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
                {
                    string path = PathExtensions.GetOutputPath(outDir, inputs[j], false /* inplace */);

                    // Try to output the file
                    outDats[j].Write(path, overwrite: false);
                });
            }

            watch.Stop();
        }

        /// <summary>
        /// Output user defined merge
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        private void MergeNoDiff(List<string> inputs, string outDir)
        {
            // If we're in SuperDAT mode, prefix all games with their respective DATs
            if (DatHeader.Type == "SuperDAT")
            {
                List<string> keys = Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = this[key].ToList();
                    List<DatItem> newItems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        DatItem newItem = item;
                        string filename = inputs[newItem.SystemID].Split('¬')[0];
                        string rootpath = inputs[newItem.SystemID].Split('¬')[1];

                        rootpath += (string.IsNullOrWhiteSpace(rootpath) ? string.Empty : Path.DirectorySeparatorChar.ToString());
                        filename = filename.Remove(0, rootpath.Length);
                        newItem.MachineName = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
                            + Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
                            + newItem.MachineName;

                        newItems.Add(newItem);
                    }

                    Remove(key);
                    AddRange(key, newItems);
                });
            }

            // Try to output the file
            Write(outDir, overwrite: false);
        }

        /// <summary>
        /// Convert, update, and filter a DAT file or set of files
        /// </summary>
        /// <param name="inputFileNames">Names of the input files and/or folders</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <param name="descAsName">True to use game descriptions as the names, false otherwise (default)</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
        private void Update(
            List<string> inputFileNames,
            string outDir,
            bool inplace,
            bool clean,
            bool remUnicode,
            bool descAsName,
            Filter filter,
            SplitType splitType)
        {
            // Iterate over the files
            foreach (string file in inputFileNames)
            {
                DatFile innerDatdata = Create(baseDat: this);
                Globals.Logger.User($"Processing '{Path.GetFileName(file.Split('¬')[0])}'");
                innerDatdata.Parse(file, 0, 0, splitType, keep: true, clean: clean, remUnicode: remUnicode, descAsName: descAsName,
                    keepext: ((innerDatdata.DatHeader.DatFormat & DatFormat.TSV) != 0
                        || (innerDatdata.DatHeader.DatFormat & DatFormat.CSV) != 0
                        || (innerDatdata.DatHeader.DatFormat & DatFormat.SSV) != 0));
                filter.FilterDatFile(innerDatdata);

                // Get the correct output path
                string realOutDir = PathExtensions.GetOutputPath(outDir, file, inplace);

                // Try to output the file, overwriting only if it's not in the current directory
                innerDatdata.Write(realOutDir, overwrite: inplace);
            }
        }

        #endregion

        #region Dictionary Manipulation

        /// <summary>
        /// Delete the file dictionary
        /// </summary>
        private void DeleteDictionary()
        {
            Items = null;
            this.SortedBy = SortedBy.Default;
            this.MergedBy = DedupeType.None;

            // Reset statistics
            DatStats.Reset();
        }

        /// <summary>
        /// Reset the file dictionary
        /// </summary>
        private void ResetDictionary()
        {
            Items = new SortedDictionary<string, List<DatItem>>();
            this.SortedBy = SortedBy.Default;
            this.MergedBy = DedupeType.None;

            // Reset statistics
            DatStats.Reset();
        }

        #endregion

        #region Filtering

        /// <summary>
        /// Use game descriptions as names in the DAT, updating cloneof/romof/sampleof
        /// </summary>
        private void MachineDescriptionToName()
        {
            try
            {
                // First we want to get a mapping for all games to description
                ConcurrentDictionary<string, string> mapping = new ConcurrentDictionary<string, string>();
                List<string> keys = Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = this[key];
                    foreach (DatItem item in items)
                    {
                        // If the key mapping doesn't exist, add it
                        if (!mapping.ContainsKey(item.MachineName))
                            mapping.TryAdd(item.MachineName, item.MachineDescription.Replace('/', '_').Replace("\"", "''").Replace(":", " -"));
                    }
                });

                // Now we loop through every item and update accordingly
                keys = Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = this[key];
                    List<DatItem> newItems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        // Update machine name
                        if (!string.IsNullOrWhiteSpace(item.MachineName) && mapping.ContainsKey(item.MachineName))
                            item.MachineName = mapping[item.MachineName];

                        // Update cloneof
                        if (!string.IsNullOrWhiteSpace(item.CloneOf) && mapping.ContainsKey(item.CloneOf))
                            item.CloneOf = mapping[item.CloneOf];

                        // Update romof
                        if (!string.IsNullOrWhiteSpace(item.RomOf) && mapping.ContainsKey(item.RomOf))
                            item.RomOf = mapping[item.RomOf];

                        // Update sampleof
                        if (!string.IsNullOrWhiteSpace(item.SampleOf) && mapping.ContainsKey(item.SampleOf))
                            item.SampleOf = mapping[item.SampleOf];

                        // Add the new item to the output list
                        newItems.Add(item);
                    }

                    // Replace the old list of roms with the new one
                    Remove(key);
                    AddRange(key, newItems);
                });
            }
            catch (Exception ex)
            {
                Globals.Logger.Warning(ex.ToString());
            }
        }

        /// <summary>
        /// Ensure that all roms are in their own game (or at least try to ensure)
        /// </summary>
        private void OneRomPerGame()
        {
            // For each rom, we want to update the game to be "<game name>/<rom name>"
            Parallel.ForEach(Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = this[key];
                for (int i = 0; i < items.Count; i++)
                {
                    string[] splitname = items[i].Name.Split('.');
                    items[i].MachineName += $"/{string.Join(".", splitname.Take(splitname.Length > 1 ? splitname.Length - 1 : 1))}";
                }
            });
        }

        /// <summary>
        /// Remove all items marked for removal from the DAT
        /// </summary>
        private void RemoveMarkedItems()
        {
            List<string> keys = Keys;
            foreach (string key in keys)
            {
                List<DatItem> items = this[key];
                List<DatItem> newItems = items.Where(i => !i.Remove).ToList();

                Remove(key);
                AddRange(key, newItems);
            }
        }

        /// <summary>
        /// Strip the dates from the beginning of scene-style set names
        /// </summary>
        private void StripSceneDatesFromItems()
        {
            // Output the logging statement
            Globals.Logger.User("Stripping scene-style dates");

            // Set the regex pattern to use
            string pattern = @"([0-9]{2}\.[0-9]{2}\.[0-9]{2}-)(.*?-.*?)";

            // Now process all of the roms
            List<string> keys = Keys;
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = this[key];
                for (int j = 0; j < items.Count; j++)
                {
                    DatItem item = items[j];
                    if (Regex.IsMatch(item.MachineName, pattern))
                        item.MachineName = Regex.Replace(item.MachineName, pattern, "$2");

                    if (Regex.IsMatch(item.MachineDescription, pattern))
                        item.MachineDescription = Regex.Replace(item.MachineDescription, pattern, "$2");

                    items[j] = item;
                }

                Remove(key);
                AddRange(key, items);
            });
        }

        #endregion

        #region Internal Merging/Splitting

        /// <summary>
        /// Use cdevice_ref tags to get full non-merged sets and remove parenting tags
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateDeviceNonMergedSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating device non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is sort by game
            BucketBy(SortedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddRomsFromDevices(false, false)) ;
            while (AddRomsFromDevices(true, false)) ;

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags plus using the device_ref tags to get full sets
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateFullyNonMergedSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating fully non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is sort by game
            BucketBy(SortedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddRomsFromDevices(true, true)) ;
            AddRomsFromDevices(false, true);
            AddRomsFromParent();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            AddRomsFromBios();

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use cloneof tags to create merged sets and remove the tags
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateMergedSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating merged sets from the DAT");

            // For sake of ease, the first thing we want to do is sort by game
            BucketBy(SortedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddRomsFromChildren();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(false);
            RemoveBiosRomsFromChild(true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateNonMergedSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is sort by game
            BucketBy(SortedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddRomsFromParent();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(false);
            RemoveBiosRomsFromChild(true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use cloneof and romof tags to create split sets and remove the tags
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateSplitSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating split sets from the DAT");

            // For sake of ease, the first thing we want to do is sort by game
            BucketBy(SortedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            RemoveRomsFromChild();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(false);
            RemoveBiosRomsFromChild(true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use romof tags to add roms to the children
        /// </summary>
        private void AddRomsFromBios()
        {
            List<string> games = Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (this[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(this[game][0].RomOf))
                    parent = this[game][0].RomOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (this[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = this[game][0];
                List<DatItem> parentItems = this[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (this[game].Where(i => i.Name == datItem.Name).Count() == 0 && !this[game].Contains(datItem))
                        Add(game, datItem);
                }
            }
        }

        /// <summary>
        /// Use device_ref and optionally slotoption tags to add roms to the children
        /// </summary>
        /// <param name="dev">True if only child device sets are touched, false for non-device sets (default)</param>
        /// <param name="slotoptions">True if slotoptions tags are used as well, false otherwise</param>
        private bool AddRomsFromDevices(bool dev = false, bool slotoptions = false)
        {
            bool foundnew = false;
            List<string> games = Keys;
            foreach (string game in games)
            {
                // If the game doesn't have items, we continue
                if (this[game] == null || this[game].Count == 0)
                    continue;

                // If the game (is/is not) a bios, we want to continue
                if (dev ^ (this[game][0].MachineType & MachineType.Device) != 0)
                    continue;

                // If the game has no devices, we continue
                if (this[game][0].Devices == null
                    || this[game][0].Devices.Count == 0
                    || (slotoptions && this[game][0].SlotOptions == null)
                    || (slotoptions && this[game][0].SlotOptions.Count == 0))
                {
                    continue;
                }

                // Determine if the game has any devices or not
                List<string> devices = this[game][0].Devices;
                List<string> newdevs = new List<string>();
                foreach (string device in devices)
                {
                    // If the device doesn't exist then we continue
                    if (this[device].Count == 0)
                        continue;

                    // Otherwise, copy the items from the device to the current game
                    DatItem copyFrom = this[game][0];
                    List<DatItem> devItems = this[device];
                    foreach (DatItem item in devItems)
                    {
                        DatItem datItem = (DatItem)item.Clone();
                        newdevs.AddRange(datItem.Devices ?? new List<string>());
                        datItem.CopyMachineInformation(copyFrom);
                        if (this[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0)
                        {
                            foundnew = true;
                            Add(game, datItem);
                        }
                    }
                }

                // Now that every device is accounted for, add the new list of devices, if they don't already exist
                foreach (string device in newdevs)
                {
                    if (!this[game][0].Devices.Contains(device))
                        this[game][0].Devices.Add(device);
                }

                // If we're checking slotoptions too
                if (slotoptions)
                {
                    // Determine if the game has any slotoptions or not
                    List<string> slotopts = this[game][0].SlotOptions;
                    List<string> newslotopts = new List<string>();
                    foreach (string slotopt in slotopts)
                    {
                        // If the slotoption doesn't exist then we continue
                        if (this[slotopt].Count == 0)
                            continue;

                        // Otherwise, copy the items from the slotoption to the current game
                        DatItem copyFrom = this[game][0];
                        List<DatItem> slotItems = this[slotopt];
                        foreach (DatItem item in slotItems)
                        {
                            DatItem datItem = (DatItem)item.Clone();
                            newslotopts.AddRange(datItem.SlotOptions ?? new List<string>());
                            datItem.CopyMachineInformation(copyFrom);
                            if (this[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0)
                            {
                                foundnew = true;
                                Add(game, datItem);
                            }
                        }
                    }

                    // Now that every slotoption is accounted for, add the new list of slotoptions, if they don't already exist
                    foreach (string slotopt in newslotopts)
                    {
                        if (!this[game][0].SlotOptions.Contains(slotopt))
                            this[game][0].SlotOptions.Add(slotopt);
                    }
                }
            }

            return foundnew;
        }

        /// <summary>
        /// Use cloneof tags to add roms to the children, setting the new romof tag in the process
        /// </summary>
        private void AddRomsFromParent()
        {
            List<string> games = Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (this[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(this[game][0].CloneOf))
                    parent = this[game][0].CloneOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (this[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = this[game][0];
                List<DatItem> parentItems = this[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (this[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0
                        && !this[game].Contains(datItem))
                    {
                        Add(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the items
                List<DatItem> items = this[game];
                string romof = this[parent][0].RomOf;
                foreach (DatItem item in items)
                {
                    item.RomOf = romof;
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to add roms to the parents, removing the child sets in the process
        /// </summary>
        private void AddRomsFromChildren()
        {
            List<string> games = Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (this[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(this[game][0].CloneOf))
                    parent = this[game][0].CloneOf;

                // If there is no parent, then we continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // Otherwise, move the items from the current game to a subfolder of the parent game
                DatItem copyFrom = this[parent].Count == 0 ? new Rom { MachineName = parent, MachineDescription = parent } : this[parent][0];
                List<DatItem> items = this[game];
                foreach (DatItem item in items)
                {
                    // If the disk doesn't have a valid merge tag OR the merged file doesn't exist in the parent, then add it
                    if (item.ItemType == ItemType.Disk && (((Disk)item).MergeTag == null || !this[parent].Select(i => i.Name).Contains(((Disk)item).MergeTag)))
                    {
                        item.CopyMachineInformation(copyFrom);
                        Add(parent, item);
                    }

                    // Otherwise, if the parent doesn't already contain the non-disk (or a merge-equivalent), add it
                    else if (item.ItemType != ItemType.Disk && !this[parent].Contains(item))
                    {
                        // Rename the child so it's in a subfolder
                        item.Name = $"{item.MachineName}\\{item.Name}";

                        // Update the machine to be the new parent
                        item.CopyMachineInformation(copyFrom);

                        // Add the rom to the parent set
                        Add(parent, item);
                    }
                }

                // Then, remove the old game so it's not picked up by the writer
                Remove(game);
            }
        }

        /// <summary>
        /// Remove all BIOS and device sets
        /// </summary>
        private void RemoveBiosAndDeviceSets()
        {
            List<string> games = Keys;
            foreach (string game in games)
            {
                if (this[game].Count > 0
                    && ((this[game][0].MachineType & MachineType.Bios) != 0
                        || (this[game][0].MachineType & MachineType.Device) != 0))
                {
                    Remove(game);
                }
            }
        }

        /// <summary>
        /// Use romof tags to remove bios roms from children
        /// </summary>
        /// <param name="bios">True if only child Bios sets are touched, false for non-bios sets (default)</param>
        private void RemoveBiosRomsFromChild(bool bios = false)
        {
            // Loop through the romof tags
            List<string> games = Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (this[game].Count == 0)
                    continue;

                // If the game (is/is not) a bios, we want to continue
                if (bios ^ (this[game][0].MachineType & MachineType.Bios) != 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(this[game][0].RomOf))
                    parent = this[game][0].RomOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (this[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we remove the items that are in the parent from the current game
                List<DatItem> parentItems = this[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (this[game].Contains(datItem))
                    {
                        Remove(game, datItem);
                    }
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to remove roms from the children
        /// </summary>
        private void RemoveRomsFromChild()
        {
            List<string> games = Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (this[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(this[game][0].CloneOf))
                    parent = this[game][0].CloneOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (this[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we remove the parent items from the current game
                List<DatItem> parentItems = this[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (this[game].Contains(datItem))
                    {
                        Remove(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the remaining items
                List<DatItem> items = this[game];
                string romof = this[parent][0].RomOf;
                foreach (DatItem item in items)
                {
                    item.RomOf = romof;
                }
            }
        }

        /// <summary>
        /// Remove all romof and cloneof tags from all games
        /// </summary>
        private void RemoveTagsFromChild()
        {
            List<string> games = Keys;
            foreach (string game in games)
            {
                List<DatItem> items = this[game];
                foreach (DatItem item in items)
                {
                    item.CloneOf = null;
                    item.RomOf = null;
                }
            }
        }

        #endregion

        // TODO: Can there be a static parse that does the same thing as Create?
        #region Parsing

        /// <summary>
        /// Create a DatFile and parse a file into it
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="datdata">The DatData object representing found roms to this point</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
        /// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
        /// <param name="useTags">True if tags from the DAT should be used to merge the output, false otherwise (default)</param>
        public static DatFile CreateAndParse(string filename, int sysid, int srcid, bool keep = false, bool clean = false,
            bool remUnicode = false, bool descAsName = false, bool keepext = false, bool useTags = false)
        {
            DatFile datFile = Create();
            datFile.Parse(filename, sysid, srcid, keep, clean, remUnicode, descAsName, keepext, useTags);
            return datFile;
        }

        /// <summary>
        /// Parse a DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="datdata">The DatData object representing found roms to this point</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
        /// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
        /// <param name="useTags">True if tags from the DAT should be used to merge the output, false otherwise (default)</param>
        public void Parse(string filename, int sysid, int srcid, bool keep = false, bool clean = false,
            bool remUnicode = false, bool descAsName = false, bool keepext = false, bool useTags = false)
        {
            Parse(filename, sysid, srcid, SplitType.None, keep: keep, clean: clean,
                remUnicode: remUnicode, descAsName: descAsName, keepext: keepext, useTags: useTags);
        }

        /// <summary>
        /// Parse a DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>>
        /// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
        /// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
        /// <param name="useTags">True if tags from the DAT should be used to merge the output, false otherwise (default)</param>
        public void Parse(
            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Rom renaming
            SplitType splitType,

            // Miscellaneous
            bool keep = false,
            bool clean = false,
            bool remUnicode = false,
            bool descAsName = false,
            bool keepext = false,
            bool useTags = false)
        {
            // Check if we have a split path and get the filename accordingly
            if (filename.Contains("¬"))
                filename = filename.Split('¬')[0];

            // Check the file extension first as a safeguard
            if (!PathExtensions.HasValidDatExtension(filename))
                return;

            // If the output filename isn't set already, get the internal filename
            DatHeader.FileName = (string.IsNullOrWhiteSpace(DatHeader.FileName) ? (keepext ? Path.GetFileName(filename) : Path.GetFileNameWithoutExtension(filename)) : DatHeader.FileName);

            // If the output type isn't set already, get the internal output type
            DatHeader.DatFormat = (DatHeader.DatFormat == 0 ? filename.GetDatFormat() : DatHeader.DatFormat);
            this.SortedBy = SortedBy.CRC; // Setting this because it can reduce issues later

            // Now parse the correct type of DAT
            try
            {
                Create(filename.GetDatFormat(), this)?.ParseFile(filename, sysid, srcid, keep, clean, remUnicode);
            }
            catch (Exception ex)
            {
                Globals.Logger.Error($"Error with file '{filename}': {ex}");
            }

            // If we want to use descriptions as names, update everything
            if (descAsName)
                MachineDescriptionToName();

            // If we are using tags from the DAT, set the proper input for split type unless overridden
            if (useTags && splitType == SplitType.None)
                splitType = DatHeader.ForceMerging.AsSplitType();

            // Now we pre-process the DAT with the splitting/merging mode
            switch (splitType)
            {
                case SplitType.None:
                    // No-op
                    break;
                case SplitType.DeviceNonMerged:
                    CreateDeviceNonMergedSets(DedupeType.None);
                    break;
                case SplitType.FullNonMerged:
                    CreateFullyNonMergedSets(DedupeType.None);
                    break;
                case SplitType.NonMerged:
                    CreateNonMergedSets(DedupeType.None);
                    break;
                case SplitType.Merged:
                    CreateMergedSets(DedupeType.None);
                    break;
                case SplitType.Split:
                    CreateSplitSets(DedupeType.None);
                    break;
            }

            // Finally, we remove any blanks, if we aren't supposed to have any
            if (!DatHeader.KeepEmptyGames)
            {
                foreach (string key in Keys)
                {
                    List<DatItem> items = this[key];
                    List<DatItem> newitems = items.Where(i => i.ItemType != ItemType.Blank).ToList();

                    this.Remove(key);
                    this.AddRange(key, newitems);
                }
            }
        }

        /// <summary>
        /// Add a rom to the Dat after checking
        /// </summary>
        /// <param name="item">Item data to check against</param>
        /// <param name="clean">True if the names should be cleaned to WoD standards, false otherwise</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <returns>The key for the item</returns>
        protected string ParseAddHelper(DatItem item, bool clean, bool remUnicode)
        {
            string key = string.Empty;

            // If there's no name in the rom, we log and skip it
            if (item.Name == null)
            {
                Globals.Logger.Warning($"{DatHeader.FileName}: Rom with no name found! Skipping...");
                return key;
            }

            // If we're in cleaning mode, sanitize the game name
            item.MachineName = (clean ? Sanitizer.CleanGameName(item.MachineName) : item.MachineName);

            // If we're stripping unicode characters, do so from all relevant things
            if (remUnicode)
            {
                item.Name = Sanitizer.RemoveUnicodeCharacters(item.Name);
                item.MachineName = Sanitizer.RemoveUnicodeCharacters(item.MachineName);
                item.MachineDescription = Sanitizer.RemoveUnicodeCharacters(item.MachineDescription);
            }

            // If we have a Rom or a Disk, clean the hash data
            if (item.ItemType == ItemType.Rom)
            {
                Rom itemRom = (Rom)item;

                // Sanitize the hashes from null, hex sizes, and "true blank" strings
                itemRom.CRC = Sanitizer.CleanCRC32(itemRom.CRC);
                itemRom.MD5 = Sanitizer.CleanMD5(itemRom.MD5);
#if NET_FRAMEWORK
                itemRom.RIPEMD160 = Sanitizer.CleanRIPEMD160(itemRom.RIPEMD160);
#endif
                itemRom.SHA1 = Sanitizer.CleanSHA1(itemRom.SHA1);
                itemRom.SHA256 = Sanitizer.CleanSHA256(itemRom.SHA256);
                itemRom.SHA384 = Sanitizer.CleanSHA384(itemRom.SHA384);
                itemRom.SHA512 = Sanitizer.CleanSHA512(itemRom.SHA512);

                // If we have the case where there is SHA-1 and nothing else, we don't fill in any other part of the data
                if (itemRom.Size == -1
                    && string.IsNullOrWhiteSpace(itemRom.CRC)
                    && string.IsNullOrWhiteSpace(itemRom.MD5)
#if NET_FRAMEWORK
                    && string.IsNullOrWhiteSpace(itemRom.RIPEMD160)
#endif
                    && !string.IsNullOrWhiteSpace(itemRom.SHA1)
                    && string.IsNullOrWhiteSpace(itemRom.SHA256)
                    && string.IsNullOrWhiteSpace(itemRom.SHA384)
                    && string.IsNullOrWhiteSpace(itemRom.SHA512))
                {
                    // No-op, just catch it so it doesn't go further
                    Globals.Logger.Verbose($"{DatHeader.FileName}: Entry with only SHA-1 found - '{itemRom.Name}'");
                }

                // If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
                else if ((itemRom.Size == 0 || itemRom.Size == -1)
                    && ((itemRom.CRC == Constants.CRCZero || string.IsNullOrWhiteSpace(itemRom.CRC))
                        || itemRom.MD5 == Constants.MD5Zero
#if NET_FRAMEWORK
                        || itemRom.RIPEMD160 == Constants.RIPEMD160Zero
#endif
                        || itemRom.SHA1 == Constants.SHA1Zero
                        || itemRom.SHA256 == Constants.SHA256Zero
                        || itemRom.SHA384 == Constants.SHA384Zero
                        || itemRom.SHA512 == Constants.SHA512Zero))
                {
                    // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                    itemRom.Size = Constants.SizeZero;
                    itemRom.CRC = Constants.CRCZero;
                    itemRom.MD5 = Constants.MD5Zero;
#if NET_FRAMEWORK
                    itemRom.RIPEMD160 = null;
                    //itemRom.RIPEMD160 = Constants.RIPEMD160Zero;
#endif
                    itemRom.SHA1 = Constants.SHA1Zero;
                    itemRom.SHA256 = null;
                    //itemRom.SHA256 = Constants.SHA256Zero;
                    itemRom.SHA384 = null;
                    //itemRom.SHA384 = Constants.SHA384Zero;
                    itemRom.SHA512 = null;
                    //itemRom.SHA512 = Constants.SHA512Zero;
                }
                // If the file has no size and it's not the above case, skip and log
                else if (itemRom.ItemStatus != ItemStatus.Nodump && (itemRom.Size == 0 || itemRom.Size == -1))
                {
                    Globals.Logger.Verbose($"{DatHeader.FileName}: Incomplete entry for '{itemRom.Name}' will be output as nodump");
                    itemRom.ItemStatus = ItemStatus.Nodump;
                }
                // If the file has a size but aboslutely no hashes, skip and log
                else if (itemRom.ItemStatus != ItemStatus.Nodump
                    && itemRom.Size > 0
                    && string.IsNullOrWhiteSpace(itemRom.CRC)
                    && string.IsNullOrWhiteSpace(itemRom.MD5)
#if NET_FRAMEWORK
                    && string.IsNullOrWhiteSpace(itemRom.RIPEMD160)
#endif
                    && string.IsNullOrWhiteSpace(itemRom.SHA1)
                    && string.IsNullOrWhiteSpace(itemRom.SHA256)
                    && string.IsNullOrWhiteSpace(itemRom.SHA384)
                    && string.IsNullOrWhiteSpace(itemRom.SHA512))
                {
                    Globals.Logger.Verbose($"{DatHeader.FileName}: Incomplete entry for '{itemRom.Name}' will be output as nodump");
                    itemRom.ItemStatus = ItemStatus.Nodump;
                }

                item = itemRom;
            }
            else if (item.ItemType == ItemType.Disk)
            {
                Disk itemDisk = (Disk)item;

                // Sanitize the hashes from null, hex sizes, and "true blank" strings
                itemDisk.MD5 = Sanitizer.CleanMD5(itemDisk.MD5);
#if NET_FRAMEWORK
                itemDisk.RIPEMD160 = Sanitizer.CleanRIPEMD160(itemDisk.RIPEMD160);
#endif
                itemDisk.SHA1 = Sanitizer.CleanSHA1(itemDisk.SHA1);
                itemDisk.SHA256 = Sanitizer.CleanSHA256(itemDisk.SHA256);
                itemDisk.SHA384 = Sanitizer.CleanSHA384(itemDisk.SHA384);
                itemDisk.SHA512 = Sanitizer.CleanSHA512(itemDisk.SHA512);

                // If the file has aboslutely no hashes, skip and log
                if (itemDisk.ItemStatus != ItemStatus.Nodump
                    && string.IsNullOrWhiteSpace(itemDisk.MD5)
#if NET_FRAMEWORK
                    && string.IsNullOrWhiteSpace(itemDisk.RIPEMD160)
#endif
                    && string.IsNullOrWhiteSpace(itemDisk.SHA1)
                    && string.IsNullOrWhiteSpace(itemDisk.SHA256)
                    && string.IsNullOrWhiteSpace(itemDisk.SHA384)
                    && string.IsNullOrWhiteSpace(itemDisk.SHA512))
                {
                    Globals.Logger.Verbose($"Incomplete entry for '{itemDisk.Name}' will be output as nodump");
                    itemDisk.ItemStatus = ItemStatus.Nodump;
                }

                item = itemDisk;
            }

            // Get the key and add the file
            key = item.GetKey(SortedBy.CRC);
            Add(key, item);

            return key;
        }

        /// <summary>
        /// Add a rom to the Dat after checking
        /// </summary>
        /// <param name="item">Item data to check against</param>
        /// <param name="clean">True if the names should be cleaned to WoD standards, false otherwise</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <returns>The key for the item</returns>
        protected async Task<string> ParseAddHelperAsync(DatItem item, bool clean, bool remUnicode)
        {
            return await Task.Run(() => ParseAddHelper(item, clean, remUnicode));
        }

        /// <summary>
        /// Parse DatFile and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="sysid">System ID for the DAT</param>
        /// <param name="srcid">Source ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="clean">True if game names are sanitized, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        protected abstract void ParseFile(
            // Standard Dat parsing
            string filename,
            int sysid,
            int srcid,

            // Miscellaneous
            bool keep,
            bool clean,
            bool remUnicode);

        #endregion

        #region Populate DAT from Directory

        /// <summary>
        /// Create a new Dat from a directory
        /// </summary>
        /// <param name="basePath">Base folder to be used in creating the DAT</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
        /// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
        /// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
        /// <param name="skipFileType">Type of files that should be skipped</param>
        /// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
        /// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
        /// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
        /// <param name="outDir">Output directory to </param>
        /// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
        /// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        public bool PopulateFromDir(string basePath, Hash omitFromScan, bool bare, bool archivesAsFiles, SkipFileType skipFileType,
            bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst, bool chdsAsFiles, Filter filter)
        {
            // If the description is defined but not the name, set the name from the description
            if (string.IsNullOrWhiteSpace(DatHeader.Name) && !string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                DatHeader.Name = DatHeader.Description;
            }

            // If the name is defined but not the description, set the description from the name
            else if (!string.IsNullOrWhiteSpace(DatHeader.Name) && string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                DatHeader.Description = DatHeader.Name + (bare ? string.Empty : $" ({DatHeader.Date})");
            }

            // If neither the name or description are defined, set them from the automatic values
            else if (string.IsNullOrWhiteSpace(DatHeader.Name) && string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                string[] splitpath = basePath.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
                DatHeader.Name = splitpath.Last();
                DatHeader.Description = DatHeader.Name + (bare ? string.Empty : $" ({DatHeader.Date})");
            }

            // Clean the temp directory path
            tempDir = DirectoryExtensions.Ensure(tempDir, temp: true);

            // Process the input
            if (Directory.Exists(basePath))
            {
                Globals.Logger.Verbose($"Folder found: {basePath}");

                // Process the files in the main folder or any subfolder
                List<string> files = Directory.EnumerateFiles(basePath, "*", SearchOption.AllDirectories).ToList();
                Parallel.ForEach(files, Globals.ParallelOptions, item =>
                {
                    CheckFileForHashes(item, basePath, omitFromScan, archivesAsFiles, skipFileType,
                        addBlanks, addDate, tempDir, copyFiles, headerToCheckAgainst, chdsAsFiles);
                });

                // Now find all folders that are empty, if we are supposed to
                if (!DatHeader.Romba && addBlanks)
                {
                    List<string> empties = DirectoryExtensions.ListEmpty(basePath);
                    Parallel.ForEach(empties, Globals.ParallelOptions, dir =>
                    {
                        // Get the full path for the directory
                        string fulldir = Path.GetFullPath(dir);

                        // Set the temporary variables
                        string gamename = string.Empty;
                        string romname = string.Empty;

                        // If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
                        if (DatHeader.Type == "SuperDAT")
                        {
                            gamename = fulldir.Remove(0, basePath.Length + 1);
                            romname = "_";
                        }

                        // Otherwise, we want just the top level folder as the game, and the file as everything else
                        else
                        {
                            gamename = fulldir.Remove(0, basePath.Length + 1).Split(Path.DirectorySeparatorChar)[0];
                            romname = Path.Combine(fulldir.Remove(0, basePath.Length + 1 + gamename.Length), "_");
                        }

                        // Sanitize the names
                        gamename = gamename.Trim(Path.DirectorySeparatorChar);
                        romname = romname.Trim(Path.DirectorySeparatorChar);

                        Globals.Logger.Verbose($"Adding blank empty folder: {gamename}");
                        this["null"].Add(new Rom(romname, gamename, omitFromScan));
                    });
                }
            }
            else if (File.Exists(basePath))
            {
                CheckFileForHashes(basePath, Path.GetDirectoryName(Path.GetDirectoryName(basePath)), omitFromScan, archivesAsFiles,
                    skipFileType, addBlanks, addDate, tempDir, copyFiles, headerToCheckAgainst, chdsAsFiles);
            }

            // Now that we're done, delete the temp folder (if it's not the default)
            Globals.Logger.User("Cleaning temp folder");
            if (tempDir != Path.GetTempPath())
                DirectoryExtensions.TryDelete(tempDir);

            // If we have a valid filter, perform the filtering now
            if (filter != null && filter != default(Filter))
                filter.FilterDatFile(this);

            return true;
        }

        /// <summary>
        /// Check a given file for hashes, based on current settings
        /// </summary>
        /// <param name="item">Filename of the item to be checked</param>
        /// <param name="basePath">Base folder to be used in creating the DAT</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
        /// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
        /// <param name="skipFileType">Type of files that should be skipped</param>
        /// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
        /// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
        /// <param name="tempDir">Name of the directory to create a temp folder in (blank is current directory)</param>
        /// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
        /// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        private void CheckFileForHashes(string item, string basePath, Hash omitFromScan, bool archivesAsFiles,
            SkipFileType skipFileType, bool addBlanks, bool addDate, string tempDir, bool copyFiles, string headerToCheckAgainst, bool chdsAsFiles)
        {
            // Special case for if we are in Romba mode (all names are supposed to be SHA-1 hashes)
            if (DatHeader.Romba)
            {
                GZipArchive gzarc = new GZipArchive(item);
                BaseFile baseFile = gzarc.GetTorrentGZFileInfo();

                // If the rom is valid, write it out
                if (baseFile != null && baseFile.Filename != null)
                {
                    // Add the list if it doesn't exist already
                    Rom rom = new Rom(baseFile);
                    Add(rom.GetKey(SortedBy.CRC), rom);
                    Globals.Logger.User($"File added: {Path.GetFileNameWithoutExtension(item)}{Environment.NewLine}");
                }
                else
                {
                    Globals.Logger.User($"File not added: {Path.GetFileNameWithoutExtension(item)}{Environment.NewLine}");
                    return;
                }

                return;
            }

            // If we're copying files, copy it first and get the new filename
            string newItem = item;
            string newBasePath = basePath;
            if (copyFiles)
            {
                newBasePath = Path.Combine(tempDir, Guid.NewGuid().ToString());
                newItem = Path.GetFullPath(Path.Combine(newBasePath, Path.GetFullPath(item).Remove(0, basePath.Length + 1)));
                DirectoryExtensions.TryCreateDirectory(Path.GetDirectoryName(newItem));
                File.Copy(item, newItem, true);
            }

            // Initialize possible archive variables
            BaseArchive archive = BaseArchive.Create(newItem);
            List<BaseFile> extracted = null;

            // If we have an archive and we're supposed to scan it
            if (archive != null && !archivesAsFiles)
                extracted = archive.GetChildren(omitFromScan: omitFromScan, date: addDate);

            // If the file should be skipped based on type, do so now
            if ((extracted != null && skipFileType == SkipFileType.Archive)
                || (extracted == null && skipFileType == SkipFileType.File))
            {
                return;
            }

            // If the extracted list is null, just scan the item itself
            if (extracted == null)
            {
                ProcessFile(newItem, string.Empty, newBasePath, omitFromScan, addDate, headerToCheckAgainst, chdsAsFiles);
            }
            // Otherwise, add all of the found items
            else
            {
                // First take care of the found items
                Parallel.ForEach(extracted, Globals.ParallelOptions, rom =>
                {
                    DatItem datItem = DatItem.Create(rom);
                    ProcessFileHelper(newItem,
                        datItem,
                        basePath,
                        (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item));
                });

                // Then, if we're looking for blanks, get all of the blank folders and add them
                if (addBlanks)
                {
                    List<string> empties = new List<string>();

                    // Now get all blank folders from the archive
                    if (archive != null)
                        empties = archive.GetEmptyFolders();

                    // Add add all of the found empties to the DAT
                    Parallel.ForEach(empties, Globals.ParallelOptions, empty =>
                    {
                        Rom emptyRom = new Rom(Path.Combine(empty, "_"), newItem, omitFromScan);
                        ProcessFileHelper(newItem,
                            emptyRom,
                            basePath,
                            (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item));
                    });
                }
            }

            // Cue to delete the file if it's a copy
            if (copyFiles && item != newItem)
                DirectoryExtensions.TryDelete(newBasePath);
        }

        /// <summary>
        /// Process a single file as a file
        /// </summary>
        /// <param name="item">File to be added</param>
        /// <param name="parent">Parent game to be used</param>
        /// <param name="basePath">Path the represents the parent directory</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
        /// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
        /// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        private void ProcessFile(string item, string parent, string basePath, Hash omitFromScan,
            bool addDate, string headerToCheckAgainst, bool chdsAsFiles)
        {
            Globals.Logger.Verbose($"'{Path.GetFileName(item)}' treated like a file");
            BaseFile baseFile = FileExtensions.GetInfo(item, omitFromScan: omitFromScan, date: addDate, header: headerToCheckAgainst, chdsAsFiles: chdsAsFiles);
            ProcessFileHelper(item, DatItem.Create(baseFile), basePath, parent);
        }

        /// <summary>
        /// Process a single file as a file (with found Rom data)
        /// </summary>
        /// <param name="item">File to be added</param>
        /// <param name="item">Rom data to be used to write to file</param>
        /// <param name="basepath">Path the represents the parent directory</param>
        /// <param name="parent">Parent game to be used</param>
        private void ProcessFileHelper(string item, DatItem datItem, string basepath, string parent)
        {
            // If we somehow got something other than a Rom or Disk, cancel out
            if (datItem.ItemType != ItemType.Rom && datItem.ItemType != ItemType.Disk)
                return;

            try
            {
                // If the basepath doesn't end with a directory separator, add it
                if (!basepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    basepath += Path.DirectorySeparatorChar.ToString();

                // Make sure we have the full item path
                item = Path.GetFullPath(item);

                // Process the item to sanitize names based on input
                SetDatItemInfo(datItem, item, parent, basepath);

                // Add the file information to the DAT
                string key = datItem.GetKey(SortedBy.CRC);
                Add(key, datItem);

                Globals.Logger.User($"File added: {datItem.Name}{Environment.NewLine}");
            }
            catch (IOException ex)
            {
                Globals.Logger.Error(ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Set proper Game and Rom names from user inputs
        /// </summary>
        /// <param name="datItem">DatItem representing the input file</param>
        /// <param name="item">Item name to use</param>
        /// <param name="parent">Parent name to use</param>
        /// <param name="basepath">Base path to use</param>
        private void SetDatItemInfo(DatItem datItem, string item, string parent, string basepath)
        {
            // Get the data to be added as game and item names
            string gamename, romname;

            // If the parent is blank, then we have a non-archive file
            if (string.IsNullOrWhiteSpace(parent))
            {
                // If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
                if (DatHeader.Type == "SuperDAT")
                {
                    gamename = Path.GetDirectoryName(item.Remove(0, basepath.Length));
                    romname = Path.GetFileName(item);
                }

                // Otherwise, we want just the top level folder as the game, and the file as everything else
                else
                {
                    gamename = item.Remove(0, basepath.Length).Split(Path.DirectorySeparatorChar)[0];
                    romname = item.Remove(0, (Path.Combine(basepath, gamename).Length));
                }
            }

            // Otherwise, we assume that we have an archive
            else
            {
                // If we have a SuperDAT, we want the archive name as the game, and the file as everything else (?)
                if (DatHeader.Type == "SuperDAT")
                {
                    gamename = parent;
                    romname = datItem.Name;
                }

                // Otherwise, we want the archive name as the game, and the file as everything else
                else
                {
                    gamename = parent;
                    romname = datItem.Name;
                }
            }

            // Sanitize the names
            gamename = gamename.Trim(Path.DirectorySeparatorChar);
            romname = romname?.Trim(Path.DirectorySeparatorChar) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(gamename) && string.IsNullOrWhiteSpace(romname))
            {
                romname = gamename;
                gamename = "Default";
            }

            // Update rom information
            datItem.Name = romname;
            datItem.MachineName = gamename;
            datItem.MachineDescription = gamename;

            // If we have a Disk, then the ".chd" extension needs to be removed
            if (datItem.ItemType == ItemType.Disk)
                datItem.Name = datItem.Name.Replace(".chd", string.Empty);
        }

        #endregion

        #region Rebuilding and Verifying

        /// <summary>
        /// Process the DAT and find all matches in input files and folders assuming they're a depot
        /// </summary>
        /// <param name="inputs">List of input files/folders to check</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="delete">True if input files should be deleted, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
        /// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <returns>True if rebuilding was a success, false otherwise</returns>
        public bool RebuildDepot(
            List<string> inputs,
            string outDir,
            bool date,
            bool delete,
            bool inverse,
            OutputFormat outputFormat,
            bool updateDat,
            string headerToCheckAgainst)
        {
            #region Perform setup

            // If the DAT is not populated and inverse is not set, inform the user and quit
            if (DatStats.Count == 0 && !inverse)
            {
                Globals.Logger.User("No entries were found to rebuild, exiting...");
                return false;
            }

            // Check that the output directory exists
            outDir = DirectoryExtensions.Ensure(outDir, create: true);

            // Now we want to get forcepack flag if it's not overridden
            if (outputFormat == OutputFormat.Folder && DatHeader.ForcePacking != ForcePacking.None)
            {
                switch (DatHeader.ForcePacking)
                {
                    case ForcePacking.Zip:
                        outputFormat = OutputFormat.TorrentZip;
                        break;
                    case ForcePacking.Unzip:
                        outputFormat = OutputFormat.Folder;
                        break;
                }
            }

            // Preload the Skipper list
            int listcount = Skipper.List.Count;

            #endregion

            bool success = true;

            #region Rebuild from depots in order

            string format = string.Empty;
            switch (outputFormat)
            {
                case OutputFormat.Folder:
                    format = "directory";
                    break;
                case OutputFormat.TapeArchive:
                    format = "TAR";
                    break;
                case OutputFormat.Torrent7Zip:
                    format = "Torrent7Z";
                    break;
                case OutputFormat.TorrentGzip:
                case OutputFormat.TorrentGzipRomba:
                    format = "TorrentGZ";
                    break;
                case OutputFormat.TorrentLRZip:
                    format = "TorrentLRZ";
                    break;
                case OutputFormat.TorrentRar:
                    format = "TorrentRAR";
                    break;
                case OutputFormat.TorrentXZ:
                    format = "TorrentXZ";
                    break;
                case OutputFormat.TorrentZip:
                    format = "TorrentZip";
                    break;
            }

            InternalStopwatch watch = new InternalStopwatch($"Rebuilding all files to {format}");

            // Now loop through and get only directories from the input paths
            List<string> directories = new List<string>();
            Parallel.ForEach(inputs, Globals.ParallelOptions, input =>
            {
                // Add to the list if the input is a directory
                if (Directory.Exists(input))
                {
                    Globals.Logger.Verbose($"Adding depot: {input}");
                    lock (directories)
                    {
                        directories.Add(input);
                    }
                }
            });

            // If we don't have any directories, we want to exit
            if (directories.Count == 0)
                return success;

            // Now that we have a list of depots, we want to sort the input DAT by SHA-1
            BucketBy(SortedBy.SHA1, DedupeType.None);

            // Then we want to loop through each of the hashes and see if we can rebuild
            List<string> hashes = Keys;
            foreach (string hash in hashes)
            {
                // Pre-empt any issues that could arise from string length
                if (hash.Length != Constants.SHA1Length)
                    continue;

                Globals.Logger.User($"Checking hash '{hash}'");

                // Get the extension path for the hash
                string subpath = PathExtensions.GetRombaPath(hash);

                // Find the first depot that includes the hash
                string foundpath = null;
                foreach (string directory in directories)
                {
                    if (File.Exists(Path.Combine(directory, subpath)))
                    {
                        foundpath = Path.Combine(directory, subpath);
                        break;
                    }
                }

                // If we didn't find a path, then we continue
                if (foundpath == null)
                    continue;

                // If we have a path, we want to try to get the rom information
                GZipArchive archive = new GZipArchive(foundpath);
                BaseFile fileinfo = archive.GetTorrentGZFileInfo();

                // If the file information is null, then we continue
                if (fileinfo == null)
                    continue;

                // Otherwise, we rebuild that file to all locations that we need to
                if (this[hash][0].ItemType == ItemType.Disk)
                    RebuildIndividualFile(new Disk(fileinfo), foundpath, outDir, date, inverse, outputFormat, updateDat, false /* isZip */, headerToCheckAgainst);
                else
                    RebuildIndividualFile(new Rom(fileinfo), foundpath, outDir, date, inverse, outputFormat, updateDat, false /* isZip */, headerToCheckAgainst);
            }

            watch.Stop();

            #endregion

            // If we're updating the DAT, output to the rebuild directory
            if (updateDat)
            {
                DatHeader.FileName = $"fixDAT_{DatHeader.FileName}";
                DatHeader.Name = $"fixDAT_{DatHeader.Name}";
                DatHeader.Description = $"fixDAT_{DatHeader.Description}";
                RemoveMarkedItems();
                Write(outDir);
            }

            return success;
        }

        /// <summary>
        /// Process the DAT and find all matches in input files and folders
        /// </summary>
        /// <param name="inputs">List of input files/folders to check</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="delete">True if input files should be deleted, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
        /// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
        /// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <returns>True if rebuilding was a success, false otherwise</returns>
        public bool RebuildGeneric(
            List<string> inputs,
            string outDir,
            bool quickScan,
            bool date,
            bool delete,
            bool inverse,
            OutputFormat outputFormat,
            ArchiveScanLevel archiveScanLevel,
            bool updateDat,
            string headerToCheckAgainst,
            bool chdsAsFiles)
        {
            #region Perform setup

            // If the DAT is not populated and inverse is not set, inform the user and quit
            if (DatStats.Count == 0 && !inverse)
            {
                Globals.Logger.User("No entries were found to rebuild, exiting...");
                return false;
            }

            // Check that the output directory exists
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
                outDir = Path.GetFullPath(outDir);
            }

            // Now we want to get forcepack flag if it's not overridden
            if (outputFormat == OutputFormat.Folder && DatHeader.ForcePacking != ForcePacking.None)
            {
                switch (DatHeader.ForcePacking)
                {
                    case ForcePacking.Zip:
                        outputFormat = OutputFormat.TorrentZip;
                        break;
                    case ForcePacking.Unzip:
                        outputFormat = OutputFormat.Folder;
                        break;
                }
            }

            // Preload the Skipper list
            int listcount = Skipper.List.Count;

            #endregion

            bool success = true;

            #region Rebuild from sources in order

            string format = string.Empty;
            switch (outputFormat)
            {
                case OutputFormat.Folder:
                    format = "directory";
                    break;
                case OutputFormat.TapeArchive:
                    format = "TAR";
                    break;
                case OutputFormat.Torrent7Zip:
                    format = "Torrent7Z";
                    break;
                case OutputFormat.TorrentGzip:
                case OutputFormat.TorrentGzipRomba:
                    format = "TorrentGZ";
                    break;
                case OutputFormat.TorrentLRZip:
                    format = "TorrentLRZ";
                    break;
                case OutputFormat.TorrentRar:
                    format = "TorrentRAR";
                    break;
                case OutputFormat.TorrentXZ:
                    format = "TorrentXZ";
                    break;
                case OutputFormat.TorrentZip:
                    format = "TorrentZip";
                    break;
            }

            InternalStopwatch watch = new InternalStopwatch($"Rebuilding all files to {format}");

            // Now loop through all of the files in all of the inputs
            foreach (string input in inputs)
            {
                // If the input is a file
                if (File.Exists(input))
                {
                    Globals.Logger.User($"Checking file: {input}");
                    RebuildGenericHelper(input, outDir, quickScan, date, delete, inverse, outputFormat, archiveScanLevel, updateDat, headerToCheckAgainst, chdsAsFiles);
                }

                // If the input is a directory
                else if (Directory.Exists(input))
                {
                    Globals.Logger.Verbose($"Checking directory: {input}");
                    foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
                    {
                        Globals.Logger.User($"Checking file: {file}");
                        RebuildGenericHelper(file, outDir, quickScan, date, delete, inverse, outputFormat, archiveScanLevel, updateDat, headerToCheckAgainst, chdsAsFiles);
                    }
                }
            }

            watch.Stop();

            #endregion

            // If we're updating the DAT, output to the rebuild directory
            if (updateDat)
            {
                DatHeader.FileName = $"fixDAT_{DatHeader.FileName}";
                DatHeader.Name = $"fixDAT_{DatHeader.Name}";
                DatHeader.Description = $"fixDAT_{DatHeader.Description}";
                RemoveMarkedItems();
                Write(outDir);
            }

            return success;
        }

        /// <summary>
        /// Attempt to add a file to the output if it matches
        /// </summary>
        /// <param name="file">Name of the file to process</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="delete">True if input files should be deleted, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
        /// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
        /// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        private void RebuildGenericHelper(
            string file,
            string outDir,
            bool quickScan,
            bool date,
            bool delete,
            bool inverse,
            OutputFormat outputFormat,
            ArchiveScanLevel archiveScanLevel,
            bool updateDat,
            string headerToCheckAgainst,
            bool chdsAsFiles)
        {
            // If we somehow have a null filename, return
            if (file == null)
                return;

            // Set the deletion variables
            bool usedExternally = false;
            bool usedInternally = false;

            // Get the required scanning level for the file
            Utilities.GetInternalExternalProcess(file, archiveScanLevel, out bool shouldExternalProcess, out bool shouldInternalProcess);

            // If we're supposed to scan the file externally
            if (shouldExternalProcess)
            {
                // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                BaseFile fileinfo = FileExtensions.GetInfo(file, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes),
                    header: headerToCheckAgainst, chdsAsFiles: chdsAsFiles);

                DatItem datItem = null;
                if (fileinfo.Type == FileType.CHD)
                    datItem = new Disk(fileinfo);
                else if (fileinfo.Type == FileType.None)
                    datItem = new Rom(fileinfo);

                usedExternally = RebuildIndividualFile(datItem, file, outDir, date, inverse, outputFormat,
                    updateDat, null /* isZip */, headerToCheckAgainst);
            }

            // If we're supposed to scan the file internally
            if (shouldInternalProcess)
            {
                // Create an empty list of BaseFile for archive entries
                List<BaseFile> entries = null;
                usedInternally = true;

                // Get the TGZ status for later
                GZipArchive tgz = new GZipArchive(file);
                bool isTorrentGzip = tgz.IsTorrent();

                // Get the base archive first
                BaseArchive archive = BaseArchive.Create(file);

                // Now get all extracted items from the archive
                if (archive != null)
                {
                    // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                    entries = archive.GetChildren(omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes), date: date);
                }

                // If the entries list is null, we encountered an error and should scan exteranlly
                if (entries == null && File.Exists(file))
                {
                    // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                    BaseFile fileinfo = FileExtensions.GetInfo(file, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes), chdsAsFiles: chdsAsFiles);

                    DatItem datItem = null;
                    if (fileinfo.Type == FileType.CHD)
                        datItem = new Disk(fileinfo);
                    else if (fileinfo.Type == FileType.None)
                        datItem = new Rom(fileinfo);

                    usedExternally = RebuildIndividualFile(datItem, file, outDir, date, inverse, outputFormat, updateDat, null /* isZip */, headerToCheckAgainst);
                }
                // Otherwise, loop through the entries and try to match
                else
                {
                    foreach (BaseFile entry in entries)
                    {
                        DatItem datItem = DatItem.Create(entry);
                        usedInternally |= RebuildIndividualFile(datItem, file, outDir, date, inverse, outputFormat, updateDat, !isTorrentGzip /* isZip */, headerToCheckAgainst);
                    }
                }
            }

            // If we are supposed to delete the file, do so
            if (delete && (usedExternally || usedInternally))
                FileExtensions.TryDelete(file);
        }

        /// <summary>
        /// Find duplicates and rebuild individual files to output
        /// </summary>
        /// <param name="datItem">Information for the current file to rebuild from</param>
        /// <param name="file">Name of the file to process</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
        /// <param name="isZip">True if the input file is an archive, false if the file is TGZ, null otherwise</param>
        /// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <returns>True if the file was able to be rebuilt, false otherwise</returns>
        private bool RebuildIndividualFile(
            DatItem datItem,
            string file,
            string outDir,
            bool date,
            bool inverse,
            OutputFormat outputFormat,
            bool updateDat,
            bool? isZip,
            string headerToCheckAgainst)
        {
            // Set the initial output value
            bool rebuilt = false;

            // If the DatItem is a Disk, force rebuilding to a folder except if TGZ
            if (datItem.ItemType == ItemType.Disk && !(outputFormat == OutputFormat.TorrentGzip || outputFormat == OutputFormat.TorrentGzipRomba))
                outputFormat = OutputFormat.Folder;

            // If we have a disk, change it into a Rom for later use
            if (datItem.ItemType == ItemType.Disk)
                datItem = ((Disk)datItem).ConvertToRom();

            // Prepopluate a few key strings
            string crc = ((Rom)datItem).CRC ?? string.Empty;
            string sha1 = ((Rom)datItem).SHA1 ?? string.Empty;

            // Find if the file has duplicates in the DAT
            bool hasDuplicates = HasDuplicates(datItem);

            // If it has duplicates and we're not filtering, rebuild it
            if (hasDuplicates && !inverse)
            {
                // Get the list of duplicates to rebuild to
                List<DatItem> dupes = GetDuplicates(datItem, remove: updateDat);

                // If we don't have any duplicates, continue
                if (dupes.Count == 0)
                    return false;

                // If we have a very specific TGZ->TGZ case, just copy it accordingly
                GZipArchive tgz = new GZipArchive(file);
                BaseFile rom = tgz.GetTorrentGZFileInfo();
                if (isZip == false && rom != null && (outputFormat == OutputFormat.TorrentGzip || outputFormat == OutputFormat.TorrentGzipRomba))
                {
                    Globals.Logger.User($"Matches found for '{Path.GetFileName(datItem.Name)}', rebuilding accordingly...");

                    // Get the proper output path
                    if (outputFormat == OutputFormat.TorrentGzipRomba)
                        outDir = Path.Combine(outDir, PathExtensions.GetRombaPath(sha1));
                    else
                        outDir = Path.Combine(outDir, sha1 + ".gz");

                    // Make sure the output folder is created
                    Directory.CreateDirectory(Path.GetDirectoryName(outDir));

                    // Now copy the file over
                    try
                    {
                        File.Copy(file, outDir);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                // Get a generic stream for the file
                Stream fileStream = new MemoryStream();

                // If we have a zipfile, extract the stream to memory
                if (isZip != null)
                {
                    string realName = null;
                    BaseArchive archive = BaseArchive.Create(file);
                    if (archive != null)
                        (fileStream, realName) = archive.CopyToStream(datItem.Name);
                }
                // Otherwise, just open the filestream
                else
                {
                    fileStream = FileExtensions.TryOpenRead(file);
                }

                // If the stream is null, then continue
                if (fileStream == null)
                    return false;

                // Seek to the beginning of the stream
                if (fileStream.CanSeek)
                    fileStream.Seek(0, SeekOrigin.Begin);

                Globals.Logger.User($"Matches found for '{Path.GetFileName(datItem.Name)}', rebuilding accordingly...");
                rebuilt = true;

                // Now loop through the list and rebuild accordingly
                foreach (DatItem item in dupes)
                {
                    // Get the output archive, if possible
                    Folder outputArchive = Folder.Create(outputFormat);

                    // Now rebuild to the output file
                    outputArchive.Write(fileStream, outDir, (Rom)item, date: date, romba: outputFormat == OutputFormat.TorrentGzipRomba);
                }

                // Close the input stream
                fileStream?.Dispose();
            }

            // If we have no duplicates and we're filtering, rebuild it
            else if (!hasDuplicates && inverse)
            {
                string machinename = null;

                // If we have a very specific TGZ->TGZ case, just copy it accordingly
                GZipArchive tgz = new GZipArchive(file);
                BaseFile rom = tgz.GetTorrentGZFileInfo();
                if (isZip == false && rom != null && outputFormat == OutputFormat.TorrentGzip)
                {
                    Globals.Logger.User($"Matches found for '{Path.GetFileName(datItem.Name)}', rebuilding accordingly...");

                    // Get the proper output path
                    if (outputFormat == OutputFormat.TorrentGzipRomba)
                        outDir = Path.Combine(outDir, PathExtensions.GetRombaPath(sha1));
                    else
                        outDir = Path.Combine(outDir, sha1 + ".gz");

                    // Make sure the output folder is created
                    Directory.CreateDirectory(Path.GetDirectoryName(outDir));

                    // Now copy the file over
                    try
                    {
                        File.Copy(file, outDir);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                // Get a generic stream for the file
                Stream fileStream = new MemoryStream();

                // If we have a zipfile, extract the stream to memory
                if (isZip != null)
                {
                    string realName;
                    BaseArchive archive = BaseArchive.Create(file);
                    if (archive != null)
                        (fileStream, realName) = archive.CopyToStream(datItem.Name);
                }
                // Otherwise, just open the filestream
                else
                {
                    fileStream = FileExtensions.TryOpenRead(file);
                }

                // If the stream is null, then continue
                if (fileStream == null)
                    return false;

                // Get the item from the current file
                Rom item = new Rom(fileStream.GetInfo(keepReadOpen: true));
                item.MachineName = Path.GetFileNameWithoutExtension(item.Name);
                item.MachineDescription = Path.GetFileNameWithoutExtension(item.Name);

                // If we are coming from an archive, set the correct machine name
                if (machinename != null)
                {
                    item.MachineName = machinename;
                    item.MachineDescription = machinename;
                }

                Globals.Logger.User($"No matches found for '{Path.GetFileName(datItem.Name)}', rebuilding accordingly from inverse flag...");

                // Get the output archive, if possible
                Folder outputArchive = Folder.Create(outputFormat);

                // Now rebuild to the output file
                if (outputArchive == null)
                {
                    string outfile = Path.Combine(outDir, Sanitizer.RemovePathUnsafeCharacters(item.MachineName), item.Name);

                    // Make sure the output folder is created
                    Directory.CreateDirectory(Path.GetDirectoryName(outfile));

                    // Now copy the file over
                    try
                    {
                        FileStream writeStream = FileExtensions.TryCreate(outfile);

                        // Copy the input stream to the output
                        int bufferSize = 4096 * 128;
                        byte[] ibuffer = new byte[bufferSize];
                        int ilen;
                        while ((ilen = fileStream.Read(ibuffer, 0, bufferSize)) > 0)
                        {
                            writeStream.Write(ibuffer, 0, ilen);
                            writeStream.Flush();
                        }

                        writeStream.Dispose();

                        if (date && !string.IsNullOrWhiteSpace(item.Date))
                            File.SetCreationTime(outfile, DateTime.Parse(item.Date));

                        rebuilt &= true;
                    }
                    catch
                    {
                        rebuilt &= false;
                    }
                }
                else
                {
                    rebuilt &= outputArchive.Write(fileStream, outDir, item, date: date, romba: outputFormat == OutputFormat.TorrentGzipRomba);
                }

                // Close the input stream
                fileStream?.Dispose();
            }

            // Now we want to take care of headers, if applicable
            if (headerToCheckAgainst != null)
            {
                // Get a generic stream for the file
                Stream fileStream = new MemoryStream();

                // If we have a zipfile, extract the stream to memory
                if (isZip != null)
                {
                    string realName;
                    BaseArchive archive = BaseArchive.Create(file);
                    if (archive != null)
                        (fileStream, realName) = archive.CopyToStream(datItem.Name);
                }
                // Otherwise, just open the filestream
                else
                {
                    fileStream = FileExtensions.TryOpenRead(file);
                }

                // If the stream is null, then continue
                if (fileStream == null)
                    return false;

                // Check to see if we have a matching header first
                SkipperRule rule = Skipper.GetMatchingRule(fileStream, Path.GetFileNameWithoutExtension(headerToCheckAgainst));

                // If there's a match, create the new file to write
                if (rule.Tests != null && rule.Tests.Count != 0)
                {
                    // If the file could be transformed correctly
                    MemoryStream transformStream = new MemoryStream();
                    if (rule.TransformStream(fileStream, transformStream, keepReadOpen: true, keepWriteOpen: true))
                    {
                        // Get the file informations that we will be using
                        Rom headerless = new Rom(transformStream.GetInfo(keepReadOpen: true));

                        // Find if the file has duplicates in the DAT
                        hasDuplicates = HasDuplicates(headerless);

                        // If it has duplicates and we're not filtering, rebuild it
                        if (hasDuplicates && !inverse)
                        {
                            // Get the list of duplicates to rebuild to
                            List<DatItem> dupes = GetDuplicates(headerless, remove: updateDat);

                            // If we don't have any duplicates, continue
                            if (dupes.Count == 0)
                                return false;

                            Globals.Logger.User($"Headerless matches found for '{Path.GetFileName(datItem.Name)}', rebuilding accordingly...");
                            rebuilt = true;

                            // Now loop through the list and rebuild accordingly
                            foreach (DatItem item in dupes)
                            {
                                // Create a headered item to use as well
                                datItem.CopyMachineInformation(item);
                                datItem.Name += $"_{crc}";

                                // If either copy succeeds, then we want to set rebuilt to true
                                bool eitherSuccess = false;

                                // Get the output archive, if possible
                                Folder outputArchive = Folder.Create(outputFormat);

                                // Now rebuild to the output file
                                eitherSuccess |= outputArchive.Write(transformStream, outDir, (Rom)item, date: date, romba: outputFormat == OutputFormat.TorrentGzipRomba);
                                eitherSuccess |= outputArchive.Write(fileStream, outDir, (Rom)datItem, date: date, romba: outputFormat == OutputFormat.TorrentGzipRomba);

                                // Now add the success of either rebuild
                                rebuilt &= eitherSuccess;
                            }
                        }
                    }

                    // Dispose of the stream
                    transformStream?.Dispose();
                }

                // Dispose of the stream
                fileStream?.Dispose();
            }

            return rebuilt;
        }

        /// <summary>
        /// Process the DAT and verify from the depots
        /// </summary>
        /// <param name="inputs">List of input directories to compare against</param>
        /// <returns>True if verification was a success, false otherwise</returns>
        public bool VerifyDepot(List<string> inputs)
        {
            bool success = true;

            InternalStopwatch watch = new InternalStopwatch("Verifying all from supplied depots");

            // Now loop through and get only directories from the input paths
            List<string> directories = new List<string>();
            foreach (string input in inputs)
            {
                // Add to the list if the input is a directory
                if (Directory.Exists(input))
                {
                    Globals.Logger.Verbose($"Adding depot: {input}");
                    directories.Add(input);
                }
            }

            // If we don't have any directories, we want to exit
            if (directories.Count == 0)
                return success;

            // Now that we have a list of depots, we want to sort the input DAT by SHA-1
            BucketBy(SortedBy.SHA1, DedupeType.None);

            // Then we want to loop through each of the hashes and see if we can rebuild
            List<string> hashes = Keys;
            foreach (string hash in hashes)
            {
                // Pre-empt any issues that could arise from string length
                if (hash.Length != Constants.SHA1Length)
                    continue;

                Globals.Logger.User($"Checking hash '{hash}'");

                // Get the extension path for the hash
                string subpath = PathExtensions.GetRombaPath(hash);

                // Find the first depot that includes the hash
                string foundpath = null;
                foreach (string directory in directories)
                {
                    if (File.Exists(Path.Combine(directory, subpath)))
                    {
                        foundpath = Path.Combine(directory, subpath);
                        break;
                    }
                }

                // If we didn't find a path, then we continue
                if (foundpath == null)
                    continue;

                // If we have a path, we want to try to get the rom information
                GZipArchive tgz = new GZipArchive(foundpath);
                BaseFile fileinfo = tgz.GetTorrentGZFileInfo();

                // If the file information is null, then we continue
                if (fileinfo == null)
                    continue;

                // Now we want to remove all duplicates from the DAT
                GetDuplicates(new Rom(fileinfo), remove: true)
                    .AddRange(GetDuplicates(new Disk(fileinfo), remove: true));
            }

            watch.Stop();

            // If there are any entries in the DAT, output to the rebuild directory
            DatHeader.FileName = $"fixDAT_{DatHeader.FileName}";
            DatHeader.Name = $"fixDAT_{DatHeader.Name}";
            DatHeader.Description = $"fixDAT_{DatHeader.Description}";
            RemoveMarkedItems();
            Write();

            return success;
        }

        /// <summary>
        /// Process the DAT and verify the output directory
        /// </summary>
        /// <param name="inputs">List of input directories to compare against</param>
        /// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
        /// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
        /// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <returns>True if verification was a success, false otherwise</returns>
        public bool VerifyGeneric(List<string> inputs, bool hashOnly, bool quickScan, string headerToCheckAgainst, bool chdsAsFiles, Filter filter)
        {
            // TODO: We want the cross section of what's the folder and what's in the DAT. Right now, it just has what's in the DAT that's not in the folder
            bool success = true;

            // Then, loop through and check each of the inputs
            Globals.Logger.User("Processing files:\n");
            foreach (string input in inputs)
            {
                // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                PopulateFromDir(input, (quickScan ? Hash.SecureHashes : Hash.DeepHashes) /* omitFromScan */, true /* bare */, false /* archivesAsFiles */,
                    SkipFileType.None, false /* addBlanks */, false /* addDate */, string.Empty /* tempDir */, false /* copyFiles */, headerToCheckAgainst, chdsAsFiles, filter);
            }

            // Setup the fixdat
            DatFile matched = Create(baseDat: this);
            matched.ResetDictionary();
            matched.DatHeader.FileName = $"fixDat_{matched.DatHeader.FileName}";
            matched.DatHeader.Name = $"fixDat_{matched.DatHeader.Name}";
            matched.DatHeader.Description = $"fixDat_{matched.DatHeader.Description}";
            matched.DatHeader.DatFormat = DatFormat.Logiqx;

            // If we are checking hashes only, essentially diff the inputs
            if (hashOnly)
            {
                // First we need to sort and dedupe by hash to get duplicates
                BucketBy(SortedBy.CRC, DedupeType.Full);

                // Then follow the same tactics as before
                foreach (string key in Keys)
                {
                    List<DatItem> roms = this[key];
                    foreach (DatItem rom in roms)
                    {
                        if (rom.SourceID == 99)
                        {
                            if (rom.ItemType == ItemType.Disk || rom.ItemType == ItemType.Rom)
                                matched.Add(((Disk)rom).SHA1, rom);
                        }
                    }
                }
            }
            // If we are checking full names, get only files found in directory
            else
            {
                foreach (string key in Keys)
                {
                    List<DatItem> roms = this[key];
                    List<DatItem> newroms = DatItem.Merge(roms);
                    foreach (Rom rom in newroms)
                    {
                        if (rom.SourceID == 99)
                            matched.Add($"{rom.Size}-{rom.CRC}", rom);
                    }
                }
            }

            // Now output the fixdat to the main folder
            RemoveMarkedItems();
            success &= matched.Write(stats: true);

            return success;
        }

        #endregion

        // TODO: Re-evaluate if any of these can be made more streamlined
        #region Splitting

        /// <summary>
        /// Split a set of input DATs based on the given information
        /// </summary>
        /// <param name="inputs">List of inputs to be used</param>
        /// <param name="outDir">Output directory for the split files</param>
        /// <param name="inplace">True if files should be written to the source folders, false otherwise</param>
        /// <param name="splittingMode">Type of split to perform, if any</param>
        /// <param name="exta">First extension to split on (Extension Split only)</param>
        /// <param name="extb">Second extension to split on (Extension Split only)</param>
        /// <param name="shortname">True if short filenames should be used, false otherwise (Level Split only)</param>
        /// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise (Level Split only)</param>
        /// <param name="radix">Long value representing the split point (Size Split only)</param>
        public void DetermineSplitType(List<string> inputs, string outDir, bool inplace, SplittingMode splittingMode,
            List<string> exta, List<string> extb, bool shortname, bool basedat, long radix)
        {
            // If we somehow have the "none" split type, return
            if (splittingMode == SplittingMode.None)
                return;

            // Get only files from the inputs
            List<string> files = DirectoryExtensions.GetFilesOnly(inputs, appendparent: true);

            // Loop over the input files
            foreach (string file in files)
            {
                // Create and fill the new DAT
                Parse(file, 0, 0);

                // Get the output directory
                outDir = PathExtensions.GetOutputPath(outDir, file, inplace);

                // Split and write the DAT
                if ((splittingMode & SplittingMode.Extension) != 0)
                    SplitByExtension(outDir, exta, extb);

                if ((splittingMode & SplittingMode.Hash) != 0)
                    SplitByHash(outDir);

                if ((splittingMode & SplittingMode.Level) != 0)
                    SplitByLevel(outDir, shortname, basedat);

                if ((splittingMode & SplittingMode.Size) != 0)
                    SplitBySize(outDir, radix);

                if ((splittingMode & SplittingMode.Type) != 0)
                    SplitByType(outDir);

                // Now re-empty the DAT to make room for the next one
                DatFormat tempFormat = DatHeader.DatFormat;
                DatHeader = new DatHeader();
                ResetDictionary();
                DatHeader.DatFormat = tempFormat;
            }
        }

        /// <summary>
        /// Split a DAT by input extensions
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <param name="extA">List of extensions to split on (first DAT)</param>
        /// <param name="extB">List of extensions to split on (second DAT)</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        private bool SplitByExtension(string outDir, List<string> extA, List<string> extB)
        {
            // If roms is empty, return false
            if (DatStats.Count == 0)
                return false;

            // Make sure all of the extensions don't have a dot at the beginning
            var newExtA = extA.Select(s => s.TrimStart('.').ToLowerInvariant());
            string newExtAString = string.Join(",", newExtA);

            var newExtB = extB.Select(s => s.TrimStart('.').ToLowerInvariant());
            string newExtBString = string.Join(",", newExtB);

            // Set all of the appropriate outputs for each of the subsets
            DatFile datdataA = Create(DatHeader.CloneStandard());
            datdataA.DatHeader.FileName += $" ({newExtAString})";
            datdataA.DatHeader.Name += $" ({newExtAString})";
            datdataA.DatHeader.Description += $" ({newExtAString})";

            DatFile datdataB = Create(DatHeader.CloneStandard());
            datdataB.DatHeader.FileName += $" ({newExtBString})";
            datdataB.DatHeader.Name += $" ({newExtBString})";
            datdataB.DatHeader.Description += $" ({newExtBString})";

            // Now separate the roms accordingly
            List<string> keys = Keys;
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = this[key];
                foreach (DatItem item in items)
                {
                    if (newExtA.Contains(PathExtensions.GetNormalizedExtension(item.Name)))
                    {
                        datdataA.Add(key, item);
                    }
                    else if (newExtB.Contains(PathExtensions.GetNormalizedExtension(item.Name)))
                    {
                        datdataB.Add(key, item);
                    }
                    else
                    {
                        datdataA.Add(key, item);
                        datdataB.Add(key, item);
                    }
                }
            });

            // Then write out both files
            bool success = datdataA.Write(outDir);
            success &= datdataB.Write(outDir);

            return success;
        }

        /// <summary>
        /// Split a DAT by best available hashes
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        private bool SplitByHash(string outDir)
        {
            // Create each of the respective output DATs
            Globals.Logger.User("Creating and populating new DATs");

            DatFile nodump = Create(DatHeader.CloneStandard());
            nodump.DatHeader.FileName += " (Nodump)";
            nodump.DatHeader.Name += " (Nodump)";
            nodump.DatHeader.Description += " (Nodump)";

            DatFile sha512 = Create(DatHeader.CloneStandard());
            sha512.DatHeader.FileName += " (SHA-512)";
            sha512.DatHeader.Name += " (SHA-512)";
            sha512.DatHeader.Description += " (SHA-512)";

            DatFile sha384 = Create(DatHeader.CloneStandard());
            sha384.DatHeader.FileName += " (SHA-384)";
            sha384.DatHeader.Name += " (SHA-384)";
            sha384.DatHeader.Description += " (SHA-384)";

            DatFile sha256 = Create(DatHeader.CloneStandard());
            sha256.DatHeader.FileName += " (SHA-256)";
            sha256.DatHeader.Name += " (SHA-256)";
            sha256.DatHeader.Description += " (SHA-256)";

            DatFile sha1 = Create(DatHeader.CloneStandard());
            sha1.DatHeader.FileName += " (SHA-1)";
            sha1.DatHeader.Name += " (SHA-1)";
            sha1.DatHeader.Description += " (SHA-1)";

#if NET_FRAMEWORK
            DatFile ripemd160 = Create(DatHeader.CloneStandard());
            ripemd160.DatHeader.FileName += " (RIPEMD160)";
            ripemd160.DatHeader.Name += " (RIPEMD160)";
            ripemd160.DatHeader.Description += " (RIPEMD160)";
#endif

            DatFile md5 = Create(DatHeader.CloneStandard());
            md5.DatHeader.FileName += " (MD5)";
            md5.DatHeader.Name += " (MD5)";
            md5.DatHeader.Description += " (MD5)";

            DatFile crc = Create(DatHeader.CloneStandard());
            crc.DatHeader.FileName += " (CRC)";
            crc.DatHeader.Name += " (CRC)";
            crc.DatHeader.Description += " (CRC)";

            DatFile other = Create(DatHeader.CloneStandard());
            other.DatHeader.FileName += " (Other)";
            other.DatHeader.Name += " (Other)";
            other.DatHeader.Description += " (Other)";

            // Now populate each of the DAT objects in turn
            List<string> keys = Keys;
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = this[key];
                foreach (DatItem item in items)
                {
                    // If the file is not a Rom or Disk, continue
                    if (item.ItemType != ItemType.Disk && item.ItemType != ItemType.Rom)
                        return;

                    // If the file is a nodump
                    if ((item.ItemType == ItemType.Rom && ((Rom)item).ItemStatus == ItemStatus.Nodump)
                        || (item.ItemType == ItemType.Disk && ((Disk)item).ItemStatus == ItemStatus.Nodump))
                    {
                        nodump.Add(key, item);
                    }
                    // If the file has a SHA-512
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).SHA512))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).SHA512)))
                    {
                        sha512.Add(key, item);
                    }
                    // If the file has a SHA-384
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).SHA384))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).SHA384)))
                    {
                        sha384.Add(key, item);
                    }
                    // If the file has a SHA-256
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).SHA256))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).SHA256)))
                    {
                        sha256.Add(key, item);
                    }
                    // If the file has a SHA-1
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).SHA1))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).SHA1)))
                    {
                        sha1.Add(key, item);
                    }
#if NET_FRAMEWORK
                    // If the file has a RIPEMD160
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).RIPEMD160))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).RIPEMD160)))
                    {
                        ripemd160.Add(key, item);
                    }
#endif
                    // If the file has an MD5
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).MD5))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).MD5)))
                    {
                        md5.Add(key, item);
                    }
                    // If the file has a CRC
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).CRC)))
                    {
                        crc.Add(key, item);
                    }
                    else
                    {
                        other.Add(key, item);
                    }
                }
            });

            // Now, output all of the files to the output directory
            Globals.Logger.User("DAT information created, outputting new files");
            bool success = true;
            success &= nodump.Write(outDir);
            success &= sha512.Write(outDir);
            success &= sha384.Write(outDir);
            success &= sha256.Write(outDir);
            success &= sha1.Write(outDir);
#if NET_FRAMEWORK
            success &= ripemd160.Write(outDir);
#endif
            success &= md5.Write(outDir);
            success &= crc.Write(outDir);

            return success;
        }

        /// <summary>
        /// Split a SuperDAT by lowest available directory level
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <param name="shortname">True if short names should be used, false otherwise</param>
        /// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        private bool SplitByLevel(string outDir, bool shortname, bool basedat)
        {
            // First, organize by games so that we can do the right thing
            BucketBy(SortedBy.Game, DedupeType.None, lower: false, norename: true);

            // Create a temporary DAT to add things to
            DatFile tempDat = Create(baseDat: this);
            tempDat.DatHeader.Name = null;

            // Sort the input keys
            List<string> keys = Keys;
            keys.Sort(SplitByLevelSort);

            // Then, we loop over the games
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                // Here, the key is the name of the game to be used for comparison
                if (tempDat.DatHeader.Name != null && tempDat.DatHeader.Name != Path.GetDirectoryName(key))
                {
                    // Reset the DAT for the next items
                    tempDat = Create(baseDat: this);
                    tempDat.DatHeader.Name = null;
                }

                // Clean the input list and set all games to be pathless
                List<DatItem> items = this[key];
                items.ForEach(item => item.MachineName = Path.GetFileName(item.MachineName));
                items.ForEach(item => item.MachineDescription = Path.GetFileName(item.MachineDescription));

                // Now add the game to the output DAT
                tempDat.AddRange(key, items);

                // Then set the DAT name to be the parent directory name
                tempDat.DatHeader.Name = Path.GetDirectoryName(key);
            });

            // TODO: Investigate why this method seems incomplete

            return true;
        }

        /// <summary>
        /// Helper function for SplitByLevel to sort the input game names
        /// </summary>
        /// <param name="a">First string to compare</param>
        /// <param name="b">Second string to compare</param>
        /// <returns>-1 for a coming before b, 0 for a == b, 1 for a coming after b</returns>
        private int SplitByLevelSort(string a, string b)
        {
            NaturalComparer nc = new NaturalComparer();
            int adeep = a.Count(c => c == '/' || c == '\\');
            int bdeep = b.Count(c => c == '/' || c == '\\');

            if (adeep == bdeep)
                return nc.Compare(a, b);

            return adeep - bdeep;
        }

        /// <summary>
        /// Helper function for SplitByLevel to clean and write out a DAT
        /// </summary>
        /// <param name="datFile">DAT to clean and write out</param>
        /// <param name="outDir">Directory to write out to</param>
        /// <param name="shortname">True if short naming scheme should be used, false otherwise</param>
        /// <param name="restore">True if original filenames should be used as the base for output filename, false otherwise</param>
        private void SplitByLevelHelper(DatFile datFile, string outDir, bool shortname, bool restore)
        {
            // Get the name from the DAT to use separately
            string name = datFile.DatHeader.Name;
            string expName = name.Replace("/", " - ").Replace("\\", " - ");

            // Now set the new output values
            datFile.DatHeader.FileName = WebUtility.HtmlDecode(string.IsNullOrWhiteSpace(name)
                ? DatHeader.FileName
                : (shortname
                    ? Path.GetFileName(name)
                    : expName
                    )
                );
            datFile.DatHeader.FileName = (restore ? $"{DatHeader.FileName} ({datFile.DatHeader.FileName})" : datFile.DatHeader.FileName);
            datFile.DatHeader.Name = $"{DatHeader.Name} ({expName})";
            datFile.DatHeader.Description = (string.IsNullOrWhiteSpace(DatHeader.Description) ? datFile.DatHeader.Name : $"{DatHeader.Description} ({expName})");
            datFile.DatHeader.Type = null;

            // Write out the temporary DAT to the proper directory
            datFile.Write(outDir);
        }

        /// <summary>
        /// Split a DAT by size of Rom
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <param name="radix">Long value representing the split point</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        private bool SplitBySize(string outDir, long radix)
        {
            // Create each of the respective output DATs
            Globals.Logger.User("Creating and populating new DATs");

            DatFile lessDat = Create(DatHeader.DatFormat);
            lessDat.DatHeader.FileName = $"{DatHeader.FileName} (less than {radix})";
            lessDat.DatHeader.Name = $"{DatHeader.Name} (less than {radix})";
            lessDat.DatHeader.Description = $"{DatHeader.Description} (less than {radix})";
            lessDat.DatHeader.Category = DatHeader.Category;
            lessDat.DatHeader.Version = DatHeader.Version;
            lessDat.DatHeader.Date = DatHeader.Date;
            lessDat.DatHeader.Author = DatHeader.Author;
            lessDat.DatHeader.Email = DatHeader.Email;
            lessDat.DatHeader.Homepage = DatHeader.Homepage;
            lessDat.DatHeader.Url = DatHeader.Url;
            lessDat.DatHeader.Comment = DatHeader.Comment;
            lessDat.DatHeader.Header = DatHeader.Header;
            lessDat.DatHeader.Type = DatHeader.Type;
            lessDat.DatHeader.ForceMerging = DatHeader.ForceMerging;
            lessDat.DatHeader.ForceNodump = DatHeader.ForceNodump;
            lessDat.DatHeader.ForcePacking = DatHeader.ForcePacking;
            lessDat.DatHeader.DedupeRoms = DatHeader.DedupeRoms;

            DatFile greaterEqualDat = Create(DatHeader.DatFormat);
            greaterEqualDat.DatHeader.FileName = $"{DatHeader.FileName} (equal-greater than {radix})";
            greaterEqualDat.DatHeader.Name = $"{DatHeader.Name} (equal-greater than {radix})";
            greaterEqualDat.DatHeader.Description = $"{DatHeader.Description} (equal-greater than {radix})";
            greaterEqualDat.DatHeader.Category = DatHeader.Category;
            greaterEqualDat.DatHeader.Version = DatHeader.Version;
            greaterEqualDat.DatHeader.Date = DatHeader.Date;
            greaterEqualDat.DatHeader.Author = DatHeader.Author;
            greaterEqualDat.DatHeader.Email = DatHeader.Email;
            greaterEqualDat.DatHeader.Homepage = DatHeader.Homepage;
            greaterEqualDat.DatHeader.Url = DatHeader.Url;
            greaterEqualDat.DatHeader.Comment = DatHeader.Comment;
            greaterEqualDat.DatHeader.Header = DatHeader.Header;
            greaterEqualDat.DatHeader.Type = DatHeader.Type;
            greaterEqualDat.DatHeader.ForceMerging = DatHeader.ForceMerging;
            greaterEqualDat.DatHeader.ForceNodump = DatHeader.ForceNodump;
            greaterEqualDat.DatHeader.ForcePacking = DatHeader.ForcePacking;
            greaterEqualDat.DatHeader.DedupeRoms = DatHeader.DedupeRoms;

            // Now populate each of the DAT objects in turn
            List<string> keys = Keys;
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = this[key];
                foreach (DatItem item in items)
                {
                    // If the file is not a Rom, it automatically goes in the "lesser" dat
                    if (item.ItemType != ItemType.Rom)
                        lessDat.Add(key, item);

                    // If the file is a Rom and less than the radix, put it in the "lesser" dat
                    else if (item.ItemType == ItemType.Rom && ((Rom)item).Size < radix)
                        lessDat.Add(key, item);

                    // If the file is a Rom and greater than or equal to the radix, put it in the "greater" dat
                    else if (item.ItemType == ItemType.Rom && ((Rom)item).Size >= radix)
                        greaterEqualDat.Add(key, item);
                }
            });

            // Now, output all of the files to the output directory
            Globals.Logger.User("DAT information created, outputting new files");
            bool success = true;
            success &= lessDat.Write(outDir);
            success &= greaterEqualDat.Write(outDir);

            return success;
        }

        /// <summary>
        /// Split a DAT by type of DatItem
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        private bool SplitByType(string outDir)
        {
            // Create each of the respective output DATs
            Globals.Logger.User("Creating and populating new DATs");
            DatFile romdat = Create(DatHeader.DatFormat);
            romdat.DatHeader.FileName = DatHeader.FileName + " (ROM)";
            romdat.DatHeader.Name = DatHeader.Name + " (ROM)";
            romdat.DatHeader.Description = DatHeader.Description + " (ROM)";
            romdat.DatHeader.Category = DatHeader.Category;
            romdat.DatHeader.Version = DatHeader.Version;
            romdat.DatHeader.Date = DatHeader.Date;
            romdat.DatHeader.Author = DatHeader.Author;
            romdat.DatHeader.Email = DatHeader.Email;
            romdat.DatHeader.Homepage = DatHeader.Homepage;
            romdat.DatHeader.Url = DatHeader.Url;
            romdat.DatHeader.Comment = DatHeader.Comment;
            romdat.DatHeader.Header = DatHeader.Header;
            romdat.DatHeader.Type = DatHeader.Type;
            romdat.DatHeader.ForceMerging = DatHeader.ForceMerging;
            romdat.DatHeader.ForceNodump = DatHeader.ForceNodump;
            romdat.DatHeader.ForcePacking = DatHeader.ForcePacking;
            romdat.DatHeader.DedupeRoms = DatHeader.DedupeRoms;

            DatFile diskdat = Create(DatHeader.DatFormat);
            diskdat.DatHeader.FileName = DatHeader.FileName + " (Disk)";
            diskdat.DatHeader.Name = DatHeader.Name + " (Disk)";
            diskdat.DatHeader.Description = DatHeader.Description + " (Disk)";
            diskdat.DatHeader.Category = DatHeader.Category;
            diskdat.DatHeader.Version = DatHeader.Version;
            diskdat.DatHeader.Date = DatHeader.Date;
            diskdat.DatHeader.Author = DatHeader.Author;
            diskdat.DatHeader.Email = DatHeader.Email;
            diskdat.DatHeader.Homepage = DatHeader.Homepage;
            diskdat.DatHeader.Url = DatHeader.Url;
            diskdat.DatHeader.Comment = DatHeader.Comment;
            diskdat.DatHeader.Header = DatHeader.Header;
            diskdat.DatHeader.Type = DatHeader.Type;
            diskdat.DatHeader.ForceMerging = DatHeader.ForceMerging;
            diskdat.DatHeader.ForceNodump = DatHeader.ForceNodump;
            diskdat.DatHeader.ForcePacking = DatHeader.ForcePacking;
            diskdat.DatHeader.DedupeRoms = DatHeader.DedupeRoms;

            DatFile sampledat = Create(DatHeader.DatFormat);
            sampledat.DatHeader.FileName = DatHeader.FileName + " (Sample)";
            sampledat.DatHeader.Name = DatHeader.Name + " (Sample)";
            sampledat.DatHeader.Description = DatHeader.Description + " (Sample)";
            sampledat.DatHeader.Category = DatHeader.Category;
            sampledat.DatHeader.Version = DatHeader.Version;
            sampledat.DatHeader.Date = DatHeader.Date;
            sampledat.DatHeader.Author = DatHeader.Author;
            sampledat.DatHeader.Email = DatHeader.Email;
            sampledat.DatHeader.Homepage = DatHeader.Homepage;
            sampledat.DatHeader.Url = DatHeader.Url;
            sampledat.DatHeader.Comment = DatHeader.Comment;
            sampledat.DatHeader.Header = DatHeader.Header;
            sampledat.DatHeader.Type = DatHeader.Type;
            sampledat.DatHeader.ForceMerging = DatHeader.ForceMerging;
            sampledat.DatHeader.ForceNodump = DatHeader.ForceNodump;
            sampledat.DatHeader.ForcePacking = DatHeader.ForcePacking;
            sampledat.DatHeader.DedupeRoms = DatHeader.DedupeRoms;

            // Now populate each of the DAT objects in turn
            List<string> keys = Keys;
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = this[key];
                foreach (DatItem item in items)
                {
                    // If the file is a Rom
                    if (item.ItemType == ItemType.Rom)
                        romdat.Add(key, item);

                    // If the file is a Disk
                    else if (item.ItemType == ItemType.Disk)
                        diskdat.Add(key, item);

                    // If the file is a Sample
                    else if (item.ItemType == ItemType.Sample)
                        sampledat.Add(key, item);
                }
            });

            // Now, output all of the files to the output directory
            Globals.Logger.User("DAT information created, outputting new files");
            bool success = true;
            success &= romdat.Write(outDir);
            success &= diskdat.Write(outDir);
            success &= sampledat.Write(outDir);

            return success;
        }

        #endregion

        #region Statistics

        /// <summary>
        /// Output the stats for the Dat in a human-readable format
        /// </summary>
        /// <param name="recalculate">True if numbers should be recalculated for the DAT, false otherwise (default)</param>
        /// <param name="game">Number of games to use, -1 means recalculate games (default)</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise (default)</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise (default)</param>
        public void WriteStatsToScreen(bool recalculate = false, long game = -1, bool baddumpCol = false, bool nodumpCol = false)
        {
            // If we're supposed to recalculate the statistics, do so
            if (recalculate)
                RecalculateStats();

            BucketBy(SortedBy.Game, DedupeType.None, norename: true);
            
            // TODO: How can the size be negative when dealing with Int64?
            if (DatStats.TotalSize < 0)
                DatStats.TotalSize = Int64.MaxValue + DatStats.TotalSize;

            // Log the results to screen
            string results = $"For '{DatHeader.FileName}':{Environment.NewLine}"
                + $"--------------------------------------------------{Environment.NewLine}"
                + $"    Uncompressed size:       {Utilities.GetBytesReadable(DatStats.TotalSize)}{Environment.NewLine}"
                + $"    Games found:             {(game == -1 ? Keys.Count() : game)}{Environment.NewLine}"
                + $"    Roms found:              {DatStats.RomCount}{Environment.NewLine}"
                + $"    Disks found:             {DatStats.DiskCount}{Environment.NewLine}"
                + $"    Roms with CRC:           {DatStats.CRCCount}{Environment.NewLine}"
                + $"    Roms with MD5:           {DatStats.MD5Count}{Environment.NewLine}"
#if NET_FRAMEWORK
                + $"    Roms with RIPEMD160:     {DatStats.RIPEMD160Count}{Environment.NewLine}"
#endif
                + $"    Roms with SHA-1:         {DatStats.SHA1Count}{Environment.NewLine}"
                + $"    Roms with SHA-256:       {DatStats.SHA256Count}{Environment.NewLine}"
                + $"    Roms with SHA-384:       {DatStats.SHA384Count}{Environment.NewLine}"
                + $"    Roms with SHA-512:       {DatStats.SHA512Count}{Environment.NewLine}"
                + (baddumpCol ? $"    Roms with BadDump status: {DatStats.BaddumpCount}{Environment.NewLine}" : string.Empty)
                + (nodumpCol ? $"    Roms with Nodump status: {DatStats.NodumpCount}{Environment.NewLine}" : string.Empty);

            // For spacing between DATs
            results += $"{Environment.NewLine}{Environment.NewLine}";

            Globals.Logger.User(results);
        }

        /// <summary>
        /// Recalculate the statistics for the Dat
        /// </summary>
        private void RecalculateStats()
        {
            // Wipe out any stats already there
            DatStats.Reset();

            // If we have a blank Dat in any way, return
            if (this == null || DatStats.Count == 0)
                return;

            // Loop through and add
            List<string> keys = Keys;
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = this[key];
                foreach (DatItem item in items)
                {
                    DatStats.AddItem(item);
                }
            });
        }

        #endregion

        #region Writing

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outDir">Set the output directory (default current directory)</param>
        /// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
        /// <param name="stats">True if DAT statistics should be output on write, false otherwise (default)</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <param name="overwrite">True if files should be overwritten (default), false if they should be renamed instead</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public bool Write(string outDir = null, bool norename = true, bool stats = false, bool ignoreblanks = false, bool overwrite = true)
        {
            // If there's nothing there, abort
            if (DatStats.Count == 0)
            {
                Globals.Logger.User("There were no items to write out!");
                return false;
            }

            // Ensure the output directory is set and created
            outDir = DirectoryExtensions.Ensure(outDir, create: true);

            // If the DAT has no output format, default to XML
            if (DatHeader.DatFormat == 0)
            {
                Globals.Logger.Verbose("No DAT format defined, defaulting to XML");
                DatHeader.DatFormat = DatFormat.Logiqx;
            }

            // Make sure that the three essential fields are filled in
            if (string.IsNullOrWhiteSpace(DatHeader.FileName) && string.IsNullOrWhiteSpace(DatHeader.Name) && string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                DatHeader.FileName = DatHeader.Name = DatHeader.Description = "Default";
            }
            else if (string.IsNullOrWhiteSpace(DatHeader.FileName) && string.IsNullOrWhiteSpace(DatHeader.Name) && !string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                DatHeader.FileName = DatHeader.Name = DatHeader.Description;
            }
            else if (string.IsNullOrWhiteSpace(DatHeader.FileName) && !string.IsNullOrWhiteSpace(DatHeader.Name) && string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                DatHeader.FileName = DatHeader.Description = DatHeader.Name;
            }
            else if (string.IsNullOrWhiteSpace(DatHeader.FileName) && !string.IsNullOrWhiteSpace(DatHeader.Name) && !string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                DatHeader.FileName = DatHeader.Description;
            }
            else if (!string.IsNullOrWhiteSpace(DatHeader.FileName) && string.IsNullOrWhiteSpace(DatHeader.Name) && string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                DatHeader.Name = DatHeader.Description = DatHeader.FileName;
            }
            else if (!string.IsNullOrWhiteSpace(DatHeader.FileName) && string.IsNullOrWhiteSpace(DatHeader.Name) && !string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                DatHeader.Name = DatHeader.Description;
            }
            else if (!string.IsNullOrWhiteSpace(DatHeader.FileName) && !string.IsNullOrWhiteSpace(DatHeader.Name) && string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                DatHeader.Description = DatHeader.Name;
            }
            else if (!string.IsNullOrWhiteSpace(DatHeader.FileName) && !string.IsNullOrWhiteSpace(DatHeader.Name) && !string.IsNullOrWhiteSpace(DatHeader.Description))
            {
                // Nothing is needed
            }

            // Output initial statistics, for kicks
            if (stats)
                WriteStatsToScreen(recalculate: (DatStats.RomCount + DatStats.DiskCount == 0), baddumpCol: true, nodumpCol: true);

            // Run the one rom per game logic, if required
            if (DatHeader.OneRom)
                OneRomPerGame();

            // Bucket and dedupe according to the flag
            if (DatHeader.DedupeRoms == DedupeType.Full)
                BucketBy(SortedBy.CRC, DatHeader.DedupeRoms, norename: norename);
            else if (DatHeader.DedupeRoms == DedupeType.Game)
                BucketBy(SortedBy.Game, DatHeader.DedupeRoms, norename: norename);

            // Bucket roms by game name, if not already
            BucketBy(SortedBy.Game, DedupeType.None, norename: norename);

            // Output the number of items we're going to be writing
            Globals.Logger.User($"A total of {DatStats.Count} items will be written out to '{DatHeader.FileName}'");

            // If we are removing scene dates, do that now
            if (DatHeader.SceneDateStrip)
                StripSceneDatesFromItems();

            // Get the outfile names
            Dictionary<DatFormat, string> outfiles = DatHeader.CreateOutFileNames(outDir, overwrite);

            try
            {
                // Write out all required formats
                Parallel.ForEach(outfiles.Keys, Globals.ParallelOptions, datFormat =>
                {
                    string outfile = outfiles[datFormat];
                    try
                    {
                        DatFile.Create(datFormat, this)?.WriteToFile(outfile, ignoreblanks);
                    }
                    catch (Exception ex)
                    {
                        Globals.Logger.Error($"Datfile {outfile} could not be written out: {ex}");
                    }

                });
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public abstract bool WriteToFile(string outfile, bool ignoreblanks = false);

        /// <summary>
        /// Process an item and correctly set the item name
        /// </summary>
        /// <param name="item">DatItem to update</param>
        /// <param name="forceRemoveQuotes">True if the Quotes flag should be ignored, false otherwise</param>
        /// <param name="forceRomName">True if the UseRomName should be always on (default), false otherwise</param>
        protected void ProcessItemName(DatItem item, bool forceRemoveQuotes, bool forceRomName = true)
        {
            string name = item.Name;

            // Backup relevant values and set new ones accordingly
            bool quotesBackup = DatHeader.Quotes;
            bool useRomNameBackup = DatHeader.UseRomName;
            if (forceRemoveQuotes)
                DatHeader.Quotes = false;

            if (forceRomName)
                DatHeader.UseRomName = true;

            // Create the proper Prefix and Postfix
            string pre = CreatePrefixPostfix(item, true);
            string post = CreatePrefixPostfix(item, false);

            // If we're in Romba mode, take care of that instead
            if (DatHeader.Romba)
            {
                if (item.ItemType == ItemType.Rom)
                {
                    Rom romItem = item as Rom;

                    // We can only write out if there's a SHA-1
                    if (!string.IsNullOrWhiteSpace(romItem.SHA1))
                    {
                        name = PathExtensions.GetRombaPath(romItem.SHA1).Replace('\\', '/');
                        item.Name = $"{pre}{name}{post}";
                    }
                }
                else if (item.ItemType == ItemType.Disk)
                {
                    Disk diskItem = item as Disk;

                    // We can only write out if there's a SHA-1
                    if (!string.IsNullOrWhiteSpace(diskItem.SHA1))
                    {
                        name = PathExtensions.GetRombaPath(diskItem.SHA1).Replace('\\', '/');
                        item.Name = pre + name + post;
                    }
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(DatHeader.ReplaceExtension) || DatHeader.RemoveExtension)
            {
                if (DatHeader.RemoveExtension)
                    DatHeader.ReplaceExtension = string.Empty;

                string dir = Path.GetDirectoryName(name);
                dir = dir.TrimStart(Path.DirectorySeparatorChar);
                name = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + DatHeader.ReplaceExtension);
            }

            if (!string.IsNullOrWhiteSpace(DatHeader.AddExtension))
                name += DatHeader.AddExtension;

            if (DatHeader.UseRomName && DatHeader.GameName)
                name = Path.Combine(item.MachineName, name);

            // Now assign back the item name
            item.Name = pre + name + post;

            // Restore all relevant values
            if (forceRemoveQuotes)
                DatHeader.Quotes = quotesBackup;

            if (forceRomName)
                DatHeader.UseRomName = useRomNameBackup;
        }

        /// <summary>
        /// Create a prefix or postfix from inputs
        /// </summary>
        /// <param name="item">DatItem to create a prefix/postfix for</param>
        /// <param name="prefix">True for prefix, false for postfix</param>
        /// <returns>Sanitized string representing the postfix or prefix</returns>
        protected string CreatePrefixPostfix(DatItem item, bool prefix)
        {
            // Initialize strings
            string fix = string.Empty,
                game = item.MachineName,
                name = item.Name,
                manufacturer = item.Manufacturer,
                publisher = item.Publisher,
                crc = string.Empty,
                md5 = string.Empty,
                ripemd160 = string.Empty,
                sha1 = string.Empty,
                sha256 = string.Empty,
                sha384 = string.Empty,
                sha512 = string.Empty,
                size = string.Empty;

            // If we have a prefix
            if (prefix)
                fix = DatHeader.Prefix + (DatHeader.Quotes ? "\"" : string.Empty);

            // If we have a postfix
            else
                fix = (DatHeader.Quotes ? "\"" : string.Empty) + DatHeader.Postfix;

            // Ensure we have the proper values for replacement
            if (item.ItemType == ItemType.Rom)
            {
                crc = ((Rom)item).CRC;
                md5 = ((Rom)item).MD5;
#if NET_FRAMEWORK
                ripemd160 = ((Rom)item).RIPEMD160;
#endif
                sha1 = ((Rom)item).SHA1;
                sha256 = ((Rom)item).SHA256;
                sha384 = ((Rom)item).SHA384;
                sha512 = ((Rom)item).SHA512;
                size = ((Rom)item).Size.ToString();
            }
            else if (item.ItemType == ItemType.Disk)
            {
                md5 = ((Disk)item).MD5;
#if NET_FRAMEWORK
                ripemd160 = ((Disk)item).RIPEMD160;
#endif
                sha1 = ((Disk)item).SHA1;
                sha256 = ((Disk)item).SHA256;
                sha384 = ((Disk)item).SHA384;
                sha512 = ((Disk)item).SHA512;
            }

            // Now do bulk replacement where possible
            fix = fix
                .Replace("%game%", game)
                .Replace("%machine%", game)
                .Replace("%name%", name)
                .Replace("%manufacturer%", manufacturer)
                .Replace("%publisher%", publisher)
                .Replace("%crc%", crc)
                .Replace("%md5%", md5)
                .Replace("%ripemd160%", ripemd160)
                .Replace("%sha1%", sha1)
                .Replace("%sha256%", sha256)
                .Replace("%sha384%", sha384)
                .Replace("%sha512%", sha512)
                .Replace("%size%", size);

            // TODO: Add GameName logic here too?

            return fix;
        }

        #endregion

        #endregion // Instance Methods

        #region Static Methods

        #region Statistics

        /// <summary>
        /// Output the stats for a list of input dats as files in a human-readable format
        /// </summary>
        /// <param name="inputs">List of input files and folders</param>
        /// <param name="reportName">Name of the output file</param>
        /// <param name="single">True if single DAT stats are output, false otherwise</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        /// <param name="statDatFormat" > Set the statistics output format to use</param>
        public static void OutputStats(List<string> inputs, string reportName, string outDir, bool single,
            bool baddumpCol, bool nodumpCol, StatReportFormat statDatFormat)
        {
            // If there's no output format, set the default
            if (statDatFormat == StatReportFormat.None)
                statDatFormat = StatReportFormat.Textfile;

            // Get the proper output file name
            if (string.IsNullOrWhiteSpace(reportName))
                reportName = "report";

            // Get the proper output directory name
            outDir = DirectoryExtensions.Ensure(outDir);

            // Get the dictionary of desired output report names
            Dictionary<StatReportFormat, string> outputs = CreateOutStatsNames(outDir, statDatFormat, reportName);

            // Make sure we have all files and then order them
            List<string> files = DirectoryExtensions.GetFilesOnly(inputs);
            files = files
                .OrderBy(i => Path.GetDirectoryName(i))
                .ThenBy(i => Path.GetFileName(i))
                .ToList();

            // Get all of the writers that we need
            List<BaseReport> reports = outputs.Select(kvp => BaseReport.Create(kvp.Key, kvp.Value, baddumpCol, nodumpCol)).ToList();

            // Write the header, if any
            reports.ForEach(report => report.WriteHeader());

            // Init all total variables
            DatStats totalStats = new DatStats();

            // Init directory-level variables
            string lastdir = null;
            string basepath = null;
            DatStats dirStats = new DatStats();

            // Now process each of the input files
            foreach (string file in files)
            {
                // Get the directory for the current file
                string thisdir = Path.GetDirectoryName(file);
                basepath = Path.GetDirectoryName(Path.GetDirectoryName(file));

                // If we don't have the first file and the directory has changed, show the previous directory stats and reset
                if (lastdir != null && thisdir != lastdir)
                {
                    // Output separator if needed
                    reports.ForEach(report => report.WriteMidSeparator());

                    DatFile lastdirdat = Create();
                    lastdirdat.DatHeader.FileName = $"DIR: {WebUtility.HtmlEncode(lastdir)}";
                    lastdirdat.DatStats = dirStats;

                    lastdirdat.WriteStatsToScreen(recalculate: false, game: dirStats.GameCount, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
                    reports.ForEach(report => report.ReplaceStatistics(lastdirdat.DatHeader.FileName, dirStats.GameCount, dirStats));
                    reports.ForEach(report => report.Write());

                    // Write the mid-footer, if any
                    reports.ForEach(report => report.WriteFooterSeparator());

                    // Write the header, if any
                    reports.ForEach(report => report.WriteMidHeader());

                    // Reset the directory stats
                    dirStats.Reset();
                }

                Globals.Logger.Verbose($"Beginning stat collection for '{file}'", false);
                List<string> games = new List<string>();
                DatFile datdata = Create();
                datdata.Parse(file, 0, 0);
                datdata.BucketBy(SortedBy.Game, DedupeType.None, norename: true);

                // Output single DAT stats (if asked)
                Globals.Logger.User($"Adding stats for file '{file}'\n", false);
                if (single)
                {
                    datdata.WriteStatsToScreen(recalculate: false, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
                    reports.ForEach(report => report.ReplaceStatistics(datdata.DatHeader.FileName, datdata.Keys.Count, datdata.DatStats));
                    reports.ForEach(report => report.Write());
                }

                // Add single DAT stats to dir
                dirStats.AddStats(datdata.DatStats);
                dirStats.GameCount += datdata.Keys.Count();

                // Add single DAT stats to totals
                totalStats.AddStats(datdata.DatStats);
                totalStats.GameCount += datdata.Keys.Count();

                // Make sure to assign the new directory
                lastdir = thisdir;
            }

            // Output the directory stats one last time
            reports.ForEach(report => report.WriteMidSeparator());

            if (single)
            {
                DatFile dirdat = Create();
                dirdat.DatHeader.FileName = $"DIR: {WebUtility.HtmlEncode(lastdir)}";
                dirdat.DatStats = dirStats;

                dirdat.WriteStatsToScreen(recalculate: false, game: dirStats.GameCount, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
                reports.ForEach(report => report.ReplaceStatistics(dirdat.DatHeader.FileName, dirStats.GameCount, dirStats));
                reports.ForEach(report => report.Write());
            }

            // Write the mid-footer, if any
            reports.ForEach(report => report.WriteFooterSeparator());

            // Write the header, if any
            reports.ForEach(report => report.WriteMidHeader());

            // Reset the directory stats
            dirStats.Reset();

            // Output total DAT stats
            DatFile totaldata = DatFile.Create();
            totaldata.DatHeader.FileName = "DIR: All DATs";
            totaldata.DatStats = totalStats;

            totaldata.WriteStatsToScreen(recalculate: false, game: totalStats.GameCount, baddumpCol: baddumpCol, nodumpCol: nodumpCol);
            reports.ForEach(report => report.ReplaceStatistics(totaldata.DatHeader.FileName, totalStats.GameCount, totalStats));
            reports.ForEach(report => report.Write());

            // Output footer if needed
            reports.ForEach(report => report.WriteFooter());

            Globals.Logger.User(@"
Please check the log folder if the stats scrolled offscreen", false);
        }

        /// <summary>
        /// Get the proper extension for the stat output format
        /// </summary>
        /// <param name="outDir">Output path to use</param>
        /// <param name="statDatFormat">StatDatFormat to get the extension for</param>
        /// <param name="reportName">Name of the input file to use</param>
        /// <returns>Dictionary of output formats mapped to file names</returns>
        private static Dictionary<StatReportFormat, string> CreateOutStatsNames(string outDir, StatReportFormat statDatFormat, string reportName, bool overwrite = true)
        {
            Dictionary<StatReportFormat, string> output = new Dictionary<StatReportFormat, string>();

            // First try to create the output directory if we need to
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            // Double check the outDir for the end delim
            if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                outDir += Path.DirectorySeparatorChar;

            // For each output format, get the appropriate stream writer
            if ((statDatFormat & StatReportFormat.Textfile) != 0)
                output.Add(StatReportFormat.Textfile, CreateOutStatsNamesHelper(outDir, ".txt", reportName, overwrite));

            if ((statDatFormat & StatReportFormat.CSV) != 0)
                output.Add(StatReportFormat.CSV, CreateOutStatsNamesHelper(outDir, ".csv", reportName, overwrite));

            if ((statDatFormat & StatReportFormat.HTML) != 0)
                output.Add(StatReportFormat.HTML, CreateOutStatsNamesHelper(outDir, ".html", reportName, overwrite));

            if ((statDatFormat & StatReportFormat.SSV) != 0)
                output.Add(StatReportFormat.SSV, CreateOutStatsNamesHelper(outDir, ".ssv", reportName, overwrite));

            if ((statDatFormat & StatReportFormat.TSV) != 0)
                output.Add(StatReportFormat.TSV, CreateOutStatsNamesHelper(outDir, ".tsv", reportName, overwrite));

            return output;
        }

        /// <summary>
        /// Help generating the outstats name
        /// </summary>
        /// <param name="outDir">Output directory</param>
        /// <param name="extension">Extension to use for the file</param>
        /// <param name="reportName">Name of the input file to use</param>
        /// <param name="overwrite">True if we ignore existing files, false otherwise</param>
        /// <returns>String containing the new filename</returns>
        private static string CreateOutStatsNamesHelper(string outDir, string extension, string reportName, bool overwrite)
        {
            string outfile = outDir + reportName + extension;
            outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());

            if (!overwrite)
            {
                int i = 1;
                while (File.Exists(outfile))
                {
                    outfile = $"{outDir}{reportName}_{i}{extension}";
                    outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());
                    i++;
                }
            }

            return outfile;
        }

        #endregion

        #endregion // Static Methods
    }
}
