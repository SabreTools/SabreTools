﻿using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents the sound output for a machine
    /// </summary>
    [JsonObject("sound"), XmlRoot("sound")]
    public sealed class Sound : DatItem<Models.Metadata.Sound>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Sound;

        #endregion

        #region Constructors

        public Sound() : base() { }

        public Sound(Models.Metadata.Sound item) : base(item)
        {
            // Process flag values
            if (GetInt64FieldValue(Models.Metadata.Sound.ChannelsKey) != null)
                SetFieldValue<string?>(Models.Metadata.Sound.ChannelsKey, GetInt64FieldValue(Models.Metadata.Sound.ChannelsKey).ToString());
        }

        #endregion
    }
}
