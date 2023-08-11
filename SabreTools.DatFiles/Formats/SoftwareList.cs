﻿// TODO: Use softwarelist.dtd and *try* to make this write more correctly
namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents parsing and writing of a SoftwareList
    /// </summary>
    /// <remarks>
    /// TODO: Check and enforce required fields in output
    /// </remarks>
    internal partial class SoftwareList : DatFile
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
    }
}
