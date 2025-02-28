
public static class Tokeniser
{
    public static void Train(int[] tokens, int vocabSize)
    {
        var pairs = new List<Pair>();
        var merges = new Dictionary<int, Tuple<int, int>>();
        var context = new TokeniserContext();
        var benchmark = new Benchmark();

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

        // #region Debug
        // Console.Write(string.Join(" ", pairs
        //        .Select(pair => pair.Value))
        //    );
        // Console.WriteLine($" {pairs.Last().ValueNext}");

        // pairs.ForEach(pair =>
        //       {
        //           Console.WriteLine($"Value: {pair.Value}, ValueNext: {pair.ValueNext}, Index: {pair.Index}, PreviousIndex: {pair.PreviousIndex}, NextIndex: {pair.NextIndex}");
        //       });
        // #endregion

        benchmark.Measure("Commit", context.Commit);
        var shouldRun = true;

        while (merges.Count < vocabSize && shouldRun)
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
                    var mergeValue = merges.Count + 256;
                    var mostFrequentPairIndexes = context.GetPairIndexes(mostFrequentPair);

                    merges[mergeValue] = mostFrequentPair;

                    // #region Debug
                    // Console.WriteLine($"{mostFrequentPair} -> {mergeValue}");
                    // Console.WriteLine($"Most frequent pair indexes: {string.Join(", ", mostFrequentPairIndexes)}");
                    // Console.WriteLine();
                    // #endregion  

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
                                // #region Debug
                                // Console.Write($"Modified mutation next: Index: {currentPair.Index}, Modified Index: {nextPair.Index}, ");
                                // Console.Write($"Value: {nextPair.Value} -> {mergeValue}, ");
                                // Console.Write($"Previous Index: {nextPair.PreviousIndex} -> {currentPair.PreviousIndex}, ");
                                // Console.Write("\n");
                                // #endregion

                                context.RemovePair(nextPair.Value, nextPair.ValueNext);
                                context.AddPair(mergeValue, nextPair.ValueNext, nextPair.Index);

                                // Update the next pair.
                                pairs[nextPair.Index].Value = mergeValue;

                                // Update the previous pair.
                                if (currentPair.PreviousIndex != null)
                                {
                                    var previousPair = pairs[currentPair.PreviousIndex.Value];

                                    // #region Debug
                                    // Console.Write($"Modified mutation prev: Index: {currentPair.Index}, Modified Index: {previousPair.Index}, ");
                                    // Console.Write($"Value Next: {previousPair.ValueNext} -> {nextPair.Value}, ");
                                    // Console.Write($"Next Index: {previousPair.NextIndex} -> {currentPair.NextIndex}, ");
                                    // Console.Write("\n");
                                    // #endregion

                                    context.RemovePair(previousPair.Value, previousPair.ValueNext);
                                    context.AddPair(previousPair.Value, nextPair.Value, previousPair.Index);

                                    pairs[previousPair.Index].ValueNext = nextPair.Value;
                                    pairs[previousPair.Index].NextIndex = currentPair.NextIndex;
                                }

                                // #region Debug
                                // Console.WriteLine($"Deleted mutation: Index: {currentPair.Index}");
                                // #endregion

                                context.RemovePair(currentPair.Value, currentPair.ValueNext);

                                // Point the next pair to the current pair's previous pair.
                                pairs[nextPair.Index].PreviousIndex = currentPair.PreviousIndex;

                                // Remove the current pair.
                                pairs[currentPair.Index].PreviousIndex = null;
                                pairs[currentPair.Index].NextIndex = null;
                            }
                        }
                    }
                }


                benchmark.Measure("Commit", context.Commit);

                // Console.WriteLine();
                // pairs.ForEach(pair =>
                //    {
                //        Console.WriteLine($"Value: {pair.Value}, ValueNext: {pair.ValueNext}, Index: {pair.Index}, PreviousIndex: {pair.PreviousIndex}, NextIndex: {pair.NextIndex}");
                //    });
                // Console.Write(string.Join(" ", pairs
                //     .Where(pair => pair.PreviousIndex != null || pair.NextIndex != null)
                //     .Select(pair => pair.Value))
                // );
                // Console.WriteLine($" {pairs.Last().ValueNext}");
                // Console.WriteLine();
            });
        }

        benchmark.PrintResults();

        Console.WriteLine($"Merges: {merges.Count}");
    }
}