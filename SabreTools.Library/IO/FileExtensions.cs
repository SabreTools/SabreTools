using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Skippers;

namespace SabreTools.Library.IO
{
    /// <summary>
    /// Extensions to File functionality
    /// </summary>
    public static class FileExtensions
    {
        /// <summary>
        /// Add an aribtrary number of bytes to the inputted file
        /// </summary>
        /// <param name="input">File to be appended to</param>
        /// <param name="output">Outputted file</param>
        /// <param name="bytesToAddToHead">Bytes to be added to head of file</param>
        /// <param name="bytesToAddToTail">Bytes to be added to tail of file</param>
        public static void AppendBytes(string input, string output, byte[] bytesToAddToHead, byte[] bytesToAddToTail)
        {
            // If any of the inputs are invalid, skip
            if (!File.Exists(input))
                return;

#if NET_FRAMEWORK
            using (FileStream fsr = TryOpenRead(input))
            using (FileStream fsw = TryOpenWrite(output))
            {
#else
            using FileStream fsr = TryOpenRead(input);
            using FileStream fsw = TryOpenWrite(output);
#endif
                StreamExtensions.AppendBytes(fsr, fsw, bytesToAddToHead, bytesToAddToTail);
#if NET_FRAMEWORK
            }
#endif
        }

        /// <summary>
        /// Get what type of DAT the input file is
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <returns>The DatFormat corresponding to the DAT</returns>
        public static DatFormat GetDatFormat(this string filename)
        {
            // Limit the output formats based on extension
            if (!PathExtensions.HasValidDatExtension(filename))
                return 0;

            // Get the extension from the filename
            string ext = PathExtensions.GetNormalizedExtension(filename);

            // Read the input file, if possible
            Globals.Logger.Verbose($"Attempting to read file to get format: {filename}");

            // Check if file exists
            if (!File.Exists(filename))
            {
                Globals.Logger.Warning($"File '{filename}' could not read from!");
                return 0;
            }

            // Some formats should only require the extension to know
            switch (ext)
            {
                case "csv":
                    return DatFormat.CSV;
                case "json":
                    return DatFormat.Json;
                case "md5":
                    return DatFormat.RedumpMD5;
#if NET_FRAMEWORK
                case "ripemd160":
                    return DatFormat.RedumpRIPEMD160;
#endif
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
                case "ssv":
                    return DatFormat.SSV;
                case "tsv":
                    return DatFormat.TSV;
            }

            // For everything else, we need to read it
            try
            {
                // Get the first two non-whitespace, non-comment lines to check, if possible
                string first = string.Empty, second = string.Empty;

                try
                {
                    using (StreamReader sr = File.OpenText(filename))
                    {
                        first = sr.ReadLine().ToLowerInvariant();
                        while ((string.IsNullOrWhiteSpace(first) || first.StartsWith("<!--"))
                            && !sr.EndOfStream)
                        {
                            first = sr.ReadLine().ToLowerInvariant();
                        }

                        if (!sr.EndOfStream)
                        {
                            second = sr.ReadLine().ToLowerInvariant();
                            while (string.IsNullOrWhiteSpace(second) || second.StartsWith("<!--")
                                && !sr.EndOfStream)
                            {
                                second = sr.ReadLine().ToLowerInvariant();
                            }
                        }
                    }
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
                        return DatFormat.SabreDat;

                    else if ((second.StartsWith("<dat") && !second.StartsWith("<datafile"))
                        || second.StartsWith("<?xml-stylesheet"))
                        return DatFormat.OfflineList;

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

                else if (first.Contains("#Name;Title;Emulator;CloneOf;Year;Manufacturer;Category;Players;Rotation;Control;Status;DisplayCount;DisplayType;AltRomname;AltTitle;Extra"))
                    return DatFormat.AttractMode;

                else
                    return DatFormat.ClrMamePro;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        /// <link>http://stackoverflow.com/questions/3825390/effective-way-to-find-any-files-encoding</link>
        public static Encoding GetEncoding(string filename)
        {
            // Read the BOM
            var bom = new byte[4];
            FileStream file = FileExtensions.TryOpenRead(filename);
            file.Read(bom, 0, 4);
            file.Dispose();

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.Default;
        }

        /// <summary>
        /// Returns the file type of an input file
        /// </summary>
        /// <param name="input">Input file to check</param>
        /// <returns>FileType of inputted file (null on error)</returns>
        public static FileType? GetFileType(this string input)
        {
            FileType? outFileType = null;

            // If the file is null, then we have no archive type
            if (input == null)
                return outFileType;

            // First line of defense is going to be the extension, for better or worse
            if (!PathExtensions.HasValidArchiveExtension(input))
                return outFileType;

            // Read the first bytes of the file and get the magic number
            try
            {
                byte[] magic = new byte[8];
                BinaryReader br = new BinaryReader(TryOpenRead(input));
                magic = br.ReadBytes(8);
                br.Dispose();

                // Now try to match it to a known signature
                if (magic.StartsWith(Constants.SevenZipSignature))
                {
                    outFileType = FileType.SevenZipArchive;
                }
                else if (magic.StartsWith(Constants.AaruFormatSignature))
                {
                    outFileType = FileType.AaruFormat;
                }
                else if (magic.StartsWith(Constants.CHDSignature))
                {
                    outFileType = FileType.CHD;
                }
                else if (magic.StartsWith(Constants.GzSignature))
                {
                    outFileType = FileType.GZipArchive;
                }
                else if (magic.StartsWith(Constants.LRZipSignature))
                {
                    outFileType = FileType.LRZipArchive;
                }
                else if (magic.StartsWith(Constants.LZ4Signature)
                    || magic.StartsWith(Constants.LZ4SkippableMinSignature)
                    || magic.StartsWith(Constants.LZ4SkippableMaxSignature))
                {
                    outFileType = FileType.LZ4Archive;
                }
                else if (magic.StartsWith(Constants.RarSignature)
                    || magic.StartsWith(Constants.RarFiveSignature))
                {
                    outFileType = FileType.RarArchive;
                }
                else if (magic.StartsWith(Constants.TarSignature)
                    || magic.StartsWith(Constants.TarZeroSignature))
                {
                    outFileType = FileType.TapeArchive;
                }
                else if (magic.StartsWith(Constants.XZSignature))
                {
                    outFileType = FileType.XZArchive;
                }
                else if (magic.StartsWith(Constants.ZipSignature)
                    || magic.StartsWith(Constants.ZipSignatureEmpty)
                    || magic.StartsWith(Constants.ZipSignatureSpanned))
                {
                    outFileType = FileType.ZipArchive;
                }
                else if (magic.StartsWith(Constants.ZPAQSignature))
                {
                    outFileType = FileType.ZPAQArchive;
                }
                else if (magic.StartsWith(Constants.ZstdSignature))
                {
                    outFileType = FileType.ZstdArchive;
                }
            }
            catch (Exception)
            {
                // Don't log file open errors
            }

            return outFileType;
        }

        /// <summary>
        /// Returns if the first byte array starts with the second array
        /// </summary>
        /// <param name="arr1">First byte array to compare</param>
        /// <param name="arr2">Second byte array to compare</param>
        /// <param name="exact">True if the input arrays should match exactly, false otherwise (default)</param>
        /// <returns>True if the first byte array starts with the second, false otherwise</returns>
        public static bool StartsWith(this byte[] arr1, byte[] arr2, bool exact = false)
        {
            // If we have any invalid inputs, we return false
            if (arr1 == null || arr2 == null
                || arr1.Length == 0 || arr2.Length == 0
                || arr2.Length > arr1.Length
                || (exact && arr1.Length != arr2.Length))
            {
                return false;
            }

            // Otherwise, loop through and see
            for (int i = 0; i < arr2.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Retrieve file information for a single file
        /// </summary>
        /// <param name="input">Filename to get information from</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
        /// <param name="date">True if the file Date should be included, false otherwise (default)</param>
        /// <param name="header">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <param name="aaruFormatAsFiles">True if AaruFormats should be treated like regular files, false otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <returns>Populated BaseFile object if success, empty one on error</returns>
        public static BaseFile GetInfo(string input, Hash omitFromScan = 0x0, bool date = false, string header = null, bool aaruFormatAsFiles = true, bool chdsAsFiles = true)
        {
            // Add safeguard if file doesn't exist
            if (!File.Exists(input))
                return null;

            // Get the information from the file stream
            BaseFile baseFile;
            if (header != null)
            {
                SkipperRule rule = Transform.GetMatchingRule(input, Path.GetFileNameWithoutExtension(header));

                // If there's a match, get the new information from the stream
                if (rule.Tests != null && rule.Tests.Count != 0)
                {
                    // Create the input and output streams
                    MemoryStream outputStream = new MemoryStream();
                    FileStream inputStream = TryOpenRead(input);

                    // Transform the stream and get the information from it
                    rule.TransformStream(inputStream, outputStream, keepReadOpen: false, keepWriteOpen: true);
                    baseFile = outputStream.GetInfo(omitFromScan: omitFromScan, keepReadOpen: false, aaruFormatAsFiles: aaruFormatAsFiles, chdsAsFiles: chdsAsFiles);

                    // Dispose of the streams
                    outputStream.Dispose();
                    inputStream.Dispose();
                }
                // Otherwise, just get the info
                else
                {
                    baseFile = TryOpenRead(input).GetInfo(omitFromScan: omitFromScan, keepReadOpen: false, aaruFormatAsFiles: aaruFormatAsFiles, chdsAsFiles: chdsAsFiles);
                }
            }
            else
            {
                baseFile = TryOpenRead(input).GetInfo(omitFromScan: omitFromScan, keepReadOpen: false, aaruFormatAsFiles: aaruFormatAsFiles, chdsAsFiles: chdsAsFiles);
            }

            // Add unique data from the file
            baseFile.Filename = Path.GetFileName(input);
            baseFile.Date = (date ? new FileInfo(input).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss") : string.Empty);

            return baseFile;
        }

        /// <summary>
        /// Get the IniReader associated with a file, if possible
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <param name="validateRows">True if rows should be in a proper format, false if invalid is okay</param>
        /// <returns>The IniReader representing the (possibly converted) file, null otherwise</returns>
        public static IniReader GetIniReader(this string filename, bool validateRows)
        {
            Globals.Logger.Verbose($"Attempting to read file: {filename}");

            // Check if file exists
            if (!File.Exists(filename))
            {
                Globals.Logger.Warning($"File '{filename}' could not read from!");
                return null;
            }

            IniReader ir = new IniReader(filename)
            {
                ValidateRows = validateRows
            };
            return ir;
        }

        /// <summary>
        /// Get the XmlTextReader associated with a file, if possible
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <returns>The XmlTextReader representing the (possibly converted) file, null otherwise</returns>
        public static XmlReader GetXmlTextReader(this string filename)
        {
            Globals.Logger.Verbose($"Attempting to read file: {filename}");

            // Check if file exists
            if (!File.Exists(filename))
            {
                Globals.Logger.Warning($"File '{filename}' could not read from!");
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
                    throw ex;
                else
                    return null;
            }
        }

        /// <summary>
        /// Try to safely delete a file, optionally throwing the error
        /// </summary>
        /// <param name="file">Name of the file to delete</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the file didn't exist or could be deleted, false otherwise</returns>
        public static bool TryDelete(string file, bool throwOnError = false)
        {
            // Check if the file exists first
            if (!File.Exists(file))
                return true;

            // Now wrap deleting the file
            try
            {
                File.Delete(file);
                return true;
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw ex;
                else
                    return false;
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
                return null;

            // Now wrap opening the file
            try
            {
                return File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw ex;
                else
                    return null;
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
                return null;

            // Now wrap opening the file
            try
            {
                return File.Open(file, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw ex;
                else
                    return null;
            }
        }

        /// <summary>
        /// Try to open an existing file for write, optionally throwing the error
        /// </summary>
        /// <param name="file">Name of the file to open</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>An opened stream representing the file on success, null otherwise</returns>
        public static FileStream TryOpenWrite(string file, bool throwOnError = false)
        {
            // Check if the file exists first
            if (!File.Exists(file))
                return null;

            // Now wrap opening the file
            try
            {
                return File.Open(file, FileMode.Open, FileAccess.Write, FileShare.ReadWrite);
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw ex;
                else
                    return null;
            }
        }
    }
}
