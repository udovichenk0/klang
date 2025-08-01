
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
    public List<Statement> statements = statements;
    public override void Execute(Interpreter i)
    {
      Environment innerEnvironment = new(i.environment);
      i.environment = innerEnvironment;
      foreach (Statement statement in statements)
      {
        statement.Execute(i);
      }
      if (innerEnvironment.enclosing is not null)
        i.environment = innerEnvironment.enclosing;
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
  public class FuncDecl(string ident, List<Expr.Ident> args, List<Statement> statements) : Statement
  {
    public string ident = ident;
    public List<Expr.Ident> args = args;
    public List<Statement> statements = statements;
    public override void Execute(Interpreter i)
    {
      Function function = new(statements, args);
      i.environment.Set(ident, function);
    }
  }
  public class Loop(Statement? init, Expr? cond, Expr? action, Statement body) : Statement
  {
    public override void Execute(Interpreter i)
    {
      init?.Execute(i);
      if (cond is not null)
      {
        object condEval = cond.Evaluate(i);
        while (cond.IsTruthy(condEval))
        {
          body.Execute(i);
          action?.Evaluate(i);
          condEval = cond.Evaluate(i);
        }
      }
      else
      {
        while (true)
        {
          body.Execute(i);
          action?.Evaluate(i);
        }
      }
    }
  }
  public class Condition(Expr condition, Statement ifStatement, Statement? elseStatement) : Statement
  {
    public override void Execute(Interpreter i)
    {
      object result = condition.Evaluate(i);
      bool isTruthy = condition.IsTruthy(result);
      if (isTruthy) ifStatement.Execute(i);
      else if (!isTruthy && elseStatement is not null) elseStatement.Execute(i);
    }
  }
}