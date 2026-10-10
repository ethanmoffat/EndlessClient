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
        [TestCase("func F($x) {\n    if ($x > 0) {\n        return 1\n    }\n    return 2\n}\n$res = F(5)", "1")]
        [TestCase("func F($x) {\n    if ($x > 0) {\n        return 1\n    }\n    return 2\n}\n$res = F(0)", "2")]
        [TestCase("func F() {\n    $i = 0\n    while (true) {\n        $i++\n        if ($i == 3) {\n            return $i\n        }\n    }\n    return -1\n}\n$res = F()", "3")]
        [TestCase("func F() {\n    for ($i = 0; $i < 10; $i++) {\n        if ($i == 4) {\n            return $i\n        }\n    }\n    return -1\n}\n$res = F()", "4")]
        [TestCase("func F($arr) {\n    foreach ($v in $arr) {\n        if ($v > 1) {\n            return $v\n        }\n    }\n    return 0\n}\n$res = F([1, 2, 3])", "2")]
        [TestCase("func F($x) {\n    if ($x == 0) {\n        return 0\n    } else if ($x == 1) {\n        return 1\n    } else {\n        return 2\n    }\n    return 3\n}\n$res = F(1)", "1")]
        [TestCase("func F($x) {\n    if ($x == 0) {\n        return 0\n    } else if ($x == 1) {\n        return 1\n    } else {\n        return 2\n    }\n    return 3\n}\n$res = F(5)", "2")]
        [TestCase("func F() {\n    while (true) {\n        while (true) {\n            return 7\n        }\n    }\n}\n$res = F()", "7")]
        public async Task Return_ExitsFunctionImmediately(string script, string expected)
        {
            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        [Test]
        public async Task Return_SkipsRemainingStatements()
        {
            var (state, result, reason) = await RunAsync("$res = 0\nfunc F() {\n    return 1\n    $res = 99\n}\n$x = F()");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("0"));
            Assert.That(state.SymbolTable["x"].Identifiable.StringValue, Is.EqualTo("1"));
        }

        [Test]
        public async Task Return_InsideForeach_RestoresIterationVariable()
        {
            var (state, result, reason) = await RunAsync("$v = 9\nfunc F() {\n    foreach ($v in [1, 2]) {\n        return $v\n    }\n}\n$res = F()");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("1"));
            Assert.That(state.SymbolTable["v"].Identifiable.StringValue, Is.EqualTo("9"));
        }

        [TestCase("break")]
        [TestCase("continue")]
        public async Task LoopControl_OutsideLoopInFunction_Fails(string keyword)
        {
            var (_, result, reason) = await RunAsync($"func F() {{\n    {keyword}\n}}\n$x = F()");

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain("outside of a loop"));
        }

        [Test]
        public async Task Break_InElseIf_SkipsElseBlock()
        {
            var script = "$i = 0\n$hit = 0\nwhile (true) {\n    $i++\n    if ($i == 0) {\n        $hit = 1\n    } else if ($i == 1) {\n        break\n    } else {\n        $hit = 2\n    }\n}";

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["hit"].Identifiable.StringValue, Is.EqualTo("0"));
        }

        [TestCase("$i = 0\nwhile ($i < 3) $i++\n$res = $i", "3")]
        [TestCase("while (false) $x = 1\n$res = 5", "5")]
        [TestCase("$sum = 0\nforeach ($v in [1, 2]) $sum += $v\n$res = $sum", "3")]
        [TestCase("foreach ($v in []) $x = 1\n$res = 5", "5")]
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
