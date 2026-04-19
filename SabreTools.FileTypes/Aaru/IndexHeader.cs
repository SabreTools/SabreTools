using System.IO;
using SabreTools.Numerics.Extensions;

namespace SabreTools.FileTypes.Aaru
{
    /// <summary>
    /// Header for the index, followed by entries
    /// </summary>
    /// <see href="https://github.com/aaru-dps/Aaru/blob/master/Aaru.Images/AaruFormat/Structs.cs" />
    public class IndexHeader
    {
        /// <summary>Identifier, <see cref="BlockType.Index" /></summary>
        public AaruBlockType identifier;
        /// <summary>How many entries follow this header</summary>
        public ushort entries;
        /// <summary>CRC64-ECMA of the index</summary>
        public ulong crc64;

        /// <summary>
        /// Read a stream as an IndexHeader
        /// </summary>
        /// <param name="stream">IndexHeader as a stream</param>
        /// <returns>Populated IndexHeader, null on failure</returns>
        public static IndexHeader Deserialize(Stream stream)
        {
            var indexHeader = new IndexHeader();

            indexHeader.identifier = (AaruBlockType)stream.ReadUInt32LittleEndian();
            indexHeader.entries = stream.ReadUInt16LittleEndian();
            indexHeader.crc64 = stream.ReadUInt64LittleEndian();

            return indexHeader;
        }
    }
}
