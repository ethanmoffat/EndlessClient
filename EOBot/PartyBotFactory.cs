namespace EOBot
{
	class PartyBotFactory : IBotFactory
	{
		public IBot CreateBot(int index, string host, ushort port)
		{
			string name = NamesList.Get(index);
			return new PartyBot(index + 1, host, port, name);
		}
	}
}
