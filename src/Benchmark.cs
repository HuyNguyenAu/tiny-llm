using System.Diagnostics;

namespace tiny_llm.src
{
    internal class Benchmark
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

            var elapsedTimeSeconds = stopwatch.ElapsedMilliseconds / 1000.0;

            if (!Results.TryAdd(task, elapsedTimeSeconds))
            {
                Results[task] += elapsedTimeSeconds;
            }
        }

        public void PrintResults()
        {
            Console.Write("Benchmark results");
            Console.WriteLine(" ----------------------------------------------------------\n");
            Console.WriteLine("|{0,10}|{1,10}|{2,10}|", "Tasks", "Seconds", "Percentage");
            Console.WriteLine("|{0,10}|{1,10}|{2,10}|", "----------", "----------", "----------");

            var totalTime = Results.Values.Sum();
            var totalPercentage = 0.0;

            foreach (var (task, time) in Results)
            {
                var percentage = time / totalTime * 100;
                totalPercentage += percentage;

                Console.WriteLine("|{0,10}|{1,10}|{2,10}|", task, string.Format($"{time:#.###}"), string.Format($"{percentage:#.###}%"));
            }

            Console.WriteLine("|{0,10}|{1,10}|{2,10}|", "Total", string.Format($"{totalTime:#.###}"), string.Format($"{totalPercentage:#.###}%"));
        }
    }
}