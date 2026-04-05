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

        public string SubstituteVariables(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            string result = input;
            foreach (var kvp in _variables)
            {
                result = result.Replace($"{{{kvp.Key}}}", kvp.Value);
            }
            return result;
        }

        public void SaveToFile(string variableName, string filePath, bool append)
        {
            var value = GetVariable(variableName);
            if (append)
            {
                System.IO.File.AppendAllText(filePath, value + System.Environment.NewLine);
            }
            else
            {
                System.IO.File.WriteAllText(filePath, value);
            }
        }

        public void PerformMathCalculation(string expression, string targetVariableName)
        {
            // Simple mock math calculation for now, since a full expression evaluator
            // (like NCalc or DataTable.Compute) is slightly complex for this snippet.
            // A basic implementation using DataTable.Compute:

            try
            {
                string substitutedExpression = SubstituteVariables(expression);
                var dataTable = new System.Data.DataTable();
                var result = dataTable.Compute(substitutedExpression, string.Empty);
                SetVariable(targetVariableName, result.ToString() ?? string.Empty);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[VariableStore] Math error: {ex.Message}");
                SetVariable(targetVariableName, "ERROR");
            }
        }
    }
}