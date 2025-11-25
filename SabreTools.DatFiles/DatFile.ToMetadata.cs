using System.Collections.Generic;
using System.Linq;
using SabreTools.Core.Tools;
using SabreTools.DatItems;

namespace SabreTools.DatFiles
{
    public partial class DatFile
    {
        #region To Metadata

        /// <summary>
        /// Convert metadata information
        /// </summary>
        internal Data.Models.Metadata.MetadataFile? ConvertToMetadata(bool ignoreblanks = false)
        {
            // If we don't have items, we can't do anything
            if (DatStatistics.TotalCount == 0)
                return null;

            // Create an object to hold the data
            var metadataFile = new Data.Models.Metadata.MetadataFile();

            // Convert and assign the header
            var header = Header.GetInternalClone();
            if (header != null)
                metadataFile[Data.Models.Metadata.MetadataFile.HeaderKey] = header;

            // Convert and assign the machines
            var machines = ConvertMachines(ignoreblanks);
            if (machines != null)
                metadataFile[Data.Models.Metadata.MetadataFile.MachineKey] = machines;

            return metadataFile;
        }

        /// <summary>
        /// Convert metadata information
        /// </summary>
        internal Data.Models.Metadata.MetadataFile? ConvertToMetadataDB(bool ignoreblanks = false)
        {
            // If we don't have items, we can't do anything
            if (ItemsDB.DatStatistics.TotalCount == 0)
                return null;

            // Create an object to hold the data
            var metadataFile = new Data.Models.Metadata.MetadataFile();

            // Convert and assign the header
            var header = Header.GetInternalClone();
            if (header != null)
                metadataFile[Data.Models.Metadata.MetadataFile.HeaderKey] = header;

            // Convert and assign the machines
            var machines = ConvertMachinesDB(ignoreblanks);
            if (machines != null)
                metadataFile[Data.Models.Metadata.MetadataFile.MachineKey] = machines;

            return metadataFile;
        }

        /// <summary>
        /// Convert machines information
        /// </summary>
        private Data.Models.Metadata.Machine[]? ConvertMachines(bool ignoreblanks = false)
        {
            // Create a machine list to hold all outputs
            List<Data.Models.Metadata.Machine> machines = [];

            // Loop through the sorted items and create games for them
            foreach (string key in Items.SortedKeys)
            {
                var items = Items.GetItemsForBucket(key, filter: true);
                if (items == null || items.Count == 0)
                    continue;

                // Create a machine to hold everything
                var machine = items[0].GetMachine()!.GetInternalClone();

                // Handle Trurip object, if it exists
                if (machine.ContainsKey(Data.Models.Metadata.Machine.TruripKey))
                {
                    var trurip = machine.Read<DatItems.Trurip>(Data.Models.Metadata.Machine.TruripKey);
                    if (trurip != null)
                    {
                        var truripItem = trurip.ConvertToLogiqx();
                        truripItem.Publisher = machine.ReadString(Data.Models.Metadata.Machine.PublisherKey);
                        truripItem.Year = machine.ReadString(Data.Models.Metadata.Machine.YearKey);
                        truripItem.Players = machine.ReadString(Data.Models.Metadata.Machine.PlayersKey);
                        truripItem.Source = machine.ReadString(Data.Models.Metadata.Machine.SourceFileKey);
                        truripItem.CloneOf = machine.ReadString(Data.Models.Metadata.Machine.CloneOfKey);
                        machine[Data.Models.Metadata.Machine.TruripKey] = truripItem;
                    }
                }

                // Create mapping dictionaries for the Parts, DataAreas, and DiskAreas associated with this machine
                Dictionary<Data.Models.Metadata.Part, Data.Models.Metadata.DatItem> partMappings = [];
                Dictionary<Data.Models.Metadata.Part, (Data.Models.Metadata.DataArea, Data.Models.Metadata.Rom)> dataAreaMappings = [];
                Dictionary<Data.Models.Metadata.Part, (Data.Models.Metadata.DiskArea, Data.Models.Metadata.Disk)> diskAreaMappings = [];

                // Loop through and convert the items to respective lists
                for (int index = 0; index < items.Count; index++)
                {
                    // Get the item
                    var item = items[index];

                    // Check for a "null" item
                    item = ProcessNullifiedItem(item);

                    // Skip if we're ignoring the item
                    if (ShouldIgnore(item, ignoreblanks))
                        continue;

                    switch (item)
                    {
                        case DatItems.Formats.Adjuster adjuster:
                            var adjusterItem = adjuster.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Adjuster?>(machine, Data.Models.Metadata.Machine.AdjusterKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.AdjusterKey, adjusterItem);
                            break;
                        case DatItems.Formats.Archive archive:
                            var archiveItem = archive.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Archive?>(machine, Data.Models.Metadata.Machine.ArchiveKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.ArchiveKey, archiveItem);
                            break;
                        case DatItems.Formats.BiosSet biosSet:
                            var biosSetItem = biosSet.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.BiosSet?>(machine, Data.Models.Metadata.Machine.BiosSetKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.BiosSetKey, biosSetItem);
                            break;
                        case DatItems.Formats.Chip chip:
                            var chipItem = chip.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Chip?>(machine, Data.Models.Metadata.Machine.ChipKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.ChipKey, chipItem);
                            break;
                        case DatItems.Formats.Configuration configuration:
                            var configurationItem = configuration.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Configuration?>(machine, Data.Models.Metadata.Machine.ConfigurationKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.ConfigurationKey, configurationItem);
                            break;
                        case DatItems.Formats.Device device:
                            var deviceItem = device.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Device?>(machine, Data.Models.Metadata.Machine.DeviceKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DeviceKey, deviceItem);
                            break;
                        case DatItems.Formats.DeviceRef deviceRef:
                            var deviceRefItem = deviceRef.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.DeviceRef?>(machine, Data.Models.Metadata.Machine.DeviceRefKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DeviceRefKey, deviceRefItem);
                            break;
                        case DatItems.Formats.DipSwitch dipSwitch:
                            var dipSwitchItem = dipSwitch.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.DipSwitch?>(machine, Data.Models.Metadata.Machine.DipSwitchKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DipSwitchKey, dipSwitchItem);

                            // Add Part mapping
                            bool dipSwitchContainsPart = dipSwitchItem.ContainsKey(DatItems.Formats.DipSwitch.PartKey);
                            if (dipSwitchContainsPart)
                            {
                                var partItem = dipSwitchItem.Read<DatItems.Formats.Part>(DatItems.Formats.DipSwitch.PartKey);
                                if (partItem != null)
                                    partMappings[partItem.GetInternalClone()] = dipSwitchItem;
                            }

                            break;
                        case DatItems.Formats.Disk disk:
                            var diskItem = disk.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Disk?>(machine, Data.Models.Metadata.Machine.DiskKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DiskKey, diskItem);

                            // Add Part and DiskArea mappings
                            bool diskContainsPart = diskItem.ContainsKey(DatItems.Formats.Disk.PartKey);
                            bool diskContainsDiskArea = diskItem.ContainsKey(DatItems.Formats.Disk.DiskAreaKey);
                            if (diskContainsPart && diskContainsDiskArea)
                            {
                                var partItem = diskItem.Read<DatItems.Formats.Part>(DatItems.Formats.Disk.PartKey);
                                if (partItem != null)
                                {
                                    var partItemInternal = partItem.GetInternalClone();
                                    partMappings[partItemInternal] = diskItem;

                                    var diskAreaItem = diskItem.Read<DatItems.Formats.DiskArea>(DatItems.Formats.Disk.DiskAreaKey);
                                    if (diskAreaItem != null)
                                        diskAreaMappings[partItemInternal] = (diskAreaItem.GetInternalClone(), diskItem);
                                }
                            }
                            break;
                        case DatItems.Formats.Display display:
                            var displayItem = ProcessItem(display, machine);
                            EnsureMachineKey<Data.Models.Metadata.Display?>(machine, Data.Models.Metadata.Machine.DisplayKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DisplayKey, displayItem);
                            break;
                        case DatItems.Formats.Driver driver:
                            var driverItem = driver.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Driver?>(machine, Data.Models.Metadata.Machine.DriverKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DriverKey, driverItem);
                            break;
                        case DatItems.Formats.Feature feature:
                            var featureItem = feature.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Feature?>(machine, Data.Models.Metadata.Machine.FeatureKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.FeatureKey, featureItem);
                            break;
                        case DatItems.Formats.Info info:
                            var infoItem = info.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Info?>(machine, Data.Models.Metadata.Machine.InfoKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.InfoKey, infoItem);
                            break;
                        case DatItems.Formats.Input input:
                            var inputItem = input.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Input?>(machine, Data.Models.Metadata.Machine.InputKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.InputKey, inputItem);
                            break;
                        case DatItems.Formats.Media media:
                            var mediaItem = media.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Media?>(machine, Data.Models.Metadata.Machine.MediaKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.MediaKey, mediaItem);
                            break;
                        case DatItems.Formats.PartFeature partFeature:
                            var partFeatureItem = partFeature.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Feature?>(machine, Data.Models.Metadata.Machine.FeatureKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.FeatureKey, partFeatureItem);

                            // Add Part mapping
                            bool partFeatureContainsPart = partFeatureItem.ContainsKey(DatItems.Formats.PartFeature.PartKey);
                            if (partFeatureContainsPart)
                            {
                                var partItem = partFeatureItem.Read<DatItems.Formats.Part>(DatItems.Formats.PartFeature.PartKey);
                                if (partItem != null)
                                    partMappings[partItem.GetInternalClone()] = partFeatureItem;
                            }
                            break;
                        case DatItems.Formats.Port port:
                            var portItem = port.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Port?>(machine, Data.Models.Metadata.Machine.PortKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.PortKey, portItem);
                            break;
                        case DatItems.Formats.RamOption ramOption:
                            var ramOptionItem = ramOption.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.RamOption?>(machine, Data.Models.Metadata.Machine.RamOptionKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.RamOptionKey, ramOptionItem);
                            break;
                        case DatItems.Formats.Release release:
                            var releaseItem = release.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Release?>(machine, Data.Models.Metadata.Machine.ReleaseKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.ReleaseKey, releaseItem);
                            break;
                        case DatItems.Formats.Rom rom:
                            var romItem = ProcessItem(rom, machine);
                            EnsureMachineKey<Data.Models.Metadata.Rom?>(machine, Data.Models.Metadata.Machine.RomKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.RomKey, romItem);

                            // Add Part and DataArea mappings
                            bool romContainsPart = romItem.ContainsKey(DatItems.Formats.Rom.PartKey);
                            bool romContainsDataArea = romItem.ContainsKey(DatItems.Formats.Rom.DataAreaKey);
                            if (romContainsPart && romContainsDataArea)
                            {
                                var partItem = romItem.Read<DatItems.Formats.Part>(DatItems.Formats.Rom.PartKey);
                                if (partItem != null)
                                {
                                    var partItemInternal = partItem.GetInternalClone();
                                    partMappings[partItemInternal] = romItem;

                                    var dataAreaItem = romItem.Read<DatItems.Formats.DataArea>(DatItems.Formats.Rom.DataAreaKey);
                                    if (dataAreaItem != null)
                                        dataAreaMappings[partItemInternal] = (dataAreaItem.GetInternalClone(), romItem);
                                }
                            }
                            break;
                        case DatItems.Formats.Sample sample:
                            var sampleItem = sample.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Sample?>(machine, Data.Models.Metadata.Machine.SampleKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SampleKey, sampleItem);
                            break;
                        case DatItems.Formats.SharedFeat sharedFeat:
                            var sharedFeatItem = sharedFeat.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.SharedFeat?>(machine, Data.Models.Metadata.Machine.SharedFeatKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SharedFeatKey, sharedFeatItem);
                            break;
                        case DatItems.Formats.Slot slot:
                            var slotItem = slot.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Slot?>(machine, Data.Models.Metadata.Machine.SlotKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SlotKey, slotItem);
                            break;
                        case DatItems.Formats.SoftwareList softwareList:
                            var softwareListItem = softwareList.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.SoftwareList?>(machine, Data.Models.Metadata.Machine.SoftwareListKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SoftwareListKey, softwareListItem);
                            break;
                        case DatItems.Formats.Sound sound:
                            var soundItem = sound.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Sound?>(machine, Data.Models.Metadata.Machine.SoundKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SoundKey, soundItem);
                            break;
                    }
                }

                // Handle Part, DiskItem, and DatItem mappings, if they exist
                if (partMappings.Count != 0)
                {
                    // Create a collection to hold the inverted Parts
                    Dictionary<string, Data.Models.Metadata.Part> partItems = [];

                    // Loop through the Part mappings
                    foreach (var partMapping in partMappings)
                    {
                        // Get the mapping pieces
                        var partItem = partMapping.Key;
                        var datItem = partMapping.Value;

                        // Get the part name and skip if there's none
                        string? partName = partItem.ReadString(Data.Models.Metadata.Part.NameKey);
                        if (partName == null)
                            continue;

                        // Create the part in the dictionary, if needed
                        if (!partItems.ContainsKey(partName))
                            partItems[partName] = [];

                        // Copy over string values
                        partItems[partName][Data.Models.Metadata.Part.NameKey] = partName;
                        if (!partItems[partName].ContainsKey(Data.Models.Metadata.Part.InterfaceKey))
                            partItems[partName][Data.Models.Metadata.Part.InterfaceKey] = partItem.ReadString(Data.Models.Metadata.Part.InterfaceKey);

                        // Clear any empty fields
                        ClearEmptyKeys(partItems[partName]);

                        // If the item has a DataArea mapping
                        if (dataAreaMappings.ContainsKey(partItem))
                        {
                            // Get the mapped items
                            var (dataArea, romItem) = dataAreaMappings[partItem];

                            // Clear any empty fields
                            ClearEmptyKeys(romItem);

                            // Get the data area name and skip if there's none
                            string? dataAreaName = dataArea.ReadString(Data.Models.Metadata.DataArea.NameKey);
                            if (dataAreaName != null)
                            {
                                // Get existing data areas as a list
                                var dataAreasArr = partItems[partName].Read<Data.Models.Metadata.DataArea[]>(Data.Models.Metadata.Part.DataAreaKey) ?? [];
                                List<Data.Models.Metadata.DataArea> dataAreas = [.. dataAreasArr];

                                // Find the existing disk area to append to, otherwise create a new disk area
                                int dataAreaIndex = dataAreas.FindIndex(da => da.ReadString(Data.Models.Metadata.DataArea.NameKey) == dataAreaName);
                                Data.Models.Metadata.DataArea aggregateDataArea;
                                if (dataAreaIndex > -1)
                                {
                                    aggregateDataArea = dataAreas[dataAreaIndex];
                                }
                                else
                                {
                                    aggregateDataArea = [];
                                    aggregateDataArea[Data.Models.Metadata.DataArea.EndiannessKey] = dataArea.ReadString(Data.Models.Metadata.DataArea.EndiannessKey);
                                    aggregateDataArea[Data.Models.Metadata.DataArea.NameKey] = dataArea.ReadString(Data.Models.Metadata.DataArea.NameKey);
                                    aggregateDataArea[Data.Models.Metadata.DataArea.SizeKey] = dataArea.ReadString(Data.Models.Metadata.DataArea.SizeKey);
                                    aggregateDataArea[Data.Models.Metadata.DataArea.WidthKey] = dataArea.ReadString(Data.Models.Metadata.DataArea.WidthKey);
                                }

                                // Clear any empty fields
                                ClearEmptyKeys(aggregateDataArea);

                                // Get existing roms as a list
                                var romsArr = aggregateDataArea.Read<Data.Models.Metadata.Rom[]>(Data.Models.Metadata.DataArea.RomKey) ?? [];
                                List<Data.Models.Metadata.Rom> roms = [.. romsArr];

                                // Add the rom to the data area
                                roms.Add(romItem);

                                // Assign back the roms
                                aggregateDataArea[Data.Models.Metadata.DataArea.RomKey] = roms.ToArray();

                                // Assign back the data area
                                if (dataAreaIndex > -1)
                                    dataAreas[dataAreaIndex] = aggregateDataArea;
                                else
                                    dataAreas.Add(aggregateDataArea);

                                // Assign back the data areas array
                                partItems[partName][Data.Models.Metadata.Part.DataAreaKey] = dataAreas.ToArray();
                            }
                        }

                        // If the item has a DiskArea mapping
                        if (diskAreaMappings.ContainsKey(partItem))
                        {
                            // Get the mapped items
                            var (diskArea, diskItem) = diskAreaMappings[partItem];

                            // Clear any empty fields
                            ClearEmptyKeys(diskItem);

                            // Get the disk area name and skip if there's none
                            string? diskAreaName = diskArea.ReadString(Data.Models.Metadata.DiskArea.NameKey);
                            if (diskAreaName != null)
                            {
                                // Get existing disk areas as a list
                                var diskAreasArr = partItems[partName].Read<Data.Models.Metadata.DiskArea[]>(Data.Models.Metadata.Part.DiskAreaKey) ?? [];
                                List<Data.Models.Metadata.DiskArea> diskAreas = [.. diskAreasArr];

                                // Find the existing disk area to append to, otherwise create a new disk area
                                int diskAreaIndex = diskAreas.FindIndex(da => da.ReadString(Data.Models.Metadata.DiskArea.NameKey) == diskAreaName);
                                Data.Models.Metadata.DiskArea aggregateDiskArea;
                                if (diskAreaIndex > -1)
                                {
                                    aggregateDiskArea = diskAreas[diskAreaIndex];
                                }
                                else
                                {
                                    aggregateDiskArea = [];
                                    aggregateDiskArea[Data.Models.Metadata.DiskArea.NameKey] = diskArea.ReadString(Data.Models.Metadata.DiskArea.NameKey);
                                }

                                // Clear any empty fields
                                ClearEmptyKeys(aggregateDiskArea);

                                // Get existing disks as a list
                                var disksArr = aggregateDiskArea.Read<Data.Models.Metadata.Disk[]>(Data.Models.Metadata.DiskArea.DiskKey) ?? [];
                                List<Data.Models.Metadata.Disk> disks = [.. disksArr];

                                // Add the disk to the data area
                                disks.Add(diskItem);

                                // Assign back the disks
                                aggregateDiskArea[Data.Models.Metadata.DiskArea.DiskKey] = disks.ToArray();

                                // Assign back the disk area
                                if (diskAreaIndex > -1)
                                    diskAreas[diskAreaIndex] = aggregateDiskArea;
                                else
                                    diskAreas.Add(aggregateDiskArea);

                                // Assign back the disk areas array
                                partItems[partName][Data.Models.Metadata.Part.DiskAreaKey] = diskAreas.ToArray();
                            }
                        }

                        // If the item is a DipSwitch
                        if (datItem is Data.Models.Metadata.DipSwitch dipSwitchItem)
                        {
                            // Get existing dipswitches as a list
                            var dipSwitchesArr = partItems[partName].Read<Data.Models.Metadata.DipSwitch[]>(Data.Models.Metadata.Part.DipSwitchKey) ?? [];
                            List<Data.Models.Metadata.DipSwitch> dipSwitches = [.. dipSwitchesArr];

                            // Clear any empty fields
                            ClearEmptyKeys(dipSwitchItem);

                            // Add the dipswitch
                            dipSwitches.Add(dipSwitchItem);

                            // Assign back the dipswitches
                            partItems[partName][Data.Models.Metadata.Part.DipSwitchKey] = dipSwitches.ToArray();
                        }

                        // If the item is a Feature
                        else if (datItem is Data.Models.Metadata.Feature featureItem)
                        {
                            // Get existing features as a list
                            var featuresArr = partItems[partName].Read<Data.Models.Metadata.Feature[]>(Data.Models.Metadata.Part.FeatureKey) ?? [];
                            List<Data.Models.Metadata.Feature> features = [.. featuresArr];

                            // Clear any empty fields
                            ClearEmptyKeys(featureItem);

                            // Add the feature
                            features.Add(featureItem);

                            // Assign back the features
                            partItems[partName][Data.Models.Metadata.Part.FeatureKey] = features.ToArray();
                        }
                    }

                    // Assign the part array to the machine
                    machine[Data.Models.Metadata.Machine.PartKey] = (Data.Models.Metadata.Part[])[.. partItems.Values];
                }

                // Add the machine to the list
                machines.Add(machine);
            }

            // Return the list of machines
            return [.. machines];
        }

        /// <summary>
        /// Convert machines information
        /// </summary>
        private Data.Models.Metadata.Machine[]? ConvertMachinesDB(bool ignoreblanks = false)
        {
            // Create a machine list to hold all outputs
            List<Data.Models.Metadata.Machine> machines = [];

            // Loop through the sorted items and create games for them
            foreach (string key in ItemsDB.SortedKeys)
            {
                var items = GetItemsForBucketDB(key, filter: true);
                if (items == null || items.Count == 0)
                    continue;

                // Create a machine to hold everything
                var machine = GetMachineForItemDB(items.First().Key).Value!.GetInternalClone();

                // Handle Trurip object, if it exists
                if (machine.ContainsKey(Data.Models.Metadata.Machine.TruripKey))
                {
                    var trurip = machine.Read<DatItems.Trurip>(Data.Models.Metadata.Machine.TruripKey);
                    if (trurip != null)
                    {
                        var truripItem = trurip.ConvertToLogiqx();
                        truripItem.Publisher = machine.ReadString(Data.Models.Metadata.Machine.PublisherKey);
                        truripItem.Year = machine.ReadString(Data.Models.Metadata.Machine.YearKey);
                        truripItem.Players = machine.ReadString(Data.Models.Metadata.Machine.PlayersKey);
                        truripItem.Source = machine.ReadString(Data.Models.Metadata.Machine.SourceFileKey);
                        truripItem.CloneOf = machine.ReadString(Data.Models.Metadata.Machine.CloneOfKey);
                        machine[Data.Models.Metadata.Machine.TruripKey] = truripItem;
                    }
                }

                // Create mapping dictionaries for the Parts, DataAreas, and DiskAreas associated with this machine
                Dictionary<Data.Models.Metadata.Part, Data.Models.Metadata.DatItem> partMappings = [];
                Dictionary<Data.Models.Metadata.Part, (Data.Models.Metadata.DataArea, Data.Models.Metadata.Rom)> dataAreaMappings = [];
                Dictionary<Data.Models.Metadata.Part, (Data.Models.Metadata.DiskArea, Data.Models.Metadata.Disk)> diskAreaMappings = [];

                // Loop through and convert the items to respective lists
                foreach (var kvp in items)
                {
                    // Check for a "null" item
                    var item = new KeyValuePair<long, DatItem>(kvp.Key, ProcessNullifiedItem(kvp.Value));

                    // Skip if we're ignoring the item
                    if (ShouldIgnore(item.Value, ignoreblanks))
                        continue;

                    switch (item.Value)
                    {
                        case DatItems.Formats.Adjuster adjuster:
                            var adjusterItem = adjuster.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Adjuster?>(machine, Data.Models.Metadata.Machine.AdjusterKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.AdjusterKey, adjusterItem);
                            break;
                        case DatItems.Formats.Archive archive:
                            var archiveItem = archive.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Archive?>(machine, Data.Models.Metadata.Machine.ArchiveKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.ArchiveKey, archiveItem);
                            break;
                        case DatItems.Formats.BiosSet biosSet:
                            var biosSetItem = biosSet.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.BiosSet?>(machine, Data.Models.Metadata.Machine.BiosSetKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.BiosSetKey, biosSetItem);
                            break;
                        case DatItems.Formats.Chip chip:
                            var chipItem = chip.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Chip?>(machine, Data.Models.Metadata.Machine.ChipKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.ChipKey, chipItem);
                            break;
                        case DatItems.Formats.Configuration configuration:
                            var configurationItem = configuration.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Configuration?>(machine, Data.Models.Metadata.Machine.ConfigurationKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.ConfigurationKey, configurationItem);
                            break;
                        case DatItems.Formats.Device device:
                            var deviceItem = device.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Device?>(machine, Data.Models.Metadata.Machine.DeviceKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DeviceKey, deviceItem);
                            break;
                        case DatItems.Formats.DeviceRef deviceRef:
                            var deviceRefItem = deviceRef.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.DeviceRef?>(machine, Data.Models.Metadata.Machine.DeviceRefKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DeviceRefKey, deviceRefItem);
                            break;
                        case DatItems.Formats.DipSwitch dipSwitch:
                            var dipSwitchItem = dipSwitch.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.DipSwitch?>(machine, Data.Models.Metadata.Machine.DipSwitchKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DipSwitchKey, dipSwitchItem);

                            // Add Part mapping
                            bool dipSwitchContainsPart = dipSwitchItem.ContainsKey(DatItems.Formats.DipSwitch.PartKey);
                            if (dipSwitchContainsPart)
                            {
                                var partItem = dipSwitchItem.Read<DatItems.Formats.Part>(DatItems.Formats.DipSwitch.PartKey);
                                if (partItem != null)
                                    partMappings[partItem.GetInternalClone()] = dipSwitchItem;
                            }

                            break;
                        case DatItems.Formats.Disk disk:
                            var diskItem = disk.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Disk?>(machine, Data.Models.Metadata.Machine.DiskKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DiskKey, diskItem);

                            // Add Part and DiskArea mappings
                            bool diskContainsPart = diskItem.ContainsKey(DatItems.Formats.Disk.PartKey);
                            bool diskContainsDiskArea = diskItem.ContainsKey(DatItems.Formats.Disk.DiskAreaKey);
                            if (diskContainsPart && diskContainsDiskArea)
                            {
                                var partItem = diskItem.Read<DatItems.Formats.Part>(DatItems.Formats.Disk.PartKey);
                                if (partItem != null)
                                {
                                    var partItemInternal = partItem.GetInternalClone();
                                    partMappings[partItemInternal] = diskItem;

                                    var diskAreaItem = diskItem.Read<DatItems.Formats.DiskArea>(DatItems.Formats.Disk.DiskAreaKey);
                                    if (diskAreaItem != null)
                                        diskAreaMappings[partItemInternal] = (diskAreaItem.GetInternalClone(), diskItem);
                                }
                            }
                            break;
                        case DatItems.Formats.Display display:
                            var displayItem = ProcessItem(display, machine);
                            EnsureMachineKey<Data.Models.Metadata.Display?>(machine, Data.Models.Metadata.Machine.DisplayKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DisplayKey, displayItem);
                            break;
                        case DatItems.Formats.Driver driver:
                            var driverItem = driver.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Driver?>(machine, Data.Models.Metadata.Machine.DriverKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.DriverKey, driverItem);
                            break;
                        case DatItems.Formats.Feature feature:
                            var featureItem = feature.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Feature?>(machine, Data.Models.Metadata.Machine.FeatureKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.FeatureKey, featureItem);
                            break;
                        case DatItems.Formats.Info info:
                            var infoItem = info.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Info?>(machine, Data.Models.Metadata.Machine.InfoKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.InfoKey, infoItem);
                            break;
                        case DatItems.Formats.Input input:
                            var inputItem = input.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Input?>(machine, Data.Models.Metadata.Machine.InputKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.InputKey, inputItem);
                            break;
                        case DatItems.Formats.Media media:
                            var mediaItem = media.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Media?>(machine, Data.Models.Metadata.Machine.MediaKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.MediaKey, mediaItem);
                            break;
                        case DatItems.Formats.PartFeature partFeature:
                            var partFeatureItem = partFeature.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Feature?>(machine, Data.Models.Metadata.Machine.FeatureKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.FeatureKey, partFeatureItem);

                            // Add Part mapping
                            bool partFeatureContainsPart = partFeatureItem.ContainsKey(DatItems.Formats.PartFeature.PartKey);
                            if (partFeatureContainsPart)
                            {
                                var partItem = partFeatureItem.Read<DatItems.Formats.Part>(DatItems.Formats.PartFeature.PartKey);
                                if (partItem != null)
                                    partMappings[partItem.GetInternalClone()] = partFeatureItem;
                            }
                            break;
                        case DatItems.Formats.Port port:
                            var portItem = port.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Port?>(machine, Data.Models.Metadata.Machine.PortKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.PortKey, portItem);
                            break;
                        case DatItems.Formats.RamOption ramOption:
                            var ramOptionItem = ramOption.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.RamOption?>(machine, Data.Models.Metadata.Machine.RamOptionKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.RamOptionKey, ramOptionItem);
                            break;
                        case DatItems.Formats.Release release:
                            var releaseItem = release.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Release?>(machine, Data.Models.Metadata.Machine.ReleaseKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.ReleaseKey, releaseItem);
                            break;
                        case DatItems.Formats.Rom rom:
                            var romItem = ProcessItem(rom, machine);
                            EnsureMachineKey<Data.Models.Metadata.Rom?>(machine, Data.Models.Metadata.Machine.RomKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.RomKey, romItem);

                            // Add Part and DataArea mappings
                            bool romContainsPart = romItem.ContainsKey(DatItems.Formats.Rom.PartKey);
                            bool romContainsDataArea = romItem.ContainsKey(DatItems.Formats.Rom.DataAreaKey);
                            if (romContainsPart && romContainsDataArea)
                            {
                                var partItem = romItem.Read<DatItems.Formats.Part>(DatItems.Formats.Rom.PartKey);
                                if (partItem != null)
                                {
                                    var partItemInternal = partItem.GetInternalClone();
                                    partMappings[partItemInternal] = romItem;

                                    var dataAreaItem = romItem.Read<DatItems.Formats.DataArea>(DatItems.Formats.Rom.DataAreaKey);
                                    if (dataAreaItem != null)
                                        dataAreaMappings[partItemInternal] = (dataAreaItem.GetInternalClone(), romItem);
                                }
                            }
                            break;
                        case DatItems.Formats.Sample sample:
                            var sampleItem = sample.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Sample?>(machine, Data.Models.Metadata.Machine.SampleKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SampleKey, sampleItem);
                            break;
                        case DatItems.Formats.SharedFeat sharedFeat:
                            var sharedFeatItem = sharedFeat.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.SharedFeat?>(machine, Data.Models.Metadata.Machine.SharedFeatKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SharedFeatKey, sharedFeatItem);
                            break;
                        case DatItems.Formats.Slot slot:
                            var slotItem = slot.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Slot?>(machine, Data.Models.Metadata.Machine.SlotKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SlotKey, slotItem);
                            break;
                        case DatItems.Formats.SoftwareList softwareList:
                            var softwareListItem = softwareList.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.SoftwareList?>(machine, Data.Models.Metadata.Machine.SoftwareListKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SoftwareListKey, softwareListItem);
                            break;
                        case DatItems.Formats.Sound sound:
                            var soundItem = sound.GetInternalClone();
                            EnsureMachineKey<Data.Models.Metadata.Sound?>(machine, Data.Models.Metadata.Machine.SoundKey);
                            AppendToMachineKey(machine, Data.Models.Metadata.Machine.SoundKey, soundItem);
                            break;
                    }
                }

                // Handle Part, DiskItem, and DatItem mappings, if they exist
                if (partMappings.Count != 0)
                {
                    // Create a collection to hold the inverted Parts
                    Dictionary<string, Data.Models.Metadata.Part> partItems = [];

                    // Loop through the Part mappings
                    foreach (var partMapping in partMappings)
                    {
                        // Get the mapping pieces
                        var partItem = partMapping.Key;
                        var datItem = partMapping.Value;

                        // Get the part name and skip if there's none
                        string? partName = partItem.ReadString(Data.Models.Metadata.Part.NameKey);
                        if (partName == null)
                            continue;

                        // Create the part in the dictionary, if needed
                        if (!partItems.ContainsKey(partName))
                            partItems[partName] = [];

                        // Copy over string values
                        partItems[partName][Data.Models.Metadata.Part.NameKey] = partName;
                        if (!partItems[partName].ContainsKey(Data.Models.Metadata.Part.InterfaceKey))
                            partItems[partName][Data.Models.Metadata.Part.InterfaceKey] = partItem.ReadString(Data.Models.Metadata.Part.InterfaceKey);

                        // Clear any empty fields
                        ClearEmptyKeys(partItems[partName]);

                        // If the item has a DataArea mapping
                        if (dataAreaMappings.ContainsKey(partItem))
                        {
                            // Get the mapped items
                            var (dataArea, romItem) = dataAreaMappings[partItem];

                            // Clear any empty fields
                            ClearEmptyKeys(romItem);

                            // Get the data area name and skip if there's none
                            string? dataAreaName = dataArea.ReadString(Data.Models.Metadata.DataArea.NameKey);
                            if (dataAreaName != null)
                            {
                                // Get existing data areas as a list
                                var dataAreasArr = partItems[partName].Read<Data.Models.Metadata.DataArea[]>(Data.Models.Metadata.Part.DataAreaKey) ?? [];
                                List<Data.Models.Metadata.DataArea> dataAreas = [.. dataAreasArr];

                                // Find the existing disk area to append to, otherwise create a new disk area
                                int dataAreaIndex = dataAreas.FindIndex(da => da.ReadString(Data.Models.Metadata.DataArea.NameKey) == dataAreaName);
                                Data.Models.Metadata.DataArea aggregateDataArea;
                                if (dataAreaIndex > -1)
                                {
                                    aggregateDataArea = dataAreas[dataAreaIndex];
                                }
                                else
                                {
                                    aggregateDataArea = [];
                                    aggregateDataArea[Data.Models.Metadata.DataArea.EndiannessKey] = dataArea.ReadString(Data.Models.Metadata.DataArea.EndiannessKey);
                                    aggregateDataArea[Data.Models.Metadata.DataArea.NameKey] = dataArea.ReadString(Data.Models.Metadata.DataArea.NameKey);
                                    aggregateDataArea[Data.Models.Metadata.DataArea.SizeKey] = dataArea.ReadString(Data.Models.Metadata.DataArea.SizeKey);
                                    aggregateDataArea[Data.Models.Metadata.DataArea.WidthKey] = dataArea.ReadString(Data.Models.Metadata.DataArea.WidthKey);
                                }

                                // Clear any empty fields
                                ClearEmptyKeys(aggregateDataArea);

                                // Get existing roms as a list
                                var romsArr = aggregateDataArea.Read<Data.Models.Metadata.Rom[]>(Data.Models.Metadata.DataArea.RomKey) ?? [];
                                List<Data.Models.Metadata.Rom> roms = [.. romsArr];

                                // Add the rom to the data area
                                roms.Add(romItem);

                                // Assign back the roms
                                aggregateDataArea[Data.Models.Metadata.DataArea.RomKey] = roms.ToArray();

                                // Assign back the data area
                                if (dataAreaIndex > -1)
                                    dataAreas[dataAreaIndex] = aggregateDataArea;
                                else
                                    dataAreas.Add(aggregateDataArea);

                                // Assign back the data areas array
                                partItems[partName][Data.Models.Metadata.Part.DataAreaKey] = dataAreas.ToArray();
                            }
                        }

                        // If the item has a DiskArea mapping
                        if (diskAreaMappings.ContainsKey(partItem))
                        {
                            // Get the mapped items
                            var (diskArea, diskItem) = diskAreaMappings[partItem];

                            // Clear any empty fields
                            ClearEmptyKeys(diskItem);

                            // Get the disk area name and skip if there's none
                            string? diskAreaName = diskArea.ReadString(Data.Models.Metadata.DiskArea.NameKey);
                            if (diskAreaName != null)
                            {
                                // Get existing disk areas as a list
                                var diskAreasArr = partItems[partName].Read<Data.Models.Metadata.DiskArea[]>(Data.Models.Metadata.Part.DiskAreaKey) ?? [];
                                List<Data.Models.Metadata.DiskArea> diskAreas = [.. diskAreasArr];

                                // Find the existing disk area to append to, otherwise create a new disk area
                                int diskAreaIndex = diskAreas.FindIndex(da => da.ReadString(Data.Models.Metadata.DiskArea.NameKey) == diskAreaName);
                                Data.Models.Metadata.DiskArea aggregateDiskArea;
                                if (diskAreaIndex > -1)
                                {
                                    aggregateDiskArea = diskAreas[diskAreaIndex];
                                }
                                else
                                {
                                    aggregateDiskArea = [];
                                    aggregateDiskArea[Data.Models.Metadata.DiskArea.NameKey] = diskArea.ReadString(Data.Models.Metadata.DiskArea.NameKey);
                                }

                                // Clear any empty fields
                                ClearEmptyKeys(aggregateDiskArea);

                                // Get existing disks as a list
                                var disksArr = aggregateDiskArea.Read<Data.Models.Metadata.Disk[]>(Data.Models.Metadata.DiskArea.DiskKey) ?? [];
                                List<Data.Models.Metadata.Disk> disks = [.. disksArr];

                                // Add the disk to the data area
                                disks.Add(diskItem);

                                // Assign back the disks
                                aggregateDiskArea[Data.Models.Metadata.DiskArea.DiskKey] = disks.ToArray();

                                // Assign back the disk area
                                if (diskAreaIndex > -1)
                                    diskAreas[diskAreaIndex] = aggregateDiskArea;
                                else
                                    diskAreas.Add(aggregateDiskArea);

                                // Assign back the disk areas array
                                partItems[partName][Data.Models.Metadata.Part.DiskAreaKey] = diskAreas.ToArray();
                            }
                        }

                        // If the item is a DipSwitch
                        if (datItem is Data.Models.Metadata.DipSwitch dipSwitchItem)
                        {
                            // Get existing dipswitches as a list
                            var dipSwitchesArr = partItems[partName].Read<Data.Models.Metadata.DipSwitch[]>(Data.Models.Metadata.Part.DipSwitchKey) ?? [];
                            List<Data.Models.Metadata.DipSwitch> dipSwitches = [.. dipSwitchesArr];

                            // Clear any empty fields
                            ClearEmptyKeys(dipSwitchItem);

                            // Add the dipswitch
                            dipSwitches.Add(dipSwitchItem);

                            // Assign back the dipswitches
                            partItems[partName][Data.Models.Metadata.Part.DipSwitchKey] = dipSwitches.ToArray();
                        }

                        // If the item is a Feature
                        else if (datItem is Data.Models.Metadata.Feature featureItem)
                        {
                            // Get existing features as a list
                            var featuresArr = partItems[partName].Read<Data.Models.Metadata.Feature[]>(Data.Models.Metadata.Part.FeatureKey) ?? [];
                            List<Data.Models.Metadata.Feature> features = [.. featuresArr];

                            // Clear any empty fields
                            ClearEmptyKeys(featureItem);

                            // Add the feature
                            features.Add(featureItem);

                            // Assign back the features
                            partItems[partName][Data.Models.Metadata.Part.FeatureKey] = features.ToArray();
                        }
                    }

                    // Assign the part array to the machine
                    machine[Data.Models.Metadata.Machine.PartKey] = (Data.Models.Metadata.Part[])[..partItems.Values];
                }

                // Add the machine to the list
                machines.Add(machine);
            }

            // Return the list of machines
            return [.. machines];
        }

        /// <summary>
        /// Convert Display information
        /// </summary>
        /// <param name="item">Item to convert</param>
        /// <param name="machine">Machine to use for Video</param>
        /// <remarks>
        /// This method is required because both a Display and a Video
        /// item might be created and added for a given Display input.
        /// </remarks>
        private static Data.Models.Metadata.Display ProcessItem(DatItems.Formats.Display item, Data.Models.Metadata.Machine machine)
        {
            var displayItem = item.GetInternalClone();

            // Create a Video for any item that has specific fields
            if (displayItem.ContainsKey(Data.Models.Metadata.Video.AspectXKey))
            {
                var videoItem = new Data.Models.Metadata.Video
                {
                    [Data.Models.Metadata.Video.AspectXKey] = displayItem.ReadLong(Data.Models.Metadata.Video.AspectXKey).ToString(),
                    [Data.Models.Metadata.Video.AspectYKey] = displayItem.ReadLong(Data.Models.Metadata.Video.AspectYKey).ToString(),
                    [Data.Models.Metadata.Video.HeightKey] = displayItem.ReadLong(Data.Models.Metadata.Display.HeightKey).ToString(),
                    [Data.Models.Metadata.Video.RefreshKey] = displayItem.ReadDouble(Data.Models.Metadata.Display.RefreshKey).ToString(),
                    [Data.Models.Metadata.Video.ScreenKey] = displayItem.ReadString(Data.Models.Metadata.Display.DisplayTypeKey).AsDisplayType().AsStringValue(),
                    [Data.Models.Metadata.Video.WidthKey] = displayItem.ReadLong(Data.Models.Metadata.Display.WidthKey).ToString()
                };

                switch (displayItem.ReadLong(Data.Models.Metadata.Display.RotateKey))
                {
                    case 0:
                    case 180:
                        videoItem[Data.Models.Metadata.Video.OrientationKey] = "horizontal";
                        break;
                    case 90:
                    case 270:
                        videoItem[Data.Models.Metadata.Video.OrientationKey] = "vertical";
                        break;
                }

                EnsureMachineKey<Data.Models.Metadata.Video?>(machine, Data.Models.Metadata.Machine.VideoKey);
                AppendToMachineKey(machine, Data.Models.Metadata.Machine.VideoKey, videoItem);
            }

            return displayItem;
        }

        /// <summary>
        /// Convert Rom information
        /// </summary>
        /// <param name="item">Item to convert</param>
        /// <param name="machine">Machine to use for Part and DataArea</param>
        /// <remarks>
        /// This method is required because both a Rom and a Dump
        /// item might be created and added for a given Rom input.
        /// </remarks>
        private static Data.Models.Metadata.Rom ProcessItem(DatItems.Formats.Rom item, Data.Models.Metadata.Machine machine)
        {
            var romItem = item.GetInternalClone();

            // Create a Dump for every Rom that has a subtype
            switch (romItem.ReadString(Data.Models.Metadata.Rom.OpenMSXMediaType).AsOpenMSXSubType())
            {
                case OpenMSXSubType.Rom:
                    var dumpRom = new Data.Models.Metadata.Dump();
                    var rom = new Data.Models.Metadata.Rom();

                    rom[Data.Models.Metadata.Rom.NameKey] = romItem.ReadString(Data.Models.Metadata.Rom.NameKey);
                    rom[Data.Models.Metadata.Rom.OffsetKey] = romItem.ReadString(Data.Models.Metadata.Rom.StartKey) ?? romItem.ReadString(Data.Models.Metadata.Rom.OffsetKey);
                    rom[Data.Models.Metadata.Rom.OpenMSXType] = romItem.ReadString(Data.Models.Metadata.Rom.OpenMSXType);
                    rom[Data.Models.Metadata.Rom.RemarkKey] = romItem.ReadString(Data.Models.Metadata.Rom.RemarkKey);
                    rom[Data.Models.Metadata.Rom.SHA1Key] = romItem.ReadString(Data.Models.Metadata.Rom.SHA1Key);
                    rom[Data.Models.Metadata.Rom.StartKey] = romItem.ReadString(Data.Models.Metadata.Rom.StartKey) ?? romItem.ReadString(Data.Models.Metadata.Rom.OffsetKey);

                    dumpRom[Data.Models.Metadata.Dump.RomKey] = rom;

                    var romOriginal = romItem.Read<DatItems.Formats.Original>("ORIGINAL");
                    if (romOriginal != null)
                    {
                        var newOriginal = new Data.Models.Metadata.Original
                        {
                            [Data.Models.Metadata.Original.ValueKey] = romOriginal.Value.FromYesNo(),
                            [Data.Models.Metadata.Original.ContentKey] = romOriginal.Content,
                        };
                        dumpRom[Data.Models.Metadata.Dump.OriginalKey] = newOriginal;
                    }

                    EnsureMachineKey<Data.Models.Metadata.Dump?>(machine, Data.Models.Metadata.Machine.DumpKey);
                    AppendToMachineKey(machine, Data.Models.Metadata.Machine.DumpKey, dumpRom);
                    break;

                case OpenMSXSubType.MegaRom:
                    var dumpMegaRom = new Data.Models.Metadata.Dump();
                    var megaRom = new Data.Models.Metadata.Rom();

                    megaRom[Data.Models.Metadata.Rom.NameKey] = romItem.ReadString(Data.Models.Metadata.Rom.NameKey);
                    megaRom[Data.Models.Metadata.Rom.OffsetKey] = romItem.ReadString(Data.Models.Metadata.Rom.StartKey) ?? romItem.ReadString(Data.Models.Metadata.Rom.OffsetKey);
                    megaRom[Data.Models.Metadata.Rom.OpenMSXType] = romItem.ReadString(Data.Models.Metadata.Rom.OpenMSXType);
                    megaRom[Data.Models.Metadata.Rom.RemarkKey] = romItem.ReadString(Data.Models.Metadata.Rom.RemarkKey);
                    megaRom[Data.Models.Metadata.Rom.SHA1Key] = romItem.ReadString(Data.Models.Metadata.Rom.SHA1Key);
                    megaRom[Data.Models.Metadata.Rom.StartKey] = romItem.ReadString(Data.Models.Metadata.Rom.StartKey) ?? romItem.ReadString(Data.Models.Metadata.Rom.OffsetKey);

                    dumpMegaRom[Data.Models.Metadata.Dump.MegaRomKey] = megaRom;

                    var megaRomOriginal = romItem.Read<DatItems.Formats.Original>("ORIGINAL");
                    if (megaRomOriginal != null)
                    {
                        var newOriginal = new Data.Models.Metadata.Original
                        {
                            [Data.Models.Metadata.Original.ValueKey] = megaRomOriginal.Value.FromYesNo(),
                            [Data.Models.Metadata.Original.ContentKey] = megaRomOriginal.Content,
                        };
                        dumpMegaRom[Data.Models.Metadata.Dump.OriginalKey] = newOriginal;
                    }

                    EnsureMachineKey<Data.Models.Metadata.Dump?>(machine, Data.Models.Metadata.Machine.DumpKey);
                    AppendToMachineKey(machine, Data.Models.Metadata.Machine.DumpKey, dumpMegaRom);
                    break;

                case OpenMSXSubType.SCCPlusCart:
                    var dumpSccPlusCart = new Data.Models.Metadata.Dump();
                    var sccPlusCart = new Data.Models.Metadata.Rom();

                    sccPlusCart[Data.Models.Metadata.Rom.NameKey] = romItem.ReadString(Data.Models.Metadata.Rom.NameKey);
                    sccPlusCart[Data.Models.Metadata.Rom.OffsetKey] = romItem.ReadString(Data.Models.Metadata.Rom.StartKey) ?? romItem.ReadString(Data.Models.Metadata.Rom.OffsetKey);
                    sccPlusCart[Data.Models.Metadata.Rom.OpenMSXType] = romItem.ReadString(Data.Models.Metadata.Rom.OpenMSXType);
                    sccPlusCart[Data.Models.Metadata.Rom.RemarkKey] = romItem.ReadString(Data.Models.Metadata.Rom.RemarkKey);
                    sccPlusCart[Data.Models.Metadata.Rom.SHA1Key] = romItem.ReadString(Data.Models.Metadata.Rom.SHA1Key);
                    sccPlusCart[Data.Models.Metadata.Rom.StartKey] = romItem.ReadString(Data.Models.Metadata.Rom.StartKey) ?? romItem.ReadString(Data.Models.Metadata.Rom.OffsetKey);

                    dumpSccPlusCart[Data.Models.Metadata.Dump.RomKey] = sccPlusCart;

                    var sccPlusCartOriginal = romItem.Read<DatItems.Formats.Original>("ORIGINAL");
                    if (sccPlusCartOriginal != null)
                    {
                        var newOriginal = new Data.Models.Metadata.Original
                        {
                            [Data.Models.Metadata.Original.ValueKey] = sccPlusCartOriginal.Value.FromYesNo(),
                            [Data.Models.Metadata.Original.ContentKey] = sccPlusCartOriginal.Content,
                        };
                        dumpSccPlusCart[Data.Models.Metadata.Dump.OriginalKey] = newOriginal;
                    }

                    EnsureMachineKey<Data.Models.Metadata.Dump?>(machine, Data.Models.Metadata.Machine.DumpKey);
                    AppendToMachineKey(machine, Data.Models.Metadata.Machine.DumpKey, dumpSccPlusCart);
                    break;
            }

            return romItem;
        }

        /// <summary>
        /// Ensure a key in a machine
        /// </summary>
        private static void EnsureMachineKey<T>(Data.Models.Metadata.Machine machine, string key)
        {
            if (machine.Read<T[]?>(key) == null)
#if NET20 || NET35 || NET40 || NET452
                machine[key] = new T[0];
#else
                machine[key] = System.Array.Empty<T>();
#endif
        }

        /// <summary>
        /// Append to a machine key as if its an array
        /// </summary>
        private static void AppendToMachineKey<T>(Data.Models.Metadata.Machine machine, string key, T value) where T : Data.Models.Metadata.DatItem
        {
            // Get the existing array
            var arr = machine.Read<T[]>(key);
            if (arr == null)
                return;

            // Trim all null fields
            ClearEmptyKeys(value);

            // Add to the array
            List<T> list = [.. arr];
            list.Add(value);
            machine[key] = list.ToArray();
        }

        /// <summary>
        /// Clear empty keys from a DictionaryBase object
        /// </summary>
        private static void ClearEmptyKeys(Data.Models.Metadata.DictionaryBase obj)
        {
            string[] fieldNames = [.. obj.Keys];
            foreach (string fieldName in fieldNames)
            {
                if (obj[fieldName] == null)
                    obj.Remove(fieldName);
            }
        }

        #endregion
    }
}
