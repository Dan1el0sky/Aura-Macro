using System.Collections.Generic;

namespace AuraMacro.Core.Engine
{
    public class VariableStore
    {
        private readonly Dictionary<string, string> _variables = new();

        public void SetVariable(string name, string value)
        {
            _variables[name] = value;
        }

        public string GetVariable(string name)
        {
            return _variables.TryGetValue(name, out var val) ? val : string.Empty;
        }

        public bool EvaluateCondition(string variableName, string expectedValue)
        {
            var actualValue = GetVariable(variableName);
            return actualValue == expectedValue;
        }
    }
}