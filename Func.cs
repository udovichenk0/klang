using klang;

class Function(List<Statement> statements, List<Expr.Ident> parameters)
{
  public object Call(Interpreter interpreter, List<Expr> args)
  {
    if (args.Count != parameters.Count)
    {
      Console.WriteLine($"Expected {parameters.Count} arguments, but got {args.Count}");
    }
    Environment env = new(interpreter.environment);
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
      if (env.enclosing is Environment environment)
        interpreter.environment = environment;
    }
    return "nil";
  }
}