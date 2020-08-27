using System;
using System.IO;
using System.Reflection;

namespace SabreTools.Library.Data
{
    /// <summary>
    /// Constants that are used throughout the library
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// The current toolset version to be used by all child applications
        /// </summary>
        ///public readonly static string Version = $"v1.0.2";
        public readonly static string Version = $"v1.0.2-{File.GetCreationTime(Assembly.GetExecutingAssembly().Location):yyyy-MM-dd HH:mm:ss}";
        public const int HeaderHeight = 3;

        #region 0-byte file constants

        public const long SizeZero = 0;
        public const string CRCZero = "00000000";
        public static readonly byte[] CRCZeroBytes =        { 0x00, 0x00, 0x00, 0x00 };
        public const string MD5Zero = "d41d8cd98f00b204e9800998ecf8427e";
        public static readonly byte[] MD5ZeroBytes =        { 0xd4, 0x1d, 0x8c, 0xd9,
                                                              0x8f, 0x00, 0xb2, 0x04,
                                                              0xe9, 0x80, 0x09, 0x98,
                                                              0xec, 0xf8, 0x42, 0x7e };
#if NET_FRAMEWORK
        public const string RIPEMD160Zero = "9c1185a5c5e9fc54612808977ee8f548b2258d31";
        public static readonly byte[] RIPEMD160ZeroBytes =  { 0x9c, 0x11, 0x85, 0xa5,
                                                              0xc5, 0xe9, 0xfc, 0x54,
                                                              0x61, 0x28, 0x08, 0x97,
                                                              0x7e, 0xe8, 0xf5, 0x48,
                                                              0xb2, 0x25, 0x8d, 0x31 };
#endif
        public const string SHA1Zero = "da39a3ee5e6b4b0d3255bfef95601890afd80709";
        public static readonly byte[] SHA1ZeroBytes =       { 0xda, 0x39, 0xa3, 0xee,
                                                              0x5e, 0x6b, 0x4b, 0x0d,
                                                              0x32, 0x55, 0xbf, 0xef,
                                                              0x95, 0x60, 0x18, 0x90,
                                                              0xaf, 0xd8, 0x07, 0x09 };
        public const string SHA256Zero = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";
        public static readonly byte[] SHA256ZeroBytes =     { 0xba, 0x78, 0x16, 0xbf,
                                                              0x8f, 0x01, 0xcf, 0xea,
                                                              0x41, 0x41, 0x40, 0xde,
                                                              0x5d, 0xae, 0x22, 0x23,
                                                              0xb0, 0x03, 0x61, 0xa3,
                                                              0x96, 0x17, 0x7a, 0x9c,
                                                              0xb4, 0x10, 0xff, 0x61,
                                                              0xf2, 0x00, 0x15, 0xad };
        public const string SHA384Zero = "cb00753f45a35e8bb5a03d699ac65007272c32ab0eded1631a8b605a43ff5bed8086072ba1e7cc2358baeca134c825a7";
        public static readonly byte[] SHA384ZeroBytes =     { 0xcb, 0x00, 0x75, 0x3f,
                                                              0x45, 0xa3, 0x5e, 0x8b,
                                                              0xb5, 0xa0, 0x3d, 0x69,
                                                              0x9a, 0xc6, 0x50, 0x07,
                                                              0x27, 0x2c, 0x32, 0xab,
                                                              0x0e, 0xde, 0xd1, 0x63,
                                                              0x1a, 0x8b, 0x60, 0x5a,
                                                              0x43, 0xff, 0x5b, 0xed,
                                                              0x80, 0x86, 0x07, 0x2b,
                                                              0xa1, 0xe7, 0xcc, 0x23,
                                                              0x58, 0xba, 0xec, 0xa1,
                                                              0x34, 0xc8, 0x25, 0xa7 };
        public const string SHA512Zero = "ddaf35a193617abacc417349ae20413112e6fa4e89a97ea20a9eeee64b55d39a2192992a274fc1a836ba3c23a3feebbd454d4423643ce80e2a9ac94fa54ca49f";
        public static readonly byte[] SHA512ZeroBytes =     { 0xdd, 0xaf, 0x35, 0xa1,
                                                              0x93, 0x61, 0x7a, 0xba,
                                                              0xcc, 0x41, 0x73, 0x49,
                                                              0xae, 0x20, 0x41, 0x31,
                                                              0x12, 0xe6, 0xfa, 0x4e,
                                                              0x89, 0xa9, 0x7e, 0xa2,
                                                              0x0a, 0x9e, 0xee, 0xe6,
                                                              0x4b, 0x55, 0xd3, 0x9a,
                                                              0x21, 0x92, 0x99, 0x2a,
                                                              0x27, 0x4f, 0xc1, 0xa8,
                                                              0x36, 0xba, 0x3c, 0x23,
                                                              0xa3, 0xfe, 0xeb, 0xbd,
                                                              0x45, 0x4d, 0x44, 0x23,
                                                              0x64, 0x3c, 0xe8, 0x0e,
                                                              0x2a, 0x9a, 0xc9, 0x4f,
                                                              0xa5, 0x4c, 0xa4, 0x9f };

        #endregion

        #region Byte (1000-based) size comparisons

        public const long KiloByte = 1000;
        public readonly static long MegaByte = (long)Math.Pow(KiloByte, 2);
        public readonly static long GigaByte = (long)Math.Pow(KiloByte, 3);
        public readonly static long TeraByte = (long)Math.Pow(KiloByte, 4);
        public readonly static long PetaByte = (long)Math.Pow(KiloByte, 5);
        public readonly static long ExaByte = (long)Math.Pow(KiloByte, 6);
        public readonly static long ZettaByte = (long)Math.Pow(KiloByte, 7);
        public readonly static long YottaByte = (long)Math.Pow(KiloByte, 8);

        #endregion

        #region Byte (1024-based) size comparisons

        public const long KibiByte = 1024;
        public readonly static long MibiByte = (long)Math.Pow(KibiByte, 2);
        public readonly static long GibiByte = (long)Math.Pow(KibiByte, 3);
        public readonly static long TibiByte = (long)Math.Pow(KibiByte, 4);
        public readonly static long PibiByte = (long)Math.Pow(KibiByte, 5);
        public readonly static long ExiByte = (long)Math.Pow(KibiByte, 6);
        public readonly static long ZittiByte = (long)Math.Pow(KibiByte, 7);
        public readonly static long YittiByte = (long)Math.Pow(KibiByte, 8);

        #endregion

        #region Database schema

        public const string HeadererDbSchema = "Headerer";
        public static string HeadererFileName = Path.Combine(Globals.ExeDir, "Headerer.sqlite");
        public static string HeadererConnectionString = $"Data Source={HeadererFileName};Version = 3;";

        #endregion

        #region DTDs

        public const string LogiqxDTD = @"<!--
   ROM Management Datafile - DTD

   For further information, see: http://www.logiqx.com/

   This DTD module is identified by the PUBLIC and SYSTEM identifiers:

   PUBLIC string.Empty -//Logiqx//DTD ROM Management Datafile//ENstring.Empty
   SYSTEM string.Emptyhttp://www.logiqx.com/Dats/datafile.dtdstring.Empty

   $Revision: 1.5 $
   $Date: 2008/10/28 21:39:16 $

-->

<!ELEMENT datafile(header?, game*, machine*)>
    <!ATTLIST datafile build CDATA #IMPLIED>
    <!ATTLIST datafile debug (yes|no) string.Emptynostring.Empty>
    <!ELEMENT header(name, description, category?, version, date?, author, email?, homepage?, url?, comment?, clrmamepro?, romcenter?)>
        <!ELEMENT name(#PCDATA)>
        <!ELEMENT description (#PCDATA)>
        <!ELEMENT category (#PCDATA)>
        <!ELEMENT version (#PCDATA)>
        <!ELEMENT date (#PCDATA)>
        <!ELEMENT author (#PCDATA)>
        <!ELEMENT email (#PCDATA)>
        <!ELEMENT homepage (#PCDATA)>
        <!ELEMENT url (#PCDATA)>
        <!ELEMENT comment (#PCDATA)>
        <!ELEMENT clrmamepro EMPTY>
            <!ATTLIST clrmamepro header CDATA #IMPLIED>
            <!ATTLIST clrmamepro forcemerging (none|split|full) string.Emptysplitstring.Empty>
            <!ATTLIST clrmamepro forcenodump(obsolete|required|ignore) string.Emptyobsoletestring.Empty>
            <!ATTLIST clrmamepro forcepacking(zip|unzip) string.Emptyzipstring.Empty>
        <!ELEMENT romcenter EMPTY>
            <!ATTLIST romcenter plugin CDATA #IMPLIED>
            <!ATTLIST romcenter rommode (merged|split|unmerged) string.Emptysplitstring.Empty>
            <!ATTLIST romcenter biosmode(merged|split|unmerged) string.Emptysplitstring.Empty>
            <!ATTLIST romcenter samplemode(merged|unmerged) string.Emptymergedstring.Empty>
            <!ATTLIST romcenter lockrommode(yes|no) string.Emptynostring.Empty>
            <!ATTLIST romcenter lockbiosmode(yes|no) string.Emptynostring.Empty>
            <!ATTLIST romcenter locksamplemode(yes|no) string.Emptynostring.Empty>
    <!ELEMENT game(comment*, description, year?, manufacturer?, release*, biosset*, rom*, disk*, sample*, archive*)>
        <!ATTLIST game name CDATA #REQUIRED>
        <!ATTLIST game sourcefile CDATA #IMPLIED>
        <!ATTLIST game isbios (yes|no) string.Emptynostring.Empty>
        <!ATTLIST game cloneof CDATA #IMPLIED>
        <!ATTLIST game romof CDATA #IMPLIED>
        <!ATTLIST game sampleof CDATA #IMPLIED>
        <!ATTLIST game board CDATA #IMPLIED>
        <!ATTLIST game rebuildto CDATA #IMPLIED>
        <!ELEMENT year (#PCDATA)>
        <!ELEMENT manufacturer (#PCDATA)>
        <!ELEMENT release EMPTY>
            <!ATTLIST release name CDATA #REQUIRED>
            <!ATTLIST release region CDATA #REQUIRED>
            <!ATTLIST release language CDATA #IMPLIED>
            <!ATTLIST release date CDATA #IMPLIED>
            <!ATTLIST release default (yes|no) string.Emptynostring.Empty>
        <!ELEMENT biosset EMPTY>
            <!ATTLIST biosset name CDATA #REQUIRED>
            <!ATTLIST biosset description CDATA #REQUIRED>
            <!ATTLIST biosset default (yes|no) string.Emptynostring.Empty>
        <!ELEMENT rom EMPTY>
            <!ATTLIST rom name CDATA #REQUIRED>
            <!ATTLIST rom size CDATA #REQUIRED>
            <!ATTLIST rom crc CDATA #IMPLIED>
            <!ATTLIST rom md5 CDATA #IMPLIED>
            <!ATTLIST rom ripemd160 CDATA #IMPLIED>
            <!ATTLIST rom sha1 CDATA #IMPLIED>
            <!ATTLIST rom sha256 CDATA #IMPLIED>
            <!ATTLIST rom sha384 CDATA #IMPLIED>
            <!ATTLIST rom sha512 CDATA #IMPLIED>
            <!ATTLIST rom merge CDATA #IMPLIED>
            <!ATTLIST rom status (baddump|nodump|good|verified) string.Emptygoodstring.Empty>
            <!ATTLIST rom date CDATA #IMPLIED>
        <!ELEMENT disk EMPTY>
            <!ATTLIST disk name CDATA #REQUIRED>
            <!ATTLIST disk md5 CDATA #IMPLIED>
            <!ATTLIST disk ripemd160 CDATA #IMPLIED>
            <!ATTLIST disk sha1 CDATA #IMPLIED>
            <!ATTLIST disk sha256 CDATA #IMPLIED>
            <!ATTLIST disk sha384 CDATA #IMPLIED>
            <!ATTLIST disk sha512 CDATA #IMPLIED>
            <!ATTLIST disk merge CDATA #IMPLIED>
            <!ATTLIST disk status (baddump|nodump|good|verified) string.Emptygoodstring.Empty>
        <!ELEMENT sample EMPTY>
            <!ATTLIST sample name CDATA #REQUIRED>
        <!ELEMENT archive EMPTY>
            <!ATTLIST archive name CDATA #REQUIRED>
    <!ELEMENT machine (comment*, description, year?, manufacturer?, release*, biosset*, rom*, disk*, sample*, archive*)>
        <!ATTLIST machine name CDATA #REQUIRED>
        <!ATTLIST machine sourcefile CDATA #IMPLIED>
        <!ATTLIST machine isbios (yes|no) string.Emptynostring.Empty>
        <!ATTLIST machine cloneof CDATA #IMPLIED>
        <!ATTLIST machine romof CDATA #IMPLIED>
        <!ATTLIST machine sampleof CDATA #IMPLIED>
        <!ATTLIST machine board CDATA #IMPLIED>
        <!ATTLIST machine rebuildto CDATA #IMPLIED>
";
        public const string MAMEDTD = @"<!ELEMENT mame (machine+)>
    <!ATTLIST mame build CDATA #IMPLIED>
    <!ATTLIST mame debug (yes|no) string.Emptynostring.Empty>
    <!ATTLIST mame mameconfig CDATA #REQUIRED>
    <!ELEMENT machine (description, year?, manufacturer?, biosset*, rom*, disk*, device_ref*, sample*, chip*, display*, sound?, input?, dipswitch*, configuration*, port*, adjuster*, driver?, feature*, device*, slot*, softwarelist*, ramoption*)>
        <!ATTLIST machine name CDATA #REQUIRED>
        <!ATTLIST machine sourcefile CDATA #IMPLIED>
        <!ATTLIST machine isbios (yes|no) string.Emptynostring.Empty>
        <!ATTLIST machine isdevice (yes|no) string.Emptynostring.Empty>
        <!ATTLIST machine ismechanical (yes|no) string.Emptynostring.Empty>
        <!ATTLIST machine runnable (yes|no) string.Emptyyesstring.Empty>
        <!ATTLIST machine cloneof CDATA #IMPLIED>
        <!ATTLIST machine romof CDATA #IMPLIED>
        <!ATTLIST machine sampleof CDATA #IMPLIED>
        <!ELEMENT description (#PCDATA)>
        <!ELEMENT year (#PCDATA)>
        <!ELEMENT manufacturer (#PCDATA)>
        <!ELEMENT biosset EMPTY>
            <!ATTLIST biosset name CDATA #REQUIRED>
            <!ATTLIST biosset description CDATA #REQUIRED>
            <!ATTLIST biosset default (yes|no) string.Emptynostring.Empty>
        <!ELEMENT rom EMPTY>
            <!ATTLIST rom name CDATA #REQUIRED>
            <!ATTLIST rom bios CDATA #IMPLIED>
            <!ATTLIST rom size CDATA #REQUIRED>
            <!ATTLIST rom crc CDATA #IMPLIED>
            <!ATTLIST rom md5 CDATA #IMPLIED>
            <!ATTLIST rom ripemd160 CDATA #IMPLIED>
            <!ATTLIST rom sha1 CDATA #IMPLIED>
            <!ATTLIST rom sha256 CDATA #IMPLIED>
            <!ATTLIST rom sha384 CDATA #IMPLIED>
            <!ATTLIST rom sha512 CDATA #IMPLIED>
            <!ATTLIST rom merge CDATA #IMPLIED>
            <!ATTLIST rom region CDATA #IMPLIED>
            <!ATTLIST rom offset CDATA #IMPLIED>
            <!ATTLIST rom status (baddump|nodump|good) string.Emptygoodstring.Empty>
            <!ATTLIST rom optional (yes|no) string.Emptynostring.Empty>
        <!ELEMENT disk EMPTY>
            <!ATTLIST disk name CDATA #REQUIRED>
            <!ATTLIST disk md5 CDATA #IMPLIED>
            <!ATTLIST disk ripemd160 CDATA #IMPLIED>
            <!ATTLIST disk sha1 CDATA #IMPLIED>
            <!ATTLIST disk sha256 CDATA #IMPLIED>
            <!ATTLIST disk sha384 CDATA #IMPLIED>
            <!ATTLIST disk sha512 CDATA #IMPLIED>
            <!ATTLIST disk merge CDATA #IMPLIED>
            <!ATTLIST disk region CDATA #IMPLIED>
            <!ATTLIST disk index CDATA #IMPLIED>
            <!ATTLIST disk writable (yes|no) string.Emptynostring.Empty>
            <!ATTLIST disk status (baddump|nodump|good) string.Emptygoodstring.Empty>
            <!ATTLIST disk optional (yes|no) string.Emptynostring.Empty>
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
            <!ATTLIST display flipx (yes|no) string.Emptynostring.Empty>
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
            <!ATTLIST input service (yes|no) string.Emptynostring.Empty>
            <!ATTLIST input tilt (yes|no) string.Emptynostring.Empty>
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
                <!ATTLIST control reverse (yes|no) string.Emptynostring.Empty>
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
                <!ATTLIST diplocation inverted (yes|no) string.Emptynostring.Empty>
            <!ELEMENT dipvalue (condition?)>
                <!ATTLIST dipvalue name CDATA #REQUIRED>
                <!ATTLIST dipvalue value CDATA #REQUIRED>
                <!ATTLIST dipvalue default (yes|no) string.Emptynostring.Empty>
        <!ELEMENT configuration (condition?, conflocation*, confsetting*)>
            <!ATTLIST configuration name CDATA #REQUIRED>
            <!ATTLIST configuration tag CDATA #REQUIRED>
            <!ATTLIST configuration mask CDATA #REQUIRED>
            <!ELEMENT conflocation EMPTY>
                <!ATTLIST conflocation name CDATA #REQUIRED>
                <!ATTLIST conflocation number CDATA #REQUIRED>
                <!ATTLIST conflocation inverted (yes|no) string.Emptynostring.Empty>
            <!ELEMENT confsetting (condition?)>
                <!ATTLIST confsetting name CDATA #REQUIRED>
                <!ATTLIST confsetting value CDATA #REQUIRED>
                <!ATTLIST confsetting default (yes|no) string.Emptynostring.Empty>
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
        <!ELEMENT feature EMPTY>
            <!ATTLIST feature type (protection|palette|graphics|sound|controls|keyboard|mouse|microphone|camera|disk|printer|lan|wan|timing) #REQUIRED>
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
                <!ATTLIST slotoption default (yes|no) string.Emptynostring.Empty>
        <!ELEMENT softwarelist EMPTY>
            <!ATTLIST softwarelist name CDATA #REQUIRED>
            <!ATTLIST softwarelist status (original|compatible) #REQUIRED>
            <!ATTLIST softwarelist filter CDATA #IMPLIED>
        <!ELEMENT ramoption (#PCDATA)>
            <!ATTLIST ramoption default CDATA #IMPLIED>
";
        public const string OpenMSXDTD = @"<!ELEMENT softwaredb (person*)>
<!ELEMENT software (title, genmsxid?, system, company,year,country,dump)>
<!ELEMENT title (#PCDATA)>
<!ELEMENT genmsxid (#PCDATA)>
<!ELEMENT system (#PCDATA)>
<!ELEMENT company (#PCDATA)>
<!ELEMENT year (#PCDATA)>
<!ELEMENT country (#PCDATA)>
<!ELEMENT dump (#PCDATA)>
";
        public const string SoftwareListDTD = @"<!ELEMENT softwarelist (software+)>
    <!ATTLIST softwarelist name CDATA #REQUIRED>
    <!ATTLIST softwarelist description CDATA #IMPLIED>
    <!ELEMENT software (description, year, publisher, info*, sharedfeat*, part*)>
        <!ATTLIST software name CDATA #REQUIRED>
        <!ATTLIST software cloneof CDATA #IMPLIED>
        <!ATTLIST software supported (yes|partial|no) string.Emptyyesstring.Empty>
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
                <!ATTLIST dataarea width (8|16|32|64) string.Empty8string.Empty>
                <!ATTLIST dataarea endianness (big|little) string.Emptylittlestring.Empty>
                <!ELEMENT rom EMPTY>
                    <!ATTLIST rom name CDATA #IMPLIED>
                    <!ATTLIST rom size CDATA #IMPLIED>
                    <!ATTLIST rom crc CDATA #IMPLIED>
                    <!ATTLIST rom md5 CDATA #IMPLIED>
                    <!ATTLIST rom ripemd160 CDATA #IMPLIED>
                    <!ATTLIST rom sha1 CDATA #IMPLIED>
                    <!ATTLIST rom sha256 CDATA #IMPLIED>
                    <!ATTLIST rom sha384 CDATA #IMPLIED>
                    <!ATTLIST rom sha512 CDATA #IMPLIED>
                    <!ATTLIST rom offset CDATA #IMPLIED>
                    <!ATTLIST rom value CDATA #IMPLIED>
                    <!ATTLIST rom status (baddump|nodump|good) string.Emptygoodstring.Empty>
                    <!ATTLIST rom loadflag (load16_byte|load16_word|load16_word_swap|load32_byte|load32_word|load32_word_swap|load32_dword|load64_word|load64_word_swap|reload|fill|continue|reload_plain|ignore) #IMPLIED>
            <!ELEMENT diskarea (disk*)>
                <!ATTLIST diskarea name CDATA #REQUIRED>
                <!ELEMENT disk EMPTY>
                    <!ATTLIST disk name CDATA #REQUIRED>
                    <!ATTLIST disk md5 CDATA #IMPLIED>
                    <!ATTLIST disk ripemd160 CDATA #IMPLIED>
                    <!ATTLIST disk sha1 CDATA #IMPLIED>
                    <!ATTLIST disk sha256 CDATA #IMPLIED>
                    <!ATTLIST disk sha384 CDATA #IMPLIED>
                    <!ATTLIST disk sha512 CDATA #IMPLIED>
                    <!ATTLIST disk status (baddump|nodump|good) string.Emptygoodstring.Empty>
                    <!ATTLIST disk writeable (yes|no) string.Emptynostring.Empty>
            <!ELEMENT dipswitch (dipvalue*)>
                <!ATTLIST dipswitch name CDATA #REQUIRED>
                <!ATTLIST dipswitch tag CDATA #REQUIRED>
                <!ATTLIST dipswitch mask CDATA #REQUIRED>
                <!ELEMENT dipvalue EMPTY>
                    <!ATTLIST dipvalue name CDATA #REQUIRED>
                    <!ATTLIST dipvalue value CDATA #REQUIRED>
                    <!ATTLIST dipvalue default (yes|no) string.Emptynostring.Empty>
";

        #endregion

        #region Hash string length constants

        public const int CRCLength = 8;
        public const int MD5Length = 32;
#if NET_FRAMEWORK
        public const int RIPEMD160Length = 40;
#endif
        public const int SHA1Length = 40;
        public const int SHA256Length = 64;
        public const int SHA384Length = 96;
        public const int SHA512Length = 128;

        #endregion

        #region Magic numbers

        public static readonly byte[] SevenZipSignature = { 0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c };
        public static readonly byte[] A7800SignatureV1 = { 0x41, 0x54, 0x41, 0x52, 0x49, 0x37, 0x38, 0x30, 0x30 }; // Offset 0x01
        public static readonly byte[] A7800SignatureV2 =            { 0x41, 0x43, 0x54, 0x55, 0x41, 0x4c, 0x20, 0x43, 0x41, 0x52, 0x54, 0x20, 0x44, 0x41,
                                                                    0x54, 0x41, 0x20, 0x53, 0x54, 0x41, 0x52, 0x54, 0x53, 0x20, 0x48, 0x45, 0x52, 0x45 }; // Offset 0x64
        public static readonly byte[] AaruFormatSignature = { 0x41, 0x41, 0x52, 0x55, 0x46, 0x52, 0x4d, 0x54 };
        public static readonly byte[] BZ2Signature = { 0x42, 0x5a, 0x68 };
        public static readonly byte[] CabinetSignature = { 0x4d, 0x53, 0x43, 0x46 };
        public static readonly byte[] CHDSignature = { 0x4d, 0x43, 0x6f, 0x6d, 0x70, 0x72, 0x48, 0x44 };
        public static readonly byte[] ELFSignature = { 0x7f, 0x45, 0x4c, 0x46 };
        public static readonly byte[] FDSSignatureV1 = { 0x46, 0x44, 0x53, 0x1a, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static readonly byte[] FDSSignatureV2 = { 0x46, 0x44, 0x53, 0x1a, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static readonly byte[] FDSSignatureV3 = { 0x46, 0x44, 0x53, 0x1a, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static readonly byte[] FDSSignatureV4 = { 0x46, 0x44, 0x53, 0x1a, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        public static readonly byte[] FreeArcSignature = { 0x41, 0x72, 0x43, 0x01 };
        public static readonly byte[] GzSignature = { 0x1f, 0x8b, 0x08 };
        public static readonly byte[] LRZipSignature = { 0x4c, 0x52, 0x5a, 0x49 };
        public static readonly byte[] LynxSignatureV1 = { 0x4c, 0x59, 0x4f, 0x58 };
        public static readonly byte[] LynxSignatureV2 = { 0x42, 0x53, 0x39 }; // Offset 0x06
        public static readonly byte[] LZ4Signature = { 0x18, 0x4d, 0x22, 0x04 };
        public static readonly byte[] LZ4SkippableMinSignature = { 0x18, 0x4d, 0x22, 0x04 };
        public static readonly byte[] LZ4SkippableMaxSignature = { 0x18, 0x4d, 0x2a, 0x5f };
        public static readonly byte[] N64Signature = { 0x40, 0x12, 0x37, 0x80 };
        public static readonly byte[] NESSignature = { 0x4e, 0x45, 0x53, 0x1a };
        public static readonly byte[] PCESignature = { 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xaa, 0xbb, 0x02 };
        public static readonly byte[] PESignature = { 0x4d, 0x5a };
        public static readonly byte[] PSIDSignatureV1 = { 0x50, 0x53, 0x49, 0x44, 0x00, 0x01, 0x00, 0x76 };
        public static readonly byte[] PSIDSignatureV2 = { 0x50, 0x53, 0x49, 0x44, 0x00, 0x02, 0x00, 0x7c };
        public static readonly byte[] PSIDSignatureV3 = { 0x50, 0x53, 0x49, 0x44, 0x00, 0x03, 0x00, 0x7c };
        public static readonly byte[] RarSignature = { 0x52, 0x61, 0x72, 0x21, 0x1a, 0x07, 0x00 };
        public static readonly byte[] RarFiveSignature = { 0x52, 0x61, 0x72, 0x21, 0x1a, 0x07, 0x01, 0x00 };
        public static readonly byte[] SMCSignature = { 0xaa, 0xbb, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00 }; // Offset 0x16
        public static readonly byte[] SPCSignature = { 0x53, 0x4e, 0x45, 0x53, 0x2d, 0x53, 0x50, 0x43 };
        public static readonly byte[] TarSignature = { 0x75, 0x73, 0x74, 0x61, 0x72, 0x20, 0x20, 0x00 };
        public static readonly byte[] TarZeroSignature = { 0x75, 0x73, 0x74, 0x61, 0x72, 0x00, 0x30, 0x30 };
        public static readonly byte[] UFOSignature = { 0x53, 0x55, 0x50, 0x45, 0x52, 0x55, 0x46, 0x4f }; // Offset 0x16
        public static readonly byte[] V64Signature = { 0x80, 0x37, 0x12, 0x40 };
        public static readonly byte[] XZSignature = { 0xfd, 0x37, 0x7a, 0x58, 0x5a, 0x00, 0x00 };
        public static readonly byte[] Z64Signature = { 0x37, 0x80, 0x40, 0x12 };
        public static readonly byte[] ZipSignature = { 0x50, 0x4b, 0x03, 0x04 };
        public static readonly byte[] ZipSignatureEmpty = { 0x50, 0x4b, 0x05, 0x06 };
        public static readonly byte[] ZipSignatureSpanned = { 0x50, 0x4b, 0x07, 0x08 };
        public static readonly byte[] ZPAQSignature = { 0x7a, 0x50, 0x51 };
        public static readonly byte[] ZstdSignature = { 0xfd, 0x2f, 0xb5 };

        #endregion

        #region Regular Expressions

        public const string XmlPattern = @"<(.*?)>(.*?)</(.*?)>";
        public const string HeaderPatternCMP = @"(^.*?) \($";
        public const string InternalPatternCMP = @"(^.*?) (\(.+\))$";
        public const string InternalPatternAttributesCMP = @"[^\s""]+|""[^""]*""";
        //public const string InternalPatternAttributesCMP = @"([^\s]*""[^""]+""[^\s]*)|[^""]?\w+[^""]?";
        public const string ItemPatternCMP = @"^\s*(\S*?) (.*)";
        public const string EndPatternCMP = @"^\s*\)\s*$";

        #endregion

        #region TorrentZip, T7z, and TGZ headers

        /* TorrentZip Header Format
            https://pkware.cachefly.net/webdocs/APPNOTE/APPNOTE_6.2.0.txt
            http://www.romvault.com/trrntzip_explained.doc

            00-03		Local file header signature (0x50, 0x4B, 0x03, 0x04) ZipSignature
            04-05		Version needed to extract (0x14, 0x00)
            06-07		General purpose bit flag (0x02, 0x00)
            08-09		Compression method (0x08, 0x00)
            0A-0B		Last mod file time (0x00, 0xBC)
            0C-0D		Last mod file date (0x98, 0x21)
        */
        public readonly static byte[] TorrentZipHeader = new byte[] { 0x50, 0x4b, 0x03, 0x04, 0x14, 0x00, 0x02, 0x00, 0x08, 0x00, 0x00, 0xbc, 0x98, 0x21 };

        /* Torrent7z Header Format
            http://cpansearch.perl.org/src/BJOERN/Compress-Deflate7-1.0/7zip/DOC/7zFormat.txt

            00-05		Local file header signature (0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C) SevenZipSignature
            06-07		ArchiveVersion (0x00, 0x03)
            The rest is unknown
        */
        public readonly static byte[] Torrent7ZipHeader = new byte[] { 0x37, 0x7a, 0xbc, 0xaf, 0x27, 0x1c, 0x00, 0x03 };
        public readonly static byte[] Torrent7ZipSignature = new byte[] { 0xa9, 0xa9, 0x9f, 0xd1, 0x57, 0x08, 0xa9, 0xd7, 0xea, 0x29, 0x64, 0xb2,
            0x36, 0x1b, 0x83, 0x52, 0x33, 0x00, 0x74, 0x6f, 0x72, 0x72, 0x65, 0x6e, 0x74, 0x37, 0x7a, 0x5f, 0x30, 0x2e, 0x39, 0x62, 0x65, 0x74, 0x61 };

        /* (Torrent)GZ Header Format
            https://tools.ietf.org/html/rfc1952

            00-01		Identification (0x1F, 0x8B) GzSignature
            02			Compression Method (0-7 reserved, 8 deflate; 0x08)
            03			Flags (0 FTEXT, 1 FHCRC, 2 FEXTRA, 3 FNAME, 4 FCOMMENT, 5 reserved, 6 reserved, 7 reserved; 0x04)
            04-07		Modification time (Unix format; 0x00, 0x00, 0x00, 0x00)
            08			Extra Flags (2 maximum compression, 4 fastest algorithm; 0x00)
            09			OS (See list on https://tools.ietf.org/html/rfc1952; 0x00)
            0A-0B		Length of extra field (mirrored; 0x1C, 0x00)
            0C-27		Extra field
                0C-1B	MD5 Hash
                1C-1F	CRC hash
                20-27	Int64 size (mirrored)
        */
        public readonly static byte[] TorrentGZHeader = new byte[] { 0x1f, 0x8b, 0x08, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1c, 0x00 };

        /* (Torrent)XZ Header Format
            https://tukaani.org/xz/xz-file-format.txt

            00-05		Identification (0xFD, '7', 'z', 'X', 'Z', 0x00) XzSignature
            06			Flags (0x01 - CRC32, 0x04 - CRC64, 0x0A - SHA-256)
            07-0A		Flags CRC32 (uint, little-endian)
        */
        public readonly static byte[] TorrentXZHeader = new byte[] { 0xfd, 0x37, 0x7a, 0x58, 0x5a, 0x00, 0x01, 0x69, 0x22, 0xde, 0x36 };

        #endregion

        #region ZIP internal signatures

        public const uint LocalFileHeaderSignature = 0x04034b50;
        public const uint EndOfLocalFileHeaderSignature = 0x08074b50;
        public const uint CentralDirectoryHeaderSignature = 0x02014b50;
        public const uint EndOfCentralDirSignature = 0x06054b50;
        public const uint Zip64EndOfCentralDirSignature = 0x06064b50;
        public const uint Zip64EndOfCentralDirectoryLocator = 0x07064b50;
        public const uint TorrentZipFileDateTime = 0x2198BC00;

        #endregion
    }
}
