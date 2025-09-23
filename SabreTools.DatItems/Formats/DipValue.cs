using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one ListXML dipvalue
    /// </summary>
    [JsonObject("dipvalue"), XmlRoot("dipvalue")]
    public sealed class DipValue : DatItem<Models.Metadata.DipValue>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.DipValue;

        [JsonIgnore]
        public bool ConditionsSpecified
        {
            get
            {
                var conditions = GetFieldValue<Condition[]?>(Models.Metadata.DipValue.ConditionKey);
                return conditions != null && conditions.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public DipValue() : base() { }

        public DipValue(Models.Metadata.DipValue item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.DipValue.DefaultKey) != null)
                SetFieldValue<string?>(Models.Metadata.DipValue.DefaultKey, GetBoolFieldValue(Models.Metadata.DipValue.DefaultKey).FromYesNo());

            // Handle subitems
            var condition = GetFieldValue<Models.Metadata.Condition>(Models.Metadata.DipValue.ConditionKey);
            if (condition != null)
                SetFieldValue<Condition?>(Models.Metadata.DipValue.ConditionKey, new Condition(condition));
        }

        public DipValue(Models.Metadata.DipValue item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override Models.Metadata.DipValue GetInternalClone()
        {
            var dipValueItem = base.GetInternalClone();

            // Handle subitems
            var subCondition = GetFieldValue<Condition>(Models.Metadata.DipValue.ConditionKey);
            if (subCondition != null)
                dipValueItem[Models.Metadata.DipValue.ConditionKey] = subCondition.GetInternalClone();

            return dipValueItem;
        }

        #endregion
    }
}
