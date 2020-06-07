using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using SearchOption = System.IO.SearchOption;
using StreamReader = System.IO.StreamReader;
using StreamWriter = System.IO.StreamWriter;
#endif

namespace RombaSharp
{
    public partial class RombaSharp
    {
        #region Init Methods

        /// <summary>
        /// Wrap adding files to the depots
        /// </summary>
        /// <param name="inputs">List of input folders to use</param>
        /// <param name="onlyNeeded">True if only files in the database and don't exist are added, false otherwise</param>
        /// <param name="resume">Resume a previously interrupted operation from the specified path</param>
        /// <param name="includeZips">flag value == 0 means: add Zip files themselves into the depot in addition to their contents, flag value == 2 means add Zip files themselves but don't add content</param>
        /// <param name="workers">How many workers to launch for the job, default from config</param>
        /// <param name="includeGZips">flag value == 0 means: add GZip files themselves into the depot in addition to their contents, flag value == 2 means add GZip files themselves but don't add content</param>
        /// <param name="include7Zips">flag value == 0 means: add 7Zip files themselves into the depot in addition to their contents, flag value == 2 means add 7Zip files themselves but don't add content</param>
        /// <param name="skipInitialScan">True to skip the initial scan of the files to determine amount of work, false otherwise</param>
        /// <param name="useGolangZip">True to use go zip implementation instead of zlib, false otherwise</param>
        /// <param name="noDb">True to archive into depot but do not touch DB index and ignore only-needed flag, false otherwise</param>
        /// TODO: Add ability to update .romba files with proper size AND use the correct depot if it fills up
        /// TODO: Add ability correctly to mark which depot the files are being rebuilt to in the DB
        private static void InitArchive(
            List<string> inputs,
            bool onlyNeeded,
            string resume,
            int includeZips,
            int workers,
            int includeGZips,
            int include7Zips,
            bool skipInitialScan,
            bool useGolangZip, // Obsolete
            bool noDb)
        {
            // First we want to get just all directories from the inputs
            List<string> onlyDirs = new List<string>();
            foreach (string input in inputs)
            {
                if (Directory.Exists(input))
                    onlyDirs.Add(Path.GetFullPath(input));
            }

            // Then process all of the input directories into an internal DAT
            DatFile df = new DatFile();
            foreach (string dir in onlyDirs)
            {
                // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                df.PopulateFromDir(dir, Hash.DeepHashes, false, false, SkipFileType.None, false, false, _tmpdir, false, null, true, null);
                df.PopulateFromDir(dir, Hash.DeepHashes, false, true, SkipFileType.None, false, false, _tmpdir, false, null, true, null);
            }

            // Create an empty Dat for files that need to be rebuilt
            DatFile need = new DatFile();

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
                                    + " WHERE crcsha1.crc=\"" + rom.CRC + "\""
                                    + " OR md5sha1.md5=\"" + rom.MD5 + "\""
                                    + " OR md5sha1.sha1=\"" + rom.SHA1 + "\"";
                        SqliteCommand slc = new SqliteCommand(query, dbc);
                        SqliteDataReader sldr = slc.ExecuteReader();
                        
                        if (sldr.HasRows)
                        {
                            // Add to the queries
                            if (!string.IsNullOrWhiteSpace(rom.CRC))
                                crcquery += " (\"" + rom.CRC + "\"),";

                            if (!string.IsNullOrWhiteSpace(rom.MD5))
                                md5query += " (\"" + rom.MD5 + "\"),";
                            
                            if (!string.IsNullOrWhiteSpace(rom.SHA1))
                            {
                                sha1query += " (\"" + rom.SHA1 + "\", \"" + _depots.Keys.ToList()[0] + "\"),";

                                if (!string.IsNullOrWhiteSpace(rom.CRC))
                                    crcsha1query += " (\"" + rom.CRC + "\", \"" + rom.SHA1 + "\"),";

                                if (!string.IsNullOrWhiteSpace(rom.MD5))
                                    md5sha1query += " (\"" + rom.MD5 + "\", \"" + rom.SHA1 + "\"),";
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
                                crcquery += " (\"" + rom.CRC + "\"),";

                            if (!string.IsNullOrWhiteSpace(rom.MD5))
                                md5query += " (\"" + rom.MD5 + "\"),";

                            if (!string.IsNullOrWhiteSpace(rom.SHA1))
                            {
                                sha1query += " (\"" + rom.SHA1 + "\", \"" + _depots.Keys.ToList()[0] + "\"),";

                                if (!string.IsNullOrWhiteSpace(rom.CRC))
                                    crcsha1query += " (\"" + rom.CRC + "\", \"" + rom.SHA1 + "\"),";

                                if (!string.IsNullOrWhiteSpace(rom.MD5))
                                    md5sha1query += " (\"" + rom.MD5 + "\", \"" + rom.SHA1 + "\"),";
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
            ArchiveScanLevel asl = Utilities.GetArchiveScanLevelFromNumbers(include7Zips, includeGZips, 2, includeZips);
            need.RebuildGeneric(onlyDirs, _depots.Keys.ToList()[0], false /*quickScan*/, false /*date*/,
                false /*delete*/, false /*inverse*/, OutputFormat.TorrentGzipRomba, asl, false /*updateDat*/,
                null /*headerToCheckAgainst*/, true /* chdsAsFiles */);
        }

        /// <summary>
        /// Wrap building all files from a set of DATs
        /// </summary>
        /// <param name="inputs">List of input DATs to rebuild from</param>
        /// <param name="outdat">Output file</param>
        /// <paran name="fixdatOnly">True to only fix dats and don't generate torrentzips, false otherwise</paran>
        /// <param name="copy">True if files should be copied to output, false for rebuild</param>
        /// <param name="workers">How many workers to launch for the job, default from config</param>
        /// <param name="subworkers">How many subworkers to launch for each worker, default from config</param>
        private static void InitBuild(
            List<string> inputs,
            string outdat,
            bool fixdatOnly,
            bool copy,
            int workers,
            int subworkers)
        {
            // Verify the filenames
            Dictionary<string, string> foundDats = GetValidDats(inputs);

            // Ensure the output directory is set
            if (string.IsNullOrWhiteSpace(outdat))
                outdat = "out";

            // Now that we have the dictionary, we can loop through and output to a new folder for each
            foreach (string key in foundDats.Keys)
            {
                // Get the DAT file associated with the key
                DatFile datFile = new DatFile();
                datFile.Parse(Path.Combine(_dats, foundDats[key]), 0, 0);

                // Create the new output directory if it doesn't exist
                string outputFolder = Path.Combine(outdat, Path.GetFileNameWithoutExtension(foundDats[key]));
                Utilities.EnsureOutputDirectory(outputFolder, create: true);

                // Get all online depots
                List<string> onlineDepots = _depots.Where(d => d.Value.Item2).Select(d => d.Key).ToList();

                // Now scan all of those depots and rebuild
                ArchiveScanLevel asl = Utilities.GetArchiveScanLevelFromNumbers(1, 1, 1, 1);
                datFile.RebuildDepot(onlineDepots, outputFolder, false /*date*/,
                    false /*delete*/, false /*inverse*/, (copy ? OutputFormat.TorrentGzipRomba : OutputFormat.TorrentZip),
                    false /*updateDat*/, null /*headerToCheckAgainst*/);
            }
        }

        /// <summary>
        /// Wrap cancelling a long-running job
        /// </summary>
        /// TODO: Implement
        private static void InitCancel()
        {
            Globals.Logger.User("This feature is not yet implemented: cancel");
        }

        /// <summary>
        /// Wrap printing dat stats
        /// </summary>
        /// <param name="inputs">List of input DATs to get stats from</param>
        private static void InitDatStats(List<string> inputs)
        {
            // If we have no inputs listed, we want to use datroot
            if (inputs == null || inputs.Count == 0)
            {
                inputs = new List<string>();
                inputs.Add(Path.GetFullPath(_dats));
            }

            // Now output the stats for all inputs
            DatFile.OutputStats(inputs, "rombasharp-datstats", null /* outDir */, true /* single */, true /* baddumpCol */, true /* nodumpCol */, StatReportFormat.Textfile);
        }

        /// <summary>
        /// Wrap printing db stats
        /// </summary>
        private static void InitDbStats()
        {
            SqliteConnection dbc = new SqliteConnection(_connectionString);
            dbc.Open();

            // Total number of CRCs
            string query = "SELECT COUNT(*) FROM crc";
            SqliteCommand slc = new SqliteCommand(query, dbc);
            Globals.Logger.User("Total CRCs: {0}", (long)slc.ExecuteScalar());

            // Total number of MD5s
            query = "SELECT COUNT(*) FROM md5";
            slc = new SqliteCommand(query, dbc);
            Globals.Logger.User("Total MD5s: {0}", (long)slc.ExecuteScalar());

            // Total number of SHA1s
            query = "SELECT COUNT(*) FROM sha1";
            slc = new SqliteCommand(query, dbc);
            Globals.Logger.User("Total SHA1s: {0}", (long)slc.ExecuteScalar());

            // Total number of DATs
            query = "SELECT COUNT(*) FROM dat";
            slc = new SqliteCommand(query, dbc);
            Globals.Logger.User("Total DATs: {0}", (long)slc.ExecuteScalar());

            slc.Dispose();
            dbc.Dispose();
        }

        /// <summary>
        /// Wrap creating a diffdat for a given old and new dat
        /// </summary>
        /// <param name="outdat">Output file</param>
        /// <param name="old">Old DAT file</param>
        /// <param name="newdat">New DAT file</param>
        /// <param name="name">Name value in DAT header</param>
        /// <param name="description">Description value in DAT header</param>
        private static void InitDiffDat(
            string outdat,
            string old,
            string newdat,
            string name,
            string description)
        {
            // Ensure the output directory
            Utilities.EnsureOutputDirectory(outdat, create: true);

            // Check that all required files exist
            if (!File.Exists(old))
            {
                Globals.Logger.Error("File '{0}' does not exist!", old);
                return;
            }

            if (!File.Exists(newdat))
            {
                Globals.Logger.Error("File '{0}' does not exist!", newdat);
                return;
            }

            // Create the encapsulating datfile
            DatFile datfile = new DatFile()
            {
                Name = name,
                Description = description,
            };

            // Create the inputs
            List<string> dats = new List<string>();
            dats.Add(newdat);
            List<string> basedats = new List<string>();
            basedats.Add(old);

            // Now run the diff on the inputs
            datfile.DetermineUpdateType(dats, basedats, outdat, UpdateMode.DiffAgainst, false /* inplace */, false /* skip */,
                false /* clean */, false /* remUnicode */, false /* descAsName */, new Filter(), SplitType.None,
                new List<Field>(), false /* onlySame */);
        }

        /// <summary>
        /// Wrap creating a dir2dat from a given source
        /// </summary>
        /// <param name="outdat">Output file</param>
        /// <param name="source">Source directory</param>
        /// <param name="name">Name value in DAT header</param>
        /// <param name="description">Description value in DAT header</param>
        private static void InitDir2Dat(
            string outdat,
            string source,
            string name,
            string description)
        {
            // Ensure the output directory
            Utilities.EnsureOutputDirectory(outdat, create: true);

            // Check that all required directories exist
            if (!Directory.Exists(source))
            {
                Globals.Logger.Error("File '{0}' does not exist!", source);
                return;
            }

            // Create the encapsulating datfile
            DatFile datfile = new DatFile()
            {
                Name = (string.IsNullOrWhiteSpace(name) ? "untitled" : name),
                Description = description,
            };

            // Now run the D2D on the input and write out
            // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
            datfile.PopulateFromDir(source, Hash.DeepHashes, true /* bare */, false /* archivesAsFiles */, SkipFileType.None, false /* addBlanks */,
                false /* addDate */, _tmpdir, false /* copyFiles */, null /* headerToCheckAgainst */, true /* chdsAsFiles */, null /* filter */);
            datfile.Write(outDir: outdat);
        }

        /// <summary>
        /// Wrap creating a diffdat for a given old and new dat
        /// </summary>
        /// <param name="outdat">Output file</param>
        /// <param name="old">Old DAT file</param>
        /// <param name="newdat">New DAT file</param>
        private static void InitEDiffDat(
            string outdat,
            string old,
            string newdat)
        {
            // Ensure the output directory
            Utilities.EnsureOutputDirectory(outdat, create: true);

            // Check that all required files exist
            if (!File.Exists(old))
            {
                Globals.Logger.Error("File '{0}' does not exist!", old);
                return;
            }

            if (!File.Exists(newdat))
            {
                Globals.Logger.Error("File '{0}' does not exist!", newdat);
                return;
            }

            // Create the encapsulating datfile
            DatFile datfile = new DatFile();

            // Create the inputs
            List<string> dats = new List<string>();
            dats.Add(newdat);
            List<string> basedats = new List<string>();
            basedats.Add(old);

            // Now run the diff on the inputs
            datfile.DetermineUpdateType(dats, basedats, outdat, UpdateMode.DiffAgainst, false /* inplace */, false /* skip */,
                false /* clean */, false /* remUnicode */, false /* descAsName */, new Filter(), SplitType.None,
                new List<Field>(), false /* onlySame */);
        }

        /// <summary>
        /// Wrap exporting the database to CSV
        /// </summary>
        /// TODO: Add ability to say which depot the files are found in
        private static void InitExport()
        {
            SqliteConnection dbc = new SqliteConnection(_connectionString);
            dbc.Open();
            StreamWriter sw = new StreamWriter(Utilities.TryCreate("export.csv"));

            // First take care of all file hashes
            sw.WriteLine("CRC,MD5,SHA-1"); // ,Depot

            string query = "SELECT crcsha1.crc, md5sha1.md5, md5sha1.sha1 FROM crcsha1 JOIN md5sha1 ON crcsha1.sha1=md5sha1.sha1"; // md5sha1.sha1=sha1depot.sha1
            SqliteCommand slc = new SqliteCommand(query, dbc);
            SqliteDataReader sldr = slc.ExecuteReader();

            if (sldr.HasRows)
            {
                while (sldr.Read())
                {
                    string line = sldr.GetString(0) + ","
                            + sldr.GetString(1) + ","
                            + sldr.GetString(2); // + ","
                            // + sldr.GetString(3);
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

        /// <summary>
        /// Wrap creating a fixdat for each Dat
        /// </summary>
        /// <param name="inputs">List of input DATs to get fixdats for</param>
        /// <param name="outdat">Output directory</param>
        /// <paran name="fixdatOnly">True to only fix dats and don't generate torrentzips, false otherwise</paran>
        /// <param name = "workers" > How many workers to launch for the job, default from config</param>
        /// <param name="subworkers">How many subworkers to launch for each worker, default from config</param>
        /// TODO: Implement
        private static void InitFixdat(
            List<string> inputs,
            string outdat,
            bool fixdatOnly,
            int workers,
            int subworkers)
        {
            Globals.Logger.Error("This feature is not yet implemented: fixdat");
        }

        /// <summary>
        /// Wrap importing CSVs into the database
        /// </summary>
        /// <param name="inputs">List of input CSV files to import information from</param>
        private static void InitImport(List<string> inputs)
        {
            Globals.Logger.Error("This feature is not yet implemented: import");

            // First ensure the inputs and database connection
            inputs = Utilities.GetOnlyFilesFromInputs(inputs);
            SqliteConnection dbc = new SqliteConnection(_connectionString);
            SqliteCommand slc = new SqliteCommand();
            dbc.Open();

            // Now, for each of these files, attempt to add the data found inside
            foreach (string input in inputs)
            {
                StreamReader sr = new StreamReader(Utilities.TryOpenRead(input));

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
                        crcquery += " (\"" + hashes[0] + "\"),";

                    if (!string.IsNullOrWhiteSpace(hashes[1]))
                        md5query += " (\"" + hashes[1] + "\"),";

                    if (!string.IsNullOrWhiteSpace(hashes[2]))
                    {
                        sha1query += " (\"" + hashes[2] + "\"),";

                        if (!string.IsNullOrWhiteSpace(hashes[0]))
                            crcsha1query += " (\"" + hashes[0] + "\", \"" + hashes[2] + "\"),";

                        if (!string.IsNullOrWhiteSpace(hashes[1]))
                            md5sha1query += " (\"" + hashes[1] + "\", \"" + hashes[2] + "\"),";
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

        /// <summary>
        /// Wrap looking up if hashes exist in the database
        /// </summary>
        /// <param name="inputs">List of input strings representing hashes to check for</param>
        /// <param name="size">Size to limit hash by, -1 otherwise</param>
        /// <param name="outdat">Output directory</param>
        private static void InitLookup(
            List<string> inputs,
            long size,
            string outdat)
        {
            // First, try to figure out what type of hash each is by length and clean it
            List<string> crc = new List<string>();
            List<string> md5 = new List<string>();
            List<string> sha1 = new List<string>();
            foreach (string input in inputs)
            {
                string temp = string.Empty;
                if (input.Length == Constants.CRCLength)
                {
                    temp = Utilities.CleanHashData(input, Constants.CRCLength);
                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        crc.Add(temp);
                    }
                }
                else if (input.Length == Constants.MD5Length)
                {
                    temp = Utilities.CleanHashData(input, Constants.MD5Length);
                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        md5.Add(temp);
                    }
                }
                else if (input.Length == Constants.SHA1Length)
                {
                    temp = Utilities.CleanHashData(input, Constants.SHA1Length);
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
                string query = "SELECT * FROM crc WHERE crc=\"" + input + "\"";
                SqliteCommand slc = new SqliteCommand(query, dbc);
                SqliteDataReader sldr = slc.ExecuteReader();
                if (sldr.HasRows)
                {
                    int count = 0;
                    while (sldr.Read())
                    {
                        count++;
                    }

                    Globals.Logger.User("For hash '{0}' there were {1} matches in the database", input, count);
                }
                else
                {
                    Globals.Logger.User("Hash '{0}' had no matches in the database", input);
                }

                sldr.Dispose();
                slc.Dispose();
            }
            foreach (string input in md5)
            {
                string query = "SELECT * FROM md5 WHERE md5=\"" + input + "\"";
                SqliteCommand slc = new SqliteCommand(query, dbc);
                SqliteDataReader sldr = slc.ExecuteReader();
                if (sldr.HasRows)
                {
                    int count = 0;
                    while (sldr.Read())
                    {
                        count++;
                    }

                    Globals.Logger.User("For hash '{0}' there were {1} matches in the database", input, count);
                }
                else
                {
                    Globals.Logger.User("Hash '{0}' had no matches in the database", input);
                }

                sldr.Dispose();
                slc.Dispose();
            }
            foreach (string input in sha1)
            {
                string query = "SELECT * FROM sha1 WHERE sha1=\"" + input + "\"";
                SqliteCommand slc = new SqliteCommand(query, dbc);
                SqliteDataReader sldr = slc.ExecuteReader();
                if (sldr.HasRows)
                {
                    int count = 0;
                    while (sldr.Read())
                    {
                        count++;
                    }

                    Globals.Logger.User("For hash '{0}' there were {1} matches in the database", input, count);
                }
                else
                {
                    Globals.Logger.User("Hash '{0}' had no matches in the database", input);
                }

                sldr.Dispose();
                slc.Dispose();
            }

            dbc.Dispose();
        }

        /// <summary>
        /// Wrap printing memory stats
        /// </summary>
        /// TODO: Implement
        private static void InitMemstats()
        {
            Globals.Logger.User("This feature is not yet implemented: memstats");
        }

        /// <summary>
        /// Wrap merging an external depot into an existing one
        /// </summary>
        /// <param name="inputs">List of input depots to merge in</param>
        /// <param name="onlyNeeded">True if only files in the database and don't exist are added, false otherwise</param>
        /// <param name="resume">Resume a previously interrupted operation from the specified path</param>
        /// <param name="workers">How many workers to launch for the job, default from config</param>
        /// <param name="skipInitialScan">True to skip the initial scan of the files to determine amount of work, false otherwise</param>
        /// TODO: Add way of specifying "current depot" since that's what Romba relies on
        /// TODO: Implement
        private static void InitMerge(
            List<string> inputs,
            bool onlyNeeded,
            string resume,
            int workers,
            bool skipInitialscan)
        {
            Globals.Logger.Error("This feature is not yet implemented: merge");

            // Verify that the inputs are valid directories
            inputs = Utilities.GetOnlyDirectoriesFromInputs(inputs);

            // Loop over all input directories
            foreach (string input in inputs)
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

        /// <summary>
        /// Wrap creating a havefile and a missfile for each Dat
        /// </summary>
        /// <param name="inputs">List of DAT files to get a miss and have for, empty means all</param>
        /// TODO: Implement
        private static void InitMiss(List<string> inputs)
        {
            // Verify the filenames
            Dictionary<string, string> foundDats = GetValidDats(inputs);

            // Create the new output directory if it doesn't exist
            Utilities.EnsureOutputDirectory(Path.Combine(Globals.ExeDir, "out"), create: true);

            // Now that we have the dictionary, we can loop through and output to a new folder for each
            foreach (string key in foundDats.Keys)
            {
                // Get the DAT file associated with the key
                DatFile datFile = new DatFile();
                datFile.Parse(Path.Combine(_dats, foundDats[key]), 0, 0);

                // Now loop through and see if all of the hash combinations exist in the database
                /* ended here */
            }

            Globals.Logger.Error("This feature is not yet implemented: miss");
        }

        /// <summary>
        /// Wrap showing progress of currently running command
        /// </summary>
        /// TODO: Implement
        private static void InitProgress()
        {
            Globals.Logger.User("This feature is not yet implemented: progress");
        }

        /// <summary>
        /// Wrap backing up of no longer needed files from the depots
        /// </summary>
        /// <param name="backup">Backup directory where backup files are moved to</param>
        /// <param name="workers">How many workers to launch for the job, default from config</param>
        /// <param name="depot">List of depots to scan files in, empty means all</param>
        /// <param name="dats">List of DATs to use as the basis of scanning, empty means all</param>
        /// <param name="logOnly">True if only the output of the operation is shown, false to actually run</param>
        /// TODO: Implement
        private static void InitPurgeBackup(
            string backup,
            int workers,
            List<string> depot,
            List<string> dats,
            bool logOnly)
        {
            Globals.Logger.Error("This feature is not yet implemented: purge-backup");
        }

        /// <summary>
        /// Wrap deleting of no longer needed files from the depots
        /// </summary>
        /// <param name="workers">How many workers to launch for the job, default from config</param>
        /// <param name="depot">List of depots to scan files in, empty means all</param>
        /// <param name="dats">List of DATs to use as the basis of scanning, empty means all</param>
        /// <param name="logOnly">True if only the output of the operation is shown, false to actually run</param>
        /// TODO: Implement
        private static void InitPurgeDelete(
            int workers,
            List<string> depot,
            List<string> dats,
            bool logOnly)
        {
            Globals.Logger.Error("This feature is not yet implemented: purge-delete");
        }

        /// <summary>
        /// Wrap refreshing the database with potentially new dats
        /// </summary>
        /// <param name="workers">How many workers to launch for the job, default from config</param>
        /// <param name="missingSha1s">Write paths of dats with missing sha1s into this file</param>
        private static void InitRefreshDats(
            int workers,
            string missingSha1s)
        {
            // Make sure the db is set
            if (string.IsNullOrWhiteSpace(_db))
            {
                _db = "db.sqlite";
                _connectionString = "Data Source=" + _db + ";Version = 3;";
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
            DatFile datroot = new DatFile { Type = "SuperDAT", };
            // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
            datroot.PopulateFromDir(_dats, Hash.DeepHashes, false, false, SkipFileType.None, false, false, _tmpdir, false, null, true, null);
            datroot.BucketBy(SortedBy.SHA1, DedupeType.None);

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
            datroot.BucketBy(SortedBy.Game, DedupeType.None, norename: true);

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
                    query += " OR hash=\"" + dathash + "\"";
                }

                query = query.Replace("WHERE OR", "WHERE");
                slc = new SqliteCommand(query, dbc);
                slc.ExecuteNonQuery();
                slc.Dispose();

                watch.Stop();
            }

            dbc.Dispose();
        }

        /// <summary>
        /// Wrap rescanning depots
        /// </summary>
        /// <param name="inputs">List of depots to rescan, empty means all</param>
        /// TODO: Verify implementation
        private static void InitRescanDepots(List<string> inputs)
        {
            Globals.Logger.Error("This feature is not yet implemented: rescan-depots");

            foreach (string depotname in inputs)
            {
                // Check that it's a valid depot first
                if (!_depots.ContainsKey(depotname))
                {
                    Globals.Logger.User("'{0}' is not a recognized depot. Please add it to your configuration file and try again", depotname);
                    return;
                }

                // Then check that the depot is online
                if (!Directory.Exists(depotname))
                {
                    Globals.Logger.User("'{0}' does not appear to be online. Please check its status and try again", depotname);
                    return;
                }

                // Open the database connection
                SqliteConnection dbc = new SqliteConnection(_connectionString);
                dbc.Open();

                // If we have it, then check for all hashes that are in that depot
                List<string> hashes = new List<string>();
                string query = "SELECT sha1 FROM sha1 WHERE depot=\"" + depotname + "\"";
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
                DatFile depot = new DatFile();
                // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
                depot.PopulateFromDir(depotname, Hash.DeepHashes, false, false, SkipFileType.None, false, false, _tmpdir, false, null, true, null);
                depot.BucketBy(SortedBy.SHA1, DedupeType.None);

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
                                crcquery += " (\"" + rom.CRC + "\"),";

                            if (!string.IsNullOrWhiteSpace(rom.MD5))
                                md5query += " (\"" + rom.MD5 + "\"),";

                            if (!string.IsNullOrWhiteSpace(rom.SHA1))
                            {
                                sha1query += " (\"" + rom.SHA1 + "\", \"" + depotname + "\"),";

                                if (!string.IsNullOrWhiteSpace(rom.CRC))
                                    crcsha1query += " (\"" + rom.CRC + "\", \"" + rom.SHA1 + "\"),";

                                if (!string.IsNullOrWhiteSpace(rom.MD5))
                                    md5sha1query += " (\"" + rom.MD5 + "\", \"" + rom.SHA1 + "\"),";
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
WHERE sha1.sha1 IN (string.Empty" + String.Join("\",\"", hashes) + "\")";
                slc = new SqliteCommand(query, dbc);
                slc.ExecuteNonQuery();

                // Dispose of the database connection
                slc.Dispose();
                dbc.Dispose();
            }
        }

        /// <summary>
        /// Wrap gracefully shutting down the server
        /// </summary>
        /// TODO: Implement
        private static void InitShutdown()
        {
            Globals.Logger.User("This feature is not yet implemented: shutdown");
        }

        /// <summary>
        /// Wrap printing the version
        /// </summary>
        private static void InitVersion()
        {
            Globals.Logger.User("RombaSharp version: {0}", Constants.Version);
        }

        #endregion
    }
}
