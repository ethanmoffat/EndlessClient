using System.Linq;

namespace EOBot.Interpreter.Extensions
{
    public static class BotTokenExtensions
    {
        public static bool Is(this BotToken token, BotTokenType expectedType, string expectedValue = null)
        {
            return expectedValue == null
                ? token.TokenType == expectedType
                : token.TokenType == expectedType && token.TokenValue == expectedValue;
        }

        public static bool IsOneOf(this BotToken token, params BotTokenType[] expectedTypes)
        {
            return expectedTypes.Contains(token.TokenType);
        }

        public static bool IsUnary(this BotToken token)
        {
            return token != null && token.Is(BotTokenType.NotOperator);
        }

        public static bool IsBinary(this BotToken token)
        {
            return token != null && token.IsOneOf(
                BotTokenType.EqualOperator, BotTokenType.NotEqualOperator,
                BotTokenType.GreaterThanOperator, BotTokenType.LessThanOperator,
                BotTokenType.GreaterThanEqOperator, BotTokenType.LessThanEqOperator,
                BotTokenType.LogicalAndOperator, BotTokenType.LogicalOrOperator,
                BotTokenType.PlusOperator, BotTokenType.MinusOperator,
                BotTokenType.MultiplyOperator, BotTokenType.DivideOperator,
                BotTokenType.ModuloOperator
            );
        }
    }
}
