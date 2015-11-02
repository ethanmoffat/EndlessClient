// Original Work Copyright (c) Ethan Moffat 2014-2015
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Windows.Forms;

namespace EndlessClient
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if !DEBUG
			try
			{
#endif
				using (EOGame.Instance)
				{
					EOGame.Instance.Run();
				}
#if !DEBUG
			}
			catch (Exception ex)
			{
				Application.EnableVisualStyles();

				Form exForm = new Form
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

				Label exLabel1 = new Label
				{
					AutoEllipsis = true,
					Dock = DockStyle.Top,
					Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold),
					Text = "An unhandled exception has caused the game to crash:"
				};

				Label exLabel2 = new Label
				{
					AutoEllipsis = true,
					Dock = DockStyle.Top,
					Padding = new Padding(5, 0, 0, 0),
					Text = ex.Message
				};

				Label exLabel3 = new Label
				{
					AutoEllipsis = true,
					Dock = DockStyle.Top,
					Font = new System.Drawing.Font(Control.DefaultFont, System.Drawing.FontStyle.Bold),
					Text = "Stack trace:"
				};

				TextBox exTextBox1 = new TextBox
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
#endif

			Logger.Close();
		}
	}
}