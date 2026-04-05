using System;
using System.IO;
using System.Threading.Tasks;
using AuraMacro.Core.Engine;
using AuraMacro.Core.Mocks;
using AuraMacro.Core.Models;
using AuraMacro.Core.Workflow;

using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace AuraMacro.ConsoleUI
{
    class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static NotifyIcon? trayIcon;
        private static bool isMinimized = false;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== AuraMacro Basic Testing UI ===");

            // Setup Console Close Hook to prompt for Tray
            AppDomain.CurrentDomain.ProcessExit += (s, e) => {
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Dispose();
                }
            };

            var inputSimulator = new MockInputSimulator();
            var ocrEngine = new MockOcrEngine { MockedResult = "TEST_TEXT" };
            var runner = new MacroRunner(inputSimulator, ocrEngine);

            MacroWorkflow currentWorkflow = new MacroWorkflow { Name = "My Basic Workflow" };

            while (true)
            {
                Console.WriteLine("\nOptions:");
                Console.WriteLine("1. Create Basic Sample Macro");
                Console.WriteLine("2. Save Macro to JSON");
                Console.WriteLine("3. Load Macro from JSON");
                Console.WriteLine("4. Run Macro");
                Console.WriteLine("5. Minimize to Tray");
                Console.WriteLine("6. Exit");
                Console.Write("Select an option: ");

                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        currentWorkflow = CreateSampleWorkflow();
                        Console.WriteLine("Sample macro created in memory.");
                        break;
                    case "2":
                        string json = WorkflowSerializer.Serialize(currentWorkflow);
                        File.WriteAllText("macro.json", json);
                        Console.WriteLine("Macro saved to macro.json.");
                        break;
                    case "3":
                        if (File.Exists("macro.json"))
                        {
                            string loadedJson = File.ReadAllText("macro.json");
                            var loaded = WorkflowSerializer.Deserialize(loadedJson);
                            if (loaded != null)
                            {
                                currentWorkflow = loaded;
                                Console.WriteLine($"Macro '{currentWorkflow.Name}' loaded from macro.json.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("macro.json does not exist.");
                        }
                        break;
                    case "4":
                        Console.WriteLine($"Running macro '{currentWorkflow.Name}'...");
                        await runner.RunAsync(currentWorkflow);
                        Console.WriteLine("Macro finished.");
                        break;
                    case "5":
                        MinimizeToTray();
                        break;
                    case "6":
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        static void MinimizeToTray()
        {
            if (isMinimized) return;

            // Run in STA thread for WinForms compatibility
            Thread staThread = new Thread(() =>
            {
                var result = MessageBox.Show(
                    "Do you want to minimize to the system tray to keep scheduled actions running using minimal resources?",
                    "Minimize to Tray",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    isMinimized = true;

                    // Hide Console
                    var handle = GetConsoleWindow();
                    ShowWindow(handle, SW_HIDE);

                    // Setup Tray Icon
                    trayIcon = new NotifyIcon();
                    trayIcon.Icon = SystemIcons.Application; // Default icon
                    trayIcon.Text = "AuraMacro Background Engine";
                    trayIcon.Visible = true;

                    ContextMenuStrip menu = new ContextMenuStrip();
                    var restoreItem = new ToolStripMenuItem("Restore");
                    restoreItem.Click += (s, e) => {
                        ShowWindow(handle, SW_SHOW);
                        trayIcon.Visible = false;
                        trayIcon.Dispose();
                        isMinimized = false;
                        Application.ExitThread(); // Fixes the deadlock
                    };

                    var exitItem = new ToolStripMenuItem("Exit");
                    exitItem.Click += (s, e) => {
                        trayIcon.Visible = false;
                        trayIcon.Dispose();
                        Environment.Exit(0);
                    };

                    menu.Items.Add(restoreItem);
                    menu.Items.Add(exitItem);
                    trayIcon.ContextMenuStrip = menu;

                    // Run a message loop so the tray icon works
                    Application.Run();
                }
                else
                {
                    Console.WriteLine("Minimization cancelled.");
                }
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
        }

        static MacroWorkflow CreateSampleWorkflow()
        {
            var wf = new MacroWorkflow { Name = "Test Workflow with Brain" };

            var click = new ClickAction { X = 100, Y = 200, Id = "step1" };
            var wait = new WaitAction { Milliseconds = 1000, Id = "step2" };
            var waitText = new WaitForTextAction
            {
                Id = "step3",
                TextToFind = "TEST",
                RegionX = 0, RegionY = 0, RegionWidth = 500, RegionHeight = 500,
                SaveToVariable = "found_text"
            };
            var ifElse = new IfElseAction
            {
                Id = "step4",
                VariableName = "found_text",
                ExpectedValue = "TEST_TEXT", // This is what MockOcrEngine returns
                OnTrueGotoId = "step_true",
                OnFalseGotoId = "step_false"
            };

            var keyTrue = new KeyPressAction { Id = "step_true", Key = "T" };
            var keyFalse = new KeyPressAction { Id = "step_false", Key = "F" };

            wf.Actions.Add(click);
            wf.Actions.Add(wait);
            wf.Actions.Add(waitText);
            wf.Actions.Add(ifElse);
            wf.Actions.Add(keyTrue); // Will jump here
            wf.Actions.Add(keyFalse);

            return wf;
        }
    }
}