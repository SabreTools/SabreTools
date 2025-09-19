using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using Xunit;

namespace SabreTools.DatFiles.Test
{
    public class DatStatisticsTests
    {
        #region Constructor

        [Fact]
        public void DefaultConstructorTest()
        {
            var stats = new DatStatistics();

            Assert.Null(stats.DisplayName);
            Assert.Equal(0, stats.MachineCount);
            Assert.False(stats.IsDirectory);
        }

        [Fact]
        public void NamedConstructorTest()
        {
            var stats = new DatStatistics("name", isDirectory: true);

            Assert.Equal("name", stats.DisplayName);
            Assert.Equal(0, stats.MachineCount);
            Assert.True(stats.IsDirectory);
        }

        #endregion

        #region End to End

        [Fact]
        public void AddRemoveStatisticsTest()
        {
            // Get items for testing
            var disk = CreateDisk();
            var file = CreateFile();
            var media = CreateMedia();
            var rom = CreateRom();
            var sample = CreateSample();

            // Create an empty stats object
            var stats = new DatStatistics();

            // Validate pre-add values
            Assert.Equal(0, stats.TotalCount);
            Assert.Equal(0, stats.TotalSize);
            Assert.Equal(0, stats.GetHashCount(HashType.CRC32));
            Assert.Equal(0, stats.GetHashCount(HashType.MD2));
            Assert.Equal(0, stats.GetHashCount(HashType.MD4));
            Assert.Equal(0, stats.GetHashCount(HashType.MD5));
            Assert.Equal(0, stats.GetHashCount(HashType.RIPEMD128));
            Assert.Equal(0, stats.GetHashCount(HashType.RIPEMD160));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA1));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA256));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA384));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA512));
            Assert.Equal(0, stats.GetHashCount(HashType.SpamSum));
            Assert.Equal(0, stats.GetItemCount(ItemType.Disk));
            Assert.Equal(0, stats.GetItemCount(ItemType.File));
            Assert.Equal(0, stats.GetItemCount(ItemType.Media));
            Assert.Equal(0, stats.GetItemCount(ItemType.Rom));
            Assert.Equal(0, stats.GetItemCount(ItemType.Sample));
            Assert.Equal(0, stats.GetStatusCount(ItemStatus.Good));

            // AddItemStatistics
            stats.AddItemStatistics(disk);
            stats.AddItemStatistics(file);
            stats.AddItemStatistics(media);
            stats.AddItemStatistics(rom);
            stats.AddItemStatistics(sample);

            // Validate post-add values
            Assert.Equal(5, stats.TotalCount);
            Assert.Equal(2, stats.TotalSize);
            Assert.Equal(2, stats.GetHashCount(HashType.CRC32));
            Assert.Equal(1, stats.GetHashCount(HashType.MD2));
            Assert.Equal(1, stats.GetHashCount(HashType.MD4));
            Assert.Equal(4, stats.GetHashCount(HashType.MD5));
            Assert.Equal(1, stats.GetHashCount(HashType.RIPEMD128));
            Assert.Equal(1, stats.GetHashCount(HashType.RIPEMD160));
            Assert.Equal(4, stats.GetHashCount(HashType.SHA1));
            Assert.Equal(3, stats.GetHashCount(HashType.SHA256));
            Assert.Equal(1, stats.GetHashCount(HashType.SHA384));
            Assert.Equal(1, stats.GetHashCount(HashType.SHA512));
            Assert.Equal(2, stats.GetHashCount(HashType.SpamSum));
            Assert.Equal(1, stats.GetItemCount(ItemType.Disk));
            Assert.Equal(1, stats.GetItemCount(ItemType.File));
            Assert.Equal(1, stats.GetItemCount(ItemType.Media));
            Assert.Equal(1, stats.GetItemCount(ItemType.Rom));
            Assert.Equal(1, stats.GetItemCount(ItemType.Sample));
            Assert.Equal(2, stats.GetStatusCount(ItemStatus.Good));

            // RemoveItemStatistics
            stats.RemoveItemStatistics(disk);
            stats.RemoveItemStatistics(file);
            stats.RemoveItemStatistics(media);
            stats.RemoveItemStatistics(rom);
            stats.RemoveItemStatistics(sample);

            // Validate post-remove values
            Assert.Equal(0, stats.TotalCount);
            Assert.Equal(0, stats.TotalSize);
            Assert.Equal(0, stats.GetHashCount(HashType.CRC32));
            Assert.Equal(0, stats.GetHashCount(HashType.MD2));
            Assert.Equal(0, stats.GetHashCount(HashType.MD4));
            Assert.Equal(0, stats.GetHashCount(HashType.MD5));
            Assert.Equal(0, stats.GetHashCount(HashType.RIPEMD128));
            Assert.Equal(0, stats.GetHashCount(HashType.RIPEMD160));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA1));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA256));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA384));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA512));
            Assert.Equal(0, stats.GetHashCount(HashType.SpamSum));
            Assert.Equal(0, stats.GetItemCount(ItemType.Disk));
            Assert.Equal(0, stats.GetItemCount(ItemType.File));
            Assert.Equal(0, stats.GetItemCount(ItemType.Media));
            Assert.Equal(0, stats.GetItemCount(ItemType.Rom));
            Assert.Equal(0, stats.GetItemCount(ItemType.Sample));
            Assert.Equal(0, stats.GetStatusCount(ItemStatus.Good));
        }

        [Fact]
        public void ResetStatisticsTest()
        {
            // Get items for testing
            var disk = CreateDisk();
            var file = CreateFile();
            var media = CreateMedia();
            var rom = CreateRom();
            var sample = CreateSample();

            // Create an empty stats object
            var stats = new DatStatistics();

            // Validate pre-add values
            Assert.Equal(0, stats.TotalCount);
            Assert.Equal(0, stats.TotalSize);
            Assert.Equal(0, stats.GetHashCount(HashType.CRC32));
            Assert.Equal(0, stats.GetHashCount(HashType.MD2));
            Assert.Equal(0, stats.GetHashCount(HashType.MD4));
            Assert.Equal(0, stats.GetHashCount(HashType.MD5));
            Assert.Equal(0, stats.GetHashCount(HashType.RIPEMD128));
            Assert.Equal(0, stats.GetHashCount(HashType.RIPEMD160));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA1));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA256));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA384));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA512));
            Assert.Equal(0, stats.GetHashCount(HashType.SpamSum));
            Assert.Equal(0, stats.GetItemCount(ItemType.Disk));
            Assert.Equal(0, stats.GetItemCount(ItemType.File));
            Assert.Equal(0, stats.GetItemCount(ItemType.Media));
            Assert.Equal(0, stats.GetItemCount(ItemType.Rom));
            Assert.Equal(0, stats.GetItemCount(ItemType.Sample));
            Assert.Equal(0, stats.GetStatusCount(ItemStatus.Good));

            // AddItemStatistics
            stats.AddItemStatistics(disk);
            stats.AddItemStatistics(file);
            stats.AddItemStatistics(media);
            stats.AddItemStatistics(rom);
            stats.AddItemStatistics(sample);

            // Validate post-add values
            Assert.Equal(5, stats.TotalCount);
            Assert.Equal(2, stats.TotalSize);
            Assert.Equal(2, stats.GetHashCount(HashType.CRC32));
            Assert.Equal(1, stats.GetHashCount(HashType.MD2));
            Assert.Equal(1, stats.GetHashCount(HashType.MD4));
            Assert.Equal(4, stats.GetHashCount(HashType.MD5));
            Assert.Equal(1, stats.GetHashCount(HashType.RIPEMD128));
            Assert.Equal(1, stats.GetHashCount(HashType.RIPEMD160));
            Assert.Equal(4, stats.GetHashCount(HashType.SHA1));
            Assert.Equal(3, stats.GetHashCount(HashType.SHA256));
            Assert.Equal(1, stats.GetHashCount(HashType.SHA384));
            Assert.Equal(1, stats.GetHashCount(HashType.SHA512));
            Assert.Equal(2, stats.GetHashCount(HashType.SpamSum));
            Assert.Equal(1, stats.GetItemCount(ItemType.Disk));
            Assert.Equal(1, stats.GetItemCount(ItemType.File));
            Assert.Equal(1, stats.GetItemCount(ItemType.Media));
            Assert.Equal(1, stats.GetItemCount(ItemType.Rom));
            Assert.Equal(1, stats.GetItemCount(ItemType.Sample));
            Assert.Equal(2, stats.GetStatusCount(ItemStatus.Good));

            // ResetStatistics
            stats.ResetStatistics();

            // Validate post-reset values
            Assert.Equal(0, stats.TotalCount);
            Assert.Equal(0, stats.TotalSize);
            Assert.Equal(0, stats.GetHashCount(HashType.CRC32));
            Assert.Equal(0, stats.GetHashCount(HashType.MD2));
            Assert.Equal(0, stats.GetHashCount(HashType.MD4));
            Assert.Equal(0, stats.GetHashCount(HashType.MD5));
            Assert.Equal(0, stats.GetHashCount(HashType.RIPEMD128));
            Assert.Equal(0, stats.GetHashCount(HashType.RIPEMD160));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA1));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA256));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA384));
            Assert.Equal(0, stats.GetHashCount(HashType.SHA512));
            Assert.Equal(0, stats.GetHashCount(HashType.SpamSum));
            Assert.Equal(0, stats.GetItemCount(ItemType.Disk));
            Assert.Equal(0, stats.GetItemCount(ItemType.File));
            Assert.Equal(0, stats.GetItemCount(ItemType.Media));
            Assert.Equal(0, stats.GetItemCount(ItemType.Rom));
            Assert.Equal(0, stats.GetItemCount(ItemType.Sample));
            Assert.Equal(0, stats.GetStatusCount(ItemStatus.Good));
        }

        #endregion

        #region AddStatistics

        [Fact]
        public void AddStatisticsTest()
        {
            var rom = CreateRom();
            var origStats = new DatStatistics();
            origStats.AddItemStatistics(rom);

            var newStats = new DatStatistics();
            newStats.AddStatistics(origStats);

            Assert.Equal(1, newStats.TotalCount);
            Assert.Equal(1, newStats.TotalSize);
            Assert.Equal(1, newStats.GetHashCount(HashType.CRC32));
            Assert.Equal(1, newStats.GetHashCount(HashType.MD2));
            Assert.Equal(1, newStats.GetHashCount(HashType.MD4));
            Assert.Equal(1, newStats.GetHashCount(HashType.MD5));
            Assert.Equal(1, newStats.GetHashCount(HashType.RIPEMD128));
            Assert.Equal(1, newStats.GetHashCount(HashType.RIPEMD160));
            Assert.Equal(1, newStats.GetHashCount(HashType.SHA1));
            Assert.Equal(1, newStats.GetHashCount(HashType.SHA256));
            Assert.Equal(1, newStats.GetHashCount(HashType.SHA384));
            Assert.Equal(1, newStats.GetHashCount(HashType.SHA512));
            Assert.Equal(1, newStats.GetHashCount(HashType.SpamSum));
            Assert.Equal(1, newStats.GetItemCount(ItemType.Rom));
            Assert.Equal(1, newStats.GetStatusCount(ItemStatus.Good));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Create a Disk for testing
        /// </summary>
        private static Disk CreateDisk()
        {
            var disk = new Disk();

            disk.SetFieldValue<string?>(Models.Metadata.Disk.StatusKey, ItemStatus.Good.AsStringValue());
            disk.SetFieldValue<string?>(Models.Metadata.Disk.MD5Key, ZeroHash.MD5Str);
            disk.SetFieldValue<string?>(Models.Metadata.Disk.SHA1Key, ZeroHash.SHA1Str);

            return disk;
        }

        /// <summary>
        /// Create a File for testing
        /// </summary>
        private static DatItems.Formats.File CreateFile()
        {
            var file = new DatItems.Formats.File();

            file.Size = 1;
            file.CRC = ZeroHash.CRC32Str;
            file.MD5 = ZeroHash.MD5Str;
            file.SHA1 = ZeroHash.SHA1Str;
            file.SHA256 = ZeroHash.SHA256Str;

            return file;
        }

        /// <summary>
        /// Create a Media for testing
        /// </summary>
        private static Media CreateMedia()
        {
            var media = new Media();

            media.SetFieldValue<string?>(Models.Metadata.Media.MD5Key, ZeroHash.MD5Str);
            media.SetFieldValue<string?>(Models.Metadata.Media.SHA1Key, ZeroHash.SHA1Str);
            media.SetFieldValue<string?>(Models.Metadata.Media.SHA256Key, ZeroHash.SHA256Str);
            media.SetFieldValue<string?>(Models.Metadata.Media.SpamSumKey, ZeroHash.SpamSumStr);

            return media;
        }

        /// <summary>
        /// Create a Rom for testing
        /// </summary>
        private static Rom CreateRom()
        {
            var rom = new Rom();

            rom.SetFieldValue<string?>(Models.Metadata.Rom.StatusKey, ItemStatus.Good.AsStringValue());
            rom.SetFieldValue<long>(Models.Metadata.Rom.SizeKey, 1);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.CRCKey, ZeroHash.CRC32Str);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.MD2Key, ZeroHash.GetString(HashType.MD2));
            rom.SetFieldValue<string?>(Models.Metadata.Rom.MD4Key, ZeroHash.GetString(HashType.MD4));
            rom.SetFieldValue<string?>(Models.Metadata.Rom.MD5Key, ZeroHash.MD5Str);
            rom.SetFieldValue<string?>("RIPEMD128", ZeroHash.GetString(HashType.RIPEMD128));
            rom.SetFieldValue<string?>("RIPEMD160", ZeroHash.GetString(HashType.RIPEMD160));
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA1Key, ZeroHash.SHA1Str);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA256Key, ZeroHash.SHA256Str);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA384Key, ZeroHash.SHA384Str);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA512Key, ZeroHash.SHA512Str);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SpamSumKey, ZeroHash.SpamSumStr);

            return rom;
        }

        /// <summary>
        /// Create a Sample for testing
        /// </summary>
        private static Sample CreateSample() => new();

        #endregion
    }
}