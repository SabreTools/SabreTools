using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Filtering;
using SabreTools.Library.IO;
using SabreTools.Library.Reports;
using SabreTools.Library.Skippers;
using SabreTools.Library.Tools;
using NaturalSort;
using Newtonsoft.Json;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents a format-agnostic DAT
    /// </summary>
    [JsonObject("datfile")]
    public abstract class DatFile
    {
        #region Fields

        /// <summary>
        /// Header values
        /// </summary>
        [JsonProperty("header")]
        public DatHeader Header { get; set; } = new DatHeader();

        /// <summary>
        /// DatItems and related statistics
        /// </summary>
        [JsonProperty("items")]
        public ItemDictionary Items { get; set; } = new ItemDictionary();

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new DatFile from an existing one
        /// </summary>
        /// <param name="datFile">DatFile to get the values from</param>
        public DatFile(DatFile datFile)
        {
            if (datFile != null)
            {
                Header = datFile.Header;
                Items = datFile.Items;
            }
        }

        /// <summary>
        /// Create a specific type of DatFile to be used based on a format and a base DAT
        /// </summary>
        /// <param name="datFormat">Format of the DAT to be created</param>
        /// <param name="baseDat">DatFile containing the information to use in specific operations</param>
        /// <returns>DatFile of the specific internal type that corresponds to the inputs</returns>
        public static DatFile Create(DatFormat? datFormat = null, DatFile baseDat = null)
        {
            switch (datFormat)
            {
                case DatFormat.AttractMode:
                    return new AttractMode(baseDat);

                case DatFormat.ClrMamePro:
                    return new ClrMamePro(baseDat);

                case DatFormat.CSV:
                    return new SeparatedValue(baseDat, ',');

                case DatFormat.DOSCenter:
                    return new DosCenter(baseDat);

                case DatFormat.EverdriveSMDB:
                    return new EverdriveSMDB(baseDat);

                case DatFormat.Json:
                    return new Json(baseDat);

                case DatFormat.Listrom:
                    return new Listrom(baseDat);

                case DatFormat.Listxml:
                    return new Listxml(baseDat);

                case DatFormat.Logiqx:
                    return new Logiqx(baseDat, false);

                case DatFormat.LogiqxDeprecated:
                    return new Logiqx(baseDat, true);

                case DatFormat.MissFile:
                    return new Missfile(baseDat);

                case DatFormat.OfflineList:
                    return new OfflineList(baseDat);

                case DatFormat.OpenMSX:
                    return new OpenMSX(baseDat);

                case DatFormat.RedumpMD5:
                    return new Hashfile(baseDat, Hash.MD5);

#if NET_FRAMEWORK
                case DatFormat.RedumpRIPEMD160:
                    return new Hashfile(baseDat, Hash.RIPEMD160);
#endif

                case DatFormat.RedumpSFV:
                    return new Hashfile(baseDat, Hash.CRC);

                case DatFormat.RedumpSHA1:
                    return new Hashfile(baseDat, Hash.SHA1);

                case DatFormat.RedumpSHA256:
                    return new Hashfile(baseDat, Hash.SHA256);

                case DatFormat.RedumpSHA384:
                    return new Hashfile(baseDat, Hash.SHA384);

                case DatFormat.RedumpSHA512:
                    return new Hashfile(baseDat, Hash.SHA512);

                case DatFormat.RomCenter:
                    return new RomCenter(baseDat);

                case DatFormat.SabreDat:
                    return new SabreDat(baseDat);

                case DatFormat.SoftwareList:
                    return new SoftwareList(baseDat);

                case DatFormat.SSV:
                    return new SeparatedValue(baseDat, ';');

                case DatFormat.TSV:
                    return new SeparatedValue(baseDat, '\t');

                // We use new-style Logiqx as a backup for generic DatFile
                case null:
                default:
                    return new Logiqx(baseDat, false);
            }
        }

        /// <summary>
        /// Create a new DatFile from an existing DatHeader
        /// </summary>
        /// <param name="datHeader">DatHeader to get the values from</param>
        public static DatFile Create(DatHeader datHeader)
        {
            DatFile datFile = Create(datHeader.DatFormat);
            datFile.Header = (DatHeader)datHeader.Clone();
            return datFile;
        }

        /// <summary>
        /// Add items from another DatFile to the existing DatFile
        /// </summary>
        /// <param name="datFile">DatFile to add from</param>
        /// <param name="delete">If items should be deleted from the source DatFile</param>
        public void AddFromExisting(DatFile datFile, bool delete = false)
        {
            // Get the list of keys from the DAT
            var keys = datFile.Items.Keys.ToList();
            foreach (string key in keys)
            {
                // Add everything from the key to the internal DAT
                Items.AddRange(key, datFile.Items[key]);

                // Now remove the key from the source DAT
                if (delete)
                    datFile.Items.Remove(key);
            }

            // Now remove the file dictionary from the source DAT
            if (delete)
                datFile.Items = null;
        }

        /// <summary>
        /// Apply a DatHeader to an existing DatFile
        /// </summary>
        /// <param name="datHeader">DatHeader to get the values from</param>
        public void ApplyDatHeader(DatHeader datHeader)
        {
            Header.ConditionalCopy(datHeader);
        }

        #endregion

        #region Converting and Updating

        /// <summary>
        /// Replace item values from the base set represented by the current DAT
        /// </summary>
        /// <param name="inputs">Names of the input files</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <param name="updateFields">List of Fields representing what should be updated [only for base replacement]</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise</param>
        public void BaseReplace(
            List<string> inputs,
            string outDir,
            bool inplace,
            ExtraIni extras,
            Filter filter,
            List<Field> updateFields,
            bool onlySame)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            BaseReplace(paths, outDir, inplace, extras, filter, updateFields, onlySame);
        }

        /// <summary>
        /// Replace item values from the base set represented by the current DAT
        /// </summary>
        /// <param name="inputs">Names of the input files</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <param name="updateFields">List of Fields representing what should be updated [only for base replacement]</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise</param>
        public void BaseReplace(
            List<ParentablePath> inputs,
            string outDir,
            bool inplace,
            ExtraIni extras,
            Filter filter,
            List<Field> updateFields,
            bool onlySame)
        {
            // We want to try to replace each item in each input DAT from the base
            foreach (ParentablePath path in inputs)
            {
                Globals.Logger.User($"Replacing items in '{path.CurrentPath}' from the base DAT");

                // First we parse in the DAT internally
                DatFile intDat = Create(Header.CloneFiltering());
                intDat.Parse(path, 1, keep: true);
                intDat.ApplyExtras(extras);
                intDat.ApplyFilter(filter, false /* useTags */);

                // If we are matching based on DatItem fields of any sort
                if (updateFields.Intersect(DatItem.DatItemFields).Any())
                {
                    // For comparison's sake, we want to use CRC as the base bucketing
                    Items.BucketBy(Field.DatItem_CRC, DedupeType.Full);
                    intDat.Items.BucketBy(Field.DatItem_CRC, DedupeType.None);

                    // Then we do a hashwise comparison against the base DAT
                    Parallel.ForEach(intDat.Items.Keys, Globals.ParallelOptions, key =>
                    {
                        List<DatItem> datItems = intDat.Items[key];
                        List<DatItem> newDatItems = new List<DatItem>();
                        foreach (DatItem datItem in datItems)
                        {
                            List<DatItem> dupes = Items.GetDuplicates(datItem, sorted: true);
                            DatItem newDatItem = datItem.Clone() as DatItem;

                            // Replace fields from the first duplicate, if we have one
                            if (dupes.Count > 0)
                                newDatItem.ReplaceFields(dupes.First(), updateFields);

                            newDatItems.Add(newDatItem);
                        }

                        // Now add the new list to the key
                        intDat.Items.Remove(key);
                        intDat.Items.AddRange(key, newDatItems);
                    });
                }

                // If we are matching based on Machine fields of any sort
                if (updateFields.Intersect(DatItem.MachineFields).Any())
                {
                    // For comparison's sake, we want to use Machine Name as the base bucketing
                    Items.BucketBy(Field.Machine_Name, DedupeType.Full);
                    intDat.Items.BucketBy(Field.Machine_Name, DedupeType.None);

                    // Then we do a namewise comparison against the base DAT
                    Parallel.ForEach(intDat.Items.Keys, Globals.ParallelOptions, key =>
                    {
                        List<DatItem> datItems = intDat.Items[key];
                        List<DatItem> newDatItems = new List<DatItem>();
                        foreach (DatItem datItem in datItems)
                        {
                            DatItem newDatItem = datItem.Clone() as DatItem;
                            if (Items.ContainsKey(key) && Items[key].Count() > 0)
                                newDatItem.Machine.ReplaceFields(Items[key][0].Machine, updateFields, onlySame);

                            newDatItems.Add(newDatItem);
                        }

                        // Now add the new list to the key
                        intDat.Items.Remove(key);
                        intDat.Items.AddRange(key, newDatItems);
                    });
                }

                // Determine the output path for the DAT
                string interOutDir = path.GetOutputPath(outDir, inplace);

                // Once we're done, try writing out
                intDat.Write(interOutDir, overwrite: inplace);

                // Due to possible memory requirements, we force a garbage collection
                GC.Collect();
            }
        }

        /// <summary>
        /// Output diffs against a base set represented by the current DAT
        /// </summary>
        /// <param name="inputs">Names of the input files</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <param name="useGames">True to diff using games, false to use hashes</param>
        public void DiffAgainst(
            List<string> inputs,
            string outDir,
            bool inplace,
            ExtraIni extras,
            Filter filter,
            bool useGames)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            DiffAgainst(paths, outDir, inplace, extras, filter, useGames);
        }

        /// <summary>
        /// Output diffs against a base set represented by the current DAT
        /// </summary>
        /// <param name="inputs">Names of the input files</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <param name="useGames">True to diff using games, false to use hashes</param>
        public void DiffAgainst(
            List<ParentablePath> inputs,
            string outDir,
            bool inplace,
            ExtraIni extras,
            Filter filter,
            bool useGames)
        {
            // For comparison's sake, we want to use a base ordering
            if (useGames)
                Items.BucketBy(Field.Machine_Name, DedupeType.None);
            else
                Items.BucketBy(Field.DatItem_CRC, DedupeType.None);

            // Now we want to compare each input DAT against the base
            foreach (ParentablePath path in inputs)
            {
                Globals.Logger.User($"Comparing '{path.CurrentPath}' to base DAT");

                // First we parse in the DAT internally
                DatFile intDat = Create(Header.CloneFiltering());
                intDat.Parse(path, 1, keep: true);
                intDat.ApplyExtras(extras);
                intDat.ApplyFilter(filter, false /* useTags */);

                // For comparison's sake, we want to a the base bucketing
                if (useGames)
                    intDat.Items.BucketBy(Field.Machine_Name, DedupeType.None);
                else
                    intDat.Items.BucketBy(Field.DatItem_CRC, DedupeType.Full);

                // Then we compare against the base DAT
                List<string> keys = intDat.Items.Keys.ToList();
                Parallel.ForEach(keys, Globals.ParallelOptions, key =>
                {
                    // Game Against uses game names
                    if (useGames)
                    {
                        // If the base DAT doesn't contain the key, keep it
                        if (!Items.ContainsKey(key))
                            return;

                        // If the number of items is different, then keep it
                        if (Items[key].Count != intDat.Items[key].Count)
                            return;

                        // Otherwise, compare by name and hash the remaining files
                        bool exactMatch = true;
                        foreach (DatItem item in intDat.Items[key])
                        {
                            // TODO: Make this granular to name as well
                            if (!Items[key].Contains(item))
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
                        List<DatItem> datItems = intDat.Items[key];
                        List<DatItem> keepDatItems = new List<DatItem>();
                        foreach (DatItem datItem in datItems)
                        {
                            if (!Items.HasDuplicates(datItem, true))
                                keepDatItems.Add(datItem);
                        }

                        // Now add the new list to the key
                        intDat.Items.Remove(key);
                        intDat.Items.AddRange(key, keepDatItems);
                    }
                });

                // Determine the output path for the DAT
                string interOutDir = path.GetOutputPath(outDir, inplace);

                // Once we're done, try writing out
                intDat.Write(interOutDir, overwrite: inplace);

                // Due to possible memory requirements, we force a garbage collection
                GC.Collect();
            }
        }

        /// <summary>
        /// Output cascading diffs
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="datHeaders">Dat headers used optionally</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        /// <param name="inplace">True if cascaded diffs are outputted in-place, false otherwise</param>
        /// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
        public void DiffCascade(
            List<string> inputs,
            List<DatHeader> datHeaders,
            string outDir,
            bool inplace,
            bool skip)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            DiffCascade(paths, datHeaders, outDir, inplace, skip);
        }

        /// <summary>
        /// Output cascading diffs
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="datHeaders">Dat headers used optionally</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        /// <param name="inplace">True if cascaded diffs are outputted in-place, false otherwise</param>
        /// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
        public void DiffCascade(
            List<ParentablePath> inputs,
            List<DatHeader> datHeaders,
            string outDir,
            bool inplace,
            bool skip)
        {
            // Create a list of DatData objects representing output files
            List<DatFile> outDats = new List<DatFile>();

            // Loop through each of the inputs and get or create a new DatData object
            InternalStopwatch watch = new InternalStopwatch("Initializing all output DATs");

            DatFile[] outDatsArray = new DatFile[inputs.Count];
            Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
            {
                string innerpost = $" ({j} - {inputs[j].GetNormalizedFileName(true)} Only)";
                DatFile diffData;

                // If we're in inplace mode or the output directory is set, take the appropriate DatData object already stored
                if (inplace || outDir != Environment.CurrentDirectory)
                {
                    diffData = Create(datHeaders[j]);
                }
                else
                {
                    diffData = Create(Header);
                    diffData.Header.FileName += innerpost;
                    diffData.Header.Name += innerpost;
                    diffData.Header.Description += innerpost;
                }

                diffData.Items = new ItemDictionary();
                outDatsArray[j] = diffData;
            });

            outDats = outDatsArray.ToList();
            watch.Stop();

            // Then, ensure that the internal dat can be bucketed in the best possible way
            Items.BucketBy(Field.DatItem_CRC, DedupeType.None);

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating all output DATs");

            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = DatItem.Merge(Items[key]);

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
                    return;

                foreach (DatItem item in items)
                {
                    // There's odd cases where there are items with System ID < 0. Skip them for now
                    if (item.Source.Index < 0)
                    {
                        Globals.Logger.Warning($"Item found with a <0 SystemID: {item.Name}");
                        continue;
                    }

                    outDats[item.Source.Index].Items.Add(key, item);
                }
            });

            watch.Stop();

            // Finally, loop through and output each of the DATs
            watch.Start("Outputting all created DATs");

            Parallel.For((skip ? 1 : 0), inputs.Count, Globals.ParallelOptions, j =>
            {
                string path = inputs[j].GetOutputPath(outDir, inplace);

                // Try to output the file
                outDats[j].Write(path, overwrite: inplace);
            });

            watch.Stop();
        }

        /// <summary>
        /// Output duplicate item diff
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        public void DiffDuplicates(List<string> inputs, string outDir)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            DiffDuplicates(paths, outDir);
        }

        /// <summary>
        /// Output duplicate item diff
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        public void DiffDuplicates(List<ParentablePath> inputs, string outDir)
        {
            InternalStopwatch watch = new InternalStopwatch("Initializing duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrWhiteSpace(Header.FileName))
                Header.FileName = "All DATs";

            if (string.IsNullOrWhiteSpace(Header.Name))
                Header.Name = "All DATs";

            if (string.IsNullOrWhiteSpace(Header.Description))
                Header.Description = "All DATs";

            string post = " (Duplicates)";
            DatFile dupeData = Create(Header);
            dupeData.Header.FileName += post;
            dupeData.Header.Name += post;
            dupeData.Header.Description += post;
            dupeData.Items = new ItemDictionary();

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating duplicate DAT");

            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = DatItem.Merge(Items[key]);

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
                    return;

                // Loop through and add the items correctly
                foreach (DatItem item in items)
                {
                    if (item.DupeType.HasFlag(DupeType.External))
                    {
                        DatItem newrom = item.Clone() as DatItem;
                        newrom.Machine.Name += $" ({Path.GetFileNameWithoutExtension(inputs[item.Source.Index].CurrentPath)})";

                        dupeData.Items.Add(key, newrom);
                    }
                }
            });

            watch.Stop();

            // Finally, loop through and output each of the DATs
            watch.Start("Outputting duplicate DAT");
            dupeData.Write(outDir, overwrite: false);
            watch.Stop();
        }

        /// <summary>
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        public void DiffIndividuals(List<string> inputs, string outDir)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            DiffIndividuals(paths, outDir);
        }

        /// <summary>
        /// Output non-cascading diffs
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        public void DiffIndividuals(List<ParentablePath> inputs, string outDir)
        {
            InternalStopwatch watch = new InternalStopwatch("Initializing all individual DATs");

            // Fill in any information not in the base DAT
            if (string.IsNullOrWhiteSpace(Header.FileName))
                Header.FileName = "All DATs";

            if (string.IsNullOrWhiteSpace(Header.Name))
                Header.Name = "All DATs";

            if (string.IsNullOrWhiteSpace(Header.Description))
                Header.Description = "All DATs";

            // Loop through each of the inputs and get or create a new DatData object
            DatFile[] outDatsArray = new DatFile[inputs.Count];

            Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
            {
                string innerpost = $" ({j} - {inputs[j].GetNormalizedFileName(true)} Only)";
                DatFile diffData = Create(Header);
                diffData.Header.FileName += innerpost;
                diffData.Header.Name += innerpost;
                diffData.Header.Description += innerpost;
                diffData.Items = new ItemDictionary();
                outDatsArray[j] = diffData;
            });

            // Create a list of DatData objects representing individual output files
            List<DatFile> outDats = outDatsArray.ToList();

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating all individual DATs");

            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = DatItem.Merge(Items[key]);

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
                    return;

                // Loop through and add the items correctly
                foreach (DatItem item in items)
                {
                    if (item.DupeType.HasFlag(DupeType.Internal) || item.DupeType == 0x00)
                        outDats[item.Source.Index].Items.Add(key, item);
                }
            });

            watch.Stop();

            // Finally, loop through and output each of the DATs
            watch.Start("Outputting all individual DATs");

            Parallel.For(0, inputs.Count, Globals.ParallelOptions, j =>
            {
                string path = inputs[j].GetOutputPath(outDir, false /* inplace */);

                // Try to output the file
                outDats[j].Write(path, overwrite: false);
            });

            watch.Stop();
        }

        /// <summary>
        /// Output non-duplicate item diff
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        public void DiffNoDuplicates(List<string> inputs, string outDir)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            DiffNoDuplicates(paths, outDir);
        }

        /// <summary>
        /// Output non-duplicate item diff
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        public void DiffNoDuplicates(List<ParentablePath> inputs, string outDir)
        {
            InternalStopwatch watch = new InternalStopwatch("Initializing no duplicate DAT");

            // Fill in any information not in the base DAT
            if (string.IsNullOrWhiteSpace(Header.FileName))
                Header.FileName = "All DATs";

            if (string.IsNullOrWhiteSpace(Header.Name))
                Header.Name = "All DATs";

            if (string.IsNullOrWhiteSpace(Header.Description))
                Header.Description = "All DATs";

            string post = " (No Duplicates)";
            DatFile outerDiffData = Create(Header);
            outerDiffData.Header.FileName += post;
            outerDiffData.Header.Name += post;
            outerDiffData.Header.Description += post;
            outerDiffData.Items = new ItemDictionary();

            watch.Stop();

            // Now, loop through the dictionary and populate the correct DATs
            watch.Start("Populating no duplicate DAT");

            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = DatItem.Merge(Items[key]);

                // If the rom list is empty or null, just skip it
                if (items == null || items.Count == 0)
                    return;

                // Loop through and add the items correctly
                foreach (DatItem item in items)
                {
                    if (item.DupeType.HasFlag(DupeType.Internal) || item.DupeType == 0x00)
                    {
                        DatItem newrom = item.Clone() as DatItem;
                        newrom.Machine.Name += $" ({Path.GetFileNameWithoutExtension(inputs[item.Source.Index].CurrentPath)})";
                        outerDiffData.Items.Add(key, newrom);
                    }
                }
            });

            watch.Stop();

            // Finally, loop through and output each of the DATs
            watch.Start("Outputting no duplicate DAT");
            outerDiffData.Write(outDir, overwrite: false);
            watch.Stop();
        }

        /// <summary>
        /// Output user defined merge
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        public void MergeNoDiff(List<string> inputs, string outDir)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            MergeNoDiff(paths, outDir);
        }

        /// <summary>
        /// Output user defined merge
        /// </summary>
        /// <param name="inputs">List of inputs to write out from</param>
        /// <param name="outDir">Output directory to write the DATs to</param>
        public void MergeNoDiff(List<ParentablePath> inputs, string outDir)
        {
            // If we're in SuperDAT mode, prefix all games with their respective DATs
            if (Header.Type == "SuperDAT")
            {
                Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = Items[key].ToList();
                    List<DatItem> newItems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        DatItem newItem = item;
                        string filename = inputs[newItem.Source.Index].CurrentPath;
                        string rootpath = inputs[newItem.Source.Index].ParentPath;

                        rootpath += (string.IsNullOrWhiteSpace(rootpath) ? string.Empty : Path.DirectorySeparatorChar.ToString());
                        filename = filename.Remove(0, rootpath.Length);
                        newItem.Machine.Name = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar
                            + Path.GetFileNameWithoutExtension(filename) + Path.DirectorySeparatorChar
                            + newItem.Machine.Name;

                        newItems.Add(newItem);
                    }

                    Items.Remove(key);
                    Items.AddRange(key, newItems);
                });
            }

            // Try to output the file
            Write(outDir, overwrite: false);
        }

        /// <summary>
        /// Populate the user DatData object from the input files
        /// </summary>
        /// <param name="inputs">Paths to DATs to parse</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <returns>List of DatData objects representing headers</returns>
        public List<DatHeader> PopulateUserData(List<string> inputs, ExtraIni extras, Filter filter)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            return PopulateUserData(paths, extras, filter);
        }

        /// <summary>
        /// Populate the user DatData object from the input files
        /// </summary>
        /// <param name="inputs">Paths to DATs to parse</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <returns>List of DatData objects representing headers</returns>
        public List<DatHeader> PopulateUserData(List<ParentablePath> inputs, ExtraIni extras, Filter filter)
        {
            DatFile[] datFiles = new DatFile[inputs.Count];
            InternalStopwatch watch = new InternalStopwatch("Processing individual DATs");

            // Parse all of the DATs into their own DatFiles in the array
            Parallel.For(0, inputs.Count, Globals.ParallelOptions, i =>
            {
                var input = inputs[i];
                Globals.Logger.User($"Adding DAT: {input.CurrentPath}");
                datFiles[i] = Create(Header.CloneFiltering());
                datFiles[i].Parse(input, i, keep: true);
            });

            watch.Stop();

            watch.Start("Populating internal DAT");
            for (int i = 0; i < inputs.Count; i++)
            {
                AddFromExisting(datFiles[i], true);
            }

            // Now that we have a merged DAT, apply extras and filter it
            ApplyExtras(extras);
            ApplyFilter(filter, false /* useTags */);

            watch.Stop();

            return datFiles.Select(d => d.Header).ToList();
        }

        /// <summary>
        /// Convert, update, and filter a DAT file or set of files
        /// </summary>
        /// <param name="inputs">Names of the input files and/or folders</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        public void Update(
            List<string> inputs,
            string outDir,
            bool inplace,
            ExtraIni extras,
            Filter filter)
        {
            List<ParentablePath> paths = inputs.Select(i => new ParentablePath(i)).ToList();
            Update(paths, outDir, inplace, extras, filter);
        }

        /// <summary>
        /// Convert, update, and filter a DAT file or set of files
        /// </summary>
        /// <param name="inputs">Names of the input files and/or folders</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="inplace">True if the output files should overwrite their inputs, false otherwise</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        public void Update(
            List<ParentablePath> inputs,
            string outDir,
            bool inplace,
            ExtraIni extras,
            Filter filter)
        {
            // Iterate over the files
            foreach (ParentablePath file in inputs)
            {
                DatFile innerDatdata = Create(Header);
                Globals.Logger.User($"Processing '{Path.GetFileName(file.CurrentPath)}'");
                innerDatdata.Parse(file, keep: true,
                    keepext: innerDatdata.Header.DatFormat.HasFlag(DatFormat.TSV)
                        || innerDatdata.Header.DatFormat.HasFlag(DatFormat.CSV)
                        || innerDatdata.Header.DatFormat.HasFlag(DatFormat.SSV));
                innerDatdata.ApplyExtras(extras);
                innerDatdata.ApplyFilter(filter, false /* useTags */);

                // Get the correct output path
                string realOutDir = file.GetOutputPath(outDir, inplace);

                // Try to output the file, overwriting only if it's not in the current directory
                innerDatdata.Write(realOutDir, overwrite: inplace);
            }
        }

        #endregion

        #region Filtering

        /// <summary>
        /// Apply a set of Extra INIs on the DatFile
        /// </summary>
        /// <param name="extras">ExtrasIni to use</param>
        /// <returns>True if the extras were applied, false on error</returns>
        public bool ApplyExtras(ExtraIni extras)
        {
            try
            {
                // Bucket by game first
                Items.BucketBy(Field.Machine_Name, DedupeType.None);

                // Create a new set of mappings based on the items
                var map = new Dictionary<string, Dictionary<Field, string>>();

                // Loop through each of the extras
                foreach (ExtraIniItem item in extras.Items)
                {
                    foreach (var mapping in item.Mappings)
                    {
                        string key = mapping.Key;
                        List<string> machineNames = mapping.Value;

                        // If we have the root folder, assume boolean
                        if (string.Equals(key, "ROOT_FOLDER"))
                            key = "true";

                        // Loop through the machines and add the new mappings
                        foreach (string machine in machineNames)
                        {
                            if (!map.ContainsKey(machine))
                                map[machine] = new Dictionary<Field, string>();

                            map[machine][item.Field] = key;
                        }
                    }
                }

                // Now apply the new set of mappings
                foreach (string key in map.Keys)
                {
                    // If the key doesn't exist, continue
                    if (!Items.ContainsKey(key))
                        continue;

                    List<DatItem> datItems = Items[key];
                    var mappings = map[key];

                    foreach (var datItem in datItems)
                    {
                        datItem.SetFields(mappings);
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Apply a Filter on the DatFile
        /// </summary>
        /// <param name="filter">Filter to use</param>
        /// <param name="useTags">True if DatFile tags override splitting, false otherwise</param>
        /// <returns>True if the DatFile was filtered, false on error</returns>
        public bool ApplyFilter(Filter filter, bool useTags)
        {
            try
            {
                // Process description to machine name
                if (filter.DescriptionAsName)
                    MachineDescriptionToName();

                // If we are using tags from the DAT, set the proper input for split type unless overridden
                if (useTags && filter.InternalSplit == MergingFlag.None)
                    filter.InternalSplit = Header.ForceMerging;

                // Run internal splitting
                ProcessSplitType(filter.InternalSplit);

                // Loop over every key in the dictionary
                List<string> keys = Items.Keys.ToList();
                foreach (string key in keys)
                {
                    // For every item in the current key
                    List<DatItem> items = Items[key];
                    List<DatItem> newitems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        // If we have a null item, we can't pass it
                        if (item == null)
                            continue;

                        // If the rom passes the filter, include it
                        if (item.PassesFilter(filter))
                        {
                            // If we're stripping unicode characters, do so from all relevant things
                            if (filter.RemoveUnicode)
                            {
                                item.Name = Sanitizer.RemoveUnicodeCharacters(item.Name);
                                item.Machine.Name = Sanitizer.RemoveUnicodeCharacters(item.Machine.Name);
                                item.Machine.Description = Sanitizer.RemoveUnicodeCharacters(item.Machine.Description);
                            }

                            // If we're in cleaning mode, do so from all relevant things
                            if (filter.Clean)
                            {
                                item.Machine.Name = Sanitizer.CleanGameName(item.Machine.Name);
                                item.Machine.Description = Sanitizer.CleanGameName(item.Machine.Description);
                            }

                            // If we are in single game mode, rename all games
                            if (filter.Single)
                                item.Machine.Name = "!";

                            // If we are in NTFS trim mode, trim the game name
                            if (filter.Trim)
                            {
                                // Windows max name length is 260
                                int usableLength = 260 - item.Machine.Name.Length - filter.Root.Length;
                                if (item.Name.Length > usableLength)
                                {
                                    string ext = Path.GetExtension(item.Name);
                                    item.Name = item.Name.Substring(0, usableLength - ext.Length);
                                    item.Name += ext;
                                }
                            }

                            // Add the item to the output
                            newitems.Add(item);
                        }
                    }

                    Items.Remove(key);
                    Items.AddRange(key, newitems);
                }

                // If we are removing scene dates, do that now
                if (Header.SceneDateStrip)
                    StripSceneDatesFromItems();

                // Run the one rom per game logic, if required
                if (Header.OneGamePerRegion)
                    OneGamePerRegion();

                // Run the one rom per game logic, if required
                if (Header.OneRomPerGame)
                    OneRomPerGame();

                // If we are removing fields, do that now
                // TODO: If this works, why in the hell do I have all of the "GetField" crap?????
                if (Header.ExcludeFields != null && Header.ExcludeFields.Any())
                    RemoveFieldsFromItems();

                // We remove any blanks, if we aren't supposed to have any
                if (!Header.KeepEmptyGames)
                {
                    List<string> possiblyEmptyKeys = Items.Keys.ToList();
                    foreach (string key in possiblyEmptyKeys)
                    {
                        List<DatItem> items = Items[key];
                        if (items == null || items.Count == 0)
                        {
                            Items.Remove(key);
                            continue;
                        }

                        List<DatItem> newitems = items.Where(i => i.ItemType != ItemType.Blank).ToList();

                        Items.Remove(key);
                        Items.AddRange(key, newitems);
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Use game descriptions as names in the DAT, updating cloneof/romof/sampleof
        /// </summary>
        public void MachineDescriptionToName()
        {
            try
            {
                // First we want to get a mapping for all games to description
                ConcurrentDictionary<string, string> mapping = new ConcurrentDictionary<string, string>();
                Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = Items[key];
                    foreach (DatItem item in items)
                    {
                        // If the key mapping doesn't exist, add it
                        mapping.TryAdd(item.Machine.Name, item.Machine.Description.Replace('/', '_').Replace("\"", "''").Replace(":", " -"));
                    }
                });

                // Now we loop through every item and update accordingly
                Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
                {
                    List<DatItem> items = Items[key];
                    List<DatItem> newItems = new List<DatItem>();
                    foreach (DatItem item in items)
                    {
                        // Update machine name
                        if (!string.IsNullOrWhiteSpace(item.Machine.Name) && mapping.ContainsKey(item.Machine.Name))
                            item.Machine.Name = mapping[item.Machine.Name];

                        // Update cloneof
                        if (!string.IsNullOrWhiteSpace(item.Machine.CloneOf) && mapping.ContainsKey(item.Machine.CloneOf))
                            item.Machine.CloneOf = mapping[item.Machine.CloneOf];

                        // Update romof
                        if (!string.IsNullOrWhiteSpace(item.Machine.RomOf) && mapping.ContainsKey(item.Machine.RomOf))
                            item.Machine.RomOf = mapping[item.Machine.RomOf];

                        // Update sampleof
                        if (!string.IsNullOrWhiteSpace(item.Machine.SampleOf) && mapping.ContainsKey(item.Machine.SampleOf))
                            item.Machine.SampleOf = mapping[item.Machine.SampleOf];

                        // Add the new item to the output list
                        newItems.Add(item);
                    }

                    // Replace the old list of roms with the new one
                    Items.Remove(key);
                    Items.AddRange(key, newItems);
                });
            }
            catch (Exception ex)
            {
                Globals.Logger.Warning(ex.ToString());
            }
        }

        /// <summary>
        /// Filter a DAT using 1G1R logic given an ordered set of regions
        /// </summary>
        /// <remarks>
        /// In the most technical sense, the way that the region list is being used does not
        /// confine its values to be just regions. Since it's essentially acting like a
        /// specialized version of the machine name filter, anything that is usually encapsulated
        /// in parenthesis would be matched on, including disc numbers, languages, editions,
        /// and anything else commonly used. Please note that, unlike other existing 1G1R 
        /// solutions, this does not have the ability to contain custom mappings of parent
        /// to clone sets based on name, nor does it have the ability to match on the 
        /// Release DatItem type.
        /// </remarks>
        public void OneGamePerRegion()
        {
            // For sake of ease, the first thing we want to do is bucket by game
            Items.BucketBy(Field.Machine_Name, DedupeType.None, norename: true);

            // Then we want to get a mapping of all machines to parents
            Dictionary<string, List<string>> parents = new Dictionary<string, List<string>>();
            foreach (string key in Items.Keys)
            {
                DatItem item = Items[key][0];

                // Match on CloneOf first
                if (!string.IsNullOrEmpty(item.Machine.CloneOf))
                {
                    if (!parents.ContainsKey(item.Machine.CloneOf.ToLowerInvariant()))
                        parents.Add(item.Machine.CloneOf.ToLowerInvariant(), new List<string>());

                    parents[item.Machine.CloneOf.ToLowerInvariant()].Add(item.Machine.Name.ToLowerInvariant());
                }

                // Then by RomOf
                else if (!string.IsNullOrEmpty(item.Machine.RomOf))
                {
                    if (!parents.ContainsKey(item.Machine.RomOf.ToLowerInvariant()))
                        parents.Add(item.Machine.RomOf.ToLowerInvariant(), new List<string>());

                    parents[item.Machine.RomOf.ToLowerInvariant()].Add(item.Machine.Name.ToLowerInvariant());
                }

                // Otherwise, treat it as a parent
                else
                {
                    if (!parents.ContainsKey(item.Machine.Name.ToLowerInvariant()))
                        parents.Add(item.Machine.Name.ToLowerInvariant(), new List<string>());

                    parents[item.Machine.Name.ToLowerInvariant()].Add(item.Machine.Name.ToLowerInvariant());
                }
            }

            // If we have null region list, make it empty
            List<string> regions = Header.RegionList;
            if (regions == null)
                regions = new List<string>();

            // Once we have the full list of mappings, filter out games to keep
            foreach (string key in parents.Keys)
            {
                // Find the first machine that matches the regions in order, if possible
                string machine = default;
                foreach (string region in regions)
                {
                    machine = parents[key].FirstOrDefault(m => Regex.IsMatch(m, @"\(.*" + region + @".*\)", RegexOptions.IgnoreCase));
                    if (machine != default)
                        break;
                }

                // If we didn't get a match, use the parent
                if (machine == default)
                    machine = key;

                // Remove the key from the list
                parents[key].Remove(machine);

                // Remove the rest of the items from this key
                parents[key].ForEach(k => Items.Remove(k));
            }

            // Finally, strip out the parent tags
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Ensure that all roms are in their own game (or at least try to ensure)
        /// </summary>
        public void OneRomPerGame()
        {
            // Because this introduces subfolders, we need to set the SuperDAT type
            Header.Type = "SuperDAT";

            // For each rom, we want to update the game to be "<game name>/<rom name>"
            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = Items[key];
                for (int i = 0; i < items.Count; i++)
                {
                    string[] splitname = items[i].Name.Split('.');
                    items[i].Machine.Name += $"/{string.Join(".", splitname.Take(splitname.Length > 1 ? splitname.Length - 1 : 1))}";
                    items[i].Name = Path.GetFileName(items[i].Name);
                }
            });
        }

        /// <summary>
        /// Remove fields as per the header
        /// </summary>
        public void RemoveFieldsFromItems()
        {
            // Output the logging statement
            Globals.Logger.User("Removing filtered fields");

            // Get the array of fields from the header
            List<Field> fields = Header.ExcludeFields;

            // Now process all of the roms
            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = Items[key];
                for (int j = 0; j < items.Count; j++)
                {
                    items[j].RemoveFields(fields);
                }

                Items.Remove(key);
                Items.AddRange(key, items);
            });
        }

        /// <summary>
        /// Strip the dates from the beginning of scene-style set names
        /// </summary>
        public void StripSceneDatesFromItems()
        {
            // Output the logging statement
            Globals.Logger.User("Stripping scene-style dates");

            // Set the regex pattern to use
            string pattern = @"([0-9]{2}\.[0-9]{2}\.[0-9]{2}-)(.*?-.*?)";

            // Now process all of the roms
            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = Items[key];
                for (int j = 0; j < items.Count; j++)
                {
                    DatItem item = items[j];
                    if (Regex.IsMatch(item.Machine.Name, pattern))
                        item.Machine.Name = Regex.Replace(item.Machine.Name, pattern, "$2");

                    if (Regex.IsMatch(item.Machine.Description, pattern))
                        item.Machine.Description = Regex.Replace(item.Machine.Description, pattern, "$2");

                    items[j] = item;
                }

                Items.Remove(key);
                Items.AddRange(key, items);
            });
        }

        #endregion

        #region Internal Splitting/Merging

        /// <summary>
        /// Process items according to split type
        /// </summary>
        /// <param name="splitType">MergingFlag to implement</param>
        public void ProcessSplitType(MergingFlag splitType)
        {
            // Now we pre-process the DAT with the splitting/merging mode
            switch (splitType)
            {
                case MergingFlag.None:
                    // No-op
                    break;
                case MergingFlag.Device:
                    CreateDeviceNonMergedSets(DedupeType.None);
                    break;
                case MergingFlag.Full:
                    CreateFullyNonMergedSets(DedupeType.None);
                    break;
                case MergingFlag.NonMerged:
                    CreateNonMergedSets(DedupeType.None);
                    break;
                case MergingFlag.Merged:
                    CreateMergedSets(DedupeType.None);
                    break;
                case MergingFlag.Split:
                    CreateSplitSets(DedupeType.None);
                    break;
            }
        }

        /// <summary>
        /// Use cdevice_ref tags to get full non-merged sets and remove parenting tags
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateDeviceNonMergedSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating device non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            Items.BucketBy(Field.Machine_Name, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddRomsFromDevices(false, false)) ;
            while (AddRomsFromDevices(true, false)) ;

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags plus using the device_ref tags to get full sets
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateFullyNonMergedSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating fully non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            Items.BucketBy(Field.Machine_Name, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            while (AddRomsFromDevices(true, true)) ;
            AddRomsFromDevices(false, true);
            AddRomsFromParent();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            AddRomsFromBios();

            // Then, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use cloneof tags to create merged sets and remove the tags
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateMergedSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            Items.BucketBy(Field.Machine_Name, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddRomsFromChildren();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(false);
            RemoveBiosRomsFromChild(true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use cloneof tags to create non-merged sets and remove the tags
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateNonMergedSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating non-merged sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            Items.BucketBy(Field.Machine_Name, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            AddRomsFromParent();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(false);
            RemoveBiosRomsFromChild(true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use cloneof and romof tags to create split sets and remove the tags
        /// </summary>
        /// <param name="mergeroms">Dedupe type to be used</param>
        private void CreateSplitSets(DedupeType mergeroms)
        {
            Globals.Logger.User("Creating split sets from the DAT");

            // For sake of ease, the first thing we want to do is bucket by game
            Items.BucketBy(Field.Machine_Name, mergeroms, norename: true);

            // Now we want to loop through all of the games and set the correct information
            RemoveRomsFromChild();

            // Now that we have looped through the cloneof tags, we loop through the romof tags
            RemoveBiosRomsFromChild(false);
            RemoveBiosRomsFromChild(true);

            // Finally, remove the romof and cloneof tags so it's not picked up by the manager
            RemoveTagsFromChild();
        }

        /// <summary>
        /// Use romof tags to add roms to the children
        /// </summary>
        private void AddRomsFromBios()
        {
            List<string> games = Items.Keys.OrderBy(g => g).ToList();
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (Items[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(Items[game][0].Machine.RomOf))
                    parent = Items[game][0].Machine.RomOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (Items[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = Items[game][0];
                List<DatItem> parentItems = Items[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (Items[game].Where(i => i.Name == datItem.Name).Count() == 0 && !Items[game].Contains(datItem))
                        Items.Add(game, datItem);
                }
            }
        }

        /// <summary>
        /// Use device_ref and optionally slotoption tags to add roms to the children
        /// </summary>
        /// <param name="dev">True if only child device sets are touched, false for non-device sets (default)</param>
        /// <param name="slotoptions">True if slotoptions tags are used as well, false otherwise</param>
        private bool AddRomsFromDevices(bool dev = false, bool slotoptions = false)
        {
            bool foundnew = false;
            List<string> games = Items.Keys.OrderBy(g => g).ToList();
            foreach (string game in games)
            {
                // If the game doesn't have items, we continue
                if (Items[game] == null || Items[game].Count == 0)
                    continue;

                // If the game (is/is not) a bios, we want to continue
                if (dev ^ (Items[game][0].Machine.MachineType.HasFlag(MachineType.Device)))
                    continue;

                // If the game has no devices, we continue
                if (Items[game][0].Machine.DeviceReferences == null
                    || Items[game][0].Machine.DeviceReferences.Count == 0
                    || (slotoptions && Items[game][0].Machine.Slots == null)
                    || (slotoptions && Items[game][0].Machine.Slots.Count == 0))
                {
                    continue;
                }

                // Determine if the game has any devices or not
                List<ListXmlDeviceReference> deviceReferences = Items[game][0].Machine.DeviceReferences;
                List<ListXmlDeviceReference> newdevs = new List<ListXmlDeviceReference>();
                foreach (ListXmlDeviceReference deviceReference in deviceReferences)
                {
                    // If the device doesn't exist then we continue
                    if (Items[deviceReference.Name].Count == 0)
                        continue;

                    // Otherwise, copy the items from the device to the current game
                    DatItem copyFrom = Items[game][0];
                    List<DatItem> devItems = Items[deviceReference.Name];
                    foreach (DatItem item in devItems)
                    {
                        DatItem datItem = (DatItem)item.Clone();
                        newdevs.AddRange(datItem.Machine.DeviceReferences ?? new List<ListXmlDeviceReference>());
                        datItem.CopyMachineInformation(copyFrom);
                        if (Items[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0)
                        {
                            foundnew = true;
                            Items.Add(game, datItem);
                        }
                    }
                }

                // Now that every device is accounted for, add the new list of devices, if they don't already exist
                foreach (ListXmlDeviceReference device in newdevs)
                {
                    if (!Items[game][0].Machine.DeviceReferences.Select(d => d.Name).Contains(device.Name))
                        Items[game][0].Machine.DeviceReferences.Add(new ListXmlDeviceReference() { Name = device.Name });
                }

                // If we're checking slotoptions too
                if (slotoptions)
                {
                    // Determine if the game has any slots or not
                    List<ListXmlSlot> slots = Items[game][0].Machine.Slots;
                    List<ListXmlSlot> newSlots = new List<ListXmlSlot>();
                    foreach (ListXmlSlot slot in slots)
                    {
                        foreach (ListXmlSlotOption slotOption in slot.SlotOptions)
                        {
                            // If the slotoption doesn't exist then we continue
                            if (Items[slotOption.Name].Count == 0)
                                continue;

                            // Otherwise, copy the items from the slotoption to the current game
                            DatItem copyFrom = Items[game][0];
                            List<DatItem> slotItems = Items[slotOption.Name];
                            foreach (DatItem item in slotItems)
                            {
                                DatItem datItem = (DatItem)item.Clone();
                                newSlots.AddRange(datItem.Machine.Slots ?? new List<ListXmlSlot>());

                                datItem.CopyMachineInformation(copyFrom);
                                if (Items[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0)
                                {
                                    foundnew = true;
                                    Items.Add(game, datItem);
                                }
                            }
                        }
                    }

                    // Now that every slotoption is accounted for, add the new list of slots, if they don't already exist
                    foreach (ListXmlSlot slot in newSlots)
                    {
                        Items[game][0].Machine.Slots.Add(slot);
                    }
                }
            }

            return foundnew;
        }

        /// <summary>
        /// Use cloneof tags to add roms to the children, setting the new romof tag in the process
        /// </summary>
        private void AddRomsFromParent()
        {
            List<string> games = Items.Keys.OrderBy(g => g).ToList();
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (Items[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(Items[game][0].Machine.CloneOf))
                    parent = Items[game][0].Machine.CloneOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (Items[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we copy the items from the parent to the current game
                DatItem copyFrom = Items[game][0];
                List<DatItem> parentItems = Items[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    datItem.CopyMachineInformation(copyFrom);
                    if (Items[game].Where(i => i.Name.ToLowerInvariant() == datItem.Name.ToLowerInvariant()).Count() == 0
                        && !Items[game].Contains(datItem))
                    {
                        Items.Add(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the items
                List<DatItem> items = Items[game];
                string romof = Items[parent][0].Machine.RomOf;
                foreach (DatItem item in items)
                {
                    item.Machine.RomOf = romof;
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to add roms to the parents, removing the child sets in the process
        /// </summary>
        /// <param name="subfolder">True to add DatItems to subfolder of parent (not including Disk), false otherwise</param>
        private void AddRomsFromChildren(bool subfolder = true)
        {
            List<string> games = Items.Keys.OrderBy(g => g).ToList();
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (Items[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(Items[game][0].Machine.CloneOf))
                    parent = Items[game][0].Machine.CloneOf;

                // If there is no parent, then we continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // Otherwise, move the items from the current game to a subfolder of the parent game
                DatItem copyFrom;
                if (Items[parent].Count == 0)
                {
                    copyFrom = new Rom();
                    copyFrom.Machine.Name = parent;
                    copyFrom.Machine.Description = parent;
                }
                else
                {
                    copyFrom = Items[parent][0];
                }

                List<DatItem> items = Items[game];
                foreach (DatItem item in items)
                {
                    // Special disk handling
                    if (item.ItemType == ItemType.Disk)
                    {
                        Disk disk = item as Disk;

                        // If the merge tag exists and the parent already contains it, skip
                        if (disk.MergeTag != null && Items[parent].Select(i => i.Name).Contains(disk.MergeTag))
                        {
                            continue;
                        }

                        // If the merge tag exists but the parent doesn't contain it, add to parent
                        else if (disk.MergeTag != null && !Items[parent].Select(i => i.Name).Contains(disk.MergeTag))
                        {
                            item.CopyMachineInformation(copyFrom);
                            Items.Add(parent, item);
                        }

                        // If there is no merge tag, add to parent
                        else if (disk.MergeTag == null)
                        {
                            item.CopyMachineInformation(copyFrom);
                            Items.Add(parent, item);
                        }
                    }

                    // Special rom handling
                    else if (item.ItemType == ItemType.Rom)
                    {
                        Rom rom = item as Rom;

                        // If the merge tag exists and the parent already contains it, skip
                        if (rom.MergeTag != null && Items[parent].Select(i => i.Name).Contains(rom.MergeTag))
                        {
                            continue;
                        }

                        // If the merge tag exists but the parent doesn't contain it, add to subfolder of parent
                        else if (rom.MergeTag != null && !Items[parent].Select(i => i.Name).Contains(rom.MergeTag))
                        {
                            if (subfolder)
                                item.Name = $"{item.Machine.Name}\\{item.Name}";

                            item.CopyMachineInformation(copyFrom);
                            Items.Add(parent, item);
                        }

                        // If the parent doesn't already contain this item, add to subfolder of parent
                        else if (!Items[parent].Contains(item))
                        {
                            if (subfolder)
                                item.Name = $"{item.Machine.Name}\\{item.Name}";

                            item.CopyMachineInformation(copyFrom);
                            Items.Add(parent, item);
                        }
                    }

                    // All other that would be missing to subfolder of parent
                    else if (!Items[parent].Contains(item))
                    {
                        if (subfolder)
                            item.Name = $"{item.Machine.Name}\\{item.Name}";

                        item.CopyMachineInformation(copyFrom);
                        Items.Add(parent, item);
                    }
                }

                // Then, remove the old game so it's not picked up by the writer
                Items.Remove(game);
            }
        }

        /// <summary>
        /// Remove all BIOS and device sets
        /// </summary>
        private void RemoveBiosAndDeviceSets()
        {
            List<string> games = Items.Keys.OrderBy(g => g).ToList();
            foreach (string game in games)
            {
                if (Items[game].Count > 0
                    && (Items[game][0].Machine.MachineType.HasFlag(MachineType.Bios)
                        || Items[game][0].Machine.MachineType.HasFlag(MachineType.Device)))
                {
                    Items.Remove(game);
                }
            }
        }

        /// <summary>
        /// Use romof tags to remove bios roms from children
        /// </summary>
        /// <param name="bios">True if only child Bios sets are touched, false for non-bios sets (default)</param>
        private void RemoveBiosRomsFromChild(bool bios = false)
        {
            // Loop through the romof tags
            List<string> games = Items.Keys.OrderBy(g => g).ToList();
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (Items[game].Count == 0)
                    continue;

                // If the game (is/is not) a bios, we want to continue
                if (bios ^ Items[game][0].Machine.MachineType.HasFlag(MachineType.Bios))
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(Items[game][0].Machine.RomOf))
                    parent = Items[game][0].Machine.RomOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (Items[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we remove the items that are in the parent from the current game
                List<DatItem> parentItems = Items[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (Items[game].Contains(datItem))
                    {
                        Items.Remove(game, datItem);
                    }
                }
            }
        }

        /// <summary>
        /// Use cloneof tags to remove roms from the children
        /// </summary>
        private void RemoveRomsFromChild()
        {
            List<string> games = Items.Keys.OrderBy(g => g).ToList();
            foreach (string game in games)
            {
                // If the game has no items in it, we want to continue
                if (Items[game].Count == 0)
                    continue;

                // Determine if the game has a parent or not
                string parent = null;
                if (!string.IsNullOrWhiteSpace(Items[game][0].Machine.CloneOf))
                    parent = Items[game][0].Machine.CloneOf;

                // If the parent doesnt exist, we want to continue
                if (string.IsNullOrWhiteSpace(parent))
                    continue;

                // If the parent doesn't have any items, we want to continue
                if (Items[parent].Count == 0)
                    continue;

                // If the parent exists and has items, we remove the parent items from the current game
                List<DatItem> parentItems = Items[parent];
                foreach (DatItem item in parentItems)
                {
                    DatItem datItem = (DatItem)item.Clone();
                    while (Items[game].Contains(datItem))
                    {
                        Items.Remove(game, datItem);
                    }
                }

                // Now we want to get the parent romof tag and put it in each of the remaining items
                List<DatItem> items = Items[game];
                string romof = Items[parent][0].Machine.RomOf;
                foreach (DatItem item in items)
                {
                    item.Machine.RomOf = romof;
                }
            }
        }

        /// <summary>
        /// Remove all romof and cloneof tags from all games
        /// </summary>
        private void RemoveTagsFromChild()
        {
            List<string> games = Items.Keys.OrderBy(g => g).ToList();
            foreach (string game in games)
            {
                List<DatItem> items = Items[game];
                foreach (DatItem item in items)
                {
                    item.Machine.CloneOf = null;
                    item.Machine.RomOf = null;
                    item.Machine.SampleOf = null;
                }
            }
        }

        #endregion

        #region Parsing

        /// <summary>
        /// Create a DatFile and parse a file into it
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        public static DatFile CreateAndParse(string filename)
        {
            DatFile datFile = Create();
            datFile.Parse(new ParentablePath(filename));
            return datFile;
        }

        /// <summary>
        /// Parse a DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
        public void Parse(string filename, int indexId = 0, bool keep = false, bool keepext = false)
        {
            ParentablePath path = new ParentablePath(filename.Trim('"'));
            Parse(path, indexId, keep, keepext);
        }

        /// <summary>
        /// Parse a DAT and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
        public void Parse(ParentablePath filename, int indexId = 0, bool keep = false, bool keepext = false)
        {
            // Get the current path from the filename
            string currentPath = filename.CurrentPath;

            // Check the file extension first as a safeguard
            if (!PathExtensions.HasValidDatExtension(currentPath))
                return;

            // If the output filename isn't set already, get the internal filename
            Header.FileName = (string.IsNullOrWhiteSpace(Header.FileName) ? (keepext ? Path.GetFileName(currentPath) : Path.GetFileNameWithoutExtension(currentPath)) : Header.FileName);

            // If the output type isn't set already, get the internal output type
            Header.DatFormat = (Header.DatFormat == 0 ? currentPath.GetDatFormat() : Header.DatFormat);
            Items.SetBucketedBy(Field.DatItem_CRC); // Setting this because it can reduce issues later

            // Now parse the correct type of DAT
            try
            {
                Create(currentPath.GetDatFormat(), this)?.ParseFile(currentPath, indexId, keep);
            }
            catch (Exception ex)
            {
                Globals.Logger.Error($"Error with file '{filename}': {ex}");
            }
        }

        /// <summary>
        /// Add a rom to the Dat after checking
        /// </summary>
        /// <param name="item">Item data to check against</param>
        /// <returns>The key for the item</returns>
        protected string ParseAddHelper(DatItem item)
        {
            string key = string.Empty;

            // If there's no name in the rom, we log and skip it
            if (item.Name == null)
            {
                Globals.Logger.Warning($"{Header.FileName}: Rom with no name found! Skipping...");
                return key;
            }

            // If we have a Rom or a Disk, clean the hash data
            if (item.ItemType == ItemType.Rom)
            {
                Rom itemRom = (Rom)item;

                // If we have the case where there is SHA-1 and nothing else, we don't fill in any other part of the data
                if (itemRom.Size == -1
                    && string.IsNullOrWhiteSpace(itemRom.CRC)
                    && string.IsNullOrWhiteSpace(itemRom.MD5)
#if NET_FRAMEWORK
                    && string.IsNullOrWhiteSpace(itemRom.RIPEMD160)
#endif
                    && !string.IsNullOrWhiteSpace(itemRom.SHA1)
                    && string.IsNullOrWhiteSpace(itemRom.SHA256)
                    && string.IsNullOrWhiteSpace(itemRom.SHA384)
                    && string.IsNullOrWhiteSpace(itemRom.SHA512))
                {
                    // No-op, just catch it so it doesn't go further
                    Globals.Logger.Verbose($"{Header.FileName}: Entry with only SHA-1 found - '{itemRom.Name}'");
                }

                // If we have a rom and it's missing size AND the hashes match a 0-byte file, fill in the rest of the info
                else if ((itemRom.Size == 0 || itemRom.Size == -1)
                    && ((itemRom.CRC == Constants.CRCZero || string.IsNullOrWhiteSpace(itemRom.CRC))
                        || itemRom.MD5 == Constants.MD5Zero
#if NET_FRAMEWORK
                        || itemRom.RIPEMD160 == Constants.RIPEMD160Zero
#endif
                        || itemRom.SHA1 == Constants.SHA1Zero
                        || itemRom.SHA256 == Constants.SHA256Zero
                        || itemRom.SHA384 == Constants.SHA384Zero
                        || itemRom.SHA512 == Constants.SHA512Zero))
                {
                    // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                    itemRom.Size = Constants.SizeZero;
                    itemRom.CRC = Constants.CRCZero;
                    itemRom.MD5 = Constants.MD5Zero;
#if NET_FRAMEWORK
                    itemRom.RIPEMD160 = null;
                    //itemRom.RIPEMD160 = Constants.RIPEMD160Zero;
#endif
                    itemRom.SHA1 = Constants.SHA1Zero;
                    itemRom.SHA256 = null;
                    //itemRom.SHA256 = Constants.SHA256Zero;
                    itemRom.SHA384 = null;
                    //itemRom.SHA384 = Constants.SHA384Zero;
                    itemRom.SHA512 = null;
                    //itemRom.SHA512 = Constants.SHA512Zero;
                }
                // If the file has no size and it's not the above case, skip and log
                else if (itemRom.ItemStatus != ItemStatus.Nodump && (itemRom.Size == 0 || itemRom.Size == -1))
                {
                    Globals.Logger.Verbose($"{Header.FileName}: Incomplete entry for '{itemRom.Name}' will be output as nodump");
                    itemRom.ItemStatus = ItemStatus.Nodump;
                }
                // If the file has a size but aboslutely no hashes, skip and log
                else if (itemRom.ItemStatus != ItemStatus.Nodump
                    && itemRom.Size > 0
                    && string.IsNullOrWhiteSpace(itemRom.CRC)
                    && string.IsNullOrWhiteSpace(itemRom.MD5)
#if NET_FRAMEWORK
                    && string.IsNullOrWhiteSpace(itemRom.RIPEMD160)
#endif
                    && string.IsNullOrWhiteSpace(itemRom.SHA1)
                    && string.IsNullOrWhiteSpace(itemRom.SHA256)
                    && string.IsNullOrWhiteSpace(itemRom.SHA384)
                    && string.IsNullOrWhiteSpace(itemRom.SHA512))
                {
                    Globals.Logger.Verbose($"{Header.FileName}: Incomplete entry for '{itemRom.Name}' will be output as nodump");
                    itemRom.ItemStatus = ItemStatus.Nodump;
                }

                item = itemRom;
            }
            else if (item.ItemType == ItemType.Disk)
            {
                Disk itemDisk = (Disk)item;

                // If the file has aboslutely no hashes, skip and log
                if (itemDisk.ItemStatus != ItemStatus.Nodump
                    && string.IsNullOrWhiteSpace(itemDisk.MD5)
#if NET_FRAMEWORK
                    && string.IsNullOrWhiteSpace(itemDisk.RIPEMD160)
#endif
                    && string.IsNullOrWhiteSpace(itemDisk.SHA1)
                    && string.IsNullOrWhiteSpace(itemDisk.SHA256)
                    && string.IsNullOrWhiteSpace(itemDisk.SHA384)
                    && string.IsNullOrWhiteSpace(itemDisk.SHA512))
                {
                    Globals.Logger.Verbose($"Incomplete entry for '{itemDisk.Name}' will be output as nodump");
                    itemDisk.ItemStatus = ItemStatus.Nodump;
                }

                item = itemDisk;
            }

            // Get the key and add the file
            key = item.GetKey(Field.DatItem_CRC);
            Items.Add(key, item);

            return key;
        }

        /// <summary>
        /// Parse DatFile and return all found games and roms within
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        protected abstract void ParseFile(string filename, int indexId, bool keep);

        #endregion

        #region Populate DAT from Directory

        /// <summary>
        /// Create a new Dat from a directory
        /// </summary>
        /// <param name="basePath">Base folder to be used in creating the DAT</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
        /// <param name="bare">True if the date should be omitted from the DAT, false otherwise</param>
        /// <param name="asFiles">TreatAsFiles representing CHD and Archive scanning</param>
        /// <param name="skipFileType">Type of files that should be skipped</param>
        /// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
        /// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
        /// <param name="outDir">Output directory to </param>
        /// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <param name="useTags">True if DatFile tags override splitting, false otherwise</param>
        /// TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
        public bool PopulateFromDir(
            string basePath,
            Hash omitFromScan = Hash.DeepHashes,
            bool bare = false,
            TreatAsFiles asFiles = 0x00,
            SkipFileType skipFileType = SkipFileType.None,
            bool addBlanks = false,
            bool addDate = false,
            bool copyFiles = false,
            ExtraIni extras = null,
            Filter filter = null,
            bool useTags = false)
        {
            // If the description is defined but not the name, set the name from the description
            if (string.IsNullOrWhiteSpace(Header.Name) && !string.IsNullOrWhiteSpace(Header.Description))
            {
                Header.Name = Header.Description;
            }

            // If the name is defined but not the description, set the description from the name
            else if (!string.IsNullOrWhiteSpace(Header.Name) && string.IsNullOrWhiteSpace(Header.Description))
            {
                Header.Description = Header.Name + (bare ? string.Empty : $" ({Header.Date})");
            }

            // If neither the name or description are defined, set them from the automatic values
            else if (string.IsNullOrWhiteSpace(Header.Name) && string.IsNullOrWhiteSpace(Header.Description))
            {
                string[] splitpath = basePath.TrimEnd(Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);
                Header.Name = splitpath.Last();
                Header.Description = Header.Name + (bare ? string.Empty : $" ({Header.Date})");
            }

            // Clean the temp directory path
            Globals.TempDir = DirectoryExtensions.Ensure(Globals.TempDir, temp: true);

            // Process the input
            if (Directory.Exists(basePath))
            {
                Globals.Logger.Verbose($"Folder found: {basePath}");

                // Process the files in the main folder or any subfolder
                List<string> files = Directory.EnumerateFiles(basePath, "*", SearchOption.AllDirectories).ToList();
                Parallel.ForEach(files, Globals.ParallelOptions, item =>
                {
                    CheckFileForHashes(item, basePath, omitFromScan, asFiles, skipFileType, addBlanks, addDate, copyFiles);
                });

                // Now find all folders that are empty, if we are supposed to
                if (!(Header.OutputDepot?.IsActive ?? false) && addBlanks)
                {
                    List<string> empties = DirectoryExtensions.ListEmpty(basePath);
                    Parallel.ForEach(empties, Globals.ParallelOptions, dir =>
                    {
                        // Get the full path for the directory
                        string fulldir = Path.GetFullPath(dir);

                        // Set the temporary variables
                        string gamename = string.Empty;
                        string romname = string.Empty;

                        // If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
                        if (Header.Type == "SuperDAT")
                        {
                            gamename = fulldir.Remove(0, basePath.Length + 1);
                            romname = "_";
                        }

                        // Otherwise, we want just the top level folder as the game, and the file as everything else
                        else
                        {
                            gamename = fulldir.Remove(0, basePath.Length + 1).Split(Path.DirectorySeparatorChar)[0];
                            romname = Path.Combine(fulldir.Remove(0, basePath.Length + 1 + gamename.Length), "_");
                        }

                        // Sanitize the names
                        gamename = gamename.Trim(Path.DirectorySeparatorChar);
                        romname = romname.Trim(Path.DirectorySeparatorChar);

                        Globals.Logger.Verbose($"Adding blank empty folder: {gamename}");
                        Items["null"].Add(new Rom(romname, gamename));
                    });
                }
            }
            else if (File.Exists(basePath))
            {
                CheckFileForHashes(basePath, Path.GetDirectoryName(Path.GetDirectoryName(basePath)), omitFromScan, asFiles,
                    skipFileType, addBlanks, addDate, copyFiles);
            }

            // Now that we're done, delete the temp folder (if it's not the default)
            Globals.Logger.User("Cleaning temp folder");
            if (Globals.TempDir != Path.GetTempPath())
                DirectoryExtensions.TryDelete(Globals.TempDir);

            // If we have valid extras, perform the application now
            if (extras != null && extras != default(ExtraIni))
                ApplyExtras(extras);

            // If we have a valid filter, perform the filtering now
            if (filter != null && filter != default(Filter))
                ApplyFilter(filter, useTags);

            return true;
        }

        /// <summary>
        /// Check a given file for hashes, based on current settings
        /// </summary>
        /// <param name="item">Filename of the item to be checked</param>
        /// <param name="basePath">Base folder to be used in creating the DAT</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
        /// <param name="asFiles">TreatAsFiles representing CHD and Archive scanning</param>
        /// <param name="skipFileType">Type of files that should be skipped</param>
        /// <param name="addBlanks">True if blank items should be created for empty folders, false otherwise</param>
        /// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
        /// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
        private void CheckFileForHashes(
            string item,
            string basePath,
            Hash omitFromScan,
            TreatAsFiles asFiles,
            SkipFileType skipFileType,
            bool addBlanks,
            bool addDate,
            bool copyFiles)
        {
            // Special case for if we are in Depot mode (all names are supposed to be SHA-1 hashes)
            if (Header.OutputDepot?.IsActive ?? false)
            {
                GZipArchive gzarc = new GZipArchive(item);
                BaseFile baseFile = gzarc.GetTorrentGZFileInfo();

                // If the rom is valid, write it out
                if (baseFile != null && baseFile.Filename != null)
                {
                    // Add the list if it doesn't exist already
                    Rom rom = new Rom(baseFile);
                    Items.Add(rom.GetKey(Field.DatItem_CRC), rom);
                    Globals.Logger.User($"File added: {Path.GetFileNameWithoutExtension(item)}{Environment.NewLine}");
                }
                else
                {
                    Globals.Logger.User($"File not added: {Path.GetFileNameWithoutExtension(item)}{Environment.NewLine}");
                    return;
                }

                return;
            }

            // If we're copying files, copy it first and get the new filename
            string newItem = item;
            string newBasePath = basePath;
            if (copyFiles)
            {
                newBasePath = Path.Combine(Globals.TempDir, Guid.NewGuid().ToString());
                newItem = Path.GetFullPath(Path.Combine(newBasePath, Path.GetFullPath(item).Remove(0, basePath.Length + 1)));
                DirectoryExtensions.TryCreateDirectory(Path.GetDirectoryName(newItem));
                File.Copy(item, newItem, true);
            }

            // Initialize possible archive variables
            BaseArchive archive = BaseArchive.Create(newItem);
            List<BaseFile> extracted = null;

            // If we have an archive and we're supposed to scan it
            if (archive != null && !asFiles.HasFlag(TreatAsFiles.Archives))
                extracted = archive.GetChildren(omitFromScan: omitFromScan, date: addDate);

            // If the file should be skipped based on type, do so now
            if ((extracted != null && skipFileType == SkipFileType.Archive)
                || (extracted == null && skipFileType == SkipFileType.File))
            {
                return;
            }

            // If the extracted list is null, just scan the item itself
            if (extracted == null)
            {
                ProcessFile(newItem, newBasePath, omitFromScan, addDate, asFiles);
            }

            // Otherwise, add all of the found items
            else
            {
                // First take care of the found items
                Parallel.ForEach(extracted, Globals.ParallelOptions, rom =>
                {
                    DatItem datItem = DatItem.Create(rom);
                    ProcessFileHelper(newItem,
                        datItem,
                        basePath,
                        (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item));
                });

                // Then, if we're looking for blanks, get all of the blank folders and add them
                if (addBlanks)
                {
                    List<string> empties = new List<string>();

                    // Now get all blank folders from the archive
                    if (archive != null)
                        empties = archive.GetEmptyFolders();

                    // Add add all of the found empties to the DAT
                    Parallel.ForEach(empties, Globals.ParallelOptions, empty =>
                    {
                        Rom emptyRom = new Rom(Path.Combine(empty, "_"), newItem);
                        ProcessFileHelper(newItem,
                            emptyRom,
                            basePath,
                            (Path.GetDirectoryName(Path.GetFullPath(item)) + Path.DirectorySeparatorChar).Remove(0, basePath.Length) + Path.GetFileNameWithoutExtension(item));
                    });
                }
            }

            // Cue to delete the file if it's a copy
            if (copyFiles && item != newItem)
                DirectoryExtensions.TryDelete(newBasePath);
        }

        /// <summary>
        /// Process a single file as a file
        /// </summary>
        /// <param name="item">File to be added</param>
        /// <param name="basePath">Path the represents the parent directory</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
        /// <param name="addDate">True if dates should be archived for all files, false otherwise</param>
        /// <param name="asFiles">TreatAsFiles representing CHD and Archive scanning</param>
        private void ProcessFile(string item, string basePath, Hash omitFromScan, bool addDate, TreatAsFiles asFiles)
        {
            Globals.Logger.Verbose($"'{Path.GetFileName(item)}' treated like a file");
            BaseFile baseFile = FileExtensions.GetInfo(item, omitFromScan: omitFromScan, date: addDate, header: Header.HeaderSkipper, chdsAsFiles: asFiles.HasFlag(TreatAsFiles.CHDs));
            ProcessFileHelper(item, DatItem.Create(baseFile), basePath, string.Empty);
        }

        /// <summary>
        /// Process a single file as a file (with found Rom data)
        /// </summary>
        /// <param name="item">File to be added</param>
        /// <param name="item">Rom data to be used to write to file</param>
        /// <param name="basepath">Path the represents the parent directory</param>
        /// <param name="parent">Parent game to be used</param>
        private void ProcessFileHelper(string item, DatItem datItem, string basepath, string parent)
        {
            // If we somehow got something other than a Rom or Disk, cancel out
            if (datItem.ItemType != ItemType.Rom && datItem.ItemType != ItemType.Disk)
                return;

            try
            {
                // If the basepath doesn't end with a directory separator, add it
                if (!basepath.EndsWith(Path.DirectorySeparatorChar.ToString()))
                    basepath += Path.DirectorySeparatorChar.ToString();

                // Make sure we have the full item path
                item = Path.GetFullPath(item);

                // Process the item to sanitize names based on input
                SetDatItemInfo(datItem, item, parent, basepath);

                // Add the file information to the DAT
                string key = datItem.GetKey(Field.DatItem_CRC);
                Items.Add(key, datItem);

                Globals.Logger.User($"File added: {datItem.Name}{Environment.NewLine}");
            }
            catch (IOException ex)
            {
                Globals.Logger.Error(ex.ToString());
                return;
            }
        }

        /// <summary>
        /// Set proper Game and Rom names from user inputs
        /// </summary>
        /// <param name="datItem">DatItem representing the input file</param>
        /// <param name="item">Item name to use</param>
        /// <param name="parent">Parent name to use</param>
        /// <param name="basepath">Base path to use</param>
        private void SetDatItemInfo(DatItem datItem, string item, string parent, string basepath)
        {
            // Get the data to be added as game and item names
            string gamename, romname;

            // If the parent is blank, then we have a non-archive file
            if (string.IsNullOrWhiteSpace(parent))
            {
                // If we have a SuperDAT, we want anything that's not the base path as the game, and the file as the rom
                if (Header.Type == "SuperDAT")
                {
                    gamename = Path.GetDirectoryName(item.Remove(0, basepath.Length));
                    romname = Path.GetFileName(item);
                }

                // Otherwise, we want just the top level folder as the game, and the file as everything else
                else
                {
                    gamename = item.Remove(0, basepath.Length).Split(Path.DirectorySeparatorChar)[0];
                    romname = item.Remove(0, (Path.Combine(basepath, gamename).Length));
                }
            }

            // Otherwise, we assume that we have an archive
            else
            {
                // If we have a SuperDAT, we want the archive name as the game, and the file as everything else (?)
                if (Header.Type == "SuperDAT")
                {
                    gamename = parent;
                    romname = datItem.Name;
                }

                // Otherwise, we want the archive name as the game, and the file as everything else
                else
                {
                    gamename = parent;
                    romname = datItem.Name;
                }
            }

            // Sanitize the names
            gamename = gamename.Trim(Path.DirectorySeparatorChar);
            romname = romname?.Trim(Path.DirectorySeparatorChar) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(gamename) && string.IsNullOrWhiteSpace(romname))
            {
                romname = gamename;
                gamename = "Default";
            }

            // Update rom information
            datItem.Name = romname;
            datItem.Machine.Name = gamename;
            datItem.Machine.Description = gamename;

            // If we have a Disk, then the ".chd" extension needs to be removed
            if (datItem.ItemType == ItemType.Disk)
                datItem.Name = datItem.Name.Replace(".chd", string.Empty);
        }

        #endregion

        #region Rebuilding and Verifying

        /// <summary>
        /// Process the DAT and find all matches in input files and folders assuming they're a depot
        /// </summary>
        /// <param name="inputs">List of input files/folders to check</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="delete">True if input files should be deleted, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
        /// <returns>True if rebuilding was a success, false otherwise</returns>
        public bool RebuildDepot(
            List<string> inputs,
            string outDir = null,
            bool date = false,
            bool delete = false,
            bool inverse = false,
            OutputFormat outputFormat = OutputFormat.Folder,
            bool updateDat = true)
        {
            #region Perform setup

            // If the DAT is not populated and inverse is not set, inform the user and quit
            if (Items.TotalCount == 0 && !inverse)
            {
                Globals.Logger.User("No entries were found to rebuild, exiting...");
                return false;
            }

            // Check that the output directory exists
            outDir = DirectoryExtensions.Ensure(outDir, create: true);

            // Now we want to get forcepack flag if it's not overridden
            if (outputFormat == OutputFormat.Folder && Header.ForcePacking != PackingFlag.None)
            {
                switch (Header.ForcePacking)
                {
                    case PackingFlag.Zip:
                        outputFormat = OutputFormat.TorrentZip;
                        break;
                    case PackingFlag.Unzip:
                        outputFormat = OutputFormat.Folder;
                        break;
                }
            }

            // Preload the Skipper list
            Transform.Init();

            #endregion

            bool success = true;

            #region Rebuild from depots in order

            string format = string.Empty;
            switch (outputFormat)
            {
                case OutputFormat.Folder:
                    format = "directory";
                    break;
                case OutputFormat.TapeArchive:
                    format = "TAR";
                    break;
                case OutputFormat.Torrent7Zip:
                    format = "Torrent7Z";
                    break;
                case OutputFormat.TorrentGzip:
                case OutputFormat.TorrentGzipRomba:
                    format = "TorrentGZ";
                    break;
                case OutputFormat.TorrentLRZip:
                    format = "TorrentLRZ";
                    break;
                case OutputFormat.TorrentRar:
                    format = "TorrentRAR";
                    break;
                case OutputFormat.TorrentXZ:
                case OutputFormat.TorrentXZRomba:
                    format = "TorrentXZ";
                    break;
                case OutputFormat.TorrentZip:
                    format = "TorrentZip";
                    break;
            }

            InternalStopwatch watch = new InternalStopwatch($"Rebuilding all files to {format}");

            // Now loop through and get only directories from the input paths
            List<string> directories = new List<string>();
            Parallel.ForEach(inputs, Globals.ParallelOptions, input =>
            {
                // Add to the list if the input is a directory
                if (Directory.Exists(input))
                {
                    Globals.Logger.Verbose($"Adding depot: {input}");
                    lock (directories)
                    {
                        directories.Add(input);
                    }
                }
            });

            // If we don't have any directories, we want to exit
            if (directories.Count == 0)
                return success;

            // Now that we have a list of depots, we want to bucket the input DAT by SHA-1
            Items.BucketBy(Field.DatItem_SHA1, DedupeType.None);

            // Then we want to loop through each of the hashes and see if we can rebuild
            var keys = Items.SortedKeys.ToList();
            foreach (string hash in keys)
            {
                // Pre-empt any issues that could arise from string length
                if (hash.Length != Constants.SHA1Length)
                    continue;

                Globals.Logger.User($"Checking hash '{hash}'");

                // Get the extension path for the hash
                string subpath = PathExtensions.GetDepotPath(hash, Header.InputDepot.Depth);

                // Find the first depot that includes the hash
                string foundpath = null;
                foreach (string directory in directories)
                {
                    if (File.Exists(Path.Combine(directory, subpath)))
                    {
                        foundpath = Path.Combine(directory, subpath);
                        break;
                    }
                }

                // If we didn't find a path, then we continue
                if (foundpath == null)
                    continue;

                // If we have a path, we want to try to get the rom information
                GZipArchive archive = new GZipArchive(foundpath);
                BaseFile fileinfo = archive.GetTorrentGZFileInfo();

                // If the file information is null, then we continue
                if (fileinfo == null)
                    continue;

                // If there are no items in the hash, we continue
                if (Items[hash] == null || Items[hash].Count == 0)
                    continue;

                // Otherwise, we rebuild that file to all locations that we need to
                bool usedInternally;
                if (Items[hash][0].ItemType == ItemType.Disk)
                    usedInternally = RebuildIndividualFile(new Disk(fileinfo), foundpath, outDir, date, inverse, outputFormat, updateDat, false /* isZip */);
                else
                    usedInternally = RebuildIndividualFile(new Rom(fileinfo), foundpath, outDir, date, inverse, outputFormat, updateDat, false /* isZip */);

                // If we are supposed to delete the depot file, do so
                if (delete && usedInternally)
                    FileExtensions.TryDelete(foundpath);
            }

            watch.Stop();

            #endregion

            // If we're updating the DAT, output to the rebuild directory
            if (updateDat)
            {
                Header.FileName = $"fixDAT_{Header.FileName}";
                Header.Name = $"fixDAT_{Header.Name}";
                Header.Description = $"fixDAT_{Header.Description}";
                Items.ClearMarked();
                Write(outDir);
            }

            return success;
        }

        /// <summary>
        /// Process the DAT and find all matches in input files and folders
        /// </summary>
        /// <param name="inputs">List of input files/folders to check</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="delete">True if input files should be deleted, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
        /// <param name="asFiles">TreatAsFiles representing CHD and Archive scanning</param>
        /// <returns>True if rebuilding was a success, false otherwise</returns>
        public bool RebuildGeneric(
            List<string> inputs,
            string outDir = null,
            bool quickScan = false,
            bool date = false,
            bool delete = false,
            bool inverse = false,
            OutputFormat outputFormat = OutputFormat.Folder,
            bool updateDat = true,
            TreatAsFiles asFiles = 0x00)
        {
            #region Perform setup

            // If the DAT is not populated and inverse is not set, inform the user and quit
            if (Items.TotalCount == 0 && !inverse)
            {
                Globals.Logger.User("No entries were found to rebuild, exiting...");
                return false;
            }

            // Check that the output directory exists
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
                outDir = Path.GetFullPath(outDir);
            }

            // Now we want to get forcepack flag if it's not overridden
            if (outputFormat == OutputFormat.Folder && Header.ForcePacking != PackingFlag.None)
            {
                switch (Header.ForcePacking)
                {
                    case PackingFlag.Zip:
                        outputFormat = OutputFormat.TorrentZip;
                        break;
                    case PackingFlag.Unzip:
                        outputFormat = OutputFormat.Folder;
                        break;
                }
            }

            // Preload the Skipper list
            Transform.Init();

            #endregion

            bool success = true;

            #region Rebuild from sources in order

            string format = string.Empty;
            switch (outputFormat)
            {
                case OutputFormat.Folder:
                    format = "directory";
                    break;
                case OutputFormat.TapeArchive:
                    format = "TAR";
                    break;
                case OutputFormat.Torrent7Zip:
                    format = "Torrent7Z";
                    break;
                case OutputFormat.TorrentGzip:
                case OutputFormat.TorrentGzipRomba:
                    format = "TorrentGZ";
                    break;
                case OutputFormat.TorrentLRZip:
                    format = "TorrentLRZ";
                    break;
                case OutputFormat.TorrentRar:
                    format = "TorrentRAR";
                    break;
                case OutputFormat.TorrentXZ:
                case OutputFormat.TorrentXZRomba:
                    format = "TorrentXZ";
                    break;
                case OutputFormat.TorrentZip:
                    format = "TorrentZip";
                    break;
            }

            InternalStopwatch watch = new InternalStopwatch($"Rebuilding all files to {format}");

            // Now loop through all of the files in all of the inputs
            foreach (string input in inputs)
            {
                // If the input is a file
                if (File.Exists(input))
                {
                    Globals.Logger.User($"Checking file: {input}");
                    RebuildGenericHelper(input, outDir, quickScan, date, delete, inverse, outputFormat, updateDat, asFiles);
                }

                // If the input is a directory
                else if (Directory.Exists(input))
                {
                    Globals.Logger.Verbose($"Checking directory: {input}");
                    foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
                    {
                        Globals.Logger.User($"Checking file: {file}");
                        RebuildGenericHelper(file, outDir, quickScan, date, delete, inverse, outputFormat, updateDat, asFiles);
                    }
                }
            }

            watch.Stop();

            #endregion

            // If we're updating the DAT, output to the rebuild directory
            if (updateDat)
            {
                Header.FileName = $"fixDAT_{Header.FileName}";
                Header.Name = $"fixDAT_{Header.Name}";
                Header.Description = $"fixDAT_{Header.Description}";
                Items.ClearMarked();
                Write(outDir);
            }

            return success;
        }

        /// <summary>
        /// Attempt to add a file to the output if it matches
        /// </summary>
        /// <param name="file">Name of the file to process</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="delete">True if input files should be deleted, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
        /// <param name="asFiles">TreatAsFiles representing CHD and Archive scanning</param>
        private void RebuildGenericHelper(
            string file,
            string outDir,
            bool quickScan,
            bool date,
            bool delete,
            bool inverse,
            OutputFormat outputFormat,
            bool updateDat,
            TreatAsFiles asFiles)
        {
            // If we somehow have a null filename, return
            if (file == null)
                return;

            // Set the deletion variables
            bool usedExternally, usedInternally = false;

            // Scan the file externally

            // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
            BaseFile externalFileInfo = FileExtensions.GetInfo(file, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes),
                header: Header.HeaderSkipper, chdsAsFiles: asFiles.HasFlag(TreatAsFiles.CHDs));

            DatItem externalDatItem = null;
            if (externalFileInfo.Type == FileType.CHD)
                externalDatItem = new Disk(externalFileInfo);
            else if (externalFileInfo.Type == FileType.None)
                externalDatItem = new Rom(externalFileInfo);

            usedExternally = RebuildIndividualFile(externalDatItem, file, outDir, date, inverse, outputFormat, updateDat, null /* isZip */);

            // Scan the file internally

            // Create an empty list of BaseFile for archive entries
            List<BaseFile> entries = null;

            // Get the TGZ status for later
            GZipArchive tgz = new GZipArchive(file);
            bool isTorrentGzip = tgz.IsTorrent();

            // Get the base archive first
            BaseArchive archive = BaseArchive.Create(file);

            // Now get all extracted items from the archive
            if (archive != null)
            {
                // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                entries = archive.GetChildren(omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes), date: date);
            }

            // If the entries list is null, we encountered an error and should scan exteranlly
            if (entries == null && File.Exists(file))
            {
                // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                BaseFile internalFileInfo = FileExtensions.GetInfo(file, omitFromScan: (quickScan ? Hash.SecureHashes : Hash.DeepHashes), chdsAsFiles: asFiles.HasFlag(TreatAsFiles.CHDs));

                DatItem internalDatItem = null;
                if (internalFileInfo.Type == FileType.CHD)
                    internalDatItem = new Disk(internalFileInfo);
                else if (internalFileInfo.Type == FileType.None)
                    internalDatItem = new Rom(internalFileInfo);

                usedExternally = RebuildIndividualFile(internalDatItem, file, outDir, date, inverse, outputFormat, updateDat, null /* isZip */);
            }
            // Otherwise, loop through the entries and try to match
            else
            {
                foreach (BaseFile entry in entries)
                {
                    DatItem internalDatItem = DatItem.Create(entry);
                    usedInternally |= RebuildIndividualFile(internalDatItem, file, outDir, date, inverse, outputFormat, updateDat, !isTorrentGzip /* isZip */);
                }
            }

            // If we are supposed to delete the file, do so
            if (delete && (usedExternally || usedInternally))
                FileExtensions.TryDelete(file);
        }

        /// <summary>
        /// Find duplicates and rebuild individual files to output
        /// </summary>
        /// <param name="datItem">Information for the current file to rebuild from</param>
        /// <param name="file">Name of the file to process</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
        /// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
        /// <param name="outputFormat">Output format that files should be written to</param>
        /// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
        /// <param name="isZip">True if the input file is an archive, false if the file is TGZ, null otherwise</param>
        /// <returns>True if the file was able to be rebuilt, false otherwise</returns>
        private bool RebuildIndividualFile(
            DatItem datItem,
            string file,
            string outDir,
            bool date,
            bool inverse,
            OutputFormat outputFormat,
            bool updateDat,
            bool? isZip)
        {
            // Set the initial output value
            bool rebuilt = false;

            // If the DatItem is a Disk, force rebuilding to a folder except if TGZ or TXZ
            if (datItem.ItemType == ItemType.Disk
                && !(outputFormat == OutputFormat.TorrentGzip || outputFormat == OutputFormat.TorrentGzipRomba)
                && !(outputFormat == OutputFormat.TorrentXZ || outputFormat == OutputFormat.TorrentXZRomba))
            {
                outputFormat = OutputFormat.Folder;
            }

            // If we have a disk, change it into a Rom for later use
            if (datItem.ItemType == ItemType.Disk)
                datItem = ((Disk)datItem).ConvertToRom();

            // Prepopluate a few key strings
            string crc = ((Rom)datItem).CRC ?? string.Empty;
            string sha1 = ((Rom)datItem).SHA1 ?? string.Empty;

            // Find if the file has duplicates in the DAT
            List<DatItem> dupes = Items.GetDuplicates(datItem, remove: updateDat);
            bool hasDuplicates = dupes.Count > 0;

            // If either we have duplicates or we're filtering
            if (hasDuplicates ^ inverse)
            {
                // If we have a very specific TGZ->TGZ case, just copy it accordingly
                GZipArchive tgz = new GZipArchive(file);
                BaseFile tgzRom = tgz.GetTorrentGZFileInfo();
                if (isZip == false && tgzRom != null && (outputFormat == OutputFormat.TorrentGzip || outputFormat == OutputFormat.TorrentGzipRomba))
                {
                    Globals.Logger.User($"Matches found for '{Path.GetFileName(datItem.Name)}', rebuilding accordingly...");

                    // Get the proper output path
                    if (outputFormat == OutputFormat.TorrentGzipRomba)
                        outDir = Path.Combine(outDir, PathExtensions.GetDepotPath(sha1, Header.OutputDepot.Depth));
                    else
                        outDir = Path.Combine(outDir, sha1 + ".gz");

                    // Make sure the output folder is created
                    Directory.CreateDirectory(Path.GetDirectoryName(outDir));

                    // Now copy the file over
                    try
                    {
                        File.Copy(file, outDir);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                // If we have a very specific TXZ->TXZ case, just copy it accordingly
                XZArchive txz = new XZArchive(file);
                BaseFile txzRom = txz.GetTorrentXZFileInfo();
                if (isZip == false && txzRom != null && (outputFormat == OutputFormat.TorrentXZ || outputFormat == OutputFormat.TorrentXZRomba))
                {
                    Globals.Logger.User($"Matches found for '{Path.GetFileName(datItem.Name)}', rebuilding accordingly...");

                    // Get the proper output path
                    if (outputFormat == OutputFormat.TorrentXZRomba)
                        outDir = Path.Combine(outDir, PathExtensions.GetDepotPath(sha1, Header.OutputDepot.Depth));
                    else
                        outDir = Path.Combine(outDir, sha1 + ".xz");

                    // Make sure the output folder is created
                    Directory.CreateDirectory(Path.GetDirectoryName(outDir));

                    // Now copy the file over
                    try
                    {
                        File.Copy(file, outDir);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                // Get a generic stream for the file
                Stream fileStream = new MemoryStream();

                // If we have a zipfile, extract the stream to memory
                if (isZip != null)
                {
                    BaseArchive archive = BaseArchive.Create(file);
                    if (archive != null)
                        (fileStream, _) = archive.CopyToStream(datItem.Name);
                }
                // Otherwise, just open the filestream
                else
                {
                    fileStream = FileExtensions.TryOpenRead(file);
                }

                // If the stream is null, then continue
                if (fileStream == null)
                    return false;

                // Seek to the beginning of the stream
                if (fileStream.CanSeek)
                    fileStream.Seek(0, SeekOrigin.Begin);

                // If we are inverse, create an output to rebuild to
                if (inverse)
                {
                    string machinename = null;

                    // Get the item from the current file
                    Rom item = new Rom(fileStream.GetInfo(keepReadOpen: true));
                    item.Machine.Name = Path.GetFileNameWithoutExtension(item.Name);
                    item.Machine.Description = Path.GetFileNameWithoutExtension(item.Name);

                    // If we are coming from an archive, set the correct machine name
                    if (machinename != null)
                    {
                        item.Machine.Name = machinename;
                        item.Machine.Description = machinename;
                    }

                    dupes.Add(item);
                }

                Globals.Logger.User($"{(inverse ? "No matches" : "Matches")} found for '{Path.GetFileName(datItem.Name)}', rebuilding accordingly...");
                rebuilt = true;

                // Now loop through the list and rebuild accordingly
                foreach (DatItem item in dupes)
                {
                    // Get the output archive, if possible
                    Folder outputArchive = Folder.Create(outputFormat);

                    // Now rebuild to the output file
                    outputArchive.Write(fileStream, outDir, (Rom)item, date: date, depth: Header.OutputDepot.Depth);
                }

                // Close the input stream
                fileStream?.Dispose();
            }

            // Now we want to take care of headers, if applicable
            if (Header.HeaderSkipper != null)
            {
                // Get a generic stream for the file
                Stream fileStream = new MemoryStream();

                // If we have a zipfile, extract the stream to memory
                if (isZip != null)
                {
                    BaseArchive archive = BaseArchive.Create(file);
                    if (archive != null)
                        (fileStream, _) = archive.CopyToStream(datItem.Name);
                }
                // Otherwise, just open the filestream
                else
                {
                    fileStream = FileExtensions.TryOpenRead(file);
                }

                // If the stream is null, then continue
                if (fileStream == null)
                    return false;

                // Check to see if we have a matching header first
                SkipperRule rule = Transform.GetMatchingRule(fileStream, Path.GetFileNameWithoutExtension(Header.HeaderSkipper));

                // If there's a match, create the new file to write
                if (rule.Tests != null && rule.Tests.Count != 0)
                {
                    // If the file could be transformed correctly
                    MemoryStream transformStream = new MemoryStream();
                    if (rule.TransformStream(fileStream, transformStream, keepReadOpen: true, keepWriteOpen: true))
                    {
                        // Get the file informations that we will be using
                        Rom headerless = new Rom(transformStream.GetInfo(keepReadOpen: true));

                        // Find if the file has duplicates in the DAT
                        dupes = Items.GetDuplicates(headerless, remove: updateDat);
                        hasDuplicates = dupes.Count > 0;

                        // If it has duplicates and we're not filtering, rebuild it
                        if (hasDuplicates && !inverse)
                        {
                            Globals.Logger.User($"Headerless matches found for '{Path.GetFileName(datItem.Name)}', rebuilding accordingly...");
                            rebuilt = true;

                            // Now loop through the list and rebuild accordingly
                            foreach (DatItem item in dupes)
                            {
                                // Create a headered item to use as well
                                datItem.CopyMachineInformation(item);
                                datItem.Name += $"_{crc}";

                                // If either copy succeeds, then we want to set rebuilt to true
                                bool eitherSuccess = false;

                                // Get the output archive, if possible
                                Folder outputArchive = Folder.Create(outputFormat);

                                // Now rebuild to the output file
                                eitherSuccess |= outputArchive.Write(transformStream, outDir, (Rom)item, date: date, depth: Header.OutputDepot.Depth);
                                eitherSuccess |= outputArchive.Write(fileStream, outDir, (Rom)datItem, date: date, depth: Header.OutputDepot.Depth);

                                // Now add the success of either rebuild
                                rebuilt &= eitherSuccess;
                            }
                        }
                    }

                    // Dispose of the stream
                    transformStream?.Dispose();
                }

                // Dispose of the stream
                fileStream?.Dispose();
            }

            return rebuilt;
        }

        /// <summary>
        /// Process the DAT and verify from the depots
        /// </summary>
        /// <param name="inputs">List of input directories to compare against</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <returns>True if verification was a success, false otherwise</returns>
        public bool VerifyDepot(List<string> inputs, string outDir = null)
        {
            bool success = true;

            InternalStopwatch watch = new InternalStopwatch("Verifying all from supplied depots");

            // Now loop through and get only directories from the input paths
            List<string> directories = new List<string>();
            foreach (string input in inputs)
            {
                // Add to the list if the input is a directory
                if (Directory.Exists(input))
                {
                    Globals.Logger.Verbose($"Adding depot: {input}");
                    directories.Add(input);
                }
            }

            // If we don't have any directories, we want to exit
            if (directories.Count == 0)
                return success;

            // Now that we have a list of depots, we want to bucket the input DAT by SHA-1
            Items.BucketBy(Field.DatItem_SHA1, DedupeType.None);

            // Then we want to loop through each of the hashes and see if we can rebuild
            var keys = Items.SortedKeys.ToList();
            foreach (string hash in keys)
            {
                // Pre-empt any issues that could arise from string length
                if (hash.Length != Constants.SHA1Length)
                    continue;

                Globals.Logger.User($"Checking hash '{hash}'");

                // Get the extension path for the hash
                string subpath = PathExtensions.GetDepotPath(hash, Header.InputDepot.Depth);

                // Find the first depot that includes the hash
                string foundpath = null;
                foreach (string directory in directories)
                {
                    if (File.Exists(Path.Combine(directory, subpath)))
                    {
                        foundpath = Path.Combine(directory, subpath);
                        break;
                    }
                }

                // If we didn't find a path, then we continue
                if (foundpath == null)
                    continue;

                // If we have a path, we want to try to get the rom information
                GZipArchive tgz = new GZipArchive(foundpath);
                BaseFile fileinfo = tgz.GetTorrentGZFileInfo();

                // If the file information is null, then we continue
                if (fileinfo == null)
                    continue;

                // Now we want to remove all duplicates from the DAT
                Items.GetDuplicates(new Rom(fileinfo), remove: true)
                    .AddRange(Items.GetDuplicates(new Disk(fileinfo), remove: true));
            }

            watch.Stop();

            // If there are any entries in the DAT, output to the rebuild directory
            Header.FileName = $"fixDAT_{Header.FileName}";
            Header.Name = $"fixDAT_{Header.Name}";
            Header.Description = $"fixDAT_{Header.Description}";
            Items.ClearMarked();
            Write(outDir, stats: true);

            return success;
        }

        /// <summary>
        /// Process the DAT and verify the output directory
        /// </summary>
        /// <param name="inputs">List of input directories to compare against</param>
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
        /// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
        /// <param name="asFiles">TreatAsFiles representing CHD and Archive scanning</param>
        /// <param name="extras">ExtraIni object to apply to the DatFile</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// <returns>True if verification was a success, false otherwise</returns>
        public bool VerifyGeneric(
            List<string> inputs,
            string outDir = null,
            bool hashOnly = false,
            bool quickScan = false,
            TreatAsFiles asFiles = 0x00,
            ExtraIni extras = null,
            Filter filter = null)
        {
            // TODO: We want the cross section of what's the folder and what's in the DAT. Right now, it just has what's in the DAT that's not in the folder
            bool success = true;

            // Then, loop through and check each of the inputs
            Globals.Logger.User("Processing files:\n");
            foreach (string input in inputs)
            {
                // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                PopulateFromDir(
                    input,
                    quickScan ? Hash.SecureHashes : Hash.DeepHashes,
                    bare: true,
                    asFiles: asFiles,
                    extras: extras,
                    filter: filter);
            }

            // Setup the fixdat
            DatFile matched = Create(Header);
            matched.Items = new ItemDictionary();
            matched.Header.FileName = $"fixDat_{matched.Header.FileName}";
            matched.Header.Name = $"fixDat_{matched.Header.Name}";
            matched.Header.Description = $"fixDat_{matched.Header.Description}";
            matched.Header.DatFormat = DatFormat.Logiqx;

            // If we are checking hashes only, essentially diff the inputs
            if (hashOnly)
            {
                // First we need to bucket and dedupe by hash to get duplicates
                Items.BucketBy(Field.DatItem_CRC, DedupeType.Full);

                // Then follow the same tactics as before
                var keys = Items.SortedKeys.ToList();
                foreach (string key in keys)
                {
                    List<DatItem> roms = Items[key];
                    foreach (DatItem rom in roms)
                    {
                        if (rom.Source.Index == 99)
                        {
                            if (rom.ItemType == ItemType.Disk || rom.ItemType == ItemType.Rom)
                                matched.Items.Add(((Disk)rom).SHA1, rom);
                        }
                    }
                }
            }
            // If we are checking full names, get only files found in directory
            else
            {
                foreach (string key in Items.Keys)
                {
                    List<DatItem> roms = Items[key];
                    List<DatItem> newroms = DatItem.Merge(roms);
                    foreach (Rom rom in newroms)
                    {
                        if (rom.Source.Index == 99)
                            matched.Items.Add($"{rom.Size}-{rom.CRC}", rom);
                    }
                }
            }

            // Now output the fixdat to the main folder
            Items.ClearMarked();
            success &= matched.Write(outDir, stats: true);

            return success;
        }

        #endregion

        // TODO: Implement Level split
        #region Splitting

        /// <summary>
        /// Split a DAT by input extensions
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <param name="extA">List of extensions to split on (first DAT)</param>
        /// <param name="extB">List of extensions to split on (second DAT)</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        public bool SplitByExtension(string outDir, List<string> extA, List<string> extB)
        {
            // If roms is empty, return false
            if (Items.TotalCount == 0)
                return false;

            // Make sure all of the extensions don't have a dot at the beginning
            var newExtA = extA.Select(s => s.TrimStart('.').ToLowerInvariant());
            string newExtAString = string.Join(",", newExtA);

            var newExtB = extB.Select(s => s.TrimStart('.').ToLowerInvariant());
            string newExtBString = string.Join(",", newExtB);

            // Set all of the appropriate outputs for each of the subsets
            DatFile datdataA = Create(Header.CloneStandard());
            datdataA.Header.FileName += $" ({newExtAString})";
            datdataA.Header.Name += $" ({newExtAString})";
            datdataA.Header.Description += $" ({newExtAString})";

            DatFile datdataB = Create(Header.CloneStandard());
            datdataB.Header.FileName += $" ({newExtBString})";
            datdataB.Header.Name += $" ({newExtBString})";
            datdataB.Header.Description += $" ({newExtBString})";

            // Now separate the roms accordingly
            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = Items[key];
                foreach (DatItem item in items)
                {
                    if (newExtA.Contains(PathExtensions.GetNormalizedExtension(item.Name)))
                    {
                        datdataA.Items.Add(key, item);
                    }
                    else if (newExtB.Contains(PathExtensions.GetNormalizedExtension(item.Name)))
                    {
                        datdataB.Items.Add(key, item);
                    }
                    else
                    {
                        datdataA.Items.Add(key, item);
                        datdataB.Items.Add(key, item);
                    }
                }
            });

            // Then write out both files
            bool success = datdataA.Write(outDir);
            success &= datdataB.Write(outDir);

            return success;
        }

        /// <summary>
        /// Split a DAT by best available hashes
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        public bool SplitByHash(string outDir)
        {
            // Create each of the respective output DATs
            Globals.Logger.User("Creating and populating new DATs");

            DatFile nodump = Create(Header.CloneStandard());
            nodump.Header.FileName += " (Nodump)";
            nodump.Header.Name += " (Nodump)";
            nodump.Header.Description += " (Nodump)";

            DatFile sha512 = Create(Header.CloneStandard());
            sha512.Header.FileName += " (SHA-512)";
            sha512.Header.Name += " (SHA-512)";
            sha512.Header.Description += " (SHA-512)";

            DatFile sha384 = Create(Header.CloneStandard());
            sha384.Header.FileName += " (SHA-384)";
            sha384.Header.Name += " (SHA-384)";
            sha384.Header.Description += " (SHA-384)";

            DatFile sha256 = Create(Header.CloneStandard());
            sha256.Header.FileName += " (SHA-256)";
            sha256.Header.Name += " (SHA-256)";
            sha256.Header.Description += " (SHA-256)";

            DatFile sha1 = Create(Header.CloneStandard());
            sha1.Header.FileName += " (SHA-1)";
            sha1.Header.Name += " (SHA-1)";
            sha1.Header.Description += " (SHA-1)";

#if NET_FRAMEWORK
            DatFile ripemd160 = Create(Header.CloneStandard());
            ripemd160.Header.FileName += " (RIPEMD160)";
            ripemd160.Header.Name += " (RIPEMD160)";
            ripemd160.Header.Description += " (RIPEMD160)";
#endif

            DatFile md5 = Create(Header.CloneStandard());
            md5.Header.FileName += " (MD5)";
            md5.Header.Name += " (MD5)";
            md5.Header.Description += " (MD5)";

            DatFile crc = Create(Header.CloneStandard());
            crc.Header.FileName += " (CRC)";
            crc.Header.Name += " (CRC)";
            crc.Header.Description += " (CRC)";

            DatFile other = Create(Header.CloneStandard());
            other.Header.FileName += " (Other)";
            other.Header.Name += " (Other)";
            other.Header.Description += " (Other)";

            // Now populate each of the DAT objects in turn
            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = Items[key];
                foreach (DatItem item in items)
                {
                    // If the file is not a Rom or Disk, continue
                    if (item.ItemType != ItemType.Disk && item.ItemType != ItemType.Rom)
                        return;

                    // If the file is a nodump
                    if ((item.ItemType == ItemType.Rom && ((Rom)item).ItemStatus == ItemStatus.Nodump)
                        || (item.ItemType == ItemType.Disk && ((Disk)item).ItemStatus == ItemStatus.Nodump))
                    {
                        nodump.Items.Add(key, item);
                    }
                    // If the file has a SHA-512
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).SHA512))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).SHA512)))
                    {
                        sha512.Items.Add(key, item);
                    }
                    // If the file has a SHA-384
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).SHA384))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).SHA384)))
                    {
                        sha384.Items.Add(key, item);
                    }
                    // If the file has a SHA-256
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).SHA256))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).SHA256)))
                    {
                        sha256.Items.Add(key, item);
                    }
                    // If the file has a SHA-1
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).SHA1))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).SHA1)))
                    {
                        sha1.Items.Add(key, item);
                    }
#if NET_FRAMEWORK
                    // If the file has a RIPEMD160
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).RIPEMD160))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).RIPEMD160)))
                    {
                        ripemd160.Items.Add(key, item);
                    }
#endif
                    // If the file has an MD5
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).MD5))
                        || (item.ItemType == ItemType.Disk && !string.IsNullOrWhiteSpace(((Disk)item).MD5)))
                    {
                        md5.Items.Add(key, item);
                    }
                    // If the file has a CRC
                    else if ((item.ItemType == ItemType.Rom && !string.IsNullOrWhiteSpace(((Rom)item).CRC)))
                    {
                        crc.Items.Add(key, item);
                    }
                    else
                    {
                        other.Items.Add(key, item);
                    }
                }
            });

            // Now, output all of the files to the output directory
            Globals.Logger.User("DAT information created, outputting new files");
            bool success = true;
            success &= nodump.Write(outDir);
            success &= sha512.Write(outDir);
            success &= sha384.Write(outDir);
            success &= sha256.Write(outDir);
            success &= sha1.Write(outDir);
#if NET_FRAMEWORK
            success &= ripemd160.Write(outDir);
#endif
            success &= md5.Write(outDir);
            success &= crc.Write(outDir);

            return success;
        }

        /// <summary>
        /// Split a SuperDAT by lowest available directory level
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <param name="shortname">True if short names should be used, false otherwise</param>
        /// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        public bool SplitByLevel(string outDir, bool shortname, bool basedat)
        {
            // First, bucket by games so that we can do the right thing
            Items.BucketBy(Field.Machine_Name, DedupeType.None, lower: false, norename: true);

            // Create a temporary DAT to add things to
            DatFile tempDat = Create(Header);
            tempDat.Header.Name = null;

            // Sort the input keys
            List<string> keys = Items.Keys.ToList();
            keys.Sort(SplitByLevelSort);

            // Then, we loop over the games
            Parallel.ForEach(keys, Globals.ParallelOptions, key =>
            {
                // Here, the key is the name of the game to be used for comparison
                if (tempDat.Header.Name != null && tempDat.Header.Name != Path.GetDirectoryName(key))
                {
                    // Reset the DAT for the next items
                    tempDat = Create(Header);
                    tempDat.Header.Name = null;
                }

                // Clean the input list and set all games to be pathless
                List<DatItem> items = Items[key];
                items.ForEach(item => item.Machine.Name = Path.GetFileName(item.Machine.Name));
                items.ForEach(item => item.Machine.Description = Path.GetFileName(item.Machine.Description));

                // Now add the game to the output DAT
                tempDat.Items.AddRange(key, items);

                // Then set the DAT name to be the parent directory name
                tempDat.Header.Name = Path.GetDirectoryName(key);
            });

            return true;
        }

        /// <summary>
        /// Helper function for SplitByLevel to sort the input game names
        /// </summary>
        /// <param name="a">First string to compare</param>
        /// <param name="b">Second string to compare</param>
        /// <returns>-1 for a coming before b, 0 for a == b, 1 for a coming after b</returns>
        private int SplitByLevelSort(string a, string b)
        {
            NaturalComparer nc = new NaturalComparer();
            int adeep = a.Count(c => c == '/' || c == '\\');
            int bdeep = b.Count(c => c == '/' || c == '\\');

            if (adeep == bdeep)
                return nc.Compare(a, b);

            return adeep - bdeep;
        }

        /// <summary>
        /// Helper function for SplitByLevel to clean and write out a DAT
        /// </summary>
        /// <param name="newDatFile">DAT to clean and write out</param>
        /// <param name="outDir">Directory to write out to</param>
        /// <param name="shortname">True if short naming scheme should be used, false otherwise</param>
        /// <param name="restore">True if original filenames should be used as the base for output filename, false otherwise</param>
        private void SplitByLevelHelper(DatFile newDatFile, string outDir, bool shortname, bool restore)
        {
            // Get the name from the DAT to use separately
            string name = newDatFile.Header.Name;
            string expName = name.Replace("/", " - ").Replace("\\", " - ");

            // Now set the new output values
            newDatFile.Header.FileName = WebUtility.HtmlDecode(string.IsNullOrWhiteSpace(name)
                ? Header.FileName
                : (shortname
                    ? Path.GetFileName(name)
                    : expName
                    )
                );
            newDatFile.Header.FileName = (restore ? $"{Header.FileName} ({newDatFile.Header.FileName})" : newDatFile.Header.FileName);
            newDatFile.Header.Name = $"{Header.Name} ({expName})";
            newDatFile.Header.Description = (string.IsNullOrWhiteSpace(Header.Description) ? newDatFile.Header.Name : $"{Header.Description} ({expName})");
            newDatFile.Header.Type = null;

            // Write out the temporary DAT to the proper directory
            newDatFile.Write(outDir);
        }

        /// <summary>
        /// Split a DAT by size of Rom
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <param name="radix">Long value representing the split point</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        public bool SplitBySize(string outDir, long radix)
        {
            // Create each of the respective output DATs
            Globals.Logger.User("Creating and populating new DATs");

            DatFile lessDat = Create(Header.CloneStandard());
            lessDat.Header.FileName += $" (less than {radix})";
            lessDat.Header.Name += $" (less than {radix})";
            lessDat.Header.Description += $" (less than {radix})";

            DatFile greaterEqualDat = Create(Header.CloneStandard());
            greaterEqualDat.Header.FileName += $" (equal-greater than {radix})";
            greaterEqualDat.Header.Name += $" (equal-greater than {radix})";
            greaterEqualDat.Header.Description += $" (equal-greater than {radix})";

            // Now populate each of the DAT objects in turn
            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = Items[key];
                foreach (DatItem item in items)
                {
                    // If the file is not a Rom, it automatically goes in the "lesser" dat
                    if (item.ItemType != ItemType.Rom)
                        lessDat.Items.Add(key, item);

                    // If the file is a Rom and less than the radix, put it in the "lesser" dat
                    else if (item.ItemType == ItemType.Rom && ((Rom)item).Size < radix)
                        lessDat.Items.Add(key, item);

                    // If the file is a Rom and greater than or equal to the radix, put it in the "greater" dat
                    else if (item.ItemType == ItemType.Rom && ((Rom)item).Size >= radix)
                        greaterEqualDat.Items.Add(key, item);
                }
            });

            // Now, output all of the files to the output directory
            Globals.Logger.User("DAT information created, outputting new files");
            bool success = true;
            success &= lessDat.Write(outDir);
            success &= greaterEqualDat.Write(outDir);

            return success;
        }

        /// <summary>
        /// Split a DAT by type of DatItem
        /// </summary>
        /// <param name="outDir">Name of the directory to write the DATs out to</param>
        /// <returns>True if split succeeded, false otherwise</returns>
        public bool SplitByType(string outDir)
        {
            // Create each of the respective output DATs
            Globals.Logger.User("Creating and populating new DATs");

            DatFile romdat = Create(Header.CloneStandard());
            romdat.Header.FileName += " (ROM)";
            romdat.Header.Name += " (ROM)";
            romdat.Header.Description += " (ROM)";

            DatFile diskdat = Create(Header.CloneStandard());
            diskdat.Header.FileName += " (Disk)";
            diskdat.Header.Name += " (Disk)";
            diskdat.Header.Description += " (Disk)";

            DatFile sampledat = Create(Header.CloneStandard());
            sampledat.Header.FileName += " (Sample)";
            sampledat.Header.Name += " (Sample)";
            sampledat.Header.Description += " (Sample)";

            // Now populate each of the DAT objects in turn
            Parallel.ForEach(Items.Keys, Globals.ParallelOptions, key =>
            {
                List<DatItem> items = Items[key];
                foreach (DatItem item in items)
                {
                    // If the file is a Rom
                    if (item.ItemType == ItemType.Rom)
                        romdat.Items.Add(key, item);

                    // If the file is a Disk
                    else if (item.ItemType == ItemType.Disk)
                        diskdat.Items.Add(key, item);

                    // If the file is a Sample
                    else if (item.ItemType == ItemType.Sample)
                        sampledat.Items.Add(key, item);
                }
            });

            // Now, output all of the files to the output directory
            Globals.Logger.User("DAT information created, outputting new files");
            bool success = true;
            success &= romdat.Write(outDir);
            success &= diskdat.Write(outDir);
            success &= sampledat.Write(outDir);

            return success;
        }

        #endregion

        #region Writing

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outDir">Set the output directory (current directory on null)</param>
        /// <param name="norename">True if games should only be compared on game and file name (default), false if system and source are counted</param>
        /// <param name="stats">True if DAT statistics should be output on write, false otherwise (default)</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <param name="overwrite">True if files should be overwritten (default), false if they should be renamed instead</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public bool Write(string outDir, bool norename = true, bool stats = false, bool ignoreblanks = false, bool overwrite = true)
        {
            // If there's nothing there, abort
            if (Items.TotalCount == 0)
            {
                Globals.Logger.User("There were no items to write out!");
                return false;
            }

            // Ensure the output directory is set and created
            outDir = DirectoryExtensions.Ensure(outDir, create: true);

            // If the DAT has no output format, default to XML
            if (Header.DatFormat == 0)
            {
                Globals.Logger.Verbose("No DAT format defined, defaulting to XML");
                Header.DatFormat = DatFormat.Logiqx;
            }

            // Make sure that the three essential fields are filled in
            if (string.IsNullOrWhiteSpace(Header.FileName) && string.IsNullOrWhiteSpace(Header.Name) && string.IsNullOrWhiteSpace(Header.Description))
            {
                Header.FileName = Header.Name = Header.Description = "Default";
            }
            else if (string.IsNullOrWhiteSpace(Header.FileName) && string.IsNullOrWhiteSpace(Header.Name) && !string.IsNullOrWhiteSpace(Header.Description))
            {
                Header.FileName = Header.Name = Header.Description;
            }
            else if (string.IsNullOrWhiteSpace(Header.FileName) && !string.IsNullOrWhiteSpace(Header.Name) && string.IsNullOrWhiteSpace(Header.Description))
            {
                Header.FileName = Header.Description = Header.Name;
            }
            else if (string.IsNullOrWhiteSpace(Header.FileName) && !string.IsNullOrWhiteSpace(Header.Name) && !string.IsNullOrWhiteSpace(Header.Description))
            {
                Header.FileName = Header.Description;
            }
            else if (!string.IsNullOrWhiteSpace(Header.FileName) && string.IsNullOrWhiteSpace(Header.Name) && string.IsNullOrWhiteSpace(Header.Description))
            {
                Header.Name = Header.Description = Header.FileName;
            }
            else if (!string.IsNullOrWhiteSpace(Header.FileName) && string.IsNullOrWhiteSpace(Header.Name) && !string.IsNullOrWhiteSpace(Header.Description))
            {
                Header.Name = Header.Description;
            }
            else if (!string.IsNullOrWhiteSpace(Header.FileName) && !string.IsNullOrWhiteSpace(Header.Name) && string.IsNullOrWhiteSpace(Header.Description))
            {
                Header.Description = Header.Name;
            }
            else if (!string.IsNullOrWhiteSpace(Header.FileName) && !string.IsNullOrWhiteSpace(Header.Name) && !string.IsNullOrWhiteSpace(Header.Description))
            {
                // Nothing is needed
            }

            // Output initial statistics, for kicks
            if (stats)
            {
                if (Items.RomCount + Items.DiskCount == 0)
                    Items.RecalculateStats();

                Items.BucketBy(Field.Machine_Name, DedupeType.None, norename: true);

                var consoleOutput = BaseReport.Create(StatReportFormat.None, null, true, true);
                consoleOutput.ReplaceStatistics(Header.FileName, Items.Keys.Count(), Items);
            }

            // Bucket and dedupe according to the flag
            if (Header.DedupeRoms == DedupeType.Full)
                Items.BucketBy(Field.DatItem_CRC, Header.DedupeRoms, norename: norename);
            else if (Header.DedupeRoms == DedupeType.Game)
                Items.BucketBy(Field.Machine_Name, Header.DedupeRoms, norename: norename);

            // Bucket roms by game name, if not already
            Items.BucketBy(Field.Machine_Name, DedupeType.None, norename: norename);

            // Output the number of items we're going to be writing
            Globals.Logger.User($"A total of {Items.TotalCount} items will be written out to '{Header.FileName}'");

            // Get the outfile names
            Dictionary<DatFormat, string> outfiles = Header.CreateOutFileNames(outDir, overwrite);

            try
            {
                // Write out all required formats
                Parallel.ForEach(outfiles.Keys, Globals.ParallelOptions, datFormat =>
                {
                    string outfile = outfiles[datFormat];
                    try
                    {
                        Create(datFormat, this)?.WriteToFile(outfile, ignoreblanks);
                    }
                    catch (Exception ex)
                    {
                        Globals.Logger.Error($"Datfile {outfile} could not be written out: {ex}");
                    }

                });
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="outfile">Name of the file to write to</param>
        /// <param name="ignoreblanks">True if blank roms should be skipped on output, false otherwise (default)</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public abstract bool WriteToFile(string outfile, bool ignoreblanks = false);

        /// <summary>
        /// Process an item and correctly set the item name
        /// </summary>
        /// <param name="item">DatItem to update</param>
        /// <param name="forceRemoveQuotes">True if the Quotes flag should be ignored, false otherwise</param>
        /// <param name="forceRomName">True if the UseRomName should be always on (default), false otherwise</param>
        protected void ProcessItemName(DatItem item, bool forceRemoveQuotes, bool forceRomName = true)
        {
            string name = item.Name;

            // Backup relevant values and set new ones accordingly
            bool quotesBackup = Header.Quotes;
            bool useRomNameBackup = Header.UseRomName;
            if (forceRemoveQuotes)
                Header.Quotes = false;

            if (forceRomName)
                Header.UseRomName = true;

            // Create the proper Prefix and Postfix
            string pre = CreatePrefixPostfix(item, true);
            string post = CreatePrefixPostfix(item, false);

            // If we're in Depot mode, take care of that instead
            if (Header.OutputDepot?.IsActive ?? false)
            {
                if (item.ItemType == ItemType.Rom)
                {
                    Rom romItem = item as Rom;

                    // We can only write out if there's a SHA-1
                    if (!string.IsNullOrWhiteSpace(romItem.SHA1))
                    {
                        name = PathExtensions.GetDepotPath(romItem.SHA1, Header.OutputDepot.Depth).Replace('\\', '/');
                        item.Name = $"{pre}{name}{post}";
                    }
                }
                else if (item.ItemType == ItemType.Disk)
                {
                    Disk diskItem = item as Disk;

                    // We can only write out if there's a SHA-1
                    if (!string.IsNullOrWhiteSpace(diskItem.SHA1))
                    {
                        name = PathExtensions.GetDepotPath(diskItem.SHA1, Header.OutputDepot.Depth).Replace('\\', '/');
                        item.Name = pre + name + post;
                    }
                }

                return;
            }

            if (!string.IsNullOrWhiteSpace(Header.ReplaceExtension) || Header.RemoveExtension)
            {
                if (Header.RemoveExtension)
                    Header.ReplaceExtension = string.Empty;

                string dir = Path.GetDirectoryName(name);
                dir = dir.TrimStart(Path.DirectorySeparatorChar);
                name = Path.Combine(dir, Path.GetFileNameWithoutExtension(name) + Header.ReplaceExtension);
            }

            if (!string.IsNullOrWhiteSpace(Header.AddExtension))
                name += Header.AddExtension;

            if (Header.UseRomName && Header.GameName)
                name = Path.Combine(item.Machine.Name, name);

            // Now assign back the item name
            item.Name = pre + name + post;

            // Restore all relevant values
            if (forceRemoveQuotes)
                Header.Quotes = quotesBackup;

            if (forceRomName)
                Header.UseRomName = useRomNameBackup;
        }

        /// <summary>
        /// Create a prefix or postfix from inputs
        /// </summary>
        /// <param name="item">DatItem to create a prefix/postfix for</param>
        /// <param name="prefix">True for prefix, false for postfix</param>
        /// <returns>Sanitized string representing the postfix or prefix</returns>
        protected string CreatePrefixPostfix(DatItem item, bool prefix)
        {
            // Initialize strings
            string fix = string.Empty,
                game = item.Machine.Name,
                name = item.Name,
                crc = string.Empty,
                md5 = string.Empty,
                ripemd160 = string.Empty,
                sha1 = string.Empty,
                sha256 = string.Empty,
                sha384 = string.Empty,
                sha512 = string.Empty,
                size = string.Empty;

            // If we have a prefix
            if (prefix)
                fix = Header.Prefix + (Header.Quotes ? "\"" : string.Empty);

            // If we have a postfix
            else
                fix = (Header.Quotes ? "\"" : string.Empty) + Header.Postfix;

            // Ensure we have the proper values for replacement
            if (item.ItemType == ItemType.Rom)
            {
                crc = ((Rom)item).CRC ?? string.Empty;
                md5 = ((Rom)item).MD5 ?? string.Empty;
#if NET_FRAMEWORK
                ripemd160 = ((Rom)item).RIPEMD160 ?? string.Empty;
#endif
                sha1 = ((Rom)item).SHA1 ?? string.Empty;
                sha256 = ((Rom)item).SHA256 ?? string.Empty;
                sha384 = ((Rom)item).SHA384 ?? string.Empty;
                sha512 = ((Rom)item).SHA512 ?? string.Empty;
                size = ((Rom)item).Size.ToString();
            }
            else if (item.ItemType == ItemType.Disk)
            {
                md5 = ((Disk)item).MD5 ?? string.Empty;
#if NET_FRAMEWORK
                ripemd160 = ((Disk)item).RIPEMD160 ?? string.Empty;
#endif
                sha1 = ((Disk)item).SHA1 ?? string.Empty;
                sha256 = ((Disk)item).SHA256 ?? string.Empty;
                sha384 = ((Disk)item).SHA384 ?? string.Empty;
                sha512 = ((Disk)item).SHA512 ?? string.Empty;
            }

            // Now do bulk replacement where possible
            fix = fix
                .Replace("%game%", game)
                .Replace("%machine%", game)
                .Replace("%name%", name)
                .Replace("%manufacturer%", item.Machine.Manufacturer ?? string.Empty)
                .Replace("%publisher%", item.Machine.Publisher ?? string.Empty)
                .Replace("%category%", item.Machine.Category ?? string.Empty)
                .Replace("%crc%", crc)
                .Replace("%md5%", md5)
                .Replace("%ripemd160%", ripemd160)
                .Replace("%sha1%", sha1)
                .Replace("%sha256%", sha256)
                .Replace("%sha384%", sha384)
                .Replace("%sha512%", sha512)
                .Replace("%size%", size);

            // TODO: Add GameName logic here too?

            return fix;
        }

        #endregion
    }
}
