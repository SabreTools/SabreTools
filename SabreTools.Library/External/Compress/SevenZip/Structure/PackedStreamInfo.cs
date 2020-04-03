using System;
using System.IO;
using System.Text;
using Compress.Utils;

namespace Compress.SevenZip.Structure
{
    public class PackedStreamInfo
    {
        public ulong PackedSize;
        public ulong? Crc;
        public ulong StreamPosition;
        public Stream PackedStream;

        public static void Read(BinaryReader br, out ulong packPosition, out PackedStreamInfo[] packedStreams)
        {
            packPosition = br.ReadEncodedUInt64();

            ulong numPackStreams = br.ReadEncodedUInt64();

            packedStreams = new PackedStreamInfo[numPackStreams];
            for (ulong i = 0; i < numPackStreams; i++)
            {
                packedStreams[i] = new PackedStreamInfo();
            }

            ulong streamPosition = 0;

            for (; ; )
            {
                HeaderProperty hp = (HeaderProperty)br.ReadByte();
                switch (hp)
                {
                    case HeaderProperty.kSize:
                        for (ulong i = 0; i < numPackStreams; i++)
                        {
                            packedStreams[i].StreamPosition = streamPosition;
                            packedStreams[i].PackedSize = br.ReadEncodedUInt64();
                            streamPosition += packedStreams[i].PackedSize;
                        }

                        continue;

                    case HeaderProperty.kCRC:
                        for (ulong i = 0; i < numPackStreams; i++)
                        {
                            packedStreams[i].Crc = br.ReadEncodedUInt64();
                        }

                        continue;

                    case HeaderProperty.kEnd:
                        return;

                    default:
                        throw new Exception(hp.ToString());
                }
            }
        }

        public static void Write(BinaryWriter bw, ulong packPosition, PackedStreamInfo[] packedStreams)
        {
            ulong numPackStreams = (ulong)packedStreams.Length;
            bw.Write((byte)HeaderProperty.kPackInfo);
            bw.WriteEncodedUInt64(packPosition);
            bw.WriteEncodedUInt64(numPackStreams);

            bw.Write((byte)HeaderProperty.kSize);
            ulong streamPosition = 0;
            for (ulong i = 0; i < numPackStreams; i++)
            {
                packedStreams[i].StreamPosition = streamPosition;
                bw.WriteEncodedUInt64(packedStreams[i].PackedSize);
                streamPosition += packedStreams[i].PackedSize;
            }

            // Only checking the first CRC assuming all the reset will be the same
            if (packedStreams[0].Crc != null)
            {
                bw.Write((byte)HeaderProperty.kCRC);
                for (ulong i = 0; i < numPackStreams; i++)
                {
                    bw.WriteEncodedUInt64(packedStreams[i].Crc ?? 0);
                }
            }

            bw.Write((byte)HeaderProperty.kEnd);
        }


        public void Report(ref StringBuilder sb)
        {
            sb.AppendLine($"    PackedSize     = {PackedSize}");
            sb.AppendLine($"    Crc            = {Crc.ToHex()}");
            sb.AppendLine($"    StreamPosition = {StreamPosition}");
        }
    }
}