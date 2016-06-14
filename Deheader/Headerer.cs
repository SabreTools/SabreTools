﻿using Mono.Data.Sqlite;
using SabreTools.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SabreTools
{
	/// <summary>
	/// Entry class for the Deheader application
	/// </summary>
	class Headerer
	{
		private static string _dbName = "Headerer.sqlite";
		private static string _connectionString = "Data Source=" + _dbName + ";Version = 3;";
		private static Logger logger;

		/// <summary>
		/// Start deheader operation with supplied parameters
		/// </summary>
		/// <param name="args">String array representing command line parameters</param>
		public static void Main(string[] args)
		{
			// If output is being redirected, don't allow clear screens
			if (!Console.IsOutputRedirected)
			{
				Console.Clear();
			}

			// Perform initial setup and verification
			logger = new Logger(true, "headerer.log");
			logger.Start();
			DBTools.EnsureDatabase(_dbName, _connectionString);
			Remapping.CreateHeaderSkips();

			// Credits take precidence over all
			if ((new List<string>(args)).Contains("--credits"))
			{
				Build.Credits();
				return;
			}

			if (args.Length == 0 || args.Length > 2)
			{
				Build.Help();
				logger.Close();
				return;
			}

			// Output the title
			Build.Start("Headerer");

			// Get the filename (or foldername)
			string file = "";
			bool deheader = true;
			if (args.Length == 1)
			{
				file = args[0];
			}
			else
			{
				
				if (args[0] == "-e")
				{
					deheader = true;
				}
				else if (args[0] == "-r")
				{
					deheader = false;
				}

				file = args[1];
			}

			if (deheader)
			{
				// If it's a single file, just check it
				if (File.Exists(file))
				{
					DetectRemoveHeader(file);
				}
				// If it's a directory, recursively check all
				else if (Directory.Exists(file))
				{
					foreach (string sub in Directory.GetFiles(file))
					{
						if (sub != ".." && sub != ".")
						{
							DetectRemoveHeader(sub);
						}
					}
				}
				// Else, show that help text
				else
				{
					Build.Help();
				}
			}
			else
			{
				// If it's a single file, just check it
				if (File.Exists(file))
				{
					ReplaceHeader(file);
				}
				// If it's a directory, recursively check all
				else if (Directory.Exists(file))
				{
					foreach (string sub in Directory.GetFiles(file))
					{
						if (sub != ".." && sub != ".")
						{
							ReplaceHeader(sub);
						}
					}
				}
				// Else, show that help text
				else
				{
					Build.Help();
				}
			}
			logger.Close();
		}

		/// <summary>
		/// Detect and remove header from the given file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		private static void DetectRemoveHeader(string file)
		{
			// First get the HeaderType, if any
			int headerSize = -1;
			HeaderType type = RomTools.GetFileHeaderType(file, out headerSize, logger);

			// If we have a valid HeaderType, remove the correct byte count
			logger.User("File has header: " + (type != HeaderType.None));
			if (type != HeaderType.None)
			{
				logger.Log("Deteched header type: " + type);

				// Now take care of the header and new output file
				string hstr = string.Empty;
				using (BinaryReader br = new BinaryReader(File.OpenRead(file)))
				{
					// Extract the header as a string for the database
					byte[] hbin = br.ReadBytes(headerSize);
					for (int i = 0; i < headerSize; i++)
					{
						hstr += BitConverter.ToString(new byte[] { hbin[i] });
					}
				}

				// Write out the remaining bytes to new file
				logger.User("Creating unheadered file: " + file + ".new");
				Output.RemoveBytesFromFile(file, file + ".new", headerSize, 0);
				logger.User("Unheadered file created!");

				// Now add the information to the database if it's not already there
				RomData rom = RomTools.GetSingleFileInfo(file + ".new");
				AddHeaderToDatabase(hstr, rom.SHA1, type);
			}
		}

		/// <summary>
		/// Add a header to the database
		/// </summary>
		/// <param name="header">String representing the header bytes</param>
		/// <param name="SHA1">SHA-1 of the deheadered file</param>
		/// <param name="type">HeaderType representing the detected header</param>
		private static void AddHeaderToDatabase(string header, string SHA1, HeaderType type)
		{
			bool exists = false;

			string query = @"SELECT * FROM data WHERE sha1='" + SHA1 + "' AND header='" + header + "'";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						exists = sldr.HasRows;
					}
				}
			}

			if (!exists)
			{
				query = @"INSERT INTO data (sha1, header, type) VALUES ('" +
				SHA1 + "', " +
				"'" + header + "', " +
				"'" + type.ToString() + "')";
				using (SqliteConnection dbc = new SqliteConnection(_connectionString))
				{
					dbc.Open();
					using (SqliteCommand slc = new SqliteCommand(query, dbc))
					{
						logger.Log("Result of inserting header: " + slc.ExecuteNonQuery());
					}
				}
			}
		}

		/// <summary>
		/// Detect and replace header(s) to the given file
		/// </summary>
		/// <param name="file">Name of the file to be parsed</param>
		private static void ReplaceHeader(string file)
		{
			// First, get the SHA-1 hash of the file
			RomData rom = RomTools.GetSingleFileInfo(file);

			// Then try to pull the corresponding headers from the database
			string header = "";

			string query = @"SELECT header, type FROM data WHERE sha1='" + rom.SHA1 + "'";
			using (SqliteConnection dbc = new SqliteConnection(_connectionString))
			{
				dbc.Open();
				using (SqliteCommand slc = new SqliteCommand(query, dbc))
				{
					using (SqliteDataReader sldr = slc.ExecuteReader())
					{
						if (sldr.HasRows)
						{
							int sub = 0;
							while (sldr.Read())
							{
								logger.Log("Found match with rom type " + sldr.GetString(1));
								header = sldr.GetString(0);

								logger.User("Creating reheadered file: " + file + ".new" + sub);
								Output.AppendBytesToFile(file, file + ".new" + sub, header, string.Empty);
								logger.User("Reheadered file created!");
							}
						}
						else
						{
							logger.Warning("No matching header could be found!");
							return;
						}
					}
				}
			}
		}
	}
}
