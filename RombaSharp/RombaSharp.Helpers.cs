using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Tools;
using Microsoft.Data.Sqlite;

namespace RombaSharp
{
    public partial class RombaSharp
    {
        #region Helper methods

        /// <summary>
        /// Gets all valid DATs that match in the DAT root
        /// </summary>
        /// <param name="inputs">List of input strings to check for, presumably file names</param>
        /// <returns>Dictionary of hash/full path for each of the valid DATs</returns>
        private static Dictionary<string, string> GetValidDats(List<string> inputs)
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
            _tmpdir = tmpdir;
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
        private static void AddDatToDatabase(Rom dat, SqliteConnection dbc)
        {
            // Get the dat full path
            string fullpath = Path.Combine(_dats, (dat.MachineName == "dats" ? string.Empty : dat.MachineName), dat.Name);

            // Parse the Dat if possible
            Globals.Logger.User($"Adding from '{dat.Name}'");
            DatFile tempdat = DatFile.CreateAndParse(fullpath);

            // If the Dat wasn't empty, add the information
            SqliteCommand slc;
            if (tempdat.GetCount() != 0)
            {
                string crcquery = "INSERT OR IGNORE INTO crc (crc) VALUES";
                string md5query = "INSERT OR IGNORE INTO md5 (md5) VALUES";
                string sha1query = "INSERT OR IGNORE INTO sha1 (sha1) VALUES";
                string crcsha1query = "INSERT OR IGNORE INTO crcsha1 (crc, sha1) VALUES";
                string md5sha1query = "INSERT OR IGNORE INTO md5sha1 (md5, sha1) VALUES";

                // Loop through the parsed entries
                foreach (string romkey in tempdat.Keys)
                {
                    foreach (DatItem datItem in tempdat[romkey])
                    {
                        Globals.Logger.Verbose($"Checking and adding file '{datItem.Name}'");

                        if (datItem.ItemType == ItemType.Rom)
                        {
                            Rom rom = (Rom)datItem;

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
                        else if (datItem.ItemType == ItemType.Disk)
                        {
                            Disk disk = (Disk)datItem;

                            if (!string.IsNullOrWhiteSpace(disk.MD5))
                                md5query += $" (\"{disk.MD5}\"),";

                            if (!string.IsNullOrWhiteSpace(disk.SHA1))
                            {
                                sha1query += $" (\"{disk.SHA1}\"),";

                                if (!string.IsNullOrWhiteSpace(disk.MD5))
                                    md5sha1query += $" (\"{disk.MD5}\", \"{disk.SHA1}\"),";
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
            }

            string datquery = $"INSERT OR IGNORE INTO dat (hash) VALUES (\"{dat.SHA1}\")";
            slc = new SqliteCommand(datquery, dbc);
            slc.ExecuteNonQuery();
            slc.Dispose();
        }

        #endregion
    }
}
