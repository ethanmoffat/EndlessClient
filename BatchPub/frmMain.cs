// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Services;

namespace BatchPub
{
    public partial class frmMain : Form
    {
        private IPubFile<EIFRecord> eif;
        private string _fname;
        private bool changes;

        private enum CompareOperator
        {
            EQ,
            GT,
            LT,
            GTE,
            LTE,
            NE,
            REGEX
        }

        public frmMain()
        {
            InitializeComponent();
            InitializeMore();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            Controls.Clear();
            ResumeLayout();
            InitializeComponent();
            InitializeMore();
            eif = null;
        }

        private void InitializeMore()
        {
            cmbStepTwoField.Items.Clear();
            cmbStepThreeField.Items.Clear();

            var eifType = typeof (EIFRecord);
            foreach(System.Reflection.PropertyInfo prop in eifType.GetProperties())
            {
                cmbStepTwoField.Items.Add(new PropInfo(prop));
                cmbStepThreeField.Items.Add(new PropInfo(prop));
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            //process everything
            System.Reflection.PropertyInfo pi = (cmbStepTwoField.SelectedItem as PropInfo).PropertyInfo;
            object newValue;
            switch ((cmbStepTwoField.SelectedItem as PropInfo).PropertyInfo.Name)
            {
                case "Type":
                case "SubType":
                case "Special":
                case "Size":
                    newValue = cmbStepTwoValue.SelectedIndex;
                    break;
                case "Name":
                    newValue = txtStepTwoValue.Text;
                    break;
                default:
                    int dummy;
                    newValue = txtStepTwoValue.Text.ToString();
                    if (!int.TryParse(newValue.ToString(), out dummy))
                    {
                        MessageBox.Show("Only integer values are supported for the value field in step two.");
                        return;
                    }
                    newValue = dummy;
                    break;
            }

            if(!chkFilterOn.Checked)
            { //process the change for EVERY item record. save changes immediately.
                if (MessageBox.Show(
                    "This change will be processed for every item immediately. The change is irreversible. Are you sure you want to continue? You may specify a filter by checking the filter option.", "No filter selected", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Exclamation) == DialogResult.No
                    )
                {
                    return;
                }

                rtfOutput.Text += "Processing change: set " + pi.Name + "(" + pi.PropertyType.ToString() + ")=" + newValue.ToString() + " for all items...";
                foreach (var rec in eif.Data)
                {
                    System.Reflection.PropertyInfo prop = rec.GetType().GetProperty(pi.Name);
                    prop.SetValue(rec, Convert.ChangeType(newValue, pi.PropertyType));
                }

                rtfOutput.Text += "done.\n\n";
            }
            else
            {
                object compareValue;
                CompareOperator op;
                try
                {
                    op = (CompareOperator)cmbCompareType.SelectedIndex;
                    if(op == CompareOperator.REGEX && cmbCompareValue.Enabled)
                    {
                        MessageBox.Show("You can't use a regex to parse enumerated types");
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Invalid comparison operator specified.");
                    return;
                }

                Regex comparer = null;
                if (op == CompareOperator.REGEX)
                {
                    try
                    {
                        compareValue = txtCompareValue.Text;
                        comparer = new Regex(compareValue.ToString());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Unable to parse regular expression:\n " + ex.Message, "Error!");
                        return;
                    }
                }
                else
                {
                    switch ((cmbStepThreeField.SelectedItem as PropInfo).PropertyInfo.Name)
                    {
                        case "Type":
                            compareValue = (ItemType)cmbCompareValue.SelectedIndex;
                            break;
                        case "SubType":
                            compareValue = (ItemSubType)cmbCompareValue.SelectedIndex;
                            break;
                        case "Special":
                            compareValue = (ItemSpecial)cmbCompareValue.SelectedIndex;
                            break;
                        case "Size":
                            compareValue = (ItemSize)cmbCompareValue.SelectedIndex;
                            break;
                        case "Name":
                            compareValue = txtCompareValue.Text;
                            break;
                        default:
                            compareValue = txtCompareValue.Text;
                            int dummy;
                            if (!int.TryParse(compareValue.ToString(), out dummy))
                            {
                                MessageBox.Show("Only integer values are allowed for this comparison type.", "Error parsing");
                                return;
                            }
                            compareValue = dummy;
                            break;
                    }
                }

                List<EIFRecord> filtered = eif.Data.Where(record =>
                    {
                        EIFRecord rec = (EIFRecord)record;
                        if (rec == null || rec.ID == 0) return false;

                        System.Reflection.PropertyInfo comparePropertyInfo = (cmbStepThreeField.SelectedItem as PropInfo).PropertyInfo;
                        System.Reflection.PropertyInfo currentInfo = rec.GetType().GetProperty(comparePropertyInfo.Name);
                        switch (op)
                        {
                            case CompareOperator.EQ:
                                return compareValue.ToString() == currentInfo.GetValue(rec).ToString();
                            case CompareOperator.GT:
                                return compareValue.ToString().CompareTo(currentInfo.GetValue(rec).ToString()) > 0;
                            case CompareOperator.GTE:
                                return compareValue.ToString().CompareTo(currentInfo.GetValue(rec).ToString()) >= 0;
                            case CompareOperator.LT:
                                return compareValue.ToString().CompareTo(currentInfo.GetValue(rec).ToString()) < 0;
                            case CompareOperator.LTE:
                                return compareValue.ToString().CompareTo(currentInfo.GetValue(rec).ToString()) <= 0;
                            case CompareOperator.NE:
                                return compareValue.ToString() != currentInfo.GetValue(rec).ToString();
                            case CompareOperator.REGEX:
                                object curValAsString = currentInfo.GetValue(rec);
                                if (curValAsString == null)
                                    return false;

                                return comparer.IsMatch(curValAsString.ToString());
                            default:
                                return false;
                        }
                    }).ToList();

                filtered.ForEach(rec =>
                {
                    if (!changes)
                        changes = true;

                    int index = rec.ID - 1;

                    rtfOutput.Text += "Found matching item " + rec.Name + " (" + rec.ID + ")\n";
                    rtfOutput.Text += "  replacing " + pi.Name + " (currently " + pi.GetValue(rec).ToString() + ") with new value " + newValue.ToString() + "\n";
                    rtfOutput.SelectionStart = rtfOutput.TextLength;
                    rtfOutput.ScrollToCaret();

                    object setter;
                    //enums are special: convert them to object
                    if (pi.PropertyType == typeof(ItemType) ||
                        pi.PropertyType == typeof(ItemSubType) ||
                        pi.PropertyType == typeof(ItemSpecial) ||
                        pi.PropertyType == typeof(ItemSize))
                        setter = Convert.ChangeType(Enum.ToObject(pi.PropertyType, newValue), pi.PropertyType);
                    else
                        setter = Convert.ChangeType(newValue, pi.PropertyType);
                    
                    pi.SetValue(rec, setter);

                    //eif.ReplaceRecordAt(index, rec); //todo: way to modify pub files
                });
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.AddExtension = true;
                ofd.Filter = "Endless Online Item Data File|*.eif";
                ofd.Multiselect = false;
                ofd.InitialDirectory = Application.ExecutablePath;

                if (ofd.ShowDialog() == DialogResult.Cancel)
                    return;

                txtFileName.Text = ofd.FileName;
            }
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            eif = new EIFFile();

            _fname = "";
            try
            {
                var fileBytes = File.ReadAllBytes(_fname = string.IsNullOrEmpty(txtFileName.Text) ? PubFileNameConstants.PathToEIFFile : txtFileName.Text);
                eif.DeserializeFromByteArray(fileBytes, new NumberEncoderService());
                lblFileName.Text = "Loaded file: " + _fname;
                grpStepTwo.Enabled = true;
                btnReset.Enabled = true;
            }
            catch(Exception ex)
            {
                eif = null;
                MessageBox.Show("Error loading " + _fname + ":\n" + ex.Message, "Error!");
            }
        }

        private void cmbStepTwoField_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbStepTwoValue.Items.Clear();
            txtStepTwoValue.Enabled = false;
            cmbStepTwoValue.Enabled = true;
            switch ((cmbStepTwoField.SelectedItem as PropInfo).DisplayName)
            {
                case "Type":
                    foreach (string val in Enum.GetNames(typeof(ItemType)))
                        cmbStepTwoValue.Items.Add(val);
                    break;
                case "SubType":
                    foreach (string val in Enum.GetNames(typeof(ItemSubType)))
                        cmbStepTwoValue.Items.Add(val);
                    break;
                case "Special":
                    foreach (string val in Enum.GetNames(typeof(ItemSpecial)))
                        cmbStepTwoValue.Items.Add(val);
                    break;
                case "Size":
                    foreach (string val in Enum.GetNames(typeof(ItemSize)))
                        cmbStepTwoValue.Items.Add(val);
                    break;
                default:
                    txtStepTwoValue.Enabled = true;
                    cmbStepTwoValue.Enabled = false;
                    break;
            }
        }

        private void LeaveStepTwo(object sender, EventArgs e)
        {
            if(cmbStepTwoField.SelectedIndex >= 0 && 
                ((txtStepTwoValue.Enabled && txtStepTwoValue.TextLength > 0) || (cmbStepTwoValue.Enabled && cmbStepTwoValue.SelectedIndex >= 0))
                )
            {
                grpStepThree.Enabled = true;
                btnProcess.Enabled = true;
            }
            else
            {
                grpStepThree.Enabled = false;
                btnProcess.Enabled = false;
            }
        }

        private void chkFilterOn_CheckedChanged(object sender, EventArgs e)
        {
            if(chkFilterOn.Checked)
            {
                cmbStepThreeField.Enabled = true;
                txtCompareValue.Enabled = true;
                cmbCompareType.Enabled = true;
                cmbCompareType.SelectedIndex = 0;
                cmbStepThreeField.SelectedIndex = 0;
            }
            else
            {
                cmbStepThreeField.Enabled = false;
                txtCompareValue.Enabled = false;
                cmbCompareType.Enabled = false;
                cmbCompareType.SelectedIndex = -1;
                cmbStepThreeField.SelectedIndex = -1;
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            MessageBox.Show("It is highly recommended that you back up your pub files before using this utility. Changes are irreversable.", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void cmbStepThreeField_SelectedIndexChanged(object sender, EventArgs e)
        {
            SuspendLayout();
            cmbCompareValue.Items.Clear();
            cmbCompareValue.Enabled = true;
            cmbCompareValue.Visible = true;
            txtCompareValue.Enabled = false;
            txtCompareValue.Visible = false;

            if (cmbStepThreeField.SelectedItem == null)
            {
                ResumeLayout();
                return;
            }

            switch ((cmbStepThreeField.SelectedItem as PropInfo).PropertyInfo.Name)
            {
                case "Type":
                    foreach (string val in Enum.GetNames(typeof(ItemType)))
                        cmbCompareValue.Items.Add(val);
                    break;
                case "SubType":
                    foreach (string val in Enum.GetNames(typeof(ItemSubType)))
                        cmbCompareValue.Items.Add(val);
                    break;
                case "Special":
                    foreach (string val in Enum.GetNames(typeof(ItemSpecial)))
                        cmbCompareValue.Items.Add(val);
                    break;
                case "Size":
                    foreach (string val in Enum.GetNames(typeof(ItemSize)))
                        cmbCompareValue.Items.Add(val);
                    break;
                default:
                    cmbCompareValue.Enabled = false;
                    cmbCompareValue.Visible = false;
                    txtCompareValue.Enabled = true;
                    txtCompareValue.Visible = true;
                    break;
            }
            ResumeLayout();
        }

        private void rtfOutput_TextChanged(object sender, EventArgs e)
        {
            if (rtfOutput.TextLength > 0)
                btnClear.Enabled = true;
            else
                btnClear.Enabled = false;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            rtfOutput.Text = "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(changes)
            {
                DialogResult dr = MessageBox.Show("WARNING: This will update the pub file you currently have open with any batch changes that have been made. Confirm that this behavior is desired.", "Confirm changes?", MessageBoxButtons.YesNo);
                if (dr == DialogResult.No)
                    return;

                //var version = eif.Version;
                //if (eif.Version == 0)
                //{
                //    version = 1;
                //    MessageBox.Show("Saving using this tool will update the version of the EIF file.");
                //}

                try
                {
                    eif.CheckSum++;//todo: recalculate checksum
                    var bytes = eif.SerializeToByteArray(new NumberEncoderService());
                }
                catch(Exception ex)
                {
                    MessageBox.Show("Error saving changes to the file:\n" + ex.Message);
                    return;
                }

                MessageBox.Show("Changes saved.");
            }
            else
            {
                MessageBox.Show("Changes to the pub file were not detected. No changes have been processed.");
            }
        }
    }

    public class PropInfo
    {
        public System.Reflection.PropertyInfo PropertyInfo { get; set; }
        public string DisplayName => PropertyInfo.Name;

        public PropInfo(System.Reflection.PropertyInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("Info parameter may not be null.");
            PropertyInfo = info;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
