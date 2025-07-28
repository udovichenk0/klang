
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
  EOL
}

struct Token
{
  public required TokenType type;
  public required string lit;
}
class Lexer
{

  string source;
  int pos;
  int start;
  public Token token;

  public Lexer(string Source)
  {
    source = Source;
  }


  void setToken(TokenType type)
  {
    token = new Token() { type = type, lit = source[start..pos] };
  }
  public void getToken()
  {
    RemoveSpaces();
    start = pos;

    char c = Next();

    switch (c)
    {
      case '+':
        setToken(TokenType.Plus);
        break;
      case '-':
        setToken(TokenType.Minus);
        break;
      case '*':
        setToken(TokenType.Mul);
        break;
      case '/':
        setToken(TokenType.Div);
        break;
      case '(':
        setToken(TokenType.OpenParen);
        break;
      case '!':
        setToken(TokenType.Not);
        break;
      case ')':
        setToken(TokenType.CloseParen);
        break;
    }
    if (char.IsNumber(c))
    {
      string lit = c.ToString();
      while (char.IsNumber(Peek()))
      {
        lit += Next();
      }
      setToken(TokenType.Number);
    }
  }

  void RemoveSpaces()
  {
    while (Peek() == ' ')
    {
      Next();
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