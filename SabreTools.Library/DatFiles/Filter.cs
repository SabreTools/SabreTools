using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents the filtering operations that need to be performed on a set of items, usually a DAT
    /// </summary>
    /// TODO: Can this use `Field` instead of explicit filters?
    public class Filter
    {
        #region Pubically facing variables

        #region Machine Filters

        /// <summary>
        /// Include or exclude machine names
        /// </summary>
        public FilterItem<string> MachineName { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include romof and cloneof when filtering machine names
        /// </summary>
        public FilterItem<bool> IncludeOfInGame { get; set; } = new FilterItem<bool>() { Neutral = false };

        /// <summary>
        /// Include or exclude machine descriptions
        /// </summary>
        public FilterItem<string> MachineDescription { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine types
        /// </summary>
        public FilterItem<MachineType> MachineTypes { get; set; } = new FilterItem<MachineType>() { Positive = MachineType.NULL, Negative = MachineType.NULL };

        /// <summary>
        /// Include or exclude items with the "Runnable" tag
        /// </summary>
        public FilterItem<bool?> Runnable { get; set; } = new FilterItem<bool?>() { Neutral = null };

        #endregion

        #region DatItem Filters

        /// <summary>
        /// Include or exclude item names
        /// </summary>
        public FilterItem<string> ItemName { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude item types
        /// </summary>
        public FilterItem<string> ItemTypes { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude item sizes
        /// </summary>
        /// <remarks>Positive means "Greater than or equal", Negative means "Less than or equal", Neutral means "Equal"</remarks>
        public FilterItem<long> Size { get; set; } = new FilterItem<long>() { Positive = -1, Negative = -1, Neutral = -1 };

        /// <summary>
        /// Include or exclude CRC32 hashes
        /// </summary>
        public FilterItem<string> CRC { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude MD5 hashes
        /// </summary>
        public FilterItem<string> MD5 { get; set; } = new FilterItem<string>();

#if NET_FRAMEWORK
        /// <summary>
        /// Include or exclude RIPEMD160 hashes
        /// </summary>
        public FilterItem<string> RIPEMD160 { get; set; } = new FilterItem<string>();
#endif

        /// <summary>
        /// Include or exclude SHA-1 hashes
        /// </summary>
        public FilterItem<string> SHA1 { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude SHA-256 hashes
        /// </summary>
        public FilterItem<string> SHA256 { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude SHA-384 hashes
        /// </summary>
        public FilterItem<string> SHA384 { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude SHA-512 hashes
        /// </summary>
        public FilterItem<string> SHA512 { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude item statuses
        /// </summary>
        public FilterItem<ItemStatus> ItemStatuses { get; set; } = new FilterItem<ItemStatus>() { Positive = ItemStatus.NULL, Negative = ItemStatus.NULL };

        #endregion

        #region Manipulation Filters

        /// <summary>
        /// Clean all names to WoD standards
        /// </summary>
        public FilterItem<bool> Clean { get; set; } = new FilterItem<bool>() { Neutral = false };

        /// <summary>
        /// Set Machine Description from Machine Name
        /// </summary>
        public FilterItem<bool> DescriptionAsName { get; set; } = new FilterItem<bool>() { Neutral = false };

        /// <summary>
        /// Internally split a DatFile
        /// </summary>
        public FilterItem<SplitType> InternalSplit { get; set; } = new FilterItem<SplitType>() { Neutral = SplitType.None };

        /// <summary>
        /// Remove all unicode characters
        /// </summary>
        public FilterItem<bool> RemoveUnicode { get; set; } = new FilterItem<bool>() { Neutral = false };

        /// <summary>
        /// Change all machine names to "!"
        /// </summary>
        public FilterItem<bool> Single { get; set; } = new FilterItem<bool>() { Neutral = false };

        /// <summary>
        /// Trim total machine and item name to not exceed NTFS limits
        /// </summary>
        public FilterItem<bool> Trim { get; set; } = new FilterItem<bool>() { Neutral = false };

        /// <summary>
        /// Include root directory when determing trim sizes
        /// </summary>
        public FilterItem<string> Root { get; set; } = new FilterItem<string>() { Neutral = null };

        #endregion

        #endregion // Pubically facing variables

        #region Instance methods

        /// <summary>
        /// Filter a DatFile using the inputs
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="useTags">True if DatFile tags override splitting, false otherwise</param>
        /// <returns>True if the DatFile was filtered, false on error</returns>
        public bool FilterDatFile(DatFile datFile, bool useTags)
        {
            try
            {
                // Loop over every key in the dictionary
                List<string> keys = datFile.Keys;
                foreach (string key in keys)
                {
                    // For every item in the current key
                    List<DatItem> items = datFile[key];
                    List<DatItem> newitems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        // If the rom passes the filter, include it
                        if (ItemPasses(item))
                        {
                            // If we're stripping unicode characters, do so from all relevant things
                            if (this.RemoveUnicode.Neutral)
                            {
                                item.Name = Sanitizer.RemoveUnicodeCharacters(item.Name);
                                item.MachineName = Sanitizer.RemoveUnicodeCharacters(item.MachineName);
                                item.MachineDescription = Sanitizer.RemoveUnicodeCharacters(item.MachineDescription);
                            }

                            // If we're in cleaning mode, do so from all relevant things
                            if (this.Clean.Neutral)
                            {
                                item.MachineName = Sanitizer.CleanGameName(item.MachineName);
                                item.MachineDescription = Sanitizer.CleanGameName(item.MachineDescription);
                            }

                            // If we are in single game mode, rename all games
                            if (this.Single.Neutral)
                                item.MachineName = "!";

                            // If we are in NTFS trim mode, trim the game name
                            if (this.Trim.Neutral)
                            {
                                // Windows max name length is 260
                                int usableLength = 260 - item.MachineName.Length - this.Root.Neutral.Length;
                                if (item.Name.Length > usableLength)
                                {
                                    string ext = Path.GetExtension(item.Name);
                                    item.Name = item.Name.Substring(0, usableLength - ext.Length);
                                    item.Name += ext;
                                }
                            }

                            // Lock the list and add the item back
                            lock (newitems)
                            {
                                newitems.Add(item);
                            }
                        }
                    }

                    datFile.Remove(key);
                    datFile.AddRange(key, newitems);
                }

                // Process description to machine name
                if (this.DescriptionAsName.Neutral)
                    MachineDescriptionToName(datFile);

                // If we are using tags from the DAT, set the proper input for split type unless overridden
                if (useTags && this.InternalSplit.Neutral == SplitType.None)
                    this.InternalSplit.Neutral = datFile.DatHeader.ForceMerging.AsSplitType();

                // Run internal splitting
                ProcessSplitType(datFile, this.InternalSplit.Neutral);

                // We remove any blanks, if we aren't supposed to have any
                if (!datFile.DatHeader.KeepEmptyGames)
                {
                    foreach (string key in datFile.Keys)
                    {
                        List<DatItem> items = datFile[key];
                        List<DatItem> newitems = items.Where(i => i.ItemType != ItemType.Blank).ToList();

                        datFile.Remove(key);
                        datFile.AddRange(key, newitems);
                    }
                }

                // If we are removing scene dates, do that now
                if (datFile.DatHeader.SceneDateStrip)
                    StripSceneDatesFromItems(datFile);

                // Run the one rom per game logic, if required
                if (datFile.DatHeader.OneRom)
                    OneRomPerGame(datFile);
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check to see if a DatItem passes the filter
        /// </summary>
        /// <param name="item">DatItem to check</param>
        /// <returns>True if the file passed the filter, false otherwise</returns>
        public bool ItemPasses(DatItem item)
        {
            // If the item is null, we automatically fail it
            if (item == null)
                return false;

            // Filter on machine type
            if (this.MachineTypes.MatchesPositive(MachineType.NULL, item.MachineType) == false)
                return false;
            if (this.MachineTypes.MatchesNegative(MachineType.NULL, item.MachineType) == true)
                return false;

            // Filter on machine runability
            if (this.Runnable.MatchesNeutral(null, item.Runnable) == false)
                return false;

            // Take care of Rom and Disk specific differences
            if (item.ItemType == ItemType.Rom)
            {
                Rom rom = (Rom)item;

                // Filter on status
                if (this.ItemStatuses.MatchesPositive(ItemStatus.NULL, rom.ItemStatus) == false)
                    return false;
                if (this.ItemStatuses.MatchesNegative(ItemStatus.NULL, rom.ItemStatus) == true)
                    return false;

                // Filter on rom size
                if (this.Size.MatchesNeutral(-1, rom.Size) == false)
                    return false;
                else if (this.Size.MatchesPositive(-1, rom.Size) == false)
                    return false;
                else if (this.Size.MatchesNegative(-1, rom.Size) == false)
                    return false;

                // Filter on CRC
                if (this.CRC.MatchesPositiveSet(rom.CRC) == false)
                    return false;
                if (this.CRC.MatchesNegativeSet(rom.CRC) == true)
                    return false;

                // Filter on MD5
                if (this.MD5.MatchesPositiveSet(rom.MD5) == false)
                    return false;
                if (this.MD5.MatchesNegativeSet(rom.MD5) == true)
                    return false;

#if NET_FRAMEWORK
                // Filter on RIPEMD160
                if (this.RIPEMD160.MatchesPositiveSet(rom.RIPEMD160) == false)
                    return false;
                if (this.RIPEMD160.MatchesNegativeSet(rom.RIPEMD160) == true)
                    return false;
#endif

                // Filter on SHA-1
                if (this.SHA1.MatchesPositiveSet(rom.SHA1) == false)
                    return false;
                if (this.SHA1.MatchesNegativeSet(rom.SHA1) == true)
                    return false;

                // Filter on SHA-256
                if (this.SHA256.MatchesPositiveSet(rom.SHA256) == false)
                    return false;
                if (this.SHA256.MatchesNegativeSet(rom.SHA256) == true)
                    return false;

                // Filter on SHA-384
                if (this.SHA384.MatchesPositiveSet(rom.SHA384) == false)
                    return false;
                if (this.SHA384.MatchesNegativeSet(rom.SHA384) == true)
                    return false;

                // Filter on SHA-512
                if (this.SHA512.MatchesPositiveSet(rom.SHA512) == false)
                    return false;
                if (this.SHA512.MatchesNegativeSet(rom.SHA512) == true)
                    return false;
            }
            else if (item.ItemType == ItemType.Disk)
            {
                Disk rom = (Disk)item;

                // Filter on status
                if (this.ItemStatuses.MatchesPositive(ItemStatus.NULL, rom.ItemStatus) == false)
                    return false;
                if (this.ItemStatuses.MatchesNegative(ItemStatus.NULL, rom.ItemStatus) == true)
                    return false;

                // Filter on MD5
                if (this.MD5.MatchesPositiveSet(rom.MD5) == false)
                    return false;
                if (this.MD5.MatchesNegativeSet(rom.MD5) == true)
                    return false;

#if NET_FRAMEWORK
                // Filter on RIPEMD160
                if (this.RIPEMD160.MatchesPositiveSet(rom.RIPEMD160) == false)
                    return false;
                if (this.RIPEMD160.MatchesNegativeSet(rom.RIPEMD160) == true)
                    return false;
#endif

                // Filter on SHA-1
                if (this.SHA1.MatchesPositiveSet(rom.SHA1) == false)
                    return false;
                if (this.SHA1.MatchesNegativeSet(rom.SHA1) == true)
                    return false;

                // Filter on SHA-256
                if (this.SHA256.MatchesPositiveSet(rom.SHA256) == false)
                    return false;
                if (this.SHA256.MatchesNegativeSet(rom.SHA256) == true)
                    return false;

                // Filter on SHA-384
                if (this.SHA384.MatchesPositiveSet(rom.SHA384) == false)
                    return false;
                if (this.SHA384.MatchesNegativeSet(rom.SHA384) == true)
                    return false;

                // Filter on SHA-512
                if (this.SHA512.MatchesPositiveSet(rom.SHA512) == false)
                    return false;
                if (this.SHA512.MatchesNegativeSet(rom.SHA512) == true)
                    return false;
            }

            // Filter on machine name
            bool? machineNameFound = this.MachineName.MatchesPositiveSet(item.MachineName);
            if (this.IncludeOfInGame.Neutral)
            {
                machineNameFound |= (this.MachineName.MatchesPositiveSet(item.CloneOf) == true);
                machineNameFound |= (this.MachineName.MatchesPositiveSet(item.RomOf) == true);
            }
            if (machineNameFound == false)
                return false;

            machineNameFound = this.MachineName.MatchesNegativeSet(item.MachineName);
            if (this.IncludeOfInGame.Neutral)
            {
                machineNameFound |= (this.MachineName.MatchesNegativeSet(item.CloneOf) == true);
                machineNameFound |= (this.MachineName.MatchesNegativeSet(item.RomOf) == true);
            }
            if (machineNameFound == false)
                return false;

            // Filter on machine description
            if (this.MachineDescription.MatchesPositiveSet(item.MachineDescription) == false)
                return false;
            if (this.MachineDescription.MatchesNegativeSet(item.MachineDescription) == true)
                return false;

            // Filter on item name
            if (this.ItemName.MatchesPositiveSet(item.Name) == false)
                return false;
            if (this.ItemName.MatchesNegativeSet(item.Name) == true)
                return false;

            // Filter on item type
            if (this.ItemTypes.PositiveSet.Count == 0 && this.ItemTypes.NegativeSet.Count == 0
                && item.ItemType != ItemType.Rom && item.ItemType != ItemType.Disk && item.ItemType != ItemType.Blank)
                return false;
            if (this.ItemTypes.MatchesPositiveSet(item.ItemType.ToString()) == false)
                return false;
            if (this.ItemTypes.MatchesNegativeSet(item.ItemType.ToString()) == true)
                return false;

            return true;
        }

        #region Internal Splitting/Merging

        /// <summary>
        /// Process items according to SplitType
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="splitType">SplitType to implement</param>
        private void ProcessSplitType(DatFile datFile, SplitType splitType)
        {
            // Now we pre-process the DAT with the splitting/merging mode
            switch (splitType)
            {
                case SplitType.None:
                    // No-op
                    break;
                case SplitType.DeviceNonMerged:
                    CreateDeviceNonMergedSets(datFile, DedupeType.None);
                    break;
                case SplitType.FullNonMerged:
                    CreateFullyNonMergedSets(datFile, DedupeType.None);
                    break;
                case SplitType.NonMerged:
                    CreateNonMergedSets(datFile, DedupeType.None);
                    break;
                case SplitType.Merged:
                    CreateMergedSets(datFile, DedupeType.None);
                    break;
                case SplitType.Split:
                    CreateSplitSets(datFile, DedupeType.None);
                    break;
            }
        }

        /// <summary>
        /// Use cdevice_ref tags to get full non-merged sets and remove parenting tags
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateDeviceNonMergedSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating device non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddRomsFromDevices(datFile, false, false)) ;
            while (AddRomsFromDevices(datFile, true, false)) ;

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags plus using the device_ref tags to get full sets
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateFullyNonMergedSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating fully non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is sort by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddRomsFromDevices(datFile, true, true)) ;
            AddRomsFromDevices(datFile, false, true);
            AddRomsFromParent(datFile);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            AddRomsFromBios(datFile);

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use cloneof tags to create merged sets and remove the tags
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateMergedSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating merged sets from the DAT");

            // For sake of ease, the first thing we want to do is sort by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddRomsFromChildren(datFile);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(datFile, false);
            RemoveBiosRomsFromChild(datFile, true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateNonMergedSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is sort by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddRomsFromParent(datFile);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(datFile, false);
            RemoveBiosRomsFromChild(datFile, true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use cloneof and romof tags to create split sets and remove the tags
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateSplitSets(DatFile datFile, DedupeType mergeroms)
        {
            Globals.Logger.User("Creating split sets from the DAT");

            // For sake of ease, the first thing we want to do is sort by game
            datFile.BucketBy(BucketedBy.Game, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            RemoveRomsFromChild(datFile);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(datFile, false);
            RemoveBiosRomsFromChild(datFile, true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild(datFile);
        }

        /// <summary>
        /// Use romof tags to add roms to the children
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void AddRomsFromBios(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].RomOf))
                    parent = datFile[game][0].RomOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (datFile[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = datFile[game][0];
                List<DatItem> parentItems = datFile[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (datFile[game].Where(i => i.Name == datItem.Name).Count() == 0 && !datFile[game].Contains(datItem))
                        datFile.Add(game, datItem);
                }
            }
        }

        /// <summary>
        /// Use device_ref and optionally slotoption tags to add roms to the children
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="dev">True if only child device sets are touched, false for non-device sets (default)</param>
        /// <param name="slotoptions">True if slotoptions tags are used as well, false otherwise</param>
        private bool AddRomsFromDevices(DatFile datFile, bool dev = false, bool slotoptions = false)
        {
            bool foundnew = false;
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game doesn't have items, we continue
                if (datFile[game] == null || datFile[game].Count == 0)
                    continue;

                // If the game (is/is not) a bios, we want to continue
                if (dev ^ (datFile[game][0].MachineType.HasFlag(MachineType.Device)))
                    continue;

                // If the game has no devices, we continue
                if (datFile[game][0].Devices == null
                    || datFile[game][0].Devices.Count == 0
                    || (slotoptions && datFile[game][0].SlotOptions == null)
                    || (slotoptions && datFile[game][0].SlotOptions.Count == 0))
                {
                    continue;
                }

                // Determine if the game has any devices or not
                List<string> devices = datFile[game][0].Devices;
                List<string> newdevs = new List<string>();
                foreach (string device in devices)
                {
                    // If the device doesn't exist then we continue
                    if (datFile[device].Count == 0)
                        continue;

                    // Otherwise, copy the items from the device to the current game
                    DatItem copyFrom = datFile[game][0];
                    List<DatItem> devItems = datFile[device];
                    foreach (DatItem item in devItems)
                    {
                        DatItem datItem = (DatItem)item.Clone();
                        newdevs.AddRange(datItem.Devices ?? new List<string>());
                        datItem.CopyMachineInformation(copyFrom);
                        if (datFile[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0)
                        {
                            foundnew = true;
                            datFile.Add(game, datItem);
                        }
                    }
                }

                // Now that every device is accounted for, add the new list of devices, if they don't already exist
                foreach (string device in newdevs)
                {
                    if (!datFile[game][0].Devices.Contains(device))
                        datFile[game][0].Devices.Add(device);
                }

                // If we're checking slotoptions too
                if (slotoptions)
                {
                    // Determine if the game has any slotoptions or not
                    List<string> slotopts = datFile[game][0].SlotOptions;
                    List<string> newslotopts = new List<string>();
                    foreach (string slotopt in slotopts)
                    {
                        // If the slotoption doesn't exist then we continue
                        if (datFile[slotopt].Count == 0)
                            continue;

                        // Otherwise, copy the items from the slotoption to the current game
                        DatItem copyFrom = datFile[game][0];
                        List<DatItem> slotItems = datFile[slotopt];
                        foreach (DatItem item in slotItems)
                        {
                            DatItem datItem = (DatItem)item.Clone();
                            newslotopts.AddRange(datItem.SlotOptions ?? new List<string>());
                            datItem.CopyMachineInformation(copyFrom);
                            if (datFile[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0)
                            {
                                foundnew = true;
                                datFile.Add(game, datItem);
                            }
                        }
                    }

                    // Now that every slotoption is accounted for, add the new list of slotoptions, if they don't already exist
                    foreach (string slotopt in newslotopts)
                    {
                        if (!datFile[game][0].SlotOptions.Contains(slotopt))
                            datFile[game][0].SlotOptions.Add(slotopt);
                    }
                }
            }

            return foundnew;
        }

        /// <summary>
        /// Use cloneof tags to add roms to the children, setting the new romof tag in the process
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void AddRomsFromParent(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].CloneOf))
                    parent = datFile[game][0].CloneOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (datFile[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = datFile[game][0];
                List<DatItem> parentItems = datFile[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (datFile[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0
                        && !datFile[game].Contains(datItem))
                    {
                        datFile.Add(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the items
                List<DatItem> items = datFile[game];
                string romof = datFile[parent][0].RomOf;
                foreach (DatItem item in items)
                {
                    item.RomOf = romof;
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to add roms to the parents, removing the child sets in the process
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void AddRomsFromChildren(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].CloneOf))
                    parent = datFile[game][0].CloneOf;

                // If there is no parent, then we continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // Otherwise, move the items from the current game to a subfolder of the parent game
                DatItem copyFrom = datFile[parent].Count == 0 ? new Rom { MachineName = parent, MachineDescription = parent } : datFile[parent][0];
                List<DatItem> items = datFile[game];
                foreach (DatItem item in items)
                {
                    // If the disk doesn't have a valid merge tag OR the merged file doesn't exist in the parent, then add it
                    if (item.ItemType == ItemType.Disk && (((Disk)item).MergeTag == null || !datFile[parent].Select(i => i.Name).Contains(((Disk)item).MergeTag)))
                    {
                        item.CopyMachineInformation(copyFrom);
                        datFile.Add(parent, item);
                    }

                    // Otherwise, if the parent doesn't already contain the non-disk (or a merge-equivalent), add it
                    else if (item.ItemType != ItemType.Disk && !datFile[parent].Contains(item))
                    {
                        // Rename the child so it's in a subfolder
                        item.Name = $"{item.MachineName}\\{item.Name}";

                        // Update the machine to be the new parent
                        item.CopyMachineInformation(copyFrom);

                        // Add the rom to the parent set
                        datFile.Add(parent, item);
                    }
                }

                // Then, remove the old game so it's not picked up by the writer
                datFile.Remove(game);
            }
        }

        /// <summary>
        /// Remove all BIOS and device sets
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void RemoveBiosAndDeviceSets(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                if (datFile[game].Count > 0
                    && (datFile[game][0].MachineType.HasFlag(MachineType.Bios)
                        || datFile[game][0].MachineType.HasFlag(MachineType.Device)))
                {
                    datFile.Remove(game);
                }
            }
        }

        /// <summary>
        /// Use romof tags to remove bios roms from children
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        /// <param name="bios">True if only child Bios sets are touched, false for non-bios sets (default)</param>
        private void RemoveBiosRomsFromChild(DatFile datFile, bool bios = false)
        {
            // Loop through the romof tags
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // If the game (is/is not) a bios, we want to continue
                if (bios ^ datFile[game][0].MachineType.HasFlag(MachineType.Bios))
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].RomOf))
                    parent = datFile[game][0].RomOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (datFile[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we remove the items that are in the parent from the current game
                List<DatItem> parentItems = datFile[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (datFile[game].Contains(datItem))
                    {
                        datFile.Remove(game, datItem);
                    }
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to remove roms from the children
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void RemoveRomsFromChild(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (datFile[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(datFile[game][0].CloneOf))
                    parent = datFile[game][0].CloneOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (datFile[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we remove the parent items from the current game
                List<DatItem> parentItems = datFile[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (datFile[game].Contains(datItem))
                    {
                        datFile.Remove(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the remaining items
                List<DatItem> items = datFile[game];
                string romof = datFile[parent][0].RomOf;
                foreach (DatItem item in items)
                {
                    item.RomOf = romof;
                }
            }
        }

        /// <summary>
        /// Remove all romof and cloneof tags from all games
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void RemoveTagsFromChild(DatFile datFile)
        {
            List<string> games = datFile.Keys;
            foreach (string game in games)
            {
                List<DatItem> items = datFile[game];
                foreach (DatItem item in items)
                {
                    item.CloneOf = null;
                    item.RomOf = null;
                }
            }
        }

        #endregion

        #region Manipulation

        /// <summary>
        /// Use game descriptions as names in the DAT, updating cloneof/romof/sampleof
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void MachineDescriptionToName(DatFile datFile)
        {
            try
            {
                // First we want to get a mapping for all games to description
                ConcurrentDictionary<string, string> mapping = new ConcurrentDictionary<string, string>();
                List<string> keys = datFile.Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = datFile[key];
                    foreach (DatItem item in items)
                    {
                        // If the key mapping doesn't exist, add it
                        mapping.TryAdd(item.MachineName, item.MachineDescription.Replace('/', '_').Replace("\"", "''").Replace(":", " -"));
                    }
                });

                // Now we loop through every item and update accordingly
                keys = datFile.Keys;
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = datFile[key];
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
                    datFile.Remove(key);
                    datFile.AddRange(key, newItems);
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
        /// <param name="datFile">DatFile to filter</param>
        /// TODO: This is incorrect for the actual 1G1R logic... this is actually just silly
        private void OneRomPerGame(DatFile datFile)
        {
            // For each rom, we want to update the game to be "<game name>/<rom name>"
            Parallel.ForEach(datFile.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = datFile[key];
                for (int i = 0; i < items.Count; i++)
                {
                    string[] splitname = items[i].Name.Split('.');
                    items[i].MachineName += $"/{string.Join(".", splitname.Take(splitname.Length > 1 ? splitname.Length - 1 : 1))}";
                }
            });
        }

        /// <summary>
        /// Strip the dates from the beginning of scene-style set names
        /// </summary>
        /// <param name="datFile">DatFile to filter</param>
        private void StripSceneDatesFromItems(DatFile datFile)
        {
            // Output the logging statement
            Globals.Logger.User("Stripping scene-style dates");

            // Set the regex pattern to use
            string pattern = @"([0-9]{2}\.[0-9]{2}\.[0-9]{2}-)(.*?-.*?)";

            // Now process all of the roms
            List<string> keys = datFile.Keys;
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = datFile[key];
                for (int j = 0; j < items.Count; j++)
                {
                    DatItem item = items[j];
                    if (Regex.IsMatch(item.MachineName, pattern))
                        item.MachineName = Regex.Replace(item.MachineName, pattern, "$2");

                    if (Regex.IsMatch(item.MachineDescription, pattern))
                        item.MachineDescription = Regex.Replace(item.MachineDescription, pattern, "$2");

                    items[j] = item;
                }

                datFile.Remove(key);
                datFile.AddRange(key, items);
            });
        }

        #endregion

        #endregion // Instance Methods
    }
}
