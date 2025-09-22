using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#if NET462_OR_GREATER || NETCOREAPP
using SabreTools.Hashing;
#endif
using SabreTools.IO.Extensions;
#if NET462_OR_GREATER || NETCOREAPP
using SharpCompress.Compressors.Xz;
#endif

namespace SabreTools.FileTypes.Archives
{
    /// <summary>
    /// Represents a TorrentXZ archive for reading and writing
    /// </summary>
    public class XZArchive : BaseArchive
    {
        /* (Torrent)XZ Header Format
            https://tukaani.org/xz/xz-file-format.txt

            00-05		Identification (0xFD, '7', 'z', 'X', 'Z', 0x00) XzSignature
            06			Flags (0x01 - CRC32, 0x04 - CRC64, 0x0A - SHA-256)
            07-0A		Flags CRC32 (uint, little-endian)
        */

        #region Fields

        /// <summary>
        /// Positive value for depth of the output depot, defaults to 4
        /// </summary>
        public int Depth { get; set; } = 4;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new TorrentGZipArchive with no base file
        /// </summary>
        public XZArchive()
            : base()
        {
        }

        /// <summary>
        /// Create a new TorrentGZipArchive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        public XZArchive(string filename)
            : base(filename)
        {
        }

        #endregion

        #region Extraction

        /// <inheritdoc/>
        public override bool CopyAll(string outDir)
        {
#if NET462_OR_GREATER || NETCOREAPP
            bool encounteredErrors = true;

            try
            {
                // Create the temp directory
                Directory.CreateDirectory(outDir);

                // Decompress the _filename stream
                FileStream outstream = File.Create(Path.Combine(outDir, Path.GetFileNameWithoutExtension(Filename)!));
                var xz = new XZStream(File.Open(Filename!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                xz.CopyTo(outstream);

                // Dispose of the streams
                outstream.Dispose();
                xz.Dispose();

                encounteredErrors = false;
            }
            catch (EndOfStreamException ex)
            {
                // Catch this but don't count it as an error because SharpCompress is unsafe
                _logger.Verbose(ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Warning(ex);
                encounteredErrors = true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                encounteredErrors = true;
            }

            return encounteredErrors;
#else
            // TODO: Support XZ archives in old .NET
            return true;
#endif
        }

        /// <inheritdoc/>
        public override string? CopyToFile(string entryName, string outDir)
        {
            // Try to extract a stream using the given information
            var stream = GetEntryStream(entryName, out string? realEntry);
            if (stream == null || realEntry == null)
                return null;

            // If the stream and the entry name are both non-null, we write to file
            realEntry = Path.Combine(outDir, realEntry);

            // Create the output subfolder now
            Directory.CreateDirectory(Path.GetDirectoryName(realEntry)!);

            // Now open and write the file if possible
            FileStream fs = File.Create(realEntry);
            if (fs != null)
            {
                if (stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);

                byte[] zbuffer = new byte[_bufferSize];
                int zlen;
                while ((zlen = stream.Read(zbuffer, 0, _bufferSize)) > 0)
                {
                    fs.Write(zbuffer, 0, zlen);
                    fs.Flush();
                }

                stream?.Dispose();
                fs?.Dispose();
            }
            else
            {
                stream?.Dispose();
                fs?.Dispose();
                realEntry = null;
            }

            return realEntry;
        }

        /// <inheritdoc/>
        public override Stream? GetEntryStream(string entryName, out string? realEntry)
        {
            // Set the default real entry name
            realEntry = null;

            // If we have an invalid file
            if (Filename == null)
                return null;

#if NET462_OR_GREATER || NETCOREAPP
            try
            {
                // Open the entry stream
                realEntry = Path.GetFileNameWithoutExtension(Filename);
                var stream = new XZStream(File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));

                // Return the stream
                return stream;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
#else
            // TODO: Support XZ archives in old .NET
            return null;
#endif
        }

        #endregion

        #region Information

        /// <inheritdoc/>
        public override List<BaseFile>? GetChildren()
        {
            // If we have children cached already
            if (_children != null && _children.Count > 0)
                return _children;

#if NET462_OR_GREATER || NETCOREAPP
            _children = [];

            string? gamename = Path.GetFileNameWithoutExtension(Filename);
            BaseFile? possibleTxz = GetTorrentXZFileInfo();

            // If it was, then add it to the outputs and continue
            if (possibleTxz != null && possibleTxz.Filename != null)
            {
                _children.Add(possibleTxz);
                return _children;
            }

            try
            {
                // Create a blank item for the entry
                BaseFile xzEntryRom = new();

                // Perform a quickscan, if flagged to
                if (_hashTypes.Length == 1 && _hashTypes[0] == HashType.CRC32)
                {
                    xzEntryRom.Filename = gamename;

                    using Stream fs = File.Open(Filename!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    fs.Seek(-8, SeekOrigin.End);
                    xzEntryRom.CRC = fs.ReadBytes(4);
                    Array.Reverse(xzEntryRom.CRC);
                    xzEntryRom.Size = fs.ReadInt32BigEndian();
                }
                // Otherwise, use the stream directly
                else
                {
                    var xzStream = new XZStream(File.Open(Filename!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                    xzEntryRom = FileTypeTool.GetInfo(xzStream, _hashTypes);
                    xzEntryRom.Filename = gamename;
                    xzStream.Dispose();
                }

                // Fill in common details and add to the list
                xzEntryRom.Parent = gamename;
                _children.Add(xzEntryRom);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }

            return _children;
#else
            // TODO: Support XZ archives in old .NET
            return [];
#endif
        }

        /// <inheritdoc/>
        public override List<string> GetEmptyFolders()
        {
            // XZ files don't contain directories
            return [];
        }

        /// <inheritdoc/>
        public override bool IsStandardized()
        {
            // Check for the file existing first
            if (Filename == null || !File.Exists(Filename))
                return false;

            string datum = Path.GetFileName(Filename).ToLowerInvariant();

            // Check if the name is the right length
            if (!Regex.IsMatch(datum, @"^[0-9a-f]{" + Hashing.Constants.SHA1Length + @"}\.xz"))
            {
                _logger.Warning($"Non SHA-1 filename found, skipping: '{Path.GetFullPath(Filename)}'");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieve file information for a single torrent XZ file
        /// </summary>
        /// <returns>Populated DatItem object if success, empty one on error</returns>
        public BaseFile? GetTorrentXZFileInfo()
        {
            // Check for the file existing first
            if (Filename == null || !File.Exists(Filename))
                return null;

            string datum = Path.GetFileName(Filename).ToLowerInvariant();

            // Check if the name is the right length
            if (!Regex.IsMatch(datum, @"^[0-9a-f]{" + Hashing.Constants.SHA1Length + @"}\.xz"))
            {
                _logger.Warning($"Non SHA-1 filename found, skipping: '{Path.GetFullPath(Filename)}'");
                return null;
            }

            BaseFile baseFile = new()
            {
                Filename = Path.GetFileNameWithoutExtension(Filename).ToLowerInvariant(),
                SHA1 = Path.GetFileNameWithoutExtension(Filename).FromHexString(),

                Parent = Path.GetFileNameWithoutExtension(Filename).ToLowerInvariant(),
            };

            return baseFile;
        }

        #endregion

        #region Writing

        /// <inheritdoc/>
        public override bool Write(string file, string outDir, BaseFile? baseFile)
        {
            // Check that the input file exists
            if (!File.Exists(file))
            {
                _logger.Warning($"File '{file}' does not exist!");
                return false;
            }

            file = Path.GetFullPath(file);

            // Get the file stream for the file and write out
            using Stream inputStream = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Write(inputStream, outDir, baseFile);
        }

        /// <inheritdoc/>
        public override bool Write(Stream? stream, string outDir, BaseFile? baseFile)
        {
#if NET462_OR_GREATER || NETCOREAPP
            bool success = false;

            // If the stream is not readable, return
            if (stream == null || !stream.CanRead)
                return success;

            // Make sure the output directory exists
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            outDir = Path.GetFullPath(outDir);

            // If the base file is null, get the hash information
            baseFile ??= FileTypeTool.GetInfo(stream, _hashTypes);

            // Get the output file name
            string outfile = Path.Combine(outDir, Core.Tools.Utilities.GetDepotPath(baseFile.SHA1, Depth)!);
            outfile = outfile.Replace(".gz", ".xz");

            // Check to see if the folder needs to be created
            if (!Directory.Exists(Path.GetDirectoryName(outfile)))
                Directory.CreateDirectory(Path.GetDirectoryName(outfile)!);

            // If the output file exists, don't try to write again
            if (!File.Exists(outfile))
            {
                // Compress the input stream
                XZStream outputStream = new(File.Create(outfile));
                stream.CopyTo(outputStream);

                // Dispose of everything
                outputStream.Dispose();
            }

            return true;
#else
            // TODO: Support XZ archives in old .NET
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool Write(List<string> files, string outDir, List<BaseFile>? baseFiles)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
