﻿using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which RAM option(s) is associated with a set
    /// </summary>
    [JsonObject("ramoption"), XmlRoot("ramoption")]
    public sealed class RamOption : DatItem<Models.Metadata.RamOption>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.RamOption;

        /// <inheritdoc>/>
        protected override string? NameKey => Models.Metadata.RamOption.NameKey;

        #endregion

        #region Constructors

        public RamOption() : base() { }

        public RamOption(Models.Metadata.RamOption item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.RamOption.DefaultKey) != null)
                SetFieldValue<string?>(Models.Metadata.RamOption.DefaultKey, GetBoolFieldValue(Models.Metadata.RamOption.DefaultKey).FromYesNo());
        }

        #endregion
    }
}
