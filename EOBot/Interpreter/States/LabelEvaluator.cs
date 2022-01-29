using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class LabelEvaluator : IScriptEvaluator
    {
        public bool Evaluate(ProgramState input)
        {
            if (!input.MatchPair(BotTokenType.Identifier, BotTokenType.Colon))
                return false;

            if (input.OperationStack.Count < 2)
                return false;
            
            // pop the colon
            input.OperationStack.Pop();

            var labelToken = input.OperationStack.Pop();
            if (labelToken.TokenType != BotTokenType.Identifier)
                return false;

            input.Labels.Add(new LabelIdentifier(labelToken.TokenValue), input.ExecutionIndex);

            return true;
        }
    }
}