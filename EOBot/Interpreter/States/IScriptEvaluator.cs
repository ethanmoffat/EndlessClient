namespace EOBot.Interpreter.States
{
    public interface IScriptEvaluator
    {
        bool Evaluate(ProgramState input);
    }
}
