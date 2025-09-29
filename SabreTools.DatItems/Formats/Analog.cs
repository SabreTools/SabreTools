using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a single analog item
    /// </summary>
    [JsonObject("analog"), XmlRoot("analog")]
    public sealed class Analog : DatItem<Data.Models.Metadata.Analog>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Analog;

        #endregion

        #region Constructors

        public Analog() : base() { }

        public Analog(Data.Models.Metadata.Analog item) : base(item) { }

        public Analog(Data.Models.Metadata.Analog item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
