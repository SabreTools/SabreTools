using System.Collections.Generic;
using System.IO;

using SabreTools.DatFiles;
using SabreTools.DatTools;
using SabreTools.Help;
using SabreTools.IO;

namespace RombaSharp.Features
{
    internal class EDiffdat : BaseFeature
    {
        public const string Value = "EDiffdat";

        public EDiffdat()
        {
            Name = Value;
            Flags = new List<string>() { "ediffdat" };
            Description = "Creates a DAT file with those entries that are in -new DAT.";
            _featureType = ParameterType.Flag;
            LongDescription = @"Creates a DAT file with those entries that are in -new DAT files and not in -old DAT files. Ignores those entries in -old that are not in -new.";
            Features = new Dictionary<string, Feature>();

            // Common Features
            AddCommonFeatures();

            AddFeature(OutStringInput);
            AddFeature(OldStringInput);
            AddFeature(NewStringInput);
        }

        public override bool ProcessFeatures(Dictionary<string, Feature> features)
        {
            // If the base fails, just fail out
            if (!base.ProcessFeatures(features))
                return false;

            // Get feature flags
            string olddat = GetString(features, OldStringValue);
            string outdat = GetString(features, OutStringValue);
            string newdat = GetString(features, NewStringValue);

            // Ensure the output directory
            outdat.Ensure(create: true);

            // Check that all required files exist
            if (!File.Exists(olddat))
            {
                logger.Error($"File '{olddat}' does not exist!");
                return false;
            }

            if (!File.Exists(newdat))
            {
                logger.Error($"File '{newdat}' does not exist!");
                return false;
            }

            // Create the encapsulating datfile
            DatFile datfile = Parser.CreateAndParse(olddat);
            datfile.Items.BucketBy(SabreTools.DatItems.ItemKey.CRC, DedupeType.None);

            // Diff against the new datfile
            DatFile intDat = Parser.CreateAndParse(newdat);
            DatFileTool.DiffAgainst(datfile, intDat, false);
            Writer.Write(intDat, outdat);
            return true;
        }
    }
}
