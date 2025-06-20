namespace tiny_llm.src
{
    internal static class SimpleTokeniser
    {
        public static List<KeyValuePair<Tuple<int, int>, int>> GetPairFrequencies(int[] tokens)
        {
            var pair_frequencies = new Dictionary<Tuple<int, int>, int>();

            for (int i = 0; i < tokens.Length - 1; i++)
            {
                var pair = new Tuple<int, int>(tokens[i], tokens[i + 1]);

                if (pair_frequencies.TryGetValue(pair, out _))
                {
                    pair_frequencies[pair]++;
                }
                else
                {
                    pair_frequencies[pair] = 1;
                }
            }

            return [.. pair_frequencies];
        }

        public static int[] Merge(int[] tokens, Tuple<int, int> pair, int mintedToken)
        {
            var mergedTokens = new List<int>();

            var i = 0;

            while (i <= tokens.Length - 1)
            {
                if (i > tokens.Length - 2)
                {
                    mergedTokens.Add(tokens[i]);
                    break;
                }

                if (tokens[i] == pair.Item1 && tokens[i + 1] == pair.Item2)
                {
                    mergedTokens.Add(mintedToken);
                    i += 2;
                }
                else
                {
                    mergedTokens.Add(tokens[i]);
                    i++;
                }
            }

            return [.. mergedTokens];
        }

        public static Tuple<int, int>? GetMostFrequentPair(int[] tokens)
        {
            var pairFrequencies = GetPairFrequencies(tokens);

            if (pairFrequencies.All(pair => pair.Value == 1))
            {
                return null;
            }

            var nonMintedTokenPair = pairFrequencies
                .Where(pair => pair.Key.Item1 < 256 && pair.Key.Item2 < 256 && pair.Value > 1)
                .OrderByDescending(pair => pair.Value)
                .Select(pair => pair.Key)
                .FirstOrDefault();

            if (nonMintedTokenPair != null)
            {
                return nonMintedTokenPair;
            }

            var mintedTokenPair = pairFrequencies
                .Where(pair => (pair.Key.Item1 >= 256 || pair.Key.Item2 >= 256) && pair.Value > 1)
                .OrderByDescending(pair => pair.Value)
                .Select(pair => pair.Key)
                .FirstOrDefault();

            return mintedTokenPair;
        }

        public static IEnumerable<TrainingStep> Train(int[] tokens, int vocabSize)
        {
            var mintedTokens = new Dictionary<int, Tuple<int, int>>();
            var mergedTokens = tokens;
            var timeElapsedStart = DateTime.UtcNow;

            for (var i = 0; i < vocabSize; i++)
            {
                var mostFrequentPair = GetMostFrequentPair(mergedTokens);

                if (mostFrequentPair == null)
                {
                    break;
                }

                var mintedToken = i + 256;
                mintedTokens[mintedToken] = mostFrequentPair;
                mergedTokens = Merge(mergedTokens, mostFrequentPair, mintedToken);

                var trainingStep = new TrainingStep
                {
                    Iterations = i + 1,
                    Pair = mostFrequentPair,
                    MintedToken = mintedToken,
                    TokensCount = tokens.Length,
                    MergeTokensCount = mergedTokens.Length,
                    MintedTokensCount = mintedTokens.Count,
                    CompressionRatio = Math.Round((double)tokens.Length / mergedTokens.Length, 3),
                    TimeElapsedSeconds = (DateTime.UtcNow - timeElapsedStart).TotalSeconds
                };

                yield return trainingStep;
            }
        }
    }
}