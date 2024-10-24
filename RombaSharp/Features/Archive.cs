﻿using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using SabreTools.Core;
using SabreTools.DatFiles;
using SabreTools.DatItems;
using SabreTools.DatItems.Formats;
using SabreTools.DatTools;
using SabreTools.FileTypes;
using SabreTools.Hashing;
using SabreTools.Help;

namespace RombaSharp.Features
{
    internal class Archive : BaseFeature
    {
        public const string Value = "Archive";

        public Archive()
        {
            Name = Value;
            Flags.AddRange(["archive"]);
            Description = "Adds ROM files from the specified directories to the ROM archive.";
            _featureType = ParameterType.Flag;
            LongDescription = @"Adds ROM files from the specified directories to the ROM archive.
Traverses the specified directory trees looking for zip files and normal files.
Unpacked files will be stored as individual entries. Prior to unpacking a zip
file, the external SHA1 is checked against the DAT index. 
If -only-needed is set, only those files are put in the ROM archive that
have a current entry in the DAT index.";

            // Common Features
            AddCommonFeatures();

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

        public override bool ProcessFeatures(Dictionary<string, SabreTools.Help.Feature?> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
                return false;

            // Get the archive scanning level
            // TODO: Remove usage
            int sevenzip = GetInt32(features, Include7ZipsInt32Value);
            int gz = GetInt32(features, IncludeGZipsInt32Value);
            int zip = GetInt32(features, IncludeZipsInt32Value);

            // Get feature flags
            bool noDb = GetBoolean(features, NoDbValue);
            bool onlyNeeded = GetBoolean(features, OnlyNeededValue);
            HashType[] hashes = [HashType.CRC32, HashType.MD5, HashType.SHA1];
            var dfd = new DatFromDir(hashes, SkipFileType.None, addBlanks: false);

            // First we want to get just all directories from the inputs
            List<string> onlyDirs = [];
            foreach (string input in Inputs)
            {
                if (Directory.Exists(input))
                    onlyDirs.Add(Path.GetFullPath(input));
            }

            // Then process all of the input directories into an internal DAT
            DatFile df = DatFile.Create();
            foreach (string dir in onlyDirs)
            {
                dfd.PopulateFromDir(df, dir, TreatAsFile.NonArchive);
                dfd.PopulateFromDir(df, dir, TreatAsFile.All);
            }

            // Create an empty Dat for files that need to be rebuilt
            var need = DatFile.Create();

            // Get the first depot as output
            var depotKeyEnumerator = _depots.Keys.GetEnumerator();
            depotKeyEnumerator.MoveNext();
            string firstDepot = depotKeyEnumerator.Current;

            // Open the database connection
            var dbc = new SqliteConnection(_connectionString);
            dbc.Open();

            // Now that we have the Dats, add the files to the database
            string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
            string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
            string sha1query = "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES";
            string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
            string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

            foreach (string key in df.Items.Keys)
            {
                ConcurrentList<DatItem>? datItems = df.Items[key];
                if (datItems == null)
                    continue;

                foreach (Rom rom in datItems)
                {
                    string? crc = rom.GetStringFieldValue(SabreTools.Models.Metadata.Rom.CRCKey);
                    string? md5 = rom.GetStringFieldValue(SabreTools.Models.Metadata.Rom.MD5Key);
                    string? sha1 = rom.GetStringFieldValue(SabreTools.Models.Metadata.Rom.SHA1Key);

                    // If we care about if the file exists, check the databse first
                    if (onlyNeeded && !noDb)
                    {
                        string query = "SELECT * FROM crcsha1 JOIN md5sha1 ON crcsha1.sha1=md5sha1.sha1"
                                    + $" WHERE crcsha1.crc=\"{crc}\""
                                    + $" OR md5sha1.md5=\"{md5}\""
                                    + $" OR md5sha1.sha1=\"{sha1}\"";
                        var slc = new SqliteCommand(query, dbc);
                        SqliteDataReader sldr = slc.ExecuteReader();

                        if (sldr.HasRows)
                        {
                            // Add to the queries
                            if (!string.IsNullOrWhiteSpace(crc))
                                crcquery += $" (\"{crc}\"),";

                            if (!string.IsNullOrWhiteSpace(md5))
                                md5query += $" (\"{md5}\"),";

                            if (!string.IsNullOrWhiteSpace(sha1))
                            {
                                sha1query += $" (\"{sha1}\", \"{firstDepot}\"),";

                                if (!string.IsNullOrWhiteSpace(crc))
                                    crcsha1query += $" (\"{crc}\", \"{sha1}\"),";

                                if (!string.IsNullOrWhiteSpace(md5))
                                    md5sha1query += $" (\"{md5}\", \"{sha1}\"),";
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
                            if (!string.IsNullOrWhiteSpace(crc))
                                crcquery += $" (\"{crc}\"),";

                            if (!string.IsNullOrWhiteSpace(md5))
                                md5query += $" (\"{md5}\"),";

                            if (!string.IsNullOrWhiteSpace(sha1))
                            {
                                sha1query += $" (\"{sha1}\", \"{firstDepot}\"),";

                                if (!string.IsNullOrWhiteSpace(crc))
                                    crcsha1query += $" (\"{crc}\", \"{sha1}\"),";

                                if (!string.IsNullOrWhiteSpace(md5))
                                    md5sha1query += $" (\"{md5}\", \"{sha1}\"),";
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
                var slc = new SqliteCommand(crcquery.TrimEnd(','), dbc);
                slc.ExecuteNonQuery();
                slc.Dispose();
            }

            if (md5query != "INSERT OR IGNORE INTO md5 (md5) VALUES")
            {
                var slc = new SqliteCommand(md5query.TrimEnd(','), dbc);
                slc.ExecuteNonQuery();
                slc.Dispose();
            }

            if (sha1query != "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES")
            {
                var slc = new SqliteCommand(sha1query.TrimEnd(','), dbc);
                slc.ExecuteNonQuery();
                slc.Dispose();
            }

            if (crcsha1query != "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES")
            {
                var slc = new SqliteCommand(crcsha1query.TrimEnd(','), dbc);
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
            Rebuilder.RebuildGeneric(
                need,
                onlyDirs,
                outDir: firstDepot,
                outputFormat: OutputFormat.TorrentGzipRomba,
                asFiles: TreatAsFile.NonArchive);

            return true;
        }
    }
}
