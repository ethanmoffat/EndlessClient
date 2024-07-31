using System;

namespace EOBot.Interpreter.Variables
{
    public class Function<T> : ICallable<T>
    {
        private readonly Func<T> _referenceFunc;

        public string StringValue { get; }

        public Function(string functionName, Func<T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException($"Calling parameterless function '{StringValue}' with parameters");

            return _referenceFunc();
        }
    }

    public class Function<TParam1, T> : ICallable<T>
    {
        private readonly Func<TParam1, T> _referenceFunc;

        public string StringValue { get; }

        public Function(string functionName, Func<TParam1, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 1)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            // This has to be dynamic because otherwise there is an InvalidCastException trying to use a user-defined conversion operator when the source type is an interface
            return _referenceFunc((TParam1)(dynamic)parameters[0]);
        }
    }

    public class Function<TParam1, TParam2, T> : ICallable<T>
    {
        private readonly Func<TParam1, TParam2, T> _referenceFunc;

        public string StringValue { get; }

        public Function(string functionName, Func<TParam1, TParam2, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 2)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1]);
        }
    }

    public class Function<TParam1, TParam2, TParam3, T> : ICallable<T>
    {
        private readonly Func<TParam1, TParam2, TParam3, T> _referenceFunc;

        public string StringValue { get; }

        public Function(string functionName, Func<TParam1, TParam2, TParam3, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 3)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2]);
        }
    }

    public class Function<TParam1, TParam2, TParam3, TParam4, T> : ICallable<T>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, T> _referenceFunc;

        public string StringValue { get; }

        public Function(string functionName, Func<TParam1, TParam2, TParam3, TParam4, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 4)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2], (TParam4)(dynamic)parameters[3]);
        }
    }

    public class Function<TParam1, TParam2, TParam3, TParam4, TParam5, T> : ICallable<T>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, T> _referenceFunc;

        public string StringValue { get; }

        public Function(string functionName, Func<TParam1, TParam2, TParam3, TParam4, TParam5, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 5)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            return _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2], (TParam4)(dynamic)parameters[3], (TParam5)(dynamic)parameters[4]);
        }
    }
}