using System.Collections.Generic;
using System.IO;
using System.Text;
using Compress.SevenZip.Structure;
using FileInfo = RVIO.FileInfo;

namespace Compress.SevenZip
{
    public partial class SevenZ : ICompress
    {
        public static bool supportZstd
        {
            get;
            private set;
        }

        public static void TestForZstd()
        {
            supportZstd = RVIO.File.Exists("libzstd.dll");
        }



        private class LocalFile
        {
            public string FileName;
            public ulong UncompressedSize;
            public bool IsDirectory;
            public byte[] CRC;
            public int StreamIndex;
            public ulong StreamOffset;
            public ZipReturn FileStatus = ZipReturn.ZipUntested;
        }


        private List<LocalFile> _localFiles = new List<LocalFile>();

        private FileInfo _zipFileInfo;

        private Stream _zipFs;

        private SignatureHeader _signatureHeader;

        private bool _compressed = true;


        private long _baseOffset;

        public string ZipFilename => _zipFileInfo != null ? _zipFileInfo.FullName : string.Empty;

        public long TimeStamp => _zipFileInfo?.LastWriteTime ?? 0;

        public ZipOpenType ZipOpen { get; private set; }
        public ZipStatus ZipStatus { get; private set; }

        public int LocalFilesCount()
        {
            return _localFiles.Count;
        }

        public string Filename(int i)
        {
            return _localFiles[i].FileName;
        }

        public ulong? LocalHeader(int i)
        {
            return 0;
        }

        public ulong UncompressedSize(int i)
        {
            return _localFiles[i].UncompressedSize;
        }

        public int StreamIndex(int i)
        {
            return _localFiles[i].StreamIndex;

        }

        public ZipReturn FileStatus(int i)
        {
            return _localFiles[i].FileStatus;
        }

        public byte[] CRC32(int i)
        {
            return _localFiles[i].CRC;
        }

        public void ZipFileCloseFailed()
        {
            switch (ZipOpen)
            {
                case ZipOpenType.Closed:
                    return;
                case ZipOpenType.OpenRead:
                    ZipFileCloseReadStream();
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

        public bool IsDirectory(int i)
        {
            return _localFiles[i].IsDirectory;
        }











        public void ZipFileClose()
        {
            switch (ZipOpen)
            {
                case ZipOpenType.Closed:
                    return;
                case ZipOpenType.OpenRead:
                    ZipFileCloseReadStream();
                    if (_zipFs != null)
                    {
                        _zipFs.Close();
                        _zipFs.Dispose();
                    }
                    ZipOpen = ZipOpenType.Closed;
                    return;
                case ZipOpenType.OpenWrite:
                    CloseWriting7Zip();
                    if (_zipFileInfo != null)
                        _zipFileInfo = new FileInfo(_zipFileInfo.FullName);
                    break;
            }

            ZipOpen = ZipOpenType.Closed;
        }


        private Header _header;

        public StringBuilder HeaderReport()
        {
            StringBuilder sb = new StringBuilder();

            if (_header == null)
            {
                sb.AppendLine("Null Header");
                return sb;
            }

            _header.Report(ref sb);

            return sb;
        }
    }
}