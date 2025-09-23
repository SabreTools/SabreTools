using System.Collections.Generic;
using System.Linq;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.IO.Logging;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Replace fields in DatItems
    /// </summary>
    /// TODO: Add tests for BaseReplace methods
    public static class Replacer
    {
        #region BaseReplace

        /// <summary>
        /// Replace item values from the base set represented by the current DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="intDat">DatFile to replace the values in</param>
        /// <param name="machineFieldNames">List of machine field names representing what should be updated</param>
        /// <param name="itemFieldNames">List of item field names representing what should be updated</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise</param>
        public static void BaseReplace(
            DatFile datFile,
            DatFile intDat,
            List<string> machineFieldNames,
            Dictionary<string, List<string>> itemFieldNames,
            bool onlySame)
        {
            InternalStopwatch watch = new($"Replacing items in '{intDat.Header.GetStringFieldValue(DatHeader.FileNameKey)}' from the base DAT");

            // If we are matching based on DatItem fields of any sort
            BaseReplaceItemsImpl(datFile, intDat, itemFieldNames);
            BaseReplaceItemsDBImpl(datFile, intDat, itemFieldNames);

            // If we are matching based on Machine fields of any sort
            BaseReplaceMachinesImpl(datFile, intDat, machineFieldNames, onlySame);
            BaseReplaceMachinesDBImpl(datFile, intDat, machineFieldNames, onlySame);

            watch.Stop();
        }

        /// <summary>
        /// Replace item values from the base set represented by the current DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="intDat">DatFile to replace the values in</param>
        /// <param name="itemFieldNames">List of item field names representing what should be updated</param>
        private static void BaseReplaceItemsImpl(
            DatFile datFile,
            DatFile intDat,
            Dictionary<string, List<string>> itemFieldNames)
        {
            // Check for field names
            if (itemFieldNames.Count == 0)
                return;

            // For comparison's sake, we want to use CRC as the base bucketing
            datFile.BucketBy(ItemKey.CRC);
            datFile.Deduplicate();
            intDat.BucketBy(ItemKey.CRC);

            // Then we do a hashwise comparison against the base DAT
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(intDat.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(intDat.Items.SortedKeys, key =>
#else
            foreach (var key in intDat.Items.SortedKeys)
#endif
            {
                List<DatItem>? datItems = intDat.GetItemsForBucket(key);
                if (datItems == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                List<DatItem> newDatItems = [];
                foreach (DatItem datItem in datItems)
                {
                    List<DatItem> dupes = datFile.GetDuplicates(datItem, sorted: true);
                    if (datItem.Clone() is not DatItem newDatItem)
                        continue;

                    // Replace fields from the first duplicate, if we have one
                    if (dupes.Count > 0)
                        ReplaceFields(newDatItem, dupes[0], itemFieldNames);

                    newDatItems.Add(newDatItem);
                }

                // Now add the new list to the key
                intDat.RemoveBucket(key);
                newDatItems.ForEach(item => intDat.AddItem(item, statsOnly: false));
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Replace item values from the base set represented by the current DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="intDat">DatFile to replace the values in</param>
        /// <param name="itemFieldNames">List of item field names representing what should be updated</param>
        private static void BaseReplaceItemsDBImpl(
            DatFile datFile,
            DatFile intDat,
            Dictionary<string, List<string>> itemFieldNames)
        {
            // Check for field names
            if (itemFieldNames.Count == 0)
                return;

            // For comparison's sake, we want to use CRC as the base bucketing
            datFile.BucketBy(ItemKey.CRC);
            datFile.Deduplicate();
            intDat.BucketBy(ItemKey.CRC);

            // Then we do a hashwise comparison against the base DAT
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(intDat.ItemsDB.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(intDat.ItemsDB.SortedKeys, key =>
#else
            foreach (var key in intDat.ItemsDB.SortedKeys)
#endif
            {
                var datItems = intDat.GetItemsForBucketDB(key);
                if (datItems == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                foreach (var datItem in datItems)
                {
                    var dupes = datFile.GetDuplicatesDB(datItem, sorted: true);
                    if (datItem.Value.Clone() is not DatItem newDatItem)
                        continue;

                    // Replace fields from the first duplicate, if we have one
                    if (dupes.Count > 0)
                        ReplaceFields(datItem.Value, dupes.First().Value, itemFieldNames);
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Replace machine values from the base set represented by the current DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="intDat">DatFile to replace the values in</param>
        /// <param name="machineFieldNames">List of machine field names representing what should be updated</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise</param>
        private static void BaseReplaceMachinesImpl(
            DatFile datFile,
            DatFile intDat,
            List<string> machineFieldNames,
            bool onlySame)
        {
            // Check for field names
            if (machineFieldNames.Count == 0)
                return;

            // For comparison's sake, we want to use Machine Name as the base bucketing
            datFile.BucketBy(ItemKey.Machine);
            datFile.Deduplicate();
            intDat.BucketBy(ItemKey.Machine);

            // Then we do a namewise comparison against the base DAT
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(intDat.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(intDat.Items.SortedKeys, key =>
#else
            foreach (var key in intDat.Items.SortedKeys)
#endif
            {
                List<DatItem>? datItems = intDat.GetItemsForBucket(key);
                if (datItems == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                List<DatItem> newDatItems = [];
                foreach (DatItem datItem in datItems)
                {
                    if (datItem.Clone() is not DatItem newDatItem)
                        continue;

                    var list = datFile.GetItemsForBucket(key);
                    if (list.Count > 0)
                        ReplaceFields(newDatItem.GetMachine()!, list[index: 0].GetMachine()!, machineFieldNames, onlySame);

                    newDatItems.Add(newDatItem);
                }

                // Now add the new list to the key
                intDat.RemoveBucket(key);
                newDatItems.ForEach(item => intDat.AddItem(item, statsOnly: false));
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Replace machine values from the base set represented by the current DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="intDat">DatFile to replace the values in</param>
        /// <param name="machineFieldNames">List of machine field names representing what should be updated</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise</param>
        private static void BaseReplaceMachinesDBImpl(
            DatFile datFile,
            DatFile intDat,
            List<string> machineFieldNames,
            bool onlySame)
        {
            // Check for field names
            if (machineFieldNames.Count == 0)
                return;

            // For comparison's sake, we want to use Machine Name as the base bucketing
            datFile.BucketBy(ItemKey.Machine);
            datFile.Deduplicate();
            intDat.BucketBy(ItemKey.Machine);

            // Then we do a namewise comparison against the base DAT
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(intDat.ItemsDB.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(intDat.ItemsDB.SortedKeys, key =>
#else
            foreach (var key in intDat.ItemsDB.SortedKeys)
#endif
            {
                var datItems = intDat.GetItemsForBucketDB(key);
                if (datItems == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                foreach (var datItem in datItems)
                {
                    var datMachine = datFile.GetMachineForItemDB(datFile.GetItemsForBucketDB(key)!.First().Key);
                    var intMachine = intDat.GetMachineForItemDB(datItem.Key);
                    if (datMachine.Value != null && intMachine.Value != null)
                        ReplaceFields(intMachine.Value, datMachine.Value, machineFieldNames, onlySame);
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        #endregion

        #region ReplaceFields

        /// <summary>
        /// Replace fields with given values
        /// </summary>
        /// <param name="machine">Machine to replace fields in</param>
        /// <param name="repMachine">Machine to pull new information from</param>
        /// <param name="machineFieldNames">List of fields representing what should be updated</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise</param>
        public static void ReplaceFields(Machine machine, Machine repMachine, List<string> machineFieldNames, bool onlySame)
        {
            // If we have an invalid input, return
            if (machine == null || repMachine == null || machineFieldNames == null)
                return;

            // Loop through and replace fields
            foreach (string fieldName in machineFieldNames)
            {
                // Special case for description
                if (machineFieldNames.Contains(Models.Metadata.Machine.DescriptionKey))
                {
                    if (!onlySame || (onlySame && machine.GetName() == machine.GetStringFieldValue(Models.Metadata.Machine.DescriptionKey)))
                        machine.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, repMachine.GetStringFieldValue(Models.Metadata.Machine.DescriptionKey));

                    continue;
                }

                machine.ReplaceField(repMachine, fieldName);
            }
        }

        /// <summary>
        /// Replace fields with given values
        /// </summary>
        /// <param name="datItem">DatItem to replace fields in</param>
        /// <param name="repDatItem">DatItem to pull new information from</param>
        /// <param name="itemFieldNames">List of fields representing what should be updated</param>
        public static void ReplaceFields(DatItem datItem, DatItem repDatItem, Dictionary<string, List<string>> itemFieldNames)
        {
            // If we have an invalid input, return
            if (datItem == null || repDatItem == null || itemFieldNames == null)
                return;

            #region Common

            if (datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) != repDatItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey))
                return;

            // If there are no field names for this type or generic, return
            string? itemType = datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue();
            if (itemType == null || (!itemFieldNames.ContainsKey(itemType) && !itemFieldNames.ContainsKey("item")))
                return;

            // Get the combined list of fields to remove
            var fieldNames = new HashSet<string>();
            if (itemFieldNames.ContainsKey(itemType))
                fieldNames.UnionWith(itemFieldNames[itemType]);
            if (itemFieldNames.ContainsKey("item"))
                fieldNames.UnionWith(itemFieldNames["item"]);

            // If the field specifically contains Name, set it separately
            if (fieldNames.Contains(Models.Metadata.Rom.NameKey))
                datItem.SetName(repDatItem.GetName());

            #endregion

            #region Item-Specific

            // Handle normal sets first
            foreach (var fieldName in fieldNames)
            {
                datItem.ReplaceField(repDatItem, fieldName);
            }

            // TODO: Filter out hashes before here so these checks actually work
            // Handle special cases
            switch (datItem, repDatItem)
            {
                case (Disk disk, Disk repDisk): ReplaceFields(disk, repDisk, [.. fieldNames]); break;
                case (DatItems.Formats.File file, DatItems.Formats.File repFile): ReplaceFields(file, repFile, [.. fieldNames]); break;
                case (Media media, Media repMedia): ReplaceFields(media, repMedia, [.. fieldNames]); break;
                case (Rom rom, Rom repRom): ReplaceFields(rom, repRom, [.. fieldNames]); break;
            }

            #endregion
        }

        /// <summary>
        /// Replace fields with given values
        /// </summary>
        /// <param name="disk">Disk to remove replace fields in</param>
        /// <param name="newItem">Disk to pull new information from</param>
        /// <param name="datItemFields">List of fields representing what should be updated</param>
        private static void ReplaceFields(Disk disk, Disk newItem, List<string> datItemFields)
        {
            if (datItemFields.Contains(Models.Metadata.Disk.MD5Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Disk.MD5Key)))
                    disk.SetFieldValue<string?>(Models.Metadata.Disk.MD5Key, newItem.GetStringFieldValue(Models.Metadata.Disk.MD5Key));
            }

            if (datItemFields.Contains(Models.Metadata.Disk.SHA1Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Disk.SHA1Key)))
                    disk.SetFieldValue<string?>(Models.Metadata.Disk.SHA1Key, newItem.GetStringFieldValue(Models.Metadata.Disk.SHA1Key));
            }
        }

        /// <summary>
        /// Replace fields with given values
        /// </summary>
        /// <param name="file">File to remove replace fields in</param>
        /// <param name="newItem">File to pull new information from</param>
        /// <param name="datItemFields">List of fields representing what should be updated</param>
        private static void ReplaceFields(DatItems.Formats.File file, DatItems.Formats.File newItem, List<string> datItemFields)
        {
            if (datItemFields.Contains(Models.Metadata.Rom.CRCKey))
            {
                if (!string.IsNullOrEmpty(newItem.CRC))
                    file.CRC = newItem.CRC;
            }

            if (datItemFields.Contains(Models.Metadata.Rom.MD5Key))
            {
                if (!string.IsNullOrEmpty(newItem.MD5))
                    file.MD5 = newItem.MD5;
            }

            if (datItemFields.Contains(Models.Metadata.Rom.SHA1Key))
            {
                if (!string.IsNullOrEmpty(newItem.SHA1))
                    file.SHA1 = newItem.SHA1;
            }

            if (datItemFields.Contains(Models.Metadata.Rom.SHA256Key))
            {
                if (!string.IsNullOrEmpty(newItem.SHA256))
                    file.SHA256 = newItem.SHA256;
            }
        }

        /// <summary>
        /// Replace fields with given values
        /// </summary>
        /// <param name="media">Media to remove replace fields in</param>
        /// <param name="newItem">Media to pull new information from</param>
        /// <param name="datItemFields">List of fields representing what should be updated</param>
        private static void ReplaceFields(Media media, Media newItem, List<string> datItemFields)
        {
            if (datItemFields.Contains(Models.Metadata.Media.MD5Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Media.MD5Key)))
                    media.SetFieldValue<string?>(Models.Metadata.Media.MD5Key, newItem.GetStringFieldValue(Models.Metadata.Media.MD5Key));
            }

            if (datItemFields.Contains(Models.Metadata.Media.SHA1Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Media.SHA1Key)))
                    media.SetFieldValue<string?>(Models.Metadata.Media.SHA1Key, newItem.GetStringFieldValue(Models.Metadata.Media.SHA1Key));
            }

            if (datItemFields.Contains(Models.Metadata.Media.SHA256Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Media.SHA256Key)))
                    media.SetFieldValue<string?>(Models.Metadata.Media.SHA256Key, newItem.GetStringFieldValue(Models.Metadata.Media.SHA256Key));
            }

            if (datItemFields.Contains(Models.Metadata.Media.SpamSumKey))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Media.SpamSumKey)))
                    media.SetFieldValue<string?>(Models.Metadata.Media.SpamSumKey, newItem.GetStringFieldValue(Models.Metadata.Media.SpamSumKey));
            }
        }

        /// <summary>
        /// Replace fields with given values
        /// </summary>
        /// <param name="rom">Rom to remove replace fields in</param>
        /// <param name="newItem">Rom to pull new information from</param>
        /// <param name="datItemFields">List of fields representing what should be updated</param>
        private static void ReplaceFields(Rom rom, Rom newItem, List<string> datItemFields)
        {
            if (datItemFields.Contains(Models.Metadata.Rom.CRCKey))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.CRCKey)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.CRCKey, newItem.GetStringFieldValue(Models.Metadata.Rom.CRCKey));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.MD2Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.MD2Key)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.MD2Key, newItem.GetStringFieldValue(Models.Metadata.Rom.MD2Key));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.MD4Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.MD4Key)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.MD4Key, newItem.GetStringFieldValue(Models.Metadata.Rom.MD4Key));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.MD5Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.MD5Key)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.MD5Key, newItem.GetStringFieldValue(Models.Metadata.Rom.MD5Key));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.RIPEMD128Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.RIPEMD128Key)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.RIPEMD128Key, newItem.GetStringFieldValue(Models.Metadata.Rom.RIPEMD128Key));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.RIPEMD160Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.RIPEMD160Key)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.RIPEMD160Key, newItem.GetStringFieldValue(Models.Metadata.Rom.RIPEMD160Key));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.SHA1Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.SHA1Key)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA1Key, newItem.GetStringFieldValue(Models.Metadata.Rom.SHA1Key));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.SHA256Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.SHA256Key)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA256Key, newItem.GetStringFieldValue(Models.Metadata.Rom.SHA256Key));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.SHA384Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.SHA384Key)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA384Key, newItem.GetStringFieldValue(Models.Metadata.Rom.SHA384Key));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.SHA512Key))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.SHA512Key)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA512Key, newItem.GetStringFieldValue(Models.Metadata.Rom.SHA512Key));
            }

            if (datItemFields.Contains(Models.Metadata.Rom.SpamSumKey))
            {
                if (!string.IsNullOrEmpty(newItem.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey)))
                    rom.SetFieldValue<string?>(Models.Metadata.Rom.SpamSumKey, newItem.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey));
            }
        }

        #endregion
    }
}
