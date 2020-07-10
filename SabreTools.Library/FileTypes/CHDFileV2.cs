using System;
using System.IO;
using System.Text;

using SabreTools.Library.Tools;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// CHD V2 File
    /// </summary>
    public class CHDFileV2 : CHDFile
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
        }

        /// <summary>
        /// Map format
        /// </summary>
        private class Map
        {
            private ulong offset;   // 44; starting offset within the file
            private ulong length;   // 20; length of data; if == hunksize, data is uncompressed
        }

        public const int HeaderSize = 80;
        public const uint Version = 2;

        // V2-specific header values
        private Flags flags;                        // flags (see above)
        private Compression compression;            // compression type
        private uint hunksize;                      // 512-byte sectors per hunk
        private uint totalhunks;                    // total # of hunks represented
        private uint cylinders;                     // number of cylinders on hard disk
        private uint heads;                         // number of heads on hard disk
        private uint sectors;                       // number of sectors on hard disk
        private byte[] md5 = new byte[16];          // MD5 checksum of raw data
        private byte[] parentmd5 = new byte[16];    // MD5 checksum of parent file
        private uint seclen;                        // number of bytes per sector

        /// <summary>
        /// Parse and validate the header as if it's V2
        /// </summary>
        public static CHDFileV2 Deserialize(Stream stream)
        {
            CHDFileV2 chd = new CHDFileV2();

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
                chd.seclen = br.ReadUInt32BigEndian();
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
