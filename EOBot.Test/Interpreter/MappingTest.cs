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

        [Test]
        public async Task Map_FrameworkParamsMethod_AcceptsArgumentsOrArray()
        {
            const string script = """
                from System map String.Join
                $a = Join("-", "x", "y")
                $b = Join("-", ["x", "y"])
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["a"].Identifiable.StringValue, Is.EqualTo("x-y"));
            Assert.That(state.SymbolTable["b"].Identifiable.StringValue, Is.EqualTo("x-y"));
        }

        [Test]
        public async Task Map_FrameworkObject_ReadsProperties()
        {
            const string script = """
                from System map object Environment
                $name = $Environment.$MachineName
                $count = $Environment.$ProcessorCount
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["name"].Identifiable.StringValue, Is.EqualTo(System.Environment.MachineName));
            Assert.That(state.SymbolTable["count"].Identifiable.StringValue, Is.EqualTo(System.Environment.ProcessorCount.ToString()));
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

        [TestCase("Sum", "Sum()", "0")]
        [TestCase("Sum", "Sum(1, 2, 3)", "6")]
        [TestCase("Sum", "Sum([1, 2, 3])", "6")]
        [TestCase("Join", "Join(\"-\", \"a\", \"b\")", "a-b")]
        [TestCase("Join", "Join(\"-\", [\"a\", \"b\"])", "a-b")]
        [TestCase("Join", "Join(\"-\")", "")]
        [TestCase("Choose", "Choose(1, 2)", "fixed")]
        [TestCase("Choose", "Choose(1)", "params 1")]
        [TestCase("Choose", "Choose([1, 2])", "params 2")]
        [TestCase("CountAll", "CountAll([1, 2])", "2")]
        [TestCase("CountAll", "CountAll(1, [1, 2])", "2")]
        [TestCase("Specific", "Specific([1])", "array")]
        [TestCase("SumList", "SumList([1, 2, 3])", "6")]
        [TestCase("SumReadOnly", "SumReadOnly([1, 2, 3])", "6")]
        [TestCase("CountDistinct", "CountDistinct([1, 1, 2])", "2")]
        [TestCase("QueueFirst", "QueueFirst([\"a\", \"b\"])", "a")]
        [TestCase("StackTop", "StackTop([\"a\", \"b\"])", "b")]
        [TestCase("SumCollection", "SumCollection([1, 2])", "3")]
        [TestCase("SumNested", "SumNested([[1, 2], [3]])", "6")]
        [TestCase("Range", "Range(3)", "[0, 1, 2]")]
        [TestCase("Letters", "Letters()", "[a, b]")]
        [TestCase("Grid", "Grid()", "[[1, 2], [3]]")]
        public async Task Map_Collections_Convert(string method, string call, string expected)
        {
            var (state, result, reason) = await RunAsync($"{From} map Collections.{method}\n$res = {call}");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["res"].Identifiable.StringValue, Is.EqualTo(expected));
        }

        [Test]
        public async Task Map_ScriptArrayToIVariableList_SharesInstance()
        {
            var (state, result, reason) = await RunAsync($"{From} map Collections.Append\n$arr = [1]\nAppend($arr, 2)");

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["arr"].Identifiable.StringValue, Is.EqualTo("[1, 2]"));
        }

        [TestCase("Sum", "Sum(\"a\")")]
        [TestCase("Sum", "Sum([1, \"a\"])")]
        [TestCase("SumList", "SumList(1)")]
        [TestCase("SumNested", "SumNested([1, 2])")]
        [TestCase("Unrelated", "Unrelated([1])")]
        public void Map_InvalidCollectionCall_Throws(string method, string call)
        {
            Assert.ThrowsAsync<BotScriptErrorException>(() => RunAsync($"{From} map Collections.{method}\n$res = {call}"));
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

        [Test]
        public async Task Map_AllMethods_MapsEachMethodName()
        {
            const string script = $"""
                {From} map StaticMath.*
                from System map Math.*
                $a = Add(1, 2)
                $b = Describe("x")
                $c = Max(1, Abs(-4))
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["a"].Identifiable.StringValue, Is.EqualTo("3"));
            Assert.That(state.SymbolTable["b"].Identifiable.StringValue, Is.EqualTo("string x"));
            Assert.That(state.SymbolTable["c"].Identifiable.StringValue, Is.EqualTo("4"));
        }

        [Test]
        public async Task Map_NestedTypes_MapsEnumsAndMethods()
        {
            const string script = $"""
                {From} map Outer.Inner
                {From} map Outer.Nested.Deeper to D
                {From} map Outer.Nested.Triple to triple
                {From} map Outer.Nested.*
                $a = Inner::B
                $b = D::Y
                $c = triple(2)
                $d = Quadruple(2)
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["a"].Identifiable.StringValue, Is.EqualTo("Inner::B"));
            Assert.That(state.SymbolTable["b"].Identifiable.StringValue, Is.EqualTo("D::Y"));
            Assert.That(state.SymbolTable["c"].Identifiable.StringValue, Is.EqualTo("6"));
            Assert.That(state.SymbolTable["d"].Identifiable.StringValue, Is.EqualTo("8"));
        }

        [Test]
        public async Task Map_Object_ReadsStaticPropertiesOnAccess()
        {
            const string script = $"""
                {From} map object Settings
                {From} map object Settings to s
                $name = $Settings.$Name
                $color = $s.$Color
                $first = $Settings.$Reads
                $second = $Settings.$Reads
                """;

            var (state, result, reason) = await RunAsync(script);

            Assert.That(result, Is.EqualTo(EvalResult.Ok), reason);
            Assert.That(state.SymbolTable["name"].Identifiable.StringValue, Is.EqualTo("test"));
            Assert.That(state.SymbolTable["color"].Identifiable, Is.InstanceOf<EnumVariable>());
            Assert.That(state.SymbolTable["color"].Identifiable.StringValue, Is.EqualTo("TestColor::Blue"));

            var first = ((IntVariable)state.SymbolTable["first"].Identifiable).Value;
            Assert.That(((IntVariable)state.SymbolTable["second"].Identifiable).Value, Is.EqualTo(first + 1));

            string[] expectedMembers = ["Name", "Color", "Reads", "Throws"];
            var members = ((RuntimeEvaluatedMemberObjectVariable)state.SymbolTable["Settings"].Identifiable).SymbolTable;
            Assert.That(members.Keys, Is.EquivalentTo(expectedMembers));
        }

        [TestCase("$Settings = 1")]
        [TestCase("$Settings.$Name = \"x\"")]
        public async Task Map_Object_IsReadOnly(string statement)
        {
            var (_, result, reason) = await RunAsync($"{From} map object Settings\n{statement}");

            Assert.That(result, Is.EqualTo(EvalResult.Failed), reason);
            Assert.That(reason, Does.Contain("read-only"));
        }

        [Test]
        public void Map_ObjectPropertyException_Throws()
        {
            Assert.ThrowsAsync<BotScriptErrorException>(() => RunAsync($"{From} map object Settings\n$x = $Settings.$Throws"));
        }

        [TestCase(From + " map Unsupported.Now")]
        [TestCase(From + " map Unsupported.Take")]
        [TestCase(From + " map Unsupported.TakeImmutable")]
        [TestCase(From + " map Unsupported.TakeChars")]
        [TestCase(From + " map Unsupported.ReturnDictionary")]
        [TestCase(From + " map StaticMath.NotAMethod")]
        [TestCase(From + " map NotAType.Add")]
        [TestCase(From + " map Counter.Increment")]
        [TestCase(From + " map StaticMath.* to all")]
        [TestCase(From + " map Unsupported.*")]
        [TestCase(From + " map StaticMath.Add\n" + From + " map StaticMath.*")]
        [TestCase(From + " map Outer.Missing.Deeper")]
        [TestCase(From + " map Outer.Nested")]
        [TestCase(From + " map Outer.Inner.A")]
        [TestCase("map Outer.Inner")]
        [TestCase(From + " map object Counter")]
        [TestCase(From + " map object NotAType")]
        [TestCase(From + " map object Settings.Name")]
        [TestCase(From + " map object Settings\n" + From + " map object Settings")]
        [TestCase("map object Settings")]
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
