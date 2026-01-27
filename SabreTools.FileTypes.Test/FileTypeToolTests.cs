using System;
using System.IO;
using SabreTools.Hashing;
using SabreTools.IO.Extensions;
using Xunit;

namespace SabreTools.FileTypes.Test
{
    public class FileTypeToolTests
    {
        private static readonly HashType[] _defaultHashes = [HashType.CRC32, HashType.MD5, HashType.SHA1];

        private const long _expectedSize = 125;
        private const string _expectedCrc = "ba02a660";
        private const string _expectedMd5 = "b722871eaa950016296184d026c5dec9";
        private const string _expectedSha1 = "eea1ee2d801d830c4bdad4df3c8da6f9f52d1a9f";

        #region GetInfo

        [Fact]
        public void GetInfo_File_EmptyPath()
        {
            string input = string.Empty;
            BaseFile actual = FileTypeTool.GetInfo(input, _defaultHashes);

            Assert.Null(actual.Filename);
            Assert.Null(actual.Size);
            Assert.Null(actual.CRC);
            Assert.Null(actual.MD5);
            Assert.Null(actual.SHA1);
        }

        [Fact]
        public void GetInfo_File_NotExists()
        {
            string input = "INVALID";
            BaseFile actual = FileTypeTool.GetInfo(input, _defaultHashes);

            Assert.Null(actual.Filename);
            Assert.Null(actual.Size);
            Assert.Null(actual.CRC);
            Assert.Null(actual.MD5);
            Assert.Null(actual.SHA1);
        }

        [Fact]
        public void GetInfo_File_NoHeader()
        {
            string input = Path.Combine(Environment.CurrentDirectory, "TestData", "file-to-hash.bin");
            BaseFile actual = FileTypeTool.GetInfo(input, _defaultHashes);

            Assert.Equal("file-to-hash.bin", actual.Filename);
            Assert.Equal(_expectedSize, actual.Size);
            Assert.Equal(_expectedCrc, actual.CRC.ToHexString());
            Assert.Equal(_expectedMd5, actual.MD5.ToHexString());
            Assert.Equal(_expectedSha1, actual.SHA1.ToHexString());
        }

        [Fact]
        public void GetInfo_File_NullHeader()
        {
            string input = Path.Combine(Environment.CurrentDirectory, "TestData", "file-to-hash.bin");
            string? header = null;
            BaseFile actual = FileTypeTool.GetInfo(input, _defaultHashes, header);

            Assert.Equal("file-to-hash.bin", actual.Filename);
            Assert.Equal(_expectedSize, actual.Size);
            Assert.Equal(_expectedCrc, actual.CRC.ToHexString());
            Assert.Equal(_expectedMd5, actual.MD5.ToHexString());
            Assert.Equal(_expectedSha1, actual.SHA1.ToHexString());
        }

        [Fact]
        public void GetInfo_File_EmptyHeader()
        {
            string input = Path.Combine(Environment.CurrentDirectory, "TestData", "file-to-hash.bin");
            string? header = string.Empty;
            BaseFile actual = FileTypeTool.GetInfo(input, _defaultHashes, header);

            Assert.Equal("file-to-hash.bin", actual.Filename);
            Assert.Equal(_expectedSize, actual.Size);
            Assert.Equal(_expectedCrc, actual.CRC.ToHexString());
            Assert.Equal(_expectedMd5, actual.MD5.ToHexString());
            Assert.Equal(_expectedSha1, actual.SHA1.ToHexString());
        }

        [Fact]
        public void GetInfo_File_NonMatchingHeader()
        {
            string input = Path.Combine(Environment.CurrentDirectory, "TestData", "file-to-hash.bin");
            string? header = "nes";
            BaseFile actual = FileTypeTool.GetInfo(input, _defaultHashes, header);

            Assert.Equal("file-to-hash.bin", actual.Filename);
            Assert.Equal(_expectedSize, actual.Size);
            Assert.Equal(_expectedCrc, actual.CRC.ToHexString());
            Assert.Equal(_expectedMd5, actual.MD5.ToHexString());
            Assert.Equal(_expectedSha1, actual.SHA1.ToHexString());
        }

        [Fact]
        public void GetInfo_Stream_Null()
        {
            Stream? input = null;
            BaseFile actual = FileTypeTool.GetInfo(input, _defaultHashes);

            Assert.Null(actual.Size);
            Assert.Null(actual.CRC);
            Assert.Null(actual.MD5);
            Assert.Null(actual.SHA1);
        }

        [Fact]
        public void GetInfo_Stream_Valid()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "TestData", "file-to-hash.bin");
            Stream? input = File.OpenRead(path);
            BaseFile actual = FileTypeTool.GetInfo(input, _defaultHashes);

            Assert.Equal(_expectedSize, actual.Size);
            Assert.Equal(_expectedCrc, actual.CRC.ToHexString());
            Assert.Equal(_expectedMd5, actual.MD5.ToHexString());
            Assert.Equal(_expectedSha1, actual.SHA1.ToHexString());
        }

        #endregion
    }
}
