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
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}