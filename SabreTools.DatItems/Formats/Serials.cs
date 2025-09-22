using System.Xml.Serialization;
using Newtonsoft.Json;

// TODO: Add item mappings for all fields
namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a single serials item
    /// </summary>
    [JsonObject("serials"), XmlRoot("serials")]
    public sealed class Serials : DatItem
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Serials;

        /// <summary>
        /// Digital serial 1 value
        /// </summary>
        [JsonProperty("digital_serial1", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("digital_serial1")]
        public string? DigitalSerial1 { get; set; }

        /// <summary>
        /// Digital serial 2 value
        /// </summary>
        [JsonProperty("digital_serial2", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("digital_serial2")]
        public string? DigitalSerial2 { get; set; }

        /// <summary>
        /// Media serial 1 value
        /// </summary>
        [JsonProperty("media_serial1", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("media_serial1")]
        public string? MediaSerial1 { get; set; }

        /// <summary>
        /// Media serial 2 value
        /// </summary>
        [JsonProperty("media_serial2", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("media_serial2")]
        public string? MediaSerial2 { get; set; }

        /// <summary>
        /// Media serial 3 value
        /// </summary>
        [JsonProperty("media_serial3", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("media_serial3")]
        public string? MediaSerial3 { get; set; }

        /// <summary>
        /// PCB serial value
        /// </summary>
        [JsonProperty("pcb_serial", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("pcb_serial")]
        public string? PcbSerial { get; set; }

        /// <summary>
        /// Rom chip serial 1 value
        /// </summary>
        [JsonProperty("romchip_serial1", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("romchip_serial1")]
        public string? RomChipSerial1 { get; set; }

        /// <summary>
        /// Rom chip serial 2 value
        /// </summary>
        [JsonProperty("romchip_serial2", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("romchip_serial2")]
        public string? RomChipSerial2 { get; set; }

        /// <summary>
        /// Lockout serial value
        /// </summary>
        [JsonProperty("lockout_serial", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("lockout_serial")]
        public string? LockoutSerial { get; set; }

        /// <summary>
        /// Save chip serial value
        /// </summary>
        [JsonProperty("savechip_serial", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("savechip_serial")]
        public string? SaveChipSerial { get; set; }

        /// <summary>
        /// Chip serial value
        /// </summary>
        [JsonProperty("chip_serial", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("chip_serial")]
        public string? ChipSerial { get; set; }

        /// <summary>
        /// Box serial value
        /// </summary>
        [JsonProperty("box_serial", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("box_serial")]
        public string? BoxSerial { get; set; }

        /// <summary>
        /// Media stamp value
        /// </summary>
        [JsonProperty("mediastamp", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("mediastamp")]
        public string? MediaStamp { get; set; }

        /// <summary>
        /// Box barcode value
        /// </summary>
        [JsonProperty("box_barcode", DefaultValueHandling = DefaultValueHandling.Ignore), XmlElement("box_barcode")]
        public string? BoxBarcode { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a default, empty Serials object
        /// </summary>
        public Serials()
        {
            SetFieldValue<ItemType>(Models.Metadata.DatItem.TypeKey, ItemType);
        }

        #endregion

        #region Cloning Methods

        /// <inheritdoc/>
        public override object Clone()
        {
            var serials = new Serials()
            {
                DigitalSerial1 = this.DigitalSerial1,
                DigitalSerial2 = this.DigitalSerial2,
                MediaSerial1 = this.MediaSerial1,
                MediaSerial2 = this.MediaSerial2,
                MediaSerial3 = this.MediaSerial3,
                PcbSerial = this.PcbSerial,
                RomChipSerial1 = this.RomChipSerial1,
                RomChipSerial2 = this.RomChipSerial2,
                LockoutSerial = this.LockoutSerial,
                SaveChipSerial = this.SaveChipSerial,
                ChipSerial = this.ChipSerial,
                BoxSerial = this.BoxSerial,
                MediaStamp = this.MediaStamp,
                BoxBarcode = this.BoxBarcode,
            };
            serials.SetFieldValue<DupeType>(DatItem.DupeTypeKey, GetFieldValue<DupeType>(DatItem.DupeTypeKey));
            serials.SetFieldValue<Machine>(DatItem.MachineKey, GetMachine());
            serials.SetFieldValue<bool?>(DatItem.RemoveKey, GetBoolFieldValue(DatItem.RemoveKey));
            serials.SetFieldValue<Source?>(DatItem.SourceKey, GetFieldValue<Source?>(DatItem.SourceKey));
            serials.SetFieldValue<string?>(Models.Metadata.DatItem.TypeKey, GetStringFieldValue(Models.Metadata.DatItem.TypeKey).AsItemType().AsStringValue());

            return serials;
        }

        #endregion

        #region Comparision Methods

        /// <inheritdoc/>
        public override bool Equals(DatItem? other)
        {
            // If we don't have a Serials, return false
            if (GetStringFieldValue(Models.Metadata.DatItem.TypeKey) != other?.GetStringFieldValue(Models.Metadata.DatItem.TypeKey))
                return false;

            // Otherwise, treat it as a Serials
            Serials? newOther = other as Serials;

            // If the Serials information matches
            return (DigitalSerial1 == newOther!.DigitalSerial1
                && DigitalSerial2 == newOther.DigitalSerial2
                && MediaSerial1 == newOther.MediaSerial1
                && MediaSerial2 == newOther.MediaSerial2
                && MediaSerial3 == newOther.MediaSerial3
                && PcbSerial == newOther.PcbSerial
                && RomChipSerial1 == newOther.RomChipSerial1
                && RomChipSerial2 == newOther.RomChipSerial2
                && LockoutSerial == newOther.LockoutSerial
                && SaveChipSerial == newOther.SaveChipSerial
                && ChipSerial == newOther.ChipSerial
                && BoxSerial == newOther.BoxSerial
                && MediaStamp == newOther.MediaStamp
                && BoxBarcode == newOther.BoxBarcode);
        }

        #endregion
    }
}
