// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using EOLib.Net.Handlers;
using Microsoft.Xna.Framework;

namespace EndlessClient.Network
{
    public class PacketHandlerGameComponent : GameComponent
    {
        private readonly IOutOfBandPacketHandler _packetHandler;

        public PacketHandlerGameComponent(IEndlessGame game,
                                          IOutOfBandPacketHandler packetHandler)
            : base((Game) game)
        {
            _packetHandler = packetHandler;
        }

        public override void Update(GameTime gameTime)
        {
            _packetHandler.PollForPacketsAndHandle();

            base.Update(gameTime);
        }
    }
}
