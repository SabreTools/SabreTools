using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Core.Filter;
using Xunit;

namespace SabreTools.Core.Test.Filter
{
    public class ExtraIniItemTests
    {
        [Fact]
        public void Constructor_EmptyPath_NoMappings()
        {
            string iniPath = string.Empty;
            ExtraIniItem extraIniItem = new ExtraIniItem("Sample.Name", iniPath);
            Assert.Empty(extraIniItem.Mappings);
        }

        [Fact]
        public void Constructor_InvalidPath_NoMappings()
        {
            string iniPath = "INVALID";
            ExtraIniItem extraIniItem = new ExtraIniItem("Sample.Name", iniPath);
            Assert.Empty(extraIniItem.Mappings);
        }

        [Fact]
        public void Constructor_ValidPath_Mappings()
        {
            string iniPath = Path.Combine(Environment.CurrentDirectory, "TestData", "extra.ini");
            ExtraIniItem extraIniItem = new ExtraIniItem("Sample.Name", iniPath);

            Dictionary<string, string> mappings = extraIniItem.Mappings;
            Assert.NotEmpty(mappings);
            Assert.Equal("true", mappings["useBool"]);
            Assert.Equal("Value", mappings["useValue"]);
            Assert.Equal("Other", mappings["useOther"]);
        }
    }
}
