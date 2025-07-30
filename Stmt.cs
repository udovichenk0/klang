
using System.Data.Common;
using System.IO.Pipelines;
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