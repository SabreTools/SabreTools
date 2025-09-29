using System;
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
        /// Normalize all names to WoD standards
        /// </summary>
        public bool Normalize { get; set; }

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
        private readonly Logger _logger = new();

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
                    datFile.BucketBy(ItemKey.CRC);
                    datFile.Deduplicate();
                }
                else if (DedupeRoms == DedupeType.Game)
                {
                    datFile.BucketBy(ItemKey.Machine);
                    datFile.Deduplicate();
                }

                // Process description to machine name
                if (DescriptionAsName == true)
                    datFile.MachineDescriptionToName(throwOnError);

                // If we are removing scene dates, do that now
                if (SceneDateStrip == true)
                    datFile.StripSceneDatesFromItems();

                // Run the one rom per game logic, if required
                if (OneGamePerRegion == true && RegionList != null)
                    datFile.SetOneGamePerRegion(RegionList);

                // Run the one rom per game logic, if required
                if (OneRomPerGame == true)
                    datFile.SetOneRomPerGame();

                // Remove all marked items
                datFile.ClearMarked();

                // We remove any blanks, if we aren't supposed to have any
                if (KeepEmptyGames == false)
                    datFile.ClearEmpty();
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Error(ex);
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
            foreach (string key in datFile.Items.SortedKeys)
            {
                // For every item in the current key
                var items = datFile.GetItemsForBucket(key);
                if (items == null)
                    continue;

                foreach (DatItem item in items)
                {
                    // If we have a null item, we can't clean it it
                    if (item == null)
                        continue;

                    // Get the machine associated with the item, if possible
                    var machine = item.GetMachine();
                    if (machine == null)
                        continue;

                    // Run cleaning per item
                    CleanDatItem(item, machine);
                }
            }
        }

        /// <summary>
        /// Clean individual items based on the current filter
        /// </summary>
        /// <param name="datFile">Current DatFile object to run operations on</param>
        internal void CleanDatItemsDB(DatFile datFile)
        {
            foreach (string key in datFile.ItemsDB.SortedKeys)
            {
                // For every item in the current key
                var items = datFile.GetItemsForBucketDB(key);
                if (items == null)
                    continue;

                foreach (var item in items)
                {
                    // If we have a null item, we can't clean it it
                    if (item.Value == null)
                        continue;

                    // Get the machine associated with the item, if possible
                    var machine = datFile.GetMachineForItemDB(item.Key);
                    if (machine.Value == null)
                        continue;

                    // Run cleaning per item
                    CleanDatItem(item.Value, machine.Value);
                }
            }
        }

        /// <summary>
        /// Clean a DatItem according to the cleaner
        /// </summary>
        /// <param name="datItem">DatItem to clean</param>
        /// <param name="machine">Machine related to the DatItem to clean</param>
        internal void CleanDatItem(DatItem datItem, Machine machine)
        {
            // Get the fields for processing
            string? machineName = machine.GetName();
            string? machineDesc = machine.GetStringFieldValue(Data.Models.Metadata.Machine.DescriptionKey);
            string? datItemName = datItem.GetName();

            // If we're stripping unicode characters, strip machine name and description
            if (RemoveUnicode)
            {
                machineName = TextHelper.RemoveUnicodeCharacters(machineName);
                machineDesc = TextHelper.RemoveUnicodeCharacters(machineDesc);
                datItemName = TextHelper.RemoveUnicodeCharacters(datItemName);
            }

            // If we're in normalization mode, sanitize machine name and description
            if (Normalize)
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
            machine.SetName(machineName);
            machine.SetFieldValue<string?>(Data.Models.Metadata.Machine.DescriptionKey, machineDesc);
            datItem.SetName(datItemName);
        }

        #endregion
    }
}
