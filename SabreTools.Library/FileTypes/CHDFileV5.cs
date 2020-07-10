using System.IO;
using System.Text;

using SabreTools.Library.Tools;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// CHD V5 File
    /// </summary>
    public class CHDFileV5 : CHDFile
    {
        /// <summary>
        /// Uncompressed map format
        /// </summary>
        private class UncompressedMap
        {
            private uint offset;    // starting offset within the file
        }

        /// <summary>
        /// Compressed map header format
        /// </summary>
        private class CompressedMapHeader
        {
            private uint length;                        // length of compressed map
            private byte[] datastart = new byte[12];    // UINT48; offset of first block
            private ushort crc;                         // crc-16 of the map
            private byte lengthbits;                    // bits used to encode complength
            private byte hunkbits;                      // bits used to encode self-refs
            private byte parentunitbits;                // bits used to encode parent unit refs
            private byte reserved;                      // future use
        }

        /// <summary>
        /// Compressed map entry format
        /// </summary>
        private class CompressedMapEntry
        {
            private byte compression;                   // compression type
            private byte[] complength = new byte[6];    // UINT24; compressed length
            private byte[] offset = new byte[12];       // UINT48; offset
            private ushort crc;                         // crc-16 of the data
        }

        public const int HeaderSize = 124;
        public const uint Version = 5;

        // V5-specific header values
        private uint[] compressors = new uint[4];   // which custom compressors are used?
        private ulong logicalbytes;                 // logical size of the data (in bytes)
        private ulong mapoffset;                    // offset to the map
        private ulong metaoffset;                   // offset to the first blob of metadata
        private uint hunkbytes;                     // number of bytes per hunk
        private uint unitbytes;                     // number of bytes per unit within each hunk
        private byte[] rawsha1 = new byte[20];      // raw data SHA1
        private byte[] sha1 = new byte[20];         // combined raw+meta SHA1
        private byte[] parentsha1 = new byte[20];   // combined raw+meta SHA1 of parent

        /// <summary>
        /// Parse and validate the header as if it's V5
        /// </summary>
        public static CHDFileV5 Deserialize(Stream stream)
        {
            CHDFileV5 chd = new CHDFileV5();

            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                chd.tag = br.ReadCharsBigEndian(8);
                chd.length = br.ReadUInt32BigEndian();
                chd.version = br.ReadUInt32BigEndian();
                chd.compressors = new uint[4];
                for (int i = 0; i < 4; i++)
                {
                    chd.compressors[i] = br.ReadUInt32BigEndian();
                }
                chd.logicalbytes = br.ReadUInt64BigEndian();
                chd.mapoffset = br.ReadUInt64BigEndian();
                chd.metaoffset = br.ReadUInt64BigEndian();
                chd.hunkbytes = br.ReadUInt32BigEndian();
                chd.unitbytes = br.ReadUInt32BigEndian();
                chd.rawsha1 = br.ReadBytesBigEndian(20);
                chd.sha1 = br.ReadBytesBigEndian(20);
                chd.parentsha1 = br.ReadBytesBigEndian(20);
            }

            return chd;
        }

        /// <summary>
        /// Return internal SHA1 hash
        /// </summary>
        public override byte[] GetHash()
        {
            return sha1;
        }
    }
}
