namespace EOLib.Data
{
	internal class NPCRecordFactory : IDataRecordFactory
	{
		public IDataRecord CreateRecord(int id)
		{
			return new NPCRecord(id);
		}
	}
}
