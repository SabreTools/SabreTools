﻿using SabreTools.DatItems;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a Archive.org file list
    /// </summary>
    internal sealed class ArchiveDotOrg : SerializableDatFile<Models.ArchiveDotOrg.Files, Serialization.Deserializers.ArchiveDotOrg, Serialization.Serializers.ArchiveDotOrg, Serialization.CrossModel.ArchiveDotOrg>
    {
        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public ArchiveDotOrg(DatFile? datFile) : base(datFile)
        {
        }

        /// <inheritdoc/>
        protected override ItemType[] GetSupportedTypes()
        {
            return
            [
                ItemType.Rom,
            ];
        }
    }
}
