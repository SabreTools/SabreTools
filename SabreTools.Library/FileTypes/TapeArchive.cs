using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using Compress.ZipFile;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// Represents a Torrent7zip archive for reading and writing
    /// </summary>
    /// TODO: Don't try to read entries to MemoryStream during write
    public class TapeArchive : BaseArchive
    {
        #region Constructors

        /// <summary>
        /// Create a new TorrentTarArchive with no base file
        /// </summary>
        public TapeArchive()
            : base()
        {
            this.Type = FileType.TapeArchive;
        }

        /// <summary>
        /// Create a new TorrentTarArchive from the given file
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
                TarArchive ta = TarArchive.Open(this.Filename);
                foreach (TarArchiveEntry entry in ta.Entries)
                {
                    entry.WriteToDirectory(outDir, new ExtractionOptions { PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
                }
                encounteredErrors = false;
                ta.Dispose();
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
                TarArchive ta = TarArchive.Open(FileExtensions.TryOpenRead(this.Filename));
                foreach (TarArchiveEntry entry in ta.Entries.Where(e => e != null && !e.IsDirectory))
                {
                    // If secure hashes are disabled, do a quickscan
                    if (omitFromScan == Hash.SecureHashes)
                    {
                        found.Add(new BaseFile
                        {
                            Filename = entry.Key,
                            Size = entry.Size,
                            CRC = BitConverter.GetBytes(entry.Crc),
                            Date = (date && entry.LastModifiedTime != null ? entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss") : null),

                            Parent = gamename,
                        });
                    }
                    // Otherwise, use the stream directly
                    else
                    {
                        Stream entryStream = entry.OpenEntryStream();
                        BaseFile tarEntryRom = entryStream.GetInfo(entry.Size, omitFromScan);
                        tarEntryRom.Filename = entry.Key;
                        tarEntryRom.Parent = gamename;
                        tarEntryRom.Date = entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss");
                        found.Add(tarEntryRom);
                        entryStream.Dispose();
                    }
                }

                // Dispose of the archive
                ta.Dispose();
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
                Globals.Logger.Error(ex.ToString());
            }

            return empties;
        }

        /// <summary>
        /// Check whether the input file is a standardized format
        /// </summary>
        public override bool IsTorrent()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Writing

        /// <summary>
        /// Write an input file to a tape archive
        /// </summary>
        /// <param name="inputFile">Input filename to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
        /// <returns>True if the archive was written properly, false otherwise</returns>
        public override bool Write(string inputFile, string outDir, Rom rom, bool date = false, bool romba = false)
        {
            // Get the file stream for the file and write out
            return Write(FileExtensions.TryOpenRead(inputFile), outDir, rom, date: date);
        }

        /// <summary>
        /// Write an input stream to a tape archive
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

            // Get the output archive name from the first rebuild rom
            string archiveFileName = Path.Combine(outDir, Sanitizer.RemovePathUnsafeCharacters(rom.MachineName) + (rom.MachineName.EndsWith(".tar") ? string.Empty : ".tar"));

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
                    // Get temporary date-time if possible
                    DateTime? usableDate = null;
                    if (date && !string.IsNullOrWhiteSpace(rom.Date) && DateTime.TryParse(rom.Date.Replace('\\', '/'), out DateTime dt))
                    {
                        usableDate = dt;
                    }

                    // Copy the input stream to the output
                    inputStream.Seek(0, SeekOrigin.Begin);
                    tarFile.AddEntry(rom.Name, inputStream, size: rom.Size, modified: usableDate);
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
                    if (!entries.Contains(rom.Name.Replace('\\', '/')))
                    {
                        inputIndexMap.Add(rom.Name.Replace('\\', '/'), -1);
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
                    keys.Sort(ZipFile.TrrntZipStringCompare);

                    // Copy over all files to the new archive
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // Get temporary date-time if possible
                        DateTime? usableDate = null;
                        if (date && !string.IsNullOrWhiteSpace(rom.Date) && DateTime.TryParse(rom.Date.Replace('\\', '/'), out DateTime dt))
                        {
                            usableDate = dt;
                        }

                        // If we have the input file, add it now
                        if (index < 0)
                        {
                            // Copy the input file to the output
                            inputStream.Seek(0, SeekOrigin.Begin);
                            tarFile.AddEntry(rom.Name, inputStream, size: rom.Size, modified: usableDate);
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
                Console.WriteLine(ex);
                success = false;
            }
            finally
            {
                inputStream.Dispose();
                tarFile.Dispose();
                oldTarFile.Dispose();
            }

            // If the old file exists, delete it and replace
            if (File.Exists(archiveFileName))
            {
                FileExtensions.TryDelete(archiveFileName);
            }
            File.Move(tempFile, archiveFileName);

            return success;
        }

        /// <summary>
        /// Write a set of input files to a tape archive (assuming the same output archive name)
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
            string archiveFileName = Path.Combine(outDir, Sanitizer.RemovePathUnsafeCharacters(roms[0].MachineName) + (roms[0].MachineName.EndsWith(".tar") ? string.Empty : ".tar"));

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

                        // Get temporary date-time if possible
                        DateTime? usableDate = null;
                        if (date && !string.IsNullOrWhiteSpace(roms[index].Date) && DateTime.TryParse(roms[index].Date.Replace('\\', '/'), out DateTime dt))
                        {
                            usableDate = dt;
                        }

                        // Copy the input stream to the output
                        tarFile.AddEntry(roms[index].Name, FileExtensions.TryOpenRead(inputFiles[index]), size: roms[index].Size, modified: usableDate);
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
                        if (entries.Contains(roms[i].Name.Replace('\\', '/')))
                        {
                            continue;
                        }

                        inputIndexMap.Add(roms[i].Name.Replace('\\', '/'), -(i + 1));
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
                    keys.Sort(ZipFile.TrrntZipStringCompare);

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
                            if (date && !string.IsNullOrWhiteSpace(roms[-index - 1].Date) && DateTime.TryParse(roms[-index - 1].Date.Replace('\\', '/'), out DateTime dt))
                            {
                                usableDate = dt;
                            }

                            // Copy the input file to the output
                            tarFile.AddEntry(roms[-index - 1].Name, FileExtensions.TryOpenRead(inputFiles[-index - 1]), size: roms[-index - 1].Size, modified: usableDate);
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
                Console.WriteLine(ex);
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
                FileExtensions.TryDelete(archiveFileName);
            }
            File.Move(tempFile, archiveFileName);

            return true;
        }

        #endregion
    }
}
