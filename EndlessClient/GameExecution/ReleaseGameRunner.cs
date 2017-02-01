// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Windows.Forms;
using Microsoft.Practices.Unity;

namespace EndlessClient.GameExecution
{
    /// <summary>
    /// A game runner that catches exceptions and display exception information
    /// </summary>
    public class ReleaseGameRunner : GameRunnerBase
    {
        public ReleaseGameRunner(IUnityContainer unityContainer)
            : base(unityContainer) { }

        public override bool SetupDependencies()
        {
            try
            {
                return base.SetupDependencies();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error setting up dependencies for the game! Error message is:\n\n{ex.Message}",
                    "Dependency setup error!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }

        public override void RunGame()
        {
            try
            {
                base.RunGame();
            }
            catch (Exception ex)
            {
                ShowExceptionDialog(ex);
            }
        }

        private static void ShowExceptionDialog(Exception ex)
        {
            Application.EnableVisualStyles();

            var exForm = new Form
            {
                Width = 350,
                Height = 200,
                MaximizeBox = false,
                MinimizeBox = false,
                Padding = new Padding(10),
                Text = "Application Error",
                BackColor = System.Drawing.Color.White,
                Icon = System.Drawing.SystemIcons.Error,
                StartPosition = FormStartPosition.CenterScreen,
                MinimumSize = new System.Drawing.Size(350, 200)
            };
            exForm.FormClosed += (sender, e) => Environment.Exit(1);

            var exLabel1 = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Top,
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold),
                Text = "An unhandled exception has caused the game to crash:"
            };

            var exLabel2 = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Top,
                Padding = new Padding(5, 0, 0, 0),
                Text = ex.Message
            };

            var exLabel3 = new Label
            {
                AutoEllipsis = true,
                Dock = DockStyle.Top,
                Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold),
                Text = "Stack trace:"
            };

            var exTextBox1 = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Text = ex.StackTrace
            };

            exForm.Controls.Add(exTextBox1);
            exForm.Controls.Add(exLabel3);
            exForm.Controls.Add(exLabel2);
            exForm.Controls.Add(exLabel1);
            exForm.ShowDialog();
        }
    }
}
