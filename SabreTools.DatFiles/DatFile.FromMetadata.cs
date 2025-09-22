using System;
using System.Collections.Generic;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif
using SabreTools.Core;
using SabreTools.Core.Filter;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles
{
    public partial class DatFile
    {
        #region From Metadata

        /// <summary>
        /// Convert metadata information
        /// </summary>
        /// <param name="item">Metadata file to convert</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        internal void ConvertFromMetadata(Models.Metadata.MetadataFile? item,
            string filename,
            int indexId,
            bool keep,
            bool statsOnly,
            FilterRunner? filterRunner)
        {
            // If the metadata file is invalid, we can't do anything
            if (item == null || item.Count == 0)
                return;

            // Create an internal source and add to the dictionary
            var source = new Source(indexId, filename);
            // long sourceIndex = AddSourceDB(source);

            // Get the header from the metadata
            var header = item.Read<Models.Metadata.Header>(Models.Metadata.MetadataFile.HeaderKey);
            if (header != null)
                ConvertHeader(header, keep);

            // Get the machines from the metadata
            var machines = item.ReadItemArray<Models.Metadata.Machine>(Models.Metadata.MetadataFile.MachineKey);
            if (machines != null)
                ConvertMachines(machines, source, sourceIndex: 0, statsOnly, filterRunner);
        }

        /// <summary>
        /// Convert header information
        /// </summary>
        /// <param name="item">Header to convert</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise</param>
        private void ConvertHeader(Models.Metadata.Header? item, bool keep)
        {
            // If the header is invalid, we can't do anything
            if (item == null || item.Count == 0)
                return;

            // Create an internal header
            var header = new DatHeader(item);

            // Convert subheader values
            if (item.ContainsKey(Models.Metadata.Header.CanOpenKey))
            {
                var canOpen = item.Read<Models.OfflineList.CanOpen>(Models.Metadata.Header.CanOpenKey);
                if (canOpen?.Extension != null)
                    Header.SetFieldValue<string[]?>(Models.Metadata.Header.CanOpenKey, canOpen.Extension);
            }
            if (item.ContainsKey(Models.Metadata.Header.ImagesKey))
            {
                var images = item.Read<Models.OfflineList.Images>(Models.Metadata.Header.ImagesKey);
                Header.SetFieldValue<Models.OfflineList.Images?>(Models.Metadata.Header.ImagesKey, images);
            }
            if (item.ContainsKey(Models.Metadata.Header.InfosKey))
            {
                var infos = item.Read<Models.OfflineList.Infos>(Models.Metadata.Header.InfosKey);
                Header.SetFieldValue<Models.OfflineList.Infos?>(Models.Metadata.Header.InfosKey, infos);
            }
            if (item.ContainsKey(Models.Metadata.Header.NewDatKey))
            {
                var newDat = item.Read<Models.OfflineList.NewDat>(Models.Metadata.Header.NewDatKey);
                Header.SetFieldValue<Models.OfflineList.NewDat?>(Models.Metadata.Header.NewDatKey, newDat);
            }
            if (item.ContainsKey(Models.Metadata.Header.SearchKey))
            {
                var search = item.Read<Models.OfflineList.Search>(Models.Metadata.Header.SearchKey);
                Header.SetFieldValue<Models.OfflineList.Search?>(Models.Metadata.Header.SearchKey, search);
            }

            // Selectively set all possible fields -- TODO: Figure out how to make this less manual
            if (Header.GetStringFieldValue(Models.Metadata.Header.AuthorKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.AuthorKey, header.GetStringFieldValue(Models.Metadata.Header.AuthorKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.BiosModeKey).AsMergingFlag() == MergingFlag.None)
                Header.SetFieldValue<string?>(Models.Metadata.Header.BiosModeKey, header.GetStringFieldValue(Models.Metadata.Header.BiosModeKey).AsMergingFlag().AsStringValue());
            if (Header.GetStringFieldValue(Models.Metadata.Header.BuildKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.BuildKey, header.GetStringFieldValue(Models.Metadata.Header.BuildKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.CategoryKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.CategoryKey, header.GetStringFieldValue(Models.Metadata.Header.CategoryKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.CommentKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.CommentKey, header.GetStringFieldValue(Models.Metadata.Header.CommentKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.DateKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.DateKey, header.GetStringFieldValue(Models.Metadata.Header.DateKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.DatVersionKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.DatVersionKey, header.GetStringFieldValue(Models.Metadata.Header.DatVersionKey));
            if (Header.GetBoolFieldValue(Models.Metadata.Header.DebugKey) == null)
                Header.SetFieldValue<bool?>(Models.Metadata.Header.DebugKey, header.GetBoolFieldValue(Models.Metadata.Header.DebugKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.EmailKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.EmailKey, header.GetStringFieldValue(Models.Metadata.Header.EmailKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.EmulatorVersionKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.EmulatorVersionKey, header.GetStringFieldValue(Models.Metadata.Header.EmulatorVersionKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.ForceMergingKey).AsMergingFlag() == MergingFlag.None)
                Header.SetFieldValue<string?>(Models.Metadata.Header.ForceMergingKey, header.GetStringFieldValue(Models.Metadata.Header.ForceMergingKey).AsMergingFlag().AsStringValue());
            if (Header.GetStringFieldValue(Models.Metadata.Header.ForceNodumpKey).AsNodumpFlag() == NodumpFlag.None)
                Header.SetFieldValue<string?>(Models.Metadata.Header.ForceNodumpKey, header.GetStringFieldValue(Models.Metadata.Header.ForceNodumpKey).AsNodumpFlag().AsStringValue());
            if (Header.GetStringFieldValue(Models.Metadata.Header.ForcePackingKey).AsPackingFlag() == PackingFlag.None)
                Header.SetFieldValue<string?>(Models.Metadata.Header.ForcePackingKey, header.GetStringFieldValue(Models.Metadata.Header.ForcePackingKey).AsPackingFlag().AsStringValue());
            if (Header.GetBoolFieldValue(Models.Metadata.Header.ForceZippingKey) == null)
                Header.SetFieldValue<bool?>(Models.Metadata.Header.ForceZippingKey, header.GetBoolFieldValue(Models.Metadata.Header.ForceZippingKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.HeaderKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.HeaderKey, header.GetStringFieldValue(Models.Metadata.Header.HeaderKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.HomepageKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.HomepageKey, header.GetStringFieldValue(Models.Metadata.Header.HomepageKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.IdKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.IdKey, header.GetStringFieldValue(Models.Metadata.Header.IdKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.ImFolderKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.ImFolderKey, header.GetStringFieldValue(Models.Metadata.Header.ImFolderKey));
            if (Header.GetBoolFieldValue(Models.Metadata.Header.LockBiosModeKey) == null)
                Header.SetFieldValue<bool?>(Models.Metadata.Header.LockBiosModeKey, header.GetBoolFieldValue(Models.Metadata.Header.LockBiosModeKey));
            if (Header.GetBoolFieldValue(Models.Metadata.Header.LockRomModeKey) == null)
                Header.SetFieldValue<bool?>(Models.Metadata.Header.LockRomModeKey, header.GetBoolFieldValue(Models.Metadata.Header.LockRomModeKey));
            if (Header.GetBoolFieldValue(Models.Metadata.Header.LockSampleModeKey) == null)
                Header.SetFieldValue<bool?>(Models.Metadata.Header.LockSampleModeKey, header.GetBoolFieldValue(Models.Metadata.Header.LockSampleModeKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.MameConfigKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.MameConfigKey, header.GetStringFieldValue(Models.Metadata.Header.MameConfigKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.NameKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, header.GetStringFieldValue(Models.Metadata.Header.NameKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.NotesKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.NotesKey, header.GetStringFieldValue(Models.Metadata.Header.NotesKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.PluginKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.PluginKey, header.GetStringFieldValue(Models.Metadata.Header.PluginKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.RefNameKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.RefNameKey, header.GetStringFieldValue(Models.Metadata.Header.RefNameKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.RomModeKey).AsMergingFlag() == MergingFlag.None)
                Header.SetFieldValue<string?>(Models.Metadata.Header.RomModeKey, header.GetStringFieldValue(Models.Metadata.Header.RomModeKey).AsMergingFlag().AsStringValue());
            if (Header.GetStringFieldValue(Models.Metadata.Header.RomTitleKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.RomTitleKey, header.GetStringFieldValue(Models.Metadata.Header.RomTitleKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.RootDirKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.RootDirKey, header.GetStringFieldValue(Models.Metadata.Header.RootDirKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.SampleModeKey).AsMergingFlag() == MergingFlag.None)
                Header.SetFieldValue<string?>(Models.Metadata.Header.SampleModeKey, header.GetStringFieldValue(Models.Metadata.Header.SampleModeKey).AsMergingFlag().AsStringValue());
            if (Header.GetStringFieldValue(Models.Metadata.Header.SchemaLocationKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.SchemaLocationKey, header.GetStringFieldValue(Models.Metadata.Header.SchemaLocationKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.ScreenshotsHeightKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.ScreenshotsHeightKey, header.GetStringFieldValue(Models.Metadata.Header.ScreenshotsHeightKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.ScreenshotsWidthKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.ScreenshotsWidthKey, header.GetStringFieldValue(Models.Metadata.Header.ScreenshotsWidthKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.SystemKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.SystemKey, header.GetStringFieldValue(Models.Metadata.Header.SystemKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.TimestampKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.TimestampKey, header.GetStringFieldValue(Models.Metadata.Header.TimestampKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.TypeKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.TypeKey, header.GetStringFieldValue(Models.Metadata.Header.TypeKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.UrlKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.UrlKey, header.GetStringFieldValue(Models.Metadata.Header.UrlKey));
            if (Header.GetStringFieldValue(Models.Metadata.Header.VersionKey) == null)
                Header.SetFieldValue<string?>(Models.Metadata.Header.VersionKey, header.GetStringFieldValue(Models.Metadata.Header.VersionKey));

            // Handle implied SuperDAT
            if (Header.GetStringFieldValue(Models.Metadata.Header.NameKey)?.Contains(" - SuperDAT") == true && keep)
            {
                if (Header.GetStringFieldValue(Models.Metadata.Header.TypeKey) == null)
                    Header.SetFieldValue<string?>(Models.Metadata.Header.TypeKey, "SuperDAT");
            }
        }

        /// <summary>
        /// Convert machines information
        /// </summary>
        /// <param name="items">Machine array to convert</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ConvertMachines(Models.Metadata.Machine[]? items,
            Source source,
            long sourceIndex,
            bool statsOnly,
            FilterRunner? filterRunner)
        {
            // If the array is invalid, we can't do anything
            if (items == null || items.Length == 0)
                return;

            // Loop through the machines and add
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(items, Core.Globals.ParallelOptions, machine =>
#elif NET40_OR_GREATER
            Parallel.ForEach(items, machine =>
#else
            foreach (var machine in items)
#endif
            {
                ConvertMachine(machine, source, sourceIndex, statsOnly, filterRunner);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }

        /// <summary>
        /// Convert machine information
        /// </summary>
        /// <param name="item">Machine to convert</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ConvertMachine(Models.Metadata.Machine? item,
            Source source,
            long sourceIndex,
            bool statsOnly,
            FilterRunner? filterRunner)
        {
            // If the machine is invalid, we can't do anything
            if (item == null || item.Count == 0)
                return;

            // If the machine doesn't pass the filter
            if (filterRunner != null && !filterRunner.Run(item))
                return;

            // Create an internal machine and add to the dictionary
            var machine = new Machine(item);
            // long machineIndex = AddMachineDB(machine);

            // Convert items in the machine
            if (item.ContainsKey(Models.Metadata.Machine.AdjusterKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Adjuster>(Models.Metadata.Machine.AdjusterKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.ArchiveKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Archive>(Models.Metadata.Machine.ArchiveKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.BiosSetKey))
            {
                var items = item.ReadItemArray<Models.Metadata.BiosSet>(Models.Metadata.Machine.BiosSetKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.ChipKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Chip>(Models.Metadata.Machine.ChipKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.ConfigurationKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Configuration>(Models.Metadata.Machine.ConfigurationKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.DeviceKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Device>(Models.Metadata.Machine.DeviceKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.DeviceRefKey))
            {
                var items = item.ReadItemArray<Models.Metadata.DeviceRef>(Models.Metadata.Machine.DeviceRefKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly);
            }
            if (item.ContainsKey(Models.Metadata.Machine.DipSwitchKey))
            {
                var items = item.ReadItemArray<Models.Metadata.DipSwitch>(Models.Metadata.Machine.DipSwitchKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.DiskKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Disk>(Models.Metadata.Machine.DiskKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.DisplayKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Display>(Models.Metadata.Machine.DisplayKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.DriverKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Driver>(Models.Metadata.Machine.DriverKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.DumpKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Dump>(Models.Metadata.Machine.DumpKey);
                string? machineName = machine.GetName();
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, machineName, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.FeatureKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Feature>(Models.Metadata.Machine.FeatureKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.InfoKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Info>(Models.Metadata.Machine.InfoKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.InputKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Input>(Models.Metadata.Machine.InputKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.MediaKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Media>(Models.Metadata.Machine.MediaKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.PartKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Part>(Models.Metadata.Machine.PartKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.PortKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Port>(Models.Metadata.Machine.PortKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.RamOptionKey))
            {
                var items = item.ReadItemArray<Models.Metadata.RamOption>(Models.Metadata.Machine.RamOptionKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.ReleaseKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Release>(Models.Metadata.Machine.ReleaseKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.RomKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Rom>(Models.Metadata.Machine.RomKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.SampleKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Sample>(Models.Metadata.Machine.SampleKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.SharedFeatKey))
            {
                var items = item.ReadItemArray<Models.Metadata.SharedFeat>(Models.Metadata.Machine.SharedFeatKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.SlotKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Slot>(Models.Metadata.Machine.SlotKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly);
            }
            if (item.ContainsKey(Models.Metadata.Machine.SoftwareListKey))
            {
                var items = item.ReadItemArray<Models.Metadata.SoftwareList>(Models.Metadata.Machine.SoftwareListKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.SoundKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Sound>(Models.Metadata.Machine.SoundKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Models.Metadata.Machine.VideoKey))
            {
                var items = item.ReadItemArray<Models.Metadata.Video>(Models.Metadata.Machine.VideoKey);
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
        }

        /// <summary>
        /// Convert Adjuster information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Adjuster[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Adjuster(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                //AddItemDB(datItem, machineIndex, sourceIndex, statsOnly, filterRunner);
            }
        }

        /// <summary>
        /// Convert Archive information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Archive[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Archive(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert BiosSet information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.BiosSet[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new BiosSet(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Chip information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Chip[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Chip(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Configuration information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Configuration[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Configuration(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Device information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Device[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Device(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert DeviceRef information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <remarks>Does not get filtered here just in case merging or splitting is done</remarks>
        private void ProcessItems(Models.Metadata.DeviceRef[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                var datItem = new DeviceRef(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert DipSwitch information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.DipSwitch[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new DipSwitch(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Disk information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Disk[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Disk(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Display information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Display[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Display(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Driver information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Driver[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Driver(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Dump information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="machineName">Machine name to use when constructing item names</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        /// TODO: Convert this into a constructor in Rom
        private void ProcessItems(Models.Metadata.Dump[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, string? machineName, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            int index = 0;
            foreach (var dump in items)
            {
                // If we don't have rom data, we can't do anything
                Models.Metadata.Rom? rom = null;
                OpenMSXSubType subType = OpenMSXSubType.NULL;
                if (dump?.Read<Models.Metadata.Rom>(Models.Metadata.Dump.RomKey) != null)
                {
                    rom = dump.Read<Models.Metadata.Rom>(Models.Metadata.Dump.RomKey);
                    subType = OpenMSXSubType.Rom;
                }
                else if (dump?.Read<Models.Metadata.Rom>(Models.Metadata.Dump.MegaRomKey) != null)
                {
                    rom = dump.Read<Models.Metadata.Rom>(Models.Metadata.Dump.MegaRomKey);
                    subType = OpenMSXSubType.MegaRom;
                }
                else if (dump?.Read<Models.Metadata.Rom>(Models.Metadata.Dump.SCCPlusCartKey) != null)
                {
                    rom = dump.Read<Models.Metadata.Rom>(Models.Metadata.Dump.SCCPlusCartKey);
                    subType = OpenMSXSubType.SCCPlusCart;
                }
                else
                {
                    continue;
                }

                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(rom!))
                    continue;

                string name = $"{machineName}_{index++}{(!string.IsNullOrEmpty(rom!.ReadString(Models.Metadata.Rom.RemarkKey)) ? $" {rom.ReadString(Models.Metadata.Rom.RemarkKey)}" : string.Empty)}";

                var datItem = new Rom();
                datItem.SetName(name);
                datItem.SetFieldValue<string?>(Models.Metadata.Rom.OffsetKey, rom.ReadString(Models.Metadata.Rom.StartKey));
                datItem.SetFieldValue<string?>(Models.Metadata.Rom.OpenMSXMediaType, subType.AsStringValue());
                datItem.SetFieldValue<string?>(Models.Metadata.Rom.OpenMSXType, rom.ReadString(Models.Metadata.Rom.OpenMSXType) ?? rom.ReadString(Models.Metadata.Rom.TypeKey));
                datItem.SetFieldValue<string?>(Models.Metadata.Rom.RemarkKey, rom.ReadString(Models.Metadata.Rom.RemarkKey));
                datItem.SetFieldValue<string?>(Models.Metadata.Rom.SHA1Key, rom.ReadString(Models.Metadata.Rom.SHA1Key));
                datItem.SetFieldValue<string?>(Models.Metadata.Rom.StartKey, rom.ReadString(Models.Metadata.Rom.StartKey));
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);

                if (dump.Read<Models.Metadata.Original>(Models.Metadata.Dump.OriginalKey) != null)
                {
                    var original = dump.Read<Models.Metadata.Original>(Models.Metadata.Dump.OriginalKey)!;
                    datItem.SetFieldValue<Original?>("ORIGINAL", new Original
                    {
                        Value = original.ReadBool(Models.Metadata.Original.ValueKey),
                        Content = original.ReadString(Models.Metadata.Original.ContentKey),
                    });
                }

                datItem.CopyMachineInformation(machine);

                // Process hash values
                if (datItem.GetInt64FieldValue(Models.Metadata.Rom.SizeKey) != null)
                    datItem.SetFieldValue<string?>(Models.Metadata.Rom.SizeKey, datItem.GetInt64FieldValue(Models.Metadata.Rom.SizeKey).ToString());
                if (datItem.GetStringFieldValue(Models.Metadata.Rom.CRCKey) != null)
                    datItem.SetFieldValue<string?>(Models.Metadata.Rom.CRCKey, TextHelper.NormalizeCRC32(datItem.GetStringFieldValue(Models.Metadata.Rom.CRCKey)));
                if (datItem.GetStringFieldValue(Models.Metadata.Rom.MD2Key) != null)
                    datItem.SetFieldValue<string?>(Models.Metadata.Rom.MD2Key, TextHelper.NormalizeMD2(datItem.GetStringFieldValue(Models.Metadata.Rom.MD2Key)));
                if (datItem.GetStringFieldValue(Models.Metadata.Rom.MD4Key) != null)
                    datItem.SetFieldValue<string?>(Models.Metadata.Rom.MD4Key, TextHelper.NormalizeMD5(datItem.GetStringFieldValue(Models.Metadata.Rom.MD4Key)));
                if (datItem.GetStringFieldValue(Models.Metadata.Rom.MD5Key) != null)
                    datItem.SetFieldValue<string?>(Models.Metadata.Rom.MD5Key, TextHelper.NormalizeMD5(datItem.GetStringFieldValue(Models.Metadata.Rom.MD5Key)));
                if (datItem.GetStringFieldValue("RIPEMD128") != null)
                    datItem.SetFieldValue<string?>("RIPEMD128", TextHelper.NormalizeRIPEMD128(datItem.GetStringFieldValue("RIPEMD128")));
                if (datItem.GetStringFieldValue("RIPEMD160") != null)
                    datItem.SetFieldValue<string?>("RIPEMD160", TextHelper.NormalizeRIPEMD160(datItem.GetStringFieldValue("RIPEMD160")));
                if (datItem.GetStringFieldValue(Models.Metadata.Rom.SHA1Key) != null)
                    datItem.SetFieldValue<string?>(Models.Metadata.Rom.SHA1Key, TextHelper.NormalizeSHA1(datItem.GetStringFieldValue(Models.Metadata.Rom.SHA1Key)));
                if (datItem.GetStringFieldValue(Models.Metadata.Rom.SHA256Key) != null)
                    datItem.SetFieldValue<string?>(Models.Metadata.Rom.SHA256Key, TextHelper.NormalizeSHA256(datItem.GetStringFieldValue(Models.Metadata.Rom.SHA256Key)));
                if (datItem.GetStringFieldValue(Models.Metadata.Rom.SHA384Key) != null)
                    datItem.SetFieldValue<string?>(Models.Metadata.Rom.SHA384Key, TextHelper.NormalizeSHA384(datItem.GetStringFieldValue(Models.Metadata.Rom.SHA384Key)));
                if (datItem.GetStringFieldValue(Models.Metadata.Rom.SHA512Key) != null)
                    datItem.SetFieldValue<string?>(Models.Metadata.Rom.SHA512Key, TextHelper.NormalizeSHA512(datItem.GetStringFieldValue(Models.Metadata.Rom.SHA512Key)));

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Feature information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Feature[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Feature(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Info information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Info[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Info(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Input information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Input[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Input(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Media information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Media[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Media(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Part information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Part[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                var partItem = new Part(item);

                // Handle subitems
                var dataAreas = item.ReadItemArray<Models.Metadata.DataArea>(Models.Metadata.Part.DataAreaKey);
                if (dataAreas != null)
                {
                    foreach (var dataArea in dataAreas)
                    {
                        var dataAreaItem = new DataArea(dataArea);
                        var roms = dataArea.ReadItemArray<Models.Metadata.Rom>(Models.Metadata.DataArea.RomKey);
                        if (roms == null)
                            continue;

                        // Handle "offset" roms
                        List<Rom> addRoms = [];
                        foreach (var rom in roms)
                        {
                            // If the item doesn't pass the filter
                            if (filterRunner != null && !filterRunner.Run(rom))
                                continue;

                            // Convert the item
                            var romItem = new Rom(rom);
                            long? size = romItem.GetInt64FieldValue(Models.Metadata.Rom.SizeKey);

                            // If the rom is a continue or ignore
                            string? loadFlag = rom.ReadString(Models.Metadata.Rom.LoadFlagKey);
                            if (loadFlag != null
                                && (loadFlag.Equals("continue", StringComparison.OrdinalIgnoreCase)
                                    || loadFlag.Equals("ignore", StringComparison.OrdinalIgnoreCase)))
                            {
                                var lastRom = addRoms[addRoms.Count - 1];
                                long? lastSize = lastRom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey);
                                lastRom.SetFieldValue<long?>(Models.Metadata.Rom.SizeKey, lastSize + size);
                                continue;
                            }

                            romItem.SetFieldValue<DataArea?>(Rom.DataAreaKey, dataAreaItem);
                            romItem.SetFieldValue<Part?>(Rom.PartKey, partItem);
                            romItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                            romItem.CopyMachineInformation(machine);

                            addRoms.Add(romItem);
                        }

                        // Add all of the adjusted roms
                        foreach (var romItem in addRoms)
                        {
                            AddItem(romItem, statsOnly);
                            // AddItemDB(romItem, machineIndex, sourceIndex, statsOnly);
                        }
                    }
                }

                var diskAreas = item.ReadItemArray<Models.Metadata.DiskArea>(Models.Metadata.Part.DiskAreaKey);
                if (diskAreas != null)
                {
                    foreach (var diskArea in diskAreas)
                    {
                        var diskAreaitem = new DiskArea(diskArea);
                        var disks = diskArea.ReadItemArray<Models.Metadata.Disk>(Models.Metadata.DiskArea.DiskKey);
                        if (disks == null)
                            continue;

                        foreach (var disk in disks)
                        {
                            // If the item doesn't pass the filter
                            if (filterRunner != null && !filterRunner.Run(disk))
                                continue;

                            var diskItem = new Disk(disk);
                            diskItem.SetFieldValue<DiskArea?>(Disk.DiskAreaKey, diskAreaitem);
                            diskItem.SetFieldValue<Part?>(Disk.PartKey, partItem);
                            diskItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                            diskItem.CopyMachineInformation(machine);

                            AddItem(diskItem, statsOnly);
                            // AddItemDB(diskItem, machineIndex, sourceIndex, statsOnly);
                        }
                    }
                }

                var dipSwitches = item.ReadItemArray<Models.Metadata.DipSwitch>(Models.Metadata.Part.DipSwitchKey);
                if (dipSwitches != null)
                {
                    foreach (var dipSwitch in dipSwitches)
                    {
                        // If the item doesn't pass the filter
                        if (filterRunner != null && !filterRunner.Run(dipSwitch))
                            continue;

                        var dipSwitchItem = new DipSwitch(dipSwitch);
                        dipSwitchItem.SetFieldValue<Part?>(DipSwitch.PartKey, partItem);
                        dipSwitchItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                        dipSwitchItem.CopyMachineInformation(machine);

                        AddItem(dipSwitchItem, statsOnly);
                        // AddItemDB(dipSwitchItem, machineIndex, sourceIndex, statsOnly);
                    }
                }

                var partFeatures = item.ReadItemArray<Models.Metadata.Feature>(Models.Metadata.Part.FeatureKey);
                if (partFeatures != null)
                {
                    foreach (var partFeature in partFeatures)
                    {
                        // If the item doesn't pass the filter
                        if (filterRunner != null && !filterRunner.Run(partFeature))
                            continue;

                        var partFeatureItem = new PartFeature(partFeature);
                        partFeatureItem.SetFieldValue<Part?>(DipSwitch.PartKey, partItem);
                        partFeatureItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                        partFeatureItem.CopyMachineInformation(machine);

                        AddItem(partFeatureItem, statsOnly);
                        // AddItemDB(partFeatureItem, machineIndex, sourceIndex, statsOnly);
                    }
                }
            }
        }

        /// <summary>
        /// Convert Port information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Port[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Port(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert RamOption information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.RamOption[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new RamOption(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Release information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Release[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Release(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Rom information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Rom[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Rom(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Sample information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Sample[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Sample(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert SharedFeat information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.SharedFeat[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new SharedFeat(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Slot information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <remarks>Does not get filtered here just in case merging or splitting is done</remarks>
        private void ProcessItems(Models.Metadata.Slot[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                var datItem = new Slot(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert SoftwareList information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.SoftwareList[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new SoftwareList(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Sound information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Sound[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Sound(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        /// <summary>
        /// Convert Video information 
        /// </summary>
        /// <param name="items">Array of internal items to convert</param>
        /// <param name="machine">Machine to use with the converted items</param>
        /// <param name="machineIndex">Index of the Machine to use with the converted items</param>
        /// <param name="source">Source to use with the converted items</param>
        /// <param name="sourceIndex">Index of the Source to use with the converted items</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="filterRunner">Optional FilterRunner to filter items on parse</param>
        private void ProcessItems(Models.Metadata.Video[]? items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items == null || items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                // If the item doesn't pass the filter
                if (filterRunner != null && !filterRunner.Run(item))
                    continue;

                var datItem = new Display(item);
                datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                datItem.CopyMachineInformation(machine);

                AddItem(datItem, statsOnly);
                // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
            }
        }

        #endregion
    }
}
