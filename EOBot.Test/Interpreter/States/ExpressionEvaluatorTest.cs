using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter;
using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;
using NUnit.Framework;

namespace EOBot.Test.Interpreter.States
{
    [TestFixture]
    public class ExpressionEvaluatorTest
    {
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.LogicalExpressions))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.ArithmeticExpressions))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.UnaryMinusExpressions))]
        public async Task TestExpressionEvaluation(string input, string expected)
        {
            await TestScriptEvaluation($"$test_res = {input}", expected);
        }

        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.UnaryMinus))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.ShortCircuit))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.StringConcatenation))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.Modulo))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.Ternary))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.IncrementDecrement))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.ObjectInitializer))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.DictInitializer))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.Foreach))]
        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.ArrayLiterals))]
        public async Task TestScriptEvaluation(string input, string expected)
        {

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            await botInterpreter.Run(state, CancellationToken.None);

            Assert.That(state.SymbolTable["test_res"].Identifiable.StringValue, Is.EqualTo(expected).IgnoreCase);
        }

        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.DictLookupConcatenation))]
        public async Task TestStringConcatenationWithDictLookup(string input, string expected)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            state.SymbolTable["d"] = (false, new DictVariable(new Dictionary<string, IVariable>
            {
                ["s"] = new StringVariable("a"),
                ["n"] = new IntVariable(1),
            }));
            await botInterpreter.Run(state, CancellationToken.None);

            Assert.That(state.SymbolTable["test_res"].Identifiable.StringValue, Is.EqualTo(expected).IgnoreCase);
        }

        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.DivisionByZero))]
        public async Task TestDivisionByZero(string input)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            var (result, reason, _) = await ScriptEvaluator.Instance.EvaluateAsync(state, CancellationToken.None);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain("Division by zero"));
        }

        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.ObjectInitializerWithoutAssignment))]
        public async Task TestObjectInitializerRequiresAssignment(string input)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            var (result, reason, _) = await ScriptEvaluator.Instance.EvaluateAsync(state, CancellationToken.None);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain(nameof(BotTokenType.AssignOperator)));
        }

        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.ObjectInitializerDuplicateMember))]
        public async Task TestObjectInitializerDuplicateMember(string input)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            var (result, reason, _) = await ScriptEvaluator.Instance.EvaluateAsync(state, CancellationToken.None);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain("Duplicate member"));
        }

        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.DictInitializerErrors))]
        public async Task TestDictInitializerErrors(string input, string expectedReason)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            var (result, reason, _) = await ScriptEvaluator.Instance.EvaluateAsync(state, CancellationToken.None);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain(expectedReason));
        }

        [Test]
        public async Task TestDictMissingKeyReadDoesNotInsert()
        {
            const string script = """
                $d = ["a": 1]
                $x = $d["b"]
                $y = $d["c"] + ""
                """;

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(script));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            var (result, reason, _) = await ScriptEvaluator.Instance.EvaluateAsync(state, CancellationToken.None);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["x"].Identifiable, Is.InstanceOf<UndefinedVariable>());
            Assert.That(((DictVariable)state.SymbolTable["d"].Identifiable).Value.Keys, Is.EquivalentTo(new[] { "a" }));
        }

        [TestCaseSource(typeof(ExpressionEvaluatorScripts), nameof(ExpressionEvaluatorScripts.DictMissingKeyWrite))]
        public async Task TestDictMissingKeyWriteInserts(string statement, string expected)
        {
            var script = $$"""
                $d = ["a": 1]
                {{statement}}
                """;

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(script));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            var (result, reason, _) = await ScriptEvaluator.Instance.EvaluateAsync(state, CancellationToken.None);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(((DictVariable)state.SymbolTable["d"].Identifiable).Value["b"].StringValue, Is.EqualTo(expected));
        }
    }
}
