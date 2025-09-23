using System;
using System.Linq;
using SabreTools.Core.Filter;
using SabreTools.DatFiles.Formats;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using Xunit;

namespace SabreTools.DatFiles.Test
{
    /// <summary>
    /// Contains tests for all specific DatFile formats
    /// </summary>
    public class FormatsTests
    {
        #region Testing Constants

        /// <summary>
        /// All defined item types
        /// </summary>
        private static readonly ItemType[] AllTypes = Enum.GetValues(typeof(ItemType)) as ItemType[] ?? [];

        #endregion

        #region ArchiveDotOrg

        [Fact]
        public void ArchiveDotOrg_SupportedTypes()
        {
            var datFile = new ArchiveDotOrg(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        #endregion

        #region AttractMode

        [Fact]
        public void AttractMode_SupportedTypes()
        {
            var datFile = new AttractMode(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void AttractMode_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new AttractMode(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
            ]));
        }

        #endregion

        #region ClrMamePro

        [Fact]
        public void ClrMamePro_SupportedTypes()
        {
            var datFile = new ClrMamePro(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Archive,
                ItemType.BiosSet,
                ItemType.Chip,
                ItemType.DipSwitch,
                ItemType.Disk,
                ItemType.Display,
                ItemType.Driver,
                ItemType.Input,
                ItemType.Media,
                ItemType.Release,
                ItemType.Rom,
                ItemType.Sample,
                ItemType.Sound,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Release()
        {
            var datItem = new Release();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Release.NameKey,
                Models.Metadata.Release.RegionKey,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_BiosSet()
        {
            var datItem = new BiosSet();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.BiosSet.NameKey,
                Models.Metadata.BiosSet.DescriptionKey,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SizeKey,
                Models.Metadata.Rom.SHA1Key,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Disk.NameKey,
                Models.Metadata.Disk.SHA1Key,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Sample()
        {
            var datItem = new Sample();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Sample.NameKey,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Archive()
        {
            var datItem = new Archive();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Archive.NameKey,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Chip()
        {
            var datItem = new Chip();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Chip.ChipTypeKey,
                Models.Metadata.Chip.NameKey,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Display()
        {
            var datItem = new Display();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Display.DisplayTypeKey,
                Models.Metadata.Display.RotateKey,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Sound()
        {
            var datItem = new Sound();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Sound.ChannelsKey,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Input()
        {
            var datItem = new Input();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Input.PlayersKey,
                Models.Metadata.Input.ControlKey,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_DipSwitch()
        {
            var datItem = new DipSwitch();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.DipSwitch.NameKey,
            ]));
        }

        [Fact]
        public void ClrMamePro_GetMissingRequiredFields_Driver()
        {
            var datItem = new Driver();
            var datFile = new ClrMamePro(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Driver.StatusKey,
                Models.Metadata.Driver.EmulationKey,
            ]));
        }

        #endregion

        #region DosCenter

        [Fact]
        public void DosCenter_SupportedTypes()
        {
            var datFile = new DosCenter(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void DosCenter_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new DosCenter(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SizeKey,
                Models.Metadata.Rom.CRCKey,
            ]));
        }

        #endregion

        #region EverdriveSMDB

        [Fact]
        public void EverdriveSMDB_SupportedTypes()
        {
            var datFile = new EverdriveSMDB(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void EverdriveSMDB_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new EverdriveSMDB(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SHA256Key,
                Models.Metadata.Rom.SHA1Key,
                Models.Metadata.Rom.MD5Key,
                Models.Metadata.Rom.CRCKey,
            ]));
        }

        #endregion

        #region Hashfile

        [Fact]
        public void SfvFile_SupportedTypes()
        {
            var datFile = new SfvFile(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void SfvFile_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new SfvFile(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.CRCKey,
            ]));
        }

        [Fact]
        public void Md2File_SupportedTypes()
        {
            var datFile = new Md2File(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void Md2File_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Md2File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.MD2Key,
            ]));
        }

        [Fact]
        public void Md4File_SupportedTypes()
        {
            var datFile = new Md4File(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void Md4File_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Md4File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.MD4Key,
            ]));
        }

        [Fact]
        public void Md5File_SupportedTypes()
        {
            var datFile = new Md5File(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Disk,
                ItemType.Media,
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void Md5File_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new Md5File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Disk.NameKey,
                Models.Metadata.Disk.MD5Key,
            ]));
        }

        [Fact]
        public void Md5File_GetMissingRequiredFields_Media()
        {
            var datItem = new Media();
            var datFile = new Md5File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Media.NameKey,
                Models.Metadata.Media.MD5Key,
            ]));
        }

        [Fact]
        public void Md5File_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Md5File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.MD5Key,
            ]));
        }

        [Fact]
        public void RipeMD128File_SupportedTypes()
        {
            var datFile = new RipeMD128File(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void RipeMD128File_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new RipeMD128File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.RIPEMD128Key,
            ]));
        }

        [Fact]
        public void RipeMD160File_SupportedTypes()
        {
            var datFile = new RipeMD160File(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void RipeMD160File_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new RipeMD160File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.RIPEMD160Key,
            ]));
        }

        [Fact]
        public void Sha1File_SupportedTypes()
        {
            var datFile = new Sha1File(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Disk,
                ItemType.Media,
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void Sha1File_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new Sha1File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Disk.NameKey,
                Models.Metadata.Disk.SHA1Key,
            ]));
        }

        [Fact]
        public void Sha1File_GetMissingRequiredFields_Media()
        {
            var datItem = new Media();
            var datFile = new Sha1File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Media.NameKey,
                Models.Metadata.Media.SHA1Key,
            ]));
        }

        [Fact]
        public void Sha1File_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Sha1File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SHA1Key,
            ]));
        }

        [Fact]
        public void Sha256File_SupportedTypes()
        {
            var datFile = new Sha256File(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Media,
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void Sha256File_GetMissingRequiredFields_Media()
        {
            var datItem = new Media();
            var datFile = new Sha256File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Media.NameKey,
                Models.Metadata.Media.SHA256Key,
            ]));
        }

        [Fact]
        public void Sha256File_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Sha256File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SHA256Key,
            ]));
        }

        [Fact]
        public void Sha384File_SupportedTypes()
        {
            var datFile = new Sha384File(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void Sha384File_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Sha384File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SHA384Key,
            ]));
        }

        [Fact]
        public void Sha512File_SupportedTypes()
        {
            var datFile = new Sha512File(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void Sha512File_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Sha512File(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SHA512Key,
            ]));
        }

        [Fact]
        public void SpamSumFile_SupportedTypes()
        {
            var datFile = new SpamSumFile(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Media,
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void SpamSumFile_GetMissingRequiredFields_Media()
        {
            var datItem = new Media();
            var datFile = new SpamSumFile(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Media.NameKey,
                Models.Metadata.Media.SpamSumKey,
            ]));
        }

        [Fact]
        public void SpamSumFile_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new SpamSumFile(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SpamSumKey,
            ]));
        }

        #endregion

        #region Listrom

        [Fact]
        public void Listrom_SupportedTypes()
        {
            var datFile = new Listrom(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Disk,
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void Listrom_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new Listrom(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Disk.NameKey,
                Models.Metadata.Disk.SHA1Key,
            ]));
        }

        [Fact]
        public void Listrom_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Listrom(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SizeKey,
                Models.Metadata.Rom.CRCKey,
                Models.Metadata.Rom.SHA1Key,
            ]));
        }

        #endregion

        #region Listxml

        [Fact]
        public void Listxml_SupportedTypes()
        {
            var datFile = new Listxml(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Adjuster,
                ItemType.BiosSet,
                ItemType.Chip,
                ItemType.Condition,
                ItemType.Configuration,
                ItemType.Device,
                ItemType.DeviceRef,
                ItemType.DipSwitch,
                ItemType.Disk,
                ItemType.Display,
                ItemType.Driver,
                ItemType.Feature,
                ItemType.Input,
                ItemType.Port,
                ItemType.RamOption,
                ItemType.Rom,
                ItemType.Sample,
                ItemType.Slot,
                ItemType.SoftwareList,
                ItemType.Sound,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_BiosSet()
        {
            var datItem = new BiosSet();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.BiosSet.NameKey,
                Models.Metadata.BiosSet.DescriptionKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SizeKey,
                Models.Metadata.Rom.SHA1Key,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Disk.NameKey,
                Models.Metadata.Disk.SHA1Key,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_DeviceRef()
        {
            var datItem = new DeviceRef();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.DeviceRef.NameKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Sample()
        {
            var datItem = new Sample();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Sample.NameKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Chip()
        {
            var datItem = new Chip();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Chip.NameKey,
                Models.Metadata.Chip.ChipTypeKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Display()
        {
            var datItem = new Display();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Display.DisplayTypeKey,
                Models.Metadata.Display.RefreshKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Sound()
        {
            var datItem = new Sound();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Sound.ChannelsKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Input()
        {
            var datItem = new Input();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Input.PlayersKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_DipSwitch()
        {
            var datItem = new DipSwitch();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.DipSwitch.NameKey,
                Models.Metadata.DipSwitch.TagKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Configuration()
        {
            var datItem = new Configuration();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Configuration.NameKey,
                Models.Metadata.Configuration.TagKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Port()
        {
            var datItem = new Port();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Port.TagKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Adjuster()
        {
            var datItem = new Adjuster();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Adjuster.NameKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Driver()
        {
            var datItem = new Driver();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Driver.StatusKey,
                Models.Metadata.Driver.EmulationKey,
                Models.Metadata.Driver.CocktailKey,
                Models.Metadata.Driver.SaveStateKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Feature()
        {
            var datItem = new Feature();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Feature.FeatureTypeKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Device()
        {
            var datItem = new Device();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Device.DeviceTypeKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_Slot()
        {
            var datItem = new Slot();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Slot.NameKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_SoftwareList()
        {
            var datItem = new DatItems.Formats.SoftwareList();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.SoftwareList.TagKey,
                Models.Metadata.SoftwareList.NameKey,
                Models.Metadata.SoftwareList.StatusKey,
            ]));
        }

        [Fact]
        public void Listxml_GetMissingRequiredFields_RamOption()
        {
            var datItem = new RamOption();
            var datFile = new Listxml(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.RamOption.NameKey,
            ]));
        }

        #endregion

        #region Logiqx

        [Fact]
        public void Logiqx_SupportedTypes()
        {
            var datFile = new Logiqx(null, false);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Archive,
                ItemType.BiosSet,
                ItemType.DeviceRef,
                ItemType.Disk,
                ItemType.Driver,
                ItemType.Media,
                ItemType.Release,
                ItemType.Rom,
                ItemType.Sample,
                ItemType.SoftwareList,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_Release()
        {
            var datItem = new Release();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Release.NameKey,
                Models.Metadata.Release.RegionKey,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_BiosSet()
        {
            var datItem = new BiosSet();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.BiosSet.NameKey,
                Models.Metadata.BiosSet.DescriptionKey,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SizeKey,
                Models.Metadata.Rom.SHA1Key,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Disk.NameKey,
                Models.Metadata.Disk.SHA1Key,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_Media()
        {
            var datItem = new Media();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Media.NameKey,
                Models.Metadata.Media.SHA1Key,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_DeviceRef()
        {
            var datItem = new DeviceRef();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.DeviceRef.NameKey,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_Sample()
        {
            var datItem = new Sample();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Sample.NameKey,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_Archive()
        {
            var datItem = new Archive();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Archive.NameKey,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_Driver()
        {
            var datItem = new Driver();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Driver.StatusKey,
                Models.Metadata.Driver.EmulationKey,
                Models.Metadata.Driver.CocktailKey,
                Models.Metadata.Driver.SaveStateKey,
            ]));
        }

        [Fact]
        public void Logiqx_GetMissingRequiredFields_SoftwareList()
        {
            var datItem = new DatItems.Formats.SoftwareList();
            var datFile = new Logiqx(null, false);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.SoftwareList.TagKey,
                Models.Metadata.SoftwareList.NameKey,
                Models.Metadata.SoftwareList.StatusKey,
            ]));
        }

        #endregion

        #region Missfile

        [Fact]
        public void Missfile_SupportedTypes()
        {
            var datFile = new Missfile(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual(AllTypes));
        }

        [Fact]
        public void Missfile_ParseFile_Throws()
        {
            var datFile = new Missfile(null);
            Assert.Throws<NotImplementedException>(() => datFile.ParseFile("path", 0, true));
        }

        #endregion

        #region OfflineList

        [Fact]
        public void OfflineList_SupportedTypes()
        {
            var datFile = new OfflineList(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void OfflineList_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new OfflineList(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.SizeKey,
                Models.Metadata.Rom.CRCKey,
            ]));
        }

        #endregion

        #region OpenMSX

        [Fact]
        public void OpenMSX_SupportedTypes()
        {
            var datFile = new OpenMSX(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void OpenMSX_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new OpenMSX(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SHA1Key,
            ]));
        }

        #endregion

        #region RomCenter

        [Fact]
        public void RomCenter_SupportedTypes()
        {
            var datFile = new RomCenter(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void RomCenter_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new RomCenter(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.CRCKey,
                Models.Metadata.Rom.SizeKey,
            ]));
        }

        #endregion

        #region SabreJSON

        [Fact]
        public void SabreJSON_SupportedTypes()
        {
            var datFile = new SabreJSON(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual(AllTypes));
        }

        #endregion

        #region SabreXML

        [Fact]
        public void SabreXML_SupportedTypes()
        {
            var datFile = new SabreXML(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual(AllTypes));
        }

        #endregion

        #region SeparatedValue

        [Fact]
        public void CommaSeparatedValue_SupportedTypes()
        {
            var datFile = new CommaSeparatedValue(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Disk,
                ItemType.Media,
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void CommaSeparatedValue_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new CommaSeparatedValue(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Disk.NameKey,
                Models.Metadata.Disk.SHA1Key,
            ]));
        }

        [Fact]
        public void CommaSeparatedValue_GetMissingRequiredFields_Media()
        {
            var datItem = new Media();
            var datFile = new CommaSeparatedValue(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Media.NameKey,
                Models.Metadata.Media.SHA1Key,
            ]));
        }

        [Fact]
        public void CommaSeparatedValue_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new CommaSeparatedValue(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SizeKey,
                Models.Metadata.Rom.SHA1Key,
            ]));
        }

        [Fact]
        public void SemicolonSeparatedValue_SupportedTypes()
        {
            var datFile = new SemicolonSeparatedValue(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Disk,
                ItemType.Media,
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void SemicolonSeparatedValue_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new SemicolonSeparatedValue(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Disk.NameKey,
                Models.Metadata.Disk.SHA1Key,
            ]));
        }

        [Fact]
        public void SemicolonSeparatedValue_GetMissingRequiredFields_Media()
        {
            var datItem = new Media();
            var datFile = new SemicolonSeparatedValue(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Media.NameKey,
                Models.Metadata.Media.SHA1Key,
            ]));
        }

        [Fact]
        public void SemicolonSeparatedValue_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new SemicolonSeparatedValue(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SizeKey,
                Models.Metadata.Rom.SHA1Key,
            ]));
        }

        [Fact]
        public void TabSeparatedValue_SupportedTypes()
        {
            var datFile = new TabSeparatedValue(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.Disk,
                ItemType.Media,
                ItemType.Rom,
            ]));
        }

        [Fact]
        public void TabSeparatedValue_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new TabSeparatedValue(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Disk.NameKey,
                Models.Metadata.Disk.SHA1Key,
            ]));
        }

        [Fact]
        public void TabSeparatedValue_GetMissingRequiredFields_Media()
        {
            var datItem = new Media();
            var datFile = new TabSeparatedValue(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Media.NameKey,
                Models.Metadata.Media.SHA1Key,
            ]));
        }

        [Fact]
        public void TabSeparatedValue_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new TabSeparatedValue(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Rom.NameKey,
                Models.Metadata.Rom.SizeKey,
                Models.Metadata.Rom.SHA1Key,
            ]));
        }

        #endregion

        #region SoftwareList

        [Fact]
        public void SoftwareList_SupportedTypes()
        {
            var datFile = new Formats.SoftwareList(null);
            var actual = datFile.SupportedTypes;
            Assert.True(actual.SequenceEqual([
                ItemType.DipSwitch,
                ItemType.Disk,
                ItemType.Info,
                ItemType.PartFeature,
                ItemType.Rom,
                ItemType.SharedFeat,
            ]));
        }

        [Fact]
        public void SoftwareList_GetMissingRequiredFields_DipSwitch()
        {
            var datItem = new DipSwitch();
            var datFile = new Formats.SoftwareList(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Part.NameKey,
                Models.Metadata.Part.InterfaceKey,
                Models.Metadata.DipSwitch.NameKey,
                Models.Metadata.DipSwitch.TagKey,
                Models.Metadata.DipSwitch.MaskKey,
            ]));
        }

        [Fact]
        public void SoftwareList_GetMissingRequiredFields_Disk()
        {
            var datItem = new Disk();
            var datFile = new Formats.SoftwareList(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Part.NameKey,
                Models.Metadata.Part.InterfaceKey,
                Models.Metadata.DiskArea.NameKey,
                Models.Metadata.Disk.NameKey,
            ]));
        }

        [Fact]
        public void SoftwareList_GetMissingRequiredFields_Info()
        {
            var datItem = new Info();
            var datFile = new Formats.SoftwareList(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Info.NameKey,
            ]));
        }

        [Fact]
        public void SoftwareList_GetMissingRequiredFields_Rom()
        {
            var datItem = new Rom();
            var datFile = new Formats.SoftwareList(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.Part.NameKey,
                Models.Metadata.Part.InterfaceKey,
                Models.Metadata.DataArea.NameKey,
                Models.Metadata.DataArea.SizeKey,
            ]));
        }

        [Fact]
        public void SoftwareList_GetMissingRequiredFields_SharedFeat()
        {
            var datItem = new SharedFeat();
            var datFile = new Formats.SoftwareList(null);

            var actual = datFile.GetMissingRequiredFields(datItem);

            Assert.NotNull(actual);
            Assert.True(actual.SequenceEqual([
                Models.Metadata.SharedFeat.NameKey,
            ]));
        }

        #endregion
    }
}