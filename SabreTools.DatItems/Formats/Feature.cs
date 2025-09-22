using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents the a feature of the machine
    /// </summary>
    [JsonObject("feature"), XmlRoot("feature")]
    public sealed class Feature : DatItem<Models.Metadata.Feature>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Feature;

        #endregion

        #region Constructors

        public Feature() : base() { }

        public Feature(Models.Metadata.Feature item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Models.Metadata.Feature.OverallKey) != null)
                SetFieldValue<string?>(Models.Metadata.Feature.OverallKey, GetStringFieldValue(Models.Metadata.Feature.OverallKey).AsFeatureStatus().AsStringValue());
            if (GetStringFieldValue(Models.Metadata.Feature.StatusKey) != null)
                SetFieldValue<string?>(Models.Metadata.Feature.StatusKey, GetStringFieldValue(Models.Metadata.Feature.StatusKey).AsFeatureStatus().AsStringValue());
            if (GetStringFieldValue(Models.Metadata.Feature.FeatureTypeKey) != null)
                SetFieldValue<string?>(Models.Metadata.Feature.FeatureTypeKey, GetStringFieldValue(Models.Metadata.Feature.FeatureTypeKey).AsFeatureType().AsStringValue());
        }

        #endregion
    }
}
