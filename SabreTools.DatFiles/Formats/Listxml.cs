using System;
using System.Collections.Generic;
using SabreTools.Core.Filter;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a MAME/M1 XML DAT
    /// </summary>
    public sealed class Listxml : SerializableDatFile<Models.Listxml.Mame, Serialization.Deserializers.Listxml, Serialization.Serializers.Listxml, Serialization.CrossModel.Listxml>
    {
        #region Constants

        /// <summary>
        /// DTD for original MAME XML DATs
        /// </summary>
        private const string MAMEDTD = @"<!DOCTYPE mame [
<!ELEMENT mame (machine+)>
	<!ATTLIST mame build CDATA #IMPLIED>
	<!ATTLIST mame debug (yes|no) ""no"">
	<!ATTLIST mame mameconfig CDATA #REQUIRED>
	<!ELEMENT machine (description, year?, manufacturer?, biosset*, rom*, disk*, device_ref*, sample*, chip*, display*, sound?, input?, dipswitch*, configuration*, port*, adjuster*, driver?, feature*, device*, slot*, softwarelist*, ramoption*)>
		<!ATTLIST machine name CDATA #REQUIRED>
		<!ATTLIST machine sourcefile CDATA #IMPLIED>
		<!ATTLIST machine isbios (yes|no) ""no"">
		<!ATTLIST machine isdevice (yes|no) ""no"">
		<!ATTLIST machine ismechanical (yes|no) ""no"">
		<!ATTLIST machine runnable (yes|no) ""yes"">
		<!ATTLIST machine cloneof CDATA #IMPLIED>
		<!ATTLIST machine romof CDATA #IMPLIED>
		<!ATTLIST machine sampleof CDATA #IMPLIED>
		<!ELEMENT description (#PCDATA)>
		<!ELEMENT year (#PCDATA)>
		<!ELEMENT manufacturer (#PCDATA)>
		<!ELEMENT biosset EMPTY>
			<!ATTLIST biosset name CDATA #REQUIRED>
			<!ATTLIST biosset description CDATA #REQUIRED>
			<!ATTLIST biosset default (yes|no) ""no"">
		<!ELEMENT rom EMPTY>
			<!ATTLIST rom name CDATA #REQUIRED>
			<!ATTLIST rom bios CDATA #IMPLIED>
			<!ATTLIST rom size CDATA #REQUIRED>
			<!ATTLIST rom crc CDATA #IMPLIED>
			<!ATTLIST rom sha1 CDATA #IMPLIED>
			<!ATTLIST rom merge CDATA #IMPLIED>
			<!ATTLIST rom region CDATA #IMPLIED>
			<!ATTLIST rom offset CDATA #IMPLIED>
			<!ATTLIST rom status (baddump|nodump|good) ""good"">
			<!ATTLIST rom optional (yes|no) ""no"">
		<!ELEMENT disk EMPTY>
			<!ATTLIST disk name CDATA #REQUIRED>
			<!ATTLIST disk sha1 CDATA #IMPLIED>
			<!ATTLIST disk merge CDATA #IMPLIED>
			<!ATTLIST disk region CDATA #IMPLIED>
			<!ATTLIST disk index CDATA #IMPLIED>
			<!ATTLIST disk writable (yes|no) ""no"">
			<!ATTLIST disk status (baddump|nodump|good) ""good"">
			<!ATTLIST disk optional (yes|no) ""no"">
		<!ELEMENT device_ref EMPTY>
			<!ATTLIST device_ref name CDATA #REQUIRED>
		<!ELEMENT sample EMPTY>
			<!ATTLIST sample name CDATA #REQUIRED>
		<!ELEMENT chip EMPTY>
			<!ATTLIST chip name CDATA #REQUIRED>
			<!ATTLIST chip tag CDATA #IMPLIED>
			<!ATTLIST chip type (cpu|audio) #REQUIRED>
			<!ATTLIST chip clock CDATA #IMPLIED>
		<!ELEMENT display EMPTY>
			<!ATTLIST display tag CDATA #IMPLIED>
			<!ATTLIST display type (raster|vector|lcd|svg|unknown) #REQUIRED>
			<!ATTLIST display rotate (0|90|180|270) #IMPLIED>
			<!ATTLIST display flipx (yes|no) ""no"">
			<!ATTLIST display width CDATA #IMPLIED>
			<!ATTLIST display height CDATA #IMPLIED>
			<!ATTLIST display refresh CDATA #REQUIRED>
			<!ATTLIST display pixclock CDATA #IMPLIED>
			<!ATTLIST display htotal CDATA #IMPLIED>
			<!ATTLIST display hbend CDATA #IMPLIED>
			<!ATTLIST display hbstart CDATA #IMPLIED>
			<!ATTLIST display vtotal CDATA #IMPLIED>
			<!ATTLIST display vbend CDATA #IMPLIED>
			<!ATTLIST display vbstart CDATA #IMPLIED>
		<!ELEMENT sound EMPTY>
			<!ATTLIST sound channels CDATA #REQUIRED>
		<!ELEMENT condition EMPTY>
			<!ATTLIST condition tag CDATA #REQUIRED>
			<!ATTLIST condition mask CDATA #REQUIRED>
			<!ATTLIST condition relation (eq|ne|gt|le|lt|ge) #REQUIRED>
			<!ATTLIST condition value CDATA #REQUIRED>
		<!ELEMENT input (control*)>
			<!ATTLIST input service (yes|no) ""no"">
			<!ATTLIST input tilt (yes|no) ""no"">
			<!ATTLIST input players CDATA #REQUIRED>
			<!ATTLIST input coins CDATA #IMPLIED>
			<!ELEMENT control EMPTY>
				<!ATTLIST control type CDATA #REQUIRED>
				<!ATTLIST control player CDATA #IMPLIED>
				<!ATTLIST control buttons CDATA #IMPLIED>
				<!ATTLIST control reqbuttons CDATA #IMPLIED>
				<!ATTLIST control minimum CDATA #IMPLIED>
				<!ATTLIST control maximum CDATA #IMPLIED>
				<!ATTLIST control sensitivity CDATA #IMPLIED>
				<!ATTLIST control keydelta CDATA #IMPLIED>
				<!ATTLIST control reverse (yes|no) ""no"">
				<!ATTLIST control ways CDATA #IMPLIED>
				<!ATTLIST control ways2 CDATA #IMPLIED>
				<!ATTLIST control ways3 CDATA #IMPLIED>
		<!ELEMENT dipswitch (condition?, diplocation*, dipvalue*)>
			<!ATTLIST dipswitch name CDATA #REQUIRED>
			<!ATTLIST dipswitch tag CDATA #REQUIRED>
			<!ATTLIST dipswitch mask CDATA #REQUIRED>
			<!ELEMENT diplocation EMPTY>
				<!ATTLIST diplocation name CDATA #REQUIRED>
				<!ATTLIST diplocation number CDATA #REQUIRED>
				<!ATTLIST diplocation inverted (yes|no) ""no"">
			<!ELEMENT dipvalue (condition?)>
				<!ATTLIST dipvalue name CDATA #REQUIRED>
				<!ATTLIST dipvalue value CDATA #REQUIRED>
				<!ATTLIST dipvalue default (yes|no) ""no"">
		<!ELEMENT configuration (condition?, conflocation*, confsetting*)>
			<!ATTLIST configuration name CDATA #REQUIRED>
			<!ATTLIST configuration tag CDATA #REQUIRED>
			<!ATTLIST configuration mask CDATA #REQUIRED>
			<!ELEMENT conflocation EMPTY>
				<!ATTLIST conflocation name CDATA #REQUIRED>
				<!ATTLIST conflocation number CDATA #REQUIRED>
				<!ATTLIST conflocation inverted (yes|no) ""no"">
			<!ELEMENT confsetting (condition?)>
				<!ATTLIST confsetting name CDATA #REQUIRED>
				<!ATTLIST confsetting value CDATA #REQUIRED>
				<!ATTLIST confsetting default (yes|no) ""no"">
		<!ELEMENT port (analog*)>
			<!ATTLIST port tag CDATA #REQUIRED>
			<!ELEMENT analog EMPTY>
				<!ATTLIST analog mask CDATA #REQUIRED>
		<!ELEMENT adjuster (condition?)>
			<!ATTLIST adjuster name CDATA #REQUIRED>
			<!ATTLIST adjuster default CDATA #REQUIRED>
		<!ELEMENT driver EMPTY>
			<!ATTLIST driver status (good|imperfect|preliminary) #REQUIRED>
			<!ATTLIST driver emulation (good|imperfect|preliminary) #REQUIRED>
			<!ATTLIST driver cocktail (good|imperfect|preliminary) #IMPLIED>
			<!ATTLIST driver savestate (supported|unsupported) #REQUIRED>
			<!ATTLIST driver requiresartwork (yes|no) ""no"">
			<!ATTLIST driver unofficial (yes|no) ""no"">
			<!ATTLIST driver nosoundhardware (yes|no) ""no"">
			<!ATTLIST driver incomplete (yes|no) ""no"">
		<!ELEMENT feature EMPTY>
			<!ATTLIST feature type (protection|timing|graphics|palette|sound|capture|camera|microphone|controls|keyboard|mouse|media|disk|printer|tape|punch|drum|rom|comms|lan|wan) #REQUIRED>
			<!ATTLIST feature status (unemulated|imperfect) #IMPLIED>
			<!ATTLIST feature overall (unemulated|imperfect) #IMPLIED>
		<!ELEMENT device (instance?, extension*)>
			<!ATTLIST device type CDATA #REQUIRED>
			<!ATTLIST device tag CDATA #IMPLIED>
			<!ATTLIST device fixed_image CDATA #IMPLIED>
			<!ATTLIST device mandatory CDATA #IMPLIED>
			<!ATTLIST device interface CDATA #IMPLIED>
			<!ELEMENT instance EMPTY>
				<!ATTLIST instance name CDATA #REQUIRED>
				<!ATTLIST instance briefname CDATA #REQUIRED>
			<!ELEMENT extension EMPTY>
				<!ATTLIST extension name CDATA #REQUIRED>
		<!ELEMENT slot (slotoption*)>
			<!ATTLIST slot name CDATA #REQUIRED>
			<!ELEMENT slotoption EMPTY>
				<!ATTLIST slotoption name CDATA #REQUIRED>
				<!ATTLIST slotoption devname CDATA #REQUIRED>
				<!ATTLIST slotoption default (yes|no) ""no"">
		<!ELEMENT softwarelist EMPTY>
			<!ATTLIST softwarelist tag CDATA #REQUIRED>
			<!ATTLIST softwarelist name CDATA #REQUIRED>
			<!ATTLIST softwarelist status (original|compatible) #REQUIRED>
			<!ATTLIST softwarelist filter CDATA #IMPLIED>
		<!ELEMENT ramoption (#PCDATA)>
			<!ATTLIST ramoption name CDATA #REQUIRED>
			<!ATTLIST ramoption default CDATA #IMPLIED>
]>
";

        #endregion

        #region Fields

        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
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
            ];

        #endregion

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public Listxml(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.Listxml);
        }

        /// <inheritdoc/>
        public override void ParseFile(string filename,
            int indexId,
            bool keep,
            bool statsOnly = false,
            FilterRunner? filterRunner = null,
            bool throwOnError = false)
        {
            try
            {
                // Deserialize the input file
                var mame = new Serialization.Deserializers.Listxml().Deserialize(filename);
                Models.Metadata.MetadataFile? metadata;
                if (mame == null)
                {
                    var m1 = new Serialization.Deserializers.M1().Deserialize(filename);
                    metadata = new Serialization.CrossModel.M1().Serialize(m1);
                }
                else
                {
                    metadata = new Serialization.CrossModel.Listxml().Serialize(mame);
                }

                // Convert to the internal format
                ConvertFromMetadata(metadata, filename, indexId, keep, statsOnly, filterRunner);
            }
            catch (Exception ex) when (!throwOnError)
            {
                string message = $"'{filename}' - An error occurred during parsing";
                _logger.Error(ex, message);
            }
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];
            switch (datItem)
            {
                case BiosSet biosset:
                    if (string.IsNullOrEmpty(biosset.GetName()))
                        missingFields.Add(Models.Metadata.BiosSet.NameKey);
                    if (string.IsNullOrEmpty(biosset.GetStringFieldValue(Models.Metadata.BiosSet.DescriptionKey)))
                        missingFields.Add(Models.Metadata.BiosSet.DescriptionKey);
                    break;

                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetName()))
                        missingFields.Add(Models.Metadata.Rom.NameKey);
                    if (rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey) == null || rom.GetInt64FieldValue(Models.Metadata.Rom.SizeKey) < 0)
                        missingFields.Add(Models.Metadata.Rom.SizeKey);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.CRCKey))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Models.Metadata.Rom.SHA1Key)))
                    {
                        missingFields.Add(Models.Metadata.Rom.SHA1Key);
                    }
                    break;

                case Disk disk:
                    if (string.IsNullOrEmpty(disk.GetName()))
                        missingFields.Add(Models.Metadata.Disk.NameKey);
                    if (string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.MD5Key))
                        && string.IsNullOrEmpty(disk.GetStringFieldValue(Models.Metadata.Disk.SHA1Key)))
                    {
                        missingFields.Add(Models.Metadata.Disk.SHA1Key);
                    }
                    break;

                case DeviceRef deviceref:
                    if (string.IsNullOrEmpty(deviceref.GetName()))
                        missingFields.Add(Models.Metadata.DeviceRef.NameKey);
                    break;

                case Sample sample:
                    if (string.IsNullOrEmpty(sample.GetName()))
                        missingFields.Add(Models.Metadata.Sample.NameKey);
                    break;

                case Chip chip:
                    if (string.IsNullOrEmpty(chip.GetName()))
                        missingFields.Add(Models.Metadata.Chip.NameKey);
                    if (chip.GetStringFieldValue(Models.Metadata.Chip.ChipTypeKey).AsChipType() == ChipType.NULL)
                        missingFields.Add(Models.Metadata.Chip.ChipTypeKey);
                    break;

                case Display display:
                    if (display.GetStringFieldValue(Models.Metadata.Display.DisplayTypeKey).AsDisplayType() == DisplayType.NULL)
                        missingFields.Add(Models.Metadata.Display.DisplayTypeKey);
                    if (display.GetDoubleFieldValue(Models.Metadata.Display.RefreshKey) == null)
                        missingFields.Add(Models.Metadata.Display.RefreshKey);
                    break;

                case Sound sound:
                    if (sound.GetInt64FieldValue(Models.Metadata.Sound.ChannelsKey) == null)
                        missingFields.Add(Models.Metadata.Sound.ChannelsKey);
                    break;

                case Input input:
                    if (input.GetInt64FieldValue(Models.Metadata.Input.PlayersKey) == null)
                        missingFields.Add(Models.Metadata.Input.PlayersKey);
                    break;

                case DipSwitch dipswitch:
                    if (string.IsNullOrEmpty(dipswitch.GetName()))
                        missingFields.Add(Models.Metadata.DipSwitch.NameKey);
                    if (string.IsNullOrEmpty(dipswitch.GetStringFieldValue(Models.Metadata.DipSwitch.TagKey)))
                        missingFields.Add(Models.Metadata.DipSwitch.TagKey);
                    break;

                case Configuration configuration:
                    if (string.IsNullOrEmpty(configuration.GetName()))
                        missingFields.Add(Models.Metadata.Configuration.NameKey);
                    if (string.IsNullOrEmpty(configuration.GetStringFieldValue(Models.Metadata.Configuration.TagKey)))
                        missingFields.Add(Models.Metadata.Configuration.TagKey);
                    break;

                case Port port:
                    if (string.IsNullOrEmpty(port.GetStringFieldValue(Models.Metadata.Port.TagKey)))
                        missingFields.Add(Models.Metadata.Port.TagKey);
                    break;

                case Adjuster adjuster:
                    if (string.IsNullOrEmpty(adjuster.GetName()))
                        missingFields.Add(Models.Metadata.Adjuster.NameKey);
                    break;

                case Driver driver:
                    if (driver.GetStringFieldValue(Models.Metadata.Driver.StatusKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Models.Metadata.Driver.StatusKey);
                    if (driver.GetStringFieldValue(Models.Metadata.Driver.EmulationKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Models.Metadata.Driver.EmulationKey);
                    if (driver.GetStringFieldValue(Models.Metadata.Driver.CocktailKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Models.Metadata.Driver.CocktailKey);
                    if (driver.GetStringFieldValue(Models.Metadata.Driver.SaveStateKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Models.Metadata.Driver.SaveStateKey);
                    break;

                case Feature feature:
                    if (feature.GetStringFieldValue(Models.Metadata.Feature.FeatureTypeKey).AsFeatureType() == FeatureType.NULL)
                        missingFields.Add(Models.Metadata.Feature.FeatureTypeKey);
                    break;

                case Device device:
                    if (device.GetStringFieldValue(Models.Metadata.Device.DeviceTypeKey).AsDeviceType() == DeviceType.NULL)
                        missingFields.Add(Models.Metadata.Device.DeviceTypeKey);
                    break;

                case Slot slot:
                    if (string.IsNullOrEmpty(slot.GetName()))
                        missingFields.Add(Models.Metadata.Slot.NameKey);
                    break;

                case DatItems.Formats.SoftwareList softwarelist:
                    if (string.IsNullOrEmpty(softwarelist.GetStringFieldValue(Models.Metadata.SoftwareList.TagKey)))
                        missingFields.Add(Models.Metadata.SoftwareList.TagKey);
                    if (string.IsNullOrEmpty(softwarelist.GetName()))
                        missingFields.Add(Models.Metadata.SoftwareList.NameKey);
                    if (softwarelist.GetStringFieldValue(Models.Metadata.SoftwareList.StatusKey).AsSoftwareListStatus() == SoftwareListStatus.None)
                        missingFields.Add(Models.Metadata.SoftwareList.StatusKey);
                    break;

                case RamOption ramoption:
                    if (string.IsNullOrEmpty(ramoption.GetName()))
                        missingFields.Add(Models.Metadata.RamOption.NameKey);
                    break;
            }

            return missingFields;
        }
    }
}
