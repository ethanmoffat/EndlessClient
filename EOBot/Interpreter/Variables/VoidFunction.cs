using System;

namespace EOBot.Interpreter.Variables
{
    public class VoidFunction : ICallable
    {
        private readonly Action _referenceFunc;

        public string StringValue { get; }

        public VoidFunction(string functionName, Action referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException($"Calling parameterless function '{StringValue}' with parameters");

            _referenceFunc();
        }
    }

    public class VoidFunction<TParam1> : ICallable
    {
        private readonly Action<TParam1> _referenceFunc;

        public string StringValue { get; }

        public VoidFunction(string functionName, Action<TParam1> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 1)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            // This has to be dynamic because otherwise there is an InvalidCastException trying to use a user-defined conversion operator when the source type is an interface
            _referenceFunc((TParam1)(dynamic)parameters[0]);
        }
    }

    public class VoidFunction<TParam1, TParam2> : ICallable
    {
        private readonly Action<TParam1, TParam2> _referenceFunc;

        public string StringValue { get; }

        public VoidFunction(string functionName, Action<TParam1, TParam2> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 2)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1]);
        }
    }

    public class VoidFunction<TParam1, TParam2, TParam3> : ICallable
    {
        private readonly Action<TParam1, TParam2, TParam3> _referenceFunc;

        public string StringValue { get; }

        public VoidFunction(string functionName, Action<TParam1, TParam2, TParam3> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 3)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2]);
        }
    }

    public class VoidFunction<TParam1, TParam2, TParam3, TParam4> : ICallable
    {
        private readonly Action<TParam1, TParam2, TParam3, TParam4> _referenceFunc;

        public string StringValue { get; }

        public VoidFunction(string functionName, Action<TParam1, TParam2, TParam3, TParam4> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 4)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2], (TParam4)(dynamic)parameters[3]);
        }
    }

    public class VoidFunction<TParam1, TParam2, TParam3, TParam4, TParam5> : ICallable
    {
        private readonly Action<TParam1, TParam2, TParam3, TParam4, TParam5> _referenceFunc;

        public string StringValue { get; }

        public VoidFunction(string functionName, Action<TParam1, TParam2, TParam3, TParam4, TParam5> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 5)
                throw new ArgumentException($"Calling function '{StringValue}' with wrong number of parameters");

            _referenceFunc((TParam1)(dynamic)parameters[0], (TParam2)(dynamic)parameters[1], (TParam3)(dynamic)parameters[2], (TParam4)(dynamic)parameters[3], (TParam5)(dynamic)parameters[4]);
        }
    }
}