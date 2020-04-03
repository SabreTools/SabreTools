using System.IO;
using System.Text;

namespace Compress.SevenZip
{
    public enum HeaderProperty
    {
        kEnd,
        kHeader,

        kArchiveProperties,

        kAdditionalStreamsInfo,
        kMainStreamsInfo,
        kFilesInfo,

        kPackInfo,
        kUnPackInfo,
        kSubStreamsInfo,

        kSize,
        kCRC,

        kFolder,

        kCodersUnPackSize,
        kNumUnPackStream,

        kEmptyStream,
        kEmptyFile,
        kAnti,

        kName,
        kCreationTime,
        kLastAccessTime,
        kLastWriteTime,
        kWinAttributes,
        kComment,

        kEncodedHeader,

        kStartPos,
        kDummy
    }

    public static class Util
    {
        public static readonly Encoding Enc = Encoding.GetEncoding(28591);

        public static void memset(byte[] buffer, int start, byte val, int len)
        {
            for (int i = 0; i < len; i++)
            {
                buffer[start + i] = val;
            }
        }

        public static void memcpyr(byte[] destBuffer, int destPoint, byte[] sourceBuffer, int sourcePoint, int len)
        {
            for (int i = len - 1; i >= 0; i--)
            {
                destBuffer[destPoint + i] = sourceBuffer[sourcePoint + i];
            }
        }


        public static bool memcmp(byte[] buffer1, int offset, byte[] buffer2, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (buffer1[offset + i] != buffer2[i])
                {
                    return false;
                }
            }
            return true;
        }


        public static bool Compare(this byte[] b1, byte[] b2)
        {
            if ((b1 == null) || (b2 == null))
            {
                return false;
            }

            if (b1.Length != b2.Length)
            {
                return false;
            }

            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                {
                    return false;
                }
            }

            return true;
        }


        public static ulong ReadEncodedUInt64(this BinaryReader br)
        {
            byte mask = 0x80;
            int i;
            byte firstByte = br.ReadByte();
            ulong value = 0;
            for (i = 0; i < 8; i++)
            {
                if ((firstByte & mask) == 0)
                {
                    ulong highPart = (ulong)(firstByte & (mask - 1));
                    value += highPart << (8 * i);
                    return value;
                }
                byte b = br.ReadByte();
                value |= (ulong)b << (8 * i);
                mask >>= 1;
            }
            return value;
        }

        public static void WriteEncodedUInt64(this BinaryWriter bw, ulong value)
        {
            byte firstByte = 0;
            byte mask = 0x80;
            int i;
            for (i = 0; i < 8; i++)
            {
                if (value < (ulong)1 << (7 * (i + 1)))
                {
                    firstByte |= (byte)(value >> (8 * i));
                    break;
                }
                firstByte |= mask;
                mask >>= 1;
            }
            bw.Write(firstByte);
            for (; i > 0; i--)
            {
                bw.Write((byte)value);
                value >>= 8;
            }
        }

        public static string ReadName(this BinaryReader br)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (; ; )
            {
                char c = (char)br.ReadUInt16();
                if (c == 0)
                {
                    return stringBuilder.ToString();
                }
                stringBuilder.Append(c);
            }
        }

        public static void WriteName(this BinaryWriter bw, string name)
        {
            char[] chars = name.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                bw.Write((ushort)chars[i]);
            }
            bw.Write((ushort)0);
        }


        public static void UnPackCRCs(BinaryReader br, ulong numItems, out uint?[] digests)
        {
            bool[] digestsDefined = ReadBoolFlagsDefaultTrue(br, numItems);
            digests = new uint?[numItems];
            for (ulong i = 0; i < numItems; i++)
            {
                if (digestsDefined[i])
                {
                    digests[i] = br.ReadUInt32();
                }
            }
        }

        public static void WritePackedCRCs(BinaryWriter bw, uint?[] digests)
        {
            bool[] digestsDefined = new bool[digests.Length];
            for (int i = 0; i < digests.Length; i++)
            {
                digestsDefined[i] = digests[i] != null;
            }
            WriteBoolFlagsDefaultTrue(bw, digestsDefined);
            for (int i = 0; i < digests.Length; i++)
            {
                if (digestsDefined[i])
                {
                    bw.Write((uint)digests[i]);
                }
            }
        }

        private static bool[] ReadBoolFlagsDefaultTrue(BinaryReader br, ulong numItems)
        {
            byte allAreDefined = br.ReadByte();
            if (allAreDefined == 0)
            {
                return ReadBoolFlags(br, numItems);
            }
            bool[] flags = new bool[numItems];
            for (ulong i = 0; i < numItems; i++)
            {
                flags[i] = true;
            }
            return flags;
        }

        private static void WriteBoolFlagsDefaultTrue(BinaryWriter bw, bool[] bArray)
        {
            bool allTrue = true;
            foreach (bool b in bArray)
            {
                allTrue &= b;
            }

            if (allTrue)
            {
                bw.Write((byte)1);
                return;
            }
            WriteBoolFlags(bw, bArray);
        }

        public static bool[] ReadBoolFlags(BinaryReader br, ulong numItems)
        {
            byte b = 0;
            byte mask = 0;

            bool[] flags = new bool[numItems];
            for (ulong i = 0; i < numItems; i++)
            {
                if (mask == 0)
                {
                    b = br.ReadByte();
                    mask = 0x80;
                }

                flags[i] = (b & mask) != 0;

                mask >>= 1;
            }
            return flags;
        }

        public static bool[] ReadBoolFlags2(BinaryReader br, ulong numItems)
        {
            byte allAreDefined = br.ReadByte();
            if (allAreDefined == 0)
            {
                return ReadBoolFlags(br, numItems);
            }


            bool[] flags = new bool[numItems];
            for (ulong i = 0; i < numItems; i++)
            {
                flags[i] = true;
            }
            return flags;
        }

        public static void WriteUint32Def(BinaryWriter br, uint[] values)
        {
            br.WriteEncodedUInt64((ulong)(values.Length * 4 + 2));
            br.Write((byte)1);
            br.Write((byte)0);
            for (int i = 0; i < values.Length; i++)
            {
                br.Write(values[i]);
            }
        }

        public static uint[] ReadUInt32Def(BinaryReader br, ulong numItems)
        {
            uint[] v = new uint[numItems];
            bool[] defs = ReadBoolFlags2(br, numItems);
            byte tmp = br.ReadByte();
            for (ulong i = 0; i < numItems; i++)
            {
                v[i] = defs[i] ? br.ReadUInt32() : 0;
            }

            return v;
        }

        public static ulong[] ReadUInt64Def(BinaryReader br, ulong numItems)
        {
            ulong[] v = new ulong[numItems];
            bool[] defs = ReadBoolFlags2(br, numItems);
            byte tmp = br.ReadByte();
            for (ulong i = 0; i < numItems; i++)
            {
                v[i] = defs[i] ? br.ReadUInt64() : 0;
            }

            return v;
        }

        public static void WriteBoolFlags(BinaryWriter bw, bool[] bArray)
        {
            bw.WriteEncodedUInt64((ulong)((bArray.Length + 7) / 8));
            byte mask = 0x80;
            byte tmpOut = 0;
            for (int i = 0; i < bArray.Length; i++)
            {
                if (bArray[i])
                {
                    tmpOut |= mask;
                }

                mask >>= 1;
                if (mask != 0)
                {
                    continue;
                }

                bw.Write(tmpOut);
                mask = 0x80;
                tmpOut = 0;
            }
            if (mask != 0x80)
            {
                bw.Write(tmpOut);
            }
        }

        public static byte[] uinttobytes(uint? crc)
        {
            if (crc == null)
            {
                return null;
            }
            uint c = (uint)crc;

            byte[] b = new byte[4];
            b[0] = (byte)((c >> 24) & 0xff);
            b[1] = (byte)((c >> 16) & 0xff);
            b[2] = (byte)((c >> 8) & 0xff);
            b[3] = (byte)((c >> 0) & 0xff);
            return b;
        }

        public static uint? bytestouint(byte[] crc)
        {
            if (crc == null)
            {
                return null;
            }

            return (uint?)((crc[0] << 24) | (crc[1] << 16) | (crc[2] << 8) | (crc[3] << 0));
        }

        public static bool ByteArrCompare(byte[] b0, byte[] b1)
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
    }
}