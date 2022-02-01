using System;
using System.Threading.Tasks;

namespace EOBot.Interpreter.Variables
{
    public class AsyncFunction<T> : IAsyncCallable<T>
    {
        private readonly Func<Task<T>> _referenceFunc;

        public string StringValue { get; }

        public AsyncFunction(string functionName, Func<Task<T>> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task<T> CallAsync(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling parameterless function with parameters", nameof(parameters));

            return _referenceFunc();
        }
    }

    public class AsyncFunction<TParam1, T> : IAsyncCallable<T>
    {
        private readonly Func<TParam1, Task<T>> _referenceFunc;

        public string StringValue { get; }

        public AsyncFunction(string functionName, Func<TParam1, Task<T>> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task<T> CallAsync(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 1)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            // This has to be dynamic because otherwise there is an InvalidCastException trying to use a user-defined conversion operator when the source type is an interface
            return _referenceFunc((TParam1)(dynamic)parameters[0]);
        }
    }

    public class AsyncFunction<TParam1, TParam2, T> : IAsyncCallable<T>
    {
        private readonly Func<TParam1, TParam2, Task<T>> _referenceFunc;

        public string StringValue { get; }

        public AsyncFunction(string functionName, Func<TParam1, TParam2, Task<T>> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task<T> CallAsync(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 2)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1]);
        }
    }

    public class AsyncFunction<TParam1, TParam2, TParam3, T> : IAsyncCallable<T>
    {
        private readonly Func<TParam1, TParam2, TParam3, Task<T>> _referenceFunc;

        public string StringValue { get; }

        public AsyncFunction(string functionName, Func<TParam1, TParam2, TParam3, Task<T>> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task<T> CallAsync(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2]);
        }
    }

    public class AsyncFunction<TParam1, TParam2, TParam3, TParam4, T> : IAsyncCallable<T>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, Task<T>> _referenceFunc;

        public string StringValue { get; }

        public AsyncFunction(string functionName, Func<TParam1, TParam2, TParam3, TParam4, Task<T>> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task<T> CallAsync(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2], (TParam4)(dynamic)parameters[3]);
        }
    }

    public class AsyncFunction<TParam1, TParam2, TParam3, TParam4, TParam5, T> : IAsyncCallable<T>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<T>> _referenceFunc;

        public string StringValue { get; }

        public AsyncFunction(string functionName, Func<TParam1, TParam2, TParam3, TParam4, TParam5, Task<T>> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public Task<T> CallAsync(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2], (TParam4)(dynamic)parameters[3], (TParam5)(dynamic)parameters[4]);
        }
    }
}
