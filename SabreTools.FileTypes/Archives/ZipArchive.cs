using System;
using System.Collections.Generic;
using System.IO;
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD
using System.IO.Compression;
#endif
using System.Linq;
using Compress;
using Compress.ZipFile;
using SabreTools.Core.Tools;
using SabreTools.Hashing;
using SabreTools.Text.Compare;

namespace SabreTools.FileTypes.Archives
{
    /// <summary>
    /// Represents a Zip archive for reading and writing
    /// </summary>
    public class ZipArchive : BaseArchive
    {
        /* TorrentZip Header Format
            https://pkware.cachefly.net/webdocs/APPNOTE/APPNOTE_6.2.0.txt
            http://www.romvault.com/trrntzip_explained.doc

            00-03		Local file header signature (0x50, 0x4B, 0x03, 0x04) ZipSignature
            04-05		Version needed to extract (0x14, 0x00)
            06-07		General purpose bit flag (0x02, 0x00)
            08-09		Compression method (0x08, 0x00)
            0A-0B		Last mod file time (0x00, 0xBC)
            0C-0D		Last mod file date (0x98, 0x21)
        */

        #region Constructors

        /// <summary>
        /// Create a new TorrentZipArchive with no base file
        /// </summary>
        public ZipArchive()
            : base()
        {
        }

        /// <summary>
        /// Create a new TorrentZipArchive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        public ZipArchive(string filename)
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
                var pkzip = Serialization.Wrappers.PKZIP.Create(inputFile);

                // Write the output file
                encounteredErrors = !pkzip.Extract(outDir, includeDebug: false);
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

            try
            {
                Stream? stream = null;

#if NET20 || NET35 || NET40
                var zf = new Zip();
                ZipReturn zr = zf.ZipFileOpen(Filename!, -1, true);
                if (zr != ZipReturn.ZipGood)
                    throw new Exception(CompressUtils.ZipErrorMessageText(zr));

                for (int i = 0; i < zf.LocalFilesCount() && zr == ZipReturn.ZipGood; i++)
                {
                    // Get the entry
                    var entry = zf.GetLocalFile(i);

                    // Skip invalid entries
                    if (entry.Filename == null)
                        continue;

                    // Skip directory entries
                    if (entry.IsDirectory)
                        continue;

                    // Skip non-matching keys
                    if (!entry.Filename.Contains(entryName))
                        continue;

                    // Open the entry stream
                    realEntry = entry.Filename;
                    zr = zf.ZipFileOpenReadStream(i, out stream, out ulong streamsize);
                    break;
                }

                zf.ZipFileClose();
                return stream;
#else
                var zf = ZipFile.OpenRead(Filename);
                if (zf == null)
                    throw new Exception($"Could not open {Filename} as a zip file");

                for (int i = 0; i < zf.Entries.Count; i++)
                {
                    // Get the entry
                    var entry = zf.Entries[i]; ;

                    // Skip invalid entries
                    if (entry.FullName == null)
                        continue;

                    // Skip directory entries
                    if (entry.FullName.EndsWith(Path.DirectorySeparatorChar.ToString())
                        || entry.FullName.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                        || entry.FullName.EndsWith(Path.PathSeparator.ToString()))
                    {
                        continue;
                    }

                    // Skip non-matching keys
                    if (!entry.FullName.Contains(entryName))
                        continue;

                    // Open the entry stream
                    realEntry = entry.FullName;
                    stream = entry.Open();
                    break;
                }

                zf.Dispose();
                return stream;
#endif
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

            List<BaseFile> found = [];
            string? gamename = Path.GetFileNameWithoutExtension(Filename);

            try
            {
#if NET20 || NET35 || NET40
                var zf = new Zip();
                ZipReturn zr = zf.ZipFileOpen(Filename!, -1, true);
                if (zr != ZipReturn.ZipGood)
                    throw new Exception(CompressUtils.ZipErrorMessageText(zr));

                for (int i = 0; i < zf.LocalFilesCount(); i++)
                {
                    // Get the local file
                    var localFile = zf.GetLocalFile(i);
                    if (localFile == null)
                        continue;

                    // If the entry is a directory (or looks like a directory), we don't want to open it
                    if (localFile.IsDirectory
                        || localFile.Filename!.EndsWith(Path.DirectorySeparatorChar.ToString())
                        || localFile.Filename!.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                        || localFile.Filename!.EndsWith(Path.PathSeparator.ToString()))
                    {
                        continue;
                    }

                    // Open the read stream
                    zr = zf.ZipFileOpenReadStream(i, false, out Stream? readStream, out _, out _);

                    // If we get a read error, log it and continue
                    if (zr != ZipReturn.ZipGood || readStream == null)
                    {
                        _logger.Warning($"An error occurred while reading archive {Filename}: Zip Error - {zr}");
                        continue;
                    }

                    // Create a blank item for the entry
                    var zipEntryRom = new BaseFile();

                    // Perform a quickscan, if flagged to
                    if (_hashTypes.Length == 1 && _hashTypes[0] == HashType.CRC32)
                    {
                        zipEntryRom.Size = (long)localFile.UncompressedSize;
                        zipEntryRom.CRC = localFile.CRC;
                    }
                    // Otherwise, use the stream directly
                    else
                    {
                        zipEntryRom = FileTypeTool.GetInfo(readStream, _hashTypes);
                    }

                    // Fill in common details and add to the list
                    zipEntryRom.Filename = localFile.Filename;
                    zipEntryRom.Parent = gamename;
                    zipEntryRom.Date = localFile.LastModified.ToString("yyyy/MM/dd HH:mm:ss");
                    found.Add(zipEntryRom);
                }

                // Dispose of the archive
                zr = zf.ZipFileCloseReadStream();
                zf.ZipFileClose();
#else
                var zf = ZipFile.OpenRead(Filename);
                if (zf == null)
                    throw new Exception($"Could not open {Filename} as a zip file");

                for (int i = 0; i < zf.Entries.Count; i++)
                {
                    // Get the local file
                    var localFile = zf.Entries[i];
                    if (localFile == null)
                        continue;

                    // If the entry is a directory (or looks like a directory), we don't want to open it
                    if (localFile.FullName!.EndsWith(Path.DirectorySeparatorChar.ToString())
                        || localFile.FullName!.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                        || localFile.FullName!.EndsWith(Path.PathSeparator.ToString()))
                    {
                        continue;
                    }

                    // Open the read stream
                    var readStream = localFile.Open();
                    if (readStream == null)
                        continue;

                    // Create a blank item for the entry
                    var zipEntryRom = new BaseFile();

                    // Perform a quickscan, if flagged to
                    if (_hashTypes.Length == 1 && _hashTypes[0] == HashType.CRC32)
                    {
                        zipEntryRom.Size = localFile.Length;
#if NETCOREAPP
                        zipEntryRom.CRC = BitConverter.GetBytes(localFile.Crc32);
#else
                        // TODO: Figure out how to get the CRC from the header
#endif
                    }
                    // Otherwise, use the stream directly
                    else
                    {
                        zipEntryRom = FileTypeTool.GetInfo(readStream, _hashTypes);
                    }

                    // Fill in common details and add to the list
                    zipEntryRom.Filename = localFile.FullName;
                    zipEntryRom.Parent = gamename;
                    zipEntryRom.Date = localFile.LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss");
                    found.Add(zipEntryRom);
                }

                // Dispose of the archive
                zf.Dispose();
#endif
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }

            return found;
        }

        /// <inheritdoc/>
        public override List<string> GetEmptyFolders()
        {
            // If we have an invalid file
            if (Filename == null)
                return [];

            List<string> empties = [];

            try
            {
#if NET20 || NET35 || NET40
                var zf = new Zip();
                ZipReturn zr = zf.ZipFileOpen(Filename!, -1, true);
                if (zr != ZipReturn.ZipGood)
                    throw new Exception(CompressUtils.ZipErrorMessageText(zr));

                Dictionary<string, bool> zipEntries = [];
                for (int i = 0; i < zf.LocalFilesCount(); i++)
                {
                    zipEntries.Add(zf.GetLocalFile(i).Filename!, zf.GetLocalFile(i).IsDirectory);
                }

                var pairs = zipEntries.OrderBy(p => p.Key, new NaturalReversedComparer()).ToList();
                string? lastZipEntry = null;
                foreach (var entry in pairs)
                {
                    // If the current is a superset of last, we skip it
                    if (lastZipEntry != null && lastZipEntry.StartsWith(entry.Key))
                    {
                        // No-op
                    }
                    // If the entry is a directory, we add it
                    else
                    {
                        if (entry.Value)
                            empties.Add(entry.Key);

                        lastZipEntry = entry.Key;
                    }
                }
#else
                var zf = ZipFile.OpenRead(Filename);
                if (zf == null)
                    throw new Exception($"Could not open {Filename} as a zip file");

                Dictionary<string, bool> zipEntries = [];
                for (int i = 0; i < zf.Entries.Count; i++)
                {
                    // Get the local file
                    var entry = zf.Entries[i];
                    if (entry == null)
                        continue;

                    // If the entry is a directory (or looks like a directory)
                    bool isDirectory = false;
                    if (entry.FullName!.EndsWith(Path.DirectorySeparatorChar.ToString())
                        || entry.FullName!.EndsWith(Path.AltDirectorySeparatorChar.ToString())
                        || entry.FullName!.EndsWith(Path.PathSeparator.ToString()))
                    {
                        isDirectory = true;
                    }

                    zipEntries.Add(entry.FullName, isDirectory);
                }

                var pairs = zipEntries.OrderBy(p => p.Key, new NaturalReversedComparer()).ToList();
                string? lastZipEntry = null;
                foreach (var entry in pairs)
                {
                    // If the current is a superset of last, we skip it
                    if (lastZipEntry != null && lastZipEntry.StartsWith(entry.Key))
                    {
                        // No-op
                    }
                    // If the entry is a directory, we add it
                    else
                    {
                        if (entry.Value)
                            empties.Add(entry.Key);

                        lastZipEntry = entry.Key;
                    }
                }
#endif
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return empties;
        }

        /// <inheritdoc/>
        public override bool IsStandardized()
        {
            Zip zf = new();
            ZipReturn zr = zf.ZipFileOpen(Filename!, -1, true);
            if (zr != ZipReturn.ZipGood)
                throw new Exception(CompressUtils.ZipErrorMessageText(zr));

            return zf.ZipStatus == ZipStatus.TrrntZip;
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
            string tempFile = Path.Combine(outDir, $"tmp{Guid.NewGuid()}");

            // If either input is null or empty, return
            if (stream == null || baseFile == null || baseFile.Filename == null)
                return success;

            // If the stream is not readable, return
            if (!stream.CanRead)
                return success;

            // Seek to the beginning of the stream
            if (stream.CanSeek)
                stream.Seek(0, SeekOrigin.Begin);

            // Get the output archive name from the first rebuild rom
            string archiveFileName = Path.Combine(outDir, TextHelper.RemovePathUnsafeCharacters(baseFile.Parent) + (baseFile.Parent!.EndsWith(".zip") ? string.Empty : ".zip"));

            // Set internal variables
            Stream? writeStream = null;
            Zip oldZipFile = new();
            Zip zipFile = new();
            ZipReturn zipReturn = ZipReturn.ZipGood;

            try
            {
                // If the full output path doesn't exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName)!);

                // If the archive doesn't exist, create it and put the single file
                if (!File.Exists(archiveFileName))
                {
                    if (stream.CanSeek)
                        stream.Seek(0, SeekOrigin.Begin);

                    zipReturn = zipFile.ZipFileCreate(tempFile);

                    // Open the input file for reading
                    ulong istreamSize = (ulong)(stream.Length);

                    DateTime dt = DateTime.Now;
                    if (_realDates && !string.IsNullOrEmpty(baseFile.Date) && DateTime.TryParse(baseFile.Date!.Replace('\\', '/'), out dt))
                    {
                        long msDosDateTime = DateTimeHelper.ConvertToMsDosTimeFormat(dt);
                        TimeStamps ts = new() { ModTime = msDosDateTime };
                        zipFile.ZipFileOpenWriteStream(false, false, baseFile.Filename.Replace('\\', '/'), istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, ts);
                    }
                    else
                    {
                        zipFile.ZipFileOpenWriteStream(false, true, baseFile.Filename.Replace('\\', '/'), istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, null);
                    }

                    // Copy the input stream to the output
                    byte[] ibuffer = new byte[_bufferSize];
                    int ilen;
                    while ((ilen = stream.Read(ibuffer, 0, _bufferSize)) > 0)
                    {
                        writeStream!.Write(ibuffer, 0, ilen);
                        writeStream.Flush();
                    }

                    zipFile.ZipFileCloseWriteStream(baseFile.CRC!);
                }

                // Otherwise, sort the input files and write out in the correct order
                else
                {
                    // Open the old archive for reading
                    oldZipFile.ZipFileOpen(archiveFileName, -1, true);

                    // Map all inputs to index
                    Dictionary<string, int> inputIndexMap = new();
                    List<string> oldZipFileContents = [];
                    for (int i = 0; i < oldZipFile.LocalFilesCount(); i++)
                    {
                        oldZipFileContents.Add(oldZipFile.GetLocalFile(i).Filename!);
                    }

                    // If the old one doesn't contain the new file, then add it
                    if (!oldZipFileContents.Contains(baseFile.Filename.Replace('\\', '/')))
                        inputIndexMap.Add(baseFile.Filename.Replace('\\', '/'), -1);

                    // Then add all of the old entries to it too
                    for (int i = 0; i < oldZipFile.LocalFilesCount(); i++)
                    {
                        inputIndexMap.Add(oldZipFile.GetLocalFile(i).Filename!, i);
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
                    List<string> keys = [.. inputIndexMap.Keys];
                    keys.Sort(CompressUtils.TrrntZipStringCompare);

                    // Copy over all files to the new archive
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // If we have the input file, add it now
                        if (index < 0)
                        {
                            // Open the input file for reading
                            ulong istreamSize = (ulong)(stream.Length);

                            DateTime dt = DateTime.Now;
                            if (_realDates && !string.IsNullOrEmpty(baseFile.Date) && DateTime.TryParse(baseFile.Date!.Replace('\\', '/'), out dt))
                            {
                                long msDosDateTime = DateTimeHelper.ConvertToMsDosTimeFormat(dt);
                                TimeStamps ts = new() { ModTime = msDosDateTime };
                                zipFile.ZipFileOpenWriteStream(false, false, baseFile.Filename.Replace('\\', '/'), istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, ts);
                            }
                            else
                            {
                                zipFile.ZipFileOpenWriteStream(false, true, baseFile.Filename.Replace('\\', '/'), istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, null);
                            }

                            // Copy the input stream to the output
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while ((ilen = stream.Read(ibuffer, 0, _bufferSize)) > 0)
                            {
                                writeStream!.Write(ibuffer, 0, ilen);
                                writeStream.Flush();
                            }

                            zipFile.ZipFileCloseWriteStream(baseFile.CRC!);
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Instantiate the streams
                            oldZipFile.ZipFileOpenReadStream(index, false, out Stream? zreadStream, out ulong istreamSize, out ushort icompressionMethod);
                            long msDosDateTime = oldZipFile.GetLocalFile(index).LastModified;
                            TimeStamps ts = new() { ModTime = msDosDateTime };
                            zipFile.ZipFileOpenWriteStream(false, true, oldZipFile.GetLocalFile(index).Filename!, istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, ts);

                            // Copy the input stream to the output
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while ((ilen = zreadStream!.Read(ibuffer, 0, _bufferSize)) > 0)
                            {
                                writeStream!.Write(ibuffer, 0, ilen);
                                writeStream.Flush();
                            }

                            oldZipFile.ZipFileCloseReadStream();
                            zipFile.ZipFileCloseWriteStream(oldZipFile.GetLocalFile(index).CRC!);
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
                _logger.Error(ex);
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
        public override bool Write(List<string> files, string outDir, List<BaseFile>? baseFiles)
        {
            bool success = false;
            string tempFile = Path.Combine(outDir, $"tmp{Guid.NewGuid()}");

            // If either list of roms is null or empty, return
            if (files == null || baseFiles == null || files.Count == 0 || baseFiles.Count == 0)
                return false;

            // If the number of inputs is less than the number of available roms, return
            if (files.Count < baseFiles.Count)
                return false;

            // If one of the files doesn't exist, return
            foreach (string file in files)
            {
                if (!File.Exists(file))
                    return false;
            }

            // Get the output archive name from the first rebuild rom
            string archiveFileName = Path.Combine(outDir, TextHelper.RemovePathUnsafeCharacters(baseFiles[0].Parent) + (baseFiles[0].Parent!.EndsWith(".zip") ? string.Empty : ".zip"));

            // Set internal variables
            Stream? writeStream = null;
            var oldZipFile = new Zip();
            var zipFile = new Zip();
            ZipReturn zipReturn = ZipReturn.ZipGood;

            try
            {
                // If the full output path doesn't exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName)!);

                // If the archive doesn't exist, create it and put the single file
                if (!File.Exists(archiveFileName))
                {
                    zipReturn = zipFile.ZipFileCreate(tempFile);

                    // Map all inputs to index
                    Dictionary<string, int> inputIndexMap = new();
                    for (int i = 0; i < files.Count; i++)
                    {
                        inputIndexMap.Add(baseFiles[i].Filename!.Replace('\\', '/'), i);
                    }

                    // Sort the keys in TZIP order
                    List<string> keys = [.. inputIndexMap.Keys];
                    keys.Sort(CompressUtils.TrrntZipStringCompare);

                    // Now add all of the files in order
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // Open the input file for reading
                        Stream freadStream = File.Open(files[index], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        ulong istreamSize = (ulong)(new FileInfo(files[index]).Length);

                        DateTime dt = DateTime.Now;
                        if (_realDates && !string.IsNullOrEmpty(baseFiles[index].Date) && DateTime.TryParse(baseFiles[index].Date?.Replace('\\', '/'), out dt))
                        {
                            long msDosDateTime = DateTimeHelper.ConvertToMsDosTimeFormat(dt);
                            TimeStamps ts = new() { ModTime = msDosDateTime };
                            zipFile.ZipFileOpenWriteStream(false, false, baseFiles[index].Filename!.Replace('\\', '/'), istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, ts);
                        }
                        else
                        {
                            zipFile.ZipFileOpenWriteStream(false, true, baseFiles[index].Filename!.Replace('\\', '/'), istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, null);
                        }

                        // Copy the input stream to the output
                        byte[] ibuffer = new byte[_bufferSize];
                        int ilen;
                        while ((ilen = freadStream.Read(ibuffer, 0, _bufferSize)) > 0)
                        {
                            writeStream!.Write(ibuffer, 0, ilen);
                            writeStream.Flush();
                        }

                        freadStream.Dispose();
                        zipFile.ZipFileCloseWriteStream(baseFiles[index].CRC!);
                    }
                }

                // Otherwise, sort the input files and write out in the correct order
                else
                {
                    // Open the old archive for reading
                    oldZipFile.ZipFileOpen(archiveFileName, -1, true);

                    // Map all inputs to index
                    Dictionary<string, int> inputIndexMap = new();
                    for (int i = 0; i < files.Count; i++)
                    {
                        List<string> oldZipFileContents = [];
                        for (int j = 0; j < oldZipFile.LocalFilesCount(); j++)
                        {
                            oldZipFileContents.Add(oldZipFile.GetLocalFile(j).Filename!);
                        }

                        // If the old one contains the new file, then just skip out
                        if (oldZipFileContents.Contains(baseFiles[i].Filename!.Replace('\\', '/')))
                            continue;

                        inputIndexMap.Add(baseFiles[i].Filename!.Replace('\\', '/'), -(i + 1));
                    }

                    // Then add all of the old entries to it too
                    for (int i = 0; i < oldZipFile.LocalFilesCount(); i++)
                    {
                        inputIndexMap.Add(oldZipFile.GetLocalFile(i).Filename!, i);
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
                    List<string> keys = [.. inputIndexMap.Keys];
                    keys.Sort(CompressUtils.TrrntZipStringCompare);

                    // Copy over all files to the new archive
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // If we have the input file, add it now
                        if (index < 0)
                        {
                            // Open the input file for reading
                            Stream freadStream = File.Open(files[-index - 1], FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            ulong istreamSize = (ulong)(new FileInfo(files[-index - 1]).Length);

                            DateTime dt = DateTime.Now;
                            if (_realDates && !string.IsNullOrEmpty(baseFiles[-index - 1].Date) && DateTime.TryParse(baseFiles[-index - 1].Date?.Replace('\\', '/'), out dt))
                            {
                                long msDosDateTime = DateTimeHelper.ConvertToMsDosTimeFormat(dt);
                                TimeStamps ts = new() { ModTime = msDosDateTime };
                                zipFile.ZipFileOpenWriteStream(false, false, baseFiles[-index - 1].Filename!.Replace('\\', '/'), istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, ts);
                            }
                            else
                            {
                                zipFile.ZipFileOpenWriteStream(false, true, baseFiles[-index - 1].Filename!.Replace('\\', '/'), istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, null);
                            }

                            // Copy the input stream to the output
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while ((ilen = freadStream.Read(ibuffer, 0, _bufferSize)) > 0)
                            {
                                writeStream!.Write(ibuffer, 0, ilen);
                                writeStream.Flush();
                            }
                            freadStream.Dispose();
                            zipFile.ZipFileCloseWriteStream(baseFiles[-index - 1].CRC!);
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Instantiate the streams
                            oldZipFile.ZipFileOpenReadStream(index, false, out Stream? zreadStream, out ulong istreamSize, out ushort icompressionMethod);
                            long msDosDateTime = oldZipFile.GetLocalFile(index).LastModified;
                            TimeStamps ts = new() { ModTime = msDosDateTime };
                            zipFile.ZipFileOpenWriteStream(false, true, oldZipFile.GetLocalFile(index).Filename!, istreamSize, (ushort)CompressionMethod.Deflated, out writeStream, ts);

                            // Copy the input stream to the output
                            byte[] ibuffer = new byte[_bufferSize];
                            int ilen;
                            while ((ilen = zreadStream!.Read(ibuffer, 0, _bufferSize)) > 0)
                            {
                                writeStream!.Write(ibuffer, 0, ilen);
                                writeStream.Flush();
                            }

                            zipFile.ZipFileCloseWriteStream(oldZipFile.GetLocalFile(index).CRC!);
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
                _logger.Error(ex);
                success = false;
            }

            // If the old file exists, delete it and replace
            if (File.Exists(archiveFileName))
                File.Delete(archiveFileName);

            File.Move(tempFile, archiveFileName);

            return true;
        }

        #endregion
    }
}
