using System.Collections.Generic;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents an OfflineList XML DAT
    /// </summary>
    public sealed class OfflineList : SerializableDatFile<Data.Models.OfflineList.Dat, Serialization.Readers.OfflineList, Serialization.Writers.OfflineList, Serialization.CrossModel.OfflineList>
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public OfflineList(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue(DatHeader.DatFormatKey, DatFormat.OfflineList);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

#pragma warning disable IDE0010
            switch (datItem)
            {
                case Rom rom:
                    if (rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) is null || rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) < 0)
                        missingFields.Add(Data.Models.Metadata.Rom.SizeKey);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey)))
                        missingFields.Add(Data.Models.Metadata.Rom.CRCKey);
                    break;
            }
#pragma warning restore IDE0010

            return missingFields;
        }
    }
}
