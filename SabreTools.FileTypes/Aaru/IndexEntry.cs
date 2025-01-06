﻿using System.IO;
using SabreTools.IO.Extensions;

namespace SabreTools.FileTypes.Aaru
{
    /// <summary>
    /// Index entry
    /// </summary>
    /// <see cref="https://github.com/aaru-dps/Aaru/blob/master/Aaru.Images/AaruFormat/Structs.cs" />
    public class IndexEntry
    {
        /// <summary>Type of item pointed by this entry</summary>
        public AaruBlockType blockType;
        /// <summary>Type of data contained by the block pointed by this entry</summary>
        public AaruDataType dataType;
        /// <summary>Offset in file where item is stored</summary>
        public ulong offset;

        /// <summary>
        /// Read a stream as an IndexHeader
        /// </summary>
        /// <param name="stream">IndexHeader as a stream</param>
        /// <returns>Populated IndexHeader, null on failure</returns>
        public static IndexEntry Deserialize(Stream stream)
        {
            var indexEntry = new IndexEntry();

            indexEntry.blockType = (AaruBlockType)stream.ReadUInt32();
            indexEntry.dataType = (AaruDataType)stream.ReadUInt16();
            indexEntry.offset = stream.ReadUInt64();

            return indexEntry;
        }
    }
}
