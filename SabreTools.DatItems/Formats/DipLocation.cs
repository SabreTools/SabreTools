using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one diplocation
    /// </summary>
    [JsonObject("diplocation"), XmlRoot("diplocation")]
    public sealed class DipLocation : DatItem<Data.Models.Metadata.DipLocation>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.DipLocation;

        #endregion

        #region Constructors

        public DipLocation() : base() { }

        public DipLocation(Data.Models.Metadata.DipLocation item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Data.Models.Metadata.DipLocation.InvertedKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.DipLocation.InvertedKey, GetBoolFieldValue(Data.Models.Metadata.DipLocation.InvertedKey).FromYesNo());
        }

        public DipLocation(Data.Models.Metadata.DipLocation item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
