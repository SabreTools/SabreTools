using System.Collections.Generic;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;

namespace SabreTools.DatFiles.Formats
{
    /// <summary>
    /// Represents an openMSX softawre list XML DAT
    /// </summary>
    public sealed class OpenMSX : SerializableDatFile<Data.Models.OpenMSX.SoftwareDb, Serialization.Readers.OpenMSX, Serialization.Writers.OpenMSX, Serialization.CrossModel.OpenMSX>
    {
        #region Constants

        /// <summary>
        /// DTD for original openMSX DATs
        /// </summary>
        private const string OpenMSXDTD = @"<!ELEMENT softwaredb (person*)>
<!ELEMENT software (title, genmsxid?, system, company,year,country,dump)>
<!ELEMENT title (#PCDATA)>
<!ELEMENT genmsxid (#PCDATA)>
<!ELEMENT system (#PCDATA)>
<!ELEMENT company (#PCDATA)>
<!ELEMENT year (#PCDATA)>
<!ELEMENT country (#PCDATA)>
<!ELEMENT dump (#PCDATA)>
";

        private const string OpenMSXCredits = @"<!-- Credits -->
<![CDATA[
The softwaredb.xml file contains information about rom mapper types

- Copyright 2003 Nicolas Beyaert (Initial Database)
- Copyright 2004-2013 BlueMSX Team
- Copyright 2005-2023 openMSX Team
- Generation MSXIDs by www.generation-msx.nl

- Thanks go out to:
- - Generation MSX/Sylvester for the incredible source of information
  - p_gimeno and diedel for their help adding and valdiating ROM additions
  - GDX for additional ROM info and validations and corrections


]]>";

        #endregion

        #region Fields

        /// <inheritdoc/>
        public override ItemType[] SupportedTypes
            => [
                ItemType.Rom,
            ];

        #endregion

        /// <summary>
        /// Constructor designed for casting a base DatFile
        /// </summary>
        /// <param name="datFile">Parent DatFile to copy from</param>
        public OpenMSX(DatFile? datFile) : base(datFile)
        {
            Header.SetFieldValue<DatFormat>(DatHeader.DatFormatKey, DatFormat.OpenMSX);
        }

        /// <inheritdoc/>
        protected internal override List<string>? GetMissingRequiredFields(DatItem datItem)
        {
            List<string> missingFields = [];

            // Check item name
            if (string.IsNullOrEmpty(datItem.GetName()))
                missingFields.Add(Data.Models.Metadata.Rom.NameKey);

            switch (datItem)
            {
                case Rom rom:
                    if (string.IsNullOrEmpty(rom.GetStringFieldValue(Data.Models.Metadata.Rom.SHA1Key)))
                        missingFields.Add(Data.Models.Metadata.Rom.SHA1Key);
                    break;
            }

            return missingFields;
        }
    }
}
