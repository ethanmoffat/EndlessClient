namespace PacketDecoder
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtEMulti = new System.Windows.Forms.TextBox();
            this.grpMultiples = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtDMulti = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpData = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtOffset = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txtLength = new System.Windows.Forms.TextBox();
            this.grpInput = new System.Windows.Forms.GroupBox();
            this.txtInputData = new System.Windows.Forms.TextBox();
            this.grpOutput = new System.Windows.Forms.GroupBox();
            this.lblAction = new System.Windows.Forms.Label();
            this.lblFamily = new System.Windows.Forms.Label();
            this.lblPacketLength = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtDecoded = new System.Windows.Forms.TextBox();
            this.txtOutput = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cmbOutputFmt = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnImportMultis = new System.Windows.Forms.Button();
            this.grpMultiples.SuspendLayout();
            this.grpData.SuspendLayout();
            this.grpInput.SuspendLayout();
            this.grpOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtEMulti
            // 
            this.txtEMulti.Location = new System.Drawing.Point(69, 19);
            this.txtEMulti.Name = "txtEMulti";
            this.txtEMulti.Size = new System.Drawing.Size(43, 20);
            this.txtEMulti.TabIndex = 1;
            this.txtEMulti.Leave += new System.EventHandler(this.intTextValidate);
            // 
            // grpMultiples
            // 
            this.grpMultiples.Controls.Add(this.label2);
            this.grpMultiples.Controls.Add(this.txtDMulti);
            this.grpMultiples.Controls.Add(this.label1);
            this.grpMultiples.Controls.Add(this.txtEMulti);
            this.grpMultiples.Location = new System.Drawing.Point(12, 12);
            this.grpMultiples.Name = "grpMultiples";
            this.grpMultiples.Size = new System.Drawing.Size(123, 74);
            this.grpMultiples.TabIndex = 0;
            this.grpMultiples.TabStop = false;
            this.grpMultiples.Text = "Multiples";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Decryption";
            // 
            // txtDMulti
            // 
            this.txtDMulti.Location = new System.Drawing.Point(69, 45);
            this.txtDMulti.Name = "txtDMulti";
            this.txtDMulti.Size = new System.Drawing.Size(43, 20);
            this.txtDMulti.TabIndex = 3;
            this.txtDMulti.Leave += new System.EventHandler(this.intTextValidate);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Encryption";
            // 
            // grpData
            // 
            this.grpData.Controls.Add(this.label4);
            this.grpData.Controls.Add(this.txtOffset);
            this.grpData.Location = new System.Drawing.Point(141, 12);
            this.grpData.Name = "grpData";
            this.grpData.Size = new System.Drawing.Size(123, 50);
            this.grpData.TabIndex = 1;
            this.grpData.TabStop = false;
            this.grpData.Text = "Data";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Offset";
            // 
            // txtOffset
            // 
            this.txtOffset.Location = new System.Drawing.Point(69, 19);
            this.txtOffset.Name = "txtOffset";
            this.txtOffset.Size = new System.Drawing.Size(43, 20);
            this.txtOffset.TabIndex = 1;
            this.txtOffset.Leave += new System.EventHandler(this.intTextValidate);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(157, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Length";
            // 
            // txtLength
            // 
            this.txtLength.Enabled = false;
            this.txtLength.Location = new System.Drawing.Point(203, 19);
            this.txtLength.Name = "txtLength";
            this.txtLength.Size = new System.Drawing.Size(43, 20);
            this.txtLength.TabIndex = 4;
            this.txtLength.TextChanged += new System.EventHandler(this.intTextValidate);
            this.txtLength.Leave += new System.EventHandler(this.intTextValidate);
            // 
            // grpInput
            // 
            this.grpInput.Controls.Add(this.txtInputData);
            this.grpInput.Location = new System.Drawing.Point(12, 92);
            this.grpInput.Name = "grpInput";
            this.grpInput.Size = new System.Drawing.Size(252, 100);
            this.grpInput.TabIndex = 2;
            this.grpInput.TabStop = false;
            this.grpInput.Text = "Packet Data";
            // 
            // txtInputData
            // 
            this.txtInputData.Location = new System.Drawing.Point(6, 19);
            this.txtInputData.Multiline = true;
            this.txtInputData.Name = "txtInputData";
            this.txtInputData.Size = new System.Drawing.Size(240, 75);
            this.txtInputData.TabIndex = 0;
            // 
            // grpOutput
            // 
            this.grpOutput.Controls.Add(this.lblAction);
            this.grpOutput.Controls.Add(this.lblFamily);
            this.grpOutput.Controls.Add(this.lblPacketLength);
            this.grpOutput.Controls.Add(this.label7);
            this.grpOutput.Controls.Add(this.txtDecoded);
            this.grpOutput.Controls.Add(this.txtLength);
            this.grpOutput.Controls.Add(this.label3);
            this.grpOutput.Controls.Add(this.txtOutput);
            this.grpOutput.Controls.Add(this.label6);
            this.grpOutput.Controls.Add(this.cmbOutputFmt);
            this.grpOutput.Controls.Add(this.label5);
            this.grpOutput.Location = new System.Drawing.Point(12, 198);
            this.grpOutput.Name = "grpOutput";
            this.grpOutput.Size = new System.Drawing.Size(252, 292);
            this.grpOutput.TabIndex = 3;
            this.grpOutput.TabStop = false;
            this.grpOutput.Text = "Output";
            // 
            // lblAction
            // 
            this.lblAction.AutoSize = true;
            this.lblAction.Location = new System.Drawing.Point(126, 75);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(43, 13);
            this.lblAction.TabIndex = 9;
            this.lblAction.Text = "Action: ";
            // 
            // lblFamily
            // 
            this.lblFamily.AutoSize = true;
            this.lblFamily.Location = new System.Drawing.Point(7, 75);
            this.lblFamily.Name = "lblFamily";
            this.lblFamily.Size = new System.Drawing.Size(42, 13);
            this.lblFamily.TabIndex = 8;
            this.lblFamily.Text = "Family: ";
            // 
            // lblPacketLength
            // 
            this.lblPacketLength.AutoSize = true;
            this.lblPacketLength.Location = new System.Drawing.Point(7, 48);
            this.lblPacketLength.Name = "lblPacketLength";
            this.lblPacketLength.Size = new System.Drawing.Size(83, 13);
            this.lblPacketLength.TabIndex = 7;
            this.lblPacketLength.Text = "Packet Length: ";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 161);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Decoded Packet";
            // 
            // txtDecoded
            // 
            this.txtDecoded.HideSelection = false;
            this.txtDecoded.Location = new System.Drawing.Point(6, 177);
            this.txtDecoded.Multiline = true;
            this.txtDecoded.Name = "txtDecoded";
            this.txtDecoded.ReadOnly = true;
            this.txtDecoded.Size = new System.Drawing.Size(240, 109);
            this.txtDecoded.TabIndex = 1;
            // 
            // txtOutput
            // 
            this.txtOutput.Location = new System.Drawing.Point(50, 101);
            this.txtOutput.Multiline = true;
            this.txtOutput.Name = "txtOutput";
            this.txtOutput.ReadOnly = true;
            this.txtOutput.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtOutput.Size = new System.Drawing.Size(196, 57);
            this.txtOutput.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 104);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(37, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "Result";
            // 
            // cmbOutputFmt
            // 
            this.cmbOutputFmt.FormattingEnabled = true;
            this.cmbOutputFmt.Items.AddRange(new object[] {
            "PacketFamily",
            "PacketAction",
            "Byte",
            "Char",
            "Short",
            "Three",
            "Int",
            "BreakString",
            "EndString",
            "FixedString"});
            this.cmbOutputFmt.Location = new System.Drawing.Point(52, 19);
            this.cmbOutputFmt.Name = "cmbOutputFmt";
            this.cmbOutputFmt.Size = new System.Drawing.Size(99, 21);
            this.cmbOutputFmt.TabIndex = 2;
            this.cmbOutputFmt.SelectedIndexChanged += new System.EventHandler(this.cmbOutputFmt_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(39, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Format";
            // 
            // btnImportMultis
            // 
            this.btnImportMultis.Location = new System.Drawing.Point(141, 68);
            this.btnImportMultis.Name = "btnImportMultis";
            this.btnImportMultis.Size = new System.Drawing.Size(123, 23);
            this.btnImportMultis.TabIndex = 4;
            this.btnImportMultis.Text = "Import Multiples";
            this.btnImportMultis.UseVisualStyleBackColor = true;
            this.btnImportMultis.Click += new System.EventHandler(this.btnImportMultis_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(275, 502);
            this.Controls.Add(this.btnImportMultis);
            this.Controls.Add(this.grpOutput);
            this.Controls.Add(this.grpInput);
            this.Controls.Add(this.grpData);
            this.Controls.Add(this.grpMultiples);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "EO Packet Decoder";
            this.grpMultiples.ResumeLayout(false);
            this.grpMultiples.PerformLayout();
            this.grpData.ResumeLayout(false);
            this.grpData.PerformLayout();
            this.grpInput.ResumeLayout(false);
            this.grpInput.PerformLayout();
            this.grpOutput.ResumeLayout(false);
            this.grpOutput.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtEMulti;
        private System.Windows.Forms.GroupBox grpMultiples;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtDMulti;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpData;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLength;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtOffset;
        private System.Windows.Forms.GroupBox grpInput;
        private System.Windows.Forms.TextBox txtInputData;
        private System.Windows.Forms.GroupBox grpOutput;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cmbOutputFmt;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtDecoded;
        private System.Windows.Forms.Label lblPacketLength;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.Label lblFamily;
        private System.Windows.Forms.Button btnImportMultis;
    }
}

