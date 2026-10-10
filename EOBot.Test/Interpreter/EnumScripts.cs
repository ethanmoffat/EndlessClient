using System.Collections.Generic;
using NUnit.Framework;

namespace EOBot.Test.Interpreter
{
    public static class EnumScripts
    {
        public static IEnumerable<TestCaseData> Declarations() =>
        [
            new TestCaseData("enum E { A, B, C }", "C", 2, "E").SetArgDisplayNames("Sequential"),
            new TestCaseData("enum E { A = 1, B, C }", "C", 3, "E").SetArgDisplayNames("ExplicitStart"),
            new TestCaseData("enum E { A = 10, B = 5, C }", "C", 6, "E").SetArgDisplayNames("ContinuesFromLastExplicit"),
            new TestCaseData("enum E { A = -5, B }", "B", -4, "E").SetArgDisplayNames("NegativeStart"),
            new TestCaseData("enum E { A, B, C, }", "C", 2, "E").SetArgDisplayNames("TrailingCommaInline"),
            new TestCaseData("""
                enum E
                {
                    A = 3,
                    B,
                    C
                }
                """, "C", 5, "E").SetArgDisplayNames("BraceOnNextLine"),
            new TestCaseData("""
                enum E {
                    A,
                    B = 7,
                    C,
                }
                """, "B", 7, "E").SetArgDisplayNames("TrailingComma"),
            new TestCaseData("""
                enum E {}
                enum F { C = 1 }
                """, "C", 1, "F").SetArgDisplayNames("EmptyEnum"),
        ];

        public static IEnumerable<TestCaseData> Expressions() =>
        [
            new TestCaseData("E::B + 1", "6").SetArgDisplayNames("AddInt"),
            new TestCaseData("E::C * 2", "12").SetArgDisplayNames("MultiplyInt"),
            new TestCaseData("E::B == 5", "true").SetArgDisplayNames("EqualsInt"),
            new TestCaseData("5 == E::B", "true").SetArgDisplayNames("IntEqualsEnum"),
            new TestCaseData("E::B != 5", "false").SetArgDisplayNames("NotEqualsInt"),
            new TestCaseData("E::B == E::B", "true").SetArgDisplayNames("EqualsSameMember"),
            new TestCaseData("E::B == E::C", "false").SetArgDisplayNames("EqualsOtherMember"),
            new TestCaseData("E::A == G::A", "false").SetArgDisplayNames("EqualsOtherEnumSameValue"),
            new TestCaseData("E::A != G::A", "true").SetArgDisplayNames("NotEqualsOtherEnumSameValue"),
            new TestCaseData("E::C > E::B", "true").SetArgDisplayNames("GreaterThan"),
            new TestCaseData("E::D < 0", "true").SetArgDisplayNames("NegativeLessThanZero"),
            new TestCaseData("E::B === 5", "false").SetArgDisplayNames("StrictEqualsInt"),
            new TestCaseData("E::B !== 5", "true").SetArgDisplayNames("StrictNotEqualsInt"),
            new TestCaseData("E::B === E::B", "true").SetArgDisplayNames("StrictEqualsSameMember"),
            new TestCaseData("E::A === G::A", "false").SetArgDisplayNames("StrictEqualsOtherEnum"),
            new TestCaseData("E::A is E", "true").SetArgDisplayNames("IsOwnEnum"),
            new TestCaseData("E::A is G", "false").SetArgDisplayNames("IsOtherEnum"),
            new TestCaseData("E::A is enum", "true").SetArgDisplayNames("IsEnum"),
            new TestCaseData("E::A is int", "false").SetArgDisplayNames("IsInt"),
            new TestCaseData("5 is enum", "false").SetArgDisplayNames("IntIsEnum"),
            new TestCaseData("5 is E", "false").SetArgDisplayNames("IntIsNamedEnum"),
            new TestCaseData("\"value: \" + E::B", "value: E::B").SetArgDisplayNames("StringConcatenation"),
            new TestCaseData("!E::A", "true").SetArgDisplayNames("NotZeroMember"),
        ];

        public static IEnumerable<TestCaseData> VariableMembers() =>
        [
            new TestCaseData("name", "B").SetArgDisplayNames("Name"),
            new TestCaseData("value", "5").SetArgDisplayNames("Value"),
            new TestCaseData("type", "E").SetArgDisplayNames("Type"),
        ];

        public static IEnumerable<TestCaseData> InvalidReferences() =>
        [
            new TestCaseData("E::Z").SetArgDisplayNames("UnknownMember"),
            new TestCaseData("Q::A").SetArgDisplayNames("UnknownEnum"),
            new TestCaseData("$e::A").SetArgDisplayNames("VariableScope"),
        ];

        public static IEnumerable<TestCaseData> InvalidDeclarations() =>
        [
            new TestCaseData("enum E { A, A }").SetArgDisplayNames("DuplicateMember"),
            new TestCaseData("enum E { A B }").SetArgDisplayNames("MissingComma"),
            new TestCaseData("""enum E { A = "x" }""").SetArgDisplayNames("StringValue"),
            new TestCaseData("enum E { A = $x }").SetArgDisplayNames("VariableValue"),
            new TestCaseData("enum E { A").SetArgDisplayNames("MissingCloseBrace"),
            new TestCaseData("enum { A }").SetArgDisplayNames("MissingName"),
            new TestCaseData("""
                enum E { A }
                enum E { B }
                """).SetArgDisplayNames("DuplicateEnum"),
            new TestCaseData("""
                func E() {
                }
                enum E { A }
                """).SetArgDisplayNames("FunctionNameConflict"),
            new TestCaseData("""
                func F() {
                    enum E { A }
                }
                """).SetArgDisplayNames("EnumInFunction"),
            new TestCaseData("""
                func F() {
                    map LoginReply
                }
                """).SetArgDisplayNames("MapInFunction"),
        ];

        public static IEnumerable<TestCaseData> InvalidMaps() =>
        [
            new TestCaseData("map NotARealEnumTypeName").SetArgDisplayNames("UnknownType"),
            new TestCaseData("from Not.A.Real.Namespace map LoginReply").SetArgDisplayNames("UnknownNamespace"),
            new TestCaseData("map *").SetArgDisplayNames("WildcardWithoutNamespace"),
            new TestCaseData("from Not.A.Real.Namespace map *").SetArgDisplayNames("WildcardUnknownNamespace"),
            new TestCaseData("from Moffat.EndlessOnline.SDK.Protocol.Net.Server map * to X").SetArgDisplayNames("WildcardWithAlias"),
            new TestCaseData("map LoginReply to").SetArgDisplayNames("MissingAlias"),
            new TestCaseData("map LoginReply extra").SetArgDisplayNames("TrailingToken"),
            new TestCaseData("from Moffat.EndlessOnline.SDK.Protocol.Net.Server LoginReply").SetArgDisplayNames("MissingMapKeyword"),
            new TestCaseData("""
                enum LoginReply { A }
                map LoginReply
                """).SetArgDisplayNames("ConflictsWithScriptEnum"),
        ];
    }
}
