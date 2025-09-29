using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which RAM option(s) is associated with a set
    /// </summary>
    [JsonObject("ramoption"), XmlRoot("ramoption")]
    public sealed class RamOption : DatItem<Data.Models.Metadata.RamOption>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.RamOption;

        #endregion

        #region Constructors

        public RamOption() : base() { }

        public RamOption(Data.Models.Metadata.RamOption item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Data.Models.Metadata.RamOption.DefaultKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.RamOption.DefaultKey, GetBoolFieldValue(Data.Models.Metadata.RamOption.DefaultKey).FromYesNo());
        }

        public RamOption(Data.Models.Metadata.RamOption item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
