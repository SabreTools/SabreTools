using System;
using System.IO;
using System.Text;

using SabreTools.Library.Tools;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// CHD V3 File
    /// </summary>
    public class CHDFileV3 : CHDFile
    {
        /// <summary>
        /// CHD flags
        /// </summary>
        [Flags]
        public enum Flags : uint
        {
            DriveHasParent = 0x00000001,
            DriveAllowsWrites = 0x00000002,
        }

        /// <summary>
        /// Compression being used in CHD
        /// </summary>
        public enum Compression : uint
        {
            CHDCOMPRESSION_NONE = 0,
            CHDCOMPRESSION_ZLIB = 1,
            CHDCOMPRESSION_ZLIB_PLUS = 2,
        }

        /// <summary>
        /// Map format
        /// </summary>
        public class Map
        {
            public ulong offset;       // starting offset within the file
            public uint crc32;         // 32-bit CRC of the uncompressed data
            public ushort length_lo;   // lower 16 bits of length
            public byte length_hi;     // upper 8 bits of length
            public byte flags;         // flags, indicating compression info
        }

        public const int HeaderSize = 120;
        public const uint Version = 3;

        // V3-specific header values
        public Flags flags;                        // flags (see above)
        public Compression compression;            // compression type
        public uint totalhunks;                    // total # of hunks represented
        public ulong logicalbytes;                 // logical size of the data (in bytes)
        public ulong metaoffset;                   // offset to the first blob of metadata
        public byte[] md5 = new byte[16];          // MD5 checksum of raw data
        public byte[] parentmd5 = new byte[16];    // MD5 checksum of parent file
        public uint hunkbytes;                     // number of bytes per hunk
        public byte[] sha1 = new byte[20];         // SHA1 checksum of raw data
        public byte[] parentsha1 = new byte[20];   // SHA1 checksum of parent file

        /// <summary>
        /// Parse and validate the header as if it's V3
        /// </summary>
        public static CHDFileV3 Deserialize(Stream stream)
        {
            CHDFileV3 chd = new CHDFileV3();

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
                chd.md5 = br.ReadBytesBigEndian(16);
                chd.parentmd5 = br.ReadBytesBigEndian(16);
                chd.hunkbytes = br.ReadUInt32BigEndian();
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
