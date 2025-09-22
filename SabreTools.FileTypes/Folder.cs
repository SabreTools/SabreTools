using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Core.Tools;
using SabreTools.Hashing;
using SabreTools.IO;
using SabreTools.IO.Extensions;
using SabreTools.IO.Logging;

namespace SabreTools.FileTypes
{
    /// <summary>
    /// Represents a folder for reading and writing
    /// </summary>
    public class Folder : BaseFile, IParent
    {
        #region Protected instance variables

        /// <summary>
        /// Hashes that are available for children
        /// </summary>
        protected HashType[] _hashTypes = [HashType.CRC32, HashType.MD5, HashType.SHA1];

        /// <summary>
        /// Set of children file objects
        /// </summary>
        protected List<BaseFile>? _children;

        /// <summary>
        /// Logging object
        /// </summary>
        protected Logger _logger;

        /// <summary>
        /// Flag specific to Folder to omit Machine name from output path
        /// </summary>
        private readonly bool _writeToParent;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new Folder with no base file
        /// </summary>
        /// <param name="writeToParent">True to write directly to parent, false otherwise</param>
        public Folder(bool writeToParent = false) : base()
        {
            _writeToParent = writeToParent;
            _logger = new Logger(this);
        }

        /// <summary>
        /// Create a new Folder from the given file
        /// </summary>
        /// <param name="filename">Name of the folder to use</param>
        /// <param name="writeToParent">True to write directly to parent, false otherwise</param>
        public Folder(string filename, bool writeToParent = false) : base(filename)
        {
            _writeToParent = writeToParent;
            _logger = new Logger(this);
        }

        #endregion

        #region Extraction

        /// <inheritdoc/>
        public bool CopyAll(string outDir)
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
                _logger.Error(ex);
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

        /// <inheritdoc/>
        public string? CopyToFile(string entryName, string outDir)
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
                string? match = files.Find(s => s.EndsWith(entryName));

                // If we had a file, copy that over to the new name
                if (!string.IsNullOrEmpty(match))
                {
                    realentry = match;
                    File.Copy(match, Path.Combine(outDir, entryName));
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return realentry;
            }

            return realentry;
        }

        /// <inheritdoc/>
        public Stream? GetEntryStream(string entryName, out string? realEntry)
        {
            // Set the default real entry name
            realEntry = null;

            // If we have an invalid filename
            if (Filename == null)
                return null;

            // Copy single file from the current folder to the output directory, if exists
            try
            {
                // Make sure the folders exist
                Directory.CreateDirectory(Filename);

                // Get all files from the input directory
                List<string> files = PathTool.GetFilesOrdered(Filename);

                // Now sort through to find the first file that matches
                string? match = files.Find(s => s.EndsWith(entryName));

                // If we had a file, open and return the stream
                if (!string.IsNullOrEmpty(match))
                    return File.Open(match, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                return null;
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                return null;
            }
        }

        #endregion

        #region Information

        /// <summary>
        /// Set the hash type that can be included in children
        /// </summary>
        public void SetHashType(HashType hashType)
            => SetHashTypes([hashType]);

        /// <summary>
        /// Set the hash types that can be included in children
        /// </summary>
        public void SetHashTypes(HashType[] hashTypes)
            => _hashTypes = hashTypes;

        /// <inheritdoc/>
        public List<BaseFile>? GetChildren()
        {
            // If we have an invalid filename
            if (Filename == null)
                return null;

            // If we already have children
            if (_children != null && _children.Count > 0)
                return _children;

            // Build the child item list
            _children = [];
            foreach (string file in IOExtensions.SafeEnumerateFiles(Filename, "*", SearchOption.TopDirectoryOnly))
            {
                BaseFile? nf = FileTypeTool.GetInfo(file, _hashTypes);
                if (nf != null)
                    _children.Add(nf);
            }

            foreach (string dir in IOExtensions.SafeEnumerateDirectories(Filename, "*", SearchOption.TopDirectoryOnly))
            {
                var fl = new Folder(dir);
                _children.Add(fl);
            }

            return _children;
        }

        /// <inheritdoc/>
        public List<string>? GetEmptyFolders()
        {
            return Filename.ListEmpty();
        }

        #endregion

        #region Writing

        /// <inheritdoc/>
        public bool Write(string file, string outDir, BaseFile? baseFile)
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
        public bool Write(Stream? stream, string outDir, BaseFile? baseFile)
        {
            // If either input is null or empty, return
            if (stream == null || baseFile == null || baseFile.Filename == null)
                return false;

            // If the stream is not readable, return
            if (!stream.CanRead)
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

                if (stream.CanSeek)
                    stream.Seek(0, SeekOrigin.Begin);

                // Copy the input stream to the output
                int bufferSize = 4096 * 128;
                byte[] ibuffer = new byte[bufferSize];
                int ilen;
                while ((ilen = stream.Read(ibuffer, 0, bufferSize)) > 0)
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
                _logger.Error(ex);
                return false;
            }
            finally
            {
                outputStream?.Dispose();
            }
        }

        /// <inheritdoc/>
        public bool Write(List<string> files, string outDir, List<BaseFile>? baseFiles)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
