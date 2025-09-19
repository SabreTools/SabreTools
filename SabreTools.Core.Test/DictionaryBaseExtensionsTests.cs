using SabreTools.Hashing;
using SabreTools.Models.Metadata;
using Xunit;

namespace SabreTools.Core.Test
{
    public class DictionaryBaseExtensionsTests
    {
        #region ConvertToRom

        [Fact]
        public void ConvertToRom_Null_Null()
        {
            DictionaryBase? self = null;
            Rom? actual = self.ConvertToRom();
            Assert.Null(actual);
        }

        [Fact]
        public void ConvertToRom_EmptyDisk_EmptyRom()
        {
            DictionaryBase? self = new Disk();
            Rom? actual = self.ConvertToRom();

            Assert.NotNull(actual);
            Assert.Equal(8, actual.Count);
            Assert.Equal(ItemType.Rom, actual["_type"]);
            Assert.Null(actual[Rom.NameKey]);
            Assert.Null(actual[Rom.MergeKey]);
            Assert.Null(actual[Rom.RegionKey]);
            Assert.Null(actual[Rom.StatusKey]);
            Assert.Null(actual[Rom.OptionalKey]);
            Assert.Null(actual[Rom.MD5Key]);
            Assert.Null(actual[Rom.SHA1Key]);
        }

        [Fact]
        public void ConvertToRom_FilledDisk_FilledRom()
        {
            DictionaryBase? self = new Disk
            {
                [Disk.NameKey] = "XXXXXX",
                [Disk.MergeKey] = "XXXXXX",
                [Disk.RegionKey] = "XXXXXX",
                [Disk.StatusKey] = "XXXXXX",
                [Disk.OptionalKey] = "XXXXXX",
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };

            Rom? actual = self.ConvertToRom();

            Assert.NotNull(actual);
            Assert.Equal(8, actual.Count);
            Assert.Equal(ItemType.Rom, actual["_type"]);
            Assert.Equal("XXXXXX.chd", actual[Rom.NameKey]);
            Assert.Equal("XXXXXX", actual[Rom.MergeKey]);
            Assert.Equal("XXXXXX", actual[Rom.RegionKey]);
            Assert.Equal("XXXXXX", actual[Rom.StatusKey]);
            Assert.Equal("XXXXXX", actual[Rom.OptionalKey]);
            Assert.Equal("XXXXXX", actual[Rom.MD5Key]);
            Assert.Equal("XXXXXX", actual[Rom.SHA1Key]);
        }

        [Fact]
        public void ConvertToRom_EmptyMedia_EmptyRom()
        {
            DictionaryBase? self = new Media();
            Rom? actual = self.ConvertToRom();

            Assert.NotNull(actual);
            Assert.Equal(6, actual.Count);
            Assert.Equal(ItemType.Rom, actual["_type"]);
            Assert.Null(actual[Rom.NameKey]);
            Assert.Null(actual[Rom.MD5Key]);
            Assert.Null(actual[Rom.SHA1Key]);
            Assert.Null(actual[Rom.SHA256Key]);
            Assert.Null(actual[Rom.SpamSumKey]);
        }

        [Fact]
        public void ConvertToRom_FilledMedia_FilledRom()
        {
            DictionaryBase? self = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            Rom? actual = self.ConvertToRom();

            Assert.NotNull(actual);
            Assert.Equal(6, actual.Count);
            Assert.Equal(ItemType.Rom, actual["_type"]);
            Assert.Equal("XXXXXX.aaruf", actual[Rom.NameKey]);
            Assert.Equal("XXXXXX", actual[Rom.MD5Key]);
            Assert.Equal("XXXXXX", actual[Rom.SHA1Key]);
            Assert.Equal("XXXXXX", actual[Rom.SHA256Key]);
            Assert.Equal("XXXXXX", actual[Rom.SpamSumKey]);
        }

        [Fact]
        public void ConvertToRom_Other_Null()
        {
            DictionaryBase? self = new Sample();
            Rom? actual = self.ConvertToRom();
            Assert.Null(actual);
        }

        #endregion

        #region EqualTo

        [Fact]
        public void EqualTo_MismatchedTypes_False()
        {
            DictionaryBase self = new Disk();
            DictionaryBase other = new Rom();

            bool actual = self.EqualTo(other);
            Assert.False(actual);
        }

        [Fact]
        public void EqualTo_Disk_Nodumps_True()
        {
            DictionaryBase self = new Disk
            {
                [Disk.StatusKey] = "nodump",
                [Disk.NameKey] = "XXXXXX",
                [Disk.MD5Key] = string.Empty,
                [Disk.SHA1Key] = string.Empty,
            };
            DictionaryBase other = new Disk
            {
                [Disk.StatusKey] = "NODUMP",
                [Disk.NameKey] = "XXXXXX",
                [Disk.MD5Key] = string.Empty,
                [Disk.SHA1Key] = string.Empty,
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Disk_Mismatch_False()
        {
            DictionaryBase self = new Disk
            {
                [Disk.NameKey] = "XXXXXX",
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = string.Empty,
            };
            DictionaryBase other = new Disk
            {
                [Disk.NameKey] = "XXXXXX",
                [Disk.MD5Key] = string.Empty,
                [Disk.SHA1Key] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.False(actual);
        }

        [Fact]
        public void EqualTo_Disk_PartialMD5_True()
        {
            DictionaryBase self = new Disk
            {
                [Disk.NameKey] = "XXXXXX1",
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = string.Empty,
            };
            DictionaryBase other = new Disk
            {
                [Disk.NameKey] = "XXXXXX2",
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Disk_PartialSHA1_True()
        {
            DictionaryBase self = new Disk
            {
                [Disk.NameKey] = "XXXXXX1",
                [Disk.MD5Key] = string.Empty,
                [Disk.SHA1Key] = "XXXXXX",
            };
            DictionaryBase other = new Disk
            {
                [Disk.NameKey] = "XXXXXX2",
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Disk_FullMatch_True()
        {
            DictionaryBase self = new Disk
            {
                [Disk.NameKey] = "XXXXXX1",
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };
            DictionaryBase other = new Disk
            {
                [Disk.NameKey] = "XXXXXX2",
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Media_Mismatch_False()
        {
            DictionaryBase self = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.False(actual);
        }

        [Fact]
        public void EqualTo_Media_PartialMD5_True()
        {
            DictionaryBase self = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Media_PartialSHA1_True()
        {
            DictionaryBase self = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Media_PartialSHA256_True()
        {
            DictionaryBase self = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Media_PartialSpamSum_True()
        {
            DictionaryBase self = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = "XXXXXX",
            };
            DictionaryBase other = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Media_FullMatch_True()
        {
            DictionaryBase self = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };
            DictionaryBase other = new Media
            {
                [Media.NameKey] = "XXXXXX",
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_Nodumps_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.StatusKey] = "nodump",
                [Rom.NameKey] = "XXXXXX",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.StatusKey] = "NODUMP",
                [Rom.NameKey] = "XXXXXX",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_Mismatch_False()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = "XXXXXX",
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.EqualTo(other);
            Assert.False(actual);
        }

        [Fact]
        public void EqualTo_Rom_NoSelfSize_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_NoOtherSize_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialCRC_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialMD2_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialMD4_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialMD5_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialRIPEMD128_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialRIPEMD160_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialSHA1_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialSHA256_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialSHA384_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialSHA512_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = string.Empty,
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_PartialSpamSum_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = "XXXXXX",
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Rom_FullMatch_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.NameKey] = "XXXXXX1",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };
            DictionaryBase other = new Rom
            {
                [Rom.NameKey] = "XXXXXX2",
                [Rom.SizeKey] = 12345,
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Other_BothEmpty_True()
        {
            DictionaryBase self = new Sample();
            DictionaryBase other = new Sample();

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        [Fact]
        public void EqualTo_Other_MismatchedCount_False()
        {
            DictionaryBase self = new Sample
            {
                ["KEY1"] = "XXXXXX",
            };
            DictionaryBase other = new Sample
            {
                ["KEY1"] = "XXXXXX",
                ["KEY2"] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.False(actual);
        }

        [Fact]
        public void EqualTo_Other_MismatchedKeys_False()
        {
            DictionaryBase self = new Sample
            {
                ["KEY1"] = "XXXXXX",
            };
            DictionaryBase other = new Sample
            {
                ["KEY2"] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.False(actual);
        }

        [Fact]
        public void EqualTo_Other_MismatchedValues_False()
        {
            DictionaryBase self = new Sample
            {
                ["KEY1"] = "XXXXXX",
            };
            DictionaryBase other = new Sample
            {
                ["KEY1"] = "ZZZZZZ",
            };

            bool actual = self.EqualTo(other);
            Assert.False(actual);
        }

        [Fact]
        public void EqualTo_Other_Matching_True()
        {
            DictionaryBase self = new Sample
            {
                ["KEY1"] = "XXXXXX",
            };
            DictionaryBase other = new Sample
            {
                ["KEY1"] = "XXXXXX",
            };

            bool actual = self.EqualTo(other);
            Assert.True(actual);
        }

        #endregion

        #region HashMatch

        [Fact]
        public void HashMatch_Disk_Mismatch_False()
        {
            Disk self = new Disk
            {
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = string.Empty,
            };
            Disk other = new Disk
            {
                [Disk.MD5Key] = string.Empty,
                [Disk.SHA1Key] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.False(actual);
        }

        [Fact]
        public void HashMatch_Disk_PartialMD5_True()
        {
            Disk self = new Disk
            {
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = string.Empty,
            };
            Disk other = new Disk
            {
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Disk_PartialSHA1_True()
        {
            Disk self = new Disk
            {
                [Disk.MD5Key] = string.Empty,
                [Disk.SHA1Key] = "XXXXXX",
            };
            Disk other = new Disk
            {
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Disk_FullMatch_True()
        {
            Disk self = new Disk
            {
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };
            Disk other = new Disk
            {
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Media_Mismatch_False()
        {
            Media self = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = string.Empty,
            };
            Media other = new Media
            {
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.False(actual);
        }

        [Fact]
        public void HashMatch_Media_PartialMD5_True()
        {
            Media self = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = string.Empty,
            };
            Media other = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Media_PartialSHA1_True()
        {
            Media self = new Media
            {
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = string.Empty,
            };
            Media other = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Media_PartialSHA256_True()
        {
            Media self = new Media
            {
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = string.Empty,
            };
            Media other = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Media_PartialSpamSum_True()
        {
            Media self = new Media
            {
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = "XXXXXX",
            };
            Media other = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Media_FullMatch_True()
        {
            Media self = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };
            Media other = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_Mismatch_False()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = "XXXXXX",
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HashMatch(other);
            Assert.False(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialCRC_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialMD2_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialMD4_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialMD5_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialRIPEMD128_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialRIPEMD160_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialSHA1_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialSHA256_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialSHA384_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialSHA512_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = string.Empty,
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_PartialSpamSum_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = "XXXXXX",
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        [Fact]
        public void HashMatch_Rom_FullMatch_True()
        {
            Rom self = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };
            Rom other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HashMatch(other);
            Assert.True(actual);
        }

        #endregion

        #region HasZeroHash

        [Fact]
        public void HasZeroHash_Disk_NoHash_True()
        {
            DictionaryBase self = new Disk();
            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Disk_NonZeroHash_False()
        {
            DictionaryBase self = new Disk
            {
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };

            bool actual = self.HasZeroHash();
            Assert.False(actual);
        }

        [Fact]
        public void HasZeroHash_Disk_ZeroMD5_True()
        {
            DictionaryBase self = new Disk
            {
                [Disk.MD5Key] = ZeroHash.MD5Str,
                [Disk.SHA1Key] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Disk_ZeroSHA1_True()
        {
            DictionaryBase self = new Disk
            {
                [Disk.MD5Key] = string.Empty,
                [Disk.SHA1Key] = ZeroHash.SHA1Str,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Disk_ZeroAll_True()
        {
            DictionaryBase self = new Disk
            {
                [Disk.MD5Key] = ZeroHash.MD5Str,
                [Disk.SHA1Key] = ZeroHash.SHA1Str,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Media_NoHash_True()
        {
            DictionaryBase self = new Media();
            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Media_NonZeroHash_False()
        {
            DictionaryBase self = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HasZeroHash();
            Assert.False(actual);
        }

        [Fact]
        public void HasZeroHash_Media_ZeroMD5_True()
        {
            DictionaryBase self = new Media
            {
                [Media.MD5Key] = ZeroHash.MD5Str,
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Media_ZeroSHA1_True()
        {
            DictionaryBase self = new Media
            {
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = ZeroHash.SHA1Str,
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Media_ZeroSHA256_True()
        {
            DictionaryBase self = new Media
            {
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = ZeroHash.SHA256Str,
                [Media.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Media_ZeroSpamSum_True()
        {
            DictionaryBase self = new Media
            {
                [Media.MD5Key] = string.Empty,
                [Media.SHA1Key] = string.Empty,
                [Media.SHA256Key] = string.Empty,
                [Media.SpamSumKey] = ZeroHash.SpamSumStr,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Media_ZeroAll_True()
        {
            DictionaryBase self = new Media
            {
                [Media.MD5Key] = ZeroHash.MD5Str,
                [Media.SHA1Key] = ZeroHash.SHA1Str,
                [Media.SHA256Key] = ZeroHash.SHA256Str,
                [Media.SpamSumKey] = ZeroHash.SpamSumStr,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_NoHash_True()
        {
            DictionaryBase self = new Rom();
            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_NonZeroHash_False()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            bool actual = self.HasZeroHash();
            Assert.False(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroCRC_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = ZeroHash.CRC32Str,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroMD2_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = ZeroHash.GetString(HashType.MD2),
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroMD4_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = ZeroHash.GetString(HashType.MD4),
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroMD5_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = ZeroHash.MD5Str,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroRIPEMD128_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = ZeroHash.GetString(HashType.RIPEMD128),
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroRIPEMD160_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = ZeroHash.GetString(HashType.RIPEMD160),
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroSHA1_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = ZeroHash.SHA1Str,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroSHA256_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = ZeroHash.SHA256Str,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroSHA384_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = ZeroHash.SHA384Str,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroSHA512_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = ZeroHash.SHA512Str,
                [Rom.SpamSumKey] = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroSpamSum_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = string.Empty,
                [Rom.MD2Key] = string.Empty,
                [Rom.MD4Key] = string.Empty,
                [Rom.MD5Key] = string.Empty,
                ["RIPEMD128"] = string.Empty,
                ["RIPEMD160"] = string.Empty,
                [Rom.SHA1Key] = string.Empty,
                [Rom.SHA256Key] = string.Empty,
                [Rom.SHA384Key] = string.Empty,
                [Rom.SHA512Key] = string.Empty,
                [Rom.SpamSumKey] = ZeroHash.SpamSumStr,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Rom_ZeroAll_True()
        {
            DictionaryBase self = new Rom
            {
                [Rom.CRCKey] = ZeroHash.CRC32Str,
                [Rom.MD2Key] = ZeroHash.GetString(HashType.MD2),
                [Rom.MD4Key] = ZeroHash.GetString(HashType.MD4),
                [Rom.MD5Key] = ZeroHash.MD5Str,
                ["RIPEMD128"] = ZeroHash.GetString(HashType.RIPEMD128),
                ["RIPEMD160"] = ZeroHash.GetString(HashType.RIPEMD160),
                [Rom.SHA1Key] = ZeroHash.SHA1Str,
                [Rom.SHA256Key] = ZeroHash.SHA256Str,
                [Rom.SHA384Key] = ZeroHash.SHA384Str,
                [Rom.SHA512Key] = ZeroHash.SHA512Str,
                [Rom.SpamSumKey] = ZeroHash.SpamSumStr,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_Other_False()
        {
            DictionaryBase self = new Sample();
            bool actual = self.HasZeroHash();
            Assert.False(actual);
        }

        #endregion

        #region FillMissingHashes

        [Fact]
        public void FillMissingHashes_Disk_BothEmpty()
        {
            DictionaryBase self = new Disk();
            DictionaryBase other = new Disk();

            self.FillMissingHashes(other);
            Assert.Single(self);
        }

        [Fact]
        public void FillMissingHashes_Disk_AllMissing()
        {
            DictionaryBase self = new Disk();
            DictionaryBase other = new Disk
            {
                [Disk.MD5Key] = "XXXXXX",
                [Disk.SHA1Key] = "XXXXXX",
            };

            self.FillMissingHashes(other);
        }

        [Fact]
        public void FillMissingHashes_Media_BothEmpty()
        {
            DictionaryBase self = new Media();
            DictionaryBase other = new Media();
            self.FillMissingHashes(other);
            Assert.Single(self);
        }

        [Fact]
        public void FillMissingHashes_Media_AllMissing()
        {
            DictionaryBase self = new Media();
            DictionaryBase other = new Media
            {
                [Media.MD5Key] = "XXXXXX",
                [Media.SHA1Key] = "XXXXXX",
                [Media.SHA256Key] = "XXXXXX",
                [Media.SpamSumKey] = "XXXXXX",
            };

            self.FillMissingHashes(other);
        }

        [Fact]
        public void FillMissingHashes_Rom_BothEmpty()
        {
            DictionaryBase self = new Rom();
            DictionaryBase other = new Rom();
            self.FillMissingHashes(other);
            Assert.Single(self);
        }

        [Fact]
        public void FillMissingHashes_Rom_AllMissing()
        {
            DictionaryBase self = new Rom();
            DictionaryBase other = new Rom
            {
                [Rom.CRCKey] = "XXXXXX",
                [Rom.MD2Key] = "XXXXXX",
                [Rom.MD4Key] = "XXXXXX",
                [Rom.MD5Key] = "XXXXXX",
                ["RIPEMD128"] = "XXXXXX",
                ["RIPEMD160"] = "XXXXXX",
                [Rom.SHA1Key] = "XXXXXX",
                [Rom.SHA256Key] = "XXXXXX",
                [Rom.SHA384Key] = "XXXXXX",
                [Rom.SHA512Key] = "XXXXXX",
                [Rom.SpamSumKey] = "XXXXXX",
            };

            self.FillMissingHashes(other);
        }

        #endregion
    }
}