using System.Threading.Tasks;

namespace AuraMacro.Core.Interfaces
{
    public interface IOcrEngine
    {
        Task<string> RecognizeTextAsync(int x, int y, int width, int height);
        Task<string> RecognizeTextFromFileAsync(string imagePath);
    }
}