﻿namespace EOBot.Interpreter
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
        Colon,
        Comma,
        Keyword,
        Variable,
        Identifier,
        Literal,
        AssignOperator,
        EqualOperator,
        LessThanOperator,
        LessThanEqOperator,
        GreaterThanOperator,
        GreaterThanEqOperator,
        NotEqualOperator,
        PlusOperator,
        MinusOperator,
        MultiplyOperator,
        DivideOperator,
        ModuloOperator,
        NotOperator,
        LogicalAndOperator,
        LogicalOrOperator,
        Dot,
        Semicolon,
        NewLine,
        Increment,
        Decrement,
        PlusEquals,
        MinusEquals,
        MultiplyEquals,
        DivideEquals,
        StrictEqualOperator,
        StrictNotEqualOperator,
        IsOperator,
        TypeSpecifier,
        Error,
    }
}
