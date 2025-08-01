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
        case TokenType.Minus:
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
    public Token token;
    public Ident(Token token)
    {
      this.token = token;
    }
    public override object Evaluate(Interpreter i)
    {
      return i.environment.Get(token.lit);
    }
  }
  public class Assign(Ident ident, Expr expr) : Expr
  {
    public Ident ident = ident;
    public override object Evaluate(Interpreter i)
    {
      object value = expr.Evaluate(i);
      i.environment.Assign(ident.token.lit, value);
      return value;
    }
  }
  public bool IsTruthy(object value)
  {
    if (value is string && ((string)value).Length > 0 && (string)value != "nil") return true;
    else if (value is double && ((double)value) > 0) return true;
    else if (value is bool) return (bool)value;
    return false;
  }
}

class RuntimeException : Exception
{
  public RuntimeException(string msg) : base(msg) { }
}