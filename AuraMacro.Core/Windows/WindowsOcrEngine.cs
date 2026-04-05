using System;
using System.Threading.Tasks;
using AuraMacro.Core.Interfaces;

namespace AuraMacro.Core.Windows
{
    // Needs Microsoft.Windows.SDK.NET to use Windows.Media.Ocr
    // We will provide a stub implementation that throws if not on Windows, or works if SDK is loaded.
    public class WindowsOcrEngine : IOcrEngine
    {
        public Task<string> RecognizeTextAsync(int x, int y, int width, int height)
        {
            // Implementation requires capturing the screen region and passing it to Windows.Media.Ocr.OcrEngine
            Console.WriteLine($"[WindowsOcrEngine] RecognizeTextAsync called for region {x},{y} {width}x{height}");
            return Task.FromResult("MOCK_TEXT"); // Mocked for now to allow compiling without full SDK interop setup
        }

        public Task<string> RecognizeTextFromFileAsync(string imagePath)
        {
            Console.WriteLine($"[WindowsOcrEngine] RecognizeTextFromFileAsync called for {imagePath}");
            return Task.FromResult("MOCK_TEXT");
        }
    }
}