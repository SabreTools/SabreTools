using System;
using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using SearchOption = System.IO.SearchOption;
#endif

namespace SabreTools
{
	public partial class SabreTools
	{
		#region Init Methods

		/// <summary>
		/// Wrap creating a DAT file from files or a directory in parallel
		/// </summary>
		/// <param name="inputs">List of input filenames</param>
		/// /* Normal DAT header info */
		/// <param name="filename">New filename</param>
		/// <param name="name">New name</param>
		/// <param name="description">New description</param>
		/// <param name="category">New category</param>
		/// <param name="version">New version</param>
		/// <param name="author">New author</param>
		/// <param name="email">New email</param>
		/// <param name="homepage">New homepage</param>
		/// <param name="url">New URL</param>
		/// <param name="comment">New comment</param>
		/// <param name="forcepack">String representing the forcepacking flag</param>
		/// <param name="excludeOf">True if cloneof, romof, and sampleof fields should be omitted from output, false otherwise</param>
		/// <param name="sceneDateStrip">True if scene-named sets have the date stripped from the beginning, false otherwise</param>
		/// <param name="datFormat">DatFormat to be used for outputting the DAT</param>
		/// /* Standard DFD info */
		/// <param name="romba">True to enable reading a directory like a Romba depot, false otherwise</param>
		/// <param name="superdat">True to enable SuperDAT-style reading, false otherwise</param>
		/// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
		/// <param name="removeDateFromAutomaticName">True if the date should be omitted from the DAT, false otherwise</param>
		/// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
		/// <param name="skipFileType">Type of files that should be skipped on scan</param>
		/// <param name="addBlankFilesForEmptyFolder">True if blank items should be created for empty folders, false otherwise</param>
		/// <param name="addFileDates">True if dates should be archived for all files, false otherwise</param>
		/// /* Output DAT info */
		/// <param name="tempDir">Name of the directory to create a temp folder in (blank is default temp directory)</param>
		/// <param name="outDir">Name of the directory to output the DAT to (blank is the current directory)</param>
		/// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		private static void InitDatFromDir(List<string> inputs,
			/* Normal DAT header info */
			string filename,
			string name,
			string description,
			string category,
			string version,
			string author,
			string email,
			string homepage,
			string url,
			string comment,
			string forcepack,
			bool excludeOf,
			bool sceneDateStrip,
			DatFormat datFormat,

			/* Standard DFD info */
			bool romba,
			bool superdat,
			Hash omitFromScan,
			bool removeDateFromAutomaticName,
			bool archivesAsFiles,
			SkipFileType skipFileType,
			bool addBlankFilesForEmptyFolder,
			bool addFileDates,

			/* Output DAT info */
			string tempDir,
			string outDir,
			bool copyFiles,
			string headerToCheckAgainst,
			bool chdsAsFiles)
		{
			ForcePacking fp = ForcePacking.None;
			switch (forcepack?.ToLowerInvariant())
			{
				case "none":
				default:
					fp = ForcePacking.None;
					break;
				case "zip":
					fp = ForcePacking.Zip;
					break;
				case "unzip":
					fp = ForcePacking.Unzip;
					break;
			}

			// Create a new DATFromDir object and process the inputs
			DatFile basedat = new DatFile
			{
				FileName = filename,
				Name = name,
				Description = description,
				Category = category,
				Version = version,
				Date = DateTime.Now.ToString("yyyy-MM-dd"),
				Author = author,
				Email = email,
				Homepage = homepage,
				Url = url,
				Comment = comment,
				ForcePacking = fp,
				DatFormat = (datFormat == 0 ? DatFormat.Logiqx : datFormat),
				Romba = romba,
				ExcludeOf = excludeOf,
				SceneDateStrip = sceneDateStrip,
				Type = (superdat ? "SuperDAT" : ""),
			};

			// Clean the temp directory
			tempDir = (String.IsNullOrEmpty(tempDir) ? Path.GetTempPath() : tempDir);

			// For each input directory, create a DAT
			foreach (string path in inputs)
			{
				if (Directory.Exists(path) || File.Exists(path))
				{
					// Clone the base Dat for information
					DatFile datdata = new DatFile(basedat);

					string basePath = Path.GetFullPath(path);
					bool success = datdata.PopulateFromDir(basePath, omitFromScan, removeDateFromAutomaticName, archivesAsFiles,
						skipFileType, addBlankFilesForEmptyFolder, addFileDates, tempDir, copyFiles, headerToCheckAgainst, chdsAsFiles);

					// If it was a success, write the DAT out
					if (success)
					{
						datdata.WriteToFile(outDir);
					}

					// Otherwise, show the help
					else
					{
						Console.WriteLine();
						_help.OutputIndividualFeature("DATFromDir");
					}
				}
			}
		}

		/// <summary>
		/// Wrap extracting headers
		/// </summary>
		/// <param name="inputs">Input file or folder names</param>
		/// <param name="outDir">Output directory to write new files to, blank defaults to rom folder</param>
		/// <param name="nostore">True if headers should not be stored in the database, false otherwise</param>
		private static void InitExtractRemoveHeader(List<string> inputs, string outDir, bool nostore)
		{
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					FileTools.DetectSkipperAndTransform(input, outDir, nostore);
				}
				else if (Directory.Exists(input))
				{
					foreach (string sub in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						FileTools.DetectSkipperAndTransform(sub, outDir, nostore);
					}
				}
			}
		}

		/// <summary>
		/// Wrap splitting a DAT by 2 extensions
		/// </summary>
		/// <param name="inputs">Input files or folders to be split</param>
		/// <param name="exta">First extension to split on</param>
		/// <param name="extb">Second extension to split on</param>
		/// <param name="outDir">Output directory for the split files</param>
		private static void InitExtSplit(List<string> inputs, List<string> exta, List<string> extb, string outDir)
		{
			// Loop over the input files
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					DatFile datFile = new DatFile();
					datFile.Parse(Path.GetFullPath(input), 0, 0);
					datFile.SplitByExtension(outDir, Path.GetDirectoryName(input), exta, extb);
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						DatFile datFile = new DatFile();
						datFile.Parse(Path.GetFullPath(file), 0, 0);
						datFile.SplitByExtension(outDir, (input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar),
							exta, extb);
					}
				}
				else
				{
					Globals.Logger.Error("'{0}' is not a valid file or folder!", input);
					Console.WriteLine();
					_help.OutputIndividualFeature("Extension Split");
					return;
				}
			}
		}

		/// <summary>
		/// Wrap splitting a DAT by best available hashes
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outDir">Output directory for the split files</param>
		private static void InitHashSplit(List<string> inputs, string outDir)
		{
			// Loop over the input files
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					DatFile datFile = new DatFile();
					datFile.Parse(Path.GetFullPath(input), 0, 0);
					datFile.SplitByHash(outDir, Path.GetDirectoryName(input));
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						DatFile datFile = new DatFile();
						datFile.Parse(Path.GetFullPath(file), 0, 0);
						datFile.SplitByHash(outDir, (input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar));
					}
				}
				else
				{
					Globals.Logger.Error("'{0}' is not a valid file or folder!", input);
					Console.WriteLine();
					_help.OutputIndividualFeature("Hash Split");
					return;
				}
			}
		}

		/// <summary>
		/// Wrap replacing headers
		/// </summary>
		/// <param name="inputs">Input file or folder names</param>
		/// <param name="outDir">Output directory to write new files to, blank defaults to rom folder</param>
		private static void InitReplaceHeader(List<string> inputs, string outDir)
		{
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					FileTools.RestoreHeader(input, outDir);
				}
				else if (Directory.Exists(input))
				{
					foreach (string sub in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						FileTools.RestoreHeader(sub, outDir);
					}
				}
			}
		}

		/// <summary>
		/// Wrap splitting a SuperDAT by lowest available level
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outDir">Output directory for the split files</param>
		/// <param name="shortname">True if short filenames should be used, false otherwise</param>
		/// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise</param>
		private static void InitLevelSplit(List<string> inputs, string outDir, bool shortname, bool basedat)
		{
			// Loop over the input files
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					DatFile datFile = new DatFile();
					datFile.Parse(Path.GetFullPath(input), 0, 0, keep: true);
					datFile.SplitByLevel(outDir, Path.GetDirectoryName(input), shortname, basedat);
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						DatFile datFile = new DatFile();
						datFile.Parse(Path.GetFullPath(file), 0, 0, keep: true);
						datFile.SplitByLevel(outDir, (input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar),
							shortname, basedat);
					}
				}
				else
				{
					Globals.Logger.Error("'{0}' is not a valid file or folder!", input);
					Console.WriteLine();
					_help.OutputIndividualFeature("Level Split");
					return;
				}
			}
		}

		/// <summary>
		/// Wrap sorting files using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="sevenzip">Integer representing the archive handling level for 7z</param>
		/// <param name="gz">Integer representing the archive handling level for GZip</param>
		/// <param name="rar">Integer representing the archive handling level for RAR</param>
		/// <param name="zip">Integer representing the archive handling level for Zip</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		private static void InitSort(List<string> datfiles, List<string> inputs, string outDir, bool quickScan, bool date, bool delete,
			bool inverse, OutputFormat outputFormat, bool romba, int sevenzip, int gz, int rar, int zip, bool updateDat, string headerToCheckAgainst,
			SplitType splitType, bool chdsAsFiles)
		{
			// Get the archive scanning level
			ArchiveScanLevel asl = FileTools.GetArchiveScanLevelFromNumbers(sevenzip, gz, rar, zip);

			// Get a list of files from the input datfiles
			datfiles = FileTools.GetOnlyFilesFromInputs(datfiles);

			InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);
			}

			watch.Stop();

			datdata.RebuildGeneric(inputs, outDir, quickScan, date, delete, inverse, outputFormat, romba, asl,
				updateDat, headerToCheckAgainst, chdsAsFiles);
		}

		/// <summary>
		/// Wrap sorting files from a depot using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">List of input files/folders to check</param>
		/// <param name="outDir">Output directory to use to build to</param>
		/// <param name="date">True if the date from the DAT should be used if available, false otherwise</param>
		/// <param name="delete">True if input files should be deleted, false otherwise</param>
		/// <param name="inverse">True if the DAT should be used as a filter instead of a template, false otherwise</param>
		/// <param name="outputFormat">Output format that files should be written to</param>
		/// <param name="romba">True if files should be output in Romba depot folders, false otherwise</param>
		/// <param name="updateDat">True if the updated DAT should be output, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		private static void InitSortDepot(List<string> datfiles, List<string> inputs, string outDir, bool date, bool delete,
			bool inverse, OutputFormat outputFormat, bool romba, bool updateDat, string headerToCheckAgainst, SplitType splitType)
		{
			InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

			// Get a list of files from the input datfiles
			datfiles = FileTools.GetOnlyFilesFromInputs(datfiles);

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);
			}

			watch.Stop();

			datdata.RebuildDepot(inputs, outDir, date, delete, inverse, outputFormat, romba,
				updateDat, headerToCheckAgainst);
		}

		/// <summary>
		/// Wrap getting statistics on a DAT or folder of DATs
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="filename">Name of the file to output to, blank for default</param>
		/// <param name="outDir">Output directory for the report files</param>
		/// <param name="single">True to show individual DAT statistics, false otherwise</param>
		/// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
		/// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
		/// <param name="statDatFormat">Set the statistics output format to use</param>
		private static void InitStats(List<string> inputs, string filename, string outDir, bool single, bool baddumpCol, bool nodumpCol,
			StatDatFormat statDatFormat)
		{
			DatFile.OutputStats(inputs, filename, outDir, single, baddumpCol, nodumpCol, statDatFormat);
		}

		/// <summary>
		/// Wrap splitting a DAT by item type
		/// </summary>
		/// <param name="inputs">List of inputs to be used</param>
		/// <param name="outDir">Output directory for the split files</param>
		private static void InitTypeSplit(List<string> inputs, string outDir)
		{
			// Loop over the input files
			foreach (string input in inputs)
			{
				if (File.Exists(input))
				{
					DatFile datFile = new DatFile();
					datFile.Parse(Path.GetFullPath(input), 0, 0);
					datFile.SplitByType(outDir, Path.GetFullPath(Path.GetDirectoryName(input)));
				}
				else if (Directory.Exists(input))
				{
					foreach (string file in Directory.EnumerateFiles(input, "*", SearchOption.AllDirectories))
					{
						DatFile datFile = new DatFile();
						datFile.Parse(Path.GetFullPath(file), 0, 0);
						datFile.SplitByType(outDir, Path.GetFullPath((input.EndsWith(Path.DirectorySeparatorChar.ToString()) ? input : input + Path.DirectorySeparatorChar)));
					}
				}
				else
				{
					Globals.Logger.Error("{0} is not a valid file or folder!", input);
					Console.WriteLine();
					_help.OutputIndividualFeature("Type Split");
					return;
				}
			}
		}

		/// <summary>
		/// Wrap converting and updating DAT file from any format to any format
		/// </summary>
		/// <param name="inputPaths">List of input filenames</param>
		/// <param name="basePaths">List of base filenames</param>
		/// /* Normal DAT header info */
		/// <param name="filename">New filename</param>
		/// <param name="name">New name</param>
		/// <param name="description">New description</param>
		/// <param name="rootdir">New rootdir</param>
		/// <param name="category">New category</param>
		/// <param name="version">New version</param>
		/// <param name="date">New date</param>
		/// <param name="author">New author</param>
		/// <param name="email">New email</param>
		/// <param name="homepage">New homepage</param>
		/// <param name="url">New URL</param>
		/// <param name="comment">New comment</param>
		/// <param name="header">New header</param>
		/// <param name="superdat">True to set SuperDAT type, false otherwise</param>
		/// <param name="forcemerge">None, Split, Full</param>
		/// <param name="forcend">None, Obsolete, Required, Ignore</param>
		/// <param name="forcepack">None, Zip, Unzip</param>
		/// <param name="excludeOf">True if cloneof, romof, and sampleof fields should be omitted from output, false otherwise</param>
		/// <param name="sceneDateStrip">True if scene-named sets have the date stripped from the beginning, false otherwise</param>
		/// <param name="datFormat">Non-zero flag for output format, zero otherwise for default</param>
		/// /* Missfile-specific DAT info */
		/// <param name="usegame">True if games are to be used in output, false if roms are</param>
		/// <param name="prefix">Generic prefix to be added to each line</param>
		/// <param name="postfix">Generic postfix to be added to each line</param>
		/// <param name="quotes">Add quotes to each item</param>
		/// <param name="repext">Replace all extensions with another</param>
		/// <param name="addext">Add an extension to all items</param>
		/// <param name="remext">Remove all extensions</param>
		/// <param name="datprefix">Add the dat name as a directory prefix</param>
		/// <param name="romba">Output files in romba format</param>
		/// /* Merging and Diffing info */
		/// <param name="merge">True if input files should be merged into a single file, false otherwise</param>
		/// <param name="diffMode">Non-zero flag for diffing mode, zero otherwise</param>
		/// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
		/// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
		/// <param name="bare">True if the date should not be appended to the default name, false otherwise [OBSOLETE]</param>
		/// /* Filtering info */
		/// <param name="filter">Pre-populated filter object for DAT filtering</param>
		/// <param name="oneGameOneRegion">True if the outputs should be created in 1G1R mode, false otherwise</param>
		/// <param name="regions">List of regions in the order they should be used, blank for default</param>
		/// /* Trimming info */
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="trim">True if we are supposed to trim names to NTFS length, false otherwise</param>
		/// <param name="single">True if all games should be replaced by '!', false otherwise</param>
		/// <param name="root">String representing root directory to compare against for length calculation</param>
		/// /* Output DAT info */
		/// <param name="outDir">Optional param for output directory</param>
		/// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
		/// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
		/// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
		/// <param name="dedup">Dedupe type to use for DAT processing</param>
		/// <param name="stripHash">StripHash that represents the hash(es) that you want to remove from the output</param>
		private static void InitUpdate(
			List<string> inputPaths,
			List<string> basePaths,

			/* Normal DAT header info */
			string filename,
			string name,
			string description,
			string rootdir,
			string category,
			string version,
			string date,
			string author,
			string email,
			string homepage,
			string url,
			string comment,
			string header,
			bool superdat,
			string forcemerge,
			string forcend,
			string forcepack,
			bool excludeOf,
			bool sceneDateStrip,
			DatFormat datFormat,

			/* Missfile-specific DAT info */
			bool usegame,
			string prefix,
			string postfix,
			bool quotes,
			string repext,
			string addext,
			bool remext,
			bool datprefix,
			bool romba,

			/* Merging and Diffing info */
			bool merge,
			UpdateMode diffMode,
			bool inplace,
			bool skip,
			bool bare,

			/* Filtering info */
			Filter filter,
			bool oneGameOneRegion,
			List<string> regions,

			/* Trimming info */
			SplitType splitType,
			bool trim,
			bool single,
			string root,

			/* Output DAT info */
			string outDir,
			bool clean,
			bool remUnicode,
			bool descAsName,
			DedupeType dedup,
			Hash stripHash)
		{
			// Set the special flags
			ForceMerging fm = ForceMerging.None;
			if (!String.IsNullOrEmpty(forcemerge))
			{
				switch (forcemerge.ToLowerInvariant())
				{
					case "none":
						fm = ForceMerging.None;
						break;
					case "split":
						fm = ForceMerging.Split;
						break;
					case "full":
						fm = ForceMerging.Full;
						break;
					default:
						Globals.Logger.Warning("{0} is not a valid merge flag", forcemerge);
						break;
				}
			}

			ForceNodump fn = ForceNodump.None;
			if (!String.IsNullOrEmpty(forcend))
			{
				switch (forcend.ToLowerInvariant())
				{
					case "none":
						fn = ForceNodump.None;
						break;
					case "obsolete":
						fn = ForceNodump.Obsolete;
						break;
					case "required":
						fn = ForceNodump.Required;
						break;
					case "ignore":
						fn = ForceNodump.Ignore;
						break;
					default:
						Globals.Logger.Warning("{0} is not a valid nodump flag", forcend);
						break;
				}
			}

			ForcePacking fp = ForcePacking.None;
			if (!String.IsNullOrEmpty(forcepack))
			{
				switch (forcepack.ToLowerInvariant())
				{
					case "none":
						fp = ForcePacking.None;
						break;
					case "zip":
						fp = ForcePacking.Zip;
						break;
					case "unzip":
						fp = ForcePacking.Unzip;
						break;
					default:
						Globals.Logger.Warning("{0} is not a valid packing flag", forcepack);
						break;
				}
			}

			// Set the 1G1R regions alphabetically if not already set
			if (regions == null || regions.Count == 0)
			{
				regions = new List<string>()
				{
					"australia",
					"canada",
					"china",
					"denmark",
					"europe",
					"finland",
					"france",
					"germany",
					"greece",
					"italy",
					"japan",
					"korea",
					"netherlands",
					"norway",
					"russia",
					"spain",
					"sweden",
					"usa",
					"usa, australia",
					"usa, europe",
					"world",
				};
			}

			// Normalize the extensions
			addext = (addext == "" || addext.StartsWith(".") ? addext : "." + addext);
			repext = (repext == "" || repext.StartsWith(".") ? repext : "." + repext);

			// If we're in merge or diff mode and the names aren't set, set defaults
			if (merge || diffMode != 0)
			{
				// Get the values that will be used
				if (date == "")
				{
					date = DateTime.Now.ToString("yyyy-MM-dd");
				}
				if (name == "")
				{
					name = (diffMode != 0 ? "DiffDAT" : "MergeDAT") + (superdat ? "-SuperDAT" : "") + (dedup != DedupeType.None ? "-deduped" : "");
				}
				if (description == "")
				{
					description = (diffMode != 0 ? "DiffDAT" : "MergeDAT") + (superdat ? "-SuperDAT" : "") + (dedup != DedupeType.None ? " - deduped" : "");
					if (!bare)
					{
						description += " (" + date + ")";
					}
				}
				if (category == "" && diffMode != 0)
				{
					category = "DiffDAT";
				}
				if (author == "")
				{
					author = "SabreTools";
				}
			}

			// Populate the DatData object
			DatFile userInputDat = new DatFile
			{
				FileName = filename,
				Name = name,
				Description = description,
				RootDir = rootdir,
				Category = category,
				Version = version,
				Date = date,
				Author = author,
				Email = email,
				Homepage = homepage,
				Url = url,
				Comment = comment,
				Header = header,
				Type = (superdat ? "SuperDAT" : null),
				ForceMerging = fm,
				ForceNodump = fn,
				ForcePacking = fp,
				DedupeRoms = dedup,
				ExcludeOf = excludeOf,
				SceneDateStrip = sceneDateStrip,
				DatFormat = datFormat,
				StripHash = stripHash,
				OneGameOneRegion = oneGameOneRegion,
				Regions = regions,

				UseGame = usegame,
				Prefix = prefix,
				Postfix = postfix,
				Quotes = quotes,
				RepExt = repext,
				AddExt = addext,
				RemExt = remext,
				GameName = datprefix,
				Romba = romba,
			};
			
			userInputDat.DetermineUpdateType(inputPaths, basePaths, outDir, merge, diffMode, inplace, skip, bare, clean,
				remUnicode, descAsName, filter, splitType, trim, single, root);
		}

		/// <summary>
		/// Wrap verifying files using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">Input directories to compare against</param>
		/// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
		/// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		/// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
		private static void InitVerify(List<string> datfiles, List<string> inputs, bool hashOnly, bool quickScan,
			string headerToCheckAgainst, SplitType splitType, bool chdsAsFiles)
		{
			// Get the archive scanning level
			ArchiveScanLevel asl = FileTools.GetArchiveScanLevelFromNumbers(1, 1, 1, 1);

			// Get a list of files from the input datfiles
			datfiles = FileTools.GetOnlyFilesFromInputs(datfiles);

			InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);
			}

			watch.Stop();

			datdata.VerifyGeneric(inputs, hashOnly, quickScan, headerToCheckAgainst, chdsAsFiles);
		}

		/// <summary>
		/// Wrap verifying files from a depot using an input DAT
		/// </summary>
		/// <param name="datfiles">Names of the DATs to compare against</param>
		/// <param name="inputs">Input directories to compare against</param>
		/// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
		/// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
		private static void InitVerifyDepot(List<string> datfiles, List<string> inputs, string headerToCheckAgainst, SplitType splitType)
		{
			InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

			// Get a list of files from the input datfiles
			datfiles = FileTools.GetOnlyFilesFromInputs(datfiles);

			// Add all of the input DATs into one huge internal DAT
			DatFile datdata = new DatFile();
			foreach (string datfile in datfiles)
			{
				datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);
			}

			watch.Stop();

			datdata.VerifyDepot(inputs, headerToCheckAgainst);
		}

		#endregion
	}
}
