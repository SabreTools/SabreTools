using System.Collections.Generic;
using System.IO;
using System.Linq;

using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.FileTypes;
using SabreTools.Library.Help;
using Microsoft.Data.Sqlite;

namespace RombaSharp.Features
{
    internal class Archive : BaseFeature
    {
        public const string Value = "Archive";

        public Archive()
        {
            Name = Value;
            Flags = new List<string>() { "archive" };
            Description = "Adds ROM files from the specified directories to the ROM archive.";
            _featureType = FeatureType.Flag;
            LongDescription = @"Adds ROM files from the specified directories to the ROM archive.
Traverses the specified directory trees looking for zip files and normal files.
Unpacked files will be stored as individual entries. Prior to unpacking a zip
file, the external SHA1 is checked against the DAT index. 
If -only-needed is set, only those files are put in the ROM archive that
have a current entry in the DAT index.";
            Features = new Dictionary<string, Feature>();

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
            base.ProcessFeatures(features);

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
                df.PopulateFromDir(dir, asFiles: TreatAsFiles.AaruFormats | TreatAsFiles.CHDs);
                df.PopulateFromDir(dir, asFiles: TreatAsFiles.AaruFormats | TreatAsFiles.Archives | TreatAsFiles.CHDs);
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

            foreach (string key in df.Items.Keys)
            {
                List<DatItem> datItems = df.Items[key];
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
                            need.Items.Add(key, rom);
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
                        need.Items.Add(key, rom);
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
            need.RebuildGeneric(
                onlyDirs,
                outDir: _depots.Keys.ToList()[0],
                outputFormat: OutputFormat.TorrentGzipRomba,
                updateDat: false,
                asFiles: TreatAsFiles.AaruFormats | TreatAsFiles.CHDs);
        }
    }
}
