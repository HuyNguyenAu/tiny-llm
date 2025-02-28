using System.Text;

using var reader = new StreamReader("data/test_data_small.txt");
var text = reader.ReadToEnd();
var token = Encoding.UTF8.GetBytes(text).Select(Convert.ToInt32).ToArray();

var tokeniser = new Tokeniser(true);
tokeniser.Train(token, 10000);

// foreach (var step in trainingSteps)
// {
//     Console.WriteLine($"Iteration: {step.Iterations}");
//     Console.WriteLine($"Pair: {step.Pair}");
//     Console.WriteLine($"MintedToken: {step.MintedToken}");
//     Console.WriteLine($"TokensCount: {step.TokensCount}");
//     Console.WriteLine($"MergeTokensCount: {step.MergeTokensCount}");
//     Console.WriteLine($"MintedTokensCount: {step.MintedTokensCount}");
//     Console.WriteLine($"CompressionRatio: {step.CompressionRatio}");
//     Console.WriteLine($"Time Elapsed Seconds: {step.TimeElapsedSeconds}");
//     Console.WriteLine();
// }
