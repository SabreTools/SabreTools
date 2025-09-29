using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one part feature object
    /// </summary>
    [JsonObject("part_feature"), XmlRoot("part_feature")]
    public sealed class PartFeature : DatItem<Data.Models.Metadata.Feature>
    {
        #region Constants

        /// <summary>
        /// Non-standard key for inverted logic
        /// </summary>
        public const string PartKey = "PART";

        #endregion

        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.PartFeature;

        #endregion

        #region Constructors

        public PartFeature() : base() { }

        public PartFeature(Data.Models.Metadata.Feature item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Data.Models.Metadata.Feature.OverallKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Feature.OverallKey, GetStringFieldValue(Data.Models.Metadata.Feature.OverallKey).AsFeatureStatus().AsStringValue());
            if (GetStringFieldValue(Data.Models.Metadata.Feature.StatusKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Feature.StatusKey, GetStringFieldValue(Data.Models.Metadata.Feature.StatusKey).AsFeatureStatus().AsStringValue());
            if (GetStringFieldValue(Data.Models.Metadata.Feature.FeatureTypeKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Feature.FeatureTypeKey, GetStringFieldValue(Data.Models.Metadata.Feature.FeatureTypeKey).AsFeatureType().AsStringValue());
        }

        public PartFeature(Data.Models.Metadata.Feature item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
