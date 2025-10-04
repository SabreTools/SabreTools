using SabreTools.CommandLine.Inputs;
using Xunit;

namespace SabreTools.CommandLine.Test.Inputs
{
    public class FlagInputTests
    {
        [Fact]
        public void ProcessInput_EmptyArgs_Failure()
        {
            string[] args = [];
            int index = 0;

            var input = new FlagInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(0, index);
            Assert.False(input.Value);
        }

        [Fact]
        public void ProcessInput_NegativeIndex_Failure()
        {
            string[] args = ["a", "true"];
            int index = -1;

            var input = new FlagInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(-1, index);
            Assert.False(input.Value);
        }

        [Fact]
        public void ProcessInput_OverIndex_Failure()
        {
            string[] args = ["a", "true"];
            int index = 2;

            var input = new FlagInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(2, index);
            Assert.False(input.Value);
        }

        [Fact]
        public void ProcessInput_ValidValue_Success()
        {
            string[] args = ["a"];
            int index = 0;

            var input = new FlagInput("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.True(actual);
            Assert.Equal(0, index);
            Assert.True(input.Value);
        }
    }
}
