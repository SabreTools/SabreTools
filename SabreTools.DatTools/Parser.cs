using System;
using System.IO;
using System.Text.RegularExpressions;

using SabreTools.Core.Tools;
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.IO;
using SabreTools.Logging;

namespace SabreTools.DatTools
{
    /// <summary>
    /// Helper methods for parsing into DatFiles
    /// </summary>
    public class Parser
    {
        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger logger = new Logger();

        #endregion

        /// <summary>
        /// Create a DatFile and parse a file into it
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        public static DatFile CreateAndParse(string filename, bool statsOnly = false, bool throwOnError = false)
        {
            DatFile datFile = DatFile.Create();
            ParseInto(datFile, new ParentablePath(filename), statsOnly: statsOnly, throwOnError: throwOnError);
            return datFile;
        }

        /// <summary>
        /// Parse a DAT and return all found games and roms within
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
        /// <param name="quotes">True if quotes are assumed in supported types (default), false otherwise</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        public static void ParseInto(
            DatFile datFile,
            string filename,
            int indexId = 0,
            bool keep = false,
            bool keepext = false,
            bool quotes = true,
            bool statsOnly = false,
            bool throwOnError = false)
        {
            ParentablePath path = new ParentablePath(filename.Trim('"'));
            ParseInto(datFile, path, indexId, keep, keepext, quotes, statsOnly, throwOnError);
        }

        /// <summary>
        /// Parse a DAT and return all found games and roms within
        /// </summary>
        /// <param name="datFile">Current DatFile object to add to</param>
        /// <param name="input">Name of the file to be parsed</param>
        /// <param name="indexId">Index ID for the DAT</param>
        /// <param name="keep">True if full pathnames are to be kept, false otherwise (default)</param>
        /// <param name="keepext">True if original extension should be kept, false otherwise (default)</param>
        /// <param name="quotes">True if quotes are assumed in supported types (default), false otherwise</param>
        /// <param name="statsOnly">True to only add item statistics while parsing, false otherwise</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        public static void ParseInto(
            DatFile datFile,
            ParentablePath input,
            int indexId = 0,
            bool keep = false,
            bool keepext = false,
            bool quotes = true,
            bool statsOnly = false,
            bool throwOnError = true)
        {
            // Get the current path from the filename
            string currentPath = input.CurrentPath;

            // Check the file extension first as a safeguard
            if (!Utilities.HasValidDatExtension(currentPath))
                return;

            // If the output filename isn't set already, get the internal filename
            datFile.Header.FileName = string.IsNullOrWhiteSpace(datFile.Header.FileName)
                ? (keepext
                    ? Path.GetFileName(currentPath)
                    : Path.GetFileNameWithoutExtension(currentPath))
                : datFile.Header.FileName;

            // If the output type isn't set already, get the internal output type
            DatFormat currentPathFormat = GetDatFormat(currentPath);
            datFile.Header.DatFormat = datFile.Header.DatFormat == 0 ? currentPathFormat : datFile.Header.DatFormat;
            datFile.Items.SetBucketedBy(ItemKey.NULL); // Setting this because it can reduce issues later, must be NULL to ensure first sorting
            
            InternalStopwatch watch = new InternalStopwatch($"Parsing '{currentPath}' into internal DAT");

            // Now parse the correct type of DAT
            try
            {
                var parsingDatFile = DatFile.Create(currentPathFormat, datFile, quotes);
                parsingDatFile?.ParseFile(currentPath, indexId, keep, statsOnly: statsOnly, throwOnError: throwOnError);
            }
            catch (Exception ex) when (!throwOnError)
            {
                logger.Error(ex, $"Error with file '{currentPath}'");
            }

            watch.Stop();
        }

        /// <summary>
        /// Get what type of DAT the input file is
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <returns>The DatFormat corresponding to the DAT</returns>
        private static DatFormat GetDatFormat(string filename)
        {
            // Limit the output formats based on extension
            if (!Utilities.HasValidDatExtension(filename))
                return 0;

            // Get the extension from the filename
            string ext = filename.GetNormalizedExtension();

            // Check if file exists
            if (!File.Exists(filename))
                return 0;
            
            // Some formats should only require the extension to know
            switch (ext)
            {
                case "csv":
                    return DatFormat.CSV;
                case "json":
                    return DatFormat.SabreJSON;
                case "md5":
                    return DatFormat.RedumpMD5;
                case "sfv":
                    return DatFormat.RedumpSFV;
                case "sha1":
                    return DatFormat.RedumpSHA1;
                case "sha256":
                    return DatFormat.RedumpSHA256;
                case "sha384":
                    return DatFormat.RedumpSHA384;
                case "sha512":
                    return DatFormat.RedumpSHA512;
                case "spamsum":
                    return DatFormat.RedumpSpamSum;
                case "ssv":
                    return DatFormat.SSV;
                case "tsv":
                    return DatFormat.TSV;
            }

            // For everything else, we need to read it
            // Get the first two non-whitespace, non-comment lines to check, if possible
            string first = string.Empty, second = string.Empty;

            try
            {
                using StreamReader sr = File.OpenText(filename);
                first = FindNextLine(sr);
                second = FindNextLine(sr);
            }
            catch { }

            // If we have an XML-based DAT
            if (first.Contains("<?xml") && first.Contains("?>"))
            {
                if (second.StartsWith("<!doctype datafile"))
                    return DatFormat.Logiqx;

                else if (second.StartsWith("<!doctype mame")
                    || second.StartsWith("<!doctype m1")
                    || second.StartsWith("<mame")
                    || second.StartsWith("<m1"))
                    return DatFormat.Listxml;

                else if (second.StartsWith("<!doctype softwaredb"))
                    return DatFormat.OpenMSX;

                else if (second.StartsWith("<!doctype softwarelist"))
                    return DatFormat.SoftwareList;

                else if (second.StartsWith("<!doctype sabredat"))
                    return DatFormat.SabreXML;

                else if ((second.StartsWith("<dat") && !second.StartsWith("<datafile"))
                    || second.StartsWith("<?xml-stylesheet"))
                    return DatFormat.OfflineList;
                
                else if (second.StartsWith("<files"))
                    return DatFormat.ArchiveDotOrg;

                // Older and non-compliant DATs
                else
                    return DatFormat.Logiqx;
            }

            // If we have an SMDB (SHA-256, Filename, SHA-1, MD5, CRC32)
            else if (Regex.IsMatch(first, @"[0-9a-f]{64}\t.*?\t[0-9a-f]{40}\t[0-9a-f]{32}\t[0-9a-f]{8}"))
                return DatFormat.EverdriveSMDB;

            // If we have an INI-based DAT
            else if (first.Contains("[") && first.Contains("]"))
                return DatFormat.RomCenter;

            // If we have a listroms DAT
            else if (first.StartsWith("roms required for driver"))
                return DatFormat.Listrom;

            // If we have a CMP-based DAT
            else if (first.Contains("clrmamepro"))
                return DatFormat.ClrMamePro;

            else if (first.Contains("romvault"))
                return DatFormat.ClrMamePro;

            else if (first.Contains("doscenter"))
                return DatFormat.DOSCenter;

            else if (first.Contains("#Name;Title;Emulator;CloneOf;Year;Manufacturer;Category;Players;Rotation;Control;Status;DisplayCount;DisplayType;AltRomname;AltTitle;Extra".ToLowerInvariant()))
                return DatFormat.AttractMode;

            else
                return DatFormat.ClrMamePro;
        }

        /// <summary>
        /// Find the next non-whitespace, non-comment line from an input
        /// </summary>
        /// <param name="sr">StreamReader representing the input</param>
        /// <returns>The next complete line, if possible</returns>
        private static string FindNextLine(StreamReader sr)
        {
            // If we're at the end of the stream, we can't do anything
            if (sr.EndOfStream)
                return string.Empty;

            // Find the first line that's not whitespace or an XML comment
            string line = sr.ReadLine().ToLowerInvariant().Trim();
            bool inComment = line.StartsWith("<!--");
            while ((string.IsNullOrWhiteSpace(line) || inComment) && !sr.EndOfStream)
            {
                // Self-contained comment lines
                if (line.StartsWith("<!--") && line.EndsWith("-->"))
                {
                    inComment = false;
                    line = sr.ReadLine().ToLowerInvariant().Trim();
                }

                // Start of block comments
                else if (line.StartsWith("<!--"))
                {
                    inComment = true;
                    line = sr.ReadLine().ToLowerInvariant().Trim();
                }

                // End of block comments
                else if (inComment && line.EndsWith("-->"))
                {
                    line = sr.ReadLine().ToLowerInvariant().Trim();
                    inComment = line.StartsWith("<!--");
                }

                // Empty lines are just skipped
                else if (string.IsNullOrWhiteSpace(line))
                {
                    line = sr.ReadLine().ToLowerInvariant().Trim();
                    inComment |= line.StartsWith("<!--");
                }

                // In-comment lines
                else if (inComment)
                {
                    line = sr.ReadLine().ToLowerInvariant().Trim();
                }
            }

            // If we ended in a comment, return an empty string
            if (inComment)
                return string.Empty;

            return line;
        }
    }
}
