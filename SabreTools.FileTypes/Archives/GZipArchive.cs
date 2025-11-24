using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Compress;
using Compress.gZip;
using Compress.Support.Compression.Deflate;
using SabreTools.Core.Tools;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;

namespace SabreTools.FileTypes.Archives
{
    /// <summary>
    /// Represents a TorrentGZip archive for reading and writing
    /// </summary>
    public class GZipArchive : BaseArchive
    {
        #region Constants

        /* (Torrent)GZ Header Format
            https://tools.ietf.org/html/rfc1952

            00-01		Identification (0x1F, 0x8B) GzSignature
            02			Compression Method (0-7 reserved, 8 deflate; 0x08)
            03			Flags (0 FTEXT, 1 FHCRC, 2 FEXTRA, 3 FNAME, 4 FCOMMENT, 5 reserved, 6 reserved, 7 reserved; 0x04)
            04-07		Modification time (Unix format; 0x00, 0x00, 0x00, 0x00)
            08			Extra Flags (2 maximum compression, 4 fastest algorithm; 0x00)
            09			OS (See list on https://tools.ietf.org/html/rfc1952; 0x00)
            0A-0B		Length of extra field (mirrored; 0x1C, 0x00)
            0C-27		Extra field
                0C-1B	MD5 Hash
                1C-1F	CRC hash
                20-27	Int64 size (mirrored)
        */
        private readonly static byte[] TorrentGZHeader = [0x1f, 0x8b, 0x08, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1c, 0x00];

        #endregion

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
        public GZipArchive()
            : base()
        {
        }

        /// <summary>
        /// Create a new TorrentGZipArchive from the given file
        /// </summary>
        public GZipArchive(string filename)
            : base(filename)
        {
        }

        #endregion

        #region Extraction

        /// <inheritdoc/>
        public override bool CopyAll(string outDir)
        {
            bool encounteredErrors = true;

            // If we have an invalid file
            if (Filename == null)
                return true;

            try
            {
                // Open the input file
                using var inputFile = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                var gzip = Serialization.Wrappers.GZip.Create(inputFile);

                // Write the output file
                encounteredErrors = !gzip.Extract(outDir, includeDebug: false);
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
            string? dir = Path.GetDirectoryName(realEntry);
            if (dir != null)
                Directory.CreateDirectory(dir);

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

            try
            {
                // Open the entry stream
                realEntry = Path.GetFileNameWithoutExtension(Filename);
                var gz = new gZip();
                ZipReturn ret = gz.ZipFileOpen(Filename);
                ret = gz.ZipFileOpenReadStream(0, out Stream? stream, out ulong streamSize);

                // Return the stream
                return stream;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }

        #endregion

        #region Information

        /// <inheritdoc/>
        public override List<BaseFile>? GetChildren()
        {
            // If we have an invalid file
            if (Filename == null)
                return null;

            // If we have children cached already
            if (_children != null && _children.Count > 0)
                return _children;

            _children = [];

            string gamename = Path.GetFileNameWithoutExtension(Filename);

            BaseFile? possibleTgz = GetTorrentGZFileInfo();

            // If it was, then add it to the outputs and continue
            if (possibleTgz != null && possibleTgz.Filename != null)
            {
                _children.Add(possibleTgz);
                return _children;
            }

            try
            {
                // Create a blank item for the entry
                BaseFile gzipEntryRom = new();

                // Perform a quickscan, if flagged to
                if (_hashTypes.Length == 1 && _hashTypes[0] == HashType.CRC32)
                {
                    gzipEntryRom.Filename = gamename;

                    using Stream stream = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    stream.Seek(-8, SeekOrigin.End);
                    gzipEntryRom.CRC = stream.ReadBytes(4);
                    Array.Reverse(gzipEntryRom.CRC);
                    gzipEntryRom.Size = stream.ReadInt32BigEndian();
                }
                // Otherwise, use the stream directly
                else
                {
                    var gz = new gZip();
                    ZipReturn ret = gz.ZipFileOpen(Filename);
                    ret = gz.ZipFileOpenReadStream(0, out Stream? gzstream, out ulong streamSize);
                    gzipEntryRom = FileTypeTool.GetInfo(gzstream, _hashTypes);
                    gzipEntryRom.Filename = gz.GetLocalFile(0).Filename;
                    gzipEntryRom.Parent = gamename;
                    gzipEntryRom.Date = (gz.TimeStamp > 0 ? gz.TimeStamp.ToString() : null);
                    gzstream?.Dispose();
                }

                // Fill in common details and add to the list
                gzipEntryRom.Parent = gamename;
                _children.Add(gzipEntryRom);
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }

            return _children;
        }

        /// <inheritdoc/>
        public override List<string> GetEmptyFolders()
        {
            // GZip files don't contain directories
            return [];
        }

        /// <inheritdoc/>
        public override bool IsStandardized()
        {
            // Check for the file existing first
            if (Filename == null || !File.Exists(Filename))
                return false;

            string datum = Path.GetFileName(Filename).ToLowerInvariant();
            long filesize = new FileInfo(Filename).Length;

            // If we have the romba depot files, just skip them gracefully
            if (datum == ".romba_size" || datum == ".romba_size.backup")
            {
                _logger.Verbose($"Romba depot file found, skipping: {Filename}");
                return false;
            }

            // Check if the name is the right length
            if (!Regex.IsMatch(datum, @"^[0-9a-f]{" + Hashing.Constants.SHA1Length + @"}\.gz"))
            {
                _logger.Warning($"Non SHA-1 filename found, skipping: '{Path.GetFullPath(Filename)}'");
                return false;
            }

            // Check if the file is at least the minimum length
            if (filesize < 40 /* bytes */)
            {
                _logger.Warning($"Possibly corrupt file '{Path.GetFullPath(Filename)}' with size {filesize}");
                return false;
            }

            // Get the Romba-specific header data
            Stream stream = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            byte[] header = stream.ReadBytes(12); // Get preamble header for checking
            _ = stream.ReadBytes(16); // headermd5
            _ = stream.ReadBytes(4); // headercrc
            _ = stream.ReadUInt64(); // headersz
            stream.Dispose();

            // If the header is not correct, return a blank rom
            bool correct = true;
            for (int i = 0; i < header.Length; i++)
            {
                // This is a temp fix to ignore the modification time and OS until romba can be fixed
                if (i == 4 || i == 5 || i == 6 || i == 7 || i == 9)
                    continue;

                correct &= (header[i] == TorrentGZHeader[i]);
            }

            if (!correct)
                return false;

            return true;
        }

        /// <summary>
        /// Retrieve file information for a single torrent GZ file
        /// </summary>
        /// <returns>Populated DatItem object if success, empty one on error</returns>
        public BaseFile? GetTorrentGZFileInfo()
        {
            // Check for the file existing first
            if (Filename == null || !File.Exists(Filename))
                return null;

            string datum = Path.GetFileName(Filename).ToLowerInvariant();
            long filesize = new FileInfo(Filename).Length;

            // If we have the romba depot files, just skip them gracefully
            if (datum == ".romba_size" || datum == ".romba_size.backup")
            {
                _logger.Verbose($"Romba depot file found, skipping: {Filename}");
                return null;
            }

            // Check if the name is the right length
            if (!Regex.IsMatch(datum, @"^[0-9a-f]{" + Hashing.Constants.SHA1Length + @"}\.gz"))
            {
                _logger.Warning($"Non SHA-1 filename found, skipping: '{Path.GetFullPath(Filename)}'");
                return null;
            }

            // Check if the file is at least the minimum length
            if (filesize < 40 /* bytes */)
            {
                _logger.Warning($"Possibly corrupt file '{Path.GetFullPath(Filename)}' with size {filesize}");
                return null;
            }

            // Get the Romba-specific header data
            byte[] header; // Get preamble header for checking
            byte[] headermd5; // MD5
            byte[] headercrc; // CRC
            ulong headersz; // Int64 size
            Stream stream = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            header = stream.ReadBytes(12);
            headermd5 = stream.ReadBytes(16);
            headercrc = stream.ReadBytes(4);
            headersz = stream.ReadUInt64();
            stream.Dispose();

            // If the header is not correct, return a blank rom
            bool correct = true;
            for (int i = 0; i < header.Length; i++)
            {
                // This is a temp fix to ignore the modification time and OS until romba can be fixed
                if (i == 4 || i == 5 || i == 6 || i == 7 || i == 9)
                {
                    continue;
                }
                correct &= (header[i] == TorrentGZHeader[i]);
            }

            if (!correct)
                return null;

            // Now convert the data and get the right position
            long extractedsize = (long)headersz;

            BaseFile baseFile = new()
            {
                Filename = Path.GetFileNameWithoutExtension(Filename).ToLowerInvariant(),
                Size = extractedsize,
                CRC = headercrc,
                MD5 = headermd5,
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
            string outfile = Path.Combine(outDir, Utilities.GetDepotPath(baseFile.SHA1, Depth) ?? string.Empty);

            // Check to see if the folder needs to be created
            if (!Directory.Exists(Path.GetDirectoryName(outfile)))
                Directory.CreateDirectory(Path.GetDirectoryName(outfile)!);

            // If the output file exists, don't try to write again
            if (!File.Exists(outfile))
            {
                // Compress the input stream
                FileStream outputStream = File.Create(outfile);

                // Open the output file for writing
                BinaryWriter sw = new(outputStream);

                // Write standard header and TGZ info
                byte[] data = [.. TorrentGZHeader, .. baseFile!.MD5!, .. baseFile.CRC!];
                sw.Write(data);
                sw.Write((ulong)(baseFile.Size ?? 0)); // Long size (Unsigned, Mirrored)

                // Now create a deflatestream from the input file
                ZlibBaseStream ds = new(outputStream, CompressionMode.Compress, CompressionLevel.BestCompression, ZlibStreamFlavor.DEFLATE, true);

                // Copy the input stream to the output
                byte[] ibuffer = new byte[_bufferSize];
                int ilen;
                while ((ilen = stream.Read(ibuffer, 0, _bufferSize)) > 0)
                {
                    ds.Write(ibuffer, 0, ilen);
                    ds.Flush();
                }

                ds.Dispose();

                // Now write the standard footer
                sw.Write([.. Enumerable.Reverse(baseFile.CRC)]);
                sw.Write((uint)(baseFile.Size ?? 0));

                // Dispose of everything
#if NET40_OR_GREATER
                sw.Dispose();
#endif
                outputStream.Dispose();
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool Write(List<string> files, string outDir, List<BaseFile>? baseFile)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
