using System.Collections.Generic;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a MAME Listrom file
    /// </summary>
    public sealed class Listrom : SerializableDatFile<Data.Models.Listrom.MetadataFile, Serialization.Readers.Listrom, Serialization.Writers.Listrom, Serialization.CrossModel.Listrom>
    {
        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Disk,
                ItemType.Rom,
            ];

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Listrom(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.Listrom);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Data.Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Disk disk:
                    if (string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key))
                        && string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key)))
                    {
                        missingFields.Add(Data.Models.Metadata.Disk.SHA1Key);
                    }
                    break;

                case Rom rom:
                    if (rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) == null || rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) < 0)
                        missingFields.Add(Data.Models.Metadata.Rom.SizeKey);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey)))
                        missingFields.Add(Data.Models.Metadata.Rom.CRCKey);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key)))
                        missingFields.Add(Data.Models.Metadata.Rom.SHA1Key);
                    break;
            }

            return missingFields;
        }
    }
}
