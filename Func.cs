using klang;
abstract class Callable
{
  public abstract object Call(Interpreter i, List<Expr> args);
}
class Function(
  List<Statement> statements,
  List<Expr.Ident> parameters,
  Environment closure
  ) : Callable
{
  public override object Call(Interpreter interpreter, List<Expr> args)
  {
    if (args.Count != parameters.Count)
    {
      Console.WriteLine($"Expected {parameters.Count} arguments, but got {args.Count}");
    }
    var savedEnv = interpreter.environment;
    Environment env = new(closure);
    interpreter.environment = env;
    for (int i = 0; i < args.Count; i++)
    {
      env.Set(parameters[i].name.lit, args[i].Evaluate(interpreter));
    }
    try
    {
      foreach (Statement statement in statements)
      {
        statement.Execute(interpreter);
      }
    }
    catch (ReturnException e)
    {
      if (e.value is null) return "nil";
      return e.value;
    }
    finally
    {
      interpreter.environment = savedEnv;
    }
    return "nil";
  }
}

class Class(Token ident, List<Statement> statements) : Callable
{
  public override object Call(Interpreter interpreter, List<Expr> args)
  {
    Environment savedEnv = interpreter.environment;
    Environment env = new(savedEnv);
    interpreter.environment = env;
    foreach (Statement stm in statements)
    {
      stm.Execute(interpreter);
    }

    foreach (Statement stm in statements)
    {
      if (stm is Statement.FuncDecl method && method.name.lit == ident.lit)
      {
        Callable val = (Callable)interpreter.environment.Get(ident);
        val.Call(interpreter, args);
      }
    }

    interpreter.environment.Get(ident);

    interpreter.environment = savedEnv;
    return "nil";
  }
}