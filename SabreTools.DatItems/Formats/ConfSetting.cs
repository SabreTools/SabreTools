using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one ListXML confsetting
    /// </summary>
    [JsonObject("confsetting"), XmlRoot("confsetting")]
    public sealed class ConfSetting : DatItem<Models.Metadata.ConfSetting>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.ConfSetting;

        [JsonIgnore]
        public bool ConditionsSpecified
        {
            get
            {
                var conditions = GetFieldValue<Condition[]?>(Models.Metadata.ConfSetting.ConditionKey);
                return conditions != null && conditions.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public ConfSetting() : base() { }

        public ConfSetting(Models.Metadata.ConfSetting item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.ConfSetting.DefaultKey) != null)
                SetFieldValue<string?>(Models.Metadata.ConfSetting.DefaultKey, GetBoolFieldValue(Models.Metadata.ConfSetting.DefaultKey).FromYesNo());

            // Handle subitems
            var condition = GetFieldValue<Models.Metadata.Condition>(Models.Metadata.ConfSetting.ConditionKey);
            if (condition != null)
                SetFieldValue<Condition?>(Models.Metadata.ConfSetting.ConditionKey, new Condition(condition));
        }

        public ConfSetting(Models.Metadata.ConfSetting item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override Models.Metadata.ConfSetting GetInternalClone()
        {
            var confSettingItem = base.GetInternalClone();

            // Handle subitems
            var condition = GetFieldValue<Condition>(Models.Metadata.ConfSetting.ConditionKey);
            if (condition != null)
                confSettingItem[Models.Metadata.ConfSetting.ConditionKey] = condition.GetInternalClone();

            return confSettingItem;
        }

        #endregion
    }
}
