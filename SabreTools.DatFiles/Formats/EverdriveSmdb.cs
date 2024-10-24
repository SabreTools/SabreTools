﻿using System.Collections.Generic;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents parsing and writing of an Everdrive SMDB file
    /// </summary>
    internal sealed class EverdriveSMDB : SerializableDatFile<Models.EverdriveSMDB.MetadataFile, Serialization.Deserializers.EverdriveSMDB, Serialization.Serializers.EverdriveSMDB, Serialization.CrossModel.EverdriveSMDB>
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public EverdriveSMDB(DatFile? datFile) : base(datFile)
        {
        }

        /// <inheritdoc/>
        protected override ItemType[] GetSupportedTypes()
        {
            return
            [
                ItemType.Rom
            ];
        }

        /// <inheritdoc/>
        protected override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            var missingFields = new List<string>();

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key)))
                        missingFields.Add(Models.Metadata.Rom.SHA256Key);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key)))
                        missingFields.Add(Models.Metadata.Rom.SHA1Key);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key)))
                        missingFields.Add(Models.Metadata.Rom.MD5Key);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey)))
                        missingFields.Add(Models.Metadata.Rom.CRCKey);
                    break;
            }

            return missingFields;
        }
    }
}
