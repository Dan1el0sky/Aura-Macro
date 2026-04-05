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

    public class SaveVariableAction : MacroAction
    {
        public string VariableName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public bool AppendToFile { get; set; } = true;
    }

    public class MathCalculationAction : MacroAction
    {
        public string Expression { get; set; } = string.Empty;
        public string SaveToVariable { get; set; } = string.Empty;
    }

    public class WaitUntilTimeAction : MacroAction
    {
        public string TimeOfDay { get; set; } = string.Empty; // Format e.g., "HH:mm:ss"
    }

    public class ExecuteScriptAction : MacroAction
    {
        public string ScriptFilePath { get; set; } = string.Empty;
    }

    public class MessageBoxAction : MacroAction
    {
        public string Message { get; set; } = string.Empty;
        public string Title { get; set; } = "Notification";
        public string Buttons { get; set; } = "OK"; // E.g., "YesNo", "OKCancel"
        public string? SaveResultToVariable { get; set; }
        public string? OnYesGotoId { get; set; }
        public string? OnNoGotoId { get; set; }
    }

    public class WebScraperAction : MacroAction
    {
        public string Url { get; set; } = string.Empty;
        public string RegexPattern { get; set; } = string.Empty;
        public string XPathSelector { get; set; } = string.Empty;
        public string SaveToVariable { get; set; } = string.Empty;
    }

    public class ExecuteProgramAction : MacroAction
    {
        public string ProgramPath { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
    }

    public class WaitForFileChangeAction : MacroAction
    {
        public string DirectoryPath { get; set; } = string.Empty;
        public string Filter { get; set; } = "*.*";
        public string ChangeType { get; set; } = "Created"; // E.g., "Created", "Deleted", "Modified"
        public int TimeoutMilliseconds { get; set; } = 60000;
        public string? OnSuccessGotoId { get; set; }
        public string? OnTimeoutGotoId { get; set; }
    }

    public class LlmPromptAction : MacroAction
    {
        public string Provider { get; set; } = "OpenRouter"; // OpenRouter, Gemini, Ollama
        public string ModelName { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string SaveToVariable { get; set; } = string.Empty;
    }

    public class AiFindObjectAction : MacroAction
    {
        public string Description { get; set; } = string.Empty;
        public string Provider { get; set; } = "Gemini"; // Or Ollama
        public string ModelName { get; set; } = string.Empty;
        public string? SaveToVariable { get; set; }
        public string? OnSuccessGotoId { get; set; }
        public string? OnFailureGotoId { get; set; }
    }
}