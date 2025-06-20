using System.Text;
using tiny_llm.src;
using Xunit;

namespace tiny_llm.tests
{
    public class TokeniserTests()
    {
        [Fact]
        public void Train_ShortText_ReturnsTrainingSteps()
        {
            // Arrange.
            var tokens = Encoding.UTF8.GetBytes("aaabdaaabac")
                .Select(Convert.ToInt32)
                .ToArray();
            var expected = new List<TrainingStep>{
            new()
            {
                Iterations = 0,
                Pair = new Tuple<int, int>(97, 97),
                MintedToken = 256,
                TokensCount = 11,
                MergeTokensCount = 2,
                MintedTokensCount = 1,
                CompressionRatio = 0,
                TimeElapsedSeconds = 0,
            },
            new()
            {
                Iterations = 1,
                Pair = new Tuple<int, int>(97, 98),
                MintedToken = 257,
                TokensCount = 11,
                MergeTokensCount = 2,
                MintedTokensCount = 2,
                CompressionRatio = 0,
                TimeElapsedSeconds = 0,
            },
            new()
            {
                Iterations = 2,
                Pair = new Tuple<int, int>(256, 257),
                MintedToken = 258,
                TokensCount = 11,
                MergeTokensCount = 2,
                MintedTokensCount = 3,
                CompressionRatio = 0,
                TimeElapsedSeconds = 0,
            },
        };

            var tokeniser = new Tokeniser(new TokeniserOptions());

            // Act.
            var trainingSteps = tokeniser.Train(tokens, 5).ToArray();

            // Assert.
            Assert.Equal(expected.Count, trainingSteps.Length);

            Assert.Equal(expected[0].Iterations, trainingSteps[0].Iterations);
            Assert.Equal(expected[0].Pair, trainingSteps[0].Pair);
            Assert.Equal(expected[0].MintedToken, trainingSteps[0].MintedToken);
            Assert.Equal(expected[0].TokensCount, trainingSteps[0].TokensCount);
            Assert.Equal(expected[0].MergeTokensCount, trainingSteps[0].MergeTokensCount);
            Assert.Equal(expected[0].MintedTokensCount, trainingSteps[0].MintedTokensCount);

            Assert.Equal(expected[1].Iterations, trainingSteps[1].Iterations);
            Assert.Equal(expected[1].Pair, trainingSteps[1].Pair);
            Assert.Equal(expected[1].MintedToken, trainingSteps[1].MintedToken);
            Assert.Equal(expected[1].TokensCount, trainingSteps[1].TokensCount);
            Assert.Equal(expected[1].MergeTokensCount, trainingSteps[1].MergeTokensCount);
            Assert.Equal(expected[1].MintedTokensCount, trainingSteps[1].MintedTokensCount);

            Assert.Equal(expected[2].Iterations, trainingSteps[2].Iterations);
            Assert.Equal(expected[2].Pair, trainingSteps[2].Pair);
            Assert.Equal(expected[2].MintedToken, trainingSteps[2].MintedToken);
            Assert.Equal(expected[2].TokensCount, trainingSteps[2].TokensCount);
            Assert.Equal(expected[2].MergeTokensCount, trainingSteps[2].MergeTokensCount);
            Assert.Equal(expected[2].MintedTokensCount, trainingSteps[2].MintedTokensCount);
        }
    }
}