
public static class TokeniserLeap
{
    public static void Train(int[] tokens, int vocabSize)
    {
        var pairs = new List<Pair>();
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
                LeapIndex = tracker.AddPair(value, valueNext, i),
                State = State.Unchanged,
            };

            ;
            pairs.Add(pair);
        }

        Console.Write(string.Join(" ", pairs
               .Select(pair => pair.Value))
           );
        Console.WriteLine($" {pairs.Last().ValueNext}");

        pairs.ForEach(pair =>
              {
                  Console.WriteLine($"Value: {pair.Value}, ValueNext: {pair.ValueNext}, Index: {pair.Index}, PreviousIndex: {pair.PreviousIndex}, NextIndex: {pair.NextIndex}");
              });

        // Console.WriteLine($"Time taken: {DateTime.UtcNow - now}");

        now = DateTime.UtcNow;
        tracker.Commit();
        // Console.WriteLine($"Time taken: {DateTime.UtcNow - now}");

        now = DateTime.UtcNow;
        for (int i = 0; i < vocabSize; i++)
        {
            var mostFrequentPair = tracker.MostFrequentPair();

            if (mostFrequentPair != null)
            {
                var isMostFrequentPairMerged = mostFrequentPair.Item1 >= 256 || mostFrequentPair.Item2 >= 256;
                var mergeValue = tracker.MergeFrequencies.Count + 256;
                var mostFrequentPairIndexes = tracker.PairIndexes[mostFrequentPair];

                #region Debug
                Console.WriteLine($"{mostFrequentPair} -> {mergeValue}");
                Console.WriteLine($"Most frequent pair indexes: {string.Join(", ", mostFrequentPairIndexes)}");
                Console.WriteLine();
                #endregion  

                if (mostFrequentPairIndexes == null)
                {
                    break;
                }

                foreach (var pairIndex in mostFrequentPairIndexes)
                {
                    var currentPair = pairs[pairIndex];
                    var isCurrentPairMerged = currentPair.Value >= 256 || currentPair.ValueNext >= 256;

                    if (currentPair.PreviousIndex == null && currentPair.NextIndex == null)
                    {
                        continue;
                    }

                    // if (currentPair.PreviousIndex != null)
                    // {
                    //     var previousPair = pairs[currentPair.PreviousIndex.Value];
                    //     var isPreviousPairMerged = previousPair.Value >= 256 || previousPair.ValueNext >= 256;

                    //     if (previousPair.ValueNext == currentPair.Value && isPreviousPairMerged == isMostFrequentPairMerged)
                    //     {
                    //         #region Debug
                    //         Console.Write($"Modified mutation prev: Index: {currentPair.Index}, Modified Index: {previousPair.Index}, ");
                    //         Console.Write($"Value Next: {previousPair.ValueNext} -> {mergeValue}, ");
                    //         Console.Write($"Next Index: {previousPair.NextIndex} -> {currentPair.NextIndex}, ");
                    //         Console.Write("\n");
                    //         #endregion

                    //         tracker.RemovePair(previousPair.Value, previousPair.ValueNext, previousPair.Index);
                    //         tracker.AddPair(previousPair.Value, mergeValue, previousPair.Index);
                    //         tracker.AddMerge(previousPair.Value, mergeValue);

                    //         // Update the previous pair.
                    //         pairs[previousPair.Index].ValueNext = mergeValue;
                    //     }
                    // }

                    if (currentPair.NextIndex != null)
                    {
                        var nextPair = pairs[currentPair.NextIndex.Value];

                        if (currentPair.ValueNext == nextPair.Value && isCurrentPairMerged == isMostFrequentPairMerged)
                        {
                            #region Debug
                            Console.Write($"Modified mutation next: Index: {currentPair.Index}, Modified Index: {nextPair.Index}, ");
                            Console.Write($"Value: {nextPair.Value} -> {mergeValue}, ");
                            Console.Write($"Previous Index: {nextPair.PreviousIndex} -> {currentPair.PreviousIndex}, ");
                            Console.Write("\n");
                            #endregion

                            tracker.RemovePair(nextPair.Value, nextPair.ValueNext, nextPair.Index);
                            tracker.AddPair(mergeValue, nextPair.ValueNext, nextPair.Index);
                            tracker.AddMerge(mergeValue, nextPair.ValueNext);

                            // Update the next pair.
                            pairs[nextPair.Index].Value = mergeValue;

                            // Update the previous pair.
                            if (currentPair.PreviousIndex != null)
                            {
                                var previousPair = pairs[currentPair.PreviousIndex.Value];

                                #region Debug
                                Console.Write($"Modified mutation prev: Index: {currentPair.Index}, Modified Index: {previousPair.Index}, ");
                                Console.Write($"Value Next: {previousPair.ValueNext} -> {nextPair.Value}, ");
                                Console.Write($"Next Index: {previousPair.NextIndex} -> {currentPair.NextIndex}, ");
                                Console.Write("\n");
                                #endregion

                                tracker.RemovePair(previousPair.Value, previousPair.ValueNext, previousPair.Index);
                                tracker.AddPair(previousPair.Value, nextPair.Value, previousPair.Index);

                                pairs[previousPair.Index].ValueNext = nextPair.Value;
                                pairs[previousPair.Index].NextIndex = currentPair.NextIndex;
                            }

                            #region Debug
                            Console.WriteLine($"Deleted mutation: Index: {currentPair.Index}");
                            #endregion

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

            Console.WriteLine();
            pairs.ForEach(pair =>
               {
                   Console.WriteLine($"Value: {pair.Value}, ValueNext: {pair.ValueNext}, Index: {pair.Index}, PreviousIndex: {pair.PreviousIndex}, NextIndex: {pair.NextIndex}");
               });
            Console.Write(string.Join(" ", pairs
                .Where(pair => pair.PreviousIndex != null || pair.NextIndex != null)
                .Select(pair => pair.Value))
            );
            Console.WriteLine($" {pairs.Last().ValueNext}");
            Console.WriteLine();

            if (i >= 2)
            {
                break;
            }
        }
        // Console.WriteLine($"Time taken: {DateTime.UtcNow - now}");

        // Console.WriteLine();
        // pairs.ForEach(pair => {
        //     Console.WriteLine($"Value: {pair.Value}, ValueNext: {pair.ValueNext}, State: {pair.State}, Index: {pair.Index}, PreviousIndex: {pair.PreviousIndex}, NextIndex: {pair.NextIndex}, ModifiedPairValue: {pair.MutationModified?.ModifiedPairValue}, ModifiedPairValueNext: {pair.MutationModified?.ModifiedPairValueNext}, ModifiedPreviousIndex: {pair.MutationModified?.ModifiedPreviousIndex}, ModifiedNextIndex: {pair.MutationModified?.ModifiedNextIndex}");
        // });
        // Console.WriteLine();
        // tracker.PairFrequencies.ToList().ForEach(pair => Console.WriteLine($"Key: {pair.Key}, Value: {pair.Value}"));
        // Console.WriteLine();
    }
}