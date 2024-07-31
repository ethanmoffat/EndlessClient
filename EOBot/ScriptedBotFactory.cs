namespace EOBot
{
    public class ScriptedBotFactory : IBotFactory
    {
        private readonly ArgumentsParser _parser;

        public ScriptedBotFactory(ArgumentsParser parser)
        {
            _parser = parser;
        }

        public IBot CreateBot(int index)
        {
            return new ScriptedBot(index, _parser);
        }
    }
}