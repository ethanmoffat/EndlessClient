// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
    partial class PacketAPI
    {
        /// <summary>
        /// Refreshes the client view - response will include other Characters, NPCs, and Map Items (see OnWarpAgree)
        /// </summary>
        /// <returns>True on successful send operation, false on failure</returns>
        public bool RequestRefresh()
        {
            //no data sent to server - just expecting a reply
            return m_client.ConnectedAndInitialized && Initialized &&
                   m_client.SendPacket(new OldPacket(PacketFamily.Refresh, PacketAction.Request));
        }
    }
}
