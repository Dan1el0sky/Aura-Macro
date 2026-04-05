using System.Collections.Generic;
using AuraMacro.Core.Models;

namespace AuraMacro.Core.Workflow
{
    public class MacroWorkflow
    {
        public string Name { get; set; } = "Untitled Workflow";
        public List<MacroAction> Actions { get; set; } = new();
    }
}