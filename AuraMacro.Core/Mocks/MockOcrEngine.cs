using System;
using System.Threading.Tasks;
using AuraMacro.Core.Interfaces;

namespace AuraMacro.Core.Mocks
{
    public class MockOcrEngine : IOcrEngine
    {
        public string MockedResult { get; set; } = string.Empty;

        public Task<string> RecognizeTextAsync(int x, int y, int width, int height)
        {
            Console.WriteLine($"[MockOcrEngine] Recognized text in region ({x},{y},{width},{height}): {MockedResult}");
            return Task.FromResult(MockedResult);
        }

        public Task<string> RecognizeTextFromFileAsync(string imagePath)
        {
            Console.WriteLine($"[MockOcrEngine] Recognized text from {imagePath}: {MockedResult}");
            return Task.FromResult(MockedResult);
        }
    }
}