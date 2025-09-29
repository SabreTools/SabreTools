using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents generic archive files to be included in a set
    /// </summary>
    [JsonObject("archive"), XmlRoot("archive")]
    public sealed class Archive : DatItem<Data.Models.Metadata.Archive>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Archive;

        // TODO: None of the following are used or checked

        /// <summary>
        /// Archive ID number
        /// </summary>
        /// <remarks>TODO: No-Intro database export only</remarks>
        [JsonProperty("number"), XmlElement("number")]
        public string? Number { get; set; }

        /// <summary>
        /// Clone value
        /// </summary>
        /// <remarks>TODO: No-Intro database export only</remarks>
        [JsonProperty("clone"), XmlElement("clone")]
        public string? CloneValue { get; set; }

        /// <summary>
        /// Regional parent value
        /// </summary>
        /// <remarks>TODO: No-Intro database export only</remarks>
        [JsonProperty("regparent"), XmlElement("regparent")]
        public string? RegParent { get; set; }

        /// <summary>
        /// Region value
        /// </summary>
        /// <remarks>TODO: No-Intro database export only</remarks>
        [JsonProperty("region"), XmlElement("region")]
        public string? Region { get; set; }

        /// <summary>
        /// Languages value
        /// </summary>
        /// <remarks>TODO: No-Intro database export only</remarks>
        [JsonProperty("languages"), XmlElement("languages")]
        public string? Languages { get; set; }

        /// <summary>
        /// Development status value
        /// </summary>
        /// <remarks>TODO: No-Intro database export only</remarks>
        [JsonProperty("devstatus"), XmlElement("devstatus")]
        public string? DevStatus { get; set; }

        /// <summary>
        /// Physical value
        /// </summary>
        /// <remarks>TODO: No-Intro database export only</remarks>
        /// <remarks>TODO: Is this numeric or a flag?</remarks>
        [JsonProperty("physical"), XmlElement("physical")]
        public string? Physical { get; set; }

        /// <summary>
        /// Complete value
        /// </summary>
        /// <remarks>TODO: No-Intro database export only</remarks>
        /// <remarks>TODO: Is this numeric or a flag?</remarks>
        [JsonProperty("complete"), XmlElement("complete")]
        public string? Complete { get; set; }

        /// <summary>
        /// Categories value
        /// </summary>
        /// <remarks>TODO: No-Intro database export only</remarks>
        [JsonProperty("categories"), XmlElement("categories")]
        public string? Categories { get; set; }

        #endregion

        #region Constructors

        public Archive() : base() { }

        public Archive(Data.Models.Metadata.Archive item) : base(item) { }

        public Archive(Data.Models.Metadata.Archive item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
