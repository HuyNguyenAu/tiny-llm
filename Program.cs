using System.Text;
using Newtonsoft.Json;

using var reader = new StreamReader("data/test_data_large.txt");
var text = reader.ReadToEnd();
var tokens = Encoding.UTF8.GetBytes(text).Select(Convert.ToInt32).ToArray();

var tokeniser = new Tokeniser(false, true);
var trainingSteps = tokeniser.Train(tokens, 100);

Console.WriteLine(JsonConvert.SerializeObject(trainingSteps, Formatting.Indented));