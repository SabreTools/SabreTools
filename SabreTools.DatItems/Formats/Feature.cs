using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents the a feature of the machine
    /// </summary>
    [JsonObject("feature"), XmlRoot("feature")]
    public sealed class Feature : DatItem<Data.Models.Metadata.Feature>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Feature;

        #endregion

        #region Constructors

        public Feature() : base() { }

        public Feature(Data.Models.Metadata.Feature item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Data.Models.Metadata.Feature.OverallKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Feature.OverallKey, GetStringFieldValue(Data.Models.Metadata.Feature.OverallKey).AsFeatureStatus().AsStringValue());
            if (GetStringFieldValue(Data.Models.Metadata.Feature.StatusKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Feature.StatusKey, GetStringFieldValue(Data.Models.Metadata.Feature.StatusKey).AsFeatureStatus().AsStringValue());
            if (GetStringFieldValue(Data.Models.Metadata.Feature.FeatureTypeKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Feature.FeatureTypeKey, GetStringFieldValue(Data.Models.Metadata.Feature.FeatureTypeKey).AsFeatureType().AsStringValue());
        }

        public Feature(Data.Models.Metadata.Feature item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
