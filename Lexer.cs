
enum TokenType
{
  Number,
  Plus,
  Minus,
  Div,
  Mul,
  OpenParen,
  CloseParen,
  Not,
}


struct Token
{
  public required TokenType type;
  public required string lit;
}
class Lexer
{

  string source;
  public List<Token> tokens = [];
  int pos;
  int start;

  public Lexer(string Source)
  {
    source = Source;
  }
  void addToken(TokenType type)
  {
    Token token = new Token()
    {
      type = type,
      lit = source[start..pos],
    };
    tokens.Add(token);
  }
  public void Parse()
  {
    while (!IsEnd())
    {
      start = pos;
      char c = Next();
      switch (c)
      {
        case ' ':
          continue;
        case '+':
          addToken(TokenType.Plus);
          continue;
        case '-':
          addToken(TokenType.Minus);
          continue;
        case '*':
          addToken(TokenType.Mul);
          continue;
        case '/':
          addToken(TokenType.Div);
          continue;
        case '(':
          addToken(TokenType.OpenParen);
          continue;
        case '!':
          addToken(TokenType.Not);
          continue;
        case ')':
          addToken(TokenType.CloseParen);
          continue;
      }
      if (char.IsNumber(c))
      {
        string lit = c.ToString();
        while (char.IsNumber(Peek()))
        {
          lit += Next();
        }
        addToken(TokenType.Number);
        continue;
      }
    }
  }

  char Peek()
  {
    if (IsEnd()) return '\x00';
    return source[pos];
  }


  char Next()
  {
    if (IsEnd()) return '\x00';
    pos++;
    return source[pos - 1];
  }

  bool IsEnd()
  {
    return pos == source.Length;
  }
}