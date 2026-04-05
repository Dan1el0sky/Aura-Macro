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