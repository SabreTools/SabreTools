using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one diplocation
    /// </summary>
    [JsonObject("diplocation"), XmlRoot("diplocation")]
    public sealed class DipLocation : DatItem<Models.Metadata.DipLocation>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.DipLocation;

        #endregion

        #region Constructors

        public DipLocation() : base() { }

        public DipLocation(Models.Metadata.DipLocation item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.DipLocation.InvertedKey) != null)
                SetFieldValue<string?>(Models.Metadata.DipLocation.InvertedKey, GetBoolFieldValue(Models.Metadata.DipLocation.InvertedKey).FromYesNo());
        }

        #endregion
    }
}
