using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter
{
    public class VariableBotToken : BotToken
    {
        public IVariable VariableValue { get; }

        public override string TokenValue => ExplicitTokenValue ?? VariableValue.StringValue;

        private string ExplicitTokenValue => base.TokenValue;

        private VariableBotToken(BotTokenType tokenType, string tokenValue, IVariable variableValue)
            : base(tokenType, tokenValue, 0, 0)
        {
            VariableValue = variableValue;
        }

        private VariableBotToken(VariableBotToken source, BotToken position)
            : base(source.TokenType, source.ExplicitTokenValue, position.LineNumber, position.Column)
        {
            VariableValue = source.VariableValue;
        }

        /// <summary>
        /// Create a token using the token position of the provided BotToken
        /// </summary>
        public VariableBotToken WithPosition(BotToken position)
        {
            return new VariableBotToken(this, position);
        }

        /// <summary>
        /// Create a token for a computed value. The token value is rendered from the variable only when it is read.
        /// </summary>
        public static VariableBotToken FromComputedValue(BotTokenType tokenType, IVariable variableValue)
        {
            return new VariableBotToken(tokenType, null, variableValue);
        }

        /// <summary>
        /// Create a token with an explicit token value.
        /// </summary>
        public static VariableBotToken Create(BotTokenType tokenType, string tokenValue, IVariable variableValue)
        {
            return new VariableBotToken(tokenType, tokenValue, variableValue);
        }
    }
}
