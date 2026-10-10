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
        private const string TestEnums = """
            enum E { A, B = 5, C, D = -2, F }
            enum G { A }

            """;

        [TestCaseSource(typeof(EnumScripts), nameof(EnumScripts.Declarations))]
        public async Task EnumDeclaration_AssignsExpectedValues(string declaration, string member, int expected, string enumName = "E")
        {
            var script = $$"""
                {{declaration}}
                $res = {{enumName}}::{{member}}
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            var res = state.SymbolTable["res"].Identifiable;
            Assert.That(res, Is.InstanceOf<EnumVariable>());
            Assert.That(((EnumVariable)res).Value, Is.EqualTo(expected));
            Assert.That(res.StringValue, Is.EqualTo($"{enumName}::{member}"));
        }

        [TestCaseSource(typeof(EnumScripts), nameof(EnumScripts.Expressions))]
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

        [TestCaseSource(typeof(EnumScripts), nameof(EnumScripts.VariableMembers))]
        public async Task EnumVariable_MembersAreAccessible(string member, string expected)
        {
            var script = $$"""
                {{TestEnums}}$e = E::B
                $res = $e.${{member}}
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        [Test]
        public async Task EnumValue_CanBeUsedInsideUserDefinedFunction()
        {
            const string script = $$"""
                {{TestEnums}}func Get($x) {
                    return $x + E::B
                }
                $res = Get(E::C)
                """;

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
            const string script = $$"""
                {{TestEnums}}goto skip
                $y = 1
                skip:
                $res = E::B
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("E::B"));
            Assert.That(state.SymbolTable.ContainsKey("y"), Is.False);
        }

        [Test]
        public async Task Declarations_BackToBack_AreAllRegistered()
        {
            const string script = """
                enum E { A } enum F { B }
                func X() {
                }
                enum G { C }
                $res = F::B + G::C
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("0"));
        }

        [TestCaseSource(typeof(EnumScripts), nameof(EnumScripts.InvalidReferences))]
        public async Task InvalidEnumReference_Fails(string expression)
        {
            var script = $$"""
                {{TestEnums}}$e = 1
                $res = {{expression}}
                """;

            var (_, result, _) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Failed).Or.EqualTo(EvalResult.NotMatch));
        }

        [Test]
        public async Task EnumReference_NonEnumIdentifier_Fails()
        {
            const string script = """
                func NotEnum() {
                }
                $res = NotEnum::A
                """;

            var (_, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Failed));
            Assert.That(reason, Does.Contain("not an enum"));
        }

        [TestCaseSource(typeof(EnumScripts), nameof(EnumScripts.InvalidDeclarations))]
        public void InvalidDeclaration_ThrowsOnParse(string script)
        {
            Assert.ThrowsAsync<BotScriptErrorException>(() => RunAsync(script));
        }

        [Test]
        public async Task Map_Implicit_ResolvesDotNetEnum()
        {
            const string script = """
                map LoginReply
                $res = LoginReply::Ok
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(((EnumVariable)state.SymbolTable["res"].Identifiable).Value, Is.EqualTo(3));
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("LoginReply::Ok"));
        }

        [Test]
        public async Task Map_ExplicitWithAlias_ResolvesDotNetEnum()
        {
            const string script = """
                from Moffat.EndlessOnline.SDK.Protocol.Net.Server map LoginReply to Reply
                $res = Reply::Banned
                $isReply = $res is Reply
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(((EnumVariable)state.SymbolTable["res"].Identifiable).Value, Is.EqualTo(4));
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("Reply::Banned"));
            Assert.That(state.SymbolTable["isReply"].Identifiable.StringValue, Is.EqualTo("true").IgnoreCase);
            Assert.That(state.SymbolTable.ContainsKey("LoginReply"), Is.False);
        }

        [Test]
        public async Task Map_Wildcard_MapsAllEnumsInNamespace()
        {
            const string script = """
                from Moffat.EndlessOnline.SDK.Protocol.Net.Server map *
                $a = AccountReply::Created
                $b = LoginReply::Ok
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["a"].Identifiable.StringValue, Is.EqualTo("AccountReply::Created"));
            Assert.That(state.SymbolTable["b"].Identifiable.StringValue, Is.EqualTo("LoginReply::Ok"));
        }

        [Test]
        public async Task Map_DotNetEnumComparesWithInt()
        {
            const string script = """
                map Direction
                $dir = 3
                $res = $dir == Direction::Right
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo("true").IgnoreCase);
        }

        [TestCaseSource(typeof(EnumScripts), nameof(EnumScripts.InvalidMaps))]
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
