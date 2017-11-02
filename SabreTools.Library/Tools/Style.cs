using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;

using BinaryReader = System.IO.BinaryReader;
using FileStream = System.IO.FileStream;
#endif

namespace SabreTools.Library.Tools
{
	/// <summary>
	/// Include character normalization and replacement mappings
	/// </summary>
	public static class Style
	{
		#region DAT Cleaning

		/// <summary>
		/// Clean a game (or rom) name to the WoD standard
		/// </summary>
		/// <param name="game">Name of the game to be cleaned</param>
		/// <returns>The cleaned name</returns>
		public static string CleanGameName(string game)
		{
			///Run the name through the filters to make sure that it's correct
			game = NormalizeChars(game);
			game = RussianToLatin(game);
			game = SearchPattern(game);

			game = new Regex(@"(([[(].*[\)\]] )?([^([]+))").Match(game).Groups[1].Value;
			game = game.TrimStart().TrimEnd();
			return game;
		}

		/// <summary>
		/// Clean a game (or rom) name to the WoD standard
		/// </summary>
		/// <param name="game">Array representing the path to be cleaned</param>
		/// <returns>The cleaned name</returns>
		public static string CleanGameName(string[] game)
		{
			game[game.Length - 1] = CleanGameName(game[game.Length - 1]);
			string outgame = String.Join(Path.DirectorySeparatorChar.ToString(), game);
			outgame = outgame.TrimStart().TrimEnd();
			return outgame;
		}

		/// <summary>
		/// Clean a hash string and pad to the correct size
		/// </summary>
		/// <param name="hash">Hash string to sanitize</param>
		/// <param name="padding">Amount of characters to pad to</param>
		/// <returns>Cleaned string</returns>
		public static string CleanHashData(string hash, int padding)
		{
			// If we have a known blank hash, return blank
			if (string.IsNullOrEmpty(hash) || hash == "-" || hash == "_")
			{
				return "";
			}

			// Check to see if it's a "hex" hash
			hash = hash.Trim().Replace("0x", "");

			// If we have a blank hash now, return blank
			if (string.IsNullOrEmpty(hash))
			{
				return "";
			}

			// If the hash shorter than the required length, pad it
			if (hash.Length < padding)
			{
				hash = hash.PadLeft(padding, '0');
			}
			// If the hash is longer than the required length, it's invalid
			else if (hash.Length > padding)
			{
				return "";
			}
			
			// Now normalize the hash
			hash = hash.ToLowerInvariant();

			// Otherwise, make sure that every character is a proper match
			for (int i = 0; i < hash.Length; i++)
			{
				if ((hash[i] < '0' || hash[i] > '9') && (hash[i] < 'a' || hash[i] > 'f'))
				{
					hash = "";
					break;
				}
			}

			return hash;
		}

		/// <summary>
		/// Clean a hash string from a Listrom DAT
		/// </summary>
		/// <param name="hash">Hash string to sanitize</param>
		/// <returns>Cleaned string</returns>
		public static string CleanListromHashData(string hash)
		{
			if (hash.StartsWith("CRC"))
			{
				return hash.Substring(4, 8).ToLowerInvariant();
			}
			else if (hash.StartsWith("SHA1"))
			{
				return hash.Substring(5, 40).ToLowerInvariant();
			}

			return hash;
		}

		/// <summary>
		/// Generate a proper outfile name based on a DAT and output directory
		/// </summary>
		/// <param name="outDir">Output directory</param>
		/// <param name="datdata">DAT information</param>
		/// <param name="overwrite">True if we ignore existing files (default), false otherwise</param>
		/// <returns>Dictionary of output formats mapped to file names</returns>
		public static Dictionary<DatFormat, string> CreateOutfileNames(string outDir, DatFile datdata, bool overwrite = true)
		{
			// Create the output dictionary
			Dictionary<DatFormat, string> outfileNames = new Dictionary<DatFormat, string>();

			// Double check the outDir for the end delim
			if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outDir += Path.DirectorySeparatorChar;
			}

			// Get the extensions from the output type

			// AttractMode
			if ((datdata.DatFormat & DatFormat.AttractMode) != 0)
			{
				outfileNames.Add(DatFormat.AttractMode, CreateOutfileNamesHelper(outDir, ".txt", datdata, overwrite));
			}

			// ClrMamePro
			if ((datdata.DatFormat & DatFormat.ClrMamePro) != 0)
			{
				outfileNames.Add(DatFormat.ClrMamePro, CreateOutfileNamesHelper(outDir, ".dat", datdata, overwrite));
			};

			// CSV
			if ((datdata.DatFormat & DatFormat.CSV) != 0)
			{
				outfileNames.Add(DatFormat.CSV, CreateOutfileNamesHelper(outDir, ".csv", datdata, overwrite));
			};

			// DOSCenter
			if ((datdata.DatFormat & DatFormat.DOSCenter) != 0
				&& (datdata.DatFormat & DatFormat.ClrMamePro) == 0
				&& (datdata.DatFormat & DatFormat.RomCenter) == 0)
			{
				outfileNames.Add(DatFormat.DOSCenter, CreateOutfileNamesHelper(outDir, ".dat", datdata, overwrite));
			};
			if ((datdata.DatFormat & DatFormat.DOSCenter) != 0
				&& ((datdata.DatFormat & DatFormat.ClrMamePro) != 0
					|| (datdata.DatFormat & DatFormat.RomCenter) != 0))
			{
				outfileNames.Add(DatFormat.DOSCenter, CreateOutfileNamesHelper(outDir, ".dc.dat", datdata, overwrite));
			}

			//MAME Listroms
			if ((datdata.DatFormat & DatFormat.Listroms) != 0
				&& (datdata.DatFormat & DatFormat.AttractMode) == 0)
			{
				outfileNames.Add(DatFormat.Listroms, CreateOutfileNamesHelper(outDir, ".txt", datdata, overwrite));
			}
			if ((datdata.DatFormat & DatFormat.Listroms) != 0
				&& (datdata.DatFormat & DatFormat.AttractMode) != 0)
			{
				outfileNames.Add(DatFormat.Listroms, CreateOutfileNamesHelper(outDir, ".lr.txt", datdata, overwrite));
			}

			// Logiqx XML
			if ((datdata.DatFormat & DatFormat.Logiqx) != 0)
			{
				outfileNames.Add(DatFormat.Logiqx, CreateOutfileNamesHelper(outDir, ".xml", datdata, overwrite));
			}

			// Missfile
			if ((datdata.DatFormat & DatFormat.MissFile) != 0
				&& (datdata.DatFormat & DatFormat.AttractMode) == 0)
			{
				outfileNames.Add(DatFormat.MissFile, CreateOutfileNamesHelper(outDir, ".txt", datdata, overwrite));
			}
			if ((datdata.DatFormat & DatFormat.MissFile) != 0
				&& (datdata.DatFormat & DatFormat.AttractMode) != 0)
			{
				outfileNames.Add(DatFormat.MissFile, CreateOutfileNamesHelper(outDir, ".miss.txt", datdata, overwrite));
			}

			// OfflineList
			if (((datdata.DatFormat & DatFormat.OfflineList) != 0)
				&& (datdata.DatFormat & DatFormat.Logiqx) == 0
				&& (datdata.DatFormat & DatFormat.SabreDat) == 0
				&& (datdata.DatFormat & DatFormat.SoftwareList) == 0)
			{
				outfileNames.Add(DatFormat.OfflineList, CreateOutfileNamesHelper(outDir, ".xml", datdata, overwrite));
			}
			if (((datdata.DatFormat & DatFormat.OfflineList) != 0
				&& ((datdata.DatFormat & DatFormat.Logiqx) != 0
					|| (datdata.DatFormat & DatFormat.SabreDat) != 0
					|| (datdata.DatFormat & DatFormat.SoftwareList) != 0)))
			{
				outfileNames.Add(DatFormat.OfflineList, CreateOutfileNamesHelper(outDir, ".ol.xml", datdata, overwrite));
			}

			// Redump MD5
			if ((datdata.DatFormat & DatFormat.RedumpMD5) != 0)
			{
				outfileNames.Add(DatFormat.RedumpMD5, CreateOutfileNamesHelper(outDir, ".md5", datdata, overwrite));
			};

			// Redump SFV
			if ((datdata.DatFormat & DatFormat.RedumpSFV) != 0)
			{
				outfileNames.Add(DatFormat.RedumpSFV, CreateOutfileNamesHelper(outDir, ".sfv", datdata, overwrite));
			};

			// Redump SHA-1
			if ((datdata.DatFormat & DatFormat.RedumpSHA1) != 0)
			{
				outfileNames.Add(DatFormat.RedumpSHA1, CreateOutfileNamesHelper(outDir, ".sha1", datdata, overwrite));
			};

			// Redump SHA-256
			if ((datdata.DatFormat & DatFormat.RedumpSHA256) != 0)
			{
				outfileNames.Add(DatFormat.RedumpSHA256, CreateOutfileNamesHelper(outDir, ".sha256", datdata, overwrite));
			};

			// RomCenter
			if ((datdata.DatFormat & DatFormat.RomCenter) != 0
				&& (datdata.DatFormat & DatFormat.ClrMamePro) == 0)
			{
				outfileNames.Add(DatFormat.RomCenter, CreateOutfileNamesHelper(outDir, ".dat", datdata, overwrite));
			};
			if ((datdata.DatFormat & DatFormat.RomCenter) != 0
				&& (datdata.DatFormat & DatFormat.ClrMamePro) != 0)
			{
				outfileNames.Add(DatFormat.RomCenter, CreateOutfileNamesHelper(outDir, ".rc.dat", datdata, overwrite));
			};

			// SabreDAT
			if ((datdata.DatFormat & DatFormat.SabreDat) != 0 && (datdata.DatFormat & DatFormat.Logiqx) == 0)
			{
				outfileNames.Add(DatFormat.SabreDat, CreateOutfileNamesHelper(outDir, ".xml", datdata, overwrite));
			};
			if ((datdata.DatFormat & DatFormat.SabreDat) != 0 && (datdata.DatFormat & DatFormat.Logiqx) != 0)
			{
				outfileNames.Add(DatFormat.SabreDat, CreateOutfileNamesHelper(outDir, ".sd.xml", datdata, overwrite));
			};

			// Software List
			if ((datdata.DatFormat & DatFormat.SoftwareList) != 0
				&& (datdata.DatFormat & DatFormat.Logiqx) == 0
				&& (datdata.DatFormat & DatFormat.SabreDat) == 0)
			{
				outfileNames.Add(DatFormat.SoftwareList, CreateOutfileNamesHelper(outDir, ".xml", datdata, overwrite));
			}
			if ((datdata.DatFormat & DatFormat.SoftwareList) != 0
				&& ((datdata.DatFormat & DatFormat.Logiqx) != 0
					|| (datdata.DatFormat & DatFormat.SabreDat) != 0))
			{
				outfileNames.Add(DatFormat.SoftwareList, CreateOutfileNamesHelper(outDir, ".sl.xml", datdata, overwrite));
			}

			// TSV
			if ((datdata.DatFormat & DatFormat.TSV) != 0)
			{
				outfileNames.Add(DatFormat.TSV, CreateOutfileNamesHelper(outDir, ".tsv", datdata, overwrite));
			};

			return outfileNames;
		}

		/// <summary>
		/// Help generating the outfile name
		/// </summary>
		/// <param name="outDir">Output directory</param>
		/// <param name="extension">Extension to use for the file</param>
		/// <param name="datdata">DAT information</param>
		/// <param name="overwrite">True if we ignore existing files, false otherwise</param>
		/// <returns>String containing the new filename</returns>
		private static string CreateOutfileNamesHelper(string outDir, string extension, DatFile datdata, bool overwrite)
		{
			string filename = (String.IsNullOrEmpty(datdata.FileName) ? datdata.Description : datdata.FileName);
			string outfile = outDir + filename + extension;
			outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
				outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
				outfile);
			if (!overwrite)
			{
				int i = 1;
				while (File.Exists(outfile))
				{
					outfile = outDir + filename + "_" + i + extension;
					outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
						outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
						outfile);
					i++;
				}
			}

			return outfile;
		}

		/// <summary>
		/// Get the proper extension for the stat output format
		/// </summary>
		/// <param name="outDir">Output path to use</param>
		/// <param name="statDatFormat">StatDatFormat to get the extension for</param>
		/// <param name="reportName">Name of the input file to use</param>
		/// <returns>Dictionary of output formats mapped to file names</returns>
		public static Dictionary<StatDatFormat, string> CreateOutStatsNames(string outDir, StatDatFormat statDatFormat, string reportName, bool overwrite = true)
		{
			Dictionary<StatDatFormat, string> output = new Dictionary<StatDatFormat, string>();

			// First try to create the output directory if we need to
			if (!Directory.Exists(outDir))
			{
				Directory.CreateDirectory(outDir);
			}

			// Double check the outDir for the end delim
			if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
			{
				outDir += Path.DirectorySeparatorChar;
			}

			// For each output format, get the appropriate stream writer
			if ((statDatFormat & StatDatFormat.None) != 0)
			{
				output.Add(StatDatFormat.None, CreateOutStatsNamesHelper(outDir, ".txt", reportName, overwrite));
			}
			if ((statDatFormat & StatDatFormat.CSV) != 0)
			{
				output.Add(StatDatFormat.CSV, CreateOutStatsNamesHelper(outDir, ".csv", reportName, overwrite));
			}
			if ((statDatFormat & StatDatFormat.HTML) != 0)
			{
				output.Add(StatDatFormat.HTML, CreateOutStatsNamesHelper(outDir, ".html", reportName, overwrite));
			}
			if ((statDatFormat & StatDatFormat.TSV) != 0)
			{
				output.Add(StatDatFormat.TSV, CreateOutStatsNamesHelper(outDir, ".tsv", reportName, overwrite));
			}

			return output;
		}

		/// <summary>
		/// Help generating the outstats name
		/// </summary>
		/// <param name="outDir">Output directory</param>
		/// <param name="extension">Extension to use for the file</param>
		/// <param name="reportName">Name of the input file to use</param>
		/// <param name="overwrite">True if we ignore existing files, false otherwise</param>
		/// <returns>String containing the new filename</returns>
		private static string CreateOutStatsNamesHelper(string outDir, string extension, string reportName, bool overwrite)
		{
			string outfile = outDir + reportName + extension;
			outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
				outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
				outfile);
			if (!overwrite)
			{
				int i = 1;
				while (File.Exists(outfile))
				{
					outfile = outDir + reportName + "_" + i + extension;
					outfile = (outfile.Contains(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString()) ?
						outfile.Replace(Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString()) :
						outfile);
					i++;
				}
			}

			return outfile;
		}

		#endregion

		#region Extensions

		/// <summary>
		/// Reads the specified number of bytes from the stream, starting from a specified point in the byte array.
		/// </summary>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="index">The starting point in the buffer at which to begin reading into the buffer.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <returns>The number of bytes read into buffer. This might be less than the number of bytes requested if that many bytes are not available, or it might be zero if the end of the stream is reached.</returns>
		public static int ReadReverse(this BinaryReader reader, byte[] buffer, int index, int count)
		{
			int retval = reader.Read(buffer, index, count);
			buffer = buffer.Reverse().ToArray();
			return retval;
		}

		/// <summary>
		/// Reads the specified number of characters from the stream, starting from a specified point in the character array.
		/// </summary>
		/// <param name="buffer">The buffer to read data into.</param>
		/// <param name="index">The starting point in the buffer at which to begin reading into the buffer.</param>
		/// <param name="count">The number of characters to read.</param>
		/// <returns>The total number of characters read into the buffer. This might be less than the number of characters requested if that many characters are not currently available, or it might be zero if the end of the stream is reached.</returns>
		public static int ReadReverse(this BinaryReader reader, char[] buffer, int index, int count)

		{
			int retval = reader.Read(buffer, index, count);
			buffer = buffer.Reverse().ToArray();
			return retval;
		}

		/// <summary>
		/// Reads the specified number of bytes from the current stream into a byte array and advances the current position by that number of bytes.
		/// </summary>
		/// <param name="count">The number of bytes to read. This value must be 0 or a non-negative number or an exception will occur.</param>
		/// <returns>A byte array containing data read from the underlying stream. This might be less than the number of bytes requested if the end of the stream is reached.</returns>
		public static byte[] ReadBytesReverse(this BinaryReader reader, int count)
		{
			byte[] retval = reader.ReadBytes(count);
			retval = retval.Reverse().ToArray();
			return retval;
		}

		/// <summary>
		/// Reads a decimal value from the current stream and advances the current position of the stream by sixteen bytes.
		/// </summary>
		/// <returns>A decimal value read from the current stream.</returns>
		public static decimal ReadDecimalReverse(this BinaryReader reader)
		{
			byte[] retval = reader.ReadBytes(16);
			retval = retval.Reverse().ToArray();

			int i1 = BitConverter.ToInt32(retval, 0);
			int i2 = BitConverter.ToInt32(retval, 4);
			int i3 = BitConverter.ToInt32(retval, 8);
			int i4 = BitConverter.ToInt32(retval, 12);

			return new decimal(new int[] { i1, i2, i3, i4 });
		}

		/// <summary>
		/// eads an 8-byte floating point value from the current stream and advances the current position of the stream by eight bytes.
		/// </summary>
		/// <returns>An 8-byte floating point value read from the current stream.</returns>
		public static double ReadDoubleReverse(this BinaryReader reader)
		{
			byte[] retval = reader.ReadBytes(8);
			retval = retval.Reverse().ToArray();
			return BitConverter.ToDouble(retval, 0);
		}

		/// <summary>
		/// Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two bytes.
		/// </summary>
		/// <returns>A 2-byte signed integer read from the current stream.</returns>
		public static short ReadInt16Reverse(this BinaryReader reader)
		{
			byte[] retval = reader.ReadBytes(2);
			retval = retval.Reverse().ToArray();
			return BitConverter.ToInt16(retval, 0);
		}

		/// <summary>
		/// Reads a 4-byte signed integer from the current stream and advances the current position of the stream by four bytes.
		/// </summary>
		/// <returns>A 4-byte signed integer read from the current stream.</returns>
		public static int ReadInt32Reverse(this BinaryReader reader)
		{
			byte[] retval = reader.ReadBytes(4);
			retval = retval.Reverse().ToArray();
			return BitConverter.ToInt32(retval, 0);
		}

		/// <summary>
		/// Reads an 8-byte signed integer from the current stream and advances the current position of the stream by eight bytes.
		/// </summary>
		/// <returns>An 8-byte signed integer read from the current stream.</returns>
		public static long ReadInt64Reverse(this BinaryReader reader)
		{
			byte[] retval = reader.ReadBytes(8);
			retval = retval.Reverse().ToArray();
			return BitConverter.ToInt64(retval, 0);
		}

		/// <summary>
		/// Reads a 4-byte floating point value from the current stream and advances the current position of the stream by four bytes.
		/// </summary>
		/// <returns>A 4-byte floating point value read from the current stream.</returns>
		public static float ReadSingleReverse(this BinaryReader reader)
		{
			byte[] retval = reader.ReadBytes(4);
			retval = retval.Reverse().ToArray();
			return BitConverter.ToSingle(retval, 0);
		}

		/// <summary>
		/// Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the position of the stream by two bytes.
		/// 
		/// This API is not CLS-compliant.
		/// </summary>
		/// <returns>A 2-byte unsigned integer read from this stream.</returns>
		public static ushort ReadUInt16Reverse(this BinaryReader reader)
		{
			byte[] retval = reader.ReadBytes(2);
			retval = retval.Reverse().ToArray();
			return BitConverter.ToUInt16(retval, 0);
		}

		/// <summary>
		/// Reads a 4-byte unsigned integer from the current stream and advances the position of the stream by four bytes.
		/// 
		/// This API is not CLS-compliant.
		/// </summary>
		/// <returns>A 4-byte unsigned integer read from this stream.</returns>
		public static uint ReadUInt32Reverse(this BinaryReader reader)
		{
			byte[] retval = reader.ReadBytes(4);
			retval = retval.Reverse().ToArray();
			return BitConverter.ToUInt32(retval, 0);
		}

		/// <summary>
		/// Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight bytes.
		/// 
		/// This API is not CLS-compliant.
		/// </summary>
		/// <returns>An 8-byte unsigned integer read from this stream.</returns>
		public static ulong ReadUInt64Reverse(this BinaryReader reader)
		{
			byte[] retval = reader.ReadBytes(8);
			retval = retval.Reverse().ToArray();
			return BitConverter.ToUInt64(retval, 0);
		}

		#endregion

		#region String Manipulation

		/// <summary>
		/// Compare strings as numeric
		/// </summary>
		/// <param name="s1">First string to compare</param>
		/// <param name="s2">Second string to compare</param>
		/// <returns>-1 if s1 comes before s2, 0 if s1 and s2 are equal, 1 if s1 comes after s2</returns>
		/// <remarks>I want to be able to handle paths properly with no issue, can I do a recursive call based on separated by path separator?</remarks>
		public static int CompareNumeric(string s1, string s2)
		{
			// Save the orginal strings, for later comparison
			string s1orig = s1;
			string s2orig = s2;

			// We want to normalize the strings, so we set both to lower case
			s1 = s1.ToLowerInvariant();
			s2 = s2.ToLowerInvariant();

			// If the strings are the same exactly, return
			if (s1 == s2)
			{
				return s1orig.CompareTo(s2orig);
			}

			// If one is null, then say that's less than
			if (s1 == null)
			{
				return -1;
			}
			if (s2 == null)
			{
				return 1;
			}

			// Now split into path parts after converting AltDirSeparator to DirSeparator
			s1 = s1.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			s2 = s2.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string[] s1parts = s1.Split(Path.DirectorySeparatorChar);
			string[] s2parts = s2.Split(Path.DirectorySeparatorChar);

			// Then compare each part in turn
			for (int j = 0; j < s1parts.Length && j < s2parts.Length; j++)
			{
				int compared = CompareNumericPart(s1parts[j], s2parts[j]);
				if (compared != 0)
				{
					return compared;
				}
			}

			// If we got out here, then it looped through at least one of the strings
			if (s1parts.Length > s2parts.Length)
			{
				return 1;
			}
			if (s1parts.Length < s2parts.Length)
			{
				return -1;
			}

			return s1orig.CompareTo(s2orig);
		}

		/// <summary>
		/// Helper for CompareNumeric
		/// </summary>
		/// <param name="s1">First string to compare</param>
		/// <param name="s2">Second string to compare</param>
		/// <returns>-1 if s1 comes before s2, 0 if s1 and s2 are equal, 1 if s1 comes after s2</returns>
		private static int CompareNumericPart(string s1, string s2)
		{
			// Otherwise, loop through until we have an answer
			for (int i = 0; i < s1.Length && i < s2.Length; i++)
			{
				int s1c = s1[i];
				int s2c = s2[i];

				// If the characters are the same, continue
				if (s1c == s2c)
				{
					continue;
				}

				// If they're different, check which one was larger
				if (s1c > s2c)
				{
					return 1;
				}
				if (s1c < s2c)
				{
					return -1;
				}
			}

			// If we got out here, then it looped through at least one of the strings
			if (s1.Length > s2.Length)
			{
				return 1;
			}
			if (s1.Length < s2.Length)
			{
				return -1;
			}

			return 0;
		}

		/// <summary>
		/// Convert all characters that are not considered XML-safe
		/// </summary>
		/// <param name="s">Input string to clean</param>
		/// <returns>Cleaned string</returns>
		public static string ConvertXMLUnsafeCharacters(string s)
		{
			return new String(s.Select(c =>
				(c == 0x9
					|| c == 0xA
					|| c == 0xD
					|| (c >= 0x20 && c <= 0xD77F)
					|| (c >= 0xE000 && c <= 0xFFFD)
					|| (c >= 0x10000 && c <= 0x10FFFF)
						? c
						: HttpUtility.HtmlEncode(c)[0]))
				.ToArray());
		}

		/// <summary>
		/// Get a proper romba sub path
		/// </summary>
		/// <param name="hash">SHA-1 hash to get the path for</param>
		/// <returns>Subfolder path for the given hash</returns>
		public static string GetRombaPath(string hash)
		{
			// If the hash isn't the right size, then we return null
			if (hash.Length != Constants.SHA1Length) // TODO: When updating to SHA-256, this needs to update to Constants.SHA256Length
			{
				return null;
			}

			return Path.Combine(hash.Substring(0, 2), hash.Substring(2, 2), hash.Substring(4, 2), hash.Substring(6, 2), hash + ".gz");
		}

		/// <summary>
		/// Get the multiplier to be used with the size given
		/// </summary>
		/// <param name="sizestring">String with possible size with extension</param>
		/// <returns>Tuple of multiplier to use on final size and fixed size string</returns>
		public static long GetSizeFromString(string sizestring)
		{
			// Make sure the string is in lower case
			sizestring = sizestring.ToLowerInvariant();

			// Get any trailing size identifiers
			long multiplier = 1;
			if (sizestring.EndsWith("k") || sizestring.EndsWith("kb"))
			{
				multiplier = Constants.KiloByte;
			}
			else if (sizestring.EndsWith("ki") || sizestring.EndsWith("kib"))
			{
				multiplier = Constants.KibiByte;
			}
			else if (sizestring.EndsWith("m") || sizestring.EndsWith("mb"))
			{
				multiplier = Constants.MegaByte;
			}
			else if (sizestring.EndsWith("mi") || sizestring.EndsWith("mib"))
			{
				multiplier = Constants.MibiByte;
			}
			else if (sizestring.EndsWith("g") || sizestring.EndsWith("gb"))
			{
				multiplier = Constants.GigaByte;
			}
			else if (sizestring.EndsWith("gi") || sizestring.EndsWith("gib"))
			{
				multiplier = Constants.GibiByte;
			}
			else if (sizestring.EndsWith("t") || sizestring.EndsWith("tb"))
			{
				multiplier = Constants.TeraByte;
			}
			else if (sizestring.EndsWith("ti") || sizestring.EndsWith("tib"))
			{
				multiplier = Constants.TibiByte;
			}
			else if (sizestring.EndsWith("p") || sizestring.EndsWith("pb"))
			{
				multiplier = Constants.PetaByte;
			}
			else if (sizestring.EndsWith("pi") || sizestring.EndsWith("pib"))
			{
				multiplier = Constants.PibiByte;
			}

			// Remove any trailing identifiers
			sizestring = sizestring.TrimEnd(new char[] { 'k', 'm', 'g', 't', 'p', 'i', 'b', ' ' });

			// Now try to get the size from the string
			if (!Int64.TryParse(sizestring, out long size))
			{
				size = -1;
			}
			else
			{
				size *= multiplier;
			}

			return size;
		}

		/// <summary>
		/// Get if a string contains Unicode characters
		/// </summary>
		/// <param name="s">Input string to test</param>
		/// <returns>True if the string contains at least one Unicode character, false otherwise</returns>
		public static bool IsUnicode(string s)
		{
			return (s.Any(c => c > 255));
		}

		/// <summary>
		/// Remove all chars that are considered path unsafe
		/// </summary>
		/// <param name="s">Input string to clean</param>
		/// <returns>Cleaned string</returns>
		public static string RemovePathUnsafeCharacters(string s)
		{
			List<char> invalidPath = Path.GetInvalidPathChars().ToList();
			return new string(s.Where(c => !invalidPath.Contains(c)).ToArray());
		}

		/// <summary>
		/// Remove all unicode-specific chars from a string
		/// </summary>
		/// <param name="s">Input string to clean</param>
		/// <returns>Cleaned string</returns>
		public static string RemoveUnicodeCharacters(string s)
		{
			return new string(s.Where(c => c <= 255).ToArray());
		}

		/// <summary>
		/// Split a line as if it were a CMP rom line
		/// </summary>
		/// <param name="s">Line to split</param>
		/// <returns>Line split</returns>
		/// <remarks>Uses code from http://stackoverflow.com/questions/554013/regular-expression-to-split-on-spaces-unless-in-quotes</remarks>
		public static string[] SplitLineAsCMP(string s)
		{
			// Get the opening and closing brace locations
			int openParenLoc = s.IndexOf('(');
			int closeParenLoc = s.LastIndexOf(')');

			// Now remove anything outside of those braces, including the braces
			s = s.Substring(openParenLoc + 1, closeParenLoc - openParenLoc - 1);
			s = s.Trim();

			// Now we get each string, divided up as cleanly as possible
			string[] matches = Regex
				//.Matches(s, @"([^\s]*""[^""]+""[^\s]*)|[^""]?\w+[^""]?")
				.Matches(s, @"[^\s""]+|""[^""]*""")
				.Cast<Match>()
				.Select(m => m.Groups[0].Value)
				.ToArray();

			return matches;
		}

		#endregion

		#region System.IO.Path Replacements

		/// <summary>
		/// Replacement for System.IO.Path.GetDirectoryName
		/// </summary>
		/// <param name="s">Path to get directory name out of</param>
		/// <returns>Directory name from path</returns>
		/// <see cref="System.IO.Path.GetDirectoryName(string)"/>
		public static string GetDirectoryName(string s)
		{
			if (s == null)
			{
				return "";
			}

			if (s.Contains("/"))
			{
				string[] tempkey = s.Split('/');
				return String.Join("/", tempkey.Take(tempkey.Length - 1));
			}
			else if (s.Contains("\\"))
			{
				string[] tempkey = s.Split('\\');
				return String.Join("\\", tempkey.Take(tempkey.Length - 1));
			}

			return "";
		}

		/// <summary>
		/// Replacement for System.IO.Path.GetFileName
		/// </summary>
		/// <param name="s">Path to get file name out of</param>
		/// <returns>File name from path</returns>
		/// <see cref="System.IO.Path.GetFileName(string)"/>
		public static string GetFileName(string s)
		{
			if (s == null)
			{
				return "";
			}

			if (s.Contains("/"))
			{
				string[] tempkey = s.Split('/');
				return tempkey.Last();
			}
			else if (s.Contains("\\"))
			{
				string[] tempkey = s.Split('\\');
				return tempkey.Last();
			}

			return s;
		}

		/// <summary>
		/// Replacement for System.IO.Path.GetFileNameWithoutExtension
		/// </summary>
		/// <param name="s">Path to get file name out of</param>
		/// <returns>File name without extension from path</returns>
		/// <see cref="System.IO.Path.GetFileNameWithoutExtension(string)"/>
		public static string GetFileNameWithoutExtension(string s)
		{
			s = GetFileName(s);
			string[] tempkey = s.Split('.');
			if (tempkey.Count() == 1)
			{
				return s;
			}
			return String.Join(".", tempkey.Take(tempkey.Length - 1));
		}

		#endregion

		#region WoD-based String Cleaning

		/// <summary>
		/// Replace accented characters
		/// </summary>
		/// <param name="input">String to be parsed</param>
		/// <returns>String with characters replaced</returns>
		public static string NormalizeChars(string input)
		{
			string[,] charmap = {
				{ "Á", "A" },   { "á", "a" },
				{ "À", "A" },   { "à", "a" },
				{ "Â", "A" },   { "â", "a" },
				{ "Ä", "Ae" },  { "ä", "ae" },
				{ "Ã", "A" },   { "ã", "a" },
				{ "Å", "A" },   { "å", "a" },
				{ "Æ", "Ae" },  { "æ", "ae" },
				{ "Ç", "C" },   { "ç", "c" },
				{ "Ð", "D" },   { "ð", "d" },
				{ "É", "E" },   { "é", "e" },
				{ "È", "E" },   { "è", "e" },
				{ "Ê", "E" },   { "ê", "e" },
				{ "Ë", "E" },   { "ë", "e" },
				{ "ƒ", "f" },
				{ "Í", "I" },   { "í", "i" },
				{ "Ì", "I" },   { "ì", "i" },
				{ "Î", "I" },   { "î", "i" },
				{ "Ï", "I" },   { "ï", "i" },
				{ "Ñ", "N" },   { "ñ", "n" },
				{ "Ó", "O" },   { "ó", "o" },
				{ "Ò", "O" },   { "ò", "o" },
				{ "Ô", "O" },   { "ô", "o" },
				{ "Ö", "Oe" },  { "ö", "oe" },
				{ "Õ", "O" },   { "õ", "o" },
				{ "Ø", "O" },   { "ø", "o" },
				{ "Š", "S" },   { "š", "s" },
				{ "ß", "ss" },
				{ "Þ", "B" },   { "þ", "b" },
				{ "Ú", "U" },   { "ú", "u" },
				{ "Ù", "U" },   { "ù", "u" },
				{ "Û", "U" },   { "û", "u" },
				{ "Ü", "Ue" },  { "ü", "ue" },
				{ "ÿ", "y" },
				{ "Ý", "Y" },   { "ý", "y" },
				{ "Ž", "Z" },   { "ž", "z" },
			};

			for (int i = 0; i < charmap.GetLength(0); i++)
			{
				input = input.Replace(charmap[i, 0], charmap[i, 1]);
			}

			return input;
		}

		/// <summary>
		/// Convert Cyrillic lettering to Latin lettering
		/// </summary>
		/// <param name="input">String to be parsed</param>
		/// <returns>String with characters replaced</returns>
		public static string RussianToLatin(string input)
		{
			string[,] charmap = {
					{ "А", "A" }, { "Б", "B" }, { "В", "V" }, { "Г", "G" }, { "Д", "D" },
					{ "Е", "E" }, { "Ё", "Yo" }, { "Ж", "Zh" }, { "З", "Z" }, { "И", "I" },
					{ "Й", "J" }, { "К", "K" }, { "Л", "L" }, { "М", "M" }, { "Н", "N" },
					{ "О", "O" }, { "П", "P" }, { "Р", "R" }, { "С", "S" }, { "Т", "T" },
					{ "У", "U" }, { "Ф", "f" }, { "Х", "Kh" }, { "Ц", "Ts" }, { "Ч", "Ch" },
					{ "Ш", "Sh" }, { "Щ", "Sch" }, { "Ъ", "" }, { "Ы", "y" }, { "Ь", "" },
					{ "Э", "e" }, { "Ю", "yu" }, { "Я", "ya" }, { "а", "a" }, { "б", "b" },
					{ "в", "v" }, { "г", "g" }, { "д", "d" }, { "е", "e" }, { "ё", "yo" },
					{ "ж", "zh" }, { "з", "z" }, { "и", "i" }, { "й", "j" }, { "к", "k" },
					{ "л", "l" }, { "м", "m" }, { "н", "n" }, { "о", "o" }, { "п", "p" },
					{ "р", "r" }, { "с", "s" }, { "т", "t" }, { "у", "u" }, { "ф", "f" },
					{ "х", "kh" }, { "ц", "ts" }, { "ч", "ch" }, { "ш", "sh" }, { "щ", "sch" },
					{ "ъ", "" }, { "ы", "y" }, { "ь", "" }, { "э", "e" }, { "ю", "yu" },
					{ "я", "ya" },
			};

			for (int i = 0; i < charmap.GetLength(0); i++)
			{
				input = input.Replace(charmap[i, 0], charmap[i, 1]);
			}

			return input;
		}

		/// <summary>
		/// Replace special characters and patterns
		/// </summary>
		/// <param name="input">String to be parsed</param>
		/// <returns>String with characters replaced</returns>
		public static string SearchPattern(string input)
		{
			string[,] charmap = {
				{ @"~", " - " },
				{ @"_", " " },
				{ @":", " " },
				{ @">", ")" },
				{ @"<", "(" },
				{ @"\|", "-" },
				{ "\"", "'" },
				{ @"\*", "." },
				{ @"\\", "-" },
				{ @"/", "-" },
				{ @"\?", " " },
				{ @"\(([^)(]*)\(([^)]*)\)([^)(]*)\)", " " },
				{ @"\(([^)]+)\)", " " },
				{ @"\[([^]]+)\]", " " },
				{ @"\{([^}]+)\}", " " },
				{ @"(ZZZJUNK|ZZZ-UNK-|ZZZ-UNK |zzz unknow |zzz unk |Copy of |[.][a-z]{3}[.][a-z]{3}[.]|[.][a-z]{3}[.])", " " },
				{ @" (r|rev|v|ver)\s*[\d\.]+[^\s]*", " " },
				{ @"(( )|(\A))(\d{6}|\d{8})(( )|(\Z))", " " },
				{ @"(( )|(\A))(\d{1,2})-(\d{1,2})-(\d{4}|\d{2})", " " },
				{ @"(( )|(\A))(\d{4}|\d{2})-(\d{1,2})-(\d{1,2})", " " },
				{ @"[-]+", "-" },
				{ @"\A\s*\)", " " },
				{ @"\A\s*(,|-)", " " },
				{ @"\s+", " " },
				{ @"\s+,", "," },
				{ @"\s*(,|-)\s*\Z", " " },
			};

			for (int i = 0; i < charmap.GetLength(0); i++)
			{
				input = Regex.Replace(input, charmap[i, 0], charmap[i, 1]);
			}

			return input;
		}

		#endregion

		#region Externally sourced methods

		/// <summary>
		///  Returns the human-readable file size for an arbitrary, 64-bit file size 
		/// The default format is "0.### XB", e.g. "4.2 KB" or "1.434 GB"
		/// </summary>
		/// <param name="input"></param>
		/// <returns>Human-readable file size</returns>
		/// <link>http://www.somacon.com/p576.php</link>
		public static string GetBytesReadable(long input)
		{
			// Get absolute value
			long absolute_i = (input < 0 ? -input : input);
			// Determine the suffix and readable value
			string suffix;
			double readable;
			if (absolute_i >= 0x1000000000000000) // Exabyte
			{
				suffix = "EB";
				readable = (input >> 50);
			}
			else if (absolute_i >= 0x4000000000000) // Petabyte
			{
				suffix = "PB";
				readable = (input >> 40);
			}
			else if (absolute_i >= 0x10000000000) // Terabyte
			{
				suffix = "TB";
				readable = (input >> 30);
			}
			else if (absolute_i >= 0x40000000) // Gigabyte
			{
				suffix = "GB";
				readable = (input >> 20);
			}
			else if (absolute_i >= 0x100000) // Megabyte
			{
				suffix = "MB";
				readable = (input >> 10);
			}
			else if (absolute_i >= 0x400) // Kilobyte
			{
				suffix = "KB";
				readable = input;
			}
			else
			{
				return input.ToString("0 B"); // Byte
			}
			// Divide by 1024 to get fractional value
			readable = (readable / 1024);
			// Return formatted number with suffix
			return readable.ToString("0.### ") + suffix;
		}

		/// <summary>
		/// http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
		/// </summary>
		public static string ByteArrayToString(byte[] bytes)
		{
			try
			{
				string hex = BitConverter.ToString(bytes);
				return hex.Replace("-", string.Empty).ToLowerInvariant();
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// http://stackoverflow.com/questions/311165/how-do-you-convert-byte-array-to-hexadecimal-string-and-vice-versa
		/// </summary>
		public static byte[] StringToByteArray(string hex)
		{
			try
			{
				int NumberChars = hex.Length;
				byte[] bytes = new byte[NumberChars / 2];
				for (int i = 0; i < NumberChars; i += 2)
					bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
				return bytes;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// http://stackoverflow.com/questions/5613279/c-sharp-hex-to-ascii
		/// </summary>
		public static string ConvertHexToAscii(string hexString)
		{
			if (hexString.Contains("-"))
			{
				hexString = hexString.Replace("-", "");
			}

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < hexString.Length; i += 2)
			{
				String hs = hexString.Substring(i, 2);
				sb.Append(Convert.ToChar(Convert.ToUInt32(hs, 16)));
			}

			return sb.ToString();
		}

		/// <summary>
		/// http://stackoverflow.com/questions/15920741/convert-from-string-ascii-to-string-hex
		/// </summary>
		public static string ConvertAsciiToHex(string asciiString)
		{
			string hexOutput = "";
			foreach (char _eachChar in asciiString.ToCharArray())
			{
				// Get the integral value of the character.
				int value = Convert.ToInt32(_eachChar);
				// Convert the decimal value to a hexadecimal value in string form.
				hexOutput += String.Format("{0:X2}", value).Remove(0, 2);
				// to make output as your eg 
				//  hexOutput +=" "+ String.Format("{0:X}", value);
			}

			return hexOutput;
		}

		/// <summary>
		/// Adapted from 7-zip Source Code: CPP/Windows/TimeUtils.cpp:FileTimeToDosTime
		/// </summary>
		public static uint ConvertDateTimeToMsDosTimeFormat(DateTime dateTime)
		{
			uint year = (uint)((dateTime.Year - 1980) % 128);
			uint mon = (uint)dateTime.Month;
			uint day = (uint)dateTime.Day;
			uint hour = (uint)dateTime.Hour;
			uint min = (uint)dateTime.Minute;
			uint sec = (uint)dateTime.Second;

			return (year << 25) | (mon << 21) | (day << 16) | (hour << 11) | (min << 5) | (sec >> 1);
		}

		/// <summary>
		/// Adapted from 7-zip Source Code: CPP/Windows/TimeUtils.cpp:DosTimeToFileTime
		/// </summary>
		public static DateTime ConvertMsDosTimeFormatToDateTime(uint msDosDateTime)
		{
			return new DateTime((int)(1980 + (msDosDateTime >> 25)), (int)((msDosDateTime >> 21) & 0xF), (int)((msDosDateTime >> 16) & 0x1F),
				(int)((msDosDateTime >> 11) & 0x1F), (int)((msDosDateTime >> 5) & 0x3F), (int)((msDosDateTime & 0x1F) * 2));
		}

		/// <summary>
		/// Determines a text file's encoding by analyzing its byte order mark (BOM).
		/// Defaults to ASCII when detection of the text file's endianness fails.
		/// http://stackoverflow.com/questions/3825390/effective-way-to-find-any-files-encoding
		/// </summary>
		/// <param name="filename">The text file to analyze.</param>
		/// <returns>The detected encoding.</returns>
		public static Encoding GetEncoding(string filename)
		{
			// Read the BOM
			var bom = new byte[4];
			FileStream file = FileTools.TryOpenRead(filename);
			file.Read(bom, 0, 4);
			file.Dispose();

			// Analyze the BOM
			if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
			if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
			if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
			if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
			if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
			return Encoding.Default;
		}

		/// <summary>
		/// http://stackoverflow.com/questions/1600962/displaying-the-build-date
		/// </summary>
		public static DateTime GetLinkerTime(this Assembly assembly, TimeZoneInfo target = null)
		{
			var filePath = assembly.Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;

			var buffer = new byte[2048];

			using (var stream = FileTools.TryOpenRead(filePath))
				stream.Read(buffer, 0, 2048);

			var offset = BitConverter.ToInt32(buffer, c_PeHeaderOffset);
			var secondsSince1970 = BitConverter.ToInt32(buffer, offset + c_LinkerTimestampOffset);
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

			var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

			var tz = target ?? TimeZoneInfo.Local;
			var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

			return localTime;
		}

		/// <summary>
		/// Indicates whether the specified array is null or has a length of zero.
		/// https://stackoverflow.com/questions/8560106/isnullorempty-equivalent-for-array-c-sharp
		/// </summary>
		/// <param name="array">The array to test.</param>
		/// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
		public static bool IsNullOrEmpty(this Array array)
		{
			return (array == null || array.Length == 0);
		}

		#endregion
	}
}
