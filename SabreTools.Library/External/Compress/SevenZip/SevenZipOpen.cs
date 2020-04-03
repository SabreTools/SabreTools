using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Compress.SevenZip.Structure;
using Compress.Utils;
using FileInfo = RVIO.FileInfo;
using FileStream = RVIO.FileStream;

namespace Compress.SevenZip
{
    public partial class SevenZ
    {
        public ZipReturn ZipFileOpen(string filename, long timestamp, bool readHeaders)
        {
            ZipFileClose();
            Debug.WriteLine(filename);
            #region open file stream

            try
            {
                if (!RVIO.File.Exists(filename))
                {
                    ZipFileClose();
                    return ZipReturn.ZipErrorFileNotFound;
                }
                _zipFileInfo = new FileInfo(filename);
                if ((timestamp != -1) && (_zipFileInfo.LastWriteTime != timestamp))
                {
                    ZipFileClose();
                    return ZipReturn.ZipErrorTimeStamp;
                }
                int errorCode = FileStream.OpenFileRead(filename, out _zipFs);
                if (errorCode != 0)
                {
                    ZipFileClose();
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

            #endregion

            ZipOpen = ZipOpenType.OpenRead;
            ZipStatus = ZipStatus.None;

            return ZipFileReadHeaders();
        }


        public ZipReturn ZipFileOpen(Stream inStream)
        {
            ZipFileClose();
            _zipFileInfo = null;
            _zipFs = inStream;
            ZipOpen = ZipOpenType.OpenRead;
            ZipStatus = ZipStatus.None;
            return ZipFileReadHeaders();
        }



        private ZipReturn ZipFileReadHeaders()
        {
            try
            {
                SignatureHeader signatureHeader = new SignatureHeader();
                if (!signatureHeader.Read(_zipFs))
                {
                    return ZipReturn.ZipSignatureError;
                }

                _baseOffset = _zipFs.Position;

                _zipFs.Seek(_baseOffset + (long)signatureHeader.NextHeaderOffset, SeekOrigin.Begin);
                byte[] mainHeader = new byte[signatureHeader.NextHeaderSize];
                _zipFs.Read(mainHeader, 0, (int)signatureHeader.NextHeaderSize);
                if (!CRC.VerifyDigest(signatureHeader.NextHeaderCRC, mainHeader, 0, (uint)signatureHeader.NextHeaderSize))
                    return ZipReturn.Zip64EndOfCentralDirError;

                if (signatureHeader.NextHeaderSize != 0)
                {
                    _zipFs.Seek(_baseOffset + (long)signatureHeader.NextHeaderOffset, SeekOrigin.Begin);
                    ZipReturn zr = Header.ReadHeaderOrPackedHeader(_zipFs, _baseOffset, out _header);
                    if (zr != ZipReturn.ZipGood)
                    {
                        return zr;
                    }
                }


                ZipStatus = ZipStatus.None;
                ZipStatus |= IsRomVault7Z(_baseOffset, signatureHeader.NextHeaderOffset, signatureHeader.NextHeaderSize, signatureHeader.NextHeaderCRC) ? ZipStatus.TrrntZip : ZipStatus.None;

                _zipFs.Seek(_baseOffset + (long)(signatureHeader.NextHeaderOffset + signatureHeader.NextHeaderSize), SeekOrigin.Begin);
                ZipStatus |= Istorrent7Z() ? ZipStatus.Trrnt7Zip : ZipStatus.None;
                PopulateLocalFiles(out _localFiles);

                return ZipReturn.ZipGood;
            }
            catch
            {
                ZipFileClose();
                return ZipReturn.ZipErrorReadingFile;
            }
        }


        private void PopulateLocalFiles(out List<LocalFile> localFiles)
        {
            int emptyFileIndex = 0;
            int folderIndex = 0;
            int unpackedStreamsIndex = 0;
            ulong streamOffset = 0;
            localFiles = new List<LocalFile>();

            if (_header == null)
                return;

            for (int i = 0; i < _header.FileInfo.Names.Length; i++)
            {
                LocalFile lf = new LocalFile { FileName = _header.FileInfo.Names[i] };

                if ((_header.FileInfo.EmptyStreamFlags == null) || !_header.FileInfo.EmptyStreamFlags[i])
                {
                    lf.StreamIndex = folderIndex;
                    lf.StreamOffset = streamOffset;
                    lf.UncompressedSize = _header.StreamsInfo.Folders[folderIndex].UnpackedStreamInfo[unpackedStreamsIndex].UnpackedSize;
                    lf.CRC = Util.uinttobytes(_header.StreamsInfo.Folders[folderIndex].UnpackedStreamInfo[unpackedStreamsIndex].Crc);

                    streamOffset += lf.UncompressedSize;
                    unpackedStreamsIndex++;

                    if (unpackedStreamsIndex >= _header.StreamsInfo.Folders[folderIndex].UnpackedStreamInfo.Length)
                    {
                        folderIndex++;
                        unpackedStreamsIndex = 0;
                        streamOffset = 0;
                    }
                }
                else
                {
                    lf.UncompressedSize = 0;
                    lf.CRC = new byte[] { 0, 0, 0, 0 };
                    lf.IsDirectory = (_header.FileInfo.EmptyFileFlags == null) || !_header.FileInfo.EmptyFileFlags[emptyFileIndex++];

                    if (lf.IsDirectory)
                    {
                        if (lf.FileName.Substring(lf.FileName.Length - 1, 1) != "/")
                        {
                            lf.FileName += "/";
                        }
                    }
                }

                localFiles.Add(lf);
            }
        }


    }
}