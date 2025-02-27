public class Pair()
{
    public required int Value { get; set; }
    public required int ValueNext { get; set; }
    public required int Index { get; set; }
    public int? PreviousIndex { get; set; }
    public int? NextIndex { get; set; }
}