using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Schema;

using SabreTools.Library.Data;
using SabreTools.Library.External;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Items;
using SabreTools.Library.Skippers;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using BinaryReader = System.IO.BinaryReader;
using BinaryWriter = System.IO.BinaryWriter;
using FileAccess = System.IO.FileAccess;
using FileMode = System.IO.FileMode;
using FileShare = System.IO.FileShare;
using FileStream = System.IO.FileStream;
using IOException = System.IO.IOException;
using MemoryStream = System.IO.MemoryStream;
using PathTooLongException = System.IO.PathTooLongException;
using SearchOption = System.IO.SearchOption;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
using StreamReader = System.IO.StreamReader;
#endif
using NaturalSort;
using OCRC;
using SharpCompress.Common;

namespace SabreTools.Library.Tools
{
	/// <summary>
	/// Tools for working with non-archive files and stream
	/// </summary>
	public static class FileTools
	{
		#region Factories

		/// <summary>
		/// Create an archive of the specified type, if possible
		/// </summary>
		/// <param name="input">Name of the file to create the archive from</param>
		/// <returns>Archive object representing the inputs</returns>
		public static BaseArchive CreateArchiveFromExistingInput(string input)
		{
			BaseArchive archive = null;

			// First get the archive type
			ArchiveType? at = GetCurrentArchiveType(input);

			// If we got back null, then it's not an archive, so we we return
			if (at == null)
			{
				return archive;
			}

			// Create the archive based on the type
			Globals.Logger.Verbose("Found archive of type: {0}", at);
			switch (at)
			{
				case ArchiveType.GZip:
					archive = new GZipArchive(input);
					break;
				case ArchiveType.Rar:
					archive = new RarArchive(input);
					break;
				case ArchiveType.SevenZip:
					archive = new SevenZipArchive(input);
					break;
				case ArchiveType.Tar:
					archive = new TapeArchive(input);
					break;
				case ArchiveType.Zip:
					archive = new TorrentZipArchive(input);
					break;
			}

			return archive;
		}

		/// <summary>
		/// Create an archive of the specified type, if possible
		/// </summary>
		/// <param name="archiveType">SharpCompress.Common.ArchiveType representing the archive to create</param>
		/// <returns>Archive object representing the inputs</returns>
		public static BaseArchive CreateArchiveFromArchiveType(ArchiveType archiveType)
		{
			switch (archiveType)
			{
				case ArchiveType.GZip:
					return new GZipArchive();
				case ArchiveType.Rar:
					return new RarArchive();
				case ArchiveType.SevenZip:
					return new SevenZipArchive();
				case ArchiveType.Tar:
					return new TapeArchive();
				case ArchiveType.Zip:
					return new TorrentZipArchive();
				default:
					return null;
			}
		}

		/// <summary>
		/// Create an archive of the specified type, if possible
		/// </summary>
		/// <param name="archiveType">SabreTools.Library.Data.SharpCompress.OutputFormat representing the archive to create</param>
		/// <returns>Archive object representing the inputs</returns>
		public static BaseArchive CreateArchiveFromOutputFormat(OutputFormat outputFormat)
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
					return new GZipArchive();
				case OutputFormat.TorrentLRZip:
					return new LRZipArchive();
				case OutputFormat.TorrentLZ4:
					return new LZ4Archive();
				case OutputFormat.TorrentRar:
					return new RarArchive();
				case OutputFormat.TorrentXZ:
					return new XZArchive();
				case OutputFormat.TorrentZip:
					return new TorrentZipArchive();
				case OutputFormat.TorrentZPAQ:
					return new ZPAQArchive();
				case OutputFormat.TorrentZstd:
					return new ZstdArchive();
				default:
					return null;
			}
		}

		#endregion

		#region File Information

		/// <summary>
		/// Get the archive scan level based on the inputs
		/// </summary>
		/// <param name="sevenzip">User-defined scan level for 7z archives</param>
		/// <param name="gzip">User-defined scan level for GZ archives</param>
		/// <param name="rar">User-defined scan level for RAR archives</param>
		/// <param name="zip">User-defined scan level for Zip archives</param>
		/// <returns>ArchiveScanLevel representing the levels</returns>
		public static ArchiveScanLevel GetArchiveScanLevelFromNumbers(int sevenzip, int gzip, int rar, int zip)
		{
			ArchiveScanLevel archiveScanLevel = 0x0000;

			// 7z
			sevenzip = (sevenzip < 0 || sevenzip > 2 ? 0 : sevenzip);
			switch (sevenzip)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.SevenZipBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.SevenZipInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.SevenZipExternal;
					break;
			}

			// GZip
			gzip = (gzip < 0 || gzip > 2 ? 0 : gzip);
			switch (gzip)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.GZipBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.GZipInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.GZipExternal;
					break;
			}

			// RAR
			rar = (rar < 0 || rar > 2 ? 0 : rar);
			switch (rar)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.RarBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.RarInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.RarExternal;
					break;
			}

			// Zip
			zip = (zip < 0 || zip > 2 ? 0 : zip);
			switch (zip)
			{
				case 0:
					archiveScanLevel |= ArchiveScanLevel.ZipBoth;
					break;
				case 1:
					archiveScanLevel |= ArchiveScanLevel.ZipInternal;
					break;
				case 2:
					archiveScanLevel |= ArchiveScanLevel.ZipExternal;
					break;
			}

			return archiveScanLevel;
		}

		/// <summary>
		/// Get internal metadata from a CHD
		/// </summary>
		/// <param name="input">Filename of possible CHD</param>
		/// <returns>A Disk object with internal SHA-1 on success, null on error, empty Disk otherwise</returns>
		/// <remarks>
		/// Original code had a "writable" param. This is not required for metadata checking
		/// </remarks>
		public static DatItem GetCHDInfo(string input)
		{
			FileStream fs = FileTools.TryOpenRead(input);
			DatItem datItem = GetCHDInfo(fs);
			fs.Dispose();
			return datItem;
		}

		/// <summary>
		/// Returns the archive type of an input file
		/// </summary>
		/// <param name="input">Input file to check</param>
		/// <returns>ArchiveType of inputted file (null on error)</returns>
		public static ArchiveType? GetCurrentArchiveType(string input)
		{
			ArchiveType? outtype = null;

			// If the file is null, then we have no archive type
			if (input == null)
			{
				return outtype;
			}

			// First line of defense is going to be the extension, for better or worse
			string ext = Path.GetExtension(input).ToLowerInvariant();
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}

			if (ext != "7z" && ext != "gz" && ext != "lzma" && ext != "rar"
				&& ext != "rev" && ext != "r00" && ext != "r01" && ext != "tar"
				&& ext != "tgz" && ext != "tlz" && ext != "zip" && ext != "zipx")
			{
				return outtype;
			}

			// Read the first bytes of the file and get the magic number
			try
			{
				byte[] magic = new byte[8];
				BinaryReader br = new BinaryReader(FileTools.TryOpenRead(input));
				magic = br.ReadBytes(8);
				br.Dispose();

				// Convert it to an uppercase string
				string mstr = string.Empty;
				for (int i = 0; i < magic.Length; i++)
				{
					mstr += BitConverter.ToString(new byte[] { magic[i] });
				}
				mstr = mstr.ToUpperInvariant();

				// Now try to match it to a known signature
				if (mstr.StartsWith(Constants.SevenZipSig))
				{
					outtype = ArchiveType.SevenZip;
				}
				else if (mstr.StartsWith(Constants.GzSig))
				{
					outtype = ArchiveType.GZip;
				}
				else if (mstr.StartsWith(Constants.RarSig) || mstr.StartsWith(Constants.RarFiveSig))
				{
					outtype = ArchiveType.Rar;
				}
				else if (mstr.StartsWith(Constants.TarSig) || mstr.StartsWith(Constants.TarZeroSig))
				{
					outtype = ArchiveType.Tar;
				}
				else if (mstr.StartsWith(Constants.ZipSig) || mstr.StartsWith(Constants.ZipSigEmpty) || mstr.StartsWith(Constants.ZipSigSpanned))
				{
					outtype = ArchiveType.Zip;
				}
			}
			catch (Exception)
			{
				// Don't log file open errors
			}

			return outtype;
		}

		/// <summary>
		/// Get what type of DAT the input file is
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>The DatFormat corresponding to the DAT</returns>
		/// <remarks>There is currently no differentiation between XML and SabreDAT here</remarks>
		public static DatFormat GetDatFormat(string filename)
		{
			// Limit the output formats based on extension
			string ext = Path.GetExtension(filename).ToLowerInvariant();
			if (ext.StartsWith("."))
			{
				ext = ext.Substring(1);
			}
			if (ext != "csv" && ext != "dat" && ext != "md5" && ext != "sfv" && ext != "sha1"
				&& ext != "sha384" && ext != "sha512" && ext != "tsv" && ext != "txt" && ext != "xml")
			{
				return 0;
			}

			// Read the input file, if possible
			Globals.Logger.Verbose("Attempting to read file to get format: {0}", filename);

			// Check if file exists
			if (!File.Exists(filename))
			{
				Globals.Logger.Warning("File '{0}' could not read from!", filename);
				return 0;
			}

			// Some formats should only require the extension to know
			if (ext == "md5")
			{
				return DatFormat.RedumpMD5;
			}
			if (ext == "sfv")
			{
				return DatFormat.RedumpSFV;
			}
			if (ext == "sha1")
			{
				return DatFormat.RedumpSHA1;
			}
			if (ext == "sha256")
			{
				return DatFormat.RedumpSHA256;
			}
			if (ext == "sha384")
			{
				return DatFormat.RedumpSHA384;
			}
			if (ext == "sha512")
			{
				return DatFormat.RedumpSHA512;
			}
			if (ext == "csv")
			{
				return DatFormat.CSV;
			}
			if (ext == "tsv")
			{
				return DatFormat.TSV;
			}

			// For everything else, we need to read it
			try
			{
				// Get the first two lines to check
				StreamReader sr = File.OpenText(filename);
				string first = sr.ReadLine().ToLowerInvariant();
				string second = sr.ReadLine().ToLowerInvariant();
				sr.Dispose();

				// If we have an XML-based DAT
				if (first.Contains("<?xml") && first.Contains("?>"))
				{
					if (second.StartsWith("<!doctype datafile"))
					{
						return DatFormat.Logiqx;
					}
					else if (second.StartsWith("<!doctype softwarelist"))
					{
						return DatFormat.SoftwareList;
					}
					else if (second.StartsWith("<!doctype sabredat"))
					{
						return DatFormat.SabreDat;
					}
					else if (second.StartsWith("<dat") && !second.StartsWith("<datafile"))
					{
						return DatFormat.OfflineList;
					}
					// Older and non-compliant DATs
					else
					{
						return DatFormat.Logiqx;
					}
				}

				// If we have an INI-based DAT
				else if (first.Contains("[") && first.Contains("]"))
				{
					return DatFormat.RomCenter;
				}

				// If we have a listroms DAT
				else if (first.StartsWith("roms required for driver"))
				{
					return DatFormat.Listroms;
				}

				// If we have a CMP-based DAT
				else if (first.Contains("clrmamepro"))
				{
					return DatFormat.ClrMamePro;
				}
				else if (first.Contains("romvault"))
				{
					return DatFormat.ClrMamePro;
				}
				else if (first.Contains("doscenter"))
				{
					return DatFormat.DOSCenter;
				}
				else if (first.Contains("#Name;Title;Emulator;CloneOf;Year;Manufacturer;Category;Players;Rotation;Control;Status;DisplayCount;DisplayType;AltRomname;AltTitle;Extra"))
				{
					return DatFormat.AttractMode;
				}
				else
				{
					return DatFormat.ClrMamePro;
				}
			}
			catch (Exception)
			{
				return 0;
			}
		}

		/// <summary>
		/// Get all empty folders within a root folder
		/// </summary>
		/// <param name="root">Root directory to parse</param>
		/// <returns>IEumerable containing all directories that are empty, an empty enumerable if the root is empty, null otherwise</returns>
		public static IEnumerable<string> GetEmptyDirectories(string root)
		{
			// Check if the root exists first
			if (!Directory.Exists(root))
			{
				return null;
			}

			// If it does and it is empty, return a blank enumerable
			if (Directory.EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories).Count() == 0)
			{
				return new List<string>();
			}

			// Otherwise, get the complete list
			return Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
				.Where(dir => Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.AllDirectories).Count() == 0);
		}

		/// <summary>
		/// Retrieve file information for a single file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
		/// <param name="offset">Set a >0 number for getting hash for part of the file, 0 otherwise (default)</param>
		/// <param name="date">True if the file Date should be included, false otherwise (default)</param>
		/// <param name="header">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		/// <returns>Populated DatItem object if success, empty one on error</returns>
		public static DatItem GetFileInfo(string input, Hash omitFromScan = 0x0,
			long offset = 0, bool date = false, string header = null, bool chdsAsFiles = true)
		{
			// Add safeguard if file doesn't exist
			if (!File.Exists(input))
			{
				return new Rom();
			}

			// Get the information from the file stream
			DatItem datItem = new Rom();
			if (header != null)
			{
				SkipperRule rule = Skipper.GetMatchingRule(input, Path.GetFileNameWithoutExtension(header));

				// If there's a match, get the new information from the stream
				if (rule.Tests != null && rule.Tests.Count != 0)
				{
					// Create the input and output streams
					MemoryStream outputStream = new MemoryStream();
					FileStream inputStream = FileTools.TryOpenRead(input);

					// Transform the stream and get the information from it
					rule.TransformStream(inputStream, outputStream, keepReadOpen: false, keepWriteOpen: true);
					datItem = GetStreamInfo(outputStream, outputStream.Length, omitFromScan: omitFromScan, keepReadOpen: false, chdsAsFiles: chdsAsFiles);

					// Dispose of the streams
					outputStream.Dispose();
					inputStream.Dispose();
				}
				// Otherwise, just get the info
				else
				{
					long length = new FileInfo(input).Length;
					datItem = GetStreamInfo(TryOpenRead(input), length, omitFromScan, offset, keepReadOpen: false, chdsAsFiles: chdsAsFiles);
				}
			}
			else
			{
				long length = new FileInfo(input).Length;
				datItem = GetStreamInfo(TryOpenRead(input), length, omitFromScan, offset, keepReadOpen: false, chdsAsFiles: chdsAsFiles);
			}

			// Add unique data from the file
			datItem.Name = Path.GetFileName(input);
			if (datItem.Type == ItemType.Rom)
			{
				((Rom)datItem).Date = (date ? new FileInfo(input).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss") : "");
			}

			return datItem;
		}

		/// <summary>
		/// Get if the current file should be scanned internally and externally
		/// </summary>
		/// <param name="input">Name of the input file to check</param>
		/// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
		/// <param name="shouldExternalProcess">Output parameter determining if file should be processed externally</param>
		/// <param name="shouldInternalProcess">Output parameter determining if file should be processed internally</param>
		public static void GetInternalExternalProcess(string input, ArchiveScanLevel archiveScanLevel,
			out bool shouldExternalProcess, out bool shouldInternalProcess)
		{
			shouldExternalProcess = true;
			shouldInternalProcess = true;

			ArchiveType? archiveType = GetCurrentArchiveType(input);
			switch (archiveType)
			{
				case null:
					shouldExternalProcess = true;
					shouldInternalProcess = false;
					break;
				case ArchiveType.GZip:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.GZipExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.GZipInternal) != 0);
					break;
				case ArchiveType.Rar:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.RarExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.RarInternal) != 0);
					break;
				case ArchiveType.SevenZip:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.SevenZipExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.SevenZipInternal) != 0);
					break;
				case ArchiveType.Zip:
					shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.ZipExternal) != 0);
					shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.ZipInternal) != 0);
					break;
			}
		}

		/// <summary>
		/// Get if file is a valid CHD
		/// </summary>
		/// <param name="input">Filename of possible CHD</param>
		/// <returns>True if a the file is a valid CHD, false otherwise</returns>
		public static bool IsValidCHD(string input)
		{
			DatItem datItem = GetCHDInfo(input);
			return datItem != null
				&& datItem.Type == ItemType.Disk
				&& ((Disk)datItem).SHA1 != null;
		}

		/// <summary>
		/// Retrieve a list of files from a directory recursively in proper order
		/// </summary>
		/// <param name="directory">Directory to parse</param>
		/// <param name="infiles">List representing existing files</param>
		/// <returns>List with all new files</returns>
		public static List<string> RetrieveFiles(string directory, List<string> infiles)
		{
			// Take care of the files in the top directory
			List<string> toadd = Directory.EnumerateFiles(directory, "*", SearchOption.TopDirectoryOnly).ToList();
			toadd.Sort(new NaturalComparer());
			infiles.AddRange(toadd);

			// Then recurse through and add from the directories
			List<string> dirs = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly).ToList();
			dirs.Sort(new NaturalComparer());
			foreach (string dir in dirs)
			{
				infiles = RetrieveFiles(dir, infiles);
			}

			// Return the new list
			return infiles;
		}

		#endregion

		#region File Manipulation

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted file
		/// </summary>
		/// <param name="input">File to be appended to</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToAddToHead">String representing bytes to be added to head of file</param>
		/// <param name="bytesToAddToTail">String representing bytes to be added to tail of file</param>
		public static void AppendBytesToFile(string input, string output, string bytesToAddToHead, string bytesToAddToTail)
		{
			// Source: http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
			byte[] bytesToAddToHeadArray = new byte[bytesToAddToHead.Length / 2];
			for (int i = 0; i < bytesToAddToHead.Length; i += 2)
			{
				bytesToAddToHeadArray[i / 2] = Convert.ToByte(bytesToAddToHead.Substring(i, 2), 16);
			}
			byte[] bytesToAddToTailArray = new byte[bytesToAddToTail.Length / 2];
			for (int i = 0; i < bytesToAddToTail.Length; i += 2)
			{
				bytesToAddToTailArray[i / 2] = Convert.ToByte(bytesToAddToTail.Substring(i, 2), 16);
			}

			AppendBytesToFile(input, output, bytesToAddToHeadArray, bytesToAddToTailArray);
		}

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted file
		/// </summary>
		/// <param name="input">File to be appended to</param>
		/// <param name="output">Outputted file</param>
		/// <param name="bytesToAddToHead">Bytes to be added to head of file</param>
		/// <param name="bytesToAddToTail">Bytes to be added to tail of file</param>
		public static void AppendBytesToFile(string input, string output, byte[] bytesToAddToHead, byte[] bytesToAddToTail)
		{
			// If any of the inputs are invalid, skip
			if (!File.Exists(input))
			{
				return;
			}

			FileStream fsr = TryOpenRead(input);
			FileStream fsw = TryOpenWrite(output);

			AppendBytesToStream(fsr, fsw, bytesToAddToHead, bytesToAddToTail);

			fsr.Dispose();
			fsw.Dispose();
		}

		/// <summary>
		/// Cleans out the temporary directory
		/// </summary>
		/// <param name="dirname">Name of the directory to clean out</param>
		public static void CleanDirectory(string dirname)
		{
			foreach (string file in Directory.EnumerateFiles(dirname, "*", SearchOption.TopDirectoryOnly))
			{
				TryDeleteFile(file);
			}
			foreach (string dir in Directory.EnumerateDirectories(dirname, "*", SearchOption.TopDirectoryOnly))
			{
				TryDeleteDirectory(dir);
			}
		}

		/// <summary>
		/// Detect header skipper compliance and create an output file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		/// <param name="outDir">Output directory to write the file to, empty means the same directory as the input file</param>
		/// <param name="nostore">True if headers should not be stored in the database, false otherwise</param>
		/// <returns>True if the output file was created, false otherwise</returns>
		public static bool DetectSkipperAndTransform(string file, string outDir, bool nostore)
		{
			// Create the output directory if it doesn't exist
			if (outDir != "" && !Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			Globals.Logger.User("\nGetting skipper information for '{0}'", file);

			// Get the skipper rule that matches the file, if any
			SkipperRule rule = Skipper.GetMatchingRule(file, "");

			// If we have an empty rule, return false
			if (rule.Tests == null || rule.Tests.Count == 0 || rule.Operation != HeaderSkipOperation.None)
			{
				return false;
			}

			Globals.Logger.User("File has a valid copier header");

			// Get the header bytes from the file first
			string hstr = string.Empty;
			BinaryReader br = new BinaryReader(TryOpenRead(file));

			// Extract the header as a string for the database
			byte[] hbin = br.ReadBytes((int)rule.StartOffset);
			hstr = Style.ByteArrayToString(hbin);
			br.Dispose();

			// Apply the rule to the file
			string newfile = (outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file)));
			rule.TransformFile(file, newfile);

			// If the output file doesn't exist, return false
			if (!File.Exists(newfile))
			{
				return false;
			}

			// Now add the information to the database if it's not already there
			if (!nostore)
			{
				Rom rom = (Rom)GetFileInfo(newfile, chdsAsFiles: true);
				DatabaseTools.AddHeaderToDatabase(hstr, rom.SHA1, rule.SourceFile);
			}

			return true;
		}

		/// <summary>
		/// Retrieve a list of just files from inputs
		/// </summary>
		/// <param name="inputs">List of strings representing directories and files</param>
		/// <param name="appendparent">True if the parent name should be appended after the special character "¬", false otherwise (default)</param>
		/// <returns>List of strings representing just files from the inputs</returns>
		public static List<string> GetOnlyFilesFromInputs(List<string> inputs, bool appendparent = false)
		{
			List<string> outputs = new List<string>();
			foreach (string input in inputs)
			{
				if (Directory.Exists(input))
				{
					List<string> files = FileTools.RetrieveFiles(input, new List<string>());
					foreach (string file in files)
					{
						try
						{
							outputs.Add(Path.GetFullPath(file) + (appendparent ? "¬" + Path.GetFullPath(input) : ""));
						}
						catch (PathTooLongException)
						{
							Globals.Logger.Warning("The path for '{0}' was too long", file);
						}
						catch (Exception ex)
						{
							Globals.Logger.Error(ex.ToString());
						}
					}
				}
				else if (File.Exists(input))
				{
					try
					{
						outputs.Add(Path.GetFullPath(input) + (appendparent ? "¬" + Path.GetFullPath(input) : ""));
					}
					catch (PathTooLongException)
					{
						Globals.Logger.Warning("The path for '{0}' was too long", input);
					}
					catch (Exception ex)
					{
						Globals.Logger.Error(ex.ToString());
					}
				}
			}

			return outputs;
		}

		/// <summary>
		/// Get the XmlTextReader associated with a file, if possible
		/// </summary>
		/// <param name="filename">Name of the file to be parsed</param>
		/// <returns>The XmlTextReader representing the (possibly converted) file, null otherwise</returns>
		public static XmlReader GetXmlTextReader(string filename)
		{
			Globals.Logger.Verbose("Attempting to read file: {0}", filename);

			// Check if file exists
			if (!File.Exists(filename))
			{
				Globals.Logger.Warning("File '{0}' could not read from!", filename);
				return null;
			}

			XmlReader xtr = XmlReader.Create(filename, new XmlReaderSettings
			{
				CheckCharacters = false,
				DtdProcessing = DtdProcessing.Ignore,
				IgnoreComments = true,
				IgnoreWhitespace = true,
				ValidationFlags = XmlSchemaValidationFlags.None,
				ValidationType = ValidationType.None,
			});
			return xtr;
		}

		/// <summary>
		/// Detect and replace header(s) to the given file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		/// <param name="outDir">Output directory to write the file to, empty means the same directory as the input file</param>
		/// <returns>True if a header was found and appended, false otherwise</returns>
		public static bool RestoreHeader(string file, string outDir)
		{
			// Create the output directory if it doesn't exist
			if (outDir != "" && !Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// First, get the SHA-1 hash of the file
			Rom rom = (Rom)GetFileInfo(file, chdsAsFiles: true);

			// Retrieve a list of all related headers from the database
			List<string> headers = DatabaseTools.RetrieveHeadersFromDatabase(rom.SHA1);

			// If we have nothing retrieved, we return false
			if (headers.Count == 0)
			{
				return false;
			}

			// Now loop through and create the reheadered files, if possible
			for (int i = 0; i < headers.Count; i++)
			{
				Globals.Logger.User("Creating reheadered file: " +
						(outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file))) + i);
				AppendBytesToFile(file,
					(outDir == "" ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file))) + i, headers[i], string.Empty);
				Globals.Logger.User("Reheadered file created!");
			}

			return true;
		}

		/// <summary>
		/// Try to create a file for write, optionally throwing the error
		/// </summary>
		/// <param name="file">Name of the file to create</param>
		/// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
		/// <returns>An opened stream representing the file on success, null otherwise</returns>
		public static FileStream TryCreate(string file, bool throwOnError = false)
		{
			// Now wrap opening the file
			try
			{
				return File.Open(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			}
			catch (Exception ex)
			{
				if (throwOnError)
				{
					throw ex;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Try to safely delete a directory, optionally throwing the error
		/// </summary>
		/// <param name="file">Name of the directory to delete</param>
		/// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
		/// <returns>True if the file didn't exist or could be deleted, false otherwise</returns>
		public static bool TryDeleteDirectory(string file, bool throwOnError = false)
		{
			// Check if the file exists first
			if (!Directory.Exists(file))
			{
				return true;
			}

			// Now wrap deleting the file
			try
			{
				Directory.Delete(file, true);
				return true;
			}
			catch (Exception ex)
			{
				if (throwOnError)
				{
					throw ex;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Try to safely delete a file, optionally throwing the error
		/// </summary>
		/// <param name="file">Name of the file to delete</param>
		/// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
		/// <returns>True if the file didn't exist or could be deleted, false otherwise</returns>
		public static bool TryDeleteFile(string file, bool throwOnError = false)
		{
			// Check if the file exists first
			if (!File.Exists(file))
			{
				return true;
			}

			// Now wrap deleting the file
			try
			{
				File.Delete(file);
				return true;
			}
			catch (Exception ex)
			{
				if (throwOnError)
				{
					throw ex;
				}
				else
				{
					return false;
				}
			}
		}

		/// <summary>
		/// Try to open a file for read, optionally throwing the error
		/// </summary>
		/// <param name="file">Name of the file to open</param>
		/// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
		/// <returns>An opened stream representing the file on success, null otherwise</returns>
		public static FileStream TryOpenRead(string file, bool throwOnError = false)
		{
			// Check if the file exists first
			if (!File.Exists(file))
			{
				return null;
			}

			// Now wrap opening the file
			try
			{
				return File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}
			catch (Exception ex)
			{
				if (throwOnError)
				{
					throw ex;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Try to open a file for read/write, optionally throwing the error
		/// </summary>
		/// <param name="file">Name of the file to open</param>
		/// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
		/// <returns>An opened stream representing the file on success, null otherwise</returns>
		public static FileStream TryOpenReadWrite(string file, bool throwOnError = false)
		{
			// Check if the file exists first
			if (!File.Exists(file))
			{
				return null;
			}

			// Now wrap opening the file
			try
			{
				return File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
			}
			catch (Exception ex)
			{
				if (throwOnError)
				{
					throw ex;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Try to open a file for write, optionally throwing the error
		/// </summary>
		/// <param name="file">Name of the file to open</param>
		/// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
		/// <returns>An opened stream representing the file on success, null otherwise</returns>
		public static FileStream TryOpenWrite(string file, bool throwOnError = false)
		{
			// Check if the file exists first
			if (!File.Exists(file))
			{
				return null;
			}

			// Now wrap opening the file
			try
			{
				return File.Open(file, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
			}
			catch (Exception ex)
			{
				if (throwOnError)
				{
					throw ex;
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		#region Stream Information

		/// <summary>
		/// Retrieve file information for a single file
		/// </summary>
		/// <param name="input">Filename to get information from</param>
		/// <param name="size">Size of the input stream</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
		/// <param name="offset">Set a >0 number for getting hash for part of the file, 0 otherwise (default)</param>
		/// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		/// <returns>Populated DatItem object if success, empty one on error</returns>
		public static DatItem GetStreamInfo(Stream input, long size, Hash omitFromScan = 0x0,
			long offset = 0, bool keepReadOpen = false, bool chdsAsFiles = true)
		{
			// We first check to see if it's a CHD
			if (chdsAsFiles == false && IsValidCHD(input))
			{
				// Seek to the starting position, if one is set
				try
				{
					if (offset < 0)
					{
						input.Seek(offset, SeekOrigin.End);
					}
					else if (offset > 0)
					{
						input.Seek(offset, SeekOrigin.Begin);
					}
				}
				catch (NotSupportedException)
				{
					Globals.Logger.Verbose("Stream does not support seeking. Stream position not changed");
				}
				catch (NotImplementedException)
				{
					Globals.Logger.Warning("Stream does not support seeking. Stream position not changed");
				}

				// Get the Disk from the information
				DatItem disk = GetCHDInfo(input);

				// Seek to the beginning of the stream if possible
				try
				{
					input.Seek(0, SeekOrigin.Begin);
				}
				catch (NotSupportedException)
				{
					Globals.Logger.Verbose("Stream does not support seeking. Stream position not changed");
				}
				catch (NotImplementedException)
				{
					Globals.Logger.Verbose("Stream does not support seeking. Stream position not changed");
				}

				if (!keepReadOpen)
				{
					input.Dispose();
				}

				return disk;
			}

			Rom rom = new Rom
			{
				Type = ItemType.Rom,
				Size = size,
				CRC = string.Empty,
				MD5 = string.Empty,
				SHA1 = string.Empty,
				SHA256 = string.Empty,
				SHA384 = string.Empty,
				SHA512 = string.Empty,
			};

			try
			{
				// Initialize the hashers
				OptimizedCRC crc = new OptimizedCRC();
				MD5 md5 = MD5.Create();
				SHA1 sha1 = SHA1.Create();
				SHA256 sha256 = SHA256.Create();
				SHA384 sha384 = SHA384.Create();
				SHA512 sha512 = SHA512.Create();
				xxHash xxHash = new xxHash();
				xxHash.Init();

				// Seek to the starting position, if one is set
				try
				{
					if (offset < 0)
					{
						input.Seek(offset, SeekOrigin.End);
					}
					else if (offset > 0)
					{
						input.Seek(offset, SeekOrigin.Begin);
					}
				}
				catch (NotSupportedException)
				{
					Globals.Logger.Verbose("Stream does not support seeking. Stream position not changed");
				}
				catch (NotImplementedException)
				{
					Globals.Logger.Warning("Stream does not support seeking. Stream position not changed");
				}

				byte[] buffer = new byte[8 * 1024];
				int read;
				while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
				{
					crc.Update(buffer, 0, read);
					if ((omitFromScan & Hash.MD5) == 0)
					{
						md5.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.SHA1) == 0)
					{
						sha1.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.SHA256) == 0)
					{
						sha256.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.SHA384) == 0)
					{
						sha384.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.SHA512) == 0)
					{
						sha512.TransformBlock(buffer, 0, read, buffer, 0);
					}
					if ((omitFromScan & Hash.xxHash) == 0)
					{
						xxHash.Update(buffer, read);
					}
				}

				crc.Update(buffer, 0, 0);
				rom.CRC = crc.Value.ToString("X8").ToLowerInvariant();

				if ((omitFromScan & Hash.MD5) == 0)
				{
					md5.TransformFinalBlock(buffer, 0, 0);
					rom.MD5 = Style.ByteArrayToString(md5.Hash);
				}
				if ((omitFromScan & Hash.SHA1) == 0)
				{
					sha1.TransformFinalBlock(buffer, 0, 0);
					rom.SHA1 = Style.ByteArrayToString(sha1.Hash);
				}
				if ((omitFromScan & Hash.SHA256) == 0)
				{
					sha256.TransformFinalBlock(buffer, 0, 0);
					rom.SHA256 = Style.ByteArrayToString(sha256.Hash);
				}
				if ((omitFromScan & Hash.SHA384) == 0)
				{
					sha384.TransformFinalBlock(buffer, 0, 0);
					rom.SHA384 = Style.ByteArrayToString(sha384.Hash);
				}
				if ((omitFromScan & Hash.SHA512) == 0)
				{
					sha512.TransformFinalBlock(buffer, 0, 0);
					rom.SHA512 = Style.ByteArrayToString(sha512.Hash);
				}
				if ((omitFromScan & Hash.xxHash) == 0)
				{
					//rom.xxHash = xxHash.Digest().ToString("X8").ToLowerInvariant();
				}

				// Dispose of the hashers
				crc.Dispose();
				md5.Dispose();
				sha1.Dispose();
				sha256.Dispose();
				sha384.Dispose();
				sha512.Dispose();
			}
			catch (IOException)
			{
				return new Rom();
			}
			finally
			{
				// Seek to the beginning of the stream if possible
				try
				{
					input.Seek(0, SeekOrigin.Begin);
				}
				catch (NotSupportedException)
				{
					Globals.Logger.Verbose("Stream does not support seeking. Stream position not changed");
				}
				catch (NotImplementedException)
				{
					Globals.Logger.Verbose("Stream does not support seeking. Stream position not changed");
				}

				if (!keepReadOpen)
				{
					input.Dispose();
				}
			}

			return rom;
		}

		/// <summary>
		/// Get internal metadata from a CHD
		/// </summary>
		/// <param name="input">Stream of possible CHD</param>
		/// <returns>A Disk object with internal SHA-1 on success, null on error, empty Disk otherwise</returns>
		/// <remarks>
		/// Original code had a "writable" param. This is not required for metadata checking
		/// </remarks>
		public static DatItem GetCHDInfo(Stream input)
		{
			// Create a blank Disk to populate and return
			Disk datItem = new Disk();

			// Get a CHD object to store the data
			CHDFile chd = new CHDFile(input);

			// Get the SHA-1 from the chd
			byte[] sha1 = chd.GetSHA1FromHeader();

			// Set the SHA-1 of the Disk to return
			datItem.SHA1 = (sha1 == null ? null : Style.ByteArrayToString(sha1));

			return datItem;
		}

		/// <summary>
		/// Get if stream is a valid CHD
		/// </summary>
		/// <param name="input">Stream of possible CHD</param>
		/// <returns>True if a the file is a valid CHD, false otherwise</returns>
		public static bool IsValidCHD(Stream input)
		{
			DatItem datItem = GetCHDInfo(input);
			return datItem != null
				&& datItem.Type == ItemType.Disk
				&& ((Disk)datItem).SHA1 != null;
		}

		#endregion

		#region Stream Manipulation

		/// <summary>
		/// Add an aribtrary number of bytes to the inputted stream
		/// </summary>
		/// <param name="input">Stream to be appended to</param>
		/// <param name="output">Outputted stream</param>
		/// <param name="bytesToAddToHead">Bytes to be added to head of stream</param>
		/// <param name="bytesToAddToTail">Bytes to be added to tail of stream</param>
		public static void AppendBytesToStream(Stream input, Stream output, byte[] bytesToAddToHead, byte[] bytesToAddToTail)
		{
			BinaryReader br = new BinaryReader(input);
			BinaryWriter bw = new BinaryWriter(output);

			if (bytesToAddToHead.Count() > 0)
			{
				bw.Write(bytesToAddToHead);
			}

			int bufferSize = 1024;

			// Now read the file in chunks and write out
			byte[] buffer = new byte[bufferSize];
			while (br.BaseStream.Position <= (br.BaseStream.Length - bufferSize))
			{
				buffer = br.ReadBytes(bufferSize);
				bw.Write(buffer);
			}

			// For the final chunk, if any, write out only that number of bytes
			int length = (int)(br.BaseStream.Length - br.BaseStream.Position);
			buffer = new byte[length];
			buffer = br.ReadBytes(length);
			bw.Write(buffer);

			if (bytesToAddToTail.Count() > 0)
			{
				bw.Write(bytesToAddToTail);
			}
		}

		#endregion
	}
}
