using System.Collections.Generic;
using SabreTools.Hashing;
using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;
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
            var machine = new Machine
            {
                Name = "bar",
                Description = "bar",
            };

            var repMachine = new Machine
            {
                Name = "foo",
                Description = "bar",
            };

            List<string> fields = ["name"];

            Replacer.ReplaceFields(machine, repMachine, fields, false);

            Assert.Equal("foo", machine.Name);
        }

        [Fact]
        public void ReplaceFields_Disk()
        {
            var datItem = new Disk();
            datItem.SetName("foo");
            datItem.MD5 = HashType.MD5.ZeroString;
            datItem.SHA1 = HashType.SHA1.ZeroString;

            var repDatItem = new Disk();
            repDatItem.SetName("bar");
            repDatItem.MD5 = "deadbeef";
            repDatItem.SHA1 = "deadbeef";

            var fields = new Dictionary<string, List<string>>
            {
                ["item"] =
                [
                    "name",
                    "md5",
                    "sha1",
                ]
            };

            Replacer.ReplaceFields(datItem, repDatItem, fields);

            Assert.Equal("bar", datItem.GetName());
            Assert.Equal("deadbeef", datItem.MD5);
            Assert.Equal("deadbeef", datItem.SHA1);
        }

        [Fact]
        public void ReplaceFields_Media()
        {
            var datItem = new Media();
            datItem.SetName("foo");
            datItem.MD5 = HashType.MD5.ZeroString;
            datItem.SHA1 = HashType.SHA1.ZeroString;
            datItem.SHA256 = HashType.SHA256.ZeroString;
            datItem.SpamSum = HashType.SpamSum.ZeroString;

            var repDatItem = new Media();
            repDatItem.SetName("bar");
            datItem.MD5 = "deadbeef";
            datItem.SHA1 = "deadbeef";
            datItem.SHA256 = "deadbeef";
            datItem.SpamSum = "deadbeef";

            var fields = new Dictionary<string, List<string>>
            {
                ["item"] =
                [
                    "name",
                    "md5",
                    "sha1",
                    "sha256",
                    "spamsum",
                ]
            };

            Replacer.ReplaceFields(datItem, repDatItem, fields);

            Assert.Equal("bar", datItem.GetName());
            Assert.Equal("deadbeef", datItem.MD5);
            Assert.Equal("deadbeef", datItem.SHA1);
            Assert.Equal("deadbeef", datItem.SHA256);
            Assert.Equal("deadbeef", datItem.SpamSum);
        }

        [Fact]
        public void ReplaceFields_Rom()
        {
            var datItem = new Rom();
            datItem.SetName("foo");
            datItem.CRC16 = HashType.CRC16.ZeroString;
            datItem.CRC32 = HashType.CRC32.ZeroString;
            datItem.CRC64 = HashType.CRC64.ZeroString;
            datItem.MD2 = HashType.MD2.ZeroString;
            datItem.MD4 = HashType.MD4.ZeroString;
            datItem.MD5 = HashType.MD5.ZeroString;
            datItem.RIPEMD128 = HashType.RIPEMD128.ZeroString;
            datItem.RIPEMD160 = HashType.RIPEMD160.ZeroString;
            datItem.SHA1 = HashType.SHA1.ZeroString;
            datItem.SHA256 = HashType.SHA256.ZeroString;
            datItem.SHA384 = HashType.SHA384.ZeroString;
            datItem.SHA512 = HashType.SHA512.ZeroString;
            datItem.SpamSum = HashType.SpamSum.ZeroString;

            var repDatItem = new Rom();
            repDatItem.SetName("bar");
            datItem.CRC16 = "deadbeef";
            datItem.CRC32 = "deadbeef";
            datItem.CRC64 = "deadbeef";
            datItem.MD2 = "deadbeef";
            datItem.MD4 = "deadbeef";
            datItem.MD5 = "deadbeef";
            datItem.RIPEMD128 = "deadbeef";
            datItem.RIPEMD160 = "deadbeef";
            datItem.SHA1 = "deadbeef";
            datItem.SHA256 = "deadbeef";
            datItem.SHA384 = "deadbeef";
            datItem.SHA512 = "deadbeef";
            datItem.SpamSum = "deadbeef";

            var fields = new Dictionary<string, List<string>>
            {
                ["item"] =
                [
                    "name",
                    "crc16",
                    "crc32",
                    "crc64",
                    "md2",
                    "md4",
                    "md5",
                    "ripemd128",
                    "ripemd160",
                    "sha1",
                    "sha256",
                    "sha384",
                    "sha512",
                    "spamsum",
                ]
            };

            Replacer.ReplaceFields(datItem, repDatItem, fields);

            Assert.Equal("bar", datItem.GetName());
            Assert.Equal("deadbeef", datItem.CRC16);
            Assert.Equal("deadbeef", datItem.CRC32);
            Assert.Equal("deadbeef", datItem.CRC64);
            Assert.Equal("deadbeef", datItem.MD2);
            Assert.Equal("deadbeef", datItem.MD4);
            Assert.Equal("deadbeef", datItem.MD5);
            Assert.Equal("deadbeef", datItem.RIPEMD128);
            Assert.Equal("deadbeef", datItem.RIPEMD160);
            Assert.Equal("deadbeef", datItem.SHA1);
            Assert.Equal("deadbeef", datItem.SHA256);
            Assert.Equal("deadbeef", datItem.SHA384);
            Assert.Equal("deadbeef", datItem.SHA512);
            Assert.Equal("deadbeef", datItem.SpamSum);
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
                ["item"] = ["name"]
            };

            Replacer.ReplaceFields(datItem, repDatItem, fields);

            Assert.Equal("bar", datItem.GetName());
        }

        #endregion
    }
}
