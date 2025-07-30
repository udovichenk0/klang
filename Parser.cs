class Parser
{
  Lexer l;
  public List<Statement> statements = new();
  public Parser(Lexer lexer)
  {
    l = lexer;
  }

  public List<Statement> Parse()
  {
    l.getToken();
    try
    {
      while (!l.eof)
      {
        statements.Add(statement());
      }
      return statements;
    }
    catch (ParseException e)
    {
      Console.WriteLine(e.ToString());
      throw;
    }
  }
  Statement statement()
  {
    if (Match([TokenType.Print])) return printStatement();
    if (Match([TokenType.Var])) return varDeclStatement();
    return expressionStatement();
  }
  Statement expressionStatement()
  {
    Expr expr = expression();
    Expect(TokenType.Semicolon);
    return new Statement.Expression(expr);
  }
  Statement varDeclStatement()
  {
    l.getToken();
    string name = l.token.lit;
    Expect(TokenType.Ident);
    Expect(TokenType.Equal);
    Expr expr = expression();
    Expect(TokenType.Semicolon);
    return new Statement.VarDecl(name, expr);
  }
  Statement printStatement()
  {
    l.getToken();
    Expr expr = expression();
    Expect(TokenType.Semicolon);
    return new Statement.Print(expr);
  }
  Expr expression()
  {
    return temp();
  }
  Expr temp()
  {
    Expr expr = factor();
    while (Match([
      TokenType.Minus,
      TokenType.Plus,
    ]))
    {
      Token op = l.token;
      l.getToken(); // skip operator
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
      l.getToken(); // skip operator
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
      l.getToken(); // skip operator
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
      case TokenType.String:
      case TokenType.True:
      case TokenType.False:
        return new Expr.Primary(token);
      case TokenType.Ident:
        return new Expr.Ident(token);
      case TokenType.OpenParen:
        Expr expr = expression();
        Expect(TokenType.CloseParen);
        return new Expr.Group(expr);
    }
    throw new ParseException($"unknown expression '{token.lit}'");
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
  void Expect(TokenType type)
  {
    if (type == l.token.type)
    {
      l.getToken();
      return;
    }
    throw new ParseException($"expects token: '{type}', got: '{l.token.type}'");
  }
}

class ParseException : Exception
{
  public ParseException(string message) : base(message) { }
}

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
    else if (expr is Expr.Group)
    {
      Expr.Group group = (Expr.Group)expr;
      return $"({WalkTree(group.expr)})";
    }
    else if (expr is Expr.Ident)
    {
      Expr.Ident ident = (Expr.Ident)expr;
      return ident.token.lit;
    }
    else
    {
      return ((Expr.Primary)expr).token.lit;
    }
  }
}