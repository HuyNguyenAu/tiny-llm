using System.Diagnostics;

namespace tiny_llm.src
{
    public class Benchmark
    {
        public Dictionary<string, double> Results { get; } = [];
        public Stopwatch Stopwatch { get; } = new Stopwatch();

        public Benchmark()
        {
            Stopwatch.Start();
        }

        public void Measure(string task, Action action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            action();

            stopwatch.Stop();

            if (Results.ContainsKey(task))
            {
                Results[task] += stopwatch.ElapsedMilliseconds / 1000.0;
            }
            else
            {
                Results.Add(task, stopwatch.ElapsedMilliseconds / 1000.0);
            }
        }

        public void PrintResults()
        {
            Console.Write("Benchmark results");
            Console.WriteLine(" ----------------------------------------------------------\n");
            Console.WriteLine("|{0,10}|{1,10}|{2,10}|", "Tasks", "Seconds", "Percentage");
            Console.WriteLine("|{0,10}|{1,10}|{2,10}|", "----------", "----------", "----------");

            var totalTime = Results.Values.Sum();

            foreach (var (task, time) in Results)
            {
                Console.WriteLine("|{0,10}|{1,10}|{2,10}|", task, string.Format($"{time:#.###}"), string.Format($"{time / totalTime * 100:#.###}%"));
            }

            Console.WriteLine("|{0,10}|{1,10}|{2,10}|", "Total", string.Format($"{totalTime:#.###}"), string.Empty);
        }
    }
}