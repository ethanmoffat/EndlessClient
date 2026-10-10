using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class OperandEvaluator : BaseEvaluator
    {
        public OperandEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            input.Match(BotTokenType.NotOperator);

            var evalRes = await EvaluateVariableOperandAsync(input, ct);
            if (evalRes.Result != EvalResult.NotMatch)
                return evalRes;

            evalRes = await Evaluator<EnumEvaluator>().EvaluateAsync(input, ct);
            if (evalRes.Result != EvalResult.NotMatch)
                return evalRes;

            var matchRes = input.MatchOneOf(BotTokenType.Literal, BotTokenType.TypeSpecifier);
            return matchRes ? Success() : (EvalResult.NotMatch, string.Empty, input.Current());
        }

        private async Task<(EvalResult Result, string Reason, BotToken Token)> EvaluateVariableOperandAsync(ProgramState input, CancellationToken ct)
        {
            var isPrefixIncrement = input.MatchOneOf(BotTokenType.Increment, BotTokenType.Decrement);

            var evalRes = await Evaluator<VariableEvaluator>().EvaluateAsync(input, ct);
            if (evalRes.Result != EvalResult.Ok)
                return isPrefixIncrement && evalRes.Result == EvalResult.NotMatch ? Error(input.Current(), BotTokenType.Variable) : evalRes;

            var isPostfixIncrement = input.MatchOneOf(BotTokenType.Increment, BotTokenType.Decrement);
            if (isPrefixIncrement || isPostfixIncrement)
                return await Evaluator<IncrementEvaluator>().EvaluateAsync(input, ct);

            return evalRes;
        }
    }
}
