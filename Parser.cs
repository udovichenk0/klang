
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
  Lexer l;
  public Parser(Lexer lexer)
  {
    l = lexer;
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
    l.getToken();
    Expr expr = factor();

    while (Match([
      TokenType.Minus,
      TokenType.Plus,
    ]))
    {
      Token op = l.token;
      l.getToken();
      Expr rightExpr = factor();
      expr = new Expr.Binary(expr, op, rightExpr);
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
      Token op = l.token;
      l.getToken();
      Expr rightExpr = unary();
      expr = new Expr.Binary(expr, op, rightExpr);
    }
    return expr;
  }
  Expr unary()
  {
    if (Match([TokenType.Not, TokenType.Minus]))
    {
      Token op = l.token;
      l.getToken();
      Expr right = unary();
      return new Expr.Unary(op, right);
    }
    return primary();
  }
  Expr primary()
  {
    Token token = l.token;
    l.getToken();
    switch (token.type)
    {
      case TokenType.Number:
        return new Expr.Primary(token);
    }
    throw new UnknownPrimaryException("lol");
  }

  bool IsEnd()
  {
    return l.token.type == TokenType.EOL;
  }
  bool Match(TokenType[] types)
  {
    if (IsEnd()) return false;
    Token token = l.token;
    foreach (TokenType type in types)
    {
      if (type == token.type)
      {
        return true;
      }
    }
    return false;
  }
}

class UnknownPrimaryException : Exception
{
  public UnknownPrimaryException(string message) { }
}
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