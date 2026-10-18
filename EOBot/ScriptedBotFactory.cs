using EOBot.Interpreter.Hosting;

namespace EOBot
{
    public class ScriptedBotFactory : IBotFactory
    {
        private readonly ArgumentsParser _parser;
        private readonly TypeCatalog _typeCatalog;

        public ScriptedBotFactory(ArgumentsParser parser)
        {
            _parser = parser;
            _typeCatalog = TypeCatalog.FromRootAssemblies(typeof(ScriptedBotFactory).Assembly);
        }

        public IBot CreateBot(int index)
        {
            return new ScriptedBot(index, _parser, new ScriptHost(_typeCatalog));
        }
    }
}
