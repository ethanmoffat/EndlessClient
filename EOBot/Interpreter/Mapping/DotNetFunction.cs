using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using EOBot.Interpreter.Variables;

namespace EOBot.Interpreter.Mapping
{
    /// <summary>
    /// A group of static .NET method overloads callable from scripts. The overload is chosen by the arguments of each call.
    /// </summary>
    public class DotNetFunction : IIdentifiable
    {
        private readonly IReadOnlyList<MethodInfo> _overloads;
        private readonly DotNetConverter _converter;

        public string Name { get; }

        public string StringValue => Name;

        public DotNetFunction(string name, IReadOnlyList<MethodInfo> overloads, DotNetConverter converter)
        {
            Name = name;
            _overloads = overloads;
            _converter = converter;
        }

        /// <summary>
        /// Calls the best matching overload. Returns null if the method has no return value.
        /// </summary>
        public async Task<IVariable> CallAsync(IReadOnlyList<IVariable> parameters, CancellationToken ct)
        {
            var (method, args) = SelectOverload(parameters, ct);

            object result;
            try
            {
                result = method.Invoke(null, args);
            }
            catch (TargetInvocationException tie) when (tie.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                throw;
            }

            if (result is Task task)
            {
                await task.ConfigureAwait(false);

                if (!method.ReturnType.IsGenericType)
                    return null;

                result = method.ReturnType.GetProperty(nameof(Task<object>.Result)).GetValue(task);
            }
            else if (method.ReturnType == typeof(void))
            {
                return null;
            }

            return _converter.ToVariable(result);
        }

        private (MethodInfo Method, object[] Args) SelectOverload(IReadOnlyList<IVariable> parameterValues, CancellationToken ct)
        {
            var candidates = new List<(MethodInfo Method, object[] Args, int Score)>();
            foreach (var method in _overloads)
            {
                var methodParamDeclarations = method.GetParameters();

                var filteredParamDeclarations = methodParamDeclarations.Where(x => x.ParameterType != typeof(CancellationToken)).ToList();
                if (parameterValues.Count > filteredParamDeclarations.Count)
                    continue;

                var args = new object[methodParamDeclarations.Length];
                var score = 0;
                var scriptIndex = 0;
                for (int i = 0; i < methodParamDeclarations.Length && score != DotNetConverter.NoMatch; i++)
                {
                    if (methodParamDeclarations[i].ParameterType == typeof(CancellationToken))
                    {
                        args[i] = ct;
                        continue;
                    }

                    if (scriptIndex >= parameterValues.Count)
                    {
                        if (!methodParamDeclarations[i].HasDefaultValue)
                            score = DotNetConverter.NoMatch;
                        else
                            args[i] = methodParamDeclarations[i].DefaultValue;
                        continue;
                    }

                    var paramScore = _converter.TryConvert(parameterValues[scriptIndex++], methodParamDeclarations[i].ParameterType, out args[i]);
                    score = paramScore == DotNetConverter.NoMatch ? DotNetConverter.NoMatch : score + paramScore;
                }

                if (score == DotNetConverter.NoMatch)
                    continue;

                candidates.Add((method, args, score));
            }

            if (candidates.Count == 0)
            {
                var provided = string.Join(", ", parameterValues.Select(x => x.TypeName));
                throw new BotScriptErrorException($"No overload of {Name} accepts ({provided}). Available: {string.Join("; ", _overloads.Select(FormatSignature))}");
            }

            var bestScore = candidates.Max(x => x.Score);
            var best = candidates.Where(x => x.Score == bestScore).ToList();
            if (best.Count > 1)
                throw new BotScriptErrorException($"Call to {Name} is ambiguous between: {string.Join("; ", best.Select(x => FormatSignature(x.Method)))}");

            return (best[0].Method, best[0].Args);
        }

        private string FormatSignature(MethodInfo method)
        {
            var parameters = method.GetParameters()
                .Where(x => x.ParameterType != typeof(CancellationToken))
                .Select(x => $"{_converter.GetScriptTypeName(x.ParameterType)} {x.Name}{(x.IsOptional ? "?" : string.Empty)}");
            return $"{Name}({string.Join(", ", parameters)})";
        }
    }
}
