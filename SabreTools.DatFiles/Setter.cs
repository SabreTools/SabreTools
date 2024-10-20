using System.Collections.Generic;
using System.Linq;
using SabreTools.Core.Filter;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Logging;

namespace SabreTools.DatFiles
{
    /// <summary>
    /// Set fields on DatItems
    /// </summary>
    /// TODO: Figure out how to get this into the Filtering namespace
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
        public Dictionary<(string, string), string> ItemFieldMappings { get; } = [];

        #endregion

        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private readonly Logger logger = new();

        #endregion

        #region Population

        /// <summary>
        /// Populate the setters using a field name and a value
        /// </summary>
        /// <param name="field">Field name</param>
        /// <param name="value">Field value</param>
        public void PopulateSetters(string field, string value)
            => PopulateSettersFromList([field], [value]);

        /// <summary>
        /// Populate the setters using a set of field names
        /// </summary>
        /// <param name="fields">List of field names</param>
        /// <param name="values">List of field values</param>
        public void PopulateSettersFromList(List<string> fields, List<string> values)
        {
            // If the list is null or empty, just return
            if (values == null || values.Count == 0)
                return;

            var watch = new InternalStopwatch("Populating setters from list");

            // Now we loop through and get values for everything
            for (int i = 0; i < fields.Count; i++)
            {
                string field = fields[i];
                string value = values[i];

                if (!SetSetter(field, value))
                    logger.Warning($"The value {value} did not match any known field names. Please check the wiki for more details on supported field names.");
            }

            watch.Stop();
        }

        /// <summary>
        /// Populate the setters using a set of field names
        /// </summary>
        /// <param name="mappings">Dictionary of mappings</param>
        public void PopulateSettersFromDictionary(Dictionary<(string, string), string>? mappings)
        {
            // If the dictionary is null or empty, just return
            if (mappings == null || mappings.Count == 0)
                return;

            var watch = new InternalStopwatch("Populating setters from dictionary");

            // Now we loop through and get values for everything
            foreach (var mapping in mappings)
            {
                string field = $"{mapping.Key.Item1}.{mapping.Key.Item2}";
                string value = mapping.Value;

                if (!SetSetter(field, value))
                    logger.Warning($"The value {value} did not match any known field names. Please check the wiki for more details on supported field names.");
            }

            watch.Stop();
        }

        /// <summary>
        /// Set remover from a value
        /// </summary>
        /// <param name="field">Key for the remover to be set</param>
        private bool SetSetter(string field, string value)
        {
            // If the key is null or empty, return false
            if (string.IsNullOrEmpty(field))
                return false;

            // Get the parser pair out of it, if possible
            (string? type, string? key) = FilterParser.ParseFilterId(field);
            if (type == null || key == null)
                return false;

            switch (type)
            {
                case Models.Metadata.MetadataFile.HeaderKey:
                    HeaderFieldMappings[key] = value;
                    return true;

                case Models.Metadata.MetadataFile.MachineKey:
                    MachineFieldMappings[key] = value;
                    return true;

                default:
                    ItemFieldMappings[(type, key)] = value;
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
            if (datHeader == null || HeaderFieldMappings.Count == 0)
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
            if (MachineFieldMappings.Count > 0 && datItem.GetFieldValue<Machine>(DatItem.MachineKey) != null)
                SetFields(datItem.GetFieldValue<Machine>(DatItem.MachineKey)!);

            // If there are no field names, return
            if (ItemFieldMappings == null || ItemFieldMappings.Count == 0)
                return;

            // If there are no field names for this type or generic, return
            string? itemType = datItem.GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsEnumValue<ItemType>().AsStringValue();
            if (itemType == null || (!ItemFieldMappings.Keys.Any(kvp => kvp.Item1 == itemType) && !ItemFieldMappings.Keys.Any(kvp => kvp.Item1 == "item")))
                return;

            // Get the combined list of fields to remove
            var fieldMappings = new Dictionary<string, string>();
            foreach (var mapping in ItemFieldMappings.Where(kvp => kvp.Key.Item1 == "item").ToDictionary(kvp => kvp.Key.Item2, kvp => kvp.Value))
            {
                fieldMappings[mapping.Key] = mapping.Value;
            }
            foreach (var mapping in ItemFieldMappings.Where(kvp => kvp.Key.Item1 == itemType).ToDictionary(kvp => kvp.Key.Item2, kvp => kvp.Value))
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
                case Adjuster adjuster: SetFields(adjuster); break;
                case Configuration condition: SetFields(condition); break;
                case ConfSetting confSetting: SetFields(confSetting); break;
                case Device device: SetFields(device); break;
                case DipSwitch dipSwitch: SetFields(dipSwitch); break;
                case DipValue dipValue: SetFields(dipValue); break;
                case Disk disk: SetFields(disk); break;
                case Input input: SetFields(input); break;
                case Part part: SetFields(part); break;
                case Port port: SetFields(port); break;
                case Rom rom: SetFields(rom); break;
                case Slot slot: SetFields(slot); break;
            }

            #endregion
        }

        /// <summary>
        /// Set fields with given values
        /// </summary>
        /// <param name="adjuster">Adjuster to remove replace fields in</param>
        private void SetFields(Adjuster adjuster)
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
        private void SetFields(Configuration configuration)
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
        private void SetFields(ConfSetting confSetting)
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
        private void SetFields(Device device)
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
        private void SetFields(DipSwitch dipSwitch)
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
        private void SetFields(DipValue dipValue)
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
        private void SetFields(Disk disk)
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
        private void SetFields(Input input)
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
        private void SetFields(Part part)
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
        private void SetFields(Port port)
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
        private void SetFields(Rom rom)
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
        private void SetFields(Slot slot)
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