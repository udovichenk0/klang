
public enum TokenType
{
  Number,
  String,
  Plus,
  Minus,
  Div,
  Mul,
  ParenOpen,
  ParenClose,
  Not,
  EOL,
  Ident,
  True,
  False,
  Semicolon,
  Print,
  Var,
  Equal,
  Nil,
  More,
  Less,
  MoreEqual,
  LessEqual,
  EqualEqual,
  CurlyOpen,
  CurlyClose,
  If,
  Else,
  Or,
  And,
  While,
  For
}

public struct Token
{
  public required TokenType type;
  public required string lit;
}
class Lexer
{
  Dictionary<string, TokenType> keywords = new()
  {
    {"true", TokenType.True },
    {"false", TokenType.False },
    {"print", TokenType.Print },
    {"var", TokenType.Var },
    {"nil", TokenType.Nil },
    {"if", TokenType.If },
    {"else", TokenType.Else },
    {"or", TokenType.Or },
    {"and", TokenType.And },
    {"while", TokenType.While },
    {"for", TokenType.For },
  };
  string source;
  int pos;
  int start;
  public bool error;
  public Token token;
  public bool eof;

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
    if (IsEnd())
    {
      return;
    }
    RemoveSpaces();
    start = pos;

    char c = Next();
    switch (c)
    {
      case '+':
        setToken(TokenType.Plus);
        return;
      case '-':
        setToken(TokenType.Minus);
        return;
      case '*':
        setToken(TokenType.Mul);
        return;
      case '/':
        setToken(TokenType.Div);
        return;
      case '(':
        setToken(TokenType.ParenOpen);
        return;
      case ')':
        setToken(TokenType.ParenClose);
        return;
      case '{':
        setToken(TokenType.CurlyOpen);
        return;
      case '}':
        setToken(TokenType.CurlyClose);
        return;
      case ';':
        setToken(TokenType.Semicolon);
        return;
      case '!':
        setToken(TokenType.Not);
        return;
      case '=':
        if (Peek() == '=')
        {
          setToken(TokenType.EqualEqual);
          Next();
        }
        else setToken(TokenType.Equal);
        return;
      case '>':
        if (Peek() == '=')
        {
          setToken(TokenType.MoreEqual);
          Next();
        }
        else setToken(TokenType.More);
        return;
      case '<':
        if (Peek() == '=')
        {
          setToken(TokenType.LessEqual);
          Next();
        }
        else setToken(TokenType.Less);
        return;
      case '"':
        start += 1;
        while (!IsEnd() && Peek() != '"')
        {
          Next();
        }
        setToken(TokenType.String);
        Expect('"');
        return;
    }
    if (char.IsNumber(c))
    {
      string lit = c.ToString();
      while (char.IsNumber(Peek()))
      {
        lit += Next();
      }
      setToken(TokenType.Number);
      return;
    }
    if (IsAlpha(c))
    {
      while (IsAlpha(Peek()))
      {
        Next();
      }
      bool isKeyword = keywords.TryGetValue(source[start..pos], out TokenType type);
      if (isKeyword)
      {
        setToken(type);
        return;
      }
      setToken(TokenType.Ident);
      return;
    }
    Console.WriteLine($"unexpected token {(byte)c}");
    error = true;
  }

  void RemoveSpaces()
  {
    while (Peek() == ' ' || Peek() == '\r' || Peek() == '\n' || Peek() == '\x01')
    {
      Next();
    }
  }
  char Peek()
  {
    if (IsEnd())
      return '\x00';
    return source[pos];
  }
  bool IsAlpha(char c)
  {
    return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
  }
  void Expect(char c)
  {
    if (Peek() == c)
    {
      Next();
      return;
    }
    throw new ExpectException();
  }
  char Next()
  {
    if (IsEnd()) return '\x01';
    pos++;
    return source[pos - 1];
  }
  bool IsEnd()
  {
    if (pos == source.Length)
    {
      eof = true;
      return true;
    }
    return false;
  }
}

class ExpectException : Exception { }