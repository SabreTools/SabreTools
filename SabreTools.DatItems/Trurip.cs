using System;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems
{
    /// <summary>
    /// Represents TruRip/EmuArc-specific values on a machine
    /// </summary>
    [JsonObject("trurip"), XmlRoot("trurip")]
    public sealed class Trurip : ICloneable
    {
        #region Fields

        /// <summary>
        /// Title ID
        /// </summary>
        [JsonProperty("titleid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("titleid")]
        public string? TitleID { get; set; } = null;

        /// <summary>
        /// Machine developer
        /// </summary>
        [JsonProperty("developer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("developer")]
        public string? Developer { get; set; } = null;

        /// <summary>
        /// Game genre
        /// </summary>
        [JsonProperty("genre", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("genre")]
        public string? Genre { get; set; } = null;

        /// <summary>
        /// Game subgenre
        /// </summary>
        [JsonProperty("subgenre", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("subgenre")]
        public string? Subgenre { get; set; } = null;

        /// <summary>
        /// Game ratings
        /// </summary>
        [JsonProperty("ratings", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("ratings")]
        public string? Ratings { get; set; } = null;

        /// <summary>
        /// Game score
        /// </summary>
        [JsonProperty("score", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("score")]
        public string? Score { get; set; } = null;

        /// <summary>
        /// Is the machine enabled
        /// </summary>
        [JsonProperty("enabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("enabled")]
        public string? Enabled { get; set; } = null; // bool?

        /// <summary>
        /// Does the game have a CRC check
        /// </summary>
        [JsonProperty("hascrc", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("hascrc")]
        public bool? Crc { get; set; } = null;

        [JsonIgnore]
        public bool CrcSpecified { get { return Crc != null; } }

        /// <summary>
        /// Machine relations
        /// </summary>
        [JsonProperty("relatedto", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("relatedto")]
        public string? RelatedTo { get; set; } = null;

        #endregion

        #region Constructors

        public Trurip() { }

        public Trurip(Data.Models.Logiqx.Trurip trurip)
        {
            TitleID = trurip.TitleID;
            Developer = trurip.Developer;
            Genre = trurip.Genre;
            Subgenre = trurip.Subgenre;
            Ratings = trurip.Ratings;
            Score = trurip.Score;
            Enabled = trurip.Enabled;
            Crc = trurip.CRC.AsYesNo();
            RelatedTo = trurip.RelatedTo;
        }

        #endregion

        #region Cloning methods

        /// <summary>
        /// Create a clone of the current object
        /// </summary>
        /// <returns>New object with the same values as the current one</returns>
        public object Clone()
        {
            return new Trurip()
            {
                TitleID = this.TitleID,
                Developer = this.Developer,
                Genre = this.Genre,
                Subgenre = this.Subgenre,
                Ratings = this.Ratings,
                Score = this.Score,
                Enabled = this.Enabled,
                Crc = this.Crc,
                RelatedTo = this.RelatedTo,
            };
        }

        /// <summary>
        /// Convert to the internal Logiqx model
        /// </summary>
        public Data.Models.Logiqx.Trurip ConvertToLogiqx()
        {
            return new Data.Models.Logiqx.Trurip()
            {
                TitleID = this.TitleID,
                Developer = this.Developer,
                Genre = this.Genre,
                Subgenre = this.Subgenre,
                Ratings = this.Ratings,
                Score = this.Score,
                Enabled = this.Enabled,
                CRC = this.Crc.FromYesNo(),
                RelatedTo = this.RelatedTo,
            };
        }

        #endregion
    }
}
