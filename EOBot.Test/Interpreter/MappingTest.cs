using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter;
using EOBot.Interpreter.Hosting;
using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;
using NUnit.Framework;

namespace EOBot.Test.Interpreter
{
    [TestFixture]
    public class MappingTest
    {
        private const string From = "from EOBot.Test.Interpreter.TestApi";

        private readonly TypeCatalog _typeCatalog = TypeCatalog.FromRootAssemblies(typeof(MappingTest).Assembly);

        [TestCase("map StaticMath.Add\n$res = Add(1, 2)", "3")]
        [TestCase("map StaticMath.Add to add\n$res = add(1, 2)", "3")]
        public async Task Map_StaticMethod_IsCallable(string script, string expected)
        {
            var (state, result, reason) = await RunAsync($"{From} {script}");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        [Test]
        public async Task Map_FrameworkMethods_ResolveIntOverloads()
        {
            const string script = """
                from System map Math.Max
                from System map Math.Abs to abs
                $res = abs(Max(-5, -3))
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("3"));
        }

        [TestCase("Describe", "Describe(1)", "int 1")]
        [TestCase("Describe", "Describe(\"x\")", "string x")]
        [TestCase("Describe", "Describe(1, 2)", "ints 1 2")]
        [TestCase("Optional", "Optional(1)", "11")]
        [TestCase("Optional", "Optional(1, 2)", "3")]
        [TestCase("Count", "Count([1, 2, 3])", "3")]
        [TestCase("Pair", "Pair(1, \"a\")", "[1, a]")]
        [TestCase("DelayedValue", "DelayedValue(7)", "7")]
        [TestCase("DelayedSum", "DelayedSum(2, 3)", "5")]
        [TestCase("Narrow", "Narrow(300)", "44")]
        [TestCase("Narrow", "Narrow(-1)", "255")]
        [TestCase("Pick", "Pick(300)", "long")]
        [TestCase("Big", "Big()", "5")]
        [TestCase("BigUnsigned", "BigUnsigned()", "-1")]
        public async Task Map_Overloads_ResolveByArguments(string method, string call, string expected)
        {
            var (state, result, reason) = await RunAsync($"{From} map StaticMath.{method}\n$res = {call}");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        [TestCase("Big", "Big()", true)]
        [TestCase("Add", "Add(1, 2)", false)]
        public async Task Map_IntegerReturn_FlagsTruncation(string method, string call, bool expected)
        {
            var (state, result, reason) = await RunAsync($"{From} map StaticMath.{method}\n$res = {call}");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(((IntVariable)state.SymbolTable["res"].Identifiable).Truncated, Is.EqualTo(expected));
        }

        [Test]
        public async Task Map_VoidMethod_CanBeCalledAsStatement()
        {
            var (_, result, reason) = await RunAsync($"{From} map StaticMath.Noop\nNoop()");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
        }

        [TestCase("Describe", "Describe(true)")]
        [TestCase("Add", "Add(1)")]
        [TestCase("OptionalWithoutDefault", "OptionalWithoutDefault()")]
        [TestCase("Add", "Add(\"a\", \"b\")")]
        [TestCase("Fail", "Fail(\"oops\")")]
        public void Map_InvalidCall_Throws(string method, string call)
        {
            Assert.ThrowsAsync<BotScriptErrorException>(() => RunAsync($"{From} map StaticMath.{method}\n$res = {call}"));
        }

        [Test]
        public async Task Map_EnumReturn_IsEnumVariable()
        {
            const string script = $"""
                {From} map StaticMath.GetColor
                {From} map StaticMath.ColorName
                {From} map TestColor to Color
                $c = GetColor()
                $eq = $c == Color::Green
                $strict = $c === Color::Green
                $is = $c is Color
                $int = $c == 1
                $name = ColorName(Color::Blue)
                $loose = ColorName(0)
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["c"].Identifiable, Is.InstanceOf<EnumVariable>());
            Assert.That(state.SymbolTable["c"].Identifiable.StringValue, Is.EqualTo("TestColor::Green"));
            Assert.That(state.SymbolTable["eq"].Identifiable.StringValue, Is.EqualTo("true").IgnoreCase);
            Assert.That(state.SymbolTable["strict"].Identifiable.StringValue, Is.EqualTo("true").IgnoreCase);
            Assert.That(state.SymbolTable["is"].Identifiable.StringValue, Is.EqualTo("true").IgnoreCase);
            Assert.That(state.SymbolTable["int"].Identifiable.StringValue, Is.EqualTo("true").IgnoreCase);
            Assert.That(state.SymbolTable["name"].Identifiable.StringValue, Is.EqualTo("Blue"));
            Assert.That(state.SymbolTable["loose"].Identifiable.StringValue, Is.EqualTo("Red"));
        }

        [TestCase(From + " map Unsupported.Now")]
        [TestCase(From + " map Unsupported.Take")]
        [TestCase(From + " map StaticMath.NotAMethod")]
        [TestCase(From + " map NotAType.Add")]
        [TestCase(From + " map Counter.Increment")]
        [TestCase(From + " map StaticMath.*")]
        [TestCase(From + " map StaticMath.Add\n" + From + " map StaticMath.Add")]
        [TestCase("map StaticMath.Add")]
        [TestCase("from Not.A.Namespace map StaticMath.Add")]
        [TestCase("func F() {\n    " + From + " map StaticMath.Add\n}")]
        public void InvalidMap_ThrowsOnParse(string script)
        {
            Assert.ThrowsAsync<BotScriptErrorException>(() => RunAsync(script));
        }

        [Test]
        public void Map_WithoutScriptHost_ThrowsOnParse()
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes($"{From} map StaticMath.Add"));
            using var sr = new StreamReader(ms);

            Assert.Throws<BotScriptErrorException>(() => new BotInterpreter(sr).Parse());
        }

        private async Task<(ProgramState State, EvalResult Result, string Reason)> RunAsync(string script)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(script));
            using var sr = new StreamReader(ms);
            var botInterpreter = new BotInterpreter(sr, new ScriptHost(_typeCatalog));

            var state = botInterpreter.Parse();

            var (result, reason, _) = await ScriptEvaluator.Instance.EvaluateAsync(state, CancellationToken.None);
            return (state, result, reason);
        }
    }
}
