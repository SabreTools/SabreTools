using SabreTools.CommandLine.Inputs;
using Xunit;

namespace SabreTools.CommandLine.Test
{
    public class CommandSetTests
    {
        [Fact]
        public void AddAndRetrieveTest()
        {
            var input1 = new FlagInput("input1", "--input1", "input1");
            var input2 = new FlagInput("input2", "--input2", "input2");

            var featureSet = new CommandSet();
            featureSet.Add(input1);
            featureSet.Add(input2);

            var actualInput1 = featureSet["input1"];
            Assert.NotNull(actualInput1);
            Assert.Equal("input1", actualInput1.Name);

            var actualInput2 = featureSet[input2];
            Assert.NotNull(actualInput2);
            Assert.Equal("input2", actualInput2.Name);

            var actualInput3 = featureSet["input3"];
            Assert.Null(actualInput3);
        }

        [Fact]
        public void GetInputNameTest()
        {
            var input1 = new FlagInput("input1", "--input1", "input1");
            var input2 = new FlagInput("input2", "--input2", "input2");

            var featureSet = new CommandSet();
            featureSet.Add(input1);
            featureSet.Add(input2);

            var actualName1 = featureSet.GetInputName("input1");
            Assert.NotEmpty(actualName1);
            Assert.Equal("input1", actualName1);

            var actualName2 = featureSet.GetInputName("--input2");
            Assert.NotEmpty(actualName2);
            Assert.Equal("input2", actualName2);

            var actualName3 = featureSet.GetInputName("input3");
            Assert.Empty(actualName3);
        }

        [Fact]
        public void TopLevelFlagTest()
        {
            var input1 = new FlagInput("input1", "--input1", "input1");
            var input2 = new FlagInput("input2", "--input2", "input2");

            var featureSet = new CommandSet();
            featureSet.Add(input1);
            featureSet.Add(input2);

            bool actualTop1 = featureSet.TopLevelFlag("input1");
            Assert.True(actualTop1);

            bool actualTop2 = featureSet.TopLevelFlag("--input2");
            Assert.True(actualTop2);

            bool actualTop3 = featureSet.TopLevelFlag("input3");
            Assert.False(actualTop3);
        }
    }
}
