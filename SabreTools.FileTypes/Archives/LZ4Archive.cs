using System;
using System.Collections.Generic;
using System.IO;

namespace SabreTools.FileTypes.Archives
{
    /// <summary>
    /// Represents a TorrentLRZip archive for reading and writing
    /// </summary>
    /// TODO: Implement from source at https://github.com/lz4/lz4
    /// http://fastcompression.blogspot.com/2013/04/lz4-streaming-format-final.html (2013)
    public class LZ4Archive : BaseArchive
    {
        #region Constructors

        /// <summary>
        /// Create a new LZ4Archive with no base file
        /// </summary>
        public LZ4Archive()
            : base()
        {
            this.Type = FileType.LZ4Archive;
        }

        /// <summary>
        /// Create a new LZ4Archive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        /// <param name="getHashes">True if hashes for this file should be calculated, false otherwise (default)</param>
        public LZ4Archive(string filename, bool getHashes = false)
            : base(filename, getHashes)
        {
            this.Type = FileType.LZ4Archive;
        }

        #endregion

        #region Extraction

        /// <inheritdoc/>
        public override bool CopyAll(string outDir)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override string CopyToFile(string entryName, string outDir)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override (MemoryStream, string) CopyToStream(string entryName)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Information

        /// <inheritdoc/>
        public override List<BaseFile> GetChildren()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override List<string> GetEmptyFolders()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool IsTorrent()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Writing

        /// <inheritdoc/>
        public override bool Write(string inputFile, string outDir, BaseFile rom)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool Write(Stream inputStream, string outDir, BaseFile rom)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool Write(List<string> inputFiles, string outDir, List<BaseFile> baseFiles)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
