using SabreTools.Library.Data;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;

namespace SabreTools.Library.Skippers
{
    public class Skipper
    {
        #region Fields

        /// <summary>
        /// Skipper name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Author names
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// File version
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Set of all rules in the skipper
        /// </summary>
        public List<SkipperRule> Rules { get; set; }

        /// <summary>
        /// Filename the skipper lives in
        /// </summary>
        public string SourceFile { get; set; }

        /// <summary>
        /// Local paths
        /// </summary>
        public static string LocalPath = Path.Combine(Globals.ExeDir, "Skippers") + Path.DirectorySeparatorChar;

        /// <summary>
        /// Header skippers represented by a list of skipper objects
        /// </summary>
        private static List<Skipper> List;

        #endregion

        #region Constructors

        /// <summary>
        /// Create an empty Skipper object
        /// </summary>
        public Skipper()
        {
            Name = string.Empty;
            Author = string.Empty;
            Version = string.Empty;
            Rules = new List<SkipperRule>();
            SourceFile = string.Empty;
        }

        /// <summary>
        /// Create a Skipper object parsed from an input file
        /// </summary>
        /// <param name="filename">Name of the file to parse</param>
        public Skipper(string filename)
        {
            Rules = new List<SkipperRule>();
            SourceFile = Path.GetFileNameWithoutExtension(filename);

            XmlReader xtr = filename.GetXmlTextReader();

            if (xtr == null)
                return;

            bool valid = false;
            xtr.MoveToContent();
            while (!xtr.EOF)
            {
                if (xtr.NodeType != XmlNodeType.Element)
                    xtr.Read();

                switch (xtr.Name.ToLowerInvariant())
                {
                    case "detector":
                        valid = true;
                        xtr.Read();
                        break;

                    case "name":
                        Name = xtr.ReadElementContentAsString();
                        break;

                    case "author":
                        Author = xtr.ReadElementContentAsString();
                        break;

                    case "version":
                        Version = xtr.ReadElementContentAsString();
                        break;

                    case "rule":
                        // Get the information from the rule first
                        SkipperRule rule = new SkipperRule
                        {
                            StartOffset = null,
                            EndOffset = null,
                            Operation = HeaderSkipOperation.None,
                            Tests = new List<SkipperTest>(),
                            SourceFile = Path.GetFileNameWithoutExtension(filename),
                        };

                        if (xtr.GetAttribute("start_offset") != null)
                        {
                            string offset = xtr.GetAttribute("start_offset");
                            if (offset.ToLowerInvariant() == "eof")
                            {
                                rule.StartOffset = null;
                            }
                            else
                            {
                                rule.StartOffset = Convert.ToInt64(offset, 16);
                            }
                        }

                        if (xtr.GetAttribute("end_offset") != null)
                        {
                            string offset = xtr.GetAttribute("end_offset");
                            if (offset.ToLowerInvariant() == "eof")
                            {
                                rule.EndOffset = null;
                            }
                            else
                            {
                                rule.EndOffset = Convert.ToInt64(offset, 16);
                            }
                        }

                        if (xtr.GetAttribute("operation") != null)
                        {
                            string operation = xtr.GetAttribute("operation");
                            switch (operation.ToLowerInvariant())
                            {
                                case "bitswap":
                                    rule.Operation = HeaderSkipOperation.Bitswap;
                                    break;
                                case "byteswap":
                                    rule.Operation = HeaderSkipOperation.Byteswap;
                                    break;
                                case "wordswap":
                                    rule.Operation = HeaderSkipOperation.Wordswap;
                                    break;
                            }
                        }

                        // Now read the individual tests into the Rule
                        XmlReader subreader = xtr.ReadSubtree();

                        if (subreader != null)
                        {
                            while (!subreader.EOF)
                            {
                                if (subreader.NodeType != XmlNodeType.Element)
                                    subreader.Read();

                                // Get the test type
                                SkipperTest test = new SkipperTest
                                {
                                    Offset = 0,
                                    Value = new byte[0],
                                    Result = true,
                                    Mask = new byte[0],
                                    Size = 0,
                                    Operator = HeaderSkipTestFileOperator.Equal,
                                };
                                switch (subreader.Name.ToLowerInvariant())
                                {
                                    case "data":
                                        test.Type = HeaderSkipTest.Data;
                                        break;

                                    case "or":
                                        test.Type = HeaderSkipTest.Or;
                                        break;

                                    case "xor":
                                        test.Type = HeaderSkipTest.Xor;
                                        break;

                                    case "and":
                                        test.Type = HeaderSkipTest.And;
                                        break;

                                    case "file":
                                        test.Type = HeaderSkipTest.File;
                                        break;

                                    default:
                                        subreader.Read();
                                        break;
                                }

                                // Now populate all the parts that we can
                                if (subreader.GetAttribute("offset") != null)
                                {
                                    string offset = subreader.GetAttribute("offset");
                                    if (offset.ToLowerInvariant() == "eof")
                                        test.Offset = null;
                                    else
                                        test.Offset = Convert.ToInt64(offset, 16);
                                }

                                if (subreader.GetAttribute("value") != null)
                                {
                                    string value = subreader.GetAttribute("value");

                                    // http://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
                                    test.Value = new byte[value.Length / 2];
                                    for (int index = 0; index < test.Value.Length; index++)
                                    {
                                        string byteValue = value.Substring(index * 2, 2);
                                        test.Value[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                    }
                                }

                                if (subreader.GetAttribute("result") != null)
                                {
                                    string result = subreader.GetAttribute("result");
                                    if (!bool.TryParse(result, out bool resultBool))
                                        resultBool = true;

                                    test.Result = resultBool;
                                }

                                if (subreader.GetAttribute("mask") != null)
                                {
                                    string mask = subreader.GetAttribute("mask");

                                    // http://stackoverflow.com/questions/321370/how-can-i-convert-a-hex-string-to-a-byte-array
                                    test.Mask = new byte[mask.Length / 2];
                                    for (int index = 0; index < test.Mask.Length; index++)
                                    {
                                        string byteValue = mask.Substring(index * 2, 2);
                                        test.Mask[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                                    }
                                }

                                if (subreader.GetAttribute("size") != null)
                                {
                                    string size = subreader.GetAttribute("size");
                                    if (size.ToLowerInvariant() == "po2")
                                        test.Size = null;
                                    else
                                        test.Size = Convert.ToInt64(size, 16);
                                }

                                if (subreader.GetAttribute("operator") != null)
                                {
                                    string oper = subreader.GetAttribute("operator");
#if NET_FRAMEWORK
                                    switch (oper.ToLowerInvariant())
                                    {
                                        case "less":
                                            test.Operator = HeaderSkipTestFileOperator.Less;
                                            break;
                                        case "greater":
                                            test.Operator = HeaderSkipTestFileOperator.Greater;
                                            break;
                                        case "equal":
                                        default:
                                            test.Operator = HeaderSkipTestFileOperator.Equal;
                                            break;
                                    }
#else
                                    test.Operator = oper.ToLowerInvariant() switch
                                    {
                                        "less" => HeaderSkipTestFileOperator.Less,
                                        "greater" => HeaderSkipTestFileOperator.Greater,
                                        "equal" => HeaderSkipTestFileOperator.Equal,
                                        _ => HeaderSkipTestFileOperator.Equal,
                                    };
#endif
                                }

                                // Add the created test to the rule
                                rule.Tests.Add(test);
                                subreader.Read();
                            }
                        }

                        // Add the created rule to the skipper
                        Rules.Add(rule);
                        xtr.Skip();
                        break;
                    default:
                        xtr.Read();
                        break;
                }
            }

            // If we somehow have an invalid file, zero out the fields
            if (!valid)
            {
                Name = null;
                Author = null;
                Version = null;
                Rules = null;
                SourceFile = null;
            }
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Initialize static fields
        /// </summary>
        public static void Init()
        {
            PopulateSkippers();
        }

        /// <summary>
        /// Populate the entire list of header Skippers
        /// </summary>
        /// <remarks>
        /// http://mamedev.emulab.it/clrmamepro/docs/xmlheaders.txt
        /// http://www.emulab.it/forum/index.php?topic=127.0
        /// </remarks>
        private static void PopulateSkippers()
        {
            if (List == null)
                List = new List<Skipper>();

            foreach (string skipperFile in Directory.EnumerateFiles(LocalPath, "*", SearchOption.AllDirectories))
            {
                List.Add(new Skipper(Path.GetFullPath(skipperFile)));
            }
        }

        /// <summary>
        /// Detect header skipper compliance and create an output file
        /// </summary>
        /// <param name="file">Name of the file to be parsed</param>
        /// <param name="outDir">Output directory to write the file to, empty means the same directory as the input file</param>
        /// <param name="nostore">True if headers should not be stored in the database, false otherwise</param>
        /// <returns>True if the output file was created, false otherwise</returns>
        public static bool DetectTransformStore(string file, string outDir, bool nostore)
        {
            // Create the output directory if it doesn't exist
            DirectoryExtensions.Ensure(outDir, create: true);

            Globals.Logger.User($"\nGetting skipper information for '{file}'");

            // Get the skipper rule that matches the file, if any
            SkipperRule rule = GetMatchingRule(file, string.Empty);

            // If we have an empty rule, return false
            if (rule.Tests == null || rule.Tests.Count == 0 || rule.Operation != HeaderSkipOperation.None)
                return false;

            Globals.Logger.User("File has a valid copier header");

            // Get the header bytes from the file first
            string hstr;
            try
            {
                // Extract the header as a string for the database
#if NET_FRAMEWORK
                using (var fs = FileExtensions.TryOpenRead(file))
                {
#else
                using var fs = FileExtensions.TryOpenRead(file);
#endif
                byte[] hbin = new byte[(int)rule.StartOffset];
                fs.Read(hbin, 0, (int)rule.StartOffset);
                hstr = Utilities.ByteArrayToString(hbin);
#if NET_FRAMEWORK
                }
#endif
            }
            catch
            {
                return false;
            }

            // Apply the rule to the file
            string newfile = (string.IsNullOrWhiteSpace(outDir) ? Path.GetFullPath(file) + ".new" : Path.Combine(outDir, Path.GetFileName(file)));
            rule.TransformFile(file, newfile);

            // If the output file doesn't exist, return false
            if (!File.Exists(newfile))
                return false;

            // Now add the information to the database if it's not already there
            if (!nostore)
            {
                BaseFile baseFile = FileExtensions.GetInfo(newfile, chdsAsFiles: true);
                DatabaseTools.AddHeaderToDatabase(hstr, Utilities.ByteArrayToString(baseFile.SHA1), rule.SourceFile);
            }

            return true;
        }

        /// <summary>
        /// Detect and replace header(s) to the given file
        /// </summary>
        /// <param name="file">Name of the file to be parsed</param>
        /// <param name="outDir">Output directory to write the file to, empty means the same directory as the input file</param>
        /// <returns>True if a header was found and appended, false otherwise</returns>
        public static bool RestoreHeader(string file, string outDir)
        {
            // Create the output directory if it doesn't exist
            if (!string.IsNullOrWhiteSpace(outDir) && !Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);

            // First, get the SHA-1 hash of the file
            BaseFile baseFile = FileExtensions.GetInfo(file, chdsAsFiles: true);

            // Retrieve a list of all related headers from the database
            List<string> headers = DatabaseTools.RetrieveHeadersFromDatabase(Utilities.ByteArrayToString(baseFile.SHA1));

            // If we have nothing retrieved, we return false
            if (headers.Count == 0)
                return false;

            // Now loop through and create the reheadered files, if possible
            for (int i = 0; i < headers.Count; i++)
            {
                string outputFile = (string.IsNullOrWhiteSpace(outDir) ? $"{Path.GetFullPath(file)}.new" : Path.Combine(outDir, Path.GetFileName(file))) + i;
                Globals.Logger.User($"Creating reheadered file: {outputFile}");
                FileExtensions.AppendBytes(file, outputFile, Utilities.StringToByteArray(headers[i]), null);
                Globals.Logger.User("Reheadered file created!");
            }

            return true;
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
                Globals.Logger.Error($"The file '{input}' does not exist so it cannot be tested");
                return new SkipperRule();
            }

            return GetMatchingRule(FileExtensions.TryOpenRead(input), skipperName);
        }

        /// <summary>
        /// Get the SkipperRule associated with a given stream
        /// </summary>
        /// <param name="input">Name of the file to be checked</param>
        /// <param name="skipperName">Name of the skipper to be used, blank to find a matching skipper</param>
        /// <param name="logger">Logger object for file and console output</param>
        /// <param name="keepOpen">True if the underlying stream should be kept open, false otherwise</param>
        /// <returns>The SkipperRule that matched the file</returns>
        public static SkipperRule GetMatchingRule(Stream input, string skipperName, bool keepOpen = false)
        {
            SkipperRule skipperRule = new SkipperRule();

            // If we have a null skipper name, we return since we're not matching skippers
            if (skipperName == null)
                return skipperRule;

            // Loop through and find a Skipper that has the right name
            Globals.Logger.Verbose("Beginning search for matching header skip rules");
            List<Skipper> tempList = new List<Skipper>();
            tempList.AddRange(List);

            foreach (Skipper skipper in tempList)
            {
                // If we're searching for the skipper OR we have a match to an inputted one
                if (string.IsNullOrWhiteSpace(skipperName)
                    || (!string.IsNullOrWhiteSpace(skipper.Name) && skipperName.ToLowerInvariant() == skipper.Name.ToLowerInvariant())
                    || (!string.IsNullOrWhiteSpace(skipper.Name) && skipperName.ToLowerInvariant() == skipper.SourceFile.ToLowerInvariant()))
                {
                    // Loop through the rules until one is found that works
                    BinaryReader br = new BinaryReader(input);

                    foreach (SkipperRule rule in skipper.Rules)
                    {
                        // Always reset the stream back to the original place
                        input.Seek(0, SeekOrigin.Begin);

                        // For each rule, make sure it passes each test
                        bool success = true;
                        foreach (SkipperTest test in rule.Tests)
                        {
                            bool result = true;
                            switch (test.Type)
                            {
                                case HeaderSkipTest.Data:
                                    // First seek to the correct position
                                    if (test.Offset == null)
                                        input.Seek(0, SeekOrigin.End);
                                    else if (test.Offset > 0 && test.Offset <= input.Length)
                                        input.Seek((long)test.Offset, SeekOrigin.Begin);
                                    else if (test.Offset < 0 && Math.Abs((long)test.Offset) <= input.Length)
                                        input.Seek((long)test.Offset, SeekOrigin.End);

                                    // Then read and compare bytewise
                                    result = true;
                                    for (int i = 0; i < test.Value.Length; i++)
                                    {
                                        try
                                        {
                                            if (br.ReadByte() != test.Value[i])
                                            {
                                                result = false;
                                                break;
                                            }
                                        }
                                        catch
                                        {
                                            result = false;
                                            break;
                                        }
                                    }

                                    // Return if the expected and actual results match
                                    success &= (result == test.Result);
                                    break;

                                case HeaderSkipTest.Or:
                                case HeaderSkipTest.Xor:
                                case HeaderSkipTest.And:
                                    // First seek to the correct position
                                    if (test.Offset == null)
                                        input.Seek(0, SeekOrigin.End);
                                    else if (test.Offset > 0 && test.Offset <= input.Length)
                                        input.Seek((long)test.Offset, SeekOrigin.Begin);
                                    else if (test.Offset < 0 && Math.Abs((long)test.Offset) <= input.Length)
                                        input.Seek((long)test.Offset, SeekOrigin.End);

                                    result = true;
                                    try
                                    {
                                        // Then apply the mask if it exists
                                        byte[] read = br.ReadBytes(test.Mask.Length);
                                        byte[] masked = new byte[test.Mask.Length];
                                        for (int i = 0; i < read.Length; i++)
                                        {
                                            masked[i] = (byte)(test.Type == HeaderSkipTest.And ? read[i] & test.Mask[i] :
                                                (test.Type == HeaderSkipTest.Or ? read[i] | test.Mask[i] : read[i] ^ test.Mask[i])
                                            );
                                        }

                                        // Finally, compare it against the value
                                        for (int i = 0; i < test.Value.Length; i++)
                                        {
                                            if (masked[i] != test.Value[i])
                                            {
                                                result = false;
                                                break;
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        result = false;
                                    }

                                    // Return if the expected and actual results match
                                    success &= (result == test.Result);
                                    break;

                                case HeaderSkipTest.File:
                                    // First get the file size from stream
                                    long size = input.Length;

                                    // If we have a null size, check that the size is a power of 2
                                    result = true;
                                    if (test.Size == null)
                                    {
                                        // http://stackoverflow.com/questions/600293/how-to-check-if-a-number-is-a-power-of-2
                                        result = (((ulong)size & ((ulong)size - 1)) == 0);
                                    }
                                    else if (test.Operator == HeaderSkipTestFileOperator.Less)
                                    {
                                        result = (size < test.Size);
                                    }
                                    else if (test.Operator == HeaderSkipTestFileOperator.Greater)
                                    {
                                        result = (size > test.Size);
                                    }
                                    else if (test.Operator == HeaderSkipTestFileOperator.Equal)
                                    {
                                        result = (size == test.Size);
                                    }

                                    // Return if the expected and actual results match
                                    success &= (result == test.Result);
                                    break;
                            }
                        }

                        // If we still have a success, then return this rule
                        if (success)
                        {
                            // If we're not keeping the stream open, dispose of the binary reader
                            if (!keepOpen)
                                input.Dispose();

                            Globals.Logger.User(" Matching rule found!");
                            return rule;
                        }
                    }
                }
            }

            // If we're not keeping the stream open, dispose of the binary reader
            if (!keepOpen)
                input.Dispose();

            // If we have a blank rule, inform the user
            if (skipperRule.Tests == null)
                Globals.Logger.Verbose("No matching rule found!");

            return skipperRule;
        }

        #endregion
    }
}
