using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.IO;
using SabreTools.Library.Tools;

namespace SabreTools.Library.FileTypes
{
    /// <summary>
    /// Represents a folder for reading and writing
    /// </summary>
    public class Folder : BaseFile
    {
        #region Protected instance variables

        protected List<BaseFile> _children;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new folder with no base file
        /// </summary>
        public Folder()
            : base()
        {
            this.Type = FileType.Folder;
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
            this.Type = FileType.Folder;
        }

        /// <summary>
        /// Create an folder object of the specified type, if possible
        /// </summary>
        /// <param name="outputFormat">SabreTools.Library.Data.OutputFormat representing the archive to create</param>
        /// <returns>Archive object representing the inputs</returns>
        public static Folder Create(OutputFormat outputFormat)
        {
            switch (outputFormat)
            {
                case OutputFormat.Folder:
                    return new Folder();

                case OutputFormat.TapeArchive:
                    return new TapeArchive();

                case OutputFormat.Torrent7Zip:
                    return new SevenZipArchive();

                case OutputFormat.TorrentGzip:
                case OutputFormat.TorrentGzipRomba:
                    return new GZipArchive();

                case OutputFormat.TorrentLRZip:
                    return new LRZipArchive();

                case OutputFormat.TorrentLZ4:
                    return new LZ4Archive();

                case OutputFormat.TorrentRar:
                    return new RarArchive();

                case OutputFormat.TorrentXZ:
                case OutputFormat.TorrentXZRomba:
                    return new XZArchive();

                case OutputFormat.TorrentZip:
                    return new ZipArchive();

                case OutputFormat.TorrentZPAQ:
                    return new ZPAQArchive();

                case OutputFormat.TorrentZstd:
                    return new ZstdArchive();

                default:
                    return null;
            }
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
            // Copy all files from the current folder to the output directory recursively
            try
            {
                // Make sure the folders exist
                Directory.CreateDirectory(this.Filename);
                Directory.CreateDirectory(outDir);

                DirectoryCopy(this.Filename, outDir, true);
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }

            return true;
        }

        // https://docs.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

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
        public virtual string CopyToFile(string entryName, string outDir)
        {
            string realentry = null;

            // Copy single file from the current folder to the output directory, if exists
            try
            {
                // Make sure the folders exist
                Directory.CreateDirectory(this.Filename);
                Directory.CreateDirectory(outDir);

                // Get all files from the input directory
                List<string> files = DirectoryExtensions.GetFilesOrdered(this.Filename);

                // Now sort through to find the first file that matches
                string match = files.Where(s => s.EndsWith(entryName)).FirstOrDefault();

                // If we had a file, copy that over to the new name
                if (!string.IsNullOrWhiteSpace(match))
                {
                    realentry = match;
                    File.Copy(match, Path.Combine(outDir, entryName));
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return realentry;
            }

            return realentry;
        }

        /// <summary>
        /// Attempt to extract a stream from an archive
        /// </summary>
        /// <param name="entryName">Name of the entry to be extracted</param>
        /// <param name="realEntry">Output representing the entry name that was found</param>
        /// <returns>MemoryStream representing the entry, null on error</returns>
        public virtual (MemoryStream, string) CopyToStream(string entryName)
        {
            MemoryStream ms = new MemoryStream();
            string realentry = null;

            // Copy single file from the current folder to the output directory, if exists
            try
            {
                // Make sure the folders exist
                Directory.CreateDirectory(this.Filename);

                // Get all files from the input directory
                List<string> files = DirectoryExtensions.GetFilesOrdered(this.Filename);

                // Now sort through to find the first file that matches
                string match = files.Where(s => s.EndsWith(entryName)).FirstOrDefault();

                // If we had a file, copy that over to the new name
                if (!string.IsNullOrWhiteSpace(match))
                {
                    FileExtensions.TryOpenRead(match).CopyTo(ms);
                    realentry = match;
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return (ms, realentry);
            }

            return (ms, realentry);
        }

        #endregion

        #region Information

        /// <summary>
        /// Generate a list of immediate children from the current folder
        /// </summary>
        /// <param name="omitFromScan">Hash representing the hashes that should be skipped</param>
        /// <param name="date">True if entry dates should be included, false otherwise (default)</param>
        /// <returns>List of BaseFile objects representing the found data</returns>
        /// <remarks>TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually</remarks>
        public virtual List<BaseFile> GetChildren(Hash omitFromScan = Hash.DeepHashes, bool date = false)
        {
            if (_children == null || _children.Count == 0)
            {
                _children = new List<BaseFile>();
                foreach (string file in Directory.EnumerateFiles(this.Filename, "*", SearchOption.TopDirectoryOnly))
                {
                    BaseFile nf = FileExtensions.GetInfo(file, omitFromScan: omitFromScan, date: date);
                    _children.Add(nf);
                }

                foreach (string dir in Directory.EnumerateDirectories(this.Filename, "*", SearchOption.TopDirectoryOnly))
                {
                    Folder fl = new Folder(dir);
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
        public virtual List<string> GetEmptyFolders()
        {
            return DirectoryExtensions.ListEmpty(this.Filename);
        }

        #endregion

        #region Writing

        /// <summary>
        /// Write an input file to an output folder
        /// </summary>
        /// <param name="inputFile">Input filename to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="depth">Positive value for depth of the output depot, defaults to 4</param>
        /// <returns>True if the write was a success, false otherwise</returns>
        /// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
        public virtual bool Write(string inputFile, string outDir, Rom rom, bool date = false, int depth = 4)
        {
            FileStream fs = FileExtensions.TryOpenRead(inputFile);
            return Write(fs, outDir, rom, date, depth);
        }

        /// <summary>
        /// Write an input stream to an output folder
        /// </summary>
        /// <param name="inputStream">Input stream to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="depth">Positive value for depth of the output depot, defaults to 4</param>
        /// <returns>True if the write was a success, false otherwise</returns>
        /// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
        public virtual bool Write(Stream inputStream, string outDir, Rom rom, bool date = false, int depth = 4)
        {
            bool success = false;

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

            // Set internal variables
            FileStream outputStream = null;

            // Get the output folder name from the first rebuild rom
            string fileName = Path.Combine(outDir, Sanitizer.RemovePathUnsafeCharacters(rom.Machine.Name), Sanitizer.RemovePathUnsafeCharacters(rom.Name));

            try
            {
                // If the full output path doesn't exist, create it
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }

                // Overwrite output files by default
                outputStream = FileExtensions.TryCreate(fileName);

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

                    if (rom.ItemType == ItemType.Rom)
                    {
                        if (date && !string.IsNullOrWhiteSpace((rom as Rom).Date))
                        {
                            File.SetCreationTime(fileName, DateTime.Parse((rom as Rom).Date));
                        }
                    }

                    success = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                success = false;
            }
            finally
            {
                inputStream.Dispose();
                outputStream?.Dispose();
            }

            return success;
        }

        /// <summary>
        /// Write a set of input files to an output folder (assuming the same output archive name)
        /// </summary>
        /// <param name="inputFiles">Input files to be moved</param>
        /// <param name="outDir">Output directory to build to</param>
        /// <param name="rom">DatItem representing the new information</param>
        /// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
        /// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
        /// <returns>True if the archive was written properly, false otherwise</returns>
        public virtual bool Write(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false, bool romba = false)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
