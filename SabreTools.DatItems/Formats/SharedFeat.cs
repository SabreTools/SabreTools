﻿using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one shared feature object
    /// </summary>
    [JsonObject("sharedfeat"), XmlRoot("sharedfeat")]
    public sealed class SharedFeat : DatItem<Models.Metadata.SharedFeat>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.SharedFeat;

        #endregion

        #region Constructors

        public SharedFeat() : base() { }

        public SharedFeat(Models.Metadata.SharedFeat item) : base(item) { }

        #endregion
    }
}
