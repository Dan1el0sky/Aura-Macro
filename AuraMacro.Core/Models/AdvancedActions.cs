namespace AuraMacro.Core.Models
{
    public class WaitForImageAction : MacroAction
    {
        public string ImagePath { get; set; } = string.Empty;
        public int TimeoutMilliseconds { get; set; } = 5000;
        public string? OnSuccessGotoId { get; set; }
        public string? OnFailureGotoId { get; set; }
    }

    public class WaitForTextAction : MacroAction
    {
        public string TextToFind { get; set; } = string.Empty;
        public int TimeoutMilliseconds { get; set; } = 5000;
        public int RegionX { get; set; }
        public int RegionY { get; set; }
        public int RegionWidth { get; set; }
        public int RegionHeight { get; set; }
        public string? SaveToVariable { get; set; }
    }

    public class IfElseAction : MacroAction
    {
        public string VariableName { get; set; } = string.Empty;
        public string ExpectedValue { get; set; } = string.Empty;
        public string? OnTrueGotoId { get; set; }
        public string? OnFalseGotoId { get; set; }
    }
}