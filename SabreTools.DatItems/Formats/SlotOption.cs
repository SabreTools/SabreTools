using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one ListXML slotoption
    /// </summary>
    [JsonObject("slotoption"), XmlRoot("slotoption")]
    public sealed class SlotOption : DatItem<Data.Models.Metadata.SlotOption>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.SlotOption;

        #endregion

        #region Constructors

        public SlotOption() : base() { }

        public SlotOption(Data.Models.Metadata.SlotOption item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Data.Models.Metadata.SlotOption.DefaultKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.SlotOption.DefaultKey, GetBoolFieldValue(Data.Models.Metadata.SlotOption.DefaultKey).FromYesNo());
        }

        public SlotOption(Data.Models.Metadata.SlotOption item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
