using System.Collections.Generic;
using System.Linq;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
using System.Threading.Tasks;
#endif
using SabreTools.Data.Extensions;
using SabreTools.Logging;
using SabreTools.Metadata.DatFiles;
using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;

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
            InternalStopwatch watch = new($"Replacing items in '{intDat.Header.FileName}' from the base DAT");

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
            datFile.BucketBy(ItemKey.CRC32);
            datFile.Deduplicate();
            intDat.BucketBy(ItemKey.CRC32);

            // Then we do a hashwise comparison against the base DAT
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(intDat.Items.SortedKeys, key =>
#else
            foreach (var key in intDat.Items.SortedKeys)
#endif
            {
                List<DatItem>? datItems = intDat.GetItemsForBucket(key);
                if (datItems is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
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
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
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
            datFile.BucketBy(ItemKey.CRC32);
            datFile.Deduplicate();
            intDat.BucketBy(ItemKey.CRC32);

            // Then we do a hashwise comparison against the base DAT
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(intDat.ItemsDB.SortedKeys, key =>
#else
            foreach (var key in intDat.ItemsDB.SortedKeys)
#endif
            {
                var datItems = intDat.GetItemsForBucketDB(key);
                if (datItems is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
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
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
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
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(intDat.Items.SortedKeys, key =>
#else
            foreach (var key in intDat.Items.SortedKeys)
#endif
            {
                List<DatItem>? datItems = intDat.GetItemsForBucket(key);
                if (datItems is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
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
                        ReplaceFields(newDatItem.Machine!, list[index: 0].Machine!, machineFieldNames, onlySame);

                    newDatItems.Add(newDatItem);
                }

                // Now add the new list to the key
                intDat.RemoveBucket(key);
                newDatItems.ForEach(item => intDat.AddItem(item, statsOnly: false));
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
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
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            Parallel.ForEach(intDat.ItemsDB.SortedKeys, key =>
#else
            foreach (var key in intDat.ItemsDB.SortedKeys)
#endif
            {
                var datItems = intDat.GetItemsForBucketDB(key);
                if (datItems is null)
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                    return;
#else
                    continue;
#endif

                foreach (var datItem in datItems)
                {
                    var datMachine = datFile.GetMachineDB(datFile.GetItemsForBucketDB(key)!.First().Value.MachineIndex);
                    var intMachine = intDat.GetMachineDB(datItem.Value.MachineIndex);
                    if (datMachine.Value is not null && intMachine.Value is not null)
                        ReplaceFields(intMachine.Value, datMachine.Value, machineFieldNames, onlySame);
                }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
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
        /// <param name="datItem">DatItem to replace fields in</param>
        /// <param name="repDatItem">DatItem to pull new information from</param>
        /// <param name="itemFieldNames">List of fields representing what should be updated</param>
        public static void ReplaceFields(DatItem datItem, DatItem repDatItem, Dictionary<string, List<string>> itemFieldNames)
        {
            // If we have an invalid input, return
            if (datItem is null || repDatItem is null || itemFieldNames is null)
                return;

            #region Common

            if (datItem.ItemType != repDatItem.ItemType)
                return;

            // If there are no field names for this type or generic, return
            string? itemType = datItem.ItemType.AsStringValue();
            if (itemType is null || (!itemFieldNames.ContainsKey(itemType) && !itemFieldNames.ContainsKey("item")))
                return;

            // Get the combined list of fields to remove
            var fieldNames = new HashSet<string>();
            if (itemFieldNames.TryGetValue(itemType, out List<string>? value))
                fieldNames.UnionWith(value);
            if (itemFieldNames.TryGetValue("item", out value))
                fieldNames.UnionWith(value);

            // If the field specifically contains Name, set it separately
            if (fieldNames.Contains("name"))
                datItem.SetName(repDatItem.GetName());

            #endregion

            #region Item-Specific

            switch (datItem, repDatItem)
            {
                case (Adjuster obj, Adjuster repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Archive obj, Archive repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (BiosSet obj, BiosSet repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Chip obj, Chip repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Configuration obj, Configuration repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (ConfLocation obj, ConfLocation repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (ConfSetting obj, ConfSetting repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Control obj, Control repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Device obj, Device repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (DeviceRef obj, DeviceRef repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (DipLocation obj, DipLocation repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (DipSwitch obj, DipSwitch repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (DipValue obj, DipValue repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Disk obj, Disk repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Display obj, Display repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Driver obj, Driver repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Feature obj, Feature repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Info obj, Info repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Input obj, Input repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Media obj, Media repObj): ReplaceFields(obj, repObj, fieldNames); break;
                // case (Original obj, Original repObj): ReplaceFields(obj, repObj, fieldNames); break;
                // TODO: Part needs to be wrapped into DipSwitch, Disk, PartFeature, and Rom
                case (PartFeature obj, PartFeature repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Port obj, Port repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (RamOption obj, RamOption repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Release obj, Release repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (ReleaseDetails obj, ReleaseDetails repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Rom obj, Rom repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Sample obj, Sample repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Serials obj, Serials repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (SharedFeat obj, SharedFeat repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Slot obj, Slot repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (SlotOption obj, SlotOption repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (SoftwareList obj, SoftwareList repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (Sound obj, Sound repObj): ReplaceFields(obj, repObj, fieldNames); break;
                case (SourceDetails obj, SourceDetails repObj): ReplaceFields(obj, repObj, fieldNames); break;

                // Ignore types not handled by this class
                default: break;
            }

            #endregion
        }

        #endregion

        #region Per-Type Replacement

        /// <summary>
        /// Replace fields with given values
        /// </summary>
        /// <param name="machine">Machine to replace fields in</param>
        /// <param name="repMachine">Machine to pull new information from</param>
        /// <param name="fieldNames">List of fields representing what should be updated</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise</param>
        public static void ReplaceFields(Machine machine, Machine repMachine, List<string> fieldNames, bool onlySame)
        {
            // If we have an invalid input, return
            if (machine is null || repMachine is null || fieldNames is null)
                return;

            if (fieldNames.Contains("board"))
                machine.Board = repMachine.Board;

            if (fieldNames.Contains("buttons"))
                machine.Buttons = repMachine.Buttons;

            if (fieldNames.Contains("category"))
                machine.Category = repMachine.Category;

            if (fieldNames.Contains("cloneof"))
                machine.CloneOf = repMachine.CloneOf;

            if (fieldNames.Contains("cloneofid"))
                machine.CloneOfId = repMachine.CloneOfId;

            if (fieldNames.Contains("comment"))
                machine.Comment = repMachine.Comment;

            if (fieldNames.Contains("company"))
                machine.Company = repMachine.Company;

            if (fieldNames.Contains("control"))
                machine.Control = repMachine.Control;

            if (fieldNames.Contains("crc"))
                machine.CRC = repMachine.CRC;

            if (fieldNames.Contains("country"))
                machine.Country = repMachine.Country;

            // Special case for description
            if (fieldNames.Contains("description"))
            {
                if (!onlySame || (onlySame && machine.Name == machine.Description))
                    machine.Description = repMachine.Description;
            }

            if (fieldNames.Contains("developer"))
                machine.Developer = repMachine.Developer;

            if (fieldNames.Contains("dirname"))
                machine.DirName = repMachine.DirName;

            if (fieldNames.Contains("displaycount"))
                machine.DisplayCount = repMachine.DisplayCount;

            if (fieldNames.Contains("displaytype"))
                machine.DisplayType = repMachine.DisplayType;

            if (fieldNames.Contains("duplicateid"))
                machine.DuplicateID = repMachine.DuplicateID;

            if (fieldNames.Contains("emulator"))
                machine.Emulator = repMachine.Emulator;

            if (fieldNames.Contains("enabled"))
                machine.Enabled = repMachine.Enabled;

            if (fieldNames.Contains("extra"))
                machine.Extra = repMachine.Extra;

            if (fieldNames.Contains("favorite"))
                machine.Favorite = repMachine.Favorite;

            if (fieldNames.Contains("genmsxid"))
                machine.GenMSXID = repMachine.GenMSXID;

            if (fieldNames.Contains("genre"))
                machine.Genre = repMachine.Genre;

            if (fieldNames.Contains("hash"))
                machine.Hash = repMachine.Hash;

            if (fieldNames.Contains("history"))
                machine.History = repMachine.History;

            if (fieldNames.Contains("id"))
                machine.Id = repMachine.Id;

            if (fieldNames.Contains("im1crc"))
                machine.Im1CRC = repMachine.Im1CRC;

            if (fieldNames.Contains("im2crc"))
                machine.Im2CRC = repMachine.Im2CRC;

            if (fieldNames.Contains("imagenumber"))
                machine.ImageNumber = repMachine.ImageNumber;

            if (fieldNames.Contains("isbios"))
                machine.IsBios = repMachine.IsBios;

            if (fieldNames.Contains("isdevice"))
                machine.IsDevice = repMachine.IsDevice;

            if (fieldNames.Contains("ismechanical"))
                machine.IsMechanical = repMachine.IsMechanical;

            if (fieldNames.Contains("language"))
                machine.Language = repMachine.Language;

            if (fieldNames.Contains("location"))
                machine.Location = repMachine.Location;

            if (fieldNames.Contains("manufacturer"))
                machine.Manufacturer = repMachine.Manufacturer;

            if (fieldNames.Contains("name"))
                machine.Name = repMachine.Name;

            if (fieldNames.Contains("notes"))
                machine.Notes = repMachine.Notes;

            if (fieldNames.Contains("playedcount"))
                machine.PlayedCount = repMachine.PlayedCount;

            if (fieldNames.Contains("playedtime"))
                machine.PlayedTime = repMachine.PlayedTime;

            if (fieldNames.Contains("players"))
                machine.Players = repMachine.Players;

            if (fieldNames.Contains("publisher"))
                machine.Publisher = repMachine.Publisher;

            if (fieldNames.Contains("ratings"))
                machine.Ratings = repMachine.Ratings;

            if (fieldNames.Contains("rebuildto"))
                machine.RebuildTo = repMachine.RebuildTo;

            if (fieldNames.Contains("relatedto"))
                machine.RelatedTo = repMachine.RelatedTo;

            if (fieldNames.Contains("releasenumber"))
                machine.ReleaseNumber = repMachine.ReleaseNumber;

            if (fieldNames.Contains("romof"))
                machine.RomOf = repMachine.RomOf;

            if (fieldNames.Contains("rotation"))
                machine.Rotation = repMachine.Rotation;

            if (fieldNames.Contains("runnable"))
                machine.Runnable = repMachine.Runnable;

            if (fieldNames.Contains("sampleof"))
                machine.SampleOf = repMachine.SampleOf;

            if (fieldNames.Contains("savetype"))
                machine.SaveType = repMachine.SaveType;

            if (fieldNames.Contains("score"))
                machine.Score = repMachine.Score;

            if (fieldNames.Contains("source"))
                machine.Source = repMachine.Source;

            if (fieldNames.Contains("sourcefile"))
                machine.SourceFile = repMachine.SourceFile;

            if (fieldNames.Contains("sourcerom"))
                machine.SourceRom = repMachine.SourceRom;

            if (fieldNames.Contains("status"))
                machine.Status = repMachine.Status;

            if (fieldNames.Contains("subgenre"))
                machine.Subgenre = repMachine.Subgenre;

            if (fieldNames.Contains("supported"))
                machine.Supported = repMachine.Supported;

            if (fieldNames.Contains("system"))
                machine.System = repMachine.System;

            if (fieldNames.Contains("tags"))
                machine.Tags = repMachine.Tags;

            if (fieldNames.Contains("titleid"))
                machine.TitleID = repMachine.TitleID;

            if (fieldNames.Contains("url"))
                machine.Url = repMachine.Url;

            if (fieldNames.Contains("year"))
                machine.Year = repMachine.Year;

        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Adjuster obj, Adjuster repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("condition.mask"))
                obj.ConditionMask = repObj.ConditionMask;

            if (fieldNames.Contains("condition.relation"))
                obj.ConditionRelation = repObj.ConditionRelation;

            if (fieldNames.Contains("condition.tag"))
                obj.ConditionTag = repObj.ConditionTag;

            if (fieldNames.Contains("condition.value"))
                obj.ConditionValue = repObj.ConditionValue;

            if (fieldNames.Contains("default"))
                obj.Default = repObj.Default;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Archive obj, Archive repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("additional"))
                obj.Additional = repObj.Additional;

            if (fieldNames.Contains("adult"))
                obj.Adult = repObj.Adult;

            if (fieldNames.Contains("alt"))
                obj.Alt = repObj.Alt;

            if (fieldNames.Contains("bios"))
                obj.Bios = repObj.Bios;

            if (fieldNames.Contains("categories"))
                obj.Categories = repObj.Categories;

            if (fieldNames.Contains("clone") || fieldNames.Contains("clonetag"))
                obj.CloneTag = repObj.CloneTag;

            if (fieldNames.Contains("complete"))
                obj.Complete = repObj.Complete;

            if (fieldNames.Contains("dat"))
                obj.Dat = repObj.Dat;

            if (fieldNames.Contains("datternote"))
                obj.DatterNote = repObj.DatterNote;

            if (fieldNames.Contains("description"))
                obj.Description = repObj.Description;

            if (fieldNames.Contains("devstatus"))
                obj.DevStatus = repObj.DevStatus;

            if (fieldNames.Contains("gameid1"))
                obj.GameId1 = repObj.GameId1;

            if (fieldNames.Contains("gameid2"))
                obj.GameId2 = repObj.GameId2;

            if (fieldNames.Contains("langchecked"))
                obj.LangChecked = repObj.LangChecked;

            if (fieldNames.Contains("languages"))
                obj.Languages = repObj.Languages;

            if (fieldNames.Contains("licensed"))
                obj.Licensed = repObj.Licensed;

            if (fieldNames.Contains("listed"))
                obj.Listed = repObj.Listed;

            if (fieldNames.Contains("mergeof"))
                obj.MergeOf = repObj.MergeOf;

            if (fieldNames.Contains("mergename"))
                obj.MergeName = repObj.MergeName;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("namealt"))
                obj.NameAlt = repObj.NameAlt;

            if (fieldNames.Contains("number"))
                obj.Number = repObj.Number;

            if (fieldNames.Contains("physical"))
                obj.Physical = repObj.Physical;

            if (fieldNames.Contains("pirate"))
                obj.Pirate = repObj.Pirate;

            if (fieldNames.Contains("private"))
                obj.Private = repObj.Private;

            if (fieldNames.Contains("region"))
                obj.Region = repObj.Region;

            if (fieldNames.Contains("regparent"))
                obj.RegParent = repObj.RegParent;

            if (fieldNames.Contains("showlang"))
                obj.ShowLang = repObj.ShowLang;

            if (fieldNames.Contains("special1"))
                obj.Special1 = repObj.Special1;

            if (fieldNames.Contains("special2"))
                obj.Special2 = repObj.Special2;

            if (fieldNames.Contains("stickynote"))
                obj.StickyNote = repObj.StickyNote;

            if (fieldNames.Contains("version1"))
                obj.Version1 = repObj.Version1;

            if (fieldNames.Contains("version2"))
                obj.Version2 = repObj.Version2;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(BiosSet obj, BiosSet repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("default"))
                obj.Default = repObj.Default;

            if (fieldNames.Contains("description"))
                obj.Description = repObj.Description;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Chip obj, Chip repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("chiptype"))
                obj.ChipType = repObj.ChipType;

            if (fieldNames.Contains("clock"))
                obj.Clock = repObj.Clock;

            if (fieldNames.Contains("flags"))
                obj.Flags = repObj.Flags;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("soundonly"))
                obj.SoundOnly = repObj.SoundOnly;

            if (fieldNames.Contains("tag"))
                obj.Tag = repObj.Tag;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Configuration obj, Configuration repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("condition.mask"))
                obj.ConditionMask = repObj.ConditionMask;

            if (fieldNames.Contains("condition.relation"))
                obj.ConditionRelation = repObj.ConditionRelation;

            if (fieldNames.Contains("condition.tag"))
                obj.ConditionTag = repObj.ConditionTag;

            if (fieldNames.Contains("condition.value"))
                obj.ConditionValue = repObj.ConditionValue;

            if (fieldNames.Contains("mask"))
                obj.Mask = repObj.Mask;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("tag"))
                obj.Tag = repObj.Tag;

            // Replacing ConfLocation is not possible
            // Replacing ConfSetting is not possible
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(ConfLocation obj, ConfLocation repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("inverted"))
                obj.Inverted = repObj.Inverted;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("number"))
                obj.Number = repObj.Number;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(ConfSetting obj, ConfSetting repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("condition.mask"))
                obj.ConditionMask = repObj.ConditionMask;

            if (fieldNames.Contains("condition.relation"))
                obj.ConditionRelation = repObj.ConditionRelation;

            if (fieldNames.Contains("condition.tag"))
                obj.ConditionTag = repObj.ConditionTag;

            if (fieldNames.Contains("condition.value"))
                obj.ConditionValue = repObj.ConditionValue;

            if (fieldNames.Contains("default"))
                obj.Default = repObj.Default;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("value"))
                obj.Value = repObj.Value;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Control obj, Control repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("buttons"))
                obj.Buttons = repObj.Buttons;

            if (fieldNames.Contains("controltype"))
                obj.ControlType = repObj.ControlType;

            if (fieldNames.Contains("keydelta"))
                obj.KeyDelta = repObj.KeyDelta;

            if (fieldNames.Contains("maximum"))
                obj.Maximum = repObj.Maximum;

            if (fieldNames.Contains("minimum"))
                obj.Minimum = repObj.Minimum;

            if (fieldNames.Contains("player"))
                obj.Player = repObj.Player;

            if (fieldNames.Contains("reqbuttons"))
                obj.ReqButtons = repObj.ReqButtons;

            if (fieldNames.Contains("reverse"))
                obj.Reverse = repObj.Reverse;

            if (fieldNames.Contains("sensitivity"))
                obj.Sensitivity = repObj.Sensitivity;

            if (fieldNames.Contains("ways"))
                obj.Ways = repObj.Ways;

            if (fieldNames.Contains("ways2"))
                obj.Ways2 = repObj.Ways2;

            if (fieldNames.Contains("ways3"))
                obj.Ways3 = repObj.Ways3;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Device obj, Device repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("devicetype"))
                obj.DeviceType = repObj.DeviceType;

            if (fieldNames.Contains("extension.name"))
                obj.ExtensionName = repObj.ExtensionName;

            if (fieldNames.Contains("fixedimage"))
                obj.FixedImage = repObj.FixedImage;

            if (fieldNames.Contains("instance.briefname"))
                obj.InstanceBriefName = repObj.InstanceBriefName;

            if (fieldNames.Contains("instance.name"))
                obj.InstanceName = repObj.InstanceName;

            if (fieldNames.Contains("interface"))
                obj.Interface = repObj.Interface;

            if (fieldNames.Contains("mandatory"))
                obj.Mandatory = repObj.Mandatory;

            if (fieldNames.Contains("tag"))
                obj.Tag = repObj.Tag;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(DeviceRef obj, DeviceRef repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(DipLocation obj, DipLocation repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("inverted"))
                obj.Inverted = repObj.Inverted;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("number"))
                obj.Number = repObj.Number;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(DipSwitch obj, DipSwitch repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("condition.mask"))
                obj.ConditionMask = repObj.ConditionMask;

            if (fieldNames.Contains("condition.relation"))
                obj.ConditionRelation = repObj.ConditionRelation;

            if (fieldNames.Contains("condition.tag"))
                obj.ConditionTag = repObj.ConditionTag;

            if (fieldNames.Contains("condition.value"))
                obj.ConditionValue = repObj.ConditionValue;

            if (fieldNames.Contains("default"))
                obj.Default = repObj.Default;

            if (fieldNames.Contains("mask"))
                obj.Mask = repObj.Mask;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("tag"))
                obj.Tag = repObj.Tag;

            // Replacing DipLocation is not possible
            // Replacing DipValue is not possible
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(DipValue obj, DipValue repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("condition.mask"))
                obj.ConditionMask = repObj.ConditionMask;

            if (fieldNames.Contains("condition.relation"))
                obj.ConditionRelation = repObj.ConditionRelation;

            if (fieldNames.Contains("condition.tag"))
                obj.ConditionTag = repObj.ConditionTag;

            if (fieldNames.Contains("condition.value"))
                obj.ConditionValue = repObj.ConditionValue;

            if (fieldNames.Contains("default"))
                obj.Default = repObj.Default;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("value"))
                obj.Value = repObj.Value;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Disk obj, Disk repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("flags"))
                obj.Flags = repObj.Flags;

            if (fieldNames.Contains("index"))
                obj.Index = repObj.Index;

            if (fieldNames.Contains("md5"))
            {
                if (!string.IsNullOrEmpty(repObj.MD5))
                    obj.MD5 = repObj.MD5;
            }

            if (fieldNames.Contains("merge"))
                obj.Merge = repObj.Merge;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("optional"))
                obj.Optional = repObj.Optional;

            if (fieldNames.Contains("region"))
                obj.Region = repObj.Region;

            if (fieldNames.Contains("sha1"))
            {
                if (!string.IsNullOrEmpty(repObj.SHA1))
                    obj.SHA1 = repObj.SHA1;
            }

            if (fieldNames.Contains("status"))
                obj.Status = repObj.Status;

            if (fieldNames.Contains("writable"))
                obj.Writable = repObj.Writable;

            // DiskArea
            if (fieldNames.Contains("diskarea.name"))
                obj.DiskAreaName = repObj.DiskAreaName;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Display obj, Display repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("aspectx"))
                obj.AspectX = repObj.AspectX;

            if (fieldNames.Contains("aspecty"))
                obj.AspectY = repObj.AspectY;

            if (fieldNames.Contains("displaytype") || fieldNames.Contains("screen"))
                obj.DisplayType = repObj.DisplayType;

            if (fieldNames.Contains("flipx"))
                obj.FlipX = repObj.FlipX;

            if (fieldNames.Contains("hbend"))
                obj.HBEnd = repObj.HBEnd;

            if (fieldNames.Contains("hbstart"))
                obj.HBStart = repObj.HBStart;

            if (fieldNames.Contains("height") || fieldNames.Contains("y"))
                obj.Height = repObj.Height;

            if (fieldNames.Contains("htotal"))
                obj.HTotal = repObj.HTotal;

            if (fieldNames.Contains("pixclock"))
                obj.PixClock = repObj.PixClock;

            if (fieldNames.Contains("refresh") || fieldNames.Contains("freq"))
                obj.Refresh = repObj.Refresh;

            if (fieldNames.Contains("rotate") || fieldNames.Contains("orientation"))
                obj.Rotate = repObj.Rotate;

            if (fieldNames.Contains("tag"))
                obj.Tag = repObj.Tag;

            if (fieldNames.Contains("vbend"))
                obj.VBEnd = repObj.VBEnd;

            if (fieldNames.Contains("vbstart"))
                obj.VBStart = repObj.VBStart;

            if (fieldNames.Contains("vtotal"))
                obj.VTotal = repObj.VTotal;

            if (fieldNames.Contains("width") || fieldNames.Contains("x"))
                obj.Width = repObj.Width;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Driver obj, Driver repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("blit"))
                obj.Blit = repObj.Blit;

            if (fieldNames.Contains("cocktail"))
                obj.Cocktail = repObj.Cocktail;

            if (fieldNames.Contains("color"))
                obj.Color = repObj.Color;

            if (fieldNames.Contains("emulation"))
                obj.Emulation = repObj.Emulation;

            if (fieldNames.Contains("incomplete"))
                obj.Incomplete = repObj.Incomplete;

            if (fieldNames.Contains("nosoundhardware"))
                obj.NoSoundHardware = repObj.NoSoundHardware;

            if (fieldNames.Contains("palettesize"))
                obj.PaletteSize = repObj.PaletteSize;

            if (fieldNames.Contains("requiresartwork"))
                obj.RequiresArtwork = repObj.RequiresArtwork;

            if (fieldNames.Contains("savestate"))
                obj.SaveState = repObj.SaveState;

            if (fieldNames.Contains("sound"))
                obj.Sound = repObj.Sound;

            if (fieldNames.Contains("status"))
                obj.Status = repObj.Status;

            if (fieldNames.Contains("unofficial"))
                obj.Unofficial = repObj.Unofficial;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Feature obj, Feature repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("featuretype"))
                obj.FeatureType = repObj.FeatureType;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("overall"))
                obj.Overall = repObj.Overall;

            if (fieldNames.Contains("status"))
                obj.Status = repObj.Status;

            if (fieldNames.Contains("value"))
                obj.Value = repObj.Value;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Info obj, Info repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("value"))
                obj.Value = repObj.Value;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Input obj, Input repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("buttons"))
                obj.Buttons = repObj.Buttons;

            if (fieldNames.Contains("coins"))
                obj.Coins = repObj.Coins;

            if (fieldNames.Contains("control") || fieldNames.Contains("controlattr"))
                obj.ControlAttr = repObj.ControlAttr;

            if (fieldNames.Contains("players"))
                obj.Players = repObj.Players;

            if (fieldNames.Contains("service"))
                obj.Service = repObj.Service;

            if (fieldNames.Contains("tilt"))
                obj.Tilt = repObj.Tilt;

            // Replacing Control is not possible
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Media obj, Media repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("md5"))
            {
                if (!string.IsNullOrEmpty(repObj.MD5))
                    obj.MD5 = repObj.MD5;
            }

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("sha1"))
            {
                if (!string.IsNullOrEmpty(repObj.SHA1))
                    obj.SHA1 = repObj.SHA1;
            }

            if (fieldNames.Contains("sha256"))
            {
                if (!string.IsNullOrEmpty(repObj.SHA256))
                    obj.SHA256 = repObj.SHA256;
            }

            if (fieldNames.Contains("spamsum"))
            {
                if (!string.IsNullOrEmpty(repObj.SpamSum))
                    obj.SpamSum = repObj.SpamSum;
            }
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(PartFeature obj, PartFeature repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("featuretype"))
                obj.FeatureType = repObj.FeatureType;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("overall"))
                obj.Overall = repObj.Overall;

            if (fieldNames.Contains("status"))
                obj.Status = repObj.Status;

            if (fieldNames.Contains("value"))
                obj.Value = repObj.Value;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Port obj, Port repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("analog.mask"))
                obj.AnalogMask = repObj.AnalogMask;

            if (fieldNames.Contains("tag"))
                obj.Tag = repObj.Tag;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(RamOption obj, RamOption repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("content"))
                obj.Content = repObj.Content;

            if (fieldNames.Contains("default"))
                obj.Default = repObj.Default;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Release obj, Release repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("date"))
                obj.Date = repObj.Date;

            if (fieldNames.Contains("default"))
                obj.Default = repObj.Default;

            if (fieldNames.Contains("language"))
                obj.Language = repObj.Language;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("region"))
                obj.Region = repObj.Region;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(ReleaseDetails obj, ReleaseDetails repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("appendtonumber"))
                obj.AppendToNumber = repObj.AppendToNumber;

            if (fieldNames.Contains("archivename"))
                obj.ArchiveName = repObj.ArchiveName;

            if (fieldNames.Contains("category"))
                obj.Category = repObj.Category;

            if (fieldNames.Contains("comment"))
                obj.Comment = repObj.Comment;

            if (fieldNames.Contains("date"))
                obj.Date = repObj.Date;

            if (fieldNames.Contains("dirname"))
                obj.DirName = repObj.DirName;

            if (fieldNames.Contains("group"))
                obj.Group = repObj.Group;

            if (fieldNames.Contains("id"))
                obj.Id = repObj.Id;

            if (fieldNames.Contains("nfocrc"))
                obj.NfoCRC = repObj.NfoCRC;

            if (fieldNames.Contains("nfoname"))
                obj.NfoName = repObj.NfoName;

            if (fieldNames.Contains("nfosize"))
                obj.NfoSize = repObj.NfoSize;

            if (fieldNames.Contains("origin"))
                obj.Origin = repObj.Origin;

            if (fieldNames.Contains("originalformat"))
                obj.OriginalFormat = repObj.OriginalFormat;

            if (fieldNames.Contains("region"))
                obj.Region = repObj.Region;

            if (fieldNames.Contains("rominfo"))
                obj.RomInfo = repObj.RomInfo;

            if (fieldNames.Contains("tool"))
                obj.Tool = repObj.Tool;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Rom obj, Rom repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("album"))
                obj.Album = repObj.Album;

            if (fieldNames.Contains("alt_romname") || fieldNames.Contains("altromname"))
                obj.AltRomname = repObj.AltRomname;

            if (fieldNames.Contains("alt_title") || fieldNames.Contains("alttitle"))
                obj.AltTitle = repObj.AltTitle;

            if (fieldNames.Contains("artist"))
                obj.Artist = repObj.Artist;

            if (fieldNames.Contains("asr_detected_lang") || fieldNames.Contains("asrdetectedlang"))
                obj.ASRDetectedLang = repObj.ASRDetectedLang;

            if (fieldNames.Contains("asr_detected_lang_conf") || fieldNames.Contains("asrdetectedlangconf"))
                obj.ASRDetectedLangConf = repObj.ASRDetectedLangConf;

            if (fieldNames.Contains("asr_transcribed_lang") || fieldNames.Contains("asrtranscribedlang"))
                obj.ASRTranscribedLang = repObj.ASRTranscribedLang;

            if (fieldNames.Contains("bios"))
                obj.Bios = repObj.Bios;

            if (fieldNames.Contains("bitrate"))
                obj.Bitrate = repObj.Bitrate;

            if (fieldNames.Contains("btih") || fieldNames.Contains("bittorrentmagnethash"))
                obj.BitTorrentMagnetHash = repObj.BitTorrentMagnetHash;

            if (fieldNames.Contains("cloth_cover_detection_module_version") || fieldNames.Contains("clothcoverdetectionmoduleversion"))
                obj.ClothCoverDetectionModuleVersion = repObj.ClothCoverDetectionModuleVersion;

            if (fieldNames.Contains("collection-catalog-number") || fieldNames.Contains("collectioncatalognumber"))
                obj.CollectionCatalogNumber = repObj.CollectionCatalogNumber;

            if (fieldNames.Contains("comment"))
                obj.Comment = repObj.Comment;

            if (fieldNames.Contains("crc16"))
            {
                if (!string.IsNullOrEmpty(repObj.CRC16))
                    obj.CRC16 = repObj.CRC16;
            }

            if (fieldNames.Contains("crc") || fieldNames.Contains("crc32"))
            {
                if (!string.IsNullOrEmpty(repObj.CRC32))
                    obj.CRC32 = repObj.CRC32;
            }

            if (fieldNames.Contains("crc64"))
            {
                if (!string.IsNullOrEmpty(repObj.CRC64))
                    obj.CRC64 = repObj.CRC64;
            }

            if (fieldNames.Contains("creator"))
                obj.Creator = repObj.Creator;

            if (fieldNames.Contains("date"))
                obj.Date = repObj.Date;

            if (fieldNames.Contains("dispose"))
                obj.Dispose = repObj.Dispose;

            if (fieldNames.Contains("extension"))
                obj.Extension = repObj.Extension;

            if (fieldNames.Contains("filecount"))
                obj.FileCount = repObj.FileCount;

            if (fieldNames.Contains("fileisavailable"))
                obj.FileIsAvailable = repObj.FileIsAvailable;

            if (fieldNames.Contains("flags"))
                obj.Flags = repObj.Flags;

            if (fieldNames.Contains("format"))
                obj.Format = repObj.Format;

            if (fieldNames.Contains("header"))
                obj.Header = repObj.Header;

            if (fieldNames.Contains("height"))
                obj.Height = repObj.Height;

            if (fieldNames.Contains("hocr_char_to_word_hocr_version") || fieldNames.Contains("hocrchartowordhocrversion"))
                obj.hOCRCharToWordhOCRVersion = repObj.hOCRCharToWordhOCRVersion;

            if (fieldNames.Contains("hocr_char_to_word_module_version") || fieldNames.Contains("hocrchartowordmoduleversion"))
                obj.hOCRCharToWordModuleVersion = repObj.hOCRCharToWordModuleVersion;

            if (fieldNames.Contains("hocr_fts_text_hocr_version") || fieldNames.Contains("hocrftstexthocrversion"))
                obj.hOCRFtsTexthOCRVersion = repObj.hOCRFtsTexthOCRVersion;

            if (fieldNames.Contains("hocr_fts_text_module_version") || fieldNames.Contains("hocrftstextmoduleversion"))
                obj.hOCRFtsTextModuleVersion = repObj.hOCRFtsTextModuleVersion;

            if (fieldNames.Contains("hocr_pageindex_hocr_version") || fieldNames.Contains("hocrpageindexhocrversion"))
                obj.hOCRPageIndexhOCRVersion = repObj.hOCRPageIndexhOCRVersion;

            if (fieldNames.Contains("hocr_pageindex_module_version") || fieldNames.Contains("hocrpageindexmoduleversion"))
                obj.hOCRPageIndexModuleVersion = repObj.hOCRPageIndexModuleVersion;

            if (fieldNames.Contains("inverted"))
                obj.Inverted = repObj.Inverted;

            if (fieldNames.Contains("mtime") || fieldNames.Contains("lastmodifiedtime"))
                obj.LastModifiedTime = repObj.LastModifiedTime;

            if (fieldNames.Contains("length"))
                obj.Length = repObj.Length;

            if (fieldNames.Contains("loadflag"))
                obj.LoadFlag = repObj.LoadFlag;

            if (fieldNames.Contains("matrix_number") || fieldNames.Contains("matrixnumber"))
                obj.MatrixNumber = repObj.MatrixNumber;

            if (fieldNames.Contains("md2"))
            {
                if (!string.IsNullOrEmpty(repObj.MD2))
                    obj.MD2 = repObj.MD2;
            }

            if (fieldNames.Contains("md4"))
            {
                if (!string.IsNullOrEmpty(repObj.MD4))
                    obj.MD4 = repObj.MD4;
            }

            if (fieldNames.Contains("md5"))
            {
                if (!string.IsNullOrEmpty(repObj.MD5))
                    obj.MD5 = repObj.MD5;
            }

            if (fieldNames.Contains("mediatype") || fieldNames.Contains("openmsxmediatype"))
                obj.OpenMSXMediaType = repObj.OpenMSXMediaType;

            if (fieldNames.Contains("merge"))
                obj.Merge = repObj.Merge;

            if (fieldNames.Contains("mia"))
                obj.MIA = repObj.MIA;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("offset"))
                obj.Offset = repObj.Offset;

            if (fieldNames.Contains("openmsxtype"))
                obj.OpenMSXType = repObj.OpenMSXType;

            if (fieldNames.Contains("optional"))
                obj.Optional = repObj.Optional;

            if (fieldNames.Contains("original"))
                obj.Original = repObj.Original;

            if (fieldNames.Contains("pdf_module_version") || fieldNames.Contains("pdfmoduleversion"))
                obj.PDFModuleVersion = repObj.PDFModuleVersion;

            if (fieldNames.Contains("preview-image") || fieldNames.Contains("previewimage"))
                obj.PreviewImage = repObj.PreviewImage;

            if (fieldNames.Contains("publisher"))
                obj.Publisher = repObj.Publisher;

            if (fieldNames.Contains("region"))
                obj.Region = repObj.Region;

            if (fieldNames.Contains("remark"))
                obj.Remark = repObj.Remark;

            if (fieldNames.Contains("ripemd128"))
            {
                if (!string.IsNullOrEmpty(repObj.RIPEMD128))
                    obj.RIPEMD128 = repObj.RIPEMD128;
            }

            if (fieldNames.Contains("ripemd160"))
            {
                if (!string.IsNullOrEmpty(repObj.RIPEMD160))
                    obj.RIPEMD160 = repObj.RIPEMD160;
            }

            if (fieldNames.Contains("rotation"))
                obj.Rotation = repObj.Rotation;

            if (fieldNames.Contains("serial"))
                obj.Serial = repObj.Serial;

            if (fieldNames.Contains("sha1"))
            {
                if (!string.IsNullOrEmpty(repObj.SHA1))
                    obj.SHA1 = repObj.SHA1;
            }

            if (fieldNames.Contains("sha256"))
            {
                if (!string.IsNullOrEmpty(repObj.SHA256))
                    obj.SHA256 = repObj.SHA256;
            }

            if (fieldNames.Contains("sha384"))
            {
                if (!string.IsNullOrEmpty(repObj.SHA384))
                    obj.SHA384 = repObj.SHA384;
            }

            if (fieldNames.Contains("sha512"))
            {
                if (!string.IsNullOrEmpty(repObj.SHA512))
                    obj.SHA512 = repObj.SHA512;
            }

            if (fieldNames.Contains("size"))
                obj.Size = repObj.Size;

            if (fieldNames.Contains("soundonly"))
                obj.SoundOnly = repObj.SoundOnly;

            if (fieldNames.Contains("spamsum"))
            {
                if (!string.IsNullOrEmpty(repObj.SpamSum))
                    obj.SpamSum = repObj.SpamSum;
            }

            if (fieldNames.Contains("start"))
                obj.Start = repObj.Start;

            if (fieldNames.Contains("status"))
                obj.Status = repObj.Status;

            if (fieldNames.Contains("summation"))
                obj.Summation = repObj.Summation;

            if (fieldNames.Contains("ocr") || fieldNames.Contains("tesseractocr"))
                obj.TesseractOCR = repObj.TesseractOCR;

            if (fieldNames.Contains("ocr_converted") || fieldNames.Contains("tesseractocrconverted"))
                obj.TesseractOCRConverted = repObj.TesseractOCRConverted;

            if (fieldNames.Contains("ocr_detected_lang") || fieldNames.Contains("tesseractocrdetectedlang"))
                obj.TesseractOCRDetectedLang = repObj.TesseractOCRDetectedLang;

            if (fieldNames.Contains("ocr_detected_lang_conf") || fieldNames.Contains("tesseractocrdetectedlangconf"))
                obj.TesseractOCRDetectedLangConf = repObj.TesseractOCRDetectedLangConf;

            if (fieldNames.Contains("ocr_detected_script") || fieldNames.Contains("tesseractocrdetectedscript"))
                obj.TesseractOCRDetectedScript = repObj.TesseractOCRDetectedScript;

            if (fieldNames.Contains("ocr_detected_script_conf") || fieldNames.Contains("tesseractocrdetectedscriptconf"))
                obj.TesseractOCRDetectedScriptConf = repObj.TesseractOCRDetectedScriptConf;

            if (fieldNames.Contains("ocr_module_version") || fieldNames.Contains("tesseractocrmoduleversion"))
                obj.TesseractOCRModuleVersion = repObj.TesseractOCRModuleVersion;

            if (fieldNames.Contains("ocr_parameters") || fieldNames.Contains("tesseractocrparameters"))
                obj.TesseractOCRParameters = repObj.TesseractOCRParameters;

            if (fieldNames.Contains("title"))
                obj.Title = repObj.Title;

            if (fieldNames.Contains("track"))
                obj.Track = repObj.Track;

            if (fieldNames.Contains("value"))
                obj.Value = repObj.Value;

            if (fieldNames.Contains("whisper_asr_module_version") || fieldNames.Contains("whisperasrmoduleversion"))
                obj.WhisperASRModuleVersion = repObj.WhisperASRModuleVersion;

            if (fieldNames.Contains("whisper_model_hash") || fieldNames.Contains("whispermodelhash"))
                obj.WhisperModelHash = repObj.WhisperModelHash;

            if (fieldNames.Contains("whisper_model_name") || fieldNames.Contains("whispermodelname"))
                obj.WhisperModelName = repObj.WhisperModelName;

            if (fieldNames.Contains("whisper_version") || fieldNames.Contains("whisperversion"))
                obj.WhisperVersion = repObj.WhisperVersion;

            if (fieldNames.Contains("width"))
                obj.Width = repObj.Width;

            if (fieldNames.Contains("word_conf_0_10") || fieldNames.Contains("wordconfidenceinterval0to10"))
                obj.WordConfidenceInterval0To10 = repObj.WordConfidenceInterval0To10;

            if (fieldNames.Contains("word_conf_11_20") || fieldNames.Contains("wordconfidenceinterval11to20"))
                obj.WordConfidenceInterval11To20 = repObj.WordConfidenceInterval11To20;

            if (fieldNames.Contains("word_conf_21_30") || fieldNames.Contains("wordconfidenceinterval21to30"))
                obj.WordConfidenceInterval21To30 = repObj.WordConfidenceInterval21To30;

            if (fieldNames.Contains("word_conf_31_40") || fieldNames.Contains("wordconfidenceinterval31to40"))
                obj.WordConfidenceInterval31To40 = repObj.WordConfidenceInterval31To40;

            if (fieldNames.Contains("word_conf_41_50") || fieldNames.Contains("wordconfidenceinterval41to50"))
                obj.WordConfidenceInterval41To50 = repObj.WordConfidenceInterval41To50;

            if (fieldNames.Contains("word_conf_51_60") || fieldNames.Contains("wordconfidenceinterval51to60"))
                obj.WordConfidenceInterval51To60 = repObj.WordConfidenceInterval51To60;

            if (fieldNames.Contains("word_conf_61_70") || fieldNames.Contains("wordconfidenceinterval61to70"))
                obj.WordConfidenceInterval61To70 = repObj.WordConfidenceInterval61To70;

            if (fieldNames.Contains("word_conf_71_80") || fieldNames.Contains("wordconfidenceinterval71to80"))
                obj.WordConfidenceInterval71To80 = repObj.WordConfidenceInterval71To80;

            if (fieldNames.Contains("word_conf_81_90") || fieldNames.Contains("wordconfidenceinterval81to90"))
                obj.WordConfidenceInterval81To90 = repObj.WordConfidenceInterval81To90;

            if (fieldNames.Contains("word_conf_91_100") || fieldNames.Contains("wordconfidenceinterval91to100"))
                obj.WordConfidenceInterval91To100 = repObj.WordConfidenceInterval91To100;

            if (fieldNames.Contains("xxhash364"))
                obj.xxHash364 = repObj.xxHash364;

            if (fieldNames.Contains("xxhash3128"))
                obj.xxHash3128 = repObj.xxHash3128;

            // DataArea
            if (fieldNames.Contains("dataarea.endianness"))
                obj.DataAreaEndianness = repObj.DataAreaEndianness;

            if (fieldNames.Contains("dataarea.name"))
                obj.DataAreaName = repObj.DataAreaName;

            if (fieldNames.Contains("dataarea.size"))
                obj.DataAreaSize = repObj.DataAreaSize;

            if (fieldNames.Contains("dataarea.width"))
                obj.DataAreaWidth = repObj.DataAreaWidth;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Sample obj, Sample repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Serials obj, Serials repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("boxbarcode"))
                obj.BoxBarcode = repObj.BoxBarcode;

            if (fieldNames.Contains("boxserial"))
                obj.BoxSerial = repObj.BoxSerial;

            if (fieldNames.Contains("chipserial"))
                obj.ChipSerial = repObj.ChipSerial;

            if (fieldNames.Contains("digitalserial1"))
                obj.DigitalSerial1 = repObj.DigitalSerial1;

            if (fieldNames.Contains("digitalserial2"))
                obj.DigitalSerial2 = repObj.DigitalSerial2;

            if (fieldNames.Contains("lockoutserial"))
                obj.LockoutSerial = repObj.LockoutSerial;

            if (fieldNames.Contains("mediaserial1"))
                obj.MediaSerial1 = repObj.MediaSerial1;

            if (fieldNames.Contains("mediaserial2"))
                obj.MediaSerial2 = repObj.MediaSerial2;

            if (fieldNames.Contains("mediaserial3"))
                obj.MediaSerial3 = repObj.MediaSerial3;

            if (fieldNames.Contains("mediastamp"))
                obj.MediaStamp = repObj.MediaStamp;

            if (fieldNames.Contains("pcbserial"))
                obj.PCBSerial = repObj.PCBSerial;

            if (fieldNames.Contains("romchipserial1"))
                obj.RomChipSerial1 = repObj.RomChipSerial1;

            if (fieldNames.Contains("romchipserial2"))
                obj.RomChipSerial2 = repObj.RomChipSerial2;

            if (fieldNames.Contains("savechipserial"))
                obj.SaveChipSerial = repObj.SaveChipSerial;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(SharedFeat obj, SharedFeat repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("value"))
                obj.Value = repObj.Value;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Slot obj, Slot repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            // Replacing SlotOption is not possible
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(SlotOption obj, SlotOption repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("default"))
                obj.Default = repObj.Default;

            if (fieldNames.Contains("devname"))
                obj.DevName = repObj.DevName;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(SoftwareList obj, SoftwareList repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("filter"))
                obj.Filter = repObj.Filter;

            if (fieldNames.Contains("name"))
                obj.Name = repObj.Name;

            if (fieldNames.Contains("status"))
                obj.Status = repObj.Status;

            if (fieldNames.Contains("tag"))
                obj.Tag = repObj.Tag;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(Sound obj, Sound repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("channels"))
                obj.Channels = repObj.Channels;
        }

        /// <summary>
        /// Replace fields from matched items
        /// </summary>
        private static void ReplaceFields(SourceDetails obj, SourceDetails repObj, HashSet<string> fieldNames)
        {
            if (fieldNames.Contains("appendtonumber"))
                obj.AppendToNumber = repObj.AppendToNumber;

            if (fieldNames.Contains("comment1"))
                obj.Comment1 = repObj.Comment1;

            if (fieldNames.Contains("comment2"))
                obj.Comment2 = repObj.Comment2;

            if (fieldNames.Contains("dumpdate"))
                obj.DumpDate = repObj.DumpDate;

            if (fieldNames.Contains("dumpdateinfo"))
                obj.DumpDateInfo = repObj.DumpDateInfo;

            if (fieldNames.Contains("dumper"))
                obj.Dumper = repObj.Dumper;

            if (fieldNames.Contains("id"))
                obj.Id = repObj.Id;

            if (fieldNames.Contains("link1"))
                obj.Link1 = repObj.Link1;

            if (fieldNames.Contains("link1public"))
                obj.Link1Public = repObj.Link1Public;

            if (fieldNames.Contains("link2"))
                obj.Link2 = repObj.Link2;

            if (fieldNames.Contains("link2public"))
                obj.Link2Public = repObj.Link2Public;

            if (fieldNames.Contains("link3"))
                obj.Link3 = repObj.Link3;

            if (fieldNames.Contains("link3public"))
                obj.Link3Public = repObj.Link3Public;

            if (fieldNames.Contains("mediatitle"))
                obj.MediaTitle = repObj.MediaTitle;

            if (fieldNames.Contains("nodump"))
                obj.Nodump = repObj.Nodump;

            if (fieldNames.Contains("origin"))
                obj.Origin = repObj.Origin;

            if (fieldNames.Contains("originalformat"))
                obj.OriginalFormat = repObj.OriginalFormat;

            if (fieldNames.Contains("project"))
                obj.Project = repObj.Project;

            if (fieldNames.Contains("region"))
                obj.Region = repObj.Region;

            if (fieldNames.Contains("releasedate"))
                obj.ReleaseDate = repObj.ReleaseDate;

            if (fieldNames.Contains("releasedateinfo"))
                obj.ReleaseDateInfo = repObj.ReleaseDateInfo;

            if (fieldNames.Contains("rominfo"))
                obj.RomInfo = repObj.RomInfo;

            if (fieldNames.Contains("section"))
                obj.Section = repObj.Section;

            if (fieldNames.Contains("tool"))
                obj.Tool = repObj.Tool;
        }

        #endregion
    }
}
