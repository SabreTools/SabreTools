using System;
using System.Collections.Generic;
using System.Linq;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
using SabreTools.Core;
#endif
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles
{
    public partial class DatFile
    {
        #region Splitting

        /// <summary>
        /// Use cdevice_ref tags to get full non-merged sets and remove parenting tags
        /// </summary>
        /// <remarks>This is a destructive process and items will be removed</remarks>
        public void ApplyDeviceNonMerged()
        {
            _logger.User("Creating device non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            BucketBy(ItemKey.Machine, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddItemsFromDevices(false, false)) ;
            while (AddItemsFromDevices(true, false)) ;

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveMachineRelationshipTags();
        }

        /// <summary>
        /// Use cloneof tags to create merged sets and remove the tags plus deduplicating if tags don't catch everything
        /// </summary>
        /// <remarks>This is a destructive process and items will be removed</remarks>
        public void ApplyFullyMerged()
        {
            _logger.User("Creating fully merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            BucketBy(ItemKey.Machine, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddItemsFromChildren(true, false);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveItemsFromRomOfChild();

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveMachineRelationshipTags();
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags plus using the device_ref tags to get full sets
        /// </summary>
        /// <remarks>This is a destructive process and items will be removed</remarks>
        public void ApplyFullyNonMerged()
        {
            _logger.User("Creating fully non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            BucketBy(ItemKey.Machine, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddItemsFromDevices(true, true)) ;
            AddItemsFromDevices(false, true);
            AddItemsFromCloneOfParent();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            AddItemsFromRomOfParent();

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveMachineRelationshipTags();
        }

        /// <summary>
        /// Use cloneof tags to create merged sets and remove the tags
        /// </summary>
        /// <remarks>This is a destructive process and items will be removed</remarks>
        public void ApplyMerged()
        {
            _logger.User("Creating merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            BucketBy(ItemKey.Machine, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddItemsFromChildren(true, true);

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveItemsFromRomOfChild();

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveMachineRelationshipTags();
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags
        /// </summary>
        /// <remarks>This is a destructive process and items will be removed</remarks>
        public void ApplyNonMerged()
        {
            _logger.User("Creating non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            BucketBy(ItemKey.Machine, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddItemsFromCloneOfParent();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveItemsFromRomOfChild();

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveMachineRelationshipTags();
        }

        /// <summary>
        /// Use cloneof and romof tags to create split sets and remove the tags
        /// </summary>
        /// <remarks>This is a destructive process and items will be removed</remarks>
        public void ApplySplit()
        {
            _logger.User("Creating split sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            BucketBy(ItemKey.Machine, norename: true);

            // Now we want to loop through all of the games and set the correct information
            RemoveItemsFromCloneOfChild();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveItemsFromRomOfChild();

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveMachineRelationshipTags();
        }

        #endregion

        #region Splitting Steps

        /// <summary>
        /// Use cloneof tags to add items to the parents, removing the child sets in the process
        /// </summary>
        /// <param name="subfolder">True to add DatItems to subfolder of parent (not including Disk), false otherwise</param>
        /// <param name="skipDedup">True to skip checking for duplicate ROMs in parent, false otherwise</param>
        /// <remarks>Assumes items are bucketed by <see cref="ItemKey.Machine"/></remarks>
        internal void AddItemsFromChildren(bool subfolder, bool skipDedup)
        {
            AddItemsFromChildrenImpl(subfolder, skipDedup);
            AddItemsFromChildrenImplDB(subfolder, skipDedup);
        }

        /// <summary>
        /// Use cloneof tags to add items to the children, setting the new romof tag in the process
        /// </summary>
        /// <remarks>Assumes items are bucketed by <see cref="ItemKey.Machine"/></remarks>
        internal void AddItemsFromCloneOfParent()
        {
            AddItemsFromCloneOfParentImpl();
            AddItemsFromCloneOfParentImplDB();
        }

        /// <summary>
        /// Use device_ref and optionally slotoption tags to add items to the children
        /// </summary>
        /// <param name="deviceOnly">True if only child device sets are touched, false for non-device sets</param>
        /// <param name="useSlotOptions">True if slotoptions tags are used as well, false otherwise</param>
        /// <remarks>Assumes items are bucketed by <see cref="ItemKey.Machine"/></remarks>
        internal bool AddItemsFromDevices(bool deviceOnly, bool useSlotOptions)
        {
            bool foundnew = AddItemsFromDevicesImpl(deviceOnly, useSlotOptions);
            foundnew |= AddItemsFromDevicesImplDB(deviceOnly, useSlotOptions);
            return foundnew;
        }

        /// <summary>
        /// Use romof tags to add items to the children
        /// </summary>
        /// <remarks>Assumes items are bucketed by <see cref="ItemKey.Machine"/></remarks>
        internal void AddItemsFromRomOfParent()
        {
            AddItemsFromRomOfParentImpl();
            AddItemsFromRomOfParentImplDB();
        }

        /// <summary>
        /// Remove all BIOS and device sets
        /// </summary>
        /// <remarks>Assumes items are bucketed by <see cref="ItemKey.Machine"/></remarks>
        internal void RemoveBiosAndDeviceSets()
        {
            RemoveBiosAndDeviceSetsImpl();
            RemoveBiosAndDeviceSetsImplDB();
        }

        /// <summary>
        /// Use cloneof tags to remove items from the children
        /// </summary>
        /// <remarks>Assumes items are bucketed by <see cref="ItemKey.Machine"/></remarks>
        internal void RemoveItemsFromCloneOfChild()
        {
            RemoveItemsFromCloneOfChildImpl();
            RemoveItemsFromCloneOfChildImplDB();
        }

        /// <summary>
        /// Use romof tags to remove bios items from children
        /// </summary>
        /// <remarks>Assumes items are bucketed by <see cref="ItemKey.Machine"/></remarks>
        internal void RemoveItemsFromRomOfChild()
        {
            RemoveItemsFromRomOfChildImpl();
            RemoveItemsFromRomOfChildImplDB();
        }

        /// <summary>
        /// Remove all romof and cloneof tags from all machines
        /// </summary>
        /// <remarks>Assumes items are bucketed by <see cref="ItemKey.Machine"/></remarks>
        internal void RemoveMachineRelationshipTags()
        {
            RemoveMachineRelationshipTagsImpl();
            RemoveMachineRelationshipTagsImplDB();
        }

        #endregion

        #region Splitting Implementations

        /// <summary>
        /// Use cloneof tags to add items to the parents, removing the child sets in the process
        /// </summary>
        /// <param name="subfolder">True to add DatItems to subfolder of parent (not including Disk), false otherwise</param>
        /// <param name="skipDedup">True to skip checking for duplicate ROMs in parent, false otherwise</param>
        /// <remarks>
        /// Applies to <see cref="Items"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void AddItemsFromChildrenImpl(bool subfolder, bool skipDedup)
        {
            string[] buckets = [.. Items.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                List<DatItem> items = GetItemsForBucket(bucket);
                if (items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetMachine();
                if (machine == null)
                    continue;

                // Get the cloneof parent items
                string? cloneOf = machine.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey);
                List<DatItem> parentItems = GetItemsForBucket(cloneOf);
                if (cloneOf == null)
                    continue;

                // Otherwise, move the items from the current game to a subfolder of the parent game
                DatItem copyFrom;
                if (parentItems.Count == 0)
                {
                    copyFrom = new Rom();
                    copyFrom.GetMachine()!.SetName(cloneOf);
                    copyFrom.GetMachine()!.SetFieldValue<string?>(Models.Metadata.Machine.DescriptionKey, cloneOf);
                }
                else
                {
                    copyFrom = parentItems[0];
                }

                items = GetItemsForBucket(bucket);
                foreach (DatItem item in items)
                {
                    // Special disk handling
                    if (item is Disk disk)
                    {
                        string? mergeTag = disk.GetStringFieldValue(Models.Metadata.Disk.MergeKey);

                        // If the merge tag exists and the parent already contains it, skip
                        if (mergeTag != null && GetItemsForBucket(cloneOf)
                            .FindAll(i => i is Disk)
                            .ConvertAll(i => (i as Disk)!.GetName())
                            .Contains(mergeTag))
                        {
                            continue;
                        }

                        // If the merge tag exists but the parent doesn't contain it, add to parent
                        else if (mergeTag != null && !GetItemsForBucket(cloneOf)
                            .FindAll(i => i is Disk)
                            .ConvertAll(i => (i as Disk)!.GetName())
                            .Contains(mergeTag))
                        {
                            disk.CopyMachineInformation(copyFrom);
                            AddItem(disk, statsOnly: false);
                        }

                        // If there is no merge tag, add to parent
                        else if (mergeTag == null && !GetItemsForBucket(cloneOf)
                            .FindAll(i => i is Disk)
                            .ConvertAll(i => (i as Disk)!.GetName())
                            .Contains(disk.GetName()))
                        {
                            disk.CopyMachineInformation(copyFrom);
                            AddItem(disk, statsOnly: false);
                        }
                    }

                    // Special rom handling
                    else if (item is Rom rom)
                    {
                        string? mergeTag = rom.GetStringFieldValue(Models.Metadata.Rom.MergeKey);

                        // If the merge tag exists and the parent already contains it, skip
                        if (mergeTag != null && GetItemsForBucket(cloneOf)
                            .FindAll(i => i is Rom)
                            .ConvertAll(i => (i as Rom)!.GetName())
                            .Contains(mergeTag))
                        {
                            continue;
                        }

                        // If the merge tag exists but the parent doesn't contain it, add to subfolder of parent
                        else if (mergeTag != null && !GetItemsForBucket(cloneOf)
                            .FindAll(i => i is Rom)
                            .ConvertAll(i => (i as Rom)!.GetName())
                            .Contains(mergeTag))
                        {
                            if (subfolder)
                                rom.SetName($"{rom.GetMachine()!.GetName()}\\{rom.GetName()}");

                            rom.CopyMachineInformation(copyFrom);
                            AddItem(rom, statsOnly: false);
                        }

                        // If the parent doesn't already contain this item, add to subfolder of parent
                        else if (!GetItemsForBucket(cloneOf).Exists(i => i.Equals(item)) || skipDedup)
                        {
                            if (subfolder)
                                rom.SetName($"{item.GetMachine()!.GetName()}\\{rom.GetName()}");

                            rom.CopyMachineInformation(copyFrom);
                            AddItem(rom, statsOnly: false);
                        }
                    }

                    // All other that would be missing to subfolder of parent
                    else if (!GetItemsForBucket(cloneOf).Exists(i => i.Equals(item)))
                    {
                        if (subfolder)
                            item.SetName($"{item.GetMachine()!.GetName()}\\{item.GetName()}");

                        item.CopyMachineInformation(copyFrom);
                        AddItem(item, statsOnly: false);
                    }
                }

                // Then, remove the old game so it's not picked up by the writer
                RemoveBucket(bucket);
            }
        }

        /// <summary>
        /// Use cloneof tags to add items to the parents, removing the child sets in the process
        /// </summary>
        /// <param name="subfolder">True to add DatItems to subfolder of parent (not including Disk), false otherwise</param>
        /// <param name="skipDedup">True to skip checking for duplicate ROMs in parent, false otherwise</param>
        /// <remarks>
        /// Applies to <see cref="ItemsDB"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void AddItemsFromChildrenImplDB(bool subfolder, bool skipDedup)
        {
            string[] buckets = [.. ItemsDB.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                Dictionary<long, DatItem> items = GetItemsForBucketDB(bucket);
                if (items.Count == 0)
                    continue;

                // Get the machine for the first item
                var machine = GetMachineForItemDB(items.First().Key);
                if (machine.Value == null)
                    continue;

                // Get the clone parent
                string? cloneOf = machine.Value.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey);
                if (string.IsNullOrEmpty(cloneOf))
                    continue;

                // Get the clone parent machine
                var cloneOfMachine = ItemsDB.GetMachine(cloneOf);
                if (cloneOfMachine.Value == null)
                    continue;

                items = GetItemsForBucketDB(bucket);
                foreach (var item in items)
                {
                    // Get the source for the current item
                    var source = GetSourceForItemDB(item.Key);

                    // Get the parent items and current machine name
                    Dictionary<long, DatItem> parentItems = GetItemsForBucketDB(cloneOf);
                    if (parentItems.Count == 0)
                        continue;

                    string? machineName = GetMachineForItemDB(item.Key).Value?
                        .GetName();

                    // Special disk handling
                    if (item.Value is Disk disk)
                    {
                        string? mergeTag = disk.GetStringFieldValue(Models.Metadata.Disk.MergeKey);

                        // If the merge tag exists and the parent already contains it, skip
                        if (mergeTag != null && GetItemsForBucketDB(cloneOf).Values
                            .Where(i => i is Disk)
                            .Select(i => (i as Disk)!.GetName())
                            .Contains(mergeTag))
                        {
                            continue;
                        }

                        // If the merge tag exists but the parent doesn't contain it, add to parent
                        else if (mergeTag != null && !GetItemsForBucketDB(cloneOf).Values
                            .Where(i => i is Disk)
                            .Select(i => (i as Disk)!.GetName())
                            .Contains(mergeTag))
                        {
                            ItemsDB.RemapDatItemToMachine(item.Key, cloneOfMachine.Key);
                            ItemsDB.AddItem(item.Value, cloneOfMachine.Key, source.Key);
                        }

                        // If there is no merge tag, add to parent
                        else if (mergeTag == null && !GetItemsForBucketDB(cloneOf).Values
                            .Where(i => i is Disk)
                            .Select(i => (i as Disk)!.GetName())
                            .Contains(disk.GetName()))
                        {
                            ItemsDB.RemapDatItemToMachine(item.Key, cloneOfMachine.Key);
                            ItemsDB.AddItem(item.Value, cloneOfMachine.Key, source.Key);
                        }
                    }

                    // Special rom handling
                    else if (item.Value is Rom rom)
                    {
                        string? mergeTag = rom.GetStringFieldValue(Models.Metadata.Rom.MergeKey);

                        // If the merge tag exists and the parent already contains it, skip
                        if (mergeTag != null && GetItemsForBucketDB(cloneOf).Values
                            .Where(i => i is Rom)
                            .Select(i => (i as Rom)!.GetName())
                            .Contains(mergeTag))
                        {
                            continue;
                        }

                        // If the merge tag exists but the parent doesn't contain it, add to subfolder of parent
                        else if (mergeTag != null && !GetItemsForBucketDB(cloneOf).Values
                            .Where(i => i is Rom)
                            .Select(i => (i as Rom)!.GetName())
                            .Contains(mergeTag))
                        {
                            if (subfolder)
                                rom.SetName($"{machineName}\\{rom.GetName()}");

                            ItemsDB.RemapDatItemToMachine(item.Key, machineIndex: cloneOfMachine.Key);
                            ItemsDB.AddItem(item.Value, cloneOfMachine.Key, source.Key);
                        }

                        // If the parent doesn't already contain this item, add to subfolder of parent
                        else if (!GetItemsForBucketDB(cloneOf).Values.Contains(item.Value) || skipDedup)
                        {
                            if (subfolder)
                                rom.SetName($"{machineName}\\{rom.GetName()}");

                            ItemsDB.RemapDatItemToMachine(item.Key, cloneOfMachine.Key);
                            ItemsDB.AddItem(item.Value, cloneOfMachine.Key, source.Key);
                        }
                    }

                    // All other that would be missing to subfolder of parent
                    else if (!GetItemsForBucketDB(cloneOf).Values.Contains(item.Value))
                    {
                        if (subfolder)
                            item.Value.SetName($"{machineName}\\{item.Value.GetName()}");

                        ItemsDB.RemapDatItemToMachine(item.Key, cloneOfMachine.Key);
                        ItemsDB.AddItem(item.Value, cloneOfMachine.Key, source.Key);
                    }

                    // Remove the current item
                    RemoveItemDB(item.Key);
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to add items to the children, setting the new romof tag in the process
        /// </summary>
        /// <remarks>
        /// Applies to <see cref="Items"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void AddItemsFromCloneOfParentImpl()
        {
            string[] buckets = [.. Items.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                List<DatItem> items = GetItemsForBucket(bucket);
                if (items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetMachine();
                if (machine == null)
                    continue;

                // Get the cloneof parent items
                string? cloneOf = machine.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey);
                List<DatItem> parentItems = GetItemsForBucket(cloneOf);
                if (parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = items[0];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (!items.Exists(i => string.Equals(i.GetName(), datItem.GetName(), StringComparison.OrdinalIgnoreCase))
                        && !items.Exists(i => i.Equals(datItem)))
                    {
                        AddItem(datItem, statsOnly: false);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the items
                items = GetItemsForBucket(bucket);
                string? romof = GetItemsForBucket(cloneOf)[0].GetMachine()!.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                foreach (DatItem item in items)
                {
                    item.GetMachine()!.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, romof);
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to add items to the children, setting the new romof tag in the process
        /// </summary>
        /// <remarks>
        /// Applies to <see cref="ItemsDB"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void AddItemsFromCloneOfParentImplDB()
        {
            string[] buckets = [.. ItemsDB.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                Dictionary<long, DatItem> items = GetItemsForBucketDB(bucket);
                if (items.Count == 0)
                    continue;

                // Get the source for the first item
                var source = GetSourceForItemDB(items.First().Key);

                // Get the machine for the first item in the list
                var machine = GetMachineForItemDB(items.First().Key);
                if (machine.Value == null)
                    continue;

                // Get the clone parent
                string? cloneOf = machine.Value.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey);
                if (string.IsNullOrEmpty(cloneOf))
                    continue;

                // If the parent doesn't have any items, we want to continue
                Dictionary<long, DatItem> parentItems = GetItemsForBucketDB(cloneOf);
                if (parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                foreach (var item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Value.Clone();
                    if (items.Values.Any(i => i.GetName()?.ToLowerInvariant() == datItem.GetName()?.ToLowerInvariant())
                        && items.Values.Any(i => i == datItem))
                    {
                        ItemsDB.AddItem(datItem, machine.Key, source.Key);
                    }
                }

                // Get the parent machine
                var parentMachine = GetMachineForItemDB(GetItemsForBucketDB(cloneOf).First().Key);
                if (parentMachine.Value == null)
                    continue;

                // Now we want to get the parent romof tag and put it in each of the items
                items = GetItemsForBucketDB(bucket);
                string? romof = parentMachine.Value.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                foreach (var key in items.Keys)
                {
                    var itemMachine = GetMachineForItemDB(key);
                    if (itemMachine.Value == null)
                        continue;

                    // TODO: Remove merge tags here
                    itemMachine.Value.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, romof);
                }
            }
        }

        /// <summary>
        /// Use device_ref and optionally slotoption tags to add items to the children
        /// </summary>
        /// <param name="deviceOnly">True if only child device sets are touched, false for non-device sets</param>
        /// <param name="useSlotOptions">True if slotoptions tags are used as well, false otherwise</param>
        /// <returns>True if any items were processed, false otherwise</returns>
        /// <remarks>
        /// Applies to <see cref="Items"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private bool AddItemsFromDevicesImpl(bool deviceOnly, bool useSlotOptions)
        {
            bool foundnew = false;

            string[] buckets = [.. Items.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket doesn't have items
                List<DatItem> datItems = GetItemsForBucket(bucket);
                if (datItems.Count == 0)
                    continue;

                // If the machine (is/is not) a device, we want to continue
                if (deviceOnly ^ (datItems[0].GetMachine()!.GetBoolFieldValue(Models.Metadata.Machine.IsDeviceKey) == true))
                    continue;

                // Get the first item from the bucket
                DatItem copyFrom = datItems[0];

                // Get all device reference names from the current machine
                HashSet<string?> deviceReferences = [];
                deviceReferences.UnionWith(datItems
                    .FindAll(i => i is DeviceRef)
                    .ConvertAll(i => i as DeviceRef)
                    .ConvertAll(dr => dr!.GetName()));

                // Get all slot option names from the current machine
                HashSet<string?> slotOptions = [];
                slotOptions.UnionWith(datItems
                   .FindAll(i => i is Slot)
                   .ConvertAll(i => i as Slot)
                   .FindAll(s => s!.SlotOptionsSpecified)
                   .SelectMany(s => s!.GetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey)!)
                   .Select(so => so.GetStringFieldValue(Models.Metadata.SlotOption.DevNameKey)));

                // If we're checking device references
                if (deviceReferences.Count > 0)
                {
                    // Loop through all names and check the corresponding machines
                    var newDeviceReferences = new HashSet<string>();
                    foreach (string? deviceReference in deviceReferences)
                    {
                        // Add to the list of new device reference names
                        List<DatItem> devItems = GetItemsForBucket(deviceReference);
                        if (devItems.Count == 0)
                            continue;

                        newDeviceReferences.UnionWith(devItems
                            .FindAll(i => i is DeviceRef)
                            .ConvertAll(i => (i as DeviceRef)!.GetName()!));

                        // Set new machine information and add to the current machine
                        foreach (DatItem item in devItems)
                        {
                            // If the parent machine doesn't already contain this item, add it
                            if (!datItems.Exists(i => i.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) == item.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) && i.GetName() == item.GetName()))
                            {
                                // Set that we found new items
                                foundnew = true;

                                // Clone the item and then add it
                                DatItem datItem = (DatItem)item.Clone();
                                datItem.CopyMachineInformation(copyFrom);
                                datItems.Add(datItem);
                                AddItem(datItem, statsOnly: false);
                            }
                        }
                    }

                    // Now that every device reference is accounted for, add the new list of device references, if they don't already exist
                    foreach (string deviceReference in newDeviceReferences)
                    {
                        if (!deviceReferences.Contains(deviceReference))
                        {
                            deviceReferences.Add(deviceReference);
                            var deviceRef = new DeviceRef();
                            deviceRef.SetName(deviceReference);
                            deviceRef.CopyMachineInformation(copyFrom);
                            Items.AddItem(deviceRef, statsOnly: false);
                        }
                    }
                }

                // If we're checking slotoptions
                if (useSlotOptions && slotOptions.Count > 0)
                {
                    // Loop through all names and check the corresponding machines
                    var newSlotOptions = new HashSet<string>();
                    foreach (string? slotOption in slotOptions)
                    {
                        // Add to the list of new slot option names
                        List<DatItem> slotItems = GetItemsForBucket(slotOption);
                        if (slotItems.Count == 0)
                            continue;

                        newSlotOptions.UnionWith(slotItems
                            .FindAll(i => i is Slot)
                            .FindAll(s => (s as Slot)!.SlotOptionsSpecified)
                            .SelectMany(s => (s as Slot)!.GetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey)!)
                            .Select(o => o.GetStringFieldValue(Models.Metadata.SlotOption.DevNameKey)!));

                        // Set new machine information and add to the current machine
                        foreach (DatItem item in slotItems)
                        {
                            // If the parent machine doesn't already contain this item, add it
                            if (!datItems.Exists(i => i.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) == item.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) && i.GetName() == item.GetName()))
                            {
                                // Set that we found new items
                                foundnew = true;

                                // Clone the item and then add it
                                DatItem datItem = (DatItem)item.Clone();
                                datItem.CopyMachineInformation(copyFrom);
                                datItems.Add(datItem);
                                AddItem(datItem, statsOnly: false);
                            }
                        }
                    }

                    // Now that every device is accounted for, add the new list of slot options, if they don't already exist
                    foreach (string slotOption in newSlotOptions)
                    {
                        if (!slotOptions.Contains(slotOption))
                        {
                            slotOptions.Add(slotOption);
                            var slotOptionItem = new SlotOption();
                            slotOptionItem.SetFieldValue<string?>(Models.Metadata.SlotOption.DevNameKey, slotOption);
                            slotOptionItem.CopyMachineInformation(copyFrom);

                            var slotItem = new Slot();
                            slotItem.SetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey, [slotOptionItem]);
                            slotItem.CopyMachineInformation(copyFrom);

                            Items.AddItem(slotItem, statsOnly: false);
                        }
                    }
                }
            }

            return foundnew;
        }

        /// <summary>
        /// Use device_ref and optionally slotoption tags to add items to the children
        /// </summary>
        /// <param name="deviceOnly">True if only child device sets are touched, false for non-device sets</param>
        /// <param name="useSlotOptions">True if slotoptions tags are used as well, false otherwise</param>
        /// <returns>True if any items were processed, false otherwise</returns>
        /// <remarks>
        /// Applies to <see cref="ItemsDB"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private bool AddItemsFromDevicesImplDB(bool deviceOnly, bool useSlotOptions)
        {
            bool foundnew = false;

            string[] buckets = [.. ItemsDB.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                Dictionary<long, DatItem> items = GetItemsForBucketDB(bucket);
                if (items.Count == 0)
                    continue;

                // Get the source for the first item
                var source = GetSourceForItemDB(items.First().Key);

                // Get the machine for the first item
                var machine = GetMachineForItemDB(items.First().Key);
                if (machine.Value == null)
                    continue;

                // If the machine (is/is not) a device, we want to continue
                if (deviceOnly ^ (machine.Value.GetBoolFieldValue(Models.Metadata.Machine.IsDeviceKey) == true))
                    continue;

                // Get all device reference names from the current machine
                List<string?> deviceReferences = items.Values
                    .Where(i => i is DeviceRef)
                    .Select(i => i as DeviceRef)
                    .Select(dr => dr!.GetName())
                    .Distinct()
                    .ToList();

                // Get all slot option names from the current machine
                List<string?> slotOptions = items.Values
                    .Where(i => i is Slot)
                    .Select(i => i as Slot)
                    .Where(s => s!.SlotOptionsSpecified)
                    .SelectMany(s => s!.GetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey)!)
                    .Select(so => so.GetStringFieldValue(Models.Metadata.SlotOption.DevNameKey))
                    .Distinct()
                    .ToList();

                // If we're checking device references
                if (deviceReferences.Count > 0)
                {
                    // Loop through all names and check the corresponding machines
                    var newDeviceReferences = new HashSet<string>();
                    foreach (string? deviceReference in deviceReferences)
                    {
                        // If the device reference is invalid
                        if (deviceReference == null)
                            continue;

                        // If the machine doesn't exist then we continue
                        Dictionary<long, DatItem> devItems = GetItemsForBucketDB(deviceReference);
                        if (devItems.Count == 0)
                            continue;

                        // Add to the list of new device reference names
                        newDeviceReferences.UnionWith(devItems.Values
                            .Where(i => i is DeviceRef)
                            .Select(i => (i as DeviceRef)!.GetName()!));

                        // Set new machine information and add to the current machine
                        var copyFrom = GetMachineForItemDB(items.First().Key);
                        if (copyFrom.Value == null)
                            continue;

                        foreach (var item in devItems.Values)
                        {
                            // If the parent machine doesn't already contain this item, add it
                            if (!items.Values.Any(i => i.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) == item.GetStringFieldValue(Models.Metadata.DatItem.TypeKey)
                                && i.GetName() == item.GetName()))
                            {
                                // Set that we found new items
                                foundnew = true;

                                // Clone the item and then add it
                                DatItem datItem = (DatItem)item.Clone();
                                ItemsDB.AddItem(datItem, machine.Key, source.Key);
                            }
                        }
                    }

                    // Now that every device reference is accounted for, add the new list of device references, if they don't already exist
                    foreach (string deviceReference in newDeviceReferences)
                    {
                        if (!deviceReferences.Contains(deviceReference))
                        {
                            var deviceRef = new DeviceRef();
                            deviceRef.SetName(deviceReference);
                            ItemsDB.AddItem(deviceRef, machine.Key, source.Key);
                        }
                    }
                }

                // If we're checking slotoptions
                if (useSlotOptions && slotOptions.Count > 0)
                {
                    // Loop through all names and check the corresponding machines
                    var newSlotOptions = new HashSet<string>();
                    foreach (string? slotOption in slotOptions)
                    {
                        // If the slot option is invalid
                        if (slotOption == null)
                            continue;

                        // If the machine doesn't exist then we continue
                        Dictionary<long, DatItem> slotItems = GetItemsForBucketDB(slotOption);
                        if (slotItems.Count == 0)
                            continue;

                        // Add to the list of new slot option names
                        newSlotOptions.UnionWith(slotItems.Values
                            .Where(i => i is Slot)
                            .Where(s => (s as Slot)!.SlotOptionsSpecified)
                            .SelectMany(s => (s as Slot)!.GetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey)!)
                            .Select(o => o.GetStringFieldValue(Models.Metadata.SlotOption.DevNameKey)!));

                        // Set new machine information and add to the current machine
                        var copyFrom = GetMachineForItemDB(GetItemsForBucketDB(bucket).First().Key);
                        if (copyFrom.Value == null)
                            continue;

                        foreach (var item in slotItems.Values)
                        {
                            // If the parent machine doesn't already contain this item, add it
                            if (!items.Values.Any(i => i.GetStringFieldValue(Models.Metadata.DatItem.TypeKey) == item.GetStringFieldValue(Models.Metadata.DatItem.TypeKey)
                                && i.GetName() == item.GetName()))
                            {
                                // Set that we found new items
                                foundnew = true;

                                // Clone the item and then add it
                                DatItem datItem = (DatItem)item.Clone();
                                ItemsDB.AddItem(datItem, machine.Key, source.Key);
                            }
                        }
                    }

                    // Now that every device is accounted for, add the new list of slot options, if they don't already exist
                    foreach (string slotOption in newSlotOptions)
                    {
                        if (!slotOptions.Contains(slotOption))
                        {
                            var slotOptionItem = new SlotOption();
                            slotOptionItem.SetFieldValue<string?>(Models.Metadata.SlotOption.DevNameKey, slotOption);

                            var slotItem = new Slot();
                            slotItem.SetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey, [slotOptionItem]);

                            ItemsDB.AddItem(slotItem, machine.Key, source.Key);
                        }
                    }
                }
            }

            return foundnew;
        }

        /// <summary>
        /// Use romof tags to add items to the children
        /// </summary>
        /// <remarks>
        /// Applies to <see cref="Items"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void AddItemsFromRomOfParentImpl()
        {
            string[] buckets = [.. Items.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                List<DatItem> items = GetItemsForBucket(bucket);
                if (items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetMachine();
                if (machine == null)
                    continue;

                // Get the romof parent items
                string? romOf = machine.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                List<DatItem> parentItems = GetItemsForBucket(romOf);
                if (parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = items[0];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (!items.Exists(i => i.GetName() == datItem.GetName()) && !items.Exists(i => i.Equals(datItem)))
                        AddItem(datItem, statsOnly: false);
                }
            }
        }

        /// <summary>
        /// Use romof tags to add items to the children
        /// </summary>
        /// <remarks>
        /// Applies to <see cref="ItemsDB"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void AddItemsFromRomOfParentImplDB()
        {
            string[] buckets = [.. ItemsDB.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                Dictionary<long, DatItem> items = GetItemsForBucketDB(bucket);
                if (items.Count == 0)
                    continue;

                // Get the source for the first item
                var source = GetSourceForItemDB(items.First().Key);

                // Get the machine for the first item
                var machine = GetMachineForItemDB(items.First().Key);
                if (machine.Value == null)
                    continue;

                // Get the romof parent items
                string? romOf = machine.Value.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                Dictionary<long, DatItem> parentItems = GetItemsForBucketDB(romOf);
                if (parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                foreach (var item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Value.Clone();
                    if (items.Any(i => i.Value.GetName() == datItem.GetName())
                        && items.Any(i => i.Value == datItem))
                    {
                        ItemsDB.AddItem(datItem, machine.Key, source.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Remove all BIOS and device sets
        /// </summary>
        /// <remarks>
        /// Applies to <see cref="Items"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void RemoveBiosAndDeviceSetsImpl()
        {
            string[] buckets = [.. Items.SortedKeys];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(buckets, Globals.ParallelOptions, bucket =>
#elif NET40_OR_GREATER
            Parallel.ForEach(buckets, bucket =>
#else
            foreach (string bucket in buckets)
#endif
            {
                // If the bucket has no items in it
                List<DatItem> items = GetItemsForBucket(bucket);
                if (items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                // Get the machine
                var machine = items[0].GetMachine();
                if (machine == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                // Remove flagged items
                if ((machine.GetBoolFieldValue(Models.Metadata.Machine.IsBiosKey) == true)
                    || (machine.GetBoolFieldValue(Models.Metadata.Machine.IsDeviceKey) == true))
                {
                    RemoveBucket(bucket);
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Remove all BIOS and device sets
        /// </summary>
        /// <remarks>
        /// Applies to <see cref="ItemsDB"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void RemoveBiosAndDeviceSetsImplDB()
        {
            string[] buckets = [.. ItemsDB.SortedKeys];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(buckets, Globals.ParallelOptions, bucket =>
#elif NET40_OR_GREATER
            Parallel.ForEach(buckets, bucket =>
#else
            foreach (string bucket in buckets)
#endif
            {
                // If the bucket has no items in it
                Dictionary<long, DatItem> items = GetItemsForBucketDB(bucket);
                if (items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                // Get the machine
                var machine = GetMachineForItemDB(items.First().Key);
                if (machine.Value == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                // Remove flagged items
                if ((machine.Value.GetBoolFieldValue(Models.Metadata.Machine.IsBiosKey) == true)
                    || (machine.Value.GetBoolFieldValue(Models.Metadata.Machine.IsDeviceKey) == true))
                {
                    foreach (var key in items.Keys)
                    {
                        RemoveItemDB(key);
                    }
                }

                // Remove the machine
                RemoveMachineDB(machine.Key);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Use cloneof tags to remove items from the children
        /// </summary>
        /// <remarks>
        /// Applies to <see cref="Items"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void RemoveItemsFromCloneOfChildImpl()
        {
            string[] buckets = [.. Items.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                List<DatItem> items = GetItemsForBucket(bucket);
                if (items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetMachine();
                if (machine == null)
                    continue;

                // Get the cloneof parent items
                string? cloneOf = machine.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey);
                List<DatItem> parentItems = GetItemsForBucket(cloneOf);
                if (parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we remove the parent items from the current game
                foreach (DatItem item in parentItems)
                {
                    while (true)
                    {
                        // Find the next index that matches the item
                        int index = items.FindIndex(i => i.Equals(item));
                        if (index < 0)
                            break;

                        // Remove the item from the local and internal lists
                        RemoveItem(bucket, items[index], index);
                        items.RemoveAt(index);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the remaining items
                items = GetItemsForBucket(bucket);
                string? romof = GetItemsForBucket(cloneOf)[0].GetMachine()!.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                foreach (DatItem item in items)
                {
                    item.GetMachine()!.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, romof);
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to remove items from the children
        /// </summary>
        /// <remarks>
        /// Applies to <see cref="ItemsDB"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void RemoveItemsFromCloneOfChildImplDB()
        {
            string[] buckets = [.. ItemsDB.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                Dictionary<long, DatItem> items = GetItemsForBucketDB(bucket);
                if (items.Count == 0)
                    continue;

                // Get the machine for the first item
                var machine = GetMachineForItemDB(items.First().Key);
                if (machine.Value == null)
                    continue;

                // Get the cloneof parent items
                string? cloneOf = machine.Value.GetStringFieldValue(Models.Metadata.Machine.CloneOfKey);
                Dictionary<long, DatItem> parentItems = GetItemsForBucketDB(cloneOf);
                if (parentItems == null || parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we remove the parent items from the current game
                foreach (var item in parentItems)
                {
                    var matchedItems = items.Where(i => i.Value.Equals(item.Value));
                    foreach (var match in matchedItems)
                    {
                        RemoveItemDB(match.Key);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the remaining items
                items = GetItemsForBucketDB(bucket);
                machine = GetMachineForItemDB(GetItemsForBucketDB(cloneOf).First().Key);
                if (machine.Value == null)
                    continue;

                string? romof = machine.Value.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                foreach (var item in items)
                {
                    machine = GetMachineForItemDB(item.Key);
                    if (machine.Value == null)
                        continue;

                    machine.Value.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, romof);
                }
            }
        }

        /// <summary>
        /// Use romof tags to remove items from children
        /// </summary>
        /// <remarks>
        /// Applies to <see cref="Items"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void RemoveItemsFromRomOfChildImpl()
        {
            string[] buckets = [.. Items.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                List<DatItem> items = GetItemsForBucket(bucket);
                if (items.Count == 0)
                    continue;

                // Get the machine
                var machine = items[0].GetMachine();
                if (machine == null)
                    continue;

                // Get the romof parent items
                string? romOf = machine.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                List<DatItem> parentItems = GetItemsForBucket(romOf);
                if (parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we remove the parent items from the current game
                foreach (DatItem item in parentItems)
                {
                    while (true)
                    {
                        // Find the next index that matches the item
                        int index = items.FindIndex(i => i.Equals(item));
                        if (index < 0)
                            break;

                        // Remove the item from the local and internal lists
                        RemoveItem(bucket, items[index], index);
                        items.RemoveAt(index);
                    }
                }
            }
        }

        /// <summary>
        /// Use romof tags to remove bios items from children
        /// </summary>
        /// <param name="bios">True if only child Bios sets are touched, false for non-bios sets</param>
        /// <remarks>
        /// Applies to <see cref="ItemsDB"/>.
        /// Assumes items are bucketed by <see cref="ItemKey.Machine"/>.
        /// </remarks>
        private void RemoveItemsFromRomOfChildImplDB()
        {
            string[] buckets = [.. ItemsDB.SortedKeys];
            foreach (string bucket in buckets)
            {
                // If the bucket has no items in it
                Dictionary<long, DatItem> items = GetItemsForBucketDB(bucket);
                if (items.Count == 0)
                    continue;

                // Get the machine for the item
                var machine = GetMachineForItemDB(items.First().Key);
                if (machine.Value == null)
                    continue;

                // Get the romof parent items
                string? romOf = machine.Value.GetStringFieldValue(Models.Metadata.Machine.RomOfKey);
                Dictionary<long, DatItem> parentItems = GetItemsForBucketDB(romOf);
                if (parentItems.Count == 0)
                    continue;

                // If the parent exists and has items, we remove the items that are in the parent from the current game
                foreach (var item in parentItems)
                {
                    var matchedItems = items.Where(i => i.Value.Equals(item.Value));
                    foreach (var match in matchedItems)
                    {
                        RemoveItemDB(match.Key);
                    }
                }
            }
        }

        /// <summary>
        /// Remove all romof and cloneof tags from all machines
        /// </summary>
        /// <remarks>Applies to <see cref="Items"/></remarks>
        private void RemoveMachineRelationshipTagsImpl()
        {
            string[] buckets = [.. Items.SortedKeys];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(buckets, Globals.ParallelOptions, bucket =>
#elif NET40_OR_GREATER
            Parallel.ForEach(buckets, bucket =>
#else
            foreach (string bucket in buckets)
#endif
            {
                // If the bucket has no items in it
                var items = GetItemsForBucket(bucket);
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                foreach (DatItem item in items)
                {
                    // Remove the merge tag
                    item.RemoveField(Models.Metadata.Rom.MergeKey);

                    // Get the machine
                    var machine = item.GetMachine();
                    if (machine == null)
                        continue;

                    machine.SetFieldValue<string?>(Models.Metadata.Machine.CloneOfKey, null);
                    machine.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, null);
                    machine.SetFieldValue<string?>(Models.Metadata.Machine.SampleOfKey, null);
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Remove all romof and cloneof tags from all machines
        /// </summary>
        /// <remarks>Applies to <see cref="ItemsDB"/></remarks>
        private void RemoveMachineRelationshipTagsImplDB()
        {
            var machines = GetMachinesDB();

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(machines, Globals.ParallelOptions, machine =>
#elif NET40_OR_GREATER
            Parallel.ForEach(machines, machine =>
#else
            foreach (var machine in machines)
#endif
            {
                // TODO: Remove merge tags here
                // Get the machine
                if (machine.Value == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                machine.Value.SetFieldValue<string?>(Models.Metadata.Machine.CloneOfKey, null);
                machine.Value.SetFieldValue<string?>(Models.Metadata.Machine.RomOfKey, null);
                machine.Value.SetFieldValue<string?>(Models.Metadata.Machine.SampleOfKey, null);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        #endregion
    }
}