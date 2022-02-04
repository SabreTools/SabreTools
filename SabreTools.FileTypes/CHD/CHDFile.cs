using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

using SabreTools.Core.Tools;
using SabreTools.IO;

// TODO: add check
namespace SabreTools.FileTypes.CHD
{
    /// <summary>
    /// This is code adapted from chd.h and chd.cpp in MAME
    /// Additional archival code from https://github.com/rtissera/libchdr/blob/master/src/chd.h
    /// </summary>
    public class CHDFile : BaseArchive
    {
        #region Private instance variables

        // Common header fields
        protected char[] tag = new char[8]; // 'MComprHD'
        protected uint length;              // length of header (including tag and length fields)
        protected uint version;             // drive format version

        protected CHDFile generated;
        protected string file;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new CHD File with no base file
        /// </summary>
        public CHDFile()
            : base()
        {
            Type = FileType.CHD;
        }

        /// <summary>
        /// Create a new CHD File  from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        /// <param name="read">True for opening file as read, false for opening file as write</param>
        /// <param name="getHashes">True if hashes for this file should be calculated, false otherwise (default)</param>
        public CHDFile(string filename, bool getHashes = false)
            : base(filename, getHashes)
        {
            try
            {
                using (FileStream chdstream = File.OpenRead(filename))
                {

                    // Read the standard CHD headers
                    (char[] tag, uint length, uint version) = GetHeaderValues(chdstream);
                    chdstream.SeekIfPossible(); // Seek back to start

                    // Validate that this is actually a valid CHD
                    uint validatedVersion = ValidateHeader(tag, length, version);
                    if (validatedVersion == 0)
                        this.Type = FileType.None;

                    // Read and retrun the current CHD
                    generated = ReadAsVersion(chdstream, version);

                    if (generated != null)
                        generated.Type = FileType.CHD;

                    this.Type = generated.Type;
                }
            }
            catch
            {
                this.Type = FileType.None;
            }
        }

       public CHDFile(Stream chdstream)
            : base()
        {
            try
            {
                // Read the standard CHD headers
                (char[] tag, uint length, uint version) = GetHeaderValues(chdstream);
                chdstream.SeekIfPossible(); // Seek back to start

                // Validate that this is actually a valid CHD
                uint validatedVersion = ValidateHeader(tag, length, version);
                if (validatedVersion == 0)
                    this.Type = FileType.None;
                else
                {
                    // Read and retrun the current CHD
                    generated = ReadAsVersion(chdstream, version);

                    chdstream.Dispose();

                    if (generated != null)
                        generated.Type = FileType.CHD;

                    this.Type = generated.Type;
                }
                chdstream?.Dispose();
            }
            catch
            {
                chdstream?.Dispose();
                this.Type = FileType.None;
            }
        }

        #endregion

        #region Abstract functionality

        /// <summary>
        /// Return the best-available hash for a particular CHD version
        /// </summary>
//        public abstract byte[] GetHash();

        #endregion

        #region Header Parsing

        /// <summary>
        /// Get the generic header values of a CHD, if possible
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static (char[] tag, uint length, uint version) GetHeaderValues(Stream stream)
        {
            char[] parsedTag = new char[8];
            uint parsedLength = 0;
            uint parsedVersion = 0;

            using (BinaryReader br = new BinaryReader(stream, Encoding.Default, true))
            {
                parsedTag = br.ReadChars(8);
                parsedLength = br.ReadUInt32BigEndian();
                parsedVersion = br.ReadUInt32BigEndian();
            }

            return (parsedTag, parsedLength, parsedVersion);
        }

        /// <summary>
        /// Validate the header values
        /// </summary>
        /// <returns>Matching version, 0 if none</returns>
        private static uint ValidateHeader(char[] tag, uint length, uint version)
        {
            if (!string.Equals(new string(tag), "MComprHD", StringComparison.Ordinal))
                return 0;

            return version switch
            {
                1 => length == CHDFileV1.HeaderSize ? version : 0,
                2 => length == CHDFileV2.HeaderSize ? version : 0,
                3 => length == CHDFileV3.HeaderSize ? version : 0,
                4 => length == CHDFileV4.HeaderSize ? version : 0,
                5 => length == CHDFileV5.HeaderSize ? version : 0,
                _ => 0,
            };
        }

        /// <summary>
        /// Read a stream as a particular CHD version
        /// </summary>
        /// <param name="stream">CHD file as a stream</param>
        /// <param name="version">CHD version to parse</param>
        /// <returns>Populated CHD file, null on failure</returns>
        private static CHDFile ReadAsVersion(Stream stream, uint version)
        {
            return version switch
            {
                1 => CHDFileV1.Deserialize(stream),
                2 => CHDFileV2.Deserialize(stream),
                3 => CHDFileV3.Deserialize(stream),
                4 => CHDFileV4.Deserialize(stream),
                5 => CHDFileV5.Deserialize(stream),
                _ => null,
            };
        }

        #endregion
		
        #region Extraction
        /// <inheritdoc/>
        public override bool CopyAll(string outDir)
        {
				// CHD has only one file
            bool encounteredErrors = true;

            try
            {
                 CopyToFile(this.Filename, outDir);
                 encounteredErrors = false;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                encounteredErrors = true;
            }

            return encounteredErrors;
        }

        /// <inheritdoc/>
        public override string CopyToFile(string entryName, string outDir)
        {
            string realentry = null;

            // Copy single file from the current folder to the output directory, if exists
            try
            {
                // Make sure the folders exist
                 Directory.CreateDirectory(outDir);

                // If we had a file, copy that over to the new name
                if (!string.IsNullOrWhiteSpace(this.Filename))
                {
                    realentry = this.Filename;
                    File.Copy(this.Filename, Path.Combine(outDir, entryName));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return realentry;
            }

            return realentry;
        }

        /// <inheritdoc/>
        public override (MemoryStream, string) CopyToStream(string entryName)
        {
            MemoryStream ms = new MemoryStream();
            string realentry = null;

            // Copy single file from the current folder to the output directory, if exists
            try
            {
                // If we had a file, copy that over to the new name
                if (!string.IsNullOrWhiteSpace(this.Filename))
                {
                    using (FileStream msstream = File.OpenRead(this.Filename))
                    {
                        msstream.CopyTo(ms);
                        realentry = this.Filename;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return (ms, realentry);
            }

            return (ms, realentry);
        }

        #endregion

        #region Information

        /// <inheritdoc/>
        public override List<BaseFile> GetChildren()
        {
            List<BaseFile> found = new List<BaseFile>();
            string gamename = Path.GetFileNameWithoutExtension(this.Filename);

            try
            {
                // only one file in CHD
                // has only md5 (v1-v4) or sha1 (v3-v5)

                // Fill in comon details and add to the list
                generated.Filename = gamename;
                generated.Parent = gamename;

                found.Add(generated);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }

            return found;
         }

        /// <inheritdoc/>
        public override List<string> GetEmptyFolders()
        {
            // CHD files don't contain directories
            return new List<string>();
         }

        /// <inheritdoc/>
        public override bool IsTorrent()
        {
           throw new NotImplementedException();
         }

        #endregion

        #region Writing

        /// <inheritdoc/>
        public override bool Write(string inputFile, string outDir, BaseFile baseFile)
        {
            // Check that the input file exists
            if (!File.Exists(inputFile))
            {
                logger.Warning($"File '{inputFile}' does not exist!");
                return false;
            }

            file = Path.GetFullPath(inputFile);

            // Get the file stream for the file and write out
            return Write(File.OpenRead(inputFile), outDir, baseFile);
        }

        /// <inheritdoc/>
        public override bool Write(Stream inputStream, string outDir, BaseFile baseFile)
        {
            bool success = false;

            // If either input is null or empty, return
            if (inputStream == null || baseFile == null || baseFile.Filename == null)
                return success;

            // If the stream is not readable, return
            if (!inputStream.CanRead)
                return success;

            // Get the output folder name from the first rebuild rom
            string archiveFileName = Path.Combine(outDir, Utilities.RemovePathUnsafeCharacters(baseFile.Parent) + (baseFile.Parent.EndsWith(".chd") ? string.Empty : ".chd"));

            FileType fileType = (FileType)BaseFile.GetFileType(file);
            if (fileType == FileType.ISOArchive)
            {
                // create to chd v5
                CHDManCheck cmc = new CHDManCheck();
                CHDManCheck.hdErr res = CHDManCheck.hdErr.HDERR_NONE;
                string error = "";
                res = cmc.ChdCreate(file, archiveFileName, out error);
                logger.User($"Create CHD - {error}");
            }
            else
            {
                // Read the standard CHD headers
                (char[] tag, uint length, uint version) = GetHeaderValues(inputStream);
                inputStream.SeekIfPossible(); // Seek back to start

                // Validate that this is actually a valid CHD
                uint validatedVersion = ValidateHeader(tag, length, version);
                if (validatedVersion == 0)
                    return success;
                else if (validatedVersion < 5) // use 6 to test
                {
                    // transform to chd v5
                    CHDManCheck cmc = new CHDManCheck();
                    CHDManCheck.hdErr res = CHDManCheck.hdErr.HDERR_NONE;
                    string error = "";
                    res = cmc.ChdUpgrade(file, archiveFileName, out error);
                    // inputStream.SeekIfPossible(); // Seek back to start
                    logger.User($"Upgrade CHD - {error}");
                }
                else
                {
logger.User($"test6 {inputStream} {outDir} {baseFile}");
                    // Set internal variables
                    FileStream outputStream = null;

                    // stream writing is faster than chdman
                    try
                    {
                        // If the full output path doesn't exist, create it
                        if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
                            Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));

                        // Overwrite output files by default
                        outputStream = File.Create(archiveFileName);

                        // If the output stream isn't null
                        if (outputStream != null)
                        {
                            // Copy the input stream to the output
                            inputStream.Seek(0, SeekOrigin.Begin);
                            int bufferSize = 4096 * 128;
                            byte[] ibuffer = new byte[bufferSize];
                            int ilen;
                            while ((ilen = inputStream.Read(ibuffer, 0, bufferSize)) > 0)
                            {
                                outputStream.Write(ibuffer, 0, ilen);
                                outputStream.Flush();
                            }

                            outputStream.Dispose();

                            if (!string.IsNullOrWhiteSpace(baseFile.Date))
                                File.SetCreationTime(archiveFileName, DateTime.Parse(baseFile.Date));

                            success = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex);
                        success = false;
                    }
                    finally
                    {
                        outputStream?.Dispose();
                    }
                }
            }

            inputStream?.Close();
            inputStream?.Dispose();
            return success;
        }

        /// <inheritdoc/>
        public override bool Write(List<string> inputFiles, string outDir, List<BaseFile> roms)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
