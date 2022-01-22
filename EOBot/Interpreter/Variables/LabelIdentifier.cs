namespace EOBot.Interpreter.Variables
{
    public class LabelIdentifier : IIdentifiable
    {
        private readonly string _value;

        public LabelIdentifier(string value) => _value = value;

        public string StringValue => _value;
    }
}
