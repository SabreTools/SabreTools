using System.IO;
using System.Text;
using Compress.SevenZip.Compress.LZMA;
using Compress.SevenZip.Compress.ZSTD;
using Compress.SevenZip.Structure;
using Compress.Utils;

// RVWorld-master\Compress\SevenZip\SevenZipWriteClose.cs
namespace Compress.SevenZip
{
    public partial class SevenZ
    {
        private void Create7ZStructure()
        {
            int fileCount = _localFiles.Count;

            //FileInfo
            _header.FileInfo = new Structure.FileInfo
            {
                Names = new string[fileCount]
            };

            ulong emptyStreamCount = 0;
            ulong emptyFileCount = 0;
            for (int i = 0; i < fileCount; i++)
            {
                _header.FileInfo.Names[i] = _localFiles[i].FileName;

                if (_localFiles[i].UncompressedSize != 0)
                {
                    continue;
                }

                if (!_localFiles[i].IsDirectory)
                {
                    emptyFileCount += 1;
                }

                emptyStreamCount += 1;
            }

            _header.FileInfo.EmptyStreamFlags = null;
            _header.FileInfo.EmptyFileFlags = null;
            _header.FileInfo.Attributes = null;

            if (emptyStreamCount > 0)
            {
                if (emptyStreamCount != emptyFileCount) //then we found directories and need to set the attributes
                {
                    _header.FileInfo.Attributes = new uint[fileCount];
                }

                if (emptyFileCount > 0)
                {
                    _header.FileInfo.EmptyFileFlags = new bool[emptyStreamCount];
                }

                emptyStreamCount = 0;
                _header.FileInfo.EmptyStreamFlags = new bool[fileCount];
                for (int i = 0; i < fileCount; i++)
                {
                    if (_localFiles[i].UncompressedSize != 0)
                    {
                        continue;
                    }

                    if (_localFiles[i].IsDirectory)
                    {
                        if (_header.FileInfo.Attributes != null)
                            _header.FileInfo.Attributes[i] = 0x10; // set attributes to directory
                    }
                    else
                    {
                        if (_header.FileInfo.EmptyFileFlags != null)
                            _header.FileInfo.EmptyFileFlags[emptyStreamCount] = true; // set empty file flag
                    }

                    _header.FileInfo.EmptyStreamFlags[i] = true;
                    emptyStreamCount += 1;
                }
            }


            //StreamsInfo

            _header.StreamsInfo = new StreamsInfo { PackPosition = 0 };

            _header.StreamsInfo.PackedStreams = new PackedStreamInfo[_packedOutStreams.Count];
            for (int i = 0; i < _packedOutStreams.Count; i++)
            {
                _header.StreamsInfo.PackedStreams[i] = new PackedStreamInfo { PackedSize = _packedOutStreams[i]._packStreamSize };
            }

            _header.StreamsInfo.Folders = new Folder[_packedOutStreams.Count];
            for (int i = 0; i < _packedOutStreams.Count; i++)
            {
                ulong _unpackedStreamSize = 0;
                foreach (UnpackedStreamInfo v in _packedOutStreams[i]._unpackedStreams)
                    _unpackedStreamSize += v.UnpackedSize;

                _header.StreamsInfo.Folders[i] = new Folder() 
                {
                    BindPairs = null,
                    Coders = new Coder[] {
                         new Coder {
                            Method = _packedOutStreams[i].Method,
                            NumInStreams = 1,
                            NumOutStreams = 1,
                            Properties = _packedOutStreams[i]._codeMSbytes
                        }
                    },
                    PackedStreamIndices = new ulong[] { (ulong)i },
                    UnpackedStreamSizes = new ulong[] { _unpackedStreamSize },
                    UnpackedStreamInfo = _packedOutStreams[i]._unpackedStreams.ToArray(),
                    UnpackCRC = null
                };
            }
        }

        private void CloseWriting7Zip()
        {
#if solid
            if (_compressed != sevenZipCompressType.uncompressed)
            {
                _lzmaStream.Flush();
                _lzmaStream.Close();
            }

            _packStreamSize = (ulong)_zipFs.Position - _packStreamStart;
#endif
            Create7ZStructure();

            byte[] newHeaderByte;
            using (Stream headerMem = new MemoryStream())
            {
                using (BinaryWriter headerBw = new BinaryWriter(headerMem, Encoding.UTF8, true))
                {
                    _header.WriteHeader(headerBw);

                    newHeaderByte = new byte[headerMem.Length];
                    headerMem.Position = 0;
                    headerMem.Read(newHeaderByte, 0, newHeaderByte.Length);
                }
            }

            uint mainHeaderCRC = CRC.CalculateDigest(newHeaderByte, 0, (uint)newHeaderByte.Length);

            #region Header Compression
            long packedHeaderPos = _zipFs.Position;
            LzmaEncoderProperties ep = new LzmaEncoderProperties(true, GetDictionarySizeFromUncompressedSize((ulong)newHeaderByte.Length), 64);
            LzmaStream lzs = new LzmaStream(ep, false, _zipFs);
            byte[] lzmaStreamProperties = lzs.Properties;
            lzs.Write(newHeaderByte, 0, newHeaderByte.Length);
            lzs.Close();

            StreamsInfo streamsInfo = new StreamsInfo()
            {
                PackPosition = (ulong)(packedHeaderPos - _baseOffset),
                Folders = new[] {
                        new Folder {
                            BindPairs = new BindPair[0],
                            Coders = new [] {
                                new Coder {
                                    Method = new byte[] { 3, 1, 1 },
                                    NumInStreams = 1,
                                    NumOutStreams = 1,
                                    Properties = lzmaStreamProperties
                                }
                            },
                            UnpackedStreamSizes = new[] {(ulong) newHeaderByte.Length},
                            UnpackCRC = mainHeaderCRC
                        }
                    },
                PackedStreams = new[] {
                        new PackedStreamInfo
                        {
                            PackedSize = (ulong)(_zipFs.Position - packedHeaderPos),
                            StreamPosition = 0
                        }
                    }
            };

            using (Stream headerMem = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(headerMem, Encoding.UTF8, true))
                {
                    bw.Write((byte)HeaderProperty.kEncodedHeader);
                    streamsInfo.WriteHeader(bw);

                    newHeaderByte = new byte[headerMem.Length];
                    headerMem.Position = 0;
                    headerMem.Read(newHeaderByte, 0, newHeaderByte.Length);

                }
            }
            mainHeaderCRC = CRC.CalculateDigest(newHeaderByte, 0, (uint)newHeaderByte.Length);
            #endregion


            using (BinaryWriter bw = new BinaryWriter(_zipFs, Encoding.UTF8, true))
            {
                ulong headerPosition = (ulong)_zipFs.Position + 32; //tzip header is 32 bytes
                WriteRomVault7Zip(bw, headerPosition, (ulong)newHeaderByte.Length, mainHeaderCRC);

                _zipFs.Write(newHeaderByte, 0, newHeaderByte.Length);
                _signatureHeader.WriteFinal(bw, headerPosition, (ulong)newHeaderByte.Length, mainHeaderCRC);
            }
            _zipFs.Flush();
            _zipFs.Close();
            _zipFs.Dispose();
        }

    }
}
