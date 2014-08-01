using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib.Data;

namespace EndlessClient
{
	public class WorldLoadException : Exception
	{
		public WorldLoadException(string msg)
			: base(msg)
		{

		}
	}

	//singleton pattern: provides global access to data files and network connection
	//	without allowing for instantiation outside of the class or inheriting from it
	public sealed class World
	{
		private static World inst = null;
		private static readonly object locker = new object();

		public static World Instance
		{
			get
			{
				lock (locker)
				{
					if (inst == null)
						inst = new World();
					return inst;
				}
			}
		}

		private readonly ItemFile m_items;
		public ItemFile EIF
		{
			get { return m_items; }
		}

		private readonly Player m_player;
		public Player MainPlayer
		{
			get { return m_player; }
		}

		private readonly AsyncClient m_client;
		public AsyncClient Client
		{
			get { return m_client; }
		}

		private World()
		{
			try
			{
				m_items = new ItemFile();
			}
			catch
			{
				throw new WorldLoadException("There was an error loading the pub files. Place all pub files in .\\pub\\");
			}

			m_player = new Player();
			m_client = new EOClient();
		}
	}
}
