using System.Text;
using tiny_llm.src;
using Xunit;

namespace tiny_llm.tests
{
    public class SimpleTokeniserTests()
    {
        [Fact]
        public void GetPairFrequencies_ReturnsOrderedDescendingPairFrequencies()
        {
            // Arrange.
            var tokens = Encoding.UTF8.GetBytes("aaabdaaabac").Select(c => (int)c).ToArray();
            var expected = new List<KeyValuePair<Tuple<int, int>, int>>
        {
            new(new Tuple<int, int>(97, 97), 4),
            new(new Tuple<int, int>(97, 98), 2),
            new(new Tuple<int, int>(98, 100), 1),
            new(new Tuple<int, int>(100, 97), 1),
            new(new Tuple<int, int>(98, 97), 1),
            new(new Tuple<int, int>(97, 99), 1),
        };

            // Act
            var result = SimpleTokeniser.GetPairFrequencies(tokens);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Merge_ReturnsMergedTokens()
        {
            // Arrange.
            var tokens = Encoding.UTF8.GetBytes("aaabdaaabac").Select(c => (int)c).ToArray();

            var pairFrequenciesStep0 = new List<KeyValuePair<Tuple<int, int>, int>>
        {
            new(new Tuple<int, int>(97, 97), 4),
            new(new Tuple<int, int>(97, 98), 2),
            new(new Tuple<int, int>(98, 100), 1),
            new(new Tuple<int, int>(100, 97), 1),
            new(new Tuple<int, int>(98, 97), 1),
            new(new Tuple<int, int>(97, 99), 1),
        };
            var pairFrequenciesStep1 = new List<KeyValuePair<Tuple<int, int>, int>>
        {
            new(new Tuple<int, int>(90, 97), 2),
            new(new Tuple<int, int>(97, 98), 2),
            new(new Tuple<int, int>(98, 100), 1),
            new(new Tuple<int, int>(100, 90), 1),
            new(new Tuple<int, int>(98, 97), 1),
            new(new Tuple<int, int>(97, 99), 1),
        };
            var pairFrequenciesStep2 = new List<KeyValuePair<Tuple<int, int>, int>>
        {
            new(new Tuple<int, int>(90, 89), 2),
            new(new Tuple<int, int>(98, 100), 1),
            new(new Tuple<int, int>(100, 90), 1),
            new(new Tuple<int, int>(89, 97), 1),
            new(new Tuple<int, int>(97, 99), 1),
        };

            var mintedTokenStep0 = 90;
            var mintedTokenStep1 = 89;
            var mintedTokenStep2 = 88;

            // Act.
            var resultStep0 = SimpleTokeniser.Merge(tokens, pairFrequenciesStep0[0].Key, mintedTokenStep0);
            var resultStep1 = SimpleTokeniser.Merge(resultStep0, pairFrequenciesStep1[1].Key, mintedTokenStep1);
            var resultStep2 = SimpleTokeniser.Merge(resultStep1, pairFrequenciesStep2[0].Key, mintedTokenStep2);

            // Assert.
            Assert.Equal(Encoding.UTF8.GetBytes("ZabdZabac").Select(c => (int)c).ToArray(), resultStep0);
            Assert.Equal(Encoding.UTF8.GetBytes("ZYdZYac").Select(c => (int)c).ToArray(), resultStep1);
            Assert.Equal(Encoding.UTF8.GetBytes("XdXac").Select(c => (int)c).ToArray(), resultStep2);
        }

        [Fact]
        public void Train_ShortText_ReturnsTrainingSteps()
        {
            // Arrange.
            var tokens = Encoding.UTF8.GetBytes("aaabdaaabac")
                .Select(Convert.ToInt32)
                .ToArray();
            var expected = new List<TrainingStep>
        {
            new()
            {
                Iterations = 1,
                Pair = new Tuple<int, int>(97, 97),
                MintedToken = 256,
                TokensCount = 11,
                MergeTokensCount = 9,
                MintedTokensCount = 1,
                CompressionRatio = 1.222,
                TimeElapsedSeconds = 0.001,
            },
            new()
            {
                Iterations = 2,
                Pair = new Tuple<int, int>(97, 98),
                MintedToken = 257,
                TokensCount = 11,
                MergeTokensCount = 7,
                MintedTokensCount = 2,
                CompressionRatio = 1.571,
                TimeElapsedSeconds = 0.002,
            },
            new()
            {
                Iterations = 3,
                Pair = new Tuple<int, int>(256, 257),
                MintedToken = 258,
                TokensCount = 11,
                MergeTokensCount = 5,
                MintedTokensCount = 3,
                CompressionRatio = 2.200,
                TimeElapsedSeconds = 0.003,
            },
        };

            // Act.
            var trainingSteps = SimpleTokeniser.Train(tokens, 5).ToArray();

            // Assert.
            Assert.Equal(3, trainingSteps.Length);

            Assert.Equal(expected[0].Iterations, trainingSteps[0].Iterations);
            Assert.Equal(expected[0].Pair, trainingSteps[0].Pair);
            Assert.Equal(expected[0].MintedToken, trainingSteps[0].MintedToken);
            Assert.Equal(expected[0].TokensCount, trainingSteps[0].TokensCount);
            Assert.Equal(expected[0].MergeTokensCount, trainingSteps[0].MergeTokensCount);
            Assert.Equal(expected[0].MintedTokensCount, trainingSteps[0].MintedTokensCount);
            Assert.Equal(expected[0].CompressionRatio, trainingSteps[0].CompressionRatio);

            Assert.Equal(expected[1].Iterations, trainingSteps[1].Iterations);
            Assert.Equal(expected[1].Pair, trainingSteps[1].Pair);
            Assert.Equal(expected[1].MintedToken, trainingSteps[1].MintedToken);
            Assert.Equal(expected[1].TokensCount, trainingSteps[1].TokensCount);
            Assert.Equal(expected[1].MergeTokensCount, trainingSteps[1].MergeTokensCount);
            Assert.Equal(expected[1].MintedTokensCount, trainingSteps[1].MintedTokensCount);
            Assert.Equal(expected[1].CompressionRatio, trainingSteps[1].CompressionRatio);

            Assert.Equal(expected[2].Iterations, trainingSteps[2].Iterations);
            Assert.Equal(expected[2].Pair, trainingSteps[2].Pair);
            Assert.Equal(expected[2].MintedToken, trainingSteps[2].MintedToken);
            Assert.Equal(expected[2].TokensCount, trainingSteps[2].TokensCount);
            Assert.Equal(expected[2].MergeTokensCount, trainingSteps[2].MergeTokensCount);
            Assert.Equal(expected[2].MintedTokensCount, trainingSteps[2].MintedTokensCount);
        }
    }
}