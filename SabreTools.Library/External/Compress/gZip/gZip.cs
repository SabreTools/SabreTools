using System;
using System.IO;
using System.Text;
using Compress.ZipFile.ZLib;
using Directory = RVIO.Directory;
using FileInfo = RVIO.FileInfo;
using FileStream = RVIO.FileStream;
using Path = RVIO.Path;


namespace Compress.gZip
{
    public class gZip : ICompress
    {
        private FileInfo _zipFileInfo;
        private Stream _zipFs;
        private Stream _compressionStream;

        public byte[] CRC { get; private set; }
        public ulong UnCompressedSize { get; private set; }
        public ulong CompressedSize { get; private set; }

        private long headerStartPos;
        private long dataStartPos;

        public int LocalFilesCount()
        {
            return 1;
        }

        public string Filename(int i)
        {
            return Path.GetFileName(ZipFilename);
        }

        public ulong? LocalHeader(int i)
        {
            return 0;
        }

        public ulong UncompressedSize(int i)
        {
            return UnCompressedSize;
        }

        public byte[] CRC32(int i)
        {
            return CRC;
        }

        public bool IsDirectory(int i)
        {
            return false;
        }

        public ZipOpenType ZipOpen { get; private set; }

        public ZipReturn ZipFileOpen(string newFilename, long timestamp = -1, bool readHeaders = true)
        {
            ZipFileClose();
            ZipStatus = ZipStatus.None;

            try
            {
                if (!RVIO.File.Exists(newFilename))
                {
                    ZipFileClose();
                    return ZipReturn.ZipErrorFileNotFound;
                }
                _zipFileInfo = new FileInfo(newFilename);
                if (timestamp != -1 && _zipFileInfo.LastWriteTime != timestamp)
                {
                    ZipFileClose();
                    return ZipReturn.ZipErrorTimeStamp;
                }
                int errorCode = FileStream.OpenFileRead(newFilename, out _zipFs);
                if (errorCode != 0)
                {
                    ZipFileClose();
                    if (errorCode == 32)
                    {
                        return ZipReturn.ZipFileLocked;
                    }
                    return ZipReturn.ZipErrorOpeningFile;
                }
            }
            catch (PathTooLongException)
            {
                ZipFileClose();
                return ZipReturn.ZipFileNameToLong;
            }
            catch (IOException)
            {
                ZipFileClose();
                return ZipReturn.ZipErrorOpeningFile;
            }
            ZipOpen = ZipOpenType.OpenRead;

            if (!readHeaders)
            {
                return ZipReturn.ZipGood;
            }
            return ZipFileReadHeaders();
        }

        public ZipReturn ZipFileOpen(Stream inStream)
        {
            ZipFileClose();
            ZipStatus = ZipStatus.None;
            _zipFileInfo = null;
            _zipFs = inStream;

            ZipOpen = ZipOpenType.OpenRead;
            return ZipFileReadHeaders();
        }

        private ZipReturn ZipFileReadHeaders()
        {
            using (BinaryReader zipBr = new BinaryReader(_zipFs, Encoding.UTF8, true))
            {

                byte ID1 = zipBr.ReadByte();
                byte ID2 = zipBr.ReadByte();

                if ((ID1 != 0x1f) || (ID2 != 0x8b))
                {
                    _zipFs.Close();
                    return ZipReturn.ZipSignatureError;
                }

                byte CM = zipBr.ReadByte();
                if (CM != 8)
                {
                    _zipFs.Close();
                    return ZipReturn.ZipUnsupportedCompression;
                }

                byte FLG = zipBr.ReadByte();


                uint MTime = zipBr.ReadUInt32();
                byte XFL = zipBr.ReadByte();
                byte OS = zipBr.ReadByte();

                ExtraData = null;
                //if FLG.FEXTRA set
                if ((FLG & 0x4) == 0x4)
                {
                    int XLen = zipBr.ReadInt16();
                    ExtraData = zipBr.ReadBytes(XLen);

                    switch (XLen)
                    {
                        case 12:
                            CRC = new byte[4];
                            Array.Copy(ExtraData, 0, CRC, 0, 4);
                            UnCompressedSize = BitConverter.ToUInt64(ExtraData, 4);
                            break;
                        case 28:
                            CRC = new byte[4];
                            Array.Copy(ExtraData, 16, CRC, 0, 4);
                            UnCompressedSize = BitConverter.ToUInt64(ExtraData, 20);
                            break;
                        case 77:
                            CRC = new byte[4];
                            Array.Copy(ExtraData, 16, CRC, 0, 4);
                            UnCompressedSize = BitConverter.ToUInt64(ExtraData, 20);
                            break;
                    }
                }

                //if FLG.FNAME set
                if ((FLG & 0x8) == 0x8)
                {
                    int XLen = zipBr.ReadInt16();
                    byte[] bytes = zipBr.ReadBytes(XLen);
                }

                //if FLG.FComment set
                if ((FLG & 0x10) == 0x10)
                {
                    int XLen = zipBr.ReadInt16();
                    byte[] bytes = zipBr.ReadBytes(XLen);
                }

                //if FLG.FHCRC set
                if ((FLG & 0x2) == 0x2)
                {
                    uint crc16 = zipBr.ReadUInt16();
                }

                CompressedSize = (ulong)(_zipFs.Length - _zipFs.Position) - 8;

                dataStartPos = _zipFs.Position;

                _zipFs.Position = _zipFs.Length - 8;
                byte[] gzcrc = zipBr.ReadBytes(4);
                uint gzLength = zipBr.ReadUInt32();

                if (CRC != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (gzcrc[3 - i] == CRC[i])
                        {
                            continue;
                        }

                        _zipFs.Close();
                        return ZipReturn.ZipDecodeError;
                    }
                }
                else
                {
                    CRC = new[] { gzcrc[3], gzcrc[2], gzcrc[1], gzcrc[0] };
                }

                if (UnCompressedSize != 0)
                {
                    if (gzLength != (UnCompressedSize & 0xffffffff))
                    {
                        _zipFs.Close();
                        return ZipReturn.ZipDecodeError;
                    }
                }

                return ZipReturn.ZipGood;
            }
        }

        public void ZipFileClose()
        {
            if (ZipOpen == ZipOpenType.Closed)
            {
                return;
            }

            if (ZipOpen == ZipOpenType.OpenRead)
            {
                if (_zipFs != null)
                {
                    _zipFs.Close();
                    _zipFs.Dispose();
                }
                ZipOpen = ZipOpenType.Closed;
                return;
            }

        }

        public ZipReturn ZipFileOpenReadStream(int index, out Stream stream, out ulong streamSize)
        {
            ZipFileCloseReadStream();

            _zipFs.Position = dataStartPos;

            _compressionStream = new ZlibBaseStream(_zipFs, CompressionMode.Decompress, CompressionLevel.Default, ZlibStreamFlavor.DEFLATE, true);
            stream = _compressionStream;
            streamSize = UnCompressedSize;

            return ZipReturn.ZipGood;
        }

        public bool hasAltFileHeader;


        public byte[] ExtraData;

        public ZipReturn ZipFileOpenWriteStream(bool raw, bool trrntzip, string filename, ulong unCompressedSize, ushort compressionMethod, uint? datetime, out Stream stream)
        {
            using (BinaryWriter zipBw = new BinaryWriter(_zipFs, Encoding.UTF8, true))
            {
                UnCompressedSize = unCompressedSize;

                zipBw.Write((byte)0x1f); // ID1 = 0x1f
                zipBw.Write((byte)0x8b); // ID2 = 0x8b
                zipBw.Write((byte)0x08); // CM  = 0x08
                zipBw.Write((byte)0x04); // FLG = 0x04
                zipBw.Write((uint)0); // MTime = 0
                zipBw.Write((byte)0x00); // XFL = 0x00
                zipBw.Write((byte)0xff); // OS  = 0x00

                if (ExtraData == null)
                {
                    zipBw.Write((short)12);
                    headerStartPos = zipBw.BaseStream.Position;
                    zipBw.Write(new byte[12]);
                }
                else
                {
                    zipBw.Write((short)ExtraData.Length); // XLEN 16+4+8+1+16+20+4+8
                    headerStartPos = zipBw.BaseStream.Position;
                    zipBw.Write(ExtraData);
                }


                dataStartPos = zipBw.BaseStream.Position;
                stream = raw
                    ? _zipFs
                    : new ZlibBaseStream(_zipFs, CompressionMode.Compress, CompressionLevel.BestCompression, ZlibStreamFlavor.DEFLATE, true);

                zipBw.Flush();
                zipBw.Close();
            }
            return ZipReturn.ZipGood;
        }

        public ZipReturn ZipFileCloseReadStream()
        {

            if (_compressionStream == null)
                return ZipReturn.ZipGood;
            if (_compressionStream is ZlibBaseStream dfStream)
            {
                dfStream.Close();
                dfStream.Dispose();
            }
            _compressionStream = null;

            return ZipReturn.ZipGood;
        }

        public ZipStatus ZipStatus { get; private set; }

        public string ZipFilename => _zipFileInfo != null ? _zipFileInfo.FullName : string.Empty;

        public long TimeStamp => _zipFileInfo?.LastWriteTime ?? 0;

        public void ZipFileAddZeroLengthFile()
        {
            throw new NotImplementedException();
        }

        public ZipReturn ZipFileCreate(string newFilename)
        {
            if (ZipOpen != ZipOpenType.Closed)
            {
                return ZipReturn.ZipFileAlreadyOpen;
            }

            CreateDirForFile(newFilename);
            _zipFileInfo = new FileInfo(newFilename);

            int errorCode = FileStream.OpenFileWrite(newFilename, out _zipFs);
            if (errorCode != 0)
            {
                ZipFileClose();
                return ZipReturn.ZipErrorOpeningFile;
            }
            ZipOpen = ZipOpenType.OpenWrite;
            return ZipReturn.ZipGood;
        }


        public ZipReturn ZipFileCloseWriteStream(byte[] crc32)
        {
            using (BinaryWriter zipBw = new BinaryWriter(_zipFs, Encoding.UTF8, true))
            {
                CompressedSize = (ulong)(zipBw.BaseStream.Position - dataStartPos);

                zipBw.Write(CRC[3]);
                zipBw.Write(CRC[2]);
                zipBw.Write(CRC[1]);
                zipBw.Write(CRC[0]);
                zipBw.Write((uint)UnCompressedSize);

                long endpos = _zipFs.Position;

                _zipFs.Position = headerStartPos;

                if (ExtraData == null)
                {
                    zipBw.Write(CRC); // 4 bytes
                    zipBw.Write(UnCompressedSize); // 8 bytes
                }
                else
                {
                    zipBw.Write(ExtraData);
                }

                _zipFs.Position = endpos;

                zipBw.Flush();
                zipBw.Close();
            }

            _zipFs.Close();

            return ZipReturn.ZipGood;
        }

        public ZipReturn ZipFileRollBack()
        {
            _zipFs.Position = dataStartPos;
            return ZipReturn.ZipGood;
        }

        public void ZipFileCloseFailed()
        {
            if (ZipOpen == ZipOpenType.Closed)
            {
                return;
            }

            if (ZipOpen == ZipOpenType.OpenRead)
            {
                if (_zipFs != null)
                {
                    _zipFs.Close();
                    _zipFs.Dispose();
                }
                ZipOpen = ZipOpenType.Closed;
                return;
            }

            _zipFs.Flush();
            _zipFs.Close();
            _zipFs.Dispose();
            RVIO.File.Delete(_zipFileInfo.FullName);
            _zipFileInfo = null;
            ZipOpen = ZipOpenType.Closed;
        }


        private static void CreateDirForFile(string sFilename)
        {
            string strTemp = Path.GetDirectoryName(sFilename);

            if (string.IsNullOrEmpty(strTemp))
            {
                return;
            }

            if (Directory.Exists(strTemp))
            {
                return;
            }


            while (strTemp.Length > 0 && !Directory.Exists(strTemp))
            {
                int pos = strTemp.LastIndexOf(Path.DirectorySeparatorChar);
                if (pos < 0)
                {
                    pos = 0;
                }
                strTemp = strTemp.Substring(0, pos);
            }

            while (sFilename.IndexOf(Path.DirectorySeparatorChar, strTemp.Length + 1) > 0)
            {
                strTemp = sFilename.Substring(0, sFilename.IndexOf(Path.DirectorySeparatorChar, strTemp.Length + 1));
                Directory.CreateDirectory(strTemp);
            }
        }

    }
}