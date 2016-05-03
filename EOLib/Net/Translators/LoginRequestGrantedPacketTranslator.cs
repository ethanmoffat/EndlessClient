// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Login;

namespace EOLib.Net.Translators
{
	public class LoginRequestGrantedPacketTranslator : IPacketTranslator<ILoginRequestGrantedData>
	{
		public ILoginRequestGrantedData TranslatePacket(IPacket packet)
		{
			throw new System.NotImplementedException();
		}
	}
}
