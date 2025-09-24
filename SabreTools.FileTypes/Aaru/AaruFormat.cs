using System.IO;
using System.Text;
using SabreTools.IO.Extensions;
using static SabreTools.FileTypes.Constants;

namespace SabreTools.FileTypes.Aaru
{
    /// <summary>
    /// AaruFormat code is based on the Aaru project
    /// </summary>
    /// <see href="https://github.com/aaru-dps/Aaru/tree/master/Aaru.Images/AaruFormat"/> 
    public class AaruFormat : BaseFile
    {
        #region Fields

        /// <summary>
        /// Internal MD5 hash of the file
        /// </summary>
        public byte[]? InternalMD5 { get; set; }

        /// <summary>
        /// Internal SHA-1 hash of the file
        /// </summary>
        public byte[]? InternalSHA1 { get; set; }

        /// <summary>
        /// Internal SHA-256 hash of the file
        /// </summary>
        public byte[]? InternalSHA256 { get; set; }

        /// <summary>
        /// Internal SpamSum fuzzy hash of the file
        /// </summary>
        public byte[]? InternalSpamSum { get; set; }

        #endregion

        #region Private instance variables

        #region Header

        protected ulong Identifier;                     // 'AARUFRMT' (0x544D524655524141)
        protected string? Application;                  // Name of application that created image
        protected byte ImageMajorVersion;               // Image format major version
        protected byte ImageMinorVersion;               // Image format minor version
        protected byte ApplicationMajorVersion;         // Major version of application that created image
        protected byte ApplicationMinorVersion;         // Minor version of application that created image
        protected AaruMediaType MediaType;              // Media type contained in image
        protected ulong IndexOffset;                    // Offset to index
        protected long CreationTime;                    // Windows filetime of creation time
        protected long LastWrittenTime;                 // Windows filetime of last written time

        #endregion

        #region Internal Values

        protected IndexHeader? IndexHeader;
        protected IndexEntry[]? IndexEntries;

        #endregion

        #endregion // Private instance variables

        #region Constructors

        /// <summary>
        /// Empty constructor
        /// </summary>
        public AaruFormat()
        {
        }

        /// <summary>
        /// Create a new AaruFormat from an input file
        /// </summary>
        /// <param name="filename">Filename respresenting the AaruFormat file</param>
        public static AaruFormat? Create(string filename)
        {
            using Stream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Create(fs);
        }

        /// <summary>
        /// Create a new AaruFormat from an input stream
        /// </summary>
        /// <param name="aarustream">Stream representing the AaruFormat file</param>
        public static AaruFormat? Create(Stream aarustream)
        {
            try
            {
                // Validate that this is actually a valid AaruFormat (by magic string alone)
                bool validated = ValidateHeader(aarustream);
                aarustream.SeekIfPossible(); // Seek back to start
                if (!validated)
                    return null;

                // Read and return the current AaruFormat
                return Deserialize(aarustream);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Header Parsing

        /// <summary>
        /// Validate we start with the right magic number
        /// </summary>
        public static bool ValidateHeader(Stream aarustream)
        {
            // Read the magic string
            byte[] magicBytes = new byte[8];
            int read = aarustream.Read(magicBytes, 0, 8);

            // If we didn't read the magic fully, we don't have an AaruFormat
            if (read < 8)
                return false;

            // If the bytes don't match, we don't have an AaruFormat
            if (!magicBytes.StartsWith(AaruFormatSignature))
                return false;

            return true;
        }

        /// <summary>
        /// Read a stream as an AaruFormat
        /// </summary>
        /// <param name="stream">AaruFormat file as a stream</param>
        /// <returns>Populated AaruFormat file, null on failure</returns>
        public static AaruFormat? Deserialize(Stream stream)
        {
            try
            {
                var aif = new AaruFormat();

                aif.Identifier = stream.ReadUInt64();
                byte[] applicationBytes = stream.ReadBytes(64);
                aif.Application = Encoding.Unicode.GetString(applicationBytes);
                aif.ImageMajorVersion = stream.ReadByteValue();
                aif.ImageMinorVersion = stream.ReadByteValue();
                aif.ApplicationMajorVersion = stream.ReadByteValue();
                aif.ApplicationMinorVersion = stream.ReadByteValue();
                aif.MediaType = (AaruMediaType)stream.ReadUInt32();
                aif.IndexOffset = stream.ReadUInt64();
                aif.CreationTime = stream.ReadInt64();
                aif.LastWrittenTime = stream.ReadInt64();

                // If the offset is bigger than the stream, we can't read it
                if (aif.IndexOffset > (ulong)stream.Length)
                    return null;

                // Otherwise, we read in the index header
                stream.Seek((long)aif.IndexOffset, SeekOrigin.Begin);
                aif.IndexHeader = IndexHeader.Deserialize(stream);
                if (aif.IndexHeader.entries == 0)
                    return null;

                // Get the list of entries
                aif.IndexEntries = new IndexEntry[aif.IndexHeader.entries];
                for (ushort index = 0; index < aif.IndexHeader.entries; index++)
                {
                    aif.IndexEntries[index] = IndexEntry.Deserialize(stream);
                    switch (aif.IndexEntries[index].blockType)
                    {
                        // We don't do anything with these block types currently
                        case AaruBlockType.DataBlock:
                        case AaruBlockType.DeDuplicationTable:
                        case AaruBlockType.Index:
                        case AaruBlockType.Index2:
                        case AaruBlockType.GeometryBlock:
                        case AaruBlockType.MetadataBlock:
                        case AaruBlockType.TracksBlock:
                        case AaruBlockType.CicmBlock:
                        case AaruBlockType.DataPositionMeasurementBlock:
                        case AaruBlockType.SnapshotBlock:
                        case AaruBlockType.ParentBlock:
                        case AaruBlockType.DumpHardwareBlock:
                        case AaruBlockType.TapeFileBlock:
                        case AaruBlockType.TapePartitionBlock:
                        case AaruBlockType.CompactDiscIndexesBlock:
                            // No-op
                            break;

                        // Read in all available hashes
                        case AaruBlockType.ChecksumBlock:
                            // If the offset is bigger than the stream, we can't read it
                            if (aif.IndexEntries[index].offset > (ulong)stream.Length)
                                return null;

                            // Otherwise, we read in the block
                            stream.Seek((long)aif.IndexEntries[index].offset, SeekOrigin.Begin);
                            ChecksumHeader checksumHeader = ChecksumHeader.Deserialize(stream);
                            if (checksumHeader.entries == 0)
                                return null;

                            // Read through each and pick out the ones we care about
                            for (byte entry = 0; entry < checksumHeader.entries; entry++)
                            {
                                ChecksumEntry? checksumEntry = ChecksumEntry.Deserialize(stream);
                                if (checksumEntry == null)
                                    continue;

                                switch (checksumEntry.type)
                                {
                                    case AaruChecksumAlgorithm.Invalid:
                                        break;
                                    case AaruChecksumAlgorithm.Md5:
                                        aif.InternalMD5 = checksumEntry.checksum;
                                        break;
                                    case AaruChecksumAlgorithm.Sha1:
                                        aif.InternalSHA1 = checksumEntry.checksum;
                                        break;
                                    case AaruChecksumAlgorithm.Sha256:
                                        aif.InternalSHA256 = checksumEntry.checksum;
                                        break;
                                    case AaruChecksumAlgorithm.SpamSum:
                                        aif.InternalSpamSum = checksumEntry.checksum;
                                        break;
                                }
                            }

                            // Once we got hashes, we return early
                            return aif;
                    }
                }

                return aif;
            }
            catch
            {
                // We don't care what the error was at this point
                return null;
            }
        }

        #endregion
    }
}
