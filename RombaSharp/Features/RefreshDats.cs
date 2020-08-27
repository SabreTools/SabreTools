using System.Collections.Generic;
using System.IO;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.DatItems;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;
using Microsoft.Data.Sqlite;

namespace RombaSharp.Features
{
    internal class RefreshDats : BaseFeature
    {
        public const string Value = "Refresh DATs";

        public RefreshDats()
        {
            Name = Value;
            Flags = new List<string>() { "refresh-dats" };
            Description = "Refreshes the DAT index from the files in the DAT master directory tree.";
            _featureType = FeatureType.Flag;
            LongDescription = @"Refreshes the DAT index from the files in the DAT master directory tree.
Detects any changes in the DAT master directory tree and updates the DAT index
accordingly, marking deleted or overwritten dats as orphaned and updating
contents of any changed dats.";
            Features = new Dictionary<string, Feature>();

            AddFeature(WorkersInt32Input);
            AddFeature(MissingSha1sStringInput);
        }

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            base.ProcessFeatures(features);

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
            datroot.Header.Type = "SuperDAT";
            datroot.PopulateFromDir(_dats, asFiles: TreatAsFiles.AaruFormats | TreatAsFiles.CHDs);
            datroot.Items.BucketBy(Field.DatItem_SHA1, DedupeType.None);

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
                if (datroot.Items.ContainsKey(hash))
                {
                    datroot.Items.Remove(hash);
                    databaseDats.Add(hash);
                }
                else if (!databaseDats.Contains(hash))
                {
                    unneeded.Add(hash);
                }
            }

            datroot.Items.BucketBy(Field.Machine_Name, DedupeType.None, norename: true);

            watch.Stop();

            slc.Dispose();
            sldr.Dispose();

            // Loop through the Dictionary and add all data
            watch.Start("Adding new DAT information");
            foreach (string key in datroot.Items.Keys)
            {
                foreach (Rom value in datroot.Items[key])
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
}
