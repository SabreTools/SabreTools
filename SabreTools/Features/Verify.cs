﻿using System.Collections.Generic;

using SabreTools.Core;
using SabreTools.DatFiles;
using SabreTools.DatTools;
using SabreTools.FileTypes;
using SabreTools.Help;
using SabreTools.IO;
using SabreTools.Logging;

namespace SabreTools.Features
{
    internal class Verify : BaseFeature
    {
        public const string Value = "Verify";

        public Verify()
        {
            Name = Value;
            Flags = new List<string>() { "ve", "verify" };
            Description = "Verify a folder against DATs";
            _featureType = ParameterType.Flag;
            LongDescription = "When used, this will use an input DAT or set of DATs to blindly check against an input folder. The base of the folder is considered the base for the combined DATs and games are either the directories or archives within. This will only do a direct verification of the items within and will create a fixdat afterwards for missing files.";
            Features = new Dictionary<string, Feature>();

            // Common Features
            AddCommonFeatures();

            AddFeature(DatListInput);
            AddFeature(DepotFlag);
            this[DepotFlag].AddFeature(DepotDepthInt32Input);
            AddFeature(OutputDirStringInput);
            AddFeature(HashOnlyFlag);
            AddFeature(QuickFlag);
            AddFeature(HeaderStringInput);
            AddFeature(AaruFormatsAsFilesFlag);
            AddFeature(ChdsAsFilesFlag);
            AddFeature(IndividualFlag);
            AddInternalSplitFeatures();
            AddFeature(ExtraIniListInput);
            AddFilteringFeatures();
        }

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            base.ProcessFeatures(features);

            // Get a list of files from the input datfiles
            var datfiles = GetList(features, DatListValue);
            var datfilePaths = PathTool.GetFilesOnly(datfiles);

            // Get feature flags
            TreatAsFile asFiles = GetTreatAsFiles(features);
            bool hashOnly = GetBoolean(features, HashOnlyValue);
            bool quickScan = GetBoolean(features, QuickValue);

            // If we are in individual mode, process each DAT on their own
            if (GetBoolean(features, IndividualValue))
            {
                foreach (ParentablePath datfile in datfilePaths)
                {
                    // Parse in from the file
                    DatFile datdata = DatFile.Create();
                    Parser.ParseInto(datdata, datfile, int.MaxValue, keep: true);

                    // Perform additional processing steps
                    Extras.ApplyExtras(datdata);
                    Splitter.ApplySplitting(datdata, true);
                    Filter.ApplyFilters(datdata);
                    Cleaner.ApplyCleaning(datdata);
                    Remover.ApplyRemovals(datdata);

                    // Set depot information
                    datdata.Header.InputDepot = Header.InputDepot?.Clone() as DepotInformation;

                    // If we have overridden the header skipper, set it now
                    if (!string.IsNullOrEmpty(Header.HeaderSkipper))
                        datdata.Header.HeaderSkipper = Header.HeaderSkipper;

                    // If we have the depot flag, respect it
                    if (Header.InputDepot?.IsActive ?? false)
                    {
                        Verification.VerifyDepot(datdata, Inputs);
                    }
                    else
                    {
                        // Loop through and add the inputs to check against
                        logger.User("Processing files:\n");
                        foreach (string input in Inputs)
                        {
                            DatTools.DatFromDir.PopulateFromDir(datdata, input, asFiles: asFiles, hashes: quickScan ? Hash.CRC : Hash.Standard);
                        }

                        Verification.VerifyGeneric(datdata, hashOnly);
                    }

                    // Now write out if there are any items left
                    Writer.WriteStatsToConsole(datdata);
                    Writer.Write(datdata, OutputDir);
                }
            }
            // Otherwise, process all DATs into the same output
            else
            {
                InternalStopwatch watch = new InternalStopwatch("Populating internal DAT");

                // Add all of the input DATs into one huge internal DAT
                DatFile datdata = DatFile.Create();
                foreach (ParentablePath datfile in datfilePaths)
                {
                    Parser.ParseInto(datdata, datfile, int.MaxValue, keep: true);
                }

                // Perform additional processing steps
                Extras.ApplyExtras(datdata);
                Splitter.ApplySplitting(datdata, true);
                Filter.ApplyFilters(datdata);
                Cleaner.ApplyCleaning(datdata);
                Remover.ApplyRemovals(datdata);

                // Set depot information
                datdata.Header.InputDepot = Header.InputDepot?.Clone() as DepotInformation;

                // If we have overridden the header skipper, set it now
                if (!string.IsNullOrEmpty(Header.HeaderSkipper))
                    datdata.Header.HeaderSkipper = Header.HeaderSkipper;

                watch.Stop();

                // If we have the depot flag, respect it
                if (Header.InputDepot?.IsActive ?? false)
                {
                    Verification.VerifyDepot(datdata, Inputs);
                }
                else
                {
                    // Loop through and add the inputs to check against
                    logger.User("Processing files:\n");
                    foreach (string input in Inputs)
                    {
                        DatTools.DatFromDir.PopulateFromDir(datdata, input, asFiles: asFiles, hashes: quickScan ? Hash.CRC : Hash.Standard);
                    }

                    Verification.VerifyGeneric(datdata, hashOnly);
                }

                // Now write out if there are any items left
                Writer.WriteStatsToConsole(datdata);
                Writer.Write(datdata, OutputDir);
            }
        }
    }
}
