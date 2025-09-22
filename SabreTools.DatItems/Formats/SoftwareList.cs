using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// Represents which SoftwareList(s) is associated with a set
    /// </summary>
    [JsonObject("softwarelist"), XmlRoot("softwarelist")]
    public sealed class SoftwareList : DatItem<Models.Metadata.SoftwareList>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.SoftwareList;

        #endregion

        #region Constructors

        public SoftwareList() : base() { }

        public SoftwareList(Models.Metadata.SoftwareList item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Models.Metadata.SoftwareList.StatusKey) != null)
                SetFieldValue<string?>(Models.Metadata.SoftwareList.StatusKey, GetStringFieldValue(Models.Metadata.SoftwareList.StatusKey).AsSoftwareListStatus().AsStringValue());

            // Handle subitems
            // TODO: Handle the Software subitem
        }

        #endregion
    }
}
