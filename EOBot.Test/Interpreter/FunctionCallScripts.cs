using System.Collections.Generic;
using NUnit.Framework;

namespace EOBot.Test.Interpreter
{
    public static class FunctionCallScripts
    {
        public static IEnumerable<TestCaseData> Recursion() =>
        [
            new TestCaseData("""
                func Fact($n) {
                    if ($n <= 1) {
                        return 1
                    }
                    return $n * Fact($n - 1)
                }
                $res = Fact(5)
                """, "120").SetArgDisplayNames("Factorial"),
            new TestCaseData("""
                func Fib($n) {
                    if ($n < 2) {
                        return $n
                    }
                    return Fib($n - 1) + Fib($n - 2)
                }
                $res = Fib(10)
                """, "55").SetArgDisplayNames("Fibonacci"),
            new TestCaseData("""
                $res = ""
                func Down($n) {
                    if ($n == 0) {
                        return 0
                    }
                    $res += "b" + $n
                    $x = Down($n - 1)
                    $res += "a" + $n
                    return 0
                }
                $x = Down(3)
                """, "b3b2b1a1a2a3").SetArgDisplayNames("SideEffectOrder"),
        ];

        public static IEnumerable<TestCaseData> NestedFunctions() =>
        [
            new TestCaseData("""
                func Outer($x) {
                    func Inner($y) {
                        return $y * 2
                    }
                    return Inner($x) + 1
                }
                $res = Outer(5)
                """, "11").SetArgDisplayNames("Simple"),
            new TestCaseData("""
                func Outer($n) {
                    func Fact($k) {
                        if ($k <= 1) {
                            return 1
                        }
                        return $k * Fact($k - 1)
                    }
                    return Fact($n)
                }
                $res = Outer(5)
                """, "120").SetArgDisplayNames("RecursiveNested"),
            new TestCaseData("""
                func Outer($n) {
                    func Inner() {
                        return 1
                    }
                    if ($n == 0) {
                        return Inner()
                    }
                    return Inner() + Outer($n - 1)
                }
                $res = Outer(3)
                """, "4").SetArgDisplayNames("RecursiveEnclosing"),
            new TestCaseData("""
                func Inner() {
                    return 1
                }
                func Outer() {
                    func Inner() {
                        return 2
                    }
                    return Inner()
                }
                $res = Outer() * 10 + Inner()
                """, "21").SetArgDisplayNames("ShadowsTopLevel"),
        ];

        public static IEnumerable<TestCaseData> NoReturn() =>
        [
            new TestCaseData("""
                func F() {
                    $x = 1
                }
                $res = F()
                """).SetArgDisplayNames("Simple"),
            new TestCaseData("""
                func G() {
                    return 5
                }
                func F() {
                    $y = G()
                }
                $res = F()
                """).SetArgDisplayNames("InnerCallReturns"),
        ];

        public static IEnumerable<TestCaseData> VoidUsedAsValue() =>
        [
            new TestCaseData("$x = Void()").SetArgDisplayNames("Assignment"),
            new TestCaseData("$x = 1 + Void()").SetArgDisplayNames("Operand"),
            new TestCaseData("""
                if (Void()) {
                    $x = 1
                }
                """).SetArgDisplayNames("Condition"),
        ];
    }
}
