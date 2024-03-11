﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SabreTools.Core;
using SabreTools.DatFiles;
using SabreTools.DatTools;
using SabreTools.Help;
using SabreTools.IO;
using SabreTools.Logging;

namespace SabreTools.Features
{
    internal class Split : BaseFeature
    {
        public const string Value = "Split";

        public Split()
        {
            Name = Value;
            Flags = ["sp", "split"];
            Description = "Split input DATs by a given criteria";
            _featureType = ParameterType.Flag;
            LongDescription = "This feature allows the user to split input DATs by a number of different possible criteria. See the individual input information for details. More than one split type is allowed at a time.";
            Features = [];

            // Common Features
            AddCommonFeatures();

            AddFeature(OutputTypeListInput);
            this[OutputTypeListInput]!.AddFeature(DeprecatedFlag);
            AddFeature(OutputDirStringInput);
            AddFeature(InplaceFlag);
            AddFeature(ExtensionFlag);
            this[ExtensionFlag]!.AddFeature(ExtaListInput);
            this[ExtensionFlag]!.AddFeature(ExtbListInput);
            AddFeature(HashFlag);
            AddFeature(LevelFlag);
            this[LevelFlag]!.AddFeature(ShortFlag);
            this[LevelFlag]!.AddFeature(BaseFlag);
            AddFeature(SizeFlag);
            this[SizeFlag]!.AddFeature(RadixInt64Input);
            AddFeature(TotalSizeFlag);
            this[TotalSizeFlag]!.AddFeature(ChunkSizeInt64Input);
            AddFeature(TypeFlag);
        }

        public override bool ProcessFeatures(Dictionary<string, Feature?> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
                return false;

            // Get the splitting mode
            SplittingMode splittingMode = GetSplittingMode(features);
            if (splittingMode == SplittingMode.None)
            {
                logger.Error("No valid splitting mode found!");
                return false;
            }

            // Get only files from the inputs
            List<ParentablePath> files = PathTool.GetFilesOnly(Inputs, appendparent: true);

            // Loop over the input files
            foreach (ParentablePath file in files)
            {
                // Create and fill the new DAT
                DatFile internalDat = DatFile.Create(Header!);
                Parser.ParseInto(internalDat, file);

                // Get the output directory
                OutputDir = (OutputDir ?? string.Empty).Ensure();
                OutputDir = file.GetOutputPath(OutputDir, GetBoolean(features, InplaceValue));

                // Extension splitting
                if (splittingMode.HasFlag(SplittingMode.Extension))
                {
                    (DatFile? extADat, DatFile? extBDat) = DatTools.Splitter.SplitByExtension(internalDat, GetList(features, ExtAListValue), GetList(features, ExtBListValue));
                    if (extADat != null && extBDat != null)
                    {
                        var watch = new InternalStopwatch("Outputting extension-split DATs");

                        // Output both possible DatFiles
                        Writer.Write(extADat, OutputDir);
                        Writer.Write(extBDat, OutputDir);

                        watch.Stop();
                    }
                }

                // Hash splitting
                if (splittingMode.HasFlag(SplittingMode.Hash))
                {
                    Dictionary<string, DatFile> typeDats = DatTools.Splitter.SplitByHash(internalDat);

                    var watch = new InternalStopwatch("Outputting hash-split DATs");

                    // Loop through each type DatFile
#if NET452_OR_GREATER || NETCOREAPP
                    Parallel.ForEach(typeDats.Keys, Globals.ParallelOptions, itemType =>
#elif NET40_OR_GREATER
                    Parallel.ForEach(typeDats.Keys, itemType =>
#else
                    foreach (var itemType in typeDats.Keys)
#endif
                    {
                        Writer.Write(typeDats[itemType], OutputDir!);
#if NET40_OR_GREATER || NETCOREAPP
                    });
#else
                    }
#endif

                    watch.Stop();
                }

                // Level splitting
                if (splittingMode.HasFlag(SplittingMode.Level))
                {
                    logger.Warning("This feature is not implemented: level-split");
                    DatTools.Splitter.SplitByLevel(
                        internalDat,
                        OutputDir!,
                        GetBoolean(features, ShortValue),
                        GetBoolean(features, BaseValue));
                }

                // Size splitting
                if (splittingMode.HasFlag(SplittingMode.Size))
                {
                    (DatFile lessThan, DatFile greaterThan) = DatTools.Splitter.SplitBySize(internalDat, GetInt64(features, RadixInt64Value));

                    var watch = new InternalStopwatch("Outputting size-split DATs");

                    // Output both possible DatFiles
                    Writer.Write(lessThan, OutputDir);
                    Writer.Write(greaterThan, OutputDir);

                    watch.Stop();
                }

                // Total Size splitting
                if (splittingMode.HasFlag(SplittingMode.TotalSize))
                {
                    logger.Warning("This feature is not implemented: level-split");
                    List<DatFile> sizedDats = DatTools.Splitter.SplitByTotalSize(internalDat, GetInt64(features, ChunkSizeInt64Value));

                    var watch = new InternalStopwatch("Outputting total-size-split DATs");

                    // Loop through each type DatFile
#if NET452_OR_GREATER || NETCOREAPP
                    Parallel.ForEach(sizedDats, Globals.ParallelOptions, sizedDat =>
#elif NET40_OR_GREATER
                    Parallel.ForEach(sizedDats, sizedDat =>
#else
                    foreach (var sizedDat in sizedDats)
#endif
                    {
                        Writer.Write(sizedDat, OutputDir);
#if NET40_OR_GREATER || NETCOREAPP
                    });
#else
                    }
#endif

                    watch.Stop();
                }

                // Type splitting
                if (splittingMode.HasFlag(SplittingMode.Type))
                {
                    Dictionary<ItemType, DatFile> typeDats = DatTools.Splitter.SplitByType(internalDat);

                    var watch = new InternalStopwatch("Outputting ItemType DATs");

                    // Loop through each type DatFile
#if NET452_OR_GREATER || NETCOREAPP
                    Parallel.ForEach(typeDats.Keys, Globals.ParallelOptions, itemType =>
#elif NET40_OR_GREATER
                    Parallel.ForEach(typeDats.Keys, itemType =>
#else
                    foreach (var itemType in typeDats.Keys)
#endif
                    {
                        Writer.Write(typeDats[itemType], OutputDir);
#if NET40_OR_GREATER || NETCOREAPP
                    });
#else
                    }
#endif

                    watch.Stop();
                }
            }

            return true;
        }
    }
}
