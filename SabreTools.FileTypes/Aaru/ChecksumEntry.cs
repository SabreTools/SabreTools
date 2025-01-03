﻿using System.IO;
using System.Text;

namespace SabreTools.FileTypes.Aaru
{
    /// <summary>
    /// Checksum entry, followed by checksum data itself
    /// </summary>
    /// <see cref="https://github.com/aaru-dps/Aaru/blob/master/Aaru.Images/AaruFormat/Structs.cs" />
    public class ChecksumEntry
    {
        /// <summary>Checksum algorithm</summary>
        public AaruChecksumAlgorithm type;
        /// <summary>Length in bytes of checksum that follows this structure</summary>
        public uint length;
        /// <summary>Checksum that follows this structure</summary>
        public byte[]? checksum;

        /// <summary>
        /// Read a stream as an v
        /// </summary>
        /// <param name="stream">ChecksumEntry as a stream</param>
        /// <returns>Populated ChecksumEntry, null on failure</returns>
        public static ChecksumEntry? Deserialize(Stream stream)
        {
            var checksumEntry = new ChecksumEntry();

#if NET20 || NET35 || NET40
            using (var br = new BinaryReader(stream, Encoding.Default))
#else
            using (var br = new BinaryReader(stream, Encoding.Default, true))
#endif
            {
                checksumEntry.type = (AaruChecksumAlgorithm)br.ReadByte();
                checksumEntry.length = br.ReadUInt32();
                if (checksumEntry.length == 0)
                    return null;

                checksumEntry.checksum = br.ReadBytes((int)checksumEntry.length);
            }

            return checksumEntry;
        }
    }
}
