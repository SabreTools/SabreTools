using System.Collections.Generic;
using System.IO;
using System.Text;
using Compress.SevenZip.Compress.LZMA;
using Compress.SevenZip.Compress.ZSTD;
using Compress.SevenZip.Structure;
using Compress.Utils;
using FileInfo = RVIO.FileInfo;
using FileStream = RVIO.FileStream;

namespace Compress.SevenZip
{
    public partial class SevenZ
    {
        private Stream _lzmaStream;

        // Method switched in SevenZipWriteCLose.cs;
        public class outStreams
        {
            public sevenZipCompressType compType;
            public byte[] Method;
            public byte[] _codeMSbytes;
            public ulong _packStreamStart;
            public ulong _packStreamSize;
            public List<UnpackedStreamInfo> _unpackedStreams;
        }

        public List<outStreams> _packedOutStreams;

        public ZipReturn ZipFileCreate(string newFilename)
        {
            return ZipFileCreate(newFilename, sevenZipCompressType.lzma);
        }
        
        public ZipReturn ZipFileCreateFromUncompressedSize(string newFilename, sevenZipCompressType ctype, ulong unCompressedSize)
        {
            return ZipFileCreate(newFilename, ctype, GetDictionarySizeFromUncompressedSize(unCompressedSize));
        }

        private sevenZipCompressType _compType;

        public ZipReturn ZipFileCreate(string newFilename, sevenZipCompressType compressOutput, int dictionarySize = 1 << 24, int numFastBytes = 64)
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

            _packedOutStreams = new List<outStreams>();

            _compType = compressOutput;

#if solid
            outStreams newStream = new()
            {
                _packStreamStart = (ulong)_zipFs.Position,
                compType = compressOutput,
                _packStreamSize = 0,
                _unpackedStreams = new List<UnpackedStreamInfo>()
            };
            switch (compressOutput)
            {
                case sevenZipCompressType.lzma:
                    LzmaEncoderProperties ep = new(true, dictionarySize, numFastBytes);
                    LzmaStream lzs = new(ep, false, _zipFs);
                    
                    newStream.Method = new byte[] { 3, 1, 1 };
                    newStream._codeMSbytes = lzs.Properties;
                    _compressStream = lzs;
                    break;

                case sevenZipCompressType.zstd:
					ZstandardStream zss = new ZstandardStream(_zipFs, 19, true);
                    newStream.Method = new byte[] { 4, 247, 17, 1 };
                    newStream._codeMSbytes = new byte[] { 1, 5, 19, 0, 0 };
                    _compressStream = zss;
                    break;

                case sevenZipCompressType.uncompressed:
                    newStream.Method = new byte[] { 0 };
                    newStream._codeMSbytes = null;
                    _compressStream = _zipFs;
                    break;
            }

            _packedOutStreams.Add(newStream);
#endif
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
            };
            _localFiles.Add(lf);
            unpackedStreamInfo = null;
        }

        public void ZipFileAddZeroLengthFile()
        {
            // do nothing here for 7zip
        }

        UnpackedStreamInfo unpackedStreamInfo;
        public ZipReturn ZipFileOpenWriteStream(bool raw, bool trrntzip, string filename, ulong uncompressedSize, ushort compressionMethod, out Stream stream, TimeStamps dateTime)
        {
            return ZipFileOpenWriteStream(filename, uncompressedSize, out stream);
        }

        private ZipReturn ZipFileOpenWriteStream(string filename, ulong uncompressedSize, out Stream stream)
        {
            // check if we are writing a directory
            if (uncompressedSize == 0 && filename.Substring(filename.Length - 1, 1) == "/")
            {
                ZipFileAddDirectory(filename);
                stream = null;
                return ZipReturn.ZipGood;
            }
            LocalFile lf = new LocalFile
            {
                FileName = filename,
                UncompressedSize = uncompressedSize,
            };
            _localFiles.Add(lf);

            if (uncompressedSize == 0)
            {
                unpackedStreamInfo = null;
                stream = null;
                return ZipReturn.ZipGood;
            }


#if !solid

            outStreams newStream = new outStreams()
            {
                _packStreamStart = (ulong)_zipFs.Position,
                compType = _compType,
                _packStreamSize = 0,
                _unpackedStreams = new List<UnpackedStreamInfo>()
            };

            switch (_compType)
            {
                case sevenZipCompressType.lzma:

                    LzmaEncoderProperties ep = new LzmaEncoderProperties(true, GetDictionarySizeFromUncompressedSize(uncompressedSize), 64);
                    LzmaStream lzs = new LzmaStream(ep, false, _zipFs);
                    newStream.Method = new byte[] { 3, 1, 1 };
                    newStream._codeMSbytes = lzs.Properties;
                    _lzmaStream = lzs;
                    break;

                case sevenZipCompressType.zstd:
					ZstandardStream zss = new ZstandardStream(_zipFs, 19, true);
                    newStream.Method = new byte[] { 4, 247, 17, 1 };
                    newStream._codeMSbytes = new byte[] { 1, 5, 19, 0, 0 };
                    _lzmaStream = zss;
                    break;

                case sevenZipCompressType.uncompressed:
                    newStream.Method = new byte[] { 0 };
                    newStream._codeMSbytes = null;
                    _lzmaStream = _zipFs;
                    break;
            }

            _packedOutStreams.Add(newStream);
#endif

            unpackedStreamInfo = new UnpackedStreamInfo { UnpackedSize = uncompressedSize };
            _packedOutStreams[_packedOutStreams.Count - 1]._unpackedStreams.Add(unpackedStreamInfo);

            stream = _lzmaStream;

            return ZipReturn.ZipGood;
        }


        public ZipReturn ZipFileCloseWriteStream(byte[] crc32)
        {
            LocalFile localFile = _localFiles[_localFiles.Count - 1];
            localFile.CRC = new[] { crc32[3], crc32[2], crc32[1], crc32[0] };

            if (unpackedStreamInfo != null)
                unpackedStreamInfo.Crc = Util.bytestouint(localFile.CRC);

#if !solid
            if (unpackedStreamInfo != null)
            {
                if (_packedOutStreams[_packedOutStreams.Count - 1].compType != sevenZipCompressType.uncompressed)
                {
                    _lzmaStream.Flush();
                    _lzmaStream.Close();
                }
                _packedOutStreams[_packedOutStreams.Count - 1]._packStreamSize = (ulong)_zipFs.Position - _packedOutStreams[_packedOutStreams.Count - 1]._packStreamStart;
            }
#endif
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
