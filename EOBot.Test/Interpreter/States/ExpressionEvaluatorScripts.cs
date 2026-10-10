using System.Collections.Generic;
using NUnit.Framework;

namespace EOBot.Test.Interpreter.States
{
    public static class ExpressionEvaluatorScripts
    {
        public static IEnumerable<TestCaseData> LogicalExpressions() =>
        [
            new TestCaseData("true && false", "false").SetArgDisplayNames("And"),
            new TestCaseData("true || false", "true").SetArgDisplayNames("Or"),
            new TestCaseData("!true", "false").SetArgDisplayNames("NotTrue"),
            new TestCaseData("!false", "true").SetArgDisplayNames("NotFalse"),
            new TestCaseData("!true && !false", "false").SetArgDisplayNames("NotOperandsAnd"),
            new TestCaseData("!(true || false)", "false").SetArgDisplayNames("NotGroupedOr"),
            new TestCaseData("!true || !false", "true").SetArgDisplayNames("NotOperandsOr"),
            new TestCaseData("!(true && false)", "true").SetArgDisplayNames("NotGroupedAnd"),
            new TestCaseData("!(false) && false", "false").SetArgDisplayNames("NotGroupedLiteralAnd"),
            new TestCaseData("!!true", "true").SetArgDisplayNames("DoubleNot"),
        ];

        public static IEnumerable<TestCaseData> ArithmeticExpressions() =>
        [
            new TestCaseData("2 / 2 + 5 * 4 - 3", "18").SetArgDisplayNames("DivisionAndMultiplicationFirst"),
            new TestCaseData("2 + 2 / 5 - 4 * 3", "-10").SetArgDisplayNames("DivisionAndMultiplicationFirstMixed"),
            new TestCaseData("5 * 4 - 3", "17").SetArgDisplayNames("MultiplyThenSubtract"),
            new TestCaseData("5 - 4 * 3", "-7").SetArgDisplayNames("SubtractMultiplied"),
            new TestCaseData("8 / 8 + 4", "5").SetArgDisplayNames("DivideThenAdd"),
            new TestCaseData("8 + 8 / 4", "10").SetArgDisplayNames("AddDivided"),
            new TestCaseData("5 - (2 * 4) + 4 - (8 / 2)", "-3").SetArgDisplayNames("ParenthesizedGroups"),
            new TestCaseData("2 * (3) + 1", "7").SetArgDisplayNames("ParenthesizedLiteral"),
        ];

        public static IEnumerable<TestCaseData> UnaryMinusExpressions() =>
        [
            new TestCaseData("-4", "-4").SetArgDisplayNames("NegativeLiteral"),
            new TestCaseData("-4 + 1", "-3").SetArgDisplayNames("NegativePlusLiteral"),
            new TestCaseData("1 + -4", "-3").SetArgDisplayNames("PlusNegative"),
            new TestCaseData("1 - -4", "5").SetArgDisplayNames("MinusNegative"),
            new TestCaseData("1-4", "-3").SetArgDisplayNames("MinusUnspaced"),
            new TestCaseData("2 * -3", "-6").SetArgDisplayNames("TimesNegative"),
            new TestCaseData("-2 * 3", "-6").SetArgDisplayNames("NegativeTimes"),
            new TestCaseData("-(2 + 3)", "-5").SetArgDisplayNames("NegatedGroup"),
            new TestCaseData("-(2) + 3", "1").SetArgDisplayNames("NegatedGroupedLiteral"),
            new TestCaseData("- -4", "4").SetArgDisplayNames("DoubleNegative"),
            new TestCaseData("- - -4", "-4").SetArgDisplayNames("TripleNegative"),
            new TestCaseData("-4 < 0", "true").SetArgDisplayNames("NegativeComparison"),
        ];

        public static IEnumerable<TestCaseData> UnaryMinus() =>
        [
            new TestCaseData("""
                $x = 3
                $test_res = -$x
                """, "-3").SetArgDisplayNames("NegateVariable"),
            new TestCaseData("""
                $x = 3
                $test_res = 10-$x
                """, "7").SetArgDisplayNames("LiteralMinusVariableUnspaced"),
            new TestCaseData("""
                $x = 3
                $test_res = $x-1
                """, "2").SetArgDisplayNames("VariableMinusLiteralUnspaced"),
            new TestCaseData("""
                $x = 3
                $test_res = -$x * -$x
                """, "9").SetArgDisplayNames("NegatedVariableProduct"),
            new TestCaseData("""
                $x = 3
                $x -= -2
                $test_res = $x
                """, "5").SetArgDisplayNames("SubtractAssignNegative"),
            new TestCaseData("""
                $a = [1, 2]
                $test_res = -$a[1]
                """, "-2").SetArgDisplayNames("NegateArrayElement"),
            new TestCaseData("""
                func F($v) {
                    return -$v
                }
                $test_res = -F(-2) + 1
                """, "-1").SetArgDisplayNames("NegateFunctionResult"),
            new TestCaseData("""
                $test_res = 0
                if (-1 < 0) $test_res = 1
                """, "1").SetArgDisplayNames("NegativeInBracelessIf"),
            new TestCaseData("""
                func F($a, $b) {
                    return $a * 10 + $b
                }
                $test_res = F(-1, -2)
                """, "-12").SetArgDisplayNames("NegativeFunctionArguments"),
            new TestCaseData("""
                func F($a, $b) {
                    return $a * 10 + $b
                }
                $test_res = F(1, 2 * -3)
                """, "4").SetArgDisplayNames("NegativeExpressionArgument"),
        ];

        public static IEnumerable<TestCaseData> ArrayLiterals() =>
        [
            new TestCaseData("$test_res = [-1, -2]", "[-1, -2]").SetArgDisplayNames("NegativeElements"),
            new TestCaseData("$test_res = [true, !true]", "[true, false]").SetArgDisplayNames("NotElement"),
        ];

        public static IEnumerable<TestCaseData> ShortCircuit() =>
        [
            new TestCaseData("$test_res = 1 == 1 && 2 == 2 && 3 == 3", "true").SetArgDisplayNames("AndChain"),
            new TestCaseData("$test_res = false || false || 1 < 2", "true").SetArgDisplayNames("OrChain"),
            new TestCaseData("$test_res = true && false || true && true", "true").SetArgDisplayNames("AndBindsTighterThanOr"),
            new TestCaseData("$test_res = true || false && false", "true").SetArgDisplayNames("OrThenAnd"),
            new TestCaseData("$test_res = -1 < 0 && !false", "true").SetArgDisplayNames("ComparisonAndNot"),
            new TestCaseData("""
                $o = undefined
                $test_res = $o != undefined && $o.$x
                """, "false").SetArgDisplayNames("AndSkipsUndefinedMember"),
            new TestCaseData("""
                $o = undefined
                $test_res = $o == undefined || $o.$x
                """, "true").SetArgDisplayNames("OrSkipsUndefinedMember"),
            new TestCaseData("""
                $o = { $x = 1 }
                $test_res = $o != undefined && $o.$x == 1
                """, "true").SetArgDisplayNames("AndEvaluatesDefinedMember"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = false && Boom()
                """, "false").SetArgDisplayNames("AndSkipsCall"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = true || Boom()
                """, "true").SetArgDisplayNames("OrSkipsCall"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = false && Boom() || true
                """, "true").SetArgDisplayNames("AndSkipsCallThenOr"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = true || Boom() && Boom()
                """, "true").SetArgDisplayNames("OrSkipsAndChain"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = false && (Boom() || Boom()) && Boom()
                """, "false").SetArgDisplayNames("AndSkipsGroupedCalls"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = 1 + 1 == 3 && Boom() == 2
                """, "false").SetArgDisplayNames("AndSkipsComparisonCall"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = [false && Boom(), true || Boom(), 1]
                """, "[false, true, 1]").SetArgDisplayNames("InArrayElements"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                func F($a, $b) {
                    return $b
                }
                $test_res = F(false && Boom(), 2)
                """, "2").SetArgDisplayNames("InFunctionArgument"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = 0
                if (!true && Boom()) $test_res = 1
                """, "0").SetArgDisplayNames("InBracelessIfCondition"),
        ];

        public static IEnumerable<TestCaseData> StringConcatenation() =>
        [
            new TestCaseData("""
                $s = "a"
                $s += "b"
                $test_res = $s
                """, "ab").SetArgDisplayNames("StringPlusString"),
            new TestCaseData("""
                $s = "a"
                $s += 1
                $test_res = $s
                """, "a1").SetArgDisplayNames("StringPlusInt"),
            new TestCaseData("""
                $s = 1
                $s += "a"
                $test_res = $s
                """, "1a").SetArgDisplayNames("IntPlusString"),
            new TestCaseData("""
                $s = 1
                $s += 2
                $test_res = $s
                """, "3").SetArgDisplayNames("IntPlusInt"),
            new TestCaseData("""
                $a = ["a"]
                $a[0] += "b"
                $test_res = $a[0]
                """, "ab").SetArgDisplayNames("ArrayElement"),
            new TestCaseData("""
                $o = { $s = "a" }
                $o.$s += "b"
                $test_res = $o.$s
                """, "ab").SetArgDisplayNames("ObjectMember"),
            new TestCaseData("""
                $a = ["b"]
                $s = "a"
                $s += $a[0]
                $test_res = $s
                """, "ab").SetArgDisplayNames("ArrayElementOperand"),
            new TestCaseData("""
                $o = { $s = "b" }
                $s = "a"
                $s += $o.$s
                $test_res = $s
                """, "ab").SetArgDisplayNames("ObjectMemberOperand"),
            new TestCaseData("""
                $a = ["a", 1]
                $a[0] += $a[1]
                $test_res = $a[0]
                """, "a1").SetArgDisplayNames("ArrayElementBothSides"),
        ];

        public static IEnumerable<TestCaseData> Modulo() =>
        [
            new TestCaseData("""
                $x = 17
                $x %= 5
                $test_res = $x
                """, "2").SetArgDisplayNames("ModuloAssign"),
            new TestCaseData("""
                $x = -7
                $x %= 3
                $test_res = $x
                """, "-1").SetArgDisplayNames("NegativeDividend"),
            new TestCaseData("""
                $a = [10]
                $a[0] %= 4
                $test_res = $a[0]
                """, "2").SetArgDisplayNames("ArrayElementModuloAssign"),
            new TestCaseData("""
                $x = 17
                $test_res = $x%5
                """, "2").SetArgDisplayNames("OperatorUnspaced"),
        ];

        public static IEnumerable<TestCaseData> Ternary() =>
        [
            new TestCaseData("$test_res = true ? 1 : 2", "1").SetArgDisplayNames("TrueCondition"),
            new TestCaseData("$test_res = false ? 1 : 2", "2").SetArgDisplayNames("FalseCondition"),
            new TestCaseData("$test_res = 1 + 1 == 2 ? 2 * 3 : 4 - 5", "6").SetArgDisplayNames("ExpressionOperands"),
            new TestCaseData("$test_res = false ? 1 : 2 + 3", "5").SetArgDisplayNames("ElseBranchExpression"),
            new TestCaseData("$test_res = (true ? 1 : 2) + 3", "4").SetArgDisplayNames("ParenthesizedThenAdd"),
            new TestCaseData("$test_res = false ? 1 : true ? 2 : 3", "2").SetArgDisplayNames("NestedElseTrue"),
            new TestCaseData("$test_res = false ? 1 : false ? 2 : 3", "3").SetArgDisplayNames("NestedElseFalse"),
            new TestCaseData("$test_res = true ? false ? 1 : 2 : 3", "2").SetArgDisplayNames("NestedThenTrue"),
            new TestCaseData("$test_res = false ? true ? 1 : 2 : 3", "3").SetArgDisplayNames("NestedInSkippedThen"),
            new TestCaseData("$test_res = false && true ? 1 : 2", "2").SetArgDisplayNames("AndCondition"),
            new TestCaseData("$test_res = true || false ? 1 : 2", "1").SetArgDisplayNames("OrCondition"),
            new TestCaseData("$test_res = -1 < 0 ? -1 : 1", "-1").SetArgDisplayNames("NegativeOperands"),
            new TestCaseData("$test_res = [true ? 1 : 2, false ? 3 : 4]", "[1, 4]").SetArgDisplayNames("InArrayElements"),
            new TestCaseData("""
                $x = 3
                $test_res = $x > 2 ? "big" : "small"
                """, "big").SetArgDisplayNames("ComparisonCondition"),
            new TestCaseData("""
                func F($a, $b) {
                    return $a * 10 + $b
                }
                $test_res = F(true ? 1 : 2, false ? 3 : 4)
                """, "14").SetArgDisplayNames("FunctionArguments"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = true ? 1 : Boom()
                """, "1").SetArgDisplayNames("SkipsFalseBranch"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = false ? Boom() : 2
                """, "2").SetArgDisplayNames("SkipsTrueBranch"),
            new TestCaseData("""
                func Boom() {
                    return $o.$x
                }
                $test_res = true ? 2 : (Boom() ? Boom() : [Boom()])
                """, "2").SetArgDisplayNames("SkipsNestedBranches"),
            new TestCaseData("""
                enum E { A, B }
                $test_res = true ? E::A : E::B
                """, "E::A").SetArgDisplayNames("EnumTrueBranch"),
            new TestCaseData("""
                enum E { A, B }
                $test_res = false ? E::A : E::B
                $test_res2 = true ? E::A : E::B
                """, "E::B").SetArgDisplayNames("EnumFalseBranch"),
        ];

        public static IEnumerable<TestCaseData> IncrementDecrement() =>
        [
            new TestCaseData("""
                $x = 1
                $test_res = $x++
                """, "1").SetArgDisplayNames("PostIncrementValue"),
            new TestCaseData("""
                $x = 1
                $y = $x++
                $test_res = $x
                """, "2").SetArgDisplayNames("PostIncrementSideEffect"),
            new TestCaseData("""
                $x = 1
                $test_res = $x--
                """, "1").SetArgDisplayNames("PostDecrementValue"),
            new TestCaseData("""
                $x = 1
                $test_res = ++$x
                """, "2").SetArgDisplayNames("PreIncrementValue"),
            new TestCaseData("""
                $x = 1
                $test_res = --$x
                """, "0").SetArgDisplayNames("PreDecrementValue"),
            new TestCaseData("""
                $x = 1
                ++$x
                $test_res = $x
                """, "2").SetArgDisplayNames("PreIncrementStatement"),
            new TestCaseData("""
                $x = 1
                --$x
                $test_res = $x
                """, "0").SetArgDisplayNames("PreDecrementStatement"),
            new TestCaseData("""
                $test_res = 0
                for ($i = 0; $i < 3; ++$i) $test_res += $i
                """, "3").SetArgDisplayNames("PreIncrementInForLoop"),
            new TestCaseData("""
                $x = 1
                $test_res = $x++ + $x
                """, "3").SetArgDisplayNames("PostIncrementThenRead"),
            new TestCaseData("""
                $x = 1
                $test_res = -++$x
                """, "-2").SetArgDisplayNames("NegatedPreIncrement"),
            new TestCaseData("""
                $x = 1
                $test_res = !$x--
                """, "false").SetArgDisplayNames("NotPostDecrement"),
            new TestCaseData("""
                $x = 0
                $test_res = [$x++, $x++, $x]
                """, "[0, 1, 2]").SetArgDisplayNames("ArrayElementOrder"),
            new TestCaseData("""
                $a = [1, 2]
                $i = 0
                $test_res = $a[$i++] + $a[$i]
                """, "3").SetArgDisplayNames("InIndexExpression"),
            new TestCaseData("""
                $a = [5]
                $y = $a[0]++
                $test_res = [$y, $a[0]]
                """, "[5, 6]").SetArgDisplayNames("ArrayElementPostIncrement"),
            new TestCaseData("""
                $o = { $n = 5 }
                $y = ++$o.$n
                $test_res = [$y, $o.$n]
                """, "[6, 6]").SetArgDisplayNames("ObjectMemberPreIncrement"),
            new TestCaseData("""
                func F($v) {
                    return $v
                }
                $x = 1
                $test_res = F(++$x) + $x
                """, "4").SetArgDisplayNames("InFunctionArgument"),
            new TestCaseData("""
                $x = 0
                $y = false ? $x++ : 5
                $y = true || $x++
                $test_res = $x
                """, "0").SetArgDisplayNames("SkippedByShortCircuit"),
        ];

        public static IEnumerable<TestCaseData> ObjectInitializer() =>
        [
            new TestCaseData("""
                $o = { $a = 1, $b = { $c = 2 } }
                $test_res = $o.$a + $o.$b.$c
                """, "3").SetArgDisplayNames("NestedObjects"),
            new TestCaseData("""
                $o = {}
                $test_res = $o
                """, "Object: []").SetArgDisplayNames("EmptyObject"),
            new TestCaseData("""
                $test_res = 0
                func F() {
                    $test_res = 5
                    return 1
                }
                $o = { $a = F() }
                """, "5").SetArgDisplayNames("FunctionCallMember"),
        ];

        public static IEnumerable<TestCaseData> DictInitializer() =>
        [
            new TestCaseData("""$test_res = ["a": 1]""", "[a, 1]").SetArgDisplayNames("SingleEntry"),
            new TestCaseData("""
                $d = ["a": 1, "b": 2]
                $test_res = $d["a"] + $d["b"]
                """, "3").SetArgDisplayNames("ReadValues"),
            new TestCaseData("""
                $d = [:]
                $d["x"] = 1
                $test_res = $d
                """, "[x, 1]").SetArgDisplayNames("EmptyDictAssign"),
            new TestCaseData("""
                $k = "x"
                $d = [$k: 1, 1 + 1: 2]
                $test_res = $d["x"] + $d["2"]
                """, "3").SetArgDisplayNames("ExpressionKeys"),
            new TestCaseData("""
                $d = [
                    "a": [1, 2],
                    "b": ["c": 3]
                ]
                $test_res = $d["a"]
                """, "[1, 2]").SetArgDisplayNames("MultiLineNested"),
            new TestCaseData("""
                $c = true
                $d = [$c ? "a" : "b": $c ? 1 : 2]
                $test_res = $d["a"]
                """, "1").SetArgDisplayNames("TernaryKeyValueTrue"),
            new TestCaseData("""
                $c = false
                $d = [$c ? "a" : "b": $c ? 1 : 2]
                $test_res = $d["b"]
                """, "2").SetArgDisplayNames("TernaryKeyValueFalse"),
        ];

        public static IEnumerable<TestCaseData> Foreach() =>
        [
            new TestCaseData("""
                $test_res = 0
                foreach ($v in [1, 2, 3]) {
                    $test_res += $v
                }
                """, "6").SetArgDisplayNames("ArrayLiteral"),
            new TestCaseData("""
                $a = [1, 2]
                $test_res = 0
                foreach ($v in $a) {
                    $test_res += $v
                }
                """, "3").SetArgDisplayNames("Variable"),
            new TestCaseData("""
                $a = [[1, 2], [3]]
                $test_res = 0
                foreach ($v in $a[0]) {
                    $test_res += $v
                }
                """, "3").SetArgDisplayNames("IndexedArray"),
            new TestCaseData("""
                func F() {
                    return [4, 5]
                }
                $test_res = 0
                foreach ($v in F()) {
                    $test_res += $v
                }
                """, "9").SetArgDisplayNames("FunctionResult"),
            new TestCaseData("""
                $test_res = ""
                foreach ($kv in ["a": 1, "b": 2]) {
                    $test_res += $kv.$key + $kv.$value
                }
                """, "a1b2").SetArgDisplayNames("DictLiteral"),
        ];

        public static IEnumerable<TestCaseData> DictLookupConcatenation() =>
        [
            new TestCaseData("""
                $d["s"] += "b"
                $test_res = $d["s"]
                """, "ab").SetArgDisplayNames("DictStringPlusString"),
            new TestCaseData("""
                $d["s"] += 1
                $test_res = $d["s"]
                """, "a1").SetArgDisplayNames("DictStringPlusInt"),
            new TestCaseData("""
                $d["n"] += "b"
                $test_res = $d["n"]
                """, "1b").SetArgDisplayNames("DictIntPlusString"),
            new TestCaseData("""
                $s = "x"
                $s += $d["s"]
                $test_res = $s
                """, "xa").SetArgDisplayNames("StringPlusDictValue"),
            new TestCaseData("""
                $n = 2
                $n += $d["s"]
                $test_res = $n
                """, "2a").SetArgDisplayNames("IntPlusDictValue"),
            new TestCaseData("""
                $d["s"] += $d["n"]
                $test_res = $d["s"]
                """, "a1").SetArgDisplayNames("DictValuePlusDictValue"),
        ];

        public static IEnumerable<TestCaseData> DivisionByZero() =>
        [
            new TestCaseData("$test_res = 5 / 0").SetArgDisplayNames("Divide"),
            new TestCaseData("$test_res = 5 % 0").SetArgDisplayNames("Modulo"),
            new TestCaseData("$test_res = 1 + 5 / (1 - 1)").SetArgDisplayNames("DivideByExpression"),
            new TestCaseData("""
                $x = 5
                $x /= 0
                """).SetArgDisplayNames("DivideAssign"),
            new TestCaseData("""
                $x = 5
                $x %= 0
                """).SetArgDisplayNames("ModuloAssign"),
            new TestCaseData("""
                $a = [5]
                $a[0] /= 0
                """).SetArgDisplayNames("ArrayElementDivideAssign"),
            new TestCaseData("""
                $a = [5]
                $a[0] %= 0
                """).SetArgDisplayNames("ArrayElementModuloAssign"),
        ];

        public static IEnumerable<TestCaseData> ObjectInitializerWithoutAssignment() =>
        [
            new TestCaseData("$o = { $a += 1 }").SetArgDisplayNames("CompoundAssign"),
            new TestCaseData("$o = { $a++ }").SetArgDisplayNames("Increment"),
            new TestCaseData("$o = { $a }").SetArgDisplayNames("BareMember"),
        ];

        public static IEnumerable<TestCaseData> ObjectInitializerDuplicateMember() =>
        [
            new TestCaseData("$o = { $a = 1, $a = 2 }").SetArgDisplayNames("Adjacent"),
            new TestCaseData("$o = { $a = 1, $b = 2, $a = 3 }").SetArgDisplayNames("NonAdjacent"),
        ];

        public static IEnumerable<TestCaseData> DictInitializerErrors() =>
        [
            new TestCaseData("""$d = [1, "a": 2]""", "cannot be mixed").SetArgDisplayNames("ListThenKeyed"),
            new TestCaseData("""$d = ["a": 1, 2]""", "cannot be mixed").SetArgDisplayNames("KeyedThenList"),
            new TestCaseData("""$d = ["a": 1, "a": 2]""", "Duplicate key").SetArgDisplayNames("DuplicateStringKey"),
            new TestCaseData("""$d = [1: 1, "1": 2]""", "Duplicate key").SetArgDisplayNames("DuplicateIntAndStringKey"),
        ];

        public static IEnumerable<TestCaseData> DictMissingKeyWrite() =>
        [
            new TestCaseData("""$d["b"] = 2""", "2").SetArgDisplayNames("Assign"),
            new TestCaseData("""$d["b"] += 2""", "2").SetArgDisplayNames("CompoundAssign"),
            new TestCaseData("""$d["b"]++""", "1").SetArgDisplayNames("Increment"),
        ];
    }
}
