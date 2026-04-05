using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AuraMacro.Core.Workflow;
using AuraMacro.Core.Models;
using AuraMacro.Core.Mocks;
using AuraMacro.Core.Engine;

namespace AuraMacro.ConsoleUI
{
    public partial class MainForm : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vlc);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 1;
        private const int MOD_CTRL = 0x0002;
        private const int VK_F9 = 0x78;
        private const int WM_HOTKEY = 0x0312;

        private MacroWorkflow _currentWorkflow = new MacroWorkflow();
        private ListBox _lstTimeline;
        private Button _btnAddAction;
        private Button _btnDeleteAction;
        private Button _btnMoveUp;
        private Button _btnMoveDown;
        private Button _btnRun;
        private Button _btnSave;
        private Button _btnLoad;
        private Button _btnSettings;
        private Label _lblStatus;

        public MainForm()
        {
            InitializeComponent();
            RefreshTimeline();

            // Register Ctrl+F9 as a global hotkey
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CTRL, VK_F9);
            this.FormClosing += MainForm_FormClosing;
        }

        private System.Threading.CancellationTokenSource? _runCts;

        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            _runCts?.Cancel();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                if (_btnRun.Enabled)
                {
                    BtnRun_Click(this, EventArgs.Empty);
                }
                else
                {
                    // If it is running, we cancel it.
                    _runCts?.Cancel();
                }
            }
        }

        private void InitializeComponent()
        {
            this.Text = "AuraMacro Dashboard";
            this.Size = new System.Drawing.Size(600, 500);

            _lstTimeline = new ListBox
            {
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(300, 400),
                DisplayMember = "DisplayText"
            };

            _btnAddAction = new Button { Text = "Add Action", Location = new System.Drawing.Point(340, 20), Size = new System.Drawing.Size(100, 30) };
            _btnAddAction.Click += BtnAddAction_Click;

            _btnDeleteAction = new Button { Text = "Delete", Location = new System.Drawing.Point(340, 60), Size = new System.Drawing.Size(100, 30) };
            _btnDeleteAction.Click += BtnDeleteAction_Click;

            _btnMoveUp = new Button { Text = "Move Up", Location = new System.Drawing.Point(340, 100), Size = new System.Drawing.Size(100, 30) };
            _btnMoveUp.Click += BtnMoveUp_Click;

            _btnMoveDown = new Button { Text = "Move Down", Location = new System.Drawing.Point(340, 140), Size = new System.Drawing.Size(100, 30) };
            _btnMoveDown.Click += BtnMoveDown_Click;

            _btnRun = new Button { Text = "Run Macro", Location = new System.Drawing.Point(340, 200), Size = new System.Drawing.Size(100, 40) };
            _btnRun.Click += BtnRun_Click;

            _btnSave = new Button { Text = "Save Macro", Location = new System.Drawing.Point(340, 260), Size = new System.Drawing.Size(100, 30) };
            _btnSave.Click += BtnSave_Click;

            _btnLoad = new Button { Text = "Load Macro", Location = new System.Drawing.Point(340, 300), Size = new System.Drawing.Size(100, 30) };
            _btnLoad.Click += BtnLoad_Click;

            _btnSettings = new Button { Text = "Settings", Location = new System.Drawing.Point(340, 340), Size = new System.Drawing.Size(100, 30) };
            _btnSettings.Click += BtnSettings_Click;

            _lblStatus = new Label
            {
                Text = "Ready",
                Location = new System.Drawing.Point(20, 430),
                Size = new System.Drawing.Size(500, 20)
            };

            this.Controls.Add(_lstTimeline);
            this.Controls.Add(_btnAddAction);
            this.Controls.Add(_btnDeleteAction);
            this.Controls.Add(_btnMoveUp);
            this.Controls.Add(_btnMoveDown);
            this.Controls.Add(_btnRun);
            this.Controls.Add(_btnSave);
            this.Controls.Add(_btnLoad);
            this.Controls.Add(_btnSettings);
            this.Controls.Add(_lblStatus);
        }

        private void RefreshTimeline()
        {
            _lstTimeline.Items.Clear();
            foreach (var action in _currentWorkflow.Actions)
            {
                string desc = action.GetType().Name.Replace("Action", "");
                if (action is ClickAction c) desc += $" ({c.X}, {c.Y})";
                else if (action is KeyPressAction k) desc += $" ({k.Key})";
                else if (action is WaitAction w) desc += $" ({w.Milliseconds}ms)";

                _lstTimeline.Items.Add(new ActionListItem { Action = action, DisplayText = desc });
            }
        }

        private void BtnAddAction_Click(object? sender, EventArgs e)
        {
            // Prompt the user to decide what action to add
            var result = MessageBox.Show(
                "Yes = Wait for Text (Ghost Overlay)\nNo = Wait Action (1s)\nCancel = Click Action",
                "Add Action",
                MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Yes)
            {
                using (var ghost = new GhostForm())
                {
                    if (ghost.ShowDialog() == DialogResult.OK)
                    {
                        var rect = ghost.SelectedRegion;
                        var waitText = new WaitForTextAction
                        {
                            TextToFind = "Enter text",
                            RegionX = rect.X,
                            RegionY = rect.Y,
                            RegionWidth = rect.Width,
                            RegionHeight = rect.Height
                        };
                        _currentWorkflow.Actions.Add(waitText);
                    }
                }
            }
            else if (result == DialogResult.No)
            {
                _currentWorkflow.Actions.Add(new WaitAction { Milliseconds = 1000 });
            }
            else if (result == DialogResult.Cancel)
            {
                _currentWorkflow.Actions.Add(new ClickAction { X = 100, Y = 100 });
            }

            RefreshTimeline();
        }

        private void BtnDeleteAction_Click(object? sender, EventArgs e)
        {
            if (_lstTimeline.SelectedIndex >= 0)
            {
                _currentWorkflow.Actions.RemoveAt(_lstTimeline.SelectedIndex);
                RefreshTimeline();
            }
        }

        private void BtnMoveUp_Click(object? sender, EventArgs e)
        {
            int index = _lstTimeline.SelectedIndex;
            if (index > 0)
            {
                var action = _currentWorkflow.Actions[index];
                _currentWorkflow.Actions.RemoveAt(index);
                _currentWorkflow.Actions.Insert(index - 1, action);
                RefreshTimeline();
                _lstTimeline.SelectedIndex = index - 1;
            }
        }

        private void BtnMoveDown_Click(object? sender, EventArgs e)
        {
            int index = _lstTimeline.SelectedIndex;
            if (index >= 0 && index < _currentWorkflow.Actions.Count - 1)
            {
                var action = _currentWorkflow.Actions[index];
                _currentWorkflow.Actions.RemoveAt(index);
                _currentWorkflow.Actions.Insert(index + 1, action);
                RefreshTimeline();
                _lstTimeline.SelectedIndex = index + 1;
            }
        }

        private async void BtnRun_Click(object? sender, EventArgs e)
        {
            _lblStatus.Text = "Running... (Press Ctrl+F9 to Stop)";
            _btnRun.Enabled = false;

            _runCts = new System.Threading.CancellationTokenSource();

            var inputSimulator = new MockInputSimulator();
            var ocrEngine = new MockOcrEngine { MockedResult = "TEST_TEXT" };
            var runner = new MacroRunner(inputSimulator, ocrEngine);

            // Wire up the alert action to the UI
            runner.ShowAlertAction = (message, title) =>
            {
                // Ensure thread safety since this is called from an async task
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                }
                else
                {
                    MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            try
            {
                await runner.RunAsync(_currentWorkflow, _runCts.Token);
                if (_runCts.Token.IsCancellationRequested)
                {
                    _lblStatus.Text = "Macro cancelled.";
                }
                else
                {
                    _lblStatus.Text = "Finished running.";
                }
            }
            catch (Exception ex)
            {
                _lblStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                _btnRun.Enabled = true;
                _runCts.Dispose();
                _runCts = null;
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog { Filter = "AuraMacro File (*.aura)|*.aura|JSON File (*.json)|*.json" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string json = WorkflowSerializer.Serialize(_currentWorkflow);
                    System.IO.File.WriteAllText(sfd.FileName, json);
                    _lblStatus.Text = $"Saved successfully to {System.IO.Path.GetFileName(sfd.FileName)}.";
                }
            }
        }

        private void BtnLoad_Click(object? sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "AuraMacro File (*.aura)|*.aura|JSON File (*.json)|*.json" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string json = System.IO.File.ReadAllText(ofd.FileName);
                    var loaded = WorkflowSerializer.Deserialize(json);
                    if (loaded != null)
                    {
                        _currentWorkflow = loaded;
                        RefreshTimeline();
                        _lblStatus.Text = $"Loaded successfully from {System.IO.Path.GetFileName(ofd.FileName)}.";
                    }
                }
            }
        }

        private void BtnSettings_Click(object? sender, EventArgs e)
        {
            using (var sf = new SettingsForm(_currentWorkflow))
            {
                sf.ShowDialog();
            }
        }

        private class ActionListItem
        {
            public MacroAction Action { get; set; } = null!;
            public string DisplayText { get; set; } = "";
        }
    }
}