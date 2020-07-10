using System;
using System.IO;
using System.Text;

using SabreTools.Library.Tools;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// CHD V4 File
    /// </summary>
    public class CHDFileV4 : CHDFile
    {
        /// <summary>
        /// CHD flags
        /// </summary>
        [Flags]
        private enum Flags : uint
        {
            DriveHasParent = 0x00000001,
            DriveAllowsWrites = 0x00000002,
        }

        /// <summary>
        /// Compression being used in CHD
        /// </summary>
        private enum Compression : uint
        {
            CHDCOMPRESSION_NONE = 0,
            CHDCOMPRESSION_ZLIB = 1,
            CHDCOMPRESSION_ZLIB_PLUS = 2,
            CHDCOMPRESSION_AV = 3,
        }

        /// <summary>
        /// Map format
        /// </summary>
        private class Map
        {
            private ulong offset;       // starting offset within the file
            private uint crc32;         // 32-bit CRC of the uncompressed data
            private ushort length_lo;   // lower 16 bits of length
            private byte length_hi;     // upper 8 bits of length
            private byte flags;         // flags, indicating compression info
        }

        public const int HeaderSize = 108;
        public const uint Version = 4;

        // V4-specific header values
        private Flags flags;                        // flags (see above)
        private Compression compression;            // compression type
        private uint totalhunks;                    // total # of hunks represented
        private ulong logicalbytes;                 // logical size of the data (in bytes)
        private ulong metaoffset;                   // offset to the first blob of metadata
        private uint hunkbytes;                     // number of bytes per hunk
        private byte[] sha1 = new byte[20];         // combined raw+meta SHA1
        private byte[] parentsha1 = new byte[20];   // combined raw+meta SHA1 of parent
        private byte[] rawsha1 = new byte[20];      // raw data SHA1

        /// <summary>
        /// Parse and validate the header as if it's V4
        /// </summary>
        public static CHDFileV4 Deserialize(Stream stream)
        {
            CHDFileV4 chd = new CHDFileV4();

            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                chd.tag = br.ReadCharsBigEndian(8);
                chd.length = br.ReadUInt32BigEndian();
                chd.version = br.ReadUInt32BigEndian();
                chd.flags = (Flags)br.ReadUInt32BigEndian();
                chd.compression = (Compression)br.ReadUInt32BigEndian();
                chd.totalhunks = br.ReadUInt32BigEndian();
                chd.logicalbytes = br.ReadUInt64BigEndian();
                chd.metaoffset = br.ReadUInt64BigEndian();
                chd.hunkbytes = br.ReadUInt32BigEndian();
                chd.sha1 = br.ReadBytesBigEndian(20);
                chd.parentsha1 = br.ReadBytesBigEndian(20);
                chd.rawsha1 = br.ReadBytesBigEndian(20);
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
