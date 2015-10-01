namespace EOLib.Data
{
	internal class ClassRecordFactory : IDataRecordFactory
	{
		public IDataRecord CreateRecord(int id)
		{
			return new ClassRecord(id);
		}
	}
}
