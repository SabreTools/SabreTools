using System;
using System.IO;
using System.Text;
using Compress.Utils;

namespace Compress.SevenZip.Structure
{
    public class Folder
    {
        public Coder[] Coders;
        public BindPair[] BindPairs;
        public ulong PackedStreamIndexBase;
        public ulong[] PackedStreamIndices;
        public ulong[] UnpackedStreamSizes;
        public uint? UnpackCRC;
        public UnpackedStreamInfo[] UnpackedStreamInfo;


        private void ReadFolder(BinaryReader br)
        {
            ulong numCoders = br.ReadEncodedUInt64();

            Coders = new Coder[numCoders];

            int numInStreams = 0;
            int numOutStreams = 0;

            for (ulong i = 0; i < numCoders; i++)
            {
                Coders[i] = new Coder();
                Coders[i].Read(br);

                numInStreams += (int)Coders[i].NumInStreams;
                numOutStreams += (int)Coders[i].NumOutStreams;
            }

            int numBindPairs = numOutStreams - 1;
            BindPairs = new BindPair[numBindPairs];
            for (int i = 0; i < numBindPairs; i++)
            {
                BindPairs[i] = new BindPair();
                BindPairs[i].Read(br);
            }

            if (numInStreams < numBindPairs)
            {
                throw new NotSupportedException("Error");
            }

            int numPackedStreams = numInStreams - numBindPairs;

            PackedStreamIndices = new ulong[numPackedStreams];

            if (numPackedStreams == 1)
            {
                uint pi = 0;
                for (uint j = 0; j < numInStreams; j++)
                {
                    for (uint k = 0; k < BindPairs.Length; k++)
                    {
                        if (BindPairs[k].InIndex == j)
                        {
                            continue;
                        }

                        PackedStreamIndices[pi++] = j;
                        break;
                    }
                }
            }
            else
            {
                for (uint i = 0; i < numPackedStreams; i++)
                {
                    PackedStreamIndices[i] = br.ReadEncodedUInt64();
                }
            }
        }

        private void ReadUnpackedStreamSize(BinaryReader br)
        {
            ulong outStreams = 0;
            foreach (Coder c in Coders)
            {
                outStreams += c.NumOutStreams;
            }

            UnpackedStreamSizes = new ulong[outStreams];
            for (uint j = 0; j < outStreams; j++)
            {
                UnpackedStreamSizes[j] = br.ReadEncodedUInt64();
            }
        }

        private ulong GetUnpackSize()
        {
            ulong outStreams = 0;
            foreach (Coder coder in Coders)
            {
                outStreams += coder.NumInStreams;
            }

            for (ulong j = 0; j < outStreams; j++)
            {
                bool found = false;
                foreach (BindPair bindPair in BindPairs)
                {
                    if (bindPair.OutIndex != j)
                    {
                        continue;
                    }

                    found = true;
                    break;
                }

                if (!found)
                {
                    return UnpackedStreamSizes[j];
                }
            }

            return 0;
        }


        public static void ReadUnPackInfo(BinaryReader br, out Folder[] Folders)
        {
            Folders = null;
            for (; ; )
            {
                HeaderProperty hp = (HeaderProperty)br.ReadByte();
                switch (hp)
                {
                    case HeaderProperty.kFolder:
                        {
                            ulong numFolders = br.ReadEncodedUInt64();

                            Folders = new Folder[numFolders];

                            byte external = br.ReadByte();
                            switch (external)
                            {
                                case 0:
                                    {
                                        ulong folderIndex = 0;
                                        for (uint i = 0; i < numFolders; i++)
                                        {
                                            Folders[i] = new Folder();
                                            Folders[i].ReadFolder(br);
                                            Folders[i].PackedStreamIndexBase = folderIndex;
                                            folderIndex += (ulong)Folders[i].PackedStreamIndices.Length;
                                        }

                                        break;
                                    }

                                case 1:
                                    throw new NotSupportedException("External flag");
                            }

                            continue;
                        }


                    case HeaderProperty.kCodersUnPackSize:
                        {
                            for (uint i = 0; i < Folders.Length; i++)
                            {
                                Folders[i].ReadUnpackedStreamSize(br);
                            }

                            continue;
                        }

                    case HeaderProperty.kCRC:
                        {
                            uint?[] crcs;
                            Util.UnPackCRCs(br, (ulong)Folders.Length, out crcs);
                            for (int i = 0; i < Folders.Length; i++)
                            {
                                Folders[i].UnpackCRC = crcs[i];
                            }

                            continue;
                        }

                    case HeaderProperty.kEnd:
                        return;

                    default:
                        throw new Exception(hp.ToString());
                }
            }
        }

        public static void ReadSubStreamsInfo(BinaryReader br, ref Folder[] Folders)
        {
            for (; ; )
            {
                HeaderProperty hp = (HeaderProperty)br.ReadByte();
                switch (hp)
                {
                    case HeaderProperty.kNumUnPackStream:
                        {
                            for (int f = 0; f < Folders.Length; f++)
                            {
                                int numStreams = (int)br.ReadEncodedUInt64();
                                Folders[f].UnpackedStreamInfo = new UnpackedStreamInfo[numStreams];
                                for (int i = 0; i < numStreams; i++)
                                {
                                    Folders[f].UnpackedStreamInfo[i] = new UnpackedStreamInfo();
                                }
                            }

                            continue;
                        }

                    case HeaderProperty.kSize:
                        {
                            for (int f = 0; f < Folders.Length; f++)
                            {
                                Folder folder = Folders[f];

                                if (folder.UnpackedStreamInfo.Length == 0)
                                {
                                    continue;
                                }

                                ulong sum = 0;
                                for (int i = 0; i < folder.UnpackedStreamInfo.Length - 1; i++)
                                {
                                    ulong size = br.ReadEncodedUInt64();
                                    folder.UnpackedStreamInfo[i].UnpackedSize = size;
                                    sum += size;
                                }

                                folder.UnpackedStreamInfo[folder.UnpackedStreamInfo.Length - 1].UnpackedSize =
                                    folder.GetUnpackSize() - sum;
                            }

                            continue;
                        }

                    case HeaderProperty.kCRC:
                        {
                            ulong numCRC = 0;
                            foreach (Folder folder in Folders)
                            {
                                if (folder.UnpackedStreamInfo == null)
                                {
                                    folder.UnpackedStreamInfo = new UnpackedStreamInfo[1];
                                    folder.UnpackedStreamInfo[0] = new UnpackedStreamInfo();
                                    folder.UnpackedStreamInfo[0].UnpackedSize = folder.GetUnpackSize();
                                }

                                if ((folder.UnpackedStreamInfo.Length != 1) || !folder.UnpackCRC.HasValue)
                                {
                                    numCRC += (ulong)folder.UnpackedStreamInfo.Length;
                                }
                            }

                            int crcIndex = 0;
                            uint?[] crc;
                            Util.UnPackCRCs(br, numCRC, out crc);
                            for (uint i = 0; i < Folders.Length; i++)
                            {
                                Folder folder = Folders[i];
                                if ((folder.UnpackedStreamInfo.Length == 1) && folder.UnpackCRC.HasValue)
                                {
                                    folder.UnpackedStreamInfo[0].Crc = folder.UnpackCRC;
                                }
                                else
                                {
                                    for (uint j = 0; j < folder.UnpackedStreamInfo.Length; j++, crcIndex++)
                                    {
                                        folder.UnpackedStreamInfo[j].Crc = crc[crcIndex];
                                    }
                                }
                            }

                            continue;
                        }

                    case HeaderProperty.kEnd:
                        return;

                    default:
                        throw new Exception(hp.ToString());
                }
            }
        }

        private void WriteFolder(BinaryWriter bw)
        {
            ulong numCoders = (ulong)Coders.Length;
            bw.WriteEncodedUInt64(numCoders);
            for (ulong i = 0; i < numCoders; i++)
            {
                Coders[i].Write(bw);
            }

            ulong numBindingPairs = BindPairs == null ? 0 : (ulong)BindPairs.Length;
            for (ulong i = 0; i < numBindingPairs; i++)
            {
                BindPairs[i].Write(bw);
            }

            //need to look at PAckedStreamIndices but don't need them for basic writing I am doing
        }

        private void WriteUnpackedStreamSize(BinaryWriter bw)
        {
            ulong numUnpackedStreamSizes = (ulong)UnpackedStreamSizes.Length;
            for (ulong i = 0; i < numUnpackedStreamSizes; i++)
            {
                bw.WriteEncodedUInt64(UnpackedStreamSizes[i]);
            }
        }

        public static void WriteUnPackInfo(BinaryWriter bw, Folder[] Folders)
        {
            bw.Write((byte)HeaderProperty.kUnPackInfo);

            bw.Write((byte)HeaderProperty.kFolder);
            ulong numFolders = (ulong)Folders.Length;
            bw.WriteEncodedUInt64(numFolders);
            bw.Write((byte)0); //External Flag
            for (ulong i = 0; i < numFolders; i++)
            {
                Folders[i].WriteFolder(bw);
            }


            bw.Write((byte)HeaderProperty.kCodersUnPackSize);
            for (ulong i = 0; i < numFolders; i++)
            {
                Folders[i].WriteUnpackedStreamSize(bw);
            }

            bool hasCRC = false;
            uint?[] CRCs = new uint?[numFolders];
            for (ulong i = 0; i < numFolders; i++)
            {
                CRCs[i] = Folders[i].UnpackCRC;
                hasCRC |= (CRCs[i] != null);
            }

            if (hasCRC)
            {
                bw.Write((byte)HeaderProperty.kCRC);
                Util.WritePackedCRCs(bw, CRCs);
            }

            bw.Write((byte)HeaderProperty.kEnd);
        }

        public static void WriteSubStreamsInfo(BinaryWriter bw, Folder[] Folders)
        {
            bw.Write((byte)HeaderProperty.kSubStreamsInfo);

            bw.Write((byte)HeaderProperty.kNumUnPackStream);
            for (int f = 0; f < Folders.Length; f++)
            {
                ulong numStreams = (ulong)Folders[f].UnpackedStreamInfo.Length;
                bw.WriteEncodedUInt64(numStreams);
            }

            bw.Write((byte)HeaderProperty.kSize);

            for (int f = 0; f < Folders.Length; f++)
            {
                Folder folder = Folders[f];
                for (int i = 0; i < folder.UnpackedStreamInfo.Length - 1; i++)
                {
                    bw.WriteEncodedUInt64(folder.UnpackedStreamInfo[i].UnpackedSize);
                }
            }

            bw.Write((byte)HeaderProperty.kCRC);
            bw.Write((byte)1); // crc flags default to true
            for (int f = 0; f < Folders.Length; f++)
            {
                Folder folder = Folders[f];
                for (int i = 0; i < folder.UnpackedStreamInfo.Length; i++)
                {
                    bw.Write(Util.uinttobytes(folder.UnpackedStreamInfo[i].Crc));
                }
            }

            bw.Write((byte)HeaderProperty.kEnd);
        }


        public void Report(ref StringBuilder sb)
        {
            if (Coders == null)
            {
                sb.AppendLine("    Coders[] = null");
            }
            else
            {
                sb.AppendLine($"    Coders[] = ({Coders.Length})");
                foreach (Coder c in Coders)
                {
                    c.Report(ref sb);
                }
            }
            if (BindPairs == null)
            {
                sb.AppendLine("    BindPairs[] = null");
            }
            else
            {
                sb.AppendLine($"    BindPairs[] = ({BindPairs.Length})");
                foreach (BindPair bp in BindPairs)
                {
                    bp.Report(ref sb);
                }
            }

            sb.AppendLine($"    PackedStreamIndexBase = {PackedStreamIndexBase}");
            sb.AppendLine($"    PackedStreamIndices[] = {PackedStreamIndices.ToArrayString()}");
            sb.AppendLine($"    UnpackedStreamSizes[] = {UnpackedStreamSizes.ToArrayString()}");
            sb.AppendLine($"    UnpackCRC             = {UnpackCRC.ToHex()}");

            if (UnpackedStreamInfo == null)
            {
                sb.AppendLine("    UnpackedStreamInfo[] = null");
            }
            else
            {
                sb.AppendLine($"    UnpackedStreamInfo[{UnpackedStreamInfo.Length}]");
                foreach (UnpackedStreamInfo usi in UnpackedStreamInfo)
                {
                    usi.Report(ref sb);
                }
            }

        }
    }
}