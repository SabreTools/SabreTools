using System.Collections.Generic;
using System.IO;
#if NET40_OR_GREATER || NETCOREAPP
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

            // Then we compare against the base DAT
#if NET452_OR_GREATER || NETCOREAPP
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
#if NET40_OR_GREATER || NETCOREAPP
                        return;
#else
                        continue;
#endif

                    // If the base DAT doesn't contain the key, keep it
                    var list = datFile.GetItemsForBucket(key);
                    if (list.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP
                        return;
#else
                        continue;
#endif

                    // If the number of items is different, then keep it
                    if (list.Count != intList.Count)
#if NET40_OR_GREATER || NETCOREAPP
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
                    List<DatItem>? datItems = intDat.GetItemsForBucket(key);
                    if (datItems == null)
#if NET40_OR_GREATER || NETCOREAPP
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
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            watch.Stop();
        }

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
            datFile.BucketBy(ItemKey.CRC);

            // Loop through each of the inputs and get or create a new DatData object
            InternalStopwatch watch = new("Initializing and filling all output DATs");

            // Create the DatFiles from the set of headers
            DatFile[] outDatsArray = new DatFile[datHeaders.Count];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, datHeaders.Count, Core.Globals.ParallelOptions, j =>
#elif NET40_OR_GREATER
            Parallel.For(0, datHeaders.Count, j =>
#else
            for (int j = 0; j < datHeaders.Count; j++)
#endif
            {
                DatFile diffData = DatFileTool.CreateDatFile(datHeaders[j], new DatModifiers());
                diffData.ResetDictionary();
                FillWithSourceIndex(datFile, diffData, j);
                FillWithSourceIndexDB(datFile, diffData, j);
                outDatsArray[j] = diffData;
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            outDats = [.. outDatsArray];
            watch.Stop();

            return outDats;
        }

        /// <summary>
        /// Output duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile Duplicates(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.ConvertAll(i => new ParentablePath(i));
            return Duplicates(datFile, paths);
            //return DuplicatesDB(datFile, paths);
        }

        /// <summary>
        /// Output duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile Duplicates(DatFile datFile, List<ParentablePath> inputs)
        {
            InternalStopwatch watch = new("Initializing duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, "datFile.All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, "datFile.All DATs");

            string post = " (Duplicates)";
            DatFile dupeData = DatFileTool.CreateDatFile(datFile.Header, datFile.Modifiers);
            dupeData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, dupeData.Header.GetStringFieldValue(DatHeader.FileNameKey) + post);
            dupeData.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, dupeData.Header.GetStringFieldValue(Models.Metadata.Header.NameKey) + post);
            dupeData.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, dupeData.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey) + post);
            dupeData.ResetDictionary();

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating duplicate DAT");

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datFile.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem> items = DatFileTool.Merge(datFile.GetItemsForBucket(key));

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP
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
                            newrom.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, newrom.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey) + $" ({Path.GetFileNameWithoutExtension(inputs[item.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].CurrentPath)})");

                        dupeData.AddItem(newrom, statsOnly: false);
                    }
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            watch.Stop();

            return dupeData;
        }

        /// <summary>
        /// Output duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile DuplicatesDB(DatFile datFile, List<ParentablePath> inputs)
        {
            var watch = new InternalStopwatch("Initializing duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, "datFile.All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, "datFile.All DATs");

            string post = " (Duplicates)";
            DatFile dupeData = DatFileTool.CreateDatFile(datFile.Header, datFile.Modifiers);
            dupeData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, dupeData.Header.GetStringFieldValue(DatHeader.FileNameKey) + post);
            dupeData.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, dupeData.Header.GetStringFieldValue(Models.Metadata.Header.NameKey) + post);
            dupeData.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, dupeData.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey) + post);
            dupeData.ResetDictionary();

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating duplicate DAT");

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
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datItems, Core.Globals.ParallelOptions, item =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Get the machine and source index for this item
                long machineIndex = datFile.ItemsDB.GetMachineForItem(item.Key).Key;
                long sourceIndex = datFile.ItemsDB.GetSourceForItem(item.Key).Key;

                // If the current item isn't an external duplicate
#if NET20 || NET35
                if ((item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.External) == 0)
#else
                if (!item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.External))
#endif
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                // Get the current source and machine
                var currentSource = sources[sourceIndex];
                string? currentMachineName = machines[machineIndex].GetStringFieldValue(Models.Metadata.Machine.NameKey);
                var currentMachine = datFile.ItemsDB.GetMachine(currentMachineName);
                if (currentMachine.Value == null)
#if NET40_OR_GREATER || NETCOREAPP
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
                    newMachine!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, renamedMachineName);
                    long newMachineIndex = dupeData.AddMachineDB(newMachine!);
                    renamedMachine = new KeyValuePair<long, Machine?>(newMachineIndex, newMachine);
                }

                dupeData.AddItemDB(item.Value, renamedMachine.Key, sourceRemapping[sourceIndex], statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            watch.Stop();

            return dupeData;
        }

        /// <summary>
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static List<DatFile> Individuals(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.ConvertAll(i => new ParentablePath(i));
            return Individuals(datFile, paths);
            //return IndividualsDB(datFile, paths);
        }

        /// <summary>
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static List<DatFile> Individuals(DatFile datFile, List<ParentablePath> inputs)
        {
            InternalStopwatch watch = new("Initializing all individual DATs");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, "All DATs");

            // Loop through each of the inputs and get or create a new DatData object
            DatFile[] outDatsArray = new DatFile[inputs.Count];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, inputs.Count, Core.Globals.ParallelOptions, j =>
#elif NET40_OR_GREATER
            Parallel.For(0, inputs.Count, j =>
#else
            for (int j = 0; j < inputs.Count; j++)
#endif
            {
                string innerpost = $" ({j} - {inputs[j].GetNormalizedFileName(true)} Only)";
                DatFile diffData = DatFileTool.CreateDatFile(datFile.Header, datFile.Modifiers);
                diffData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, diffData.Header.GetStringFieldValue(DatHeader.FileNameKey) + innerpost);
                diffData.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, diffData.Header.GetStringFieldValue(Models.Metadata.Header.NameKey) + innerpost);
                diffData.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, diffData.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey) + innerpost);
                diffData.ResetDictionary();
                outDatsArray[j] = diffData;
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            // Create a list of DatData objects representing individual output files
            List<DatFile> outDats = [.. outDatsArray];

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating all individual DATs");

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datFile.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem> items = DatFileTool.Merge(datFile.GetItemsForBucket(key));

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP
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
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            watch.Stop();

            return [.. outDats];
        }

        /// <summary>
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static List<DatFile> IndividualsDB(DatFile datFile, List<ParentablePath> inputs)
        {
            InternalStopwatch watch = new("Initializing all individual DATs");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, "All DATs");

            // Loop through each of the inputs and get or create a new DatData object
            DatFile[] outDatsArray = new DatFile[inputs.Count];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, inputs.Count, Core.Globals.ParallelOptions, j =>
#elif NET40_OR_GREATER
            Parallel.For(0, inputs.Count, j =>
#else
            for (int j = 0; j < inputs.Count; j++)
#endif
            {
                string innerpost = $" ({j} - {inputs[j].GetNormalizedFileName(true)} Only)";
                DatFile diffData = DatFileTool.CreateDatFile(datFile.Header, datFile.Modifiers);
                diffData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, diffData.Header.GetStringFieldValue(DatHeader.FileNameKey) + innerpost);
                diffData.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, diffData.Header.GetStringFieldValue(Models.Metadata.Header.NameKey) + innerpost);
                diffData.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, diffData.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey) + innerpost);
                diffData.ResetDictionary();
                outDatsArray[j] = diffData;
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            // Create a list of DatData objects representing individual output files
            List<DatFile> outDats = [.. outDatsArray];

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating all individual DATs");

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

                for (int i = 1; i < outDats.Count; i++)
                {
                    _ = outDats[i].AddSourceDB(source.Value);
                }
            }

            // Loop through and add all machines
            foreach (var machine in machines)
            {
                long newMachineIndex = outDats[0].AddMachineDB(machine.Value);
                machineRemapping[machine.Key] = newMachineIndex;

                for (int i = 1; i < outDats.Count; i++)
                {
                    _ = outDats[i].AddMachineDB(machine.Value);
                }
            }

            // Loop through and add the items
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datItems, Core.Globals.ParallelOptions, item =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Get the machine and source index for this item
                long machineIndex = datFile.ItemsDB.GetMachineForItem(item.Key).Key;
                long sourceIndex = datFile.ItemsDB.GetSourceForItem(item.Key).Key;

                // Get the source associated with the item
                var source = datFile.ItemsDB.GetSource(sourceIndex);
                if (source == null)
#if NET40_OR_GREATER || NETCOREAPP
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
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            watch.Stop();

            return [.. outDats];
        }

        /// <summary>
        /// Output non-duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile NoDuplicates(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.ConvertAll(i => new ParentablePath(i));
            return NoDuplicates(datFile, paths);
            //return NoDuplicatesDB(datFile, paths);
        }

        /// <summary>
        /// Output non-duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile NoDuplicates(DatFile datFile, List<ParentablePath> inputs)
        {
            InternalStopwatch watch = new("Initializing no duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, "All DATs");

            string post = " (No Duplicates)";
            DatFile outerDiffData = DatFileTool.CreateDatFile(datFile.Header, datFile.Modifiers);
            outerDiffData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, outerDiffData.Header.GetStringFieldValue(DatHeader.FileNameKey) + post);
            outerDiffData.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, outerDiffData.Header.GetStringFieldValue(Models.Metadata.Header.NameKey) + post);
            outerDiffData.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, outerDiffData.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey) + post);
            outerDiffData.ResetDictionary();

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating no duplicate DAT");

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datFile.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem> items = DatFileTool.Merge(datFile.GetItemsForBucket(key));

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP
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

                        newrom.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, newrom.GetFieldValue<Machine>(DatItem.MachineKey)!.GetStringFieldValue(Models.Metadata.Machine.NameKey) + $" ({Path.GetFileNameWithoutExtension(inputs[newrom.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].CurrentPath)})");
                        outerDiffData.AddItem(newrom, statsOnly: false);
                    }
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            watch.Stop();

            return outerDiffData;
        }

        /// <summary>
        /// Output non-duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile NoDuplicatesDB(DatFile datFile, List<ParentablePath> inputs)
        {
            var watch = new InternalStopwatch("Initializing no duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, "All DATs");

            string post = " (No Duplicates)";
            DatFile outerDiffData = DatFileTool.CreateDatFile(datFile.Header, datFile.Modifiers);
            outerDiffData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, outerDiffData.Header.GetStringFieldValue(DatHeader.FileNameKey) + post);
            outerDiffData.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, outerDiffData.Header.GetStringFieldValue(Models.Metadata.Header.NameKey) + post);
            outerDiffData.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, outerDiffData.Header.GetStringFieldValue(Models.Metadata.Header.DescriptionKey) + post);
            outerDiffData.ResetDictionary();

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating no duplicate DAT");

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
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datItems, Core.Globals.ParallelOptions, item =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Get the machine and source index for this item
                long machineIndex = datFile.ItemsDB.GetMachineForItem(item.Key).Key;
                long sourceIndex = datFile.ItemsDB.GetSourceForItem(item.Key).Key;

                // If the current item isn't a duplicate
#if NET20 || NET35
                if ((item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.Internal) == 0 && item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) != 0x00)
#else
                if (!item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.Internal) && item.Value.GetFieldValue<DupeType>(DatItem.DupeTypeKey) != 0x00)
#endif
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                // Get the current source and machine
                var currentSource = sources[sourceIndex];
                string? currentMachineName = machines[machineIndex].GetStringFieldValue(Models.Metadata.Machine.NameKey);
                var currentMachine = datFile.ItemsDB.GetMachine(currentMachineName);
                if (currentMachine.Value == null)
#if NET40_OR_GREATER || NETCOREAPP
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
                    newMachine!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, renamedMachineName);
                    long newMachineIndex = outerDiffData.AddMachineDB(newMachine);
                    renamedMachine = new KeyValuePair<long, Machine?>(newMachineIndex, newMachine);
                }

                outerDiffData.AddItemDB(item.Value, renamedMachine.Key, sourceRemapping[sourceIndex], statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            watch.Stop();

            return outerDiffData;
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
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datFile.Items.SortedKeys, Core.Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.SortedKeys, key =>
#else
            foreach (var key in datFile.Items.SortedKeys)
#endif
            {
                List<DatItem> items = DatFileTool.Merge(datFile.GetItemsForBucket(key));

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP
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
#if NET40_OR_GREATER || NETCOREAPP
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
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datItems, Core.Globals.ParallelOptions, item =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datItems, item =>
#else
            foreach (var item in datItems)
#endif
            {
                // Get the machine and source index for this item
                long machineIndex = datFile.ItemsDB.GetMachineForItem(item.Key).Key;
                long sourceIndex = datFile.ItemsDB.GetSourceForItem(item.Key).Key;

                // Get the source associated with the item
                var source = datFile.ItemsDB.GetSource(sourceIndex);

                if (source != null && source.Index == index)
                    indexDat.AddItemDB(item.Value, machineRemapping[machineIndex], sourceRemapping[sourceIndex], statsOnly: false);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }
    }
}