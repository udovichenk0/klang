
using System.Linq.Expressions;
using System.Security.AccessControl;

class Expr
{
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
  }
  public class Primary : Expr
  {
    public Token token;
    public Primary(Token token)
    {
      this.token = token;
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
  }
}
class Parser
{
  List<Token> tokens;
  int pos;
  public Parser(List<Token> tokens)
  {
    this.tokens = tokens;
    expression();
  }

  void expression()
  {
    Expr expr = temp();
    Printer printer = new Printer(expr);
    printer.Print();
  }
  Expr temp()
  {
    Expr expr = factor();

    while (Match([
      TokenType.Minus,
      TokenType.Plus,
    ]))
    {
      Token operation = Prev();
      Expr rightExpr = factor();
      expr = new Expr.Binary(expr, operation, rightExpr);
    }
    return expr;
  }
  Expr factor()
  {
    Expr expr = unary();
    while (Match([
      TokenType.Div,
      TokenType.Mul,
    ]))
    {
      Token operation = Prev();
      Expr rightExpr = unary();
      expr = new Expr.Binary(expr, operation, rightExpr);
    }
    return expr;
  }
  Expr unary()
  {
    if (Match([TokenType.Not, TokenType.Minus]))
    {
      Token op = Prev();
      Expr right = unary();
      return new Expr.Unary(op, right);
    }
    return primary();
  }
  Expr primary()
  {
    Token token = Next();
    switch (token.type)
    {
      case TokenType.Number:
        return new Expr.Primary(token);
    }
    throw new UnknownPrimaryException();
  }
  Token Next()
  {
    if (IsEnd())
    {
      throw new ZeroTokenLeftException();
    }
    pos++;
    return tokens[pos - 1];
  }
  Token Peek()
  {
    if (IsEnd())
    {
      throw new ZeroTokenLeftException();
    }
    return tokens[pos];
  }
  bool IsEnd()
  {
    return pos >= tokens.Count;
  }
  bool Match(TokenType[] types)
  {
    if (IsEnd()) return false;
    Token token = Peek();
    foreach (TokenType type in types)
    {
      if (type == token.type)
      {
        Next();
        return true;
      }
    }
    return false;
  }
  Token Prev()
  {
    if (pos <= 0)
    {
      return tokens[0];
    }
    return tokens[pos - 1];
  }
}

class UnknownPrimaryException : Exception { }
class ZeroTokenLeftException : Exception { }

class Printer
{
  Expr expr;
  public Printer(Expr expr)
  {
    this.expr = expr;
  }
  public void Print()
  {
    if (expr is not null)
    {
      string str = WalkTree(expr);
      Console.WriteLine(str);
    }
  }
  public string WalkTree(Expr expr)
  {
    if (expr is Expr.Binary)
    {
      Expr.Binary binary = (Expr.Binary)expr;
      string left = WalkTree(binary.left);
      string operation = binary.op.lit;
      string right = WalkTree(binary.right);
      return $"({left} {operation} {right})";
    }
    else if (expr is Expr.Unary)
    {
      Expr.Unary unary = (Expr.Unary)expr;
      return $"({unary.op.lit}{WalkTree(unary.expr)})";
    }
    else
    {
      return ((Expr.Primary)expr).token.lit;
    }
  }
}