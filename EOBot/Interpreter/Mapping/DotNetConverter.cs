using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.Mapping
{
    /// <summary>
    /// Converts values between script variables and .NET types.
    /// </summary>
    public class DotNetConverter
    {
        public const int NoMatch = -1;

        // Scores for how well a script value matches a .NET parameter type, used to pick the best overload
        private const int ExactMatch = 3;
        private const int WideningMatch = 2;
        private const int LooseMatch = 1;
        private const int TruncatingMatch = 0;

        private static readonly HashSet<Type> IntegerTypes = [typeof(int), typeof(long), typeof(short), typeof(byte), typeof(sbyte), typeof(uint), typeof(ushort), typeof(ulong)];

        private readonly Dictionary<Type, EnumDefinition> _enumDefinitions = [];

        /// <summary>
        /// Whether a method's parameter and return types can all be converted. <see cref="CancellationToken"/> parameters are supplied by the interpreter.
        /// </summary>
        public bool IsSupported(MethodInfo method)
        {
            return !method.ContainsGenericParameters
                && method.GetParameters().All(x => IsSupportedType(x.ParameterType) || x.ParameterType == typeof(CancellationToken))
                && IsSupportedReturnType(method.ReturnType);
        }

        /// <summary>
        /// Converts a script value to the target type. Returns a score for how well the value matches the type (higher is better), or <see cref="NoMatch"/>.
        /// </summary>
        public int TryConvert(IVariable value, Type target, out object result)
        {
            (var score, result) = target switch
            {
                _ when typeof(IVariable).IsAssignableFrom(target) => ConvertVariable(value, target),
                _ when target == typeof(string) => ConvertString(value),
                _ when target == typeof(bool) => ConvertBool(value),
                { IsEnum: true } => ConvertEnum(value, target),
                _ when IntegerTypes.Contains(target) => ConvertInteger(value, target),
                _ when target == typeof(List<IVariable>) => ConvertList(value),
                _ when target == typeof(Dictionary<string, IVariable>) => ConvertDictionary(value),
                _ => (NoMatch, null),
            };

            return score;
        }

        /// <summary>
        /// Converts a .NET value to a script variable.
        /// </summary>
        public IVariable ToVariable(object value)
        {
            switch (value)
            {
                case null: return UndefinedVariable.Instance;
                case IVariable variable: return variable;
                case string s: return new StringVariable(s);
                case bool b: return new BoolVariable(b);
                case Enum e: return ToEnumVariable(e);
                case List<IVariable> list: return new ArrayVariable(list);
                case Dictionary<string, IVariable> dict: return new DictVariable(dict);
                case IConvertible when IntegerTypes.Contains(value.GetType()):
                    var truncated = value is ulong ul ? unchecked((int)ul) : unchecked((int)Convert.ToInt64(value));
                    return new IntVariable(truncated, truncated: Convert.ToDecimal(value) != truncated);
                default:
                    throw new BotScriptErrorException($"Values of type {value.GetType().FullName} cannot be used in scripts");
            }
        }

        public string GetScriptTypeName(Type type)
        {
            if (type == typeof(string)) return Keywords.String;
            if (type == typeof(bool)) return Keywords.Bool;
            if (type.IsEnum) return type.Name;
            if (IntegerTypes.Contains(type)) return Keywords.Int;
            if (type == typeof(List<IVariable>)) return Keywords.Array;
            if (type == typeof(Dictionary<string, IVariable>)) return Keywords.Dict;
            return type.Name;
        }

        private EnumVariable ToEnumVariable(Enum value)
        {
            var enumType = value.GetType();
            if (!_enumDefinitions.TryGetValue(enumType, out var definition))
                _enumDefinitions[enumType] = definition = EnumDefinition.FromType(enumType);

            return new EnumVariable(definition, Enum.GetName(enumType, value) ?? value.ToString(), Convert.ToInt32(value));
        }

        private static (int Score, object Result) ConvertVariable(IVariable value, Type target)
        {
            if (!target.IsInstanceOfType(value))
                return (NoMatch, null);

            return (target == value.GetType() ? ExactMatch : LooseMatch, value);
        }

        private static (int Score, object Result) ConvertString(IVariable value) =>
            value is StringVariable sv ? (ExactMatch, sv.Value) : (NoMatch, null);

        private static (int Score, object Result) ConvertBool(IVariable value) =>
            value is BoolVariable bv ? (ExactMatch, bv.Value) : (NoMatch, null);

        private static (int Score, object Result) ConvertEnum(IVariable value, Type target)
        {
            if (value is EnumVariable { Definition.DotNetType: not null } ev)
                return ev.Definition.DotNetType == target ? (ExactMatch, Enum.ToObject(target, ev.Value)) : (NoMatch, null);

            return value is IntVariable iv ? (LooseMatch, Enum.ToObject(target, iv.Value)) : (NoMatch, null);
        }

        private static (int Score, object Result) ConvertInteger(IVariable value, Type target)
        {
            if (value is not IntVariable iv)
                return (NoMatch, null);

            var result = TruncateInteger(iv.Value, target);
            if (Convert.ToDecimal(result) != iv.Value)
                return (TruncatingMatch, result);

            return (target == typeof(int) && value is not EnumVariable ? ExactMatch : WideningMatch, result);
        }

        private static object TruncateInteger(int value, Type target) => Type.GetTypeCode(target) switch
        {
            TypeCode.Int32 => value,
            TypeCode.Int64 => (long)value,
            TypeCode.Int16 => unchecked((short)value),
            TypeCode.Byte => unchecked((byte)value),
            TypeCode.SByte => unchecked((sbyte)value),
            TypeCode.UInt32 => unchecked((uint)value),
            TypeCode.UInt16 => unchecked((ushort)value),
            TypeCode.UInt64 => unchecked((ulong)value),
            _ => throw new ArgumentException($"{target} is not an integer type", nameof(target)),
        };

        private static (int Score, object Result) ConvertList(IVariable value) =>
            value is ArrayVariable av ? (ExactMatch, av.Value) : (NoMatch, null);

        private static (int Score, object Result) ConvertDictionary(IVariable value) =>
            value is DictVariable dv ? (ExactMatch, dv.Value) : (NoMatch, null);

        private static bool IsSupportedReturnType(Type type)
        {
            if (type == typeof(void) || type == typeof(Task))
                return true;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                return IsSupportedType(type.GetGenericArguments()[0]);

            return IsSupportedType(type);
        }

        private static bool IsSupportedType(Type type) =>
            type == typeof(string)
            || type == typeof(bool)
            || type.IsEnum
            || IntegerTypes.Contains(type)
            || type == typeof(List<IVariable>)
            || type == typeof(Dictionary<string, IVariable>)
            || typeof(IVariable).IsAssignableFrom(type);
    }
}
