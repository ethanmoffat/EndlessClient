namespace EOBot.Interpreter.States
{
    public enum EvalResult
    {
        /// <summary>
        /// The result of the evaluation succeeded
        /// </summary>
        Ok,
        /// <summary>
        /// The result of the evaluation failed
        /// </summary>
        Failed,
        /// <summary>
        /// The evaluator was not a match for the program's current execution point
        /// </summary>
        NotMatch,
        /// <summary>
        /// The operation was cancelled by the user
        /// </summary>
        Cancelled,
        /// <summary>
        /// The result is a control flow operation that should be handled at a higher level of the evaluation stack
        /// </summary>
        ControlFlow
    }
}
