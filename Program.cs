using System.Text;
using tiny_llm.src;


using var reader = new StreamReader("C:\\Users\\Kaze\\source\\repos\\tiny-llm\\data\\test_data_small.txt");
var text = reader.ReadToEnd();
var tokens = Encoding.UTF8.GetBytes(text).Select(Convert.ToInt32).ToArray();

var tokeniserOptions = new TokeniserOptions
{
    ShowBenchmark = true,
    ShowDebug = true,
};
var tokeniser = new Tokeniser(tokeniserOptions);
var trainingSteps = tokeniser.Train(tokens, 100);
tokeniser.Decode(tokens);