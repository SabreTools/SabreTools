using System.Collections.Generic;
using System.IO;

using SabreTools.FileTypes.Archives;

namespace SabreTools.FileTypes
{
    public abstract class BaseArchive : Folder
    {
        #region Fields

        /// <summary>
        /// Determines if dates are read or written
        /// </summary>
        public bool UseDates { get; set; } = false;

        #endregion

        #region Protected instance variables

        /// <summary>
        /// Buffer size used by archives
        /// </summary>
        protected const int _bufferSize = 4096 * 128;

        #endregion

        #region Construtors

        /// <summary>
        /// Create a new Archive with no base file
        /// </summary>
        public BaseArchive()
        {
        }

        /// <summary>
        /// Create a new Archive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        /// <param name="getHashes">True if hashes for this file should be calculated, false otherwise (default)</param>
        public BaseArchive(string filename, bool getHashes = false)
            : base(filename, getHashes)
        {
        }

        /// <summary>
        /// Create an archive object from a filename, if possible
        /// </summary>
        /// <param name="input">Name of the file to create the archive from</param>
        /// <returns>Archive object representing the inputs</returns>
        public static BaseArchive Create(string input)
        {
            BaseArchive archive = null;

            // First get the archive type
            FileType? at = GetFileType(input);

            // If we got back null, then it's not an archive, so we we return
            if (at == null)
                return archive;

            // Create the archive based on the type
            staticLogger.Verbose($"Found archive of type: {at}");
            switch (at)
            {
                case FileType.GZipArchive:
                    archive = new GZipArchive(input);
                    break;

                case FileType.RarArchive:
                    archive = new RarArchive(input);
                    break;

                case FileType.SevenZipArchive:
                    archive = new SevenZipArchive(input);
                    break;

                case FileType.TapeArchive:
                    archive = new TapeArchive(input);
                    break;

                case FileType.ZipArchive:
                    archive = new ZipArchive(input);
                    break;

                case FileType.ISOArchive:
                    archive = new ISOArchive(input);
                    break;

                default:
                    // We ignore all other types for now
                    break;
            }

            return archive;
        }

        /// <summary>
        /// Create an archive object of the specified type, if possible
        /// </summary>
        /// <param name="archiveType">SharpCompress.Common.ArchiveType representing the archive to create</param>
        /// <returns>Archive object representing the inputs</returns>
        public static BaseArchive Create(FileType archiveType)
        {
            return archiveType switch
            {
                FileType.GZipArchive => new GZipArchive(),
                FileType.RarArchive => new RarArchive(),
                FileType.SevenZipArchive => new SevenZipArchive(),
                FileType.TapeArchive => new TapeArchive(),
                FileType.ZipArchive => new ZipArchive(),
                _ => null,
            };
        }

        #endregion

        #region Extraction

        /// <inheritdoc/>
        public override abstract bool CopyAll(string outDir);

        /// <inheritdoc/>
        public override abstract string CopyToFile(string entryName, string outDir);

        /// <inheritdoc/>
        public override abstract (MemoryStream, string) CopyToStream(string entryName);

        #endregion

        #region Information

        /// <inheritdoc/>
        public override abstract List<BaseFile> GetChildren();

        /// <inheritdoc/>
        public override abstract List<string> GetEmptyFolders();

        /// <summary>
        /// Check whether the input file is a standardized format
        /// </summary>
        public abstract bool IsTorrent();

        #endregion

        #region Writing

        /// <inheritdoc/>
        public override abstract bool Write(string inputFile, string outDir, BaseFile baseFile);

        /// <inheritdoc/>
        public override abstract bool Write(Stream inputStream, string outDir, BaseFile baseFile);

        /// <inheritdoc/>
        public override abstract bool Write(List<string> inputFiles, string outDir, List<BaseFile> baseFiles);

        #endregion
    }
}
