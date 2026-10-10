using System.Collections.Generic;
using NUnit.Framework;

namespace EOBot.Test.Interpreter
{
    public static class ControlFlowScripts
    {
        public static IEnumerable<TestCaseData> Return() =>
        [
            new TestCaseData("""
                func F($x) {
                    if ($x > 0) {
                        return 1
                    }
                    return 2
                }
                $res = F(5)
                """, "1").SetArgDisplayNames("IfTrueBranch"),
            new TestCaseData("""
                func F($x) {
                    if ($x > 0) {
                        return 1
                    }
                    return 2
                }
                $res = F(0)
                """, "2").SetArgDisplayNames("AfterIf"),
            new TestCaseData("""
                func F() {
                    $i = 0
                    while (true) {
                        $i++
                        if ($i == 3) {
                            return $i
                        }
                    }
                    return -1
                }
                $res = F()
                """, "3").SetArgDisplayNames("InsideWhile"),
            new TestCaseData("""
                func F() {
                    for ($i = 0; $i < 10; $i++) {
                        if ($i == 4) {
                            return $i
                        }
                    }
                    return -1
                }
                $res = F()
                """, "4").SetArgDisplayNames("InsideFor"),
            new TestCaseData("""
                func F($arr) {
                    foreach ($v in $arr) {
                        if ($v > 1) {
                            return $v
                        }
                    }
                    return 0
                }
                $res = F([1, 2, 3])
                """, "2").SetArgDisplayNames("InsideForeach"),
            new TestCaseData("""
                func F($x) {
                    if ($x == 0) {
                        return 0
                    } else if ($x == 1) {
                        return 1
                    } else {
                        return 2
                    }
                    return 3
                }
                $res = F(1)
                """, "1").SetArgDisplayNames("ElseIfBranch"),
            new TestCaseData("""
                func F($x) {
                    if ($x == 0) {
                        return 0
                    } else if ($x == 1) {
                        return 1
                    } else {
                        return 2
                    }
                    return 3
                }
                $res = F(5)
                """, "2").SetArgDisplayNames("ElseBranch"),
            new TestCaseData("""
                func F() {
                    while (true) {
                        while (true) {
                            return 7
                        }
                    }
                }
                $res = F()
                """, "7").SetArgDisplayNames("NestedWhile"),
        ];

        public static IEnumerable<TestCaseData> BracelessLoop() =>
        [
            new TestCaseData("""
                $i = 0
                while ($i < 3) $i++
                $res = $i
                """, "3").SetArgDisplayNames("While"),
            new TestCaseData("""
                while (false) $x = 1
                $res = 5
                """, "5").SetArgDisplayNames("WhileFalse"),
            new TestCaseData("""
                $sum = 0
                foreach ($v in [1, 2]) $sum += $v
                $res = $sum
                """, "3").SetArgDisplayNames("Foreach"),
            new TestCaseData("""
                foreach ($v in []) $x = 1
                $res = 5
                """, "5").SetArgDisplayNames("ForeachEmpty"),
        ];
    }
}
