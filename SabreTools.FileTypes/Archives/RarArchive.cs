﻿using System;
using System.Collections.Generic;
using System.IO;
#if NET462_OR_GREATER || NETCOREAPP
using System.Linq;
using SabreTools.Hashing;
using SabreTools.Matching.Compare;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;
#endif

namespace SabreTools.FileTypes.Archives
{
    /// <summary>
    /// Represents a TorrentRAR archive for reading and writing
    /// </summary>
    public class RarArchive : BaseArchive
    {
        #region Constructors

        /// <summary>
        /// Create a new TorrentRARArchive with no base file
        /// </summary>
        public RarArchive()
            : base()
        {
        }

        /// <summary>
        /// Create a new TorrentRARArchive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        public RarArchive(string filename)
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

            // If we have an invalid file
            if (Filename == null)
                return true;

            try
            {
                // Create the temp directory
                Directory.CreateDirectory(outDir);

                // Extract all files to the temp directory
                SharpCompress.Archives.Rar.RarArchive ra = SharpCompress.Archives.Rar.RarArchive.Open(Filename);
                foreach (RarArchiveEntry entry in ra.Entries)
                {
                    entry.WriteToDirectory(outDir, new SharpCompress.Common.ExtractionOptions { PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
                }
                encounteredErrors = false;
                ra.Dispose();
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
#else
            // TODO: Support RAR archives in old .NET
            return true;
#endif
        }

        /// <inheritdoc/>
        public override string? CopyToFile(string entryName, string outDir)
        {
            // Try to extract a stream using the given information
            (Stream? stream, string? realEntry) = GetEntryStream(entryName);
            if (stream == null || realEntry == null)
                return null;

            // If the stream and the entry name are both non-null, we write to file
            realEntry = Path.Combine(outDir, realEntry);

            // Create the output subfolder now
            Directory.CreateDirectory(Path.GetDirectoryName(realEntry) ?? string.Empty);

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
        public override (Stream?, string?) GetEntryStream(string entryName)
        {
#if NET462_OR_GREATER || NETCOREAPP
            // If we have an invalid file
            if (Filename == null)
                return (null, null);

            try
            {
                Stream? stream = null;
                string? realEntry = null;

                var ra = SharpCompress.Archives.Rar.RarArchive.Open(Filename, new ReaderOptions { LeaveStreamOpen = false, });
                foreach (RarArchiveEntry entry in ra.Entries)
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

                ra.Dispose();
                return (stream, realEntry);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return (null, null);
            }
#else
            // TODO: Support RAR archives in old .NET
            return (null, null);
#endif
        }

        #endregion

        #region Information

        /// <inheritdoc/>
        public override List<BaseFile>? GetChildren()
        {
#if NET462_OR_GREATER || NETCOREAPP
            // If we have an invalid file
            if (Filename == null)
                return null;

            List<BaseFile> found = [];
            string? gamename = Path.GetFileNameWithoutExtension(Filename);

            try
            {
                SharpCompress.Archives.Rar.RarArchive ra = SharpCompress.Archives.Rar.RarArchive.Open(File.OpenRead(Filename));
                foreach (RarArchiveEntry entry in ra.Entries.Where(e => e != null && !e.IsDirectory))
                {
                    // Create a blank item for the entry
                    BaseFile rarEntryRom = new();

                    // Perform a quickscan, if flagged to
                    if (AvailableHashTypes.Length == 1 && AvailableHashTypes[0] == HashType.CRC32)
                    {
                        rarEntryRom.Size = entry.Size;
                        rarEntryRom.CRC = BitConverter.GetBytes(entry.Crc);
                    }
                    // Otherwise, use the stream directly
                    else
                    {
                        using Stream entryStream = entry.OpenEntryStream();
                        rarEntryRom = GetInfo(entryStream, size: entry.Size, hashes: AvailableHashTypes);
                    }

                    // Fill in common details and add to the list
                    rarEntryRom.Filename = entry.Key;
                    rarEntryRom.Parent = gamename;
                    rarEntryRom.Date = entry.LastModifiedTime?.ToString("yyyy/MM/dd HH:mm:ss");
                    found.Add(rarEntryRom);
                }

                // Dispose of the archive
                ra.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }

            return found;
#else
            // TODO: Support RAR archives in old .NET
            return null;
#endif
        }

        /// <inheritdoc/>
        public override List<string> GetEmptyFolders()
        {
#if NET462_OR_GREATER || NETCOREAPP
            List<string> empties = [];

            // If we have an invalid file
            if (Filename == null)
                return empties;

            try
            {
                SharpCompress.Archives.Rar.RarArchive ra = SharpCompress.Archives.Rar.RarArchive.Open(Filename, new ReaderOptions { LeaveStreamOpen = false });
                List<RarArchiveEntry> rarEntries = [.. ra.Entries.OrderBy(e => e.Key ?? string.Empty, new NaturalReversedComparer())];
                string? lastRarEntry = null;
                foreach (RarArchiveEntry entry in rarEntries)
                {
                    if (entry?.Key == null)
                        continue;

                    // If the current is a superset of last, we skip it
                    if (lastRarEntry != null && lastRarEntry.StartsWith(entry.Key))
                    {
                        // No-op
                    }
                    // If the entry is a directory, we add it
                    else if (entry.IsDirectory)
                    {
                        empties.Add(entry.Key);
                        lastRarEntry = entry.Key;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            return empties;
#else
            // TODO: Support RAR archives in old .NET
            return [];
#endif
        }

        /// <inheritdoc/>
        public override bool IsTorrent()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Writing

        /// <inheritdoc/>
        public override bool Write(string inputFile, string outDir, BaseFile? baseFile)
        {
            // Get the file stream for the file and write out
            return Write(File.OpenRead(inputFile), outDir, baseFile);
        }

        /// <inheritdoc/>
        public override bool Write(Stream? inputStream, string outDir, BaseFile? baseFile)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override bool Write(List<string> inputFiles, string outDir, List<BaseFile>? roms)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
