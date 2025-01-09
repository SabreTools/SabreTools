using System;
using System.Linq;
using SabreTools.Hashing;
using SabreTools.Models.Metadata;

namespace SabreTools.Core
{
    public static class DictionaryBaseExtensions
    {
        #region Cloning

        /// <summary>
        /// Deep clone a DictionaryBase object
        /// </summary>
        public static DictionaryBase? Clone(this DictionaryBase dictionaryBase)
        {
            // If construction failed, we can't do anything
            if (Activator.CreateInstance(dictionaryBase.GetType()) is not DictionaryBase clone)
                return null;

            // Loop through and clone per type
            foreach (string key in dictionaryBase.Keys)
            {
                object? value = dictionaryBase[key];
                clone[key] = value switch
                {
                    // Primative types
                    bool or long or double or string => value,

                    // DictionaryBase types
                    DictionaryBase db => db.Clone(),

                    // Enumerable types
                    byte[] bytArr => bytArr.Clone(),
                    string[] strArr => strArr.Clone(),
                    DictionaryBase[] dbArr => Array.ConvertAll(dbArr, Clone),
                    ICloneable[] clArr => Array.ConvertAll(clArr, cl => cl.Clone()),

                    // Everything else just copies
                    _ => value,
                };
            }

            return clone;
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Convert a DictionaryBase to a Rom
        /// </summary>
        public static Rom? ConvertToRom(this DictionaryBase? self)
        {
            // If the DatItem is missing, we can't do anything
            if (self == null)
                return null;

            return self switch
            {
                Disk diskSelf => ConvertToRom(diskSelf),
                Media mediaSelf => ConvertToRom(mediaSelf),
                _ => null,
            };
        }

        /// <summary>
        /// Convert a Disk to a Rom
        /// </summary>
        private static Rom? ConvertToRom(this Disk? disk)
        {
            // If the Disk is missing, we can't do anything
            if (disk == null)
                return null;

            // Append a suffix to the name
            string? name = disk.ReadString(Disk.NameKey);
            if (name != null)
                name += ".chd";

            return new Rom
            {
                [Rom.NameKey] = name,
                [Rom.MergeKey] = disk.ReadString(Disk.MergeKey),
                [Rom.RegionKey] = disk.ReadString(Disk.RegionKey),
                [Rom.StatusKey] = disk.ReadString(Disk.StatusKey),
                [Rom.OptionalKey] = disk.ReadString(Disk.OptionalKey),
                [Rom.MD5Key] = disk.ReadString(Disk.MD5Key),
                [Rom.SHA1Key] = disk.ReadString(Disk.SHA1Key),
            };
        }

        /// <summary>
        /// Convert a Media to a Rom
        /// </summary>
        private static Rom? ConvertToRom(this Media? media)
        {
            // If the Media is missing, we can't do anything
            if (media == null)
                return null;

            // Append a suffix to the name
            string? name = media.ReadString(Media.NameKey);
            if (name != null)
                name += ".aaruf";

            return new Rom
            {
                [Rom.NameKey] = name,
                [Rom.MD5Key] = media.ReadString(Media.MD5Key),
                [Rom.SHA1Key] = media.ReadString(Media.SHA1Key),
                [Rom.SHA256Key] = media.ReadString(Media.SHA256Key),
                [Rom.SpamSumKey] = media.ReadString(Media.SpamSumKey),
            };
        }

        #endregion

        #region Equality Checking

        /// <summary>
        /// Check equality of two DictionaryBase objects
        /// </summary>
        public static bool EqualTo(this DictionaryBase self, DictionaryBase other)
        {
            // Check types first
            if (self.GetType() != other.GetType())
                return false;

            // Check based on the item type
            return (self, other) switch
            {
                (Disk diskSelf, Disk diskOther) => EqualsImpl(diskSelf, diskOther),
                (Media mediaSelf, Media mediaOther) => EqualsImpl(mediaSelf, mediaOther),
                (Rom romSelf, Rom romOther) => EqualsImpl(romSelf, romOther),
                _ => EqualsImpl(self, other),
            };
        }

        /// <summary>
        /// Check equality of two DictionaryBase objects
        /// </summary>
        private static bool EqualsImpl(this DictionaryBase self, DictionaryBase other)
        {
            // If the number of key-value pairs doesn't match, they can't match
            if (self.Count != other.Count)
                return false;

            // If any keys are missing on either side, they can't match
            if (self.Keys.Except(other.Keys).Any())
                return false;
            if (other.Keys.Except(self.Keys).Any())
                return false;

            // Check all pairs to see if they're equal
            foreach (var kvp in self)
            {
                switch (kvp.Value, other[kvp.Key])
                {
                    case (string selfString, string otherString):
                        if (!string.Equals(selfString, otherString, StringComparison.OrdinalIgnoreCase))
                            return false;
                        break;

                    case (ModelBackedItem selfMbi, ModelBackedItem otherMbi):
                        if (!selfMbi.Equals(otherMbi))
                            return false;
                        break;

                    case (DictionaryBase selfDb, DictionaryBase otherDb):
                        if (!selfDb.Equals(otherDb))
                            return false;
                        break;

                    // TODO: Make this case-insensitive
                    case (string[] selfStrArr, string[] otherStrArr):
                        if (selfStrArr.Length != otherStrArr.Length)
                            return false;
                        if (selfStrArr.Except(otherStrArr).Any())
                            return false;
                        if (otherStrArr.Except(selfStrArr).Any())
                            return false;
                        break;

                    // TODO: Fix the logic here for real equality
                    case (DictionaryBase[] selfDbArr, DictionaryBase[] otherDbArr):
                        if (selfDbArr.Length != otherDbArr.Length)
                            return false;
                        if (selfDbArr.Except(otherDbArr).Any())
                            return false;
                        if (otherDbArr.Except(selfDbArr).Any())
                            return false;
                        break;

                    default:
                        // Handle cases where a null is involved
                        if (kvp.Value == null && other[kvp.Key] == null)
                            return true;
                        else if (kvp.Value == null && other[kvp.Key] != null)
                            return false;
                        else if (kvp.Value != null && other[kvp.Key] == null)
                            return false;

                        // Try to rely on type hashes
                        else if (kvp.Value!.GetHashCode() != other[kvp.Key]!.GetHashCode())
                            return false;

                        break;
                }
            }

            return true;
        }

        /// <summary>
        /// Check equality of two Disk objects
        /// </summary>
        private static bool EqualsImpl(this Disk self, Disk other)
        {
            string? selfStatus = self.ReadString(Disk.StatusKey);
            string? otherStatus = other.ReadString(Disk.StatusKey);

            string? selfName = self.ReadString(Disk.NameKey);
            string? otherName = other.ReadString(Disk.NameKey);

            // If all hashes are empty but they're both nodump and the names match, then they're dupes
            if (string.Equals(selfStatus, "nodump", StringComparison.OrdinalIgnoreCase)
                && string.Equals(otherStatus, "nodump", StringComparison.OrdinalIgnoreCase)
                && string.Equals(selfName, otherName, StringComparison.OrdinalIgnoreCase)
                && !self.HasHashes()
                && !other.HasHashes())
            {
                return true;
            }

            // If we get a partial match
            if (self.HashMatch(other))
                return true;

            // All other cases fail
            return false;
        }

        /// <summary>
        /// Check equality of two Media objects
        /// </summary>
        private static bool EqualsImpl(this Media self, Media other)
        {
            // If we get a partial match
            if (self.HashMatch(other))
                return true;

            // All other cases fail
            return false;
        }

        /// <summary>
        /// Check equality of two Rom objects
        /// </summary>
        private static bool EqualsImpl(this Rom self, Rom other)
        {
            string? selfStatus = self.ReadString(Rom.StatusKey);
            string? otherStatus = other.ReadString(Rom.StatusKey);

            string? selfName = self.ReadString(Rom.NameKey);
            string? otherName = other.ReadString(Rom.NameKey);

            long? selfSize = self.ReadLong(Rom.SizeKey);
            long? otherSize = other.ReadLong(Rom.SizeKey);

            // If all hashes are empty but they're both nodump and the names match, then they're dupes
            if (string.Equals(selfStatus, "nodump", StringComparison.OrdinalIgnoreCase)
                && string.Equals(otherStatus, "nodump", StringComparison.OrdinalIgnoreCase)
                && string.Equals(selfName, otherName, StringComparison.OrdinalIgnoreCase)
                && !self.HasHashes()
                && !other.HasHashes())
            {
                return true;
            }

            // If we have a file that has no known size, rely on the hashes only
            if (selfSize == null && self.HashMatch(other))
                return true;
            else if (otherSize == null && self.HashMatch(other))
                return true;

            // If we get a partial match
            if (selfSize == otherSize && self.HashMatch(other))
                return true;

            // All other cases fail
            return false;
        }

        #endregion

        #region Hash Checking

        /// <summary>
        /// Returns if any hashes are common
        /// </summary>
        public static bool HashMatch(this Disk self, Disk other)
        {
            // If either have no hashes, we return false, otherwise this would be a false positive
            if (!self.HasHashes() || !other.HasHashes())
                return false;

            // If neither have hashes in common, we return false, otherwise this would be a false positive
            if (!self.HasCommonHash(other))
                return false;

            // Return if all hashes match according to merge rules
            string? selfMd5 = self.ReadString(Disk.MD5Key);
            string? otherMd5 = other.ReadString(Disk.MD5Key);
            bool conditionalMd5 = Tools.Utilities.ConditionalHashEquals(selfMd5, otherMd5);

            string? selfSha1 = self.ReadString(Disk.SHA1Key);
            string? otherSha1 = other.ReadString(Disk.SHA1Key);
            bool conditionalSha1 = Tools.Utilities.ConditionalHashEquals(selfSha1, otherSha1);

            return conditionalMd5
                && conditionalSha1;
        }

        /// <summary>
        /// Returns if any hashes are common
        /// </summary>
        public static bool HashMatch(this Media self, Media other)
        {
            // If either have no hashes, we return false, otherwise this would be a false positive
            if (!self.HasHashes() || !other.HasHashes())
                return false;

            // If neither have hashes in common, we return false, otherwise this would be a false positive
            if (!self.HasCommonHash(other))
                return false;

            // Return if all hashes match according to merge rules
            string? selfMd5 = self.ReadString(Media.MD5Key);
            string? otherMd5 = other.ReadString(Media.MD5Key);
            bool conditionalMd5 = Tools.Utilities.ConditionalHashEquals(selfMd5, otherMd5);

            string? selfSha1 = self.ReadString(Media.SHA1Key);
            string? otherSha1 = other.ReadString(Media.SHA1Key);
            bool conditionalSha1 = Tools.Utilities.ConditionalHashEquals(selfSha1, otherSha1);

            string? selfSha256 = self.ReadString(Media.SHA256Key);
            string? otherSha256 = other.ReadString(Media.SHA256Key);
            bool conditionalSha256 = Tools.Utilities.ConditionalHashEquals(selfSha256, otherSha256);

            string? selfSpamSum = self.ReadString(Media.SpamSumKey);
            string? otherSpamSum = other.ReadString(Media.SpamSumKey);
            bool conditionalSpamSum = Tools.Utilities.ConditionalHashEquals(selfSpamSum, otherSpamSum);

            return conditionalMd5
                && conditionalSha1
                && conditionalSha256
                && conditionalSpamSum;
        }

        /// <summary>
        /// Returns if any hashes are common
        /// </summary>
        public static bool HashMatch(this Rom self, Rom other)
        {
            // If either have no hashes, we return false, otherwise this would be a false positive
            if (!self.HasHashes() || !other.HasHashes())
                return false;

            // If neither have hashes in common, we return false, otherwise this would be a false positive
            if (!self.HasCommonHash(other))
                return false;

            // Return if all hashes match according to merge rules
            string? selfCrc = self.ReadString(Rom.CRCKey);
            string? otherCrc = other.ReadString(Rom.CRCKey);
            bool conditionalCrc = Tools.Utilities.ConditionalHashEquals(selfCrc, otherCrc);

            string? selfMd2 = self.ReadString(Rom.MD2Key);
            string? otherMd2 = other.ReadString(Rom.MD2Key);
            bool conditionalMd2 = Tools.Utilities.ConditionalHashEquals(selfMd2, otherMd2);

            string? selfMd4 = self.ReadString(Rom.MD4Key);
            string? otherMd4 = other.ReadString(Rom.MD4Key);
            bool conditionalMd4 = Tools.Utilities.ConditionalHashEquals(selfMd4, otherMd4);

            string? selfMd5 = self.ReadString(Rom.MD5Key);
            string? otherMd5 = other.ReadString(Rom.MD5Key);
            bool conditionalMd5 = Tools.Utilities.ConditionalHashEquals(selfMd5, otherMd5);

            string? selfSha1 = self.ReadString(Rom.SHA1Key);
            string? otherSha1 = other.ReadString(Rom.SHA1Key);
            bool conditionalSha1 = Tools.Utilities.ConditionalHashEquals(selfSha1, otherSha1);

            string? selfSha256 = self.ReadString(Rom.SHA256Key);
            string? otherSha256 = other.ReadString(Rom.SHA256Key);
            bool conditionalSha256 = Tools.Utilities.ConditionalHashEquals(selfSha256, otherSha256);

            string? selfSha384 = self.ReadString(Rom.SHA384Key);
            string? otherSha384 = other.ReadString(Rom.SHA384Key);
            bool conditionalSha384 = Tools.Utilities.ConditionalHashEquals(selfSha384, otherSha384);

            string? selfSha512 = self.ReadString(Rom.SHA512Key);
            string? otherSha512 = other.ReadString(Rom.SHA512Key);
            bool conditionalSha512 = Tools.Utilities.ConditionalHashEquals(selfSha512, otherSha512);

            string? selfSpamSum = self.ReadString(Rom.SpamSumKey);
            string? otherSpamSum = other.ReadString(Rom.SpamSumKey);
            bool conditionalSpamSum = Tools.Utilities.ConditionalHashEquals(selfSpamSum, otherSpamSum);

            return conditionalCrc
                && conditionalMd2
                && conditionalMd4
                && conditionalMd5
                && conditionalSha1
                && conditionalSha256
                && conditionalSha384
                && conditionalSha512
                && conditionalSpamSum;
        }

        /// <summary>
        /// Returns if any hashes exist
        /// </summary>
        public static bool HasHashes(this DictionaryBase self)
        {
            return self switch
            {
                Disk diskSelf => diskSelf.HasHashes(),
                Media mediaSelf => mediaSelf.HasHashes(),
                Rom romSelf => romSelf.HasHashes(),
                _ => false,
            };
        }

        /// <summary>
        /// Returns if all of the hashes are set to their 0-byte values or null
        /// </summary>
        public static bool HasZeroHash(this DictionaryBase self)
        {
            return self switch
            {
                Disk diskSelf => diskSelf.HasZeroHash(),
                Media mediaSelf => mediaSelf.HasZeroHash(),
                Rom romSelf => romSelf.HasZeroHash(),
                _ => false,
            };
        }

        /// <summary>
        /// Returns if there are no, non-empty hashes in common
        /// </summary>
        private static bool HasCommonHash(this Disk self, Disk other)
        {
            bool md5Null = string.IsNullOrEmpty(self.ReadString(Disk.MD5Key));
            md5Null ^= string.IsNullOrEmpty(other.ReadString(Disk.MD5Key));

            bool sha1Null = string.IsNullOrEmpty(self.ReadString(Disk.SHA1Key));
            sha1Null ^= string.IsNullOrEmpty(other.ReadString(Disk.SHA1Key));

            return !md5Null
                || !sha1Null;
        }

        /// <summary>
        /// Returns if there are no, non-empty hashes in common
        /// </summary>
        private static bool HasCommonHash(this Media self, Media other)
        {
            bool md5Null = string.IsNullOrEmpty(self.ReadString(Media.MD5Key));
            md5Null ^= string.IsNullOrEmpty(other.ReadString(Media.MD5Key));

            bool sha1Null = string.IsNullOrEmpty(self.ReadString(Media.SHA1Key));
            sha1Null ^= string.IsNullOrEmpty(other.ReadString(Media.SHA1Key));

            bool sha256Null = string.IsNullOrEmpty(self.ReadString(Media.SHA256Key));
            sha256Null ^= string.IsNullOrEmpty(other.ReadString(Media.SHA256Key));

            bool spamsumNull = string.IsNullOrEmpty(self.ReadString(Media.SpamSumKey));
            spamsumNull ^= string.IsNullOrEmpty(other.ReadString(Media.SpamSumKey));

            return !md5Null
                || !sha1Null
                || !sha256Null
                || !spamsumNull;
        }

        /// <summary>
        /// Returns if there are no, non-empty hashes in common
        /// </summary>
        private static bool HasCommonHash(this Rom self, Rom other)
        {
            bool crcNull = string.IsNullOrEmpty(self.ReadString(Rom.CRCKey));
            crcNull ^= string.IsNullOrEmpty(other.ReadString(Rom.CRCKey));

            bool md2Null = string.IsNullOrEmpty(self.ReadString(Rom.MD2Key));
            md2Null ^= string.IsNullOrEmpty(other.ReadString(Rom.MD2Key));

            bool md4Null = string.IsNullOrEmpty(self.ReadString(Rom.MD4Key));
            md4Null ^= string.IsNullOrEmpty(other.ReadString(Rom.MD4Key));

            bool md5Null = string.IsNullOrEmpty(self.ReadString(Rom.MD5Key));
            md5Null ^= string.IsNullOrEmpty(other.ReadString(Rom.MD5Key));

            bool sha1Null = string.IsNullOrEmpty(self.ReadString(Rom.SHA1Key));
            sha1Null ^= string.IsNullOrEmpty(other.ReadString(Rom.SHA1Key));

            bool sha256Null = string.IsNullOrEmpty(self.ReadString(Rom.SHA256Key));
            sha256Null ^= string.IsNullOrEmpty(other.ReadString(Rom.SHA256Key));

            bool sha384Null = string.IsNullOrEmpty(self.ReadString(Rom.SHA384Key));
            sha384Null ^= string.IsNullOrEmpty(other.ReadString(Rom.SHA384Key));

            bool sha512Null = string.IsNullOrEmpty(self.ReadString(Rom.SHA512Key));
            sha512Null ^= string.IsNullOrEmpty(other.ReadString(Rom.SHA512Key));

            bool spamsumNull = string.IsNullOrEmpty(self.ReadString(Rom.SpamSumKey));
            spamsumNull ^= string.IsNullOrEmpty(other.ReadString(Rom.SpamSumKey));

            return !crcNull
                || !md2Null
                || !md4Null
                || !md5Null
                || !sha1Null
                || !sha256Null
                || !sha384Null
                || !sha512Null
                || !spamsumNull;
        }

        /// <summary>
        /// Returns if any hashes exist
        /// </summary>
        private static bool HasHashes(this Disk disk)
        {
            bool md5Null = string.IsNullOrEmpty(disk.ReadString(Disk.MD5Key));
            bool sha1Null = string.IsNullOrEmpty(disk.ReadString(Disk.SHA1Key));

            return !md5Null
                || !sha1Null;
        }

        /// <summary>
        /// Returns if any hashes exist
        /// </summary>
        private static bool HasHashes(this Media media)
        {
            bool md5Null = string.IsNullOrEmpty(media.ReadString(Media.MD5Key));
            bool sha1Null = string.IsNullOrEmpty(media.ReadString(Media.SHA1Key));
            bool sha256Null = string.IsNullOrEmpty(media.ReadString(Media.SHA256Key));
            bool spamsumNull = string.IsNullOrEmpty(media.ReadString(Media.SpamSumKey));

            return !md5Null
                || !sha1Null
                || !sha256Null
                || !spamsumNull;
        }

        /// <summary>
        /// Returns if any hashes exist
        /// </summary>
        private static bool HasHashes(this Rom rom)
        {
            bool crcNull = string.IsNullOrEmpty(rom.ReadString(Rom.CRCKey));
            bool md2Null = string.IsNullOrEmpty(rom.ReadString(Rom.MD2Key));
            bool md4Null = string.IsNullOrEmpty(rom.ReadString(Rom.MD4Key));
            bool md5Null = string.IsNullOrEmpty(rom.ReadString(Rom.MD5Key));
            bool sha1Null = string.IsNullOrEmpty(rom.ReadString(Rom.SHA1Key));
            bool sha256Null = string.IsNullOrEmpty(rom.ReadString(Rom.SHA256Key));
            bool sha384Null = string.IsNullOrEmpty(rom.ReadString(Rom.SHA384Key));
            bool sha512Null = string.IsNullOrEmpty(rom.ReadString(Rom.SHA512Key));
            bool spamsumNull = string.IsNullOrEmpty(rom.ReadString(Rom.SpamSumKey));

            return !crcNull
                || !md2Null
                || !md4Null
                || !md5Null
                || !sha1Null
                || !sha256Null
                || !sha384Null
                || !sha512Null
                || !spamsumNull;
        }

        /// <summary>
        /// Returns if all of the hashes are set to their 0-byte values or null
        /// </summary>
        private static bool HasZeroHash(this Disk disk)
        {
            string? md5 = disk.ReadString(Disk.MD5Key);
            bool md5Null = string.IsNullOrEmpty(md5) || string.Equals(md5, ZeroHash.MD5Str, StringComparison.OrdinalIgnoreCase);

            string? sha1 = disk.ReadString(Disk.SHA1Key);
            bool sha1Null = string.IsNullOrEmpty(sha1) || string.Equals(sha1, ZeroHash.SHA1Str, StringComparison.OrdinalIgnoreCase);

            return md5Null
                && sha1Null;
        }

        /// <summary>
        /// Returns if all of the hashes are set to their 0-byte values or null
        /// </summary>
        private static bool HasZeroHash(this Media media)
        {
            string? md5 = media.ReadString(Media.MD5Key);
            bool md5Null = string.IsNullOrEmpty(md5) || string.Equals(md5, ZeroHash.MD5Str, StringComparison.OrdinalIgnoreCase);

            string? sha1 = media.ReadString(Media.SHA1Key);
            bool sha1Null = string.IsNullOrEmpty(sha1) || string.Equals(sha1, ZeroHash.SHA1Str, StringComparison.OrdinalIgnoreCase);

            string? sha256 = media.ReadString(Media.SHA256Key);
            bool sha256Null = string.IsNullOrEmpty(sha256) || string.Equals(sha256, ZeroHash.SHA256Str, StringComparison.OrdinalIgnoreCase);

            string? spamsum = media.ReadString(Media.SpamSumKey);
            bool spamsumNull = string.IsNullOrEmpty(spamsum) || string.Equals(spamsum, ZeroHash.SpamSumStr, StringComparison.OrdinalIgnoreCase);

            return md5Null
                && sha1Null
                && sha256Null
                && spamsumNull;
        }

        /// <summary>
        /// Returns if all of the hashes are set to their 0-byte values or null
        /// </summary>
        private static bool HasZeroHash(this Rom rom)
        {
            string? crc = rom.ReadString(Rom.CRCKey);
            bool crcNull = string.IsNullOrEmpty(crc) || string.Equals(crc, ZeroHash.CRC32Str, StringComparison.OrdinalIgnoreCase);

            string? md2 = rom.ReadString(Rom.MD2Key);
            bool md2Null = string.IsNullOrEmpty(md2) || string.Equals(md2, ZeroHash.GetString(HashType.MD2), StringComparison.OrdinalIgnoreCase);

            string? md4 = rom.ReadString(Rom.MD4Key);
            bool md4Null = string.IsNullOrEmpty(md4) || string.Equals(md4, ZeroHash.GetString(HashType.MD4), StringComparison.OrdinalIgnoreCase);

            string? md5 = rom.ReadString(Rom.MD5Key);
            bool md5Null = string.IsNullOrEmpty(md5) || string.Equals(md5, ZeroHash.MD5Str, StringComparison.OrdinalIgnoreCase);

            string? sha1 = rom.ReadString(Rom.SHA1Key);
            bool sha1Null = string.IsNullOrEmpty(sha1) || string.Equals(sha1, ZeroHash.SHA1Str, StringComparison.OrdinalIgnoreCase);

            string? sha256 = rom.ReadString(Rom.SHA256Key);
            bool sha256Null = string.IsNullOrEmpty(sha256) || string.Equals(sha256, ZeroHash.SHA256Str, StringComparison.OrdinalIgnoreCase);

            string? sha384 = rom.ReadString(Rom.SHA384Key);
            bool sha384Null = string.IsNullOrEmpty(sha384) || string.Equals(sha384, ZeroHash.SHA384Str, StringComparison.OrdinalIgnoreCase);

            string? sha512 = rom.ReadString(Rom.SHA512Key);
            bool sha512Null = string.IsNullOrEmpty(sha512) || string.Equals(sha512, ZeroHash.SHA512Str, StringComparison.OrdinalIgnoreCase);

            string? spamsum = rom.ReadString(Rom.SpamSumKey);
            bool spamsumNull = string.IsNullOrEmpty(spamsum) || string.Equals(spamsum, ZeroHash.SpamSumStr, StringComparison.OrdinalIgnoreCase);

            return crcNull
                && md2Null
                && md4Null
                && md5Null
                && sha1Null
                && sha256Null
                && sha384Null
                && sha512Null
                && spamsumNull;
        }

        #endregion

        #region Information Filling

        /// <summary>
        /// Fill any missing size and hash information from another DictionaryBase
        /// </summary>
        public static void FillMissingHashes(this DictionaryBase? self, DictionaryBase? other)
        {
            if (self == null || other == null)
                return;

            switch (self, other)
            {
                case (Disk diskSelf, Disk diskOther):
                    diskSelf.FillMissingHashes(diskOther);
                    break;
                case (Media mediaSelf, Media mediaOther):
                    mediaSelf.FillMissingHashes(mediaOther);
                    break;
                case (Rom romSelf, Rom romOther):
                    romSelf.FillMissingHashes(romOther);
                    break;
            };
        }

        /// <summary>
        /// Fill any missing size and hash information from another Disk
        /// </summary>
        private static void FillMissingHashes(this Disk? self, Disk? other)
        {
            if (self == null || other == null)
                return;

            string? selfMd5 = self.ReadString(Disk.MD5Key);
            string? otherMd5 = other.ReadString(Disk.MD5Key);
            if (string.IsNullOrEmpty(selfMd5) && !string.IsNullOrEmpty(otherMd5))
                self[Disk.MD5Key] = otherMd5;

            string? selfSha1 = self.ReadString(Disk.SHA1Key);
            string? otherSha1 = other.ReadString(Disk.SHA1Key);
            if (string.IsNullOrEmpty(selfSha1) && !string.IsNullOrEmpty(otherSha1))
                self[Disk.SHA1Key] = otherSha1;
        }

        /// <summary>
        /// Fill any missing size and hash information from another Media
        /// </summary>
        private static void FillMissingHashes(this Media? self, Media? other)
        {
            if (self == null || other == null)
                return;

            string? selfMd5 = self.ReadString(Media.MD5Key);
            string? otherMd5 = other.ReadString(Media.MD5Key);
            if (string.IsNullOrEmpty(selfMd5) && !string.IsNullOrEmpty(otherMd5))
                self[Media.MD5Key] = otherMd5;

            string? selfSha1 = self.ReadString(Media.SHA1Key);
            string? otherSha1 = other.ReadString(Media.SHA1Key);
            if (string.IsNullOrEmpty(selfSha1) && !string.IsNullOrEmpty(otherSha1))
                self[Media.SHA1Key] = otherSha1;

            string? selfSha256 = self.ReadString(Media.SHA256Key);
            string? otherSha256 = other.ReadString(Media.SHA256Key);
            if (string.IsNullOrEmpty(selfSha256) && !string.IsNullOrEmpty(otherSha256))
                self[Media.SHA256Key] = otherSha256;

            string? selfSpamSum = self.ReadString(Media.SpamSumKey);
            string? otherSpamSum = other.ReadString(Media.SpamSumKey);
            if (string.IsNullOrEmpty(selfSpamSum) && !string.IsNullOrEmpty(otherSpamSum))
                self[Media.SpamSumKey] = otherSpamSum;
        }

        /// <summary>
        /// Fill any missing size and hash information from another Rom
        /// </summary>
        private static void FillMissingHashes(this Rom? self, Rom? other)
        {
            if (self == null || other == null)
                return;

            long? selfSize = self.ReadLong(Rom.SizeKey);
            long? otherSize = other.ReadLong(Rom.SizeKey);
            if (selfSize == null && otherSize != null)
                self[Rom.SizeKey] = otherSize;

            string? selfCrc = self.ReadString(Rom.CRCKey);
            string? otherCrc = other.ReadString(Rom.CRCKey);
            if (string.IsNullOrEmpty(selfCrc) && !string.IsNullOrEmpty(otherCrc))
                self[Rom.CRCKey] = otherCrc;

            string? selfMd2 = self.ReadString(Rom.MD2Key);
            string? otherMd2 = other.ReadString(Rom.MD2Key);
            if (string.IsNullOrEmpty(selfMd2) && !string.IsNullOrEmpty(otherMd2))
                self[Rom.MD2Key] = otherMd2;

            string? selfMd4 = self.ReadString(Rom.MD4Key);
            string? otherMd4 = other.ReadString(Rom.MD4Key);
            if (string.IsNullOrEmpty(selfMd4) && !string.IsNullOrEmpty(otherMd4))
                self[Rom.MD4Key] = otherMd4;

            string? selfMd5 = self.ReadString(Rom.MD5Key);
            string? otherMd5 = other.ReadString(Rom.MD5Key);
            if (string.IsNullOrEmpty(selfMd5) && !string.IsNullOrEmpty(otherMd5))
                self[Rom.MD5Key] = otherMd5;

            string? selfSha1 = self.ReadString(Rom.SHA1Key);
            string? otherSha1 = other.ReadString(Rom.SHA1Key);
            if (string.IsNullOrEmpty(selfSha1) && !string.IsNullOrEmpty(otherSha1))
                self[Rom.SHA1Key] = otherSha1;

            string? selfSha256 = self.ReadString(Rom.SHA256Key);
            string? otherSha256 = other.ReadString(Rom.SHA256Key);
            if (string.IsNullOrEmpty(selfSha256) && !string.IsNullOrEmpty(otherSha256))
                self[Rom.SHA256Key] = otherSha256;

            string? selfSha384 = self.ReadString(Rom.SHA384Key);
            string? otherSha384 = other.ReadString(Rom.SHA384Key);
            if (string.IsNullOrEmpty(selfSha384) && !string.IsNullOrEmpty(otherSha384))
                self[Rom.SHA384Key] = otherSha384;

            string? selfSha512 = self.ReadString(Rom.SHA512Key);
            string? otherSha512 = other.ReadString(Rom.SHA512Key);
            if (string.IsNullOrEmpty(selfSha512) && !string.IsNullOrEmpty(otherSha512))
                self[Rom.SHA512Key] = otherSha512;

            string? selfSpamSum = self.ReadString(Rom.SpamSumKey);
            string? otherSpamSum = other.ReadString(Rom.SpamSumKey);
            if (string.IsNullOrEmpty(selfSpamSum) && !string.IsNullOrEmpty(otherSpamSum))
                self[Rom.SpamSumKey] = otherSpamSum;
        }

        #endregion

        #region Suffix Generation

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        public static string GetDuplicateSuffix(this DictionaryBase? self)
        {
            if (self == null)
                return string.Empty;

            return self switch
            {
                Disk diskSelf => diskSelf.GetDuplicateSuffix(),
                Media mediaSelf => mediaSelf.GetDuplicateSuffix(),
                Rom romSelf => romSelf.GetDuplicateSuffix(),
                _ => "_1",
            };
        }

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        private static string GetDuplicateSuffix(this Disk self)
        {
            string? md5 = self.ReadString(Disk.MD5Key);
            if (!string.IsNullOrEmpty(md5))
                return $"_{md5}";

            string? sha1 = self.ReadString(Disk.SHA1Key);
            if (!string.IsNullOrEmpty(sha1))
                return $"_{sha1}";

            return "_1";
        }

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        private static string GetDuplicateSuffix(this Media self)
        {
            string? md5 = self.ReadString(Media.MD5Key);
            if (!string.IsNullOrEmpty(md5))
                return $"_{md5}";

            string? sha1 = self.ReadString(Media.SHA1Key);
            if (!string.IsNullOrEmpty(sha1))
                return $"_{sha1}";

            string? sha256 = self.ReadString(Media.SHA256Key);
            if (!string.IsNullOrEmpty(sha256))
                return $"_{sha256}";

            string? spamSum = self.ReadString(Media.SpamSumKey);
            if (!string.IsNullOrEmpty(spamSum))
                return $"_{spamSum}";

            return "_1";
        }

        /// <summary>
        /// Get unique duplicate suffix on name collision
        /// </summary>
        private static string GetDuplicateSuffix(this Rom self)
        {
            string? crc = self.ReadString(Rom.CRCKey);
            if (!string.IsNullOrEmpty(crc))
                return $"_{crc}";

            string? md2 = self.ReadString(Rom.MD2Key);
            if (!string.IsNullOrEmpty(md2))
                return $"_{md2}";

            string? md4 = self.ReadString(Rom.MD4Key);
            if (!string.IsNullOrEmpty(md4))
                return $"_{md4}";

            string? md5 = self.ReadString(Rom.MD5Key);
            if (!string.IsNullOrEmpty(md5))
                return $"_{md5}";

            string? sha1 = self.ReadString(Rom.SHA1Key);
            if (!string.IsNullOrEmpty(sha1))
                return $"_{sha1}";

            string? sha256 = self.ReadString(Rom.SHA256Key);
            if (!string.IsNullOrEmpty(sha256))
                return $"_{sha256}";

            string? sha384 = self.ReadString(Rom.SHA384Key);
            if (!string.IsNullOrEmpty(sha384))
                return $"_{sha384}";

            string? sha512 = self.ReadString(Rom.SHA512Key);
            if (!string.IsNullOrEmpty(sha512))
                return $"_{sha512}";

            string? spamSum = self.ReadString(Rom.SpamSumKey);
            if (!string.IsNullOrEmpty(spamSum))
                return $"_{spamSum}";

            return "_1";
        }

        #endregion
    }
}