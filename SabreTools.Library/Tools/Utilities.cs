using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Reports;
using SabreTools.Library.Skippers;
using Compress.ThreadReaders;
using NaturalSort;

namespace SabreTools.Library.Tools
{
    /// <summary>
    /// Static utility functions used throughout the library
    /// </summary>
    public static class Utilities
    {
        #region BinaryReader Extensions

        /// <summary>
        /// Reads the specified number of bytes from the stream, starting from a specified point in the byte array.
        /// </summary>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="index">The starting point in the buffer at which to begin reading into the buffer.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>The number of bytes read into buffer. This might be less than the number of bytes requested if that many bytes are not available, or it might be zero if the end of the stream is reached.</returns>
        public static int ReadReverse(this BinaryReader reader, byte[] buffer, int index, int count)
        {
            int retval = reader.Read(buffer, index, count);
            buffer = buffer.Reverse().ToArray();
            return retval;
        }

        /// <summary>
        /// Reads the specified number of characters from the stream, starting from a specified point in the character array.
        /// </summary>
        /// <param name="buffer">The buffer to read data into.</param>
        /// <param name="index">The starting point in the buffer at which to begin reading into the buffer.</param>
        /// <param name="count">The number of characters to read.</param>
        /// <returns>The total number of characters read into the buffer. This might be less than the number of characters requested if that many characters are not currently available, or it might be zero if the end of the stream is reached.</returns>
        public static int ReadReverse(this BinaryReader reader, char[] buffer, int index, int count)

        {
            int retval = reader.Read(buffer, index, count);
            buffer = buffer.Reverse().ToArray();
            return retval;
        }

        /// <summary>
        /// Reads the specified number of bytes from the current stream into a byte array and advances the current position by that number of bytes.
        /// </summary>
        /// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
        /// <returns>A byte array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
        public static byte[] ReadBytesReverse(this BinaryReader reader, int count)
        {
            byte[] retval = reader.ReadBytes(count);
            retval = retval.Reverse().ToArray();
            return retval;
        }

        /// <summary>
        /// Reads a decimal value from the current stream and advances the current position of the stream by sixteen bytes.
        /// </summary>
        /// <returns>A decimal value read from the current stream.</returns>
        public static decimal ReadDecimalReverse(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(16);
            retval = retval.Reverse().ToArray();

            int i1 = BitConverter.ToInt32(retval, 0);
            int i2 = BitConverter.ToInt32(retval, 4);
            int i3 = BitConverter.ToInt32(retval, 8);
            int i4 = BitConverter.ToInt32(retval, 12);

            return new decimal(new int[] { i1, i2, i3, i4 });
        }

        /// <summary>
        /// eads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>An 8-byte floating point value read from the current stream.</returns>
        public static double ReadDoubleReverse(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(8);
            retval = retval.Reverse().ToArray();
            return BitConverter.ToDouble(retval, 0);
        }

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.
        /// </summary>
        /// <returns>A 2-byte signed integer read from the current stream.</returns>
        public static short ReadInt16Reverse(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(2);
            retval = retval.Reverse().ToArray();
            return BitConverter.ToInt16(retval, 0);
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte signed integer read from the current stream.</returns>
        public static int ReadInt32Reverse(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(4);
            retval = retval.Reverse().ToArray();
            return BitConverter.ToInt32(retval, 0);
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.
        /// </summary>
        /// <returns>An 8-byte signed integer read from the current stream.</returns>
        public static long ReadInt64Reverse(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(8);
            retval = retval.Reverse().ToArray();
            return BitConverter.ToInt64(retval, 0);
        }

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte floating point value read from the current stream.</returns>
        public static float ReadSingleReverse(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(4);
            retval = retval.Reverse().ToArray();
            return BitConverter.ToSingle(retval, 0);
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.
        /// 
        /// This API is not CLS-compliant.
        /// </summary>
        /// <returns>A 2-byte unsigned integer read from this stream.</returns>
        public static ushort ReadUInt16Reverse(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(2);
            retval = retval.Reverse().ToArray();
            return BitConverter.ToUInt16(retval, 0);
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.
        /// 
        /// This API is not CLS-compliant.
        /// </summary>
        /// <returns>A 4-byte unsigned integer read from this stream.</returns>
        public static uint ReadUInt32Reverse(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(4);
            retval = retval.Reverse().ToArray();
            return BitConverter.ToUInt32(retval, 0);
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.
        /// 
        /// This API is not CLS-compliant.
        /// </summary>
        /// <returns>An 8-byte unsigned integer read from this stream.</returns>
        public static ulong ReadUInt64Reverse(this BinaryReader reader)
        {
            byte[] retval = reader.ReadBytes(8);
            retval = retval.Reverse().ToArray();
            return BitConverter.ToUInt64(retval, 0);
        }

        #endregion

        #region DAT Cleaning

        /// <summary>
        /// Clean a game (or rom) name to the WoD standard
        /// </summary>
        /// <param name="game">Name of the game to be cleaned</param>
        /// <returns>The cleaned name</returns>
        public static string CleanGameName(string game)
        {
            ///Run the name through the filters to make sure that it's correct
            game = NormalizeChars(game);
            game = RussianToLatin(game);
            game = SearchPattern(game);

            game = new Regex(@"(([[(].*[\)\]] )?([^([]+))").Match(game).Groups[1].Value;
            game = game.TrimStart().TrimEnd();
            return game;
        }

        /// <summary>
        /// Clean a game (or rom) name to the WoD standard
        /// </summary>
        /// <param name="game">Array representing the path to be cleaned</param>
        /// <returns>The cleaned name</returns>
        public static string CleanGameName(string[] game)
        {
            game[game.Length - 1] = CleanGameName(game.Last());
            string outgame = string.Join(Path.DirectorySeparatorChar.ToString(), game);
            outgame = outgame.TrimStart().TrimEnd();
            return outgame;
        }

        /// <summary>
        /// Clean a hash string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <param name="padding">Amount of characters to pad to</param>
        /// <returns>Cleaned string</returns>
        public static string CleanHashData(string hash, int padding)
        {
            // If we have a known blank hash, return blank
            if (string.IsNullOrWhiteSpace(hash) || hash == "-" || hash == "_")
                return string.Empty;

            // Check to see if it's a "hex" hash
            hash = hash.Trim().Replace("0x", string.Empty);

            // If we have a blank hash now, return blank
            if (string.IsNullOrWhiteSpace(hash))
                return string.Empty;

            // If the hash shorter than the required length, pad it
            if (hash.Length < padding)
                hash = hash.PadLeft(padding, '0');

            // If the hash is longer than the required length, it's invalid
            else if (hash.Length > padding)
                return string.Empty;

            // Now normalize the hash
            hash = hash.ToLowerInvariant();

            // Otherwise, make sure that every character is a proper match
            for (int i = 0; i < hash.Length; i++)
            {
                if ((hash[i] < '0' || hash[i] > '9') && (hash[i] < 'a' || hash[i] > 'f'))
                {
                    hash = string.Empty;
                    break;
                }
            }

            return hash;
        }

        /// <summary>
        /// Clean a hash string from a Listrom DAT
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        public static string CleanListromHashData(string hash)
        {
            if (hash.StartsWith("CRC"))
                return hash.Substring(4, 8).ToLowerInvariant();

            else if (hash.StartsWith("SHA1"))
                return hash.Substring(5, 40).ToLowerInvariant();

            return hash;
        }

        /// <summary>
        /// Replace accented characters
        /// </summary>
        /// <param name="input">String to be parsed</param>
        /// <returns>String with characters replaced</returns>
        public static string NormalizeChars(string input)
        {
            string[,] charmap = {
                { "Á", "A" },   { "á", "a" },
                { "À", "A" },   { "à", "a" },
                { "Â", "A" },   { "â", "a" },
                { "Ä", "Ae" },  { "ä", "ae" },
                { "Ã", "A" },   { "ã", "a" },
                { "Å", "A" },   { "å", "a" },
                { "Æ", "Ae" },  { "æ", "ae" },
                { "Ç", "C" },   { "ç", "c" },
                { "Ð", "D" },   { "ð", "d" },
                { "É", "E" },   { "é", "e" },
                { "È", "E" },   { "è", "e" },
                { "Ê", "E" },   { "ê", "e" },
                { "Ë", "E" },   { "ë", "e" },
                { "ƒ", "f" },
                { "Í", "I" },   { "í", "i" },
                { "Ì", "I" },   { "ì", "i" },
                { "Î", "I" },   { "î", "i" },
                { "Ï", "I" },   { "ï", "i" },
                { "Ñ", "N" },   { "ñ", "n" },
                { "Ó", "O" },   { "ó", "o" },
                { "Ò", "O" },   { "ò", "o" },
                { "Ô", "O" },   { "ô", "o" },
                { "Ö", "Oe" },  { "ö", "oe" },
                { "Õ", "O" },   { "õ", "o" },
                { "Ø", "O" },   { "ø", "o" },
                { "Š", "S" },   { "š", "s" },
                { "ß", "ss" },
                { "Þ", "B" },   { "þ", "b" },
                { "Ú", "U" },   { "ú", "u" },
                { "Ù", "U" },   { "ù", "u" },
                { "Û", "U" },   { "û", "u" },
                { "Ü", "Ue" },  { "ü", "ue" },
                { "ÿ", "y" },
                { "Ý", "Y" },   { "ý", "y" },
                { "Ž", "Z" },   { "ž", "z" },
            };

            for (int i = 0; i < charmap.GetLength(0); i++)
            {
                input = input.Replace(charmap[i, 0], charmap[i, 1]);
            }

            return input;
        }

        /// <summary>
        /// Convert Cyrillic lettering to Latin lettering
        /// </summary>
        /// <param name="input">String to be parsed</param>
        /// <returns>String with characters replaced</returns>
        public static string RussianToLatin(string input)
        {
            string[,] charmap = {
                    { "А", "A" }, { "Б", "B" }, { "В", "V" }, { "Г", "G" }, { "Д", "D" },
                    { "Е", "E" }, { "Ё", "Yo" }, { "Ж", "Zh" }, { "З", "Z" }, { "И", "I" },
                    { "Й", "J" }, { "К", "K" }, { "Л", "L" }, { "М", "M" }, { "Н", "N" },
                    { "О", "O" }, { "П", "P" }, { "Р", "R" }, { "С", "S" }, { "Т", "T" },
                    { "У", "U" }, { "Ф", "f" }, { "Х", "Kh" }, { "Ц", "Ts" }, { "Ч", "Ch" },
                    { "Ш", "Sh" }, { "Щ", "Sch" }, { "Ъ", string.Empty }, { "Ы", "y" }, { "Ь", string.Empty },
                    { "Э", "e" }, { "Ю", "yu" }, { "Я", "ya" }, { "а", "a" }, { "б", "b" },
                    { "в", "v" }, { "г", "g" }, { "д", "d" }, { "е", "e" }, { "ё", "yo" },
                    { "ж", "zh" }, { "з", "z" }, { "и", "i" }, { "й", "j" }, { "к", "k" },
                    { "л", "l" }, { "м", "m" }, { "н", "n" }, { "о", "o" }, { "п", "p" },
                    { "р", "r" }, { "с", "s" }, { "т", "t" }, { "у", "u" }, { "ф", "f" },
                    { "х", "kh" }, { "ц", "ts" }, { "ч", "ch" }, { "ш", "sh" }, { "щ", "sch" },
                    { "ъ", string.Empty }, { "ы", "y" }, { "ь", string.Empty }, { "э", "e" }, { "ю", "yu" },
                    { "я", "ya" },
            };

            for (int i = 0; i < charmap.GetLength(0); i++)
            {
                input = input.Replace(charmap[i, 0], charmap[i, 1]);
            }

            return input;
        }

        /// <summary>
        /// Replace special characters and patterns
        /// </summary>
        /// <param name="input">String to be parsed</param>
        /// <returns>String with characters replaced</returns>
        public static string SearchPattern(string input)
        {
            string[,] charmap = {
                { @"~", " - " },
                { @"_", " " },
                { @":", " " },
                { @">", ")" },
                { @"<", "(" },
                { @"\|", "-" },
                { "\"", "'" },
                { @"\*", "." },
                { @"\\", "-" },
                { @"/", "-" },
                { @"\?", " " },
                { @"\(([^)(]*)\(([^)]*)\)([^)(]*)\)", " " },
                { @"\(([^)]+)\)", " " },
                { @"\[([^]]+)\]", " " },
                { @"\{([^}]+)\}", " " },
                { @"(ZZZJUNK|ZZZ-UNK-|ZZZ-UNK |zzz unknow |zzz unk |Copy of |[.][a-z]{3}[.][a-z]{3}[.]|[.][a-z]{3}[.])", " " },
                { @" (r|rev|v|ver)\s*[\d\.]+[^\s]*", " " },
                { @"(( )|(\A))(\d{6}|\d{8})(( )|(\Z))", " " },
                { @"(( )|(\A))(\d{1,2})-(\d{1,2})-(\d{4}|\d{2})", " " },
                { @"(( )|(\A))(\d{4}|\d{2})-(\d{1,2})-(\d{1,2})", " " },
                { @"[-]+", "-" },
                { @"\A\s*\)", " " },
                { @"\A\s*(,|-)", " " },
                { @"\s+", " " },
                { @"\s+,", "," },
                { @"\s*(,|-)\s*\Z", " " },
            };

            for (int i = 0; i < charmap.GetLength(0); i++)
            {
                input = Regex.Replace(input, charmap[i, 0], charmap[i, 1]);
            }

            return input;
        }

        #endregion

        #region Factories

        /// <summary>
        /// Create an archive of the specified type, if possible
        /// </summary>
        /// <param name="input">Name of the file to create the archive from</param>
        /// <returns>Archive object representing the inputs</returns>
        public static BaseArchive GetArchive(string input)
        {
            BaseArchive archive = null;

            // First get the archive type
            FileType? at = GetFileType(input);

            // If we got back null, then it's not an archive, so we we return
            if (at == null)
                return archive;

            // Create the archive based on the type
            Globals.Logger.Verbose($"Found archive of type: {at}");
            switch (at)
            {
                case FileType.GZipArchive:
                    archive = new GZipArchive(input);
                    break;

                case FileType.RarArchive:
                    archive = new RarArchive(input);
                    break;

                case FileType.SevenZipArchive:
                    archive = new SevenZipArchive(input);
                    break;

                case FileType.TapeArchive:
                    archive = new TapeArchive(input);
                    break;

                case FileType.ZipArchive:
                    archive = new ZipArchive(input);
                    break;

                default:
                    // We ignore all other types for now
                    break;
            }

            return archive;
        }

        /// <summary>
        /// Create an archive of the specified type, if possible
        /// </summary>
        /// <param name="archiveType">SharpCompress.Common.ArchiveType representing the archive to create</param>
        /// <returns>Archive object representing the inputs</returns>
        public static BaseArchive GetArchive(FileType archiveType)
        {
            switch (archiveType)
            {
                case FileType.GZipArchive:
                    return new GZipArchive();

                case FileType.RarArchive:
                    return new RarArchive();

                case FileType.SevenZipArchive:
                    return new SevenZipArchive();

                case FileType.TapeArchive:
                    return new TapeArchive();

                case FileType.ZipArchive:
                    return new ZipArchive();

                default:
                    return null;
            }
        }

        /// <summary>
        /// Create an archive of the specified type, if possible
        /// </summary>
        /// <param name="archiveType">SabreTools.Library.Data.OutputFormat representing the archive to create</param>
        /// <returns>Archive object representing the inputs</returns>
        public static Folder GetArchive(OutputFormat outputFormat)
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
                case OutputFormat.TorrentGzipRomba:
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
                    return new ZipArchive();

                case OutputFormat.TorrentZPAQ:
                    return new ZPAQArchive();

                case OutputFormat.TorrentZstd:
                    return new ZstdArchive();

                default:
                    return null;
            }
        }

        /// <summary>
        /// Create a specific type of BaseReport to be used based on a format and user inputs
        /// </summary>
        /// <param name="statReportFormat">Format of the Statistics Report to be created</param>
        /// <param name="filename">Name of the file to write out to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        /// <returns>BaseReport of the specific internal type that corresponds to the inputs</returns>
        public static BaseReport GetBaseReport(StatReportFormat statReportFormat, string filename, bool baddumpCol, bool nodumpCol)
        {
            switch (statReportFormat)
            {
                case StatReportFormat.Textfile:
                    return new Textfile(null, filename, baddumpCol, nodumpCol);

                case StatReportFormat.CSV:
                    return new Reports.SeparatedValue(null, filename, ',', baddumpCol, nodumpCol);

                case StatReportFormat.HTML:
                    return new Html(null, filename, baddumpCol, nodumpCol);

                case StatReportFormat.SSV:
                    return new Reports.SeparatedValue(null, filename, ';', baddumpCol, nodumpCol);

                case StatReportFormat.TSV:
                    return new Reports.SeparatedValue(null, filename, '\t', baddumpCol, nodumpCol);
            }

            return null;
        }

        /// <summary>
        /// Get a sanitized Date from an input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>Date as a string, if possible</returns>
        public static string GetDate(string input)
        {
            string date = string.Empty;
            if (input != null)
            {
                if (DateTime.TryParse(input, out DateTime dateTime))
                    date = dateTime.ToString();
                else
                    date = input;
            }

            return date;
        }

        /// <summary>
        /// Create a specific type of DatFile to be used based on an input file and a base DAT
        /// </summary>
        /// <param name="input">Name of the file to determine the DAT format from</param>
        /// <param name="baseDat">DatFile containing the information to use in specific operations</param>
        /// <returns>DatFile of the specific internal type that corresponds to the inputs</returns>
        public static DatFile GetDatFile(string input, DatFile baseDat)
        {
            DatFormat datFormat = GetDatFormatFromFile(input);
            return GetDatFile(datFormat, baseDat);
        }

        /// <summary>
        /// Create a specific type of DatFile to be used based on a format and a base DAT
        /// </summary>
        /// <param name="datFormat">Format of the DAT to be created</param>
        /// <param name="baseDat">DatFile containing the information to use in specific operations</param>
        /// <returns>DatFile of the specific internal type that corresponds to the inputs</returns>
        public static DatFile GetDatFile(DatFormat datFormat, DatFile baseDat)
        {
            switch (datFormat)
            {
                case DatFormat.AttractMode:
                    return new AttractMode(baseDat);

                case DatFormat.ClrMamePro:
                    return new ClrMamePro(baseDat);

                case DatFormat.CSV:
                    return new DatFiles.SeparatedValue(baseDat, ',');

                case DatFormat.DOSCenter:
                    return new DosCenter(baseDat);

                case DatFormat.EverdriveSMDB:
                    return new EverdriveSMDB(baseDat);

                case DatFormat.Listrom:
                    return new Listrom(baseDat);

                case DatFormat.Listxml:
                    return new Listxml(baseDat);

                case DatFormat.Logiqx:
                    return new Logiqx(baseDat, false);

                case DatFormat.LogiqxDeprecated:
                    return new Logiqx(baseDat, true);

                case DatFormat.MissFile:
                    return new Missfile(baseDat);

                case DatFormat.OfflineList:
                    return new OfflineList(baseDat);

                case DatFormat.OpenMSX:
                    return new OpenMSX(baseDat);

                case DatFormat.RedumpMD5:
                    return new Hashfile(baseDat, Hash.MD5);

                case DatFormat.RedumpRIPEMD160:
                    return new Hashfile(baseDat, Hash.RIPEMD160);

                case DatFormat.RedumpSFV:
                    return new Hashfile(baseDat, Hash.CRC);

                case DatFormat.RedumpSHA1:
                    return new Hashfile(baseDat, Hash.SHA1);

                case DatFormat.RedumpSHA256:
                    return new Hashfile(baseDat, Hash.SHA256);

                case DatFormat.RedumpSHA384:
                    return new Hashfile(baseDat, Hash.SHA384);

                case DatFormat.RedumpSHA512:
                    return new Hashfile(baseDat, Hash.SHA512);

                case DatFormat.RomCenter:
                    return new RomCenter(baseDat);

                case DatFormat.SabreDat:
                    return new SabreDat(baseDat);

                case DatFormat.SoftwareList:
                    return new SoftwareList(baseDat);

                case DatFormat.SSV:
                    return new DatFiles.SeparatedValue(baseDat, ';');

                case DatFormat.TSV:
                    return new DatFiles.SeparatedValue(baseDat, '\t');
            }

            return null;
        }

        /// <summary>
        /// Create a specific type of DatItem to be used based on an ItemType
        /// </summary>
        /// <param name="itemType">Type of the DatItem to be created</param>
        /// <returns>DatItem of the specific internal type that corresponds to the inputs</returns>
        public static DatItem GetDatItem(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Archive:
                    return new Archive();

                case ItemType.BiosSet:
                    return new BiosSet();

                case ItemType.Disk:
                    return new Disk();

                case ItemType.Release:
                    return new Release();

                case ItemType.Sample:
                    return new Sample();

                case ItemType.Rom:
                default:
                    return new Rom();
            }
        }

        /// <summary>
        /// Create a specific type of DatItem to be used based on a BaseFile
        /// </summary>
        /// <param name="baseFile">BaseFile containing information to be created</param>
        /// <returns>DatItem of the specific internal type that corresponds to the inputs</returns>
        public static DatItem GetDatItem(BaseFile baseFile)
        {
            switch (baseFile.Type)
            {
                case FileType.CHD:
                    return new Disk(baseFile);

                case FileType.GZipArchive:
                case FileType.LRZipArchive:
                case FileType.LZ4Archive:
                case FileType.None:
                case FileType.RarArchive:
                case FileType.SevenZipArchive:
                case FileType.TapeArchive:
                case FileType.XZArchive:
                case FileType.ZipArchive:
                case FileType.ZPAQArchive:
                case FileType.ZstdArchive:
                    return new Rom(baseFile);

                case FileType.Folder:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get DatFormat value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>DatFormat value corresponding to the string</returns>
        public static DatFormat GetDatFormat(string input)
        {
            switch (input?.Trim().ToLowerInvariant())
            {
                case "all":
                    return DatFormat.ALL;
                case "am":
                case "attractmode":
                    return DatFormat.AttractMode;
                case "cmp":
                case "clrmamepro":
                    return DatFormat.ClrMamePro;
                case "csv":
                    return DatFormat.CSV;
                case "dc":
                case "doscenter":
                    return DatFormat.DOSCenter;
                case "lr":
                case "listrom":
                    return DatFormat.Listrom;
                case "lx":
                case "listxml":
                    return DatFormat.Listxml;
                case "md5":
                    return DatFormat.RedumpMD5;
                case "miss":
                case "missfile":
                    return DatFormat.MissFile;
                case "msx":
                case "openmsx":
                    return DatFormat.OpenMSX;
                case "ol":
                case "offlinelist":
                    return DatFormat.OfflineList;
                case "rc":
                case "romcenter":
                    return DatFormat.RomCenter;
                case "ripemd160":
                    return DatFormat.RedumpRIPEMD160;
                case "sd":
                case "sabredat":
                    return DatFormat.SabreDat;
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
                case "sl":
                case "softwarelist":
                    return DatFormat.SoftwareList;
                case "smdb":
                case "everdrive":
                    return DatFormat.EverdriveSMDB;
                case "ssv":
                    return DatFormat.SSV;
                case "tsv":
                    return DatFormat.TSV;
                case "xml":
                case "logiqx":
                    return DatFormat.Logiqx;
                default:
                    return 0x0;
            }
        }

        /// <summary>
        /// Get Field value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>Field value corresponding to the string</returns>
        public static Field GetField(string input)
        {
            switch (input?.ToLowerInvariant())
            {
                case "areaname":
                    return Field.AreaName;
                case "areasize":
                    return Field.AreaSize;
                case "bios":
                    return Field.Bios;
                case "biosdescription":
                case "bios description":
                case "biossetdescription":
                case "biosset description":
                case "bios set description":
                    return Field.BiosDescription;
                case "board":
                    return Field.Board;
                case "cloneof":
                    return Field.CloneOf;
                case "comment":
                    return Field.Comment;
                case "crc":
                    return Field.CRC;
                case "default":
                    return Field.Default;
                case "date":
                    return Field.Date;
                case "description":
                    return Field.Description;
                case "devices":
                    return Field.Devices;
                case "features":
                    return Field.Features;
                case "gamename":
                case "machinename":
                    return Field.MachineName;
                case "gametype":
                case "machinetype":
                    return Field.MachineType;
                case "index":
                    return Field.Index;
                case "infos":
                    return Field.Infos;
                case "language":
                    return Field.Language;
                case "manufacturer":
                    return Field.Manufacturer;
                case "md5":
                    return Field.MD5;
                case "merge":
                    return Field.Merge;
                case "name":
                    return Field.Name;
                case "offset":
                    return Field.Offset;
                case "optional":
                    return Field.Optional;
                case "partinterface":
                    return Field.PartInterface;
                case "partname":
                    return Field.PartName;
                case "publisher":
                    return Field.Publisher;
                case "rebuildto":
                    return Field.RebuildTo;
                case "region":
                    return Field.Region;
                case "ripemd160":
                    return Field.RIPEMD160;
                case "romof":
                    return Field.RomOf;
                case "runnable":
                    return Field.Runnable;
                case "sampleof":
                    return Field.SampleOf;
                case "sha1":
                    return Field.SHA1;
                case "sha256":
                    return Field.SHA256;
                case "sha384":
                    return Field.SHA384;
                case "sha512":
                    return Field.SHA512;
                case "size":
                    return Field.Size;
                case "slotoptions":
                    return Field.SlotOptions;
                case "sourcefile":
                    return Field.SourceFile;
                case "status":
                    return Field.Status;
                case "supported":
                    return Field.Supported;
                case "writable":
                    return Field.Writable;
                case "year":
                    return Field.Year;
                default:
                    return Field.NULL;
            }
        }

        /// <summary>
        /// Get ForceMerging value from input string
        /// </summary>
        /// <param name="forcemerge">String to get value from</param>
        /// <returns>ForceMerging value corresponding to the string</returns>
        public static ForceMerging GetForceMerging(string forcemerge)
        {
            switch (forcemerge?.ToLowerInvariant())
            {
                case "split":
                    return ForceMerging.Split;
                case "merged":
                    return ForceMerging.Merged;
                case "nonmerged":
                    return ForceMerging.NonMerged;
                case "full":
                    return ForceMerging.Full;
                case "none":
                default:
                    return ForceMerging.None;
            }
        }

        /// <summary>
        /// Get ForceNodump value from input string
        /// </summary>
        /// <param name="forcend">String to get value from</param>
        /// <returns>ForceNodump value corresponding to the string</returns>
        public static ForceNodump GetForceNodump(string forcend)
        {
            switch (forcend?.ToLowerInvariant())
            {
                case "obsolete":
                    return ForceNodump.Obsolete;
                case "required":
                    return ForceNodump.Required;
                case "ignore":
                    return ForceNodump.Ignore;
                case "none":
                default:
                    return ForceNodump.None;
            }
        }

        /// <summary>
        /// Get ForcePacking value from input string
        /// </summary>
        /// <param name="forcepack">String to get value from</param>
        /// <returns>ForcePacking value corresponding to the string</returns>
        public static ForcePacking GetForcePacking(string forcepack)
        {
            switch (forcepack?.ToLowerInvariant())
            {
                case "yes":
                case "zip":
                    return ForcePacking.Zip;
                case "no":
                case "unzip":
                    return ForcePacking.Unzip;
                case "none":
                default:
                    return ForcePacking.None;
            }
        }

        /// <summary>
        /// Get ItemStatus value from input string
        /// </summary>
        /// <param name="status">String to get value from</param>
        /// <returns>ItemStatus value corresponding to the string</returns>
        public static ItemStatus GetItemStatus(string status)
        {
            switch (status?.ToLowerInvariant())
            {
                case "good":
                    return ItemStatus.Good;
                case "baddump":
                    return ItemStatus.BadDump;
                case "nodump":
                case "yes":
                    return ItemStatus.Nodump;
                case "verified":
                    return ItemStatus.Verified;
                case "none":
                case "no":
                default:
                    return ItemStatus.None;
            }
        }

        /// <summary>
        /// Get ItemType? value from input string
        /// </summary>
        /// <param name="itemType">String to get value from</param>
        /// <returns>ItemType? value corresponding to the string</returns>
        public static ItemType? GetItemType(string itemType)
        {
            switch (itemType?.ToLowerInvariant())
            {
                case "archive":
                    return ItemType.Archive;
                case "biosset":
                    return ItemType.BiosSet;
                case "disk":
                    return ItemType.Disk;
                case "release":
                    return ItemType.Release;
                case "rom":
                    return ItemType.Rom;
                case "sample":
                    return ItemType.Sample;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get MachineType value from input string
        /// </summary>
        /// <param name="gametype">String to get value from</param>
        /// <returns>MachineType value corresponding to the string</returns>
        public static MachineType GetMachineType(string gametype)
        {
            switch (gametype?.ToLowerInvariant())
            {
                case "bios":
                    return MachineType.Bios;
                case "dev":
                case "device":
                    return MachineType.Device;
                case "mech":
                case "mechanical":
                    return MachineType.Mechanical;
                case "none":
                default:
                    return MachineType.None;
            }
        }

        /// <summary>
        /// Get StatReportFormat value from input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>StatReportFormat value corresponding to the string</returns>
        public static StatReportFormat GetStatFormat(string input)
        {
            switch (input?.Trim().ToLowerInvariant())
            {
                case "all":
                    return StatReportFormat.All;
                case "csv":
                    return StatReportFormat.CSV;
                case "html":
                    return StatReportFormat.HTML;
                case "ssv":
                    return StatReportFormat.SSV;
                case "text":
                    return StatReportFormat.Textfile;
                case "tsv":
                    return StatReportFormat.TSV;
                default:
                    return 0x0;
            }
        }

        /// <summary>
        /// Get a sanitized size from an input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>Size as a long, if possible</returns>
        public static long GetSize(string input)
        {
            long size = -1;
            if (input != null && input.Contains("0x"))
                size = Convert.ToInt64(input, 16);

            else if (input != null)
                Int64.TryParse(input, out size);

            return size;
        }

        /// <summary>
        /// Get SplitType value from input ForceMerging
        /// </summary>
        /// <param name="forceMerging">ForceMerging to get value from</param>
        /// <returns>SplitType value corresponding to the string</returns>
        public static SplitType GetSplitType(ForceMerging forceMerging)
        {
            switch (forceMerging)
            {
                case ForceMerging.Split:
                    return SplitType.Split;
                case ForceMerging.Merged:
                    return SplitType.Merged;
                case ForceMerging.NonMerged:
                    return SplitType.NonMerged;
                case ForceMerging.Full:
                    return SplitType.FullNonMerged;
                case ForceMerging.None:
                default:
                    return SplitType.None;
            }
        }

        /// <summary>
        /// Get bool value from input string
        /// </summary>
        /// <param name="yesno">String to get value from</param>
        /// <returns>Bool corresponding to the string</returns>
        public static bool? GetYesNo(string yesno)
        {
            switch (yesno?.ToLowerInvariant())
            {
                case "yes":
                    return true;
                case "no":
                    return false;
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
        /// <returns>A CHDFile object with internal SHA-1 on success, null otherwise</returns>
        /// <remarks>
        /// Original code had a "writable" param. This is not required for metadata checking
        /// </remarks>
        public static BaseFile GetCHDInfo(string input)
        {
            FileStream fs = TryOpenRead(input);
            BaseFile chd = GetCHDInfo(fs);
            fs.Dispose();
            return chd;
        }

        /// <summary>
        /// Get what type of DAT the input file is
        /// </summary>
        /// <param name="filename">Name of the file to be parsed</param>
        /// <returns>The DatFormat corresponding to the DAT</returns>
        public static DatFormat GetDatFormatFromFile(string filename)
        {
            // Limit the output formats based on extension
            if (!HasValidDatExtension(filename))
                return 0;

            // Get the extension from the filename
            string ext = GetExtension(filename);

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
                case "md5":
                    return DatFormat.RedumpMD5;
                case "ripemd160":
                    return DatFormat.RedumpRIPEMD160;
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
                // Get the first two non-whitespace, non-comment lines to check
                StreamReader sr = File.OpenText(filename);
                string first = sr.ReadLine().ToLowerInvariant();
                while (string.IsNullOrWhiteSpace(first) || first.StartsWith("<!--"))
                    first = sr.ReadLine().ToLowerInvariant();

                string second = sr.ReadLine().ToLowerInvariant();
                while (string.IsNullOrWhiteSpace(second) || second.StartsWith("<!--"))
                    second = sr.ReadLine().ToLowerInvariant();

                sr.Dispose();

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
        /// Get all empty folders within a root folder
        /// </summary>
        /// <param name="root">Root directory to parse</param>
        /// <returns>IEumerable containing all directories that are empty, an empty enumerable if the root is empty, null otherwise</returns>
        public static IEnumerable<string> GetEmptyDirectories(string root)
        {
            // Check if the root exists first
            if (!Directory.Exists(root))
                return null;

            // If it does and it is empty, return a blank enumerable
            if (Directory.EnumerateFileSystemEntries(root, "*", SearchOption.AllDirectories).Count() == 0)
                return new List<string>();

            // Otherwise, get the complete list
            return Directory.EnumerateDirectories(root, "*", SearchOption.AllDirectories)
                .Where(dir => Directory.EnumerateFileSystemEntries(dir, "*", SearchOption.AllDirectories).Count() == 0);
        }

        /// <summary>
        /// Retrieve file information for a single file
        /// </summary>
        /// <param name="input">Filename to get information from</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
        /// <param name="date">True if the file Date should be included, false otherwise (default)</param>
        /// <param name="header">Populated string representing the name of the skipper to use, a blank string to use the first available checker, null otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <returns>Populated BaseFile object if success, empty one on error</returns>
        public static BaseFile GetFileInfo(string input, Hash omitFromScan = 0x0, bool date = false, string header = null, bool chdsAsFiles = true)
        {
            // Add safeguard if file doesn't exist
            if (!File.Exists(input))
                return null;

            // Get the information from the file stream
            BaseFile baseFile = new BaseFile();
            if (header != null)
            {
                SkipperRule rule = Skipper.GetMatchingRule(input, Path.GetFileNameWithoutExtension(header));

                // If there's a match, get the new information from the stream
                if (rule.Tests != null && rule.Tests.Count != 0)
                {
                    // Create the input and output streams
                    MemoryStream outputStream = new MemoryStream();
                    FileStream inputStream = TryOpenRead(input);

                    // Transform the stream and get the information from it
                    rule.TransformStream(inputStream, outputStream, keepReadOpen: false, keepWriteOpen: true);
                    baseFile = GetStreamInfo(outputStream, outputStream.Length, omitFromScan, false, chdsAsFiles);

                    // Dispose of the streams
                    outputStream.Dispose();
                    inputStream.Dispose();
                }
                // Otherwise, just get the info
                else
                {
                    long length = new FileInfo(input).Length;
                    baseFile = GetStreamInfo(TryOpenRead(input), length, omitFromScan, false, chdsAsFiles);
                }
            }
            else
            {
                long length = new FileInfo(input).Length;
                baseFile = GetStreamInfo(TryOpenRead(input), length, omitFromScan, false, chdsAsFiles);
            }

            // Add unique data from the file
            baseFile.Filename = Path.GetFileName(input);
            baseFile.Date = (date ? new FileInfo(input).LastWriteTime.ToString("yyyy/MM/dd HH:mm:ss") : string.Empty);

            return baseFile;
        }

        /// <summary>
        /// Returns the file type of an input file
        /// </summary>
        /// <param name="input">Input file to check</param>
        /// <returns>FileType of inputted file (null on error)</returns>
        public static FileType? GetFileType(string input)
        {
            FileType? outFileType = null;

            // If the file is null, then we have no archive type
            if (input == null)
                return outFileType;

            // First line of defense is going to be the extension, for better or worse
            if (!HasValidArchiveExtension(input))
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

            FileType? fileType = GetFileType(input);
            switch (fileType)
            {				
                case FileType.GZipArchive:
                    shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.GZipExternal) != 0);
                    shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.GZipInternal) != 0);
                    break;
                case FileType.RarArchive:
                    shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.RarExternal) != 0);
                    shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.RarInternal) != 0);
                    break;
                case FileType.SevenZipArchive:
                    shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.SevenZipExternal) != 0);
                    shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.SevenZipInternal) != 0);
                    break;
                case FileType.ZipArchive:
                    shouldExternalProcess = ((archiveScanLevel & ArchiveScanLevel.ZipExternal) != 0);
                    shouldInternalProcess = ((archiveScanLevel & ArchiveScanLevel.ZipInternal) != 0);
                    break;
                case null:
                default:
                    shouldExternalProcess = true;
                    shouldInternalProcess = false;
                    break;
            }
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

        /// <summary>
        /// Retrieve a list of directories from a directory recursively in proper order
        /// </summary>
        /// <param name="directory">Directory to parse</param>
        /// <param name="infiles">List representing existing files</param>
        /// <returns>List with all new files</returns>
        public static List<string> RetrieveDirectories(string directory, List<string> infiles)
        {
            // Take care of the files in the top directory
            List<string> toadd = Directory.EnumerateDirectories(directory, "*", SearchOption.TopDirectoryOnly).ToList();
            toadd.Sort(new NaturalComparer());
            infiles.AddRange(toadd);

            // Then recurse through and add from the directories
            foreach (string dir in toadd)
            {
                infiles = RetrieveDirectories(dir, infiles);
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
                return;

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
            EnsureOutputDirectory(outDir, create: true);

            Globals.Logger.User($"\nGetting skipper information for '{file}'");

            // Get the skipper rule that matches the file, if any
            SkipperRule rule = Skipper.GetMatchingRule(file, string.Empty);

            // If we have an empty rule, return false
            if (rule.Tests == null || rule.Tests.Count == 0 || rule.Operation != HeaderSkipOperation.None)
                return false;

            Globals.Logger.User("File has a valid copier header");

            // Get the header bytes from the file first
            string hstr = string.Empty;
            BinaryReader br = new BinaryReader(TryOpenRead(file));

            // Extract the header as a string for the database
            byte[] hbin = br.ReadBytes((int)rule.StartOffset);
            hstr = ByteArrayToString(hbin);
            br.Dispose();

            // Apply the rule to the file
            string newfile = (string.IsNullOrWhiteSpace(outDir) ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file)));
            rule.TransformFile(file, newfile);

            // If the output file doesn't exist, return false
            if (!File.Exists(newfile))
                return false;

            // Now add the information to the database if it's not already there
            if (!nostore)
            {
                BaseFile baseFile = GetFileInfo(newfile, chdsAsFiles: true);
                DatabaseTools.AddHeaderToDatabase(hstr, ByteArrayToString(baseFile.SHA1), rule.SourceFile);
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
                    List<string> files = RetrieveFiles(input, new List<string>());
                    foreach (string file in files)
                    {
                        try
                        {
                            outputs.Add(Path.GetFullPath(file) + (appendparent ? $"¬{Path.GetFullPath(input)}" : string.Empty));
                        }
                        catch (PathTooLongException)
                        {
                            Globals.Logger.Warning($"The path for '{file}' was too long");
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
                        outputs.Add(Path.GetFullPath(input) + (appendparent ? $"¬{Path.GetFullPath(input)}" : string.Empty));
                    }
                    catch (PathTooLongException)
                    {
                        Globals.Logger.Warning($"The path for '{input}' was too long");
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
        /// Retrieve a list of just directories from inputs
        /// </summary>
        /// <param name="inputs">List of strings representing directories and files</param>
        /// <param name="appendparent">True if the parent name should be appended after the special character "¬", false otherwise (default)</param>
        /// <returns>List of strings representing just directories from the inputs</returns>
        public static List<string> GetOnlyDirectoriesFromInputs(List<string> inputs, bool appendparent = false)
        {
            List<string> outputs = new List<string>();
            foreach (string input in inputs)
            {
                if (Directory.Exists(input))
                {
                    List<string> directories = RetrieveDirectories(input, new List<string>());
                    foreach (string dir in directories)
                    {
                        try
                        {
                            outputs.Add(Path.GetFullPath(dir) + (appendparent ? $"¬{Path.GetFullPath(input)}" : string.Empty));
                        }
                        catch (PathTooLongException)
                        {
                            Globals.Logger.Warning($"The path for '{dir}' was too long");
                        }
                        catch (Exception ex)
                        {
                            Globals.Logger.Error(ex.ToString());
                        }
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
        /// Detect and replace header(s) to the given file
        /// </summary>
        /// <param name="file">Name of the file to be parsed</param>
        /// <param name="outDir">Output directory to write the file to, empty means the same directory as the input file</param>
        /// <returns>True if a header was found and appended, false otherwise</returns>
        public static bool RestoreHeader(string file, string outDir)
        {
            // Create the output directory if it doesn't exist
            if (!string.IsNullOrWhiteSpace(outDir) && !Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            // First, get the SHA-1 hash of the file
            BaseFile baseFile = GetFileInfo(file, chdsAsFiles: true);

            // Retrieve a list of all related headers from the database
            List<string> headers = DatabaseTools.RetrieveHeadersFromDatabase(ByteArrayToString(baseFile.SHA1));

            // If we have nothing retrieved, we return false
            if (headers.Count == 0)
                return false;

            // Now loop through and create the reheadered files, if possible
            for (int i = 0; i < headers.Count; i++)
            {
                string outputFile = (string.IsNullOrWhiteSpace(outDir) ? $"{Path.GetFullPath(file)}.new" : Path.Combine(outDir, Path.GetFileName(file))) + i;
                Globals.Logger.User($"Creating reheadered file: {outputFile}");
                AppendBytesToFile(file, outputFile, headers[i], string.Empty);
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
                    throw ex;
                else
                    return null;
            }
        }

        /// <summary>
        /// Try to safely delete a directory, optionally throwing the error
        /// </summary>
        /// <param name="file">Name of the directory to delete</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the file didn't exist or could be deleted, false otherwise</returns>
        public static bool TryCreateDirectory(string file, bool throwOnError = false)
        {
            // Now wrap creating the directory
            try
            {
                Directory.CreateDirectory(file);
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
        /// Try to safely delete a directory, optionally throwing the error
        /// </summary>
        /// <param name="file">Name of the directory to delete</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the file didn't exist or could be deleted, false otherwise</returns>
        public static bool TryDeleteDirectory(string file, bool throwOnError = false)
        {
            // Check if the directory exists first
            if (!Directory.Exists(file))
                return true;

            // Now wrap deleting the directory
            try
            {
                Directory.Delete(file, true);
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
        /// Try to safely delete a file, optionally throwing the error
        /// </summary>
        /// <param name="file">Name of the file to delete</param>
        /// <param name="throwOnError">True if the error that is thrown should be thrown back to the caller, false otherwise</param>
        /// <returns>True if the file didn't exist or could be deleted, false otherwise</returns>
        public static bool TryDeleteFile(string file, bool throwOnError = false)
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

        #endregion

        #region Stream Information

        /// <summary>
        /// Retrieve file information for a single file
        /// </summary>
        /// <param name="input">Filename to get information from</param>
        /// <param name="size">Size of the input stream</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
        /// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <returns>Populated BaseFile object if success, empty one on error</returns>
        public static BaseFile GetStreamInfo(Stream input, long size, Hash omitFromScan = 0x0, bool keepReadOpen = false, bool chdsAsFiles = true)
        {
            return GetStreamInfoAsync(input, size, omitFromScan, keepReadOpen, chdsAsFiles).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieve file information for a single file
        /// </summary>
        /// <param name="input">Filename to get information from</param>
        /// <param name="size">Size of the input stream</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
        /// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <returns>Populated BaseFile object if success, empty one on error</returns>
        public static async Task<BaseFile> GetStreamInfoAsync(Stream input, long size, Hash omitFromScan = 0x0, bool keepReadOpen = false, bool chdsAsFiles = true)
        {
            // We first check to see if it's a CHD if we have to
            if (!chdsAsFiles)
            {
                BaseFile chd = GetCHDInfo(input);
                SeekIfPossible(input);

                // If we found a valid CHD
                if (chd != null)
                {
                    if (!keepReadOpen)
                        input.Dispose();

                    return chd;
                }
            }

            BaseFile rom = new BaseFile()
            {
                Size = size,
            };

            try
            {
                // Get a list of hashers to run over the buffer
                List<Hasher> hashers = new List<Hasher>()
                {
                    new Hasher(Hash.CRC),
                    new Hasher(Hash.MD5),
#if NET_FRAMEWORK
                    new Hasher(Hash.RIPEMD160),
#endif
                    new Hasher(Hash.SHA1),
                    new Hasher(Hash.SHA256),
                    new Hasher(Hash.SHA384),
                    new Hasher(Hash.SHA512),
                };

                // Initialize the hashing helpers
                var loadBuffer = new ThreadLoadBuffer(input);
                int buffersize = 3 * 1024 * 1024;
                byte[] buffer0 = new byte[buffersize];
                byte[] buffer1 = new byte[buffersize];

                /*
                Please note that some of the following code is adapted from
                RomVault. This is a modified version of how RomVault does
                threaded hashing. As such, some of the terminology and code
                is the same, though variable names and comments may have
                been tweaked to better fit this code base.
                */

                // Pre load the first buffer
                int next = size > buffersize ? buffersize : (int)size;
                input.Read(buffer0, 0, next);
                int current = next;
                size -= next;
                bool bufferSelect = true;

                while (current > 0)
                {
                    // Trigger the buffer load on the second buffer
                    next = size > buffersize ? buffersize : (int)size;
                    if (next > 0)
                        loadBuffer.Trigger(bufferSelect ? buffer1 : buffer0, next);

                    byte[] buffer = bufferSelect ? buffer0 : buffer1;

                    // Run hashes in parallel
                    await Task.WhenAll(hashers.Select(h => h.Process(buffer, current)));

                    // Wait for the load buffer worker, if needed
                    if (next > 0)
                        loadBuffer.Wait();

                    // Setup for the next hashing step
                    current = next;
                    size -= next;
                    bufferSelect = !bufferSelect;
                }

                // Finalize all hashing helpers
                loadBuffer.Finish();
                await Task.WhenAll(hashers.Select(h => h.Finalize()));

                // Get the results
                if ((omitFromScan & Hash.CRC) == 0)
                    rom.CRC = hashers.First(h => h.HashType == Hash.CRC).GetHash();
                if ((omitFromScan & Hash.MD5) == 0)
                    rom.MD5 = hashers.First(h => h.HashType == Hash.MD5).GetHash();
#if NET_FRAMEWORK
                if ((omitFromScan & Hash.RIPEMD160) == 0)
                    rom.RIPEMD160 = hashers.First(h => h.HashType == Hash.RIPEMD160).GetHash();
#endif
                if ((omitFromScan & Hash.SHA1) == 0)
                    rom.SHA1 = hashers.First(h => h.HashType == Hash.SHA1).GetHash();
                if ((omitFromScan & Hash.SHA256) == 0)
                    rom.SHA256 = hashers.First(h => h.HashType == Hash.SHA256).GetHash();
                if ((omitFromScan & Hash.SHA384) == 0)
                    rom.SHA384 = hashers.First(h => h.HashType == Hash.SHA384).GetHash();
                if ((omitFromScan & Hash.SHA512) == 0)
                    rom.SHA512 = hashers.First(h => h.HashType == Hash.SHA512).GetHash();

                // Dispose of the hashers
                loadBuffer.Dispose();
                hashers.ForEach(h => h.Dispose());
            }
            catch (IOException)
            {
                return new BaseFile();
            }
            finally
            {
                if (!keepReadOpen)
                    input.Dispose();
                else
                    SeekIfPossible(input);
            }

            return rom;
        }

        /// <summary>
        /// Get internal metadata from a CHD
        /// </summary>
        /// <param name="input">Stream of possible CHD</param>
        /// <returns>A CHDFile object with internal MD5 (v1, v2) or SHA-1 (v3, v4, v5) on success, null otherwise</returns>
        /// <remarks>
        /// Original code had a "writable" param. This is not required for metadata checking
        /// </remarks>
        public static BaseFile GetCHDInfo(Stream input)
        {
            CHDFile chd = new CHDFile(input);
            return (chd.Version != null && (chd.MD5 != null || chd.SHA1 != null) ? chd : null);
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
                bw.Write(bytesToAddToHead);

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
                bw.Write(bytesToAddToTail);
        }

        /// <summary>
        /// Seek to a specific point in the stream, if possible
        /// </summary>
        /// <param name="input">Input stream to try seeking on</param>
        /// <param name="offset">Optional offset to seek to</param>
        public static void SeekIfPossible(Stream input, long offset = 0)
        {
            if (input.CanSeek)
            {
                try
                {
                    if (offset < 0)
                        input.Seek(offset, SeekOrigin.End);
                    else if (offset >= 0)
                        input.Seek(offset, SeekOrigin.Begin);
                }
                catch (NotSupportedException)
                {
                    Globals.Logger.Verbose("Stream does not support seeking to starting offset. Stream position not changed");
                }
                catch (NotImplementedException)
                {
                    Globals.Logger.Warning("Stream does not support seeking to starting offset. Stream position not changed");
                }
            }
        }

        #endregion

        #region String Manipulation

        /// <summary>
        /// Compare strings as numeric
        /// </summary>
        /// <param name="s1">First string to compare</param>
        /// <param name="s2">Second string to compare</param>
        /// <returns>-1 if s1 comes before s2, 0 if s1 and s2 are equal, 1 if s1 comes after s2</returns>
        /// <remarks>I want to be able to handle paths properly with no issue, can I do a recursive call based on separated by path separator?</remarks>
        public static int CompareNumeric(string s1, string s2)
        {
            // Save the orginal strings, for later comparison
            string s1orig = s1;
            string s2orig = s2;

            // We want to normalize the strings, so we set both to lower case
            s1 = s1.ToLowerInvariant();
            s2 = s2.ToLowerInvariant();

            // If the strings are the same exactly, return
            if (s1 == s2)
                return s1orig.CompareTo(s2orig);

            // If one is null, then say that's less than
            if (s1 == null)
                return -1;
            if (s2 == null)
                return 1;

            // Now split into path parts after converting AltDirSeparator to DirSeparator
            s1 = s1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            s2 = s2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            string[] s1parts = s1.Split(Path.DirectorySeparatorChar);
            string[] s2parts = s2.Split(Path.DirectorySeparatorChar);

            // Then compare each part in turn
            for (int j = 0; j < s1parts.Length && j < s2parts.Length; j++)
            {
                int compared = CompareNumericPart(s1parts[j], s2parts[j]);
                if (compared != 0)
                    return compared;
            }

            // If we got out here, then it looped through at least one of the strings
            if (s1parts.Length > s2parts.Length)
                return 1;
            if (s1parts.Length < s2parts.Length)
                return -1;

            return s1orig.CompareTo(s2orig);
        }

        /// <summary>
        /// Helper for CompareNumeric
        /// </summary>
        /// <param name="s1">First string to compare</param>
        /// <param name="s2">Second string to compare</param>
        /// <returns>-1 if s1 comes before s2, 0 if s1 and s2 are equal, 1 if s1 comes after s2</returns>
        private static int CompareNumericPart(string s1, string s2)
        {
            // Otherwise, loop through until we have an answer
            for (int i = 0; i < s1.Length && i < s2.Length; i++)
            {
                int s1c = s1[i];
                int s2c = s2[i];

                // If the characters are the same, continue
                if (s1c == s2c)
                    continue;

                // If they're different, check which one was larger
                if (s1c > s2c)
                    return 1;
                if (s1c < s2c)
                    return -1;
            }

            // If we got out here, then it looped through at least one of the strings
            if (s1.Length > s2.Length)
                return 1;
            if (s1.Length < s2.Length)
                return -1;

            return 0;
        }

        /// <summary>
        /// Convert all characters that are not considered XML-safe
        /// </summary>
        /// <param name="s">Input string to clean</param>
        /// <returns>Cleaned string</returns>
        public static string ConvertXMLUnsafeCharacters(string s)
        {
            return new String(s.Select(c =>
                (c == 0x9
                    || c == 0xA
                    || c == 0xD
                    || (c >= 0x20 && c <= 0xD77F)
                    || (c >= 0xE000 && c <= 0xFFFD)
                    || (c >= 0x10000 && c <= 0x10FFFF)
                        ? c
                        : WebUtility.HtmlEncode(c.ToString())[0]))
                .ToArray());
        }

        /// <summary>
        /// Ensure the output directory is a proper format and can be created
        /// </summary>
        /// <param name="outDir">Output directory to check</param>
        /// <param name="create">True if the output directory should be created, false otherwise (default)</param>
        /// <returns>Full path to the proper output directory</returns>
        public static string EnsureOutputDirectory(string outDir, bool create = false)
        {
            // If the output directory is invalid
            if (string.IsNullOrWhiteSpace(outDir))
                outDir = Environment.CurrentDirectory;

            // Get the full path for the output directory
            outDir = Path.GetFullPath(outDir);

            // If we're creating the output folder, do so
            if (create)
                Directory.CreateDirectory(outDir);

            return outDir;
        }

        /// <summary>
        /// Ensure the temp directory is a proper format and can be created
        /// </summary>
        /// <param name="tempDir">Temp directory to check</param>
        /// <param name="create">True if the temp directory should be created, false otherwise (default)</param>
        /// <returns>Full path to the proper temp directory</returns>
        public static string EnsureTempDirectory(string tempDir, bool create = false)
        {
            // If the output directory is invalid
            if (string.IsNullOrWhiteSpace(tempDir))
                tempDir = Path.GetTempPath();

            // Get the full path for the output directory
            tempDir = Path.GetFullPath(tempDir);

            // If we're creating the output folder, do so
            if (create)
                Directory.CreateDirectory(tempDir);

            return tempDir;
        }

        /// <summary>
        /// Get the extension from the path, if possible
        /// </summary>
        /// <param name="path">Path to get extension from</param>
        /// <returns>Extension, if possible</returns>
        public static string GetExtension(string path)
        {
            // Check null or empty first
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // Get the extension from the path, if possible
            string ext = Path.GetExtension(path)?.ToLowerInvariant();

            // Check if the extension is null or empty
            if (string.IsNullOrWhiteSpace(ext))
                return null;

            // Make sure that extensions are valid
            ext = ext.TrimStart('.');

            return ext;
        }

        /// <summary>
        /// Get the proper filename (with subpath) from the file and parent combination
        /// </summary>
        /// <param name="path">Input combined path to use</param>
        /// <param name="sanitize">True if path separators should be converted to '-', false otherwise</param>
        /// <returns>Subpath for the file</returns>
        public static string GetFilenameFromFileAndParent(string path, bool sanitize)
        {
            // Check that we have a combined path first
            if (!path.Contains("¬"))
            {
                string filename = Path.GetFileName(path);
                if (sanitize)
                    filename.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');

                return filename;
            }

            // First separate out the parts
            string child = path.Split('¬')[0];
            string parent = path.Split('¬')[1];

            // If the parts are the same, return the filename from the first part
            if (string.Equals(child, parent, StringComparison.Ordinal))
            {
                string filename = Path.GetFileName(child);
                if (sanitize)
                    filename.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');

                return filename;
            }

            // Otherwise, remove the parent from the child and return the remainder
            else
            {
                string filename = child.Remove(0, parent.Length + 1);
                if (sanitize)
                    filename.Replace(Path.DirectorySeparatorChar, '-').Replace(Path.AltDirectorySeparatorChar, '-');

                return filename;
            }
        }

        /// <summary>
        /// Get the dictionary key that should be used for a given item and sorting type
        /// </summary>
        /// <param name="item">DatItem to get the key for</param>
        /// <param name="sortedBy">SortedBy enum representing what key to get</param>
        /// <param name="lower">True if the key should be lowercased (default), false otherwise</param>
        /// <param name="norename">True if games should only be compared on game and file name, false if system and source are counted</param>
        /// <returns>String representing the key to be used for the DatItem</returns>
        public static string GetKeyFromDatItem(DatItem item, SortedBy sortedBy, bool lower = true, bool norename = true)
        {
            // Set the output key as the default blank string
            string key = string.Empty;

            // Now determine what the key should be based on the sortedBy value
            switch (sortedBy)
            {
                case SortedBy.CRC:
                    key = (item.ItemType == ItemType.Rom ? ((Rom)item).CRC : Constants.CRCZero);
                    break;

                case SortedBy.Game:
                    key = (norename ? string.Empty
                        : item.SystemID.ToString().PadLeft(10, '0')
                            + "-"
                            + item.SourceID.ToString().PadLeft(10, '0') + "-")
                    + (string.IsNullOrWhiteSpace(item.MachineName)
                            ? "Default"
                            : item.MachineName);
                    if (lower)
                        key = key.ToLowerInvariant();

                    if (key == null)
                        key = "null";

                    key = WebUtility.HtmlEncode(key);
                    break;

                case SortedBy.MD5:
                    key = (item.ItemType == ItemType.Rom
                        ? ((Rom)item).MD5
                        : (item.ItemType == ItemType.Disk
                            ? ((Disk)item).MD5
                            : Constants.MD5Zero));
                    break;

                case SortedBy.RIPEMD160:
                    key = (item.ItemType == ItemType.Rom
                        ? ((Rom)item).RIPEMD160
                        : (item.ItemType == ItemType.Disk
                            ? ((Disk)item).RIPEMD160
                            : Constants.RIPEMD160Zero));
                    break;

                case SortedBy.SHA1:
                    key = (item.ItemType == ItemType.Rom
                        ? ((Rom)item).SHA1
                        : (item.ItemType == ItemType.Disk
                            ? ((Disk)item).SHA1
                            : Constants.SHA1Zero));
                    break;

                case SortedBy.SHA256:
                    key = (item.ItemType == ItemType.Rom
                        ? ((Rom)item).SHA256
                        : (item.ItemType == ItemType.Disk
                            ? ((Disk)item).SHA256
                            : Constants.SHA256Zero));
                    break;

                case SortedBy.SHA384:
                    key = (item.ItemType == ItemType.Rom
                        ? ((Rom)item).SHA384
                        : (item.ItemType == ItemType.Disk
                            ? ((Disk)item).SHA384
                            : Constants.SHA384Zero));
                    break;

                case SortedBy.SHA512:
                    key = (item.ItemType == ItemType.Rom
                        ? ((Rom)item).SHA512
                        : (item.ItemType == ItemType.Disk
                            ? ((Disk)item).SHA512
                            : Constants.SHA512Zero));
                    break;
            }

            // Double and triple check the key for corner cases
            if (key == null)
                key = string.Empty;

            return key;
        }

        /// <summary>
        /// Get the proper output path for a given input file and output directory
        /// </summary>
        /// <param name="outDir">Output directory to use</param>
        /// <param name="inputpath">Input path to create output for</param>
        /// <param name="inplace">True if the output file should go to the same input folder, false otherwise</param>
        /// <returns>Complete output path</returns>
        public static string GetOutputPath(string outDir, string inputpath, bool inplace)
        {
            // First, we need to ensure the output directory
            outDir = EnsureOutputDirectory(outDir);

            // Check if we have a split path or not
            bool splitpath = inputpath.Contains("¬");

            // If we have a split path, we need to treat the input separately
            if (splitpath)
            {
                string[] split = inputpath.Split('¬');

                // If we have an inplace output, use the directory name from the input path
                if (inplace)
                {
                    outDir = Path.GetDirectoryName(split[0]);
                }

                // TODO: Should this be the default? Always create a subfolder if a folder is found?
                // If we are processing a path that is coming from a directory and we are outputting to the current directory, we want to get the subfolder to write to
                else if (split[0].Length != split[1].Length && outDir == Environment.CurrentDirectory)
                {
                    outDir = Path.GetDirectoryName(Path.Combine(outDir, split[0].Remove(0, Path.GetDirectoryName(split[1]).Length + 1)));
                }

                // If we are processing a path that is coming from a directory, we want to get the subfolder to write to
                else if (split[0].Length != split[1].Length)
                {
                    outDir = Path.GetDirectoryName(Path.Combine(outDir, split[0].Remove(0, split[1].Length + 1)));
                }

                // If we are processing a single file from the root of a directory, we just use the output directory
                else
                {
                    // No-op
                }
            }
            // Otherwise, assume the input path is just a filename
            else
            {
                // If we have an inplace output, use the directory name from the input path
                if (inplace)
                {
                    outDir = Path.GetDirectoryName(inputpath);
                }

                // Otherwise, just use the supplied output directory
                else
                {
                    // No-op
                }
            }

            // Finally, return the output directory
            return outDir;
        }

        /// <summary>
        /// Get a proper romba sub path
        /// </summary>
        /// <param name="hash">SHA-1 hash to get the path for</param>
        /// <returns>Subfolder path for the given hash</returns>
        public static string GetRombaPath(string hash)
        {
            // If the hash isn't the right size, then we return null
            if (hash.Length != Constants.SHA1Length) // TODO: When updating to SHA-256, this needs to update to Constants.SHA256Length
                return null;

            return Path.Combine(hash.Substring(0, 2), hash.Substring(2, 2), hash.Substring(4, 2), hash.Substring(6, 2), hash + ".gz");
        }

        /// <summary>
        /// Get the multiplier to be used with the size given
        /// </summary>
        /// <param name="sizestring">String with possible size with extension</param>
        /// <returns>Tuple of multiplier to use on final size and fixed size string</returns>
        public static long GetSizeFromString(string sizestring)
        {
            // If the string is null or empty, we return -1
            if (string.IsNullOrWhiteSpace(sizestring))
                return -1;

            // Make sure the string is in lower case
            sizestring = sizestring.ToLowerInvariant();

            // Get any trailing size identifiers
            long multiplier = 1;
            if (sizestring.EndsWith("k") || sizestring.EndsWith("kb"))
                multiplier = Constants.KiloByte;
            else if (sizestring.EndsWith("ki") || sizestring.EndsWith("kib"))
                multiplier = Constants.KibiByte;
            else if (sizestring.EndsWith("m") || sizestring.EndsWith("mb"))
                multiplier = Constants.MegaByte;
            else if (sizestring.EndsWith("mi") || sizestring.EndsWith("mib"))
                multiplier = Constants.MibiByte;
            else if (sizestring.EndsWith("g") || sizestring.EndsWith("gb"))
                multiplier = Constants.GigaByte;
            else if (sizestring.EndsWith("gi") || sizestring.EndsWith("gib"))
                multiplier = Constants.GibiByte;
            else if (sizestring.EndsWith("t") || sizestring.EndsWith("tb"))
                multiplier = Constants.TeraByte;
            else if (sizestring.EndsWith("ti") || sizestring.EndsWith("tib"))
                multiplier = Constants.TibiByte;
            else if (sizestring.EndsWith("p") || sizestring.EndsWith("pb"))
                multiplier = Constants.PetaByte;
            else if (sizestring.EndsWith("pi") || sizestring.EndsWith("pib"))
                multiplier = Constants.PibiByte;

            // Remove any trailing identifiers
            sizestring = sizestring.TrimEnd(new char[] { 'k', 'm', 'g', 't', 'p', 'i', 'b', ' ' });

            // Now try to get the size from the string
            if (!Int64.TryParse(sizestring, out long size))
                size = -1;
            else
                size *= multiplier;

            return size;
        }

        /// <summary>
        /// Get if the given path has a valid DAT extension
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the extension is valid, false otherwise</returns>
        public static bool HasValidArchiveExtension(string path)
        {
            // Get the extension from the path, if possible
            string ext = GetExtension(path);

            // Check against the list of known archive extensions
            switch (ext)
            {
                case "7z":
                case "gz":
                case "lzma":
                case "rar":
                case "rev":
                case "r00":
                case "r01":
                case "tar":
                case "tgz":
                case "tlz":
                case "zip":
                case "zipx":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get if the given path has a valid DAT extension
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the extension is valid, false otherwise</returns>
        public static bool HasValidDatExtension(string path)
        {
            // Get the extension from the path, if possible
            string ext = GetExtension(path);

            // Check against the list of known DAT extensions
            switch (ext)
            {
                case "csv":
                case "dat":
                case "md5":
                case "ripemd160":
                case "sfv":
                case "sha1":
                case "sha256":
                case "sha384":
                case "sha512":
                case "ssv":
                case "tsv":
                case "txt":
                case "xml":
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Get if a string contains Unicode characters
        /// </summary>
        /// <param name="s">Input string to test</param>
        /// <returns>True if the string contains at least one Unicode character, false otherwise</returns>
        public static bool IsUnicode(string s)
        {
            return (s.Any(c => c > 255));
        }

        /// <summary>
        /// Remove all chars that are considered path unsafe
        /// </summary>
        /// <param name="s">Input string to clean</param>
        /// <returns>Cleaned string</returns>
        public static string RemovePathUnsafeCharacters(string s)
        {
            List<char> invalidPath = Path.GetInvalidPathChars().ToList();
            return new string(s.Where(c => !invalidPath.Contains(c)).ToArray());
        }

        /// <summary>
        /// Remove all unicode-specific chars from a string
        /// </summary>
        /// <param name="s">Input string to clean</param>
        /// <returns>Cleaned string</returns>
        public static string RemoveUnicodeCharacters(string s)
        {
            return new string(s.Where(c => c <= 255).ToArray());
        }

        /// <summary>
        /// Split a line as if it were a CMP rom line
        /// </summary>
        /// <param name="s">Line to split</param>
        /// <returns>Line split</returns>
        /// <remarks>Uses code from http://stackoverflow.com/questions/554013/regular-expression-to-split-on-spaces-unless-in-quotes</remarks>
        public static string[] SplitLineAsCMP(string s)
        {
            // Get the opening and closing brace locations
            int openParenLoc = s.IndexOf('(');
            int closeParenLoc = s.LastIndexOf(')');

            // Now remove anything outside of those braces, including the braces
            s = s.Substring(openParenLoc + 1, closeParenLoc - openParenLoc - 1);
            s = s.Trim();

            // Now we get each string, divided up as cleanly as possible
            string[] matches = Regex
                //.Matches(s, @"([^\s]*string.Empty[^string.Empty]+string.Empty[^\s]*)|[^string.Empty]?\w+[^string.Empty]?")
                .Matches(s, @"[^\sstring.Empty]+|string.Empty[^string.Empty]*string.Empty")
                .Cast<Match>()
                .Select(m => m.Groups[0].Value)
                .ToArray();

            return matches;
        }

        #endregion

        #region Miscellaneous / Externally Sourced

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
        /// Returns the human-readable file size for an arbitrary, 64-bit file size 
        /// The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Human-readable file size</returns>
        /// <link>http://www.somacon.com/p576.php</link>
        public static string GetBytesReadable(long input)
        {
            // Get absolute value
            long absolute_i = (input < 0 ? -input : input);

            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (input >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (input >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (input >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (input >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (input >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = input;
            }
            else
            {
                return input.ToString("0 B"); // Byte
            }

            // Divide by 1024 to get fractional value
            readable /= 1024;

            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }

        /// <summary>
        /// Convert a byte array to a hex string
        /// </summary>
        /// <param name="bytes">Byte array to convert</param>
        /// <returns>Hex string representing the byte array</returns>
        /// <link>http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa</link>
        public static string ByteArrayToString(byte[] bytes)
        {
            // If we get null in, we send null out
            if (bytes == null)
                return null;

            try
            {
                string hex = BitConverter.ToString(bytes);
                return hex.Replace("-", string.Empty).ToLowerInvariant();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert a hex string to a byte array
        /// </summary>
        /// <param name="hex">Hex string to convert</param>
        /// <returns>Byte array represenging the hex string</returns>
        /// <link>http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa</link>
        public static byte[] StringToByteArray(string hex)
        {
            // If we get null in, we send null out
            if (hex == null)
                return null;

            try
            {
                int NumberChars = hex.Length;
                byte[] bytes = new byte[NumberChars / 2];
                for (int i = 0; i < NumberChars; i += 2)
                {
                    bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
                }

                return bytes;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convert a hex string to an ASCII one
        /// </summary>
        /// <param name="hexString">Hex string to convert</param>
        /// <returns>ASCII string representing the hex string</returns>
        /// <link>http://stackoverflow.com/questions/5613279/c-sharp-hex-to-ascii</link>
        public static string ConvertHexToAscii(string hexString)
        {
            // If we get null in, we send null out
            if (hexString == null)
                return null;

            if (hexString.Contains("-"))
                hexString = hexString.Replace("-", string.Empty);

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < hexString.Length; i += 2)
            {
                String hs = hexString.Substring(i, 2);
                sb.Append(Convert.ToChar(Convert.ToUInt32(hs, 16)));
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert an ASCII string to a hex one
        /// </summary>
        /// <param name="asciiString">ASCII string to convert</param>
        /// <returns>Hex string representing the ASCII string</returns>
        /// <link>http://stackoverflow.com/questions/15920741/convert-from-string-ascii-to-string-hex</link>
        public static string ConvertAsciiToHex(string asciiString)
        {
            // If we get null in, we send null out
            if (asciiString == null)
                return null;

            string hexOutput = string.Empty;
            foreach (char _eachChar in asciiString.ToCharArray())
            {
                // Get the integral value of the character.
                int value = Convert.ToInt32(_eachChar);

                // Convert the decimal value to a hexadecimal value in string form.
                hexOutput += String.Format("{0:X2}", value).Remove(0, 2);
            }

            return hexOutput;
        }

        /// <summary>
        /// Convert .NET DateTime to MS-DOS date format
        /// </summary>
        /// <param name="dateTime">.NET DateTime object to convert</param>
        /// <returns>UInt32 representing the MS-DOS date</returns>
        /// <remarks>
        /// Adapted from 7-zip Source Code: CPP/Windows/TimeUtils.cpp:FileTimeToDosTime
        /// </remarks>
        public static uint ConvertDateTimeToMsDosTimeFormat(DateTime dateTime)
        {
            uint year = (uint)((dateTime.Year - 1980) % 128);
            uint mon = (uint)dateTime.Month;
            uint day = (uint)dateTime.Day;
            uint hour = (uint)dateTime.Hour;
            uint min = (uint)dateTime.Minute;
            uint sec = (uint)dateTime.Second;

            return (year << 25) | (mon << 21) | (day << 16) | (hour << 11) | (min << 5) | (sec >> 1);
        }

        /// <summary>
        /// Convert MS-DOS date format to .NET DateTime
        /// </summary>
        /// <param name="msDosDateTime">UInt32 representing the MS-DOS date to convert</param>
        /// <returns>.NET DateTime object representing the converted date</returns>
        /// <remarks>
        /// Adapted from 7-zip Source Code: CPP/Windows/TimeUtils.cpp:DosTimeToFileTime
        /// </remarks>
        public static DateTime ConvertMsDosTimeFormatToDateTime(uint msDosDateTime)
        {
            return new DateTime((int)(1980 + (msDosDateTime >> 25)), (int)((msDosDateTime >> 21) & 0xF), (int)((msDosDateTime >> 16) & 0x1F),
                (int)((msDosDateTime >> 11) & 0x1F), (int)((msDosDateTime >> 5) & 0x3F), (int)((msDosDateTime & 0x1F) * 2));
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
            FileStream file = TryOpenRead(filename);
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
        /// Extension method to get the DateTime that an assembly was linked
        /// </summary>
        /// <param name="assembly">Assembly to get linker time from</param>
        /// <param name="target">Target timezone to convert the time to (default null)</param>
        /// <returns>DateTime that the assembly was linked</returns>
        /// <link>http://stackoverflow.com/questions/1600962/displaying-the-build-date</link>
        public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = TryOpenRead(filePath))
                stream.Read(buffer, 0, 2048);

            var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

            var tz = target ?? TimeZoneInfo.Local;
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

            return localTime;
        }

        /// <summary>
        /// Indicates whether the specified array is null or has a length of zero
        /// </summary>
        /// <param name="array">The array to test</param>
        /// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
        /// <link>https://stackoverflow.com/questions/8560106/isnullorempty-equivalent-for-array-c-sharp</link>
        public static bool IsNullOrWhiteSpace(this Array array)
        {
            return (array == null || array.Length == 0);
        }

        /// <summary>
        /// Get the field associated with each hash type
        /// </summary>
        public static Field GetFieldFromHash(Hash hash)
        {
            switch (hash)
            {
                case Hash.CRC:
                    return Field.CRC;
                case Hash.MD5:
                    return Field.MD5;
                case Hash.RIPEMD160:
                    return Field.RIPEMD160;
                case Hash.SHA1:
                    return Field.SHA1;
                case Hash.SHA256:
                    return Field.SHA256;
                case Hash.SHA384:
                    return Field.SHA384;
                case Hash.SHA512:
                    return Field.SHA512;

                default:
                    return Field.NULL;
            }
        }

        #endregion
    }
}
