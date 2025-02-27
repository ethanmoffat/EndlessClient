using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.States
{
    public class ForeachEvaluator : BlockEvaluator
    {
        public ForeachEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            if (!input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_FOREACH))
                return (EvalResult.NotMatch, string.Empty, input.Current());

            if (!input.ExpectPair(BotTokenType.Keyword, BotTokenType.LParen))
                return (EvalResult.Failed, "Missing keyword and lparen to start foreach evaluation", input.Current());

            var (result, reason, token) = await GetVariableToken(input, ct);
            if (result != EvalResult.Ok)
                return (result, reason, token);

            var targetVariable = (IdentifierBotToken)token;
            if (targetVariable.Member != null || targetVariable.ArrayIndex != null)
                return (EvalResult.Failed, "foreach iteration must be assigned to simple identifier", targetVariable);

            if (!input.Current().Is(BotTokenType.Keyword, BotTokenParser.KEYWORD_IN))
                return Error(input.Current(), BotTokenType.Keyword);
            input.Expect(BotTokenType.Keyword);

            (result, reason, token) = await GetVariableToken(input, ct);
            if (result != EvalResult.Ok)
                return (result, reason, token);

            (result, reason, token) = input.SymbolTable.ResolveIdentifier((IdentifierBotToken)token);
            if (result != EvalResult.Ok)
                return (result, reason, token);

            if (token is not VariableBotToken collectionVariable ||
                collectionVariable.VariableValue is not ArrayVariable arrayVariable)
            {
                return (EvalResult.Failed, "foreach iteration must be over an array", token);
            }

            if (!input.Expect(BotTokenType.RParen))
                return Error(input.Current(), BotTokenType.RParen);

            var blockStartIndex = input.ExecutionIndex;

            for (int i = 0; i < arrayVariable.Value.Count; i++)
            {
                try
                {
                    input.SymbolTable[targetVariable.TokenValue] = (true, arrayVariable.Value[i]);

                    var blockEval = await EvaluateBlockAsync(input, ct);
                    if (blockEval.Item1 == EvalResult.ControlFlow)
                    {
                        if (IsBreak(input)) break;
                    }
                    else if (blockEval.Item1 != EvalResult.Ok)
                    {
                        return blockEval;
                    }
                }
                finally
                {
                    input.SymbolTable.Remove(targetVariable.TokenValue);
                }

                input.Goto(blockStartIndex);
            }

            if (result == EvalResult.Ok)
            {
                SkipBlock(input);
            }

            return (result, reason, token);
        }

        private async Task<(EvalResult, string, BotToken)> GetVariableToken(ProgramState input, CancellationToken ct)
        {
            var (res, reason, token) = await Evaluator<VariableEvaluator>().EvaluateAsync(input, ct);
            if (res != EvalResult.Ok)
                return (res, reason, token);

            if (input.OperationStack.Peek() is not IdentifierBotToken)
                return (EvalResult.Failed, "Expected identifier token in foreach expression", input.OperationStack.Peek());

            return Success(input.OperationStack.Pop());
        }
    }
}
