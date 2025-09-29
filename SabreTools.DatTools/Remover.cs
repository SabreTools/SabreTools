using System.Collections.Generic;
using SabreTools.Core.Filter;
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.IO.Logging;

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
            => PopulateExclusionsFromList([field]);

        /// <summary>
        /// Populate the exclusion objects using a set of field names
        /// </summary>
        /// <param name="fields">List of field names</param>
        public void PopulateExclusionsFromList(List<string>? fields)
        {
            // If the list is null or empty, just return
            if (fields == null || fields.Count == 0)
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
                    case Data.Models.Metadata.MetadataFile.HeaderKey:
                        HeaderFieldNames.Add(key.FieldName);
                        return true;

                    case Data.Models.Metadata.MetadataFile.MachineKey:
                        MachineFieldNames.Add(key.FieldName);
                        return true;

                    default:
                        if (!ItemFieldNames.ContainsKey(key.ItemName))
                            ItemFieldNames[key.ItemName] = [];

                        ItemFieldNames[key.ItemName].Add(key.FieldName);
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

            RemoveHeaderFields(datFile.Header);
            RemoveItemFields(datFile.Items);
            RemoveItemFieldsDB(datFile.ItemsDB);

            watch.Stop();
        }

        /// <summary>
        /// Remove header fields with given values
        /// </summary>
        public void RemoveHeaderFields(DatHeader? datHeader)
        {
            // If we have an invalid input, return
            if (datHeader == null || HeaderFieldNames.Count == 0)
                return;

            foreach (var fieldName in HeaderFieldNames)
            {
                bool removed = datHeader.RemoveField(fieldName);
                _logger.Verbose($"Header field {fieldName} {(removed ? "removed" : "could not be removed")}");
            }
        }

        /// <summary>
        /// Apply removals to the item dictionary
        /// </summary>
        public void RemoveItemFields(ItemDictionary? itemDictionary)
        {
            // If we have an invalid input, return
            if (itemDictionary == null || (MachineFieldNames.Count == 0 && ItemFieldNames.Count == 0))
                return;

            foreach (var key in itemDictionary.SortedKeys)
            {
                List<DatItem>? items = itemDictionary.GetItemsForBucket(key);
                if (items == null)
                    continue;

                for (int j = 0; j < items.Count; j++)
                {
                    // Handle machine removals
                    var machine = items[j].GetMachine();
                    RemoveFields(machine);

                    // Handle item removals
                    RemoveFields(items[j]);
                }
            }
        }

        /// <summary>
        /// Apply removals to the item dictionary
        /// </summary>
        public void RemoveItemFieldsDB(ItemDictionaryDB? itemDictionary)
        {
            // If we have an invalid input, return
            if (itemDictionary == null || (MachineFieldNames.Count == 0 && ItemFieldNames.Count == 0))
                return;

            // Handle machine removals
            foreach (var kvp in itemDictionary.GetMachines())
            {
                RemoveFields(kvp.Value);
            }

            // Handle item removals
            foreach (var key in itemDictionary.SortedKeys)
            {
                var items = itemDictionary.GetItemsForBucket(key);
                if (items == null)
                    continue;

                foreach (var item in items.Values)
                {
                    RemoveFields(item);
                }
            }
        }

        /// <summary>
        /// Remove machine fields with given values
        /// </summary>
        internal void RemoveFields(Machine? machine)
        {
            // If we have an invalid input, return
            if (machine == null || MachineFieldNames.Count == 0)
                return;

            foreach (var fieldName in MachineFieldNames)
            {
                machine.RemoveField(fieldName);
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="datItem">DatItem to remove fields from</param>
        internal void RemoveFields(DatItem? datItem)
        {
            if (datItem == null)
                return;

            #region Common

            // If there are no field names, return
            if (ItemFieldNames == null || ItemFieldNames.Count == 0)
                return;

            // If there are no field names for this type or generic, return
            string? itemType = datItem.GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue();
            if (itemType == null || (!ItemFieldNames.ContainsKey(itemType) && !ItemFieldNames.ContainsKey("item")))
                return;

            // Get the combined list of fields to remove
            var fields = new HashSet<string>();
            if (ItemFieldNames.ContainsKey(itemType))
                fields.UnionWith(ItemFieldNames[itemType]);
            if (ItemFieldNames.ContainsKey("item"))
                fields.UnionWith(ItemFieldNames["item"]);

            // If the field specifically contains Name, set it separately
            if (fields.Contains(Data.Models.Metadata.Rom.NameKey))
                datItem.SetName(null);

            #endregion

            #region Item-Specific

            // Handle unnested removals first
            foreach (var datItemField in fields)
            {
                datItem.RemoveField(datItemField);
            }

            // Handle nested removals
            switch (datItem)
            {
                case Adjuster adjuster: RemoveNestedFields(adjuster); break;
                case Configuration configuration: RemoveNestedFields(configuration); break;
                case ConfSetting confSetting: RemoveNestedFields(confSetting); break;
                case Device device: RemoveNestedFields(device); break;
                case DipSwitch dipSwitch: RemoveNestedFields(dipSwitch); break;
                case DipValue dipValue: RemoveNestedFields(dipValue); break;
                case Disk disk: RemoveNestedFields(disk); break;
                case Input input: RemoveNestedFields(input); break;
                case Part part: RemoveNestedFields(part); break;
                case Port port: RemoveNestedFields(port); break;
                case Rom rom: RemoveNestedFields(rom); break;
                case Slot slot: RemoveNestedFields(slot); break;
            }

            #endregion
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="adjuster">Adjuster to remove fields from</param>
        private void RemoveNestedFields(Adjuster adjuster)
        {
            var conditions = adjuster.GetFieldValue<Condition[]?>(Data.Models.Metadata.Adjuster.ConditionKey) ?? [];
            foreach (Condition subCondition in conditions)
            {
                RemoveFields(subCondition);
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="configuration">Configuration to remove fields from</param>
        private void RemoveNestedFields(Configuration configuration)
        {
            var conditions = configuration.GetFieldValue<Condition[]?>(Data.Models.Metadata.Configuration.ConditionKey) ?? [];
            foreach (Condition subCondition in conditions)
            {
                RemoveFields(subCondition);
            }

            var locations = configuration.GetFieldValue<ConfLocation[]?>(Data.Models.Metadata.Configuration.ConfLocationKey) ?? [];
            foreach (ConfLocation subLocation in locations)
            {
                RemoveFields(subLocation);
            }

            var settings = configuration.GetFieldValue<ConfSetting[]?>(Data.Models.Metadata.Configuration.ConfSettingKey) ?? [];
            foreach (ConfSetting subSetting in settings)
            {
                RemoveFields(subSetting);
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="confsetting">ConfSetting to remove fields from</param>
        private void RemoveNestedFields(ConfSetting confsetting)
        {
            var conditions = confsetting.GetFieldValue<Condition[]?>(Data.Models.Metadata.ConfSetting.ConditionKey) ?? [];
            foreach (Condition subCondition in conditions)
            {
                RemoveFields(subCondition);
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="device">Device to remove fields from</param>
        private void RemoveNestedFields(Device device)
        {
            var extensions = device.GetFieldValue<Extension[]?>(Data.Models.Metadata.Device.ExtensionKey) ?? [];
            foreach (Extension subExtension in extensions)
            {
                RemoveFields(subExtension);
            }

            var instances = device.GetFieldValue<Instance[]?>(Data.Models.Metadata.Device.InstanceKey) ?? [];
            foreach (Instance subInstance in instances)
            {
                RemoveFields(subInstance);
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="dipSwitch">DipSwitch to remove fields from</param>
        private void RemoveNestedFields(DipSwitch dipSwitch)
        {
            var conditions = dipSwitch.GetFieldValue<Condition[]?>(Data.Models.Metadata.DipSwitch.ConditionKey) ?? [];
            foreach (Condition subCondition in conditions)
            {
                RemoveFields(subCondition);
            }

            var locations = dipSwitch.GetFieldValue<DipLocation[]?>(Data.Models.Metadata.DipSwitch.DipLocationKey) ?? [];
            foreach (DipLocation subLocation in locations)
            {
                RemoveFields(subLocation);
            }

            var dipValues = dipSwitch.GetFieldValue<DipValue[]?>(Data.Models.Metadata.DipSwitch.DipValueKey) ?? [];
            foreach (DipValue subValue in dipValues)
            {
                RemoveFields(subValue);
            }

            var part = dipSwitch.GetFieldValue<Part?>(DipSwitch.PartKey);
            if (part != null)
                RemoveFields(part);
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="dipValue">DipValue to remove fields from</param>
        private void RemoveNestedFields(DipValue dipValue)
        {
            var conditions = dipValue.GetFieldValue<Condition[]?>(Data.Models.Metadata.DipValue.ConditionKey) ?? [];
            foreach (Condition subCondition in conditions)
            {
                RemoveFields(subCondition);
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="disk">Disk to remove fields from</param>
        private void RemoveNestedFields(Disk disk)
        {
            var diskArea = disk.GetFieldValue<DiskArea?>(Disk.DiskAreaKey);
            if (diskArea != null)
                RemoveFields(diskArea);

            var part = disk.GetFieldValue<Part?>(Disk.PartKey);
            if (part != null)
                RemoveFields(part);
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="input">Input to remove fields from</param>
        private void RemoveNestedFields(Input input)
        {
            var controls = input.GetFieldValue<Control[]?>(Data.Models.Metadata.Input.ControlKey) ?? [];
            foreach (Control subControl in controls)
            {
                RemoveFields(subControl);
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="part">Part to remove fields from</param>
        private void RemoveNestedFields(Part part)
        {
            var features = part.GetFieldValue<PartFeature[]?>(Data.Models.Metadata.Part.FeatureKey) ?? [];
            foreach (PartFeature subPartFeature in features)
            {
                RemoveFields(subPartFeature);
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="port">Port to remove fields from</param>
        private void RemoveNestedFields(Port port)
        {
            var analogs = port.GetFieldValue<Analog[]?>(Data.Models.Metadata.Port.AnalogKey) ?? [];
            foreach (Analog subAnalog in analogs)
            {
                RemoveFields(subAnalog);
            }
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="rom">Rom to remove fields from</param>
        private void RemoveNestedFields(Rom rom)
        {
            var dataArea = rom.GetFieldValue<DataArea?>(Rom.DataAreaKey);
            if (dataArea != null)
                RemoveFields(dataArea);

            var part = rom.GetFieldValue<Part?>(Rom.PartKey);
            if (part != null)
                RemoveFields(part);
        }

        /// <summary>
        /// Remove fields with given values
        /// </summary>
        /// <param name="slot">Slot to remove fields from</param>
        private void RemoveNestedFields(Slot slot)
        {
            var slotOptions = slot.GetFieldValue<SlotOption[]?>(Data.Models.Metadata.Slot.SlotOptionKey) ?? [];
            foreach (SlotOption subSlotOption in slotOptions)
            {
                RemoveFields(subSlotOption);
            }
        }

        #endregion
    }
}
