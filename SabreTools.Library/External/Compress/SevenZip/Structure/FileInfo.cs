using System;
using System.IO;
using System.Text;

namespace Compress.SevenZip.Structure
{
    public class FileInfo
    {
        public string[] Names;
        public bool[] EmptyStreamFlags;
        public bool[] EmptyFileFlags;
        public uint[] Attributes;

        public void Read(BinaryReader br)
        {
            ulong size = br.ReadEncodedUInt64();
            Names = new string[size];

            ulong numEmptyFiles = 0;

            for (; ; )
            {
                HeaderProperty hp = (HeaderProperty)br.ReadByte();
                if (hp == HeaderProperty.kEnd)
                {
                    return;
                }

                ulong bytessize = br.ReadEncodedUInt64();
                switch (hp)
                {
                    case HeaderProperty.kName:
                        if (br.ReadByte() != 0)
                        {
                            throw new Exception("Cannot be external");
                        }

                        for (ulong i = 0; i < size; i++)
                        {
                            Names[i] = br.ReadName();
                        }

                        continue;

                    case HeaderProperty.kEmptyStream:
                        EmptyStreamFlags = Util.ReadBoolFlags(br, (ulong)Names.Length);
                        for (ulong i = 0; i < size; i++)
                        {
                            if (EmptyStreamFlags[i])
                            {
                                numEmptyFiles++;
                            }
                        }
                        continue;

                    case HeaderProperty.kEmptyFile:
                        EmptyFileFlags = Util.ReadBoolFlags(br, numEmptyFiles);
                        continue;

                    case HeaderProperty.kWinAttributes:
                        Attributes = Util.ReadUInt32Def(br, size);
                        continue;

                    // don't know what this is.
                    case HeaderProperty.kAnti:
                        br.ReadBytes((int)bytessize);
                        continue;

                    case HeaderProperty.kCreationTime:
                    case HeaderProperty.kLastAccessTime:
                    case HeaderProperty.kLastWriteTime:
                        br.ReadBytes((int)bytessize);
                        continue;

                    case HeaderProperty.kDummy:
                        br.ReadBytes((int)bytessize);
                        continue;

                    default:
                        throw new Exception(hp.ToString());
                }
            }
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write((byte)HeaderProperty.kFilesInfo);
            bw.WriteEncodedUInt64((ulong)Names.Length);


            byte[] namebyte;
            using (MemoryStream nameMem = new MemoryStream())
            {
                using (BinaryWriter nameBw = new BinaryWriter(nameMem, Encoding.UTF8, true))
                {
                    nameBw.Write((byte)0); //not external
                    foreach (string name in Names)
                    {
                        nameBw.WriteName(name);
                    }

                    namebyte = new byte[nameMem.Length];
                    nameMem.Position = 0;
                    nameMem.Read(namebyte, 0, namebyte.Length);
                }
            }

            bw.Write((byte)HeaderProperty.kName);
            bw.WriteEncodedUInt64((ulong)namebyte.Length);
            bw.Write(namebyte);

            if (EmptyStreamFlags != null)
            {
                bw.Write((byte)HeaderProperty.kEmptyStream);
                Util.WriteBoolFlags(bw, EmptyStreamFlags);
            }

            if (EmptyFileFlags != null)
            {
                bw.Write((byte)HeaderProperty.kEmptyFile);
                Util.WriteBoolFlags(bw, EmptyFileFlags);
            }

            if (Attributes != null)
            {
                bw.Write((byte)HeaderProperty.kWinAttributes);
                Util.WriteUint32Def(bw, Attributes);
            }

            bw.Write((byte)HeaderProperty.kEnd);
        }

        public void Report(ref StringBuilder sb)
        {
            sb.AppendLine("  FileInfo");
            sb.AppendLine("  ------");
        }

    }
}