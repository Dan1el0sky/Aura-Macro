using System.Text.Json;
using AuraMacro.Core.Models;
using AuraMacro.Core.Workflow;

namespace AuraMacro.Core.Engine
{
    public static class WorkflowSerializer
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string Serialize(MacroWorkflow workflow)
        {
            return JsonSerializer.Serialize(workflow, _options);
        }

        public static MacroWorkflow? Deserialize(string json)
        {
            return JsonSerializer.Deserialize<MacroWorkflow>(json, _options);
        }
    }
}