﻿using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a (usually WAV-formatted) sample to be included for use in the set
    /// </summary>
    [JsonObject("sample"), XmlRoot("sample")]
    public class Sample : DatItem<Models.Metadata.Sample>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Sample;

        #endregion

        #region Constructors

        public Sample() : base() { }

        public Sample(Models.Metadata.Sample item) : base(item) { }

        #endregion
    }
}
