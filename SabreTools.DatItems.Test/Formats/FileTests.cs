using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using Xunit;

namespace SabreTools.DatItems.Test.Formats
{
    public class FileTests
    {
        #region ConvertToRom

        [Fact]
        public void ConvertToRomTest()
        {
            Machine machine = new Machine();
            machine.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "XXXXXX");

            Source source = new Source(0, "XXXXXX");

            File file = new File();
            file.Id = "XXXXXX";
            file.Extension = "XXXXXX";
            file.Size = 12345;
            file.CRC = "DEADBEEF";
            file.MD5 = "DEADBEEF";
            file.SHA1 = "DEADBEEF";
            file.SHA256 = "DEADBEEF";
            file.Format = "XXXXXX";
            file.SetFieldValue(DatItem.DupeTypeKey, DupeType.All | DupeType.External);
            file.SetFieldValue(DatItem.MachineKey, machine);
            file.SetFieldValue(DatItem.RemoveKey, (bool?)false);
            file.SetFieldValue(DatItem.SourceKey, source);
            file.SetFieldValue<Machine>(DatItem.MachineKey, machine);
            file.SetFieldValue<Source>(DatItem.SourceKey, source);

            Rom actual = file.ConvertToRom();

            Assert.Equal("XXXXXX.XXXXXX", actual.GetName());
            Assert.Equal(12345, actual.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey));
            Assert.Equal("deadbeef", actual.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey));
            Assert.Equal("000000000000000000000000deadbeef", actual.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key));
            Assert.Equal("00000000000000000000000000000000deadbeef", actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal("00000000000000000000000000000000000000000000000000000000deadbeef", actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key));
            Assert.Equal(DupeType.All | DupeType.External, actual.GetFieldValue<DupeType>(DatItem.DupeTypeKey));

            Machine? actualMachine = actual.GetMachine();
            Assert.NotNull(actualMachine);
            Assert.Equal("XXXXXX", actualMachine.GetName());

            Assert.Equal(false, actual.GetBoolFieldValue(DatItem.RemoveKey));

            Source? actualSource = actual.GetFieldValue<Source?>(DatItem.SourceKey);
            Assert.NotNull(actualSource);
            Assert.Equal(0, actualSource.Index);
            Assert.Equal("XXXXXX", actualSource.Name);
        }

        #endregion

        #region FillMissingInformation

        [Fact]
        public void FillMissingInformation_BothEmpty()
        {
            File self = new File();
            File other = new File();

            self.FillMissingInformation(other);

            Assert.Null(self.Size);
            Assert.Null(self.CRC);
            Assert.Null(self.MD5);
            Assert.Null(self.SHA1);
            Assert.Null(self.SHA256);
        }

        [Fact]
        public void FillMissingInformation_AllMissing()
        {
            File self = new File();

            File other = new File
            {
                Size = 12345,
                CRC = "DEADBEEF",
                MD5 = "DEADBEEF",
                SHA1 = "DEADBEEF",
                SHA256 = "DEADBEEF",
            };

            self.FillMissingInformation(other);

            Assert.Equal(12345, self.Size);
            Assert.Equal("deadbeef", self.CRC);
            Assert.Equal("000000000000000000000000deadbeef", self.MD5);
            Assert.Equal("00000000000000000000000000000000deadbeef", self.SHA1);
            Assert.Equal("00000000000000000000000000000000000000000000000000000000deadbeef", self.SHA256);
        }

        #endregion

        #region HasHashes

        [Fact]
        public void HasHashes_NoHash_False()
        {
            File self = new File();
            bool actual = self.HasHashes();
            Assert.False(actual);
        }

        [Fact]
        public void HasHashes_CRC_True()
        {
            File self = new File
            {
                CRC = "deadbeef",
                MD5 = string.Empty,
                SHA1 = string.Empty,
                SHA256 = string.Empty,
            };

            bool actual = self.HasHashes();
            Assert.True(actual);
        }

        [Fact]
        public void HasHashes_MD5_True()
        {
            File self = new File
            {
                CRC = string.Empty,
                MD5 = "deadbeef",
                SHA1 = string.Empty,
                SHA256 = string.Empty,
            };

            bool actual = self.HasHashes();
            Assert.True(actual);
        }

        [Fact]
        public void HasHashes_SHA1_True()
        {
            File self = new File
            {
                CRC = string.Empty,
                MD5 = string.Empty,
                SHA1 = "deadbeef",
                SHA256 = string.Empty,
            };

            bool actual = self.HasHashes();
            Assert.True(actual);
        }

        [Fact]
        public void HasHashes_SHA256_True()
        {
            File self = new File
            {
                CRC = string.Empty,
                MD5 = string.Empty,
                SHA1 = string.Empty,
                SHA256 = "deadbeef",
            };

            bool actual = self.HasHashes();
            Assert.True(actual);
        }

        [Fact]
        public void HasHashes_All_True()
        {
            File self = new File
            {
                CRC = "deadbeef",
                MD5 = "deadbeef",
                SHA1 = "deadbeef",
                SHA256 = "deadbeef",
            };

            bool actual = self.HasHashes();
            Assert.True(actual);
        }

        #endregion

        #region HasZeroHash

        [Fact]
        public void HasZeroHash_NoHash_True()
        {
            File self = new File();
            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_NonZeroHash_False()
        {
            File self = new File
            {
                CRC = "deadbeef",
                MD5 = "deadbeef",
                SHA1 = "deadbeef",
                SHA256 = "deadbeef",
            };

            bool actual = self.HasZeroHash();
            Assert.False(actual);
        }

        [Fact]
        public void HasZeroHash_ZeroCRC_True()
        {
            File self = new File
            {
                CRC = ZeroHash.CRC32Str,
                MD5 = string.Empty,
                SHA1 = string.Empty,
                SHA256 = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_ZeroMD5_True()
        {
            File self = new File
            {
                CRC = string.Empty,
                MD5 = ZeroHash.MD5Str,
                SHA1 = string.Empty,
                SHA256 = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_ZeroSHA1_True()
        {
            File self = new File
            {
                CRC = string.Empty,
                MD5 = string.Empty,
                SHA1 = ZeroHash.SHA1Str,
                SHA256 = string.Empty,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_ZeroSHA256_True()
        {
            File self = new File
            {
                CRC = string.Empty,
                MD5 = string.Empty,
                SHA1 = string.Empty,
                SHA256 = ZeroHash.SHA256Str,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_ZeroAll_True()
        {
            File self = new File
            {
                CRC = ZeroHash.CRC32Str,
                MD5 = ZeroHash.MD5Str,
                SHA1 = ZeroHash.SHA1Str,
                SHA256 = ZeroHash.SHA256Str,
            };

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        #endregion

        // TODO: Change when Machine retrieval gets fixed
        #region GetKey

        [Theory]
        [InlineData(ItemKey.NULL, false, false, "")]
        [InlineData(ItemKey.NULL, false, true, "")]
        [InlineData(ItemKey.NULL, true, false, "")]
        [InlineData(ItemKey.NULL, true, true, "")]
        [InlineData(ItemKey.Machine, false, false, "0000000000-Machine")]
        [InlineData(ItemKey.Machine, false, true, "Machine")]
        [InlineData(ItemKey.Machine, true, false, "0000000000-machine")]
        [InlineData(ItemKey.Machine, true, true, "machine")]
        [InlineData(ItemKey.CRC, false, false, "deadbeef")]
        [InlineData(ItemKey.CRC, false, true, "deadbeef")]
        [InlineData(ItemKey.CRC, true, false, "deadbeef")]
        [InlineData(ItemKey.CRC, true, true, "deadbeef")]
        [InlineData(ItemKey.MD2, false, false, "8350e5a3e24c153df2275c9f80692773")]
        [InlineData(ItemKey.MD2, false, true, "8350e5a3e24c153df2275c9f80692773")]
        [InlineData(ItemKey.MD2, true, false, "8350e5a3e24c153df2275c9f80692773")]
        [InlineData(ItemKey.MD2, true, true, "8350e5a3e24c153df2275c9f80692773")]
        [InlineData(ItemKey.MD4, false, false, "31d6cfe0d16ae931b73c59d7e0c089c0")]
        [InlineData(ItemKey.MD4, false, true, "31d6cfe0d16ae931b73c59d7e0c089c0")]
        [InlineData(ItemKey.MD4, true, false, "31d6cfe0d16ae931b73c59d7e0c089c0")]
        [InlineData(ItemKey.MD4, true, true, "31d6cfe0d16ae931b73c59d7e0c089c0")]
        [InlineData(ItemKey.MD5, false, false, "000000000000000000000000deadbeef")]
        [InlineData(ItemKey.MD5, false, true, "000000000000000000000000deadbeef")]
        [InlineData(ItemKey.MD5, true, false, "000000000000000000000000deadbeef")]
        [InlineData(ItemKey.MD5, true, true, "000000000000000000000000deadbeef")]
        [InlineData(ItemKey.SHA1, false, false, "00000000000000000000000000000000deadbeef")]
        [InlineData(ItemKey.SHA1, false, true, "00000000000000000000000000000000deadbeef")]
        [InlineData(ItemKey.SHA1, true, false, "00000000000000000000000000000000deadbeef")]
        [InlineData(ItemKey.SHA1, true, true, "00000000000000000000000000000000deadbeef")]
        [InlineData(ItemKey.SHA256, false, false, "00000000000000000000000000000000000000000000000000000000deadbeef")]
        [InlineData(ItemKey.SHA256, false, true, "00000000000000000000000000000000000000000000000000000000deadbeef")]
        [InlineData(ItemKey.SHA256, true, false, "00000000000000000000000000000000000000000000000000000000deadbeef")]
        [InlineData(ItemKey.SHA256, true, true, "00000000000000000000000000000000000000000000000000000000deadbeef")]
        [InlineData(ItemKey.SHA384, false, false, "38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b")]
        [InlineData(ItemKey.SHA384, false, true, "38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b")]
        [InlineData(ItemKey.SHA384, true, false, "38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b")]
        [InlineData(ItemKey.SHA384, true, true, "38b060a751ac96384cd9327eb1b1e36a21fdb71114be07434c0cc7bf63f6e1da274edebfe76f65fbd51ad2f14898b95b")]
        [InlineData(ItemKey.SHA512, false, false, "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e")]
        [InlineData(ItemKey.SHA512, false, true, "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e")]
        [InlineData(ItemKey.SHA512, true, false, "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e")]
        [InlineData(ItemKey.SHA512, true, true, "cf83e1357eefb8bdf1542850d66d8007d620e4050b5715dc83f4a921d36ce9ce47d0d13c5d85f2b0ff8318d2877eec2f63b931bd47417a81a538327af927da3e")]
        [InlineData(ItemKey.SpamSum, false, false, "3::")]
        [InlineData(ItemKey.SpamSum, false, true, "3::")]
        [InlineData(ItemKey.SpamSum, true, false, "3::")]
        [InlineData(ItemKey.SpamSum, true, true, "3::")]
        public void GetKeyDBTest(ItemKey bucketedBy, bool lower, bool norename, string expected)
        {
            Source source = new Source(0);

            Machine machine = new Machine();
            machine.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "Machine");

            DatItem datItem = new File
            {
                CRC = "DEADBEEF",
                MD5 = "DEADBEEF",
                SHA1 = "DEADBEEF",
                SHA256 = "DEADBEEF",
            };

            string actual = datItem.GetKey(bucketedBy, machine, source, lower, norename);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}