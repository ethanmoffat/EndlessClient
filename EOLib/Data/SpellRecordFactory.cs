namespace EOLib.Data
{
	internal class SpellRecordFactory : IDataRecordFactory
	{
		public IDataRecord CreateRecord(int id)
		{
			return new SpellRecord(id);
		}
	}
}
