using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents the OpenMSX original value
    /// </summary>
    [JsonObject("original"), XmlRoot("original")]
    public sealed class Original
    {
        [JsonProperty("value"), XmlElement("value")]
        public bool? Value
        {
            get => _internal.ReadBool(Data.Models.Metadata.Original.ValueKey);
            set => _internal[Data.Models.Metadata.Original.ValueKey] = value;
        }

        [JsonProperty("content"), XmlElement("content")]
        public string? Content
        {
            get => _internal.ReadString(Data.Models.Metadata.Original.ContentKey);
            set => _internal[Data.Models.Metadata.Original.ContentKey] = value;
        }

        /// <summary>
        /// Internal Original model
        /// </summary>
        [JsonIgnore]
        private readonly Data.Models.Metadata.Original _internal = [];
    }
}
