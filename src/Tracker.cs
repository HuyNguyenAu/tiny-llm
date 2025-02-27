public class Tracker
{
    public OrderedDictionary<Tuple<int, int>, int> PairFrequencies { get; private set; } = [];
    public Dictionary<Tuple<int, int>, List<int>> PairIndexes { get; private set; } = [];

    public OrderedDictionary<int, int> MergeFrequencies { get; private set; } = [];
    public Dictionary<int, List<int>> MergeIndexes { get; private set; } = [];

    public void Commit()
    {
        var sortedPairFrequencies = PairFrequencies
            .Where(pair => pair.Value > 1)
            .OrderByDescending(pair => pair.Value);
        PairFrequencies = new OrderedDictionary<Tuple<int, int>, int>(sortedPairFrequencies);

        var sortedMergeFrequencies = MergeFrequencies
            .Where(merge => merge.Value > 1)
            .OrderByDescending(merge => merge.Value);
        MergeFrequencies = new OrderedDictionary<int, int>(sortedMergeFrequencies);
    }

    public int? MostFrequentMerge()
    {
        if (MergeFrequencies.Count == 0)
        {
            return null;
        }

        return MergeFrequencies.First().Key;
    }

    public Tuple<int, int>? MostFrequentPair()
    {
        var mostFrequentPair = PairFrequencies.First();

        if (mostFrequentPair.Value == 1)
        {
            return null;
        }

        return mostFrequentPair.Key;
    }

    public void AddMerge(int mergeValue, int mergeIndex)
    {
        if (MergeFrequencies.TryGetValue(mergeValue, out _))
        {
            MergeFrequencies[mergeValue]++;
        }
        else
        {
            MergeFrequencies[mergeValue] = 1;
        }

        if (MergeIndexes.TryGetValue(mergeValue, out var _))
        {
            MergeIndexes[mergeValue].Add(mergeIndex);
        }
        else
        {
            MergeIndexes[mergeValue] = [mergeIndex];
        }
    }

    public int? AddPair(int pairValue, int pairValueNext, int pairIndex)
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
            return PairIndexes[pair].Last();
        }
        else
        {
            PairIndexes[pair] = [pairIndex];
            return null;
        }
    }

    public void RemovePair(int pairValue, int pairValueNext, int pairIndex)
    {
        var pair = new Tuple<int, int>(pairValue, pairValueNext);

        if (PairFrequencies.TryGetValue(pair, out _))
        {
            PairFrequencies[pair]--;
        }

        // if (PairIndexes.TryGetValue(pair, out var _))
        // {
        //     PairIndexes[pair].Remove(pairIndex);
        // }
    }
}