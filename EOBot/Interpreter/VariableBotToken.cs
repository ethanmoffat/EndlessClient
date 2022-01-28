using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter
{
    public class VariableBotToken : BotToken
    {
        public IVariable VariableValue { get; }

        public VariableBotToken(BotTokenType tokenType, string tokenValue, IVariable variableValue)
            : base(tokenType, tokenValue)
        {
            VariableValue = variableValue;
        }
    }
}
