﻿using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents release information about a set
    /// </summary>
    [JsonObject("release"), XmlRoot("release")]
    public sealed class Release : DatItem<Models.Metadata.Release>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Release;

        /// <inheritdoc>/>
        protected override string? NameKey => Models.Metadata.Release.NameKey;

        #endregion

        #region Constructors

        public Release() : base() { }

        public Release(Models.Metadata.Release item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.Release.DefaultKey) != null)
                SetFieldValue<string?>(Models.Metadata.Release.DefaultKey, GetBoolFieldValue(Models.Metadata.Release.DefaultKey).FromYesNo());
        }

        #endregion
    }
}
