using SabreTools.DatItems;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a Archive.org file list
    /// </summary>
    public sealed class ArchiveDotOrg : SerializableDatFile<Data.Models.ArchiveDotOrg.Files, Serialization.Readers.ArchiveDotOrg, Serialization.Writers.ArchiveDotOrg, Serialization.CrossModel.ArchiveDotOrg>
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
        public ArchiveDotOrg(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.ArchiveDotOrg);
        }
    }
}
