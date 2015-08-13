using System;
using System.Windows.Forms;

namespace EndlessClient
{
#if WINDOWS
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
#if DEBUG
			//XNAControls.XNAControl.DrawOrderVisible = true;
#endif
			try
			{
				using (EOGame.Instance)
				{
					EOGame.Instance.Run();
				}
			}
			catch (Exception ex)
			{
				Application.EnableVisualStyles();

				Form exForm = new Form();
				exForm.Width = 350;
				exForm.Height = 200;
				exForm.MaximizeBox = false;
				exForm.MinimizeBox = false;
				exForm.Padding = new Padding(10);
				exForm.Text = "Application Error";
				exForm.BackColor = System.Drawing.Color.White;
				exForm.Icon = System.Drawing.SystemIcons.Error;
				exForm.StartPosition = FormStartPosition.CenterScreen;
				exForm.MinimumSize = new System.Drawing.Size(350, 200);
				exForm.FormClosed += (object sender, FormClosedEventArgs e) => {
					// Report the Exception?
					Environment.Exit(1);
				};

				Label exLabel1 = new Label();
				exLabel1.AutoEllipsis = true;
				exLabel1.Dock = DockStyle.Top;
				exLabel1.Font = new System.Drawing.Font(exLabel1.Font, System.Drawing.FontStyle.Bold);
				exLabel1.Text = "An unhandled exception has caused the game to crash:";

				Label exLabel2 = new Label();
				exLabel2.AutoEllipsis = true;
				exLabel2.Dock = DockStyle.Top;
				exLabel2.Padding = new Padding(5, 0, 0, 0);
				exLabel2.Text = ex.Message;

				Label exLabel3 = new Label();
				exLabel3.AutoEllipsis = true;
				exLabel3.Dock = DockStyle.Top;
				exLabel3.Font = new System.Drawing.Font(exLabel3.Font, System.Drawing.FontStyle.Bold);
				exLabel3.Text = "Stack trace:";

				TextBox exTextBox1 = new TextBox();
				exTextBox1.Dock = DockStyle.Fill;
				exTextBox1.Multiline = true;
				exTextBox1.ReadOnly = true;
				exTextBox1.ScrollBars = ScrollBars.Vertical;
				exTextBox1.Text = ex.ToString();

				exForm.Controls.Add(exTextBox1);
				exForm.Controls.Add(exLabel3);
				exForm.Controls.Add(exLabel2);
				exForm.Controls.Add(exLabel1);
				exForm.ShowDialog();
			}

			Logger.Close();
		}
	}
#endif
}

