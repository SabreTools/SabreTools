using System;
using System.Collections.Generic;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
using System.Threading.Tasks;
#endif
using SabreTools.Core;
using SabreTools.Core.Filter;
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
        internal void ConvertFromMetadata(Data.Models.Metadata.MetadataFile? item,
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
            var header = item.Read<Data.Models.Metadata.Header>(Data.Models.Metadata.MetadataFile.HeaderKey);
            if (header != null)
                ConvertHeader(header, keep);

            // Get the machines from the metadata
            var machines = item.ReadItemArray<Data.Models.Metadata.Machine>(Data.Models.Metadata.MetadataFile.MachineKey);
            if (machines != null)
                ConvertMachines(machines, source, sourceIndex: 0, statsOnly, filterRunner);
        }

        /// <summary>
        /// Convert header information
        /// </summary>
        /// <param name="item">Header to convert</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise</param>
        private void ConvertHeader(Data.Models.Metadata.Header? item, bool keep)
        {
            // If the header is invalid, we can't do anything
            if (item == null || item.Count == 0)
                return;

            // Create an internal header
            var header = new DatHeader(item);

            // Convert subheader values
            if (item.ContainsKey(Data.Models.Metadata.Header.CanOpenKey))
            {
                var canOpen = item.Read<Data.Models.OfflineList.CanOpen>(Data.Models.Metadata.Header.CanOpenKey);
                if (canOpen?.Extension != null)
                    Header.SetFieldValue<string[]?>(Data.Models.Metadata.Header.CanOpenKey, canOpen.Extension);
            }
            if (item.ContainsKey(Data.Models.Metadata.Header.ImagesKey))
            {
                var images = item.Read<Data.Models.OfflineList.Images>(Data.Models.Metadata.Header.ImagesKey);
                Header.SetFieldValue<Data.Models.OfflineList.Images?>(Data.Models.Metadata.Header.ImagesKey, images);
            }
            if (item.ContainsKey(Data.Models.Metadata.Header.InfosKey))
            {
                var infos = item.Read<Data.Models.OfflineList.Infos>(Data.Models.Metadata.Header.InfosKey);
                Header.SetFieldValue<Data.Models.OfflineList.Infos?>(Data.Models.Metadata.Header.InfosKey, infos);
            }
            if (item.ContainsKey(Data.Models.Metadata.Header.NewDatKey))
            {
                var newDat = item.Read<Data.Models.OfflineList.NewDat>(Data.Models.Metadata.Header.NewDatKey);
                Header.SetFieldValue<Data.Models.OfflineList.NewDat?>(Data.Models.Metadata.Header.NewDatKey, newDat);
            }
            if (item.ContainsKey(Data.Models.Metadata.Header.SearchKey))
            {
                var search = item.Read<Data.Models.OfflineList.Search>(Data.Models.Metadata.Header.SearchKey);
                Header.SetFieldValue<Data.Models.OfflineList.Search?>(Data.Models.Metadata.Header.SearchKey, search);
            }

            // Selectively set all possible fields -- TODO: Figure out how to make this less manual
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.AuthorKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.AuthorKey, header.GetStringFieldValue(Data.Models.Metadata.Header.AuthorKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.BiosModeKey).AsMergingFlag() == MergingFlag.None)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.BiosModeKey, header.GetStringFieldValue(Data.Models.Metadata.Header.BiosModeKey).AsMergingFlag().AsStringValue());
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.BuildKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.BuildKey, header.GetStringFieldValue(Data.Models.Metadata.Header.BuildKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.CategoryKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.CategoryKey, header.GetStringFieldValue(Data.Models.Metadata.Header.CategoryKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.CommentKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.CommentKey, header.GetStringFieldValue(Data.Models.Metadata.Header.CommentKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.DateKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DateKey, header.GetStringFieldValue(Data.Models.Metadata.Header.DateKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.DatVersionKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DatVersionKey, header.GetStringFieldValue(Data.Models.Metadata.Header.DatVersionKey));
            if (Header.GetBoolFieldValue(Data.Models.Metadata.Header.DebugKey) == null)
                Header.SetFieldValue<bool?>(Data.Models.Metadata.Header.DebugKey, header.GetBoolFieldValue(Data.Models.Metadata.Header.DebugKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.EmailKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.EmailKey, header.GetStringFieldValue(Data.Models.Metadata.Header.EmailKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.EmulatorVersionKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.EmulatorVersionKey, header.GetStringFieldValue(Data.Models.Metadata.Header.EmulatorVersionKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.ForceMergingKey).AsMergingFlag() == MergingFlag.None)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.ForceMergingKey, header.GetStringFieldValue(Data.Models.Metadata.Header.ForceMergingKey).AsMergingFlag().AsStringValue());
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.ForceNodumpKey).AsNodumpFlag() == NodumpFlag.None)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.ForceNodumpKey, header.GetStringFieldValue(Data.Models.Metadata.Header.ForceNodumpKey).AsNodumpFlag().AsStringValue());
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.ForcePackingKey).AsPackingFlag() == PackingFlag.None)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.ForcePackingKey, header.GetStringFieldValue(Data.Models.Metadata.Header.ForcePackingKey).AsPackingFlag().AsStringValue());
            if (Header.GetBoolFieldValue(Data.Models.Metadata.Header.ForceZippingKey) == null)
                Header.SetFieldValue<bool?>(Data.Models.Metadata.Header.ForceZippingKey, header.GetBoolFieldValue(Data.Models.Metadata.Header.ForceZippingKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.HeaderKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.HeaderKey, header.GetStringFieldValue(Data.Models.Metadata.Header.HeaderKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.HomepageKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.HomepageKey, header.GetStringFieldValue(Data.Models.Metadata.Header.HomepageKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.IdKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.IdKey, header.GetStringFieldValue(Data.Models.Metadata.Header.IdKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.ImFolderKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.ImFolderKey, header.GetStringFieldValue(Data.Models.Metadata.Header.ImFolderKey));
            if (Header.GetBoolFieldValue(Data.Models.Metadata.Header.LockBiosModeKey) == null)
                Header.SetFieldValue<bool?>(Data.Models.Metadata.Header.LockBiosModeKey, header.GetBoolFieldValue(Data.Models.Metadata.Header.LockBiosModeKey));
            if (Header.GetBoolFieldValue(Data.Models.Metadata.Header.LockRomModeKey) == null)
                Header.SetFieldValue<bool?>(Data.Models.Metadata.Header.LockRomModeKey, header.GetBoolFieldValue(Data.Models.Metadata.Header.LockRomModeKey));
            if (Header.GetBoolFieldValue(Data.Models.Metadata.Header.LockSampleModeKey) == null)
                Header.SetFieldValue<bool?>(Data.Models.Metadata.Header.LockSampleModeKey, header.GetBoolFieldValue(Data.Models.Metadata.Header.LockSampleModeKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.MameConfigKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.MameConfigKey, header.GetStringFieldValue(Data.Models.Metadata.Header.MameConfigKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.NotesKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NotesKey, header.GetStringFieldValue(Data.Models.Metadata.Header.NotesKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.PluginKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.PluginKey, header.GetStringFieldValue(Data.Models.Metadata.Header.PluginKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.RefNameKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.RefNameKey, header.GetStringFieldValue(Data.Models.Metadata.Header.RefNameKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.RomModeKey).AsMergingFlag() == MergingFlag.None)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.RomModeKey, header.GetStringFieldValue(Data.Models.Metadata.Header.RomModeKey).AsMergingFlag().AsStringValue());
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.RomTitleKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.RomTitleKey, header.GetStringFieldValue(Data.Models.Metadata.Header.RomTitleKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.RootDirKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.RootDirKey, header.GetStringFieldValue(Data.Models.Metadata.Header.RootDirKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.SampleModeKey).AsMergingFlag() == MergingFlag.None)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.SampleModeKey, header.GetStringFieldValue(Data.Models.Metadata.Header.SampleModeKey).AsMergingFlag().AsStringValue());
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.SchemaLocationKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.SchemaLocationKey, header.GetStringFieldValue(Data.Models.Metadata.Header.SchemaLocationKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.ScreenshotsHeightKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.ScreenshotsHeightKey, header.GetStringFieldValue(Data.Models.Metadata.Header.ScreenshotsHeightKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.ScreenshotsWidthKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.ScreenshotsWidthKey, header.GetStringFieldValue(Data.Models.Metadata.Header.ScreenshotsWidthKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.SystemKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.SystemKey, header.GetStringFieldValue(Data.Models.Metadata.Header.SystemKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.TimestampKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.TimestampKey, header.GetStringFieldValue(Data.Models.Metadata.Header.TimestampKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.TypeKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.TypeKey, header.GetStringFieldValue(Data.Models.Metadata.Header.TypeKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.UrlKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.UrlKey, header.GetStringFieldValue(Data.Models.Metadata.Header.UrlKey));
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.VersionKey) == null)
                Header.SetFieldValue<string?>(Data.Models.Metadata.Header.VersionKey, header.GetStringFieldValue(Data.Models.Metadata.Header.VersionKey));

            // Handle implied SuperDAT
            if (Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)?.Contains(" - SuperDAT") == true && keep)
            {
                if (Header.GetStringFieldValue(Data.Models.Metadata.Header.TypeKey) == null)
                    Header.SetFieldValue<string?>(Data.Models.Metadata.Header.TypeKey, "SuperDAT");
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
        private void ConvertMachines(Data.Models.Metadata.Machine[]? items,
            Source source,
            long sourceIndex,
            bool statsOnly,
            FilterRunner? filterRunner)
        {
            // If the array is invalid, we can't do anything
            if (items == null || items.Length == 0)
                return;

            // Loop through the machines and add
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(items, Globals.ParallelOptions, machine =>
#elif NET40_OR_GREATER
            Parallel.ForEach(items, machine =>
#else
            foreach (var machine in items)
#endif
            {
                ConvertMachine(machine, source, sourceIndex, statsOnly, filterRunner);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
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
        private void ConvertMachine(Data.Models.Metadata.Machine? item,
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
            if (item.ContainsKey(Data.Models.Metadata.Machine.AdjusterKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Adjuster>(Data.Models.Metadata.Machine.AdjusterKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Adjuster(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.ArchiveKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Archive>(Data.Models.Metadata.Machine.ArchiveKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Archive(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.BiosSetKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.BiosSet>(Data.Models.Metadata.Machine.BiosSetKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new BiosSet(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.ChipKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Chip>(Data.Models.Metadata.Machine.ChipKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Chip(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.ConfigurationKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Configuration>(Data.Models.Metadata.Machine.ConfigurationKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Configuration(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.DeviceKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Device>(Data.Models.Metadata.Machine.DeviceKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Device(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.DeviceRefKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.DeviceRef>(Data.Models.Metadata.Machine.DeviceRefKey) ?? [];
                // Do not filter these due to later use
                Array.ForEach(items, item =>
                {
                    var datItem = new DeviceRef(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.DipSwitchKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.DipSwitch>(Data.Models.Metadata.Machine.DipSwitchKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new DipSwitch(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.DiskKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Disk>(Data.Models.Metadata.Machine.DiskKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Disk(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.DisplayKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Display>(Data.Models.Metadata.Machine.DisplayKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Display(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.DriverKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Driver>(Data.Models.Metadata.Machine.DriverKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Driver(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.DumpKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Dump>(Data.Models.Metadata.Machine.DumpKey) ?? [];
                for (int i = 0; i < items.Length; i++)
                {
                    var datItem = new Rom(items[i], machine, source, i);
                    if (datItem.GetName() != null)
                    {
                        AddItem(datItem, statsOnly);
                        // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                    }
                }
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.FeatureKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Feature>(Data.Models.Metadata.Machine.FeatureKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Feature(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.InfoKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Info>(Data.Models.Metadata.Machine.InfoKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Info(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.InputKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Input>(Data.Models.Metadata.Machine.InputKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Input(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.MediaKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Media>(Data.Models.Metadata.Machine.MediaKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Media(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.PartKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Part>(Data.Models.Metadata.Machine.PartKey) ?? [];
                ProcessItems(items, machine, machineIndex: 0, source, sourceIndex, statsOnly, filterRunner);
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.PortKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Port>(Data.Models.Metadata.Machine.PortKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Port(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.RamOptionKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.RamOption>(Data.Models.Metadata.Machine.RamOptionKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new RamOption(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.ReleaseKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Release>(Data.Models.Metadata.Machine.ReleaseKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Release(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.RomKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Rom>(Data.Models.Metadata.Machine.RomKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Rom(item, machine, source);
                    datItem.SetFieldValue<Source?>(DatItem.SourceKey, source);
                    datItem.CopyMachineInformation(machine);

                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.SampleKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Sample>(Data.Models.Metadata.Machine.SampleKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Sample(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.SharedFeatKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.SharedFeat>(Data.Models.Metadata.Machine.SharedFeatKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new SharedFeat(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.SlotKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Slot>(Data.Models.Metadata.Machine.SlotKey) ?? [];
                // Do not filter these due to later use
                Array.ForEach(items, item =>
                {
                    var datItem = new Slot(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.SoftwareListKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.SoftwareList>(Data.Models.Metadata.Machine.SoftwareListKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new SoftwareList(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.SoundKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Sound>(Data.Models.Metadata.Machine.SoundKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Sound(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
            }
            if (item.ContainsKey(Data.Models.Metadata.Machine.VideoKey))
            {
                var items = item.ReadItemArray<Data.Models.Metadata.Video>(Data.Models.Metadata.Machine.VideoKey) ?? [];
                var filtered = filterRunner == null ? items : Array.FindAll(items, i => filterRunner.Run(item));
                Array.ForEach(filtered, item =>
                {
                    var datItem = new Display(item, machine, source);
                    AddItem(datItem, statsOnly);
                    // AddItemDB(datItem, machineIndex, sourceIndex, statsOnly);
                });
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
        private void ProcessItems(Data.Models.Metadata.Part[] items, Machine machine, long machineIndex, Source source, long sourceIndex, bool statsOnly, FilterRunner? filterRunner)
        {
            // If the array is null or empty, return without processing
            if (items.Length == 0)
                return;

            // Loop through the items and add
            foreach (var item in items)
            {
                var partItem = new Part(item, machine, source);

                // Handle subitems
                var dataAreas = item.ReadItemArray<Data.Models.Metadata.DataArea>(Data.Models.Metadata.Part.DataAreaKey);
                if (dataAreas != null)
                {
                    foreach (var dataArea in dataAreas)
                    {
                        var dataAreaItem = new DataArea(dataArea, machine, source);
                        var roms = dataArea.ReadItemArray<Data.Models.Metadata.Rom>(Data.Models.Metadata.DataArea.RomKey);
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
                            var romItem = new Rom(rom, machine, source);
                            long? size = romItem.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey);

                            // If the rom is a continue or ignore
                            string? loadFlag = rom.ReadString(Data.Models.Metadata.Rom.LoadFlagKey);
                            if (loadFlag != null
                                && (loadFlag.Equals("continue", StringComparison.OrdinalIgnoreCase)
                                    || loadFlag.Equals("ignore", StringComparison.OrdinalIgnoreCase)))
                            {
                                var lastRom = addRoms[addRoms.Count - 1];
                                long? lastSize = lastRom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey);
                                lastRom.SetFieldValue<long?>(Data.Models.Metadata.Rom.SizeKey, lastSize + size);
                                continue;
                            }

                            romItem.SetFieldValue<DataArea?>(Rom.DataAreaKey, dataAreaItem);
                            romItem.SetFieldValue<Part?>(Rom.PartKey, partItem);

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

                var diskAreas = item.ReadItemArray<Data.Models.Metadata.DiskArea>(Data.Models.Metadata.Part.DiskAreaKey);
                if (diskAreas != null)
                {
                    foreach (var diskArea in diskAreas)
                    {
                        var diskAreaitem = new DiskArea(diskArea, machine, source);
                        var disks = diskArea.ReadItemArray<Data.Models.Metadata.Disk>(Data.Models.Metadata.DiskArea.DiskKey);
                        if (disks == null)
                            continue;

                        foreach (var disk in disks)
                        {
                            // If the item doesn't pass the filter
                            if (filterRunner != null && !filterRunner.Run(disk))
                                continue;

                            var diskItem = new Disk(disk, machine, source);
                            diskItem.SetFieldValue<DiskArea?>(Disk.DiskAreaKey, diskAreaitem);
                            diskItem.SetFieldValue<Part?>(Disk.PartKey, partItem);

                            AddItem(diskItem, statsOnly);
                            // AddItemDB(diskItem, machineIndex, sourceIndex, statsOnly);
                        }
                    }
                }

                var dipSwitches = item.ReadItemArray<Data.Models.Metadata.DipSwitch>(Data.Models.Metadata.Part.DipSwitchKey);
                if (dipSwitches != null)
                {
                    foreach (var dipSwitch in dipSwitches)
                    {
                        // If the item doesn't pass the filter
                        if (filterRunner != null && !filterRunner.Run(dipSwitch))
                            continue;

                        var dipSwitchItem = new DipSwitch(dipSwitch, machine, source);
                        dipSwitchItem.SetFieldValue<Part?>(DipSwitch.PartKey, partItem);

                        AddItem(dipSwitchItem, statsOnly);
                        // AddItemDB(dipSwitchItem, machineIndex, sourceIndex, statsOnly);
                    }
                }

                var partFeatures = item.ReadItemArray<Data.Models.Metadata.Feature>(Data.Models.Metadata.Part.FeatureKey);
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

        #endregion
    }
}
