using SabreTools.DatFiles;
using Xunit;

namespace SabreTools.DatTools.Test
{
    public class WriterTests
    {
        [Theory]
        [InlineData(DatFormat.CSV, "csv")]
        [InlineData(DatFormat.ClrMamePro, "dat")]
        [InlineData(DatFormat.RomCenter, "dat")]
        [InlineData(DatFormat.DOSCenter, "dat")]
        [InlineData(DatFormat.SabreJSON, "json")]
        [InlineData(DatFormat.RedumpMD2, "md2")]
        [InlineData(DatFormat.RedumpMD4, "md4")]
        [InlineData(DatFormat.RedumpMD5, "md5")]
        [InlineData(DatFormat.RedumpRIPEMD128, "ripemd128")]
        [InlineData(DatFormat.RedumpRIPEMD160, "ripemd160")]
        [InlineData(DatFormat.RedumpSFV, "sfv")]
        [InlineData(DatFormat.RedumpSHA1, "sha1")]
        [InlineData(DatFormat.RedumpSHA256, "sha256")]
        [InlineData(DatFormat.RedumpSHA384, "sha384")]
        [InlineData(DatFormat.RedumpSHA512, "sha512")]
        [InlineData(DatFormat.RedumpSpamSum, "spamsum")]
        [InlineData(DatFormat.SSV, "ssv")]
        [InlineData(DatFormat.TSV, "tsv")]
        [InlineData(DatFormat.AttractMode, "txt")]
        [InlineData(DatFormat.Listrom, "txt")]
        [InlineData(DatFormat.MissFile, "txt")]
        [InlineData(DatFormat.EverdriveSMDB, "txt")]
        [InlineData(DatFormat.Logiqx, "xml")]
        [InlineData(DatFormat.LogiqxDeprecated, "xml")]
        [InlineData(DatFormat.SabreXML, "xml")]
        [InlineData(DatFormat.SoftwareList, "xml")]
        [InlineData(DatFormat.Listxml, "xml")]
        [InlineData(DatFormat.OfflineList, "xml")]
        [InlineData(DatFormat.OpenMSX, "xml")]
        [InlineData(DatFormat.ArchiveDotOrg, "xml")]
        public void CreateOutFileNames_SingleFormat(DatFormat datFormat, string extension)
        {
            // Create the empty DatHeader
            var datHeader = new DatHeader();
            datHeader.SetFieldValue<string?>(DatHeader.FileNameKey, "test.dat");
            datHeader.SetFieldValue(DatHeader.DatFormatKey, datFormat);

            // Invoke the method
            string outDir = "C:\\Test";
            var actual = Writer.CreateOutFileNames(datHeader, outDir, overwrite: true);

            // Check the result
            string expected = $"{outDir}{System.IO.Path.DirectorySeparatorChar}test.{extension}";
            Assert.Single(actual);
            Assert.Equal(expected, actual[datFormat]);
        }

        [Fact]
        public void CreateOutFileNames_AllFormats()
        {
            // Create the empty DatHeader
            var datHeader = new DatHeader();
            datHeader.SetFieldValue<string?>(DatHeader.FileNameKey, "test.dat");
            datHeader.SetFieldValue(DatHeader.DatFormatKey, DatFormat.ALL);

            // Invoke the method
            string outDir = "C:\\Test";
            var actual = Writer.CreateOutFileNames(datHeader, outDir, overwrite: true);

            // Check the normalized results
            Assert.Equal(30, actual.Count);
            Assert.Equal("C:\\Test\\test.csv", actual[DatFormat.CSV].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.dat", actual[DatFormat.ClrMamePro].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.rc.dat", actual[DatFormat.RomCenter].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.dc.dat", actual[DatFormat.DOSCenter].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.json", actual[DatFormat.SabreJSON].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.md2", actual[DatFormat.RedumpMD2].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.md4", actual[DatFormat.RedumpMD4].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.md5", actual[DatFormat.RedumpMD5].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.ripemd128", actual[DatFormat.RedumpRIPEMD128].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.ripemd160", actual[DatFormat.RedumpRIPEMD160].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.sfv", actual[DatFormat.RedumpSFV].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.sha1", actual[DatFormat.RedumpSHA1].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.sha256", actual[DatFormat.RedumpSHA256].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.sha384", actual[DatFormat.RedumpSHA384].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.sha512", actual[DatFormat.RedumpSHA512].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.spamsum", actual[DatFormat.RedumpSpamSum].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.ssv", actual[DatFormat.SSV].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.tsv", actual[DatFormat.TSV].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.txt", actual[DatFormat.AttractMode].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.lr.txt", actual[DatFormat.Listrom].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.miss.txt", actual[DatFormat.MissFile].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.smdb.txt", actual[DatFormat.EverdriveSMDB].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.xml", actual[DatFormat.Logiqx].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.xml", actual[DatFormat.LogiqxDeprecated].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.sd.xml", actual[DatFormat.SabreXML].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.sl.xml", actual[DatFormat.SoftwareList].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.mame.xml", actual[DatFormat.Listxml].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.ol.xml", actual[DatFormat.OfflineList].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.msx.xml", actual[DatFormat.OpenMSX].Replace('/', '\\'));
            Assert.Equal("C:\\Test\\test.ado.xml", actual[DatFormat.ArchiveDotOrg].Replace('/', '\\'));
        }
    }
}