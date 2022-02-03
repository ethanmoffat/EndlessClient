using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EOBot.Interpreter.States
{
    public class VariableEvaluator : BaseEvaluator
    {
        public VariableEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input)
        {
            if (!input.Match(BotTokenType.Variable))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            int? arrayIndex = null;

            if (input.Expect(BotTokenType.LBracket))
            {
                var expressionEval = await Evaluator<ExpressionEvaluator>().EvaluateAsync(input);
                if (expressionEval.Result != EvalResult.Ok)
                    return expressionEval;

                // todo: expression_tail might need to be checked here before the RBracket...
                if (!input.Expect(BotTokenType.RBracket))
                    return Error(input.Current(), BotTokenType.RBracket);

                if (input.OperationStack.Count == 0)
                    return StackEmptyError(input.Current());
                var expressionResult = (VariableBotToken)input.OperationStack.Pop();

                if (!(expressionResult.VariableValue is IntVariable))
                    return (EvalResult.Failed, $"Expected index to be int, but it was {expressionResult.VariableValue.GetType().Name}", expressionResult);

                arrayIndex = ((IntVariable)expressionResult.VariableValue).Value;
            }

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());
            var identifier = input.OperationStack.Pop();

            input.OperationStack.Push(new IdentifierBotToken(BotTokenType.Variable, identifier.TokenValue, identifier.LineNumber, identifier.Column, arrayIndex));

            return Success();
        }
    }
}
