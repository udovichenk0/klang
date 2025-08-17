using System.Runtime.CompilerServices;
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

class Class(
  Token ident,
  List<Statement> statements,
  Dictionary<string, Function> methods,
  Class? superClass
 ) : Callable
{
  List<Statement> statements = statements;
  public Token ident = ident;
  Dictionary<string, Function> methods = methods;
  Class? superClass = superClass;

  public override object Call(Interpreter interpreter, List<Expr> args)
  {
    Dictionary<string, object> fields = [];
    foreach (Statement stm in statements)
    {
      if (stm is Statement.VarDecl varDecl)
        fields.Add(varDecl.token.lit, varDecl.expr.Evaluate(interpreter));
    }

    Instance instance = new(this, fields);

    Function? init = FindMethod(ident.lit);
    if (init is not null)
    {
      if (args.Count != init.arity)
      {
        Console.WriteLine($"Expected {init.arity} arguments, but got {args.Count}");
      }

      init.Bind(instance);
      init.Call(interpreter, args);
    }

    return instance;
  }
  public Function? FindMethod(string name)
  {
    methods.TryGetValue(name, out Function? fn);
    if (fn is not null) return fn;

    if (superClass is not null)
      return superClass.FindMethod(name);

    return null;
  }
}

class Instance(Class klass, Dictionary<string, object> fields)
{
  Class klass = klass;
  Dictionary<string, object> fields = fields;

  public object Get(string ident)
  {
    bool has = fields.TryGetValue(ident, out object? value);
    if (has && value is Function f)
    {
      return f.Bind(this);
    }
    if (has && value is not null) return value;

    Function? fn = klass.FindMethod(ident);
    if (fn is null) throw new RuntimeException($"Undefined property {ident} in {klass}");

    return fn.Bind(this);
  }

  public void Set(string ident, object value)
  {
    if (fields.ContainsKey(ident))
      throw new RuntimeException($"property with key '{ident}' already exists");
    fields.Add(ident, value);
  }
}