using System.Xml.Serialization;
using Newtonsoft.Json;

// TODO: Add item mappings for all fields
namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a single source details item
    /// </summary>
    [JsonObject("source_details"), XmlRoot("source_details")]
    public sealed class SourceDetails : DatItem
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.SourceDetails;

        /// <summary>
        /// Id value
        /// </summary>
        /// <remarks>TODO: Is this required?</remarks>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Section value
        /// </summary>
        [JsonProperty("section", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("section")]
        public string? Section { get; set; }

        /// <summary>
        /// Rom info value
        /// </summary>
        [JsonProperty("rominfo", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("rominfo")]
        public string? RomInfo { get; set; }

        /// <summary>
        /// Dumping date value
        /// </summary>
        [JsonProperty("d_date", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("d_date")]
        public string? DDate { get; set; }

        /// <summary>
        /// Dumping date info value
        /// </summary>
        [JsonProperty("d_date_info", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("d_date_info")]
        public string? DDateInfo { get; set; }

        /// <summary>
        /// Release date value
        /// </summary>
        [JsonProperty("r_date", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("r_date")]
        public string? RDate { get; set; }

        /// <summary>
        /// Release date info value
        /// </summary>
        [JsonProperty("r_date_info", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("r_date_info")]
        public string? RDateInfo { get; set; }

        /// <summary>
        /// Origin value
        /// </summary>
        [JsonProperty("origin", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("origin")]
        public string? Origin { get; set; }

        /// <summary>
        /// Region value
        /// </summary>
        [JsonProperty("region", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("region")]
        public string? Region { get; set; }

        /// <summary>
        /// Media title value
        /// </summary>
        [JsonProperty("media_title", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("media_title")]
        public string? MediaTitle { get; set; }

        /// <summary>
        /// Dumper value
        /// </summary>
        [JsonProperty("dumper", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("dumper")]
        public string? Dumper { get; set; }

        /// <summary>
        /// Project value
        /// </summary>
        [JsonProperty("project", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("project")]
        public string? Project { get; set; }

        /// <summary>
        /// Original format value
        /// </summary>
        [JsonProperty("originalformat", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("originalformat")]
        public string? OriginalFormat { get; set; }

        /// <summary>
        /// Nodump value
        /// </summary>
        [JsonProperty("nodump", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("nodump")]
        public string? Nodump { get; set; }

        /// <summary>
        /// Tool value
        /// </summary>
        [JsonProperty("tool", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("tool")]
        public string? Tool { get; set; }

        /// <summary>
        /// Comment 1 value
        /// </summary>
        [JsonProperty("comment1", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("comment1")]
        public string? Comment1 { get; set; }

        /// <summary>
        /// Link 2 value
        /// </summary>
        [JsonProperty("comment2", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("comment2")]
        public string? Comment2 { get; set; }

        /// <summary>
        /// Link 1 value
        /// </summary>
        [JsonProperty("link1", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("link1")]
        public string? Link1 { get; set; }

        /// <summary>
        /// Link 2 value
        /// </summary>
        [JsonProperty("link2", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("link2")]
        public string? Link2 { get; set; }

        /// <summary>
        /// Link 3 value
        /// </summary>
        [JsonProperty("link3", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("link3")]
        public string? Link3 { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty SourceDetails object
        /// </summary>
        public SourceDetails()
        {
            SetFieldValue<ItemType>(Models.Metadata.DatItem.TypeKey, ItemType.SourceDetails);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override object Clone()
        {
            var sourceDetails = new SourceDetails()
            {
                Id = this.Id,
                Section = this.Section,
                RomInfo = this.RomInfo,
                DDate = this.DDate,
                DDateInfo = this.DDateInfo,
                RDate = this.RDate,
                RDateInfo = this.RDateInfo,
                Origin = this.Origin,
                Region = this.Region,
                MediaTitle = this.MediaTitle,
                Dumper = this.Dumper,
                Project = this.Project,
                OriginalFormat = this.OriginalFormat,
                Nodump = this.Nodump,
                Tool = this.Tool,
                Comment1 = this.Comment1,
                Comment2 = this.Comment2,
                Link1 = this.Link1,
                Link2 = this.Link2,
                Link3 = this.Link3,
            };
            sourceDetails.SetFieldValue<DupeType>(DatItem.DupeTypeKey, GetFieldValue<DupeType>(DatItem.DupeTypeKey));
            sourceDetails.SetFieldValue<Machine>(DatItem.MachineKey, GetMachine());
            sourceDetails.SetFieldValue<bool?>(DatItem.RemoveKey, GetBoolFieldValue(DatItem.RemoveKey));
            sourceDetails.SetFieldValue<Source?>(DatItem.SourceKey, GetFieldValue<Source?>(DatItem.SourceKey));
            sourceDetails.SetFieldValue<string?>(Models.Metadata.DatItem.TypeKey, GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue());

            return sourceDetails;
        }

        #endregion

        #region Comparision Methods

        /// <inheritdoc/>
        public override bool Equals(DatItem? other)
        {
            // If we don't have a SourceDetails, return false
            if (GetStringFieldValue(Models.Metadata.DatItem.TypeKey) != other?.GetStringFieldValue(Models.Metadata.DatItem.TypeKey))
                return false;

            // Otherwise, treat it as a SourceDetails
            SourceDetails? newOther = other as SourceDetails;

            // If the Details information matches
            return (Id == newOther!.Id
                && Section == newOther.Section
                && RomInfo == newOther.RomInfo
                && DDate == newOther.DDate
                && DDateInfo == newOther.DDateInfo
                && RomInfo == newOther.RomInfo
                && RDate == newOther.RDate
                && RDateInfo == newOther.RDateInfo
                && Origin == newOther.Origin
                && Region == newOther.Region
                && MediaTitle == newOther.MediaTitle
                && Dumper == newOther.Dumper
                && Project == newOther.Project
                && OriginalFormat == newOther.OriginalFormat
                && Nodump == newOther.Nodump
                && Tool == newOther.Tool
                && Comment1 == newOther.Comment1
                && Comment2 == newOther.Comment2
                && Link1 == newOther.Link1
                && Link2 == newOther.Link2
                && Link3 == newOther.Link3);
        }

        #endregion
    }
}
