using System.IO;
using System.Text;
using Compress.SevenZip.Compress.LZMA;
using Compress.SevenZip.Structure;
using Compress.Utils;
using Zstandard.Net;

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
            ulong outFileCount = (ulong)_localFiles.Count - emptyStreamCount;

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

            //StreamsInfo.PackedStreamsInfo
            if (_compressed)
            {
                _header.StreamsInfo.PackedStreams = new PackedStreamInfo[1];
                _header.StreamsInfo.PackedStreams[0] = new PackedStreamInfo { PackedSize = _packStreamSize };
            }
            else
            {
                _header.StreamsInfo.PackedStreams = new PackedStreamInfo[outFileCount];
                int fileIndex = 0;
                for (int i = 0; i < fileCount; i++)
                {
                    if (_localFiles[i].UncompressedSize == 0)
                    {
                        continue;
                    }
                    _header.StreamsInfo.PackedStreams[fileIndex++] = new PackedStreamInfo { PackedSize = _localFiles[i].UncompressedSize };
                }
            }
            //StreamsInfo.PackedStreamsInfo, no CRC or StreamPosition required

            if (_compressed)
            {
                //StreamsInfo.Folders
                _header.StreamsInfo.Folders = new Folder[1];

                //StreamsInfo.Folders.Coder
                // flags 0x23

                Folder folder = new Folder
                {
                    BindPairs = null,
                    Coders = new[] {
                         new Coder {
                            Method = new byte[] { 3, 1, 1 },
                            NumInStreams = 1,
                            NumOutStreams = 1,
                            Properties = _codeMSbytes
                        }
                    },
                    PackedStreamIndices = new[] { (ulong)0 },
                    UnpackedStreamSizes = new[] { _unpackedStreamSize },
                    UnpackedStreamInfo = new UnpackedStreamInfo[outFileCount],
                    UnpackCRC = null
                };

                switch (_lzmaStream)
                {
                    case LzmaStream _:
                        folder.Coders[0].Method = new byte[] { 3, 1, 1 };
                        break;
                    case ZstandardStream _:
                        folder.Coders[0].Method = new byte[] { 4, 247, 17, 1 };
                        break;
                }


                int fileIndex = 0;
                for (int i = 0; i < fileCount; i++)
                {
                    if (_localFiles[i].UncompressedSize == 0)
                    {
                        continue;
                    }
                    UnpackedStreamInfo unpackedStreamInfo = new UnpackedStreamInfo
                    {
                        UnpackedSize = _localFiles[i].UncompressedSize,
                        Crc = Util.bytestouint(_localFiles[i].CRC)
                    };
                    folder.UnpackedStreamInfo[fileIndex++] = unpackedStreamInfo;
                }
                _header.StreamsInfo.Folders[0] = folder;
            }
            else
            {
                _header.StreamsInfo.Folders = new Folder[outFileCount];
                int fileIndex = 0;
                for (int i = 0; i < fileCount; i++)
                {
                    if (_localFiles[i].UncompressedSize == 0)
                    {
                        continue;
                    }

                    Folder folder = new Folder
                    {
                        BindPairs = null,
                        Coders = new[] {
                            new Coder {
                                Method = new byte[] {0},
                                NumInStreams = 1,
                                NumOutStreams = 1,
                                Properties = null
                            }
                        },
                        PackedStreamIndices = new[] { (ulong)i },
                        UnpackedStreamSizes = new[] { _localFiles[i].UncompressedSize },
                        UnpackCRC = null,

                        UnpackedStreamInfo = new[] {
                            new UnpackedStreamInfo {
                                UnpackedSize = _localFiles[i].UncompressedSize,
                                Crc = Util.bytestouint(_localFiles[i].CRC)
                            }
                        }
                    };

                    _header.StreamsInfo.Folders[fileIndex++] = folder;
                }
            }
        }



        private void CloseWriting7Zip()
        {
            if (_compressed)
            {
                _lzmaStream.Close();
            }

            _packStreamSize = (ulong)_zipFs.Position - _packStreamStart;

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

            StreamsInfo streamsInfo = new StreamsInfo
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