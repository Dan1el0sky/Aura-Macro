using System;
using System.Collections.Generic;
using AuraMacro.Core.Interfaces;
using AuraMacro.Core.Models;

namespace AuraMacro.Core.Engine
{
    public class ActionRecorder
    {
        private readonly IInputHook _inputHook;
        private readonly List<MacroAction> _recordedActions = new();

        public ActionRecorder(IInputHook inputHook)
        {
            _inputHook = inputHook;
            _inputHook.OnActionRecorded += OnActionRecorded;
        }

        private void OnActionRecorded(object? sender, MacroAction action)
        {
            _recordedActions.Add(action);
            Console.WriteLine($"[ActionRecorder] Recorded: {action.GetType().Name}");
        }

        public void Start()
        {
            _recordedActions.Clear();
            _inputHook.StartRecording();
        }

        public List<MacroAction> Stop()
        {
            _inputHook.StopRecording();
            return new List<MacroAction>(_recordedActions);
        }
    }
}