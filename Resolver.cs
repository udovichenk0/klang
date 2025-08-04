using klang;

class Resolver(Interpreter i)
{
  List<Dictionary<Token, bool>> scopes = [];
  Interpreter interpreter = i;
  public void Resolve(List<Statement> statements)
  {
    foreach (Statement statement in statements)
    {
      ResolveStatement(statement);
    }
  }
  void ResolveStatement(Statement statement)
  {
    if (statement is Statement.Block block) ResolveBlock(block);
    if (statement is Statement.VarDecl vardecl) ResolveVarDecl(vardecl);
    if (statement is Statement.FuncDecl funcDecl) ResolveFuncDecl(funcDecl);
    if (statement is Statement.Print print) ResolvePrint(print);
    if (statement is Statement.Expression expr) ResolveExprStatement(expr);
    if (statement is Statement.Loop loop) ResolveLoop(loop);
    if (statement is Statement.Return returnStmt) ResolveReturn(returnStmt);
    if (statement is Statement.Condition condition) ResolveCondition(condition);
  }
  void ResolveBlock(Statement.Block block)
  {
    BeginScope();
    Resolve(block.statements);
    EndScope();
  }

  void ResolveVarDecl(Statement.VarDecl vardecl)
  {
    Declare(vardecl.token);
    if (vardecl.expr is not null)
      ResolveExpr(vardecl.expr);
    Define(vardecl.token);
  }

  void ResolveFuncDecl(Statement.FuncDecl funcDecl)
  {
    Define(funcDecl.name);

    BeginScope();

    foreach (Expr.Ident arg in funcDecl.args)
    {
      Declare(arg.name);
      Define(arg.name);
    }

    Resolve(funcDecl.statements);

    EndScope();
  }

  void ResolvePrint(Statement.Print print)
  {
    ResolveExpr(print.expr);
  }

  void ResolveExprStatement(Statement.Expression expr)
  {
    ResolveExpr(expr.expr);
  }

  void ResolveLoop(Statement.Loop loop)
  {
    BeginScope();

    if (loop.init is not null)
      ResolveStatement(loop.init);
    if (loop.cond is not null)
      ResolveExpr(loop.cond);
    if (loop.action is not null)
      ResolveExpr(loop.action);
    ResolveStatement(loop.body);

    EndScope();
  }
  void ResolveReturn(Statement.Return returnStmt)
  {
    if (returnStmt.expr is not null)
      ResolveExpr(returnStmt.expr);
  }
  void ResolveCondition(Statement.Condition condition)
  {
    ResolveExpr(condition.condition);
    ResolveStatement(condition.ifStatement);
    if (condition.elseStatement is not null)
      ResolveStatement(condition.elseStatement);
  }

  // Expression
  void ResolveExpr(Expr expr)
  {
    if (expr is Expr.Ident ident)
      ResolveLocal(ident.name);

    else if (expr is Expr.Call call)
    {
      ResolveLocal(call.name);
      foreach (Expr arg in call.args) ResolveExpr(arg);
    }

    else if (expr is Expr.Binary binary)
    {
      ResolveExpr(binary.left);
      ResolveExpr(binary.right);
    }
    else if (expr is Expr.Unary unary)
      ResolveExpr(unary.expr);
    else if (expr is Expr.Group group) ResolveExpr(group.expr);
    else if (expr is Expr.Assign assign) ResolveExpr(assign.expr);
    else if (expr is Expr.Logical logical)
    {
      ResolveExpr(logical.left);
      ResolveExpr(logical.right);
    }
  }



  void ResolveLocal(Token name)
  {
    for (int i = scopes.Count - 1; i >= 0; i--)
    {
      var scope = scopes[i];
      if (scope.ContainsKey(name))
      {
        interpreter.Resolve(name, i);
      }
    }
  }
  void BeginScope()
  {
    Dictionary<Token, bool> scope = [];
    scopes.Add(scope);
  }
  void EndScope()
  {
    scopes.RemoveAt(scopes.Count - 1);
  }
  Dictionary<Token, bool>? Peek()
  {
    return scopes.Count > 0 ? scopes.Last() : null;
  }
  void Declare(Token token)
  {
    Peek()?.Add(token, false);
  }
  void Define(Token token)
  {
    Dictionary<Token, bool>? scope = Peek();
    if (scope is not null)
    {
      scope[token] = true;
    }
  }
}