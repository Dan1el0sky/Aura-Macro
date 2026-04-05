using System;
using OpenCvSharp;

namespace AuraMacro.Core.Engine
{
    public class ImageMatcher
    {
        public bool FindImageOnScreen(string templateImagePath, out int x, out int y)
        {
            x = -1;
            y = -1;

            // In a real application, we would capture the screen here.
            // For now, we simulate capturing the screen or expect a mock.
            // For testing the logic, we will assume we have a way to pass a Mat or we mock it.
            // But since this is core logic, we write the OpenCV logic as if we had screen images.

            // Due to environment restrictions, we'll make this method virtual or create an interface
            // if we need to mock it, but OpenCvSharp can be run in tests if we provide dummy images.

            throw new NotImplementedException("Screen capturing for OpenCv needs implementation or to be mocked.");
        }

        public bool FindImage(string sourceImagePath, string templateImagePath, out int x, out int y, double threshold = 0.8)
        {
            x = -1;
            y = -1;

            try
            {
                using var sourceMat = Cv2.ImRead(sourceImagePath, ImreadModes.Color);
                using var templateMat = Cv2.ImRead(templateImagePath, ImreadModes.Color);

                if (sourceMat.Empty() || templateMat.Empty()) return false;

                using var result = new Mat();
                Cv2.MatchTemplate(sourceMat, templateMat, result, TemplateMatchModes.CCoeffNormed);

                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out Point maxLoc);

                if (maxVal >= threshold)
                {
                    x = maxLoc.X + (templateMat.Width / 2); // Return center of match
                    y = maxLoc.Y + (templateMat.Height / 2);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ImageMatcher] Error finding image: {ex.Message}");
            }

            return false;
        }
    }
}