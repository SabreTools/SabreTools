using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a condition on a machine or other item
    /// </summary>
    [JsonObject("condition"), XmlRoot("condition")]
    public sealed class Condition : DatItem<Data.Models.Metadata.Condition>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Condition;

        #endregion

        #region Constructors

        public Condition() : base() { }

        public Condition(Data.Models.Metadata.Condition item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Data.Models.Metadata.Condition.RelationKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Condition.RelationKey, GetStringFieldValue(Data.Models.Metadata.Condition.RelationKey).AsRelation().AsStringValue());
        }

        public Condition(Data.Models.Metadata.Condition item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
