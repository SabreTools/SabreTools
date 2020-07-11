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
            public uint offset; // starting offset within the file
        }

        /// <summary>
        /// Compressed map header format
        /// </summary>
        private class CompressedMapHeader
        {
            public uint length;                         // length of compressed map
            public byte[] datastart = new byte[12];     // UINT48; offset of first block
            public ushort crc;                          // crc-16 of the map
            public byte lengthbits;                     // bits used to encode complength
            public byte hunkbits;                       // bits used to encode self-refs
            public byte parentunitbits;                 // bits used to encode parent unit refs
            public byte reserved;                       // future use
        }

        /// <summary>
        /// Compressed map entry format
        /// </summary>
        private class CompressedMapEntry
        {
            public byte compression;                   // compression type
            public byte[] complength = new byte[6];    // UINT24; compressed length
            public byte[] offset = new byte[12];       // UINT48; offset
            public ushort crc;                         // crc-16 of the data
        }

        public const int HeaderSize = 124;
        public const uint Version = 5;

        // V5-specific header values
        public uint[] compressors = new uint[4];   // which custom compressors are used?
        public ulong logicalbytes;                 // logical size of the data (in bytes)
        public ulong mapoffset;                    // offset to the map
        public ulong metaoffset;                   // offset to the first blob of metadata
        public uint hunkbytes;                     // number of bytes per hunk
        public uint unitbytes;                     // number of bytes per unit within each hunk
        public byte[] rawsha1 = new byte[20];      // raw data SHA1
        public byte[] sha1 = new byte[20];         // combined raw+meta SHA1
        public byte[] parentsha1 = new byte[20];   // combined raw+meta SHA1 of parent

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
