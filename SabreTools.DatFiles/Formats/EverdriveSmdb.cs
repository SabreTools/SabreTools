using System.Collections.Generic;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents parsing and writing of an Everdrive SMDB file
    /// </summary>
    public sealed class EverdriveSMDB : SerializableDatFile<Data.Models.EverdriveSMDB.MetadataFile, Serialization.Readers.EverdriveSMDB, Serialization.Writers.EverdriveSMDB, Serialization.CrossModel.EverdriveSMDB>
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
        public EverdriveSMDB(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.EverdriveSMDB);
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
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key)))
                        missingFields.Add(Data.Models.Metadata.Rom.SHA256Key);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key)))
                        missingFields.Add(Data.Models.Metadata.Rom.SHA1Key);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key)))
                        missingFields.Add(Data.Models.Metadata.Rom.MD5Key);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey)))
                        missingFields.Add(Data.Models.Metadata.Rom.CRCKey);
                    break;
            }

            return missingFields;
        }
    }
}
