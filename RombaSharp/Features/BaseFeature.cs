using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Help;
using SabreTools.Library.IO;
using SabreTools.Library.Tools;
using Microsoft.Data.Sqlite;

namespace RombaSharp.Features
{
    internal class BaseFeature : TopLevel
    {
        #region Private Flag features

        internal const string CopyValue = "copy";
        internal static Feature CopyFlag
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

        internal const string FixdatOnlyValue = "fixdat-only";
        internal static Feature FixdatOnlyFlag
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

        internal const string LogOnlyValue = "log-only";
        internal static Feature LogOnlyFlag
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

        internal const string NoDbValue = "no-db";
        internal static Feature NoDbFlag
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

        internal const string OnlyNeededValue = "only-needed";
        internal static Feature OnlyNeededFlag
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

        internal const string SkipInitialScanValue = "skip-initial-scan";
        internal static Feature SkipInitialScanFlag
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

        internal const string UseGolangZipValue = "use-golang-zip";
        internal static Feature UseGolangZipFlag
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

        internal const string Include7ZipsInt32Value = "include-7zips";
        internal static Feature Include7ZipsInt32Input
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

        internal const string IncludeGZipsInt32Value = "include-gzips";
        internal static Feature IncludeGZipsInt32Input
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

        internal const string IncludeZipsInt32Value = "include-zips";
        internal static Feature IncludeZipsInt32Input
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

        internal const string SubworkersInt32Value = "subworkers";
        internal static Feature SubworkersInt32Input
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

        internal const string WorkersInt32Value = "workers";
        internal static Feature WorkersInt32Input
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

        internal const string SizeInt64Value = "size";
        internal static Feature SizeInt64Input
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

        internal const string DatsListStringValue = "dats";
        internal static Feature DatsListStringInput
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

        internal const string DepotListStringValue = "depot";
        internal static Feature DepotListStringInput
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

        internal const string BackupStringValue = "backup";
        internal static Feature BackupStringInput
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

        internal const string DescriptionStringValue = "description";
        internal static Feature DescriptionStringInput
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

        internal const string MissingSha1sStringValue = "missing-sha1s";
        internal static Feature MissingSha1sStringInput
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

        internal const string NameStringValue = "name";
        internal static Feature NameStringInput
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

        internal const string NewStringValue = "new";
        internal static Feature NewStringInput
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

        internal const string OldStringValue = "old";
        internal static Feature OldStringInput
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

        internal const string OutStringValue = "out";
        internal static Feature OutStringInput
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

        internal const string ResumeStringValue = "resume";
        internal static Feature ResumeStringInput
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

        internal const string SourceStringValue = "source";
        internal static Feature SourceStringInput
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

        // General settings
        internal static string _logdir;		// Log folder location
        internal static string _webdir;		// Web frontend location
        internal static string _baddir;		// Fail-to-unpack file folder location
        internal static int _verbosity;		// Verbosity of the output
        internal static int _cores;			// Forced CPU cores

        // DatRoot settings
        internal static string _dats;		// DatRoot folder location
        internal static string _db;			// Database name

        // Depot settings
        internal static Dictionary<string, Tuple<long, bool>> _depots; // Folder location, Max size

        // Server settings
        internal static int _port;			// Web server port

        // Other internal variables
        internal const string _config = "config.xml";
        internal const string _dbSchema = "rombasharp";
        internal static string _connectionString;

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            InitializeConfiguration();
            DatabaseTools.EnsureDatabase(_dbSchema, _db, _connectionString);
        }

        #region Helper methods

        /// <summary>
        /// Gets all valid DATs that match in the DAT root
        /// </summary>
        /// <param name="inputs">List of input strings to check for, presumably file names</param>
        /// <returns>Dictionary of hash/full path for each of the valid DATs</returns>
        internal static Dictionary<string, string> GetValidDats(List<string> inputs)
        {
            // Get a dictionary of filenames that actually exist in the DATRoot, logging which ones are not
            List<string> datRootDats = Directory.EnumerateFiles(_dats, "*", SearchOption.AllDirectories).ToList();
            List<string> lowerCaseDats = datRootDats.ConvertAll(i => Path.GetFileName(i).ToLowerInvariant());
            Dictionary<string, string> foundDats = new Dictionary<string, string>();
            foreach (string input in inputs)
            {
                if (lowerCaseDats.Contains(input.ToLowerInvariant()))
                {
                    string fullpath = Path.GetFullPath(datRootDats[lowerCaseDats.IndexOf(input.ToLowerInvariant())]);
                    string sha1 = Utilities.ByteArrayToString(FileExtensions.GetInfo(fullpath).SHA1);
                    foundDats.Add(sha1, fullpath);
                }
                else
                {
                    Globals.Logger.Warning($"The file '{input}' could not be found in the DAT root");
                }
            }

            return foundDats;
        }

        /// <summary>
        /// Initialize the Romba application from XML config
        /// </summary>
        private static void InitializeConfiguration()
        {
            // Get default values if they're not written
            int workers = 4,
                verbosity = 1,
                cores = 4,
                port = 4003;
            string logdir = "logs",
                tmpdir = "tmp",
                webdir = "web",
                baddir = "bad",
                dats = "dats",
                db = "db";
            Dictionary<string, Tuple<long, bool>> depots = new Dictionary<string, Tuple<long, bool>>();

            // Get the XML text reader for the configuration file, if possible
            XmlReader xtr = _config.GetXmlTextReader();

            // Now parse the XML file for settings
            if (xtr != null)
            {
                xtr.MoveToContent();
                while (!xtr.EOF)
                {
                    // We only want elements
                    if (xtr.NodeType != XmlNodeType.Element)
                    {
                        xtr.Read();
                        continue;
                    }

                    switch (xtr.Name)
                    {
                        case "workers":
                            workers = xtr.ReadElementContentAsInt();
                            break;
                        case "logdir":
                            logdir = xtr.ReadElementContentAsString();
                            break;
                        case "tmpdir":
                            tmpdir = xtr.ReadElementContentAsString();
                            break;
                        case "webdir":
                            webdir = xtr.ReadElementContentAsString();
                            break;
                        case "baddir":
                            baddir = xtr.ReadElementContentAsString();
                            break;
                        case "verbosity":
                            verbosity = xtr.ReadElementContentAsInt();
                            break;
                        case "cores":
                            cores = xtr.ReadElementContentAsInt();
                            break;
                        case "dats":
                            dats = xtr.ReadElementContentAsString();
                            break;
                        case "db":
                            db = xtr.ReadElementContentAsString();
                            break;
                        case "depot":
                            XmlReader subreader = xtr.ReadSubtree();
                            if (subreader != null)
                            {
                                string root = string.Empty;
                                long maxsize = -1;
                                bool online = true;

                                while (!subreader.EOF)
                                {
                                    // We only want elements
                                    if (subreader.NodeType != XmlNodeType.Element)
                                    {
                                        subreader.Read();
                                        continue;
                                    }

                                    switch (subreader.Name)
                                    {
                                        case "root":
                                            root = subreader.ReadElementContentAsString();
                                            break;
                                        case "maxsize":
                                            maxsize = subreader.ReadElementContentAsLong();
                                            break;
                                        case "online":
                                            online = subreader.ReadElementContentAsBoolean();
                                            break;
                                        default:
                                            subreader.Read();
                                            break;
                                    }
                                }

                                try
                                {
                                    depots.Add(root, new Tuple<long, bool>(maxsize, online));
                                }
                                catch
                                {
                                    // Ignore add errors
                                }
                            }

                            xtr.Skip();
                            break;
                        case "port":
                            port = xtr.ReadElementContentAsInt();
                            break;
                        default:
                            xtr.Read();
                            break;
                    }
                }
            }

            // Now validate the values given
            if (workers < 1)
                workers = 1;
            if (workers > 8)
                workers = 8;

            if (!Directory.Exists(logdir))
                Directory.CreateDirectory(logdir);

            if (!Directory.Exists(tmpdir))
                Directory.CreateDirectory(tmpdir);

            if (!Directory.Exists(webdir))
                Directory.CreateDirectory(webdir);

            if (!Directory.Exists(baddir))
                Directory.CreateDirectory(baddir);

            if (verbosity < 0)
                verbosity = 0;

            if (verbosity > 3)
                verbosity = 3;

            if (cores < 1)
                cores = 1;

            if (cores > 16)
                cores = 16;

            if (!Directory.Exists(dats))
                Directory.CreateDirectory(dats);

            db = $"{Path.GetFileNameWithoutExtension(db)}.sqlite";
            string connectionString = $"Data Source={db};Version = 3;";
            foreach (string key in depots.Keys)
            {
                if (!Directory.Exists(key))
                {
                    Directory.CreateDirectory(key);
                    File.CreateText(Path.Combine(key, ".romba_size"));
                    File.CreateText(Path.Combine(key, ".romba_size.backup"));
                }
                else
                {
                    if (!File.Exists(Path.Combine(key, ".romba_size")))
                        File.CreateText(Path.Combine(key, ".romba_size"));

                    if (!File.Exists(Path.Combine(key, ".romba_size.backup")))
                        File.CreateText(Path.Combine(key, ".romba_size.backup"));
                }
            }

            if (port < 0)
                port = 0;

            if (port > 65535)
                port = 65535;

            // Finally set all of the fields
            Globals.MaxThreads = workers;
            _logdir = logdir;
            Globals.TempDir = tmpdir;
            _webdir = webdir;
            _baddir = baddir;
            _verbosity = verbosity;
            _cores = cores;
            _dats = dats;
            _db = db;
            _connectionString = connectionString;
            _depots = depots;
            _port = port;
        }

        /// <summary>
        /// Add a new DAT to the database
        /// </summary>
        /// <param name="dat">DatFile hash information to add</param>
        /// <param name="dbc">Database connection to use</param>
        internal static void AddDatToDatabase(Rom dat, SqliteConnection dbc)
        {
            // Get the dat full path
            string fullpath = Path.Combine(_dats, (dat.Machine.Name == "dats" ? string.Empty : dat.Machine.Name), dat.Name);

            // Parse the Dat if possible
            Globals.Logger.User($"Adding from '{dat.Name}'");
            DatFile tempdat = DatFile.CreateAndParse(fullpath);

            // If the Dat wasn't empty, add the information
            SqliteCommand slc = null;
            string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
            string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
            string sha1query = "INSERT OR IGNORE INTO sha1 (sha1) VALUES";
            string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
            string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

            // Loop through the parsed entries
            bool hasItems = false;
            foreach (string romkey in tempdat.Items.Keys)
            {
                foreach (DatItem datItem in tempdat.Items[romkey])
                {
                    Globals.Logger.Verbose($"Checking and adding file '{datItem.Name}'");

                    if (datItem.ItemType == ItemType.Disk)
                    {
                        Disk disk = (Disk)datItem;
                        hasItems = true;

                        if (!string.IsNullOrWhiteSpace(disk.MD5))
                            md5query += $" (\"{disk.MD5}\"),";

                        if (!string.IsNullOrWhiteSpace(disk.SHA1))
                        {
                            sha1query += $" (\"{disk.SHA1}\"),";

                            if (!string.IsNullOrWhiteSpace(disk.MD5))
                                md5sha1query += $" (\"{disk.MD5}\", \"{disk.SHA1}\"),";
                        }
                    }
                    else if (datItem.ItemType == ItemType.Media)
                    {
                        Media media = (Media)datItem;
                        hasItems = true;

                        if (!string.IsNullOrWhiteSpace(media.MD5))
                            md5query += $" (\"{media.MD5}\"),";

                        if (!string.IsNullOrWhiteSpace(media.SHA1))
                        {
                            sha1query += $" (\"{media.SHA1}\"),";

                            if (!string.IsNullOrWhiteSpace(media.MD5))
                                md5sha1query += $" (\"{media.MD5}\", \"{media.SHA1}\"),";
                        }
                    }
                    else if (datItem.ItemType == ItemType.Rom)
                    {
                        Rom rom = (Rom)datItem;
                        hasItems = true;

                        if (!string.IsNullOrWhiteSpace(rom.CRC))
                            crcquery += $" (\"{rom.CRC}\"),";

                        if (!string.IsNullOrWhiteSpace(rom.MD5))
                            md5query += $" (\"{rom.MD5}\"),";

                        if (!string.IsNullOrWhiteSpace(rom.SHA1))
                        {
                            sha1query += $" (\"{rom.SHA1}\"),";

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

            // Only add the DAT if it's non-empty
            if (hasItems)
            {
                string datquery = $"INSERT OR IGNORE INTO dat (hash) VALUES (\"{dat.SHA1}\")";
                slc = new SqliteCommand(datquery, dbc);
                slc.ExecuteNonQuery();
            }

            slc?.Dispose();
        }

        #endregion
    }
}
