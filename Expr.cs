abstract class Expr
{
  abstract public object Interpret();
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

    public override object Interpret()
    {
      object leftValue = left.Interpret();
      object rightValue = right.Interpret();
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
      }
      throw new RuntimeException($"unknown operation: {op.lit}");
    }
  }
  public class Primary : Expr
  {
    public Token token;
    public Primary(Token token)
    {
      this.token = token;
    }
    public override object Interpret()
    {
      if (token.type == TokenType.True) return true;
      if (token.type == TokenType.False) return false;
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
    public override object Interpret()
    {
      object val = expr.Interpret();
      switch (op.type)
      {
        case TokenType.Not:
          if (val is string)
          {
            if (((string)val).Length == 0) return true;
            return false;
          }
          else if (val is double)
          {
            if ((double)val == 0) return true;
            return false;
          }
          else if (val is bool)
          {
            if ((bool)val)
            {
              return false;
            }
            return true;
          }
          break;
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
    public override object Interpret()
    {
      return expr.Interpret();
    }
  }
  // public class Ident : Expr
  // {
  //   public Token token;
  //   public Ident(Token token)
  //   {
  //     this.token = token;
  //   }
  // }
}

class RuntimeException : Exception
{
  public RuntimeException(string msg) : base(msg) { }
}