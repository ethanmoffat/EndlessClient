﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using EOLib.Config;
using EOLib.IO.Services;
using EOLib.Logger;
using EOLib.Net;
using EOLib.Net.PacketProcessing;

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
        private readonly IPacketProcessActions _packetProcessActions;
        private readonly IPacketEncoderRepository _packetEncoderRepository;
        private int m_packetOffset, m_dataLength;
        private bool m_suppressEvent;

        public MainForm()
        {
            InitializeComponent();

            cmbOutputFmt_SelectedIndexChanged(null, null);

            _packetEncoderRepository = new PacketEncoderRepository();
            _packetProcessActions = new PacketProcessActions(new SequenceRepository(),
                                                             _packetEncoderRepository,
                                                             new PacketEncoderService(new NumberEncoderService(), new DataEncoderService()),
                                                             new PacketSequenceService(),
                                                             new LoggerProvider(new LoggerFactory(new ConfigurationRepository())));
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
                _packetProcessActions.SetEncodeMultiples(param, _packetEncoderRepository.SendMultiplier);
                if (param < 6 || param > 12)
                {
                    txtDMulti.BackColor = Color.FromArgb(255, 255, 128, 128);
                }
                else
                {
                    txtDMulti.BackColor = Color.White;
                }
            }
            else if (txt == txtEMulti)
            {
                _packetProcessActions.SetEncodeMultiples(_packetEncoderRepository.ReceiveMultiplier, param);
                if (param < 6 || param > 12)
                {
                    txtEMulti.BackColor = Color.FromArgb(255, 255, 128, 128);
                }
                else
                {
                    txtEMulti.BackColor = Color.White;
                }
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
            lblPacketLength.Text = "Packet Length: " + new NumberEncoderService().DecodeNumber(lenDat).ToString();

            for (int i = 0; i < bytes.Length; i += 2)
            {
                data[i/2] = Convert.ToByte(bytes.Substring(i, 2), 16);
            }

            var pkt = _packetProcessActions.DecodeData(data);
            pkt.Seek(m_packetOffset, SeekOrigin.Begin);

            lblFamily.Text = pkt.Family.ToString();
            lblAction.Text = pkt.Action.ToString();

            string decoded = "";
            for (int i = 0; i < pkt.Length; i++)
            {
                decoded += $"{pkt.RawData[i].ToString("D3")} ";
            }
            txtDecoded.Text = decoded;

            switch ((DataTypes) cmbOutputFmt.SelectedIndex)
            {
                case DataTypes.None:
                    txtOutput.Text = pkt.ReadEndString();
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
                    txtOutput.Text = pkt.PeekString(m_dataLength);
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
                        selLen = 3*(pkt.Length - pkt.ReadPosition) - 1;
                        break;
                    case DataTypes.BreakString:
                        int oldPos = pkt.ReadPosition;
                        while (pkt.ReadByte() != 255) ;
                        selLen = pkt.ReadPosition - oldPos;
                        pkt.Seek(oldPos, SeekOrigin.Begin);
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
            var pkt = new Packet(data);
            pkt.Seek(3, SeekOrigin.Current);
            txtDMulti.Text = pkt.ReadByte().ToString();
            txtEMulti.Text = pkt.ReadByte().ToString();
            _packetProcessActions.SetEncodeMultiples(pkt.RawData[5], pkt.RawData[6]);
        }
    }
}
