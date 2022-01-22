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
                throw new ArgumentException("Calling parameterless function with parameters", nameof(parameters));

            _referenceFunc();
        }
    }

    public class VoidFunctionRef<TParam1> : ICallable
    {
        private readonly Action<TParam1> _referenceFunc;

        public string StringValue { get; }

        public VoidFunctionRef(string functionName, Action<TParam1> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 1)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            _referenceFunc((TParam1)parameters[0]);
        }
    }

    public class VoidFunctionRef<TParam1, TParam2> : ICallable
    {
        private readonly Action<TParam1, TParam2> _referenceFunc;

        public string StringValue { get; }

        public VoidFunctionRef(string functionName, Action<TParam1, TParam2> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 2)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            _referenceFunc((TParam1)parameters[0], (TParam2)parameters[1]);
        }
    }

    public class VoidFunctionRef<TParam1, TParam2, TParam3> : ICallable
    {
        private readonly Action<TParam1, TParam2, TParam3> _referenceFunc;

        public string StringValue { get; }

        public VoidFunctionRef(string functionName, Action<TParam1, TParam2, TParam3> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            _referenceFunc((TParam1)parameters[0], (TParam2)parameters[1], (TParam3)parameters[2]);
        }
    }

    public class VoidFunctionRef<TParam1, TParam2, TParam3, TParam4> : ICallable
    {
        private readonly Action<TParam1, TParam2, TParam3, TParam4> _referenceFunc;

        public string StringValue { get; }

        public VoidFunctionRef(string functionName, Action<TParam1, TParam2, TParam3, TParam4> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling function with wrong number of parameters", nameof(parameters));

            _referenceFunc((TParam1)parameters[0], (TParam2)parameters[1], (TParam3)parameters[2], (TParam4)parameters[3]);
        }
    }

    public class VoidFunctionRef<TParam1, TParam2, TParam3, TParam4, TParam5> : ICallable
    {
        private readonly Action<TParam1, TParam2, TParam3, TParam4, TParam5> _referenceFunc;

        public string StringValue { get; }

        public VoidFunctionRef(string functionName, Action<TParam1, TParam2, TParam3, TParam4, TParam5> referenceFunc)
        {
            StringValue = functionName;
            _referenceFunc = referenceFunc;
        }

        public void Call(params IIdentifiable[] parameters)
        {
            if (parameters.Length != 0)
                throw new ArgumentException("Calling VoidVoidFunction with wrong number of parameters", nameof(parameters));

            _referenceFunc((TParam1)parameters[0], (TParam2)parameters[1], (TParam3)parameters[2], (TParam4)parameters[3], (TParam5)parameters[4]);
        }
    }
}
