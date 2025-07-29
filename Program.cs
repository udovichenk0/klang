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
      if (l.error)
      {
        Console.WriteLine("Error happened");
        return;
      }
      Parser p = new Parser(l);
      Expr expr = p.Parse();
      object result = expr.Interpret();
      Console.WriteLine(result);
    }
    catch (IOException e)
    {
      Console.WriteLine($"Could not read file: {path}");
      Console.WriteLine(e.Message);
    }
  }
}
