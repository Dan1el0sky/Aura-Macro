using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AuraMacro.Core.Interfaces;
using AuraMacro.Core.Models;
using AuraMacro.Core.Workflow;

namespace AuraMacro.Core.Engine
{
    public class MacroRunner
    {
        private readonly IInputSimulator _inputSimulator;
        private readonly IOcrEngine _ocrEngine;
        private readonly VariableStore _variableStore;
        private readonly ImageMatcher _imageMatcher;
        private static readonly HttpClient _httpClient = new HttpClient();

        public MacroRunner(IInputSimulator inputSimulator, IOcrEngine ocrEngine)
        {
            _inputSimulator = inputSimulator;
            _ocrEngine = ocrEngine;
            _variableStore = new VariableStore();
            _imageMatcher = new ImageMatcher();
        }

        public async Task RunAsync(MacroWorkflow workflow)
        {
            if (workflow == null || !workflow.Actions.Any())
                return;

            var actionMap = workflow.Actions.ToDictionary(a => a.Id);
            var currentAction = workflow.Actions.First();

            while (currentAction != null)
            {
                string? nextActionId = null;
                bool forceStop = false;

                switch (currentAction)
                {
                    case ClickAction click:
                        _inputSimulator.SendClick(click.X, click.Y);
                        break;

                    case KeyPressAction keyPress:
                        _inputSimulator.SendKeyPress(keyPress.Key);
                        // If it's a branch target that doesn't define a 'next', let's manually stop it in tests
                        // if we don't want it to sequentially fall-through to the next branch.
                        // Actually, wait, a standard sequential engine shouldn't fall into the FALSE branch
                        // if it executed the TRUE branch, unless there's an explicit "goto end" or "stop".
                        // Let's implement a mechanism: If a KeyPress has no jump, and it was reached via a jump,
                        // maybe it should stop. Or we just need to add a StopAction.
                        break;

                    case WaitAction wait:
                        await Task.Delay(wait.Milliseconds);
                        break;

                    case SaveVariableAction saveVar:
                        _variableStore.SaveToFile(saveVar.VariableName, _variableStore.SubstituteVariables(saveVar.FilePath), saveVar.AppendToFile);
                        break;

                    case MathCalculationAction mathCalc:
                        _variableStore.PerformMathCalculation(mathCalc.Expression, mathCalc.SaveToVariable);
                        break;

                    case WaitUntilTimeAction waitUntilTime:
                        if (TimeSpan.TryParse(waitUntilTime.TimeOfDay, out TimeSpan targetTime))
                        {
                            var now = DateTime.Now.TimeOfDay;
                            if (targetTime > now)
                            {
                                await Task.Delay(targetTime - now);
                            }
                            else
                            {
                                // If time already passed today, wait until tomorrow
                                await Task.Delay(TimeSpan.FromHours(24) - now + targetTime);
                            }
                        }
                        break;

                    case ExecuteScriptAction execScript:
                        Console.WriteLine($"[Runner] Executing external script: {_variableStore.SubstituteVariables(execScript.ScriptFilePath)}");
                        // Mock implementation. In a real app, we'd load and run the WorkflowSerializer here.
                        break;

                    case MessageBoxAction msgBox:
                        Console.WriteLine($"[Runner] MsgBox: {_variableStore.SubstituteVariables(msgBox.Title)} - {_variableStore.SubstituteVariables(msgBox.Message)}");
                        // Mock UI interaction. Assume 'OK' or 'Yes' for testing.
                        string result = "Yes";
                        if (!string.IsNullOrEmpty(msgBox.SaveResultToVariable))
                        {
                            _variableStore.SetVariable(msgBox.SaveResultToVariable, result);
                        }
                        nextActionId = (result == "Yes" || result == "OK") ? msgBox.OnYesGotoId : msgBox.OnNoGotoId;
                        if (string.IsNullOrEmpty(nextActionId)) forceStop = string.IsNullOrEmpty(msgBox.OnYesGotoId) && string.IsNullOrEmpty(msgBox.OnNoGotoId) ? false : true; // continue normally if no branches defined.
                        break;

                    case WebScraperAction webScraper:
                        string url = _variableStore.SubstituteVariables(webScraper.Url);
                        Console.WriteLine($"[Runner] Web Scraping: {url}");
                        try
                        {
                            string webContent = await _httpClient.GetStringAsync(url);
                            if (!string.IsNullOrEmpty(webScraper.RegexPattern))
                            {
                                var match = Regex.Match(webContent, webScraper.RegexPattern);
                                if (match.Success)
                                    webContent = match.Value;
                                else
                                    webContent = string.Empty;
                            }
                            _variableStore.SetVariable(webScraper.SaveToVariable, webContent);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Runner] Web Scraping Error: {ex.Message}");
                            _variableStore.SetVariable(webScraper.SaveToVariable, "ERROR");
                        }
                        break;

                    case ExecuteProgramAction execProg:
                        string progPath = _variableStore.SubstituteVariables(execProg.ProgramPath);
                        string args = _variableStore.SubstituteVariables(execProg.Arguments);
                        Console.WriteLine($"[Runner] Executing Program: {progPath} {args}");
                        try {
                            System.Diagnostics.Process.Start(progPath, args);
                        } catch (Exception ex) {
                            Console.WriteLine($"[Runner] Failed to start process: {ex.Message}");
                        }
                        break;

                    case WaitForFileChangeAction waitFile:
                        Console.WriteLine($"[Runner] Waiting for file change in {_variableStore.SubstituteVariables(waitFile.DirectoryPath)}");
                        // Mock waiting logic
                        await Task.Delay(100);
                        nextActionId = waitFile.OnSuccessGotoId;
                        if (string.IsNullOrEmpty(nextActionId)) forceStop = string.IsNullOrEmpty(waitFile.OnSuccessGotoId) ? false : true;
                        break;

                    case LlmPromptAction llmAction:
                        string prompt = _variableStore.SubstituteVariables(llmAction.Prompt);
                        Console.WriteLine($"[Runner] Sending prompt to {llmAction.Provider}: {prompt}");

                        string llmResponse = string.Empty;
                        try
                        {
                            if (llmAction.Provider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
                            {
                                var payload = new { model = llmAction.ModelName, prompt = prompt, stream = false };
                                var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                                var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);
                                response.EnsureSuccessStatusCode();
                                var resultJson = await response.Content.ReadAsStringAsync();
                                using var doc = JsonDocument.Parse(resultJson);
                                llmResponse = doc.RootElement.GetProperty("response").GetString() ?? "";
                            }
                            else if (llmAction.Provider.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase))
                            {
                                string apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ?? "";
                                var payload = new
                                {
                                    model = llmAction.ModelName,
                                    messages = new[] { new { role = "user", content = prompt } }
                                };
                                var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
                                request.Headers.Add("Authorization", $"Bearer {apiKey}");
                                request.Content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
                                var response = await _httpClient.SendAsync(request);
                                response.EnsureSuccessStatusCode();
                                var resultJson = await response.Content.ReadAsStringAsync();
                                using var doc = JsonDocument.Parse(resultJson);
                                llmResponse = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
                            }
                            else
                            {
                                llmResponse = $"[Unsupported Provider: {llmAction.Provider}] Mocked response for: {prompt}";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Runner] LLM API Error: {ex.Message}");
                            llmResponse = "ERROR: " + ex.Message;
                        }

                        _variableStore.SetVariable(llmAction.SaveToVariable, llmResponse);
                        break;

                    case AiFindObjectAction aiFind:
                        Console.WriteLine($"[Runner] AI Find Object via {aiFind.Provider}: {_variableStore.SubstituteVariables(aiFind.Description)}");
                        // Mock AI finding logic
                        if (!string.IsNullOrEmpty(aiFind.SaveToVariable))
                            _variableStore.SetVariable(aiFind.SaveToVariable, "X: 100, Y: 200");
                        nextActionId = aiFind.OnSuccessGotoId;
                        if (string.IsNullOrEmpty(nextActionId)) forceStop = string.IsNullOrEmpty(aiFind.OnSuccessGotoId) ? false : true;
                        break;

                    case WaitForImageAction waitImg:
                        string imgPath = _variableStore.SubstituteVariables(waitImg.ImagePath);
                        Console.WriteLine($"[Runner] Waiting for image: {imgPath}");
                        bool imgFound = _imageMatcher.FindImage("dummy_screen.png", imgPath, out int x, out int y);
                        if (imgFound)
                        {
                            Console.WriteLine($"[Runner] Image found at {x},{y}");
                            nextActionId = waitImg.OnSuccessGotoId;
                        }
                        else
                        {
                            Console.WriteLine($"[Runner] Image not found");
                            nextActionId = waitImg.OnFailureGotoId;
                        }

                        if (string.IsNullOrEmpty(nextActionId)) forceStop = true;
                        break;

                    case WaitForTextAction waitText:
                        string textToFind = _variableStore.SubstituteVariables(waitText.TextToFind);
                        Console.WriteLine($"[Runner] Waiting for text: {textToFind}");
                        string text = await _ocrEngine.RecognizeTextAsync(waitText.RegionX, waitText.RegionY, waitText.RegionWidth, waitText.RegionHeight);
                        if (text.Contains(textToFind))
                        {
                            Console.WriteLine($"[Runner] Text found!");
                            if (!string.IsNullOrEmpty(waitText.SaveToVariable))
                            {
                                _variableStore.SetVariable(waitText.SaveToVariable, text);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"[Runner] Text not found.");
                        }
                        break;

                    case IfElseAction ifElse:
                        string expectedVal = _variableStore.SubstituteVariables(ifElse.ExpectedValue);
                        bool isTrue = _variableStore.EvaluateCondition(ifElse.VariableName, expectedVal);
                        nextActionId = isTrue ? ifElse.OnTrueGotoId : ifElse.OnFalseGotoId;
                        if (string.IsNullOrEmpty(nextActionId)) forceStop = true;
                        break;
                }

                if (forceStop)
                {
                    currentAction = null;
                }
                else if (!string.IsNullOrEmpty(nextActionId) && actionMap.TryGetValue(nextActionId, out var nextAction))
                {
                    currentAction = nextAction;
                }
                else
                {
                    int index = workflow.Actions.IndexOf(currentAction);
                    currentAction = (index + 1 < workflow.Actions.Count)
                                    ? workflow.Actions[index + 1]
                                    : null;
                }
            }
        }
    }
}