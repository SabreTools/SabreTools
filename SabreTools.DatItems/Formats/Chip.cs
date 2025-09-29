using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which Chip(s) is associated with a set
    /// </summary>
    [JsonObject("chip"), XmlRoot("chip")]
    public sealed class Chip : DatItem<Data.Models.Metadata.Chip>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Chip;

        #endregion

        #region Constructors

        public Chip() : base() { }

        public Chip(Data.Models.Metadata.Chip item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Data.Models.Metadata.Chip.SoundOnlyKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Chip.SoundOnlyKey, GetBoolFieldValue(Data.Models.Metadata.Chip.SoundOnlyKey).FromYesNo());
            if (GetStringFieldValue(Data.Models.Metadata.Chip.ChipTypeKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Chip.ChipTypeKey, GetStringFieldValue(Data.Models.Metadata.Chip.ChipTypeKey).AsChipType().AsStringValue());
        }

        public Chip(Data.Models.Metadata.Chip item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
