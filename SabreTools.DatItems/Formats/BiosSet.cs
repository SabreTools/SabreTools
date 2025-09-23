using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which BIOS(es) is associated with a set
    /// </summary>
    [JsonObject("biosset"), XmlRoot("biosset")]
    public sealed class BiosSet : DatItem<Models.Metadata.BiosSet>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.BiosSet;

        #endregion

        #region Constructors

        public BiosSet() : base() { }

        public BiosSet(Models.Metadata.BiosSet item) : base(item)
        {
            // Process flag values
            if (GetBoolFieldValue(Models.Metadata.BiosSet.DefaultKey) != null)
                SetFieldValue<string?>(Models.Metadata.BiosSet.DefaultKey, GetBoolFieldValue(Models.Metadata.BiosSet.DefaultKey).FromYesNo());
        }

        public BiosSet(Models.Metadata.BiosSet item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
