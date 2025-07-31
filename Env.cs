public class Environment
{
  Dictionary<string, object?> vars = new();

  public object Get(string key)
  {
    bool isExist = vars.TryGetValue(key, out object? value);
    if (isExist)
    {
      if (value is null) return "nil";
      return value;
    }
    throw new RuntimeException($"undefined variable {key}");
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

    throw new RuntimeException($"undefined variable {key}");
  }
}