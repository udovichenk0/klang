using klang;

public abstract class Expr
{
  abstract public object Evaluate(Interpreter i);
  public class Binary : Expr
  {
    public Expr left;
    public Expr right;
    public Token op;
    public Binary(Expr left, Token op, Expr right)
    {
      this.left = left;
      this.op = op;
      this.right = right;
    }

    public override object Evaluate(Interpreter i)
    {
      object leftValue = left.Evaluate(i);
      object rightValue = right.Evaluate(i);
      bool isString = leftValue is string || rightValue is string;
      if (op.type == TokenType.Plus)
      {
        if (isString) return $"{leftValue}{rightValue}";
        if (leftValue is bool) leftValue = Convert.ToDouble(leftValue);
        if (rightValue is bool) rightValue = Convert.ToDouble(rightValue);
        return (double)leftValue + (double)rightValue;
      }
      if (isString) throw new RuntimeException($"operation '{op.lit}' not supported between string and number");
      if (leftValue is bool)
      {
        leftValue = Convert.ToDouble(leftValue);
      }
      if (rightValue is bool)
      {
        rightValue = Convert.ToDouble(rightValue);
      }
      switch (op.type)
      {
        case TokenType.Minus:
          return (double)leftValue - (double)rightValue;
        case TokenType.Mul:
          return (double)leftValue * (double)rightValue;
        case TokenType.Div:
          return (double)leftValue / (double)rightValue;
        case TokenType.Less:
          return (double)leftValue < (double)rightValue;
        case TokenType.More:
          return (double)leftValue > (double)rightValue;
        case TokenType.LessEqual:
          return (double)leftValue <= (double)rightValue;
        case TokenType.MoreEqual:
          return (double)leftValue >= (double)rightValue;
        case TokenType.EqualEqual:
          return (double)leftValue == (double)rightValue;
      }
      throw new RuntimeException($"unknown operation: {op.lit}");
    }
  }
  public class Logical(Expr left, Token op, Expr right) : Expr
  {
    public Expr left = left;
    public Expr right = right;
    public override object Evaluate(Interpreter i)
    {
      object eval = left.Evaluate(i);
      if (op.type == TokenType.Or)
      {
        if (IsTruthy(eval)) return eval;
        return right.Evaluate(i);
      }
      else
      {
        object leftEval = left.Evaluate(i);
        object rightEval = right.Evaluate(i);
        if (!IsTruthy(leftEval))
        {
          Console.WriteLine(leftEval);
          return leftEval;
        }
        if (!IsTruthy(rightEval)) return rightEval;
        return rightEval;
      }
      throw new NotImplementedException();
    }
  }
  public class Primary : Expr
  {
    public Token token;
    public Primary(Token token)
    {
      this.token = token;
    }
    public override object Evaluate(Interpreter i)
    {
      if (token.type == TokenType.True) return true;
      if (token.type == TokenType.False) return false;
      if (token.type == TokenType.Nil) return "nil";
      if (token.type == TokenType.Number) return Convert.ToDouble(token.lit);

      return token.lit;
    }
  }
  public class Unary : Expr
  {
    public Token op;
    public Expr expr;
    public Unary(Token op, Expr expr)
    {
      this.op = op;
      this.expr = expr;
    }
    public override object Evaluate(Interpreter i)
    {
      object val = expr.Evaluate(i);
      switch (op.type)
      {
        case TokenType.Not:
          return !IsTruthy(val);
        case TokenType.PlusPlus:
          if (val is double)
          {
            double newval = (double)val + 1;
            if (expr is not Ident)
              throw new RuntimeException($"increment target must be variable");
            Ident var = (Ident)expr;
            i.environment.Assign(var.name.lit, newval);
            return newval;
          }
          throw new RuntimeException($"can't increment non integer value '{val}'");
        case TokenType.MinusMinus:
          if (val is double)
          {
            double newval = (double)val - 1;
            if (expr is not Ident)
              throw new RuntimeException($"decrement target must be variable");
            Ident var = (Ident)expr;
            i.environment.Assign(var.name.lit, newval);
            return newval;
          }
          throw new RuntimeException($"can't decrement non integer value '{val}'");
        case
           TokenType.Minus:
          if (val is double)
          {
            return -(double)val;
          }
          throw new RuntimeException($"Can't apply negate operation to right value: '{val}'");
      }
      throw new RuntimeException("");
    }
  }
  public class Group : Expr
  {
    public Expr expr;
    public Group(Expr expr)
    {
      this.expr = expr;
    }
    public override object Evaluate(Interpreter i)
    {
      return expr.Evaluate(i);
    }
  }
  public class Ident : Expr
  {
    public Token name;
    public Ident(Token token)
    {
      this.name = token;
    }
    public override object Evaluate(Interpreter i)
    {
      return LookupVariable(name, i);
    }
  }
  object LookupVariable(Token ident, Interpreter i)
  {
    bool has = i.locals.TryGetValue(ident, out int distance);
    if (!has) return i.environment.GetFromGlobal(ident);
    return i.environment.GetAt(ident, distance);
  }
  public class Call(Expr expr, List<Expr> args) : Expr
  {
    public Expr callee = expr;
    public List<Expr> args = args;
    public override object Evaluate(Interpreter i)
    {
      object f = callee.Evaluate(i);
      if (f is not Callable callable) throw new RuntimeException($"{f} is not an instance");
      return callable.Call(i, args);
    }
  }
  public class Getter(Expr expr, Ident ident) : Expr
  {
    public Expr expr = expr;
    public Ident ident = ident;

    public override object Evaluate(Interpreter i)
    {
      object klassInstance = expr.Evaluate(i);
      if (klassInstance is not Instance instance)
        throw new RuntimeException($"can't access property of {klassInstance}");
      return instance.Get(ident.name.lit);
    }
  }
  public class Setter(Expr expr, Token ident, Expr value) : Expr
  {
    public override object Evaluate(Interpreter i)
    {
      object klassInstance = expr.Evaluate(i);
      if (klassInstance is not Instance instance)
        throw new RuntimeException($"can't access property of {klassInstance}");
      object v = value.Evaluate(i);
      instance.Set(ident.lit, v);
      return v;
    }
  }
  public class Assign(Ident ident, Expr expr) : Expr
  {
    public Ident ident = ident;
    public Expr expr = expr;
    public override object Evaluate(Interpreter i)
    {
      object value = expr.Evaluate(i);
      i.environment.Assign(ident.name.lit, value);
      return value;
    }
  }
  public class This(Token ident) : Expr
  {
    public Token ident = ident;
    public override object Evaluate(Interpreter i)
    {
      return i.environment.Get(ident);
    }
  }
  public bool IsTruthy(object value)
  {
    if (value is string v && v.Length > 0 && v != "nil") return true;
    else if (value is double v1 && v1 > 0) return true;
    else if (value is bool v2) return v2;
    return false;
  }
}

class RuntimeException : Exception
{
  public RuntimeException(string msg) : base(msg) { }
}