// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using EOLib.Net.Handlers;
using EOLib.PacketHandlers.Chat;
using EOLib.PacketHandlers.Commands;
using Microsoft.Practices.Unity;

namespace EOLib.PacketHandlers
{
    public class PacketHandlerDependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            //connection ping - tells server to keep client alive, update network sequence numbers
            container.RegisterVaried<IPacketHandler, ConnectionPlayerHandler>();

            //commands
            container
                .RegisterVaried<IPacketHandler, PingResponseHandler>()
                .RegisterVaried<IPacketHandler, FindCommandPlayerNotFoundHandler>()
                .RegisterVaried<IPacketHandler, FindCommandPlayerSameMapHandler>()
                .RegisterVaried<IPacketHandler, FindCommandPlayerDifferentMapHandler>();

            //chat
            container.RegisterVaried<IPacketHandler, PublicChatHandler>()
                .RegisterVaried<IPacketHandler, GroupChatHandler>()
                .RegisterVaried<IPacketHandler, PrivateMessageTargetNotFound>()
                .RegisterVaried<IPacketHandler, PrivateMessageHandler>()
                .RegisterVaried<IPacketHandler, GuildMessageHandler>()
                .RegisterVaried<IPacketHandler, GlobalMessageHandler>()
                .RegisterVaried<IPacketHandler, ServerMessageHandler>()
                .RegisterVaried<IPacketHandler, AdminMessageHandler>()
                .RegisterVaried<IPacketHandler, AnnounceMessageHandler>()
                .RegisterVaried<IPacketHandler, MuteHandler>();

            //player actions
            container.RegisterVaried<IPacketHandler, PlayerDirectionHandler>()
                .RegisterVaried<IPacketHandler, PlayerWalkHandler>()
                .RegisterVaried<IPacketHandler, MainPlayerWalkHandler>()
                .RegisterVaried<IPacketHandler, PlayerAttackHandler>()
                .RegisterVaried<IPacketHandler, PlayerEnterMapHandler>()
                .RegisterVaried<IPacketHandler, PlayerLeaveMapHandler>()
                .RegisterVaried<IPacketHandler, PlayerAvatarChangeHandler>()
                .RegisterVaried<IPacketHandler, PlayerLevelUpHandler>()
                .RegisterVaried<IPacketHandler, PlayerLevelUpFromSpellCastHandler>()
                .RegisterVaried<IPacketHandler, StatTrainingHandler>()
                .RegisterVaried<IPacketHandler, RecoverStatListHandler>()
                .RegisterVaried<IPacketHandler, PlayerRecoverHandler>();

            //npcs
            container.RegisterVaried<IPacketHandler, NPCActionHandler>()
                .RegisterVaried<IPacketHandler, NPCEnterMapHandler>()
                .RegisterVaried<IPacketHandler, NPCLeaveMapHandler>()
                .RegisterVaried<IPacketHandler, NPCDieFromSpellCastHandler>();

            //warps
            container.RegisterVaried<IPacketHandler, BeginPlayerWarpHandler>()
                .RegisterVaried<IPacketHandler, EndPlayerWarpHandler>()
                .RegisterVaried<IPacketHandler, RefreshMapStateHandler>()
                .RegisterVaried<IPacketHandler, DoorOpenHandler>();

            //admin
            container.RegisterVaried<IPacketHandler, AdminHideHandler>()
                .RegisterVaried<IPacketHandler, AdminShowHandler>();
        }
    }
}
