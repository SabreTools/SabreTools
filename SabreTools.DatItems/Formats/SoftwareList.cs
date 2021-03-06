﻿using System.Xml.Serialization;

using SabreTools.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which SoftwareList(s) is associated with a set
    /// </summary>
    [JsonObject("softwarelist"), XmlRoot("softwarelist")]
    public class SoftwareList : DatItem
    {
        #region Fields

        /// <summary>
        /// Name of the item
        /// </summary>
        [JsonProperty("name")]
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Status of the softare list according to the machine
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        [XmlElement("status")]
        public SoftwareListStatus Status { get; set; }

        [JsonIgnore]
        public bool StatusSpecified { get { return Status != SoftwareListStatus.NULL; } }

        /// <summary>
        /// Filter to apply to the software list
        /// </summary>
        [JsonProperty("filter", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("filter")]
        public string Filter { get; set; }

        #endregion

        #region Accessors

        /// <inheritdoc/>
        public override string GetName() => Name;

        /// <inheritdoc/>
        public override void SetName(string name) => Name = name;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty SoftwareList object
        /// </summary>
        public SoftwareList()
        {
            Name = string.Empty;
            ItemType = ItemType.SoftwareList;
        }

        #endregion

        #region Cloning Methods

        public override object Clone()
        {
            return new SoftwareList()
            {
                ItemType = this.ItemType,
                DupeType = this.DupeType,

                Machine = this.Machine.Clone() as Machine,
                Source = this.Source.Clone() as Source,
                Remove = this.Remove,

                Name = this.Name,
                Status = this.Status,
                Filter = this.Filter,
            };
        }

        #endregion

        #region Comparision Methods

        public override bool Equals(DatItem other)
        {
            // If we don't have a sample, return false
            if (ItemType != other.ItemType)
                return false;

            // Otherwise, treat it as a SoftwareList
            SoftwareList newOther = other as SoftwareList;

            // If the SoftwareList information matches
            return (Name == newOther.Name
                && Status == newOther.Status
                && Filter == newOther.Filter);
        }

        #endregion
    }
}
