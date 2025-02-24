
public static class BPETokeniser
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

        return [.. pair_frequencies.OrderByDescending(pair => pair.Value)];
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

    public static bool IsPairInMintedTokens(Tuple<int, int> pair, Dictionary<int, Tuple<int, int>> mintedTokens)
    {
        return mintedTokens.TryGetValue(pair.Item1, out _) || mintedTokens.TryGetValue(pair.Item2, out _);
    }

    public static bool IsAllPairFrequenciesOne(List<KeyValuePair<Tuple<int, int>, int>> pairFrequencies)
    {
        return pairFrequencies.All(pair => pair.Value == 1);
    }
    
    public static bool ShouldRecursivelyEncode(List<KeyValuePair<Tuple<int, int>, int>> pairFrequencies, Dictionary<int, Tuple<int, int>> mintedTokens)
    {
        return !pairFrequencies.Any(pair => !IsPairInMintedTokens(pair.Key, mintedTokens) && pair.Value > 1);
    }

    public static Tuple<int, int>? GetMostFrequentPair(int[] tokens, Dictionary<int, Tuple<int, int>> mintedTokens)
    {
        var pairFrequencies = GetPairFrequencies(tokens);

        if (IsAllPairFrequenciesOne(pairFrequencies))
        {
            return null;
        }

        if (ShouldRecursivelyEncode(pairFrequencies, mintedTokens))
        {
            return pairFrequencies.First().Key;
        }

        for (int i = 0; i < pairFrequencies.Count; i++)
        {
            var pair = pairFrequencies[i].Key;

            if (!IsPairInMintedTokens(pair, mintedTokens))
            {
                return pair;
            }
        }

        return null;
    }

    public static TrainingStep[] Train(int[] tokens, int vocabSize)
    {
        var trainingSteps = new List<TrainingStep>();
        var mintedTokens = new Dictionary<int, Tuple<int, int>>();
        var mergedTokens = tokens;

        for (var i = 0; i < vocabSize; i++)
        {
            var mostFrequentPair = GetMostFrequentPair(mergedTokens, mintedTokens);

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
                CompressionRatio = Math.Round((double) tokens.Length / mergedTokens.Length, 3)
            };
            trainingSteps.Add(trainingStep);
        }

        return [.. trainingSteps];
    }
}