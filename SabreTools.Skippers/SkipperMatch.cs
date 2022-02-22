using System.Collections.Generic;
using System.IO;

using SabreTools.IO;
using SabreTools.Logging;

namespace SabreTools.Skippers
{
    /// <summary>
    /// Class for matching existing Skippers
    /// <summary>
    /// <remarks>
    /// Skippers, in general, are distributed as XML files by some projects
    /// in order to denote a way of transforming a file so that it will match
    /// the hashes included in their DATs. Each skipper file can contain multiple
    /// skipper rules, each of which denote a type of header/transformation. In
    /// turn, each of those rules can contain multiple tests that denote that
    /// a file should be processed using that rule. Transformations can include
    /// simply skipping over a portion of the file all the way to byteswapping
    /// the entire file. For the purposes of this library, Skippers also denote
    /// a way of changing files directly in order to produce a file whose external
    /// hash would match those same DATs.
    /// </remarks>
    public static class SkipperMatch
    {
        /// <summary>
        /// Header skippers represented by a list of skipper objects
        /// </summary>
        private static List<SkipperFile> Skippers = null;

        /// <summary>
        /// Local paths
        /// </summary>
        private static readonly string LocalPath = Path.Combine(PathTool.GetRuntimeDirectory(), "Skippers") + Path.DirectorySeparatorChar;

        #region Logging

        /// <summary>
        /// Logging object
        /// </summary>
        private static readonly Logger logger = new Logger();

        #endregion

        /// <summary>
        /// Initialize static fields
        /// </summary>
        /// <param name="experimental">True to enable internal header skipper generation, false to use file-based generation (default)</param>
        public static void Init(bool experimental = false)
        {
            // If the list is populated, don't add to it
            if (Skippers != null)
                return;

            // If we're using internal skipper generation
            if (experimental)
                PopulateSkippersInternal();

            // If we're using file-based skipper generation
            else
                PopulateSkippers();
        }

        /// <summary>
        /// Populate the entire list of header skippers from physical files
        /// </summary>
        /// <remarks>
        /// http://mamedev.emulab.it/clrmamepro/docs/xmlheaders.txt
        /// http://www.emulab.it/forum/index.php?topic=127.0
        /// </remarks>
        private static void PopulateSkippers()
        {
            // Ensure the list exists
            if (Skippers == null)
                Skippers = new List<SkipperFile>();

            // Get skippers for each known header type
            var searchfiles = Directory.EnumerateFiles(LocalPath, "*", SearchOption.AllDirectories);
            foreach (string skipperFile in searchfiles)
            {
                Skippers.Add(new SkipperFile(Path.GetFullPath(skipperFile)));
            }
        }

        /// <summary>
        /// Populate the entire list of header skippers from generated objects
        /// </summary>
        /// <remarks>
        /// http://mamedev.emulab.it/clrmamepro/docs/xmlheaders.txt
        /// http://www.emulab.it/forum/index.php?topic=127.0
        /// </remarks>
        private static void PopulateSkippersInternal()
        {
            // Ensure the list exists
            if (Skippers == null)
                Skippers = new List<SkipperFile>();

            // Get skippers for each known header type
            Skippers.Add(GetAtari7800());
            Skippers.Add(GetAtariLynx());
            Skippers.Add(GetCommodorePSID());
            Skippers.Add(GetNECPCEngine());
            Skippers.Add(GetNintendo64());
            Skippers.Add(GetNintendoEntertainmentSystem());
            Skippers.Add(GetNintendoFamicomDiskSystem());
            Skippers.Add(GetSuperNintendoEntertainmentSystem());
            Skippers.Add(GetSuperFamicomSPC());
        }

        /// <summary>
        /// Get the SkipperRule associated with a given file
        /// </summary>
        /// <param name="input">Name of the file to be checked</param>
        /// <param name="skipperName">Name of the skipper to be used, blank to find a matching skipper</param>
        /// <param name="logger">Logger object for file and console output</param>
        /// <returns>The SkipperRule that matched the file</returns>
        public static SkipperRule GetMatchingRule(string input, string skipperName)
        {
            // If the file doesn't exist, return a blank skipper rule
            if (!File.Exists(input))
            {
                logger.Error($"The file '{input}' does not exist so it cannot be tested");
                return new SkipperRule();
            }

            return GetMatchingRule(File.OpenRead(input), skipperName);
        }

        /// <summary>
        /// Get the SkipperRule associated with a given stream
        /// </summary>
        /// <param name="input">Name of the file to be checked</param>
        /// <param name="skipperName">Name of the skipper to be used, blank to find a matching skipper</param>
        /// <param name="keepOpen">True if the underlying stream should be kept open, false otherwise</param>
        /// <returns>The SkipperRule that matched the file</returns>
        public static SkipperRule GetMatchingRule(Stream input, string skipperName, bool keepOpen = false)
        {
            SkipperRule skipperRule = new SkipperRule();

            // If we have a null skipper name, we return since we're not matching skippers
            if (skipperName == null)
                return skipperRule;

            // Loop through and find a Skipper that has the right name
            logger.Verbose("Beginning search for matching header skip rules");
            List<SkipperFile> tempList = new List<SkipperFile>();
            tempList.AddRange(Skippers);

            // Loop through all known SkipperFiles
            foreach (SkipperFile skipper in tempList)
            {
                skipperRule = skipper.GetMatchingRule(input, skipperName);
                if (skipperRule != null)
                    break;
            }

            // If we're not keeping the stream open, dispose of the binary reader
            if (!keepOpen)
                input.Dispose();

            // If the SkipperRule is null, make it empty
            if (skipperRule == null)
                skipperRule = new SkipperRule();

            // If we have a blank rule, inform the user
            if (skipperRule.Tests == null)
                logger.Verbose("No matching rule found!");
            else
                logger.User("Matching rule found!");

            return skipperRule;
        }

        // These are hardcoded versions of the XML files that get parsed in
        // TODO: Should these be in their own derived classes?
        #region Skipper Generation

        /// <summary>
        /// Generate a SkipperFile for Atari 7800 headers
        /// </summary>
        /// <remarks>Originally from a7800.xml</remarks>
        private static SkipperFile GetAtari7800()
        {
            // Create tests
            var rule1Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x01,
                Value = new byte[] { 0x41, 0x54, 0x41, 0x52, 0x49, 0x37, 0x38, 0x30, 0x30 },
                Result = true,
            };

            var rule2Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x64,
                Value = new byte[] { 0x41, 0x43, 0x54, 0x55, 0x41, 0x4C, 0x20, 0x43, 0x41, 0x52, 0x54, 0x20, 0x44, 0x41, 0x54, 0x41, 0x20, 0x53, 0x54, 0x41, 0x52, 0x54, 0x53, 0x20, 0x48, 0x45, 0x52, 0x45 },
                Result = true,
            };

            // Create rules
            var rule1 = new SkipperRule
            {
                StartOffset = 0x80,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule1Test1,
                }
            };

            var rule2 = new SkipperRule
            {
                StartOffset = 0x80,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule2Test1,
                }
            };

            // Create file
            var skipperFile = new SkipperFile
            {
                Name = "Atari 7800",
                Author = "Roman Scherzer",
                Version = "1.0",
                SourceFile = "a7800",
                Rules = new List<SkipperRule>
                {
                    rule1,
                    rule2,
                },
            };

            return skipperFile;
        }

        /// <summary>
        /// Generate a SkipperFile for Atari Lynx headers
        /// </summary>
        /// <remarks>Originally from lynx.xml</remarks>
        private static SkipperFile GetAtariLynx()
        {
            // Create tests
            var rule1Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x4C, 0x59, 0x4E, 0x58 },
                Result = true,
            };

            var rule2Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x06,
                Value = new byte[] { 0x42, 0x53, 0x39 },
                Result = true,
            };

            // Create rules
            var rule1 = new SkipperRule
            {
                StartOffset = 0x40,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule1Test1,
                }
            };

            var rule2 = new SkipperRule
            {
                StartOffset = 0x40,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule2Test1,
                }
            };

            // Create file
            var skipperFile = new SkipperFile
            {
                Name = "Atari Lynx",
                Author = "Roman Scherzer",
                Version = "1.0",
                SourceFile = "lynx",
                Rules = new List<SkipperRule>
                {
                    rule1,
                    rule2,
                },
            };

            return skipperFile;
        }

        /// <summary>
        /// Generate a SkipperFile for Commodore PSID headers
        /// </summary>
        /// <remarks>Originally from psid.xml</remarks>
        private static SkipperFile GetCommodorePSID()
        {
            // Create tests
            var rule1Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x50, 0x53, 0x49, 0x44, 0x00, 0x01, 0x00, 0x76 },
                Result = true,
            };

            var rule2Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x50, 0x53, 0x49, 0x44, 0x00, 0x03, 0x00, 0x7c },
                Result = true,
            };

            var rule3Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x50, 0x53, 0x49, 0x44, 0x00, 0x02, 0x00, 0x7c },
                Result = true,
            };

            var rule4Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x50, 0x53, 0x49, 0x44, 0x00, 0x01, 0x00, 0x7c },
                Result = true,
            };

            var rule5Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x52, 0x53, 0x49, 0x44, 0x00, 0x02, 0x00, 0x7c },
                Result = true,
            };

            // Create rules
            var rule1 = new SkipperRule
            {
                StartOffset = 0x76,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule1Test1,
                }
            };

            var rule2 = new SkipperRule
            {
                StartOffset = 0x76,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule2Test1,
                }
            };

            var rule3 = new SkipperRule
            {
                StartOffset = 0x7c,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule3Test1,
                }
            };

            var rule4 = new SkipperRule
            {
                StartOffset = 0x7c,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule4Test1,
                }
            };

            var rule5 = new SkipperRule
            {
                StartOffset = 0x7c,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule5Test1,
                }
            };

            // Create file
            var skipperFile = new SkipperFile
            {
                Name = "psid",
                Author = "Yori Yoshizuki",
                Version = "1.2",
                SourceFile = "psid",
                Rules = new List<SkipperRule>
                {
                    rule1,
                    rule2,
                    rule3,
                    rule4,
                    rule5,
                },
            };

            return skipperFile;
        }

        /// <summary>
        /// Generate a SkipperFile for NEC PC-Engine / TurboGrafx 16 headers
        /// </summary>
        /// <remarks>Originally from pce.xml</remarks>
        private static SkipperFile GetNECPCEngine()
        {
            // Create tests
            var rule1Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAA, 0xBB, 0x02 },
                Result = true,
            };

            // Create rules
            var rule1 = new SkipperRule
            {
                StartOffset = 0x200,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule1Test1,
                }
            };

            // Create file
            var skipperFile = new SkipperFile
            {
                Name = "NEC TurboGrafx-16/PC-Engine",
                Author = "Matt Nadareski (darksabre76)",
                Version = "1.0",
                SourceFile = "pce",
                Rules = new List<SkipperRule>
                {
                    rule1,
                },
            };

            return skipperFile;
        }

        /// <summary>
        /// Generate a SkipperFile for Nintendo 64 headers
        /// </summary>
        /// <remarks>Originally from n64.xml</remarks>
        private static SkipperFile GetNintendo64()
        {
            // Create tests
            var rule1Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x80, 0x37, 0x12, 0x40 },
                Result = true,
            };

            var rule2Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x37, 0x80, 0x40, 0x12 },
                Result = true,
            };

            var rule3Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x40, 0x12, 0x37, 0x80 },
                Result = true,
            };

            // Create rules
            var rule1 = new SkipperRule
            {
                StartOffset = 0x00,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule1Test1,
                }
            };

            var rule2 = new SkipperRule
            {
                StartOffset = 0x00,
                EndOffset = null,
                Operation = HeaderSkipOperation.Byteswap,
                Tests = new List<SkipperTest>
                {
                    rule2Test1,
                }
            };

            var rule3 = new SkipperRule
            {
                StartOffset = 0x00,
                EndOffset = null,
                Operation = HeaderSkipOperation.Wordswap,
                Tests = new List<SkipperTest>
                {
                    rule3Test1,
                }
            };

            // Create file
            var skipperFile = new SkipperFile
            {
                Name = "Nintendo 64 - ABCD",
                Author = "CUE",
                Version = "1.1",
                SourceFile = "n64",
                Rules = new List<SkipperRule>
                {
                    rule1, // V64
                    rule2, // Z64
                    rule3, // N64
                },
            };

            return skipperFile;
        }

        /// <summary>
        /// Generate a SkipperFile for Nintendo Entertainment System headers
        /// </summary>
        /// <remarks>Originally from nes.xml</remarks>
        private static SkipperFile GetNintendoEntertainmentSystem()
        {
            // Create tests
            var rule1Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x4E, 0x45, 0x53, 0x1A },
                Result = true,
            };

            // Create rules
            var rule1 = new SkipperRule
            {
                StartOffset = 0x10,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule1Test1,
                }
            };

            // Create file
            var skipperFile = new SkipperFile
            {
                Name = "Nintendo Famicon/NES",
                Author = "Roman Scherzer",
                Version = "1.1",
                SourceFile = "nes",
                Rules = new List<SkipperRule>
                {
                    rule1,
                },
            };

            return skipperFile;
        }

        /// <summary>
        /// Generate a SkipperFile for Nintendo Famicom Disk System headers
        /// </summary>
        /// <remarks>Originally from fds.xml</remarks>
        private static SkipperFile GetNintendoFamicomDiskSystem()
        {
            // Create tests
            var rule1Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x46, 0x44, 0x53, 0x1A, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                Result = true,
            };

            var rule2Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x46, 0x44, 0x53, 0x1A, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                Result = true,
            };

            var rule3Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x46, 0x44, 0x53, 0x1A, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                Result = true,
            };

            var rule4Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x46, 0x44, 0x53, 0x1A, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                Result = true,
            };

            // Create rules
            var rule1 = new SkipperRule
            {
                StartOffset = 0x10,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule1Test1,
                }
            };

            var rule2 = new SkipperRule
            {
                StartOffset = 0x10,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule2Test1,
                }
            };

            var rule3 = new SkipperRule
            {
                StartOffset = 0x10,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule3Test1,
                }
            };            

            var rule4 = new SkipperRule
            {
                StartOffset = 0x10,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule4Test1,
                }
            };

            // Create file
            var skipperFile = new SkipperFile
            {
                Name = "fds",
                Author = "Yori Yoshizuki",
                Version = "1.0",
                SourceFile = "fds",
                Rules = new List<SkipperRule>
                {
                    rule1,
                    rule2,
                    rule3,
                    rule4,
                },
            };

            return skipperFile;
        }

        /// <summary>
        /// Generate a SkipperFile for Super Nintendo Entertainment System headers
        /// </summary>
        /// <remarks>Originally from snes.xml</remarks>
        private static SkipperFile GetSuperNintendoEntertainmentSystem()
        {
            // Create tests
            var rule1Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x16,
                Value = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 },
                Result = true,
            };

            var rule2Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x16,
                Value = new byte[] { 0xAA, 0xBB, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00 },
                Result = true,
            };

            var rule3Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x16,
                Value = new byte[] { 0x53, 0x55, 0x50, 0x45, 0x52, 0x55, 0x46, 0x4F },
                Result = true,
            };

            // Create rules
            var rule1 = new SkipperRule
            {
                StartOffset = 0x200,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule1Test1,
                }
            };

            var rule2 = new SkipperRule
            {
                StartOffset = 0x200,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule2Test1,
                }
            };

            var rule3 = new SkipperRule
            {
                StartOffset = 0x200,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule3Test1,
                }
            };

            // Create file
            var skipperFile = new SkipperFile
            {
                Name = "Nintendo Super Famicom/SNES",
                Author = "Matt Nadareski (darksabre76)",
                Version = "1.0",
                SourceFile = "snes",
                Rules = new List<SkipperRule>
                {
                    rule1, // FIG
                    rule2, // SMC
                    rule3, // UFO
                },
            };

            return skipperFile;
        }

        /// <summary>
        /// Generate a SkipperFile for Super Famicom SPC headers
        /// </summary>
        /// <remarks>Originally from spc.xml</remarks>
        private static SkipperFile GetSuperFamicomSPC()
        {
            // Create tests
            var rule1Test1 = new SkipperTest
            {
                Type = HeaderSkipTest.Data,
                Offset = 0x00,
                Value = new byte[] { 0x53, 0x4E, 0x45, 0x53, 0x2D, 0x53, 0x50, 0x43 },
                Result = true,
            };

            // Create rules
            var rule1 = new SkipperRule
            {
                StartOffset = 0x100,
                EndOffset = null,
                Operation = HeaderSkipOperation.None,
                Tests = new List<SkipperTest>
                {
                    rule1Test1,
                }
            };

            // Create file
            var skipperFile = new SkipperFile
            {
                Name = "Nintendo Super Famicon SPC",
                Author = "Yori Yoshizuki",
                Version = "1.0",
                SourceFile = "spc",
                Rules = new List<SkipperRule>
                {
                    rule1,
                },
            };

            return skipperFile;
        }

        #endregion
    }
}
