using SabreTools.Core.Tools;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.FileTypes;
using SabreTools.FileTypes.Aaru;
using SabreTools.FileTypes.CHD;
using SabreTools.IO.Extensions;

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
                disk.SetFieldValue<string?>(Models.Metadata.Disk.MD5Key, chd.InternalMD5.ToHexString());
                disk.SetFieldValue<string?>(Models.Metadata.Disk.SHA1Key, chd.InternalSHA1.ToHexString());
            }
            else
            {
                disk.SetFieldValue<string?>(Models.Metadata.Disk.MD5Key, baseFile.MD5.ToHexString());
                disk.SetFieldValue<string?>(Models.Metadata.Disk.SHA1Key, baseFile.SHA1.ToHexString());
            }

            disk.RemoveField(Models.Metadata.Disk.StatusKey);
            disk.SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);

            return disk;
        }

        /// <summary>
        /// Convert a BaseFile value to a File
        /// </summary>
        /// <param name="baseFile">BaseFile to convert</param>
        /// <returns>File containing original BaseFile information</returns>
        public static DatItems.Formats.File ConvertToFile(this BaseFile baseFile)
        {
            var file = new DatItems.Formats.File();

            file.CRC = baseFile.CRC.ToHexString();
            file.MD5 = baseFile.MD5.ToHexString();
            file.SHA1 = baseFile.SHA1.ToHexString();
            file.SHA256 = baseFile.SHA256.ToHexString();

            file.SetFieldValue<ItemType>(Models.Metadata.DatItem.TypeKey, ItemType.File);
            file.SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);

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
                media.SetFieldValue<string?>(Models.Metadata.Media.MD5Key, aif.InternalMD5.ToHexString());
                media.SetFieldValue<string?>(Models.Metadata.Media.SHA1Key, aif.InternalSHA1.ToHexString());
                media.SetFieldValue<string?>(Models.Metadata.Media.SHA256Key, aif.InternalSHA256.ToHexString());
                media.SetFieldValue<string?>(Models.Metadata.Media.SpamSumKey, System.Text.Encoding.UTF8.GetString(aif.InternalSpamSum ?? []));
            }
            else
            {
                media.SetFieldValue<string?>(Models.Metadata.Media.MD5Key, baseFile.MD5.ToHexString());
                media.SetFieldValue<string?>(Models.Metadata.Media.SHA1Key, baseFile.SHA1.ToHexString());
                media.SetFieldValue<string?>(Models.Metadata.Media.SHA256Key, baseFile.SHA256.ToHexString());
                media.SetFieldValue<string?>(Models.Metadata.Media.SpamSumKey, System.Text.Encoding.UTF8.GetString(baseFile.SpamSum ?? []));
            }

            media.SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);

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
            rom.SetFieldValue<string?>(Models.Metadata.Rom.DateKey, baseFile.Date);
            rom.SetFieldValue<string?>(Models.Metadata.Rom.CRCKey, baseFile.CRC.ToHexString());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.MD2Key, baseFile.MD2.ToHexString());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.MD4Key, baseFile.MD4.ToHexString());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.MD5Key, baseFile.MD5.ToHexString());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA1Key, baseFile.SHA1.ToHexString());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA256Key, baseFile.SHA256.ToHexString());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA384Key, baseFile.SHA384.ToHexString());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SHA512Key, baseFile.SHA512.ToHexString());
            rom.SetFieldValue<string?>(Models.Metadata.Rom.SizeKey, baseFile.Size.ToString());
            if (baseFile.SpamSum != null)
                rom.SetFieldValue<string?>(Models.Metadata.Rom.SpamSumKey, System.Text.Encoding.UTF8.GetString(baseFile.SpamSum));

            rom.RemoveField(Models.Metadata.Rom.StatusKey);
            rom.SetFieldValue<DupeType>(DatItem.DupeTypeKey, 0x00);

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
            var machine = disk.GetMachine();
            if (machine != null)
                machineName = machine.GetName();

            return new CHDFile()
            {
                Filename = disk.GetName(),
                Parent = machineName,
                MD5 = disk.GetStringFieldValue(Models.Metadata.Disk.MD5Key).FromHexString(),
                InternalMD5 = disk.GetStringFieldValue(Models.Metadata.Disk.MD5Key).FromHexString(),
                SHA1 = disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key).FromHexString(),
                InternalSHA1 = disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key).FromHexString(),
            };
        }

        /// <summary>
        /// Convert a File value to a BaseFile
        /// </summary>
        /// <param name="file">File to convert</param>
        /// <returns>BaseFile containing original File information</returns>
        public static BaseFile ConvertToBaseFile(this DatItems.Formats.File file)
        {
            string? machineName = null;
            var machine = file.GetMachine();
            if (machine != null)
                machineName = machine.GetName();

            return new BaseFile()
            {
                Parent = machineName,
                CRC = file.CRC.FromHexString(),
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
            var machine = media.GetMachine();
            if (machine != null)
                machineName = machine.GetName();

            return new AaruFormat()
            {
                Filename = media.GetName(),
                Parent = machineName,
                MD5 = media.GetStringFieldValue(Models.Metadata.Media.MD5Key).FromHexString(),
                InternalMD5 = media.GetStringFieldValue(Models.Metadata.Media.MD5Key).FromHexString(),
                SHA1 = media.GetStringFieldValue(Models.Metadata.Media.SHA1Key).FromHexString(),
                InternalSHA1 = media.GetStringFieldValue(Models.Metadata.Media.SHA1Key).FromHexString(),
                SHA256 = media.GetStringFieldValue(Models.Metadata.Media.SHA256Key).FromHexString(),
                InternalSHA256 = media.GetStringFieldValue(Models.Metadata.Media.SHA256Key).FromHexString(),
                SpamSum = System.Text.Encoding.UTF8.GetBytes(media.GetStringFieldValue(Models.Metadata.Media.SpamSumKey) ?? string.Empty),
                InternalSpamSum = System.Text.Encoding.UTF8.GetBytes(media.GetStringFieldValue(Models.Metadata.Media.SpamSumKey) ?? string.Empty),
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
            var machine = rom.GetMachine();
            if (machine != null)
                machineName = machine.GetName();

            string? spamSum = rom.GetStringFieldValue(Models.Metadata.Rom.SpamSumKey);
            return new BaseFile()
            {
                Filename = rom.GetName(),
                Parent = machineName,
                Date = rom.GetStringFieldValue(Models.Metadata.Rom.DateKey),
                Size = NumberHelper.ConvertToInt64(rom.GetStringFieldValue(Models.Metadata.Rom.SizeKey)),
                CRC = rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey).FromHexString(),
                MD2 = rom.GetStringFieldValue(Models.Metadata.Rom.MD2Key).FromHexString(),
                MD4 = rom.GetStringFieldValue(Models.Metadata.Rom.MD4Key).FromHexString(),
                MD5 = rom.GetStringFieldValue(Models.Metadata.Rom.MD5Key).FromHexString(),
                SHA1 = rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key).FromHexString(),
                SHA256 = rom.GetStringFieldValue(Models.Metadata.Rom.SHA256Key).FromHexString(),
                SHA384 = rom.GetStringFieldValue(Models.Metadata.Rom.SHA384Key).FromHexString(),
                SHA512 = rom.GetStringFieldValue(Models.Metadata.Rom.SHA512Key).FromHexString(),
                SpamSum = spamSum != null ? System.Text.Encoding.UTF8.GetBytes(spamSum) : null,
            };
        }

        #endregion
    }
}