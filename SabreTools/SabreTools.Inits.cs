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
        /// <param name="datHeader">All DatHeader info to be used</param>
        /// /* Standard DFD info */
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated</param>
        /// <param name="removeDateFromAutomaticName">True if the date should be omitted from the DAT, false otherwise</param>
        /// <param name="archivesAsFiles">True if archives should be treated as files, false otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <param name="skipFileType">Type of files that should be skipped on scan</param>
        /// <param name="addBlankFilesForEmptyFolder">True if blank items should be created for empty folders, false otherwise</param>
        /// <param name="addFileDates">True if dates should be archived for all files, false otherwise</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        /// /* Output DAT info */
        /// <param name="tempDir">Name of the directory to create a temp folder in (blank is default temp directory)</param>
        /// <param name="outDir">Name of the directory to output the DAT to (blank is the current directory)</param>
        /// <param name="copyFiles">True if files should be copied to the temp directory before hashing, false otherwise</param>
        /// /* Filtering info */
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        private static void InitDatFromDir(
            List<string> inputs,

            /* Normal DAT header info */
            DatHeader datHeader,

            /* Standard DFD info */
            Hash omitFromScan,
            bool removeDateFromAutomaticName,
            bool archivesAsFiles,
            bool chdsAsFiles,
            SkipFileType skipFileType,
            bool addBlankFilesForEmptyFolder,
            bool addFileDates,

            /* Output DAT info */
            string tempDir,
            string outDir,
            bool copyFiles,
            
            /* Filtering info */
            Filter filter)
        {
            // Create a new DATFromDir object and process the inputs
            DatFile basedat = new DatFile(datHeader)
            {
                Date = DateTime.Now.ToString("yyyy-MM-dd"),
            };

            // For each input directory, create a DAT
            foreach (string path in inputs)
            {
                if (Directory.Exists(path) || File.Exists(path))
                {
                    // Clone the base Dat for information
                    DatFile datdata = new DatFile(basedat);

                    string basePath = Path.GetFullPath(path);
                    bool success = datdata.PopulateFromDir(basePath, omitFromScan, removeDateFromAutomaticName, archivesAsFiles,
                        skipFileType, addBlankFilesForEmptyFolder, addFileDates, tempDir, copyFiles, datHeader.Header, chdsAsFiles, filter);

                    if (success)
                    {
                        datdata.Write(outDir);
                    }
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
        private static void InitExtractRemoveHeader(
            List<string> inputs,
            string outDir,
            bool nostore)
        {
            // Get only files from the inputs
            List<string> files = Utilities.GetOnlyFilesFromInputs(inputs);

            foreach (string file in files)
            {
                Utilities.DetectSkipperAndTransform(file, outDir, nostore);
            }
        }

        /// <summary>
        /// Wrap replacing headers
        /// </summary>
        /// <param name="inputs">Input file or folder names</param>
        /// <param name="outDir">Output directory to write new files to, blank defaults to rom folder</param>
        private static void InitReplaceHeader(
            List<string> inputs,
            string outDir)
        {
            // Get only files from the inputs
            List<string> files = Utilities.GetOnlyFilesFromInputs(inputs);

            foreach (string file in files)
            {
                Utilities.RestoreHeader(file, outDir);
            }
        }

        /// <summary>
        /// Wrap sorting files using an input DAT
        /// </summary>
        /// <param name="datfiles">Names of the DATs to compare against</param>
        /// <param name="inputs">List of input files/folders to check</param>
        /// <param name="outDir">Output directory to use to build to</param>
        /// <param name="depot">True if the input direcories are treated as romba depots, false otherwise</param>
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
        /// <param name="individual">True if DATs should be sorted individually, false if they should be done in bulk</param>
        private static void InitSort(
            List<string> datfiles,
            List<string> inputs,
            string outDir,
            bool depot,
            bool quickScan,
            bool date,
            bool delete,
            bool inverse,
            OutputFormat outputFormat,
            bool romba,
            int sevenzip,
            int gz,
            int rar,
            int zip,
            bool updateDat,
            string headerToCheckAgainst,
            SplitType splitType,
            bool chdsAsFiles,
            bool individual)
        {
            // Get the archive scanning level
            ArchiveScanLevel asl = Utilities.GetArchiveScanLevelFromNumbers(sevenzip, gz, rar, zip);

            // If we have TorrentGzip output and the romba flag, update
            if (romba && outputFormat == OutputFormat.TorrentGzip)
                outputFormat = OutputFormat.TorrentGzipRomba;

            // Get a list of files from the input datfiles
            datfiles = Utilities.GetOnlyFilesFromInputs(datfiles);

            // If we are in individual mode, process each DAT on their own, appending the DAT name to the output dir
            if (individual)
            {				
                foreach (string datfile in datfiles)
                {
                    DatFile datdata = new DatFile();
                    datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);

                    // If we have the depot flag, respect it
                    if (depot)
                        datdata.RebuildDepot(inputs, Path.Combine(outDir, datdata.FileName), date, delete, inverse, outputFormat, updateDat, headerToCheckAgainst);
                    else
                        datdata.RebuildGeneric(inputs, Path.Combine(outDir, datdata.FileName), quickScan, date, delete, inverse, outputFormat, asl, updateDat, headerToCheckAgainst, chdsAsFiles);
                }
            }
            // Otherwise, process all DATs into the same output
            else
            {
                InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

                // Add all of the input DATs into one huge internal DAT
                DatFile datdata = new DatFile();
                foreach (string datfile in datfiles)
                {
                    datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);
                }

                watch.Stop();

                // If we have the depot flag, respect it
                if (depot)
                    datdata.RebuildDepot(inputs, outDir, date, delete, inverse, outputFormat, updateDat, headerToCheckAgainst);
                else
                    datdata.RebuildGeneric(inputs, outDir, quickScan, date, delete, inverse, outputFormat, asl, updateDat, headerToCheckAgainst, chdsAsFiles);
            }
        }

        /// <summary>
        /// Wrap splitting a DAT by any known type
        /// </summary>
        /// <param name="inputs">List of inputs to be used</param>
        /// <param name="outDir">Output directory for the split files</param>
        /// <param name="inplace">True if files should be written to the source folders, false otherwise</param>
        /// <param name="datFormat">DatFormat to be used for outputting the DAT</param>
        /// <param name="splittingMode">Type of split to perform, if any</param>
        /// <param name="exta">First extension to split on (Extension Split only)</param>
        /// <param name="extb">Second extension to split on (Extension Split only)</param>
        /// <param name="shortname">True if short filenames should be used, false otherwise (Level Split only)</param>
        /// <param name="basedat">True if original filenames should be used as the base for output filename, false otherwise (Level Split only)</param>
        /// <param name="radix">Long value representing the split point (Size Split only)</param>
        private static void InitSplit(
            List<string> inputs,
            string outDir,
            bool inplace,
            DatFormat datFormat,
            SplittingMode splittingMode,
            List<string> exta,
            List<string> extb,
            bool shortname,
            bool basedat,
            long radix)
        {
            DatFile datfile = new DatFile();
            datfile.DatFormat = datFormat;
            datfile.DetermineSplitType(inputs, outDir, inplace, splittingMode, exta, extb, shortname, basedat, radix);
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
        private static void InitStats(
            List<string> inputs,
            string filename,
            string outDir,
            bool single,
            bool baddumpCol,
            bool nodumpCol,
            StatReportFormat statDatFormat)
        {
            DatFile.OutputStats(inputs, filename, outDir, single, baddumpCol, nodumpCol, statDatFormat);
        }

        /// <summary>
        /// Wrap converting and updating DAT file from any format to any format
        /// </summary>
        /// <param name="inputPaths">List of input filenames</param>
        /// <param name="basePaths">List of base filenames</param>
        /// /* Normal DAT header info */
        /// <param name="datHeader">All DatHeader info to be used</param>
        /// /* Merging and Diffing info */
        /// <param name="updateMode">Non-zero flag for diffing mode, zero otherwise</param>
        /// <param name="inplace">True if the cascade-diffed files should overwrite their inputs, false otherwise</param>
        /// <param name="skip">True if the first cascaded diff file should be skipped on output, false otherwise</param>
        /// <param name="bare">True if the date should not be appended to the default name, false otherwise</param>
        /// /* Filtering info */
        /// <param name="filter">Pre-populated filter object for DAT filtering</param>
        /// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
        /// /* Output DAT info */
        /// <param name="outDir">Optional param for output directory</param>
        /// <param name="clean">True to clean the game names to WoD standard, false otherwise (default)</param>
        /// <param name="remUnicode">True if we should remove non-ASCII characters from output, false otherwise (default)</param>
        /// <param name="descAsName">True if descriptions should be used as names, false otherwise (default)</param>
        /// <param name="updateFields">List of Fields representing what should be updated [only for base replacement]</param>
        /// <param name="onlySame">True if descriptions should only be replaced if the game name is the same, false otherwise [only for base replacement]</param>
        private static void InitUpdate(
            List<string> inputPaths,
            List<string> basePaths,

            /* Normal DAT header info */
            DatHeader datHeader,

            /* Merging and Diffing info */
            UpdateMode updateMode,
            bool inplace,
            bool skip,
            bool bare,

            /* Filtering info */
            Filter filter,
            SplitType splitType,

            /* Output DAT info */
            string outDir,
            bool clean,
            bool remUnicode,
            bool descAsName,
            List<Field> updateFields,
            bool onlySame)
        {
            // Normalize the extensions
            datHeader.AddExtension = (string.IsNullOrWhiteSpace(datHeader.AddExtension) || datHeader.AddExtension.StartsWith(".")
                ? datHeader.AddExtension
                : "." + datHeader.AddExtension);
            datHeader.ReplaceExtension = (string.IsNullOrWhiteSpace(datHeader.ReplaceExtension) || datHeader.ReplaceExtension.StartsWith(".")
                ? datHeader.ReplaceExtension
                : "." + datHeader.ReplaceExtension);

            // If we're in a special update mode and the names aren't set, set defaults
            if (updateMode != 0)
            {
                // Get the values that will be used
                if (string.IsNullOrWhiteSpace(datHeader.Date))
                    datHeader.Date = DateTime.Now.ToString("yyyy-MM-dd");

                if (string.IsNullOrWhiteSpace(datHeader.Name))
                {
                    datHeader.Name = (updateMode != 0 ? "DiffDAT" : "MergeDAT")
                        + (datHeader.Type == "SuperDAT" ? "-SuperDAT" : string.Empty)
                        + (datHeader.DedupeRoms != DedupeType.None ? "-deduped" : string.Empty);
                }

                if (string.IsNullOrWhiteSpace(datHeader.Description))
                {
                    datHeader.Description = (updateMode != 0 ? "DiffDAT" : "MergeDAT")
                        + (datHeader.Type == "SuperDAT" ? "-SuperDAT" : string.Empty)
                        + (datHeader.DedupeRoms != DedupeType.None ? " - deduped" : string.Empty);

                    if (!bare)
                        datHeader.Description += " (" + datHeader.Date + ")";
                }

                if (string.IsNullOrWhiteSpace(datHeader.Category) && updateMode != 0)
                    datHeader.Category = "DiffDAT";

                if (string.IsNullOrWhiteSpace(datHeader.Author))
                    datHeader.Author = "SabreTools";
            }

            // If no update fields are set, default to Names
            if (updateFields == null || updateFields.Count == 0)
                updateFields = new List<Field>() { Field.Name };

            // Populate the DatData object
            DatFile userInputDat = new DatFile(datHeader);
            
            userInputDat.DetermineUpdateType(inputPaths, basePaths, outDir, updateMode, inplace, skip, clean,
                remUnicode, descAsName, filter, splitType, updateFields, onlySame);
        }

        /// <summary>
        /// Wrap verifying files using an input DAT
        /// </summary>
        /// <param name="datfiles">Names of the DATs to compare against</param>
        /// <param name="inputs">Input directories to compare against</param>
        /// <param name="depot">True if the input direcories are treated as romba depots, false otherwise</param>
        /// <param name="hashOnly">True if only hashes should be checked, false for full file information</param>
        /// <param name="quickScan">True to enable external scanning of archives, false otherwise</param>
        /// <param name="headerToCheckAgainst">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <param name="splitType">Type of the split that should be performed (split, merged, fully merged)</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <param name="individual">True if DATs should be verified individually, false if they should be done in bulk</param>
        /// <param name="filter">Filter object to be passed to the DatItem level</param>
        private static void InitVerify(
            List<string> datfiles,
            List<string> inputs,
            bool depot,
            bool hashOnly,
            bool quickScan,
            string headerToCheckAgainst,
            SplitType splitType,
            bool chdsAsFiles,
            bool individual,
            Filter filter)
        {
            // Get a list of files from the input datfiles
            datfiles = Utilities.GetOnlyFilesFromInputs(datfiles);

            // If we are in individual mode, process each DAT on their own
            if (individual)
            {
                foreach (string datfile in datfiles)
                {
                    DatFile datdata = new DatFile();
                    datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);

                    // If we have the depot flag, respect it
                    if (depot)
                        datdata.VerifyDepot(inputs, headerToCheckAgainst);
                    else
                        datdata.VerifyGeneric(inputs, hashOnly, quickScan, headerToCheckAgainst, chdsAsFiles, filter);
                }
            }
            // Otherwise, process all DATs into the same output
            else
            {
                InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

                // Add all of the input DATs into one huge internal DAT
                DatFile datdata = new DatFile();
                foreach (string datfile in datfiles)
                {
                    datdata.Parse(datfile, 99, 99, splitType, keep: true, useTags: true);
                }

                watch.Stop();

                // If we have the depot flag, respect it
                if (depot)
                    datdata.VerifyDepot(inputs, headerToCheckAgainst);
                else
                    datdata.VerifyGeneric(inputs, hashOnly, quickScan, headerToCheckAgainst, chdsAsFiles, filter);
            }
        }

        #endregion
    }
}
