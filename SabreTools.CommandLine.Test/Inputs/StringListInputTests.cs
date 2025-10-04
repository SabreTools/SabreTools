using SabreTools.CommandLine.Inputs;
using Xunit;

namespace SabreTools.CommandLine.Test.Inputs
{
    public class StringListInputTests
    {
        [Fact]
        public void ProcessInput_EmptyArgs_Failure()
        {
            string[] args = [];
            int index = 0;

            var input = new StringListInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(0, index);
            Assert.Null(input.Value);
        }

        [Fact]
        public void ProcessInput_NegativeIndex_Failure()
        {
            string[] args = ["a", "true"];
            int index = -1;

            var input = new StringListInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(-1, index);
            Assert.Null(input.Value);
        }

        [Fact]
        public void ProcessInput_OverIndex_Failure()
        {
            string[] args = ["a", "true"];
            int index = 2;

            var input = new StringListInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(2, index);
            Assert.Null(input.Value);
        }

        [Fact]
        public void ProcessInput_Space_InvalidLength_Failure()
        {
            string[] args = ["a"];
            int index = 0;

            var input = new StringListInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(0, index);
            Assert.Null(input.Value);
        }

        [Fact]
        public void ProcessInput_Space_ValidValue_Success()
        {
            string[] args = ["a", "value"];
            int index = 0;

            var input = new StringListInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.True(actual);
            Assert.Equal(1, index);
            Assert.NotNull(input.Value);
            var value = Assert.Single(input.Value);
            Assert.Equal("value", value);
        }

        [Fact]
        public void ProcessInput_Equal_Empty_Success()
        {
            string[] args = ["a="];
            int index = 0;

            var input = new StringListInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.True(actual);
            Assert.Equal(0, index);
            Assert.NotNull(input.Value);
            var value = Assert.Single(input.Value);
            Assert.Empty(value);
        }

        [Fact]
        public void ProcessInput_Equal_ValidValue_Success()
        {
            string[] args = ["a=value"];
            int index = 0;

            var input = new StringListInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.True(actual);
            Assert.Equal(0, index);
            Assert.NotNull(input.Value);
            var value = Assert.Single(input.Value);
            Assert.Equal("value", value);
        }
    }
}
