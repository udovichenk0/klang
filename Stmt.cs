
using klang;

public abstract class Statement
{
  public abstract void Execute(Interpreter i);

  public class Expression(Expr expr) : Statement
  {
    public override void Execute(Interpreter i)
    {
      expr.Evaluate(i);
    }
  }
  public class Print(Expr expr) : Statement
  {
    public override void Execute(Interpreter i)
    {
      object result = expr.Evaluate(i);
      Console.WriteLine(result);
    }
  }
  public class Block(List<Statement> statements) : Statement
  {
    List<Statement> statements = statements;
    public override void Execute(Interpreter i)
    {
      Environment innerEnvironment = new(i.environment);
      i.environment = innerEnvironment;
      foreach (Statement statement in statements)
      {
        statement.Execute(i);
      }
    }
  }
  public class VarDecl(string ident, Expr? expr) : Statement
  {
    public override void Execute(Interpreter i)
    {
      if (expr is not null)
      {
        object result = expr.Evaluate(i);
        i.environment.Set(ident, result);
        return;
      }
      i.environment.Set(ident, null);
    }
  }
}