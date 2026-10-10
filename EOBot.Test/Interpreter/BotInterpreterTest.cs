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
    public class BotInterpreterTest
    {
        [TestCase("if (")]
        [TestCase("$x = $nope + 1")]
        [TestCase("nope()")]
        [TestCase("$x = 1 + \"a\" * []")]
        [TestCase("$x = 1 / 0")]
        public async Task Run_ScriptError_ReturnsNonZero(string input)
        {
            var (interpreter, state) = Parse(input);

            Assert.That(await interpreter.Run(state, CancellationToken.None), Is.Not.Zero);
        }

        [Test]
        public async Task Run_Success_ReturnsZero()
        {
            var (interpreter, state) = Parse("$x = 1 + 2");

            Assert.That(await interpreter.Run(state, CancellationToken.None), Is.Zero);
        }

        [Test]
        public async Task Run_Cancelled_Returns130()
        {
            var (interpreter, state) = Parse("$x = 1");

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.That(await interpreter.Run(state, cts.Token), Is.EqualTo(130));
        }

        private static (BotInterpreter, ProgramState) Parse(string input)
        {
            var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(input)));
            var interpreter = new BotInterpreter(sr);
            return (interpreter, interpreter.Parse());
        }
    }
}
