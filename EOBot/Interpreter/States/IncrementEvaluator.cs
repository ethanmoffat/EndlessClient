using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    /// <summary>
    /// Evaluator for pre and postfix ++/-- operators
    /// </summary>
    public class IncrementEvaluator : AssignmentEvaluator
    {
        public IncrementEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return Task.FromResult((EvalResult.Cancelled, string.Empty, (BotToken)null));

            var top = input.OperationStack.Pop();
            var isPostfix = top.IsOneOf(BotTokenType.Increment, BotTokenType.Decrement);
            var (incrementOperator, target) = isPostfix
                ? (top, (IdentifierBotToken)input.OperationStack.Pop())     // [$x, ++]
                : (input.OperationStack.Pop(), (IdentifierBotToken)top);    // [++, $x]

            return Task.FromResult(Increment(input, target, incrementOperator, isPostfix));
        }

        private static (EvalResult, string, BotToken) Increment(ProgramState input, IdentifierBotToken target, BotToken incrementOperator, bool isPostfix)
        {
            var (result, reason, oldValue) = input.SymbolTable.ResolveIdentifier(target);
            if (result != EvalResult.Ok)
                return (result, reason, oldValue);

            (result, reason, var token) = Assign(input.SymbolTable, target, (VariableBotToken)oldValue, incrementOperator);
            if (result != EvalResult.Ok)
                return (result, reason, token);

            if (isPostfix)
            {
                input.OperationStack.Push(oldValue);
                return Success();
            }

            (result, reason, var newValue) = input.SymbolTable.ResolveIdentifier(target);
            if (result != EvalResult.Ok)
                return (result, reason, newValue);

            input.OperationStack.Push(newValue);
            return Success();
        }
    }
}
