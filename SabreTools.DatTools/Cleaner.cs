﻿using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Core.Tools;
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.IO.Logging;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Represents the cleaning operations that need to be performed on a set of items, usually a DAT
    /// </summary>
    public class Cleaner
    {
        #region Fields

        /// <summary>
        /// Clean all names to WoD standards
        /// </summary>
        public bool Clean { get; set; }

        /// <summary>
        /// Deduplicate items using the given method
        /// </summary>
        public DedupeType DedupeRoms { get; set; }

        /// <summary>
        /// Set Machine Description from Machine Name
        /// </summary>
        public bool DescriptionAsName { get; set; }

        /// <summary>
        /// Keep machines that don't contain any items
        /// </summary>
        public bool KeepEmptyGames { get; set; }

        /// <summary>
        /// Enable "One Rom, One Region (1G1R)" mode
        /// </summary>
        public bool OneGamePerRegion { get; set; }

        /// <summary>
        /// Ordered list of regions for "One Rom, One Region (1G1R)" mode
        /// </summary>
        public List<string>? RegionList { get; set; }

        /// <summary>
        /// Ensure each rom is in their own game
        /// </summary>
        public bool OneRomPerGame { get; set; }

        /// <summary>
        /// Remove all unicode characters
        /// </summary>
        public bool RemoveUnicode { get; set; }

        /// <summary>
        /// Include root directory when determing trim sizes
        /// </summary>
        public string? Root { get; set; }

        /// <summary>
        /// Remove scene dates from the beginning of machine names
        /// </summary>
        public bool SceneDateStrip { get; set; }

        /// <summary>
        /// Change all machine names to "!"
        /// </summary>
        public bool Single { get; set; }

        /// <summary>
        /// Trim total machine and item name to not exceed NTFS limits
        /// </summary>
        public bool Trim { get; set; }

        #endregion

        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private readonly Logger logger = new();

        #endregion

        #region Running

        /// <summary>
        /// Apply cleaning methods to the DatFile
        /// </summary>
        /// <param name="datFile">Current DatFile object to run operations on</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if cleaning was successful, false on error</returns>
        public bool ApplyCleaning(DatFile datFile, bool throwOnError = false)
        {
            InternalStopwatch watch = new("Applying cleaning steps to DAT");

            try
            {
                // Perform item-level cleaning
                CleanDatItems(datFile);
                CleanDatItemsDB(datFile);

                // Bucket and dedupe according to the flag
                if (DedupeRoms == DedupeType.Full)
                {
                    datFile.Items.BucketBy(ItemKey.CRC, DedupeRoms);
                    datFile.ItemsDB.BucketBy(ItemKey.CRC, DedupeRoms);
                }
                else if (DedupeRoms == DedupeType.Game)
                {
                    datFile.Items.BucketBy(ItemKey.Machine, DedupeRoms);
                    datFile.ItemsDB.BucketBy(ItemKey.Machine, DedupeRoms);
                }

                // Process description to machine name
                if (DescriptionAsName == true)
                {
                    datFile.Items.MachineDescriptionToName(throwOnError);
                    datFile.ItemsDB.MachineDescriptionToName(throwOnError);
                }

                // If we are removing scene dates, do that now
                if (SceneDateStrip == true)
                {
                    datFile.Items.StripSceneDatesFromItems();
                    datFile.ItemsDB.StripSceneDatesFromItems();
                }

                // Run the one rom per game logic, if required
                if (OneGamePerRegion == true && RegionList != null)
                {
                    datFile.Items.SetOneGamePerRegion(RegionList);
                    datFile.ItemsDB.SetOneGamePerRegion(RegionList);
                }

                // Run the one rom per game logic, if required
                if (OneRomPerGame == true)
                {
                    datFile.Items.SetOneRomPerGame();
                    datFile.ItemsDB.SetOneRomPerGame();
                }

                // Remove all marked items
                datFile.Items.ClearMarked();
                datFile.ItemsDB.ClearMarked();

                // We remove any blanks, if we aren't supposed to have any
                if (KeepEmptyGames == false)
                {
                    datFile.Items.ClearEmpty();
                    datFile.ItemsDB.ClearEmpty();
                }
            }
            catch (Exception ex) when (!throwOnError)
            {
                logger.Error(ex);
                return false;
            }
            finally
            {
                watch.Stop();
            }

            return true;
        }

        /// <summary>
        /// Clean individual items based on the current filter
        /// </summary>
        /// <param name="datFile">Current DatFile object to run operations on</param>
        internal void CleanDatItems(DatFile datFile)
        {
            List<string> keys = [.. datFile.Items.Keys];
            foreach (string key in keys)
            {
                // For every item in the current key
                var items = datFile.Items[key];
                if (items == null)
                    continue;

                foreach (DatItem item in items)
                {
                    // If we have a null item, we can't clean it it
                    if (item == null)
                        continue;

                    // Run cleaning per item
                    CleanDatItem(item);
                }

                // Assign back for caution
                datFile.Items[key] = items;
            }
        }

        /// <summary>
        /// Clean individual items based on the current filter
        /// </summary>
        /// <param name="datFile">Current DatFile object to run operations on</param>
        internal void CleanDatItemsDB(DatFile datFile)
        {
            List<string> keys = [.. datFile.ItemsDB.SortedKeys];
            foreach (string key in keys)
            {
                // For every item in the current key
                var items = datFile.ItemsDB.GetItemsForBucket(key);
                if (items == null)
                    continue;

                foreach (var item in items)
                {
                    // If we have a null item, we can't clean it it
                    if (item.Value == null)
                        continue;

                    // Run cleaning per item
                    CleanDatItemDB(datFile.ItemsDB, item);
                }
            }
        }

        /// <summary>
        /// Clean a DatItem according to the cleaner
        /// </summary>
        /// <param name="datItem">DatItem to clean</param>
        internal void CleanDatItem(DatItem datItem)
        {
            // Get the machine associated with the item, if possible
            var machine = datItem.GetFieldValue<Machine>(DatItem.MachineKey);
            if (machine == null)
                return;

            // Get the fields for processing
            string? machineName = machine.GetStringFieldValue(Models.Metadata.Machine.NameKey);
            string? machineDesc = machine.GetStringFieldValue(Models.Metadata.Machine.DescriptionKey);
            string? datItemName = datItem.GetName();

            // If we're stripping unicode characters, strip machine name and description
            if (RemoveUnicode)
            {
                machineName = TextHelper.RemoveUnicodeCharacters(machineName);
                machineDesc = TextHelper.RemoveUnicodeCharacters(machineDesc);
                datItemName = TextHelper.RemoveUnicodeCharacters(datItemName);
            }

            // If we're in cleaning mode, sanitize machine name and description
            if (Clean)
            {
                machineName = TextHelper.NormalizeCharacters(machineName);
                machineDesc = TextHelper.NormalizeCharacters(machineDesc);
            }

            // If we are in single game mode, rename the machine
            if (Single)
            {
                machineName = "!";
                machineDesc = "!";
            }

            // If we are in NTFS trim mode, trim the item name
            if (Trim && datItemName != null)
            {
                // Windows max name length is 260
                int usableLength = 260 - (machineName?.Length ?? 0) - (Root?.Length ?? 0);
                if (datItemName.Length > usableLength)
                {
                    string ext = Path.GetExtension(datItemName);
                    datItemName = datItemName.Substring(0, usableLength - ext.Length) + ext;
                }
            }

            // Set the fields back, if necessary
            machine.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, machineName);
            machine.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, machineDesc);
            datItem.SetName(datItemName);
        }

        /// <summary>
        /// Clean a DatItem according to the cleaner
        /// </summary>
        /// <param name="db">ItemDictionaryDB to get machine information from</param>
        /// <param name="datItem">DatItem to clean</param>
        internal void CleanDatItemDB(ItemDictionaryDB db, KeyValuePair<long, DatItem> datItem)
        {
            // Get the machine associated with the item, if possible
            var machine = db.GetMachineForItem(datItem.Key);
            if (machine.Value == null)
                return;

            // Get the fields for processing
            string? machineName = machine.Value.GetStringFieldValue(Models.Metadata.Machine.NameKey);
            string? machineDesc = machine.Value.GetStringFieldValue(Models.Metadata.Machine.DescriptionKey);
            string? datItemName = datItem.Value.GetName();

            // If we're stripping unicode characters, strip machine name and description
            if (RemoveUnicode)
            {
                machineName = TextHelper.RemoveUnicodeCharacters(machineName);
                machineDesc = TextHelper.RemoveUnicodeCharacters(machineDesc);
                datItemName = TextHelper.RemoveUnicodeCharacters(datItemName);
            }

            // If we're in cleaning mode, sanitize machine name and description
            if (Clean)
            {
                machineName = TextHelper.NormalizeCharacters(machineName);
                machineDesc = TextHelper.NormalizeCharacters(machineDesc);
            }

            // If we are in single game mode, rename the machine
            if (Single)
            {
                machineName = "!";
                machineDesc = "!";
            }

            // If we are in NTFS trim mode, trim the item name
            if (Trim && datItemName != null)
            {
                // Windows max name length is 260
                int usableLength = 260 - (machineName?.Length ?? 0) - (Root?.Length ?? 0);
                if (datItemName.Length > usableLength)
                {
                    string ext = Path.GetExtension(datItemName);
                    datItemName = datItemName.Substring(0, usableLength - ext.Length) + ext;
                }
            }

            // Set the fields back, if necessary
            machine.Value.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, machineName);
            machine.Value.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, machineDesc);
            datItem.Value.SetName(datItemName);
        }

        #endregion
    }
}
