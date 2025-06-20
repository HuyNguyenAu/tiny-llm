namespace tiny_llm.src
{
    public record TrainingStep
    {
        public required int Iterations { get; init; }
        public required int MintedToken { get; init; }
        public required Tuple<int, int> Pair { get; init; }
        public required int TokensCount { get; init; }
        public required int MergeTokensCount { get; init; }
        public required int MintedTokensCount { get; init; }
        public required double CompressionRatio { get; init; }
        public required double TimeElapsedSeconds { get; init; }
    }
}