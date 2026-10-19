using System;
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
            var candidates = new List<Binding>();
            foreach (var method in _overloads)
            {
                var binding = BindNormalForm(method, parameterValues, ct);
                if (binding == null && IsParamsMethod(method))
                    binding = BindExpandedForm(method, parameterValues, ct);

                if (binding != null)
                    candidates.Add(binding);
            }

            if (candidates.Count == 0)
            {
                var provided = string.Join(", ", parameterValues.Select(x => x.TypeName));
                throw new BotScriptErrorException($"No overload of {Name} accepts ({provided}). Available: {string.Join("; ", _overloads.Select(FormatSignature))}");
            }

            var bestScore = candidates.Max(x => x.Score);
            var best = candidates.Where(x => x.Score == bestScore).ToList();

            var mostSpecific = best.Where(x => best.All(y => y == x || IsMoreSpecific(x, y))).ToList();
            if (mostSpecific.Count == 1)
                best = mostSpecific;

            if (best.Count > 1 && best.Any(x => !x.Expanded))
                best = best.Where(x => !x.Expanded).ToList();

            if (best.Count > 1)
                throw new BotScriptErrorException($"Call to {Name} is ambiguous between: {string.Join("; ", best.Select(x => FormatSignature(x.Method)))}");

            return (best[0].Method, best[0].Args);
        }

        private Binding BindNormalForm(MethodInfo method, IReadOnlyList<IVariable> parameterValues, CancellationToken ct)
        {
            var methodParamDeclarations = method.GetParameters();
            var scriptParamTypes = methodParamDeclarations.Select(x => x.ParameterType).Where(x => x != typeof(CancellationToken)).ToList();
            if (parameterValues.Count > scriptParamTypes.Count)
                return null;

            var args = new object[methodParamDeclarations.Length];
            var score = BindParameters(methodParamDeclarations, methodParamDeclarations.Length, parameterValues, ct, args);
            return score == DotNetConverter.NoMatch
                ? null
                : new Binding(method, args, score, scriptParamTypes.Take(parameterValues.Count).ToList(), Expanded: false);
        }

        private Binding BindExpandedForm(MethodInfo method, IReadOnlyList<IVariable> parameterValues, CancellationToken ct)
        {
            var methodParamDeclarations = method.GetParameters();
            var paramsIndex = methodParamDeclarations.Length - 1;
            var fixedParamTypes = methodParamDeclarations.Take(paramsIndex).Select(x => x.ParameterType).Where(x => x != typeof(CancellationToken)).ToList();

            var args = new object[methodParamDeclarations.Length];
            var score = BindParameters(methodParamDeclarations, paramsIndex, parameterValues.Take(fixedParamTypes.Count).ToList(), ct, args);
            if (score == DotNetConverter.NoMatch)
                return null;

            var elementType = methodParamDeclarations[paramsIndex].ParameterType.GetElementType();
            var elements = Array.CreateInstance(elementType, Math.Max(parameterValues.Count - fixedParamTypes.Count, 0));
            for (int i = 0; i < elements.Length; i++)
            {
                var elementScore = _converter.TryConvert(parameterValues[fixedParamTypes.Count + i], elementType, out var element);
                if (elementScore == DotNetConverter.NoMatch)
                    return null;

                score += elementScore;
                elements.SetValue(element, i);
            }

            args[paramsIndex] = elements;
            var argumentTypes = fixedParamTypes.Concat(Enumerable.Repeat(elementType, elements.Length)).Take(parameterValues.Count).ToList();
            return new Binding(method, args, score, argumentTypes, Expanded: true);
        }

        private int BindParameters(ParameterInfo[] methodParamDeclarations, int count, IReadOnlyList<IVariable> parameterValues, CancellationToken ct, object[] args)
        {
            var score = 0;
            var scriptIndex = 0;
            for (int i = 0; i < count && score != DotNetConverter.NoMatch; i++)
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

            return score;
        }

        private static bool IsParamsMethod(MethodInfo method) =>
            method.GetParameters().LastOrDefault()?.IsDefined(typeof(ParamArrayAttribute)) == true;

        private static bool IsMoreSpecific(Binding binding, Binding other) =>
            binding.ArgumentTypes.Zip(other.ArgumentTypes).All(x => x.Second.IsAssignableFrom(x.First))
            && !binding.ArgumentTypes.SequenceEqual(other.ArgumentTypes);

        private string FormatSignature(MethodInfo method)
        {
            var parameters = method.GetParameters()
                .Where(x => x.ParameterType != typeof(CancellationToken))
                .Select(FormatParameter);
            return $"{Name}({string.Join(", ", parameters)})";
        }

        private string FormatParameter(ParameterInfo parameter)
        {
            var typeName = parameter.IsDefined(typeof(ParamArrayAttribute))
                ? $"{_converter.GetScriptTypeName(parameter.ParameterType.GetElementType())}..."
                : _converter.GetScriptTypeName(parameter.ParameterType);
            return $"{typeName} {parameter.Name}{(parameter.IsOptional ? "?" : string.Empty)}";
        }

        private sealed record Binding(MethodInfo Method, object[] Args, int Score, IReadOnlyList<Type> ArgumentTypes, bool Expanded);
    }
}
