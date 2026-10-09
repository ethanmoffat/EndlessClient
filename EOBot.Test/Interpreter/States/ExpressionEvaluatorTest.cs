using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter;
using NUnit.Framework;

namespace EOBot.Test.Interpreter.States
{
    [TestFixture]
    public class ExpressionEvaluatorTest
    {
        [TestCase("true && false", "false")]
        [TestCase("true || false", "true")]
        [TestCase("!true", "false")]
        [TestCase("!false", "true")]
        [TestCase("!true && !false", "false")]
        [TestCase("!(true || false)", "false")]
        [TestCase("!true || !false", "true")]
        [TestCase("!(true && false)", "true")]
        [TestCase("2 / 2 + 5 * 4 - 3", "18")]
        [TestCase("2 + 2 / 5 - 4 * 3", "-10")]
        [TestCase("5 * 4 - 3", "17")]
        [TestCase("5 - 4 * 3", "-7")]
        [TestCase("8 / 8 + 4", "5")]
        [TestCase("8 + 8 / 4", "10")]
        [TestCase("5 - (2 * 4) + 4 - (8 / 2)", "-3")]
        [TestCase("2 * (3) + 1", "7")]
        [TestCase("!(false) && false", "false")]
        [TestCase("-4", "-4")]
        [TestCase("-4 + 1", "-3")]
        [TestCase("1 + -4", "-3")]
        [TestCase("1 - -4", "5")]
        [TestCase("1-4", "-3")]
        [TestCase("2 * -3", "-6")]
        [TestCase("-2 * 3", "-6")]
        [TestCase("-(2 + 3)", "-5")]
        [TestCase("-(2) + 3", "1")]
        [TestCase("- -4", "4")]
        [TestCase("- - -4", "-4")]
        [TestCase("!!true", "true")]
        [TestCase("-4 < 0", "true")]
        public async Task TestExpressionEvaluation(string input, string expected)
        {
            await TestScriptEvaluation($"$test_res = {input}", expected);
        }

        [TestCase("$x = 3\n$test_res = -$x", "-3")]
        [TestCase("$x = 3\n$test_res = 10-$x", "7")]
        [TestCase("$x = 3\n$test_res = $x-1", "2")]
        [TestCase("$x = 3\n$test_res = -$x * -$x", "9")]
        [TestCase("$x = 3\n$x -= -2\n$test_res = $x", "5")]
        [TestCase("$a = [1, 2]\n$test_res = -$a[1]", "-2")]
        [TestCase("func F($v) {\n    return -$v\n}\n$test_res = -F(-2) + 1", "-1")]
        [TestCase("$test_res = 0\nif (-1 < 0) $test_res = 1", "1")]
        [TestCase("$test_res = [-1, -2]", "[-1, -2]")]
        [TestCase("$test_res = [true, !true]", "[true, false]")]
        [TestCase("func F($a, $b) {\n    return $a * 10 + $b\n}\n$test_res = F(-1, -2)", "-12")]
        [TestCase("func F($a, $b) {\n    return $a * 10 + $b\n}\n$test_res = F(1, 2 * -3)", "4")]
        public async Task TestScriptEvaluation(string input, string expected)
        {

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            await botInterpreter.Run(state, CancellationToken.None);

            Assert.That(state.SymbolTable["test_res"].Identifiable.StringValue, Is.EqualTo(expected).IgnoreCase);
        }
    }
}
