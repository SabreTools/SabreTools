using System;
using System.Collections.Generic;
using SabreTools.CommandLine.Inputs;
using Xunit;

namespace SabreTools.CommandLine.Test.Inputs
{
    public class UserInputTests
    {
        [Fact]
        public void AddAndRetrieveTest()
        {
            var input1 = new FlagInput("input1", "--input1", "input1");
            var input2 = new FlagInput("input2", "--input2", "input2");

            var userInput = new MockUserInput("a", "a", "a");
            userInput.Add(input1);
            userInput.Add(input2);

            var actualInput1 = userInput["input1"];
            Assert.NotNull(actualInput1);
            Assert.Equal("input1", actualInput1.Name);

            var actualInput2 = userInput[input2];
            Assert.NotNull(actualInput2);
            Assert.Equal("input2", actualInput2.Name);

            var actualInput3 = userInput["input3"];
            Assert.Null(actualInput3);
        }

        [Fact]
        public void ContainsFlagTest()
        {
            var userInput = new MockUserInput("a", ["a", "--b"], "a");

            bool exactActual = userInput.ContainsFlag("a");
            Assert.True(exactActual);

            bool equalsActual = userInput.ContainsFlag("--b=");
            Assert.True(equalsActual);

            bool noMatchActual = userInput.ContainsFlag("-c");
            Assert.False(noMatchActual);
        }

        [Fact]
        public void StartsWithTest()
        {
            var userInput = new MockUserInput("a", ["a", "--b"], "a");

            bool exactActual = userInput.StartsWith('a');
            Assert.True(exactActual);

            bool trimActual = userInput.StartsWith('b');
            Assert.True(trimActual);

            bool noMatchActual = userInput.StartsWith('c');
            Assert.False(noMatchActual);
        }

        #region GetBoolean

        [Fact]
        public void GetBoolean_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new BooleanInput("b", "b", "b");
            userInput.Add(child);

            bool actual = userInput.GetBoolean("c");
            Assert.False(actual);
        }

        [Fact]
        public void GetBoolean_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetBoolean("b"));
        }

        [Fact]
        public void GetBoolean_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new BooleanInput("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "true"], ref index);

            bool actual = userInput.GetBoolean("b");
            Assert.True(actual);
        }

        [Fact]
        public void GetBoolean_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new BooleanInput("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "true"], ref index);

            bool actual = userInput.GetBoolean("c");
            Assert.True(actual);
        }

        #endregion

        #region GetInt8

        [Fact]
        public void GetInt8_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new Int8Input("b", "b", "b");
            userInput.Add(child);

            sbyte actual = userInput.GetInt8("c");
            Assert.Equal(sbyte.MinValue, actual);
        }

        [Fact]
        public void GetInt8_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetInt8("b"));
        }

        [Fact]
        public void GetInt8_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new Int8Input("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "5"], ref index);

            sbyte actual = userInput.GetInt8("b");
            Assert.Equal(5, actual);
        }

        [Fact]
        public void GetInt8_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new Int8Input("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "5"], ref index);

            sbyte actual = userInput.GetInt8("c");
            Assert.Equal(5, actual);
        }

        #endregion

        #region GetInt16

        [Fact]
        public void GetInt16_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new Int16Input("b", "b", "b");
            userInput.Add(child);

            short actual = userInput.GetInt16("c");
            Assert.Equal(short.MinValue, actual);
        }

        [Fact]
        public void GetInt16_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetInt16("b"));
        }

        [Fact]
        public void GetInt16_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new Int16Input("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "5"], ref index);

            short actual = userInput.GetInt16("b");
            Assert.Equal(5, actual);
        }

        [Fact]
        public void GetInt16_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new Int16Input("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "5"], ref index);

            short actual = userInput.GetInt16("c");
            Assert.Equal(5, actual);
        }

        #endregion

        #region GetInt32

        [Fact]
        public void GetInt32_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new Int32Input("b", "b", "b");
            userInput.Add(child);

            int actual = userInput.GetInt32("c");
            Assert.Equal(int.MinValue, actual);
        }

        [Fact]
        public void GetInt32_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetInt32("b"));
        }

        [Fact]
        public void GetInt32_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new Int32Input("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "5"], ref index);

            int actual = userInput.GetInt32("b");
            Assert.Equal(5, actual);
        }

        [Fact]
        public void GetInt32_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new Int32Input("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "5"], ref index);

            int actual = userInput.GetInt32("c");
            Assert.Equal(5, actual);
        }

        #endregion

        #region GetInt64

        [Fact]
        public void GetInt64_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new Int64Input("b", "b", "b");
            userInput.Add(child);

            long actual = userInput.GetInt64("c");
            Assert.Equal(long.MinValue, actual);
        }

        [Fact]
        public void GetInt64_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetInt64("b"));
        }

        [Fact]
        public void GetInt64_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new Int64Input("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "5"], ref index);

            long actual = userInput.GetInt64("b");
            Assert.Equal(5, actual);
        }

        [Fact]
        public void GetInt64_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new Int64Input("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "5"], ref index);

            long actual = userInput.GetInt64("c");
            Assert.Equal(5, actual);
        }

        #endregion

        #region GetString

        [Fact]
        public void GetString_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new StringInput("b", "b", "b");
            userInput.Add(child);

            string? actual = userInput.GetString("c");
            Assert.Null(actual);
        }

        [Fact]
        public void GetString_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetString("b"));
        }

        [Fact]
        public void GetString_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new StringInput("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "value"], ref index);

            string? actual = userInput.GetString("b");
            Assert.Equal("value", actual);
        }

        [Fact]
        public void GetString_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new StringInput("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "value"], ref index);

            string? actual = userInput.GetString("c");
            Assert.Equal("value", actual);
        }

        #endregion

        #region GetStringList

        [Fact]
        public void GetStringList_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new StringListInput("b", "b", "b");
            userInput.Add(child);

            List<string> actual = userInput.GetStringList("c");
            Assert.Empty(actual);
        }

        [Fact]
        public void GetStringList_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetStringList("b"));
        }

        [Fact]
        public void GetStringList_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new StringListInput("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "value"], ref index);

            List<string> actual = userInput.GetStringList("b");
            string value = Assert.Single(actual);
            Assert.Equal("value", value);
        }

        [Fact]
        public void GetStringList_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new StringListInput("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "value"], ref index);

            List<string> actual = userInput.GetStringList("c");
            string value = Assert.Single(actual);
            Assert.Equal("value", value);
        }

        #endregion

        #region GetUInt8

        [Fact]
        public void GetUInt8_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new UInt8Input("b", "b", "b");
            userInput.Add(child);

            byte actual = userInput.GetUInt8("c");
            Assert.Equal(byte.MinValue, actual);
        }

        [Fact]
        public void GetUInt8_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetUInt8("b"));
        }

        [Fact]
        public void GetUInt8_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new UInt8Input("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "5"], ref index);

            byte actual = userInput.GetUInt8("b");
            Assert.Equal(5, actual);
        }

        [Fact]
        public void GetUInt8_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new UInt8Input("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "5"], ref index);

            byte actual = userInput.GetUInt8("c");
            Assert.Equal(5, actual);
        }

        #endregion

        #region GetUInt16

        [Fact]
        public void GetUInt16_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new UInt16Input("b", "b", "b");
            userInput.Add(child);

            ushort actual = userInput.GetUInt16("c");
            Assert.Equal(ushort.MinValue, actual);
        }

        [Fact]
        public void GetUInt16_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetUInt16("b"));
        }

        [Fact]
        public void GetUInt16_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new UInt16Input("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "5"], ref index);

            ushort actual = userInput.GetUInt16("b");
            Assert.Equal(5, actual);
        }

        [Fact]
        public void GetUInt16_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new UInt16Input("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "5"], ref index);

            ushort actual = userInput.GetUInt16("c");
            Assert.Equal(5, actual);
        }

        #endregion

        #region GetUInt32

        [Fact]
        public void GetUInt32_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new UInt32Input("b", "b", "b");
            userInput.Add(child);

            uint actual = userInput.GetUInt32("c");
            Assert.Equal(uint.MinValue, actual);
        }

        [Fact]
        public void GetUInt32_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetUInt32("b"));
        }

        [Fact]
        public void GetUInt32_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new UInt32Input("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "5"], ref index);

            uint actual = userInput.GetUInt32("b");
            Assert.Equal(5u, actual);
        }

        [Fact]
        public void GetUInt32_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new UInt32Input("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "5"], ref index);

            uint actual = userInput.GetUInt32("c");
            Assert.Equal(5u, actual);
        }

        #endregion

        #region GetUInt64

        [Fact]
        public void GetUInt64_InvalidKey_DefaultValue()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new UInt64Input("b", "b", "b");
            userInput.Add(child);

            ulong actual = userInput.GetUInt64("c");
            Assert.Equal(ulong.MinValue, actual);
        }

        [Fact]
        public void GetUInt64_Exists_WrongType_Throws()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);

            Assert.Throws<ArgumentException>(() => _ = userInput.GetUInt64("b"));
        }

        [Fact]
        public void GetUInt64_Exists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new UInt64Input("b", "b", "b");
            userInput.Add(child);

            int index = 0;
            child.ProcessInput(["b", "5"], ref index);

            ulong actual = userInput.GetUInt64("b");
            Assert.Equal(5u, actual);
        }

        [Fact]
        public void GetUInt64_NestedExists_Returns()
        {
            UserInput userInput = new MockUserInput("a", "a", "a");
            var child = new MockUserInput("b", "b", "b");
            userInput.Add(child);
            var subChild = new UInt64Input("c", "c", "c");
            child.Add(subChild);

            int index = 0;
            subChild.ProcessInput(["c", "5"], ref index);

            ulong actual = userInput.GetUInt64("c");
            Assert.Equal(5u, actual);
        }

        #endregion

        /// <summary>
        /// Mock UserInput implementation for testing
        /// </summary>
        private class MockUserInput : UserInput<object?>
        {
            public MockUserInput(string name, string flag, string description, string? longDescription = null)
                : base(name, flag, description, longDescription)
            {
            }

            public MockUserInput(string name, string[] flags, string description, string? longDescription = null)
                : base(name, flags, description, longDescription)
            {
            }

            /// <inheritdoc/>
            public override bool ProcessInput(string[] args, ref int index) => true;

            /// <inheritdoc/>
            protected override string FormatFlags() => string.Empty;
        }
    }
}
