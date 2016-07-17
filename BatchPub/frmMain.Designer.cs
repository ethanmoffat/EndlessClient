namespace BatchPub
{
    partial class frmMain
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
            this.grpStepOne = new System.Windows.Forms.GroupBox();
            this.btnLoad = new System.Windows.Forms.Button();
            this.lblFileName = new System.Windows.Forms.Label();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.grpStepTwo = new System.Windows.Forms.GroupBox();
            this.cmbStepTwoValue = new System.Windows.Forms.ComboBox();
            this.txtStepTwoValue = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbStepTwoField = new System.Windows.Forms.ComboBox();
            this.grpStepThree = new System.Windows.Forms.GroupBox();
            this.chkFilterOn = new System.Windows.Forms.CheckBox();
            this.cmbCompareType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbStepThreeField = new System.Windows.Forms.ComboBox();
            this.txtCompareValue = new System.Windows.Forms.TextBox();
            this.cmbCompareValue = new System.Windows.Forms.ComboBox();
            this.grpOutput = new System.Windows.Forms.GroupBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.rtfOutput = new System.Windows.Forms.RichTextBox();
            this.btnProcess = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.grpStepOne.SuspendLayout();
            this.grpStepTwo.SuspendLayout();
            this.grpStepThree.SuspendLayout();
            this.grpOutput.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpStepOne
            // 
            this.grpStepOne.Controls.Add(this.btnLoad);
            this.grpStepOne.Controls.Add(this.lblFileName);
            this.grpStepOne.Controls.Add(this.btnBrowse);
            this.grpStepOne.Controls.Add(this.txtFileName);
            this.grpStepOne.Location = new System.Drawing.Point(12, 12);
            this.grpStepOne.Name = "grpStepOne";
            this.grpStepOne.Size = new System.Drawing.Size(260, 86);
            this.grpStepOne.TabIndex = 0;
            this.grpStepOne.TabStop = false;
            this.grpStepOne.Text = "Step One: Load Pub";
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(207, 17);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(47, 23);
            this.btnLoad.TabIndex = 3;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // lblFileName
            // 
            this.lblFileName.Location = new System.Drawing.Point(6, 43);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(248, 36);
            this.lblFileName.TabIndex = 2;
            this.lblFileName.Text = "No file loaded.";
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(175, 17);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(26, 23);
            this.btnBrowse.TabIndex = 1;
            this.btnBrowse.Text = "...";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(6, 19);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(163, 20);
            this.txtFileName.TabIndex = 0;
            // 
            // grpStepTwo
            // 
            this.grpStepTwo.Controls.Add(this.cmbStepTwoValue);
            this.grpStepTwo.Controls.Add(this.txtStepTwoValue);
            this.grpStepTwo.Controls.Add(this.label3);
            this.grpStepTwo.Controls.Add(this.label2);
            this.grpStepTwo.Controls.Add(this.cmbStepTwoField);
            this.grpStepTwo.Enabled = false;
            this.grpStepTwo.Location = new System.Drawing.Point(12, 104);
            this.grpStepTwo.Name = "grpStepTwo";
            this.grpStepTwo.Size = new System.Drawing.Size(260, 74);
            this.grpStepTwo.TabIndex = 1;
            this.grpStepTwo.TabStop = false;
            this.grpStepTwo.Text = "Step Two: Choose Field/Value";
            // 
            // cmbStepTwoValue
            // 
            this.cmbStepTwoValue.Enabled = false;
            this.cmbStepTwoValue.FormattingEnabled = true;
            this.cmbStepTwoValue.Location = new System.Drawing.Point(148, 46);
            this.cmbStepTwoValue.Name = "cmbStepTwoValue";
            this.cmbStepTwoValue.Size = new System.Drawing.Size(106, 21);
            this.cmbStepTwoValue.TabIndex = 4;
            this.cmbStepTwoValue.SelectedIndexChanged += new System.EventHandler(this.LeaveStepTwo);
            this.cmbStepTwoValue.Leave += new System.EventHandler(this.LeaveStepTwo);
            // 
            // txtStepTwoValue
            // 
            this.txtStepTwoValue.Location = new System.Drawing.Point(79, 46);
            this.txtStepTwoValue.Name = "txtStepTwoValue";
            this.txtStepTwoValue.Size = new System.Drawing.Size(63, 20);
            this.txtStepTwoValue.TabIndex = 3;
            this.txtStepTwoValue.TextChanged += new System.EventHandler(this.LeaveStepTwo);
            this.txtStepTwoValue.Leave += new System.EventHandler(this.LeaveStepTwo);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(32, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Field:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Set value to:";
            // 
            // cmbStepTwoField
            // 
            this.cmbStepTwoField.FormattingEnabled = true;
            this.cmbStepTwoField.Location = new System.Drawing.Point(44, 19);
            this.cmbStepTwoField.Name = "cmbStepTwoField";
            this.cmbStepTwoField.Size = new System.Drawing.Size(98, 21);
            this.cmbStepTwoField.TabIndex = 0;
            this.cmbStepTwoField.SelectedIndexChanged += new System.EventHandler(this.cmbStepTwoField_SelectedIndexChanged);
            this.cmbStepTwoField.Leave += new System.EventHandler(this.LeaveStepTwo);
            // 
            // grpStepThree
            // 
            this.grpStepThree.Controls.Add(this.chkFilterOn);
            this.grpStepThree.Controls.Add(this.cmbCompareType);
            this.grpStepThree.Controls.Add(this.label4);
            this.grpStepThree.Controls.Add(this.cmbStepThreeField);
            this.grpStepThree.Controls.Add(this.txtCompareValue);
            this.grpStepThree.Controls.Add(this.cmbCompareValue);
            this.grpStepThree.Enabled = false;
            this.grpStepThree.Location = new System.Drawing.Point(12, 184);
            this.grpStepThree.Name = "grpStepThree";
            this.grpStepThree.Size = new System.Drawing.Size(260, 77);
            this.grpStepThree.TabIndex = 2;
            this.grpStepThree.TabStop = false;
            this.grpStepThree.Text = "Step Three: Search Criteria";
            // 
            // chkFilterOn
            // 
            this.chkFilterOn.AutoSize = true;
            this.chkFilterOn.Location = new System.Drawing.Point(7, 36);
            this.chkFilterOn.Name = "chkFilterOn";
            this.chkFilterOn.Size = new System.Drawing.Size(48, 17);
            this.chkFilterOn.TabIndex = 9;
            this.chkFilterOn.Text = "Filter";
            this.chkFilterOn.UseVisualStyleBackColor = true;
            this.chkFilterOn.CheckedChanged += new System.EventHandler(this.chkFilterOn_CheckedChanged);
            // 
            // cmbCompareType
            // 
            this.cmbCompareType.Enabled = false;
            this.cmbCompareType.FormattingEnabled = true;
            this.cmbCompareType.Items.AddRange(new object[] {
            "==",
            ">",
            "<",
            ">=",
            "<=",
            "!=",
            "regex"});
            this.cmbCompareType.Location = new System.Drawing.Point(58, 46);
            this.cmbCompareType.Name = "cmbCompareType";
            this.cmbCompareType.Size = new System.Drawing.Size(65, 21);
            this.cmbCompareType.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(55, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(32, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Field:";
            // 
            // cmbStepThreeField
            // 
            this.cmbStepThreeField.Enabled = false;
            this.cmbStepThreeField.FormattingEnabled = true;
            this.cmbStepThreeField.Location = new System.Drawing.Point(93, 19);
            this.cmbStepThreeField.Name = "cmbStepThreeField";
            this.cmbStepThreeField.Size = new System.Drawing.Size(151, 21);
            this.cmbStepThreeField.TabIndex = 3;
            this.cmbStepThreeField.SelectedIndexChanged += new System.EventHandler(this.cmbStepThreeField_SelectedIndexChanged);
            // 
            // txtCompareValue
            // 
            this.txtCompareValue.Enabled = false;
            this.txtCompareValue.Location = new System.Drawing.Point(129, 47);
            this.txtCompareValue.Name = "txtCompareValue";
            this.txtCompareValue.Size = new System.Drawing.Size(115, 20);
            this.txtCompareValue.TabIndex = 8;
            // 
            // cmbCompareValue
            // 
            this.cmbCompareValue.Enabled = false;
            this.cmbCompareValue.FormattingEnabled = true;
            this.cmbCompareValue.Location = new System.Drawing.Point(129, 46);
            this.cmbCompareValue.Name = "cmbCompareValue";
            this.cmbCompareValue.Size = new System.Drawing.Size(115, 21);
            this.cmbCompareValue.TabIndex = 10;
            this.cmbCompareValue.Visible = false;
            // 
            // grpOutput
            // 
            this.grpOutput.Controls.Add(this.btnClear);
            this.grpOutput.Controls.Add(this.rtfOutput);
            this.grpOutput.Location = new System.Drawing.Point(278, 12);
            this.grpOutput.Name = "grpOutput";
            this.grpOutput.Size = new System.Drawing.Size(277, 219);
            this.grpOutput.TabIndex = 3;
            this.grpOutput.TabStop = false;
            this.grpOutput.Text = "Results/Output";
            // 
            // btnClear
            // 
            this.btnClear.Enabled = false;
            this.btnClear.Location = new System.Drawing.Point(101, 190);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 1;
            this.btnClear.Text = "Clear Output";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // rtfOutput
            // 
            this.rtfOutput.BackColor = System.Drawing.SystemColors.Control;
            this.rtfOutput.Location = new System.Drawing.Point(6, 19);
            this.rtfOutput.Name = "rtfOutput";
            this.rtfOutput.ReadOnly = true;
            this.rtfOutput.Size = new System.Drawing.Size(265, 165);
            this.rtfOutput.TabIndex = 0;
            this.rtfOutput.Text = "";
            this.rtfOutput.TextChanged += new System.EventHandler(this.rtfOutput_TextChanged);
            // 
            // btnProcess
            // 
            this.btnProcess.Enabled = false;
            this.btnProcess.Location = new System.Drawing.Point(337, 237);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(75, 23);
            this.btnProcess.TabIndex = 4;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(480, 237);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(75, 23);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnReset
            // 
            this.btnReset.Enabled = false;
            this.btnReset.Location = new System.Drawing.Point(278, 237);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(53, 23);
            this.btnReset.TabIndex = 4;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(418, 237);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(56, 23);
            this.btnSave.TabIndex = 5;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 272);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnProcess);
            this.Controls.Add(this.grpOutput);
            this.Controls.Add(this.grpStepThree);
            this.Controls.Add(this.grpStepTwo);
            this.Controls.Add(this.grpStepOne);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.Text = "PubBatch";
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.grpStepOne.ResumeLayout(false);
            this.grpStepOne.PerformLayout();
            this.grpStepTwo.ResumeLayout(false);
            this.grpStepTwo.PerformLayout();
            this.grpStepThree.ResumeLayout(false);
            this.grpStepThree.PerformLayout();
            this.grpOutput.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpStepOne;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.GroupBox grpStepTwo;
        private System.Windows.Forms.ComboBox cmbStepTwoValue;
        private System.Windows.Forms.TextBox txtStepTwoValue;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cmbStepTwoField;
        private System.Windows.Forms.GroupBox grpStepThree;
        private System.Windows.Forms.ComboBox cmbCompareType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cmbStepThreeField;
        private System.Windows.Forms.CheckBox chkFilterOn;
        private System.Windows.Forms.TextBox txtCompareValue;
        private System.Windows.Forms.GroupBox grpOutput;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.RichTextBox rtfOutput;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.ComboBox cmbCompareValue;
        private System.Windows.Forms.Button btnSave;
    }
}

