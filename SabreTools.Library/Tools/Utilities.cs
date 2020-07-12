using System;

using SabreTools.Library.Data;

namespace SabreTools.Library.Tools
{
    /// <summary>
    /// Static utility functions used throughout the library
    /// </summary>
    public static class Utilities
    {
        #region Archive Scanning Information

        /// <summary>
        /// Get the archive scan level based on the inputs
        /// </summary>
        /// <param name="sevenzip">User-defined scan level for 7z archives</param>
        /// <param name="gzip">User-defined scan level for GZ archives</param>
        /// <param name="rar">User-defined scan level for RAR archives</param>
        /// <param name="zip">User-defined scan level for Zip archives</param>
        /// <returns>ArchiveScanLevel representing the levels</returns>
        /// TODO: Can this be phased out?
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
        /// Get if the current file should be scanned internally and externally
        /// </summary>
        /// <param name="input">Name of the input file to check</param>
        /// <param name="archiveScanLevel">ArchiveScanLevel representing the archive handling levels</param>
        /// <param name="shouldExternalProcess">Output parameter determining if file should be processed externally</param>
        /// <param name="shouldInternalProcess">Output parameter determining if file should be processed internally</param>
        /// TODO: Can this be phased out?
        public static void GetInternalExternalProcess(string input, ArchiveScanLevel archiveScanLevel,
            out bool shouldExternalProcess, out bool shouldInternalProcess)
        {
            FileType? fileType = input.GetFileType();
            switch (fileType)
            {
                case FileType.GZipArchive:
                    shouldExternalProcess = (archiveScanLevel.HasFlag(ArchiveScanLevel.GZipExternal));
                    shouldInternalProcess = (archiveScanLevel.HasFlag(ArchiveScanLevel.GZipInternal));
                    break;
                case FileType.RarArchive:
                    shouldExternalProcess = (archiveScanLevel.HasFlag(ArchiveScanLevel.RarExternal));
                    shouldInternalProcess = (archiveScanLevel.HasFlag(ArchiveScanLevel.RarInternal));
                    break;
                case FileType.SevenZipArchive:
                    shouldExternalProcess = (archiveScanLevel.HasFlag(ArchiveScanLevel.SevenZipExternal));
                    shouldInternalProcess = (archiveScanLevel.HasFlag(ArchiveScanLevel.SevenZipInternal));
                    break;
                case FileType.ZipArchive:
                    shouldExternalProcess = (archiveScanLevel.HasFlag(ArchiveScanLevel.ZipExternal));
                    shouldInternalProcess = (archiveScanLevel.HasFlag(ArchiveScanLevel.ZipInternal));
                    break;
                case null:
                default:
                    shouldExternalProcess = true;
                    shouldInternalProcess = false;
                    break;
            }
        }

        #endregion    

        #region Miscellaneous / Externally Sourced

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

        /// Indicates whether the specified array is null or has a length of zero
        /// </summary>
        /// <param name="array">The array to test</param>
        /// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
        /// <link>https://stackoverflow.com/questions/8560106/isnullorempty-equivalent-for-array-c-sharp</link>
        public static bool IsNullOrEmpty(this Array array)
        {
            return (array == null || array.Length == 0);
        }

        #endregion
    }
}
