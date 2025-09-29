using System.Collections.Generic;
using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.Hashing;
using Xunit;

namespace SabreTools.DatTools.Test
{
    public class ReplacerTests
    {
        #region ReplaceFields

        // TODO: Add ReplaceFields_DatHeader test

        [Fact]
        public void ReplaceFields_Machine()
        {
            var machine = new Machine();
            machine.SetName("bar");
            machine.SetFieldValue<string?>(Data.Models.Metadata.Machine.DescriptionKey, "bar");

            var repMachine = new Machine();
            machine.SetName("foo");
            machine.SetFieldValue<string?>(Data.Models.Metadata.Machine.DescriptionKey, "bar");

            List<string> fields = [Data.Models.Metadata.Machine.NameKey];

            Replacer.ReplaceFields(machine, repMachine, fields, false);

            Assert.Equal("foo", machine.GetName());
        }

        [Fact]
        public void ReplaceFields_Disk()
        {
            var datItem = new Disk();
            datItem.SetName("foo");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Disk.MD5Key, ZeroHash.MD5Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Disk.SHA1Key, ZeroHash.SHA1Str);

            var repDatItem = new Disk();
            repDatItem.SetName("bar");
            repDatItem.SetFieldValue<string?>(Data.Models.Metadata.Disk.MD5Key, "deadbeef");
            repDatItem.SetFieldValue<string?>(Data.Models.Metadata.Disk.SHA1Key, "deadbeef");

            var fields = new Dictionary<string, List<string>>
            {
                ["item"] =
                [
                    Data.Models.Metadata.Disk.NameKey,
                    Data.Models.Metadata.Disk.MD5Key,
                    Data.Models.Metadata.Disk.SHA1Key,
                ]
            };

            Replacer.ReplaceFields(datItem, repDatItem, fields);

            Assert.Equal("bar", datItem.GetName());
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key));
        }

        [Fact]
        public void ReplaceFields_File()
        {
            var datItem = new File();
            datItem.CRC = ZeroHash.CRC32Str;
            datItem.MD5 = ZeroHash.MD5Str;
            datItem.SHA1 = ZeroHash.SHA1Str;
            datItem.SHA256 = ZeroHash.SHA256Str;

            var repDatItem = new File();
            repDatItem.CRC = TextHelper.NormalizeCRC32("deadbeef");
            repDatItem.MD5 = TextHelper.NormalizeMD5("deadbeef");
            repDatItem.SHA1 = TextHelper.NormalizeSHA1("deadbeef");
            repDatItem.SHA256 = TextHelper.NormalizeSHA256("deadbeef");

            var fields = new Dictionary<string, List<string>>
            {
                ["item"] =
                [
                    Data.Models.Metadata.Rom.CRCKey,
                    Data.Models.Metadata.Rom.MD5Key,
                    Data.Models.Metadata.Rom.SHA1Key,
                    Data.Models.Metadata.Rom.SHA256Key,
                ]
            };

            Replacer.ReplaceFields(datItem, repDatItem, fields);

            Assert.Equal(TextHelper.NormalizeCRC32("deadbeef"), datItem.CRC);
            Assert.Equal(TextHelper.NormalizeMD5("deadbeef"), datItem.MD5);
            Assert.Equal(TextHelper.NormalizeSHA1("deadbeef"), datItem.SHA1);
            Assert.Equal(TextHelper.NormalizeSHA256("deadbeef"), datItem.SHA256);
        }

        [Fact]
        public void ReplaceFields_Media()
        {
            var datItem = new Media();
            datItem.SetName("foo");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Media.MD5Key, ZeroHash.MD5Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Media.SHA1Key, ZeroHash.SHA1Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Media.SHA256Key, ZeroHash.SHA256Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Media.SpamSumKey, ZeroHash.SpamSumStr);

            var repDatItem = new Media();
            repDatItem.SetName("bar");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Media.MD5Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Media.SHA1Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Media.SHA256Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Media.SpamSumKey, "deadbeef");

            var fields = new Dictionary<string, List<string>>
            {
                ["item"] =
                [
                    Data.Models.Metadata.Media.NameKey,
                    Data.Models.Metadata.Media.MD5Key,
                    Data.Models.Metadata.Media.SHA1Key,
                    Data.Models.Metadata.Media.SHA256Key,
                    Data.Models.Metadata.Media.SpamSumKey,
                ]
            };

            Replacer.ReplaceFields(datItem, repDatItem, fields);

            Assert.Equal("bar", datItem.GetName());
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Media.MD5Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Media.SHA1Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Media.SHA256Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Media.SpamSumKey));
        }

        [Fact]
        public void ReplaceFields_Rom()
        {
            var datItem = new Rom();
            datItem.SetName("foo");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, ZeroHash.CRC32Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD2Key, ZeroHash.GetString(HashType.MD2));
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD4Key, ZeroHash.GetString(HashType.MD4));
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD5Key, ZeroHash.MD5Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD128Key, ZeroHash.GetString(HashType.RIPEMD128));
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD160Key, ZeroHash.GetString(HashType.RIPEMD160));
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, ZeroHash.SHA1Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA256Key, ZeroHash.SHA256Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA384Key, ZeroHash.SHA384Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA512Key, ZeroHash.SHA512Str);
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SpamSumKey, ZeroHash.SpamSumStr);

            var repDatItem = new Rom();
            repDatItem.SetName("bar");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.CRCKey, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD2Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD4Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.MD5Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD128Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.RIPEMD160Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA1Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA256Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA384Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SHA512Key, "deadbeef");
            datItem.SetFieldValue<string?>(Data.Models.Metadata.Rom.SpamSumKey, "deadbeef");

            var fields = new Dictionary<string, List<string>>
            {
                ["item"] =
                [
                    Data.Models.Metadata.Rom.NameKey,
                    Data.Models.Metadata.Rom.CRCKey,
                    Data.Models.Metadata.Rom.MD2Key,
                    Data.Models.Metadata.Rom.MD4Key,
                    Data.Models.Metadata.Rom.MD5Key,
                    Data.Models.Metadata.Rom.RIPEMD128Key,
                    Data.Models.Metadata.Rom.RIPEMD160Key,
                    Data.Models.Metadata.Rom.SHA1Key,
                    Data.Models.Metadata.Rom.SHA256Key,
                    Data.Models.Metadata.Rom.SHA384Key,
                    Data.Models.Metadata.Rom.SHA512Key,
                    Data.Models.Metadata.Rom.SpamSumKey,
                ]
            };

            Replacer.ReplaceFields(datItem, repDatItem, fields);

            Assert.Equal("bar", datItem.GetName());
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.MD2Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.MD4Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key));
            Assert.Equal("deadbeef", datItem.GetStringFieldValue(Data.Models.Metadata.Rom.SpamSumKey));
        }

        [Fact]
        public void ReplaceFields_Sample()
        {
            var datItem = new Sample();
            datItem.SetName("foo");

            var repDatItem = new Sample();
            repDatItem.SetName("bar");

            var fields = new Dictionary<string, List<string>>
            {
                ["item"] = [Data.Models.Metadata.Rom.NameKey]
            };

            Replacer.ReplaceFields(datItem, repDatItem, fields);

            Assert.Equal("bar", datItem.GetName());
        }

        #endregion
    }
}