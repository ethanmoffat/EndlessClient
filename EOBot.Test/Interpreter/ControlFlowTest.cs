using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter;
using EOBot.Interpreter.States;
using NUnit.Framework;

namespace EOBot.Test.Interpreter
{
    [TestFixture]
    public class ControlFlowTest
    {
        [TestCaseSource(typeof(ControlFlowScripts), nameof(ControlFlowScripts.Return))]
        public async Task Return_ExitsFunctionImmediately(string script, string expected)
        {
            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        [Test]
        public async Task Return_SkipsRemainingStatements()
        {
            const string script = """
                $res = 0
                func F() {
                    return 1
                    $res = 99
                }
                $x = F()
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("0"));
            Assert.That(state.SymbolTable["x"].Identifiable.StringValue, Is.EqualTo("1"));
        }

        [Test]
        public async Task Return_InsideForeach_RestoresIterationVariable()
        {
            const string script = """
                $v = 9
                func F() {
                    foreach ($v in [1, 2]) {
                        return $v
                    }
                }
                $res = F()
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("1"));
            Assert.That(state.SymbolTable["v"].Identifiable.StringValue, Is.EqualTo("9"));
        }

        [TestCase("break")]
        [TestCase("continue")]
        public async Task LoopControl_OutsideLoopInFunction_Fails(string keyword)
        {
            var script = $$"""
                func F() {
                    {{keyword}}
                }
                $x = F()
                """;

            var (_, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain("outside of a loop"));
        }

        [Test]
        public async Task Break_InElseIf_SkipsElseBlock()
        {
            const string script = """
                $i = 0
                $hit = 0
                while (true) {
                    $i++
                    if ($i == 0) {
                        $hit = 1
                    } else if ($i == 1) {
                        break
                    } else {
                        $hit = 2
                    }
                }
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["hit"].Identifiable.StringValue, Is.EqualTo("0"));
        }

        [TestCaseSource(typeof(ControlFlowScripts), nameof(ControlFlowScripts.BracelessLoop))]
        public async Task BracelessLoop_FollowedByStatement(string script, string expected)
        {
            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        private static async Task<(ProgramState State, EvalResult Result, string Reason)> RunAsync(string script)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(script));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            var (result, reason, _) = await ScriptEvaluator.Instance.EvaluateAsync(state, CancellationToken.None);
            return (state, result, reason);
        }
    }
}
