﻿using System.Collections.Generic;

using SabreTools.Core;
using SabreTools.Help;

namespace RombaSharp.Features
{
    internal class Version : BaseFeature
    {
        public const string Value = "Version";

        public Version()
        {
            Name = Value;
            Flags = new List<string>() { "version" };
            Description = "Prints version";
            _featureType = ParameterType.Flag;
            LongDescription = "Prints version.";
            Features = new Dictionary<string, Feature>();

            // Common Features
            AddCommonFeatures();
        }

        public override void ProcessFeatures(Dictionary<string, Feature> features)
        {
            base.ProcessFeatures(features);
            logger.User($"RombaSharp version: {Prepare.Version}");
        }
    }
}
