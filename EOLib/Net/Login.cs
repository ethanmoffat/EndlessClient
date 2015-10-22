// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading;

namespace EOLib.Net
{
	public enum LoginReply : short
	{
		WrongUser = 1,
		WrongUserPass = 2,
		Ok = 3,
		LoggedIn = 5,
		Busy = 6,
		THIS_IS_WRONG = 255
	}

	partial class PacketAPI
	{
		private AutoResetEvent m_login_responseEvent;
		private LoginReply m_login_reply;

		private void _createLoginMembers()
		{
			m_login_responseEvent = new AutoResetEvent(false);
			m_login_reply = LoginReply.THIS_IS_WRONG;

			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Login, PacketAction.Reply), _handleLoginReply, false);
		}

		private void _disposeLoginMembers()
		{
			if (m_login_responseEvent != null)
			{
				m_login_responseEvent.Dispose();
				m_login_responseEvent = null;
			}
		}

		public bool LoginRequest(string user, string pass, out LoginReply reply, out CharacterRenderData[] data)
		{
			reply = LoginReply.THIS_IS_WRONG;
			data = null;
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Login, PacketAction.Request);
			pkt.AddBreakString(user);
			pkt.AddBreakString(pass);

			if(!m_client.SendPacket(pkt) || !m_login_responseEvent.WaitOne(Constants.ResponseTimeout))
				return false;

			reply = m_login_reply;
			if (reply == LoginReply.Ok && m_character_data != null)
			{
				data = m_character_data;
			}

			return true;
		}

		//handler for LOGIN_REPLY received from server
		private void _handleLoginReply(Packet pkt)
		{
			m_login_reply = (LoginReply)pkt.GetShort();

			if (m_login_reply == LoginReply.Ok)
			{
				byte numCharacters = pkt.GetChar();
				pkt.GetByte();
				pkt.GetByte();

				m_character_data = new CharacterRenderData[numCharacters];

				for (int i = 0; i < numCharacters; ++i)
				{
					CharacterRenderData nextData = new CharacterRenderData(pkt);
					m_character_data[i] = nextData;
					if (255 != pkt.GetByte())
						return; //malformed packet - time out and signal error
				}
			}

			m_login_responseEvent.Set();
		}

		public DATCONST1 LoginResponseMessage()
		{
			DATCONST1 message = DATCONST1.NICE_TRY_HAXOR;
			switch (m_login_reply)
			{
				case LoginReply.LoggedIn:
					message = DATCONST1.LOGIN_ACCOUNT_ALREADY_LOGGED_ON;
					break;
				case LoginReply.Busy:
					message = DATCONST1.CONNECTION_SERVER_IS_FULL;
					break;
				case LoginReply.WrongUser:
					message = DATCONST1.LOGIN_ACCOUNT_NAME_NOT_FOUND;
					break;
				case LoginReply.WrongUserPass:
					message = DATCONST1.LOGIN_ACCOUNT_NAME_OR_PASSWORD_NOT_FOUND;
					break;
			}

			return message;
		}
	}
}
