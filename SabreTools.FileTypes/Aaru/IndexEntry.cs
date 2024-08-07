﻿using System.IO;
using System.Text;

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
            IndexEntry indexEntry = new IndexEntry();

#if NET20 || NET35 || NET40
            using (var br = new BinaryReader(stream, Encoding.Default))
#else
            using (var br = new BinaryReader(stream, Encoding.Default, true))
#endif
            {
                indexEntry.blockType = (AaruBlockType)br.ReadUInt32();
                indexEntry.dataType = (AaruDataType)br.ReadUInt16();
                indexEntry.offset = br.ReadUInt64();
            }

            return indexEntry;
        }
    }
}
