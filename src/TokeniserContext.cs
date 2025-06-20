namespace tiny_llm.src
{
    internal class TokeniserContext
    {
        public OrderedDictionary<int, Tuple<int, int>> Merges { get; private set; } = [];
        private OrderedDictionary<Tuple<int, int>, int> PairFrequencies { get; set; } = [];
        private Dictionary<Tuple<int, int>, List<int>> PairIndexes { get; set; } = [];
        private Tuple<int, int>? MostFrequentPair { get; set; }

        public int MergesCount { get; private set; } = 0;

        public void Commit()
        {
            if (PairFrequencies.Count == 0)
            {
                MostFrequentPair = null;
            }

            var mostFrequentPairFrequencies = PairFrequencies.MaxBy(pair => pair.Value);

            if (mostFrequentPairFrequencies.Value == 1)
            {
                MostFrequentPair = null;
            }
            else
            {
                MostFrequentPair = mostFrequentPairFrequencies.Key;
            }
        }

        public Tuple<int, int>? GetMostFrequentPair()
        {
            return MostFrequentPair;
        }

        public List<int> GetPairIndexes(Tuple<int, int> pair)
        {
            if (PairIndexes.TryGetValue(pair, out var _))
            {
                return PairIndexes[pair];
            }

            return [];
        }

        public void AddPair(int pairValue, int pairValueNext, int pairIndex)
        {
            var pair = new Tuple<int, int>(pairValue, pairValueNext);

            if (PairFrequencies.TryGetValue(pair, out _))
            {
                PairFrequencies[pair]++;
            }
            else
            {
                PairFrequencies[pair] = 1;
            }

            if (PairIndexes.TryGetValue(pair, out var _))
            {
                PairIndexes[pair].Add(pairIndex);
            }
            else
            {
                PairIndexes[pair] = [pairIndex];
            }
        }

        public void RemovePair(int pairValue, int pairValueNext)
        {
            var pair = new Tuple<int, int>(pairValue, pairValueNext);

            if (PairFrequencies.TryGetValue(pair, out _))
            {
                PairFrequencies[pair]--;
            }
        }

        public void AddMerge(int pairValue, int pairValueNext, int mergeValue, bool ignoreMergesCountTracking = false)
        {
            Merges[mergeValue] = new Tuple<int, int>(pairValue, pairValueNext);

            if (!ignoreMergesCountTracking)
            {
                MergesCount++;
            }
        }

        public void ResetMergeCount()
        {
            MergesCount = 0;
        }
    }
}