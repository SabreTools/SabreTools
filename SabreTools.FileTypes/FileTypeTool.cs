using System;
using System.IO;
using SabreTools.FileTypes.Aaru;
using SabreTools.FileTypes.Archives;
using SabreTools.FileTypes.CHD;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;
using SabreTools.IO.Logging;
using SabreTools.Matching;
using SabreTools.Skippers;
using static SabreTools.FileTypes.Constants;

namespace SabreTools.FileTypes
{
    public static class FileTypeTool
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger _staticLogger = new();

        #endregion

        #region File Info

        /// <summary>
        /// Retrieve file information for a single file
        /// </summary>
        /// <param name="input">Filename to get information from</param>
        /// <param name="hashes">Hashes to include in the information</param>
        /// <returns>Populated BaseFile object if success, empty on error</returns>
        public static BaseFile GetInfo(string input, HashType[] hashes)
            => GetInfo(input, hashes, header: null);

        /// <summary>
        /// Retrieve file information for a single file
        /// </summary>
        /// <param name="input">Filename to get information from</param>
        /// <param name="hashes">Hashes to include in the information</param>
        /// <param name="header">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <returns>Populated BaseFile object if success, empty on error</returns>
        public static BaseFile GetInfo(string input, HashType[] hashes, string? header)
        {
            // Add safeguard if file is invalid
            if (string.IsNullOrEmpty(input))
                return new BaseFile();
            else if (!File.Exists(input))
                return new BaseFile();

            try
            {
                // Get input information
                var fileType = GetFileType(input);
                Stream inputStream = GetInfoStream(input, header);
                BaseFile? baseFile = GetBaseFile(inputStream, fileType, hashes);

                // Dispose of the input stream
                inputStream.Dispose();

                // Add unique data from the file
                baseFile!.Filename = Path.GetFileName(input);
                baseFile.Date = new FileInfo(input).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");

                return baseFile;
            }
            catch (Exception ex)
            {
                _staticLogger.Error(ex);
                return new BaseFile();
            }
        }

        /// <summary>
        /// Retrieve file information for a single file
        /// </summary>
        /// <param name="input">Stream to get information from</param>
        /// <param name="hashes">Hashes to include in the information</param>
        /// <returns>Populated BaseFile object if success, empty one on error</returns>
        public static BaseFile GetInfo(Stream? input, HashType[] hashes)
        {
            // If we have no stream
            if (input == null)
                return new BaseFile();

            try
            {
                // Run the hashing on the input stream
                var hashDict = HashTool.GetStreamHashesAndSize(input, hashes, leaveOpen: true, out long size);
                if (hashDict == null)
                    return new BaseFile();

                // Create a base file with the resulting hashes
                var baseFile = new BaseFile()
                {
                    Size = size,
                    CRC = hashDict.ContainsKey(HashType.CRC32) ? hashDict[HashType.CRC32].FromHexString() : null,
                    MD2 = hashDict.ContainsKey(HashType.MD2) ? hashDict[HashType.MD2].FromHexString() : null,
                    MD4 = hashDict.ContainsKey(HashType.MD4) ? hashDict[HashType.MD4].FromHexString() : null,
                    MD5 = hashDict.ContainsKey(HashType.MD5) ? hashDict[HashType.MD5].FromHexString() : null,
                    RIPEMD128 = hashDict.ContainsKey(HashType.RIPEMD128) ? hashDict[HashType.RIPEMD128].FromHexString() : null,
                    RIPEMD160 = hashDict.ContainsKey(HashType.RIPEMD160) ? hashDict[HashType.RIPEMD160].FromHexString() : null,
                    SHA1 = hashDict.ContainsKey(HashType.SHA1) ? hashDict[HashType.SHA1].FromHexString() : null,
                    SHA256 = hashDict.ContainsKey(HashType.SHA256) ? hashDict[HashType.SHA256].FromHexString() : null,
                    SHA384 = hashDict.ContainsKey(HashType.SHA384) ? hashDict[HashType.SHA384].FromHexString() : null,
                    SHA512 = hashDict.ContainsKey(HashType.SHA512) ? hashDict[HashType.SHA512].FromHexString() : null,
                    SpamSum = hashDict.ContainsKey(HashType.SpamSum) ? hashDict[HashType.SpamSum].FromHexString() : null,
                };

                // Deal with the input stream and return
                input.SeekIfPossible();
                return baseFile;
            }
            catch (Exception ex)
            {
                _staticLogger.Error(ex);
                return new BaseFile();
            }
        }

        /// <summary>
        /// Copy all missing information from one BaseFile to another
        /// </summary>
        private static void CopyInformation(BaseFile from, BaseFile to)
        {
            to.Filename ??= from.Filename;
            to.Parent ??= from.Parent;
            to.Date ??= from.Date;
            to.Size ??= from.Size;
            to.CRC ??= from.CRC;
            to.MD2 ??= from.MD2;
            to.MD4 ??= from.MD4;
            to.MD5 ??= from.MD5;
            to.RIPEMD128 ??= from.RIPEMD128;
            to.RIPEMD160 ??= from.RIPEMD160;
            to.SHA1 ??= from.SHA1;
            to.SHA256 ??= from.SHA256;
            to.SHA384 ??= from.SHA384;
            to.SHA512 ??= from.SHA512;
            to.SpamSum ??= from.SpamSum;
        }

        /// <summary>
        /// Get the correct base file based on the type and filter options
        /// </summary>
        private static BaseFile? GetBaseFile(Stream input, FileType? fileType, HashType[] hashes)
        {
            // Get external file information
            BaseFile? baseFile = GetInfo(input, hashes);

            // Get internal hashes, if they exist
            if (fileType == FileType.AaruFormat)
            {
                AaruFormat? aif = AaruFormat.Create(input);
                if (aif != null)
                {
                    CopyInformation(baseFile, aif);
                    return aif;
                }
            }
            else if (fileType == FileType.CHD)
            {
                CHDFile? chd = CHDFile.Create(input);
                if (chd != null)
                {
                    CopyInformation(baseFile, chd);
                    return chd;
                }
            }

            return baseFile;
        }

        /// <summary>
        /// Get the required stream for info hashing
        /// </summary>
        private static Stream GetInfoStream(string input, string? header)
        {
            // Open the file directly
            Stream inputStream = File.Open(input, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            if (header == null)
                return inputStream;

            // Try to match the supplied header skipper
            SkipperMatch.Init();
            var rule = SkipperMatch.GetMatchingRule(input, Path.GetFileNameWithoutExtension(header));

            // If there's no match, return the original stream
            if (rule.Tests == null || rule.Tests.Length == 0)
                return inputStream;

            // Transform the stream and get the information from it
            var outputStream = new MemoryStream();
            rule.TransformStream(inputStream, outputStream, keepReadOpen: false, keepWriteOpen: true);
            return outputStream;
        }

        #endregion

        #region File Type

        /// <summary>
        /// Create an archive object from a filename, if possible
        /// </summary>
        /// <param name="input">Name of the file to create the archive from</param>
        /// <returns>Archive object representing the inputs</returns>
        public static BaseArchive? CreateArchiveType(string input)
        {
            FileType? fileType = GetFileType(input);
            return fileType switch
            {
                FileType.GZipArchive => new GZipArchive(input),
                FileType.RarArchive => new RarArchive(input),
                FileType.SevenZipArchive => new SevenZipArchive(input),
                FileType.TapeArchive => new TapeArchive(input),
                FileType.ZipArchive => new ZipArchive(input),
                _ => null,
            };
        }

        /// <summary>
        /// Create an IFolder object of the specified type, if possible
        /// </summary>
        /// <param name="outputFormat">OutputFormat representing the archive to create</param>
        /// <returns>IFolder object representing the inputs</returns>
        public static IParent? CreateFolderType(OutputFormat outputFormat)
        {
            return outputFormat switch
            {
                OutputFormat.Folder => new Folder(false),
                OutputFormat.ParentFolder => new Folder(true),
                OutputFormat.TapeArchive => new TapeArchive(),
                OutputFormat.Torrent7Zip => new SevenZipArchive(),
                OutputFormat.TorrentGzip => new GZipArchive(),
                OutputFormat.TorrentGzipRomba => new GZipArchive(),
                OutputFormat.TorrentRar => new RarArchive(),
                OutputFormat.TorrentXZ => new XZArchive(),
                OutputFormat.TorrentXZRomba => new XZArchive(),
                OutputFormat.TorrentZip => new ZipArchive(),
                _ => null,
            };
        }

        /// <summary>
        /// Returns the file type of an input file
        /// </summary>
        /// <param name="input">Input file to check</param>
        /// <returns>FileType of inputted file (null on error)</returns>
        public static FileType? GetFileType(string input)
        {
            // If the file is null, then we have no archive type
            if (string.IsNullOrEmpty(input))
                return null;

            // First line of defense is going to be the extension, for better or worse
            if (!HasValidArchiveExtension(input))
                return null;

            // Read the first bytes of the file and get the magic number
            byte[] magic;
            try
            {
                using Stream stream = File.Open(input, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                magic = stream.ReadBytes(8);
            }
            catch (Exception ex)
            {
                _staticLogger.Error(ex);
                return null;
            }

            // Now try to match it to a known signature
            if (magic.StartsWith(SevenZipSignature))
            {
                return FileType.SevenZipArchive;
            }
            else if (magic.StartsWith(AaruFormatSignature))
            {
                return FileType.AaruFormat;
            }
            else if (magic.StartsWith(CHDSignature))
            {
                return FileType.CHD;
            }
            else if (magic.StartsWith(GzSignature))
            {
                return FileType.GZipArchive;
            }
            else if (magic.StartsWith(RarSignature)
                || magic.StartsWith(RarFiveSignature))
            {
                return FileType.RarArchive;
            }
            else if (magic.StartsWith(TarSignature)
                || magic.StartsWith(TarZeroSignature))
            {
                return FileType.TapeArchive;
            }
            else if (magic.StartsWith(XZSignature))
            {
                return FileType.XZArchive;
            }
            else if (magic.StartsWith(Models.PKZIP.Constants.LocalFileHeaderSignatureBytes)
                || magic.StartsWith(Models.PKZIP.Constants.EndOfCentralDirectoryRecordSignatureBytes)
                || magic.StartsWith(Models.PKZIP.Constants.DataDescriptorSignatureBytes))
            {
                return FileType.ZipArchive;
            }

            return null;
        }

        /// <summary>
        /// Get if the given path has a valid DAT extension
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the extension is valid, false otherwise</returns>
        private static bool HasValidArchiveExtension(string path)
        {
            // Get the extension from the path, if possible
            string? ext = path.GetNormalizedExtension();

            // Check against the list of known archive extensions
            return ext switch
            {
                // Aaruformat
                "aaru" => true,
                "aaruf" => true,
                "aaruformat" => true,
                "aif" => true,
                "dicf" => true,

                // Archive
                "7z" => true,
                "gz" => true,
                "lzma" => true,
                "rar" => true,
                "rev" => true,
                "r00" => true,
                "r01" => true,
                "tar" => true,
                "tgz" => true,
                "tlz" => true,
                "zip" => true,
                "zipx" => true,

                // CHD
                "chd" => true,

                _ => false,
            };
        }

        #endregion
    }
}