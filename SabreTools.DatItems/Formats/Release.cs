using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents release information about a set
    /// </summary>
    [JsonObject("release"), XmlRoot("release")]
    public sealed class Release : DatItem<Data.Models.Metadata.Release>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.Release;

        #endregion

        #region Constructors

        public Release() : base() { }

        public Release(Data.Models.Metadata.Release item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Data.Models.Metadata.Release.DefaultKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.Release.DefaultKey, GetBoolFieldValue(Data.Models.Metadata.Release.DefaultKey).FromYesNo());
        }

        public Release(Data.Models.Metadata.Release item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
