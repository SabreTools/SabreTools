﻿using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one part feature object
    /// </summary>
    [JsonObject("part_feature"), XmlRoot("part_feature")]
    public sealed class PartFeature : DatItem<Models.Metadata.Feature>
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

        public PartFeature(Models.Metadata.Feature item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Models.Metadata.Feature.OverallKey) != null)
                SetFieldValue<string?>(Models.Metadata.Feature.OverallKey, GetStringFieldValue(Models.Metadata.Feature.OverallKey).AsEnumValue<FeatureStatus>().AsStringValue());
            if (GetStringFieldValue(Models.Metadata.Feature.StatusKey) != null)
                SetFieldValue<string?>(Models.Metadata.Feature.StatusKey, GetStringFieldValue(Models.Metadata.Feature.StatusKey).AsEnumValue<FeatureStatus>().AsStringValue());
            if (GetStringFieldValue(Models.Metadata.Feature.FeatureTypeKey) != null)
                SetFieldValue<string?>(Models.Metadata.Feature.FeatureTypeKey, GetStringFieldValue(Models.Metadata.Feature.FeatureTypeKey).AsEnumValue<FeatureType>().AsStringValue());
        }

        #endregion
    }
}
