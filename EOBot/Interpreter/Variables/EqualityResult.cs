namespace EOBot.Interpreter.Variables
{
    /// <summary>
    /// The result of a loose equality (==) comparison. Ok is false when the values cannot be compared, with the reason in Reason.
    /// </summary>
    public readonly record struct EqualityResult(bool Ok, bool Equal, string Reason)
    {
        public static EqualityResult Create(bool equal) => new(true, equal, string.Empty);

        public static EqualityResult IncompatibleComparison(IVariable lhs, IVariable rhs) =>
            new(false, false, $"{lhs.TypeName} and {rhs.TypeName} cannot be compared with ==");

        public static EqualityResult SelfContaining(IVariable variable) =>
            new(false, false, $"{variable.TypeName} contains itself and cannot be compared with ==");
    }
}
