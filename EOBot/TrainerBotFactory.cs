namespace EOBot
{
    public class TrainerBotFactory : IBotFactory
    {
        private readonly ArgumentsParser _parser;

        public TrainerBotFactory(ArgumentsParser parser)
        {
            _parser = parser;
        }

        public IBot CreateBot(int index)
        {
            return new TrainerBot(index, _parser.Account, _parser.Password, _parser.Character);
        }
    }
}
