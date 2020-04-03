using System;
using System.IO;

namespace Compress.SevenZip.Filters
{
    public class BCJ2Filter : Stream
    {
        private Stream baseStream;

        private long position = 0;
        private byte[] output = new byte[4];
        private int outputOffset = 0;
        private int outputCount = 0;

        private Stream control;
        private Stream data1;
        private Stream data2;

        private ushort[] p = new ushort[256 + 2];
        private uint range, code;
        private byte prevByte = 0;

        private const int kNumTopBits = 24;
        private const int kTopValue = 1 << kNumTopBits;

        private const int kNumBitModelTotalBits = 11;
        private const int kBitModelTotal = 1 << kNumBitModelTotalBits;
        private const int kNumMoveBits = 5;

        private static bool IsJ(byte b0, byte b1)
        {
            return (b1 & 0xFE) == 0xE8 || IsJcc(b0, b1);
        }

        private static bool IsJcc(byte b0, byte b1)
        {
            return b0 == 0x0F && (b1 & 0xF0) == 0x80;
        }

        public BCJ2Filter(Stream baseStream, Stream data1, Stream data2, Stream control)
        {
            this.control = control;
            this.data1 = data1;
            this.data2 = data2;
            this.baseStream = baseStream;

            int i;
            for (i = 0; i < p.Length; i++)
                p[i] = kBitModelTotal >> 1;

            code = 0;
            range = 0xFFFFFFFF;

            byte[] controlbuf = new byte[5];
            control.Read(controlbuf, 0, 5);

            for (i = 0; i < 5; i++)
                code = (code << 8) | controlbuf[i];
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { return baseStream.Length + data1.Length + data2.Length; }
        }

        public override long Position
        {
            get
            {
                return position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int size = 0;
            byte b = 0;

            while (size < count)
            {
                while (outputOffset < outputCount)
                {
                    b = output[outputOffset++];
                    buffer[offset++] = b;
                    size++;
                    position++;

                    prevByte = b;
                    if (size == count)
                        return size;
                }

                b = (byte)baseStream.ReadByte();
                buffer[offset++] = b;
                size++;
                position++;

                if (!IsJ(prevByte, b))
                    prevByte = b;
                else
                {
                    int prob;
                    if (b == 0xE8)
                        prob = prevByte;
                    else if (b == 0xE9)
                        prob = 256;
                    else
                        prob = 257;

                    uint bound = (range >> kNumBitModelTotalBits) * p[prob];
                    if (code < bound)
                    {
                        range = bound;
                        p[prob] += (ushort)((kBitModelTotal - p[prob]) >> kNumMoveBits);
                        if (range < kTopValue)
                        {
                            range <<= 8;
                            code = (code << 8) | (byte)control.ReadByte();
                        }
                        prevByte = b;
                    }
                    else
                    {
                        range -= bound;
                        code -= bound;
                        p[prob] -= (ushort)(p[prob] >> kNumMoveBits);
                        if (range < kTopValue)
                        {
                            range <<= 8;
                            code = (code << 8) | (byte)control.ReadByte();
                        }

                        uint dest;
                        if (b == 0xE8)
                            dest = (uint)((data1.ReadByte() << 24) | (data1.ReadByte() << 16) | (data1.ReadByte() << 8) | data1.ReadByte());
                        else
                            dest = (uint)((data2.ReadByte() << 24) | (data2.ReadByte() << 16) | (data2.ReadByte() << 8) | data2.ReadByte());
                        dest -= (uint)(position + 4);

                        output[0] = (byte)dest;
                        output[1] = (byte)(dest >> 8);
                        output[2] = (byte)(dest >> 16);
                        output[3] = (byte)(dest >> 24);
                        outputOffset = 0;
                        outputCount = 4;
                    }
                }
            }

            return size;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin != SeekOrigin.Current)
                throw new NotImplementedException();

            const int bufferSize = 10240;
            byte[] seekBuffer = new byte[bufferSize];
            long seekToGo = offset;
            while (seekToGo > 0)
            {
                long get = seekToGo > bufferSize ? bufferSize : seekToGo;
                Read(seekBuffer, 0, (int)get);
                seekToGo -= get;
            }
            return position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}