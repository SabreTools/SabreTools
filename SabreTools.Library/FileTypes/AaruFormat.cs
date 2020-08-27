using System;
using System.IO;
using System.Text;

using SabreTools.Library.Data;
using SabreTools.Library.IO;
using SabreTools.Library.Tools;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// AaruFormat code is based on the Aaru project
    /// See https://github.com/aaru-dps/Aaru/tree/master/Aaru.Images/AaruFormat
    /// </summary>
    public class AaruFormat : BaseFile
    {
        #region Private instance variables

        #region Header

        protected char[] identifier = new char[8];      // 'AARUFRMT' (0x544D524655524141)
        protected char[] application = new char[32];    // Name of application that created image
        protected byte imageMajorVersion;               // Image format major version
        protected byte imageMinorVersion;               // Image format minor version
        protected byte applicationMajorVersion;         // Major version of application that created image
        protected byte applicationMinorVersion;         // Minor version of application that created image
        protected AaruMediaType mediaType;              // Media type contained in image
        protected ulong indexOffset;                    // Offset to index
        protected long creationTime;                    // Windows filetime of creation time
        protected long lastWrittenTime;                 // Windows filetime of last written time

        #endregion

        #region Index Header Values

        // TODO: Read https://github.com/aaru-dps/Aaru/blob/master/Aaru.Images/AaruFormat/Verify.cs

        #endregion

        #endregion // Private instance variables

        #region Constructors

        /// <summary>
        /// Create a new AaruFormat from an input file
        /// </summary>
        /// <param name="filename">Filename respresenting the AaruFormat file</param>
        public static AaruFormat Create(string filename)
        {
            using (FileStream fs = FileExtensions.TryOpenRead(filename))
            {
                return Create(fs);
            }
        }

        /// <summary>
        /// Create a new AaruFormat from an input stream
        /// </summary>
        /// <param name="aarustream">Stream representing the AaruFormat file</param>
        public static AaruFormat Create(Stream aarustream)
        {
            try
            {
                // Validate that this is actually a valid AaruFormat (by magic string alone)
                bool validated = ValidateHeader(aarustream);
                aarustream.Seek(-8, SeekOrigin.Current); // Seek back to start
                if (!validated)
                    return null;

                // Read and retrun the current AaruFormat
                AaruFormat generated = Deserialize(aarustream);
                if (generated != null)
                    generated.Type = FileType.AaruFormat;

                return generated;
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
        private static bool ValidateHeader(Stream aarustream)
        {
            // Read the magic string
            byte[] magicBytes = new byte[8];
            int read = aarustream.Read(magicBytes, 0, 8);

            // If we didn't read the magic fully, we don't have an AaruFormat
            if (read < 8)
                return false;

            // If the bytes don't match, we don't have an AaruFormat
            if (!magicBytes.StartsWith(Constants.AaruFormatSignature))
                return false;

            return true;
        }

        /// <summary>
        /// Read a stream as an AaruFormat
        /// </summary>
        /// <param name="stream">AaruFormat file as a stream</param>
        /// <returns>Populated AaruFormat file, null on failure</returns>
        private static AaruFormat Deserialize(Stream stream)
        {
            try
            {
                AaruFormat aif = new AaruFormat();

                using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
                {
                    aif.identifier = br.ReadChars(8);
                    aif.application = br.ReadChars(32);
                    aif.imageMajorVersion = br.ReadByte();
                    aif.imageMinorVersion = br.ReadByte();
                    aif.applicationMajorVersion = br.ReadByte();
                    aif.applicationMinorVersion = br.ReadByte();
                    aif.mediaType = (AaruMediaType)br.ReadUInt32();
                    aif.indexOffset = br.ReadUInt64();
                    aif.creationTime = br.ReadInt64();
                    aif.lastWrittenTime = br.ReadInt64();
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
