using System.Text;

using var reader = new StreamReader("data/linux.txt");
var text = reader.ReadToEnd();
var tokens = Encoding.UTF8.GetBytes(text).Select(Convert.ToInt32).ToArray();

var tokeniser = new Tokeniser(false, true);
tokeniser.Train(tokens, 100000);