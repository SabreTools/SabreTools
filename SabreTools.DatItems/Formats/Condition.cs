using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a condition on a machine or other item
    /// </summary>
    [JsonObject("condition"), XmlRoot("condition")]
    public sealed class Condition : DatItem<Models.Metadata.Condition>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Condition;

        #endregion

        #region Constructors

        public Condition() : base() { }

        public Condition(Models.Metadata.Condition item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Models.Metadata.Condition.RelationKey) != null)
                SetFieldValue<string?>(Models.Metadata.Condition.RelationKey, GetStringFieldValue(Models.Metadata.Condition.RelationKey).AsRelation().AsStringValue());
        }

        #endregion
    }
}
