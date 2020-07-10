using System.IO;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

namespace SabreTools.Library.FileTypes
{
    public class BaseFile
    {
        #region Publicly facing variables

        // TODO: Get all of these values automatically so there is no public "set"
        public FileType Type { get; protected set; }
        public string Filename { get; set; }
        public string Parent { get; set; }
        public string Date { get; set; }
        public long? Size { get; set; }
        public byte[] CRC { get; set; }
        public byte[] MD5 { get; set; }
#if NET_FRAMEWORK
        public byte[] RIPEMD160 { get; set; }
#endif
        public byte[] SHA1 { get; set; }
        public byte[] SHA256 { get; set; }
        public byte[] SHA384 { get; set; }
        public byte[] SHA512 { get; set; }

        #endregion

        #region Construtors

        /// <summary>
        /// Create a new BaseFile with no base file
        /// </summary>
        public BaseFile()
        {
        }

        /// <summary>
        /// Create a new BaseFile from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use</param>
        /// <param name="getHashes">True if hashes for this file should be calculated (default), false otherwise</param>
        public BaseFile(string filename, bool getHashes = true)
        {
            this.Filename = filename;

            if (getHashes)
            {
                BaseFile temp = FileExtensions.GetInfo(this.Filename);
                if (temp != null)
                {
                    this.Parent = temp.Parent;
                    this.Date = temp.Date;
                    this.CRC = temp.CRC;
                    this.MD5 = temp.MD5;
#if NET_FRAMEWORK
                    this.RIPEMD160 = temp.RIPEMD160;
#endif
                    this.SHA1 = temp.SHA1;
                    this.SHA256 = temp.SHA256;
                    this.SHA384 = temp.SHA384;
                    this.SHA512 = temp.SHA512;
                }
            }
        }

        /// <summary>
        /// Create a new BaseFile from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use</param>
        /// <param name="stream">Stream to populate information from</param>
        /// <param name="getHashes">True if hashes for this file should be calculated (default), false otherwise</param>
        public BaseFile(string filename, Stream stream, bool getHashes = true)
        {
            this.Filename = filename;

            if (getHashes)
            {
                BaseFile temp = stream.GetInfo();
                if (temp != null)
                {
                    this.Parent = temp.Parent;
                    this.Date = temp.Date;
                    this.CRC = temp.CRC;
                    this.MD5 = temp.MD5;
#if NET_FRAMEWORK
                    this.RIPEMD160 = temp.RIPEMD160;
#endif
                    this.SHA1 = temp.SHA1;
                    this.SHA256 = temp.SHA256;
                    this.SHA384 = temp.SHA384;
                    this.SHA512 = temp.SHA512;
                }
            }

        }

        #endregion
    }
}
