using System;
using System.IO;
using Compress.Utils;
using Path = RVIO.Path;
using FileInfo = RVIO.FileInfo;
using FileStream = RVIO.FileStream;

namespace Compress.File
{
    public class File : ICompress
    {
        private FileInfo _fileInfo;
        private Stream _inStream;
        private byte[] _crc;

        public string ZipFilename => _fileInfo?.FullName ?? "";

        public long TimeStamp => _fileInfo?.LastWriteTime ?? 0;

        public ZipOpenType ZipOpen { get; private set; }

        public ZipStatus ZipStatus { get; private set; }

        public int LocalFilesCount()
        {
            return 1;
        }

        public string Filename(int i)
        {
            return Path.GetFileName(ZipFilename);
        }

        public bool IsDirectory(int i)
        {
            return RVIO.Directory.Exists(ZipFilename);
        }

        public ulong UncompressedSize(int i)
        {
            return _fileInfo != null ? (ulong)_fileInfo.Length : (ulong)_inStream.Length;
        }

        public ulong? LocalHeader(int i)
        {
            return 0;
        }

        public ZipReturn FileStatus(int i)
        {
            return ZipReturn.ZipGood;
        }

        public byte[] CRC32(int i)
        {
            return _crc;
        }

        public long LastModified(int i)
        {
            return _fileInfo.LastWriteTime;
        }

        public long? Accessed(int i)
        {
            return _fileInfo.LastAccessTime;
        }

        public long? Created(int i)
        {
            return _fileInfo.CreationTime;
        }
        
        public ZipReturn ZipFileCreate(string newFilename)
        {
            if (ZipOpen != ZipOpenType.Closed)
            {
                return ZipReturn.ZipFileAlreadyOpen;
            }

            DirUtil.CreateDirForFile(newFilename);
            _fileInfo = new FileInfo(newFilename);

            int errorCode = FileStream.OpenFileWrite(newFilename, out _inStream);
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
                // if FileInfo is valid (not null), then we created the stream and need to close it.
                // if we open from a stream, then FileInfo will be null, and we do not need to close the stream.
                if (_fileInfo != null && _inStream != null)
                {
                    _inStream.Close();
                    _inStream.Dispose();
                    _inStream = null;
                }
                ZipOpen = ZipOpenType.Closed;
                return;
            }

            if (ZipOpen == ZipOpenType.OpenWrite)
            {
                _inStream.Flush();
                _inStream.Close();
                _inStream.Dispose();
                _inStream = null;
                _fileInfo = new FileInfo(_fileInfo.FullName);
                ZipOpen = ZipOpenType.Closed;
            }
        }

        public ZipReturn ZipFileOpen(string newFilename, long timestamp, bool readHeaders)
        {
            ZipFileClose();
            ZipStatus = ZipStatus.None;
            _fileInfo = null;

            try
            {
                if (!RVIO.File.Exists(newFilename))
                {
                    ZipFileClose();
                    return ZipReturn.ZipErrorFileNotFound;
                }
                _fileInfo = new FileInfo(newFilename);
                if (timestamp != -1 && _fileInfo.LastWriteTime != timestamp)
                {
                    ZipFileClose();
                    return ZipReturn.ZipErrorTimeStamp;
                }
                int errorCode = FileStream.OpenFileRead(newFilename, out _inStream);
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

            //return ZipFileReadHeaders();
            return ZipReturn.ZipGood;
        }


        public ZipReturn ZipFileOpen(Stream inStream)
        {
            ZipFileClose();
            ZipStatus = ZipStatus.None;
            _fileInfo = null;
            _inStream = inStream;
            ZipOpen = ZipOpenType.OpenRead;

            //return ZipFileReadHeaders();
            return ZipReturn.ZipGood;
        }

        public void ZipFileAddZeroLengthFile()
        {
            throw new NotImplementedException();
        }

        public ZipReturn ZipFileCloseWriteStream(byte[] crc32)
        {
            _crc = crc32;
            return ZipReturn.ZipGood;
        }

        public void ZipFileCloseFailed()
        {
            throw new NotImplementedException();
        }

        public ZipReturn ZipFileOpenReadStream(int index, out Stream stream, out ulong streamSize)
        {
            _inStream.Position = 0;
            stream = _inStream;
            streamSize = (ulong)_inStream.Length;
            return ZipReturn.ZipGood;
        }

        public ZipReturn ZipFileOpenWriteStream(bool raw, bool trrntzip, string filename, ulong uncompressedSize, ushort compressionMethod, out Stream stream, TimeStamps dateTime)
        {
            _inStream.Position = 0;
            stream = _inStream;
            return ZipReturn.ZipGood;
        }

        public ZipReturn ZipFileCloseReadStream()
        {
            return ZipReturn.ZipGood;
        }
    }
}
