
public class Tokeniser(bool debug = false, bool showBenchmark = false)
{
    private OrderedDictionary<int, Tuple<int, int>> Merges = [];

    private void PrintPairs(List<Pair> pairs)
    {
        Console.Write("Pairs");
        Console.WriteLine(" -----------------------------------------------------------------------\n");
        pairs.ForEach(pair =>
            Console.WriteLine("Value: {0,3}, Value Next: {1,3}, Index: {2,3}, Previous Index: {2,3}, Next Index: {2,3}", pair.Value, pair.ValueNext, pair.Index, pair.PreviousIndex, pair.NextIndex)
        );
    }

    private void PrintValues(List<Pair> pairs)
    {
        var values = pairs
            .Where(pair => pair.PreviousIndex != null || pair.NextIndex != null)
            .Select(pair => pair.Value);

        Console.Write("Values");
        Console.WriteLine(" ----------------------------------------------------------------------\n");
        Console.Write(string.Join(" ", values));
        Console.WriteLine($" {pairs.Last().ValueNext}");
    }

    public IList<TrainingStep> Train(int[] tokens, int vocabSize)
    {
        var pairs = new List<Pair>();
        var context = new TokeniserContext();
        var benchmark = new Benchmark();
        var trainingSteps = new List<TrainingStep>();

        benchmark.Measure("Prepare", () =>
        {
            for (int i = 0; i < tokens.Length - 1; i++)
            {
                var previousPairIndex = i - 1;
                var nextPairIndex = i + 1;
                var value = tokens[i];
                var valueNext = tokens[i + 1];

                var pair = new Pair
                {
                    Value = value,
                    ValueNext = valueNext,
                    Index = i,
                    PreviousIndex = previousPairIndex < 0 ? null : previousPairIndex,
                    NextIndex = nextPairIndex > tokens.Length - 1 ? null : nextPairIndex,
                };

                pairs.Add(pair);
                context.AddPair(value, valueNext, i);
            }
        });

        #region Debug
        if (debug)
        {
            Console.WriteLine();
            PrintPairs(pairs);
            Console.WriteLine();
            PrintValues(pairs);
            Console.WriteLine();
        }
        #endregion

        benchmark.Measure("Commit", context.Commit);

        var shouldRun = true;

        while (Merges.Count < vocabSize && shouldRun)
        {
            benchmark.Measure("Merge", () =>
            {
                var mostFrequentPair = context.GetMostFrequentPair();

                if (mostFrequentPair == null)
                {
                    shouldRun = false;
                    return;
                }

                if (mostFrequentPair != null)
                {
                    var isMostFrequentPairMerged = mostFrequentPair.Item1 >= 256 || mostFrequentPair.Item2 >= 256;
                    var mergeValue = Merges.Count + 256;
                    var mostFrequentPairIndexes = context.GetPairIndexes(mostFrequentPair);

                    Merges[mergeValue] = mostFrequentPair;

                    #region Debug
                    if (debug)
                    {
                        Console.Write("Merges");
                        Console.WriteLine(" ----------------------------------------------------------------------\n");
                        Console.WriteLine($"Most frequent pair: {mostFrequentPair.Item1} {mostFrequentPair.Item2}");
                        Console.WriteLine($"Most frequent pair indexes: {string.Join(", ", mostFrequentPairIndexes)}");
                        Console.WriteLine();
                    }

                    #endregion

                    foreach (var pairIndex in mostFrequentPairIndexes)
                    {
                        var currentPair = pairs[pairIndex];
                        var isCurrentPairMerged = currentPair.Value >= 256 || currentPair.ValueNext >= 256;

                        if (currentPair.PreviousIndex == null && currentPair.NextIndex == null)
                        {
                            continue;
                        }

                        if (currentPair.NextIndex != null)
                        {
                            var nextPair = pairs[currentPair.NextIndex.Value];

                            if (currentPair.ValueNext == nextPair.Value && isCurrentPairMerged == isMostFrequentPairMerged)
                            {
                                #region Debug
                                if (debug)
                                {
                                    Console.WriteLine("[Modified Next] Index: {0,3}, Modified Index: {1,3}, Value: {2,3}, Previous Index: {3,3}", currentPair.Index, nextPair.Index, $"{nextPair.Value} -> {mergeValue}", $"{nextPair.PreviousIndex?.ToString() ?? "null"} -> {currentPair.PreviousIndex?.ToString() ?? "null"}");
                                }
                                #endregion

                                context.AddMerge(nextPair.Value, currentPair.Value, mergeValue);
                                context.RemovePair(nextPair.Value, nextPair.ValueNext);
                                context.AddPair(mergeValue, nextPair.ValueNext, nextPair.Index);

                                // Update the next pair.
                                pairs[nextPair.Index].Value = mergeValue;

                                // Update the previous pair.
                                if (currentPair.PreviousIndex != null)
                                {
                                    var previousPair = pairs[currentPair.PreviousIndex.Value];

                                    #region Debug
                                    if (debug)
                                    {
                                        Console.WriteLine("[Modified Prev] Index: {0,3}, Modified Index: {1,3}, Value Next: {2,3}, Next Index: {3,3}", currentPair.Index, previousPair.Index, $"{previousPair.ValueNext} -> {nextPair.Value}", $"{previousPair.NextIndex?.ToString() ?? "null"} -> {currentPair.NextIndex?.ToString() ?? "null"}");
                                    }
                                    #endregion

                                    context.AddMerge(previousPair.Value, nextPair.Value, mergeValue);
                                    context.RemovePair(previousPair.Value, previousPair.ValueNext);
                                    context.AddPair(previousPair.Value, nextPair.Value, previousPair.Index);

                                    pairs[previousPair.Index].ValueNext = nextPair.Value;
                                    pairs[previousPair.Index].NextIndex = currentPair.NextIndex;
                                }

                                #region Debug
                                if (debug)
                                {
                                    Console.WriteLine("[Deleted Next]  Index: {0,3}", currentPair.Index);
                                }
                                #endregion

                                context.RemovePair(currentPair.Value, currentPair.ValueNext);

                                // Point the next pair to the current pair's previous pair.
                                pairs[nextPair.Index].PreviousIndex = currentPair.PreviousIndex;

                                // Remove the current pair.
                                pairs[currentPair.Index].PreviousIndex = null;
                                pairs[currentPair.Index].NextIndex = null;
                            }
                        }
                    }

                     trainingSteps.Add(new TrainingStep
                        {
                            Iterations = trainingSteps.Count,
                            MintedToken = mergeValue,
                            Pair = mostFrequentPair,
                            TokensCount = tokens.Length,
                            MergeTokensCount = mostFrequentPairIndexes.Count,
                            MintedTokensCount = context.Merges.Count,
                            CompressionRatio = (double)tokens.Length / context.Merges.Count,
                            TimeElapsedSeconds = benchmark.Stopwatch.ElapsedMilliseconds / 1000.0,
                        });
                }


                benchmark.Measure("Commit", context.Commit);

                #region Debug
                if (debug)
                {
                    Console.WriteLine();
                    PrintPairs(pairs);
                    Console.WriteLine();
                    PrintValues(pairs);
                    Console.WriteLine();
                }
                #endregion
            });
        }

        #region Debug
        if (debug || showBenchmark)
        {
            Console.Write("Merges Count");
            Console.WriteLine(" ----------------------------------------------------------------\n");
            Console.WriteLine(Merges.Count);
            Console.WriteLine();
        }

        if (showBenchmark)
        {
            benchmark.PrintResults();
        }
        #endregion

        return trainingSteps;
    }

    public void Decode(int[] tokens)
    {
        foreach (var token in tokens)
        {
            if (Merges.ContainsKey(token))
            {
                var merge = Merges[token];
                Console.Write($"{merge.Item1} {merge.Item2} ");
            }
            else
            {
                Console.Write($"{token} ");
            }
        }
    }
}