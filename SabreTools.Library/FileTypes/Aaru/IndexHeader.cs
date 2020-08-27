using System.IO;
using System.Text;

namespace SabreTools.Library.FileTypes.Aaru
{
    /// <summary>
    /// Header for the index, followed by entries
    /// </summary>
    /// <see cref="https://github.com/aaru-dps/Aaru/blob/master/Aaru.Images/AaruFormat/Structs.cs" />
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
            IndexHeader indexHeader = new IndexHeader();

            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                indexHeader.identifier = (AaruBlockType)br.ReadUInt32();
                indexHeader.entries = br.ReadUInt16();
                indexHeader.crc64 = br.ReadUInt64();
            }

            return indexHeader;
        }
    }
}
