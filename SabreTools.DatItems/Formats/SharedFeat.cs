using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents one shared feature object
    /// </summary>
    [JsonObject("sharedfeat"), XmlRoot("sharedfeat")]
    public sealed class SharedFeat : DatItem<Data.Models.Metadata.SharedFeat>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.SharedFeat;

        #endregion

        #region Constructors

        public SharedFeat() : base() { }

        public SharedFeat(Data.Models.Metadata.SharedFeat item) : base(item) { }

        public SharedFeat(Data.Models.Metadata.SharedFeat item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
