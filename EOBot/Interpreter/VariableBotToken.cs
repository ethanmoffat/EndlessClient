using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter
{
    public class VariableBotToken : BotToken
    {
        public IVariable VariableValue { get; }

        public VariableBotToken(BotTokenType tokenType, string tokenValue, IVariable variableValue)
            : base(tokenType, tokenValue, 0, 0)
        {
            VariableValue = variableValue;
        }

        private VariableBotToken(VariableBotToken source, BotToken position)
            : base(source.TokenType, source.TokenValue, position.LineNumber, position.Column)
        {
            VariableValue = source.VariableValue;
        }

        public VariableBotToken WithPosition(BotToken position)
        {
            return new VariableBotToken(this, position);
        }
    }
}
