using System;
using System.Threading;
using System.Threading.Tasks;

namespace EOBot.Interpreter.Variables
{
    public class AsyncVoidFunction : IAsyncCallable
    {
        private readonly Func<CancellationToken, Task> _referenceFunc;

        public string StringValue { get; }

        public AsyncVoidFunction(string functionName, Func<CancellationToken, Task> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task CallAsync(CancellationToken ct, params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException($"Calling parameterless function '{StringValue}' with parameters");

            return _referenceFunc(ct);
        }
    }

    public class AsyncVoidFunction<TParam1> : IAsyncCallable
    {
        private readonly Func<TParam1, CancellationToken, Task> _referenceFunc;

        public string StringValue { get; }

        public AsyncVoidFunction(string functionName, Func<TParam1, CancellationToken, Task> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task CallAsync(CancellationToken ct, params IIdentifiable[] parameters)
        {
            if (parameters.Length != 1)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            // This has to be dynamic because otherwise there is an InvalidCastException trying to use a user-defined conversion operator when the source type is an interface
            return _referenceFunc((TParam1)(dynamic)parameters[0], ct);
        }
    }

    public class AsyncVoidFunction<TParam1, TParam2> : IAsyncCallable
    {
        private readonly Func<TParam1, TParam2, CancellationToken, Task> _referenceFunc;

        public string StringValue { get; }

        public AsyncVoidFunction(string functionName, Func<TParam1, TParam2, CancellationToken, Task> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task CallAsync(CancellationToken ct, params IIdentifiable[] parameters)
        {
            if (parameters.Length != 2)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], ct);
        }
    }

    public class AsyncVoidFunction<TParam1, TParam2, TParam3> : IAsyncCallable
    {
        private readonly Func<TParam1, TParam2, TParam3, CancellationToken, Task> _referenceFunc;

        public string StringValue { get; }

        public AsyncVoidFunction(string functionName, Func<TParam1, TParam2, TParam3, CancellationToken, Task> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task CallAsync(CancellationToken ct, params IIdentifiable[] parameters)
        {
            if (parameters.Length != 3)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2], ct);
        }
    }

    public class AsyncVoidFunction<TParam1, TParam2, TParam3, TParam4> : IAsyncCallable
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task> _referenceFunc;

        public string StringValue { get; }

        public AsyncVoidFunction(string functionName, Func<TParam1, TParam2, TParam3, TParam4, CancellationToken, Task> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task CallAsync(CancellationToken ct, params IIdentifiable[] parameters)
        {
            if (parameters.Length != 4)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2], (TParam4)(dynamic)parameters[3], ct);
        }
    }

    public class AsyncVoidFunction<TParam1, TParam2, TParam3, TParam4, TParam5> : IAsyncCallable
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task> _referenceFunc;

        public string StringValue { get; }

        public AsyncVoidFunction(string functionName, Func<TParam1, TParam2, TParam3, TParam4, TParam5, CancellationToken, Task> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task CallAsync(CancellationToken ct, params IIdentifiable[] parameters)
        {
            if (parameters.Length != 5)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2], (TParam4)(dynamic)parameters[3], (TParam5)(dynamic)parameters[4], ct);
        }
    }
}
