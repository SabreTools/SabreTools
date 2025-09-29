using System.Collections.Generic;
using SabreTools.DatItems;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents an AttractMode DAT
    /// </summary>
    public sealed class AttractMode : SerializableDatFile<Data.Models.AttractMode.MetadataFile, Serialization.Readers.AttractMode, Serialization.Writers.AttractMode, Serialization.CrossModel.AttractMode>
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
        public AttractMode(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.AttractMode);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Data.Models.Metadata.Rom.NameKey);

            return missingFields;
        }
    }
}
