using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter;
using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;
using NUnit.Framework;

namespace EOBot.Test.Interpreter
{
    [TestFixture]
    public class FunctionCallTest
    {
        [TestCaseSource(typeof(FunctionCallScripts), nameof(FunctionCallScripts.Recursion))]
        public async Task Recursion_EvaluatesEachCallIndependently(string script, string expected)
        {
            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        [Test]
        public async Task Recursion_ExceedingMaxCallDepth_Fails()
        {
            const string script = """
                func F($n) {
                    return F($n + 1)
                }
                $res = F(0)
                """;

            var (_, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain("Maximum call depth"));
        }

        [Test]
        public async Task Recursion_AtMaxCallDepth_Succeeds()
        {
            var script = $$"""
                func F($n) {
                    if ($n == {{UserDefinedFunction.MaxCallDepth}}) {
                        return $n
                    }
                    return F($n + 1)
                }
                $res = F(1)
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo($"{UserDefinedFunction.MaxCallDepth}"));
        }

        [TestCaseSource(typeof(FunctionCallScripts), nameof(FunctionCallScripts.NestedFunctions))]
        public async Task NestedFunction_IsCallableInsideEnclosingFunction(string script, string expected)
        {
            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        [Test]
        public async Task NestedFunction_IsNotVisibleAfterEnclosingFunctionReturns()
        {
            const string script = """
                func Outer() {
                    func Inner() {
                        return 2
                    }
                    return Inner()
                }
                $x = Outer()
                $res = Inner()
                """;

            var (state, result, _) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(state.SymbolTable.ContainsKey("Inner"), Is.False);
        }

        [TestCaseSource(typeof(FunctionCallScripts), nameof(FunctionCallScripts.NoReturn))]
        public async Task FunctionWithoutReturn_ReturnsUndefined(string script)
        {
            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable, Is.SameAs(UndefinedVariable.Instance));
        }

        [TestCaseSource(typeof(FunctionCallScripts), nameof(FunctionCallScripts.VoidUsedAsValue))]
        public async Task VoidFunction_UsedAsValue_Fails(string script)
        {
            var (_, result, reason) = await RunAsync(script, Setup);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain("Function 'Void' must return a value when used in an expression"));
        }

        [Test]
        public async Task VoidFunction_AsStatement_Succeeds()
        {
            var (_, result, reason) = await RunAsync("Void()", Setup);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
        }

        [Test]
        public async Task UndefinedArgument_IsPassedToFunction()
        {
            const string script = """
                func F($a) {
                    return $a
                }
                $res = F(undefined)
                $res2 = Echo(undefined)
                """;

            var (state, result, reason) = await RunAsync(script, Setup);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable, Is.SameAs(UndefinedVariable.Instance));
            Assert.That(state.SymbolTable["res2"].Identifiable.StringValue, Is.EqualTo(UndefinedVariable.Instance.StringValue));
        }

        [Test]
        public async Task InvalidArgumentType_Fails()
        {
            var (_, result, reason) = await RunAsync("$res = Upper(undefined)", Setup);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain("Invalid argument type calling function 'Upper'"));
        }

        private static void Setup(ProgramState state)
        {
            state.SymbolTable["Void"] = (true, new VoidFunction("Void", () => { }));
            state.SymbolTable["Echo"] = (true, new Function<object, string>("Echo", x => x.ToString()));
            state.SymbolTable["Upper"] = (true, new Function<string, string>("Upper", x => x.ToUpper()));
        }

        private static async Task<(ProgramState State, EvalResult Result, string Reason)> RunAsync(string script, Action<ProgramState> setup = null)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(script));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            setup?.Invoke(state);

            var (result, reason, _) = await ScriptEvaluator.Instance.EvaluateAsync(state, CancellationToken.None);
            return (state, result, reason);
        }
    }
}
