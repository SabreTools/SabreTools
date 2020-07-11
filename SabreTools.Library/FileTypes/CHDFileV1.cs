using System;
using System.IO;
using System.Text;

using SabreTools.Library.Tools;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// CHD V1 File
    /// </summary>
    public class CHDFileV1 : CHDFile
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
        }

        /// <summary>
        /// Map format
        /// </summary>
        public class Map
        {
            public ulong offset;   // 44; starting offset within the file
            public ulong length;   // 20; length of data; if == hunksize, data is uncompressed
        }

        public const int HeaderSize = 76;
        public const uint Version = 1;

        // V1-specific header values
        public Flags flags;                        // flags (see above)
        public Compression compression;            // compression type
        public uint hunksize;                      // 512-byte sectors per hunk
        public uint totalhunks;                    // total # of hunks represented
        public uint cylinders;                     // number of cylinders on hard disk
        public uint heads;                         // number of heads on hard disk
        public uint sectors;                       // number of sectors on hard disk
        public byte[] md5 = new byte[16];          // MD5 checksum of raw data
        public byte[] parentmd5 = new byte[16];    // MD5 checksum of parent file

        /// <summary>
        /// Parse and validate the header as if it's V1
        /// </summary>
        public static CHDFileV1 Deserialize(Stream stream)
        {
            CHDFileV1 chd = new CHDFileV1();

            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                chd.tag = br.ReadCharsBigEndian(8);
                chd.length = br.ReadUInt32BigEndian();
                chd.version = br.ReadUInt32BigEndian();
                chd.flags = (Flags)br.ReadUInt32BigEndian();
                chd.compression = (Compression)br.ReadUInt32BigEndian();
                chd.hunksize = br.ReadUInt32BigEndian();
                chd.totalhunks = br.ReadUInt32BigEndian();
                chd.cylinders = br.ReadUInt32BigEndian();
                chd.heads = br.ReadUInt32BigEndian();
                chd.sectors = br.ReadUInt32BigEndian();
                chd.md5 = br.ReadBytesBigEndian(16);
                chd.parentmd5 = br.ReadBytesBigEndian(16);
            }

            return chd;
        }

        /// <summary>
        /// Return internal MD5 hash
        /// </summary>
        public override byte[] GetHash()
        {
            return md5;
        }
    }
}
