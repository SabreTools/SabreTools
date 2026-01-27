using System;
using SabreTools.Core.Filter;
using Xunit;

namespace SabreTools.Core.Test.Filter
{
    public class FilterKeyTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("ItemName")]
        [InlineData("ItemName.FieldName.Extra")]
        [InlineData("InvalidItemName.FieldName")]
        [InlineData("DatItem.InvalidFieldName")]
        [InlineData("Item.InvalidFieldName")]
        [InlineData("Sample.InvalidFieldName")]
        public void Constructor_InvalidKey_Throws(string? key)
        {
            Assert.Throws<ArgumentException>(() => new FilterKey(key));
        }

        [Theory]
        [InlineData("header.name", "header", "name")]
        [InlineData("HEADER.NAME", "header", "name")]
        [InlineData("game.name", "machine", "name")]
        [InlineData("GAME.NAME", "machine", "name")]
        [InlineData("machine.name", "machine", "name")]
        [InlineData("MACHINE.NAME", "machine", "name")]
        [InlineData("resource.name", "machine", "name")]
        [InlineData("RESOURCE.NAME", "machine", "name")]
        [InlineData("set.name", "machine", "name")]
        [InlineData("SET.NAME", "machine", "name")]
        [InlineData("datitem.name", "item", "name")]
        [InlineData("DATITEM.NAME", "item", "name")]
        [InlineData("item.name", "item", "name")]
        [InlineData("ITEM.NAME", "item", "name")]
        [InlineData("sample.name", "sample", "name")]
        [InlineData("SAMPLE.NAME", "sample", "name")]
        public void Constructor_ValidKey_Sets(string? key, string expectedItemName, string expectedFieldName)
        {
            FilterKey filterKey = new FilterKey(key);
            Assert.Equal(expectedItemName, filterKey.ItemName);
            Assert.Equal(expectedFieldName, filterKey.FieldName);
        }

        [Theory]
        [InlineData("", "FieldName")]
        [InlineData("ItemName", "")]
        [InlineData("DatItem", "InvalidFieldName")]
        [InlineData("Item", "InvalidFieldName")]
        [InlineData("Sample", "InvalidFieldName")]
        public void Constructor_InvalidNames_Throws(string itemName, string fieldName)
        {
            Assert.Throws<ArgumentException>(() => new FilterKey(itemName, fieldName));
        }

        [Theory]
        [InlineData("header", "name", "header", "name")]
        [InlineData("HEADER", "NAME", "header", "name")]
        [InlineData("game", "name", "machine", "name")]
        [InlineData("GAME", "NAME", "machine", "name")]
        [InlineData("machine", "name", "machine", "name")]
        [InlineData("MACHINE", "NAME", "machine", "name")]
        [InlineData("resource", "name", "machine", "name")]
        [InlineData("RESOURCE", "NAME", "machine", "name")]
        [InlineData("set", "name", "machine", "name")]
        [InlineData("SET", "NAME", "machine", "name")]
        [InlineData("datitem", "name", "item", "name")]
        [InlineData("DATITEM", "NAME", "item", "name")]
        [InlineData("item", "name", "item", "name")]
        [InlineData("ITEM", "NAME", "item", "name")]
        [InlineData("sample", "name", "sample", "name")]
        [InlineData("SAMPLE", "NAME", "sample", "name")]
        public void Constructor_ValidNames_Sets(string itemName, string fieldName, string expectedItemName, string expectedFieldName)
        {
            FilterKey filterKey = new FilterKey(itemName, fieldName);
            Assert.Equal(expectedItemName, filterKey.ItemName);
            Assert.Equal(expectedFieldName, filterKey.FieldName);
        }
    }
}
