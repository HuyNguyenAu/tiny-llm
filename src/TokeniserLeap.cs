
public static class TokeniserLeap
{
    public static void Train(int[] tokens, int vocabSize)
    {
        var pairs = new List<Pair>();
        var merges = new Dictionary<int, Tuple<int, int>>();
        var tracker = new Tracker();

        var now = DateTime.UtcNow;

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
            tracker.AddPair(value, valueNext, i);
        }

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

        tracker.Commit();
        Console.WriteLine($"Pre Time taken: {DateTime.UtcNow - now}");

        now = DateTime.UtcNow;
        while (merges.Count < vocabSize)
        {
            var mostFrequentPair = tracker.MostFrequentPair();

            if (mostFrequentPair == null)
            {
                break;
            }

            if (mostFrequentPair != null)
            {
                var isMostFrequentPairMerged = mostFrequentPair.Item1 >= 256 || mostFrequentPair.Item2 >= 256;
                var mergeValue = merges.Count + 256;
                var mostFrequentPairIndexes = tracker.PairIndexes[mostFrequentPair];

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

                            tracker.RemovePair(nextPair.Value, nextPair.ValueNext, nextPair.Index);
                            tracker.AddPair(mergeValue, nextPair.ValueNext, nextPair.Index);

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

                                tracker.RemovePair(previousPair.Value, previousPair.ValueNext, previousPair.Index);
                                tracker.AddPair(previousPair.Value, nextPair.Value, previousPair.Index);

                                pairs[previousPair.Index].ValueNext = nextPair.Value;
                                pairs[previousPair.Index].NextIndex = currentPair.NextIndex;
                            }

                            // #region Debug
                            // Console.WriteLine($"Deleted mutation: Index: {currentPair.Index}");
                            // #endregion

                            tracker.RemovePair(currentPair.Value, currentPair.ValueNext, currentPair.Index);

                            // Point the next pair to the current pair's previous pair.
                            pairs[nextPair.Index].PreviousIndex = currentPair.PreviousIndex;

                            // Remove the current pair.
                            pairs[currentPair.Index].PreviousIndex = null;
                            pairs[currentPair.Index].NextIndex = null;
                        }
                    }
                }
            }

            tracker.Commit();

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
        }
        Console.WriteLine($"Time taken: {DateTime.UtcNow - now}");

        Console.WriteLine($"Merges: {merges.Count}");
    }
}