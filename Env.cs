public class Environment(Environment? environment)
{
  Dictionary<string, object?> vars = [];

  public Environment? enclosing = environment;

  public object Get(Token key)
  {
    bool isExist = vars.TryGetValue(key.lit, out object? value);
    if (isExist)
    {
      if (value is null) return "nil";
      return value;
    }
    if (enclosing is not null)
    {
      return enclosing.Get(key);
    }
    throw new RuntimeException($"undefined variable {key.lit}");
  }
  public object GetAt(Token ident, int distance)
  {
    Environment env = this;
    for (int i = 0; i < distance; i++)
    {
      if (env.enclosing is not null)
        env = env.enclosing;
    }
    return env.Get(ident);
  }

  public object GetFromGlobal(Token ident)
  {
    Environment env = this;
    while (env.enclosing is not null)
      env = env.enclosing;

    return env.Get(ident);
  }

  public void Set(string key, object? value)
  {
    bool isExist = vars.ContainsKey(key);
    if (isExist) throw new RuntimeException($"variable {key} already exists");

    vars.Add(key, value);
  }
  public void Assign(string key, object value)
  {
    if (vars.ContainsKey(key))
    {
      vars[key] = value;
      return;
    }
    if (enclosing is not null)
    {
      enclosing.Assign(key, value);
      return;
    }

    throw new RuntimeException($"undefined variable {key}");
  }
}