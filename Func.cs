using klang;
abstract class Callable
{
  public abstract object Call(Interpreter i, List<Expr> args);

}
class Function(
  List<Statement> statements,
  List<Expr.Ident> parameters,
  Environment closure
  ) : Callable()
{
  public int arity = parameters.Count;
  public override object Call(Interpreter interpreter, List<Expr> args)
  {
    if (args.Count != arity)
    {
      Console.WriteLine($"Expected {arity} arguments, but got {args.Count}");
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
  public Function Bind(Instance instance)
  {
    Environment env = new(closure);
    env.Set("this", instance);
    return new Function(statements, parameters, env);
  }
}

class Class(Token ident, List<Statement> statements) : Callable
{
  public override object Call(Interpreter interpreter, List<Expr> args)
  {
    Environment savedEnv = interpreter.environment;
    interpreter.environment = new(savedEnv);
    Dictionary<string, object> properties = [];
    Function? init = null;
    foreach (Statement stm in statements)
    {
      if (stm is Statement.FuncDecl funcDecl)
      {
        Function fn = new(funcDecl.statements, funcDecl.args, interpreter.environment);
        if (funcDecl.name.lit == ident.lit) init = fn;
        else
          properties.Add(funcDecl.name.lit, fn);
        continue;
      }
      else if (stm is Statement.VarDecl varDecl)
      {
        properties.Add(varDecl.token.lit, varDecl.expr.Evaluate(interpreter));
        continue;
      }

      throw new RuntimeException($"Unknown statement {stm}");
    }

    Instance instance = new(this, properties, interpreter.environment);
    if (init is not null)
    {
      if (args.Count != init.arity)
      {
        Console.WriteLine($"Expected {init.arity} arguments, but got {args.Count}");
      }
      init.Bind(instance);
      init.Call(interpreter, args);
    }
    interpreter.environment = savedEnv;
    return instance;
  }
}

class Instance(Class klass, Dictionary<string, object> properties, Environment env)
{
  Class klass = klass;
  Dictionary<string, object> properties = properties;
  public object Get(string ident)
  {
    bool has = properties.TryGetValue(ident, out object value);
    if (!has) throw new RuntimeException($"no {ident} property in {klass}");
    if (value is Function fn)
      return fn.Bind(this);
    return value;
  }

  public void Set(string ident, object value)
  {
    if (properties.ContainsKey(ident))
      throw new RuntimeException($"property with key '{ident}' already exists");
    properties.Add(ident, value);
  }
}