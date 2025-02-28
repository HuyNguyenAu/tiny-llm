public class Tracker
{
    public OrderedDictionary<Tuple<int, int>, int> PairFrequencies { get; private set; } = [];
    public Dictionary<Tuple<int, int>, List<int>> PairIndexes { get; private set; } = [];

    public void Commit()
    {
        var sortedPairFrequencies = PairFrequencies
            .Where(pair => pair.Value > 1)
            .OrderByDescending(pair => pair.Value);
        PairFrequencies = new OrderedDictionary<Tuple<int, int>, int>(sortedPairFrequencies);
    }

    public Tuple<int, int>? MostFrequentPair()
    {
        if (PairFrequencies.Count == 0)
        {
            return null;
        }

        var mostFrequentPair = PairFrequencies.First();

        if (mostFrequentPair.Value == 1)
        {
            return null;
        }

        return mostFrequentPair.Key;
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
}