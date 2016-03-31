// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using EOLib.Net.PacketProcessing;

namespace EOLib.Net.API
{
	public enum InitReply : byte
	{
		INIT_OUT_OF_DATE = 1,
		INIT_OK = 2,
		INIT_BANNED = 3,
		INIT_FILE_MAP = 4,
		INIT_FILE_EIF = 5,
		INIT_FILE_ENF = 6,
		INIT_FILE_ESF = 7,
		INIT_PLAYERS = 8,
		INIT_MAP_MUTATION = 9,
		INIT_FRIEND_LIST_PLAYERS = 10,
		INIT_FILE_ECF = 11,
		THIS_IS_WRONG = 0
	}

	public enum InitBanType : byte
	{
		INIT_BAN_TEMP = 0,
		INIT_BAN_PERM = 2,
		THIS_IS_WRONG = 255
	}

	public enum InitFileType : byte
	{
		Map = 1,
		Item = 2,
		Npc = 3,
		Spell = 4,
		Class = 5
	}

	public struct InitData
	{
		public byte seq_1, seq_2, emulti_e, emulti_d;
		public short clientID;
		public int response;

		public InitReply ServerResponse;
		public InitBanType BanType;
		public byte BanMinsLeft;
		public byte RequiredVersionNumber;
	}

	public enum PaperdollIconType
	{
		Normal = 0,
		GM = 4,
		HGM = 5,
		Party = 6,
		GMParty = 9,
		HGMParty = 10,
		SLNBot = 20
	}

	public class OnlineEntry
	{
		private readonly string m_name, m_title, m_guild;
		private readonly int m_class;
		private readonly PaperdollIconType m_iconType;

		public string Name
		{
			get { return m_name; }
		}

		public string Title
		{
			get { return m_title; }
		}

		public string Guild
		{
			get { return m_guild; }
		}

		public int ClassID
		{
			get { return m_class; }
		}

		public PaperdollIconType Icon
		{
			get { return m_iconType; }
		}

		public OnlineEntry(string name, string title, string guild, int clss, PaperdollIconType icon)
		{
			m_name = name;
			m_title = title;
			m_guild = guild;
			m_class = clss;
			m_iconType = icon;
		}
	}

	public partial class PacketAPI
	{
		private AutoResetEvent m_init_responseEvent;

		//shared between API calls and response handler
		private InitData m_init_initData;
		private int m_init_requestedMap;
		private List<OnlineEntry> m_init_onlinePlayerList;

		public event Action OnMapMutation;

		private void _createInitMembers()
		{
			m_init_responseEvent = new AutoResetEvent(false);

			m_init_initData = new InitData();
			m_init_requestedMap = 0;
			m_init_onlinePlayerList = null;

			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Init, PacketAction.Init), _handleInitInit, false);
		}

		private void _disposeInitMembers()
		{
			if (m_init_responseEvent != null)
			{
				m_init_responseEvent.Dispose();
				m_init_responseEvent = null;
			}
		}

		public bool Initialize(byte versionMajor, byte versionMinor, byte versionBuild, string hdSerialNum, out InitData data)
		{
			return Initialize(new[] {versionMajor, versionMinor, versionBuild}, hdSerialNum, out data);
		}

		private bool Initialize(byte[] versionNumber, string hdSerialNum, out InitData data)
		{
			Initialized = false;
			data = new InitData();
			if (!m_client.ConnectedAndInitialized || versionNumber.Length != 3 || hdSerialNum.Length == 0)
			{
				return false;
			}

			OldPacket builder = new OldPacket(PacketFamily.Init, PacketAction.Init);

			var hashService = new HashService();
			builder.AddThree(hashService.StupidHash(new Random().Next(6, 12)));

			builder.AddChar(versionNumber[0]);
			builder.AddChar(versionNumber[1]);
			builder.AddChar(versionNumber[2]); //client version
			builder.AddChar(112); //unknown
			builder.AddChar((byte)hdSerialNum.Length);
			builder.AddString(hdSerialNum);

			if (!m_client.SendRaw(builder))
				return false;

			if (!m_init_responseEvent.WaitOne(Constants.ResponseTimeout))
				return false;

			data = m_init_initData;

			return true;
		}

		public bool RequestFile(InitFileType file, short mapID = -1)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			if (file == InitFileType.Map && mapID <= 0)
				return false;

			OldPacket builder = new OldPacket(PacketFamily.Welcome, PacketAction.Agree);
			builder.AddChar((byte)file);
			m_client.ExpectingFile = true;
			m_init_requestedMap = mapID;

			if (!m_client.SendPacket(builder) || !m_init_responseEvent.WaitOne(Constants.ResponseFileTimeout))
				return false;

			return true;
		}

		private bool RequestWarpMap(int MapID)
		{
			//arbitrary upper bound is 32768
			if (MapID < 1 || MapID > short.MaxValue)
				return false;

			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			//if player list was requested, wait for it to finish
			if (m_client.ExpectingPlayerList && !m_init_responseEvent.WaitOne(Constants.ResponseTimeout))
				return false;

			//send the file request
			OldPacket builder = new OldPacket(PacketFamily.Warp, PacketAction.Take);
			m_client.ExpectingFile = true;
			m_init_requestedMap = MapID;

			if (!m_client.SendPacket(builder))
				return false;

			if (!m_init_responseEvent.WaitOne(Constants.ResponseFileTimeout))
				return false;

			return true;
		}

		/// <summary>
		/// Performas a synchronous request for the list of online players
		/// </summary>
		/// <param name="includeFullInfo">True if requesting the full information including Name, Title, Class, Guild, and Icon. False if requesting names only</param>
		/// <param name="list">The list of players represented by OnlineEntry objects</param>
		/// <returns>True if operation was successful, false otherwise.</returns>
		public bool RequestOnlinePlayers(bool includeFullInfo, out List<OnlineEntry> list)
		{
			list = null;
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			//wait for file if it is in process
			if (m_client.ExpectingFile && !m_init_responseEvent.WaitOne(Constants.ResponseFileTimeout))
				return false;

			m_client.ExpectingPlayerList = true;
			OldPacket pkt = new OldPacket(PacketFamily.Players, includeFullInfo ? PacketAction.Request : PacketAction.List);
			if (!m_client.SendPacket(pkt))
				return false;

			if (!m_init_responseEvent.WaitOne(Constants.ResponseTimeout))
				return false;

			list = m_init_onlinePlayerList;

			return true;
		}

		private void _handleInitInit(OldPacket pkt)
		{
			InitReply response = (InitReply)pkt.GetByte();
			switch (response)
			{
				case InitReply.INIT_FRIEND_LIST_PLAYERS:
				case InitReply.INIT_PLAYERS:
					if (!m_client.ExpectingPlayerList)
						break;
					_handlePlayerList(pkt, response == InitReply.INIT_FRIEND_LIST_PLAYERS);
					break;
				case InitReply.INIT_BANNED:
				{
					//sends INIT_BANNED in response to WARP_TAKE packet
					if (m_client.ExpectingFile)
					{
						response = InitReply.INIT_FILE_MAP;
						goto case InitReply.INIT_FILE_MAP;
					}

					InitBanType BanType = (InitBanType) pkt.GetByte();
					byte BanMinsLeft = 0;
					if (BanType == InitBanType.INIT_BAN_TEMP)
						BanMinsLeft = pkt.GetByte();

					m_init_initData = new InitData {ServerResponse = response, BanType = BanType, BanMinsLeft = BanMinsLeft};
				}
					break;
				case InitReply.INIT_OUT_OF_DATE:
				{
					pkt.GetChar(); //always 0
					pkt.GetChar(); //always 0
					m_init_initData = new InitData
					{
						ServerResponse = response,
						RequiredVersionNumber = pkt.GetChar() //actual version number required
					};
				}
					break;
				case InitReply.INIT_OK:
				{
					m_init_initData = new InitData
					{
						ServerResponse = response,
						seq_1 = pkt.GetByte(),
						seq_2 = pkt.GetByte(),
						//These are switched around from the server: 
						//	the server's encode function is the client's decode function (and vice versa)
						emulti_d = pkt.GetByte(),
						emulti_e = pkt.GetByte(),
						clientID = pkt.GetShort(),
						response = pkt.GetThree()
					};
				}
					break;
					//file transfer is all handled the same way
				case InitReply.INIT_FILE_EIF:
				case InitReply.INIT_FILE_ENF:
				case InitReply.INIT_FILE_ESF:
				case InitReply.INIT_FILE_ECF:
				case InitReply.INIT_FILE_MAP:
				case InitReply.INIT_MAP_MUTATION:
				{
					string localDir = response == InitReply.INIT_FILE_MAP || response == InitReply.INIT_MAP_MUTATION ? "maps" : "pub";

					if (response == InitReply.INIT_MAP_MUTATION)
						m_init_requestedMap = 0;

					if (!Directory.Exists(localDir))
						Directory.CreateDirectory(localDir);

					string filename;
					if (response == InitReply.INIT_FILE_EIF)
						filename = "dat001.eif";
					else if (response == InitReply.INIT_FILE_ENF)
						filename = "dtn001.enf";
					else if (response == InitReply.INIT_FILE_ESF)
						filename = "dsl001.esf";
					else if (response == InitReply.INIT_FILE_ECF)
						filename = "dat001.ecf";
					else
						filename = string.Format("{0,5:D5}.emf", m_init_requestedMap);

					using (FileStream fs = File.Create(Path.Combine(localDir, filename)))
					{
						int dataLen = pkt.Length - 3;
						if (dataLen == 0)
							return; //trigger error by not setting response event
						fs.Write(pkt.GetBytes(dataLen), 0, dataLen);
					}

					if (response == InitReply.INIT_MAP_MUTATION && OnMapMutation != null)
					{
						OnMapMutation();
					}
				}
					break;
			}

			m_client.ExpectingFile = false;
			m_client.ExpectingPlayerList = false;
			m_init_responseEvent.Set(); //packet was handled
		}

		private void _handlePlayerList(OldPacket pkt, bool isFriendList)
		{
			short numTotal = pkt.GetShort();
			if (pkt.GetByte() != 255)
				return;

			m_init_onlinePlayerList = new List<OnlineEntry>();
			for (int i = 0; i < numTotal; ++i)
			{
				string name = pkt.GetBreakString();

				if (!isFriendList)
				{
					string title = pkt.GetBreakString();
					if (string.IsNullOrWhiteSpace(title))
						title = "-";
					if (pkt.GetChar() != 0)
						return;

					PaperdollIconType iconType = (PaperdollIconType)pkt.GetChar();
					
					int clsId = pkt.GetChar();
					
					string guild = pkt.GetBreakString();
					if (string.IsNullOrWhiteSpace(guild))
						guild = "-";

					name = char.ToUpper(name[0]) + name.Substring(1);
					title = char.ToUpper(title[0]) + title.Substring(1);

					m_init_onlinePlayerList.Add(new OnlineEntry(name, title, guild, clsId, iconType));
				}
				else
				{
					m_init_onlinePlayerList.Add(new OnlineEntry(name, "", "", 0, PaperdollIconType.Normal));
				}
			}
		}

		public DATCONST1 GetInitResponseMessage(out string extra)
		{
			DATCONST1 msg;

			switch (m_init_initData.ServerResponse)
			{
				case InitReply.INIT_BANNED:
					if (m_init_initData.BanType == InitBanType.INIT_BAN_TEMP)
					{
						msg = DATCONST1.CONNECTION_IP_BAN_TEMP;
						extra = " " + m_init_initData.BanMinsLeft + " minutes.";
					}
					else
					{
						msg = DATCONST1.CONNECTION_IP_BAN_PERM;
						extra = "";
					}

					break;
				case InitReply.INIT_OUT_OF_DATE:
					msg = DATCONST1.CONNECTION_CLIENT_OUT_OF_DATE;
					extra = " 0.000.0" + m_init_initData.RequiredVersionNumber;
					break;
				default:
					msg = DATCONST1.CONNECTION_SERVER_NOT_FOUND;
					extra = "";
					break;
			}

			return msg;
		}
	}
}
