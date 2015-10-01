namespace EOLib.Data
{
	public class ClassRecordFactory : IDataRecordFactory
	{
		public IDataRecord CreateRecord(int id)
		{
			return new ClassRecord(id);
		}
	}
}
