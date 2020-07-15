using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;
using Microsoft.Data.Sqlite;

namespace RombaSharp
{
    public partial class RombaSharp
    {
        #region Private Flag features

        public const string CopyValue = "copy";
        private static Feature CopyFlag
        {
            get
            {
                return new Feature(
                    CopyValue,
                    "-copy",
                    "Copy files to output instead of rebuilding",
                    FeatureType.Flag);
            }
        } // Unique to RombaSharp

        public const string FixdatOnlyValue = "fixdat-only";
        private static Feature FixdatOnlyFlag
        {
            get
            {
                return new Feature(
                    FixdatOnlyValue,
                    "-fixdatOnly",
                    "only fix dats and don't generate torrentzips",
                    FeatureType.Flag);
            }
        }

        public const string LogOnlyValue = "log-only";
        private static Feature LogOnlyFlag
        {
            get
            {
                return new Feature(
                LogOnlyValue,
                "-log-only",
                "Only write out actions to log",
                FeatureType.Flag);
            }
        }

        public const string NoDbValue = "no-db";
        private static Feature NoDbFlag
        {
            get
            {
                return new Feature(
                    NoDbValue,
                    "-no-db",
                    "archive into depot but do not touch DB index and ignore only-needed flag",
                    FeatureType.Flag);
            }
        }

        public const string OnlyNeededValue = "only-needed";
        private static Feature OnlyNeededFlag
        {
            get
            {
                return new Feature(
                    OnlyNeededValue,
                    "-only-needed",
                    "only archive ROM files actually referenced by DAT files from the DAT index",
                    FeatureType.Flag);
            }
        }

        public const string SkipInitialScanValue = "skip-initial-scan";
        private static Feature SkipInitialScanFlag
        {
            get
            {
                return new Feature(
                    SkipInitialScanValue,
                    "-skip-initial-scan",
                    "skip the initial scan of the files to determine amount of work",
                    FeatureType.Flag);
            }
        }

        public const string UseGolangZipValue = "use-golang-zip";
        private static Feature UseGolangZipFlag
        {
            get
            {
                return new Feature(
                    UseGolangZipValue,
                    "-use-golang-zip",
                    "use go zip implementation instead of zlib",
                    FeatureType.Flag);
            }
        }

        #endregion

        #region Private Int32 features

        public const string Include7ZipsInt32Value = "include-7zips";
        private static Feature Include7ZipsInt32Input
        {
            get
            {
                return new Feature(
                    Include7ZipsInt32Value,
                    "-include-7zips",
                    "flag value == 0 means: add 7zip files themselves into the depot in addition to their contents, flag value == 2 means add 7zip files themselves but don't add content",
                    FeatureType.Int32);
            }
        }

        public const string IncludeGZipsInt32Value = "include-gzips";
        private static Feature IncludeGZipsInt32Input
        {
            get
            {
                return new Feature(
                    IncludeGZipsInt32Value,
                    "-include-gzips",
                    "flag value == 0 means: add gzip files themselves into the depot in addition to their contents, flag value == 2 means add gzip files themselves but don't add content",
                    FeatureType.Int32);
            }
        }

        public const string IncludeZipsInt32Value = "include-zips";
        private static Feature IncludeZipsInt32Input
        {
            get
            {
                return new Feature(
                    IncludeZipsInt32Value,
                    "-include-zips",
                    "flag value == 0 means: add zip files themselves into the depot in addition to their contents, flag value == 2 means add zip files themselves but don't add content",
                    FeatureType.Int32);
            }
        }

        public const string SubworkersInt32Value = "subworkers";
        private static Feature SubworkersInt32Input
        {
            get
            {
                return new Feature(
                    SubworkersInt32Value,
                    "-subworkers",
                    "how many subworkers to launch for each worker",
                    FeatureType.Int32);
            }
        } // Defaults to Workers count in config

        public const string WorkersInt32Value = "workers";
        private static Feature WorkersInt32Input
        {
            get
            {
                return new Feature(
                    WorkersInt32Value,
                    "-workers",
                    "how many workers to launch for the job",
                    FeatureType.Int32);
            }
        } // Defaults to Workers count in config

        #endregion

        #region Private Int64 features

        public const string SizeInt64Value = "size";
        private static Feature SizeInt64Input
        {
            get
            {
                return new Feature(
                    SizeInt64Value,
                    "-size",
                    "size of the rom to lookup",
                    FeatureType.Int64);
            }
        }

        #endregion

        #region Private List<String> features

        public const string DatsListStringValue = "dats";
        private static Feature DatsListStringInput
        {
            get
            {
                return new Feature(
                    DatsListStringValue,
                    "-dats",
                    "purge only roms declared in these dats",
                    FeatureType.List);
            }
        }

        public const string DepotListStringValue = "depot";
        private static Feature DepotListStringInput
        {
            get
            {
                return new Feature(
                    DepotListStringValue,
                    "-depot",
                    "work only on specified depot path",
                    FeatureType.List);
            }
        }

        #endregion

        #region Private String features

        public const string BackupStringValue = "backup";
        private static Feature BackupStringInput
        {
            get
            {
                return new Feature(
                    BackupStringValue,
                    "-backup",
                    "backup directory where backup files are moved to",
                    FeatureType.String);
            }
        }

        public const string DescriptionStringValue = "description";
        private static Feature DescriptionStringInput
        {
            get
            {
                return new Feature(
                    DescriptionStringValue,
                    "-description",
                    "description value in DAT header",
                    FeatureType.String);
            }
        }

        public const string MissingSha1sStringValue = "missing-sha1s";
        private static Feature MissingSha1sStringInput
        {
            get
            {
                return new Feature(
                    MissingSha1sStringValue,
                    "-missingSha1s",
                    "write paths of dats with missing sha1s into this file",
                    FeatureType.String);
            }
        }

        public const string NameStringValue = "name";
        private static Feature NameStringInput
        {
            get
            {
                return new Feature(
                    NameStringValue,
                    "-name",
                    "name value in DAT header",
                    FeatureType.String);
            }
        }

        public const string NewStringValue = "new";
        private static Feature NewStringInput
        {
            get
            {
                return new Feature(
                    NewStringValue,
                    "-new",
                    "new DAT file",
                    FeatureType.String);
            }
        }

        public const string OldStringValue = "old";
        private static Feature OldStringInput
        {
            get
            {
                return new Feature(
                    OldStringValue,
                    "-old",
                    "old DAT file",
                    FeatureType.String);
            }
        }

        public const string OutStringValue = "out";
        private static Feature OutStringInput
        {
            get
            {
                return new Feature(
                    OutStringValue,
                    "-out",
                    "output file",
                    FeatureType.String);
            }
        }

        public const string ResumeStringValue = "resume";
        private static Feature ResumeStringInput
        {
            get
            {
                return new Feature(
                    ResumeStringValue,
                    "-resume",
                    "resume a previously interrupted operation from the specified path",
                    FeatureType.String);
            }
        }

        public const string SourceStringValue = "source";
        private static Feature SourceStringInput
        {
            get
            {
                return new Feature(
                    SourceStringValue,
                    "-source",
                    "source directory",
                    FeatureType.String);
            }
        }

        #endregion

        public static Help RetrieveHelp()
        {
            // Create and add the header to the Help object
            string barrier = "-----------------------------------------";
            List<string> helpHeader = new List<string>()
            {
                "RombaSharp - C# port of the Romba rom management tool",
                barrier,
                "Usage: RombaSharp [option] [filename|dirname] ...",
                string.Empty
            };

            // Create the base help object with header
            Help help = new Help(helpHeader);

            // Add all of the features
            help.Add(new HelpFeature());
            help.Add(new DetailedHelpFeature());
            help.Add(new ScriptFeature());
            help.Add(new ArchiveFeature());
            help.Add(new BuildFeature());
            help.Add(new CancelFeature());
            help.Add(new DatStatsFeature());
            help.Add(new DbStatsFeature());
            help.Add(new DiffdatFeature());
            help.Add(new Dir2DatFeature());
            help.Add(new EDiffdatFeature());
            help.Add(new ExportFeature());
            help.Add(new FixdatFeature());
            help.Add(new ImportFeature());
            help.Add(new LookupFeature());
            help.Add(new MemstatsFeature());
            help.Add(new MergeFeature());
            help.Add(new MissFeature());
            help.Add(new PurgeBackupFeature());
            help.Add(new PurgeDeleteFeature());
            help.Add(new RefreshDatsFeature());
            help.Add(new RescanDepotsFeature());
            help.Add(new ProgressFeature());
            help.Add(new ShutdownFeature());
            help.Add(new VersionFeature());

            return help;
        }

        #region Top-level Features

        private class RombaSharpFeature : TopLevel
        {
        }

        private class ArchiveFeature : RombaSharpFeature
        {
            public const string Value = "Archive";

            public ArchiveFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "archive" };
                this.Description = "Adds ROM files from the specified directories to the ROM archive.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"Adds ROM files from the specified directories to the ROM archive.
Traverses the specified directory trees looking for zip files and normal files.
Unpacked files will be stored as individual entries. Prior to unpacking a zip
file, the external SHA1 is checked against the DAT index. 
If -only-needed is set, only those files are put in the ROM archive that
have a current entry in the DAT index.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(OnlyNeededFlag);
                AddFeature(ResumeStringInput);
                AddFeature(IncludeZipsInt32Input); // Defaults to 0
                AddFeature(WorkersInt32Input);
                AddFeature(IncludeGZipsInt32Input); // Defaults to 0
                AddFeature(Include7ZipsInt32Input); // Defaults to 0
                AddFeature(SkipInitialScanFlag);
                AddFeature(UseGolangZipFlag);
                AddFeature(NoDbFlag);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get the archive scanning level
                // TODO: Remove usage
                int sevenzip = GetInt32(features, Include7ZipsInt32Value);
                int gz = GetInt32(features, IncludeGZipsInt32Value);
                int zip = GetInt32(features, IncludeZipsInt32Value);

                // Get feature flags
                bool noDb = GetBoolean(features, NoDbValue);
                bool onlyNeeded = GetBoolean(features, OnlyNeededValue);

                // First we want to get just all directories from the inputs
                List<string> onlyDirs = new List<string>();
                foreach (string input in Inputs)
                {
                    if (Directory.Exists(input))
                        onlyDirs.Add(Path.GetFullPath(input));
                }

                // Then process all of the input directories into an internal DAT
                DatFile df = DatFile.Create();
                foreach (string dir in onlyDirs)
                {
                    // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                    df.PopulateFromDir(dir, Hash.DeepHashes, false, false, SkipFileType.None, false, false, _tmpdir, false, null, true, null);
                    df.PopulateFromDir(dir, Hash.DeepHashes, false, true, SkipFileType.None, false, false, _tmpdir, false, null, true, null);
                }

                // Create an empty Dat for files that need to be rebuilt
                DatFile need = DatFile.Create();

                // Open the database connection
                SqliteConnection dbc = new SqliteConnection(_connectionString);
                dbc.Open();

                // Now that we have the Dats, add the files to the database
                string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
                string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
                string sha1query = "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES";
                string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
                string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

                foreach (string key in df.Keys)
                {
                    List<DatItem> datItems = df[key];
                    foreach (Rom rom in datItems)
                    {
                        // If we care about if the file exists, check the databse first
                        if (onlyNeeded && !noDb)
                        {
                            string query = "SELECT * FROM crcsha1 JOIN md5sha1 ON crcsha1.sha1=md5sha1.sha1"
                                        + $" WHERE crcsha1.crc=\"{rom.CRC}\""
                                        + $" OR md5sha1.md5=\"{rom.MD5}\""
                                        + $" OR md5sha1.sha1=\"{rom.SHA1}\"";
                            SqliteCommand slc = new SqliteCommand(query, dbc);
                            SqliteDataReader sldr = slc.ExecuteReader();

                            if (sldr.HasRows)
                            {
                                // Add to the queries
                                if (!string.IsNullOrWhiteSpace(rom.CRC))
                                    crcquery += $" (\"{rom.CRC}\"),";

                                if (!string.IsNullOrWhiteSpace(rom.MD5))
                                    md5query += $" (\"{rom.MD5}\"),";

                                if (!string.IsNullOrWhiteSpace(rom.SHA1))
                                {
                                    sha1query += $" (\"{rom.SHA1}\", \"{_depots.Keys.ToList()[0]}\"),";

                                    if (!string.IsNullOrWhiteSpace(rom.CRC))
                                        crcsha1query += $" (\"{rom.CRC}\", \"{rom.SHA1}\"),";

                                    if (!string.IsNullOrWhiteSpace(rom.MD5))
                                        md5sha1query += $" (\"{rom.MD5}\", \"{rom.SHA1}\"),";
                                }

                                // Add to the Dat
                                need.Add(key, rom);
                            }
                        }
                        // Otherwise, just add the file to the list
                        else
                        {
                            // Add to the queries
                            if (!noDb)
                            {
                                if (!string.IsNullOrWhiteSpace(rom.CRC))
                                    crcquery += $" (\"{rom.CRC}\"),";

                                if (!string.IsNullOrWhiteSpace(rom.MD5))
                                    md5query += $" (\"{rom.MD5}\"),";

                                if (!string.IsNullOrWhiteSpace(rom.SHA1))
                                {
                                    sha1query += $" (\"{rom.SHA1}\", \"{_depots.Keys.ToList()[0]}\"),";

                                    if (!string.IsNullOrWhiteSpace(rom.CRC))
                                        crcsha1query += $" (\"{rom.CRC}\", \"{rom.SHA1}\"),";

                                    if (!string.IsNullOrWhiteSpace(rom.MD5))
                                        md5sha1query += $" (\"{rom.MD5}\", \"{rom.SHA1}\"),";
                                }
                            }

                            // Add to the Dat
                            need.Add(key, rom);
                        }
                    }
                }

                // Now run the queries, if they're populated
                if (crcquery != "INSERT OR IGNORE INTO crc (crc) VALUES")
                {
                    SqliteCommand slc = new SqliteCommand(crcquery.TrimEnd(','), dbc);
                    slc.ExecuteNonQuery();
                    slc.Dispose();
                }

                if (md5query != "INSERT OR IGNORE INTO md5 (md5) VALUES")
                {
                    SqliteCommand slc = new SqliteCommand(md5query.TrimEnd(','), dbc);
                    slc.ExecuteNonQuery();
                    slc.Dispose();
                }

                if (sha1query != "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES")
                {
                    SqliteCommand slc = new SqliteCommand(sha1query.TrimEnd(','), dbc);
                    slc.ExecuteNonQuery();
                    slc.Dispose();
                }

                if (crcsha1query != "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES")
                {
                    SqliteCommand slc = new SqliteCommand(crcsha1query.TrimEnd(','), dbc);
                    slc.ExecuteNonQuery();
                    slc.Dispose();
                }

                if (md5sha1query != "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES")
                {
                    SqliteCommand slc = new SqliteCommand(md5sha1query.TrimEnd(','), dbc);
                    slc.ExecuteNonQuery();
                    slc.Dispose();
                }

                // Create the sorting object to use and rebuild the needed files
                need.RebuildGeneric(onlyDirs, _depots.Keys.ToList()[0], false /*quickScan*/, false /*date*/,
                    false /*delete*/, false /*inverse*/, OutputFormat.TorrentGzipRomba, false /*updateDat*/,
                    null /*headerToCheckAgainst*/, true /* chdsAsFiles */);
            }
        }

        private class BuildFeature : RombaSharpFeature
        {
            public const string Value = "Build";

            public BuildFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "build" };
                this.Description = "For each specified DAT file it creates the torrentzip files.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"For each specified DAT file it creates the torrentzip files in the specified
output dir. The files will be placed in the specified location using a folder
structure according to the original DAT master directory tree structure.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(OutStringInput);
                AddFeature(FixdatOnlyFlag);
                AddFeature(CopyFlag);
                AddFeature(WorkersInt32Input);
                AddFeature(SubworkersInt32Input);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                bool copy = GetBoolean(features, CopyValue);
                string outdat = GetString(features, OutStringValue);

                // Verify the filenames
                Dictionary<string, string> foundDats = GetValidDats(Inputs);

                // Ensure the output directory is set
                if (string.IsNullOrWhiteSpace(outdat))
                    outdat = "out";

                // Now that we have the dictionary, we can loop through and output to a new folder for each
                foreach (string key in foundDats.Keys)
                {
                    // Get the DAT file associated with the key
                    DatFile datFile = DatFile.CreateAndParse(Path.Combine(_dats, foundDats[key]));

                    // Create the new output directory if it doesn't exist
                    string outputFolder = Path.Combine(outdat, Path.GetFileNameWithoutExtension(foundDats[key]));
                    DirectoryExtensions.Ensure(outputFolder, create: true);

                    // Get all online depots
                    List<string> onlineDepots = _depots.Where(d => d.Value.Item2).Select(d => d.Key).ToList();

                    // Now scan all of those depots and rebuild
                    datFile.RebuildDepot(onlineDepots, outputFolder, false /*date*/,
                        false /*delete*/, false /*inverse*/, (copy ? OutputFormat.TorrentGzipRomba : OutputFormat.TorrentZip),
                        false /*updateDat*/, null /*headerToCheckAgainst*/);
                }
            }
        }

        private class CancelFeature : RombaSharpFeature
        {
            public const string Value = "Cancel";

            public CancelFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "cancel" };
                this.Description = "Cancels current long-running job";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Cancels current long-running job.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                Globals.Logger.User("This feature is not yet implemented: cancel");
            }
        }

        private class DatStatsFeature : RombaSharpFeature
        {
            public const string Value = "DatStats";

            public DatStatsFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "datstats" };
                this.Description = "Prints dat stats.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Print dat stats.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // If we have no inputs listed, we want to use datroot
                if (Inputs == null || Inputs.Count == 0)
                {
                    Inputs = new List<string> { Path.GetFullPath(_dats) };
                }

                // Now output the stats for all inputs
                DatStats.OutputStats(Inputs, "rombasharp-datstats", null /* outDir */, true /* single */, true /* baddumpCol */, true /* nodumpCol */, StatReportFormat.Textfile);
            }
        }

        private class DbStatsFeature : RombaSharpFeature
        {
            public const string Value = "DbStats";

            public DbStatsFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "dbstats" };
                this.Description = "Prints db stats.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Print db stats.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                SqliteConnection dbc = new SqliteConnection(_connectionString);
                dbc.Open();

                // Total number of CRCs
                string query = "SELECT COUNT(*) FROM crc";
                SqliteCommand slc = new SqliteCommand(query, dbc);
                Globals.Logger.User($"Total CRCs: {(long)slc.ExecuteScalar()}");

                // Total number of MD5s
                query = "SELECT COUNT(*) FROM md5";
                slc = new SqliteCommand(query, dbc);
                Globals.Logger.User($"Total MD5s: {(long)slc.ExecuteScalar()}");

                // Total number of SHA1s
                query = "SELECT COUNT(*) FROM sha1";
                slc = new SqliteCommand(query, dbc);
                Globals.Logger.User($"Total SHA1s: {(long)slc.ExecuteScalar()}");

                // Total number of DATs
                query = "SELECT COUNT(*) FROM dat";
                slc = new SqliteCommand(query, dbc);
                Globals.Logger.User($"Total DATs: {(long)slc.ExecuteScalar()}");

                slc.Dispose();
                dbc.Dispose();
            }
        }

        private class DetailedHelpFeature : RombaSharpFeature
        {
            public const string Value = "Help (Detailed)";

            public DetailedHelpFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-??", "-hd", "--help-detailed" };
                this.Description = "Show this detailed help";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Display a detailed help text to the screen.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override bool ProcessArgs(string[] args, Help help)
            {
                // If we had something else after help
                if (args.Length > 1)
                {
                    help.OutputIndividualFeature(args[1], includeLongDescription: true);
                    return true;
                }

                // Otherwise, show generic help
                else
                {
                    help.OutputAllHelp();
                    return true;
                }
            }
        }

        private class DiffdatFeature : RombaSharpFeature
        {
            public const string Value = "Diffdat";

            public DiffdatFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "diffdat" };
                this.Description = "Creates a DAT file with those entries that are in -new DAT.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"Creates a DAT file with those entries that are in -new DAT file and not
in -old DAT file. Ignores those entries in -old that are not in -new.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(OutStringInput);
                AddFeature(OldStringInput);
                AddFeature(NewStringInput);
                AddFeature(NameStringInput);
                AddFeature(DescriptionStringInput);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                string name = GetString(features, NameStringValue);
                string description = GetString(features, DescriptionStringValue);
                string newdat = GetString(features, NewStringValue);
                string olddat = GetString(features, OldStringValue);
                string outdat = GetString(features, OutStringValue);

                // Ensure the output directory
                DirectoryExtensions.Ensure(outdat, create: true);

                // Check that all required files exist
                if (!File.Exists(olddat))
                {
                    Globals.Logger.Error($"File '{olddat}' does not exist!");
                    return;
                }

                if (!File.Exists(newdat))
                {
                    Globals.Logger.Error($"File '{newdat}' does not exist!");
                    return;
                }

                // Create the encapsulating datfile
                DatFile datfile = DatFile.Create();
                datfile.SetName(name);
                datfile.SetDescription(description);

                // Create the inputs
                List<string> dats = new List<string> { newdat };
                List<string> basedats = new List<string> { olddat };

                // Now run the diff on the inputs
                datfile.DetermineUpdateType(dats, basedats, outdat, UpdateMode.DiffAgainst, false /* inplace */, false /* skip */,
                    new Filter(), new List<Field>(), false /* onlySame */);
            }
        }

        private class Dir2DatFeature : RombaSharpFeature
        {
            public const string Value = "Dir2Dat";

            public Dir2DatFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "dir2dat" };
                this.Description = "Creates a DAT file for the specified input directory and saves it to the -out filename.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Creates a DAT file for the specified input directory and saves it to the -out filename.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(OutStringInput);
                AddFeature(SourceStringInput);
                AddFeature(NameStringInput); // Defaults to "untitled"
                AddFeature(DescriptionStringInput);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                string name = GetString(features, NameStringValue);
                string description = GetString(features, DescriptionStringValue);
                string source = GetString(features, SourceStringValue);
                string outdat = GetString(features, OutStringValue);

                // Ensure the output directory
                DirectoryExtensions.Ensure(outdat, create: true);

                // Check that all required directories exist
                if (!Directory.Exists(source))
                {
                    Globals.Logger.Error($"File '{source}' does not exist!");
                    return;
                }

                // Create the encapsulating datfile
                DatFile datfile = DatFile.Create();
                datfile.SetName(string.IsNullOrWhiteSpace(name) ? "untitled" : name);
                datfile.SetDescription(description);

                // Now run the D2D on the input and write out
                // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                datfile.PopulateFromDir(source, Hash.DeepHashes, true /* bare */, false /* archivesAsFiles */, SkipFileType.None, false /* addBlanks */,
                    false /* addDate */, _tmpdir, false /* copyFiles */, null /* headerToCheckAgainst */, true /* chdsAsFiles */, null /* filter */);
                datfile.Write(outDir: outdat);
            }
        }

        private class EDiffdatFeature : RombaSharpFeature
        {
            public const string Value = "EDiffdat";

            public EDiffdatFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "ediffdat" };
                this.Description = "Creates a DAT file with those entries that are in -new DAT.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"Creates a DAT file with those entries that are in -new DAT files and not
in -old DAT files. Ignores those entries in -old that are not in -new.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(OutStringInput);
                AddFeature(OldStringInput);
                AddFeature(NewStringInput);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                string olddat = GetString(features, OldStringValue);
                string outdat = GetString(features, OutStringValue);
                string newdat = GetString(features, NewStringValue);

                // Ensure the output directory
                DirectoryExtensions.Ensure(outdat, create: true);

                // Check that all required files exist
                if (!File.Exists(olddat))
                {
                    Globals.Logger.Error($"File '{olddat}' does not exist!");
                    return;
                }

                if (!File.Exists(newdat))
                {
                    Globals.Logger.Error($"File '{newdat}' does not exist!");
                    return;
                }

                // Create the encapsulating datfile
                DatFile datfile = DatFile.Create();

                // Create the inputs
                List<string> dats = new List<string> { newdat };
                List<string> basedats = new List<string> { olddat };

                // Now run the diff on the inputs
                datfile.DetermineUpdateType(dats, basedats, outdat, UpdateMode.DiffAgainst, false /* inplace */, false /* skip */,
                    new Filter(), new List<Field>(), false /* onlySame */);
            }
        }

        private class ExportFeature : RombaSharpFeature
        {
            public const string Value = "Export";

            // Unique to RombaSharp
            public ExportFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "export" };
                this.Description = "Exports db to export.csv";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Exports db to standardized export.csv";
                this.Features = new Dictionary<string, Feature>();
            }

            // TODO: Add ability to say which depot the files are found in
            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                SqliteConnection dbc = new SqliteConnection(_connectionString);
                dbc.Open();
                StreamWriter sw = new StreamWriter(FileExtensions.TryCreate("export.csv"));

                // First take care of all file hashes
                sw.WriteLine("CRC,MD5,SHA-1"); // ,Depot

                string query = "SELECT crcsha1.crc, md5sha1.md5, md5sha1.sha1 FROM crcsha1 JOIN md5sha1 ON crcsha1.sha1=md5sha1.sha1"; // md5sha1.sha1=sha1depot.sha1
                SqliteCommand slc = new SqliteCommand(query, dbc);
                SqliteDataReader sldr = slc.ExecuteReader();

                if (sldr.HasRows)
                {
                    while (sldr.Read())
                    {
                        string line = $"{sldr.GetString(0)},{sldr.GetString(1)},{sldr.GetString(2)}"; // + ",{sldr.GetString(3)}";
                        sw.WriteLine(line);
                    }
                }

                // Then take care of all DAT hashes
                sw.WriteLine();
                sw.WriteLine("DAT Hash");

                query = "SELECT hash FROM dat";
                slc = new SqliteCommand(query, dbc);
                sldr = slc.ExecuteReader();

                if (sldr.HasRows)
                {
                    while (sldr.Read())
                    {
                        sw.WriteLine(sldr.GetString(0));
                    }
                }

                sldr.Dispose();
                slc.Dispose();
                sw.Dispose();
                dbc.Dispose();
            }
        }

        private class FixdatFeature : RombaSharpFeature
        {
            public const string Value = "Fixdat";

            public FixdatFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "fixdat" };
                this.Description = "For each specified DAT file it creates a fix DAT.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"For each specified DAT file it creates a fix DAT with the missing entries for
that DAT. If nothing is missing it doesn't create a fix DAT for that
particular DAT.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(OutStringInput);
                AddFeature(FixdatOnlyFlag); // Enabled by default
                AddFeature(WorkersInt32Input);
                AddFeature(SubworkersInt32Input);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                // Inputs
                bool fixdatOnly = GetBoolean(features, FixdatOnlyValue);
                int subworkers = GetInt32(features, SubworkersInt32Value);
                int workers = GetInt32(features, WorkersInt32Value);
                string outdat = GetString(features, OutStringValue);

                Globals.Logger.Error("This feature is not yet implemented: fixdat");
            }
        }

        private class HelpFeature : RombaSharpFeature
        {
            public const string Value = "Help";

            public HelpFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "-?", "-h", "--help" };
                this.Description = "Show this help";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Built-in to most of the programs is a basic help text.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override bool ProcessArgs(string[] args, Help help)
            {
                // If we had something else after help
                if (args.Length > 1)
                {
                    help.OutputIndividualFeature(args[1]);
                    return true;
                }

                // Otherwise, show generic help
                else
                {
                    help.OutputGenericHelp();
                    return true;
                }
            }
        }

        private class ImportFeature : RombaSharpFeature
        {
            public const string Value = "Import";

            // Unique to RombaSharp
            public ImportFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "import" };
                this.Description = "Import a database from a formatted CSV file";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Import a database from a formatted CSV file";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                Globals.Logger.Error("This feature is not yet implemented: import");

                // First ensure the inputs and database connection
                Inputs = DirectoryExtensions.GetFilesOnly(Inputs);
                SqliteConnection dbc = new SqliteConnection(_connectionString);
                SqliteCommand slc = new SqliteCommand();
                dbc.Open();

                // Now, for each of these files, attempt to add the data found inside
                foreach (string input in Inputs)
                {
                    StreamReader sr = new StreamReader(FileExtensions.TryOpenRead(input));

                    // The first line should be the hash header
                    string line = sr.ReadLine();
                    if (line != "CRC,MD5,SHA-1") // ,Depot
                    {
                        Globals.Logger.Error("{0} is not a valid export file");
                        continue;
                    }

                    // Define the insert queries
                    string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
                    string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
                    string sha1query = "INSERT OR IGNORE INTO sha1 (sha1) VALUES";
                    string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
                    string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

                    // For each line until we hit a blank line...
                    while (!sr.EndOfStream && line != string.Empty)
                    {
                        line = sr.ReadLine();
                        string[] hashes = line.Split(',');

                        // Loop through the parsed entries
                        if (!string.IsNullOrWhiteSpace(hashes[0]))
                            crcquery += $" (\"{hashes[0]}\"),";

                        if (!string.IsNullOrWhiteSpace(hashes[1]))
                            md5query += $" (\"{hashes[1]}\"),";

                        if (!string.IsNullOrWhiteSpace(hashes[2]))
                        {
                            sha1query += $" (\"{hashes[2]}\"),";

                            if (!string.IsNullOrWhiteSpace(hashes[0]))
                                crcsha1query += $" (\"{hashes[0]}\", \"{hashes[2]}\"),";

                            if (!string.IsNullOrWhiteSpace(hashes[1]))
                                md5sha1query += $" (\"{hashes[1]}\", \"{hashes[2]}\"),";
                        }
                    }

                    // Now run the queries after fixing them
                    if (crcquery != "INSERT OR IGNORE INTO crc (crc) VALUES")
                    {
                        slc = new SqliteCommand(crcquery.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    if (md5query != "INSERT OR IGNORE INTO md5 (md5) VALUES")
                    {
                        slc = new SqliteCommand(md5query.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    if (sha1query != "INSERT OR IGNORE INTO sha1 (sha1) VALUES")
                    {
                        slc = new SqliteCommand(sha1query.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    if (crcsha1query != "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES")
                    {
                        slc = new SqliteCommand(crcsha1query.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    if (md5sha1query != "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES")
                    {
                        slc = new SqliteCommand(md5sha1query.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    // Now add all of the DAT hashes
                    // TODO: Do we really need to save the DAT hashes?

                    sr.Dispose();
                }

                slc.Dispose();
                dbc.Dispose();
            }
        }

        private class LookupFeature : RombaSharpFeature
        {
            public const string Value = "Lookup";

            public LookupFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "lookup" };
                this.Description = "For each specified hash it looks up any available information.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "For each specified hash it looks up any available information (dat or rom).";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(SizeInt64Input); // Defaults to -1
                AddFeature(OutStringInput);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                long size = GetInt64(features, SizeInt64Value);
                string outdat = GetString(features, OutStringValue);

                // First, try to figure out what type of hash each is by length and clean it
                List<string> crc = new List<string>();
                List<string> md5 = new List<string>();
                List<string> sha1 = new List<string>();
                foreach (string input in Inputs)
                {
                    string temp = string.Empty;
                    if (input.Length == Constants.CRCLength)
                    {
                        temp = Sanitizer.CleanCRC32(input);
                        if (!string.IsNullOrWhiteSpace(temp))
                        {
                            crc.Add(temp);
                        }
                    }
                    else if (input.Length == Constants.MD5Length)
                    {
                        temp = Sanitizer.CleanMD5(input);
                        if (!string.IsNullOrWhiteSpace(temp))
                        {
                            md5.Add(temp);
                        }
                    }
                    else if (input.Length == Constants.SHA1Length)
                    {
                        temp = Sanitizer.CleanSHA1(input);
                        if (!string.IsNullOrWhiteSpace(temp))
                        {
                            sha1.Add(temp);
                        }
                    }
                }

                SqliteConnection dbc = new SqliteConnection(_connectionString);
                dbc.Open();

                // Now, search for each of them and return true or false for each
                foreach (string input in crc)
                {
                    string query = $"SELECT * FROM crc WHERE crc=\"{input}\"";
                    SqliteCommand slc = new SqliteCommand(query, dbc);
                    SqliteDataReader sldr = slc.ExecuteReader();
                    if (sldr.HasRows)
                    {
                        int count = 0;
                        while (sldr.Read())
                        {
                            count++;
                        }

                        Globals.Logger.User($"For hash '{input}' there were {count} matches in the database");
                    }
                    else
                    {
                        Globals.Logger.User($"Hash '{input}' had no matches in the database");
                    }

                    sldr.Dispose();
                    slc.Dispose();
                }
                foreach (string input in md5)
                {
                    string query = $"SELECT * FROM md5 WHERE md5=\"{input}\"";
                    SqliteCommand slc = new SqliteCommand(query, dbc);
                    SqliteDataReader sldr = slc.ExecuteReader();
                    if (sldr.HasRows)
                    {
                        int count = 0;
                        while (sldr.Read())
                        {
                            count++;
                        }

                        Globals.Logger.User($"For hash '{input}' there were {count} matches in the database");
                    }
                    else
                    {
                        Globals.Logger.User($"Hash '{input}' had no matches in the database");
                    }

                    sldr.Dispose();
                    slc.Dispose();
                }
                foreach (string input in sha1)
                {
                    string query = $"SELECT * FROM sha1 WHERE sha1=\"{input}\"";
                    SqliteCommand slc = new SqliteCommand(query, dbc);
                    SqliteDataReader sldr = slc.ExecuteReader();
                    if (sldr.HasRows)
                    {
                        int count = 0;
                        while (sldr.Read())
                        {
                            count++;
                        }

                        Globals.Logger.User($"For hash '{input}' there were {count} matches in the database");
                    }
                    else
                    {
                        Globals.Logger.User($"Hash '{input}' had no matches in the database");
                    }

                    sldr.Dispose();
                    slc.Dispose();
                }

                dbc.Dispose();
            }
        }

        private class MemstatsFeature : RombaSharpFeature
        {
            public const string Value = "Memstats";

            public MemstatsFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "memstats" };
                this.Description = "Prints memory stats.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Print memory stats.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                Globals.Logger.User("This feature is not yet implemented: memstats");
            }
        }

        private class MergeFeature : RombaSharpFeature
        {
            public const string Value = "Merge";

            public MergeFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "merge" };
                this.Description = "Merges depot";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Merges specified depot into current depot.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(OnlyNeededFlag);
                AddFeature(ResumeStringInput);
                AddFeature(WorkersInt32Input);
                AddFeature(SkipInitialScanFlag);
            }

            // TODO: Add way of specifying "current depot" since that's what Romba relies on
            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                bool onlyNeeded = GetBoolean(features, OnlyNeededValue);
                bool skipInitialscan = GetBoolean(features, SkipInitialScanValue);
                int workers = GetInt32(features, WorkersInt32Value);
                string resume = GetString(features, ResumeStringValue);

                Globals.Logger.Error("This feature is not yet implemented: merge");

                // Verify that the inputs are valid directories
                Inputs = DirectoryExtensions.GetDirectoriesOnly(Inputs);

                // Loop over all input directories
                foreach (string input in Inputs)
                {
                    List<string> depotFiles = Directory.EnumerateFiles(input, "*.gz", SearchOption.AllDirectories).ToList();

                    // If we are copying all that is possible but we want to scan first
                    if (!onlyNeeded && !skipInitialscan)
                    {

                    }

                    // If we are copying all that is possible but we don't care to scan first
                    else if (!onlyNeeded && skipInitialscan)
                    {

                    }

                    // If we are copying only what is needed but we want to scan first
                    else if (onlyNeeded && !skipInitialscan)
                    {

                    }

                    // If we are copying only what is needed but we don't care to scan first
                    else if (onlyNeeded && skipInitialscan)
                    {

                    }
                }
            }
        }

        private class MissFeature : RombaSharpFeature
        {
            public const string Value = "Miss";

            // Unique to RombaSharp
            public MissFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "miss" };
                this.Description = "Create miss and have file";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "For each specified DAT file, create miss and have file";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Verify the filenames
                Dictionary<string, string> foundDats = GetValidDats(Inputs);

                // Create the new output directory if it doesn't exist
                DirectoryExtensions.Ensure(Path.Combine(Globals.ExeDir, "out"), create: true);

                // Now that we have the dictionary, we can loop through and output to a new folder for each
                foreach (string key in foundDats.Keys)
                {
                    // Get the DAT file associated with the key
                    DatFile datFile = DatFile.CreateAndParse(Path.Combine(_dats, foundDats[key]));

                    // Now loop through and see if all of the hash combinations exist in the database
                    /* ended here */
                }

                Globals.Logger.Error("This feature is not yet implemented: miss");
            }
        }

        private class ProgressFeature : RombaSharpFeature
        {
            public const string Value = "Progress";

            public ProgressFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "progress" };
                this.Description = "Shows progress of the currently running command.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Shows progress of the currently running command.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                Globals.Logger.User("This feature is not yet implemented: progress");
            }
        }

        private class PurgeBackupFeature : RombaSharpFeature
        {
            public const string Value = "Purge Backup";

            public PurgeBackupFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "purge-backup" };
                this.Description = "Moves DAT index entries for orphaned DATs.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"Deletes DAT index entries for orphaned DATs and moves ROM files that are no
longer associated with any current DATs to the specified backup folder.
The files will be placed in the backup location using
a folder structure according to the original DAT master directory tree
structure. It also deletes the specified DATs from the DAT index.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(BackupStringInput);
                AddFeature(WorkersInt32Input);
                AddFeature(DepotListStringInput);
                AddFeature(DatsListStringInput);
                AddFeature(LogOnlyFlag);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                bool logOnly = GetBoolean(features, LogOnlyValue);
                int workers = GetInt32(features, WorkersInt32Value);
                string backup = GetString(features, BackupStringValue);
                List<string> dats = GetList(features, DatsListStringValue);
                List<string> depot = GetList(features, DepotListStringValue);

                Globals.Logger.Error("This feature is not yet implemented: purge-backup");
            }
        }

        private class PurgeDeleteFeature : RombaSharpFeature
        {
            public const string Value = "Purge Delete";

            // Unique to RombaSharp
            public PurgeDeleteFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "purge-delete" };
                this.Description = "Deletes DAT index entries for orphaned DATs";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"Deletes DAT index entries for orphaned DATs and moves ROM files that are no
longer associated with any current DATs to the specified backup folder.
The files will be placed in the backup location using
a folder structure according to the original DAT master directory tree
structure. It also deletes the specified DATs from the DAT index.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(WorkersInt32Input);
                AddFeature(DepotListStringInput);
                AddFeature(DatsListStringInput);
                AddFeature(LogOnlyFlag);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                bool logOnly = GetBoolean(features, LogOnlyValue);
                int workers = GetInt32(features, WorkersInt32Value);
                List<string> dats = GetList(features, DatsListStringValue);
                List<string> depot = GetList(features, DepotListStringValue);

                Globals.Logger.Error("This feature is not yet implemented: purge-delete");
            }
        }

        private class RefreshDatsFeature : RombaSharpFeature
        {
            public const string Value = "Refresh DATs";

            public RefreshDatsFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "refresh-dats" };
                this.Description = "Refreshes the DAT index from the files in the DAT master directory tree.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = @"Refreshes the DAT index from the files in the DAT master directory tree.
Detects any changes in the DAT master directory tree and updates the DAT index
accordingly, marking deleted or overwritten dats as orphaned and updating
contents of any changed dats.";
                this.Features = new Dictionary<string, Feature>();

                AddFeature(WorkersInt32Input);
                AddFeature(MissingSha1sStringInput);
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                // Get feature flags
                int workers = GetInt32(features, WorkersInt32Value);
                string missingSha1s = GetString(features, MissingSha1sStringValue);

                // Make sure the db is set
                if (string.IsNullOrWhiteSpace(_db))
                {
                    _db = "db.sqlite";
                    _connectionString = $"Data Source={_db};Version = 3;";
                }

                // Make sure the file exists
                if (!File.Exists(_db))
                    DatabaseTools.EnsureDatabase(_dbSchema, _db, _connectionString);

                // Make sure the dats dir is set
                if (string.IsNullOrWhiteSpace(_dats))
                    _dats = "dats";

                _dats = Path.Combine(Globals.ExeDir, _dats);

                // Make sure the folder exists
                if (!Directory.Exists(_dats))
                    Directory.CreateDirectory(_dats);

                // First get a list of SHA-1's from the input DATs
                DatFile datroot = DatFile.Create();
                datroot.SetType("SuperDAT");

                // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                datroot.PopulateFromDir(_dats, Hash.DeepHashes, false, false, SkipFileType.None, false, false, _tmpdir, false, null, true, null);
                datroot.BucketBy(BucketedBy.SHA1, DedupeType.None);

                // Create a List of dat hashes in the database (SHA-1)
                List<string> databaseDats = new List<string>();
                List<string> unneeded = new List<string>();

                SqliteConnection dbc = new SqliteConnection(_connectionString);
                dbc.Open();

                // Populate the List from the database
                InternalStopwatch watch = new InternalStopwatch("Populating the list of existing DATs");

                string query = "SELECT DISTINCT hash FROM dat";
                SqliteCommand slc = new SqliteCommand(query, dbc);
                SqliteDataReader sldr = slc.ExecuteReader();
                if (sldr.HasRows)
                {
                    sldr.Read();
                    string hash = sldr.GetString(0);
                    if (datroot.Contains(hash))
                    {
                        datroot.Remove(hash);
                        databaseDats.Add(hash);
                    }
                    else if (!databaseDats.Contains(hash))
                    {
                        unneeded.Add(hash);
                    }
                }
                datroot.BucketBy(BucketedBy.Game, DedupeType.None, norename: true);

                watch.Stop();

                slc.Dispose();
                sldr.Dispose();

                // Loop through the Dictionary and add all data
                watch.Start("Adding new DAT information");
                foreach (string key in datroot.Keys)
                {
                    foreach (Rom value in datroot[key])
                    {
                        AddDatToDatabase(value, dbc);
                    }
                }

                watch.Stop();

                // Now loop through and remove all references to old Dats
                if (unneeded.Count > 0)
                {
                    watch.Start("Removing unmatched DAT information");

                    query = "DELETE FROM dat WHERE";
                    foreach (string dathash in unneeded)
                    {
                        query += $" OR hash=\"{dathash}\"";
                    }

                    query = query.Replace("WHERE OR", "WHERE");
                    slc = new SqliteCommand(query, dbc);
                    slc.ExecuteNonQuery();
                    slc.Dispose();

                    watch.Stop();
                }

                dbc.Dispose();
            }
        }

        private class RescanDepotsFeature : RombaSharpFeature
        {
            public const string Value = "Rescan Depots";

            // Unique to RombaSharp
            public RescanDepotsFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "depot-rescan" };
                this.Description = "Rescan a specific depot to get new information";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Rescan a specific depot to get new information";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                Globals.Logger.Error("This feature is not yet implemented: rescan-depots");

                foreach (string depotname in Inputs)
                {
                    // Check that it's a valid depot first
                    if (!_depots.ContainsKey(depotname))
                    {
                        Globals.Logger.User($"'{depotname}' is not a recognized depot. Please add it to your configuration file and try again");
                        return;
                    }

                    // Then check that the depot is online
                    if (!Directory.Exists(depotname))
                    {
                        Globals.Logger.User($"'{depotname}' does not appear to be online. Please check its status and try again");
                        return;
                    }

                    // Open the database connection
                    SqliteConnection dbc = new SqliteConnection(_connectionString);
                    dbc.Open();

                    // If we have it, then check for all hashes that are in that depot
                    List<string> hashes = new List<string>();
                    string query = $"SELECT sha1 FROM sha1 WHERE depot=\"{depotname}\"";
                    SqliteCommand slc = new SqliteCommand(query, dbc);
                    SqliteDataReader sldr = slc.ExecuteReader();
                    if (sldr.HasRows)
                    {
                        while (sldr.Read())
                        {
                            hashes.Add(sldr.GetString(0));
                        }
                    }

                    // Now rescan the depot itself
                    DatFile depot = DatFile.Create();
                    // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                    depot.PopulateFromDir(depotname, Hash.DeepHashes, false, false, SkipFileType.None, false, false, _tmpdir, false, null, true, null);
                    depot.BucketBy(BucketedBy.SHA1, DedupeType.None);

                    // Set the base queries to use
                    string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
                    string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
                    string sha1query = "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES";
                    string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
                    string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

                    // Once we have both, check for any new files
                    List<string> dupehashes = new List<string>();
                    List<string> keys = depot.Keys;
                    foreach (string key in keys)
                    {
                        List<DatItem> roms = depot[key];
                        foreach (Rom rom in roms)
                        {
                            if (hashes.Contains(rom.SHA1))
                            {
                                dupehashes.Add(rom.SHA1);
                                hashes.Remove(rom.SHA1);
                            }
                            else if (!dupehashes.Contains(rom.SHA1))
                            {
                                if (!string.IsNullOrWhiteSpace(rom.CRC))
                                    crcquery += $" (\"{rom.CRC}\"),";

                                if (!string.IsNullOrWhiteSpace(rom.MD5))
                                    md5query += $" (\"{rom.MD5}\"),";

                                if (!string.IsNullOrWhiteSpace(rom.SHA1))
                                {
                                    sha1query += $" (\"{rom.SHA1}\", \"{depotname}\"),";

                                    if (!string.IsNullOrWhiteSpace(rom.CRC))
                                        crcsha1query += $" (\"{rom.CRC}\", \"{rom.SHA1}\"),";

                                    if (!string.IsNullOrWhiteSpace(rom.MD5))
                                        md5sha1query += $" (\"{rom.MD5}\", \"{rom.SHA1}\"),";
                                }
                            }
                        }
                    }

                    // Now run the queries after fixing them
                    if (crcquery != "INSERT OR IGNORE INTO crc (crc) VALUES")
                    {
                        slc = new SqliteCommand(crcquery.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    if (md5query != "INSERT OR IGNORE INTO md5 (md5) VALUES")
                    {
                        slc = new SqliteCommand(md5query.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    if (sha1query != "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES")
                    {
                        slc = new SqliteCommand(sha1query.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    if (crcsha1query != "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES")
                    {
                        slc = new SqliteCommand(crcsha1query.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    if (md5sha1query != "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES")
                    {
                        slc = new SqliteCommand(md5sha1query.TrimEnd(','), dbc);
                        slc.ExecuteNonQuery();
                    }

                    // Now that we've added the information, we get to remove all of the hashes that we want to
                    query = @"DELETE FROM sha1
JOIN crcsha1
    ON sha1.sha1=crcsha1.sha1
JOIN md5sha1
    ON sha1.sha1=md5sha1.sha1
JOIN crc
    ON crcsha1.crc=crc.crc
JOIN md5
    ON md5sha1.md5=md5.md5
WHERE sha1.sha1 IN ";
                    query += $"({string.Join("\",\"", hashes)}\")";
                    slc = new SqliteCommand(query, dbc);
                    slc.ExecuteNonQuery();

                    // Dispose of the database connection
                    slc.Dispose();
                    dbc.Dispose();
                }
            }
        }

        private class ScriptFeature : RombaSharpFeature
        {
            public const string Value = "Script";

            public ScriptFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "--script" };
                this.Description = "Enable script mode (no clear screen)";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "For times when RombaSharp is being used in a scripted environment, the user may not want the screen to be cleared every time that it is called. This flag allows the user to skip clearing the screen on run just like if the console was being redirected.";
                this.Features = new Dictionary<string, Feature>();
            }
        }

        private class ShutdownFeature : RombaSharpFeature
        {
            public const string Value = "Shutdown";

            public ShutdownFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "shutdown" };
                this.Description = "Gracefully shuts down server.";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Gracefully shuts down server saving all the cached data.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                Globals.Logger.User("This feature is not yet implemented: shutdown");
            }
        }

        private class VersionFeature : RombaSharpFeature
        {
            public const string Value = "Version";

            public VersionFeature()
            {
                this.Name = Value;
                this.Flags = new List<string>() { "version" };
                this.Description = "Prints version";
                this._featureType = FeatureType.Flag;
                this.LongDescription = "Prints version.";
                this.Features = new Dictionary<string, Feature>();
            }

            public override void ProcessFeatures(Dictionary<string, Feature> features)
            {
                Globals.Logger.User($"RombaSharp version: {Constants.Version}");
            }
        }

        #endregion
    }
}
