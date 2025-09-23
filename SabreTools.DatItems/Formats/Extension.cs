using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents a matchable extension
    /// </summary>
    [JsonObject("extension"), XmlRoot("extension")]
    public sealed class Extension : DatItem<Models.Metadata.Extension>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Extension;

        #endregion

        #region Constructors

        public Extension() : base() { }

        public Extension(Models.Metadata.Extension item) : base(item) { }

        public Extension(Models.Metadata.Extension item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
