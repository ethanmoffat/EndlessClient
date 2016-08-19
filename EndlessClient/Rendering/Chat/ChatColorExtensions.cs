// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.HUD.Chat;
using EOLib.Domain.Chat;
using EOLib.Graphics;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Chat
{
    public static class ChatColorExtensions
    {
        public static Color ToColor(this ChatColor chatColor)
        {
            switch (chatColor)
            {
                case ChatColor.Default: return Color.Black;
                case ChatColor.Error: return Color.FromNonPremultiplied(0x7d, 0x0a, 0x0a, 0xff);
                case ChatColor.PM: return Color.FromNonPremultiplied(0x5a, 0x3c, 0x00, 0xff);
                case ChatColor.Server: return Color.FromNonPremultiplied(0xe6, 0xd2, 0xc8, 0xff);
                case ChatColor.ServerGlobal: return ColorConstants.LightYellowText;
                case ChatColor.Admin: return Color.FromNonPremultiplied(0xc8, 0xaa, 0x96, 0xff);
                default: throw new ArgumentOutOfRangeException("chatColor", chatColor, "Unrecognized chat color");
            }
        }
    }
}
