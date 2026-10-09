using System.IO;
using System.Linq;
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
    public class EnumTest
    {
        private const string TestEnums = "enum E { A, B = 5, C, D = -2, F }\nenum G { A }\n";

        [TestCase("enum E { A, B, C }", "C", 2)]
        [TestCase("enum E { A = 1, B, C }", "C", 3)]
        [TestCase("enum E { A = 10, B = 5, C }", "C", 6)]
        [TestCase("enum E { A = -5, B }", "B", -4)]
        [TestCase("enum E { A, B, C, }", "C", 2)]
        [TestCase("enum E\n{\n    A = 3,\n    B,\n    C\n}", "C", 5)]
        [TestCase("enum E {\n    A,\n    B = 7,\n    C,\n}", "B", 7)]
        [TestCase("enum E {}\nenum F { C = 1 }", "C", 1, "F")]
        public async Task EnumDeclaration_AssignsExpectedValues(string declaration, string member, int expected, string enumName = "E")
        {
            var (state, result, reason) = await RunAsync($"{declaration}\n$res = {enumName}::{member}");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            var res = state.SymbolTable["res"].Identifiable;
            Assert.That(res, Is.InstanceOf<EnumVariable>());
            Assert.That(((EnumVariable)res).Value, Is.EqualTo(expected));
            Assert.That(res.StringValue, Is.EqualTo($"{enumName}::{member}"));
        }

        [TestCase("E::B + 1", "6")]
        [TestCase("E::C * 2", "12")]
        [TestCase("E::B == 5", "true")]
        [TestCase("5 == E::B", "true")]
        [TestCase("E::B != 5", "false")]
        [TestCase("E::B == E::B", "true")]
        [TestCase("E::B == E::C", "false")]
        [TestCase("E::A == G::A", "false")]
        [TestCase("E::A != G::A", "true")]
        [TestCase("E::C > E::B", "true")]
        [TestCase("E::D < 0", "true")]
        [TestCase("E::B === 5", "false")]
        [TestCase("E::B !== 5", "true")]
        [TestCase("E::B === E::B", "true")]
        [TestCase("E::A === G::A", "false")]
        [TestCase("E::A is E", "true")]
        [TestCase("E::A is G", "false")]
        [TestCase("E::A is enum", "true")]
        [TestCase("E::A is int", "false")]
        [TestCase("5 is enum", "false")]
        [TestCase("5 is E", "false")]
        [TestCase("\"value: \" + E::B", "value: E::B")]
        [TestCase("!E::A", "true")]
        public async Task EnumExpression_EvaluatesExpectedResult(string expression, string expected)
        {
            var (state, result, reason) = await RunAsync($"{TestEnums}$res = {expression}");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected).IgnoreCase);
        }

        [Test]
        public async Task EnumArithmetic_ResultIsPlainInt()
        {
            var (state, result, reason) = await RunAsync($"{TestEnums}$res = E::B + 1");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.GetType(), Is.EqualTo(typeof(IntVariable)));
        }

        [TestCase("name", "B")]
        [TestCase("value", "5")]
        [TestCase("type", "E")]
        public async Task EnumVariable_MembersAreAccessible(string member, string expected)
        {
            var (state, result, reason) = await RunAsync($"{TestEnums}$e = E::B\n$res = $e.${member}");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        [Test]
        public async Task EnumValue_CanBeUsedInsideUserDefinedFunction()
        {
            var script = $"{TestEnums}func Get($x) {{\n    return $x + E::B\n}}\n$res = Get(E::C)";

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("11"));
        }

        [Test]
        public async Task EnumValue_CanBePassedToBuiltInIntParameter()
        {
            var (state, result, reason) = await RunAsync($"{TestEnums}$res = abs(E::D)", s =>
                s.SymbolTable["abs"] = (true, new Function<int, int>("abs", System.Math.Abs)));

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("2"));
        }

        [Test]
        public async Task EnumValue_DoesNotBreakLabels()
        {
            var script = $"{TestEnums}goto skip\n$y = 1\nskip:\n$res = E::B";

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("E::B"));
            Assert.That(state.SymbolTable.ContainsKey("y"), Is.False);
        }

        [Test]
        public async Task Declarations_BackToBack_AreAllRegistered()
        {
            var script = "enum E { A } enum F { B }\nfunc X() {\n}\nenum G { C }\n$res = F::B + G::C";

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("0"));
        }

        [TestCase("E::Z")]
        [TestCase("Q::A")]
        [TestCase("$e::A")]
        public async Task InvalidEnumReference_Fails(string expression)
        {
            var (_, result, _) = await RunAsync($"{TestEnums}$e = 1\n$res = {expression}");

            Assert.That(result, Is.EqualTo(EvalResult.Failed).Or.EqualTo(EvalResult.NotMatch));
        }

        [Test]
        public async Task EnumReference_NonEnumIdentifier_Fails()
        {
            var (_, result, reason) = await RunAsync("func NotEnum() {\n}\n$res = NotEnum::A");

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain("not an enum"));
        }

        [TestCase("enum E { A, A }")]
        [TestCase("enum E { A B }")]
        [TestCase("enum E { A = \"x\" }")]
        [TestCase("enum E { A = $x }")]
        [TestCase("enum E { A")]
        [TestCase("enum { A }")]
        [TestCase("enum E { A }\nenum E { B }")]
        [TestCase("func E() {\n}\nenum E { A }")]
        [TestCase("func F() {\n    enum E { A }\n}")]
        [TestCase("func F() {\n    map LoginReply\n}")]
        public void InvalidDeclaration_ThrowsOnParse(string script)
        {
            Assert.ThrowsAsync<BotScriptErrorException>(() => RunAsync(script));
        }

        [Test]
        public async Task Map_Implicit_ResolvesDotNetEnum()
        {
            var (state, result, reason) = await RunAsync("map LoginReply\n$res = LoginReply::Ok");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(((EnumVariable)state.SymbolTable["res"].Identifiable).Value, Is.EqualTo(3));
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("LoginReply::Ok"));
        }

        [Test]
        public async Task Map_ExplicitWithAlias_ResolvesDotNetEnum()
        {
            var (state, result, reason) = await RunAsync("from Moffat.EndlessOnline.SDK.Protocol.Net.Server map LoginReply to Reply\n$res = Reply::Banned\n$isReply = $res is Reply");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(((EnumVariable)state.SymbolTable["res"].Identifiable).Value, Is.EqualTo(4));
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("Reply::Banned"));
            Assert.That(state.SymbolTable["isReply"].Identifiable.StringValue, Is.EqualTo("true").IgnoreCase);
            Assert.That(state.SymbolTable.ContainsKey("LoginReply"), Is.False);
        }

        [Test]
        public async Task Map_Wildcard_MapsAllEnumsInNamespace()
        {
            var (state, result, reason) = await RunAsync("from Moffat.EndlessOnline.SDK.Protocol.Net.Server map *\n$a = AccountReply::Created\n$b = LoginReply::Ok");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["a"].Identifiable.StringValue, Is.EqualTo("AccountReply::Created"));
            Assert.That(state.SymbolTable["b"].Identifiable.StringValue, Is.EqualTo("LoginReply::Ok"));
        }

        [Test]
        public async Task Map_DotNetEnumComparesWithInt()
        {
            var (state, result, reason) = await RunAsync("map Direction\n$dir = 3\n$res = $dir == Direction::Right");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("true").IgnoreCase);
        }

        [TestCase("map NotARealEnumTypeName")]
        [TestCase("from Not.A.Real.Namespace map LoginReply")]
        [TestCase("map *")]
        [TestCase("from Not.A.Real.Namespace map *")]
        [TestCase("from Moffat.EndlessOnline.SDK.Protocol.Net.Server map * to X")]
        [TestCase("map LoginReply to")]
        [TestCase("map LoginReply extra")]
        [TestCase("from Moffat.EndlessOnline.SDK.Protocol.Net.Server LoginReply")]
        [TestCase("enum LoginReply { A }\nmap LoginReply")]
        public void InvalidMap_ThrowsOnParse(string script)
        {
            Assert.ThrowsAsync<BotScriptErrorException>(() => RunAsync(script));
        }

        [Test]
        public void Map_AmbiguousName_ThrowsOnParse()
        {
            var ambiguousName = EnumTypeCatalog.EnumTypes.GroupBy(x => x.Name).First(x => x.Count() > 1).Key;

            var ex = Assert.ThrowsAsync<BotScriptErrorException>(() => RunAsync($"map {ambiguousName}"));
            Assert.That(ex.Message, Does.Contain("ambiguous"));
        }

        private static async Task<(ProgramState State, EvalResult Result, string Reason)> RunAsync(string script, System.Action<ProgramState> setup = null)
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
