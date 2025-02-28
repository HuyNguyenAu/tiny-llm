using System.Diagnostics;

public class Benchmark
{
    public Dictionary<string, double> Results { get; } = [];

    public void Measure(string task, Action action)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        action();

        stopwatch.Stop();

        if (Results.ContainsKey(task))
        {
            Results[task] += stopwatch.ElapsedMilliseconds / 1000d;
        }
        else
        {
            Results.Add(task, stopwatch.ElapsedMilliseconds / 1000d);
        }
    }

    public void PrintResults()
    {
        Console.WriteLine("Benchmark results:");
        Console.WriteLine("|{0,10}|{1,10}|{2,10}|", "Tasks", "Seconds", "Percentage");
        Console.WriteLine("|{0,10}|{1,10}|{2,10}|", "----------", "----------", "----------");

        var totalTime = Results.Values.Sum();

        foreach (var (task, time) in Results)
        {
            Console.WriteLine("|{0,10}|{1,10}|{2,10}|", task, string.Format($"{time:#.##}"), string.Format($"{time / totalTime * 100:#.##}%"));
        }

         Console.WriteLine("|{0,10}|{1,10}|{2,10}|", "Total", string.Format($"{totalTime:#.##}"), string.Empty);
    }
}