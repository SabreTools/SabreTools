using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which Adjuster(s) is associated with a set
    /// </summary>
    [JsonObject("adjuster"), XmlRoot("adjuster")]
    public sealed class Adjuster : DatItem<Data.Models.Metadata.Adjuster>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Adjuster;

        [JsonIgnore]
        public bool ConditionsSpecified
        {
            get
            {
                var conditions = GetFieldValue<Condition[]?>(Data.Models.Metadata.Adjuster.ConditionKey);
                return conditions != null && conditions.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public Adjuster() : base() { }

        public Adjuster(Data.Models.Metadata.Adjuster item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Data.Models.Metadata.Adjuster.DefaultKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Adjuster.DefaultKey, GetBoolFieldValue(Data.Models.Metadata.Adjuster.DefaultKey).FromYesNo());

            // Handle subitems
            var condition = item.Read<Data.Models.Metadata.Condition>(Data.Models.Metadata.Adjuster.ConditionKey);
            if (condition != null)
                SetFieldValue(Data.Models.Metadata.Adjuster.ConditionKey, new Condition(condition));
        }

        public Adjuster(Data.Models.Metadata.Adjuster item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override Data.Models.Metadata.Adjuster GetInternalClone()
        {
            var adjusterItem = base.GetInternalClone();

            var condition = GetFieldValue<Condition?>(Data.Models.Metadata.Adjuster.ConditionKey);
            if (condition != null)
                adjusterItem[Data.Models.Metadata.Adjuster.ConditionKey] = condition.GetInternalClone();

            return adjusterItem;
        }

        #endregion
    }
}
