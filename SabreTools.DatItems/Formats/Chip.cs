using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which Chip(s) is associated with a set
    /// </summary>
    [JsonObject("chip"), XmlRoot("chip")]
    public sealed class Chip : DatItem<Models.Metadata.Chip>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Chip;

        #endregion

        #region Constructors

        public Chip() : base() { }

        public Chip(Models.Metadata.Chip item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.Chip.SoundOnlyKey) != null)
                SetFieldValue<string?>(Models.Metadata.Chip.SoundOnlyKey, GetBoolFieldValue(Models.Metadata.Chip.SoundOnlyKey).FromYesNo());
            if (GetStringFieldValue(Models.Metadata.Chip.ChipTypeKey) != null)
                SetFieldValue<string?>(Models.Metadata.Chip.ChipTypeKey, GetStringFieldValue(Models.Metadata.Chip.ChipTypeKey).AsChipType().AsStringValue());
        }

        public Chip(Models.Metadata.Chip item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
