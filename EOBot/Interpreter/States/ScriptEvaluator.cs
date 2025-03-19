using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Extensions;

namespace EOBot.Interpreter.States
{
    public class ScriptEvaluator : BaseEvaluator
    {
        static ScriptEvaluator()
        {
            var evaluators = new List<IScriptEvaluator>();
            evaluators.Add(new StatementListEvaluator(evaluators));
            evaluators.Add(new StatementEvaluator(evaluators));
            evaluators.Add(new AssignmentEvaluator(evaluators));
            evaluators.Add(new KeywordEvaluator(evaluators));
            evaluators.Add(new LabelEvaluator());
            evaluators.Add(new FunctionEvaluator(evaluators));
            evaluators.Add(new VariableEvaluator(evaluators));
            evaluators.Add(new ExpressionEvaluator(evaluators));
            evaluators.Add(new ExpressionTailEvaluator(evaluators));
            evaluators.Add(new OperandEvaluator(evaluators));
            evaluators.Add(new IfEvaluator(evaluators));
            evaluators.Add(new WhileEvaluator(evaluators));
            evaluators.Add(new ForEvaluator(evaluators));
            evaluators.Add(new ForeachEvaluator(evaluators));
            evaluators.Add(new GotoEvaluator());
            evaluators.Add(new ReturnEvaluator(evaluators));
            Instance = new ScriptEvaluator(evaluators);
        }

        public static ScriptEvaluator Instance { get; }

        public ScriptEvaluator(IEnumerable<IScriptEvaluator> evaluators)
            : base(evaluators) { }

        public override async Task<(EvalResult, string, BotToken)> EvaluateAsync(ProgramState input, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return (EvalResult.Cancelled, string.Empty, null);

            var evalResult = await Evaluator<StatementListEvaluator>().EvaluateAsync(input, ct);
            if (evalResult.Result != EvalResult.Ok)
                return evalResult;

            if (!input.Expect(BotTokenType.EOF))
                return Error(input.Current(), BotTokenType.EOF);

            return Success();
        }
    }
}
