// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient
{
    public partial class EOGame
    {
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private SpriteBatch _spriteBatch;

        public KeyboardDispatcher Dispatcher { get; private set; }

        private XNAButton _backButton;
        private bool _backButtonPressed; //workaround so the lost connection dialog doesn't show from the client disconnect event

        public HUD.Controls.HUD Hud { get; private set; }
        public SoundManager SoundManager { get; private set; }

        private void InitializeControls(bool reinit = false)
        {
            Texture2D back = GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 24, true);
            _backButton = new XNAButton(back, new Vector2(589, 0), new Rectangle(0, 0, back.Width, back.Height / 2),
                new Rectangle(0, back.Height / 2, back.Width, back.Height / 2)) { DrawOrder = 100 };
            _backButton.OnClick += MainButtonPress;
            _backButton.ClickArea = new Rectangle(4, 16, 16, 16);

            //hide all the components to start with
            foreach (IGameComponent iGameComp in Components)
            {
                DrawableGameComponent component = iGameComp as DrawableGameComponent;
                //don't hide dialogs if reinitializing
                if (reinit && (XNAControl.Dialogs.Contains(component as XNAControl) || 
                    (component as XNAControl != null && XNAControl.Dialogs.Contains((component as XNAControl).TopParent))))
                    continue;

                //...except for the four main buttons
                if (component != null)
                    component.Visible = false;
            }
        }

        private void MainButtonPress(object sender, EventArgs e)
        {
            if (!IsActive)
                return;
            
            if (sender == _backButton && State == GameStates.PlayingTheGame)
            {
                EOMessageBox.Show(DATCONST1.EXIT_GAME_ARE_YOU_SURE, XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader, 
                    (ss, ee) =>
                    {
                        if(ee.Result == XNADialogResult.OK)
                        {
                            _backButtonPressed = true;
                            Dispatcher.Subscriber = null;
                            OldWorld.Instance.ResetGameElements();
                            if (OldWorld.Instance.Client.ConnectedAndInitialized)
                                OldWorld.Instance.Client.Disconnect();
                            doStateChange(GameStates.Initial);
                            _backButtonPressed = false;
                        }
                    });
            }
        }
    }
}
