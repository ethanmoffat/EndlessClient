// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Data;
using EOLib.IO;
using EOLib.Net;
using Microsoft.Practices.Unity;

#if !DEBUG
using System.Windows.Forms;
#endif

namespace EndlessClient
{
	public static class Program
	{
		[STAThread]
		public static void Main()
		{
#if !DEBUG
			try
			{
#endif
				using (var unityContainer = new UnityContainer())
				{
					var registrar = new DependencyRegistrar(unityContainer);

					registrar.RegisterDependencies(new DataDependencyContainer(),
						new IODependencyContainer(),
						new NetworkDependencyContainer());

					registrar.InitializeDependencies(new NetworkDependencyContainer());

					var game = unityContainer.Resolve<IEndlessGame>();
					game.Run();
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