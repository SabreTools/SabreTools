using System.Xml.Serialization;
using Newtonsoft.Json;

// TODO: Add item mappings for all fields
namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a single release details item
    /// </summary>
    [JsonObject("release_details"), XmlRoot("release_details")]
    public sealed class ReleaseDetails : DatItem
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.ReleaseDetails;

        /// <summary>
        /// Id value
        /// </summary>
        /// <remarks>TODO: Is this required?</remarks>
        [JsonProperty("id", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Directory name value
        /// </summary>
        [JsonProperty("dirname", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("dirname")]
        public string? DirName { get; set; }

        /// <summary>
        /// Rom info value
        /// </summary>
        [JsonProperty("rominfo", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("rominfo")]
        public string? RomInfo { get; set; }

        /// <summary>
        /// Category value
        /// </summary>
        [JsonProperty("category", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("category")]
        public string? Category { get; set; }

        /// <summary>
        /// NFO name value
        /// </summary>
        [JsonProperty("nfoname", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("nfoname")]
        public string? NfoName { get; set; }

        /// <summary>
        /// NFO size value
        /// </summary>
        [JsonProperty("nfosize", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("nfosize")]
        public long? NfoSize { get; set; }

        /// <summary>
        /// NFO CRC value
        /// </summary>
        [JsonProperty("nfocrc", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("nfocrc")]
        public string? NfoCrc { get; set; }

        /// <summary>
        /// Archive name value
        /// </summary>
        [JsonProperty("archivename", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("archivename")]
        public string? ArchiveName { get; set; }

        /// <summary>
        /// Original format value
        /// </summary>
        [JsonProperty("originalformat", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("originalformat")]
        public string? OriginalFormat { get; set; }

        /// <summary>
        /// Date value
        /// </summary>
        [JsonProperty("date", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("date")]
        public string? Date { get; set; }

        /// <summary>
        /// Grpup value
        /// </summary>
        [JsonProperty("group", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("group")]
        public string? Group { get; set; }

        /// <summary>
        /// Comment value
        /// </summary>
        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("comment")]
        public string? Comment { get; set; }

        /// <summary>
        /// Tool value
        /// </summary>
        [JsonProperty("tool", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("tool")]
        public string? Tool { get; set; }

        /// <summary>
        /// Region value
        /// </summary>
        [JsonProperty("region", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("region")]
        public string? Region { get; set; }

        /// <summary>
        /// Origin value
        /// </summary>
        [JsonProperty("origin", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("origin")]
        public string? Origin { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty ReleaseDetails object
        /// </summary>
        public ReleaseDetails()
        {
            SetFieldValue<ItemType>(Data.Models.Metadata.DatItem.TypeKey, ItemType);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override object Clone()
        {
            var releaseDetails = new ReleaseDetails()
            {
                Id = this.Id,
                DirName = this.DirName,
                RomInfo = this.RomInfo,
                Category = this.Category,
                NfoName = this.NfoName,
                NfoSize = this.NfoSize,
                NfoCrc = this.NfoCrc,
                ArchiveName = this.ArchiveName,
                OriginalFormat = this.OriginalFormat,
                Date = this.Date,
                Group = this.Group,
                Comment = this.Comment,
                Tool = this.Tool,
                Region = this.Region,
                Origin = this.Origin,
            };
            releaseDetails.SetFieldValue<DupeType>(DatItem.DupeTypeKey, GetFieldValue<DupeType>(DatItem.DupeTypeKey));
            releaseDetails.SetFieldValue<Machine>(DatItem.MachineKey, GetMachine());
            releaseDetails.SetFieldValue<bool?>(DatItem.RemoveKey, GetBoolFieldValue(DatItem.RemoveKey));
            releaseDetails.SetFieldValue<Source?>(DatItem.SourceKey, GetFieldValue<Source?>(DatItem.SourceKey));
            releaseDetails.SetFieldValue<string?>(Data.Models.Metadata.DatItem.TypeKey, GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue());

            return releaseDetails;
        }

        #endregion

        #region Comparision Methods

        /// <inheritdoc/>
        public override bool Equals(DatItem? other)
        {
            // If we don't have a Details, return false
            if (GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey) != other?.GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey))
                return false;

            // Otherwise, treat it as a Details
            ReleaseDetails? newOther = other as ReleaseDetails;

            // If the Details information matches
            return (Id == newOther!.Id
                && DirName == newOther.DirName
                && RomInfo == newOther.RomInfo
                && Category == newOther.Category
                && NfoName == newOther.NfoName
                && NfoSize == newOther.NfoSize
                && NfoCrc == newOther.NfoCrc
                && ArchiveName == newOther.ArchiveName
                && OriginalFormat == newOther.OriginalFormat
                && Date == newOther.Date
                && Group == newOther.Group
                && Comment == newOther.Comment
                && Tool == newOther.Tool
                && Region == newOther.Region
                && Origin == newOther.Origin);
        }

        #endregion
    }
}
