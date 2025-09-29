using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a single instance of another item
    /// </summary>
    [JsonObject("instance"), XmlRoot("instance")]
    public sealed class Instance : DatItem<Data.Models.Metadata.Instance>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Instance;

        #endregion

        #region Constructors

        public Instance() : base() { }

        public Instance(Data.Models.Metadata.Instance item) : base(item) { }

        public Instance(Data.Models.Metadata.Instance item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
