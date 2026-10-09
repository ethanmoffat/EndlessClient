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
        [TestCase("$o = undefined\n$test_res = $o != undefined && $o.$x", "false")]
        [TestCase("$o = undefined\n$test_res = $o == undefined || $o.$x", "true")]
        [TestCase("$o = { $x = 1 }\n$test_res = $o != undefined && $o.$x == 1", "true")]
        [TestCase("func Boom() {\n    return $o.$x\n}\n$test_res = false && Boom()", "false")]
        [TestCase("func Boom() {\n    return $o.$x\n}\n$test_res = true || Boom()", "true")]
        [TestCase("func Boom() {\n    return $o.$x\n}\n$test_res = false && Boom() || true", "true")]
        [TestCase("func Boom() {\n    return $o.$x\n}\n$test_res = true || Boom() && Boom()", "true")]
        [TestCase("func Boom() {\n    return $o.$x\n}\n$test_res = false && (Boom() || Boom()) && Boom()", "false")]
        [TestCase("func Boom() {\n    return $o.$x\n}\n$test_res = 1 + 1 == 3 && Boom() == 2", "false")]
        [TestCase("func Boom() {\n    return $o.$x\n}\n$test_res = [false && Boom(), true || Boom(), 1]", "[false, true, 1]")]
        [TestCase("func Boom() {\n    return $o.$x\n}\nfunc F($a, $b) {\n    return $b\n}\n$test_res = F(false && Boom(), 2)", "2")]
        [TestCase("func Boom() {\n    return $o.$x\n}\n$test_res = 0\nif (!true && Boom()) $test_res = 1", "0")]
        [TestCase("$test_res = 1 == 1 && 2 == 2 && 3 == 3", "true")]
        [TestCase("$test_res = false || false || 1 < 2", "true")]
        [TestCase("$test_res = true && false || true && true", "true")]
        [TestCase("$test_res = true || false && false", "true")]
        [TestCase("$test_res = -1 < 0 && !false", "true")]
        [TestCase("$s = \"a\"\n$s += \"b\"\n$test_res = $s", "ab")]
        [TestCase("$s = \"a\"\n$s += 1\n$test_res = $s", "a1")]
        [TestCase("$s = 1\n$s += \"a\"\n$test_res = $s", "1a")]
        [TestCase("$s = 1\n$s += 2\n$test_res = $s", "3")]
        [TestCase("$a = [\"a\"]\n$a[0] += \"b\"\n$test_res = $a[0]", "ab")]
        [TestCase("$o = { $s = \"a\" }\n$o.$s += \"b\"\n$test_res = $o.$s", "ab")]
        [TestCase("$a = [\"b\"]\n$s = \"a\"\n$s += $a[0]\n$test_res = $s", "ab")]
        [TestCase("$o = { $s = \"b\" }\n$s = \"a\"\n$s += $o.$s\n$test_res = $s", "ab")]
        [TestCase("$a = [\"a\", 1]\n$a[0] += $a[1]\n$test_res = $a[0]", "a1")]
        public async Task TestScriptEvaluation(string input, string expected)
        {

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            await botInterpreter.Run(state, CancellationToken.None);

            Assert.That(state.SymbolTable["test_res"].Identifiable.StringValue, Is.EqualTo(expected).IgnoreCase);
        }

        [TestCase("$d[\"s\"] += \"b\"\n$test_res = $d[\"s\"]", "ab")]
        [TestCase("$d[\"s\"] += 1\n$test_res = $d[\"s\"]", "a1")]
        [TestCase("$d[\"n\"] += \"b\"\n$test_res = $d[\"n\"]", "1b")]
        [TestCase("$s = \"x\"\n$s += $d[\"s\"]\n$test_res = $s", "xa")]
        [TestCase("$n = 2\n$n += $d[\"s\"]\n$test_res = $n", "2a")]
        [TestCase("$d[\"s\"] += $d[\"n\"]\n$test_res = $d[\"s\"]", "a1")]
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

        [TestCase("$test_res = 5 / 0")]
        [TestCase("$test_res = 5 % 0")]
        [TestCase("$test_res = 1 + 5 / (1 - 1)")]
        [TestCase("$x = 5\n$x /= 0")]
        [TestCase("$a = [5]\n$a[0] /= 0")]
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
    }
}
