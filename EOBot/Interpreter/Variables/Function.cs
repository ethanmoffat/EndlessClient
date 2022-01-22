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
                throw new ArgumentException("Calling parameterless function with parameters", nameof(parameters));

            return _referenceFunc();
        }
    }

    public class FunctionRef<TParam1, T> : ICallable<T>
    {
        private readonly Func<TParam1, T> _referenceFunc;

        public string StringValue { get; }

        public FunctionRef(string functionName, Func<TParam1, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 1)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            return _referenceFunc((TParam1)parameters[0]);
        }
    }

    public class FunctionRef<TParam1, TParam2, T> : ICallable<T>
    {
        private readonly Func<TParam1, TParam2, T> _referenceFunc;

        public string StringValue { get; }

        public FunctionRef(string functionName, Func<TParam1, TParam2, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 2)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            return _referenceFunc((TParam1)parameters[0], (TParam2)parameters[1]);
        }
    }

    public class FunctionRef<TParam1, TParam2, TParam3, T> : ICallable<T>
    {
        private readonly Func<TParam1, TParam2, TParam3, T> _referenceFunc;

        public string StringValue { get; }

        public FunctionRef(string functionName, Func<TParam1, TParam2, TParam3, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            return _referenceFunc((TParam1)parameters[0], (TParam2)parameters[1], (TParam3)parameters[2]);
        }
    }

    public class FunctionRef<TParam1, TParam2, TParam3, TParam4, T> : ICallable<T>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, T> _referenceFunc;

        public string StringValue { get; }

        public FunctionRef(string functionName, Func<TParam1, TParam2, TParam3, TParam4, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            return _referenceFunc((TParam1)parameters[0], (TParam2)parameters[1], (TParam3)parameters[2], (TParam4)parameters[3]);
        }
    }

    public class FunctionRef<TParam1, TParam2, TParam3, TParam4, TParam5, T> : ICallable<T>
    {
        private readonly Func<TParam1, TParam2, TParam3, TParam4, TParam5, T> _referenceFunc;

        public string StringValue { get; }

        public FunctionRef(string functionName, Func<TParam1, TParam2, TParam3, TParam4, TParam5, T> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public T Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            return _referenceFunc((TParam1)parameters[0], (TParam2)parameters[1], (TParam3)parameters[2], (TParam4)parameters[3], (TParam5)parameters[4]);
        }
    }
}
