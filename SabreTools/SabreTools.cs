using System;
using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.DatFiles;
using SabreTools.Library.Help;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using Alphaleonis.Win32.Filesystem;
#endif

namespace SabreTools
{
    /// <summary>
    /// Entry class for the DATabase application
    /// </summary>
    /// TODO: Look into async read/write to make things quicker. Ask edc for help?
    public partial class SabreTools
    {
        // Private required variables
        private static Help _help;

        /// <summary>
        /// Entry class for the SabreTools application
        /// </summary>
        /// <param name="args">String array representing command line parameters</param>
        public static void Main(string[] args)
        {
            // Perform initial setup and verification
            Globals.Logger = new Logger(true, "sabretools.log");

            // Create a new Help object for this program
            _help = SabreTools.RetrieveHelp();

            // Get the location of the script tag, if it exists
            int scriptLocation = (new List<string>(args)).IndexOf("--script");

            // If output is being redirected or we are in script mode, don't allow clear screens
            if (!Console.IsOutputRedirected && scriptLocation == -1)
            {
                Console.Clear();
                Build.PrepareConsole("SabreTools");
            }

            // Now we remove the script tag because it messes things up
            if (scriptLocation > -1)
            {
                List<string> newargs = new List<string>(args);
                newargs.RemoveAt(scriptLocation);
                args = newargs.ToArray();
            }

            // Credits take precidence over all
            if ((new List<string>(args)).Contains("--credits"))
            {
                _help.OutputCredits();
                Globals.Logger.Close();
                return;
            }

            // If there's no arguments, show help
            if (args.Length == 0)
            {
                _help.OutputGenericHelp();
                Globals.Logger.Close();
                return;
            }

            // User flags
            bool addBlankFiles = false,
                addFileDates = false,
                archivesAsFiles = false,
                basedat = false,
                chdsAsFiles = false,
                cleanGameNames = false,
                copyFiles = false,
                delete = false,
                depot = false,
                descAsName = false,
                hashOnly = false,
                individual = false,
                inplace = false,
                inverse = false,
                noAutomaticDate = false,
                nostore = false,
                onlySame = false,
                quickScan = false,
                removeUnicode = false,
                showBaddumpColumn = false,
                showNodumpColumn = false,
                shortname = false,
                skipFirstOutput = false,
                updateDat = false;
            Hash omitFromScan = Hash.DeepHashes; // TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually
            OutputFormat outputFormat = OutputFormat.Folder;
            SkipFileType skipFileType = SkipFileType.None;
            SplittingMode splittingMode = SplittingMode.None;
            SplitType splitType = SplitType.None;
            StatReportFormat statDatFormat = StatReportFormat.None;
            UpdateMode updateMode = UpdateMode.None;

            // User inputs
            int gz = 1,
                rar = 1,
                sevenzip = 1,
                zip = 1;
            long radix = 0;
            string outDir = null,
                tempDir = string.Empty;
            DatHeader datHeader = new DatHeader();
            Filter filter = new Filter();
            List<string> basePaths = new List<string>();
            List<string> datfiles = new List<string>();
            List<string> exta = new List<string>();
            List<string> extb = new List<string>();
            List<string> inputs = new List<string>();
            List<Field> updateFields = new List<Field>();

            // Get the first argument as a feature flag
            string feature = args[0];

            // Verify that the flag is valid
            if (!_help.TopLevelFlag(feature))
            {
                Globals.Logger.User("'{0}' is not valid feature flag", feature);
                _help.OutputIndividualFeature(feature);
                Globals.Logger.Close();
                return;
            }

            // Now get the proper name for the feature
            feature = _help.GetFeatureName(feature);

            // If we had the help feature first
            if (feature == "Help")
            {
                // If we had something else after help
                if (args.Length > 1)
                {
                    _help.OutputIndividualFeature(args[1]);
                    Globals.Logger.Close();
                    return;
                }
                // Otherwise, show generic help
                else
                {
                    _help.OutputGenericHelp();
                    Globals.Logger.Close();
                    return;
                }
            }
            else if (feature == "Help (Detailed)")
            {
                // If we had something else after help
                if (args.Length > 1)
                {
                    _help.OutputIndividualFeature(args[1], includeLongDescription: true);
                    Globals.Logger.Close();
                    return;
                }
                // Otherwise, show generic help
                else
                {
                    _help.OutputAllHelp();
                    Globals.Logger.Close();
                    return;
                }
            }

            // Now verify that all other flags are valid
            for (int i = 1; i < args.Length; i++)
            {
                // Verify that the current flag is proper for the feature
                if (!_help[feature].ValidateInput(args[i]))
                {
                    Globals.Logger.Error("Invalid input detected: {0}", args[i]);
                    _help.OutputIndividualFeature(feature);
                    Globals.Logger.Close();
                    return;
                }

                // Special precautions for files and directories
                if (File.Exists(args[i]) || Directory.Exists(args[i]))
                {
                    inputs.Add(args[i]);
                }
            }

            // Now loop through all inputs
            Dictionary<string, Feature> features = _help.GetEnabledFeatures();
            foreach (KeyValuePair<string, Feature> feat in features)
            {
                // Check all of the flag names and translate to arguments
                switch (feat.Key)
                {
                    #region User Flags

                    case "add-blank-files":
                        addBlankFiles = true;
                        break;
                    case "add-date":
                        addFileDates = true;
                        break;
                    case "archives-as-files":
                        archivesAsFiles = true;
                        break;
                    case "baddump-column":
                        showBaddumpColumn = true;
                        break;
                    case "base":
                        basedat = true;
                        break;
                    case "base-replace":
                        updateMode |= UpdateMode.BaseReplace;
                        break;
                    case "chds-as-Files":
                        chdsAsFiles = true;
                        break;
                    case "copy-files":
                        copyFiles = true;
                        break;
                    case "clean":
                        cleanGameNames = true;
                        break;
                    case "dat-device-non-merged":
                        splitType = SplitType.DeviceNonMerged;
                        break;
                    case "dat-full-non-merged":
                        splitType = SplitType.FullNonMerged;
                        break;
                    case "dat-merged":
                        splitType = SplitType.Merged;
                        break;
                    case "dat-non-merged":
                        splitType = SplitType.NonMerged;
                        break;
                    case "dat-split":
                        splitType = SplitType.Split;
                        break;
                    case "dedup":
                        datHeader.DedupeRoms = DedupeType.Full;
                        break;
                    case "delete":
                        delete = true;
                        break;
                    case "depot":
                        depot = true;
                        break;
                    case "depreciated":
                        // Remove the Logiqx standard output if this is included
                        if ((datHeader.DatFormat & DatFormat.Logiqx) != 0)
                        {
                            datHeader.DatFormat &= ~DatFormat.Logiqx;
                        }
                        datHeader.DatFormat |= DatFormat.LogiqxDepreciated;
                        break;
                    case "description-as-name":
                        descAsName = true;
                        break;
                    case "diff-against":
                        updateMode |= UpdateMode.DiffAgainst;
                        break;
                    case "diff-all":
                        updateMode |= UpdateMode.AllDiffs;
                        break;
                    case "diff-cascade":
                        updateMode |= UpdateMode.DiffCascade;
                        break;
                    case "diff-duplicates":
                        updateMode |= UpdateMode.DiffDupesOnly;
                        break;
                    case "diff-individuals":
                        updateMode |= UpdateMode.DiffIndividualsOnly;
                        break;
                    case "diff-no-duplicates":
                        updateMode |= UpdateMode.DiffNoDupesOnly;
                        break;
                    case "diff-reverse-cascade":
                        updateMode |= UpdateMode.DiffReverseCascade;
                        break;
                    case "extension":
                        splittingMode |= SplittingMode.Extension;
                        break;
                    case "game-dedup":
                        datHeader.DedupeRoms = DedupeType.Game;
                        break;
                    case "game-prefix":
                        datHeader.GameName = true;
                        break;
                    case "hash":
                        splittingMode |= SplittingMode.Hash;
                        break; 
                    case "hash-only":
                        hashOnly = true;
                        break;
                    case "individual":
                        individual = true;
                        break;
                    case "inplace":
                        inplace = true;
                        break;
                    case "inverse":
                        inverse = true;
                        break;
                    case "keep-empty-games":
                        datHeader.KeepEmptyGames = true;
                        break;
                    case "level":
                        splittingMode |= SplittingMode.Level;
                        break;
                    case "match-of-tags":
                        filter.IncludeOfInGame.Neutral = true;
                        break;
                    case "merge":
                        updateMode |= UpdateMode.Merge;
                        break;
                    case "no-automatic-date":
                        noAutomaticDate = true;
                        break;
                    case "nodump-column":
                        showNodumpColumn = true;
                        break;
                    case "not-runnable":
                        filter.Runnable.Neutral = false;
                        break;
                    case "no-store-header":
                        nostore = true;
                        break;
                    case "one-rom-per-game":
                        datHeader.OneRom = true;
                        break;
                    case "only-same":
                        onlySame = true;
                        break;
                    case "quick":
                        quickScan = true;
                        break;
                    case "quotes":
                        datHeader.Quotes = true;
                        break;
                    case "remove-extensions":
                        datHeader.RemoveExtension = true;
                        break;
                    case "remove-unicode":
                        removeUnicode = true;
                        break;
                    case "reverse-base-name":
                        updateMode |= UpdateMode.ReverseBaseReplace;
                        break;
                    case "romba":
                        datHeader.Romba = true;
                        break;
                    case "roms":
                        datHeader.UseRomName = true;
                        break;
                    case "runnable":
                        filter.Runnable.Neutral = true;
                        break;
                    case "scan-all":
                        sevenzip = 0;
                        gz = 0;
                        rar = 0;
                        zip = 0;
                        break;
                    case "scene-date-strip":
                        datHeader.SceneDateStrip = true;
                        break;
                    case "short":
                        shortname = true;
                        break;
                    case "size":
                        splittingMode |= SplittingMode.Size;
                        break;
                    case "skip-archives":
                        skipFileType = SkipFileType.Archive;
                        break;
                    case "skip-files":
                        skipFileType = SkipFileType.File;
                        break;
                    case "skip-first-output":
                        skipFirstOutput = true;
                        break;
                    case "skip-md5":
                        omitFromScan |= Hash.MD5;
                        break;
                    case "skip-ripemd160":
                        omitFromScan &= ~Hash.RIPEMD160; // This needs to be inverted later
                        break;
                    case "skip-sha1":
                        omitFromScan |= Hash.SHA1;
                        break;
                    case "skip-sha256":
                        omitFromScan &= ~Hash.SHA256; // This needs to be inverted later
                        break;
                    case "skip-sha384":
                        omitFromScan &= ~Hash.SHA384; // This needs to be inverted later
                        break;
                    case "skip-sha512":
                        omitFromScan &= ~Hash.SHA512; // This needs to be inverted later
                        break;
                    case "single-set":
                        filter.Single.Neutral = true;
                        break;
                    case "superdat":
                        datHeader.Type = "SuperDAT";
                        break;
                    case "tar":
                        outputFormat = OutputFormat.TapeArchive;
                        break;
                    case "torrent-7zip":
                        outputFormat = OutputFormat.Torrent7Zip;
                        break;
                    case "torrent-gzip":
                        outputFormat = OutputFormat.TorrentGzip;
                        break;
                    case "torrent-lrzip":
                        outputFormat = OutputFormat.TorrentLRZip;
                        break;
                    case "torrent-lz4":
                        outputFormat = OutputFormat.TorrentLZ4;
                        break;
                    case "torrent-rar":
                        outputFormat = OutputFormat.TorrentRar;
                        break;
                    case "torrent-xz":
                        outputFormat = OutputFormat.TorrentXZ;
                        break;
                    case "torrent-zip":
                        outputFormat = OutputFormat.TorrentZip;
                        break;
                    case "torrent-zpaq":
                        outputFormat = OutputFormat.TorrentZPAQ;
                        break;
                    case "torrent-zstd":
                        outputFormat = OutputFormat.TorrentZstd;
                        break;
                    case "trim":
                        filter.Trim.Neutral = true;
                        break;
                    case "type":
                        splittingMode |= SplittingMode.Type;
                        break;
                    case "update-dat":
                        updateDat = true;
                        break;
                    case "update-description":
                        Globals.Logger.User("This flag '{0}' is depreciated, please use {1} instead", feat.Key, String.Join(", ", _updateFieldListInput.Flags));
                        updateFields.Add(Field.Description);
                        break;
                    case "update-game-type":
                        Globals.Logger.User("This flag '{0}' is depreciated, please use {1} instead", feat.Key, String.Join(", ", _updateFieldListInput.Flags));
                        updateFields.Add(Field.MachineType);
                        break;
                    case "update-hashes":
                        Globals.Logger.User("This flag '{0}' is depreciated, please use {1} instead", feat.Key, String.Join(", ", _updateFieldListInput.Flags));
                        updateFields.Add(Field.CRC);
                        updateFields.Add(Field.MD5);
                        updateFields.Add(Field.RIPEMD160);
                        updateFields.Add(Field.SHA1);
                        updateFields.Add(Field.SHA256);
                        updateFields.Add(Field.SHA384);
                        updateFields.Add(Field.SHA512);
                        break;
                    case "update-manufacturer":
                        Globals.Logger.User("This flag '{0}' is depreciated, please use {1} instead", feat.Key, String.Join(", ", _updateFieldListInput.Flags));
                        updateFields.Add(Field.Manufacturer);
                        break;
                    case "update-names":
                        Globals.Logger.User("This flag '{0}' is depreciated, please use {1} instead", feat.Key, String.Join(", ", _updateFieldListInput.Flags));
                        updateFields.Add(Field.Name);
                        break;
                    case "update-parents":
                        Globals.Logger.User("This flag '{0}' is depreciated, please use {1} instead", feat.Key, String.Join(", ", _updateFieldListInput.Flags));
                        updateFields.Add(Field.CloneOf);
                        updateFields.Add(Field.RomOf);
                        updateFields.Add(Field.SampleOf);
                        break;
                    case "update-year":
                        Globals.Logger.User("This flag '{0}' is depreciated, please use {1} instead", feat.Key, String.Join(", ", _updateFieldListInput.Flags));
                        updateFields.Add(Field.Year);
                        break;

                    #endregion

                    #region User Int32 Inputs

                    case "7z":
                        sevenzip = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
                        break;
                    case "gz":
                        gz = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
                        break;
                    case "rar":
                        rar = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
                        break;
                    case "threads":
                        int val = (int)feat.Value.GetValue();
                        if (val != Int32.MinValue)
                            Globals.MaxThreads = val;

                        break;
                    case "zip":
                        zip = (int)feat.Value.GetValue() == Int32.MinValue ? (int)feat.Value.GetValue() : 1;
                        break;

                    #endregion

                    #region User Int64 Inputs

                    case "radix":
                        radix = (long)feat.Value.GetValue() == Int64.MinValue ? (long)feat.Value.GetValue() : 0;
                        break;

                    #endregion

                    #region User List<string> Inputs

                    case "base-dat":
                        basePaths.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "crc":
                        filter.CRC.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "dat":
                        datfiles.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "exclude-field":
                        foreach (string field in (List<string>)feat.Value.GetValue())
                        {
                            datHeader.ExcludeFields[(int)Utilities.GetField(field)] = true;
                        }
                        break;
                    case "exta":
                        exta.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "extb":
                        extb.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "game-description":
                        filter.MachineDescription.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "game-name":
                        filter.MachineName.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "game-type":
                        foreach (string mach in (List<string>)feat.Value.GetValue())
                        {
                            filter.MachineTypes.Positive |= Utilities.GetMachineType(mach);
                        }
                        break;
                    case "item-name":
                        filter.ItemName.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "item-type":
                        filter.ItemTypes.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "md5":
                        filter.MD5.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-crc":
                        filter.CRC.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-game-description":
                        filter.MachineDescription.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-game-name":
                        filter.MachineName.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-game-type":
                        foreach (string nmach in (List<string>)feat.Value.GetValue())
                        {
                            filter.MachineTypes.Negative |= Utilities.GetMachineType(nmach);
                        }
                        break;
                    case "not-item-name":
                        filter.ItemName.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-item-type":
                        filter.ItemTypes.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-md5":
                        filter.MD5.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-ripemd160":
                        filter.RIPEMD160.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-sha1":
                        filter.SHA1.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-sha256":
                        filter.SHA256.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-sha384":
                        filter.SHA384.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-sha512":
                        filter.SHA512.NegativeSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "not-status":
                        foreach (string nstat in (List<string>)feat.Value.GetValue())
                        {
                            filter.ItemStatuses.Negative |= Utilities.GetItemStatus(nstat);
                        }
                        break;
                    case "output-type":
                        foreach (string ot in (List<string>)feat.Value.GetValue())
                        {
                            DatFormat dftemp = Utilities.GetDatFormat(ot);
                            if (dftemp != DatFormat.Logiqx
                                || (dftemp == DatFormat.Logiqx && (datHeader.DatFormat & DatFormat.LogiqxDepreciated) == 0))
                            {
                                datHeader.DatFormat |= dftemp;
                            }
                        }
                        break;
                    case "report-type":
                        foreach (string rt in (List<string>)feat.Value.GetValue())
                        {
                            statDatFormat |= Utilities.GetStatFormat(rt);
                        }
                        break;
                    case "ripemd160":
                        filter.RIPEMD160.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "sha1":
                        filter.SHA1.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "sha256":
                        filter.SHA256.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "sha384":
                        filter.SHA384.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "sha512":
                        filter.SHA512.PositiveSet.AddRange((List<string>)feat.Value.GetValue());
                        break;
                    case "status":
                        foreach (string stat in (List<string>)feat.Value.GetValue())
                        {
                            filter.ItemStatuses.Positive |= Utilities.GetItemStatus(stat);
                        }
                        break;
                    case "update-field":
                        foreach (string field in (List<string>)feat.Value.GetValue())
                        {
                            updateFields.Add(Utilities.GetField(field));
                        }
                        break;

                    #endregion

                    #region User String Inputs

                    case "add-extension":
                        datHeader.AddExtension = (string)feat.Value.GetValue();
                        break;
                    case "author":
                        datHeader.Author = (string)feat.Value.GetValue();
                        break;
                    case "category":
                        datHeader.Category = (string)feat.Value.GetValue();
                        break;
                    case "comment":
                        datHeader.Comment = (string)feat.Value.GetValue();
                        break;
                    case "date":
                        datHeader.Date = (string)feat.Value.GetValue();
                        break;
                    case "description":
                        datHeader.Description = (string)feat.Value.GetValue();
                        break;
                    case "email":
                        datHeader.Email = (string)feat.Value.GetValue();
                        break;
                    case "equal":
                        filter.Size.Neutral = Utilities.GetSizeFromString((string)feat.Value.GetValue());
                        break;
                    case "filename":
                        datHeader.FileName = (string)feat.Value.GetValue();
                        break;
                    case "forcemerging":
                        datHeader.ForceMerging = Utilities.GetForceMerging((string)feat.Value.GetValue());
                        break;
                    case "forcenodump":
                        datHeader.ForceNodump = Utilities.GetForceNodump((string)feat.Value.GetValue());
                        break;
                    case "forcepacking":
                        datHeader.ForcePacking = Utilities.GetForcePacking((string)feat.Value.GetValue());
                        break;
                    case "greater":
                        filter.Size.Positive = Utilities.GetSizeFromString((string)feat.Value.GetValue());
                        break;
                    case "header":
                        datHeader.Header = (string)feat.Value.GetValue();
                        break;
                    case "homepage":
                        datHeader.Homepage = (string)feat.Value.GetValue();
                        break;
                    case "less":
                        filter.Size.Negative = Utilities.GetSizeFromString((string)feat.Value.GetValue());
                        break;
                    case "name":
                        datHeader.Name = (string)feat.Value.GetValue();
                        break;
                    case "output-dir":
                        outDir = (string)feat.Value.GetValue();
                        break;
                    case "postfix":
                        datHeader.Postfix = (string)feat.Value.GetValue();
                        break;
                    case "prefix":
                        datHeader.Prefix = (string)feat.Value.GetValue();
                        break;
                    case "replace-extension":
                        datHeader.ReplaceExtension = (string)feat.Value.GetValue();
                        break;
                    case "root":
                        datHeader.RootDir = (string)feat.Value.GetValue();
                        break;
                    case "root-dir":
                        filter.Root.Neutral = (string)feat.Value.GetValue();
                        break;
                    case "temp":
                        tempDir = (string)feat.Value.GetValue();
                        break;
                    case "url":
                        datHeader.Url = (string)feat.Value.GetValue();
                        break;
                    case "version":
                        datHeader.Version = (string)feat.Value.GetValue();
                        break;

                        #endregion
                }
            }

            // Now take care of each mode in succesion
            switch (feature)
            {
                case "Help":
                    // No-op as this should be caught
                    break;
                // Create a DAT from a directory or set of directories
                case "DATFromDir":
                    VerifyInputs(inputs, feature);
                    InitDatFromDir(inputs, datHeader, omitFromScan, noAutomaticDate, archivesAsFiles, chdsAsFiles,
                        skipFileType, addBlankFiles, addFileDates, tempDir, outDir, copyFiles, filter);
                    break;
                // If we're in header extract and remove mode
                case "Extract":
                    VerifyInputs(inputs, feature);
                    InitExtractRemoveHeader(inputs, outDir, nostore);
                    break;
                // If we're in header restore mode
                case "Restore":
                    VerifyInputs(inputs, feature);
                    InitReplaceHeader(inputs, outDir);
                    break;
                case "Script":
                    // No-op as this should be caught
                    break;
                // If we're using the sorter
                case "Sort":
                    InitSort(datfiles, inputs, outDir, depot, quickScan, addFileDates, delete, inverse,
                        outputFormat, datHeader.Romba, sevenzip, gz, rar, zip, updateDat, datHeader.Header,
                        splitType, chdsAsFiles, individual);
                    break;
                // Split a DAT by the split type
                case "Split":
                    VerifyInputs(inputs, feature);
                    InitSplit(inputs, outDir, inplace, datHeader.DatFormat, splittingMode, exta, extb, shortname, basedat, radix);
                    break;
                // Get statistics on input files
                case "Stats":
                    VerifyInputs(inputs, feature);
                    InitStats(inputs, datHeader.FileName, outDir, individual, showBaddumpColumn, showNodumpColumn, statDatFormat);
                    break;
                // Convert, update, merge, diff, and filter a DAT or folder of DATs
                case "Update":
                    VerifyInputs(inputs, feature);
                    InitUpdate(inputs, basePaths, datHeader, updateMode, inplace, skipFirstOutput, noAutomaticDate, filter,
                        splitType, outDir, cleanGameNames, removeUnicode, descAsName, updateFields, onlySame);
                    break;
                // If we're using the verifier
                case "Verify":
                    VerifyInputs(inputs, feature);
                    InitVerify(datfiles, inputs, depot, hashOnly, quickScan, datHeader.Header, splitType, chdsAsFiles, individual, filter);
                    break;
                // If nothing is set, show the help
                default:
                    _help.OutputGenericHelp();
                    break;
            }

            Globals.Logger.Close();
            return;
        }

        private static void VerifyInputs(List<string> inputs, string feature)
        {
            if (inputs.Count == 0)
            {
                Globals.Logger.Error("This feature requires at least one input");
                _help.OutputIndividualFeature(feature);
                Environment.Exit(0);
            }
        }
    }
}
