using System.IO;
using SabreTools.Data.Models.CHD;

namespace SabreTools.FileTypes.CHD
{
    public class CHDFile : BaseFile
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

        #endregion

        #region Private instance variables

        /// <summary>
        /// Model representing the correct CHD header
        /// </summary>
        protected Header? _header;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new CHDFile from an input file
        /// </summary>
        /// <param name="filename">Filename respresenting the CHD file</param>
        public static CHDFile? Create(string filename)
        {
            using Stream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Create(fs);
        }

        /// <summary>
        /// Create a new CHDFile from an input stream
        /// </summary>
        /// <param name="stream">Stream representing the CHD file</param>
        public static CHDFile? Create(Stream stream)
        {
            try
            {
                var header = new Serialization.Readers.CHD().Deserialize(stream);
                return header switch
                {
                    HeaderV1 v1 => new CHDFile
                    {
                        _header = header,
                        InternalMD5 = v1.MD5,
                    },
                    HeaderV2 v2 => new CHDFile
                    {
                        _header = header,
                        InternalMD5 = v2.MD5,
                    },
                    HeaderV3 v3 => new CHDFile
                    {
                        _header = header,
                        InternalMD5 = v3.MD5,
                        InternalSHA1 = v3.SHA1,
                    },
                    HeaderV4 v4 => new CHDFile
                    {
                        _header = header,
                        InternalSHA1 = v4.SHA1,
                    },
                    HeaderV5 v5 => new CHDFile
                    {
                        _header = header,
                        InternalSHA1 = v5.SHA1,
                    },
                    _ => null,
                };
            }
            catch
            {
                return null;
            }
        }

        #endregion
    }
}
