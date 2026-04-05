using System;
using System.Windows.Forms;
using AuraMacro.Core.Workflow;

namespace AuraMacro.ConsoleUI
{
    public class SettingsForm : Form
    {
        private MacroWorkflow _workflow;

        private ComboBox _cboFailureBehavior;
        private TextBox _txtAlertMessage;
        private NumericUpDown _numCooldown;
        private Button _btnSave;

        public SettingsForm(MacroWorkflow workflow)
        {
            _workflow = workflow;
            InitializeComponent();
            LoadSettings();
        }

        private void InitializeComponent()
        {
            this.Text = "Macro Settings";
            this.Size = new System.Drawing.Size(350, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            var lblBehavior = new Label { Text = "On Failure Behavior:", Location = new System.Drawing.Point(20, 20), AutoSize = true };
            _cboFailureBehavior = new ComboBox
            {
                Location = new System.Drawing.Point(150, 20),
                Size = new System.Drawing.Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cboFailureBehavior.Items.AddRange(Enum.GetNames(typeof(FailureBehavior)));

            var lblAlert = new Label { Text = "Alert Message:", Location = new System.Drawing.Point(20, 60), AutoSize = true };
            _txtAlertMessage = new TextBox
            {
                Location = new System.Drawing.Point(150, 60),
                Size = new System.Drawing.Size(150, 25)
            };

            var lblCooldown = new Label { Text = "OCR Cooldown (ms):", Location = new System.Drawing.Point(20, 100), AutoSize = true };
            _numCooldown = new NumericUpDown
            {
                Location = new System.Drawing.Point(150, 100),
                Size = new System.Drawing.Size(150, 25),
                Minimum = 0,
                Maximum = 10000
            };

            _btnSave = new Button { Text = "Save", Location = new System.Drawing.Point(120, 160), Size = new System.Drawing.Size(100, 30) };
            _btnSave.Click += BtnSave_Click;

            this.Controls.Add(lblBehavior);
            this.Controls.Add(_cboFailureBehavior);
            this.Controls.Add(lblAlert);
            this.Controls.Add(_txtAlertMessage);
            this.Controls.Add(lblCooldown);
            this.Controls.Add(_numCooldown);
            this.Controls.Add(_btnSave);
        }

        private void LoadSettings()
        {
            _cboFailureBehavior.SelectedItem = _workflow.GlobalOnFailureBehavior.ToString();
            _txtAlertMessage.Text = _workflow.FailureAlertMessage;
            _numCooldown.Value = _workflow.OcrCooldownMilliseconds;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (Enum.TryParse<FailureBehavior>(_cboFailureBehavior.SelectedItem?.ToString(), out var behavior))
            {
                _workflow.GlobalOnFailureBehavior = behavior;
            }
            _workflow.FailureAlertMessage = _txtAlertMessage.Text;
            _workflow.OcrCooldownMilliseconds = (int)_numCooldown.Value;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}