using System.Collections.Generic;

using SabreTools.Filtering;
using Xunit;

namespace SabreTools.Test.Filtering
{
    public class PopulationTests
    {
        [Fact]
        public void PopulateExclusionNullListTest()
        {
            // Setup the list
            List<string> exclusions = null;

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusionsFromList(exclusions);

            // Check the exclusion lists
            Assert.Empty(remover.DatHeaderRemover.DatHeaderFields);
            Assert.Empty(remover.DatItemRemover.MachineFields);
            Assert.Empty(remover.DatItemRemover.DatItemFields);
        }

        [Fact]
        public void PopulateExclusionEmptyListTest()
        {
            // Setup the list
            List<string> exclusions = new List<string>();

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusionsFromList(exclusions);

            // Check the exclusion lists
            Assert.Empty(remover.DatHeaderRemover.DatHeaderFields);
            Assert.Empty(remover.DatItemRemover.MachineFields);
            Assert.Empty(remover.DatItemRemover.DatItemFields);
        }

        [Fact]
        public void PopulateExclusionHeaderFieldTest()
        {
            // Setup the list
            List<string> exclusions = new List<string>
            {
                "header.datname",
            };

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusionsFromList(exclusions);

            // Check the exclusion lists
            Assert.Single(remover.DatHeaderRemover.DatHeaderFields);
            Assert.Empty(remover.DatItemRemover.MachineFields);
            Assert.Empty(remover.DatItemRemover.DatItemFields);
        }

        [Fact]
        public void PopulateExclusionMachineFieldTest()
        {
            // Setup the list
            List<string> exclusions = new List<string>
            {
                "machine.name",
            };

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusionsFromList(exclusions);

            // Check the exclusion lists
            Assert.Empty(remover.DatHeaderRemover.DatHeaderFields);
            Assert.Single(remover.DatItemRemover.MachineFields);
            Assert.Empty(remover.DatItemRemover.DatItemFields);
        }

        [Fact]
        public void PopulateExclusionDatItemFieldTest()
        {
            // Setup the list
            List<string> exclusions = new List<string>
            {
                "item.name",
            };

            // Setup the remover
            var remover = new Remover();
            remover.PopulateExclusionsFromList(exclusions);

            // Check the exclusion lists
            Assert.Empty(remover.DatHeaderRemover.DatHeaderFields);
            Assert.Empty(remover.DatItemRemover.MachineFields);
            Assert.Single(remover.DatItemRemover.DatItemFields);
        }
    
        [Fact]
        public void PopulateFilterNullListTest()
        {
            // Setup the list
            List<string> filters = null;

            // Setup the filter
            var filter = new Filter();
            filter.PopulateFiltersFromList(filters);

            // Check the filters
            Assert.NotNull(filter.MachineFilter);
            Assert.NotNull(filter.DatItemFilter);
        }

        [Fact]
        public void PopulateFilterEmptyListTest()
        {
            // Setup the list
            List<string> filters = new List<string>();

            // Setup the filter
            var filter = new Filter();
            filter.PopulateFiltersFromList(filters);

            // Check the filters
            Assert.NotNull(filter.MachineFilter);
            Assert.NotNull(filter.DatItemFilter);
        }

        [Fact]
        public void PopulateFilterMachineFieldTest()
        {
            // Setup the list
            List<string> filters = new List<string>
            {
                "machine.name:foo",
                "!machine.name:bar",
            };

            // Setup the filter
            var filter = new Filter();
            filter.PopulateFiltersFromList(filters);

            // Check the filters
            Assert.Contains("foo", filter.MachineFilter.Name.PositiveSet);
            Assert.Contains("bar", filter.MachineFilter.Name.NegativeSet);
            Assert.NotNull(filter.DatItemFilter);
        }

        [Fact]
        public void PopulateFilterDatItemFieldTest()
        {
            // Setup the list
            List<string> filters = new List<string>
            {
                "item.name:foo",
                "!item.name:bar"
            };

            // Setup the filter
            var filter = new Filter();
            filter.PopulateFiltersFromList(filters);

            // Check the filters
            Assert.NotNull(filter.MachineFilter);
            Assert.Contains("foo", filter.DatItemFilter.Name.PositiveSet);
            Assert.Contains("bar", filter.DatItemFilter.Name.NegativeSet);
        }
        
        [Fact]
        public void PopulateFilterMiaDatItemFieldTest()
        {
            // Setup the list
            List<string> filters = new List<string>
            {
                "item.mia:true"
            };

            // Setup the filter
            var filter = new Filter();
            filter.PopulateFiltersFromList(filters);

            // Check the filters
            Assert.NotNull(filter.DatItemFilter);
            Assert.Equal(true, filter.DatItemFilter.MIA.Neutral);
        }
        
        [Fact]
        public void PopulateFilterNegateMiaDatItemFieldTest()
        {
            // Setup the list
            List<string> filters = new List<string>
            {
                "item.mia:false"
            };

            // Setup the filter
            var filter = new Filter();
            filter.PopulateFiltersFromList(filters);

            // Check the filters
            Assert.NotNull(filter.DatItemFilter);
            Assert.Equal(false, filter.DatItemFilter.MIA.Neutral);
        }
    }
}