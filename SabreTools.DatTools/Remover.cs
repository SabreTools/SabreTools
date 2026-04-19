using System.Collections.Generic;
using SabreTools.Data.Extensions;
using SabreTools.Logging;
using SabreTools.Metadata.DatFiles;
using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;
using SabreTools.Metadata.Filter;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Represents the removal operations that need to be performed on a set of items, usually a DAT
    /// </summary>
    public class Remover
    {
        #region Fields

        /// <summary>
        /// List of header fields to exclude from writing
        /// </summary>
        public readonly List<string> HeaderFieldNames = [];

        /// <summary>
        /// List of machine fields to exclude from writing
        /// </summary>
        public readonly List<string> MachineFieldNames = [];

        /// <summary>
        /// List of fields to exclude from writing
        /// </summary>
        public readonly Dictionary<string, List<string>> ItemFieldNames = [];

        #endregion

        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        protected Logger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        public Remover()
        {
            _logger = new Logger(this);
        }

        #endregion

        #region Population

        /// <summary>
        /// Populate the exclusion objects using a field name
        /// </summary>
        /// <param name="field">Field name</param>
        public void PopulateExclusions(string field)
            => PopulateExclusions([field]);

        /// <summary>
        /// Populate the exclusion objects using a set of field names
        /// </summary>
        /// <param name="fields">List of field names</param>
        public void PopulateExclusions(List<string>? fields)
        {
            // If the list is null or empty, just return
            if (fields is null || fields.Count == 0)
                return;

            var watch = new InternalStopwatch("Populating removals from list");

            foreach (string field in fields)
            {
                bool removerSet = SetRemover(field);
                if (!removerSet)
                    _logger.Warning($"The value {field} did not match any known field names. Please check the wiki for more details on supported field names.");
            }

            watch.Stop();
        }

        /// <summary>
        /// Set remover from a value
        /// </summary>
        /// <param name="field">Key for the remover to be set</param>
        private bool SetRemover(string field)
        {
            // If the key is null or empty, return false
            if (string.IsNullOrEmpty(field))
                return false;

            // Get the parser pair out of it, if possible
            try
            {
                var key = new FilterKey(field);
                switch (key.ItemName)
                {
                    case "header":
                        HeaderFieldNames.Add(key.FieldName);
                        return true;

                    case "machine":
                        MachineFieldNames.Add(key.FieldName);
                        return true;

                    default:
                        string itemName = key.ItemName;
                        string fieldName = key.FieldName;

                        // Replace some specific prefixes
                        if (key.ItemName == "dataarea")
                        {
                            itemName = "rom";
                            fieldName = $"dataarea.{fieldName}";
                        }
                        else if (key.ItemName == "diskarea")
                        {
                            itemName = "disk";
                            fieldName = $"diskarea.{fieldName}";
                        }
                        // TODO: Handle Part since that splits into 4 item names
                        else if (key.ItemName == "video")
                        {
                            itemName = "display";
                        }

                        if (!ItemFieldNames.ContainsKey(key.ItemName))
                            ItemFieldNames[key.ItemName] = [];

                        ItemFieldNames[itemName].Add(fieldName);
                        return true;
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Running

        /// <summary>
        /// Remove fields from a DatFile
        /// </summary>
        /// <param name="datFile">Current DatFile object to run operations on</param>
        public void ApplyRemovals(DatFile datFile)
        {
            InternalStopwatch watch = new("Applying removals to DAT");

            RemoveFields(datFile.Header);
            RemoveItemFields(datFile.Items);
            RemoveItemFieldsDB(datFile.ItemsDB);

            watch.Stop();
        }

        /// <summary>
        /// Apply removals to the item dictionary
        /// </summary>
        public void RemoveItemFields(ItemDictionary? itemDictionary)
        {
            // If we have an invalid input, return
            if (itemDictionary is null || (MachineFieldNames.Count == 0 && ItemFieldNames.Count == 0))
                return;

            foreach (var key in itemDictionary.SortedKeys)
            {
                List<DatItem>? items = itemDictionary.GetItemsForBucket(key);
                if (items is null)
                    continue;

                for (int j = 0; j < items.Count; j++)
                {
                    // Handle machine removals
                    var machine = items[j].Machine;
                    RemoveFields(machine);

                    // Handle item removals
                    RemoveFields(items[j]);
                }
            }
        }

        /// <summary>
        /// Apply removals to the item dictionary
        /// </summary>
        public void RemoveItemFieldsDB(ItemDatabase? itemDatabase)
        {
            // If we have an invalid input, return
            if (itemDatabase is null || (MachineFieldNames.Count == 0 && ItemFieldNames.Count == 0))
                return;

            // Handle machine removals
            foreach (var kvp in itemDatabase.GetMachines())
            {
                RemoveFields(kvp.Value);
            }

            // Handle item removals
            foreach (var key in itemDatabase.SortedKeys)
            {
                var items = itemDatabase.GetItemsForBucket(key);
                if (items is null)
                    continue;

                foreach (var item in items.Values)
                {
                    RemoveFields(item);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="datItem">DatItem to remove fields from</param>
        internal void RemoveFields(DatItem? datItem)
        {
            if (datItem is null)
                return;

            #region Common

            // If there are no field names, return
            if (ItemFieldNames is null || ItemFieldNames.Count == 0)
                return;

            // If there are no field names for this type or generic, return
            string? itemType = datItem.ItemType.AsStringValue();
            if (itemType is null || (!ItemFieldNames.ContainsKey(itemType) && !ItemFieldNames.ContainsKey("item")))
                return;

            // Get the combined list of fields to remove
            var fieldNames = new HashSet<string>();
            if (ItemFieldNames.TryGetValue(itemType, out List<string>? value))
                fieldNames.UnionWith(value);
            if (ItemFieldNames.TryGetValue("item", out value))
                fieldNames.UnionWith(value);

            #endregion

            #region Item-Specific

            // Handle all item types
            switch (datItem)
            {
                case Adjuster obj: RemoveFields(obj, fieldNames); break;
                case Archive obj: RemoveFields(obj, fieldNames); break;
                case BiosSet obj: RemoveFields(obj, fieldNames); break;
                case Chip obj: RemoveFields(obj, fieldNames); break;
                case Configuration obj: RemoveFields(obj, fieldNames); break;
                case ConfLocation obj: RemoveFields(obj, fieldNames); break;
                case ConfSetting obj: RemoveFields(obj, fieldNames); break;
                case Control obj: RemoveFields(obj, fieldNames); break;
                case Device obj: RemoveFields(obj, fieldNames); break;
                case DeviceRef obj: RemoveFields(obj, fieldNames); break;
                case DipLocation obj: RemoveFields(obj, fieldNames); break;
                case DipSwitch obj: RemoveFields(obj, fieldNames); break;
                case DipValue obj: RemoveFields(obj, fieldNames); break;
                case Disk obj: RemoveFields(obj, fieldNames); break;
                case Display obj: RemoveFields(obj, fieldNames); break;
                case Driver obj: RemoveFields(obj, fieldNames); break;
                case Feature obj: RemoveFields(obj, fieldNames); break;
                case Info obj: RemoveFields(obj, fieldNames); break;
                case Input obj: RemoveFields(obj, fieldNames); break;
                case Media obj: RemoveFields(obj, fieldNames); break;
                // case Original obj: RemoveFields(obj, fields); break;
                // TODO: Part needs to be wrapped into DipSwitch, Disk, PartFeature, and Rom
                case PartFeature obj: RemoveFields(obj, fieldNames); break;
                case Port obj: RemoveFields(obj, fieldNames); break;
                case RamOption obj: RemoveFields(obj, fieldNames); break;
                case Release obj: RemoveFields(obj, fieldNames); break;
                case ReleaseDetails obj: RemoveFields(obj, fieldNames); break;
                case Rom obj: RemoveFields(obj, fieldNames); break;
                case Sample obj: RemoveFields(obj, fieldNames); break;
                case Serials obj: RemoveFields(obj, fieldNames); break;
                case SharedFeat obj: RemoveFields(obj, fieldNames); break;
                case Slot obj: RemoveFields(obj, fieldNames); break;
                case SlotOption obj: RemoveFields(obj, fieldNames); break;
                case SoftwareList obj: RemoveFields(obj, fieldNames); break;
                case Sound obj: RemoveFields(obj, fieldNames); break;
                case SourceDetails obj: RemoveFields(obj, fieldNames); break;

                // Ignore types not handled by this class
                default: break;
            }

            #endregion
        }

        #endregion

        #region Per-Type Removal

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        public void RemoveFields(DatHeader? obj)
        {
            // If we have an invalid input, return
            if (obj is null || HeaderFieldNames.Count == 0)
                return;

            foreach (var fieldName in HeaderFieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "author":
                        obj.Author = null;
                        removed = true;
                        break;
                    case "biosmode":
                        obj.BiosMode = 0x00;
                        removed = true;
                        break;
                    case "build":
                        obj.Build = null;
                        removed = true;
                        break;
                    // Header.CanOpen is intentionally skipped
                    case "category":
                        obj.Category = null;
                        removed = true;
                        break;
                    case "comment":
                        obj.Comment = null;
                        removed = true;
                        break;
                    case "date":
                        obj.Date = null;
                        removed = true;
                        break;
                    case "datversion":
                        obj.DatVersion = null;
                        removed = true;
                        break;
                    case "debug":
                        obj.Debug = null;
                        removed = true;
                        break;
                    case "description":
                        obj.Description = null;
                        removed = true;
                        break;
                    case "email":
                        obj.Email = null;
                        removed = true;
                        break;
                    case "emulatorversion":
                        obj.EmulatorVersion = null;
                        removed = true;
                        break;
                    case "filename":
                        obj.FileName = null;
                        removed = true;
                        break;
                    case "forcemerging":
                        obj.ForceMerging = 0x00;
                        removed = true;
                        break;
                    case "forcenodump":
                        obj.ForceNodump = 0x00;
                        removed = true;
                        break;
                    case "forcepacking":
                        obj.ForcePacking = 0x00;
                        removed = true;
                        break;
                    case "forcezipping":
                        obj.ForceZipping = null;
                        removed = true;
                        break;
                    // Header.HeaderRow is intentionally skipped
                    case "header":
                    case "headerskipper":
                    case "skipper":
                        obj.HeaderSkipper = null;
                        removed = true;
                        break;
                    case "homepage":
                        obj.Homepage = null;
                        removed = true;
                        break;
                    case "id":
                        obj.Id = null;
                        removed = true;
                        break;
                    // Header.Images is intentionally skipped
                    case "imfolder":
                        obj.ImFolder = null;
                        removed = true;
                        break;
                    // Header.Infos is intentionally skipped
                    case "lockbiosmode":
                        obj.LockBiosMode = null;
                        removed = true;
                        break;
                    case "lockrommode":
                        obj.LockRomMode = null;
                        removed = true;
                        break;
                    case "locksamplemode":
                        obj.LockSampleMode = null;
                        removed = true;
                        break;
                    case "mameconfig":
                        obj.MameConfig = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    // Header.NewDat is intentionally skipped
                    case "notes":
                        obj.Notes = null;
                        removed = true;
                        break;
                    case "plugin":
                        obj.Plugin = null;
                        removed = true;
                        break;
                    case "refname":
                        obj.RefName = null;
                        removed = true;
                        break;
                    case "rommode":
                        obj.RomMode = 0x00;
                        removed = true;
                        break;
                    case "romtitle":
                        obj.RomTitle = null;
                        removed = true;
                        break;
                    case "rootdir":
                        obj.RootDir = null;
                        removed = true;
                        break;
                    case "samplemode":
                        obj.SampleMode = 0x00;
                        removed = true;
                        break;
                    case "schemalocation":
                        obj.SchemaLocation = null;
                        removed = true;
                        break;
                    case "screenshotsheight":
                        obj.ScreenshotsHeight = null;
                        removed = true;
                        break;
                    case "screenshotswidth":
                        obj.ScreenshotsWidth = null;
                        removed = true;
                        break;
                    // Header.Search is intentionally skipped
                    case "system":
                        obj.System = null;
                        removed = true;
                        break;
                    case "timestamp":
                        obj.Timestamp = null;
                        removed = true;
                        break;
                    case "type":
                        obj.Type = null;
                        removed = true;
                        break;
                    case "url":
                        obj.Url = null;
                        removed = true;
                        break;
                    case "version":
                        obj.Version = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Header field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        public void RemoveFields(Machine? obj)
        {
            // If we have an invalid input, return
            if (obj is null || MachineFieldNames.Count == 0)
                return;

            foreach (var fieldName in MachineFieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "board":
                        obj.Board = null;
                        removed = true;
                        break;
                    case "buttons":
                        obj.Buttons = null;
                        removed = true;
                        break;
                    case "category":
                        obj.Category = null;
                        removed = true;
                        break;
                    case "cloneof":
                        obj.CloneOf = null;
                        removed = true;
                        break;
                    case "cloneofid":
                        obj.CloneOfId = null;
                        removed = true;
                        break;
                    case "comment":
                        obj.Comment = null;
                        removed = true;
                        break;
                    case "company":
                        obj.Company = null;
                        removed = true;
                        break;
                    case "control":
                        obj.Control = null;
                        removed = true;
                        break;
                    case "crc":
                        obj.CRC = null;
                        removed = true;
                        break;
                    case "country":
                        obj.Country = null;
                        removed = true;
                        break;
                    case "description":
                        obj.Description = null;
                        removed = true;
                        break;
                    case "developer":
                        obj.Developer = null;
                        removed = true;
                        break;
                    case "dirname":
                        obj.DirName = null;
                        removed = true;
                        break;
                    case "displaycount":
                        obj.DisplayCount = null;
                        removed = true;
                        break;
                    case "displaytype":
                        obj.DisplayType = null;
                        removed = true;
                        break;
                    case "duplicateid":
                        obj.DuplicateID = null;
                        removed = true;
                        break;
                    case "emulator":
                        obj.Emulator = null;
                        removed = true;
                        break;
                    case "enabled":
                        obj.Enabled = null;
                        removed = true;
                        break;
                    case "extra":
                        obj.Extra = null;
                        removed = true;
                        break;
                    case "favorite":
                        obj.Favorite = null;
                        removed = true;
                        break;
                    case "genmsxid":
                        obj.GenMSXID = null;
                        removed = true;
                        break;
                    case "genre":
                        obj.Genre = null;
                        removed = true;
                        break;
                    case "hash":
                        obj.Hash = null;
                        removed = true;
                        break;
                    case "history":
                        obj.History = null;
                        removed = true;
                        break;
                    case "id":
                        obj.Id = null;
                        removed = true;
                        break;
                    case "im1crc":
                        obj.Im1CRC = null;
                        removed = true;
                        break;
                    case "im2crc":
                        obj.Im2CRC = null;
                        removed = true;
                        break;
                    case "imagenumber":
                        obj.ImageNumber = null;
                        removed = true;
                        break;
                    case "isbios":
                        obj.IsBios = null;
                        removed = true;
                        break;
                    case "isdevice":
                        obj.IsDevice = null;
                        removed = true;
                        break;
                    case "ismechanical":
                        obj.IsMechanical = null;
                        removed = true;
                        break;
                    case "language":
                        obj.Language = null;
                        removed = true;
                        break;
                    case "location":
                        obj.Location = null;
                        removed = true;
                        break;
                    case "manufacturer":
                        obj.Manufacturer = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "notes":
                        obj.Notes = null;
                        removed = true;
                        break;
                    case "playedcount":
                        obj.PlayedCount = null;
                        removed = true;
                        break;
                    case "playedtime":
                        obj.PlayedTime = null;
                        removed = true;
                        break;
                    case "players":
                        obj.Players = null;
                        removed = true;
                        break;
                    case "publisher":
                        obj.Publisher = null;
                        removed = true;
                        break;
                    case "ratings":
                        obj.Ratings = null;
                        removed = true;
                        break;
                    case "rebuildto":
                        obj.RebuildTo = null;
                        removed = true;
                        break;
                    case "relatedto":
                        obj.RelatedTo = null;
                        removed = true;
                        break;
                    case "releasenumber":
                        obj.ReleaseNumber = null;
                        removed = true;
                        break;
                    case "romof":
                        obj.RomOf = null;
                        removed = true;
                        break;
                    case "rotation":
                        obj.Rotation = null;
                        removed = true;
                        break;
                    case "runnable":
                        obj.Runnable = null;
                        removed = true;
                        break;
                    case "sampleof":
                        obj.SampleOf = null;
                        removed = true;
                        break;
                    case "savetype":
                        obj.SaveType = null;
                        removed = true;
                        break;
                    case "score":
                        obj.Score = null;
                        removed = true;
                        break;
                    case "source":
                        obj.Source = null;
                        removed = true;
                        break;
                    case "sourcefile":
                        obj.SourceFile = null;
                        removed = true;
                        break;
                    case "sourcerom":
                        obj.SourceRom = null;
                        removed = true;
                        break;
                    case "status":
                        obj.Status = null;
                        removed = true;
                        break;
                    case "subgenre":
                        obj.Subgenre = null;
                        removed = true;
                        break;
                    case "supported":
                        obj.Supported = null;
                        removed = true;
                        break;
                    case "system":
                        obj.System = null;
                        removed = true;
                        break;
                    case "tags":
                        obj.Tags = null;
                        removed = true;
                        break;
                    case "titleid":
                        obj.TitleID = null;
                        removed = true;
                        break;
                    case "url":
                        obj.Url = null;
                        removed = true;
                        break;
                    case "year":
                        obj.Year = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Machine field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Adjuster? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "condition.mask":
                        obj.ConditionMask = null;
                        removed = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = null;
                        removed = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = null;
                        removed = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = null;
                        removed = true;
                        break;
                    case "default":
                        obj.Default = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Archive? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "additional":
                        obj.Additional = null;
                        removed = true;
                        break;
                    case "adult":
                        obj.Adult = null;
                        removed = true;
                        break;
                    case "alt":
                        obj.Alt = null;
                        removed = true;
                        break;
                    case "bios":
                        obj.Bios = null;
                        removed = true;
                        break;
                    case "categories":
                        obj.Categories = null;
                        removed = true;
                        break;
                    case "clone":
                    case "clonetag":
                        obj.CloneTag = null;
                        removed = true;
                        break;
                    case "complete":
                        obj.Complete = null;
                        removed = true;
                        break;
                    case "dat":
                        obj.Dat = null;
                        removed = true;
                        break;
                    case "datternote":
                        obj.DatterNote = null;
                        removed = true;
                        break;
                    case "description":
                        obj.Description = null;
                        removed = true;
                        break;
                    case "devstatus":
                        obj.DevStatus = null;
                        removed = true;
                        break;
                    case "gameid1":
                        obj.GameId1 = null;
                        removed = true;
                        break;
                    case "gameid2":
                        obj.GameId2 = null;
                        removed = true;
                        break;
                    case "langchecked":
                        obj.LangChecked = null;
                        removed = true;
                        break;
                    case "languages":
                        obj.Languages = null;
                        removed = true;
                        break;
                    case "licensed":
                        obj.Licensed = null;
                        removed = true;
                        break;
                    case "listed":
                        obj.Listed = null;
                        removed = true;
                        break;
                    case "mergeof":
                        obj.MergeOf = null;
                        removed = true;
                        break;
                    case "mergename":
                        obj.MergeName = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "namealt":
                        obj.NameAlt = null;
                        removed = true;
                        break;
                    case "number":
                        obj.Number = null;
                        removed = true;
                        break;
                    case "physical":
                        obj.Physical = null;
                        removed = true;
                        break;
                    case "pirate":
                        obj.Pirate = null;
                        removed = true;
                        break;
                    case "private":
                        obj.Private = null;
                        removed = true;
                        break;
                    case "region":
                        obj.Region = null;
                        removed = true;
                        break;
                    case "regparent":
                        obj.RegParent = null;
                        removed = true;
                        break;
                    case "showlang":
                        obj.ShowLang = null;
                        removed = true;
                        break;
                    case "special1":
                        obj.Special1 = null;
                        removed = true;
                        break;
                    case "special2":
                        obj.Special2 = null;
                        removed = true;
                        break;
                    case "stickynote":
                        obj.StickyNote = null;
                        removed = true;
                        break;
                    case "version1":
                        obj.Version1 = null;
                        removed = true;
                        break;
                    case "version2":
                        obj.Version2 = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(BiosSet? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "default":
                        obj.Default = null;
                        removed = true;
                        break;
                    case "description":
                        obj.Description = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Chip? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "chiptype":
                        obj.ChipType = null;
                        removed = true;
                        break;
                    case "clock":
                        obj.Clock = null;
                        removed = true;
                        break;
                    case "flags":
                        obj.Flags = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "soundonly":
                        obj.SoundOnly = null;
                        removed = true;
                        break;
                    case "tag":
                        obj.Tag = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Configuration? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "condition.mask":
                        obj.ConditionMask = null;
                        removed = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = null;
                        removed = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = null;
                        removed = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = null;
                        removed = true;
                        break;
                    case "mask":
                        obj.Mask = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "tag":
                        obj.Tag = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }

            if (obj.ConfLocation is not null)
            {
                foreach (var confLocation in obj.ConfLocation)
                {
                    RemoveFields(confLocation);
                }
            }

            if (obj.ConfSetting is not null)
            {
                foreach (var confSetting in obj.ConfSetting)
                {
                    RemoveFields(confSetting);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(ConfLocation? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "inverted":
                        obj.Inverted = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "number":
                        obj.Number = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(ConfSetting? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "condition.mask":
                        obj.ConditionMask = null;
                        removed = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = null;
                        removed = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = null;
                        removed = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = null;
                        removed = true;
                        break;
                    case "default":
                        obj.Default = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "value":
                        obj.Value = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Control? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "buttons":
                        obj.Buttons = null;
                        removed = true;
                        break;
                    case "controltype":
                        obj.ControlType = null;
                        removed = true;
                        break;
                    case "keydelta":
                        obj.KeyDelta = null;
                        removed = true;
                        break;
                    case "maximum":
                        obj.Maximum = null;
                        removed = true;
                        break;
                    case "minimum":
                        obj.Minimum = null;
                        removed = true;
                        break;
                    case "player":
                        obj.Player = null;
                        removed = true;
                        break;
                    case "reqbuttons":
                        obj.ReqButtons = null;
                        removed = true;
                        break;
                    case "reverse":
                        obj.Reverse = null;
                        removed = true;
                        break;
                    case "sensitivity":
                        obj.Sensitivity = null;
                        removed = true;
                        break;
                    case "ways":
                        obj.Ways = null;
                        removed = true;
                        break;
                    case "ways2":
                        obj.Ways2 = null;
                        removed = true;
                        break;
                    case "ways3":
                        obj.Ways3 = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Device? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "devicetype":
                        obj.DeviceType = null;
                        removed = true;
                        break;
                    case "extension.name":
                        obj.ExtensionName = null;
                        removed = true;
                        break;
                    case "fixedimage":
                        obj.FixedImage = null;
                        removed = true;
                        break;
                    case "instance.briefname":
                        obj.InstanceBriefName = null;
                        removed = true;
                        break;
                    case "instance.name":
                        obj.InstanceName = null;
                        removed = true;
                        break;
                    case "interface":
                        obj.Interface = null;
                        removed = true;
                        break;
                    case "mandatory":
                        obj.Mandatory = null;
                        removed = true;
                        break;
                    case "tag":
                        obj.Tag = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(DeviceRef? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(DipLocation? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "inverted":
                        obj.Inverted = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "number":
                        obj.Number = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(DipSwitch? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "condition.mask":
                        obj.ConditionMask = null;
                        removed = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = null;
                        removed = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = null;
                        removed = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = null;
                        removed = true;
                        break;
                    case "default":
                        obj.Default = null;
                        removed = true;
                        break;
                    case "mask":
                        obj.Mask = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "tag":
                        obj.Tag = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }

            if (obj.DipLocation is not null)
            {
                foreach (var dipLocation in obj.DipLocation)
                {
                    RemoveFields(dipLocation);
                }
            }

            if (obj.DipValue is not null)
            {
                foreach (var dipValue in obj.DipValue)
                {
                    RemoveFields(dipValue);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(DipValue? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "condition.mask":
                        obj.ConditionMask = null;
                        removed = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = null;
                        removed = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = null;
                        removed = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = null;
                        removed = true;
                        break;
                    case "default":
                        obj.Default = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "value":
                        obj.Value = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Disk? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "flags":
                        obj.Flags = null;
                        removed = true;
                        break;
                    case "index":
                        obj.Index = null;
                        removed = true;
                        break;
                    case "md5":
                        obj.MD5 = null;
                        removed = true;
                        break;
                    case "merge":
                        obj.Merge = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "optional":
                        obj.Optional = null;
                        removed = true;
                        break;
                    case "region":
                        obj.Region = null;
                        removed = true;
                        break;
                    case "sha1":
                        obj.SHA1 = null;
                        removed = true;
                        break;
                    case "status":
                        obj.Status = null;
                        removed = true;
                        break;
                    case "writable":
                        obj.Writable = null;
                        removed = true;
                        break;

                    // DiskArea
                    case "diskarea.name":
                        obj.DiskAreaName = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Display? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "aspectx":
                        obj.AspectX = null;
                        removed = true;
                        break;
                    case "aspecty":
                        obj.AspectY = null;
                        removed = true;
                        break;
                    case "displaytype":
                    case "screen":
                        obj.DisplayType = null;
                        removed = true;
                        break;
                    case "flipx":
                        obj.FlipX = null;
                        removed = true;
                        break;
                    case "hbend":
                        obj.HBEnd = null;
                        removed = true;
                        break;
                    case "hbstart":
                        obj.HBStart = null;
                        removed = true;
                        break;
                    case "height":
                    case "y":
                        obj.Height = null;
                        removed = true;
                        break;
                    case "htotal":
                        obj.HTotal = null;
                        removed = true;
                        break;
                    case "pixclock":
                        obj.PixClock = null;
                        removed = true;
                        break;
                    case "refresh":
                    case "freq":
                        obj.Refresh = null;
                        removed = true;
                        break;
                    case "rotate":
                    case "orientation":
                        obj.Rotate = null;
                        removed = true;
                        break;
                    case "tag":
                        obj.Tag = null;
                        removed = true;
                        break;
                    case "vbend":
                        obj.VBEnd = null;
                        removed = true;
                        break;
                    case "vbstart":
                        obj.VBStart = null;
                        removed = true;
                        break;
                    case "vtotal":
                        obj.VTotal = null;
                        removed = true;
                        break;
                    case "width":
                    case "x":
                        obj.Width = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Driver? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "blit":
                        obj.Blit = null;
                        removed = true;
                        break;
                    case "cocktail":
                        obj.Cocktail = null;
                        removed = true;
                        break;
                    case "color":
                        obj.Color = null;
                        removed = true;
                        break;
                    case "emulation":
                        obj.Emulation = null;
                        removed = true;
                        break;
                    case "incomplete":
                        obj.Incomplete = null;
                        removed = true;
                        break;
                    case "nosoundhardware":
                        obj.NoSoundHardware = null;
                        removed = true;
                        break;
                    case "palettesize":
                        obj.PaletteSize = null;
                        removed = true;
                        break;
                    case "requiresartwork":
                        obj.RequiresArtwork = null;
                        removed = true;
                        break;
                    case "savestate":
                        obj.SaveState = null;
                        removed = true;
                        break;
                    case "sound":
                        obj.Sound = null;
                        removed = true;
                        break;
                    case "status":
                        obj.Status = null;
                        removed = true;
                        break;
                    case "unofficial":
                        obj.Unofficial = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Feature? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "featuretype":
                        obj.FeatureType = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "overall":
                        obj.Overall = null;
                        removed = true;
                        break;
                    case "status":
                        obj.Status = null;
                        removed = true;
                        break;
                    case "value":
                        obj.Value = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Info? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "value":
                        obj.Value = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Input? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "buttons":
                        obj.Buttons = null;
                        removed = true;
                        break;
                    case "coins":
                        obj.Coins = null;
                        removed = true;
                        break;
                    case "control":
                    case "controlattr":
                        obj.ControlAttr = null;
                        removed = true;
                        break;
                    case "players":
                        obj.Players = null;
                        removed = true;
                        break;
                    case "service":
                        obj.Service = null;
                        removed = true;
                        break;
                    case "tilt":
                        obj.Tilt = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }

            if (obj.Control is not null)
            {
                foreach (var control in obj.Control)
                {
                    RemoveFields(control);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Media? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "md5":
                        obj.MD5 = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "sha1":
                        obj.SHA1 = null;
                        removed = true;
                        break;
                    case "sha256":
                        obj.SHA256 = null;
                        removed = true;
                        break;
                    case "spamsum":
                        obj.SpamSum = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(PartFeature? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "featuretype":
                        obj.FeatureType = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "overall":
                        obj.Overall = null;
                        removed = true;
                        break;
                    case "status":
                        obj.Status = null;
                        removed = true;
                        break;
                    case "value":
                        obj.Value = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Port? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "analog.mask":
                        obj.AnalogMask = null;
                        removed = true;
                        break;
                    case "tag":
                        obj.Tag = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(RamOption? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "content":
                        obj.Content = null;
                        removed = true;
                        break;
                    case "default":
                        obj.Default = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Release? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "date":
                        obj.Date = null;
                        removed = true;
                        break;
                    case "default":
                        obj.Default = null;
                        removed = true;
                        break;
                    case "language":
                        obj.Language = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "region":
                        obj.Region = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(ReleaseDetails? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "appendtonumber":
                        obj.AppendToNumber = null;
                        removed = true;
                        break;
                    case "archivename":
                        obj.ArchiveName = null;
                        removed = true;
                        break;
                    case "category":
                        obj.Category = null;
                        removed = true;
                        break;
                    case "comment":
                        obj.Comment = null;
                        removed = true;
                        break;
                    case "date":
                        obj.Date = null;
                        removed = true;
                        break;
                    case "dirname":
                        obj.DirName = null;
                        removed = true;
                        break;
                    case "group":
                        obj.Group = null;
                        removed = true;
                        break;
                    case "id":
                        obj.Id = null;
                        removed = true;
                        break;
                    case "nfocrc":
                        obj.NfoCRC = null;
                        removed = true;
                        break;
                    case "nfoname":
                        obj.NfoName = null;
                        removed = true;
                        break;
                    case "nfosize":
                        obj.NfoSize = null;
                        removed = true;
                        break;
                    case "origin":
                        obj.Origin = null;
                        removed = true;
                        break;
                    case "originalformat":
                        obj.OriginalFormat = null;
                        removed = true;
                        break;
                    case "region":
                        obj.Region = null;
                        removed = true;
                        break;
                    case "rominfo":
                        obj.RomInfo = null;
                        removed = true;
                        break;
                    case "tool":
                        obj.Tool = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Rom? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "album":
                        obj.Album = null;
                        removed = true;
                        break;
                    case "alt_romname":
                    case "altromname":
                        obj.AltRomname = null;
                        removed = true;
                        break;
                    case "alt_title":
                    case "alttitle":
                        obj.AltTitle = null;
                        removed = true;
                        break;
                    case "artist":
                        obj.Artist = null;
                        removed = true;
                        break;
                    case "asr_detected_lang":
                    case "asrdetectedlang":
                        obj.ASRDetectedLang = null;
                        removed = true;
                        break;
                    case "asr_detected_lang_conf":
                    case "asrdetectedlangconf":
                        obj.ASRDetectedLangConf = null;
                        removed = true;
                        break;
                    case "asr_transcribed_lang":
                    case "asrtranscribedlang":
                        obj.ASRTranscribedLang = null;
                        removed = true;
                        break;
                    case "bios":
                        obj.Bios = null;
                        removed = true;
                        break;
                    case "bitrate":
                        obj.Bitrate = null;
                        removed = true;
                        break;
                    case "btih":
                    case "bittorrentmagnethash":
                        obj.BitTorrentMagnetHash = null;
                        removed = true;
                        break;
                    case "cloth_cover_detection_module_version":
                    case "clothcoverdetectionmoduleversion":
                        obj.ClothCoverDetectionModuleVersion = null;
                        removed = true;
                        break;
                    case "collection-catalog-number":
                    case "collectioncatalognumber":
                        obj.CollectionCatalogNumber = null;
                        removed = true;
                        break;
                    case "comment":
                        obj.Comment = null;
                        removed = true;
                        break;
                    case "crc16":
                        obj.CRC16 = null;
                        removed = true;
                        break;
                    case "crc":
                    case "crc32":
                        obj.CRC32 = null;
                        removed = true;
                        break;
                    case "crc64":
                        obj.CRC64 = null;
                        removed = true;
                        break;
                    case "creator":
                        obj.Creator = null;
                        removed = true;
                        break;
                    case "date":
                        obj.Date = null;
                        removed = true;
                        break;
                    case "dispose":
                        obj.Dispose = null;
                        removed = true;
                        break;
                    case "extension":
                        obj.Extension = null;
                        removed = true;
                        break;
                    case "filecount":
                        obj.FileCount = null;
                        removed = true;
                        break;
                    case "fileisavailable":
                        obj.FileIsAvailable = null;
                        removed = true;
                        break;
                    case "flags":
                        obj.Flags = null;
                        removed = true;
                        break;
                    case "format":
                        obj.Format = null;
                        removed = true;
                        break;
                    case "header":
                        obj.Header = null;
                        removed = true;
                        break;
                    case "height":
                        obj.Height = null;
                        removed = true;
                        break;
                    case "hocr_char_to_word_hocr_version":
                    case "hocrchartowordhocrversion":
                        obj.hOCRCharToWordhOCRVersion = null;
                        removed = true;
                        break;
                    case "hocr_char_to_word_module_version":
                    case "hocrchartowordmoduleversion":
                        obj.hOCRCharToWordModuleVersion = null;
                        removed = true;
                        break;
                    case "hocr_fts_text_hocr_version":
                    case "hocrftstexthocrversion":
                        obj.hOCRFtsTexthOCRVersion = null;
                        removed = true;
                        break;
                    case "hocr_fts_text_module_version":
                    case "hocrftstextmoduleversion":
                        obj.hOCRFtsTextModuleVersion = null;
                        removed = true;
                        break;
                    case "hocr_pageindex_hocr_version":
                    case "hocrpageindexhocrversion":
                        obj.hOCRPageIndexhOCRVersion = null;
                        removed = true;
                        break;
                    case "hocr_pageindex_module_version":
                    case "hocrpageindexmoduleversion":
                        obj.hOCRPageIndexModuleVersion = null;
                        removed = true;
                        break;
                    case "inverted":
                        obj.Inverted = null;
                        removed = true;
                        break;
                    case "mtime":
                    case "lastmodifiedtime":
                        obj.LastModifiedTime = null;
                        removed = true;
                        break;
                    case "length":
                        obj.Length = null;
                        removed = true;
                        break;
                    case "loadflag":
                        obj.LoadFlag = null;
                        removed = true;
                        break;
                    case "matrix_number":
                    case "matrixnumber":
                        obj.MatrixNumber = null;
                        removed = true;
                        break;
                    case "md2":
                        obj.MD2 = null;
                        removed = true;
                        break;
                    case "md4":
                        obj.MD4 = null;
                        removed = true;
                        break;
                    case "md5":
                        obj.MD5 = null;
                        removed = true;
                        break;
                    case "mediatype":
                    case "openmsxmediatype":
                        obj.OpenMSXMediaType = null;
                        removed = true;
                        break;
                    case "merge":
                        obj.Merge = null;
                        removed = true;
                        break;
                    case "mia":
                        obj.MIA = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "offset":
                        obj.Offset = null;
                        removed = true;
                        break;
                    case "openmsxtype":
                        obj.OpenMSXType = null;
                        removed = true;
                        break;
                    case "optional":
                        obj.Optional = null;
                        removed = true;
                        break;
                    case "original":
                        obj.Original = null;
                        removed = true;
                        break;
                    case "pdf_module_version":
                    case "pdfmoduleversion":
                        obj.PDFModuleVersion = null;
                        removed = true;
                        break;
                    case "preview-image":
                    case "previewimage":
                        obj.PreviewImage = null;
                        removed = true;
                        break;
                    case "publisher":
                        obj.Publisher = null;
                        removed = true;
                        break;
                    case "region":
                        obj.Region = null;
                        removed = true;
                        break;
                    case "remark":
                        obj.Remark = null;
                        removed = true;
                        break;
                    case "ripemd128":
                        obj.RIPEMD128 = null;
                        removed = true;
                        break;
                    case "ripemd160":
                        obj.RIPEMD160 = null;
                        removed = true;
                        break;
                    case "rotation":
                        obj.Rotation = null;
                        removed = true;
                        break;
                    case "serial":
                        obj.Serial = null;
                        removed = true;
                        break;
                    case "sha1":
                        obj.SHA1 = null;
                        removed = true;
                        break;
                    case "sha256":
                        obj.SHA256 = null;
                        removed = true;
                        break;
                    case "sha384":
                        obj.SHA384 = null;
                        removed = true;
                        break;
                    case "sha512":
                        obj.SHA512 = null;
                        removed = true;
                        break;
                    case "size":
                        obj.Size = null;
                        removed = true;
                        break;
                    case "soundonly":
                        obj.SoundOnly = null;
                        removed = true;
                        break;
                    case "source":
                        obj.Source = null;
                        removed = true;
                        break;
                    case "spamsum":
                        obj.SpamSum = null;
                        removed = true;
                        break;
                    case "start":
                        obj.Start = null;
                        removed = true;
                        break;
                    case "status":
                        obj.Status = null;
                        removed = true;
                        break;
                    case "summation":
                        obj.Summation = null;
                        removed = true;
                        break;
                    case "ocr":
                    case "tesseractocr":
                        obj.TesseractOCR = null;
                        removed = true;
                        break;
                    case "ocr_converted":
                    case "tesseractocrconverted":
                        obj.TesseractOCRConverted = null;
                        removed = true;
                        break;
                    case "ocr_detected_lang":
                    case "tesseractocrdetectedlang":
                        obj.TesseractOCRDetectedLang = null;
                        removed = true;
                        break;
                    case "ocr_detected_lang_conf":
                    case "tesseractocrdetectedlangconf":
                        obj.TesseractOCRDetectedLangConf = null;
                        removed = true;
                        break;
                    case "ocr_detected_script":
                    case "tesseractocrdetectedscript":
                        obj.TesseractOCRDetectedScript = null;
                        removed = true;
                        break;
                    case "ocr_detected_script_conf":
                    case "tesseractocrdetectedscriptconf":
                        obj.TesseractOCRDetectedScriptConf = null;
                        removed = true;
                        break;
                    case "ocr_module_version":
                    case "tesseractocrmoduleversion":
                        obj.TesseractOCRModuleVersion = null;
                        removed = true;
                        break;
                    case "ocr_parameters":
                    case "tesseractocrparameters":
                        obj.TesseractOCRParameters = null;
                        removed = true;
                        break;
                    case "title":
                        obj.Title = null;
                        removed = true;
                        break;
                    case "track":
                        obj.Track = null;
                        removed = true;
                        break;
                    case "value":
                        obj.Value = null;
                        removed = true;
                        break;
                    case "whisper_asr_module_version":
                    case "whisperasrmoduleversion":
                        obj.WhisperASRModuleVersion = null;
                        removed = true;
                        break;
                    case "whisper_model_hash":
                    case "whispermodelhash":
                        obj.WhisperModelHash = null;
                        removed = true;
                        break;
                    case "whisper_model_name":
                    case "whispermodelname":
                        obj.WhisperModelName = null;
                        removed = true;
                        break;
                    case "whisper_version":
                    case "whisperversion":
                        obj.WhisperVersion = null;
                        removed = true;
                        break;
                    case "width":
                        obj.Width = null;
                        removed = true;
                        break;
                    case "word_conf_0_10":
                    case "wordconfidenceinterval0to10":
                        obj.WordConfidenceInterval0To10 = null;
                        removed = true;
                        break;
                    case "word_conf_11_20":
                    case "wordconfidenceinterval11to20":
                        obj.WordConfidenceInterval11To20 = null;
                        removed = true;
                        break;
                    case "word_conf_21_30":
                    case "wordconfidenceinterval21to30":
                        obj.WordConfidenceInterval21To30 = null;
                        removed = true;
                        break;
                    case "word_conf_31_40":
                    case "wordconfidenceinterval31to40":
                        obj.WordConfidenceInterval31To40 = null;
                        removed = true;
                        break;
                    case "word_conf_41_50":
                    case "wordconfidenceinterval41to50":
                        obj.WordConfidenceInterval41To50 = null;
                        removed = true;
                        break;
                    case "word_conf_51_60":
                    case "wordconfidenceinterval51to60":
                        obj.WordConfidenceInterval51To60 = null;
                        removed = true;
                        break;
                    case "word_conf_61_70":
                    case "wordconfidenceinterval61to70":
                        obj.WordConfidenceInterval61To70 = null;
                        removed = true;
                        break;
                    case "word_conf_71_80":
                    case "wordconfidenceinterval71to80":
                        obj.WordConfidenceInterval71To80 = null;
                        removed = true;
                        break;
                    case "word_conf_81_90":
                    case "wordconfidenceinterval81to90":
                        obj.WordConfidenceInterval81To90 = null;
                        removed = true;
                        break;
                    case "word_conf_91_100":
                    case "wordconfidenceinterval91to100":
                        obj.WordConfidenceInterval91To100 = null;
                        removed = true;
                        break;
                    case "xxhash364":
                        obj.xxHash364 = null;
                        removed = true;
                        break;
                    case "xxhash3128":
                        obj.xxHash3128 = null;
                        removed = true;
                        break;

                    // DataArea
                    case "dataarea.endianness":
                        obj.DataAreaEndianness = null;
                        removed = true;
                        break;
                    case "dataarea.name":
                        obj.DataAreaName = null;
                        removed = true;
                        break;
                    case "dataarea.size":
                        obj.DataAreaSize = null;
                        removed = true;
                        break;
                    case "dataarea.width":
                        obj.DataAreaWidth = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Sample? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Serials? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "boxbarcode":
                        obj.BoxBarcode = null;
                        removed = true;
                        break;
                    case "boxserial":
                        obj.BoxSerial = null;
                        removed = true;
                        break;
                    case "chipserial":
                        obj.ChipSerial = null;
                        removed = true;
                        break;
                    case "digitalserial1":
                        obj.DigitalSerial1 = null;
                        removed = true;
                        break;
                    case "digitalserial2":
                        obj.DigitalSerial2 = null;
                        removed = true;
                        break;
                    case "lockoutserial":
                        obj.LockoutSerial = null;
                        removed = true;
                        break;
                    case "mediaserial1":
                        obj.MediaSerial1 = null;
                        removed = true;
                        break;
                    case "mediaserial2":
                        obj.MediaSerial2 = null;
                        removed = true;
                        break;
                    case "mediaserial3":
                        obj.MediaSerial3 = null;
                        removed = true;
                        break;
                    case "mediastamp":
                        obj.MediaStamp = null;
                        removed = true;
                        break;
                    case "pcbserial":
                        obj.PCBSerial = null;
                        removed = true;
                        break;
                    case "romchipserial1":
                        obj.RomChipSerial1 = null;
                        removed = true;
                        break;
                    case "romchipserial2":
                        obj.RomChipSerial2 = null;
                        removed = true;
                        break;
                    case "savechipserial":
                        obj.SaveChipSerial = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(SharedFeat? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "value":
                        obj.Value = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Slot? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }

            if (obj.SlotOption is not null)
            {
                foreach (var slotOption in obj.SlotOption)
                {
                    RemoveFields(slotOption);
                }
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(SlotOption? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "default":
                        obj.Default = null;
                        removed = true;
                        break;
                    case "devname":
                        obj.DevName = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(SoftwareList? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "filter":
                        obj.Filter = null;
                        removed = true;
                        break;
                    case "name":
                        obj.Name = null;
                        removed = true;
                        break;
                    case "status":
                        obj.Status = null;
                        removed = true;
                        break;
                    case "tag":
                        obj.Tag = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(Sound? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "channels":
                        obj.Channels = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        private void RemoveFields(SourceDetails? obj, HashSet<string> fieldNames)
        {
            // If we have an invalid input, return
            if (obj is null || fieldNames.Count == 0)
                return;

            foreach (var fieldName in fieldNames)
            {
                bool removed;
                switch (fieldName)
                {
                    case "appendtonumber":
                        obj.AppendToNumber = null;
                        removed = true;
                        break;
                    case "comment1":
                        obj.Comment1 = null;
                        removed = true;
                        break;
                    case "comment2":
                        obj.Comment2 = null;
                        removed = true;
                        break;
                    case "dumpdate":
                        obj.DumpDate = null;
                        removed = true;
                        break;
                    case "dumpdateinfo":
                        obj.DumpDateInfo = null;
                        removed = true;
                        break;
                    case "dumper":
                        obj.Dumper = null;
                        removed = true;
                        break;
                    case "id":
                        obj.Id = null;
                        removed = true;
                        break;
                    case "link1":
                        obj.Link1 = null;
                        removed = true;
                        break;
                    case "link1public":
                        obj.Link1Public = null;
                        removed = true;
                        break;
                    case "link2":
                        obj.Link2 = null;
                        removed = true;
                        break;
                    case "link2public":
                        obj.Link2Public = null;
                        removed = true;
                        break;
                    case "link3":
                        obj.Link3 = null;
                        removed = true;
                        break;
                    case "link3public":
                        obj.Link3Public = null;
                        removed = true;
                        break;
                    case "mediatitle":
                        obj.MediaTitle = null;
                        removed = true;
                        break;
                    case "nodump":
                        obj.Nodump = null;
                        removed = true;
                        break;
                    case "origin":
                        obj.Origin = null;
                        removed = true;
                        break;
                    case "originalformat":
                        obj.OriginalFormat = null;
                        removed = true;
                        break;
                    case "project":
                        obj.Project = null;
                        removed = true;
                        break;
                    case "region":
                        obj.Region = null;
                        removed = true;
                        break;
                    case "releasedate":
                        obj.ReleaseDate = null;
                        removed = true;
                        break;
                    case "releasedateinfo":
                        obj.ReleaseDateInfo = null;
                        removed = true;
                        break;
                    case "rominfo":
                        obj.RomInfo = null;
                        removed = true;
                        break;
                    case "section":
                        obj.Section = null;
                        removed = true;
                        break;
                    case "tool":
                        obj.Tool = null;
                        removed = true;
                        break;

                    default:
                        removed = false;
                        break;
                }

                _logger.Verbose($"Item field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        #endregion
    }
}
