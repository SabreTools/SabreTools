using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiscUtils;

namespace SabreTools.FileTypes.Archives
{
    /// <summary>
    /// Represents a ISOArchive archive for reading and writing
    /// </summary>
    public class ISOArchive : BaseArchive
    {
        #region Constructors

        /// <summary>
        /// Create a new ISOArchive with no base file
        /// </summary>
        public ISOArchive()
            : base()
        {
            this.Type = FileType.ISOArchive;
        }

        /// <summary>
        /// Create a new ISOArchive from the given file
        /// </summary>
        /// <param name="filename">Name of the file to use as an archive</param>
        /// <param name="read">True for opening file as read, false for opening file as write</param>
        /// <param name="getHashes">True if hashes for this file should be calculated, false otherwise (default)</param>
        public ISOArchive(string filename, bool getHashes = false)
            : base(filename, getHashes)
        {
            this.Type = FileType.ISOArchive;
        }

        #endregion

        #region Abstract functionality
/*
                //compare ISO with DiscUtils 
                //FileSystemManager.DetectDefaultFileSystems um zu schauen welches System auf der ISO ist
                // dann CDReader oder UDFReader nehmen
                using (FileStream isoStream = File.Open(@"C:\temp\sample.iso"))
                {
                CDReader cd = new CDReader(isoStream, true);
                Stream fileStream = cd.OpenFile(@"Folder\Hello.txt", FileMode.Open);
                // Use fileStream...
                }
*/

        public void WriteIsoFolder(DiscFileSystem cdReader, string sIsoPath, string sDestinationRootPath)
        {
            try
            {
                string[] saFiles = cdReader.GetFiles(sIsoPath);
                foreach (string sFile in saFiles)
                {
                    DiscFileInfo dfiIso = cdReader.GetFileInfo(sFile);
                    string sDestinationPath = Path.Combine(sDestinationRootPath, dfiIso.DirectoryName.Substring(0, dfiIso.DirectoryName.Length - 1));
                    if (!Directory.Exists(sDestinationPath))
                        Directory.CreateDirectory(sDestinationPath);
                    
                    string sDestinationFile = Path.Combine(sDestinationPath, dfiIso.Name);
                    DiscUtils.Streams.SparseStream streamIsoFile = cdReader.OpenFile(sFile, FileMode.Open);
                    FileStream fsDest = new FileStream(sDestinationFile, FileMode.Create);
                    byte[] baData = new byte[0x4000];
                    while (true)
                    {
                        int nReadCount = streamIsoFile.Read(baData, 0, baData.Length);
                        if (nReadCount < 1)
                        {
                            break;
                        }
                        else
                        {
                            fsDest.Write(baData, 0, nReadCount);
                        }
                    }
                    streamIsoFile.Close();
                    fsDest.Close();
                }
                string[] saDirectories = cdReader.GetDirectories(sIsoPath);
                foreach (string sDirectory in saDirectories)
                {
                    WriteIsoFolder(cdReader, sDirectory, sDestinationRootPath);
                }
            }
            catch (Exception ex)
            {
                logger.User(ex.ToString());
            }

            return;
        }

        public (MemoryStream, string) WriteIsoFolderToStream(DiscFileSystem cdReader, string sIsoPath, MemoryStream ms, string realEntry, string entryName)
        {
            try
            {
                string[] saFiles = cdReader.GetFiles(sIsoPath);
                foreach (string sFile in saFiles)
                {
                    DiscFileInfo dfiIso = cdReader.GetFileInfo(sFile);
 
                    if (sFile != null && sFile.Contains(entryName))
                    {
                        // Write the file out
                        DiscUtils.Streams.SparseStream streamIsoFile = cdReader.OpenFile(sFile, FileMode.Open);

                        realEntry = sFile;
                        streamIsoFile.CopyTo(ms);
                    }
                }
                string[] saDirectories = cdReader.GetDirectories(sIsoPath);
                foreach (string sDirectory in saDirectories)
                {
                    WriteIsoFolderToStream(cdReader, sDirectory, ms, realEntry, entryName);
                }
            }
            catch (Exception ex)
            {
                logger.User(ex.ToString());
                ms = null;
                realEntry = null;
            }

            return (ms, realEntry);
        }

        public List<BaseFile> ReadIsoFiles(DiscFileSystem cdReader, string sIsoPath, List<BaseFile> found)
        {
            string gamename = Path.GetFileNameWithoutExtension(this.Filename);

            try
            {
                string[] saFiles = cdReader.GetFiles(sIsoPath);
                foreach (string sFile in saFiles)
                {
                    DiscFileInfo dfiIso = cdReader.GetFileInfo(sFile);
                    
                    // Create a blank item for the entry
                    BaseFile ISOEntryRom = new BaseFile();

                    // Otherwise, use the stream directly
                    using Stream entryStream = dfiIso.OpenRead();
                    ISOEntryRom = GetInfo(entryStream, size: dfiIso.Length, hashes: this.AvailableHashes);

                    // Fill in comon details and add to the list
                    ISOEntryRom.Filename = dfiIso.Name;
                    ISOEntryRom.Parent = gamename;

                    found.Add(ISOEntryRom);
                }
                string[] saDirectories = cdReader.GetDirectories(sIsoPath);
                foreach (string sDirectory in saDirectories)
                {
                    ReadIsoFiles(cdReader, sDirectory, found);
                }
            }
            catch (Exception ex)
            {
                logger.User(ex.ToString());
                found = null;
            }

            return found;
        }

        public List<string> ReadIsoEmptyFolders(DiscFileSystem cdReader, string sIsoPath, List<string> empties)
        {
            try
            {
                string[] saDirectories = cdReader.GetDirectories(sIsoPath);
                string lastISOEntry = null;

                foreach (string sDirectory in saDirectories)
                {
                    if (sDirectory != null)
                    {
                        // If the current is a superset of last, we skip it
                        if (lastISOEntry != null && lastISOEntry.StartsWith(sDirectory))
                        {
                            // No-op
                        }
                        // If the entry is a directory, we add it
                        else
                        {
                            empties.Add(sDirectory);
                            lastISOEntry = sDirectory;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.User(ex.ToString());
                empties = null;
            }
            
            return empties;
        }
        #endregion
        #region Extraction

        /// <inheritdoc/>
        public override bool CopyAll(string outDir)
        {
            throw new NotImplementedException();
/*
            bool encounteredErrors = true;

            try
            {
                // Create the temp directory
                Directory.CreateDirectory(outDir);

                // Extract all files to the temp directory
                SharpCompress.Archives.Rar.RarArchive ra = SharpCompress.Archives.Rar.RarArchive.Open(this.Filename);
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
*/
        }

        /// <inheritdoc/>
        public override string CopyToFile(string entryName, string outDir)
        {
            throw new NotImplementedException();
/*
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
*/
        }

        /// <inheritdoc/>
        public override (MemoryStream, string) CopyToStream(string entryName)
        {
            MemoryStream ms = new MemoryStream();
            string realEntry = null;

            Stream inputStream = File.OpenRead(this.Filename);

            try
            {
                DiscUtils.FileSystemInfo[] fsia = FileSystemManager.DetectFileSystems(inputStream);
                if (fsia.Length < 1)
                {
                    logger.User("No valid disc file system detected.");
                }
                else
                {
                    DiscFileSystem dfs = fsia[0].Open(inputStream);                    
                    (ms, realEntry) = WriteIsoFolderToStream(dfs, @"", ms, realEntry, entryName);       
                }

                inputStream.Dispose();
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
            Stream inputStream = File.OpenRead(this.Filename);

            try
            {
                DiscUtils.FileSystemInfo[] fsia = FileSystemManager.DetectFileSystems(inputStream);
                if (fsia.Length < 1)
                {
                    logger.User("No valid disc file system detected.");
                    return null;
                }
                else
                {
                    DiscFileSystem dfs = fsia[0].Open(inputStream);                    
                    found = ReadIsoFiles(dfs, @"", found);       
                }

                inputStream.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
            finally
            {
                inputStream?.Dispose();
            }

            return found;
        }

        /// <inheritdoc/>
        public override List<string> GetEmptyFolders()
        {
            List<string> empties = new List<string>();
            Stream inputStream = File.OpenRead(this.Filename);

            try
            {
                DiscUtils.FileSystemInfo[] fsia = FileSystemManager.DetectFileSystems(inputStream);
                if (fsia.Length < 1)
                {
                    logger.User("No valid disc file system detected.");
                }
                else
                {
                    DiscFileSystem dfs = fsia[0].Open(inputStream);                    
                    empties = ReadIsoEmptyFolders(dfs, @"", empties);       
                }

                inputStream.Dispose();
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                return null;
            }
            finally
            {
                inputStream?.Dispose();
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

            // If either input is null or empty, return
            if (inputStream == null || baseFile == null || baseFile.Filename == null)
                return success;

            // If the stream is not readable, return
            if (!inputStream.CanRead)
                return success;

            try
            {
                DiscUtils.FileSystemInfo[] fsia = FileSystemManager.DetectFileSystems(inputStream);
                if (fsia.Length < 1)
                {
                    logger.User("No valid disc file system detected.");
                }
                else
                {
                    DiscFileSystem dfs = fsia[0].Open(inputStream);                    
                    WriteIsoFolder(dfs, @"", outDir);
                   
                    success = true;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                success = false;
            }
            finally
            {
                inputStream?.Dispose();
            }
            inputStream?.Close();
            inputStream?.Dispose();
            return success;
        }

        /// <inheritdoc/>
        public override bool Write(List<string> inputFiles, string outDir, List<BaseFile> roms)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
