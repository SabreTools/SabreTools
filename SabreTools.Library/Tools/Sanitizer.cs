using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;

namespace SabreTools.Library.Tools
{
    public static class Sanitizer
    {
        /// <summary>
        /// Get a sanitized Date from an input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>Date as a string, if possible</returns>
        public static string CleanDate(string input)
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
#if NET_FRAMEWORK
            game[game.Length - 1] = CleanGameName(game.Last());
#else
            game[^1] = CleanGameName(game[^1]);
#endif
            string outgame = string.Join(Path.DirectorySeparatorChar.ToString(), game);
            outgame = outgame.TrimStart().TrimEnd();
            return outgame;
        }

        /// <summary>
        /// Clean a CRC32 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        public static string CleanCRC32(string hash)
        {
            return CleanHashData(hash, Constants.CRCLength);
        }

        /// <summary>
        /// Clean a MD5 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        public static string CleanMD5(string hash)
        {
            return CleanHashData(hash, Constants.MD5Length);
        }

#if NET_FRAMEWORK
        /// <summary>
        /// Clean a RIPEMD160 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        public static string CleanRIPEMD160(string hash)
        {
            return CleanHashData(hash, Constants.RIPEMD160Length);
        }
#endif

        /// <summary>
        /// Clean a SHA1 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        public static string CleanSHA1(string hash)
        {
            return CleanHashData(hash, Constants.SHA1Length);
        }

        /// <summary>
        /// Clean a SHA256 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        public static string CleanSHA256(string hash)
        {
            return CleanHashData(hash, Constants.SHA256Length);
        }

        /// <summary>
        /// Clean a SHA384 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        public static string CleanSHA384(string hash)
        {
            return CleanHashData(hash, Constants.SHA384Length);
        }

        /// <summary>
        /// Clean a SHA512 string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <returns>Cleaned string</returns>
        public static string CleanSHA512(string hash)
        {
            return CleanHashData(hash, Constants.SHA512Length);
        }

        /// <summary>
        /// Clean a hash string and pad to the correct size
        /// </summary>
        /// <param name="hash">Hash string to sanitize</param>
        /// <param name="padding">Amount of characters to pad to</param>
        /// <returns>Cleaned string</returns>
        private static string CleanHashData(string hash, int padding)
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
        /// Get a sanitized size from an input string
        /// </summary>
        /// <param name="input">String to get value from</param>
        /// <returns>Size as a long, if possible</returns>
        public static long CleanSize(string input)
        {
            long size = -1;
            if (input != null && input.Contains("0x"))
                size = Convert.ToInt64(input, 16);

            else if (input != null)
                Int64.TryParse(input, out size);

            return size;
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
        /// Replace accented characters
        /// </summary>
        /// <param name="input">String to be parsed</param>
        /// <returns>String with characters replaced</returns>
        private static string NormalizeChars(string input)
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
        private static string RussianToLatin(string input)
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
        private static string SearchPattern(string input)
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

        /// <summary>
        /// Get the multiplier to be used with the size given
        /// </summary>
        /// <param name="sizestring">String with possible size with extension</param>
        /// <returns>Tuple of multiplier to use on final size and fixed size string</returns>
        public static long ToSize(string sizestring)
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
    }
}
