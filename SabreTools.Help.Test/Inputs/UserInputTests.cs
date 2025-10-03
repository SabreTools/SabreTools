using SabreTools.Help.Inputs;
using Xunit;

namespace SabreTools.Help.Test.Inputs
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

        // TODO: Add Get* tests
        // TODO: Add TryGet* tests

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
