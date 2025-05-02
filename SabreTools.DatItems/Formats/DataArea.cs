﻿using System.Xml.Serialization;
using Newtonsoft.Json;
using SabreTools.Core.Tools;

namespace SabreTools.DatItems.Formats
{
    /// <summary>
    /// SoftwareList dataarea information
    /// </summary>
    /// <remarks>One DataArea can contain multiple Rom items</remarks>
    [JsonObject("dataarea"), XmlRoot("dataarea")]
    public sealed class DataArea : DatItem<Models.Metadata.DataArea>
    {
        #region Fields

        /// <inheritdoc>/>
        protected override ItemType ItemType => ItemType.DataArea;

        #endregion

        #region Constructors

        public DataArea() : base() { }

        public DataArea(Models.Metadata.DataArea item) : base(item)
        {
            // Process flag values
            if (GetStringFieldValue(Models.Metadata.DataArea.EndiannessKey) != null)
                SetFieldValue<string?>(Models.Metadata.DataArea.EndiannessKey, GetStringFieldValue(Models.Metadata.DataArea.EndiannessKey).AsEnumValue<Endianness>().AsStringValue());
            if (GetInt64FieldValue(Models.Metadata.DataArea.SizeKey) != null)
                SetFieldValue<string?>(Models.Metadata.DataArea.SizeKey, GetInt64FieldValue(Models.Metadata.DataArea.SizeKey).ToString());
            if (GetInt64FieldValue(Models.Metadata.DataArea.WidthKey) != null)
                SetFieldValue<string?>(Models.Metadata.DataArea.WidthKey, GetInt64FieldValue(Models.Metadata.DataArea.WidthKey).ToString());
        }

        #endregion
    }
}
