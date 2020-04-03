using System.Text;
using Compress.Utils;

namespace Compress.SevenZip.Structure
{
    public class UnpackedStreamInfo
    {
        public ulong UnpackedSize;
        public uint? Crc;

        public void Report(ref StringBuilder sb)
        {
            sb.AppendLine($"      Crc = {Crc.ToHex()} , Size = {UnpackedSize}");
        }
    }
}