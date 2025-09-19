using System;
using System.IO;
using System.Text.RegularExpressions;
using SabreTools.Hashing;

namespace SabreTools.Core.Tools
{
    public static class TextHelper
    {
        #region Normalization

        /// <summary>
        /// Normalize a string to the WoD standard
        /// </summary>
        public static string? NormalizeCharacters(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            ///Run the name through the filters to make sure that it's correct
            input = NormalizeChars(input!);
            input = RussianToLatin(input);
            input = SearchPattern(input);

            input = new Regex(@"(([[(].*[\)\]] )?([^([]+))").Match(input).Groups[1].Value;
            input = input.TrimStart().TrimEnd();
            return input;
        }

        /// <summary>
        /// Normalize a CRC32 string and pad to the correct size
        /// </summary>
        public static string? NormalizeCRC32(string? hash)
            => NormalizeHashData(hash, Constants.CRCLength);

        /// <summary>
        /// Normalize a MD2 string and pad to the correct size
        /// </summary>
        /// <remarks>MD2 is the same length as MD5</remarks>
        public static string? NormalizeMD2(string? hash)
            => NormalizeHashData(hash, Constants.MD5Length);

        /// <summary>
        /// Normalize a MD4 string and pad to the correct size
        /// </summary>
        /// <remarks>MD4 is the same length as MD5</remarks>
        public static string? NormalizeMD4(string? hash)
            => NormalizeHashData(hash, Constants.MD5Length);

        /// <summary>
        /// Normalize a MD5 string and pad to the correct size
        /// </summary>
        public static string? NormalizeMD5(string? hash)
            => NormalizeHashData(hash, Constants.MD5Length);

        /// <summary>
        /// Normalize a RIPEMD128 string and pad to the correct size
        /// </summary>
        public static string? NormalizeRIPEMD128(string? hash)
            => NormalizeHashData(hash, Constants.MD5Length);

        /// <summary>
        /// Normalize a RIPEMD160 string and pad to the correct size
        /// </summary>
        public static string? NormalizeRIPEMD160(string? hash)
            => NormalizeHashData(hash, Constants.SHA1Length);

        /// <summary>
        /// Normalize a SHA1 string and pad to the correct size
        /// </summary>
        public static string? NormalizeSHA1(string? hash)
            => NormalizeHashData(hash, Constants.SHA1Length);

        /// <summary>
        /// Normalize a SHA256 string and pad to the correct size
        /// </summary>
        public static string? NormalizeSHA256(string? hash)
            => NormalizeHashData(hash, Constants.SHA256Length);

        /// <summary>
        /// Normalize a SHA384 string and pad to the correct size
        /// </summary>
        public static string? NormalizeSHA384(string? hash)
            => NormalizeHashData(hash, Constants.SHA384Length);

        /// <summary>
        /// Normalize a SHA512 string and pad to the correct size
        /// </summary>
        public static string? NormalizeSHA512(string? hash)
            => NormalizeHashData(hash, Constants.SHA512Length);

        /// <summary>
        /// Remove all chars that are considered path unsafe
        /// </summary>
        public static string RemovePathUnsafeCharacters(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            foreach (char invalid in Path.GetInvalidPathChars())
            {
                input = input!.Replace(invalid.ToString(), string.Empty);
            }

            return input!;
        }

        /// <summary>
        /// Remove all Unicode-specific chars from a string
        /// </summary>
        /// <remarks>
        /// "Unicode characters" here means any characters outside of the
        /// Extended ASCII (0x00 to 0xFF) set. This is just a simple
        /// way of filtering out characters that won't work on all
        /// supported platforms.
        /// </remarks>
        public static string RemoveUnicodeCharacters(string? input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return new string(Array.FindAll(input!.ToCharArray(), c => c <= 255));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Replace accented characters
        /// </summary>
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
        /// Normalize a hash string and pad to the correct size
        /// </summary>
        private static string? NormalizeHashData(string? hash, int expectedLength)
        {
            // If we have a known blank hash, return blank
            if (hash == null)
                return null;
            else if (hash == string.Empty || hash == "-" || hash == "_")
                return string.Empty;

            // Check to see if it's a "hex" hash
            hash = hash!.Trim().Replace("0x", string.Empty);

            // If we have a blank hash now, return blank
            if (string.IsNullOrEmpty(hash))
                return string.Empty;

            // If the hash shorter than the required length, pad it
            if (hash.Length < expectedLength)
                hash = hash.PadLeft(expectedLength, '0');

            // If the hash is longer than the required length, it's invalid
            else if (hash.Length > expectedLength)
                return string.Empty;

            // Now normalize the hash
            hash = hash.ToLowerInvariant();

            // Otherwise, make sure that every character is a proper match
            for (int i = 0; i < hash.Length; i++)
            {
                char c = hash[i];
#if NET7_0_OR_GREATER
                if (!char.IsAsciiHexDigit(c))
#else
                if (!c.IsAsciiHexDigit())
#endif
                {
                    hash = string.Empty;
                    break;
                }
            }

            return hash;
        }

        /// <summary>
        /// Convert Cyrillic lettering to Latin lettering
        /// </summary>
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

        #endregion
    }
}