using System.Collections.Generic;
using System.Linq;
using SabreTools.Core;

namespace SabreTools.Serialization
{
    /// <summary>
    /// Serializer for Hashfile models to internal structure
    /// </summary>
    public partial class Internal
    {
        #region Serialize

        /// <summary>
        /// Convert from <cref="Models.Hashfile.Hashfile"/> to <cref="Models.Internal.Machine"/>
        /// </summary>
        public static Models.Internal.Machine ConvertMachineFromHashfile(Models.Hashfile.Hashfile item)
        {
            var machine = new Models.Internal.Machine();

            if (item.SFV != null && item.SFV.Any())
            {
                var roms = new List<Models.Internal.Rom>();
                foreach (var sfv in item.SFV)
                {
                    roms.Add(ConvertFromSFV(sfv));
                }
                machine[Models.Internal.Machine.RomKey] = roms.ToArray();
            }

            else if (item.MD5 != null && item.MD5.Any())
            {
                var roms = new List<Models.Internal.Rom>();
                foreach (var md5 in item.MD5)
                {
                    roms.Add(ConvertFromMD5(md5));
                }
                machine[Models.Internal.Machine.RomKey] = roms.ToArray();
            }

            else if (item.SHA1 != null && item.SHA1.Any())
            {
                var roms = new List<Models.Internal.Rom>();
                foreach (var sha1 in item.SHA1)
                {
                    roms.Add(ConvertFromSHA1(sha1));
                }
                machine[Models.Internal.Machine.RomKey] = roms.ToArray();
            }

            else if (item.SHA256 != null && item.SHA256.Any())
            {
                var roms = new List<Models.Internal.Rom>();
                foreach (var sha256 in item.SHA256)
                {
                    roms.Add(ConvertFromSHA256(sha256));
                }
                machine[Models.Internal.Machine.RomKey] = roms.ToArray();
            }

            else if (item.SHA384 != null && item.SHA384.Any())
            {
                var roms = new List<Models.Internal.Rom>();
                foreach (var sha384 in item.SHA384)
                {
                    roms.Add(ConvertFromSHA384(sha384));
                }
                machine[Models.Internal.Machine.RomKey] = roms.ToArray();
            }

            else if (item.SHA512 != null && item.SHA512.Any())
            {
                var roms = new List<Models.Internal.Rom>();
                foreach (var sha512 in item.SHA512)
                {
                    roms.Add(ConvertFromSHA512(sha512));
                }
                machine[Models.Internal.Machine.RomKey] = roms.ToArray();
            }

            else if (item.SpamSum != null && item.SpamSum.Any())
            {
                var roms = new List<Models.Internal.Rom>();
                foreach (var spamSum in item.SpamSum)
                {
                    roms.Add(ConvertFromSpamSum(spamSum));
                }
                machine[Models.Internal.Machine.RomKey] = roms.ToArray();
            }

            return machine;
        }

        /// <summary>
        /// Convert from <cref="Models.Hashfile.MD5"/> to <cref="Models.Internal.Rom"/>
        /// </summary>
        public static Models.Internal.Rom ConvertFromMD5(Models.Hashfile.MD5 item)
        {
            var rom = new Models.Internal.Rom
            {
                [Models.Internal.Rom.MD5Key] = item.Hash,
                [Models.Internal.Rom.NameKey] = item.File,
            };
            return rom;
        }

        /// <summary>
        /// Convert from <cref="Models.Hashfile.SFV"/> to <cref="Models.Internal.Rom"/>
        /// </summary>
        public static Models.Internal.Rom ConvertFromSFV(Models.Hashfile.SFV item)
        {
            var rom = new Models.Internal.Rom
            {
                [Models.Internal.Rom.NameKey] = item.File,
                [Models.Internal.Rom.CRCKey] = item.Hash,
            };
            return rom;
        }

        /// <summary>
        /// Convert from <cref="Models.Hashfile.SHA1"/> to <cref="Models.Internal.Rom"/>
        /// </summary>
        public static Models.Internal.Rom ConvertFromSHA1(Models.Hashfile.SHA1 item)
        {
            var rom = new Models.Internal.Rom
            {
                [Models.Internal.Rom.SHA1Key] = item.Hash,
                [Models.Internal.Rom.NameKey] = item.File,
            };
            return rom;
        }

        /// <summary>
        /// Convert from <cref="Models.Hashfile.SHA256"/> to <cref="Models.Internal.Rom"/>
        /// </summary>
        public static Models.Internal.Rom ConvertFromSHA256(Models.Hashfile.SHA256 item)
        {
            var rom = new Models.Internal.Rom
            {
                [Models.Internal.Rom.SHA256Key] = item.Hash,
                [Models.Internal.Rom.NameKey] = item.File,
            };
            return rom;
        }

        /// <summary>
        /// Convert from <cref="Models.Hashfile.SHA384"/> to <cref="Models.Internal.Rom"/>
        /// </summary>
        public static Models.Internal.Rom ConvertFromSHA384(Models.Hashfile.SHA384 item)
        {
            var rom = new Models.Internal.Rom
            {
                [Models.Internal.Rom.SHA384Key] = item.Hash,
                [Models.Internal.Rom.NameKey] = item.File,
            };
            return rom;
        }

        /// <summary>
        /// Convert from <cref="Models.Hashfile.SHA512"/> to <cref="Models.Internal.Rom"/>
        /// </summary>
        public static Models.Internal.Rom ConvertFromSHA512(Models.Hashfile.SHA512 item)
        {
            var rom = new Models.Internal.Rom
            {
                [Models.Internal.Rom.SHA512Key] = item.Hash,
                [Models.Internal.Rom.NameKey] = item.File,
            };
            return rom;
        }

        /// <summary>
        /// Convert from <cref="Models.Hashfile.SpamSum"/> to <cref="Models.Internal.Rom"/>
        /// </summary>
        public static Models.Internal.Rom ConvertFromSpamSum(Models.Hashfile.SpamSum item)
        {
            var rom = new Models.Internal.Rom
            {
                [Models.Internal.Rom.SpamSumKey] = item.Hash,
                [Models.Internal.Rom.NameKey] = item.File,
            };
            return rom;
        }

        #endregion

        #region Deserialize

        /// <summary>
        /// Convert from <cref="Models.Internal.Machine"/> to <cref="Models.Hashfile.Hashfile"/>
        /// </summary>
        public static Models.Hashfile.Hashfile ConvertMachineToHashfile(Models.Internal.Machine item, Hash hash)
        {
            var hashfile = new Models.Hashfile.Hashfile();

            if (item.ContainsKey(Models.Internal.Machine.RomKey) && item[Models.Internal.Machine.RomKey] is Models.Internal.Rom[] roms)
            {
                switch (hash)
                {
                    case Hash.CRC:
                        var sfvItems = new List<Models.Hashfile.SFV>();
                        foreach (var rom in roms)
                        {
                            sfvItems.Add(ConvertToSFV(rom));
                        }
                        hashfile.SFV = sfvItems.ToArray();
                        break;

                    case Hash.MD5:
                        var md5Items = new List<Models.Hashfile.MD5>();
                        foreach (var rom in roms)
                        {
                            md5Items.Add(ConvertToMD5(rom));
                        }
                        hashfile.MD5 = md5Items.ToArray();
                        break;

                    case Hash.SHA1:
                        var sha1Items = new List<Models.Hashfile.SHA1>();
                        foreach (var rom in roms)
                        {
                            sha1Items.Add(ConvertToSHA1(rom));
                        }
                        hashfile.SHA1 = sha1Items.ToArray();
                        break;

                    case Hash.SHA256:
                        var sha256Items = new List<Models.Hashfile.SHA256>();
                        foreach (var rom in roms)
                        {
                            sha256Items.Add(ConvertToSHA256(rom));
                        }
                        hashfile.SHA256 = sha256Items.ToArray();
                        break;

                    case Hash.SHA384:
                        var sha384Items = new List<Models.Hashfile.SHA384>();
                        foreach (var rom in roms)
                        {
                            sha384Items.Add(ConvertToSHA384(rom));
                        }
                        hashfile.SHA384 = sha384Items.ToArray();
                        break;

                    case Hash.SHA512:
                        var sha512Items = new List<Models.Hashfile.SHA512>();
                        foreach (var rom in roms)
                        {
                            sha512Items.Add(ConvertToSHA512(rom));
                        }
                        hashfile.SHA512 = sha512Items.ToArray();
                        break;

                    case Hash.SpamSum:
                        var spamSumItems = new List<Models.Hashfile.SpamSum>();
                        foreach (var rom in roms)
                        {
                            spamSumItems.Add(ConvertToSpamSum(rom));
                        }
                        hashfile.SpamSum = spamSumItems.ToArray();
                        break;
                }
            }

            return hashfile;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Rom"/> to <cref="Models.Hashfile.MD5"/>
        /// </summary>
        public static Models.Hashfile.MD5 ConvertToMD5(Models.Internal.Rom item)
        {
            var md5 = new Models.Hashfile.MD5
            {
                Hash = item.ReadString(Models.Internal.Rom.MD5Key),
                File = item.ReadString(Models.Internal.Rom.NameKey),
            };
            return md5;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Rom"/> to <cref="Models.Hashfile.SFV"/>
        /// </summary>
        public static Models.Hashfile.SFV ConvertToSFV(Models.Internal.Rom item)
        {
            var sfv = new Models.Hashfile.SFV
            {
                File = item.ReadString(Models.Internal.Rom.NameKey),
                Hash = item.ReadString(Models.Internal.Rom.CRCKey),
            };
            return sfv;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Rom"/> to <cref="Models.Hashfile.SHA1"/>
        /// </summary>
        public static Models.Hashfile.SHA1 ConvertToSHA1(Models.Internal.Rom item)
        {
            var sha1 = new Models.Hashfile.SHA1
            {
                Hash = item.ReadString(Models.Internal.Rom.SHA1Key),
                File = item.ReadString(Models.Internal.Rom.NameKey),
            };
            return sha1;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Rom"/> to <cref="Models.Hashfile.SHA256"/>
        /// </summary>
        public static Models.Hashfile.SHA256 ConvertToSHA256(Models.Internal.Rom item)
        {
            var sha256 = new Models.Hashfile.SHA256
            {
                Hash = item.ReadString(Models.Internal.Rom.SHA256Key),
                File = item.ReadString(Models.Internal.Rom.NameKey),
            };
            return sha256;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Rom"/> to <cref="Models.Hashfile.SHA384"/>
        /// </summary>
        public static Models.Hashfile.SHA384 ConvertToSHA384(Models.Internal.Rom item)
        {
            var sha384 = new Models.Hashfile.SHA384
            {
                Hash = item.ReadString(Models.Internal.Rom.SHA384Key),
                File = item.ReadString(Models.Internal.Rom.NameKey),
            };
            return sha384;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Rom"/> to <cref="Models.Hashfile.SHA512"/>
        /// </summary>
        public static Models.Hashfile.SHA512 ConvertToSHA512(Models.Internal.Rom item)
        {
            var sha512 = new Models.Hashfile.SHA512
            {
                Hash = item.ReadString(Models.Internal.Rom.SHA512Key),
                File = item.ReadString(Models.Internal.Rom.NameKey),
            };
            return sha512;
        }

        /// <summary>
        /// Convert from <cref="Models.Internal.Rom"/> to <cref="Models.Hashfile.SpamSum"/>
        /// </summary>
        public static Models.Hashfile.SpamSum ConvertToSpamSum(Models.Internal.Rom item)
        {
            var spamsum = new Models.Hashfile.SpamSum
            {
                Hash = item.ReadString(Models.Internal.Rom.SpamSumKey),
                File = item.ReadString(Models.Internal.Rom.NameKey),
            };
            return spamsum;
        }

        #endregion
    }
}