using EOBot.Interpreter.States;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.Extensions
{
    public static class VariableExtensions
    {
        /// <summary>
        /// Index into an array or dictionary. <paramref name="identifier"/> names the indexed variable in error messages; null for computed values.
        /// </summary>
        public static (EvalResult Result, string Reason, IVariable Variable) Index(this IVariable container, IVariable indexer, string identifier = null)
        {
            switch (container)
            {
                case ArrayVariable arrayVariable:
                    if (indexer is not IntVariable iv)
                        return (EvalResult.Failed, $"Expected array indexer {indexer} to be an integer", null);

                    if (iv.Value < 0 || iv.Value >= arrayVariable.Value.Count)
                    {
                        var arrayName = identifier == null ? "the array" : $"the array {identifier}";
                        return (EvalResult.Failed, $"Index {iv.Value} is out of range of {arrayName} (size {arrayVariable.Value.Count})", null);
                    }

                    return (EvalResult.Ok, string.Empty, arrayVariable.Value[iv.Value]);
                case DictVariable dictVariable:
                    return (EvalResult.Ok, string.Empty, dictVariable.Value.TryGetValue(indexer.StringValue, out var entry) ? entry : UndefinedVariable.Instance);
                default:
                    var subject = identifier == null ? "Value" : $"Identifier {identifier}";
                    return (EvalResult.Failed, $"{subject} cannot be indexed: {container.TypeName} is not an array or dictionary", null);
            }
        }
    }
}
