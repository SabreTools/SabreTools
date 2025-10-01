using System;

namespace SabreTools.Core.Tools
{
    public static class NumberHelper
    {
        #region Constants

        #region Byte (1000-based) size comparisons

        private const long KiloByte = 1000;
        private readonly static long MegaByte = (long)Math.Pow(KiloByte, 2);
        private readonly static long GigaByte = (long)Math.Pow(KiloByte, 3);
        private readonly static long TeraByte = (long)Math.Pow(KiloByte, 4);
        private readonly static long PetaByte = (long)Math.Pow(KiloByte, 5);

        // The following are too big to be represented in Int64
        // private readonly static long ExaByte = (long)Math.Pow(KiloByte, 6);
        // private readonly static long ZettaByte = (long)Math.Pow(KiloByte, 7);
        // private readonly static long YottaByte = (long)Math.Pow(KiloByte, 8);

        #endregion

        #region Byte (1024-based) size comparisons

        private const long KibiByte = 1024;
        private readonly static long MibiByte = (long)Math.Pow(KibiByte, 2);
        private readonly static long GibiByte = (long)Math.Pow(KibiByte, 3);
        private readonly static long TibiByte = (long)Math.Pow(KibiByte, 4);
        private readonly static long PibiByte = (long)Math.Pow(KibiByte, 5);

        // The following are too big to be represented in Int64
        // private readonly static long ExiByte = (long)Math.Pow(KibiByte, 6);
        // private readonly static long ZittiByte = (long)Math.Pow(KibiByte, 7);
        // private readonly static long YittiByte = (long)Math.Pow(KibiByte, 8);

        #endregion

        #endregion

        /// <summary>
        /// Convert a string to a Double
        /// </summary>
        public static double? ConvertToDouble(string? numeric)
        {
            // If we don't have a valid string, we can't do anything
            if (string.IsNullOrEmpty(numeric))
                return null;

            if (!double.TryParse(numeric, out double doubleValue))
                return null;

            return doubleValue;
        }

        /// <summary>
        /// Convert a string to an Int64
        /// </summary>
        public static long? ConvertToInt64(string? numeric)
        {
            // If we don't have a valid string, we can't do anything
            if (string.IsNullOrEmpty(numeric))
                return null;

            // Normalize the string for easier comparison
            numeric = numeric!.ToLowerInvariant();

            // Parse the numeric string, if possible
            if (numeric.StartsWith("0x"))
            {
                return Convert.ToInt64(numeric.Substring(2), 16);
            }
            else
            {
                // Get the multiplication modifier and trim characters
                long multiplier = DetermineMultiplier(numeric);
                numeric = numeric.TrimEnd(['k', 'm', 'g', 't', 'p', 'e', 'z', 'y', 'i', 'b', ' ']);

                // Apply the multiplier and return
                if (!long.TryParse(numeric, out long longValue))
                    return null;

                return longValue * multiplier;
            }
        }

        /// <summary>
        /// Determine the multiplier from a numeric string
        /// </summary>
        public static long DetermineMultiplier(string? numeric)
        {
            if (string.IsNullOrEmpty(numeric))
                return 0;

            long multiplier = 1;
            if (numeric!.EndsWith("k") || numeric.EndsWith("kb"))
                multiplier = KiloByte;
            else if (numeric.EndsWith("ki") || numeric.EndsWith("kib"))
                multiplier = KibiByte;
            else if (numeric.EndsWith("m") || numeric.EndsWith("mb"))
                multiplier = MegaByte;
            else if (numeric.EndsWith("mi") || numeric.EndsWith("mib"))
                multiplier = MibiByte;
            else if (numeric.EndsWith("g") || numeric.EndsWith("gb"))
                multiplier = GigaByte;
            else if (numeric.EndsWith("gi") || numeric.EndsWith("gib"))
                multiplier = GibiByte;
            else if (numeric.EndsWith("t") || numeric.EndsWith("tb"))
                multiplier = TeraByte;
            else if (numeric.EndsWith("ti") || numeric.EndsWith("tib"))
                multiplier = TibiByte;
            else if (numeric.EndsWith("p") || numeric.EndsWith("pb"))
                multiplier = PetaByte;
            else if (numeric.EndsWith("pi") || numeric.EndsWith("pib"))
                multiplier = PibiByte;

            // The following are too big to be represented in Int64
            // else if (numeric.EndsWith("e") || numeric.EndsWith("eb"))
            //     multiplier = ExaByte;
            // else if (numeric.EndsWith("ei") || numeric.EndsWith("eib"))
            //     multiplier = ExiByte;
            // else if (numeric.EndsWith("z") || numeric.EndsWith("zb"))
            //     multiplier = ZettaByte;
            // else if (numeric.EndsWith("zi") || numeric.EndsWith("zib"))
            //     multiplier = ZittiByte;
            // else if (numeric.EndsWith("y") || numeric.EndsWith("yb"))
            //     multiplier = YottaByte;
            // else if (numeric.EndsWith("yi") || numeric.EndsWith("yib"))
            //     multiplier = YittiByte;

            return multiplier;
        }

        /// <summary>
        /// Determine if a string is fully numeric or not
        /// </summary>
        public static bool IsNumeric(string? value)
        {
            // If we have no value, it is not numeric
            if (string.IsNullOrEmpty(value))
                return false;

            // If we have a hex value
            value = value!.ToLowerInvariant();
            if (value.StartsWith("0x"))
                value = value.Substring(2);

            // If we have a negative value
            if (value.StartsWith("-"))
                value = value.Substring(1);

            // If the value has a multiplier
            if (DetermineMultiplier(value) > 1)
                value = value.TrimEnd(['k', 'm', 'g', 't', 'p', 'e', 'z', 'y', 'i', 'b', ' ']);

            // If the value is empty after trimming
            if (value.Length == 0)
                return false;

            // Otherwise, make sure that every character is a proper match
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
#if NET7_0_OR_GREATER
                if (!char.IsAsciiHexDigit(c) && c != '.' && c != ',')
#else
                if (!c.IsAsciiHexDigit() && c != '.' && c != ',')
#endif
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the human-readable file size for an arbitrary, 64-bit file size 
        /// The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB".
        /// </summary>
        /// <link>http://www.somacon.com/p576.php</link>
        /// <remarks>This uses 1024-byte partitions, not 1000-byte</remarks>
        public static string GetBytesReadable(long input)
        {
            // Get absolute value
            long absolute_i = (input < 0 ? -input : input);

            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000_0000_0000_0000) // Exabyte
            {
                suffix = "EB";
                readable = (input >> 50);
            }
            else if (absolute_i >= 0x4_0000_0000_0000) // Petabyte
            {
                suffix = "PB";
                readable = (input >> 40);
            }
            else if (absolute_i >= 0x100_0000_0000) // Terabyte
            {
                suffix = "TB";
                readable = (input >> 30);
            }
            else if (absolute_i >= 0x4000_0000) // Gigabyte
            {
                suffix = "GB";
                readable = (input >> 20);
            }
            else if (absolute_i >= 0x10_0000) // Megabyte
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

#if NETFRAMEWORK || NETCOREAPP3_1 || NET5_0 || NET6_0 || NETSTANDARD2_0_OR_GREATER
        /// <summary>
        /// Indicates whether a character is categorized as an ASCII hexademical digit.
        /// </summary>
        /// <param name="c">The character to evaluate.</param>
        /// <returns>true if c is a hexademical digit; otherwise, false.</returns>
        /// <remarks>This method determines whether the character is in the range '0' through '9', inclusive, 'A' through 'F', inclusive, or 'a' through 'f', inclusive.</remarks>
        internal static bool IsAsciiHexDigit(this char c)
        {
            return char.ToLowerInvariant(c) switch
            {
                '0' => true,
                '1' => true,
                '2' => true,
                '3' => true,
                '4' => true,
                '5' => true,
                '6' => true,
                '7' => true,
                '8' => true,
                '9' => true,
                'a' => true,
                'b' => true,
                'c' => true,
                'd' => true,
                'e' => true,
                'f' => true,
                _ => false,
            };
        }
#endif
    }
}
