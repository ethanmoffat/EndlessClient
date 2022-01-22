namespace EOBot.Interpreter
{
    public enum BotTokenType
    {
        EOF,
        LParen,
        RParen,
        LBrace,
        RBrace,
        LBracket,
        RBracket,
        Keyword,
        Identifier,
        Label,
        Literal,
        AssignOperator,
        EqualOperator,
        LessThanOperator,
        LessThanEqOperator,
        GreaterThanOperator,
        GreaterThanEqOperator,
        NotEqualOperator,
        NewLine,
        Error
    }
}
