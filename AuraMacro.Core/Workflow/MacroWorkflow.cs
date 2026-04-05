using System.Collections.Generic;
using AuraMacro.Core.Models;

namespace AuraMacro.Core.Workflow
{
    public class MacroWorkflow
    {
        public string Name { get; set; } = "Untitled Workflow";
        public FailureBehavior GlobalOnFailureBehavior { get; set; } = FailureBehavior.Stop;
        public string FailureAlertMessage { get; set; } = "A macro step failed.";
        public int OcrCooldownMilliseconds { get; set; } = 500;
        public List<MacroAction> Actions { get; set; } = new();
    }

    public enum FailureBehavior
    {
        Stop,
        Continue,
        AlertAndStop
    }
}