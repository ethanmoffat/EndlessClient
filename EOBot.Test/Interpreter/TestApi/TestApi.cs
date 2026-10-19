using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
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

    public static class Collections
    {
        public static int Sum(params int[] values) => values.Sum();

        public static string Join(string separator, params string[] values) => string.Join(separator, values);

        public static string Choose(int a, int b) => "fixed";

        public static string Choose(params int[] values) => $"params {values.Length}";

        public static int CountAll(params IVariable[] values) => values.Length;

        public static string Specific(int[] values) => "array";

        public static string Specific(IEnumerable<int> values) => "enumerable";

        public static string Unrelated(int[] values) => "array";

        public static string Unrelated(List<int> values) => "list";

        public static int SumList(List<int> values) => values.Sum();

        public static long SumReadOnly(IReadOnlyList<long> values) => values.Sum();

        public static int CountDistinct(ISet<int> values) => values.Count;

        public static string QueueFirst(Queue<string> values) => values.Peek();

        public static string StackTop(Stack<string> values) => values.Peek();

        public static int SumCollection(Collection<int> values) => values.Sum();

        public static int SumNested(List<int[]> values) => values.Sum(x => x.Sum());

        public static void Append(IList<IVariable> values, IVariable value) => values.Add(value);

        public static IEnumerable<int> Range(int count) => Enumerable.Range(0, count);

        public static string[] Letters() => ["a", "b"];

        public static List<int[]> Grid() => [[1, 2], [3]];

        public static Dictionary<string, IVariable> Lookup() => new() { ["a"] = new IntVariable(1) };
    }

    public class Counter
    {
        public int Count { get; private set; }

        public int Increment() => ++Count;
    }

    public static class Settings
    {
        private static int _reads;

        public static string Name => "test";

        public static TestColor Color => TestColor.Blue;

        public static int Reads => ++_reads;

        public static int WriteOnly { private get; set; }

        public static DateTime Started => DateTime.MinValue;

        public static string Throws => throw new BotScriptErrorException("property failed");
    }

    public static class Outer
    {
        public enum Inner
        {
            A,
            B,
        }

        public static class Nested
        {
            public enum Deeper
            {
                X,
                Y,
            }

            public static int Triple(int value) => value * 3;

            public static int Quadruple(int value) => value * 4;
        }
    }

    public static class Unsupported
    {
        public static DateTime Now() => DateTime.Now;

        public static void Take(DateTime value)
        {
        }

        public static void TakeImmutable(ImmutableList<int> values)
        {
        }

        public static void TakeChars(List<char> values)
        {
        }

        public static Dictionary<string, int> ReturnDictionary() => [];
    }
}
