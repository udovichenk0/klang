using System.Reflection;

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
    if (Match([TokenType.CurlyOpen])) return blockStatement();
    if (Match([TokenType.If])) return conditionStatement();
    if (Match([TokenType.While])) return whileStatement();
    if (Match([TokenType.For])) return forStatement();
    if (Match([TokenType.Print])) return printStatement();
    if (Match([TokenType.Var])) return varDeclStatement();
    return expressionStatement();
  }
  Statement conditionStatement()
  {
    l.getToken();
    Expect(TokenType.ParenOpen);
    Expr condition = expression();
    Expect(TokenType.ParenClose);
    Statement ifStm = statement();
    Statement? elseStm = null;
    if (Match([TokenType.Else]))
    {
      l.getToken();
      elseStm = statement();
    }

    return new Statement.Condition(condition, ifStm, elseStm);
  }
  Statement whileStatement()
  {
    l.getToken();
    Expect(TokenType.ParenOpen);
    Expr cond = expression();
    Expect(TokenType.ParenClose);
    Statement body = statement();
    return new Statement.Loop(null, cond, null, body);
  }
  Statement forStatement()
  {
    l.getToken();
    Expect(TokenType.ParenOpen);
    Statement? init = null;
    Expr? cond = null;
    Expr? action = null;
    if (Match([TokenType.Semicolon])) l.getToken();
    else init = varDeclStatement();
    if (Match([TokenType.Semicolon])) l.getToken();
    else
    {
      cond = expression();
      Expect(TokenType.Semicolon);
    }
    if (Match([TokenType.ParenClose])) l.getToken();
    else
    {
      action = expression();
      Expect(TokenType.ParenClose);
    }

    Statement body = statement();
    return new Statement.Loop(init, cond, action, body);
  }
  Statement expressionStatement()
  {
    Expr expr = expression();
    Expect(TokenType.Semicolon);
    return new Statement.Expression(expr);
  }
  Statement blockStatement()
  {
    l.getToken();
    List<Statement> statements = [];
    while (IsEnd() || !Match([TokenType.CurlyClose]))
    {
      statements.Add(statement());
    }
    Expect(TokenType.CurlyClose);
    return new Statement.Block(statements);
  }
  Statement varDeclStatement()
  {
    l.getToken();
    string name = l.token.lit;
    Expect(TokenType.Ident);
    Expr? expr = null;
    if (Match([TokenType.Equal]))
    {
      l.getToken(); // skip equal token
      expr = expression();
    }
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
    return assignment();
  }
  Expr assignment()
  {
    Expr ident = or();
    if (Match([TokenType.Equal]))
    {
      l.getToken(); // skip operator
      if (ident is Expr.Ident)
      {
        Expr expr = or();
        return new Expr.Assign((Expr.Ident)ident, expr);
      }
      throw new ParseException("invalid assignment target");
    }
    return ident;
  }
  Expr or()
  {
    Expr leftExpr = and();
    if (Match([TokenType.Or]))
    {
      Token op = l.token;
      l.getToken();
      Expr rightExpr = and();
      return new Expr.Logical(leftExpr, op, rightExpr);
    }
    return leftExpr;
  }
  Expr and()
  {
    Expr leftExpr = equality();
    while (Match([TokenType.And]))
    {
      Token op = l.token;
      l.getToken();
      Expr rightExpr = equality();
      return new Expr.Logical(leftExpr, op, rightExpr);
    }
    return leftExpr;
  }
  Expr equality()
  {
    Expr leftExpr = temp();
    if (Match([
      TokenType.Less,
      TokenType.More,
      TokenType.LessEqual,
      TokenType.MoreEqual,
      TokenType.EqualEqual,
    ]))
    {
      Token op = l.token;
      l.getToken(); // skip operator
      Expr rightExpr = temp();
      return new Expr.Binary(leftExpr, op, rightExpr);
    }
    return leftExpr;
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
    Expr left = primary();
    if (Match([TokenType.PlusPlus, TokenType.MinusMinus]))
    {
      Token op = l.token;
      l.getToken(); // skip operator
      if (left is Expr.Ident) return new Expr.Unary(op, left);
      throw new ParseException($"can't do '{op.lit}' operation to right value");
    }
    return left;
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
      case TokenType.Nil:
        return new Expr.Primary(token);
      case TokenType.Ident:
        return new Expr.Ident(token);
      case TokenType.ParenOpen:
        Expr expr = expression();
        Expect(TokenType.ParenClose);
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