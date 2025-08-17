namespace klang;


public class Program
{
  static void Main()
  {
    string path = "./test.txt";

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

      Parser p = new(l);
      List<Statement> statements = p.Parse();

      Interpreter i = new(statements);
      Resolver r = new(i);
      r.Resolve(statements);
      i.Interpret();
    }
    catch (IOException e)
    {
      Console.WriteLine($"Could not read file: {path}");
      Console.WriteLine(e.Message);
    }
  }
}

public enum ClassType
{
  None,
  Class,
  SubClass,
}

public class Interpreter(List<Statement> stmts)
{

  public Environment environment = new(null);
  public Dictionary<Token, int> locals = [];
  public bool isInFunction = false;
  public ClassType ClassType = ClassType.None;

  public void Interpret()
  {
    foreach (var stmt in stmts)
    {
      stmt.Execute(this);
    }
  }
  public void Resolve(Token name, int depth)
  {
    locals.TryAdd(name, depth);
  }
}