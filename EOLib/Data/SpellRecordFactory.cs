namespace EOLib.Data
{
	class SpellRecordFactory : IDataRecordFactory
	{
		public IDataRecord CreateRecord(int id)
		{
			return new SpellRecord(id);
		}
	}
}
