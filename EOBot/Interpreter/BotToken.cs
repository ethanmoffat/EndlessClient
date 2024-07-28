namespace EOBot.Interpreter;

public class BotToken
{
    public BotTokenType TokenType { get; }

    public string TokenValue { get; }

    public int LineNumber { get; }

    public int Column { get; }

    public BotToken(BotTokenType tokenType, string tokenValue, int line, int col)
    {
        TokenType = tokenType;
        TokenValue = tokenValue;
        LineNumber = line;
        Column = col;
    }

    public override string ToString() => $"{TokenType}: {TokenValue}";
}