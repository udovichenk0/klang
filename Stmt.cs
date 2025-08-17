
using klang;

public abstract class Statement
{
  public abstract void Execute(Interpreter i);

  public class Expression(Expr expr) : Statement
  {
    public Expr expr = expr;
    public override void Execute(Interpreter i)
    {
      expr.Evaluate(i);
    }
  }


  public class Print(Expr expr) : Statement
  {
    public Expr expr = expr;
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
  public class VarDecl(Token ident, Expr? expr) : Statement
  {
    public Token token = ident;
    public Expr? expr = expr;
    public override void Execute(Interpreter i)
    {
      if (expr is not null)
      {
        object result = expr.Evaluate(i);
        i.environment.Set(token.lit, result);
        return;
      }
      i.environment.Set(token.lit, null);
    }
  }

  public class FuncDecl(Token ident, List<Expr.Ident> args, List<Statement> statements) : Statement
  {
    public Token name = ident;
    public List<Expr.Ident> args = args;
    public List<Statement> statements = statements;
    public override void Execute(Interpreter i)
    {
      Environment closure = i.environment;
      Function function = new(statements, args, closure);
      i.environment.Set(name.lit, function);
    }
  }

  public class ClassDecl(Token ident, List<Statement> statements, Expr.Ident? parent) : Statement
  {
    public Token ident = ident;
    public List<Statement> statements = statements;
    public Expr.Ident? superClass = parent;
    public override void Execute(Interpreter i)
    {
      i.environment.Set(ident.lit, null);
      Environment savedEnv = i.environment;
      Class? klass = null;
      if (superClass is not null)
      {
        object value = superClass.Evaluate(i);
        if (value is not Class superKlass)
          throw new ParseException("Superclass must be a class");
        klass = superKlass;
      }

      if (superClass is not null)
      {
        i.environment = new Environment(savedEnv);
        i.environment.Set("super", klass);
      }

      Dictionary<string, Function> methods = [];


      foreach (Statement stm in statements)
      {
        if (stm is FuncDecl funcDecl)
        {
          Function fn = new(funcDecl.statements, funcDecl.args, i.environment);
          methods.Add(funcDecl.name.lit, fn);
        }
      }

      if (superClass is not null)
        i.environment = savedEnv;

      i.environment.Assign(ident.lit, new Class(ident, statements, methods, klass));
    }

  }
  public class Loop(Statement? init, Expr? cond, Expr? action, Statement body) : Statement
  {
    public Statement? init = init;
    public Expr? cond = cond;
    public Expr? action = action;
    public Statement body = body;
    public override void Execute(Interpreter i)
    {
      Environment savedEnv = i.environment;
      i.environment = new Environment(savedEnv);
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

      i.environment = savedEnv;
    }
  }
  public class Return(Expr? expr) : Statement
  {
    public Expr? expr = expr;
    public override void Execute(Interpreter i)
    {
      if (expr is not null)
      {
        throw new ReturnException(expr.Evaluate(i));
      }
    }
  }
  public class Condition(Expr condition, Statement ifStatement, Statement? elseStatement) : Statement
  {
    public Expr condition = condition;
    public Statement ifStatement = ifStatement;
    public Statement? elseStatement = elseStatement;
    public override void Execute(Interpreter i)
    {
      object result = condition.Evaluate(i);
      bool isTruthy = condition.IsTruthy(result);
      if (isTruthy) ifStatement.Execute(i);
      else if (!isTruthy && elseStatement is not null) elseStatement.Execute(i);
    }
  }
}
class ReturnException(object value) : Exception
{
  public object? value = value;
}