namespace SabreTools.FileTypes
{
    public class BaseFile
    {
        #region Fields

        /// <summary>
        /// Filename or path to the file
        /// </summary>
        public string? Filename { get; set; }

        /// <summary>
        /// Direct parent of the file
        /// </summary>
        public string? Parent { get; set; }

        /// <summary>
        /// Date stamp of the file
        /// </summary>
        public string? Date { get; set; }

        /// <summary>
        /// Optional size of the file
        /// </summary>
        public long? Size { get; set; }

        /// <summary>
        /// CRC32 hash of the file
        /// </summary>
        public byte[]? CRC { get; set; }

        /// <summary>
        /// MD2 hash of the file
        /// </summary>
        public byte[]? MD2 { get; set; }

        /// <summary>
        /// MD4 hash of the file
        /// </summary>
        public byte[]? MD4 { get; set; }

        /// <summary>
        /// MD5 hash of the file
        /// </summary>
        public byte[]? MD5 { get; set; }

        /// <summary>
        /// RIPEMD128 hash of the file
        /// </summary>
        public byte[]? RIPEMD128 { get; set; }

        /// <summary>
        /// RIPEMD160 hash of the file
        /// </summary>
        public byte[]? RIPEMD160 { get; set; }

        /// <summary>
        /// SHA-1 hash of the file
        /// </summary>
        public byte[]? SHA1 { get; set; }

        /// <summary>
        /// SHA-256 hash of the file
        /// </summary>
        public byte[]? SHA256 { get; set; }

        /// <summary>
        /// SHA-384 hash of the file
        /// </summary>
        public byte[]? SHA384 { get; set; }

        /// <summary>
        /// SHA-512 hash of the file
        /// </summary>
        public byte[]? SHA512 { get; set; }

        /// <summary>
        /// SpamSum fuzzy hash of the file
        /// </summary>
        public byte[]? SpamSum { get; set; }

        #endregion

        /// <summary>
        /// Create a new BaseFile with no base file
        /// </summary>
        public BaseFile() { }

        /// <summary>
        /// Create a new BaseFile from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use</param>
        public BaseFile(string filename)
        {
            Filename = filename;
        }
    }
}
