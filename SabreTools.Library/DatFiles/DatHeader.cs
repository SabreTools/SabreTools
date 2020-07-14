using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Library.Data;
using Newtonsoft.Json;

namespace SabreTools.Library.DatFiles
{
    /// <summary>
    /// Represents all possible DAT header information
    /// </summary>
    public class DatHeader : ICloneable
    {
        #region Publicly facing variables

        #region Data common to most DAT types

        /// <summary>
        /// External name of the DAT
        /// </summary>
        [JsonProperty("filename")]
        public string FileName { get; set; }

        /// <summary>
        /// Internal name of the DAT
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// DAT description
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Root directory for the files; currently TruRip/EmuARC-exclusive
        /// </summary>
        [JsonProperty("rootdir")]
        public string RootDir { get; set; }

        /// <summary>
        /// General category of items found in the DAT
        /// </summary>
        [JsonProperty("category")]
        public string Category { get; set; }

        /// <summary>
        /// Version of the DAT
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Creation or modification date
        /// </summary>
        [JsonProperty("date")]
        public string Date { get; set; }

        /// <summary>
        /// List of authors who contributed to the DAT
        /// </summary>
        [JsonProperty("author")]
        public string Author { get; set; }

        /// <summary>
        /// Email address for DAT author(s)
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Author or distribution homepage name
        /// </summary>
        [JsonProperty("homepage")]
        public string Homepage { get; set; }

        /// <summary>
        /// Author or distribution URL
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// Any comment that does not already fit an existing field
        /// </summary>
        [JsonProperty("comment")]
        public string Comment { get; set; }

        /// <summary>
        /// Header skipper to be used when loading the DAT
        /// </summary>
        [JsonProperty("header")]
        public string Header { get; set; }

        /// <summary>
        /// Classification of the DAT. Generally only used for SuperDAT
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Force a merging style when loaded
        /// </summary>
        [JsonProperty("forcemerging")]
        public ForceMerging ForceMerging { get; set; }

        /// <summary>
        /// Force nodump handling when loaded
        /// </summary>
        [JsonProperty("forcenodump")]
        public ForceNodump ForceNodump { get; set; }

        /// <summary>
        /// Force output packing when loaded
        /// </summary>
        [JsonProperty("forcepacking")]
        public ForcePacking ForcePacking { get; set; }

        /// <summary>
        /// Read or write format
        /// </summary>
        [JsonIgnore]
        public DatFormat DatFormat { get; set; }

        /// <summary>
        /// List of fields in machine and items to exclude from writing
        /// </summary>
        [JsonIgnore]
        public bool[] ExcludeFields { get; set; } = new bool[Enum.GetNames(typeof(Field)).Length];

        /// <summary>
        /// Enable "One Rom, One Region (1G1R)" mode
        /// </summary>
        [JsonIgnore]
        public bool OneRom { get; set; }

        /// <summary>
        /// Keep machines that don't contain any items
        /// </summary>
        [JsonIgnore]
        public bool KeepEmptyGames { get; set; }

        /// <summary>
        /// Remove scene dates from the beginning of machine names
        /// </summary>
        [JsonIgnore]
        public bool SceneDateStrip { get; set; }

        /// <summary>
        /// Deduplicate items using the given method
        /// </summary>
        [JsonIgnore]
        public DedupeType DedupeRoms { get; set; }

        /// <summary>
        /// Strip hash types from items
        /// </summary>
        [JsonIgnore]
        public Hash StripHash { get; private set; }

        #endregion

        #region Write pre-processing

        /// <summary>
        /// Text to prepend to all outputted lines
        /// </summary>
        [JsonIgnore]
        public string Prefix { get; set; }

        /// <summary>
        /// Text to append to all outputted lines
        /// </summary>
        [JsonIgnore]
        public string Postfix { get; set; }

        /// <summary>
        /// Add a new extension to all items
        /// </summary>
        [JsonIgnore]
        public string AddExtension { get; set; }

        /// <summary>
        /// Replace all item extensions
        /// </summary>
        [JsonIgnore]
        public string ReplaceExtension { get; set; }

        /// <summary>
        /// Remove all item extensions
        /// </summary>
        [JsonIgnore]
        public bool RemoveExtension { get; set; }

        /// <summary>
        /// Romba output mode
        /// </summary>
        [JsonIgnore]
        public bool Romba { get; set; }

        /// <summary>
        /// Output the machine name
        /// </summary>
        [JsonIgnore]
        public bool GameName { get; set; }

        /// <summary>
        /// Wrap quotes around the entire line, sans prefix and postfix
        /// </summary>
        [JsonIgnore]
        public bool Quotes { get; set; }

        #endregion

        #region Data specific to the Miss DAT type

        /// <summary>
        /// Output the item name
        /// </summary>
        [JsonIgnore]
        public bool UseRomName { get; set; }

        #endregion

        #endregion

        #region Instance Methods

        #region Cloning Methods

        /// <summary>
        /// Clone the current header
        /// </summary>
        public object Clone()
        {
            return new DatHeader()
            {
                FileName = this.FileName,
                Name = this.Name,
                Description = this.Description,
                RootDir = this.RootDir,
                Category = this.Category,
                Version = this.Version,
                Date = this.Date,
                Author = this.Author,
                Email = this.Email,
                Homepage = this.Homepage,
                Url = this.Url,
                Comment = this.Comment,
                Header = this.Header,
                Type = this.Type,
                ForceMerging = this.ForceMerging,
                ForceNodump = this.ForceNodump,
                ForcePacking = this.ForcePacking,
                DatFormat = this.DatFormat,
                ExcludeFields = this.ExcludeFields,
                OneRom = this.OneRom,
                KeepEmptyGames = this.KeepEmptyGames,
                SceneDateStrip = this.SceneDateStrip,
                DedupeRoms = this.DedupeRoms,
                StripHash = this.StripHash,

                UseRomName = this.UseRomName,
                Prefix = this.Prefix,
                Postfix = this.Postfix,
                Quotes = this.Quotes,
                ReplaceExtension = this.ReplaceExtension,
                AddExtension = this.AddExtension,
                RemoveExtension = this.RemoveExtension,
                GameName = this.GameName,
                Romba = this.Romba,
            };
        }

        /// <summary>
        /// Clone the standard parts of the current header
        /// </summary>
        public DatHeader CloneStandard()
        {
            return new DatHeader()
            {
                FileName = this.FileName,
                Name = this.Name,
                Description = this.Description,
                RootDir = this.RootDir,
                Category = this.Category,
                Version = this.Version,
                Date = this.Date,
                Author = this.Author,
                Email = this.Email,
                Homepage = this.Homepage,
                Url = this.Url,
                Comment = this.Comment,
                Header = this.Header,
                Type = this.Type,
                ForceMerging = this.ForceMerging,
                ForceNodump = this.ForceNodump,
                ForcePacking = this.ForcePacking,
                DatFormat = this.DatFormat,
                ExcludeFields = this.ExcludeFields,
                OneRom = this.OneRom,
                KeepEmptyGames = this.KeepEmptyGames,
                SceneDateStrip = this.SceneDateStrip,
                DedupeRoms = this.DedupeRoms,
                StripHash = this.StripHash,
            };
        }

        /// <summary>
        /// Clone the filtering parts of the current header
        /// </summary>
        public DatHeader CloneFiltering()
        {
            return new DatHeader()
            {
                DatFormat = this.DatFormat,
                ExcludeFields = this.ExcludeFields,
                OneRom = this.OneRom,
                KeepEmptyGames = this.KeepEmptyGames,
                SceneDateStrip = this.SceneDateStrip,
                DedupeRoms = this.DedupeRoms,
                StripHash = this.StripHash,

                UseRomName = this.UseRomName,
                Prefix = this.Prefix,
                Postfix = this.Postfix,
                Quotes = this.Quotes,
                ReplaceExtension = this.ReplaceExtension,
                AddExtension = this.AddExtension,
                RemoveExtension = this.RemoveExtension,
                GameName = this.GameName,
                Romba = this.Romba,
            };
        }

        /// <summary>
        /// Overwrite local values from another DatHeader if not default
        /// </summary>
        /// <param name="datHeader">DatHeader to get the values from</param>
        public void ConditionalCopy(DatHeader datHeader)
        {
            if (!string.IsNullOrWhiteSpace(datHeader.FileName))
                this.FileName = datHeader.FileName;

            if (!string.IsNullOrWhiteSpace(datHeader.Name))
                this.Name = datHeader.Name;

            if (!string.IsNullOrWhiteSpace(datHeader.Description))
                this.Description = datHeader.Description;

            if (!string.IsNullOrWhiteSpace(datHeader.RootDir))
                this.RootDir = datHeader.RootDir;

            if (!string.IsNullOrWhiteSpace(datHeader.Category))
                this.Category = datHeader.Category;

            if (!string.IsNullOrWhiteSpace(datHeader.Version))
                this.Version = datHeader.Version;

            if (!string.IsNullOrWhiteSpace(datHeader.Date))
                this.Date = datHeader.Date;

            if (!string.IsNullOrWhiteSpace(datHeader.Author))
                this.Author = datHeader.Author;

            if (!string.IsNullOrWhiteSpace(datHeader.Email))
                this.Email = datHeader.Email;

            if (!string.IsNullOrWhiteSpace(datHeader.Homepage))
                this.Homepage = datHeader.Homepage;

            if (!string.IsNullOrWhiteSpace(datHeader.Url))
                this.Url = datHeader.Url;

            if (!string.IsNullOrWhiteSpace(datHeader.Comment))
                this.Comment = datHeader.Comment;

            if (!string.IsNullOrWhiteSpace(datHeader.Header))
                this.Header = datHeader.Header;

            if (!string.IsNullOrWhiteSpace(datHeader.Type))
                this.Type = datHeader.Type;

            if (datHeader.ForceMerging != ForceMerging.None)
                this.ForceMerging = datHeader.ForceMerging;

            if (datHeader.ForceNodump != ForceNodump.None)
                this.ForceNodump = datHeader.ForceNodump;

            if (datHeader.ForcePacking != ForcePacking.None)
                this.ForcePacking = datHeader.ForcePacking;

            if (datHeader.DatFormat != 0x00)
                this.DatFormat = datHeader.DatFormat;

            if (datHeader.ExcludeFields != null)
                this.ExcludeFields = datHeader.ExcludeFields;

            this.OneRom = datHeader.OneRom;
            this.KeepEmptyGames = datHeader.KeepEmptyGames;
            this.SceneDateStrip = datHeader.SceneDateStrip;
            this.DedupeRoms = datHeader.DedupeRoms;
            //this.StripHash = datHeader.StripHash;

            if (!string.IsNullOrWhiteSpace(datHeader.Prefix))
                this.Prefix = datHeader.Prefix;

            if (!string.IsNullOrWhiteSpace(datHeader.Postfix))
                this.Postfix = datHeader.Postfix;

            if (!string.IsNullOrWhiteSpace(datHeader.AddExtension))
                this.AddExtension = datHeader.AddExtension;

            if (!string.IsNullOrWhiteSpace(datHeader.ReplaceExtension))
                this.ReplaceExtension = datHeader.ReplaceExtension;

            this.RemoveExtension = datHeader.RemoveExtension;
            this.Romba = datHeader.Romba;
            this.GameName = datHeader.GameName;
            this.Quotes = datHeader.Quotes;
            this.UseRomName = datHeader.UseRomName;
        }

        #endregion

        #region Writing

        /// <summary>
        /// Generate a proper outfile name based on a DAT and output directory
        /// </summary>
        /// <param name="outDir">Output directory</param>
        /// <param name="overwrite">True if we ignore existing files (default), false otherwise</param>
        /// <returns>Dictionary of output formats mapped to file names</returns>
        public Dictionary<DatFormat, string> CreateOutFileNames(string outDir, bool overwrite = true)
        {
            // Create the output dictionary
            Dictionary<DatFormat, string> outfileNames = new Dictionary<DatFormat, string>();

            // Double check the outDir for the end delim
            if (!outDir.EndsWith(Path.DirectorySeparatorChar.ToString()))
                outDir += Path.DirectorySeparatorChar;

            // Get all used extensions
            List<string> usedExtensions = new List<string>();

            // Get the extensions from the output type
            // TODO: Can the system of adding be more automated?

            #region .csv

            // CSV
            if (DatFormat.HasFlag(DatFormat.CSV))
            {
                outfileNames.Add(DatFormat.CSV, CreateOutFileNamesHelper(outDir, ".csv", overwrite));
                usedExtensions.Add(".csv");
            };

            #endregion

            #region .dat

            // ClrMamePro
            if (DatFormat.HasFlag(DatFormat.ClrMamePro))
            {
                outfileNames.Add(DatFormat.ClrMamePro, CreateOutFileNamesHelper(outDir, ".dat", overwrite));
                usedExtensions.Add(".dat");
            };

            // RomCenter
            if (DatFormat.HasFlag(DatFormat.RomCenter))
            {
                if (usedExtensions.Contains(".dat"))
                {
                    outfileNames.Add(DatFormat.RomCenter, CreateOutFileNamesHelper(outDir, ".rc.dat", overwrite));
                    usedExtensions.Add(".rc.dat");
                }
                else
                {
                    outfileNames.Add(DatFormat.RomCenter, CreateOutFileNamesHelper(outDir, ".dat", overwrite));
                    usedExtensions.Add(".dat");
                }
            }

            // DOSCenter
            if (DatFormat.HasFlag(DatFormat.DOSCenter))
            {
                if (usedExtensions.Contains(".dat"))
                {
                    outfileNames.Add(DatFormat.DOSCenter, CreateOutFileNamesHelper(outDir, ".dc.dat", overwrite));
                    usedExtensions.Add(".dc.dat");
                }
                else
                {
                    outfileNames.Add(DatFormat.DOSCenter, CreateOutFileNamesHelper(outDir, ".dat", overwrite));
                    usedExtensions.Add(".dat");
                }
            }

            #endregion

            #region .json

            // JSON
            if (DatFormat.HasFlag(DatFormat.Json))
            {
                outfileNames.Add(DatFormat.Json, CreateOutFileNamesHelper(outDir, ".json", overwrite));
                usedExtensions.Add(".json");
            }

            #endregion

            #region .md5

            // Redump MD5
            if (DatFormat.HasFlag(DatFormat.RedumpMD5))
            {
                outfileNames.Add(DatFormat.RedumpMD5, CreateOutFileNamesHelper(outDir, ".md5", overwrite));
                usedExtensions.Add(".md5");
            };

            #endregion

#if NET_FRAMEWORK
            #region .ripemd160

            // Redump RIPEMD160
            if (DatFormat.HasFlag(DatFormat.RedumpRIPEMD160))
            {
                outfileNames.Add(DatFormat.RedumpRIPEMD160, CreateOutFileNamesHelper(outDir, ".ripemd160", overwrite));
                usedExtensions.Add(".ripemd160");
            };

            #endregion
#endif

            #region .sfv

            // Redump SFV
            if (DatFormat.HasFlag(DatFormat.RedumpSFV))
            {
                outfileNames.Add(DatFormat.RedumpSFV, CreateOutFileNamesHelper(outDir, ".sfv", overwrite));
                usedExtensions.Add(".sfv");
            };

            #endregion

            #region .sha1

            // Redump SHA-1
            if (DatFormat.HasFlag(DatFormat.RedumpSHA1))
            {
                outfileNames.Add(DatFormat.RedumpSHA1, CreateOutFileNamesHelper(outDir, ".sha1", overwrite));
                usedExtensions.Add(".sha1");
            };

            #endregion

            #region .sha256

            // Redump SHA-256
            if (DatFormat.HasFlag(DatFormat.RedumpSHA256))
            {
                outfileNames.Add(DatFormat.RedumpSHA256, CreateOutFileNamesHelper(outDir, ".sha256", overwrite));
                usedExtensions.Add(".sha256");
            };

            #endregion

            #region .sha384

            // Redump SHA-384
            if (DatFormat.HasFlag(DatFormat.RedumpSHA384))
            {
                outfileNames.Add(DatFormat.RedumpSHA384, CreateOutFileNamesHelper(outDir, ".sha384", overwrite));
                usedExtensions.Add(".sha384");
            };

            #endregion

            #region .sha512

            // Redump SHA-512
            if (DatFormat.HasFlag(DatFormat.RedumpSHA512))
            {
                outfileNames.Add(DatFormat.RedumpSHA512, CreateOutFileNamesHelper(outDir, ".sha512", overwrite));
                usedExtensions.Add(".sha512");
            };

            #endregion

            #region .ssv

            // SSV
            if (DatFormat.HasFlag(DatFormat.SSV))
            {
                outfileNames.Add(DatFormat.SSV, CreateOutFileNamesHelper(outDir, ".ssv", overwrite));
                usedExtensions.Add(".ssv");
            };

            #endregion

            #region .tsv

            // TSV
            if (DatFormat.HasFlag(DatFormat.TSV))
            {
                outfileNames.Add(DatFormat.TSV, CreateOutFileNamesHelper(outDir, ".tsv", overwrite));
                usedExtensions.Add(".tsv");
            };

            #endregion

            #region .txt

            // AttractMode
            if (DatFormat.HasFlag(DatFormat.AttractMode))
            {
                outfileNames.Add(DatFormat.AttractMode, CreateOutFileNamesHelper(outDir, ".txt", overwrite));
                usedExtensions.Add(".txt");
            }

            // MAME Listroms
            if (DatFormat.HasFlag(DatFormat.Listrom))
            {
                if (usedExtensions.Contains(".txt"))
                {
                    outfileNames.Add(DatFormat.Listrom, CreateOutFileNamesHelper(outDir, ".lr.txt", overwrite));
                    usedExtensions.Add(".lr.txt");
                }
                else
                {
                    outfileNames.Add(DatFormat.Listrom, CreateOutFileNamesHelper(outDir, ".txt", overwrite));
                    usedExtensions.Add(".txt");
                }
            }

            // Missfile
            if (DatFormat.HasFlag(DatFormat.MissFile))
            {
                if (usedExtensions.Contains(".txt"))
                {
                    outfileNames.Add(DatFormat.MissFile, CreateOutFileNamesHelper(outDir, ".miss.txt", overwrite));
                    usedExtensions.Add(".miss.txt");
                }
                else
                {
                    outfileNames.Add(DatFormat.MissFile, CreateOutFileNamesHelper(outDir, ".txt", overwrite));
                    usedExtensions.Add(".txt");
                }
            }

            // Everdrive SMDB
            if (DatFormat.HasFlag(DatFormat.EverdriveSMDB))
            {
                if (usedExtensions.Contains(".txt"))
                {
                    outfileNames.Add(DatFormat.EverdriveSMDB, CreateOutFileNamesHelper(outDir, ".smdb.txt", overwrite));
                    usedExtensions.Add(".smdb.txt");
                }
                else
                {
                    outfileNames.Add(DatFormat.EverdriveSMDB, CreateOutFileNamesHelper(outDir, ".txt", overwrite));
                    usedExtensions.Add(".txt");
                }
            }

            #endregion

            #region .xml

            // Logiqx XML
            if (DatFormat.HasFlag(DatFormat.Logiqx))
            {
                outfileNames.Add(DatFormat.Logiqx, CreateOutFileNamesHelper(outDir, ".xml", overwrite));
            }
            if (DatFormat.HasFlag(DatFormat.LogiqxDeprecated))
            {
                outfileNames.Add(DatFormat.LogiqxDeprecated, CreateOutFileNamesHelper(outDir, ".xml", overwrite));
            }

            // SabreDAT
            if (DatFormat.HasFlag(DatFormat.SabreDat))
            {
                if (usedExtensions.Contains(".xml"))
                {
                    outfileNames.Add(DatFormat.SabreDat, CreateOutFileNamesHelper(outDir, ".sd.xml", overwrite));
                    usedExtensions.Add(".sd.xml");
                }
                else
                {
                    outfileNames.Add(DatFormat.SabreDat, CreateOutFileNamesHelper(outDir, ".xml", overwrite));
                    usedExtensions.Add(".xml");
                }
            }

            // Software List
            if (DatFormat.HasFlag(DatFormat.SoftwareList))
            {
                if (usedExtensions.Contains(".xml"))
                {
                    outfileNames.Add(DatFormat.SoftwareList, CreateOutFileNamesHelper(outDir, ".sl.xml", overwrite));
                    usedExtensions.Add(".sl.xml");
                }
                else
                {
                    outfileNames.Add(DatFormat.SoftwareList, CreateOutFileNamesHelper(outDir, ".xml", overwrite));
                    usedExtensions.Add(".xml");
                }
            }

            // MAME Listxml
            if (DatFormat.HasFlag(DatFormat.Listxml))
            {
                if (usedExtensions.Contains(".xml"))
                {
                    outfileNames.Add(DatFormat.Listxml, CreateOutFileNamesHelper(outDir, ".mame.xml", overwrite));
                    usedExtensions.Add(".mame.xml");
                }
                else
                {
                    outfileNames.Add(DatFormat.Listxml, CreateOutFileNamesHelper(outDir, ".xml", overwrite));
                    usedExtensions.Add(".xml");
                }
            }

            // OfflineList
            if (DatFormat.HasFlag(DatFormat.OfflineList))
            {
                if (usedExtensions.Contains(".xml"))
                {
                    outfileNames.Add(DatFormat.OfflineList, CreateOutFileNamesHelper(outDir, ".ol.xml", overwrite));
                    usedExtensions.Add(".ol.xml");
                }
                else
                {
                    outfileNames.Add(DatFormat.OfflineList, CreateOutFileNamesHelper(outDir, ".xml", overwrite));
                    usedExtensions.Add(".xml");
                }
            }

            // openMSX
            if (DatFormat.HasFlag(DatFormat.OpenMSX))
            {
                if (usedExtensions.Contains(".xml"))
                {
                    outfileNames.Add(DatFormat.OpenMSX, CreateOutFileNamesHelper(outDir, ".msx.xml", overwrite));
                    usedExtensions.Add(".msx.xml");
                }
                else
                {
                    outfileNames.Add(DatFormat.OpenMSX, CreateOutFileNamesHelper(outDir, ".xml", overwrite));
                    usedExtensions.Add(".xml");
                }
            }

            #endregion

            return outfileNames;
        }

        /// <summary>
        /// Help generating the outfile name
        /// </summary>
        /// <param name="outDir">Output directory</param>
        /// <param name="extension">Extension to use for the file</param>
        /// <param name="overwrite">True if we ignore existing files, false otherwise</param>
        /// <returns>String containing the new filename</returns>
        private string CreateOutFileNamesHelper(string outDir, string extension, bool overwrite)
        {
            string filename = (string.IsNullOrWhiteSpace(FileName) ? Description : FileName);
            filename = Path.GetFileNameWithoutExtension(filename);
            string outfile = $"{outDir}{filename}{extension}";
            outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());

            if (!overwrite)
            {
                int i = 1;
                while (File.Exists(outfile))
                {
                    outfile = $"{outDir}{filename}_{i}{extension}";
                    outfile = outfile.Replace($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}", Path.DirectorySeparatorChar.ToString());
                    i++;
                }
            }

            return outfile;
        }

        #endregion

        #endregion // Instance Methods
    }
}
