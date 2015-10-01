namespace EOLib.Data
{
	internal class ItemRecordFactory : IDataRecordFactory
	{
		public IDataRecord CreateRecord(int id)
		{
			return new ItemRecord(id);
		}
	}
}
