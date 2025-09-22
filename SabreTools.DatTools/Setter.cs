using System.Collections.Generic;
using System.Linq;
using SabreTools.Core.Filter;
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.IO.Logging;

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
            => PopulateSettersFromList([field], [value]);

        /// <summary>
        /// Populate the setters using a set of field names
        /// </summary>
        /// <param name="fields">List of field names</param>
        /// <param name="values">List of field values</param>
        public void PopulateSettersFromList(List<FilterKey> fields, List<string> values)
        {
            // If the list is null or empty, just return
            if (values == null || values.Count == 0)
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
        public void PopulateSettersFromDictionary(Dictionary<FilterKey, string>? mappings)
        {
            // If the dictionary is null or empty, just return
            if (mappings == null || mappings.Count == 0)
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
        /// Set remover from a value
        /// </summary>
        /// <param name="key">Key for the remover to be set</param>
        private bool SetSetter(FilterKey key, string value)
        {
            switch (key.ItemName)
            {
                case Models.Metadata.MetadataFile.HeaderKey:
                    HeaderFieldMappings[key.FieldName] = value;
                    return true;

                case Models.Metadata.MetadataFile.MachineKey:
                    MachineFieldMappings[key.FieldName] = value;
                    return true;

                default:
                    ItemFieldMappings[key] = value;
                    return true;
            }
        }

        #endregion

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="datHeader">DatHeader to set fields on</param>
        public void SetFields(DatHeader datHeader)
        {
            // If we have an invalid input, return
            if (HeaderFieldMappings.Count == 0)
                return;

            foreach (var kvp in HeaderFieldMappings)
            {
                datHeader.SetField(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="machine">Machine to set fields on</param>
        public void SetFields(Machine? machine)
        {
            // If we have an invalid input, return
            if (machine == null || MachineFieldMappings.Count == 0)
                return;

            foreach (var kvp in MachineFieldMappings)
            {
                machine.SetField(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="datItem">DatItem to set fields on</param>
        public void SetFields(DatItem datItem)
        {
            // If we have an invalid input, return
            if (datItem == null)
                return;

            #region Common

            // Handle Machine fields
            if (MachineFieldMappings.Count > 0 && datItem.GetMachine() != null)
                SetFields(datItem.GetMachine()!);

            // If there are no field names, return
            if (ItemFieldMappings == null || ItemFieldMappings.Count == 0)
                return;

            // If there are no field names for this type or generic, return
            string? itemType = datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue();
            if (itemType == null || (!ItemFieldMappings.Keys.Any(kvp => kvp.ItemName == itemType) && !ItemFieldMappings.Keys.Any(kvp => kvp.ItemName == "item")))
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
            if (fieldMappings.Keys.Contains(Models.Metadata.Rom.NameKey))
            {
                datItem.SetName(fieldMappings[Models.Metadata.Rom.NameKey]);
                fieldMappings.Remove(Models.Metadata.Rom.NameKey);
            }

            #endregion

            #region Item-Specific

            // Handle unnested sets first
            foreach (var kvp in fieldMappings)
            {
                datItem.SetField(kvp.Key, kvp.Value);
            }

            // Handle nested sets
            switch (datItem)
            {
                case Adjuster adjuster: SetNestedFields(adjuster); break;
                case Configuration condition: SetNestedFields(condition); break;
                case ConfSetting confSetting: SetNestedFields(confSetting); break;
                case Device device: SetNestedFields(device); break;
                case DipSwitch dipSwitch: SetNestedFields(dipSwitch); break;
                case DipValue dipValue: SetNestedFields(dipValue); break;
                case Disk disk: SetNestedFields(disk); break;
                case Input input: SetNestedFields(input); break;
                case Part part: SetNestedFields(part); break;
                case Port port: SetNestedFields(port); break;
                case Rom rom: SetNestedFields(rom); break;
                case Slot slot: SetNestedFields(slot); break;
            }

            #endregion
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="adjuster">Adjuster to remove replace fields in</param>
        private void SetNestedFields(Adjuster adjuster)
        {
            // Field.DatItem_Conditions does not apply here
            if (adjuster.ConditionsSpecified)
            {
                foreach (Condition subCondition in adjuster.GetFieldValue<Condition[]?>(Models.Metadata.Adjuster.ConditionKey)!)
                {
                    SetFields(subCondition);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="configuration">Configuration to remove replace fields in</param>
        private void SetNestedFields(Configuration configuration)
        {
            if (configuration.ConditionsSpecified)
            {
                foreach (Condition subCondition in configuration.GetFieldValue<Condition[]?>(Models.Metadata.Configuration.ConditionKey)!)
                {
                    SetFields(subCondition);
                }
            }

            if (configuration.LocationsSpecified)
            {
                foreach (ConfLocation subLocation in configuration.GetFieldValue<ConfLocation[]?>(Models.Metadata.Configuration.ConfLocationKey)!)
                {
                    SetFields(subLocation);
                }
            }

            if (configuration.SettingsSpecified)
            {
                foreach (ConfSetting subSetting in configuration.GetFieldValue<ConfSetting[]?>(Models.Metadata.Configuration.ConfSettingKey)!)
                {
                    SetFields(subSetting as DatItem);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="confSetting">ConfSetting to remove replace fields in</param>
        private void SetNestedFields(ConfSetting confSetting)
        {
            if (confSetting.ConditionsSpecified)
            {
                foreach (Condition subCondition in confSetting.GetFieldValue<Condition[]?>(Models.Metadata.ConfSetting.ConditionKey)!)
                {
                    SetFields(subCondition);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="device">Device to remove replace fields in</param>
        private void SetNestedFields(Device device)
        {
            if (device.ExtensionsSpecified)
            {
                foreach (Extension subExtension in device.GetFieldValue<Extension[]?>(Models.Metadata.Device.ExtensionKey)!)
                {
                    SetFields(subExtension);
                }
            }

            if (device.InstancesSpecified)
            {
                foreach (Instance subInstance in device.GetFieldValue<Instance[]?>(Models.Metadata.Device.InstanceKey)!)
                {
                    SetFields(subInstance);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="dipSwitch">DipSwitch to remove replace fields in</param>
        private void SetNestedFields(DipSwitch dipSwitch)
        {
            if (dipSwitch.ConditionsSpecified)
            {
                foreach (Condition subCondition in dipSwitch.GetFieldValue<Condition[]?>(Models.Metadata.DipSwitch.ConditionKey)!)
                {
                    SetFields(subCondition);
                }
            }

            if (dipSwitch.LocationsSpecified)
            {
                foreach (DipLocation subLocation in dipSwitch.GetFieldValue<DipLocation[]?>(Models.Metadata.DipSwitch.DipLocationKey)!)
                {
                    SetFields(subLocation);
                }
            }

            if (dipSwitch.ValuesSpecified)
            {
                foreach (DipValue subValue in dipSwitch.GetFieldValue<DipValue[]?>(Models.Metadata.DipSwitch.DipValueKey)!)
                {
                    SetFields(subValue as DatItem);
                }
            }

            if (!dipSwitch.PartSpecified)
                dipSwitch.SetFieldValue<Part?>(DipSwitch.PartKey, new Part());

            SetFields((dipSwitch.GetFieldValue<Part?>(DipSwitch.PartKey) as DatItem)!);
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="dipValue">DipValue to remove replace fields in</param>
        private void SetNestedFields(DipValue dipValue)
        {
            if (dipValue.ConditionsSpecified)
            {
                foreach (Condition subCondition in dipValue.GetFieldValue<Condition[]?>(Models.Metadata.DipValue.ConditionKey)!)
                {
                    SetFields(subCondition);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="disk">Disk to remove replace fields in</param>
        private void SetNestedFields(Disk disk)
        {
            if (!disk.DiskAreaSpecified)
                disk.SetFieldValue<DiskArea?>(Disk.DiskAreaKey, new DiskArea());

            SetFields(disk.GetFieldValue<DiskArea?>(Disk.DiskAreaKey)! as DatItem);

            if (!disk.PartSpecified)
                disk.SetFieldValue<Part?>(Disk.PartKey, new Part());

            SetFields(disk.GetFieldValue<Part?>(Disk.PartKey)! as DatItem);
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="input">Input to remove replace fields in</param>
        private void SetNestedFields(Input input)
        {
            if (input.ControlsSpecified)
            {
                foreach (Control subControl in input.GetFieldValue<Control[]?>(Models.Metadata.Input.ControlKey)!)
                {
                    SetFields(subControl);
                }
            }
        }

        /// <summary>s
        /// Set fields with given values
        /// </summary>
        /// <param name="part">Part to remove replace fields in</param>
        private void SetNestedFields(Part part)
        {
            if (part.FeaturesSpecified)
            {
                foreach (PartFeature subPartFeature in part.GetFieldValue<PartFeature[]?>(Models.Metadata.Part.FeatureKey)!)
                {
                    SetFields(subPartFeature);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="port">Port to remove replace fields in</param>
        private void SetNestedFields(Port port)
        {
            if (port.AnalogsSpecified)
            {
                foreach (Analog subAnalog in port.GetFieldValue<Analog[]?>(Models.Metadata.Port.AnalogKey)!)
                {
                    SetFields(subAnalog);
                }
            }
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="rom">Rom to remove replace fields in</param>
        private void SetNestedFields(Rom rom)
        {
            if (!rom.DataAreaSpecified)
                rom.SetFieldValue<DataArea?>(Rom.DataAreaKey, new DataArea());

            SetFields(rom.GetFieldValue<DataArea?>(Rom.DataAreaKey)! as DatItem);

            if (!rom.PartSpecified)
                rom.SetFieldValue<Part?>(Rom.PartKey, new Part());

            SetFields(rom.GetFieldValue<Part?>(Rom.PartKey)! as DatItem);
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="slot">Slot to remove replace fields in</param>
        private void SetNestedFields(Slot slot)
        {
            if (slot.SlotOptionsSpecified)
            {
                foreach (SlotOption subSlotOption in slot.GetFieldValue<SlotOption[]?>(Models.Metadata.Slot.SlotOptionKey)!)
                {
                    SetFields(subSlotOption);
                }
            }
        }
    }
}
