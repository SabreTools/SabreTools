using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using SharpCompress.Compressors.Xz;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// Represents a TorrentXZ archive for reading and writing
    /// </summary>
    public class XZArchive : BaseArchive
    {
        #region Constructors

        /// <summary>
        /// Create a new TorrentGZipArchive with no base file
        /// </summary>
        public XZArchive()
            : base()
        {
            this.Type = FileType.XZArchive;
        }

        /// <summary>
        /// Create a new TorrentGZipArchive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        /// <param name="read">True for opening file as read, false for opening file as write</param>
        /// <param name="getHashes">True if hashes for this file should be calculated, false otherwise (default)</param>
        public XZArchive(string filename, bool getHashes = false)
            : base(filename, getHashes)
        {
            this.Type = FileType.XZArchive;
        }

        #endregion

        #region Extraction

        /// <summary>
        /// Attempt to extract a file as an archive
        /// </summary>
        /// <param name="outDir">Output directory for archive extraction</param>
        /// <returns>True if the extraction was a success, false otherwise</returns>
        public override bool CopyAll(string outDir)
        {
            bool encounteredErrors = true;

            try
            {
                // Create the temp directory
                Directory.CreateDirectory(outDir);

                // Decompress the _filename stream
                FileStream outstream = FileExtensions.TryCreate(Path.Combine(outDir, Path.GetFileNameWithoutExtension(this.Filename)));
                var xz = new XZStream(File.OpenRead(this.Filename));
                xz.CopyTo(outstream);

                // Dispose of the streams
                outstream.Dispose();
                xz.Dispose();

                encounteredErrors = false;
            }
            catch (EndOfStreamException)
            {
                // Catch this but don't count it as an error because SharpCompress is unsafe
            }
            catch (InvalidOperationException)
            {
                encounteredErrors = true;
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                encounteredErrors = true;
            }

            return encounteredErrors;
        }

        /// <summary>
        /// Attempt to extract a file from an archive
        /// </summary>
        /// <param name="entryName">Name of the entry to be extracted</param>
        /// <param name="outDir">Output directory for archive extraction</param>
        /// <returns>Name of the extracted file, null on error</returns>
        public override string CopyToFile(string entryName, string outDir)
        {
            // Try to extract a stream using the given information
            (MemoryStream ms, string realEntry) = CopyToStream(entryName);

            // If the memory stream and the entry name are both non-null, we write to file
            if (ms != null && realEntry != null)
            {
                realEntry = Path.Combine(outDir, realEntry);

                // Create the output subfolder now
                Directory.CreateDirectory(Path.GetDirectoryName(realEntry));

                // Now open and write the file if possible
                FileStream fs = FileExtensions.TryCreate(realEntry);
                if (fs != null)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    byte[] zbuffer = new byte[_bufferSize];
                    int zlen;
                    while ((zlen = ms.Read(zbuffer, 0, _bufferSize)) > 0)
                    {
                        fs.Write(zbuffer, 0, zlen);
                        fs.Flush();
                    }

                    ms?.Dispose();
                    fs?.Dispose();
                }
                else
                {
                    ms?.Dispose();
                    fs?.Dispose();
                    realEntry = null;
                }
            }

            return realEntry;
        }

        /// <summary>
        /// Attempt to extract a stream from an archive
        /// </summary>
        /// <param name="entryName">Name of the entry to be extracted</param>
        /// <param name="realEntry">Output representing the entry name that was found</param>
        /// <returns>MemoryStream representing the entry, null on error</returns>
        public override (MemoryStream, string) CopyToStream(string entryName)
        {
            MemoryStream ms = new MemoryStream();
            string realEntry;

            try
            {
                // Decompress the _filename stream
                realEntry = Path.GetFileNameWithoutExtension(this.Filename);
                var xz = new XZStream(File.OpenRead(this.Filename));

                // Write the file out
                byte[] xbuffer = new byte[_bufferSize];
                int xlen;
                while ((xlen = xz.Read(xbuffer, 0, _bufferSize)) > 0)
                {

                    ms.Write(xbuffer, 0, xlen);
                    ms.Flush();
                }

                // Dispose of the streams
                xz.Dispose();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                ms = null;
                realEntry = null;
            }

            return (ms, realEntry);
        }

        #endregion

        #region Information

        /// <summary>
        /// Generate a list of DatItem objects from the header values in an archive
        /// </summary>
        /// <param name="omitFromScan">Hash representing the hashes that should be skipped</param>
        /// <param name="date">True if entry dates should be included, false otherwise (default)</param>
        /// <returns>List of DatItem objects representing the found data</returns>
        /// <remarks>TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually</remarks>
        public override List<BaseFile> GetChildren(Hash omitFromScan = Hash.DeepHashes, bool date = false)
        {
            if (_children == null || _children.Count == 0)
            {
                _children = new List<BaseFile>();

                string gamename = Path.GetFileNameWithoutExtension(this.Filename);

                BaseFile possibleTxz = GetTorrentXZFileInfo();

                // If it was, then add it to the outputs and continue
                if (possibleTxz != null && possibleTxz.Filename != null)
                {
                    _children.Add(possibleTxz);
                }
                else
                {
                    try
                    {
                        // If secure hashes are disabled, do a quickscan
                        if (omitFromScan == Hash.SecureHashes)
                        {
                            BaseFile tempRom = new BaseFile()
                            {
                                Filename = gamename,
                            };
                            BinaryReader br = new BinaryReader(FileExtensions.TryOpenRead(this.Filename));
                            br.BaseStream.Seek(-8, SeekOrigin.End);
                            tempRom.CRC = br.ReadBytesBigEndian(4);
                            tempRom.Size = br.ReadInt32BigEndian();
                            br.Dispose();

                            _children.Add(tempRom);
                        }
                        // Otherwise, use the stream directly
                        else
                        {
                            var xzStream = new XZStream(File.OpenRead(this.Filename));
                            BaseFile xzEntryRom = xzStream.GetInfo(omitFromScan: omitFromScan);
                            xzEntryRom.Filename = gamename;
                            xzEntryRom.Parent = gamename;
                            _children.Add(xzEntryRom);
                            xzStream.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Globals.Logger.Error(ex.ToString());
                        return null;
                    }
                }
            }

            return _children;
        }

        /// <summary>
        /// Generate a list of empty folders in an archive
        /// </summary>
        /// <param name="input">Input file to get data from</param>
        /// <returns>List of empty folders in the archive</returns>
        public override List<string> GetEmptyFolders()
        {
            // XZ files don't contain directories
            return new List<string>();
        }

        /// <summary>
        /// Check whether the input file is a standardized format
        /// </summary>
        public override bool IsTorrent()
        {
            // Check for the file existing first
            if (!File.Exists(this.Filename))
                return false;

            string datum = Path.GetFileName(this.Filename).ToLowerInvariant();

            // Check if the name is the right length
            if (!Regex.IsMatch(datum, @"^[0-9a-f]{" + Constants.SHA1Length + @"}\.xz")) // TODO: When updating to SHA-256, this needs to update to Constants.SHA256Length
            {
                Globals.Logger.Warning($"Non SHA-1 filename found, skipping: '{Path.GetFullPath(this.Filename)}'");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieve file information for a single torrent XZ file
        /// </summary>
        /// <returns>Populated DatItem object if success, empty one on error</returns>
        public BaseFile GetTorrentXZFileInfo()
        {
            // Check for the file existing first
            if (!File.Exists(this.Filename))
                return null;

            string datum = Path.GetFileName(this.Filename).ToLowerInvariant();

            // Check if the name is the right length
            if (!Regex.IsMatch(datum, @"^[0-9a-f]{" + Constants.SHA1Length + @"}\.xz")) // TODO: When updating to SHA-256, this needs to update to Constants.SHA256Length
            {
                Globals.Logger.Warning($"Non SHA-1 filename found, skipping: '{Path.GetFullPath(this.Filename)}'");
                return null;
            }

            BaseFile baseFile = new BaseFile
            {
                Filename = Path.GetFileNameWithoutExtension(this.Filename).ToLowerInvariant(),
                SHA1 = Utilities.StringToByteArray(Path.GetFileNameWithoutExtension(this.Filename)), // TODO: When updating to SHA-256, this needs to update to SHA256

                Parent = Path.GetFileNameWithoutExtension(this.Filename).ToLowerInvariant(),
            };

            return baseFile;
        }

        #endregion

        #region Writing

        /// <summary>
        /// Write an input file to a torrent XZ file
        /// </summary>
        /// <param name="inputFile">Input filename to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
        /// <returns>True if the write was a success, false otherwise</returns>
        /// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
        public override bool Write(string inputFile, string outDir, Rom rom, bool date = false, bool romba = false)
        {
            // Check that the input file exists
            if (!File.Exists(inputFile))
            {
                Globals.Logger.Warning($"File '{inputFile}' does not exist!");
                return false;
            }

            inputFile = Path.GetFullPath(inputFile);

            // Get the file stream for the file and write out
            return Write(FileExtensions.TryOpenRead(inputFile), outDir, rom, date, romba);
        }

        /// <summary>
        /// Write an input file to a torrent XZ archive
        /// </summary>
        /// <param name="inputStream">Input stream to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
        /// <returns>True if the archive was written properly, false otherwise</returns>
        public override bool Write(Stream inputStream, string outDir, Rom rom, bool date = false, bool romba = false)
        {
            bool success = false;

            // If the stream is not readable, return
            if (!inputStream.CanRead)
                return success;

            // Make sure the output directory exists
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            outDir = Path.GetFullPath(outDir);

            // Now get the Rom info for the file so we have hashes and size
            rom = new Rom(inputStream.GetInfo(keepReadOpen: true));

            // Get the output file name
            string outfile;

            // If we have a romba output, add the romba path
            if (romba)
            {
                outfile = Path.Combine(outDir, PathExtensions.GetRombaPath(rom.SHA1)); // TODO: When updating to SHA-256, this needs to update to SHA256

                // Check to see if the folder needs to be created
                if (!Directory.Exists(Path.GetDirectoryName(outfile)))
                    Directory.CreateDirectory(Path.GetDirectoryName(outfile));
            }
            // Otherwise, we're just rebuilding to the main directory
            else
            {
                outfile = Path.Combine(outDir, rom.SHA1 + ".xz"); // TODO: When updating to SHA-256, this needs to update to SHA256
            }

            // If the output file exists, don't try to write again
            if (!File.Exists(outfile))
            {
                // Compress the input stream
                XZStream outputStream = new XZStream(FileExtensions.TryCreate(outfile));
                inputStream.CopyTo(outputStream);

                // Dispose of everything
                outputStream.Dispose();
                inputStream.Dispose();
            }

            return true;
        }

        /// <summary>
        /// Write a set of input files to a torrent XZ archive (assuming the same output archive name)
        /// </summary>
        /// <param name="inputFiles">Input files to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
        /// <returns>True if the archive was written properly, false otherwise</returns>
        public override bool Write(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false, bool romba = false)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
