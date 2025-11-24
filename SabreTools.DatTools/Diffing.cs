using System.Collections.Generic;
using System.IO;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
using System.Threading.Tasks;
#endif
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.IO;
using SabreTools.IO.Logging;

namespace SabreTools.DatTools
{
    /// <summary>
    /// This file represents all methods for diffing DatFiles
    /// </summary>
    public class Diffing
    {
        #region Against

        /// <summary>
        /// Output diffs against a base set represented by the current DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="intDat">DatFile to replace the values in</param>
        /// <param name="useGames">True to diff using games, false to use hashes</param>
        public static void Against(DatFile datFile, DatFile intDat, bool useGames)
        {
            InternalStopwatch watch = new($"Comparing '{intDat.Header.GetStringFieldValue(DatHeader.FileNameKey)}' to base DAT");

            // For comparison's sake, we want to a the base bucketing
            if (useGames)
            {
                intDat.BucketBy(ItemKey.Machine);
            }
            else
            {
                intDat.BucketBy(ItemKey.CRC);
                intDat.Deduplicate();
            }

            AgainstImpl(datFile, intDat, useGames);
            AgainstDBImpl(datFile, intDat, useGames);
            watch.Stop();
        }

        /// <summary>
        /// Output diffs against a base set represented by the current DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="intDat">DatFile to replace the values in</param>
        /// <param name="useGames">True to diff using games, false to use hashes</param>
        private static void AgainstImpl(DatFile datFile, DatFile intDat, bool useGames)
        {
            // Then we compare against the base DAT
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(intDat.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(intDat.Items.SortedKeys, key =>
#else
            foreach (var key in intDat.Items.SortedKeys)
#endif
            {
                // Game Against uses game names
                if (useGames)
                {
                    // If the key is null, keep it
                    var intList = intDat.GetItemsForBucket(key);
                    if (intList.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif

                    // If the base DAT doesn't contain the key, keep it
                    var list = datFile.GetItemsForBucket(key);
                    if (list.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif

                    // If the number of items is different, then keep it
                    if (list.Count != intList.Count)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif

                    // Otherwise, compare by name and hash the remaining files
                    bool exactMatch = true;
                    foreach (DatItem item in intList)
                    {
                        // TODO: Make this granular to name as well
                        if (!list.Contains(item))
                        {
                            exactMatch = false;
                            break;
                        }
                    }

                    // If we have an exact match, remove the game
                    if (exactMatch)
                        intDat.RemoveBucket(key);
                }

                // Standard Against uses hashes
                else
                {
                    List<DatItem> datItems = intDat.GetItemsForBucket(key);
                    if (datItems.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif

                    List<DatItem> keepDatItems = [];
                    foreach (DatItem datItem in datItems)
                    {
                        if (!datFile.HasDuplicates(datItem, true))
                            keepDatItems.Add(datItem);
                    }

                    // Now add the new list to the key
                    intDat.RemoveBucket(key);
                    keepDatItems.ForEach(item => intDat.AddItem(item, statsOnly: false));
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Output diffs against a base set represented by the current DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="intDat">DatFile to replace the values in</param>
        /// <param name="useGames">True to diff using games, false to use hashes</param>
        private static void AgainstDBImpl(DatFile datFile, DatFile intDat, bool useGames)
        {
            // Then we compare against the base DAT
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(intDat.ItemsDB.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(intDat.ItemsDB.SortedKeys, key =>
#else
            foreach (var key in intDat.ItemsDB.SortedKeys)
#endif
            {
                // Game Against uses game names
                if (useGames)
                {
                    // If the key is null, keep it
                    var intList = intDat.GetItemsForBucketDB(key);
                    if (intList.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif

                    // If the base DAT doesn't contain the key, keep it
                    List<DatItem> list = [.. datFile.GetItemsForBucketDB(key).Values];
                    if (list.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif

                    // If the number of items is different, then keep it
                    if (list.Count != intList.Count)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif

                    //

                    // Otherwise, compare by name and hash the remaining files
                    bool exactMatch = true;
                    foreach (KeyValuePair<long, DatItem> item in intList)
                    {
                        // TODO: Make this granular to name as well
                        if (!list.Contains(item.Value))
                        {
                            exactMatch = false;
                            break;
                        }
                    }

                    // If we have an exact match, remove the game
                    if (exactMatch)
                        intDat.RemoveBucket(key);
                }

                // Standard Against uses hashes
                else
                {
                    Dictionary<long, DatItem> datItems = intDat.GetItemsForBucketDB(key);
                    if (datItems == null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                        return;
#else
                        continue;
#endif

                    List<KeyValuePair<long, DatItem>> keepDatItems = [];
                    foreach (KeyValuePair<long, DatItem> datItem in datItems)
                    {
                        if (!datFile.HasDuplicates(datItem, true))
                            keepDatItems.Add(datItem);
                    }

                    // Get all existing mappings
                    List<ItemMappings> currentMappings = keepDatItems.ConvertAll(item =>
                    {
                        return new ItemMappings(
                            item.Value,
                            intDat.GetMachineForItemDB(item.Key).Key,
                            intDat.GetSourceForItemDB(item.Key).Key
                        );
                    });

                    // Now add the new list to the key
                    intDat.RemoveBucketDB(key);
                    currentMappings.ForEach(map =>
                        intDat.AddItemDB(map.Item, map.MachineId, map.SourceId, statsOnly: false));
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion

        #region Cascade

        /// <summary>
        /// Output cascading diffs
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="datHeaders">Dat headers used optionally</param>
        /// <returns>List of DatFiles representing the individually indexed items</returns>
        public static List<DatFile> Cascade(DatFile datFile, List<DatHeader> datHeaders)
        {
            // Create a list of DatData objects representing output files
            List<DatFile> outDats = [];

            // Ensure the current DatFile is sorted optimally
            datFile.BucketBy(ItemKey.CRC, norename: false);
            datFile.Deduplicate();

            // Loop through each of the inputs and get or create a new DatData object
            InternalStopwatch watch = new("Initializing and filling all output DATs");

            // Create the DatFiles from the set of headers
            DatFile[] outDatsArray = new DatFile[datHeaders.Count];
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.For(0, datHeaders.Count, Core.Globals.ParallelOptions, j =>
#elif NET40_OR_GREATER
            Parallel.For(0, datHeaders.Count, j =>
#else
            for (int j = 0; j < datHeaders.Count; j++)
#endif
            {
                DatFile diffData = Parser.CreateDatFile(datHeaders[j], new DatModifiers());
                diffData.ResetDictionary();
                FillWithSourceIndex(datFile, diffData, j);
                FillWithSourceIndexDB(datFile, diffData, j);
                outDatsArray[j] = diffData;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif

            outDats = [.. outDatsArray];
            watch.Stop();

            return outDats;
        }

        /// <summary>
        /// Fill a DatFile with all items with a particular source index ID
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="indexDat">DatFile to add found items to</param>
        /// <param name="index">Source index ID to retrieve items for</param>
        /// <returns>DatFile containing all items with the source index ID/returns>
        private static void FillWithSourceIndex(DatFile datFile, DatFile indexDat, int index)
        {
            // Loop through and add the items for this index to the output
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem> items = datFile.GetItemsForBucket(key);

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                foreach (DatItem item in items)
                {
                    var source = item.GetFieldValue<Source?>(DatItem.SourceKey);
                    if (source != null && source.Index == index)
                        indexDat.AddItem(item, statsOnly: false);
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Fill a DatFile with all items with a particular source index ID
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="indexDat">DatFile to add found items to</param>
        /// <param name="index">Source index ID to retrieve items for</param>
        /// <returns>DatFile containing all items with the source index ID/returns>
        private static void FillWithSourceIndexDB(DatFile datFile, DatFile indexDat, int index)
        {
            // Get all current items, machines, and mappings
            var datItems = datFile.ItemsDB.GetItems();
            var machines = datFile.GetMachinesDB();
            var sources = datFile.ItemsDB.GetSources();

            // Create mappings from old index to new index
            var machineRemapping = new Dictionary<long, long>();
            var sourceRemapping = new Dictionary<long, long>();

            // Loop through and add all sources
            foreach (var source in sources)
            {
                long newSourceIndex = indexDat.AddSourceDB(source.Value);
                sourceRemapping[source.Key] = newSourceIndex;
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = indexDat.AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;
            }

            // Loop through and add the items
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datItems, Core.Globals.ParallelOptions, item =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Get the machine and source index for this item
                long machineIndex = datFile.GetMachineForItemDB(item.Key).Key;
                long sourceIndex = datFile.GetSourceForItemDB(item.Key).Key;

                // Get the source associated with the item
                var source = datFile.ItemsDB.GetSource(sourceIndex);

                if (source != null && source.Index == index)
                    indexDat.AddItemDB(item.Value, machineRemapping[machineIndex], sourceRemapping[sourceIndex], statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion

        #region Duplicates

        /// <summary>
        /// Output duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile Duplicates(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.ConvertAll(i => new ParentablePath(i));
            return Duplicates(datFile, paths);
        }

        /// <summary>
        /// Output duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile Duplicates(DatFile datFile, List<ParentablePath> inputs)
        {
            // Initialize duplicate data
            DatFile dupeData = DuplicatesInit(datFile);

            // Now, loop through the dictionary and populate the correct DATs
            var watch = new InternalStopwatch("Populating duplicate DAT");
            DuplicatesImpl(datFile, inputs, dupeData);
            DuplicatesDBImpl(datFile, inputs, dupeData);
            watch.Stop();

            return dupeData;
        }

        /// <summary>
        /// Create a duplicate data DatFile based on an input DatFile
        /// </summary>
        /// <param name="datFile">DatFile containing header and modifier information</param>
        private static DatFile DuplicatesInit(DatFile datFile)
        {
            var watch = new InternalStopwatch("Initializing duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, "datFile.All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, "datFile.All DATs");

            string post = " (Duplicates)";
            DatFile dupeData = Parser.CreateDatFile(datFile.Header, datFile.Modifiers);
            dupeData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, dupeData.Header.GetStringFieldValue(DatHeader.FileNameKey) + post);
            dupeData.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, dupeData.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey) + post);
            dupeData.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, dupeData.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey) + post);
            dupeData.ResetDictionary();

            watch.Stop();
            return dupeData;
        }

        /// <summary>
        /// Populate duplicates data from inputs
        /// </summary>
        /// <param name="datFile">Source DatFile to process</param>
        /// <param name="inputs">Set of input paths for naming</param>
        /// <param name="dupeData">Duplicate data DatFile</param>
        private static void DuplicatesImpl(DatFile datFile, List<ParentablePath> inputs, DatFile dupeData)
        {
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem> items = ItemDictionary.Merge(datFile.GetItemsForBucket(key));

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                // Loop through and add the items correctly
                foreach (DatItem item in items)
                {
#if NET20 || NET35
                    if ((item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.External) != 0)
#else
                    if (item.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.External))
#endif
                    {
                        if (item.Clone() is not DatItem newrom)
                            continue;

                        if (item.GetFieldValue<Source?>(DatItem.SourceKey) != null)
                            newrom.GetMachine()!.SetName(newrom.GetMachine()!.GetName() + $" ({Path.GetFileNameWithoutExtension(inputs[item.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].CurrentPath)})");

                        dupeData.AddItem(newrom, statsOnly: false);
                    }
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Populate duplicates data from inputs
        /// </summary>
        /// <param name="datFile">Source DatFile to process</param>
        /// <param name="inputs">Set of input paths for naming</param>
        /// <param name="dupeData">Duplicate data DatFile</param>
        private static void DuplicatesDBImpl(DatFile datFile, List<ParentablePath> inputs, DatFile dupeData)
        {
            // Get all current items, machines, and mappings
            var datItems = datFile.ItemsDB.GetItems();
            var machines = datFile.GetMachinesDB();
            var sources = datFile.ItemsDB.GetSources();

            // Create mappings from old index to new index
            var machineRemapping = new Dictionary<long, long>();
            var sourceRemapping = new Dictionary<long, long>();

            // Loop through and add all sources
            foreach (var source in sources)
            {
                long newSourceIndex = dupeData.AddSourceDB(source.Value);
                sourceRemapping[source.Key] = newSourceIndex;
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = dupeData.AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;
            }

            // Loop through and add the items
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datItems, Core.Globals.ParallelOptions, item =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Get the machine and source index for this item
                long machineIndex = datFile.GetMachineForItemDB(item.Key).Key;
                long sourceIndex = datFile.GetSourceForItemDB(item.Key).Key;

                // If the current item isn't an external duplicate
#if NET20 || NET35
                if ((item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.External) == 0)
#else
                if (!item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.External))
#endif
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                // Get the current source and machine
                var currentSource = sources[sourceIndex];
                string? currentMachineName = machines[machineIndex].GetName();
                var currentMachine = datFile.ItemsDB.GetMachine(currentMachineName);
                if (currentMachine.Value == null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                // Get the source-specific machine
                string? renamedMachineName = $"{currentMachineName} ({Path.GetFileNameWithoutExtension(inputs[currentSource!.Index].CurrentPath)})";
                var renamedMachine = datFile.ItemsDB.GetMachine(renamedMachineName);
                if (renamedMachine.Value == null)
                {
                    var newMachine = currentMachine.Value.Clone() as Machine;
                    newMachine!.SetName(renamedMachineName);
                    long newMachineIndex = dupeData.AddMachineDB(newMachine!);
                    renamedMachine = new KeyValuePair<long, Machine?>(newMachineIndex, newMachine);
                }

                dupeData.AddItemDB(item.Value, renamedMachine.Key, sourceRemapping[sourceIndex], statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion

        #region Individuals

        /// <summary>
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static List<DatFile> Individuals(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.ConvertAll(i => new ParentablePath(i));
            return Individuals(datFile, paths);
        }

        /// <summary>
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static List<DatFile> Individuals(DatFile datFile, List<ParentablePath> inputs)
        {
            // Create a set of DatData objects representing individual output files
            DatFile[] outDats = IndividualsInit(datFile, inputs);

            // Now, loop through the dictionary and populate the correct DATs
            var watch = new InternalStopwatch("Populating all individual DATs");
            IndividualsImpl(datFile, outDats);
            IndividualsDBImpl(datFile, outDats);
            watch.Stop();

            return [.. outDats];
        }

        /// <summary>
        /// Create an individual data DatFile based on an input DatFile
        /// </summary>
        /// <param name="datFile">DatFile containing header and modifier information</param>
        /// <param name="inputs">List of inputs to write out from</param>
        private static DatFile[] IndividualsInit(DatFile datFile, List<ParentablePath> inputs)
        {
            var watch = new InternalStopwatch("Initializing all individual DATs");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, "All DATs");

            // Loop through each of the inputs and get or create a new DatData object
            DatFile[] outDatsArray = new DatFile[inputs.Count];

#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.For(0, inputs.Count, Core.Globals.ParallelOptions, j =>
#elif NET40_OR_GREATER
            Parallel.For(0, inputs.Count, j =>
#else
            for (int j = 0; j < inputs.Count; j++)
#endif
            {
                string innerpost = $" ({j} - {inputs[j].GetNormalizedFileName(true)} Only)";
                DatFile diffData = Parser.CreateDatFile(datFile.Header, datFile.Modifiers);
                diffData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, diffData.Header.GetStringFieldValue(DatHeader.FileNameKey) + innerpost);
                diffData.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, diffData.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey) + innerpost);
                diffData.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, diffData.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey) + innerpost);
                diffData.ResetDictionary();
                outDatsArray[j] = diffData;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
            watch.Stop();
            return outDatsArray;
        }

        /// <summary>
        /// Populate individuals data from inputs
        /// </summary>
        /// <param name="datFile">Source DatFile to process</param>
        /// <param name="outDats">Individual data DatFiles</param>
        private static void IndividualsImpl(DatFile datFile, DatFile[] outDats)
        {
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem> items = ItemDictionary.Merge(datFile.GetItemsForBucket(key));

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                // Loop through and add the items correctly
                foreach (DatItem item in items)
                {
                    if (item.GetFieldValue<Source?>(DatItem.SourceKey) == null)
                        continue;

#if NET20 || NET35
                    if ((item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.Internal) != 0 || item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#else
                    if (item.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.Internal) || item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#endif
                        outDats[item.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].AddItem(item, statsOnly: false);
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Populate individuals data from inputs
        /// </summary>
        /// <param name="datFile">Source DatFile to process</param>
        /// <param name="outDats">Individual data DatFiles</param>
        private static void IndividualsDBImpl(DatFile datFile, DatFile[] outDats)
        {
            // Get all current items, machines, and mappings
            var datItems = datFile.ItemsDB.GetItems();
            var machines = datFile.GetMachinesDB();
            var sources = datFile.ItemsDB.GetSources();

            // Create mappings from old index to new index
            var machineRemapping = new Dictionary<long, long>();
            var sourceRemapping = new Dictionary<long, long>();

            // Loop through and add all sources
            foreach (var source in sources)
            {
                long newSourceIndex = outDats[0].AddSourceDB(source.Value);
                sourceRemapping[source.Key] = newSourceIndex;

                for (int i = 1; i < outDats.Length; i++)
                {
                    _ = outDats[i].AddSourceDB(source.Value);
                }
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = outDats[0].AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;

                for (int i = 1; i < outDats.Length; i++)
                {
                    _ = outDats[i].AddMachineDB(machine.Value);
                }
            }

            // Loop through and add the items
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datItems, Core.Globals.ParallelOptions, item =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Get the machine and source index for this item
                long machineIndex = datFile.GetMachineForItemDB(item.Key).Key;
                long sourceIndex = datFile.GetSourceForItemDB(item.Key).Key;

                // Get the source associated with the item
                var source = datFile.ItemsDB.GetSource(sourceIndex);
                if (source == null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

#if NET20 || NET35
                if ((item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.Internal) != 0 || item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#else
                if (item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.Internal) || item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#endif
                    outDats[source.Index].AddItemDB(item.Value, machineRemapping[machineIndex], sourceRemapping[sourceIndex], statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion

        #region NoDuplicates

        /// <summary>
        /// Output non-duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile NoDuplicates(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.ConvertAll(i => new ParentablePath(i));
            return NoDuplicates(datFile, paths);
        }

        /// <summary>
        /// Output non-duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile NoDuplicates(DatFile datFile, List<ParentablePath> inputs)
        {
            // Initialize no duplicate data
            DatFile outerDiffData = NoDuplicatesInit(datFile);

            // Now, loop through the dictionary and populate the correct DATs
            var watch = new InternalStopwatch("Populating no duplicate DAT");
            NoDuplicatesImpl(datFile, inputs, outerDiffData);
            NoDuplicatesDBImpl(datFile, inputs, outerDiffData);
            watch.Stop();

            return outerDiffData;
        }

        /// <summary>
        /// Create a no duplicate data DatFile based on an input DatFile
        /// </summary>
        /// <param name="datFile">DatFile containing header and modifier information</param>
        private static DatFile NoDuplicatesInit(DatFile datFile)
        {
            var watch = new InternalStopwatch("Initializing no duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, "All DATs");

            string post = " (No Duplicates)";
            DatFile outerDiffData = Parser.CreateDatFile(datFile.Header, datFile.Modifiers);
            outerDiffData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, outerDiffData.Header.GetStringFieldValue(DatHeader.FileNameKey) + post);
            outerDiffData.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, outerDiffData.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey) + post);
            outerDiffData.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, outerDiffData.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey) + post);
            outerDiffData.ResetDictionary();

            watch.Stop();
            return outerDiffData;
        }

        /// <summary>
        /// Populate no duplicates data from inputs
        /// </summary>
        /// <param name="datFile">Source DatFile to process</param>
        /// <param name="inputs">Set of input paths for naming</param>
        /// <param name="outerDiffData">No duplicate data DatFile</param>
        private static void NoDuplicatesImpl(DatFile datFile, List<ParentablePath> inputs, DatFile outerDiffData)
        {
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem> items = ItemDictionary.Merge(datFile.GetItemsForBucket(key));

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                // Loop through and add the items correctly
                foreach (DatItem item in items)
                {
#if NET20 || NET35
                    if ((item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.Internal) != 0 || item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#else
                    if (item.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.Internal) || item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#endif
                    {
                        if (item.Clone() is not DatItem newrom || newrom.GetFieldValue<Source?>(DatItem.SourceKey) == null)
                            continue;

                        newrom.GetMachine()!.SetName(newrom.GetMachine()!.GetName() + $" ({Path.GetFileNameWithoutExtension(inputs[newrom.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].CurrentPath)})");
                        outerDiffData.AddItem(newrom, statsOnly: false);
                    }
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        /// <summary>
        /// Populate no duplicates data from inputs
        /// </summary>
        /// <param name="datFile">Source DatFile to process</param>
        /// <param name="inputs">Set of input paths for naming</param>
        /// <param name="outerDiffData">No duplicate data DatFile</param>
        private static void NoDuplicatesDBImpl(DatFile datFile, List<ParentablePath> inputs, DatFile outerDiffData)
        {
            // Get all current items, machines, and mappings
            var datItems = datFile.ItemsDB.GetItems();
            var machines = datFile.GetMachinesDB();
            var sources = datFile.ItemsDB.GetSources();

            // Create mappings from old index to new index
            var machineRemapping = new Dictionary<long, long>();
            var sourceRemapping = new Dictionary<long, long>();

            // Loop through and add all sources
            foreach (var source in sources)
            {
                long newSourceIndex = outerDiffData.AddSourceDB(source.Value);
                sourceRemapping[source.Key] = newSourceIndex;
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = outerDiffData.AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;
            }

            // Loop through and add the items
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(datItems, Core.Globals.ParallelOptions, item =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Get the machine and source index for this item
                long machineIndex = datFile.GetMachineForItemDB(item.Key).Key;
                long sourceIndex = datFile.GetSourceForItemDB(item.Key).Key;

                // If the current item isn't a duplicate
#if NET20 || NET35
                if ((item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.Internal) == 0 && item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) != 0x00)
#else
                if (!item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.Internal) && item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) != 0x00)
#endif
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                // Get the current source and machine
                var currentSource = sources[sourceIndex];
                string? currentMachineName = machines[machineIndex].GetName();
                var currentMachine = datFile.ItemsDB.GetMachine(currentMachineName);
                if (currentMachine.Value == null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                // Get the source-specific machine
                string? renamedMachineName = $"{currentMachineName} ({Path.GetFileNameWithoutExtension(inputs[currentSource!.Index].CurrentPath)})";
                var renamedMachine = datFile.ItemsDB.GetMachine(renamedMachineName);
                if (renamedMachine.Value == null)
                {
                    var newMachine = currentMachine.Value.Clone() as Machine;
                    newMachine!.SetName(renamedMachineName);
                    long newMachineIndex = outerDiffData.AddMachineDB(newMachine);
                    renamedMachine = new KeyValuePair<long, Machine?>(newMachineIndex, newMachine);
                }

                outerDiffData.AddItemDB(item.Value, renamedMachine.Key, sourceRemapping[sourceIndex], statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            });
#else
            }
#endif
        }

        #endregion
    }
}
