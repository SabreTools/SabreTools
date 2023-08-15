﻿using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents special information about a machine
    /// </summary>
    [JsonObject("info"), XmlRoot("info")]
    public class Info : DatItem
    {
        #region Fields

        /// <summary>
        /// Name of the item
        /// </summary>
        [JsonProperty("name"), XmlElement("name")]
        public string? Name
        {
            get => _internal.ReadString(Models.Internal.Info.NameKey);
            set => _internal[Models.Internal.Info.NameKey] = value;
        }

        /// <summary>
        /// Information value
        /// </summary>
        [JsonProperty("value"), XmlElement("value")]
        public string? Value
        {
            get => _internal.ReadString(Models.Internal.Info.ValueKey);
            set => _internal[Models.Internal.Info.ValueKey] = value;
        }

        #endregion

        #region Accessors

        /// <inheritdoc/>
        public override string? GetName() => Name;

        /// <inheritdoc/>
        public override void SetName(string? name) => Name = name;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty Info object
        /// </summary>
        public Info()
        {
            _internal = new Models.Internal.Info();
            Machine = new Machine();

            Name = string.Empty;
            ItemType = ItemType.Info;
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override object Clone()
        {
            return new Info()
            {
                ItemType = this.ItemType,
                DupeType = this.DupeType,

                Machine = this.Machine.Clone() as Machine ?? new Machine(),
                Source = this.Source?.Clone() as Source,
                Remove = this.Remove,

                _internal = this._internal?.Clone() as Models.Internal.Info ?? new Models.Internal.Info(),
            };
        }

        #endregion
    }
}
