using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public abstract class CommaDelimitedListEvaluator : BaseEvaluator
    {
        protected CommaDelimitedListEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        protected async Task<(EvalResult Result, string Message, BotToken Token)> EvalCommaDelimitedList<TEvaluator>(ProgramState input, BotTokenType listEndToken, CancellationToken ct)
            where TEvaluator : IScriptEvaluator
        {
            var firstParam = true;
            while (!input.Match(listEndToken))
            {
                if (input.Expect(BotTokenType.EOF))
                    return UnexpectedTokenError(input.Current(), BotTokenType.EOF);

                if (!firstParam && !input.Expect(BotTokenType.Comma))
                    return Error(input.Current(), BotTokenType.Comma);

                while (input.Expect(BotTokenType.NewLine)) ;

                var parameterExpression = await Evaluator<TEvaluator>().EvaluateAsync(input, ct);
                if (parameterExpression.Result != EvalResult.Ok)
                    return parameterExpression;

                while (input.Expect(BotTokenType.NewLine)) ;

                firstParam = false;
            }

            if (input.OperationStack.Count == 0)
                return StackEmptyError(input.Current());

            var rightToken = input.OperationStack.Pop();
            if (rightToken.TokenType != listEndToken)
                return StackTokenError(listEndToken, rightToken);

            return Success();
        }

        protected static List<VariableBotToken> GetParametersFromStack(ProgramState input, BotTokenType listStartToken)
        {
            var parameters = new List<VariableBotToken>();
            while (input.OperationStack.Count > 0 && input.OperationStack.Peek().TokenType != listStartToken)
            {
                // parameters are popped in reverse order
                var parameter = (VariableBotToken)input.OperationStack.Pop();
                parameters.Insert(0, parameter);
            }
            return parameters;
        }

        protected static List<(IdentifierBotToken, VariableBotToken)> GetAssignmentPairsFromStack(ProgramState input, BotTokenType listStartToken)
        {
            var parameters = new List<(IdentifierBotToken, VariableBotToken)>();
            while (input.OperationStack.Count > 0 && input.OperationStack.Peek().TokenType != listStartToken)
            {
                // parameters are popped in reverse order
                var parameter = (VariableBotToken)input.OperationStack.Pop();

                var assignOp = input.OperationStack.Pop();
                if (assignOp.TokenType != BotTokenType.AssignOperator)
                    throw new BotScriptErrorException("Expected assignment token in object initialized", assignOp);

                var target = (IdentifierBotToken)input.OperationStack.Pop();

                parameters.Insert(0, (target, parameter));
            }
            return parameters;
        }
    }
}
