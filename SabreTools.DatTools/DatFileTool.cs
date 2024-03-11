using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NET40_OR_GREATER || NETCOREAPP
using System.Threading.Tasks;
#endif
using SabreTools.Core;
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.Filtering;
using SabreTools.IO;
using SabreTools.Logging;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Helper methods for updating and converting DatFiles
    /// </summary>
    public static class DatFileTool
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger logger = new();

        #endregion

        /// <summary>
        /// Apply SuperDAT naming logic to a merged DatFile
        /// </summary>
        /// <param name="datFile">Current DatFile object to run operations on</param>
        /// <param name="inputs">List of inputs to use for renaming</param>
        public static void ApplySuperDAT(DatFile datFile, List<ParentablePath> inputs)
        {
            List<string> keys = [.. datFile.Items.Keys];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(keys, key =>
#else
            foreach (var key in keys)
#endif
            {
                ConcurrentList<DatItem>? items = datFile.Items[key];
                if (items == null)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                ConcurrentList<DatItem> newItems = [];
                foreach (DatItem item in items)
                {
                    DatItem newItem = item;
                    if (newItem.GetFieldValue<Source?>(DatItem.SourceKey) == null)
                        continue;

                    string filename = inputs[newItem.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].CurrentPath;
                    string? rootpath = inputs[newItem.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].ParentPath;

                    if (!string.IsNullOrEmpty(rootpath)
#if NETFRAMEWORK
                        && !rootpath!.EndsWith(Path.DirectorySeparatorChar.ToString())
                        && !rootpath!.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
#else
                        && !rootpath.EndsWith(Path.DirectorySeparatorChar)
                        && !rootpath.EndsWith(Path.AltDirectorySeparatorChar))
#endif
                    {
                        rootpath += Path.DirectorySeparatorChar.ToString();
                    }

                    filename = filename.Remove(0, rootpath?.Length ?? 0);
                    newItem.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
                        + Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
                        + newItem.GetFieldValue<Machine>(DatItem.MachineKey)!.GetFieldValue<string?>(Models.Metadata.Machine.NameKey));

                    newItems.Add(newItem);
                }

                datFile.Items.Remove(key);
                datFile.Items.AddRange(key, newItems);
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
            InternalStopwatch watch = new($"Replacing items in '{intDat.Header.GetFieldValue<string?>(DatHeader.FileNameKey)}' from the base DAT");

            // If we are matching based on DatItem fields of any sort
            if (itemFieldNames.Any())
            {
                // For comparison's sake, we want to use CRC as the base bucketing
                datFile.Items.BucketBy(ItemKey.CRC, DedupeType.Full);
                intDat.Items.BucketBy(ItemKey.CRC, DedupeType.None);

                // Then we do a hashwise comparison against the base DAT
#if NET452_OR_GREATER || NETCOREAPP
                Parallel.ForEach(intDat.Items.Keys, Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
                Parallel.ForEach(intDat.Items.Keys, key =>
#else
                foreach (var key in intDat.Items.Keys)
#endif
                {
                    ConcurrentList<DatItem>? datItems = intDat.Items[key];
                    if (datItems == null)
#if NET40_OR_GREATER || NETCOREAPP
                        return;
#else
                        continue;
#endif

                    ConcurrentList<DatItem> newDatItems = [];
                    foreach (DatItem datItem in datItems)
                    {
                        ConcurrentList<DatItem> dupes = datFile.Items.GetDuplicates(datItem, sorted: true);
                        if (datItem.Clone() is not DatItem newDatItem)
                            continue;

                        // Replace fields from the first duplicate, if we have one
                        if (dupes.Count > 0)
                            Replacer.ReplaceFields(newDatItem, dupes.First(), itemFieldNames);

                        newDatItems.Add(newDatItem);
                    }

                    // Now add the new list to the key
                    intDat.Items.Remove(key);
                    intDat.Items.AddRange(key, newDatItems);
#if NET40_OR_GREATER || NETCOREAPP
                });
#else
                }
#endif
            }

            // If we are matching based on Machine fields of any sort
            if (machineFieldNames.Any())
            {
                // For comparison's sake, we want to use Machine Name as the base bucketing
                datFile.Items.BucketBy(ItemKey.Machine, DedupeType.Full);
                intDat.Items.BucketBy(ItemKey.Machine, DedupeType.None);

                // Then we do a namewise comparison against the base DAT
#if NET452_OR_GREATER || NETCOREAPP
                Parallel.ForEach(intDat.Items.Keys, Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
                Parallel.ForEach(intDat.Items.Keys, key =>
#else
                foreach (var key in intDat.Items.Keys)
#endif
                {
                    ConcurrentList<DatItem>? datItems = intDat.Items[key];
                    if (datItems == null)
#if NET40_OR_GREATER || NETCOREAPP
                        return;
#else
                        continue;
#endif

                    ConcurrentList<DatItem> newDatItems = [];
                    foreach (DatItem datItem in datItems)
                    {
                        if (datItem.Clone() is not DatItem newDatItem)
                            continue;

                        if (!datFile.Items.TryGetValue(key, out var list) || list == null)
                            continue;

                        if (datFile.Items.ContainsKey(key) && list.Count > 0)
                            Replacer.ReplaceFields(newDatItem.GetFieldValue<Machine>(DatItem.MachineKey)!, list[index: 0].GetFieldValue<Machine>(DatItem.MachineKey)!, machineFieldNames, onlySame);

                        newDatItems.Add(newDatItem);
                    }

                    // Now add the new list to the key
                    intDat.Items.Remove(key);
                    intDat.Items.AddRange(key, newDatItems);
#if NET40_OR_GREATER || NETCOREAPP
                });
#else
                }
#endif
            }

            watch.Stop();
        }

        /// <summary>
        /// Output diffs against a base set represented by the current DAT
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="intDat">DatFile to replace the values in</param>
        /// <param name="useGames">True to diff using games, false to use hashes</param>
        public static void DiffAgainst(DatFile datFile, DatFile intDat, bool useGames)
        {
            // For comparison's sake, we want to use a base ordering
            if (useGames)
                datFile.Items.BucketBy(ItemKey.Machine, DedupeType.None);
            else
                datFile.Items.BucketBy(ItemKey.CRC, DedupeType.None);

            InternalStopwatch watch = new($"Comparing '{intDat.Header.GetFieldValue<string?>(DatHeader.FileNameKey)}' to base DAT");

            // For comparison's sake, we want to a the base bucketing
            if (useGames)
                intDat.Items.BucketBy(ItemKey.Machine, DedupeType.None);
            else
                intDat.Items.BucketBy(ItemKey.CRC, DedupeType.Full);

            // Then we compare against the base DAT
            List<string> keys = [.. intDat.Items.Keys];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(keys, key =>
#else
            foreach (var key in keys)
#endif
            {
                // Game Against uses game names
                if (useGames)
                {
                    // If the key is null, keep it
                    if (!intDat.Items.TryGetValue(key, out var intList) || intList == null)
#if NET40_OR_GREATER || NETCOREAPP
                        return;
#else
                        continue;
#endif

                    // If the base DAT doesn't contain the key, keep it
                    if (!datFile.Items.TryGetValue(key, out var list) || list == null)
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
                        intDat.Items.Remove(key);
                }

                // Standard Against uses hashes
                else
                {
                    ConcurrentList<DatItem>? datItems = intDat.Items[key];
                    if (datItems == null)
#if NET40_OR_GREATER || NETCOREAPP
                        return;
#else
                        continue;
#endif

                    ConcurrentList<DatItem> keepDatItems = [];
                    foreach (DatItem datItem in datItems)
                    {
                        if (!datFile.Items.HasDuplicates(datItem, true))
                            keepDatItems.Add(datItem);
                    }

                    // Now add the new list to the key
                    intDat.Items.Remove(key);
                    intDat.Items.AddRange(key, keepDatItems);
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
        public static List<DatFile> DiffCascade(DatFile datFile, List<DatHeader> datHeaders)
        {
            // Create a list of DatData objects representing output files
            List<DatFile> outDats = [];

            // Ensure the current DatFile is sorted optimally
            datFile.Items.BucketBy(ItemKey.CRC, DedupeType.None);

            // Loop through each of the inputs and get or create a new DatData object
            InternalStopwatch watch = new("Initializing and filling all output DATs");

            // Create the DatFiles from the set of headers
            DatFile[] outDatsArray = new DatFile[datHeaders.Count];
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, datHeaders.Count, Globals.ParallelOptions, j =>
#elif NET40_OR_GREATER
            Parallel.For(0, datHeaders.Count, j =>
#else
            for (int j = 0; j < datHeaders.Count; j++)
#endif
            {
                DatFile diffData = DatFile.Create(datHeaders[j]);
                diffData.Items = [];
                FillWithSourceIndex(datFile, diffData, j);
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
        public static DatFile DiffDuplicates(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            return DiffDuplicates(datFile, paths);
        }

        /// <summary>
        /// Output duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile DiffDuplicates(DatFile datFile, List<ParentablePath> inputs)
        {
            InternalStopwatch watch = new("Initializing duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetFieldValue<string?>(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetFieldValue<string?>(Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, "datFile.All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetFieldValue<string?>(Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, "datFile.All DATs");

            string post = " (Duplicates)";
            DatFile dupeData = DatFile.Create(datFile.Header);
            dupeData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, dupeData.Header.GetFieldValue<string?>(DatHeader.FileNameKey) + post);
            dupeData.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, dupeData.Header.GetFieldValue<string?>(Models.Metadata.Header.NameKey) + post);
            dupeData.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, dupeData.Header.GetFieldValue<string?>(Models.Metadata.Header.DescriptionKey) + post);
            dupeData.Items = [];

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating duplicate DAT");

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datFile.Items.Keys, Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.Keys, key =>
#else
            foreach (var key in datFile.Items.Keys)
#endif
            {
                ConcurrentList<DatItem> items = DatItem.Merge(datFile.Items[key]);

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
#if NETFRAMEWORK
                    if ((item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.External) != 0)
#else
                    if (item.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.External))
#endif
                    {
                        if (item.Clone() is not DatItem newrom)
                            continue;

                        if (item.GetFieldValue<Source?>(DatItem.SourceKey) != null)
                            newrom.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, newrom.GetFieldValue<Machine>(DatItem.MachineKey)!.GetFieldValue<string?>(Models.Metadata.Machine.NameKey) + $" ({Path.GetFileNameWithoutExtension(inputs[item.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].CurrentPath)})");

                        dupeData.Items.Add(key, newrom);
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
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static List<DatFile> DiffIndividuals(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            return DiffIndividuals(datFile, paths);
        }

        /// <summary>
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static List<DatFile> DiffIndividuals(DatFile datFile, List<ParentablePath> inputs)
        {
            InternalStopwatch watch = new("Initializing all individual DATs");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetFieldValue<string?>(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetFieldValue<string?>(Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetFieldValue<string?>(Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, "All DATs");

            // Loop through each of the inputs and get or create a new DatData object
            DatFile[] outDatsArray = new DatFile[inputs.Count];

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
#elif NET40_OR_GREATER
            Parallel.For(0, inputs.Count, j =>
#else
            for (int j = 0; j < inputs.Count; j++)
#endif
            {
                string innerpost = $" ({j} - {inputs[j].GetNormalizedFileName(true)} Only)";
                DatFile diffData = DatFile.Create(datFile.Header);
                diffData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, diffData.Header.GetFieldValue<string?>(DatHeader.FileNameKey) + innerpost);
                diffData.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, diffData.Header.GetFieldValue<string?>(Models.Metadata.Header.NameKey) + innerpost);
                diffData.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, diffData.Header.GetFieldValue<string?>(Models.Metadata.Header.DescriptionKey) + innerpost);
                diffData.Items = [];
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
            Parallel.ForEach(datFile.Items.Keys, Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.Keys, key =>
#else
            foreach (var key in datFile.Items.Keys)
#endif
            {
                ConcurrentList<DatItem> items = DatItem.Merge(datFile.Items[key]);

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

#if NETFRAMEWORK
                    if ((item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.Internal) != 0 || item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#else
                    if (item.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.Internal) || item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#endif
                        outDats[item.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].Items.Add(key, item);
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
        /// Output non-duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile DiffNoDuplicates(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            return DiffNoDuplicates(datFile, paths);
        }

        /// <summary>
        /// Output non-duplicate item diff
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">List of inputs to write out from</param>
        public static DatFile DiffNoDuplicates(DatFile datFile, List<ParentablePath> inputs)
        {
            InternalStopwatch watch = new("Initializing no duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrEmpty(datFile.Header.GetFieldValue<string?>(DatHeader.FileNameKey)))
                datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetFieldValue<string?>(Models.Metadata.Header.NameKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, "All DATs");

            if (string.IsNullOrEmpty(datFile.Header.GetFieldValue<string?>(Models.Metadata.Header.DescriptionKey)))
                datFile.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, "All DATs");

            string post = " (No Duplicates)";
            DatFile outerDiffData = DatFile.Create(datFile.Header);
            outerDiffData.Header.SetFieldValue<string?>(DatHeader.FileNameKey, outerDiffData.Header.GetFieldValue<string?>(DatHeader.FileNameKey) + post);
            outerDiffData.Header.SetFieldValue<string?>(Models.Metadata.Header.NameKey, outerDiffData.Header.GetFieldValue<string?>(Models.Metadata.Header.NameKey) + post);
            outerDiffData.Header.SetFieldValue<string?>(Models.Metadata.Header.DescriptionKey, outerDiffData.Header.GetFieldValue<string?>(Models.Metadata.Header.DescriptionKey) + post);
            outerDiffData.Items = [];

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating no duplicate DAT");

#if NET452_OR_GREATER || NETCOREAPP
            Parallel.ForEach(datFile.Items.Keys, Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.Keys, key =>
#else
            foreach (var key in datFile.Items.Keys)
#endif
            {
                ConcurrentList<DatItem> items = DatItem.Merge(datFile.Items[key]);

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
#if NETFRAMEWORK
                    if ((item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) & DupeType.Internal) != 0 || item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#else
                    if (item.GetFieldValue<DupeType>(DatItem.DupeTypeKey).HasFlag(DupeType.Internal) || item.GetFieldValue<DupeType>(DatItem.DupeTypeKey) == 0x00)
#endif
                    {
                        if (item.Clone() is not DatItem newrom || newrom.GetFieldValue<Source?>(DatItem.SourceKey) == null)
                            continue;

                        newrom.GetFieldValue<Machine>(DatItem.MachineKey)!.SetFieldValue<string?>(Models.Metadata.Machine.NameKey, newrom.GetFieldValue<Machine>(DatItem.MachineKey)!.GetFieldValue<string?>(Models.Metadata.Machine.NameKey) + $" ({Path.GetFileNameWithoutExtension(inputs[newrom.GetFieldValue<Source?>(DatItem.SourceKey)!.Index].CurrentPath)})");
                        outerDiffData.Items.Add(key, newrom);
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
        /// Populate from multiple paths while returning the invividual headers
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">Paths to DATs to parse</param>
        /// <returns>List of DatHeader objects representing headers</returns>
        public static List<DatHeader> PopulateUserData(DatFile datFile, List<string> inputs)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            return PopulateUserData(datFile, paths);
        }

        /// <summary>
        /// Populate from multiple paths while returning the invividual headers
        /// </summary>
        /// <param name="datFile">Current DatFile object to use for updating</param>
        /// <param name="inputs">Paths to DATs to parse</param>
        /// <returns>List of DatHeader objects representing headers</returns>
        public static List<DatHeader> PopulateUserData(DatFile datFile, List<ParentablePath> inputs)
        {
            DatFile[] datFiles = new DatFile[inputs.Count];
            InternalStopwatch watch = new("Processing individual DATs");

            // Parse all of the DATs into their own DatFiles in the array
#if NET452_OR_GREATER || NETCOREAPP
            Parallel.For(0, inputs.Count, Globals.ParallelOptions, i =>
#elif NET40_OR_GREATER
            Parallel.For(0, inputs.Count, i =>
#else
            for (int i = 0; i < inputs.Count; i++)
#endif
            {
                var input = inputs[i];
                logger.User($"Adding DAT: {input.CurrentPath}");
                datFiles[i] = DatFile.Create(datFile.Header.CloneFiltering());
                Parser.ParseInto(datFiles[i], input, i, keep: true);
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif

            watch.Stop();

            watch.Start("Populating internal DAT");
            for (int i = 0; i < inputs.Count; i++)
            {
                AddFromExisting(datFile, datFiles[i], true);
            }

            watch.Stop();

            return datFiles.Select(d => d.Header).ToList();
        }

        /// <summary>
        /// Add items from another DatFile to the existing DatFile
        /// </summary>
        /// <param name="addTo">DatFile to add to</param>
        /// <param name="addFrom">DatFile to add from</param>
        /// <param name="delete">If items should be deleted from the source DatFile</param>
        private static void AddFromExisting(DatFile addTo, DatFile addFrom, bool delete = false)
        {
            // Get the list of keys from the DAT
            var keys = addFrom.Items.Keys.ToList();
            foreach (string key in keys)
            {
                // Add everything from the key to the internal DAT
                addTo.Items.AddRange(key, addFrom.Items[key]);

                // Now remove the key from the source DAT
                if (delete)
                    addFrom.Items.Remove(key);
            }

            // Now remove the file dictionary from the source DAT
            if (delete)
                addFrom.Items = [];
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
            Parallel.ForEach(datFile.Items.Keys, Globals.ParallelOptions, key =>
#elif NET40_OR_GREATER
            Parallel.ForEach(datFile.Items.Keys, key =>
#else
            foreach (var key in datFile.Items.Keys)
#endif
            {
                ConcurrentList<DatItem> items = DatItem.Merge(datFile.Items[key]);

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
#if NET40_OR_GREATER || NETCOREAPP
                    return;
#else
                    continue;
#endif

                foreach (DatItem item in items)
                {
                    if (item.GetFieldValue<Source?>(DatItem.SourceKey) != null && item.GetFieldValue<Source?>(DatItem.SourceKey)!.Index == index)
                        indexDat.Items.Add(key, item);
                }
#if NET40_OR_GREATER || NETCOREAPP
            });
#else
            }
#endif
        }
    }
}
