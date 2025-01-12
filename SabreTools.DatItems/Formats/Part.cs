﻿using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// SoftwareList part information
    /// </summary>
    /// <remarks>One Part can contain multiple PartFeature, DataArea, DiskArea, and DipSwitch items</remarks>
    [JsonObject("part"), XmlRoot("part")]
    public sealed class Part : DatItem<Models.Metadata.Part>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Part;

        /// <inheritdoc>/>
        protected override string? NameKey => Models.Metadata.Part.NameKey;

        [JsonIgnore]
        public bool FeaturesSpecified
        {
            get
            {
                var features = GetFieldValue<PartFeature[]?>(Models.Metadata.Part.FeatureKey);
                return features != null && features.Length > 0;
            }
        }

        #endregion

        #region Constructors

        public Part() : base() { }

        public Part(Models.Metadata.Part item) : base(item) { }

        #endregion
    }
}
