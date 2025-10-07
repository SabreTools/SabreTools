using System;
using System.Collections.Generic;
using System.IO;
#if NET462_OR_GREATER || NETCOREAPP
using System.Linq;
using Compress;
using SabreTools.Core.Tools;
using SabreTools.Hashing;
using SabreTools.Text.Compare;
using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
#endif

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
        }

        /// <summary>
        /// Create a new Tape archive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        public TapeArchive(string filename)
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
                var tar = Serialization.Wrappers.TapeArchive.Create(inputFile);

                // Write the output files
                encounteredErrors = !tar.Extract(outDir, includeDebug: false);
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

#if NET462_OR_GREATER || NETCOREAPP
            try
            {
                Stream? stream = null;

                var ta = TarArchive.Open(Filename!, new ReaderOptions { LeaveStreamOpen = false, });
                foreach (TarArchiveEntry entry in ta.Entries)
                {
                    // Skip invalid entries
                    if (entry?.Key == null || !entry.IsComplete)
                        continue;

                    // Skip directory entries
                    if (entry.IsDirectory)
                        continue;

                    // Skip non-matching keys
                    if (!entry.Key.Contains(entryName))
                        continue;

                    // Open the entry stream
                    realEntry = entry.Key;
                    stream = entry.OpenEntryStream();
                    break;
                }

                ta.Dispose();
                return stream;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
#else
            // TODO: Support tape archives in old .NET
            return null;
#endif
        }

        #endregion

        #region Information

        /// <inheritdoc/>
        public override List<BaseFile>? GetChildren()
        {
#if NET462_OR_GREATER || NETCOREAPP
            List<BaseFile> found = [];
            string? gamename = Path.GetFileNameWithoutExtension(Filename);

            try
            {
                TarArchive ta = TarArchive.Open(File.Open(Filename!, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                foreach (TarArchiveEntry entry in ta.Entries)
                {
                    // Skip invalid entries
                    if (entry == null || entry.IsDirectory)
                        continue;

                    // Create a blank item for the entry
                    BaseFile tarEntryRom = new();

                    // Perform a quickscan, if flagged to
                    if (_hashTypes.Length == 1 && _hashTypes[0] == HashType.CRC32)
                    {
                        tarEntryRom.Size = entry.Size;
                        tarEntryRom.CRC = BitConverter.GetBytes(entry.Crc);
                    }
                    // Otherwise, use the stream directly
                    else
                    {
                        using Stream entryStream = entry.OpenEntryStream();
                        tarEntryRom = FileTypeTool.GetInfo(entryStream, _hashTypes);
                    }

                    // Fill in common details and add to the list
                    tarEntryRom.Filename = entry.Key;
                    tarEntryRom.Parent = gamename;
                    tarEntryRom.Date = entry.LastModifiedTime?.ToString("yyyy/MM/dd HH:mm:ss");
                    found.Add(tarEntryRom);
                }

                // Dispose of the archive
                ta.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }

            return found;
#else
            // TODO: Support tape archives in old .NET
            return null;
#endif
        }

        /// <inheritdoc/>
        public override List<string> GetEmptyFolders()
        {
#if NET462_OR_GREATER || NETCOREAPP
            List<string> empties = [];

            try
            {
                TarArchive ta = TarArchive.Open(Filename!, new ReaderOptions { LeaveStreamOpen = false });
                List<TarArchiveEntry> tarEntries = ta.Entries.OrderBy(e => e.Key ?? string.Empty, new NaturalReversedComparer()).ToList();
                string? lastTarEntry = null;
                foreach (TarArchiveEntry entry in tarEntries)
                {
                    if (entry?.Key == null)
                        continue;

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
            catch (Exception ex)
            {
                _logger.Error(ex);
            }

            return empties;
#else
            // TODO: Support tape archives in old .NET
            return [];
#endif
        }

        /// <inheritdoc/>
        public override bool IsStandardized()
        {
            throw new NotImplementedException();
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
            string tempFile = Path.Combine(outDir, $"tmp{Guid.NewGuid()}");

            // If either input is null or empty, return
            if (stream == null || baseFile == null || baseFile.Filename == null)
                return success;

            // If the stream is not readable, return
            if (!stream.CanRead)
                return success;

            // Get the output archive name from the first rebuild rom
            string archiveFileName = Path.Combine(outDir, TextHelper.RemovePathUnsafeCharacters(baseFile.Parent) + (baseFile.Parent!.EndsWith(".tar") ? string.Empty : ".tar"));

            // Set internal variables
            TarArchive oldTarFile = TarArchive.Create();
            TarArchive tarFile = TarArchive.Create();

            try
            {
                // If the full output path doesn't exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName)!);

                // If the archive doesn't exist, create it and put the single file
                if (!File.Exists(archiveFileName))
                {
                    // Get temporary date-time if possible
                    DateTime? usableDate = null;
                    if (_realDates && !string.IsNullOrEmpty(baseFile.Date) && DateTime.TryParse(baseFile.Date!.Replace('\\', '/'), out DateTime dt))
                        usableDate = dt;

                    // Copy the input stream to the output
                    if (stream.CanSeek)
                        stream.Seek(0, SeekOrigin.Begin);

                    tarFile.AddEntry(baseFile.Filename, stream, size: baseFile.Size ?? 0, modified: usableDate);
                }

                // Otherwise, sort the input files and write out in the correct order
                else
                {
                    // Open the old archive for reading
                    oldTarFile = TarArchive.Open(archiveFileName);

                    // Get a list of all current entries
                    List<string> entries = [.. oldTarFile.Entries.Select(i => i.Key)];

                    // Map all inputs to index
                    var inputIndexMap = new Dictionary<string, int>();

                    // If the old one doesn't contain the new file, then add it
                    if (!entries.Contains(baseFile.Filename.Replace('\\', '/')))
                        inputIndexMap.Add(baseFile.Filename.Replace('\\', '/'), -1);

                    // Then add all of the old entries to it too
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var entry = entries[i];
                        if (entry == null)
                            continue;

                        inputIndexMap.Add(entry, i);
                    }

                    // If the number of entries is the same as the old archive, skip out
                    if (inputIndexMap.Keys.Count <= entries.Count)
                    {
                        success = true;
                        return success;
                    }

                    // Get the order for the entries with the new file
                    List<string> keys = [.. inputIndexMap.Keys];
                    keys.Sort(CompressUtils.TrrntZipStringCompare);

                    // Copy over all files to the new archive
                    foreach (string key in keys)
                    {
                        // Get the index mapped to the key
                        int index = inputIndexMap[key];

                        // Get temporary date-time if possible
                        DateTime? usableDate = null;
                        if (_realDates && !string.IsNullOrEmpty(baseFile.Date) && DateTime.TryParse(baseFile.Date!.Replace('\\', '/'), out DateTime dt))
                            usableDate = dt;

                        // If we have the input file, add it now
                        if (index < 0)
                        {
                            // Copy the input file to the output
                            if (stream.CanSeek)
                                stream.Seek(0, SeekOrigin.Begin);

                            tarFile.AddEntry(baseFile.Filename, stream, size: baseFile.Size ?? 0, modified: usableDate);
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Get the stream from the original archive
                            TarArchiveEntry tae = oldTarFile.Entries.ElementAt(index);
                            MemoryStream entry = new();
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
                _logger.Error(ex);
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
#else
            // TODO: Support tape archives in old .NET
            return false;
#endif
        }

        /// <inheritdoc/>
        public override bool Write(List<string> files, string outDir, List<BaseFile>? baseFiles)
        {
#if NET462_OR_GREATER || NETCOREAPP
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
            string archiveFileName = Path.Combine(outDir, TextHelper.RemovePathUnsafeCharacters(baseFiles[0].Parent) + (baseFiles[0].Parent!.EndsWith(".tar") ? string.Empty : ".tar"));

            // Set internal variables
            TarArchive oldTarFile = TarArchive.Create();
            TarArchive tarFile = TarArchive.Create();

            try
            {
                // If the full output path doesn't exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(archiveFileName)))
                    Directory.CreateDirectory(Path.GetDirectoryName(archiveFileName)!);

                // If the archive doesn't exist, create it and put the single file
                if (!File.Exists(archiveFileName))
                {
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

                        // Get temporary date-time if possible
                        DateTime? usableDate = null;
                        if (_realDates && !string.IsNullOrEmpty(baseFiles[index].Date) && DateTime.TryParse(baseFiles[index].Date?.Replace('\\', '/'), out DateTime dt))
                            usableDate = dt;

                        // Copy the input stream to the output
                        tarFile.AddEntry(baseFiles[index].Filename!,
                            File.Open(files[index], FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                            size: baseFiles[index].Size ?? 0,
                            modified: usableDate);
                    }
                }

                // Otherwise, sort the input files and write out in the correct order
                else
                {
                    // Open the old archive for reading
                    oldTarFile = TarArchive.Open(archiveFileName);

                    // Get a list of all current entries
                    List<string> entries = [.. oldTarFile.Entries.Select(i => i.Key)];

                    // Map all inputs to index
                    var inputIndexMap = new Dictionary<string, int>();
                    for (int i = 0; i < files.Count; i++)
                    {
                        // If the old one contains the new file, then just skip out
                        if (entries.Contains(baseFiles[i].Filename!.Replace('\\', '/')))
                        {
                            continue;
                        }

                        inputIndexMap.Add(baseFiles[i].Filename!.Replace('\\', '/'), -(i + 1));
                    }

                    // Then add all of the old entries to it too
                    for (int i = 0; i < entries.Count; i++)
                    {
                        var entry = entries[i];
                        if (entry == null)
                            continue;

                        inputIndexMap.Add(entry, i);
                    }

                    // If the number of entries is the same as the old archive, skip out
                    if (inputIndexMap.Keys.Count <= entries.Count)
                    {
                        success = true;
                        return success;
                    }

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
                            // Get temporary date-time if possible
                            DateTime? usableDate = null;
                            if (_realDates && !string.IsNullOrEmpty(baseFiles[-index - 1].Date) && DateTime.TryParse(baseFiles[-index - 1].Date?.Replace('\\', '/'), out DateTime dt))
                                usableDate = dt;

                            // Copy the input file to the output
                            tarFile.AddEntry(baseFiles[-index - 1].Filename!,
                                File.Open(files[-index - 1], FileMode.Open, FileAccess.Read, FileShare.ReadWrite),
                                size: baseFiles[-index - 1].Size ?? 0,
                                modified: usableDate);
                        }

                        // Otherwise, copy the file from the old archive
                        else
                        {
                            // Get the stream from the original archive
                            TarArchiveEntry tae = oldTarFile.Entries.ElementAt(index);
                            MemoryStream entry = new();
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
                _logger.Error(ex);
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

            return true;
#else
            // TODO: Support tape archives in old .NET
            return false;
#endif
        }

        #endregion
    }
}
