using System.Collections.Generic;

using SabreTools.Library.Data;
using SabreTools.Library.Help;

namespace RombaSharp
{
    public partial class RombaSharp
    {
        #region Private Flag features

        private static Feature copyFlag
        {
            get
            {
                return new Feature(
                    "copy",
                    "-copy",
                    "Copy files to output instead of rebuilding",
                    FeatureType.Flag);
            }
        } // Unique to RombaSharp
        private static Feature fixdatOnlyFlag
        {
            get
            {
                return new Feature(
                    "fixdatOnly",
                    "-fixdatOnly",
                    "only fix dats and don't generate torrentzips",
                    FeatureType.Flag);
            }
        }
        private static Feature logOnlyFlag
        {
            get
            {
                return new Feature(
                "log-only",
                "-log-only",
                "Only write out actions to log",
                FeatureType.Flag);
            }
        }
        private static Feature noDbFlag
        {
            get
            {
                return new Feature(
                    "no-db",
                    "-no-db",
                    "archive into depot but do not touch DB index and ignore only-needed flag",
                    FeatureType.Flag);
            }
        }
        private static Feature onlyNeededFlag
        {
            get
            {
                return new Feature(
                    "only-needed",
                    "-only-needed",
                    "only archive ROM files actually referenced by DAT files from the DAT index",
                    FeatureType.Flag);
            }
        }
        private static Feature skipInitialScanFlag
        {
            get
            {
                return new Feature(
                    "skip-initial-scan",
                    "-skip-initial-scan",
                    "skip the initial scan of the files to determine amount of work",
                    FeatureType.Flag);
            }
        }
        private static Feature useGolangZipFlag
        {
            get
            {
                return new Feature(
                    "use-golang-zip",
                    "-use-golang-zip",
                    "use go zip implementation instead of zlib",
                    FeatureType.Flag);
            }
        }

        #endregion

        #region Private Int32 features

        private static Feature include7ZipsInt32Input
        {
            get
            {
                return new Feature(
                    "include-7zips",
                    "-include-7zips",
                    "flag value == 0 means: add 7zip files themselves into the depot in addition to their contents, flag value == 2 means add 7zip files themselves but don't add content",
                    FeatureType.Int32);
            }
        }
        private static Feature includeGZipsInt32Input
        {
            get
            {
                return new Feature(
                    "include-gzips",
                    "-include-gzips",
                    "flag value == 0 means: add gzip files themselves into the depot in addition to their contents, flag value == 2 means add gzip files themselves but don't add content",
                    FeatureType.Int32);
            }
        }
        private static Feature includeZipsInt32Input
        {
            get
            {
                return new Feature(
                    "include-zips",
                    "-include-zips",
                    "flag value == 0 means: add zip files themselves into the depot in addition to their contents, flag value == 2 means add zip files themselves but don't add content",
                    FeatureType.Int32);
            }
        }
        private static Feature subworkersInt32Input
        {
            get
            {
                return new Feature(
                    "subworkers",
                    "-subworkers",
                    "how many subworkers to launch for each worker",
                    FeatureType.Int32);
            }
        } // Defaults to Workers count in config
        private static Feature workersInt32Input
        {
            get
            {
                return new Feature(
                    "workers",
                    "-workers",
                    "how many workers to launch for the job",
                    FeatureType.Int32);
            }
        } // Defaults to Workers count in config

        #endregion

        #region Private Int64 features

        private static Feature sizeInt64Input
        {
            get
            {
                return new Feature(
                    "size",
                    "-size",
                    "size of the rom to lookup",
                    FeatureType.Int64);
            }
        }

        #endregion

        #region Private List<String> features

        private static Feature datsListStringInput
        {
            get
            {
                return new Feature(
                    "dats",
                    "-dats",
                    "purge only roms declared in these dats",
                    FeatureType.List);
            }
        }
        private static Feature depotListStringInput
        {
            get
            {
                return new Feature(
                    "depot",
                    "-depot",
                    "work only on specified depot path",
                    FeatureType.List);
            }
        }

        #endregion

        #region Private String features

        private static Feature backupStringInput
        {
            get
            {
                return new Feature(
                    "backup",
                    "-backup",
                    "backup directory where backup files are moved to",
                    FeatureType.String);
            }
        }
        private static Feature descriptionStringInput
        {
            get
            {
                return new Feature(
                    "description",
                    "-description",
                    "description value in DAT header",
                    FeatureType.String);
            }
        }
        private static Feature missingSha1sStringInput
        {
            get
            {
                return new Feature(
                    "missingSha1s",
                    "-missingSha1s",
                    "write paths of dats with missing sha1s into this file",
                    FeatureType.String);
            }
        }
        private static Feature nameStringInput
        {
            get
            {
                return new Feature(
                    "name",
                    "-name",
                    "name value in DAT header",
                    FeatureType.String);
            }
        }
        private static Feature newStringInput
        {
            get
            {
                return new Feature(
                    "new",
                    "-new",
                    "new DAT file",
                    FeatureType.String);
            }
        }
        private static Feature oldStringInput
        {
            get
            {
                return new Feature(
                    "old",
                    "-old",
                    "old DAT file",
                    FeatureType.String);
            }
        }
        private static Feature outStringInput
        {
            get
            {
                return new Feature(
                    "out",
                    "-out",
                    "output file",
                    FeatureType.String);
            }
        }
        private static Feature resumeStringInput
        {
            get
            {
                return new Feature(
                    "resume",
                    "-resume",
                    "resume a previously interrupted operation from the specified path",
                    FeatureType.String);
            }
        }
        private static Feature sourceStringInput
        {
            get
            {
                return new Feature(
                    "source",
                    "-source",
                    "source directory",
                    FeatureType.String);
            }
        }

        #endregion

        public static Help RetrieveHelp()
        {
            // Create and add the header to the Help object
            string barrier = "-----------------------------------------";
            List<string> helpHeader = new List<string>()
            {
                "RombaSharp - C# port of the Romba rom management tool",
                barrier,
                "Usage: RombaSharp [option] [filename|dirname] ...",
                string.Empty
            };
            Help help = new Help(helpHeader);

            #region Help

            Feature helpFeature = new Feature(
                "Help",
                new List<string>() { "-?", "-h", "--help" },
                "Show this help",
                FeatureType.Flag);

            #endregion

            #region Script

            Feature script = new Feature(
                "Script",
                "--script",
                "Enable script mode (no clear screen)",
                FeatureType.Flag,
                longDescription: "For times when RombaSharp is being used in a scripted environment, the user may not want the screen to be cleared every time that it is called. This flag allows the user to skip clearing the screen on run just like if the console was being redirected.");

            #endregion

            #region Archive

            Feature archive = new Feature(
                "Archive",
                "archive",
                "Adds ROM files from the specified directories to the ROM archive.",
                FeatureType.Flag,
                longDescription: @"Adds ROM files from the specified directories to the ROM archive.
Traverses the specified directory trees looking for zip files and normal files.
Unpacked files will be stored as individual entries. Prior to unpacking a zip
file, the external SHA1 is checked against the DAT index. 
If -only-needed is set, only those files are put in the ROM archive that
have a current entry in the DAT index.");
            archive.AddFeature(onlyNeededFlag);
            archive.AddFeature(resumeStringInput);
            archive.AddFeature(includeZipsInt32Input); // Defaults to 0
            archive.AddFeature(workersInt32Input);
            archive.AddFeature(includeGZipsInt32Input); // Defaults to 0
            archive.AddFeature(include7ZipsInt32Input); // Defaults to 0
            archive.AddFeature(skipInitialScanFlag);
            archive.AddFeature(useGolangZipFlag);
            archive.AddFeature(noDbFlag);

            #endregion

            #region Build

            Feature build = new Feature(
                "Build",
                "build",
                "For each specified DAT file it creates the torrentzip files.",
                FeatureType.Flag,
                longDescription: @"For each specified DAT file it creates the torrentzip files in the specified
output dir. The files will be placed in the specified location using a folder
structure according to the original DAT master directory tree structure.");
            build.AddFeature(outStringInput);
            build.AddFeature(fixdatOnlyFlag);
            build.AddFeature(copyFlag);
            build.AddFeature(workersInt32Input);
            build.AddFeature(subworkersInt32Input);

            #endregion

            #region Cancel

            Feature cancel = new Feature(
                "Cancel",
                "cancel",
                "Cancels current long-running job",
                FeatureType.Flag,
                longDescription: @"Cancels current long-running job.");

            #endregion

            #region DatStats

            Feature datstats = new Feature(
                "DatStats",
                "datstats",
                "Prints dat stats.",
                FeatureType.Flag,
                longDescription: @"Print dat stats.");

            #endregion

            #region DbStats

            Feature dbstats = new Feature(
                "DbStats",
                "dbstats",
                "Prints db stats.",
                FeatureType.Flag,
                longDescription: @"Print db stats.");

            #endregion

            #region Diffdat

            Feature diffdat = new Feature(
                "Diffdat",
                "diffdat",
                "Creates a DAT file with those entries that are in -new DAT.",
                FeatureType.Flag,
                longDescription: @"Creates a DAT file with those entries that are in -new DAT file and not
in -old DAT file. Ignores those entries in -old that are not in -new.");
            diffdat.AddFeature(outStringInput);
            diffdat.AddFeature(oldStringInput);
            diffdat.AddFeature(newStringInput);
            diffdat.AddFeature(nameStringInput);
            diffdat.AddFeature(descriptionStringInput);

            #endregion

            #region Dir2Dat

            Feature dir2dat = new Feature(
                "Dir2Dat",
                "dir2dat",
                "Creates a DAT file for the specified input directory and saves it to the -out filename.",
                FeatureType.Flag);
            dir2dat.AddFeature(outStringInput);
            dir2dat.AddFeature(sourceStringInput);
            dir2dat.AddFeature(nameStringInput); // Defaults to "untitled"
            dir2dat.AddFeature(descriptionStringInput);

            #endregion

            #region EDiffdat

            Feature ediffdat = new Feature(
                "EDiffdat",
                "ediffdat",
                "Creates a DAT file with those entries that are in -new DAT.",
                FeatureType.Flag,
                longDescription: @"Creates a DAT file with those entries that are in -new DAT files and not
in -old DAT files. Ignores those entries in -old that are not in -new.");
            ediffdat.AddFeature(outStringInput);
            ediffdat.AddFeature(oldStringInput);
            ediffdat.AddFeature(newStringInput);

            #endregion

            #region Export

            // Unique to RombaSharp
            Feature export = new Feature(
                "Export",
                "export",
                "Exports db to export.csv",
                FeatureType.Flag,
                longDescription: "Exports db to standardized export.csv");

            #endregion

            #region Fixdat

            Feature fixdat = new Feature(
                "Fixdat",
                "fixdat",
                "For each specified DAT file it creates a fix DAT.",
                FeatureType.Flag,
                longDescription: @"For each specified DAT file it creates a fix DAT with the missing entries for
that DAT. If nothing is missing it doesn't create a fix DAT for that
particular DAT.");
            fixdat.AddFeature(outStringInput);
            fixdat.AddFeature(fixdatOnlyFlag); // Enabled by default
            fixdat.AddFeature(workersInt32Input);
            fixdat.AddFeature(subworkersInt32Input);

            #endregion

            #region Import

            // Unique to RombaSharp
            Feature import = new Feature(
                "Import",
                "import",
                "Import a database from a formatted CSV file",
                FeatureType.Flag,
                longDescription: @"Import a database from a formatted CSV file");

            #endregion

            #region Lookup

            Feature lookup = new Feature(
                "Lookup",
                "lookup",
                "For each specified hash it looks up any available information.",
                FeatureType.Flag,
                longDescription: @"For each specified hash it looks up any available information (dat or rom).");
            lookup.AddFeature(sizeInt64Input); // Defaults to -1
            lookup.AddFeature(outStringInput);

            #endregion

            #region Memstats

            Feature memstats = new Feature(
                "Memstats",
                "memstats",
                "Prints memory stats.",
                FeatureType.Flag,
                longDescription: @"Print memory stats.");

            #endregion

            #region Merge

            Feature merge = new Feature(
                "Merge",
                "merge",
                "Merges depot",
                FeatureType.Flag,
                longDescription: @"Merges specified depot into current depot.");
            merge.AddFeature(onlyNeededFlag);
            merge.AddFeature(resumeStringInput);
            merge.AddFeature(workersInt32Input);
            merge.AddFeature(skipInitialScanFlag);

            #endregion

            #region Miss

            // Unique to RombaSharp
            Feature miss = new Feature(
                "Miss",
                "miss",
                "Create miss and have file",
                FeatureType.Flag,
                longDescription: @"For each specified DAT file, create miss and have file");

            #endregion

            #region Progress

            Feature progress = new Feature(
                "Progress",
                "progress",
                "Shows progress of the currently running command.",
                FeatureType.Flag,
                longDescription: @"Shows progress of the currently running command.");

            #endregion

            #region Purge Backup

            Feature purgeBackup = new Feature(
                "Purge Backup",
                "purge-backup",
                "Moves DAT index entries for orphaned DATs.",
                FeatureType.Flag,
                longDescription: @"Deletes DAT index entries for orphaned DATs and moves ROM files that are no
longer associated with any current DATs to the specified backup folder.
The files will be placed in the backup location using
a folder structure according to the original DAT master directory tree
structure. It also deletes the specified DATs from the DAT index.");
            purgeBackup.AddFeature(backupStringInput);
            purgeBackup.AddFeature(workersInt32Input);
            purgeBackup.AddFeature(depotListStringInput);
            purgeBackup.AddFeature(datsListStringInput);
            purgeBackup.AddFeature(logOnlyFlag);

            #endregion

            #region Purge Delete

            // Unique to RombaSharp
            Feature purgeDelete = new Feature(
                "Purge Delete",
                "purge-delete",
                "Deletes DAT index entries for orphaned DATs",
                FeatureType.Flag);
            purgeDelete.AddFeature(workersInt32Input);
            purgeDelete.AddFeature(depotListStringInput);
            purgeDelete.AddFeature(datsListStringInput);
            purgeDelete.AddFeature(logOnlyFlag);

            #endregion

            #region Refresh DATs

            Feature refreshDats = new Feature(
                "Refresh DATs",
                "refresh-dats",
                "Refreshes the DAT index from the files in the DAT master directory tree.",
                FeatureType.Flag,
                longDescription: @"Refreshes the DAT index from the files in the DAT master directory tree.
Detects any changes in the DAT master directory tree and updates the DAT index
accordingly, marking deleted or overwritten dats as orphaned and updating
contents of any changed dats.");
            refreshDats.AddFeature(workersInt32Input);
            refreshDats.AddFeature(missingSha1sStringInput);

            #endregion

            #region Recan Depots

            // Unique to RombaSharp
            Feature rescanDepots = new Feature(
                "Rescan Depots",
                "depot-rescan",
                "Rescan a specific depot to get new information",
                FeatureType.Flag);

            #endregion

            #region Shutdown

            Feature shutdown = new Feature(
                "Shutdown",
                "shutdown",
                "Gracefully shuts down server.",
                FeatureType.Flag,
                longDescription: @"Gracefully shuts down server saving all the cached data.");

            #endregion

            #region Version

            Feature version = new Feature(
                "Version",
                "version",
                "Prints version",
                FeatureType.Flag,
                longDescription: @"Prints version.");

            #endregion

            // Now, add all of the main features to the Help object
            help.Add(helpFeature);
            help.Add(script);
            help.Add(archive);
            help.Add(build);
            help.Add(cancel);
            help.Add(datstats);
            help.Add(dbstats);
            help.Add(diffdat);
            help.Add(dir2dat);
            help.Add(ediffdat);
            help.Add(export);
            help.Add(fixdat);
            help.Add(import);
            help.Add(lookup);
            help.Add(memstats);
            help.Add(merge);
            help.Add(miss);
            help.Add(purgeBackup);
            help.Add(purgeDelete);
            help.Add(refreshDats);
            help.Add(rescanDepots);
            help.Add(progress);
            help.Add(shutdown);
            help.Add(version);

            return help;
        }
    }
}
