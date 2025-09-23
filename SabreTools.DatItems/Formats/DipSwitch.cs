using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which DIP Switch(es) is associated with a set
    /// </summary>
    [JsonObject("dipswitch"), XmlRoot("dipswitch")]
    public sealed class DipSwitch : DatItem<Models.Metadata.DipSwitch>
    {
        #region Constants

        /// <summary>
        /// Non-standard key for inverted logic
        /// </summary>
        public const string PartKey = "PART";

        #endregion

        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.DipSwitch;

        [JsonIgnore]
        public bool ConditionsSpecified
        {
            get
            {
                var conditions = GetFieldValue<Condition[]?>(Models.Metadata.DipSwitch.ConditionKey);
                return conditions != null && conditions.Length > 0;
            }
        }

        [JsonIgnore]
        public bool LocationsSpecified
        {
            get
            {
                var locations = GetFieldValue<DipLocation[]?>(Models.Metadata.DipSwitch.DipLocationKey);
                return locations != null && locations.Length > 0;
            }
        }

        [JsonIgnore]
        public bool ValuesSpecified
        {
            get
            {
                var values = GetFieldValue<DipValue[]?>(Models.Metadata.DipSwitch.DipValueKey);
                return values != null && values.Length > 0;
            }
        }

        [JsonIgnore]
        public bool PartSpecified
        {
            get
            {
                var part = GetFieldValue<Part?>(DipSwitch.PartKey);
                return part != null
                    && (!string.IsNullOrEmpty(part.GetName())
                        || !string.IsNullOrEmpty(part.GetStringFieldValue(Models.Metadata.Part.InterfaceKey)));
            }
        }

        #endregion

        #region Constructors

        public DipSwitch() : base() { }

        public DipSwitch(Models.Metadata.DipSwitch item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.DipSwitch.DefaultKey) != null)
                SetFieldValue<string?>(Models.Metadata.DipSwitch.DefaultKey, GetBoolFieldValue(Models.Metadata.DipSwitch.DefaultKey).FromYesNo());

            // Handle subitems
            var condition = item.Read<Models.Metadata.Condition>(Models.Metadata.DipSwitch.ConditionKey);
            if (condition != null)
                SetFieldValue<Condition?>(Models.Metadata.DipSwitch.ConditionKey, new Condition(condition));

            var dipLocations = item.ReadItemArray<Models.Metadata.DipLocation>(Models.Metadata.DipSwitch.DipLocationKey);
            if (dipLocations != null)
            {
                DipLocation[] dipLocationItems = Array.ConvertAll(dipLocations, dipLocation => new DipLocation(dipLocation));
                SetFieldValue<DipLocation[]?>(Models.Metadata.DipSwitch.DipLocationKey, dipLocationItems);
            }

            var dipValues = item.ReadItemArray<Models.Metadata.DipValue>(Models.Metadata.DipSwitch.DipValueKey);
            if (dipValues != null)
            {
                DipValue[] dipValueItems = Array.ConvertAll(dipValues, dipValue => new DipValue(dipValue));
                SetFieldValue<DipValue[]?>(Models.Metadata.DipSwitch.DipValueKey, dipValueItems);
            }
        }

        public DipSwitch(Models.Metadata.DipSwitch item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override Models.Metadata.DipSwitch GetInternalClone()
        {
            var dipSwitchItem = base.GetInternalClone();

            var condition = GetFieldValue<Condition?>(Models.Metadata.DipSwitch.ConditionKey);
            if (condition != null)
                dipSwitchItem[Models.Metadata.DipSwitch.ConditionKey] = condition.GetInternalClone();

            var dipLocations = GetFieldValue<DipLocation[]?>(Models.Metadata.DipSwitch.DipLocationKey);
            if (dipLocations != null)
            {
                Models.Metadata.DipLocation[] dipLocationItems = Array.ConvertAll(dipLocations, dipLocation => dipLocation.GetInternalClone());
                dipSwitchItem[Models.Metadata.DipSwitch.DipLocationKey] = dipLocationItems;
            }

            var dipValues = GetFieldValue<DipValue[]?>(Models.Metadata.DipSwitch.DipValueKey);
            if (dipValues != null)
            {
                Models.Metadata.DipValue[] dipValueItems = Array.ConvertAll(dipValues, dipValue => dipValue.GetInternalClone());
                dipSwitchItem[Models.Metadata.DipSwitch.DipValueKey] = dipValueItems;
            }

            return dipSwitchItem;
        }

        #endregion
    }
}
