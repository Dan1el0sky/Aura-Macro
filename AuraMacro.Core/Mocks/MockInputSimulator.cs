using System;
using AuraMacro.Core.Interfaces;

namespace AuraMacro.Core.Mocks
{
    public class MockInputSimulator : IInputSimulator
    {
        public void SendClick(int x, int y)
        {
            Console.WriteLine($"[MOCK] Click simulated at ({x}, {y})");
        }

        public void SendKeyPress(string key)
        {
            Console.WriteLine($"[MOCK] KeyPress simulated: {key}");
        }
    }

    public class MockInputHook : IInputHook
    {
        public event EventHandler<Models.MacroAction>? OnActionRecorded;
        private bool _isRecording;

        public void StartRecording()
        {
            _isRecording = true;
            Console.WriteLine("[MOCK] Recording started...");
        }

        public void StopRecording()
        {
            _isRecording = false;
            Console.WriteLine("[MOCK] Recording stopped.");
        }

        public void SimulateClick(int x, int y)
        {
            if (_isRecording)
            {
                OnActionRecorded?.Invoke(this, new Models.ClickAction { X = x, Y = y });
            }
        }

        public void SimulateKeyPress(string key)
        {
            if (_isRecording)
            {
                OnActionRecorded?.Invoke(this, new Models.KeyPressAction { Key = key });
            }
        }
    }
}