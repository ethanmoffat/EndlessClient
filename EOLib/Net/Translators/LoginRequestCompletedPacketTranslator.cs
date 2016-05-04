// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Login;

namespace EOLib.Net.Translators
{
	public class LoginRequestCompletedPacketTranslator : IPacketTranslator<ILoginRequestCompletedData>
	{
		public ILoginRequestCompletedData TranslatePacket(IPacket packet)
		{
			var reply = (CharacterLoginReply)packet.ReadShort();
			if (reply != CharacterLoginReply.RequestCompleted)
				throw new MalformedPacketException("Unexpected welcome response in packet: " + reply, packet);

			throw new System.NotImplementedException();
		}
	}
}
