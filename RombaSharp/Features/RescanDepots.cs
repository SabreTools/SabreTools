using System.Collections.Generic;
using System.IO;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Help;
using Microsoft.Data.Sqlite;

namespace RombaSharp.Features
{
    internal class RescanDepots : BaseFeature
    {
        public const string Value = "Rescan Depots";

        // Unique to RombaSharp
        public RescanDepots()
        {
            Name = Value;
            Flags = new List<string>() { "depot-rescan" };
            Description = "Rescan a specific depot to get new information";
            _featureType = FeatureType.Flag;
            LongDescription = "Rescan a specific depot to get new information";
            Features = new Dictionary<string, Feature>();
        }

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            base.ProcessFeatures(features);
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
                depot.PopulateFromDir(depotname, asFiles: TreatAsFiles.AaruFormats | TreatAsFiles.CHDs);
                depot.Items.BucketBy(Field.DatItem_SHA1, DedupeType.None);

                // Set the base queries to use
                string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
                string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
                string sha1query = "INSERT OR IGNORE INTO sha1 (sha1, depot) VALUES";
                string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
                string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

                // Once we have both, check for any new files
                List<string> dupehashes = new List<string>();
                IEnumerable<string> keys = depot.Items.Keys;
                foreach (string key in keys)
                {
                    List<DatItem> roms = depot.Items[key];
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
}
