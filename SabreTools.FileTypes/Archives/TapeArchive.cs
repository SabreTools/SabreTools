﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Core;
using SabreTools.Core.Tools;
using Compress.ZipFile;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace SabreTools.FileTypes.Archives
{
    /// <summary>
    /// Represents a Tape archive for reading and writing
    /// </summary>
    /// TODO: Don't try to read entries to MemoryStream during write
    public class TapeArchive : BaseArchive
    {
        #region Constructors

        /// <summary>
        /// Create a new Tape archive with no base file
        /// </summary>
        public TapeArchive()
            : base()
        {
            this.Type = FileType.TapeArchive;
        }

        /// <summary>
        /// Create a new Tape archive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        /// <param name="read">True for opening file as read, false for opening file as write</param>
        /// <param name="getHashes">True if hashes for this file should be calculated, false otherwise (default)</param>
        public TapeArchive(string filename, bool getHashes = false)
            : base(filename, getHashes)
        {
            this.Type = FileType.TapeArchive;
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
                TarArchive ta = TarArchive.Open(this.Filename);
                foreach (TarArchiveEntry entry in ta.Entries)
                {
                    entry.WriteToDirectory(outDir, new ExtractionOptions { PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
                }
                encounteredErrors = false;
                ta.Dispose();
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
                TarArchive ta = TarArchive.Open(this.Filename, new ReaderOptions { LeaveStreamOpen = false, });
                foreach (TarArchiveEntry entry in ta.Entries)
                {
                    if (entry != null && !entry.IsDirectory && entry.Key.Contains(entryName))
                    {
                        // Write the file out
                        realEntry = entry.Key;
                        entry.WriteTo(ms);
                    }
                }
                ta.Dispose();
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
                TarArchive ta = TarArchive.Open(File.OpenRead(this.Filename));
                foreach (TarArchiveEntry entry in ta.Entries.Where(e => e != null && !e.IsDirectory))
                {
                    // Create a blank item for the entry
                    BaseFile tarEntryRom = new BaseFile();

                    // Perform a quickscan, if flagged to
                    if (this.AvailableHashes == Hash.CRC)
                    {
                        tarEntryRom.Size = entry.Size;
                        tarEntryRom.CRC = BitConverter.GetBytes(entry.Crc);
                    }
                    // Otherwise, use the stream directly
                    else
                    {
                        using Stream entryStream = entry.OpenEntryStream();
                        tarEntryRom = GetInfo(entryStream, size: entry.Size, hashes: this.AvailableHashes);
                    }

                    // Fill in comon details and add to the list
                    tarEntryRom.Filename = entry.Key;
                    tarEntryRom.Parent = gamename;
                    tarEntryRom.Date = entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss");
                    found.Add(tarEntryRom);
                }

                // Dispose of the archive
                ta.Dispose();
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
                TarArchive ta = TarArchive.Open(this.Filename, new ReaderOptions { LeaveStreamOpen = false });
                List<TarArchiveEntry> tarEntries = ta.Entries.OrderBy(e => e.Key, new NaturalSort.NaturalReversedComparer()).ToList();
                string lastTarEntry = null;
                foreach (TarArchiveEntry entry in tarEntries)
                {
                    if (entry != null)
                    {
                        // If the current is a superset of last, we skip it
                        if (lastTarEntry != null && lastTarEntry.StartsWith(entry.Key))
                        {
                            // No-op
                        }
                        // If the entry is a directory, we add it
                        else if (entry.IsDirectory)
                        {
                            empties.Add(entry.Key);
                            lastTarEntry = entry.Key;
                        }
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
            throw new NotImplementedException();
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

            // Get the output archive name from the first rebuild rom
            string archiveFileName = Path.Combine(outDir, Utilities.RemovePathUnsafeCharacters(baseFile.Parent) + (baseFile.Parent.EndsWith(".tar") ? string.Empty : ".tar"));

            // Set internal variables
            TarArchive oldTarFile = TarArchive.Create();
            TarArchive tarFile = TarArchive.Create();

            try
            {
                // If the full output path doesn't exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName));

                // If the archive doesn't exist, create it and put the single file
                if (!File.Exists(archiveFileName))
                {
                    // Get temporary date-time if possible
                    DateTime? usableDate = null;
                    if (UseDates && !string.IsNullOrWhiteSpace(baseFile.Date) && DateTime.TryParse(baseFile.Date.Replace('\\', '/'), out DateTime dt))
                        usableDate = dt;

                    // Copy the input stream to the output
                    inputStream.Seek(0, SeekOrigin.Begin);
                    tarFile.AddEntry(baseFile.Filename, inputStream, size: baseFile.Size ?? 0, modified: usableDate);
                }

                // Otherwise, sort the input files and write out in the correct order
                else
                {
                    // Open the old archive for reading
                    oldTarFile = TarArchive.Open(archiveFileName);

                    // Get a list of all current entries
                    List<string> entries = oldTarFile.Entries.Select(i => i.Key).ToList();

                    // Map all inputs to index
                    Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();

                    // If the old one doesn't contain the new file, then add it
                    if (!entries.Contains(baseFile.Filename.Replace('\\', '/')))
                        inputIndexMap.Add(baseFile.Filename.Replace('\\', '/'), -1);

                    // Then add all of the old entries to it too
                    for (int i = 0; i < entries.Count; i++)
                    {
                        inputIndexMap.Add(entries[i], i);
                    }

                    // If the number of entries is the same as the old archive, skip out
                    if (inputIndexMap.Keys.Count <= entries.Count)
                    {
                        success = true;
                        return success;
                    }

                    // Get the order for the entries with the new file
                    List<string> keys = inputIndexMap.Keys.ToList();
                    keys.Sort(ZipUtils.TrrntZipStringCompare);

                    // Copy over all files to the new archive
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // Get temporary date-time if possible
                        DateTime? usableDate = null;
                        if (UseDates && !string.IsNullOrWhiteSpace(baseFile.Date) && DateTime.TryParse(baseFile.Date.Replace('\\', '/'), out DateTime dt))
                            usableDate = dt;

                        // If we have the input file, add it now
                        if (index < 0)
                        {
                            // Copy the input file to the output
                            inputStream.Seek(0, SeekOrigin.Begin);
                            tarFile.AddEntry(baseFile.Filename, inputStream, size: baseFile.Size ?? 0, modified: usableDate);
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Get the stream from the original archive
                            TarArchiveEntry tae = oldTarFile.Entries.ElementAt(index);
                            MemoryStream entry = new MemoryStream();
                            tae.OpenEntryStream().CopyTo(entry);

                            // Copy the input stream to the output
                            tarFile.AddEntry(key, entry, size: tae.Size, modified: tae.LastModifiedTime);
                        }
                    }
                }

                // Close the output tar file
                tarFile.SaveTo(tempFile, new WriterOptions(CompressionType.None));

                success = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                success = false;
            }
            finally
            {
                tarFile.Dispose();
                oldTarFile.Dispose();
            }

            // If the old file exists, delete it and replace
            if (File.Exists(archiveFileName))
                File.Delete(archiveFileName);

            File.Move(tempFile, archiveFileName);

            return success;
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
            string archiveFileName = Path.Combine(outDir, Utilities.RemovePathUnsafeCharacters(baseFiles[0].Parent) + (baseFiles[0].Parent.EndsWith(".tar") ? string.Empty : ".tar"));

            // Set internal variables
            TarArchive oldTarFile = TarArchive.Create();
            TarArchive tarFile = TarArchive.Create();

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

                        // Get temporary date-time if possible
                        DateTime? usableDate = null;
                        if (UseDates && !string.IsNullOrWhiteSpace(baseFiles[index].Date) && DateTime.TryParse(baseFiles[index].Date.Replace('\\', '/'), out DateTime dt))
                            usableDate = dt;

                        // Copy the input stream to the output
                        tarFile.AddEntry(baseFiles[index].Filename, File.OpenRead(inputFiles[index]), size: baseFiles[index].Size ?? 0, modified: usableDate);
                    }
                }

                // Otherwise, sort the input files and write out in the correct order
                else
                {
                    // Open the old archive for reading
                    oldTarFile = TarArchive.Open(archiveFileName);

                    // Get a list of all current entries
                    List<string> entries = oldTarFile.Entries.Select(i => i.Key).ToList();

                    // Map all inputs to index
                    Dictionary<string, int> inputIndexMap = new Dictionary<string, int>();
                    for (int i = 0; i < inputFiles.Count; i++)
                    {
                        // If the old one contains the new file, then just skip out
                        if (entries.Contains(baseFiles[i].Filename.Replace('\\', '/')))
                        {
                            continue;
                        }

                        inputIndexMap.Add(baseFiles[i].Filename.Replace('\\', '/'), -(i + 1));
                    }

                    // Then add all of the old entries to it too
                    for (int i = 0; i < entries.Count; i++)
                    {
                        inputIndexMap.Add(entries[i], i);
                    }

                    // If the number of entries is the same as the old archive, skip out
                    if (inputIndexMap.Keys.Count <= entries.Count)
                    {
                        success = true;
                        return success;
                    }

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
                            // Get temporary date-time if possible
                            DateTime? usableDate = null;
                            if (UseDates && !string.IsNullOrWhiteSpace(baseFiles[-index - 1].Date) && DateTime.TryParse(baseFiles[-index - 1].Date.Replace('\\', '/'), out DateTime dt))
                                usableDate = dt;

                            // Copy the input file to the output
                            tarFile.AddEntry(baseFiles[-index - 1].Filename, File.OpenRead(inputFiles[-index - 1]), size: baseFiles[-index - 1].Size ?? 0, modified: usableDate);
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Get the stream from the original archive
                            TarArchiveEntry tae = oldTarFile.Entries.ElementAt(index);
                            MemoryStream entry = new MemoryStream();
                            tae.OpenEntryStream().CopyTo(entry);

                            // Copy the input stream to the output
                            tarFile.AddEntry(key, entry, size: tae.Size, modified: tae.LastModifiedTime);
                        }
                    }
                }

                // Close the output tar file
                tarFile.SaveTo(tempFile, new WriterOptions(CompressionType.None));

                success = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                success = false;
            }
            finally
            {
                tarFile.Dispose();
                oldTarFile.Dispose();
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
