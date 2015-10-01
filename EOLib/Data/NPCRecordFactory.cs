namespace EOLib.Data
{
	public class NPCRecordFactory : IDataRecordFactory
	{
		public IDataRecord CreateRecord(int id)
		{
			return new NPCRecord(id);
		}
	}
}
