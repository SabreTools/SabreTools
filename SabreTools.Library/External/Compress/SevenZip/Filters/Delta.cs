using System;
using System.IO;

namespace Compress.SevenZip.Filters
{
    public class Delta : Stream
    {
        private readonly Stream _baseStream;
        private long _position;
        private readonly byte[] _bVal;
        private readonly int _dSize;
        private int _bIndex;

        // properties values are 0,1,3
        public Delta(byte[] properties, Stream inputStream)
        {
            _dSize = properties[0] + 1;
            _bVal = new byte[_dSize];

            _baseStream = inputStream;
        }

        public override void Flush()
        {
            throw new NotImplementedException();
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
            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = _baseStream.Read(buffer, offset, count);

            for (int i = 0; i < read; i++)
            {
                buffer[i] = _bVal[_bIndex] = (byte)(buffer[i] + _bVal[_bIndex]);
                _bIndex = (_bIndex + 1) % _dSize;
            }

            _position += read;

            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
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
        public override long Length
        {
            get { return _baseStream.Length; }
        }
        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}