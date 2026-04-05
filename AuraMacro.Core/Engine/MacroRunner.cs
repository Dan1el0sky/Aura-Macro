using System;
using System.Collections.Generic;
using System.Linq;
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

                    case WaitForImageAction waitImg:
                        Console.WriteLine($"[Runner] Waiting for image: {waitImg.ImagePath}");
                        bool imgFound = _imageMatcher.FindImage("dummy_screen.png", waitImg.ImagePath, out int x, out int y);
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
                        Console.WriteLine($"[Runner] Waiting for text: {waitText.TextToFind}");
                        string text = await _ocrEngine.RecognizeTextAsync(waitText.RegionX, waitText.RegionY, waitText.RegionWidth, waitText.RegionHeight);
                        if (text.Contains(waitText.TextToFind))
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
                        bool isTrue = _variableStore.EvaluateCondition(ifElse.VariableName, ifElse.ExpectedValue);
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