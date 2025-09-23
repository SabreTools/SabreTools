using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// SoftwareList diskarea information
    /// </summary>
    /// <remarks>One DiskArea can contain multiple Disk items</remarks>
    [JsonObject("diskarea"), XmlRoot("diskarea")]
    public sealed class DiskArea : DatItem<Models.Metadata.DiskArea>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.DiskArea;

        #endregion

        #region Constructors

        public DiskArea() : base() { }

        public DiskArea(Models.Metadata.DiskArea item) : base(item) { }

        public DiskArea(Models.Metadata.DiskArea item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
