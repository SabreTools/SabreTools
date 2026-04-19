using System.Text;
using SabreTools.FileTypes;
using SabreTools.FileTypes.Aaru;
using SabreTools.FileTypes.Archives;
using SabreTools.FileTypes.CHD;
using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;
using SabreTools.Text.Extensions;
using Xunit;
using ItemType = SabreTools.Data.Models.Metadata.ItemType;

namespace SabreTools.DatTools.Test
{
    public class DatItemToolTests
    {
        #region CreateDatItem

        [Theory]
        [InlineData(FileType.None, (TreatAsFile)0x00, ItemType.Rom)]
        [InlineData(FileType.AaruFormat, (TreatAsFile)0x00, ItemType.Media)]
        [InlineData(FileType.AaruFormat, TreatAsFile.AaruFormat, ItemType.Rom)]
        [InlineData(FileType.AaruFormat, TreatAsFile.NonArchive, ItemType.Rom)]
        [InlineData(FileType.AaruFormat, TreatAsFile.All, ItemType.Rom)]
        [InlineData(FileType.CHD, (TreatAsFile)0x00, ItemType.Disk)]
        [InlineData(FileType.CHD, TreatAsFile.CHD, ItemType.Rom)]
        [InlineData(FileType.CHD, TreatAsFile.NonArchive, ItemType.Rom)]
        [InlineData(FileType.CHD, TreatAsFile.All, ItemType.Rom)]
        [InlineData(FileType.Folder, (TreatAsFile)0x00, null)]
        [InlineData(FileType.SevenZipArchive, (TreatAsFile)0x00, ItemType.Rom)]
        [InlineData(FileType.SevenZipArchive, TreatAsFile.Archive, ItemType.Rom)]
        [InlineData(FileType.SevenZipArchive, TreatAsFile.All, ItemType.Rom)]
        [InlineData(FileType.GZipArchive, (TreatAsFile)0x00, ItemType.Rom)]
        [InlineData(FileType.GZipArchive, TreatAsFile.Archive, ItemType.Rom)]
        [InlineData(FileType.GZipArchive, TreatAsFile.All, ItemType.Rom)]
        [InlineData(FileType.RarArchive, (TreatAsFile)0x00, ItemType.Rom)]
        [InlineData(FileType.RarArchive, TreatAsFile.Archive, ItemType.Rom)]
        [InlineData(FileType.RarArchive, TreatAsFile.All, ItemType.Rom)]
        [InlineData(FileType.TapeArchive, (TreatAsFile)0x00, ItemType.Rom)]
        [InlineData(FileType.TapeArchive, TreatAsFile.Archive, ItemType.Rom)]
        [InlineData(FileType.TapeArchive, TreatAsFile.All, ItemType.Rom)]
        [InlineData(FileType.XZArchive, (TreatAsFile)0x00, ItemType.Rom)]
        [InlineData(FileType.XZArchive, TreatAsFile.Archive, ItemType.Rom)]
        [InlineData(FileType.XZArchive, TreatAsFile.All, ItemType.Rom)]
        [InlineData(FileType.ZipArchive, (TreatAsFile)0x00, ItemType.Rom)]
        [InlineData(FileType.ZipArchive, TreatAsFile.Archive, ItemType.Rom)]
        [InlineData(FileType.ZipArchive, TreatAsFile.All, ItemType.Rom)]
        public void CreateDatItemTest(FileType fileType, TreatAsFile asFile, ItemType? expected)
        {
            var baseFile = CreateBaseFile(fileType);
            var actual = DatItemTool.CreateDatItem(baseFile, asFile);
            Assert.Equal(expected, actual?.ItemType);
        }

        #endregion

        #region ConvertToDisk

        [Fact]
        public void ConvertToDisk_CHDFile()
        {
            string filename = "XXXXXX";
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;

            BaseFile baseFile = new CHDFile
            {
                Filename = filename,
                MD5 = null,
                InternalMD5 = md5.FromHexString(),
                SHA1 = null,
                InternalSHA1 = sha1.FromHexString(),
            };

            Disk actual = baseFile.ConvertToDisk();

            Assert.Equal(filename, actual.Name);
            Assert.Equal(md5, actual.MD5);
            Assert.Equal(sha1, actual.SHA1);
            Assert.Equal((DupeType)0x00, actual.DupeType);
        }

        [Fact]
        public void ConvertToDisk_Generic()
        {
            string filename = "XXXXXX";
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;

            BaseFile baseFile = new BaseFile
            {
                Filename = filename,
                MD5 = md5.FromHexString(),
                SHA1 = sha1.FromHexString(),
            };

            Disk actual = baseFile.ConvertToDisk();

            Assert.Equal(filename, actual.Name);
            Assert.Equal(md5, actual.MD5);
            Assert.Equal(sha1, actual.SHA1);
            Assert.Equal((DupeType)0x00, actual.DupeType);
        }

        #endregion

        #region ConvertToFile

        [Fact]
        public void ConvertToFile_Generic()
        {
            string crc = TextHelper.NormalizeCRC32("1234abcd")!;
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;
            string sha256 = TextHelper.NormalizeSHA256("1234abcd")!;

            BaseFile baseFile = new BaseFile
            {
                CRC32 = crc.FromHexString(),
                MD5 = md5.FromHexString(),
                SHA1 = sha1.FromHexString(),
                SHA256 = sha256.FromHexString(),
            };

            File actual = baseFile.ConvertToFile();

            Assert.Equal(crc, actual.CRC);
            Assert.Equal(md5, actual.MD5);
            Assert.Equal(sha1, actual.SHA1);
            Assert.Equal(sha256, actual.SHA256);
            Assert.Equal((DupeType)0x00, actual.DupeType);
        }

        #endregion

        #region ConvertToMedia

        [Fact]
        public void ConvertToMedia_AaruFormat()
        {
            string filename = "XXXXXX";
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;
            string sha256 = TextHelper.NormalizeSHA256("1234abcd")!;
            string spamSum = "1234abcd";

            BaseFile baseFile = new AaruFormat
            {
                Filename = filename,
                MD5 = null,
                InternalMD5 = md5.FromHexString(),
                SHA1 = null,
                InternalSHA1 = sha1.FromHexString(),
                SHA256 = null,
                InternalSHA256 = sha256.FromHexString(),
                SpamSum = null,
                InternalSpamSum = Encoding.ASCII.GetBytes(spamSum),
            };

            Media actual = baseFile.ConvertToMedia();

            Assert.Equal(filename, actual.Name);
            Assert.Equal(md5, actual.MD5);
            Assert.Equal(sha1, actual.SHA1);
            Assert.Equal(sha256, actual.SHA256);
            Assert.Equal(spamSum, actual.SpamSum);
            Assert.Equal((DupeType)0x00, actual.DupeType);
        }

        [Fact]
        public void ConvertToMedia_Generic()
        {
            string filename = "XXXXXX";
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;
            string sha256 = TextHelper.NormalizeSHA256("1234abcd")!;
            string spamSum = "1234abcd";

            BaseFile baseFile = new BaseFile
            {
                Filename = filename,
                MD5 = md5.FromHexString(),
                SHA1 = sha1.FromHexString(),
                SHA256 = sha256.FromHexString(),
                SpamSum = Encoding.ASCII.GetBytes(spamSum),
            };

            Media actual = baseFile.ConvertToMedia();

            Assert.Equal(filename, actual.Name);
            Assert.Equal(md5, actual.MD5);
            Assert.Equal(sha1, actual.SHA1);
            Assert.Equal(sha256, actual.SHA256);
            Assert.Equal(spamSum, actual.SpamSum);
            Assert.Equal((DupeType)0x00, actual.DupeType);
        }

        #endregion

        #region ConvertToRom

        [Fact]
        public void ConvertToRom_Generic()
        {
            string filename = "XXXXXX";
            string date = "XXXXXX";
            // string crc16 = TextHelper.NormalizeCRC16("1234abcd")!;
            string crc32 = TextHelper.NormalizeCRC32("1234abcd")!;
            // string crc64 = TextHelper.NormalizeCRC64("1234abcd")!;
            string md2 = TextHelper.NormalizeMD2("1234abcd")!;
            string md4 = TextHelper.NormalizeMD4("1234abcd")!;
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string ripemd128 = TextHelper.NormalizeRIPEMD128("1234abcd")!;
            string ripemd160 = TextHelper.NormalizeRIPEMD160("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;
            string sha256 = TextHelper.NormalizeSHA256("1234abcd")!;
            string sha384 = TextHelper.NormalizeSHA384("1234abcd")!;
            string sha512 = TextHelper.NormalizeSHA512("1234abcd")!;
            string spamSum = "1234abcd";
            long size = 12345;

            BaseFile baseFile = new BaseFile
            {
                Filename = filename,
                Date = date,
                CRC32 = crc32.FromHexString(),
                MD2 = md2.FromHexString(),
                MD4 = md4.FromHexString(),
                MD5 = md5.FromHexString(),
                RIPEMD128 = ripemd128.FromHexString(),
                RIPEMD160 = ripemd160.FromHexString(),
                SHA1 = sha1.FromHexString(),
                SHA256 = sha256.FromHexString(),
                SHA384 = sha384.FromHexString(),
                SHA512 = sha512.FromHexString(),
                SpamSum = Encoding.ASCII.GetBytes(spamSum),
                Size = size,
            };

            Rom actual = baseFile.ConvertToRom();

            Assert.Equal(filename, actual.Name);
            Assert.Equal(date, actual.Date);
            Assert.Equal(crc32, actual.CRC32);
            Assert.Equal(md2, actual.MD2);
            Assert.Equal(md4, actual.MD4);
            Assert.Equal(md5, actual.MD5);
            Assert.Equal(ripemd128, actual.RIPEMD128);
            Assert.Equal(ripemd160, actual.RIPEMD160);
            Assert.Equal(sha1, actual.SHA1);
            Assert.Equal(sha256, actual.SHA256);
            Assert.Equal(sha384, actual.SHA384);
            Assert.Equal(sha512, actual.SHA512);
            Assert.Equal(spamSum, actual.SpamSum);
            Assert.Equal(size, actual.Size);
            Assert.Equal((DupeType)0x00, actual.DupeType);
        }

        #endregion

        #region ConvertToBaseFile

        [Fact]
        public void ConvertToBaseFile_Disk()
        {
            string filename = "XXXXXX";
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;

            Disk disk = new Disk
            {
                Name = filename,
                MD5 = md5,
                SHA1 = sha1,
            };

            BaseFile actual = disk.ConvertToBaseFile();
            CHDFile? actualChd = actual as CHDFile;
            Assert.NotNull(actualChd);

            Assert.Equal(filename, actualChd.Filename);
            Assert.Equal(md5, actualChd.MD5.ToHexString());
            Assert.Equal(md5, actualChd.InternalMD5.ToHexString());
            Assert.Equal(sha1, actualChd.SHA1.ToHexString());
            Assert.Equal(sha1, actualChd.InternalSHA1.ToHexString());
        }

        [Fact]
        public void ConvertToBaseFile_File()
        {
            string crc = TextHelper.NormalizeCRC32("1234abcd")!;
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;
            string sha256 = TextHelper.NormalizeSHA256("1234abcd")!;

            File file = new File
            {
                CRC = crc,
                MD5 = md5,
                SHA1 = sha1,
                SHA256 = sha256,
            };

            BaseFile actual = file.ConvertToBaseFile();

            Assert.Equal(crc, actual.CRC32.ToHexString());
            Assert.Equal(md5, actual.MD5.ToHexString());
            Assert.Equal(sha1, actual.SHA1.ToHexString());
            Assert.Equal(sha256, actual.SHA256.ToHexString());
        }

        [Fact]
        public void ConvertToBaseFile_Media()
        {
            string filename = "XXXXXX";
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;
            string sha256 = TextHelper.NormalizeSHA256("1234abcd")!;
            string spamSum = "1234abcd";

            Media media = new Media
            {
                Name = filename,
                MD5 = md5,
                SHA1 = sha1,
                SHA256 = sha256,
                SpamSum = spamSum,
            };

            BaseFile actual = media.ConvertToBaseFile();
            AaruFormat? actualAif = actual as AaruFormat;
            Assert.NotNull(actualAif);

            Assert.Equal(filename, actualAif.Filename);
            Assert.Equal(md5, actualAif.MD5.ToHexString());
            Assert.Equal(md5, actualAif.InternalMD5.ToHexString());
            Assert.Equal(sha1, actualAif.SHA1.ToHexString());
            Assert.Equal(sha1, actualAif.InternalSHA1.ToHexString());
            Assert.Equal(sha256, actualAif.SHA256.ToHexString());
            Assert.Equal(sha256, actualAif.InternalSHA256.ToHexString());
            string actualSpamSum = Encoding.ASCII.GetString(actualAif.SpamSum!);
            Assert.Equal(spamSum, actualSpamSum);
            actualSpamSum = Encoding.ASCII.GetString(actualAif.InternalSpamSum!);
            Assert.Equal(spamSum, actualSpamSum);
        }

        [Fact]
        public void ConvertToBaseFile_Rom()
        {
            string filename = "XXXXXX";
            string date = "XXXXXX";
            // string crc16 = TextHelper.NormalizeCRC16("1234abcd")!;
            string crc32 = TextHelper.NormalizeCRC32("1234abcd")!;
            // string crc64 = TextHelper.NormalizeCRC64("1234abcd")!;
            string md2 = TextHelper.NormalizeMD2("1234abcd")!;
            string md4 = TextHelper.NormalizeMD4("1234abcd")!;
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string ripemd128 = TextHelper.NormalizeRIPEMD128("1234abcd")!;
            string ripemd160 = TextHelper.NormalizeRIPEMD160("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;
            string sha256 = TextHelper.NormalizeSHA256("1234abcd")!;
            string sha384 = TextHelper.NormalizeSHA384("1234abcd")!;
            string sha512 = TextHelper.NormalizeSHA512("1234abcd")!;
            string spamSum = "1234abcd";
            long size = 12345;

            Rom rom = new Rom
            {
                Name = filename,
                Date = date,
                CRC32 = crc32,
                MD2 = md2,
                MD4 = md4,
                MD5 = md5,
                RIPEMD128 = ripemd128,
                RIPEMD160 = ripemd160,
                SHA1 = sha1,
                SHA256 = sha256,
                SHA384 = sha384,
                SHA512 = sha512,
                SpamSum = spamSum,
                Size = size,
            };

            BaseFile actual = rom.ConvertToBaseFile();

            Assert.Equal(filename, actual.Filename);
            Assert.Equal(date, actual.Date);
            Assert.Equal(crc32, actual.CRC32.ToHexString());
            Assert.Equal(md2, actual.MD2.ToHexString());
            Assert.Equal(md4, actual.MD4.ToHexString());
            Assert.Equal(md5, actual.MD5.ToHexString());
            Assert.Equal(ripemd128, actual.RIPEMD128.ToHexString());
            Assert.Equal(ripemd160, actual.RIPEMD160.ToHexString());
            Assert.Equal(sha1, actual.SHA1.ToHexString());
            Assert.Equal(sha256, actual.SHA256.ToHexString());
            Assert.Equal(sha384, actual.SHA384.ToHexString());
            Assert.Equal(sha512, actual.SHA512.ToHexString());
            string actualSpamSum = Encoding.ASCII.GetString(actual.SpamSum!);
            Assert.Equal(spamSum, actualSpamSum);
            Assert.Equal(size, actual.Size);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Create a BaseFile for testing
        /// </summary>
        private static BaseFile CreateBaseFile(FileType fileType)
        {
            return fileType switch
            {
                FileType.None => new BaseFile(),
                FileType.AaruFormat => new AaruFormat(),
                FileType.CHD => new CHDFile(),
                FileType.Folder => new Folder(),
                FileType.SevenZipArchive => new ZipArchive(),
                FileType.GZipArchive => new ZipArchive(),
                FileType.RarArchive => new ZipArchive(),
                FileType.TapeArchive => new ZipArchive(),
                FileType.XZArchive => new ZipArchive(),
                FileType.ZipArchive => new ZipArchive(),
                _ => new BaseFile(),
            };
        }

        #endregion
    }
}
