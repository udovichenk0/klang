namespace klang;

public class Program
{
  static void Main()
  {
    string path = "./test";
    try
    {
      using StreamReader reader = new(path);
      string source = reader.ReadToEnd();

      Lexer l = new Lexer(source);
      l.Parse();

    }
    catch (IOException e)
    {
      Console.WriteLine($"Could not read file: {path}");
      Console.WriteLine(e.Message);
    }
  }
}
