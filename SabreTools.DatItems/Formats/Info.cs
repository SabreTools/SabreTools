using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents special information about a machine
    /// </summary>
    [JsonObject("info"), XmlRoot("info")]
    public sealed class Info : DatItem<Data.Models.Metadata.Info>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Info;

        #endregion

        #region Constructors

        public Info() : base() { }

        public Info(Data.Models.Metadata.Info item) : base(item) { }

        public Info(Data.Models.Metadata.Info item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
