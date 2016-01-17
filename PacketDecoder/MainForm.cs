// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Windows.Forms;
using EOLib;

namespace PacketDecoder
{
	public partial class MainForm : Form
	{
		private enum DataTypes
		{
			None = -1,
			PacketFamily,
			PacketAction,
			Byte,
			Char,
			Short,
			Three,
			Int,
			BreakString,
			EndString,
			FixedString,
		}

		private DataTypes m_type;
		private ClientPacketProcessor m_processor;
		private int m_packetOffset, m_dataLength;
		private bool m_suppressEvent;

		public MainForm()
		{
			InitializeComponent();

			cmbOutputFmt_SelectedIndexChanged(null, null);
			m_processor = new ClientPacketProcessor();
		}

		private void cmbOutputFmt_SelectedIndexChanged(object sender, EventArgs e)
		{
			m_type = (DataTypes) cmbOutputFmt.SelectedIndex;
			if (m_type == DataTypes.FixedString)
			{
				txtLength.Enabled = true;
				txtLength.Text = "";
			}
			else
			{
				txtLength.Enabled = false;
				switch (m_type)
				{
					case DataTypes.BreakString:
					case DataTypes.EndString:
					case DataTypes.None:
						txtLength.Text = "";
						break;
					case DataTypes.PacketFamily:
						txtLength.Text = "1";
						break;
					case DataTypes.PacketAction:
						txtLength.Text = "1";
						break;
					case DataTypes.Byte:
						txtLength.Text = "1";
						break;
					case DataTypes.Char:
						txtLength.Text = "1";
						break;
					case DataTypes.Short:
						txtLength.Text = "2";
						break;
					case DataTypes.Three:
						txtLength.Text = "3";
						break;
					case DataTypes.Int:
						txtLength.Text = "4";
						break;
				}
			}
			_checkRequiredInputs();
		}

		private void intTextValidate(object sender, EventArgs e)
		{
			if (m_suppressEvent) return;

			TextBox txt = sender as TextBox;
			if (txt == null)
				return;

			int param;
			if (!int.TryParse(txt.Text, out param))
			{
				if (txt == txtLength)
					m_dataLength = 0;
				return;
			}

			if (txt == txtDMulti)
			{
				m_processor.RecvMulti = (byte) param;
				if (param < 6 || param > 12)
					MessageBox.Show("This should be between 6 and 12...", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (txt == txtEMulti)
			{
				m_processor.SendMulti = (byte) param;
				if (param < 6 || param > 12)
					MessageBox.Show("This should be between 6 and 12...", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
			else if (txt == txtOffset)
			{
				m_packetOffset = param;
				if (param >= txtInputData.TextLength)
					param = txtInputData.TextLength - 1;
			}
			else if (txt == txtLength)
				m_dataLength = param;

			_checkRequiredInputs();
		}

		private void _checkRequiredInputs()
		{
			txtOutput.Text = "";

			if (txtDMulti.TextLength == 0 || txtEMulti.TextLength == 0 || txtOffset.TextLength == 0)
				return;

			if (txtLength.TextLength == 0 && m_type == DataTypes.FixedString)
				return;

			string inputData = txtInputData.Text;
			if (inputData.Length == 0)
				return;
			
			//input data is copied from wireshark. colon delimited.
			string bytes = inputData.Replace(":", "");
			string len = bytes.Substring(0, 4);//first 2 bytes are the length!
			bytes = bytes.Substring(4);
			byte[] data = new byte[bytes.Length / 2];

			byte[] lenDat = new byte[2];
			for (int i = 0; i < len.Length; i += 2)
			{
				lenDat[i/2] = Convert.ToByte(len.Substring(i, 2), 16);
			}
			lblPacketLength.Text = "Packet Length: " + Packet.DecodeNumber(lenDat).ToString();

			for (int i = 0; i < bytes.Length; i += 2)
			{
				data[i/2] = Convert.ToByte(bytes.Substring(i, 2), 16);
			}

			m_processor.Decode(ref data);
			Packet pkt = new Packet(data) {ReadPos = m_packetOffset};

			lblFamily.Text = pkt.Family.ToString();
			lblAction.Text = pkt.Action.ToString();

			string decoded = "";
			for (int i = 0; i < data.Length; i++)
			{
				decoded += string.Format("{0} ", data[i].ToString("D3"));
			}
			txtDecoded.Text = decoded;

			switch ((DataTypes) cmbOutputFmt.SelectedIndex)
			{
				case DataTypes.None:
					txtOutput.Text = pkt.GetEndString();
					break;
				case DataTypes.PacketFamily:
					txtOutput.Text = ((PacketFamily) pkt.PeekByte()).ToString();
					break;
				case DataTypes.PacketAction:
					txtOutput.Text = ((PacketAction)pkt.PeekByte()).ToString();
					break;
				case DataTypes.Byte:
					txtOutput.Text = pkt.PeekByte().ToString();
					break;
				case DataTypes.Char:
					txtOutput.Text = pkt.PeekChar().ToString();
					break;
				case DataTypes.Short:
					txtOutput.Text = pkt.PeekShort().ToString();
					break;
				case DataTypes.Three:
					txtOutput.Text = pkt.PeekThree().ToString();
					break;
				case DataTypes.Int:
					txtOutput.Text = pkt.PeekInt().ToString();
					break;
				case DataTypes.BreakString:
					txtOutput.Text = pkt.PeekBreakString();
					break;
				case DataTypes.EndString:
					txtOutput.Text = pkt.PeekEndString();
					break;
				case DataTypes.FixedString:
					txtOutput.Text = pkt.PeekFixedString(m_dataLength);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			int selLen;
			if (m_dataLength > 0) selLen = m_dataLength;
			else
				switch (m_type)
				{
					case DataTypes.EndString:
						selLen = 3*(pkt.Length - pkt.ReadPos) - 1;
						break;
					case DataTypes.BreakString:
						int oldPos = pkt.ReadPos;
						while (pkt.GetByte() != 255) ;
						selLen = pkt.ReadPos - oldPos;
						pkt.ReadPos = oldPos;
						break;
					default:
						selLen = 0;
						break;
				}
			txtDecoded.Select(4 * m_packetOffset, 4 * selLen - 1);

			if (m_type == DataTypes.EndString || m_type == DataTypes.BreakString)
			{
				m_suppressEvent = true;
				txtLength.Text = selLen.ToString();
				m_suppressEvent = false;
			}
		}

		private void btnImportMultis_Click(object sender, EventArgs e)
		{
			string inp = Microsoft.VisualBasic.Interaction.InputBox("Paste the raw, colon-delimited packet data here: ", "Enter packet data");

			if (inp.Length == 0)
				return;

			inp = inp.Replace(":", "");
			if (inp.Length%2 != 0) return;
			inp = inp.Substring(4);
			byte[] data = new byte[inp.Length / 2];
			for (int i = 0; i < inp.Length; i += 2)
				data[i/2] = Convert.ToByte(inp.Substring(i, 2), 16);
			
			//no need to decrypt since it's init data
			Packet pkt = new Packet(data);
			pkt.Skip(3);
			txtDMulti.Text = pkt.GetByte().ToString();
			txtEMulti.Text = pkt.GetByte().ToString();
			m_processor.RecvMulti = pkt.Get()[5];
			m_processor.SendMulti = pkt.Get()[6];
		}
	}
}
