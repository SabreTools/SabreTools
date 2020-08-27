using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.IO;
using SabreTools.Library.Tools;
using Compress;
using Compress.SevenZip;
using Compress.ZipFile;
using NaturalSort;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// Represents a Torrent7zip archive for reading and writing
    /// </summary>
    public class SevenZipArchive : BaseArchive
    {
        #region Constructors

        /// <summary>
        /// Create a new TorrentSevenZipArchive with no base file
        /// </summary>
        public SevenZipArchive()
            : base()
        {
            this.Type = FileType.SevenZipArchive;
        }

        /// <summary>
        /// Create a new TorrentSevenZipArchive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        /// <param name="read">True for opening file as read, false for opening file as write</param>
        /// <param name="getHashes">True if hashes for this file should be calculated, false otherwise (default)</param>
        public SevenZipArchive(string filename, bool getHashes = false)
            : base(filename, getHashes)
        {
            this.Type = FileType.SevenZipArchive;
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

                // Extract all files to the temp directory
                SevenZ zf = new SevenZ();
                ZipReturn zr = zf.ZipFileOpen(this.Filename, -1, true);
                if (zr != ZipReturn.ZipGood)
                {
                    throw new Exception(ZipFile.ZipErrorMessageText(zr));
                }

                for (int i = 0; i < zf.LocalFilesCount() && zr == ZipReturn.ZipGood; i++)
                {
                    // Open the read stream
                    zr = zf.ZipFileOpenReadStream(i, out Stream readStream, out ulong streamsize);

                    // Create the rest of the path, if needed
                    if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(zf.Filename(i))))
                        Directory.CreateDirectory(Path.Combine(outDir, Path.GetDirectoryName(zf.Filename(i))));

                    // If the entry ends with a directory separator, continue to the next item, if any
                    if (zf.Filename(i).EndsWith(Path.DirectorySeparatorChar.ToString())
                        || zf.Filename(i).EndsWith(Path.AltDirectorySeparatorChar.ToString())
                        || zf.Filename(i).EndsWith(Path.PathSeparator.ToString()))
                    {
                        zf.ZipFileCloseReadStream();
                        continue;
                    }

                    FileStream writeStream = FileExtensions.TryCreate(Path.Combine(outDir, zf.Filename(i)));

                    // If the stream is smaller than the buffer, just run one loop through to avoid issues
                    if (streamsize < _bufferSize)
                    {
                        byte[] ibuffer = new byte[streamsize];
                        int ilen = readStream.Read(ibuffer, 0, (int)streamsize);
                        writeStream.Write(ibuffer, 0, ilen);
                        writeStream.Flush();
                    }
                    // Otherwise, we do the normal loop
                    else
                    {
                        int realBufferSize = (streamsize < _bufferSize ? (int)streamsize : _bufferSize);
                        byte[] ibuffer = new byte[realBufferSize];
                        int ilen;
                        while ((ilen = readStream.Read(ibuffer, 0, realBufferSize)) > 0)
                        {
                            writeStream.Write(ibuffer, 0, ilen);
                            writeStream.Flush();
                        }
                    }

                    zr = zf.ZipFileCloseReadStream();
                    writeStream.Dispose();
                }

                zf.ZipFileClose();
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
            string realEntry = null;

            try
            {
                SevenZ zf = new SevenZ();
                ZipReturn zr = zf.ZipFileOpen(this.Filename, -1, true);
                if (zr != ZipReturn.ZipGood)
                {
                    throw new Exception(ZipFile.ZipErrorMessageText(zr));
                }

                for (int i = 0; i < zf.LocalFilesCount() && zr == ZipReturn.ZipGood; i++)
                {
                    if (zf.Filename(i).Contains(entryName))
                    {
                        // Open the read stream
                        realEntry = zf.Filename(i);
                        zr = zf.ZipFileOpenReadStream(i, out Stream readStream, out ulong streamsize);

                        // If the stream is smaller than the buffer, just run one loop through to avoid issues
                        if (streamsize < _bufferSize)
                        {
                            byte[] ibuffer = new byte[streamsize];
                            int ilen = readStream.Read(ibuffer, 0, (int)streamsize);
                            ms.Write(ibuffer, 0, ilen);
                            ms.Flush();
                        }
                        // Otherwise, we do the normal loop
                        else
                        {
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while (streamsize > _bufferSize)
                            {
                                ilen = readStream.Read(ibuffer, 0, _bufferSize);
                                ms.Write(ibuffer, 0, ilen);
                                ms.Flush();
                                streamsize -= _bufferSize;
                            }

                            ilen = readStream.Read(ibuffer, 0, (int)streamsize);
                            ms.Write(ibuffer, 0, ilen);
                            ms.Flush();
                        }

                        zr = zf.ZipFileCloseReadStream();
                    }
                }

                zf.ZipFileClose();
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
            List<BaseFile> found = new List<BaseFile>();
            string gamename = Path.GetFileNameWithoutExtension(this.Filename);

            try
            {
                SevenZ zf = new SevenZ();
                ZipReturn zr = zf.ZipFileOpen(this.Filename, -1, true);
                if (zr != ZipReturn.ZipGood)
                {
                    throw new Exception(ZipFile.ZipErrorMessageText(zr));
                }

                for (int i = 0; i < zf.LocalFilesCount(); i++)
                {
                    // If the entry is a directory (or looks like a directory), we don't want to open it
                    if (zf.IsDirectory(i)
                        || zf.Filename(i).EndsWith(Path.DirectorySeparatorChar.ToString())
                        || zf.Filename(i).EndsWith(Path.AltDirectorySeparatorChar.ToString())
                        || zf.Filename(i).EndsWith(Path.PathSeparator.ToString()))
                    {
                        continue;
                    }

                    // Open the read stream
                    zr = zf.ZipFileOpenReadStream(i, out Stream readStream, out ulong streamsize);

                    // If we get a read error, log it and continue
                    if (zr != ZipReturn.ZipGood)
                    {
                        Globals.Logger.Warning($"An error occurred while reading archive {this.Filename}: Zip Error - {zr}");
                        zr = zf.ZipFileCloseReadStream();
                        continue;
                    }

                    // If secure hashes are disabled, do a quickscan
                    if (omitFromScan == Hash.SecureHashes)
                    {
                        string newname = zf.Filename(i);
                        long newsize = (long)zf.UncompressedSize(i);
                        byte[] newcrc = zf.CRC32(i);

                        found.Add(new BaseFile
                        {
                            Filename = newname,
                            Size = newsize,
                            CRC = newcrc,

                            Parent = gamename,
                        });
                    }
                    // Otherwise, use the stream directly
                    else
                    {
                        BaseFile zipEntryRom = readStream.GetInfo(size: (long)zf.UncompressedSize(i), omitFromScan: omitFromScan, keepReadOpen: true);
                        zipEntryRom.Filename = zf.Filename(i);
                        zipEntryRom.Parent = gamename;
                        found.Add(zipEntryRom);
                    }
                }

                // Dispose of the archive
                zr = zf.ZipFileCloseReadStream();
                zf.ZipFileClose();
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return null;
            }

            return found;
        }

        /// <summary>
        /// Generate a list of empty folders in an archive
        /// </summary>
        /// <param name="input">Input file to get data from</param>
        /// <returns>List of empty folders in the archive</returns>
        public override List<string> GetEmptyFolders()
        {
            List<string> empties = new List<string>();

            try
            {
                SevenZ zf = new SevenZ();
                ZipReturn zr = zf.ZipFileOpen(this.Filename, -1, true);
                if (zr != ZipReturn.ZipGood)
                {
                    throw new Exception(ZipFile.ZipErrorMessageText(zr));
                }

                List<(string, bool)> zipEntries = new List<(string, bool)>();
                for (int i = 0; i < zf.LocalFilesCount(); i++)
                {
                    zipEntries.Add((zf.Filename(i), zf.IsDirectory(i)));
                }

                zipEntries = zipEntries.OrderBy(p => p.Item1, new NaturalReversedComparer()).ToList();
                string lastZipEntry = null;
                foreach ((string, bool) entry in zipEntries)
                {
                    // If the current is a superset of last, we skip it
                    if (lastZipEntry != null && lastZipEntry.StartsWith(entry.Item1))
                    {
                        // No-op
                    }
                    // If the entry is a directory, we add it
                    else
                    {
                        if (entry.Item2)
                        {
                            empties.Add(entry.Item1);
                        }
                        lastZipEntry = entry.Item1;
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
            }

            return empties;
        }

        /// <summary>
        /// Check whether the input file is a standardized format
        /// </summary>
        public override bool IsTorrent()
        {
            SevenZ zf = new SevenZ();
            ZipReturn zr = zf.ZipFileOpen(this.Filename, -1, true);
            if (zr != ZipReturn.ZipGood)
            {
                throw new Exception(ZipFile.ZipErrorMessageText(zr));
            }

            return zf.ZipStatus == ZipStatus.Trrnt7Zip;
        }

        #endregion

        #region Writing

        /// <summary>
        /// Write an input file to a torrent7z archive
        /// </summary>
        /// <param name="inputFile">Input filename to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="depth">Positive value for depth of the output depot, defaults to 4</param>
        /// <returns>True if the archive was written properly, false otherwise</returns>
        public override bool Write(string inputFile, string outDir, Rom rom, bool date = false, int depth = 4)
        {
            // Get the file stream for the file and write out
            return Write(FileExtensions.TryOpenRead(inputFile), outDir, rom, date: date);
        }

        /// <summary>
        /// Write an input file to a torrent7z archive
        /// </summary>
        /// <param name="inputStream">Input stream to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="depth">Positive value for depth of the output depot, defaults to 4</param>
        /// <returns>True if the archive was written properly, false otherwise</returns>
        public override bool Write(Stream inputStream, string outDir, Rom rom, bool date = false, int depth = 4)
        {
            bool success = false;
            string tempFile = Path.Combine(outDir, $"tmp{Guid.NewGuid()}");

            // If either input is null or empty, return
            if (inputStream == null || rom == null || rom.Name == null)
            {
                return success;
            }

            // If the stream is not readable, return
            if (!inputStream.CanRead)
            {
                return success;
            }

            // Seek to the beginning of the stream
            inputStream.Seek(0, SeekOrigin.Begin);

            // Get the output archive name from the first rebuild rom
            string archiveFileName = Path.Combine(outDir, Sanitizer.RemovePathUnsafeCharacters(rom.Machine.Name) + (rom.Machine.Name.EndsWith(".7z") ? string.Empty : ".7z"));

            // Set internal variables
            Stream writeStream = null;
            SevenZ oldZipFile = new SevenZ();
            SevenZ zipFile = new SevenZ();
            ZipReturn zipReturn = ZipReturn.ZipGood;

            try
            {
                // If the full output path doesn't exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));
                }

                // If the archive doesn't exist, create it and put the single file
                if (!File.Exists(archiveFileName))
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                    zipReturn = zipFile.ZipFileCreate(tempFile);

                    // Open the input file for reading
                    ulong istreamSize = (ulong)(inputStream.Length);

                    DateTime dt = DateTime.Now;
                    if (date && !string.IsNullOrWhiteSpace(rom.Date) && DateTime.TryParse(rom.Date.Replace('\\', '/'), out dt))
                    {
                        uint msDosDateTime = Utilities.ConvertDateTimeToMsDosTimeFormat(dt);
                        zipFile.ZipFileOpenWriteStream(false, false, rom.Name.Replace('\\', '/'), istreamSize, 0, msDosDateTime, out writeStream);
                    }
                    else
                    {
                        zipFile.ZipFileOpenWriteStream(false, true, rom.Name.Replace('\\', '/'), istreamSize, 0, null, out writeStream);
                    }

                    // Copy the input stream to the output
                    byte[] ibuffer = new byte[_bufferSize];
                    int ilen;
                    while ((ilen = inputStream.Read(ibuffer, 0, _bufferSize)) > 0)
                    {
                        writeStream.Write(ibuffer, 0, ilen);
                        writeStream.Flush();
                    }
                    inputStream.Dispose();
                    zipFile.ZipFileCloseWriteStream(Utilities.StringToByteArray(rom.CRC));
                }

                // Otherwise, sort the input files and write out in the correct order
                else
                {
                    // Open the old archive for reading
                    oldZipFile.ZipFileOpen(archiveFileName, -1, true);

                    // Map all inputs to index
                    Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
                    var oldZipFileContents = new List<string>();
                    for (int i = 0; i < oldZipFile.LocalFilesCount(); i++)
                    {
                        oldZipFileContents.Add(oldZipFile.Filename(i));
                    }

                    // If the old one doesn't contain the new file, then add it
                    if (!oldZipFileContents.Contains(rom.Name.Replace('\\', '/')))
                    {
                        inputIndexMap.Add(rom.Name.Replace('\\', '/'), -1);
                    }

                    // Then add all of the old entries to it too
                    for (int i = 0; i < oldZipFile.LocalFilesCount(); i++)
                    {
                        inputIndexMap.Add(oldZipFile.Filename(i), i);
                    }

                    // If the number of entries is the same as the old archive, skip out
                    if (inputIndexMap.Keys.Count <= oldZipFile.LocalFilesCount())
                    {
                        success = true;
                        return success;
                    }

                    // Otherwise, process the old zipfile
                    zipFile.ZipFileCreate(tempFile);

                    // Get the order for the entries with the new file
                    List<string> keys = inputIndexMap.Keys.ToList();
                    keys.Sort(ZipFile.TrrntZipStringCompare);

                    // Copy over all files to the new archive
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // If we have the input file, add it now
                        if (index < 0)
                        {
                            // Open the input file for reading
                            ulong istreamSize = (ulong)(inputStream.Length);

                            DateTime dt = DateTime.Now;
                            if (date && !string.IsNullOrWhiteSpace(rom.Date) && DateTime.TryParse(rom.Date.Replace('\\', '/'), out dt))
                            {
                                uint msDosDateTime = Utilities.ConvertDateTimeToMsDosTimeFormat(dt);
                                zipFile.ZipFileOpenWriteStream(false, false, rom.Name.Replace('\\', '/'), istreamSize, 0, msDosDateTime, out writeStream);
                            }
                            else
                            {
                                zipFile.ZipFileOpenWriteStream(false, true, rom.Name.Replace('\\', '/'), istreamSize, 0, null, out writeStream);
                            }

                            // Copy the input stream to the output
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while ((ilen = inputStream.Read(ibuffer, 0, _bufferSize)) > 0)
                            {
                                writeStream.Write(ibuffer, 0, ilen);
                                writeStream.Flush();
                            }

                            inputStream.Dispose();
                            zipFile.ZipFileCloseWriteStream(Utilities.StringToByteArray(rom.CRC));
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Instantiate the streams
                            oldZipFile.ZipFileOpenReadStream(index, out Stream zreadStream, out ulong istreamSize);
                            zipFile.ZipFileOpenWriteStream(false, true, oldZipFile.Filename(index), istreamSize, 0, null, out writeStream);

                            // Copy the input stream to the output
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while ((ilen = zreadStream.Read(ibuffer, 0, _bufferSize)) > 0)
                            {
                                writeStream.Write(ibuffer, 0, ilen);
                                writeStream.Flush();
                            }

                            oldZipFile.ZipFileCloseReadStream();
                            zipFile.ZipFileCloseWriteStream(oldZipFile.CRC32(index));
                        }
                    }
                }

                // Close the output zip file
                zipFile.ZipFileClose();
                oldZipFile.ZipFileClose();

                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                success = false;
            }
            finally
            {
                inputStream?.Dispose();
            }

            // If the old file exists, delete it and replace
            if (File.Exists(archiveFileName))
            {
                FileExtensions.TryDelete(archiveFileName);
            }
            File.Move(tempFile, archiveFileName);

            return true;
        }

        /// <summary>
        /// Write a set of input files to a torrent7z archive (assuming the same output archive name)
        /// </summary>
        /// <param name="inputFiles">Input files to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
        /// <returns>True if the archive was written properly, false otherwise</returns>
        public override bool Write(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false, bool romba = false)
        {
            bool success = false;
            string tempFile = Path.Combine(outDir, $"tmp{Guid.NewGuid()}");

            // If either list of roms is null or empty, return
            if (inputFiles == null || roms == null || inputFiles.Count == 0 || roms.Count == 0)
            {
                return success;
            }

            // If the number of inputs is less than the number of available roms, return
            if (inputFiles.Count < roms.Count)
            {
                return success;
            }

            // If one of the files doesn't exist, return
            foreach (string file in inputFiles)
            {
                if (!File.Exists(file))
                {
                    return success;
                }
            }

            // Get the output archive name from the first rebuild rom
            string archiveFileName = Path.Combine(outDir, Sanitizer.RemovePathUnsafeCharacters(roms[0].Machine.Name) + (roms[0].Machine.Name.EndsWith(".7z") ? string.Empty : ".7z"));

            // Set internal variables
            Stream writeStream = null;
            SevenZ oldZipFile = new SevenZ();
            SevenZ zipFile = new SevenZ();
            ZipReturn zipReturn = ZipReturn.ZipGood;

            try
            {
                // If the full output path doesn't exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));
                }

                // If the archive doesn't exist, create it and put the single file
                if (!File.Exists(archiveFileName))
                {
                    zipReturn = zipFile.ZipFileCreate(tempFile);

                    // Map all inputs to index
                    Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
                    for (int i = 0; i < inputFiles.Count; i++)
                    {
                        inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), i);
                    }

                    // Sort the keys in TZIP order
                    List<string> keys = inputIndexMap.Keys.ToList();
                    keys.Sort(ZipFile.TrrntZipStringCompare);

                    // Now add all of the files in order
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // Open the input file for reading
                        Stream freadStream = FileExtensions.TryOpenRead(inputFiles[index]);
                        ulong istreamSize = (ulong)(new FileInfo(inputFiles[index]).Length);

                        DateTime dt = DateTime.Now;
                        if (date && !string.IsNullOrWhiteSpace(roms[index].Date) && DateTime.TryParse(roms[index].Date.Replace('\\', '/'), out dt))
                        {
                            uint msDosDateTime = Utilities.ConvertDateTimeToMsDosTimeFormat(dt);
                            zipFile.ZipFileOpenWriteStream(false, false, roms[index].Name.Replace('\\', '/'), istreamSize, 0, msDosDateTime, out writeStream);
                        }
                        else
                        {
                            zipFile.ZipFileOpenWriteStream(false, true, roms[index].Name.Replace('\\', '/'), istreamSize, 0, null, out writeStream);
                        }

                        // Copy the input stream to the output
                        byte[] ibuffer = new byte[_bufferSize];
                        int ilen;
                        while ((ilen = freadStream.Read(ibuffer, 0, _bufferSize)) > 0)
                        {
                            writeStream.Write(ibuffer, 0, ilen);
                            writeStream.Flush();
                        }

                        freadStream.Dispose();
                        zipFile.ZipFileCloseWriteStream(Utilities.StringToByteArray(roms[index].CRC));
                    }
                }

                // Otherwise, sort the input files and write out in the correct order
                else
                {
                    // Open the old archive for reading
                    oldZipFile.ZipFileOpen(archiveFileName, -1, true);

                    // Map all inputs to index
                    Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
                    for (int i = 0; i < inputFiles.Count; i++)
                    {
                        var oldZipFileContents = new List<string>();
                        for (int j = 0; j < oldZipFile.LocalFilesCount(); j++)
                        {
                            oldZipFileContents.Add(oldZipFile.Filename(j));
                        }

                        // If the old one contains the new file, then just skip out
                        if (oldZipFileContents.Contains(roms[i].Name.Replace('\\', '/')))
                        {
                            continue;
                        }

                        inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), -(i + 1));
                    }

                    // Then add all of the old entries to it too
                    for (int i = 0; i < oldZipFile.LocalFilesCount(); i++)
                    {
                        inputIndexMap.Add(oldZipFile.Filename(i), i);
                    }

                    // If the number of entries is the same as the old archive, skip out
                    if (inputIndexMap.Keys.Count <= oldZipFile.LocalFilesCount())
                    {
                        success = true;
                        return success;
                    }

                    // Otherwise, process the old zipfile
                    zipFile.ZipFileCreate(tempFile);

                    // Get the order for the entries with the new file
                    List<string> keys = inputIndexMap.Keys.ToList();
                    keys.Sort(ZipFile.TrrntZipStringCompare);

                    // Copy over all files to the new archive
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // If we have the input file, add it now
                        if (index < 0)
                        {
                            // Open the input file for reading
                            Stream freadStream = FileExtensions.TryOpenRead(inputFiles[-index - 1]);
                            ulong istreamSize = (ulong)(new FileInfo(inputFiles[-index - 1]).Length);

                            DateTime dt = DateTime.Now;
                            if (date && !string.IsNullOrWhiteSpace(roms[-index - 1].Date) && DateTime.TryParse(roms[-index - 1].Date.Replace('\\', '/'), out dt))
                            {
                                uint msDosDateTime = Utilities.ConvertDateTimeToMsDosTimeFormat(dt);
                                zipFile.ZipFileOpenWriteStream(false, false, roms[-index - 1].Name.Replace('\\', '/'), istreamSize, 0, msDosDateTime, out writeStream);
                            }
                            else
                            {
                                zipFile.ZipFileOpenWriteStream(false, true, roms[-index - 1].Name.Replace('\\', '/'), istreamSize, 0, null, out writeStream);
                            }

                            // Copy the input stream to the output
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while ((ilen = freadStream.Read(ibuffer, 0, _bufferSize)) > 0)
                            {
                                writeStream.Write(ibuffer, 0, ilen);
                                writeStream.Flush();
                            }
                            freadStream.Dispose();
                            zipFile.ZipFileCloseWriteStream(Utilities.StringToByteArray(roms[-index - 1].CRC));
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Instantiate the streams
                            oldZipFile.ZipFileOpenReadStream(index, out Stream zreadStream, out ulong istreamSize);
                            zipFile.ZipFileOpenWriteStream(false, true, oldZipFile.Filename(index), istreamSize, 0, null, out writeStream);

                            // Copy the input stream to the output
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while ((ilen = zreadStream.Read(ibuffer, 0, _bufferSize)) > 0)
                            {
                                writeStream.Write(ibuffer, 0, ilen);
                                writeStream.Flush();
                            }

                            zipFile.ZipFileCloseWriteStream(oldZipFile.CRC32(index));
                        }
                    }
                }

                // Close the output zip file
                zipFile.ZipFileClose();
                oldZipFile.ZipFileClose();

                success = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                success = false;
            }

            // If the old file exists, delete it and replace
            if (File.Exists(archiveFileName))
            {
                FileExtensions.TryDelete(archiveFileName);
            }
            File.Move(tempFile, archiveFileName);

            return true;
        }

        #endregion
    }
}
