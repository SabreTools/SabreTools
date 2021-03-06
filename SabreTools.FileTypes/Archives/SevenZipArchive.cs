﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Core;
using SabreTools.Core.Tools;
using Compress;
using Compress.SevenZip;
using Compress.Utils;
using Compress.ZipFile;
using NaturalSort;

namespace SabreTools.FileTypes.Archives
{
    /// <summary>
    /// Represents a Torrent7zip archive for reading and writing
    /// </summary>
    public class SevenZipArchive : BaseArchive
    {
        #region Constants

        /* Torrent7z Header Format
            http://cpansearch.perl.org/src/BJOERN/Compress-Deflate7-1.0/7zip/DOC/7zFormat.txt

            00-05		Local file header signature (0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C) SevenZipSignature
            06-07		ArchiveVersion (0x00, 0x03)
            The rest is unknown
        */
        private readonly static byte[] Torrent7ZipHeader = new byte[] { 0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c, 0x00, 0x03 };
        private readonly static byte[] Torrent7ZipSignature = new byte[] { 0xa9, 0xa9, 0x9f, 0xd1, 0x57, 0x08, 0xa9, 0xd7, 0xea, 0x29, 0x64, 0xb2,
            0x36, 0x1b, 0x83, 0x52, 0x33, 0x00, 0x74, 0x6f, 0x72, 0x72, 0x65, 0x6e, 0x74, 0x37, 0x7a, 0x5f, 0x30, 0x2e, 0x39, 0x62, 0x65, 0x74, 0x61 };

        #endregion

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

        /// <inheritdoc/>
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
                    throw new Exception(ZipUtils.ZipErrorMessageText(zr));
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

                    FileStream writeStream = File.Create(Path.Combine(outDir, zf.Filename(i)));

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
            catch (EndOfStreamException ex)
            {
                // Catch this but don't count it as an error because SharpCompress is unsafe
                logger.Verbose(ex);
            }
            catch (InvalidOperationException ex)
            {
                logger.Warning(ex);
                encounteredErrors = true;
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
            // Try to extract a stream using the given information
            (MemoryStream ms, string realEntry) = CopyToStream(entryName);

            // If the memory stream and the entry name are both non-null, we write to file
            if (ms != null && realEntry != null)
            {
                realEntry = Path.Combine(outDir, realEntry);

                // Create the output subfolder now
                Directory.CreateDirectory(Path.GetDirectoryName(realEntry));

                // Now open and write the file if possible
                FileStream fs = File.Create(realEntry);
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

        /// <inheritdoc/>
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
                    throw new Exception(ZipUtils.ZipErrorMessageText(zr));
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
                logger.Error(ex);
                ms = null;
                realEntry = null;
            }

            return (ms, realEntry);
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
                SevenZ zf = new SevenZ();
                ZipReturn zr = zf.ZipFileOpen(this.Filename, -1, true);
                if (zr != ZipReturn.ZipGood)
                {
                    throw new Exception(ZipUtils.ZipErrorMessageText(zr));
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
                        logger.Warning($"An error occurred while reading archive {this.Filename}: Zip Error - {zr}");
                        zr = zf.ZipFileCloseReadStream();
                        continue;
                    }

                    // Create a blank item for the entry
                    BaseFile zipEntryRom = new BaseFile();

                    // Perform a quickscan, if flagged to
                    if (this.AvailableHashes == Hash.CRC)
                    {
                        zipEntryRom.Size = (long)zf.UncompressedSize(i);
                        zipEntryRom.CRC = zf.CRC32(i);
                    }
                    // Otherwise, use the stream directly
                    else
                    {
                        zipEntryRom = GetInfo(readStream, size: (long)zf.UncompressedSize(i), hashes: this.AvailableHashes, keepReadOpen: true);
                    }

                    // Fill in comon details and add to the list
                    zipEntryRom.Filename = zf.Filename(i);
                    zipEntryRom.Parent = gamename;
                    found.Add(zipEntryRom);
                }

                // Dispose of the archive
                zr = zf.ZipFileCloseReadStream();
                zf.ZipFileClose();
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
            List<string> empties = new List<string>();

            try
            {
                SevenZ zf = new SevenZ();
                ZipReturn zr = zf.ZipFileOpen(this.Filename, -1, true);
                if (zr != ZipReturn.ZipGood)
                {
                    throw new Exception(ZipUtils.ZipErrorMessageText(zr));
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
                logger.Error(ex);
            }

            return empties;
        }

        /// <inheritdoc/>
        public override bool IsTorrent()
        {
            SevenZ zf = new SevenZ();
            ZipReturn zr = zf.ZipFileOpen(this.Filename, -1, true);
            if (zr != ZipReturn.ZipGood)
            {
                throw new Exception(ZipUtils.ZipErrorMessageText(zr));
            }

            return zf.ZipStatus == ZipStatus.Trrnt7Zip;
        }

        #endregion

        #region Writing

        /// <inheritdoc/>
        public override bool Write(string inputFile, string outDir, BaseFile baseFile)
        {
            // Get the file stream for the file and write out
            return Write(File.OpenRead(inputFile), outDir, baseFile);
        }

        /// <inheritdoc/>
        public override bool Write(Stream inputStream, string outDir, BaseFile baseFile)
        {
            bool success = false;
            string tempFile = Path.Combine(outDir, $"tmp{Guid.NewGuid()}");

            // If either input is null or empty, return
            if (inputStream == null || baseFile == null || baseFile.Filename == null)
                return success;

            // If the stream is not readable, return
            if (!inputStream.CanRead)
                return success;

            // Seek to the beginning of the stream
            inputStream.Seek(0, SeekOrigin.Begin);

            // Get the output archive name from the first rebuild rom
            string archiveFileName = Path.Combine(outDir, Utilities.RemovePathUnsafeCharacters(baseFile.Parent) + (baseFile.Parent.EndsWith(".7z") ? string.Empty : ".7z"));

            // Set internal variables
            Stream writeStream = null;
            SevenZ oldZipFile = new SevenZ();
            SevenZ zipFile = new SevenZ();
            ZipReturn zipReturn = ZipReturn.ZipGood;

            try
            {
                // If the full output path doesn't exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));

                // If the archive doesn't exist, create it and put the single file
                if (!File.Exists(archiveFileName))
                {
                    inputStream.Seek(0, SeekOrigin.Begin);
                    zipReturn = zipFile.ZipFileCreate(tempFile);

                    // Open the input file for reading
                    ulong istreamSize = (ulong)(inputStream.Length);

                    DateTime dt = DateTime.Now;
                    if (UseDates && !string.IsNullOrWhiteSpace(baseFile.Date) && DateTime.TryParse(baseFile.Date.Replace('\\', '/'), out dt))
                    {
                        long msDosDateTime = Utilities.ConvertDateTimeToMsDosTimeFormat(dt);
                        TimeStamps ts = new TimeStamps { ModTime = msDosDateTime };
                        zipFile.ZipFileOpenWriteStream(false, false, baseFile.Filename.Replace('\\', '/'), istreamSize, 0, out writeStream, ts);
                    }
                    else
                    {
                        zipFile.ZipFileOpenWriteStream(false, true, baseFile.Filename.Replace('\\', '/'), istreamSize, 0, out writeStream, null);
                    }

                    // Copy the input stream to the output
                    byte[] ibuffer = new byte[_bufferSize];
                    int ilen;
                    while ((ilen = inputStream.Read(ibuffer, 0, _bufferSize)) > 0)
                    {
                        writeStream.Write(ibuffer, 0, ilen);
                        writeStream.Flush();
                    }

                    zipFile.ZipFileCloseWriteStream(baseFile.CRC);
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
                    if (!oldZipFileContents.Contains(baseFile.Filename.Replace('\\', '/')))
                    {
                        inputIndexMap.Add(baseFile.Filename.Replace('\\', '/'), -1);
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
                    keys.Sort(ZipUtils.TrrntZipStringCompare);

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
                            if (UseDates && !string.IsNullOrWhiteSpace(baseFile.Date) && DateTime.TryParse(baseFile.Date.Replace('\\', '/'), out dt))
                            {
                                long msDosDateTime = Utilities.ConvertDateTimeToMsDosTimeFormat(dt);
                                TimeStamps ts = new TimeStamps { ModTime = msDosDateTime };
                                zipFile.ZipFileOpenWriteStream(false, false, baseFile.Filename.Replace('\\', '/'), istreamSize, 0, out writeStream, ts);
                            }
                            else
                            {
                                zipFile.ZipFileOpenWriteStream(false, true, baseFile.Filename.Replace('\\', '/'), istreamSize, 0, out writeStream, null);
                            }

                            // Copy the input stream to the output
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while ((ilen = inputStream.Read(ibuffer, 0, _bufferSize)) > 0)
                            {
                                writeStream.Write(ibuffer, 0, ilen);
                                writeStream.Flush();
                            }

                            zipFile.ZipFileCloseWriteStream(baseFile.CRC);
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Instantiate the streams
                            oldZipFile.ZipFileOpenReadStream(index, out Stream zreadStream, out ulong istreamSize);
                            zipFile.ZipFileOpenWriteStream(false, true, oldZipFile.Filename(index), istreamSize, 0, out writeStream, null);

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
                logger.Error(ex);
                success = false;
            }
            finally
            {
            }

            // If the old file exists, delete it and replace
            if (File.Exists(archiveFileName))
                File.Delete(archiveFileName);

            File.Move(tempFile, archiveFileName);

            return true;
        }

        /// <inheritdoc/>
        public override bool Write(List<string> inputFiles, string outDir, List<BaseFile> baseFiles)
        {
            bool success = false;
            string tempFile = Path.Combine(outDir, $"tmp{Guid.NewGuid()}");

            // If either list of roms is null or empty, return
            if (inputFiles == null || baseFiles == null || inputFiles.Count == 0 || baseFiles.Count == 0)
            {
                return success;
            }

            // If the number of inputs is less than the number of available roms, return
            if (inputFiles.Count < baseFiles.Count)
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
            string archiveFileName = Path.Combine(outDir, Utilities.RemovePathUnsafeCharacters(baseFiles[0].Parent) + (baseFiles[0].Parent.EndsWith(".7z") ? string.Empty : ".7z"));

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
                        inputIndexMap.Add(baseFiles[i].Filename.Replace('\\', '/'), i);
                    }

                    // Sort the keys in TZIP order
                    List<string> keys = inputIndexMap.Keys.ToList();
                    keys.Sort(ZipUtils.TrrntZipStringCompare);

                    // Now add all of the files in order
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // Open the input file for reading
                        Stream freadStream = File.OpenRead(inputFiles[index]);
                        ulong istreamSize = (ulong)(new FileInfo(inputFiles[index]).Length);

                        DateTime dt = DateTime.Now;
                        if (UseDates && !string.IsNullOrWhiteSpace(baseFiles[index].Date) && DateTime.TryParse(baseFiles[index].Date.Replace('\\', '/'), out dt))
                        {
                            long msDosDateTime = Utilities.ConvertDateTimeToMsDosTimeFormat(dt);
                            TimeStamps ts = new TimeStamps { ModTime = msDosDateTime };
                            zipFile.ZipFileOpenWriteStream(false, false, baseFiles[index].Filename.Replace('\\', '/'), istreamSize, 0, out writeStream, ts);
                        }
                        else
                        {
                            zipFile.ZipFileOpenWriteStream(false, true, baseFiles[index].Filename.Replace('\\', '/'), istreamSize, 0, out writeStream, null);
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
                        zipFile.ZipFileCloseWriteStream(baseFiles[index].CRC);
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
                        if (oldZipFileContents.Contains(baseFiles[i].Filename.Replace('\\', '/')))
                        {
                            continue;
                        }

                        inputIndexMap.Add(baseFiles[i].Filename.Replace('\\', '/'), -(i + 1));
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
                    keys.Sort(ZipUtils.TrrntZipStringCompare);

                    // Copy over all files to the new archive
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // If we have the input file, add it now
                        if (index < 0)
                        {
                            // Open the input file for reading
                            Stream freadStream = File.OpenRead(inputFiles[-index - 1]);
                            ulong istreamSize = (ulong)(new FileInfo(inputFiles[-index - 1]).Length);

                            DateTime dt = DateTime.Now;
                            if (UseDates && !string.IsNullOrWhiteSpace(baseFiles[-index - 1].Date) && DateTime.TryParse(baseFiles[-index - 1].Date.Replace('\\', '/'), out dt))
                            {
                                long msDosDateTime = Utilities.ConvertDateTimeToMsDosTimeFormat(dt);
                                TimeStamps ts = new TimeStamps { ModTime = msDosDateTime };
                                zipFile.ZipFileOpenWriteStream(false, false, baseFiles[-index - 1].Filename.Replace('\\', '/'), istreamSize, 0, out writeStream, ts);
                            }
                            else
                            {
                                zipFile.ZipFileOpenWriteStream(false, true, baseFiles[-index - 1].Filename.Replace('\\', '/'), istreamSize, 0,  out writeStream, null);
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
                            zipFile.ZipFileCloseWriteStream(baseFiles[-index - 1].CRC);
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Instantiate the streams
                            oldZipFile.ZipFileOpenReadStream(index, out Stream zreadStream, out ulong istreamSize);
                            zipFile.ZipFileOpenWriteStream(false, true, oldZipFile.Filename(index), istreamSize, 0, out writeStream, null);

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
                logger.Error(ex);
                success = false;
            }

            // If the old file exists, delete it and replace
            if (File.Exists(archiveFileName))
            {
                File.Delete(archiveFileName);
            }
            File.Move(tempFile, archiveFileName);

            return true;
        }

        #endregion
    }
}
