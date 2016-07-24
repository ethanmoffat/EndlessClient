// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class ControlKeyListener : InputKeyListenerBase
    {
        public ControlKeyListener()
        {
            if (Game.Components.Any(x => x is ControlKeyListener))
                throw new InvalidOperationException("The game already contains an arrow key listener");
            Game.Components.Add(this);
        }

        public override void Update(GameTime gameTime)
        {
            if (!IgnoreInput && Character.State != CharacterActionState.Attacking)
            {
                UpdateInputTime();

                EODirection direction = EODirection.Invalid;
                if (IsKeyPressed(Keys.LeftControl) || IsKeyPressed(Keys.RightControl))
                    direction = Character.RenderData.facing;

                byte destX, destY;
                switch (direction)
                {
                    case EODirection.Up:
                        destX = (byte) Character.X;
                        destY = (byte) (Character.Y - 1);
                        break;
                    case EODirection.Down:
                        destX = (byte) Character.X;
                        destY = (byte) (Character.Y + 1);
                        break;
                    case EODirection.Right:
                        destX = (byte) (Character.X + 1);
                        destY = (byte) Character.Y;
                        break;
                    case EODirection.Left:
                        destX = (byte) (Character.X - 1);
                        destY = (byte) Character.Y;
                        break;
                    default:
                        destX = destY = 255;
                        break;
                }

                if (direction != EODirection.Invalid && Character.State == CharacterActionState.Standing)
                {
                    if (Character.CanAttack)
                    {
                        //var info = OldWorld.Instance.ActiveMapRenderer.GetTileInfo((byte) Character.X, (byte) Character.Y);
                        //Character.Attack(direction, destX, destY); //destX and destY validity check above
                        //Renderer.PlayerAttack(info.ReturnType == TileInfoReturnType.IsTileSpec); && info.Spec == TileSpec.Water);
                    }
                    else if (Character.Weight > Character.MaxWeight)
                    {
                        EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                            EOResourceID.STATUS_LABEL_CANNOT_ATTACK_OVERWEIGHT);
                    }
                }
            }

            base.Update(gameTime);
        }
    }
}
