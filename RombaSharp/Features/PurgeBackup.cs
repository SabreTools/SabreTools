﻿using System.Collections.Generic;
using SabreTools.Help;

namespace RombaSharp.Features
{
    internal class PurgeBackup : BaseFeature
    {
        public const string Value = "Purge Backup";

        public PurgeBackup()
        {
            Name = Value;
            Flags.AddRange(["purge-backup"]);
            Description = "Moves DAT index entries for orphaned DATs.";
            _featureType = ParameterType.Flag;
            LongDescription = @"Deletes DAT index entries for orphaned DATs and moves ROM files that are no
longer associated with any current DATs to the specified backup folder.
The files will be placed in the backup location using
a folder structure according to the original DAT master directory tree
structure. It also deletes the specified DATs from the DAT index.";

            // Common Features
            AddCommonFeatures();

            AddFeature(BackupStringInput);
            AddFeature(WorkersInt32Input);
            AddFeature(DepotListStringInput);
            AddFeature(DatsListStringInput);
            AddFeature(LogOnlyFlag);
        }

        public override bool ProcessFeatures(Dictionary<string, Feature?> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
                return false;

            // Get feature flags
            bool logOnly = GetBoolean(features, LogOnlyValue);
            int workers = GetInt32(features, WorkersInt32Value);
            string? backup = GetString(features, BackupStringValue);
            List<string> dats = GetList(features, DatsListStringValue);
            List<string> depot = GetList(features, DepotListStringValue);

            logger.Error("This feature is not yet implemented: purge-backup");
            return true;
        }
    }
}
