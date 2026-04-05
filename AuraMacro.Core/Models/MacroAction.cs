using System.Text.Json.Serialization;

namespace AuraMacro.Core.Models
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
    [JsonDerivedType(typeof(ClickAction), "click")]
    [JsonDerivedType(typeof(KeyPressAction), "keypress")]
    [JsonDerivedType(typeof(WaitAction), "wait")]
    [JsonDerivedType(typeof(WaitForImageAction), "wait_for_image")]
    [JsonDerivedType(typeof(WaitForTextAction), "wait_for_text")]
    [JsonDerivedType(typeof(IfElseAction), "ifelse")]
    [JsonDerivedType(typeof(SaveVariableAction), "save_variable")]
    [JsonDerivedType(typeof(MathCalculationAction), "math_calculation")]
    [JsonDerivedType(typeof(WaitUntilTimeAction), "wait_until_time")]
    [JsonDerivedType(typeof(ExecuteScriptAction), "execute_script")]
    [JsonDerivedType(typeof(MessageBoxAction), "message_box")]
    [JsonDerivedType(typeof(WebScraperAction), "web_scraper")]
    [JsonDerivedType(typeof(ExecuteProgramAction), "execute_program")]
    [JsonDerivedType(typeof(WaitForFileChangeAction), "wait_for_file_change")]
    [JsonDerivedType(typeof(LlmPromptAction), "llm_prompt")]
    [JsonDerivedType(typeof(AiFindObjectAction), "ai_find_object")]
    public abstract class MacroAction
    {
        public string Id { get; set; } = System.Guid.NewGuid().ToString();
    }

    public class ClickAction : MacroAction
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class KeyPressAction : MacroAction
    {
        public string Key { get; set; } = string.Empty;
    }

    public class WaitAction : MacroAction
    {
        public int Milliseconds { get; set; }
    }
}