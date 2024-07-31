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
        NotMatch
    }
}