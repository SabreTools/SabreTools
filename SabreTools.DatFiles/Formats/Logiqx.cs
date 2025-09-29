using System;
using System.Collections.Generic;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents a Logiqx-derived DAT
    /// </summary>
    public sealed class Logiqx : SerializableDatFile<Data.Models.Logiqx.Datafile, Serialization.Readers.Logiqx, Serialization.Writers.Logiqx, Serialization.CrossModel.Logiqx>
    {
        #region Constants

        /// <summary>
        /// DTD for original Logiqx DATs
        /// </summary>
        /// <remarks>This has been edited to reflect actual current standards</remarks>
        private const string LogiqxDTD = @"<!--
   ROM Management Datafile - DTD

   For further information, see: http://www.logiqx.com/

   This DTD module is identified by the PUBLIC and SYSTEM identifiers:

   PUBLIC "" -//Logiqx//DTD ROM Management Datafile//EN""
   SYSTEM ""http://www.logiqx.com/Dats/datafile.dtd""

   $Revision: 1.5 $
   $Date: 2008/10/28 21:39:16 $

-->

<!ELEMENT datafile(header?, game*, machine*)>
    <!ATTLIST datafile build CDATA #IMPLIED>
    <!ATTLIST datafile debug (yes|no) ""no"">
    <!ELEMENT header (id?, name, description, rootdir?, type?, category?, version, date?, author, email?, homepage?, url?, comment?, clrmamepro?, romcenter?)>
        <!ELEMENT id (#PCDATA)>
        <!ELEMENT name(#PCDATA)>
        <!ELEMENT description (#PCDATA)>
        <!ELEMENT rootdir (#PCDATA)>
        <!ELEMENT type (#PCDATA)>
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
            <!ATTLIST clrmamepro forcemerging (none|split|merged|nonmerged|fullmerged|device|full) ""split"">
            <!ATTLIST clrmamepro forcenodump(obsolete|required|ignore) ""obsolete"">
            <!ATTLIST clrmamepro forcepacking(zip|unzip) ""zip"">
        <!ELEMENT romcenter EMPTY>
            <!ATTLIST romcenter plugin CDATA #IMPLIED>
            <!ATTLIST romcenter rommode (none|split|merged|unmerged|fullmerged|device|full) ""split"">
            <!ATTLIST romcenter biosmode (none|split|merged|unmerged|fullmerged|device|full) ""split"">
            <!ATTLIST romcenter samplemode (none|split|merged|unmerged|fullmerged|device|full) ""merged"">
            <!ATTLIST romcenter lockrommode(yes|no) ""no"">
            <!ATTLIST romcenter lockbiosmode(yes|no) ""no"">
            <!ATTLIST romcenter locksamplemode(yes|no) ""no"">
    <!ELEMENT game (comment*, description, year?, manufacturer?, publisher?, category?, trurip?, release*, biosset*, rom*, disk*, media*, sample*, archive*)>
        <!ATTLIST game name CDATA #REQUIRED>
        <!ATTLIST game sourcefile CDATA #IMPLIED>
        <!ATTLIST game isbios (yes|no) ""no"">
        <!ATTLIST game cloneof CDATA #IMPLIED>
        <!ATTLIST game romof CDATA #IMPLIED>
        <!ATTLIST game sampleof CDATA #IMPLIED>
        <!ATTLIST game board CDATA #IMPLIED>
        <!ATTLIST game rebuildto CDATA #IMPLIED>
        <!ATTLIST game id CDATA #IMPLIED>
        <!ATTLIST game cloneofid CDATA #IMPLIED>
        <!ATTLIST game runnable (no|partial|yes) ""no"" #IMPLIED>
        <!ELEMENT year (#PCDATA)>
        <!ELEMENT manufacturer (#PCDATA)>
        <!ELEMENT publisher (#PCDATA)>
        <!ELEMENT trurip (titleid?, publisher?, developer?, year?, genre?, subgenre?, ratings?, score?, players?, enabled?, crc?, source?, cloneof?, relatedto?)>
            <!ELEMENT titleid (#PCDATA)>
            <!ELEMENT developer (#PCDATA)>
            <!ELEMENT year (#PCDATA)>
            <!ELEMENT genre (#PCDATA)>
            <!ELEMENT subgenre (#PCDATA)>
            <!ELEMENT ratings (#PCDATA)>
            <!ELEMENT score (#PCDATA)>
            <!ELEMENT players (#PCDATA)>
            <!ELEMENT enabled (#PCDATA)>
            <!ELEMENT crc (#PCDATA)>
            <!ELEMENT source (#PCDATA)>
            <!ELEMENT cloneof (#PCDATA)>
            <!ELEMENT relatedto (#PCDATA)>
        <!ELEMENT release EMPTY>
            <!ATTLIST release name CDATA #REQUIRED>
            <!ATTLIST release region CDATA #REQUIRED>
            <!ATTLIST release language CDATA #IMPLIED>
            <!ATTLIST release date CDATA #IMPLIED>
            <!ATTLIST release default (yes|no) ""no"">
        <!ELEMENT biosset EMPTY>
            <!ATTLIST biosset name CDATA #REQUIRED>
            <!ATTLIST biosset description CDATA #REQUIRED>
            <!ATTLIST biosset default (yes|no) ""no"">
        <!ELEMENT rom EMPTY>
            <!ATTLIST rom name CDATA #REQUIRED>
            <!ATTLIST rom size CDATA #REQUIRED>
            <!ATTLIST rom crc CDATA #IMPLIED>
            <!ATTLIST rom md5 CDATA #IMPLIED>
            <!ATTLIST rom sha1 CDATA #IMPLIED>
            <!ATTLIST rom sha256 CDATA #IMPLIED>
            <!ATTLIST rom sha384 CDATA #IMPLIED>
            <!ATTLIST rom sha512 CDATA #IMPLIED>
            <!ATTLIST rom spamsum CDATA #IMPLIED>
            <!ATTLIST rom xxh3_64 CDATA #IMPLIED>
            <!ATTLIST rom xxh3_128 CDATA #IMPLIED>
            <!ATTLIST rom merge CDATA #IMPLIED>
            <!ATTLIST rom status (baddump|nodump|good|verified) ""good"">
            <!ATTLIST rom serial CDATA #IMPLIED>
            <!ATTLIST rom header CDATA #IMPLIED>
            <!ATTLIST rom date CDATA #IMPLIED>
            <!ATTLIST rom inverted CDATA #IMPLIED>
            <!ATTLIST rom mia CDATA #IMPLIED>
        <!ELEMENT disk EMPTY>
            <!ATTLIST disk name CDATA #REQUIRED>
            <!ATTLIST disk md5 CDATA #IMPLIED>
            <!ATTLIST disk sha1 CDATA #IMPLIED>
            <!ATTLIST disk merge CDATA #IMPLIED>
            <!ATTLIST disk status (baddump|nodump|good|verified) ""good"">
        <!ELEMENT media EMPTY>
            <!ATTLIST media name CDATA #REQUIRED>
            <!ATTLIST media md5 CDATA #IMPLIED>
            <!ATTLIST media sha1 CDATA #IMPLIED>
            <!ATTLIST media sha256 CDATA #IMPLIED>
            <!ATTLIST media spamsum CDATA #IMPLIED>
        <!ELEMENT sample EMPTY>
            <!ATTLIST sample name CDATA #REQUIRED>
        <!ELEMENT archive EMPTY>
            <!ATTLIST archive name CDATA #REQUIRED>
    <!ELEMENT machine (comment*, description, year?, manufacturer?, publisher?, category?, trurip?, release*, biosset*, rom*, disk*, media*, sample*, archive*)>
        <!ATTLIST game name CDATA #REQUIRED>
        <!ATTLIST game sourcefile CDATA #IMPLIED>
        <!ATTLIST game isbios (yes|no) ""no"">
        <!ATTLIST game cloneof CDATA #IMPLIED>
        <!ATTLIST game romof CDATA #IMPLIED>
        <!ATTLIST game sampleof CDATA #IMPLIED>
        <!ATTLIST game board CDATA #IMPLIED>
        <!ATTLIST game rebuildto CDATA #IMPLIED>
        <!ATTLIST game id CDATA #IMPLIED>
        <!ATTLIST game cloneofid CDATA #IMPLIED>
        <!ATTLIST game runnable (no|partial|yes) ""no"" #IMPLIED>
    <!ELEMENT dir (game*, machine*)>
        <!ATTLIST dir name CDATA #REQUIRED>
";

        /// <summary>
        /// XSD for No-Intro Logiqx-derived DATs
        /// </summary>
        private const string NoIntroXSD = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xs:schema attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""datafile"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""header"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""id"" type=""xs:int""/>
              <xs:element name=""name"" type=""xs:string""/>
              <xs:element name=""description"" type=""xs:string""/>
              <xs:element name=""version"" type=""xs:string""/>
              <xs:element name=""author"" type=""xs:string""/>
              <xs:element name=""homepage"" type=""xs:string""/>
              <xs:element name=""url"" type=""xs:string""/>
              <xs:element name=""clrmamepro"">
                <xs:complexType>
                  <xs:attribute name=""forcenodump"" default=""obsolete"" use=""optional"">
                    <xs:simpleType>
                      <xs:restriction base=""xs:token"">
                        <xs:enumeration value=""obsolete""/>
                        <xs:enumeration value=""required""/>
                        <xs:enumeration value=""ignore""/>
                      </xs:restriction>
                    </xs:simpleType>
                  </xs:attribute>
                  <xs:attribute name=""header"" type=""xs:string"" use=""optional""/>
                </xs:complexType>
              </xs:element>
              <xs:element name=""romcenter"" minOccurs=""0"">
                <xs:complexType>
                  <xs:attribute name=""plugin"" type=""xs:string"" use=""optional""/>
                </xs:complexType>
              </xs:element>
            </xs:sequence>
          </xs:complexType>
        </xs:element>
        <xs:element maxOccurs=""unbounded"" name=""game"">
          <xs:complexType>
            <xs:sequence>
              <xs:element name=""description"" type=""xs:string""/>
              <xs:element name=""rom"">
                <xs:complexType>
                  <xs:attribute name=""name"" type=""xs:string"" use=""required""/>
                  <xs:attribute name=""size"" type=""xs:unsignedInt"" use=""required""/>
                  <xs:attribute name=""crc"" type=""xs:string"" use=""required""/>
                  <xs:attribute name=""md5"" type=""xs:string"" use=""required""/>
                  <xs:attribute name=""sha1"" type=""xs:string"" use=""required""/>
                  <xs:attribute name=""sha256"" type=""xs:string"" use=""optional""/>
                  <xs:attribute name=""status"" type=""xs:string"" use=""optional""/>
                  <xs:attribute name=""serial"" type=""xs:string"" use=""optional""/>
                  <xs:attribute name=""header"" type=""xs:string"" use=""optional""/>
                </xs:complexType>
              </xs:element>
            </xs:sequence>
            <xs:attribute name=""name"" type=""xs:string"" use=""required""/>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>
";

        #endregion

        #region Fields

        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
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
            ];

        /// <summary>
        /// Indicates if game should be used instead of machine
        /// </summary>
        private readonly bool _useGame;

        #endregion

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        /// <param name="useGame">True if the output uses "game", false if the output uses "machine"</param>
        public Logiqx(DatFile? datFile, bool useGame) : base(datFile)
        {
            _useGame = useGame;
            if (useGame)
                Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.LogiqxDeprecated);
            else
                Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.Logiqx);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];
            switch (datItem)
            {
                case Release release:
                    if (string.IsNullOrEmpty(release.GetName()))
                        missingFields.Add(Data.Models.Metadata.Release.NameKey);
                    if (string.IsNullOrEmpty(release.GetStringFieldValue(Data.Models.Metadata.Release.RegionKey)))
                        missingFields.Add(Data.Models.Metadata.Release.RegionKey);
                    break;

                case BiosSet biosset:
                    if (string.IsNullOrEmpty(biosset.GetName()))
                        missingFields.Add(Data.Models.Metadata.BiosSet.NameKey);
                    if (string.IsNullOrEmpty(biosset.GetStringFieldValue(Data.Models.Metadata.BiosSet.DescriptionKey)))
                        missingFields.Add(Data.Models.Metadata.BiosSet.DescriptionKey);
                    break;

                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetName()))
                        missingFields.Add(Data.Models.Metadata.Rom.NameKey);
                    if (rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) == null || rom.GetInt64FieldValue(Data.Models.Metadata.Rom.SizeKey) < 0)
                        missingFields.Add(Data.Models.Metadata.Rom.SizeKey);
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.CRCKey))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue("MD2"))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue("MD4"))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.MD5Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD128Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.RIPEMD160Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA256Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA384Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA512Key))
                        && string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SpamSumKey)))
                    {
                        missingFields.Add(Data.Models.Metadata.Rom.SHA1Key);
                    }
                    break;

                case Disk disk:
                    if (string.IsNullOrEmpty(disk.GetName()))
                        missingFields.Add(Data.Models.Metadata.Disk.NameKey);
                    if (string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.MD5Key))
                        && string.IsNullOrEmpty(disk.GetStringFieldValue(Data.Models.Metadata.Disk.SHA1Key)))
                    {
                        missingFields.Add(Data.Models.Metadata.Disk.SHA1Key);
                    }
                    break;

                case Media media:
                    if (string.IsNullOrEmpty(media.GetName()))
                        missingFields.Add(Data.Models.Metadata.Media.NameKey);
                    if (string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.MD5Key))
                        && string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.SHA1Key))
                        && string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.SHA256Key))
                        && string.IsNullOrEmpty(media.GetStringFieldValue(Data.Models.Metadata.Media.SpamSumKey)))
                    {
                        missingFields.Add(Data.Models.Metadata.Media.SHA1Key);
                    }
                    break;

                case DeviceRef deviceref:
                    if (string.IsNullOrEmpty(deviceref.GetName()))
                        missingFields.Add(Data.Models.Metadata.DeviceRef.NameKey);
                    break;

                case Sample sample:
                    if (string.IsNullOrEmpty(sample.GetName()))
                        missingFields.Add(Data.Models.Metadata.Sample.NameKey);
                    break;

                case Archive archive:
                    if (string.IsNullOrEmpty(archive.GetName()))
                        missingFields.Add(Data.Models.Metadata.Archive.NameKey);
                    break;

                case Driver driver:
                    if (driver.GetStringFieldValue(Data.Models.Metadata.Driver.StatusKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Data.Models.Metadata.Driver.StatusKey);
                    if (driver.GetStringFieldValue(Data.Models.Metadata.Driver.EmulationKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Data.Models.Metadata.Driver.EmulationKey);
                    if (driver.GetStringFieldValue(Data.Models.Metadata.Driver.CocktailKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Data.Models.Metadata.Driver.CocktailKey);
                    if (driver.GetStringFieldValue(Data.Models.Metadata.Driver.SaveStateKey).AsSupportStatus() == SupportStatus.NULL)
                        missingFields.Add(Data.Models.Metadata.Driver.SaveStateKey);
                    break;

                case DatItems.Formats.SoftwareList softwarelist:
                    if (string.IsNullOrEmpty(softwarelist.GetStringFieldValue(Data.Models.Metadata.SoftwareList.TagKey)))
                        missingFields.Add(Data.Models.Metadata.SoftwareList.TagKey);
                    if (string.IsNullOrEmpty(softwarelist.GetName()))
                        missingFields.Add(Data.Models.Metadata.SoftwareList.NameKey);
                    if (softwarelist.GetStringFieldValue(Data.Models.Metadata.SoftwareList.StatusKey).AsSoftwareListStatus() == SoftwareListStatus.None)
                        missingFields.Add(Data.Models.Metadata.SoftwareList.StatusKey);
                    break;
            }

            return missingFields;
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool ignoreblanks = false, bool throwOnError = false)
        {
            try
            {
                _logger.User($"Writing to '{outfile}'...");

                // Serialize the input file
                var metadata = ConvertToMetadata(ignoreblanks);
                var datafile = new Serialization.CrossModel.Logiqx().Deserialize(metadata, _useGame);

                // TODO: Reenable doctype writing
                // Only write the doctype if we don't have No-Intro data
                bool success;
                if (string.IsNullOrEmpty(Header.GetStringFieldValue(Data.Models.Metadata.Header.IdKey)))
                    success = new Serialization.Writers.Logiqx().Serialize(datafile, outfile, null, null, null, null);
                else
                    success = new Serialization.Writers.Logiqx().Serialize(datafile, outfile, null, null, null, null);

                if (!success)
                {
                    _logger.Warning($"File '{outfile}' could not be written! See the log for more details.");
                    return false;
                }
            }
            catch (Exception ex) when (!throwOnError)
            {
                _logger.Error(ex);
                return false;
            }

            _logger.User($"'{outfile}' written!{Environment.NewLine}");
            return true;
        }
    }
}
