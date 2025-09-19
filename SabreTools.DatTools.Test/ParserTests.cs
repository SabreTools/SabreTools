using System;
using System.IO;
using SabreTools.Core.Filter;
using SabreTools.DatFiles;
using SabreTools.Reports;
using Xunit;

namespace SabreTools.DatTools.Test
{
    public class ParserTests
    {
        [Fact]
        public void CreateDatFile_Default_Logiqx()
        {
            var datFile = Parser.CreateDatFile();
            Assert.Equal(DatFormat.Logiqx, datFile.Header.GetFieldValue<DatFormat>(DatHeader.DatFormatKey));
            Assert.Equal(0, datFile.Items.DatStatistics.TotalCount);
            Assert.Equal(0, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Theory]
        [InlineData((DatFormat)0x00, DatFormat.Logiqx)]
        [InlineData(DatFormat.Logiqx, DatFormat.Logiqx)]
        [InlineData(DatFormat.LogiqxDeprecated, DatFormat.LogiqxDeprecated)]
        [InlineData(DatFormat.SoftwareList, DatFormat.SoftwareList)]
        [InlineData(DatFormat.Listxml, DatFormat.Listxml)]
        [InlineData(DatFormat.OfflineList, DatFormat.OfflineList)]
        [InlineData(DatFormat.SabreXML, DatFormat.SabreXML)]
        [InlineData(DatFormat.OpenMSX, DatFormat.OpenMSX)]
        [InlineData(DatFormat.ArchiveDotOrg, DatFormat.ArchiveDotOrg)]
        [InlineData(DatFormat.ClrMamePro, DatFormat.ClrMamePro)]
        [InlineData(DatFormat.RomCenter, DatFormat.RomCenter)]
        [InlineData(DatFormat.DOSCenter, DatFormat.DOSCenter)]
        [InlineData(DatFormat.AttractMode, DatFormat.AttractMode)]
        [InlineData(DatFormat.MissFile, DatFormat.MissFile)]
        [InlineData(DatFormat.CSV, DatFormat.CSV)]
        [InlineData(DatFormat.SSV, DatFormat.SSV)]
        [InlineData(DatFormat.TSV, DatFormat.TSV)]
        [InlineData(DatFormat.Listrom, DatFormat.Listrom)]
        [InlineData(DatFormat.EverdriveSMDB, DatFormat.EverdriveSMDB)]
        [InlineData(DatFormat.SabreJSON, DatFormat.SabreJSON)]
        [InlineData(DatFormat.RedumpSFV, DatFormat.RedumpSFV)]
        [InlineData(DatFormat.RedumpMD2, DatFormat.RedumpMD2)]
        [InlineData(DatFormat.RedumpMD4, DatFormat.RedumpMD4)]
        [InlineData(DatFormat.RedumpMD5, DatFormat.RedumpMD5)]
        [InlineData(DatFormat.RedumpRIPEMD128, DatFormat.RedumpRIPEMD128)]
        [InlineData(DatFormat.RedumpRIPEMD160, DatFormat.RedumpRIPEMD160)]
        [InlineData(DatFormat.RedumpSHA1, DatFormat.RedumpSHA1)]
        [InlineData(DatFormat.RedumpSHA256, DatFormat.RedumpSHA256)]
        [InlineData(DatFormat.RedumpSHA384, DatFormat.RedumpSHA384)]
        [InlineData(DatFormat.RedumpSHA512, DatFormat.RedumpSHA512)]
        [InlineData(DatFormat.RedumpSpamSum, DatFormat.RedumpSpamSum)]
        public void CreateDatFile_Format_NoBaseDat(DatFormat datFormat, DatFormat expected)
        {
            var datFile = Parser.CreateDatFile(datFormat, baseDat: null);
            Assert.Equal(expected, datFile.Header.GetFieldValue<DatFormat>(DatHeader.DatFormatKey));
            Assert.Equal(0, datFile.Items.DatStatistics.TotalCount);
            Assert.Equal(0, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Theory]
        [InlineData((DatFormat)0x00, DatFormat.Logiqx)]
        [InlineData(DatFormat.Logiqx, DatFormat.Logiqx)]
        [InlineData(DatFormat.LogiqxDeprecated, DatFormat.LogiqxDeprecated)]
        [InlineData(DatFormat.SoftwareList, DatFormat.SoftwareList)]
        [InlineData(DatFormat.Listxml, DatFormat.Listxml)]
        [InlineData(DatFormat.OfflineList, DatFormat.OfflineList)]
        [InlineData(DatFormat.SabreXML, DatFormat.SabreXML)]
        [InlineData(DatFormat.OpenMSX, DatFormat.OpenMSX)]
        [InlineData(DatFormat.ArchiveDotOrg, DatFormat.ArchiveDotOrg)]
        [InlineData(DatFormat.ClrMamePro, DatFormat.ClrMamePro)]
        [InlineData(DatFormat.RomCenter, DatFormat.RomCenter)]
        [InlineData(DatFormat.DOSCenter, DatFormat.DOSCenter)]
        [InlineData(DatFormat.AttractMode, DatFormat.AttractMode)]
        [InlineData(DatFormat.MissFile, DatFormat.MissFile)]
        [InlineData(DatFormat.CSV, DatFormat.CSV)]
        [InlineData(DatFormat.SSV, DatFormat.SSV)]
        [InlineData(DatFormat.TSV, DatFormat.TSV)]
        [InlineData(DatFormat.Listrom, DatFormat.Listrom)]
        [InlineData(DatFormat.EverdriveSMDB, DatFormat.EverdriveSMDB)]
        [InlineData(DatFormat.SabreJSON, DatFormat.SabreJSON)]
        [InlineData(DatFormat.RedumpSFV, DatFormat.RedumpSFV)]
        [InlineData(DatFormat.RedumpMD2, DatFormat.RedumpMD2)]
        [InlineData(DatFormat.RedumpMD4, DatFormat.RedumpMD4)]
        [InlineData(DatFormat.RedumpMD5, DatFormat.RedumpMD5)]
        [InlineData(DatFormat.RedumpRIPEMD128, DatFormat.RedumpRIPEMD128)]
        [InlineData(DatFormat.RedumpRIPEMD160, DatFormat.RedumpRIPEMD160)]
        [InlineData(DatFormat.RedumpSHA1, DatFormat.RedumpSHA1)]
        [InlineData(DatFormat.RedumpSHA256, DatFormat.RedumpSHA256)]
        [InlineData(DatFormat.RedumpSHA384, DatFormat.RedumpSHA384)]
        [InlineData(DatFormat.RedumpSHA512, DatFormat.RedumpSHA512)]
        [InlineData(DatFormat.RedumpSpamSum, DatFormat.RedumpSpamSum)]
        public void CreateDatFile_Format_BaseDat(DatFormat datFormat, DatFormat expected)
        {
            var baseDat = Parser.CreateDatFile();
            baseDat.Header.SetFieldValue<string?>(DatHeader.FileNameKey, "filename");

            var datFile = Parser.CreateDatFile(datFormat, baseDat);
            Assert.Equal(expected, datFile.Header.GetFieldValue<DatFormat>(DatHeader.DatFormatKey));
            Assert.Equal("filename", datFile.Header.GetFieldValue<string?>(DatHeader.FileNameKey));
            Assert.Equal(0, datFile.Items.DatStatistics.TotalCount);
            Assert.Equal(0, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Theory]
        [InlineData((DatFormat)0x00, (DatFormat)0x00)] // I think this is a bug
        [InlineData(DatFormat.Logiqx, DatFormat.Logiqx)]
        [InlineData(DatFormat.LogiqxDeprecated, DatFormat.LogiqxDeprecated)]
        [InlineData(DatFormat.SoftwareList, DatFormat.SoftwareList)]
        [InlineData(DatFormat.Listxml, DatFormat.Listxml)]
        [InlineData(DatFormat.OfflineList, DatFormat.OfflineList)]
        [InlineData(DatFormat.SabreXML, DatFormat.SabreXML)]
        [InlineData(DatFormat.OpenMSX, DatFormat.OpenMSX)]
        [InlineData(DatFormat.ArchiveDotOrg, DatFormat.ArchiveDotOrg)]
        [InlineData(DatFormat.ClrMamePro, DatFormat.ClrMamePro)]
        [InlineData(DatFormat.RomCenter, DatFormat.RomCenter)]
        [InlineData(DatFormat.DOSCenter, DatFormat.DOSCenter)]
        [InlineData(DatFormat.AttractMode, DatFormat.AttractMode)]
        [InlineData(DatFormat.MissFile, DatFormat.MissFile)]
        [InlineData(DatFormat.CSV, DatFormat.CSV)]
        [InlineData(DatFormat.SSV, DatFormat.SSV)]
        [InlineData(DatFormat.TSV, DatFormat.TSV)]
        [InlineData(DatFormat.Listrom, DatFormat.Listrom)]
        [InlineData(DatFormat.EverdriveSMDB, DatFormat.EverdriveSMDB)]
        [InlineData(DatFormat.SabreJSON, DatFormat.SabreJSON)]
        [InlineData(DatFormat.RedumpSFV, DatFormat.RedumpSFV)]
        [InlineData(DatFormat.RedumpMD2, DatFormat.RedumpMD2)]
        [InlineData(DatFormat.RedumpMD4, DatFormat.RedumpMD4)]
        [InlineData(DatFormat.RedumpMD5, DatFormat.RedumpMD5)]
        [InlineData(DatFormat.RedumpRIPEMD128, DatFormat.RedumpRIPEMD128)]
        [InlineData(DatFormat.RedumpRIPEMD160, DatFormat.RedumpRIPEMD160)]
        [InlineData(DatFormat.RedumpSHA1, DatFormat.RedumpSHA1)]
        [InlineData(DatFormat.RedumpSHA256, DatFormat.RedumpSHA256)]
        [InlineData(DatFormat.RedumpSHA384, DatFormat.RedumpSHA384)]
        [InlineData(DatFormat.RedumpSHA512, DatFormat.RedumpSHA512)]
        [InlineData(DatFormat.RedumpSpamSum, DatFormat.RedumpSpamSum)]
        public void CreateDatFile_Format_FromHeader(DatFormat datFormat, DatFormat expected)
        {
            DatHeader datHeader = new DatHeader();
            datHeader.SetFieldValue(DatHeader.DatFormatKey, datFormat);
            datHeader.SetFieldValue<string?>(DatHeader.FileNameKey, "filename");

            DatModifiers datModifiers = new DatModifiers();
            datModifiers.Quotes = true;

            var datFile = Parser.CreateDatFile(datHeader, datModifiers);
            Assert.Equal(expected, datFile.Header.GetFieldValue<DatFormat>(DatHeader.DatFormatKey));
            Assert.Equal("filename", datFile.Header.GetFieldValue<string?>(DatHeader.FileNameKey));
            Assert.True(datFile.Modifiers.Quotes);
            Assert.Equal(0, datFile.Items.DatStatistics.TotalCount);
            Assert.Equal(0, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Theory]
        [InlineData(null, (DatFormat)0x00, 0)]
        [InlineData("test-logiqx.xml", DatFormat.Logiqx, 6)]
        //[InlineData(null, DatFormat.LogiqxDeprecated, 0)] // Not parsed separately
        [InlineData("test-softwarelist.xml", DatFormat.SoftwareList, 6)]
        [InlineData("test-listxml.xml", DatFormat.Listxml, 19)]
        [InlineData("test-offlinelist.xml", DatFormat.OfflineList, 1)]
        //[InlineData(null, DatFormat.SabreXML, 0)] // TODO: Create good-enough test file for this
        [InlineData("test-openmsx.xml", DatFormat.OpenMSX, 3)]
        [InlineData("test-archivedotorg.xml", DatFormat.ArchiveDotOrg, 1)]
        [InlineData("test-cmp.dat", DatFormat.ClrMamePro, 6)]
        [InlineData("test-romcenter.dat", DatFormat.RomCenter, 1)]
        [InlineData("test-doscenter.dat", DatFormat.DOSCenter, 1)]
        [InlineData("test-attractmode.txt", DatFormat.AttractMode, 1)]
        //[InlineData(null, DatFormat.MissFile, 0)] // Parsing is not supported
        //[InlineData(null, DatFormat.CSV, 0)] // TODO: Create good-enough test file for this
        //[InlineData(null, DatFormat.SSV, 0)] // TODO: Create good-enough test file for this
        //[InlineData(null, DatFormat.TSV, 0)] // TODO: Create good-enough test file for this
        [InlineData("test-listrom.txt", DatFormat.Listrom, 6)]
        [InlineData("test-smdb.txt", DatFormat.EverdriveSMDB, 1)]
        //[InlineData(null, DatFormat.SabreJSON, 0)] // TODO: Create good-enough test file for this
        [InlineData("test-sfv.sfv", DatFormat.RedumpSFV, 1)]
        [InlineData("test-md2.md2", DatFormat.RedumpMD2, 1)]
        [InlineData("test-md4.md4", DatFormat.RedumpMD4, 1)]
        [InlineData("test-md5.md5", DatFormat.RedumpMD5, 1)]
        // [InlineData("test-ripemd128.ripemd128", DatFormat.RedumpRIPEMD128, 1)] // TODO: Enable when Serialization is updated
        // [InlineData("test-ripemd160.ripemd160", DatFormat.RedumpRIPEMD160, 1)] // TODO: Enable when Serialization is updated
        [InlineData("test-sha1.sha1", DatFormat.RedumpSHA1, 1)]
        [InlineData("test-sha256.sha256", DatFormat.RedumpSHA256, 1)]
        [InlineData("test-sha384.sha384", DatFormat.RedumpSHA384, 1)]
        [InlineData("test-sha512.sha512", DatFormat.RedumpSHA512, 1)]
        [InlineData("test-spamsum.spamsum", DatFormat.RedumpSpamSum, 1)]
        public void ParseIntoTest(string? filename, DatFormat datFormat, int totalCount)
        {
            // For all filenames, add the local path for test data
            if (filename != null)
                filename = Path.Combine(Environment.CurrentDirectory, "TestData", filename);
            else
                filename = string.Empty;

            var datFile = Parser.CreateDatFile();
            datFile.Header.RemoveField(DatHeader.DatFormatKey);

            Parser.ParseInto(datFile, filename, throwOnError: true);
            Assert.Equal(datFormat, datFile.Header.GetFieldValue<DatFormat>(DatHeader.DatFormatKey));
            Assert.Equal(totalCount, datFile.Items.DatStatistics.TotalCount);
            //Assert.Equal(totalCount, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Theory]
        [InlineData(null, (DatFormat)0x00, 0)]
        [InlineData("test-logiqx.xml", DatFormat.Logiqx, 6)]
        //[InlineData(null, DatFormat.LogiqxDeprecated, 0)] // Not parsed separately
        [InlineData("test-softwarelist.xml", DatFormat.SoftwareList, 6)]
        [InlineData("test-listxml.xml", DatFormat.Listxml, 19)]
        [InlineData("test-offlinelist.xml", DatFormat.OfflineList, 1)]
        //[InlineData(null, DatFormat.SabreXML, 0)] // TODO: Create good-enough test file for this
        [InlineData("test-openmsx.xml", DatFormat.OpenMSX, 3)]
        [InlineData("test-archivedotorg.xml", DatFormat.ArchiveDotOrg, 1)]
        [InlineData("test-cmp.dat", DatFormat.ClrMamePro, 6)]
        [InlineData("test-romcenter.dat", DatFormat.RomCenter, 1)]
        [InlineData("test-doscenter.dat", DatFormat.DOSCenter, 1)]
        [InlineData("test-attractmode.txt", DatFormat.AttractMode, 1)]
        //[InlineData(null, DatFormat.MissFile, 0)] // Parsing is not supported
        //[InlineData(null, DatFormat.CSV, 0)] // TODO: Create good-enough test file for this
        //[InlineData(null, DatFormat.SSV, 0)] // TODO: Create good-enough test file for this
        //[InlineData(null, DatFormat.TSV, 0)] // TODO: Create good-enough test file for this
        [InlineData("test-listrom.txt", DatFormat.Listrom, 6)]
        [InlineData("test-smdb.txt", DatFormat.EverdriveSMDB, 1)]
        //[InlineData(null, DatFormat.SabreJSON, 0)] // TODO: Create good-enough test file for this
        [InlineData("test-sfv.sfv", DatFormat.RedumpSFV, 1)]
        [InlineData("test-md2.md2", DatFormat.RedumpMD2, 1)]
        [InlineData("test-md4.md4", DatFormat.RedumpMD4, 1)]
        [InlineData("test-md5.md5", DatFormat.RedumpMD5, 1)]
        //[InlineData("test-ripemd128.ripemd128", DatFormat.RedumpRIPEMD128, 1)] // TODO: Enable when Serialization is updated
        //[InlineData("test-ripemd160.ripemd160", DatFormat.RedumpRIPEMD160, 1)] // TODO: Enable when Serialization is updated
        [InlineData("test-sha1.sha1", DatFormat.RedumpSHA1, 1)]
        [InlineData("test-sha256.sha256", DatFormat.RedumpSHA256, 1)]
        [InlineData("test-sha384.sha384", DatFormat.RedumpSHA384, 1)]
        [InlineData("test-sha512.sha512", DatFormat.RedumpSHA512, 1)]
        [InlineData("test-spamsum.spamsum", DatFormat.RedumpSpamSum, 1)]
        public void ParseStatisticsTest(string? filename, DatFormat datFormat, int totalCount)
        {
            // For all filenames, add the local path for test data
            if (filename != null)
                filename = Path.Combine(Environment.CurrentDirectory, "TestData", filename);

            var datFile = Parser.ParseStatistics(filename, new FilterRunner(Array.Empty<string>()), throwOnError: true);
            Assert.Equal(datFormat, datFile.Header.GetFieldValue<DatFormat>(DatHeader.DatFormatKey));
            Assert.Equal(totalCount, datFile.Items.DatStatistics.TotalCount);
            //Assert.Equal(totalCount, datFile.ItemsDB.DatStatistics.TotalCount);
        }

        [Theory]
        [InlineData(StatReportFormat.None, typeof(Reports.Formats.Textfile))]
        [InlineData(StatReportFormat.Textfile, typeof(Reports.Formats.Textfile))]
        [InlineData(StatReportFormat.CSV, typeof(Reports.Formats.CommaSeparatedValue))]
        [InlineData(StatReportFormat.HTML, typeof(Reports.Formats.Html))]
        [InlineData(StatReportFormat.SSV, typeof(Reports.Formats.SemicolonSeparatedValue))]
        [InlineData(StatReportFormat.TSV, typeof(Reports.Formats.TabSeparatedValue))]
        [InlineData((StatReportFormat)0xFF, typeof(Reports.Formats.Textfile))]
        public void CreateReportTest(StatReportFormat reportFormat, Type expected)
        {
            var report = Parser.CreateReport(reportFormat, []);
            Assert.Equal(expected, report.GetType());
        }
    }
}