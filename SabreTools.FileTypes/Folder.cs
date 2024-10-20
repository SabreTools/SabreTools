﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SabreTools.Core.Tools;
using SabreTools.FileTypes.Archives;
using SabreTools.IO;
using SabreTools.IO.Extensions;
using SabreTools.Logging;

namespace SabreTools.FileTypes
{
    /// <summary>
    /// Represents a folder for reading and writing
    /// </summary>
    public class Folder : BaseFile
    {
        #region Protected instance variables

        protected List<BaseFile>? _children;

        /// <summary>
        /// Logging object
        /// </summary>
        protected Logger logger;

        /// <summary>
        /// Static logger for static methods
        /// </summary>
        protected static Logger staticLogger = new();

        /// <summary>
        /// Flag specific to Folder to omit Machine name from output path
        /// </summary>
        private readonly bool _writeToParent = false;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new folder with no base file
        /// </summary>
        /// <param name="writeToParent">True to write directly to parent, false otherwise</param>
        public Folder(bool writeToParent = false)
            : base()
        {
            _writeToParent = writeToParent;
            logger = new Logger(this);
        }

        /// <summary>
        /// Create a new folder from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        /// <param name="read">True for opening file as read, false for opening file as write</param>
        /// <param name="getHashes">True if hashes for this file should be calculated, false otherwise (default)</param>
        public Folder(string filename, bool getHashes = false)
            : base(filename, getHashes)
        {
            logger = new Logger(this);
        }

        /// <summary>
        /// Create an folder object of the specified type, if possible
        /// </summary>
        /// <param name="outputFormat">OutputFormat representing the archive to create</param>
        /// <returns>Archive object representing the inputs</returns>
        public static Folder? Create(OutputFormat outputFormat)
        {
            return outputFormat switch
            {
                OutputFormat.Folder => new Folder(false),
                OutputFormat.ParentFolder => new Folder(true),
                OutputFormat.TapeArchive => new TapeArchive(),
                OutputFormat.Torrent7Zip => new SevenZipArchive(),
                OutputFormat.TorrentGzip => new GZipArchive(),
                OutputFormat.TorrentGzipRomba => new GZipArchive(),
                OutputFormat.TorrentRar => new RarArchive(),
                OutputFormat.TorrentXZ => new XZArchive(),
                OutputFormat.TorrentXZRomba => new XZArchive(),
                OutputFormat.TorrentZip => new ZipArchive(),
                _ => null,
            };
        }

        #endregion

        #region Extraction

        /// <summary>
        /// Attempt to extract a file as an archive
        /// </summary>
        /// <param name="outDir">Output directory for archive extraction</param>
        /// <returns>True if the extraction was a success, false otherwise</returns>
        public virtual bool CopyAll(string outDir)
        {
            // If we have an invalid filename
            if (Filename == null)
                return false;

            // Copy all files from the current folder to the output directory recursively
            try
            {
                // Make sure the folders exist
                Directory.CreateDirectory(Filename);
                Directory.CreateDirectory(outDir);

                DirectoryCopy(Filename, outDir, true);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }

            return true;
        }

        // https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        /// <summary>
        /// Attempt to extract a file from an archive
        /// </summary>
        /// <param name="entryName">Name of the entry to be extracted</param>
        /// <param name="outDir">Output directory for archive extraction</param>
        /// <returns>Name of the extracted file, null on error</returns>
        public virtual string? CopyToFile(string entryName, string outDir)
        {
            string? realentry = null;

            // If we have an invalid filename
            if (Filename == null)
                return null;

            // Copy single file from the current folder to the output directory, if exists
            try
            {
                // Make sure the folders exist
                Directory.CreateDirectory(Filename);
                Directory.CreateDirectory(outDir);

                // Get all files from the input directory
                List<string> files = PathTool.GetFilesOrdered(Filename);

                // Now sort through to find the first file that matches
                string? match = files.Where(s => s.EndsWith(entryName)).FirstOrDefault();

                // If we had a file, copy that over to the new name
                if (!string.IsNullOrEmpty(match))
                {
                    realentry = match;
                    File.Copy(match, Path.Combine(outDir, entryName));
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return realentry;
            }

            return realentry;
        }

        /// <summary>
        /// Attempt to extract a stream from an archive
        /// </summary>
        /// <param name="entryName">Name of the entry to be extracted</param>
        /// <returns>Stream representing the entry, null on error</returns>
        public virtual (Stream?, string?) GetEntryStream(string entryName)
        {
            // If we have an invalid filename
            if (Filename == null)
                return (null, null);

            // Copy single file from the current folder to the output directory, if exists
            try
            {
                // Make sure the folders exist
                Directory.CreateDirectory(Filename);

                // Get all files from the input directory
                List<string> files = PathTool.GetFilesOrdered(Filename);

                // Now sort through to find the first file that matches
                string? match = files.Where(s => s.EndsWith(entryName)).FirstOrDefault();

                // If we had a file, open and return the stream
                if (!string.IsNullOrEmpty(match))
                {
                    var stream = File.OpenRead(match);
                    return (stream, match);
                }

                return (null, null);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return (null, null);
            }
        }

        #endregion

        #region Information

        /// <summary>
        /// Generate a list of immediate children from the current folder
        /// </summary>
        /// <returns>List of BaseFile objects representing the found data</returns>
        public virtual List<BaseFile>? GetChildren()
        {
            // If we have an invalid filename
            if (Filename == null)
                return null;

            if (_children == null || _children.Count == 0)
            {
                _children = [];
#if NET20 || NET35
                foreach (string file in Directory.GetFiles(Filename, "*"))
#else
                foreach (string file in Directory.EnumerateFiles(Filename, "*", SearchOption.TopDirectoryOnly))
#endif
                {
                    BaseFile? nf = GetInfo(file, hashes: AvailableHashTypes);
                    if (nf != null)
                        _children.Add(nf);
                }

#if NET20 || NET35
                foreach (string dir in Directory.GetDirectories(Filename, "*"))
#else
                foreach (string dir in Directory.EnumerateDirectories(Filename, "*", SearchOption.TopDirectoryOnly))
#endif
                {
                    Folder fl = new(dir);
                    _children.Add(fl);
                }
            }

            return _children;
        }

        /// <summary>
        /// Generate a list of empty folders in an archive
        /// </summary>
        /// <param name="input">Input file to get data from</param>
        /// <returns>List of empty folders in the folder</returns>
        public virtual List<string>? GetEmptyFolders()
        {
            return Filename.ListEmpty();
        }

        #endregion

        #region Writing

        /// <summary>
        /// Write an input file to an output folder
        /// </summary>
        /// <param name="inputFile">Input filename to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="baseFile">BaseFile representing the new information</param>
        /// <returns>True if the write was a success, false otherwise</returns>
        /// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
        public virtual bool Write(string inputFile, string outDir, BaseFile? baseFile)
        {
            FileStream fs = File.OpenRead(inputFile);
            return Write(fs, outDir, baseFile);
        }

        /// <summary>
        /// Write an input stream to an output folder
        /// </summary>
        /// <param name="inputStream">Input stream to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="baseFile">BaseFile representing the new information</param>
        /// <returns>True if the write was a success, false otherwise</returns>
        /// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
        public virtual bool Write(Stream? inputStream, string outDir, BaseFile? baseFile)
        {
            // If either input is null or empty, return
            if (inputStream == null || baseFile == null || baseFile.Filename == null)
                return false;

            // If the stream is not readable, return
            if (!inputStream.CanRead)
                return false;

            // Set internal variables
            FileStream? outputStream = null;

            // Get the output folder name from the first rebuild rom
            string fileName;
            if (_writeToParent)
                fileName = Path.Combine(outDir, TextHelper.RemovePathUnsafeCharacters(baseFile.Filename) ?? string.Empty);
            else
#if NET20 || NET35
                fileName = Path.Combine(Path.Combine(outDir, TextHelper.RemovePathUnsafeCharacters(baseFile.Parent) ?? string.Empty), TextHelper.RemovePathUnsafeCharacters(baseFile.Filename) ?? string.Empty);
#else
                fileName = Path.Combine(outDir, TextHelper.RemovePathUnsafeCharacters(baseFile.Parent) ?? string.Empty, TextHelper.RemovePathUnsafeCharacters(baseFile.Filename) ?? string.Empty);
#endif

            // Replace any incorrect directory characters
            if (Path.DirectorySeparatorChar == '\\')
                fileName = fileName.Replace('/', '\\');
            else if (Path.DirectorySeparatorChar == '/')
                fileName = fileName.Replace('\\', '/');

            try
            {
                // If the full output path doesn't exist, create it
                string? dir = Path.GetDirectoryName(fileName);
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Overwrite output files by default
                outputStream = File.Create(fileName);
                if (outputStream == null)
                    return false;

                if (inputStream.CanSeek)
                    inputStream.Seek(0, SeekOrigin.Begin);

                // Copy the input stream to the output
                int bufferSize = 4096 * 128;
                byte[] ibuffer = new byte[bufferSize];
                int ilen;
                while ((ilen = inputStream.Read(ibuffer, 0, bufferSize)) > 0)
                {
                    outputStream.Write(ibuffer, 0, ilen);
                    outputStream.Flush();
                }

                outputStream.Dispose();

                // Try to set the creation time
                try
                {
                    if (!string.IsNullOrEmpty(baseFile.Date))
                        File.SetCreationTime(fileName, DateTime.Parse(baseFile.Date));
                }
                catch { }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return false;
            }
            finally
            {
                outputStream?.Dispose();
            }
        }

        /// <summary>
        /// Write a set of input files to an output folder (assuming the same output archive name)
        /// </summary>
        /// <param name="inputFiles">Input files to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="baseFiles">BaseFiles representing the new information</param>
        /// <returns>True if the inputs were written properly, false otherwise</returns>
        public virtual bool Write(List<string> inputFiles, string outDir, List<BaseFile>? baseFiles)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
