using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using Xunit;

namespace SabreTools.DatItems.Test.Formats
{
    public class DiskTests
    {
        #region ConvertToRom

        [Fact]
        public void ConvertToRomTest()
        {
            DiskArea diskArea = new DiskArea();
            diskArea.SetName("XXXXXX");

            Machine machine = new Machine();
            machine.SetFieldValue(Data.Models.Metadata.Machine.NameKey, "XXXXXX");

            Part part = new Part();
            part.SetName("XXXXXX");

            Source source = new Source(0, "XXXXXX");

            Disk disk = new Disk();
            disk.SetName("XXXXXX");
            disk.SetFieldValue(Disk.DiskAreaKey, diskArea);
            disk.SetFieldValue(Data.Models.Metadata.Disk.MergeKey, "XXXXXX");
            disk.SetFieldValue(Data.Models.Metadata.Disk.RegionKey, "XXXXXX");
            disk.SetFieldValue(Data.Models.Metadata.Disk.StatusKey, "good");
            disk.SetFieldValue(Data.Models.Metadata.Disk.OptionalKey, "XXXXXX");
            disk.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, ZeroHash.MD5Str);
            disk.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, ZeroHash.SHA1Str);
            disk.SetFieldValue(DatItem.DupeTypeKey, DupeType.All | DupeType.External);
            disk.SetFieldValue(DatItem.MachineKey, machine);
            disk.SetFieldValue(Disk.PartKey, part);
            disk.SetFieldValue(DatItem.RemoveKey, (bool?)false);
            disk.SetFieldValue(DatItem.SourceKey, source);

            Rom actual = disk.ConvertToRom();

            Assert.Equal("XXXXXX.chd", actual.GetName());
            Assert.Equal("XXXXXX", actual.GetStringFieldValue(Data.Models.Metadata.Rom.MergeKey));
            Assert.Equal("XXXXXX", actual.GetStringFieldValue(Data.Models.Metadata.Rom.RegionKey));
            Assert.Equal("good", actual.GetStringFieldValue(Data.Models.Metadata.Rom.StatusKey));
            Assert.Equal("XXXXXX", actual.GetStringFieldValue(Data.Models.Metadata.Rom.OptionalKey));
            Assert.Equal(ZeroHash.MD5Str, actual.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key));
            Assert.Equal(ZeroHash.SHA1Str, actual.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal(DupeType.All | DupeType.External, actual.GetFieldValue<DupeType>(DatItem.DupeTypeKey));

            DataArea? actualDataArea = actual.GetFieldValue<DataArea?>(Rom.DataAreaKey);
            Assert.NotNull(actualDataArea);
            Assert.Equal("XXXXXX", actualDataArea.GetStringFieldValue(Data.Models.Metadata.DataArea.NameKey));

            Machine? actualMachine = actual.GetMachine();
            Assert.NotNull(actualMachine);
            Assert.Equal("XXXXXX", actualMachine.GetName());

            Assert.Equal(false, actual.GetBoolFieldValue(DatItem.RemoveKey));

            Part? actualPart = actual.GetFieldValue<Part?>(Rom.PartKey);
            Assert.NotNull(actualPart);
            Assert.Equal("XXXXXX", actualPart.GetStringFieldValue(Data.Models.Metadata.Part.NameKey));

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
            Disk self = new Disk();
            Disk other = new Disk();

            self.FillMissingInformation(other);

            Assert.Null(self.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key));
            Assert.Null(self.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key));
        }

        [Fact]
        public void FillMissingInformation_AllMissing()
        {
            Disk self = new Disk();

            Disk other = new Disk();
            other.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, "XXXXXX");
            other.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, "XXXXXX");

            self.FillMissingInformation(other);

            Assert.Equal("XXXXXX", self.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key));
            Assert.Equal("XXXXXX", self.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key));
        }

        #endregion

        #region HasHashes

        [Fact]
        public void HasHashes_NoHash_False()
        {
            Disk self = new Disk();
            bool actual = self.HasHashes();
            Assert.False(actual);
        }

        [Fact]
        public void HasHashes_MD5_True()
        {
            Disk self = new Disk();
            self.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, "XXXXXX");
            self.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, string.Empty);

            bool actual = self.HasHashes();
            Assert.True(actual);
        }

        [Fact]
        public void HasHashes_SHA1_True()
        {
            Disk self = new Disk();
            self.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, string.Empty);
            self.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, "XXXXXX");

            bool actual = self.HasHashes();
            Assert.True(actual);
        }

        [Fact]
        public void HasHashes_All_True()
        {
            Disk self = new Disk();
            self.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, "XXXXXX");
            self.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, "XXXXXX");

            bool actual = self.HasHashes();
            Assert.True(actual);
        }

        #endregion

        #region HasZeroHash

        [Fact]
        public void HasZeroHash_NoHash_True()
        {
            Disk self = new Disk();
            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_NonZeroHash_False()
        {
            Disk self = new Disk();
            self.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, "DEADBEEF");
            self.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, "DEADBEEF");

            bool actual = self.HasZeroHash();
            Assert.False(actual);
        }

        [Fact]
        public void HasZeroHash_ZeroMD5_True()
        {
            Disk self = new Disk();
            self.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, ZeroHash.MD5Str);
            self.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, string.Empty);

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_ZeroSHA1_True()
        {
            Disk self = new Disk();
            self.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, string.Empty);
            self.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, ZeroHash.SHA1Str);

            bool actual = self.HasZeroHash();
            Assert.True(actual);
        }

        [Fact]
        public void HasZeroHash_ZeroAll_True()
        {
            Disk self = new Disk();
            self.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, ZeroHash.MD5Str);
            self.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, ZeroHash.SHA1Str);

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
        [InlineData(ItemKey.CRC, false, false, "00000000")]
        [InlineData(ItemKey.CRC, false, true, "00000000")]
        [InlineData(ItemKey.CRC, true, false, "00000000")]
        [InlineData(ItemKey.CRC, true, true, "00000000")]
        [InlineData(ItemKey.MD2, false, false, "8350e5a3e24c153df2275c9f80692773")]
        [InlineData(ItemKey.MD2, false, true, "8350e5a3e24c153df2275c9f80692773")]
        [InlineData(ItemKey.MD2, true, false, "8350e5a3e24c153df2275c9f80692773")]
        [InlineData(ItemKey.MD2, true, true, "8350e5a3e24c153df2275c9f80692773")]
        [InlineData(ItemKey.MD4, false, false, "31d6cfe0d16ae931b73c59d7e0c089c0")]
        [InlineData(ItemKey.MD4, false, true, "31d6cfe0d16ae931b73c59d7e0c089c0")]
        [InlineData(ItemKey.MD4, true, false, "31d6cfe0d16ae931b73c59d7e0c089c0")]
        [InlineData(ItemKey.MD4, true, true, "31d6cfe0d16ae931b73c59d7e0c089c0")]
        [InlineData(ItemKey.MD5, false, false, "DEADBEEF")]
        [InlineData(ItemKey.MD5, false, true, "DEADBEEF")]
        [InlineData(ItemKey.MD5, true, false, "deadbeef")]
        [InlineData(ItemKey.MD5, true, true, "deadbeef")]
        [InlineData(ItemKey.SHA1, false, false, "DEADBEEF")]
        [InlineData(ItemKey.SHA1, false, true, "DEADBEEF")]
        [InlineData(ItemKey.SHA1, true, false, "deadbeef")]
        [InlineData(ItemKey.SHA1, true, true, "deadbeef")]
        [InlineData(ItemKey.SHA256, false, false, "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
        [InlineData(ItemKey.SHA256, false, true, "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
        [InlineData(ItemKey.SHA256, true, false, "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
        [InlineData(ItemKey.SHA256, true, true, "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")]
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

            DatItem datItem = new Disk();
            datItem.SetFieldValue(Data.Models.Metadata.Disk.MD5Key, "DEADBEEF");
            datItem.SetFieldValue(Data.Models.Metadata.Disk.SHA1Key, "DEADBEEF");

            string actual = datItem.GetKey(bucketedBy, machine, source, lower, norename);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}