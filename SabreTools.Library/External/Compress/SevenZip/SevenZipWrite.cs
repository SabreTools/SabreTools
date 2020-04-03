using System.IO;
using System.Text;
using Compress.SevenZip.Compress.LZMA;
using Compress.SevenZip.Structure;
using Compress.Utils;
using FileInfo = RVIO.FileInfo;
using FileStream = RVIO.FileStream;

namespace Compress.SevenZip
{
    public partial class SevenZ
    {
        private Stream _lzmaStream;
        private ulong _packStreamStart;
        private ulong _packStreamSize;
        private ulong _unpackedStreamSize;
        private byte[] _codeMSbytes;


        public ZipReturn ZipFileCreate(string newFilename)
        {
            return ZipFileCreate(newFilename, true);
        }


        public ZipReturn ZipFileCreateFromUncompressedSize(string newFilename, ulong unCompressedSize)
        {
            return ZipFileCreate(newFilename, true, GetDictionarySizeFromUncompressedSize(unCompressedSize));
        }

        public ZipReturn ZipFileCreate(string newFilename, bool compressOutput, int dictionarySize = 1 << 24, int numFastBytes = 64)
        {
            if (ZipOpen != ZipOpenType.Closed)
            {
                return ZipReturn.ZipFileAlreadyOpen;
            }

            DirUtil.CreateDirForFile(newFilename);
            _zipFileInfo = new FileInfo(newFilename);

            int errorCode = FileStream.OpenFileWrite(newFilename, out _zipFs);
            if (errorCode != 0)
            {
                ZipFileClose();
                return ZipReturn.ZipErrorOpeningFile;
            }
            ZipOpen = ZipOpenType.OpenWrite;

            _signatureHeader = new SignatureHeader();
            _header = new Header();

            using (BinaryWriter bw = new BinaryWriter(_zipFs, Encoding.UTF8, true))
            {
                _signatureHeader.Write(bw);
            }

            _baseOffset = _zipFs.Position;

            _compressed = compressOutput;

            _unpackedStreamSize = 0;
            if (_compressed)
            {
                LzmaEncoderProperties ep = new LzmaEncoderProperties(true, dictionarySize, numFastBytes);
                LzmaStream lzs = new LzmaStream(ep, false, _zipFs);
                _codeMSbytes = lzs.Properties;
                _lzmaStream = lzs;


                /*
                ZstandardStream zss = new ZstandardStream(_zipFs, 22, true);
                _codeMSbytes = new byte[] { 1, 4, 18, 0, 0 };
                _lzmaStream = zss;
                */
                _packStreamStart = (ulong)_zipFs.Position;
            }

            return ZipReturn.ZipGood;
        }

        public void ZipFileAddDirectory(string filename)
        {
            string fName = filename;
            if (fName.Substring(fName.Length - 1, 1) == @"/")
                fName = fName.Substring(0, fName.Length - 1);

            LocalFile lf = new LocalFile
            {
                FileName = fName,
                UncompressedSize = 0,
                IsDirectory = true,
                StreamOffset = 0
            };
            _localFiles.Add(lf);
        }

        public void ZipFileAddZeroLengthFile()
        {
            // do nothing here for 7zip
        }

        public ZipReturn ZipFileOpenWriteStream(bool raw, bool trrntzip, string filename, ulong uncompressedSize, ushort compressionMethod, out Stream stream)
        {
            return ZipFileOpenWriteStream(filename, uncompressedSize, out stream);
        }

        private ZipReturn ZipFileOpenWriteStream(string filename, ulong uncompressedSize, out Stream stream)
        {
            LocalFile lf = new LocalFile
            {
                FileName = filename,
                UncompressedSize = uncompressedSize,
                StreamOffset = (ulong)(_zipFs.Position - _signatureHeader.BaseOffset)
            };
            if (uncompressedSize == 0 && filename.Substring(filename.Length - 1, 1) == "/")
            {
                lf.FileName = filename.Substring(0, filename.Length - 1);
                lf.IsDirectory = true;
            }

            _unpackedStreamSize += uncompressedSize;

            _localFiles.Add(lf);
            stream = _compressed ? _lzmaStream : _zipFs;
            return ZipReturn.ZipGood;
        }


        public ZipReturn ZipFileCloseWriteStream(byte[] crc32)
        {
            _localFiles[_localFiles.Count - 1].CRC = new[] { crc32[3], crc32[2], crc32[1], crc32[0] };
            return ZipReturn.ZipGood;
        }


        private static readonly int[] DictionarySizes =
        {
            0x10000,
            0x18000,
            0x20000,
            0x30000,
            0x40000,
            0x60000,
            0x80000,
            0xc0000,

            0x100000,
            0x180000,
            0x200000,
            0x300000,
            0x400000,
            0x600000,
            0x800000,
            0xc00000,

            0x1000000,
            0x1800000,
            0x2000000,
            0x3000000,
            0x4000000,
            0x6000000
        };


        private static int GetDictionarySizeFromUncompressedSize(ulong unCompressedSize)
        {
            foreach (int v in DictionarySizes)
            {
                if ((ulong)v >= unCompressedSize)
                    return v;
            }

            return DictionarySizes[DictionarySizes.Length - 1];
        }
    }
}