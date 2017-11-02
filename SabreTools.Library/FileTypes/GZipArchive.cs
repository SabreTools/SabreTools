using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.Items;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using BinaryReader = System.IO.BinaryReader;
using BinaryWriter = System.IO.BinaryWriter;
using EndOfStreamException = System.IO.EndOfStreamException;
using FileStream = System.IO.FileStream;
using MemoryStream = System.IO.MemoryStream;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
#endif
using Ionic.Zlib;
using SharpCompress.Common;

namespace SabreTools.Library.FileTypes
{
	/// <summary>
	/// Represents a TorrentGZip archive for reading and writing
	/// </summary>
	public class GZipArchive : BaseArchive
	{
		#region Constructors

		/// <summary>
		/// Create a new TorrentGZipArchive with no base file
		/// </summary>
		public GZipArchive()
			: base()
		{
		}

		/// <summary>
		/// Create a new TorrentGZipArchive from the given file
		/// </summary>
		/// <param name="filename">Name of the file to use as an archive</param>
		/// <param name="read">True for opening file as read, false for opening file as write</param>
		public GZipArchive(string filename)
			: base(filename)
		{
			_archiveType = ArchiveType.GZip;
		}

		#endregion

		#region Extraction

		/// <summary>
		/// Attempt to extract a file as an archive
		/// </summary>
		/// <param name="outDir">Output directory for archive extraction</param>
		/// <returns>True if the extraction was a success, false otherwise</returns>
		public override bool ExtractAll(string outDir)
		{
			bool encounteredErrors = true;

			try
			{
				// Create the temp directory
				Directory.CreateDirectory(outDir);

				// Decompress the _filename stream
				FileStream outstream = FileTools.TryCreate(Path.Combine(outDir, Path.GetFileNameWithoutExtension(_filename)));
				GZipStream gzstream = new GZipStream(FileTools.TryOpenRead(_filename), Ionic.Zlib.CompressionMode.Decompress);
				gzstream.CopyTo(outstream);

				// Dispose of the streams
				outstream.Dispose();
				gzstream.Dispose();

				encounteredErrors = false;
			}
			catch (EndOfStreamException)
			{
				// Catch this but don't count it as an error because SharpCompress is unsafe
			}
			catch (InvalidOperationException)
			{
				encounteredErrors = true;
			}
			catch (Exception)
			{
				// Don't log file open errors
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
		public override string ExtractEntry(string entryName, string outDir)
		{
			// Try to extract a stream using the given information
			(MemoryStream ms, string realEntry) = ExtractEntryStream(entryName);

			// If the memory stream and the entry name are both non-null, we write to file
			if (ms != null && realEntry != null)
			{
				realEntry = Path.Combine(outDir, realEntry);

				// Create the output subfolder now
				Directory.CreateDirectory(Path.GetDirectoryName(realEntry));

				// Now open and write the file if possible
				FileStream fs = FileTools.TryCreate(realEntry);
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
		public override (MemoryStream, string) ExtractEntryStream(string entryName)
		{
			MemoryStream ms = new MemoryStream();
			string realEntry = null;

			try
			{
				// Decompress the _filename stream
				realEntry = Path.GetFileNameWithoutExtension(_filename);
				GZipStream gzstream = new GZipStream(FileTools.TryOpenRead(_filename), Ionic.Zlib.CompressionMode.Decompress);

				// Write the file out
				byte[] gbuffer = new byte[_bufferSize];
				int glen;
				while ((glen = gzstream.Read(gbuffer, 0, _bufferSize)) > 0)
				{

					ms.Write(gbuffer, 0, glen);
					ms.Flush();
				}

				// Dispose of the streams
				gzstream.Dispose();
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
		public override List<Rom> GetArchiveFileInfo(Hash omitFromScan = Hash.DeepHashes, bool date = false)
		{
			List<Rom> found = new List<Rom>();
			string gamename = Path.GetFileNameWithoutExtension(_filename);

			Rom possibleTgz = GetTorrentGZFileInfo();

			// If it was, then add it to the outputs and continue
			if (possibleTgz != null && possibleTgz.Name != null)
			{
				found.Add(possibleTgz);
				return found;
			}

			try
			{
				// If secure hashes are disabled, do a quickscan
				if (omitFromScan == Hash.SecureHashes)
				{
					Rom tempRom = new Rom(gamename, gamename, omitFromScan);
					BinaryReader br = new BinaryReader(FileTools.TryOpenRead(_filename));
					br.BaseStream.Seek(-8, SeekOrigin.End);
					byte[] headercrc = br.ReadBytesReverse(4);
					tempRom.CRC = Style.ByteArrayToString(headercrc);
					tempRom.Size = br.ReadInt32Reverse();
					br.Dispose();

					found.Add(tempRom);
				}
				// Otherwise, use the stream directly
				else
				{
					GZipStream gzstream = new GZipStream(FileTools.TryOpenRead(_filename), Ionic.Zlib.CompressionMode.Decompress);
					Rom gzipEntryRom = (Rom)FileTools.GetStreamInfo(gzstream, gzstream.Length, omitFromScan: omitFromScan);
					gzipEntryRom.Name = gzstream.FileName;
					gzipEntryRom.MachineName = gamename;
					gzipEntryRom.Date = (date && gzstream.LastModified != null ? gzstream.LastModified?.ToString("yyyy/MM/dd hh:mm:ss") : null);
					found.Add(gzipEntryRom);
					gzstream.Dispose();
				}
			}
			catch (Exception)
			{
				// Don't log file open errors
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
			// GZip files don't contain directories
			return new List<string>();
		}

		/// <summary>
		/// Check whether the input file is a standardized format
		/// </summary>
		public override bool IsTorrent()
		{
			// Check for the file existing first
			if (!File.Exists(_filename))
			{
				return false;
			}

			string datum = Path.GetFileName(_filename).ToLowerInvariant();
			long filesize = new FileInfo(_filename).Length;

			// If we have the romba depot files, just skip them gracefully
			if (datum == ".romba_size" || datum == ".romba_size.backup")
			{
				Globals.Logger.Verbose("Romba depot file found, skipping: {0}", _filename);
				return false;
			}

			// Check if the name is the right length
			if (!Regex.IsMatch(datum, @"^[0-9a-f]{" + Constants.SHA1Length + @"}\.gz")) // TODO: When updating to SHA-256, this needs to update to Constants.SHA256Length
			{
				Globals.Logger.Warning("Non SHA-1 filename found, skipping: '{0}'", Path.GetFullPath(_filename));
				return false;
			}

			// Check if the file is at least the minimum length
			if (filesize < 40 /* bytes */)
			{
				Globals.Logger.Warning("Possibly corrupt file '{0}' with size {1}", Path.GetFullPath(_filename), Style.GetBytesReadable(filesize));
				return false;
			}

			// Get the Romba-specific header data
			byte[] header; // Get preamble header for checking
			byte[] headermd5; // MD5
			byte[] headercrc; // CRC
			ulong headersz; // Int64 size
			BinaryReader br = new BinaryReader(FileTools.TryOpenRead(_filename));
			header = br.ReadBytes(12);
			headermd5 = br.ReadBytes(16);
			headercrc = br.ReadBytes(4);
			headersz = br.ReadUInt64();
			br.Dispose();

			// If the header is not correct, return a blank rom
			bool correct = true;
			for (int i = 0; i < header.Length; i++)
			{
				// This is a temp fix to ignore the modification time and OS until romba can be fixed
				if (i == 4 || i == 5 || i == 6 || i == 7 || i == 9)
				{
					continue;
				}
				correct &= (header[i] == Constants.TorrentGZHeader[i]);
			}
			if (!correct)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Retrieve file information for a single torrent GZ file
		/// </summary>
		/// <returns>Populated DatItem object if success, empty one on error</returns>
		public Rom GetTorrentGZFileInfo()
		{
			// Check for the file existing first
			if (!File.Exists(_filename))
			{
				return null;
			}

			string datum = Path.GetFileName(_filename).ToLowerInvariant();
			long filesize = new FileInfo(_filename).Length;

			// If we have the romba depot files, just skip them gracefully
			if (datum == ".romba_size" || datum == ".romba_size.backup")
			{
				Globals.Logger.Verbose("Romba depot file found, skipping: {0}", _filename);
				return null;
			}

			// Check if the name is the right length
			if (!Regex.IsMatch(datum, @"^[0-9a-f]{" + Constants.SHA1Length + @"}\.gz")) // TODO: When updating to SHA-256, this needs to update to Constants.SHA256Length
			{
				Globals.Logger.Warning("Non SHA-1 filename found, skipping: '{0}'", Path.GetFullPath(_filename));
				return null;
			}

			// Check if the file is at least the minimum length
			if (filesize < 40 /* bytes */)
			{
				Globals.Logger.Warning("Possibly corrupt file '{0}' with size {1}", Path.GetFullPath(_filename), Style.GetBytesReadable(filesize));
				return null;
			}

			// Get the Romba-specific header data
			byte[] header; // Get preamble header for checking
			byte[] headermd5; // MD5
			byte[] headercrc; // CRC
			ulong headersz; // Int64 size
			BinaryReader br = new BinaryReader(FileTools.TryOpenRead(_filename));
			header = br.ReadBytes(12);
			headermd5 = br.ReadBytes(16);
			headercrc = br.ReadBytes(4);
			headersz = br.ReadUInt64();
			br.Dispose();

			// If the header is not correct, return a blank rom
			bool correct = true;
			for (int i = 0; i < header.Length; i++)
			{
				// This is a temp fix to ignore the modification time and OS until romba can be fixed
				if (i == 4 || i == 5 || i == 6 || i == 7 || i == 9)
				{
					continue;
				}
				correct &= (header[i] == Constants.TorrentGZHeader[i]);
			}
			if (!correct)
			{
				return null;
			}

			// Now convert the data and get the right position
			string gzmd5 = Style.ByteArrayToString(headermd5);
			string gzcrc = Style.ByteArrayToString(headercrc);
			long extractedsize = (long)headersz;

			Rom rom = new Rom
			{
				Type = ItemType.Rom,
				Name = Path.GetFileNameWithoutExtension(_filename).ToLowerInvariant(),
				Size = extractedsize,
				CRC = gzcrc.ToLowerInvariant(),
				MD5 = gzmd5.ToLowerInvariant(),
				SHA1 = Path.GetFileNameWithoutExtension(_filename).ToLowerInvariant(), // TODO: When updating to SHA-256, this needs to update to SHA256

				MachineName = Path.GetFileNameWithoutExtension(_filename).ToLowerInvariant(),
			};

			return rom;
		}

		#endregion

		#region Writing

		/// <summary>
		/// Write an input file to a torrent GZ file
		/// </summary>
		/// <param name="inputFile">Input filename to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">DatItem representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <returns>True if the write was a success, false otherwise</returns>
		/// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
		public override bool Write(string inputFile, string outDir, Rom rom = null, bool date = false, bool romba = false)
		{
			// Check that the input file exists
			if (!File.Exists(inputFile))
			{
				Globals.Logger.Warning("File '{0}' does not exist!", inputFile);
				return false;
			}
			inputFile = Path.GetFullPath(inputFile);

			// Get the file stream for the file and write out
			return Write(FileTools.TryOpenRead(inputFile), outDir, rom, date, romba);
		}

		/// <summary>
		/// Write an input stream to a torrent GZ file
		/// </summary>
		/// <param name="inputStream">Input stream to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">DatItem representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <returns>True if the write was a success, false otherwise</returns>
		/// <remarks>This works for now, but it can be sped up by using Ionic.Zip or another zlib wrapper that allows for header values built-in. See edc's code.</remarks>
		public override bool Write(Stream inputStream, string outDir, Rom rom = null, bool date = false, bool romba = false)
		{
			bool success = false;

			// If the stream is not readable, return
			if (!inputStream.CanRead)
			{
				return success;
			}

			// Make sure the output directory exists
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}
			outDir = Path.GetFullPath(outDir);

			// Now get the Rom info for the file so we have hashes and size
			rom = (Rom)FileTools.GetStreamInfo(inputStream, inputStream.Length, keepReadOpen: true);

			// Get the output file name
			string outfile = null;

			// If we have a romba output, add the romba path
			if (romba)
			{
				outfile = Path.Combine(outDir, Style.GetRombaPath(rom.SHA1)); // TODO: When updating to SHA-256, this needs to update to SHA256

				// Check to see if the folder needs to be created
				if (!Directory.Exists(Path.GetDirectoryName(outfile)))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(outfile));
				}
			}
			// Otherwise, we're just rebuilding to the main directory
			else
			{
				outfile = Path.Combine(outDir, rom.SHA1 + ".gz"); // TODO: When updating to SHA-256, this needs to update to SHA256
			}

			// If the output file exists, don't try to write again
			if (!File.Exists(outfile))
			{
				// Compress the input stream
				FileStream outputStream = FileTools.TryCreate(outfile);

				// Open the output file for writing
				BinaryWriter sw = new BinaryWriter(outputStream);

				// Write standard header and TGZ info
				byte[] data = Constants.TorrentGZHeader
								.Concat(Style.StringToByteArray(rom.MD5)) // MD5
								.Concat(Style.StringToByteArray(rom.CRC)) // CRC
							.ToArray();
				sw.Write(data);
				sw.Write((ulong)rom.Size); // Long size (Unsigned, Mirrored)

				// Now create a deflatestream from the input file
				DeflateStream ds = new DeflateStream(outputStream, Ionic.Zlib.CompressionMode.Compress, Ionic.Zlib.CompressionLevel.BestCompression, true);

				// Copy the input stream to the output
				byte[] ibuffer = new byte[_bufferSize];
				int ilen;
				while ((ilen = inputStream.Read(ibuffer, 0, _bufferSize)) > 0)
				{
					ds.Write(ibuffer, 0, ilen);
					ds.Flush();
				}
				ds.Dispose();

				// Now write the standard footer
				sw.Write(Style.StringToByteArray(rom.CRC).Reverse().ToArray());
				sw.Write((uint)rom.Size);

				// Dispose of everything
				sw.Dispose();
				outputStream.Dispose();
				inputStream.Dispose();
			}

			return true;
		}

		/// <summary>
		/// Write a set of input files to a torrent GZ archive (assuming the same output archive name)
		/// </summary>
		/// <param name="inputFiles">Input files to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">DatItem representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public override bool Write(List<string> inputFiles, string outDir, List<Rom> roms, bool date = false, bool romba = false)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
