using System.Collections.Generic;
using System.Linq;
using SabreTools.Data.Extensions;
using SabreTools.Logging;
using SabreTools.Metadata.DatFiles;
using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;
using SabreTools.Metadata.Filter;
using SabreTools.Text.Extensions;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Set fields on DatItems
    /// </summary>
    public class Setter
    {
        #region Fields

        /// <summary>
        /// Mappings to set DatHeader fields
        /// </summary>
        public Dictionary<string, string> HeaderFieldMappings { get; } = [];

        /// <summary>
        /// Mappings to set Machine fields
        /// </summary>
        public Dictionary<string, string> MachineFieldMappings { get; } = [];

        /// <summary>
        /// Mappings to set DatItem fields
        /// </summary>
        public Dictionary<FilterKey, string> ItemFieldMappings { get; } = [];

        #endregion

        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private readonly Logger _logger = new();

        #endregion

        #region Population

        /// <summary>
        /// Populate the setters using a field name and a value
        /// </summary>
        /// <param name="field">Field name</param>
        /// <param name="value">Field value</param>
        public void PopulateSetters(FilterKey field, string value)
            => PopulateSetters([field], [value]);

        /// <summary>
        /// Populate the setters using a set of field names
        /// </summary>
        /// <param name="fields">List of field names</param>
        /// <param name="values">List of field values</param>
        public void PopulateSetters(List<FilterKey> fields, List<string> values)
        {
            // If the list is null or empty, just return
            if (values is null || values.Count == 0)
                return;

            var watch = new InternalStopwatch("Populating setters from list");

            // Now we loop through and get values for everything
            for (int i = 0; i < fields.Count; i++)
            {
                FilterKey field = fields[i];
                string value = values[i];

                if (!SetSetter(field, value))
                    _logger.Warning($"The value {value} did not match any known field names. Please check the wiki for more details on supported field names.");
            }

            watch.Stop();
        }

        /// <summary>
        /// Populate the setters using a set of field names
        /// </summary>
        /// <param name="mappings">Dictionary of mappings</param>
        public void PopulateSetters(Dictionary<FilterKey, string>? mappings)
        {
            // If the dictionary is null or empty, just return
            if (mappings is null || mappings.Count == 0)
                return;

            var watch = new InternalStopwatch("Populating setters from dictionary");

            // Now we loop through and get values for everything
            foreach (var mapping in mappings)
            {
                FilterKey field = mapping.Key;
                string value = mapping.Value;

                if (!SetSetter(field, value))
                    _logger.Warning($"The value {value} did not match any known field names. Please check the wiki for more details on supported field names.");
            }

            watch.Stop();
        }

        /// <summary>
        /// Set setter from a value
        /// </summary>
        /// <param name="key">Key for the setter to be set</param>
        private bool SetSetter(FilterKey key, string value)
        {
            switch (key.ItemName)
            {
                case "header":
                    HeaderFieldMappings[key.FieldName] = value;
                    return true;

                case "machine":
                    MachineFieldMappings[key.FieldName] = value;
                    return true;

                default:
                    // Replace some specific prefixes
                    if (key.ItemName == "dataarea")
                        key = new FilterKey("rom", $"dataarea.{key.FieldName}");
                    else if (key.ItemName == "diskarea")
                        key = new FilterKey("disk", $"diskarea.{key.FieldName}");
                    // TODO: Handle Part since that splits into 4 item names
                    else if (key.ItemName == "video")
                        key = new FilterKey("display", key.FieldName);

                    ItemFieldMappings[key] = value;
                    return true;
            }
        }

        #endregion

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="datItem">DatItem to set fields on</param>
        public void SetFields(DatItem datItem)
        {
            // If we have an invalid input, return
            if (datItem is null)
                return;

            #region Common

            // Handle Machine fields
            if (MachineFieldMappings.Count > 0 && datItem.Machine is not null)
                SetFields(datItem.Machine!);

            // If there are no field names, return
            if (ItemFieldMappings is null || ItemFieldMappings.Count == 0)
                return;

            // If there are no field names for this type or generic, return
            string? itemType = datItem.ItemType.AsStringValue();
            if (itemType is null || (!ItemFieldMappings.Keys.Any(kvp => kvp.ItemName == itemType) && !ItemFieldMappings.Keys.Any(kvp => kvp.ItemName == "item")))
                return;

            // Get the combined list of fields to remove
            var fieldMappings = new Dictionary<string, string>();
            foreach (var mapping in ItemFieldMappings.Where(kvp => kvp.Key.ItemName == "item").ToDictionary(kvp => kvp.Key.FieldName, kvp => kvp.Value))
            {
                fieldMappings[mapping.Key] = mapping.Value;
            }

            foreach (var mapping in ItemFieldMappings.Where(kvp => kvp.Key.ItemName == itemType).ToDictionary(kvp => kvp.Key.FieldName, kvp => kvp.Value))
            {
                fieldMappings[mapping.Key] = mapping.Value;
            }

            // If the field specifically contains Name, set it separately
            if (fieldMappings.TryGetValue("name", out string? name))
            {
                datItem.SetName(name);
                fieldMappings.Remove("name");
            }

            #endregion

            #region Item-Specific

            // Handle all item types
            switch (datItem)
            {
                case Adjuster obj: SetFields(obj, fieldMappings); break;
                case Archive obj: SetFields(obj, fieldMappings); break;
                case BiosSet obj: SetFields(obj, fieldMappings); break;
                case Chip obj: SetFields(obj, fieldMappings); break;
                case Configuration obj: SetFields(obj, fieldMappings); break;
                case ConfLocation obj: SetFields(obj, fieldMappings); break;
                case ConfSetting obj: SetFields(obj, fieldMappings); break;
                case Control obj: SetFields(obj, fieldMappings); break;
                case Device obj: SetFields(obj, fieldMappings); break;
                case DeviceRef obj: SetFields(obj, fieldMappings); break;
                case DipLocation obj: SetFields(obj, fieldMappings); break;
                case DipSwitch obj: SetFields(obj, fieldMappings); break;
                case DipValue obj: SetFields(obj, fieldMappings); break;
                case Disk obj: SetFields(obj, fieldMappings); break;
                case Display obj: SetFields(obj, fieldMappings); break;
                case Driver obj: SetFields(obj, fieldMappings); break;
                case Feature obj: SetFields(obj, fieldMappings); break;
                case Info obj: SetFields(obj, fieldMappings); break;
                case Input obj: SetFields(obj, fieldMappings); break;
                case Media obj: SetFields(obj, fieldMappings); break;
                // case Original obj: SetFields(obj, fieldMappings); break;
                // TODO: Part needs to be wrapped into DipSwitch, Disk, PartFeature, and Rom
                case PartFeature obj: SetFields(obj, fieldMappings); break;
                case Port obj: SetFields(obj, fieldMappings); break;
                case RamOption obj: SetFields(obj, fieldMappings); break;
                case Release obj: SetFields(obj, fieldMappings); break;
                case ReleaseDetails obj: SetFields(obj, fieldMappings); break;
                case Rom obj: SetFields(obj, fieldMappings); break;
                case Sample obj: SetFields(obj, fieldMappings); break;
                case Serials obj: SetFields(obj, fieldMappings); break;
                case SharedFeat obj: SetFields(obj, fieldMappings); break;
                case Slot obj: SetFields(obj, fieldMappings); break;
                case SlotOption obj: SetFields(obj, fieldMappings); break;
                case SoftwareList obj: SetFields(obj, fieldMappings); break;
                case Sound obj: SetFields(obj, fieldMappings); break;
                case SourceDetails obj: SetFields(obj, fieldMappings); break;

                // Ignore types not handled by this class
                default: break;
            }

            #endregion
        }

        #region Per-Type Setting

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        public void SetFields(DatHeader? obj)
        {
            // If we have an invalid input, return
            if (obj is null || HeaderFieldMappings.Count == 0)
                return;

            foreach (var kvp in HeaderFieldMappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "author":
                        obj.Author = kvp.Value;
                        set = true;
                        break;
                    case "biosmode":
                        obj.BiosMode = kvp.Value.AsMergingFlag();
                        set = true;
                        break;
                    case "build":
                        obj.Build = kvp.Value;
                        set = true;
                        break;
                    // Header.CanOpen is intentionally skipped
                    case "category":
                        obj.Category = kvp.Value;
                        set = true;
                        break;
                    case "comment":
                        obj.Comment = kvp.Value;
                        set = true;
                        break;
                    case "date":
                        obj.Date = kvp.Value;
                        set = true;
                        break;
                    case "datversion":
                        obj.DatVersion = kvp.Value;
                        set = true;
                        break;
                    case "debug":
                        obj.Debug = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "description":
                        obj.Description = kvp.Value;
                        set = true;
                        break;
                    case "email":
                        obj.Email = kvp.Value;
                        set = true;
                        break;
                    case "emulatorversion":
                        obj.EmulatorVersion = kvp.Value;
                        set = true;
                        break;
                    case "filename":
                        obj.FileName = kvp.Value;
                        set = true;
                        break;
                    case "forcemerging":
                        obj.ForceMerging = kvp.Value.AsMergingFlag();
                        set = true;
                        break;
                    case "forcenodump":
                        obj.ForceNodump = kvp.Value.AsNodumpFlag();
                        set = true;
                        break;
                    case "forcepacking":
                        obj.ForcePacking = kvp.Value.AsPackingFlag();
                        set = true;
                        break;
                    case "forcezipping":
                        obj.ForceZipping = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    // Header.HeaderRow is intentionally skipped
                    case "header":
                    case "headerskipper":
                    case "skipper":
                        obj.HeaderSkipper = kvp.Value;
                        set = true;
                        break;
                    case "homepage":
                        obj.Homepage = kvp.Value;
                        set = true;
                        break;
                    case "id":
                        obj.Id = kvp.Value;
                        set = true;
                        break;
                    // Header.Images is intentionally skipped
                    case "imfolder":
                        obj.ImFolder = kvp.Value;
                        set = true;
                        break;
                    // Header.Infos is intentionally skipped
                    case "lockbiosmode":
                        obj.LockBiosMode = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "lockrommode":
                        obj.LockRomMode = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "locksamplemode":
                        obj.LockSampleMode = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "mameconfig":
                        obj.MameConfig = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    // Header.NewDat is intentionally skipped
                    case "notes":
                        obj.Notes = kvp.Value;
                        set = true;
                        break;
                    case "plugin":
                        obj.Plugin = kvp.Value;
                        set = true;
                        break;
                    case "refname":
                        obj.RefName = kvp.Value;
                        set = true;
                        break;
                    case "rommode":
                        obj.RomMode = kvp.Value.AsMergingFlag();
                        set = true;
                        break;
                    case "romtitle":
                        obj.RomTitle = kvp.Value;
                        set = true;
                        break;
                    case "rootdir":
                        obj.RootDir = kvp.Value;
                        set = true;
                        break;
                    case "samplemode":
                        obj.SampleMode = kvp.Value.AsMergingFlag();
                        set = true;
                        break;
                    case "schemalocation":
                        obj.SchemaLocation = kvp.Value;
                        set = true;
                        break;
                    case "screenshotsheight":
                        obj.ScreenshotsHeight = kvp.Value;
                        set = true;
                        break;
                    case "screenshotswidth":
                        obj.ScreenshotsWidth = kvp.Value;
                        set = true;
                        break;
                    // Header.Search is intentionally skipped
                    case "system":
                        obj.System = kvp.Value;
                        set = true;
                        break;
                    case "timestamp":
                        obj.Timestamp = kvp.Value;
                        set = true;
                        break;
                    case "type":
                        obj.Type = kvp.Value;
                        set = true;
                        break;
                    case "url":
                        obj.Url = kvp.Value;
                        set = true;
                        break;
                    case "version":
                        obj.Version = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Header field {kvp} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        public void SetFields(Machine? obj)
        {
            // If we have an invalid input, return
            if (obj is null || MachineFieldMappings.Count == 0)
                return;

            foreach (var kvp in MachineFieldMappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "board":
                        obj.Board = kvp.Value;
                        set = true;
                        break;
                    case "buttons":
                        obj.Buttons = kvp.Value;
                        set = true;
                        break;
                    case "category":
                        obj.Category = [kvp.Value];
                        set = true;
                        break;
                    case "cloneof":
                        obj.CloneOf = kvp.Value;
                        set = true;
                        break;
                    case "cloneofid":
                        obj.CloneOfId = kvp.Value;
                        set = true;
                        break;
                    case "comment":
                        obj.Comment = [kvp.Value];
                        set = true;
                        break;
                    case "company":
                        obj.Company = kvp.Value;
                        set = true;
                        break;
                    case "control":
                        obj.Control = kvp.Value;
                        set = true;
                        break;
                    case "crc":
                        obj.CRC = kvp.Value;
                        set = true;
                        break;
                    case "country":
                        obj.Country = kvp.Value;
                        set = true;
                        break;
                    case "description":
                        obj.Description = kvp.Value;
                        set = true;
                        break;
                    case "developer":
                        obj.Developer = kvp.Value;
                        set = true;
                        break;
                    case "dirname":
                        obj.DirName = kvp.Value;
                        set = true;
                        break;
                    case "displaycount":
                        obj.DisplayCount = kvp.Value;
                        set = true;
                        break;
                    case "displaytype":
                        obj.DisplayType = kvp.Value;
                        set = true;
                        break;
                    case "duplicateid":
                        obj.DuplicateID = kvp.Value;
                        set = true;
                        break;
                    case "emulator":
                        obj.Emulator = kvp.Value;
                        set = true;
                        break;
                    case "enabled":
                        obj.Enabled = kvp.Value;
                        set = true;
                        break;
                    case "extra":
                        obj.Extra = kvp.Value;
                        set = true;
                        break;
                    case "favorite":
                        obj.Favorite = kvp.Value;
                        set = true;
                        break;
                    case "genmsxid":
                        obj.GenMSXID = kvp.Value;
                        set = true;
                        break;
                    case "genre":
                        obj.Genre = kvp.Value;
                        set = true;
                        break;
                    case "hash":
                        obj.Hash = kvp.Value;
                        set = true;
                        break;
                    case "history":
                        obj.History = kvp.Value;
                        set = true;
                        break;
                    case "id":
                        obj.Id = kvp.Value;
                        set = true;
                        break;
                    case "im1crc":
                        obj.Im1CRC = kvp.Value;
                        set = true;
                        break;
                    case "im2crc":
                        obj.Im2CRC = kvp.Value;
                        set = true;
                        break;
                    case "imagenumber":
                        obj.ImageNumber = kvp.Value;
                        set = true;
                        break;
                    case "isbios":
                        obj.IsBios = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "isdevice":
                        obj.IsDevice = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "ismechanical":
                        obj.IsMechanical = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "language":
                        obj.Language = kvp.Value;
                        set = true;
                        break;
                    case "location":
                        obj.Location = kvp.Value;
                        set = true;
                        break;
                    case "manufacturer":
                        obj.Manufacturer = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "notes":
                        obj.Notes = kvp.Value;
                        set = true;
                        break;
                    case "playedcount":
                        obj.PlayedCount = kvp.Value;
                        set = true;
                        break;
                    case "playedtime":
                        obj.PlayedTime = kvp.Value;
                        set = true;
                        break;
                    case "players":
                        obj.Players = kvp.Value;
                        set = true;
                        break;
                    case "publisher":
                        obj.Publisher = kvp.Value;
                        set = true;
                        break;
                    case "ratings":
                        obj.Ratings = kvp.Value;
                        set = true;
                        break;
                    case "rebuildto":
                        obj.RebuildTo = kvp.Value;
                        set = true;
                        break;
                    case "relatedto":
                        obj.RelatedTo = kvp.Value;
                        set = true;
                        break;
                    case "releasenumber":
                        obj.ReleaseNumber = kvp.Value;
                        set = true;
                        break;
                    case "romof":
                        obj.RomOf = kvp.Value;
                        set = true;
                        break;
                    case "rotation":
                        obj.Rotation = kvp.Value;
                        set = true;
                        break;
                    case "runnable":
                        obj.Runnable = kvp.Value.AsRunnable();
                        set = true;
                        break;
                    case "sampleof":
                        obj.SampleOf = kvp.Value;
                        set = true;
                        break;
                    case "savetype":
                        obj.SaveType = kvp.Value;
                        set = true;
                        break;
                    case "score":
                        obj.Score = kvp.Value;
                        set = true;
                        break;
                    case "source":
                        obj.Source = kvp.Value;
                        set = true;
                        break;
                    case "sourcefile":
                        obj.SourceFile = kvp.Value;
                        set = true;
                        break;
                    case "sourcerom":
                        obj.SourceRom = kvp.Value;
                        set = true;
                        break;
                    case "status":
                        obj.Status = kvp.Value;
                        set = true;
                        break;
                    case "subgenre":
                        obj.Subgenre = kvp.Value;
                        set = true;
                        break;
                    case "supported":
                        obj.Supported = kvp.Value.AsSupported();
                        set = true;
                        break;
                    case "system":
                        obj.System = kvp.Value;
                        set = true;
                        break;
                    case "tags":
                        obj.Tags = kvp.Value;
                        set = true;
                        break;
                    case "titleid":
                        obj.TitleID = kvp.Value;
                        set = true;
                        break;
                    case "url":
                        obj.Url = kvp.Value;
                        set = true;
                        break;
                    case "year":
                        obj.Year = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Machine field {kvp} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Adjuster? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "condition.mask":
                        obj.ConditionMask = kvp.Value;
                        set = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = kvp.Value.AsRelation();
                        set = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = kvp.Value;
                        set = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = kvp.Value;
                        set = true;
                        break;
                    case "default":
                        obj.Default = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Archive? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "additional":
                        obj.Additional = kvp.Value;
                        set = true;
                        break;
                    case "adult":
                        obj.Adult = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "alt":
                        obj.Alt = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "bios":
                        obj.Bios = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "categories":
                        obj.Categories = kvp.Value;
                        set = true;
                        break;
                    case "clone":
                    case "clonetag":
                        obj.CloneTag = kvp.Value;
                        set = true;
                        break;
                    case "complete":
                        obj.Complete = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "dat":
                        obj.Dat = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "datternote":
                        obj.DatterNote = kvp.Value;
                        set = true;
                        break;
                    case "description":
                        obj.Description = kvp.Value;
                        set = true;
                        break;
                    case "devstatus":
                        obj.DevStatus = kvp.Value;
                        set = true;
                        break;
                    case "gameid1":
                        obj.GameId1 = kvp.Value;
                        set = true;
                        break;
                    case "gameid2":
                        obj.GameId2 = kvp.Value;
                        set = true;
                        break;
                    case "langchecked":
                        obj.LangChecked = kvp.Value;
                        set = true;
                        break;
                    case "languages":
                        obj.Languages = kvp.Value;
                        set = true;
                        break;
                    case "licensed":
                        obj.Licensed = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "listed":
                        obj.Listed = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "mergeof":
                        obj.MergeOf = kvp.Value;
                        set = true;
                        break;
                    case "mergename":
                        obj.MergeName = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "namealt":
                        obj.NameAlt = kvp.Value;
                        set = true;
                        break;
                    case "number":
                        obj.Number = kvp.Value;
                        set = true;
                        break;
                    case "physical":
                        obj.Physical = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "pirate":
                        obj.Pirate = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "private":
                        obj.Private = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "region":
                        obj.Region = kvp.Value;
                        set = true;
                        break;
                    case "regparent":
                        obj.RegParent = kvp.Value;
                        set = true;
                        break;
                    case "showlang":
                        obj.ShowLang = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "special1":
                        obj.Special1 = kvp.Value;
                        set = true;
                        break;
                    case "special2":
                        obj.Special2 = kvp.Value;
                        set = true;
                        break;
                    case "stickynote":
                        obj.StickyNote = kvp.Value;
                        set = true;
                        break;
                    case "version1":
                        obj.Version1 = kvp.Value;
                        set = true;
                        break;
                    case "version2":
                        obj.Version2 = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(BiosSet? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "default":
                        obj.Default = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "description":
                        obj.Description = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Chip? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "chiptype":
                        obj.ChipType = kvp.Value.AsChipType();
                        set = true;
                        break;
                    case "clock":
                        obj.Clock = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "flags":
                        obj.Flags = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "soundonly":
                        obj.SoundOnly = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "tag":
                        obj.Tag = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Configuration? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "condition.mask":
                        obj.ConditionMask = kvp.Value;
                        set = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = kvp.Value.AsRelation();
                        set = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = kvp.Value;
                        set = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = kvp.Value;
                        set = true;
                        break;
                    case "mask":
                        obj.Mask = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "tag":
                        obj.Tag = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }

            if (obj.ConfLocation is not null)
            {
                foreach (var confLocation in obj.ConfLocation)
                {
                    SetFields(confLocation);
                }
            }

            if (obj.ConfSetting is not null)
            {
                foreach (var confSetting in obj.ConfSetting)
                {
                    SetFields(confSetting);
                }
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(ConfLocation? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "inverted":
                        obj.Inverted = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "number":
                        obj.Number = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(ConfSetting? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "condition.mask":
                        obj.ConditionMask = kvp.Value;
                        set = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = kvp.Value.AsRelation();
                        set = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = kvp.Value;
                        set = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = kvp.Value;
                        set = true;
                        break;
                    case "default":
                        obj.Default = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "value":
                        obj.Value = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Control? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "buttons":
                        obj.Buttons = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "controltype":
                        obj.ControlType = kvp.Value.AsControlType();
                        set = true;
                        break;
                    case "keydelta":
                        obj.KeyDelta = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "maximum":
                        obj.Maximum = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "minimum":
                        obj.Minimum = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "player":
                        obj.Player = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "reqbuttons":
                        obj.ReqButtons = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "reverse":
                        obj.Reverse = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "sensitivity":
                        obj.Sensitivity = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "ways":
                        obj.Ways = kvp.Value;
                        set = true;
                        break;
                    case "ways2":
                        obj.Ways2 = kvp.Value;
                        set = true;
                        break;
                    case "ways3":
                        obj.Ways3 = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Device? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "devicetype":
                        obj.DeviceType = kvp.Value.AsDeviceType();
                        set = true;
                        break;
                    case "extension.name":
                        obj.ExtensionName = [kvp.Value];
                        set = true;
                        break;
                    case "fixedimage":
                        obj.FixedImage = kvp.Value;
                        set = true;
                        break;
                    case "instance.briefname":
                        obj.InstanceBriefName = kvp.Value;
                        set = true;
                        break;
                    case "instance.name":
                        obj.InstanceName = kvp.Value;
                        set = true;
                        break;
                    case "interface":
                        obj.Interface = kvp.Value;
                        set = true;
                        break;
                    case "mandatory":
                        obj.Mandatory = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "tag":
                        obj.Tag = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(DeviceRef? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(DipLocation? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "inverted":
                        obj.Inverted = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "number":
                        obj.Number = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(DipSwitch? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "condition.mask":
                        obj.ConditionMask = kvp.Value;
                        set = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = kvp.Value.AsRelation();
                        set = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = kvp.Value;
                        set = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = kvp.Value;
                        set = true;
                        break;
                    case "default":
                        obj.Default = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "mask":
                        obj.Mask = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "tag":
                        obj.Tag = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }

            if (obj.DipLocation is not null)
            {
                foreach (var dipLocation in obj.DipLocation)
                {
                    SetFields(dipLocation);
                }
            }

            if (obj.DipValue is not null)
            {
                foreach (var dipValue in obj.DipValue)
                {
                    SetFields(dipValue);
                }
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(DipValue? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "condition.mask":
                        obj.ConditionMask = kvp.Value;
                        set = true;
                        break;
                    case "condition.relation":
                        obj.ConditionRelation = kvp.Value.AsRelation();
                        set = true;
                        break;
                    case "condition.tag":
                        obj.ConditionTag = kvp.Value;
                        set = true;
                        break;
                    case "condition.value":
                        obj.ConditionValue = kvp.Value;
                        set = true;
                        break;
                    case "default":
                        obj.Default = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "value":
                        obj.Value = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Disk? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "flags":
                        obj.Flags = kvp.Value;
                        set = true;
                        break;
                    case "index":
                        obj.Index = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "md5":
                        obj.MD5 = kvp.Value;
                        set = true;
                        break;
                    case "merge":
                        obj.Merge = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "optional":
                        obj.Optional = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "region":
                        obj.Region = kvp.Value;
                        set = true;
                        break;
                    case "sha1":
                        obj.SHA1 = kvp.Value;
                        set = true;
                        break;
                    case "status":
                        obj.Status = kvp.Value.AsItemStatus();
                        set = true;
                        break;
                    case "writable":
                        obj.Writable = kvp.Value.AsYesNo();
                        set = true;
                        break;

                    // DiskArea
                    case "diskarea.name":
                        obj.DiskAreaName = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Display? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "aspectx":
                        obj.AspectX = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "aspecty":
                        obj.AspectY = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "displaytype":
                    case "screen":
                        obj.DisplayType = kvp.Value.AsDisplayType();
                        set = true;
                        break;
                    case "flipx":
                        obj.FlipX = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "hbend":
                        obj.HBEnd = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "hbstart":
                        obj.HBStart = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "height":
                    case "y":
                        obj.Height = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "htotal":
                        obj.HTotal = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "pixclock":
                        obj.PixClock = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "refresh":
                    case "freq":
                        obj.Refresh = NumberHelper.ConvertToDouble(kvp.Value);
                        set = true;
                        break;
                    case "rotate":
                    case "orientation":
                        obj.Rotate = kvp.Value.AsRotation();
                        set = true;
                        break;
                    case "tag":
                        obj.Tag = kvp.Value;
                        set = true;
                        break;
                    case "vbend":
                        obj.VBEnd = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "vbstart":
                        obj.VBStart = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "vtotal":
                        obj.VTotal = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "width":
                    case "x":
                        obj.Width = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Driver? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "blit":
                        obj.Blit = kvp.Value.AsBlit();
                        set = true;
                        break;
                    case "cocktail":
                        obj.Cocktail = kvp.Value.AsSupportStatus();
                        set = true;
                        break;
                    case "color":
                        obj.Color = kvp.Value.AsSupportStatus();
                        set = true;
                        break;
                    case "emulation":
                        obj.Emulation = kvp.Value.AsSupportStatus();
                        set = true;
                        break;
                    case "incomplete":
                        obj.Incomplete = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "nosoundhardware":
                        obj.NoSoundHardware = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "palettesize":
                        obj.PaletteSize = kvp.Value;
                        set = true;
                        break;
                    case "requiresartwork":
                        obj.RequiresArtwork = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "savestate":
                        obj.SaveState = kvp.Value.AsSupported();
                        set = true;
                        break;
                    case "sound":
                        obj.Sound = kvp.Value.AsSupportStatus();
                        set = true;
                        break;
                    case "status":
                        obj.Status = kvp.Value.AsSupportStatus();
                        set = true;
                        break;
                    case "unofficial":
                        obj.Unofficial = kvp.Value.AsYesNo();
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Feature? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "featuretype":
                        obj.FeatureType = kvp.Value.AsFeatureType();
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "overall":
                        obj.Overall = kvp.Value.AsFeatureStatus();
                        set = true;
                        break;
                    case "status":
                        obj.Status = kvp.Value.AsFeatureStatus();
                        set = true;
                        break;
                    case "value":
                        obj.Value = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Info? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "value":
                        obj.Value = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Input? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "buttons":
                        obj.Buttons = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "coins":
                        obj.Coins = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "control":
                    case "controlattr":
                        obj.ControlAttr = kvp.Value;
                        set = true;
                        break;
                    case "players":
                        obj.Players = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "service":
                        obj.Service = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "tilt":
                        obj.Tilt = kvp.Value.AsYesNo();
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }

            if (obj.Control is not null)
            {
                foreach (var control in obj.Control)
                {
                    SetFields(control);
                }
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Media? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "md5":
                        obj.MD5 = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "sha1":
                        obj.SHA1 = kvp.Value;
                        set = true;
                        break;
                    case "sha256":
                        obj.SHA256 = kvp.Value;
                        set = true;
                        break;
                    case "spamsum":
                        obj.SpamSum = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(PartFeature? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "featuretype":
                        obj.FeatureType = kvp.Value.AsFeatureType();
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "overall":
                        obj.Overall = kvp.Value.AsFeatureStatus();
                        set = true;
                        break;
                    case "status":
                        obj.Status = kvp.Value.AsFeatureStatus();
                        set = true;
                        break;
                    case "value":
                        obj.Value = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Port? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "analog.mask":
                        obj.AnalogMask = [kvp.Value];
                        set = true;
                        break;
                    case "tag":
                        obj.Tag = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(RamOption? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "content":
                        obj.Content = kvp.Value;
                        set = true;
                        break;
                    case "default":
                        obj.Default = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Release? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "date":
                        obj.Date = kvp.Value;
                        set = true;
                        break;
                    case "default":
                        obj.Default = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "language":
                        obj.Language = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "region":
                        obj.Region = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(ReleaseDetails? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "appendtonumber":
                        obj.AppendToNumber = kvp.Value;
                        set = true;
                        break;
                    case "archivename":
                        obj.ArchiveName = kvp.Value;
                        set = true;
                        break;
                    case "category":
                        obj.Category = kvp.Value;
                        set = true;
                        break;
                    case "comment":
                        obj.Comment = kvp.Value;
                        set = true;
                        break;
                    case "date":
                        obj.Date = kvp.Value;
                        set = true;
                        break;
                    case "dirname":
                        obj.DirName = kvp.Value;
                        set = true;
                        break;
                    case "group":
                        obj.Group = kvp.Value;
                        set = true;
                        break;
                    case "id":
                        obj.Id = kvp.Value;
                        set = true;
                        break;
                    case "nfocrc":
                        obj.NfoCRC = kvp.Value;
                        set = true;
                        break;
                    case "nfoname":
                        obj.NfoName = kvp.Value;
                        set = true;
                        break;
                    case "nfosize":
                        obj.NfoSize = kvp.Value;
                        set = true;
                        break;
                    case "origin":
                        obj.Origin = kvp.Value;
                        set = true;
                        break;
                    case "originalformat":
                        obj.OriginalFormat = kvp.Value;
                        set = true;
                        break;
                    case "region":
                        obj.Region = kvp.Value;
                        set = true;
                        break;
                    case "rominfo":
                        obj.RomInfo = kvp.Value;
                        set = true;
                        break;
                    case "tool":
                        obj.Tool = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Rom? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "album":
                        obj.Album = kvp.Value;
                        set = true;
                        break;
                    case "alt_romname":
                    case "altromname":
                        obj.AltRomname = kvp.Value;
                        set = true;
                        break;
                    case "alt_title":
                    case "alttitle":
                        obj.AltTitle = kvp.Value;
                        set = true;
                        break;
                    case "artist":
                        obj.Artist = kvp.Value;
                        set = true;
                        break;
                    case "asr_detected_lang":
                    case "asrdetectedlang":
                        obj.ASRDetectedLang = kvp.Value;
                        set = true;
                        break;
                    case "asr_detected_lang_conf":
                    case "asrdetectedlangconf":
                        obj.ASRDetectedLangConf = kvp.Value;
                        set = true;
                        break;
                    case "asr_transcribed_lang":
                    case "asrtranscribedlang":
                        obj.ASRTranscribedLang = kvp.Value;
                        set = true;
                        break;
                    case "bios":
                        obj.Bios = kvp.Value;
                        set = true;
                        break;
                    case "bitrate":
                        obj.Bitrate = kvp.Value;
                        set = true;
                        break;
                    case "btih":
                    case "bittorrentmagnethash":
                        obj.BitTorrentMagnetHash = kvp.Value;
                        set = true;
                        break;
                    case "cloth_cover_detection_module_version":
                    case "clothcoverdetectionmoduleversion":
                        obj.ClothCoverDetectionModuleVersion = kvp.Value;
                        set = true;
                        break;
                    case "collection-catalog-number":
                    case "collectioncatalognumber":
                        obj.CollectionCatalogNumber = kvp.Value;
                        set = true;
                        break;
                    case "comment":
                        obj.Comment = kvp.Value;
                        set = true;
                        break;
                    case "crc16":
                        obj.CRC16 = kvp.Value;
                        set = true;
                        break;
                    case "crc":
                    case "crc32":
                        obj.CRC32 = kvp.Value;
                        set = true;
                        break;
                    case "crc64":
                        obj.CRC64 = kvp.Value;
                        set = true;
                        break;
                    case "creator":
                        obj.Creator = kvp.Value;
                        set = true;
                        break;
                    case "date":
                        obj.Date = kvp.Value;
                        set = true;
                        break;
                    case "dispose":
                        obj.Dispose = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "extension":
                        obj.Extension = kvp.Value;
                        set = true;
                        break;
                    case "filecount":
                        obj.FileCount = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "fileisavailable":
                        obj.FileIsAvailable = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "flags":
                        obj.Flags = kvp.Value;
                        set = true;
                        break;
                    case "format":
                        obj.Format = kvp.Value;
                        set = true;
                        break;
                    case "header":
                        obj.Header = kvp.Value;
                        set = true;
                        break;
                    case "height":
                        obj.Height = kvp.Value;
                        set = true;
                        break;
                    case "hocr_char_to_word_hocr_version":
                    case "hocrchartowordhocrversion":
                        obj.hOCRCharToWordhOCRVersion = kvp.Value;
                        set = true;
                        break;
                    case "hocr_char_to_word_module_version":
                    case "hocrchartowordmoduleversion":
                        obj.hOCRCharToWordModuleVersion = kvp.Value;
                        set = true;
                        break;
                    case "hocr_fts_text_hocr_version":
                    case "hocrftstexthocrversion":
                        obj.hOCRFtsTexthOCRVersion = kvp.Value;
                        set = true;
                        break;
                    case "hocr_fts_text_module_version":
                    case "hocrftstextmoduleversion":
                        obj.hOCRFtsTextModuleVersion = kvp.Value;
                        set = true;
                        break;
                    case "hocr_pageindex_hocr_version":
                    case "hocrpageindexhocrversion":
                        obj.hOCRPageIndexhOCRVersion = kvp.Value;
                        set = true;
                        break;
                    case "hocr_pageindex_module_version":
                    case "hocrpageindexmoduleversion":
                        obj.hOCRPageIndexModuleVersion = kvp.Value;
                        set = true;
                        break;
                    case "inverted":
                        obj.Inverted = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "mtime":
                    case "lastmodifiedtime":
                        obj.LastModifiedTime = kvp.Value;
                        set = true;
                        break;
                    case "length":
                        obj.Length = kvp.Value;
                        set = true;
                        break;
                    case "loadflag":
                        obj.LoadFlag = kvp.Value.AsLoadFlag();
                        set = true;
                        break;
                    case "matrix_number":
                    case "matrixnumber":
                        obj.MatrixNumber = kvp.Value;
                        set = true;
                        break;
                    case "md2":
                        obj.MD2 = kvp.Value;
                        set = true;
                        break;
                    case "md4":
                        obj.MD4 = kvp.Value;
                        set = true;
                        break;
                    case "md5":
                        obj.MD5 = kvp.Value;
                        set = true;
                        break;
                    case "mediatype":
                    case "openmsxmediatype":
                        obj.OpenMSXMediaType = kvp.Value.AsOpenMSXSubType();
                        set = true;
                        break;
                    case "merge":
                        obj.Merge = kvp.Value;
                        set = true;
                        break;
                    case "mia":
                        obj.MIA = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "offset":
                        obj.Offset = kvp.Value;
                        set = true;
                        break;
                    case "openmsxtype":
                        obj.OpenMSXType = kvp.Value;
                        set = true;
                        break;
                    case "optional":
                        obj.Optional = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    // Original is skipped here intentionally
                    case "pdf_module_version":
                    case "pdfmoduleversion":
                        obj.PDFModuleVersion = kvp.Value;
                        set = true;
                        break;
                    case "preview-image":
                    case "previewimage":
                        obj.PreviewImage = kvp.Value;
                        set = true;
                        break;
                    case "publisher":
                        obj.Publisher = kvp.Value;
                        set = true;
                        break;
                    case "region":
                        obj.Region = kvp.Value;
                        set = true;
                        break;
                    case "remark":
                        obj.Remark = kvp.Value;
                        set = true;
                        break;
                    case "ripemd128":
                        obj.RIPEMD128 = kvp.Value;
                        set = true;
                        break;
                    case "ripemd160":
                        obj.RIPEMD160 = kvp.Value;
                        set = true;
                        break;
                    case "rotation":
                        obj.Rotation = kvp.Value;
                        set = true;
                        break;
                    case "serial":
                        obj.Serial = kvp.Value;
                        set = true;
                        break;
                    case "sha1":
                        obj.SHA1 = kvp.Value;
                        set = true;
                        break;
                    case "sha256":
                        obj.SHA256 = kvp.Value;
                        set = true;
                        break;
                    case "sha384":
                        obj.SHA384 = kvp.Value;
                        set = true;
                        break;
                    case "sha512":
                        obj.SHA512 = kvp.Value;
                        set = true;
                        break;
                    case "size":
                        obj.Size = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "soundonly":
                        obj.SoundOnly = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "spamsum":
                        obj.SpamSum = kvp.Value;
                        set = true;
                        break;
                    case "start":
                        obj.Start = kvp.Value;
                        set = true;
                        break;
                    case "status":
                        obj.Status = kvp.Value.AsItemStatus();
                        set = true;
                        break;
                    case "summation":
                        obj.Summation = kvp.Value;
                        set = true;
                        break;
                    case "ocr":
                    case "tesseractocr":
                        obj.TesseractOCR = kvp.Value;
                        set = true;
                        break;
                    case "ocr_converted":
                    case "tesseractocrconverted":
                        obj.TesseractOCRConverted = kvp.Value;
                        set = true;
                        break;
                    case "ocr_detected_lang":
                    case "tesseractocrdetectedlang":
                        obj.TesseractOCRDetectedLang = kvp.Value;
                        set = true;
                        break;
                    case "ocr_detected_lang_conf":
                    case "tesseractocrdetectedlangconf":
                        obj.TesseractOCRDetectedLangConf = kvp.Value;
                        set = true;
                        break;
                    case "ocr_detected_script":
                    case "tesseractocrdetectedscript":
                        obj.TesseractOCRDetectedScript = kvp.Value;
                        set = true;
                        break;
                    case "ocr_detected_script_conf":
                    case "tesseractocrdetectedscriptconf":
                        obj.TesseractOCRDetectedScriptConf = kvp.Value;
                        set = true;
                        break;
                    case "ocr_module_version":
                    case "tesseractocrmoduleversion":
                        obj.TesseractOCRModuleVersion = kvp.Value;
                        set = true;
                        break;
                    case "ocr_parameters":
                    case "tesseractocrparameters":
                        obj.TesseractOCRParameters = kvp.Value;
                        set = true;
                        break;
                    case "title":
                        obj.Title = kvp.Value;
                        set = true;
                        break;
                    case "track":
                        obj.Track = kvp.Value;
                        set = true;
                        break;
                    case "value":
                        obj.Value = kvp.Value;
                        set = true;
                        break;
                    case "whisper_asr_module_version":
                    case "whisperasrmoduleversion":
                        obj.WhisperASRModuleVersion = kvp.Value;
                        set = true;
                        break;
                    case "whisper_model_hash":
                    case "whispermodelhash":
                        obj.WhisperModelHash = kvp.Value;
                        set = true;
                        break;
                    case "whisper_model_name":
                    case "whispermodelname":
                        obj.WhisperModelName = kvp.Value;
                        set = true;
                        break;
                    case "whisper_version":
                    case "whisperversion":
                        obj.WhisperVersion = kvp.Value;
                        set = true;
                        break;
                    case "width":
                        obj.Width = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_0_10":
                    case "wordconfidenceinterval0to10":
                        obj.WordConfidenceInterval0To10 = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_11_20":
                    case "wordconfidenceinterval11to20":
                        obj.WordConfidenceInterval11To20 = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_21_30":
                    case "wordconfidenceinterval21to30":
                        obj.WordConfidenceInterval21To30 = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_31_40":
                    case "wordconfidenceinterval31to40":
                        obj.WordConfidenceInterval31To40 = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_41_50":
                    case "wordconfidenceinterval41to50":
                        obj.WordConfidenceInterval41To50 = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_51_60":
                    case "wordconfidenceinterval51to60":
                        obj.WordConfidenceInterval51To60 = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_61_70":
                    case "wordconfidenceinterval61to70":
                        obj.WordConfidenceInterval61To70 = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_71_80":
                    case "wordconfidenceinterval71to80":
                        obj.WordConfidenceInterval71To80 = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_81_90":
                    case "wordconfidenceinterval81to90":
                        obj.WordConfidenceInterval81To90 = kvp.Value;
                        set = true;
                        break;
                    case "word_conf_91_100":
                    case "wordconfidenceinterval91to100":
                        obj.WordConfidenceInterval91To100 = kvp.Value;
                        set = true;
                        break;
                    case "xxhash364":
                        obj.xxHash364 = kvp.Value;
                        set = true;
                        break;
                    case "xxhash3128":
                        obj.xxHash3128 = kvp.Value;
                        set = true;
                        break;

                    // DataArea
                    case "dataarea.endianness":
                        obj.DataAreaEndianness = kvp.Value.AsEndianness();
                        set = true;
                        break;
                    case "dataarea.name":
                        obj.DataAreaName = kvp.Value;
                        set = true;
                        break;
                    case "dataarea.size":
                        obj.DataAreaSize = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;
                    case "dataarea.width":
                        obj.DataAreaWidth = kvp.Value.AsWidth();
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Sample? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Serials? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "boxbarcode":
                        obj.BoxBarcode = kvp.Value;
                        set = true;
                        break;
                    case "boxserial":
                        obj.BoxSerial = kvp.Value;
                        set = true;
                        break;
                    case "chipserial":
                        obj.ChipSerial = kvp.Value;
                        set = true;
                        break;
                    case "digitalserial1":
                        obj.DigitalSerial1 = kvp.Value;
                        set = true;
                        break;
                    case "digitalserial2":
                        obj.DigitalSerial2 = kvp.Value;
                        set = true;
                        break;
                    case "lockoutserial":
                        obj.LockoutSerial = kvp.Value;
                        set = true;
                        break;
                    case "mediaserial1":
                        obj.MediaSerial1 = kvp.Value;
                        set = true;
                        break;
                    case "mediaserial2":
                        obj.MediaSerial2 = kvp.Value;
                        set = true;
                        break;
                    case "mediaserial3":
                        obj.MediaSerial3 = kvp.Value;
                        set = true;
                        break;
                    case "mediastamp":
                        obj.MediaStamp = kvp.Value;
                        set = true;
                        break;
                    case "pcbserial":
                        obj.PCBSerial = kvp.Value;
                        set = true;
                        break;
                    case "romchipserial1":
                        obj.RomChipSerial1 = kvp.Value;
                        set = true;
                        break;
                    case "romchipserial2":
                        obj.RomChipSerial2 = kvp.Value;
                        set = true;
                        break;
                    case "savechipserial":
                        obj.SaveChipSerial = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(SharedFeat? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "value":
                        obj.Value = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Slot? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }

            if (obj.SlotOption is not null)
            {
                foreach (var slotOption in obj.SlotOption)
                {
                    SetFields(slotOption);
                }
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(SlotOption? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "default":
                        obj.Default = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "devname":
                        obj.DevName = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(SoftwareList? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "filter":
                        obj.Filter = kvp.Value;
                        set = true;
                        break;
                    case "name":
                        obj.Name = kvp.Value;
                        set = true;
                        break;
                    case "status":
                        obj.Status = kvp.Value.AsSoftwareListStatus();
                        set = true;
                        break;
                    case "tag":
                        obj.Tag = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(Sound? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "channels":
                        obj.Channels = NumberHelper.ConvertToInt64(kvp.Value);
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        /// <summary>
        /// Set fields to mapped values
        /// </summary>
        private void SetFields(SourceDetails? obj, Dictionary<string, string> mappings)
        {
            // If we have an invalid input, return
            if (obj is null || mappings.Count == 0)
                return;

            foreach (var kvp in mappings)
            {
                bool set;
                switch (kvp.Key)
                {
                    case "appendtonumber":
                        obj.AppendToNumber = kvp.Value;
                        set = true;
                        break;
                    case "comment1":
                        obj.Comment1 = kvp.Value;
                        set = true;
                        break;
                    case "comment2":
                        obj.Comment2 = kvp.Value;
                        set = true;
                        break;
                    case "dumpdate":
                        obj.DumpDate = kvp.Value;
                        set = true;
                        break;
                    case "dumpdateinfo":
                        obj.DumpDateInfo = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "dumper":
                        obj.Dumper = kvp.Value;
                        set = true;
                        break;
                    case "id":
                        obj.Id = kvp.Value;
                        set = true;
                        break;
                    case "link1":
                        obj.Link1 = kvp.Value;
                        set = true;
                        break;
                    case "link1public":
                        obj.Link1Public = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "link2":
                        obj.Link2 = kvp.Value;
                        set = true;
                        break;
                    case "link2public":
                        obj.Link2Public = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "link3":
                        obj.Link3 = kvp.Value;
                        set = true;
                        break;
                    case "link3public":
                        obj.Link3Public = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "mediatitle":
                        obj.MediaTitle = kvp.Value;
                        set = true;
                        break;
                    case "nodump":
                        obj.Nodump = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "origin":
                        obj.Origin = kvp.Value;
                        set = true;
                        break;
                    case "originalformat":
                        obj.OriginalFormat = kvp.Value;
                        set = true;
                        break;
                    case "project":
                        obj.Project = kvp.Value;
                        set = true;
                        break;
                    case "region":
                        obj.Region = kvp.Value;
                        set = true;
                        break;
                    case "releasedate":
                        obj.ReleaseDate = kvp.Value;
                        set = true;
                        break;
                    case "releasedateinfo":
                        obj.ReleaseDateInfo = kvp.Value.AsYesNo();
                        set = true;
                        break;
                    case "rominfo":
                        obj.RomInfo = kvp.Value;
                        set = true;
                        break;
                    case "section":
                        obj.Section = kvp.Value;
                        set = true;
                        break;
                    case "tool":
                        obj.Tool = kvp.Value;
                        set = true;
                        break;

                    default:
                        set = false;
                        break;
                }

                _logger.Verbose($"Item field {kvp.Key} {(set ? "set" : "could not be set")}");
            }
        }

        #endregion
    }
}
