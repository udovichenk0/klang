using System.Data.Common;

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
    if (Match([TokenType.CurlyOpen])) return BlockStatement();
    if (Match([TokenType.If])) return ConditionStatement();
    if (Match([TokenType.While])) return WhileStatement();
    if (Match([TokenType.Return])) return ReturnStatement();
    if (Match([TokenType.Func])) return FuncDeclStatement();
    if (Match([TokenType.For])) return ForStatement();
    if (Match([TokenType.Print])) return PrintStatement();
    if (Match([TokenType.Var])) return VarDeclStatement();
    return ExpressionStatement();
  }
  Statement.Condition ConditionStatement()
  {
    l.getToken();
    Expect(TokenType.ParenOpen);
    Expr condition = Expression();
    Expect(TokenType.ParenClose);
    if (Match([TokenType.Var, TokenType.Func]))
    {
      throw new ParseException("Embedded statement cannot be a declaration");
    }

    Statement ifStm = statement();

    Statement? elseStm = null;
    if (Match([TokenType.Else]))
    {
      l.getToken();
      elseStm = statement();
    }

    return new Statement.Condition(condition, ifStm, elseStm);
  }
  Statement.Loop WhileStatement()
  {
    l.getToken();
    Expect(TokenType.ParenOpen);
    Expr cond = Expression();
    Expect(TokenType.ParenClose);
    if (Match([TokenType.Var, TokenType.Func]))
    {
      throw new ParseException("Embedded statement cannot be a declaration");
    }
    Statement body = statement();
    return new Statement.Loop(null, cond, null, body);
  }
  Statement.Loop ForStatement()
  {
    l.getToken();
    Expect(TokenType.ParenOpen);
    Statement? init = null;
    Expr? cond = null;
    Expr? action = null;
    if (Match([TokenType.Semicolon])) l.getToken();
    else init = VarDeclStatement();
    if (Match([TokenType.Semicolon])) l.getToken();
    else
    {
      cond = Expression();
      Expect(TokenType.Semicolon);
    }
    if (Match([TokenType.ParenClose])) l.getToken();
    else
    {
      action = Expression();
      Expect(TokenType.ParenClose);
    }

    if (Match([TokenType.Var, TokenType.Func]))
    {
      throw new ParseException("Embedded statement cannot be a declaration");
    }

    Statement body = statement();
    return new Statement.Loop(init, cond, action, body);
  }
  Statement.Expression ExpressionStatement()
  {
    Expr expr = Expression();
    Expect(TokenType.Semicolon);
    return new Statement.Expression(expr);
  }
  Statement.Block BlockStatement()
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
  Statement.VarDecl VarDeclStatement()
  {
    l.getToken();
    Token token = l.token;
    Expect(TokenType.Ident);
    Expr? expr = null;
    if (Match([TokenType.Equal]))
    {
      l.getToken(); // skip equal token
      expr = Expression();
    }
    Expect(TokenType.Semicolon);
    return new Statement.VarDecl(token, expr);
  }
  Statement FuncDeclStatement()
  {
    l.getToken();
    Token name = l.token;
    Expect(TokenType.Ident);
    Expect(TokenType.ParenOpen);
    List<Expr.Ident> args = [];
    if (!Match([TokenType.ParenClose]))
    {
      Expr expr = Expression();
      if (expr is Expr.Ident firstParam)
        args.Add(firstParam);
      else throw new ParseException("params must be valid identificators");
      while (Match([TokenType.Coma]))
      {
        l.getToken();
        expr = Expression();
        if (expr is Expr.Ident param)
          args.Add(param);
      }
    }
    Expect(TokenType.ParenClose);

    int savedPos = l.pos;
    Expect(TokenType.CurlyOpen);
    l.pos = savedPos;
    return new Statement.FuncDecl(name, args, BlockStatement().statements);
  }
  Statement.Print PrintStatement()
  {
    l.getToken();
    Expr expr = Expression();
    Expect(TokenType.Semicolon);
    return new Statement.Print(expr);
  }
  Statement.Return ReturnStatement()
  {
    l.getToken();
    Expr? value = null;
    if (!Match([TokenType.Semicolon]))
    {
      value = Expression();
    }
    Expect(TokenType.Semicolon);
    return new Statement.Return(value);
  }
  Expr Expression()
  {
    return Assignment();
  }
  Expr Assignment()
  {
    Expr ident = Or();
    if (Match([TokenType.Equal]))
    {
      l.getToken(); // skip operator
      if (ident is Expr.Ident)
      {
        Expr expr = Or();
        return new Expr.Assign((Expr.Ident)ident, expr);
      }
      throw new ParseException("invalid assignment target");
    }
    return ident;
  }
  Expr Or()
  {
    Expr leftExpr = And();
    if (Match([TokenType.Or]))
    {
      Token op = l.token;
      l.getToken();
      Expr rightExpr = And();
      return new Expr.Logical(leftExpr, op, rightExpr);
    }
    return leftExpr;
  }
  Expr And()
  {
    Expr leftExpr = Equality();
    while (Match([TokenType.And]))
    {
      Token op = l.token;
      l.getToken();
      Expr rightExpr = Equality();
      return new Expr.Logical(leftExpr, op, rightExpr);
    }
    return leftExpr;
  }
  Expr Equality()
  {
    Expr leftExpr = Temp();
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
      Expr rightExpr = Temp();
      return new Expr.Binary(leftExpr, op, rightExpr);
    }
    return leftExpr;
  }
  Expr Temp()
  {
    Expr expr = Factor();
    while (Match([
      TokenType.Minus,
      TokenType.Plus,
    ]))
    {
      Token op = l.token;
      l.getToken(); // skip operator
      Expr rightExpr = Factor();
      expr = new Expr.Binary(expr, op, rightExpr);
    }
    return expr;
  }
  Expr Factor()
  {
    Expr expr = Unary();
    while (Match([
      TokenType.Div,
      TokenType.Mul,
    ]))
    {
      Token op = l.token;
      l.getToken(); // skip operator
      Expr rightExpr = Unary();
      expr = new Expr.Binary(expr, op, rightExpr);
    }
    return expr;
  }
  Expr Unary()
  {
    if (Match([TokenType.Not, TokenType.Minus]))
    {
      Token op = l.token;
      l.getToken(); // skip operator
      Expr right = Call();
      return new Expr.Unary(op, right);
    }
    Expr left = Call();
    if (Match([TokenType.PlusPlus, TokenType.MinusMinus]))
    {
      Token op = l.token;
      l.getToken(); // skip operator
      if (left is Expr.Ident) return new Expr.Unary(op, left);
      throw new ParseException($"can't do '{op.lit}' operation to right value");
    }
    return left;
  }
  Expr Call()
  {
    Expr primary = Primary();
    if (Match([TokenType.ParenOpen]))
    {
      l.getToken();
      List<Expr> args = [];
      if (!Match([TokenType.ParenClose]))
      {
        args.Add(Expression());
        while (Match([TokenType.Coma]))
        {
          l.getToken();
          args.Add(Expression());
        }
      }
      Expect(TokenType.ParenClose);
      Expr.Ident var = (Expr.Ident)primary;
      return new Expr.Call(var.name, args);
    }
    return primary;
  }
  Expr Primary()
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
        Expr expr = Expression();
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
      return ident.name.lit;
    }
    else
    {
      return ((Expr.Primary)expr).token.lit;
    }
  }
}