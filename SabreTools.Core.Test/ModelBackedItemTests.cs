using SabreTools.Data.Models.Metadata;
using Xunit;

namespace SabreTools.Core.Test
{
    public class ModelBackedItemTests
    {
        #region Private Testing Classes

        /// <summary>
        /// Testing implementation of DictionaryBase
        /// </summary>
        private class TestDictionaryBase : DictionaryBase
        {
            public const string NameKey = "__NAME__";
        }

        /// <summary>
        /// Testing implementation of ModelBackedItem
        /// </summary>
        private class TestModelBackedItem : ModelBackedItem<TestDictionaryBase>
        {
            #region Comparision Methods

            /// <inheritdoc/>
            public override bool Equals(ModelBackedItem? other)
            {
                // If other is null
                if (other == null)
                    return false;

                // If the type is mismatched
                if (other is not TestModelBackedItem otherItem)
                    return false;

                // Compare internal models
                return _internal.EqualTo(otherItem._internal);
            }

            /// <inheritdoc/>
            public override bool Equals(ModelBackedItem<TestDictionaryBase>? other)
            {
                // If other is null
                if (other == null)
                    return false;

                // If the type is mismatched
                if (other is not TestModelBackedItem otherItem)
                    return false;

                // Compare internal models
                return _internal.EqualTo(otherItem._internal);
            }

            #endregion
        }

        /// <summary>
        /// Alternate testing implementation of ModelBackedItem
        /// </summary>
        private class TestModelAltBackedItem : ModelBackedItem<TestDictionaryBase>
        {
            #region Comparision Methods

            /// <inheritdoc/>
            public override bool Equals(ModelBackedItem? other)
            {
                // If other is null
                if (other == null)
                    return false;

                // If the type is mismatched
                if (other is not TestModelAltBackedItem otherItem)
                    return false;

                // Compare internal models
                return _internal.EqualTo(otherItem._internal);
            }

            /// <inheritdoc/>
            public override bool Equals(ModelBackedItem<TestDictionaryBase>? other)
            {
                // If other is null
                if (other == null)
                    return false;

                // If the type is mismatched
                if (other is not TestModelAltBackedItem otherItem)
                    return false;

                // Compare internal models
                return _internal.EqualTo(otherItem._internal);
            }

            #endregion
        }

        #endregion

        #region Equals

        [Fact]
        public void Equals_NullOther_False()
        {
            ModelBackedItem self = new TestModelBackedItem();
            ModelBackedItem? other = null;

            bool actual = self.Equals(other);
            Assert.False(actual);
        }

        [Fact]
        public void Equals_MismatchedType_False()
        {
            ModelBackedItem self = new TestModelBackedItem();
            ModelBackedItem? other = new TestModelAltBackedItem();

            bool actual = self.Equals(other);
            Assert.False(actual);
        }

        [Fact]
        public void Equals_MismatchedTypeAlt_False()
        {
            ModelBackedItem self = new TestModelAltBackedItem();
            ModelBackedItem? other = new TestModelBackedItem();

            bool actual = self.Equals(other);
            Assert.False(actual);
        }

        [Fact]
        public void Equals_DifferentModels_False()
        {
            ModelBackedItem<TestDictionaryBase> self = new TestModelBackedItem();
            self.SetFieldValue(TestDictionaryBase.NameKey, "self");

            ModelBackedItem<TestDictionaryBase>? other = new TestModelBackedItem();
            other.SetFieldValue(TestDictionaryBase.NameKey, "other");

            bool actual = self.Equals(other);
            Assert.False(actual);
        }

        [Fact]
        public void Equals_EqualModels_True()
        {
            ModelBackedItem<TestDictionaryBase> self = new TestModelBackedItem();
            self.SetFieldValue(TestDictionaryBase.NameKey, "name");

            ModelBackedItem<TestDictionaryBase>? other = new TestModelBackedItem();
            other.SetFieldValue(TestDictionaryBase.NameKey, "name");

            bool actual = self.Equals(other);
            Assert.True(actual);
        }

        #endregion

        #region RemoveField

        [Fact]
        public void RemoveField_NullItem_False()
        {
            TestModelBackedItem? modelBackedItem = null;
            string? fieldName = TestDictionaryBase.NameKey;
            bool? actual = modelBackedItem?.RemoveField(fieldName);
            Assert.Null(actual);
        }

        [Fact]
        public void RemoveField_EmptyFieldName_False()
        {
            var modelBackedItem = new TestModelBackedItem();
            string? fieldName = string.Empty;
            bool actual = modelBackedItem.RemoveField(fieldName);
            Assert.False(actual);
        }

        [Fact]
        public void RemoveField_MissingKey_True()
        {
            var modelBackedItem = new TestModelBackedItem();
            string? fieldName = TestDictionaryBase.NameKey;
            bool actual = modelBackedItem.RemoveField(fieldName);
            Assert.True(actual);
            Assert.Null(modelBackedItem.GetStringFieldValue(fieldName));
        }

        [Fact]
        public void RemoveField_ValidKey_True()
        {
            var modelBackedItem = new TestModelBackedItem();
            modelBackedItem.SetFieldValue(TestDictionaryBase.NameKey, "value");
            string? fieldName = TestDictionaryBase.NameKey;
            bool actual = modelBackedItem.RemoveField(fieldName);
            Assert.True(actual);
            Assert.Null(modelBackedItem.GetStringFieldValue(fieldName));
        }

        #endregion

        #region ReplaceField

        [Fact]
        public void ReplaceField_NullFrom_False()
        {
            TestModelBackedItem? from = null;
            var to = new TestModelBackedItem();
            string? fieldName = TestDictionaryBase.NameKey;
            bool actual = to.ReplaceField(from, fieldName);
            Assert.False(actual);
        }

        [Fact]
        public void ReplaceField_NullTo_False()
        {
            TestModelBackedItem? from = null;
            TestModelBackedItem? to = new TestModelBackedItem();
            string? fieldName = TestDictionaryBase.NameKey;
            bool actual = to.ReplaceField(from, fieldName);
            Assert.False(actual);
        }

        [Fact]
        public void ReplaceField_EmptyFieldName_False()
        {
            TestModelBackedItem? from = new TestModelBackedItem();
            TestModelBackedItem? to = new TestModelBackedItem();
            string? fieldName = string.Empty;
            bool actual = to.ReplaceField(from, fieldName);
            Assert.False(actual);
        }

        [Fact]
        public void ReplaceField_MissingKey_False()
        {
            TestModelBackedItem? from = new TestModelBackedItem();
            TestModelBackedItem? to = new TestModelBackedItem();
            string? fieldName = TestDictionaryBase.NameKey;
            bool actual = to.ReplaceField(from, fieldName);
            Assert.False(actual);
        }

        [Fact]
        public void ReplaceField_ValidKey_True()
        {
            TestModelBackedItem? from = new TestModelBackedItem();
            from.SetFieldValue(TestDictionaryBase.NameKey, "value");
            TestModelBackedItem? to = new TestModelBackedItem();
            string? fieldName = TestDictionaryBase.NameKey;
            bool actual = to.ReplaceField(from, fieldName);
            Assert.True(actual);
            Assert.Equal("value", to.GetStringFieldValue(TestDictionaryBase.NameKey));
        }

        #endregion

        #region SetField

        [Fact]
        public void SetField_NullItem_False()
        {
            TestModelBackedItem? modelBackedItem = null;
            string? fieldName = TestDictionaryBase.NameKey;
            object value = "value";
            bool? actual = modelBackedItem?.SetField(fieldName, value);
            Assert.Null(actual);
        }

        [Fact]
        public void SetField_EmptyFieldName_False()
        {
            TestModelBackedItem? modelBackedItem = new TestModelBackedItem();
            string? fieldName = string.Empty;
            object value = "value";
            bool actual = modelBackedItem.SetField(fieldName, value);
            Assert.False(actual);
        }

        [Fact]
        public void SetField_MissingKey_False()
        {
            TestModelBackedItem? modelBackedItem = new TestModelBackedItem();
            string? fieldName = Rom.SHA1Key;
            object value = "value";
            bool actual = modelBackedItem.SetField(fieldName, value);
            Assert.False(actual);
        }

        [Fact]
        public void SetField_InvalidKey_True()
        {
            TestModelBackedItem? modelBackedItem = new TestModelBackedItem();
            modelBackedItem.SetFieldValue(TestDictionaryBase.NameKey, "old");
            string? fieldName = "INVALID";
            object value = "value";
            bool actual = modelBackedItem.SetField(fieldName, value);
            Assert.False(actual);
            Assert.Null(modelBackedItem.GetStringFieldValue(fieldName));
        }

        [Fact]
        public void SetField_ValidKey_True()
        {
            TestModelBackedItem? modelBackedItem = new TestModelBackedItem();
            modelBackedItem.SetFieldValue(TestDictionaryBase.NameKey, "old");
            string? fieldName = TestDictionaryBase.NameKey;
            object value = "value";
            bool actual = modelBackedItem.SetField(fieldName, value);
            Assert.True(actual);
            Assert.Equal(value, modelBackedItem.GetStringFieldValue(fieldName));
        }

        #endregion
    }
}
