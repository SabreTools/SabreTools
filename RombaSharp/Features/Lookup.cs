﻿using System.Collections.Generic;
using SabreTools.Hashing;
using SabreTools.Help;
using Microsoft.Data.Sqlite;

namespace RombaSharp.Features
{
    internal class Lookup : BaseFeature
    {
        public const string Value = "Lookup";

        public Lookup()
        {
            Name = Value;
            Flags.AddRange(["lookup"]);
            Description = "For each specified hash it looks up any available information.";
            _featureType = ParameterType.Flag;
            LongDescription = "For each specified hash it looks up any available information (dat or rom).";

            // Common Features
            AddCommonFeatures();

            AddFeature(SizeInt64Input); // Defaults to -1
            AddFeature(OutStringInput);
        }

        public override bool ProcessFeatures(Dictionary<string, Feature?> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
                return false;

            // Get feature flags
            long size = GetInt64(features, SizeInt64Value);
            string? outdat = GetString(features, OutStringValue);

            // First, try to figure out what type of hash each is by length and clean it
            List<string> crc = [];
            List<string> md5 = [];
            List<string> sha1 = [];
            foreach (string input in Inputs)
            {
                if (input.Length == Constants.CRCLength)
                    crc.Add(input);
                else if (input.Length == Constants.MD5Length)
                    md5.Add(input);
                else if (input.Length == Constants.SHA1Length)
                    sha1.Add(input);
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

                    logger.User($"For hash '{input}' there were {count} matches in the database");
                }
                else
                {
                    logger.User($"Hash '{input}' had no matches in the database");
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

                    logger.User($"For hash '{input}' there were {count} matches in the database");
                }
                else
                {
                    logger.User($"Hash '{input}' had no matches in the database");
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

                    logger.User($"For hash '{input}' there were {count} matches in the database");
                }
                else
                {
                    logger.User($"Hash '{input}' had no matches in the database");
                }

                sldr.Dispose();
                slc.Dispose();
            }

            dbc.Dispose();
            return true;
        }
    }
}
