using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Readers;

namespace SabreTools.Library.FileTypes
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
			this.Type = FileType.RarArchive;
		}

		/// <summary>
		/// Create a new TorrentRARArchive from the given file
		/// </summary>
		/// <param name="filename">Name of the file to use as an archive</param>
		/// <param name="read">True for opening file as read, false for opening file as write</param>
		/// <param name="getHashes">True if hashes for this file should be calculated, false otherwise (default)</param>
		public RarArchive(string filename, bool getHashes = false)
			: base(filename, getHashes)
		{
			this.Type = FileType.RarArchive;
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
				SharpCompress.Archives.Rar.RarArchive ra = SharpCompress.Archives.Rar.RarArchive.Open(this.Filename);
				foreach (RarArchiveEntry entry in ra.Entries)
				{
					entry.WriteToDirectory(outDir, new SharpCompress.Common.ExtractionOptions { PreserveFileTime = true, ExtractFullPath = true, Overwrite = true });
				}
				encounteredErrors = false;
				ra.Dispose();
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
				SharpCompress.Archives.Rar.RarArchive ra = SharpCompress.Archives.Rar.RarArchive.Open(this.Filename, new ReaderOptions { LeaveStreamOpen = false, });
				foreach (RarArchiveEntry entry in ra.Entries)
				{
					if (entry != null && !entry.IsDirectory && entry.Key.Contains(entryName))
					{
						// Write the file out
						realEntry = entry.Key;
						entry.WriteTo(ms);
					}
				}
				ra.Dispose();
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
				SharpCompress.Archives.Rar.RarArchive ra = SharpCompress.Archives.Rar.RarArchive.Open(FileExtensions.TryOpenRead(this.Filename));
				foreach (RarArchiveEntry entry in ra.Entries.Where(e => e != null && !e.IsDirectory))
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
						BaseFile rarEntryRom = entryStream.GetInfo(entry.Size, omitFromScan);
						rarEntryRom.Filename = entry.Key;
						rarEntryRom.Parent = gamename;
						rarEntryRom.Date = entry.LastModifiedTime?.ToString("yyyy/MM/dd hh:mm:ss");
						found.Add(rarEntryRom);
						entryStream.Dispose();
					}
				}

				// Dispose of the archive
				ra.Dispose();
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
				SharpCompress.Archives.Rar.RarArchive ra = SharpCompress.Archives.Rar.RarArchive.Open(this.Filename, new ReaderOptions { LeaveStreamOpen = false });
				List<RarArchiveEntry> rarEntries = ra.Entries.OrderBy(e => e.Key, new NaturalSort.NaturalReversedComparer()).ToList();
				string lastRarEntry = null;
				foreach (RarArchiveEntry entry in rarEntries)
				{
					if (entry != null)
					{
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
		/// Write an input file to a torrentrar archive
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
			return Write(FileExtensions.TryOpenRead(inputFile), outDir, rom, date, romba);
		}

		/// <summary>
		/// Write an input stream to a torrentrar archive
		/// </summary>
		/// <param name="inputStream">Input stream to be moved</param>
		/// <param name="outDir">Output directory to build to</param>
		/// <param name="rom">DatItem representing the new information</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise (default)</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <returns>True if the archive was written properly, false otherwise</returns>
		public override bool Write(Stream inputStream, string outDir, Rom rom, bool date = false, bool romba = false)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Write a set of input files to a torrentrar archive (assuming the same output archive name)
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
