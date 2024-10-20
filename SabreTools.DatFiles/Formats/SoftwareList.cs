﻿using System.Collections.Generic;
using System.Linq;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents parsing and writing of a SoftwareList
    /// </summary>
    internal sealed class SoftwareList : SerializableDatFile<Models.SoftwareList.SoftwareList, Serialization.Deserializers.SoftwareList, Serialization.Serializers.SoftwareList, Serialization.CrossModel.SoftwareList>
    {
        /// <summary>
        /// DTD for original MAME Software List DATs
        /// </summary>
        /// <remarks>
        /// TODO: See if there's an updated DTD and then check for required fields
        /// </remarks>
        private const string SoftwareListDTD = @"<!ELEMENT softwarelist (notes?, software+)>
	<!ATTLIST softwarelist name CDATA #REQUIRED>
	<!ATTLIST softwarelist description CDATA #IMPLIED>
	<!ELEMENT notes (#PCDATA)>
	<!ELEMENT software (description, year, publisher, notes?, info*, sharedfeat*, part*)>
		<!ATTLIST software name CDATA #REQUIRED>
		<!ATTLIST software cloneof CDATA #IMPLIED>
		<!ATTLIST software supported (yes|partial|no) ""yes"">
		<!ELEMENT description (#PCDATA)>
		<!ELEMENT year (#PCDATA)>
		<!ELEMENT publisher (#PCDATA)>
		<!ELEMENT info EMPTY>
			<!ATTLIST info name CDATA #REQUIRED>
			<!ATTLIST info value CDATA #IMPLIED>
		<!ELEMENT sharedfeat EMPTY>
			<!ATTLIST sharedfeat name CDATA #REQUIRED>
			<!ATTLIST sharedfeat value CDATA #IMPLIED>
		<!ELEMENT part (feature*, dataarea*, diskarea*, dipswitch*)>
			<!ATTLIST part name CDATA #REQUIRED>
			<!ATTLIST part interface CDATA #REQUIRED>
			<!-- feature is used to store things like pcb-type, mapper type, etc. Specific values depend on the system. -->
			<!ELEMENT feature EMPTY>
				<!ATTLIST feature name CDATA #REQUIRED>
				<!ATTLIST feature value CDATA #IMPLIED>
			<!ELEMENT dataarea (rom*)>
				<!ATTLIST dataarea name CDATA #REQUIRED>
				<!ATTLIST dataarea size CDATA #REQUIRED>
				<!ATTLIST dataarea width (8|16|32|64) ""8"">
				<!ATTLIST dataarea endianness (big|little) ""little"">
				<!ELEMENT rom EMPTY>
					<!ATTLIST rom name CDATA #IMPLIED>
					<!ATTLIST rom size CDATA #IMPLIED>
					<!ATTLIST rom crc CDATA #IMPLIED>
					<!ATTLIST rom sha1 CDATA #IMPLIED>
					<!ATTLIST rom offset CDATA #IMPLIED>
					<!ATTLIST rom value CDATA #IMPLIED>
					<!ATTLIST rom status (baddump|nodump|good) ""good"">
					<!ATTLIST rom loadflag (load16_byte|load16_word|load16_word_swap|load32_byte|load32_word|load32_word_swap|load32_dword|load64_word|load64_word_swap|reload|fill|continue|reload_plain|ignore) #IMPLIED>
			<!ELEMENT diskarea (disk*)>
				<!ATTLIST diskarea name CDATA #REQUIRED>
				<!ELEMENT disk EMPTY>
					<!ATTLIST disk name CDATA #REQUIRED>
					<!ATTLIST disk sha1 CDATA #IMPLIED>
					<!ATTLIST disk status (baddump|nodump|good) ""good"">
					<!ATTLIST disk writeable (yes|no) ""no"">
			<!ELEMENT dipswitch (dipvalue*)>
				<!ATTLIST dipswitch name CDATA #REQUIRED>
				<!ATTLIST dipswitch tag CDATA #REQUIRED>
				<!ATTLIST dipswitch mask CDATA #REQUIRED>
				<!ELEMENT dipvalue EMPTY>
					<!ATTLIST dipvalue name CDATA #REQUIRED>
					<!ATTLIST dipvalue value CDATA #REQUIRED>
					<!ATTLIST dipvalue default (yes|no) ""no"">
";

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public SoftwareList(DatFile? datFile)
            : base(datFile)
        {
        }

        /// <inheritdoc/>
        protected override ItemType[] GetSupportedTypes()
        {
            return
            [
                ItemType.DipSwitch,
                ItemType.Disk,
                ItemType.Info,
                ItemType.PartFeature,
                ItemType.Rom,
                ItemType.SharedFeat,
            ];
        }

        /// <inheritdoc/>
        protected override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            var missingFields = new List<string>();

            switch (datItem)
            {
                case DipSwitch dipSwitch:
                    if (!dipSwitch.PartSpecified)
                    {
                        missingFields.Add(Models.Metadata.Part.NameKey);
                        missingFields.Add(Models.Metadata.Part.InterfaceKey);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(dipSwitch.GetFieldValue<Part?>(DipSwitch.PartKey)!.GetName()))
                            missingFields.Add(Models.Metadata.Part.NameKey);
                        if (string.IsNullOrEmpty(dipSwitch.GetFieldValue<Part?>(DipSwitch.PartKey)!.GetStringFieldValue(Models.Metadata.Part.InterfaceKey)))
                            missingFields.Add(Models.Metadata.Part.InterfaceKey);
                    }
                    if (string.IsNullOrEmpty(dipSwitch.GetName()))
                        missingFields.Add(Models.Metadata.DipSwitch.NameKey);
                    if (string.IsNullOrEmpty(dipSwitch.GetStringFieldValue(Models.Metadata.DipSwitch.TagKey)))
                        missingFields.Add(Models.Metadata.DipSwitch.TagKey);
                    if (string.IsNullOrEmpty(dipSwitch.GetStringFieldValue(Models.Metadata.DipSwitch.MaskKey)))
                        missingFields.Add(Models.Metadata.DipSwitch.MaskKey);
                    if (dipSwitch.ValuesSpecified)
                    {
                        var dipValues = dipSwitch.GetFieldValue<DipValue[]?>(Models.Metadata.DipSwitch.DipValueKey);
                        if (dipValues!.Any(dv => string.IsNullOrEmpty(dv.GetName())))
                            missingFields.Add(Models.Metadata.DipValue.NameKey);
                        if (dipValues!.Any(dv => string.IsNullOrEmpty(dv.GetStringFieldValue(Models.Metadata.DipValue.ValueKey))))
                            missingFields.Add(Models.Metadata.DipValue.ValueKey);
                    }

                    break;

                case Disk disk:
                    if (!disk.PartSpecified)
                    {
                        missingFields.Add(Models.Metadata.Part.NameKey);
                        missingFields.Add(Models.Metadata.Part.InterfaceKey);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(disk.GetFieldValue<Part?>(Disk.PartKey)!.GetName()))
                            missingFields.Add(Models.Metadata.Part.NameKey);
                        if (string.IsNullOrEmpty(disk.GetFieldValue<Part?>(Disk.PartKey)!.GetStringFieldValue(Models.Metadata.Part.InterfaceKey)))
                            missingFields.Add(Models.Metadata.Part.InterfaceKey);
                    }
                    if (!disk.DiskAreaSpecified)
                    {
                        missingFields.Add(Models.Metadata.DiskArea.NameKey);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(disk.GetFieldValue<DiskArea?>(Disk.DiskAreaKey)!.GetName()))
                            missingFields.Add(Models.Metadata.DiskArea.NameKey);
                    }
                    if (string.IsNullOrEmpty(disk.GetName()))
                        missingFields.Add(Models.Metadata.Disk.NameKey);
                    break;

                case Info info:
                    if (string.IsNullOrEmpty(info.GetName()))
                        missingFields.Add(Models.Metadata.Info.NameKey);
                    break;

                case Rom rom:
                    if (!rom.PartSpecified)
                    {
                        missingFields.Add(Models.Metadata.Part.NameKey);
                        missingFields.Add(Models.Metadata.Part.InterfaceKey);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(rom.GetFieldValue<Part?>(Rom.PartKey)!.GetName()))
                            missingFields.Add(Models.Metadata.Part.NameKey);
                        if (string.IsNullOrEmpty(rom.GetFieldValue<Part?>(Rom.PartKey)!.GetStringFieldValue(Models.Metadata.Part.InterfaceKey)))
                            missingFields.Add(Models.Metadata.Part.InterfaceKey);
                    }
                    if (!rom.DataAreaSpecified)
                    {
                        missingFields.Add(Models.Metadata.DataArea.NameKey);
                        missingFields.Add(Models.Metadata.DataArea.SizeKey);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(rom.GetFieldValue<DataArea?>(Rom.DataAreaKey)!.GetName()))
                            missingFields.Add(Models.Metadata.DataArea.NameKey);
                        if (rom.GetFieldValue<DataArea?>(Rom.DataAreaKey)!.GetInt64FieldValue(Models.Metadata.DataArea.SizeKey) == null)
                            missingFields.Add(Models.Metadata.DataArea.SizeKey);
                    }
                    break;

                case SharedFeat sharedFeat:
                    if (string.IsNullOrEmpty(sharedFeat.GetName()))
                        missingFields.Add(Models.Metadata.SharedFeat.NameKey);
                    break;
                default:
                    // Unsupported ItemTypes should be caught already
                    return null;
            }

            return missingFields;
        }
    }
}
