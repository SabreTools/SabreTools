using System.Text;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.FileTypes;
using SabreTools.FileTypes.Aaru;
using SabreTools.FileTypes.Archives;
using SabreTools.FileTypes.CHD;
using SabreTools.IO.Extensions;
using Xunit;

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
            Assert.Equal(expected, actual?.GetStringFieldValue(Data.Models.Metadata.DatItem.TypeKey).AsItemType());
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

            Assert.Equal(filename, actual.GetStringFieldValue(Data.Models.Metadata.Disk.NameKey));
            Assert.Equal(md5, actual.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key));
            Assert.Equal(sha1, actual.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key));
            Assert.Equal((DupeType)0x00, actual.GetFieldValue<DupeType>(DatItem.DupeTypeKey));
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

            Assert.Equal(filename, actual.GetStringFieldValue(Data.Models.Metadata.Disk.NameKey));
            Assert.Equal(md5, actual.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key));
            Assert.Equal(sha1, actual.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key));
            Assert.Equal((DupeType)0x00, actual.GetFieldValue<DupeType>(DatItem.DupeTypeKey));
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
                CRC = crc.FromHexString(),
                MD5 = md5.FromHexString(),
                SHA1 = sha1.FromHexString(),
                SHA256 = sha256.FromHexString(),
            };

            File actual = baseFile.ConvertToFile();

            Assert.Equal(crc, actual.CRC);
            Assert.Equal(md5, actual.MD5);
            Assert.Equal(sha1, actual.SHA1);
            Assert.Equal(sha256, actual.SHA256);
            Assert.Equal((DupeType)0x00, actual.GetFieldValue<DupeType>(DatItem.DupeTypeKey));
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

            Assert.Equal(filename, actual.GetStringFieldValue(Data.Models.Metadata.Media.NameKey));
            Assert.Equal(md5, actual.GetStringFieldValue(Data.Models.Metadata.Media.MD5Key));
            Assert.Equal(sha1, actual.GetStringFieldValue(Data.Models.Metadata.Media.SHA1Key));
            Assert.Equal(sha256, actual.GetStringFieldValue(Data.Models.Metadata.Media.SHA256Key));
            Assert.Equal(spamSum, actual.GetStringFieldValue(Data.Models.Metadata.Media.SpamSumKey));
            Assert.Equal((DupeType)0x00, actual.GetFieldValue<DupeType>(DatItem.DupeTypeKey));
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

            Assert.Equal(filename, actual.GetStringFieldValue(Data.Models.Metadata.Media.NameKey));
            Assert.Equal(md5, actual.GetStringFieldValue(Data.Models.Metadata.Media.MD5Key));
            Assert.Equal(sha1, actual.GetStringFieldValue(Data.Models.Metadata.Media.SHA1Key));
            Assert.Equal(sha256, actual.GetStringFieldValue(Data.Models.Metadata.Media.SHA256Key));
            Assert.Equal(spamSum, actual.GetStringFieldValue(Data.Models.Metadata.Media.SpamSumKey));
            Assert.Equal((DupeType)0x00, actual.GetFieldValue<DupeType>(DatItem.DupeTypeKey));
        }

        #endregion

        #region ConvertToRom

        [Fact]
        public void ConvertToRom_Generic()
        {
            string filename = "XXXXXX";
            string date = "XXXXXX";
            string crc = TextHelper.NormalizeCRC32("1234abcd")!;
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
                CRC = crc.FromHexString(),
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

            Assert.Equal(filename, actual.GetStringFieldValue(Data.Models.Metadata.Rom.NameKey));
            Assert.Equal(date, actual.GetStringFieldValue(Data.Models.Metadata.Rom.DateKey));
            Assert.Equal(crc, actual.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey));
            Assert.Equal(md2, actual.GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key));
            Assert.Equal(md4, actual.GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key));
            Assert.Equal(md5, actual.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key));
            Assert.Equal(ripemd128, actual.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key));
            Assert.Equal(ripemd160, actual.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key));
            Assert.Equal(sha1, actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal(sha256, actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key));
            Assert.Equal(sha384, actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key));
            Assert.Equal(sha512, actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key));
            Assert.Equal(spamSum, actual.GetStringFieldValue(Data.Models.Metadata.Rom.SpamSumKey));
            Assert.Equal(size, actual.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey));
            Assert.Equal((DupeType)0x00, actual.GetFieldValue<DupeType>(DatItem.DupeTypeKey));
        }

        #endregion

        #region ConvertToBaseFile

        [Fact]
        public void ConvertToBaseFile_Disk()
        {
            string filename = "XXXXXX";
            string md5 = TextHelper.NormalizeMD5("1234abcd")!;
            string sha1 = TextHelper.NormalizeSHA1("1234abcd")!;

            Disk disk = new Disk();
            disk.SetFieldValue<string?>(Data.Models.Metadata.Disk.NameKey, filename);
            disk.SetFieldValue<string?>(Data.Models.Metadata.Disk.MD5Key, md5);
            disk.SetFieldValue<string?>(Data.Models.Metadata.Disk.SHA1Key, sha1);

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

            DatItems.Formats.File file = new DatItems.Formats.File
            {
                CRC = crc,
                MD5 = md5,
                SHA1 = sha1,
                SHA256 = sha256,
            };

            BaseFile actual = file.ConvertToBaseFile();

            Assert.Equal(crc, actual.CRC.ToHexString());
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

            Media media = new Media();
            media.SetFieldValue<string?>(Data.Models.Metadata.Media.NameKey, filename);
            media.SetFieldValue<string?>(Data.Models.Metadata.Media.MD5Key, md5);
            media.SetFieldValue<string?>(Data.Models.Metadata.Media.SHA1Key, sha1);
            media.SetFieldValue<string?>(Data.Models.Metadata.Media.SHA256Key, sha256);
            media.SetFieldValue<string?>(Data.Models.Metadata.Media.SpamSumKey, spamSum);

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
            string crc = TextHelper.NormalizeCRC32("1234abcd")!;
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

            Rom rom = new Rom();
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.NameKey, filename);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.DateKey, date);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, crc);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD2Key, md2);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD4Key, md4);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD5Key, md5);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD128Key, ripemd128);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD160Key, ripemd160);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, sha1);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA256Key, sha256);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA384Key, sha384);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA512Key, sha512);
            rom.SetFieldValue<string?>(Data.Models.Metadata.Rom.SpamSumKey, spamSum);
            rom.SetFieldValue<long>(Data.Models.Metadata.Rom.SizeKey, size);

            BaseFile actual = rom.ConvertToBaseFile();

            Assert.Equal(filename, actual.Filename);
            Assert.Equal(date, actual.Date);
            Assert.Equal(crc, actual.CRC.ToHexString());
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