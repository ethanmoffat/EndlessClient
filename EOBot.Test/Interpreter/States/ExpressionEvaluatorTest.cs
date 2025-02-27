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
        public async Task TestExpressionEvaluation(string input, string expected)
        {
            input = $"$test_res = {input}";

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(input));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr);

            var state = botInterpreter.Parse();
            await botInterpreter.Run(state, CancellationToken.None);

            Assert.That(state.SymbolTable["test_res"].Identifiable.StringValue, Is.EqualTo(expected).IgnoreCase);
        }
    }
}
