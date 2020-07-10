using System;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// This is code adapted from chd.h and chd.cpp in MAME
    /// Additional archival code from https://github.com/rtissera/libchdr/blob/master/src/chd.h
    /// </summary>
    public abstract class CHDFile : BaseFile
    {
        #region Private instance variables

        // Common header fields
        protected char[] tag = new char[8]; // 'MComprHD'
        protected uint length;              // length of header (including tag and length fields)
        protected uint version;             // drive format version

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new CHDFile from an input file
        /// </summary>
        /// <param name="filename">Filename respresenting the CHD file</param>
        public static CHDFile Create(string filename)
        {
            using (FileStream fs = FileExtensions.TryOpenRead(filename))
            {
                return Create(fs);
            }
        }

        /// <summary>
        /// Create a new CHDFile from an input stream
        /// </summary>
        /// <param name="chdstream">Stream representing the CHD file</param>
        public static CHDFile Create(Stream chdstream)
        {
            // Read the standard CHD headers
            (char[] tag, uint length, uint version) = GetHeaderValues(chdstream);
            chdstream.Seek(-16, SeekOrigin.Current); // Seek back to start

            // Validate that this is actually a valid CHD
            uint validatedVersion = ValidateHeader(tag, length, version);
            if (validatedVersion == 0)
                return null;

            // Read and retrun the current CHD
            CHDFile generated = ReadAsVersion(chdstream, version);
            if (generated != null)
                generated.Type = FileType.CHD;

            return generated;
        }

        #endregion

        #region Abstract functionality

        /// <summary>
        /// Return the best-available hash for a particular CHD version
        /// </summary>
        public abstract byte[] GetHash();

        #endregion

        #region Header Parsing

        /// <summary>
        /// Get the generic header values of a CHD, if possible
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static (char[] tag, uint length, uint version) GetHeaderValues(Stream stream)
        {
            char[] parsedTag = new char[8];
            uint parsedLength = 0;
            uint parsedVersion = 0;

            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                parsedTag = br.ReadCharsBigEndian(8);
                parsedLength = br.ReadUInt32BigEndian();
                parsedVersion = br.ReadUInt32BigEndian();
            }

            return (parsedTag, parsedLength, parsedVersion);
        }

        /// <summary>
        /// Validate the header values
        /// </summary>
        /// <returns>Matching version, 0 if none</returns>
        private static uint ValidateHeader(char[] tag, uint length, uint version)
        {
            if (!string.Equals(new string(tag), "MComprHD", StringComparison.Ordinal))
                return 0;

            switch (version)
            {
                case 1:
                    return length == CHDFileV1.HeaderSize ? version : 0;
                case 2:
                    return length == CHDFileV2.HeaderSize ? version : 0;
                case 3:
                    return length == CHDFileV3.HeaderSize ? version : 0;
                case 4:
                    return length == CHDFileV4.HeaderSize ? version : 0;
                case 5:
                    return length == CHDFileV5.HeaderSize ? version : 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Read a stream as a particular CHD version
        /// </summary>
        /// <param name="stream">CHD file as a stream</param>
        /// <param name="version">CHD version to parse</param>
        /// <returns>Populated CHD file, null on failure</returns>
        private static CHDFile ReadAsVersion(Stream stream, uint version)
        {
            switch (version)
            {
                case 1:
                    return CHDFileV1.Deserialize(stream);
                case 2:
                    return CHDFileV2.Deserialize(stream);
                case 3:
                    return CHDFileV3.Deserialize(stream);
                case 4:
                    return CHDFileV4.Deserialize(stream);
                case 5:
                    return CHDFileV5.Deserialize(stream);
                default:
                    return null;
            }
        }

        #endregion
    }
}
