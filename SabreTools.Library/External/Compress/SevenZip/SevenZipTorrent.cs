using System.IO;
using System.Text;

namespace Compress.SevenZip
{
    public partial class SevenZ
    {
        // not finalized yet, so do not use
        private void WriteRomVault7Zip(BinaryWriter bw, ulong headerPos, ulong headerLength, uint headerCRC)
        {
            const string sig = "RomVault7Z01";
            byte[] RV7Zid = Util.Enc.GetBytes(sig);

            // RomVault 7Zip torrent header
            // 12 bytes :  RomVault7Zip
            //  4 bytes :  HeaderCRC
            //  8 bytes :  HeaderPos
            //  8 bytes :  HeaderLength

            bw.Write(RV7Zid);
            bw.Write(headerCRC);
            bw.Write(headerPos);
            bw.Write(headerLength);

            ZipStatus = ZipStatus.TrrntZip;
        }

        private bool IsRomVault7Z(long testBaseOffset, ulong testHeaderPos, ulong testHeaderLength, uint testHeaderCRC)
        {
            long length = _zipFs.Length;
            if (length < 32)
            {
                return false;
            }
            _zipFs.Seek(_baseOffset + (long)testHeaderPos - 32, SeekOrigin.Begin);

            const string sig = "RomVault7Z01";
            byte[] rv7Zid = Util.Enc.GetBytes(sig);

            byte[] header = new byte[12];
            _zipFs.Read(header, 0, 12);
            for (int i = 0; i < 12; i++)
            {
                if (header[i] != rv7Zid[i])
                {
                    return false;
                }
            }

            uint headerCRC;
            ulong headerOffset; // is location of header in file
            ulong headerSize;
            using (BinaryReader br = new BinaryReader(_zipFs, Encoding.UTF8, true))
            {
                headerCRC = br.ReadUInt32();
                headerOffset = br.ReadUInt64();
                headerSize = br.ReadUInt64();
            }

            if (headerCRC != testHeaderCRC)
                return false;

            if (headerOffset != testHeaderPos + (ulong)testBaseOffset)
                return false;

            return headerSize == testHeaderLength;
        }
        private bool Istorrent7Z()
        {
            const int crcsz = 128;
            const int t7ZsigSize = 16 + 1 + 9 + 4 + 4;
            byte[] kSignature = { (byte)'7', (byte)'z', 0xBC, 0xAF, 0x27, 0x1C };
            int kSignatureSize = kSignature.Length;
            const string sig = "\xa9\x9f\xd1\x57\x08\xa9\xd7\xea\x29\x64\xb2\x36\x1b\x83\x52\x33\x01torrent7z_0.9beta";
            byte[] t7Zid = Util.Enc.GetBytes(sig);
            int t7ZidSize = t7Zid.Length;

            const int tmpbufsize = 256 + t7ZsigSize + 8 + 4;
            byte[] buffer = new byte[tmpbufsize];

            // read fist 128 bytes, pad with zeros if less bytes
            int bufferPos = 0;
            _zipFs.Seek(0, SeekOrigin.Begin);
            int ar = _zipFs.Read(buffer, bufferPos, crcsz);
            if (ar < crcsz)
            {
                Util.memset(buffer, bufferPos + ar, 0, crcsz - ar);
            }
            bufferPos = crcsz;

            long foffs = _zipFs.Length;
            int endReadLength = crcsz + t7ZsigSize + 4;
            foffs = foffs < endReadLength ? 0 : foffs - endReadLength;

            _zipFs.Seek(foffs, SeekOrigin.Begin);

            ar = _zipFs.Read(buffer, bufferPos, endReadLength);
            if (ar < endReadLength)
            {
                if (ar >= t7ZsigSize + 4)
                {
                    ar -= t7ZsigSize + 4;
                }
                if (ar < kSignatureSize)
                {
                    ar = kSignatureSize;
                }
                Util.memset(buffer, bufferPos + ar, 0, crcsz - ar);
                Util.memcpyr(buffer, crcsz * 2 + 8, buffer, bufferPos + ar, t7ZsigSize + 4);
            }
            else
            {
                Util.memcpyr(buffer, crcsz * 2 + 8, buffer, crcsz * 2, t7ZsigSize + 4);
            }

            foffs = _zipFs.Length;
            foffs -= t7ZsigSize + 4;

            //memcpy(buffer, crcsz * 2, &foffs, 8);
            buffer[crcsz * 2 + 0] = (byte)((foffs >> 0) & 0xff);
            buffer[crcsz * 2 + 1] = (byte)((foffs >> 8) & 0xff);
            buffer[crcsz * 2 + 2] = (byte)((foffs >> 16) & 0xff);
            buffer[crcsz * 2 + 3] = (byte)((foffs >> 24) & 0xff);
            buffer[crcsz * 2 + 4] = 0;
            buffer[crcsz * 2 + 5] = 0;
            buffer[crcsz * 2 + 6] = 0;
            buffer[crcsz * 2 + 7] = 0;

            if (Util.memcmp(buffer, 0, kSignature, kSignatureSize))
            {
                t7Zid[16] = buffer[crcsz * 2 + 4 + 8 + 16];
                if (Util.memcmp(buffer, crcsz * 2 + 4 + 8, t7Zid, t7ZidSize))
                {
                    uint inCrc32 = (uint)(buffer[crcsz * 2 + 8 + 0] +
                                           (buffer[crcsz * 2 + 8 + 1] << 8) +
                                           (buffer[crcsz * 2 + 8 + 2] << 16) +
                                           (buffer[crcsz * 2 + 8 + 3] << 24));

                    buffer[crcsz * 2 + 8 + 0] = 0xff;
                    buffer[crcsz * 2 + 8 + 1] = 0xff;
                    buffer[crcsz * 2 + 8 + 2] = 0xff;
                    buffer[crcsz * 2 + 8 + 3] = 0xff;

                    uint calcCrc32 = Utils.CRC.CalculateDigest(buffer, 0, crcsz * 2 + 8 + t7ZsigSize + 4);

                    if (inCrc32 == calcCrc32)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}