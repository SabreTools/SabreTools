using SabreTools.Help.Inputs;
using Xunit;

namespace SabreTools.Help.Test.Inputs
{
    public class UInt64InputTests
    {
        [Fact]
        public void ProcessInput_EmptyArgs_Failure()
        {
            string[] args = [];
            int index = 0;

            var input = new UInt64Input("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(0, index);
            Assert.Equal(ulong.MinValue, input.Value);
        }

        [Fact]
        public void ProcessInput_NegativeIndex_Failure()
        {
            string[] args = ["a", "5"];
            int index = -1;

            var input = new UInt64Input("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(-1, index);
            Assert.Equal(ulong.MinValue, input.Value);
        }

        [Fact]
        public void ProcessInput_OverIndex_Failure()
        {
            string[] args = ["a", "5"];
            int index = 2;

            var input = new UInt64Input("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(2, index);
            Assert.Equal(ulong.MinValue, input.Value);
        }

        [Fact]
        public void ProcessInput_Space_InvalidLength_Failure()
        {
            string[] args = ["a"];
            int index = 0;

            var input = new UInt64Input("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(0, index);
            Assert.Equal(ulong.MinValue, input.Value);
        }

        [Fact]
        public void ProcessInput_Space_InvalidValue_Failure()
        {
            string[] args = ["a", "ANY"];
            int index = 0;

            var input = new UInt64Input("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(0, index);
            Assert.Equal(ulong.MinValue, input.Value);
        }

        [Fact]
        public void ProcessInput_Space_ValidValue_Success()
        {
            string[] args = ["a", "5"];
            int index = 0;

            var input = new UInt64Input("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.True(actual);
            Assert.Equal(1, index);
            Assert.Equal(5u, input.Value);
        }

        [Fact]
        public void ProcessInput_Equal_InvalidLength_Failure()
        {
            string[] args = ["a="];
            int index = 0;

            var input = new UInt64Input("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(0, index);
            Assert.Equal(ulong.MinValue, input.Value);
        }

        [Fact]
        public void ProcessInput_Equal_InvalidValue_Failure()
        {
            string[] args = ["a=ANY"];
            int index = 0;

            var input = new UInt64Input("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.False(actual);
            Assert.Equal(0, index);
            Assert.Equal(ulong.MinValue, input.Value);
        }

        [Fact]
        public void ProcessInput_Equal_ValidValue_Success()
        {
            string[] args = ["a=5"];
            int index = 0;

            var input = new UInt64Input("a", "a", "a");
            bool actual = input.ProcessInput(args, ref index);

            Assert.True(actual);
            Assert.Equal(0, index);
            Assert.Equal(5u, input.Value);
        }
    }
}
