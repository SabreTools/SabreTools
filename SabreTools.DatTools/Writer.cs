using System;
using System.Collections.Generic;
using System.IO;
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
using System.Threading.Tasks;
#endif
using SabreTools.Core.Tools;
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.IO.Extensions;
using SabreTools.IO.Logging;
using SabreTools.Reports;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Helper methods for writing from DatFiles
    /// </summary>
    public class Writer
    {
        #region Private Constants

        /// <summary>
        /// Map of all formats to extensions, including "backup" extensions
        /// </summary>
        private static readonly Dictionary<DatFormat, string[]> ExtensionMappings = new()
        {
            // .csv
            { DatFormat.CSV, new string[] { ".csv" } },

            // .dat
            { DatFormat.ClrMamePro, new string[] { ".dat" } },
            { DatFormat.RomCenter, new string[] { ".dat", ".rc.dat" } },
            { DatFormat.DOSCenter, new string[] { ".dat", ".dc.dat" } },

            // .json
            { DatFormat.SabreJSON, new string[] { ".json" } },

            // .md2
            { DatFormat.RedumpMD2, new string[] { ".md2" } },

            // .md4
            { DatFormat.RedumpMD4, new string[] { ".md4" } },

            // .md5
            { DatFormat.RedumpMD5, new string[] { ".md5" } },

            // .ripemd128
            { DatFormat.RedumpRIPEMD128, new string[] { ".ripemd128" } },

            // .ripemd160
            { DatFormat.RedumpRIPEMD160, new string[] { ".ripemd160" } },

            // .sfv
            { DatFormat.RedumpSFV, new string[] { ".sfv" } },

            // .sha1
            { DatFormat.RedumpSHA1, new string[] { ".sha1" } },

            // .sha256
            { DatFormat.RedumpSHA256, new string[] { ".sha256" } },

            // .sha384
            { DatFormat.RedumpSHA384, new string[] { ".sha384" } },

            // .sha512
            { DatFormat.RedumpSHA512, new string[] { ".sha512" } },

            // .spamsum
            { DatFormat.RedumpSpamSum, new string[] { ".spamsum" } },

            // .ssv
            { DatFormat.SSV, new string[] { ".ssv" } },

            // .tsv
            { DatFormat.TSV, new string[] { ".tsv" } },

            // .txt
            { DatFormat.AttractMode, new string[] { ".txt" } },
            { DatFormat.Listrom, new string[] { ".txt", ".lr.txt" } },
            { DatFormat.MissFile, new string[] { ".txt", ".miss.txt" } },
            { DatFormat.EverdriveSMDB, new string[] { ".txt", ".smdb.txt" } },

            // .xml
            { DatFormat.Logiqx, new string[] { ".xml" } },
            { DatFormat.LogiqxDeprecated, new string[] { ".xml", ".xml" } }, // Intentional duplicate
            { DatFormat.SabreXML, new string[] { ".xml", ".sd.xml" } },
            { DatFormat.SoftwareList, new string[] { ".xml", ".sl.xml" } },
            { DatFormat.Listxml, new string[] { ".xml", ".mame.xml" } },
            { DatFormat.OfflineList, new string[] { ".xml", ".ol.xml" } },
            { DatFormat.OpenMSX, new string[] { ".xml", ".msx.xml" } },
            { DatFormat.ArchiveDotOrg, new string[] { ".xml", ".ado.xml" } },
        };

        #endregion

        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger _staticLogger = new();

        #endregion

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        /// <param name="outDir">Set the output directory (current directory on null)</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public static bool Write(DatFile datFile, string? outDir)
            => Write(datFile, outDir, overwrite: true, throwOnError: false);

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        /// <param name="outDir">Set the output directory (current directory on null)</param>
        /// <param name="overwrite">True if files should be overwritten, false if they should be renamed instead</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public static bool Write(DatFile datFile, string? outDir, bool overwrite)
            => Write(datFile, outDir, overwrite, throwOnError: false);

        /// <summary>
        /// Create and open an output file for writing direct from a dictionary
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        /// <param name="outDir">Set the output directory (current directory on null)</param>
        /// <param name="overwrite">True if files should be overwritten, false if they should be renamed instead</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the DAT was written correctly, false otherwise</returns>
        public static bool Write(DatFile datFile, string? outDir, bool overwrite, bool throwOnError)
        {
            // If we have nothing writable, abort
            if (!HasWritable(datFile))
            {
                _staticLogger.User("There were no items to write out!");
                return false;
            }

            // Ensure the output directory is set and created
            outDir = outDir.Ensure(create: true);

            InternalStopwatch watch = new($"Writing out internal dat to '{outDir}'");

            // If the DAT has no output format, default to XML
            if (datFile.Header.GetFieldValue<DatFormat>(DatHeader.DatFormatKey) == 0)
            {
                _staticLogger.Verbose("No DAT format defined, defaulting to XML");
                datFile.Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.Logiqx);
            }

            // Make sure that the three essential fields are filled in
            EnsureHeaderFields(datFile);

            // Bucket roms by game name, if not already
            datFile.BucketBy(ItemKey.Machine);

            // Output the number of items we're going to be writing
            _staticLogger.User($"A total of {datFile.DatStatistics.TotalCount - datFile.DatStatistics.RemovedCount} items will be written out to '{datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)}'");

            // Get the outfile names
            Dictionary<DatFormat, string> outfiles = CreateOutFileNames(datFile.Header, outDir!, overwrite);

            try
            {
                // Write out all required formats
#if NET452_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                Parallel.ForEach(outfiles.Keys, Core.Globals.ParallelOptions, datFormat =>
#elif NET40_OR_GREATER
                Parallel.ForEach(outfiles.Keys, datFormat =>
#else
                foreach (var datFormat in outfiles.Keys)
#endif
                {
                    string outfile = outfiles[datFormat];
                    try
                    {
                        DatFile writingDatFile = Parser.CreateDatFile(datFormat, datFile);
                        writingDatFile.WriteToFile(outfile, ignoreblanks: true, throwOnError);
                    }
                    catch (Exception ex) when (!throwOnError)
                    {
                        _staticLogger.Error(ex, $"Datfile '{outfile}' could not be written out");
                    }
#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
                });
#else
                }
#endif
            }
            catch (Exception ex) when (!throwOnError)
            {
                _staticLogger.Error(ex);
                return false;
            }
            finally
            {
                watch.Stop();
            }

            return true;
        }

        /// <summary>
        /// Write the stats out to console for the current DatFile
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        public static void WriteStatsToConsole(DatFile datFile)
        {
            long diskCount = datFile.DatStatistics.GetItemCount(ItemType.Disk);
            long mediaCount = datFile.DatStatistics.GetItemCount(ItemType.Media);
            long romCount = datFile.DatStatistics.GetItemCount(ItemType.Rom);

            if (diskCount + mediaCount + romCount == 0)
                datFile.RecalculateStats();

            datFile.BucketBy(ItemKey.Machine, norename: true);

            datFile.DatStatistics.DisplayName = datFile.Header.GetStringFieldValue(DatHeader.FileNameKey);
            datFile.DatStatistics.MachineCount = datFile.Items.SortedKeys.Length;

            List<DatStatistics> statsList =
            [
                datFile.DatStatistics,
            ];
            var consoleOutput = Parser.CreateReport(StatReportFormat.None, statsList);
            consoleOutput!.WriteToStream(Console.OpenStandardOutput(), true, true);
        }

        /// <summary>
        /// Generate a proper outfile name based on a DAT and output directory
        /// </summary>
        /// <param name="datHeader">DatHeader value to pull information from</param>
        /// <param name="outDir">Output directory</param>
        /// <param name="overwrite">True if we ignore existing files (default), false otherwise</param>
        /// <returns>Dictionary of output formats mapped to file names</returns>
        internal static Dictionary<DatFormat, string> CreateOutFileNames(DatHeader datHeader, string outDir, bool overwrite = true)
        {
            // Create the output dictionary
            Dictionary<DatFormat, string> outfileNames = [];

            // Get the filename to use
            string? filename = string.IsNullOrEmpty(datHeader.GetStringFieldValue(DatHeader.FileNameKey))
                ? datHeader.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)
                : datHeader.GetStringFieldValue(DatHeader.FileNameKey);

            // Strip off the extension if it's a holdover from the DAT
            if (Utilities.HasValidDatExtension(filename))
                filename = Path.GetFileNameWithoutExtension(filename);

            // Double check the outDir for the end delim
            if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                outDir += Path.DirectorySeparatorChar;

            // Get the current format types
            DatFormat datFormat = datHeader.GetFieldValue<DatFormat>(DatHeader.DatFormatKey);
            List<DatFormat> usedFormats = SplitFormats(datFormat);

            // Get the extensions from the output type
            List<string> usedExtensions = [];
            foreach (var map in ExtensionMappings)
            {
                // Split the pair
                DatFormat format = map.Key;
                string[] extensions = map.Value;

                // Ignore unused formats
                if (!usedFormats.Contains(format))
                    continue;

                // Get the correct extension, assuming a backup exists
                string extension = extensions[0];
                if (usedExtensions.Contains(extension))
                    extension = extensions[1];

                // Create the filename and set the extension as used
                outfileNames.Add(format, CreateOutFileNamesHelper(filename, outDir, extension, overwrite));
                usedExtensions.Add(extension);
            }

            return outfileNames;
        }

        /// <summary>
        /// Help generating the outfile name
        /// </summary>
        /// <param name="filename">Base filename to use</param>
        /// <param name="outDir">Output directory</param>
        /// <param name="extension">Extension to use for the file</param>
        /// <param name="overwrite">True if we ignore existing files, false otherwise</param>
        /// <returns>String containing the new filename</returns>
        private static string CreateOutFileNamesHelper(string? filename, string outDir, string extension, bool overwrite)
        {
            string outfile = $"{outDir}{filename}{extension}";
            outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());

            if (!overwrite)
            {
                int i = 1;
                while (File.Exists(outfile))
                {
                    outfile = $"{outDir}{filename}_{i}{extension}";
                    outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());
                    i++;
                }
            }

            return outfile;
        }

        /// <summary>
        /// Ensure that FileName, Name, and Description are filled with some value
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        private static void EnsureHeaderFields(DatFile datFile)
        {
            // Empty FileName
            if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(DatHeader.FileNameKey)))
            {
                if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)) && string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                {
                    datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "Default");
                    datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, "Default");
                    datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, "Default");
                }

                else if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)) && !string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                {
                    datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey));
                    datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey));
                }

                else if (!string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)) && string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                {
                    datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey));
                    datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey));
                }

                else if (!string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)) && !string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                {
                    datFile.Header.SetFieldValue<string?>(DatHeader.FileNameKey, datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey));
                }
            }

            // Filled FileName
            else
            {
                if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)) && string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                {
                    datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, datFile.Header.GetStringFieldValue(DatHeader.FileNameKey));
                    datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, datFile.Header.GetStringFieldValue(DatHeader.FileNameKey));
                }

                else if (string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)) && !string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                {
                    datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.NameKey, datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey));
                }

                else if (!string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey)) && string.IsNullOrEmpty(datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.DescriptionKey)))
                {
                    datFile.Header.SetFieldValue<string?>(Data.Models.Metadata.Header.DescriptionKey, datFile.Header.GetStringFieldValue(Data.Models.Metadata.Header.NameKey));
                }
            }
        }

        /// <summary>
        /// Get if the DatFile has any writable items
        /// </summary>
        /// <param name="datFile">Current DatFile object to write from</param>
        /// <returns>True if there are any writable items, false otherwise</returns>
        private static bool HasWritable(DatFile datFile)
        {
            // Force a statistics recheck, just in case
            datFile.RecalculateStats();

            // If there's nothing there, abort
            if (datFile.DatStatistics.TotalCount == 0)
                return false;
            // if (datFile.ItemsDB.DatStatistics.TotalCount == 0)
            //     return false;

            // If every item is removed, abort
            if (datFile.DatStatistics.TotalCount == datFile.DatStatistics.RemovedCount)
                return false;
            // if (datFile.ItemsDB.DatStatistics.TotalCount == datFile.ItemsDB.DatStatistics.RemovedCount)
            //     return false;

            return true;
        }

        /// <summary>
        /// Split a format flag into multiple distinct values
        /// </summary>
        /// <param name="datFormat">Combined DatFormat value to split</param>
        /// <returns>List representing the individual flag values set</returns>
        /// TODO: Consider making DatFormat a non-flag enum so this doesn't need to happen
        private static List<DatFormat> SplitFormats(DatFormat datFormat)
        {
            List<DatFormat> usedFormats = [];

#if NET20 || NET35
            if ((datFormat & DatFormat.ArchiveDotOrg) != 0)
                usedFormats.Add(DatFormat.ArchiveDotOrg);
            if ((datFormat & DatFormat.AttractMode) != 0)
                usedFormats.Add(DatFormat.AttractMode);
            if ((datFormat & DatFormat.ClrMamePro) != 0)
                usedFormats.Add(DatFormat.ClrMamePro);
            if ((datFormat & DatFormat.CSV) != 0)
                usedFormats.Add(DatFormat.CSV);
            if ((datFormat & DatFormat.DOSCenter) != 0)
                usedFormats.Add(DatFormat.DOSCenter);
            if ((datFormat & DatFormat.EverdriveSMDB) != 0)
                usedFormats.Add(DatFormat.EverdriveSMDB);
            if ((datFormat & DatFormat.Listrom) != 0)
                usedFormats.Add(DatFormat.Listrom);
            if ((datFormat & DatFormat.Listxml) != 0)
                usedFormats.Add(DatFormat.Listxml);
            if ((datFormat & DatFormat.Logiqx) != 0)
                usedFormats.Add(DatFormat.Logiqx);
            if ((datFormat & DatFormat.LogiqxDeprecated) != 0)
                usedFormats.Add(DatFormat.LogiqxDeprecated);
            if ((datFormat & DatFormat.MissFile) != 0)
                usedFormats.Add(DatFormat.MissFile);
            if ((datFormat & DatFormat.OfflineList) != 0)
                usedFormats.Add(DatFormat.OfflineList);
            if ((datFormat & DatFormat.OpenMSX) != 0)
                usedFormats.Add(DatFormat.OpenMSX);
            if ((datFormat & DatFormat.RedumpMD2) != 0)
                usedFormats.Add(DatFormat.RedumpMD2);
            if ((datFormat & DatFormat.RedumpMD4) != 0)
                usedFormats.Add(DatFormat.RedumpMD4);
            if ((datFormat & DatFormat.RedumpMD5) != 0)
                usedFormats.Add(DatFormat.RedumpMD5);
            if ((datFormat & DatFormat.RedumpRIPEMD128) != 0)
                usedFormats.Add(DatFormat.RedumpRIPEMD128);
            if ((datFormat & DatFormat.RedumpRIPEMD160) != 0)
                usedFormats.Add(DatFormat.RedumpRIPEMD160);
            if ((datFormat & DatFormat.RedumpSFV) != 0)
                usedFormats.Add(DatFormat.RedumpSFV);
            if ((datFormat & DatFormat.RedumpSHA1) != 0)
                usedFormats.Add(DatFormat.RedumpSHA1);
            if ((datFormat & DatFormat.RedumpSHA256) != 0)
                usedFormats.Add(DatFormat.RedumpSHA256);
            if ((datFormat & DatFormat.RedumpSHA384) != 0)
                usedFormats.Add(DatFormat.RedumpSHA384);
            if ((datFormat & DatFormat.RedumpSHA512) != 0)
                usedFormats.Add(DatFormat.RedumpSHA512);
            if ((datFormat & DatFormat.RedumpSpamSum) != 0)
                usedFormats.Add(DatFormat.RedumpSpamSum);
            if ((datFormat & DatFormat.RomCenter) != 0)
                usedFormats.Add(DatFormat.RomCenter);
            if ((datFormat & DatFormat.SabreJSON) != 0)
                usedFormats.Add(DatFormat.SabreJSON);
            if ((datFormat & DatFormat.SabreXML) != 0)
                usedFormats.Add(DatFormat.SabreXML);
            if ((datFormat & DatFormat.SoftwareList) != 0)
                usedFormats.Add(DatFormat.SoftwareList);
            if ((datFormat & DatFormat.SSV) != 0)
                usedFormats.Add(DatFormat.SSV);
            if ((datFormat & DatFormat.TSV) != 0)
                usedFormats.Add(DatFormat.TSV);
#else
            if (datFormat.HasFlag(DatFormat.ArchiveDotOrg))
                usedFormats.Add(DatFormat.ArchiveDotOrg);
            if (datFormat.HasFlag(DatFormat.AttractMode))
                usedFormats.Add(DatFormat.AttractMode);
            if (datFormat.HasFlag(DatFormat.ClrMamePro))
                usedFormats.Add(DatFormat.ClrMamePro);
            if (datFormat.HasFlag(DatFormat.CSV))
                usedFormats.Add(DatFormat.CSV);
            if (datFormat.HasFlag(DatFormat.DOSCenter))
                usedFormats.Add(DatFormat.DOSCenter);
            if (datFormat.HasFlag(DatFormat.EverdriveSMDB))
                usedFormats.Add(DatFormat.EverdriveSMDB);
            if (datFormat.HasFlag(DatFormat.Listrom))
                usedFormats.Add(DatFormat.Listrom);
            if (datFormat.HasFlag(DatFormat.Listxml))
                usedFormats.Add(DatFormat.Listxml);
            if (datFormat.HasFlag(DatFormat.Logiqx))
                usedFormats.Add(DatFormat.Logiqx);
            if (datFormat.HasFlag(DatFormat.LogiqxDeprecated))
                usedFormats.Add(DatFormat.LogiqxDeprecated);
            if (datFormat.HasFlag(DatFormat.MissFile))
                usedFormats.Add(DatFormat.MissFile);
            if (datFormat.HasFlag(DatFormat.OfflineList))
                usedFormats.Add(DatFormat.OfflineList);
            if (datFormat.HasFlag(DatFormat.OpenMSX))
                usedFormats.Add(DatFormat.OpenMSX);
            if (datFormat.HasFlag(DatFormat.RedumpMD2))
                usedFormats.Add(DatFormat.RedumpMD2);
            if (datFormat.HasFlag(DatFormat.RedumpMD4))
                usedFormats.Add(DatFormat.RedumpMD4);
            if (datFormat.HasFlag(DatFormat.RedumpMD5))
                usedFormats.Add(DatFormat.RedumpMD5);
            if (datFormat.HasFlag(DatFormat.RedumpRIPEMD128))
                usedFormats.Add(DatFormat.RedumpRIPEMD128);
            if (datFormat.HasFlag(DatFormat.RedumpRIPEMD160))
                usedFormats.Add(DatFormat.RedumpRIPEMD160);
            if (datFormat.HasFlag(DatFormat.RedumpSFV))
                usedFormats.Add(DatFormat.RedumpSFV);
            if (datFormat.HasFlag(DatFormat.RedumpSHA1))
                usedFormats.Add(DatFormat.RedumpSHA1);
            if (datFormat.HasFlag(DatFormat.RedumpSHA256))
                usedFormats.Add(DatFormat.RedumpSHA256);
            if (datFormat.HasFlag(DatFormat.RedumpSHA384))
                usedFormats.Add(DatFormat.RedumpSHA384);
            if (datFormat.HasFlag(DatFormat.RedumpSHA512))
                usedFormats.Add(DatFormat.RedumpSHA512);
            if (datFormat.HasFlag(DatFormat.RedumpSpamSum))
                usedFormats.Add(DatFormat.RedumpSpamSum);
            if (datFormat.HasFlag(DatFormat.RomCenter))
                usedFormats.Add(DatFormat.RomCenter);
            if (datFormat.HasFlag(DatFormat.SabreJSON))
                usedFormats.Add(DatFormat.SabreJSON);
            if (datFormat.HasFlag(DatFormat.SabreXML))
                usedFormats.Add(DatFormat.SabreXML);
            if (datFormat.HasFlag(DatFormat.SoftwareList))
                usedFormats.Add(DatFormat.SoftwareList);
            if (datFormat.HasFlag(DatFormat.SSV))
                usedFormats.Add(DatFormat.SSV);
            if (datFormat.HasFlag(DatFormat.TSV))
                usedFormats.Add(DatFormat.TSV);
#endif

            return usedFormats;
        }
    }
}
