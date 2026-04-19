using System.Text;
using SabreTools.FileTypes;
using SabreTools.FileTypes.Aaru;
using SabreTools.FileTypes.CHD;
using SabreTools.Metadata.DatItems;
using SabreTools.Metadata.DatItems.Formats;
using SabreTools.Text.Extensions;

namespace SabreTools.DatTools
{
    public static class DatItemTool
    {
        #region Creation

        /// <summary>
        /// Create a specific type of DatItem to be used based on a BaseFile
        /// </summary>
        /// <param name="baseFile">BaseFile containing information to be created</param>
        /// <param name="asFile">TreatAsFile representing special format scanning</param>
        /// <returns>DatItem of the specific internal type that corresponds to the inputs</returns>
        public static DatItem? CreateDatItem(BaseFile? baseFile, TreatAsFile asFile = 0x00)
        {
            return baseFile switch
            {
                // Disk
#if NET20 || NET35
                CHDFile when (asFile & TreatAsFile.CHD) == 0 => baseFile.ConvertToDisk(),
#else
                CHDFile when !asFile.HasFlag(TreatAsFile.CHD) => baseFile.ConvertToDisk(),
#endif

                // Media
#if NET20 || NET35
                AaruFormat when (asFile & TreatAsFile.AaruFormat) == 0 => baseFile.ConvertToMedia(),
#else
                AaruFormat when !asFile.HasFlag(TreatAsFile.AaruFormat) => baseFile.ConvertToMedia(),
#endif

                // Rom
                BaseArchive => baseFile.ConvertToRom(),
                Folder => null, // Folders cannot be a DatItem
                BaseFile => baseFile.ConvertToRom(),

                // Miscellaneous
                _ => null,
            };
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Convert a BaseFile value to a Disk
        /// </summary>
        /// <param name="baseFile">BaseFile to convert</param>
        /// <returns>Disk containing original BaseFile information</returns>
        public static Disk ConvertToDisk(this BaseFile baseFile)
        {
            var disk = new Disk();

            disk.SetName(baseFile.Filename);
            if (baseFile is CHDFile chd)
            {
                disk.MD5 = chd.InternalMD5.ToHexString();
                disk.SHA1 = chd.InternalSHA1.ToHexString();
            }
            else
            {
                disk.MD5 = baseFile.MD5.ToHexString();
                disk.SHA1 = baseFile.SHA1.ToHexString();
            }

            disk.Status = null;
            disk.DupeType = 0x00;

            return disk;
        }

        /// <summary>
        /// Convert a BaseFile value to a File
        /// </summary>
        /// <param name="baseFile">BaseFile to convert</param>
        /// <returns>File containing original BaseFile information</returns>
        public static File ConvertToFile(this BaseFile baseFile)
        {
            var file = new File
            {
                CRC = baseFile.CRC32.ToHexString(),
                MD5 = baseFile.MD5.ToHexString(),
                SHA1 = baseFile.SHA1.ToHexString(),
                SHA256 = baseFile.SHA256.ToHexString(),
                DupeType = 0x00,
            };

            return file;
        }

        /// <summary>
        /// Convert a BaseFile value to a Media
        /// </summary>
        /// <param name="baseFile">BaseFile to convert</param>
        /// <returns>Media containing original BaseFile information</returns>
        public static Media ConvertToMedia(this BaseFile baseFile)
        {
            var media = new Media();

            media.SetName(baseFile.Filename);
            if (baseFile is AaruFormat aif)
            {
                media.MD5 = aif.InternalMD5.ToHexString();
                media.SHA1 = aif.InternalSHA1.ToHexString();
                media.SHA256 = aif.InternalSHA256.ToHexString();
                media.SpamSum = Encoding.UTF8.GetString(aif.InternalSpamSum ?? []);
            }
            else
            {
                media.MD5 = baseFile.MD5.ToHexString();
                media.SHA1 = baseFile.SHA1.ToHexString();
                media.SHA256 = baseFile.SHA256.ToHexString();
                media.SpamSum = Encoding.UTF8.GetString(baseFile.SpamSum ?? []);
            }

            media.DupeType = 0x00;

            return media;
        }

        /// <summary>
        /// Convert a BaseFile value to a Rom
        /// </summary>
        /// <param name="baseFile">BaseFile to convert</param>
        /// <returns>Rom containing original BaseFile information</returns>
        public static Rom ConvertToRom(this BaseFile baseFile)
        {
            var rom = new Rom();

            rom.SetName(baseFile.Filename);
            rom.Date = baseFile.Date;
            rom.CRC32 = baseFile.CRC32.ToHexString();
            rom.MD2 = baseFile.MD2.ToHexString();
            rom.MD4 = baseFile.MD4.ToHexString();
            rom.MD5 = baseFile.MD5.ToHexString();
            rom.RIPEMD128 = baseFile.RIPEMD128.ToHexString();
            rom.RIPEMD160 = baseFile.RIPEMD160.ToHexString();
            rom.SHA1 = baseFile.SHA1.ToHexString();
            rom.SHA256 = baseFile.SHA256.ToHexString();
            rom.SHA384 = baseFile.SHA384.ToHexString();
            rom.SHA512 = baseFile.SHA512.ToHexString();
            rom.Size = baseFile.Size;
            if (baseFile.SpamSum is not null)
                rom.SpamSum = Encoding.UTF8.GetString(baseFile.SpamSum);

            rom.Status = null;
            rom.DupeType = 0x00;

            return rom;
        }

        /// <summary>
        /// Convert a Disk value to a BaseFile
        /// </summary>
        /// <param name="disk">Disk to convert</param>
        /// <returns>BaseFile containing original Disk information</returns>
        public static BaseFile ConvertToBaseFile(this Disk disk)
        {
            string? machineName = null;
            var machine = disk.Machine;
            if (machine is not null)
                machineName = machine.Name;

            return new CHDFile()
            {
                Filename = disk.GetName(),
                Parent = machineName,
                MD5 = disk.MD5.FromHexString(),
                InternalMD5 = disk.MD5.FromHexString(),
                SHA1 = disk.SHA1.FromHexString(),
                InternalSHA1 = disk.SHA1.FromHexString(),
            };
        }

        /// <summary>
        /// Convert a File value to a BaseFile
        /// </summary>
        /// <param name="file">File to convert</param>
        /// <returns>BaseFile containing original File information</returns>
        public static BaseFile ConvertToBaseFile(this File file)
        {
            string? machineName = null;
            var machine = file.Machine;
            if (machine is not null)
                machineName = machine.Name;

            return new BaseFile()
            {
                Parent = machineName,
                CRC32 = file.CRC.FromHexString(),
                MD5 = file.MD5.FromHexString(),
                SHA1 = file.SHA1.FromHexString(),
                SHA256 = file.SHA256.FromHexString(),
            };
        }

        /// <summary>
        /// Convert a Media value to a BaseFile
        /// </summary>
        /// <param name="media">Media to convert</param>
        /// <returns>BaseFile containing original Media information</returns>
        public static BaseFile ConvertToBaseFile(this Media media)
        {
            string? machineName = null;
            var machine = media.Machine;
            if (machine is not null)
                machineName = machine.Name;

            return new AaruFormat()
            {
                Filename = media.GetName(),
                Parent = machineName,
                MD5 = media.MD5.FromHexString(),
                InternalMD5 = media.MD5.FromHexString(),
                SHA1 = media.SHA1.FromHexString(),
                InternalSHA1 = media.SHA1.FromHexString(),
                SHA256 = media.SHA256.FromHexString(),
                InternalSHA256 = media.SHA256.FromHexString(),
                SpamSum = Encoding.UTF8.GetBytes(media.SpamSum ?? string.Empty),
                InternalSpamSum = Encoding.UTF8.GetBytes(media.SpamSum ?? string.Empty),
            };
        }

        /// <summary>
        /// Convert a Rom value to a BaseFile
        /// </summary>
        /// <param name="rom">Rom to convert</param>
        /// <returns>BaseFile containing original Rom information</returns>
        public static BaseFile ConvertToBaseFile(this Rom rom)
        {
            string? machineName = null;
            var machine = rom.Machine;
            if (machine is not null)
                machineName = machine.Name;

            string? spamSum = rom.SpamSum;
            return new BaseFile()
            {
                Filename = rom.GetName(),
                Parent = machineName,
                Date = rom.Date,
                Size = rom.Size,
                CRC32 = rom.CRC32.FromHexString(),
                MD2 = rom.MD2.FromHexString(),
                MD4 = rom.MD4.FromHexString(),
                MD5 = rom.MD5.FromHexString(),
                RIPEMD128 = rom.RIPEMD128.FromHexString(),
                RIPEMD160 = rom.RIPEMD160.FromHexString(),
                SHA1 = rom.SHA1.FromHexString(),
                SHA256 = rom.SHA256.FromHexString(),
                SHA384 = rom.SHA384.FromHexString(),
                SHA512 = rom.SHA512.FromHexString(),
                SpamSum = spamSum is not null ? Encoding.UTF8.GetBytes(spamSum) : null,
            };
        }

        #endregion
    }
}
