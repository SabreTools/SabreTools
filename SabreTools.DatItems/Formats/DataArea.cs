using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// SoftwareList dataarea information
    /// </summary>
    /// <remarks>One DataArea can contain multiple Rom items</remarks>
    [JsonObject("dataarea"), XmlRoot("dataarea")]
    public sealed class DataArea : DatItem<Data.Models.Metadata.DataArea>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.DataArea;

        #endregion

        #region Constructors

        public DataArea() : base() { }

        public DataArea(Data.Models.Metadata.DataArea item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Data.Models.Metadata.DataArea.EndiannessKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.DataArea.EndiannessKey, GetStringFieldValue(Data.Models.Metadata.DataArea.EndiannessKey).AsEndianness().AsStringValue());
            if (GetInt64FieldValue(Data.Models.Metadata.DataArea.SizeKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.DataArea.SizeKey, GetInt64FieldValue(Data.Models.Metadata.DataArea.SizeKey).ToString());
            if (GetInt64FieldValue(Data.Models.Metadata.DataArea.WidthKey) != null)
                SetFieldValue<string?>(Data.Models.Metadata.DataArea.WidthKey, GetInt64FieldValue(Data.Models.Metadata.DataArea.WidthKey).ToString());
        }

        public DataArea(Data.Models.Metadata.DataArea item, Machine machine, Source source) : this(item)
        {
            SetFieldValue<Source?>(DatItem.SourceKey, source);
            CopyMachineInformation(machine);
        }

        #endregion
    }
}
