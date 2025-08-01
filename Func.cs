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
      env.Set(parameters[i].token.lit, args[i].Evaluate(interpreter));
    }
    foreach (Statement statement in statements)
    {
      statement.Execute(interpreter);
    }

    if (env.enclosing is Environment environment)
      interpreter.environment = environment;
    return "";
  }
}