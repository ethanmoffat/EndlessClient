using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter;
using EOBot.Interpreter.Variables;

namespace EOBot.Test.Interpreter.TestApi
{
    public enum TestColor
    {
        Red,
        Green,
        Blue,
    }

    public static class StaticMath
    {
        public static int Add(int a, int b) => a + b;

        public static string Describe(int value) => $"int {value}";

        public static string Describe(string value) => $"string {value}";

        public static string Describe(int a, int b) => $"ints {a} {b}";

        public static int Optional(int a, int b = 10) => a + b;

        public static int OptionalWithoutDefault([Optional] int a) => a;

        public static int Narrow(byte value) => value;

        public static string Pick(byte value) => "byte";

        public static string Pick(long value) => "long";

        public static long Big() => (1L << 32) + 5;

        public static ulong BigUnsigned() => ulong.MaxValue;

        public static TestColor GetColor() => TestColor.Green;

        public static string ColorName(TestColor color) => color.ToString();

        public static int Count(List<IVariable> values) => values.Count;

        public static List<IVariable> Pair(IVariable a, IVariable b) => [a, b];

        public static async Task<int> DelayedValue(int value, CancellationToken ct)
        {
            await Task.Delay(1, ct);
            return value;
        }

        public static async Task<int> DelayedSum(CancellationToken ct, int a, int b)
        {
            await Task.Delay(1, ct);
            return a + b;
        }

        public static void Noop()
        {
        }

        public static void Fail(string message) => throw new BotScriptErrorException(message);
    }

    public class Counter
    {
        public int Count { get; private set; }

        public int Increment() => ++Count;
    }

    public static class Unsupported
    {
        public static DateTime Now() => DateTime.Now;

        public static void Take(DateTime value)
        {
        }
    }
}
