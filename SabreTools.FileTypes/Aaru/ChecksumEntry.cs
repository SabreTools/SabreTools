using System.IO;
using SabreTools.IO.Extensions;
using SabreTools.Numerics.Extensions;

namespace SabreTools.FileTypes.Aaru
{
    /// <summary>
    /// Checksum entry, followed by checksum data itself
    /// </summary>
    /// <see href="https://github.com/aaru-dps/Aaru/blob/master/Aaru.Images/AaruFormat/Structs.cs" />
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

            checksumEntry.type = (AaruChecksumAlgorithm)stream.ReadByteValue();
            checksumEntry.length = stream.ReadUInt32();
            if (checksumEntry.length == 0)
                return null;

            checksumEntry.checksum = stream.ReadBytes((int)checksumEntry.length);

            return checksumEntry;
        }
    }
}
