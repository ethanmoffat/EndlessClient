﻿using System;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Old;
using EOLib.Graphics;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls.Old;

namespace EndlessClient.Dialogs
{
    public abstract class EODialogBase : XNADialog
    {
        protected readonly Texture2D smallButtonSheet;
        protected readonly PacketAPI m_api;

        protected EODialogBase(PacketAPI apiHandle)
        {
            if (apiHandle != null)
            {
                if (!apiHandle.Initialized)
                    throw new ArgumentException("The API is not initialzied. Data transfer will not work.");
                m_api = apiHandle;
            }
            smallButtonSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true);
        }

        protected EODialogBase(INativeGraphicsManager nativeGraphicsManager)
        {
            smallButtonSheet = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 15, true);
        }

        protected void endConstructor(bool centerDialog = true)
        {
            //center dialog based on txtSize of background texture
            if (centerDialog)
            {
                Center(Game.GraphicsDevice);
                if (EOGame.Instance.State == GameStates.PlayingTheGame)
                {
                    DrawLocation = new Vector2(DrawLocation.X, (330 - DrawArea.Height) / 2f);
                }
            }
            _fixDrawOrder();
            DrawOrder += 100;
            Dialogs.Push(this);

            Game.Components.Add(this);
        }

        //todo: usage of this should eventually be refactored out if possible
        protected void CenterAndFixDrawOrder(IGraphicsDeviceProvider graphicsDeviceProvider,
                                             IGameStateProvider gameStateProvider,
                                             bool shouldCenter = true)
        {
            if (shouldCenter)
            {
                Center(graphicsDeviceProvider.GraphicsDevice);

                if (gameStateProvider.CurrentState == GameStates.PlayingTheGame)
                    DrawLocation = new Vector2(DrawLocation.X, (330 - DrawArea.Height)/2f);
            }

            _fixDrawOrder();
            DrawOrder += 100;
            
            Dialogs.Push(this);
            Game.Components.Add(this);
        }

        protected Rectangle _getSmallButtonOut(SmallButton whichOne)
        {
            int widthDelta = smallButtonSheet.Width / 2;
            int heightDelta = smallButtonSheet.Height / (int)SmallButton.NUM_BUTTONS;
            return new Rectangle(0, heightDelta * (int)whichOne, widthDelta, heightDelta);
        }

        protected Rectangle _getSmallButtonOver(SmallButton whichOne)
        {
            int widthDelta = smallButtonSheet.Width / 2;
            int heightDelta = smallButtonSheet.Height / (int)SmallButton.NUM_BUTTONS;
            return new Rectangle(widthDelta, heightDelta * (int)whichOne, widthDelta, heightDelta);
        }

        protected enum ListIcon
        {
            Buy = 0,
            Sell,
            BankDeposit,
            BankWithdraw,
            Craft,
            BankLockerUpgrade,

            Learn = 20,
            Forget = 21,
        }
        protected Texture2D _getDlgIcon(ListIcon whichOne)
        {
            const int NUM_PER_ROW = 9;
            const int ICON_SIZE = 31;

            Texture2D weirdSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 27);
            Color[] dat = new Color[ICON_SIZE * ICON_SIZE];

            Rectangle src = new Rectangle(((int)whichOne % NUM_PER_ROW) * ICON_SIZE, 291 + ((int)whichOne / NUM_PER_ROW) * ICON_SIZE, ICON_SIZE, ICON_SIZE);
            weirdSheet.GetData(0, src, dat, 0, dat.Length);

            Texture2D ret = new Texture2D(EOGame.Instance.GraphicsDevice, ICON_SIZE, ICON_SIZE);
            ret.SetData(dat);
            return ret;
        }

        /// <summary>
        /// This is a no-op to hide Resharper warnings. It may eventually do something.
        ///  It would be nice to trigger adding all controls to Components by calling this.
        /// </summary>
        public virtual void Show() { }
    }
}
