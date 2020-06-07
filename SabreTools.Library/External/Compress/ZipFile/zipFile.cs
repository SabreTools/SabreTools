using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Compress.Utils;
using Compress.ZipFile.ZLib;
using Directory = RVIO.Directory;
using FileInfo = RVIO.FileInfo;
using FileStream = RVIO.FileStream;
using Path = RVIO.Path;

// UInt16 = ushort
// UInt32 = uint
// ULong = ulong

namespace Compress.ZipFile
{
    public class ZipFile : ICompress
    {
        private const uint LocalFileHeaderSignature = 0x04034b50;
        private const uint CentralDirectoryHeaderSigniature = 0x02014b50;
        private const uint EndOfCentralDirSignature = 0x06054b50;
        private const uint Zip64EndOfCentralDirSignatue = 0x06064b50;
        private const uint Zip64EndOfCentralDirectoryLocator = 0x07064b50;
        private readonly List<LocalFile> _localFiles = new List<LocalFile>();


        private FileInfo _zipFileInfo;

        private ulong _centerDirStart;
        private ulong _centerDirSize;
        private ulong _endOfCenterDir64;

        private byte[] _fileComment;
        private Stream _zipFs;
        private Stream _compressionStream;

        private uint _localFilesCount;

        private bool _zip64;

        public string ZipFilename => _zipFileInfo != null ? _zipFileInfo.FullName : string.Empty;

        public long TimeStamp => _zipFileInfo?.LastWriteTime ?? 0;

        public ZipOpenType ZipOpen { get; private set; }


        public ZipStatus ZipStatus { get; set; }

        public int LocalFilesCount()
        {
            return _localFiles.Count;
        }

        public string Filename(int i)
        {
            return _localFiles[i].FileName;
        }

        public ulong UncompressedSize(int i)
        {
            return _localFiles[i].UncompressedSize;
        }

        public ulong? LocalHeader(int i)
        {
            return (_localFiles[i].GeneralPurposeBitFlag & 8) == 0 ? (ulong?)_localFiles[i].RelativeOffsetOfLocalHeader : null;
        }

        public byte[] CRC32(int i)
        {
            return _localFiles[i].CRC;
        }

        public bool IsDirectory(int i)
        {
            try
            {
                if (_localFiles[i].UncompressedSize != 0)
                    return false;
                string filename = _localFiles[i].FileName;
                char lastChar = filename[filename.Length - 1];
                return lastChar == '/' || lastChar == '\\';
            }
            catch (Exception ex)
            {
                ArgumentException argEx = new ArgumentException("Error in file " + _zipFileInfo?.FullName + " : " + ex.Message, ex.InnerException);
                throw argEx;
            }

        }

        public DateTime LastModified(int i)
        {
            return _localFiles[i].DateTime;
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

            _zip64 = false;
            bool lTrrntzip = true;

            _centerDirStart = (ulong)_zipFs.Position;

            using (CrcCalculatorStream crcCs = new CrcCalculatorStream(_zipFs, true))
            {
                foreach (LocalFile t in _localFiles)
                {
                    t.CenteralDirectoryWrite(crcCs);
                    _zip64 |= t.Zip64;
                    lTrrntzip &= t.TrrntZip;
                }

                crcCs.Flush();
                crcCs.Close();

                _centerDirSize = (ulong)_zipFs.Position - _centerDirStart;

                _fileComment = lTrrntzip ? GetBytes("TORRENTZIPPED-" + crcCs.Crc.ToString("X8")) : new byte[0];
                ZipStatus = lTrrntzip ? ZipStatus.TrrntZip : ZipStatus.None;
            }
            _zip64 |= _centerDirStart >= 0xffffffff;
            _zip64 |= _centerDirSize >= 0xffffffff;
            _zip64 |= _localFiles.Count >= 0xffff;

            if (_zip64)
            {
                _endOfCenterDir64 = (ulong)_zipFs.Position;
                Zip64EndOfCentralDirWrite();
                Zip64EndOfCentralDirectoryLocatorWrite();
            }
            EndOfCentralDirWrite();

            _zipFs.SetLength(_zipFs.Position);
            _zipFs.Flush();
            _zipFs.Close();
            _zipFs.Dispose();
            _zipFileInfo = new FileInfo(_zipFileInfo.FullName);
            ZipOpen = ZipOpenType.Closed;
        }


        public void ZipFileCloseFailed()
        {
            switch (ZipOpen)
            {
                case ZipOpenType.Closed:
                    return;
                case ZipOpenType.OpenRead:
                    if (_zipFs != null)
                    {
                        _zipFs.Close();
                        _zipFs.Dispose();
                    }
                    break;
                case ZipOpenType.OpenWrite:
                    _zipFs.Flush();
                    _zipFs.Close();
                    _zipFs.Dispose();
                    if (_zipFileInfo != null)
                        RVIO.File.Delete(_zipFileInfo.FullName);
                    _zipFileInfo = null;
                    break;
            }

            ZipOpen = ZipOpenType.Closed;
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

        public ZipReturn ZipFileOpenWriteStream(bool raw, bool trrntzip, string filename, ulong uncompressedSize, ushort compressionMethod, uint? datetime, out Stream stream)
        {
            stream = null;
            if (ZipOpen != ZipOpenType.OpenWrite)
            {
                return ZipReturn.ZipWritingToInputFile;
            }

            LocalFile lf = new LocalFile(filename, datetime);

            ZipReturn retVal = lf.LocalFileOpenWriteStream(_zipFs, raw, trrntzip, uncompressedSize, compressionMethod, out stream);

            _compressionStream = stream;
            _localFiles.Add(lf);

            return retVal;
        }


        public ZipReturn ZipFileCloseWriteStream(byte[] crc32)
        {
            if (_compressionStream is ZlibBaseStream dfStream)
            {
                dfStream.Flush();
                dfStream.Close();
                dfStream.Dispose();
            }
            _compressionStream = null;

            return _localFiles[_localFiles.Count - 1].LocalFileCloseWriteStream(_zipFs, crc32);
        }

        public ZipReturn ZipFileRollBack()
        {
            if (ZipOpen != ZipOpenType.OpenWrite)
            {
                return ZipReturn.ZipWritingToInputFile;
            }

            int fileCount = _localFiles.Count;
            if (fileCount == 0)
            {
                return ZipReturn.ZipErrorRollBackFile;
            }

            LocalFile lf = _localFiles[fileCount - 1];

            _localFiles.RemoveAt(fileCount - 1);
            _zipFs.Position = (long)lf.LocalFilePos;
            return ZipReturn.ZipGood;
        }

        public void ZipFileAddZeroLengthFile()
        {
            LocalFile.LocalFileAddZeroLengthFile(_zipFs);
        }

        /*
        public void BreakTrrntZip(string filename)
        {
            _zipFs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite);
            using (BinaryReader zipBr = new BinaryReader(_zipFs,Encoding.UTF8,true))
            {
            _zipFs.Position = _zipFs.Length - 22;
            byte[] fileComment = zipBr.ReadBytes(22);
            if (GetString(fileComment).Substring(0, 14) == "TORRENTZIPPED-")
            {
                _zipFs.Position = _zipFs.Length - 8;
                _zipFs.WriteByte(48); _zipFs.WriteByte(48); _zipFs.WriteByte(48); _zipFs.WriteByte(48);
                _zipFs.WriteByte(48); _zipFs.WriteByte(48); _zipFs.WriteByte(48); _zipFs.WriteByte(48);
            }
            }
            _zipFs.Flush();
            _zipFs.Close();
        }
        */

        ~ZipFile()
        {
            if (_zipFs != null)
            {
                _zipFs.Close();
                _zipFs.Dispose();
            }
        }


        private ZipReturn FindEndOfCentralDirSignature()
        {
            long fileSize = _zipFs.Length;
            long maxBackSearch = 0xffff;

            if (_zipFs.Length < maxBackSearch)
            {
                maxBackSearch = fileSize;
            }

            const long buffSize = 0x400;

            byte[] buffer = new byte[buffSize + 4];

            long backPosition = 4;
            while (backPosition < maxBackSearch)
            {
                backPosition += buffSize;
                if (backPosition > maxBackSearch)
                {
                    backPosition = maxBackSearch;
                }

                long readSize = backPosition > buffSize + 4 ? buffSize + 4 : backPosition;

                _zipFs.Position = fileSize - backPosition;

                _zipFs.Read(buffer, 0, (int)readSize);


                for (long i = readSize - 4; i >= 0; i--)
                {
                    if (buffer[i] != 0x50 || buffer[i + 1] != 0x4b || buffer[i + 2] != 0x05 || buffer[i + 3] != 0x06)
                    {
                        continue;
                    }

                    _zipFs.Position = fileSize - backPosition + i;
                    return ZipReturn.ZipGood;
                }
            }
            return ZipReturn.ZipCentralDirError;
        }


        private ZipReturn EndOfCentralDirRead()
        {
            using (BinaryReader zipBr = new BinaryReader(_zipFs, Encoding.UTF8, true))
            {
                uint thisSignature = zipBr.ReadUInt32();
                if (thisSignature != EndOfCentralDirSignature)
                {
                    return ZipReturn.ZipEndOfCentralDirectoryError;
                }

                ushort tushort = zipBr.ReadUInt16(); // NumberOfThisDisk
                if (tushort != 0)
                {
                    return ZipReturn.ZipEndOfCentralDirectoryError;
                }

                tushort = zipBr.ReadUInt16(); // NumberOfThisDiskCenterDir
                if (tushort != 0)
                {
                    return ZipReturn.ZipEndOfCentralDirectoryError;
                }

                _localFilesCount = zipBr.ReadUInt16(); // TotalNumberOfEnteriesDisk

                tushort = zipBr.ReadUInt16(); // TotalNumber of enteries in the central directory
                if (tushort != _localFilesCount)
                {
                    return ZipReturn.ZipEndOfCentralDirectoryError;
                }

                _centerDirSize = zipBr.ReadUInt32(); // SizeOfCenteralDir
                _centerDirStart = zipBr.ReadUInt32(); // Offset

                ushort zipFileCommentLength = zipBr.ReadUInt16();

                _fileComment = zipBr.ReadBytes(zipFileCommentLength);

                if (_zipFs.Position != _zipFs.Length)
                {
                    ZipStatus |= ZipStatus.ExtraData;
                }

                return ZipReturn.ZipGood;
            }
        }

        private void EndOfCentralDirWrite()
        {
            using (BinaryWriter bw = new BinaryWriter(_zipFs, Encoding.UTF8, true))
            {
                bw.Write(EndOfCentralDirSignature);
                bw.Write((ushort)0); // NumberOfThisDisk
                bw.Write((ushort)0); // NumberOfThisDiskCenterDir
                bw.Write((ushort)(_localFiles.Count >= 0xffff ? 0xffff : _localFiles.Count)); // TotalNumberOfEnteriesDisk
                bw.Write((ushort)(_localFiles.Count >= 0xffff ? 0xffff : _localFiles.Count)); // TotalNumber of enteries in the central directory
                bw.Write((uint)(_centerDirSize >= 0xffffffff ? 0xffffffff : _centerDirSize));
                bw.Write((uint)(_centerDirStart >= 0xffffffff ? 0xffffffff : _centerDirStart));
                bw.Write((ushort)_fileComment.Length);
                bw.Write(_fileComment, 0, _fileComment.Length);
            }
        }

        private ZipReturn Zip64EndOfCentralDirRead()
        {
            using (BinaryReader zipBr = new BinaryReader(_zipFs, Encoding.UTF8, true))
            {
                _zip64 = true;
                uint thisSignature = zipBr.ReadUInt32();
                if (thisSignature != Zip64EndOfCentralDirSignatue)
                {
                    return ZipReturn.ZipEndOfCentralDirectoryError;
                }

                ulong tulong = zipBr.ReadUInt64(); // Size of zip64 end of central directory record
                if (tulong != 44)
                {
                    return ZipReturn.Zip64EndOfCentralDirError;
                }

                zipBr.ReadUInt16(); // version made by

                ushort tushort = zipBr.ReadUInt16(); // version needed to extract
                if (tushort != 45)
                {
                    return ZipReturn.Zip64EndOfCentralDirError;
                }

                uint tuint = zipBr.ReadUInt32(); // number of this disk
                if (tuint != 0)
                {
                    return ZipReturn.Zip64EndOfCentralDirError;
                }

                tuint = zipBr.ReadUInt32(); // number of the disk with the start of the central directory
                if (tuint != 0)
                {
                    return ZipReturn.Zip64EndOfCentralDirError;
                }

                _localFilesCount =
                    (uint)zipBr.ReadUInt64(); // total number of entries in the central directory on this disk

                tulong = zipBr.ReadUInt64(); // total number of entries in the central directory
                if (tulong != _localFilesCount)
                {
                    return ZipReturn.Zip64EndOfCentralDirError;
                }

                _centerDirSize = zipBr.ReadUInt64(); // size of central directory

                _centerDirStart =
                    zipBr.ReadUInt64(); // offset of start of central directory with respect to the starting disk number

                return ZipReturn.ZipGood;
            }
        }

        private void Zip64EndOfCentralDirWrite()
        {
            using (BinaryWriter bw = new BinaryWriter(_zipFs, Encoding.UTF8, true))
            {
                bw.Write(Zip64EndOfCentralDirSignatue);
                bw.Write((ulong)44); // Size of zip64 end of central directory record
                bw.Write((ushort)45); // version made by
                bw.Write((ushort)45); // version needed to extract
                bw.Write((uint)0); // number of this disk
                bw.Write((uint)0); // number of the disk with the start of the central directroy
                bw.Write((ulong)_localFiles.Count); // total number of entries in the central directory on this disk
                bw.Write((ulong)_localFiles.Count); // total number of entries in the central directory
                bw.Write(_centerDirSize); // size of central directory
                bw.Write(_centerDirStart); // offset of start of central directory with respect to the starting disk number
            }
        }

        private ZipReturn Zip64EndOfCentralDirectoryLocatorRead()
        {
            using (BinaryReader zipBr = new BinaryReader(_zipFs, Encoding.UTF8, true))
            {
                _zip64 = true;
                uint thisSignature = zipBr.ReadUInt32();
                if (thisSignature != Zip64EndOfCentralDirectoryLocator)
                {
                    return ZipReturn.ZipEndOfCentralDirectoryError;
                }

                uint tuint =
                    zipBr.ReadUInt32(); // number of the disk with the start of the zip64 end of centeral directory
                if (tuint != 0)
                {
                    return ZipReturn.Zip64EndOfCentralDirectoryLocatorError;
                }

                _endOfCenterDir64 = zipBr.ReadUInt64(); // relative offset of the zip64 end of central directroy record

                tuint = zipBr.ReadUInt32(); // total number of disks
                if (tuint != 1)
                {
                    return ZipReturn.Zip64EndOfCentralDirectoryLocatorError;
                }

                return ZipReturn.ZipGood;
            }
        }

        private void Zip64EndOfCentralDirectoryLocatorWrite()
        {
            using (BinaryWriter bw = new BinaryWriter(_zipFs, Encoding.UTF8, true))
            {
                bw.Write(Zip64EndOfCentralDirectoryLocator);
                bw.Write((uint)0); // number of the disk with the start of the zip64 end of centeral directory
                bw.Write(_endOfCenterDir64); // relative offset of the zip64 end of central directroy record
                bw.Write((uint)1); // total number of disks
            }
        }




        public ZipReturn ZipFileOpen(string newFilename, long timestamp, bool readHeaders)
        {
            ZipFileClose();
            ZipStatus = ZipStatus.None;
            _zip64 = false;
            _centerDirStart = 0;
            _centerDirSize = 0;
            _zipFileInfo = null;

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
            _zip64 = false;
            _centerDirStart = 0;
            _centerDirSize = 0;
            _zipFileInfo = null;
            _zipFs = inStream;

            ZipOpen = ZipOpenType.OpenRead;
            return ZipFileReadHeaders();
        }


        private ZipReturn ZipFileReadHeaders()
        {
            try
            {
                ZipReturn zRet = FindEndOfCentralDirSignature();
                if (zRet != ZipReturn.ZipGood)
                {
                    ZipFileClose();
                    return zRet;
                }

                long endOfCentralDir = _zipFs.Position;
                zRet = EndOfCentralDirRead();
                if (zRet != ZipReturn.ZipGood)
                {
                    ZipFileClose();
                    return zRet;
                }

                // check if this is a ZIP64 zip and if it is read the Zip64 End Of Central Dir Info
                if (_centerDirStart == 0xffffffff || _centerDirSize == 0xffffffff || _localFilesCount == 0xffff)
                {
                    _zip64 = true;
                    _zipFs.Position = endOfCentralDir - 20;
                    zRet = Zip64EndOfCentralDirectoryLocatorRead();
                    if (zRet != ZipReturn.ZipGood)
                    {
                        ZipFileClose();
                        return zRet;
                    }
                    _zipFs.Position = (long)_endOfCenterDir64;
                    zRet = Zip64EndOfCentralDirRead();
                    if (zRet != ZipReturn.ZipGood)
                    {
                        ZipFileClose();
                        return zRet;
                    }
                }

                bool trrntzip = false;

                // check if the ZIP has a valid TorrentZip file comment
                if (_fileComment.Length == 22)
                {
                    if (GetString(_fileComment).Substring(0, 14) == "TORRENTZIPPED-")
                    {
                        CrcCalculatorStream crcCs = new CrcCalculatorStream(_zipFs, true);
                        byte[] buffer = new byte[_centerDirSize];
                        _zipFs.Position = (long)_centerDirStart;
                        crcCs.Read(buffer, 0, (int)_centerDirSize);
                        crcCs.Flush();
                        crcCs.Close();

                        uint r = (uint)crcCs.Crc;
                        crcCs.Dispose();

                        string tcrc = GetString(_fileComment).Substring(14, 8);
                        string zcrc = r.ToString("X8");
                        if (string.Compare(tcrc, zcrc, StringComparison.Ordinal) == 0)
                        {
                            trrntzip = true;
                        }
                    }
                }


                // now read the central directory
                _zipFs.Position = (long)_centerDirStart;

                _localFiles.Clear();
                _localFiles.Capacity = (int)_localFilesCount;
                for (int i = 0; i < _localFilesCount; i++)
                {
                    LocalFile lc = new LocalFile();
                    zRet = lc.CenteralDirectoryRead(_zipFs);
                    if (zRet != ZipReturn.ZipGood)
                    {
                        ZipFileClose();
                        return zRet;
                    }
                    _zip64 |= lc.Zip64;
                    _localFiles.Add(lc);
                }

                for (int i = 0; i < _localFilesCount; i++)
                {
                    zRet = _localFiles[i].LocalFileHeaderRead(_zipFs);
                    if (zRet != ZipReturn.ZipGood)
                    {
                        ZipFileClose();
                        return zRet;
                    }
                    trrntzip &= _localFiles[i].TrrntZip;
                }

                // check trrntzip file order
                if (trrntzip)
                {
                    for (int i = 0; i < _localFilesCount - 1; i++)
                    {
                        if (TrrntZipStringCompare(_localFiles[i].FileName, _localFiles[i + 1].FileName) < 0)
                        {
                            continue;
                        }
                        trrntzip = false;
                        break;
                    }
                }

                // check trrntzip directories
                if (trrntzip)
                {
                    for (int i = 0; i < _localFilesCount - 1; i++)
                    {
                        // see if we found a directory
                        string filename0 = _localFiles[i].FileName;
                        if (filename0.Substring(filename0.Length - 1, 1) != "/")
                        {
                            continue;
                        }

                        // see if the next file is in that directory
                        string filename1 = _localFiles[i + 1].FileName;
                        if (filename1.Length <= filename0.Length)
                        {
                            continue;
                        }
                        if (TrrntZipStringCompare(filename0, filename1.Substring(0, filename0.Length)) != 0)
                        {
                            continue;
                        }

                        // if we found a file in the directory then we do not need the directory entry
                        trrntzip = false;
                        break;
                    }
                }

                if (trrntzip)
                {
                    ZipStatus |= ZipStatus.TrrntZip;
                }

                return ZipReturn.ZipGood;
            }
            catch
            {
                ZipFileClose();
                return ZipReturn.ZipErrorReadingFile;
            }
        }

        public void ZipCreateFake()
        {
            if (ZipOpen != ZipOpenType.Closed)
            {
                return;
            }

            ZipOpen = ZipOpenType.OpenFakeWrite;
        }

        public void ZipFileCloseFake(ulong fileOffset, out byte[] centeralDir)
        {
            centeralDir = null;
            if (ZipOpen != ZipOpenType.OpenFakeWrite)
            {
                return;
            }

            _zip64 = false;
            bool lTrrntzip = true;

            _zipFs = new MemoryStream();

            _centerDirStart = fileOffset;
            if (_centerDirStart >= 0xffffffff)
            {
                _zip64 = true;
            }

            CrcCalculatorStream crcCs = new CrcCalculatorStream(_zipFs, true);

            foreach (LocalFile t in _localFiles)
            {
                t.CenteralDirectoryWrite(crcCs);
                _zip64 |= t.Zip64;
                lTrrntzip &= t.TrrntZip;
            }

            crcCs.Flush();
            crcCs.Close();

            _centerDirSize = (ulong)_zipFs.Position;

            _fileComment = lTrrntzip ? GetBytes("TORRENTZIPPED-" + crcCs.Crc.ToString("X8")) : new byte[0];
            ZipStatus = lTrrntzip ? ZipStatus.TrrntZip : ZipStatus.None;

            crcCs.Dispose();

            if (_zip64)
            {
                _endOfCenterDir64 = fileOffset + (ulong)_zipFs.Position;
                Zip64EndOfCentralDirWrite();
                Zip64EndOfCentralDirectoryLocatorWrite();
            }
            EndOfCentralDirWrite();

            centeralDir = ((MemoryStream)_zipFs).ToArray();
            _zipFs.Close();
            _zipFs.Dispose();
            ZipOpen = ZipOpenType.Closed;
        }



        public ZipReturn ZipFileOpenReadStream(int index, out Stream stream, out ulong streamSize)
        {
            return ZipFileOpenReadStream(index, false, out stream, out streamSize, out ushort _);
        }

        public ZipReturn ZipFileOpenReadStream(int index, bool raw, out Stream stream, out ulong streamSize, out ushort compressionMethod)
        {
            ZipFileCloseReadStream();

            streamSize = 0;
            compressionMethod = 0;
            stream = null;
            if (ZipOpen != ZipOpenType.OpenRead)
            {
                return ZipReturn.ZipReadingFromOutputFile;
            }

            ZipReturn zRet = _localFiles[index].LocalFileHeaderRead(_zipFs);
            if (zRet != ZipReturn.ZipGood)
            {
                ZipFileClose();
                return zRet;
            }

            zRet = _localFiles[index].LocalFileOpenReadStream(_zipFs, raw, out stream, out streamSize, out compressionMethod);
            _compressionStream = stream;
            return zRet;
        }

        public ZipReturn ZipFileOpenReadStreamQuick(ulong pos, bool raw, out Stream stream, out ulong streamSize, out ushort compressionMethod)
        {
            ZipFileCloseReadStream();

            LocalFile tmpFile = new LocalFile { LocalFilePos = pos };
            _localFiles.Clear();
            _localFiles.Add(tmpFile);
            ZipReturn zRet = tmpFile.LocalFileHeaderReadQuick(_zipFs);
            if (zRet != ZipReturn.ZipGood)
            {
                stream = null;
                streamSize = 0;
                compressionMethod = 0;
                return zRet;
            }

            zRet = tmpFile.LocalFileOpenReadStream(_zipFs, raw, out stream, out streamSize, out compressionMethod);
            _compressionStream = stream;
            return zRet;
        }

        public ZipReturn ZipFileAddFake(string filename, ulong fileOffset, ulong uncompressedSize, ulong compressedSize, byte[] crc32, uint? datetime, out byte[] localHeader)
        {
            localHeader = null;

            if (ZipOpen != ZipOpenType.OpenFakeWrite)
            {
                return ZipReturn.ZipWritingToInputFile;
            }

            LocalFile lf = new LocalFile(filename, datetime);
            _localFiles.Add(lf);

            MemoryStream ms = new MemoryStream();
            lf.LocalFileHeaderFake(fileOffset, uncompressedSize, compressedSize, crc32, ms);

            localHeader = ms.ToArray();
            ms.Close();

            return ZipReturn.ZipGood;
        }

        public static void CreateDirForFile(string sFilename)
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


        public static string ZipErrorMessageText(ZipReturn zS)
        {
            string ret = "Unknown";
            switch (zS)
            {
                case ZipReturn.ZipGood:
                    ret = string.Empty;
                    break;
                case ZipReturn.ZipFileCountError:
                    ret = "The number of file in the Zip does not mach the number of files in the Zips Centeral Directory";
                    break;
                case ZipReturn.ZipSignatureError:
                    ret = "An unknown Signature Block was found in the Zip";
                    break;
                case ZipReturn.ZipExtraDataOnEndOfZip:
                    ret = "Extra Data was found on the end of the Zip";
                    break;
                case ZipReturn.ZipUnsupportedCompression:
                    ret = "An unsupported Compression method was found in the Zip, if you recompress this zip it will be usable";
                    break;
                case ZipReturn.ZipLocalFileHeaderError:
                    ret = "Error reading a zipped file header information";
                    break;
                case ZipReturn.ZipCentralDirError:
                    ret = "There is an error in the Zip Centeral Directory";
                    break;
                case ZipReturn.ZipReadingFromOutputFile:
                    ret = "Trying to write to a Zip file open for output only";
                    break;
                case ZipReturn.ZipWritingToInputFile:
                    ret = "Tring to read from a Zip file open for input only";
                    break;
                case ZipReturn.ZipErrorGettingDataStream:
                    ret = "Error creating Data Stream";
                    break;
                case ZipReturn.ZipCRCDecodeError:
                    ret = "CRC error";
                    break;
                case ZipReturn.ZipDecodeError:
                    ret = "Error unzipping a file";
                    break;
            }

            return ret;
        }

        private static byte[] GetBytes(string s)
        {
            char[] c = s.ToCharArray();
            byte[] b = new byte[c.Length];
            for (int i = 0; i < c.Length; i++)
            {
                char t = c[i];
                b[i] = t > 255 ? (byte)'?' : (byte)c[i];
            }
            return b;
        }

        private static bool IsUnicode(string s)
        {
            char[] charArr = s.ToCharArray();
            foreach (char ch in charArr)
            {
                if (ch > 127)
                {
                    return true;
                }
            }
            return false;
        }

        private static string GetString(byte[] byteArr)
        {
            string s = string.Empty;
            foreach (byte by in byteArr)
            {
                s += (char)by;
            }
            return s;
        }

        private static bool CompareString(string s1, string s2)
        {
            char[] c1 = s1.ToCharArray();
            char[] c2 = s2.ToCharArray();

            if (c1.Length != c2.Length)
            {
                return false;
            }

            for (int i = 0; i < c1.Length; i++)
            {
                if (c1[i] != c2[i])
                {
                    return false;
                }
            }
            return true;
        }


        private static bool ByteArrCompare(byte[] b0, byte[] b1)
        {
            if ((b0 == null) || (b1 == null))
            {
                return false;
            }
            if (b0.Length != b1.Length)
            {
                return false;
            }

            for (int i = 0; i < b0.Length; i++)
            {
                if (b0[i] != b1[i])
                {
                    return false;
                }
            }
            return true;
        }

        public static int TrrntZipStringCompare(string string1, string string2)
        {
            char[] bytes1 = string1.ToCharArray();
            char[] bytes2 = string2.ToCharArray();

            int pos1 = 0;
            int pos2 = 0;

            for (; ; )
            {
                if (pos1 == bytes1.Length)
                {
                    return pos2 == bytes2.Length ? 0 : -1;
                }
                if (pos2 == bytes2.Length)
                {
                    return 1;
                }

                int byte1 = bytes1[pos1++];
                int byte2 = bytes2[pos2++];

                if (byte1 >= 65 && byte1 <= 90)
                {
                    byte1 += 0x20;
                }
                if (byte2 >= 65 && byte2 <= 90)
                {
                    byte2 += 0x20;
                }

                if (byte1 < byte2)
                {
                    return -1;
                }
                if (byte1 > byte2)
                {
                    return 1;
                }
            }
        }

        private class LocalFile
        {
            private ushort _compressionMethod;
            private ushort _lastModFileTime;
            private ushort _lastModFileDate;
            private uint? _lastModFileDateTime;
            private ulong _compressedSize;
            public ulong RelativeOffsetOfLocalHeader; // only in centeral directory

            private ulong _crc32Location;
            private ulong _extraLocation;
            private ulong _dataLocation;

            public LocalFile()
            {
            }

            public LocalFile(string filename, uint? datetime)
            {
                Zip64 = false;
                GeneralPurposeBitFlag = 2; // Maximum Compression Deflating
                _compressionMethod = 8; // Compression Method Deflate
                if (datetime == null)
                {
                    _lastModFileTime = 48128;
                    _lastModFileDate = 8600;
                }
                else
                {
                    _lastModFileDateTime = datetime;
                }

                FileName = filename;
            }

            public string FileName { get; private set; }
            public ushort GeneralPurposeBitFlag { get; private set; }
            public byte[] CRC { get; private set; }
            public ulong UncompressedSize { get; private set; }

            public bool Zip64 { get; private set; }
            public bool TrrntZip { get; private set; }

            public DateTime DateTime
            {
                get
                {
                    if (_lastModFileDateTime == null)
                    {
                        int second = (_lastModFileTime & 0x1f) * 2;
                        int minute = (_lastModFileTime >> 5) & 0x3f;
                        int hour = (_lastModFileTime >> 11) & 0x1f;

                        int day = _lastModFileDate & 0x1f;
                        int month = (_lastModFileDate >> 5) & 0x0f;
                        int year = ((_lastModFileDate >> 9) & 0x7f) + 1980;

                        return new DateTime(year, month, day, hour, minute, second);
                    }
                    else
                    {
                        return SabreTools.Library.Tools.Utilities.ConvertMsDosTimeFormatToDateTime(_lastModFileDateTime.Value);
                    }
                }
            }

            public ulong LocalFilePos
            {
                get => RelativeOffsetOfLocalHeader;
                set => RelativeOffsetOfLocalHeader = value;
            }


            public ZipReturn CenteralDirectoryRead(Stream zipFs)
            {
                try
                {
                    using (BinaryReader br = new BinaryReader(zipFs, Encoding.UTF8, true))
                    {
                        uint thisSignature = br.ReadUInt32();
                        if (thisSignature != CentralDirectoryHeaderSigniature)
                        {
                            return ZipReturn.ZipCentralDirError;
                        }

                        br.ReadUInt16(); // Version Made By

                        br.ReadUInt16(); // Version Needed To Extract


                        GeneralPurposeBitFlag = br.ReadUInt16();
                        _compressionMethod = br.ReadUInt16();
                        if (_compressionMethod != 8 && _compressionMethod != 0)
                        {
                            return ZipReturn.ZipUnsupportedCompression;
                        }

                        _lastModFileTime = br.ReadUInt16();
                        _lastModFileDate = br.ReadUInt16();
                        CRC = ReadCRC(br);

                        _compressedSize = br.ReadUInt32();
                        UncompressedSize = br.ReadUInt32();

                        ushort fileNameLength = br.ReadUInt16();
                        ushort extraFieldLength = br.ReadUInt16();
                        ushort fileCommentLength = br.ReadUInt16();

                        br.ReadUInt16(); // diskNumberStart
                        br.ReadUInt16(); // internalFileAttributes
                        br.ReadUInt32(); // externalFileAttributes

                        RelativeOffsetOfLocalHeader = br.ReadUInt32();

                        byte[] bFileName = br.ReadBytes(fileNameLength);
                        FileName = (GeneralPurposeBitFlag & (1 << 11)) == 0
                            ? GetString(bFileName)
                            : Encoding.UTF8.GetString(bFileName, 0, fileNameLength);

                        byte[] extraField = br.ReadBytes(extraFieldLength);
                        br.ReadBytes(fileCommentLength); // File Comments

                        int pos = 0;
                        while (extraFieldLength > pos)
                        {
                            ushort type = BitConverter.ToUInt16(extraField, pos);
                            pos += 2;
                            ushort blockLength = BitConverter.ToUInt16(extraField, pos);
                            pos += 2;
                            switch (type)
                            {
                                case 0x0001:
                                    Zip64 = true;
                                    if (UncompressedSize == 0xffffffff)
                                    {
                                        UncompressedSize = BitConverter.ToUInt64(extraField, pos);
                                        pos += 8;
                                    }

                                    if (_compressedSize == 0xffffffff)
                                    {
                                        _compressedSize = BitConverter.ToUInt64(extraField, pos);
                                        pos += 8;
                                    }

                                    if (RelativeOffsetOfLocalHeader == 0xffffffff)
                                    {
                                        RelativeOffsetOfLocalHeader = BitConverter.ToUInt64(extraField, pos);
                                        pos += 8;
                                    }

                                    break;
                                case 0x7075:
                                    //byte version = extraField[pos];
                                    pos += 1;
                                    uint nameCRC32 = BitConverter.ToUInt32(extraField, pos);
                                    pos += 4;

                                    CRC crcTest = new CRC();
                                    crcTest.SlurpBlock(bFileName, 0, fileNameLength);
                                    uint fCRC = crcTest.Crc32ResultU;

                                    if (nameCRC32 != fCRC)
                                    {
                                        return ZipReturn.ZipCentralDirError;
                                    }

                                    int charLen = blockLength - 5;

                                    FileName = Encoding.UTF8.GetString(extraField, pos, charLen);
                                    pos += charLen;

                                    break;
                                default:
                                    pos += blockLength;
                                    break;
                            }
                        }

                        return ZipReturn.ZipGood;
                    }
                }
                catch
                {
                    return ZipReturn.ZipCentralDirError;
                }
            }

            public void CenteralDirectoryWrite(Stream crcStream)
            {
                using (BinaryWriter bw = new BinaryWriter(crcStream, Encoding.UTF8, true))
                {
                    const uint header = 0x2014B50;

                    List<byte> extraField = new List<byte>();

                    uint cdUncompressedSize;
                    if (UncompressedSize >= 0xffffffff)
                    {
                        Zip64 = true;
                        cdUncompressedSize = 0xffffffff;
                        extraField.AddRange(BitConverter.GetBytes(UncompressedSize));
                    }
                    else
                    {
                        cdUncompressedSize = (uint)UncompressedSize;
                    }

                    uint cdCompressedSize;
                    if (_compressedSize >= 0xffffffff)
                    {
                        Zip64 = true;
                        cdCompressedSize = 0xffffffff;
                        extraField.AddRange(BitConverter.GetBytes(_compressedSize));
                    }
                    else
                    {
                        cdCompressedSize = (uint)_compressedSize;
                    }

                    uint cdRelativeOffsetOfLocalHeader;
                    if (RelativeOffsetOfLocalHeader >= 0xffffffff)
                    {
                        Zip64 = true;
                        cdRelativeOffsetOfLocalHeader = 0xffffffff;
                        extraField.AddRange(BitConverter.GetBytes(RelativeOffsetOfLocalHeader));
                    }
                    else
                    {
                        cdRelativeOffsetOfLocalHeader = (uint)RelativeOffsetOfLocalHeader;
                    }


                    if (extraField.Count > 0)
                    {
                        ushort exfl = (ushort)extraField.Count;
                        extraField.InsertRange(0, BitConverter.GetBytes((ushort)0x0001));
                        extraField.InsertRange(2, BitConverter.GetBytes(exfl));
                    }

                    ushort extraFieldLength = (ushort)extraField.Count;

                    byte[] bFileName;
                    if (IsUnicode(FileName))
                    {
                        GeneralPurposeBitFlag |= 1 << 11;
                        bFileName = Encoding.UTF8.GetBytes(FileName);
                    }
                    else
                    {
                        bFileName = GetBytes(FileName);
                    }

                    ushort fileNameLength = (ushort)bFileName.Length;

                    ushort versionNeededToExtract = (ushort)(Zip64 ? 45 : 20);

                    bw.Write(header);
                    bw.Write((ushort)0);
                    bw.Write(versionNeededToExtract);
                    bw.Write(GeneralPurposeBitFlag);
                    bw.Write(_compressionMethod);
                    if (_lastModFileDateTime == null)
                    {
                        bw.Write(_lastModFileTime);
                        bw.Write(_lastModFileDate);
                    }
                    else
                    {
                        bw.Write(_lastModFileDateTime.Value);
                    }
                    bw.Write(CRC[3]);
                    bw.Write(CRC[2]);
                    bw.Write(CRC[1]);
                    bw.Write(CRC[0]);
                    bw.Write(cdCompressedSize);
                    bw.Write(cdUncompressedSize);
                    bw.Write(fileNameLength);
                    bw.Write(extraFieldLength);
                    bw.Write((ushort)0); // file comment length
                    bw.Write((ushort)0); // disk number start
                    bw.Write((ushort)0); // internal file attributes
                    bw.Write((uint)0); // external file attributes
                    bw.Write(cdRelativeOffsetOfLocalHeader);

                    bw.Write(bFileName, 0, fileNameLength);
                    bw.Write(extraField.ToArray(), 0, extraFieldLength);
                    // No File Comment
                }
            }
            public ZipReturn LocalFileHeaderRead(Stream zipFs)
            {
                try
                {
                    using (BinaryReader br = new BinaryReader(zipFs, Encoding.UTF8, true))
                    {

                        TrrntZip = true;

                        zipFs.Position = (long)RelativeOffsetOfLocalHeader;
                        uint thisSignature = br.ReadUInt32();
                        if (thisSignature != LocalFileHeaderSignature)
                        {
                            return ZipReturn.ZipLocalFileHeaderError;
                        }

                        br.ReadUInt16(); // version needed to extract
                        ushort generalPurposeBitFlagLocal = br.ReadUInt16();
                        if (generalPurposeBitFlagLocal != GeneralPurposeBitFlag)
                        {
                            TrrntZip = false;
                        }

                        ushort tshort = br.ReadUInt16();
                        if (tshort != _compressionMethod)
                        {
                            return ZipReturn.ZipLocalFileHeaderError;
                        }

                        tshort = br.ReadUInt16();
                        if (tshort != _lastModFileTime)
                        {
                            return ZipReturn.ZipLocalFileHeaderError;
                        }

                        tshort = br.ReadUInt16();
                        if (tshort != _lastModFileDate)
                        {
                            return ZipReturn.ZipLocalFileHeaderError;
                        }

                        byte[] tCRC = ReadCRC(br);
                        ulong tCompressedSize = br.ReadUInt32();
                        ulong tUnCompressedSize = br.ReadUInt32();

                        ushort fileNameLength = br.ReadUInt16();
                        ushort extraFieldLength = br.ReadUInt16();


                        byte[] bFileName = br.ReadBytes(fileNameLength);
                        string tFileName = (generalPurposeBitFlagLocal & (1 << 11)) == 0
                            ? GetString(bFileName)
                            : Encoding.UTF8.GetString(bFileName, 0, fileNameLength);

                        byte[] extraField = br.ReadBytes(extraFieldLength);


                        Zip64 = false;
                        int pos = 0;
                        while (extraFieldLength > pos)
                        {
                            ushort type = BitConverter.ToUInt16(extraField, pos);
                            pos += 2;
                            ushort blockLength = BitConverter.ToUInt16(extraField, pos);
                            pos += 2;
                            switch (type)
                            {
                                case 0x0001:
                                    Zip64 = true;
                                    if (tUnCompressedSize == 0xffffffff)
                                    {
                                        tUnCompressedSize = BitConverter.ToUInt64(extraField, pos);
                                        pos += 8;
                                    }

                                    if (tCompressedSize == 0xffffffff)
                                    {
                                        tCompressedSize = BitConverter.ToUInt64(extraField, pos);
                                        pos += 8;
                                    }

                                    break;
                                case 0x7075:
                                    //byte version = extraField[pos];
                                    pos += 1;
                                    uint nameCRC32 = BitConverter.ToUInt32(extraField, pos);
                                    pos += 4;

                                    CRC crcTest = new CRC();
                                    crcTest.SlurpBlock(bFileName, 0, fileNameLength);
                                    uint fCRC = crcTest.Crc32ResultU;

                                    if (nameCRC32 != fCRC)
                                    {
                                        return ZipReturn.ZipLocalFileHeaderError;
                                    }

                                    int charLen = blockLength - 5;

                                    tFileName = Encoding.UTF8.GetString(extraField, pos, charLen);
                                    pos += charLen;

                                    break;
                                default:
                                    pos += blockLength;
                                    break;
                            }
                        }

                        if (!CompareString(FileName, tFileName))
                        {
                            return ZipReturn.ZipLocalFileHeaderError;
                        }

                        _dataLocation = (ulong)zipFs.Position;

                        if ((GeneralPurposeBitFlag & 8) == 8)
                        {
                            zipFs.Position += (long)_compressedSize;

                            tCRC = ReadCRC(br);
                            if (!ByteArrCompare(tCRC, new byte[] { 0x50, 0x4b, 0x07, 0x08 }))
                            {
                                tCRC = ReadCRC(br);
                            }

                            tCompressedSize = br.ReadUInt32();
                            tUnCompressedSize = br.ReadUInt32();
                        }



                        if (!ByteArrCompare(tCRC, CRC))
                        {
                            return ZipReturn.ZipLocalFileHeaderError;
                        }

                        if (tCompressedSize != _compressedSize)
                        {
                            return ZipReturn.ZipLocalFileHeaderError;
                        }

                        if (tUnCompressedSize != UncompressedSize)
                        {
                            return ZipReturn.ZipLocalFileHeaderError;
                        }

                        return ZipReturn.ZipGood;
                    }
                }
                catch
                {
                    return ZipReturn.ZipLocalFileHeaderError;
                }
            }

            public ZipReturn LocalFileHeaderReadQuick(Stream zipFs)
            {
                try
                {

                    using (BinaryReader br = new BinaryReader(zipFs, Encoding.UTF8, true))
                    {
                        TrrntZip = true;


                        zipFs.Position = (long)RelativeOffsetOfLocalHeader;
                        uint thisSignature = br.ReadUInt32();
                        if (thisSignature != LocalFileHeaderSignature)
                        {
                            return ZipReturn.ZipLocalFileHeaderError;
                        }

                        br.ReadUInt16(); // version needed to extract
                        GeneralPurposeBitFlag = br.ReadUInt16();
                        if ((GeneralPurposeBitFlag & 8) == 8)
                        {
                            return ZipReturn.ZipCannotFastOpen;
                        }

                        _compressionMethod = br.ReadUInt16();
                        _lastModFileTime = br.ReadUInt16();
                        _lastModFileDate = br.ReadUInt16();
                        CRC = ReadCRC(br);
                        _compressedSize = br.ReadUInt32();
                        UncompressedSize = br.ReadUInt32();

                        ushort fileNameLength = br.ReadUInt16();
                        ushort extraFieldLength = br.ReadUInt16();

                        byte[] bFileName = br.ReadBytes(fileNameLength);

                        FileName = (GeneralPurposeBitFlag & (1 << 11)) == 0
                            ? GetString(bFileName)
                            : Encoding.UTF8.GetString(bFileName, 0, fileNameLength);

                        byte[] extraField = br.ReadBytes(extraFieldLength);

                        Zip64 = false;
                        int pos = 0;
                        while (extraFieldLength > pos)
                        {
                            ushort type = BitConverter.ToUInt16(extraField, pos);
                            pos += 2;
                            ushort blockLength = BitConverter.ToUInt16(extraField, pos);
                            pos += 2;
                            switch (type)
                            {
                                case 0x0001:
                                    Zip64 = true;
                                    if (UncompressedSize == 0xffffffff)
                                    {
                                        UncompressedSize = BitConverter.ToUInt64(extraField, pos);
                                        pos += 8;
                                    }

                                    if (_compressedSize == 0xffffffff)
                                    {
                                        _compressedSize = BitConverter.ToUInt64(extraField, pos);
                                        pos += 8;
                                    }

                                    break;
                                case 0x7075:
                                    pos += 1;
                                    uint nameCRC32 = BitConverter.ToUInt32(extraField, pos);
                                    pos += 4;

                                    CRC crcTest = new CRC();
                                    crcTest.SlurpBlock(bFileName, 0, fileNameLength);
                                    uint fCRC = crcTest.Crc32ResultU;

                                    if (nameCRC32 != fCRC)
                                    {
                                        return ZipReturn.ZipLocalFileHeaderError;
                                    }

                                    int charLen = blockLength - 5;

                                    FileName = Encoding.UTF8.GetString(extraField, pos, charLen);

                                    pos += charLen;

                                    break;
                                default:
                                    pos += blockLength;
                                    break;
                            }
                        }

                        _dataLocation = (ulong)zipFs.Position;
                        return ZipReturn.ZipGood;
                    }
                }
                catch
                {
                    return ZipReturn.ZipLocalFileHeaderError;
                }
            }


            private void LocalFileHeaderWrite(Stream zipFs)
            {
                using (BinaryWriter bw = new BinaryWriter(zipFs, Encoding.UTF8, true))
                {
                    Zip64 = UncompressedSize >= 0xffffffff;

                    byte[] bFileName;
                    if (IsUnicode(FileName))
                    {
                        GeneralPurposeBitFlag |= 1 << 11;
                        bFileName = Encoding.UTF8.GetBytes(FileName);
                    }
                    else
                    {
                        bFileName = GetBytes(FileName);
                    }

                    ushort versionNeededToExtract = (ushort)(Zip64 ? 45 : 20);

                    RelativeOffsetOfLocalHeader = (ulong)zipFs.Position;
                    const uint header = 0x4034B50;
                    bw.Write(header);
                    bw.Write(versionNeededToExtract);
                    bw.Write(GeneralPurposeBitFlag);
                    bw.Write(_compressionMethod);
                    if (_lastModFileDateTime == null)
                    {
                        bw.Write(_lastModFileTime);
                        bw.Write(_lastModFileDate);
                    }
                    else
                    {
                        bw.Write(_lastModFileDateTime.Value);
                    }

                    _crc32Location = (ulong)zipFs.Position;

                    // these 3 values will be set correctly after the file data has been written
                    bw.Write(0xffffffff);
                    bw.Write(0xffffffff);
                    bw.Write(0xffffffff);

                    ushort fileNameLength = (ushort)bFileName.Length;
                    bw.Write(fileNameLength);

                    ushort extraFieldLength = (ushort)(Zip64 ? 20 : 0);
                    bw.Write(extraFieldLength);

                    bw.Write(bFileName, 0, fileNameLength);

                    _extraLocation = (ulong)zipFs.Position;
                    if (Zip64)
                        bw.Write(new byte[20], 0, extraFieldLength);
                }
            }

            public void LocalFileHeaderFake(ulong filePosition, ulong uncompressedSize, ulong compressedSize, byte[] crc32, MemoryStream ms)
            {
                using (BinaryWriter bw = new BinaryWriter(ms, Encoding.UTF8, true))
                {
                    RelativeOffsetOfLocalHeader = filePosition;
                    TrrntZip = true;
                    UncompressedSize = uncompressedSize;
                    _compressedSize = compressedSize;
                    CRC = crc32;

                    Zip64 = UncompressedSize >= 0xffffffff || _compressedSize >= 0xffffffff;

                    byte[] bFileName;
                    if (IsUnicode(FileName))
                    {
                        GeneralPurposeBitFlag |= 1 << 11;
                        bFileName = Encoding.UTF8.GetBytes(FileName);
                    }
                    else
                    {
                        bFileName = GetBytes(FileName);
                    }

                    ushort versionNeededToExtract = (ushort)(Zip64 ? 45 : 20);

                    const uint header = 0x4034B50;
                    bw.Write(header);
                    bw.Write(versionNeededToExtract);
                    bw.Write(GeneralPurposeBitFlag);
                    bw.Write(_compressionMethod);
                    if (_lastModFileDateTime == null)
                    {
                        bw.Write(_lastModFileTime);
                        bw.Write(_lastModFileDate);
                    }
                    else
                    {
                        bw.Write(_lastModFileDateTime.Value);
                    }

                    uint tCompressedSize;
                    uint tUncompressedSize;
                    if (Zip64)
                    {
                        tCompressedSize = 0xffffffff;
                        tUncompressedSize = 0xffffffff;
                    }
                    else
                    {
                        tCompressedSize = (uint)_compressedSize;
                        tUncompressedSize = (uint)UncompressedSize;
                    }

                    bw.Write(CRC[3]);
                    bw.Write(CRC[2]);
                    bw.Write(CRC[1]);
                    bw.Write(CRC[0]);
                    bw.Write(tCompressedSize);
                    bw.Write(tUncompressedSize);

                    ushort fileNameLength = (ushort)bFileName.Length;
                    bw.Write(fileNameLength);

                    ushort extraFieldLength = (ushort)(Zip64 ? 20 : 0);
                    bw.Write(extraFieldLength);

                    bw.Write(bFileName, 0, fileNameLength);

                    if (Zip64)
                    {
                        bw.Write((ushort)0x0001); // id
                        bw.Write((ushort)16); // data length
                        bw.Write(UncompressedSize);
                        bw.Write(_compressedSize);
                    }
                }
            }
            public ZipReturn LocalFileOpenReadStream(Stream zipFs, bool raw, out Stream readStream, out ulong streamSize, out ushort compressionMethod)
            {
                streamSize = 0;
                compressionMethod = _compressionMethod;

                readStream = null;
                zipFs.Seek((long)_dataLocation, SeekOrigin.Begin);

                switch (_compressionMethod)
                {
                    case 8:
                        if (raw)
                        {
                            readStream = zipFs;
                            streamSize = _compressedSize;
                        }
                        else
                        {
                            readStream = new ZlibBaseStream(zipFs, CompressionMode.Decompress, CompressionLevel.Default, ZlibStreamFlavor.DEFLATE, true);
                            streamSize = UncompressedSize;
                        }
                        break;
                    case 0:
                        readStream = zipFs;
                        streamSize = _compressedSize; // same as UncompressedSize
                        break;
                }

                return readStream == null ? ZipReturn.ZipErrorGettingDataStream : ZipReturn.ZipGood;
            }

            public ZipReturn LocalFileOpenWriteStream(Stream zipFs, bool raw, bool trrntZip, ulong uncompressedSize, ushort compressionMethod, out Stream writeStream)
            {
                UncompressedSize = uncompressedSize;
                _compressionMethod = compressionMethod;

                LocalFileHeaderWrite(zipFs);
                _dataLocation = (ulong)zipFs.Position;

                if (raw)
                {
                    writeStream = zipFs;
                    TrrntZip = trrntZip;
                }
                else
                {
                    if (compressionMethod == 0)
                    {
                        writeStream = zipFs;
                        TrrntZip = false;
                    }
                    else
                    {
                        writeStream = new ZlibBaseStream(zipFs, CompressionMode.Compress, CompressionLevel.BestCompression, ZlibStreamFlavor.DEFLATE, true);
                        TrrntZip = true;
                    }
                }

                return writeStream == null ? ZipReturn.ZipErrorGettingDataStream : ZipReturn.ZipGood;
            }

            public ZipReturn LocalFileCloseWriteStream(Stream zipFs, byte[] crc32)
            {
                _compressedSize = (ulong)zipFs.Position - _dataLocation;

                if (_compressedSize == 0 && UncompressedSize == 0)
                {
                    LocalFileAddZeroLengthFile(zipFs);
                    _compressedSize = (ulong)zipFs.Position - _dataLocation;
                }
                else if (_compressedSize == 0 && UncompressedSize != 0)
                {
                    return ZipReturn.ZipErrorWritingToOutputStream;
                }

                CRC = crc32;
                WriteCompressedSize(zipFs);

                return ZipReturn.ZipGood;
            }

            private void FixFileForZip64(Stream zipFs)
            {
                long posNow = zipFs.Position;
                using (BinaryWriter bw = new BinaryWriter(zipFs, Encoding.UTF8, true))
                {
                    // _crc32Loction - 10  needs set to 45
                    zipFs.Seek((long)_crc32Location - 10, SeekOrigin.Begin);
                    ushort versionNeededToExtract = 45;
                    bw.Write(versionNeededToExtract);

                    zipFs.Seek((long)_crc32Location + 14, SeekOrigin.Begin);
                    ushort extraFieldLength = 20;
                    bw.Write(extraFieldLength);
                }
                ExpandFile(zipFs, (long)_extraLocation, posNow, 20);
                zipFs.Position = posNow + 20;
            }

            private static void ExpandFile(Stream stream, long offset, long length, int extraBytes)
            {
                const int bufferSize = 40960;
                byte[] buffer = new byte[bufferSize];
                // Expand file
                long pos = length;
                while (pos > offset)
                {
                    int toRead = pos - bufferSize >= offset ? bufferSize : (int)(pos - offset);
                    pos -= toRead;
                    stream.Position = pos;
                    stream.Read(buffer, 0, toRead);
                    stream.Position = pos + extraBytes;
                    stream.Write(buffer, 0, toRead);
                }
            }

            private void WriteCompressedSize(Stream zipFs)
            {
                if (_compressedSize >= 0xffffffff && !Zip64)
                {
                    Zip64 = true;
                    FixFileForZip64(zipFs);
                }


                long posNow = zipFs.Position;
                zipFs.Seek((long)_crc32Location, SeekOrigin.Begin);
                using (BinaryWriter bw = new BinaryWriter(zipFs, Encoding.UTF8, true))
                {
                    uint tCompressedSize;
                    uint tUncompressedSize;
                    if (Zip64)
                    {
                        tCompressedSize = 0xffffffff;
                        tUncompressedSize = 0xffffffff;
                    }
                    else
                    {
                        tCompressedSize = (uint)_compressedSize;
                        tUncompressedSize = (uint)UncompressedSize;
                    }

                    bw.Write(CRC[3]);
                    bw.Write(CRC[2]);
                    bw.Write(CRC[1]);
                    bw.Write(CRC[0]);
                    bw.Write(tCompressedSize);
                    bw.Write(tUncompressedSize);


                    // also need to write extradata
                    if (Zip64)
                    {
                        zipFs.Seek((long)_extraLocation, SeekOrigin.Begin);
                        bw.Write((ushort)0x0001); // id
                        bw.Write((ushort)16); // data length
                        bw.Write(UncompressedSize);
                        bw.Write(_compressedSize);
                    }
                }

                zipFs.Seek(posNow, SeekOrigin.Begin);
            }

            public static void LocalFileAddZeroLengthFile(Stream zipFs)
            {
                zipFs.WriteByte(03);
                zipFs.WriteByte(00);
            }

            private static byte[] ReadCRC(BinaryReader br)
            {
                byte[] tCRC = new byte[4];
                tCRC[3] = br.ReadByte();
                tCRC[2] = br.ReadByte();
                tCRC[1] = br.ReadByte();
                tCRC[0] = br.ReadByte();
                return tCRC;
            }
        }
    }
}