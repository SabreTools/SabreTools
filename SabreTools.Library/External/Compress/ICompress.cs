using System.IO;

namespace Compress
{
    public interface ICompress
    {
        int LocalFilesCount();

        string Filename(int i);
        ulong? LocalHeader(int i);
        ulong UncompressedSize(int i);
        byte[] CRC32(int i);

        bool IsDirectory(int i);

        ZipOpenType ZipOpen { get; }

        ZipReturn ZipFileOpen(string newFilename, long timestamp = -1, bool readHeaders = true);

        ZipReturn ZipFileOpen(Stream inStream);
        void ZipFileClose();

        ZipReturn ZipFileOpenReadStream(int index, out Stream stream, out ulong streamSize);
        ZipReturn ZipFileOpenWriteStream(bool raw, bool trrntzip, string filename, ulong uncompressedSize, ushort compressionMethod, out Stream stream);
        ZipReturn ZipFileCloseReadStream();


        ZipStatus ZipStatus { get; }

        string ZipFilename { get; }
        long TimeStamp { get; }

        void ZipFileAddZeroLengthFile();

        ZipReturn ZipFileCreate(string newFilename);
        ZipReturn ZipFileCloseWriteStream(byte[] crc32);
        void ZipFileCloseFailed();

    }
}