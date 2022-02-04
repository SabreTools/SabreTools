using System;
using System.Xml.Serialization;

using SabreTools.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SabreTools.DatItems
{
    /// <summary>
    /// Represents the information specific to a set/game/machine
    /// </summary>
    [JsonObject("machine"), XmlRoot("machine")]
    public class Machine : ICloneable
    {
        #region Fields

        #region Common

        /// <summary>
        /// Name of the machine
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Include)]
        [XmlElement("name")]
        public string Name { get; set; } = null;

        /// <summary>
        /// Additional notes
        /// </summary>
        /// <remarks>Known as "Extra" in AttractMode</remarks>
        [JsonProperty("comment", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("comment")]
        public string Comment { get; set; } = null;

        /// <summary>
        /// Extended description
        /// </summary>
        [JsonProperty("description", DefaultValueHandling = DefaultValueHandling.Include)]
        [XmlElement("description")]
        public string Description { get; set; } = null;

        /// <summary>
        /// Year(s) of release/manufacture
        /// </summary>
        [JsonProperty("year", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("year")]
        public string Year { get; set; } = null;

        /// <summary>
        /// Manufacturer, if available
        /// </summary>
        [JsonProperty("manufacturer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("manufacturer")]
        public string Manufacturer { get; set; } = null;

        /// <summary>
        /// Publisher, if available
        /// </summary>
        [JsonProperty("publisher", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("publisher")]
        public string Publisher { get; set; } = null;

        /// <summary>
        /// Category, if available
        /// </summary>
        [JsonProperty("category", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("category")]
        public string Category { get; set; } = null;

        /// <summary>
        /// romof parent
        /// </summary>
        [JsonProperty("romof", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("romof")]
        public string RomOf { get; set; } = null;

        /// <summary>
        /// cloneof parent
        /// </summary>
        [JsonProperty("cloneof", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("cloneof")]
        public string CloneOf { get; set; } = null;

        /// <summary>
        /// sampleof parent
        /// </summary>
        [JsonProperty("sampleof", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("sampleof")]
        public string SampleOf { get; set; } = null;

        /// <summary>
        /// Type of the machine
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        [XmlElement("type")]
        public MachineType MachineType { get; set; } = 0x0;

        [JsonIgnore]
        public bool MachineTypeSpecified { get { return MachineType != 0x0 && MachineType != MachineType.NULL; } }

        #endregion

        #region AttractMode

        /// <summary>
        /// Player count
        /// </summary>
        /// <remarks>Also in Logiqx EmuArc</remarks>
        [JsonProperty("players", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("players")]
        public string Players { get; set; } = null;

        /// <summary>
        /// Screen rotation
        /// </summary>
        [JsonProperty("rotation", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("rotation")]
        public string Rotation { get; set; } = null;

        /// <summary>
        /// Control method
        /// </summary>
        [JsonProperty("control", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("control")]
        public string Control { get; set; } = null;

        /// <summary>
        /// Support status
        /// </summary>
        [JsonProperty("status", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("status")]
        public string Status { get; set; } = null;

        /// <summary>
        /// Display count
        /// </summary>
        [JsonProperty("displaycount", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("displaycount")]
        public string DisplayCount { get; set; } = null;

        /// <summary>
        /// Display type
        /// </summary>
        [JsonProperty("displaytype", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("displaytype")]
        public string DisplayType { get; set; } = null;

        /// <summary>
        /// Number of input buttons
        /// </summary>
        [JsonProperty("buttons", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("buttons")]
        public string Buttons { get; set; } = null;

        #endregion

        #region ListXML

        /// <summary>
        /// History.dat entry for the machine
        /// </summary>
        [JsonProperty("history", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("history")]
        public string History { get; set; } = null;

        /// <summary>
        /// Emulator source file related to the machine
        /// </summary>
        /// <remarks>Also in Logiqx</remarks>
        [JsonProperty("sourcefile", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("sourcefile")]
        public string SourceFile { get; set; } = null;

        /// <summary>
        /// Machine runnable status
        /// </summary>
        /// <remarks>Also in Logiqx</remarks>
        [JsonProperty("runnable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("runnable")]
        public Runnable Runnable { get; set; } = Runnable.NULL;

        [JsonIgnore]
        public bool RunnableSpecified { get { return Runnable != Runnable.NULL; } }

        #endregion

        #region Logiqx

        /// <summary>
        /// Machine board name
        /// </summary>
        [JsonProperty("board", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("board")]
        public string Board { get; set; } = null;

        /// <summary>
        /// Rebuild location if different than machine name
        /// </summary>
        [JsonProperty("rebuildto", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("rebuildto")]
        public string RebuildTo { get; set; } = null;

        #endregion

        // TODO: Should this be a separate object for TruRip?
        #region Logiqx EmuArc

        /// <summary>
        /// Title ID
        /// </summary>
        [JsonProperty("titleid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("titleid")]
        public string TitleID { get; set; } = null;

        /// <summary>
        /// Machine developer
        /// </summary>
        [JsonProperty("developer", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("developer")]
        public string Developer { get; set; } = null;

        /// <summary>
        /// Game genre
        /// </summary>
        [JsonProperty("genre", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("genre")]
        public string Genre { get; set; } = null;

        /// <summary>
        /// Game subgenre
        /// </summary>
        [JsonProperty("subgenre", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("subgenre")]
        public string Subgenre { get; set; } = null;

        /// <summary>
        /// Game ratings
        /// </summary>
        [JsonProperty("ratings", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("ratings")]
        public string Ratings { get; set; } = null;

        /// <summary>
        /// Game score
        /// </summary>
        [JsonProperty("score", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("score")]
        public string Score { get; set; } = null;

        /// <summary>
        /// Is the machine enabled
        /// </summary>
        [JsonProperty("enabled", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("enabled")]
        public string Enabled { get; set; } = null; // bool?

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
        public string RelatedTo { get; set; } = null;

        #endregion

        #region OpenMSX

        /// <summary>
        /// Generation MSX ID
        /// </summary>
        [JsonProperty("genmsxid", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("genmsxid")]
        public string GenMSXID { get; set; } = null;

        /// <summary>
        /// MSX System
        /// </summary>
        [JsonProperty("system", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("system")]
        public string System { get; set; } = null;

        /// <summary>
        /// Machine country of origin
        /// </summary>
        [JsonProperty("country", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("country")]
        public string Country { get; set; } = null;

        #endregion

        #region SoftwareList

        /// <summary>
        /// Support status
        /// </summary>
        [JsonProperty("supported", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [XmlElement("supported")]
        public Supported Supported { get; set; } = Supported.NULL;

        [JsonIgnore]
        public bool SupportedSpecified { get { return Supported != Supported.NULL; } }

        #endregion

        #endregion // Fields

        #region Constructors

        /// <summary>
        /// Create a new Machine object
        /// </summary>
        public Machine()
        {
        }

        /// <summary>
        /// Create a new Machine object with the included information
        /// </summary>
        /// <param name="name">Name of the machine</param>
        /// <param name="description">Description of the machine</param>
        public Machine(string name, string description)
        {
            Name = name;
            Description = description;
        }

        #endregion

        #region Cloning methods

        /// <summary>
        /// Create a clone of the current machine
        /// </summary>
        /// <returns>New machine with the same values as the current one</returns>
        public object Clone()
        {
            return new Machine()
            {
                #region Common

                Name = this.Name,
                Comment = this.Comment,
                Description = this.Description,
                Year = this.Year,
                Manufacturer = this.Manufacturer,
                Publisher = this.Publisher,
                Category = this.Category,
                RomOf = this.RomOf,
                CloneOf = this.CloneOf,
                SampleOf = this.SampleOf,
                MachineType = this.MachineType,

                #endregion

                #region AttractMode

                Players = this.Players,
                Rotation = this.Rotation,
                Control = this.Control,
                Status = this.Status,
                DisplayCount = this.DisplayCount,
                DisplayType = this.DisplayType,
                Buttons = this.Buttons,

                #endregion

                #region ListXML

                History = this.History,
                SourceFile = this.SourceFile,
                Runnable = this.Runnable,

                #endregion

                #region Logiqx

                Board = this.Board,
                RebuildTo = this.RebuildTo,

                #endregion

                #region Logiqx EmuArc

                TitleID = this.TitleID,
                Developer = this.Developer,
                Genre = this.Genre,
                Subgenre = this.Subgenre,
                Ratings = this.Ratings,
                Score = this.Score,
                Enabled = this.Enabled,
                Crc = this.Crc,
                RelatedTo = this.RelatedTo,

                #endregion

                #region OpenMSX

                GenMSXID = this.GenMSXID,
                System = this.System,
                Country = this.Country,

                #endregion

                #region SoftwareList

                Supported = this.Supported,

                #endregion
            };
        }

        #endregion
    }
}
