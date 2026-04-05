namespace AuraMacro.Core.Interfaces
{
    public interface IInputSimulator
    {
        void SendClick(int x, int y);
        void SendKeyPress(string key);
    }

    public interface IInputHook
    {
        void StartRecording();
        void StopRecording();
        event System.EventHandler<Models.MacroAction>? OnActionRecorded;
    }
}