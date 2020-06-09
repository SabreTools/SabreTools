using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents the filtering operations that need to be performed on a set of items, usually a DAT
    /// </summary>
    /// TODO: Can this use `Field` instead of explicit filters?
    public class Filter
    {
        #region Pubically facing variables

        /// <summary>
        /// Include or exclude machine names
        /// </summary>
        public FilterItem<string> MachineName { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude machine descriptions
        /// </summary>
        public FilterItem<string> MachineDescription { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude item names
        /// </summary>
        public FilterItem<string> ItemName { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude item types
        /// </summary>
        public FilterItem<string> ItemTypes { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude CRC32 hashes
        /// </summary>
        public FilterItem<string> CRC { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude MD5 hashes
        /// </summary>
        public FilterItem<string> MD5 { get; set; } = new FilterItem<string>();

        /// <summary>
        /// Include or exclude RIPEMD160 hashes
        /// </summary>
        public FilterItem<string> RIPEMD160 { get; set; } = new FilterItem<string>();

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

        /// <summary>
        /// Include or exclude machine types
        /// </summary>
        public FilterItem<MachineType> MachineTypes { get; set; } = new FilterItem<MachineType>() { Positive = MachineType.NULL, Negative = MachineType.NULL };

        /// <summary>
        /// Include or exclude item sizes
        /// </summary>
        /// <remarks>Positive means "Greater than or equal", Negative means "Less than or equal", Neutral means "Equal"</remarks>
        public FilterItem<long> Size { get; set; } = new FilterItem<long>() { Positive = -1, Negative = -1, Neutral = -1 };

        /// <summary>
        /// Include romof and cloneof when filtering machine names
        /// </summary>
        public FilterItem<bool> IncludeOfInGame { get; set; } = new FilterItem<bool>() { Neutral = false };

        /// <summary>
        /// Include or exclude items with the "Runnable" tag
        /// </summary>
        public FilterItem<bool?> Runnable { get; set; } = new FilterItem<bool?>() { Neutral = null };

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

        #endregion // Pubically facing variables

        #region Instance methods

        /// <summary>
        /// Filter a DatFile using the inputs
        /// </summary>
        /// <param name="datFile"></param>
        /// <returns>True if the DatFile was filtered, false on error</returns>
        public bool FilterDatFile(DatFile datFile)
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

                // Filter on RIPEMD160
                if (this.RIPEMD160.MatchesPositiveSet(rom.RIPEMD160) == false)
                    return false;
                if (this.RIPEMD160.MatchesNegativeSet(rom.RIPEMD160) == true)
                    return false;

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

                // Filter on RIPEMD160
                if (this.RIPEMD160.MatchesPositiveSet(rom.RIPEMD160) == false)
                    return false;
                if (this.RIPEMD160.MatchesNegativeSet(rom.RIPEMD160) == true)
                    return false;

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

        #endregion
    }
}
